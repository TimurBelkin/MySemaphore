namespace Semaphore
{
    /// <summary>
    /// Semaphore, which provides basic syncronization methods
    /// </summary>
    public interface ISemaphore
    {
        /// <summary>
        /// Aquires the semaphore (if it has been already released) or blocks the current thread until the semaphore would be released
        /// </summary>
        void Acquire();
        
        /// <summary>
        /// Tries to acquire the semaphore or returns immediately if semaphore could not be acquired
        /// </summary>
        /// <returns>True if acquiring the semaphore is successful, otherwise - false</returns>
        bool TryAcquire();
        
        /// <summary>
        /// Releases the semaphore a specified number of times
        /// </summary>
        /// <param name="releaseCount">The number of times to exit the semaphore.</param>
        /// <returns>The count on the semaphore before the Release method was called.</returns>
        int Release(int releaseCount);
    }
}