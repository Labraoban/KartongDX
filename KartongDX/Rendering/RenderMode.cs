using System;

using D3D11 = SharpDX.Direct3D11;

namespace KartongDX.Rendering
{


    class RenderMode
    {
        public string Name { get; private set; }

        public bool UseSwapChain;

        public D3D11.Texture2DDescription DepthBufferDesc;
        public D3D11.Texture2DDescription RenderTargetDesc;

        public D3D11.RenderTargetViewDescription RenderTargetViewDesc;
        public D3D11.DepthStencilViewDescription DepthStencilViewDesc;

        public D3D11.DepthStencilStateDescription DepthStencilStateDesc;

        public D3D11.DepthStencilState DepthStencilState;



        public RenderMode(string name)
        {
            this.Name = name;
        }
    }
}
