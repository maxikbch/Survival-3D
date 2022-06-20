using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace SurvivalGame.Cameras
{
    internal class FollowCamera : Camera
    {
        private Vector3 PlayerPosition = Vector3.Zero;

        float YaxisAngle = 0;
        float Yhigh = 2;
        float Xdistance = 3.5f;

        private Vector3 AnglePosition;

        // Angles

        public FollowCamera(float aspectRatio, Vector3 position) : base(aspectRatio)
        {
            AnglePosition = new Vector3(-Xdistance, 0, 0);
            Position = position + AnglePosition;
            UpdateCameraVectors();
            CalculateView();
        }

        public float MovementSpeed { get; set; } = 100f;
        public float MouseSensitivity { get; set; } = 5f;

        private void CalculateView()
        {
            View = Matrix.CreateLookAt(Position, PlayerPosition, UpDirection);
        }

        public override float YAxisAngle()
        {
            return YaxisAngle;
        }

        public override void UpdatePlayerPosition(Vector3 position)
        {
            PlayerPosition = position;
        }

        /// <inheritdoc />
        public override void Update(GameTime gameTime)
        {
            var elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            ProcessKeyboard(elapsedTime);

            Position = AnglePosition + PlayerPosition;

            CalculateView();
        }

        private void ProcessKeyboard(float elapsedTime)
        {
            var keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(SKey.rotateCameraLeft))
            {
                YaxisAngle += 1f * elapsedTime;
            }

            if (keyboardState.IsKeyDown(SKey.rotateCameraRight))
            {
                YaxisAngle += -1f * elapsedTime;
            }

            AnglePosition = new Vector3(Xdistance * MathF.Cos(YaxisAngle), Yhigh, Xdistance * MathF.Sin(YaxisAngle));

            UpdateCameraVectors();
        }

        public void UpdateCameraVectors()
        {
            // Calculate the new Front vector
            Vector3 tempFront;
            tempFront.X = PlayerPosition.X - Position.X;
            tempFront.Y = 0;
            tempFront.Z = PlayerPosition.Z - Position.Z;

            FrontDirection = Vector3.Normalize(tempFront);

            // Also re-calculate the Right and Up vector
            // Normalize the vectors, because their length gets closer to 0 the more you look up or down which results in slower movement.
            RightDirection = Vector3.Normalize(Vector3.Cross(FrontDirection, Vector3.Up));
            UpDirection = Vector3.Normalize(Vector3.Cross(RightDirection, FrontDirection));
        }
    }
}