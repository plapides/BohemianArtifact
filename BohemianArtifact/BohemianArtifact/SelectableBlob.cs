using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace BohemianArtifact
{
    public class SelectableBlob : SelectableObject
    {
        //*
        private float middleRadius;
        private float circleRadius;
        private Vector3 centerPosition;
        private float spanAngle;
        private float middleAngle;
        private Color color;
        private Color insideEdgeColor;
        private Color outsideEdgeColor;
        private Texture2D texture;

        public float MiddleRadius
        {
            get
            {
                return middleRadius;
            }
            set
            {
                middleRadius = value;
                Recompute();
            }
        }
        public float CircleRadius
        {
            get
            {
                return circleRadius;
            }
            set
            {
                circleRadius = value;
                Recompute();
            }
        }
        public Vector3 CenterPosition
        {
            get
            {
                return centerPosition;
            }
            set
            {
                centerPosition = value;
            }
        }
        public float SpanAngle
        {
            get
            {
                return spanAngle;
            }
            set
            {
                SetSpanAngle(value);
            }
        }
        public float MiddleAngle
        {
            get
            {
                return middleAngle;
            }
            set
            {
                SetMiddleAngle(value);
            }
        }
        public Color Color
        {
            get
            {
                return color;
            }
            set
            {
                SetColor(value);
            }
        }
        public float EdgeThickness
        {
            get
            {
                return edgeThickness;
            }
            set
            {
                edgeThickness = value;
                RecomputeBorder();
            }
        }

        private void SetColor(Color color)
        {
            this.color = color;
            for (int i = 0; i < subdivisions + 1; i++)
            {
                leftPoints[i].Color = color;
                rightPoints[i].Color = color;
            }
            for (int i = 0; i < subdivisions * 2; i++)
            {
                middlePoints[i].Color = color;
            }
        }
        public void SetBorderColors(Color inside, Color outside)
        {
            this.insideEdgeColor = inside;
            this.outsideEdgeColor = outside;
            for (int i = 0; i < subdivisions; i++)
            {
                leftEdgePoints[i].Color = outside;
                leftEdgePoints[i + subdivisions].Color = inside;
                rightEdgePoints[i].Color = outside;
                rightEdgePoints[i + subdivisions].Color = inside;
                outerEdgePoints[i].Color = outside;
                outerEdgePoints[i + subdivisions].Color = inside;
                innerEdgePoints[i].Color = outside;
                innerEdgePoints[i + subdivisions].Color = inside;
            }
        }

        private int subdivisions = 32;

        // this object is split into three main segments: left semicircle, middle, and right semicircle
        private VertexPositionColorTexture[] leftPoints;
        private VertexPositionColorTexture[] middlePoints;
        private VertexPositionColorTexture[] rightPoints;
        // mirror the same structure for the selection vertex data
        private VertexPositionColorTexture[] selectLeftPoints;
        private VertexPositionColorTexture[] selectMiddlePoints;
        private VertexPositionColorTexture[] selectRightPoints;
        // same for indices - these will do for both screen and select points since they have the same data (just different colors)
        private int[] leftIndices;
        private int[] middleIndices;
        private int[] rightIndices;

        // these are for drawing thick edges
        private float edgeThickness;
        private VertexPositionColor[] leftEdgePoints;
        private VertexPositionColor[] rightEdgePoints;
        private VertexPositionColor[] outerEdgePoints;
        private VertexPositionColor[] innerEdgePoints;
        private int[] leftEdgeIndices;
        private int[] rightEdgeIndices;
        private int[] outerEdgeIndices;
        private int[] innerEdgeIndices;

        public SelectableBlob(Vector2 start, Vector2 end, float curveDistance, float circleRadius, float edgeThickness, Color color, Color insideEdgeColor, Color outsideEdgeColor, ref float angle)
        {
            if (curveDistance < 0)
            {
                curveDistance *= -1;
                Vector2 temp = end;
                end = start;
                start = temp;
            }
            Vector3 chord = new Vector3((end - start), 0);
            float chordLength = chord.Length();
            float midRadius = (chordLength * chordLength / 4 - curveDistance * curveDistance) / (2 * curveDistance);
            float middleRadius = curveDistance + midRadius;
            Vector3 chordMidPoint = new Vector3((end + start) / 2, 0);
            Vector3 toCentre = Vector3.Cross(chord, Vector3.UnitZ);
            toCentre.Normalize();
            toCentre *= midRadius;
            Vector3 centre = chordMidPoint + toCentre;
            float spanAngle = 2 * (float)Math.Asin(chordLength / (2 * middleRadius));
            // angle calculation
            angle = (float)Math.Atan2(-toCentre.Y, -toCentre.X);


            Initialize(new Vector2(centre.X, centre.Y), spanAngle, circleRadius, middleRadius, edgeThickness, color, insideEdgeColor, outsideEdgeColor, null);
        }
        
        public SelectableBlob(Vector2 centerPosition, float circleRadius, Color color, Texture2D texture)
        {
            Initialize(centerPosition, spanAngle, circleRadius, 0, 0, color, color, color, texture);
            SetMiddleAngle(0);
            Recompute();
        }

        public SelectableBlob(Vector2 centerPosition, float spanAngle, float circleRadius, float middleRadius, float edgeThickness, Color color, Color insideEdgeColor, Color outsideEdgeColor, Texture2D texture)
        {
            Initialize(centerPosition, spanAngle, circleRadius, middleRadius, edgeThickness, color, insideEdgeColor, outsideEdgeColor, texture);
        }

        private void Initialize(Vector2 centerPosition, float spanAngle, float circleRadius, float middleRadius, float edgeThickness, Color color, Color insideEdgeColor, Color outsideEdgeColor, Texture2D texture)
        {
            this.middleRadius = middleRadius;
            this.circleRadius = circleRadius;
            this.centerPosition = new Vector3(centerPosition, 0);
            this.spanAngle = spanAngle;
            this.color = color;
            this.texture = texture;
            this.insideEdgeColor = insideEdgeColor;
            this.outsideEdgeColor = outsideEdgeColor;
            this.edgeThickness = edgeThickness;

            SetSpanAngle(spanAngle);

            // the last point in this array (what the +1 is for) is the centre of the semicircle
            leftPoints = new VertexPositionColorTexture[subdivisions + 1];
            rightPoints = new VertexPositionColorTexture[subdivisions + 1];
            middlePoints = new VertexPositionColorTexture[subdivisions * 2];
            leftIndices = new int[3 * subdivisions];
            rightIndices = new int[3 * subdivisions];
            middleIndices = new int[3 * (subdivisions - 1) * 2];

            selectLeftPoints = new VertexPositionColorTexture[leftPoints.Length];
            selectRightPoints = new VertexPositionColorTexture[rightPoints.Length];
            selectMiddlePoints = new VertexPositionColorTexture[middlePoints.Length];

            // this is to draw a nice thick edge around the circle. (the +1 is to have the last set of points overlap with the first set for linestrips)
            leftEdgePoints = new VertexPositionColor[subdivisions * 2];
            rightEdgePoints = new VertexPositionColor[subdivisions * 2];
            outerEdgePoints = new VertexPositionColor[subdivisions * 2];
            innerEdgePoints = new VertexPositionColor[subdivisions * 2];
            leftEdgeIndices = new int[3 * (subdivisions - 1) * 2];
            rightEdgeIndices = new int[3 * (subdivisions - 1) * 2];
            outerEdgeIndices = new int[3 * (subdivisions - 1) * 2];
            innerEdgeIndices = new int[3 * (subdivisions - 1) * 2];

            ComputeAllIndices();
            Recompute();
        }

        public void SetDimensions(float spanAngle, float circleRadius, float middleRadius)
        {
            SetSpanAngle(spanAngle);
            this.circleRadius = circleRadius;
            this.middleRadius = middleRadius;
            Recompute();
        }

        private void ComputeAllIndices()
        {
            // MAIN indices
            // compute left/right indices
            for (int i = 0; i < subdivisions; i++)
            {
                // set the left indices
                // set the 3 indices starting at the current (i'th) point
                leftIndices[i * 3 + 0] = i;
                // moving to the next one (i+1, taking into account wrap around)
                leftIndices[i * 3 + 1] = (i + 1) % subdivisions;
                // and ending always with the centre point (index id: 'subdivisions')
                leftIndices[i * 3 + 2] = subdivisions;
            }
            // duplicate the left indices to the right indices. (same point order, same index order)
            leftIndices.CopyTo(rightIndices, 0);

            // middle indices
            for (int i = 0; i < subdivisions - 1; i++)
            {
                // handle the indices
                middleIndices[i * 6 + 0] = i;
                middleIndices[i * 6 + 1] = i + subdivisions;
                middleIndices[i * 6 + 2] = i + subdivisions + 1;
                middleIndices[i * 6 + 3] = i;
                middleIndices[i * 6 + 4] = i + subdivisions + 1;
                middleIndices[i * 6 + 5] = i + 1;
            }

            // BORDER indices
            // compute the left and right border indices
            for (int i = 0; i < subdivisions - 1; i++)
            {
                rightEdgeIndices[i * 6 + 0] = i;
                rightEdgeIndices[i * 6 + 1] = i + 1;
                rightEdgeIndices[i * 6 + 2] = i + subdivisions + 1;
                rightEdgeIndices[i * 6 + 3] = i;
                rightEdgeIndices[i * 6 + 4] = i + subdivisions + 1;
                rightEdgeIndices[i * 6 + 5] = i + subdivisions;
            }
            rightEdgeIndices.CopyTo(leftEdgeIndices, 0);
            // compute the middle border indices
            for (int i = 0; i < subdivisions - 1; i++)
            {
                innerEdgeIndices[i * 6 + 0] = i;
                innerEdgeIndices[i * 6 + 1] = i + subdivisions;
                innerEdgeIndices[i * 6 + 2] = i + 1;
                innerEdgeIndices[i * 6 + 3] = i + 1;
                innerEdgeIndices[i * 6 + 4] = i + subdivisions;
                innerEdgeIndices[i * 6 + 5] = i + subdivisions + 1;

                outerEdgeIndices[i * 6 + 0] = i;
                outerEdgeIndices[i * 6 + 1] = i + 1;
                outerEdgeIndices[i * 6 + 2] = i + subdivisions + 1;
                outerEdgeIndices[i * 6 + 3] = i;
                outerEdgeIndices[i * 6 + 4] = i + subdivisions + 1;
                outerEdgeIndices[i * 6 + 5] = i + subdivisions;
            }
        }

        public void Recompute()
        {
            Vector3 p;
            p.Z = 0;
            Vector2 t;

            // first compute the left and right circles (fill)
            for (int i = 0; i < subdivisions; i++)
            {
                // compute the point coordinates.
                p.X = circleRadius * (float)Math.Cos(i * Math.PI / (subdivisions - 1));
                p.Y = circleRadius * (float)Math.Sin(i * Math.PI / (subdivisions - 1));
                // forget about the texture coordinates right now

                // the left circle's points are mirrored (-p)
                rightPoints[i] = new VertexPositionColorTexture(p, color, Vector2.Zero);
                leftPoints[i] = new VertexPositionColorTexture(-p, color, Vector2.Zero);
            }
            // this is the centre of the circle point
            leftPoints[subdivisions] = new VertexPositionColorTexture(Vector3.Zero, color, Vector2.Zero);
            rightPoints[subdivisions] = new VertexPositionColorTexture(Vector3.Zero, color, Vector2.Zero);

            // this translates/rotates the left and right semicircles to the proper position at the ends of the middle section
            // use subdivisions + 1 so that we also translate the center point of each semicircle
            for (int i = 0; i < subdivisions + 1; i++)
            {
                leftPoints[i].Position = Vector3.Transform(leftPoints[i].Position, Matrix.CreateTranslation(middleRadius, 0, 0) * Matrix.CreateRotationZ(-middleAngle / 2));
                rightPoints[i].Position = Vector3.Transform(rightPoints[i].Position, Matrix.CreateTranslation(middleRadius, 0, 0) * Matrix.CreateRotationZ(middleAngle / 2));
                // at this point we can set up our selectPoints arrays
                selectLeftPoints[i].Position = leftPoints[i].Position;
                selectRightPoints[i].Position = rightPoints[i].Position;
            }

            // set up the textures (ADD MORE COMMENTS HERE)
            for (int i = 0; i < subdivisions + 1; i++)
            {
                t.X = leftPoints[i].Position.X;
                t.Y = leftPoints[i].Position.Y;
                t = Vector2.Transform(t, Matrix.CreateScale(1 / (middleRadius + circleRadius)));
                t.X = (t.X + 1) / 2;
                t.Y = (t.Y + 1) / 2;
                leftPoints[i].TextureCoordinate = t;

                t.X = rightPoints[i].Position.X;
                t.Y = rightPoints[i].Position.Y;
                t = Vector2.Transform(t, Matrix.CreateScale(1 / (middleRadius + circleRadius)));
                t.X = (t.X + 1) / 2;
                t.Y = (t.Y + 1) / 2;
                rightPoints[i].TextureCoordinate = t;
            }

            // set up the middle part of the blob
            RecomputeMiddle();

            RecomputeBorder();
        }

        private void RecomputeMiddle()
        {
            Vector3 p;
            p.Z = 0;
            Vector2 t;

            //*
            // compute the middle section for fill
            for (int i = 0; i < subdivisions; i++)
            {
                // compute the inner point coordinates.
                p.X = (middleRadius - circleRadius) * (float)Math.Cos(middleAngle * i / (subdivisions - 1) - middleAngle / 2);
                p.Y = (middleRadius - circleRadius) * (float)Math.Sin(middleAngle * i / (subdivisions - 1) - middleAngle / 2);
                middlePoints[i] = new VertexPositionColorTexture(p, color, Vector2.Zero); // forget about the texture coords, do that later

                // compute the outer point coordinates.
                p.X = (middleRadius + circleRadius) * (float)Math.Cos(middleAngle * i / (subdivisions - 1) - middleAngle / 2);
                p.Y = (middleRadius + circleRadius) * (float)Math.Sin(middleAngle * i / (subdivisions - 1) - middleAngle / 2);
                middlePoints[i + subdivisions] = new VertexPositionColorTexture(p, color, Vector2.Zero); // forget about texture coord

                // at this point we can set up our selectPoints array for the middle
                selectMiddlePoints[i].Position = middlePoints[i].Position;
                selectMiddlePoints[i + subdivisions].Position = middlePoints[i + subdivisions].Position;
            }

            // compute texture coordinates
            for (int i = 0; i < subdivisions; i++)
            {
                // do the inner radius first
                t.X = middlePoints[i].Position.X;
                t.Y = middlePoints[i].Position.Y;
                t = Vector2.Transform(t, Matrix.CreateScale(1 / (middleRadius + circleRadius)));
                t.X = (t.X + 1) / 2;
                t.Y = (t.Y + 1) / 2;
                middlePoints[i].TextureCoordinate = t;

                // then the outer radius
                t.X = middlePoints[i + subdivisions].Position.X;
                t.Y = middlePoints[i + subdivisions].Position.Y;
                t = Vector2.Transform(t, Matrix.CreateScale(1 / (middleRadius + circleRadius)));
                t.X = (t.X + 1) / 2;
                t.Y = (t.Y + 1) / 2;
                middlePoints[i + subdivisions].TextureCoordinate = t;
            }
        }

        private void RecomputeBorder()
        {
            Vector3 p = new Vector3(0, 0, 0);
            // compute the left and right border points
            for (int i = 0; i < subdivisions; i++)
            {
                // the outside edge of the border
                p.X = circleRadius * (float)Math.Cos(i * Math.PI / (subdivisions - 1));
                p.Y = circleRadius * (float)Math.Sin(i * Math.PI / (subdivisions - 1));
                rightEdgePoints[i] = new VertexPositionColor(p, outsideEdgeColor);
                leftEdgePoints[i] = new VertexPositionColor(-p, outsideEdgeColor);
                // the inside edge of the border
                p.X = (circleRadius - edgeThickness) * (float)Math.Cos(i * Math.PI / (subdivisions - 1));
                p.Y = (circleRadius - edgeThickness) * (float)Math.Sin(i * Math.PI / (subdivisions - 1));
                rightEdgePoints[i + subdivisions] = new VertexPositionColor(p, insideEdgeColor);
                leftEdgePoints[i + subdivisions] = new VertexPositionColor(-p, insideEdgeColor);

                // transform the edge points to the correct position (away from origin)
                rightEdgePoints[i].Position = Vector3.Transform(rightEdgePoints[i].Position,
                    Matrix.CreateTranslation(middleRadius, 0, 0) * Matrix.CreateRotationZ(middleAngle / 2));
                rightEdgePoints[i + subdivisions].Position = Vector3.Transform(rightEdgePoints[i + subdivisions].Position,
                    Matrix.CreateTranslation(middleRadius, 0, 0) * Matrix.CreateRotationZ(middleAngle / 2));
                leftEdgePoints[i].Position = Vector3.Transform(leftEdgePoints[i].Position,
                    Matrix.CreateTranslation(middleRadius, 0, 0) * Matrix.CreateRotationZ(-middleAngle / 2));
                leftEdgePoints[i + subdivisions].Position = Vector3.Transform(leftEdgePoints[i + subdivisions].Position,
                    Matrix.CreateTranslation(middleRadius, 0, 0) * Matrix.CreateRotationZ(-middleAngle / 2));
            }

            // compute the middle border points
            for (int i = 0; i < subdivisions; i++)
            {
                // compute the inner border coordinates. that is, the border that is closest to the center position
                // first the outside edge of the border
                p.X = (middleRadius - circleRadius) * (float)Math.Cos(middleAngle * i / (subdivisions - 1) - middleAngle / 2);
                p.Y = (middleRadius - circleRadius) * (float)Math.Sin(middleAngle * i / (subdivisions - 1) - middleAngle / 2);
                innerEdgePoints[i] = new VertexPositionColor(p, outsideEdgeColor);
                // then the inside edge of the border
                p.X = (middleRadius - circleRadius + edgeThickness) * (float)Math.Cos(middleAngle * i / (subdivisions - 1) - middleAngle / 2);
                p.Y = (middleRadius - circleRadius + edgeThickness) * (float)Math.Sin(middleAngle * i / (subdivisions - 1) - middleAngle / 2);
                innerEdgePoints[i + subdivisions] = new VertexPositionColor(p, insideEdgeColor);

                // then the outer border coordinates. the border furthest from the center position
                // first the outside edge of the border
                p.X = (middleRadius + circleRadius) * (float)Math.Cos(middleAngle * i / (subdivisions - 1) - middleAngle / 2);
                p.Y = (middleRadius + circleRadius) * (float)Math.Sin(middleAngle * i / (subdivisions - 1) - middleAngle / 2);
                outerEdgePoints[i] = new VertexPositionColor(p, outsideEdgeColor);
                // then the inside edge of the border
                p.X = (middleRadius + circleRadius - edgeThickness) * (float)Math.Cos(middleAngle * i / (subdivisions - 1) - middleAngle / 2);
                p.Y = (middleRadius + circleRadius - edgeThickness) * (float)Math.Sin(middleAngle * i / (subdivisions - 1) - middleAngle / 2);
                outerEdgePoints[i + subdivisions] = new VertexPositionColor(p, insideEdgeColor);
            }

        }

        private void SetSpanAngle(float span)
        {
            // set the span angle
            spanAngle = span;
            // and compute the middle angle
            if (middleRadius == 0)
            {
                // if this blob is acting like a circle (e.g. no middle radius)
                middleAngle = 0;
                spanAngle = 0;
            }
            else
            {
                middleAngle = (spanAngle * middleRadius - 2 * circleRadius) / middleRadius; // trivial derivation
            }

            if (middleAngle < 0)
            {
                SetMiddleAngle(0);
            }
        }

        private void SetMiddleAngle(float middle)
        {
            // set the middle angle
            middleAngle = middle;
            // and compute the span angle
            if (middleRadius == 0)
            {
                // if this blob is acting like a circle (e.g. no middle radius)
                middleAngle = 0;
                spanAngle = 0;
            }
            else
            {
                spanAngle = (middleAngle * middleRadius + 2 * circleRadius) / middleRadius; // trivial derivation
            }
        }

        public void DrawFill()
        {
            DrawFill(false);
        }

        public void DrawFill(bool texturingEnabled)
        {
            if (texturingEnabled == true && texture != null)
            {
                XNA.Texture = texture;
                XNA.Texturing = true;
            }
            else
            {
                XNA.Texturing = false;
            }

            // draw the left and right semicircles, and the middle section
            graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, leftPoints, 0, subdivisions + 1, leftIndices, 0, subdivisions - 1);
            graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, rightPoints, 0, subdivisions + 1, rightIndices, 0, subdivisions - 1);
            graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, middlePoints, 0, subdivisions * 2, middleIndices, 0, (subdivisions - 1) * 2);
        }

        public void DrawBorder()
        {
            XNA.Texturing = false;
            graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, leftEdgePoints, 0, subdivisions * 2, leftEdgeIndices, 0, (subdivisions - 1) * 2);
            graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, rightEdgePoints, 0, subdivisions * 2, rightEdgeIndices, 0, (subdivisions - 1) * 2);
            graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, innerEdgePoints, 0, subdivisions * 2, innerEdgeIndices, 0, (subdivisions - 1) * 2);
            graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, outerEdgePoints, 0, subdivisions * 2, outerEdgeIndices, 0, (subdivisions - 1) * 2);
        }

        public void DrawFillBorder()
        {
            DrawFillBorder(false);
        }

        public void DrawFillBorder(bool texturingEnabled)
        {
            DrawFill(texturingEnabled);
            DrawBorder();
        }

        /*
        public void DrawEdge()
        {
            XNA.Texturing = false;
            XNA.PushMatrix();
            XNA.Translate(centerPosition);
            //XNA.RotateZ(spanAngle);
            // do not forget that the number of points is subdivisions + 1 (+1 accounts for the centre point)
            graphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, middlePoints, 0, subdivisions - 1);
            graphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, middlePoints, subdivisions, subdivisions - 1);
            XNA.PopMatrix();
        }

        public void DrawOutsideEdge()
        {
            XNA.PushMatrix();
            XNA.Translate(position);
            graphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, edgePoints, 0, subdivisions);
            XNA.PopMatrix();
        }

        public void DrawInsideEdge()
        {
            XNA.PushMatrix();
            XNA.Translate(position);
            // do not forget that the number of points in the outer edge is subdivisions + 1 (+1 accounts for the overlap point)
            graphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, edgePoints, subdivisions + 1, subdivisions);
            XNA.PopMatrix();
        }
        //*/

        public void DrawSelectable()
        {
            // draw the left and right semicircles, and the middle section
            graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, selectLeftPoints, 0, subdivisions + 1, leftIndices, 0, subdivisions - 1);
            graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, selectRightPoints, 0, subdivisions + 1, rightIndices, 0, subdivisions - 1);
            graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, selectMiddlePoints, 0, subdivisions * 2, middleIndices, 0, (subdivisions - 1) * 2);
        }

        public override void SelectableColorChanged()
        {
            for (int i = 0; i < subdivisions; i++)
            {
                selectLeftPoints[i].Color = selectableColor;
                selectRightPoints[i].Color = selectableColor;
                selectMiddlePoints[i].Color = selectableColor;
                selectMiddlePoints[i + subdivisions].Color = selectableColor;
            }
            selectLeftPoints[subdivisions].Color = selectableColor;
            selectRightPoints[subdivisions].Color = selectableColor;

            base.SelectableColorChanged();
        }
    }
}