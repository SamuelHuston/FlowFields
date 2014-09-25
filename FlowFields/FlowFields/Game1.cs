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

        Field Terrain;

        int Wavelength = 50;
        int Width = 100;
        int Height = 100;
        int Depth = 15;

        FluidSim FSim;

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

            Terrain = TerrainGenerator.CreateCave(Wavelength, Width, Height, Depth, 1.4, 0.7);

            FSim = new FluidSim(Terrain, Width, Height);
        }

        protected override void UnloadContent()
        {
        }

        protected override void Update(GameTime gameTime)
        {
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                this.Exit();

            FSim.Update();

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GDevice.Clear(Color.Purple);

            Batch.Begin();

            FSim.Draw();

            Batch.End();

            base.Draw(gameTime);
        }
    }
}
