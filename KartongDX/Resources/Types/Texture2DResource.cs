using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using D3D11 = SharpDX.Direct3D11;
using SharpDX.D3DCompiler;
using SharpDX;

namespace KartongDX.Resources.Types
{
	class Texture2DResourceDesc : ResourceDesc
	{
		public D3D11.Device Device { get; set; }
        public bool CreateSRVT { get; set; }

		public Texture2DResourceDesc()
			: base() { }

		public Texture2DResourceDesc(string FileName, string Alias, D3D11.Device device, bool createSRVT = true)
			: base(FileName, Alias)
		{
			Device = device;
            CreateSRVT = createSRVT;
		}
	}

	class Texture2DResource : Resource
	{
		public Texture2DResource(Texture2DResourceDesc resourceDescription)
			: base(resourceDescription)
		{ }

        public Rendering.Texture Texture { get; private set; }

        public int Width { get; private set; }
		public int Height { get; private set; }

		public override void Dispose()
		{
			Texture.Dispose();
			PrintDisposeLog();
		}

		protected override void Load(ResourceDesc resourceDescription)
		{
			Texture2DResourceDesc desc = (Texture2DResourceDesc)resourceDescription;

			var texture2D = CommonDX.TextureLoader.CreateTexture2DFromBitmap(desc.Device, CommonDX.TextureLoader.LoadBitmap(new SharpDX.WIC.ImagingFactory2(), desc.FileName));
			Width = texture2D.Description.Width;
			Height = texture2D.Description.Height;


            D3D11.ShaderResourceView srvt = null;

            if(desc.CreateSRVT)
                srvt = new D3D11.ShaderResourceView(desc.Device, texture2D);

            Texture = new Rendering.Texture(texture2D, srvt);
		}

	}
}
