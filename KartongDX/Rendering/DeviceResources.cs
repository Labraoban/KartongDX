using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;
using DXGI = SharpDX.DXGI;

namespace KartongDX.Rendering
{
    class DeviceResources : IDisposable
    {
        public D3D11.Device Device { get; private set; }
        public D3D11.DeviceContext DeviceContext { get; private set; }
        public DXGI.SwapChain SwapChain { get; private set; }

        public D3D11.Texture2D BackBuffer { get; private set; }
        public D3D11.RenderTargetView BackBufferRTV { get; private set; }

        // TODO: Use same as in renderer
        private DXGI.SampleDescription sampleDescription = new DXGI.SampleDescription(1, 0);

        public DeviceResources(IntPtr windowHandle, int renderResWidth, int renderResHeight, int targetFPS)
        {
            Device = new D3D11.Device(DriverType.Hardware, D3D11.DeviceCreationFlags.Debug | D3D11.DeviceCreationFlags.DisableGpuTimeout);
            DeviceContext = Device.ImmediateContext;


            var backBufferDesc = new DXGI.ModeDescription(
                renderResWidth,
                renderResHeight,
                new DXGI.Rational(targetFPS, 1),
                DXGI.Format.R8G8B8A8_UNorm
            );

            //var backBufferDesc = new DXGI.ModeDescription()
            //{
            //    Width = renderResWidth,
            //    Height = renderResHeight,
            //    RefreshRate = new DXGI.Rational(targetFPS, 1),
            //    Format = DXGI.Format.R8G8B8A8_UNorm
            //};

            var swapChainDesc = new DXGI.SwapChainDescription()
            {
                ModeDescription = backBufferDesc,
                SampleDescription = sampleDescription,
                Usage = DXGI.Usage.RenderTargetOutput,
                BufferCount = 1,
                OutputHandle = windowHandle,
                IsWindowed = true
            };

            // Create SwapChain
            DXGI.Factory factory = new DXGI.Factory1();
            SwapChain = new DXGI.SwapChain(factory, Device, swapChainDesc);

            // Create BackBuffer RTV
            BackBuffer = SwapChain.GetBackBuffer<D3D11.Texture2D>(0);
            BackBufferRTV = new D3D11.RenderTargetView(Device, BackBuffer);

        }

        public void Dispose()
        {
            BackBufferRTV.Dispose();
            BackBuffer.Dispose();
            SwapChain.Dispose();
            Device.Dispose();
            DeviceContext.Dispose();

        }
    }
}
