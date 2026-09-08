using Android.App;
using Android.OS;
using Android.Widget;
using Android.Views;
using Android.Content.PM;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Android;

/// <summary>
/// Android entry point, separate from the Windows Forms executable. Installs immutable
/// assets, routes controller events, and gates the game worker on activity/focus lifetime.
/// </summary>
[Activity(Label = "Super Metroid C# Testing", MainLauncher = true, Exported = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize)]
public sealed partial class MainActivity : Activity
{
    private AndroidGameSession? session;
    private bool resumed;
    private bool focused;
    private bool destroyed;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        ActionBar?.Hide();
        EnterImmersiveMode();
        var status = new TextView(this) { Text = "Installing private testing assets…" };
        SetContentView(status);
        _ = PrepareAssets(status);
    }

    private async Task PrepareAssets(TextView status)
    {
        try
        {
            InitializeHostFocus();
            string root = FilesDir?.AbsolutePath ?? throw new IOException("Android did not provide private storage.");
            await Task.Run(() => AndroidAssetInstaller.Install(Assets!, "game", Path.Combine(root, "game")));
            if (destroyed) return;
            LoadControllerPreferences();
            var view = new AndroidGameView(this);
            view.LongClick += (_, _) => ShowTestingMenu();
            SetContentView(view);
            session = new AndroidGameSession(root, view);
            RefreshRunGate(requestFocus: true);
        }
        catch (Exception error)
        {
            // Startup failure remains visible and available to adb rather than causing an
            // opaque native crash dialog. Runtime failures will also get durable reports.
            global::Android.Util.Log.Error("SuperMetroid", error.ToString());
            status.Text = error.ToString();
        }
    }

    protected override void OnResume()
    {
        base.OnResume();
        resumed = true;
        EnterImmersiveMode();
        RefreshRunGate(requestFocus: true);
    }

    protected override void OnPause()
    {
        resumed = false;
        session?.SetActive(false);
        ReleaseAudioFocus();
        base.OnPause();
    }

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

    protected override void OnDestroy()
    {
        destroyed = true;
        DisposeHostFocus();
        if (session is not null) _ = session.Stop();
        base.OnDestroy();
    }
}
