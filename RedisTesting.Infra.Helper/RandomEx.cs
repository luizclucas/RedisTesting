using System;

namespace RedisTesting.Infra.Helper
{
    public static class RandomEx
    {
        private static object _lock = new object();
        private static Random _rnd = new Random();

        public static int Next(int min, int max)
        {
            lock (_lock)
            {
                return _rnd.Next(min, max);
            }
        }

        public static int NextVariable(int value, double variation)
        {
            lock (_lock)
            {
                var min = (int)Math.Round(value * (1 - variation), 0);
                var max = (int)Math.Round((value * (1 + variation)), 0);

                return Next(min, max);
            }
        }

        public static TimeSpan Next(TimeSpan min, TimeSpan max)
        {
            lock (_lock)
            {
                return TimeSpan.FromMilliseconds(_rnd.Next((int)min.TotalMilliseconds, (int)max.TotalMilliseconds));
            }
        }

        public static int Next(int max)
        {
            lock (_lock)
            {
                return _rnd.Next(max);
            }
        }

        public static int Next()
        {
            lock (_lock)
            {
                return _rnd.Next();
            }
        }

        public static double NextDouble()
        {
            lock (_lock)
            {
                return _rnd.NextDouble();
            }
        }
    }
}
