using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace FlowFields
{
    public class Game1 : Game
    {
        GraphicsDeviceManager Graphics;
        public static SpriteBatch Batch;
        public static ContentManager CManager;
        public static GraphicsDevice GDevice;

        SpriteFont Font;
        Texture2D Tile;

        Field Terrain;
        Field TotalAltitude;
        Field CurrentWater;
        Field NextWater;
        double[,] DeltaWater;

        int Wavelength = 50;
        int Width = 100;
        int Height = 100;
        int Depth = 15;

        float Scale = 10;
        double FlowFraction = 1;

        double StartingWater = 0;

        FieldRenderer TerrainRenderer;

        Vector2[,] Gradient;
        Line[,] Lines;

        double TotalWater = 0;

        Vector2 LocalGradient;
        double LocalFreeWater;
        Vector2 LocalTotalDisplacement;
        Point LocalDisplacementPoint;
        Vector2 LocalOffset;
        double[,] LocalTemp;

        double LocalXGrad;
        double LocalYGrad;

        public Game1()
        {
            Graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            Graphics.PreferredBackBufferWidth = 1000;
            Graphics.PreferredBackBufferHeight = 1000;
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            base.Initialize();
        }

        protected override void LoadContent()
        {
            CManager = Content;
            GDevice = GraphicsDevice;

            Batch = new SpriteBatch(GDevice);

            Terrain = FieldTools.GenerateFractalNoise(Wavelength, Width, Height, Depth, 1.4);
            Field LowerSlice = FieldTools.Slice(Terrain, 0, 0.7);
            Field UpperSlice = FieldTools.Slice(Terrain, 0.7, 1);
            LowerSlice = FieldTools.ReshapeSquare(LowerSlice);
            LowerSlice = FieldTools.ReshapeSquare(LowerSlice);
            LowerSlice = FieldTools.ReshapeSquare(LowerSlice);
            Terrain = FieldTools.Sum(LowerSlice, UpperSlice);

            TotalAltitude = new Field(Width, Height);
            CurrentWater = new Field(Width, Height);
            CurrentWater.Add(StartingWater);
            NextWater = new Field(Width, Height);
            DeltaWater = new double[Width, Height];

            Terrain = FieldTools.CreateCentralDepression(Terrain);

            TerrainRenderer = new FieldRenderer(Terrain);

            Gradient = new Vector2[Width, Height];
            Tile = Content.Load<Texture2D>("tile");
            Font = Content.Load<SpriteFont>("Font");
            Lines = new Line[Width, Height];

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    Lines[x, y] = new Line(x, y, 0, 0, 0);

            CurrentWater.Add(StartingWater);

            ResetStates();
            ComputeGradient();
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            MouseState ms = Mouse.GetState();

            if (Mouse.GetState().LeftButton == ButtonState.Pressed)
            {
                Point scaledMousePos = new Point((int)(ms.X / Scale), (int)(ms.Y / Scale));

                if (scaledMousePos.X > 0 && scaledMousePos.X < Width && scaledMousePos.Y > 0 && scaledMousePos.Y < Height)
                    CurrentWater.Data[scaledMousePos.X, scaledMousePos.Y] += 0.1;
            }

            ResetStates();
            ComputeGradient();


            ResetWater();

            for (int y = 1; y < Height - 1; y++)
                for (int x = 1; x < Width - 1; x++)
                {
                    LocalGradient = Gradient[x, y];

                    LocalFreeWater = ComputeFreeWater(y, x);

                    if (CurrentWater.Data[x, y] > 0)
                    {
                        LocalTotalDisplacement = new Vector2(x + LocalGradient.X, y + LocalGradient.Y);
                        LocalDisplacementPoint.X = (int)Math.Floor(LocalTotalDisplacement.X);
                        LocalDisplacementPoint.Y = (int)Math.Floor(LocalTotalDisplacement.Y);
                        LocalOffset.X = LocalTotalDisplacement.X - LocalDisplacementPoint.X;
                        LocalOffset.Y = LocalTotalDisplacement.Y - LocalDisplacementPoint.Y;

                        NextWater.Data[x, y] += CurrentWater.Data[x, y] - LocalFreeWater / FlowFraction;

                        if (LocalDisplacementPoint.X > 1 && LocalDisplacementPoint.X < Width - 1 && LocalDisplacementPoint.Y > 1 && LocalDisplacementPoint.Y < Height - 1)
                        {
                            NextWater.Data[LocalDisplacementPoint.X, LocalDisplacementPoint.Y] += (1.0 - LocalOffset.X) * (1.0 - LocalOffset.Y) * LocalFreeWater * FlowFraction;
                            NextWater.Data[LocalDisplacementPoint.X + 1, LocalDisplacementPoint.Y] += LocalOffset.X * (1.0 - LocalOffset.Y) * LocalFreeWater * FlowFraction;
                            NextWater.Data[LocalDisplacementPoint.X, LocalDisplacementPoint.Y + 1] += (1.0 - LocalOffset.X) * LocalOffset.Y * LocalFreeWater * FlowFraction;
                            NextWater.Data[LocalDisplacementPoint.X + 1, LocalDisplacementPoint.Y + 1] += LocalOffset.X * LocalOffset.Y * LocalFreeWater * FlowFraction;
                        }
                        else
                            NextWater.Data[x, y] = LocalFreeWater * (1.0 - FlowFraction);

                        DeltaWater[x, y] = NextWater.Data[x, y] - CurrentWater.Data[x, y];
                    }
                }

            LocalTemp = CurrentWater.Data;
            CurrentWater.Data = NextWater.Data;
            NextWater.Data = LocalTemp;

            base.Update(gameTime);
        }

        private void ResetStates()
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    TotalAltitude.Data[x, y] = CurrentWater.Data[x, y] + Terrain.Data[x, y];
        }

        private void ResetWater()
        {
            TotalWater = 0;

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                {
                    NextWater.Data[x, y] = 0;
                    TotalWater += CurrentWater.Data[x, y];
                }
        }

        private void ComputeGradient()
        {
            for (int y = 1; y < Height - 1; y++)
                for (int x = 1; x < Width - 1; x++)
                {
                    LocalXGrad = 3 * TotalAltitude.Data[x - 1, y - 1] + 10 * TotalAltitude.Data[x - 1, y] + 3 * TotalAltitude.Data[x - 1, y + 1]
                        - 3 * TotalAltitude.Data[x + 1, y - 1] - 10 * TotalAltitude.Data[x + 1, y] - 3 * TotalAltitude.Data[x + 1, y + 1];
                    LocalYGrad = 3 * TotalAltitude.Data[x - 1, y - 1] + 10 * TotalAltitude.Data[x, y - 1] + 3 * TotalAltitude.Data[x + 1, y - 1]
                        - 3 * TotalAltitude.Data[x - 1, y + 1] - 10 * TotalAltitude.Data[x, y + 1] - 3 * TotalAltitude.Data[x + 1, y + 1];

                    Gradient[x, y].X = (float)LocalXGrad;
                    Gradient[x, y].Y = (float)LocalYGrad;

                    Lines[x, y].Initialize(x, y, (float)LocalXGrad, (float)LocalYGrad, (int)Scale);
                }
        }

        private double ComputeFreeWater(int y, int x)
        {
            double freeWater = 0;

            if (CurrentWater.Data[x, y] > 0)
            {
                freeWater = ComputeDiagonals(y, x, freeWater);
                freeWater = ComputeMains(y, x, freeWater);
                freeWater *= FlowFraction;
            }

            return freeWater;
        }

        private double ComputeMains(int y, int x, double freeWater)
        {
            if (TotalAltitude.Data[x, y] > TotalAltitude.Data[x, y - 1])
                if (Terrain.Data[x, y] > TotalAltitude.Data[x, y - 1])
                    freeWater = CurrentWater.Data[x, y];
                else
                    freeWater = Math.Max(freeWater, TotalAltitude.Data[x, y] - TotalAltitude.Data[x, y - 1]);

            //y = 0
            if (TotalAltitude.Data[x, y] > TotalAltitude.Data[x - 1, y])
                if (Terrain.Data[x, y] > TotalAltitude.Data[x - 1, y])
                    freeWater = CurrentWater.Data[x, y];
                else
                    freeWater = Math.Max(freeWater, TotalAltitude.Data[x, y] - TotalAltitude.Data[x - 1, y]);

            if (TotalAltitude.Data[x, y] > TotalAltitude.Data[x + 1, y])
                if (Terrain.Data[x, y] > TotalAltitude.Data[x + 1, y])
                    freeWater = CurrentWater.Data[x, y];
                else
                    freeWater = Math.Max(freeWater, TotalAltitude.Data[x, y] - TotalAltitude.Data[x + 1, y]);

            //y = +1
            if (TotalAltitude.Data[x, y] > TotalAltitude.Data[x, y + 1])
                if (Terrain.Data[x, y] > TotalAltitude.Data[x, y + 1])
                    freeWater = CurrentWater.Data[x, y];
                else
                    freeWater = Math.Max(freeWater, TotalAltitude.Data[x, y] - TotalAltitude.Data[x, y + 1]);
            return freeWater;
        }

        private double ComputeDiagonals(int y, int x, double freeWater)
        {
            if (TotalAltitude.Data[x, y] > TotalAltitude.Data[x - 1, y - 1])
                if (Terrain.Data[x, y] > TotalAltitude.Data[x - 1, y - 1])
                    freeWater = CurrentWater.Data[x, y];
                else
                    freeWater = Math.Max(freeWater, TotalAltitude.Data[x, y] - TotalAltitude.Data[x - 1, y - 1]);

            if (TotalAltitude.Data[x, y] > TotalAltitude.Data[x + 1, y - 1])
                if (Terrain.Data[x, y] > TotalAltitude.Data[x + 1, y - 1])
                    freeWater = CurrentWater.Data[x, y];
                else
                    freeWater = Math.Max(freeWater, TotalAltitude.Data[x, y] - TotalAltitude.Data[x + 1, y - 1]);

            if (TotalAltitude.Data[x, y] > TotalAltitude.Data[x - 1, y + 1])
                if (Terrain.Data[x, y] > TotalAltitude.Data[x - 1, y + 1])
                    freeWater = CurrentWater.Data[x, y];
                else
                    freeWater = Math.Max(freeWater, TotalAltitude.Data[x, y] - TotalAltitude.Data[x - 1, y + 1]);

            if (TotalAltitude.Data[x, y] > TotalAltitude.Data[x + 1, y + 1])
                if (Terrain.Data[x, y] > TotalAltitude.Data[x + 1, y + 1])
                    freeWater = CurrentWater.Data[x, y];
                else
                    freeWater = Math.Max(freeWater, TotalAltitude.Data[x, y] - TotalAltitude.Data[x + 1, y + 1]);
            return freeWater;
        }

        protected override void Draw(GameTime gameTime)
        {
            MouseState ms = Mouse.GetState();

            GDevice.Clear(Color.CornflowerBlue);

            Batch.Begin();

            TerrainRenderer.Draw(Batch, Scale);

            if (!Keyboard.GetState().IsKeyDown(Keys.Space))
                for (int y = 0; y < Height; y++)
                    for (int x = 0; x < Width; x++)
                        if (CurrentWater.Data[x, y] > 0.00001)
                        {
                            float c = (float)CurrentWater.Data[x, y] + 0.2f;
                            Batch.Draw(Tile, new Vector2(x, y) * Scale, null, new Color(0, 0, c, c), 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
                        }

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    Lines[x, y].Draw(Tile);

            Vector2 mv = new Vector2(ms.X, ms.Y) / Scale;

            if (mv.X > 0 && mv.X < Width && mv.Y > 0 && mv.Y < Height)
            {
                Batch.DrawString(Font, "Total Altitude " + TotalAltitude.Data[ms.X / (int)Scale, ms.Y / (int)Scale], new Vector2(0, 0), Color.Red);
                Batch.DrawString(Font, "Water " + CurrentWater.Data[ms.X / (int)Scale, ms.Y / (int)Scale], new Vector2(0, 20), Color.Red);
                Batch.DrawString(Font, "Gradient " + Gradient[ms.X / (int)Scale, ms.Y / (int)Scale], new Vector2(0, 40), Color.Red);
            }

            Batch.DrawString(Font, "Total Water " + TotalWater, new Vector2(0, 70), Color.Red);

            Batch.End();

            base.Draw(gameTime);
        }
    }
}
