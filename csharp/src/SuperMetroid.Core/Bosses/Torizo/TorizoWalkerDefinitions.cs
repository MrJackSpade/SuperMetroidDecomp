namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>How each Torizo walks (#2335): the turns a wall starts and its wait before it turns toward Samus behind it.</summary>
internal static class TorizoWalkerDefinitions
{
    /// <summary>Bomb Torizo with its face: a wall turns it about, and it waits long before it turns toward Samus.</summary>
    internal static TorizoWalker BombTorizo { get; } =
        new(TorizoAnimation.TurningLeft, TorizoAnimation.TurningRight, TorizoDecisionDefinitions.BombTurnDelay);

    /// <summary>Bomb Torizo once its face has blown up: a wall turns it about faceless.</summary>
    internal static TorizoWalker FacelessBombTorizo { get; } =
        new(TorizoAnimation.FacelessTurningLeft, TorizoAnimation.FacelessTurningRight, TorizoDecisionDefinitions.BombTurnDelay);

    /// <summary>Golden Torizo: a wall turns it about, and it turns toward Samus soon.</summary>
    internal static TorizoWalker GoldenTorizo { get; } =
        new(TorizoAnimation.GoldenTurningLeft, TorizoAnimation.GoldenTurningRight, TorizoDecisionDefinitions.GoldenTurnDelay);
}
