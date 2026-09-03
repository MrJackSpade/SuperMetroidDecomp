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

/// <summary>Mother Brain bomb, breath, dust, and escape-particle verification.</summary>
static void VerifyMotherBrainBombProjectiles()
{
    var bus = new TestAddressSpace();

    // Literal `$A9:9F00-$9F33` phase-three bomb list. The spritemap words are fixture
    // sentinels because the head interpreter only publishes them; every duration, opcode,
    // operand, and branch target is the retail byte stream being verified here.
    bus.WriteBytes(0xa99f00, [
        0x04, 0x00, 0x00, 0xa0,
        0x04, 0x00, 0x02, 0xa0,
        0x08, 0x00, 0x04, 0xa0,
        0x20, 0x9b,
        0x04, 0x00, 0x04, 0xa0,
        0x04, 0x00, 0x06, 0xa0,
        0x28, 0x9b, 0x6f, 0x00,
        0x08, 0x00, 0x08, 0xa0,
        0xbd, 0x9e, 0x01, 0x00,
        0x6d, 0x9b,
        0x20, 0x00, 0x08, 0xa0,
        0x04, 0x00, 0x06, 0xa0,
        0x10, 0x00, 0x04, 0xa0,
        0x14, 0x9b, 0xb9, 0x9c,
    ]);
    bus.WriteBytes(0xa99cb9, [
        0x04, 0x00, 0x00, 0xa0,
        0x04, 0x00, 0x02, 0xa0,
        0x08, 0x00, 0x04, 0xa0,
        0x04, 0x00, 0x02, 0xa0,
        0x04, 0x00, 0x00, 0xa0,
        0x04, 0x00, 0x02, 0xa0,
        0x08, 0x00, 0x04, 0xa0,
        0x08, 0x00, 0x02, 0xa0,
        0x0d, 0x9d,
        0x04, 0x00, 0x00, 0xa0,
        0x0f, 0x9b, 0xb9, 0x9c,
    ]);

    // Literal `$86:C76E-$C795` bomb animation. Its nine durations sum to 34 calls and
    // `$81AB` loops directly to the first record without introducing a blank frame.
    bus.WriteBytes(0x86c76e, [
        0x06, 0x00, 0xdc, 0x82,
        0x05, 0x00, 0xe8, 0x82,
        0x04, 0x00, 0xf4, 0x82,
        0x03, 0x00, 0x00, 0x83,
        0x02, 0x00, 0x0c, 0x83,
        0x02, 0x00, 0x18, 0x83,
        0x03, 0x00, 0x24, 0x83,
        0x04, 0x00, 0x30, 0x83,
        0x05, 0x00, 0x3c, 0x83,
        0xab, 0x81, 0x6e, 0xc7,
    ]);
    // Bomb/Samus-bomb collision selects misc-dust parameter nine. Its source bomb is the
    // highest occupied slot in this fixture, so the allocator must reuse that slot and the
    // outer `$86:8128` reload must execute this first record immediately.
    WriteTestWord(bus, 0x86e43e, 0xe1b0);
    bus.WriteBytes(0x86e1b0, [0x05, 0x00, 0x5a, 0x9a]);

    var samus = new SamusState { XPosition = 0x00e0, YPosition = 0x0078 };
    var motherBrain = new MotherBrainRainbowBeamAttackSequence
    {
        BrainXPosition = 0x0040,
        BrainYPosition = 0x0060,
    };
    motherBrain.Body.XPosition = 0x0070;
    motherBrain.BeginPhase3RecoveryFromBabyCutscene();
    motherBrain.Step(bus, samus, 0, 0);
    for (int call = 0; call < 32; call++)
        motherBrain.Step(bus, samus, 0, 0);
    MotherBrainRainbowBeamAttackStepResult selected = motherBrain.Step(
        bus, samus, 0, 0, randomNumberSeed: 0x8000);
    AssertEqual(MotherBrainPhase3AttackKind.Bomb, selected.Phase3Attack,
        "phase-three combat selects bomb fixture");
    AssertEqual(0x9f00, motherBrain.HeadInstructionPointer,
        "bomb selection exposes first head bytecode word");

    MotherBrainHeadAnimationStepResult spawnHead = default;
    ushort? headSoundLibraryTwo = null;
    int headCalls = 0;
    while (spawnHead.BombSpawn is null)
    {
        spawnHead = motherBrain.StepHeadAnimation(bus, samus, baby: null, randomNumberSeed: 0x0100);
        if (spawnHead.QueuedSoundLibraryTwo is { } sound)
            headSoundLibraryTwo = sound;
        headCalls++;
        AssertTrue(headCalls <= 33, "phase-three bomb head list reaches spawn opcode");
    }
    AssertEqual(33, headCalls, "head durations reach `$9EBD` on call 33");
    AssertEqual(new MotherBrainBombSpawnRequest(1), spawnHead.BombSpawn!.Value,
        "phase-three head supplies one-afterburn operand");
    AssertTrue(spawnHead.PurpleBreathBigSpawnRequested,
        "same head call executes adjacent large-purple-breath opcode");
    AssertEqual(0x006f, headSoundLibraryTwo,
        "phase-three bomb head consumes library-two cry operand");
    AssertEqual(0x9f28, spawnHead.InstructionPointerAfter,
        "spawn call loads the 32-frame open-mouth record after both opcodes");

    // Finish the close-mouth tail. `$9B14` must re-enable the neck, jump to `$9CB9`, and
    // load the first neutral frame during the same interpreter call.
    for (int call = 0; call < 52; call++)
        motherBrain.StepHeadAnimation(bus, samus, baby: null, randomNumberSeed: 0x0100);
    AssertEqual(0x9cbd, motherBrain.HeadInstructionPointer,
        "bomb tail returns to first phase-three neutral frame without a blank call");
    AssertEqual(1, motherBrain.NeckMovementEnabled,
        "bomb tail re-enables neck movement before entering neutral list");

    // The retail `$9D0D` contains an unconditional BRA over a tempting cry branch. Low
    // twelve-bit RNG below `$EC0` loops to `$9CD1`; exactly `$EC0` falls through to `$9CDB`.
    for (int call = 0; call < 44; call++)
        motherBrain.StepHeadAnimation(bus, samus, baby: null, randomNumberSeed: 0x0ebf);
    AssertEqual(0x9cd5, motherBrain.HeadInstructionPointer,
        "neutral low-RNG branch reloads `$9CD1` frame");
    for (int call = 0; call < 16; call++)
        motherBrain.StepHeadAnimation(bus, samus, baby: null, randomNumberSeed: 0x0ec0);
    AssertEqual(0x9cdf, motherBrain.HeadInstructionPointer,
        "neutral RNG `$EC0` boundary falls through to `$9CDB` frame");

    var projectiles = new MotherBrainEnemyProjectileSystem();
    AssertEqual<int?>(17, projectiles.SpawnBomb(motherBrain, spawnHead.BombSpawn.Value),
        "Mother Brain bomb uses highest free shared slot");
    AssertEqual(1, motherBrain.BombCounter, "bomb initializer increments body counter");
    MotherBrainEnemyProjectileSlot bomb = projectiles.Slots[17];
    AssertEqual(0x004c, bomb.XPosition, "bomb initializes at brain X plus twelve");
    AssertEqual(0x0070, bomb.YPosition, "bomb initializes at brain Y plus sixteen");
    AssertEqual(0x0001, bomb.XSubposition,
        "eight-bit initializer preserves afterburn count in low X-subposition byte");

    MotherBrainEnemyProjectileFrameResult first = projectiles.StepFrame(
        bus, motherBrain, baby: null, samus, layer1X: 0);
    AssertEqual(0x004c, bomb.XPosition, "first `$00.DE` X move remains subpixel");
    AssertEqual(0xde01, bomb.XSubposition,
        "first X move updates high fraction without destroying low afterburn byte");
    AssertEqual(0x0071, bomb.YPosition, "first `$01.07` Y move advances one pixel");
    AssertEqual(0x0700, bomb.YSubposition, "first Y move retains `$07` fraction");
    AssertEqual(0x00de, bomb.XVelocity, "pre-bounce friction subtracts exactly two");
    AssertEqual(0x0107, bomb.YVelocity, "first gravity stage adds seven");
    AssertEqual(0x82dc, bomb.SpritemapPointer, "spawn frame loads bomb spritemap zero");
    AssertEqual(0, first.BombEvents.Count, "ordinary movement emits no synthetic event");

    // Check every frame in the first complete 34-call ROM animation cycle. This catches
    // duration off-by-one errors independently from the much longer physical bounce route.
    ushort[] animationPointers = [0x82dc, 0x82e8, 0x82f4, 0x8300, 0x830c, 0x8318, 0x8324, 0x8330, 0x833c];
    int[] animationDurations = [6, 5, 4, 3, 2, 2, 3, 4, 5];
    int animationCall = 1;
    for (int frame = 0; frame < animationPointers.Length; frame++)
    {
        int alreadyChecked = frame == 0 ? 1 : 0;
        for (int repeat = alreadyChecked; repeat < animationDurations[frame]; repeat++)
        {
            projectiles.StepFrame(bus, motherBrain, baby: null, samus, layer1X: 0);
            animationCall++;
            AssertEqual(animationPointers[frame], bomb.SpritemapPointer,
                $"bomb animation call {animationCall} retains frame {frame}");
        }
    }
    AssertEqual(34, animationCall, "bomb animation cycle consumes exact summed duration");

    var bounceEvents = new List<MotherBrainBombEvent>();
    MotherBrainBombEvent? expired = null;
    for (int call = 35; call < 1000 && expired is null; call++)
    {
        MotherBrainEnemyProjectileFrameResult frame = projectiles.StepFrame(
            bus, motherBrain, baby: null, samus, layer1X: 0);
        foreach (MotherBrainBombEvent bombEvent in frame.BombEvents)
        {
            if (bombEvent.Kind == MotherBrainBombEventKind.Bounced)
                bounceEvents.Add(bombEvent);
            else if (bombEvent.Kind == MotherBrainBombEventKind.Expired)
                expired = bombEvent;
        }
    }
    AssertEqual(9, bounceEvents.Count, "bomb traverses all nine nonzero bounce stages");
    for (int bounceIndex = 0; bounceIndex < bounceEvents.Count; bounceIndex++)
    {
        AssertEqual(unchecked((ushort)((bounceIndex + 1) * 2)),
            bounceEvents[bounceIndex].BounceTableOffset,
            $"bounce {bounceIndex + 1} advances acceleration-table byte offset");
        AssertEqual(0x00d0, bounceEvents[bounceIndex].YPosition,
            $"bounce {bounceIndex + 1} clamps to floor Y `$D0`");
    }
    AssertTrue(expired.HasValue, "zero acceleration-table entry naturally expires bomb");
    AssertEqual(1, expired!.Value.AfterburnCount,
        "natural expiry publishes preserved low-byte afterburn count");
    AssertEqual(3, expired.Value.DustParameter, "natural expiry requests misc dust three");
    AssertEqual(0x0013, expired.Value.QueuedSoundLibraryThree,
        "natural expiry queues library-three sound `$13`");
    AssertTrue(!expired.Value.EnemyDropRequested, "natural expiry does not spawn enemy drops");
    AssertEqual(0, motherBrain.BombCounter, "natural expiry decrements body counter");

    // Build a genuine bank-$93 normal bomb and run it to timer zero. The bank-$86 collision
    // then consumes the public translated slot state exactly as gameplay ordering does.
    WriteTestWord(bus, 0x9383fb, 0x8675);
    bus.WriteBytes(0x938675, [0x1e, 0x00, 0xbf, 0x9f]);
    WriteTestWord(bus, 0x938683, 0xa06b);
    bus.WriteBytes(0x939fbf, [
        0x05, 0x00, 0x45, 0xad, 0x04, 0x04, 0x00, 0x00,
        0x39, 0x82, 0xbf, 0x9f,
    ]);
    bus.WriteBytes(0x939fe3, [
        0x01, 0x00, 0x45, 0xad, 0x04, 0x04, 0x00, 0x00,
        0x39, 0x82, 0xe3, 0x9f,
    ]);
    bus.WriteBytes(0x93a06b, [
        0x02, 0x00, 0x3e, 0xa8, 0x08, 0x08, 0x00, 0x00,
        0x2f, 0x82,
    ]);

    const int roomWidth = 16;
    const int roomHeight = 16;
    var emptyBlocks = new ushort[roomWidth * roomHeight];
    RoomLevelData room = CreateRoom(
        roomWidth, roomHeight, emptyBlocks, new byte[emptyBlocks.Length]);
    var bombSamus = new SamusState
    {
        Pose = SamusPoseIds.MorphBallGroundRightPose,
        EquippedItems = 0x1004,
        XPosition = 0x004c,
        YPosition = 0x0070,
    };
    var samusBombs = new SamusBombProjectileSystem();
    samusBombs.StepFrame(bus, room, bombSamus, (ushort)SnesButton.X, (ushort)SnesButton.X);
    while (samusBombs.Slots[0].BombTimer != 0)
        samusBombs.StepFrame(bus, room, bombSamus, 0, 0);
    AssertTrue(samusBombs.Slots[0].IsExploding, "real Samus bomb reaches timer-zero explosion");

    var collisionProjectiles = new MotherBrainEnemyProjectileSystem();
    AssertTrue(collisionProjectiles.SpawnBomb(motherBrain, new(7)).HasValue,
        "collision fixture allocates Mother Brain bomb");
    MotherBrainEnemyProjectileFrameResult collision = collisionProjectiles.StepFrame(
        bus, motherBrain, baby: null, bombSamus, layer1X: 0, samusBombs: samusBombs);
    AssertEqual(1, collision.BombEvents.Count, "Samus explosion emits one bomb deletion event");
    MotherBrainBombEvent destroyed = collision.BombEvents[0];
    AssertEqual(MotherBrainBombEventKind.DestroyedBySamusBomb, destroyed.Kind,
        "timer-zero normal bomb selects collision deletion path");
    AssertEqual(9, destroyed.DustParameter, "collision path requests misc dust nine");
    AssertTrue(destroyed.EnemyDropRequested, "collision path requests Mother Brain head drops");
    AssertEqual(null, destroyed.AfterburnCount,
        "collision path suppresses natural afterburn spawn");
    AssertEqual(null, destroyed.QueuedSoundLibraryThree,
        "collision path suppresses natural expiry sound");
    AssertEqual(0, motherBrain.BombCounter, "collision deletion decrements body counter");
    MotherBrainEnemyProjectileSlot collisionDust = collisionProjectiles.Slots[17];
    AssertEqual(MotherBrainEnemyProjectileSystem.MiscDustDefinition,
        collisionDust.ProjectileId,
        "collision deletion replaces the source bomb with parameter-nine dust");
    AssertEqual(0x0005, collisionDust.InstructionTimer,
        "same-slot collision dust loads its first duration immediately");
    AssertEqual(0x9a5a, collisionDust.SpritemapPointer,
        "same-slot collision dust loads its first spritemap immediately");

    Console.WriteLine(
        "  Mother Brain bombs: head bytecode, 8.8 motion, animation, bounce table, and both deletion paths agree.");
}

