using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartongDX.Resources
{
	class ResourceContainer<T>
		: IDisposable
		where T : Resource
	{
		private Dictionary<string, T> data;
		private Dictionary<string, string> aliases;
		private string type;
		private string typeRootPath;

		public ResourceContainer(string typeRootPath)
		{
			this.typeRootPath = typeRootPath;

			data = new Dictionary<string, T>();
			aliases = new Dictionary<string, string>();
			string fullType = typeof(T).ToString();
			int lastIndex = fullType.LastIndexOf('.');
			type = fullType.Substring(lastIndex + 1, fullType.Length - lastIndex - 1);
			Logger.Write(LogType.Info, "Resource Container for \"{0}\" Created", type);
		}

		public void Dispose()
		{
			foreach (KeyValuePair<string, T> entry in data)
			{
				entry.Value.Dispose();
			}
		}

		public void Load(ResourceDesc resourceDescription)
		{
			string fullPath = typeRootPath + resourceDescription.FileName;
			resourceDescription.FileName = fullPath;

			if (!data.ContainsKey(resourceDescription.FileName))
			{
				T resource = (T)Activator.CreateInstance(typeof(T), new object[] { resourceDescription });
				data.Add(resourceDescription.FileName, resource);

				if (resourceDescription.Alias != null)
				{
					aliases.Add(resourceDescription.Alias, resourceDescription.FileName);
					Logger.Write(LogType.Info, "{0} - Loaded Resource \"{1}\" with alias \"{2}\"", type, resourceDescription.FileName, resourceDescription.Alias);
				}
				else
				{
					Logger.Write(LogType.Info, "{0} - Loaded Resource \"{1}\"", type, resourceDescription.FileName);
				}
			}
		}

		public T Get(string fileName)
		{
			return data[fileName];
		}

		public T GetFromAlias(string alias)
		{
			return data[aliases[alias]];
		}

	}
}
