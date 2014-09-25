using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace FlowFields
{
    class FieldRenderer
    {
        Field Terrain;
        Texture2D Texture;

        public FieldRenderer(Field field)
        {
            Terrain = field;
            Texture = new Texture2D(Game1.GDevice, field.Width, field.Height);

            Color[] colors = new Color[field.Width * field.Height];

            for (int y = 0; y < Terrain.Height; y++)
                for (int x = 0; x < Terrain.Width; x++)
                    colors[x + field.Width * y] = new Color((float)field.Data[x, y], (float)field.Data[x, y], (float)field.Data[x, y], 1);

            Texture.SetData<Color>(colors);
        }

        public void Draw(SpriteBatch batch, float Scale)
        {
            batch.Draw(Texture, Vector2.Zero, null, Color.White, 0, Vector2.Zero, Scale, SpriteEffects.None, 0);
        }
    }
}
