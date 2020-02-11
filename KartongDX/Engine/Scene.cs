using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using KartongDX.Engine.ComponentSystem;

namespace KartongDX.Engine
{
    class Scene
    {
        public GameObject SceneRoot { get; private set; }
        public Rendering.Skybox Skybox { get; set; }
        public List<GameObject> sceneObjects;

        public bool IsDirty { get; private set; }

        public Scene()
        {
            SceneRoot = new GameObject();
            SceneRoot.Name = "SceneRoot";
            sceneObjects = new List<GameObject>();

            //Skybox = new Rendering.Skybox(); ;
        }

        public void AddObjectAsRootChild(GameObject gameObject)
        {
            SceneRoot.AddChild(gameObject);
            IsDirty = true;
        }

        public void Update(GameTime gameTime)
        {
            if (IsDirty)
                Flatten();

            foreach (GameObject gameObject in sceneObjects)
            {
                Component[] components = gameObject.Components.GetAll();
                foreach (Component component in components)
                {
                    if (component.IsUpdatable)
                        component.Update();
                }
            }
        }

        public void Draw(Rendering.RenderQueue renderQueue)
        {
            SceneRoot.ResolveDirty();

            foreach (GameObject gameObject in sceneObjects)
            {
                var modelComp = gameObject.Components.Get<_ModelComponent>();

                if (modelComp != null)
                    renderQueue.Stage(modelComp.Model, gameObject.WorldMatrix);
            }
        }

        public Rendering.RenderQueue GetRenderQueue()
        {
            if (IsDirty)
                Flatten();

            Rendering.RenderQueue renderQueue = new Rendering.RenderQueue();

            SceneRoot.ResolveDirty();

            foreach (GameObject gameObject in sceneObjects)
            {
                var modelComp = gameObject.Components.Get<_ModelComponent>();

                if (modelComp != null)
                    renderQueue.Stage(modelComp.Model, gameObject.WorldMatrix);
            }
            return renderQueue;
        }

        private void Flatten()
        {
            sceneObjects.Clear();
            Add(SceneRoot);
            IsDirty = false;
        }

        private void Add(Transform node)
        {
            sceneObjects.Add(node as GameObject);

            Transform[] children = node.GetChildren();
            foreach (Transform child in children)
            {
                Add(child);
            }
        }
    }
}
