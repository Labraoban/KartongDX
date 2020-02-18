using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using KartongDX.Engine;

namespace KartongDX.Rendering
{
    class Camera : GameObject
    {
        public Matrix View { get; private set; }
        public Matrix Proj { get; private set; }
        public Matrix ViewProj { get; private set; }

        public Matrix StaticView { get; private set; }
        public Matrix OrthoProj { get; private set; }
        public Matrix UIViewProj { get; private set; }

        public Viewport Viewport { get; private set; }


        private int width;
        private int height;
        private float aspect;

        private float fov = (float)Math.PI / 4.0f;
        public float ZNear { get; private set; }
        public float ZFar { get; private set; }

        private Vector3 target;


        public Camera(int x, int y, int width, int height, int fov)
            : base("Camera", int.MaxValue)
        {
            this.width = width;
            this.height = height;
            this.aspect = width / (float)height;
            target = new Vector3(0, 0, 0);

            this.fov = MathUtil.DegreesToRadians(fov);
            ZNear = 0.1f;
            ZFar = 100.0f;
            Viewport = new Viewport(x, y, width, height);
        }

        public override void ResolveDirty()
        {
            if (IsDirty)
            {
                base.ResolveDirty();

                Vector3 viewTarget = GetWorldPosition() + WorldMatrix.Forward;
                View = Matrix.LookAtLH(GetWorldPosition(), viewTarget, WorldMatrix.Down);
                Proj = Matrix.PerspectiveFovLH(fov, aspect, ZNear, ZFar);
                ViewProj = Matrix.Multiply(View, Proj);


                StaticView = Matrix.LookAtLH(new Vector3(0, 0, -10), Vector3.ForwardLH, WorldMatrix.Up);
                OrthoProj = Matrix.OrthoLH(width, height, ZNear, ZFar);
                UIViewProj = Matrix.Multiply(StaticView, OrthoProj);
            }
        }

    }
}
