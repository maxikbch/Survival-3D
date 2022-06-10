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
        int chunkSize = 9;
        int worldSize;
        int worldRadio = 4; //MantenerImparidad
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
            to000 = new Vector3(1, 0, 1) * (float)(worldSize * (chunkSize)) / 2;

            InitChunks();
            InitWorldPoints();
            SetChunkTriangles();
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

            for (int i = 0; i < worldSize; i++)
            {
                for (int j = 0; j < worldSize; j++)
                {
                    List<int> lastHighs = new List<int>();
                    if (i == 0 && j == 0) lastHighs.Add(0);
                    if (i > 0) lastHighs.Add(chunks[i - 1,j].high);
                    if (j > 0) lastHighs.Add(chunks[i, j - 1].high);
                    int newHigh = lastHighs[MathC.IntRandom(0, lastHighs.Count - 1)];
                    newHigh = MathC.IntRandom(newHigh - 1, newHigh + 1);
                    chunks[i, j].high = newHigh;
                }
            }
        }

        public void InitWorldPoints()
        {
            points = new WorldPoint[chunkSize * worldSize + 1, chunkSize * worldSize + 1];

            for (int i = 0; i < chunkSize * worldSize + 1; i++)
            {
                for (int j = 0; j < chunkSize * worldSize + 1; j++)
                {
                    float y = GetWorldPointHigh(i, j);
                    float x = i - to000.X;
                    float z = j - to000.X;
                    points[i, j] = new WorldPoint(new Vector3(x, y - 1, z), Color.Green);
                }
            }

        }

        public int GetWorldPointHigh(int x, int y)
        {
            int high;
            bool xBorder = x % (chunkSize) == 0 && x != 0;
            bool yBorder = y % (chunkSize) == 0 && y != 0;
            int xChunkMax = x / chunkSize;
            if (xChunkMax > worldSize - 1) { xChunkMax--; xBorder = false; }
            int yChunkMax = y / chunkSize;
            if (yChunkMax > worldSize - 1) { yChunkMax--; yBorder = false; }
            List<int> highs = new List<int>();
            highs.Add(chunks[xChunkMax, yChunkMax].high);
            if (xBorder) highs.Add(chunks[xChunkMax - 1, yChunkMax].high);
            if (yBorder) highs.Add(chunks[xChunkMax, yChunkMax - 1].high);
            if (xBorder && yBorder) highs.Add(chunks[xChunkMax - 1, yChunkMax - 1].high);
            high = highs[MathC.IntRandom(highs.Count - 1)];
            return high;
        }

        public void SetChunkTriangles()
        {
            for (int i = 0; i < worldSize; i++)
            {
                for (int j = 0; j < worldSize; j++)
                {
                    chunks[i, j].triangles = GenerateTrianglesFromMatrix(GetChunkPoints(i, j));
                }
            }

        }

        private WorldPoint[,] GetChunkPoints(int x, int y)
        {
            WorldPoint[,] _points = new WorldPoint[chunkSize + 1, chunkSize + 1];
            for (int i = 0; i < chunkSize + 1; i++)
            {
                for (int j = 0; j < chunkSize + 1; j++)
                {
                    _points[i, j] = points[i + chunkSize * x, j + chunkSize * y];
                }
            }
            return _points;
        }

        private TrianglePrimitive[] GenerateTrianglesFromMatrix(WorldPoint[,] _points)
        {
            List<TrianglePrimitive> _triangles = new List<TrianglePrimitive>();
            for (int i = 0; i < chunkSize; i++)
            {
                for (int j = 0; j < chunkSize; j++)
                {
                    _triangles.AddRange(GenerateQuad(_points[i, j], _points[i + 1, j + 1], _points[i + 1, j], _points[i, j + 1]));
                }
            }
            return _triangles.ToArray();
        }

        private bool TakeOtherTriangles(Vector3 p1, Vector3 p2, Vector3 p3, Vector3 p4)
        {
            if(p1.Y != p2.Y && p3.Y == p4.Y)
                return true;
            if (p1.Y == p2.Y && p3.Y == p4.Y && p1.Y > p3.Y)
                return true;
            return false;
        }

        private List<TrianglePrimitive> GenerateQuad(WorldPoint p1, WorldPoint p2, WorldPoint p3, WorldPoint p4)
        {
            List<TrianglePrimitive> _triangles = new List<TrianglePrimitive>();
            //!TakeOtherTriangles(p1.position, p2.position, p3.position, p4.position)
            if (true)
            {
                _triangles.Add(GenerateTriangle(p1,p2,p3));
                _triangles.Add(GenerateTriangle(p1,p4,p2));
            }
            else
            {
                _triangles.Add(GenerateTriangle(p3,p2,p4));
                _triangles.Add(GenerateTriangle(p3,p4,p1));
            }
            return _triangles;
        }

        public TrianglePrimitive GenerateTriangle(WorldPoint a, WorldPoint b, WorldPoint c)
        {
            GraphicsDevice graphicsDevice = SElem.graphicsDevice;
            ContentManager content = SElem.content;
            return new TrianglePrimitive(graphicsDevice, content,
                a.position, b.position, c.position,
                a.color, b.color, c.color);
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
            Vector3 p3 = points[(int)MathF.Ceiling(pjRepos.X), (int)MathF.Ceiling(pjRepos.Z)].position;
            Vector3 p2;
            Vector2 _p1 = new Vector2(MathF.Floor(pjRepos.X), MathF.Ceiling(pjRepos.Z));
            Vector2 _p2 = new Vector2(MathF.Ceiling(pjRepos.X), MathF.Floor(pjRepos.Z));
            Vector2 _pj = new Vector2(pjRepos.X, pjRepos.Z);
            if (Vector2.Distance(_p1, _pj) < Vector2.Distance(_p2, _pj))
                p2 = points[(int)_p1.X, (int)_p1.Y].position;
            else
                p2 = points[(int)_p2.X, (int)_p2.Y].position;
            return MathC.NormalWith3Points(p1, p2, p3);
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
        public TrianglePrimitive[] triangles;
        public Biome biome = Biome.Llanura;
        public int high;

        public Chunk(int chunkSize)
        {
            triangles = new TrianglePrimitive[(int)Math.Pow(chunkSize - 1, 2) * 2];
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
