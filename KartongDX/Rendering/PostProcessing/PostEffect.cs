using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartongDX.Rendering.PostProcessing
{
    class PostEffect : Material
    {
        public int Order { get; set; }

        public PostEffect(Shader shader) : base(shader)
        {
            Order = 0;
        }

        public PostEffect(Shader shader, int order) : base(shader)
        {
            this.Order = order;
        }
    }
}
