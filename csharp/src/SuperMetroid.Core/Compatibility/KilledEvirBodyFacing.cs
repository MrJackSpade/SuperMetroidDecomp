namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The facing an Evir's arms and spit read from their body on the update a hit kills it (#1543): left. The kill's
/// follow-up deletes the arms and spit, but they still run that update, after the body in the update's order; they place
/// themselves beside the killed body, at the room's origin, on its left, and the idle spit aims from there.
/// </summary>
/// <remarks>
/// Consulted whenever the arms or spit read their body's facing, which a living or deleted body answers itself. Evidence:
/// the arms' and spit's routines read the facing in the body's enemy record without testing it, and the kill clears that
/// record, which leaves the arms and the idle spit four pixels left of the origin on that update.
/// </remarks>
[CompatibilityService("an Evir's arms and spit run out the update their body is killed in, reading the killed body as facing left")]
public static class KilledEvirBodyFacing
{
    /// <summary>The facing an Evir's arms or spit read from their body: its own, or the emptied facing once a hit killed it.</summary>
    /// <typeparam name="TFacing">An Evir's facing.</typeparam>
    /// <param name="bodyFacing">The facing the body last had.</param>
    /// <param name="emptied">The facing an emptied body reads as, left.</param>
    /// <param name="bodyKilled">Whether a hit has killed the body.</param>
    public static TFacing Facing<TFacing>(TFacing bodyFacing, TFacing emptied, bool bodyKilled) =>
        bodyKilled ? emptied : bodyFacing;
}
