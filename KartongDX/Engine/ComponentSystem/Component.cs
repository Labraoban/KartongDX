using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartongDX.Engine.ComponentSystem
{
	class Component
	{
		public GameObject Owner { get; private set; }
		public int ID { get; private set; }
        public bool IsDrawable { get; protected set; }
        public bool IsUpdatable { get; protected set; }

        protected Component(int id, GameObject owner, bool isUpdatable, bool isDrawable)
		{
			ID = id;
			Owner = owner;
            IsUpdatable = isUpdatable;
            IsDrawable = isDrawable;
		}

		public virtual void Update() {}
	}
}
