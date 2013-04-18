using System;
using System.Collections.Generic;
//using CoreInteractionFramework;
using Microsoft.Surface;
using Microsoft.Surface.Core;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Collections;

namespace BohemianBookshelf
{
    /// <summary>
    /// This is the main type for your application.
    /// </summary>
    public class BohemianBookshelf: Microsoft.Xna.Framework.Game
    {
        //private UIController controller;

        private Random r = new Random();
        // the graphics object
        private readonly GraphicsDeviceManager graphics;
        // whether or not we are rotated by 180 degrees or not
        private Matrix screenTransform = Matrix.Identity;
        private UserOrientation currentOrientation;

        // touch input target and the table with the current touch points
        private TouchTarget touchTarget;
        private Hashtable touchPoints;
        // vertices for a crosshair showing each of the finger touches
        private VertexPositionColorTexture[] touchCrosshair;
        private int inputFrameDelay = 1;
        private int inputFrames = 0;

        // array of colors for each pixel in the colorPickerTarget texture
        private Color[] colorPickerData;
        private RenderTarget2D colorPickerTarget;

        // rendering effect, work, view, and projection matrices
        private BasicEffect basicEffect;
        private Matrix world;
        private Matrix view;
        private Matrix projection;

        // the library of books
        private BookLibrary library;

        // the object manager for selections
        private SelectableObjectManager selectableObjects;
        // the timeline view
        private TimelineView timelineView;

        // so that we can render fonts
        private SpriteBatch spriteBatch;
        private SpriteFont kootenayFont;

        protected GraphicsDeviceManager Graphics
        {
            get { return graphics; }
        }
        protected new GraphicsDevice GraphicsDevice
        {
            get
            {
                return graphics.GraphicsDevice;
            }
        }
        protected TouchTarget TouchTarget
        {
            get { return touchTarget; }
        }

        public BohemianBookshelf()
        {
            graphics = new GraphicsDeviceManager(this);
            // enables textures larger than 2048 so we can do color based object picking
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
        }

        #region Initialization
        private void SetWindowOnSurface()
        {
            System.Diagnostics.Debug.Assert(Window != null && Window.Handle != IntPtr.Zero, "Window initialization must be complete before SetWindowOnSurface is called");
            if (Window == null || Window.Handle == IntPtr.Zero)
            {
                return;
            }

            // Get the window sized right.
            Program.InitializeWindow(Window);
            // Set the graphics device buffers.
            graphics.PreferredBackBufferWidth = Program.WindowSize.Width;
            graphics.PreferredBackBufferHeight = Program.WindowSize.Height;
            graphics.ApplyChanges();
        }

        private void InitializeSurfaceInput()
        {
            System.Diagnostics.Debug.Assert(Window != null && Window.Handle != IntPtr.Zero, "Window initialization must be complete before InitializeSurfaceInput is called");
            if (Window == null || Window.Handle == IntPtr.Zero)
            {
                return;
            }
            System.Diagnostics.Debug.Assert(touchTarget == null, "Surface input already initialized");
            if (touchTarget != null)
            {
                return;
            }

            // Create a target for surface input.
            touchTarget = new TouchTarget(Window.Handle, EventThreadChoice.OnBackgroundThread);
            touchTarget.EnableInput();

            touchTarget.TouchDown += new EventHandler<TouchEventArgs>(touchTarget_TouchDown);
            touchTarget.TouchMove += new EventHandler<TouchEventArgs>(touchTarget_TouchMove);
            touchTarget.TouchUp += new EventHandler<TouchEventArgs>(touchTarget_TouchUp);
            touchPoints = new Hashtable();
        }

