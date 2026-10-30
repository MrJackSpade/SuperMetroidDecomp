using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// Names the pause frame's pictures (#1467): each area's backdrop without its button rows, the button rows without their
/// labels, and each label lit or dim; and says which labels each highlight lights.
/// </summary>
internal static class PauseFramePictures
{
    /// <summary>The frame name of <paramref name="area"/>'s backdrop, its area lettering included, without the button rows.</summary>
    internal static string BackdropFrame(AreaId area) => $"Pause.Backdrop.{area}";

    /// <summary>The frame name of the button rows without their labels.</summary>
    internal const string ButtonRowsFrame = "Pause.ButtonRows";

    /// <summary>The frame name of <paramref name="label"/>, lit or dim.</summary>
    internal static string ButtonFrame(PauseButtonLabel label, PauseButtonLabelShade shade) => $"Pause.Button.{label}.{shade}";

    /// <summary>The shade <paramref name="highlight"/> shows <paramref name="label"/> in.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The highlight or label is undefined.</exception>
    internal static PauseButtonLabelShade ShadeOf(PauseButtonHighlight highlight, PauseButtonLabel label) =>
        Enum.IsDefined(label) ? label == PauseButtonLabel.Exit || label == LitSide(highlight) ? PauseButtonLabelShade.Lit : PauseButtonLabelShade.Dim
            : throw new ArgumentOutOfRangeException(nameof(label), label, "Undefined pause button label.");

    /// <summary>The side label <paramref name="highlight"/> lights beside Exit, which every highlight lights; none while unpausing.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The highlight is undefined.</exception>
    private static PauseButtonLabel? LitSide(PauseButtonHighlight highlight) => highlight switch
    {
        PauseButtonHighlight.MapPage => PauseButtonLabel.Samus,
        PauseButtonHighlight.Unpausing => null,
        PauseButtonHighlight.EquipmentPage => PauseButtonLabel.Map,
        _ => throw new ArgumentOutOfRangeException(nameof(highlight), highlight, "Undefined pause highlight."),
    };
}
