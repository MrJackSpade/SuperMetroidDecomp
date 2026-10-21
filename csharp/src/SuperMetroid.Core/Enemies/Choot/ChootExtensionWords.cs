namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>
/// The Choot values kept per physical slot for the room rather than per occupant, like the native per-slot RAM they
/// model: values an initializer does not write keep the previous occupant's until room load.
/// </summary>
internal sealed class ChootExtensionWords
{
    /// <summary>Population positions restored after the final falling-path loop.</summary>
    internal Position16<RoomPixels> SpawnPosition { get; set; }

    /// <summary>Calculated apex position where the Choot begins its falling pattern.</summary>
    internal Position16<RoomPixels> InitialFallingPosition { get; set; }

    /// <summary>Current origin advanced by the pattern's Y distance after each terminator.</summary>
    internal Position16<RoomPixels> FallingOrigin { get; set; }

    /// <summary>Ascent quadratic-speed record index restored for each subsequent activation.</summary>
    internal int InitialYSpeedTableIndex { get; set; }

    /// <summary>Parameter-two delay remaining before each armed Choot jumps.</summary>
    internal WrappingCountdown JumpDelayTimer { get; set; }
}
