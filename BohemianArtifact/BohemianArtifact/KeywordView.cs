using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.IO;

namespace BohemianArtifact
{
    public class KeywordView : IViewBB
    {
        public class KeywordContainer
        {
            public SelectableEllipse Circle;
            public Artifact Artifact;

            public override bool Equals(object obj)
            {
                return (obj == Circle);
            }
            public override int GetHashCode()
            {
                return base.GetHashCode();
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
        private SelectableText titleText;
        private BohemianArtifact bookshelf;

        private List<KeywordContainer> artifactCircles;
        private const int MAX_CIRCLES = 9;
        private const int CENTER_CIRCLE_ID = MAX_CIRCLES - 1;
        private int numArtifactCircles = MAX_CIRCLES;

        private const float RELATED_TEXT_LABEL_SCALE = 0.0006f;
        private const float CENTER_TEXT_LABEL_SCALE = 0.0008f;

        private VertexPositionColor[] wavelet;

        private float centerCircleRadius;
        private float relatedCircleRadius;

        private Vector3 center = new Vector3(0.5f, 0.5f, 0);

        private Timer animationTimer;

        public KeywordView(BohemianArtifact bbshelf, Vector3 position, Vector3 size)
        {
            bookshelf = bbshelf;
            bookshelf.Library.SelectedArtifactChanged += new ArtifactLibrary.SelectedArtifactHandler(library_SelectedArtifactChanged);
            this.position = position;
            this.size = size;
            font = bookshelf.Content.Load<SpriteFont>("Arial");

            titleText = new SelectableText(font, "Keywords", new Vector3(0.4f, 0, 0), bookshelf.GlobalTextColor, Color.White);
            titleText.InverseScale(0.8f, size.X, size.Y);

            centerCircleRadius = 0.1f;
            relatedCircleRadius = centerCircleRadius * 0.5f;

            artifactCircles = new List<KeywordContainer>();
            for (int i = 0; i < MAX_CIRCLES; i++)
            {
                KeywordContainer newContainer = new KeywordContainer();
                newContainer.Circle = new SelectableEllipse(new Vector2(center.X, center.Y), relatedCircleRadius, centerCircleRadius * 0.05f, Color.White, new Color(1, 1, 1, 0), Color.Black, null);
                newContainer.Circle.TouchReleased += new TouchReleaseEventHandler(Circle_TouchReleased);
                bookshelf.SelectableObjects.AddObject(newContainer.Circle);
                artifactCircles.Add(newContainer);
            }
            artifactCircles[CENTER_CIRCLE_ID].Circle.Radius = centerCircleRadius;
            PositionRelatedCircles();

            wavelet = new VertexPositionColor[10];
            for (int i = 0; i < wavelet.Length; i++)
            {
                wavelet[i].Color = Color.Blue;
                wavelet[i].Position.X = 0.25f * i / wavelet.Length;
                wavelet[i].Position.Y = 0.5f;
            }

            animationTimer = new Timer(0.5f);
            animationTimer.FinishEvent += new TimerFinished(AnimationTimerFinished);
        }

        private void PositionRelatedCircles()
        {
            float rad = 0.15f;
            Vector3 position = Vector3.Zero;
            for (int i = 0; i < numArtifactCircles; i++)
            {
                position.X = rad * 2 * (float)(Math.Cos(2 * Math.PI * i / (numArtifactCircles))) + center.X;
                position.Y = rad * 2 * (float)(Math.Sin(2 * Math.PI * i / (numArtifactCircles))) + center.Y;
                artifactCircles[i].Circle.Position = position;
            }
            artifactCircles[CENTER_CIRCLE_ID].Circle.Position = center;
        }

        public void Draw()
        {
            XNA.PushMatrix();
            XNA.Translate(position);
            XNA.Scale(size);

            titleText.DrawFill();

            lock (artifactCircles)
            {
                // draw the related circles
                for (int i = 0; i < numArtifactCircles; i++)
                {
                    XNA.PushMatrix();
                    XNA.Translate(artifactCircles[i].Circle.Position);
                    artifactCircles[i].Circle.DrawFillBorder(true);
                    XNA.Translate(RELATED_TEXT_LABEL_SCALE * -artifactCircles[i].Artifact.Text.TextSize.X / 2, artifactCircles[i].Circle.Radius, 0);
                    artifactCircles[i].Artifact.Text.DrawScale(RELATED_TEXT_LABEL_SCALE);
                    XNA.PopMatrix();
                }
                // draw the center circle
                XNA.PushMatrix();
                XNA.Translate(artifactCircles[CENTER_CIRCLE_ID].Circle.Position);
                artifactCircles[CENTER_CIRCLE_ID].Circle.DrawFillBorder(true);
                XNA.Translate(CENTER_TEXT_LABEL_SCALE * -artifactCircles[CENTER_CIRCLE_ID].Artifact.Text.TextSize.X / 2, artifactCircles[CENTER_CIRCLE_ID].Circle.Radius, 0);
                artifactCircles[CENTER_CIRCLE_ID].Artifact.Text.DrawScale(CENTER_TEXT_LABEL_SCALE);
                XNA.PopMatrix();
            }

            XNA.PushMatrix();
            //XNA.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, wavelet, 0, 9);
            XNA.PopMatrix();

            XNA.PopMatrix();
        }

        public void DrawSelectable()
        {
            XNA.PushMatrix();
            XNA.Translate(position);
            XNA.Scale(size);

            lock (artifactCircles)
            {
                // draw the related circles
                for (int i = 0; i < numArtifactCircles; i++)
                {
                    XNA.PushMatrix();
                    XNA.Translate(artifactCircles[i].Circle.Position);
                    artifactCircles[i].Circle.DrawSelectable();
                    XNA.PopMatrix();
                }
                // draw the center circle
                XNA.PushMatrix();
                XNA.Translate(artifactCircles[CENTER_CIRCLE_ID].Circle.Position);
                artifactCircles[CENTER_CIRCLE_ID].Circle.DrawSelectable();
                XNA.PopMatrix();
            }

            XNA.PopMatrix();
        }

        private void AnimationTimerFinished()
        {
        }

        public void Update(GameTime time)
        {
            animationTimer.Update(time.TotalGameTime.TotalSeconds);

            wavelet[1].Position.Y = (float)(Math.Sin(time.TotalGameTime.TotalSeconds) * 0.01f + 0.5f);
            for (int i = 2; i < wavelet.Length; i++)
            {
                //wavelet[i].Position = wavelet[i - 1].Position
                //wavelet[i].Position.X = 0.25f * i / wavelet.Length;
                //wavelet[i].Position.Y = (float)(Math.Sin((float)i / 10 * 2 * Math.PI) * Math.Sin(Math.Cos(time.TotalGameTime.TotalSeconds)) * 0.1f + 0.5f);
            }
        }

        private KeywordContainer FindMaterialContainerFromCircle(SelectableEllipse circle)
        {
            foreach (KeywordContainer keyContainer in artifactCircles)
            {
                if (keyContainer.Circle == circle)
                {
                    return keyContainer;
                }
            }
            return null;
        }

        private void Circle_TouchReleased(object sender, TouchReleaseEventArgs e)
        {
            if (bookshelf.TouchPoints.ContainsKey(e.TouchId) == false)
            {
                lock (artifactCircles)
                {
                    KeywordContainer selectedCircle = FindMaterialContainerFromCircle((SelectableEllipse)sender);

                    // only do stuff if we tap on a circle that's not the center circle
                    if (selectedCircle != artifactCircles[CENTER_CIRCLE_ID])
                    {
                        bookshelf.Library.SelectedArtifact = selectedCircle.Artifact;
                    }
                }
            }
        }

        private int CompareArtifactSimilarity(KeyValuePair<Artifact, float> a, KeyValuePair<Artifact, float> b)
        {
            if (a.Value < b.Value)
            {
                return 1;
            }
            else if (b.Value < a.Value)
            {
                return -1;
            }
            else
            {
                return 0;
            }
        }

        private void library_SelectedArtifactChanged(Artifact selectedArtifact)
        {
            // change the central circle artifact and texture
            artifactCircles[CENTER_CIRCLE_ID].Artifact = selectedArtifact;
            artifactCircles[CENTER_CIRCLE_ID].Circle.Texture = selectedArtifact.Texture;

            Console.WriteLine("*** New selected artifact " + selectedArtifact.ArticleName + ", colors (" +  selectedArtifact.Color.ToString() + ") and it's stems:");
            Console.Write("\t");
            foreach (StemPair stempair in selectedArtifact.Stems)
            {
                Console.Write(stempair.Stem + ", ");
            }
            Console.Write("\n");

            List<Artifact> candidateList = new List<Artifact>();
            // use the stem graph to find related artifacts and put all found into a candidate list
            foreach (StemPair sp in selectedArtifact.Stems)
            {
                List<StemPair> relatedStemPairs = bookshelf.Library.StemGraph[sp.Stem];
                foreach (StemPair relatedSP in relatedStemPairs)
                {
                    if (candidateList.Contains(relatedSP.ArtifactRef) == false && relatedSP.ArtifactRef != selectedArtifact)
                    {
                        candidateList.Add(relatedSP.ArtifactRef);
                    }
                }
            }

            List<KeyValuePair<Artifact, float>> weightedCandidateList = new List<KeyValuePair<Artifact, float>>();
            foreach (Artifact a in candidateList)
            {
                float sameStemCount = 0;
                int numMatchedStems = 0;
                foreach (StemPair sp in a.Stems)
                {
                    if (selectedArtifact.Stems.Contains(sp) == true)
                    {
                        sameStemCount += bookshelf.Library.StemGraph[sp.Stem].Count;
                        numMatchedStems++;
                        //sameStemCount++;
                    }
                }
                weightedCandidateList.Add(new KeyValuePair<Artifact, float>(a, sameStemCount / (numMatchedStems * numMatchedStems)));
            }

            // sort based on relevence
            weightedCandidateList.Sort(CompareArtifactSimilarity);
            weightedCandidateList.Reverse();

            /* debugging
            foreach (KeyValuePair<Artifact, float> kvp in weightedCandidateList)
            {
                Artifact a = kvp.Key;
                Console.WriteLine(a.ArticleName);
                Console.Write("\t");
                foreach (StemPair stempair in a.Stems)
                {
                    if (selectedArtifact.Stems.Contains(stempair))
                    {
                        Console.Write(stempair.Stem + ", ");
                    }
                }
                Console.Write("\n\n");
            }
            //*/

            int i;
            lock (artifactCircles)
            {
                for (i = 0; i < MAX_CIRCLES - 1 && i < weightedCandidateList.Count; i++)
                {
                    artifactCircles[i].Artifact = weightedCandidateList[i].Key;
                    artifactCircles[i].Circle.Texture = artifactCircles[i].Artifact.Texture;
                }
            }
            numArtifactCircles = i;

            // add half very related
            /*
            for (i = 0; i < MAX_CIRCLES / 2 && i < weightedCandidateList.Count; i++)
            {
                artifactCircles[i].Artifact = weightedCandidateList[i].Key;
                artifactCircles[i].Circle.Texture = artifactCircles[i].Artifact.Texture;
            }
            // and half 
            Random random = new Random();
            for (; i < MAX_CIRCLES - 1; i++)
            {
                Artifact randomArtifact;
                // make sure the random artifact hasn't already been used
                do
                {
                    randomArtifact = bookshelf.Library.Artifacts[random.Next(bookshelf.Library.Artifacts.Count)];
                } while (candidateList.Contains(randomArtifact) == true);

                artifactCircles[i].Artifact = randomArtifact;
                artifactCircles[i].Circle.Texture = artifactCircles[i].Artifact.Texture;
            }
            numArtifactCircles = MAX_CIRCLES - 1;
            // add 1/4 least related
            // add 1/4 random
            //*/
            PositionRelatedCircles();
        }
    }
}
