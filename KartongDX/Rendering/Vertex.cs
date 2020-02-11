using SharpDX;
using System.Runtime.InteropServices;


namespace KartongDX.Rendering
{
	[StructLayoutAttribute(LayoutKind.Sequential)]
	public struct Vertex
	{
		public Vector3 Position;
        public Vector3 Normal;
        public Vector2 UV;
        public Color4 Color;
        public Vector3 Tangent;
        public Vector3 Bitangent;

        public Vertex(Vector3 position, Vector3 normal, Vector2 uv, Color4 color)
		{
			Position = position;
            Normal = normal;
            UV = uv;
            Color = color;
            Tangent = new Vector3();
            Bitangent = new Vector3();
        }
	}
}