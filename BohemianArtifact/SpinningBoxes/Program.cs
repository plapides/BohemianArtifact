using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Surface;
using Microsoft.Surface.Core;
using Microsoft.Xna.Framework;

namespace BohemianBookshelf
{
    static class Program
    {
        // Hold on to the game window.
        static GameWindow Window;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            // Disable the WinForms unhandled exception dialog.
            // SurfaceShell will notify the user.
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);

            // Apply Surface globalization settings
            GlobalizationSettings.ApplyToCurrentThread();

            using (BohemianBookshelf app = new BohemianBookshelf())
            {
                app.Run();
            }
        }

        /// <summary>
        /// Gets the size of the main window.
        /// </summary>
        internal static Size WindowSize
        {
            get
            {
                return ((Form)Form.FromHandle(Window.Handle)).ClientSize;// DesktopBounds.Size;
            }
        }

        /// <summary>
        /// Position and adorn the Window appropriately.
        /// </summary>
        /// <param name="window"></param>
        internal static void InitializeWindow(GameWindow window)
        {
            if (window == null)
            {
                throw new ArgumentNullException("window");
            }

            Window = window;

            Form form = (Form)Form.FromHandle(Window.Handle);
            form.LocationChanged += OnFormLocationChanged;

            SetWindowSizeStylePosition();
        }

        /// <summary>
        /// Respond to changes in the form location, adjust if necessary.
        /// </summary>
        private static void OnFormLocationChanged(object sender, EventArgs e)
        {
            if (SurfaceEnvironment.IsSurfaceEnvironmentAvailable)
            {
                Form form = (Form)Form.FromHandle(Window.Handle);
                form.LocationChanged -= OnFormLocationChanged;
                //PositionWindow();
                form.LocationChanged += OnFormLocationChanged;
            }
        }

        /// <summary>
        /// Size the window to the primary device.
        /// </summary>
        private static void SetWindowSizeStylePosition()
        {
            int left;
            int top;
            int width;
            int height;
            if (InteractiveSurface.PrimarySurfaceDevice != null)
            {
                left = InteractiveSurface.PrimarySurfaceDevice.WorkingAreaLeft;
                top = InteractiveSurface.PrimarySurfaceDevice.WorkingAreaTop;
                width = InteractiveSurface.PrimarySurfaceDevice.WorkingAreaWidth;
                height = InteractiveSurface.PrimarySurfaceDevice.WorkingAreaHeight;
            }
            else
            {
                left = Screen.PrimaryScreen.WorkingArea.Left;
                top = Screen.PrimaryScreen.WorkingArea.Top;
                width = Screen.PrimaryScreen.WorkingArea.Width;
                height = Screen.PrimaryScreen.WorkingArea.Height;
            }

            Window.AllowUserResizing = true;
            Form form = (Form)Form.FromHandle(Window.Handle);
            form.UseWaitCursor = false;
            form.Cursor = null;
            form.ClientSize = new Size(width, height);
            form.WindowState = FormWindowState.Normal;
            form.FormBorderStyle = FormBorderStyle.None;
            form.Left = left;
            form.Top = top;
        }
    }
}

