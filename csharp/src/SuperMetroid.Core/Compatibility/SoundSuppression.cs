namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Whether the game turns an admitted sound request away as it is made, part of <see cref="SoundQueueAdmission"/>'s rule
/// (#1620): the sound queues test, after a request's priority, whether sounds are disabled (the door transition's switch),
/// whether an attract demo is playing and whether a power bomb is exploding, and queue nothing while any of them holds.
/// </summary>
/// <remarks>Evidence: the original's three sound-queue routines, as <see cref="SoundQueueAdmission"/> records.</remarks>
/// <param name="SoundsDisabled">Whether the door transition's switch disables new sounds.</param>
/// <param name="AttractDemoPlaying">Whether an attract demo drives the game.</param>
/// <param name="PowerBombExploding">Whether a power bomb's explosion, or Crystal Flash's, is under way.</param>
[CompatibilityService("whether the game turns an admitted sound request away as it is made")]
[IndependentFlag("the disable-sounds switch, an attract demo and an exploding power bomb are separate states the sound queues each test")]
public readonly record struct SoundSuppression(bool SoundsDisabled, bool AttractDemoPlaying, bool PowerBombExploding)
{
    /// <summary>Whether any of the tested states turns the request away.</summary>
    public bool TurnsSoundsAway => SoundsDisabled || AttractDemoPlaying || PowerBombExploding;
}
