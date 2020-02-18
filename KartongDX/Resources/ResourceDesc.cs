using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartongDX.Resources
{
	class ResourceDesc
	{
		public string FileName { get; set; }
		public string Alias { get; set; }


		public ResourceDesc()
			: this(null, null) { }

		public ResourceDesc(string fileName, string alias)
		{
			FileName = fileName;
			Alias = alias;
		}

	}
}
