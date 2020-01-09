using Microsoft.VisualStudio.TestTools.UnitTesting;
using Semaphore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Semaphore.Tests
{
    public abstract class MyBaseSemaphoreTest
    {
        protected ISemaphore semaphore;
        protected ISemaphore semaphore3;
        private int enteranceCounter = 0;
        private int result = 0;
        private static int timeWait = 500;

        [TestMethod]
        [ExpectedException(typeof(System.Threading.SemaphoreFullException))]
        public void Release_Throws()
        {
            semaphore.Release(1);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentOutOfRangeException))]
        public abstract void NegativeNumberOfthread();

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentOutOfRangeException))]
        public abstract void NullNumberOfthread();

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentOutOfRangeException))]
        public void NegativeReleaseNumber()
        {
            System.Threading.Thread t = new Thread(new ThreadStart(Do));
            t.Start();
            t.Join(timeWait);
            semaphore.Release(-1);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentOutOfRangeException))]
        public void NullReleaseNumber()
        {
            System.Threading.Thread t = new Thread(new ThreadStart(Do));
            t.Start();
            t.Join(timeWait);
            semaphore.Release(0);
        }

        [TestMethod]
        public void AcquireTestThread()
        {
            result = 0;
            int threadNum = 10;
            var threads = createThreads(threadNum);
            var waitHandles = createHandles(threadNum);
            RunThreads(threads, waitHandles);
            bool isGood = true;
            
            waitMe(waitHandles, 3);
            isGood &= (Interlocked.CompareExchange(ref result, result, 3) == 3);

            semaphore3.Release(1);
            waitMe(waitHandles, 4);
            isGood &= (Interlocked.CompareExchange(ref result, result, 4) == 4);

            semaphore3.Release(3);
            waitMe(waitHandles, 7);
            isGood &= (Interlocked.CompareExchange(ref result, result, 7) == 7);

            semaphore3.Release(2);
            waitMe(waitHandles, 9);
            isGood &= (Interlocked.CompareExchange(ref result, result, 9) == 9);

            semaphore3.Release(1);
            waitMe(waitHandles, 10);
            isGood &= (Interlocked.CompareExchange(ref result, result, 10) == 10);
            Assert.AreEqual(true, isGood);
        }

        private void waitMe(WaitHandle[] waitHandles, int stoppedThreadCounter)
        {
            while (Interlocked.CompareExchange(ref result, result, stoppedThreadCounter) != stoppedThreadCounter)
            {
                WaitHandle.WaitAny(waitHandles);
            }
        }
        private IEnumerable<Thread> createThreads(int threadNumber)
        {
            List<Thread> threads = new List<Thread>();
            for (int it = 0; it < threadNumber; ++it)
            {
                Thread t = new Thread(new ParameterizedThreadStart(DoSmth));
                threads.Add(t);
            }
            return threads;
        }

        private void RunThreads(IEnumerable<Thread> threads, WaitHandle[] waitHandles)
        {
            var en = waitHandles.GetEnumerator();
            foreach (var thread in threads)
            {
                en.MoveNext();
                thread.Start(en.Current);
            }
        }
        public void DoSmth(object state)
        {
            AutoResetEvent are = (AutoResetEvent)state;
            semaphore3.Acquire();
            Interlocked.Increment(ref result);
            are.Set();
        }

        private static WaitHandle[] createHandles(int threadNumber)
        {
            return Enumerable.Repeat<WaitHandle>(new AutoResetEvent(false), threadNumber).ToArray();
        }


        [TestMethod]
        public void AcquireTest()
        {
            enteranceCounter = 0;
            System.Threading.Thread t = new Thread(new ThreadStart(Do));
            System.Threading.Thread t2 = new Thread(new ThreadStart(Do));
            System.Threading.Thread t3 = new Thread(new ThreadStart(Do));
            t.Start();
            t2.Start();
            t3.Start();
            t.Join(timeWait);
            t2.Join(timeWait);
            t3.Join(timeWait);
            bool isGood = true;
            isGood &= (Interlocked.CompareExchange(ref enteranceCounter, 2, 2) == 2);
            semaphore.Release(1);
            t3.Join(timeWait);
            isGood &= (Interlocked.CompareExchange(ref enteranceCounter, 3, 3) == 3);
            semaphore.Release(2);
            Assert.AreEqual(true, isGood);
        }

        [TestMethod]
        public void TryAcquireTest()
        {
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
            Assert.AreEqual(true, isGood);
        }

        public void Do()
        {
            semaphore.Acquire();
            Interlocked.Increment(ref enteranceCounter);
        }
    }

    [TestClass]
    public class MySemaphoreTest : MyBaseSemaphoreTest
    {
        [TestInitialize]
        public void Setup()
        {
            this.semaphore = new MySemaphore(2);
            this.semaphore3 = new MySemaphore(3);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentOutOfRangeException))]
        public override void NegativeNumberOfthread()
        {
            ISemaphore sem = new MySemaphore(-1);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentOutOfRangeException))]
        public override void NullNumberOfthread()
        {
            ISemaphore sem = new MySemaphore(0);
        }
    }

    [TestClass]
    public class MyMonitorSemaphoreTest : MyBaseSemaphoreTest
    {
        [TestInitialize]
        public void Setup()
        {
            this.semaphore = new MyMonitorSemaphore(2);
            this.semaphore3 = new MyMonitorSemaphore(3);
    }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentOutOfRangeException))]
        public override void NegativeNumberOfthread()
        {
            ISemaphore sem = new MyMonitorSemaphore(-1);
        }

        [TestMethod]
        [ExpectedException(typeof(System.ArgumentOutOfRangeException))]
        public override void NullNumberOfthread()
        {
            ISemaphore sem = new MyMonitorSemaphore(0);
        }
    }
}