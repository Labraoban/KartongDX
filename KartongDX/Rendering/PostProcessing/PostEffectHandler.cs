using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KartongDX.Rendering.PostProcessing
{
    class PostEffectHandler
    {
        public int NumberOfEffects { get { return effectStack.Count; } }

        private Dictionary<string, PostEffect> effects;
        private List<PostEffect> effectStack;

        public PostEffectHandler()
        {
            effects = new Dictionary<string, PostEffect>();
            effectStack = new List<PostEffect>();
        }

        public void AttachEffect(PostEffect effect, string alias)
        {
            Logger.Write(LogType.Debug, "Added effect using Shader: {0}", alias);
            effects.Add(alias, effect);

        }

        public void AttachEffect(PostEffect effect)
        {
            AttachEffect(effect, effect.Shader.Alias);
        }

        public bool EffectIsAttached(string alias)
        {
            return effects.ContainsKey(alias);
        }

        public bool EffectIsEnabled(string alias)
        {
            return effectStack.Contains(GetAttachedEffect(alias));
        }

        public void EnableEffect(string alias)
        {
            if (EffectIsAttached(alias) && !EffectIsEnabled(alias))
            {
                effectStack.Add(effects[alias]);
                effectStack.OrderBy(o => o.Order);
                Logger.Write(LogType.Debug, "Enabled PostEffect: {0}", alias);
            }
        }

        public void DisableEffect(string alias)
        {
            if (EffectIsEnabled(alias))
            {
                effectStack.Remove(effects[alias]);
                Logger.Write(LogType.Debug, "Disabled PostEffect: {0}", alias);
            }
        }

        public PostEffect GetEffectFromStack(int index)
        {
            return effectStack[index];
        }

        private PostEffect GetAttachedEffect(string alias)
        {
            return effects[alias];
        }
    }
}
