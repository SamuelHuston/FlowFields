using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace FlowFields
{
    class Line
    {
        Rectangle rect = new Rectangle();
        float angle;
        Vector2 origin = new Vector2(0, 0.5f);

        public Line(int xPos, int yPos, float xDim, float yDim, int scale)
        {
            Initialize(xPos, yPos, xDim, yDim, scale);
        }

        public void Initialize(int xPos, int yPos, float xDim, float yDim, int scale)
        {
            if (xDim < 0)
                angle = (float)Math.Atan(yDim / xDim) + MathHelper.Pi;
            else
                angle = (float)Math.Atan(yDim / xDim);

            rect = new Rectangle((int)((xPos + 0.5) * scale), (int)((yPos + 0.5) * scale), (int)(new Vector2(xDim, yDim).Length() * scale), 1);
        }

        public void Draw(Texture2D texture)
        {
            Game1.Batch.Draw(texture, rect, null, Color.Red, angle, origin, SpriteEffects.None, 0);
        }
    }
}
