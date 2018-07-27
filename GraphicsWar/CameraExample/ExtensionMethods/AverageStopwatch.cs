using System.Diagnostics;

namespace GraphicsWar.ExtensionMethods
{
    public class AverageStopwatch
    {
        private float _time = 0;
        private float _ticks = 0;
        private float _count = 0;

        public float AverageTime => _time / _count;
        public float AverageTicks => _ticks / _count;

        public void AddMeasuerment(float ms, float tick)
        {
            _time += ms;
            _ticks += tick;
            _count++;
        }
    }
}
