using SuperMetroid.Desktop;

internal static partial class Program
{
    private static void VerifyAudioEndpointRecovery()
    {
        long now = 0;
        int opens = 0;
        var first = new EndpointFixture();
        var replacement = new EndpointFixture();
        using var output = new RecoveringAudioOutput(() => ++opens switch
        {
            1 => first,
            2 => throw new WaveOutDeviceException(6, "waveOutOpen"),
            _ => replacement,
        }, () => now);
        output.Submit([1, 2]);
        first.Failure = new WaveOutDeviceException(6, "waveOutPrepareHeader");
        first.DisposeFailure = true;
        ExpectEndpointFailure(() => output.Submit([3, 4]));
        Check(first.Disposals == 1 && output.CanAcceptFrame,
            "lost endpoint must be retired once without blocking further emulation");
        for (int frame = 0; frame < 60; frame++) output.Submit([5, 6]);
        Check(opens == 1, "lost output must not reopen on every emulated frame");
        now = 1000;
        ExpectEndpointFailure(() => output.Submit([7, 8]));
        Check(opens == 2, "missing endpoint must receive one timed retry");
        now = 1999;
        output.Submit([9, 10]);
        Check(opens == 2, "failed reopen must restart the retry interval");
        now = 2000;
        output.Submit([11, 12]);
        Check(opens == 3 && first.Frames.Single().SequenceEqual(new short[] { 1, 2 }) &&
            replacement.Frames.Single().SequenceEqual(new short[] { 11, 12 }),
            "reconnection must submit current PCM, never duplicate or replay outage frames");
        replacement.Accepting = false;
        Check(!output.CanAcceptFrame, "healthy output must retain bounded queue backpressure");
        replacement.Accepting = true;
        output.Reset();
        Check(replacement.Resets == 1, "normal pause must reset the current output");
        replacement.Failure = new WaveOutDeviceException(5, "waveOutReset");
        output.Reset();
        Check(replacement.Disposals == 1 && output.CanAcceptFrame,
            "endpoint loss while pausing must retire the device without a fatal reset error");

        var unrelated = new EndpointFixture { Failure = new WaveOutDeviceException(7, "waveOutPrepareHeader") };
        using var strict = new RecoveringAudioOutput(() => unrelated, () => now);
        bool propagated = false;
        try { strict.Submit([13, 14]); }
        catch (WaveOutDeviceException error) when (!error.EndpointUnavailable) { propagated = true; }
        Check(propagated && unrelated.Disposals == 0,
            "unexpected device errors must remain visible rather than being classified as endpoint loss");
        WaveOutAudioDevice? native = null;
        using (var live = new RecoveringAudioOutput(() => native = new WaveOutAudioDevice(48000, 2, 1600, volumePercent: 0)))
        {
            live.Submit(new short[1600]);
            native!.WaitForPendingSubmissions();
            live.Reset();
            live.Submit(new short[1600]);
            native.WaitForPendingSubmissions();
            Check(native.PreparedBufferCountForVerification == WaveOutAudioPolicy.PrerollSilenceBufferCount + 1,
                "recovery wrapper must preserve native startup and pause-reset preroll");
        }
        Console.WriteLine("Audio endpoint recovery: reported driver loss, cleanup failure, bounded retry, current PCM on reconnect, queue pacing and pause loss pass.");
    }

    private static void ExpectEndpointFailure(Action action)
    {
        bool failed = false;
        try { action(); }
        catch (WaveOutDeviceException error) when (error.EndpointUnavailable) { failed = true; }
        Check(failed, "original endpoint error must remain reportable");
    }

    private sealed class EndpointFixture : IHostAudioOutput
    {
        internal readonly List<short[]> Frames = [];
        internal Exception? Failure;
        internal bool DisposeFailure;
        internal bool Accepting = true;
        internal int Disposals, Resets;
        public bool CanAcceptFrame => Accepting;
        public WaveOutQueueHealthSnapshot QueueHealth => new(0, 0, null, null, 0);
        public void Submit(ReadOnlySpan<short> samples)
        {
            if (Failure is not null) throw Failure;
            Frames.Add(samples.ToArray());
        }
        public void Reset()
        {
            if (Failure is not null) throw Failure;
            Resets++;
        }
        public void Dispose()
        {
            Disposals++;
            if (DisposeFailure) throw new AggregateException("Previously reported worker failure", Failure!);
        }
    }
}