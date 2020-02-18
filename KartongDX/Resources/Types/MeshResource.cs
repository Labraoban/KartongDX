using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using D3D11 = SharpDX.Direct3D11;
using SharpDX.D3DCompiler;

namespace KartongDX.Resources.Types
{
    class MeshResourceDesc : ResourceDesc
    {
        public D3D11.Device Device { get; set; }

        public MeshResourceDesc()
            : base() { }

        public MeshResourceDesc(string FileName, string Alias)
            : base(FileName, Alias)
        {
        }
    }

    class MeshResource : Resource
    {
        public MeshResource(MeshResourceDesc resourceDescription)
            : base(resourceDescription)
        { }

        public Rendering.Mesh Mesh { get; private set; }

        public override void Dispose()
        {
            Mesh.Dispose();
            PrintDisposeLog();
        }

        protected override void Load(ResourceDesc resourceDescription)
        {
            MeshResourceDesc desc = (MeshResourceDesc)resourceDescription;
            Mesh = MeshLoader.LoadMesh(resourceDescription.FileName);
        }
    }
}
