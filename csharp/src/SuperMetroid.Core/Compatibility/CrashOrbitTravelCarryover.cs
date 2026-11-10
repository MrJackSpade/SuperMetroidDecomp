namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// How far a shinespark crash's two orbiting copies have turned around Samus, which the first echo the crash releases as
/// it finishes overwrites with its height while it flies (#1589). The crash's orbit starts from whatever this holds: its
/// entry does not restart it. Each orbiting update adds four, and the copies stop orbiting and close in once it is 128
/// or more, read as the signed difference from 128. A crash after one that released no first echo starts at the 128 or
/// more the last orbit ended on and orbits for a single update; a crash after a first echo that left the screen, or
/// after Samus's shots were cleared, starts from nothing and orbits for its full 32 updates; a first echo still flying
/// as the crash starts, or while it orbits, leaves its height, so an echo at line 124 or lower ends the orbit on its
/// next update and one higher up shortens it. How long the orbit lasts moves the update on which the crash finishes and
/// Samus regains control, so the value is gameplay-observable; where the copies and echoes are drawn is not.
/// </summary>
/// <remarks>
/// <para>
/// Consulted by the crash on every orbiting update, which advances it and tests it. Left by the crash's finish, when it
/// releases its first echo, as Samus's height; by the first echo's update in the projectile pass, as the echo's height
/// while it stays on the screen and as nothing on the update it leaves; and cleared when Samus's shots are cleared (a
/// room load, the elevator and the opening cinematic's flashbacks). Lifetime: Samus's; a new Samus starts with nothing.
/// </para>
/// <para>
/// Evidence: the original crash keeps its orbit's travel in the vertical position the first released echo is drawn at,
/// and its entry does not initialize it; the echo's update rewrites that position with the echo's height and clears it
/// when the echo leaves the screen, and clearing the projectiles clears it. The crash's angular step shares the echo's
/// horizontal position the same way, but only the drawn copies read it, so the crash keeps its own.
/// </para>
/// </remarks>
[CompatibilityService("a shinespark crash's orbit counts on from the travel the last orbit or its first released echo left, which can shorten the orbit and moves when Samus regains control")]
public sealed class CrashOrbitTravelCarryover
{
    /// <summary>The largest travel held; one more step continues from zero.</summary>
    public const int LargestTravel = 65535;

    /// <summary>The travel at which the copies stop orbiting.</summary>
    private const int OrbitEnd = 128;

    /// <summary>The travel one orbiting update adds.</summary>
    private const int TravelStep = 4;

    /// <summary>
    /// The orbit's travel, 0 through <see cref="LargestTravel"/>: what the last orbiting update made of it, or the height
    /// the first released echo last left, or nothing.
    /// </summary>
    public int Travel { get; private set; }

    /// <summary>
    /// Adds one orbiting update's travel, continuing past <see cref="LargestTravel"/> from zero, and reports whether the
    /// orbit has ended: whether the travel's signed difference from 128 is zero or more, so a travel that continued from
    /// zero, or one far above 128, keeps orbiting until it climbs back to 128.
    /// </summary>
    /// <returns>Whether the copies stop orbiting.</returns>
    public bool Advance()
    {
        int advanced = Travel + TravelStep;
        Travel = advanced > LargestTravel ? advanced - (LargestTravel + 1) : advanced;
        return CoordinateAxis.Difference(Travel, OrbitEnd) >= 0;
    }

    /// <summary>The first released echo leaves its height: Samus's as it departs, then its own each update it moves.</summary>
    /// <param name="height">The room height, 0 through <see cref="LargestTravel"/>.</param>
    public void LeaveEchoHeight(int height) => Travel = Ensure.BetweenInclusive(0, LargestTravel, height);

    /// <summary>The first released echo has left the screen, or Samus's shots have been cleared: the travel is nothing.</summary>
    public void Clear() => Travel = 0;
}
