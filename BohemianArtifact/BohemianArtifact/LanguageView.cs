using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.IO;

namespace BohemianArtifact
{
    public class LanguageView : IViewBB
    {
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
        

        private BohemianArtifact bookshelf;

        private Vector3 center = new Vector3(0.5f, 0.25f, 0);

        private SelectableEllipse englishCircle;
        private SelectableEllipse frenchCircle;
        private float unselectedAlpha = 0.3f;
        private float selectedAlpha = 0.7f;

        public LanguageView(BohemianArtifact bbshelf, Vector3 position, Vector3 size)
        {
            bookshelf = bbshelf;
            this.position = position;
            this.size = size;

            Texture2D englishTexture = XNA.LoadTexture(BohemianArtifact.TexturePath + "en_flag.jpg");
            englishCircle = new SelectableEllipse(new Vector2(0.25f, 0.25f), 0.225f, 0.01f, Color.White, Color.Gray, englishTexture);
            englishCircle.Alpha = unselectedAlpha;
            englishCircle.TouchActivated += new TouchActivatedEventHandler(Circle_TouchActivated);
            englishCircle.TouchReleased += new TouchReleaseEventHandler(Circle_TouchReleased);
            bookshelf.SelectableObjects.AddObject(englishCircle);

            Texture2D frenchTexture = XNA.LoadTexture(BohemianArtifact.TexturePath + "fr_flag.jpg");
            frenchCircle = new SelectableEllipse(new Vector2(0.75f, 0.25f), 0.225f, 0.01f, Color.White, Color.Gray, frenchTexture);
            frenchCircle.Alpha = unselectedAlpha;
            frenchCircle.TouchActivated += new TouchActivatedEventHandler(Circle_TouchActivated);
            frenchCircle.TouchReleased += new TouchReleaseEventHandler(Circle_TouchReleased);
            bookshelf.SelectableObjects.AddObject(frenchCircle);

            if (Artifact.CurrentLanguage == Artifact.LANGUAGE_ENGLISH)
                englishCircle.Alpha = selectedAlpha;
            else
                frenchCircle.Alpha = selectedAlpha;
        }

        void Circle_TouchActivated(object sender, TouchArgs e)
        {
            //SelectableEllipse circle = sender as SelectableEllipse;
            //circle.Alpha = selectedAlpha;
        }

        void Circle_TouchReleased(object sender, TouchArgs e)
        {
            SelectableEllipse circle = sender as SelectableEllipse;
            //circle.Alpha = unselectedAlpha;
            if (bookshelf.TouchPoints.ContainsKey(e.TouchId) == false)
            {
                if (sender == englishCircle && Artifact.CurrentLanguage == Artifact.LANGUAGE_FRENCH)
                {
                    Artifact.CurrentLanguage = Artifact.LANGUAGE_ENGLISH;
                    Console.WriteLine("english selected");
                    frenchCircle.Alpha = unselectedAlpha;
                    englishCircle.Alpha = selectedAlpha;
                    bookshelf.Library.fireLanguageChangedEvent(Artifact.CurrentLanguage);                    
                }
                else if (sender == frenchCircle && Artifact.CurrentLanguage == Artifact.LANGUAGE_ENGLISH)
                {
                    Artifact.CurrentLanguage = Artifact.LANGUAGE_FRENCH;
                    Console.WriteLine("french selected");
                    englishCircle.Alpha = unselectedAlpha;
                    frenchCircle.Alpha = selectedAlpha;
                    bookshelf.Library.fireLanguageChangedEvent(Artifact.CurrentLanguage);
                }
            }
        }

        public void Draw()
        {
            XNA.PushMatrix();
            XNA.Translate(position);
            XNA.Scale(size);

            XNA.PushMatrix();
            XNA.Translate(englishCircle.Position);
            englishCircle.DrawFillBorder(true);
            XNA.PopMatrix();

            XNA.PushMatrix();
            XNA.Translate(frenchCircle.Position);
            frenchCircle.DrawFillBorder(true);
            XNA.PopMatrix();

            XNA.PopMatrix();
        }

        public void DrawSelectable()
        {
            XNA.PushMatrix();
            XNA.Translate(position);
            XNA.Scale(size);

            XNA.PushMatrix();
            XNA.Translate(englishCircle.Position);
            englishCircle.DrawSelectable();
            XNA.PopMatrix();

            XNA.PushMatrix();
            XNA.Translate(frenchCircle.Position);
            frenchCircle.DrawSelectable();
            XNA.PopMatrix();

            XNA.PopMatrix();
        }

        public void Update(GameTime time)
        {
        }

    }
}
