using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BohemianBookshelf
{
    public class SelectableQuadE : SelectableObject
    {
        private VertexPositionNormalTexture[] screenPoints = new VertexPositionNormalTexture[4];
        private Vector3 color;
        private Vector3 sColor;
        private int[] indices = { 0, 1, 2, 0, 2, 3 };

        public VertexPositionNormalTexture[] Points
        {
            get
            {
                return screenPoints;
            }
        }

        public SelectableQuadE(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4, Color c)
            : base()
        {
            color = new Vector3((float)c.R / 255, (float)c.G / 255, (float)c.B / 255);
            Vector3 upVector = new Vector3(0, 0, 1);
            Vector2 texCoord = new Vector2(0, 0);
            screenPoints[0] = new VertexPositionNormalTexture(p1, upVector, texCoord);
            screenPoints[1] = new VertexPositionNormalTexture(p2, upVector, texCoord);
            screenPoints[2] = new VertexPositionNormalTexture(p3, upVector, texCoord);
            screenPoints[3] = new VertexPositionNormalTexture(p4, upVector, texCoord);
        }

        public void DrawSelectable(BasicEffect effect)
        {
            effect.LightingEnabled = true;
            effect.EmissiveColor = sColor;
            effect.VertexColorEnabled = false;
            foreach (EffectPass ef in effect.CurrentTechnique.Passes)
            {
                ef.Apply();
            }

            effect.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, screenPoints, 0, 4, indices, 0, 2);
        }

        public void Draw(BasicEffect effect)
        {
            effect.LightingEnabled = true;
            effect.EmissiveColor = color;
            effect.VertexColorEnabled = false;
            foreach (EffectPass ef in effect.CurrentTechnique.Passes)
            {
                ef.Apply();
            }

            effect.GraphicsDevice.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, screenPoints, 0, 4, indices, 0, 2);
        }

        public override void SelectableColorChanged()
        {
            sColor.X = (float)selectableColor.R / 255;
            sColor.Y = (float)selectableColor.G / 255;
            sColor.Z = (float)selectableColor.B / 255;
            base.SelectableColorChanged();
        }
    }
}
