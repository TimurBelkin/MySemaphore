using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Semaphore
{
    public class MyMonitorSemaphore : ISemaphore
    {
        private readonly int threadCount;
        private int currentCount;
        private object locker;

        /// <summary>
        /// Creates new instance of MySemaphore
        /// </summary>
        /// <param name="count">Initial number of entries and the maximum number of concurrent entries</param>
        public MyMonitorSemaphore(int count)
        {
            if (count <= 0)
            {
                throw new System.ArgumentOutOfRangeException();
            }
            threadCount = count;
            currentCount = 0;
            locker = new object();
        }

        /// <summary>
        /// Aquires the semaphore (if it has been already released) or blocks the current thread until the semaphore would be released
        /// </summary>
        public void Acquire()
        {
            lock(locker)
            {
                if (currentCount >= threadCount)
                {
                    Monitor.Wait(locker);
                }
                ++currentCount;
            }
        }

        /// <summary>
        /// Tries to acquire the semaphore or returns immediately if semaphore could not be acquired
        /// </summary>
        /// <returns>True if acquiring the semaphore is successful, otherwise - false</returns>
        public bool TryAcquire()
        {
            lock (locker)
            {
                bool isEntered = false;
                if (currentCount < threadCount)
                {
                    ++currentCount;
                    isEntered = true;
                }
                Monitor.PulseAll(locker);
                return isEntered;
            }
        }

        /// <summary>
        /// Releases the semaphore a specified number of times
        /// </summary>
        /// <param name="releaseCount">The number of times to exit the semaphore.</param>
        /// <returns>The count on the semaphore before the Release method was called.</returns>
        public int Release(int releaseCount)
        {
            lock (locker)
            {
                if (releaseCount <= 0)
                {
                    throw new System.ArgumentOutOfRangeException();
                }
                int previous = currentCount;
                if (releaseCount > currentCount)
                {
                    throw new System.Threading.SemaphoreFullException();
                }
                currentCount -= releaseCount;
                while (releaseCount-- > 0)
                {
                    Monitor.Pulse(locker);
                }
                return previous;
            }
        }
    }
}