/// <summary>
/// Verifies the finite `$86:CB2F` purple-breath definition and the shared bank-$86 draw
/// passes. The timing fixture is the literal ROM instruction list; synthetic bank-$8D
/// spritemaps isolate priority and coordinate behavior without duplicating retail art.
/// </summary>
static void VerifyMotherBrainProjectileRendering()
{
    var bus = new TestAddressSpace();

    // `$86:CAA4-CAC7`: clear the pre-instruction, display eight timed frames for a total
    // of 76 calls, then delete. No host-side animation table is allowed to replace it.
    bus.WriteBytes(0x86caa4, [
        0x6a, 0x81,
        0x08, 0x00, 0x4f, 0x95,
        0x08, 0x00, 0x5b, 0x95,
        0x09, 0x00, 0x71, 0x95,
        0x09, 0x00, 0x96, 0x95,
        0x0a, 0x00, 0xbc, 0x95,
        0x0a, 0x00, 0xe7, 0x95,
        0x0b, 0x00, 0x13, 0x96,
        0x0b, 0x00, 0x44, 0x96,
        0x54, 0x81,
    ]);

    // Only the first bomb animation record is needed for this draw-order fixture. Its long
    // duration keeps the low-priority slot live while the breath timing is inspected.
    bus.WriteBytes(0x86c76e, [0xff, 0x00, 0xdc, 0x82]);

    // One unmistakable OBJ per definition. Purple breath uses tile $11 and the bomb uses
    // tile $22; graphics index `$0400` then selects palette two for the bomb.
    bus.WriteBytes(0x8d954f, [0x01, 0x00, 0x01, 0x80, 0x02, 0x11, 0x20]);
    bus.WriteBytes(0x8d82dc, [0x01, 0x00, 0xfe, 0x01, 0xff, 0x22, 0x20]);

    var motherBrain = new MotherBrainRainbowBeamAttackSequence
    {
        BrainXPosition = 0x0040,
        BrainYPosition = 0x0060,
    };
    var samus = new SamusState { XPosition = 0x0100, YPosition = 0x0080 };
    var mixedPool = new MotherBrainEnemyProjectileSystem();
    AssertEqual<int?>(17, mixedPool.SpawnBomb(motherBrain, new(1)),
        "low-priority bomb takes highest shared slot");
    AssertEqual<int?>(16, mixedPool.SpawnPurpleBreathBig(motherBrain),
        "high-priority breath takes next shared slot");
    MotherBrainEnemyProjectileSlot bomb = mixedPool.Slots[17];
    MotherBrainEnemyProjectileSlot breath = mixedPool.Slots[16];
    AssertEqual(0x40a0, bomb.Properties, "bomb definition properties");
    AssertEqual(0x3000, breath.Properties, "purple-breath definition properties");
    AssertEqual(0x0046, breath.XPosition, "purple breath initializes at brain X plus six");
    AssertEqual(0x0070, breath.YPosition, "purple breath initializes at brain Y plus sixteen");

    mixedPool.StepFrame(bus, motherBrain, baby: null, samus, layer1X: 0);
    AssertEqual(0x954f, breath.SpritemapPointer,
        "purple breath spawn call loads first bank-$8D spritemap");
    AssertEqual(0x82dc, bomb.SpritemapPointer,
        "bomb spawn call loads first bank-$8D spritemap");

    var oam = new OamBuffer();
    oam.BeginFrame();
    mixedPool.DrawHighPriority(bus, oam, layer1X: 0, layer1Y: 0);
    AssertEqual(4, oam.NextByteOffset, "high pass emits only purple breath");
    OamEntry high = oam.GetEntry(0);
    AssertEqual(0x047, high.X, "high-pass breath screen X plus spritemap offset");
    AssertEqual(0x72, high.Y, "high-pass breath screen Y plus spritemap offset");
    AssertTrue(high.IsLarge, "high-pass breath preserves large OBJ bit");
    AssertEqual(0x011, high.TileNumber, "high-pass breath tile");

    mixedPool.DrawLowPriority(bus, oam, layer1X: 0, layer1Y: 0);
    AssertEqual(8, oam.NextByteOffset, "low pass appends only bomb after high pass");
    OamEntry low = oam.GetEntry(1);
    AssertEqual(0x04a, low.X, "low-pass bomb signed X offset");
    AssertEqual(0x70, low.Y, "low-pass bomb negative Y offset");
    AssertEqual(2, low.Palette, "low-pass bomb graphics-index palette");
    AssertEqual(0x022, low.TileNumber, "low-pass bomb tile");

    // Use a fresh pool so the exact finite lifetime starts at call one. Each instruction
    // duration is expanded independently, then call 77 must execute `$8154` and clear ID.
    var timingPool = new MotherBrainEnemyProjectileSystem();
    AssertEqual<int?>(17, timingPool.SpawnPurpleBreathBig(motherBrain),
        "purple-breath timing fixture allocation");
    MotherBrainEnemyProjectileSlot timedBreath = timingPool.Slots[17];
    ushort[] spritemaps = [0x954f, 0x955b, 0x9571, 0x9596, 0x95bc, 0x95e7, 0x9613, 0x9644];
    int[] durations = [8, 8, 9, 9, 10, 10, 11, 11];
    int call = 0;
    for (int frame = 0; frame < spritemaps.Length; frame++)
    {
        for (int repeat = 0; repeat < durations[frame]; repeat++)
        {
            timingPool.StepFrame(bus, motherBrain, baby: null, samus, layer1X: 0);
            call++;
            AssertTrue(timedBreath.IsActive, $"purple breath remains active on call {call}");
            AssertEqual(spritemaps[frame], timedBreath.SpritemapPointer,
                $"purple-breath animation call {call}");
            AssertEqual(0x0046, timedBreath.XPosition,
                $"purple breath remains stationary in X on call {call}");
            AssertEqual(0x0070, timedBreath.YPosition,
                $"purple breath remains stationary in Y on call {call}");
        }
    }
    AssertEqual(76, call, "purple-breath timed frames sum to 76 calls");
    timingPool.StepFrame(bus, motherBrain, baby: null, samus, layer1X: 0);
    AssertTrue(!timedBreath.IsActive, "purple breath deletes on call 77");

    Console.WriteLine(
        "  Mother Brain projectiles: purple-breath timing and high/low OAM passes agree.");
}

