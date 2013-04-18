using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BohemianBookshelf
{
    public class SelectableQuad : SelectableObject
    {
        protected VertexPositionColorTexture[] screenPoints = new VertexPositionColorTexture[4];
        protected VertexPositionColorTexture[] selectPoints = new VertexPositionColorTexture[4];
        protected int[] indices = { 0, 1, 2, 0, 2, 3 };
        public Vector3 Position;
        public float RotationSpeed;

        public VertexPositionColorTexture[] Points
        {
            get
            {
                return screenPoints;
            }
        }

        public SelectableQuad(Color color)
        {
            for (int i = 0; i < 4; i++)
            {
                screenPoints[i].Color = color;
            }
        }

        public SelectableQuad(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Color color)
            : base()
        {
            screenPoints[0] = new VertexPositionColorTexture(p1, color, Vector2.Zero);
            screenPoints[1] = new VertexPositionColorTexture(p2, color, Vector2.UnitX);
            screenPoints[2] = new VertexPositionColorTexture(p3, color, Vector2.One);
            screenPoints[3] = new VertexPositionColorTexture(p4, color, Vector2.UnitY);
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

        public void DrawSelectable(GraphicsDevice graphicsDevice)
        {
            graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, selectPoints, 0, 4, indices, 0, 2);
        }

        public void DrawSelectable(BasicEffect effect)
        {
            if (selected)
            {
                effect.World = Matrix.CreateScale(2, 2, 2) * Matrix.CreateTranslation(Position);
            }
            else
            {
                effect.World = Matrix.CreateTranslation(Position);
            }
            effect.VertexColorEnabled = true;
            effect.TextureEnabled = false;
            effect.CurrentTechnique.Passes[0].Apply();
            effect.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, selectPoints, 0, 4, indices, 0, 2);
        }

        public void Draw(GraphicsDevice graphicsDevice)
        {
            graphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, screenPoints, 0, 4, indices, 0, 2);
        }

        public void Draw(BasicEffect effect)
        {
            if (selected)
            {
                effect.World = Matrix.CreateScale(2, 2, 2) * Matrix.CreateTranslation(Position);
            }
            else
            {
                effect.World = Matrix.CreateTranslation(Position);
            }
            effect.VertexColorEnabled = true;
            effect.TextureEnabled = false;
            effect.CurrentTechnique.Passes[0].Apply();
            effect.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, screenPoints, 0, 4, indices, 0, 2);
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
