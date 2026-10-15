namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Which run of Ridley's fireball and afterburn poses a frame belongs to.</summary>
internal enum RidleyFireballFrameKind
{
    /// <summary>The two launch poses ($86:9554/955C).</summary>
    Launch,
    /// <summary>The four flight poses ($86:9560..956C).</summary>
    Flight,
    /// <summary>The five dying-afterburn poses ($86:9576..9586).</summary>
    AfterburnFinal,
    /// <summary>The five horizontal-center poses ($86:95A2..95B4).</summary>
    HorizontalCenter,
    /// <summary>The five vertical-center poses ($86:95D5..95E7).</summary>
    VerticalCenter,
    /// <summary>The five directional-afterburn poses ($86:9608..961A).</summary>
    Directional,
}
