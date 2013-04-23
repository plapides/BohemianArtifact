using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.IO;

namespace BohemianArtifact
{
    public class TimelineView : IViewBB
    {
        public class TimelineContainer
        {
            public SelectableEllipse Circle;
            public Artifact Artifact;
            public VagueDate Manufacture;
            public VagueDate Use;
            public VagueDate Catalog;
            public SelectableCurve TopCurve;
            public SelectableCurve BottomCurve;
            public SelectableLine TopLine;
            public SelectableLine BottomLine;
            public bool Selected = false;
            public bool Related = false;

            public override bool Equals(object obj)
            {
                return (obj == Artifact);
            }
            public override int GetHashCode()
            {
                return base.GetHashCode();
            }
        }
        public class TimelineYearTick
        {
            public SelectableText TickText;
            public float Year;

            public TimelineYearTick(float year)
            {
                Year = year;
                TickText = new SelectableText(XNA.Font, year.ToString(), Vector3.Zero, Color.Gray);
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

        // this is the box on the sides of the timeline
        private VertexPositionColor[] leftWhiteVertices;
        private VertexPositionColor[] rightWhiteVertices;
        private int[] hexWhiteIndices;
        private VertexPositionColor[] leftBlackVertices;
        private VertexPositionColor[] rightBlackVertices;
        private int[] quadBlackIndices;

        private SpriteFont font;
        private SelectableText titleText;
        private BohemianArtifact bookshelf;

        private const float MAX_HEIGHT = 0.3f;

        // set vertical positions of each of the timelines and the line thickness
        private float catalogLineHeight = (1 / 8.0f) * MAX_HEIGHT;
        private float useLineHeight = (4 / 8.0f) * MAX_HEIGHT;
        private float manufactureLineHeight = (7 / 8.0f) * MAX_HEIGHT;
        private float lineThickness = 0.075f * MAX_HEIGHT;

        // lines that are rendered to show the linelines
        private SelectableLine catalogLine;
        private SelectableLine useLine;
        private SelectableLine manufactureLine;

        // widgets
        private SelectableLine[] catalogWidgets;
        private SelectableLine[] useWidgets;
        private SelectableLine[] manufactureWidgets;
        private const int LEFT_WIDGET = 0;
        private const int MID_WIDGET = 1;
        private const int RIGHT_WIDGET = 2;
        private int widgetTouchId = Touch.NO_ID;

        // stuff to handle the timeline ranges
        float catalogMinYear = 10000;
        float catalogMaxYear = -10000;
        float useMinYear = 10000;
        float useMaxYear = -10000;
        float manuMinYear = 10000;
        float manuMaxYear = -10000;
        private SelectableEllipse[] catalogCircles, useCircles, manufactureCircles;
        private float[] catalogTimelineRange, useTimelineRange, manufactureTimelineRange;
        private const int L_RANGE = 0;
        private const int L_OFFSET = 1;
        private const int R_RANGE = 2;
        private const int R_OFFSET = 3;

        // tick marks
        private List<TimelineYearTick> catalogTicks;
        private List<TimelineYearTick> useTicks;
        private List<TimelineYearTick> manufactureTicks;
        private float catalogTickSkipScale;
        private float useTickSkipScale;
        private float manufactureTickSkipScale;

        private List<TimelineContainer> timelineArtifactList;
        private Dictionary<SelectableLine, TimelineContainer> lineArtifactDictionary;
        private TimelineContainer selectedContainer;
        private TimelineContainer highlightedContainer;
        private List<TimelineContainer> relatedContainers;

        private const float ALPHA_RELATED = 0.2f;
        private const float ALPHA_SELECTED = 0.7f;
        private const float TEXT_LABEL_SCALE = 0.0003f;

        private List<KeyValuePair<TimelineContainer, float>> topSorted;
        private List<KeyValuePair<TimelineContainer, float>> bottomSorted;

        private Timer animationTimer;

        public TimelineView(BohemianArtifact bbshelf, Vector3 position, Vector3 size)
        {
            bookshelf = bbshelf;
            bookshelf.Library.SelectedArtifactChanged += new ArtifactLibrary.SelectedArtifactHandler(library_SelectedArtifactChanged);
            this.position = position;
            this.size = size;
            font = bookshelf.Content.Load<SpriteFont>("Arial");
            
            // create left and right white fadeouts
            leftWhiteVertices = new VertexPositionColor[6];
            leftWhiteVertices[0] = new VertexPositionColor(new Vector3(-2, 0, 0), Color.White);
            leftWhiteVertices[1] = new VertexPositionColor(new Vector3(0, 0, 0), Color.White);
            leftWhiteVertices[2] = new VertexPositionColor(new Vector3(0.005f, 0, 0), Color.Transparent);
            leftWhiteVertices[3] = new VertexPositionColor(new Vector3(0.005f, 1, 0), Color.Transparent);
            leftWhiteVertices[4] = new VertexPositionColor(new Vector3(0, 1, 0), Color.White);
            leftWhiteVertices[5] = new VertexPositionColor(new Vector3(-2, 1, 0), Color.White);
            rightWhiteVertices = new VertexPositionColor[6];
            rightWhiteVertices[0] = new VertexPositionColor(new Vector3(1 - 0.005f, 0, 0), Color.Transparent);
            rightWhiteVertices[1] = new VertexPositionColor(new Vector3(1, 0, 0), Color.White);
            rightWhiteVertices[2] = new VertexPositionColor(new Vector3(3, 0, 0), Color.White);
            rightWhiteVertices[3] = new VertexPositionColor(new Vector3(3, 1, 0), Color.White);
            rightWhiteVertices[4] = new VertexPositionColor(new Vector3(1, 1, 0), Color.White);
            rightWhiteVertices[5] = new VertexPositionColor(new Vector3(1 - 0.005f, 1, 0), Color.Transparent);
            hexWhiteIndices = new int[12];
            hexWhiteIndices[0] = 0;
            hexWhiteIndices[1] = 1;
            hexWhiteIndices[2] = 4;
            hexWhiteIndices[3] = 4;
            hexWhiteIndices[4] = 5;
            hexWhiteIndices[5] = 0;
            hexWhiteIndices[6] = 1;
            hexWhiteIndices[7] = 2;
            hexWhiteIndices[8] = 3;
            hexWhiteIndices[9] = 3;
            hexWhiteIndices[10] = 4;
            hexWhiteIndices[11] = 1;
            // create left and right black quads
            leftBlackVertices = new VertexPositionColor[4];
            leftBlackVertices[0] = new VertexPositionColor(new Vector3(-2, 0, 0), Color.Black);
            leftBlackVertices[1] = new VertexPositionColor(new Vector3(0, 0, 0), Color.Black);
            leftBlackVertices[2] = new VertexPositionColor(new Vector3(0, 1, 0), Color.Black);
            leftBlackVertices[3] = new VertexPositionColor(new Vector3(-2, 1, 0), Color.Black);
            rightBlackVertices = new VertexPositionColor[4];
            rightBlackVertices[0] = new VertexPositionColor(new Vector3(1, 0, 0), Color.Black);
            rightBlackVertices[1] = new VertexPositionColor(new Vector3(3, 0, 0), Color.Black);
            rightBlackVertices[2] = new VertexPositionColor(new Vector3(3, 1, 0), Color.Black);
            rightBlackVertices[3] = new VertexPositionColor(new Vector3(1, 1, 0), Color.Black);
            quadBlackIndices = new int[6];
            quadBlackIndices[0] = 0;
            quadBlackIndices[1] = 1;
            quadBlackIndices[2] = 2;
            quadBlackIndices[3] = 2;
            quadBlackIndices[4] = 3;
            quadBlackIndices[5] = 0;

            titleText = new SelectableText(font, "Timeline", new Vector3(0.4f, 0, 0), bookshelf.GlobalTextColor, Color.White);
            titleText.InverseScale(0.8f, size.X, size.Y);

            // arrays to hold tick marks for each timeline
            catalogTicks = new List<TimelineYearTick>();
            useTicks = new List<TimelineYearTick>();
            manufactureTicks = new List<TimelineYearTick>();

            // create a line for each timeline, these will not be interactive elements
            catalogLine = new SelectableLine(new Vector3(0, catalogLineHeight, 0), new Vector3(1, catalogLineHeight, 0), Color.LightGray, lineThickness);
            useLine = new SelectableLine(new Vector3(0, useLineHeight, 0), new Vector3(1, useLineHeight, 0), Color.LightGray, lineThickness);
            manufactureLine = new SelectableLine(new Vector3(0, manufactureLineHeight, 0), new Vector3(1, manufactureLineHeight, 0), Color.LightGray, lineThickness);

            // underneath each timeline, there are three widgets to control zooming and traversing
            catalogWidgets = new SelectableLine[3];
            useWidgets = new SelectableLine[3];
            manufactureWidgets = new SelectableLine[3];
            for (int i = 0; i < 3; i++)
            {
                // create interactive elements and add them to the object manager
                catalogWidgets[i] = new SelectableLine(new Vector3((float)i / 3, catalogLineHeight, 0), new Vector3((float)(i + 1) / 3, catalogLineHeight, 0), new Color(i * 255 / 2, 0, 0), lineThickness * 2);
                useWidgets[i] = new SelectableLine(new Vector3((float)i / 3, useLineHeight, 0), new Vector3((float)(i + 1) / 3, useLineHeight, 0), new Color(0, i * 255 / 2, 0), lineThickness * 2);
                manufactureWidgets[i] = new SelectableLine(new Vector3((float)i / 3, manufactureLineHeight, 0), new Vector3((float)(i + 1) / 3, manufactureLineHeight, 0), new Color(0, 0, i * 255 / 2), lineThickness * 2);
                catalogWidgets[i].TouchActivated += new TouchActivatedEventHandler(TimelineView_TouchActivated);
                useWidgets[i].TouchActivated += new TouchActivatedEventHandler(TimelineView_TouchActivated);
                manufactureWidgets[i].TouchActivated += new TouchActivatedEventHandler(TimelineView_TouchActivated);
                bookshelf.SelectableObjects.AddObject(catalogWidgets[i]);
                bookshelf.SelectableObjects.AddObject(useWidgets[i]);
                bookshelf.SelectableObjects.AddObject(manufactureWidgets[i]);
            }

            // the circles are test objects to show how the timeline range is being manipulated
            catalogCircles = new SelectableEllipse[2];
            useCircles = new SelectableEllipse[2];
            manufactureCircles = new SelectableEllipse[2];
            // these arrays store the left/right for each timeline, along with a left/right offset that is used as a temp variable
            // the order is: L_RANGE, L_OFFSET, R_RANGE, R_OFFSET
            catalogTimelineRange = new float[4] { 0, 0, 1, 1 };
            useTimelineRange = new float[4] { 0, 0, 1, 1 };
            manufactureTimelineRange = new float[4] { 0, 0, 1, 1 };
            catalogCircles[0] = new SelectableEllipse(Vector2.Zero, 0.01f, 0.001f, Color.Orange, Color.Black, null);
            catalogCircles[1] = new SelectableEllipse(Vector2.Zero, 0.01f, 0.001f, Color.Pink, Color.Black, null);
            useCircles[0] = new SelectableEllipse(Vector2.Zero, 0.01f, 0.001f, Color.Orange, Color.Black, null);
            useCircles[1] = new SelectableEllipse(Vector2.Zero, 0.01f, 0.001f, Color.Pink, Color.Black, null);
            manufactureCircles[0] = new SelectableEllipse(Vector2.Zero, 0.01f, 0.001f, Color.Orange, Color.Black, null);
            manufactureCircles[1] = new SelectableEllipse(Vector2.Zero, 0.01f, 0.001f, Color.Pink, Color.Black, null);

            topSorted = new List<KeyValuePair<TimelineContainer, float>>();
            bottomSorted = new List<KeyValuePair<TimelineContainer, float>>();

            timelineArtifactList = new List<TimelineContainer>();
            lineArtifactDictionary = new Dictionary<SelectableLine, TimelineContainer>();
            selectedContainer = null;
            highlightedContainer = null;
            relatedContainers = new List<TimelineContainer>();
            InitializeTimelineList();

            animationTimer = new Timer(0.5f);
            animationTimer.FinishEvent += new TimerFinished(AnimationTimerFinished);
        }

        private void InitializeTimelineList()
        {
            catalogMinYear = 10000;
            catalogMaxYear = -10000;
            useMinYear = 10000;
            useMaxYear = -10000;
            manuMinYear = 10000;
            manuMaxYear = -10000;

            foreach (Artifact artifact in bookshelf.Library.Artifacts)
            {
                TimelineContainer newContainer = new TimelineContainer();
                newContainer.Artifact = artifact;

                Color diminishedColor = artifact.Color;
                diminishedColor.A = (byte)(ALPHA_RELATED * 255);

                newContainer.TopCurve = new SelectableCurve(diminishedColor);
                newContainer.BottomCurve = new SelectableCurve(diminishedColor);
                bookshelf.SelectableObjects.AddObject(newContainer.TopCurve);
                bookshelf.SelectableObjects.AddObject(newContainer.BottomCurve);

                float selectableLineThickness = 0.0075f;
                newContainer.TopLine = new SelectableLine(new Vector3(0, catalogLineHeight + lineThickness / 2, 0), new Vector3(0, useLineHeight - lineThickness / 2, 0), Color.LightGray, selectableLineThickness);
                newContainer.TopLine.TouchActivated += new TouchActivatedEventHandler(Line_TouchActivated);
                newContainer.TopLine.TouchReleased += new TouchReleaseEventHandler(Line_TouchReleased);
                newContainer.BottomLine = new SelectableLine(new Vector3(0, useLineHeight + lineThickness / 2, 0), new Vector3(0, manufactureLineHeight - lineThickness / 2, 0), Color.LightGray, selectableLineThickness);
                newContainer.BottomLine.TouchActivated += new TouchActivatedEventHandler(Line_TouchActivated);
                newContainer.BottomLine.TouchReleased += new TouchReleaseEventHandler(Line_TouchReleased);
                //newContainer.BottomLine.TouchActivated += new TouchActivatedEventHandler(Line_TouchActivated);
                bookshelf.SelectableObjects.AddObject(newContainer.TopLine);
                bookshelf.SelectableObjects.AddObject(newContainer.BottomLine);

                float circleRadius = 0.02f;
                newContainer.Circle = new SelectableEllipse(Vector2.Zero, circleRadius, circleRadius * 0.05f, Color.White, new Color(1, 1, 1, 0), Color.Black, artifact.Texture);

                timelineArtifactList.Add(newContainer);
                lineArtifactDictionary.Add(newContainer.TopLine, newContainer);
                lineArtifactDictionary.Add(newContainer.BottomLine, newContainer);

                /*
                // don't bother checking the qualifier of the catalog date. we assume they are exact
                // check usedate qualifier
                switch (artifact.UseDate.DateQualifier)
                {
                    case VagueDate.Qualifier.Before:
                        newContainer.TopCurve.BottomLeftAlpha = 0;
                        newContainer.BottomCurve.TopLeftAlpha = 0;
                        break;
                    case VagueDate.Qualifier.After:
                        newContainer.TopCurve.BottomRightAlpha = 0;
                        newContainer.BottomCurve.TopRightAlpha = 0;
                        break;
                    case VagueDate.Qualifier.Circa:
                        //newContainer.TopCurve.BottomLeftAlpha = 0;
                        //newContainer.TopCurve.BottomRightAlpha = 0;
                        //newContainer.BottomCurve.TopLeftAlpha = 0;
                        //newContainer.BottomCurve.TopRightAlpha = 0;
                        break;
                }
                switch (artifact.ManufactureDate.DateQualifier)
                {
                    case VagueDate.Qualifier.Before:
                        newContainer.BottomCurve.BottomLeftAlpha = 0;
                        break;
                    case VagueDate.Qualifier.After:
                        newContainer.BottomCurve.BottomRightAlpha = 0;
                        break;
                    case VagueDate.Qualifier.Circa:
                        //newContainer.BottomCurve.BottomLeftAlpha = 0;
                        //newContainer.BottomCurve.BottomRightAlpha = 0;
                        break;
                }
                //*/

                float useLength = newContainer.Artifact.UseDate.EndYear + newContainer.Artifact.UseDate.EndError - newContainer.Artifact.UseDate.StartYear - newContainer.Artifact.UseDate.StartError;
                float manuLength = newContainer.Artifact.ManufactureDate.EndYear + newContainer.Artifact.ManufactureDate.EndError - newContainer.Artifact.ManufactureDate.StartYear - newContainer.Artifact.ManufactureDate.StartError;
                topSorted.Add(new KeyValuePair<TimelineContainer, float>(newContainer, useLength));
                bottomSorted.Add(new KeyValuePair<TimelineContainer, float>(newContainer, (useLength + manuLength) / 2));

                // update catalog year limits
                if (artifact.CatalogDate.StartYear < catalogMinYear)
                {
                    catalogMinYear = artifact.CatalogDate.StartYear;
                }
                if (catalogMaxYear < artifact.CatalogDate.EndYear)
                {
                    catalogMaxYear = artifact.CatalogDate.EndYear;
                }
                // update manu year limits
                if (artifact.ManufactureDate.StartYear < manuMinYear)
                {
                    manuMinYear = artifact.ManufactureDate.StartYear;
                }
                if (manuMaxYear < artifact.ManufactureDate.EndYear)
                {
                    manuMaxYear = artifact.ManufactureDate.EndYear;
                }
                // update use year limits
                if (artifact.UseDate.StartYear < useMinYear)
                {
                    useMinYear = artifact.UseDate.StartYear;
                }
                if (useMaxYear < artifact.UseDate.EndYear)
                {
                    useMaxYear = artifact.UseDate.EndYear;
                }
            }

            topSorted.Sort(CompareTimelineArea);
            bottomSorted.Sort(CompareTimelineArea);

            // initialize the catalog timeline ticks
            catalogMinYear = ((int)catalogMinYear / 10) * 10; // round down to the nearest 10
            catalogMaxYear = ((int)catalogMaxYear / 10 + 1) * 10; // round UP to the nearest 10
            for (int i = (int)catalogMinYear; i <= catalogMaxYear; i++)
            {
                TimelineYearTick catalogTick = new TimelineYearTick(i);
                catalogTick.TickText.InverseScale(0.35f, size.X, size.Y);
                catalogTicks.Add(catalogTick);
            }
            catalogTickSkipScale = ComputeTickScale(catalogTicks.Count, 40, 80, 200);

            // initialize the use timeline ticks
            useMinYear = ((int)useMinYear / 10) * 10; // round down to the nearest 10
            useMaxYear = ((int)useMaxYear / 10 + 1) * 10; // round UP to the nearest 10
            for (int i = (int)useMinYear; i <= useMaxYear; i++)
            {
                TimelineYearTick useTick = new TimelineYearTick(i);
                useTick.TickText.InverseScale(0.35f, size.X, size.Y);
                useTicks.Add(useTick);
            }
            useTickSkipScale = ComputeTickScale(useTicks.Count, 40, 80, 200);
            
            // manufacture ticks
            manuMinYear = ((int)manuMinYear / 10) * 10; // round down to the nearest 10
            manuMaxYear = ((int)manuMaxYear / 10 + 1) * 10; // round UP to the nearest 10
            for (int i = (int)manuMinYear; i <= manuMaxYear; i++)
            {
                TimelineYearTick manuTick = new TimelineYearTick(i);
                manuTick.TickText.InverseScale(0.35f, size.X, size.Y);
                //manuTick.TickText.Alpha = 0.1f;
                manufactureTicks.Add(manuTick);
            }
            manufactureTickSkipScale = ComputeTickScale(manufactureTicks.Count, 40, 80, 200);

            UpdateTopCurves();
            UpdateBottomCurves();
        }

        private void TimelineView_TouchActivated(object sender, TouchArgs e)
        {
            if (bookshelf.TouchPoints.ContainsKey(e.TouchId) == true)
            {
                if (bookshelf.TouchPoints[e.TouchId].OriginObject == sender)
                {
                    // only set the widget id if the touch in question originated on the widget
                    // e.g. if we happened to drag our finger over top of the widget, do nothing
                    widgetTouchId = e.TouchId;
                }
            }
        }

        void Line_TouchReleased(object sender, TouchArgs e)
        {
            if (widgetTouchId != Touch.NO_ID)
            {
                // get out of here if we are touching the widgets
                return;
            }

            if (bookshelf.TouchPoints.ContainsKey(e.TouchId) == false)
            {
                // this touch release was caused by a touch up event, so the user has "SELECTED" a certain artifact
                SelectableLine line = (SelectableLine)sender;
                if (lineArtifactDictionary.ContainsKey(line) == true)
                {
                    TimelineContainer container = lineArtifactDictionary[line];
                    bookshelf.Library.SelectedArtifact = container.Artifact;
                }
            }
            else
            {
                // this touch release was caused by the user scrubbing over this line
                // start the fade timer
                highlightedContainer = null;
            }
        }

        private void Line_TouchActivated(object sender, TouchArgs e)
        {
            if (widgetTouchId != Touch.NO_ID)
            {
                // get out of here if we are touching the widgets
                return;
            }

            SelectableLine line = (SelectableLine)sender;
            if (lineArtifactDictionary.ContainsKey(line) == true)
            {
                // container is only highlighted if we are not using the timeline widgets
                highlightedContainer = lineArtifactDictionary[line];
            }

            //Console.WriteLine("line was touched with id " + e.TouchId);
            //SelectableLine line = (SelectableLine)sender;
            //line.Color = Color.Black;
            //line.Selected = true;
        }

        private int CompareTimelineArea(KeyValuePair<TimelineContainer, float> a, KeyValuePair<TimelineContainer, float> b)
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


        public void Draw()
        {
            XNA.PushMatrix();
            XNA.Translate(position);
            XNA.Scale(size);

            titleText.DrawFill();

            catalogLine.DrawThick();
            useLine.DrawThick();
            manufactureLine.DrawThick();

            // catalog line left and right ranges
            /*
            XNA.PushMatrix();
            XNA.Translate(catalogTimelineRange[L_RANGE], catalogLine.LinePoints[0].Position.Y, 0);
            catalogCircles[0].DrawFillBorder();
            XNA.PopMatrix();
            XNA.PushMatrix();
            XNA.Translate(catalogTimelineRange[R_RANGE], catalogLine.LinePoints[0].Position.Y, 0);
            catalogCircles[1].DrawFillBorder();
            XNA.PopMatrix();

            // use line ranges
            XNA.PushMatrix();
            XNA.Translate(useTimelineRange[L_RANGE], useLine.LinePoints[0].Position.Y, 0);
            useCircles[0].DrawFillBorder();
            XNA.PopMatrix();
            XNA.PushMatrix();
            XNA.Translate(useTimelineRange[R_RANGE], useLine.LinePoints[0].Position.Y, 0);
            useCircles[1].DrawFillBorder();
            XNA.PopMatrix();

            // manufacture line ranges
            XNA.PushMatrix();
            XNA.Translate(manufactureTimelineRange[L_RANGE], manufactureLine.LinePoints[0].Position.Y, 0);
            manufactureCircles[0].DrawFillBorder();
            XNA.PopMatrix();
            XNA.PushMatrix();
            XNA.Translate(manufactureTimelineRange[R_RANGE], manufactureLine.LinePoints[0].Position.Y, 0);
            manufactureCircles[1].DrawFillBorder();
            XNA.PopMatrix();
            //*/

            //foreach (TimelineContainer container in timelineArtifactList)
            //{
            //    container.TopCurve.Draw();
            //    container.BottomCurve.Draw();
            //}

            XNA.GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            //XNA.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            foreach (TimelineContainer container in timelineArtifactList)
            {
                if (container.Selected == true || container.Related == true)
                {
                    // if the container is selected or related we should skip it since we will draw it after the lines so it will be on top
                    continue;
                }

                if (container.TopLine.Selected == true || container.BottomLine.Selected == true)
                {
                    // if we are not touching a widget, and either the top line or bottom line are being selected, then draw the curve
                    //container.TopCurve.Draw();
                    //container.BottomCurve.Draw();
                }
                else
                {
                }
                container.TopLine.Draw();
                container.BottomLine.Draw();
            }
            // draw the selected container
            selectedContainer.TopCurve.Draw();
            selectedContainer.BottomCurve.Draw();
            XNA.PushMatrix();
            XNA.Translate(selectedContainer.Circle.Position);
            selectedContainer.Circle.DrawFillBorder(true);
            XNA.RotateZ(-(float)Math.PI / 4);
            XNA.Translate(selectedContainer.Circle.Radius * 1.05f, -selectedContainer.Artifact.Text.TextSize.Y * TEXT_LABEL_SCALE / 2, 0);
            selectedContainer.Artifact.Text.DrawScale(TEXT_LABEL_SCALE);
            XNA.PopMatrix();

            // draw the highlighted container
            if (highlightedContainer != null && highlightedContainer != selectedContainer)
            {
                // only draw if there is something highlighted and it's not the same as the selectedContainer
                highlightedContainer.TopCurve.Draw();
                highlightedContainer.BottomCurve.Draw();
                XNA.PushMatrix();
                XNA.Translate(highlightedContainer.Circle.Position);
                highlightedContainer.Circle.DrawFillBorder(true);
                XNA.RotateZ(-(float)Math.PI / 4);
                XNA.Translate(highlightedContainer.Circle.Radius * 1.05f, -highlightedContainer.Artifact.Text.TextSize.Y * TEXT_LABEL_SCALE / 2, 0);
                highlightedContainer.Artifact.Text.DrawScale(TEXT_LABEL_SCALE);
                XNA.PopMatrix();
            }
            // draw the related containers
            foreach (TimelineContainer relatedContainer in relatedContainers)
            {
                relatedContainer.TopCurve.Draw();
                relatedContainer.BottomCurve.Draw();
            }

            XNA.GraphicsDevice.BlendState = BlendState.AlphaBlend;
            // draw catalog timeline tick marks
            for (int i = 0; i < catalogTicks.Count; i += (int)catalogTickSkipScale)
            {
                // get the X position by interpolating i / icks.count between L_RANGE and R_RANGE
                float xPosition = i * (catalogTimelineRange[R_RANGE] - catalogTimelineRange[L_RANGE]) / catalogTicks.Count + catalogTimelineRange[L_RANGE];
                if (xPosition < -0.05f || 1.05f < xPosition)
                {
                    // don't draw ticks when they're not going to be seen
                    continue;
                }
                XNA.PushMatrix();
                XNA.Translate(xPosition, catalogLine.LinePoints[0].Position.Y, 0);
                XNA.RotateZ(-(float)Math.PI / 5);
                catalogTicks[i].TickText.Draw();
                XNA.PopMatrix();
            }

            // draw use timeline tick marks
            for (int i = 0; i < useTicks.Count; i += (int)useTickSkipScale)
            {
                // get the X position by interpolating i / icks.count between L_RANGE and R_RANGE
                float xPosition = i * (useTimelineRange[R_RANGE] - useTimelineRange[L_RANGE]) / useTicks.Count + useTimelineRange[L_RANGE];
                if (xPosition < -0.05f || 1.05f < xPosition)
                {
                    // don't draw ticks when they're not going to be seen
                    continue;
                }
                XNA.PushMatrix();
                XNA.Translate(xPosition, useLine.LinePoints[0].Position.Y, 0);
                XNA.RotateZ(-(float)Math.PI / 5);
                useTicks[i].TickText.Draw();
                XNA.PopMatrix();
            }

            // draw manufacture timeline tick marks
            for (int i = 0; i < manufactureTicks.Count; i += (int)manufactureTickSkipScale)
            {
                // get the X position by interpolating i / icks.count between L_RANGE and R_RANGE
                float xPosition = i * (manufactureTimelineRange[R_RANGE] - manufactureTimelineRange[L_RANGE]) / manufactureTicks.Count + manufactureTimelineRange[L_RANGE];
                if (xPosition < -0.05f || 1.05f < xPosition)
                {
                    // don't draw ticks when they're not going to be seen
                    continue;
                }
                XNA.PushMatrix();
                XNA.Translate(xPosition, manufactureLine.LinePoints[0].Position.Y, 0);
                XNA.RotateZ(-(float)Math.PI / 5);
                manufactureTicks[i].TickText.Draw();
                XNA.PopMatrix();
            }

            XNA.Texturing = false;
            XNA.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, leftWhiteVertices, 0, 6, hexWhiteIndices, 0, 4);
            XNA.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, rightWhiteVertices, 0, 6, hexWhiteIndices, 0, 4);

