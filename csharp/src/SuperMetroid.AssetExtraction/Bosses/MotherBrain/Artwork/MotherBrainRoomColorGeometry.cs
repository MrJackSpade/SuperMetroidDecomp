namespace SuperMetroid.AssetExtraction.Bosses.MotherBrain.Artwork;

/// <summary>
/// The shape of Mother Brain's installed room colours, which the import reads, writes and pre-colours her room's
/// pictures in (#1527): the fake-death room flash, the final room, phase two's attack and rear-leg colours, the room-entry
/// glass and tube colours and the phase-three room lights. The game plays only the compiled shades.
/// </summary>
public static class MotherBrainRoomColorGeometry
{
    /// <summary>The colours in each of a room-flash image's two slices.</summary>
    public const int SliceColors = 12;

    /// <summary>The colours of phase two's attack and rear-leg images, after their transparent first colour.</summary>
    public const int PhaseTwoColors = 15;

    /// <summary>The colours of the room-entry glass-shard and tube-projectile images, after their transparent first colour.</summary>
    public const int InitialColors = 15;

    /// <summary>The colours in each of a room-light image's two slices.</summary>
    public const int RecoveryLightsSliceColors = 14;
}
