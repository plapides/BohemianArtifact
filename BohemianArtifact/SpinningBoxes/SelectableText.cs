using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.IO;

namespace BohemianBookshelf
{
    public class SelectableText : SelectableQuad
    {
        private static SpriteBatch spriteBatch;

        private SpriteFont spriteFont;
        public Texture2D fontTexture;

        private Vector2 textPosition;
        private Color textColor;
        private string text;
        private float rotation;

        public Vector2 Position
        {
            get
            {
                return textPosition;
            }
        }

        public string Text
        {
            get
            {
                return text;
            }
            set
            {
                text = value;
                Recompute();
            }
        }

        public Color TextColor
        {
            get
            {
                return textColor;
            }
        }

        public float Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                rotation = value;
            }
        }

        public static void Initialize(GraphicsDevice graphicsDevice)
        {
            spriteBatch = new SpriteBatch(graphicsDevice);
        }

        public SelectableText(SpriteFont font, string t)
            : base(Color.White)
        {
            spriteFont = font;
            textPosition = new Vector2(0, 0);
            text = t;
            textColor = Color.Black;
            rotation = 0;

            CreateTexture();
            Recompute();
        }

        public SelectableText(SpriteFont font, string t, Color color)
            : base(Color.White)
        {
            spriteFont = font;
            textPosition = new Vector2(0, 0);
            text = t;
            textColor = color;
            rotation = 0;

            CreateTexture();
            Recompute();
        }

        public SelectableText(SpriteFont font, string t, Vector2 position, Color color)
            : base(Color.White)
        {
            spriteFont = font;
            textPosition = position;
            text = t;
            textColor = color;
            rotation = 0;

            CreateTexture();
            Recompute();
        }

        public SelectableText(SpriteFont font, string t, Vector2 position, Color color, Color fillColor)
            : base(fillColor)
        {
            spriteFont = font;
            textPosition = position;
            text = t;
            textColor = color;
            rotation = 0;

            CreateTexture();
            Recompute();
        }

        private void CreateTexture()
        {
            Vector2 textSize = spriteFont.MeasureString(text);
            int textureWidth = (int)Math.Pow(2, Math.Ceiling(Math.Log(textSize.X, 2)));
            int textureHeight = (int)Math.Pow(2, Math.Ceiling(Math.Log(textSize.Y, 2)));
            fontTexture = new Texture2D(spriteBatch.GraphicsDevice, textureWidth, textureHeight);
            RenderTarget2D target = new RenderTarget2D(spriteBatch.GraphicsDevice, textureWidth, textureHeight);

            BasicEffect effect = new BasicEffect(spriteBatch.GraphicsDevice);
            effect.TextureEnabled = true;
            effect.VertexColorEnabled = true;
            effect.CurrentTechnique.Passes[0].Apply();

            spriteBatch.GraphicsDevice.SetRenderTarget(target);
            spriteBatch.GraphicsDevice.Clear(Color.Transparent);
            
            spriteBatch.Begin();
            spriteBatch.DrawString(spriteFont, text, Vector2.Zero, Color.Black);
            spriteBatch.End();

            spriteBatch.GraphicsDevice.SetRenderTarget(null);
            target.SaveAsPng(new FileStream("testText.png", FileMode.Create), textureWidth, textureHeight);

            Color[] textureData = new Color[textureWidth * textureHeight];
            target.GetData<Color>(textureData);
            fontTexture.SetData<Color>(textureData);

            effect.TextureEnabled = true;
            effect.Texture = fontTexture;

            //screenPoints[0].TextureCoordinate = new Vector2(0, 0);
            //screenPoints[1].TextureCoordinate = new Vector2(1, 0);
            //screenPoints[2].TextureCoordinate = new Vector2(1, 1);
            //screenPoints[3].TextureCoordinate = new Vector2(0, 1);

            screenPoints[0].TextureCoordinate = new Vector2(0, 0);
            screenPoints[1].TextureCoordinate = new Vector2(textSize.X / textureWidth, 0);
            screenPoints[2].TextureCoordinate = new Vector2(textSize.X / textureWidth, textSize.Y / textureHeight);
            screenPoints[3].TextureCoordinate = new Vector2(0, textSize.Y / textureHeight);
        }

        public void Recompute()
        {
            Vector2 textSize = spriteFont.MeasureString(text);
            Vector3 textX = Vector3.Transform(new Vector3(textSize.X, 0, 0), Matrix.CreateFromYawPitchRoll(0, 0, rotation));
            Vector3 textY = Vector3.Transform(new Vector3(0, textSize.Y, 0), Matrix.CreateFromYawPitchRoll(0, 0, rotation));

            screenPoints[0].Position = new Vector3(textPosition, 0);
            screenPoints[1].Position = screenPoints[0].Position + textX;
            screenPoints[2].Position = screenPoints[1].Position + textY;
            screenPoints[3].Position = screenPoints[0].Position + textY;

            // this is to synchronize the selectPoints in the SelectableQuad base class
            Synchronize();
        }

        public new void Draw(GraphicsDevice graphicsDevice)
        {
            base.Draw(graphicsDevice);
        }

        public void Draw(BasicEffect effect)
        {
            effect.TextureEnabled = true;
            effect.Texture = fontTexture;
            effect.CurrentTechnique.Passes[0].Apply();
            base.Draw(effect.GraphicsDevice);
        }

        public void DrawFill(BasicEffect effect)
        {
            effect.TextureEnabled = false;
            effect.CurrentTechnique.Passes[0].Apply();
            base.Draw(effect.GraphicsDevice);
            Draw(effect);
        }

        /*
        static Vector2 origin = new Vector2();
        public new void Draw(GraphicsDevice graphicsDevice)
        {
            spriteBatch.Begin();
            spriteBatch.DrawString(spriteFont, text, textPosition, textColor, rotation, origin, 1, SpriteEffects.None, 0);
            spriteBatch.End();
        }

        public void DrawFill(GraphicsDevice graphicsDevice)
        {
            // draw the quad first
            base.Draw(graphicsDevice);
            // then the text
            Draw(graphicsDevice);
        }
        //*/
    }
}
