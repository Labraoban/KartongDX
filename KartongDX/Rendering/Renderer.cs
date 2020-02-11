using System;
using System.Collections.Generic;
using System.Linq;

using SharpDX;
using SharpDX.Windows;
using SharpDX.DXGI;

using D3D11 = SharpDX.Direct3D11;
using SharpDX.Direct3D;
using SharpDX.D3DCompiler;

using KartongDX.Rendering.ResourceViews;
using KartongDX.Engine;

namespace KartongDX.Rendering
{
    class Renderer : IDisposable
    {
        public const int SKYBOX_ENV_SLOT = 32;
        public const int SKYBOX_IRR_SLOT = 33;
        public const int SKYBOX_RAD_SLOT = 34;
        public const int SKYBOX_BRDF_SLOT = 35;

        public RenderController RenderController { get; private set; }

        public int RenderRes_X { get; private set; }
        public int RenderRes_Y { get; private set; }
        public int TargetFPS { get; private set; }

        public D3D11.Device Device { get; private set; }

        private Window window;

        private SwapChain swapChain;

        private D3D11.DeviceContext deviceContext;
        private D3D11.SamplerState sampler;

        private D3D11.Buffer perObjectConstantShaderBuffer;
        private D3D11.Buffer perFrameConstantShaderBuffer;


        private PerObjectBuffer perObjectBuffer;
        public PerFrameBuffer perFrameBuffer;

        private Camera camera;

        private Action gameLoopCallback;

        private SampleDescription sampleDescription = new SampleDescription(1, 0);

        private RenderMode renderMode;

        private List<Scene> scenes;

        private D3D11.Texture2D backBuffer;
        private D3D11.RenderTargetView backBufferRTV;

        PostProcessing.PostEffectHandler effectHandler;

        Shader copyToBackShader; // TODO temp

        public Renderer(Window window, int renderRes_X, int renderRes_Y, int targetFPS, PostProcessing.PostEffectHandler effectHandler)
        {
            this.window = window;
            this.RenderRes_X = renderRes_X;
            this.RenderRes_Y = renderRes_Y;
            this.TargetFPS = targetFPS;
            this.effectHandler = effectHandler;

            Device = new D3D11.Device(DriverType.Hardware, D3D11.DeviceCreationFlags.Debug | D3D11.DeviceCreationFlags.DisableGpuTimeout);
            deviceContext = Device.ImmediateContext;
            renderMode = new RenderMode("Game");
            renderMode.UseSwapChain = true;


            var backBufferDesc = new ModeDescription(RenderRes_X, RenderRes_Y, new Rational(TargetFPS, 1), Format.R8G8B8A8_UNorm);
            var swapChainDesc = new SwapChainDescription()
            {
                ModeDescription = backBufferDesc,
                SampleDescription = sampleDescription,
                Usage = Usage.RenderTargetOutput,
                BufferCount = 1,
                OutputHandle = window.GetHandle(),
                IsWindowed = true
            };

            // Create SwapChain
            Factory factory = new Factory1();
            swapChain = new SwapChain(factory, Device, swapChainDesc);

            // Create BackBuffer RTV
            backBuffer = swapChain.GetBackBuffer<D3D11.Texture2D>(0);
            backBufferRTV = new D3D11.RenderTargetView(Device, backBuffer);

            SetupRenderMode(renderMode);
            InitRenderMode(renderMode);
        }

        public void StartRenderLoop()
        {
            RenderLoop.Run(window.GetForm(), () =>
            {
                gameLoopCallback();
                DrawScenes();
                PostProcess();

                if (renderMode.UseSwapChain)
                {
                    swapChain.Present(1, PresentFlags.None);
                }
            });
        }

        public void Init()
        {
            EnableRenderMode(renderMode);

            // TODO temp
            copyToBackShader = Resources.ResourceManager.instance.Shaders.GetFromAlias("KDX_COPY_TO_BACK").Shader;
        }

        public void PostProcess()
        {
            int numberOfEffects = effectHandler.NumberOfEffects;
            if(numberOfEffects < 1)
            {
                // TODO: Fallback
                Logger.Write(LogType.Error, "No post effects, cant copy to backbuffer");
            }

            RenderViews r0 = renderMode.RenderViews;
            RenderViews r1 = renderMode.RenderViewsSwap;

            // Backbuffer does not use depth
            RenderViews b0 = new RenderViews(backBufferRTV, null); 

            RenderViews[] views =
            {
                r0,
                r1
            };

            int targetIndex = 0;

            RenderViews target;
            RenderViews source;

            for (int i = 0; i < effectHandler.NumberOfEffects; ++i)
            {
                // Only One effect Left, use Backbuffer
                if (i == effectHandler.NumberOfEffects - 1)
                {
                    source = views[targetIndex];
                    target = b0; // set target to backbuffer
                }
                else
                {
                    source = views[targetIndex];
                    targetIndex = (targetIndex + 1) % 2;
                    target = views[targetIndex];
                }

                ApplyPostEffect(source, target, effectHandler.GetEffectFromStack(i));
            }
        }

