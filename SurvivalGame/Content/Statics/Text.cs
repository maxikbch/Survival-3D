using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using SurvivalGame.Geometries;

namespace SurvivalGame
{
    public class Text
    {

        public static void DrawCenterText(string msg, float escala, Color color, SpriteFont font)
        {
            var W = SElem.graphicsDevice.Viewport.Width;
            var H = SElem.graphicsDevice.Viewport.Height;
            var size = font.MeasureString(msg) * escala;
            SElem.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null,
                Matrix.CreateScale(escala) * Matrix.CreateTranslation((W - size.X) / 2, (H - size.Y) / 2, 0));
            SElem.spriteBatch.DrawString(font, msg, new Vector2(0, 0), color);
            SElem.spriteBatch.End();
        }

        public static void DrawCenterTextY(string msg, float Y, float escala, Color color, SpriteFont font)
        {
            var W = SElem.graphicsDevice.Viewport.Width;
            var H = SElem.graphicsDevice.Viewport.Height;
            var size = font.MeasureString(msg) * escala;
            SElem.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null,
                Matrix.CreateScale(escala) * Matrix.CreateTranslation((W - size.X) / 2, Y, 0));
            SElem.spriteBatch.DrawString(font, msg, new Vector2(0, 0), color);
            SElem.spriteBatch.End();
        }

        public static void DrawTextFromCenter(string msg, float X, float Y, float escala, Color color, SpriteFont font)
        {
            var W = SElem.graphicsDevice.Viewport.Width;
            var H = SElem.graphicsDevice.Viewport.Height;
            var size = font.MeasureString(msg) * escala;
            SElem.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null,
                Matrix.CreateScale(escala) * Matrix.CreateTranslation(((W - size.X) / 2) + X, ((H - size.Y) / 2) + Y, 0));
            SElem.spriteBatch.DrawString(font, msg, new Vector2(0, 0), color);
            SElem.spriteBatch.End();
        }

        public static void DrawTextFromCenterNotCentered(string msg, float X, float Y, float escala, Color color, SpriteFont font)
        {
            var W = SElem.graphicsDevice.Viewport.Width;
            var H = SElem.graphicsDevice.Viewport.Height;
            SElem.spriteBatch.Begin(SpriteSortMode.Deferred, null, null, null, null, null,
                Matrix.CreateScale(escala) * Matrix.CreateTranslation((W / 2) + X, (H / 2) + Y, 0));
            SElem.spriteBatch.DrawString(font, msg, new Vector2(0, 0), color);
            SElem.spriteBatch.End();
        }

        public static void DrawSelectedText(string msg, float X, float Y, float escala, float selectorY, SpriteFont font, float selectorX = 0)
        {
            if(selectorY == 0 && selectorX == 0)
                DrawTextFromCenter(msg, X, Y, escala, Color.Yellow, font);
            else
                DrawTextFromCenter(msg, X, Y, escala, Color.White, font);
        }
    }
}
