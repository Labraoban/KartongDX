using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using D3D11 = SharpDX.Direct3D11;

namespace KartongDX.Rendering
{
    class Texture : IDisposable
    {
        public D3D11.Texture2D Texture2D { get; private set; }
        public D3D11.ShaderResourceView SRVT { get; private set; }

        public Texture(D3D11.Texture2D texture2D, D3D11.ShaderResourceView shaderResourceView)
        {
            this.Texture2D = texture2D;
            this.SRVT = shaderResourceView;
        }

        public void Dispose()
        {
            Texture2D.Dispose();
            SRVT.Dispose();
        }

    }
}