        public void ApplyPostEffect(RenderViews source, RenderViews target, PostProcessing.PostEffect effect)
        {
            deviceContext.OutputMerger.SetRenderTargets(null, target.RTV);


            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
            deviceContext.InputAssembler.InputLayout = effect.Shader.InputLayout;

            deviceContext.VertexShader.Set(effect.Shader.VertexShader);
            deviceContext.PixelShader.Set(effect.Shader.PixelShader);

            deviceContext.PixelShader.SetShaderResource(0, source.RTV_SRVT);
            deviceContext.PixelShader.SetShaderResource(1, source.DSV_SRVT);

            deviceContext.PixelShader.SetSampler(0, sampler);

            deviceContext.Draw(4, 0);

            // Reset
            deviceContext.OutputMerger.SetRenderTargets(renderMode.RenderViews.DSV, renderMode.RenderViews.RTV);
        }


        public void SetupRenderMode(RenderMode renderMode)
        {
            Logger.Write(LogType.Info, "Setup RenderMode {0}", renderMode.Name);

            renderMode.RenderTargetDesc = new D3D11.Texture2DDescription()
            {
                Format = /* Format.R8G8B8A8_UNorm */ Format.R16G16B16A16_Float,
                ArraySize = 1,
                MipLevels = 1,
                Width = RenderRes_X,
                Height = RenderRes_Y,
                SampleDescription = sampleDescription,
                Usage = D3D11.ResourceUsage.Default,
                BindFlags = D3D11.BindFlags.RenderTarget | D3D11.BindFlags.ShaderResource,
                CpuAccessFlags = D3D11.CpuAccessFlags.None,
                OptionFlags = D3D11.ResourceOptionFlags.None
            };

            renderMode.DepthBufferDesc = new D3D11.Texture2DDescription()
            {
                Format = Format.R24G8_Typeless,
                ArraySize = 1,
                MipLevels = 1,
                Width = RenderRes_X,
                Height = RenderRes_Y,
                SampleDescription = sampleDescription,
                Usage = D3D11.ResourceUsage.Default,
                BindFlags = D3D11.BindFlags.DepthStencil | D3D11.BindFlags.ShaderResource,
                CpuAccessFlags = D3D11.CpuAccessFlags.None,
                OptionFlags = D3D11.ResourceOptionFlags.None
            };

            renderMode.DepthStencilStateDesc = new D3D11.DepthStencilStateDescription
            {
                IsDepthEnabled = true,
                DepthComparison = D3D11.Comparison.LessEqual,
                DepthWriteMask = D3D11.DepthWriteMask.All
            };
        }

