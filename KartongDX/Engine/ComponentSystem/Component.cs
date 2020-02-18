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
        public Accessors Accessors { protected get; set; }
		public int ID { get; private set; }

        /* - id
         * - owner
         * - isUpdatable    Determines if 
         * - isDrawable
         */
        protected Component(int id, GameObject owner)
		{
			ID = id;
			Owner = owner;
		}
	}
}
