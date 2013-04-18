using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.IO;

namespace BohemianArtifact
{
    public class MaterialsView : IViewBB
    {
        public class AnimationParameter
        {
            public float Angle;
            public float SpanAngle;
            public float MiddleRadius;
            public float CircleRadius;
        }

        public class MaterialContainer
        {
            public SelectableBlob Blob;
            public float Proportion;
            public string Name;
            public float Angle;
            public AnimationParameter StartKey;
            public AnimationParameter EndKey;

            public override bool Equals(object obj)
            {
                return (obj == Blob);
            }
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
        struct Tier
        {
            public Tier(float middleRadius, float circleRadius)
            {
                MiddleRadius = middleRadius;
                CircleRadius = circleRadius;
            }
            public float MiddleRadius;
            public float CircleRadius;
        }

        private List<ArrayList> materialTiers;
        private List<string> materialConstraints;
        private Tier[] tierBoundaries;
        private const int MAX_TIERS = 5;

        private Dictionary<string, Texture2D> materialTextures;
        private Dictionary<string, Color> materialColors;
        private Dictionary<string, SelectableText> materialTexts;

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

        private SelectableEllipse borderCircle;
        private Vector3 center = new Vector3(0.5f, 0.5f, 0);

        private Timer animationTimer;

        public MaterialsView(BohemianArtifact bbshelf, Vector3 position, Vector3 size)
        {
            bookshelf = bbshelf;
            bookshelf.Library.SelectedArtifactChanged += new ArtifactLibrary.SelectedArtifactHandler(library_SelectedArtifactChanged);
            this.position = position;
            this.size = size;
            font = bookshelf.Content.Load<SpriteFont>("Arial");

            titleText = new SelectableText(font, "Materials", new Vector3(0.4f, 0, 0), bookshelf.GlobalTextColor, Color.White);
            titleText.InverseScale(0.8f, size.X, size.Y);

            // create a list of tier boundaries
            tierBoundaries = new Tier[MAX_TIERS];
            tierBoundaries[0] = new Tier(0, 0.25f);
            tierBoundaries[1] = new Tier(0.275f, 0.02f);
            tierBoundaries[2] = new Tier(0.325f, 0.020f);
            tierBoundaries[3] = new Tier(0.375f, 0.020f);
            tierBoundaries[4] = new Tier(0.425f, 0.020f);
            // and a list of materials that were previously selected
            materialConstraints = new List<string>();
            // and the list of tiers, including the center
            materialTiers = new List<ArrayList>();

            // the border circle that encases the centre and separates the tiers
            borderCircle = new SelectableEllipse(new Vector2(center.X, center.Y), tierBoundaries[0].CircleRadius, 0.01f, Color.White, Color.Gray, null);

            animationTimer = new Timer(0.5f);
            animationTimer.FinishEvent += new TimerFinished(AnimationTimerFinished);

            materialColors = new Dictionary<string, Color>();
            materialTextures = new Dictionary<string, Texture2D>();
            LoadMaterialTextures();
            materialTexts = new Dictionary<string, SelectableText>();
            LoadMaterialTexts();

            CreateMaterialBlobs();
            PackAllTiers();
            animationTimer.Start();
        }

        private void LoadMaterialTextures()
        {
            Dictionary<string, int> materialList = bookshelf.Library.GetMaterialsTally(new List<string>());

            foreach (string material in materialList.Keys)
            {
                materialTextures.Add(material, null);
                /* for now we will just use colors, no textures
                try
                {
                    Texture2D texture = XNAHelper.LoadTexture(BohemianArtifact.TexturePath + "material\\" + material + ".jpg");
                    materialTextures.Add(material, texture);
                }
                catch (Exception e)
                {
                    materialTextures.Add(material, null);
                }
                //*/
            }
            materialColors.Add("ceramic", Color.Goldenrod);
            materialColors.Add("céramique", Color.Goldenrod);

            materialColors.Add("synthetic", Color.LightPink);
            materialColors.Add("synthétique", Color.LightPink);

            materialColors.Add("fluid", Color.LightBlue);
            materialColors.Add("liquide", Color.LightBlue);

            materialColors.Add("skin", Color.Beige);
            materialColors.Add("peau", Color.Beige);

            materialColors.Add("glass", Color.LightGray);
            materialColors.Add("verre", Color.LightGray);

            materialColors.Add("stone", Color.Gray);
            materialColors.Add("pierre", Color.Gray);

            materialColors.Add("plant", Color.ForestGreen);
            materialColors.Add("plante", Color.ForestGreen);

            materialColors.Add("metal", Color.AliceBlue);
            materialColors.Add("métal", Color.AliceBlue);

            materialColors.Add("wood", Color.BurlyWood);
            materialColors.Add("bois", Color.BurlyWood);

            materialColors.Add("resin", Color.Tan);
            materialColors.Add("résine", Color.Tan);

            materialColors.Add("fibre", Color.Thistle);

            materialColors.Add("animal", Color.Turquoise);

            materialColors.Add("paper", Color.PaleGoldenrod);
            materialColors.Add("papier", Color.PaleGoldenrod);

            materialColors.Add("composite", Color.PapayaWhip);
        }

