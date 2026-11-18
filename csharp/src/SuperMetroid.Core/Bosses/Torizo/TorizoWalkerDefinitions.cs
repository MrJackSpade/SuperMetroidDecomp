namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>
/// How each Torizo walks in each of its states (#2335, #2472): the turn about a wall or its wait behind Samus starts, and
/// that wait's length.
/// </summary>
internal static class TorizoWalkerDefinitions
{
    /// <summary>Bomb Torizo with its face: it turns about, and it waits long before it turns toward Samus.</summary>
    internal static TorizoWalker BombTorizo { get; } =
        new(TorizoAnimation.TurningLeft, TorizoAnimation.TurningRight, TorizoDecisionDefinitions.BombTurnDelay);

    /// <summary>Bomb Torizo once its face has blown up: it turns about faceless.</summary>
    internal static TorizoWalker FacelessBombTorizo { get; } =
        new(TorizoAnimation.FacelessTurningLeft, TorizoAnimation.FacelessTurningRight, TorizoDecisionDefinitions.BombTurnDelay);

    /// <summary>Golden Torizo: it turns about, and it turns toward Samus soon.</summary>
    internal static TorizoWalker GoldenTorizo { get; } =
        new(TorizoAnimation.GoldenTurningLeft, TorizoAnimation.GoldenTurningRight, TorizoDecisionDefinitions.GoldenTurnDelay);
}
