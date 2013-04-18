using System;
using System.Collections.Generic;

namespace BohemianArtifact
{
    public delegate void TimerFinished();

    public class Timer
    {
        private bool looping;
        private bool running;

        private double intervalTime;
        private double startTime;
        private double endTime;
        private double currentTime;
        private double previousTime;

        private event TimerFinished finishEvent;

        public bool Running
        {
            get
            {
                return running;
            }
        }
        public float Elapsed
        {
            get
            {
                return (float)((currentTime - startTime) / (endTime - startTime));
            }
        }
        //public float ElapsedTime
        //{
        //    get
        //    {
        //        return interval * Elapsed;
        //    }
        //}
        //public float Remains
        //{
        //    get
        //    {
        //        return 1 - Elapsed;
        //    }
        //}
        //public float RemainsTime
        //{
        //    get
        //    {
        //        return interval * Remains;
        //    }
        //}
        public TimerFinished FinishEvent
        {
            get
            {
                return finishEvent;
            }
            set
            {
                finishEvent = value;
            }
        }
        public double IntervalTime
        {
            get
            {
                return intervalTime;
            }
            set
            {
                intervalTime = value;
                //periodTicks = (ulong)(interval * App.Timer.Frequency);
            }
        }
        public bool Looping
        {
            get
            {
                return looping;
            }
            set
            {
                looping = value;
            }
        }

        public Timer()
        {
            this.intervalTime = 0;
            this.running = false;
        }

        public Timer(float interval)
        {
            this.intervalTime = interval;
            this.running = false;
        }

        public void Start()
        {
            startTime = -1;
            running = true;
        }

        public void Stop()
        {
            running = false;
        }

        public void Finish()
        {
            if (running == false)
            {
                return;
            }

            if (finishEvent != null)
            {
                finishEvent.Invoke();
            }

            if (looping)
            {
                Start();
            }
            else
            {
                Stop();
            }
        }

        public void Update(double time)
        {
            if (running == false)
            {
                return;
            }

            if (startTime == -1)
            {
                // this timer *just* started
                startTime = time;
                endTime = startTime + intervalTime;
                currentTime = startTime;
                previousTime = currentTime;

            }

            previousTime = currentTime;
            currentTime = time;

            if (endTime < currentTime)
            {
                Finish();
            }
        }
    }
}
