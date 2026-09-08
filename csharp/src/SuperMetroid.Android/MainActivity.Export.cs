using Android.App;
using Android.Content;

namespace SuperMetroid.Android;

public sealed partial class MainActivity
{
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
                    StartActivityForResult(intent, AndroidDocumentRequests.DiagnosticExport);
#pragma warning restore CS0618
                }
                catch (Exception error) { _ = ShowStateResult(Task.FromException<string>(error)); }
            })!
            .SetNegativeButton("Cancel", (_, _) => { menuOpen = false; ShowTestingMenu(); })!
            .SetCancelable(false)!.Show();
    }

#pragma warning disable CS0672, CS0618 // Paired with the document request above; no AndroidX dependency.
    protected override void OnActivityResult(int requestCode, Result resultCode, Intent? data)
    {
        base.OnActivityResult(requestCode, resultCode, data);
        if (requestCode is not (AndroidDocumentRequests.DiagnosticExport or AndroidDocumentRequests.StateImport or AndroidDocumentRequests.SaveImport)) return;
        if (resultCode != Result.Ok || data?.Data is not { } uri)
        {
            menuOpen = false;
            ShowTestingMenu();
            return;
        }
        _ = ShowStateResult(requestCode == AndroidDocumentRequests.DiagnosticExport
            ? ExportDiagnostics(uri) : ImportDocument(uri, requestCode == AndroidDocumentRequests.StateImport));
    }
#pragma warning restore CS0672, CS0618

    private async Task<string> ExportDiagnostics(global::Android.Net.Uri uri)
    {
        string temporary = Path.Combine(CacheDir!.AbsolutePath, $"diagnostics-{Guid.NewGuid():N}.zip");
        try
        {
            if (session is null) throw new InvalidOperationException("No game session is available for export.");
            await session.ExportDiagnostics(temporary, selectedSlot);
            using Stream destination = ContentResolver!.OpenOutputStream(uri, "wt")
                ?? throw new IOException("Android could not open the selected export document.");
            using Stream source = File.OpenRead(temporary);
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
}
