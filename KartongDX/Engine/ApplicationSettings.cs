using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartongDX.Engine
{
    class ApplicationSettings
    {
        private Resources.IniHandler iniHandler;


        public bool Fullscreen { get; private set; }
        public bool Resizeable { get; private set; }

        public int WindowWidth { get; private set; }
        public int WindowHeight { get; private set; }

        public int RenderWidth { get; private set; }
        public int RenderHeight { get; private set; }

        public bool Force24BitColor { get; private set; }

        public ApplicationSettings()
        {
            iniHandler = new Resources.IniHandler();
        }

        public void Load()
        {
            iniHandler.Load("data/config.ini");

            Fullscreen = iniHandler.GetBool("window", "fullscreen", false);
            Resizeable = iniHandler.GetBool("window", "resizeable", false);

            WindowWidth = iniHandler.GetInt("window", "width", 200);
            WindowHeight = iniHandler.GetInt("window", "height", 100);

            RenderWidth = iniHandler.GetInt("rendering", "width", 200);
            RenderHeight = iniHandler.GetInt("rendering", "height", 100);

            Force24BitColor = iniHandler.GetBool("rendering", "force_24bit_color", false);   
        }

    }
}
