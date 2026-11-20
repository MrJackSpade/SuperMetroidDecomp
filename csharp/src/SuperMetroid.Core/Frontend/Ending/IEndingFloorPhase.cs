namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// An ending phase whose turning floor shows a shot (#1515, #2558): each escape scene's clouds from its setup through its
/// fade-out, the exploding planet from its fade-in through its hold, the gunship flying away from the explosion's
/// animation through the fade to the credits, and the reward icon while the reward's shot turns into the camera. A phase
/// before the first scene, through the credits or in a post-credits scene that shows no floor is no such phase.
/// </summary>
internal interface IEndingFloorPhase
{
    /// <summary>The shot the turning floor shows in the phase.</summary>
    EndingFloorShot FloorShot { get; }
}
