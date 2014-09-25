using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace FlowFields
{
    class FluidSim
    {
        Field Terrain;

        SpriteFont Font;
        Texture2D Tile;

        Field TotalAltitude;
        Field CurrentWater;
        Field NextWater;
        double[,] DeltaWater;

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

        int Width;
        int Height;

        MouseState MState;

        public FluidSim(Field terrain, int width, int height)
        {
            Terrain = terrain;
            Width = width;
            Height = height;

            TotalAltitude = new Field(Width, Height);
            CurrentWater = new Field(Width, Height);
            CurrentWater.Add(StartingWater);
            NextWater = new Field(Width, Height);
            DeltaWater = new double[Width, Height];

            TerrainRenderer = new FieldRenderer(terrain);

            Gradient = new Vector2[Width, Height];
            Tile = Game1.CManager.Load<Texture2D>("tile");
            Font = Game1.CManager.Load<SpriteFont>("Font");
            Lines = new Line[Width, Height];

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    Lines[x, y] = new Line(x, y, 0, 0, 0);

            CurrentWater.Add(StartingWater);

            ResetStates();
            ComputeGradient();
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

        public void Update()
        {
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
        }

        public void Draw()
        {
            TerrainRenderer.Draw(Game1.Batch, Scale);

            if (!Keyboard.GetState().IsKeyDown(Keys.Space))
                for (int y = 0; y < Height; y++)
                    for (int x = 0; x < Width; x++)
                        if (CurrentWater.Data[x, y] > 0.00001)
                        {
                            float c = (float)CurrentWater.Data[x, y] + 0.2f;
                            Game1.Batch.Draw(Tile, new Vector2(x, y) * Scale, null, new Color(0, 0, c, c), 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
                        }

            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    Lines[x, y].Draw(Tile);

            Vector2 mv = new Vector2(MState.X, MState.Y) / Scale;

            if (mv.X > 0 && mv.X < Width && mv.Y > 0 && mv.Y < Height)
            {
                Game1.Batch.DrawString(Font, "Total Altitude " + TotalAltitude.Data[MState.X / (int)Scale, MState.Y / (int)Scale], new Vector2(0, 0), Color.Red);
                Game1.Batch.DrawString(Font, "Water " + CurrentWater.Data[MState.X / (int)Scale, MState.Y / (int)Scale], new Vector2(0, 20), Color.Red);
                Game1.Batch.DrawString(Font, "Gradient " + Gradient[MState.X / (int)Scale, MState.Y / (int)Scale], new Vector2(0, 40), Color.Red);
            }

            Game1.Batch.DrawString(Font, "Total Water " + TotalWater, new Vector2(0, 70), Color.Red);
        }
    }
}
