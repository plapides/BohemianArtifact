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
        class Container
        {
            public SelectableEllipse Ellipse;
            public Artifact Artifact;
            public Container(SelectableEllipse se, Artifact a)
            {
                Ellipse = se;
                Artifact = a;
            }
        }

        // represents a connected color wheel
        // each node knows how to go left, right, and closer and farther from the center of the color wheel
        // lets us query "neighboring" nodes easily so we can display some nearby items for perusal
        //class ColorWheel
        //{
        //    public static float Rotation = 90f; // the wheel can be rotated so red isn't at 3 o'clock

        //    ColorWheelNode[,] wheel;
        //    int numTheta, numR;
        //    SelectableQuad boundingQuad;

        //    // pass in how you want to discretize the tree
        //    public ColorWheel(int numTheta, int numR, List<Artifact> allArtifacts, SelectableQuad boundingQuad)
        //    {
        //        this.numTheta = numTheta;
        //        this.numR = numR;
        //        this.boundingQuad = boundingQuad;

        //        buildTree();
        //        classifyArtifacts(allArtifacts);
        //        determineCircles();
        //    }

        //    #region Color Conversion Algorithms

        //    // h in [0, 360), s in [0, 1], v in [0, 1]
        //    // r, g, b in [0, 255]
        //    public void HSVtoRGB(float h, float s, float v, out byte b_r, out byte b_g, out byte b_b)
        //    {
        //        int i;
        //        float f, p, q, t;
        //        float r, g, b; // temporarily save it as [0,1] float

        //        if (s < 0.00001f) // if s == 0 
        //        {
        //            // achromatic (grey)
        //            b_r = b_g = b_b = (byte)Math.Round(255 * v);
        //            return;
        //        }

        //        h /= 60;			// sector 0 to 5
        //        i = (int)Math.Floor(h);
        //        f = h - i;			// factorial part of h
        //        p = v * (1 - s);
        //        q = v * (1 - s * f);
        //        t = v * (1 - s * (1 - f));

        //        switch (i)
        //        {
        //            case 0:
        //                r = v; g = t; b = p;
        //                break;
        //            case 1:
        //                r = q; g = v; b = p;
        //                break;
        //            case 2:
        //                r = p; g = v; b = t;
        //                break;
        //            case 3:
        //                r = p; g = q; b = v;
        //                break;
        //            case 4:
        //                r = t; g = p; b = v;
        //                break;
        //            default:		// case 5:
        //                r = v; g = p; b = q;
        //                break;
        //        }
        //        // r,g,b are now correct in [0,1] range, convert to [0,255]
        //        b_r = (byte)Math.Round(255 * r);
        //        b_g = (byte)Math.Round(255 * g);
        //        b_b = (byte)Math.Round(255 * b);

        //    }

        //    // h in [0, 360), s in [0, 1], v in [0, 1]
        //    // r, g, b in [0, 255]
        //    public static void RGBtoHSV(byte b_r, byte b_g, byte b_b, out float h, out float s, out float v)
        //    {
        //        float min, max, delta;
        //        float r = b_r / 255f, g = b_g / 255f, b = b_b / 255f;

        //        min = Math.Min(r, Math.Min(g, b));
        //        max = Math.Max(r, Math.Max(g, b));
        //        v = max; // set V here

        //        delta = max - min;

        //        if (max != 0)
        //            s = delta / max; // set S here
        //        else
        //        {
        //            // R, G, and B are all 0 (black), so S and H are arbitrarily set and we finish
        //            s = h = 0;
        //            return;
        //        }

        //        if (r == max)
        //            h = (g - b) / delta;		// between yellow & magenta
        //        else if (g == max)
        //            h = 2 + (b - r) / delta;	// between cyan & yellow
        //        else
        //            h = 4 + (r - g) / delta;	// between magenta & cyan

        //        h *= 60;				// degrees
        //        if (h < 0)
        //            h += 360;
        //    }

        //    #endregion

        //    #region Tree Building

        //    public void buildTree()
        //    {
        //        wheel = new ColorWheelNode[numTheta, numR];

        //        for (int i = 0; i < numTheta; i++)
        //            for (int j = 0; j < numR; j++)
        //                // array starts with radius = 1 down to 0 (but never reaching 0), theta at 0 to 360
        //                wheel[i, j] = new ColorWheelNode((float)(numR - j) / numR, 360f * i / numTheta, j, i);
        //    }

        //    public void classifyArtifacts(List<Artifact> allArtifacts)
        //    {
        //        foreach (Artifact a in allArtifacts)
        //        {
        //            int thetaIndex, rIndex;
        //            float theta, r;
        //            getWheelLocation(a, out theta, out r);
        //            getWheelArrayPosition(theta, r, out thetaIndex, out rIndex);                    

        //            wheel[thetaIndex, rIndex].Artifacts.Add(a);
        //        }
        //    }

        //    private bool thetaInBounds(float theta, float lBound, float uBound)
        //    {
        //        return (lBound <= theta && uBound >= theta) || (lBound <= theta - 360 && uBound >= theta - 360);
        //    }

        //    private bool rInBounds(float r, float lBound, float uBound)
        //    {
        //        return lBound <= r && uBound >= r;
        //    }

        //    private void determineCircles()
        //    {
        //        float radius = boundingQuad.Center.X - boundingQuad.Top; // radius of the entire wheel
        //        float rotate = 90f; // can rotate the color wheel from normal position of red = 0 degrees

        //        ColorWheelNode p1 = wheel[0, numR - 1], p2 = wheel[1, numR - 1]; // two adjacent nodes on the innermost ring

        //        // the x,y coordinates of where p1 and p2 end up on the actual color wheel
        //        Vector3 vec1 = new Vector3(p1.R * radius * (float)Math.Cos(p1.Theta), p1.R * radius * (float)Math.Sin(p1.Theta), 0);
        //        Vector3 vec2 = new Vector3(p2.R * radius * (float)Math.Cos(p2.Theta), p2.R * radius * (float)Math.Sin(p2.Theta), 0);

        //        // the maximum radius a circle can be if it's full of artifacts
        //        // it will be either half the distance between p1 and p2 OR half the distance to the center, whichever is smaller
        //        float maxRadius = Math.Min((vec2 - vec1).Length() / 2, p1.R / 2 * radius);

        //        int maxCutoff = 10; // a node is considered full (ie, will reach max radius) if it contains this many artifacts or more

        //        for (int i = 0; i < numTheta; i++)
        //        {
        //            for (int j = 0; j < numR; j++)
        //            {
        //                ColorWheelNode n = wheel[i,j];
        //                float nodeRadius = n.Artifacts.Count / (float)maxCutoff;
        //                Color nodeColor = getCircleColor(n);
        //                if(nodeRadius > 1)
        //                    nodeRadius = 1;
        //                wheel[i, j].Circle = new SelectableEllipse(new Vector2(n.R * (float)Math.Cos((n.Theta + rotate) * Math.PI / 180), n.R * (float)Math.Sin((n.Theta + rotate) * Math.PI / 180)),
        //                    nodeRadius * maxRadius, 0, nodeColor, nodeColor, null);
        //                wheel[i, j].Circle.Position = radius * wheel[i, j].Circle.Position + boundingQuad.Center; // scale it to where we're drawing the color wheel
        //            }
        //        }
        //    }

        //    private Color getCircleColor(ColorWheelNode node)
        //    {
        //        byte r, g, b;
        //        HSVtoRGB(node.Theta, 1, node.R, out r, out g, out b);
        //        return new Color(r, g, b); 
        //    }

        //    //private void createLeftRightLinks(ColorWheelNode[] ring)
        //    //{
        //    //    ring[0].Left = ring[ring.Length - 1];
        //    //    for(int i=0; i<ring.Length; i++)
        //    //    {
        //    //        if (i > 0)
        //    //            ring[i].Left = ring[i - 1];
        //    //        if (i < ring.Length - 1)
        //    //            ring[i].Right = ring[i + 1];
        //    //    }
        //    //    ring[ring.Length - 1].Right = ring[0];
        //    //}

        //    //private void createInnerOuterLinks(ColorWheelNode[] outer, ColorWheelNode[] ring, ColorWheelNode[] inner)
        //    //{
        //    //    for (int i = 0; i < ring.Length; i++)
        //    //    {
        //    //        ring[i].Outer = outer == null ? null : outer[i];
        //    //        ring[i].Inner = inner == null ? null : inner[i];
        //    //    }
        //    //}

        //    // gets position on the color wheel for a given artifact
        //    private void getWheelLocation(Artifact art, out float theta, out float r)
        //    {
        //        float v;
        //        // theta is hue, r is saturation
        //        RGBtoHSV(art.Color.R, art.Color.G, art.Color.B, out theta, out r, out v);
        //    }

        //    // given position on the color wheel, returns where it would be located within the array
        //    private void getWheelArrayPosition(float theta, float r, out int thetaIndex, out int rIndex)
        //    {
        //        float thetaRange = (wheel[1, 0].Theta - wheel[0, 0].Theta) / 2;
        //        float rRange = (wheel[0, 0].R - wheel[0, 1].R) / 2;

        //        thetaIndex = rIndex = -1;

        //        for (int i = 0; i < numTheta && thetaIndex == -1; i++)
        //            if (thetaInBounds(theta, wheel[i, 0].Theta - thetaRange, wheel[i, 0].Theta + thetaRange))
        //                thetaIndex = i;
        //        for (int i = 0; i < numR && rIndex == -1; i++)
        //            // our split doesn't work quite as well for R because it doesn't wrap around the space
        //            // so if we're on our last R (the one that's supposed to cover all the way down to 0), we have to give it a boost
        //            if (rInBounds(r, wheel[0, i].R - (i == numR - 1 ? 3 : 1) * rRange, wheel[0, i].R + rRange))
        //                rIndex = i;
        //    }

        //    #endregion

        //    public void Draw()
        //    {
        //        for (int i = 0; i < numTheta; i++)
        //            for (int j = 0; j < numR; j++)
        //            {
        //                XNA.PushMatrix();
        //                XNA.Translate(wheel[i, j].Circle.Position);
        //                wheel[i, j].Circle.DrawFill(false);
        //                XNA.PopMatrix();
        //            }
        //    }

        //}

        //class ColorWheelNode
        //{
        //    public float R, Theta; // polar coordinates
        //    public int ThetaIndex, RIndex;
        //    //public ColorWheelNode Left, Right, Inner, Outer; // left, right = lower, higher theta. inner, outer = smaller, larger radius
        //    public List<Artifact> Artifacts; // the artifacts we've judged to belong to this spot on the color wheel
        //    public SelectableEllipse Circle;
        //    public ColorWheelNode(float r, float theta, int rIndex, int thetaIndex)
        //    {
        //        R = r;
        //        Theta = theta;
        //        RIndex = rIndex;
        //        ThetaIndex = thetaIndex;
        //        Artifacts = new List<Artifact>();
        //    }
        //}

        // ColorWheelLattice will query this class to get points that are on the unit circle and converted to box coordinates
        class CircleConverter
        {
            public SelectableQuad boundingQuad;
            public float quadMaxRadius, unitMaxRadius;
            public float rotation = (float)Math.PI / 2;

            public CircleConverter(SelectableQuad quad, float quadMaxRadius)
            {
                boundingQuad = quad;
                this.quadMaxRadius = quadMaxRadius;
                unitMaxRadius = quadMaxRadius / (0.5f * quad.Width);
            }

            public Vector2 getPointUnit(float r, float theta)
            {
                return new Vector2((float)(r * Math.Cos(theta)), (float)(r * Math.Sin(theta)));
            }

            public Vector2 getPointBox(float r, float theta)
            {
                Vector2 box = getPointUnit(r * boundingQuad.Width * 0.5f, theta + rotation);
                box.X += boundingQuad.Center.X;
                box.Y += boundingQuad.Center.Y;
                return box;
            }
        }

        // represents a hexagonal lattice packing of colors in a color wheel
        // the color blobs that get drawn are ONLY for appearance, the user secretly interacts with an ArtifactGrid instead
        class ColorWheelLattice
        {
            List<ColorWheelLatticeNode> nodes;
            float maxRadius;
            SelectableQuad boundingQuad;
            CircleConverter converter;

            // make nodesInCenterRow an odd number so there's an actual center
            // boundingQuad should be square
            public ColorWheelLattice(int nodesInCenterRow, SelectableQuad boundingQuad, List<Artifact> allArtifacts, BohemianArtifact bookshelf)
            {
                nodes = new List<ColorWheelLatticeNode>();
                maxRadius = boundingQuad.Width / (nodesInCenterRow * 2);
                this.boundingQuad = boundingQuad;
                converter = new CircleConverter(boundingQuad, maxRadius);
                Vector2 quadCenter = new Vector2(boundingQuad.Center.X, boundingQuad.Center.Y);

                // create the lattice on an inside-out ring pattern, starting with the center
                nodes.Add(new ColorWheelLatticeNode(quadCenter, 0, 0, boundingQuad));

                int ringCount = 1;
                float x, y, r, theta;
                Vector2 pt;
                bool stopLoop = false;
                ColorWheelLatticeNode node;
                while (!stopLoop)
                {
                    for (int i = 0; i < 360; i += 60)
                    {
                        // create the node on the vertex of the hexagon first
                        //r = maxRadius * 2 * ringCount;
                        r = converter.unitMaxRadius * 2 * ringCount;
                        theta = (float)(i * Math.PI / 180);
                        pt = converter.getPointBox(r, theta);
                        //x = (float)(r * Math.Cos(theta));
                        //y = (float)(r * Math.Sin(theta));
                        //node = new ColorWheelLatticeNode(new Vector2(x, y) + quadCenter, r, theta, boundingQuad);
                        node = new ColorWheelLatticeNode(pt, r, theta, boundingQuad);
                        if (circleInBounds(node.Circle, boundingQuad) && r <= 1)
                            nodes.Add(node);
                        else
                            stopLoop = true; // will still try to add more nodes if they fit, but we won't execute another pass through loop

                        // there will be more circles on the edges of the hexagon in all likelihood (for ringCount = 2 or higher)
                        int circlesOnEdge = ringCount - 1;
                        if (circlesOnEdge > 0)
                        {
                            float nexttheta = (float)((i + 60) * Math.PI / 180);
                            Vector2 nextpt = converter.getPointBox(r, nexttheta); 
                            //float nextx = (float)(r * Math.Cos(nexttheta));
                            //float nexty = (float)(r * Math.Sin(nexttheta));

                            for (int j = 1; j <= circlesOnEdge; j++)
                            {
                                float delta = (float)j / ringCount;
                                // this node is some linear combination of the vertex created above and the next vertex on the hexagon, measured by 0 <= delta <= 1
                                //node = new ColorWheelLatticeNode(new Vector2((1 - delta) * pt.X + delta * nextpt.X, (1 - delta) * pt.Y + delta * nextpt.Y) + quadCenter,
                                node = new ColorWheelLatticeNode(new Vector2((1 - delta) * pt.X + delta * nextpt.X, (1 - delta) * pt.Y + delta * nextpt.Y),
                                    r, (1 - delta) * theta + delta * nexttheta, boundingQuad);
                                if (circleInBounds(node.Circle, boundingQuad) && r <= 1)
                                    nodes.Add(node);
                                else
                                    stopLoop = true; // will still try to add more nodes if they fit, but we won't execute another pass through loop
                            }
                        }
                    }
                    ringCount++;
                }

                foreach (ColorWheelLatticeNode n in nodes)
                {
                    n.Circle.TouchReleased += new TouchReleaseEventHandler(Circle_TouchReleased);
                    bookshelf.SelectableObjects.AddObject(n.Circle);
                }

                // ok, all the nodes are created now
                // have to size them based on how many artifacts are colored within the bounds of the circle
                int artifactCutoff = 8; // needs this many artifacts (or more) to hit maxRadius
                int foundCount = 0;
                foreach (Artifact a in allArtifacts)
                {
                    float v;
                    Color color = a.Color;
                    //color.R = 180; color.G = 0; color.B = 0;
                    RGBtoHSV(color.R, color.G, color.B, out theta, out r, out v);
                    //r /= 2;
                    //x = (float)(r * Math.Cos(theta));
                    //y = (float)(r * Math.Sin(theta));
                    bool found = false;
                    foreach (ColorWheelLatticeNode n in nodes)
                    {
                        if (n.containsPoint(converter.getPointBox(r, theta), maxRadius))
                        {
                            found = true;
                            n.artifacts.Add(a);
                            n.Radius += maxRadius / artifactCutoff;
                            if (n.Radius > maxRadius)
                                n.Radius = maxRadius;
                            break;
                        }
                    }
                    if (found) foundCount++;
                }

                Console.WriteLine("Found count: " + foundCount);

                //float dx = boundingQuad[0].X;
                //float dy = boundingQuad[0].Y + maxRadius;

                //bool offsetLattice = true;
                //for (int i = 0; i < nodesInCenterRow; i++)
                //{
                //    if (offsetLattice)
                //    {
                //        dx += maxRadius * 2;
                //        for (int j = 1; j < nodesInCenterRow; j++)
                //        {
                //            nodes.Add(new ColorWheelLatticeNode(new Vector2(dx, dy), boundingQuad));
                //            dx += maxRadius * 2;
                //        }
                //    }
                //    else
                //    {
                //        dx += maxRadius;
                //        for (int j = 0; j < nodesInCenterRow; j++)
                //        {
                //            nodes.Add(new ColorWheelLatticeNode(new Vector2(dx, dy), boundingQuad));
                //            dx += maxRadius * 2;
                //        }
                //    }
                //    offsetLattice = !offsetLattice;
                //    dx = boundingQuad[0].X;
                //    dy += maxRadius * 2;
                //}

                foreach (ColorWheelLatticeNode n in nodes)
                {
                    //n.Radius = maxRadius;
                    n.finalizeRadius();
                }
            }

            void Circle_TouchReleased(object sender, TouchArgs e)
            {
                SelectableEllipse c = sender as SelectableEllipse;

                ColorWheelLatticeNode found = null;
                for (int i = 0; i < nodes.Count && found == null; i++)
                    if (nodes[i].Circle == c)
                        found = nodes[i];

                Console.WriteLine("Circle color: " + found.Color.ToString());
                foreach (Artifact a in found.artifacts)
                    Console.WriteLine("Artifact: " + a.Color);
            }

            public bool circleInBounds(SelectableEllipse circle, SelectableQuad box)
            {
                return circle.Position.X - circle.Radius >= box[0].X && circle.Position.X + circle.Radius <= box[2].X &&
                    circle.Position.Y - circle.Radius >= box[0].Y && circle.Position.Y + circle.Radius <= box[2].Y;
            }

            public void Draw()
            {
                foreach (ColorWheelLatticeNode n in nodes)
                {
                    XNA.PushMatrix();
                    XNA.Translate(n.Circle.Position);
                    n.Circle.DrawFill(false);
                    XNA.PopMatrix();
                }
            }

            public void DrawSelectable()
            {
                foreach (ColorWheelLatticeNode n in nodes)
                {
                    XNA.PushMatrix();
                    XNA.Translate(n.Circle.Position);
                    n.Circle.DrawSelectable();
                    XNA.PopMatrix();
                }
            }

        }

        class ColorWheelLatticeNode
        {
            float radius;
            SelectableEllipse circle;
            float r, theta;
            public List<Artifact> artifacts = new List<Artifact>();

            public ColorWheelLatticeNode(Vector2 center, float r, float thetaRad, SelectableQuad boundingQuad)
            {
                // this is needed because the circle we originally sampled on cut a space of [0,1], so the circle's radius was actually only 0.5 in the "boundingQuad space"
                // our theoretical color wheel has radius 1
                //this.r = r * 2;
                this.r = r;
                theta = thetaRad;
                Color color = getCircleColor(this.r, (float)(thetaRad * 180 / Math.PI));

                circle = new SelectableEllipse(center, 0, 0, color, color, null);
            }

            public Color getCircleColor(float r, float theta)
            {
                if ((r == 0 && theta == 0) || r > 1)
                    return new Color(200, 200, 200);
                byte br, bg, bb;
                HSVtoRGB(theta, r, 1, out br, out bg, out bb);
                return new Color(br, bg, bb); 
            }



            public float Radius
            {
                get { return radius; }
                set { radius = value; }
            }

            public SelectableEllipse Circle
            {
                get { return circle; }
            }

            public Color Color
            {
                get { return circle.Color; }
            }

            public void finalizeRadius()
            {
                circle.Radius = radius;
            }

            public bool containsPoint(Vector2 boxPt, float maxRadius)
            {
                bool res = (boxPt.X - circle.Position.X) * (boxPt.X - circle.Position.X) + (boxPt.Y - circle.Position.Y) * (boxPt.Y - circle.Position.Y) <= maxRadius * maxRadius;
                return res;
            }
        }

        // represents a square grid which is subdivided into smaller squares which each contain artifacts
        // the artifacts are dispersed throughout the grid based on their position on a color wheel, centered in the grid
        class ArtifactGrid
        {
            ArtifactGridNode[,] grid;
            int dimension;

            public ArtifactGrid(int dimension, List<Artifact> allArtifacts)
            {
                grid = new ArtifactGridNode[dimension, dimension];
                this.dimension = dimension;
                for (int i = 0; i < dimension; i++)
                    for (int j = 0; j < dimension; j++)
                        grid[i, j] = new ArtifactGridNode(i, j);

                populateGrid(allArtifacts);
            }

            public void populateGrid(List<Artifact> allArtifacts)
            {
                int i, j;
                foreach (Artifact a in allArtifacts)
                {
                    getGridIndices(a, out i, out j);
                    grid[i, j].Artifacts.Add(a);
                }
            }

            // gets position on the color wheel for a given artifact, then translate it to grid indices
            private void getGridIndices(Artifact art, out int i, out int j)
            {
                float v;
                float theta, r;
                // theta is hue, r is saturation
                RGBtoHSV(art.Color.R, art.Color.G, art.Color.B, out theta, out r, out v);

                // translate position on unit color wheel to x,y coords inside unit square
                double x = r * Math.Cos(theta);
                double y = r * Math.Sin(theta);
                // x + 1 / 2 is shorthand for x - (-1) / 1 - (-1), converts x to a percentage of the range [-1, 1]
                i = (int)Math.Floor((x + 1) / 2 * dimension); // then convert to index by doing * dimension and clamp
                j = (int)Math.Floor((y + 1) / 2 * dimension);
                // need to clamp, as x = 1 or y = 1 tries to leave the grid
                if (i >= dimension)
                    i = dimension - 1;
                if (j >= dimension)
                    j = dimension - 1;
            }
        }

        class ArtifactGridNode
        {
            public int I, J; // indices into artifactgrid
            public List<Artifact> Artifacts;

            public ArtifactGridNode(int i, int j)
            {
                I = i;
                J = j;
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

        List<Container> colorCircles;
        //ColorWheel colorWheel;
        ColorWheelLattice colorWheelLattice;

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

            colorCircles = new List<Container>();
            //colorWheel = new ColorWheel(6, 6, bookshelf.Library.Artifacts,
            //    new SelectableQuad(new Vector2(0.1f, 0.1f), new Vector2(0.8f, 0.8f), Color.Black));
            colorWheelLattice = new ColorWheelLattice(9, new SelectableQuad(new Vector2(0.0f, 0.0f), new Vector2(1f, 1f), Color.Black), bookshelf.Library.Artifacts, bbshelf);

            //ArtifactGrid grid = new ArtifactGrid(10, bookshelf.Library.Artifacts);

            //setupColorCircles();
            showColorWheel();



        }

        public void setupColorCircles()
        {
            Dictionary<Color, int> dict = new Dictionary<Color, int>();

            //foreach (Artifact a in bookshelf.Library.Artifacts)
            //{
            //    if (a.Color.B > 100)
            //    {
            //        if (dict.ContainsKey(a.Color))
            //            dict[a.Color]++;
            //        else
            //            dict.Add(a.Color, 1);
            //    }
            //}
            
            //List<KeyValuePair<Color, int>> list = dict.ToList();
            //foreach (KeyValuePair<Color, int> kvp in list)
            //    colorCircles.Add(new Container(new SelectableEllipse(new Vector2(), 0.02f, 0, kvp.Key, kvp.Key, null), 0));

            foreach(Artifact a in bookshelf.Library.Artifacts)
            {
                Container c = new Container(new SelectableEllipse(new Vector2(), 0.02f, 0, a.Color, a.Color, null), a);
                c.Ellipse.TouchReleased += new TouchReleaseEventHandler(Ellipse_TouchReleased);
                bookshelf.SelectableObjects.AddObject(c.Ellipse);
                colorCircles.Add(c);
            }

            //alignCirclesLinear();
            alignCirclesPolar();

        }

        void Ellipse_TouchReleased(object sender, TouchArgs e)
        {
            SelectableEllipse ellipse = sender as SelectableEllipse;
            Container found = null;
            for (int i = 0; i < colorCircles.Count && found == null; i++)
                if (colorCircles[i].Ellipse == ellipse)
                    found = colorCircles[i];

            bookshelf.Library.SelectedArtifact = found.Artifact;
        }

        public void showColorWheel()
        {
            colorCircles.Clear();
            for (int i = 0; i < 360; i += 15)
            {
                byte r, g, b;
                HSVtoRGB(i, 1, 1, out r, out g, out b);
                colorCircles.Add(new Container(new SelectableEllipse(new Vector2(), 0.02f, 0, new Color(r, g, b), Color.Black, null), null));
                HSVtoRGB(i, 0.75f, 1, out r, out g, out b);
                colorCircles.Add(new Container(new SelectableEllipse(new Vector2(), 0.02f, 0, new Color(r, g, b), Color.Black, null), null));
                HSVtoRGB(i, 0.5f, 1, out r, out g, out b);
                colorCircles.Add(new Container(new SelectableEllipse(new Vector2(), 0.02f, 0, new Color(r, g, b), Color.Black, null), null));
                HSVtoRGB(i, 0.25f, 1, out r, out g, out b);
                colorCircles.Add(new Container(new SelectableEllipse(new Vector2(), 0.02f, 0, new Color(r, g, b), Color.Black, null), null));
            }
            alignCirclesPolar();
        }

        public void alignCirclesLinear()
        {
            float dx = colorCircles[0].Ellipse.Radius;
            float dy = 0.15f;

            foreach (Container c in colorCircles)
            {
                SelectableEllipse se = c.Ellipse;
                se.Position = new Vector3(dx, dy, 0);
                dx += se.Radius * 2;
                if (dx > 1 - se.Radius)
                {
                    dx = colorCircles[0].Ellipse.Radius;
                    dy += se.Radius * 2;
                }
            }
        }

        public void alignCirclesPolar()
        {
            Vector3 wheelCenter = new Vector3(0.5f, 0.5f, 0);
            float radius = 0.4f;
            float rotate = 90f; // in degrees, rotate the color wheel (normally, red = 0 degrees)

            foreach (Container c in colorCircles)
            {
                SelectableEllipse se = c.Ellipse;
                float h, s, v;
                RGBtoHSV(se.Color.R, se.Color.G, se.Color.B, out h, out s, out v);
                double theta = (h + rotate) * Math.PI / 180.0f;
                float r = s;

                Vector3 unitCirclePos = new Vector3(r * (float)Math.Cos(theta), r * (float)Math.Sin(theta), 0);
                se.Position = unitCirclePos * radius + wheelCenter;
            }
        }

        #region Color Conversion Algorithms

        // h in [0, 360), s in [0, 1], v in [0, 1]
        // r, g, b in [0, 255]
        public static void HSVtoRGB(float h, float s, float v, out byte b_r, out byte b_g, out byte b_b)
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

        #endregion

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

            //foreach (Container c in colorCircles)
            //{
            //    XNA.PushMatrix();
            //    XNA.Translate(c.Ellipse.Position);
            //    c.Ellipse.DrawFill(false);
            //    XNA.PopMatrix();
            //}

            //colorWheel.Draw();
            colorWheelLattice.Draw();

            XNA.PopMatrix();
        }

        public void DrawSelectable()
        {
            XNA.PushMatrix();
            XNA.Translate(position);
            XNA.Scale(size);

            //foreach (Container c in colorCircles)
            //{
            //    XNA.PushMatrix();
            //    XNA.Translate(c.Ellipse.Position);
            //    c.Ellipse.DrawSelectable();
            //    XNA.PopMatrix();
            //}
            colorWheelLattice.DrawSelectable();

            XNA.PopMatrix();
        }
    }
}