            XNA.PopMatrix();
        }

        public void DrawSelectable()
        {
            XNA.PushMatrix();
            XNA.Translate(position);
            XNA.Scale(size);

            for (int i = 0; i < 3; i++)
            {
                catalogWidgets[i].DrawSelectable();
                useWidgets[i].DrawSelectable();
                manufactureWidgets[i].DrawSelectable();
            }

            foreach (TimelineContainer container in timelineArtifactList)
            {
                container.TopLine.DrawSelectable();
                container.BottomLine.DrawSelectable();
            }

            XNA.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, leftBlackVertices, 0, 4, quadBlackIndices, 0, 2);
            XNA.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, rightBlackVertices, 0, 4, quadBlackIndices, 0, 2);

            XNA.PopMatrix();
        }

        private void AnimationTimerFinished()
        {
        }

        private void UpdateTopCurves()
        {
            //UpdateTopLines();
            foreach (TimelineContainer container in timelineArtifactList)
            {
                float topLeft, topRight, bottomLeft = 0, bottomRight = 0;
                // we are assuming that all catalog dates are exact
                topLeft = (catalogTimelineRange[R_RANGE] - catalogTimelineRange[L_RANGE]) * (container.Artifact.CatalogDate.StartYear - catalogMinYear) / (catalogMaxYear - catalogMinYear) + catalogTimelineRange[L_RANGE];
                topRight = topLeft;
                switch (container.Artifact.UseDate.DateQualifier)
                {
                    case VagueDate.Qualifier.Exact:
                        bottomLeft = (useTimelineRange[R_RANGE] - useTimelineRange[L_RANGE]) * (container.Artifact.UseDate.StartYear - useMinYear) / (useMaxYear - useMinYear) + useTimelineRange[L_RANGE];
                        bottomRight = bottomLeft;
                        break;
                    case VagueDate.Qualifier.Circa:
                        bottomLeft = (useTimelineRange[R_RANGE] - useTimelineRange[L_RANGE]) * (container.Artifact.UseDate.StartYear - 10 - useMinYear) / (useMaxYear - useMinYear) + useTimelineRange[L_RANGE];
                        bottomRight = (useTimelineRange[R_RANGE] - useTimelineRange[L_RANGE]) * (container.Artifact.UseDate.StartYear + 10 - useMinYear) / (useMaxYear - useMinYear) + useTimelineRange[L_RANGE];
                        break;
                    case VagueDate.Qualifier.After:
                        bottomLeft = (useTimelineRange[R_RANGE] - useTimelineRange[L_RANGE]) * (container.Artifact.UseDate.StartYear - useMinYear) / (useMaxYear - useMinYear) + useTimelineRange[L_RANGE];
                        bottomRight = (useTimelineRange[R_RANGE] - useTimelineRange[L_RANGE]) * (container.Artifact.UseDate.StartYear + 10 - useMinYear) / (useMaxYear - useMinYear) + useTimelineRange[L_RANGE];
                        break;
                    case VagueDate.Qualifier.Before:
                        bottomLeft = (useTimelineRange[R_RANGE] - useTimelineRange[L_RANGE]) * (container.Artifact.UseDate.StartYear - 10 - useMinYear) / (useMaxYear - useMinYear) + useTimelineRange[L_RANGE];
                        bottomRight = (useTimelineRange[R_RANGE] - useTimelineRange[L_RANGE]) * (container.Artifact.UseDate.EndYear - useMinYear) / (useMaxYear - useMinYear) + useTimelineRange[L_RANGE];
                        break;
                    case VagueDate.Qualifier.Between:
                        bottomLeft = (useTimelineRange[R_RANGE] - useTimelineRange[L_RANGE]) * (container.Artifact.UseDate.StartYear - useMinYear) / (useMaxYear - useMinYear) + useTimelineRange[L_RANGE];
                        bottomRight = (useTimelineRange[R_RANGE] - useTimelineRange[L_RANGE]) * (container.Artifact.UseDate.EndYear - useMinYear) / (useMaxYear - useMinYear) + useTimelineRange[L_RANGE];
                        break;
                }
                //bottomLeft = (useTimelineRange[R_RANGE] - useTimelineRange[L_RANGE]) * (container.Artifact.UseDate.StartYear - useMinYear) / (useMaxYear - useMinYear) + useTimelineRange[L_RANGE];
                //bottomRight = bottomLeft + 0.005f;
                container.TopCurve.Recompute(catalogLineHeight + lineThickness / 2, topLeft, topRight, useLineHeight - lineThickness / 2, bottomLeft, bottomRight);
                container.TopLine.LinePoints[0].Position.X = topLeft;
                container.TopLine.LinePoints[1].Position.X = bottomLeft;
                container.TopLine.Recompute();
                container.Circle.Position = new Vector3(topLeft, catalogLineHeight - lineThickness / 2, 0);
            }
        }
        
        private void UpdateBottomCurves()
        {
            //UpdateBottomLines();
            foreach (TimelineContainer container in timelineArtifactList)
            {
                float topLeft = 0, topRight = 0, bottomLeft = 0, bottomRight = 0;
                switch (container.Artifact.UseDate.DateQualifier)
                {
                    case VagueDate.Qualifier.Exact:
                        topLeft = (useTimelineRange[R_RANGE] - useTimelineRange[L_RANGE]) * (container.Artifact.UseDate.StartYear - useMinYear) / (useMaxYear - useMinYear) + useTimelineRange[L_RANGE];
                        topRight = topLeft;
                        break;
                    case VagueDate.Qualifier.Circa:
                        topLeft = (useTimelineRange[R_RANGE] - useTimelineRange[L_RANGE]) * (container.Artifact.UseDate.StartYear - 10 - useMinYear) / (useMaxYear - useMinYear) + useTimelineRange[L_RANGE];
                        topRight = (useTimelineRange[R_RANGE] - useTimelineRange[L_RANGE]) * (container.Artifact.UseDate.StartYear + 10 - useMinYear) / (useMaxYear - useMinYear) + useTimelineRange[L_RANGE];
                        break;
                    case VagueDate.Qualifier.After:
                        topLeft = (useTimelineRange[R_RANGE] - useTimelineRange[L_RANGE]) * (container.Artifact.UseDate.StartYear - useMinYear) / (useMaxYear - useMinYear) + useTimelineRange[L_RANGE];
                        topRight = (useTimelineRange[R_RANGE] - useTimelineRange[L_RANGE]) * (container.Artifact.UseDate.StartYear + 10 - useMinYear) / (useMaxYear - useMinYear) + useTimelineRange[L_RANGE];
                        break;
                    case VagueDate.Qualifier.Before:
                        topLeft = (useTimelineRange[R_RANGE] - useTimelineRange[L_RANGE]) * (container.Artifact.UseDate.StartYear - 10 - useMinYear) / (useMaxYear - useMinYear) + useTimelineRange[L_RANGE];
                        topRight = (useTimelineRange[R_RANGE] - useTimelineRange[L_RANGE]) * (container.Artifact.UseDate.EndYear - useMinYear) / (useMaxYear - useMinYear) + useTimelineRange[L_RANGE];
                        break;
                    case VagueDate.Qualifier.Between:
                        topLeft = (useTimelineRange[R_RANGE] - useTimelineRange[L_RANGE]) * (container.Artifact.UseDate.StartYear - useMinYear) / (useMaxYear - useMinYear) + useTimelineRange[L_RANGE];
                        topRight = (useTimelineRange[R_RANGE] - useTimelineRange[L_RANGE]) * (container.Artifact.UseDate.EndYear - useMinYear) / (useMaxYear - useMinYear) + useTimelineRange[L_RANGE];
                        break;
                }
                switch (container.Artifact.ManufactureDate.DateQualifier)
                {
                    case VagueDate.Qualifier.Exact:
                        bottomLeft = (manufactureTimelineRange[R_RANGE] - manufactureTimelineRange[L_RANGE]) * (container.Artifact.ManufactureDate.StartYear - manuMinYear) / (manuMaxYear - manuMinYear) + manufactureTimelineRange[L_RANGE];
                        bottomRight = bottomLeft;
                        break;
                    case VagueDate.Qualifier.Circa:
                        bottomLeft = (manufactureTimelineRange[R_RANGE] - manufactureTimelineRange[L_RANGE]) * (container.Artifact.ManufactureDate.StartYear - 10 - manuMinYear) / (manuMaxYear - manuMinYear) + manufactureTimelineRange[L_RANGE];
                        bottomRight = (manufactureTimelineRange[R_RANGE] - manufactureTimelineRange[L_RANGE]) * (container.Artifact.ManufactureDate.StartYear + 10 - manuMinYear) / (manuMaxYear - manuMinYear) + manufactureTimelineRange[L_RANGE];
                        break;
                    case VagueDate.Qualifier.After:
                        bottomLeft = (manufactureTimelineRange[R_RANGE] - manufactureTimelineRange[L_RANGE]) * (container.Artifact.ManufactureDate.StartYear - manuMinYear) / (manuMaxYear - manuMinYear) + manufactureTimelineRange[L_RANGE];
                        bottomRight = (manufactureTimelineRange[R_RANGE] - manufactureTimelineRange[L_RANGE]) * (container.Artifact.ManufactureDate.StartYear + 10 - manuMinYear) / (manuMaxYear - manuMinYear) + manufactureTimelineRange[L_RANGE];
                        break;
                    case VagueDate.Qualifier.Before:
                        bottomLeft = (manufactureTimelineRange[R_RANGE] - manufactureTimelineRange[L_RANGE]) * (container.Artifact.ManufactureDate.StartYear - 10 - manuMinYear) / (manuMaxYear - manuMinYear) + manufactureTimelineRange[L_RANGE];
                        bottomRight = (manufactureTimelineRange[R_RANGE] - manufactureTimelineRange[L_RANGE]) * (container.Artifact.ManufactureDate.EndYear - manuMinYear) / (manuMaxYear - manuMinYear) + manufactureTimelineRange[L_RANGE];
                        break;
                    case VagueDate.Qualifier.Between:
                        bottomLeft = (manufactureTimelineRange[R_RANGE] - manufactureTimelineRange[L_RANGE]) * (container.Artifact.ManufactureDate.StartYear - manuMinYear) / (manuMaxYear - manuMinYear) + manufactureTimelineRange[L_RANGE];
                        bottomRight = (manufactureTimelineRange[R_RANGE] - manufactureTimelineRange[L_RANGE]) * (container.Artifact.ManufactureDate.EndYear - manuMinYear) / (manuMaxYear - manuMinYear) + manufactureTimelineRange[L_RANGE];
                        break;
                }

                //topLeft = (useTimelineRange[R_RANGE] - useTimelineRange[L_RANGE]) * (container.Artifact.UseDate.StartYear - useMinYear) / (useMaxYear - useMinYear) + useTimelineRange[L_RANGE];
                //topRight = topLeft + 0.005f;
                //bottomLeft = (manufactureTimelineRange[R_RANGE] - manufactureTimelineRange[L_RANGE]) * (container.Artifact.ManufactureDate.StartYear - manuMinYear) / (manuMaxYear - manuMinYear) + manufactureTimelineRange[L_RANGE];
                //bottomRight = bottomLeft + 0.005f;
                container.BottomCurve.Recompute(useLineHeight + lineThickness / 2, topLeft, topRight, manufactureLineHeight - lineThickness / 2, bottomLeft, bottomRight);
                container.BottomLine.LinePoints[0].Position.X = topLeft;
                container.BottomLine.LinePoints[1].Position.X = bottomLeft;
                container.BottomLine.Recompute();
            }
        }

        public void Update(GameTime time)
        {
            animationTimer.Update(time.TotalGameTime.TotalSeconds);

            if (bookshelf.TouchPoints.ContainsKey(widgetTouchId) == true)
            {
                // if this touch currently exists, then move the timelines around
                HandleTimelineWidgetTouch(bookshelf.TouchPoints[widgetTouchId]);
            }
            else
            {
                // otherwise there is no current widget touch
                widgetTouchId = Touch.NO_ID;
                UpdateTimelineRanges();
            }
        }

        private void HandleTimelineWidgetTouch(Touch touch)
        {
            /*
             * The timeline interaction code and statemachine is a little bit complex and has several conditional checks for each widget that constraint the timeline
             * Each timeline has three widgets, LEFT_WIDGET, MID_WIDGET, RIGHT_WIDGET.
             * LEFT and RIGHT _WIDGET control the left and right zoom and operate symmetrically - the equations are nearly the same and their state machine is identical.

             * The constraints ensure that the L_RANGE should not be greater than 0, and the R_RANGE should not be less than 1.
             * This constraint ensures that the timeline range is never out of bounds, inverted (e.g. R_RANGE less than L_RANGE), etc.
             * The timeline works on a single touch interaction. Whenever the user slides their finger along the timeline, something should happen.
             * Either it should zoom or it should slide.
             * 
             * Here is how the LEFT_WIDGET operates (from here on out called L_W).
             *     In the constraint condition, when L_RANGE = 0:
             * (1) - if the user drags L_W to the left, the timeline should "zoom in" by moving L_RANGE to the left.
             * (2) - if the user drags L_W to the right, it should zoom the other way, moving R_RANGE to the right.
             *     In the arbitrary state (L_RANGE < 0):
             * (3) - if the user drags L_W either left or right, timeline should zoom between L_RANGE and 1 (not between L_RANGE & R_RANGE)
             *       in other words, if year 2000 is at the very rightmost edge of the timeline (at position 1), year 2000 should stay at that position.
             *       This means that R_RANGE has to be moved along with L_RANGE. The trivial situation is if R_RANGE = 1, then it stays where it is.
             *       Bug if 1 < R_RANGE, then it has to be moved to keep 2000 at position 1, following this equation that can be derived without much difficulty:
             *       Let L & R be the old L_RANGE and R_RANGE values. _L & _R are the new L_RANGE and R_RANGE values. In this case, we are solving for _R, all others are known.
             *       Let x = (1 - L) / (R - L) be the proportion of the timeline that is between L and 1 (the year at 1 should be clamped)
             *       When L_RANGE moves, this proportion should stay the same, so that x = (1 - _L) / (_R - _L) giving:
             *       _R = _L + (R - L) * (1 - _L) / (1 - L);
             * (4) - there is a special condition when L_RANGE starts < 0 and reaches its constraint L_RANGE = 0.
             *       In this case, the timeline should start zooming by moving R_RANGE to the right. There is a minor bug related to (touch.X - touch.OriginX)
             *       that causes R_RANGE to suddenly jump when this condition is reached. If we add catalogTimelineRange[L_OFFSET] to the R_RANGE, then it behaves smoothly.
             *       
             * The behavior of RIGHT_WIDGET is identical to L_W. The equation in (3) is slightly different but the derivation is identical.
             * Condition (4) is also changed. We must add (catalogTimelineRange[R_OFFSET] - 1) to ensure continuity of L_RANGE.
             * 
             * MIDDLE_WIDGET has simple behavior because it changes L_RANGE and R_RANGE by the same values causing the timeline to slide.
             * If L_RANGE or R_RANGE reach their constraint, they are simply clamped to either 0 or 1 but the other value continues to move, creating the situation of zooming.
            */

            float L, R; // old L & R range values
            float _L, _R; // new L & R range values

            // Catalog widget
            // copy the current RANGE values, these are now the "old" values
            L = catalogTimelineRange[L_RANGE];
            R = catalogTimelineRange[R_RANGE];
            // Only LEFT_WIDGET behavior is documented, RIGHT_WIDGET has symmetrical behavior.
            if (touch.OriginObject == catalogWidgets[LEFT_WIDGET])
            {
                // compute new L range
                _L = catalogTimelineRange[L_OFFSET] + (touch.X - touch.OriginX) / size.X;
                if (0 < _L)
                {
                    // L_RANGE can't be greater than 0. this handles state (2) and (4)
                    _L = 0;
                    _R = catalogTimelineRange[R_OFFSET] + (touch.X - touch.OriginX) / size.X + catalogTimelineRange[L_OFFSET];
                    if (_R < 1)
                    {
                        // sanity check, make SURE that R_RANGE is not less than 1.
                        _R = 1;
                    }
                }
                else
                {
                    // state (3)
                    _R = _L + (R - L) * (1 - _L) / (1 - L);
                }
                // copy the new RANGE values to the array
                catalogTimelineRange[L_RANGE] = _L;
                catalogTimelineRange[R_RANGE] = _R;
                UpdateTopCurves();
            }
            else if (touch.OriginObject == catalogWidgets[RIGHT_WIDGET])
            {
                _R = catalogTimelineRange[R_OFFSET] + (touch.X - touch.OriginX) / size.X;
                if (_R < 1)
                {
                    _R = 1;
                    _L = catalogTimelineRange[L_OFFSET] + (touch.X - touch.OriginX) / size.X + (catalogTimelineRange[R_OFFSET] - 1);
                    if (0 < _L)
                    {
                        _L = 0;
                    }
                }
                else
                {
                    _L = _R - _R * (R - L) / R;
                }
                catalogTimelineRange[L_RANGE] = _L;
                catalogTimelineRange[R_RANGE] = _R;
                UpdateTopCurves();
            }
            else if (touch.OriginObject == catalogWidgets[MID_WIDGET])
            {
                // move L_RANGE and R_RANGE by the same amount
                _L = catalogTimelineRange[L_OFFSET] + (touch.X - touch.OriginX) / size.X;
                _R = catalogTimelineRange[R_OFFSET] + (touch.X - touch.OriginX) / size.X;
                // check the constraint on each
                if (0 < _L)
                {
                    _L = 0;
                }
                if (_R < 1)
                {
                    _R = 1;
                }
                catalogTimelineRange[L_RANGE] = _L;
                catalogTimelineRange[R_RANGE] = _R;
                UpdateTopCurves();
            }
            // update tick scale
            catalogTickSkipScale = ComputeTickScale(catalogTicks.Count / (catalogTimelineRange[R_RANGE] - catalogTimelineRange[L_RANGE]), 40, 80, 200);

            // use widgets
            L = useTimelineRange[L_RANGE];
            R = useTimelineRange[R_RANGE];
            if (touch.OriginObject == useWidgets[LEFT_WIDGET])
            {
                _L = useTimelineRange[L_OFFSET] + (touch.X - touch.OriginX) / size.X;
                if (0 < _L)
                {
                    _L = 0;
                    _R = useTimelineRange[R_OFFSET] + (touch.X - touch.OriginX) / size.X + useTimelineRange[L_OFFSET];
                    if (_R < 1)
                    {
                        _R = 1;
                    }
                }
                else
                {
                    _R = _L + (R - L) * (1 - _L) / (1 - L);
                }
                useTimelineRange[L_RANGE] = _L;
                useTimelineRange[R_RANGE] = _R;
                UpdateTopCurves();
                UpdateBottomCurves();
            }
            else if (touch.OriginObject == useWidgets[RIGHT_WIDGET])
            {
                _R = useTimelineRange[R_OFFSET] + (touch.X - touch.OriginX) / size.X;
                if (_R < 1)
                {
                    _R = 1;
                    _L = useTimelineRange[L_OFFSET] + (touch.X - touch.OriginX) / size.X + (useTimelineRange[R_OFFSET] - 1);
                    if (0 < _L)
                    {
                        _L = 0;
                    }
                }
                else
                {
                    _L = _R - _R * (R - L) / R;
                }
                useTimelineRange[L_RANGE] = _L;
                useTimelineRange[R_RANGE] = _R;
                UpdateTopCurves();
                UpdateBottomCurves();
            }
            else if (touch.OriginObject == useWidgets[MID_WIDGET])
            {
                _L = useTimelineRange[L_OFFSET] + (touch.X - touch.OriginX) / size.X;
                _R = useTimelineRange[R_OFFSET] + (touch.X - touch.OriginX) / size.X;
                if (0 < _L)
                {
                    _L = 0;
                }
                if (_R < 1)
                {
                    _R = 1;
                }
                useTimelineRange[L_RANGE] = _L;
                useTimelineRange[R_RANGE] = _R;
                UpdateTopCurves();
                UpdateBottomCurves();
            }
            useTickSkipScale = ComputeTickScale(useTicks.Count / (useTimelineRange[R_RANGE] - useTimelineRange[L_RANGE]), 40, 80, 200);

            // manufacture widgets
            L = manufactureTimelineRange[L_RANGE];
            R = manufactureTimelineRange[R_RANGE];
            if (touch.OriginObject == manufactureWidgets[LEFT_WIDGET])
            {
                _L = manufactureTimelineRange[L_OFFSET] + (touch.X - touch.OriginX) / size.X;
                if (0 < _L)
                {
                    _L = 0;
                    _R = manufactureTimelineRange[R_OFFSET] + (touch.X - touch.OriginX) / size.X + manufactureTimelineRange[L_OFFSET];
                    if (_R < 1)
                    {
                        _R = 1;
                    }
                }
                else
                {
                    _R = _L + (R - L) * (1 - _L) / (1 - L);
                }
                manufactureTimelineRange[L_RANGE] = _L;
                manufactureTimelineRange[R_RANGE] = _R;
                UpdateBottomCurves();
            }
            else if (touch.OriginObject == manufactureWidgets[RIGHT_WIDGET])
            {
                _R = manufactureTimelineRange[R_OFFSET] + (touch.X - touch.OriginX) / size.X;
                if (_R < 1)
                {
                    _R = 1;
                    _L = manufactureTimelineRange[L_OFFSET] + (touch.X - touch.OriginX) / size.X + (manufactureTimelineRange[R_OFFSET] - 1);
                    if (0 < _L)
                    {
                        _L = 0;
                    }
                }
                else
                {
                    _L = _R - _R * (R - L) / R;
                }
                manufactureTimelineRange[L_RANGE] = _L;
                manufactureTimelineRange[R_RANGE] = _R;
                UpdateBottomCurves();
            }
            else if (touch.OriginObject == manufactureWidgets[MID_WIDGET])
            {
                _L = manufactureTimelineRange[L_OFFSET] + (touch.X - touch.OriginX) / size.X;
                _R = manufactureTimelineRange[R_OFFSET] + (touch.X - touch.OriginX) / size.X;
                if (0 < _L)
                {
                    _L = 0;
                }
                if (_R < 1)
                {
                    _R = 1;
                }
                manufactureTimelineRange[L_RANGE] = _L;
                manufactureTimelineRange[R_RANGE] = _R;
                UpdateBottomCurves();
            }
            manufactureTickSkipScale = ComputeTickScale(manufactureTicks.Count / (manufactureTimelineRange[R_RANGE] - manufactureTimelineRange[L_RANGE]), 40, 80, 200);
        }

        private void UpdateTimelineRanges()
        {
            // copy the current RANGE values of each timeline to the OFFSET values for use in the next iteration
            catalogTimelineRange[L_OFFSET] = catalogTimelineRange[L_RANGE];
            catalogTimelineRange[R_OFFSET] = catalogTimelineRange[R_RANGE];
            useTimelineRange[L_OFFSET] = useTimelineRange[L_RANGE];
            useTimelineRange[R_OFFSET] = useTimelineRange[R_RANGE];
            manufactureTimelineRange[L_OFFSET] = manufactureTimelineRange[L_RANGE];
            manufactureTimelineRange[R_OFFSET] = manufactureTimelineRange[R_RANGE];
        }

        private float ComputeTickScale(float density, float t1, float t2, float t3)
        {
            float tickScale;
            if (t3 < density)
            {
                tickScale = 10;
            }
            else if (t2 < density)
            {
                tickScale = 5 + (density - t2) / (t3 - t2);
            }
            else if (t1 < density && density < t2)
            {
                tickScale = 1 + density / t2;
            }
            else
            {
                tickScale = 1;
            }
            return tickScale;
        }

        private void library_SelectedArtifactChanged(Artifact selectedArtifact)
        {
            TimelineContainer newContainer = null;
            // change the selected artifact
            foreach (TimelineContainer container in timelineArtifactList)
            {
                if (container.Artifact == selectedArtifact)
                {
                    newContainer = container;
                    break;
                }
            }
            if (newContainer == null)
            {
                return;
            }

            if (selectedContainer != null)
            {
                // deselect the current selected artifact
                selectedContainer.Selected = false;
                selectedContainer.TopCurve.Alpha = ALPHA_RELATED;
                selectedContainer.BottomCurve.Alpha = ALPHA_RELATED;
            }

            // update the selected container
            selectedContainer = newContainer;

            // and change it's status to being selected
            selectedContainer.Selected = true;
            selectedContainer.TopCurve.Alpha = ALPHA_SELECTED;
            selectedContainer.BottomCurve.Alpha = ALPHA_SELECTED;

            // since something was just selected, nothing is highlighted
            highlightedContainer = null;
        }
    }
}
