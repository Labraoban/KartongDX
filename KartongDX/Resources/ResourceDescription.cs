using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartongDX.Resources
{
	class ResourceDescription
	{
		public string FileName { get; set; }
		public string Alias { get; set; }


		public ResourceDescription()
			: this(null, null) { }

		public ResourceDescription(string fileName, string alias)
		{
			FileName = fileName;
			Alias = alias;
		}

	}
}
