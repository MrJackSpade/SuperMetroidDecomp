using Android.App;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Android;

public sealed partial class MainActivity
{
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
}
