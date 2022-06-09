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
        int worldRadio = 3; //MantenerImparidad
        int visionRange = 2; //RangoDeChunks
        float centration = 0;
        Chunk[,] chunks;
        Vector2 center;
        public WorldPoint[,] points;
        public Vector3 to000;
        public float floorScale = 1f;

        public World()
        {
            worldSize = 1 + (worldRadio - 1) * 2;
            to000 = new Vector3(1, 0, 1) * (float)(worldSize * (chunkSize) - 1) / 2;
            InitChunks();
            InitWorldPoints();
        }

        public void InitChunks()
        {
            chunks = new Chunk[worldSize, worldSize];
            for (int i = 0; i < worldSize; i++)
            {
                for (int j = 0; j < worldSize; j++)
                {
                    Vector2 chunkPos = new Vector2(i * (chunkSize), j * (chunkSize)) - new Vector2(to000.X, to000.X);
                    var e = 0;
                    chunks[i, j] = new Chunk(chunkSize, chunkPos);
                }
            }
            int chunkLastSlot = chunkSize - 1;

            for (int i = 0; i < worldSize - 1; i++)
            {
                for (int j = 0; j < worldSize - 1; j++)
                {
                    for (int e = 0; e < chunkLastSlot; e++) {
                        chunks[i, j].AddTriangle(
                            chunks[i, j].points[chunkLastSlot, e],
                            chunks[i + 1, j].points[0, e + 1],
                            chunks[i + 1, j].points[0, e]);
                        chunks[i, j].AddTriangle(
                            chunks[i, j].points[chunkLastSlot, e],
                            chunks[i, j].points[chunkLastSlot, e + 1],
                            chunks[i + 1, j].points[0, e + 1]);
                        chunks[i, j].AddTriangle(
                            chunks[i, j].points[e, chunkLastSlot],
                            chunks[i, j + 1].points[e, 0],
                            chunks[i, j + 1].points[e + 1, 0]);
                        chunks[i, j].AddTriangle(
                            chunks[i, j].points[e, chunkLastSlot],
                            chunks[i, j + 1].points[e + 1, 0],
                            chunks[i, j].points[e + 1, chunkLastSlot]);
                    }
                    chunks[i, j].AddTriangle(
                            chunks[i, j].points[chunkLastSlot, chunkLastSlot],
                            chunks[i, j + 1].points[chunkLastSlot, 0],
                            chunks[i + 1, j + 1].points[0, 0]);
                    chunks[i, j].AddTriangle(
                            chunks[i, j].points[chunkLastSlot, chunkLastSlot],
                            chunks[i + 1, j + 1].points[0, 0],
                            chunks[i + 1, j].points[0, chunkLastSlot]);
                }
            }

            for (int i = 0; i < worldSize - 1; i++)
            {
                int j = worldSize - 1;
                for (int e = 0; e < chunkLastSlot; e++)
                {
                    chunks[i, j].AddTriangle(
                            chunks[i, j].points[chunkLastSlot, e],
                            chunks[i + 1, j].points[0, e + 1],
                            chunks[i + 1, j].points[0, e]);
                    chunks[i, j].AddTriangle(
                        chunks[i, j].points[chunkLastSlot, e],
                        chunks[i, j].points[chunkLastSlot, e + 1],
                        chunks[i + 1, j].points[0, e + 1]);
                }
            }

            for (int j = 0; j < worldSize - 1; j++)
            {
                int i = worldSize - 1;
                for (int e = 0; e < chunkLastSlot; e++)
                {
                    chunks[i, j].AddTriangle(
                        chunks[i, j].points[e, chunkLastSlot],
                        chunks[i, j + 1].points[e, 0],
                        chunks[i, j + 1].points[e + 1, 0]);
                    chunks[i, j].AddTriangle(
                        chunks[i, j].points[e, chunkLastSlot],
                        chunks[i, j + 1].points[e + 1, 0],
                        chunks[i, j].points[e + 1, chunkLastSlot]);
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
                    int x = i / chunkSize;
                    int y = j / chunkSize;
                    int a = i % chunkSize;
                    int b = j % chunkSize;
                    points[i, j] = chunks[x, y].points[a, b];
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
                for (int j = (int) center.Y - range; j <= (int)center.Y + range; j++)
                {
                    if (i < worldSize && j < worldSize && i > -1 && j > -1)
                    {
                        chunks[i, j].Draw(view, projection);
                    }
                }
            }
        }

        public float GetFloorHigh(Vector3 position)
        {
            Vector3 pj = position;
            Vector3 pjRepos = position + to000;
            if (
                !MathC.BetweenValuesIncluded((int)MathF.Floor(pjRepos.X), 0, worldSize * chunkSize - 1) ||
                !MathC.BetweenValuesIncluded((int)MathF.Floor(pjRepos.Z), 0, worldSize * chunkSize - 1))
            {
                return 0;
            }
            Vector3 p1 = points[(int)MathF.Floor(pjRepos.X), (int)MathF.Floor(pjRepos.Z)].position;
            Vector3 v1j = p1 - pj;
            Vector3 n = GetFloorNormal(position);
            float y = (v1j.X * n.X + v1j.Z * n.Z) / n.Y + p1.Y;
            return y;
        }

        public Vector3 GetFloorNormal(Vector3 position)
        {
            Vector3 pjRepos = position + to000;
            if (
                !MathC.BetweenValuesIncluded((int)MathF.Floor(pjRepos.X), 0, worldSize * chunkSize - 1) ||
                !MathC.BetweenValuesIncluded((int)MathF.Floor(pjRepos.Z), 0, worldSize * chunkSize - 1) ||
                !MathC.BetweenValuesIncluded((int)MathF.Ceiling(pjRepos.X), 0, worldSize * chunkSize - 1) ||
                !MathC.BetweenValuesIncluded((int)MathF.Ceiling(pjRepos.Z), 0, worldSize * chunkSize - 1))
            {
                return Vector3.Up;
            }
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


    }

    public class Chunk
    {
        public List<TrianglePrimitive> triangles = new List<TrianglePrimitive>();
        public Biome biome = Biome.Llanura;
        public WorldPoint[,] points;
        public Vector2 center;

        public Chunk(int chunkSize, Vector2 center)
        {
            points = new WorldPoint[chunkSize, chunkSize];
            this.center = center;
            InitPoints(chunkSize);
            InitChunkTriangles(chunkSize);
        }

        public void InitPoints(int chunkSize)
        {
            points = new WorldPoint[chunkSize, chunkSize];
            Random rnd = new Random();
            float y = rnd.Next(0, 2);
            for (int i = 0; i < chunkSize; i++)
            {
                for (int j = 0; j < chunkSize; j++)
                {
                    float x = i + center.X;
                    float z = j + center.Y;
                    Vector3 position = new Vector3(x, y - 1, z);
                    Color color = biome.color;
                    points[i, j] = new WorldPoint(position, color);
                }
            }
        }

        public void InitChunkTriangles(int chunkSize)
        {
            for (int i = 0; i < chunkSize - 1; i++)
            {
                for (int j = 0; j < chunkSize - 1; j++)
                {
                    triangles.AddRange(AddFloorTriangles(i, j));
                }
            }
        }

        private List<TrianglePrimitive> AddFloorTriangles(int i, int j)
        {
            GraphicsDevice graphicsDevice = SElem.graphicsDevice;
            ContentManager content = SElem.content;
            List<TrianglePrimitive> triangles = new List<TrianglePrimitive>();
            triangles.Add(new TrianglePrimitive(graphicsDevice, content,
                points[i + 1, j + 1].position, points[i + 1, j].position, points[i, j].position,
                points[i + 1, j + 1].color, points[i + 1, j].color, points[i, j].color));
            triangles.Add(new TrianglePrimitive(graphicsDevice, content,
                points[i, j].position, points[i, j + 1].position, points[i + 1, j + 1].position,
                points[i, j].color, points[i, j + 1].color, points[i + 1, j + 1].color));
            return triangles;
        }

        public void AddTriangle(WorldPoint a, WorldPoint b, WorldPoint c)
        {
            GraphicsDevice graphicsDevice = SElem.graphicsDevice;
            ContentManager content = SElem.content;
            triangles.Add(new TrianglePrimitive(graphicsDevice, content,
                a.position, b.position, c.position,
                a.color, b.color, c.color));
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
