using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using SharpDX;

namespace KartongDX.Engine
{
    class Transform
    {
        public Matrix LocalMatrix { get; protected set; }
        public Matrix WorldMatrix { get; protected set; }
        public bool IsDirty { get; private set; }
        public bool IsStatic { get; private set; }

        private Transform parent;
        private List<Transform> children;

        private Vector3 position;
        private Vector3 eulerRotation;
        private Vector3 scale;

        private Quaternion rotation;

        public Transform()
        {
            children = new List<Transform>();
            position = new Vector3(0, 0, 0);
            scale = new Vector3(1, 1, 1);
            rotation = Quaternion.Identity;
            SetDirty(true);
        }

        public Transform(Transform parent)
            : base()
        {
            this.parent = parent;
        }

        public virtual void ResolveDirty()
        {
            if (IsDirty)
            {
                IsDirty = false;
                LocalMatrix = Matrix.Scaling(scale) *
                              Matrix.RotationQuaternion(rotation) *
                              Matrix.Translation(position);

                if (parent != null)
                    WorldMatrix = LocalMatrix * parent.WorldMatrix;
                else
                    WorldMatrix = LocalMatrix;
            }

            foreach (Transform child in children)
            {
                child.ResolveDirty();
            }
        }

        public void SetPosition(Vector3 position)
        {
            if (this.position != position)
            {
                this.position = position;
                SetDirty(true);
            }
        }

        /* Returns the local position of the Transform
         */
        public Vector3 GetPosition()
        {
            return position;
        }

        public Vector3 GetWorldPosition()
        {
            if (IsDirty)
                ResolveDirty();
            return WorldMatrix.TranslationVector;
        }

        public void SetRotation(Quaternion rotation)
        {
            if (this.rotation != rotation)
            {
                this.rotation = rotation;
                SetDirty(true);
            }
        }

        public void SetEulerRotation(Vector3 euler)
        {
            {
                this.eulerRotation = euler;

                float roll = MathUtil.DegreesToRadians(euler.X);
                float pitch = MathUtil.DegreesToRadians(euler.Y);
                float yaw = MathUtil.DegreesToRadians(euler.Z);

                float rollOver2 = roll * 0.5f;
                float sinRollOver2 = (float)Math.Sin((double)rollOver2);
                float cosRollOver2 = (float)Math.Cos((double)rollOver2);
                float pitchOver2 = pitch * 0.5f;
                float sinPitchOver2 = (float)Math.Sin((double)pitchOver2);
                float cosPitchOver2 = (float)Math.Cos((double)pitchOver2);
                float yawOver2 = yaw * 0.5f;
                float sinYawOver2 = (float)Math.Sin((double)yawOver2);
                float cosYawOver2 = (float)Math.Cos((double)yawOver2);

                rotation.X = cosYawOver2 * cosPitchOver2 * cosRollOver2 + sinYawOver2 * sinPitchOver2 * sinRollOver2;
                rotation.Y = cosYawOver2 * cosPitchOver2 * sinRollOver2 - sinYawOver2 * sinPitchOver2 * cosRollOver2;
                rotation.Z = cosYawOver2 * sinPitchOver2 * cosRollOver2 + sinYawOver2 * cosPitchOver2 * sinRollOver2;
                rotation.W = sinYawOver2 * cosPitchOver2 * cosRollOver2 - cosYawOver2 * sinPitchOver2 * sinRollOver2;

                SetDirty(true);
            }
        }

        public Quaternion GetRotation()
        {
            return rotation;
        }

        public void SetScale(Vector3 scale)
        {
            this.scale = scale;
            SetDirty(true);
        }

        public void SetDirty(bool dirty)
        {
            IsDirty = dirty;
            foreach (Transform child in children)
            {
                child.SetDirty(true);
            }
        }

        public void SetParent(Transform newParent)
        {
            if (newParent.IsChildOf(this))
            {
                Logger.Write(LogType.Warning, "Can not set child as parent");
                return;
            }
        }

        public void AddChild(Transform child)
        {
            if (child != null)
            {
                children.Add(child);
                child.parent = this;
            }
        }

        public void RemoveChild(Transform child)
        {
            if (child != null)
            {
                child.parent = null;
                children.Remove(child);
            }
        }

        public bool IsChildOf(Transform transform)
        {
            return transform.children.Contains(this);
        }

        public Transform[] GetChildren()
        {
            return children.ToArray();
        }

        public void SetStatic(bool isStatic)
        {
            this.IsStatic = isStatic;
            IsDirty = true;
        }
    }
}
