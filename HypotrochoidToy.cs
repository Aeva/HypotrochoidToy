using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;
using static System.Math;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;
using Vector2 = Microsoft.Xna.Framework.Vector2;

namespace HypotrochoidToy
{
    public class HypotrochoidToy : Microsoft.Xna.Framework.Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch? _spriteBatch;

        public Texture2D FillWhite;

        public Color[] RampPoints;

        public Random Randomizer;

        public float Speed1 = 1.0f;
        public float Speed2 = 1.0f;
        public float Ratio = 0.5f;

        public bool[] KeyLatch;

        public bool Fullscreen = false;
        Vector2 Span = new Vector2(900, 900);
        Vector2 Center = new Vector2(450, 450);
        Vector2 PointSizeRange = new Vector2(2.0f, 16.0f);

        public HypotrochoidToy()
        {
            _graphics = new GraphicsDeviceManager(this);
            _graphics.PreferMultiSampling = true;
            _graphics.HardwareModeSwitch = false;
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;
            ApplyWindowMode();

            Content.RootDirectory = "Content";

            Randomizer = new Random();

            KeyLatch = new bool[255];
            for (int KeyIndex = 0; KeyIndex < KeyLatch.Length;  ++KeyIndex)
            {
                KeyLatch[KeyIndex] = false;
            }

            SetPreset(Randomizer.Next(7));
        }

        public void ApplyWindowMode()
        {
            if (Fullscreen)
            {
                _graphics.PreferredBackBufferWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
                _graphics.PreferredBackBufferHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;
                _graphics.IsFullScreen = true;
                IsMouseVisible = false;
            }
            else
            {
                _graphics.PreferredBackBufferWidth = 900;
                _graphics.PreferredBackBufferHeight = 900;
                _graphics.IsFullScreen = false;
                IsMouseVisible = true;
            }
            Span.X = _graphics.PreferredBackBufferWidth;
            Span.Y = _graphics.PreferredBackBufferHeight;
            Center.X = Span.X / 2;
            Center.Y = Span.Y / 2;

            PointSizeRange.X = (2.0f / 900.0f) * Span.Y;
            PointSizeRange.Y = (16.0f / 900.0f) * Span.Y;

            _graphics.ApplyChanges();
        }

        public void SetPreset(int Preset)
        {
            switch (Preset)
            {
                case 0:
                    Speed1 = 0.8f;
                    Speed2 = 5.0f;
                    Ratio = 10.0f / 45.0f;
                    break;
                case 1:
                    Speed1 = 0.5f;
                    Speed2 = 3.0f;
                    Ratio = 10.0f / 45.0f;
                    break;
                case 2:
                    Speed1 = 4.424953f;
                    Speed2 = 10.6436644f;
                    Ratio = 0.3491416f;
                    break;
                case 3:
                    Speed1 = 1.091094f;
                    Speed2 = 9.587883f;
                    Ratio = 0.363624126f;
                    break;
                case 4:
                    Speed1 = 2.644092f;
                    Speed2 = 9.247984f;
                    Ratio = 0.222030759f;
                    break;
                case 5:
                    Speed1 = 7.92284632f;
                    Speed2 = 2.263693f;
                    Ratio = 0.3806913f;
                    break;
                case 6:
                    Speed1 = 2.77697587f;
                    Speed2 = 9.247984f;
                    Ratio = 0.402030617f;
                    break;
                default:
                    Randomize();
                    break;
            }
        }

        public void Randomize()
        {
            Speed1 = RandomRange(0.1f, 11.0f);
            Speed2 = RandomRange(0.1f, 11.0f);
            Ratio = RandomRange(0.1f, 0.4f);
        }

        public bool GetLatch(Keys Key)
        {
            return KeyLatch[(int)Key];
        }

        public void SetLatch(Keys Key, bool Value)
        {
            KeyLatch[(int)Key] = Value;
        }

        public bool OnKeyDown(Keys Key)
        {
            bool Latched = GetLatch(Key);
            if (!Latched && Keyboard.GetState().IsKeyDown(Key))
            {
                SetLatch(Key, true);
                return true;
            }
            else if (Latched && Keyboard.GetState().IsKeyUp(Key))
            {
                SetLatch(Key, false);
            }
            return false;
        }

        public float RandomRange(float Low, float High)
        {
            // According to https://learn.microsoft.com/en-us/dotnet/api/system.random.nextdouble?view=net-8.0#remarks
            // the upper bound of Random.NextDouble is 0.99999999999999978, which is equal to this:
            double RandomUpperBound = BitDecrement(BitDecrement(1.0));
            float Alpha = (float)(Randomizer.NextDouble() / RandomUpperBound);
            float InvAlpha = 1.0f - Alpha;
            return InvAlpha * Low + Alpha * High;
        }

