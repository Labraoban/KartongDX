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
            //Resources.Utility.HDRFileLoader.LoadHDRFile("data/textures/studio_small_05_4k.hdr");
            //Resources.Utility.HDRFileLoader.LoadHDRFile("data/textures/hdrtest.hdr");


            Engine.GameEngine game = new Engine.GameEngine();
			game.Run();
			game.Dispose();

			Logger.Write(LogType.Info, "Program Exit");
		}
	}
}
