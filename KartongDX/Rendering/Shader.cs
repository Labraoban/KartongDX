using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using D3D11 = SharpDX.Direct3D11;
using SharpDX;
using SharpDX.D3DCompiler;

namespace KartongDX.Rendering
{
	class Shader
	{
		public D3D11.PixelShader PixelShader { get; private set; }
		public D3D11.VertexShader VertexShader { get; private set; }

		public ShaderSignature InputSignature { get; private set; }
		public D3D11.InputLayout InputLayout { get; private set; }

        public string Alias { get; private set; }

        public Shader(D3D11.Device device,
            D3D11.VertexShader vertexShader,
            D3D11.PixelShader pixelShader,
            ShaderSignature inputSignature,
            D3D11.InputElement[] inputElements,
            string alias)
        {
            this.VertexShader = vertexShader;
            this.PixelShader = pixelShader;
            this.InputSignature = inputSignature;
            this.InputLayout = new D3D11.InputLayout(device, InputSignature, inputElements);
            this.Alias = alias;
        }
    }
}
