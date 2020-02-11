using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using IniParser;
using IniParser.Model;

namespace KartongDX.Resources
{
    class IniHandler
    {


        FileIniDataParser parser;
        IniData data;

        public IniHandler()
        {
            string filename = "data/config.ini";

            parser = new FileIniDataParser();

        }

        public void Load(string filename)
        {
            data = parser.ReadFile(filename);
            data["test"]["fullscreen"] = "true";
        }

        public string GetString(string section, string id, string fallback)
        {
            string str = "";
            try
            {
                str = data[section][id];
                if (str == null)
                    throw new Exception();
            }
            catch
            {
                str = fallback;
                LogFail(section, id, fallback);
            }
            return str;
        }

        public bool GetBool(string section, string id, bool fallback)
        {
            string str = data[section][id];
            bool value = fallback;

            try
            {
                bool.TryParse(str, out value);
                LogSuccess(section, id, str);
            }
            catch
            {
                LogFail(section, id, fallback.ToString());
            }

            return value;
        }

        public int GetInt(string section, string id, int fallback)
        {
            string str = data[section][id];
            int value = fallback;

            try
            {

                int.TryParse(str, out value);
                LogSuccess(section, id, str);
            }
            catch
            {
                LogFail(section, id, fallback.ToString());
            }
            return value;
        }

        private void LogSuccess(string section, string id, string value)
        {
            Logger.Write(LogType.Debug, "Found entry [{0}][{1}], value: {2}", section, id, value);
        }

        private void LogFail(string section, string id, string fallback)
        {
            Logger.Write(LogType.Warning, "Could not find entry [{0}][{1}], using fallback: {2}", section, id, fallback);
        }

    }
}
