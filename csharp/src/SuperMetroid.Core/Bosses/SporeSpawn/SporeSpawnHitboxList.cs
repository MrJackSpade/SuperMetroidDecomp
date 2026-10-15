namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>The twelve bank-$A5 Spore Spawn hitbox lists selected by its collision components.</summary>
internal enum SporeSpawnHitboxList : ushort
{
    /// <summary>$A5:EF73, closed-head hitbox with no active touch response.</summary>
    ClosedHead = 0xef73,
    /// <summary>$A5:EF8D, open-head hitbox.</summary>
    OpenHead = 0xef8d,
    /// <summary>$A5:EFA7, extended open-head hitbox.</summary>
    ExtendedHead = 0xefa7,
    /// <summary>$A5:EFC1, first four-rectangle moving head.</summary>
    MovingHead0 = 0xefc1,
    /// <summary>$A5:EFF3, second four-rectangle moving head.</summary>
    MovingHead1 = 0xeff3,
    /// <summary>$A5:F025, third four-rectangle moving head.</summary>
    MovingHead2 = 0xf025,
    /// <summary>$A5:F057, fourth four-rectangle moving head.</summary>
    MovingHead3 = 0xf057,
    /// <summary>$A5:F139, trailing vulnerable point with Spore Spawn shot AI.</summary>
    TrailingShotPoint = 0xf139,
    /// <summary>$A5:F147, mirrored trailing vulnerable point.</summary>
    MirroredTrailingShotPoint = 0xf147,
    /// <summary>$A5:F155, trailing point with dud-shot AI.</summary>
    TrailingDudPoint = 0xf155,
    /// <summary>$A5:F1A9, fifth four-rectangle moving head.</summary>
    MovingHead4 = 0xf1a9,
    /// <summary>$A5:F1DB, sixth four-rectangle moving head.</summary>
    MovingHead5 = 0xf1db,
}
