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
        public int chunkSize = 9;
        public int worldSize;
        int worldRadio = 4; //MantenerImparidad
        int visionRange = 2; //RangoDeChunks
        float centration = 0;
        Chunk[,] chunks;
        Vector2 center;
        public ColorPoint[,] points;
        public Vector3 to000;


        public World()
        {
            worldSize = 1 + (worldRadio - 1) * 2;
            to000 = new Vector3(1, 0, 1) * (float)(worldSize * (chunkSize)) / 2;

            InitChunks();
            InitWorldPoints();
            SetChunkTriangles();
            FinalChunkGeneration();
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
                    if (i == 0 && j == 0) lastHighs.Add(MathC.IntRandom(chunks[i,j].biome.minHigh, chunks[i, j].biome.maxHigh));
                    if (i > 0) lastHighs.Add(chunks[i - 1,j].high);
                    if (j > 0) lastHighs.Add(chunks[i, j - 1].high);
                    int newHigh = lastHighs[MathC.IntRandom(0, lastHighs.Count - 1)];
                    int newHighTop = Math.Min(newHigh + 1, chunks[i, j].biome.maxHigh);
                    int newHighBottom = Math.Max(newHigh - 1, chunks[i, j].biome.minHigh);
                    newHigh = MathC.IntRandom(newHighBottom, newHighTop);
                    chunks[i, j].high = newHigh;
                }
            }
        }

        public void InitWorldPoints()
        {
            points = new ColorPoint[chunkSize * worldSize + 1, chunkSize * worldSize + 1];

            int[,] highs = GetPointHighsMatrix();

            for (int i = 0; i < chunkSize * worldSize + 1; i++)
            {
                for (int j = 0; j < chunkSize * worldSize + 1; j++)
                {
                    float y = highs[i, j];
                    float x = i - to000.X;
                    float z = j - to000.X;
                    points[i, j] = new ColorPoint(new Vector3(x, y - 1, z), Color.Green);
                }
            }

        }

        private int[,] GetPointHighsMatrix()
        {
            int[,] highs = new int[chunkSize * worldSize + 1, chunkSize * worldSize + 1];

            for (int i = 0; i < chunkSize * worldSize + 1; i++)
            {
                for (int j = 0; j < chunkSize * worldSize + 1; j++)
                {
                    highs[i, j] = GetWorldPointHigh(i, j);
                }
            }

            highs = SmoothHighsMatrix(highs);

            return highs;
        }

        private int[,] SmoothHighsMatrix(int[,] highs)
        {

            for (int i = 1; i < chunkSize * worldSize; i++)
            {
                for (int j = 1; j < chunkSize * worldSize; j++)
                {
                    highs[i, j] = SmoothHigh(highs[i, j - 1], highs[i, j], highs[i, j + 1]);
                }
            }

            for (int j = 1; j < chunkSize * worldSize; j++)
            {
                for (int i = 1; i < chunkSize * worldSize; i++)
                {
                    highs[i, j] = SmoothHigh(highs[i - 1, j], highs[i, j], highs[i + 1, j]);
                }
            }

            return highs;
        }

        private int SmoothHigh(int prehigh, int high, int posthigh)
        {
            if (prehigh == posthigh)
            {
                return posthigh;
            }
            return high;
        }

        public int GetWorldPointHigh(int x, int y)
        {
            int high;
            bool notX0 = x != 0;
            bool notY0 = y != 0;
            bool xBorder = x % (chunkSize) == 0;
            bool yBorder = y % (chunkSize) == 0;
            int xChunkMax = x / chunkSize;
            if (xChunkMax > worldSize - 1) { xChunkMax--; xBorder = false; }
            int yChunkMax = y / chunkSize;
            if (yChunkMax > worldSize - 1) { yChunkMax--; yBorder = false; }
            List<int> highs = new List<int>();
            highs.Add(chunks[xChunkMax, yChunkMax].high);

            if (xBorder && notX0) highs.Add(chunks[xChunkMax - 1, yChunkMax].high);
            if (yBorder && notY0) highs.Add(chunks[xChunkMax, yChunkMax - 1].high);
            if (xBorder && yBorder && notX0 && notY0) highs.Add(chunks[xChunkMax - 1, yChunkMax - 1].high);
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

        public void FinalChunkGeneration()
        {
            for (int i = 0; i < worldSize; i++)
            {
                for (int j = 0; j < worldSize; j++)
                {
                    chunks[i, j].GenerateAmbient(this, new Vector2(i, j) * chunkSize);
                }
            }

        }

        private ColorPoint[,] GetChunkPoints(int x, int y)
        {
            ColorPoint[,] _points = new ColorPoint[chunkSize + 1, chunkSize + 1];
            for (int i = 0; i < chunkSize + 1; i++)
            {
                for (int j = 0; j < chunkSize + 1; j++)
                {
                    _points[i, j] = points[i + chunkSize * x, j + chunkSize * y];
                }
            }
            return _points;
        }

        private TrianglePrimitive[] GenerateTrianglesFromMatrix(ColorPoint[,] _points)
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
            if (p1.Y != p2.Y && p3.Y == p4.Y)
                return true;
            return false;
        }

        private List<TrianglePrimitive> GenerateQuad(ColorPoint p1, ColorPoint p2, ColorPoint p3, ColorPoint p4)
        {
            List<TrianglePrimitive> _triangles = new List<TrianglePrimitive>();
            if (!TakeOtherTriangles(p1.position, p2.position, p3.position, p4.position))
            {
                _triangles.Add(DrawGeometry.Triangle(p1, p2, p3));
                _triangles.Add(DrawGeometry.Triangle(p1, p4, p2));
            }
            else
            {
                _triangles.Add(DrawGeometry.Triangle(p3, p4, p2));
                _triangles.Add(DrawGeometry.Triangle(p3, p1, p4));
            }
            return _triangles;
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
            if (PlayerOutOfWorldSpace(pjRepos))
            {
                return 0;
            }
            Vector3 XZ = new Vector3(1, 0, 1);
            Vector3 p1 = points[(int)MathF.Floor(pjRepos.X), (int)MathF.Floor(pjRepos.Z)].position;
            Vector3 p2 = points[(int)MathF.Ceiling(pjRepos.X), (int)MathF.Ceiling(pjRepos.Z)].position;
            Vector3 p3 = points[(int)MathF.Ceiling(pjRepos.X), (int)MathF.Floor(pjRepos.Z)].position;
            Vector3 p4 = points[(int)MathF.Floor(pjRepos.X), (int)MathF.Ceiling(pjRepos.Z)].position;
            Vector3 pf;
            if (!TakeOtherTriangles(p1, p2, p3, p4))
            {
                pf = p1;
            }
            else
            {
                pf = p3;
            }
            Vector3 v1j = pf - pj;
            Vector3 n = GetFloorNormal(position);
            float y = (v1j.X * n.X + v1j.Z * n.Z) / n.Y + pf.Y;
            return y;
        }

        public Vector3 GetFloorNormal(Vector3 pj)
        {
            Vector3 pjRepos = pj + to000;
            if (PlayerOutOfWorldSpace(pjRepos))
            {
                return Vector3.Up;
            }
            Vector3 XZ = new Vector3(1,0,1);
            Vector3 p1 = points[(int)MathF.Floor(pjRepos.X), (int)MathF.Floor(pjRepos.Z)].position;
            Vector3 p2 = points[(int)MathF.Ceiling(pjRepos.X), (int)MathF.Ceiling(pjRepos.Z)].position;
            Vector3 p3 = points[(int)MathF.Ceiling(pjRepos.X), (int)MathF.Floor(pjRepos.Z)].position;
            Vector3 p4 = points[(int)MathF.Floor(pjRepos.X), (int)MathF.Ceiling(pjRepos.Z)].position;
            if (!TakeOtherTriangles(p1, p2, p3, p4))
            {
                if (Vector3.Distance(p3 * XZ, pj * XZ) < Vector3.Distance(p4 * XZ, pj * XZ))
                    return MathC.NormalWith3Points(p1, p2, p3);
                else
                    return MathC.NormalWith3Points(p1, p4, p2);
            } else
            {
                if (Vector3.Distance(p1 * XZ, pj * XZ) < Vector3.Distance(p2 * XZ, pj * XZ))
                    return MathC.NormalWith3Points(p3, p1, p4);
                else
                    return MathC.NormalWith3Points(p3, p4, p2);
            }
        }

        public bool IsPlane(Vector3 pj)
        {
            Vector3 pjRepos = pj;
            
            Vector3 p1 = points[(int)MathF.Floor(pjRepos.X), (int)MathF.Floor(pjRepos.Z)].position;
            Vector3 p2 = points[(int)MathF.Ceiling(pjRepos.X), (int)MathF.Ceiling(pjRepos.Z)].position;
            Vector3 p3 = points[(int)MathF.Ceiling(pjRepos.X), (int)MathF.Floor(pjRepos.Z)].position;
            Vector3 p4 = points[(int)MathF.Floor(pjRepos.X), (int)MathF.Ceiling(pjRepos.Z)].position;
            
            return MathC.NormalWith3Points(p1, p2, p3) == Vector3.Up &&
                MathC.NormalWith3Points(p1, p4, p2) == Vector3.Up &&
                MathC.NormalWith3Points(p3, p1, p4) == Vector3.Up &&
                MathC.NormalWith3Points(p3, p4, p2) == Vector3.Up;
        }

        private bool PlayerOutOfWorldSpace(Vector3 position)
        {
            return !MathC.BetweenValuesIncluded(position.X, 0, worldSize * chunkSize) ||
                   !MathC.BetweenValuesIncluded(position.Z, 0, worldSize * chunkSize);
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
        public AmbientObject[] ambientObjects = { };

        public Chunk(int chunkSize)
        {
            triangles = new TrianglePrimitive[(int)Math.Pow(chunkSize - 1, 2) * 2];
        }

        public void GenerateAmbient(World world, Vector2 position)
        {
            float amount = MathC.IntRandom(0, 3);
            int size = world.chunkSize - 1;
            List<AmbientObject> objects = new List<AmbientObject>();
            List<Vector3> usedPositions = new List<Vector3>();

            for(int i = 0; i < amount; i++)
            {
                Vector3 pos;
                bool notUsed;
                bool validPosition;
                do {
                    float X = MathC.IntRandom(0, world.chunkSize - 1) + position.X + 0.5f;
                    float Z = MathC.IntRandom(0, world.chunkSize - 1) + position.Y + 0.5f;
                    float Y = world.GetFloorHigh(new Vector3(X, 0, Z) - world.to000);
                    pos = new Vector3(X, Y, Z);
                    notUsed = !usedPositions.Exists(position => position == pos);
                    validPosition = world.IsPlane(pos - Vector3.One * 0.1f);
                } while (!notUsed && !validPosition);

                objects.Add(biome.elements[MathC.IntRandom(0, biome.elements.Length -1)].New(pos - world.to000));
            }

            ambientObjects = objects.ToArray();
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
            foreach(AmbientObject ambientObject in ambientObjects)
            {
                ambientObject.Draw(view, projection);
            }
        }

    }

    public class Biome
    {
        public Color color;
        public int minHigh;
        public int maxHigh;
        public AmbientObject[] elements;

        public Biome(Color color, int minHigh, int maxHigh, AmbientObject[] elements)
        {
            this.color = color;
            this.minHigh = minHigh;
            this.maxHigh = maxHigh;
            this.elements = elements;
        }

        public static Biome Llanura = new Biome(Color.Green, -1, 1,
            new AmbientObject[]{new Tree()}
            );

    }
}
