using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartongDX.Engine
{
    class Accessors
    {
        public GameTime.GameTimeAccessor GameTime { get; private set; }
        public Input.InputAcessor Input { get; private set; }

        public Accessors(
            GameTime.GameTimeAccessor gameTimeAccessor,
            Input.InputAcessor inputAcessor
            )
        {
            this.GameTime = gameTimeAccessor;
            this.Input = inputAcessor;
        }
    }
}
