namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Which run of Ridley's fireball and afterburn poses a frame belongs to.</summary>
internal enum RidleyFireballFrameKind
{
    /// <summary>The two launch poses.</summary>
    Launch,
    /// <summary>The four flight poses.</summary>
    Flight,
    /// <summary>The five dying-afterburn poses.</summary>
    AfterburnFinal,
    /// <summary>The five horizontal-center poses.</summary>
    HorizontalCenter,
    /// <summary>The five vertical-center poses.</summary>
    VerticalCenter,
    /// <summary>The five directional-afterburn poses.</summary>
    Directional,
}
