using System;
using System.Collections.Generic;
using System.Linq;

using SharpDX;
using SharpDX.Windows;
using SharpDX.DXGI;

using D3D11 = SharpDX.Direct3D11;
using SharpDX.Direct3D;

using KartongDX.Engine;

namespace KartongDX.Rendering
{
    class Renderer : IDisposable
    {
        public const int SKYBOX_ENV_SLOT = 32;
        public const int SKYBOX_IRR_SLOT = 33;
        public const int SKYBOX_RAD_SLOT = 34;
        public const int SKYBOX_BRDF_SLOT = 35;
        
        public int RenderRes_X { get; private set; }
        public int RenderRes_Y { get; private set; }
        public int TargetFPS { get; private set; }

        private DeviceResources deviceResources;
        private Window window;



        private D3D11.SamplerState sampler;

        private D3D11.Buffer perObjectConstantShaderBuffer;
        private D3D11.Buffer perFrameConstantShaderBuffer;


        private PerObjectBuffer perObjectBuffer;
        public PerFrameBuffer perFrameBuffer;

        //private Camera camera;

        private Action gameLoopCallback;

        private SampleDescription sampleDescription = new SampleDescription(1, 0);

        //private RenderMode renderMode;

        private List<Scene> scenes;

        PostProcessing.PostEffectHandler effectHandler;
        RenderTargetHandler renderTargetHandler;

        public Renderer(Window window, DeviceResources deviceResources, int renderRes_X, int renderRes_Y, int targetFPS, PostProcessing.PostEffectHandler effectHandler)
        {
            this.window = window;
            this.deviceResources = deviceResources;
            this.RenderRes_X = renderRes_X;
            this.RenderRes_Y = renderRes_Y;
            this.TargetFPS = targetFPS;
            this.effectHandler = effectHandler;

            renderTargetHandler = new RenderTargetHandler();

            InitRenderTargets();
            SetupConstantBuffers();

            Logger.Write(LogType.Info, "Renderer Created!");
        }

        public void StartRenderLoop()
        {
            RenderLoop.Run(window.GetForm(), () =>
            {
                gameLoopCallback();
                DrawScenes();
                PostProcess();

                deviceResources.SwapChain.Present(1, PresentFlags.None);
            });
        }

        /* Clears RenderTarget and DepthStencil then renders all staged scenes.
         * Finally, render skybox last.
         */
        private void DrawScenes()
        {
            ClearDepth(renderTargetHandler.GetDepthStencilView());
            ClearRTV(renderTargetHandler.GetRenderTargetView());

            //camera.ResolveDirty();
            //UpdatePerFrameBuffer();

            // Always use first skybox if several scenes contrain skyboxes;
            // Skybox is drawn with camera from its scene.
            Skybox skybox = scenes[0].Skybox;
            Camera skyCam = scenes[0].Camera;
            SetSkyboxSRVT(skybox);

            foreach (Scene scene in scenes)
            {
                DrawScene(scene);
            }

            // Render Skybox last
            UpdatePerObjectBuffer(Matrix.Identity, skyCam);
            Render(skybox.model);
        }

        /* Calls the Render method for each object in RenderQueue
         */
        private void DrawScene(Scene scene)
        {
            Camera camera = scene.Camera;
            RenderQueue queue = scene.GetRenderQueue();


            //camera.ResolveDirty();
            UpdatePerFrameBuffer(camera); // Should be named PerSceneBuffer now.
            deviceResources.DeviceContext.Rasterizer.SetViewport(camera.Viewport);

            foreach (RenderItem renderItem in queue.GetList())
            {
                UpdatePerObjectBuffer(renderItem.WorldTransform, camera);

                Render(renderItem.Model);
            }
        }

        private void Render(Model model)
        {
            D3D11.DeviceContext deviceContext = deviceResources.DeviceContext;

            if (!model.Mesh.HasBuffers)
            {
                // TODO: Should not initialize one buffer per object
                //       Investigate way to "pool" meshes in.
                //       Also, should not be done here.
                model.Mesh.CreateBuffers(deviceResources.Device);
            }

            D3D11.Buffer vertexBuffer = model.Mesh.VertexBuffer;
            D3D11.Buffer indexBuffer = model.Mesh.IndexBuffer;

            SetShader(model.Material.Shader);
            SetMaterialSRVT(model.Material);

            deviceContext.InputAssembler.SetVertexBuffers(0, new D3D11.VertexBufferBinding(vertexBuffer, Utilities.SizeOf<Vertex>(), 0));
            deviceContext.InputAssembler.SetIndexBuffer(indexBuffer, Format.R32_UInt, 0);

            deviceContext.DrawIndexed(model.Mesh.Triangles.Count() * 3, 0, 0);
        }

