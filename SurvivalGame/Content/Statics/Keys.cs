using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using SurvivalGame.Geometries;
using Microsoft.Xna.Framework.Input;

namespace SurvivalGame
{
    public class SKey
    {
        public SKey(Keys key)
        {
            this.key = key;
        }

        Keys key;
        float cooldown = 0;

        public bool Pressed()
        {
            return Keyboard.GetState().IsKeyDown(key);
        }

        public bool PressedCD(float newCD, float deltaTime)
        {
            bool pressed = Keyboard.GetState().IsKeyDown(key);
            if (pressed & cooldown <= 0)
            {
                cooldown = newCD;
                return true;
            } else
            {
                cooldown -= deltaTime;
                return false;
            }
        }

        public static Keys centerFreeCamera = Keys.G;
        public static Keys rotateCameraLeft = Keys.Q;
        public static Keys rotateCameraRight = Keys.E;

        public static Keys playerMoveForward = Keys.W;
        public static Keys playerMoveBackward = Keys.S;
        public static Keys playerMoveLeft = Keys.A;
        public static Keys playerMoveRight = Keys.D;

        public static SKey switchXRay = new SKey(Keys.X);
    }
}
