using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartongDX.Engine.ComponentSystem
{
    class Components
    {
        private Dictionary<Type, Component> components;
        private GameObject owner;

        public Components(GameObject owner)
        {
            this.owner = owner;
            components = new Dictionary<Type, Component>();
        }

        public void Create<T>() where T : Component
        {
            T component = (T)Activator.CreateInstance(typeof(T), new object[] { 0, owner });
            components.Add(typeof(T), component as Component);
        }

        public T Get<T>() where T : Component
        {
            if (components.ContainsKey(typeof(T)))
                return components[typeof(T)] as T;
            return null;
        }

        public void Remove<T>() where T : Component
        {
            if (components.ContainsKey(typeof(T)))
                components.Remove(typeof(T));
        }

        public Component[] GetAll()
        {
            return components.Values.ToArray();
        }
    }
}
