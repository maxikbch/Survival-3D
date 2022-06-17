using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System;
using System.Collections.Generic;
using SurvivalGame.Geometries;

namespace SurvivalGame.Elements
{

    public class AmbientObject
    {
        public Vector3 position;
        public Matrix world;

        public virtual void Update(GameTime gameTime)
        {
        }

        public virtual void Draw(Matrix view, Matrix projection)
        {
        }

        public virtual AmbientObject New(Vector3 position)
        {
            return null;
        }
    }

    public class Tree : AmbientObject
    {
        public TrianglePrimitive[] baseLog = { };
        public TrianglePrimitive[] topLog = { };
        public TrianglePrimitive[] leaves = { };

        public Tree(Vector3 position)
        {
            this.position = position;

            float bottomRadius = MathC.FloatRandom(0.2f, 0.4f);
            float topRadius = MathC.FloatRandom(bottomRadius*2/6, bottomRadius * 4 / 6);
            float leavesRadius = MathC.FloatRandom(bottomRadius * 12 / 6, bottomRadius * 16 / 6);

            float bottomRotation = MathC.FloatRandom(0f, MathF.PI / 2);
            float topRotation = MathC.FloatRandom(0f, 1f) + bottomRotation;

            float topY = MathC.FloatRandom(1.2f, 1.7f);
            float dispRadius = (bottomRadius - topRadius) * 1 / 3;
            float topX = MathC.FloatRandom(-dispRadius, dispRadius);
            float topZ = MathC.FloatRandom(-dispRadius, dispRadius);
            Vector3 topDisp = new Vector3(topX, topY, topZ);

            ColorPoint[] bottom = GenerateBase(bottomRadius, bottomRotation, Vector3.Zero, Color.Brown);
            ColorPoint[] top = GenerateBase(topRadius, topRotation, topDisp, Color.Brown);
            ColorPoint[] mid = GenerateMid(bottom, top);

            baseLog = FormLog(bottom, mid);
            topLog = FormLog(mid, top);
            leaves = FormLeaves(topDisp, leavesRadius);
        }

        public Tree()
        {
        }

        public override AmbientObject New(Vector3 position)
        {
            return new Tree(position);
        }

        public ColorPoint[] GenerateBase(float radius, float rotation, Vector3 move, Color color) {
            List<ColorPoint> points = new List<ColorPoint>();
            float angle = rotation + MathF.PI / 4;
            for(int i = 0; i < 4; i++)
            {
                Vector3 pos = new Vector3(radius * MathF.Cos(angle), 0, radius * MathF.Sin(angle)) + move;
                angle += MathF.PI /2;
                points.Add(new ColorPoint(pos, color));
            }
            return points.ToArray();
        }

        public ColorPoint[] GenerateMid(ColorPoint[] bottom, ColorPoint[] top)
        {
            List<ColorPoint> points = new List<ColorPoint>();
            float y = 0.3f;
            for (int i = 0; i < 4; i++)
            {
                Vector3 vector = top[i].position - bottom[i].position;
                float alpha = (y - bottom[i].position.Y) / vector.Y;
                Vector3 pos = bottom[i].position + vector * alpha;
                points.Add(new ColorPoint(pos, bottom[i].color));
            }
            return points.ToArray();
        }

        public TrianglePrimitive[] FormLog(ColorPoint[] bottom, ColorPoint[] top)
        {
            List<TrianglePrimitive> triangles = new List<TrianglePrimitive>();
            triangles.AddRange(DrawGeometry.Quad(bottom[0], bottom[1], bottom[2], bottom[3]));
            triangles.AddRange(DrawGeometry.Quad(top[0], top[1], top[2], top[3]));

            for (int i = 0; i < 4; i++)
            {
                int x = i + 1;
                if (x == 4) x = 0;
                triangles.AddRange(DrawGeometry.Quad(top[i], top[x], bottom[x], bottom[i]));
            }

            return triangles.ToArray();
        }

        public TrianglePrimitive[] FormLeaves(Vector3 position, float radius)
        {
            List<TrianglePrimitive> triangles = new List<TrianglePrimitive>();

            for (int i = 0; i < 3; i++)
            {
                Vector3 rotationVector = new Vector3(MathC.FloatRandom(0, 1), MathC.FloatRandom(0, 1), MathC.FloatRandom(0, 1));
                rotationVector = Vector3.Normalize(rotationVector);
                float rotationAngle = MathC.FloatRandom(0, MathF.PI*2);
                Quaternion rotation = Quaternion.CreateFromAxisAngle(rotationVector, rotationAngle);
                triangles.AddRange(DrawGeometry.Cube(position, rotation, radius, Color.LimeGreen));
            }
            
            return triangles.ToArray();
        }

        public override void Update(GameTime gameTime)
        {
            
        }

        public override void Draw(Matrix view, Matrix projection)
        {
            world = Matrix.CreateTranslation(position);
            List<TrianglePrimitive> triangles = new List<TrianglePrimitive>();
            triangles.AddRange(baseLog);
            triangles.AddRange(topLog);
            triangles.AddRange(leaves);
            foreach (TrianglePrimitive triangle in triangles)
            {
                triangle.Draw(world, view, projection);
            }
        }

    }

}
