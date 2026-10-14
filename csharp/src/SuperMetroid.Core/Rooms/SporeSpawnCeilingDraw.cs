namespace SuperMetroid.Core.Rooms;

/// <summary>The four bank-$84 Spore Spawn ceiling draw lists, valued by native address.</summary>
internal enum SporeSpawnCeilingDraw : ushort
{
    /// <summary><c>$84:9413</c>: clear the ceiling after defeat or crumble.</summary>
    Clear = 0x9413,
    /// <summary><c>$84:9423</c>: first crumble appearance.</summary>
    CrumbleFirst = 0x9423,
    /// <summary><c>$84:9433</c>: second crumble appearance.</summary>
    CrumbleSecond = 0x9433,
    /// <summary><c>$84:9443</c>: third crumble appearance.</summary>
    CrumbleThird = 0x9443,
}
