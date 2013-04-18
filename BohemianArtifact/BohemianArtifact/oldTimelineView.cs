using System;
using System.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Surface.Core;

namespace BohemianArtifact
{
    public class TimelineView :  IViewBB
    {
        private Vector3 position;
        private Vector3 size;
        private SpriteFont font;

        private SelectableLine cross1;
        private SelectableLine cross2;

        private float lineYStart = 0.2f;
        private float lineYEnd = 0.9f;

        private SelectableLine[] lines;
        private Hashtable lineBookTable;
        private float[] pubYearLimit = { 10000, -10000, 0};
        private float[] pubYearZoom = { 0, 1 };
        private float[] subjectYearLimit = { 10000, -10000, 0 };
        private float[] subjectYearZoom = { 0, 1 };
        private const int MIN = 0;
        private const int MAX = 1;
        private const int PADDING = 2;
        private bool areLinesUpdated = true;

        private SelectableText titleText;

        private SelectableQuad topBar;
        private SelectableQuad bottomBar;
        
        private float topOffset;
        private float bottomOffset;

        private BohemianArtifact bookshelf;

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

        public TimelineView(BohemianArtifact bbshelf, Vector3 p, Vector3 s)
        {
            bookshelf = bbshelf;
            bookshelf.Library.SelectedArtifactChanged += new ArtifactLibrary.SelectedArtifactHandler(library_SelectedArtifactChanged);
            position = p;
            size = s;
            font = bookshelf.Content.Load<SpriteFont>("Kootenay");
            lineBookTable = new Hashtable();

            CreateLinesFromBooks();

            cross1 = new SelectableLine(new Vector3(0, 0.2f, 0), new Vector3(0, 0.9f, 0), Color.Blue, 10);
            cross2 = new SelectableLine(new Vector3(1, 0.2f, 0), new Vector3(1, 0.9f, 0), Color.Red, 10);

            titleText = new SelectableText(font, "Timeline View", new Vector3(0.4f, 0, 0), bookshelf.GlobalTextColor, Color.White);
            titleText.InverseScale(0.5f, size.X, size.Y);

            topBar = new SelectableQuad(new Vector2(0, 0.1f), new Vector2(1, 0.1f), Color.LightGray);
            bookshelf.SelectableObjects.AddObject(topBar);
            bottomBar = new SelectableQuad(new Vector2(0, 0.9f), new Vector2(1, 0.1f), Color.LightGray);
            bookshelf.SelectableObjects.AddObject(bottomBar);

            topOffset = 0;
            bottomOffset = 1;
        }

        private void CreateLinesFromBooks()
        {
            // we will have two lines per book
            lines = new SelectableLine[bookshelf.Library.Books.Count * 2];

            int i = 0;
            foreach(Book book in bookshelf.Library.Books)
            {
                // create two lines for each book
                lines[i] = new SelectableLine(new Vector3(0, lineYStart, 0), new Vector3(0, lineYEnd, 0), Color.Gray, 0.01f);
                lines[i].Color = Color.Black;
                lineBookTable.Add(lines[i], book);
                bookshelf.SelectableObjects.AddObject(lines[i]);

                lines[i + 1] = new SelectableLine(new Vector3(0, lineYStart, 0), new Vector3(0, lineYEnd, 0), Color.Gray, 0.01f);
                lines[i + 1].Color = Color.Black;
                lineBookTable.Add(lines[i + 1], book);
                bookshelf.SelectableObjects.AddObject(lines[i + 1]);
                i += 2;

                // and update the pubyear range and the subject year range
                if (book.PubYear < pubYearLimit[MIN])
                {
                    pubYearLimit[MIN] = book.PubYear;
                }
                if (pubYearLimit[MAX] < book.PubYear)
                {
                    pubYearLimit[MAX] = book.PubYear;
                }

                if (book.SubjectTime[0] < subjectYearLimit[MIN])
                {
                    subjectYearLimit[MIN] = book.SubjectTime[0];
                }
                if (subjectYearLimit[MAX] < book.SubjectTime[1])
                {
                    subjectYearLimit[MAX] = book.SubjectTime[1];
                }

                // set the padding levels for both zoom ranges
                pubYearLimit[PADDING] = 0.05f * (pubYearLimit[MAX] - pubYearLimit[MIN]);
                subjectYearLimit[PADDING] = 0.05f * (subjectYearLimit[MAX] - subjectYearLimit[MIN]);

                // set the zoom to show all pubyears and all subject years (showing the entire range for both values)
                pubYearZoom[MIN] = pubYearLimit[MIN];
                pubYearZoom[MAX] = pubYearLimit[MAX];
                subjectYearZoom[MIN] = subjectYearLimit[MIN];
                subjectYearZoom[MAX] = subjectYearLimit[MAX];
            }

            // now position all the lines properly.
            RepositionLines(true);
        }

