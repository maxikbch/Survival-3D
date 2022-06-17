using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SurvivalGame.Cameras;
using SurvivalGame.Geometries;
using System.Collections.Generic;

namespace SurvivalGame
{
    public class Game1 : Game
    {
        //Graficos

        private GraphicsDeviceManager _graphics;

        private SpriteBatch _spriteBatch;

        //Camara

        private Camera Camera;

        private TargetCamera ShadowCamera;

        private Point screenSize { get; set; }

        //Matrices

        private Matrix World { get; set; }
        private Matrix View { get; set; }
        private Matrix Projection { get; set; }

        //Shader

        private Effect Effect { get; set; }

        private Vector3 LightPosition = new Vector3(250, 250, 0);

        private readonly float ShadowCameraFarPlaneDistance = 3000f;

        private readonly float ShadowCameraNearPlaneDistance = 5f;

        private const int ShadowmapSize = 2048;

        private RenderTarget2D ShadowMapRenderTarget;

        //GameInstance

        private GameInstance game;

        //Carpetas

        private static string AssetsFolder = "Assets/";
        private static string ShadersFolder = "Shaders/";
        private static string SpriteFontsFolder = "SpriteFonts/";

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            screenSize = new Point(GraphicsDevice.Viewport.Width / 2, GraphicsDevice.Viewport.Height / 2);
            
            Camera = new FollowCamera(GraphicsDevice.Viewport.AspectRatio, new Vector3(0, 2, 0));

            ShadowCamera = new TargetCamera(1f, LightPosition, Vector3.Zero);
            ShadowCamera.BuildProjection(1f, ShadowCameraNearPlaneDistance, ShadowCameraFarPlaneDistance,
                MathHelper.PiOver2);

            SElem.effect = Effect;
            SElem.content = Content;
            SElem.graphicsDevice = GraphicsDevice;
            SElem.spriteBatch = new SpriteBatch(GraphicsDevice);
            SElem.fontA = Content.Load<SpriteFont>(SpriteFontsFolder + "Cascadia/CascadiaCodePL");

            World = Matrix.Identity;
            View = Matrix.CreateLookAt(Vector3.UnitZ * 150, Vector3.Zero, Vector3.Up);
            var viewMatrix = Matrix.CreateLookAt(new Vector3(0, 0, 50), Vector3.Forward, Vector3.Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 1, 250);

            game = new GameInstance();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            Effect = Content.Load<Effect>(ShadersFolder + "ShaderGod");

            Effect.Parameters["lightPosition"]?.SetValue(LightPosition);

            ShadowMapRenderTarget = new RenderTarget2D(GraphicsDevice, ShadowmapSize, ShadowmapSize, false,
                SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.PlatformContents);

            Effect.Parameters["ambientColor"]?.SetValue(Color.White.ToVector3());
            Effect.Parameters["diffuseColor"]?.SetValue(Color.White.ToVector3());
            Effect.Parameters["specularColor"]?.SetValue(Color.White.ToVector3());

            Effect.Parameters["KAmbient"]?.SetValue(0.65f);
            Effect.Parameters["KDiffuse"]?.SetValue(0.4f);
            Effect.Parameters["KSpecular"]?.SetValue(0.15f);
            Effect.Parameters["shininess"]?.SetValue(1f);


            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            //GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed
            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            Camera.UpdatePlayerPosition(game.player.position);
            Camera.Update(gameTime);

            ShadowCamera.Position = LightPosition;
            ShadowCamera.BuildView();

            game.player.Update(gameTime, Camera.FrontDirection, Camera.RightDirection);
            game.player.UpdateY(game.world.GetFloorHigh(game.player.position), game.world.GetFloorNormal(game.player.position));

            game.world.Update(gameTime, game.player.position);

            // TODO: Add your update logic here

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {

            GraphicsDevice.RasterizerState = new RasterizerState { CullMode = CullMode.None };
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            //Seteamos render target en el shadowmap
            GraphicsDevice.SetRenderTarget(ShadowMapRenderTarget);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

            //Effect.CurrentTechnique = Effect.Techniques["DepthPass"];

            //Dibujamos en el shadowmap
            game.world.Draw(GraphicsDevice, Content, ShadowCamera.View, ShadowCamera.Projection);

            game.player.Draw(ShadowCamera.View, ShadowCamera.Projection);

            //Seteamos el render target en null para dibujar directamente en la pantalla
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.CornflowerBlue, 1f, 0);

            // Para dibujar le modelo necesitamos pasarle informacion que el efecto esta esperando.
            //Effect.CurrentTechnique = Effect.Techniques["DrawShadowedPCF"];
            Effect.Parameters["View"].SetValue(Camera.View);
            Effect.Parameters["Projection"].SetValue(Camera.Projection);
            Effect.Parameters["eyePosition"]?.SetValue(Camera.Position);
            Effect.Parameters["lightPosition"]?.SetValue(LightPosition);
            Effect.Parameters["shadowMapSize"]?.SetValue(Vector2.One * ShadowmapSize);
            Effect.Parameters["shadowMap"]?.SetValue(ShadowMapRenderTarget);
            Effect.Parameters["LightViewProjection"]?.SetValue(ShadowCamera.View * ShadowCamera.Projection);

            // TODO: Add your drawing code here
            game.world.Draw(GraphicsDevice, Content, Camera.View, Camera.Projection);

            game.player.Draw(Camera.View, Camera.Projection);

            Vector3 normal = game.world.GetFloorNormal(game.player.position);
            string txt = "Normal: {" + normal.X + ";" + normal.Y + ";" + normal.Z + "}";
            string txt2 = "";
            Text.DrawTextFromCenter(txt, 0, -200, 1f, Color.Red, SElem.fontA);
            Text.DrawTextFromCenter(txt2, 0, -225, 1f, Color.Red, SElem.fontA);

            base.Draw(gameTime);
        }

        private void DrawShadows(GameTime gameTime)
        {
            
        }

    }
}
