namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The horizontal sub-pixel position each of Samus's shot places hands its next projectile (#1590). A beam fired to the
/// left first checks its muzzle a fraction of a pixel to its left, from the sub-pixel position its projectile starts
/// with; when that is below the step, the beam starts one pixel further left, which moves its whole flight and the frame
/// it reaches an enemy. Every other projectile either sets its sub-pixel position before reading it or never reads it.
/// </summary>
/// <remarks>
/// <para>
/// What a place hands on. When Samus's shots are cleared all at once (a room load, an elevator ride, the intro
/// flashback's reset), each place keeps the sub-pixel position of the projectile holding it, and a place no projectile
/// holds keeps what it was already keeping. The next projectile to take the place starts from what it kept, and the place
/// then keeps nothing more: a projectile deleted from its place leaves zero. A projectile that takes over a place another
/// projectile still holds (a special attack's particles, a crash echo) continues from that projectile's sub-pixel
/// position, so a clear while it holds the place keeps the earlier projectile's fraction; the Super Missile's collision
/// link alone starts from zero.
/// </para>
/// <para>
/// Consulted when a projectile takes a place (<see cref="Start"/>), when Samus's shots are cleared (<see cref="Keep"/>),
/// and across a room load, where the outgoing room's places leave what they keep (<see cref="RoomHandOver"/>) and the next
/// room's start from it before any shot is fired. Lifetime: one room's shot pool, created with it.
/// </para>
/// <para>
/// Evidence: the original fire routines write only the parts of a shot they set, the left muzzle check runs before
/// the launch velocity clears the sub-pixel positions, deletion clears the sub-pixel position and the all-at-once
/// clear does not; the 100% movie diverges in enemy damage at update 23200 without the carried fractions (ef4b79943a).
/// </para>
/// </remarks>
[CompatibilityService("a beam fired to the left starts its muzzle check from the horizontal sub-pixel position its shot place kept from the projectile that held it when Samus's shots were last cleared")]
public sealed class LeftMuzzleSubpixelCarry
{
    /// <summary>The steps of a fraction of a pixel in a sub-pixel position.</summary>
    private const int FractionSteps = 65536;

    /// <summary>What each place keeps for its next projectile, in place order, 0 through 65535.</summary>
    private readonly int[] _kept;

    /// <summary>Starts a pool of places that keep nothing.</summary>
    /// <param name="placeCount">The number of shot places, at least one.</param>
    public LeftMuzzleSubpixelCarry(int placeCount) =>
        _kept = new int[Ensure.BetweenInclusive(1, int.MaxValue, placeCount)];

    /// <summary>What <paramref name="place"/> keeps for its next projectile, 0 through 65535.</summary>
    public int Kept(int place) => _kept[place];

    /// <summary>
    /// <paramref name="place"/> keeps <paramref name="fraction"/> for its next projectile: Samus's shots are being cleared
    /// while a projectile with that sub-pixel position holds it, or a room load hands the place's fraction on.
    /// </summary>
    /// <param name="place">The zero-based shot place.</param>
    /// <param name="fraction">The horizontal sub-pixel position, 0 through 65535.</param>
    public void Keep(int place, int fraction) =>
        _kept[place] = Ensure.BetweenInclusive(0, FractionSteps - 1, fraction);

    /// <summary>
    /// A projectile takes <paramref name="place"/>: returns what the place kept for it, and the place keeps nothing more.
    /// </summary>
    /// <param name="place">The zero-based shot place.</param>
    public int Start(int place)
    {
        int fraction = _kept[place];
        _kept[place] = 0;
        return fraction;
    }
}
