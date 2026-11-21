using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// Names the pause frame's pictures (#1467): each area's backdrop without its button rows, the button rows without their
/// labels, and each label lit or dim; and says which labels each highlight lights.
/// </summary>
internal static class PauseFramePictures
{
    /// <summary>The frame name of <paramref name="area"/>'s backdrop, its area lettering included, without the button rows.</summary>
    [InstalledKey]
    internal static string BackdropFrame(AreaId area) => $"Pause.Backdrop.{AreaIdKeys.Installed.Of(area)}";

    /// <summary>The frame name of the button rows without their labels.</summary>
    internal const string ButtonRowsFrame = "Pause.ButtonRows";

    /// <summary>The frame name of <paramref name="label"/>, lit or dim.</summary>
    [InstalledKey]
    internal static string ButtonFrame(PauseButtonLabel label, PauseButtonLabelShade shade) =>
        $"Pause.Button.{PauseButtonLabelKeys.Installed.Of(label)}.{PauseButtonLabelShadeKeys.Installed.Of(shade)}";

    /// <summary>The shade <paramref name="highlight"/> shows <paramref name="label"/> in.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The highlight or label is undefined.</exception>
    internal static PauseButtonLabelShade ShadeOf(PauseButtonHighlight highlight, PauseButtonLabel label) =>
        label == PauseButtonLabel.Exit || label == LitSide(highlight) ? PauseButtonLabelShade.Lit : PauseButtonLabelShade.Dim;

    /// <summary>The lit side of each highlight.</summary>
    private static IdentityTable<PauseButtonHighlight, PauseButtonLabel?> LitSideByHighlight => field ??= new(new Dictionary<PauseButtonHighlight, PauseButtonLabel?>
    {
        [PauseButtonHighlight.MapPage] = PauseButtonLabel.Samus,
        [PauseButtonHighlight.Unpausing] = null,
        [PauseButtonHighlight.EquipmentPage] = PauseButtonLabel.Map,
    });

    /// <summary>The side label <paramref name="highlight"/> lights beside Exit, which every highlight lights; none while unpausing.</summary>
    private static PauseButtonLabel? LitSide(PauseButtonHighlight highlight) => LitSideByHighlight[highlight];
}
