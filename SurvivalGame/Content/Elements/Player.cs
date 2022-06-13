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
    
    public class Player
    {
        public Vector3 position = Vector3.Zero;
        CylinderPrimitive body;
        Matrix bodyMatrix;
        float playerSpeed = 5f;
        Vector3 scale = new Vector3(0.5f, 0.5f, 0.5f);
        Quaternion rotation = Quaternion.Identity;

        public Player()
        {
            body = new CylinderPrimitive(SElem.graphicsDevice, SElem.content, Color.Blue);
        }

        public void Update(GameTime gameTime, Vector3 frontDirection, Vector3 rightDirection)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if (Keyboard.GetState().IsKeyDown(SKeys.playerMoveForward))
                position += frontDirection * deltaTime * playerSpeed;
            if (Keyboard.GetState().IsKeyDown(SKeys.playerMoveBackward))
                position -= frontDirection * deltaTime * playerSpeed;
            if (Keyboard.GetState().IsKeyDown(SKeys.playerMoveLeft))
                position -= rightDirection * deltaTime * playerSpeed;
            if (Keyboard.GetState().IsKeyDown(SKeys.playerMoveRight))
                position += rightDirection * deltaTime * playerSpeed;
        }

        public void UpdateY(float y, Vector3 normal)
        {
            position = new Vector3(position.X, y + scale.Y / 2, position.Z);
            Vector3 ortNormal = Vector3.Normalize(Vector3.Cross(normal, Vector3.Up)); //Vector normal al plano entre la normal y el vector up
            Vector3 proNormal = Vector3.Normalize(normal * new Vector3(1, 0, 1));
            float angle = MathC.AngleBetween2Vectors(normal, proNormal) / MathF.PI;
            if (normal == Vector3.Up) {
                ortNormal = Vector3.Right;
                angle = 0;
            }
            rotation = new Quaternion(ortNormal, angle);
        }

        public void Draw(Matrix view, Matrix projection)
        {
            bodyMatrix = Matrix.CreateScale(scale) * Matrix.CreateTranslation(position);
            bodyMatrix = Matrix.CreateScale(scale) * Matrix.CreateFromQuaternion(rotation) * Matrix.CreateTranslation(position);
            body.Draw(bodyMatrix, view, projection);
        }

    }

}
