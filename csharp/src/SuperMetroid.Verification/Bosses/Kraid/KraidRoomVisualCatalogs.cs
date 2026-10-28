using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>The Kraid and Maridia elevatube visual catalogs a Kraid draw check installs (#633).</summary>
internal readonly record struct KraidRoomVisualCatalogs
{
    /// <summary>Kraid.</summary>
    public required InstalledRoomPlmKraidVisuals? Kraid { get; init; }

    /// <summary>Tube.</summary>
    public required InstalledRoomPlmMaridiaElevatubeVisuals Tube { get; init; }
}
