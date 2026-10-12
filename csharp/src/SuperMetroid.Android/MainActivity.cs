using Android.App;
using Android.OS;
using Android.Widget;
using Android.Views;
using Android.Content.PM;
using SuperMetroid.Core.Input;
using SuperMetroid.AssetExtraction;
using Android.Content;
using Android.Hardware.Input;
using Android.Media;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Desktop;

namespace SuperMetroid.Android;

/// <summary>
/// Android entry point. Imports a user's cartridge, routes controller events, and gates
/// the game worker on activity/focus lifetime.
/// </summary>
[Activity(Label = "Super Metroid C# Testing", MainLauncher = true, Exported = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
public sealed class MainActivity : Activity
{
    private AndroidGameSession? session;
    private bool resumed;
    private bool focused;
    private bool destroyed;
    private readonly CancellationTokenSource setupStopping = new();

    /// <summary>Initializes immersive display and host focus services, then starts cancellable installation repair or cartridge setup.</summary>
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        ActionBar?.Hide();
        EnterImmersiveMode();
        InitializeHostFocus();
        var status = new TextView(this) { Text = "Checking game files…" };
        SetContentView(status);
        _ = PrepareAssets(status);
    }

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
        if (OperatingSystem.IsAndroidVersionAtLeast(30))
        {
            // Edge-to-edge is enforced on newer targets; older releases need the opt-in.
            if (!OperatingSystem.IsAndroidVersionAtLeast(35)) window.SetDecorFitsSystemWindows(false);
            if (window.InsetsController is { } controller)
            {
                controller.SystemBarsBehavior = (int)WindowInsetsControllerBehavior.ShowTransientBarsBySwipe;
                controller.Hide(WindowInsets.Type.SystemBars());
            }
        }
        else
        {
#pragma warning disable CS0618 // API 26-29 compatibility; modern devices use insets above.
            window.DecorView.SystemUiVisibility = (StatusBarVisibility)(SystemUiFlags.Fullscreen |
                SystemUiFlags.HideNavigation | SystemUiFlags.ImmersiveSticky |
                SystemUiFlags.LayoutFullscreen | SystemUiFlags.LayoutHideNavigation | SystemUiFlags.LayoutStable);
#pragma warning restore CS0618
        }
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

    private AndroidControllerPreferences controllerPreferences = new();
    private string ControllerPreferencesPath => Path.Combine(FilesDir!.AbsolutePath, "controller-bindings.json");

    private void LoadControllerPreferences()
    {
        if (File.Exists(ControllerPreferencesPath))
            controllerPreferences = AndroidControllerPreferences.Parse(File.ReadAllText(ControllerPreferencesPath));
    }

    private void ShowControllerSettings()
    {
        menuOpen = true;
        var keys = AndroidControllerMapping.Keys;
        string[] labels = keys.Select(key => $"{key} → {controllerPreferences.Resolve(key.ToString(), AndroidControllerMapping.Map(key))}").ToArray();
        new AlertDialog.Builder(this).SetTitle("Controller → SNES button (applies now)")!
            .SetItems(labels, (_, selected) =>
            {
                var key = keys[selected.Which];
                SnesButton[] buttons = Enum.GetValues<SnesButton>()
                    .Where(button => button == SnesButton.None || (((ushort)button & ((ushort)button - 1)) == 0)).ToArray();
                var current = controllerPreferences.Resolve(key.ToString(), AndroidControllerMapping.Map(key));
                new AlertDialog.Builder(this).SetTitle($"Map {key} to SNES")!
                    .SetSingleChoiceItems(buttons.Select(button => button.ToString()).ToArray(), Array.IndexOf(buttons, current),
                        (sender, choice) =>
                        {
                            try
                            {
                                SaveControllerPreferences(controllerPreferences.WithBinding(key.ToString(), buttons[choice.Which]));
                                ((AlertDialog)sender!).Dismiss();
                                ShowControllerSettings();
                            }
                            catch (Exception error) { _ = ShowStateResult(Task.FromException<string>(error)); }
                        })!
                    .SetNegativeButton("Back", (_, _) => ShowControllerSettings())!
                    .SetCancelable(false)!.Show();
            })!
            .SetNeutralButton("Restore defaults", (_, _) =>
            {
                try { SaveControllerPreferences(new()); ShowControllerSettings(); }
                catch (Exception error) { _ = ShowStateResult(Task.FromException<string>(error)); }
            })!
            .SetNegativeButton("Back", (_, _) => { menuOpen = false; ShowTestingMenu(); })!
            .SetCancelable(false)!.Show();
    }

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

    private void ChooseDiagnosticExport()
    {
        menuOpen = true;
        new AlertDialog.Builder(this).SetTitle("Export private diagnostics")!
            .SetMessage($"Include saved slot {selectedSlot}, regular save, recordings and their preserved seeds, settings, and error/timing logs. " +
                "This does not overwrite or create a state slot. Debugger states may contain ROM data: keep the ZIP private. " +
                "Choose local device storage to transfer it later; nothing is uploaded automatically.")!
            .SetPositiveButton("Choose file", (_, _) =>
            {
                try
                {
                    using var intent = new Intent(Intent.ActionCreateDocument);
                    intent.AddCategory(Intent.CategoryOpenable);
                    intent.SetType("application/zip");
                    intent.PutExtra(Intent.ExtraTitle, $"SuperMetroid-diagnostics-{DateTime.Now:yyyyMMdd-HHmmss}.zip");
#pragma warning disable CS0618 // Plain Activity host; result is handled explicitly below.
                    StartActivityForResult(intent, (int)AndroidDocumentRequest.DiagnosticExport);
#pragma warning restore CS0618
                }
                catch (Exception error) { _ = ShowStateResult(Task.FromException<string>(error)); }
            })!
            .SetNegativeButton("Cancel", (_, _) => { menuOpen = false; ShowTestingMenu(); })!
            .SetCancelable(false)!.Show();
    }

#pragma warning disable CS0672, CS0618 // Paired with the document request above; no AndroidX dependency.
    /// <summary>Routes document-picker results to cartridge setup, private diagnostic export, debugger-state import, or regular-save import.</summary>
    /// <remarks>Cancelled cartridge selection returns to setup; cancelled export/import returns to the testing menu. Unknown request codes receive only framework handling.</remarks>
    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        if (!Enum.IsDefined((AndroidDocumentRequest)requestCode)) return;
        var request = (AndroidDocumentRequest)requestCode;
        if (request == AndroidDocumentRequest.RomImport)
        {
            if (resultCode == Result.Ok && data?.Data is { } romUri) _ = ImportRomDocument(romUri);
            else ShowRomSetup();
            return;
        }
        if (resultCode != Result.Ok || data?.Data is not { } uri)
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
            _ => throw new InvalidOperationException($"Undefined {nameof(AndroidDocumentRequest)} {(int)request}."),
        });
    }
