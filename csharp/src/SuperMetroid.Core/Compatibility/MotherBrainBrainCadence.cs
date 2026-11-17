namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// When Mother Brain's brain animation steps (#2317). It steps at her brain's draw, which runs once every enemy's AI has
/// run in the update, so its cues (her drool, onion rings, bombs and laser, and the random holds and drool bursts that
/// read the random number) act after the update's enemy AI and its sounds go straight to the enemy sound queue; with time
/// frozen it does not step. An animation her AI starts that opens with a picture shows that picture from the start, the
/// start counting as the picture's first update, so the picture shows at one draw fewer than the updates it states; one
/// that opens with a cue plays the cue, and shows the picture after it, at her brain's next draw.
/// </summary>
/// <remarks>
/// Consulted when her AI starts a brain animation, and at her brain's draw on every update her brain is drawn; a rule
/// with no state, the brain animation it paces being her encounter's. Evidence: the original game runs her brain's
/// list from her brain's draw routine, not from the enemy animation pass, and counts each frame's draws up from one,
/// moving on once the count passes the frame's duration; a start points the list at its first entry with the count at
/// one, as if that entry's frame had already shown once. The brain cadence check (VerifyMotherBrainAnimations) runs both
/// kinds of start against those counts, and the Mother Brain glitch movie's replay compares the random number on every
/// update of her fight.
/// </remarks>
[CompatibilityService("Mother Brain's brain animation steps at her brain's draw, after every enemy's AI, and an animation her AI starts that opens with a picture counts its start as the picture's first update")]
public static class MotherBrainBrainCadence
{
    /// <summary>
    /// Whether an animation her AI starts shows its opening picture at the start: one that opens with a picture does; one
    /// that opens with a cue waits for her brain's next draw.
    /// </summary>
    /// <param name="opensWithPicture">Whether the animation opens with a picture rather than a cue.</param>
    public static bool ShowsOpeningPictureAtStart(bool opensWithPicture) => opensWithPicture;

    /// <summary>Whether her brain's animation steps at this draw of her brain: it does unless time is frozen.</summary>
    /// <param name="timeIsFrozen">Whether time is frozen this update.</param>
    public static bool StepsAtDraw(bool timeIsFrozen) => !timeIsFrozen;
}