        protected override void Initialize()
        {
            IsMouseVisible = true; // easier for debugging not to "lose" mouse
            SetWindowOnSurface();
            InitializeSurfaceInput();

            // Set the application's orientation based on the orientation at launch
            currentOrientation = ApplicationServices.InitialOrientation;

            // Subscribe to surface window availability events
            ApplicationServices.WindowInteractive += OnWindowInteractive;
            ApplicationServices.WindowNoninteractive += OnWindowNoninteractive;
            ApplicationServices.WindowUnavailable += OnWindowUnavailable;

            // Create a rotation matrix to orient the screen so it is viewed correctly,
            // when the user orientation is 180 degress different.
            Matrix rotation = Matrix.CreateRotationZ(MathHelper.ToRadians(180));
            Matrix translation = Matrix.CreateTranslation(graphics.GraphicsDevice.Viewport.Width,
                                                          graphics.GraphicsDevice.Viewport.Height, 0);
            Matrix inverted = rotation * translation;

            if (currentOrientation == UserOrientation.Top)
            {
                screenTransform = inverted;
            }

            base.Initialize();
        }
        #endregion

        #region TouchTarget TouchUp, TouchMove, TouchDown events
        void touchTarget_TouchMove(object sender, TouchEventArgs e)
        {
            lock (touchPoints)
            {
                touchPoints[e.TouchPoint.Id] = e.TouchPoint;
                /* extra safety checks
                if (touchPoints.ContainsKey(e.TouchPoint.Id) == true)
                {
                    touchPoints[e.TouchPoint.Id] = t;
                }
                else
                {
                    touchPoints.Add(e.TouchPoint.Id, t);
                }
                //*/
            }
        }

        void touchTarget_TouchUp(object sender, TouchEventArgs e)
        {
            lock (touchPoints)
            {
                touchPoints.Remove(e.TouchPoint.Id);
                /* extra safety checks
                if (touchPoints.ContainsKey(e.TouchPoint.Id) == true)
                {
                    touchPoints.Remove(e.TouchPointt.Id);
                }
                //*/
            }
        }

        void touchTarget_TouchDown(object sender, TouchEventArgs e)
        {
            lock (touchPoints)
            {
                touchPoints.Add(e.TouchPoint.Id, e.TouchPoint);
                /* extra safety checks
                if (touchPoints.ContainsKey(t.Id) == false)
                {
                    touchPoints.Add(e.TouchPoint.Id, e.TouchPoint);
                }
                //*/
            }
        }
        #endregion

        SelectableLine lineTest;
        SelectableQuad rectTest1;
        SelectableQuad rectTest2;
        SelectableText textTest;
        SelectableQuad[] quads;

