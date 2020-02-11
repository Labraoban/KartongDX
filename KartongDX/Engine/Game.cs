using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Drawing;
using SharpDX;
using SharpDX.Windows;
using SharpDX.DXGI;

using D3D11 = SharpDX.Direct3D11;
using SharpDX.Direct3D;



namespace KartongDX.Engine
{
	class Game : IDisposable
	{
        private ApplicationSettings settings;

        private Rendering.PostProcessing.PostEffectHandler effectHandler;
        private Rendering.Window window;
		private Rendering.Renderer renderer;
		private Rendering.Camera camera;

        private Resources.ResourceManager resourceManager;

        private States.GameStateMachine stateMachine;

        private GameTime gameTime;

        private Input input;

        GameObject light;

		Vector3 cameraEulerRotation = new Vector3();

		public Game()
		{
            settings = new ApplicationSettings();
            settings.Load();

            int width = settings.WindowWidth;
            int height = settings.WindowHeight;

            int render_width = settings.RenderWidth;
            int render_height = settings.RenderHeight;

            effectHandler = new Rendering.PostProcessing.PostEffectHandler();
            window = new Rendering.Window("Testing", width, height, settings.Resizeable);
			renderer = new Rendering.Renderer(window, render_width, render_height, 60, effectHandler);
			camera = new Rendering.Camera(0, 0, width, height, 50);

            gameTime = new GameTime();

			resourceManager = new Resources.ResourceManager();
            InitializeDeviceResources();
            PostResourceLoadInit();

            input = new Input();
            stateMachine = new States.GameStateMachine();
            States.GameState testState = new States._TestState();
            stateMachine.Attach(testState);
            stateMachine.AddToStack("TestState");

            testState.Load(resourceManager, renderer.Device);
            testState.Init(resourceManager);

            renderer.Init();
            renderer.SetCamera(camera);

			camera.SetPosition(new Vector3(0, 0, -5));



            light = new GameObject();
            light.SetScale(new Vector3(0.1f, 0.1f, 0.1f));
            light.SetPosition(new Vector3(-5, 0, -5));



            Logger.Write(LogType.Info, "Engine Initialized");
        }

        private void InitializeDeviceResources()
        {
            Logger.StartLogSection("Loading Device Resources");
            resourceManager.Meshes.Load(new Resources.Types.MeshResourceDescription("data/models/KDX_CUBE.fbx", "KDX_CUBE"));
            resourceManager.Meshes.Load(new Resources.Types.MeshResourceDescription("data/models/KDX_CUBE_IN.fbx", "KDX_CUBE_IN"));
            resourceManager.Shaders.Load(new Resources.Types.ShaderResourceDescription("data/shaders/KDX_SKYBOX_ENV_RENDERER.hlsl", "KDX_SKYBOX_ENV_RENDERER", renderer.Device));
            resourceManager.Shaders.Load(new Resources.Types.ShaderResourceDescription("data/shaders/KDX_SKYBOX_IRR_RENDERER.hlsl", "KDX_SKYBOX_IRR_RENDERER", renderer.Device));

            List<string> renderBufferCopyDefines = new List<string>();
            if(settings.Force24BitColor)
                renderBufferCopyDefines.Add("_24_BIT_COLOR_");
                
            resourceManager.Shaders.Load(new Resources.Types.ShaderResourceDescription("data/shaders/KDX_EFFECTS/KDX_COPY_TO_BACK.hlsl", "KDX_COPY_TO_BACK", renderer.Device, renderBufferCopyDefines));
            
            
            resourceManager.Shaders.Load(new Resources.Types.ShaderResourceDescription("data/shaders/KDX_EFFECTS/KDX_GRAYSCALE.hlsl", "KDX_GRAYSCALE", renderer.Device));
            resourceManager.Shaders.Load(new Resources.Types.ShaderResourceDescription("data/shaders/KDX_EFFECTS/KDX_BLACK_AND_WHITE.hlsl", "KDX_BLACK_AND_WHITE", renderer.Device));
            resourceManager.Shaders.Load(new Resources.Types.ShaderResourceDescription("data/shaders/KDX_EFFECTS/KDX_DEPTH.hlsl", "KDX_DEPTH", renderer.Device));
            Logger.EndLogSection();
        }

