using System.Collections.Immutable;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Reflection;
using System.Threading.Channels;

namespace TicketBurst.ServiceInfra;

public class DevboxClusterOrchestrator : IAsyncDisposable
{
    private static readonly string __executablePath = Path.Combine(
        Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!,
        $"reservationd{(OperatingSystem.IsWindows() ? ".exe" : "")}");
    private static readonly string __memberListFilePath = Path.Combine(
        Path.GetDirectoryName(Assembly.GetEntryAssembly()!.Location)!,
        $"devbox-members.txt");
    
    private readonly int _startPortNumber;
    private readonly Channel<Action> _requestQueue = Channel.CreateBounded<Action>(capacity: 100);
    private readonly CancellationTokenSource _loopCancellation = new CancellationTokenSource();
    private readonly DevboxClusterInfoProvider _infoProvider;
    private ImmutableDictionary<int, Process> _processByMemberIndex;
    private readonly Task _controlLoopTask;

    public DevboxClusterOrchestrator(int startPortNumber, DevboxClusterInfoProvider infoProvider)
    {
        Console.WriteLine($"DevboxClusterOrchestrator> starting port [{startPortNumber}], executable=[{__executablePath}]");
        
        _startPortNumber = startPortNumber;
        _infoProvider = infoProvider;
        _processByMemberIndex = ImmutableDictionary<int, Process>.Empty.Add(0, Process.GetCurrentProcess());
        _controlLoopTask = RunControlLoop();
        
        NotifyChange();
    }

    public void RequestScale(int desiredCount)
    {
        if (!_requestQueue.Writer.TryWrite(() => FulfillScalingRequest(desiredCount)))
        {
            throw new Exception("Cannot submit scale request");
        }
    }

    public async ValueTask DisposeAsync()
    {
        _requestQueue.Writer.Complete();
        _loopCancellation.Cancel();
        await _controlLoopTask;
    }

    private async Task RunControlLoop()
    {
        await foreach (var action in _requestQueue.Reader.ReadAllAsync(_loopCancellation.Token))
        {
            try
            {
                action();
            }
            catch (Exception e)
            {
                Console.WriteLine($"DevboxClusterOrchestrator request FAILED! {e.ToString()}");
            }
        }
    }

    private void FulfillScalingRequest(int desiredCount)
    {
        while (desiredCount > _processByMemberIndex.Count)
        {
            ScaleOutOne();
        }
        
        while (desiredCount < _processByMemberIndex.Count)
        {
            ScaleInOne();
        }

        void ScaleOutOne()
        {
            var newMemberIndex = _processByMemberIndex.Count;
            var newProcess = SpawnNewProcess(newMemberIndex);

            _processByMemberIndex = _processByMemberIndex.Add(newMemberIndex, newProcess);
            NotifyChange();
        }

        void ScaleInOne()
        {
            if (_processByMemberIndex.Count <= 1)
            {
                throw new InvalidOperationException("Scaling in to 0 is not supported");
            }

            var memberIndex = _processByMemberIndex.Count - 1;
            try
            {
                KillProcess(_processByMemberIndex[memberIndex]);
            }
            finally
            {
                _processByMemberIndex = _processByMemberIndex.Remove(memberIndex);
                NotifyChange();
            }
        }

        Process SpawnNewProcess(int memberIndex)
        {
            var httpPort = 3010 + memberIndex;
            var actorPort = _startPortNumber + memberIndex;
            
            var psi = new ProcessStartInfo(
                fileName: __executablePath, 
                arguments: $"--mock-db --http-port {httpPort} --actor-port {actorPort} --member-index {memberIndex}");
            
            psi.UseShellExecute = true;
            var process = Process.Start(psi);

            if (process == null)
            {
                throw new Exception("Could not start new process!");
            }
            
            return process;
        }

        void KillProcess(Process process)
        {
            process.CloseMainWindow();
            if (!process.WaitForExit(3000))
            {
                process.Kill();
            }
        }
    }

    private void NotifyChange()
    {
        var newPortNumbers = GetPortNumberArray();
        _infoProvider.InjectChange(newPortNumbers);

        try
        {
            File.WriteAllLines(MemberListFilePath, newPortNumbers.Select(p => p.ToString()));
        }
        catch (Exception e)
        {
            Console.WriteLine($"DevboxClusterOrchestrator> Failed to write member-list.txt: {e.Message}");
        }

        int[] GetPortNumberArray() => _processByMemberIndex.Keys
            .Select(key => _startPortNumber + key)
            .ToArray();
    }

    public static string MemberListFilePath => __memberListFilePath;
}