        Texture2D whiteTexture;
        VertexPositionColor[] verts;
        protected override void LoadContent()
        {
            //whiteTexture = Content.Load<Texture2D>("white");
            
            string filename = System.Windows.Forms.Application.ExecutablePath;
            string path = System.IO.Path.GetDirectoryName(filename) + "\\Resources\\";

            // setup the render target for picking objects
            PresentationParameters pickerPP = GraphicsDevice.PresentationParameters;
            colorPickerTarget = new RenderTarget2D(GraphicsDevice, pickerPP.BackBufferWidth, pickerPP.BackBufferHeight, false, pickerPP.BackBufferFormat, pickerPP.DepthStencilFormat);
            colorPickerData = new Color[pickerPP.BackBufferWidth * pickerPP.BackBufferHeight];

            // this creates an origin at screen coordinates 0, 0 and makes the Y axis increase as you move down the screen
            world = Matrix.CreateTranslation(0, 0, 0) * Matrix.CreateScale(1, 1, 1);
            //world = Matrix.CreateTranslation(0, 0, 0);
            view = Matrix.CreateLookAt(new Vector3(0, 0, 30), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
            //projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800f / 480f, 0.01f, 100f);//graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.LineList, 0, 1);
            projection = Matrix.CreateOrthographic(Program.WindowSize.Width, Program.WindowSize.Height, 0.01f, 100.0f);

            // setup the effect we will use to render with. the important part is the view and projection matricies, and vertex coloring.
            basicEffect = new BasicEffect(GraphicsDevice);
            basicEffect.World = world;
            basicEffect.View = view;
            basicEffect.Projection = projection;
            basicEffect.VertexColorEnabled = true;

            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            rasterizerState.MultiSampleAntiAlias = true;
            GraphicsDevice.RasterizerState = rasterizerState;

            // this creates two lines that form a crosshair to show where the user is touching. this crosshair should be scaled to the size of the touchpoint
            touchCrosshair = new VertexPositionColorTexture[4];
            touchCrosshair[0] = new VertexPositionColorTexture(new Vector3(-2, 0, 0), Color.DarkGray, Vector2.Zero);
            touchCrosshair[1] = new VertexPositionColorTexture(new Vector3(2, 0, 0), Color.DarkGray, Vector2.Zero);
            touchCrosshair[2] = new VertexPositionColorTexture(new Vector3(0, -2, 0), Color.DarkGray, Vector2.Zero);
            touchCrosshair[3] = new VertexPositionColorTexture(new Vector3(0, 2, 0), Color.DarkGray, Vector2.Zero);

            // load the book library
            library = new BookLibrary("library.txt");

            // initialize the graphics device for SelectableText elements
            SelectableText.Initialize(GraphicsDevice);
            // and load our fonts
            //kootenayFont = Content.Load<SpriteFont>("Kootenay");

            // create the selectable object manager which will issue colors to all objects that need hit testing
            selectableObjects = new SelectableObjectManager();
            // create the timeline object
            timelineView = new TimelineView(library, new Vector3(100, 100, 0), new Vector3(1200, 800, 1));

            //* load testing quads and quads with emissive color. regular quads win in performance
            int numBoxes = 2000;
            int boxSize = 15;
            int radius = 700;
            quads = new SelectableQuad[numBoxes];
            Vector3[] points = new Vector3[4];
            for (int i = 0; i < numBoxes; i++)
            {
                points[0] = new Vector3(-boxSize, -boxSize, 0);
                points[1] = new Vector3(boxSize, -boxSize, 0);
                points[2] = new Vector3(boxSize, boxSize, 0);
                points[3] = new Vector3(-boxSize, boxSize, 0);
                Color newColor = new Color((float)r.NextDouble(), (float)r.NextDouble(), (float)r.NextDouble());
                SelectableQuad newQuad = new SelectableQuad(points[0], points[1], points[2], points[3], newColor);
                // give this box a random position
                newQuad.Position = Vector3.Transform(new Vector3((float)Math.Sqrt(r.NextDouble()) * radius, 0, 0), Matrix.CreateRotationZ((float)(r.NextDouble() * 2 * Math.PI)));
                newQuad.RotationSpeed = 5 * (float)r.NextDouble() / (float)Math.Sqrt(Math.Abs(newQuad.Position.X));
                quads[i] = newQuad;
                selectableObjects.AddObject(newQuad);
            }
            //*/

            /* test the Selectable objects
            lineTest = new SelectableLine(new Vector3(50, 50, 0), new Vector3(2000, 1000, 0), Color.Red, 10);
            rectTest1 = new SelectableQuad(new Vector3(100, 100, 0), new Vector3(400, 100, 0), new Vector3(400, 800, 0), new Vector3(100, 800, 0), Color.Navy);
            rectTest2 = new SelectableQuad(new Vector3(700, 100, 0), new Vector3(1000, 100, 0), new Vector3(1000, 800, 0), new Vector3(700, 800, 0), Color.Green);
            textTest = new SelectableText(kootenayFont, "T", new Vector2(980, 780), Color.Red, Color.Orange);
            //textTest.SetQuadColor(new Color(1, 0.7f, 0, 0.1f));
            //textTest.Rotation = 1;
            //textTest.Recompute();

            selectableObjects.AddObject(lineTest);
            selectableObjects.AddObject(rectTest1);
            selectableObjects.AddObject(rectTest2);
            selectableObjects.AddObject(textTest);
            //*/

            // this is to test rendering triangles and lines with user defined primary objects
            //verts = new VertexPositionColor[5];
            //verts[0] = new VertexPositionColor(new Vector3(0, 100, 0), Color.Red);
            //verts[1] = new VertexPositionColor(new Vector3(-100, -50, 0), Color.Green);
            //verts[2] = new VertexPositionColor(new Vector3(100, -50, 0), Color.Blue);

            //verts[3] = new VertexPositionColor(new Vector3(0, 0, 0), Color.Yellow);
            //verts[4] = new VertexPositionColor(new Vector3(500, 100, 0), Color.Yellow);

            // this is to test loading of the screen with a grid of many many vertical and horizontal lines
            //int pixelsPerLine = 2;
            //hLines = new VertexPositionColor[2 * GraphicsDevice.DisplayMode.Width / pixelsPerLine];
            //vLines = new VertexPositionColor[2 * GraphicsDevice.DisplayMode.Height / pixelsPerLine];
            //for (int i = 0; i < 2 * GraphicsDevice.DisplayMode.Width / pixelsPerLine; i += 2)
            //{
            //    hLines[i] = new VertexPositionColor(new Vector3(i * pixelsPerLine / 2, 0, 0), Color.White);
            //    hLines[i + 1] = new VertexPositionColor(new Vector3(i * pixelsPerLine / 2, GraphicsDevice.DisplayMode.Height, 0), Color.Gray);
            //}
            //for (int i = 0; i < 2 * GraphicsDevice.DisplayMode.Height / pixelsPerLine; i += 2)
            //{
            //    vLines[i] = new VertexPositionColor(new Vector3(0, i * pixelsPerLine / 2, 0), Color.White);
            //    vLines[i + 1] = new VertexPositionColor(new Vector3(GraphicsDevice.DisplayMode.Width, i * pixelsPerLine / 2, 0), Color.Gray);
            //}
            //*/

            // test drawing text to the screen, including a background color behind the text (eventually for hit testing)
            //spriteBatch = new SpriteBatch(graphics.GraphicsDevice);
            //Vector2 textSize = spriteFont.MeasureString("Testing");
            //spriteTexture = new Texture2D(GraphicsDevice, (int)textSize.X, (int)textSize.Y);
            //Color[] textureColor = new Color[(int)textSize.X * (int)textSize.Y];
            //for (int i = 0; i < textureColor.Length; i++)
            //{
            //    textureColor[i] = Color.Salmon;
            //    textureColor[i].A = 200;
            //}
            //spriteTexture.SetData<Color>(textureColor);

            // test the backbuffer renderer
            /*
            GraphicsDevice.SetRenderTarget(colorPickerTarget);
            Draw(new GameTime());
            GraphicsDevice.SetRenderTarget(null);
            FileStream pngFile = new FileStream("pickerTarget.png", FileMode.Create);
            colorPickerTarget.SaveAsPng(pngFile, graphics.PreferredBackBufferWidth, graphics.PreferredBackBufferHeight);
            //*/
        }

        protected override void UnloadContent()
        {
            Content.Unload();
        }

        protected override void Update(GameTime gameTime)
        {
            for (int i = 0; i < quads.Length; i++)
            {
                    quads[i].Position = Vector3.Transform(quads[i].Position, Matrix.CreateRotationZ(quads[i].RotationSpeed * (float)gameTime.ElapsedGameTime.TotalSeconds));
            }
            base.Update(gameTime);
        }

        float aveMilli = 0;
        int frameCount = 0;
        protected override void Draw(GameTime gameTime)
        {
            if (frameCount == 10)
            {
                frameCount = 0;
                aveMilli /= 10;
                //Console.WriteLine("average render time in ms: " + aveMilli);
                aveMilli = 0;
            }
            aveMilli += gameTime.ElapsedGameTime.Milliseconds;
            frameCount++;

            // if someone is touching the screen... then we have to render everything with selectable colors
            inputFrames++;
            if (inputFrameDelay < inputFrames)
            {
                inputFrames = 0;
            }
            if (0 < touchPoints.Count && inputFrames == 0)
            {
                // render to the color picker target
                GraphicsDevice.SetRenderTarget(colorPickerTarget);
                GraphicsDevice.Clear(Color.Black);

                basicEffect.World = world;
                basicEffect.LightingEnabled = false;
                basicEffect.VertexColorEnabled = true;
                ApplyAllEffects(basicEffect);
                //basicEffect.CurrentTechnique.Passes[0].Apply();

                for (int i = 0; i < quads.Length; i++)
                {
                    quads[i].DrawSelectable(basicEffect);
                    //quads[i].DrawSelectable(GraphicsDevice);
                }

                //rectTest1.DrawSelectable(GraphicsDevice);
                //rectTest2.DrawSelectable(GraphicsDevice);
                //lineTest.DrawSelectable(GraphicsDevice);
                //textTest.DrawSelectable(GraphicsDevice);

                //GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, 1);
                //GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, verts, 3, 1);
            }

            // then render to the screen
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.White);

            // setup the world projection matrix
            //basicEffect.World = Matrix.CreateScale(timelineView.Size) * Matrix.CreateTranslation(timelineView.Position) * world;
            basicEffect.World = world;
            basicEffect.VertexColorEnabled = true;
            basicEffect.TextureEnabled = false;
            ApplyAllEffects(basicEffect);

            //basicEffect.World = Matrix.CreateScale(timelineView.Size) * Matrix.CreateTranslation(timelineView.Position) * world;
            //timelineView.Draw(GraphicsDevice);

            //rectTest1.Draw(GraphicsDevice);
            //rectTest2.Draw(GraphicsDevice);
            //lineTest.Draw(GraphicsDevice);
            //textTest.DrawFill(basicEffect);

            for (int i = 0; i < quads.Length; i++)
            {
                //quads[i].Draw(GraphicsDevice);
                quads[i].Draw(basicEffect);
            }

            // render the grid of lines
            //GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, hLines, 0, hLines.Length / 2);
            //GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, vLines, 0, vLines.Length / 2);

            // render the test user primitives
            //GraphicsDevice.DrawUserPrimitives(PrimitiveType.TriangleList, verts, 0, 1);
            //GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, verts, 3, 1);
            //GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, lineTest.Points, 0, 1);

            /*
            // render the test text and background color
            spriteBatch.Begin();
            Vector2 textPosition = new Vector2(0, 0);
            // background color
            spriteBatch.Draw(spriteTexture, textPosition, Color.White);
            // text
            for (int i = 0; i < 1; i++)
            {
                textPosition.X = i * 4;
                textPosition.Y = i * 20;
                spriteBatch.DrawString(spriteFont, "Testing", textPosition, Color.Orange);
            }
            spriteBatch.End();
            //*/

            if (0 < touchPoints.Count && inputFrames == 0)
            {
                // copy the entire "backbuffer" to our own data structure
                colorPickerTarget.GetData<Color>(colorPickerData);

                // get access to the touch point data so that fired events will not change this data while you are doing hit testing
                lock (touchPoints)
                {
                    foreach (TouchPoint t in touchPoints.Values)
                    {
                        /*
                        for (int i = 0; i < 4; i++)
                        {
                            hit[i].Color = colorPickerData[(int)t.X + (int)t.Y * GraphicsDevice.DisplayMode.Width];
                        }
                        //*/
                        // this throws exception when finger goes onto screen border!!!
                        int tX = (int)t.X;
                        int tY = (int)t.Y;
                        if (tX < 0)
                        {
                            tX = 0;
                        }
                        else if (GraphicsDevice.DisplayMode.Width <= tX)
                        {
                            tX = GraphicsDevice.DisplayMode.Width - 1;
                        }
                        if (tY < 0)
                        {
                            tY = 0;
                        }
                        else if (GraphicsDevice.DisplayMode.Height <= tY)
                        {
                            tY = GraphicsDevice.DisplayMode.Height - 1;
                        }
                        Color touchPointColor = colorPickerData[tX + tY * GraphicsDevice.DisplayMode.Width];
                        uint touchColorId = (uint)(touchPointColor.R * 255 * 255 + touchPointColor.G * 255 + touchPointColor.B);
                        SelectableObject selectedObject;
                        if ((selectedObject = selectableObjects.FindObject(touchColorId)) != null)
                        {
                            selectedObject.Selected = true;
                            //for (int i = 0; i < 4; i++)
                            //{
                            //    if (selectedObject == rectTest1)
                            //    {
                            //        touchCrosshair[i].Color = Color.OrangeRed;
                            //    }
                            //    else if (selectedObject == rectTest2)
                            //    {
                            //        touchCrosshair[i].Color = Color.Yellow;
                            //    }
                            //    else if (selectedObject == lineTest)
                            //    {
                            //        touchCrosshair[i].Color = Color.SkyBlue;
                            //    }
                            //    else if (selectedObject == textTest)
                            //    {
                            //        touchCrosshair[i].Color = Color.Magenta;
                            //    }
                            //}
                            // draw the crosshair for each touchpoint, position it and scale it to the touchpoint size
                            //basicEffect.World = Matrix.CreateScale(t.MajorAxis, t.MajorAxis, 1) * Matrix.CreateTranslation(t.X, t.Y, 0) * world;
                            //basicEffect.LightingEnabled = false;
                            ////basicEffect.EmissiveColor = new Vector3(1, 1, 1);
                            //basicEffect.VertexColorEnabled = true;
                            //ApplyAllEffects(basicEffect);
                            ////basicEffect.CurrentTechnique.Passes[0].Apply();
                            //GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, touchCrosshair, 0, 2);
                        }
                        /*
                        if (colorPickerData[(int)t.X + (int)t.Y * GraphicsDevice.DisplayMode.Width] != Color.Black)
                        {
                            GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, hit, 0, 2);
                        }
                        //*/
                    }
                }
            }


            base.Draw(gameTime);
        }

