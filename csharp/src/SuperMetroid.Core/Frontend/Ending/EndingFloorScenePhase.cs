namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>A phase run by its own scene whose turning floor shows a shot (#2558): the reward's shot into the camera.</summary>
internal sealed class EndingFloorScenePhase : EndingCreditsPhase, IEndingFloorPhase
{
    /// <summary>A phase named <paramref name="name"/> whose floor shows <paramref name="floorShot"/>.</summary>
    /// <param name="name">The phase's name, for diagnostics.</param>
    /// <param name="floorShot">The shot the turning floor shows in the phase.</param>
    internal EndingFloorScenePhase(string name, EndingFloorShot floorShot)
        : base(name) => FloorShot = floorShot;

    /// <inheritdoc/>
    public EndingFloorShot FloorShot { get; }
}
