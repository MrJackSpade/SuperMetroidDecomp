namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The seven bank-$84 n00b-tube draw lists, valued by their native address.</summary>
internal enum NoobTubeDraw : ushort
{
    /// <summary>Intact projectile-trigger block at $84:98D1.</summary>
    Intact = 0x98d1,
    /// <summary>First damaged origin block at $84:98D7.</summary>
    Damaged = 0x98d7,
    /// <summary>Opened origin block at $84:98DD.</summary>
    Opened = 0x98dd,
    /// <summary>Four-row cleared tube at $84:98E3.</summary>
    Cleared = 0x98e3,
    /// <summary>Late broken-tube panel at $84:9953.</summary>
    BrokenLate = 0x9953,
    /// <summary>Three-row opened tube at $84:9991.</summary>
    OpenedRows = 0x9991,
    /// <summary>Full broken-tube panel at $84:99E5.</summary>
    BrokenFull = 0x99e5,
}
