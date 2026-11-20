namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>What each grapple phase does beside moving the beam (<see cref="GrapplePhases"/>).</summary>
internal static class GrapplePhaseDefinitions
{
    /// <summary>
    /// The phases the grapple keeps its sound in: while it fires, swings, holds locked, grabs a wall and lets go of one. It
    /// is silent once it is cancelled and through everything after.
    /// </summary>
    internal static readonly GrapplePhase[] SoundPlaying =
        [GrapplePhase.Firing, GrapplePhase.ConnectedSwinging, GrapplePhase.ConnectedLocked, GrapplePhase.WallGrab, GrapplePhase.WallGrabRelease];
}
