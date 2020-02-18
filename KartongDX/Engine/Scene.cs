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
        public Rendering.Skybox Skybox { get; private set; }
        public bool IsDirty { get; private set; }

        private List<GameObject> sceneObjects;
        private Accessors accessors;

        public Rendering.Camera Camera { get; private set; }

        public Scene(Accessors accessors)
        {
            SceneRoot = new GameObject("root", 0);
            SceneRoot.Name = "SceneRoot";
            sceneObjects = new List<GameObject>();

            this.accessors = accessors;
        }

        public void AddObjectAsRootChild(GameObject gameObject)
        {
            SceneRoot.AddChild(gameObject);
            IsDirty = true;
        }

        /* Updates all GameObjects in sceneObjects that
         * implements IUpdatable.
         */
        public void Update()
        {
            if (IsDirty)
                Flatten();

            foreach (GameObject gameObject in sceneObjects)
            {
                Component[] components = gameObject.Components.GetAll();
                foreach (Component component in components)
                {
                    if (component is IUpdatable)
                        ((IUpdatable)component).Update(accessors);
                }
            }
        }

        /* Drtaws all GameObjects in sceneObjects that
         * implements IDrawable.
         */
        public void Draw(Rendering.RenderQueue renderQueue)
        {
            SceneRoot.ResolveDirty();

            foreach (GameObject gameObject in sceneObjects)
            {
                if(gameObject is IDrawable)
                {
                    // TODO: Implement support for additional drawables.

                    var modelComp = gameObject.Components.Get<_ModelComponent>();

                    if (modelComp != null)
                        renderQueue.Stage(modelComp.Model, gameObject.WorldMatrix);
                }
            }
        }

        public void SetSkybox(Rendering.Skybox skybox)
        {
            this.Skybox = skybox;
        }

        /* If not already created and up to date, this method
         * constructs a new list of Meshes to be rendered.
         * 
         * * return     flattened list of GameObjects with meshes
         */
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

        /* Flatten Scene for simple and quick list of gameobjects
         * for the RenderQueue.
         * TODO: Implement new way of construcing Render Queue
         */
        private void Flatten()
        {
            sceneObjects.Clear();
            RecursiveAdd(SceneRoot);
            IsDirty = false;
        }

        /* Recursively inserts node and its children to sceneObjects list.
         * * Transform node       Node to add to list
         */
        private void RecursiveAdd(Transform node)
        {
            sceneObjects.Add(node as GameObject);

            Transform[] children = node.GetChildren();
            foreach (Transform child in children)
            {
                RecursiveAdd(child);
            }
        }

        public void SetCamera(Rendering.Camera camera)
        {
            this.Camera = camera;
        }
    }
}
