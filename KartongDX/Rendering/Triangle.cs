using System.Runtime.InteropServices;
using SharpDX;

namespace KartongDX.Rendering
{
	[StructLayoutAttribute(LayoutKind.Sequential)]
	struct Triangle
	{
		public readonly uint v0;
		public readonly uint v1;
		public readonly uint v2;

		public Triangle(uint v0, uint v1, uint v2)
		{
			this.v0 = v0;
			this.v1 = v1;
			this.v2 = v2;
		}
	}
}