        public void PostProcess()
        {
            int numberOfEffects = effectHandler.NumberOfEffects;
            if(numberOfEffects < 1)
            {
                // TODO: Implement some fallback
                Logger.Write(LogType.Error, "No post effects, Can not copy to backbuffer");
            }

            for (int i = 0; i < effectHandler.NumberOfEffects; ++i)
            {
                D3D11.ShaderResourceView sourceRenderTarget = renderTargetHandler.GetRenderTargetResourceView();
                D3D11.ShaderResourceView sourceDepthStencil = renderTargetHandler.GetDepthStencilResourceView();
                D3D11.RenderTargetView destRenderTarget;
                
                // Only One effect Left, use Backbuffer
                if (i == effectHandler.NumberOfEffects - 1)
                {
                    destRenderTarget = deviceResources.BackBufferRTV;
                }
                else
                {
                    destRenderTarget = renderTargetHandler.GetRenderTargetView();
                }

                ApplyPostEffect(sourceRenderTarget, sourceDepthStencil, destRenderTarget, effectHandler.GetEffectFromStack(i));
                renderTargetHandler.Swap();
            }
            renderTargetHandler.ResetToDefault();
        }

        public void ApplyPostEffect(
            D3D11.ShaderResourceView sourceRenderTarget,
            D3D11.ShaderResourceView sourceDepthStencil,
            D3D11.RenderTargetView destRenderTarget, 
            PostProcessing.PostEffect effect)
        {
            D3D11.DeviceContext deviceContext = deviceResources.DeviceContext;
            // No rendering directly to DepthStencil
            deviceContext.OutputMerger.SetRenderTargets(null, destRenderTarget);

            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleStrip;
            deviceContext.InputAssembler.InputLayout = effect.Shader.InputLayout;

            deviceContext.VertexShader.Set(effect.Shader.VertexShader);
            deviceContext.PixelShader.Set(effect.Shader.PixelShader);

            deviceContext.PixelShader.SetShaderResource(0, sourceRenderTarget);
            deviceContext.PixelShader.SetShaderResource(1, sourceDepthStencil);

            deviceContext.PixelShader.SetSampler(0, sampler);

            deviceContext.Draw(4, 0);

            // Reset
            deviceContext.OutputMerger.SetRenderTargets(
                renderTargetHandler.GetDepthStencilView(), 
                renderTargetHandler.GetRenderTargetView()
            );
        }

        public void InitRenderTargets()
        {
            D3D11.DeviceContext deviceContext = deviceResources.DeviceContext;

            var renderTargetDesc = new D3D11.Texture2DDescription()
            {
                Format = Format.R16G16B16A16_Float,
                ArraySize = 1,
                MipLevels = 1,
                Width = deviceResources.BackBuffer.Description.Width,
                Height = deviceResources.BackBuffer.Description.Height,
                SampleDescription = sampleDescription,
                Usage = D3D11.ResourceUsage.Default,
                BindFlags = D3D11.BindFlags.RenderTarget | D3D11.BindFlags.ShaderResource,
                CpuAccessFlags = D3D11.CpuAccessFlags.None,
                OptionFlags = D3D11.ResourceOptionFlags.None
            };

            var depthStencilDesc = new D3D11.Texture2DDescription()
            {
                Format = Format.R24G8_Typeless,
                ArraySize = 1,
                MipLevels = 1,
                Width = deviceResources.BackBuffer.Description.Width,
                Height = deviceResources.BackBuffer.Description.Height,
                SampleDescription = sampleDescription,
                Usage = D3D11.ResourceUsage.Default,
                BindFlags = D3D11.BindFlags.DepthStencil | D3D11.BindFlags.ShaderResource,
                CpuAccessFlags = D3D11.CpuAccessFlags.None,
                OptionFlags = D3D11.ResourceOptionFlags.None
            };

            var depthStencilStateDesc = new D3D11.DepthStencilStateDescription
            {
                IsDepthEnabled = true,
                DepthComparison = D3D11.Comparison.LessEqual,
                DepthWriteMask = D3D11.DepthWriteMask.All,
            };


            // Create Descripptions for DepthStencil
            // RenderTarget works without Desc
            D3D11.DepthStencilViewDescription dsvDesc = new D3D11.DepthStencilViewDescription();
            dsvDesc.Format = Format.D24_UNorm_S8_UInt;
            dsvDesc.Dimension = D3D11.DepthStencilViewDimension.Texture2D;
            dsvDesc.Texture2D.MipSlice = 0;
            dsvDesc.Flags = 0;

            D3D11.ShaderResourceViewDescription dsSrvtDesc = new D3D11.ShaderResourceViewDescription();
            dsSrvtDesc.Texture2D.MipLevels = 1;
            dsSrvtDesc.Dimension = ShaderResourceViewDimension.Texture2D;
            dsSrvtDesc.Format = Format.R24_UNorm_X8_Typeless; // TODO: Check Format


            // Create RenderTarget Textures
            renderTargetHandler.CreateRenderTarget(deviceResources.Device, renderTargetDesc);
            renderTargetHandler.CreateDepthStencil(deviceResources.Device, depthStencilDesc, dsvDesc, dsSrvtDesc);


            deviceContext.OutputMerger.SetRenderTargets(
                renderTargetHandler.GetDepthStencilView(),
                renderTargetHandler.GetRenderTargetView()
            );



            // Create DepthStencilState
            var depthStencilState = new D3D11.DepthStencilState(deviceResources.Device, depthStencilStateDesc);
            deviceContext.OutputMerger.SetDepthStencilState(depthStencilState);

            // Create Sampler
            sampler = CreateSampler();
            deviceContext.VertexShader.SetSampler(0, sampler);
            deviceContext.PixelShader.SetSampler(0, sampler);

            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;

            Logger.Write(LogType.Info, "Renderer Created");
        }

