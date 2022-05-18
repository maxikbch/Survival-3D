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
        int chunkSize = 20;
        int worldSize;
        int worldRadio = 100; //MantenerImparidad
        int visionRange = 3; //RangoDeChunks
        float centration = 0;
        Chunk[,] chunks;
        Chunk[,] actualChunks;
        List<TrianglePrimitive> triangles = new List<TrianglePrimitive>();
        Vector2 lastCenter = Vector2.UnitX;
        Vector2 center;
        Matrix cubeWorld;
        CubePrimitive cube;
        public WorldPoint[,] points;
        public WorldPoint[,] drawPointsMatrix;
        bool actualiceWorldDraw = true;
        bool actualiceTriangles = false;

        public World()
        {
            worldSize = 1 + (worldRadio - 1) * 2;
            if (MathC.Even(chunkSize)) centration = 0.5f;
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
                    chunks[i, j] = new Chunk(chunkSize);
                }
            }
        }

        public void InitWorldPoints()
        {
            points = new WorldPoint[chunkSize * worldSize, chunkSize * worldSize];
            Vector3 to000 = new Vector3(1, 0, 1) * ((float)((worldSize * chunkSize) - 1) / 2);
            for (int i = 0; i < chunkSize * worldSize; i++)
            {
                for (int j = 0; j < chunkSize * worldSize; j++)
                {
                    float y = 2f * MathF.Sin(i * MathF.PI/12) * MathF.Sin(j * MathF.PI / 12);
                    Vector3 position = new Vector3(i, y, j);
                    points[i, j] = new WorldPoint(position - to000);
                }
            }
        }

        public void Update(GameTime gameTime, Vector3 playerPosition)
        {
            SetWorldViewCenter(playerPosition);
            if (actualiceWorldDraw)
            {
                SetActualChunks();
                SetDrawPointsMatrix();
            }
        }

        public void Draw(GraphicsDevice graphicsDevice, ContentManager content, Matrix view, Matrix projection)
        {
            if (actualiceTriangles)
            {
                AddFloorTriangles(graphicsDevice, content);
                actualiceTriangles = false;
            }

            Matrix world = Matrix.CreateTranslation(0, 0, 0) * Matrix.CreateScale(5f);
            
            foreach (TrianglePrimitive triangle in triangles)
            {
                triangle.Draw(world, view, projection);
            }

            cube = new CubePrimitive(graphicsDevice, content);
            cubeWorld = Matrix.CreateTranslation(Vector3.Zero) * Matrix.CreateScale(10f);

            //cube.Draw(cubeWorld, view, projection);
        }

        private void SetActualChunks()
        {
            int range = 1 + (visionRange - 1) * 2;
            actualChunks = new Chunk[range, range];
            for (int i = 0; i < range; i++)
            {
                for (int j = 0; j < range; j++)
                {
                    if (i - (visionRange - 1) + center.X >= 0 && j - (visionRange -1) + center.Y >= 0)
                        actualChunks[i, j] = chunks[(int)(i - (visionRange - 1) + center.X), (int)(j - (visionRange - 1) + center.Y)];
                    else
                        actualChunks[i, j] = null;
                }
            }
        }

        private void SetWorldViewCenter(Vector3 playerPosition)
        {
            Vector2 planePlayerPosition = new Vector2(playerPosition.X, playerPosition.Z);
            Vector2 _center = planePlayerPosition / (chunkSize + centration * 2);
            _center = new Vector2(
                MathF.Floor(_center.X + worldRadio - 1),
                MathF.Floor(_center.Y + worldRadio - 1));
            center = _center;
            if (center != lastCenter)
            {
                actualiceWorldDraw = true;
                actualiceTriangles = true;
            }
            else
                actualiceWorldDraw = false;
            lastCenter = center;
        }

        private void SetDrawPointsMatrix()
        {
            int range = 1 + (visionRange - 1) * 2;
            int size = chunkSize * range;
            drawPointsMatrix = new WorldPoint[size, size];
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    int x = i + chunkSize * ((int)center.X - visionRange + 1);
                    int y = j + chunkSize * ((int)center.Y - visionRange + 1);
                    if (MathC.InIndex(points.Length, x) && MathC.InIndex(points.Length, y))
                        drawPointsMatrix[i, j] = points[x, y];
                    else
                        drawPointsMatrix[i, j] = null;
                }
            }
        }

        private void AddFloorTriangles(GraphicsDevice graphicsDevice, ContentManager content)
        {
            int range = 1 + (visionRange - 1) * 2;
            triangles = new List<TrianglePrimitive>();
            WorldPoint[,] dm = drawPointsMatrix;
            for (int i = 0; i < range * chunkSize - 1; i++)
            {
                for (int j = 0; j < range * chunkSize - 1; j++)
                {
                    if (dm[i, j] != null && dm[i + 1, j + 1] != null) {
                        if (dm[i + 1, j] != null)
                        {
                            /*
                            triangles.Add(new TrianglePrimitive(graphicsDevice, content,
                                dm[i, j].position, dm[i + 1, j].position, dm[i + 1, j + 1].position,
                                dm[i, j].color, dm[i + 1, j].color, dm[i + 1, j + 1].color));
                            */
                            triangles.Add(new TrianglePrimitive(graphicsDevice, content,
                                dm[i + 1, j + 1].position, dm[i + 1, j].position, dm[i, j].position,
                                dm[i + 1, j + 1].color, dm[i + 1, j].color, dm[i, j].color));
                        }
                        if (dm[i, j + 1] != null)
                        {
                            triangles.Add(new TrianglePrimitive(graphicsDevice, content,
                                dm[i, j].position, dm[i, j + 1].position, dm[i + 1, j + 1].position,
                                dm[i, j].color, dm[i, j + 1].color, dm[i + 1, j + 1].color));
                        }
                    }
                }
            }
        }

    }

    public class Chunk
    {

        public Chunk(int chunkSize)
        {
            
        }

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(GameTime gameTime)
        {
        }

    }

    public class WorldPoint
    {
        public Vector3 position;
        public Color color = Color.Green;

        public WorldPoint(Vector3 position)
        {
            this.position = position;
        }

    }
}
