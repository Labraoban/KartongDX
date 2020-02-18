using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartongDX.Engine.ComponentSystem
{
    interface IUpdatable
    {
        void Update(Accessors accessors);
    }
}
