using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BohemianBookshelf
{
    public class SelectableLine : SelectableQuad
    {
        private VertexPositionColorTexture[] linePoints = new VertexPositionColorTexture[2];
        private float selectableThickness;

        public VertexPositionColorTexture[] LinePoints
        {
            get
            {
                return linePoints;
            }
        }

        public float SelectableThickness
        {
            get
            {
                return selectableThickness;
            }
            set
            {
                selectableThickness = value;
            }
        }

        public SelectableLine(Color color)
            : base(color)
        {
            linePoints[0].Color = color;
            linePoints[1].Color = color;
            selectableThickness = 1;
        }

        public SelectableLine(Vector3 start, Vector3 end, Color color, float thickness)
            : base(color)
        {
            linePoints[0] = new VertexPositionColorTexture(start, color, Vector2.Zero);
            linePoints[1] = new VertexPositionColorTexture(end, color, Vector2.Zero);
            selectableThickness = thickness;
            Recompute();
        }

        public void Recompute()
        {
            Vector3 line = linePoints[1].Position - linePoints[0].Position;
            Vector3 cross = Vector3.Cross(line, Vector3.UnitZ);
            cross.Normalize();
            cross *= selectableThickness / 2;

            screenPoints[0].Position = linePoints[0].Position - cross;
            screenPoints[1].Position = linePoints[0].Position + cross;
            screenPoints[2].Position = linePoints[1].Position + cross;
            screenPoints[3].Position = linePoints[1].Position - cross;

            // this is to synchronize the selectPoints in the SelectableQuad base class
            Synchronize();
        }

        public new void Draw(GraphicsDevice graphicsDevice)
        {
            graphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, linePoints, 0, 1);
        }
    }
}
