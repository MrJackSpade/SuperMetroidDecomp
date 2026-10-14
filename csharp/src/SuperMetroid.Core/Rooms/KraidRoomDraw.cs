namespace SuperMetroid.Core.Rooms;

/// <summary>The ten reachable bank-$84 Kraid ceiling and spike draw lists.</summary>
internal enum KraidRoomDraw : ushort
{
    /// <summary><c>$84:9367</c>: first crumble frame, also used by the elevatube.</summary>
    CrumbleFirst = MaridiaElevatubePlmDefinitions.DrawPointer,
    /// <summary><c>$84:936D</c>: second crumble frame.</summary>
    CrumbleSecond = 0x936d,
    /// <summary><c>$84:9373</c>: third crumble frame.</summary>
    CrumbleThird = 0x9373,
    /// <summary><c>$84:9379</c>: ceiling background-one final block.</summary>
    CeilingBackground1 = 0x9379,
    /// <summary><c>$84:937F</c>: ceiling background-two final block.</summary>
    CeilingBackground2 = 0x937f,
    /// <summary><c>$84:9385</c>: ceiling background-three final block.</summary>
    CeilingBackground3 = 0x9385,
    /// <summary><c>$84:9391</c>: spike first-column final block.</summary>
    SpikeFirst = 0x9391,
    /// <summary><c>$84:9397</c>: spike second-column final block.</summary>
    SpikeSecond = 0x9397,
    /// <summary><c>$84:939D</c>: already-defeated ceiling clear, fifteen blocks.</summary>
    ClearCeiling = 0x939d,
    /// <summary><c>$84:93BF</c>: already-defeated spikes clear, twenty-two blocks.</summary>
    ClearSpikes = 0x93bf,
}