        private void LoadMaterialTexts()
        {
            Dictionary<string, int> materialList = bookshelf.Library.GetMaterialsTally(new List<string>());

            foreach (string material in materialList.Keys)
            {
                SelectableText text = new SelectableText(font, material, new Vector3(0, 0, 0), Color.Black, Color.White);
                text.InverseScale(0.5f, size.X, size.Y);
                materialTexts.Add(material, text);
            }
        }

        private void CreateMaterialBlobs()
        {
            ArrayList newMaterialTier = new ArrayList();
            //materialTiers.Add(newMaterialTier);
            materialTiers.Insert(0, newMaterialTier);

            Dictionary<string, int> materialTally = bookshelf.Library.GetMaterialsTally(materialConstraints);
            int maxTallyCount = 0;
            int totalTallyCount = 0;
            
            // find the maxTallyCount
            foreach (int tally in materialTally.Values)
            {
                if (maxTallyCount < tally)
                {
                    maxTallyCount = tally;
                }
                totalTallyCount += tally;
            }

            float smoothedTallyTotal = 0;
            // compute the total tally after smoothing (sqrt)
            foreach (int i in materialTally.Values)
            {
                smoothedTallyTotal += (float)Math.Sqrt(Math.Sqrt((double)i));
            }

            // create a circle for each material, and scale it's size based on maxTallyCount
            foreach (string materialName in materialTally.Keys)
            {
                float size = (float)(Math.Sqrt(Math.Sqrt((float)((int)materialTally[materialName]))) / smoothedTallyTotal);
                float radius = 0.25f * size;
                //float size = (float)((int)materialTally[materialName]) / (float)totalTallyCount;
                //float radius = 0.04f * (float)Math.Sqrt(Math.Sqrt(size));

                // create a new blob as a circle for the time being
                MaterialContainer newContainer = new MaterialContainer();
                if (materialTextures[materialName] == null)
                {
                    //newContainer.Blob = new SelectableBlob(new Vector2(center.X, center.Y), 0, (Color)materialColors[materialName], null);
                    newContainer.Blob = new SelectableBlob(new Vector2(center.X, center.Y), 0, radius, 0, 0.005f,
                        (Color)materialColors[materialName], XNA.DarkenColor((Color)materialColors[materialName], 0.75f), XNA.DarkenColor((Color)materialColors[materialName], 0.5f), null);
                }
                else
                {
                    //newContainer.Blob = new SelectableBlob(new Vector2(center.X, center.Y), 0, Color.White, (Texture2D)materialTextures[materialName]);
                    newContainer.Blob = new SelectableBlob(new Vector2(center.X, center.Y), 0, 0, 0, 10,
                        Color.White, Color.DarkGray, Color.DarkGray, (Texture2D)materialTextures[materialName]);
                }
                //^ set the edge thickness above
                //newContainer.Blob.SetBorderColors(XNA.DarkenColor(newContainer.Blob.Color, 0.75f), XNA.DarkenColor(newContainer.Blob.Color, 0.25f));
                newContainer.Blob.TouchReleased += new TouchReleaseEventHandler(Blob_TouchReleased);
                newContainer.Name = materialName;
                newContainer.Proportion = size;
                newContainer.Angle = 0;
                newContainer.StartKey = new AnimationParameter();
                newContainer.EndKey = new AnimationParameter();
                // add the blob to the selectable objects list
                bookshelf.SelectableObjects.AddObject(newContainer.Blob);
                //SelectableEllipse newBlob = new SelectableEllipse(Vector2.Zero, radius, 0, Color.Red, Color.Black, null);
                newMaterialTier.Add(newContainer);
            }
        }

