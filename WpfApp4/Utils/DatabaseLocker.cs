using System;

namespace WpfApp4.Utils
{
    public static class DatabaseLocker
    {
        public static readonly object LockObject = new object();

        public static void ExecuteWithLock(Action action)
        {
            lock (LockObject)
            {
                action();
            }
        }

        public static T ExecuteWithLock<T>(Func<T> function)
        {
            lock (LockObject)
            {
                return function();
            }
        }
    }
}