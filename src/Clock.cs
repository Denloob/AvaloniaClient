using System;
using System.ComponentModel;

using Avalonia.Threading;

public class Clock
{
    private DispatcherTimer timer = new DispatcherTimer();

    private Action action;
    private int intervalSec;

    private DateTime nextCycle;

    public Clock(int intervalSec, Action action)
    {
        this.action = action;
        this.intervalSec = intervalSec;

        this.nextCycle = DateTime.Now.AddSeconds(this.intervalSec);

        this.action();

        this.timer.Interval = TimeSpan.FromSeconds(this.intervalSec);
        this.timer.Tick += Tick;
        this.timer.Start();
    }

    public void Terminate()
    {
        this.timer.Stop();
        this.nextCycle = DateTime.MinValue; // No more cycles.
    }

    public bool IsTerminated()
    {
        return this.nextCycle == DateTime.MinValue;
    }

    private void Tick(object? sender, EventArgs e)
    {
        if (DateTime.Now < this.nextCycle && !IsTerminated())
        {
            return;
        }

        this.nextCycle = DateTime.Now.AddSeconds(this.intervalSec);
        this.action();
    }
}
