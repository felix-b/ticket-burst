using System.Threading.Channels;

namespace TicketBurst.ServiceInfra;

public abstract class InProcessJob<TWorkItem> : IDisposable
{
    private readonly Func<TWorkItem, Task> _doWork;
    private readonly string _name;
    private readonly CancellationTokenSource _cancellation = new CancellationTokenSource();
    private readonly Channel<TWorkItem> _workerQueue;
    private readonly Task _workerTask;

    protected InProcessJob(string name, int boundCapacity, Func<TWorkItem, Task> doWork)
    {
        _name = name;
        _doWork = doWork;

        _workerQueue = Channel.CreateBounded<TWorkItem>(
            options: new BoundedChannelOptions(boundCapacity),
            itemDropped: (workItem) => {
                Console.WriteLine($"{_name}: ERROR, work item dropped [{workItem}]");
            });
        
        _workerTask = RunWorkerLoop();
    }

    public void Dispose()
    {
        _cancellation.Cancel();
        
        if (!_workerTask.Wait(TimeSpan.FromSeconds(10)))
        {
            Console.WriteLine($"{_name}: ERROR, worker loop failed to stop in timely fashion");
        }
    }

    protected void EnqueueWorkItem(TWorkItem workItem)
    {
        if (!_workerQueue.Writer.TryWrite(workItem))
        {
            Console.WriteLine($"{_name}: ERROR, failed to enqueue work item [{workItem}]");
        }
    }

    private async Task RunWorkerLoop()
    {
        Console.WriteLine($"{_name}: JOB STARTING");

        try
        {
            await foreach (var workItem in _workerQueue.Reader.ReadAllAsync(_cancellation.Token))
            {
                Console.WriteLine($"{_name}: work item [{workItem}]");

                try
                {
                    await _doWork(workItem);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{_name}: WORK ITEM FAILED! [{workItem}], {e}");
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine($"{_name}: JOB STOPPING upon request");
        }
        catch (Exception e)
        {        
            Console.WriteLine($"{_name}: JOB WORKER LOOP CRASHED! {e}");
        }

        Console.WriteLine($"{_name}: JOB STOPPED");
    }
}
