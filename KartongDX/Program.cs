using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;
using SharpDX.Win32;

namespace KartongDX
{	

	class Program
	{

		
		[STAThread]
		static void Main(string[] args)
		{
            Engine.Game game = new Engine.Game();
			game.Run();
			game.Dispose();

			Logger.Write(LogType.Info, "Program Exit");
		}
	}
}