        private void SetupConstantBuffers()
        {
            perObjectConstantShaderBuffer = new D3D11.Buffer(deviceResources.Device, SharpDX.Utilities.SizeOf<PerObjectBuffer>(),
                D3D11.ResourceUsage.Default,
                D3D11.BindFlags.ConstantBuffer,
                D3D11.CpuAccessFlags.None,
                D3D11.ResourceOptionFlags.None, 0);

            perFrameConstantShaderBuffer = new D3D11.Buffer(deviceResources.Device, SharpDX.Utilities.SizeOf<PerFrameBuffer>(),
                D3D11.ResourceUsage.Default,
                D3D11.BindFlags.ConstantBuffer,
                D3D11.CpuAccessFlags.None,
                D3D11.ResourceOptionFlags.None, 0);

            perObjectBuffer = new PerObjectBuffer();
            perFrameBuffer = new PerFrameBuffer();
        }

        /* Sets shader for rendering of current object
         */
        public void SetShader(Shader shader)
        {
            D3D11.DeviceContext deviceContext = deviceResources.DeviceContext;

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
                deviceResources.DeviceContext.PixelShader.SetShaderResource(slot, material.GetTexture(slot).SRVT);
            }
        }

        public void SetSkyboxSRVT(Skybox skybox)
        {
            D3D11.DeviceContext deviceContext = deviceResources.DeviceContext;

            deviceContext.PixelShader.SetShaderResource(SKYBOX_ENV_SLOT, skybox.Cubemap.Enviroment.SRVT);
            deviceContext.PixelShader.SetShaderResource(SKYBOX_IRR_SLOT, skybox.Cubemap.Irradiance.SRVT);
        }

        private void UpdatePerFrameBuffer(Camera camera)
        {
            // TODO: Scene lights should be passed here and added to constant buffer 

            perFrameBuffer.viewPosition = camera.GetWorldPosition();
            perFrameBuffer.viewDir = camera.WorldMatrix.Forward;
            perFrameBuffer.zFar = camera.ZFar;

            deviceResources.DeviceContext.UpdateSubresource(ref perFrameBuffer, perFrameConstantShaderBuffer);
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

            deviceResources.DeviceContext.UpdateSubresource(ref perObjectBuffer, perObjectConstantShaderBuffer);
        }

        public void StageScenes(List<Scene> scenes)
        {
            this.scenes = scenes;
        }

        public void SetEngineLoopCallback(Action gameLoopCallback)
        {
            this.gameLoopCallback = gameLoopCallback;
        }

        private D3D11.SamplerState CreateSampler()
        {
            return new D3D11.SamplerState(deviceResources.Device, new D3D11.SamplerStateDescription()
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
            deviceResources.DeviceContext.ClearDepthStencilView(depthStencilView, D3D11.DepthStencilClearFlags.Depth, 1.0f, 0);
            //deviceResources.DeviceContext.ClearDepthStencilView(depthStencilView, D3D11.DepthStencilClearFlags.Stencil, 1.0f, 0);
        }

        private void ClearRTV(D3D11.RenderTargetView renderTargetView)
        {
            deviceResources.DeviceContext.ClearRenderTargetView(renderTargetView, new SharpDX.Color(32, 103, 178));
        }

        public void Dispose()
        {
            renderTargetHandler.Dispose();
        }
    }
}
