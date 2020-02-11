using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace KartongDX.Rendering
{
	[StructLayoutAttribute(LayoutKind.Sequential)]
	struct PerObjectBuffer
	{
        public SharpDX.Matrix world;
        public SharpDX.Matrix view;
        public SharpDX.Matrix proj;
        public SharpDX.Matrix worldViewProj;
        public SharpDX.Matrix normalMatrix;
        public SharpDX.Matrix invProj;
    }

    [StructLayoutAttribute(LayoutKind.Sequential)]
	struct PerFrameBuffer
	{
        public SharpDX.Vector3 lightDir;
        private int a;
        public SharpDX.Vector3 viewPosition;
        private int b;
        public SharpDX.Vector3 viewDir;
        private int c;
        public float zFar;
        private SharpDX.Vector3 d;
    }
}
