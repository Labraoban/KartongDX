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
using SharpDX.D3DCompiler;

using KartongDX.Rendering.ResourceViews;
using KartongDX.Engine;

namespace KartongDX.Rendering
{
    class RenderViews
    {
        public D3D11.RenderTargetView RTV;
        public D3D11.DepthStencilView DSV;

        public D3D11.ShaderResourceView RTV_SRVT;
        public D3D11.ShaderResourceView DSV_SRVT;

        public RenderViews(D3D11.RenderTargetView rtv, D3D11.DepthStencilView dsv)
        {
            this.RTV = rtv;
            this.DSV = dsv;
        }

        public RenderViews(D3D11.RenderTargetView rtv, D3D11.DepthStencilView dsv,
            D3D11.ShaderResourceView rtv_srvt, D3D11.ShaderResourceView dsv_srvt)
            : this(rtv, dsv)
        {
            this.RTV_SRVT = rtv_srvt;
            this.DSV_SRVT = dsv_srvt;
        }
    }

    class RenderMode : IDisposable
    {
        public string Name { get; private set; }

        public bool UseSwapChain;
        public bool UseDepthStencil { get { return RenderViews.DSV != null; } }

        public D3D11.Texture2DDescription DepthBufferDesc;
        public D3D11.Texture2DDescription RenderTargetDesc;

        public D3D11.RenderTargetViewDescription RenderTargetViewDesc;
        public D3D11.DepthStencilViewDescription DepthStencilViewDesc;

        public D3D11.DepthStencilStateDescription DepthStencilStateDesc;

        public D3D11.DepthStencilState DepthStencilState;


        public D3D11.Texture2D RenderTarget;
        public D3D11.Texture2D DepthBuffer;
        public RenderViews RenderViews;

        public D3D11.Texture2D RenderTargetSwap;
        public D3D11.Texture2D DepthBufferSwap;
        public RenderViews RenderViewsSwap;


        public RenderMode(string name)
        {
            this.Name = name;
        }

        public void Dispose()
        {
            RenderTarget.Dispose();
            DepthBuffer.Dispose();

            RenderTargetSwap.Dispose();
            DepthBufferSwap.Dispose();
        }
    }
}
