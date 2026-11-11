namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The <see cref="WreckedShipGhostFade"/> after one brightening or fade step, and how much the step changed (#1552).
/// Produced on every update of the ghost's brightening, materializing and vanishing phases.
/// </summary>
/// <remarks>
/// Evidence: the original brightening and fade routines for the ghost end on the first step that changes no colour or
/// moves no channel, and the ghost's AI moves on then; the Wrecked Ship ghost replays confirm when each ends.
/// </remarks>
/// <param name="Next">The ghost's colours after the step.</param>
/// <param name="Changes">
/// The colours a brightening step changed, or the channels a fade step moved; none ends the brightening or the fade.
/// </param>
[CompatibilityService("the Wrecked Ship ghost's colours after one brightening or fade step, and how much the step changed, none ending it")]
public readonly record struct WreckedShipGhostFadeStep(WreckedShipGhostFade Next, int Changes);
