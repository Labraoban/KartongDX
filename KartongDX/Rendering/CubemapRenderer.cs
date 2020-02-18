using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Windows;
using SharpDX.DXGI;
using System.Runtime.InteropServices;

using D3D11 = SharpDX.Direct3D11;
using SharpDX.Direct3D;
using SharpDX.D3DCompiler;

namespace KartongDX.Rendering
{
    [StructLayoutAttribute(LayoutKind.Sequential)]
    struct KDX_SKYBOX_ENV_RENDERER_BUFFER
    {
        public Matrix World;
        public Matrix View;
        public Matrix Proj;
        public Matrix MVP;
    };

    class Cubemap
    {
        public Texture Enviroment { get; set; }
        public Texture Irradiance { get; set; }
        public Texture Radiance { get; set; }
        public Texture BRDF { get; set; }
    }

    class CubemapRenderer
    {
        D3D11.Device device;

        private Mesh cubeMesh;
        private D3D11.Buffer perFaceBuffer;
        private Format format = Format.B8G8R8A8_UNorm;

        Shader shaderEnv;
        Shader shaderIrr;

        Matrix[] views = new Matrix[] {
                Matrix.LookAtLH(new Vector3(0.0f, 0.0f, 0.0f), Vector3.Right, Vector3.Down),
                Matrix.LookAtLH(new Vector3(0.0f, 0.0f, 0.0f), Vector3.Left, Vector3.Down),
                Matrix.LookAtLH(new Vector3(0.0f, 0.0f, 0.0f), Vector3.Down, Vector3.ForwardLH),
                Matrix.LookAtLH(new Vector3(0.0f, 0.0f, 0.0f), Vector3.Up, Vector3.BackwardLH),
                Matrix.LookAtLH(new Vector3(0.0f, 0.0f, 0.0f), Vector3.BackwardLH, Vector3.Down),
                Matrix.LookAtLH(new Vector3(0.0f, 0.0f, 0.0f), Vector3.ForwardLH, Vector3.Down)
        };

        public CubemapRenderer(D3D11.Device device, Resources.ResourceManager resourceManager)
        {
            this.device = device;

            shaderEnv = resourceManager.Shaders.GetFromAlias("KDX_SKYBOX_ENV_RENDERER").Shader;
            shaderIrr = resourceManager.Shaders.GetFromAlias("KDX_SKYBOX_IRR_RENDERER").Shader;


            cubeMesh = resourceManager.Meshes.GetFromAlias("KDX_CUBE_IN").Mesh;

            if (!cubeMesh.HasBuffers)
                cubeMesh.CreateBuffers(device);
        }

        public Cubemap GenerateFromEquirectangular(D3D11.Texture2D equirectangular)
        {
            Logger.Write(LogType.Info, "Generating Cubemaps");

            Cubemap cubemap = new Cubemap();

            var enviroment = GenerateCubemap(device, shaderEnv, 1024, equirectangular, 1, 0);
            var irradiance = GenerateCubemap(device, shaderIrr, 128, equirectangular, 1, 0);

            var enviromentSRTV = CreateShaderResourceView(device, format, enviroment);
            var irradianceSRVT = CreateShaderResourceView(device, format, irradiance);

            cubemap.Enviroment = new Texture(enviroment, enviromentSRTV);
            cubemap.Irradiance = new Texture(enviroment, irradianceSRVT);

            return cubemap;
        }

        public D3D11.Texture2D GenerateCubemap(D3D11.Device device, Shader shader, int size, D3D11.Texture2D texture, int mipLevels, int mip)
        {
            Format format = Format.B8G8R8A8_UNorm;

            D3D11.Texture2DDescription renderTextureDesc = GetTexture2DDescription(size, format, mipLevels);

            D3D11.SamplerState sampler = GetSamplerState(device);

            D3D11.Texture2D RenderTexture = new D3D11.Texture2D(device, renderTextureDesc);

            D3D11.RenderTargetView[] skyRTVs = CreateRTVArray(device, format, RenderTexture, mip);

            perFaceBuffer = new D3D11.Buffer(device, SharpDX.Utilities.SizeOf<KDX_SKYBOX_ENV_RENDERER_BUFFER>(),
                D3D11.ResourceUsage.Default,
                D3D11.BindFlags.ConstantBuffer,
                D3D11.CpuAccessFlags.None,
                D3D11.ResourceOptionFlags.None, 0);


            //D3D11.DeviceContext deviceContext = device.ImmediateContext;

            SetupContext(device, size, shader, sampler, texture);
            Render(device, skyRTVs);


            return RenderTexture;
        }

