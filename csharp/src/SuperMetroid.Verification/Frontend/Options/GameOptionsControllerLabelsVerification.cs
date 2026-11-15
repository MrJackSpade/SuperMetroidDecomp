using SuperMetroid.Core.Frontend.Options;
using SuperMetroid.Core.Frontend.Options.Artwork;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Verification.Frontend.Options;

/// <summary>
/// Checks the controller page's labels (#1985): each is pictured under its action's name and the bound button's, and a
/// binding no label names fails loudly instead of showing another button's label.
/// </summary>
internal static class GameOptionsControllerLabelsVerification
{
    /// <summary>Checks the label pictures' names, the page's labels for the default bindings and an unassignable binding.</summary>
    internal static void VerifyGameOptionsControllerLabels()
    {
        AssertEqual("Options.Controller.Shoot.X", GameOptionsNames.ControllerLabelFrame(ControllerAction.Shoot, ControllerButton.X),
            "a label is pictured under its action's name");
        AssertEqual("Options.Controller.AimDown.Select", GameOptionsNames.ControllerLabelFrame(ControllerAction.AimDown, ControllerButton.Select),
            "the last row's label is pictured under its action's name");
        AssertThrows<ArgumentOutOfRangeException>(() => GameOptionsNames.ControllerLabelFrame(ControllerAction.Jump, ControllerButton.Start),
            "Start has no label picture");

        var page = new ShownOptionsPage(GameOptionsScreen.ControllerEnglish, false, ControllerBindings.Default, false, false);
        string[] expected =
        [
            GameOptionsNames.PageFrame(GameOptionsScreen.ControllerEnglish),
            "Options.Controller.Shoot.X", "Options.Controller.Jump.A", "Options.Controller.Dash.B",
            "Options.Controller.ItemSelect.Select", "Options.Controller.ItemCancel.Y", "Options.Controller.AimUp.R",
            "Options.Controller.AimDown.L",
        ];
        AssertTrue(page.Frames().SequenceEqual(expected), "the page shows each action's label for its bound button, top to bottom");

        ShownOptionsPage unassignable = page with { Bindings = ControllerBindings.Default with { Dash = ControllerButton.Start } };
        AssertThrows<InvalidDataException>(() => unassignable.Frames(), "a binding no label names fails instead of showing X's label");
        Console.WriteLine("Options controller labels: pictured by action and button; an unassignable binding fails loudly.");
    }
}