/// <summary>
/// Verifies the `$86:E509` producer used by the Baby death sequence: pointer-table lookup,
/// exact parameter-three animation duration, shared high-priority OAM, and `$E6E0`'s strict
/// layer-1-origin deletion boundaries.
/// </summary>
static void VerifyMiscDustProjectiles()
{
    var bus = new TestAddressSpace();

    // Parameter three indexes word `$86:E432`, whose retail value points at `$E138`.
    WriteTestWord(bus, 0x86e432, 0xe138);
    bus.WriteBytes(0x86e138, [
        0x04, 0x00, 0x00, 0x98,
        0x06, 0x00, 0x07, 0x98,
        0x05, 0x00, 0x0e, 0x98,
        0x05, 0x00, 0x15, 0x98,
        0x05, 0x00, 0x1c, 0x98,
        0x06, 0x00, 0x23, 0x98,
        0x54, 0x81,
    ]);
    for (int frame = 0; frame < 6; frame++)
    {
        ushort pointer = unchecked((ushort)(0x9800 + frame * 7));
        bus.WriteBytes(0x8d0000 | pointer, [
            0x01, 0x00,
            0x00, 0x00, 0x00, unchecked((byte)(0x30 + frame)), 0x20,
        ]);
    }

    var motherBrain = new MotherBrainRainbowBeamAttackSequence();
    var samus = new SamusState();
    var projectiles = new MotherBrainEnemyProjectileSystem();
    AssertEqual<int?>(17,
        projectiles.SpawnMiscDust(bus, 0x0064, 0x0050, animationIndex: 3),
        "misc dust uses highest free shared slot");
    MotherBrainEnemyProjectileSlot dust = projectiles.Slots[17];
    AssertEqual(MotherBrainEnemyProjectileSystem.MiscDustDefinition, dust.ProjectileId,
        "misc-dust definition ID");
    AssertEqual(0x1000, dust.Properties, "misc dust uses high-priority pass");
    AssertEqual(3, dust.SpawnParameter, "misc-dust animation parameter retained");
    AssertEqual(0xe138, dust.InstructionPointer,
        "misc-dust initializer follows parameter-three pointer table");

    ushort[] spritemaps = [0x9800, 0x9807, 0x980e, 0x9815, 0x981c, 0x9823];
    int[] durations = [4, 6, 5, 5, 5, 6];
    int call = 0;
    for (int frame = 0; frame < spritemaps.Length; frame++)
    {
        for (int repeat = 0; repeat < durations[frame]; repeat++)
        {
            projectiles.StepFrame(
                bus, motherBrain, baby: null, samus, layer1X: 0, layer1Y: 0);
            call++;
            AssertTrue(dust.IsActive, $"misc dust remains active on call {call}");
            AssertEqual(spritemaps[frame], dust.SpritemapPointer,
                $"misc-dust animation call {call}");
            AssertEqual(0x0064, dust.XPosition,
                $"misc dust remains stationary in X on call {call}");
            AssertEqual(0x0050, dust.YPosition,
                $"misc dust remains stationary in Y on call {call}");
        }
    }
    AssertEqual(31, call, "parameter-three small explosion has 31 visible calls");

    // The final live frame still belongs to `$86:8390` because property bit `$1000` is set.
    var oam = new OamBuffer();
    oam.BeginFrame();
    projectiles.DrawHighPriority(bus, oam, layer1X: 0, layer1Y: 0);
    AssertEqual(4, oam.NextByteOffset, "live misc dust emits one high-priority OBJ");
    AssertEqual(0x035, oam.GetEntry(0).TileNumber,
        "last misc-dust frame reaches synthetic bank-$8D tile");
    projectiles.DrawLowPriority(bus, oam, layer1X: 0, layer1Y: 0);
    AssertEqual(4, oam.NextByteOffset, "misc dust is absent from low-priority pass");

    projectiles.StepFrame(bus, motherBrain, baby: null, samus, layer1X: 0, layer1Y: 0);
    AssertTrue(!dust.IsActive, "parameter-three small explosion deletes on call 32");

    var offscreen = new MotherBrainEnemyProjectileSystem();
    AssertTrue(offscreen.SpawnMiscDust(bus, 0x0064, 0x0050, 3).HasValue,
        "off-screen misc-dust fixture allocation");
    offscreen.StepFrame(
        bus, motherBrain, baby: null, samus, layer1X: 0x0065, layer1Y: 0);
    AssertTrue(!offscreen.Slots[17].IsActive,
        "misc-dust origin one pixel left of layer one deletes before animation");

    var rightBoundary = new MotherBrainEnemyProjectileSystem();
    AssertTrue(rightBoundary.SpawnMiscDust(bus, 0x0100, 0x0050, 3).HasValue,
        "right-boundary misc-dust fixture allocation");
    rightBoundary.StepFrame(
        bus, motherBrain, baby: null, samus, layer1X: 0, layer1Y: 0);
    AssertTrue(!rightBoundary.Slots[17].IsActive,
        "misc-dust origin at layer-one X plus 256 deletes");
    AssertThrows<ArgumentOutOfRangeException>(
        () => projectiles.SpawnMiscDust(bus, 0, 0, 0x001e),
        "misc-dust animation parameter $1E rejected");

    Console.WriteLine(
        "  Misc dust: pointer lookup, small-explosion timing, OAM, and off-screen deletion agree.");
}

