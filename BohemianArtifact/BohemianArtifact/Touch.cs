using System;
using System.Windows.Forms;
using Windows7.Multitouch;

namespace BohemianArtifact
{
    public class Touch
    {
        public const int NO_ID = -1;

        private int id;
        private bool isActive;
        private float x;
        private float y;
        private float originX;
        private float originY;
        private float width;
        private float height;
        private float radius;
        private SelectableObject originObject;
        private SelectableObject currentObject;

        public int Id
        {
            get
            {
                return id;
            }
            set
            {
                id = value;
            }
        }
        public bool IsActive
        {
            get
            {
                return isActive;
            }
            set
            {
                isActive = value;
                if (isActive == false)
                {
                    originObject = null;
                    currentObject = null;
                }
            }
        }
        public float X
        {
            get
            {
                return x;
            }
            set
            {
                x = value;
            }
        }
        public float Y
        {
            get
            {
                return y;
            }
            set
            {
                y = value;
            }
        }
        public float OriginX
        {
            get
            {
                return originX;
            }
        }
        public float OriginY
        {
            get
            {
                return originY;
            }
        }
        public float Width
        {
            get
            {
                return width;
            }
            set
            {
                width = value;
            }
        }
        public float Height
        {
            get
            {
                return height;
            }
            set
            {
                height = value;
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
                radius = value;
            }
        }
        public SelectableObject OriginObject
        {
            get
            {
                return originObject;
            }
            set
            {
                originObject = value;
            }
        }
        public SelectableObject CurrentObject
        {
            get
            {
                return currentObject;
            }
            set
            {
                currentObject = value;
            }
        }

        public Touch(TouchEventArgs tp)
        {
            id = tp.Id;
            x = tp.Location.X;
            y = tp.Location.Y;
            width = tp.ContactSize.Value.Width;
            height = tp.ContactSize.Value.Width;
            radius = tp.ContactSize.Value.Width;
            isActive = true;

            originX = x;
            originY = y;
        }

        public Touch(int mouseId, MouseEventArgs e)
        {
            id = mouseId;
            x = e.X;
            y = e.Y;
            width = 10;
            height = 10;
            radius = 10;
            isActive = true;

            originX = x;
            originY = y;
        }

        public void Update(TouchEventArgs tp)
        {
            x = tp.Location.X;
            y = tp.Location.Y;
            width = tp.ContactSize.Value.Width;
            height = tp.ContactSize.Value.Width;
            radius = tp.ContactSize.Value.Width;
        }

        public void Update(MouseEventArgs e)
        {
            x = e.X;
            y = e.Y;
        }

        public void SetObject(SelectableObject obj)
        {
            // check if this is the first time that the object reference is being set
            if (originObject == null)
            {
                // clearly this is a new touch, set both the origin and current object
                originObject = obj;
                currentObject = originObject;
                currentObject.TouchId = id;
            }

            // check if the touch is over top of a new object
            if (obj != currentObject && currentObject != null)
            {
                // if it's a new object, clear the touchId of the previous currentObject
                currentObject.TouchId = NO_ID;
                currentObject = obj;
                currentObject.TouchId = id;
            }
        }

        public void ClearObjects()
        {
            if (originObject != null)
            {
                originObject.TouchId = NO_ID;
            }
            if (currentObject != null)
            {
                currentObject.TouchId = NO_ID;
            }
        }
    }
}
