using Android.Hardware.Input;
using Android.Media;
using Android.OS;

namespace SuperMetroid.Android;

public sealed partial class MainActivity
{
    /// <summary>Android audio service used to request and abandon focus for gameplay audio.</summary>
    private AudioManager? audioManager;
    /// <summary>Configured focus request that pauses playback when audio is ducked.</summary>
    private AudioFocusRequestClass? audioFocusRequest;
    /// <summary>Listener shared by audio-focus and input-device callbacks.</summary>
    private HostFocusListener? hostFocusListener;
    /// <summary>Android service used to observe input-device changes and clear stale controls.</summary>
    private InputManager? inputManager;
    /// <summary>Main-thread handler associated with host-focus and input listener registration.</summary>
    private Handler? hostHandler;
    /// <summary>Whether an audio-focus request is pending or currently held.</summary>
    private bool audioFocusRequested;
    /// <summary>Whether Android has granted the requested focus needed to run the session.</summary>
    private bool audioFocusGranted;
    // Compiled away in normal APKs; implemented only by opt-in device probes.
    /// <summary>Optional instrumentation hook for recording host audio-focus transitions.</summary>
    /// <param name="change">Text form of the Android focus-change value.</param>
    partial void ObserveAudioFocus(string change);

    /// <summary>Whether current session options permit audio playback; defaults to enabled before a session exists.</summary>
    private bool AudioEnabled => session?.EffectiveOptions?.AudioEnabled ?? true;
    /// <summary>Combined lifecycle, window-focus, menu, audio-option, and granted-focus gate for game updates.</summary>
    private bool CanRun => AndroidRunPolicy.CanRun(resumed, focused, menuOpen, destroyed,
        AudioEnabled, audioFocusGranted);

    /// <summary>Creates Android audio/input listeners and registers them on the main thread.</summary>
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

    /// <summary>Requests audio focus when explicitly asked in the foreground, then updates the session run gate.</summary>
    /// <param name="requestFocus">True only for an explicit foreground/resume transition that may request focus.</param>
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

    /// <summary>Clears local focus ownership before abandoning the Android request, so late callbacks cannot resume play.</summary>
    private void ReleaseAudioFocus()
    {
        // Clear ownership first: a delayed callback after abandon must not restart a
        // backgrounded session. Request again only on an explicit foreground/resume event.
        audioFocusRequested = audioFocusGranted = false;
        if (audioManager is not null && audioFocusRequest is not null)
            audioManager.AbandonAudioFocusRequest(audioFocusRequest);
    }

    /// <summary>Releases audio focus and unregisters/disposes the Android host listeners and main-thread handler.</summary>
    private void DisposeHostFocus()
    {
        ReleaseAudioFocus();
        if (hostFocusListener is not null) inputManager?.UnregisterInputDeviceListener(hostFocusListener);
        audioFocusRequest?.Dispose();
        hostFocusListener?.Dispose();
        hostHandler?.Dispose();
    }

    /// <summary>Routes Android audio-focus changes and input-device lifecycle events to the owning activity.</summary>
    /// <param name="owner">Activity whose session and host-focus state are updated by callbacks.</param>
    private sealed class HostFocusListener(MainActivity owner) : Java.Lang.Object,
        AudioManager.IOnAudioFocusChangeListener, InputManager.IInputDeviceListener
    {
        /// <summary>Records the new focus grant state and refreshes whether the game session may run.</summary>
        /// <param name="change">Android grant, duck, transient loss, or permanent loss notification.</param>
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

        /// <summary>Clears held and pending controls when an input device is connected.</summary>
        /// <param name="deviceId">Android identifier of the added device.</param>
        public void OnInputDeviceAdded(int deviceId) => ClearInput(deviceId, "added");
        /// <summary>Clears held and pending controls when an input device changes configuration.</summary>
        /// <param name="deviceId">Android identifier of the changed device.</param>
        public void OnInputDeviceChanged(int deviceId) => ClearInput(deviceId, "changed");
        /// <summary>Clears held and pending controls when an input device is disconnected.</summary>
        /// <param name="deviceId">Android identifier of the removed device.</param>
        public void OnInputDeviceRemoved(int deviceId) => ClearInput(deviceId, "removed");

        /// <summary>Clears session input and logs the device event so a disconnected control cannot remain held.</summary>
        /// <param name="deviceId">Android identifier of the device that changed.</param>
        /// <param name="reason">Short event description used in the diagnostic log.</param>
        private void ClearInput(int deviceId, string reason)
        {
            owner.session?.Input.Clear();
            global::Android.Util.Log.Info("SuperMetroid", $"Input device {deviceId} {reason}; held and pending input cleared.");
        }
    }
}