/// <summary>
/// Verifies all eight `$86:CB21` initializers, shared-slot allocation, exact 8.8 movement,
/// looping spritemap durations, 33-call lifetime, and terminal parameter-nine dust spawns.
/// </summary>
static void VerifyMotherBrainEscapeDoorParticles()
{
    var bus = new TestAddressSpace();

    // Literal `$86:CA22-$CA45` instruction list. Supplying it through the bus proves the
    // production interpreter follows ROM words instead of a parallel host animation table.
    bus.WriteBytes(0x86ca22, [
        0x01, 0x00, 0x9b, 0x96,
        0x01, 0x00, 0xa2, 0x96,
        0x01, 0x00, 0xa9, 0x96,
        0x01, 0x00, 0xb0, 0x96,
        0x03, 0x00, 0xb7, 0x96,
        0x03, 0x00, 0xbe, 0x96,
        0x04, 0x00, 0xc5, 0x96,
        0x04, 0x00, 0xcc, 0x96,
        0xab, 0x81, 0x22, 0xca,
    ]);
    bus.WriteBytes(0x86cb0d, [
        0x01, 0x00, 0x0b, 0x97,
        0x59, 0x81,
    ]);
    // Parameter nine selects `$E1B0`. One literal timed record is sufficient to prove that
    // terminal fragments which reuse their own slots enter the NEW definition's animation
    // handler during call 33, rather than retaining stale fragment art for one extra frame.
    WriteTestWord(bus, 0x86e43e, 0xe1b0);
    bus.WriteBytes(0x86e1b0, [0x05, 0x00, 0xbc, 0x9a]);

    var projectiles = new MotherBrainEnemyProjectileSystem();
    for (ushort parameter = 0; parameter < 8; parameter++)
    {
        int? allocated = projectiles.SpawnEscapeDoorParticle(new(parameter));
        AssertEqual<int?>(17 - parameter, allocated,
            $"door fragment {parameter} uses highest free shared slot");
    }
    AssertThrows<ArgumentOutOfRangeException>(
        () => projectiles.SpawnEscapeDoorParticle(new(8)),
        "door fragment parameter eight rejected");

    short[] expectedYOffsets = [-0x20, -0x18, -0x10, -0x08, 0, 0x08, 0x10, 0x18];
    short[] expectedYVelocities = [-0x0200, -0x0100, -0x0100, -0x0080, -0x0080, 0x0080, -0x0100, 0x0200];
    for (ushort parameter = 0; parameter < 8; parameter++)
    {
        MotherBrainEnemyProjectileSlot slot = projectiles.Slots[17 - parameter];
        AssertEqual(MotherBrainEnemyProjectileSystem.EscapeDoorParticleDefinition,
            slot.ProjectileId, $"door fragment {parameter} definition");
        AssertEqual(0x0010, slot.XPosition, $"door fragment {parameter} initial X");
        AssertEqual(unchecked((ushort)(0x0080 + expectedYOffsets[parameter])),
            slot.YPosition, $"door fragment {parameter} initial Y");
        AssertEqual(0x0500, slot.XVelocity,
            $"door fragment {parameter} initial X velocity");
        AssertEqual(unchecked((ushort)expectedYVelocities[parameter]),
            slot.YVelocity, $"door fragment {parameter} initial Y velocity");
        AssertEqual(0x0020, slot.Lifetime,
            $"door fragment {parameter} initial lifetime");
    }

    var motherBrain = new MotherBrainRainbowBeamAttackSequence
    {
        BrainXPosition = 0x0100,
        BrainYPosition = 0x0100,
    };
    var samus = new SamusState { XPosition = 0x0400, YPosition = 0x0400 };
    MotherBrainEnemyProjectileFrameResult first = projectiles.StepFrame(
        bus, motherBrain, baby: null, samus, layer1X: 0);
    AssertEqual(8, first.ActiveCount, "all eight door fragments survive first movement call");
    MotherBrainEnemyProjectileSlot firstFragment = projectiles.Slots[17];
    AssertEqual(0x0014, firstFragment.XPosition,
        "first fragment applies slowed `$04.F0` whole X movement");
    AssertEqual(0xf000, firstFragment.XSubposition,
        "first fragment retains `$F0` X velocity fraction in high subposition byte");
    AssertEqual(0x005e, firstFragment.YPosition,
        "first fragment applies gravity then signed `$FE.20` Y movement");
    AssertEqual(0x2000, firstFragment.YSubposition,
        "first fragment retains `$20` Y fraction");
    AssertEqual(0x04f0, firstFragment.XVelocity,
        "first fragment X friction subtracts `$10`");
    AssertEqual(0xfe20, firstFragment.YVelocity,
        "first fragment gravity adds `$20`");
    AssertEqual(0x969b, firstFragment.SpritemapPointer,
        "spawn frame loads exploded-door spritemap zero");
    AssertEqual(0x001f, firstFragment.Lifetime,
        "spawn frame performs lifetime decrement one");

    ushort[] expectedSpritemapsByCall =
    [
        0x96a2, 0x96a9, 0x96b0,
        0x96b7, 0x96b7, 0x96b7,
        0x96be, 0x96be, 0x96be,
        0x96c5, 0x96c5, 0x96c5, 0x96c5,
        0x96cc, 0x96cc, 0x96cc, 0x96cc,
        0x969b,
    ];
    for (int callIndex = 0; callIndex < expectedSpritemapsByCall.Length; callIndex++)
    {
        projectiles.StepFrame(bus, motherBrain, baby: null, samus, layer1X: 0);
        AssertEqual(expectedSpritemapsByCall[callIndex], firstFragment.SpritemapPointer,
            $"door fragment animation call {callIndex + 2}");
    }

    // Nineteen calls have run. Calls 20..32 remain active; call 33 changes Var0 zero to
    // `$FFFF`, deletes every fragment after its last movement, subtracts four from Y, and
    // allocates one parameter-nine misc-dust projectile per physical slot. With no unrelated
    // holes above the source, every allocation reuses that source slot and `$86:8128` makes
    // the newborn dust consume its first timed frame immediately.
    MotherBrainEnemyProjectileFrameResult lifetimeResult = default;
    for (int call = 20; call <= 33; call++)
    {
        lifetimeResult = projectiles.StepFrame(bus, motherBrain, baby: null, samus, layer1X: 0);
        if (call < 33)
            AssertEqual(8, lifetimeResult.ActiveCount, $"door fragments active through call {call}");
    }
    // Parameter seven's strongly downward fragment finishes below layer1Y+$100. Native
    // still allocates its dust, then the same-slot `$E4FE` pre-instruction immediately
    // deletes that off-window origin. The other seven remain visible.
    AssertEqual(7, lifetimeResult.ActiveCount,
        "seven on-window terminal dust projectiles survive call 33");
    AssertEqual(8, lifetimeResult.EscapeDoorDustRequests.Count,
        "all eight expiring fragments request terminal dust");
    foreach (MotherBrainEscapeDoorParticleDustRequest dust in lifetimeResult.EscapeDoorDustRequests)
        AssertEqual(0x0009, dust.ProjectileParameter, "terminal fragment dust parameter");
    AssertTrue(!projectiles.Slots[10].IsActive,
        "lowest slot's downward fragment dust deletes off-screen during the same pass");
    for (int slotIndex = 11; slotIndex < MotherBrainEnemyProjectileSystem.SlotCount; slotIndex++)
    {
        MotherBrainEnemyProjectileSlot terminalDust = projectiles.Slots[slotIndex];
        AssertEqual(MotherBrainEnemyProjectileSystem.MiscDustDefinition,
            terminalDust.ProjectileId,
            $"terminal fragment slot {slotIndex} now contains misc dust");
        AssertEqual(0x0005, terminalDust.InstructionTimer,
            $"same-pass terminal dust slot {slotIndex} loads first duration");
        AssertEqual(0x9abc, terminalDust.SpritemapPointer,
            $"same-pass terminal dust slot {slotIndex} loads first spritemap");
        AssertEqual(0xe1b4, terminalDust.InstructionPointer,
            $"same-pass terminal dust slot {slotIndex} advances its list pointer");
    }

    // A separate pool demonstrates that rings and fragments genuinely compete for the same
    // eighteen entries. Eight fragments plus ten rings fill it; a nineteenth allocation
    // fails without displacing any live projectile.
    var sharedPool = new MotherBrainEnemyProjectileSystem();
    for (ushort parameter = 0; parameter < 8; parameter++)
        AssertTrue(sharedPool.SpawnEscapeDoorParticle(new(parameter)).HasValue,
            $"shared pool door fragment {parameter}");
    for (byte ring = 0; ring < 10; ring++)
        AssertTrue(sharedPool.Spawn(bus, motherBrain, new MotherBrainOnionRingSpawnRequest(ring)).HasValue,
            $"shared pool ring {ring}");
    AssertEqual<int?>(null,
        sharedPool.Spawn(bus, motherBrain, new MotherBrainOnionRingSpawnRequest(0x10)),
        "nineteenth mixed Mother Brain projectile allocation fails");

    // The subtitle uses the same allocator, loads one `$8D:970B` spritemap immediately,
    // then sleeps forever. Two passes prove its pre-instruction keeps publishing the fixed
    // coordinates and zero velocities after the instruction pointer reaches sleep.
    var subtitlePool = new MotherBrainEnemyProjectileSystem();
    AssertEqual<int?>(17, subtitlePool.SpawnTimeBombSetSubtitle(),
        "alternate subtitle uses highest free shared slot");
    MotherBrainEnemyProjectileSlot subtitle = subtitlePool.Slots[17];
    subtitlePool.StepFrame(bus, motherBrain, baby: null, samus, layer1X: 0);
    AssertEqual(0x970b, subtitle.SpritemapPointer,
        "alternate subtitle loads Japanese text spritemap");
    subtitlePool.StepFrame(bus, motherBrain, baby: null, samus, layer1X: 0);
    AssertEqual(0x0080, subtitle.XPosition,
        "alternate subtitle pre-instruction repins X");
    AssertEqual(0x00c0, subtitle.YPosition,
        "alternate subtitle pre-instruction repins Y");
    AssertEqual(0, subtitle.XVelocity,
        "alternate subtitle pre-instruction clears X velocity");
    AssertEqual(0, subtitle.YVelocity,
        "alternate subtitle pre-instruction clears Y velocity");
    AssertTrue(subtitle.IsActive, "alternate subtitle sleep keeps projectile alive");

    Console.WriteLine(
        "  Mother Brain projectiles: shared allocation, subtitle pinning, door-fragment motion, animation, and dust agree.");
}

}
