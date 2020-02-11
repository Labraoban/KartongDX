using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;
using KartongDX.Engine;

namespace KartongDX.Rendering
{
	class Camera : Transform
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
		private float zNear = 0.1f;
        public float ZFar { get; private set; }

		private Vector3 target;


		public Camera(int x, int y, int width, int height, int fov)
			: base()
		{
			this.width = width;
			this.height = height;
			this.aspect = width / (float)height;
			target = new Vector3(0, 0, 0);

            this.fov = MathUtil.DegreesToRadians(fov);

			Viewport = new Viewport(x, y, width, height);
		}

		public override void ResolveDirty()
		{
			if (IsDirty)
			{
				base.ResolveDirty();

				Vector3 viewTarget = GetPosition() + WorldMatrix.Forward;
				View = Matrix.LookAtLH(GetPosition(), viewTarget, WorldMatrix.Up);
				Proj = Matrix.PerspectiveFovLH(fov, aspect, zNear, ZFar);
				ViewProj = Matrix.Multiply(View, Proj);


                StaticView = Matrix.LookAtLH(new Vector3(0, 0, -10), Vector3.ForwardLH, WorldMatrix.Up);
                OrthoProj = Matrix.OrthoLH(width, height, zNear, ZFar);
                UIViewProj = Matrix.Multiply(StaticView, OrthoProj);
			}
		}

	}
}
