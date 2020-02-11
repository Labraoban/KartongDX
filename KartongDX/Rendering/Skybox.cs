using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using SharpDX.Windows;
using SharpDX.DXGI;
using System.Runtime.InteropServices;

using D3D11 = SharpDX.Direct3D11;
using SharpDX.Direct3D;
using SharpDX.D3DCompiler;

namespace KartongDX.Rendering
{

    class Skybox : IDisposable
    {
        public Model model;
        public Cubemap Cubemap { get; set; }

        public Skybox(Cubemap cubemap, Mesh mesh, Shader shader)
        {
            Cubemap = cubemap;

            Material material = new Material(shader);
            material.AddTexture(0, cubemap.Enviroment);

            model = new Model(mesh, material);
        }

        public void Dispose()
        {
            // TODO: Dispose Cubemap
        }
    }
}