        private void SetupContext(D3D11.Device device, int size, Shader shader, D3D11.SamplerState sampler, D3D11.Texture2D texture)
        {
            D3D11.DeviceContext deviceContext = device.ImmediateContext;

            deviceContext.Rasterizer.SetViewport(new Viewport(0, 0, size, size));

            deviceContext.InputAssembler.PrimitiveTopology = PrimitiveTopology.TriangleList;
            deviceContext.InputAssembler.InputLayout = shader.InputLayout;

            deviceContext.VertexShader.Set(shader.VertexShader);
            deviceContext.VertexShader.SetConstantBuffer(0, perFaceBuffer);

            deviceContext.PixelShader.Set(shader.PixelShader);
            deviceContext.PixelShader.SetSampler(0, sampler);
            deviceContext.PixelShader.SetShaderResource(0, new D3D11.ShaderResourceView(device, texture));

            deviceContext.InputAssembler.SetVertexBuffers(0, new D3D11.VertexBufferBinding(cubeMesh.VertexBuffer, Utilities.SizeOf<Vertex>(), 0));
            deviceContext.InputAssembler.SetIndexBuffer(cubeMesh.IndexBuffer, Format.R32_UInt, 0);
        }

        private void Render(D3D11.Device device, D3D11.RenderTargetView[] rtvs)
        {
            D3D11.DeviceContext deviceContext = device.ImmediateContext;


            for (int i = 0; i < 6; ++i)
            {
                deviceContext.OutputMerger.SetRenderTargets(rtvs[i]);
                deviceContext.ClearRenderTargetView(rtvs[i], Color.Red);

                //if (i == 5)
                //    continue;
                KDX_SKYBOX_ENV_RENDERER_BUFFER buffer = new KDX_SKYBOX_ENV_RENDERER_BUFFER();
                buffer.World = Matrix.Transpose(Matrix.Identity);
                buffer.View = Matrix.Transpose(views[i]);
                buffer.Proj = Matrix.Transpose(Matrix.PerspectiveFovLH(MathUtil.DegreesToRadians(90), 1, 0.01f, 100.0f));
                buffer.MVP = (buffer.Proj * buffer.View * buffer.World);

                deviceContext.UpdateSubresource(ref buffer, perFaceBuffer);

                deviceContext.DrawIndexed(cubeMesh.Triangles.Count() * 3, 0, 0);

                rtvs[i].Dispose();
            }
        }

        private D3D11.Texture2DDescription GetTexture2DDescription(int width, Format format, int mipLevels)
        {
            return new D3D11.Texture2DDescription()
            {
                Height = width,
                Width = width,
                MipLevels = mipLevels,
                ArraySize = 6,
                Format = format,
                SampleDescription = new SampleDescription(1, 0),
                Usage = D3D11.ResourceUsage.Default,
                BindFlags = D3D11.BindFlags.ShaderResource | D3D11.BindFlags.RenderTarget,
                CpuAccessFlags = D3D11.CpuAccessFlags.None,
                OptionFlags = D3D11.ResourceOptionFlags.GenerateMipMaps | D3D11.ResourceOptionFlags.TextureCube
            };
        }

        private D3D11.SamplerState GetSamplerState(D3D11.Device device)
        {
            return new D3D11.SamplerState(device, new D3D11.SamplerStateDescription()
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

        private D3D11.RenderTargetView[] CreateRTVArray(D3D11.Device device, Format format, D3D11.Texture2D texture, int mipSlice)
        {
            var rtvDesc = new D3D11.RenderTargetViewDescription();
            rtvDesc.Format = format;
            rtvDesc.Texture2D.MipSlice = mipSlice;
            rtvDesc.Dimension = D3D11.RenderTargetViewDimension.Texture2DArray;
            rtvDesc.Texture2DArray.FirstArraySlice = 0;
            //rtvDesc.Texture2DArray.ArraySize = 6;
            //D3D11.RenderTargetView rtv = new D3D11.RenderTargetView(device, texture, rtvDesc);

            rtvDesc.Texture2DArray.ArraySize = 1;
            D3D11.RenderTargetView[] faceRTVs = new D3D11.RenderTargetView[6];

            for (int i = 0; i < 6; ++i)
            {
                rtvDesc.Texture2DArray.FirstArraySlice = i;
                faceRTVs[i] = new D3D11.RenderTargetView(device, texture, rtvDesc);
            }

            return faceRTVs;
        }

        private D3D11.ShaderResourceView CreateShaderResourceView(D3D11.Device device, Format format, D3D11.Texture2D texture)
        {
            var srvDesc = new D3D11.ShaderResourceViewDescription();
            srvDesc.Format = format;
            srvDesc.Dimension = SharpDX.Direct3D.ShaderResourceViewDimension.TextureCube;
            srvDesc.TextureCube.MostDetailedMip = 0;
            srvDesc.TextureCube.MipLevels = -1;
            return new D3D11.ShaderResourceView(device, texture, srvDesc);
        }

    }
}