        protected override void Initialize()
        {
            Window.Title = "Hypotrochoid Toy";

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            Debug.Assert(FillWhite == null);
            FillWhite = new Texture2D(_graphics.GraphicsDevice, 1, 1);
            FillWhite.SetData<Color>(new Color[] { Color.White });

            Debug.Assert(RampPoints == null);
            RampPoints = new Color[6];
            RampPoints[0] = new Color(255,   0,   0);
            RampPoints[1] = new Color(255, 255,   0);
            RampPoints[2] = new Color(  0, 255,   0);
            RampPoints[3] = new Color(  0, 255, 255);
            RampPoints[4] = new Color(  0,   0, 255);
            RampPoints[5] = new Color(255,   0, 255);
        }

        public Color Ramp(float Phase)
        {
            Phase = Clamp(Phase, 0.0f, 1.0f);
            int First = (int)(Phase * 6) % 6;
            int Second = (First + 1) % 6;
            Color Color1 = RampPoints[First];
            Color Color2 = RampPoints[Second];
            float Wedge = 1.0f / 6.0f;
            float Alpha = (Phase - (First * Wedge)) / Wedge;
            return Color.Lerp(Color1, Color2, Alpha);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
            {
                Exit();
            }

            if (OnKeyDown(Keys.Enter))
            {
                Randomize();
            }

            if (OnKeyDown(Keys.F11))
            {
                Fullscreen = !Fullscreen;
                ApplyWindowMode();
            }

            if (Keyboard.GetState().IsKeyDown(Keys.PageDown))
            {
                Ratio = Math.Clamp(Ratio - 0.01f, 0.1f, 0.9f);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.PageUp))
            {
                Ratio = Math.Clamp(Ratio + 0.01f, 0.01f, 0.99f);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                Speed1 = Math.Max(1.0f / ((1.0f / Speed1) - 0.0001f), 0.1f);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                Speed1 = Math.Max(1.0f / ((1.0f / Speed1) + 0.0001f), 0.1f);
            }

            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                Speed2 = Math.Max(1.0f / ((1.0f / Speed2) - 0.0002f), 0.1f);
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                Speed2 = Math.Max(1.0f / ((1.0f / Speed2) + 0.0002f), 0.1f);
            }

            for (int Preset = 0; Preset < 7; ++Preset)
            {
                Keys Number = (Keys)((int)Keys.D1 + Preset);
                if (OnKeyDown(Number))
                {
                    SetPreset(Preset);
                }
            }

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);

            Debug.Assert(_spriteBatch != null);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp);

            var Spin = (float Now, float Period) =>
            {
                return (Now % Period) / Period;
            };

            var Orbit = (Vector2 Center, float Radius, float Turn) =>
            {
                float Angle = Turn * 360.0f * (float)(PI / 180.0);
                return new Vector2(
                    (float)Sin(Angle) * Radius + Center.X,
                    (float)Cos(Angle) * Radius + Center.Y);
            };

            var Rotate = (Vector2 Point, Vector2 Center, float Turn) =>
            {
                float Angle = Turn * 360.0f * (float)(PI / 180.0);
                float S = (float)Math.Sin(Angle);
                float C = (float)Math.Cos(Angle);

                Point -= Center;
                Point = new Vector2(
                    Point.X * C - Point.Y * S,
                    Point.Y * C + Point.X * S);
                Point += Center;
                return Point;
            };

            float Interval = 1.0f / (60.0f * 6.0f);
            float Speed = 2.0f;
            float Now = (float)gameTime.TotalGameTime.TotalSeconds * Speed;

            Color Invisible = new Color(0, 0, 0, 0);

            int History = 7000;
            for (int Past = 0; Past > -History; --Past)
            {
                float Then = Now - (float)Past * Interval;
                float Alpha = (float)(Past) / (float)(-History);
                //Alpha = Alpha * Alpha;
                float InvAlpha = 1.0f - Alpha;
                int PointSize = (int)(PointSizeRange.X * InvAlpha + PointSizeRange.Y * Alpha);
                //float CounterSpin = (1.0f / 300.0f) * InvAlpha;

                var Rec = new Rectangle(0, 0, PointSize, PointSize);

                float Offset1 = Math.Min(Center.X, Center.Y) * Ratio;
                float Offset2 = Math.Min(Center.X, Center.Y) * (1.0f - Ratio);

                Vector2 Pos = Orbit(Center, Offset1, Spin(Then, Speed1));
                Pos = Orbit(Pos, Offset2, Spin(Then, Speed2));

                //Pos = Rotate(Pos, Center, Now * CounterSpin);
                //Pos -= Rec.Center.ToVector2();

                Color PointColor = Color.Lerp(Invisible, Ramp(Spin(Then, 2)), Alpha);

                for (int i = 0; i < 90; i += 5)
                {
                    _spriteBatch.Draw(FillWhite, Pos, Rec, PointColor, i, Rec.Center.ToVector2(), 1.0f, SpriteEffects.None, 0.00001f);
                }
            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
