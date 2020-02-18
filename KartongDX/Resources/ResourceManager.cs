using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartongDX.Resources
{

	class ResourceManager : IDisposable
    {
        public ResourceContainer<Types.ShaderResource> Shaders { get; private set; }
        public ResourceContainer<Types.Texture2DResource> Textures { get; private set; }
        public ResourceContainer<Types.MeshResource> Meshes { get; private set; }

        public ResourceManager(string resourcePath)
		{
            Logger.StartLogSection("Creating Resource Containers");
            Shaders = new ResourceContainer<Types.ShaderResource>(resourcePath + "shaders/");
			Textures = new ResourceContainer<Types.Texture2DResource>(resourcePath + "textures/");
            Meshes = new ResourceContainer<Types.MeshResource>(resourcePath + "models/");
            Logger.EndLogSection();
		}

		public void Dispose()
		{
            Shaders.Dispose();
			Textures.Dispose();
            Meshes.Dispose();
		}
	}
}