        public void InitRenderMode(RenderMode renderMode)
        {
            Logger.Write(LogType.Info, "Init RenderMode {0}", renderMode.Name);

            // Create RenderTarget Textures
            renderMode.RenderTarget = new D3D11.Texture2D(Device, renderMode.RenderTargetDesc);
            renderMode.DepthBuffer = new D3D11.Texture2D(Device, renderMode.DepthBufferDesc);

            // Create Swap RenderTargetTextures for PostPorcessing
            renderMode.RenderTargetSwap = new D3D11.Texture2D(Device, renderMode.RenderTargetDesc);
            renderMode.DepthBufferSwap = new D3D11.Texture2D(Device, renderMode.DepthBufferDesc);

            D3D11.DepthStencilViewDescription dsvDesc = new D3D11.DepthStencilViewDescription();
            dsvDesc.Format = Format.D24_UNorm_S8_UInt;
            dsvDesc.Dimension = D3D11.DepthStencilViewDimension.Texture2D;
            dsvDesc.Texture2D.MipSlice = 0;
            dsvDesc.Flags = 0;

            D3D11.RenderTargetViewDescription rtvDesc = new D3D11.RenderTargetViewDescription();
            rtvDesc.Format = Format.R16G16B16A16_Float;
            rtvDesc.Dimension = D3D11.RenderTargetViewDimension.Texture2D;
            rtvDesc.Texture2D.MipSlice = 0;

            D3D11.ShaderResourceViewDescription dsvStvtDesc = new D3D11.ShaderResourceViewDescription();
            dsvStvtDesc.Texture2D.MipLevels = 1;
            dsvStvtDesc.Dimension = ShaderResourceViewDimension.Texture2D;
            dsvStvtDesc.Format = Format.R24_UNorm_X8_Typeless;

            D3D11.ShaderResourceViewDescription rtvSrtvDesc = new D3D11.ShaderResourceViewDescription();
            rtvSrtvDesc.Texture2D.MipLevels = 1;
            rtvSrtvDesc.Dimension = ShaderResourceViewDimension.Texture2D;
            rtvSrtvDesc.Format = Format.R16G16B16A16_Float;

            //Create Views
            renderMode.RenderViews = new RenderViews(
                new D3D11.RenderTargetView(Device, renderMode.RenderTarget),
                new D3D11.DepthStencilView(Device, renderMode.DepthBuffer, dsvDesc),
                new D3D11.ShaderResourceView(Device, renderMode.RenderTarget),
                new D3D11.ShaderResourceView(Device, renderMode.DepthBuffer, dsvStvtDesc)
            );

            //Create Views for Swap
            renderMode.RenderViewsSwap = new RenderViews(
                new D3D11.RenderTargetView(Device, renderMode.RenderTargetSwap, rtvDesc),
                new D3D11.DepthStencilView(Device, renderMode.DepthBufferSwap, dsvDesc),
                new D3D11.ShaderResourceView(Device, renderMode.RenderTargetSwap, rtvSrtvDesc),
                new D3D11.ShaderResourceView(Device, renderMode.DepthBufferSwap, dsvStvtDesc)
            );


            // Create DepthStencilState
            renderMode.DepthStencilState = new D3D11.DepthStencilState(Device, renderMode.DepthStencilStateDesc);

            // Create Sampler
            sampler = CreateSampler();

            //deviceContext.OutputMerger.SetRenderTargets(depthStencilView, renderTargetView);
            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            perObjectConstantShaderBuffer = new SharpDX.Direct3D11.Buffer(Device, SharpDX.Utilities.SizeOf<PerObjectBuffer>(),
                D3D11.ResourceUsage.Default,
                D3D11.BindFlags.ConstantBuffer,
                D3D11.CpuAccessFlags.None,
                D3D11.ResourceOptionFlags.None, 0);

            perFrameConstantShaderBuffer = new SharpDX.Direct3D11.Buffer(Device, SharpDX.Utilities.SizeOf<PerFrameBuffer>(),
                D3D11.ResourceUsage.Default,
                D3D11.BindFlags.ConstantBuffer,
                D3D11.CpuAccessFlags.None,
                D3D11.ResourceOptionFlags.None, 0);

            perObjectBuffer = new PerObjectBuffer();
            perFrameBuffer = new PerFrameBuffer();

            Logger.Write(LogType.Info, "Renderer Created");
        }

        public void EnableRenderMode(RenderMode renderMode)
        {
            Logger.Write(LogType.Info, "Enable RenderMode {0}", renderMode.Name);

            deviceContext.OutputMerger.SetDepthStencilState(renderMode.DepthStencilState);
            deviceContext.OutputMerger.SetRenderTargets(renderMode.RenderViews.DSV, renderMode.RenderViews.RTV);

            if (camera != null)
                deviceContext.Rasterizer.SetViewport(camera.Viewport);
            deviceContext.VertexShader.SetSampler(0, sampler);
        }

        /* Sets shader for rendering of current object
         */
        public void SetShader(Shader shader)
        {
            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            deviceContext.InputAssembler.InputLayout = shader.InputLayout;

            deviceContext.VertexShader.Set(shader.VertexShader);
            deviceContext.VertexShader.SetConstantBuffer(0, perObjectConstantShaderBuffer);
            deviceContext.VertexShader.SetConstantBuffer(1, perFrameConstantShaderBuffer);

            deviceContext.PixelShader.SetConstantBuffer(0, perObjectConstantShaderBuffer);
            deviceContext.PixelShader.SetConstantBuffer(1, perFrameConstantShaderBuffer);

            deviceContext.PixelShader.Set(shader.PixelShader);
        }

        /* Passes ShaderResourceViews to the PixelShader
         */
        public void SetMaterialSRVT(Material material)
        {
            foreach (var slot in material.Slots)
            {
                deviceContext.PixelShader.SetShaderResource(slot, material.GetTexture(slot).SRVT);
            }
        }

        public void SetSkyboxSRVT(Skybox skybox)
        {
            deviceContext.PixelShader.SetShaderResource(SKYBOX_ENV_SLOT, skybox.Cubemap.Enviroment.SRVT);
            deviceContext.PixelShader.SetShaderResource(SKYBOX_IRR_SLOT, skybox.Cubemap.Irradiance.SRVT);
        }

        public void SetCamera(Camera camera)
        {
            this.camera = camera;
            deviceContext.Rasterizer.SetViewport(camera.Viewport);
        }

