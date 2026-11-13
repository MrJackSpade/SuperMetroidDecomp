namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How the shot or bomb that destroys a destroyable vertical shutter still sets it off (#1543). The shutter's reaction
/// runs after the hit's damage even when the damage destroyed it, and finds the destroyed shutter at rest wherever it
/// was moving: a one-shot shutter that has reacted before stays silent, and any other starts moving and requests its
/// activation sound when its killed place, the room's top-left corner, lies on screen.
/// </summary>
/// <remarks>
/// Consulted once per destroying hit, by the shutter's shot and bomb reactions after the common damage. Evidence: the
/// destroyable shutter's shot reaction calls the common shot damage, which on a kill clears the shutter's enemy record
/// (its action and position among it), and then runs the shutter reaction on that record without testing whether the
/// shutter died.
/// </remarks>
[CompatibilityService("the hit that destroys a destroyable shutter still sets it off, as a shutter at rest at the room's top-left corner")]
public static class DestroyedShutterReaction
{
    /// <summary>Whether a shutter's reaction finds it already moving: never the shutter its own hit destroyed.</summary>
    /// <param name="moving">Whether the shutter was moving when the hit struck it.</param>
    /// <param name="healthAfterHit">The shutter's health after the hit's damage; zero when the hit destroyed it.</param>
    public static bool FindsMoving(bool moving, int healthAfterHit) => moving && healthAfterHit != 0;
}
