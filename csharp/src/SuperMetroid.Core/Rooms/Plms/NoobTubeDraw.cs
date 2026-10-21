namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>The seven n00b-tube draw lists.</summary>
internal enum NoobTubeDraw
{
    /// <summary>Intact projectile-trigger block.</summary>
    Intact,
    /// <summary>First damaged origin block.</summary>
    Damaged,
    /// <summary>Opened origin block.</summary>
    Opened,
    /// <summary>Four-row cleared tube.</summary>
    Cleared,
    /// <summary>Late broken-tube panel.</summary>
    BrokenLate,
    /// <summary>Three-row opened tube.</summary>
    OpenedRows,
    /// <summary>Full broken-tube panel.</summary>
    BrokenFull,
}
