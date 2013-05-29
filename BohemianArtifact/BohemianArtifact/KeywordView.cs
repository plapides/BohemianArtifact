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
        private static Random random = new Random();

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


        // a class that represents linear interpolation between points
        public class InterpolatePosition
        {
            public List<Vector3> finalPosition, initialPosition;
            public Vector3 finalPositionCircle, initialPositionCircle;

            public InterpolatePosition(List<Vector3> initPts, List<Vector3> finalPts, Vector3 initCirc, Vector3 finalCirc)
            {
                initialPosition = new List<Vector3>();
                initialPosition.AddRange(initPts);
                finalPosition = new List<Vector3>();
                finalPosition.AddRange(finalPts);
                finalPositionCircle = finalCirc;
                initialPositionCircle = initCirc;
            }

            // if delta = 1, will be at initial, if delta = 0, will be at final
            // kind of an odd interpretation but needed in order to make my life easier
            // to query the circle, pass in -1 for the index
            public Vector3 interpolate(int index, float delta)
            {
                Vector3 init, final;
                if (index < 0)
                {
                    init = initialPositionCircle;
                    final = finalPositionCircle;
                }
                else
                {
                    init = initialPosition[index];
                    final = finalPosition[index];
                }
                return delta * init + (1 - delta) * final;
            }
        }

        public class WavyText
        {
            public enum DisplayMode { Normal, GrowOutward, StraightLine, BounceBack, BounceBackFinished};

            private static Dictionary<char, SelectableText> TextDictionary = new Dictionary<char, SelectableText>();
            private static Color textColor = new Color(185, 185, 185);

            private static int NumLettersToFillSpace = 30; // a string with this many letters will fill the entire [0, 2pi] theta range
            private static float NumSecondsToCycle = 20f; // if waveSpeed is 1, it will take this long to go from 0 to 2pi (adjust using waveSpeed variable)
            private static float NumSecondsToMove = 10f; // if movementSpeed is 1, it will take this long to go from initialLineDirection to initialLineDirection + thetaRange
            private static float NumSecondsToScale = 8f; // if scaleSpeed is 1, it will take this long to move between height/distance targets
            private float drawScale = 0.0007f; // font size, as a percentage of the passed in font

            DisplayMode mode;

            bool insideOutText;

            Vector2 lineStart, lineDirection, initialLineDirection;
            float lineAngle;
            string text;

            double currentTheta;
            float waveSpeed; // how fast theta progresses along the timeline

            Vector2 dragPosition; // only used when someone is dragging a circle around

            // ranges that move during normal operation
            // height = height of the sin wave
            // scale = how compressed the letters are between each other
            // movement = controls how the end point of the text moves around
            // bendy = controls how many cycles of the sin wave get completed when drawing text
            FloatRange heightRange, scaleRange, movementRange, bendyRange;
            // ranges that move during special modes
            FloatRange growOutwardRange, bounceBackRange;

            double totalStraightTime; // keeps track of how much time the user has spent in straight mode, basically a timer for whether a release = bounce back or select
            static double TouchReleaseCutoff = 0.15;
            int numTimesToBounceBack = 4, bounceBackCount = 0;

            // assuming 0 = the final destination of the wave curve
            // start = how far past 0 (positive) do you want the curve to bounce, on the way back to the straight line?
            // end = how far past 0 (negative) do you want the curve to bounce, on the way away from the straight line?
            float bounceBackStart = 0.15f;
            float bounceBackEnd = 0.1f;
            float bounceBackTime = 0.2f; // the amount of time per bounce back cycle (so total time bouncing back is this * numTimes)
            InterpolatePosition bounceBackInterpolate;

            float maxCharWidth;

            SpriteFont font;
            List<SelectableText> letters;
            List<Vector3> letterPositions; // have to save these separately since letters contains references to a master list
            List<Vector3> oldLetterPositions; // during bounce back, we have to save where the letters should be trying to end up
            SelectableEllipse circle;
            Vector3 oldCirclePosition;

            public WavyText(Vector2 start, Vector2 direction, float height, string text)
                : this(start, direction, height, text, null)
            { }

            // pass in: the location of its anchor, the direction pointing away from the anchor you want the text to be drawn
            // the height of the sin wave, and the text/optional circle you want to display
            public WavyText(Vector2 start, Vector2 direction, float height, string text, SelectableEllipse circle)
            {
                lineStart = start;
                lineDirection = direction;
                lineDirection.Normalize();
                initialLineDirection = lineDirection;
                insideOutText = false;

                this.text = text;
                this.circle = circle;

                lineAngle = (float)Math.Acos(lineDirection.X) * Math.Sign(lineDirection.Y);
                waveSpeed = 1f;
                font = XNA.Font;

                assignRandomTheta();
                letters = new List<SelectableText>();
                letterPositions = new List<Vector3>();
                oldLetterPositions = new List<Vector3>();

                heightRange = new FloatRange(height, NumSecondsToScale, 1, 0.5f);
                scaleRange = new FloatRange(1, NumSecondsToScale, 1, 0.25f);
                movementRange = new FloatRange(0, NumSecondsToMove, 1, (float)(-Math.PI / 6), (float)(Math.PI / 6));
                bendyRange = new FloatRange(1, 5, 1, 0.25f);

                growOutwardRange = new FloatRange(0f, 0.5f, 1, 0, 1); // grow from 0 stretch to 1 stretch
                growOutwardRange.MovementDirection = 1; // grow in the positive direction
                growOutwardRange.AllowRandomSpeed = growOutwardRange.AllowRandomTarget = false; // make sure it grows at same rate
                growOutwardRange.ChangeDirectionEvent = new FloatRange.ChangeDirection(growOutwardFinished); // when it tries to change direction, we're done growing outward
                growOutwardRange.initialize(); // need to reinitialize because we changed some movement properties

                createLetters();
                positionLetters();
            }

            #region Getters and Setters (control how the wave operates)

            // how fast you want the text to progress along the wave (faster looks like the wind is blowing harder): a positive value where 1 is normal
            public float WaveSpeed
            {
                get { return waveSpeed; }
                set { waveSpeed = value; }
            }

            // how fast you want the text to bounce between ThetaRange off its initial direction: a positive value where 1 is normal
            public float MovementSpeed
            {
                get { return movementRange.MovementSpeed; }
                set { movementRange.MovementSpeed = value; }
            }

            // how fast you want the characters to stretch and contract along the StretchFactor: a positive value where 1 is normal
            public float StretchSpeed
            {
                get { return scaleRange.MovementSpeed; }
                set { scaleRange.MovementSpeed = value; }
            }

            // how much you want the characters to stretch (in a sense, controls the "length" of lineDirection vector): a positive value where 1 is normal
            public float StretchFactor
            {
                get { return scaleRange.InitialValue; }
                set { scaleRange.InitialValue = value; }
            }

            // the maximum value you want the lineDirection vector to perturb (in either direction) to indicate movement of the whole text, measured in radians
            // default is 30 degrees (pi / 6)
            public float ThetaRange
            {
                get { return movementRange.Max; }
                set
                {
                    value = Math.Abs(value);
                    movementRange.Min = -value;
                    movementRange.Max = value;
                }
            }

            public float DrawScale
            {
                get { return drawScale; }
                set { drawScale = value; }
            }

            public bool InsideOutText
            {
                get { return insideOutText; }
                set { insideOutText = value; }
            }

            public SelectableEllipse Circle
            {
                get { return circle; }
                set { circle = value; }
            }

            public Vector2 DragPosition
            {
                set { dragPosition = value; }
            }

            public bool FastRelease
            {
                get { return totalStraightTime <= TouchReleaseCutoff; }
            }

            public void resetMovement()
            {
                movementRange.initialize();
                scaleRange.initialize();
                heightRange.initialize();
            }

            public DisplayMode Mode
            {
                get { return mode; }
                set
                {
                    if (value == DisplayMode.StraightLine && mode == DisplayMode.Normal)
                    {
                        // save the old positions so we know how to bounce back to it
                        oldLetterPositions.Clear();
                        oldLetterPositions.AddRange(letterPositions);
                        oldCirclePosition = circle.Position;
                        totalStraightTime = 0;
                    }
                    else if (value == DisplayMode.BounceBack)
                    {
                        // bounceBackRange defines the stages of bouncing back
                        // let 1 = the position on the stretched line, and 0 = the final position, back on the curved wave before they dragged
                        // we'll be fluctuating this delta value so it appears to be interpolating between the line and the old curve
                        // we will start at 1 and try to move some distance past 0 into negative values, then rebound to some distance past 0 positive (but not as much as it started) and repeat
                        // bounceBackFinished event handles the cutting of the min/max until eventually we hit 0 and return to normal operation
                        bounceBackRange = new FloatRange(1, bounceBackTime, 1, -(1 + bounceBackEnd), (bounceBackStart + bounceBackEnd));
                        bounceBackRange.MovementDirection = -1; // grow in the negative direction
                        bounceBackRange.AllowRandomSpeed = bounceBackRange.AllowRandomTarget = false; // make sure it grows at same rate
                        bounceBackRange.ChangeDirectionEvent = new FloatRange.ChangeDirection(bounceBackFinished);
                        bounceBackRange.initialize();

                        // when bouncing back, we'll be interpolating between the straight line (ie, current positions) and the way the wavy text used to be (old positions)
                        bounceBackInterpolate = new InterpolatePosition(letterPositions, oldLetterPositions, circle.Position, oldCirclePosition);
                    }

                    mode = value;
                }
            }

            private void assignRandomTheta()
            {
                currentTheta = random.Next(360) * Math.PI / 180f;
            }

            #endregion

            #region Event Handlers

            public void growOutwardFinished()
            {
                mode = DisplayMode.Normal;
            }

            // this is the event handler for bouncing back
            public void bounceBackFinished()
            {
                bounceBackCount++;
                // the number of times (not counting initial value of 1) we will be stop on the negative and positive sides of 0
                int numPositiveBounce = numTimesToBounceBack / 2;
                int numNegativeBounce = (numTimesToBounceBack - 1) / 2;

                // we need to adjust the min range the first time it bounces back
                // this way it doesn't swing wildly back to the straight line, but rather closer to 0
                // basically we don't want to divide the space between [1, 0], but rather [some small value, 0]
                if (bounceBackCount == 1)
                    bounceBackRange.Min = -(bounceBackStart + bounceBackEnd);

                if (bounceBackCount >= numTimesToBounceBack + 1)
                {
                    // we're done; it bounced back all the times specified, and the math worked out so the extra time bounces it to 0
                    // (ie, 0 = the final resting place, the curve in its wavy form before it was dragged)
                    // then we can return to normal operation and everything lines up
                    bounceBackCount = 0;
                    mode = DisplayMode.BounceBackFinished;
                }
                else
                {
                    // else, we need to let it keep bouncing, but cut the min/max so it won't bounce back as far as last time
                    // since min and max are absolute (not relative) values (basically, how far should we move from where we currently are)
                    // we need to simply subtract some movement "percentage" value, rather than set a hard value
                    float v;
                    // divide the space between start or end into num + 1 pieces
                    if (bounceBackRange.MovementDirection > 0)
                        v = bounceBackStart / (numPositiveBounce + 1);
                    else
                        v = bounceBackEnd / (numNegativeBounce + 1);

                    // then it will move this much LESS the next time it bounces
                    bounceBackRange.Max -= v;
                    bounceBackRange.Min += v;
                }

                bounceBackRange.InitialValue = bounceBackRange.Value;
                bounceBackRange.initialize();
            }

            #endregion


            #region Base Letter Operations

            public static void createAllLetters(List<Artifact> allArtifacts)
            {
                foreach (Artifact a in allArtifacts)
                    foreach (char ch in a.ArticleName)
                        if(!TextDictionary.ContainsKey(ch))
                            TextDictionary.Add(ch, new SelectableText(XNA.Font, ch.ToString(), new Vector3(), textColor, textColor));
            }

            private void createLetters()
            {
                letters.Clear();
                foreach (char ch in text)
                {
                    if (!TextDictionary.ContainsKey(ch))
                        TextDictionary.Add(ch, new SelectableText(XNA.Font, ch.ToString(), new Vector3(), textColor, textColor));

                    letters.Add(TextDictionary[ch]);
                    letterPositions.Add(new Vector3()); // will position later
                }

                maxCharWidth = -1;
                foreach (SelectableText t in letters)
                    if (t.TextSize.X > maxCharWidth)
                        maxCharWidth = t.TextSize.X;
            }

            // positions the letters where they need to be, based on a sin wave and the various parameters that are bouncing around on timers
            // also subtly changes its behavior based on the different modes
            private void positionLetters()
            {
                if (mode == DisplayMode.BounceBack || mode == DisplayMode.BounceBackFinished)
                {
                    positionLettersBounceBack();
                    return;
                }

                double thetaDelta = 2 * Math.PI * bendyRange.Value / NumLettersToFillSpace;

                float growOutwardModifier = 1;
                // this variable will only come into play when growing outward, will contribute to stretch factor
                if (mode == DisplayMode.GrowOutward)
                    growOutwardModifier = growOutwardRange.Value;

                double theta = currentTheta;

                float x = 0;
                float xIncreasePerLetter = maxCharWidth * scaleRange.Value * growOutwardModifier * drawScale;
                // if trying to draw on a straight line, clamp the values a certain way to give a line, rather than call a specialized function
                if (mode == DisplayMode.StraightLine)
                {
                    theta = thetaDelta = 0; // sin(0) = 0, ie will be a straight line
                    float distance = (lineStart - dragPosition).Length();
                    xIncreasePerLetter = (distance - circle.Radius) * 0.95f / (text.Length - 1);
                    lineDirection = dragPosition - lineStart;
                    lineDirection.Normalize();
                    lineAngle = (float)Math.Acos(lineDirection.X) * Math.Sign(lineDirection.Y);
                }

                // ok, now simply run along the x-axis, incrementing x by a certain amount based on stretch factors, and evaluate the sin wave
                // then rotate it to be on the current line
                Vector2 pos;
                for (int i = 0; i < letters.Count; i++)
                {
                    pos = new Vector2(x, (float)evaluate(theta));
                    pos = rotateVector(pos, lineAngle);
                    pos += lineStart;

                    letterPositions[i] = new Vector3(pos, 0);

                    theta += thetaDelta;
                    x += xIncreasePerLetter;
                }

                // and do the same for the circle
                if (circle != null)
                {
                    x += 1 * xIncreasePerLetter + circle.Radius; // a little extra push away from the letters; some number of chars + its radius
                    pos = new Vector2(x, (float)evaluate(theta));
                    pos = rotateVector(pos, lineAngle);
                    pos += lineStart;

                    if (mode == DisplayMode.StraightLine)
                        circle.Position = new Vector3(dragPosition, 0);
                    else
                        circle.Position = new Vector3(pos, 0);
                }

                if (insideOutText)
                    letterPositions.Reverse();

            }

            private void positionLettersBounceBack()
            {
                // v is the delta value: 1 = fully the straight line, 0 = fully the old position before grabbing
                float v = bounceBackRange.Value;
                for (int i = 0; i < letters.Count; i++)
                    letterPositions[i] = bounceBackInterpolate.interpolate(i, v);

                circle.Position = bounceBackInterpolate.interpolate(-1, v);

                // needed to do one more call of this function to prevent a weird bug
                // but once the letters get positioned one last time, can return to normal just fine
                if (mode == DisplayMode.BounceBackFinished)
                    mode = DisplayMode.Normal;
            }

            #endregion

            #region Methods that Move the Text

            // gets the y-value on the sin wave at angle theta
            private double evaluate(double theta)
            {
                return heightRange.Value * Math.Sin(theta);
            }

            public void performTimestep(double deltaTime)
            {
                switch (mode)
                {
                    case DisplayMode.Normal:
                        currentTheta += 2 * Math.PI / ((NumSecondsToCycle * 60 / WaveSpeed) * (deltaTime * 60));
                        if (currentTheta >= 2 * Math.PI)
                            currentTheta -= 2 * Math.PI; // not sure this line is even needed, really, but keeps our theta variable in check

                        // make all our different parameters move a bit
                        heightRange.performTimestep(deltaTime);
                        scaleRange.performTimestep(deltaTime);
                        movementRange.performTimestep(deltaTime);
                        bendyRange.performTimestep(deltaTime);

                        // the other ranges get incorporated directly into positionLetters(), but movementRange has to change a bit more stuff first
                        perturbInitialDirectionVector((float)movementRange.Value);
                        break;

                    case DisplayMode.GrowOutward:
                        growOutwardRange.performTimestep(deltaTime);
                        break;
                        
                    case DisplayMode.StraightLine:
                        totalStraightTime += deltaTime;
                        break;

                    case DisplayMode.BounceBack:
                        bounceBackRange.performTimestep(deltaTime);
                        //Console.WriteLine("range: " + bounceBackRange.Value);
                        break;
                }
                positionLetters();
            }

            public void perturbInitialDirectionVector(float howMuchRad)
            {
                lineDirection = rotateVector(initialLineDirection, howMuchRad);
                lineAngle = (float)Math.Acos(lineDirection.X) * Math.Sign(lineDirection.Y);
            }

            public static Vector2 rotateVector(Vector2 vec, double theta)
            {
                return new Vector2((float)(vec.X * Math.Cos(theta) - vec.Y * Math.Sin(theta)), (float)(vec.X * Math.Sin(theta) + vec.Y * Math.Cos(theta)));
            }

            #endregion

            public void Draw()
            {
                for (int i = 0; i < letters.Count; i++)
                {
                    XNA.PushMatrix();
                    XNA.Translate(letterPositions[i]);
                    letters[i].DrawScale(drawScale);
                    XNA.PopMatrix();
                }

                if (circle != null)
                {
                    XNA.PushMatrix();
                    XNA.Translate(circle.Position);
                    circle.DrawFill(true);
                    XNA.PopMatrix();
                }
            }

            public void DrawSelectable()
            {
                if (circle != null)
                {
                    XNA.PushMatrix();
                    XNA.Translate(circle.Position);
                    circle.DrawSelectable();
                    XNA.PopMatrix();
                }
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

        private SelectableText titleText;
        private BohemianArtifact bookshelf;

        private List<KeywordContainer> artifactCircles;
        private const int MAX_CIRCLES = 9;
        private const int CENTER_CIRCLE_ID = MAX_CIRCLES - 1;
        private int numArtifactCircles = MAX_CIRCLES;

        private const float RELATED_TEXT_LABEL_SCALE = 0.0006f;
        private const float CENTER_TEXT_LABEL_SCALE = 0.0008f;

        private float centerCircleRadius;
        private float relatedCircleRadius;

        private Vector3 center = new Vector3(0.5f, 0.5f, 0);

        private List<WavyText> wavyText = new List<WavyText>();
        private WavyText draggedText;
        private int maxTextLength = 24; // -1 = no limit

        public KeywordView(BohemianArtifact bbshelf, Vector3 position, Vector3 size)
        {
            bookshelf = bbshelf;
            bookshelf.Library.SelectedArtifactChanged += new ArtifactLibrary.SelectedArtifactHandler(library_SelectedArtifactChanged);
            bookshelf.Library.LanguageChanged += new ArtifactLibrary.ChangeLanguageHandler(Library_LanguageChanged);
            this.position = position;
            this.size = size;
            draggedText = null;

            titleText = new SelectableText(XNA.Font, "Keywords", new Vector3(0.4f, 0, 0), bookshelf.GlobalTextColor, Color.White);
            titleText.InverseScale(0.8f, size.X, size.Y);

            WavyText.createAllLetters(bbshelf.Library.Artifacts);

            centerCircleRadius = 0.1f;
            relatedCircleRadius = centerCircleRadius * 0.5f;

            artifactCircles = new List<KeywordContainer>();
            for (int i = 0; i < MAX_CIRCLES; i++)
            {
                KeywordContainer newContainer = new KeywordContainer();
                newContainer.Circle = new SelectableEllipse(new Vector2(center.X, center.Y), relatedCircleRadius, centerCircleRadius * 0.05f, Color.White, new Color(1, 1, 1, 0), Color.Black, null);
                newContainer.Circle.TouchActivated += new TouchActivatedEventHandler(Circle_TouchActivated);
                newContainer.Circle.TouchReleased += new TouchReleaseEventHandler(Circle_TouchReleased);
                bookshelf.SelectableObjects.AddObject(newContainer.Circle);
                artifactCircles.Add(newContainer);
            }
            artifactCircles[CENTER_CIRCLE_ID].Circle.Radius = centerCircleRadius;
            PositionRelatedCircles();

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
                //// draw the related circles
                //for (int i = 0; i < numArtifactCircles; i++)
                //{
                //    XNA.PushMatrix();
                //    XNA.Translate(artifactCircles[i].Circle.Position);
                //    artifactCircles[i].Circle.DrawFillBorder(true);
                //    XNA.Translate(RELATED_TEXT_LABEL_SCALE * -artifactCircles[i].Artifact.Text.TextSize.X / 2, artifactCircles[i].Circle.Radius, 0);
                //    artifactCircles[i].Artifact.Text.DrawScale(RELATED_TEXT_LABEL_SCALE);
                //    XNA.PopMatrix();
                //}
                // draw the center circle
                XNA.PushMatrix();
                XNA.Translate(artifactCircles[CENTER_CIRCLE_ID].Circle.Position);
                artifactCircles[CENTER_CIRCLE_ID].Circle.DrawFillBorder(true);
                XNA.Translate(CENTER_TEXT_LABEL_SCALE * -artifactCircles[CENTER_CIRCLE_ID].Artifact.Text.TextSize.X / 2, artifactCircles[CENTER_CIRCLE_ID].Circle.Radius, 0);
                artifactCircles[CENTER_CIRCLE_ID].Artifact.Text.DrawScale(CENTER_TEXT_LABEL_SCALE);
                XNA.PopMatrix();
            }

            foreach (WavyText t in wavyText)
                t.Draw();

            //XNA.PushMatrix();
            //XNA.GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, wavelet, 0, 9);
            //XNA.PopMatrix();

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
                //XNA.PushMatrix();
                //XNA.Translate(artifactCircles[CENTER_CIRCLE_ID].Circle.Position);
                //artifactCircles[CENTER_CIRCLE_ID].Circle.DrawSelectable();
                //XNA.PopMatrix();
            }

            XNA.PopMatrix();
        }


        public void Update(GameTime time)
        {
            //animationTimer.Update(time.TotalGameTime.TotalSeconds);

            if (draggedText != null)
            {
                if (bookshelf.TouchPoints.ContainsKey(draggedText.Circle.TouchId))
                {
                    Touch touch = bookshelf.TouchPoints[draggedText.Circle.TouchId];
                    draggedText.DragPosition = convertTouchAbsoluteToRelative(new Vector2(touch.X, touch.Y));
                }
            }

            double timeElapsed = time.ElapsedGameTime.TotalSeconds;
            if (timeElapsed > 0)
                foreach (WavyText t in wavyText)
                    t.performTimestep(timeElapsed);
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

        private WavyText FindWavyTextFromCircle(SelectableEllipse circle)
        {
            foreach (WavyText t in wavyText)
                if (t.Circle == circle)
                    return t;
            return null;
        }

        void Circle_TouchActivated(object sender, TouchArgs e)
        {
            WavyText wave = FindWavyTextFromCircle(sender as SelectableEllipse);
            if (wave != null && wave != draggedText && bookshelf.TouchPoints.ContainsKey(e.TouchId))
            {
                draggedText = wave;
                Touch touch = bookshelf.TouchPoints[e.TouchId];
                wave.DragPosition = convertTouchAbsoluteToRelative(new Vector2(touch.X, touch.Y));
                wave.Mode = WavyText.DisplayMode.StraightLine;
            }
        }


        private void Circle_TouchReleased(object sender, TouchArgs e)
        {
            if (bookshelf.TouchPoints.ContainsKey(e.TouchId) == false)
            {
                bool fastRelease = true;
                if (draggedText != null)
                {
                    fastRelease = draggedText.FastRelease;
                    draggedText.Mode = WavyText.DisplayMode.BounceBack; // if we fast released, this object is getting wiped out anyway, otherwise bounce back
                }
                draggedText = null;
                if (fastRelease)
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

        // pass in an absolute (screen coordinate) x, y value and it will convert it to relative position (0 to 1) within keyword view's position/size
        public Vector2 convertTouchAbsoluteToRelative(Vector2 absolutePos)
        {
            absolutePos -= new Vector2(position.X, position.Y); // move everything to 0, 0
            // to clamp the drag to just the keyword view's bounds, uncomment the lines below
            //if (absolutePos.X < 0) absolutePos.X = 0;
            //if (absolutePos.X > size.X) absolutePos.X = size.X;
            //if (absolutePos.Y < 0) absolutePos.Y = 0;
            //if (absolutePos.Y > size.Y) absolutePos.Y = size.Y;

            return new Vector2(absolutePos.X / size.X, absolutePos.Y / size.Y);
        }

        private void library_SelectedArtifactChanged(Artifact selectedArtifact)
        {
            // change the central circle artifact and texture
            artifactCircles[CENTER_CIRCLE_ID].Artifact = selectedArtifact;
            artifactCircles[CENTER_CIRCLE_ID].Circle.Texture = selectedArtifact.Texture;

            Console.WriteLine("*** New selected artifact " + selectedArtifact.ArticleName + ", colors (" + selectedArtifact.Color.ToString() + ") and it's stems:");
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

            // create the wavy text

            wavyText.Clear();
            WavyText wave;
            for (i = 0; i < numArtifactCircles; i++)
            {
                float angleDeg = (float)(i * 360f / numArtifactCircles);
                angleDeg += 22.5f;
                float angleRad = (float)(angleDeg * Math.PI / 180);
                Vector2 direction = WavyText.rotateVector(Vector2.UnitX, angleRad);
                direction.Normalize();
                Vector2 startPos = new Vector2(artifactCircles[CENTER_CIRCLE_ID].Circle.Position.X, artifactCircles[CENTER_CIRCLE_ID].Circle.Position.Y);
                startPos += (artifactCircles[CENTER_CIRCLE_ID].Circle.Radius * 1.3f) * direction;

                float waveHeight = 0.03f;
                string text = artifactCircles[i].Artifact.ArticleName;
                if (maxTextLength > 0 && text.Length > maxTextLength)
                    text = text.Substring(0, maxTextLength - 3) + "...";
                wave = new WavyText(startPos, direction, waveHeight, text, artifactCircles[i].Circle);

                int textLength = text.Length;

                // make some lengths of text appear better on screen by shrinking the font size (DrawScale) and squishing/expanding the space between letters (StretchFactor)
                // DrawScale is absolute value so have to multiply by a percentage, StretchFactor is relative so just assign percentage directly
                if (textLength <= 5)
                {
                    wave.DrawScale *= 1.45f;
                    wave.StretchFactor = 1.25f;
                }
                else if (textLength <= 11)
                {
                    wave.DrawScale *= 1.25f;
                    wave.StretchFactor = 1.15f;
                }
                else if (textLength > 20)
                {
                    wave.DrawScale *= 0.85f;
                    wave.StretchFactor = 0.9f;
                }

                //wave.WaveSpeed = 1f;
                // if between 90 degrees and 270 degrees, flip the text
                if (angleDeg > 90 && angleDeg <= 270)
                    wave.InsideOutText = true;

                wave.resetMovement(); // only need to do if you manually change movement parameters after creating object
                wave.Mode = WavyText.DisplayMode.GrowOutward; // starts by growing outward, will change itself when it hits max
                wavyText.Add(wave);
            }

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

        void Library_LanguageChanged(int newLanguage)
        {
            library_SelectedArtifactChanged(bookshelf.Library.SelectedArtifact);
        }
    }
}
