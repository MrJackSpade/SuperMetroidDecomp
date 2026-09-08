using Android.App;

namespace SuperMetroid.Android;

public sealed partial class MainActivity
{
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
            .SetTitle($"Testing tools - slot {selectedSlot}")!
            .SetItems(new[] { "Resume", "Choose state slot", "Save state", "Load state", "INI settings (next launch)" }, (_, args) =>
            {
                menuOpen = false;
                switch (args.Which)
                {
                    case 0: session.SetActive(resumed && focused); break;
                    case 1: ChooseStateSlot(); break;
                    case 2: _ = ShowStateResult(session.SaveSlot(selectedSlot)); break;
                    case 3: _ = ShowStateResult(session.LoadSlot(selectedSlot)); break;
                    case 4: ShowIniSettings(); break;
                }
            })!
            .SetOnCancelListener(new MenuCancelled(this))!
            .Show();
    }

    private void ChooseStateSlot()
    {
        menuOpen = true;
        new AlertDialog.Builder(this).SetTitle("Debugger state slot")!
            .SetSingleChoiceItems(Enumerable.Range(0, 10).Select(slot => $"Slot {slot}").ToArray(), selectedSlot,
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
            owner.session?.SetActive(owner.resumed && owner.focused);
        }
    }
}
