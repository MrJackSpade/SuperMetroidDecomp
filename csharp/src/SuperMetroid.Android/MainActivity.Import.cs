using Android.App;
using Android.Content;

namespace SuperMetroid.Android;

public sealed partial class MainActivity
{
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
                    StartActivityForResult(intent, state ? AndroidDocumentRequests.StateImport : AndroidDocumentRequests.SaveImport);
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
            using (Stream source = ContentResolver!.OpenInputStream(uri)
                ?? throw new IOException("Android could not open the selected import."))
            using (Stream destination = new FileStream(temporary, FileMode.CreateNew, FileAccess.Write))
                await source.CopyToAsync(destination);
            if (session is null) throw new InvalidOperationException("No active session is available for import.");
            return state ? await session.ImportState(temporary, selectedSlot) : await session.ImportSave(temporary);
        }
        finally { if (File.Exists(temporary)) File.Delete(temporary); }
    }
}
