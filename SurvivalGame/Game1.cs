using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SurvivalGame.Cameras;
using SurvivalGame.Geometries;
using System;
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

        private Vector3 LightAngle = new Vector3(1, 1, 0);
        private float LightDistance = 50;
        private Vector3 LightPosition;

        private readonly float ShadowCameraFarPlaneDistance = 3000f;

        private readonly float ShadowCameraNearPlaneDistance = 5f;

        private const int ShadowmapSize = 1024 * 20;

        //Render Targets

        private RenderTarget2D ShadowMapRenderTarget;

        private RenderTarget2D ProcesedRenderTarget;

        private RenderTarget2D BorderRenderTarget;

        private RenderTarget2D NormalsRenderTarget;

        private FullScreenQuad FullScreenQuad;

        //GameInstance

        private GameInstance game;

        //xRay
        private bool xRayOn = false;

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

            ShadowCamera = new TargetCamera(1f, LightAngle * LightDistance, Vector3.Zero);
            ShadowCamera.BuildProjection(1f, ShadowCameraNearPlaneDistance, ShadowCameraFarPlaneDistance,
                MathHelper.PiOver2);


            SElem.effectDirectory = ShadersFolder + "ShaderGod";
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

            Effect = Content.Load<Effect>(SElem.effectDirectory);

            Effect.Parameters["lightPosition"]?.SetValue(LightAngle * LightDistance);

            ShadowMapRenderTarget = new RenderTarget2D(GraphicsDevice, ShadowmapSize, ShadowmapSize, false,
                SurfaceFormat.Single, DepthFormat.Depth24, 0, RenderTargetUsage.PlatformContents);

            ProcesedRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0,
                RenderTargetUsage.DiscardContents);

            BorderRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0,
                RenderTargetUsage.DiscardContents);

            NormalsRenderTarget = new RenderTarget2D(GraphicsDevice, GraphicsDevice.Viewport.Width,
                GraphicsDevice.Viewport.Height, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0,
                RenderTargetUsage.DiscardContents);

            Effect.Parameters["ambientColor"]?.SetValue(Color.White.ToVector3());
            Effect.Parameters["diffuseColor"]?.SetValue(Color.White.ToVector3());
            Effect.Parameters["specularColor"]?.SetValue(Color.White.ToVector3());

            Effect.Parameters["KAmbient"]?.SetValue(0.65f);
            Effect.Parameters["KDiffuse"]?.SetValue(0.4f);
            Effect.Parameters["KSpecular"]?.SetValue(0.15f);
            Effect.Parameters["shininess"]?.SetValue(1f);

            FullScreenQuad = new FullScreenQuad(GraphicsDevice);


            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            //GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed

            //GameTimeCicle(gameTime);

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            LightPosition = LightAngle * LightDistance + game.player.position;

            if (Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            if (SKey.switchXRay.PressedCD(0.2f, deltaTime))
                xRayOn = MathC.BoolSwitch(xRayOn);

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
            GraphicsDevice.RasterizerState = RasterizerState.CullNone;
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            if (!xRayOn)
                DrawWithoutBorders();
            else
                DrawWithBorders();


            Vector3 normal = game.world.GetFloorNormal(game.player.position);
            Vector3 position = game.player.position;
            position = new Vector3(MathF.Round(position.X), MathF.Round(position.Y), MathF.Round(position.Z));
            //string txt = "Normal: {" + normal.X + ";" + normal.Y + ";" + normal.Z + "}";
            string txt = "Posicion: {" + position.X + ";" + position.Y + ";" + position.Z + "}";
            string txt2 = "";
            Text.DrawTextFromCenter(txt, 0, -200, 1f, Color.Red, SElem.fontA);
            Text.DrawTextFromCenter(txt2, 0, -225, 1f, Color.Red, SElem.fontA);

            base.Draw(gameTime);
        }

        private void DrawWithBorders() 
        {
            DrawShadowMap();

            DrawMainRenderTarget(ProcesedRenderTarget);

            DrawBorderRenderTarget();

            //Dibujo los renderTargets en la pantalla

            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

            Effect.CurrentTechnique = Effect.Techniques["DrawRenderTargets"];

            Effect.Parameters["borderTexture"]?.SetValue(BorderRenderTarget);
            Effect.Parameters["procesedTexture"]?.SetValue(ProcesedRenderTarget);

            FullScreenQuad.Draw(Effect);

        }

        private void DrawWithoutBorders()
        {
            DrawShadowMap();

            DrawMainRenderTarget(null);

        }

        private void DrawShadowMap()
        {
            Effect.Parameters["View"].SetValue(ShadowCamera.View);
            Effect.Parameters["Projection"].SetValue(ShadowCamera.Projection);

            //Seteamos render target en el shadowmap
            GraphicsDevice.SetRenderTarget(ShadowMapRenderTarget);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

            Effect.CurrentTechnique = Effect.Techniques["DepthPass"];

            //Dibujamos en el shadowmap
            game.world.DrawFloor(GraphicsDevice, Content, ShadowCamera.View, ShadowCamera.Projection);
            game.world.DrawElements(GraphicsDevice, Content, ShadowCamera.View, ShadowCamera.Projection);

            game.player.Draw(ShadowCamera.View, ShadowCamera.Projection);
        }

        private void DrawMainRenderTarget(RenderTarget2D render)
        {
            //Seteamos el render target en null para dibujar directamente en la pantalla
            GraphicsDevice.SetRenderTarget(render);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

            // Para dibujar le modelo necesitamos pasarle informacion que el efecto esta esperando.
            //Effect.CurrentTechnique = Effect.Techniques["DrawShadowedPCF"];
            Effect.CurrentTechnique = Effect.Techniques["BasicColorDrawing"];
            Effect.Parameters["View"].SetValue(Camera.View);
            Effect.Parameters["Projection"].SetValue(Camera.Projection);
            Effect.Parameters["eyePosition"]?.SetValue(Camera.Position);
            Effect.Parameters["lightPosition"]?.SetValue(LightPosition);
            Effect.Parameters["shadowMapSize"]?.SetValue(Vector2.One * ShadowmapSize);
            Effect.Parameters["shadowMap"]?.SetValue(ShadowMapRenderTarget);
            Effect.Parameters["LightViewProjection"]?.SetValue(ShadowCamera.View * ShadowCamera.Projection);

            // TODO: Add your drawing code here
            game.world.DrawFloor(GraphicsDevice, Content, Camera.View, Camera.Projection);
            game.world.DrawElements(GraphicsDevice, Content, Camera.View, Camera.Projection);

            game.player.Draw(Camera.View, Camera.Projection);
        }

        private void DrawBorderRenderTarget()
        {
            //Seteamos el render target en null para dibujar directamente en la pantalla
            GraphicsDevice.SetRenderTarget(NormalsRenderTarget);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

            // Para dibujar le modelo necesitamos pasarle informacion que el efecto esta esperando.
            Effect.CurrentTechnique = Effect.Techniques["DrawNormals"];
            Effect.Parameters["View"].SetValue(Camera.View);
            Effect.Parameters["Projection"].SetValue(Camera.Projection);

            // TODO: Add your drawing code here
            game.world.DrawElements(GraphicsDevice, Content, Camera.View, Camera.Projection);
            //game.world.DrawFloor(GraphicsDevice, Content, Camera.View, Camera.Projection);

            game.player.Draw(Camera.View, Camera.Projection);

            GraphicsDevice.SetRenderTarget(BorderRenderTarget);
            GraphicsDevice.Clear(ClearOptions.Target | ClearOptions.DepthBuffer, Color.Black, 1f, 0);

            Effect.CurrentTechnique = Effect.Techniques["DrawBorders"];
            Effect.Parameters["normalsTexture"]?.SetValue(NormalsRenderTarget);
            Effect.Parameters["resolution"]?.SetValue(new Vector2(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height));

            FullScreenQuad.Draw(Effect);

        }


        private void GameTimeCicle(GameTime gameTime)
        {
            float time = (float)gameTime.TotalGameTime.TotalSeconds / 2;
            LightAngle = new Vector3(MathF.Cos(time), MathF.Sin(time), 0);
            if (MathF.Sin(time) >= 0)
            {
                Effect.Parameters["ambientColor"]?.SetValue(Color.White.ToVector3());
                Effect.Parameters["diffuseColor"]?.SetValue(Color.White.ToVector3());
                Effect.Parameters["specularColor"]?.SetValue(Color.White.ToVector3());
            }
            else
            {
                LightAngle *= -1;
                Effect.Parameters["ambientColor"]?.SetValue(Color.DarkBlue.ToVector3());
                Effect.Parameters["diffuseColor"]?.SetValue(Color.DarkBlue.ToVector3());
                Effect.Parameters["specularColor"]?.SetValue(Color.DarkBlue.ToVector3());
            }
        }

    }
}
