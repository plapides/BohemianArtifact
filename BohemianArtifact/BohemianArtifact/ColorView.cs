using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace BohemianArtifact
{
    class ColorView : IViewBB
    {
        // represents a connected color wheel
        // each node knows how to go left, right, and closer and farther from the center of the color wheel
        // lets us query "neighboring" nodes easily so we can display some nearby items for perusal
        class ColorWheel
        {
            public static float Rotation = 90f; // the wheel can be rotated so red isn't at 3 o'clock

            ColorWheelNode[,] wheel;
            int numTheta, numR;
            SelectableQuad boundingQuad;

            // pass in how you want to discretize the tree
            public ColorWheel(int numTheta, int numR, List<Artifact> allArtifacts, SelectableQuad boundingQuad)
            {
                this.numTheta = numTheta;
                this.numR = numR;

                buildTree();
                classifyArtifacts(allArtifacts);
                this.boundingQuad = boundingQuad;
            }

            #region Tree Building

            public void buildTree()
            {
                wheel = new ColorWheelNode[numTheta, numR];

                for (int i = 0; i < numTheta; i++)
                    for (int j = 0; j < numR; j++)
                        // array starts with radius = 1 down to 0, theta at 0 to 360
                        wheel[i, j] = new ColorWheelNode((float)(1 - j) / (numR - 1), 360f * i / numTheta);
            }

            public void classifyArtifacts(List<Artifact> allArtifacts)
            {
                float thetaRange = (wheel[1, 0].Theta - wheel[0, 0].Theta) / 2;
                float rRange = (wheel[0, 1].R - wheel[0, 0].R) / 2;

                foreach (Artifact a in allArtifacts)
                {
                    int thetaIndex = -1, rIndex = -1;
                    float theta, r;
                    getWheelLocation(a, out theta, out r);
                    for (int i = 0; i < numTheta && thetaIndex == -1; i++)
                        if (thetaInBounds(theta, wheel[i, 0].Theta - thetaRange, wheel[i, 0].Theta + thetaRange))
                            thetaIndex = i;
                    for (int i = 0; i < numR && rIndex == -1; i++)
                        // our split doesn't work quite as well for R because it doesn't wrap around the space
                        // so if we're on our last R (the one that's supposed to cover all the way down to 0), we have to give it a boost
                        if (rInBounds(r, wheel[0, i].R - (i == numR - 1 ? 3 : 1) * rRange, wheel[0, i].R + rRange))
                            rIndex = i;

                    wheel[thetaIndex, rIndex].Artifacts.Add(a);
                }
            }

            private bool thetaInBounds(float theta, float lBound, float uBound)
            {
                return (lBound <= theta && uBound >= theta) || (lBound <= theta - 360 && uBound >= theta - 360);
            }

            private bool rInBounds(float r, float lBound, float uBound)
            {
                return lBound <= r && uBound >= r;
            }

            //private void createLeftRightLinks(ColorWheelNode[] ring)
            //{
            //    ring[0].Left = ring[ring.Length - 1];
            //    for(int i=0; i<ring.Length; i++)
            //    {
            //        if (i > 0)
            //            ring[i].Left = ring[i - 1];
            //        if (i < ring.Length - 1)
            //            ring[i].Right = ring[i + 1];
            //    }
            //    ring[ring.Length - 1].Right = ring[0];
            //}

            //private void createInnerOuterLinks(ColorWheelNode[] outer, ColorWheelNode[] ring, ColorWheelNode[] inner)
            //{
            //    for (int i = 0; i < ring.Length; i++)
            //    {
            //        ring[i].Outer = outer == null ? null : outer[i];
            //        ring[i].Inner = inner == null ? null : inner[i];
            //    }
            //}

            private void getWheelLocation(Artifact art, out float theta, out float r)
            {
                float v;
                // theta is hue, r is saturation
                RGBtoHSV(art.Color.R, art.Color.G, art.Color.B, out theta, out r, out v);
            }

            #endregion

            public void Draw()
            {
                float radius = boundingQuad.Center.X - boundingQuad.Top;


            }

        }

        class ColorWheelNode
        {
            public float R, Theta; // polar coordinates
            //public ColorWheelNode Left, Right, Inner, Outer; // left, right = lower, higher theta. inner, outer = smaller, larger radius
            public List<Artifact> Artifacts; // the artifacts we've judged to belong to this spot on the color wheel
            public ColorWheelNode(float r, float theta)
            {
                R = r;
                Theta = theta;
                Artifacts = new List<Artifact>();
            }
        }


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
        private SelectableText titleText;
        private BohemianArtifact bookshelf;

        List<SelectableEllipse> colorCircles;

        private BoundingBox boundingBox;

        private Vector3 center = new Vector3(0.5f, 0.5f, 0);

        private Timer animationTimer;

        public ColorView(BohemianArtifact bbshelf, Vector3 position, Vector3 size)
        {
            bookshelf = bbshelf;
            bookshelf.Library.SelectedArtifactChanged += new ArtifactLibrary.SelectedArtifactHandler(Library_SelectedArtifactChanged);
            this.position = position;
            this.size = size;
            font = bookshelf.Content.Load<SpriteFont>("Arial");

            titleText = new SelectableText(font, "Colours", new Vector3(0.4f, 0, 0), bookshelf.GlobalTextColor, Color.White);
            titleText.InverseScale(0.8f, size.X, size.Y);

            boundingBox = new BoundingBox(new Vector3(0, 0, 0), new Vector3(1, 1, 0), Color.Black, 0.005f);

            colorCircles = new List<SelectableEllipse>();
            //setupColorCircles();
            showColorWheel();
        }

        public void setupColorCircles()
        {
            Dictionary<Color, int> dict = new Dictionary<Color, int>();

            foreach (Artifact a in bookshelf.Library.Artifacts)
            {
                if (dict.ContainsKey(a.Color))
                    dict[a.Color]++;
                else
                    dict.Add(a.Color, 1);
            }
            
            List<KeyValuePair<Color, int>> list = dict.ToList();
            foreach (KeyValuePair<Color, int> kvp in list)
                colorCircles.Add(new SelectableEllipse(new Vector2(), 0.02f, 0, kvp.Key, kvp.Key, null));

            //alignCirclesLinear();
            alignCirclesPolar();

        }

        public void showColorWheel()
        {
            colorCircles.Clear();
            for (int i = 0; i < 360; i += 15)
            {
                byte r, g, b;
                HSVtoRGB(i, 1, 1, out r, out g, out b);
                colorCircles.Add(new SelectableEllipse(new Vector2(), 0.02f, 0, new Color(r, g, b), Color.Black, null));
                HSVtoRGB(i, 0.75f, 1, out r, out g, out b);
                colorCircles.Add(new SelectableEllipse(new Vector2(), 0.02f, 0, new Color(r, g, b), Color.Black, null));
                HSVtoRGB(i, 0.5f, 1, out r, out g, out b);
                colorCircles.Add(new SelectableEllipse(new Vector2(), 0.02f, 0, new Color(r, g, b), Color.Black, null));
                HSVtoRGB(i, 0.25f, 1, out r, out g, out b);
                colorCircles.Add(new SelectableEllipse(new Vector2(), 0.02f, 0, new Color(r, g, b), Color.Black, null));
            }
            alignCirclesPolar();
        }

        public void alignCirclesLinear()
        {
            float dx = colorCircles[0].Radius;
            float dy = 0.15f;

            foreach (SelectableEllipse se in colorCircles)
            {
                se.Position = new Vector3(dx, dy, 0);
                dx += se.Radius * 2;
                if (dx > 1 - se.Radius)
                {
                    dx = colorCircles[0].Radius;
                    dy += se.Radius * 2;
                }
            }
        }

        public void alignCirclesPolar()
        {
            Vector3 wheelCenter = new Vector3(0.5f, 0.5f, 0);
            float radius = 0.4f;
            float rotate = 90f; // in degrees, rotate the color wheel (normally, red = 0 degrees)

            foreach (SelectableEllipse se in colorCircles)
            {
                float h, s, v;
                RGBtoHSV(se.Color.R, se.Color.G, se.Color.B, out h, out s, out v);
                double theta = (h + rotate) * Math.PI / 180.0f;
                float r = s;

                Vector3 unitCirclePos = new Vector3(r * (float)Math.Cos(theta), r * (float)Math.Sin(theta), 0);
                se.Position = unitCirclePos * radius + wheelCenter;
            }
        }

        // h in [0, 360), s in [0, 1], v in [0, 1]
        // r, g, b in [0, 255]
        public void HSVtoRGB(float h, float s, float v, out byte b_r, out byte b_g, out byte b_b)
        {
            int i;
            float f, p, q, t;
            float r, g, b; // temporarily save it as [0,1] float

            if (s < 0.00001f) // if s == 0 
            {
                // achromatic (grey)
                b_r = b_g = b_b = (byte)Math.Round(255 * v);
                return;
            }

            h /= 60;			// sector 0 to 5
            i = (int)Math.Floor(h);
            f = h - i;			// factorial part of h
            p = v * (1 - s);
            q = v * (1 - s * f);
            t = v * (1 - s * (1 - f));

            switch (i)
            {
                case 0:
                    r = v; g = t; b = p;
                    break;
                case 1:
                    r = q; g = v; b = p;
                    break;
                case 2:
                    r = p; g = v; b = t;
                    break;
                case 3:
                    r = p; g = q; b = v;
                    break;
                case 4:
                    r = t; g = p; b = v;
                    break;
                default:		// case 5:
                    r = v; g = p; b = q;
                    break;
            }
            // r,g,b are now correct in [0,1] range, convert to [0,255]
            b_r = (byte)Math.Round(255 * r);
            b_g = (byte)Math.Round(255 * g);
            b_b = (byte)Math.Round(255 * b);

        }

        // h in [0, 360), s in [0, 1], v in [0, 1]
        // r, g, b in [0, 255]
        public static void RGBtoHSV(byte b_r, byte b_g, byte b_b, out float h, out float s, out float v)
        {
            float min, max, delta;
            float r = b_r / 255f, g = b_g / 255f, b = b_b / 255f;

            min = Math.Min(r, Math.Min(g, b));
            max = Math.Max(r, Math.Max(g, b));
            v = max; // set V here

            delta = max - min;

            if (max != 0)
                s = delta / max; // set S here
            else
            {
                // R, G, and B are all 0 (black), so S and H are arbitrarily set and we finish
                s = h = 0;
                return;
            }

            if (r == max)
                h = (g - b) / delta;		// between yellow & magenta
            else if (g == max)
                h = 2 + (b - r) / delta;	// between cyan & yellow
            else
                h = 4 + (r - g) / delta;	// between magenta & cyan

            h *= 60;				// degrees
            if (h < 0)
                h += 360;
        }

        public void Update(GameTime time)
        {
            //animationTimer.Update(time.TotalGameTime.TotalSeconds);

        }

        void Library_SelectedArtifactChanged(Artifact selectedArtifact)
        {
            //throw new NotImplementedException();
        }

        public void Draw()
        {
            XNA.PushMatrix();
            XNA.Translate(position);
            XNA.Scale(size);

            titleText.DrawFill();
            boundingBox.Draw();

            foreach (SelectableEllipse se in colorCircles)
            {
                XNA.PushMatrix();
                XNA.Translate(se.Position);
                se.DrawFill(false);
                XNA.PopMatrix();
            }

            XNA.PopMatrix();
        }

        public void DrawSelectable()
        {
            XNA.PushMatrix();
            XNA.Translate(position);
            XNA.Scale(size);




            XNA.PopMatrix();
        }
    }
}