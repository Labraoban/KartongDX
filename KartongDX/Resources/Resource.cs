using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartongDX.Resources
{
	abstract class Resource : IDisposable
	{
		private ResourceDesc resourceDescription;

		public Resource(ResourceDesc resourceDescription)
		{
			Load(resourceDescription);
			this.resourceDescription = resourceDescription;
		}

		public abstract void Dispose();
		protected abstract void Load(ResourceDesc resourceDescription);

		protected void PrintDisposeLog()
		{
			Logger.Write(LogType.Info, "Disposed: [{0}] -  [{1}]", resourceDescription.Alias, resourceDescription.FileName);
		}
	}
}
