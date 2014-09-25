using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FlowFields
{
    class TerrainGenerator
    {
        public static Field CreateCave(int wavelength, int width, int height, int depth, double fractionalThreshold, double altitudinalThreshold)
        {
            Field terrain = FieldTools.GenerateFractalNoise(wavelength, width, height, depth, fractionalThreshold);
            Field lowerSlice = FieldTools.Slice(terrain, 0, altitudinalThreshold);
            Field upperSlice = FieldTools.Slice(terrain, altitudinalThreshold, 1);
            lowerSlice = FieldTools.ReshapeSquare(lowerSlice);
            lowerSlice = FieldTools.ReshapeSquare(lowerSlice);
            lowerSlice = FieldTools.ReshapeSquare(lowerSlice);
            terrain = FieldTools.Sum(lowerSlice, upperSlice);

            terrain = FieldTools.CreateCentralDepression(terrain);

            return terrain;
        }
    }
}
