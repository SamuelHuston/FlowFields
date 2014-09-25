using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace FlowFields
{
    class FieldTools
    {
        public static Field GenerateFractalNoise(double scalingFactor, int width, int height, int depth, double fraction)
        {
            Perlin p = new Perlin(0);

            Field f = new Field(width, height);

            for (int z = 1; z < depth + 1; z++)
                for (int y = 0; y < height; y++)
                    for (int x = 0; x < width; x++)
                    {
                        double xIn = (double)x / (scalingFactor / z);
                        double yIn = (double)y / (scalingFactor / z);
                        double zIn = (double)z;

                        double noiseValue = p.Noise(xIn, yIn, zIn) / Math.Pow(z, fraction) + 0.5;
                        f.Data[x, y] += noiseValue;
                    }

            f = Normalize(f);

            return f;
        }

        public static Field Normalize(Field input)
        {
            Field output = new Field(input.Width, input.Height);
            double min = double.MaxValue;
            double max = double.MinValue;

            for (int y = 0; y < input.Height; y++)
                for (int x = 0; x < input.Width; x++)
                {
                    min = Math.Min(input.Data[x, y], min);
                    max = Math.Max(input.Data[x, y], max);
                }

            double divisor = max - min;

            for (int y = 0; y < input.Height; y++)
                for (int x = 0; x < input.Width; x++)
                    output.Data[x, y] = (input.Data[x, y] - min) / divisor;

            return output;
        }

        public static Field Slice(Field input, double lowerBound, double upperBound)
        {
            Field output = new Field(input.Width, input.Height);
            double[,] data = input.Data;

            for (int y = 0; y < input.Height; y++)
                for (int x = 0; x < input.Width; x++)
                    if (data[x, y] > lowerBound && data[x, y] < upperBound)
                        output.Data[x, y] = data[x, y];
                    else if (data[x, y] <= lowerBound)
                        output.Data[x, y] = lowerBound;
                    else
                        output.Data[x, y] = upperBound;

            output = Normalize(output);

            return output;
        }

        public static Field ReshapeSquare(Field input)
        {
            Field output = new Field(input.Width, input.Height);
            double[,] data = input.Data;

            for (int y = 0; y < input.Height; y++)
                for (int x = 0; x < input.Width; x++)
                    output.Data[x, y] = data[x, y] * data[x, y];

            output = Normalize(output);

            return output;
        }

        public static Field Sum(params Field[] fields)
        {
            Field output = new Field(fields[0].Width, fields[0].Height);

            for (int z = 0; z < fields.GetLength(0); z++)
                for (int y = 0; y < fields[z].Height; y++)
                    for (int x = 0; x < fields[z].Width; x++)
                        output.Data[x, y] += fields[z].Data[x, y];

            output = Normalize(output);
            return output;
        }

        public static Field CreateBoundary(Field input)
        {
            Field output = new Field(input.Width, input.Height);

            return output;
        }

        public static Field CreateCentralDepression(Field input)
        {
            Field output = new Field(input.Width, input.Height);

            int cX = input.Width / 2;
            int cY = input.Height / 2;

            float fraction = 0;

            Vector2 center = new Vector2(cX, cY);
            Vector2 current = Vector2.Zero;

            for (int y = 0; y < input.Height; y++)
                for (int x = 0; x < input.Width; x++)
                {
                    current.X = x;
                    current.Y = y;

                    Vector2 displaced = current - center;

                    if (displaced.Length() <= cX)
                    {
                        fraction = (current - center).Length() / cX;
                        output.Data[x, y] = input.Data[x, y] * (1 - fraction) + fraction;
                    }
                    else
                        output.Data[x, y] = 1;
                }

            return output;
        }
    }
}
