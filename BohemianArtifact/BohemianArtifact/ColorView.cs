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
        // couples an artifact with the ellipse it's drawn in
        class ColorViewContainer
        {
            public SelectableEllipse Ellipse;
            public Artifact Artifact;
            public ColorViewContainer(SelectableEllipse se, Artifact a)
            {
                Ellipse = se;
                Artifact = a;
            }

            public override string ToString()
            {
                return Artifact.ArticleName;
            }
        }

        // couples an artifact and some information about its location and how far away it is from a centralized point
        struct ArtifactDistance : IComparable<ArtifactDistance>
        {
            public Artifact Artifact;
            public float Distance;
            public Vector2 Location;
            public ArtifactDistance(Artifact a, float d, Vector2 l)
            {
                Artifact = a;
                Distance = d;
                Location = l;
            }

            public int CompareTo(ArtifactDistance rhs)
            {
                // faster to do it this way than call Distance.CompareTo, I think (as per benchmarks in ColourVis)
                // and we will be doing this lots of times per game step so it must be fast
                if (Distance < rhs.Distance) return -1;
                if (Distance > rhs.Distance) return 1;
                return 0; 
            }

            public override string ToString()
            {
                return Artifact.ArticleName + " " + Distance;
            }

        }

        // copules an artifact with information about how it's going to move and resize itself to a new position
        struct ArtifactMoving
        {
            ColorViewContainer movingArtifact;
            float initialRadius, finalRadius;
            float initialThickness, finalThickness;
            Vector3 initialPosition, finalPosition;
            Vector3 positionVector;
            float totalTime;

            float doNothingTime, doNothingTimeCounter;

            float radiusMovement, positionMovement;
            bool radiusDone, positionDone, thicknessDone;

            public ArtifactMoving(ColorViewContainer artifact, float time, Vector3 finalPosition)
                : this(artifact, time, finalPosition, artifact.Ellipse.Radius)
            {
                radiusDone = true;
            }

            public ArtifactMoving(ColorViewContainer artifact, float time, float finalRadius)
                : this(artifact, time, artifact.Ellipse.Position, finalRadius)
            {
                positionDone = true;
            }

            public ArtifactMoving(ColorViewContainer artifact, float time, Vector3 finalPosition, float finalRadius)
            {
                movingArtifact = artifact;
                initialRadius = artifact.Ellipse.Radius;
                this.finalRadius = finalRadius;
                totalTime = time;

                initialPosition = artifact.Ellipse.Position;
                this.finalPosition = finalPosition;
                positionVector = finalPosition - initialPosition;

                initialThickness = finalThickness = artifact.Ellipse.EdgeThickness;

                radiusMovement = positionMovement = 0;
                radiusDone = positionDone = false;
                thicknessDone = true;

                doNothingTime = doNothingTimeCounter = 0;
            }

            public void setBorderThickness(float final)
            {
                finalThickness = final;
                thicknessDone = false;
            }

            public float DoNothingTime
            {
                set { doNothingTime = value; }
            }

            // returns if it finished
            public bool performTimestep(double deltaTime)
            {
                if (doNothingTime > 0 && doNothingTime > doNothingTimeCounter)
                {
                    doNothingTimeCounter += (float)deltaTime;
                    return false;
                }

                if (!thicknessDone)
                {
                    float thicknessDelta = (finalThickness - initialThickness) * (float)(deltaTime / totalTime);

                    float r = movingArtifact.Ellipse.EdgeThickness + thicknessDelta;
                    if ((finalThickness > initialThickness && r >= finalThickness) || (finalThickness < initialThickness && r <= finalThickness))
                    {
                        thicknessDone = true;
                        r = finalThickness;
                    }

                    movingArtifact.Ellipse.setEdgeThickness(r, radiusDone); // only recompute if we aren't going to set the radius below, no point doing it twice
                }

                if (!radiusDone)
                {
                    float radiusDelta = (finalRadius - initialRadius) * (float)(deltaTime / totalTime);

                    float r = movingArtifact.Ellipse.Radius + radiusDelta;
                    if ((finalRadius > initialRadius && r >= finalRadius) || (finalRadius < initialRadius && r <= finalRadius))
                    {
                        radiusDone = true;
                        r = finalRadius;
                    }

                    movingArtifact.Ellipse.Radius = r;
                }

                if (!positionDone)
                {
                    float movementDelta = (float)(deltaTime / totalTime);
                    movingArtifact.Ellipse.Position += movementDelta * positionVector;

                    // find out if we've moved past our final location by projecting onto the line segment it was moving on and testing the t value
                    float t;
                    dist2PointToSegment(movingArtifact.Ellipse.Position, initialPosition, finalPosition, out t);

                    if (t >= 1) // it's at final location (or past it, which will cause jittery movement)
                    {
                        movingArtifact.Ellipse.Position = finalPosition; // move manually to its final resting place
                        positionDone = true;
                    }
                }

                return radiusDone && positionDone && thicknessDone;
            }

            // get the distance (squared) of a point to a segment.
            //    Input:  a Point P and a Segment (sA,sB) (in any dimension)
            //    Return: the shortest distance from P to S, point on S is t * (sB-sA)
            public static float dist2PointToSegment(Vector3 P, Vector3 sA, Vector3 sB, out float t)
            {
                Vector3 v = sB - sA;
                Vector3 w = P - sA;
                float c1 = Vector3.Dot(w, v);
                float c2 = Vector3.Dot(v, v);
                t = c1 / c2;

                if (t < 0.0f)
                    return Vector3.DistanceSquared(P, sA);
                else if (t > 1.0f)
                    return Vector3.DistanceSquared(P, sB);
                else
                    return Vector3.DistanceSquared(P, sA + t * v);
            }
        }

        // ColorWheelLattice and ArtifactGrid will query this class to get points that are on the unit circle and converted to box coordinates
        // if you want to rotate the whole color wheel in the color view, you can do it by changing the rotation variable below
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

            // given a color, returns where it should be on the circle
            // theta is returned in radians, which lets you plug it in to the other getters in this class
            public void getPolarCoords(Color color, out float r, out float theta)
            {
                float v;
                RGBtoHSV(color.R, color.G, color.B, out theta, out r, out v);
                theta = (float)(theta * Math.PI / 180f);
            }

            public Vector2 getPointUnitRotated(float r, float theta)
            {
                return new Vector2((float)(r * Math.Cos(theta + rotation)), (float)(r * Math.Sin(theta + rotation)));
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
                        r = converter.unitMaxRadius * 2 * ringCount;
                        theta = (float)(i * Math.PI / 180);
                        pt = converter.getPointBox(r, theta);
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

                            for (int j = 1; j <= circlesOnEdge; j++)
                            {
                                float delta = (float)j / ringCount;
                                // this node is some linear combination of the vertex created above and the next vertex on the hexagon, measured by 0 <= delta <= 1
                                //node = new ColorWheelLatticeNode(new Vector2((1 - delta) * pt.X + delta * nextpt.X, (1 - delta) * pt.Y + delta * nextpt.Y),
                                //    r, (1 - delta) * theta + delta * nexttheta, boundingQuad); 
                                pt = converter.getPointBox(r, (1 - delta) * theta + delta * nexttheta);
                                node = new ColorWheelLatticeNode(pt, r, (1 - delta) * theta + delta * nexttheta, boundingQuad);
                                if (circleInBounds(node.Circle, boundingQuad) && r <= 1)
                                    nodes.Add(node);
                                else
                                    stopLoop = true; // will still try to add more nodes if they fit, but we won't execute another pass through loop
                            }
                        }
                    }
                    ringCount++;
                }

                //foreach (ColorWheelLatticeNode n in nodes)
                //{
                //    n.Circle.TouchReleased += new TouchReleaseEventHandler(Circle_TouchReleased);
                //    bookshelf.SelectableObjects.AddObject(n.Circle);
                //}

                // ok, all the nodes are created now
                // have to size them based on how many artifacts are colored within the bounds of the circle
                // notice that not ALL artifacts will hit a circle (hexagon packing is not 100% space filling), but it should be close enough to get something that looks good
                int artifactCutoff = 6; // needs this many artifacts (or more) to hit maxRadius
                int foundCount = 0;
                foreach (Artifact a in allArtifacts)
                {
                    converter.getPolarCoords(a.Color, out r, out theta);

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

                //Console.WriteLine("Found count: " + foundCount);

                foreach (ColorWheelLatticeNode n in nodes)
                {
                    //n.Radius = maxRadius;
                    n.finalizeRadius();
                }
            }

            // only ever used for debugging, will not be attached to any ellipses for normal operation
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
                // our theoretical color wheel has radius 1
                // r and theta represent positions on unit circle, but center is passed in already converted to a coordinate within the box
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
                return (boxPt.X - circle.Position.X) * (boxPt.X - circle.Position.X) + (boxPt.Y - circle.Position.Y) * (boxPt.Y - circle.Position.Y) <= maxRadius * maxRadius;
            }
        }

        // represents a square grid which is subdivided into smaller squares which each contain artifacts
        // the artifacts are dispersed throughout the grid based on their position on a color wheel, centered in the grid
        // despite not being drawn in grid form to the screen, this class is actually what the user interacts with
        // and unlike ColorWheelLattice, every artifact will be included in some square
        class ArtifactGrid
        {
            ArtifactGridNode[,] grid;
            int dimension;
            SelectableQuad boundingQuad;
            CircleConverter converter;
            SelectableText[,] debugText;
            ColorView colorView;
            List<ColorViewContainer> activeArtifacts = new List<ColorViewContainer>();
            List<ColorViewContainer> shrinkingArtifacts = new List<ColorViewContainer>();

            List<ArtifactMoving> movingArtifacts = new List<ArtifactMoving>();

            static float ActiveArtifactRadius = 0.1f; // the radius of the large center artifact being shown
            static float NumSecondsToShrink = 1.2f; // how long you want it to take to shrink an artifact after it's been deselected
            static float ShrinkRadiusPerDelta = ActiveArtifactRadius / (NumSecondsToShrink * 60);

            //static float SubArtifactRadius = ActiveArtifactRadius * 0.7f;
            static float SubArtifactRadiusMin = ActiveArtifactRadius * 0.3f;
            static float SubArtifactRadiusMax = ActiveArtifactRadius * 0.75f;
            //static float SubWheelRadius = ActiveArtifactRadius + SubArtifactRadius; // get placed evenly on a circle of this radius, with center = 'location'

            static float ActiveArtifactBorderWidth = 0.012f;
            static float SubArtifactBorderWidth = 0.005f;
            static float ShrinkBorderPerDelta = ActiveArtifactBorderWidth / (NumSecondsToShrink * 60);


            static float NumSecondsToMove = 0.6f; // how long you want it to take for circles to move during their animation

            int touchedCircleIndex = -1; // whether or not the user touched a ColorView circle to show a new artifact

            public ArtifactGrid(int dimension, SelectableQuad boundingQuad, List<Artifact> allArtifacts, ColorView colorView)
            {
                grid = new ArtifactGridNode[dimension, dimension];
                this.colorView = colorView;
                this.boundingQuad = boundingQuad;
                converter = new CircleConverter(boundingQuad, 1);

                this.dimension = dimension;
                for (int i = 0; i < dimension; i++)
                    for (int j = 0; j < dimension; j++)
                        grid[i, j] = new ArtifactGridNode(i, j);

                populateGrid(allArtifacts);

                boundingQuad.TouchActivated += new TouchActivatedEventHandler(boundingQuad_TouchActivated);
                boundingQuad.TouchReleased += new TouchReleaseEventHandler(boundingQuad_TouchReleased);
                colorView.Bookshelf.SelectableObjects.AddObject(boundingQuad);
            }

            void boundingQuad_TouchActivated(object sender, TouchArgs e)
            {
                Touch touch = colorView.Bookshelf.TouchPoints[e.TouchId];
                // a two-stage conversion is needed, one to confine the touch coord (in absolute screen coords) into color view's relative coord system
                // then to further confine it to the bounding box for the grid
                // (which will probably go from 0, 0 to 1, 1, anyway but still have to do it)
                Vector2 relativePos = convertRelativeParentToBox(colorView.convertTouchAbsoluteToRelative(new Vector2(touch.X, touch.Y)));
                //Console.WriteLine("touch at " + touch.X + ", " + touch.Y + " --- Relative at " + relativePos.X + ", " + relativePos.Y);

                List<ArtifactDistance> closest = getClosestArtifacts(relativePos, 5, 0.05f);

                // we want to shrink those artifacts which WERE in the active list, but will no longer be (because they are no longer in the closest list)
                // this picks all active artifacts that are NOT in closest (with care taken because they are lists of different class types)
                // linq is fun!
                shrinkingArtifacts.AddRange(activeArtifacts.Where(a => !(closest.Select(c => c.Artifact).Contains(a.Artifact))));

                // now that we know which are shrinking, we can simply make the active artifacts contain all the ones that are now closest
                foreach (ColorViewContainer c in activeArtifacts)
                    colorView.Bookshelf.SelectableObjects.RemoveObject(c.Ellipse);
                activeArtifacts.Clear();
                foreach (ArtifactDistance a in closest)
                    activeArtifacts.Add(new ColorViewContainer(new SelectableEllipse(a.Location, ActiveArtifactRadius, ActiveArtifactBorderWidth, Color.White, 
                        a.Artifact.Color, a.Artifact.Color, a.Artifact.Texture), a.Artifact));

            }

            // this isn't an actual event, but colorview's Update function will call this
            public void boundingQuad_TouchMoved(object sender, TouchArgs e)
            {
                boundingQuad_TouchActivated(sender, e);
            }

            void boundingQuad_TouchReleased(object sender, TouchArgs e)
            {
                if (activeArtifacts.Count > 0)
                    colorView.Bookshelf.Library.SelectedArtifact = activeArtifacts[0].Artifact;
            }

            

            // given 0 to 1 coords for where a point is in the parent ColorView, convert it to be relative to the bounding quad
            // this will probably do nothing as it's pretty likely the bounding quad is 0, 0 to 1, 1 anyway
            Vector2 convertRelativeParentToBox(Vector2 relativeParent)
            {
                relativeParent -= new Vector2(boundingQuad[0].X, boundingQuad[0].Y); // move everything to 0, 0
                if (relativeParent.X < 0) relativeParent.X = 0;
                if (relativeParent.X > boundingQuad.Width) relativeParent.X = boundingQuad.Width;
                if (relativeParent.Y < 0) relativeParent.Y = 0;
                if (relativeParent.Y > boundingQuad.Height) relativeParent.Y = boundingQuad.Height;

                return new Vector2(relativeParent.X / boundingQuad.Width, relativeParent.Y / boundingQuad.Height);
            }

            public void populateGrid(List<Artifact> allArtifacts)
            {
                int i, j;
                foreach (Artifact a in allArtifacts)
                {
                    Vector2 boxLocation;
                    getGridIndices(a, out i, out j, out boxLocation);
                    grid[i, j].addArtifact(a, boxLocation);
                }
                debugText = new SelectableText[dimension, dimension];
                for (i = 0; i < dimension; i++)
                    for (j = 0; j < dimension; j++)
                    {
                        Vector3 center = boundingQuad[0] + new Vector3((i + 0.5f) / dimension * boundingQuad.Width, (j + 0.5f) / dimension * boundingQuad.Height, 0);
                        debugText[i, j] = new SelectableText(XNA.Font, grid[i,j].ArtifactCount.ToString(), center, Color.DarkRed, Color.White);
                    }
            }

            // gets position on the color wheel for a given artifact, then translate it to grid indices
            private void getGridIndices(Artifact art, out int i, out int j, out Vector2 location)
            {
                float r, theta;
                converter.getPolarCoords(art.Color, out r, out theta);

                Vector2 unitCirclePt = converter.getPointUnitRotated(r, theta);

                // translate position on unit color wheel to x,y coords inside unit square
                // x + 1 / 2 is shorthand for x - (-1) / 1 - (-1), converts x to a percentage of the range [-1, 1]
                i = (int)Math.Floor((unitCirclePt.X + 1) / 2 * dimension); // then convert to index by doing * dimension and clamp
                j = (int)Math.Floor((unitCirclePt.Y + 1) / 2 * dimension);
                // need to clamp, as x = 1 or y = 1 tries to leave the grid
                if (i >= dimension)
                    i = dimension - 1;
                if (j >= dimension)
                    j = dimension - 1;

                location = converter.getPointBox(r, theta);
            }

            public SelectableQuad BoundingQuad
            {
                get { return boundingQuad; }
            }

            #region Artifact Stuff

            // give it a position relative to its bounding quad
            // also pass in how many artifacts you want to return (maximum), and how far away from the boxPos will an artifact be considered close enough
            // returns a list of artifacts and their x, y positions in the box
            public List<ArtifactDistance> getClosestArtifacts(Vector2 boxPos, int howMany, float selectionRadius)
            {
                int i = (int)Math.Floor(boxPos.X * dimension);
                if (i >= dimension) i = dimension;
                int j = (int)Math.Floor(boxPos.Y * dimension);
                if (j >= dimension) j = dimension;

                List<ArtifactDistance> candidates = new List<ArtifactDistance>();
                // construct the list of potential candidates from the artifacts contained in any adjacent grid position
                for (int m = -1; m <= 1; m++)
                    for (int n = -1; n <= 1; n++)
                        if (i + m >= 0 && i + m < dimension && j + n >= 0 && j + n < dimension) // if in bounds
                            for (int l = 0; l < grid[i + m, j + n].Artifacts.Count; l++) // loop through all the artifacts
                            // create a temporary bundle of the artifact, plus how far away it is from boxPos
                            // but only adds it if it's within the selection radius, otherwise it's not really a candidate
                            {
                                float dist = (boxPos - grid[i + m, j + n].Locations[l]).LengthSquared();
                                if (dist <= selectionRadius)
                                    candidates.Add(new ArtifactDistance(grid[i + m, j + n].Artifacts[l], dist, grid[i + m, j + n].Locations[l]));
                            }

                candidates.Sort();

                // cull the list if we have too many
                if (candidates.Count > howMany)
                    candidates.RemoveRange(howMany, candidates.Count - howMany);

                return candidates;
            }

            // once the user has chosen an artifact (via the color tool or some other view), display it
            // this involves picking some nearby artifacts and displaying them in a circle around the main artifact
            public void displayArtifact(Artifact a)
            {
                int indexI, indexJ;
                Vector2 location;
                float theta;
                ArtifactMoving am;

                getGridIndices(a, out indexI, out indexJ, out location);

                List<ArtifactDistance> closest = getClosestArtifacts(location, 8, 0.05f);

                // remove "a" from closest, it will probably be the first in the list since it is closest to itself but in rare cases may not be
                int index = -1;
                for (int i = 0; i < closest.Count && index < 0; i++)
                    if (closest[i].Artifact == a)
                        index = i;
                if (index >= 0)
                    closest.RemoveAt(index);

                // if they touched a color view outer circle, we have to do different types of animations
                if (touchedCircleIndex != -1)
                {
                    ColorViewContainer touchedCircle = activeArtifacts[touchedCircleIndex];

                    movingArtifacts.Clear();
                    ColorViewContainer[] newArtifacts = new ColorViewContainer[closest.Count + 1]; // +1 for the center one

                    // the one they clicked is now the center
                    newArtifacts[0] = touchedCircle;
                    am = new ArtifactMoving(touchedCircle, NumSecondsToMove, new Vector3(location, 0), ActiveArtifactRadius);
                    am.setBorderThickness(ActiveArtifactBorderWidth);
                    movingArtifacts.Add(am);

                    // the center one moves to replace the one they touched
                    if (touchedCircleIndex >= newArtifacts.Length)
                        touchedCircleIndex = newArtifacts.Length - 1; // sometimes, if you display fewer artifacts than before, this will be off the end
                    newArtifacts[touchedCircleIndex] = activeArtifacts[0]; // 0 is always the center
                    am = new ArtifactMoving(activeArtifacts[0], NumSecondsToMove, getLocationOnOuterRing(touchedCircleIndex, newArtifacts.Length, location),
                        getSubArtifactRadius(touchedCircleIndex, newArtifacts.Length));
                    am.setBorderThickness(SubArtifactBorderWidth);
                    movingArtifacts.Add(am);

                    // now we have to fill in the remaining spots in new artifacts
                    // first, scan through currently active artifacts and, if they're sticking around, try to keep their position the same on the outer wheel
                    List<ArtifactDistance> remaining = new List<ArtifactDistance>();
                    foreach (ArtifactDistance ad in closest)
                    {
                        int indexInNewList = whereInArtifactList(ad.Artifact, newArtifacts);
                        if (indexInNewList == -1) // if already in the new list, ignore it
                        {
                            int indexInOldList = whereInArtifactList(ad.Artifact, activeArtifacts);
                            if (indexInOldList >= 0)
                            {
                                ColorViewContainer moving = activeArtifacts[indexInOldList];
                                // we want it to be at same index, but it might be off the end if closest is smaller than before; shrink it until it isn't
                                if (indexInOldList >= newArtifacts.Length)
                                {
                                    indexInOldList = newArtifacts.Length - 1;
                                    while (indexInOldList >= 0 && newArtifacts[indexInOldList] != null)
                                        indexInOldList--; // find first available spot
                                }
                                if (indexInOldList >= 0) // if it didn't get shoved off the back of the list (meaning all spots were full), go ahead and slot it in
                                {
                                    newArtifacts[indexInOldList] = moving;
                                    movingArtifacts.Add(new ArtifactMoving(moving, NumSecondsToMove, getLocationOnOuterRing(indexInOldList, newArtifacts.Length, location),
                                        getSubArtifactRadius(indexInOldList, newArtifacts.Length))); // do not need to adjust border thickness for these guys
                                }
                            }
                            else
                            {
                                remaining.Add(ad); // deal with it in the next pass; it's a totally new artifact and will get put in the first available spot
                            }
                        }
                    }

                    int firstAvailIndex = 1; // starts at 1 because we know the center is taken
                    while (firstAvailIndex < newArtifacts.Length && newArtifacts[firstAvailIndex] != null)
                        firstAvailIndex++;

                    // so now, all artifacts that were on the grid before (and still in closest) should be showing, and any empty space just gets filled up by what's left, in order
                    foreach (ArtifactDistance ad in remaining)
                    {
                        // if we spend all our available slots, whatever's left in "remaining" will just not be shown
                        if (firstAvailIndex < newArtifacts.Length)
                        {
                            // a reeeally small radius so it doesn't show in the transition until later when it grows
                            SelectableEllipse ellipse = new SelectableEllipse(location, 0.00001f, SubArtifactBorderWidth, Color.White, ad.Artifact.Color, ad.Artifact.Color, ad.Artifact.Texture);
                            ColorViewContainer container = new ColorViewContainer(ellipse, ad.Artifact);

                            newArtifacts[firstAvailIndex] = container;
                            // here is where it grows from small radius to its proper size
                            ArtifactMoving moving = new ArtifactMoving(container, NumSecondsToMove, getLocationOnOuterRing(firstAvailIndex, newArtifacts.Length, location),
                                getSubArtifactRadius(firstAvailIndex, newArtifacts.Length));
                            moving.DoNothingTime = NumSecondsToMove; // don't show these until the others have finished moving, then they'll pop out all cool-like
                            movingArtifacts.Add(moving);

                        }
                        // find the next available index
                        while (firstAvailIndex < newArtifacts.Length && newArtifacts[firstAvailIndex] != null)
                            firstAvailIndex++;
                    }

                    // hooray, everything should now be set up, just tidy up some loose ends
                    foreach (ColorViewContainer c in activeArtifacts)
                    {
                        // some of these are going away forever, we just don't know which
                        // so do it for all, then re-add all the active ones at the bottom of the function
                        c.Ellipse.TouchReleased -= Ellipse_TouchReleased;
                        colorView.Bookshelf.SelectableObjects.RemoveObject(c.Ellipse);
                    }

                    activeArtifacts.Clear();

                    foreach (ColorViewContainer cv in newArtifacts)
                        if (cv != null)
                            activeArtifacts.Add(cv);

                    // aid garbage collection?? (probably makes no difference...)
                    newArtifacts = null;
                    remaining.Clear();
                }

                // else, the artifact is being displayed through a means other than clicking on a color wheel circle, so display it differently
                else
                {
                    // everything that was previously active is getting removed, so make them all shrink
                    shrinkingArtifacts.AddRange(activeArtifacts);

                    foreach (ColorViewContainer c in activeArtifacts)
                        colorView.Bookshelf.SelectableObjects.RemoveObject(c.Ellipse);
                    activeArtifacts.Clear();

                    // the main artifact gets displayed where it's located
                    activeArtifacts.Add(new ColorViewContainer(new SelectableEllipse(location, ActiveArtifactRadius, ActiveArtifactBorderWidth, Color.White, a.Color, a.Color, a.Texture), a));

                    // the other ones in "closest" get displayed in a circle around 'a'               
                    for (int i = 0; i < closest.Count; i++)
                    {
                        //theta = (float)(i * 2 * Math.PI / closest.Count);
                        //Vector3 center = new Vector3(SubWheelRadius * (float)Math.Cos(theta) + location.X, SubWheelRadius * (float)Math.Sin(theta) + location.Y, 0);
                        Vector3 center = getLocationOnOuterRing(i + 1, closest.Count + 1, location);

                        // it starts in the same location as the parent circle, and moves to center
                        SelectableEllipse ellipse = new SelectableEllipse(location, getSubArtifactRadius(i + 1, closest.Count + 1), SubArtifactBorderWidth, Color.White,
                            closest[i].Artifact.Color, closest[i].Artifact.Color, closest[i].Artifact.Texture);
                        ColorViewContainer container = new ColorViewContainer(ellipse, closest[i].Artifact);

                        activeArtifacts.Add(container);
                        movingArtifacts.Add(new ArtifactMoving(container, NumSecondsToMove, center));
                    }
                }

                // don't make the center one selectable, start at index 1
                for (int i = 1; i < activeArtifacts.Count; i++)
                {
                    ColorViewContainer c = activeArtifacts[i];
                    c.Ellipse.TouchReleased += new TouchReleaseEventHandler(Ellipse_TouchReleased);
                    colorView.Bookshelf.SelectableObjects.AddObject(c.Ellipse);
                }

                touchedCircleIndex = -1; // we're done with this, turn it off for future interactions
            }

            public Vector3 getLocationOnOuterRing(int i, int numIncludingCenter, Vector2 center)
            {
                float theta = (float)((i - 1) * 2 * Math.PI / (numIncludingCenter - 1));
                float radius = ActiveArtifactRadius + getSubArtifactRadius(i, numIncludingCenter);
                return new Vector3(radius * (float)Math.Cos(theta) + center.X, radius * (float)Math.Sin(theta) + center.Y, 0);
            }

            public float getSubArtifactRadius(int i, int numIncludingCenter)
            {
                float delta = (float)(i - 1) / (numIncludingCenter - 1);
                return delta * SubArtifactRadiusMin + (1 - delta) * SubArtifactRadiusMax;
            }

            public int whereInArtifactList(Artifact a, ColorViewContainer[] list)
            {
                for (int i = 0; i < list.Length; i++)
                    if (list[i] != null && list[i].Artifact == a)
                        return i;
                return -1;
            }

            public int whereInArtifactList(Artifact a, List<ColorViewContainer> list)
            {
                for (int i = 0; i < list.Count; i++)
                    if (list[i] != null && list[i].Artifact == a)
                        return i;
                return -1;
            }

            void Ellipse_TouchReleased(object sender, TouchArgs e)
            {
                touchedCircleIndex = -1;
                for (int i = 0; i < activeArtifacts.Count && touchedCircleIndex == -1; i++)
                    if (activeArtifacts[i].Ellipse == sender)
                        touchedCircleIndex = i;

                if (touchedCircleIndex != -1)
                {
                    colorView.Bookshelf.Library.SelectedArtifact = activeArtifacts[touchedCircleIndex].Artifact;
                }
            }

            // pass in the time between updates, hopefully it's 1/60th of a second most of the time
            public void shrinkArtifactsByTimestep(double deltaTime)
            {
                List<ColorViewContainer> newShrink = new List<ColorViewContainer>();
                foreach (ColorViewContainer c in shrinkingArtifacts)
                {
                    float thickness = c.Ellipse.EdgeThickness - ShrinkBorderPerDelta * (float)(deltaTime * 60);
                    if (thickness < 0)
                        thickness = 0;
                    c.Ellipse.setEdgeThickness(thickness, false); // don't recompute, we're doing it below with the radius setter
                    c.Ellipse.Radius -= ShrinkRadiusPerDelta * (float)(deltaTime * 60); // if deltaTime is 1/60th of a second, this will evaluate to * 1, otherwise it'll scale shrinkradius a bit
                    if (c.Ellipse.Radius > 0)
                        newShrink.Add(c); // still good!
                }

                // it is much faster to create a new list of the good circles rather than removing them from an existing list, which does expensive array resizing + other nonsense
                // we have to create a temporary array to do the removal anyway
                shrinkingArtifacts = newShrink;
            }

            public void moveArtifactsByTimestep(double deltaTime)
            {
                if (movingArtifacts.Count > 0)
                {
                    List<ArtifactMoving> newMoving = new List<ArtifactMoving>();

                    foreach (ArtifactMoving a in movingArtifacts)
                    {
                        if (!a.performTimestep(deltaTime))
                            newMoving.Add(a);
                    }

                    // add everything to a new list and replace, it's faster
                    movingArtifacts = newMoving;
                }
            }

            #endregion

            #region Draw Methods

            public void DrawDebug()
            {
                XNA.PushMatrix();
                XNA.Translate(boundingQuad[0]); // move to top-left corner

                //titleText = new SelectableText(font, "Colours", new Vector3(0.4f, 0, 0), bookshelf.GlobalTextColor, Color.White);
                //titleText.InverseScale(0.8f, size.X, size.Y);

                for (int i = 0; i < dimension; i++)
                {
                    for (int j = 0; j < dimension; j++)
                    {
                        XNA.PushMatrix();
                        XNA.Translate(debugText[i, j].Position);
                        debugText[i, j].DrawScale(0.001f);
                        XNA.PopMatrix();
                    }
                }
                
                XNA.PopMatrix();
            }

            public void DrawActiveArtifacts()
            {
                foreach (ColorViewContainer c in shrinkingArtifacts)
                {
                    XNA.PushMatrix();
                    XNA.Translate(c.Ellipse.Position);
                    c.Ellipse.DrawFillBorder(true);
                    //c.Ellipse.DrawFill(true);
                    XNA.PopMatrix();
                }

                // drawing in reverse order puts the closest on top
                for (int i = activeArtifacts.Count - 1; i >= 0; i--)
                {
                    ColorViewContainer c = activeArtifacts[i];
                    XNA.PushMatrix();
                    XNA.Translate(c.Ellipse.Position);
                    //c.Ellipse.DrawFill(true);
                    c.Ellipse.DrawFillBorder(true);
                    XNA.PopMatrix();
                }
            }

            public void DrawSelectable()
            {
                XNA.PushMatrix();
                XNA.Translate(boundingQuad[0]); // move to top-left corner
                boundingQuad.DrawSelectable();
                XNA.PopMatrix();

                foreach (ColorViewContainer c in activeArtifacts)
                {
                    if (c.Ellipse.Id > 0)
                    {
                        XNA.PushMatrix();
                        XNA.Translate(c.Ellipse.Position);
                        c.Ellipse.DrawSelectable();
                        XNA.PopMatrix();
                    }
                }
            }

            #endregion
        }

        class ArtifactGridNode
        {
            int i, j; // indices into artifactgrid
            List<Artifact> artifacts;
            List<Vector2> locations; // a parallel array to artifacts, gives x, y locations relative to the container holding the grid

            public ArtifactGridNode(int i, int j)
            {
                this.i = i;
                this.j = j;
                artifacts = new List<Artifact>();
                locations = new List<Vector2>();
            }

            #region Accessors and Artifact Methods

            public int I
            {
                get { return i; }
            }

            public int J
            {
                get { return j; }
            }

            public void addArtifact(Artifact a, Vector2 loc)
            {
                artifacts.Add(a);
                locations.Add(loc);
            }

            public int ArtifactCount
            {
                get { return artifacts.Count; }
            }

            public List<Artifact> Artifacts
            {
                get { return artifacts; }
            }

            public List<Vector2> Locations
            {
                get { return locations; }
            }

            #endregion
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

        List<ColorViewContainer> colorCircles;
        ColorWheelLattice colorWheelLattice;
        ArtifactGrid artifactGrid;

        private BoundingBox boundingBox;

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

            colorCircles = new List<ColorViewContainer>();
            SelectableQuad boundingQuad = new SelectableQuad(new Vector2(0.0f, 0.0f), new Vector2(1f, 1f), Color.Black);

            // here's the two biggies
            colorWheelLattice = new ColorWheelLattice(9, boundingQuad, bookshelf.Library.Artifacts, bbshelf);
            artifactGrid = new ArtifactGrid(10, boundingQuad, bookshelf.Library.Artifacts, this);

            //setupColorCircles();
            //showColorWheel();
        }

        #region Getters and Setters

        public BohemianArtifact Bookshelf
        {
            get { return bookshelf; }
        }

        #endregion

        // pass in an absolute (screen coordinate) x, y value and it will convert it to relative position (0 to 1) within color view's position/size
        public Vector2 convertTouchAbsoluteToRelative(Vector2 absolutePos)
        {
            absolutePos -= new Vector2(position.X, position.Y); // move everything to 0, 0
            if (absolutePos.X < 0) absolutePos.X = 0;
            if (absolutePos.X > size.X) absolutePos.X = size.X;
            if (absolutePos.Y < 0) absolutePos.Y = 0;
            if (absolutePos.Y > size.Y) absolutePos.Y = size.Y;

            return new Vector2(absolutePos.X / size.X, absolutePos.Y / size.Y);
        }

        #region Debugging Functions
        public void setupColorCircles()
        {
            Dictionary<Color, int> dict = new Dictionary<Color, int>();

            foreach(Artifact a in bookshelf.Library.Artifacts)
            {
                ColorViewContainer c = new ColorViewContainer(new SelectableEllipse(new Vector2(), 0.02f, 0, a.Color, a.Color, null), a);
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
            ColorViewContainer found = null;
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
                colorCircles.Add(new ColorViewContainer(new SelectableEllipse(new Vector2(), 0.02f, 0, new Color(r, g, b), Color.Black, null), null));
                HSVtoRGB(i, 0.75f, 1, out r, out g, out b);
                colorCircles.Add(new ColorViewContainer(new SelectableEllipse(new Vector2(), 0.02f, 0, new Color(r, g, b), Color.Black, null), null));
                HSVtoRGB(i, 0.5f, 1, out r, out g, out b);
                colorCircles.Add(new ColorViewContainer(new SelectableEllipse(new Vector2(), 0.02f, 0, new Color(r, g, b), Color.Black, null), null));
                HSVtoRGB(i, 0.25f, 1, out r, out g, out b);
                colorCircles.Add(new ColorViewContainer(new SelectableEllipse(new Vector2(), 0.02f, 0, new Color(r, g, b), Color.Black, null), null));
            }
            alignCirclesPolar();
        }

        public void alignCirclesLinear()
        {
            float dx = colorCircles[0].Ellipse.Radius;
            float dy = 0.15f;

            foreach (ColorViewContainer c in colorCircles)
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

            CircleConverter converter = new CircleConverter(new SelectableQuad(new Vector2(0f, 0f), new Vector2(1f, 1f), Color.Black), 0.02f);

            foreach (ColorViewContainer c in colorCircles)
            {
                SelectableEllipse se = c.Ellipse;
                float h, s, v;
                RGBtoHSV(se.Color.R, se.Color.G, se.Color.B, out h, out s, out v);
                //double theta = (h + rotate) * Math.PI / 180.0f;
                float theta = h * (float)Math.PI / 180.0f;
                float r = s;

                //Vector3 unitCirclePos = new Vector3(r * (float)Math.Cos(theta), r * (float)Math.Sin(theta), 0);
                //se.Position = unitCirclePos * radius + wheelCenter;
                se.Position = new Vector3(converter.getPointBox(r, theta), 0);
            }
        }

        #endregion

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
            if (bookshelf.TouchPoints.Count > 0 && bookshelf.TouchPoints.ContainsKey(artifactGrid.BoundingQuad.TouchId))
                artifactGrid.boundingQuad_TouchMoved(artifactGrid.BoundingQuad, new TouchArgs(artifactGrid.BoundingQuad.TouchId));

            double delta = time.ElapsedGameTime.TotalSeconds;
            if (delta > 0)
            {
                artifactGrid.shrinkArtifactsByTimestep(delta);
                artifactGrid.moveArtifactsByTimestep(delta);
            }
        }

        void Library_SelectedArtifactChanged(Artifact selectedArtifact)
        {
            artifactGrid.displayArtifact(selectedArtifact);
        }

        #region Draw Methods

        public void Draw()
        {
            XNA.PushMatrix();
            XNA.Translate(position);
            XNA.Scale(size);

            titleText.DrawFill();
            //boundingBox.Draw();

            //foreach (Container c in colorCircles)
            //{
            //    XNA.PushMatrix();
            //    XNA.Translate(c.Ellipse.Position);
            //    c.Ellipse.DrawFill(false);
            //    XNA.PopMatrix();
            //}

            colorWheelLattice.Draw();
            artifactGrid.DrawActiveArtifacts();

            //artifactGrid.DrawDebug();

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
            //colorWheelLattice.DrawSelectable();
            artifactGrid.DrawSelectable();

            XNA.PopMatrix();
        }

        #endregion
    }
}