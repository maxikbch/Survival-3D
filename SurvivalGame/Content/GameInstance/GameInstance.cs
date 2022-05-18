using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using System.Collections.Generic;
using SurvivalGame.Elements;

namespace SurvivalGame
{
    public class GameInstance
    {
       
        public float KeyCoolDown = 0f;

        public World world = new World();

        public Player player = new Player();

        public GameInstance()
        {
            
        }

    }
}
