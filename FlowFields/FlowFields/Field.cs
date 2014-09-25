using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlowFields
{
    class Field
    {
        public int Width;
        public int Height;

        public double[,] Data;

        public Field(int width, int height)
        {
            Width = width;
            Height = height;

            Data = new double[Width, Height];
        }

        public void Add(double quantity)
        {
            for (int y = 0; y < Height; y++)
                for (int x = 0; x < Width; x++)
                    Data[x, y] = quantity;
        }
    }
}
