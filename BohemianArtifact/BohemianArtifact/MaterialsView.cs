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
            public Color Color;
            public float Proportion;
            public Material Material;
            public float Angle;
            public bool IsConstraint = false;
            public bool IsClickable = true;
            public AnimationParameter StartKey;
            public AnimationParameter EndKey;
            private SelectableText[] text;
            public SelectableText Text
            {
                get
                {
                    return text[Artifact.CurrentLanguage];
                }
            }
            public SelectableText[] TextArray
            {
                get
                {
                    return text;
                }
                set
                {
                    text = value;
                }
            }

            public override bool Equals(object obj)
            {
                return (obj == Blob);
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

        private List<MaterialContainer> materialList;
        private List<Material> materialConstraints;

        private Dictionary<Material, Texture2D> materialTextures;
        private Dictionary<Material, Color> materialColors;
        private Dictionary<Material, SelectableText[]> materialTexts;

        private SelectableBlob borderCircle;
        private const float BORDER_RADIUS = 0.35f;
        private const float SHELL_RADIUS_THICKNESS = 0.2f;

        private SelectableText titleText;
        private BohemianArtifact bookshelf;

        private Vector3 center = new Vector3(0.5f, 0.5f, 0);
        private Vector2 circleCentre = new Vector2(0.5f, 0.65f);

        private Timer animationTimer;

        public MaterialsView(BohemianArtifact bbshelf, Vector3 position, Vector3 size)
        {
            bookshelf = bbshelf;
            bookshelf.Library.SelectedArtifactChanged += new ArtifactLibrary.SelectedArtifactHandler(library_SelectedArtifactChanged);
            this.position = position;
            this.size = size;

            titleText = new SelectableText(XNA.Font, "Materials", new Vector3(0.4f, 0, 0), bookshelf.GlobalTextColor, Color.White);
            titleText.InverseScale(0.8f, size.X, size.Y);

            // the border circle that encases the centre and separates the tiers
            borderCircle = new SelectableBlob(circleCentre, (float)Math.PI, BORDER_RADIUS * 0.01f, BORDER_RADIUS, 0, Color.Black, Color.Black, Color.Black, null);

            animationTimer = new Timer(0.5f);
            animationTimer.FinishEvent += new TimerFinished(AnimationTimerFinished);

            //materialTexts = new Dictionary<string, SelectableText>();
            //LoadMaterialTexts();
            //materialTextures = new Dictionary<string, Texture2D>();
            //LoadMaterialTextures();
            materialColors = new Dictionary<Material, Color>();
            LoadMaterialColors();

            // a list of materials that are selected
            materialConstraints = new List<Material>();

            materialList = new List<MaterialContainer>();
            CreateMaterialBlobs();
            ShellPack();
            //PackAllTiers();
            animationTimer.Start();
        }

        private void LoadMaterialColors()
        {
            materialColors.Add(new Material("ceramic", "céramique"), Color.Goldenrod);

            materialColors.Add(new Material("synthetic", "synthétique"), Color.LightPink);

            materialColors.Add(new Material("fluid", "liquide"), Color.LightBlue);

            materialColors.Add(new Material("skin", "peau"), Color.Beige);

            materialColors.Add(new Material("glass", "verre"), Color.LightGray);

            materialColors.Add(new Material("stone", "pierre"), Color.Gray);

            materialColors.Add(new Material("plant", "plante"), Color.ForestGreen);

            materialColors.Add(new Material("metal", "métal"), Color.AliceBlue);

            materialColors.Add(new Material("wood", "bois"), Color.BurlyWood);

            materialColors.Add(new Material("resin", "résine"), Color.Tan);

            materialColors.Add(new Material("fibre", "fibre"), Color.Thistle);

            materialColors.Add(new Material("animal", "animal"), Color.Turquoise);

            materialColors.Add(new Material("paper", "papier"), Color.PaleGoldenrod);

            materialColors.Add(new Material("composite", "composite"), Color.PapayaWhip);

            //    materialColors.Add("ceramic", Color.Goldenrod);
            //    materialColors.Add("céramique", Color.Goldenrod);

            //    materialColors.Add("synthetic", Color.LightPink);
            //    materialColors.Add("synthétique", Color.LightPink);

            //    materialColors.Add("fluid", Color.LightBlue);
            //    materialColors.Add("liquide", Color.LightBlue);

            //    materialColors.Add("skin", Color.Beige);
            //    materialColors.Add("peau", Color.Beige);

            //    materialColors.Add("glass", Color.LightGray);
            //    materialColors.Add("verre", Color.LightGray);

            //    materialColors.Add("stone", Color.Gray);
            //    materialColors.Add("pierre", Color.Gray);

            //    materialColors.Add("plant", Color.ForestGreen);
            //    materialColors.Add("plante", Color.ForestGreen);

            //    materialColors.Add("metal", Color.AliceBlue);
            //    materialColors.Add("métal", Color.AliceBlue);

            //    materialColors.Add("wood", Color.BurlyWood);
            //    materialColors.Add("bois", Color.BurlyWood);

            //    materialColors.Add("resin", Color.Tan);
            //    materialColors.Add("résine", Color.Tan);

            //    materialColors.Add("fibre", Color.Thistle);

            //    materialColors.Add("animal", Color.Turquoise);

            //    materialColors.Add("paper", Color.PaleGoldenrod);
            //    materialColors.Add("papier", Color.PaleGoldenrod);

            //    materialColors.Add("composite", Color.PapayaWhip);
        }

        /*
        private void LoadMaterialTextures()
        {
            Dictionary<string, int> materialList = bookshelf.Library.GetMaterialsTally(new List<string>());

            foreach (string material in materialList.Keys)
            {
                materialTextures.Add(material, null);
                // for now we will just use colors, no textures
                //try
                //{
                //    Texture2D texture = XNAHelper.LoadTexture(BohemianArtifact.TexturePath + "material\\" + material + ".jpg");
                //    materialTextures.Add(material, texture);
                //}
                //catch (Exception e)
                //{
                //    materialTextures.Add(material, null);
                //}
            }
        }
        //*/

        private void LoadMaterialTexts()
        {
            Dictionary<Material, int> materialList = bookshelf.Library.GetMaterialsTally(new List<Material>());

            foreach (Material material in materialList.Keys)
            {
                SelectableText[] text = new SelectableText[2] { new SelectableText(XNA.Font, material.PrimaryArray[Artifact.LANGUAGE_ENGLISH], new Vector3(0, 0, 0), Color.Black, Color.White), 
                    new SelectableText(XNA.Font, material.PrimaryArray[Artifact.LANGUAGE_FRENCH], new Vector3(0, 0, 0), Color.Black, Color.White) };
                text[Artifact.LANGUAGE_ENGLISH].InverseScale(0.5f, size.X, size.Y);
                text[Artifact.LANGUAGE_FRENCH].InverseScale(0.5f, size.X, size.Y);
                materialTexts.Add(material, text);
            }
        }

        private void CreateMaterialBlobs()
        {
            Dictionary<Material, int> materialTally = bookshelf.Library.GetMaterialsTally(materialConstraints);
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
            foreach (Material material in materialTally.Keys)
            {
                // sqrt(sqrt(tally)) / sum[i from 0 to n, sqrt(sqrt(i))] (see above)
                // this is the "smoothing function" that we apply to the material tally
                // some materials have only several occurences while others have hundreds
                // this function puts the size of a materials on the same order of magnitude
                float mSize = (float)(Math.Sqrt(Math.Sqrt((float)((int)materialTally[material]))) / smoothedTallyTotal);
                float radius = 0.25f * mSize;

                MaterialContainer newContainer = new MaterialContainer();
                /* we don't use textures, so skip this code
                // create a new blob as a circle for the time being
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
                //*/

                newContainer.Blob = new SelectableBlob(circleCentre, 0, radius, BORDER_RADIUS + radius, 0.0025f,
                    (Color)materialColors[material], XNA.DarkenColor((Color)materialColors[material], 0.75f), XNA.DarkenColor((Color)materialColors[material], 0.5f), null);
                
                //^ set the edge thickness above
                //newContainer.Blob.SetBorderColors(XNA.DarkenColor(newContainer.Blob.Color, 0.75f), XNA.DarkenColor(newContainer.Blob.Color, 0.25f));
                newContainer.Blob.TouchReleased += new TouchReleaseEventHandler(Blob_TouchReleased);
                newContainer.Proportion = mSize;
                newContainer.Angle = 0;
                newContainer.StartKey = new AnimationParameter();
                newContainer.EndKey = new AnimationParameter();

                newContainer.TextArray = new SelectableText[2] { new SelectableText(XNA.Font, material.PrimaryArray[Artifact.LANGUAGE_ENGLISH], new Vector3(0, 0, 0), Color.Black, Color.White), 
                    new SelectableText(XNA.Font, material.PrimaryArray[Artifact.LANGUAGE_FRENCH], new Vector3(0, 0, 0), Color.Black, Color.White) };
                newContainer.TextArray[Artifact.LANGUAGE_ENGLISH].InverseScale(0.5f, size.X, size.Y);
                newContainer.TextArray[Artifact.LANGUAGE_FRENCH].InverseScale(0.5f, size.X, size.Y);


                // add the blob to the selectable objects list
                bookshelf.SelectableObjects.AddObject(newContainer.Blob);
                //SelectableEllipse newBlob = new SelectableEllipse(Vector2.Zero, radius, 0, Color.Red, Color.Black, null);
                materialList.Add(newContainer);
            }
        }

        private void ShellPack()
        {
            // keep a running sum how much each blob is rotated so we can position the next blob next to it
            float totalShellAngle = 0;
            float totalCenterAngle = 0;
            float totalShellProportions = 0;
            float totalCenterProportions = 0;
            int numShellBlobs = 0;
            int numCenterBlobs = 0;
            foreach (MaterialContainer container in materialList)
            {
                if (container.IsConstraint == true)
                {
                    totalCenterProportions += container.Proportion;
                    numCenterBlobs++;
                }
                else
                {
                    totalShellProportions += container.Proportion;
                    numShellBlobs++;
                }
            }

            for (int i = 0; i < materialList.Count; i++)
            {

                // copy the keyframe details of the blob to the StartKey
                // we do this regardless of whether or not this container is a constraint material or not
                ((MaterialContainer)materialList[i]).StartKey.Angle = ((MaterialContainer)materialList[i]).Angle;
                ((MaterialContainer)materialList[i]).StartKey.CircleRadius = ((MaterialContainer)materialList[i]).Blob.CircleRadius;
                ((MaterialContainer)materialList[i]).StartKey.MiddleRadius = ((MaterialContainer)materialList[i]).Blob.MiddleRadius;
                ((MaterialContainer)materialList[i]).StartKey.SpanAngle = ((MaterialContainer)materialList[i]).Blob.SpanAngle;

                //((MaterialContainer)tier[i]).Blob.CenterPosition = center;
                //((MaterialContainer)tier[i]).Blob.CircleRadius = tierBoundaries[tierId].CircleRadius;
                //((MaterialContainer)tier[i]).Blob.MiddleRadius = tierBoundaries[tierId].MiddleRadius;
                //((MaterialContainer)tier[i]).Blob.SpanAngle = angle * 0.95f;
                //((MaterialContainer)tier[i]).Angle = angle / 2 + totalAngle;

                // and now setup the EndKey
                if (materialList[i].IsConstraint == true)
                {
                    // compute what angle it sits at in the center
                    float angle = (float)(((MaterialContainer)materialList[i]).Proportion * Math.PI / totalCenterProportions);

                    // if this blob is a contraint, then put it in the centre
                    ((MaterialContainer)materialList[i]).EndKey.Angle = angle / 2 + totalCenterAngle;
                    ((MaterialContainer)materialList[i]).EndKey.CircleRadius = materialList[i].Proportion * 0.25f / (float)Math.Sqrt(totalCenterProportions);
                    ((MaterialContainer)materialList[i]).EndKey.MiddleRadius = BORDER_RADIUS * 0.6f;
                    ((MaterialContainer)materialList[i]).EndKey.SpanAngle = 0;

                    totalCenterAngle += angle;
                }
                else
                {
                    // otherwise put it on the shell

                    // compute the spanangle of the blob and what angle it sits at in the shell
                    //float angle = (float)(((MaterialContainer)materialList[i]).Proportion * Math.PI / totalShellProportions);
                    float angle = (float)(Math.PI / numShellBlobs);

                    ((MaterialContainer)materialList[i]).EndKey.Angle = angle / 2 + totalShellAngle;
                    ((MaterialContainer)materialList[i]).EndKey.CircleRadius = materialList[i].Proportion * SHELL_RADIUS_THICKNESS;
                    ((MaterialContainer)materialList[i]).EndKey.MiddleRadius = materialList[i].Proportion * SHELL_RADIUS_THICKNESS + BORDER_RADIUS * 1.02f;
                    ((MaterialContainer)materialList[i]).EndKey.SpanAngle = angle * 0.95f;

                    // running sum of the blob angles
                    totalShellAngle += angle;
                }
            }
        }


        private MaterialContainer FindMaterialContainerFromBlob(SelectableBlob blob)
        {
            foreach (MaterialContainer container in materialList)
            {
                if (container.Blob == blob)
                {
                    return container;
                }
            }
            return null;
        }

        /*
        // this is for the first tier, which is the center of the circle
        private void CirclePack()
        {
            for (int i = 0; i < materialList.Count; i++)
            {
                float angle = (float)(2 * i * Math.PI / materialList.Count);
                float distance = 0.6f * tierBoundaries[0].CircleRadius;
                //Vector3 newPosition = new Vector3(distance * (float)Math.Cos(angle) / 2, distance * (float)Math.Sin(angle) / 2, 0);
                //newPosition += center;
                //((MaterialContainer)tier[i]).Blob.CenterPosition = newPosition;
                //((MaterialContainer)tier[i]).Blob.SpanAngle = 0;
                //((MaterialContainer)tier[i]).Angle = angle;

                ((MaterialContainer)materialList[i]).StartKey.Angle = ((MaterialContainer)materialList[i]).Angle;
                ((MaterialContainer)materialList[i]).StartKey.CircleRadius = ((MaterialContainer)materialList[i]).Blob.CircleRadius;
                ((MaterialContainer)materialList[i]).StartKey.MiddleRadius = ((MaterialContainer)materialList[i]).Blob.MiddleRadius;
                if (((MaterialContainer)materialList[i]).Blob.SpanAngle == 0)
                {
                    ((MaterialContainer)materialList[i]).StartKey.SpanAngle = 0;
                }
                else
                {
                    ((MaterialContainer)materialList[i]).StartKey.SpanAngle = ((MaterialContainer)materialList[i]).Blob.SpanAngle;
                }

                ((MaterialContainer)materialList[i]).EndKey.Angle = angle;
                ((MaterialContainer)materialList[i]).EndKey.CircleRadius = 0.25f * ((MaterialContainer)materialList[i]).Proportion;
                ((MaterialContainer)materialList[i]).EndKey.MiddleRadius = distance;
                ((MaterialContainer)materialList[i]).EndKey.SpanAngle = 0;

                // set the color to make sure that it isn't darkened
                ((MaterialContainer)materialList[i]).Blob.Color = (Color)materialColors[((MaterialContainer)materialList[i]).Name];
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
        //*/

        public void Draw()
        {
            XNA.PushMatrix();
            XNA.Translate(position);
            XNA.Scale(size);

            titleText.DrawFill();
            
            XNA.PushMatrix();
            XNA.Translate(borderCircle.CenterPosition);
            XNA.RotateZ((float)Math.PI / 2);
            borderCircle.DrawFillBorder(false);
            XNA.PopMatrix();

            //*
            lock (materialList)
            {
                // first draw each blob
                foreach (MaterialContainer container in materialList)
                {
                    XNA.PushMatrix();
                    XNA.Translate(container.Blob.CenterPosition);
                    XNA.RotateZ(container.Angle);
                    container.Blob.DrawFillBorder(false);
                    //container.Blob.DrawBorder();
                    XNA.PopMatrix();
                }
                // then draw the text over top of it
                foreach (MaterialContainer container in materialList)
                {
                    //SelectableText text = (SelectableText)materialTexts[container.Material];
                    SelectableText text = container.Text;
                    // text for circle packing in the center
                    XNA.PushMatrix();
                    XNA.Translate(container.Blob.CenterPosition);
                    XNA.RotateZ(container.Angle);
                    XNA.Translate(container.Blob.MiddleRadius + container.Blob.CircleRadius, 0, 0);
                    // rotate the material text to the correct angle
                    if (container.IsConstraint == true)
                    {
                        XNA.RotateZ(-container.Angle);
                    }
                    else
                    {
                        XNA.RotateZ(-(float)Math.PI / 2);
                    }
                    //XNA.Translate(-text.TextSize.X / 2, -text.TextSize.Y / 2, 0); // this one translate to the centre of the text
                    XNA.Translate(-text.TextSize.X / 2, 0, 0);
                    text.Draw();
                    XNA.PopMatrix();
                }
            }
            //*/

            XNA.PopMatrix();
        }

        public void DrawSelectable()
        {
            XNA.PushMatrix();
            XNA.Translate(position);
            XNA.Scale(size);

            // draw the blobs
            lock (materialList)
            {
                foreach (MaterialContainer container in materialList)
                {
                    XNA.PushMatrix();
                    XNA.Translate(container.Blob.CenterPosition);
                    XNA.RotateZ(container.Angle);
                    container.Blob.DrawSelectable();
                    XNA.PopMatrix();
                }
            }

            XNA.PopMatrix();
        }

        private void AnimateAllBlobs()
        {
            //*
            lock (materialList)
            {
                for (int i = 0; i < materialList.Count; i++)
                {
                    float circleRadius = animationTimer.Elapsed * (((MaterialContainer)materialList[i]).EndKey.CircleRadius - ((MaterialContainer)materialList[i]).StartKey.CircleRadius) + ((MaterialContainer)materialList[i]).StartKey.CircleRadius;
                    float middleRadius = animationTimer.Elapsed * (((MaterialContainer)materialList[i]).EndKey.MiddleRadius - ((MaterialContainer)materialList[i]).StartKey.MiddleRadius) + ((MaterialContainer)materialList[i]).StartKey.MiddleRadius;
                    float spanAngle = animationTimer.Elapsed * (((MaterialContainer)materialList[i]).EndKey.SpanAngle - ((MaterialContainer)materialList[i]).StartKey.SpanAngle) + ((MaterialContainer)materialList[i]).StartKey.SpanAngle;
                    ((MaterialContainer)materialList[i]).Blob.SetDimensions(spanAngle, circleRadius, middleRadius);
                    ((MaterialContainer)materialList[i]).Angle = animationTimer.Elapsed * (((MaterialContainer)materialList[i]).EndKey.Angle - ((MaterialContainer)materialList[i]).StartKey.Angle) + ((MaterialContainer)materialList[i]).StartKey.Angle;

                    //((MaterialContainer)tier[i]).Blob.CircleRadius = animationTimer.Elapsed * (((MaterialContainer)tier[i]).EndKey.CircleRadius - ((MaterialContainer)tier[i]).StartKey.CircleRadius) + ((MaterialContainer)tier[i]).StartKey.CircleRadius;
                    //((MaterialContainer)tier[i]).Blob.MiddleRadius = animationTimer.Elapsed * (((MaterialContainer)tier[i]).EndKey.MiddleRadius - ((MaterialContainer)tier[i]).StartKey.MiddleRadius) + ((MaterialContainer)tier[i]).StartKey.MiddleRadius;
                    //((MaterialContainer)tier[i]).Blob.SpanAngle = animationTimer.Elapsed * (((MaterialContainer)tier[i]).EndKey.SpanAngle - ((MaterialContainer)tier[i]).StartKey.SpanAngle) + ((MaterialContainer)tier[i]).StartKey.SpanAngle;
                }
            }
            //*/
        }

        private void AnimationTimerFinished()
        {
            lock (materialList)
            {
                for (int i = 0; i < materialList.Count; i++)
                {
                    ((MaterialContainer)materialList[i]).Angle = ((MaterialContainer)materialList[i]).EndKey.Angle;
                    ((MaterialContainer)materialList[i]).Blob.CircleRadius = ((MaterialContainer)materialList[i]).EndKey.CircleRadius;
                    ((MaterialContainer)materialList[i]).Blob.MiddleRadius = ((MaterialContainer)materialList[i]).EndKey.MiddleRadius;
                    ((MaterialContainer)materialList[i]).Blob.SpanAngle = ((MaterialContainer)materialList[i]).EndKey.SpanAngle;
                    //((MaterialContainer)tier[i]).Blob.Recompute();
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
            foreach (MaterialContainer container in materialList)
            {
                if (container.Blob.TouchId != Touch.NO_ID)// && ((Touch)bookshelf.TouchPoints[blob.TouchId]).IsActive == false)
                {
                    // very basic touch detection at this point, only on touch-down, not on touch-release
                    foundContainer = container;
                    break;
                }
            }
        }

        private void library_SelectedArtifactChanged(Artifact selectedArtifact)
        {
            //// code to change the selected book
            //throw new NotImplementedException();
        }

        private void Blob_TouchReleased(object sender, TouchArgs e)
        {
            if (bookshelf.TouchPoints.ContainsKey(e.TouchId) == false)
            {
                //*
                lock (materialList)
                {
                    MaterialContainer selectedContainer = FindMaterialContainerFromBlob((SelectableBlob)sender);
                    if (selectedContainer == null)
                    {
                        return;
                    }

                    selectedContainer.IsConstraint = true;
                    materialConstraints.Add(selectedContainer.Material);
                    
                    ShellPack();
                    animationTimer.Start();
                }
                //*/
            }
        }

    }
}
