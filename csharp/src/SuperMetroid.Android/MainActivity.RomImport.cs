using Android.App;
using Android.Content;
using Android.Widget;
using SuperMetroid.AssetExtraction;

namespace SuperMetroid.Android;

public sealed partial class MainActivity
{
    private void ShowRomSetup(string? error = null)
    {
        if (destroyed) return;
        menuOpen = true;
        var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };
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
                StartActivityForResult(intent, AndroidDocumentRequests.RomImport);
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
                using Stream source = ContentResolver!.OpenInputStream(uri)
                    ?? throw new IOException("Android could not open that file. Choose another local ROM.");
                GameAssetInstaller.Install(source, root, token, progress);
            }, token);
            if (!destroyed) StartInstalledGame(root);
        }
        catch (OperationCanceledException) when (destroyed) { }
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
}
