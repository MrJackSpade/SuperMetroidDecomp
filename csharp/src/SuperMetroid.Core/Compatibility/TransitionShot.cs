namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The shot a pose transition hands to the next update's weapon firing (#2562): a normal jump started on a fresh Shoot
/// press hands the new pose's direction, and a turn out of a moonwalk hands the moonwalk's. A pose transition hands it after
/// the update's weapon firing has run; the next update's weapon firing and the HUD's beam producer fire it, and that
/// update's input phase drops it after its projectiles, whether or not a shot was fired.
/// </summary>
/// <remarks>
/// Evidence: the game's jump and turn transitions write the shot direction its pose handling leaves for the weapon firing,
/// which its next HUD producer reads, and its input phase clears that direction right after it handles the projectiles.
/// Lifetime: Samus's.
/// </remarks>
/// <typeparam name="TShot">The shot handed: its direction and the pose it was fired from.</typeparam>
public sealed partial class TransitionShot<TShot>
    where TShot : struct
{
    /// <summary>The handed shot, or null when no transition handed one.</summary>
    public TShot? Handed { get; private set; }

    /// <summary>A jump or turn transition hands <paramref name="shot"/> to the next update's weapon firing.</summary>
    internal void Hand(TShot shot) => Handed = shot;

    /// <summary>A transition hands the shot it chose, or none.</summary>
    internal void Hand(TShot? shot) => Handed = shot;

    /// <summary>The input phase drops the handed shot after its projectiles, or a cinematic's Samus does as it starts.</summary>
    internal void Drop() => Handed = null;
}
