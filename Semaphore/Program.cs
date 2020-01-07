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
        private static ISemaphore mySemaphore;
        private static int result = 0;
        private static int timeWait = 500;
        static void Main(string[] args)
        {
            try
            {
                mySemaphore = new MyMonitorSemaphore(2);
                //mySemaphore.Release(1);
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
                mySemaphore.Release(1);
                t3.Join(timeWait);
                Console.WriteLine("Result: {0}", result);
                int p = 0;
            }
            catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
            int y = 0;
        }

        public static void Do()
        { 
            mySemaphore.Acquire();
            Interlocked.Increment(ref result);
        }
    }
}