        private void PostResourceLoadInit()
        {
            Logger.StartLogSection("Post Resource Load Init");
            Logger.StartLogSection("Attaching Post Effects");
            effectHandler.AttachEffect(new Rendering.PostProcessing.PostEffect(resourceManager.Shaders.GetFromAlias("KDX_COPY_TO_BACK").Shader, int.MaxValue));
            effectHandler.AttachEffect(new Rendering.PostProcessing.PostEffect(resourceManager.Shaders.GetFromAlias("KDX_BLACK_AND_WHITE").Shader, 0));
            effectHandler.AttachEffect(new Rendering.PostProcessing.PostEffect(resourceManager.Shaders.GetFromAlias("KDX_DEPTH").Shader, 0));
            Logger.EndLogSection();

            effectHandler.EnableEffect("KDX_COPY_TO_BACK");
            Logger.EndLogSection();
        }

        public void Run()
		{
			renderer.SetEngineLoopCallback(Update);
			renderer.StartRenderLoop();
		}

		private void Update()
		{
			input.Update();
            gameTime.Update();
            stateMachine.Update(gameTime);

            // --
            float x = 0;
            float y = 0;

			x -= input.GetKey(SharpDX.DirectInput.Key.D) ? 1.0f : 0.0f;
			x += input.GetKey(SharpDX.DirectInput.Key.A) ? 1.0f : 0.0f;

			y += input.GetKey(SharpDX.DirectInput.Key.W) ? 1.0f : 0.0f;
			y -= input.GetKey(SharpDX.DirectInput.Key.S) ? 1.0f : 0.0f;

			cameraEulerRotation.X = 0;

            cameraEulerRotation.Y += input.GetKey(SharpDX.DirectInput.Key.Right) ? 1.0f : 0.0f;
            cameraEulerRotation.Y -= input.GetKey(SharpDX.DirectInput.Key.Left) ? 1.0f : 0.0f;

            cameraEulerRotation.Z += input.GetKey(SharpDX.DirectInput.Key.Up) ? 1.0f : 0.0f;
            cameraEulerRotation.Z -= input.GetKey(SharpDX.DirectInput.Key.Down) ? 1.0f : 0.0f;

            cameraEulerRotation.Y = cameraEulerRotation.Y % 360;
            cameraEulerRotation.Z = cameraEulerRotation.Z % 360;


            camera.SetEulerRotation(cameraEulerRotation * 2.0f);

            Vector3 movement = new Vector3(x, 0, y) * 0.1f;
            Vector3 newPos = camera.GetPosition() + AdjustDirectionToCamera(movement);
			camera.SetPosition(newPos);
			camera.ResolveDirty();

            renderer.perFrameBuffer.lightDir = light.GetPosition();
            renderer.perFrameBuffer.lightDir.Normalize();
            // --

            List<Scene> scenes = stateMachine.GetScenes();
            renderer.StageScenes(scenes);

            if (input.GetKeyPressed(SharpDX.DirectInput.Key.K))
                effectHandler.EnableEffect("KDX_DEPTH");
            if (input.GetKeyPressed(SharpDX.DirectInput.Key.L))
                effectHandler.DisableEffect("KDX_DEPTH");
        }

		Vector3 AdjustDirectionToCamera(Vector3 move)
		{
			Vector3 forward = camera.WorldMatrix.Forward;
			Vector3 right = camera.WorldMatrix.Right;
			forward.Y = 0;
			right.Y = 0;

			forward.Normalize();
			right.Normalize();
			
			return (move.X * right + move.Z * forward);
		}

		public void Dispose()
		{
			renderer.Dispose();
			window.Dispose();
			resourceManager.Dispose();
		}


    }
}
