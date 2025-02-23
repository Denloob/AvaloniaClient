using System;

public class TimeCache<T>
{
    private readonly Action<T> updateFunction;
    private readonly TimeSpan cacheTime;
    private DateTime lastUpdateTime;
    private T cachedItem;
    private readonly object lockObject = new object();

    public TimeCache(Action<T> updateFunction, TimeSpan cacheTime, Func<T> factory)
    {
        this.updateFunction = updateFunction;
        this.cacheTime = cacheTime;
        this.lastUpdateTime = DateTime.MinValue;
        this.cachedItem = factory();
    }

    public T Item
    {
        get
        {
            lock (lockObject)
            {
                if ((DateTime.Now - this.lastUpdateTime) > this.cacheTime)
                {
                    this.updateFunction(this.cachedItem);
                    this.lastUpdateTime = DateTime.Now;
                }

                return this.cachedItem;
            }
        }
    }
}
