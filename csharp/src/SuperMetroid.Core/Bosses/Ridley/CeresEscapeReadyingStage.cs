namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Which stage of readying its warning the Ceres self-destruct stands in (#1406, #1538).</summary>
internal enum CeresEscapeReadyingStage
{
    /// <summary>None: the self-destruct has not begun its warning.</summary>
    None,

    /// <summary>Bringing up the escape timer and warning, which the Ridley arena's backgrounds then show.</summary>
    Warning,

    /// <summary>Readying the arena's backgrounds for the warning text and the doors, which show nothing new.</summary>
    Backgrounds,
}
