namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Cartridge values of <see cref="DraygonIntroDanceDefinitions"/> that only verification reads: where the dance's
/// delays and path are and how the cartridge lays them out (#1473). The game holds start delays in updates and the
/// path as typed steps.
/// </summary>
internal static class DraygonIntroDanceDefinitionsConstants
{
    /// <summary>
    /// The four reachable words of <c>MovementLatencyForEachEvirSpriteObject</c> at
    /// <c>$A5:A19F-$A5:A1A6</c>, ordered by sprite slots 28 through 31: each Evir's start delay as a negative byte
    /// distance into the path.
    /// </summary>
    /// <remarks>The native loop at $A5:A13E indexes with 2*(slot-28).</remarks>
    public const int NativeMovementLatencyAddress = 0xa5a19f;
    /// <summary>Native start of <c>DraygonFightIntroDanceData</c> at <c>$A5:CE07</c>.</summary>
    public const int NativeMovementStreamAddress = 0xa5ce07;

    /// <summary>The bytes of one path record; the signed X and Y movement are its first two.</summary>
    public const int NativeMovementRecordBytes = 4;

    /// <summary>The byte both coordinates of a delete record hold.</summary>
    public const byte NativeDeleteByte = 0x80;
}
