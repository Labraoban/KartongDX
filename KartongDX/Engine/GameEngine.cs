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
	class GameEngine : IDisposable
	{
        private ApplicationSettings settings;

        private Rendering.PostProcessing.PostEffectHandler effectHandler;
        private Rendering.DeviceResources deviceResources;
        private Rendering.Window window;
		private Rendering.Renderer renderer;

        private Resources.ResourceManager resourceManager;
        private States.GameStateMachine stateMachine;

        private GameTime gameTime;
        private Input input;
        private Accessors accessors;

        // TODO: remove light from here after adding light component;
        GameObject light;

		public GameEngine()
		{
            settings = new ApplicationSettings();
            settings.Load();

            int width = settings.WindowWidth;
            int height = settings.WindowHeight;

            int render_width = settings.RenderWidth;
            int render_height = settings.RenderHeight;

            resourceManager = new Resources.ResourceManager("data/");

            effectHandler = new Rendering.PostProcessing.PostEffectHandler();
            window = new Rendering.Window("Testing", width, height, settings.Resizeable);
            deviceResources = new Rendering.DeviceResources(window.GetHandle(), render_width, render_height, 60);

            // Load basic resources included in engine.
            // Some resources are required for renderer to function.
            InitializeEngineResources();
            PostResourceLoadInitialization();

            input = new Input();
            gameTime = new GameTime();

            accessors = new Accessors(
                gameTime.Accessor, 
                input.Acessor
            );

            stateMachine = new States.GameStateMachine(accessors);
            States.GameState testState = new States._TestState(accessors);
            stateMachine.Attach(testState);
            stateMachine.AddToStack("TestState");

            testState.Load(resourceManager, deviceResources.Device);
            testState.Init(resourceManager);

            light = new GameObject("light", 0);
            light.SetScale(new Vector3(0.1f, 0.1f, 0.1f));
            light.SetPosition(new Vector3(-5, 0, -5));

            
            // TODO: Work this out.
            // Renderer must be created after state has loaded resources.
            // Need to make CubemapRenderer play nice with Renderer.
            renderer = new Rendering.Renderer(window, deviceResources, render_width, render_height, 60, effectHandler);


            Logger.Write(LogType.Info, "Engine Initialized");
        }

        private void InitializeEngineResources()
        {
            var device = deviceResources.Device;

            List<string> renderBufferCopyDefines = new List<string>();
            if (settings.Force24BitColor)
                renderBufferCopyDefines.Add("_24_BIT_COLOR_");

            Logger.StartLogSection("Loading Engine Resources");
            resourceManager.Meshes.Load(new Resources.Types.MeshResourceDesc("KDX_CUBE.fbx", "KDX_CUBE"));
            resourceManager.Meshes.Load(new Resources.Types.MeshResourceDesc("KDX_CUBE_IN.fbx", "KDX_CUBE_IN"));

            resourceManager.Shaders.Load(new Resources.Types.ShaderResourceDesc("KDX_SKYBOX_ENV_RENDERER.hlsl", "KDX_SKYBOX_ENV_RENDERER", device));
            resourceManager.Shaders.Load(new Resources.Types.ShaderResourceDesc("KDX_SKYBOX_IRR_RENDERER.hlsl", "KDX_SKYBOX_IRR_RENDERER", device));


            resourceManager.Shaders.Load(
                new Resources.Types.ShaderResourceDesc(
                    "KDX_EFFECTS/KDX_COPY_TO_BACK.hlsl", 
                    "KDX_COPY_TO_BACK", 
                    device, 
                    renderBufferCopyDefines
                )
            );            
            
            // TODO: Remove from here
            // Only post effect required by engine is KDX_COPY_TO_BACK
            // therefore only that one should be loaded here.
            resourceManager.Shaders.Load(new Resources.Types.ShaderResourceDesc("KDX_EFFECTS/KDX_GRAYSCALE.hlsl", "KDX_GRAYSCALE", device));
            resourceManager.Shaders.Load(new Resources.Types.ShaderResourceDesc("KDX_EFFECTS/KDX_BLACK_AND_WHITE.hlsl", "KDX_BLACK_AND_WHITE", device));
            resourceManager.Shaders.Load(new Resources.Types.ShaderResourceDesc("KDX_EFFECTS/KDX_DEPTH.hlsl", "KDX_DEPTH", device));
            Logger.EndLogSection();
        }

        private void PostResourceLoadInitialization()
        {
            Logger.StartLogSection("Post Resource Load Init");
            Logger.StartLogSection("Attaching Post Effects");
            effectHandler.AttachEffect(new Rendering.PostProcessing.PostEffect(resourceManager.Shaders.GetFromAlias("KDX_COPY_TO_BACK").Shader, int.MaxValue));
            effectHandler.AttachEffect(new Rendering.PostProcessing.PostEffect(resourceManager.Shaders.GetFromAlias("KDX_BLACK_AND_WHITE").Shader, 0));
            effectHandler.AttachEffect(new Rendering.PostProcessing.PostEffect(resourceManager.Shaders.GetFromAlias("KDX_GRAYSCALE").Shader, 0));
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
            stateMachine.Update();



            List<Scene> scenes = stateMachine.GetScenes();
            renderer.StageScenes(scenes);


            // TODO: Light should not be here.
            renderer.perFrameBuffer.lightDir = light.GetPosition();
            renderer.perFrameBuffer.lightDir.Normalize();

            if (input.Acessor.GetKeyPressed(SharpDX.DirectInput.Key.K))
                effectHandler.EnableEffect("KDX_GRAYSCALE");
            if (input.Acessor.GetKeyPressed(SharpDX.DirectInput.Key.L))
                effectHandler.DisableEffect("KDX_GRAYSCALE");
        }

		public void Dispose()
		{
			renderer.Dispose();
			window.Dispose();
			resourceManager.Dispose();
            deviceResources.Dispose();
		}


    }
}
