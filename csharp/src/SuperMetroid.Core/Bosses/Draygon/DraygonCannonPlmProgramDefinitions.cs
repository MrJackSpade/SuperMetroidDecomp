using SuperMetroid.Core.Programs;
using Step = SuperMetroid.Core.Programs.AnimationProgramStep<SuperMetroid.Core.Bosses.Draygon.DraygonCannonProgram, SuperMetroid.Core.Bosses.Draygon.DraygonCannonDraw, SuperMetroid.Core.Bosses.Draygon.DraygonCannonCue>;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Draygon's cannon PLM programs (#1296), in place of their bank-$84 instruction lists. A shielded cannon sleeps behind
/// its shield; a missile wakes it into the hit program, which counts the hit and flashes the shield; the third hit
/// destroys it, and a destroyed cannon smokes forever.
/// </summary>
internal static class DraygonCannonPlmProgramDefinitions
{
    /// <summary>How long the idle shield shows before the sleep.</summary>
    private const int ShieldDuration = 8;
    /// <summary>How long the first of each pair of hit-flash images shows.</summary>
    private const int FlashDuration = 3;
    /// <summary>How long the second of each pair of hit-flash images shows.</summary>
    private const int FlashAlternateDuration = 4;
    /// <summary>How long each smoke image shows.</summary>
    private const int SmokeDuration = 6;

    /// <summary>Every cannon program.</summary>
    internal static AnimationProgramSet<DraygonCannonProgram, DraygonCannonDraw, DraygonCannonCue> Programs { get; } = new(
        new Dictionary<DraygonCannonProgram, Step[]>
        {
            [DraygonCannonProgram.RightStart] = Start(DraygonCannonProgram.RightIdle),
            [DraygonCannonProgram.RightIdle] = Idle(DraygonCannonDraw.RightShieldA),
            [DraygonCannonProgram.RightHit] = Hit(DraygonCannonProgram.RightIdle, DraygonCannonDraw.RightShieldA, DraygonCannonDraw.RightShieldB),
            [DraygonCannonProgram.RightDestroy] = Destroy(DraygonCannonProgram.RightSmoke),
            [DraygonCannonProgram.RightSmoke] = Smoke(DraygonCannonProgram.RightSmoke,
                DraygonCannonDraw.RightDamagedA, DraygonCannonDraw.RightDamagedB, DraygonCannonDraw.RightDamagedC, DraygonCannonDraw.RightDamagedD),
            [DraygonCannonProgram.LeftStart] = Start(DraygonCannonProgram.LeftIdle),
            [DraygonCannonProgram.LeftIdle] = Idle(DraygonCannonDraw.LeftShieldA),
            [DraygonCannonProgram.LeftHit] = Hit(DraygonCannonProgram.LeftIdle, DraygonCannonDraw.LeftShieldA, DraygonCannonDraw.LeftShieldB),
            [DraygonCannonProgram.LeftDestroy] = Destroy(DraygonCannonProgram.LeftSmoke),
            [DraygonCannonProgram.LeftSmoke] = Smoke(DraygonCannonProgram.LeftSmoke,
                DraygonCannonDraw.LeftDamagedA, DraygonCannonDraw.LeftDamagedB, DraygonCannonDraw.LeftDamagedC, DraygonCannonDraw.LeftDamagedD),
        });

    /// <summary>Whether <paramref name="program"/> belongs to the right-facing cannon.</summary>
    internal static bool FacesRight(DraygonCannonProgram program) => program switch
    {
        DraygonCannonProgram.RightStart or DraygonCannonProgram.RightIdle or DraygonCannonProgram.RightHit
            or DraygonCannonProgram.RightDestroy or DraygonCannonProgram.RightSmoke => true,
        DraygonCannonProgram.LeftStart or DraygonCannonProgram.LeftIdle or DraygonCannonProgram.LeftHit
            or DraygonCannonProgram.LeftDestroy or DraygonCannonProgram.LeftSmoke => false,
        _ => throw new InvalidOperationException($"Undefined {nameof(DraygonCannonProgram)} {program}."),
    };

    /// <summary>The first program of an intact cannon facing <paramref name="orientation"/>.</summary>
    internal static DraygonCannonProgram StartProgram(DraygonCannonOrientation orientation) => orientation switch
    {
        DraygonCannonOrientation.Right => DraygonCannonProgram.RightStart,
        DraygonCannonOrientation.Left => DraygonCannonProgram.LeftStart,
        _ => throw new InvalidOperationException($"Undefined {nameof(DraygonCannonOrientation)} {orientation}."),
    };

    /// <summary>The first program of a cannon facing <paramref name="orientation"/> that spawns already destroyed.</summary>
    internal static DraygonCannonProgram DestroyProgram(DraygonCannonOrientation orientation) => orientation switch
    {
        DraygonCannonOrientation.Right => DraygonCannonProgram.RightDestroy,
        DraygonCannonOrientation.Left => DraygonCannonProgram.LeftDestroy,
        _ => throw new InvalidOperationException($"Undefined {nameof(DraygonCannonOrientation)} {orientation}."),
    };

    /// <summary>Links the hit program, arms the missile wake and runs into the idle shield.</summary>
    private static Step[] Start(DraygonCannonProgram idle) =>
        [Step.Publish(DraygonCannonCue.LinkHit), Step.Publish(DraygonCannonCue.ArmMissileWake), Step.FallInto(idle)];

    /// <summary>Shows the shield and sleeps until a missile wakes the cannon at its hit program.</summary>
    private static Step[] Idle(DraygonCannonDraw shield) => [Step.Show(shield, ShieldDuration), Step.Hold];

    /// <summary>Counts the hit, flashes the shield three times and returns to the idle shield.</summary>
    private static Step[] Hit(DraygonCannonProgram idle, DraygonCannonDraw shield, DraygonCannonDraw flash) =>
    [
        Step.Publish(DraygonCannonCue.CountHit),
        Step.Show(shield, FlashDuration), Step.Show(flash, FlashAlternateDuration),
        Step.Show(shield, FlashDuration), Step.Show(flash, FlashAlternateDuration),
        Step.Show(shield, FlashDuration), Step.Show(flash, FlashAlternateDuration),
        Step.GoTo(idle),
    ];

    /// <summary>Destroys the cannon and runs into its smoke.</summary>
    private static Step[] Destroy(DraygonCannonProgram smoke) => [Step.Publish(DraygonCannonCue.Damage), Step.FallInto(smoke)];

    /// <summary>Loops the four smoke images forever.</summary>
    private static Step[] Smoke(DraygonCannonProgram self, DraygonCannonDraw a, DraygonCannonDraw b, DraygonCannonDraw c, DraygonCannonDraw d) =>
        [Step.Show(a, SmokeDuration), Step.Show(b, SmokeDuration), Step.Show(c, SmokeDuration), Step.Show(d, SmokeDuration), Step.GoTo(self)];
}