        public void ApplyAllEffects(BasicEffect effect)
        {
            foreach (EffectPass ef in effect.CurrentTechnique.Passes)
            {
                ef.Apply();
            }
        }

        #region Application Event Handlers

        /// <summary>
        /// This is called when the user can interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowInteractive(object sender, EventArgs e)
        {
            //TODO: enable audio, animations here
        }

        /// <summary>
        /// This is called when the user can see but not interact with the application's window.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowNoninteractive(object sender, EventArgs e)
        {
            //TODO: Disable audio here if it is enabled

            //TODO: optionally enable animations here
        }

        /// <summary>
        /// This is called when the application's window is not visible or interactive.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnWindowUnavailable(object sender, EventArgs e)
        {
            //TODO: disable audio, animations here
        }

        #endregion

        #region IDisposable
        protected override void Dispose(bool disposing)
        {
            if (disposing) 
            {
                IDisposable graphicsDispose = graphics as IDisposable;
                if (graphicsDispose != null)
                {
                    graphicsDispose.Dispose();
                }
                if (spriteBatch != null)
                {
                    spriteBatch.Dispose();
                    spriteBatch = null;
                }
                if (touchTarget != null)
                {
                    touchTarget.Dispose();
                    touchTarget = null;
                }
                /*
                if (scatterView != null)
                {
                    scatterView.Dispose();
                    scatterView = null;
                }
                //*/
            }
            base.Dispose(disposing);
        }
        #endregion
    }
}

