using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

internal static partial class Program
{

/// <summary>Baby Metroid cutscene verification and its private fixtures.</summary>
static void VerifyBabyMetroidCutsceneEntrance()
{
    VerifyBabyMetroidRouteDefinitions();
    var bus = new TestAddressSpace();

    // `$A0:B443` is trunc(sin(i*pi/128)*256), stored as a sign-extended word. Seed every
    // possible eight-bit index because the curve visits non-cardinal angles. The first
    // flight assertion below is a literal retail-ROM coordinate/velocity witness, so a
    // future accidental rounding change cannot make production and fixture drift together.
    for (int angle = 0; angle < 256; angle++)
    {
        short sine = unchecked((short)(Math.Sin(angle * Math.PI / 128.0) * 256.0));
        WriteTestWord(bus, 0xa0b443 + angle * 2, unchecked((ushort)sine));
    }

    // The drain producer alternates four retail walk speeds in both directions, then runs
    // the fast crouch. Seed mechanically equivalent bytecode at the literal list addresses;
    // private-ROM integration below separately proves those addresses against cartridge data.
    SeedMotherBrainWalkProgram(bus, 0x9730, duration: 2, forward: true);
    SeedMotherBrainWalkProgram(bus, 0x976a, duration: 4, forward: true);
    SeedMotherBrainWalkProgram(bus, 0x97a4, duration: 6, forward: true);
    SeedMotherBrainWalkProgram(bus, 0x97de, duration: 8, forward: true);
    SeedMotherBrainWalkProgram(bus, 0x9818, duration: 10, forward: true);
    SeedMotherBrainWalkProgram(bus, 0x988c, duration: 2, forward: false);
    SeedMotherBrainWalkProgram(bus, 0x98c6, duration: 4, forward: false);
    SeedMotherBrainWalkProgram(bus, 0x9900, duration: 6, forward: false);
    SeedMotherBrainWalkProgram(bus, 0x9852, duration: 8, forward: false);
    SeedMotherBrainWalkProgram(bus, 0x993a, duration: 10, forward: false);
    SeedMotherBrainCrouchFastProgram(bus);
    // Controller one reads the current `$E9` direction byte, then rebinds the new `$EB`
    // animation pointer without refreshing radii. These are the only Samus ROM fields the
    // entrance consumes; the dedicated drained-controller suite proves their animation.
    WritePoseDefinition(bus, SamusPoseIds.DrainedCrouchingLeftPose,
        [0x04, 0x1b, 0xff, 0xff, 0xfc, 0x00, 0x15, 0x00]);
    WritePoseDefinition(bus, SamusPoseIds.DrainedStandingLeftPose,
        [0x04, 0x1b, 0xff, 0xff, 0xfc, 0x00, 0x15, 0x00]);
    WriteTestWord(bus, 0x91b010 + SamusPoseIds.DrainedStandingLeftPose * 2, 0xc100);
    bus.WriteByte(0x91c100, 0x10);

    var samus = new SamusState
    {
        Pose = SamusPoseIds.DrainedCrouchingLeftPose,
        XPosition = 0x00ca,
        YPosition = 0x00c0,
        // These are the private-ROM runner's post-rainbow values. The Baby must add one
        // point on each `$CA7A` call and stop exactly at 899 rather than using a host fill.
        Health = 200,
        MaxHealth = 899,
        ReserveEnergy = 7,
        MaxReserveEnergy = 99,
    };
    // The real route reached `$E9` through controller zero, which had already loaded the
    // pose radius. Controller one/four intentionally do not refresh it, so initialize that
    // pre-existing WRAM state once rather than widening touch collision in production code.
    samus.RefreshCollisionRadii(bus);
    var motherBrain = new MotherBrainRainbowBeamAttackSequence
    {
        BrainXPosition = 0x0040,
        BrainYPosition = 0x0060,
    };
    motherBrain.Body.XPosition = 0x0040;
    motherBrain.Body.YPosition = 0x0064;
    motherBrain.StartAttackCycle(); // Establishes the pre-existing enabled neck flag.

    var baby = new BabyMetroidCutsceneState();
    baby.Initialize();
    AssertEqual(0x3800, baby.Properties, "Baby population/init property OR");
    AssertEqual(0x0e00, baby.Palette, "Baby cutscene palette");
    AssertEqual(0x00a0, baby.GraphicsOffset, "Baby transferred-tile offset");
    AssertEqual(BabyMetroidCutsceneState.InitialInstructionList, baby.InstructionList,
        "Baby initial instruction list");
    // Whole/subpixel coordinates in native `$0F7A/$0F7C/$0F7E/$0F80` order.
    (ushort, ushort, ushort, ushort) BabyPoint() =>
        (baby.XPosition, baby.XSubposition, baby.YPosition, baby.YSubposition);
    static (ushort, ushort, ushort, ushort) ExpectedBabyPoint(
        ushort xPosition, ushort xSubposition, ushort yPosition, ushort ySubposition) =>
        (xPosition, xSubposition, yPosition, ySubposition);

    AssertEqual(ExpectedBabyPoint(0x0140, 0, 0x0060, 0),
        BabyPoint(),
        "Baby initialization overwrites population coordinates");

    // `$F8` reaches zero without expiring. This is 248 visibly stationary calls, not an
    // approximate four-second host delay.
    for (int call = 1; call <= 248; call++)
        baby.Step(bus, samus, motherBrain);
    AssertEqual(BabyMetroidCutscenePhase.DashOntoScreen, baby.Phase,
        "Baby dash delay retains function at timer zero");
    AssertEqual(0, baby.FunctionTimer, "Baby dash delay exact zero boundary");
    AssertEqual(0x0140, baby.XPosition, "Baby remains still through call 248");
    AssertEqual(0x0060, baby.YPosition, "Baby Y remains still through call 248");

    baby.Step(bus, samus, motherBrain);
    AssertEqual(BabyMetroidCutscenePhase.CurveTowardMotherBrainHead, baby.Phase,
        "Baby call 249 falls through into curve function");
    AssertEqual(0xd680, baby.Angle, "Baby first curve angle");
    AssertEqual(0x0a00, baby.Speed, "Baby first curve speed");
    AssertEqual(0xf772, baby.XVelocity, "Baby first ROM sine X velocity");
    AssertEqual(0x051e, baby.YVelocity, "Baby first ROM cosine Y velocity");
    AssertEqual(ExpectedBabyPoint(0x0137, 0x7200, 0x0065, 0x1e00),
        BabyPoint(),
        "Baby first curve fixed-point displacement");

    BabyMetroidCutsceneStepResult latch = default;
    bool sawBodyStumbleRequest = false;
    bool sawMotherBrainInterrupt = false;
    bool sawLatchSound = false;
    int calls = 249;
    while (baby.Phase != BabyMetroidCutscenePhase.WaitForMotherBrainToTurnToCorpse &&
           calls < 700)
    {
        byte samusPoseBeforeStep = samus.Pose;
        latch = baby.Step(bus, samus, motherBrain);
        calls++;
        sawBodyStumbleRequest |= latch.BodyStumbleRequested;
        sawMotherBrainInterrupt |= motherBrain.Phase ==
            MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidTakenAback;
        sawLatchSound |= latch.LatchSoundQueued;

        if (calls == 259)
        {
            AssertEqual(BabyMetroidCutscenePhase.GetRightUpInMotherBrainsFace, baby.Phase,
                "Baby curve timer expires on call 259");
            AssertEqual(ExpectedBabyPoint(0x00da, 0x0c00, 0x0086, 0x7a00),
                BabyPoint(),
                "Baby curve endpoint from retail ROM");
        }
        else if (calls == 269)
        {
            AssertEqual(BabyMetroidCutscenePhase.LatchOntoMotherBrain, baby.Phase,
                "Baby face timer expires on call 269");
            AssertTrue(samusPoseBeforeStep != samus.Pose,
                "Baby face completion calls drained controller one");
            AssertEqual(SamusPoseIds.DrainedStandingLeftPose, samus.Pose,
                "Baby face completion installs left drained standing pose");
            AssertEqual(ExpectedBabyPoint(0x008c, 0xc400, 0x004b, 0x6300),
                BabyPoint(),
                "Baby face endpoint from retail ROM");
        }
    }

    AssertEqual(425, calls, "Baby entrance reaches exact head pin on call 425");
    AssertEqual(BabyMetroidCutscenePhase.WaitForMotherBrainToTurnToCorpse, baby.Phase,
        "Baby enters corpse-state wait after pin");
    AssertTrue(sawBodyStumbleRequest, "Baby requests Mother Brain fast backward stumble");
    AssertTrue(sawMotherBrainInterrupt, "Baby overwrites Mother Brain final-beam function");
    AssertTrue(sawLatchSound, "Baby latch queues sound library one effect $40");
    AssertEqual(BabyMetroidCutsceneState.DrainingMotherBrainInstructionList,
        baby.InstructionList,
        "Baby pin installs draining animation");
    AssertEqual(ExpectedBabyPoint(0x0040, 0x1400, 0x0048, 0xd200),
        BabyPoint(),
        "Baby pin changes whole coordinates but retains native subpositions");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.BodyWalkingBackwardReallyFastInstructionList,
        motherBrain.Body.InstructionPointer,
        "Baby stumble uses animation-delay index two");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidTakenAback,
        motherBrain.Phase,
        "Baby installs Mother Brain $BE38 for the following actor turn");

    MotherBrainRainbowBeamAttackStepResult takenAback = motherBrain.Step(
        bus, samus, enemyFrameCounter: 0, mainEnemyExecutionCounter: 0);
    AssertEqual(MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidRegainBalance,
        takenAback.PhaseAfter,
        "Mother Brain taken-aback setup falls through into regain balance");
    AssertEqual(0x002f, motherBrain.FunctionTimer,
        "Mother Brain first regain call decrements $30 to $2F");
    AssertEqual(3, motherBrain.Body.Form, "Baby drain changes Mother Brain form to three");
    AssertEqual(8, motherBrain.LowerNeckMovementIndex,
        "Mother Brain taken-aback lower neck index");
    AssertEqual(8, motherBrain.UpperNeckMovementIndex,
        "Mother Brain taken-aback upper neck index");
    AssertEqual(0x0700, motherBrain.NeckAngleDelta,
        "Mother Brain taken-aback neck delta");

    int regainCalls = 1;
    while (motherBrain.Phase == MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidRegainBalance)
    {
        motherBrain.Step(bus, samus, enemyFrameCounter: 0, mainEnemyExecutionCounter: 0);
        regainCalls++;
    }
    AssertEqual(49, regainCalls, "Mother Brain regain balance includes setup fallthrough call");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidFiringRainbowBeam,
        motherBrain.Phase,
        "Mother Brain advances to painful firing function");
    AssertEqual(2, motherBrain.LowerNeckMovementIndex,
        "Mother Brain drained firing lower neck index");
    AssertEqual(4, motherBrain.UpperNeckMovementIndex,
        "Mother Brain drained firing upper neck index");

    Console.WriteLine(
        "  Baby Metroid: entrance, first latch and Mother Brain regain balance agree.");
}
}
