using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace KartongDX.Engine
{
    class GameTime
    {
        public GameTimeAccessor Accessor { get; private set; }

        private float deltaTime;
        private float scaledDeltaTime;
        private float timeScale;

        private float time;

        private long startup;
        private long lastTime;

        public GameTime()
        {
            Accessor = new GameTimeAccessor(this);

            deltaTime = 0.0f;
            scaledDeltaTime = 0.0f;
            timeScale = 1.0f;
            startup = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        public void Update()
        {
            long currentTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            this.deltaTime = (currentTime - lastTime) / 1000.0f;
            this.scaledDeltaTime = deltaTime * timeScale;
            this.time = (currentTime - startup) / 1000.0f;

            var startTime = Process.GetCurrentProcess().StartTime;
            lastTime = currentTime; ;
        }

        public class GameTimeAccessor
        {
            private GameTime gameTime;

            public float DeltaTime { get { return gameTime.deltaTime; } }
            public float ScaledDeltaTime { get { return gameTime.scaledDeltaTime; } }
            public float TimeScale { get { return gameTime.timeScale; } }
            public float Time { get { return gameTime.time; } }

            public GameTimeAccessor(GameTime gameTime)
            {
                this.gameTime = gameTime;
            }
        }
    }
}
