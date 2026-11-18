namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>What the ending's actors' motions read of the scene they move in (#2489).</summary>
internal interface IEndingActorScene
{
    /// <summary>The ending's current phase.</summary>
    EndingCreditsPhase Phase { get; }

    /// <summary>The turning floor's zoom.</summary>
    FloorZoom FloorZoom { get; }

    /// <summary>Whether the exploding planet's actor is still shown.</summary>
    bool ShowsExplodingPlanet { get; }
}