        private MaterialContainer FindMaterialContainerFromBlob(SelectableBlob blob, out int tier)
        {
            tier = 0;
            foreach (ArrayList list in materialTiers)
            {
                int index;
                if ((index = list.IndexOf(blob)) != -1) // change the arraylist into a hashtable to make the list searchable by the SelectableBlobs
                {
                    return (MaterialContainer)list[index];
                }
                tier++;
            }
            tier = -1;
            return null;
        }

        // this is for the first tier, which is the center of the circle
        private void CirclePack()
        {
            ArrayList tier = materialTiers[0];

            for (int i = 0; i < tier.Count; i++)
            {
                float angle = (float)(2 * i * Math.PI / tier.Count);
                float distance = 0.6f * tierBoundaries[0].CircleRadius;
                //Vector3 newPosition = new Vector3(distance * (float)Math.Cos(angle) / 2, distance * (float)Math.Sin(angle) / 2, 0);
                //newPosition += center;
                //((MaterialContainer)tier[i]).Blob.CenterPosition = newPosition;
                //((MaterialContainer)tier[i]).Blob.SpanAngle = 0;
                //((MaterialContainer)tier[i]).Angle = angle;

                ((MaterialContainer)tier[i]).StartKey.Angle = ((MaterialContainer)tier[i]).Angle;
                ((MaterialContainer)tier[i]).StartKey.CircleRadius = ((MaterialContainer)tier[i]).Blob.CircleRadius;
                ((MaterialContainer)tier[i]).StartKey.MiddleRadius = ((MaterialContainer)tier[i]).Blob.MiddleRadius;
                if (((MaterialContainer)tier[i]).Blob.SpanAngle == 0)
                {
                    ((MaterialContainer)tier[i]).StartKey.SpanAngle = 0;
                }
                else
                {
                    ((MaterialContainer)tier[i]).StartKey.SpanAngle = ((MaterialContainer)tier[i]).Blob.SpanAngle;
                }

                ((MaterialContainer)tier[i]).EndKey.Angle = angle;
                ((MaterialContainer)tier[i]).EndKey.CircleRadius = 0.25f * ((MaterialContainer)tier[i]).Proportion;
                ((MaterialContainer)tier[i]).EndKey.MiddleRadius = distance;
                ((MaterialContainer)tier[i]).EndKey.SpanAngle = 0;

                // set the color to make sure that it isn't darkened
                ((MaterialContainer)tier[i]).Blob.Color = (Color)materialColors[((MaterialContainer)tier[i]).Name];
            }
        }

        private void ShellPack(int tierId)
        {
            ArrayList tier = materialTiers[tierId];
            // keep a running sum how much each blob is rotated so we can position the next blob next to it
            float totalAngle = 0;

            for (int i = 0; i < tier.Count; i++)
            {
                // copy the keyframe details of the blob to the StartKey
                ((MaterialContainer)tier[i]).StartKey.Angle = ((MaterialContainer)tier[i]).Angle;
                ((MaterialContainer)tier[i]).StartKey.CircleRadius = ((MaterialContainer)tier[i]).Blob.CircleRadius;
                ((MaterialContainer)tier[i]).StartKey.MiddleRadius = ((MaterialContainer)tier[i]).Blob.MiddleRadius;
                ((MaterialContainer)tier[i]).StartKey.SpanAngle = ((MaterialContainer)tier[i]).Blob.SpanAngle;

                //((MaterialContainer)tier[i]).Blob.CenterPosition = center;
                //((MaterialContainer)tier[i]).Blob.CircleRadius = tierBoundaries[tierId].CircleRadius;
                //((MaterialContainer)tier[i]).Blob.MiddleRadius = tierBoundaries[tierId].MiddleRadius;
                //((MaterialContainer)tier[i]).Blob.SpanAngle = angle * 0.95f;
                //((MaterialContainer)tier[i]).Angle = angle / 2 + totalAngle;

                // first we must compute the spanangle of the blob and what angle it sits at in the shell
                float angle = (float)(((MaterialContainer)tier[i]).Proportion * 2 * Math.PI);

                // and now setup the EndKey
                ((MaterialContainer)tier[i]).EndKey.Angle = angle / 2 + totalAngle;
                ((MaterialContainer)tier[i]).EndKey.CircleRadius = tierBoundaries[tierId].CircleRadius;
                ((MaterialContainer)tier[i]).EndKey.MiddleRadius = tierBoundaries[tierId].MiddleRadius;
                ((MaterialContainer)tier[i]).EndKey.SpanAngle = angle * 0.95f;

                // running sum of the blob angles
                totalAngle += angle;
            }
        }

