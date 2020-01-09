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
        static void Main(string[] args)
        {
            try
            {
                semaphore = new MyMonitorSemaphore(3);
                TestThread();
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

        private static void TestThread()
        {
            int threadNum = 10;
            var threads = createThreads(threadNum);
            var waitHandles = createHandles(threadNum);
            RunThreads(threads, waitHandles);
           
            waitMe(waitHandles, 3);

            semaphore.Release(1);
            waitMe(waitHandles, 4);

            semaphore.Release(3);
            waitMe(waitHandles, 7);

            semaphore.Release(2);
            waitMe(waitHandles, 9);

            semaphore.Release(1);
            waitMe(waitHandles, 10);


            /*
            waitWhileNoThreadHasState(threads, ThreadState.Running);
            Console.WriteLine(getNumberOfBlocked(threads));

            semaphore.Release(1);
            waitWhileNoThreadHasState(threads, ThreadState.Running);
            Console.WriteLine(getNumberOfBlocked(threads));

            semaphore.Release(3);
            waitWhileNoThreadHasState(threads, ThreadState.Running);
            Console.WriteLine(getNumberOfBlocked(threads));

            semaphore.Release(2);
            waitWhileNoThreadHasState(threads, ThreadState.Running);
            Console.WriteLine(getNumberOfBlocked(threads));

            semaphore.Release(1);
            waitWhileNoThreadHasState(threads, ThreadState.Running);
            Console.WriteLine(getNumberOfBlocked(threads));

            semaphore.Release(1);
            waitWhileNoThreadHasState(threads, ThreadState.Running);
            Console.WriteLine(getNumberOfBlocked(threads));
            */
            int stop = 1;
        }

        private static void waitMe(WaitHandle[] waitHandles, int stoppedThreadCounter)
        {
            while (Interlocked.CompareExchange(ref result, result, stoppedThreadCounter) != stoppedThreadCounter)
            {
                WaitHandle.WaitAny(waitHandles);
                Console.WriteLine(Interlocked.Read(ref result));
            }
            Console.WriteLine("Result is {0}", Interlocked.Read(ref result));
        }
        private static WaitHandle[] InitializeWork (int threadNumber)
        {
            IEnumerable<Thread> threads = createThreads(threadNumber);
            WaitHandle[] waitHandles = Enumerable.Repeat<WaitHandle>(new AutoResetEvent(false), threadNumber).ToArray();
            // WaitHandle[] wlaitHandes = new WaitHandle[threadNumber];
            var en = waitHandles.GetEnumerator();
            foreach (var thread in threads)
            {
                en.MoveNext();
                thread.Start(en.Current);
            }
            while(Interlocked.CompareExchange(ref result, 3, 3) != 3)
            {
                WaitHandle.WaitAny(waitHandles);
                Console.WriteLine(Interlocked.Read(ref result));
            }
            Console.WriteLine(Interlocked.Read(ref result));
            //waitWhileNoThreadHasState(threads, ThreadState.Unstarted);
            return waitHandles;
        }
        private static IEnumerable<Thread> createThreads(int threadNumber)
        {
            List<Thread> threads = new List<Thread>();
            for(int it = 0; it < threadNumber; ++it)
            {
                Thread t = new Thread(new ParameterizedThreadStart(DoSmth));
                threads.Add(t);
            }
            return threads;
        }

        private static WaitHandle[] createHandles(int threadNumber)
        {
            return  Enumerable.Repeat<WaitHandle>(new AutoResetEvent(false), threadNumber).ToArray();
        }

        private static void RunThreads(IEnumerable<Thread> threads, WaitHandle [] waitHandles)
        {
            var en = waitHandles.GetEnumerator();
            foreach (var thread in threads)
            {
                en.MoveNext();
                thread.Start(en.Current);
            }
        }

        private static void waitWhileNoThreadHasState(IEnumerable<Thread> threads, ThreadState state)
        {
            bool isAnyThreadHasState = true;
            while (isAnyThreadHasState)
            {
                isAnyThreadHasState = false;
                foreach (var thread in threads)
                {
                    if (thread.ThreadState == state)
                    {
                        isAnyThreadHasState = true;
                        break;
                    }
                }
            }
        }

        private static int getNumberOfBlocked(IEnumerable<Thread> threads)
        {
            int number = 0;
            foreach (var thread in threads)
            {
                if (thread.ThreadState == ThreadState.WaitSleepJoin)
                {
                    ++number;
                }
            }
            return number;
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