        private void RepositionLines(bool recompute)
        {
            int i = 0;
            foreach (Book book in bookshelf.Library.Books)
            {
                //lines[i].LinePoints[0].Position.X = (float)(book.PubYear - pubYearLimit[MIN]) / (pubYearLimit[MAX] - pubYearLimit[MIN]);
                //lines[i].LinePoints[1].Position.X = (float)(book.SubjectTime[0] - subjectYearLimit[MIN]) / (subjectYearLimit[MAX] - subjectYearLimit[MIN]);
                //lines[i + 1].LinePoints[0].Position.X = (float)(book.PubYear - pubYearLimit[MIN]) / (pubYearLimit[MAX] - pubYearLimit[MIN]);
                //lines[i + 1].LinePoints[1].Position.X = (float)(book.SubjectTime[0] - subjectYearLimit[MIN]) / (subjectYearLimit[MAX] - subjectYearLimit[MIN]);
                //i += 2;
                lines[i].LinePoints[0].Position.X = (float)(book.PubYear - pubYearZoom[MIN]) / (pubYearZoom[MAX] - pubYearZoom[MIN]);
                lines[i].LinePoints[1].Position.X = (float)(book.SubjectTime[0] - subjectYearZoom[MIN]) / (subjectYearZoom[MAX] - subjectYearZoom[MIN]);
                lines[i + 1].LinePoints[0].Position.X = (float)(book.PubYear - pubYearZoom[MIN]) / (pubYearZoom[MAX] - pubYearZoom[MIN]);
                lines[i + 1].LinePoints[1].Position.X = (float)(book.SubjectTime[0] - subjectYearZoom[MIN]) / (subjectYearZoom[MAX] - subjectYearZoom[MIN]);
                if (recompute == true)
                {
                    lines[i].Recompute();
                    lines[i + 1].Recompute();
                }
                i += 2;
            }
        }

        void library_SelectedBookChanged(Artifact selectedArtifact)
        {
            // code to change the selected book
            throw new NotImplementedException();
        }

        public void Draw()
        {
            XNA.PushMatrix();
            XNA.Translate(position);
            XNA.Scale(size);

            foreach (SelectableLine line in lines)
            {
                if (line.TouchId != -1)
                {
                    ((SelectableQuad)line).Draw();
                }
                else
                {
                    line.Draw();
                }
            }

            titleText.DrawFill();

            topBar.Draw();
            bottomBar.Draw();

            XNA.PopMatrix();
        }

        public void DrawSelectable()
        {
            XNA.PushMatrix();
            XNA.Translate(position);
            XNA.Scale(size);

            foreach (SelectableLine line in lines)
            {
                line.DrawSelectable();
            }

            topBar.DrawSelectable();
            bottomBar.DrawSelectable();

            XNA.PopMatrix();
        }

        public void Update(GameTime time)
        {
            Touch touch;

            // adjust the top bar
            if ((touch = (Touch)bookshelf.TouchPoints[topBar.TouchId]) != null)
            {
                pubYearZoom[MIN] = topOffset - (pubYearLimit[MAX] - pubYearLimit[MIN]) * (touch.X - touch.OriginX) / size.X;
                if (pubYearZoom[MIN] < pubYearLimit[MIN])
                {
                    pubYearZoom[MIN] = pubYearLimit[MIN];
                }
                else if (pubYearLimit[MAX] - pubYearLimit[PADDING] < pubYearZoom[MIN])
                {
                    pubYearZoom[MIN] = pubYearLimit[MAX] - pubYearLimit[PADDING];
                }
            }
            else
            {
                topOffset = pubYearZoom[MIN];
            }

            // adjust the bottom bar
            if ((touch = (Touch)bookshelf.TouchPoints[bottomBar.TouchId]) != null)
            {
                subjectYearZoom[MIN] = bottomOffset - (subjectYearLimit[MAX] - subjectYearLimit[MIN]) * (touch.X - touch.OriginX) / size.X;
                if (subjectYearZoom[MIN] < subjectYearLimit[MIN])
                {
                    subjectYearZoom[MIN] = subjectYearLimit[MIN];
                }
                else if (subjectYearLimit[MAX] - subjectYearLimit[PADDING] < subjectYearZoom[MIN])
                {
                    subjectYearZoom[MIN] = subjectYearLimit[MAX] - subjectYearLimit[PADDING];
                }
            }
            else
            {
                bottomOffset = subjectYearZoom[MIN];
            }

            RepositionLines(true);
        }
    }
}
