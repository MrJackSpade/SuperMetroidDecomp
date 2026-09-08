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
public sealed class MainActivity : Activity
{
    private AndroidGameSession? session;
    private bool resumed;
    private bool focused;
    private bool destroyed;

    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
        ActionBar?.Hide();
        var status = new TextView(this) { Text = "Installing private testing assets…" };
        SetContentView(status);
        _ = PrepareAssets(status);
    }

    private async Task PrepareAssets(TextView status)
    {
        try
        {
            string root = FilesDir?.AbsolutePath ?? throw new IOException("Android did not provide private storage.");
            await Task.Run(() => AndroidAssetInstaller.Install(Assets!, "game", Path.Combine(root, "game")));
            if (destroyed) return;
            var view = new AndroidGameView(this);
            SetContentView(view);
            session = new AndroidGameSession(root, view);
            session.SetActive(resumed && focused);
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
        session?.SetActive(focused);
    }

    protected override void OnPause()
    {
        resumed = false;
        session?.SetActive(false);
        base.OnPause();
    }

    public override void OnWindowFocusChanged(bool hasFocus)
    {
        base.OnWindowFocusChanged(hasFocus);
        focused = hasFocus;
        session?.SetActive(resumed && focused);
    }

    public override bool DispatchKeyEvent(KeyEvent? e)
    {
        if (e is not null && session is not null)
        {
            session.RecordInput($"{e.KeyCode} {e.Action} resumed={resumed} focused={focused}");
            SnesButton mapped = AndroidControllerMapping.Map(e.KeyCode);
            if (mapped != SnesButton.None)
            {
                if (resumed && focused && e.Action is KeyEventActions.Down or KeyEventActions.Up)
                    session.Input.Set((int)e.KeyCode, e.Action == KeyEventActions.Down ? mapped : SnesButton.None);
                return true;
            }
        }
        return base.DispatchKeyEvent(e);
    }

    protected override void OnDestroy()
    {
        destroyed = true;
        if (session is not null) _ = session.Stop();
        base.OnDestroy();
    }
}
