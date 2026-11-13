namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// Whether a Norfair Pipe Bug formation member counts as waiting with the rest (#1543). A formation of five Gamets rises
/// only once all five wait in their pipe; a member killed and not yet respawned never counts as waiting, so it holds the
/// formation back until its respawn sets it up waiting again.
/// </summary>
/// <remarks>
/// Consulted by the formation's leader on every update it waits for its formation, once per member. Evidence: the
/// leader compares the action held in each of the four following enemy records without testing whether a Gamet still
/// occupies the record, and a kill clears the record, action included; the respawned Gamet's setup writes the waiting
/// action again.
/// </remarks>
[CompatibilityService("a Norfair Pipe Bug formation does not rise while one of its members is killed and not yet respawned")]
public static class KilledGametHoldsFormation
{
    /// <summary>Whether a formation member counts as waiting with the rest: a living member as it waits, a killed one never.</summary>
    /// <typeparam name="TMember">The state of one formation member.</typeparam>
    /// <param name="living">The member's state, or null while the member is killed and not yet respawned.</param>
    /// <param name="waits">Whether a living member waits in its pipe.</param>
    public static bool Waits<TMember>(TMember? living, Func<TMember, bool> waits)
        where TMember : class =>
        living is not null && Ensure.NotNull(waits)(living);
}
