using Android.Content;
using Android.Media;

namespace SuperMetroid.Android;

/// <summary>
/// Opt-in diagnostic build only. Creates a second real AudioManager client while
/// the game Activity retains window focus; never plays audio or accesses a microphone.
/// </summary>
public sealed partial class MainActivity
{
    private bool focusProbeActive;
    partial void ObserveAudioFocus(string change) => ProbeLog($"callback={change} focused={focused} resumed={resumed} canRun={CanRun}");
    private void ProbeLog(string text) => File.AppendAllText(Path.Combine(FilesDir!.AbsolutePath, "audio-focus-probe.log"), $"{DateTimeOffset.UtcNow:O} {text}\n");

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        if (intent?.GetStringExtra("audio-focus-probe") is not { } mode) return;
        // Delivering a single-top intent temporarily pauses the Activity. Let its
        // normal resume/window-focus callbacks finish before testing audio alone.
        hostHandler!.PostDelayed(() => RunAudioFocusProbe(mode), 750);
    }

    private void RunAudioFocusProbe(string mode)
    {
        if (focusProbeActive || audioManager is null || hostHandler is null || !CanRun)
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
        ProbeLog($"requested={mode} result={result} focused={focused} resumed={resumed}");
        string? heldTiming = null;
        hostHandler.PostDelayed(() => heldTiming = File.ReadLines(Path.Combine(FilesDir!.AbsolutePath, "timing.log")).Last(), 1000);
        hostHandler.PostDelayed(() => ProbeLog($"heldTimingUnchanged={heldTiming == File.ReadLines(Path.Combine(FilesDir!.AbsolutePath, "timing.log")).Last()} canRun={CanRun}"), 2500);
        hostHandler.PostDelayed(() =>
        {
            ProbeLog($"release={mode} focused={focused} resumed={resumed} canRun={CanRun}");
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
