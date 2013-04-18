using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BohemianArtifact
{
    public class TouchReleaseEventArgs : EventArgs
    {
        private int touchId;
        public int TouchId
        {
            get
            {
                return touchId;
            }
        }
        public TouchReleaseEventArgs(int touchId)
        {
            this.touchId = touchId;
        }
    }

    public delegate void TouchReleaseEventHandler(object sender, TouchReleaseEventArgs e);

    public class SelectableObject
    {
        protected bool selected;
        protected uint objectId;
        protected Color selectableColor;
        protected BasicEffect effect;
        protected int touchId;

        protected static GraphicsDevice graphicsDevice;

        public event TouchReleaseEventHandler TouchReleased;

        public bool Selected
        {
            get
            {
                return selected;
            }
            set
            {
                selected = value;
            }
        }
        public int TouchId
        {
            get
            {
                return touchId;
            }
            set
            {
                if (value == Touch.NO_ID && touchId != Touch.NO_ID)
                {
                    if (TouchReleased != null)
                    {
                        TouchReleased(this, new TouchReleaseEventArgs(touchId));
                    }
                }
                else if (value != Touch.NO_ID)
                {

                }
                touchId = value;
            }
        }

        public uint Id
        {
            get
            {
                return objectId;
            }
            set
            {
                objectId = value;
            }
        }

        public Color SelectableColor
        {
            get
            {
                return selectableColor;
            }
        }

        public static void SetGraphicsDevice(GraphicsDevice graphics)
        {
            graphicsDevice = graphics;
        }

        public SelectableObject()
        {
            selected = false;
            objectId = 0;
            selectableColor = new Color();
            touchId = -1;
        }

        public void SetObjectId(uint id)
        {
            objectId = id;
            selectableColor.R = (byte)(((objectId / 255) / 255) % 255);
            selectableColor.G = (byte)((objectId / 255) % 255);
            selectableColor.B = (byte)(objectId % 255);
            selectableColor.A = 255;
            SelectableColorChanged();
        }

        public virtual void SelectableColorChanged() { }
    }
}
