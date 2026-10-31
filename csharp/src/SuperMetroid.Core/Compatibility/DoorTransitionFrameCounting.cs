namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How a door transition counts frames on the short frame counter: from the update that places Samus for the new room's
/// tiles through the update that finishes the door's scroll, every update leaves the short counter at zero, while the
/// long counter keeps counting. The first update after the transition hands the screen back therefore sees the short
/// counter at one, and every enemy, projectile or minimap blink that samples it starts from there. The original program
/// produced this by drawing each scrolling frame with a routine that also cleared the short counter's neighbouring
/// word, but the count it leaves is gameplay-observable.
/// </summary>
/// <remarks>
/// Started when the transition places Samus for tile loading, consulted on every update of the transition after that
/// (blank and scrolling updates alike, after the update's frame counters advance), and finished on the update that
/// returns the screen to the destination room. Evidence: the door scroll's drawing routines (<c>$80:97BA</c>,
/// <c>$80:9823</c>) store a word of zero over the short counter; the Ridley full-movie check reads the counter at zero
/// through the scroll's 61 updates and at one on the first destination drawing update.
/// </remarks>
[CompatibilityService("a door's scroll restarts the short frame counter on every update from Samus's placement to the scroll's end")]
public sealed class DoorTransitionFrameCounting
{
    /// <summary>Whether the transition's scroll is restarting the short counter on each update.</summary>
    public bool RestartsShortCounter { get; private set; }

    /// <summary>Starts restarting the short counter: the transition has placed Samus for the new room's tiles.</summary>
    public void BeginScroll() => RestartsShortCounter = true;

    /// <summary>Stops restarting the short counter: the transition has handed the screen back to the room.</summary>
    public void EndScroll() => RestartsShortCounter = false;
}
