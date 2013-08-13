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

        public class ArtifactContainer
        {
            public Artifact Artifact;
            public SelectableEllipse Circle;
            public bool IsConstraint;
            public float LocationOnPath;
            public int PathId;
            public const int HIDDEN = -1;
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
            public bool IsTransitioning = false;
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

        public class SteamPath
        {
            public SelectableBlob Blob;
            public List<ArtifactContainer> Artifacts;
            public float Angle;
            public float Length;
            public int MaxArtifacts;
            public bool Visible;
            public float Padding;
            public SteamPath(Vector2 start, Vector2 end, float curveDistance, float circleRadius, float edgeThickness, Color color, Color insideEdgeColor, Color outsideEdgeColor)
            {
                Padding = 1.1f;
                Blob = new SelectableBlob(start, end, curveDistance, circleRadius, edgeThickness, color, insideEdgeColor, outsideEdgeColor, ref Angle);
                Length = Blob.MiddleRadius * Blob.SpanAngle; // 2*pi*r * span/(2*pi) = r * span
                MaxArtifacts = (int)(Length / (ARTIFACT_CIRCLE_RADIUS * Padding)); // 10% padding
                Artifacts = new List<ArtifactContainer>();
                Visible = false;
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

        private List<ArtifactContainer> artifactList;
        private List<MaterialContainer> materialList;
        private List<Material> materialConstraints;
        private int numArtifactsVisible;

        //private Dictionary<Material, Texture2D> materialTextures;
        //private Dictionary<Material, SelectableText[]> materialTexts;
        private Dictionary<Material, Color> materialColors;

        private SelectableBlob borderCircle;
        private const float BORDER_RADIUS = 0.35f;
        private const float SHELL_RADIUS_THICKNESS = 0.2f;

        private SteamPath[] steamPaths;
        private int numSteam = 7;
        private float steamSpeed;

        private const float ARTIFACT_CIRCLE_RADIUS = 0.05f;

        private SelectableText titleText;
        private BohemianArtifact bookshelf;

        private Vector3 center = new Vector3(0.5f, 0.5f, 0);
        private Vector2 circleCenter = new Vector2(0.5f, 0.65f);

        // the min and max Y coordinates to draw the artifacts inside of - between the title text and the borderCircle (circleCenter)
        private float minArtifactY = 0.1f;
        private float maxArtifactY = 0.65f;
        private Random random = new Random();

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
            borderCircle = new SelectableBlob(circleCenter, (float)Math.PI, BORDER_RADIUS * 0.01f, BORDER_RADIUS, 0, Color.Black, Color.Black, Color.Black, null);

            // create the steam paths
            steamPaths = new SteamPath[numSteam];
            steamSpeed = 5;
            Color steamColor = new Color(0.95f, 0.95f, 0.95f);
            float steamWidth = 0.03f;
            float steamFade = 0.025f;
            // start from the center and work outwards
            steamPaths[0] = new SteamPath(new Vector2(0.5f, circleCenter.Y), new Vector2(0.58f, 0.0f), -0.015f, steamWidth, steamFade, steamColor, steamColor, Color.White);
            steamPaths[1] = new SteamPath(new Vector2(0.6f, circleCenter.Y), new Vector2(0.75f, 0.025f), -0.03f, steamWidth, steamFade, steamColor, steamColor, Color.White);
            steamPaths[2] = new SteamPath(new Vector2(0.4f, circleCenter.Y), new Vector2(0.3f, 0.025f), 0.04f, steamWidth, steamFade, steamColor, steamColor, Color.White);
            steamPaths[3] = new SteamPath(new Vector2(0.3f, circleCenter.Y), new Vector2(0.1f, 0.1f), 0.07f, steamWidth, steamFade, steamColor, steamColor, Color.White);
            steamPaths[4] = new SteamPath(new Vector2(0.7f, circleCenter.Y), new Vector2(0.95f, 0.1f), -0.06f, steamWidth, steamFade, steamColor, steamColor, Color.White);
            steamPaths[5] = new SteamPath(new Vector2(0.8f, circleCenter.Y), new Vector2(1.0f, 0.35f), -0.05f, steamWidth, steamFade, steamColor, steamColor, Color.White);
            steamPaths[6] = new SteamPath(new Vector2(0.2f, circleCenter.Y), new Vector2(0.0f, 0.3f), 0.05f, steamWidth, steamFade, steamColor, steamColor, Color.White);

            animationTimer = new Timer(0.4f);
            animationTimer.FinishEvent += new TimerFinished(AnimationTimerFinished);

            //materialTexts = new Dictionary<string, SelectableText>();
            //LoadMaterialTexts();
            //materialTextures = new Dictionary<string, Texture2D>();
            //LoadMaterialTextures();
            materialColors = new Dictionary<Material, Color>();
            LoadMaterialColors();

            // a list of materials that are selected
            materialConstraints = new List<Material>();

            artifactList = new List<ArtifactContainer>();
            CreateArtifactList();
            UpdateArtifactsFromConstraints();

            RepositionArtifacts();

            // create the list of materials
            materialList = new List<MaterialContainer>();
            CreateMaterialBlobs();
            RepositionBlobs();

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

        #region LoadMaterialTextures & LoadMaterialTexts
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
        //*/
        #endregion

        private void CreateArtifactList()
        {
            foreach (Artifact artifact in bookshelf.Library.Artifacts)
            {
                ArtifactContainer container = new ArtifactContainer();
                container.Artifact = artifact;
                container.IsConstraint = false;
                container.LocationOnPath = 0;
                container.PathId = ArtifactContainer.HIDDEN;
                container.Circle = new SelectableEllipse(Vector2.Zero, ARTIFACT_CIRCLE_RADIUS, ARTIFACT_CIRCLE_RADIUS * 0.05f, Color.White, new Color(1, 1, 1, 0), Color.Black, artifact.Texture);
                container.Circle.TouchReleased += new TouchReleaseEventHandler(Circle_TouchReleased);
                bookshelf.SelectableObjects.AddObject(container.Circle);
                artifactList.Add(container);
            }
        }

        private void UpdateArtifactsFromConstraints()
        {
            numArtifactsVisible = 0;
            foreach (ArtifactContainer container in artifactList)
            {
                container.IsConstraint = true;
                foreach (Material constraint in materialConstraints)
                {
                    if (container.Artifact.Materials.Contains(constraint) == false)
                    {
                        container.IsConstraint = false;
                        break;
                    }
                }

                if (container.IsConstraint == true)
                {
                    numArtifactsVisible++;
                }
            }

            PopulateSteamPaths();
        }

        private void PopulateSteamPaths()
        {
            // first turn off all the paths and clear their artifact lists
            foreach (SteamPath path in steamPaths)
            {
                path.Visible = false;
                path.Artifacts.Clear();
            }

            // number of visible paths
            int numVisiblePaths = 0;
            // count the total length of the visible paths
            // we will populate each path with artifacts in proportion to its length
            float totalPathLength = 0;

            // total number of artifacts added to the paths so far
            int numArtifactsInPaths = numArtifactsVisible;
            bool firstPath = true;
            foreach (SteamPath path in steamPaths)
            {
                if (path.MaxArtifacts < numArtifactsInPaths)
                {
                    // if there are more artifacts left over than this path can hold, make it visible
                    path.Visible = true;
                    numArtifactsInPaths -= path.MaxArtifacts;
                    numVisiblePaths++;
                    totalPathLength += path.Length;

                    firstPath = false;
                }
                else
                {
                    if (firstPath == true)
                    {
                        // in case we have so few artifacts that even the first path isn't made visible,
                        // then we make it visible here
                        path.Visible = true;
                        totalPathLength += path.Length;
                    }
                    break;
                }
            }

            // keep track of our place in the artifact array so we know what artifacts we've already added to the paths
            int artifactIndex = 0;
            int totalArtifactsAddedToPaths = 0;
            foreach (SteamPath path in steamPaths)
            {
                if (path.Visible == false)
                {
                    continue;
                }

                int numArtifactsToAddToPath = (int)(numArtifactsVisible * path.Length / totalPathLength);
                int artifactsAdded = 0;
                while(artifactsAdded < numArtifactsToAddToPath && artifactIndex < artifactList.Count)
                {
                    if (artifactList[artifactIndex].IsConstraint == false)
                    {
                        artifactIndex++;
                        continue;
                    }

                    path.Artifacts.Add(artifactList[artifactIndex]);
                    artifactsAdded++;
                    totalArtifactsAddedToPaths++;
                    artifactIndex++;
                }
            }
            // this takes care of rounding issues when we make the proportion calculation
            while (totalArtifactsAddedToPaths <= numArtifactsVisible && artifactIndex < artifactList.Count)
            {
                if (artifactList[artifactIndex].IsConstraint == false)
                {
                    artifactIndex++;
                    continue;
                }

                // add them to the main path
                steamPaths[0].Artifacts.Add(artifactList[artifactIndex]);
                artifactIndex++;
                totalArtifactsAddedToPaths++;
            }
        }

        private void RepositionArtifacts()
        {
            foreach (SteamPath path in steamPaths)
            {
                if (path.Visible == false)
                {
                    continue;
                }

                for (int i = 0; i < path.Artifacts.Count; i++)
                {
                    path.Artifacts[i].LocationOnPath = (float)i / path.Artifacts.Count;
                    path.Artifacts[i].Circle.Position = new Vector3(
                        path.Blob.CenterPosition.X + path.Blob.MiddleRadius * (float)Math.Cos(path.Angle + path.Blob.SpanAngle * path.Artifacts[i].LocationOnPath - path.Blob.SpanAngle / 2),
                        path.Blob.CenterPosition.Y + path.Blob.MiddleRadius * (float)Math.Sin(path.Angle + path.Blob.SpanAngle * path.Artifacts[i].LocationOnPath - path.Blob.SpanAngle / 2),
                        0);
                }

                ///////// the rest of the code here!!!
                //foreach (ArtifactContainer container in path.Artifacts)
                //{
                //    container.LocationOnPath += steamSpeed / path.Length; // divide by path length to normalize the speed between longer and shorter paths
                //}

                //if (0 < steamSpeed)
                //{
                //    // if we are going up, check if the last artifact in the list should be removed
                //}

                //if (0 < path.Artifacts.Count && // there is at least some artifacts on the path
                //    ARTIFACT_CIRCLE_RADIUS * path.Padding < path.Artifacts[0].LocationOnPath * path.Length && // and the artifact just added is far enough up that a new one can be added
                //    path.Artifacts.Count < path.MaxArtifacts)
                //{

                //}
            }
            foreach (ArtifactContainer container in artifactList)
            {
                if (container.PathId != ArtifactContainer.HIDDEN)
                {
                    container.LocationOnPath += 0.01f;
                    if (1 <= container.LocationOnPath)
                    {
                        // remove container from path
                        //steamPaths[container.PathId].NumArtifacts--;
                        container.PathId = ArtifactContainer.HIDDEN;
                    }
                    // this artifact is on a path
                }
            }
        }

        private void RepositionArtifactsOld()
        {
            // number of rows
            float circleSpacing = ARTIFACT_CIRCLE_RADIUS * 2 * 1.5f;
            int numRows = (int)((maxArtifactY - minArtifactY) / circleSpacing);
            int numCols = (int)(1 / circleSpacing);

            Console.WriteLine("numRows: {0}, numCols: {1}, numArtifacts: {2}", numRows, numCols, numArtifactsVisible);
            while (numArtifactsVisible < numRows * numCols)
            {
                // the grid should be resized because there aren't enough artifacts to fill the entire space
                if (numRows == numCols)
                {
                    numRows--;
                }
                else
                {
                    numCols--;
                }
            }

            float xPosition = (1 - numCols * circleSpacing) / 2;
            float yPosition = ((maxArtifactY - minArtifactY) - numRows * circleSpacing) / 2 + minArtifactY;
            float yStart = yPosition;

            Console.WriteLine("repositioning artifacts. numRows: {0}, numCols: {1}, xStart: {2}, yStart: {3}", numRows, numCols, xPosition, yPosition);

            int i = 0;
            foreach (ArtifactContainer container in artifactList)
            {
                if (container.IsConstraint == true)
                {
                    container.Circle.Position = new Vector3(xPosition + ((float)random.NextDouble() - 0.5f) * ARTIFACT_CIRCLE_RADIUS, yPosition + ((float)random.NextDouble() - 0.5f) * ARTIFACT_CIRCLE_RADIUS, 0);

                    i++;
                    if (i % numRows == 0)
                    {
                        xPosition += circleSpacing;
                        yPosition = yStart;
                    }
                    else
                    {
                        yPosition += circleSpacing;
                    }
                }
            }
        }

        private void UpdateBlobsFromConstraints()
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

            // change whether or not the container is visible or not
            foreach (MaterialContainer container in materialList)
            {
                if (materialTally.ContainsKey(container.Material) == false)
                {
                    if (container.IsClickable == true)
                    {
                        // this container was clickable, but now isn't
                        container.IsTransitioning = true;
                    }
                    container.IsClickable = false;
                    continue;
                }
                else
                {
                    if (container.IsClickable == false)
                    {
                        // this container wasn't clickable, but now is
                        container.IsTransitioning = true;
                    }
                    container.IsClickable = true;
                }

                // sqrt(sqrt(tally)) / sum[i from 0 to n, sqrt(sqrt(i))] (see above)
                // this is the "smoothing function" that we apply to the material tally
                // some materials have only several occurences while others have hundreds
                // this function puts the size of a materials on the same order of magnitude
                float mSize = (float)(Math.Sqrt(Math.Sqrt((float)((int)materialTally[container.Material]))) / smoothedTallyTotal);
                container.Proportion = mSize;
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

                newContainer.Blob = new SelectableBlob(circleCenter, 0, radius, BORDER_RADIUS + radius, 0.0025f,
                    (Color)materialColors[material], XNA.DarkenColor((Color)materialColors[material], 0.75f), XNA.DarkenColor((Color)materialColors[material], 0.5f), null);
                
                //^ set the edge thickness above
                //newContainer.Blob.SetBorderColors(XNA.DarkenColor(newContainer.Blob.Color, 0.75f), XNA.DarkenColor(newContainer.Blob.Color, 0.25f));
                newContainer.Blob.TouchReleased += new TouchReleaseEventHandler(Blob_TouchReleased);
                newContainer.Proportion = mSize;
                newContainer.Angle = 0;
                newContainer.Material = material;
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

        private void RepositionBlobs()
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
                if (container.IsClickable == false)
                {
                    continue;
                }

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
                materialList[i].StartKey.Angle = materialList[i].Angle;
                materialList[i].StartKey.CircleRadius = materialList[i].Blob.CircleRadius;
                materialList[i].StartKey.MiddleRadius = materialList[i].Blob.MiddleRadius;
                materialList[i].StartKey.SpanAngle = materialList[i].Blob.SpanAngle;

                // and now setup the EndKey
                if (materialList[i].IsClickable == true)
                {
                    if (materialList[i].IsConstraint == true)
                    {
                        // compute what angle it sits at in the center
                        float angle = (float)(((MaterialContainer)materialList[i]).Proportion * Math.PI / totalCenterProportions);

                        // if this blob is a contraint, then put it in the centre
                        materialList[i].EndKey.Angle = angle / 2 + totalCenterAngle;
                        materialList[i].EndKey.CircleRadius = materialList[i].Proportion * 0.25f / (float)Math.Sqrt(totalCenterProportions);
                        materialList[i].EndKey.MiddleRadius = BORDER_RADIUS * 0.6f;
                        materialList[i].EndKey.SpanAngle = 0;

                        totalCenterAngle += angle;
                    }
                    else
                    {
                        // otherwise put it on the shell

                        // compute the spanangle of the blob and what angle it sits at in the shell
                        //float angle = (float)(((MaterialContainer)materialList[i]).Proportion * Math.PI / totalShellProportions);
                        float angle = (float)(Math.PI / numShellBlobs);

                        materialList[i].EndKey.Angle = angle / 2 + totalShellAngle;
                        materialList[i].EndKey.CircleRadius = materialList[i].Proportion * SHELL_RADIUS_THICKNESS;
                        materialList[i].EndKey.MiddleRadius = materialList[i].Proportion * SHELL_RADIUS_THICKNESS + BORDER_RADIUS * 1.02f;
                        materialList[i].EndKey.SpanAngle = angle * 0.95f;

                        // running sum of the blob angles
                        totalShellAngle += angle;
                    }
                }
                else
                {
                    float angle = (float)(Math.PI / numShellBlobs);

                    materialList[i].EndKey.Angle = materialList[i].Angle;
                    materialList[i].EndKey.CircleRadius = 0;
                    materialList[i].EndKey.MiddleRadius = materialList[i].Proportion * SHELL_RADIUS_THICKNESS + BORDER_RADIUS * 1.2f;
                    materialList[i].EndKey.SpanAngle = 0;
                }
            }
        }

        private ArtifactContainer FindArtifactContainerFromCircle(SelectableEllipse circle)
        {
            foreach (ArtifactContainer container in artifactList)
            {
                if (container.Circle == circle)
                {
                    return container;
                }
            }
            return null;
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

        public void Draw()
        {
            XNA.PushMatrix();
            XNA.Translate(position);
            XNA.Scale(size);

            titleText.DrawFill();

            // draw the steam paths
            for (int i = 0; i < numSteam; i++)
            {
                if (steamPaths[i].Visible == false)
                {
                    continue;
                }

                XNA.PushMatrix();
                XNA.Translate(steamPaths[i].Blob.CenterPosition);
                XNA.RotateZ(steamPaths[i].Angle);
                steamPaths[i].Blob.DrawFillBorder(false);
                XNA.PopMatrix();
            }
            
            // draw the border around the blobs
            XNA.PushMatrix();
            XNA.Translate(borderCircle.CenterPosition);
            XNA.RotateZ((float)Math.PI / 2);
            borderCircle.DrawFillBorder(false);
            XNA.PopMatrix();

            lock (materialList)
            {
                // draw each blob
                foreach (MaterialContainer container in materialList)
                {
                    if (container.IsClickable == false && container.IsTransitioning == false)
                    {
                        continue;
                    }

                    XNA.PushMatrix();
                    XNA.Translate(container.Blob.CenterPosition);

                    // draw the blob
                    XNA.RotateZ(container.Angle);
                    container.Blob.DrawFillBorder(false);

                    // translate to the edge of the blob to draw the text
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

                    if (container.IsClickable == false && container.IsTransitioning == true)
                    {
                        // shrink the text if this blob is disappearing
                        XNA.Scale(1 - animationTimer.Elapsed, 1 - animationTimer.Elapsed, 1);
                    }
                    else if (container.IsClickable == true && container.IsTransitioning == true)
                    {
                        // grow the text if it's appearing
                        XNA.Scale(animationTimer.Elapsed, animationTimer.Elapsed, 1);
                    }

                    XNA.Translate(-container.Text.TextSize.X / 2, 0, 0);

                    container.Text.Draw();
                    XNA.PopMatrix();
                }
            }

            foreach (ArtifactContainer container in artifactList)
            {
                if (container.IsConstraint == false)
                {
                    continue;
                }

                XNA.PushMatrix();
                XNA.Translate(container.Circle.Position);
                container.Circle.DrawFillBorder(true);
                XNA.PopMatrix();
            }

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

            foreach (ArtifactContainer container in artifactList)
            {
                if (container.IsConstraint == false)
                {
                    continue;
                }

                XNA.PushMatrix();
                XNA.Translate(container.Circle.Position);
                container.Circle.DrawSelectable();
                XNA.PopMatrix();
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
                    float circleRadius = animationTimer.Elapsed * (materialList[i].EndKey.CircleRadius - materialList[i].StartKey.CircleRadius) + materialList[i].StartKey.CircleRadius;
                    float middleRadius = animationTimer.Elapsed * (materialList[i].EndKey.MiddleRadius - materialList[i].StartKey.MiddleRadius) + materialList[i].StartKey.MiddleRadius;
                    float spanAngle = animationTimer.Elapsed * (materialList[i].EndKey.SpanAngle - materialList[i].StartKey.SpanAngle) + materialList[i].StartKey.SpanAngle;
                    materialList[i].Blob.SetDimensions(spanAngle, circleRadius, middleRadius);
                    materialList[i].Angle = animationTimer.Elapsed * (materialList[i].EndKey.Angle - materialList[i].StartKey.Angle) + materialList[i].StartKey.Angle;

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
                    materialList[i].Angle = materialList[i].EndKey.Angle;
                    materialList[i].Blob.CircleRadius = materialList[i].EndKey.CircleRadius;
                    materialList[i].Blob.MiddleRadius = materialList[i].EndKey.MiddleRadius;
                    materialList[i].Blob.SpanAngle = materialList[i].EndKey.SpanAngle;
                    materialList[i].IsTransitioning = false; // no matter what, when the animation is finished, it's done transitioning
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
            materialConstraints.Clear();

            foreach (MaterialContainer materialContainer in materialList)
            {
                if (selectedArtifact.Materials.Contains(materialContainer.Material) == true)
                {
                    materialContainer.IsConstraint = true;
                    materialConstraints.Add(materialContainer.Material);
                }
            }

            UpdateBlobsFromConstraints();
            RepositionBlobs();
            UpdateArtifactsFromConstraints();
            RepositionArtifacts();
            animationTimer.Start();
        }

        private void Circle_TouchReleased(object sender, TouchArgs e)
        {
            if (bookshelf.TouchPoints.ContainsKey(e.TouchId) == false)
            {
                //*
                lock (artifactList)
                {
                    ArtifactContainer selectedContainer = FindArtifactContainerFromCircle((SelectableEllipse)sender);
                    if (selectedContainer == null)
                    {
                        return;
                    }

                    bookshelf.Library.SelectedArtifact = selectedContainer.Artifact;
                }
                //*/
            }
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

                    if (selectedContainer.IsConstraint == false)
                    {
                        // if this material is in the shell (not a constraint), put it in the center and add it to the constraint list
                        selectedContainer.IsConstraint = true;
                        materialConstraints.Add(selectedContainer.Material);
                    }
                    else
                    {
                        // if it's in the centre, then put it on the shell and remove it from constraints
                        selectedContainer.IsConstraint = false;
                        if (materialConstraints.Contains(selectedContainer.Material) == true)
                        {
                            materialConstraints.Remove(selectedContainer.Material);
                        }
                    }

                    UpdateBlobsFromConstraints();
                    RepositionBlobs();
                    UpdateArtifactsFromConstraints();
                    RepositionArtifacts();
                    animationTimer.Start();
                }
                //*/
            }
        }

    }
}
