using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartongDX.Rendering
{
	class Model
	{
		public Mesh Mesh { get; protected set; }
		public Material Material { get; private set; }

		public Model(Mesh mesh, Material material)
		{
			Mesh = mesh;
			Material = material;
		}
	}
}
