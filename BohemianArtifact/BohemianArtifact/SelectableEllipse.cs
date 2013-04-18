using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace BohemianArtifact
{
    public class SelectableEllipse : SelectableObject
    {
        private float radius;
        private float edgeThickness;
        private Vector3 position;
        private Color color;
        private Color insideEdgeColor;
        private Color outsideEdgeColor;
        private Texture2D texture;

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
        public Texture2D Texture
        {
            get
            {
                return texture;
            }
            set
            {
                texture = value;
            }
        }
        public float Radius
        {
            get
            {
                return radius;
            }
            set
            {
                SetRadius(value);
            }
        }

        private int subdivisions = 64;
        private VertexPositionColorTexture[] screenPoints;
        private VertexPositionColorTexture[] selectPoints;
        private int[] indices;
        // these are for drawing thick edges
        private float edgeRadius;
        private VertexPositionColor[] edgePoints;
        private int[] edgeIndices;

        private void SetRadius(float radius)
        {
            this.radius = radius;
            this.edgeRadius = radius - edgeThickness;
            Recompute();
        }

        public SelectableEllipse(Vector2 pos, float rad, float edgeThickness, Color col, Color edgeCol, Texture2D tex)
        {
            this.radius = rad;
            this.position = new Vector3(pos, 0);
            this.color = col;
            this.texture = tex;
            this.insideEdgeColor = edgeCol;
            this.outsideEdgeColor = edgeCol;
            this.edgeThickness = edgeThickness;
            this.edgeRadius = radius - edgeThickness;

            // the last point in this array (what the +1 is for) is the centre of the circle
            screenPoints = new VertexPositionColorTexture[subdivisions + 1];
            selectPoints = new VertexPositionColorTexture[subdivisions + 1];
            indices = new int[3 * subdivisions];
            // this is to draw a nice thick edge around the circle. (the +1 is to have the last set of points overlap with the first set for linestrips)
            edgePoints = new VertexPositionColor[(subdivisions + 1) * 2];
            edgeIndices = new int[3 * subdivisions * 2];

            Recompute();
        }

        public SelectableEllipse(Vector2 pos, float rad, float edgeThickness, Color col, Color insideEdgeCol, Color outsideEdgeCol, Texture2D tex)
        {
            this.radius = rad;
            this.position = new Vector3(pos, 0);
            this.color = col;
            this.texture = tex;
            this.insideEdgeColor = insideEdgeCol;
            this.outsideEdgeColor = outsideEdgeCol;
            this.edgeThickness = edgeThickness;
            this.edgeRadius = radius - edgeThickness;

            // the last point in this array (what the +1 is for) is the centre of the circle
            screenPoints = new VertexPositionColorTexture[subdivisions + 1];
            selectPoints = new VertexPositionColorTexture[subdivisions + 1];
            indices = new int[3 * subdivisions];
            // this is to draw a nice thick edge around the circle. (the +1 is to have the last set of points overlap with the first set for linestrips)
            edgePoints = new VertexPositionColor[(subdivisions + 1) * 2];
            edgeIndices = new int[3 * subdivisions * 2];

            Recompute();
        }

        public void Recompute()
        {
            Vector3 p;
            p.Z = 0;
            Vector2 t;
            // first compute the entire circle (fill)
            for (int i = 0; i < subdivisions; i++)
            {
                // compute the point coordinates
                p.X = radius * (float)Math.Cos(i * 2 * Math.PI / subdivisions);
                p.Y = radius * (float)Math.Sin(i * 2 * Math.PI / subdivisions);
                // compute the texture coordinates
                t.X = (float)Math.Cos(i * 2 * Math.PI / subdivisions) * 0.5f + 0.5f;
                t.Y = (float)Math.Sin(i * 2 * Math.PI / subdivisions) * 0.5f + 0.5f;
                screenPoints[i] = new VertexPositionColorTexture(p, color, t);
                selectPoints[i] = screenPoints[i];

                // set the 3 indices starting at the current (i'th) point
                indices[i * 3 + 0] = i;
                // moving to the next one (i+1, taking into account wrap around)
                indices[i * 3 + 1] = (i + 1) % subdivisions;
                // and ending always with the centre point (index id: 'subdivisions')
                indices[i * 3 + 2] = subdivisions;
            }
            // this is the centre of the circle
            screenPoints[subdivisions] = new VertexPositionColorTexture(Vector3.Zero, color, new Vector2(0.5f, 0.5f));
            selectPoints[subdivisions] = screenPoints[subdivisions];

            // now compute the edge points
            for (int i = 0; i < subdivisions + 1; i++)
            {
                // first compute the outer points
                p.X = radius * (float)Math.Cos(i * 2 * Math.PI / subdivisions);
                p.Y = radius * (float)Math.Sin(i * 2 * Math.PI / subdivisions);
                edgePoints[i] = new VertexPositionColor(p, outsideEdgeColor);
                // then the inner points (store in 2nd half of array)
                p.X = edgeRadius * (float)Math.Cos(i * 2 * Math.PI / subdivisions);
                p.Y = edgeRadius * (float)Math.Sin(i * 2 * Math.PI / subdivisions);
                edgePoints[i + subdivisions + 1] = new VertexPositionColor(p, insideEdgeColor);
            }
            // now compute the indices for the edge
            // we don't want to add indices to the last set of points (the ones that overlap)
            // so we have a separate for loop with a different condition
            for (int i = 0; i < subdivisions; i++)
            {
                // set the indices starting at these two points (inner and outer) and extending to the next two
                // first triangle
                edgeIndices[i * 6 + 0] = i;
                edgeIndices[i * 6 + 1] = i + 1;
                edgeIndices[i * 6 + 2] = i + subdivisions + 2;
                // second triangle
                edgeIndices[i * 6 + 3] = i;
                edgeIndices[i * 6 + 4] = i + subdivisions + 2;
                edgeIndices[i * 6 + 5] = i + subdivisions + 1;
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
            // do not forget that the number of points is subdivisions + 1 (+1 accounts for the centre point)
            graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, screenPoints, 0, subdivisions + 1, indices, 0, subdivisions);
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

        public void DrawBorder()
        {
            XNA.Texturing = false;
            // do not forget that the number of points is subdivisions + 1 (+1 accounts for the centre point)
            graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, edgePoints, 0, (subdivisions + 1) * 2, edgeIndices, 0, subdivisions * 2);
        }

        public void DrawOutsideEdge()
        {
            graphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, edgePoints, 0, subdivisions);
        }

        public void DrawInsideEdge()
        {
            // do not forget that the number of points in the outer edge is subdivisions + 1 (+1 accounts for the overlap point)
            graphicsDevice.DrawUserPrimitives(PrimitiveType.LineStrip, edgePoints, subdivisions + 1, subdivisions);
        }

        public void DrawSelectable()
        {
            // do not forget that the number of points is subdivisions + 1 (+1 accounts for the centre point)
            graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, selectPoints, 0, subdivisions + 1, indices, 0, subdivisions);
        }

        public override void SelectableColorChanged()
        {
            for (int i = 0; i < subdivisions + 1; i++)
            {
                selectPoints[i].Color = selectableColor;
            }
            base.SelectableColorChanged();
        }

    }
}