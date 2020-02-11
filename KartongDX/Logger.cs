using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace KartongDX
{
	public enum LogType
	{
		Debug,
		Info,
		Warning,
		Error,
		Section,
	}

	class Logger
	{


		private static Stack<string> logSections = new Stack<string>();
		private const string SECTION_START = "---- {0} ----";
		private const string SECTION_END = "---- {0} ----";
		private const string INDENT = "  ";

		private static string indent = "";

		private static Dictionary<LogType, string> typePrints = new Dictionary<LogType, string>()
		{
			{ LogType.Debug,   "Debug  " },
			{ LogType.Info,    "Info   " },
			{ LogType.Warning, "Warning" },
			{ LogType.Error,   "Error  " },
			{ LogType.Section, "Section" },
		};

		public static void Write(LogType type, string str, params object[] args)
		{
            //StackFrame frame = new StackTrace().GetFrame(1);
            //string file = frame.GetFileName();
            //int line = frame.GetFileLineNumber();
            //Console.WriteLine(file);

            string prefix = string.Format("{0} | ", GetLogHeader(type)/*, file, line*/);

            Console.WriteLine(prefix + str, args);
		}

		private static string GetLogHeader(LogType type)
		{
			return string.Format("[{0}] {1}{2}", DateTime.Now.ToString("HH:mm:ss"), typePrints[type], indent);
		}

		public static void StartLogSection(string str)
		{
			Write(LogType.Section, string.Format(SECTION_START, str));
			logSections.Push(str);

			indent = "";
			for (int i = 0; i < logSections.Count; ++i)
				indent += INDENT;
		}

		public static void EndLogSection()
		{
			if (logSections.Count <= 0)
				return;

			string str = logSections.Peek();
			logSections.Pop();

			indent = "";
			for (int i = 0; i < logSections.Count; ++i)
				indent += INDENT;

            //Write(LogType.Section, /* string.Format(SECTION_END, str)*/ "----");

        }
	}
}
