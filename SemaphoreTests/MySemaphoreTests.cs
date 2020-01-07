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
        private int enteranceCounter = 0;
        private static int timeWait = 500;

        [TestMethod]
        [ExpectedException(typeof(System.Threading.SemaphoreFullException))]
        public void Withdraw_AmountMoreThanBalance_Throws()
        {
            semaphore.Release(1);
        }

        [TestMethod()]
        public void AcquireTest()
        {
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
        }
    }

    [TestClass]
    public class MyMonitorSemaphoreTest : MyBaseSemaphoreTest
    {
        [TestInitialize]
        public void Setup()
        {
            this.semaphore = new MyMonitorSemaphore(2);
        }
    }
    /*
    [TestClass()]
    public class MySemaphoreTests
    {
        private MySemaphore mySemaphore;
        private int enteranceCounter = 0;
        private static int timeWait = 500;

        [TestMethod]
        [ExpectedException(typeof(System.Threading.SemaphoreFullException))]
        public void Withdraw_AmountMoreThanBalance_Throws()
        {
            mySemaphore = new MySemaphore(2);

            mySemaphore.Release(1);
        }

        [TestMethod()]
        public void AcquireTest()
        {
            mySemaphore = new MySemaphore(2);
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
            mySemaphore.Release(1);
            t3.Join(timeWait);
            isGood &= (Interlocked.CompareExchange(ref enteranceCounter, 3, 3) == 3);
            Assert.AreEqual(true, isGood);
        }

        public void Do()
        {
            mySemaphore.Acquire();
            Interlocked.Increment(ref enteranceCounter);
        }
    }
     */
}