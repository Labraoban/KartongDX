using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.Windows;
using System.Drawing;

namespace KartongDX.Rendering
{
	class Window : IDisposable
	{
		private RenderForm renderForm;
		public static int Width { get; private set; }
		public static int Height { get; private set; }

		public Window(string title, int width, int height, bool resizeable)
		{
			Width = width;
			Height = height;

			renderForm = new RenderForm(title);
			renderForm.ClientSize = new Size(width, height);
			renderForm.AllowUserResizing = resizeable;
		}

		public void Dispose()
		{
			renderForm.Dispose();
		}

		public IntPtr GetHandle()
		{
			return renderForm.Handle;
		}

		public RenderForm GetForm()
		{
			return renderForm;
		}
	}
}
