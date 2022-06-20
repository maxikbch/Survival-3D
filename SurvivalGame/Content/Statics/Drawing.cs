using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using SurvivalGame.Geometries;

namespace SurvivalGame
{
    public class ColorPoint
    {
        public Vector3 position;
        public Color color;

        public ColorPoint(Vector3 position, Color color)
        {
            this.position = position;
            this.color = color;
        }
    }

    public class DrawGeometry
    {
        public static TrianglePrimitive Triangle(ColorPoint a, ColorPoint b, ColorPoint c)
        {
            GraphicsDevice graphicsDevice = SElem.graphicsDevice;
            ContentManager content = SElem.content;
            return new TrianglePrimitive(graphicsDevice, content,
                a.position, b.position, c.position,
                a.color, b.color, c.color);
        }

        public static List<TrianglePrimitive> Quad(ColorPoint p1, ColorPoint p2, ColorPoint p3, ColorPoint p4)
        {
            List<TrianglePrimitive> _triangles = new List<TrianglePrimitive>();
            _triangles.Add(Triangle(p1, p2, p3));
            _triangles.Add(Triangle(p1, p3, p4));
            return _triangles;
        }

        public static List<TrianglePrimitive> Cube(Vector3 position, Quaternion rotation, float radius, Color color)
        {
            List<ColorPoint> bottom = new List<ColorPoint>();
            List<ColorPoint> top = new List<ColorPoint>();
            List<TrianglePrimitive> triangles = new List<TrianglePrimitive>();

            Matrix transform = Matrix.CreateScale(radius) * Matrix.CreateFromQuaternion(rotation) * Matrix.CreateTranslation(position);

            float angle = MathF.PI / 4;
            for (int i = 0; i < 4; i++)
            {
                Vector3 rot = Vector3.Zero;
                if (i == 0) rot = new Vector3(-1, 0, -1);
                if (i == 1) rot = new Vector3(1, 0, -1);
                if (i == 2) rot = new Vector3(1, 0, 1);
                if (i == 3) rot = new Vector3(-1, 0, 1);
                Vector3 posTop = Vector3.Transform(Vector3.Normalize(rot + Vector3.Up), transform);
                Vector3 posBot = Vector3.Transform(Vector3.Normalize(rot - Vector3.Up), transform);
                angle += MathF.PI / 2;
                top.Add(new ColorPoint(posTop, color));
                bottom.Add(new ColorPoint(posBot, color));
            }

            triangles.AddRange(Quad(bottom[0], bottom[1], bottom[2], bottom[3]));
            triangles.AddRange(Quad(top[0], top[1], top[2], top[3]));

            for (int i = 0; i < 4; i++)
            {
                int x = i + 1;
                if (x == 4) x = 0;
                triangles.AddRange(Quad(top[i], top[x], bottom[x], bottom[i]));
            }
            return triangles;
        }

    }
}
