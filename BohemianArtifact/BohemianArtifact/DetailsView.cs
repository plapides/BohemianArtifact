using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.IO;
using System.Text;
using System.Linq;

namespace BohemianArtifact
{
    public class DetailsView : IViewBB
    {
        class BoundingBox
        {
            SelectableLine top, left, right, bottom;

            public BoundingBox(Vector3 topleft, Vector3 bottomright, Color c, float thickness)
            {
                top = new SelectableLine(topleft, new Vector3(topleft.X, bottomright.Y, 0), c, thickness);
                left = new SelectableLine(topleft, new Vector3(bottomright.X, topleft.Y, 0), c, thickness);
                right = new SelectableLine(new Vector3(topleft.X, bottomright.Y, 0), bottomright, c, thickness);
                bottom = new SelectableLine(new Vector3(bottomright.X, topleft.Y, 0), bottomright, c, thickness);
            }

            public void Draw()
            {
                top.DrawThick();
                left.DrawThick();
                right.DrawThick();
                bottom.DrawThick();
            }
        }

        private Vector3 position;
        private Vector3 size;
        public Vector3 Position
        {
            get
            {
                return position;
            }
            set
            {
                position = value;
            }
        }
        public Vector3 Size
        {
            get
            {
                return size;
            }
            set
            {
                size = value;
            }
        }

        private SpriteFont font;
        private SpriteBatch batch;
        private SelectableText titleText;
        private BohemianArtifact bookshelf;

        private SelectableEllipse borderCircle;
        private Vector3 center = new Vector3(0.5f, 0.5f, 0);
        private SelectableQuad maxImageBox; // the space a 512x512 image would fill, lesser images will fill a portion of this box
        private SelectableQuad imageBox;

        private BoundingBox boundingBox;

        private FloatRange rotationRange;

        private string headerText, subheaderText, bodyText;
        private float headerScale, subheaderScale, bodyScale;
        private float headerBigScale; // if header is too big to fit in a line with headerScale, this is an additional compression factor
        private float spacing = 0.02f;
        private float subheaderYOffset;
        private float heightOfBodyLine;
        private Vector2 startOfBody;

        private Color headerColor = new Color(80, 80, 80);
        private Color subheaderColor = Color.DarkRed;
        private Color bodyColor = new Color(40, 40, 100); //new Color(128, 128, 128);


        List<KeyValuePair<Artifact, int>> descLengthsList;
        int currLenIndex = -1;


        public DetailsView(BohemianArtifact bbshelf, Vector3 position, Vector3 size)
        {
            bookshelf = bbshelf;
            bookshelf.Library.SelectedArtifactChanged += new ArtifactLibrary.SelectedArtifactHandler(library_SelectedArtifactChanged);
            bookshelf.Library.LanguageChanged += new ArtifactLibrary.ChangeLanguageHandler(Library_LanguageChanged);
            this.position = position;
            this.size = size;
            font = bookshelf.Content.Load<SpriteFont>("Arial");

            titleText = new SelectableText(font, "Details", new Vector3(0.4f, 0, 0), bookshelf.GlobalTextColor, Color.White);
            titleText.InverseScale(0.8f, size.X, size.Y);

            boundingBox = new BoundingBox(new Vector3(0, 0, 0), new Vector3(1, 1, 0), Color.Black, 0.005f);
            float maxWidth = 0.5f; // as a percentage of the width of the details box

            maxImageBox = new SelectableQuad(new Vector2(0, 0), new Vector2(maxWidth, maxWidth * size.X / size.Y), Color.White);
            rotationRange = new FloatRange(0, 10, 1, (float)(-Math.PI / 24), (float)(Math.PI / 24));

            batch = new SpriteBatch(XNA.GraphicsDevice);
            headerScale = 0.9f;
            subheaderScale = 0.65f;
            bodyScale = 0.5f;

            heightOfBodyLine = font.MeasureString("E").Y * bodyScale;

            setLengths(bbshelf.Library.Artifacts);

            maxImageBox.TouchReleased += new TouchReleaseEventHandler(maxImageBox_TouchReleased);
            bbshelf.SelectableObjects.AddObject(maxImageBox);
        }

        void maxImageBox_TouchReleased(object sender, TouchArgs e)
        {
            currLenIndex++;
            if (currLenIndex >= descLengthsList.Count)
                currLenIndex = 0;

            bookshelf.Library.SelectedArtifact = descLengthsList[currLenIndex].Key;
            Console.WriteLine("selecting index " + currLenIndex + ", length = " + descLengthsList[currLenIndex].Value);
        }

