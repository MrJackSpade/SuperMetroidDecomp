using Android.App;
using SuperMetroid.Core.Frontend;

namespace SuperMetroid.Android;

public sealed partial class MainActivity
{
    private void ShowIniSettings()
    {
        menuOpen = true;
        try
        {
            string path = Path.Combine(FilesDir!.AbsolutePath, "SuperMetroid.ini");
            var saved = SuperMetroidGameOptionsIni.Parse(File.ReadAllText(path), path);
            var definitions = AndroidSettingDefinitions.All;
            new AlertDialog.Builder(this).SetTitle("Settings — apply on next app launch")!
                .SetItems(definitions.Select(d => $"{d.Label}: {d.Read(saved)}").ToArray(), (_, selected) =>
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
}
