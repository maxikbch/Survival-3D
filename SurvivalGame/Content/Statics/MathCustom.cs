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
        public static bool BetweenValuesIncluded(float number, int min, int max)
        {
            if (number < min) return false;
            if (number > max) return false;
            return true;
        }

        public static float AngleBetween2Vectors(Vector3 a, Vector3 b)
        {
            float num = Vector3.Dot(a, b);
            float den = Vector3.Distance(Vector3.Zero, a) * Vector3.Distance(Vector3.Zero, b);
            
            return MathF.Acos(num / den);
        }

        public static Vector3 NormalWith3Points(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            Vector3 v12 = (p1 - p2);
            Vector3 v13 = (p1 - p3);
            Vector3 n = Vector3.Cross(v12, v13);
            n = Vector3.Normalize(n);
            return n;
        }

        public static float FloatRandom(float max)
        {
            Random rnd = new Random();
            float value = (float)rnd.NextDouble();
            return value * max;
        }

        public static float FloatRandom(float min = 0, float max = 1)
        {
            Random rnd = new Random();
            float value = (float)rnd.NextDouble();
            return value * (max - min) + min;
        }

        public static int IntRandom(int max)
        {
            Random rnd = new Random();
            int value = rnd.Next(0, max + 1);
            return value;
        }

        public static int IntRandom(int min = 0, int max = 1)
        {
            Random rnd = new Random();
            int value = rnd.Next(min, max + 1);
            return value;
        }

        public static bool BoolRandom()
        {
            int value = IntRandom();
            if (value == 1) return true;
            return false;
        }

        public static bool BoolSwitch(bool input)
        {
            if (input) return false;
            return true;
        }

    }
}
