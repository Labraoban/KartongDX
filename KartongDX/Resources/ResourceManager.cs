using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartongDX.Resources
{

	class ResourceManager : IDisposable
    {
        public static ResourceManager instance; // TODO: Remove singleton
        public ResourceContainer<Types.ShaderResource> Shaders { get; private set; }
        public ResourceContainer<Types.Texture2DResource> Textures { get; private set; }
        public ResourceContainer<Types.MeshResource> Meshes { get; private set; }

        public ResourceManager()
		{
            Logger.StartLogSection("Creating Resource Containers");
            Shaders = new ResourceContainer<Types.ShaderResource>();
			Textures = new ResourceContainer<Types.Texture2DResource>();
            Meshes = new ResourceContainer<Types.MeshResource>();
            Logger.EndLogSection();

            instance = this;
		}

		public void Dispose()
		{
            Shaders.Dispose();
			Textures.Dispose();
            Meshes.Dispose();
		}

	}
}
