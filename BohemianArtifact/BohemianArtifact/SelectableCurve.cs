using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace BohemianArtifact
{
    public class SelectableCurve : SelectableObject
    {
        private Color color;

        private int subdivisions = 2;
        private int iterations = 2;
        private VertexPositionColorTexture[] leftPoints;
        private Vector3[][] intermediary;
        private VertexPositionColorTexture[] screenPoints;
        private VertexPositionColorTexture[] selectPoints;
        private float[] cornerAlpha;
        private int[] indices;

        public float Alpha
        {
            get
            {
                return (float)screenPoints[0].Color.A / 255;
            }
            set
            {
                SetAlpha(value);
            }
        }
        public float TopLeftAlpha
        {
            set
            {
                cornerAlpha[0] = value;
            }
        }
        public float TopRightAlpha
        {
            set
            {
                cornerAlpha[1] = value;
            }
        }
        public float BottomLeftAlpha
        {
            set
            {
                cornerAlpha[3] = value;
            }
        }
        public float BottomRightAlpha
        {
            set
            {
                cornerAlpha[2] = value;
            }
        }

        public SelectableCurve(Color color)
        {
            this.color = color;

            screenPoints = new VertexPositionColorTexture[subdivisions * 2];
            selectPoints = new VertexPositionColorTexture[subdivisions * 2];
            indices = new int[3 * subdivisions];

            cornerAlpha = new float[4] { 1, 1, 1, 1 };

            // create left points array and initialize each vertex
            leftPoints = new VertexPositionColorTexture[GetNumPointsAfterIterations(6, iterations)];
            for (int i = 0; i < leftPoints.Length; i++)
            {
                leftPoints[i] = new VertexPositionColorTexture(Vector3.Zero, color, Vector2.Zero);
            }
            intermediary = new Vector3[iterations + 1][];
            for (int i = 0; i < iterations + 1; i++)
            {
                intermediary[i] = new Vector3[GetNumPointsAfterIterations(6, i)];
            }

            screenPoints[0] = new VertexPositionColorTexture(Vector3.Zero, color, Vector2.Zero);
            screenPoints[1] = new VertexPositionColorTexture(Vector3.Zero, color, Vector2.Zero);
            screenPoints[2] = new VertexPositionColorTexture(Vector3.Zero, color, Vector2.Zero);
            screenPoints[3] = new VertexPositionColorTexture(Vector3.Zero, color, Vector2.Zero);

            selectPoints[0] = new VertexPositionColorTexture(Vector3.Zero, color, Vector2.Zero);
            selectPoints[1] = new VertexPositionColorTexture(Vector3.Zero, color, Vector2.Zero);
            selectPoints[2] = new VertexPositionColorTexture(Vector3.Zero, color, Vector2.Zero);
            selectPoints[3] = new VertexPositionColorTexture(Vector3.Zero, color, Vector2.Zero);

            ComputeIndices();
        }

        private int GetNumPointsAfterIterations(int start, int iter)
        {
            for (int i = 0; i < iter; i++)
            {
                start = (start - 2) * 2 + 2;
            }
            return start;
        }

        private void ComputeIndices()
        {
            indices[0] = 0;
            indices[1] = 1;
            indices[2] = 2;
            indices[3] = 2;
            indices[4] = 3;
            indices[5] = 0;
        }

        public void RecomputeChaikin(float topHeight, float topLeft, float topRight, float bottomHeight, float bottomLeft, float bottomRight)
        {
            float heightDiff = (bottomHeight - topHeight) * 0.05f;

            intermediary[0][0] = new Vector3(topLeft, topHeight, 0);
            intermediary[0][1] = new Vector3(topLeft, heightDiff + topHeight, 0);
            intermediary[0][2] = new Vector3((topLeft + topRight) / 2, heightDiff * 2 + topHeight, 0);
            intermediary[0][3] = new Vector3((bottomLeft + bottomRight) / 2, -heightDiff * 2 + bottomHeight, 0);
            intermediary[0][4] = new Vector3(bottomLeft, -heightDiff + bottomHeight, 0);
            intermediary[0][5] = new Vector3(bottomLeft, bottomHeight, 0);

        }

        public void Recompute(float topHeight, float topLeft, float topRight, float bottomHeight, float bottomLeft, float bottomRight)
        {
            screenPoints[0].Position.Y = topHeight;
            screenPoints[1].Position.Y = topHeight;
            screenPoints[2].Position.Y = bottomHeight;
            screenPoints[3].Position.Y = bottomHeight;

            screenPoints[0].Position.X = topLeft;
            screenPoints[1].Position.X = topRight;
            screenPoints[2].Position.X = bottomRight;
            screenPoints[3].Position.X = bottomLeft;

            selectPoints[0].Position.Y = topHeight;
            selectPoints[1].Position.Y = topHeight;
            selectPoints[2].Position.Y = bottomHeight;
            selectPoints[3].Position.Y = bottomHeight;

            selectPoints[0].Position.X = topLeft;
            selectPoints[1].Position.X = topRight;
            selectPoints[2].Position.X = bottomRight;
            selectPoints[3].Position.X = bottomLeft;
        }

        private void SetAlpha(float alpha)
        {
            for (int i = 0; i < 4; i++)
            {
                screenPoints[i].Color.A = (byte)(alpha * cornerAlpha[i] * 255);
            }
        }

        public void Draw()
        {
            XNA.Texturing = false;
            graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, screenPoints, 0, subdivisions * 2, indices, 0, subdivisions);
        }

        public void DrawSelectable()
        {
            XNA.Texturing = false;
            graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, selectPoints, 0, subdivisions * 2, indices, 0, subdivisions);
        }

        public override void SelectableColorChanged()
        {
            for (int i = 0; i < subdivisions * 2; i++)
            {
                selectPoints[i].Color = selectableColor;
            }
            base.SelectableColorChanged();
        }

    }
}