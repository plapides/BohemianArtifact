using System;
using System.Collections.Generic;
//using CoreInteractionFramework;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Collections;
using System.Windows.Forms;
using Windows7.Multitouch.WinForms;
using Windows7.Multitouch;

namespace BohemianArtifact
{
    /// <summary>
    /// This is the main type for your application.
    /// </summary>
    public class BohemianArtifact: Microsoft.Xna.Framework.Game
    {
        private Random random = new Random();
        // the graphics object
        private readonly GraphicsDeviceManager graphics;
        // whether or not we are rotated by 180 degrees or not
        private Matrix screenTransform = Matrix.Identity;
        private BlendState blendState;

        // the form that contains the application
        private Form form;
        private const int MOUSE_ID = 1;

        // touch input target
        private TouchHandler touchTarget;
        // and the table with the current touch points
        private Dictionary<int, Touch> touchPoints;

        // vertices for a crosshair showing each of the finger touches
        private VertexPositionColorTexture[] touchCrosshair;

        // number of frames to skip when doing hittesting
        private int inputFrameDelay = 0;
        private int inputFrames = 0;

        // array of colors for each pixel in the colorPickerTarget texture
        private Color[] colorPickerData;
        private RenderTarget2D colorPickerTarget;

        // the library of books
        private ArtifactLibrary library;

        // the object manager for selections
        private SelectableObjectManager selectableObjects;

        private MaterialsView materialView;
        private KeywordView keywordView;
        private TimelineView timelineView;
        
        // so that we can render fonts
        private SpriteBatch spriteBatch;
        private SpriteFont spriteFont;

        private Color globalTextColor;

        private static string texturePath = "texture\\";
        public static string TexturePath
        {
            get
            {
                return texturePath;
            }
        }

        public ArtifactLibrary Library
        {
            get
            {
                return library;
            }
        }
        public SelectableObjectManager SelectableObjects
        {
            get
            {
                return selectableObjects;
            }
        }
        public Dictionary<int, Touch> TouchPoints
        {
            get
            {
                return touchPoints;
            }
        }
        public Color GlobalTextColor
        {
            get
            {
                return globalTextColor;
            }
        }
        public SpriteFont SpriteFont
        {
            get
            {
                return spriteFont;
            }
        }

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
        protected TouchHandler TouchTarget
        {
            get { return touchTarget; }
        }

        public BohemianArtifact()
        {
            graphics = new GraphicsDeviceManager(this);
            // turn on anti-aliasing
            graphics.PreferMultiSampling = true;
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
            form = Program.InitializeWindow(Window);
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
            touchTarget = Windows7.Multitouch.WinForms.Factory.CreateHandler<Windows7.Multitouch.TouchHandler>(form);
            touchTarget.DisablePalmRejection = true;
            //touchTarget.EnableInput();

            touchTarget.TouchDown += new EventHandler<TouchEventArgs>(touchTarget_TouchDown);
            touchTarget.TouchMove += new EventHandler<TouchEventArgs>(touchTarget_TouchMove);
            touchTarget.TouchUp += new EventHandler<TouchEventArgs>(touchTarget_TouchUp);

            //form.MouseDown += new MouseEventHandler(form_MouseDown);
            //form.MouseMove += new MouseEventHandler(form_MouseMove);
            //form.MouseUp += new MouseEventHandler(form_MouseUp);
            
            touchPoints = new Dictionary<int, Touch>();
        }

        protected override void Initialize()
        {
            IsMouseVisible = true; // easier for debugging not to "lose" mouse
            SetWindowOnSurface();
            InitializeSurfaceInput();

            // Create a rotation matrix to orient the screen so it is viewed correctly,
            // when the user orientation is 180 degress different.
            Matrix rotation = Matrix.CreateRotationZ(MathHelper.ToRadians(180));
            Matrix translation = Matrix.CreateTranslation(graphics.GraphicsDevice.Viewport.Width,
                                                          graphics.GraphicsDevice.Viewport.Height, 0);
            Matrix inverted = rotation * translation;

            //screenTransform = inverted;

            XNA.Initialize(GraphicsDevice);

            base.Initialize();
        }
        #endregion

