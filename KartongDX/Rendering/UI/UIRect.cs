using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace KartongDX.Rendering.UI
{
    class UIRect : Model
    {
        public UIRect(RectangleF rect, Material material) : base(null, material)
        {
            Vertex[] verts = new Vertex[]
            {
                new Vertex(new Vector3(rect.TopLeft, 0), Vector3.BackwardLH, new Vector2(0, 0), Color.White),
                new Vertex(new Vector3(rect.TopRight, 0), Vector3.BackwardLH, new Vector2(1, 0), Color.White),
                new Vertex(new Vector3(rect.BottomLeft, 0), Vector3.BackwardLH, new Vector2(0, 1), Color.White),
                new Vertex(new Vector3(rect.BottomRight, 0), Vector3.BackwardLH, new Vector2(1, 1), Color.White)
            };

            Triangle[] tris = new Triangle[]
            {
                new Triangle(2, 1, 0),
                new Triangle(3, 1, 2)
            };

            Mesh = new Mesh(verts, tris);
        }
    }
}
