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
    
    public class World
    {
        int chunkSize = 10;
        int worldSize;
        int worldRadio = 20; //MantenerImparidad
        int visionRange = 3; //RangoDeChunks
        float centration = 0;
        Chunk[,] chunks;
        Vector2 center;
        public WorldPoint[,] points;
        public Vector3 to000;
        public float floorScale = 1f;

        public World()
        {
            worldSize = 1 + (worldRadio - 1) * 2;
            to000 = new Vector3(1, 0, 1) * ((float)((worldSize * chunkSize) - 1) / 2);
            if (MathC.Even(chunkSize)) centration = 0.5f;
            InitChunks();
            InitWorldPoints();
            InitChunkTriangles();
        }

        public void InitChunks()
        {
            chunks = new Chunk[worldSize, worldSize];
            for (int i = 0; i < worldSize; i++)
            {
                for (int j = 0; j < worldSize; j++)
                {
                    chunks[i, j] = new Chunk(chunkSize);
                }
            }
        }

        public void InitWorldPoints()
        {
            points = new WorldPoint[chunkSize * worldSize, chunkSize * worldSize];
            for (int i = 0; i < chunkSize * worldSize; i++)
            {
                for (int j = 0; j < chunkSize * worldSize; j++)
                {
                    float x = i - to000.X;
                    float z = j - to000.Z;
                    float y = 1f * MathF.Sin(x * MathF.PI/12) * MathF.Sin(z * MathF.PI / 12);
                    Vector3 position = new Vector3(x, y - 1, z);
                    int chunkX = (int)MathF.Floor(i / chunkSize);
                    int chunkZ = (int)MathF.Floor(j / chunkSize);
                    Color color = chunks[chunkX, chunkZ].GetColor();
                    points[i, j] = new WorldPoint(position, color);
                }
            }
        }

        public void InitChunkTriangles()
        {
            for (int i = 0; i < chunkSize * worldSize - 1; i++)
            {
                for (int j = 0; j < chunkSize * worldSize - 1; j++)
                {
                    int chunkX = (int)MathF.Floor(i / chunkSize);
                    int chunkZ = (int)MathF.Floor(j / chunkSize);
                    chunks[chunkX, chunkZ].triangles.AddRange(AddFloorTriangles(SElem.graphicsDevice, SElem.content, i, j));
                }
            }
        }

        public void Update(GameTime gameTime, Vector3 playerPosition)
        {
            SetWorldViewCenter(playerPosition);
        }

        public void Draw(GraphicsDevice graphicsDevice, ContentManager content, Matrix view, Matrix projection)
        {

            int range = 1 + (visionRange - 1) * 2;

            for (int i = (int) center.X - range; i <= (int)center.X + range; i++)
            {
                for (int j = (int)center.Y - range; j <= (int)center.Y + range; j++)
                {
                    chunks[i, j].Draw(view, projection);
                }
            }
        }

        public float GetFloorHigh(Vector3 position)
        {
            Vector3 pj = position;
            Vector3 pjRepos = position + to000;
            Vector3 p1 = points[(int)MathF.Floor(pjRepos.X), (int)MathF.Floor(pjRepos.Z)].position;
            Vector3 v1j = p1 - pj;
            Vector3 n = GetFloorNormal(position);
            float y = (v1j.X * n.X + v1j.Z * n.Z) / n.Y + p1.Y;
            return y;
        }

        public Vector3 GetFloorNormal(Vector3 position)
        {
            Vector3 pjRepos = position + to000;
            Vector3 p1 = points[(int)MathF.Floor(pjRepos.X), (int)MathF.Floor(pjRepos.Z)].position;
            Vector3 p2 = points[(int)MathF.Floor(pjRepos.X), (int)MathF.Ceiling(pjRepos.Z)].position;
            Vector3 p3 = points[(int)MathF.Ceiling(pjRepos.X), (int)MathF.Ceiling(pjRepos.Z)].position;
            Vector3 v12 = (p1 - p2);
            Vector3 v13 = (p1 - p3);
            Vector3 n = Vector3.Cross(v12, v13);
            return n;
        }


        private void SetWorldViewCenter(Vector3 playerPosition)
        {
            Vector2 planePlayerPosition = new Vector2(playerPosition.X, playerPosition.Z);
            Vector2 _center = planePlayerPosition / (chunkSize + centration * 2);
            _center = new Vector2(
                MathF.Floor(_center.X + worldRadio - 1),
                MathF.Floor(_center.Y + worldRadio - 1));
            center = _center;
        }

        private List<TrianglePrimitive> AddFloorTriangles(GraphicsDevice graphicsDevice, ContentManager content, int i, int j)
        {
            List<TrianglePrimitive> triangles = new List<TrianglePrimitive>();
            triangles.Add(new TrianglePrimitive(graphicsDevice, content,
                points[i + 1, j + 1].position, points[i + 1, j].position, points[i, j].position,
                points[i + 1, j + 1].color, points[i + 1, j].color, points[i, j].color));
            triangles.Add(new TrianglePrimitive(graphicsDevice, content,
                points[i, j].position, points[i, j + 1].position, points[i + 1, j + 1].position,
                points[i, j].color, points[i, j + 1].color, points[i + 1, j + 1].color));
            return triangles;
        }

    }

    public class Chunk
    {
        public List<TrianglePrimitive> triangles = new List<TrianglePrimitive>();
        public Biome biome = Biome.Llanura;
        public WorldPoint[,] points;

        public Color GetColor()
        {
            return biome.color;
        }

        public Chunk(int chunkSize)
        {
            points = new WorldPoint[chunkSize, chunkSize];
        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(Matrix view, Matrix projection)
        {
            Matrix world = Matrix.CreateTranslation(0, 0, 0);
            foreach (TrianglePrimitive triangle in triangles)
            {
                triangle.Draw(world, view, projection);
            }
        }

    }

    public class WorldPoint
    {
        public Vector3 position;
        public Color color;

        public WorldPoint(Vector3 position, Color color)
        {
            this.position = position;
            this.color = color;
        }

    }

    public class Biome
    {
        public Color color;

        public Biome(Color color)
        {
            this.color = color;
        }

        public static Biome Llanura = new Biome(Color.Green);

    }
}