        #region TouchTarget & Mouse events
        void form_MouseUp(object sender, MouseEventArgs e)
        {
            lock (touchPoints)
            {
                Touch touch = (Touch)touchPoints[MOUSE_ID];
                touchPoints.Remove(MOUSE_ID);
                touch.ClearObjects();
            }
        }

        void form_MouseMove(object sender, MouseEventArgs e)
        {
            lock (touchPoints)
            {
                if (touchPoints.ContainsKey(MOUSE_ID) == true)
                {
                    Touch touch = (Touch)touchPoints[MOUSE_ID];
                    touch.Update(e);
                }
            }
        }

        void form_MouseDown(object sender, MouseEventArgs e)
        {
            lock (touchPoints)
            {
                if (touchPoints.Count == 0)
                {
                    touchPoints.Add(MOUSE_ID, new Touch(MOUSE_ID, e));
                }
            }
        }

        void touchTarget_TouchMove(object sender, TouchEventArgs e)
        {
            lock (touchPoints)
            {
                if (touchPoints.ContainsKey(e.Id) == true)
                {
                    Touch touch = (Touch)touchPoints[e.Id];
                    touch.Update(e);
                }
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
                if (touchPoints.ContainsKey(e.Id) == true)
                {
                    Touch touch = (Touch)touchPoints[e.Id];
                    touchPoints.Remove(e.Id);
                    touch.ClearObjects();
                }
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
                if (touchPoints.Count == 0)
                {
                    Touch touch = new Touch(e);
                    touchPoints.Add(e.Id, touch);
                }
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
        SelectableEllipse circleTest;
        SelectableBlob blobTest1;
        SelectableBlob blobTest2;
        Timer timer;

        SelectableQuad[] quads;

        protected override void LoadContent()
        {
            string filename = System.Windows.Forms.Application.ExecutablePath;
            string path = System.IO.Path.GetDirectoryName(filename) + "\\Resources\\";

            // setup the render target for picking objects
            PresentationParameters presentation = GraphicsDevice.PresentationParameters;
            colorPickerTarget = new RenderTarget2D(GraphicsDevice, presentation.BackBufferWidth, presentation.BackBufferHeight, false, presentation.BackBufferFormat, presentation.DepthStencilFormat);
            colorPickerData = new Color[presentation.BackBufferWidth * presentation.BackBufferHeight];

            RasterizerState rasterizerState = new RasterizerState();
            rasterizerState.CullMode = CullMode.None;
            rasterizerState.MultiSampleAntiAlias = true;
            XNA.GraphicsDevice.RasterizerState = rasterizerState;
            BlendState bs = BlendState.NonPremultiplied;
            //BlendState bs1 = BlendState.Additive;
            blendState = new BlendState();
            blendState.AlphaBlendFunction = BlendFunction.Add;
            blendState.ColorBlendFunction = BlendFunction.Add;
            blendState.AlphaDestinationBlend = Blend.InverseSourceAlpha;
            blendState.AlphaSourceBlend = Blend.SourceAlpha;
            blendState.ColorDestinationBlend = Blend.InverseSourceAlpha;
            blendState.ColorSourceBlend = Blend.SourceAlpha;
            blendState = BlendState.AlphaBlend;

            // this creates an origin at screen coordinates 0, 0 and makes the Y axis increase as you move down the screen
            Matrix world = Matrix.CreateTranslation(-presentation.BackBufferWidth / 2, -presentation.BackBufferHeight / 2, 0) * Matrix.CreateScale(1, -1, 1);
            Matrix view = Matrix.CreateLookAt(new Vector3(0, 0, 30), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
            //projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800f / 480f, 0.01f, 100f);//graphics.GraphicsDevice.DrawPrimitives(PrimitiveType.LineList, 0, 1);
            Matrix projection = Matrix.CreateOrthographic(Program.WindowSize.Width, Program.WindowSize.Height, 0.01f, 100.0f);

            // initialize the global effect with the world, view, and projection matricies and several drawing flags
            XNA.Effect.World = world;
            XNA.Effect.View = view;
            XNA.Effect.Projection = projection;
            XNA.VertexColor = true;
            XNA.Texturing = false;
            XNA.ApplyEffect();

            // this creates two lines that form a crosshair to show where the user is touching. this crosshair should be scaled to the size of the touchpoint
            touchCrosshair = new VertexPositionColorTexture[4];
            touchCrosshair[0] = new VertexPositionColorTexture(new Vector3(-2, 0, 0), Color.Red, Vector2.Zero);
            touchCrosshair[1] = new VertexPositionColorTexture(new Vector3(2, 0, 0), Color.Green, Vector2.Zero);
            touchCrosshair[2] = new VertexPositionColorTexture(new Vector3(0, -2, 0), Color.Blue, Vector2.Zero);
            touchCrosshair[3] = new VertexPositionColorTexture(new Vector3(0, 2, 0), Color.Yellow, Vector2.Zero);

            // initialize the graphics device for all selectable objects
            SelectableObject.SetGraphicsDevice(GraphicsDevice);
            // initialize the graphics device for SelectableText elements
            SelectableText.Initialize();
            // and load our fonts
            spriteFont = Content.Load<SpriteFont>("Arial");
            XNA.Font = spriteFont;

            globalTextColor = new Color(0.7f, 0.7f, 0.7f, 1);

            // create the selectable object manager which will issue colors to all objects that need hit testing
            selectableObjects = new SelectableObjectManager();

            // load the book library
            //library = new ArtifactLibrary("small_sample.xml");
            library = new ArtifactLibrary("large_sample_sorted_colors.xml");
            Console.WriteLine("loaded library");

            // create the visualization objects
            // we are using the screen width for the x and y calculation, so the max y value is 9/16 (0.5625f)
            float viewPadding = 0.025f;
            float materialSize = 0.3f;
            float materialLeft = viewPadding;
            float materialTop = viewPadding;
            float keywordSize = materialSize;
            float keywordLeft = materialLeft + materialSize + viewPadding;
            float keywordTop = viewPadding;
            float infoSize = materialSize;
            float infoLeft = viewPadding;
            float infoTop = materialTop + materialSize + viewPadding;
            float timelineSize = 0.6f; // timeline is only 0.3 * size in height (e.g. 0.15)
            float timelineLeft = infoLeft + infoSize + viewPadding;
            float timelineTop = keywordTop + keywordSize + viewPadding;
            //float timelineTop = 1 - timelineSize * 0.3f - viewPadding;
            materialView = new MaterialsView(this, new Vector3(materialLeft * presentation.BackBufferWidth, materialTop * presentation.BackBufferWidth, 0), new Vector3(materialSize * presentation.BackBufferWidth, materialSize * presentation.BackBufferWidth, 1));
            keywordView = new KeywordView(this, new Vector3(keywordLeft * presentation.BackBufferWidth, keywordTop * presentation.BackBufferWidth, 0), new Vector3(keywordSize * presentation.BackBufferWidth, keywordSize * presentation.BackBufferWidth, 1));
            timelineView = new TimelineView(this, new Vector3(timelineLeft * presentation.BackBufferWidth, timelineTop * presentation.BackBufferWidth, 0), new Vector3(timelineSize * presentation.BackBufferWidth, timelineSize * presentation.BackBufferWidth, 1));
            //timelineView = new TimelineView(this, new Vector3(400, 400, 0), new Vector3(1500, 1500, 1));

            // select a random artifact
            int numArtifacts = library.Artifacts.Count;
            int artifactIndex = random.Next(numArtifacts);
            artifactIndex = 0;
            library.SelectArtifact(library.Artifacts[artifactIndex]);

            FileStream file = new FileStream("texture\\material\\metal_2.jpg", FileMode.Open);
            Texture2D texture = Texture2D.FromStream(GraphicsDevice, file);
            file.Close();
            blobTest1 = new SelectableBlob(new Vector2(1000, 500), (float)Math.PI / 4, 25, 300, 0, Color.RosyBrown, Color.Black, Color.Black, null);
            //blobTest2 = new SelectableBlob(new Vector2(1000, 500), 80, Color.White, texture);
            blobTest2 = new SelectableBlob(new Vector2(1000, 500), 0*(float)Math.PI / 4, 0, 0, 10, Color.Red, Color.Blue, Color.Green, texture);
            blobTest2.CircleRadius = 40;
            blobTest2.MiddleRadius = 0;
            timer = new Timer(1);
            selectableObjects.AddObject(blobTest2);

            Color c1 = Color.Navy;
            //c1.A = 127;
            Color c2 = new Color(0.0f, 1.0f, 0.0f, 1.0f);
            //c2.A = 127;
            rectTest1 = new SelectableQuad(new Vector3(100, 100, -10), new Vector3(400, 100, -10), new Vector3(400, 800, -10), new Vector3(100, 800, -10), c1);
            rectTest2 = new SelectableQuad(new Vector3(200, 200, 0), new Vector3(600, 200, 0), new Vector3(600, 600, 0), new Vector3(200, 600, 0), c2);
            rectTest2.Points[0].Color = Color.Transparent;
            rectTest2.Points[3].Color.A = 0;
            /* test the Selectable objects
            lineTest = new SelectableLine(new Vector3(50, 50, 0), new Vector3(2000, 1000, 0), Color.Red, 20);
            rectTest1 = new SelectableQuad(new Vector3(100, 100, 0), new Vector3(400, 100, 0), new Vector3(400, 800, 0), new Vector3(100, 800, 0), Color.Navy);
            rectTest2 = new SelectableQuad(new Vector3(700, 100, 0), new Vector3(1000, 100, 0), new Vector3(1000, 800, 0), new Vector3(700, 800, 0), Color.Green);
            textTest = new SelectableText(kootenayFont, "Kootenay", new Vector3(980, 780, 0), Color.Red, Color.Orange);

            //textTest.SetQuadColor(new Color(1, 0.7f, 0, 0.1f));
            //textTest.Rotation = 1;
            //textTest.Recompute();

            selectableObjects.AddObject(lineTest);
            selectableObjects.AddObject(rectTest1);
            selectableObjects.AddObject(rectTest2);
            selectableObjects.AddObject(textTest);
            //*/

            /* this is to test rendering triangles and lines with user defined primary objects
            verts = new VertexPositionColor[5];
            verts[0] = new VertexPositionColor(new Vector3(0, 0, 0), Color.Red);
            verts[1] = new VertexPositionColor(new Vector3(2000, 0, 0), Color.Green);
            verts[2] = new VertexPositionColor(new Vector3(1000, 2000, 0), Color.Blue);

            verts[3] = new VertexPositionColor(new Vector3(0, 0, 0), Color.Yellow);
            verts[4] = new VertexPositionColor(new Vector3(500, 100, 0), Color.Yellow);

            // this is to test loading of the screen with a grid of many many vertical and horizontal lines
            int pixelsPerLine = 2;
            hLines = new VertexPositionColor[2 * GraphicsDevice.DisplayMode.Width / pixelsPerLine];
            vLines = new VertexPositionColor[2 * GraphicsDevice.DisplayMode.Height / pixelsPerLine];
            for (int i = 0; i < 2 * GraphicsDevice.DisplayMode.Width / pixelsPerLine; i += 2)
            {
                hLines[i] = new VertexPositionColor(new Vector3(i * pixelsPerLine / 2, 0, 0), Color.White);
                hLines[i + 1] = new VertexPositionColor(new Vector3(i * pixelsPerLine / 2, GraphicsDevice.DisplayMode.Height, 0), Color.Gray);
            }
            for (int i = 0; i < 2 * GraphicsDevice.DisplayMode.Height / pixelsPerLine; i += 2)
            {
                vLines[i] = new VertexPositionColor(new Vector3(0, i * pixelsPerLine / 2, 0), Color.White);
                vLines[i + 1] = new VertexPositionColor(new Vector3(GraphicsDevice.DisplayMode.Width, i * pixelsPerLine / 2, 0), Color.Gray);
            }
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
            materialView.Update(gameTime);
            keywordView.Update(gameTime);
            timelineView.Update(gameTime);

            // the follow code can be deleted, it was used to test timers and blobs
            timer.Update(gameTime.TotalGameTime.TotalSeconds);
            if (timer.Running)
            {
                if (timer.Elapsed < 0.5f)
                {
                    blobTest2.MiddleRadius = 275 * timer.Elapsed * 2;
                    blobTest2.CircleRadius = (25 - 80) * timer.Elapsed * 2 + 80;
                }
                else
                {
                    blobTest2.SpanAngle = (timer.Elapsed - 0.5f) * 2 * (float)Math.PI / 2;
                }
                blobTest2.Recompute();
            }

            base.Update(gameTime);
        }

        float aveMilli = 0;
        int frameCount = 0;
        protected override void Draw(GameTime gameTime)
        {
            if (frameCount == 20)
            {
                frameCount = 0;
                aveMilli /= 20;
                //Console.WriteLine("Average render time in ms: " + aveMilli);
                aveMilli = 0;
            }
            aveMilli += gameTime.ElapsedGameTime.Milliseconds;
            frameCount++;

            // we have to call this at the start of our draw loop
            XNA.ApplyEffect();
            //XNA.RestoreWorldMatrix();

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

                // turn texturing off, just in case
                XNA.Texturing = false;

                materialView.DrawSelectable();
                keywordView.DrawSelectable();
                timelineView.DrawSelectable();

                XNA.PushMatrix();
                XNA.Translate(blobTest2.CenterPosition);
                //blobTest1.DrawFill(false);
                //blobTest2.DrawSelectable();
                XNA.PopMatrix();

                //rectTest1.DrawSelectable();
                //rectTest2.DrawSelectable();
                //lineTest.DrawSelectable(basicEffect);
                //textTest.DrawSelectable(basicEffect);
            }

            // then render to the screen
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.White);

            XNA.GraphicsDevice.BlendState = blendState;
            //XNA.GraphicsDevice.BlendState = BlendState.NonPremultiplied;

            materialView.Draw();
            keywordView.Draw();
            timelineView.Draw();

            //rectTest1.Draw();
            //rectTest2.Draw();
            //lineTest.Draw(basicEffect);
            //textTest.Draw();

            XNA.PushMatrix();
            XNA.Translate(blobTest2.CenterPosition);
            //blobTest1.DrawFill(false);
            //blobTest2.DrawFill(false);
            //blobTest2.DrawBorder();
            XNA.PopMatrix();

            //blobTest.DrawEdge();
            //circleTest.DrawBorder();
            //circleTest.DrawOutsideEdge();
            //circleTest.DrawInsideEdge();

            // draw the hit tested touch point crosshairs
            if (0 < touchPoints.Count && inputFrames == 0)
            {
                // copy the entire "backbuffer" to our own data structure
                colorPickerTarget.GetData<Color>(colorPickerData);

                // get access to the touch point data so that fired events will not change this data while you are doing hit testing
                lock (touchPoints)
                {
                    foreach (Touch t in touchPoints.Values)
                    {
                        int tX = (int)t.X;
                        int tY = (int)t.Y;
                        // ensure that the touchpoint is on the screen
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
                        // otherwise reading this array may cause an out of bounds exception!
                        Color touchPointColor = colorPickerData[tX + tY * GraphicsDevice.DisplayMode.Width];
                        uint touchColorId = (uint)(touchPointColor.R * 255 * 255 + touchPointColor.G * 255 + touchPointColor.B);
                        SelectableObject selectedObject;
                        if ((selectedObject = selectableObjects.FindObject(touchColorId)) != null)
                        {
                            // tell the Touch what object it's on top of
                            t.SetObject(selectedObject);
                            //HandleTouch(selectedObject);

                            // draw the crosshair for each touchpoint, position it and scale it to the touchpoint size
                            //basicEffect.World = Matrix.CreateScale(t.Radius, t.Radius, 1) * Matrix.CreateTranslation(t.X, t.Y, 0) * world;
                            //basicEffect.VertexColorEnabled = true;
                            //basicEffect.TextureEnabled = false;
                            //basicEffect.CurrentTechnique.Passes[0].Apply();

                            XNA.PushMatrix();
                            XNA.Translate(t.X, t.Y, 0);
                            XNA.Scale(t.Radius, t.Radius, 1);
                            XNA.Texturing = false;
                            GraphicsDevice.DrawUserPrimitives(PrimitiveType.LineList, touchCrosshair, 0, 2);
                            XNA.PopMatrix();
                        }
                        else
                        {
                            t.ClearObjects();
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

        private void HandleTouch(SelectableObject obj)
        {
            if (obj == blobTest2)
            {
                if (blobTest2.CircleRadius == 80)
                {
                    timer.Start();
                }
                else
                {
                    blobTest2.MiddleRadius = 0;
                    blobTest2.CircleRadius = 80;
                    blobTest2.SpanAngle = 0;
                    blobTest2.Recompute();
                }
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
                    //touchTarget.Dispose();
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

