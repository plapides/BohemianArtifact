using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BohemianArtifact
{
    public class SelectableQuad : SelectableObject
    {
        protected VertexPositionColorTexture[] screenPoints = new VertexPositionColorTexture[4];
        protected VertexPositionColorTexture[] selectPoints = new VertexPositionColorTexture[4];
        protected int[] indices = { 0, 1, 2, 0, 2, 3 };
        protected Vector3 center;

        public VertexPositionColorTexture[] Points
        {
            get
            {
                return screenPoints;
            }
        }
        public Color Color
        {
            get
            {
                return screenPoints[0].Color;
            }
            set
            {
                screenPoints[0].Color = value;
                screenPoints[1].Color = value;
                screenPoints[2].Color = value;
                screenPoints[3].Color = value;
            }
        }

        public Vector3 Center
        {
            get { return center; }
        }

        // returns x-coordinate of top of quad
        public float Top
        {
            get { return screenPoints[0].Position.X; }
        }

        protected SelectableQuad(Color color)
        {
            for (int i = 0; i < 4; i++)
            {
                screenPoints[i].Color = color;
            }
        }

        public SelectableQuad(Vector2 position, Vector2 size, Color color)
            : base()
        {
            screenPoints[0] = new VertexPositionColorTexture(new Vector3(position.X, position.Y, 0), color, Vector2.UnitX);
            screenPoints[1] = new VertexPositionColorTexture(new Vector3(position.X + size.X, position.Y, 0), color, Vector2.UnitX);
            screenPoints[2] = new VertexPositionColorTexture(new Vector3(position.X + size.X, position.Y + size.Y, 0), color, Vector2.UnitX);
            screenPoints[3] = new VertexPositionColorTexture(new Vector3(position.X, position.Y + size.Y, 0), color, Vector2.UnitX);
            center = new Vector3(position.X + size.X / 2, position.Y + size.Y / 2, 0);
            Synchronize();
        }

        public SelectableQuad(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Color color)
            : base()
        {
            screenPoints[0] = new VertexPositionColorTexture(p1, color, Vector2.Zero);
            screenPoints[1] = new VertexPositionColorTexture(p2, color, Vector2.UnitX);
            screenPoints[2] = new VertexPositionColorTexture(p3, color, Vector2.One);
            screenPoints[3] = new VertexPositionColorTexture(p4, color, Vector2.UnitY);
            center = 0.5f * (p3 - p1);
            Synchronize();
        }

        public void Synchronize()
        {
            // copy the point positions of the quad to the selectedPoints array so the selectable quad matches the screen quad
            for (int i = 0; i < 4; i++)
            {
                selectPoints[i].Position = screenPoints[i].Position;
            }
        }

        public void SetQuadColor(Color newColor)
        {
            for (int i = 0; i < 4; i++)
            {
                screenPoints[i].Color = newColor;
            }
        }

        public void DrawSelectable()
        {
            graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, selectPoints, 0, 4, indices, 0, 2);
        }

        public void DrawSelectable(BasicEffect effect)
        {
            graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, selectPoints, 0, 4, indices, 0, 2);
        }

        public void Draw()
        {
            Draw(false);
        }

        public void Draw(bool textureEnabled)
        {
            XNA.Texturing = textureEnabled;
            graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, screenPoints, 0, 4, indices, 0, 2);
        }

        public void Draw(BasicEffect effect)
        {
            Draw(effect, false);
        }

        public void Draw(BasicEffect effect, bool textureEnabled)
        {
            //effect.TextureEnabled = textureEnabled;
            //effect.CurrentTechnique.Passes[0].Apply();
            graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, screenPoints, 0, 4, indices, 0, 2);
        }

        public override void SelectableColorChanged()
        {
            for (int i = 0; i < 4; i++)
            {
                selectPoints[i].Color = selectableColor;
            }
            base.SelectableColorChanged();
        }
    }
}
