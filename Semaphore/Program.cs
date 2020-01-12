using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Semaphore
{
    class Program
    {
        private static ISemaphore semaphore;
        private static long result = 0;
        private static int timeWait = 500;
        protected ISemaphore TwoPlaceSemaphore;
        protected static ISemaphore ThreePlaceSemaphore;
        private static int enteranceCounter = 0;
        private static readonly int MAX_WAIT_TIME = 50;
        static void Main(string[] args)
        {
            try
            {
                semaphore = new MyMonitorSemaphore(3);
                ThreePlaceSemaphore = new MyMonitorSemaphore(3);

                AutoAcquireTestThread();
                semaphore = new MySemaphore(2);

                semaphore.Release(-1);
                int x = 0;
                Test();
                TryAcquireTest();
               
            }
            catch(Exception ex)
            {
                Console.WriteLine("Type of exception {0}", ex.GetType().ToString());
                Console.WriteLine(ex.ToString());
            }
            int y = 0;
        }

        public static void AutoAcquireTestThread()
        {
            enteranceCounter = 0;
            int threadNumber = 153;
            var threads = CreateThreads(threadNumber);
            var waitHandles = createHandles(threadNumber);
            RunThreads(threads, waitHandles);
            bool isOK = true;

            WaitSomeThreadsToFinish(waitHandles, 3);
            isOK &= (Interlocked.CompareExchange(ref enteranceCounter, enteranceCounter, 3) == 3);
            Console.WriteLine("Counter {0}", 3);
            for(int it = 6; it <= 153; it += 3)
            {
                ThreePlaceSemaphore.Release(3);
                WaitSomeThreadsToFinish(waitHandles, it);
                isOK &= (Interlocked.CompareExchange(ref enteranceCounter, enteranceCounter, it) == it);
                Console.WriteLine("Counter {0}", it);
            }
            ThreePlaceSemaphore.Release(3);
        }

        public static void AcquireTestThread()
        {
            enteranceCounter = 0;
            int threadNumber = 10;
            var threads = CreateThreads(threadNumber);
            var waitHandles = createHandles(threadNumber);
            RunThreads(threads, waitHandles);
            bool isOK = true;

            WaitSomeThreadsToFinish(waitHandles, 3);
            isOK &= (Interlocked.CompareExchange(ref enteranceCounter, enteranceCounter, 3) == 3);
            Console.WriteLine("Counter {0}", 3);

            ThreePlaceSemaphore.Release(1);
            WaitSomeThreadsToFinish(waitHandles, 4);
            isOK &= (Interlocked.CompareExchange(ref enteranceCounter, enteranceCounter, 4) == 4);
            Console.WriteLine("Counter {0}", 4);

            ThreePlaceSemaphore.Release(3);
            WaitSomeThreadsToFinish(waitHandles, 7);
            isOK &= (Interlocked.CompareExchange(ref enteranceCounter, enteranceCounter, 7) == 7);
            Console.WriteLine("Counter {0}", 7);

            ThreePlaceSemaphore.Release(2);
            WaitSomeThreadsToFinish(waitHandles, 9);
            isOK &= (Interlocked.CompareExchange(ref enteranceCounter, enteranceCounter, 9) == 9);
            Console.WriteLine("Counter {0}", 9);

            ThreePlaceSemaphore.Release(1);
            WaitSomeThreadsToFinish(waitHandles, 10);
            isOK &= (Interlocked.CompareExchange(ref enteranceCounter, enteranceCounter, 10) == 10);
            Console.WriteLine("Counter {0}", 10);

            ThreePlaceSemaphore.Release(3);
        }

        private static void WaitSomeThreadsToFinish(WaitHandle[] waitHandles, int numberThreadsToWait)
        {
            while (Interlocked.CompareExchange(ref enteranceCounter, enteranceCounter, numberThreadsToWait) != numberThreadsToWait)
            {
                WaitHandle.WaitAny(waitHandles);
            }
        }
        private static IEnumerable<Thread> CreateThreads(int threadNumber)
        {
            List<Thread> threads = new List<Thread>();
            for (int it = 0; it < threadNumber; ++it)
            {
                Thread thread = new Thread(new ParameterizedThreadStart(IncrementCounterWithNotice));
                threads.Add(thread);
            }
            return threads;
        }

        private static void RunThreads(IEnumerable<Thread> threads, WaitHandle[] waitHandles)
        {
            var en = waitHandles.GetEnumerator();
            foreach (var thread in threads)
            {
                en.MoveNext();
                thread.Start(en.Current);
            }
        }
        public static void IncrementCounterWithNotice(object state)
        {
            AutoResetEvent are = (AutoResetEvent)state;
            ThreePlaceSemaphore.Acquire();
            Interlocked.Increment(ref enteranceCounter);
            are.Set();
        }

        private static WaitHandle[] createHandles(int threadNumber)
        {
            return Enumerable.Repeat<WaitHandle>(new AutoResetEvent(false), threadNumber).ToArray();
        }
        private static void Test()
        {
            semaphore = new MyMonitorSemaphore(2);
            //semaphore.Release(1);
            System.Threading.Thread t = new Thread(new ThreadStart(Do));
            System.Threading.Thread t2 = new Thread(new ThreadStart(Do));
            System.Threading.Thread t3 = new Thread(new ThreadStart(Do));
            t.Start();
            t2.Start();
            t3.Start();
            t.Join(timeWait);
            t2.Join(timeWait);
            t3.Join(timeWait);
            Console.WriteLine("Result: {0}", result);
            semaphore.Release(1);
            t3.Join(timeWait);
            Console.WriteLine("Result: {0}", result);
            int p = 0;
        }
        public static void Do()
        { 
            semaphore.Acquire();
            Interlocked.Increment(ref result);
        }

        public static void DoSmth(object state)
        { 
            AutoResetEvent are = (AutoResetEvent)state;
            semaphore.Acquire();
            Interlocked.Increment(ref result);
            are.Set();
        }

        public static void TryAcquireTest()
        {
            semaphore = new MyMonitorSemaphore(2);
            System.Threading.Thread t = new Thread(new ThreadStart(Do));
            System.Threading.Thread t2 = new Thread(new ThreadStart(Do));
            System.Threading.Thread t3 = new Thread(new ThreadStart(Do));

            bool isGood = true;

            isGood &= semaphore.TryAcquire();
            semaphore.Release(1);

            t.Start();
            t.Join(timeWait);

            isGood &= semaphore.TryAcquire();
            semaphore.Release(1);

            t2.Start();
            t2.Join(timeWait);

            isGood &= !semaphore.TryAcquire();

            t3.Start();
            t3.Join(timeWait);

            isGood &= !semaphore.TryAcquire();

            semaphore.Release(1);
            t3.Join(timeWait);

            isGood &= !semaphore.TryAcquire();

            semaphore.Release(1);
            isGood &= semaphore.TryAcquire();
            semaphore.Release(1);

            semaphore.Release(1);
        }
    }
}
