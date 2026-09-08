using Android.App;
using Android.OS;
using Android.Widget;

namespace SuperMetroid.Android;

/// <summary>
/// Android entry point, separate from the Windows Forms executable. The first milestone
/// verifies deployment and private asset installation before attaching the game session.
/// </summary>
[Activity(Label = "Super Metroid C# Testing", MainLauncher = true, Exported = true)]
public sealed class MainActivity : Activity
{
    protected override void OnCreate(Bundle? savedInstanceState)
    {
        base.OnCreate(savedInstanceState);
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
            status.Text = "Android host ready. Private cartridge and audio assets installed.\nGameplay host integration is in progress.";
        }
        catch (Exception error)
        {
            // Startup failure remains visible and available to adb rather than causing an
            // opaque native crash dialog. Runtime failures will also get durable reports.
            global::Android.Util.Log.Error("SuperMetroid", error.ToString());
            status.Text = error.ToString();
        }
    }
}