        private void setLengths(List<Artifact> allArtifacts)
        {
            Dictionary<Artifact, int> descLengths = new Dictionary<Artifact, int>();
            foreach (Artifact a in allArtifacts)
            {
                int len = a.Function.Length + a.CanadianSignificance.Length + a.TechSignificance.Length;
                descLengths.Add(a, len);
            }
            descLengths = descLengths.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            descLengthsList = descLengths.ToList();
        }

        private void setImageLocation(Artifact selectedArtifact)
        {
            int w = selectedArtifact.Texture.Width, h = selectedArtifact.Texture.Height;
            if (w > h)
                imageBox = new SelectableQuad(new Vector2(0, 0), new Vector2(w / 512f * maxImageBox.Width, h / 512f * maxImageBox.Height), Color.White);
            else
                imageBox = new SelectableQuad(new Vector2(0, 0), new Vector2(w / 512f * maxImageBox.Width, h / 512f * maxImageBox.Height), Color.White);
        }

        public string WrapText(SpriteFont spriteFont, string text, float maxLineWidth, float fontScale)
        {
            return WrapText(spriteFont, text, maxLineWidth, fontScale, 0);
        }

        // returns a string containing manual line breaks whenever a word exceeds max line width
        // numIndentSpaces of spaces will be inserted at the beginning of each word-wrapped line
        // can still force a line break with \n in your string (will not get indented)
        public string WrapText(SpriteFont spriteFont, string text, float maxLineWidth, float fontScale, int numIndentSpaces)
        {
            string[] words = text.Split(new char[] { ' ' });
            StringBuilder sb = new StringBuilder();
            float lineWidth = 0f;

            float spaceWidth = spriteFont.MeasureString(" ").X * fontScale;

            string currWord;
            foreach (string word in words)
            {
                currWord = word;
                string[] subwords = word.Split(new char[] { '\n' });
                if (subwords.Length > 1)
                    currWord = subwords[0]; // work on the first word

                Vector2 size = spriteFont.MeasureString(currWord) * fontScale;

                if (lineWidth + size.X < maxLineWidth)
                {
                    sb.Append(currWord + " ");
                    lineWidth += size.X + spaceWidth;
                }
                else
                {
                    sb.Append("\n");
                    for (int i = 0; i < numIndentSpaces; i++)
                        sb.Append(" ");
                    sb.Append(currWord + " ");
                    lineWidth = spaceWidth * numIndentSpaces + size.X + spaceWidth;
                }

                if (subwords.Length > 1)
                {
                    size = spriteFont.MeasureString(subwords[subwords.Length - 1]) * fontScale;
                    for (int i = 1; i < subwords.Length; i++)
                        sb.Append("\n" + subwords[i] + " ");
                    lineWidth = size.X + spaceWidth;
                }
            }

            return sb.ToString();
        }

        public void Draw()
        {
            XNA.PushMatrix();
            XNA.Translate(position);
            XNA.Scale(size);

            //titleText.DrawFill();
            //boundingBox.Draw();

            //XNA.PushMatrix();
            //XNA.Translate(imageBox.Center);
            //XNA.RotateZ(rotationRange.Value);
            //XNA.Translate(-imageBox.Center);
            //XNA.Texture = imageBox.Texture;
            //XNA.Texturing = true;
            for (int i = 0; i < 50; i++)
                //XNA.ApplyEffect();
                imageBox.Draw(true);
            //XNA.PopMatrix();

            DrawHeader();
            DrawSubHeader();
            DrawBody();

            XNA.PopMatrix();
        }

        private void DrawHeader()
        {
            batch.Begin();
            batch.DrawString(font, headerText, new Vector2(position.X + size.X * (imageBox.Width + spacing), position.Y),
                headerColor, 0, Vector2.Zero, headerScale * headerBigScale, SpriteEffects.None, 0.5f);
            batch.End();
        }

        private void DrawSubHeader()
        {
            batch.Begin();
            batch.DrawString(font, subheaderText, new Vector2(position.X + size.X * (imageBox.Width + spacing), position.Y + subheaderYOffset),
                subheaderColor, 0, Vector2.Zero, subheaderScale, SpriteEffects.None, 0.5f);
            batch.End();
        }

        private void DrawBody()
        {
            batch.Begin();
            batch.DrawString(font, bodyText, startOfBody,
                bodyColor, 0, Vector2.Zero, bodyScale, SpriteEffects.None, 0.5f);
            batch.End();
        }