#pragma warning restore CS0672, CS0618

    private async Task<string> ExportDiagnostics(global::Android.Net.Uri uri)
    {
        string temporary = Path.Combine(CacheDir!.AbsolutePath, $"diagnostics-{Guid.NewGuid():N}.zip");
        try
        {
            if (session is null) throw new InvalidOperationException("No game session is available for export.");
            await session.ExportDiagnostics(temporary, selectedSlot);
            using System.IO.Stream destination = ContentResolver!.OpenOutputStream(uri, "wt")
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

    private AudioManager? audioManager;
    private AudioFocusRequestClass? audioFocusRequest;
    private HostFocusListener? hostFocusListener;
    private InputManager? inputManager;
    private Handler? hostHandler;
    private bool audioFocusRequested;
    private bool audioFocusGranted;
#if ANDROID_HOST_PROBES
    // Opt-in device probe builds only (AndroidHostProbes=true); normal APKs compile none of this.
    private AudioFocusProbe? audioFocusProbe;

    private AudioFocusProbe AudioFocusProbe =>
        audioFocusProbe ??= new AudioFocusProbe(this, () => new(focused, resumed, CanRun, audioManager, hostHandler));

    protected override void OnNewIntent(Intent? intent)
    {
        base.OnNewIntent(intent);
        AudioFocusProbe.OnNewIntent(intent);
    }
#endif

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
#if ANDROID_HOST_PROBES
            owner.AudioFocusProbe.Observe(change.ToString());
#endif
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

    private void ChooseImport(bool state)
    {
        menuOpen = true;
        new AlertDialog.Builder(this).SetTitle(state ? $"Import into slot {selectedSlot}?" : "Import regular save?")!
            .SetMessage(state
                ? "Choose a trusted .smstate file, not a ZIP. After validation it replaces this slot; the previous file is retained in import-backups. It is not automatically loaded."
                : "Choose a Super Metroid JSON save, not a ZIP or .srm. It will replace the regular save at next app launch, after validation and backup. Current gameplay is unchanged.")!
            .SetPositiveButton("Choose file", (_, _) =>
            {
                try
                {
                    using var intent = new Intent(Intent.ActionOpenDocument);
                    intent.AddCategory(Intent.CategoryOpenable);
                    intent.SetType("*/*");
#pragma warning disable CS0618
                    StartActivityForResult(intent, (int)(state ? AndroidDocumentRequest.StateImport : AndroidDocumentRequest.SaveImport));
#pragma warning restore CS0618
                }
                catch (Exception error) { _ = ShowStateResult(Task.FromException<string>(error)); }
            })!
            .SetNegativeButton("Cancel", (_, _) => { menuOpen = false; ShowTestingMenu(); })!
            .SetCancelable(false)!.Show();
    }

    private async Task<string> ImportDocument(global::Android.Net.Uri uri, bool state)
    {
        string temporary = Path.Combine(CacheDir!.AbsolutePath, $"import-{Guid.NewGuid():N}.tmp");
        try
        {
            using (System.IO.Stream source = ContentResolver!.OpenInputStream(uri)
                ?? throw new IOException("Android could not open the selected import."))
            using (System.IO.Stream destination = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write))
                await source.CopyToAsync(destination);
            if (session is null) throw new InvalidOperationException("No active session is available for import.");
            return state ? await session.ImportState(temporary, selectedSlot) : await session.ImportSave(temporary);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }

    private void ShowRomSetup(string? error = null)
    {
        if (destroyed) return;
        menuOpen = true;
        var layout = new LinearLayout(this) { Orientation = global::Android.Widget.Orientation.Vertical };
        int padding = (int)(24 * Resources!.DisplayMetrics!.Density);
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
                using var intent = new Intent(Intent.ActionOpenDocument);
                intent.AddCategory(Intent.CategoryOpenable);
                intent.SetType("*/*");
#pragma warning disable CS0618
                StartActivityForResult(intent, (int)AndroidDocumentRequest.RomImport);
#pragma warning restore CS0618
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
                using System.IO.Stream source = ContentResolver!.OpenInputStream(uri)
                    ?? throw new IOException("Android could not open that file. Choose another local ROM.");
                GameAssetInstaller.Install(source, root, token, progress);
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

    private void ShowIniSettings()
    {
        menuOpen = true;
        try
        {
            string path = Path.Combine(FilesDir!.AbsolutePath, "SuperMetroid.ini");
            var saved = SuperMetroidGameOptionsIni.Parse(File.ReadAllText(path), path);
            var definitions = AndroidSettingDefinitions.All;
            new AlertDialog.Builder(this).SetTitle("Settings — apply on next app launch")!
                .SetItems(definitions.Select(d => $"{d.Label}: {d.Read(saved)}" +
                    (session?.EffectiveOptions is { } active && d.Read(active) != d.Read(saved)
                        ? $" (active: {d.Read(active)})" : "")).ToArray(), (_, selected) =>
                {
                    var definition = definitions[selected.Which];
                    new AlertDialog.Builder(this).SetTitle(definition.Label)!
                        .SetSingleChoiceItems(definition.Values, Array.IndexOf(definition.Values, definition.Read(saved)),
                            (sender, choice) =>
                            {
                                try
                                {
                                    // Re-read just before editing. Preserve all unrelated settings;
                                    // validate before replacing the durable file atomically.
                                    string updated = SuperMetroidGameOptionsIni.WithValue(File.ReadAllText(path),
                                        definition.Section, definition.Key, definition.Values[choice.Which]);
                                    File.WriteAllText(path + ".tmp", updated);
                                    File.Move(path + ".tmp", path, overwrite: true);
                                    ((AlertDialog)sender!).Dismiss();
                                    ShowIniSettings();
                                }
                                catch (Exception error) { _ = ShowStateResult(Task.FromException<string>(error)); }
                            })!
                        .SetNegativeButton("Back", (_, _) => ShowIniSettings())!
                        .SetCancelable(false)!.Show();
                })!
                .SetNegativeButton("Back", (_, _) => { menuOpen = false; ShowTestingMenu(); })!
                .SetCancelable(false)!.Show();
        }
        catch (Exception error) { _ = ShowStateResult(Task.FromException<string>(error)); }
    }

    private bool menuOpen;
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
        new AlertDialog.Builder(this)
            .SetTitle($"Testing tools - slot {DebuggerSaveStateStore.SlotName(selectedSlot)}")!
            .SetItems(new[] { "Resume", "Choose state slot", "Save state", "Load state", "INI settings (next launch)", "Controller bindings", "Export private diagnostics", "Import debugger state", "Import regular save (next launch)" }, (_, args) =>
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
                }
            })!
            .SetOnCancelListener(new MenuCancelled(this))!
            .Show();
    }

    private void ChooseStateSlot()
    {
        menuOpen = true;
        new AlertDialog.Builder(this).SetTitle("Debugger state slot")!
            .SetSingleChoiceItems(Enumerable.Range(0, DebuggerStateFormat.AutomaticSlot + 1).Select(slot => $"Slot {DebuggerSaveStateStore.SlotName(slot)}").ToArray(), selectedSlot,
                (sender, args) =>
                {
                    selectedSlot = args.Which;
                    ((AlertDialog)sender!).Dismiss();
                    menuOpen = false;
                    ShowTestingMenu();
                })!
            .SetOnCancelListener(new MenuCancelled(this))!.Show();
    }

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
        new AlertDialog.Builder(this).SetTitle("Debugger state")!.SetMessage(message)!
            .SetPositiveButton("OK", (_, _) => { menuOpen = false; ShowTestingMenu(); })!
            .SetCancelable(false)!.Show();
    }

    private sealed class MenuCancelled(MainActivity owner) : Java.Lang.Object, global::Android.Content.IDialogInterfaceOnCancelListener
    {
        public void OnCancel(global::Android.Content.IDialogInterface? dialog)
        {
            owner.menuOpen = false;
            owner.RefreshRunGate(requestFocus: true);
        }
    }
}
