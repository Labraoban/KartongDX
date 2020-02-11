using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using D3D11 = SharpDX.Direct3D11;
using KartongDX.Resources;

namespace KartongDX.Engine.States
{
    abstract class GameState
    {
        public string Name { get; private set; }

        protected Scene stateScene;
        protected Scene uiScene;

        public GameState(string name)
        {
            this.Name = name;
            stateScene = new Scene();
            uiScene = new Scene();
        }

        public void Load(Resources.ResourceManager resourceManager, SharpDX.Direct3D11.Device device)
        {
            Logger.StartLogSection(string.Format("{0} - Loading State Resources", Name));
            LoadResources(resourceManager, device);
            Logger.EndLogSection();
        }

        protected abstract void LoadResources(Resources.ResourceManager resourceManager, SharpDX.Direct3D11.Device device);

        public abstract void Init(ResourceManager resourceManager);

        public virtual void Update(GameTime gameTime)
        {
            stateScene.Update(gameTime);
            uiScene.Update(gameTime);
        }

        public virtual void Draw(Rendering.RenderQueue renderQueue)
        {
            stateScene.Draw(renderQueue);
        }

        public virtual void DrawUI(Rendering.RenderQueue renderQueue)
        {
            //uiScene.Draw(renderQueue);
        }

        public Scene GetScene()
        {
            return stateScene;
        }
    }
}
