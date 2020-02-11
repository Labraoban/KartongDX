using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartongDX.Engine.States
{
    class GameStateMachine
    {
        public Dictionary<string, GameState> gameStates;
        public List<GameState> stateStack;

        public GameStateMachine()
        {
            gameStates = new Dictionary<string, GameState>();
            stateStack = new List<GameState>();
        }

        public void Attach(GameState gameState)
        {
            if(gameStates.Keys.Contains(gameState.Name))
            {
                // already attached
            }
            else
            {
                gameStates.Add(gameState.Name, gameState);
            }
        }
        
        public void AddToStack(string name)
        {
            GameState state = gameStates[name];
            stateStack.Add(state);
        }

        public void Update(GameTime gameTime)
        {
            foreach(GameState state in stateStack)
            {
                state.Update(gameTime);
            }
        }

        public void Draw(Rendering.RenderController renderController)
        {
            foreach (GameState state in stateStack)
            {
                state.Draw(renderController.GetRenderQueue(Rendering.RenderController.QueueType.Q3D));
                state.DrawUI(renderController.GetRenderQueue(Rendering.RenderController.QueueType.QUI));
            }
        }

        public List<Scene> GetScenes()
        {
            List<Scene> scenes = new List<Scene>();
            foreach (GameState state in stateStack)
            {
                scenes.Add(state.GetScene());
            }
            return scenes;
        }
    }
}
