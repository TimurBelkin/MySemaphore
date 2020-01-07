namespace Semaphore
{
    /// <summary>
    /// Implements ISemaphore, based on the System.Threading.Semaphore class
    /// </summary>
    public class MySemaphore : ISemaphore
    {
        private readonly System.Threading.Semaphore m_Semaphore;
        
        /// <summary>
        /// Creates new instance of MySemaphore
        /// </summary>
        /// <param name="count">Initial number of entries and the maximum number of concurrent entries</param>
        public MySemaphore(int count)
        {
            m_Semaphore = new System.Threading.Semaphore(count, count);
        }
        
        /// <summary>
        /// Aquires the semaphore (if it has been already released) or blocks the current thread until the semaphore would be released
        /// </summary>
        public void Acquire()
        {
            m_Semaphore.WaitOne();
        }
        
        /// <summary>
        /// Tries to acquire the semaphore or returns immediately if semaphore could not be acquired
        /// </summary>
        /// <returns>True if acquiring the semaphore is successful, otherwise - false</returns>
        public bool TryAcquire()
        {
            return m_Semaphore.WaitOne(1);
        }

        /// <summary>
        /// Releases the semaphore a specified number of times
        /// </summary>
        /// <param name="releaseCount">The number of times to exit the semaphore.</param>
        /// <returns>The count on the semaphore before the Release method was called.</returns>
        public int Release(int releaseCount)
        {
            return m_Semaphore.Release(releaseCount);
        }
    }
}