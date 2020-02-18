using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Windows;
using SharpDX.DXGI;

using D3D11 = SharpDX.Direct3D11;

namespace KartongDX.Rendering
{
    class RenderViews : IDisposable
    {
        public D3D11.RenderTargetView RTV { get; set; }
        public D3D11.DepthStencilView DSV { get; set; }

        public D3D11.ShaderResourceView RT_SRVT { get; set; }
        public D3D11.ShaderResourceView DS_SRVT { get; set; }

        public RenderViews()
        {

        }

        public RenderViews(D3D11.RenderTargetView rtv, D3D11.DepthStencilView dsv)
        {
            this.RTV = rtv;
            this.DSV = dsv;
        }

        public RenderViews(D3D11.RenderTargetView rtv, D3D11.DepthStencilView dsv,
            D3D11.ShaderResourceView rtv_srvt, D3D11.ShaderResourceView dsv_srvt)
            : this(rtv, dsv)
        {
            this.RT_SRVT = rtv_srvt;
            this.DS_SRVT = dsv_srvt;
        }

        public void Dispose()
        {
            RTV.Dispose();
            DSV.Dispose();
            RT_SRVT.Dispose();
            DS_SRVT.Dispose();
        }
    }
    class RenderTargetHandler : IDisposable
    {
        private enum RenderTarget
        {
            Default,
            Swap
        };

        private RenderTarget next;

        private D3D11.Texture2D renderTarget;
        private D3D11.Texture2D renderTargetSwap;
        private D3D11.Texture2D depthStencil;
        private D3D11.Texture2D depthStencilSwap;

        RenderViews defaultRenderViews;
        RenderViews swapRenderViews;

        public RenderTargetHandler()
        {
            defaultRenderViews = new RenderViews();
            swapRenderViews = new RenderViews();
        }

        public void CreateRenderTarget(
            D3D11.Device device,
            D3D11.Texture2DDescription textureDesc)
        {
            renderTarget = new D3D11.Texture2D(device, textureDesc);
            renderTargetSwap = new D3D11.Texture2D(device, textureDesc);

            defaultRenderViews.RTV = new D3D11.RenderTargetView(device, renderTarget);
            defaultRenderViews.RT_SRVT = new D3D11.ShaderResourceView(device, renderTarget);

            swapRenderViews.RTV = new D3D11.RenderTargetView(device, renderTarget);
            swapRenderViews.RT_SRVT = new D3D11.ShaderResourceView(device, renderTarget);
        }

        public void CreateDepthStencil(
            D3D11.Device device,
            D3D11.Texture2DDescription textureDesc,
            D3D11.DepthStencilViewDescription viewDesc,
            D3D11.ShaderResourceViewDescription resourceViewDesc)
        {
            depthStencil = new D3D11.Texture2D(device, textureDesc);
            depthStencilSwap = new D3D11.Texture2D(device, textureDesc);


            defaultRenderViews.DSV = new D3D11.DepthStencilView(device, depthStencil, viewDesc);
            swapRenderViews.DSV = new D3D11.DepthStencilView(device, depthStencil, viewDesc);

            defaultRenderViews.DS_SRVT = new D3D11.ShaderResourceView(device, depthStencil, resourceViewDesc);
            swapRenderViews.DS_SRVT = new D3D11.ShaderResourceView(device, depthStencil, resourceViewDesc);
        }

        public D3D11.RenderTargetView GetRenderTargetView()
        {
            //if (next == RenderTarget.Default)
            //    Logger.Write(LogType.Debug, "RTV DEF");
            //else
            //    Logger.Write(LogType.Debug, "RTF SWP");


            if (next == RenderTarget.Default)
                return defaultRenderViews.RTV;
            else
                return swapRenderViews.RTV;
        }

        public D3D11.ShaderResourceView GetRenderTargetResourceView()
        {
            //if (next == RenderTarget.Default)
            //    Logger.Write(LogType.Debug, "SRVT DEF");
            //else
            //    Logger.Write(LogType.Debug, "SRVT SWP");

            if (next != RenderTarget.Default)
                return defaultRenderViews.RT_SRVT;
            else
                return swapRenderViews.RT_SRVT;
        }

        public D3D11.DepthStencilView GetDepthStencilView()
        {
            if (next == RenderTarget.Default)
                return defaultRenderViews.DSV;
            else
                return swapRenderViews.DSV;
        }

        public D3D11.ShaderResourceView GetDepthStencilResourceView()
        {
            if (next != RenderTarget.Default)
                return defaultRenderViews.DS_SRVT;
            else
                return swapRenderViews.DS_SRVT;
        }

        public void Swap()
        {
            int n = (int)next + 1;
            next = (RenderTarget)(n % 2);
        }

        public void ResetToDefault()
        {
            next = RenderTarget.Swap;
        }

        public void Dispose()
        {
            defaultRenderViews.Dispose();
            swapRenderViews.Dispose();

            renderTarget.Dispose();
            depthStencil.Dispose();

            renderTargetSwap.Dispose();
            depthStencilSwap.Dispose();
        }
    }
}
