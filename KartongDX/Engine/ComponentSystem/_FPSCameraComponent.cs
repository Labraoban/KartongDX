using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SharpDX;

namespace KartongDX.Engine.ComponentSystem
{
    class _FPSCameraComponent : Component, IUpdatable
    {
        private Vector3 cameraEulerRotation;

        public _FPSCameraComponent(int id, GameObject owner) : base(id, owner) //TODO id
        {

        }

        public void Update(Accessors accessors)
        {
            float x = 0;
            float y = 0;

            x += accessors.Input.GetKey(SharpDX.DirectInput.Key.D) ? 1.0f : 0.0f;
            x -= accessors.Input.GetKey(SharpDX.DirectInput.Key.A) ? 1.0f : 0.0f;

            y += accessors.Input.GetKey(SharpDX.DirectInput.Key.W) ? 1.0f : 0.0f;
            y -= accessors.Input.GetKey(SharpDX.DirectInput.Key.S) ? 1.0f : 0.0f;

            cameraEulerRotation.X = 0;

            cameraEulerRotation.Y -= accessors.Input.GetKey(SharpDX.DirectInput.Key.Right) ? 1.0f : 0.0f;
            cameraEulerRotation.Y += accessors.Input.GetKey(SharpDX.DirectInput.Key.Left) ? 1.0f : 0.0f;

            cameraEulerRotation.Z += accessors.Input.GetKey(SharpDX.DirectInput.Key.Up) ? 1.0f : 0.0f;
            cameraEulerRotation.Z -= accessors.Input.GetKey(SharpDX.DirectInput.Key.Down) ? 1.0f : 0.0f;

            cameraEulerRotation.Y = cameraEulerRotation.Y % 360;
            cameraEulerRotation.Z = cameraEulerRotation.Z % 360;


            Owner.SetEulerRotation(cameraEulerRotation * 2.0f);

            Rendering.Camera camera = (Rendering.Camera)Owner.GetChildren()[0];

            Vector3 movement = new Vector3(x, 0, y) * 0.1f;
            Vector3 newPos = Owner.GetPosition() + AdjustDirectionToCamera(movement);

            Owner.SetPosition(newPos);
        }

        private Vector3 AdjustDirectionToCamera(Vector3 move)
        {
            Vector3 forward = Owner.WorldMatrix.Forward;
            Vector3 right = Owner.WorldMatrix.Right;
            forward.Y = 0;
            right.Y = 0;

            forward.Normalize();
            right.Normalize();

            return (move.X * right + move.Z * forward);
        }
    }
}
