using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX.DirectInput;

namespace KartongDX.Engine
{
    class Input
    {
        public InputAcessor Acessor { get; private set; }

        private DirectInput directInput;
        private Keyboard keyboard;
        private Mouse mouse;

        private bool[] currentKeys = new bool[256];
        private bool[] prevKeys = new bool[256];

        private bool[] currentMouseButtons = new bool[16];
        private bool[] prevMouseButtons = new bool[16];

        private int x_offset = 0;
        private int y_offset = 0;

        public Input()
        {
            Acessor = new InputAcessor(this);

            directInput = new DirectInput();
            keyboard = new Keyboard(directInput);
            mouse = new Mouse(directInput);

            keyboard.Properties.BufferSize = 256;
            keyboard.Acquire();

            mouse.Properties.BufferSize = 16;
            mouse.Acquire();
        }

        public void Update()
        {
            for (int i = 0; i < 128; ++i)
                prevKeys[i] = currentKeys[i];

            for (int i = 0; i < 16; ++i)
                prevMouseButtons[i] = currentMouseButtons[i];

            keyboard.Poll();
            var data = keyboard.GetBufferedData();
            foreach (var state in data)
            {
                if (state.IsPressed)
                    currentKeys[(int)state.Key] = true;
                if (!state.IsPressed)
                    currentKeys[(int)state.Key] = false;
            }

            mouse.Poll();
            var mouseData = mouse.GetBufferedData();
            foreach (var state in mouseData)
            {
                if (state.IsButton)
                    currentMouseButtons[(int)state.Offset] = state.Value > 0 ? true : false;

                if (state.Offset == MouseOffset.X)
                    x_offset = state.Value;

                if (state.Offset == MouseOffset.Y)
                    y_offset = -state.Value;
            }

            //Logger.Write(LogType.Debug, string.Format("X {0} Y {1}", x_offset, y_offset));
        }

        /* A Class that can be passed down to States and Components to access
         * Specific methods from the Input class.
         */
        public class InputAcessor
        {
            private Input input;

            public InputAcessor(Input input)
            {
                this.input = input;
            }

            public bool GetKey(Key key)
            {
                return input.currentKeys[(int)key];
            }

            public bool GetKeyPressed(Key key)
            {
                return !input.prevKeys[(int)key] && input.currentKeys[(int)key];
            }

            public bool GetKeyReleased(Key key)
            {
                return input.prevKeys[(int)key] && !input.currentKeys[(int)key];
            }

            public bool GetMouseButton(MouseOffset mouse)
            {
                return input.currentMouseButtons[(int)mouse];
            }


            public bool GetMouseButtonPressed(MouseOffset mouse)
            {
                return !input.prevMouseButtons[(int)mouse] && input.currentMouseButtons[(int)mouse];
            }

            public bool GetMouseButtonReleased(MouseOffset mouse)
            {
                return input.prevMouseButtons[(int)mouse] && !input.currentMouseButtons[(int)mouse];
            }

            public SharpDX.Vector2 GetMouseOffset()
            {
                return new SharpDX.Vector2(input.x_offset, input.y_offset);
            }


        }
    }
}
