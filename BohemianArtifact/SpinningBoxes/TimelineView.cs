using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BohemianBookshelf
{
    public class TimelineView :  IViewBB
    {
        private Vector3 position;
        private Vector3 size;
        private SelectableLine[] lineList;
        private SelectableLine cross1;
        private SelectableLine cross2;
        private SelectableText text1;

        private BookLibrary library;

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

        public Vector3 Size
        {
            get
            {
                return size;
            }
            set
            {
                size = value;
            }
        }

        public TimelineView(BookLibrary lib, Vector3 p, Vector3 s)
        {
            library = lib;
            library.SelectedBookChanged += new BookLibrary.SelectedBookHandler(library_SelectedBookChanged);
            position = p;
            size = s;
            cross1 = new SelectableLine(new Vector3(0, 0, 0), new Vector3(1, 1, 0), Color.Purple, 10);
            cross2 = new SelectableLine(new Vector3(1, 0, 0), new Vector3(0, 1, 0), Color.Pink, 10);
            //text1 = new SelectableText(new Vector2(0.5f, 0.5f), "Timeline View", Color.Black);

            lineList = new SelectableLine[10];
            for (int i = 0; i < 10; i++)
            {
                lineList[i] = new SelectableLine(new Vector3(i * 0.1f, 0, 0), new Vector3(i * 0.1f, 1, 0), Color.Red, 10);
            }
        }

        void library_SelectedBookChanged(Book selectedBook)
        {
            // code to change the selected book
            throw new NotImplementedException();
        }

        public void Draw(GraphicsDevice graphicsDevice)
        {
            cross1.Draw(graphicsDevice);
            cross2.Draw(graphicsDevice);

            foreach (SelectableLine l in lineList)
            {
                l.Draw(graphicsDevice);
            }
            //quad.Draw(graphicsDevice);

            //text1.Draw(graphicsDevice);
        }

        public void DrawSelectable(GraphicsDevice graphicsDevice)
        {
        }
    }
}
