using System;

namespace ExponentialRetry
{



    class Program
    {
        static void Main(string[] args)
        {
            var retry = RetryExponential(5, new TimeSpan(0, 0, 3), new TimeSpan(0, 0, 90), new TimeSpan(0, 0, 6));

            TimeSpan delay;

            for (int i = 0; i < 11; i++)
            {
                var ex = new Exception();
                var re = retry();
                if (re(i, ex, out delay))
                    Console.WriteLine(delay);

            }
            


            Console.ReadLine();
        }

        public delegate ShouldRetry RetryPolicy();
        public delegate bool ShouldRetry(int retryCount, Exception lastException, out TimeSpan delay);

        public static RetryPolicy RetryExponential(int retryCount, TimeSpan minBackoff, TimeSpan maxBackoff, TimeSpan deltaBackoff)
        {
            return () =>
            {
                return (int currentRetryCount, Exception lastException, out TimeSpan retryInterval) =>
                {
                    if (currentRetryCount < retryCount)
                    {
                        Random rand = new Random();
                        int increment = (int)((Math.Pow(2, currentRetryCount) - 1) * rand.Next((int)(deltaBackoff.TotalMilliseconds * 0.8), (int)(deltaBackoff.TotalMilliseconds * 1.2)));
                        int timeToSleepMsec = (int)Math.Min(minBackoff.TotalMilliseconds + increment, maxBackoff.TotalMilliseconds);

                        retryInterval = TimeSpan.FromMilliseconds(timeToSleepMsec);

                        return true;
                    }

                    retryInterval = TimeSpan.Zero;
                    return false;
                };
            };
        }

        public static RetryPolicy LinearRetry(int retryCount, TimeSpan intervalBetweenRetries)
        {
            return () =>
            {
                return (int currentRetryCount, Exception lastException, out TimeSpan retryInterval) =>
                {
                    // Do custom work here               
                    // Set backoff
                    retryInterval = intervalBetweenRetries;
                    // Decide if we should retry, return bool
                    return currentRetryCount < retryCount;

                };
            };
        }
    }
}
