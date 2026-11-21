namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>How far Bomb Torizo's body has broken (#2560), in the order his blow-ups come; his death breaks it whole.</summary>
internal enum TorizoBreakage
{
    /// <summary>His body is whole.</summary>
    Intact,
    /// <summary>His gut is blown up.</summary>
    GutBlownUp,
    /// <summary>His gut and his face are blown up.</summary>
    GutAndFaceBlownUp,
}
