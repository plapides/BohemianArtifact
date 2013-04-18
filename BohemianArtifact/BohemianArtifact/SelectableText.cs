using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.IO;

namespace BohemianArtifact
{
    public class SelectableText : SelectableQuad
    {
        private static SpriteBatch spriteBatch;

        private SpriteFont spriteFont;
        public Texture2D fontTexture;

        private Vector3 textPosition;
        private Vector3 textSize;
        private Color textColor;
        private float fontSize;
        private string text;
        private float rotation;

        public Vector3 Position
        {
            get
            {
                return textPosition;
            }
            set
            {
                textPosition = value;
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
                CreateTexture();
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

        public Vector3 TextSize
        {
            get
            {
                return textSize;
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
        public float Alpha
        {
            get
            {
                return screenPoints[0].Color.A;
            }
            set
            {
                SetAlpha(value);
            }
        }

        public static void Initialize()
        {
            spriteBatch = new SpriteBatch(graphicsDevice);
        }

        public SelectableText(SpriteFont font, string t)
            : base(Color.White)
        {
            text = t;
            spriteFont = font;
            textPosition = new Vector3(0, 0, 0);
            textSize = new Vector3(spriteFont.MeasureString(text), 1);
            textColor = Color.Black;
            rotation = 0;

            CreateTexture();
            Recompute();
        }

        public SelectableText(SpriteFont font, string t, Vector3 position)
            : base(Color.White)
        {
            text = t;
            spriteFont = font;
            textPosition = position;
            textSize = new Vector3(spriteFont.MeasureString(text), 1);
            textColor = Color.Black;
            rotation = 0;

            CreateTexture();
            Recompute();
        }

        public SelectableText(SpriteFont font, string t, Vector3 position, Color color)
            : base(Color.White)
        {
            text = t;
            spriteFont = font;
            textPosition = position;
            textSize = new Vector3(spriteFont.MeasureString(text), 1);
            textColor = color;
            rotation = 0;

            CreateTexture();
            Recompute();
        }

        public SelectableText(SpriteFont font, string t, Vector3 position, Color color, Color fillColor)
            : base(fillColor)
        {
            text = t;
            spriteFont = font;
            textPosition = position;
            textSize = new Vector3(spriteFont.MeasureString(text), 1); ;
            textColor = color;
            rotation = 0;

            CreateTexture();
            Recompute();
        }

        public void InverseScale(float scaleOverall, float scaleX, float scaleY)
        {
            textSize.X *= scaleOverall / scaleX;
            textSize.Y *= scaleOverall / scaleY;
            Recompute();
        }

        private void CreateTexture()
        {
            // create texture dimensions that are the smallest powers of 2 but still big enough to hold the text
            int textureWidth = (int)Math.Pow(2, Math.Ceiling(Math.Log(textSize.X, 2)));
            int textureHeight = (int)Math.Pow(2, Math.Ceiling(Math.Log(textSize.Y, 2)));
            // create the font that will hold the texture
            fontTexture = new Texture2D(spriteBatch.GraphicsDevice, textureWidth, textureHeight);
            // and create the render target that we will render the sprite batch to
            RenderTarget2D target = new RenderTarget2D(spriteBatch.GraphicsDevice, textureWidth, textureHeight);

            // create a textured effect
            BasicEffect effect = new BasicEffect(spriteBatch.GraphicsDevice);
            effect.TextureEnabled = true;
            effect.VertexColorEnabled = true;
            effect.CurrentTechnique.Passes[0].Apply();

            // switch to our render target
            spriteBatch.GraphicsDevice.SetRenderTarget(target);
            spriteBatch.GraphicsDevice.Clear(Color.Transparent);
            
            // draw the string to the target
            spriteBatch.Begin();
            spriteBatch.DrawString(spriteFont, text, Vector2.Zero, textColor);
            spriteBatch.End();

            spriteBatch.GraphicsDevice.SetRenderTarget(null);

            // then copy the data from the target to the font texture
            Color[] textureData = new Color[textureWidth * textureHeight];
            target.GetData<Color>(textureData);
            fontTexture.SetData<Color>(textureData);

            // and finally set the texture coordinates for the underlying quad
            screenPoints[0].TextureCoordinate = new Vector2(0, 0);
            screenPoints[1].TextureCoordinate = new Vector2(textSize.X / textureWidth, 0);
            screenPoints[2].TextureCoordinate = new Vector2(textSize.X / textureWidth, textSize.Y / textureHeight);
            screenPoints[3].TextureCoordinate = new Vector2(0, textSize.Y / textureHeight);
        }

        public void Recompute()
        {
            // get the x and y coords after rotation
            Vector3 textX = Vector3.Transform(new Vector3(textSize.X, 0, 0), Matrix.CreateFromYawPitchRoll(0, 0, rotation));
            Vector3 textY = Vector3.Transform(new Vector3(0, textSize.Y, 0), Matrix.CreateFromYawPitchRoll(0, 0, rotation));

            screenPoints[0].Position = textPosition;
            screenPoints[1].Position = screenPoints[0].Position + textX;
            screenPoints[2].Position = screenPoints[1].Position + textY;
            screenPoints[3].Position = screenPoints[0].Position + textY;

            // this is to synchronize the selectPoints in the SelectableQuad base class
            Synchronize();
        }

        private void SetAlpha(float alpha)
        {
            screenPoints[0].Color.A = (byte)(255 * alpha);
            screenPoints[1].Color.A = (byte)(255 * alpha);
            screenPoints[2].Color.A = (byte)(255 * alpha);
            screenPoints[3].Color.A = (byte)(255 * alpha);
        }


        public new void DrawScale(float scale)
        {
            XNA.Texture = fontTexture;
            XNA.PushMatrix();
            XNA.Scale(scale, scale, scale);
            base.Draw(true);
            XNA.PopMatrix();
        }
        
        public new void Draw()
        {
            XNA.Texture = fontTexture;
            base.Draw(true);
        }

        public void DrawFill()
        {
            base.Draw(false);
            Draw();
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
