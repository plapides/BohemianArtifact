using System;
using Microsoft.Xna.Framework;

namespace BohemianBookshelf
{
    public class SelectableObject
    {
        protected bool selected;
        protected uint objectId;
        protected Color selectableColor;

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

        public SelectableObject()
        {
            selected = false;
            objectId = 0;
            selectableColor = new Color();
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
