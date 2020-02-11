using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartongDX.Engine.ComponentSystem
{
	class _ModelComponent : Component
	{
		public Rendering.Model Model { get; set; } //TODO: private set

		public _ModelComponent(int id, GameObject owner) : base(id, owner, false, true) //TODO id
		{

		}

	}
}
