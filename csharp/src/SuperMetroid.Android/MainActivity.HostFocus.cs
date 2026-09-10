using Android.Hardware.Input;
using Android.Media;
using Android.OS;

namespace SuperMetroid.Android;

public sealed partial class MainActivity
{
    private AudioManager? audioManager;
    private AudioFocusRequestClass? audioFocusRequest;
    private HostFocusListener? hostFocusListener;
    private InputManager? inputManager;
    private Handler? hostHandler;
    private bool audioFocusRequested;
    private bool audioFocusGranted;
    // Compiled away in normal APKs; implemented only by opt-in device probes.
    partial void ObserveAudioFocus(string change);

    private bool AudioEnabled => session?.EffectiveOptions?.AudioEnabled ?? true;
    private bool CanRun => AndroidRunPolicy.CanRun(resumed, focused, menuOpen, destroyed,
        AudioEnabled, audioFocusGranted);

    private void InitializeHostFocus()
    {
        audioManager = (AudioManager?)GetSystemService(AudioService)
            ?? throw new IOException("Android audio service is unavailable.");
        inputManager = (InputManager?)GetSystemService(InputService)
            ?? throw new IOException("Android input service is unavailable.");
        hostHandler = new Handler(Looper.MainLooper!);
        hostFocusListener = new HostFocusListener(this);
        using var attributes = new AudioAttributes.Builder().SetUsage(AudioUsageKind.Game)!
            .SetContentType(AudioContentType.Music)!.Build()!;
        using var builder = new AudioFocusRequestClass.Builder(AudioFocus.Gain);
        audioFocusRequest = builder.SetAudioAttributes(attributes)!
            .SetAcceptsDelayedFocusGain(true)!
            .SetWillPauseWhenDucked(true)!
            .SetOnAudioFocusChangeListener(hostFocusListener, hostHandler)!.Build();
        inputManager.RegisterInputDeviceListener(hostFocusListener, hostHandler);
    }

    private void RefreshRunGate(bool requestFocus = false)
    {
        if (session is not null && requestFocus && resumed && focused && !menuOpen && !destroyed && AudioEnabled &&
            !audioFocusRequested && audioManager is not null && audioFocusRequest is not null)
        {
            var result = audioManager.RequestAudioFocus(audioFocusRequest);
            audioFocusRequested = result != AudioFocusRequest.Failed;
            audioFocusGranted = result == AudioFocusRequest.Granted;
            global::Android.Util.Log.Info("SuperMetroid", $"Audio focus request: {result}");
        }
        session?.SetActive(CanRun);
    }

    private void ReleaseAudioFocus()
    {
        // Clear ownership first: a delayed callback after abandon must not restart a
        // backgrounded session. Request again only on an explicit foreground/resume event.
        audioFocusRequested = audioFocusGranted = false;
        if (audioManager is not null && audioFocusRequest is not null)
            audioManager.AbandonAudioFocusRequest(audioFocusRequest);
    }

    private void DisposeHostFocus()
    {
        ReleaseAudioFocus();
        if (hostFocusListener is not null) inputManager?.UnregisterInputDeviceListener(hostFocusListener);
        audioFocusRequest?.Dispose();
        hostFocusListener?.Dispose();
        hostHandler?.Dispose();
    }

    private sealed class HostFocusListener(MainActivity owner) : Java.Lang.Object,
        AudioManager.IOnAudioFocusChangeListener, InputManager.IInputDeviceListener
    {
        public void OnAudioFocusChange(AudioFocus change)
        {
            if (owner.destroyed || !owner.audioFocusRequested) return;
            owner.audioFocusGranted = change == AudioFocus.Gain;
            if (change == AudioFocus.Loss) owner.audioFocusRequested = false;
            global::Android.Util.Log.Info("SuperMetroid", $"Audio focus changed: {change}");
            // Pausing for duck requests preserves game/audio synchronization. Crucially,
            // this callback never requests focus back from the interrupting application.
            owner.RefreshRunGate();
            owner.ObserveAudioFocus(change.ToString());
        }

        public void OnInputDeviceAdded(int deviceId) => ClearInput(deviceId, "added");
        public void OnInputDeviceChanged(int deviceId) => ClearInput(deviceId, "changed");
        public void OnInputDeviceRemoved(int deviceId) => ClearInput(deviceId, "removed");

        private void ClearInput(int deviceId, string reason)
        {
            owner.session?.Input.Clear();
            global::Android.Util.Log.Info("SuperMetroid", $"Input device {deviceId} {reason}; held and pending input cleared.");
        }
    }
}
