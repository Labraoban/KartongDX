using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace KartongDX.Rendering
{
	class RenderItem
	{
		public Model Model { get; set; }
		public Matrix WorldTransform { get; set; }
	}

	class RenderQueue
	{
        public RenderItem Skybox { get; set; }
		List<RenderItem> queue;

		public RenderQueue()
		{
			queue = new List<RenderItem>();
		}

		public void Stage(Model model, Matrix worldTransform)
		{
			queue.Add(new RenderItem() { Model = model, WorldTransform = worldTransform });
		}

		public List<RenderItem> GetList()
		{
			return queue;
		}

		public void Clear()
		{
			queue.Clear();
		}
	}
}
