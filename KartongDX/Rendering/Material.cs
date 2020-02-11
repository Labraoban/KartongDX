using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D3D11 = SharpDX.Direct3D11;

namespace KartongDX.Rendering
{
	public enum TextureMap
	{
		Albedo,
		Normal,
        Roughness,
        Metallic,
		Height,
		Ambient,
		Emissive,
		Other
	}

	class Material
	{
		public Shader Shader { get; private set; }
		private Dictionary<int, Texture> textures;

        public List<int> Slots { get; private set; }

		public Material(Shader shader)
		{
			Shader = shader;
			textures = new Dictionary<int, Texture>();
            Slots = new List<int>();
		}

        public void AddTexture(int slot, Texture texture)
		{
			textures.Add(slot, texture);
            Slots = textures.Keys.ToList();
        }

		public Texture GetTexture(int slot)
		{
			return textures[slot];
		}
	}
}
