using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using D3D11 = SharpDX.Direct3D11;
using SharpDX.D3DCompiler;

namespace KartongDX.Resources.Types
{
    class MeshResourceDescription : ResourceDescription
    {
        public D3D11.Device Device { get; set; }

        public MeshResourceDescription()
            : base() { }

        public MeshResourceDescription(string FileName, string Alias)
            : base(FileName, Alias)
        {
        }
    }

    class MeshResource : Resource
    {
        public MeshResource(MeshResourceDescription resourceDescription)
            : base(resourceDescription)
        { }

        public Rendering.Mesh Mesh { get; private set; }

        public override void Dispose()
        {
            Mesh.Dispose();
            PrintDisposeLog();
        }

        protected override void Load(ResourceDescription resourceDescription)
        {
            MeshResourceDescription desc = (MeshResourceDescription)resourceDescription;
            Mesh = MeshLoader.LoadMesh(resourceDescription.FileName);
        }
    }
}
