using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>
/// The installed key of each <see cref="PauseReserveLabel"/> (#2548): the name the installed pause reserve UI document
/// keys the label by, written out so renaming a member never changes it.
/// </summary>
internal static class PauseReserveLabelKeys
{
    /// <summary>Each PauseReserveLabel member's installed key.</summary>
    internal static InstalledKeys<PauseReserveLabel> Installed { get; } = new(new Dictionary<PauseReserveLabel, string>
    {
        [PauseReserveLabel.Mode] = "Mode",
        [PauseReserveLabel.ReserveTank] = "ReserveTank",
        [PauseReserveLabel.Manual] = "Manual",
        [PauseReserveLabel.Auto] = "Auto",
    });
}
