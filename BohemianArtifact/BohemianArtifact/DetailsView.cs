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

        private Vector3 center = new Vector3(0.5f, 0.5f, 0);
        private SelectableQuad maxImageBox; // the space a 512x512 image would fill, lesser images will fill a portion of this box
        private SelectableQuad imageBox;

        private BoundingBox boundingBox;

        private FloatRange rotationRange;

        // inline body refers to what occurs to the right of the picture, other body is what's left underneath
        private string headerText, subheaderText, inlineBodyText, lowerBodyText;
        private string bodyText;
        private float headerScale, subheaderScale, bodyScale;
        private float headerBigScale; // if header is too big to fit in a line with headerScale, this is an additional compression factor
        private float spacing = 0.02f;
        private float subheaderYOffset;
        private float inlineBodyYOffset;
        private float heightOfBodyLine, heightofSpaceBetweenLines;
        private Vector2 startOfLowerBody;

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
            subheaderScale = 0.7f;
            bodyScale = 0.55f;

            heightOfBodyLine = font.MeasureString("Eg").Y * bodyScale; // Eg = a high capital letter + low-hanging lowercase to fill the space
            heightofSpaceBetweenLines = font.MeasureString("Eg\nEg").Y * bodyScale - 2 * heightOfBodyLine;

            //setLengths(bbshelf.Library.Artifacts); // for debugging

            maxImageBox.TouchReleased += new TouchReleaseEventHandler(maxImageBox_TouchReleased);
            //bbshelf.SelectableObjects.AddObject(maxImageBox); // for debugging
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
                if(len > 0)
                    descLengths.Add(a, len);
            }
            descLengths = descLengths.OrderByDescending(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
            //descLengths = descLengths.OrderBy(x => x.Value).ToDictionary(x => x.Key, x => x.Value);
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

        #region Text-wrapping Functions

        public string wrapText(SpriteFont spriteFont, string text, float maxLineWidth, float fontScale)
        {
            return wrapText(spriteFont, text, maxLineWidth, fontScale, 0);
        }

        // returns a string containing manual line breaks whenever a word exceeds max line width
        // numIndentSpaces of spaces will be inserted at the beginning of each word-wrapped line
        // can still force a line break with \n in your string (will not get indented)
        public string wrapText(SpriteFont spriteFont, string text, float maxLineWidth, float fontScale, int numIndentSpaces)
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

        // returns a string containing manual line breaks whenever a word exceeds max line width
        // it will go until numLineBreaks is met, then it will return the remainder of the string unchanged (might be the empty string)
        // can still force a line break with \n in your string, will count towards the numLineBreaks limit
        public string wrapTextPartial(SpriteFont spriteFont, string text, float maxLineWidth, float fontScale, int numLineBreaks, out string remainder)
        {
            string[] words = text.Split(new char[] { ' ' });
            StringBuilder sb = new StringBuilder(); // the main word-wrapped text
            StringBuilder more = new StringBuilder(); // the remainder
            float lineWidth = 0f;

            float spaceWidth = spriteFont.MeasureString(" ").X * fontScale;

            int breakCount = 0;
            string currWord;
            int j = 0;
            for(j = 0; j < words.Length && breakCount < numLineBreaks; j++)
            {
                currWord = words[j];
                string[] subwords = currWord.Split(new char[] { '\n' });
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
                    breakCount++;
                    if (breakCount >= numLineBreaks)
                        more.Append(currWord + " ");
                    else
                        sb.Append(currWord + " ");
                    lineWidth = size.X + spaceWidth;
                }

                if (subwords.Length > 1)
                {
                    size = spriteFont.MeasureString(subwords[subwords.Length - 1]) * fontScale;
                    for (int i = 1; i < subwords.Length; i++)
                    {
                        sb.Append("\n");
                        breakCount++;
                        if (breakCount >= numLineBreaks)
                            more.Append(subwords[i] + " ");
                        else
                            sb.Append(subwords[i] + " ");
                    }
                    lineWidth = size.X + spaceWidth;
                }
            }

            // check if we broke because of the line break count; if so, we have to append all the remaining words
            if (j < words.Length)
            {
                for (int jj = j; jj < words.Length; jj++)
                    more.Append(words[jj] + " ");
                remainder = more.ToString();
            }
            else
                remainder = String.Empty;

            return sb.ToString();
        }

        #endregion

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
            for (int i = 0; i < 50; i++)
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
            batch.DrawString(font, inlineBodyText, new Vector2(position.X + size.X * (imageBox.Width + spacing), position.Y + inlineBodyYOffset),
                bodyColor, 0, Vector2.Zero, bodyScale, SpriteEffects.None, 0.5f);
            batch.End();

            batch.Begin();
            batch.DrawString(font, lowerBodyText, startOfLowerBody,
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
            // lower body will start under the picture by default, but Y coordinate might get adjusted slightly
            startOfLowerBody = new Vector2(position.X + size.X * spacing, position.Y + size.Y * (spacing + imageBox.Height));

            //headerText = WrapText(font, headerText, headerMaxWidth * 1.5f, headerScale * headerBigScale); // * 1.5 to ensure it never gets word-wrapped
            subheaderText = wrapText(font, subheaderText, (1 - imageBox.Width - spacing) * size.X, subheaderScale, 4);

            // split the body text into two strings; one that goes to the side of the image, and the other that goes below
            // one of these may be null, depending on how big the picture is and how long the string is
            // we will draw the two chunks in two different spots, but it will look as if it just wraps around the picture
            divideBodyString();

            // lower body text still needs to get wrapped
            lowerBodyText = wrapText(font, lowerBodyText, size.X - 2 * spacing * size.X, bodyScale);

            // cut off body text if it's too long by measuring how many lines are allowed before we hit the end of the box
            float endOfBoxY = position.Y + size.Y;
            int numLinesAllowed = (int)Math.Floor((endOfBoxY - startOfLowerBody.Y) / heightOfBodyLine);
            int lastIndex = 0, currIndex = 0;
            int i;
            for (i = 0; i < numLinesAllowed; i++)
            {
                currIndex = lowerBodyText.IndexOf('\n', lastIndex, lowerBodyText.Length - lastIndex);
                if (currIndex > 0)
                    lastIndex = currIndex + 1;
                else
                    break;
            }

            // if we completed the loop AND still have found line breaks, we have to cut off the string there
            if (i == numLinesAllowed && currIndex > 0)
                lowerBodyText = lowerBodyText.Substring(0, currIndex - 3) + "...";
        }

        // body text contains the entire body string, but just putting it under the picture might look bad
        // instead, break it up into two strings that will fit around the picture, starting to the right of the picture
        // if the picture isn't tall enough, then just put it all under the image
        private void divideBodyString()
        {
            // where we should start drawing the inline body; below the subheader and spaced a bit farther down
            inlineBodyYOffset = subheaderYOffset + font.MeasureString(subheaderText).Y * subheaderScale + spacing * 2 * size.Y;
            float bottomOfPicture = imageBox.Height * size.Y;
            float numLinesBeside = (bottomOfPicture - inlineBodyYOffset) / (heightOfBodyLine + heightofSpaceBetweenLines);
            int wholeNumLines = (int)Math.Floor(numLinesBeside);

            // bump y offset down a bit so it lines up nice with the edge of the picture
            inlineBodyYOffset += (numLinesBeside - wholeNumLines) * (heightOfBodyLine + heightofSpaceBetweenLines + spacing * size.Y); 

            int linesCutoff = 4; // if we can display this many (or more) lines next to the picture, go for it; otherwise, forget it and display everything below
            if (wholeNumLines >= linesCutoff)
            {
                // wrap num lines using the width beside the picture, and receive the remainder to be wrapped with a different width later
                inlineBodyText = wrapTextPartial(font, bodyText, (1 - imageBox.Width - spacing) * size.X, bodyScale, wholeNumLines, out lowerBodyText);
                // this value needs to be adjusted so it perfectly lines up with the vertical spacing of the inline text
                startOfLowerBody.Y = position.Y + inlineBodyYOffset + (heightOfBodyLine + heightofSpaceBetweenLines) * wholeNumLines;
            }
            else
            {
                // everything has to be displayed below the picture because there's not enough room beside it
                inlineBodyText = "";
                lowerBodyText = bodyText;
            }
        }

        public void DrawSelectable()
        {
            XNA.PushMatrix();
            XNA.Translate(position);
            XNA.Scale(size);

            // debugging only; hitting the image box returns artifacts based on the length of their description
            // maxImageBox should not be put in selectable objects in final build
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
                    sb.Append(materials[i].Primary + ", ");
                }
            }
            // need to remove final comma; can't just count entries in above loop because duplicates can mess it up
            if (sb.Length > 2 && sb[sb.Length - 2] == ',')
                sb.Remove(sb.Length - 2, 2);
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

            formatAllText();
        }

        void Library_LanguageChanged(int newLanguage)
        {
            library_SelectedArtifactChanged(bookshelf.Library.SelectedArtifact); // just get it to update itself with same artifact
        }

    }
}
