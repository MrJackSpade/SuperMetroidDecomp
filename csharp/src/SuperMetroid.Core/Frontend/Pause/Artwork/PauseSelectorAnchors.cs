namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>
/// Names the pause sheet's frames of the equipment screen's item selector; each control names its own installed anchor
/// (<see cref="PauseEquipmentControl.Anchor"/>).
/// </summary>
public static class PauseSelectorAnchors
{
    /// <summary>The pause sheet's frame for the selector presentation's frame <paramref name="frame"/>.</summary>
    public static string SheetFrameName(string frame) => $"Selector.{frame}";
}