        private void PackAllTiers()
        {
            CirclePack();
            for (int i = 1; i < materialTiers.Count; i++)
            {
                ShellPack(i);
            }
        }

        public void Draw()
        {
            XNA.PushMatrix();
            XNA.Translate(position);
            XNA.Scale(size);

            titleText.DrawFill();
            
            XNA.PushMatrix();
            XNA.Translate(borderCircle.Position);
            borderCircle.DrawFillBorder(false);
            XNA.PopMatrix();

            lock (materialTiers)
            {
                foreach (ArrayList list in materialTiers)
                {
                    // first draw each blob
                    foreach (MaterialContainer container in list)
                    {
                        XNA.PushMatrix();
                        XNA.Translate(container.Blob.CenterPosition);
                        XNA.RotateZ(container.Angle);
                        container.Blob.DrawFill(true);
                        container.Blob.DrawBorder();
                        XNA.PopMatrix();
                    }
                    // then draw the text over top of it
                    foreach (MaterialContainer container in list)
                    {
                        SelectableText text = (SelectableText)materialTexts[container.Name];
                        // text for circle packing in the center (vertically aligned)
                        XNA.PushMatrix();
                        XNA.Translate(-text.TextSize.X / 2, -text.TextSize.Y / 2, 0);
                        XNA.Translate(container.Blob.CenterPosition);
                        XNA.RotateZ(container.Angle);
                        XNA.Translate(container.Blob.MiddleRadius, 0, 0);
                        XNA.RotateZ(-container.Angle);
                        text.Draw();
                        XNA.PopMatrix();
                    }
                }
            }

            XNA.PopMatrix();
        }

        public void DrawSelectable()
        {
            XNA.PushMatrix();
            XNA.Translate(position);
            XNA.Scale(size);

            // draw the blobs
            lock (materialTiers)
            {
                foreach (ArrayList list in materialTiers)
                {
                    foreach (MaterialContainer container in list)
                    {
                        XNA.PushMatrix();
                        XNA.Translate(container.Blob.CenterPosition);
                        XNA.RotateZ(container.Angle);
                        container.Blob.DrawSelectable();
                        XNA.PopMatrix();
                    }
                }
            }

            XNA.PopMatrix();
        }

        private void AnimateAllBlobs()
        {
            lock (materialTiers)
            {
                foreach (ArrayList tier in materialTiers)
                {
                    for (int i = 0; i < tier.Count; i++)
                    {
                        float circleRadius = animationTimer.Elapsed * (((MaterialContainer)tier[i]).EndKey.CircleRadius - ((MaterialContainer)tier[i]).StartKey.CircleRadius) + ((MaterialContainer)tier[i]).StartKey.CircleRadius;
                        float middleRadius = animationTimer.Elapsed * (((MaterialContainer)tier[i]).EndKey.MiddleRadius - ((MaterialContainer)tier[i]).StartKey.MiddleRadius) + ((MaterialContainer)tier[i]).StartKey.MiddleRadius;
                        float spanAngle = animationTimer.Elapsed * (((MaterialContainer)tier[i]).EndKey.SpanAngle - ((MaterialContainer)tier[i]).StartKey.SpanAngle) + ((MaterialContainer)tier[i]).StartKey.SpanAngle;
                        ((MaterialContainer)tier[i]).Blob.SetDimensions(spanAngle, circleRadius, middleRadius);
                        ((MaterialContainer)tier[i]).Angle = animationTimer.Elapsed * (((MaterialContainer)tier[i]).EndKey.Angle - ((MaterialContainer)tier[i]).StartKey.Angle) + ((MaterialContainer)tier[i]).StartKey.Angle;

                        //((MaterialContainer)tier[i]).Blob.CircleRadius = animationTimer.Elapsed * (((MaterialContainer)tier[i]).EndKey.CircleRadius - ((MaterialContainer)tier[i]).StartKey.CircleRadius) + ((MaterialContainer)tier[i]).StartKey.CircleRadius;
                        //((MaterialContainer)tier[i]).Blob.MiddleRadius = animationTimer.Elapsed * (((MaterialContainer)tier[i]).EndKey.MiddleRadius - ((MaterialContainer)tier[i]).StartKey.MiddleRadius) + ((MaterialContainer)tier[i]).StartKey.MiddleRadius;
                        //((MaterialContainer)tier[i]).Blob.SpanAngle = animationTimer.Elapsed * (((MaterialContainer)tier[i]).EndKey.SpanAngle - ((MaterialContainer)tier[i]).StartKey.SpanAngle) + ((MaterialContainer)tier[i]).StartKey.SpanAngle;
                    }
                }
            }
        }

