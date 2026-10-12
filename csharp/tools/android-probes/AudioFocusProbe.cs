using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;

namespace SuperMetroid.Android;

/// <summary>
/// Opt-in diagnostic build only. Creates a second real AudioManager client while
/// the game Activity retains window focus; never plays audio or accesses a microphone.
/// </summary>
internal sealed class AudioFocusProbe(Activity activity, Func<AudioFocusProbe.HostState> host)
{
    /// <summary>The Activity's focus, lifecycle and audio services at the moment the probe reads them.</summary>
    internal readonly record struct HostState(bool Focused, bool Resumed, bool CanRun, AudioManager? AudioManager, Handler? Handler);

    private bool focusProbeActive;

    /// <summary>Logs one audio-focus callback the game's own focus listener received.</summary>
    internal void Observe(string change)
    {
        HostState state = host();
        Log($"callback={change} focused={state.Focused} resumed={state.Resumed} canRun={state.CanRun}");
    }

    /// <summary>Starts a probe when the single-top intent names one.</summary>
    internal void OnNewIntent(Intent? intent)
    {
        if (intent?.GetStringExtra("audio-focus-probe") is not { } mode) return;
        // Delivering a single-top intent temporarily pauses the Activity. Let its
        // normal resume/window-focus callbacks finish before testing audio alone.
        Handler handler = host().Handler ?? throw new InvalidOperationException("Audio probe requires the host handler.");
        handler.PostDelayed(() => Run(mode), 750);
    }

    private void Log(string text) =>
        File.AppendAllText(Path.Combine(activity.FilesDir!.AbsolutePath, "audio-focus-probe.log"), $"{DateTimeOffset.UtcNow:O} {text}\n");

    private void Run(string mode)
    {
        HostState state = host();
        if (focusProbeActive || state.AudioManager is not { } audioManager || state.Handler is not { } hostHandler || !state.CanRun)
            throw new InvalidOperationException("Audio probe requires an active foreground game and no existing probe.");
        AudioFocus gain = mode switch
        {
            "transient" => AudioFocus.GainTransient,
            "duck" => AudioFocus.GainTransientMayDuck,
            _ => throw new ArgumentException("Expected transient or duck audio-focus probe.")
        };
        var listener = new ProbeFocusListener();
        using var attributes = new AudioAttributes.Builder().SetUsage(AudioUsageKind.Game)!
            .SetContentType(AudioContentType.Music)!.Build()!;
        using var builder = new AudioFocusRequestClass.Builder(gain);
        var request = builder.SetAudioAttributes(attributes)!
            .SetOnAudioFocusChangeListener(listener, hostHandler)!.Build()!;
        focusProbeActive = true;
        var result = audioManager.RequestAudioFocus(request);
        Log($"requested={mode} result={result} focused={state.Focused} resumed={state.Resumed}");
        string timingLog = Path.Combine(activity.FilesDir!.AbsolutePath, "timing.log");
        string? heldTiming = null;
        hostHandler.PostDelayed(() => heldTiming = File.ReadLines(timingLog).Last(), 1000);
        hostHandler.PostDelayed(() => Log($"heldTimingUnchanged={heldTiming == File.ReadLines(timingLog).Last()} canRun={host().CanRun}"), 2500);
        hostHandler.PostDelayed(() =>
        {
            HostState released = host();
            Log($"release={mode} focused={released.Focused} resumed={released.Resumed} canRun={released.CanRun}");
            audioManager.AbandonAudioFocusRequest(request);
            request.Dispose();
            listener.Dispose();
            focusProbeActive = false;
        }, 3000);
    }

    private sealed class ProbeFocusListener : Java.Lang.Object, AudioManager.IOnAudioFocusChangeListener
    {
        public void OnAudioFocusChange(AudioFocus focusChange) =>
            global::Android.Util.Log.Info("SuperMetroid", $"PROBE secondary focus={focusChange}");
    }
}
