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
        private static int result = 0;
        private static int timeWait = 500;
        static void Main(string[] args)
        {
            try
            {
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
