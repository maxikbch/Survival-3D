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
        int chunkSize = 4;
        int worldSize;
        int worldRadio = 2; //MantenerImparidad
        int visionRange = 2; //RangoDeChunks
        float centration = 0;
        Chunk[,] chunks;
        Chunk[,] actualChunks;
        List<TrianglePrimitive> triangles = new List<TrianglePrimitive>();

        Matrix cubeWorld;
        CubePrimitive cube;

        public World()
        {
            worldSize = 1 + (worldRadio - 1) * 2;
            chunks = new Chunk[worldSize, worldSize];
            if (MathC.Even(chunkSize)) centration = 0.5f;
            for (int i = 0; i < worldSize; i++)
            {
                for (int j = 0; j < worldSize; j++)
                {
                    Vector3 position = new Vector3(
                        i * chunkSize - chunkSize / 2 - centration - (worldSize - 1) * chunkSize,
                        0,
                        j * chunkSize - chunkSize / 2 - centration - (worldSize - 1) * chunkSize);
                    chunks[i, j] = new Chunk(position);
                }
            }
        }

        public void Update(GameTime gameTime, Vector3 playerPosition)
        {
            SetActualChunks(playerPosition);
        }

        public void Draw(GraphicsDevice graphicsDevice, ContentManager content, Matrix view, Matrix projection)
        {
            WorldPoint[,] drawMatrix = GetDrawMatrix();
            AddFloorTriangles(graphicsDevice, content, drawMatrix);

            Matrix world = Matrix.CreateScale(10f);
            
            foreach (TrianglePrimitive triangle in triangles)
            {
                triangle.Draw(world, view, projection);
            }

            cube = new CubePrimitive(graphicsDevice, content);
            cubeWorld = Matrix.CreateTranslation(Vector3.Zero) * Matrix.CreateScale(10f);

            //cube.Draw(cubeWorld, view, projection);
        }

        private void SetActualChunks(Vector3 playerPosition)
        {
            int range = 1 + (visionRange - 1) * 2;
            actualChunks = new Chunk[range, range];
            Vector2 center = GetWorldViewCenter(playerPosition);
            for (int i = 0; i < range; i++)
            {
                for (int j = 0; j < range; j++)
                {
                    if (i - (visionRange - 1) + center.X >= 0 || j - (visionRange - 1) + center.Y >= 0)
                        actualChunks[i, j] = chunks[(int)(i - (visionRange - 1) + center.X), (int)(j - (visionRange - 1) + center.Y)];
                    else
                        actualChunks[i, j] = null;
                }
            }
        }

        private Vector2 GetWorldViewCenter(Vector3 playerPosition)
        {
            Vector2 planePlayerPosition = new Vector2(playerPosition.X, playerPosition.Z);
            Vector2 center = planePlayerPosition / (chunkSize + centration * 2);
            center = new Vector2(
                MathF.Floor(center.X + worldRadio - 1),
                MathF.Floor(center.Y + worldRadio - 1));
            return center;
        }

        private WorldPoint[,] GetDrawMatrix()
        {
            int range = 1 + (visionRange - 1) * 2;
            int size = chunkSize * worldSize;
            WorldPoint[,] DrawMatrix = new WorldPoint[size, size];
            for (int i = 0; i < range; i++)
            {
                for (int j = 0; j < range; j++)
                {
                    DrawMatrix = GetDrawMatrixP(DrawMatrix, actualChunks[i, j].points, i * chunkSize, j * chunkSize);
                }
            }
            return DrawMatrix;
        }

        private WorldPoint[,] GetDrawMatrixP(WorldPoint[,] mainMatrix, WorldPoint[,] matrix, int x, int y)
        {
            for (int i = 0; i < chunkSize; i++)
            {
                for (int j = 0; j < chunkSize; j++)
                {
                    mainMatrix[i + x, j + y] = matrix[i, j];
                }
            }
            return mainMatrix;
        }

        private void AddFloorTriangles(GraphicsDevice graphicsDevice, ContentManager content, WorldPoint[,] DrawMatrix)
        {
            triangles = new List<TrianglePrimitive>();
            for (int i = 0; i < worldSize * chunkSize - 1; i++)
            {
                for (int j = 0; j < worldSize * chunkSize - 1; j++)
                {
                    triangles.Add(new TrianglePrimitive(graphicsDevice, content, 
                        DrawMatrix[i, j].position, DrawMatrix[i + 1, j].position, DrawMatrix[i + 1, j + 1].position,
                        DrawMatrix[i, j].color, DrawMatrix[i + 1, j].color, DrawMatrix[i + 1, j + 1].color));
                    triangles.Add(new TrianglePrimitive(graphicsDevice, content,
                        DrawMatrix[i, j].position, DrawMatrix[i, j + 1].position, DrawMatrix[i + 1, j + 1].position,
                        DrawMatrix[i, j].color, DrawMatrix[i, j + 1].color, DrawMatrix[i + 1, j + 1].color));
                }
            }
        }

    }

    public class Chunk
    {
        int chunkSize = 10;
        public WorldPoint[,] points;

        public Chunk(Vector3 worldPosition)
        {
            points = new WorldPoint[chunkSize, chunkSize];
            for (int i = 0; i < chunkSize; i++)
            {
                for (int j = 0; j < chunkSize; j++)
                {
                    Vector3 position = new Vector3(i + worldPosition.X, 0, j + worldPosition.Z);
                    points[i, j] = new WorldPoint(position);
                }
            }
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
