namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>How the dead Torizo's corpse rots.</summary>
internal static class DeadTorizoCorpseDefinitions
{
    /// <summary>The corpse's height in pixel rows, each of which its rot schedule drops.</summary>
    internal const int RowCount = 96;

    /// <summary>The pixel rows of the corpse's sand heap.</summary>
    internal const int SandRowCount = 16;

    /// <summary>The lines of sand that fall, one row each, from the heap's last row up.</summary>
    internal const int SandLines = 15;

    /// <summary>The rot updates between one line of sand falling and the next.</summary>
    internal const int RotUpdatesPerSandLine = 15;

    /// <summary>The speed the corpse falls at when it starts to rot: a thirty-second of a pixel an update.</summary>
    internal static SignedEightEight StartingFallSpeed { get; } = SignedEightEight.FromPixelFraction(1, 32);

    /// <summary>How much the rotting corpse's fall speeds up each update: the finest change a speed keeps.</summary>
    internal static SignedEightEight RotGravity { get; } = SignedEightEight.FromPixelFraction(1, 256);

    /// <summary>The room point each fallen row's dust cloud is spread right from, whichever room the corpse lies in.</summary>
    internal static readonly Position16<RoomPixels> DustOrigin = new(272, 188);
}
