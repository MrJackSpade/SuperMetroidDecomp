namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Which enemies a composite enemy's follow-up to a hit reaches (#1500): the enemies placed directly after the part
/// that was hit, whichever part that is. A hit on a Dragon passes its hurt and freeze timers to the next enemy, or
/// deletes it when the hit kills; a hit on an Evir freezes or deletes the next two. Touches and shots reach only the
/// body, whose followers are its own wing, arms and spit, but a power bomb also hits the intangible trailing parts, so
/// its follow-up reaches the enemies placed after them: the next Dragon's body, a Geruta, a Sova, a Hibashi or a
/// Squeept after a Dragon wing, and the next Evir's body after an Evir's arms, which a kill deletes.
/// </summary>
/// <remarks>
/// Consulted by the Dragon's and the Evir's follow-up after the common touch, shot or power-bomb reaction, once per hit.
/// Evidence: the follow-ups at <c>$A2:E7DA</c> and <c>$A8:8B16</c> write the records one and two slots after the
/// struck enemy's slot without testing which part was struck, and the power-bomb sweep at <c>$A0:A306</c> skips no
/// intangible enemy. In every retail Dragon and Evir population the trailing parts lie within the enemy slots: the last
/// Dragon wing occupies slot 18 and the last Evir arms slot 7, so the recipients never pass the final slot, and asking
/// past it is refused.
/// </remarks>
[CompatibilityService("a composite enemy's follow-up to a hit reaches the enemies placed after the part that was hit, so a power bomb hitting a trailing part carries it to the next enemies")]
public static class FollowUpAfterStruckPart
{
    /// <summary>The <paramref name="reach"/> slots placed directly after the struck slot, nearest first.</summary>
    /// <typeparam name="TSlot">An enemy slot.</typeparam>
    /// <param name="slots">Every enemy slot, in placement order.</param>
    /// <param name="struckSlot">The zero-based slot of the part that was hit.</param>
    /// <param name="reach">How many following slots the follow-up reaches, one or more.</param>
    /// <exception cref="ArgumentOutOfRangeException">The struck slot is not a slot, or the follow-up would reach past the final slot.</exception>
    public static IReadOnlyList<TSlot> Recipients<TSlot>(IReadOnlyList<TSlot> slots, int struckSlot, int reach)
    {
        Ensure.NotNull(slots);
        _ = Ensure.BetweenInclusive(1, int.MaxValue, reach);
        if ((uint)struckSlot >= (uint)slots.Count || slots.Count - 1 - struckSlot < reach)
        {
            throw new ArgumentOutOfRangeException(nameof(struckSlot), struckSlot,
                $"A follow-up reaching {reach} slots past slot {struckSlot} passes the final of {slots.Count} enemy slots.");
        }
        return [.. Enumerable.Range(struckSlot + 1, reach).Select(slot => slots[slot])];
    }
}