        private void formatAllText()
        {
            Vector2 headerSize = font.MeasureString(headerText) * headerScale;
            float headerMaxWidth = (1 - imageBox.Width - spacing) * size.X;
            headerBigScale = headerMaxWidth / headerSize.X; // maybe we need to shrink the header a little extra bit, for massively long strings
            if (headerBigScale > 1)
                headerBigScale = 1; // but don't grow it, headerScale is the max size it can be

            subheaderYOffset = headerSize.Y * headerBigScale + spacing * 0.5f * size.Y;

            headerText = WrapText(font, headerText, headerMaxWidth * 1.5f, headerScale * headerBigScale); // * 1.5 to ensure it never gets word-wrapped
            subheaderText = WrapText(font, subheaderText, (1 - imageBox.Width - spacing) * size.X, subheaderScale, 4);
            bodyText = WrapText(font, bodyText, size.X - 2 * spacing * size.X, bodyScale);

            // cut off body text if it's too long by measuring how many lines are allowed before we hit the end of the box
            startOfBody = new Vector2(position.X + size.X * spacing, position.Y + size.Y * (spacing + imageBox.Height));
            float endOfBoxY = position.Y + size.Y;

            int numLinesAllowed = (int)Math.Floor((endOfBoxY - startOfBody.Y) / heightOfBodyLine);
            int lastIndex = 0, currIndex = 0;
            int i;
            for (i = 0; i < numLinesAllowed; i++)
            {
                currIndex = bodyText.IndexOf('\n', lastIndex, bodyText.Length - lastIndex);
                if (currIndex > 0)
                    lastIndex = currIndex + 1;
                else
                    break;
            }

            // if we completed the loop AND still have found line breaks, we have to cut off the string there
            if (i == numLinesAllowed && currIndex > 0)
                bodyText = bodyText.Substring(0, currIndex - 3) + "...";
        }

        public void DrawSelectable()
        {
            XNA.PushMatrix();
            XNA.Translate(position);
            XNA.Scale(size);

            maxImageBox.DrawSelectable();

            XNA.PopMatrix();
        }

        public void Update(GameTime time)
        {
            double timeElapsed = time.ElapsedGameTime.TotalSeconds;
            if (timeElapsed > 0)
                rotationRange.performTimestep(timeElapsed);
        }

        private string concatenateStrings(string[] str, bool useLinebreaks)
        {
            StringBuilder sb = new StringBuilder();
            foreach (string s in str)
            {
                sb.Append(s);
                if (useLinebreaks)
                    sb.Append("\n");
                else
                {
                    if (s.Length > 0)
                        sb.Append(s[s.Length - 1] == ' ' ? "" : " "); // append a space if the last char is not already a space
                }
            }
            return sb.ToString();
        }

        // returns a string of materials suitable for view
        // inserts commas and removes duplicate entries
        private string getMaterialString(List<Material> materials)
        {
            StringBuilder sb = new StringBuilder();
            Dictionary<string, bool> usedMats = new Dictionary<string, bool>();

            for (int i = 0; i < materials.Count; i++)
            {
                if (!usedMats.ContainsKey(materials[i].Primary))
                {
                    usedMats.Add(materials[i].Primary, true);
                    sb.Append(materials[i].Primary + (i < materials.Count - 1 ? ", " : ""));
                }
            }
            if (sb.Length > 3 && sb[sb.Length - 3] == ',')
                sb.Remove(sb.Length - 3, 2);
            return sb.ToString();
        }

        private void library_SelectedArtifactChanged(Artifact selectedArtifact)
        {
            setImageLocation(selectedArtifact);
            imageBox.Texture = selectedArtifact.Texture;

            headerText = selectedArtifact.ArticleName;
            StringBuilder sb = new StringBuilder();
            //subheaderText = "Dates: " + selectedArtifact.CatalogDate.StartYear + ", " + selectedArtifact.ManufactureDate.StartYear + ", " + selectedArtifact.UseDate.StartYear + "\n";
            sb.Append("Catalog #: "); sb.Append(selectedArtifact.CatalogNumber); sb.Append("\n");
            sb.Append("Materials: "); sb.Append(getMaterialString(selectedArtifact.Materials)); sb.Append("\n");

            subheaderText = sb.ToString();
            
            //subheaderText += "Color: " + selectedArtifact.Color.ToString();

            bodyText = concatenateStrings(new string[] { selectedArtifact.Function, selectedArtifact.CanadianSignificance, selectedArtifact.TechSignificance }, false);

            //bodyText = "we don't know what to put here yet, so here's a super long string that will represent a description of some sort later, hopefully... " +
            //    "we don't know what to put here yet, so here's a super long string that will represent a description of some sort later, hopefully... " +
            //    "we don't know what to put here yet, so here's a super long string that will represent a description of some sort later, hopefully";

            formatAllText();
        }

        void Library_LanguageChanged(int newLanguage)
        {
            library_SelectedArtifactChanged(bookshelf.Library.SelectedArtifact); // just get it to update itself with same artifact
        }

    }
}
