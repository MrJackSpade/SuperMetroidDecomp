using SuperMetroid.Desktop;

internal static partial class Program
{
    private static void VerifyAudioQueueBackpressure()
    {
        using var device = new WaveOutAudioDevice(48000, 2, 1600, volumePercent: 0);
        var silence = new short[1600];
        object gate = typeof(WaveOutAudioDevice).GetField("deviceGate",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(device)!;
        int polls = 0, advances = 0;
        lock (gate)
        {
            // Hold the real device owner while filling the actual bounded producer queue.
            // The worker may take one block, but cannot submit it until this gate opens.
            device.Submit(silence);
            Check(SpinWait.SpinUntil(() => device.QueueHealth.ManagedQueued == 0, 1000),
                "audio worker must remove its first block before the queue is filled");
            while (device.CanAcceptFrame)
                device.Submit(silence);
            var stalled = PlaybackFrameBatch.Run(4, () => { polls++; return 0; }, _ =>
            {
                advances++;
                device.Submit(silence);
                return default(SuperMetroid.Core.Frontend.FrontendFrame);
            }, () => device.CanAcceptFrame);
            Check(stalled.CompletedFrames == 0 && polls == 0 && advances == 0,
                "full audio queue must defer before polling input or advancing gameplay");
        }
        device.WaitForPendingSubmissions();
        var resumed = PlaybackFrameBatch.Run(4, () => { polls++; return 0; }, _ =>
        {
            advances++;
            device.Submit(silence);
            return default(SuperMetroid.Core.Frontend.FrontendFrame);
        }, () => device.CanAcceptFrame);
        device.WaitForPendingSubmissions();
        Check(resumed.CompletedFrames == 4 && polls == 4 && advances == 4,
            "returning device ownership must resume all deferred gameplay/audio in order");
        Console.WriteLine("Audio backpressure: full real queue defers input/gameplay, then resumes without dropping PCM.");
    }

    private static async Task VerifyNativeAudioQueueHealth()
    {
        using var device = new WaveOutAudioDevice(48000, 2, 1600, volumePercent: 0);
        var silence = new short[1600];
        device.Submit(silence);
        device.WaitForPendingSubmissions();
        Check(device.QueueHealth.EmptyBeforeRefill == 0, "native startup must not count as starvation");
        long deadline = Environment.TickCount64 + 5000;
        while (device.NativeQueuedBufferCountForVerification != 0)
        {
            if (Environment.TickCount64 >= deadline) throw new TimeoutException("waveOut did not drain test silence.");
            await Task.Delay(10);
        }
        device.Submit(silence);
        device.WaitForPendingSubmissions();
        Check(device.QueueHealth.EmptyBeforeRefill == 1, "native drain before refill must be observed");
        device.Reset();
        device.Submit(silence);
        device.WaitForPendingSubmissions();
        Check(device.QueueHealth.EmptyBeforeRefill == 1, "explicit native reset must not add starvation");
        Console.WriteLine("Native waveOut: silent startup, observed drain/refill, and reset exclusion passed.");
    }

    private static void VerifyAudioQueueHealth()
    {
        var health = new WaveOutQueueHealth();
        health.Observe(0, prerollRequired: true);
        Check(health.Snapshot(0).MinimumNativeQueued is null, "startup must not invent zero occupancy history");
        health.Observe(3, prerollRequired: false);
        health.Observe(1, prerollRequired: false);
        health.Observe(0, prerollRequired: false);
        health.Observe(0, prerollRequired: true); // An explicit pause/reset is not starvation.
        health.Observe(4, prerollRequired: false);
        var snapshot = health.Snapshot(2);
        Check(snapshot.RefillObservations == 4 && snapshot.EmptyBeforeRefill == 1 &&
            snapshot.MinimumNativeQueued == 0 && snapshot.MaximumNativeQueued == 4 && snapshot.ManagedQueued == 2,
            "audio occupancy/drain counters must exclude startup/reset without suppressing later starvation");
        Console.WriteLine("Audio queue health: startup/reset exclusion, genuine empty refill and occupancy history passed.");
    }
}
