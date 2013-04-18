using System;
using System.IO;
using System.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BohemianArtifact
{
    public static class XNA
    {
        private static Stack matrixStack;
        private static BasicEffect effect;
        private static SpriteFont font;

        public static SpriteFont Font
        {
            get
            {
                return font;
            }
            set
            {
                font = value;
            }
        }
        public static Matrix CurrentMatrix
        {
            get
            {
                return (Matrix)matrixStack.Peek();
            }
        }

        public static BasicEffect Effect
        {
            get
            {
                return effect;
            }
        }
        public static bool VertexColor
        {
            get
            {
                return effect.VertexColorEnabled;
            }
            set
            {
                if (value != effect.VertexColorEnabled)
                {
                    effect.VertexColorEnabled = value;
                    ApplyEffect();
                }
            }
        }
        public static bool Texturing
        {
            get
            {
                return effect.TextureEnabled;
            }
            set
            {
                if (value != effect.TextureEnabled)
                {
                    effect.TextureEnabled = value;
                    ApplyEffect();
                }
            }
        }
        public static Texture2D Texture
        {
            get
            {
                return effect.Texture;
            }
            set
            {
                if (value != effect.Texture)
                {
                    effect.Texture = value;
                    ApplyEffect();
                }
            }
        }
        public static GraphicsDevice GraphicsDevice
        {
            get
            {
                return effect.GraphicsDevice;
            }
        }

        public static void Initialize(GraphicsDevice graphicsDevice)
        {
            matrixStack = new Stack();
            effect = new BasicEffect(graphicsDevice);
            effect.World = Matrix.Identity;
        }

        public static void ApplyEffect()
        {
            effect.CurrentTechnique.Passes[0].Apply();
        }

        public static void PushMatrix()
        {
            matrixStack.Push(effect.World);
        }

        public static void PopMatrix()
        {
            effect.World = (Matrix)(matrixStack.Pop());
            effect.CurrentTechnique.Passes[0].Apply();
        }

        public static void Transform(Matrix transform)
        {
//            effect.World = effect.World * transform;
            effect.World = transform * effect.World;
            effect.CurrentTechnique.Passes[0].Apply();
        }

        public static void Translate(float x, float y, float z)
        {
            Transform(Matrix.CreateTranslation(x, y, z));
        }

        public static void Translate(Vector3 vector)
        {
            Transform(Matrix.CreateTranslation(vector));
        }

        public static void Scale(float x, float y, float z)
        {
            Transform(Matrix.CreateScale(x, y, z));
        }

        public static void Scale(Vector3 vector)
        {
            Transform(Matrix.CreateScale(vector));
        }

        public static void RotateX(float angle)
        {
            Transform(Matrix.CreateRotationX(angle));
        }
        public static void RotateY(float angle)
        {
            Transform(Matrix.CreateRotationY(angle));
        }
        public static void RotateZ(float angle)
        {
            Transform(Matrix.CreateRotationZ(angle));
        }

        public static void DarkenColor(ref Color color, float amount)
        {
            color.R = (byte)((float)color.R * amount);
            color.G = (byte)((float)color.G * amount);
            color.B = (byte)((float)color.B * amount);
        }

        public static Color DarkenColor(Color color, float amount)
        {
            DarkenColor(ref color, amount);
            return color;
        }

        public static Texture2D LoadTexture(string path)
        {
            Texture2D texture = null;
            try
            {
                FileStream file = new FileStream(path, FileMode.Open);
                texture = Texture2D.FromStream(XNA.GraphicsDevice, file);
                file.Close();
            }
            catch (IOException e)
            {
                //Console.WriteLine("Load texture IO exception: " + e.Message);
            }
            return texture;
        }
    }
}
