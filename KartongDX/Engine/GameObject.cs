using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartongDX.Engine
{
	class GameObject : Transform
	{
		public string Name { get; set; }
        public int Id { get; private set; }
		public List<string> Tags { get; private set; }

        public bool IsActive { get; private set; }

        public ComponentSystem._ModelComponent ModelComponent { get; set; }
        public ComponentSystem.Components Components { get; set; }

        private GameObject()
			: base()
		{
            Components = new ComponentSystem.Components(this);
		}

        public GameObject(string name, int id)
            : this()
        {
            Name = name;
            Id = id;
            Tags = new List<string>();
            IsActive = true;
        }
	}
}