        private void AnimationTimerFinished()
        {
            lock (materialTiers)
            {
                foreach (ArrayList tier in materialTiers)
                {
                    for (int i = 0; i < tier.Count; i++)
                    {
                        ((MaterialContainer)tier[i]).Angle = ((MaterialContainer)tier[i]).EndKey.Angle;
                        ((MaterialContainer)tier[i]).Blob.CircleRadius = ((MaterialContainer)tier[i]).EndKey.CircleRadius;
                        ((MaterialContainer)tier[i]).Blob.MiddleRadius = ((MaterialContainer)tier[i]).EndKey.MiddleRadius;
                        ((MaterialContainer)tier[i]).Blob.SpanAngle = ((MaterialContainer)tier[i]).EndKey.SpanAngle;
                        //((MaterialContainer)tier[i]).Blob.Recompute();
                    }
                }
            }
        }

        public void Update(GameTime time)
        {
            animationTimer.Update(time.TotalGameTime.TotalSeconds);
            if (animationTimer.Running == true)
            {
                AnimateAllBlobs();
            }

            if (bookshelf.TouchPoints.Count == 0)
            {
                return;
            }
            MaterialContainer foundContainer = null;
            foreach (ArrayList list in materialTiers)
            {
                foreach (MaterialContainer container in list)
                {
                    if (container.Blob.TouchId != Touch.NO_ID)// && ((Touch)bookshelf.TouchPoints[blob.TouchId]).IsActive == false)
                    {
                        // very basic touch detection at this point, only on touch-down, not on touch-release
                        foundContainer = container;
                        break;
                    }
                }
                if (foundContainer != null)
                {
                    break;
                }
            }
        }

        private void library_SelectedArtifactChanged(Artifact selectedArtifact)
        {
            //// code to change the selected book
            //throw new NotImplementedException();
        }

        private void Blob_TouchReleased(object sender, TouchReleaseEventArgs e)
        {
            if (bookshelf.TouchPoints.ContainsKey(e.TouchId) == false)
            {
                lock (materialTiers)
                {
                    int tierId;
                    MaterialContainer selectedMaterial = FindMaterialContainerFromBlob((SelectableBlob)sender, out tierId);
                    if (tierId == 0)
                    {
                        ArrayList list = materialTiers[tierId];
                        for (int i = 0; i < list.Count; i++)
                        {
                            // darken all the non-selected material colors
                            if (list[i] != selectedMaterial)
                            {
                                ((MaterialContainer)(list[i])).Blob.Color = XNA.DarkenColor(((MaterialContainer)(list[i])).Blob.Color, 0.5f); ;
                            }
                        }
                        materialConstraints.Add(selectedMaterial.Name);
                        CreateMaterialBlobs();
                    }
                    else
                    {
                        for (int i = 0; i < tierId; i++)
                        {
                            materialConstraints.RemoveAt(materialConstraints.Count - 1);
                            materialTiers.RemoveAt(0);
                        }
                    }
                    PackAllTiers();
                    animationTimer.Start();
                }
            }
        }

    }
}
