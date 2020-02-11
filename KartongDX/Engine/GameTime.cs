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

        public float DeltaTime { get; private set; }
        public float ScaledDeltaTime { get; private set; }
        public float TimeScale { get; private set; }

        public float Time { get; private set; }

        private long startup;
        private long lastTime;

        public GameTime()
        {
            DeltaTime = 0.0f;
            ScaledDeltaTime = 0.0f;
            TimeScale = 1.0f;
            startup = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
        }

        public void Update()
        {
            long currentTime = DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;

            this.DeltaTime = (currentTime - lastTime) / 1000.0f;
            this.ScaledDeltaTime = DeltaTime * TimeScale;
            this.Time = (currentTime - startup) / 1000.0f;

            var startTime = Process.GetCurrentProcess().StartTime;
            lastTime = currentTime;;
        }
    }
}
