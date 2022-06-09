using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using SurvivalGame.Geometries;

namespace SurvivalGame
{
    public class MathC
    {
        public static float ToRadians(float angle)
        {
            return MathF.PI * 2 * angle / 360;
        }

        public static bool Even(int number)
        {
            int result = number % 2;
            if (result == 0) return true;
            return false;
        }

        public static float Sign(float number)
        {
            if (number >= 0) return 1;
            return -1;
        }

        public static float Locate(float number, float min, float max)
        {
            float varation = max - min;
            while (number > max) number -= varation;
            while (number < min) number += varation;
            return number;
        }

        public static bool InIndex(int lenght, int number)
        {
            if (number >= lenght) return false;
            if (number < 0) return false;
            return true;
        }

        public static bool BetweenValuesIncluded(int number, int min, int max)
        {
            if (number < min) return false;
            if (number > max) return false;
            return true;
        }
    }
}
