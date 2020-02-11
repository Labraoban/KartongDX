using SharpDX;
using System.Runtime.InteropServices;
using System;
using D3D11 = SharpDX.Direct3D11;

namespace KartongDX.Rendering
{
    class Mesh : IDisposable
    {
        public readonly Vertex[] Vertices;
        public readonly Triangle[] Triangles;

        public D3D11.Buffer VertexBuffer { get; private set; }
        public D3D11.Buffer IndexBuffer { get; private set; }

        public bool HasBuffers { get; private set; }

        public Mesh(Vertex[] vertices, Triangle[] indexes)
        {
            Vertices = vertices;
            Triangles = indexes;
            HasBuffers = false;

        }

        public void CreateBuffers(D3D11.Device device)
        {
            VertexBuffer = D3D11.Buffer.Create<Vertex>(device, D3D11.BindFlags.VertexBuffer, Vertices);
            IndexBuffer = D3D11.Buffer.Create<Triangle>(device, D3D11.BindFlags.IndexBuffer, Triangles);
            HasBuffers = true;
        }

        public void Dispose()
        {

        }

    }

}
