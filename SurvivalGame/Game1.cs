using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SurvivalGame.Cameras;
using SurvivalGame.Geometries;

namespace SurvivalGame
{
    public class Game1 : Game
    {
        //Graficos

        private GraphicsDeviceManager _graphics;

        private SpriteBatch _spriteBatch;

        //Camara

        private Camera Camera;
        private Point screenSize { get; set; }

        //Matrices

        private Matrix World { get; set; }
        private Matrix View { get; set; }
        private Matrix Projection { get; set; }

        //Shader

        private Effect Effect { get; set; }

        //GameInstance

        private GameInstance game = new GameInstance();

        //Carpetas

        private static string AssetsFolder = "Assets/";
        private static string ShadersFolder = "Shaders/";

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            screenSize = new Point(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            
            Camera = new FreeCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0, 1, 0), screenSize);


            World = Matrix.Identity;
            View = Matrix.CreateLookAt(Vector3.UnitZ * 150, Vector3.Zero, Vector3.Up);
            var viewMatrix = Matrix.CreateLookAt(new Vector3(0, 0, 50), Vector3.Forward, Vector3.Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 250);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            Effect = Content.Load<Effect>(ShadersFolder + "ShaderBlingPhong");

            Effect.Parameters["lightPosition"].SetValue(new Vector3(0, 1000, 0));

            Effect.Parameters["ambientColor"].SetValue(Color.White.ToVector3());
            Effect.Parameters["diffuseColor"].SetValue(Color.White.ToVector3());
            Effect.Parameters["specularColor"].SetValue(Color.White.ToVector3());

            Effect.Parameters["KAmbient"].SetValue(0.7f);
            Effect.Parameters["KDiffuse"].SetValue(0.6f);
            Effect.Parameters["KSpecular"].SetValue(0.3f);


            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            game.world.Update(gameTime, Vector3.Zero);

            Camera.Update(gameTime);

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            GraphicsDevice.RasterizerState = new RasterizerState { CullMode = CullMode.None };
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            // Para dibujar le modelo necesitamos pasarle informacion que el efecto esta esperando.
            Effect.Parameters["View"].SetValue(Camera.View);
            Effect.Parameters["Projection"].SetValue(Camera.Projection);
            Effect.Parameters["eyePosition"].SetValue(Camera.Position);

            // TODO: Add your drawing code here
            game.world.Draw(GraphicsDevice, Content, Camera.View, Camera.Projection);



            base.Draw(gameTime);
        }
    }
}