        private void DrawScenes()
        {
            //if (renderMode.UseDepthStencil)
            ClearDepth(renderMode.RenderViews.DSV);
            ClearRTV(renderMode.RenderViews.RTV);

            camera.ResolveDirty();
            UpdatePerFrameBuffer();

            Skybox skybox = scenes[0].Skybox;

            SetSkyboxSRVT(skybox);

            foreach (Scene scene in scenes)
            {
                DrawScene(scene, camera);
            }

            // Render Skybox last
            UpdatePerObjectBuffer(Matrix.Identity, camera);
            Render(skybox.model);
        }

        private void DrawScene(Scene scene, Camera cam)
        {
            RenderQueue queue = scene.GetRenderQueue();

            foreach (RenderItem renderItem in queue.GetList())
            {
                UpdatePerObjectBuffer(renderItem.WorldTransform, cam);
                Render(renderItem.Model);
            }
        }

        public void StageScenes(List<Scene> scenes)
        {
            this.scenes = scenes;
        }


        private void Render(Model model)
        {
            if (!model.Mesh.HasBuffers)
            {
                model.Mesh.CreateBuffers(Device);
            }

            D3D11.Buffer vertexBuffer = model.Mesh.VertexBuffer;
            D3D11.Buffer indexBuffer = model.Mesh.IndexBuffer;

            SetShader(model.Material.Shader);
            SetMaterialSRVT(model.Material);

            deviceContext.InputAssembler.SetVertexBuffers(0, new D3D11.VertexBufferBinding(vertexBuffer, Utilities.SizeOf<Vertex>(), 0));
            deviceContext.InputAssembler.SetIndexBuffer(indexBuffer, Format.R32_UInt, 0);

            deviceContext.DrawIndexed(model.Mesh.Triangles.Count() * 3, 0, 0);
        }

        private void UpdatePerFrameBuffer()
        {

            //shaderLightConstant.lightDir = new Vector3(1, 0, 0);
            //shaderLightConstant.lightDir.Normalize(); // TODO This is temp recplaced by public ShaderLightConstand

            perFrameBuffer.viewPosition = camera.GetPosition();
            perFrameBuffer.viewDir = camera.WorldMatrix.Forward;
            perFrameBuffer.zFar = camera.ZFar;

            deviceContext.UpdateSubresource(ref perFrameBuffer, perFrameConstantShaderBuffer);
        }

        private void UpdatePerObjectBuffer(Matrix modelMatrix, Camera cam)
        {
            // Transpose to get Column Major matrixes for Shader
            Matrix world = Matrix.Transpose(modelMatrix);
            Matrix view = Matrix.Transpose(cam.View);
            Matrix proj = Matrix.Transpose(cam.Proj);

            Matrix mvp = proj * view * world;

            perObjectBuffer = new PerObjectBuffer();
            perObjectBuffer.world = world;
            perObjectBuffer.view = view;
            perObjectBuffer.proj = proj;
            perObjectBuffer.worldViewProj = mvp;
            perObjectBuffer.normalMatrix = Matrix.Invert(Matrix.Transpose(world));
            perObjectBuffer.invProj = Matrix.Invert(proj);

            deviceContext.UpdateSubresource(ref perObjectBuffer, perObjectConstantShaderBuffer);
        }

        public void Dispose()
        {
            renderMode.Dispose();
            Device.Dispose();
        }

        public void SetEngineLoopCallback(Action gameLoopCallback)
        {
            this.gameLoopCallback = gameLoopCallback;
        }

        private D3D11.SamplerState CreateSampler()
        {
            return new D3D11.SamplerState(Device, new D3D11.SamplerStateDescription()
            {
                Filter = D3D11.Filter.MinMagMipLinear,
                AddressU = D3D11.TextureAddressMode.Wrap,
                AddressV = D3D11.TextureAddressMode.Wrap,
                AddressW = D3D11.TextureAddressMode.Clamp,
                BorderColor = SharpDX.Color.Magenta,
                ComparisonFunction = D3D11.Comparison.Never,
                MaximumAnisotropy = 16,
                MipLodBias = 0,
                MinimumLod = 0,
                MaximumLod = 16
            });
        }

        private void ClearDepth(D3D11.DepthStencilView depthStencilView)
        {
            deviceContext.ClearDepthStencilView(depthStencilView, D3D11.DepthStencilClearFlags.Depth, 1.0f, 0);
        }

        private void ClearRTV(D3D11.RenderTargetView renderTargetView)
        {
            deviceContext.ClearRenderTargetView(renderTargetView, new SharpDX.Color(32, 103, 178));
        }

    }
}
