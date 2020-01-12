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
        private static readonly int MAX_WAIT_TIME = 50;

        protected ISemaphore TwoPlaceSemaphore;
        protected ISemaphore ThreePlaceSemaphore;
        private int EnteranceCounter = 0;

        protected const int TIME_LIMIT_FOR_TEST = 2000;

        /// <summary>
        /// Simple clear tests for two placed Semaphore
        /// </summary>
        /// 
        [TestMethod, Timeout(TIME_LIMIT_FOR_TEST)]
        [ExpectedException(typeof(System.Threading.SemaphoreFullException))]
        public void Release_Throws()
        {
            TwoPlaceSemaphore.Release(1);
        }

        [TestMethod, Timeout(TIME_LIMIT_FOR_TEST)]
        [ExpectedException(typeof(System.ArgumentOutOfRangeException))]
        public abstract void NegativeNumberOfthread();

        [TestMethod, Timeout(TIME_LIMIT_FOR_TEST)]
        [ExpectedException(typeof(System.ArgumentOutOfRangeException))]
        public abstract void NullNumberOfthread();

        [TestMethod, Timeout(TIME_LIMIT_FOR_TEST)]
        [ExpectedException(typeof(System.ArgumentOutOfRangeException))]
        public void NegativeReleaseNumber()
        {
            Thread thread = new Thread(new ThreadStart(IncrementCounter));
            thread.Start();
            thread.Join(MAX_WAIT_TIME);
            TwoPlaceSemaphore.Release(-1);
        }

        [TestMethod, Timeout(TIME_LIMIT_FOR_TEST)]
        [ExpectedException(typeof(System.ArgumentOutOfRangeException))]
        public void NullReleaseNumber()
        {
            Thread thread = new Thread(new ThreadStart(IncrementCounter));
            thread.Start();
            thread.Join(MAX_WAIT_TIME);
            TwoPlaceSemaphore.Release(0);
        }

        [TestMethod, Timeout(TIME_LIMIT_FOR_TEST)]
        public void AcquireTest()
        {
            EnteranceCounter = 0;
            Thread thread = new Thread(new ThreadStart(IncrementCounter));
            Thread secondThread = new Thread(new ThreadStart(IncrementCounter));
            Thread thirdThread = new Thread(new ThreadStart(IncrementCounter));

            thread.Start();
            secondThread.Start();
            thirdThread.Start();

            thread.Join(MAX_WAIT_TIME);
            secondThread.Join(MAX_WAIT_TIME);
            thirdThread.Join(MAX_WAIT_TIME);

            bool isOK = true;
            isOK &= (Interlocked.CompareExchange(ref EnteranceCounter, 2, 2) == 2);

            TwoPlaceSemaphore.Release(1);
            thirdThread.Join(MAX_WAIT_TIME);
            isOK &= (Interlocked.CompareExchange(ref EnteranceCounter, 3, 3) == 3);

            TwoPlaceSemaphore.Release(2);

            Assert.AreEqual(true, isOK);
        }

        [TestMethod, Timeout(TIME_LIMIT_FOR_TEST)]
        public void TryAcquireTest()
        {
            Thread thread = new Thread(new ThreadStart(IncrementCounter));
            Thread secondThread = new Thread(new ThreadStart(IncrementCounter));
            Thread thirdThread = new Thread(new ThreadStart(IncrementCounter));

            bool isOK = true;

            isOK &= TwoPlaceSemaphore.TryAcquire();
            TwoPlaceSemaphore.Release(1);

            thread.Start();
            thread.Join(MAX_WAIT_TIME);  // 1 place from 2 is occupied

            isOK &= TwoPlaceSemaphore.TryAcquire(); // 2 places from 2 are occupied

            isOK &= !TwoPlaceSemaphore.TryAcquire(); // no entrance, all places are occupied

            TwoPlaceSemaphore.Release(1); // 1 place from 2 is occupied

            secondThread.Start();
            secondThread.Join(MAX_WAIT_TIME);  // 2 places from 2 is occupied

            isOK &= !TwoPlaceSemaphore.TryAcquire(); // no entrance, all places are occupied

            thirdThread.Start();
            thirdThread.Join(MAX_WAIT_TIME); // third thread tries to enter.

            isOK &= !TwoPlaceSemaphore.TryAcquire(); // no entrance, all places are occupied third thread is in queue

            TwoPlaceSemaphore.Release(1);
            thirdThread.Join(MAX_WAIT_TIME);  // 2nd thread out, 3d in. 2 places from 2 is occupied

            isOK &= !TwoPlaceSemaphore.TryAcquire(); // no entrance, all places are occupied

            TwoPlaceSemaphore.Release(1); // 1 place from 2 is occupied
            isOK &= TwoPlaceSemaphore.TryAcquire();  // 2 place from 2 is occupied
            
            TwoPlaceSemaphore.Release(2); 

            Assert.AreEqual(true, isOK);
        }

        /// <summary>
        /// More complicated tests 3 placed semaphore
        /// </summary>
        /// 
        [TestMethod, Timeout(TIME_LIMIT_FOR_TEST)]
        public void AcquireTestThread()
        {
            EnteranceCounter = 0;
            int threadNumber = 10;
            var threads = CreateThreads(threadNumber, IncrementCounterWithNotice);
            var waitHandles = createHandles(threadNumber);
            RunThreads(threads, waitHandles);
            bool isOK = true;
            
            WaitSomeThreadsToFinish(waitHandles, 3);
            isOK &= (Interlocked.CompareExchange(ref EnteranceCounter, EnteranceCounter, 3) == 3);

            ThreePlaceSemaphore.Release(1);
            WaitSomeThreadsToFinish(waitHandles, 4);
            isOK &= (Interlocked.CompareExchange(ref EnteranceCounter, EnteranceCounter, 4) == 4);

            ThreePlaceSemaphore.Release(3);
            WaitSomeThreadsToFinish(waitHandles, 7);
            isOK &= (Interlocked.CompareExchange(ref EnteranceCounter, EnteranceCounter, 7) == 7);

            ThreePlaceSemaphore.Release(2);
            WaitSomeThreadsToFinish(waitHandles, 9);
            isOK &= (Interlocked.CompareExchange(ref EnteranceCounter, EnteranceCounter, 9) == 9);

            ThreePlaceSemaphore.Release(1);
            WaitSomeThreadsToFinish(waitHandles, 10);
            isOK &= (Interlocked.CompareExchange(ref EnteranceCounter, EnteranceCounter, 10) == 10);

            ThreePlaceSemaphore.Release(3);

            Assert.AreEqual(true, isOK);
        }

        [TestMethod, Timeout(TIME_LIMIT_FOR_TEST)]
        public void AutoAcquireTestThread()
        {
            EnteranceCounter = 0;
            int threadNumber = 63; // limited by autoWaitHandle maximum  number (64)
            int semaphoreCapacity = 3;
            var threads = CreateThreads(threadNumber, IncrementCounterWithNotice);
            var waitHandles = createHandles(threadNumber);
            RunThreads(threads, waitHandles);
            bool isOK = true;

            WaitSomeThreadsToFinish(waitHandles, semaphoreCapacity);
            isOK &= (Interlocked.CompareExchange(ref EnteranceCounter, EnteranceCounter, semaphoreCapacity) == semaphoreCapacity);

            int releaseCounter = 0;
            for (int it = semaphoreCapacity * 2; it <= threadNumber; it += semaphoreCapacity)
            {
                ThreePlaceSemaphore.Release(semaphoreCapacity);
                releaseCounter += semaphoreCapacity;
                WaitSomeThreadsToFinish(waitHandles, it);
                isOK &= (Interlocked.CompareExchange(ref EnteranceCounter, EnteranceCounter, it) == it);
            }

            ThreePlaceSemaphore.Release(threadNumber - releaseCounter);

            Assert.AreEqual(true, isOK);
        }

        private void WaitSomeThreadsToFinish(WaitHandle[] waitHandles, int numberThreadsToWait)
        {
            while (Interlocked.CompareExchange(ref EnteranceCounter, EnteranceCounter, numberThreadsToWait) != numberThreadsToWait)
            {
                WaitHandle.WaitAny(waitHandles);
            }
        }
        private IEnumerable<Thread> CreateThreads(int threadNumber, Action<object> action)
        {
            List<Thread> threads = new List<Thread>();
            for (int it = 0; it < threadNumber; ++it)
            {
                Thread thread = new Thread(new ParameterizedThreadStart(action));
                threads.Add(thread);
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
        public void IncrementCounterWithNotice(object state)
        {
            AutoResetEvent are = (AutoResetEvent)state;
            ThreePlaceSemaphore.Acquire();
            Interlocked.Increment(ref EnteranceCounter);
            are.Set();
        }

        private static WaitHandle[] createHandles(int threadNumber)
        {
            return Enumerable.Repeat<WaitHandle>(new AutoResetEvent(false), threadNumber).ToArray();
        }

        public void IncrementCounter()
        {
            TwoPlaceSemaphore.Acquire();
            Interlocked.Increment(ref EnteranceCounter);
        }
    }
     
    [TestClass]
    public class MyMonitorSemaphoreTest : MyBaseSemaphoreTest
    {
        [TestInitialize]
        public void Setup()
        {
            this.TwoPlaceSemaphore = new MyMonitorSemaphore(2);
            this.ThreePlaceSemaphore = new MyMonitorSemaphore(3);
    }

        [TestMethod, Timeout(TIME_LIMIT_FOR_TEST)]
        [ExpectedException(typeof(System.ArgumentOutOfRangeException))]
        public override void NegativeNumberOfthread()
        {
            ISemaphore myMonitorSemaphore = new MyMonitorSemaphore(-1);
        }

        [TestMethod, Timeout(TIME_LIMIT_FOR_TEST)]
        [ExpectedException(typeof(System.ArgumentOutOfRangeException))]
        public override void NullNumberOfthread()
        {
            ISemaphore myMonitoSemaphore = new MyMonitorSemaphore(0);
        }
    }

    [TestClass]
    public class MySemaphoreTest : MyBaseSemaphoreTest
    {
        [TestInitialize]
        public void Setup()
        {
            this.TwoPlaceSemaphore = new MySemaphore(2);
            this.ThreePlaceSemaphore = new MySemaphore(3);
        }

        [TestMethod, Timeout(TIME_LIMIT_FOR_TEST)]
        [ExpectedException(typeof(System.ArgumentOutOfRangeException))]
        public override void NegativeNumberOfthread()
        {
            ISemaphore mySemaphore = new MySemaphore(-1);
        }

        [TestMethod, Timeout(TIME_LIMIT_FOR_TEST)]
        [ExpectedException(typeof(System.ArgumentOutOfRangeException))]
        public override void NullNumberOfthread()
        {
            ISemaphore mySemaphore = new MySemaphore(0);
        }
    }
}