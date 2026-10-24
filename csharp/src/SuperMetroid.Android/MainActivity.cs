using SuperMetroid.Core;
using Android.OS;
using Android.Views;
using Android.Content.PM;
using SuperMetroid.Core.Input;
using Android.Content;
using Android.Hardware.Input;
using Android.Media;
using SuperMetroid.Core.Frontend;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Android.Diagnostics;
using SuperMetroid.Diagnostics.Configuration;
using SuperMetroid.Diagnostics.SaveStates;
using AndroidX.Activity;
using AndroidX.Activity.Result;
using AndroidX.Activity.Result.Contract;
using AndroidX.Core.View;

namespace SuperMetroid.Android;

/// <summary>
/// Android entry point. Imports a user's cartridge, routes controller events, and gates
/// the game worker on activity/focus lifetime.
/// </summary>
[Activity(Label = "Super Metroid C# Testing", MainLauncher = true, Exported = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
public sealed class MainActivity : ComponentActivity
{
    /// <summary>The MIME types the open-document pickers accept: any file, validated after it is chosen.</summary>
    private static readonly string[] AnyDocument = ["*/*"];
    /// <summary>The document pickers, registered at creation before the activity can launch any of them.</summary>
    private AndroidDocumentPickers? documentPickers;
    /// <summary>Installed game's worker and input owner, created only after cartridge setup succeeds.</summary>
    private AndroidGameSession? session;
    /// <summary>Activity lifecycle input to the gameplay and audio run gate.</summary>
    private bool resumed;
    /// <summary>Window-focus input to the gameplay and audio run gate.</summary>
    private bool focused;
    /// <summary>Prevents late installation callbacks from starting a disposed activity.</summary>
    private bool destroyed;
    /// <summary>Cancels installation or repair work when the activity is destroyed.</summary>
    private readonly CancellationTokenSource setupStopping = new();

    /// <summary>Initializes immersive display and host focus services, then starts cancellable installation repair or cartridge setup.</summary>
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        documentPickers = new AndroidDocumentPickers(
            DiagnosticExport: RegisterDocumentPicker(new ActivityResultContracts.CreateDocument("application/zip"), AndroidDocumentRequest.DiagnosticExport),
            StateImport: RegisterDocumentPicker(new ActivityResultContracts.OpenDocument(), AndroidDocumentRequest.StateImport),
            SaveImport: RegisterDocumentPicker(new ActivityResultContracts.OpenDocument(), AndroidDocumentRequest.SaveImport),
            RomImport: RegisterDocumentPicker(new ActivityResultContracts.OpenDocument(), AndroidDocumentRequest.RomImport));
        ActionBar?.Hide();
        EnterImmersiveMode();
        InitializeHostFocus();
        var status = new TextView(this) { Text = "Checking game files…" };
        SetContentView(status);
        _ = PrepareAssets(status);
    }

    /// <summary>Repairs installed assets in private storage or presents cartridge setup, reporting failures in the setup view.</summary>
    private async Task PrepareAssets(TextView status)
    {
        try
        {
            string root = FilesDir?.AbsolutePath ?? throw new IOException("Android did not provide private storage.");
            CancellationToken token = setupStopping.Token;
            var progress = new Progress<string>(message => { if (!destroyed) status.Text = message; });
            GameInstallation? installed = await Task.Run(() =>
                GameAssetInstaller.OpenOrRepair(root, token, progress), token);
            if (destroyed) return;
            if (installed is null) ShowRomSetup();
            else StartInstalledGame(root);
        }
        catch (Exception error)
        {
            // Startup failure remains visible and available to adb rather than causing an
            // opaque native crash dialog. Runtime failures will also get durable reports.
            global::Android.Util.Log.Error("SuperMetroid", error.ToString());
            if (!destroyed) ShowRomSetup(error.Message);
        }
    }

    /// <summary>Marks the activity resumed, restores immersive display, and requests audio focus before reevaluating the session run gate.</summary>
    protected override void OnResume()
    {
        base.OnResume();
        resumed = true;
        EnterImmersiveMode();
        RefreshRunGate(requestFocus: true);
    }

    /// <summary>Closes the session run gate, clears latched input through session deactivation, and abandons audio focus before framework pause.</summary>
    protected override void OnPause()
    {
        resumed = false;
        session?.SetActive(false);
        ReleaseAudioFocus();
        base.OnPause();
    }

    /// <summary>Updates window-focus ownership and reevaluates gameplay eligibility; regained focus also restores immersive display and may request audio focus.</summary>
    public override void OnWindowFocusChanged(bool hasFocus)
    {
        base.OnWindowFocusChanged(hasFocus);
        focused = hasFocus;
        if (hasFocus) EnterImmersiveMode();
        RefreshRunGate(requestFocus: hasFocus);
    }

    /// <summary>Keep system chrome out of the game surface, including after dialogs/resume.</summary>
    private void EnterImmersiveMode()
    {
        if (Window is not { } window) return;
        // Edge-to-edge is enforced from API 35; earlier releases need the opt-in.
        if (!OperatingSystem.IsAndroidVersionAtLeast(35)) WindowCompat.SetDecorFitsSystemWindows(window, false);
        WindowInsetsControllerCompat controller = WindowCompat.GetInsetsController(window, window.DecorView)
            ?? throw new InvalidOperationException("AndroidX supplied no window insets controller.");
        controller.SystemBarsBehavior = WindowInsetsControllerCompat.BehaviorShowTransientBarsBySwipe;
        controller.Hide(WindowInsetsCompat.Type.SystemBars());
    }

    /// <summary>Routes mapped controller keys to the session input latch while gameplay can run, and opens the testing menu on Back/Mode release.</summary>
    /// <returns>True for consumed menu or mapped controller events; otherwise the framework result, including keys dispatched while a menu is open.</returns>
    public override bool DispatchKeyEvent(KeyEvent? e)
    {
        if (e is not null && session is not null)
        {
            session.RecordInput($"{e.KeyCode} {e.Action} resumed={resumed} focused={focused}");
            if (e.KeyCode is Keycode.Back or Keycode.ButtonMode && !menuOpen)
            {
                // Open on release so the same Back-up cannot dismiss the new dialog.
                if (e.Action == KeyEventActions.Up) ShowTestingMenu();
                return true;
            }
            if (menuOpen) return base.DispatchKeyEvent(e);
            SnesButton mapped = controllerPreferences.Resolve(e.KeyCode.ToString(), AndroidControllerMapping.Map(e.KeyCode));
            if (mapped != SnesButton.None)
            {
                if (CanRun && e.Action is KeyEventActions.Down or KeyEventActions.Up)
                    session.Input.Set((int)e.KeyCode, e.Action == KeyEventActions.Down ? mapped : SnesButton.None);
                return true;
            }
        }
        return base.DispatchKeyEvent(e);
    }

    /// <summary>Marks the activity destroyed, cancels pending asset setup, releases focus listeners, and requests asynchronous session shutdown.</summary>
    /// <remarks>The activity callback does not wait for the game worker; late setup completions cannot restart a destroyed activity.</remarks>
    protected override void OnDestroy()
    {
        destroyed = true;
        setupStopping.Cancel();
        setupStopping.Dispose();
        DisposeHostFocus();
        if (session is not null) _ = session.Stop();
        base.OnDestroy();
    }

    /// <summary>Current key-to-SNES binding overrides used by controller event dispatch.</summary>
    private AndroidControllerPreferences controllerPreferences = new();
    /// <summary>Private-storage path for durable controller binding overrides.</summary>
    private string ControllerPreferencesPath => Path.Combine(PrivateFilesPath, "controller-bindings.json");

    /// <summary>The app's private file directory; Android supplies it to every running activity.</summary>
    private string PrivateFilesPath => FilesDir?.AbsolutePath ??
        throw new IOException("Android app file storage is unavailable.");

    /// <summary>The app's cache directory; Android supplies it to every running activity.</summary>
    private string PrivateCachePath => CacheDir?.AbsolutePath ??
        throw new IOException("Android app cache storage is unavailable.");

    /// <summary>The activity's content resolver, which opens user-chosen documents.</summary>
    private ContentResolver Documents => ContentResolver ??
        throw new IOException("Android supplied no content resolver to open documents.");

    /// <summary>Loads persisted binding overrides when an installed game session starts.</summary>
    private void LoadControllerPreferences()
    {
        if (File.Exists(ControllerPreferencesPath))
            controllerPreferences = AndroidControllerPreferences.Parse(File.ReadAllText(ControllerPreferencesPath));
    }

    /// <summary>Presents each Android controller key and its current SNES mapping for immediate editing.</summary>
    private void ShowControllerSettings()
    {
        menuOpen = true;
        var keys = AndroidControllerMapping.Keys;
        string[] labels = keys.Select(key => $"{key} → {controllerPreferences.Resolve(key.ToString(), AndroidControllerMapping.Map(key))}").ToArray();
        var dialog = new AlertDialog.Builder(this);
        dialog.SetTitle("Controller → SNES button (applies now)");
        dialog.SetItems(labels, (_, selected) =>
            {
                var key = keys[selected.Which];
                SnesButton[] buttons = Enum.GetValues<SnesButton>()
                    .Where(button => button == SnesButton.None || (((ushort)button & ((ushort)button - 1)) == 0)).ToArray();
                var current = controllerPreferences.Resolve(key.ToString(), AndroidControllerMapping.Map(key));
                var choiceDialog = new AlertDialog.Builder(this);
                choiceDialog.SetTitle($"Map {key} to SNES");
                choiceDialog.SetSingleChoiceItems(buttons.Select(button => button.ToString()).ToArray(), Array.IndexOf(buttons, current),
                        (sender, choice) =>
                        {
                            try
                            {
                                SaveControllerPreferences(controllerPreferences.WithBinding(key.ToString(), buttons[choice.Which]));
                                (sender as AlertDialog ?? throw new InvalidOperationException("A dialog choice is raised by its dialog.")).Dismiss();
                                ShowControllerSettings();
                            }
                            catch (Exception error) { _ = ShowStateResult(Task.FromException<string>(error)); }
                        });
                choiceDialog.SetNegativeButton("Back", (_, _) => ShowControllerSettings());
                choiceDialog.SetCancelable(false);
                choiceDialog.Show();
            });
        dialog.SetNeutralButton("Restore defaults", (_, _) =>
            {
                try { SaveControllerPreferences(new()); ShowControllerSettings(); }
                catch (Exception error) { _ = ShowStateResult(Task.FromException<string>(error)); }
            });
        dialog.SetNegativeButton("Back", (_, _) => { menuOpen = false; ShowTestingMenu(); });
        dialog.SetCancelable(false);
        dialog.Show();
    }

    /// <summary>Atomically persists updated bindings and clears held or pending input before publishing them.</summary>
    private void SaveControllerPreferences(AndroidControllerPreferences updated)
    {
        // Persist before publishing the new mapping. A failed write cannot leave the
        // screen claiming a binding that will disappear on restart. Clear pending taps
        // as well as holds so changing a mapping cannot leave an old SNES button down.
        string path = ControllerPreferencesPath;
        File.WriteAllText(path + ".tmp", updated.Serialize());
        File.Move(path + ".tmp", path, overwrite: true);
        session?.Input.Clear();
        controllerPreferences = updated;
    }

    /// <summary>Opens a document picker to write a private diagnostic ZIP without changing state slots.</summary>
    private void ChooseDiagnosticExport()
    {
        menuOpen = true;
        var dialog2 = new AlertDialog.Builder(this);
        dialog2.SetTitle("Export private diagnostics");
        dialog2.SetMessage($"Include saved slot {selectedSlot}, regular save, recordings and their preserved seeds, settings, and error/timing logs. " +
                "This does not overwrite or create a state slot. Debugger states may contain ROM data: keep the ZIP private. " +
                "Choose local device storage to transfer it later; nothing is uploaded automatically.");
        dialog2.SetPositiveButton("Choose file", (_, _) =>
            {
                try
                {
                    LaunchDocumentPicker(AndroidDocumentRequest.DiagnosticExport,
                        new Java.Lang.String($"SuperMetroid-diagnostics-{DateTime.Now:yyyyMMdd-HHmmss}.zip"));
                }
                catch (Exception error) { _ = ShowStateResult(Task.FromException<string>(error)); }
            });
        dialog2.SetNegativeButton("Cancel", (_, _) => { menuOpen = false; ShowTestingMenu(); });
        dialog2.SetCancelable(false);
        dialog2.Show();
    }

    /// <summary>Registers the picker for <paramref name="request"/>, whose result is routed by <see cref="OnDocumentChosen"/>.</summary>
    private ActivityResultLauncher RegisterDocumentPicker(ActivityResultContract contract, AndroidDocumentRequest request) =>
        RegisterForActivityResult(contract, new AndroidDocumentResult(uri => OnDocumentChosen(request, uri)));

    /// <summary>Opens the picker for <paramref name="request"/> with the contract's input: a suggested file name or the accepted MIME types.</summary>
    private void LaunchDocumentPicker(AndroidDocumentRequest request, Java.Lang.Object input) =>
        (documentPickers ?? throw new InvalidOperationException("Document pickers are registered when the activity is created."))
            .For(request).Launch(input);

    /// <summary>Routes document-picker results to cartridge setup, private diagnostic export, debugger-state import, or regular-save import.</summary>
    /// <remarks>Cancelled cartridge selection returns to setup; cancelled export/import returns to the testing menu.</remarks>
    private void OnDocumentChosen(AndroidDocumentRequest request, global::Android.Net.Uri? uri)
    {
        if (request == AndroidDocumentRequest.RomImport)
        {
            if (uri is not null) _ = ImportRomDocument(uri);
            else ShowRomSetup();
            return;
        }
        if (uri is null)
        {
            menuOpen = false;
            ShowTestingMenu();
            return;
        }
        _ = ShowStateResult(request switch
        {
            AndroidDocumentRequest.DiagnosticExport => ExportDiagnostics(uri),
            AndroidDocumentRequest.StateImport => ImportDocument(uri, true),
            AndroidDocumentRequest.SaveImport => ImportDocument(uri, false),
            AndroidDocumentRequest.RomImport => throw new InvalidOperationException("ROM import results are routed to cartridge setup."),
            _ => throw new InvalidOperationException($"Undefined {nameof(AndroidDocumentRequest)} {request}."),
        });
    }

    /// <summary>Stages a diagnostic ZIP, copies it to the chosen document, and removes the staging file.</summary>
    private async Task<string> ExportDiagnostics(global::Android.Net.Uri uri)
    {
        string temporary = Path.Combine(PrivateCachePath, $"diagnostics-{Guid.NewGuid():N}.zip");
        try
        {
            if (session is null) throw new InvalidOperationException("No game session is available for export.");
            await session.ExportDiagnostics(temporary, selectedSlot);
            using System.IO.Stream destination = Documents.OpenOutputStream(uri, "wt")
                ?? throw new IOException("Android could not open the selected export document.");
            using System.IO.Stream source = File.OpenRead(temporary);
            await source.CopyToAsync(destination);
            await destination.FlushAsync();
            return "Exported private diagnostic ZIP. Existing saves and state slots were not overwritten.";
        }
        finally
        {
            // Only this uniquely named staging file belongs to this operation.
            if (File.Exists(temporary)) File.Delete(temporary);
        }
    }

    /// <summary>Android service used to request and abandon exclusive game audio focus.</summary>
    private AudioManager? audioManager;
    /// <summary>Configured game-audio request that accepts delayed focus and pauses on ducking.</summary>
    private AudioFocusRequestClass? audioFocusRequest;
    /// <summary>Receives audio focus and controller-device lifetime callbacks.</summary>
    private HostFocusListener? hostFocusListener;
    /// <summary>Android service that publishes controller-device changes.</summary>
    private InputManager? inputManager;
    /// <summary>Main-thread handler for host focus and device callbacks.</summary>
    private Handler? hostHandler;
    /// <summary>Records whether a focus request remains outstanding, including delayed grants.</summary>
    private bool audioFocusRequested;
    /// <summary>Records whether the session currently owns focus needed for enabled audio.</summary>
    private bool audioFocusGranted;
#if ANDROID_HOST_PROBES
    /// <summary>Opt-in device probe included only when <c>AndroidHostProbes</c> is enabled.</summary>
    private AudioFocusProbe? audioFocusProbe;

    /// <summary>Creates the opt-in audio focus observer with the activity's current run-gate inputs.</summary>
    private AudioFocusProbe AudioFocusProbe =>
        audioFocusProbe ??= new AudioFocusProbe(this, () => new(focused, resumed, CanRun, audioManager, hostHandler));

    /// <summary>Forwards new probe intents to the optional audio focus observer.</summary>
    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        AudioFocusProbe.OnNewIntent(intent);
    }
#endif

    /// <summary>Uses the installed session's audio setting, assuming audio is enabled during setup.</summary>
    private bool AudioEnabled => session?.EffectiveOptions?.AudioEnabled ?? true;
    /// <summary>Combines lifecycle, dialog, and audio-focus state to decide whether the worker may advance.</summary>
    private bool CanRun => AndroidRunPolicy.CanRun(resumed, focused, menuOpen, destroyed,
        AudioEnabled, audioFocusGranted);

    /// <summary>Registers Android audio-focus and input-device listeners on the main looper.</summary>
    private void InitializeHostFocus()
    {
        audioManager = (AudioManager?)GetSystemService(AudioService)
            ?? throw new IOException("Android audio service is unavailable.");
        inputManager = (InputManager?)GetSystemService(InputService)
            ?? throw new IOException("Android input service is unavailable.");
        hostHandler = new Handler(Looper.MainLooper ?? throw new IOException("Android has no main looper."));
        hostFocusListener = new HostFocusListener(this);
        using var attributesBuilder = new AudioAttributes.Builder();
        attributesBuilder.SetUsage(AudioUsageKind.Game);
        attributesBuilder.SetContentType(AudioContentType.Music);
        using var attributes = attributesBuilder.Build() ??
            throw new IOException("Android could not build the game audio attributes.");
        using var builder = new AudioFocusRequestClass.Builder(AudioFocus.Gain);
        audioFocusRequest = builder.SetAudioAttributes(attributes)
            .SetAcceptsDelayedFocusGain(true)
            .SetWillPauseWhenDucked(true)
            .SetOnAudioFocusChangeListener(hostFocusListener, hostHandler).Build();
        inputManager.RegisterInputDeviceListener(hostFocusListener, hostHandler);
    }

    /// <summary>Optionally requests focus on foreground entry, then applies the current run policy to the session.</summary>
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

    /// <summary>Clears focus ownership before abandoning the request so delayed callbacks cannot restart gameplay.</summary>
    private void ReleaseAudioFocus()
    {
        // Clear ownership first: a delayed callback after abandon must not restart a
        // backgrounded session. Request again only on an explicit foreground/resume event.
        audioFocusRequested = audioFocusGranted = false;
        if (audioManager is not null && audioFocusRequest is not null)
            audioManager.AbandonAudioFocusRequest(audioFocusRequest);
    }

    /// <summary>Releases focus and unregisters the activity's audio and controller-device listeners.</summary>
    private void DisposeHostFocus()
    {
        ReleaseAudioFocus();
        if (hostFocusListener is not null) inputManager?.UnregisterInputDeviceListener(hostFocusListener);
        audioFocusRequest?.Dispose();
        hostFocusListener?.Dispose();
        hostHandler?.Dispose();
    }

    /// <summary>Relays Android audio-focus and input-device changes into the activity run gate.</summary>
    [ArchitectureReview("back-reference", "An Android audio-focus and input-device callback; the framework delivers it to this listener, which must pause or resume the activity that registered it.")]
    private sealed class HostFocusListener(MainActivity owner) : Java.Lang.Object,
        AudioManager.IOnAudioFocusChangeListener, InputManager.IInputDeviceListener
    {
        /// <summary>Pauses or resumes the session according to a valid focus callback without requesting focus back.</summary>
        public void OnAudioFocusChange(AudioFocus change)
        {
            if (owner.destroyed || !owner.audioFocusRequested) return;
            owner.audioFocusGranted = change == AudioFocus.Gain;
            if (change == AudioFocus.Loss) owner.audioFocusRequested = false;
            global::Android.Util.Log.Info("SuperMetroid", $"Audio focus changed: {change}");
            // Pausing for duck requests preserves game/audio synchronization. Crucially,
            // this callback never requests focus back from the interrupting application.
            owner.RefreshRunGate();
#if ANDROID_HOST_PROBES
            owner.AudioFocusProbe.Observe(change.ToString());
#endif
        }

        /// <summary>Clears latched buttons when Android discovers a controller.</summary>
        public void OnInputDeviceAdded(int deviceId) => ClearInput(deviceId, "added");
        /// <summary>Clears latched buttons when a controller mapping changes.</summary>
        public void OnInputDeviceChanged(int deviceId) => ClearInput(deviceId, "changed");
        /// <summary>Clears latched buttons when a controller disconnects.</summary>
        public void OnInputDeviceRemoved(int deviceId) => ClearInput(deviceId, "removed");

        /// <summary>Releases held and queued input after any device change and records the reason.</summary>
        private void ClearInput(int deviceId, string reason)
        {
            owner.session?.Input.Clear();
            global::Android.Util.Log.Info("SuperMetroid", $"Input device {deviceId} {reason}; held and pending input cleared.");
        }
    }

    /// <summary>Prompts for a debugger-state or regular-save document, explaining its validation and backup behavior.</summary>
    private void ChooseImport(bool state)
    {
        menuOpen = true;
        var dialog3 = new AlertDialog.Builder(this);
        dialog3.SetTitle(state ? $"Import into slot {selectedSlot}?" : "Import regular save?");
        dialog3.SetMessage(state
                ? "Choose a trusted .smstate file, not a ZIP. After validation it replaces this slot; the previous file is retained in import-backups. It is not automatically loaded."
                : "Choose a Super Metroid JSON save, not a ZIP or .srm. It will replace the regular save at next app launch, after validation and backup. Current gameplay is unchanged.");
        dialog3.SetPositiveButton("Choose file", (_, _) =>
            {
                try
                {
                    LaunchDocumentPicker(state ? AndroidDocumentRequest.StateImport : AndroidDocumentRequest.SaveImport, AnyDocument);
                }
                catch (Exception error) { _ = ShowStateResult(Task.FromException<string>(error)); }
            });
        dialog3.SetNegativeButton("Cancel", (_, _) => { menuOpen = false; ShowTestingMenu(); });
        dialog3.SetCancelable(false);
        dialog3.Show();
    }

    /// <summary>Copies the selected document to a temporary file and hands it to the session's validated importer.</summary>
    private async Task<string> ImportDocument(global::Android.Net.Uri uri, bool state)
    {
        string temporary = Path.Combine(PrivateCachePath, $"import-{Guid.NewGuid():N}.tmp");
        try
        {
            using (System.IO.Stream source = Documents.OpenInputStream(uri)
                ?? throw new IOException("Android could not open the selected import."))
            using (System.IO.Stream destination = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write))
                await source.CopyToAsync(destination);
            if (session is null) throw new InvalidOperationException("No active session is available for import.");
            return state ? await session.ImportState(temporary, selectedSlot) : await session.ImportSave(temporary);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    /// <summary>Shows cartridge selection or a recoverable setup error when no usable installation exists.</summary>
    private void ShowRomSetup(string? error = null)
    {
        if (destroyed) return;
        menuOpen = true;
        var layout = new LinearLayout(this) { Orientation = global::Android.Widget.Orientation.Vertical };
        int padding = (int)(24 * (Resources?.DisplayMetrics?.Density ??
            throw new IOException("Android supplied no display metrics.")));
        layout.SetPadding(padding, padding, padding, padding);
        var title = new TextView(this) { Text = "Choose your Super Metroid ROM", TextSize = 24 };
        var description = new TextView(this)
        {
            Text = "Supply your own Japan/USA NTSC v1.0 ROM (.smc or .sfc). " +
                   "The app will keep a private copy and extract its audio. Your original file is kept.",
            TextSize = 17,
        };
        var status = new TextView(this) { Text = error ?? "No game data is included in the APK.", TextSize = 16 };
        var choose = new Button(this) { Text = "Choose ROM" };
        choose.Click += (_, _) =>
        {
            try
            {
                LaunchDocumentPicker(AndroidDocumentRequest.RomImport, AnyDocument);
            }
            catch (Exception failure) { ShowRomSetup(failure.Message); }
        };
        layout.AddView(title);
        layout.AddView(description);
        layout.AddView(status);
        layout.AddView(choose);
        SetContentView(layout);
        choose.RequestFocus();
    }

    /// <summary>Installs a selected cartridge from the document provider and starts the game after extraction.</summary>
    private async Task ImportRomDocument(global::Android.Net.Uri uri)
    {
        var status = new TextView(this) { Text = "Checking ROM…", TextSize = 18 };
        SetContentView(status);
        try
        {
            string root = FilesDir?.AbsolutePath ?? throw new IOException("Android did not provide private storage.");
            CancellationToken token = setupStopping.Token;
            var progress = new Progress<string>(message => { if (!destroyed) status.Text = message; });
            await Task.Run(() =>
            {
                // The installer reads a ROM file; the picked document is copied to one first.
                string picked = Path.Combine(CacheDir?.AbsolutePath ?? throw new IOException("Android did not provide a cache directory."),
                    "picked-rom.smc");
                using (System.IO.Stream source = Documents.OpenInputStream(uri)
                    ?? throw new IOException("Android could not open that file. Choose another local ROM."))
                using (var copy = File.Create(picked))
                    source.CopyTo(copy);
                try { GameAssetInstaller.Install(picked, root, token, progress); }
                finally { File.Delete(picked); }
            }, token);
            if (!destroyed) StartInstalledGame(root);
        }
        catch (System.OperationCanceledException) when (destroyed) { }
        catch (Exception error)
        {
            global::Android.Util.Log.Error("SuperMetroid", error.ToString());
            if (!destroyed) ShowRomSetup(error.Message);
        }
    }

    /// <summary>Creates the game surface and session once, then reapplies the lifecycle and focus run gate.</summary>
    private void StartInstalledGame(string root)
    {
        if (destroyed || session is not null) return;
        LoadControllerPreferences();
        var view = new AndroidGameView(this);
        view.LongClick += (_, _) => ShowTestingMenu();
        SetContentView(view);
        menuOpen = false;
        session = new AndroidGameSession(root, view);
        RefreshRunGate(requestFocus: true);
    }

    /// <summary>Edits persisted INI settings atomically for the next launch while showing active values.</summary>
    private void ShowIniSettings()
    {
        menuOpen = true;
        try
        {
            string path = Path.Combine(PrivateFilesPath, "SuperMetroid.ini");
            var saved = HostOptionsIni.Parse(File.ReadAllText(path), path);
            var definitions = AndroidSettingDefinitions.All;
            var dialog4 = new AlertDialog.Builder(this);
            dialog4.SetTitle("Settings — apply on next app launch");
            dialog4.SetItems(definitions.Select(d => $"{d.Label}: {d.Read(saved)}" +
                    (session?.EffectiveOptions is { } active && d.Read(active) != d.Read(saved)
                        ? $" (active: {d.Read(active)})" : "")).ToArray(), (_, selected) =>
                {
                    var definition = definitions[selected.Which];
                    var choiceDialog2 = new AlertDialog.Builder(this);
                    choiceDialog2.SetTitle(definition.Label);
                    choiceDialog2.SetSingleChoiceItems(definition.Values, Array.IndexOf(definition.Values, definition.Read(saved)),
                            (sender, choice) =>
                            {
                                try
                                {
                                    // Re-read just before editing. Preserve all unrelated settings;
                                    // validate before replacing the durable file atomically.
                                    string updated = AndroidIniEditing.WithValue(File.ReadAllText(path),
                                        definition.Section, definition.Key, definition.Values[choice.Which]);
                                    File.WriteAllText(path + ".tmp", updated);
                                    File.Move(path + ".tmp", path, overwrite: true);
                                    (sender as AlertDialog ?? throw new InvalidOperationException("A dialog choice is raised by its dialog.")).Dismiss();
                                    ShowIniSettings();
                                }
                                catch (Exception error) { _ = ShowStateResult(Task.FromException<string>(error)); }
                            });
                    choiceDialog2.SetNegativeButton("Back", (_, _) => ShowIniSettings());
                    choiceDialog2.SetCancelable(false);
                    choiceDialog2.Show();
                });
            dialog4.SetNegativeButton("Back", (_, _) => { menuOpen = false; ShowTestingMenu(); });
            dialog4.SetCancelable(false);
            dialog4.Show();
        }
        catch (Exception error) { _ = ShowStateResult(Task.FromException<string>(error)); }
    }

    /// <summary>Closes gameplay input while a host tool dialog is active.</summary>
    private bool menuOpen;
    /// <summary>Debugger state slot selected by the testing menu and diagnostic exporter.</summary>
    private int selectedSlot;

    /// <summary>
    /// Host tools are outside cartridge pause/input logic. Native dialogs supply D-pad
    /// navigation, while the worker services state requests at an emulation-frame boundary.
    /// </summary>
    private void ShowTestingMenu()
    {
        if (session is null || menuOpen) return;
        menuOpen = true;
        session.SetActive(false);
        var dialog5 = new AlertDialog.Builder(this);
        dialog5.SetTitle($"Testing tools - slot {DebuggerSaveStateStore.SlotName(selectedSlot)}");
        dialog5.SetItems(new[] { "Resume", "Choose state slot", "Save state", "Load state", "INI settings (next launch)", "Controller bindings", "Export private diagnostics", "Import debugger state", "Import regular save (next launch)" }, (_, args) =>
            {
                menuOpen = false;
                switch (args.Which)
                {
                    case 0: RefreshRunGate(requestFocus: true); break;
                    case 1: ChooseStateSlot(); break;
                    case 2: _ = ShowStateResult(session.SaveSlot(selectedSlot)); break;
                    case 3: _ = ShowStateResult(session.LoadSlot(selectedSlot)); break;
                    case 4: ShowIniSettings(); break;
                    case 5: ShowControllerSettings(); break;
                    case 6: ChooseDiagnosticExport(); break;
                    case 7: ChooseImport(state: true); break;
                    case 8: ChooseImport(state: false); break;
                    default:
                        throw new InvalidOperationException($"Testing menu has no item {args.Which}.");
                }
            });
        dialog5.SetOnCancelListener(new MenuCancelled(this));
        dialog5.Show();
    }

    /// <summary>Shows the available debugger state slots and returns to the testing menu after selection.</summary>
    private void ChooseStateSlot()
    {
        menuOpen = true;
        var dialog6 = new AlertDialog.Builder(this);
        dialog6.SetTitle("Debugger state slot");
        dialog6.SetSingleChoiceItems(Enumerable.Range(0, DebuggerStateFormat.AutomaticSlot + 1).Select(slot => $"Slot {DebuggerSaveStateStore.SlotName(slot)}").ToArray(), selectedSlot,
                (sender, args) =>
                {
                    selectedSlot = args.Which;
                    (sender as AlertDialog ?? throw new InvalidOperationException("A dialog choice is raised by its dialog.")).Dismiss();
                    menuOpen = false;
                    ShowTestingMenu();
                });
        dialog6.SetOnCancelListener(new MenuCancelled(this));
        dialog6.Show();
    }

    /// <summary>Displays an asynchronous host-tool result or full exception after logging failures.</summary>
    private async Task ShowStateResult(Task<string> operation)
    {
        menuOpen = true;
        string message;
        try { message = await operation; }
        catch (Exception error)
        {
            message = error.ToString();
            global::Android.Util.Log.Error("SuperMetroid", message);
        }
        if (destroyed) return;
        var dialog7 = new AlertDialog.Builder(this);
        dialog7.SetTitle("Debugger state");
        dialog7.SetMessage(message);
        dialog7.SetPositiveButton("OK", (_, _) => { menuOpen = false; ShowTestingMenu(); });
        dialog7.SetCancelable(false);
        dialog7.Show();
    }

    /// <summary>Reopens gameplay eligibility when a cancellable host dialog closes.</summary>
    private sealed class MenuCancelled(MainActivity owner) : Java.Lang.Object, global::Android.Content.IDialogInterfaceOnCancelListener
    {
        /// <summary>Closes the dialog run-gate hold and requests focus for resumed gameplay.</summary>
        public void OnCancel(global::Android.Content.IDialogInterface? dialog)
        {
            owner.menuOpen = false;
            owner.RefreshRunGate(requestFocus: true);
        }
    }
}
