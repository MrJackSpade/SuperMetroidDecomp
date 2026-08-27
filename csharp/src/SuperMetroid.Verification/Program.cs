using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;

// This is deliberately a plain console executable rather than an xUnit/MSTest project.
// It keeps the reverse-engineering workspace dependency-free and makes every check easy
// to step through in Visual Studio. A failed check throws immediately with concrete state.
// Windows otherwise turns an unhandled CLR assertion into a modal "unknown software
// exception" dialog. That is actively hostile to an automated verifier: the useful stack
// trace belongs in this console and a dialog must never steal focus or stall the process.
if (OperatingSystem.IsWindows())
    NativeConsoleProcess.SetErrorMode(0x0001 | 0x0002 | 0x8000);

try
{
Console.WriteLine("Verifying translated Super Metroid routines...");

VerifyRandomNumberGeneratorExhaustively();
VerifyKnownRandomSequence();
VerifyTimedHeldInputTimeline();
VerifyEventBitfield();
VerifyBossBitfield();
VerifyMultiplicationExhaustively();
VerifyVramWriteQueue();
VerifyEscapeTimerBcd();
VerifyEscapeTimerStateMachine();
VerifyControllerInputLatch();
VerifyFrameRuntime();
VerifySuperMetroidAddressSpace();
VerifyOamSpritemapPacking();
VerifySamusRenderingSlice();
VerifySamusPoseTransitionMatching();
VerifySamusHorizontalSpeed();
VerifySamusExtraDisplacement();
VerifySamusStoredShineAndShinespark();
VerifySamusCrystalFlash();
VerifySamusXray();
VerifySamusDeathSequence();
VerifySamusDrainedController();
VerifySamusGrabbedByDraygon();
VerifyMotherBrainRainbowBeamSamusMovement();
VerifyMotherBrainRainbowBeamAttackSequence();
VerifyMotherBrainBombProjectiles();
VerifyMotherBrainProjectileRendering();
VerifyMiscDustProjectiles();
VerifyMotherBrainEscapeDoorParticles();
VerifyBabyMetroidCutsceneEntrance();
VerifySamusSolidEnemyCollision();
VerifySamusAerialMovement();
VerifySamusSpaceJumpAndScrewAttack();
VerifySamusLiquidPhysics();
VerifySamusAtmosphericEffects();
VerifySamusAerialTurnsAndWallJump();
VerifySamusKnockbackAndDamageBoost();
VerifySamusGrappleSwingAndRelease();
VerifyBreakableGrapplePlms();
VerifySamusPostureMovement();
VerifySamusPowerBeamProjectiles();
VerifySamusMorphBallMovement();
VerifySamusStandingAimMovement();
VerifySamusAimedAerialMovement();
VerifySamusGunExtendedMovement();
VerifySamusSlopePhysics();
VerifySamusBlockCollision();
VerifySamusGroundedMovement();
VerifySamusGroundedReversal();
VerifySamusMoonwalking();
VerifySamusRanIntoWall();
VerifyObjRendering();
VerifyHudStateAndBg3Rendering();
VerifyDebugRoomCamera();
VerifyRoomScrollGridAndBoundaryCamera();
VerifyMovedSamusCameraTracking();
VerifyBackgroundScrollState();
VerifyLevelBlockTilemapExpansion();
VerifyRoomLevelData();
VerifyBackgroundTilemapStreamer();
VerifyFourBitBackgroundRendering();
VerifyHostRoomViewportAlignment();
VerifyPowerBombColorMathWindow();
VerifyScrollingSkyState();

Console.WriteLine("All bank $80 verification checks passed.");
return 0;
}
catch (Exception exception)
{
    // This is deliberately handled here, at the process boundary. Assertions still stop the
    // verifier immediately, but Windows never receives an unhandled CLR exception that it can
    // turn into a focus-stealing dialog. ToString() retains the type, message, inner exception,
    // and complete stack trace in the terminal where the failure is actually actionable.
    Console.Error.WriteLine(exception);
    return 1;
}

/// <summary>
/// Compares the production port against a mechanically different description of the
/// original 65C816 register operations for every possible seed.
/// </summary>
static void VerifyRandomNumberGeneratorExhaustively()
{
    for (int seed = ushort.MinValue; seed <= ushort.MaxValue; seed++)
    {
        var state = new Bank80SystemState((ushort)seed);
        ushort expected = ReferenceNextRandom((ushort)seed);
        ushort actual = state.NextRandom();
        AssertEqual(expected, actual, $"RNG seed ${seed:X4}");
        AssertEqual(actual, state.RandomNumber, $"RNG stored state for seed ${seed:X4}");
    }

    Console.WriteLine("  RNG: all 65,536 seeds agree with the assembly-level reference.");
}

/// <summary>
/// Hard-coded values make the test capable of catching a future accidental change to both
/// the production and reference implementations. The sequence begins at the ROM's $0061
/// initialization value.
/// </summary>
static void VerifyKnownRandomSequence()
{
    ushort[] expected = [
        0x02f6, 0x0fdf, 0x506c, 0x932d,
        0xe0f2, 0x65cb, 0xfe08, 0xf739,
    ];

    var state = new Bank80SystemState();
    foreach (ushort expectedValue in expected)
        AssertEqual(expectedValue, state.NextRandom(), "known RNG sequence");

    Console.WriteLine("  RNG: known power-on sequence agrees.");
}

/// <summary>
/// Recreates a press-and-hold timeline with the pause-screen reset value of three. The new
/// press is excluded on frame zero, the changed held value reloads on frame one, and four
/// further stable calls count 3,2,1,0,$FFFF before activation.
/// </summary>
static void VerifyTimedHeldInputTimeline()
{
    const ushort button = 0x0080;
    var state = new Bank80SystemState();

    // Frame 0: the button is new, so TRB removes it from the held-only sample.
    state.UpdateHeldInput(timerReset: 3, controllerInput: button, controllerNewInput: button);
    AssertEqual((ushort)0, state.TimedHeldInput, "new press is not held input");

    // Frame 1: held-only input changes from zero to the button and reloads the timer.
    state.UpdateHeldInput(3, button, 0);
    AssertEqual((ushort)3, state.TimedHeldInputTimer, "changed input reloads timer");
    AssertEqual((ushort)0, state.TimedHeldInput, "changed input stays suppressed");

    // Frames 2-4: DEC yields 2, 1, and 0. BPL keeps taking the suppression path.
    for (ushort expectedTimer = 2; ; expectedTimer--)
    {
        state.UpdateHeldInput(3, button, 0);
        AssertEqual(expectedTimer, state.TimedHeldInputTimer, "stable-input countdown");
        AssertEqual((ushort)0, state.TimedHeldInput, "countdown suppression");
        if (expectedTimer == 0)
            break;
    }

    // Frame 5: zero wraps to $FFFF, which is negative to BPL, and activates the button.
    state.UpdateHeldInput(3, button, 0);
    AssertEqual((ushort)0, state.TimedHeldInputTimer, "expired timer is pinned to zero");
    AssertEqual(button, state.TimedHeldInput, "held input activates after underflow");
    AssertEqual(button, state.NewlyTimedHeldInput, "activation creates a rising-edge pulse");

    // Frame 6: the active input remains present, while its rising-edge output clears.
    state.UpdateHeldInput(3, button, 0);
    AssertEqual(button, state.TimedHeldInput, "stable expired input remains active");
    AssertEqual((ushort)0, state.NewlyTimedHeldInput, "rising-edge pulse lasts one update");

    // Releasing changes the held sample, reloads the timer, and suppresses the output.
    state.UpdateHeldInput(3, 0, 0);
    AssertEqual((ushort)3, state.TimedHeldInputTimer, "release reloads timer");
    AssertEqual((ushort)0, state.TimedHeldInput, "release clears filtered input");
    AssertEqual((ushort)0, state.NewlyTimedHeldInput, "release is not a new held press");

    Console.WriteLine("  Input: delayed-held timeline and edge pulse agree.");
}

/// <summary>
/// Exercises every allocated event bit and verifies its exact containing byte. This catches
/// swapped shift/index operations as well as clear-mask width errors.
/// </summary>
static void VerifyEventBitfield()
{
    var state = new Bank80SystemState();
    for (int eventNumber = 0; eventNumber < Bank80SystemState.EventByteCount * 8; eventNumber++)
    {
        state.SetEvent(eventNumber);
        AssertTrue(state.HasEvent(eventNumber), $"event ${eventNumber:X2} set");

        int expectedByteIndex = eventNumber >> 3;
        byte expectedMask = (byte)(1 << (eventNumber & 7));
        AssertEqual(expectedMask, state.GetEventByteRaw(expectedByteIndex), $"event ${eventNumber:X2} byte/mask");

        state.ClearEvent(eventNumber);
        AssertTrue(!state.HasEvent(eventNumber), $"event ${eventNumber:X2} clear");
        AssertEqual((byte)0, state.GetEventByteRaw(expectedByteIndex), $"event ${eventNumber:X2} cleared byte");
    }

    AssertThrows<ArgumentOutOfRangeException>(() => state.SetEvent(-1), "negative event rejected");
    AssertThrows<ArgumentOutOfRangeException>(() => state.SetEvent(64), "event past allocation rejected");
    Console.WriteLine("  Events: all 64 allocated bits set, map, and clear correctly.");
}

/// <summary>
/// Verifies that boss masks combine independently in all eight area bytes.
/// </summary>
static void VerifyBossBitfield()
{
    var state = new Bank80SystemState();
    for (int area = 0; area < Bank80SystemState.AreaCount; area++)
    {
        state.SetBossBits(area, BossBits.AreaBoss | BossBits.AreaTorizo);
        AssertTrue(state.HasAnyBossBits(area, BossBits.AreaBoss), $"area {area} boss bit");
        AssertTrue(!state.HasAnyBossBits(area, BossBits.AreaMiniBoss), $"area {area} mini-boss remains clear");
        AssertEqual((byte)0x05, state.GetBossBitsRaw(area), $"area {area} combined raw mask");

        state.ClearBossBits(area, BossBits.AreaTorizo);
        AssertEqual((byte)0x01, state.GetBossBitsRaw(area), $"area {area} selective clear");
    }

    AssertThrows<ArgumentOutOfRangeException>(() => state.SetBossBits(-1, BossBits.AreaBoss), "negative area rejected");
    AssertThrows<ArgumentOutOfRangeException>(() => state.SetBossBits(8, BossBits.AreaBoss), "area past allocation rejected");
    Console.WriteLine("  Boss state: all eight area bytes preserve independent masks.");
}

/// <summary>
/// The full Cartesian product would require 2^32 cases. Iterating all left operands against
/// carefully selected right operands covers every input bit, carry boundary, and high-word
/// behavior in a fraction of the time.
/// </summary>
static void VerifyMultiplicationExhaustively()
{
    ushort[] rightOperands = [0, 1, 2, 3, 0x00ff, 0x0100, 0x7fff, 0x8000, 0xffff];
    foreach (ushort right in rightOperands)
    {
        for (int left = ushort.MinValue; left <= ushort.MaxValue; left++)
        {
            uint expected = (uint)left * right;
            uint actual = Bank80SystemState.Multiply16By16((ushort)left, right);
            AssertEqual(expected, actual, $"multiply ${left:X4} by ${right:X4}");
        }
    }

    Console.WriteLine("  Multiply: 589,824 boundary-spanning products agree.");
}

/// <summary>
/// Verifies both VMAIN modes used by the queue, packed-tail accounting, source-bank wrap,
/// ordered overwrite behavior, and the NMI consumer's unconditional queue reset.
/// </summary>
static void VerifyVramWriteQueue()
{
    var bus = new TestAddressSpace();
    var vram = new SnesVram();
    var queue = new VramWriteQueue();

    bus.WriteBytes(0x7e1000, [0x10, 0x11, 0x12, 0x13]);
    queue.Enqueue(sizeInBytes: 4, sourceAddress: 0x7e1000, encodedVramDestination: 0x0001);
    AssertEqual(VramWriteQueue.EntryByteCount, queue.TailInBytes, "one packed VRAM queue tail");

    // Setting destination bit 15 selects VMAIN=$81. The physical start remains word one,
    // but the next source pair lands 32 words later at word $0021.
    bus.WriteBytes(0x7e2000, [0x20, 0x21, 0x22, 0x23]);
    queue.Enqueue(sizeInBytes: 4, sourceAddress: 0x7e2000, encodedVramDestination: 0x8001);
    AssertEqual(VramWriteQueue.EntryByteCount * 2, queue.TailInBytes, "two packed VRAM queue tails");

    // This transfer crosses $80:FFFF. DMA keeps bank $80 fixed, so its second byte must
    // come from $80:0000 rather than the linear address $81:0000.
    bus.WriteByte(0x80ffff, 0xa0);
    bus.WriteByte(0x800000, 0xa1);
    queue.Enqueue(sizeInBytes: 2, sourceAddress: 0x80ffff, encodedVramDestination: 0x0100);

    queue.DrainTo(vram, bus);

    // The later column transfer intentionally overwrites the first pair at word one. This
    // proves insertion order is preserved, while word two retains the ordinary transfer.
    AssertEqual((byte)0x20, vram.ReadByte(0x0001 * 2), "column transfer low byte at first word");
    AssertEqual((byte)0x21, vram.ReadByte(0x0001 * 2 + 1), "column transfer high byte at first word");
    AssertEqual((byte)0x12, vram.ReadByte(0x0002 * 2), "linear transfer increments one word");
    AssertEqual((byte)0x13, vram.ReadByte(0x0002 * 2 + 1), "linear transfer high byte");
    AssertEqual((byte)0x22, vram.ReadByte(0x0021 * 2), "column transfer increments 32 words");
    AssertEqual((byte)0x23, vram.ReadByte(0x0021 * 2 + 1), "column transfer second high byte");
    AssertEqual((byte)0xa0, vram.ReadByte(0x0100 * 2), "bank-wrap transfer first byte");
    AssertEqual((byte)0xa1, vram.ReadByte(0x0100 * 2 + 1), "bank-wrap transfer wrapped byte");

    AssertEqual(0, queue.TailInBytes, "drain clears packed tail");
    AssertEqual(0, queue.Entries.Count, "drain clears typed records");
    AssertThrows<ArgumentOutOfRangeException>(() => queue.Enqueue(0, 0x808000, 0), "zero-size VRAM entry rejected");
    AssertThrows<ArgumentOutOfRangeException>(() => queue.Enqueue(1, 0x1000000, 0), "25-bit VRAM source rejected");

    // Fill a fresh queue to its real table boundary. The next seven-byte record would
    // leave insufficient space for the two-byte terminator and must be rejected.
    var fullQueue = new VramWriteQueue();
    while (fullQueue.TailInBytes + VramWriteQueue.EntryByteCount + 2 <= VramWriteQueue.StorageByteCount)
        fullQueue.Enqueue(1, 0x808000, 0);
    AssertThrows<InvalidOperationException>(() => fullQueue.Enqueue(1, 0x808000, 0), "VRAM queue overflow rejected");

    Console.WriteLine("  VRAM: write queue, VMAIN stepping, wrap, and reset agree.");
}

/// <summary>
/// Checks every valid two-digit BCD input against ordinary decimal subtraction for the two
/// correction values the ROM table uses.
/// </summary>
static void VerifyEscapeTimerBcd()
{
    for (int decimalValue = 0; decimalValue <= 99; decimalValue++)
    {
        byte packed = PackBcd(decimalValue);
        for (byte decrement = 1; decrement <= 2; decrement++)
        {
            byte actual = EscapeTimer.SubtractPackedBcd(packed, decrement, out bool actualBorrow);
            int decimalResult = decimalValue - decrement;
            bool expectedBorrow = decimalResult < 0;
            if (expectedBorrow)
                decimalResult += 100;

            AssertEqual(PackBcd(decimalResult), actual, $"BCD {decimalValue:D2} - {decrement}");
            AssertEqual(expectedBorrow, actualBorrow, $"BCD borrow {decimalValue:D2} - {decrement}");
        }
    }

    Console.WriteLine("  Timer: all valid BCD decrements and borrows agree.");
}

/// <summary>
/// Walks the Ceres timer from request through all animation phases, then isolates second,
/// minute, expiration, correction-table, and Mother Brain initialization boundaries.
/// </summary>
static void VerifyEscapeTimerStateMachine()
{
    var timer = new EscapeTimer();
    timer.RequestCeresStart();
    AssertTrue(!timer.Process(0), "Ceres initialization does not report expiration");
    AssertEqual(EscapeTimerState.InitialDelay, timer.State, "Ceres enters initial delay");
    AssertEqual((byte)0x01, timer.MinutesBcd, "Ceres starts at one minute");
    AssertEqual((ushort)0x8000, timer.XPositionFixed, "timer starts at X $80.00");
    AssertEqual((ushort)0x8000, timer.YPositionFixed, "timer starts at Y $80.00");

    // State 3 increments the low X byte from $00 through $10 without decrementing time.
    for (ushort frame = 1; frame <= 16; frame++)
        AssertTrue(!timer.Process(frame), $"initial-delay frame {frame}");
    AssertEqual(EscapeTimerState.RunningMovementDelayed, timer.State, "initial delay advances at counter $10");
    AssertEqual((ushort)0x8010, timer.XPositionFixed, "X subpixel byte contains delay counter $10");
    AssertEqual((byte)0x00, timer.SecondsBcd, "initial delay did not decrement seconds");

    // State 4 continues the same aliased counter to $60, then zeroes only its low byte.
    for (ushort frame = 17; frame <= 96; frame++)
        timer.Process(frame);
    AssertEqual(EscapeTimerState.RunningMovingIntoPlace, timer.State, "movement delay advances at counter $60");
    AssertEqual((ushort)0x8000, timer.XPositionFixed, "movement transition clears X fraction only");

    // X reaches its clamp in 106 frames; Y requires 107, so the state advances on 107.
    for (ushort frame = 97; frame <= 203; frame++)
        timer.Process(frame);
    AssertEqual(EscapeTimerState.RunningInPlace, timer.State, "timer reaches stationary running state");
    AssertEqual((ushort)0xdc00, timer.XPositionFixed, "timer X clamps at pixel 220");
    AssertEqual((ushort)0x3000, timer.YPositionFixed, "timer Y clamps at pixel 48");

    var borrowSecond = CreateRunningTimer(0x00, 0x01, 0x00);
    AssertTrue(!borrowSecond.Process(1), "00:01.00 remains nonzero after decrement");
    AssertEqual((byte)0x00, borrowSecond.SecondsBcd, "centisecond borrow decrements seconds");
    AssertEqual((byte)0x98, borrowSecond.CentisecondsBcd, "table value two wraps centiseconds to 98");

    var borrowMinute = CreateRunningTimer(0x01, 0x00, 0x00);
    AssertTrue(!borrowMinute.Process(0), "01:00.00 remains nonzero after decrement");
    AssertEqual((byte)0x00, borrowMinute.MinutesBcd, "second borrow decrements minutes");
    AssertEqual((byte)0x59, borrowMinute.SecondsBcd, "minute borrow reloads seconds to 59");
    AssertEqual((byte)0x99, borrowMinute.CentisecondsBcd, "table value one wraps centiseconds to 99");

    var expiration = CreateRunningTimer(0x00, 0x00, 0x01);
    AssertTrue(expiration.Process(0), "00:00.01 expires on a decrement of one");
    AssertEqual((byte)0, expiration.CentisecondsBcd, "expiration saturates centiseconds");

    var underflow = CreateRunningTimer(0x00, 0x00, 0x00);
    AssertTrue(underflow.Process(1), "zero timer remains expired after attempted decrement");
    AssertEqual((byte)0, underflow.MinutesBcd, "underflow saturates minutes");
    AssertEqual((byte)0, underflow.SecondsBcd, "underflow saturates seconds");
    AssertEqual((byte)0, underflow.CentisecondsBcd, "underflow saturates centiseconds");

    var motherBrain = new EscapeTimer();
    motherBrain.RequestMotherBrainStart();
    motherBrain.Process(0);
    AssertEqual((byte)0x03, motherBrain.MinutesBcd, "Mother Brain starts at three minutes");
    AssertEqual((ushort)0x8003, motherBrain.RawStatus, "active timer retains status high flag");

    Console.WriteLine("  Timer: initialization, animation, BCD borrow, and expiration agree.");
}

static EscapeTimer CreateRunningTimer(byte minutes, byte seconds, byte centiseconds)
{
    // Advancing the startup path is preferable to adding a production-only raw status
    // setter: this fixture reaches the real state-$8006 path exactly as gameplay does.
    var timer = new EscapeTimer();
    timer.RequestCeresStart();
    timer.Process(0);
    for (ushort frame = 1; timer.State != EscapeTimerState.RunningInPlace; frame++)
        timer.Process(frame);
    timer.SetTime(minutes, seconds, centiseconds);
    return timer;
}

static byte PackBcd(int decimalValue) => (byte)(((decimalValue / 10) << 4) | (decimalValue % 10));

/// <summary>
/// Exercises rising edges, releases, multi-frame holds, and the otherwise-unused repeat
/// path from the NMI controller routine.
/// </summary>
static void VerifyControllerInputLatch()
{
    ushort button = (ushort)SnesButton.A;
    var input = new ControllerInputState(initialRepeatDelay: 2, subsequentRepeatDelay: 1);

    input.Latch(button);
    AssertEqual(button, input.Current, "controller current on first press");
    AssertEqual(button, input.NewlyPressed, "controller rising edge on first press");
    AssertEqual(button, input.NewlyPressedWithRepeat, "repeat output includes real first press");
    AssertEqual((ushort)2, input.RepeatTimer, "first press loads initial repeat delay");

    input.Latch(button);
    AssertEqual((ushort)0, input.NewlyPressed, "stable hold has no new edge");
    AssertEqual((ushort)0, input.NewlyPressedWithRepeat, "first held frame has no repeat");
    AssertEqual((ushort)1, input.RepeatTimer, "held input decrements repeat timer");

    input.Latch(button);
    AssertEqual(button, input.NewlyPressedWithRepeat, "repeat timer zero emits held input");
    AssertEqual((ushort)1, input.RepeatTimer, "repeat pulse loads subsequent delay");

    input.Latch(0);
    AssertEqual((ushort)0, input.NewlyPressed, "release is not a rising edge");
    AssertEqual((ushort)2, input.RepeatTimer, "release reloads initial repeat delay");

    Console.WriteLine("  Input: NMI latch, rising edges, and repeat countdown agree.");
}

/// <summary>
/// Confirms the integrated frame seam preserves NMI ordering, accepted/lagged counters,
/// controller latching, VRAM draining, and timer dispatch.
/// </summary>
static void VerifyFrameRuntime()
{
    var bus = new TestAddressSpace();
    bus.WriteBytes(0x7e1234, [0xca, 0xfe]);
    var runtime = new SuperMetroidRuntime(bus);
    runtime.VramWrites.Enqueue(2, 0x7e1234, 0x0020);
    runtime.EscapeTimer.RequestCeresStart();

    RuntimeFrameResult first = runtime.StepFrame((ushort)SnesButton.Start);
    AssertEqual((ushort)1, first.FrameNumber, "first accepted runtime frame");
    AssertEqual((ushort)SnesButton.Start, first.ControllerNewInput, "runtime latches controller before logic");
    AssertEqual(EscapeTimerState.InitialDelay, first.EscapeTimerState, "runtime dispatches timer after NMI");
    AssertEqual((byte)0xca, runtime.Vram.ReadByte(0x40), "runtime NMI drains VRAM low byte");
    AssertEqual((byte)0xfe, runtime.Vram.ReadByte(0x41), "runtime NMI drains VRAM high byte");
    AssertEqual(0, runtime.VramWrites.TailInBytes, "runtime NMI clears VRAM queue");

    RuntimeFrameResult second = runtime.StepFrame((ushort)SnesButton.Start);
    AssertEqual((ushort)0, second.ControllerNewInput, "second runtime frame sees stable hold");
    AssertEqual((ushort)2, runtime.NmiFrameCounter, "accepted NMI counter advances twice");

    // A lag NMI must not sample the changed input or advance accepted-frame state.
    runtime.RunNmi((ushort)SnesButton.A, mainLoopRequestedNmi: false);
    AssertEqual((ushort)SnesButton.Start, runtime.Controller1.Current, "lag NMI skips controller read");
    AssertEqual((ushort)2, runtime.NmiFrameCounter, "lag NMI skips accepted frame counter");
    AssertEqual((ushort)1, runtime.NmiLagCounter, "lag NMI increments consecutive lag");
    AssertEqual((ushort)1, runtime.MaximumNmiLag, "lag NMI records maximum lag");
    AssertEqual((ushort)3, runtime.NmiCounterIncludingLag, "all-NMI counter includes lag");

    Console.WriteLine("  Runtime: frame seam, NMI order, and lag accounting agree.");
}

/// <summary>
/// Checks FastROM/slow-bank LoROM mirrors, high-bank offsets, WRAM mirrors, SRAM mirrors,
/// and deliberate failures for unimplemented hardware and ROM writes.
/// </summary>
static void VerifySuperMetroidAddressSpace()
{
    var rom = new byte[SuperMetroidAddressSpace.RetailRomByteCount];
    rom[0x000000] = 0x80;
    rom[0x008000] = 0x81;
    rom[0x200000] = 0xc0;
    var bus = new SuperMetroidAddressSpace(rom);

    // Banks $00 and $80 are timing mirrors of the first physical 32 KiB ROM bank.
    AssertEqual((byte)0x80, bus.ReadByte(0x008000), "slow-bank first LoROM byte");
    AssertEqual((byte)0x80, bus.ReadByte(0x808000), "FastROM mirror first LoROM byte");
    AssertEqual((byte)0x81, bus.ReadByte(0x818000), "next FastROM bank advances $8000 bytes");
    AssertEqual((byte)0xc0, bus.ReadByte(0xc08000), "bank C0 maps to physical ROM $200000");
    AssertEqual(0x200000, SuperMetroidAddressSpace.ToRomOffset(0xc08000), "native RomPtr mask mapping");

    bus.WriteByte(0x7e1234, 0x55);
    AssertEqual((byte)0x55, bus.ReadByte(0x001234), "bank 00 low WRAM mirror");
    AssertEqual((byte)0x55, bus.ReadByte(0x801234), "bank 80 low WRAM mirror");
    bus.WriteByte(0x7f1234, 0x66);
    AssertEqual((byte)0x66, bus.ReadByte(0x7f1234), "second physical WRAM bank");
    AssertEqual((byte)0x55, bus.ReadByte(0x7e1234), "WRAM banks remain independent");

    bus.WriteByte(0x700123, 0x77);
    AssertEqual((byte)0x77, bus.ReadByte(0x702123), "8 KiB SRAM offset mirror");
    AssertEqual((byte)0x77, bus.ReadByte(0xf00123), "8 KiB SRAM bank mirror");

    AssertThrows<InvalidOperationException>(() => bus.WriteByte(0x808000, 0), "ROM writes rejected");
    AssertThrows<NotSupportedException>(() => bus.ReadByte(0x004000), "unimplemented register/expansion read rejected");
    AssertThrows<ArgumentOutOfRangeException>(() => SuperMetroidAddressSpace.ToRomOffset(0x800000), "lower LoROM offset rejected");

    Console.WriteLine("  Bus: LoROM, WRAM, SRAM mirrors and protection agree.");
}

/// <summary>
/// Builds synthetic packed spritemaps to verify signed/modular offsets, shared high-OAM
/// pairs, palette replacement, clipping, finalization, and the exact 544-byte upload size.
/// </summary>
static void VerifyOamSpritemapPacking()
{
    var bus = new TestAddressSpace();
    var oam = new OamBuffer();

    // Two entries at $80:8000. Entry 0 is large, +5 X, -2 Y. Entry 1 is small,
    // -16 X, +32 Y. Attribute palettes are intentionally replaced by caller palette 5.
    bus.WriteBytes(0x808000, [
        0x02, 0x00,
        0x05, 0x80, 0xfe, 0xaa, 0x64,
        0xf0, 0x01, 0x20, 0x55, 0x21,
    ]);

    oam.BeginFrame();
    oam.AddOnScreenSpritemap(bus, 0x808000, originX: 0x00fe, originY: 0x0001, paletteBits: 0x0a00);

    OamEntry first = oam.GetEntry(0);
    AssertEqual(0x103, first.X, "OAM first 9-bit X");
    AssertEqual((byte)0xff, first.Y, "OAM negative Y wraps above screen");
    AssertTrue(first.IsLarge, "OAM size bit extracted from encoded X word");
    AssertEqual(5, first.Palette, "OAM caller palette replaces source palette");
    AssertEqual(0x0aa, first.TileNumber, "OAM tile number retained");

    OamEntry second = oam.GetEntry(1);
    AssertEqual(0x0ee, second.X, "OAM signed 9-bit negative X offset");
    AssertEqual((byte)0x21, second.Y, "OAM positive Y offset");
    AssertTrue(!second.IsLarge, "OAM second size bit clear");
    AssertEqual((byte)0x03, oam.HighTable[0], "OAM four-sprite shared high byte");

    oam.FinalizeFrame();
    AssertEqual(2, oam.LastFinalizedSpriteCount, "OAM finalized used count");
    AssertEqual((byte)0xf0, oam.GetEntry(2).Y, "OAM first unused sprite parked offscreen");
    AssertEqual(0, oam.NextByteOffset, "OAM finalization resets stack pointer");
    AssertEqual(OamBuffer.UploadByteCount, oam.CreateUploadPayload().Length, "OAM DMA payload size");

    // The main loop's finalized buffer does not become PPU-visible until accepted NMI.
    var displayedOam = new OamBuffer();
    displayedOam.CopyFinalizedFrom(oam);
    AssertEqual(2, displayedOam.LastFinalizedSpriteCount, "NMI OAM copy retains used count");
    AssertEqual(first, displayedOam.GetEntry(0), "NMI OAM copy retains split low/high record");

    // A positive Y offset reaching $E0 takes the explicit X=$180/Y=$E0 hide path.
    bus.WriteBytes(0x808100, [0x01, 0x00, 0x00, 0x00, 0x20, 0x01, 0x00]);
    oam.BeginFrame();
    oam.AddOnScreenSpritemap(bus, 0x808100, originX: 0, originY: 0x00c0, paletteBits: 0);
    OamEntry clipped = oam.GetEntry(0);
    AssertEqual(0x180, clipped.X, "vertically clipped OAM X park position");
    AssertEqual((byte)0xe0, clipped.Y, "vertically clipped OAM Y park position");

    // Enemy projectiles use the distinct bank-$8D loader at `$81:8C0A/$81:8C7F`.
    // The first entry is large/+5 X/-2 Y; the second is small/-16 X/+2 Y. Source
    // attribute `$21FE + $04` deliberately carries from tile number into the high byte,
    // proving that the native routine uses ADC before ORing graphics-index palette bits.
    bus.WriteBytes(0x8d9800, [
        0x02, 0x00,
        0x05, 0x80, 0xfe, 0x0f, 0x20,
        0xf0, 0x01, 0x02, 0xfe, 0x21,
    ]);
    oam.BeginFrame();
    oam.AddEnemyProjectileSpritemap(
        bus,
        bank8dSpritemapPointer: 0x9800,
        originX: 0x00fe,
        originY: 0x0001,
        graphicsIndex: 0x0a04,
        originYIsOnScreen: true);
    OamEntry enemyFirst = oam.GetEntry(0);
    AssertEqual(0x103, enemyFirst.X, "enemy-projectile complete encoded X addition");
    AssertEqual((byte)0xf0, enemyFirst.Y, "on-screen origin hides uncrossed negative Y");
    AssertTrue(enemyFirst.IsLarge, "enemy-projectile size comes from encoded X bit fifteen");
    AssertEqual(0x013, enemyFirst.TileNumber, "enemy-projectile base tile uses addition");
    AssertEqual(5, enemyFirst.Palette, "enemy-projectile graphics palette OR");
    AssertEqual(2, enemyFirst.Priority, "enemy-projectile source priority survives palette OR");
    OamEntry enemySecond = oam.GetEntry(1);
    AssertEqual(0x0ee, enemySecond.X, "enemy-projectile signed nine-bit negative X");
    AssertEqual((byte)0x03, enemySecond.Y, "on-screen origin retains uncrossed positive Y");
    AssertTrue(!enemySecond.IsLarge, "enemy-projectile small size bit");
    AssertEqual(0x002, enemySecond.TileNumber, "enemy-projectile ADC tile carry");

    // An origin at Y=$FFFF selects `$81:8C7F`'s opposite carry rule. The negative piece
    // remains above screen and is parked, while +2 crosses into visible Y=$01.
    oam.BeginFrame();
    oam.AddEnemyProjectileSpritemap(
        bus,
        bank8dSpritemapPointer: 0x9800,
        originX: 0,
        originY: 0xffff,
        graphicsIndex: 0,
        originYIsOnScreen: false);
    AssertEqual((byte)0xf0, oam.GetEntry(0).Y,
        "off-screen origin hides negative piece that remains above screen");
    AssertEqual((byte)0x01, oam.GetEntry(1).Y,
        "off-screen origin admits positive piece crossing into screen");

    Console.WriteLine(
        "  OAM: spritemap packing, enemy-projectile arithmetic/wrap, and finalization agree.");
}

/// <summary>
/// Fixes the first Samus slice to the retail pose-$01 pointer chain and independently
/// checks palette placement, split tile DMA, position math, and native OAM attributes.
/// </summary>
static void VerifySamusRenderingSlice()
{
    var bus = new TestAddressSpace();
    SeedPoseOneSamusData(bus);

    var samus = new SamusState
    {
        Pose = SamusState.FacingRightNormalPose,
        AnimationFrame = 0,
        XPosition = 0x0480,
        YPosition = 0x0086,
    };
    var cgram = new SnesCgram();
    var vram = new SnesVram();
    var oam = new OamBuffer();

    samus.LoadPowerSuitPalette(bus, cgram);
    AssertEqual((ushort)0x3800, cgram.Colors[192], "Samus power-suit palette color zero at CGRAM 192");
    AssertEqual((ushort)0x000d, cgram.Colors[207], "Samus power-suit palette color fifteen at CGRAM 207");

    samus.PrimeGraphics(bus);
    AssertEqual(0x92d0b0, samus.TileTransfers.TopDefinitionAddress, "pose 1 frame 0 top tile definition");
    AssertEqual(0x92d1c8, samus.TileTransfers.BottomDefinitionAddress, "pose 1 frame 0 bottom tile definition");
    AssertTrue(samus.TileTransfers.TopTransferEnabled, "Samus top tile DMA flag");
    AssertTrue(samus.TileTransfers.BottomTransferEnabled, "Samus bottom tile DMA flag");

    samus.TileTransfers.TransferToVram(bus, vram);
    AssertEqual((byte)0x10, vram.ReadByte(0xc000), "Samus top part 1 reaches VRAM word $6000");
    AssertEqual((byte)0x20, vram.ReadByte(0xc200), "Samus top part 2 reaches VRAM word $6100");
    AssertEqual((byte)0x30, vram.ReadByte(0xc100), "Samus bottom part 1 reaches VRAM word $6080");
    AssertEqual((byte)0x40, vram.ReadByte(0xc300), "Samus bottom part 2 reaches VRAM word $6180");

    samus.InitializeAnimation(bus);
    AssertEqual((ushort)10, samus.AnimationFrameTimer, "pose 1 initial animation delay");

    // Frames 0-3 each last ten calls. The fifth byte is command $F6, which returns a
    // healthy Samus to frame zero rather than ever exposing command index four as art.
    for (int tick = 0; tick < 9; tick++)
        samus.AnimateNoFx(bus);
    AssertEqual((ushort)0, samus.AnimationFrame, "standing frame remains zero for first nine ticks");
    AssertEqual((ushort)1, samus.AnimationFrameTimer, "standing timer reaches one before advance");
    samus.AnimateNoFx(bus);
    AssertEqual((ushort)1, samus.AnimationFrame, "standing frame advances on tenth tick");
    AssertEqual((ushort)10, samus.AnimationFrameTimer, "next standing frame reloads ten ticks");
    for (int tick = 0; tick < 30; tick++)
        samus.AnimateNoFx(bus);
    AssertEqual((ushort)0, samus.AnimationFrame, "healthy $F6 command loops standing animation");
    AssertEqual((byte)0xf6, samus.LastAnimationDelayCommand!.Value, "healthy standing loop command");

    // Below 30 energy, the same command enters frames 5-8. Command $FE,$04 then subtracts
    // four byte positions and loops that faster eight-tick breathing sequence.
    samus.Health = 29;
    samus.InitializeAnimation(bus);
    for (int tick = 0; tick < 40; tick++)
        samus.AnimateNoFx(bus);
    AssertEqual((ushort)5, samus.AnimationFrame, "low-health $F6 enters alternate sequence");
    AssertEqual((ushort)8, samus.AnimationFrameTimer, "low-health sequence uses eight-tick delay");
    for (int tick = 0; tick < 32; tick++)
        samus.AnimateNoFx(bus);
    AssertEqual((ushort)5, samus.AnimationFrame, "$FE,$04 loops low-health standing sequence");
    AssertEqual((byte)0xfe, samus.LastAnimationDelayCommand!.Value, "low-health backward-loop command");

    // Restore the ordinary debugger scenario before verifying frame-zero drawing below.
    samus.Health = 99;
    samus.InitializeAnimation(bus);

    oam.BeginFrame();
    samus.Draw(bus, oam, layer1X: 0x0400, layer1Y: 0);
    oam.FinalizeFrame();

    // Pose $01's graphics Y offset is six, so world (1152,134) becomes origin (128,128).
    AssertEqual((ushort)128, samus.SpritemapXPosition, "Samus default screen X calculation");
    AssertEqual((ushort)128, samus.SpritemapYPosition, "Samus signed graphics-Y offset calculation");
    AssertEqual((ushort)0x019a, samus.TopSpritemapIndex, "Samus pose 1 top spritemap index");
    AssertEqual((ushort)0x04aa, samus.BottomSpritemapIndex, "Samus pose 1 bottom spritemap index");
    AssertEqual(7, oam.LastFinalizedSpriteCount, "Samus pose 1 emits four top and three bottom OBJs");

    OamEntry firstTop = oam.GetEntry(0);
    AssertEqual(121, firstTop.X, "Samus top OBJ signed X offset");
    AssertEqual((byte)120, firstTop.Y, "Samus top OBJ signed Y offset");
    AssertTrue(firstTop.IsLarge, "Samus top OBJ preserves ROM size bit");
    AssertEqual(4, firstTop.Palette, "Samus top OBJ preserves ROM palette");
    AssertEqual(2, firstTop.Priority, "Samus top OBJ preserves ROM priority");
    AssertEqual(0, firstTop.TileNumber, "Samus top OBJ tile number");

    OamEntry firstBottom = oam.GetEntry(4);
    AssertEqual(113, firstBottom.X, "Samus bottom OBJ signed X offset");
    AssertEqual((byte)144, firstBottom.Y, "Samus bottom OBJ positive Y offset");
    AssertEqual(8, firstBottom.TileNumber, "Samus bottom OBJ tile number");

    // The verified ordinary transition applies pose $09 after movement/animation and before
    // drawing. Its movement type one uses default position math and always draws both halves.
    SeedPoseNineSamusData(bus);
    samus.ApplyStandingRightToRunningRight(bus);
    AssertEqual((byte)0x09, samus.Pose, "standing-right transition applies running-right pose");
    AssertEqual((ushort)0, samus.AnimationFrame, "running transition resets animation frame");
    AssertEqual((ushort)2, samus.AnimationFrameTimer, "running pose first delay byte");
    AssertEqual((ushort)21, samus.Kinematics.YRadius, "running pose refreshes collision radius");

    oam.BeginFrame();
    samus.Draw(bus, oam, layer1X: 0x0400, layer1Y: 0);
    oam.FinalizeFrame();
    AssertEqual((ushort)0x00f9, samus.TopSpritemapIndex, "running pose top spritemap index");
    AssertEqual((ushort)0x00e3, samus.BottomSpritemapIndex, "running pose bottom spritemap index");
    AssertEqual(2, oam.LastFinalizedSpriteCount, "synthetic running pose draws top and bottom pieces");

    samus.ApplyRunningRightToStandingRight(bus);
    AssertEqual((byte)0x01, samus.Pose, "running no-button fallback applies standing-right pose");
    AssertEqual((ushort)0, samus.AnimationFrame, "standing fallback resets animation frame");
    AssertEqual((ushort)10, samus.AnimationFrameTimer, "standing fallback reloads frame-zero delay");

    // `$90:868D` inserts one direct small-OBJ write between pose `$00`'s ordinary top and
    // bottom spritemaps. This record is easy to lose in a high-level “draw both halves”
    // abstraction, so verify its exact OAM order, coordinates, size, and attribute word.
    SeedForwardFacingSamusData(bus);
    samus.EquippedItems = 0;
    samus.HorizontalSpeed.BaseSpeed = 3;
    samus.Kinematics.YSpeed = 2;
    samus.ApplyForwardFacingPoseSetup(bus);
    AssertEqual(SamusState.ForwardFacingPowerSuitPose, samus.Pose, "no suit selects power forward pose");
    AssertEqual((ushort)24, samus.Kinematics.YRadius, "power forward setup reads radius 24");
    AssertEqual((ushort)8, samus.AnimationFrameTimer, "power forward setup reads delay eight");
    AssertEqual((ushort)0, samus.HorizontalSpeed.BaseSpeed, "forward setup clears base X speed");
    AssertEqual((ushort)0, samus.Kinematics.YSpeed, "forward setup clears Y speed");
    oam.BeginFrame();
    samus.Draw(bus, oam, layer1X: 0x0400, layer1Y: 0);
    oam.FinalizeFrame();
    AssertEqual((ushort)0x0002, samus.TopSpritemapIndex, "power-suit forward top spritemap index");
    AssertEqual((ushort)0x0062, samus.BottomSpritemapIndex, "power-suit forward bottom spritemap index");
    AssertEqual(3, oam.LastFinalizedSpriteCount, "power-suit forward top/chest/bottom OAM order");
    OamEntry chestCover = oam.GetEntry(1);
    AssertEqual(121, chestCover.X, "forward chest-cover X is Samus screen X minus seven");
    AssertEqual((byte)117, chestCover.Y, "forward chest-cover Y is Samus screen Y minus seventeen");
    AssertEqual(0x21, chestCover.TileNumber, "forward chest-cover tile number");
    AssertEqual(4, chestCover.Palette, "forward chest-cover palette");
    AssertEqual(3, chestCover.Priority, "forward chest-cover priority");
    AssertTrue(!chestCover.IsLarge, "forward chest-cover is one small OBJ");

    // `$9B` uses dedicated suited art and therefore must not inherit `$00`'s chest patch.
    samus.EquippedItems = 0x0001;
    samus.ApplyForwardFacingPoseSetup(bus);
    AssertEqual(SamusState.ForwardFacingSuitedPose, samus.Pose, "Varia selects suited forward pose");
    oam.BeginFrame();
    samus.Draw(bus, oam, layer1X: 0x0400, layer1Y: 0);
    oam.FinalizeFrame();
    AssertEqual((ushort)0x00c2, samus.TopSpritemapIndex, "suited forward top spritemap index");
    AssertEqual((ushort)0x0122, samus.BottomSpritemapIndex, "suited forward bottom spritemap index");
    AssertEqual(2, oam.LastFinalizedSpriteCount, "suited forward emits no power-suit chest patch");

    Console.WriteLine("  Samus: standing/running/forward pose tables, split tile DMA, position, and OAM agree.");
}

/// <summary>
/// Constructs the exact pointer topology and meaningful bytes for retail pose $01 frame 0.
/// The graphics payload uses diagnostic values because correctness here is about routing;
/// the DebugRunner separately executes the same code against the user's actual cartridge.
/// </summary>
static void SeedPoseOneSamusData(TestAddressSpace bus)
{
    // Pose definition $91:B631: facing right, standing movement type, +6 graphics Y.
    bus.WriteBytes(0x91b631, [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]);

    // Pose 1's delay-table pointer and its complete healthy/low-health bytecode stream.
    WriteTestWord(bus, 0x91b012, 0xb298);
    bus.WriteBytes(0x91b298, [0x0a, 0x0a, 0x0a, 0x0a, 0xf6, 0x08, 0x08, 0x08, 0x08, 0xfe, 0x04]);

    // Pose-to-spritemap base indices and the two pointer-table entries they resolve.
    WriteTestWord(bus, 0x929265, 0x019a);
    WriteTestWord(bus, 0x92945f, 0x04aa);
    WriteTestWord(bus, 0x9283c1, 0xa072);
    WriteTestWord(bus, 0x9289e1, 0xadbb);

    // Top spritemap $92:A072: four pieces. The assembler macro ORs size bit $8000
    // into X values such as $43F9, yielding the deliberately odd encoded word $C3F9.
    bus.WriteBytes(0x92a072, [
        0x04, 0x00,
        0xf9, 0xc3, 0xf8, 0x00, 0x28,
        0xf9, 0xc3, 0xf0, 0x02, 0x28,
        0x0a, 0x00, 0xfd, 0x04, 0x28,
        0x02, 0x00, 0xfd, 0x05, 0x28,
    ]);

    // Bottom spritemap $92:ADBB: three large pieces using tile names $08/$0A/$0C.
    bus.WriteBytes(0x92adbb, [
        0x03, 0x00,
        0xf1, 0xc3, 0x10, 0x08, 0x28,
        0xf9, 0xc3, 0x10, 0x0a, 0x28,
        0xf9, 0xc3, 0x00, 0x0c, 0x28,
    ]);

    // Pose 1 -> animation list $DB48; frame 0 -> top set 7/position C and bottom
    // set 0/position 6. Their list pointers plus position*7 produce D0B0 and D1C8.
    WriteTestWord(bus, 0x92d950, 0xdb48);
    bus.WriteBytes(0x92db48, [0x07, 0x0c, 0x00, 0x06]);
    WriteTestWord(bus, 0x92d92c, 0xd05c);
    WriteTestWord(bus, 0x92d938, 0xd19e);
    bus.WriteBytes(0x92d0b0, [0x00, 0xe1, 0x9c, 0xc0, 0x00, 0x80, 0x00]);
    bus.WriteBytes(0x92d1c8, [0x20, 0x88, 0x9d, 0xc0, 0x00, 0xc0, 0x00]);

    // Distinct first bytes prove each of the four fixed NMI destinations independently.
    bus.WriteByte(0x9ce100, 0x10);
    bus.WriteByte(0x9ce1c0, 0x20);
    bus.WriteByte(0x9d8820, 0x30);
    bus.WriteByte(0x9d88e0, 0x40);

    ushort[] powerSuitColors = [
        0x3800, 0x0108, 0x03bd, 0x1405, 0x3be0, 0x21a8, 0x579f, 0x4ad2,
        0x3a4e, 0x00bb, 0x02b5, 0x016b, 0x0252, 0x1104, 0x0074, 0x000d,
    ];
    for (int color = 0; color < powerSuitColors.Length; color++)
        WriteTestWord(bus, 0x9b9400 + color * 2, powerSuitColors[color]);
}

/// <summary>
/// Seeds the two front-view pose records with compact diagnostic spritemaps. The base
/// indices, pose metadata, and raw chest-cover attributes are retail values; the one-piece
/// body maps keep this verifier focused on routing because DebugRunner covers real artwork.
/// </summary>
static void SeedForwardFacingSamusData(TestAddressSpace bus)
{
    // Both records face neither left nor right, use standing movement type zero, have an
    // eight-pixel graphics-origin offset, and use the front-view radius of 24 pixels.
    bus.WriteBytes(0x91b629, [0x00, 0x00, 0xff, 0xff, 0x08, 0x00, 0x18, 0x00]);
    bus.WriteBytes(0x91bb01, [0x00, 0x00, 0xff, 0xff, 0x08, 0x00, 0x18, 0x00]);

    // `$00/$9B` share `$91:B56F`: eight ticks on frame zero followed by command `$FF`.
    WriteTestWord(bus, 0x91b010, 0xb56f);
    WriteTestWord(bus, 0x91b146, 0xb56f);
    bus.WriteBytes(0x91b56f, [0x08, 0xff]);

    // Retail frame-zero bases: `$00` top/bottom `$0002/$0062`, `$9B` `$00C2/$0122`.
    WriteTestWord(bus, 0x929263, 0x0002);
    WriteTestWord(bus, 0x92945d, 0x0062);
    WriteTestWord(bus, 0x929399, 0x00c2);
    WriteTestWord(bus, 0x929593, 0x0122);

    // Point those four indices at one-piece diagnostic maps. The direct `$3821` chest
    // record is not included here: production Draw must append it independently.
    WriteTestWord(bus, 0x928091, 0xa200);
    WriteTestWord(bus, 0x928151, 0xa210);
    WriteTestWord(bus, 0x928211, 0xa220);
    WriteTestWord(bus, 0x9282d1, 0xa230);
    bus.WriteBytes(0x92a200, [0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x28]);
    bus.WriteBytes(0x92a210, [0x01, 0x00, 0x00, 0x00, 0x10, 0x08, 0x28]);
    bus.WriteBytes(0x92a220, [0x01, 0x00, 0x00, 0x00, 0x00, 0x01, 0x28]);
    bus.WriteBytes(0x92a230, [0x01, 0x00, 0x00, 0x00, 0x10, 0x09, 0x28]);

    // Draw's final tile-selection step follows each pose's four-byte frame record. Zeroed
    // set pointers are safe because this verifier does not execute the resulting DMA.
    WriteTestWord(bus, 0x92d94e, 0xe000);
    WriteTestWord(bus, 0x92da84, 0xe004);
    bus.WriteBytes(0x92e000, [0x00, 0x00, 0xff, 0x00, 0x00, 0x00, 0xff, 0x00]);
}

/// <summary>Seeds the retail pose-$09 pointer topology with compact diagnostic spritemaps.</summary>
static void SeedPoseNineSamusData(TestAddressSpace bus)
{
    // Pose $09: facing right, movement type one, new-pose-unless-buttons $01, +6 graphics
    // offset, and radius 21. These are the eight retail bytes at $91:B671.
    bus.WriteBytes(0x91b671, [0x08, 0x01, 0x01, 0x02, 0x06, 0x00, 0x15, 0x00]);

    // Ten alternating 2/3-tick running frames followed by command $FF back to frame zero.
    WriteTestWord(bus, 0x91b022, 0xb20a);
    bus.WriteBytes(0x91b20a, [0x02, 0x03, 0x02, 0x03, 0x02, 0x03, 0x02, 0x03, 0x02, 0x03, 0xff]);

    // Retail frame-zero bases are top $00F9 and bottom $00E3. Their pointer slots select
    // compact one-piece diagnostic maps; production DebugRunner reads the real maps.
    WriteTestWord(bus, 0x929275, 0x00f9);
    WriteTestWord(bus, 0x92946f, 0x00e3);
    WriteTestWord(bus, 0x92827f, 0xa100);
    WriteTestWord(bus, 0x928253, 0xa108);
    bus.WriteBytes(0x92a100, [0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x28]);
    bus.WriteBytes(0x92a108, [0x01, 0x00, 0x00, 0x08, 0x08, 0x28, 0x28]);

    // Reuse pose one's already-seeded diagnostic tile definitions by making pose $09 frame
    // zero choose the same top set/position and bottom set/position record.
    WriteTestWord(bus, 0x92d960, 0xdc48);
    bus.WriteBytes(0x92dc48, [0x07, 0x0c, 0x00, 0x06]);
}

/// <summary>
/// Checks required-new/required-held masks, extra-button acceptance, ROM priority order,
/// terminators, and same-pose suppression in the bank-$91 prospective-pose lookup.
/// </summary>
static void VerifySamusPoseTransitionMatching()
{
    var bus = new TestAddressSpace();

    // Pose $01 points at a compact synthetic table shaped like the real $91:A0EC data.
    // Jump+Up outranks plain Up, which outranks Right because the matcher stops at the
    // first record whose complete required masks are present.
    WriteTestWord(bus, 0x919ee4, 0xa0ec);
    bus.WriteBytes(0x91a0ec, [
        0x80, 0x00, 0x00, 0x08, 0x55, 0x00,
        0x00, 0x00, 0x00, 0x08, 0x03, 0x00,
        0x00, 0x00, 0x00, 0x01, 0x09, 0x00,
        0x00, 0x00, 0x00, 0x02, 0x01, 0x00,
        0xff, 0xff,
    ]);

    SamusPoseTransition jumpUp = SamusPoseTransitionTable.Find(
        bus,
        currentPose: 1,
        canonicalHeldInput: 0x0980,
        canonicalNewInput: 0x0080)!.Value;
    AssertEqual((ushort)0x55, jumpUp.ProspectivePose, "Samus transition required-new plus held chord");
    AssertEqual(0x91a0ec, jumpUp.EntryAddress, "Samus transition winning ROM record address");

    SamusPoseTransition up = SamusPoseTransitionTable.Find(
        bus,
        currentPose: 1,
        canonicalHeldInput: 0x0900,
        canonicalNewInput: 0)!.Value;
    AssertEqual((ushort)0x03, up.ProspectivePose, "Samus transition permits extra held direction");

    SamusPoseTransition right = SamusPoseTransitionTable.Find(
        bus,
        currentPose: 1,
        canonicalHeldInput: 0x0100,
        canonicalNewInput: 0)!.Value;
    AssertEqual((ushort)0x09, right.ProspectivePose, "Samus standing Right proposes running pose");

    AssertEqual<SamusPoseTransition?>(null,
        SamusPoseTransitionTable.Find(bus, 1, canonicalHeldInput: 0, canonicalNewInput: 0),
        "Samus zero input bypasses transition table");
    AssertEqual<SamusPoseTransition?>(null,
        SamusPoseTransitionTable.Find(bus, 1, canonicalHeldInput: 0x0200, canonicalNewInput: 0),
        "Samus same-pose transition is suppressed");
    AssertEqual<SamusPoseTransition?>(null,
        SamusPoseTransitionTable.Find(bus, 1, canonicalHeldInput: 0x0400, canonicalNewInput: 0),
        "Samus transition terminator returns no match");

    Console.WriteLine("  Samus input: ROM transition masks, priority, and terminator agree.");
}

/// <summary>
/// Exercises the exact normal-air pointer chain and word-oriented arithmetic from
/// <c>$94:97D0</c>, <c>$90:9BD1</c>, <c>$90:9A7E</c>, and <c>$90:E4E6-$90:E4E5</c>.
/// Synthetic bus bytes are the retail running entry at $90:9F61, so these assertions stay
/// deterministic while the DebugRunner independently reads the user's cartridge.
/// </summary>
static void VerifySamusHorizontalSpeed()
{
    var bus = new TestAddressSpace();

    // Normal air stores $9F55. Running movement type one advances by one more 12-byte
    // record to $9F61: acceleration 0.3000, maximum 2.C000, deceleration 0.8000.
    bus.WriteBytes(0x909f61, [
        0x00, 0x00, 0x00, 0x30,
        0x02, 0x00, 0x00, 0xc0,
        0x00, 0x00, 0x00, 0x80,
    ]);

    var speed = new SamusHorizontalSpeedState();
    speed.SelectNormalAirSpeedTable();
    AssertEqual(0x909f61, speed.ResolveEntryAddress(movementType: 1), "running speed entry pointer chain");

    SpeedTableEntry entry = speed.ReadEntry(bus, movementType: 1);
    AssertEqual((ushort)0x0000, entry.Acceleration, "running acceleration whole word");
    AssertEqual((ushort)0x3000, entry.AccelerationSubspeed, "running acceleration fraction");
    AssertEqual((ushort)0x0002, entry.MaximumSpeed, "running maximum whole word");
    AssertEqual((ushort)0xc000, entry.MaximumSubspeed, "running maximum fraction");
    AssertEqual((ushort)0x8000, entry.DecelerationSubspeed, "running deceleration fraction");

    // Four acceleration calls produce 0.C000 exactly. Fifteen calls would reach 2.D000,
    // so the native quirked comparison clamps that result down to the table's 2.C000 cap.
    for (int frame = 0; frame < 4; frame++)
        speed.CalculateBaseSpeed(bus, movementType: 1);
    AssertEqual(0x0000c000u, speed.BaseFixed, "running acceleration after four calls");
    for (int frame = 4; frame < 15; frame++)
        speed.CalculateBaseSpeed(bus, movementType: 1);
    AssertEqual(0x0002c000u, speed.BaseFixed, "running acceleration clamps at 2.C000");

    // Deceleration subtracts 0.8000 per call. Crossing below zero makes the signed high
    // word negative, clearing both halves and restoring acceleration mode zero.
    speed.AccelerationMode = 2;
    for (int frame = 0; frame < 6; frame++)
        speed.CalculateBaseSpeed(bus, movementType: 1);
    AssertEqual(0u, speed.BaseFixed, "running deceleration underflow clears speed");
    AssertEqual((ushort)0, speed.AccelerationMode, "running deceleration restores acceleration mode");

    // `$90:973E` adds the literal no-booster 0.1000 pair once per running+B frame. The
    // native cap test runs before addition, so call 32 reaches exactly 2.0000 and call 33
    // performs the visible clamp write without changing the pair.
    for (int frame = 0; frame < 32; frame++)
    {
        speed.HandleExtraRunSpeed(
            movementType: 1,
            controllerInput: (ushort)SnesButton.B,
            speedBoosterEquipped: false);
    }
    AssertTrue(speed.HasRunningMomentum, "ordinary Dash establishes native momentum flag");
    AssertEqual((ushort)0, speed.SpeedBoostCounter, "ordinary Dash leaves booster stage zero");
    AssertEqual((ushort)2, speed.ExtraRunSpeed, "ordinary Dash whole-speed cap");
    AssertEqual((ushort)0, speed.ExtraRunSubspeed, "ordinary Dash fractional-speed cap");
    speed.HandleExtraRunSpeed(1, (ushort)SnesButton.B, speedBoosterEquipped: false);
    AssertEqual((ushort)2, speed.ExtraRunSpeed, "ordinary Dash remains clamped on next call");

    // B release and an airborne movement type both take `$90:9808`; a set momentum flag
    // bypasses the numeric clear. Only the separately invoked cancel routine clears the
    // flag/counter, after which another non-running call clears the retained pair.
    speed.HandleExtraRunSpeed(1, controllerInput: 0, speedBoosterEquipped: false);
    speed.HandleExtraRunSpeed(3, controllerInput: 0, speedBoosterEquipped: false);
    AssertEqual((ushort)2, speed.ExtraRunSpeed, "Dash release and spin jump retain extra speed");
    speed.CancelRunningMomentum(poseXDirection: 8);
    AssertTrue(!speed.HasRunningMomentum, "CancelSpeedBoost clears ordinary momentum flag");
    speed.HandleExtraRunSpeed(3, controllerInput: 0, speedBoosterEquipped: false);
    AssertEqual((ushort)0, speed.ExtraRunSpeed, "post-cancel airborne handler clears extra speed");

    // The animation side reads its ordinary-Dash cadence through the live pointer at
    // `$91:B5D1`. The pose-specific stream deliberately uses different delays so these
    // checks would fail if the implementation merely sped up a host timer by coincidence.
    bus.WriteBytes(0x91b671, [0x08, 0x01, 0xff, 0x02, 0x00, 0x00, 0x15, 0x00]); // pose $09
    WriteTestWord(bus, 0x91b022, 0xc000); // pose $09's normal delay stream
    WriteTestWord(bus, 0x91b5d1, 0xc100); // shared ordinary-Dash delay stream pointer
    bus.WriteBytes(0x91c000, [0x09, 0x09, 0xff]);
    bus.WriteBytes(0x91c100, [0x02, 0x03, 0xff]);
    var dashAnimation = new SamusState { Pose = SamusState.MovingRightNormalPose };
    dashAnimation.InitializeAnimation(bus);
    dashAnimation.HorizontalSpeed.HandleExtraRunSpeed(
        movementType: 1,
        controllerInput: (ushort)SnesButton.B,
        speedBoosterEquipped: false);
    for (int tick = 0; tick < 9; tick++)
        dashAnimation.AnimateNoFx(bus, (ushort)SnesButton.B);
    AssertEqual((ushort)1, dashAnimation.AnimationFrame, "Dash advances into running frame one");
    AssertEqual((ushort)3, dashAnimation.AnimationFrameTimer, "Dash selects shared frame-one delay");
    for (int tick = 0; tick < 3; tick++)
        dashAnimation.AnimateNoFx(bus, (ushort)SnesButton.B);
    AssertEqual((ushort)0, dashAnimation.AnimationFrame, "Dash command interception restarts frame zero");
    AssertEqual((ushort)2, dashAnimation.AnimationFrameTimer, "Dash restart uses shared frame-zero delay");

    // `$91:B61F` supplies each stage's command-loop countdown, while `$91:B5DE` supplies
    // the corresponding animation stream. Distinct synthetic values prove both lookups
    // remain ROM-backed and that stage four publishes the echo/contact-damage events.
    for (int stage = 0; stage <= 4; stage++)
    {
        WriteTestWord(bus, 0x91b61f + stage * 2, (ushort)(stage == 0 ? 3 : 2));
        WriteTestWord(bus, 0x91b5de + stage * 2, (ushort)(0xc200 + stage * 0x10));
        bus.WriteBytes(0x91c200 + stage * 0x10, [(byte)(3 + stage), 0xff]);
    }

    var booster = new SamusHorizontalSpeedState();
    booster.HandleExtraRunSpeed(
        movementType: 1,
        controllerInput: (ushort)SnesButton.B,
        speedBoosterEquipped: true,
        bus);
    AssertTrue(booster.HasRunningMomentum, "Speed Booster establishes momentum");
    AssertEqual((ushort)3, booster.SpeedBoostCounter, "Speed Booster seeds stage-zero countdown from ROM");
    AssertEqual((ushort)1, booster.SpecialPaletteTimer, "Speed Booster seeds special-palette timer");

    // Hexadecimal `.1000` is one sixteenth, so 112 movement calls reach 7.0000 exactly.
    for (int frame = 1; frame < 112; frame++)
    {
        booster.HandleExtraRunSpeed(
            movementType: 1,
            controllerInput: (ushort)SnesButton.B,
            speedBoosterEquipped: true,
            bus);
    }
    AssertEqual((ushort)7, booster.ExtraRunSpeed, "Speed Booster reaches exact 7.0000 cap");
    AssertEqual((ushort)0, booster.ExtraRunSubspeed, "Speed Booster cap has zero fraction");

    ushort boostFrame = 1;
    for (int command = 0; command < 9; command++)
    {
        bool intercepted = booster.TryAdvanceSpeedBoosterAnimationStage(
            bus,
            movementType: 1,
            controllerInput: (ushort)SnesButton.B,
            animationFrameBuffer: 0,
            ref boostFrame,
            out ushort boostTimer);
        if (command == 2)
        {
            AssertTrue(intercepted, "third stage-zero command advances Speed Booster");
            AssertEqual((ushort)0, boostFrame, "Speed Booster stage change restarts animation");
            AssertEqual((ushort)4, boostTimer, "stage-one delay comes from ROM-selected stream");
        }
    }
    AssertEqual((ushort)0x0402, booster.SpeedBoostCounter, "Speed Booster reaches stage four countdown");
    AssertTrue(booster.EchoSoundRequested, "stage four publishes speed-echo sound event");
    AssertEqual((ushort)1, booster.ContactDamageIndex, "stage four enables contact damage");

    // `$91:DAA9` is a pointer to the active suit's four-entry palette-pointer list, not a
    // direct bank-$9B palette address. Distinct first/second colors prove both levels of
    // indirection, the one-then-four frame timer, and the pinned frame-six progression.
    WriteTestWord(bus, 0x91daa9, 0xd100); // Power Suit speed-palette list in bank $91.
    WriteTestWord(bus, 0x91d100, 0xe000);
    WriteTestWord(bus, 0x91d102, 0xe020);
    WriteTestWord(bus, 0x91d104, 0xe040);
    WriteTestWord(bus, 0x91d106, 0xe060);
    WriteTestWord(bus, 0x9be000, 0x1234);
    WriteTestWord(bus, 0x9be020, 0x4567);
    WriteTestWord(bus, 0x91d727, 0x9400); // Normal Power Suit palette for cancellation.
    WriteTestWord(bus, 0x9b9400, 0x0321);

    var boostCgram = new SnesCgram();
    AssertTrue(booster.UpdateSpeedBoosterPalette(
        bus, boostCgram, movementType: 1, animationFrame: 0, equippedItems: 0x2000),
        "stage-four palette timer one copies immediately");
    AssertEqual((ushort)0x1234, boostCgram.Colors[192], "first Speed Booster palette comes from bank $9B");
    AssertEqual((ushort)2, booster.SpecialPaletteFrame, "Speed Booster palette advances to pointer offset two");
    AssertEqual((ushort)4, booster.SpecialPaletteTimer, "Speed Booster palette reloads four-frame timer");
    for (int paletteTick = 0; paletteTick < 3; paletteTick++)
    {
        AssertTrue(!booster.UpdateSpeedBoosterPalette(
            bus, boostCgram, movementType: 1, animationFrame: 0, equippedItems: 0x2000),
            "Speed Booster palette waits during positive timer");
    }
    AssertTrue(booster.UpdateSpeedBoosterPalette(
        bus, boostCgram, movementType: 1, animationFrame: 0, equippedItems: 0x2000),
        "fourth Speed Booster palette tick copies next frame");
    AssertEqual((ushort)0x4567, boostCgram.Colors[192], "second Speed Booster palette pointer");

    // `$90:EEE7` samples post-movement positions only on game-time multiples of four and
    // alternates native word offsets zero/two. These become the two trailing bodies drawn
    // by `$90:87BD`; capture itself deliberately contains no interpolation.
    AssertTrue(!booster.CaptureSpeedEchoPosition(3, 100, 80), "speed echo skips non-fourth frame");
    AssertTrue(booster.CaptureSpeedEchoPosition(4, 101, 81), "speed echo captures slot zero");
    AssertEqual((ushort)2, booster.SpeedEchoIndex, "speed echo advances to slot one");
    AssertTrue(booster.CaptureSpeedEchoPosition(8, 105, 82), "speed echo captures slot one");
    AssertEqual((ushort)0, booster.SpeedEchoIndex, "speed echo alternation wraps after slot one");
    AssertEqual((ushort)101, booster.FirstSpeedEchoXPosition, "first speed echo X snapshot");
    AssertEqual((ushort)105, booster.SecondSpeedEchoXPosition, "second speed echo X snapshot");

    booster.CancelRunningMomentum(poseXDirection: 8);
    AssertTrue(booster.NormalSuitPaletteRestoreRequested,
        "CancelSpeedBoost publishes normal-suit palette restoration");
    AssertEqual((ushort)0xffff, booster.SpeedEchoIndex,
        "right-facing cancellation enters high-bit echo departure");
    AssertEqual((ushort)8, booster.FirstSpeedEchoXSpeed,
        "right-facing first departure speed is positive eight");
    AssertEqual((ushort)8, booster.SecondSpeedEchoXSpeed,
        "right-facing second departure speed is positive eight");

    // `$90:87D3-$90:884B` advances slot one, then slot zero as an actual draw side effect.
    // Both stored bodies trail a rightward-running Samus, so +8 approaches X=120 while Y
    // independently approaches 90 by two. Crossing clears a slot before it can emit OAM.
    AssertTrue(booster.AdvanceDepartingSpeedEcho(1, 120, 90),
        "right departure slot one remains before crossing");
    AssertEqual((ushort)113, booster.SecondSpeedEchoXPosition,
        "right departure slot one advances positive eight");
    AssertEqual((ushort)84, booster.SecondSpeedEchoYPosition,
        "right departure slot one approaches Samus Y by two");
    AssertTrue(booster.AdvanceDepartingSpeedEcho(0, 120, 90),
        "right departure slot zero remains before crossing");
    AssertEqual((ushort)109, booster.FirstSpeedEchoXPosition,
        "right departure slot zero advances positive eight");
    AssertEqual((ushort)83, booster.FirstSpeedEchoYPosition,
        "right departure slot zero approaches Samus Y by two");
    booster.FinishDepartingSpeedEchoFrame();
    AssertEqual((ushort)0xffff, booster.SpeedEchoIndex,
        "departure index survives while either echo remains");

    // Samus_CancelSpeedBoost is called every applicable standing/turn frame. Its BMI guard
    // must preserve an already-running departure even if the new pose faces the other way.
    booster.CancelRunningMomentum(poseXDirection: 4);
    AssertEqual((ushort)8, booster.FirstSpeedEchoXSpeed,
        "repeated cancellation cannot reverse an active departure");
    AssertTrue(!booster.AdvanceDepartingSpeedEcho(1, 120, 90),
        "right departure slot one clears on crossing");
    AssertEqual((ushort)0, booster.SecondSpeedEchoXPosition,
        "crossed slot one publishes empty sentinel");
    AssertTrue(booster.AdvanceDepartingSpeedEcho(0, 120, 90),
        "right departure slot zero remains one more frame");
    booster.FinishDepartingSpeedEchoFrame();
    AssertEqual((ushort)0xffff, booster.SpeedEchoIndex,
        "one surviving departure keeps high-bit index");
    AssertTrue(!booster.AdvanceDepartingSpeedEcho(0, 120, 90),
        "right departure slot zero eventually crosses");
    booster.FinishDepartingSpeedEchoFrame();
    AssertEqual((ushort)0, booster.SpeedEchoIndex,
        "last crossed departure restores ordinary echo index");

    // Mirror the same signed-word comparison for a left-facing cancellation. This uses the
    // shared position-word publisher because those words really are overloaded by active
    // boost, departure, and shinespark-crash state in WRAM.
    var leftDeparture = new SamusHorizontalSpeedState();
    leftDeparture.SetShinesparkCrashEchoState(
        encodedIndex: 0,
        firstX: 150,
        secondX: 154,
        firstY: 90,
        secondY: 94);
    leftDeparture.CancelRunningMomentum(poseXDirection: 4);
    AssertEqual(unchecked((ushort)-8), leftDeparture.FirstSpeedEchoXSpeed,
        "left-facing first departure speed is negative eight");
    AssertEqual(unchecked((ushort)-8), leftDeparture.SecondSpeedEchoXSpeed,
        "left-facing second departure speed is negative eight");
    AssertTrue(leftDeparture.AdvanceDepartingSpeedEcho(1, 140, 100),
        "left departure slot one remains before crossing");
    AssertEqual((ushort)146, leftDeparture.SecondSpeedEchoXPosition,
        "left departure slot one advances negative eight");
    AssertEqual((ushort)96, leftDeparture.SecondSpeedEchoYPosition,
        "left departure slot one approaches Samus Y by two");
    AssertTrue(leftDeparture.AdvanceDepartingSpeedEcho(0, 140, 100),
        "left departure slot zero remains before crossing");
    AssertEqual((ushort)142, leftDeparture.FirstSpeedEchoXPosition,
        "left departure slot zero advances negative eight");
    AssertTrue(!leftDeparture.AdvanceDepartingSpeedEcho(1, 140, 100),
        "left departure slot one clears after passing Samus");
    AssertTrue(!leftDeparture.AdvanceDepartingSpeedEcho(0, 140, 100),
        "left departure slot zero clears after passing Samus");
    leftDeparture.FinishDepartingSpeedEchoFrame();
    AssertEqual((ushort)0, leftDeparture.SpeedEchoIndex,
        "left departure clears shared index after both slots cross");

    AssertTrue(booster.UpdateSpeedBoosterPalette(
        bus, boostCgram, movementType: 0, animationFrame: 0, equippedItems: 0x2000),
        "cancel copies normal suit palette through runtime seam");
    AssertEqual((ushort)0x0321, boostCgram.Colors[192], "cancel restores ROM-authored Power Suit palette");
    AssertTrue(!booster.NormalSuitPaletteRestoreRequested, "normal-suit palette request is one-shot");

    var boostedJump = new SamusState { EquippedItems = 0x2000 };
    boostedJump.Kinematics.YSpeed = 4;
    boostedJump.Kinematics.YSubspeed = 0xe000;
    boostedJump.HorizontalSpeed.ExtraRunSpeed = 3;
    boostedJump.HorizontalSpeed.ExtraRunSubspeed = 0x4000;
    SamusAerialMovement.ApplyEquippedSpeedBoosterJumpBonus(boostedJump);
    AssertEqual((ushort)5, boostedJump.Kinematics.YSpeed, "boosted jump adds half whole extra speed");
    AssertEqual((ushort)0x2000, boostedJump.Kinematics.YSubspeed,
        "boosted jump wraps fractional addition without carrying");

    // $90:E4E6 caps a nonsensically large divisor at four. Extra run speed is added before
    // that shift; 2.0 + 2.0 therefore becomes 0.4000 when divisor $1234 is stored.
    speed.ExtraRunSpeed = 2;
    speed.ExtraRunSubspeed = 0;
    speed.SpeedDivisor = 0x1234;
    AssertEqual(0x00004000u, speed.CalculateTotalSpeed(0x00020000), "total speed divisor caps at four");
    AssertEqual((ushort)0x4000, speed.TotalSubspeed, "total subspeed publication");

    // The displacement clamp replaces only the signed whole word and preserves the low
    // fraction. These deliberately oversized values catch a tempting floating-point clamp.
    speed.ExtraRunSpeed = 0;
    speed.SpeedDivisor = 0;
    AssertEqual(0x000f1234, speed.CalculateRightDisplacement(0x00101234), "right displacement +15 clamp");
    // Negation happens before clamping, so 0 - $0010:1234 is $FFEF:EDCC. The clamp
    // replaces only its high word, producing $FFF1:EDCC rather than mirroring $1234.
    AssertEqual(unchecked((int)0xfff1edcc), speed.CalculateLeftDisplacement(0x00101234), "left displacement -15 clamp");

    Console.WriteLine("  Samus speed: acceleration, deceleration, boost departure echoes, divisor, and clamp agree.");
}

/// <summary>
/// Exercises the four persistent external-displacement words through real movement-family
/// consumers. The fixtures are intentionally all-air so accepted 16.16 movement exposes
/// arithmetic directly instead of conflating it with the separately verified block clipper.
/// </summary>
static void VerifySamusExtraDisplacement()
{
    var bus = new TestAddressSpace();

    // Literal right-standing and normal-jump records. Only direction/movement/radius are
    // needed here, but using the retail bytes ensures production follows its normal metadata
    // path instead of a test-only pose shortcut.
    bus.WriteBytes(0x91b631, [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]); // $01
    bus.WriteBytes(
        0x91b629 + SamusState.MorphBallFallingRightPose * 8,
        [0x08, 0x08, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]); // $31
    bus.WriteBytes(0x91b881, [0x08, 0x02, 0xff, 0x02, 0x03, 0x00, 0x13, 0x00]); // $4B
    bus.WriteBytes(0x91b891, [0x08, 0x02, 0xff, 0x02, 0x08, 0x00, 0x13, 0x00]); // $4D

    // Type-two normal-air X physics is deliberately zero. That isolates extra displacement
    // while still exercising the real pointer selection and no-input base-speed clear.
    for (int offset = 0; offset < 12; offset += 2)
        WriteTestWord(bus, 0x909f6d + offset, 0);

    // Falling Morph Ball uses the type-eight record. Keeping it at exactly zero prevents
    // ordinary rolling physics from obscuring the external-producer bounce override below.
    for (int offset = 0; offset < 12; offset += 2)
        WriteTestWord(bus, 0x909f55 + 8 * 12 + offset, 0);

    const int width = 16;
    const int height = 16;
    var level = new RoomLevelData(
        width,
        height,
        new ushort[width * height],
        new byte[width * height],
        new ushort[width * height],
        new byte[8]);

    var standing = new SamusState
    {
        Pose = SamusState.FacingRightNormalPose,
        XPosition = 64,
        YPosition = 64,
    };
    standing.Kinematics.XRadius = 5;
    standing.Kinematics.YRadius = 21;
    standing.Kinematics.ExtraXDisplacement = 1;
    standing.Kinematics.ExtraXSubdisplacement = 0x8000;
    standing.Kinematics.ExtraYDisplacement = 2;
    standing.Kinematics.ExtraYSubdisplacement = 0x4000;
    GroundedMovementResult positive = SamusGroundedMovement.StepStandingRight(
        bus,
        level,
        standing,
        nmiFrameCounter: 0);
    AssertEqual(0x00018000, positive.Horizontal.AcceptedDisplacement,
        "standing external +X bypasses zero base speed");
    AssertEqual(0x00034000, positive.Vertical.AcceptedDisplacement,
        "positive no-speed external Y gains one whole pixel");
    AssertEqual((ushort)1, standing.Kinematics.ExtraXDisplacement,
        "movement does not consume persistent external X producer word");
    AssertEqual((ushort)2, standing.Kinematics.ExtraYDisplacement,
        "movement does not consume persistent external Y producer word");

    var negative = new SamusState
    {
        Pose = SamusState.FacingRightNormalPose,
        XPosition = 64,
        YPosition = 64,
    };
    negative.Kinematics.XRadius = 5;
    negative.Kinematics.YRadius = 21;
    negative.Kinematics.ExtraYDisplacement = 0xfffe;
    negative.Kinematics.ExtraYSubdisplacement = 0x8000;
    GroundedMovementResult upward = SamusGroundedMovement.StepStandingRight(
        bus,
        level,
        negative,
        nmiFrameCounter: 1);
    AssertEqual(unchecked((int)0xfffe8000), upward.Vertical.AcceptedDisplacement,
        "negative no-speed external Y has no downward bias");

    var transition = new SamusState
    {
        Pose = SamusState.NeutralJumpTransitionRightPose,
        XPosition = 64,
        YPosition = 64,
    };
    transition.Kinematics.XRadius = 5;
    transition.Kinematics.YRadius = 19;
    transition.Kinematics.ExtraXDisplacement = 0xffff;
    transition.Kinematics.ExtraXSubdisplacement = 0x8000; // -0.8000
    transition.Kinematics.ExtraYDisplacement = 0;
    transition.Kinematics.ExtraYSubdisplacement = 0x4000; // +0.4000
    AerialMovementResult transitionResult = SamusAerialMovement.StepNormalJump(
        bus,
        level,
        transition,
        (ushort)SnesButton.A,
        nmiFrameCounter: 0);
    AssertEqual(unchecked((int)0xffff8000), transitionResult.Horizontal.AcceptedDisplacement,
        "jump transition applies signed external X with zero base speed");
    AssertEqual(0x00014000, transitionResult.Vertical!.Value.AcceptedDisplacement,
        "jump transition applies extra-only Y with positive bias");

    var rising = new SamusState
    {
        Pose = SamusState.NeutralJumpRightPose,
        XPosition = 64,
        YPosition = 64,
    };
    rising.Kinematics.XRadius = 5;
    rising.Kinematics.YRadius = 19;
    rising.Kinematics.YDirection = 1;
    rising.Kinematics.YSpeed = 1;
    rising.Kinematics.YSubspeed = 0;
    rising.Kinematics.ExtraYDisplacement = 1;
    rising.Kinematics.ExtraYSubdisplacement = 0x8000;
    AerialMovementResult reversed = SamusAerialMovement.StepNormalJump(
        bus,
        level,
        rising,
        (ushort)SnesButton.A,
        nmiFrameCounter: 0);
    AssertEqual(0x00008000, reversed.Vertical!.Value.AcceptedDisplacement,
        "gravity path adds external Y directly and may reverse actual direction");
    AssertEqual((ushort)1, rising.Kinematics.YDirection,
        "external reversal does not rewrite native velocity-direction word");

    var bouncingBall = new SamusState
    {
        Pose = SamusState.MorphBallFallingRightPose,
        XPosition = 64,
        YPosition = 64,
        MorphBallBounceState = 1,
        KnockbackDirection = 0,
    };
    bouncingBall.Kinematics.XRadius = 5;
    bouncingBall.Kinematics.YRadius = 7;
    bouncingBall.Kinematics.YDirection = 1;
    bouncingBall.Kinematics.YSpeed = 3;
    bouncingBall.Kinematics.YSubspeed = 0x4000;
    bouncingBall.Kinematics.ExtraYDisplacement = 0xffff;
    bouncingBall.Kinematics.ExtraYSubdisplacement = 0x8000; // -0.8000
    MorphBallMovementResult displacedBounce = SamusMorphBallMovement.StepFalling(
        bus,
        level,
        bouncingBall,
        controllerInput: 0,
        nmiFrameCounter: 0);
    AssertEqual(unchecked((int)0xffff8000), displacedBounce.Vertical.AcceptedDisplacement,
        "external Y replaces an active Morph Ball rebound rather than joining gravity");
    AssertEqual((ushort)0, bouncingBall.Kinematics.YSpeed,
        "external Morph Ball bounce override clears whole rebound speed");
    AssertEqual((ushort)0, bouncingBall.Kinematics.YSubspeed,
        "external Morph Ball bounce override clears fractional rebound speed");
    AssertEqual((ushort)2, bouncingBall.Kinematics.YDirection,
        "external Morph Ball bounce override forces native direction word two");
    AssertTrue(!displacedBounce.HitCeiling && !displacedBounce.Landed,
        "unobstructed signed-negative Morph Ball producer moves without a false collision");

    Console.WriteLine(
        "  Samus displacement: signed X/Y, grounded bias, transition motion, persistence, gravity reversal, and Morph Ball override agree.");
}

/// <summary>
/// Exercises `$91:F7B0/$91:DAC7/$90:CFFA-$D2B9` without relying on host elapsed time:
/// stage-gated storage, ROM palette indirection, windup timeout, directional installation,
/// 16.16 acceleration, energy drain, and the low-energy crash handoff.
/// </summary>
static void VerifySamusStoredShineAndShinespark()
{
    var bus = new TestAddressSpace();

    // These eight pose records contain only fields consumed by this isolated state test:
    // direction, movement type `$1B`, and collision radius. Animation still follows the
    // normal live pointer table, proving special movement does not bypass cartridge art.
    foreach (byte pose in new byte[]
    {
        SamusState.ShinesparkWindupRightPose,
        SamusState.ShinesparkWindupLeftPose,
        SamusState.ShinesparkHorizontalRightPose,
        SamusState.ShinesparkHorizontalLeftPose,
        SamusState.ShinesparkVerticalRightPose,
        SamusState.ShinesparkVerticalLeftPose,
        SamusState.ShinesparkDiagonalRightPose,
        SamusState.ShinesparkDiagonalLeftPose,
    })
    {
        bus.WriteBytes(0x91b629 + pose * 8,
            [(byte)((pose & 1) != 0 ? 0x08 : 0x04), 0x1b, 0xff, 0x02, 0x00, 0x00, 0x13, 0x00]);
        ushort stream = unchecked((ushort)(0xc600 + pose));
        WriteTestWord(bus, 0x91b010 + pose * 2, stream);
        bus.WriteByte(0x910000 | stream, 4);
    }
    bus.WriteBytes(0x91b629 + SamusState.FacingRightNormalPose * 8,
        [0x08, 0x00, 0xff, 0x02, 0x00, 0x00, 0x15, 0x00]);
    WriteTestWord(bus, 0x91b010 + SamusState.FacingRightNormalPose * 2, 0xc500);
    bus.WriteByte(0x91c500, 4);

    // Give every positive-half sine/cosine entry magnitude 1.000. This deliberately is
    // not a trigonometric approximation: it makes `$90:CC8A`'s byte-angle sign folding and
    // radius multiplication observable with simple ±radius expectations below.
    for (int angle = 0; angle < 128; angle++)
        WriteTestWord(bus, 0xa0b443 + angle * 2, 0x0100);

    // Stored-shine table `$91:DB10` -> bank-$91 list -> bank-$9B palette. Distinct
    // sentinel colors make either missing indirection immediately observable.
    WriteTestWord(bus, 0x91db10, 0xd200);
    for (int frame = 0; frame < 6; frame++)
    {
        WriteTestWord(bus, 0x91d200 + frame * 2, unchecked((ushort)(0xe100 + frame * 0x20)));
        WriteTestWord(bus, 0x9be100 + frame * 0x20, unchecked((ushort)(0x1100 + frame)));
    }
    WriteTestWord(bus, 0x91d727, 0x9400);
    WriteTestWord(bus, 0x9b9400, 0x0321);

    var shine = new SamusState
    {
        Pose = SamusState.ShinesparkWindupRightPose,
        XPosition = 160,
        YPosition = 160,
        Health = 99,
    };
    AssertTrue(!shine.Shinespark.TryStoreFromSpeedBooster(0x03ff),
        "stage three cannot store shine");
    AssertTrue(shine.Shinespark.TryStoreFromSpeedBooster(0x0400),
        "stage four stores shine");
    AssertEqual((ushort)180, shine.Shinespark.ShineTimer, "stored shine begins at 180");
    AssertEqual((ushort)1, shine.Shinespark.PaletteType, "stored shine installs palette handler one");

    var cgram = new SnesCgram();
    AssertTrue(shine.Shinespark.UpdatePalette(bus, cgram, equippedItems: 0),
        "stored shine copies first ROM palette");
    AssertEqual((ushort)0x1100, cgram.Colors[192], "stored shine follows double pointer");
    AssertEqual((ushort)179, shine.Shinespark.ShineTimer, "stored palette decrements every frame");
    AssertEqual((ushort)2, shine.Shinespark.PaletteFrameOffset,
        "stored palette advances an even pointer offset");

    // Ten further calls arrive with timer 170 on the final call and publish the warning.
    for (int tick = 0; tick < 10; tick++)
        shine.Shinespark.UpdatePalette(bus, cgram, equippedItems: 0);
    AssertTrue(shine.Shinespark.StoredShineWarningSoundRequested,
        "stored timer 170 requests warning sound");
    AssertEqual((ushort)169, shine.Shinespark.ShineTimer,
        "warning frame still decrements timer");

    shine.RefreshCollisionRadii(bus);
    shine.InitializeAnimation(bus);
    shine.Shinespark.BeginWindup(shine);
    AssertEqual(ShinesparkPhase.Windup, shine.Shinespark.Phase,
        "stored shine installs windup handler");
    AssertEqual((ushort)30, shine.Shinespark.StartStopTimer, "windup timer is 30");
    AssertEqual((ushort)8, shine.HorizontalSpeed.ExtraRunSpeed,
        "windup seeds exact 8.0000 extra speed");
    AssertEqual((ushort)0x0400, shine.HorizontalSpeed.SpeedBoostCounter,
        "windup republishes stage four");

    const int width = 24;
    const int height = 24;
    var sparkForeground = new ushort[width * height];
    var sparkBehavior = new byte[width * height];
    // X=160, radius=19, and the first 8.2800 rightward move put the leading edge in
    // block column 10. BTS 7 is the cartridge's 2x2 non-respawning collision bomb block.
    // Pose `$C9` must make `$84:CE83` clear its dispatcher nibble without reporting a hit.
    int sparkBombBlockIndex = 9 * width + 10;
    int sparkExtensionBlockIndex = 8 * width + 10;
    sparkForeground[sparkBombBlockIndex] = 0xf123;
    sparkBehavior[sparkBombBlockIndex] = 7;
    // Type `$D` adds signed BTS rows and asks the dispatcher to run again. Point the first
    // scanned row down one row to the bomb block; the native real-room case uses FF/up.
    sparkForeground[sparkExtensionBlockIndex] = 0xd456;
    sparkBehavior[sparkExtensionBlockIndex] = 1;
    var empty = new RoomLevelData(
        width,
        height,
        sparkForeground,
        sparkBehavior,
        new ushort[width * height],
        new byte[8]);

    // Exercise all six stable direction poses through the public install and movement
    // handlers. The pose definition's literal X-direction byte—not target-pose parity in
    // production—selects horizontal sign, while the phase selects which block axes run.
    RoomLevelData directionLevel = new(
        width,
        height,
        new ushort[width * height],
        new byte[width * height],
        new ushort[width * height],
        new byte[8]);
    foreach ((byte windupPose, byte targetPose, ShinesparkPhase expectedPhase,
        bool expectsHorizontal, bool expectsVertical) in new[]
    {
        (SamusState.ShinesparkWindupRightPose, SamusState.ShinesparkHorizontalRightPose,
            ShinesparkPhase.Horizontal, true, false),
        (SamusState.ShinesparkWindupLeftPose, SamusState.ShinesparkHorizontalLeftPose,
            ShinesparkPhase.Horizontal, true, false),
        (SamusState.ShinesparkWindupRightPose, SamusState.ShinesparkVerticalRightPose,
            ShinesparkPhase.Vertical, false, true),
        (SamusState.ShinesparkWindupLeftPose, SamusState.ShinesparkVerticalLeftPose,
            ShinesparkPhase.Vertical, false, true),
        (SamusState.ShinesparkWindupRightPose, SamusState.ShinesparkDiagonalRightPose,
            ShinesparkPhase.Diagonal, true, true),
        (SamusState.ShinesparkWindupLeftPose, SamusState.ShinesparkDiagonalLeftPose,
            ShinesparkPhase.Diagonal, true, true),
    })
    {
        var directional = new SamusState
        {
            Pose = windupPose,
            XPosition = 160,
            YPosition = 160,
            Health = 99,
        };
        directional.RefreshCollisionRadii(bus);
        directional.InitializeAnimation(bus);
        directional.Shinespark.TryStoreFromSpeedBooster(0x0400);
        directional.Shinespark.BeginWindup(directional);
        directional.Shinespark.BeginDirectionalLaunch(bus, directional, targetPose);
        directional.Kinematics.YAcceleration = 0;
        directional.Kinematics.YSubacceleration = 0x2800;
        ShinesparkMovementResult directionalStep = directional.Shinespark.Step(
            bus, directionLevel, directional, nmiFrameCounter: 0);
        AssertEqual(expectedPhase, directional.Shinespark.Phase,
            $"pose ${targetPose:X2} installs expected shinespark handler");
        AssertEqual(expectsHorizontal, directionalStep.Horizontal is not null,
            $"pose ${targetPose:X2} horizontal-axis dispatch");
        AssertEqual(expectsVertical, directionalStep.Vertical is not null,
            $"pose ${targetPose:X2} vertical-axis dispatch");
        if (expectsHorizontal)
        {
            AssertEqual(targetPose is SamusState.ShinesparkHorizontalLeftPose or
                SamusState.ShinesparkDiagonalLeftPose,
                directional.XPosition < 160,
                $"pose ${targetPose:X2} follows ROM X-direction metadata");
        }
        if (expectsVertical)
        {
            AssertTrue(directional.YPosition < 160,
                $"pose ${targetPose:X2} moves upward through block collision");
        }
    }

    // `$90:D261-$D29F` adds the producer after negating the spark speed. A sufficiently
    // positive external Y value can therefore reverse an upward shinespark. Native then
    // skips its 15-pixel upward-only clamp because the temporary magnitude is negative.
    // Starting at 7.0000 with acceleration 0.2800 and adding +8.0000 yields +0.D800.
    var externallyReversedSpark = new SamusState
    {
        Pose = SamusState.ShinesparkWindupRightPose,
        XPosition = 160,
        YPosition = 160,
        Health = 99,
    };
    externallyReversedSpark.RefreshCollisionRadii(bus);
    externallyReversedSpark.InitializeAnimation(bus);
    externallyReversedSpark.Shinespark.TryStoreFromSpeedBooster(0x0400);
    externallyReversedSpark.Shinespark.BeginWindup(externallyReversedSpark);
    externallyReversedSpark.Shinespark.BeginDirectionalLaunch(
        bus,
        externallyReversedSpark,
        SamusState.ShinesparkVerticalRightPose);
    externallyReversedSpark.Kinematics.YAcceleration = 0;
    externallyReversedSpark.Kinematics.YSubacceleration = 0x2800;
    externallyReversedSpark.Kinematics.ExtraYDisplacement = 8;
    externallyReversedSpark.Kinematics.ExtraYSubdisplacement = 0;
    ShinesparkMovementResult reversedSpark = externallyReversedSpark.Shinespark.Step(
        bus,
        directionLevel,
        externallyReversedSpark,
        nmiFrameCounter: 0);
    AssertEqual(0x0000d800, reversedSpark.Vertical!.Value.AcceptedDisplacement,
        "positive external Y reverses vertical shinespark after native speed negation");
    AssertEqual((ushort)160, externallyReversedSpark.YPosition,
        "subpixel shinespark reversal retains the whole Y coordinate");
    AssertEqual((ushort)0xd800, externallyReversedSpark.Kinematics.YSubposition,
        "subpixel shinespark reversal publishes exact D800 fraction");

    for (ushort frame = 0; frame < 29; frame++)
    {
        ShinesparkMovementResult waiting = shine.Shinespark.Step(bus, empty, shine, frame);
        AssertTrue(!waiting.WindupTimedOut, $"windup frame {frame} remains stationary");
    }
    ShinesparkMovementResult timeout = shine.Shinespark.Step(bus, empty, shine, 29);
    AssertTrue(timeout.WindupTimedOut, "thirtieth windup frame launches vertically");
    AssertEqual(SamusState.ShinesparkVerticalRightPose, shine.Pose,
        "windup timeout selects right-metadata vertical pose");
    AssertEqual(ShinesparkPhase.Vertical, shine.Shinespark.Phase,
        "windup timeout installs vertical handler");

    // Use a fresh state so horizontal arithmetic begins from exactly CFFA's writes.
    var horizontal = new SamusState
    {
        Pose = SamusState.ShinesparkWindupRightPose,
        XPosition = 160,
        YPosition = 160,
        Health = 30,
    };
    horizontal.RefreshCollisionRadii(bus);
    horizontal.InitializeAnimation(bus);
    horizontal.Shinespark.TryStoreFromSpeedBooster(0x0400);
    horizontal.Shinespark.BeginWindup(horizontal);
    horizontal.Shinespark.BeginDirectionalLaunch(
        bus, horizontal, SamusState.ShinesparkHorizontalRightPose);
    horizontal.Kinematics.YAcceleration = 0;
    horizontal.Kinematics.YSubacceleration = 0x2800;
    var shineBombPlms = new RoomPlmSystem();

    ShinesparkMovementResult first = horizontal.Shinespark.Step(
        bus, empty, horizontal, nmiFrameCounter: 4, plms: shineBombPlms);
    AssertTrue(first.Horizontal is { Collided: false } && first.Vertical is null,
        "horizontal spark uses only block X movement");
    AssertEqual(sparkBombBlockIndex, first.Horizontal!.Value.BrokenBombBlock!.Value.Index,
        "horizontal spark publishes the broken BTS-7 block");
    AssertEqual((ushort)0x0123, empty.ForegroundEntries.Span[sparkBombBlockIndex],
        "bomb-block setup clears only the collision nibble");
    AssertEqual(1, shineBombPlms.ActiveCount,
        "shinespark collision installs the bank-$84 BTS-7 PLM in the active room owner");
    AssertEqual((ushort)0xd456, empty.ForegroundEntries.Span[sparkExtensionBlockIndex],
        "extension redispatch mutates its target rather than the extension word");
    AssertEqual((ushort)168, horizontal.XPosition, "first horizontal spark moves 8 whole pixels");
    AssertEqual((ushort)0x2800, horizontal.Kinematics.XSubposition,
        "first horizontal spark preserves 16.16 fraction");
    AssertEqual((ushort)29, horizontal.Health, "active spark drains one energy at threshold 30");
    AssertEqual((ushort)2, horizontal.HorizontalSpeed.ContactDamageIndex,
        "active spark publishes contact damage two");

    ShinesparkMovementResult lowEnergy = horizontal.Shinespark.Step(
        bus, empty, horizontal, nmiFrameCounter: 5);
    AssertTrue(lowEnergy.EndedByLowEnergy && !lowEnergy.EnergyDrained,
        "29-energy frame moves then enters crash without another drain");
    AssertEqual(ShinesparkPhase.Crash, horizontal.Shinespark.Phase,
        "low energy installs crash handler");
    AssertEqual((ushort)0, horizontal.HorizontalSpeed.ExtraRunSpeed,
        "crash clears horizontal spark velocity");

    ushort crashCenterX = horizontal.XPosition;
    ushort crashCenterY = horizontal.YPosition;
    ShinesparkMovementResult firstCrash = horizontal.Shinespark.Step(
        bus, empty, horizontal, nmiFrameCounter: 6);
    AssertEqual((byte)4, horizontal.Shinespark.CrashRadius,
        "crash orbit expands by four");
    AssertEqual(unchecked((ushort)(crashCenterX - 4)),
        horizontal.HorizontalSpeed.FirstSpeedEchoXPosition,
        "right-facing first crash echo folds into negative X half");
    AssertEqual(unchecked((ushort)(crashCenterY - 4)),
        horizontal.HorizontalSpeed.FirstSpeedEchoYPosition,
        "right-facing first crash echo folds into negative Y half");
    AssertEqual(unchecked((ushort)(crashCenterX + 4)),
        horizontal.HorizontalSpeed.SecondSpeedEchoXPosition,
        "opposite crash echo uses positive X half");
    AssertTrue(!firstCrash.CrashSequenceFinished,
        "first crash orbit frame cannot finish sequence");

    // Four expansion calls, 32 separation calls, and four contraction calls comprise the
    // exact `$90:D383-$D3CC` orbit. One was consumed above, so 39 remain.
    for (int frame = 0; frame < 39; frame++)
        horizontal.Shinespark.Step(bus, empty, horizontal, unchecked((ushort)(7 + frame)));
    AssertEqual(ShinesparkPhase.CrashEchoCircle, horizontal.Shinespark.Phase,
        "crash orbit contracts into echo-circle hold");
    AssertEqual((ushort)30, horizontal.Shinespark.StartStopTimer,
        "echo circle begins at 30 frames");

    for (int frame = 0; frame < 30; frame++)
        horizontal.Shinespark.Step(bus, empty, horizontal, unchecked((ushort)(46 + frame)));
    AssertEqual(ShinesparkPhase.CrashFinish, horizontal.Shinespark.Phase,
        "thirtieth echo-circle frame installs finish handler");
    ShinesparkMovementResult finished = horizontal.Shinespark.Step(
        bus, empty, horizontal, nmiFrameCounter: 76);
    AssertTrue(finished.CrashSequenceFinished, "finish handler publishes completion");
    AssertEqual(ShinesparkPhase.Inactive, horizontal.Shinespark.Phase,
        "finish restores ordinary movement handler");
    AssertEqual(SamusState.FacingRightNormalPose, horizontal.Pose,
        "right-facing crash finish returns through standing pose one");

    // `$90:D40D` sampled horizontal-right crash pose `$C9`, so its literal pair is
    // angle $00/$80. Both fixed projectile slots begin centered at radius 64 and do not
    // execute `$90:D4D2` until the following alpha projectile pass.
    AssertEqual(2, horizontal.Shinespark.ReleasedCrashEchoCount,
        "empty projectile capacity admits both departing crash echoes");
    AssertEqual((byte)0x00, horizontal.Shinespark.FirstReleasedCrashEcho.Angle,
        "horizontal-right first departing echo uses ROM table angle zero");
    AssertEqual((byte)0x80, horizontal.Shinespark.SecondReleasedCrashEcho.Angle,
        "horizontal-right second departing echo uses opposite ROM table angle $80");
    AssertEqual((ushort)64, horizontal.Shinespark.FirstReleasedCrashEcho.Radius,
        "departing echo initializes to native radius 64");
    AssertEqual(crashCenterX, horizontal.Shinespark.FirstReleasedCrashEcho.XPosition,
        "departing echo does not move in its spawn frame");

    horizontal.Shinespark.StepReleasedCrashEchoProjectiles(
        bus, horizontal,
        layer1X: unchecked((ushort)(crashCenterX - 128)),
        layer1Y: 0);
    AssertEqual((ushort)72, horizontal.Shinespark.FirstReleasedCrashEcho.Radius,
        "speed-echo pre-instruction expands radius by eight");
    AssertEqual(unchecked((ushort)(crashCenterX + 72)),
        horizontal.Shinespark.FirstReleasedCrashEcho.XPosition,
        "angle-zero departing echo uses positive sine-table X component");
    AssertEqual(unchecked((ushort)(crashCenterY - 72)),
        horizontal.Shinespark.FirstReleasedCrashEcho.YPosition,
        "angle-zero departing echo uses negative cosine-table Y component");
    AssertEqual(unchecked((ushort)(crashCenterX - 72)),
        horizontal.Shinespark.SecondReleasedCrashEcho.XPosition,
        "angle-$80 departing echo uses negative sine-table X component");
    AssertEqual(unchecked((ushort)(crashCenterY + 72)),
        horizontal.Shinespark.SecondReleasedCrashEcho.YPosition,
        "angle-$80 departing echo uses positive cosine-table Y component");

    // Both rays leave the 256-pixel-tall viewport at radius 136. The pre-instruction
    // clears every published word at that instant; it does not keep an off-screen ghost.
    for (int frame = 0; frame < 8; frame++)
    {
        horizontal.Shinespark.StepReleasedCrashEchoProjectiles(
            bus, horizontal,
            layer1X: unchecked((ushort)(crashCenterX - 128)),
            layer1Y: 0);
    }
    AssertEqual(0, horizontal.Shinespark.ReleasedCrashEchoCount,
        "departing crash echoes delete themselves outside the 256-pixel viewport");

    // Repeat only the state-machine spine for native projectile-capacity edges. A count of
    // four suppresses fixed slot three but still allocates slot four; five suppresses both.
    static SamusState FinishAtProjectileCount(
        TestAddressSpace fixtureBus,
        RoomLevelData fixtureLevel,
        ushort projectileCounter)
    {
        var fixture = new SamusState
        {
            Pose = SamusState.ShinesparkWindupRightPose,
            XPosition = 160,
            YPosition = 160,
            Health = 29,
        };
        fixture.RefreshCollisionRadii(fixtureBus);
        fixture.InitializeAnimation(fixtureBus);
        fixture.Shinespark.TryStoreFromSpeedBooster(0x0400);
        fixture.Shinespark.BeginWindup(fixture);
        fixture.Shinespark.BeginDirectionalLaunch(
            fixtureBus, fixture, SamusState.ShinesparkHorizontalRightPose);
        fixture.Kinematics.YAcceleration = 0;
        fixture.Kinematics.YSubacceleration = 0;
        fixture.Shinespark.Step(
            fixtureBus, fixtureLevel, fixture, 0, projectileCounter);
        for (ushort frame = 1; frame <= 71; frame++)
        {
            fixture.Shinespark.Step(
                fixtureBus, fixtureLevel, fixture, frame, projectileCounter);
        }
        return fixture;
    }

    SamusState countFour = FinishAtProjectileCount(bus, directionLevel, 4);
    AssertEqual(1, countFour.Shinespark.ReleasedCrashEchoCount,
        "projectile count four admits only fixed crash-echo slot four");
    AssertTrue(!countFour.Shinespark.FirstReleasedCrashEcho.Active &&
        countFour.Shinespark.SecondReleasedCrashEcho.Active,
        "capacity-four branch preserves native fixed-slot selection");
    SamusState countFive = FinishAtProjectileCount(bus, directionLevel, 5);
    AssertEqual(0, countFive.Shinespark.ReleasedCrashEchoCount,
        "projectile count five suppresses both departing crash echoes");

    Console.WriteLine("  Shinespark: storage, palette, launch, motion, bomb blocks, crash orbit/circle, departing echoes, and standing return agree.");
}

/// <summary>
/// Exercises the complete no-equipment neutral-jump route: ROM pose transition bytecode,
/// jump initialization constants, old-speed displacement ordering, variable-height cut,
/// falling acceleration, solid-floor landing, radius alignment, and landing animation.
/// </summary>
/// <summary>
/// Walks the exact `$90:D5A2-$D792` Crystal Flash handler chain, including its strict
/// controller equality test, ten raise calls, NMI-mod-eight ammo cadence, ROM delay-list
/// finish command, energy overflow, and one-frame-late movement-handler cleanup.
/// </summary>
static void VerifySamusCrystalFlash()
{
    var bus = new TestAddressSpace();

    // Only bytes zero, one, and six matter to this fixture: facing, movement type, and Y
    // radius. The production implementation still reads them through the actual ROM table
    // addresses instead of receiving test-only pose metadata.
    bus.WriteBytes(0x91b629 + SamusState.FacingRightNormalPose * 8,
        [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b629 + SamusState.FacingLeftNormalPose * 8,
        [0x04, 0x00, 0xff, 0x07, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b629 + SamusState.CrystalFlashRightPose * 8,
        [0x08, 0x1b, 0xff, 0xff, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b629 + SamusState.CrystalFlashLeftPose * 8,
        [0x04, 0x1b, 0xff, 0xff, 0x06, 0x00, 0x15, 0x00]);

    WriteTestWord(bus, 0x91b010 + SamusState.FacingRightNormalPose * 2, 0xb600);
    WriteTestWord(bus, 0x91b010 + SamusState.FacingLeftNormalPose * 2, 0xb601);
    WriteTestWord(bus, 0x91b010 + SamusState.CrystalFlashRightPose * 2, 0xb545);
    WriteTestWord(bus, 0x91b010 + SamusState.CrystalFlashLeftPose * 2, 0xb556);
    bus.WriteBytes(0x91b600, [0x05, 0x05]);
    bus.WriteBytes(0x91b545, [
        0x03, 0x03, 0x01, 0x01, 0xfe, 0x02,
        0x0c, 0x0c, 0x0c, 0x0c, 0xfe, 0x04,
        0x03, 0x03, 0x03, 0xfd, 0x01,
    ]);
    bus.WriteBytes(0x91b556, [
        0x03, 0x03, 0x01, 0x01, 0xfe, 0x02,
        0x0c, 0x0c, 0x0c, 0x0c, 0xfe, 0x04,
        0x03, 0x03, 0x03, 0xfd, 0x02,
    ]);

    // Palette handler seven splits sprite palette six into ten body colors and six bubble
    // colors. Distinct synthetic records expose both destination ranges, their independent
    // timers, and `$90:ACC2`'s full beam-palette restoration at finish.
    WriteTestWord(bus, 0x91dc00, 0x9500);
    WriteTestWord(bus, 0x91dc02, 10);
    WriteTestWord(bus, 0x91dc28, 0x9600);
    WriteTestWord(bus, 0x91dc2a, 0x9620);
    WriteTestWord(bus, 0x90c3c9, 0x9700);
    for (ushort color = 0; color < 10; color++)
        WriteTestWord(bus, 0x9b9500 + color * 2, unchecked((ushort)(0x0100 + color)));
    for (ushort color = 0; color < 6; color++)
    {
        WriteTestWord(bus, 0x9b9600 + color * 2, unchecked((ushort)(0x0200 + color)));
        WriteTestWord(bus, 0x9b9620 + color * 2, unchecked((ushort)(0x0220 + color)));
    }
    for (ushort color = 0; color < 16; color++)
        WriteTestWord(bus, 0x909700 + color * 2, unchecked((ushort)(0x0300 + color)));

    // The final Crystal Flash explosion frame selects color index three from the shared
    // bank-$88 Power Bomb table. Components 4/3/2 make its exact four-call fade cadence
    // observable without depending on a second copy of the production state machine.
    bus.WriteBytes(0x888d85 + 3 * 3, [0x04, 0x03, 0x02]);

    const ushort chord = (ushort)(SnesButton.Down | SnesButton.L | SnesButton.R | SnesButton.X);
    var samus = new SamusState
    {
        Pose = SamusState.FacingRightNormalPose,
        XPosition = 0x0120,
        YPosition = 0x0080,
        Health = 1,
        MaxHealth = 99,
        Missiles = 10,
        SuperMissiles = 10,
        PowerBombs = 10,
    };
    samus.RefreshCollisionRadii(bus);

    AssertTrue(!samus.CrystalFlash.TryBegin(bus, samus, chord | (ushort)SnesButton.A),
        "Crystal Flash rejects extra held input");
    samus.Kinematics.YSubspeed = 1;
    AssertTrue(!samus.CrystalFlash.TryBegin(bus, samus, chord),
        "Crystal Flash rejects fractional vertical movement");
    samus.Kinematics.YSubspeed = 0;
    AssertTrue(samus.CrystalFlash.TryBegin(bus, samus, chord),
        "Crystal Flash accepts exact chord and resources");
    AssertEqual(SamusState.CrystalFlashRightPose, samus.Pose,
        "source direction selects right Crystal Flash pose");
    AssertEqual(CrystalFlashPhase.Raising, samus.CrystalFlash.Phase,
        "Crystal Flash installs raise handler");
    AssertEqual((ushort)7, samus.CrystalFlash.SpecialPaletteType,
        "Crystal Flash installs palette handler seven");

    var crystalCgram = new SnesCgram();
    AssertTrue(samus.CrystalFlash.UpdatePalette(bus, crystalCgram, samus),
        "Crystal Flash palette handler owns first visible frame");
    AssertEqual((ushort)0x0100, crystalCgram.Colors[0xe0],
        "Crystal Flash body palette begins at sprite palette-six color zero");
    AssertEqual((ushort)0x0109, crystalCgram.Colors[0xe9],
        "Crystal Flash body palette copies ten colors");
    AssertEqual((ushort)0x0200, crystalCgram.Colors[0xea],
        "Crystal Flash bubble palette begins at color ten");
    AssertEqual((ushort)0x0205, crystalCgram.Colors[0xef],
        "Crystal Flash bubble palette copies six colors");
    AssertEqual((ushort)5, samus.CrystalFlash.SpecialPaletteTimer,
        "Crystal Flash bubble timer reloads five");
    AssertEqual((ushort)10, samus.CrystalFlash.CrystalPaletteTimer,
        "Crystal Flash body timer comes from interleaved ROM record");
    AssertEqual((ushort)4, samus.CrystalFlash.CommonPaletteTimer,
        "Crystal Flash body record advances four bytes");
    AssertEqual((ushort)2, samus.HorizontalSpeed.SpecialPaletteFrame,
        "Crystal Flash publishes aliased $0ACE palette frame");
    AssertEqual((ushort)4, samus.HorizontalSpeed.SpecialPaletteTimer,
        "Crystal Flash publishes aliased $0AD0 record offset");

    // Five more calls expire only the bubble timer and select pointer one. The ten-call
    // body timer remains halfway through its first record.
    for (int paletteCall = 0; paletteCall < 5; paletteCall++)
        samus.CrystalFlash.UpdatePalette(bus, crystalCgram, samus);
    AssertEqual((ushort)0x0220, crystalCgram.Colors[0xea],
        "Crystal Flash bubble palette advances independently");
    AssertEqual((ushort)5, samus.CrystalFlash.CrystalPaletteTimer,
        "Crystal Flash body palette retains independent countdown");

    ushort initialY = samus.YPosition;
    for (int frame = 0; frame < 9; frame++)
    {
        CrystalFlashMovementResult raise = samus.CrystalFlash.Step(bus, samus, (ushort)frame);
        AssertEqual(CrystalFlashPhase.Raising, raise.PhaseAfterStep,
            $"raise frame {frame} retains start handler");
        samus.AnimateNoFx(bus, chord);
    }
    AssertEqual((ushort)(initialY - 18), samus.YPosition, "first nine raise calls move 18 pixels");

    CrystalFlashMovementResult raiseTransition = samus.CrystalFlash.Step(bus, samus, 9);
    AssertEqual(CrystalFlashPhase.DrainingAmmo, raiseTransition.PhaseAfterStep,
        "tenth raise call installs ammo handler");
    AssertEqual((ushort)(initialY - 20), samus.YPosition, "complete raise is 20 pixels");
    AssertEqual(samus.YPosition, samus.CrystalFlash.RaisedYPosition,
        "raised Y capture follows tenth displacement");
    AssertEqual((ushort)6, samus.AnimationFrame, "raise transition forces animation frame six");
    AssertTrue(samus.CrystalFlash.BubbleHdmaRequested,
        "raise transition publishes Crystal Flash HDMA spawn seam");

    var crystalWindow = new SamusPowerBombExplosionState();
    crystalWindow.Arm();
    crystalWindow.BeginCrystalFlash(samus.XPosition, samus.YPosition);
    AssertTrue(!crystalWindow.IsArmed,
        "Crystal Flash spawn clears one-at-a-time Power Bomb flag");
    AssertEqual(PowerBombExplosionPhase.CrystalFlashExplosion, crystalWindow.Phase,
        "Crystal Flash installs bank-$88 stage-one pre-instruction");
    AssertEqual((ushort)0x0400, crystalWindow.ExplosionRadius,
        "Crystal Flash bubble begins at radius four");

    // Seventeen calls remain below `$20.00`; call eighteen adds the old `$0330` speed,
    // reaches `$20B0`, and installs the afterglow. Its rendered table still used `$1D80`.
    for (int hdmaCall = 1; hdmaCall <= 17; hdmaCall++)
        AssertTrue(!crystalWindow.StepFrame(bus), $"Crystal Flash expansion call {hdmaCall}");
    AssertEqual(PowerBombExplosionPhase.CrystalFlashExplosion, crystalWindow.Phase,
        "Crystal Flash expansion remains active through call seventeen");
    AssertTrue(!crystalWindow.StepFrame(bus), "Crystal Flash expansion transition is not cleanup");
    AssertEqual(PowerBombExplosionPhase.CrystalFlashAfterglow, crystalWindow.Phase,
        "Crystal Flash call eighteen installs afterglow");
    AssertEqual((ushort)0x20b0, crystalWindow.ExplosionRadius,
        "Crystal Flash transition radius preserves 8.8 acceleration sum");
    AssertEqual((ushort)0x1d80, crystalWindow.RenderedExplosionRadius,
        "Crystal Flash transition renders pre-update radius");
    AssertEqual((byte)4, crystalWindow.FixedColorRed,
        "Crystal Flash transition selects shared fixed-color entry three");

    // Components 4/3/2 decrement on afterglow calls 1/5/9/13. Calls 14..16 count down
    // timer 3..0; call 17 observes all-zero color and executes `$88:A317` cleanup.
    for (int hdmaCall = 1; hdmaCall <= 16; hdmaCall++)
        AssertTrue(!crystalWindow.StepFrame(bus), $"Crystal Flash afterglow call {hdmaCall}");
    AssertTrue(crystalWindow.StepFrame(bus),
        "Crystal Flash afterglow call seventeen performs cleanup");
    AssertEqual(PowerBombExplosionPhase.Inactive, crystalWindow.Phase,
        "Crystal Flash HDMA cleanup clears phase");
    AssertEqual((ushort)0, crystalWindow.Status,
        "Crystal Flash HDMA cleanup clears shared status");
    samus.AnimateNoFx(bus, chord);
    AssertEqual((ushort)2, samus.AnimationFrameTimer,
        "same beta frame decrements forced timer three to two");

    // Call only accepted NMI counters divisible by eight. These are the only handler calls
    // that can consume ammo or restore energy; skipped counters are checked separately.
    CrystalFlashMovementResult skipped = samus.CrystalFlash.Step(bus, samus, 15);
    AssertTrue(!skipped.ConsumedAmmo && !skipped.RestoredEnergy,
        "non-mod-eight frame leaves Crystal Flash resources untouched");
    for (ushort drain = 1; drain <= 30; drain++)
        samus.CrystalFlash.Step(bus, samus, unchecked((ushort)(drain * 8)));

    AssertEqual((ushort)0, samus.Missiles, "Crystal Flash consumes ten missiles");
    AssertEqual((ushort)0, samus.SuperMissiles, "Crystal Flash consumes ten supers");
    AssertEqual((ushort)0, samus.PowerBombs, "Crystal Flash consumes ten power bombs");
    AssertEqual((ushort)99, samus.Health, "Crystal Flash energy restoration caps at max");
    AssertEqual(CrystalFlashPhase.Finishing, samus.CrystalFlash.Phase,
        "thirtieth drain installs finish handler");
    AssertEqual((ushort)12, samus.AnimationFrame, "ammo completion forces finish frame twelve");

    // Finish animation is ROM bytecode, not a host countdown. Movement runs before animation
    // each frame; `$FD,$01` publishes standing-right, which is committed after animation.
    samus.AnimateNoFx(bus, chord);
    int finishFrames = 0;
    while (samus.PendingTransitionalPose is null && finishFrames++ < 20)
    {
        samus.CrystalFlash.Step(bus, samus, (ushort)(0x0100 + finishFrames));
        samus.AnimateNoFx(bus, chord);
    }
    AssertEqual<byte?>(SamusState.FacingRightNormalPose, samus.PendingTransitionalPose,
        "Crystal Flash ROM finish command publishes standing right");
    AssertTrue(samus.ApplyPendingVerifiedAnimationTransition(bus),
        "Crystal Flash standing transition is applied");
    AssertEqual(CrystalFlashPhase.Finishing, samus.CrystalFlash.Phase,
        "pose transition does not prematurely replace installed handler");
    CrystalFlashMovementResult cleanup = samus.CrystalFlash.Step(bus, samus, 0x0200);
    AssertTrue(cleanup.Completed, "following beta pass restores normal movement handler");
    AssertEqual((ushort)0xffff, samus.CrystalFlash.SpecialPaletteTimer,
        "cleanup requests normal palette restoration");
    AssertTrue(samus.CrystalFlash.UpdatePalette(bus, crystalCgram, samus),
        "Crystal Flash finish restores beam palette");
    AssertEqual((ushort)0x0300, crystalCgram.Colors[0xe0],
        "Crystal Flash finish restores beam palette color zero");
    AssertEqual((ushort)0x030f, crystalCgram.Colors[0xef],
        "Crystal Flash finish restores all sixteen beam palette colors");
    AssertEqual((ushort)0, samus.CrystalFlash.SpecialPaletteType,
        "Crystal Flash palette handler clears after restoration");

    var left = new SamusState
    {
        Pose = SamusState.FacingLeftNormalPose,
        Health = 50,
        Missiles = 10,
        SuperMissiles = 10,
        PowerBombs = 10,
    };
    left.RefreshCollisionRadii(bus);
    AssertTrue(left.CrystalFlash.TryBegin(bus, left, chord),
        "left-facing Crystal Flash begins");
    AssertEqual(SamusState.CrystalFlashLeftPose, left.Pose,
        "source direction selects left Crystal Flash pose");

    Console.WriteLine("  Crystal Flash: prerequisites, handlers, resources, HDMA bubble, palette, and ROM animation agree.");
}

/// <summary>
/// Walks `$91:E16D/$91:EEA6/$91:FCAF`, `$90:E94F`, `$88:86EF-$8AA3`, and
/// `$91:DCB4-$DD30`: admission, all four bodies, dedicated turning, angle frames, setup,
/// widening, aiming, visor palette, teardown, and the native crouched-turn stand-up glitch.
/// </summary>
static void VerifySamusXray()
{
    var bus = new TestAddressSpace();

    // Complete literal pose records from `$91:B631/B751/B851/BCD1-BCF9`. X-ray does not
    // invent a new movement type: standing bodies are zero, crouched bodies five, and its
    // intermediate turn bodies use `$0E`, which makes the installed movement handler RTS.
    (byte Pose, byte[] Definition)[] poses = [
        (SamusState.FacingRightNormalPose, [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]),
        (SamusState.FacingLeftNormalPose, [0x04, 0x00, 0xff, 0x07, 0x06, 0x00, 0x15, 0x00]),
        (SamusState.TurningRightToLeftPose, [0x04, 0x0e, 0xff, 0xfb, 0x06, 0x00, 0x15, 0x00]),
        (SamusState.TurningLeftToRightPose, [0x08, 0x0e, 0xff, 0xfb, 0x06, 0x00, 0x15, 0x00]),
        (SamusState.CrouchingRightPose, [0x08, 0x05, 0x27, 0x02, 0x00, 0x00, 0x10, 0x00]),
        (SamusState.CrouchingLeftPose, [0x04, 0x05, 0x28, 0x07, 0x00, 0x00, 0x10, 0x00]),
        (SamusState.TurningRightToLeftCrouchingPose, [0x04, 0x0e, 0xff, 0xfb, 0x00, 0x00, 0x10, 0x00]),
        (SamusState.TurningLeftToRightCrouchingPose, [0x08, 0x0e, 0xff, 0xfb, 0x00, 0x00, 0x10, 0x00]),
        (SamusState.XrayingStandingRightPose, [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]),
        (SamusState.XrayingStandingLeftPose, [0x04, 0x00, 0xff, 0x07, 0x06, 0x00, 0x15, 0x00]),
        (SamusState.XrayingCrouchingRightPose, [0x08, 0x05, 0xff, 0x02, 0x00, 0x00, 0x10, 0x00]),
        (SamusState.XrayingCrouchingLeftPose, [0x04, 0x05, 0xff, 0x07, 0x00, 0x00, 0x10, 0x00]),
    ];
    foreach ((byte pose, byte[] definition) in poses)
        bus.WriteBytes(0x91b629 + pose * 8, definition);

    // The four X-ray poses share retail `$0F,$0F,$0F,$0F,$0F,$FF`. Synthetic one-tick
    // turn/ordinary lists make the frame-two/timer-one completion seam deterministic while
    // preserving the byte-indexed command shape expected by the production interpreter.
    ushort nextStream = 0xc600;
    foreach ((byte pose, _) in poses)
    {
        ushort stream = nextStream;
        nextStream = unchecked((ushort)(nextStream + 0x10));
        WriteTestWord(bus, 0x91b010 + pose * 2, stream);
        bool xrayPose = pose is
            SamusState.XrayingStandingRightPose or SamusState.XrayingStandingLeftPose or
            SamusState.XrayingCrouchingRightPose or SamusState.XrayingCrouchingLeftPose;
        bus.WriteBytes(
            0x910000 | stream,
            xrayPose ? [0x0f, 0x0f, 0x0f, 0x0f, 0x0f, 0xff] : [0x01, 0x01, 0x01, 0xff]);
    }

    // `$9B:A3C0` contains widening colors 3BE0/5FF0/7FFF and full-beam colors
    // 43FF/2F5A/1AB5. The normal-palette pointer is deliberately synthetic so teardown's
    // complete 16-color restoration cannot pass by leaving the previous visor word behind.
    ushort[] visorColors = [0x3be0, 0x5ff0, 0x7fff, 0x43ff, 0x2f5a, 0x1ab5];
    for (int index = 0; index < visorColors.Length; index++)
        WriteTestWord(bus, 0x9ba3c0 + index * 2, visorColors[index]);
    WriteTestWord(bus, 0x91d727, 0x9400);
    for (ushort index = 0; index < 16; index++)
        WriteTestWord(bus, 0x9b9400 + index * 2, unchecked((ushort)(0x0100 + index)));

    // Full right-facing width ten uses boundary angles `$36/$4A`. Both entries in the
    // literal `$91:C9D4` absolute-tangent table are `$03FE`; seeding only those two words
    // makes the window test fail if production code invents trigonometry or reads a nearby
    // table entry. Ten scanlines from the origin, the 8.8 accumulator lands on X+39.
    WriteTestWord(bus, 0x91c9d4 + 0x36 * 2, 0x03fe);
    WriteTestWord(bus, 0x91c9d4 + 0x4a * 2, 0x03fe);

    var cgram = new SnesCgram();
    var standing = new SamusState
    {
        Pose = SamusState.FacingRightNormalPose,
        XPosition = 100,
        YPosition = 200,
    };
    standing.RefreshCollisionRadii(bus);
    standing.InitializeAnimation(bus);

    AssertTrue(
        standing.Xray.TryBegin(bus, standing, previousMovementType: 0),
        "standing X-ray setup accepted");
    AssertEqual((byte)0xd5, standing.Pose, "right standing X-ray pose");
    AssertEqual((ushort)21, standing.Kinematics.YRadius, "standing X-ray radius");
    AssertEqual((ushort)2, standing.AnimationFrame, "command five starts X-ray frame two");
    AssertEqual((ushort)0x3f, standing.AnimationFrameTimer, "command five X-ray timer");
    AssertEqual((ushort)0x40, standing.Xray.Angle, "right X-ray initial angle");
    AssertEqual((byte)1, standing.Xray.SetupStage, "X-ray starts setup stage one");
    AssertTrue(standing.Xray.TimeIsFrozen, "X-ray freezes time");
    AssertTrue(standing.Xray.ActivationSoundRequested, "X-ray activation sound requested");

    AssertEqual((ushort)2, standing.Xray.StepMovement(bus, standing)!.Value,
        "angle 40 selects forward X-ray frame");
    AssertEqual((ushort)15, standing.AnimationFrameTimer, "X-ray movement forces timer fifteen");
    standing.AnimateNoFx(bus);
    AssertEqual((ushort)14, standing.AnimationFrameTimer,
        "generic animation follows X-ray movement timer write");

    // Palette handler eight runs in the palette-FX phase. Timer one expires immediately,
    // writes only visor color four, advances byte offset zero to two, and reloads five.
    AssertTrue(standing.Xray.UpdatePalette(bus, cgram, standing.EquippedItems),
        "X-ray widening palette writes first visor color");
    AssertEqual((ushort)0x3be0, cgram.Colors[196], "first widening visor color");
    AssertEqual((ushort)2, standing.Xray.SpecialPaletteFrame, "widening palette offset advances");
    AssertEqual((ushort)5, standing.Xray.CommonPaletteTimer, "widening palette timer reload");

    // Eight instruction-list setup functions execute before the main bank-$88 preinstruction.
    // The eighth call clears SetupStage; the next call changes X-ray state zero to one.
    for (int stage = 1; stage <= 8; stage++)
        standing.Xray.StepBeam(bus, standing, (ushort)SnesButton.B);
    AssertEqual((byte)0, standing.Xray.SetupStage, "eight X-ray setup stages complete");
    AssertEqual(XrayBeamPhase.NoBeam, standing.Xray.BeamPhase, "setup retains state zero");
    standing.Xray.StepBeam(bus, standing, (ushort)SnesButton.B);
    AssertEqual(XrayBeamPhase.Widening, standing.Xray.BeamPhase, "state zero starts widening");

    // Fixed-point acceleration reaches 10.F800 after 26 widening calls and would cross
    // eleven on call 27; native clamps the whole width to ten, clears its fraction, and
    // advances to full-beam state two on that exact call.
    for (int frame = 1; frame <= 26; frame++)
        standing.Xray.StepBeam(bus, standing, (ushort)SnesButton.B);
    AssertEqual(XrayBeamPhase.Widening, standing.Xray.BeamPhase,
        "X-ray remains widening through call 26");
    AssertEqual((ushort)10, standing.Xray.AngularWidth, "call 26 whole width");
    AssertEqual((ushort)0xf800, standing.Xray.AngularSubwidth, "call 26 fractional width");
    standing.Xray.StepBeam(bus, standing, (ushort)SnesButton.B);
    AssertEqual(XrayBeamPhase.Full, standing.Xray.BeamPhase, "call 27 reaches full beam");
    AssertEqual((ushort)10, standing.Xray.AngularWidth, "full beam clamps width ten");
    AssertEqual((ushort)0, standing.Xray.AngularSubwidth, "full beam clears width fraction");

    // `$88:88B8/$88DC` puts this standing-right fixture's origin at screen (103,184).
    // `$91:C5FF` advances `$03FE` once per scanline and publishes the high byte, making
    // X=142 the inclusive upper/lower boundary ten lines away. `$88:817B` keeps the cone
    // bright and applies add-seven-then-half color math everywhere outside it.
    var xrayFrame = new Rgba32[SnesGameplayFrameRenderer.Width * SnesGameplayFrameRenderer.Height];
    Array.Fill(xrayFrame, new Rgba32(248, 248, 248, 255));
    SnesGameplayFrameRenderer.ApplyXrayWindowColorMath(
        xrayFrame,
        bus,
        standing.Xray,
        standing,
        layer1X: 0,
        layer1Y: 0);
    AssertEqual((byte)248, xrayFrame[184 * 256 + 255].R,
        "X-ray horizontal center remains inside window");
    AssertEqual((byte)123, xrayFrame[184 * 256].R,
        "X-ray opposite half-plane receives outside half color math");
    AssertEqual((byte)248, xrayFrame[174 * 256 + 142].R,
        "X-ray upper tangent boundary is inclusive after 8.8 truncation");
    AssertEqual((byte)123, xrayFrame[174 * 256 + 141].R,
        "X-ray pixel beyond upper tangent boundary is outside");
    AssertEqual((byte)248, xrayFrame[194 * 256 + 142].R,
        "X-ray lower tangent boundary mirrors upper boundary");
    AssertEqual((byte)248, xrayFrame[0].R,
        "X-ray gameplay window never modifies the IRQ-owned HUD band");

    AssertTrue(standing.Xray.UpdatePalette(bus, cgram, standing.EquippedItems),
        "full beam enters visor cycle");
    AssertEqual((ushort)1, standing.Xray.BeamSizeFlag, "full beam palette flag");
    AssertEqual((ushort)0x43ff, cgram.Colors[196], "first full-beam visor color");
    AssertEqual((ushort)8, standing.Xray.SpecialPaletteFrame, "full-beam palette offset advances");

    // Full-beam aiming moves one angle unit per call and Up wins over Down. Width ten clamps
    // the right-facing center at angle ten, so 80 calls cannot wrap into left-facing space.
    standing.Xray.StepBeam(
        bus,
        standing,
        (ushort)(SnesButton.B | SnesButton.Up | SnesButton.Down));
    AssertEqual((ushort)0x3f, standing.Xray.Angle, "X-ray Up wins over Down");
    for (int frame = 0; frame < 80; frame++)
        standing.Xray.StepBeam(bus, standing, (ushort)(SnesButton.B | SnesButton.Up));
    AssertEqual((ushort)10, standing.Xray.Angle, "right X-ray upper clamp includes width");
    AssertEqual((ushort)0, standing.Xray.StepMovement(bus, standing)!.Value,
        "upper-clamped angle selects looking-up art");

    // Start a turn while the dedicated handler owns input. `$0100-angle` mirrors ten to
    // F6, pose `$25` supplies type `$0E`, and two one-tick animation advances reach the
    // exact frame-two/timer-one completion gate before `$D6` is installed.
    XrayPoseInputResult startedTurn = standing.Xray.HandlePoseInput(
        bus,
        standing,
        (ushort)SnesButton.Left);
    AssertTrue(startedTurn.StartedTurn, "X-ray starts standing turn");
    AssertEqual((byte)0x25, standing.Pose, "X-ray right-to-left standing turn pose");
    AssertEqual((ushort)0xf6, standing.Xray.Angle, "X-ray turn mirrors angle");
    AssertTrue(standing.Xray.StepMovement(bus, standing) is null,
        "X-ray movement is RTS during type-E turn");
    standing.AnimateNoFx(bus);
    standing.AnimateNoFx(bus);
    AssertEqual((ushort)2, standing.AnimationFrame, "X-ray turn reaches frame two");
    AssertEqual((ushort)1, standing.AnimationFrameTimer, "X-ray turn reaches timer one");
    XrayPoseInputResult completedTurn = standing.Xray.HandlePoseInput(bus, standing, 0);
    AssertTrue(completedTurn.CompletedTurn, "X-ray completes standing turn");
    AssertEqual((byte)0xd6, standing.Pose, "X-ray turn installs left standing body");
    AssertEqual((ushort)0, standing.Xray.StepMovement(bus, standing)!.Value,
        "left near-up angle selects looking-up art");

    // Releasing Dash enters states three/four/five. State five restores ordinary left
    // standing, requests palette restoration and sound ten, and unfreezes every subsystem.
    standing.Xray.StepBeam(bus, standing, 0);
    AssertEqual(XrayBeamPhase.RestoreFirstHalf, standing.Xray.BeamPhase,
        "Dash release starts first BG2 restore");
    standing.Xray.StepBeam(bus, standing, 0);
    standing.Xray.StepBeam(bus, standing, 0);
    XrayBeamStepResult finished = standing.Xray.StepBeam(bus, standing, 0);
    AssertTrue(finished.Completed, "X-ray state five completes");
    AssertTrue(!standing.Xray.IsActive && !standing.Xray.TimeIsFrozen,
        "X-ray teardown restores time and handlers");
    AssertEqual((byte)0x02, standing.Pose, "left X-ray exits to ordinary standing");
    AssertEqual((ushort)0xffff, standing.Xray.BeamSizeFlag,
        "X-ray teardown requests palette restoration");
    AssertTrue(standing.Xray.DeactivationSoundRequested, "X-ray deactivation sound requested");
    AssertTrue(standing.Xray.UpdatePalette(bus, cgram, standing.EquippedItems),
        "X-ray teardown restores normal suit palette");
    AssertEqual((ushort)0x0104, cgram.Colors[196], "normal palette replaces visor color");
    AssertEqual((ushort)0, standing.Xray.SpecialPaletteType, "X-ray palette handler clears");

    // Crouched setup selects `$D9`. Releasing while its `$43` turn is still active makes
    // `$91:E2AD` classify movement type `$0E` as standing, choose left `$02`, expand radius
    // 16 -> 21, and move the center five pixels upward: the retail X-ray stand-up glitch.
    var crouched = new SamusState
    {
        Pose = SamusState.CrouchingRightPose,
        XPosition = 100,
        YPosition = 200,
    };
    crouched.RefreshCollisionRadii(bus);
    crouched.InitializeAnimation(bus);
    AssertTrue(crouched.Xray.TryBegin(bus, crouched, previousMovementType: 5),
        "crouched X-ray setup accepted");
    AssertEqual((byte)0xd9, crouched.Pose, "right crouched X-ray pose");
    AssertEqual((ushort)16, crouched.Kinematics.YRadius, "crouched X-ray radius");
    crouched.Xray.HandlePoseInput(bus, crouched, (ushort)SnesButton.Left);
    AssertEqual((byte)0x43, crouched.Pose, "crouched X-ray turn pose");
    AssertEqual((ushort)16, crouched.Kinematics.YRadius, "crouched turn retains radius");
    for (int stage = 1; stage <= 8; stage++)
        crouched.Xray.StepBeam(bus, crouched, 0);
    crouched.Xray.StepBeam(bus, crouched, 0); // State 0 -> state 3 on released Dash.
    crouched.Xray.StepBeam(bus, crouched, 0); // State 3 -> state 4.
    crouched.Xray.StepBeam(bus, crouched, 0); // State 4 -> state 5.
    crouched.Xray.StepBeam(bus, crouched, 0); // State 5 -> teardown.
    AssertEqual((byte)0x02, crouched.Pose, "crouched-turn release triggers standing-left glitch");
    AssertEqual((ushort)21, crouched.Kinematics.YRadius, "stand-up glitch expands radius");
    AssertEqual((ushort)195, crouched.YPosition, "stand-up glitch moves center up five pixels");

    // Admission failures are kept independent so no broad host-side `grounded` boolean can
    // accidentally replace the native previous/current type, landing, velocity, and rare
    // five-bomb conjunction checks.
    SamusState Rejected(byte pose, ushort ySpeed = 0, ushort ySubspeed = 0)
    {
        var sample = new SamusState { Pose = pose };
        sample.RefreshCollisionRadii(bus);
        sample.InitializeAnimation(bus);
        sample.Kinematics.YSpeed = ySpeed;
        sample.Kinematics.YSubspeed = ySubspeed;
        return sample;
    }

    // Seed excluded landing `$A4` and falling `$29` definitions/animations only for gates.
    bus.WriteBytes(0x91b629 + SamusState.NormalLandingRightPose * 8,
        [0x08, 0x00, 0xff, 0x02, 0x03, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b629 + SamusState.FallingRightPose * 8,
        [0x08, 0x06, 0xff, 0x02, 0x08, 0x00, 0x13, 0x00]);
    WriteTestWord(bus, 0x91b010 + SamusState.NormalLandingRightPose * 2, 0xc700);
    WriteTestWord(bus, 0x91b010 + SamusState.FallingRightPose * 2, 0xc710);
    bus.WriteBytes(0x91c700, [0x01, 0xff]);
    bus.WriteBytes(0x91c710, [0x01, 0xff]);
    var landing = Rejected(SamusState.NormalLandingRightPose);
    AssertTrue(!landing.Xray.TryBegin(bus, landing, previousMovementType: 0),
        "X-ray rejects landing pose");
    var movingVertically = Rejected(SamusState.FacingRightNormalPose, ySubspeed: 1);
    AssertTrue(!movingVertically.Xray.TryBegin(bus, movingVertically, previousMovementType: 0),
        "X-ray rejects fractional Y velocity");
    var badPrevious = Rejected(SamusState.FacingRightNormalPose);
    AssertTrue(!badPrevious.Xray.TryBegin(bus, badPrevious, previousMovementType: 6),
        "X-ray rejects unsupported previous movement type");
    var fiveBombQuirk = Rejected(SamusState.FacingRightNormalPose);
    fiveBombQuirk.XSpeedDivisor = 2;
    AssertTrue(!fiveBombQuirk.Xray.TryBegin(
        bus,
        fiveBombQuirk,
        previousMovementType: 0,
        projectileCooldownTimer: 7,
        bombCounter: 5),
        "X-ray preserves five-bomb cooldown/divisor rejection");

    Console.WriteLine(
        "  X-ray: admission, poses, turns, ROM-tangent window, half color math, visor palette, teardown, and stand-up glitch agree.");
}

/// <summary>
/// Walks `$9B:B3A7-$B85F` and `$90:8976-$89FF`: movement-type-selected `$D7/$D8`
/// setup, animation-only preflash, five death-tile transfers, alternating suit/suitless
/// palettes, 60-call flash, nine explosion frames, non-Samus palette whiteout, and terminal
/// no-draw state.
/// </summary>
static void VerifySamusDeathSequence()
{
    var bus = new TestAddressSpace();

    // Source standing/morph/spin records prove three distinct `$9B:B420` decisions. The
    // death records are literal `$91:BCE1/BCE9`: type `$0A`, ordinary humanoid radii, and
    // right/left direction bytes. Their delay stream is exactly 2,2,2,2,2,2,FE,01.
    bus.WriteBytes(0x91b629 + SamusState.FacingRightNormalPose * 8,
        [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b629 + SamusState.MorphBallGroundLeftPose * 8,
        [0x04, 0x04, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    bus.WriteBytes(0x91b629 + SamusState.SpinJumpRightPose * 8,
        [0x08, 0x03, 0xff, 0xff, 0x00, 0x00, 0x0b, 0x00]);
    bus.WriteBytes(0x91b629 + SamusState.DeathSequenceRightPose * 8,
        [0x08, 0x0a, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b629 + SamusState.DeathSequenceLeftPose * 8,
        [0x04, 0x0a, 0xff, 0x07, 0x06, 0x00, 0x15, 0x00]);
    WriteTestWord(bus, 0x91b010 + SamusState.FacingRightNormalPose * 2, 0xc000);
    WriteTestWord(bus, 0x91b010 + SamusState.MorphBallGroundLeftPose * 2, 0xc010);
    WriteTestWord(bus, 0x91b010 + SamusState.SpinJumpRightPose * 2, 0xc020);
    WriteTestWord(bus, 0x91b010 + SamusState.DeathSequenceRightPose * 2, 0xb567);
    WriteTestWord(bus, 0x91b010 + SamusState.DeathSequenceLeftPose * 2, 0xb567);
    bus.WriteBytes(0x91c000, [0x01, 0xff]);
    bus.WriteBytes(0x91c010, [0x01, 0xff]);
    bus.WriteBytes(0x91c020, [0x01, 0xff]);
    bus.WriteBytes(0x91b567, [0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0xfe, 0x01]);

    // Ten suit and ten suitless pointers reproduce `$9B:B7D3/B80F`. Every palette gets a
    // unique first color, making each table index independently visible in assertions.
    for (ushort palette = 0; palette < 10; palette++)
    {
        ushort suitPointer = unchecked((ushort)(0xc000 + palette * 0x20));
        ushort suitlessPointer = unchecked((ushort)(0xd400 + palette * 0x20));
        WriteTestWord(bus, 0x9bb7d3 + palette * 2, suitPointer);
        WriteTestWord(bus, 0x9bb80f + palette * 2, suitlessPointer);
        for (ushort color = 0; color < 16; color++)
        {
            WriteTestWord(bus, 0x9b0000 | (suitPointer + color * 2),
                unchecked((ushort)(0x0100 + palette * 0x20 + color)));
            WriteTestWord(bus, 0x9b0000 | (suitlessPointer + color * 2),
                unchecked((ushort)(0x0400 + palette * 0x20 + color)));
        }
    }

    // Interleaved timer/palette bytes at `$9B:B823`: index zero uses 21/palette zero,
    // indices one through eight use 6/2,3/3,4/4,5/5,5/6,6/7,6/8,80/9.
    bus.WriteBytes(0x9bb823,
        [0x15, 0x00, 0x06, 0x02, 0x03, 0x03, 0x04, 0x04, 0x05, 0x05,
         0x05, 0x06, 0x06, 0x07, 0x06, 0x08, 0x50, 0x09]);
    ushort[] shades =
    [
        0x0421, 0x0c63, 0x14a5, 0x1ce7, 0x2529, 0x2d6b, 0x35ad, 0x4210,
        0x4a52, 0x4e73, 0x5294, 0x56b5, 0x5ad6, 0x5ef7, 0x6318, 0x6739,
        0x6b5a, 0x6f7b, 0x739c, 0x77bd, 0x7bde, 0x7fff,
    ];
    for (int index = 0; index < shades.Length; index++)
        WriteTestWord(bus, 0x9bb835 + index * 2, shades[index]);

    var samus = new SamusState
    {
        Pose = SamusState.FacingRightNormalPose,
        XPosition = 0x0480,
        YPosition = 0x04c0,
    };
    samus.RefreshCollisionRadii(bus);
    samus.InitializeAnimation(bus);
    SamusDeathSequenceStartResult start = samus.DeathSequence.Begin(
        bus,
        samus,
        layer1X: 0x03e0,
        layer1Y: 0x0400);
    AssertEqual((byte)0, start.SourceMovementType, "death source standing type");
    AssertEqual((byte)0xd7, start.DeathPose, "death selects right pose");
    AssertEqual((ushort)5, start.InitialFrame, "ordinary death starts unmorphed frame five");
    AssertEqual((ushort)0x00a0, start.ScreenX, "death captures screen X");
    AssertEqual((ushort)0x00c0, start.ScreenY, "death captures screen Y");
    AssertTrue(!start.SpinJumpSoundRequested, "ordinary death does not request spin SFX");
    AssertEqual((ushort)2, samus.AnimationFrameTimer, "death pose retains two-tick delay");

    var cgram = new SnesCgram();
    var writes = new VramWriteQueue();
    SamusDeathSequenceStepResult step = default;
    for (int call = 1; call <= 16; call++)
        step = samus.DeathSequence.Step(bus, samus, cgram, writes);
    AssertEqual(SamusDeathSequencePhase.Flashing, samus.DeathSequence.Phase,
        "sixteen preflash calls enter flashing");
    AssertEqual((ushort)5, samus.AnimationFrame, "unmorphed death frame loops at five");
    AssertTrue(step.DrawPose, "last preflash call still draws death pose");

    // Calls one through four queue the four high OBJ segments. Call 60 finishes flashing,
    // queues segment four at `$6000`, resets the explosion state, immediately decrements
    // 21 to 20, and draws right-facing spritemap `$81C`.
    for (int call = 1; call <= 60; call++)
        step = samus.DeathSequence.Step(bus, samus, cgram, writes);
    AssertEqual(SamusDeathSequencePhase.SuitExplosion, samus.DeathSequence.Phase,
        "60 flashing calls enter suit explosion");
    AssertEqual(5, writes.Entries.Count, "death queues exactly five tile segments");
    (int Source, ushort Destination)[] expectedSegments =
    [
        (0x9b8400, 0x6200),
        (0x9b8800, 0x6400),
        (0x9b8c00, 0x6600),
        (0x9b9000, 0x6800),
        (0x9b8000, 0x6000),
    ];
    for (int index = 0; index < expectedSegments.Length; index++)
    {
        AssertEqual((ushort)0x0400, writes.Entries[index].SizeInBytes,
            $"death segment {index} size");
        AssertEqual(expectedSegments[index].Source, writes.Entries[index].SourceAddress,
            $"death segment {index} source");
        AssertEqual(expectedSegments[index].Destination,
            writes.Entries[index].EncodedVramDestination,
            $"death segment {index} destination");
    }
    AssertEqual((ushort)0, samus.DeathSequence.AnimationIndex,
        "explosion begins at index zero");
    AssertEqual((ushort)20, samus.DeathSequence.AnimationTimer,
        "same-call first explosion decrement");
    AssertEqual((ushort)0x081c, step.ExplosionSpritemapIndex!.Value,
        "right explosion base spritemap");
    AssertEqual((ushort)0x0100, cgram.Colors[192], "finish restores suit palette zero");
    AssertEqual((ushort)0x0400, cgram.Colors[240], "finish restores suitless palette zero");

    // The remaining literal timers total 135 calls. The terminal call increments index
    // eight to nine, forces white shade 21, and deliberately emits no tenth spritemap.
    int explosionCalls = 0;
    while (samus.DeathSequence.Phase != SamusDeathSequencePhase.Complete)
    {
        step = samus.DeathSequence.Step(bus, samus, cgram, writes);
        explosionCalls++;
        AssertTrue(explosionCalls <= 135, "death explosion terminates on native timer sum");
    }
    AssertEqual(135, explosionCalls, "death explosion remaining call count");
    AssertTrue(step.Completed && !step.DrawExplosion,
        "terminal death call completes without drawing");
    AssertEqual((ushort)9, samus.DeathSequence.AnimationIndex,
        "death terminal index nine");
    AssertEqual((ushort)0x0015, samus.DeathSequence.AnimationCounter,
        "death terminal whiteout shade index");
    AssertEqual((ushort)0x7fff, cgram.Colors[0], "death whiteout reaches full white");
    AssertEqual((ushort)0x7fff, cgram.Colors[239], "death whiteout includes palette six end");
    AssertEqual((ushort)0x0220, cgram.Colors[192],
        "whiteout preserves final Samus suit palette nine");
    AssertEqual((ushort)0x0520, cgram.Colors[240],
        "whiteout preserves final suitless palette nine");

    // Morph Ball begins frame one and uses left pose `$D8`; spin jumping still starts frame
    // five but uniquely requests library-one sound `$32` before pose replacement.
    var morphedLeft = new SamusState
    {
        Pose = SamusState.MorphBallGroundLeftPose,
        XPosition = 64,
        YPosition = 80,
    };
    morphedLeft.RefreshCollisionRadii(bus);
    morphedLeft.InitializeAnimation(bus);
    SamusDeathSequenceStartResult morphStart = morphedLeft.DeathSequence.Begin(
        bus, morphedLeft, layer1X: 0, layer1Y: 0);
    AssertEqual((byte)0xd8, morphStart.DeathPose, "left Morph death selects D8");
    AssertEqual((ushort)1, morphStart.InitialFrame, "Morph death begins unmorph frame one");

    var spinning = new SamusState { Pose = SamusState.SpinJumpRightPose };
    spinning.RefreshCollisionRadii(bus);
    spinning.InitializeAnimation(bus);
    SamusDeathSequenceStartResult spinStart = spinning.DeathSequence.Begin(
        bus, spinning, layer1X: 0, layer1Y: 0);
    AssertTrue(spinStart.SpinJumpSoundRequested, "spin death requests sound $32");
    AssertEqual((ushort)5, spinStart.InitialFrame, "spin death begins unmorphed frame five");

    Console.WriteLine(
        "  Samus death: D7/D8 selection, unmorph art, five VRAM segments, flash palettes, whiteout, and nine explosion frames agree.");
}

/// <summary>
/// Exercises all five `$91:E4AD` drained-controller entries, command `$F7`, the shared
/// old-speed vertical recurrence, collision handoff, and both asymmetric `$FD` releases.
/// </summary>
static void VerifySamusDrainedController()
{
    var bus = new TestAddressSpace();

    // These are the literal direction/type/graphics-offset/radius fields for the source,
    // four drained records, and their eventual ordinary-standing destinations.
    bus.WriteBytes(0x91b629 + SamusState.CrouchingRightPose * 8,
        [0x08, 0x05, 0xff, 0x02, 0x03, 0x00, 0x10, 0x00]);
    bus.WriteBytes(0x91b629 + SamusState.CrouchingLeftPose * 8,
        [0x04, 0x05, 0xff, 0x07, 0x03, 0x00, 0x10, 0x00]);
    bus.WriteBytes(0x91b629 + SamusState.KnockbackLeftPose * 8,
        [0x04, 0x0a, 0xff, 0xff, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b629 + SamusState.DrainedCrouchingRightPose * 8,
        [0x08, 0x1b, 0xff, 0xff, 0xfc, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b629 + SamusState.DrainedCrouchingLeftPose * 8,
        [0x04, 0x1b, 0xff, 0xff, 0xfc, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b629 + SamusState.DrainedStandingRightPose * 8,
        [0x08, 0x1b, 0xff, 0xff, 0xfc, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b629 + SamusState.DrainedStandingLeftPose * 8,
        [0x04, 0x1b, 0xff, 0xff, 0xfc, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b629 + SamusState.FacingRightNormalPose * 8,
        [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b629 + SamusState.FacingLeftNormalPose * 8,
        [0x04, 0x00, 0xff, 0x07, 0x06, 0x00, 0x15, 0x00]);

    WriteTestWord(bus, 0x91b010 + SamusState.CrouchingRightPose * 2, 0xc000);
    WriteTestWord(bus, 0x91b010 + SamusState.CrouchingLeftPose * 2, 0xc001);
    WriteTestWord(bus, 0x91b010 + SamusState.KnockbackLeftPose * 2, 0xc020);
    WriteTestWord(bus, 0x91b010 + SamusState.DrainedCrouchingRightPose * 2, 0xb257);
    WriteTestWord(bus, 0x91b010 + SamusState.DrainedCrouchingLeftPose * 2, 0xb268);
    WriteTestWord(bus, 0x91b010 + SamusState.DrainedStandingRightPose * 2, 0xb288);
    WriteTestWord(bus, 0x91b010 + SamusState.DrainedStandingLeftPose * 2, 0xb290);
    WriteTestWord(bus, 0x91b010 + SamusState.FacingRightNormalPose * 2, 0xc010);
    WriteTestWord(bus, 0x91b010 + SamusState.FacingLeftNormalPose * 2, 0xc011);
    bus.WriteBytes(0x91c000, [0x05, 0x05]);
    bus.WriteBytes(0x91c010, [0x05, 0x05]);
    bus.WriteBytes(0x91c020, [0x01, 0xfe, 0x01]);

    // Copy the byte-oriented streams verbatim. Controller-written frame values are byte
    // indices, so command operands remain part of the index space by design.
    bus.WriteBytes(0x91b257, [
        0x02, 0x02, 0x02, 0x10, 0xf7,
        0x01, 0xfe, 0x01,
        0x10, 0x10, 0x10, 0x10, 0xfe, 0x04,
        0x03, 0xfd, 0x01,
    ]);
    bus.WriteBytes(0x91b268, [
        0x02, 0x02, 0x10, 0xf7,
        0x01, 0xfe, 0x01,
        0x08, 0x10, 0x10, 0x10, 0x10, 0xfe, 0x04,
        0x03, 0x03, 0x03, 0xfd, 0x02,
        0x10, 0x10, 0x10, 0x10, 0xfe, 0x0e,
        0x10, 0xfe, 0x11,
        0x10, 0xfe, 0x01,
    ]);
    bus.WriteBytes(0x91b288, [0x10, 0x10, 0x10, 0x10, 0xff, 0x03, 0xfd, 0x01]);
    bus.WriteBytes(0x91b290, [0x10, 0x10, 0x10, 0x10, 0xff, 0x03, 0xfd, 0x02]);

    // `$91:D99E` points at ten complete Hyper Beam palettes in reverse-numbered order.
    // Distinct synthetic words expose both the pointer index and all-$20-byte copy. The
    // Power Suit entry at `$91:D727` independently verifies controller-zero/command-$17
    // restoration instead of allowing the last rainbow palette to remain accidentally.
    for (ushort palette = 0; palette < 10; palette++)
    {
        ushort pointer = unchecked((ushort)(0xa000 + palette * 0x20));
        WriteTestWord(bus, 0x91d99e + palette * 2, pointer);
        for (ushort color = 0; color < 16; color++)
        {
            WriteTestWord(
                bus,
                0x9b0000 | (pointer + color * 2),
                unchecked((ushort)(0x1000 + palette * 0x20 + color)));
        }
    }
    WriteTestWord(bus, 0x91d727, 0xb800);
    for (ushort color = 0; color < 16; color++)
        WriteTestWord(bus, 0x9bb800 + color * 2, unchecked((ushort)(0x3000 + color)));

    // `$8D:E1F0` points at the compact Hyper Beam projectile-palette program. Each of its
    // ten records lasts two handler calls and writes CGRAM `$E1-$E8`; synthetic colors make
    // the record number, color number, and exact loop boundary independently observable.
    WriteTestWord(bus, 0x8de1f0, 0xc685);
    WriteTestWord(bus, 0x8de1f2, 0xd900);
    WriteTestWord(bus, 0x8dd900, 0xc655);
    WriteTestWord(bus, 0x8dd902, 0x01c2);
    for (ushort frame = 0; frame < HyperBeamPaletteFxState.FrameCount; frame++)
    {
        int record = 0x8dd904 + frame * 20;
        WriteTestWord(bus, record, 2);
        for (ushort color = 0; color < HyperBeamPaletteFxState.ColorsPerFrame; color++)
        {
            WriteTestWord(
                bus,
                record + 2 + color * 2,
                unchecked((ushort)(0x0100 + frame * 0x20 + color)));
        }
        WriteTestWord(bus, record + 18, 0xc595);
    }
    WriteTestWord(bus, 0x8dd9cc, 0xc61e);
    WriteTestWord(bus, 0x8dd9ce, 0xd904);
    var drainedCgram = new SnesCgram();

    const int width = 8;
    const int height = 8;
    var foreground = new ushort[width * height];
    for (int x = 0; x < width; x++)
        foreground[6 * width + x] = 0x8000; // Solid floor begins at whole Y 96.
    var level = new RoomLevelData(
        width,
        height,
        foreground,
        new byte[foreground.Length],
        new ushort[foreground.Length],
        new byte[8]);

    var right = new SamusState
    {
        Pose = SamusState.CrouchingRightPose,
        XPosition = 48,
        YPosition = 75,
    };
    right.RefreshCollisionRadii(bus);
    right.InitializeAnimation(bus);
    right.HorizontalSpeed.BaseSpeed = 3;
    right.HorizontalSpeed.BaseSubspeed = 0x4444;
    right.Kinematics.YSpeed = 2;
    right.Kinematics.YSubspeed = 0x2222;

    right.Drained.LetFall(bus, right);
    AssertEqual(SamusState.DrainedCrouchingRightPose, right.Pose,
        "drained controller zero selects right pose");
    AssertEqual((ushort)70, right.YPosition,
        "drained controller preserves source bottom while radius grows 16 to 21");
    AssertEqual((ushort)2, right.AnimationFrame, "drained fall begins at byte index two");
    AssertEqual((ushort)0, right.Kinematics.YSpeed, "drained fall clears whole Y speed");
    AssertEqual((ushort)2, right.Kinematics.YDirection, "drained fall selects down direction");
    AssertTrue(right.Drained.UpdatePalette(bus, drainedCgram, right.EquippedItems),
        "drained controller zero restores selected suit palette");
    AssertEqual((ushort)0x300f, drainedCgram.Colors[207],
        "drained controller zero copies all sixteen suit colors");

    // Index two lasts two ticks; index three lasts sixteen. Expiration reaches `$F7` at
    // index four, which installs the handler and advances again to index five's delay one.
    for (int tick = 0; tick < 18; tick++)
        right.AnimateNoFx(bus);
    AssertEqual((byte)0xf7, right.LastAnimationDelayCommand!.Value,
        "drained animation reaches F7");
    AssertEqual(DrainedSamusPhase.Falling, right.Drained.Phase,
        "F7 installs drained movement handler");
    AssertEqual((ushort)5, right.AnimationFrame, "F7 performs its second frame increment");
    AssertEqual((ushort)1, right.AnimationFrameTimer, "F7 selects following literal delay");

    right.Kinematics.YAcceleration = 0;
    right.Kinematics.YSubacceleration = 0x4000;
    ushort firstHandlerY = right.YPosition;
    DrainedSamusMovementResult first = right.Drained.StepFalling(bus, level, right, 0);
    AssertEqual(0, first.Vertical.AcceptedDisplacement,
        "first drained handler call uses old zero speed");
    AssertEqual(firstHandlerY, right.YPosition, "first drained handler frame is stationary");
    AssertEqual((ushort)0x4000, right.Kinematics.YSubspeed,
        "first drained handler frame stores gravity for next call");

    DrainedSamusMovementResult landing = default;
    for (ushort frame = 1; frame < 100 && !landing.Landed; frame++)
        landing = right.Drained.StepFalling(bus, level, right, frame);
    AssertTrue(landing.Landed, "drained handler reaches block floor");
    AssertEqual((ushort)75, right.YPosition, "drained body rests at floor minus radius");
    AssertEqual(DrainedSamusPhase.OnFloor, right.Drained.Phase,
        "collision restores normal movement pointer");
    AssertEqual((ushort)7, right.AnimationFrame, "collision jumps to crouched floor art");
    AssertEqual((ushort)8, right.AnimationFrameTimer, "collision loads literal floor-art timer");
    AssertTrue(landing.ImpactYSpeed != 0 || landing.ImpactYSubspeed != 0,
        "drained landing preserves pre-clear impact magnitude");

    right.Drained.PutStanding(bus, right);
    AssertEqual(SamusState.DrainedStandingRightPose, right.Pose,
        "controller one selects standing right");
    AssertEqual((ushort)0, right.AnimationFrame, "standing drained starts index zero");
    AssertEqual((ushort)16, right.AnimationFrameTimer, "standing drained timer is literal sixteen");

    right.Drained.Release(bus, right);
    AssertEqual((ushort)4, right.AnimationFrame, "standing release writes byte index four");
    AssertEqual((ushort)1, right.AnimationFrameTimer, "standing release writes timer one");
    for (int tick = 0; tick < 8 && right.PendingTransitionalPose is null; tick++)
        right.AnimateNoFx(bus);
    AssertEqual<byte?>(SamusState.FacingRightNormalPose, right.PendingTransitionalPose,
        "standing drained release reaches ROM FD operand");
    AssertTrue(right.ApplyPendingVerifiedAnimationTransition(bus),
        "right drained release applies standing transition");
    AssertEqual(DrainedSamusPhase.Inactive, right.Drained.Phase,
        "right drained release clears host handler marker");

    var left = new SamusState
    {
        Pose = SamusState.CrouchingLeftPose,
        XPosition = 48,
        YPosition = 75,
    };
    left.RefreshCollisionRadii(bus);
    left.InitializeAnimation(bus);
    left.Drained.PutCrouchingOrFalling(bus, left);
    AssertEqual(SamusState.DrainedCrouchingLeftPose, left.Pose,
        "controller four selects left crouching/falling pose");
    AssertEqual((ushort)8, left.AnimationFrame, "controller four writes byte index eight");
    AssertEqual((ushort)16, left.AnimationFrameTimer, "controller four writes timer sixteen");
    left.Drained.Release(bus, left);
    AssertEqual((ushort)13, left.AnimationFrame,
        "crouched release preserves literal operand-adjacent byte index thirteen");
    for (int tick = 0; tick < 64 && left.PendingTransitionalPose is null; tick++)
        left.AnimateNoFx(bus);
    AssertEqual<byte?>(SamusState.FacingLeftNormalPose, left.PendingTransitionalPose,
        "left crouched release reaches asymmetric FD operand");
    AssertTrue(left.ApplyPendingVerifiedAnimationTransition(bus),
        "left drained release applies standing transition");

    left.Drained.EnableHyperBeam(left);
    AssertEqual((ushort)0x1009, left.EquippedBeams,
        "controller three installs exact hyper beam equipment word");
    AssertEqual((ushort)0x8000, left.HyperBeam,
        "controller three sets hyper beam flag");
    AssertTrue(left.Drained.HyperBeamPaletteFxRequested,
        "controller three spawns Hyper Beam palette-FX object");
    AssertEqual((ushort)1, left.Drained.HyperBeamPaletteFx.InstructionTimer,
        "palette-FX spawn installs native timer one");
    AssertEqual((ushort)0xd900, left.Drained.HyperBeamPaletteFx.InstructionPointer,
        "palette-FX spawn installs object instruction pointer");

    // Seed both neighboring colors so the test can distinguish the exact eight-color
    // write from a convenient whole-palette copy. Calls 1/2 show frame zero, calls 3/4
    // show frame one, and call 21 executes `$C61E,$D904` and reloads frame zero.
    drainedCgram.SetColor(0xe0, 0x4567);
    drainedCgram.SetColor(0xe9, 0x2345);
    for (int call = 0; call < 21; call++)
    {
        HyperBeamPaletteFxStepResult paletteFx =
            left.Drained.HyperBeamPaletteFx.Step(bus, drainedCgram);
        int expectedFrame = (call / 2) % HyperBeamPaletteFxState.FrameCount;
        AssertEqual(expectedFrame, paletteFx.FrameIndex,
            $"Hyper Beam palette-FX call {call + 1} frame index");
        AssertEqual((call & 1) == 0, paletteFx.PaletteWritten,
            $"Hyper Beam palette-FX call {call + 1} write cadence");
        for (int color = 0; color < HyperBeamPaletteFxState.ColorsPerFrame; color++)
        {
            AssertEqual(
                unchecked((ushort)(0x0100 + expectedFrame * 0x20 + color)),
                drainedCgram.Colors[0xe1 + color],
                $"Hyper Beam palette-FX call {call + 1} color {color}");
        }
    }
    AssertEqual((ushort)1, left.Drained.HyperBeamPaletteFx.CompletedCycles,
        "Hyper Beam palette-FX completes one cycle on call twenty-one");
    AssertEqual((ushort)0x4567, drainedCgram.Colors[0xe0],
        "Hyper Beam palette-FX preserves color before its range");
    AssertEqual((ushort)0x2345, drainedCgram.Colors[0xe9],
        "Hyper Beam palette-FX preserves color after its range");

    // Mother Brain's first rainbow-beam hit calls command five or `$18`. Both routes force
    // pose `$54` and lock normal input; only their installed Up-edge handler differs.
    var able = new SamusState
    {
        Pose = SamusState.FacingRightNormalPose,
        XPosition = 48,
        YPosition = 75,
    };
    able.RefreshCollisionRadii(bus);
    able.InitializeAnimation(bus);
    able.Drained.SetupForRainbowBeamAbleToStand(bus, able);
    AssertEqual(SamusState.KnockbackLeftPose, able.Pose,
        "rainbow command five unconditionally selects left knockback pose $54");
    AssertEqual(DrainedSamusPhase.RainbowBeamLocked, able.Drained.Phase,
        "rainbow command five locks Samus-side movement");
    AssertEqual(DrainedGetUpHandler.AbleToStand, able.Drained.GetUpHandler,
        "rainbow command five installs able timer handler");

    // The handler survives the later controller-four pose change. It accepts Up only from
    // left drained `$E9` at frame >=8, writes 13/1, and replaces itself with RTS.
    able.Drained.PutCrouchingOrFalling(bus, able);
    AssertTrue(!able.Drained.StepGetUpHandler(able, newlyPressedInput: 0),
        "able timer handler ignores absent Up edge");
    AssertTrue(able.Drained.StepGetUpHandler(able, newlyPressedInput: 0x0800),
        "able timer handler accepts Up from E9 frame eight");
    AssertEqual((ushort)13, able.AnimationFrame, "able timer handler selects stand-up frame thirteen");
    AssertEqual((ushort)1, able.AnimationFrameTimer, "able timer handler selects one-tick timer");
    AssertEqual(DrainedGetUpHandler.Inactive, able.Drained.GetUpHandler,
        "able timer handler replaces itself with RTS");

    var unable = new SamusState
    {
        Pose = SamusState.FacingRightNormalPose,
        XPosition = 48,
        YPosition = 75,
    };
    unable.RefreshCollisionRadii(bus);
    unable.InitializeAnimation(bus);
    unable.Drained.SetupForRainbowBeamUnableToStand(bus, unable);
    unable.Drained.PutCrouchingOrFalling(bus, unable);
    AssertEqual(DrainedGetUpHandler.UnableToStand, unable.Drained.GetUpHandler,
        "rainbow command $18 installs failed-stand timer handler");
    AssertTrue(unable.Drained.StepGetUpHandler(unable, newlyPressedInput: 0x0800),
        "failed-stand handler accepts Up only inside frames eight through eleven");
    AssertEqual((ushort)18, unable.AnimationFrame, "failed-stand handler selects frame eighteen");
    AssertEqual(DrainedGetUpHandler.UnableToStand, unable.Drained.GetUpHandler,
        "failed-stand handler remains installed");

    // The two later cutscene commands are direct animation writes. They do not select a
    // new pose or reinitialize a delay stream.
    unable.Drained.FreezeForHyperBeamAcquisition(unable);
    AssertEqual((ushort)28, unable.AnimationFrame, "command $19 freezes drained art at frame $1C");
    AssertEqual((ushort)1, unable.AnimationFrameTimer, "command $19 freeze timer");
    // Command `$16` starts negative-super-special palette zero with one-call cadence. After
    // the Baby raises its delay to two, index two must remain visible for two calls before
    // advancing. Each call loads before decrementing, matching `$91:D96F-$D997` ordering.
    unable.Drained.EnableRainbow(unable);
    AssertTrue(unable.Drained.UpdatePalette(bus, drainedCgram, unable.EquippedItems),
        "rainbow handler owns palette dispatcher");
    AssertEqual((ushort)0x1000, drainedCgram.Colors[192],
        "rainbow first call loads Hyper Beam palette zero");
    AssertEqual((ushort)1, unable.Drained.ChargePaletteIndex,
        "one-call rainbow cadence advances immediately");
    unable.Drained.IncrementRainbowPaletteFrame(maximumFrame: 10);
    unable.Drained.UpdatePalette(bus, drainedCgram, unable.EquippedItems);
    AssertEqual((ushort)2, unable.Drained.CommonPaletteTimer,
        "Baby-raised rainbow delay reloads two");
    AssertEqual((ushort)2, unable.Drained.ChargePaletteIndex,
        "rainbow second palette advances into delayed index");
    unable.Drained.UpdatePalette(bus, drainedCgram, unable.EquippedItems);
    AssertEqual((ushort)0x1040, drainedCgram.Colors[192],
        "rainbow delayed index loads before timer decrement");
    AssertEqual((ushort)2, unable.Drained.ChargePaletteIndex,
        "rainbow delay holds palette index for first call");
    unable.Drained.UpdatePalette(bus, drainedCgram, unable.EquippedItems);
    AssertEqual((ushort)3, unable.Drained.ChargePaletteIndex,
        "rainbow delay advances on second call");

    unable.Drained.DisableRainbowAndStartStandingAnimation(unable);
    AssertEqual((ushort)13, unable.AnimationFrame, "command $17 resumes standing animation at frame thirteen");
    AssertEqual((ushort)1, unable.AnimationFrameTimer, "command $17 resume timer");
    AssertTrue(unable.Drained.UpdatePalette(bus, drainedCgram, unable.EquippedItems),
        "command $17 restores selected suit palette");
    AssertEqual((ushort)0x3000, drainedCgram.Colors[192],
        "command $17 replaces rainbow palette immediately");

    Console.WriteLine("  Drained Samus: rainbow/body and Hyper Beam projectile palettes, controllers, fall, and releases agree.");
}

/// <summary>
/// Exercises the shared bank-$A0 enemy probe independently of room blocks. The fixtures are
/// intentionally synthetic: no translated room currently owns a live enemy actor list, and
/// silently substituting terrain or decorative sprites would not test the native routine.
/// </summary>
static void VerifySamusSolidEnemyCollision()
{
    var samus = new SamusKinematicsState
    {
        XPosition = 100,
        XSubposition = 0,
        YPosition = 100,
        YSubposition = 0x7777,
        XRadius = 5,
        YRadius = 10,
    };

    // The no-enemy path returns A=0 and leaves Samus completely untouched.
    SolidEnemyCollisionResult empty = SamusSolidEnemyCollision.Probe(
        samus, [], SamusCollisionDirection.Right, distance: 3, distanceSubposition: 0x4000);
    AssertTrue(!empty.Collided, "empty interactive-enemy list does not collide");
    AssertEqual((ushort)104, empty.TargetXPosition, "right fractional target rounds outward");
    AssertEqual((ushort)0x7777, samus.YSubposition, "no collision preserves Y subposition");

    // `$A0:A90A-$A0:A9B7` has asymmetric-looking but literal carry/borrow rounding. Test all
    // four jump-table entries, including fractional underflow and overflow, so a later
    // refactor cannot replace this with ordinary truncation or Math.Round.
    SolidEnemyCollisionResult left = SamusSolidEnemyCollision.Probe(
        samus, [], SamusCollisionDirection.Left, distance: 0, distanceSubposition: 0x8000);
    AssertEqual((ushort)98, left.TargetXPosition, "left fractional borrow plus outward decrement");
    AssertEqual((ushort)100, left.TargetYPosition, "left probe preserves target Y");

    samus.XSubposition = 0xf000;
    SolidEnemyCollisionResult rightCarry = SamusSolidEnemyCollision.Probe(
        samus, [], SamusCollisionDirection.Right, distance: 0, distanceSubposition: 0x2000);
    AssertEqual((ushort)102, rightCarry.TargetXPosition, "right fractional carry plus outward increment");

    samus.YSubposition = 0;
    SolidEnemyCollisionResult up = SamusSolidEnemyCollision.Probe(
        samus, [], SamusCollisionDirection.Up, distance: 1, distanceSubposition: 0x8000);
    AssertEqual((ushort)97, up.TargetYPosition, "up target shares negative-direction rounding");

    samus.YSubposition = 0xf000;
    SolidEnemyCollisionResult down = SamusSolidEnemyCollision.Probe(
        samus, [], SamusCollisionDirection.Down, distance: 1, distanceSubposition: 0x2000);
    AssertEqual((ushort)103, down.TargetYPosition, "down target shares positive-direction rounding");

    samus.XSubposition = 0;
    samus.YSubposition = 0x7777;
    var decorative = new SolidEnemyCollisionBody(
        Index: 0x0040, XPosition: 110, YPosition: 100, XRadius: 5, YRadius: 5,
        FreezeTimer: 0, Properties: 0);
    SolidEnemyCollisionResult ignored = SamusSolidEnemyCollision.Probe(
        samus, [decorative], SamusCollisionDirection.Right, distance: 2, distanceSubposition: 0);
    AssertTrue(!ignored.Collided, "non-solid unfrozen enemy is ignored");

    // A frozen enemy is eligible even without property $8000. With a one-pixel current gap,
    // the future box overlaps and the routine clips the requested distance to exactly one.
    SolidEnemyCollisionBody frozen = decorative with
    {
        Index = 0x0080,
        XPosition = 111,
        FreezeTimer = 1,
    };
    SolidEnemyCollisionResult frozenHit = SamusSolidEnemyCollision.Probe(
        samus, [frozen], SamusCollisionDirection.Right, distance: 2, distanceSubposition: 0);
    AssertTrue(frozenHit.Collided, "frozen enemy is solid to Samus");
    AssertEqual((ushort)1, frozenHit.Distance, "right collision publishes current edge gap");
    AssertEqual((ushort)0, frozenHit.DistanceSubposition, "collision clears fractional distance output");
    AssertEqual((ushort?)0x0080, frozenHit.EnemyIndex, "collision publishes native enemy index");
    AssertTrue(!frozenHit.WasTouching, "positive gap is not reported as touching");
    AssertEqual((ushort)0x7777, samus.YSubposition, "positive-gap collision preserves Samus subposition");

    // Property bit 15 takes the other eligibility route. Exact contact reaches `$A0:AAC8`,
    // whose STZ $0AFC bug clears Y subposition even though this is a horizontal probe.
    SolidEnemyCollisionBody solidTouch = decorative with
    {
        Index = 0x00c0,
        Properties = 0x8000,
    };
    SolidEnemyCollisionResult touching = SamusSolidEnemyCollision.Probe(
        samus, [solidTouch], SamusCollisionDirection.Right, distance: 1, distanceSubposition: 0);
    AssertTrue(touching.Collided && touching.WasTouching, "zero-gap solid enemy takes touching path");
    AssertEqual((ushort)0, touching.Distance, "touching collision publishes zero distance");
    AssertEqual((ushort)0, samus.YSubposition, "horizontal enemy touch preserves native Y-subposition bug");

    // The future broad-phase test is strict. Boxes that merely touch at their radii sum do
    // not advance to the directional gap test.
    SolidEnemyCollisionBody tangent = solidTouch with { XPosition = 110 };
    SolidEnemyCollisionResult tangentMiss = SamusSolidEnemyCollision.Probe(
        samus, [tangent], SamusCollisionDirection.Right, distance: 0, distanceSubposition: 0);
    AssertTrue(!tangentMiss.Collided, "future box tangency is not broad-phase overlap");

    // A negative directional gap is a signed/BPL rejection: Samus already overlaps this
    // enemy, so the routine must not trap her inside it or manufacture distance zero.
    SolidEnemyCollisionBody embedded = solidTouch with { XPosition = 109 };
    SolidEnemyCollisionResult embeddedMiss = SamusSolidEnemyCollision.Probe(
        samus, [embedded], SamusCollisionDirection.Right, distance: 2, distanceSubposition: 0);
    AssertTrue(!embeddedMiss.Collided, "enemy already intersecting movement axis is skipped");

    // Broad overlap must succeed on the perpendicular axis as well.
    SolidEnemyCollisionBody verticalMiss = frozen with { YPosition = 116 };
    SolidEnemyCollisionResult perpendicularMiss = SamusSolidEnemyCollision.Probe(
        samus, [verticalMiss], SamusCollisionDirection.Right, distance: 2, distanceSubposition: 0);
    AssertTrue(!perpendicularMiss.Collided, "perpendicular separation rejects directional candidate");

    // Native list order wins. The first entry is farther away than the second but is still
    // returned as soon as both its broad and directional tests pass.
    SolidEnemyCollisionBody fartherFirst = solidTouch with { Index = 0x0100, XPosition = 120 };
    SolidEnemyCollisionBody nearerSecond = solidTouch with { Index = 0x0140, XPosition = 112 };
    SolidEnemyCollisionResult ordered = SamusSolidEnemyCollision.Probe(
        samus, [fartherFirst, nearerSecond], SamusCollisionDirection.Right,
        distance: 20, distanceSubposition: 0);
    AssertEqual((ushort?)0x0100, ordered.EnemyIndex, "first interactive collision wins over nearest geometry");
    AssertEqual((ushort)10, ordered.Distance, "first list entry publishes its own gap");

    // Exercise both vertical directional formulas rather than relying only on target tests.
    SolidEnemyCollisionBody above = solidTouch with
    {
        Index = 0x0180,
        XPosition = 100,
        YPosition = 84,
        XRadius = 5,
        YRadius = 5,
    };
    SolidEnemyCollisionResult ceiling = SamusSolidEnemyCollision.Probe(
        samus, [above], SamusCollisionDirection.Up, distance: 2, distanceSubposition: 0);
    AssertTrue(ceiling.Collided, "upward solid-enemy collision detected");
    AssertEqual((ushort)1, ceiling.Distance, "upward collision publishes top-to-bottom gap");

    SolidEnemyCollisionBody below = above with { Index = 0x01c0, YPosition = 116 };
    SolidEnemyCollisionResult floor = SamusSolidEnemyCollision.Probe(
        samus, [below], SamusCollisionDirection.Down, distance: 2, distanceSubposition: 0);
    AssertTrue(floor.Collided, "downward solid-enemy collision detected");
    AssertEqual((ushort)1, floor.Distance, "downward collision publishes bottom-to-top gap");

    // Ordinary managed movement now preserves the native wrapper order: enemy probe first,
    // room blocks only on a miss, then position addition. An all-air room isolates that seam.
    const int roomWidth = 8;
    const int roomHeight = 8;
    var airRoom = new RoomLevelData(
        roomWidth,
        roomHeight,
        new ushort[roomWidth * roomHeight],
        new byte[roomWidth * roomHeight],
        new ushort[roomWidth * roomHeight],
        new byte[8]);
    var integrated = new SamusKinematicsState
    {
        XPosition = 100,
        YPosition = 100,
        XRadius = 5,
        YRadius = 10,
        InteractiveEnemies = [frozen],
    };
    BlockMoveResult integratedHorizontal = SamusBlockCollision.MoveHorizontal(
        new TestAddressSpace(), airRoom, integrated, displacement: 2 << 16);
    AssertTrue(integratedHorizontal.Collided, "ordinary horizontal mover reports enemy collision");
    AssertEqual((ushort)101, integrated.XPosition, "ordinary horizontal mover clips to enemy gap");
    AssertTrue(integratedHorizontal.CollisionBlock is null, "enemy collision does not invent terrain block");
    AssertEqual(
        (ushort?)0x0080,
        integratedHorizontal.EnemyCollision?.EnemyIndex,
        "ordinary mover retains colliding enemy identity");

    integrated.YPosition = 100;
    integrated.InteractiveEnemies = [below];
    BlockMoveResult integratedVertical = SamusBlockCollision.MoveVertical(
        new TestAddressSpace(),
        airRoom,
        integrated,
        displacement: 2 << 16,
        scanLeftToRight: true);
    AssertTrue(integratedVertical.Collided, "ordinary vertical mover reports enemy collision");
    AssertEqual((ushort)101, integrated.YPosition, "ordinary vertical mover clips to enemy gap");
    AssertEqual((ushort?)0x01c0, integratedVertical.EnemyCollision?.EnemyIndex,
        "vertical mover retains colliding enemy identity");

    // Wall-jump probing copies Samus so it cannot commit movement. The one exception is the
    // original `$A0:AAC8` touching bug, which still clears the real Y subposition.
    integrated.XPosition = 100;
    integrated.YPosition = 100;
    integrated.YSubposition = 0x4321;
    integrated.InteractiveEnemies = [solidTouch];
    BlockMoveResult wallProbe = SamusBlockCollision.ProbeWallHorizontal(
        new TestAddressSpace(), airRoom, integrated, signedDistance: 1 << 16);
    AssertTrue(wallProbe.EnemyCollision is { WasTouching: true },
        "wall probe detects touching enemy before blocks");
    AssertEqual((ushort)100, integrated.XPosition, "wall probe does not commit X motion");
    AssertEqual((ushort)0, integrated.YSubposition, "wall probe commits only native touching side effect");

    AssertThrows<ArgumentOutOfRangeException>(
        () => SamusSolidEnemyCollision.Probe(
            samus, [], (SamusCollisionDirection)4, distance: 0, distanceSubposition: 0),
        "invalid solid-enemy direction rejected");

    Console.WriteLine("  Solid enemies: eligibility, native rounding, overlap, gaps, order, and touching quirk agree.");
}

/// <summary>
/// Exercises Mother Brain's separate bank-$A9 position owner. These checks deliberately
/// target byte carry, signed 8.8 easing, hardcoded arena clamps, trig-table scaling, and
/// previous-position publication—the details most likely to be lost in a float rewrite.
/// </summary>
/// <summary>
/// Exercises the complete ten-pose movement-type-`$1A` family, both owner offsets, ROM
/// transition-table priority, no-input fallback, the alternating-D-pad escape counter, and
/// every velocity word cleared by `$90:E2DE` release.
/// </summary>
static void VerifySamusGrabbedByDraygon()
{
    var bus = new TestAddressSpace();

    byte[] leftPoses = [
        SamusState.DraygonGrabbedNeutralLeftPose,
        SamusState.DraygonGrabbedAimUpLeftPose,
        SamusState.DraygonGrabbedFiringLeftPose,
        SamusState.DraygonGrabbedAimDownLeftPose,
        SamusState.DraygonGrabbedMovingLeftPose,
    ];
    byte[] rightPoses = [
        SamusState.DraygonGrabbedNeutralRightPose,
        SamusState.DraygonGrabbedAimUpRightPose,
        SamusState.DraygonGrabbedFiringRightPose,
        SamusState.DraygonGrabbedAimDownRightPose,
        SamusState.DraygonGrabbedMovingRightPose,
    ];

    // These are the literal retail pose-definition records at `$91:BBF9-$BC20` and
    // `$91:BD89-$BDB0`. In particular, the four non-neutral records on each side fall back
    // to `$BA/$EC`, all ten use radius 21, and moving art disables projectile direction.
    byte[][] leftDefinitions = [
        [0x04, 0x1a, 0xff, 0x07, 0x06, 0x00, 0x15, 0x00],
        [0x04, 0x1a, 0xba, 0x08, 0x06, 0x00, 0x15, 0x00],
        [0x04, 0x1a, 0xba, 0x07, 0x06, 0x00, 0x15, 0x00],
        [0x04, 0x1a, 0xba, 0x06, 0x06, 0x00, 0x15, 0x00],
        [0x04, 0x1a, 0xba, 0xff, 0x06, 0x00, 0x15, 0x00],
    ];
    byte[][] rightDefinitions = [
        [0x08, 0x1a, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00],
        [0x08, 0x1a, 0xec, 0x01, 0x06, 0x00, 0x15, 0x00],
        [0x08, 0x1a, 0xec, 0x02, 0x06, 0x00, 0x15, 0x00],
        [0x08, 0x1a, 0xec, 0x03, 0x06, 0x00, 0x15, 0x00],
        [0x08, 0x1a, 0xec, 0xff, 0x06, 0x00, 0x15, 0x00],
    ];
    for (int index = 0; index < leftPoses.Length; index++)
    {
        bus.WriteBytes(0x91b629 + leftPoses[index] * 8, leftDefinitions[index]);
        bus.WriteBytes(0x91b629 + rightPoses[index] * 8, rightDefinitions[index]);

        // `$BA-$BD/$EC-$EF` use stationary `$B2B4`; `$BE/$F0` use six-frame `$B53C`.
        ushort leftDelay = index == 4 ? (ushort)0xb53c : (ushort)0xb2b4;
        ushort rightDelay = index == 4 ? (ushort)0xb53c : (ushort)0xb2b4;
        WriteTestWord(bus, 0x91b010 + leftPoses[index] * 2, leftDelay);
        WriteTestWord(bus, 0x91b010 + rightPoses[index] * 2, rightDelay);

        // All five poses on a side point to the same held-input transition program.
        WriteTestWord(bus, 0x919ee2 + leftPoses[index] * 2, 0xae18);
        WriteTestWord(bus, 0x919ee2 + rightPoses[index] * 2, 0xae56);
    }

    // Ordinary release destinations need real metadata and a harmless initial delay.
    bus.WriteBytes(0x91b629 + SamusState.FacingRightNormalPose * 8,
        [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b629 + SamusState.FacingLeftNormalPose * 8,
        [0x04, 0x00, 0xff, 0x07, 0x06, 0x00, 0x15, 0x00]);
    WriteTestWord(bus, 0x91b010 + SamusState.FacingRightNormalPose * 2, 0xc000);
    WriteTestWord(bus, 0x91b010 + SamusState.FacingLeftNormalPose * 2, 0xc001);
    bus.WriteBytes(0x91b2b4, [0x10, 0xff]);
    bus.WriteBytes(0x91b53c, [0x05, 0x05, 0x05, 0x05, 0x05, 0x05, 0xff]);
    bus.WriteBytes(0x91c000, [0x0a]);
    bus.WriteBytes(0x91c001, [0x0a]);

    static void WriteTransitionProgram(
        TestAddressSpace target,
        int address,
        (ushort Held, ushort Pose)[] records)
    {
        foreach ((ushort held, ushort pose) in records)
        {
            WriteTestWord(target, address, 0x0000);
            WriteTestWord(target, address + 2, held);
            WriteTestWord(target, address + 4, pose);
            address += 6;
        }
        WriteTestWord(target, address, 0xffff);
    }

    WriteTransitionProgram(bus, 0x91ae18, [
        (0x0a40, 0x00bb), (0x0640, 0x00bd), (0x0240, 0x00bc),
        (0x0010, 0x00bb), (0x0020, 0x00bd), (0x0040, 0x00bc),
        (0x0200, 0x00be), (0x0100, 0x00be), (0x0800, 0x00be),
        (0x0400, 0x00be),
    ]);
    WriteTransitionProgram(bus, 0x91ae56, [
        (0x0940, 0x00ed), (0x0540, 0x00ef), (0x0140, 0x00ee),
        (0x0010, 0x00ed), (0x0020, 0x00ef), (0x0040, 0x00ee),
        (0x0200, 0x00f0), (0x0100, 0x00f0), (0x0800, 0x00f0),
        (0x0400, 0x00f0),
    ]);

    var samus = new SamusState { XPosition = 0x0080, YPosition = 0x0100 };
    samus.DraygonGrabbed.Begin(bus, samus, draygonFacingRight: true);
    AssertEqual(SamusState.DraygonGrabbedNeutralRightPose, samus.Pose,
        "right-facing Draygon entry selects $EC");
    AssertEqual((ushort)21, samus.Kinematics.YRadius, "Draygon entry loads radius 21");

    DraygonOwnerPlacement rightPlacement = samus.DraygonGrabbed.ApplyOwnerPosition(
        samus, ownerXPosition: 0x0100, ownerYPosition: 0x0180, draygonFacingRight: true);
    AssertEqual((short)8, rightPlacement.XOffset, "right-facing claw offset is +8");
    AssertEqual((ushort)0x0108, samus.XPosition, "right-facing owner placement X");
    AssertEqual((ushort)0x01a8, samus.YPosition, "owner placement Y is body plus $28");

    samus.SolidVerticalCollisionResult = 5;
    DraygonGrabbedMovementResult movement = samus.DraygonGrabbed.StepMovement(samus);
    AssertEqual((ushort)5, movement.PreviousSolidVerticalCollisionResult,
        "type-$1A observes stale vertical collision word");
    AssertEqual((ushort)0, movement.SolidVerticalCollisionResult,
        "type-$1A performs its sole STZ side effect");
    AssertEqual((ushort)0x0108, movement.XPosition, "type-$1A does not move X");
    AssertEqual((ushort)0x01a8, movement.YPosition, "type-$1A does not move Y");

    // The larger right+up+shoot chord must select the first `$AE56` record (`$ED`), not
    // the later generic shoot record (`$EE`), proving ROM priority rather than host rules.
    SamusPoseTransition rightUpShoot = SamusPoseTransitionTable.Find(
        bus, samus.Pose, canonicalHeldInput: 0x0940, canonicalNewInput: 0)!.Value;
    AssertEqual((ushort)SamusState.DraygonGrabbedAimUpRightPose,
        rightUpShoot.ProspectivePose, "right grabbed transition priority");
    samus.ApplyDraygonGrabbedPoseChange(bus, (byte)rightUpShoot.ProspectivePose);
    AssertEqual(SamusState.DraygonGrabbedAimUpRightPose, samus.Pose,
        "right grabbed aim transition applies");
    AssertEqual((ushort)SamusState.DraygonGrabbedNeutralRightPose,
        samus.ReadNoInputFallbackPose(bus), "right grabbed aim fallback is $EC");
    samus.ApplyDraygonGrabbedPoseChange(bus, samus.ReadNoInputFallbackPose(bus));

    DraygonEscapeResult locked = samus.DraygonGrabbed.StepEscapeHandler(
        bus, samus, newlyPressedInput: 0, grappleLockedInPlace: true);
    AssertTrue(locked.SuppressProspectivePose, "locked grapple suppresses grabbed pose transition");
    AssertEqual((ushort)0, locked.EscapeButtonCounter, "no D-pad edge does not count");

    DraygonEscapeResult firstUp = samus.DraygonGrabbed.StepEscapeHandler(
        bus, samus, newlyPressedInput: 0x0800, grappleLockedInPlace: false);
    DraygonEscapeResult repeatedUp = samus.DraygonGrabbed.StepEscapeHandler(
        bus, samus, newlyPressedInput: 0x0800, grappleLockedInPlace: false);
    AssertTrue(firstUp.CountedInput, "first Up edge increments escape counter");
    AssertTrue(!repeatedUp.CountedInput, "repeated D-pad pattern is rejected");
    AssertEqual((ushort)1, repeatedUp.EscapeButtonCounter,
        "repeated direction leaves escape counter unchanged");

    samus.HorizontalSpeed.BaseSpeed = 3;
    samus.HorizontalSpeed.BaseSubspeed = 0x4000;
    samus.HorizontalSpeed.ExtraRunSpeed = 2;
    samus.HorizontalSpeed.ExtraRunSubspeed = 0x8000;
    samus.HorizontalSpeed.AccelerationMode = 2;
    samus.Kinematics.YSpeed = 4;
    samus.Kinematics.YSubspeed = 0xc000;
    samus.Kinematics.YDirection = 2;
    samus.MorphBallBounceState = 2;

    DraygonEscapeResult release = repeatedUp;
    for (int inputNumber = 1; inputNumber < SamusDraygonGrabbedState.EscapeButtonCounterTarget; inputNumber++)
    {
        ushort direction = (inputNumber & 1) != 0 ? (ushort)0x0400 : (ushort)0x0800;
        release = samus.DraygonGrabbed.StepEscapeHandler(
            bus, samus, direction, grappleLockedInPlace: false);
    }
    AssertTrue(release.Released, "sixtieth alternating D-pad input releases Samus");
    AssertEqual(SamusState.FacingRightNormalPose, samus.Pose,
        "right grabbed family releases to pose $01");
    AssertEqual((ushort)0, samus.HorizontalSpeed.BaseSpeed, "release clears base X speed");
    AssertEqual((ushort)0, samus.HorizontalSpeed.BaseSubspeed, "release clears base X subspeed");
    AssertEqual((ushort)2, samus.HorizontalSpeed.ExtraRunSpeed,
        "release intentionally preserves extra run speed");
    AssertEqual((ushort)0x8000, samus.HorizontalSpeed.ExtraRunSubspeed,
        "release intentionally preserves extra run subspeed");
    AssertEqual((ushort)0, samus.Kinematics.YSpeed, "release clears Y speed");
    AssertEqual((ushort)0, samus.Kinematics.YSubspeed, "release clears Y subspeed");
    AssertEqual((ushort)0, samus.Kinematics.YDirection, "release clears Y direction");
    AssertEqual((ushort)0, samus.MorphBallBounceState, "release clears bounce state");
    AssertEqual((ushort)0, samus.HorizontalSpeed.AccelerationMode,
        "release clears X acceleration mode");
    AssertTrue(samus.DraygonGrabbed.ConsumeOwnerReleaseSignal(),
        "release publishes one owner-consumed bit");
    AssertTrue(!samus.DraygonGrabbed.ConsumeOwnerReleaseSignal(),
        "owner release bit is one-shot");

    // Mirror the entry/owner/release direction without repeating the 60-input route.
    samus.DraygonGrabbed.Begin(bus, samus, draygonFacingRight: false);
    DraygonOwnerPlacement leftPlacement = samus.DraygonGrabbed.ApplyOwnerPosition(
        samus, ownerXPosition: 0x0100, ownerYPosition: 0x0180, draygonFacingRight: false);
    AssertEqual((short)-8, leftPlacement.XOffset, "left-facing claw offset is -8");
    AssertEqual((ushort)0x00f8, samus.XPosition, "left-facing owner placement X");
    samus.DraygonGrabbed.Release(bus, samus);
    AssertEqual(SamusState.FacingLeftNormalPose, samus.Pose,
        "left grabbed family releases to pose $02");

    Console.WriteLine("  Samus/Draygon: ten poses, owner offsets, transitions, escape, and release agree.");
}

static void VerifyMotherBrainRainbowBeamSamusMovement()
{
    var bus = new TestAddressSpace();

    // `$86:C272` uses table index angle+$40. Give angle zero a literal +$0100 entry and
    // angle $80 a literal -$0100 entry so the expected $10.00 components are unambiguous.
    WriteTestWord(bus, 0xa0b443 + 0x40 * 2, 0x0100);
    WriteTestWord(bus, 0xa0b443 + 0xc0 * 2, 0xff00);

    var movement = new MotherBrainRainbowBeamSamusMovement();
    var samus = new SamusState
    {
        XPosition = 100,
        YPosition = 100,
    };

    // Begin-fall seeds -$01.00 X and zero Y. The first call changes those to -$00.FE and
    // +$00.18. Adding fractional FE to an existing FF must carry into the signed whole
    // delta: -1+1 is zero, while the untouched low subposition bytes remain AA/BB.
    samus.Kinematics.XSubposition = 0xffaa;
    samus.Kinematics.YSubposition = 0xf0bb;
    movement.BeginFallingAfterRainbowBeam();
    MotherBrainForcedSamusMovementResult first =
        movement.StepFallingAfterRainbowBeam(samus);
    AssertEqual((ushort)0xff02, movement.CustomXVelocity, "rainbow fall first eased X velocity");
    AssertEqual((ushort)0x0018, movement.CustomYVelocity, "rainbow fall first accelerated Y velocity");
    AssertEqual((ushort)100, samus.XPosition, "rainbow fall signed X plus fractional carry");
    AssertEqual((ushort)0x01aa, samus.Kinematics.XSubposition, "rainbow fall preserves X low sub-byte");
    AssertEqual((ushort)101, samus.YPosition, "rainbow fall Y fractional carry");
    AssertEqual((ushort)0x08bb, samus.Kinematics.YSubposition, "rainbow fall preserves Y low sub-byte");
    AssertEqual(first.After, first.CameraPreviousPosition,
        "forced movement publishes new position as camera previous");
    AssertTrue(!first.NativeCarry && !first.ReachedVerticalBoundary,
        "unclamped first fall returns clear vertical carry");

    // Exactly 127 more +$0002 updates carry the negative X word to zero. Native clamps it
    // there instead of allowing a positive recoil; subsequent calls must remain zero.
    for (int call = 1; call < 128; call++)
        movement.StepFallingAfterRainbowBeam(samus);
    AssertEqual((ushort)0, movement.CustomXVelocity, "rainbow fall X easing clamps at zero");
    movement.StepFallingAfterRainbowBeam(samus);
    AssertEqual((ushort)0, movement.CustomXVelocity, "rainbow fall X easing stays zero");
    AssertEqual((ushort)0x00c0, samus.YPosition, "rainbow fall clamps at arena floor Y $C0");
    AssertEqual((ushort)0, samus.Kinematics.YSubposition, "arena floor clamp clears Y subposition");

    // At/below $7C, `$A9:BBCF` chooses +$00.40. It is not a snap: this deliberately
    // crosses from $7C.D0 to $7D.10 through the eight-bit fractional carry.
    samus.YPosition = 0x007c;
    samus.Kinematics.YSubposition = 0xd055;
    MotherBrainForcedSamusMovementResult middleDown = movement.MoveTowardMiddleOfWall(samus);
    AssertEqual((ushort)0x0040, middleDown.YVelocity, "middle-wall below target velocity");
    AssertEqual((ushort)0x007d, samus.YPosition, "middle-wall downward whole carry");
    AssertEqual((ushort)0x1055, samus.Kinematics.YSubposition, "middle-wall downward fraction");

    // Above $7C, two's-complement $FFC0 moves upward. $7D.10 + (-$00.40) becomes $7C.D0.
    MotherBrainForcedSamusMovementResult middleUp = movement.MoveTowardMiddleOfWall(samus);
    AssertEqual((ushort)0xffc0, middleUp.YVelocity, "middle-wall above target velocity");
    AssertEqual((ushort)0x007c, samus.YPosition, "middle-wall upward whole borrow");
    AssertEqual((ushort)0xd055, samus.Kinematics.YSubposition, "middle-wall upward fraction");

    // Angle zero reads +$0100 at index $40. Speed $1000 * sine $0100 >> 8 therefore
    // produces +$1000 on Y, while the independent horizontal helper also adds $10 pixels.
    samus.XPosition = 100;
    samus.Kinematics.XSubposition = 0x0022;
    samus.YPosition = 100;
    samus.Kinematics.YSubposition = 0x0033;
    movement.RainbowBeamAngle = 0;
    MotherBrainForcedSamusMovementResult beam = movement.MoveTowardWall(bus, samus);
    AssertEqual((ushort)0x1000, beam.YVelocity, "rainbow beam table-derived Y velocity");
    AssertEqual((ushort)116, samus.XPosition, "rainbow beam horizontal $10.00 step");
    AssertEqual((ushort)116, samus.YPosition, "rainbow beam vertical $10.00 step");
    AssertTrue(!beam.NativeCarry, "rainbow beam caller clears vertical-helper carry");

    // Reaching X $EB returns set carry and skips vertical calculation entirely.
    samus.XPosition = 0x00e0;
    samus.Kinematics.XSubposition = 0x7777;
    samus.YPosition = 100;
    MotherBrainForcedSamusMovementResult wall = movement.MoveTowardWall(bus, samus);
    AssertTrue(wall.ReachedWall && wall.NativeCarry, "rainbow beam wall clamp returns carry");
    AssertEqual((ushort)0x00eb, samus.XPosition, "rainbow beam hardcoded wall X $EB");
    AssertEqual((ushort)0, samus.Kinematics.XSubposition, "rainbow wall clamp clears X subposition");
    AssertEqual((ushort)100, samus.YPosition, "rainbow wall clamp skips vertical movement");

    // A negative table component below Y $30 proves the separate ceiling clamp and its
    // subposition clear. MoveTowardWall then clears native carry because X did not clamp.
    samus.XPosition = 100;
    samus.Kinematics.XSubposition = 0;
    samus.YPosition = 0x0030;
    samus.Kinematics.YSubposition = 0x9999;
    movement.RainbowBeamAngle = 0x80;
    MotherBrainForcedSamusMovementResult ceiling = movement.MoveTowardWall(bus, samus);
    AssertTrue(ceiling.ReachedVerticalBoundary && !ceiling.NativeCarry,
        "rainbow ceiling clamp is hidden from caller carry");
    AssertEqual((ushort)0x0030, samus.YPosition, "rainbow hardcoded ceiling Y $30");
    AssertEqual((ushort)0, samus.Kinematics.YSubposition, "rainbow ceiling clears Y subposition");

    Console.WriteLine("  Mother Brain: rainbow-beam forced 8.8 movement and arena clamps agree.");
}

/// <summary>
/// Drives the complete active `$A9:B983-$BB2D` body-function chain. Long loops are valuable
/// here: the attack's 300 drain calls and 129 decision-timer calls expose off-by-one mistakes
/// that isolated helper checks cannot see.
/// </summary>
static void VerifyMotherBrainRainbowBeamAttackSequence()
{
    var bus = new TestAddressSpace();

    // `$B7:CE00` contains two side-by-side corpse frames in the retail ROM. Give every byte
    // in the complete source window a deterministic, nonzero-heavy identity pattern so the
    // shared graphics initializer must select the six exact right-frame slices; sparse zero
    // memory would let a wrong source, length, or destination pass accidentally.
    for (int byteOffset = 0; byteOffset < 0x0c00; byteOffset++)
    {
        bus.WriteByte(
            0xb7ce00 + byteOffset,
            unchecked((byte)(byteOffset * 37 + 0x5a)));
    }

    // The active chain needs only command-five/$18's forced `$54` pose and controller-zero's
    // later `$E9` pose. These bytes are the retail direction/type/radius metadata and minimal
    // byte-indexed animation streams already proven by the dedicated drained-controller test.
    bus.WriteBytes(0x91b629 + SamusState.KnockbackLeftPose * 8,
        [0x04, 0x0a, 0xff, 0xff, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b629 + SamusState.DrainedCrouchingLeftPose * 8,
        [0x04, 0x1b, 0xff, 0xff, 0xfc, 0x00, 0x15, 0x00]);
    WriteTestWord(bus, 0x91b010 + SamusState.KnockbackLeftPose * 2, 0xc020);
    WriteTestWord(bus, 0x91b010 + SamusState.DrainedCrouchingLeftPose * 2, 0xb268);
    bus.WriteBytes(0x91c020, [0x01, 0xfe, 0x01]);
    bus.WriteBytes(0x91b268, [0x02, 0x02, 0x10, 0xf7, 0x01]);

    // Exact `$A9:9818` and `$A9:993A` command/duration shapes. Spritemap operands are
    // deliberately small sentinels because this check targets the enemy interpreter; the
    // private-ROM regression separately reads the retail extended-spritemap pointers.
    ushort[] forwardReallySlow =
    [
        0x9708, 0x000a, 0x1000,
        0x95fc, 0x000a, 0x1001,
        0x960c, 0x000a, 0x1002,
        0x961c, 0x000a, 0x1003,
        0x9622, 0x000a, 0x1004,
        0x9638, 0x000a, 0x1005,
        0x9648, 0x000a, 0x1006,
        0x9658, 0x000a, 0x1007,
        0x9668, 0x9700, 0x000a, 0x1008, 0x812f,
    ];
    ushort[] backwardReallySlow =
    [
        0x9708, 0x000a, 0x1100,
        0x96f0, 0x000a, 0x1101,
        0x96e0, 0x000a, 0x1102,
        0x96d0, 0x000a, 0x1103,
        0x96ba, 0x000a, 0x1104,
        0x96aa, 0x000a, 0x1105,
        0x96a4, 0x000a, 0x1106,
        0x9694, 0x000a, 0x1107,
        0x967e, 0x9700, 0x000a, 0x1108, 0x812f,
    ];
    for (int index = 0; index < forwardReallySlow.Length; index++)
    {
        WriteTestWord(bus, 0xa99818 + index * 2, forwardReallySlow[index]);
        WriteTestWord(bus, 0xa9993a + index * 2, backwardReallySlow[index]);
    }

    // Exact command/duration topology for the three posture programs reached by the
    // finish-off loop. As above, only spritemap operands are synthetic sentinels.
    ushort[] standUpFast =
    [
        0x9718, 0x0008, 0x1200,
        0x95b6, 0x0008, 0x1201,
        0x95c0, 0x0008, 0x1202,
        0x95ca, 0x0008, 0x1203,
        0x9700, 0x812f,
    ];
    ushort[] standUpAfterLeaning =
    [
        0x9718, 0x0008, 0x1210,
        0x95ca, 0x0008, 0x1211,
        0x9700, 0x812f,
    ];
    ushort[] leanDown =
    [
        0x9718, 0x0008, 0x1220,
        0x95de, 0x9728, 0x0008, 0x1221,
        0x812f,
    ];
    for (int index = 0; index < standUpFast.Length; index++)
        WriteTestWord(bus, 0xa999c6 + index * 2, standUpFast[index]);
    for (int index = 0; index < standUpAfterLeaning.Length; index++)
        WriteTestWord(bus, 0xa999e2 + index * 2, standUpAfterLeaning[index]);
    for (int index = 0; index < leanDown.Length; index++)
        WriteTestWord(bus, 0xa999f2 + index * 2, leanDown[index]);

    var samus = new SamusState
    {
        Health = 999,
        Missiles = 80,
        SuperMissiles = 80,
        PowerBombs = 400,
        XPosition = 220,
        YPosition = 124,
    };
    var attack = new MotherBrainRainbowBeamAttackSequence
    {
        BrainXPosition = 64,
        BrainYPosition = 96,
    };
    attack.Body.XPosition = 64;
    attack.Body.YPosition = 100;

    attack.StartActiveBeam(bus, samus);
    AssertEqual(MotherBrainRainbowBeamAttackPhase.MoveSamusTowardWall, attack.Phase,
        "active rainbow start installs wall-motion function");
    AssertEqual(SamusState.KnockbackLeftPose, samus.Pose,
        "active rainbow start runs native drained setup command");
    AssertEqual(DrainedGetUpHandler.AbleToStand, samus.Drained.GetUpHandler,
        "energy 999 selects command five able handler");
    AssertTrue(samus.InputLocked, "rainbow start locks Samus input handlers");
    AssertTrue(attack.HdmaActive, "rainbow start requests active HDMA beam");
    AssertEqual((ushort)0x0200, attack.AngularWidth, "rainbow start width");

    MotherBrainRainbowBeamAttackStepResult wall = attack.Step(
        bus, samus, enemyFrameCounter: 2, mainEnemyExecutionCounter: 1);
    AssertEqual((ushort)0x00eb, samus.XPosition, "actor sequence moves Samus to hardcoded wall");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.OneFrameDelay, attack.Phase,
        "wall carry installs one-frame-delay function");
    AssertTrue(wall.SoundQueued && wall.PaletteRequested,
        "wall function runs sound and bit-one palette cadence");
    AssertEqual((ushort)0x0380, wall.AngularWidth, "wall function widens before aiming");
    AssertTrue(wall.Explosion is { XOffset: 6, YOffset: 2, SoundEffect: 0x24 },
        "zero explosion timer increments to literal offset record one");

    MotherBrainRainbowBeamAttackStepResult delay = attack.Step(
        bus, samus, enemyFrameCounter: 0, mainEnemyExecutionCounter: 2);
    AssertEqual(MotherBrainRainbowBeamAttackPhase.StartDrainingSamus, attack.Phase,
        "zero delay timer underflows and schedules drain initializer");
    AssertEqual((ushort)8, delay.EarthquakeType, "delay underflow selects earthquake type eight");
    AssertEqual((ushort)8, delay.EarthquakeTimer, "delay underflow seeds eight-frame earthquake");

    int drainCalls = 0;
    int queuedBeamSounds = (wall.SoundQueued ? 1 : 0) + (delay.SoundQueued ? 1 : 0);
    int explosions = wall.Explosion is null ? 0 : 1;
    while (attack.Phase is MotherBrainRainbowBeamAttackPhase.StartDrainingSamus or
           MotherBrainRainbowBeamAttackPhase.DrainingSamus)
    {
        MotherBrainRainbowBeamAttackStepResult drain = attack.Step(
            bus,
            samus,
            enemyFrameCounter: unchecked((ushort)drainCalls),
            mainEnemyExecutionCounter: unchecked((ushort)drainCalls));
        drainCalls++;
        if (drain.SoundQueued)
            queuedBeamSounds++;
        if (drain.Explosion is not null)
            explosions++;
    }

    AssertEqual(300, drainCalls, "$012B drain timer includes fallthrough call and expires after 300");
    AssertEqual((ushort)399, samus.Health, "300 no-Varia rainbow hits subtract two each");
    AssertEqual((ushort)5, samus.Missiles, "missiles decrement every fourth enemy pass");
    AssertEqual((ushort)5, samus.SuperMissiles, "supers share every-fourth cadence");
    AssertEqual((ushort)100, samus.PowerBombs, "power bombs decrement every drain call");
    AssertEqual(7, queuedBeamSounds, "count six queues seven rainbow beam sound attempts");
    AssertTrue(explosions > 1, "explosion timer continues across wall and drain phases");
    AssertEqual((ushort)0x0c00, attack.AngularWidth, "drain widening clamps at $0C00");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.FinishFiring, attack.Phase,
        "drain timer underflow installs finish-firing function");

    int narrowingCalls = 0;
    bool observedUnlock = false;
    while (attack.Phase == MotherBrainRainbowBeamAttackPhase.FinishFiring)
    {
        MotherBrainRainbowBeamAttackStepResult narrowing = attack.Step(
            bus, samus, enemyFrameCounter: 0, mainEnemyExecutionCounter: 0);
        narrowingCalls++;
        observedUnlock |= narrowing.UnlockedSamus;
    }
    AssertEqual(7, narrowingCalls, "$0C00 beam narrows below $0200 in seven calls");
    AssertEqual((ushort)0x0200, attack.AngularWidth, "beam shutdown pins angular width floor");
    AssertTrue(observedUnlock && !samus.InputLocked, "beam shutdown runs Samus command one");
    AssertTrue(!attack.HdmaActive, "beam shutdown disables its HDMA channel");
    AssertEqual((ushort)8, attack.SamusProjectileCooldownTimer,
        "beam shutdown reloads Samus projectile cooldown");

    int fallingCalls = 0;
    while (attack.Phase is MotherBrainRainbowBeamAttackPhase.LetSamusFall or
           MotherBrainRainbowBeamAttackPhase.WaitForSamusToLand)
    {
        MotherBrainRainbowBeamAttackStepResult falling = attack.Step(
            bus, samus, enemyFrameCounter: 0, mainEnemyExecutionCounter: 0);
        fallingCalls++;
        AssertTrue(falling.Movement is not null,
            $"custom post-beam fall call {fallingCalls} owns Samus coordinates");
    }
    AssertEqual(MotherBrainRainbowBeamAttackPhase.LowerHead, attack.Phase,
        "custom falling carry installs lower-head function");
    AssertEqual((ushort)0x00c0, samus.YPosition, "custom falling reaches hardcoded floor $C0");
    AssertEqual(SamusState.DrainedCrouchingLeftPose, samus.Pose,
        "let-fall controller selects left drained pose from forced $54 direction");

    attack.Step(bus, samus, enemyFrameCounter: 0, mainEnemyExecutionCounter: 0);
    AssertEqual(MotherBrainRainbowBeamAttackPhase.DecideNextAction, attack.Phase,
        "lower-head function installs decision timer");
    AssertEqual((ushort)0x0080, attack.FunctionTimer, "lower-head function seeds $80 timer");

    int decisionCalls = 0;
    while (attack.Phase == MotherBrainRainbowBeamAttackPhase.DecideNextAction)
    {
        attack.Step(bus, samus, enemyFrameCounter: 0, mainEnemyExecutionCounter: 0);
        decisionCalls++;
    }
    AssertEqual(129, decisionCalls, "$80 decision timer expires on 129th DEC/BPL call");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.FinishSamusOff, attack.Phase,
        "post-drain health below $190 chooses finish-Samus chain");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.BodyWalkingForwardReallySlowInstructionList,
        attack.Body.InstructionPointer,
        "low-health decision immediately installs native forward body walk");
    AssertEqual((ushort)1, attack.Body.InstructionTimer,
        "low-health decision makes forward walk eligible in same enemy frame");
    MotherBrainRainbowBeamAttackStepResult finishThreshold = attack.Step(
        bus, samus, enemyFrameCounter: 0, mainEnemyExecutionCounter: 0);
    AssertEqual(MotherBrainRainbowBeamAttackPhase.FinishSamusOff, finishThreshold.PhaseAfter,
        "399 energy remains above no-suit finish threshold 340");
    AssertEqual<MotherBrainFinishOffAttackKind?>(null, finishThreshold.FinishOffAttack,
        "RNG zero takes finish-off no-attack branch");

    // Boundary 700 uses command five; 699 uses `$18`. The second fixture also proves that
    // `$A9:C4E8` loads literal zero even when a different HUD item made the prior CMP fail.
    var low = new SamusState
    {
        Health = 699,
        Missiles = 1,
        SuperMissiles = 1,
        PowerBombs = 1,
        SelectedHudItem = 2,
        AutoCancelHudItemIndex = 9,
        XPosition = 220,
        YPosition = 124,
    };
    var lowAttack = new MotherBrainRainbowBeamAttackSequence();
    lowAttack.StartActiveBeam(bus, low);
    AssertEqual(DrainedGetUpHandler.UnableToStand, low.Drained.GetUpHandler,
        "energy 699 selects command $18 unable handler");
    lowAttack.Step(bus, low, enemyFrameCounter: 0, mainEnemyExecutionCounter: 1);
    lowAttack.Step(bus, low, enemyFrameCounter: 0, mainEnemyExecutionCounter: 2);
    MotherBrainRainbowBeamAttackStepResult firstDrain = lowAttack.Step(
        bus, low, enemyFrameCounter: 0, mainEnemyExecutionCounter: 0);
    AssertEqual(MotherBrainRainbowBeamAttackPhase.DrainingSamus, firstDrain.PhaseAfter,
        "drain initializer falls through into first resource tick");
    AssertEqual((ushort)0, low.Missiles,
        "depleted missiles clear even when a different HUD item is selected");
    AssertEqual((ushort)0, low.SuperMissiles,
        "selected supers clear HUD item and reach zero");
    AssertEqual((ushort)0, low.PowerBombs,
        "power bombs reach the shared literal-zero reset regardless of HUD selection");
    AssertEqual((ushort)0, low.SelectedHudItem, "selected depleted item clears HUD selection");
    AssertEqual((ushort)0, low.AutoCancelHudItemIndex, "ammo depletion resets auto-cancel index");

    // Walk programs execute in the ordinary enemy-instruction stage after AI. A complete
    // really-slow list contains nine ten-frame spritemaps and reaches sleep on call 91.
    var backwardBody = new MotherBrainBodyAnimationState
    {
        XPosition = 64,
        YPosition = 100,
        Form = 3,
    };
    backwardBody.SetInstructionList(
        MotherBrainRainbowBeamAttackSequence.BodyWalkingBackwardReallySlowInstructionList);
    int backwardAnimationCalls = 0;
    int backwardFootsteps = 0;
    while (!backwardBody.Sleeping)
    {
        MotherBrainBodyAnimationStepResult bodyStep = backwardBody.Step(bus);
        backwardAnimationCalls++;
        if (bodyStep.FootstepRequested)
        {
            backwardFootsteps++;
            AssertTrue(bodyStep.FootstepSoundRequested,
                "form-three backward footstep retains otherwise-silent sound request");
            AssertEqual((ushort)1, bodyStep.EarthquakeType, "backward footstep earthquake type");
            AssertEqual((ushort)4, bodyStep.EarthquakeTimer, "backward footstep earthquake timer");
        }
        AssertTrue(backwardAnimationCalls < 100, "backward walk reaches common sleep");
    }
    AssertEqual(91, backwardAnimationCalls, "nine ten-frame backward records then sleep");
    AssertEqual(2, backwardFootsteps, "backward walk executes two footstep opcodes");
    AssertEqual((ushort)40, backwardBody.XPosition, "backward walk literal net X delta -24");
    AssertEqual((ushort)100, backwardBody.YPosition, "backward walk literal Y deltas cancel");
    AssertEqual((ushort)0, backwardBody.Pose, "backward walk restores standing pose");
    AssertEqual((ushort)0xfffa, backwardBody.Bg2XScroll,
        "backward walk keeps BG2 X at $22 minus body X");
    AssertEqual((ushort)0, backwardBody.Bg2YScroll,
        "backward walk inverse BG2 Y deltas cancel");
    AssertEqual((ushort)0x9972, backwardBody.InstructionPointer,
        "backward common sleep pins its own opcode address");

    var forwardBody = new MotherBrainBodyAnimationState
    {
        XPosition = 64,
        YPosition = 100,
        Form = 2,
    };
    forwardBody.SetInstructionList(
        MotherBrainRainbowBeamAttackSequence.BodyWalkingForwardReallySlowInstructionList);
    int forwardAnimationCalls = 0;
    int forwardFootsteps = 0;
    while (!forwardBody.Sleeping)
    {
        MotherBrainBodyAnimationStepResult bodyStep = forwardBody.Step(bus);
        forwardAnimationCalls++;
        if (bodyStep.FootstepRequested)
        {
            forwardFootsteps++;
            AssertTrue(!bodyStep.FootstepSoundRequested,
                "non-form-three forward footstep suppresses sound request");
        }
        AssertTrue(forwardAnimationCalls < 100, "forward walk reaches common sleep");
    }
    AssertEqual(91, forwardAnimationCalls, "nine ten-frame forward records then sleep");
    AssertEqual(2, forwardFootsteps, "forward walk executes two footstep opcodes");
    AssertEqual((ushort)88, forwardBody.XPosition, "forward walk literal net X delta +24");
    AssertEqual((ushort)100, forwardBody.YPosition, "forward walk literal Y deltas cancel");
    AssertEqual((ushort)0, forwardBody.Pose, "forward walk restores standing pose");
    AssertEqual((ushort)0x9850, forwardBody.InstructionPointer,
        "forward common sleep pins its own opcode address");

    // The finish-off loop can idle Mother Brain into a lean and must later wait for the
    // corresponding stand-up bytecode. Verify both posture directions and the longer fast
    // crouch recovery independently so their body-world/BG2 compensation cannot regress.
    var postureBody = new MotherBrainBodyAnimationState
    {
        XPosition = 64,
        YPosition = 100,
    };
    postureBody.SetInstructionList(
        MotherBrainRainbowBeamAttackSequence.BodyLeaningDownInstructionList);
    int leanCalls = 0;
    while (!postureBody.Sleeping)
    {
        postureBody.Step(bus);
        leanCalls++;
        AssertTrue(leanCalls < 30, "lean-down animation reaches sleep");
    }
    AssertEqual(17, leanCalls, "two eight-frame lean records then sleep");
    AssertEqual((ushort)6, postureBody.Pose, "lean-down list publishes pose six");
    AssertEqual((ushort)112, postureBody.YPosition, "lean-down list moves body down twelve");
    AssertEqual((ushort)0xfff4, postureBody.Bg2YScroll,
        "lean-down body Y movement is cancelled in BG2");
    AssertEqual((ushort)0xffe6, postureBody.Bg2XScroll,
        "lean-down opcode applies temporary left-four BG2 compensation");

    postureBody.SetInstructionList(
        MotherBrainRainbowBeamAttackSequence.BodyStandingUpAfterLeaningDownInstructionList);
    int leanStandCalls = 0;
    while (!postureBody.Sleeping)
    {
        postureBody.Step(bus);
        leanStandCalls++;
        AssertTrue(leanStandCalls < 30, "lean stand-up animation reaches sleep");
    }
    AssertEqual(17, leanStandCalls, "lean recovery has two eight-frame records");
    AssertEqual((ushort)0, postureBody.Pose, "lean recovery restores standing pose");
    AssertEqual((ushort)100, postureBody.YPosition, "lean recovery reverses twelve-pixel drop");
    AssertEqual((ushort)0, postureBody.Bg2YScroll, "lean recovery reverses BG2 Y offset");
    AssertEqual((ushort)0xffe0, postureBody.Bg2XScroll,
        "lean recovery finishes with right-two BG2 compensation");

    var crouchedBody = new MotherBrainBodyAnimationState
    {
        XPosition = 64,
        YPosition = 138,
        Pose = 3,
    };
    crouchedBody.SetInstructionList(
        MotherBrainRainbowBeamAttackSequence.BodyStandingUpAfterCrouchingFastInstructionList);
    int crouchStandCalls = 0;
    while (!crouchedBody.Sleeping)
    {
        crouchedBody.Step(bus);
        crouchStandCalls++;
        AssertTrue(crouchStandCalls < 50, "fast crouch stand-up reaches sleep");
    }
    AssertEqual(33, crouchStandCalls, "four eight-frame fast stand records then sleep");
    AssertEqual((ushort)0, crouchedBody.Pose, "fast crouch recovery restores standing pose");
    AssertEqual((ushort)100, crouchedBody.YPosition, "fast crouch recovery moves body up 38");
    AssertEqual((ushort)38, crouchedBody.Bg2YScroll,
        "fast crouch recovery applies inverse 38-pixel BG2 Y movement");

    WriteTestWord(bus, 0xa99000, 0xffff);
    var unknownBody = new MotherBrainBodyAnimationState();
    unknownBody.SetInstructionList(0x9000);
    AssertThrows<InvalidOperationException>(() => unknownBody.Step(bus),
        "unknown Mother Brain animation command is an explicit translation seam");

    // Now drive the entire `$B8EB-$B983` repeat cycle with the body interpreter after each
    // AI call. This validates both independent timers and their three native fallthroughs.
    var repeatSamus = new SamusState
    {
        Health = 800,
        XPosition = 220,
        YPosition = 124,
    };
    var repeatAttack = new MotherBrainRainbowBeamAttackSequence
    {
        BrainXPosition = 64,
        BrainYPosition = 96,
    };
    repeatAttack.Body.XPosition = 64;
    repeatAttack.Body.YPosition = 100;
    repeatAttack.StartAttackCycle();
    AssertEqual(MotherBrainRainbowBeamAttackPhase.StartCharging, repeatAttack.Phase,
        "repeat setup installs first charge wait");
    AssertEqual((ushort)0x0100, repeatAttack.FunctionTimer, "repeat first wait starts at $100");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.HeadNeutralPhase2InstructionList,
        repeatAttack.HeadInstructionList, "repeat setup selects neutral phase-two head art");

    int firstChargeCalls = 0;
    while (repeatAttack.Phase == MotherBrainRainbowBeamAttackPhase.StartCharging)
    {
        repeatAttack.Step(bus, repeatSamus, 0, 0);
        repeatAttack.Body.Step(bus);
        firstChargeCalls++;
    }
    AssertEqual(257, firstChargeCalls, "$100 first charge wait expires on DEC call 257");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.RetractNeck, repeatAttack.Phase,
        "first wait falls through into retracting walk");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.HeadChargingRainbowInstructionList,
        repeatAttack.HeadInstructionList, "first wait selects charging head program");
    AssertEqual((ushort)1, repeatAttack.Body.Pose,
        "same-frame enemy stage begins the requested backward body animation");

    int retractCalls = 0;
    while (repeatAttack.Phase == MotherBrainRainbowBeamAttackPhase.RetractNeck)
    {
        repeatAttack.Step(bus, repeatSamus, 0, 0);
        repeatAttack.Body.Step(bus);
        retractCalls++;
        AssertTrue(retractCalls < 100, "retracting walk reaches X $28");
    }
    AssertEqual(41, retractCalls,
        "AI advances after the -15 opcode crosses hard X $30 boundary");
    AssertEqual((ushort)46, repeatAttack.Body.XPosition,
        "retract AI handoff occurs at overshot X $2E before walk animation settles");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.WaitForCharge, repeatAttack.Phase,
        "retract target falls through into second charge wait");
    AssertEqual((ushort)0x0050, repeatAttack.NeckAngleDelta, "retract neck NTSC delta");
    AssertEqual((ushort)8, repeatAttack.LowerNeckMovementIndex, "retract lower neck index");
    AssertEqual((ushort)6, repeatAttack.UpperNeckMovementIndex, "retract upper neck index");
    AssertEqual((ushort)0x00ff, repeatAttack.FunctionTimer,
        "second wait is decremented once by retract fallthrough");

    int secondChargeCalls = 0;
    MotherBrainRainbowBeamAttackStepResult chargeCompletion = default;
    while (repeatAttack.Phase == MotherBrainRainbowBeamAttackPhase.WaitForCharge)
    {
        chargeCompletion = repeatAttack.Step(bus, repeatSamus, 0, 0);
        repeatAttack.Body.Step(bus);
        secondChargeCalls++;
    }
    AssertEqual(256, secondChargeCalls, "remaining second charge wait calls");
    AssertTrue(chargeCompletion.ChargeSoundQueued,
        "second charge underflow queues sound-library-two effect $71");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.StartFiring, repeatAttack.Phase,
        "second wait falls through neck-down setup and first firing call");
    AssertEqual((ushort)8, repeatAttack.SamusProjectileCooldownTimer,
        "neck-down setup writes projectile cooldown eight");
    AssertEqual((ushort)6, repeatAttack.LowerNeckMovementIndex, "firing lower neck index");
    AssertEqual((ushort)6, repeatAttack.UpperNeckMovementIndex, "firing upper neck index");
    AssertEqual((ushort)0x0500, repeatAttack.NeckAngleDelta, "firing NTSC neck delta");
    AssertEqual((ushort)0x0180, repeatAttack.AngularWidth,
        "fallthrough firing call widens prior zero width");
    AssertEqual((ushort)0x000f, repeatAttack.FunctionTimer,
        "fallthrough firing call decrements regional timer 16");

    MotherBrainRainbowBeamAttackStepResult frozenCharge = repeatAttack.Step(
        bus, repeatSamus, 0, 0, powerBombActive: true);
    AssertEqual((ushort)0x000f, frozenCharge.FunctionTimer,
        "active power bomb freezes firing countdown");
    AssertEqual((ushort)0x0300, frozenCharge.AngularWidth,
        "active power bomb does not freeze beam aiming/width growth");

    int firingCalls = 0;
    while (repeatAttack.Phase == MotherBrainRainbowBeamAttackPhase.StartFiring)
    {
        repeatAttack.Step(bus, repeatSamus, 0, 0);
        repeatAttack.Body.Step(bus);
        firingCalls++;
    }
    AssertEqual(16, firingCalls, "timer $000F reaches active beam on 16th unfrozen call");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.MoveSamusTowardWall, repeatAttack.Phase,
        "start-firing underflow executes `$B983` active setup in same call");
    AssertEqual((ushort)0x0200, repeatAttack.AngularWidth,
        "active setup resets prefire width to $0200");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.HeadFiringRainbowInstructionList,
        repeatAttack.HeadInstructionList, "active setup selects firing head program");
    AssertTrue(repeatSamus.InputLocked, "repeat cycle ends by locking Samus through command five");

    var exactThreshold = new SamusState
    {
        Health = 700,
        EquippedItems = 1, // Varia carry makes each of the 300 hits subtract one.
        XPosition = 220,
        YPosition = 124,
    };
    var thresholdAttack = new MotherBrainRainbowBeamAttackSequence();
    thresholdAttack.StartActiveBeam(bus, exactThreshold);
    AssertEqual(DrainedGetUpHandler.AbleToStand, exactThreshold.Drained.GetUpHandler,
        "energy exactly $02BC takes native BPL able branch");

    int thresholdRouteCalls = 0;
    while (thresholdAttack.Phase != MotherBrainRainbowBeamAttackPhase.DecideNextAction)
    {
        thresholdAttack.Step(
            bus,
            exactThreshold,
            enemyFrameCounter: unchecked((ushort)thresholdRouteCalls),
            mainEnemyExecutionCounter: unchecked((ushort)thresholdRouteCalls));
        thresholdRouteCalls++;
        AssertTrue(thresholdRouteCalls < 500, "exact-threshold route reaches decision timer");
    }
    while (thresholdAttack.Phase == MotherBrainRainbowBeamAttackPhase.DecideNextAction)
        thresholdAttack.Step(bus, exactThreshold, 0, 0);
    AssertEqual((ushort)400, exactThreshold.Health,
        "700 with Varia reaches exact repeat/finish boundary after 300 hits");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.RepeatAttack, thresholdAttack.Phase,
        "health exactly $0190 selects repeat attack");
    thresholdAttack.Step(bus, exactThreshold, 0, 0);
    AssertEqual(MotherBrainRainbowBeamAttackPhase.StartCharging, thresholdAttack.Phase,
        "repeat pointer executes neck-extension setup on following AI call");
    AssertEqual((ushort)0x0100, thresholdAttack.FunctionTimer,
        "repeated neck-extension setup reloads first charge timer");

    // `$BD45`'s first threshold is suit-dependent and inclusive. Check every retail suit
    // path at the exact boundary and one energy above it before exercising RNG selection.
    foreach ((ushort items, ushort threshold, string suitName) in new[]
    {
        ((ushort)0x0000, (ushort)340, "Power"),
        ((ushort)0x0001, (ushort)180, "Varia"),
        ((ushort)0x0020, (ushort)100, "Gravity"),
    })
    {
        var atBoundary = new SamusState { Health = threshold, EquippedItems = items };
        var boundaryFinish = new MotherBrainRainbowBeamAttackSequence();
        boundaryFinish.Body.XPosition = 64;
        boundaryFinish.Body.YPosition = 100;
        boundaryFinish.StartFinishOffSequence();
        boundaryFinish.Step(bus, atBoundary, 0, 0);
        AssertEqual(MotherBrainRainbowBeamAttackPhase.FinishStandUp, boundaryFinish.Phase,
            $"{suitName} exact finish-off threshold takes BPL done branch");

        var aboveBoundary = new SamusState
        {
            Health = unchecked((ushort)(threshold + 1)),
            EquippedItems = items,
        };
        var aboveFinish = new MotherBrainRainbowBeamAttackSequence();
        aboveFinish.Body.XPosition = 64;
        aboveFinish.Body.YPosition = 100;
        aboveFinish.StartFinishOffSequence();
        aboveFinish.Step(bus, aboveBoundary, 1, 0, randomNumberSeed: 0);
        AssertEqual(MotherBrainRainbowBeamAttackPhase.FinishSamusOff, aboveFinish.Phase,
            $"{suitName} threshold plus one remains in attack loop");
    }

    var onionFinishSamus = new SamusState { Health = 341 };
    var onionFinish = new MotherBrainRainbowBeamAttackSequence();
    onionFinish.Body.XPosition = 64;
    onionFinish.StartFinishOffSequence();
    MotherBrainRainbowBeamAttackStepResult onionAttack = onionFinish.Step(
        bus, onionFinishSamus, 1, 0, randomNumberSeed: 0x0fef);
    AssertEqual(MotherBrainFinishOffAttackKind.TwoOnionRings, onionAttack.FinishOffAttack,
        "RNG $FEF selects two onion rings at upper boundary minus one");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.HeadAttackingTwoOnionRingsPhase2InstructionList,
        onionFinish.HeadInstructionList, "onion-ring selection installs retail head list");

    var bombFinish = new MotherBrainRainbowBeamAttackSequence();
    bombFinish.Body.XPosition = 64;
    bombFinish.StartFinishOffSequence();
    MotherBrainRainbowBeamAttackStepResult bombAttack = bombFinish.Step(
        bus, new SamusState { Health = 341 }, 1, 0, randomNumberSeed: 0x0ff0);
    AssertEqual(MotherBrainFinishOffAttackKind.Bomb, bombAttack.FinishOffAttack,
        "RNG $FF0 selects bomb at exact BCS boundary");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.HeadAttackingBombPhase2InstructionList,
        bombFinish.HeadInstructionList, "bomb selection installs retail head list");

    var idleFinish = new MotherBrainRainbowBeamAttackSequence();
    idleFinish.Body.XPosition = 64;
    idleFinish.Body.YPosition = 100;
    idleFinish.StartFinishOffSequence();
    MotherBrainRainbowBeamAttackStepResult idlePosture = idleFinish.Step(
        bus,
        new SamusState { Health = 341 },
        enemyFrameCounter: 0,
        mainEnemyExecutionCounter: 0,
        randomNumberSeed: 0x09c0);
    AssertTrue(idlePosture.BodyPostureRequested,
        "no-attack frame with low byte $C0 requests posture change");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.BodyLeaningDownInstructionList,
        idleFinish.Body.InstructionPointer, "standing posture helper requests lean bytecode");

    // Drive the complete low-health handoff. The initial forward walk runs in the separate
    // enemy-instruction stage; only after it restores pose zero can `$C670` report carry.
    var finalSamus = new SamusState { Health = 340 };
    var finalAttack = new MotherBrainRainbowBeamAttackSequence();
    finalAttack.Body.XPosition = 64;
    finalAttack.Body.YPosition = 100;
    finalAttack.StartFinishOffSequence();
    finalAttack.Body.Step(bus); // Same-frame enemy stage following `$BB1A`'s list request.
    finalAttack.Step(bus, finalSamus, 0, 0);
    finalAttack.Body.Step(bus);
    AssertEqual(MotherBrainRainbowBeamAttackPhase.FinishStandUp, finalAttack.Phase,
        "low-health finish loop installs stand-up body function");

    int standWaitCalls = 0;
    while (finalAttack.Phase == MotherBrainRainbowBeamAttackPhase.FinishStandUp)
    {
        finalAttack.Step(bus, finalSamus, 0, 0);
        finalAttack.Body.Step(bus);
        standWaitCalls++;
        AssertTrue(standWaitCalls < 120, "finish-off stand-up waits for forward walk pose zero");
    }
    AssertEqual(MotherBrainRainbowBeamAttackPhase.AdmireJobWellDone, finalAttack.Phase,
        "standing carry enters admire delay");
    AssertEqual((ushort)0x000f, finalAttack.FunctionTimer,
        "stand-up fallthrough immediately decrements admire timer");
    AssertEqual((ushort)88, finalAttack.Body.XPosition,
        "initial finish-off walk completes one literal really-slow program");

    int admireCalls = 0;
    while (finalAttack.Phase == MotherBrainRainbowBeamAttackPhase.AdmireJobWellDone)
    {
        finalAttack.Step(bus, finalSamus, 0, 0);
        finalAttack.Body.Step(bus);
        admireCalls++;
    }
    AssertEqual(16, admireCalls, "remaining admire delay underflows after sixteen calls");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.HeadStretchingPhase2InstructionList,
        finalAttack.HeadInstructionList, "admire expiry installs stretching head animation");
    AssertEqual((ushort)0x0100, finalAttack.FunctionTimer,
        "admire expiry loads 256-count final charge");

    int finalChargeCalls = 0;
    MotherBrainRainbowBeamAttackStepResult firstTile = default;
    while (finalAttack.Phase == MotherBrainRainbowBeamAttackPhase.ChargeFinalRainbowBeam)
    {
        firstTile = finalAttack.Step(bus, finalSamus, 0, 0);
        finalAttack.Body.Step(bus);
        finalChargeCalls++;
    }
    AssertEqual(257, finalChargeCalls, "final charge `$100` expires on DEC call 257");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.HeadChargingRainbowInstructionList,
        finalAttack.HeadInstructionList, "final charge expiry installs charging head list");
    AssertTrue(firstTile.SpriteTileTransfer is
        { EntryIndex: 0, Size: 0x0200, SourceAddress: 0xb18400, VramDestination: 0x7c00 },
        "charge underflow falls through into first Baby tile transfer");

    var transfers = new List<MotherBrainSpriteTileTransferRequest>
    {
        firstTile.SpriteTileTransfer!.Value,
    };
    MotherBrainRainbowBeamAttackStepResult spawnCall = firstTile;
    while (finalAttack.Phase == MotherBrainRainbowBeamAttackPhase.LoadBabyMetroidTiles)
    {
        spawnCall = finalAttack.Step(bus, finalSamus, 0, 0);
        finalAttack.Body.Step(bus);
        if (spawnCall.SpriteTileTransfer is { } transfer)
            transfers.Add(transfer);
    }
    AssertEqual(4, transfers.Count, "Baby graphics list emits four frame-spread transfers");
    AssertEqual(new MotherBrainSpriteTileTransferRequest(1, 0x0200, 0xb18600, 0x7d00),
        transfers[1], "Baby transfer entry one");
    AssertEqual(new MotherBrainSpriteTileTransferRequest(2, 0x0200, 0xb18800, 0x7e00),
        transfers[2], "Baby transfer entry two");
    AssertEqual(new MotherBrainSpriteTileTransferRequest(3, 0x0200, 0xb18a00, 0x7f00),
        transfers[3], "Baby transfer entry three");
    AssertTrue(spawnCall.BabySpawnRequested && finalAttack.BabyMetroidSpawned,
        "terminating transfer entry retracts head and requests Baby spawn");
    AssertEqual((ushort)0x0050, finalAttack.NeckAngleDelta,
        "Baby spawn call uses NTSC head-retraction delta");
    AssertEqual((ushort)0x0100, finalAttack.FunctionTimer,
        "Baby spawn call loads final-beam wait");

    int finalBeamCalls = 0;
    MotherBrainRainbowBeamAttackStepResult finalShot = default;
    while (finalAttack.Phase == MotherBrainRainbowBeamAttackPhase.FireFinalRainbowBeam)
    {
        finalShot = finalAttack.Step(bus, finalSamus, 0, 0);
        finalAttack.Body.Step(bus);
        finalBeamCalls++;
    }
    AssertEqual(257, finalBeamCalls, "final-beam wait `$100` expires on DEC call 257");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.FinalRainbowBeamHolding, finalAttack.Phase,
        "final shot installs self-return holding function");
    AssertTrue(finalShot.FinalBeamSoundQueued,
        "final shot queues sound-library-two effect $71");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.HeadFiringRainbowInstructionList,
        finalAttack.HeadInstructionList, "final shot installs firing head animation");
    AssertEqual((ushort)6, finalAttack.LowerNeckMovementIndex,
        "final shot lower neck index");
    AssertEqual((ushort)6, finalAttack.UpperNeckMovementIndex,
        "final shot upper neck index");
    AssertEqual((ushort)0x0500, finalAttack.NeckAngleDelta,
        "final shot NTSC neck delta");

    Console.WriteLine("  Mother Brain actor: ROM posture/walk bytecode, repeat/active/final rainbow chain, thresholds, VRAM, and Baby spawn agree.");
}

/// <summary>
/// Runs the complete `$A9:C710-$C8E1` entrance from initialization through head pinning.
/// The hard-coded milestone coordinates came from the private retail-ROM runner, while the
/// synthetic sine fixture is generated independently from the table's documented definition.
/// This combination catches timer, angle, multiplication, subposition, collision, and
/// cross-enemy ordering regressions without making the ordinary verifier depend on a ROM.
/// </summary>
static void VerifyBabyMetroidCutsceneEntrance()
{
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
    SeedBabyCeilingToSamusRoute(bus);

    // Controller one reads the current `$E9` direction byte, then rebinds the new `$EB`
    // animation pointer without refreshing radii. These are the only Samus ROM fields the
    // entrance consumes; the dedicated drained-controller suite proves their animation.
    bus.WriteBytes(0x91b629 + SamusState.DrainedCrouchingLeftPose * 8,
        [0x04, 0x1b, 0xff, 0xff, 0xfc, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b629 + SamusState.DrainedStandingLeftPose * 8,
        [0x04, 0x1b, 0xff, 0xff, 0xfc, 0x00, 0x15, 0x00]);
    WriteTestWord(bus, 0x91b010 + SamusState.DrainedStandingLeftPose * 2, 0xc100);
    bus.WriteByte(0x91c100, 0x10);

    var samus = new SamusState
    {
        Pose = SamusState.DrainedCrouchingLeftPose,
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
    AssertEqual((ushort)0x3800, baby.Properties, "Baby population/init property OR");
    AssertEqual((ushort)0x0e00, baby.Palette, "Baby cutscene palette");
    AssertEqual((ushort)0x00a0, baby.GraphicsOffset, "Baby transferred-tile offset");
    AssertEqual(BabyMetroidCutsceneState.InitialInstructionList, baby.InstructionList,
        "Baby initial instruction list");
    AssertEqual(new BabyMetroidCutscenePoint(0x0140, 0, 0x0060, 0),
        new BabyMetroidCutscenePoint(
            baby.XPosition, baby.XSubposition, baby.YPosition, baby.YSubposition),
        "Baby initialization overwrites population coordinates");

    // `$F8` reaches zero without expiring. This is 248 visibly stationary calls, not an
    // approximate four-second host delay.
    for (int call = 1; call <= 248; call++)
        baby.Step(bus, samus, motherBrain);
    AssertEqual(BabyMetroidCutscenePhase.DashOntoScreen, baby.Phase,
        "Baby dash delay retains function at timer zero");
    AssertEqual((ushort)0, baby.FunctionTimer, "Baby dash delay exact zero boundary");
    AssertEqual((ushort)0x0140, baby.XPosition, "Baby remains still through call 248");
    AssertEqual((ushort)0x0060, baby.YPosition, "Baby Y remains still through call 248");

    BabyMetroidCutsceneStepResult firstCurve = baby.Step(bus, samus, motherBrain);
    AssertEqual(BabyMetroidCutscenePhase.CurveTowardMotherBrainHead, baby.Phase,
        "Baby call 249 falls through into curve function");
    AssertEqual((ushort)0xd680, baby.Angle, "Baby first curve angle");
    AssertEqual((ushort)0x0a00, baby.Speed, "Baby first curve speed");
    AssertEqual((ushort)0xf772, firstCurve.XVelocity, "Baby first ROM sine X velocity");
    AssertEqual((ushort)0x051e, firstCurve.YVelocity, "Baby first ROM cosine Y velocity");
    AssertEqual(new BabyMetroidCutscenePoint(0x0137, 0x7200, 0x0065, 0x1e00),
        firstCurve.After,
        "Baby first curve fixed-point displacement");

    BabyMetroidCutsceneStepResult latch = default;
    bool sawBodyStumbleRequest = false;
    bool sawMotherBrainInterrupt = false;
    bool sawLatchSound = false;
    int calls = 249;
    while (baby.Phase != BabyMetroidCutscenePhase.WaitForMotherBrainToTurnToCorpse &&
           calls < 700)
    {
        latch = baby.Step(bus, samus, motherBrain);
        calls++;
        sawBodyStumbleRequest |= latch.BodyStumbleRequested;
        sawMotherBrainInterrupt |= latch.MotherBrainInterrupted;
        sawLatchSound |= latch.LatchSoundQueued;

        if (calls == 259)
        {
            AssertEqual(BabyMetroidCutscenePhase.GetRightUpInMotherBrainsFace, baby.Phase,
                "Baby curve timer expires on call 259");
            AssertEqual(new BabyMetroidCutscenePoint(0x00da, 0x0c00, 0x0086, 0x7a00),
                latch.After,
                "Baby curve endpoint from retail ROM");
        }
        else if (calls == 269)
        {
            AssertEqual(BabyMetroidCutscenePhase.LatchOntoMotherBrain, baby.Phase,
                "Baby face timer expires on call 269");
            AssertTrue(latch.SamusStandingRequested,
                "Baby face completion calls drained controller one");
            AssertEqual(SamusState.DrainedStandingLeftPose, samus.Pose,
                "Baby face completion installs left drained standing pose");
            AssertEqual(new BabyMetroidCutscenePoint(0x008c, 0xc400, 0x004b, 0x6300),
                latch.After,
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
    AssertEqual(new BabyMetroidCutscenePoint(0x0040, 0x1400, 0x0048, 0xd200),
        latch.After,
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
    AssertEqual((ushort)0x002f, motherBrain.FunctionTimer,
        "Mother Brain first regain call decrements $30 to $2F");
    AssertEqual((ushort)3, motherBrain.Body.Form, "Baby drain changes Mother Brain form to three");
    AssertEqual((ushort)8, motherBrain.LowerNeckMovementIndex,
        "Mother Brain taken-aback lower neck index");
    AssertEqual((ushort)8, motherBrain.UpperNeckMovementIndex,
        "Mother Brain taken-aback upper neck index");
    AssertEqual((ushort)0x0700, motherBrain.NeckAngleDelta,
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
    AssertEqual((ushort)2, motherBrain.LowerNeckMovementIndex,
        "Mother Brain drained firing lower neck index");
    AssertEqual((ushort)4, motherBrain.UpperNeckMovementIndex,
        "Mother Brain drained firing upper neck index");

    int drainFrame = 0;
    int beamRunOutFrame = 0;
    int lowPowerFrame = 0;
    int greyStartFrame = 0;
    int corpseFrame = 0;
    int stopDrainingFrame = 0;
    int letGoFrame = 0;
    int dustFrame = 0;
    int ceilingFrame = 0;
    // Capture the actual three allocation records, not merely the transition edge. This
    // independently locks the signed offset arithmetic and native helper-call order.
    var releaseDustClouds = new List<BabyMetroidReleaseDustRequest>(capacity: 3);
    bool sawSamusCrouch = false;
    while (baby.Phase != BabyMetroidCutscenePhase.MoveToSamus)
    {
        MotherBrainRainbowBeamAttackPhase mbBefore = motherBrain.Phase;
        BabyMetroidCutscenePhase babyBefore = baby.Phase;
        motherBrain.Step(
            bus,
            samus,
            enemyFrameCounter: unchecked((ushort)drainFrame),
            mainEnemyExecutionCounter: unchecked((ushort)drainFrame));
        motherBrain.Body.Step(bus);
        motherBrain.StepNeckMovement(bus, samus);
        BabyMetroidCutsceneStepResult babyDrain = baby.Step(
            bus,
            samus,
            motherBrain,
            enemyFrameCounter: unchecked((ushort)drainFrame));
        motherBrain.StepBrainShakeForDraw();
        drainFrame++;

        if (mbBefore != motherBrain.Phase)
        {
            if (motherBrain.Phase == MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidRainbowBeamRunOut)
                beamRunOutFrame = drainFrame;
            if (motherBrain.Phase == MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidGoIntoLowPowerMode)
                lowPowerFrame = drainFrame;
            if (motherBrain.Phase == MotherBrainRainbowBeamAttackPhase.DrainedByBabyMetroidTransitionToGrey)
                greyStartFrame = drainFrame;
            if (motherBrain.Phase == MotherBrainRainbowBeamAttackPhase.Phase2ReviveSelfInanimateGrey)
                corpseFrame = drainFrame;
        }
        if (babyBefore != baby.Phase)
        {
            if (baby.Phase == BabyMetroidCutscenePhase.StopDraining)
                stopDrainingFrame = drainFrame;
            if (baby.Phase == BabyMetroidCutscenePhase.LetGoAndSpawnDustClouds)
                letGoFrame = drainFrame;
            if (baby.Phase == BabyMetroidCutscenePhase.MoveToTheCeiling)
                dustFrame = drainFrame;
            if (baby.Phase == BabyMetroidCutscenePhase.MoveToSamus)
                ceilingFrame = drainFrame;
        }
        releaseDustClouds.AddRange(babyDrain.ReleaseDustClouds);
        sawSamusCrouch |= babyDrain.SamusCrouchingRequested;
        AssertTrue(drainFrame < 2500,
            $"Baby drain/release reaches ceiling; MB={motherBrain.Phase}/stage{motherBrain.PainfulWalkingStage}/" +
            $"body({motherBrain.Body.XPosition},{motherBrain.Body.YPosition}) pose{motherBrain.Body.Pose}, " +
            $"neck={motherBrain.LowerNeckMovementIndex}/{motherBrain.UpperNeckMovementIndex}, Baby={baby.Phase}");
    }

    // Hard boundaries from the independent bytecode fixture. These counts begin with the
    // first `$BE96` call after the already-proven 49-call regain phase.
    AssertEqual(593, beamRunOutFrame, "painful stage six ends rainbow beam");
    AssertEqual(977, lowPowerFrame, "painful stage eight enters low-power mode");
    AssertEqual(1439, greyStartFrame, "neck raise and crouch delay reach grey transition");
    AssertEqual(1591, corpseFrame, "nine grey-table probes publish corpse state");
    AssertEqual(corpseFrame, stopDrainingFrame,
        "later Baby slot observes corpse flag on its publication frame");
    AssertEqual(1656, letGoFrame, "stop-draining `$40` expires after 65 calls");
    AssertEqual(1689, dustFrame, "let-go `$20` requests dust on call 33");
    AssertEqual(1703, ceilingFrame, "gradual ceiling acceleration reaches collision rectangle");
    AssertEqual((ushort)40, motherBrain.Body.XPosition, "corpse body rests at rear X");
    AssertEqual((ushort)138, motherBrain.Body.YPosition, "fast crouch lowers body by 38 pixels");
    AssertEqual((ushort)81, motherBrain.BrainXPosition, "neck geometry publishes corpse brain X");
    AssertEqual((ushort)78, motherBrain.BrainYPosition, "neck geometry publishes corpse brain Y");
    AssertEqual((ushort)81, baby.XPosition, "ceiling handoff Baby X");
    AssertEqual((ushort)40, baby.YPosition, "ceiling handoff Baby Y after common mover");
    AssertEqual((ushort)0x0000, baby.XVelocity, "ceiling handoff X velocity");
    AssertEqual((ushort)0xff78, baby.YVelocity,
        "synthetic ceiling handoff preserves its independently accumulated Y velocity");
    AssertEqual(3, releaseDustClouds.Count,
        "Baby release requests exactly three Mother Brain head dust clouds");
    AssertEqual(new BabyMetroidReleaseDustRequest(65, 70, 9), releaseDustClouds[0],
        "Baby release first dust uses brain offset (-16,-8)");
    AssertEqual(new BabyMetroidReleaseDustRequest(81, 62, 9), releaseDustClouds[1],
        "Baby release second dust uses brain offset (0,-16)");
    AssertEqual(new BabyMetroidReleaseDustRequest(97, 70, 9), releaseDustClouds[2],
        "Baby release third dust uses brain offset (+16,-8)");
    AssertTrue(sawSamusCrouch, "Baby ceiling collision calls drained controller four");
    AssertEqual(BabyMetroidCutsceneState.CeilingToSamusMovementTable,
        baby.MovementTablePointer,
        "Baby ceiling collision installs `$CA24` movement table");
    AssertEqual((ushort)0x8ca0, motherBrain.BrainHealth,
        "Mother Brain grey completion rewrites brain health to 36,000");
    AssertEqual((ushort)1, motherBrain.Phase2CorpseState,
        "Mother Brain grey completion publishes corpse state one");
    AssertEqual(SamusState.DrainedCrouchingLeftPose, samus.Pose,
        "ceiling collision installs left drained crouching pose");

    // Continue in native enemy-slot order through the eight ROM route records, generic
    // touch AI, and 699 one-point healing calls. This deliberately remains an integrated
    // synthetic fixture: its earlier entrance used a stationary neck target, so its inherited
    // subpixels are not falsely presented as coordinates captured from the full retail run.
    var routePointerFrames = new Dictionary<ushort, int>();
    var routePointerPoints = new Dictionary<ushort, BabyMetroidCutscenePoint>();
    int latchOntoSamusFrame = 0;
    int healSamusFrame = 0;
    int healingCompleteFrame = 0;
    bool sawSamusTouch = false;
    bool sawAmbientCryThreshold = false;
    while (baby.Phase != BabyMetroidCutscenePhase.IdleUntilNoHealth)
    {
        ushort pointerBefore = baby.MovementTablePointer;
        BabyMetroidCutscenePhase phaseBefore = baby.Phase;
        motherBrain.Step(
            bus,
            samus,
            enemyFrameCounter: unchecked((ushort)drainFrame),
            mainEnemyExecutionCounter: unchecked((ushort)drainFrame));
        motherBrain.Body.Step(bus);
        motherBrain.StepNeckMovement(bus, samus);
        BabyMetroidCutsceneStepResult routeStep = baby.Step(
            bus,
            samus,
            motherBrain,
            enemyFrameCounter: unchecked((ushort)drainFrame),
            // `$FA0` proves the inclusive random-cry threshold without influencing motion.
            randomNumber: 0x0fa0);
        motherBrain.StepBrainShakeForDraw();
        drainFrame++;

        sawSamusTouch |= routeStep.SamusTouchCollision;
        sawAmbientCryThreshold |= routeStep.AmbientCrySoundQueued;
        if (pointerBefore != baby.MovementTablePointer)
        {
            routePointerFrames[baby.MovementTablePointer] = drainFrame;
            routePointerPoints[baby.MovementTablePointer] = routeStep.After;
        }
        if (phaseBefore != baby.Phase)
        {
            if (baby.Phase == BabyMetroidCutscenePhase.LatchOntoSamus)
                latchOntoSamusFrame = drainFrame;
            if (baby.Phase == BabyMetroidCutscenePhase.HealSamusToFullHealth)
                healSamusFrame = drainFrame;
            if (baby.Phase == BabyMetroidCutscenePhase.IdleUntilNoHealth)
            {
                healingCompleteFrame = drainFrame;
                AssertTrue(routeStep.HealingCompleted,
                    "final one-point heal publishes completion on the transition call");
            }
        }
        AssertTrue(drainFrame < 3200,
            $"Baby route/heal reaches idle state; phase={baby.Phase}, pointer=${baby.MovementTablePointer:X4}");
    }

    // These are the deterministic witnesses produced by this fixture's own inherited
    // fixed-point state. The DebugRunner separately locks the retail-ROM witnesses; keeping
    // both sets makes any accidental dependence on a fabricated initial subposition visible.
    AssertEqual(1781, routePointerFrames[0xca2c], "route reaches `$CA2C` record");
    AssertEqual(1847, routePointerFrames[0xca34], "route reaches `$CA34` record");
    AssertEqual(1946, routePointerFrames[0xca3c], "route reaches `$CA3C` record");
    AssertEqual(1947, routePointerFrames[0xca44], "overlapping route advances again on next call");
    AssertEqual(2027, routePointerFrames[0xca4c], "route reaches `$CA4C` record");
    AssertEqual(2046, routePointerFrames[0xca54], "route reaches `$CA54` record");
    AssertEqual(2063, routePointerFrames[0xca5c], "route reaches final `$CA5C` record");
    AssertEqual(new BabyMetroidCutscenePoint(0x007e, 0xac00, 0x0051, 0x9700),
        routePointerPoints[0xca2c], "first route-leg endpoint");
    AssertEqual(new BabyMetroidCutscenePoint(0x010b, 0xaf00, 0x008d, 0xc700),
        routePointerPoints[0xca34], "second route-leg endpoint");
    AssertEqual(new BabyMetroidCutscenePoint(0x00e7, 0x1b00, 0x004b, 0x0200),
        routePointerPoints[0xca3c], "third route-leg endpoint");
    AssertEqual(new BabyMetroidCutscenePoint(0x00e5, 0xa400, 0x0049, 0xf400),
        routePointerPoints[0xca44], "fourth route-leg endpoint");
    AssertEqual(new BabyMetroidCutscenePoint(0x00c7, 0x7900, 0x0059, 0x4b00),
        routePointerPoints[0xca4c], "fifth route-leg endpoint");
    AssertEqual(new BabyMetroidCutscenePoint(0x00cb, 0x1c00, 0x0069, 0x0000),
        routePointerPoints[0xca54], "sixth route-leg endpoint");
    AssertEqual(new BabyMetroidCutscenePoint(0x00cd, 0x8d00, 0x007a, 0x0f00),
        routePointerPoints[0xca5c], "seventh route-leg endpoint");
    AssertEqual(2077, latchOntoSamusFrame,
        "final route record overlays +8 with signed `$CA66` function pointer");
    AssertEqual(2095, healSamusFrame,
        "post-main enemy touch reaches `$CF03` latch target");
    AssertEqual(2794, healingCompleteFrame,
        "699 one-point heals reach 899 energy");
    AssertTrue(sawSamusTouch, "generic collision dispatches Baby `$CF03` touch AI");
    AssertTrue(sawAmbientCryThreshold, "route accepts random cry threshold `$FA0`");
    AssertTrue(!baby.CrySoundEnabled, "route/heal keeps ordinary cry request clear");
    AssertTrue(baby.HealthBasedPaletteEnabled, "route enables health-based Baby palette");
    AssertEqual((ushort)899, samus.Health, "Baby healing clamps at maximum energy");
    AssertEqual((ushort)99, samus.ReserveEnergy, "healing completion fills reserve energy");
    AssertEqual((ushort)3200, baby.Health, "healing does not invent Mother Brain damage");

    // Drive the same actor into `$CABD`'s zero-health transition without fabricating a
    // function pointer. One saturating hit represents the already-verified ring collision
    // producer; all following coordinates, timers, and cross-actor writes remain Baby AI.
    BabyMetroidOnionRingHitResult ordinaryFatal = baby.ApplyMotherBrainOnionRingHit(3200);
    AssertTrue(ordinaryFatal.Applied && ordinaryFatal.HealthAfter == 0,
        "ordinary murder volley can saturate Baby health to zero");
    baby.Step(bus, samus, motherBrain);
    AssertEqual(BabyMetroidCutscenePhase.ReleaseSamus, baby.Phase,
        "zero-health idle call installs release function");
    AssertEqual((ushort)0x0140, baby.Health,
        "ordinary zero-health transition restores $140 for flight");

    // The fixed target chain takes a deterministic but fixture-dependent number of calls.
    // Bound it, then use the real `$86:C381` damage path for the intended 79->0 final hit.
    int finalRouteCalls = 0;
    while (baby.Phase != BabyMetroidCutscenePhase.FinalCharge)
    {
        baby.Step(bus, samus, motherBrain, enemyFrameCounter: unchecked((ushort)finalRouteCalls));
        finalRouteCalls++;
        AssertTrue(finalRouteCalls < 1000,
            $"Baby reaches final charge; current phase={baby.Phase}");
    }
    AssertEqual((ushort)0x004f, baby.Health,
        "final-charge staging point assigns literal 79 health");
    BabyMetroidOnionRingHitResult finalFatal = baby.ApplyMotherBrainOnionRingHit();
    AssertTrue(finalFatal.Applied && finalFatal.HealthAfter == 0,
        "one final 80-damage ring saturates 79 health");

    while (baby.Phase != BabyMetroidCutscenePhase.DeathSequence)
    {
        baby.Step(bus, samus, motherBrain, enemyFrameCounter: unchecked((ushort)finalRouteCalls));
        finalRouteCalls++;
        AssertTrue(finalRouteCalls < 1200,
            $"fatal shake/theme delays reach death sequence; current phase={baby.Phase}");
    }
    AssertEqual((ushort)28, samus.AnimationFrame,
        "prepare-Hyper expiry executes Samus command $19");
    AssertEqual((ushort)1, samus.AnimationFrameTimer,
        "Samus command $19 freezes animation with timer one");
    AssertEqual(BabyMetroidSamusRainbowPhase.ActivateWhenEnemyIsLow,
        baby.SamusRainbowPhase,
        "prepare-Hyper expiry installs low-enemy rainbow handler");

    var blackPalettes = new List<BabyMetroidPaletteTransferRequest>();
    var deathExplosions = new List<BabyMetroidDeathExplosionRequest>();
    int deathCalls = 0;
    while (baby.Phase == BabyMetroidCutscenePhase.DeathSequence)
    {
        BabyMetroidCutsceneStepResult babyDeath = baby.Step(
            bus,
            samus,
            motherBrain,
            enemyFrameCounter: unchecked((ushort)deathCalls));
        deathCalls++;
        if (babyDeath.DeathExplosion is { } deathExplosion)
            deathExplosions.Add(deathExplosion);
        if (babyDeath.BabyPaletteTransfer is { } blackPalette)
            blackPalettes.Add(blackPalette);
        AssertTrue(deathCalls < 500, "Baby black fade reaches unload phase");
    }
    AssertEqual(BabyMetroidCutscenePhase.UnloadTiles, baby.Phase,
        "seventh black-table probe installs unload function");
    AssertEqual(6, blackPalettes.Count, "black fade publishes six palette records");
    uint[] expectedBlackSources = [
        0xade90c, 0xade928, 0xade944, 0xade960, 0xade97c, 0xade998,
    ];
    for (int index = 0; index < expectedBlackSources.Length; index++)
    {
        AssertEqual((ushort)(index + 1), blackPalettes[index].PaletteIndex,
            $"black palette {index + 1} index");
        AssertEqual(expectedBlackSources[index], blackPalettes[index].SourceAddress,
            $"black palette {index + 1} source");
        AssertEqual((ushort)0x01e2, blackPalettes[index].DestinationColorIndex,
            $"black palette {index + 1} destination");
    }
    AssertTrue(deathExplosions.Count > 1, "death sequence emits repeating dust explosions");
    AssertEqual((ushort)1, deathExplosions[0].PatternIndex,
        "cleared death pattern increments before first lookup");
    AssertEqual((ushort)2, deathExplosions[1].PatternIndex,
        "death explosions advance to the following table pair");
    AssertEqual(36, (int)deathExplosions[1].XPosition - deathExplosions[0].XPosition,
        "death explosion entries one/two retain their -20/+16 X offsets");
    AssertTrue(baby.IsInvisible, "black-fade completion sets enemy invisibility property");

    var attackTransfers = new List<MotherBrainSpriteTileTransferRequest>();
    int unloadCalls = 0;
    BabyMetroidCutsceneStepResult fourthTransfer = default;
    while (baby.Phase == BabyMetroidCutscenePhase.UnloadTiles)
    {
        fourthTransfer = baby.Step(bus, samus, motherBrain);
        unloadCalls++;
        if (fourthTransfer.AttackTileTransfer is { } transfer)
            attackTransfers.Add(transfer);
        AssertTrue(unloadCalls < 200, "Baby unload wait reaches all four attack DMAs");
    }
    AssertEqual(132, unloadCalls,
        "unload waits 129 calls then publishes four consecutive transfers");
    AssertEqual(4, attackTransfers.Count, "attack graphics have four DMA records");
    uint[] expectedAttackSources = [0xb7a000, 0xb7a200, 0xb7a400, 0xb7a600];
    ushort[] expectedAttackDestinations = [0x7c00, 0x7d00, 0x7e00, 0x7f00];
    for (int index = 0; index < attackTransfers.Count; index++)
    {
        AssertEqual(expectedAttackSources[index], attackTransfers[index].SourceAddress,
            $"attack DMA {index} source");
        AssertEqual(expectedAttackDestinations[index], attackTransfers[index].VramDestination,
            $"attack DMA {index} destination");
        AssertEqual((ushort)0x0200, attackTransfers[index].Size,
            $"attack DMA {index} size");
    }
    AssertEqual(BabyMetroidCutscenePhase.LetSamusRainbowSomeMore, baby.Phase,
        "fourth attack DMA observes zero terminator");
    AssertEqual((ushort)0x00af, baby.FunctionTimer,
        "fourth attack DMA falls through and decrements new $B0 timer");

    var roomPalettes = new List<MotherBrainBackgroundPaletteTransferRequest>();
    int rainbowDelayCalls = 0;
    BabyMetroidCutsceneStepResult firstRoomPalette = default;
    while (baby.Phase == BabyMetroidCutscenePhase.LetSamusRainbowSomeMore)
    {
        firstRoomPalette = baby.Step(bus, samus, motherBrain);
        rainbowDelayCalls++;
        if (firstRoomPalette.BackgroundPaletteTransfer is { } palette)
            roomPalettes.Add(palette);
    }
    AssertEqual(176, rainbowDelayCalls,
        "post-DMA `$AF..0` wait expires after 176 additional calls");
    AssertEqual(BabyMetroidCutscenePhase.FinalCutscene, baby.Phase,
        "rainbow delay falls through into final room-light function");
    AssertEqual(1, roomPalettes.Count,
        "final-cutscene fallthrough publishes room palette zero immediately");

    int finalPaletteCalls = 1;
    while (!baby.IsDeleted)
    {
        BabyMetroidCutsceneStepResult finalPalette = baby.Step(bus, samus, motherBrain);
        finalPaletteCalls++;
        if (finalPalette.BackgroundPaletteTransfer is { } palette)
            roomPalettes.Add(palette);
        AssertTrue(finalPaletteCalls < 20, "phase-three light table reaches zero terminator");
    }
    AssertEqual(8, finalPaletteCalls,
        "room-light table publishes seven palettes then completes on entry seven");
    AssertEqual(7, roomPalettes.Count, "phase-three light restore has seven records");
    for (int index = 0; index < roomPalettes.Count; index++)
    {
        AssertEqual((ushort)index, roomPalettes[index].PaletteIndex,
            $"room-light palette {index} index");
        AssertEqual(unchecked((uint)(0xadf3d3 - index * 0x38)), roomPalettes[index].SourceAddress,
            $"room-light palette {index} reverse source");
    }
    AssertTrue(!samus.Drained.RainbowPaletteEnabled,
        "final cutscene executes command $17 rainbow disable");
    AssertEqual((ushort)13, samus.AnimationFrame,
        "command $17 starts drained Samus standing animation at frame 13");
    AssertEqual((ushort)0x8000, samus.HyperBeam,
        "drained controller three grants Hyper Beam");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.Phase3RecoverFromCutsceneMakeSomeDistance,
        motherBrain.Phase,
        "Baby deletion installs Mother Brain `$C1CF` for following actor call");

    MotherBrainRainbowBeamAttackStepResult recovery = motherBrain.Step(bus, samus, 0, 0);
    AssertEqual(MotherBrainRainbowBeamAttackPhase.Phase3RecoverFromCutsceneSetupForFighting,
        recovery.PhaseAfter,
        "phase-three recovery publishes form and setup timer");
    AssertEqual((ushort)4, motherBrain.Body.Form, "phase-three recovery sets body form four");
    AssertEqual((ushort)0x0020, motherBrain.FunctionTimer,
        "phase-three recovery loads literal $20 setup wait");
    int setupCalls = 0;
    while (motherBrain.Phase ==
           MotherBrainRainbowBeamAttackPhase.Phase3RecoverFromCutsceneSetupForFighting)
    {
        motherBrain.Step(bus, samus, 0, 0);
        setupCalls++;
    }
    AssertEqual(33, setupCalls, "phase-three `$20` wait expires on wrapped call 33");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.Phase3FightingMain, motherBrain.Phase,
        "recovery reaches genuine `$C209` phase-three combat seam");

    // Exercise the new combat scheduler with its real body bytecode rather than changing
    // X directly. `$C1F0` falls into main on setup call 33, so that same call must initialize
    // the neck and request the first fourteen-pixel retreat.
    var phase3Samus = new SamusState { XPosition = 224, YPosition = 120 };
    var phase3 = new MotherBrainRainbowBeamAttackSequence();
    phase3.Body.XPosition = 0x0070;
    phase3.Body.YPosition = 0x0064;
    phase3.BeginPhase3RecoveryFromBabyCutscene();
    phase3.Step(bus, phase3Samus, 0, 0);
    for (int call = 0; call < 33; call++)
        phase3.Step(bus, phase3Samus, 0, 0);
    AssertEqual(MotherBrainRainbowBeamAttackPhase.Phase3FightingMain, phase3.Phase,
        "phase-three fixture reaches main after exact setup wait");
    AssertEqual(MotherBrainPhase3NeckPhase.Inactive, phase3.Phase3NeckPhase,
        "same-call normal neck initializer reduces to its RTS state");
    AssertEqual((ushort)0x0080, phase3.NeckAngleDelta,
        "normal neck initializer selects delta $80");
    AssertEqual(MotherBrainPhase3WalkingPhase.RetreatQuickly, phase3.Phase3WalkingPhase,
        "zero walk credit falls through to quick retreat");
    AssertEqual((ushort)0x0062, phase3.Phase3TargetXPosition,
        "quick retreat target is body X minus fourteen");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.BodyWalkingBackwardReallyFastInstructionList,
        phase3.Body.InstructionPointer,
        "quick retreat selects delay-index-two body bytecode");

    int quickWalkCalls = 0;
    while (!phase3.Body.Sleeping)
    {
        phase3.Body.Step(bus);
        quickWalkCalls++;
        AssertTrue(quickWalkCalls < 40, "phase-three quick retreat bytecode sleeps");
    }
    AssertEqual((ushort)0x0058, phase3.Body.XPosition,
        "quick retreat completes its native net-minus-24 animation");
    MotherBrainRainbowBeamAttackStepResult quickReached =
        phase3.Step(bus, phase3Samus, 0, 0);
    AssertTrue(!quickReached.BodyWalkRequested,
        "quick-retreat target call changes scheduler without same-call slow request");
    AssertEqual(MotherBrainPhase3WalkingPhase.RetreatSlowly, phase3.Phase3WalkingPhase,
        "quick retreat hands off to slow retreat");
    AssertEqual((ushort)0x004a, phase3.Phase3TargetXPosition,
        "slow retreat chooses a fresh fourteen-pixel target");

    MotherBrainRainbowBeamAttackStepResult slowRequest =
        phase3.Step(bus, phase3Samus, 0, 0);
    AssertTrue(slowRequest.BodyWalkRequested, "following call requests slow retreat");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.BodyWalkingBackwardFastInstructionList,
        phase3.Body.InstructionPointer,
        "slow retreat selects delay-index-four body bytecode");
    int slowWalkCalls = 0;
    while (!phase3.Body.Sleeping)
    {
        phase3.Body.Step(bus);
        slowWalkCalls++;
        AssertTrue(slowWalkCalls < 80, "phase-three slow retreat bytecode sleeps");
    }
    AssertEqual((ushort)0x0040, phase3.Body.XPosition,
        "slow retreat completes another native net-minus-24 animation");
    phase3.Step(bus, phase3Samus, 0, 0);
    AssertEqual(MotherBrainPhase3WalkingPhase.TryToInchForward, phase3.Phase3WalkingPhase,
        "slow-retreat target restores inch-forward scheduler");
    AssertEqual((ushort)0x0040, phase3.Phase3WalkCounter,
        "slow-retreat completion loads literal $40 walk credit");
    AssertEqual((ushort)0x0041, phase3.Phase3TargetXPosition,
        "inch-forward handoff records current X plus one");

    // Five calls reach `$E0`; the sixth reaches `$100` and emits a one-pixel forward walk.
    for (int call = 0; call < 5; call++)
    {
        MotherBrainRainbowBeamAttackStepResult accumulating =
            phase3.Step(bus, phase3Samus, 0, 0);
        AssertTrue(!accumulating.BodyWalkRequested,
            $"walk-credit accumulation call {call} does not move before $100");
    }
    AssertEqual((ushort)0x00e0, phase3.Phase3WalkCounter,
        "five standing calls accumulate walk credit to $E0");
    MotherBrainRainbowBeamAttackStepResult inch = phase3.Step(bus, phase3Samus, 0, 0);
    AssertTrue(inch.BodyWalkRequested, "$100 walk credit requests one-pixel forward target");
    AssertEqual((ushort)0x0100, phase3.Phase3WalkCounter,
        "inch-forward request preserves accumulated credit");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.BodyWalkingForwardFastInstructionList,
        phase3.Body.InstructionPointer,
        "RNG bit one clear chooses phase-three delay index four");

    // Begin the visible body instruction before applying a Hyper Beam reaction. This keeps
    // the next AI call in native order: neck recoil still runs, walking is pose-gated, and
    // the negative `$0100-$010A` subtraction clamps the walk counter to zero.
    phase3.Body.Step(bus);
    AssertEqual((ushort)1, phase3.Body.Pose, "forward bytecode publishes walking pose");
    phase3.ApplyPhase2Or3ShotReaction(MotherBrainProjectileType.Beam);
    AssertEqual((ushort)0, phase3.Phase3WalkCounter,
        "Hyper Beam underflow clamps walk counter to zero");
    AssertEqual(MotherBrainPhase3NeckPhase.SetupHyperBeamRecoil, phase3.Phase3NeckPhase,
        "Hyper Beam underflow installs recoil setup");
    MotherBrainRainbowBeamAttackStepResult recoilSetup =
        phase3.Step(bus, phase3Samus, 0, 0, randomNumberSeed: 0xffff);
    AssertEqual(MotherBrainPhase3NeckPhase.HyperBeamRecoil, phase3.Phase3NeckPhase,
        "recoil setup falls into recoil timer");
    AssertEqual((ushort)0x000a, phase3.Phase3NeckFunctionTimer,
        "recoil setup decrements freshly loaded $0B to $0A");
    AssertEqual((ushort)1, phase3.Phase3DisableAttacks,
        "Hyper Beam recoil disables attack selection");
    AssertEqual<MotherBrainPhase3AttackKind?>(null, recoilSetup.Phase3Attack,
        "negative RNG cannot attack while recoil disable is set");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.HeadHyperBeamRecoilInstructionList,
        phase3.HeadInstructionList,
        "recoil installs exact `$9BE7` head animation");
    AssertEqual((ushort)0x0900, phase3.NeckAngleDelta, "Hyper Beam recoil neck delta");
    AssertEqual((ushort)8, phase3.LowerNeckMovementIndex, "Hyper Beam lower recoil index");
    AssertEqual((ushort)8, phase3.UpperNeckMovementIndex, "Hyper Beam upper recoil index");
    AssertEqual((ushort)0x0032, phase3.BrainMainShakeTimer,
        "Hyper Beam recoil seeds brain shake timer fifty");

    int recoilTimerCalls = 0;
    while (phase3.Phase3NeckPhase == MotherBrainPhase3NeckPhase.HyperBeamRecoil)
    {
        phase3.Step(bus, phase3Samus, 0, 0);
        recoilTimerCalls++;
    }
    AssertEqual(11, recoilTimerCalls,
        "remaining `$0A` recoil timer expires only after zero underflows");
    AssertEqual(MotherBrainPhase3NeckPhase.SetupRecoilRecovery, phase3.Phase3NeckPhase,
        "recoil expiration defers recovery setup to following call");
    AssertEqual((ushort)0, phase3.Phase3DisableAttacks,
        "recoil expiration reenables attacks before recovery setup");

    phase3.Step(bus, phase3Samus, 0, 0);
    AssertEqual(MotherBrainPhase3NeckPhase.RecoilRecovery, phase3.Phase3NeckPhase,
        "recovery setup falls through into its timer");
    AssertEqual((ushort)0x000f, phase3.Phase3NeckFunctionTimer,
        "recovery setup decrements freshly loaded $10 to $0F");
    int recoveryTimerCalls = 0;
    while (phase3.Phase3NeckPhase == MotherBrainPhase3NeckPhase.RecoilRecovery)
    {
        phase3.Step(bus, phase3Samus, 0, 0);
        recoveryTimerCalls++;
    }
    AssertEqual(16, recoveryTimerCalls,
        "remaining `$0F` recovery timer expires only after zero underflows");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.HeadAttackingFourOnionRingsPhase3InstructionList,
        phase3.HeadInstructionList,
        "recovery expiration installs phase-three four-ring head list");
    AssertEqual(MotherBrainPhase3NeckPhase.Normal, phase3.Phase3NeckPhase,
        "recovery expiration defers normal-neck initializer");
    phase3.Step(bus, phase3Samus, 0, 0);
    AssertEqual(MotherBrainPhase3NeckPhase.Inactive, phase3.Phase3NeckPhase,
        "following call consumes one-shot normal-neck initializer");

    // Attack selection is independent of the movement request produced earlier in the same
    // AI call. A fresh fixture stays standing because this direct test intentionally does
    // not advance its newly requested body list until after observing both producers.
    var phase3Attack = new MotherBrainRainbowBeamAttackSequence();
    phase3Attack.Body.XPosition = 0x0070;
    phase3Attack.BeginPhase3RecoveryFromBabyCutscene();
    phase3Attack.Step(bus, phase3Samus, 0, 0);
    for (int call = 0; call < 32; call++)
        phase3Attack.Step(bus, phase3Samus, 0, 0);
    MotherBrainRainbowBeamAttackStepResult bombSelection =
        phase3Attack.Step(bus, phase3Samus, 0, 0, randomNumberSeed: 0x8000);
    AssertTrue(bombSelection.BodyWalkRequested,
        "phase-three setup fallthrough can request movement and attack together");
    AssertEqual(MotherBrainPhase3AttackKind.Bomb, bombSelection.Phase3Attack,
        "negative RNG with low byte below $80 selects bomb");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.HeadAttackingBombPhase3InstructionList,
        phase3Attack.HeadInstructionList, "phase-three bomb installs exact `$9F00` list");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.Phase3FightingAttackCooldown,
        phase3Attack.Phase, "phase-three attack installs cooldown function");
    AssertEqual((ushort)0x0040, phase3Attack.FunctionTimer,
        "phase-three attack cooldown starts at literal $40");

    int cooldownCalls = 0;
    while (phase3Attack.Phase ==
           MotherBrainRainbowBeamAttackPhase.Phase3FightingAttackCooldown)
    {
        phase3Attack.Step(bus, phase3Samus, 0, 0);
        cooldownCalls++;
    }
    AssertEqual(65, cooldownCalls, "$40 attack cooldown expires on underflow call 65");
    MotherBrainRainbowBeamAttackStepResult ringsSelection =
        phase3Attack.Step(bus, phase3Samus, 0, 0, randomNumberSeed: 0x8080);
    AssertEqual(MotherBrainPhase3AttackKind.FourOnionRings, ringsSelection.Phase3Attack,
        "negative RNG with low byte exactly $80 selects four rings");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.HeadAttackingFourOnionRingsPhase3InstructionList,
        phase3Attack.HeadInstructionList, "phase-three rings install exact `$9DBB` list");

    // Drive a separate healthy phase-three actor into zero health through the public generic-
    // damage boundary. The combat function only installs `$AEE1`; it must not perform any of
    // the death function's property writes or movement on that same AI call.
    var death = new MotherBrainRainbowBeamAttackSequence
    {
        BrainXPosition = 0x0050,
        BrainYPosition = 0x0060,
    };
    death.Body.XPosition = 0x0040;
    death.Body.YPosition = 0x0064;
    death.InitializeCorpseRotting(bus);

    // Head initialization creates entries `(47,0)` through `(0,94)` in native WRAM.
    // Check every entry, not merely the endpoints, because one reversed loop direction
    // changes both the visible dissolve order and its 118-call completion time.
    for (int entryIndex = 0; entryIndex < MotherBrainCorpseRottingState.EntryCount; entryIndex++)
    {
        AssertEqual(
            new MotherBrainCorpseRotEntry(
                YOffset: unchecked((short)(47 - entryIndex)),
                Timer: unchecked((ushort)(entryIndex * 2))),
            death.CorpseRotting.ReadEntry(bus, entryIndex),
            $"corpse rot-table entry {entryIndex}");
    }

    // Verify all copied bytes plus the deliberately untouched `$20`-byte gaps in the first
    // four `$E0`-byte rows. This catches the tempting but wrong six-uniform-row extraction.
    for (int row = 0; row < 6; row++)
    {
        int copiedLength = row < 4 ? 0x00c0 : 0x00e0;
        int sourceOffset = 0x00c0 + row * 0x0200;
        int destinationOffset = row * 0x00e0;
        for (int byteIndex = 0; byteIndex < copiedLength; byteIndex++)
        {
            AssertEqual(
                bus.ReadByte(0xb7ce00 + sourceOffset + byteIndex),
                bus.ReadByte(
                    MotherBrainCorpseRottingState.GraphicsBufferAddress +
                    destinationOffset + byteIndex),
                $"corpse graphics extraction row {row} byte ${byteIndex:X2}");
        }
        for (int byteIndex = copiedLength; byteIndex < 0x00e0; byteIndex++)
        {
            AssertEqual(
                (byte)0,
                bus.ReadByte(
                    MotherBrainCorpseRottingState.GraphicsBufferAddress +
                    destinationOffset + byteIndex),
                $"corpse graphics untouched gap row {row} byte ${byteIndex:X2}");
        }
    }
    death.BeginPhase3RecoveryFromBabyCutscene();
    death.Step(bus, phase3Samus, 0, 0);
    for (int call = 0; call < 33; call++)
        death.Step(bus, phase3Samus, 0, 0);
    death.ApplyCalculatedBrainDamage(0x0bb8);
    MotherBrainRainbowBeamAttackStepResult deathHandoff =
        death.Step(bus, phase3Samus, 0, 0);
    AssertEqual((ushort)0, death.BrainHealth, "calculated damage saturates brain health at zero");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceMoveToBackOfRoom,
        deathHandoff.PhaseAfter, "zero-health combat call installs `$AEE1` without fallthrough");
    AssertTrue(death.HitboxesEnabled,
        "death handoff defers `$AEE1` hitbox clear to following actor call");

    var deathRandom = new Bank80SystemState(0x0061);
    MotherBrainRainbowBeamAttackStepResult firstDeathMove = death.Step(
        bus, phase3Samus, 0, 0, nextRandomNumber: deathRandom.NextRandom);
    AssertTrue(firstDeathMove.BodyWalkRequested, "death requests medium backward body list");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.BodyWalkingBackwardMediumInstructionList,
        death.Body.InstructionPointer, "death retreat selects exact `$9900` list");
    AssertEqual((ushort)0x0400, death.BodyProperties, "death sets raw body property `$0400`");
    AssertEqual((ushort)0x0400, death.BrainProperties, "death sets raw brain property `$0400`");
    AssertTrue(!death.HitboxesEnabled, "death entry disables shared hitboxes");

    // The medium backward program has the same net -24 motion as every admitted walk list.
    // Run its real command stream to sleeping rather than assigning the `$28` destination.
    int deathRetreatBodyCalls = 0;
    while (!death.Body.Sleeping)
    {
        death.Body.Step(bus);
        deathRetreatBodyCalls++;
        AssertTrue(deathRetreatBodyCalls < 100, "death retreat body bytecode sleeps");
    }
    AssertEqual((ushort)0x0028, death.Body.XPosition, "death retreat reaches back-room X `$28`");

    MotherBrainRainbowBeamAttackStepResult firstSmokyBatch = death.Step(
        bus, phase3Samus, 0, 0, nextRandomNumber: deathRandom.NextRandom);
    AssertEqual(MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceIdleWhilstExploding,
        death.Phase, "back-room carry falls through into smoky idle");
    AssertEqual((ushort)0x007f, death.FunctionTimer,
        "same-call smoky idle decrements freshly loaded `$80`");
    AssertEqual((ushort)0x0010, death.DeathExplosionIntervalTimer,
        "zero death-explosion timer emits immediately and reloads smoky interval `$10`");
    AssertEqual((ushort)6, death.DeathExplosionIndex,
        "zero death-explosion index wraps backward to record six");
    AssertEqual(2, firstSmokyBatch.DeathExplosions.Count,
        "smoky generator emits two simultaneous projectiles");
    AssertEqual(new MotherBrainDeathExplosionRequest(
            PatternIndex: 6,
            XOffset: 0x000a,
            YOffset: -0x001f,
            XPosition: 0x0032,
            YPosition: 0x0045,
            ProjectileParameter: 1,
            SoundEffect: 0x0013),
        firstSmokyBatch.DeathExplosions[0],
        "first smoky projectile uses record-six pair zero and smoke parameter");
    AssertEqual((short)-0x0014, firstSmokyBatch.DeathExplosions[1].XOffset,
        "second smoky projectile advances to record-six pair one");

    int smokyIdleCalls = 0;
    while (death.Phase == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceIdleWhilstExploding)
    {
        death.Step(bus, phase3Samus, 0, 0, nextRandomNumber: deathRandom.NextRandom);
        smokyIdleCalls++;
        AssertTrue(smokyIdleCalls < 200, "smoky idle timer reaches stumble");
    }
    AssertEqual(128, smokyIdleCalls,
        "remaining `$7F` smoky-idle timer expires only after zero underflows");

    // `$AF21` can require multiple complete forward programs because the carry test uses
    // strict signed overshoot. Advance the actor and instruction stages in native order.
    int stumbleCalls = 0;
    while (death.Phase == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceStumbleToMiddleOfRoom)
    {
        death.Step(bus, phase3Samus, 0, 0, nextRandomNumber: deathRandom.NextRandom);
        death.Body.Step(bus);
        stumbleCalls++;
        AssertTrue(stumbleCalls < 200, "death stumble reaches middle-room target");
    }
    AssertEqual((ushort)0x006d, death.Body.XPosition,
        "third really-fast program reports carry on its mid-list +15px crossing");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.HeadDyingDroolInstructionList,
        death.HeadInstructionList, "stumble completion installs dying-drool head list");
    AssertEqual((ushort)0x0020, death.FunctionTimer,
        "stumble completion loads brain-effects delay `$20`");

    int disableEffectsCalls = 0;
    MotherBrainRainbowBeamAttackStepResult disableResult = default;
    while (death.Phase == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceDisableBrainEffects)
    {
        disableResult = death.Step(
            bus, phase3Samus, 0, 0, nextRandomNumber: deathRandom.NextRandom);
        // Enemy instruction processing remains independent after body AI changes function;
        // finish the still-visible third walk exactly as the following native slot stage does.
        death.Body.Step(bus);
        disableEffectsCalls++;
        AssertTrue(disableEffectsCalls < 50, "brain-effects timer reaches body fade");
    }
    AssertEqual(33, disableEffectsCalls, "brain-effects `$20` timer expires on call 33");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceFadeOutBody,
        death.Phase, "disable/setup functions fall through into first body-fade call");
    AssertEqual((ushort)1, death.GreyTransitionCounter,
        "same-call fade performs black palette record zero");
    AssertEqual((ushort)0x0010, death.FunctionTimer,
        "same-call fade reloads palette cadence `$10`");
    AssertTrue(disableResult.PaletteRequested,
        "disable/fade fallthrough exposes palette-copy work");
    AssertTrue(!death.DroolGenerationEnabled && !death.SmallPurpleBreathGenerationEnabled &&
               !death.BrainPaletteHandlingEnabled && !death.HealthBasedPaletteHandlingEnabled,
        "death disables all four brain-effect producers");
    AssertEqual((ushort)0x0e00, death.BrainPaletteIndex,
        "death forces sprite palette-seven index `$0E00`");
    AssertEqual((ushort)0x0070, death.Body.XPosition,
        "in-flight third walk finishes to `$70` during brain-effects delay");

    int fadeBodyCalls = 0;
    while (death.Phase == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceFadeOutBody)
    {
        death.Step(bus, phase3Samus, 0, 0, nextRandomNumber: deathRandom.NextRandom);
        fadeBodyCalls++;
        AssertTrue(fadeBodyCalls < 350, "body palette reaches black terminator");
    }
    AssertEqual(272, fadeBodyCalls,
        "remaining sixteen black-table probes use exact seventeen-call cadence");
    AssertEqual((ushort)17, death.GreyTransitionCounter,
        "black fade consumes sixteen records plus null entry");
    AssertTrue(death.EnemyBg2TilemapClearRequested,
        "black terminator requests native `$02C6..0` BG2 clear");
    AssertEqual((ushort)0x0500, death.BodyProperties,
        "black terminator preserves `$0400`, sets `$0100`, and clears `$2000`");

    int finalExplosionCalls = 0;
    while (death.Phase == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceFinalFewExplosions)
    {
        death.Step(bus, phase3Samus, 0, 0, nextRandomNumber: deathRandom.NextRandom);
        finalExplosionCalls++;
    }
    AssertEqual(17, finalExplosionCalls,
        "final-explosion `$10` timer expires only after zero underflows");

    MotherBrainRainbowBeamAttackStepResult firstFall = death.Step(
        bus, phase3Samus, 0, 0, nextRandomNumber: deathRandom.NextRandom);
    AssertEqual(MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceBrainFallsToGround,
        firstFall.PhaseAfter, "decapitation falls through into brain motion");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.HeadDecapitatedInstructionList,
        death.HeadInstructionList, "decapitation installs exact `$9C29` head list");
    AssertTrue(death.BrainDrawSetupRequested,
        "decapitation publishes separate brain draw-setup request");
    AssertEqual((ushort)0x0020, death.FunctionTimer,
        "first 8.8 falling call accelerates from zero to `$0020`");
    AssertEqual((ushort)0x0060, death.BrainYPosition,
        "first falling velocity has zero whole-pixel displacement");

    int remainingFallCalls = 0;
    while (death.Phase == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceBrainFallsToGround)
    {
        death.Step(bus, phase3Samus, 0, 0, nextRandomNumber: deathRandom.NextRandom);
        remainingFallCalls++;
        AssertTrue(remainingFallCalls < 100, "8.8 brain fall reaches floor");
    }
    AssertEqual(42, remainingFallCalls,
        "brain reaches `$C4` after 43 total 8.8 integration calls");
    AssertEqual((ushort)0x00c4, death.BrainYPosition, "brain fall clamps to floor Y `$C4`");
    AssertEqual((ushort)2, death.EarthquakeType, "brain floor hit requests earthquake type two");
    AssertEqual((ushort)20, death.EarthquakeTimer, "brain floor hit requests twenty frames");
    AssertEqual((ushort)0x0100, death.FunctionTimer,
        "brain floor hit seeds corpse-load scratch timer `$0100`");

    var corpseTransfers = new List<MotherBrainSpriteTileTransferRequest>();
    while (death.Phase == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceLoadCorpseTiles)
    {
        MotherBrainRainbowBeamAttackStepResult transfer = death.Step(
            bus, phase3Samus, 0, 0, nextRandomNumber: deathRandom.NextRandom);
        AssertTrue(transfer.SpriteTileTransfer is not null, "corpse-load call emits one DMA record");
        corpseTransfers.Add(transfer.SpriteTileTransfer!.Value);
    }
    AssertEqual(6, corpseTransfers.Count, "corpse transfer list has six records");
    for (int index = 0; index < corpseTransfers.Count; index++)
    {
        AssertEqual(unchecked((uint)(0xb7ce00 + index * 0x200)),
            corpseTransfers[index].SourceAddress, $"corpse DMA {index} source");
        AssertEqual(unchecked((ushort)(0x7a00 + index * 0x100)),
            corpseTransfers[index].VramDestination, $"corpse DMA {index} destination");
        AssertEqual((ushort)0x01c0, corpseTransfers[index].Size,
            $"corpse DMA {index} skips two source rows");
    }
    AssertEqual(MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceSetupFadeToGrey,
        death.Phase, "sixth corpse DMA observes terminator on same call");

    int greySetupCalls = 0;
    while (death.Phase == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceSetupFadeToGrey)
    {
        death.Step(bus, phase3Samus, 0, 0, nextRandomNumber: deathRandom.NextRandom);
        greySetupCalls++;
    }
    AssertEqual(33, greySetupCalls, "corpse-grey setup `$20` expires on call 33");
    int greyFadeCalls = 0;
    int greyPaletteCopies = 0;
    while (death.Phase == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceFadeToGrey)
    {
        MotherBrainRainbowBeamAttackStepResult grey = death.Step(
            bus, phase3Samus, 0, 0, nextRandomNumber: deathRandom.NextRandom);
        if (grey.PaletteRequested)
            greyPaletteCopies++;
        greyFadeCalls++;
        AssertTrue(greyFadeCalls < 180, "corpse-grey table reaches terminator");
    }
    AssertEqual(137, greyFadeCalls, "eight grey records and terminator use native cadence");
    AssertEqual(8, greyPaletteCopies, "real-death grey transition copies eight palettes");
    AssertEqual(MotherBrainRainbowBeamAttackSequence.HeadCorpseInstructionList,
        death.HeadInstructionList, "grey terminator installs exact `$9D25` corpse list");
    AssertEqual((ushort)0x0100, death.FunctionTimer, "corpse tip-over delay starts at `$100`");

    int corpseTipCalls = 0;
    while (death.Phase == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceCorpseTipsOver)
    {
        death.Step(bus, phase3Samus, 0, 0, nextRandomNumber: deathRandom.NextRandom);
        corpseTipCalls++;
    }
    AssertEqual(257, corpseTipCalls, "corpse `$100` display delay expires on call 257");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceCorpseRotsAway,
        death.Phase, "tip-over reaches shared corpse-rotting engine");

    // `$DB12` processes all 48 entries on every call. The staggered timers finish exactly
    // one entry on each of 48 irregularly spaced calls; carry stays set for the first 117
    // calls and the final entry returns carry clear on call 118 without queuing VRAM work.
    int corpseRottingCalls = 0;
    int corpseDustCount = 0;
    MotherBrainCorpseDustRequest? firstCorpseDust = null;
    MotherBrainCorpseDustRequest? lastCorpseDust = null;
    MotherBrainRainbowBeamAttackStepResult corpseFinished = default;
    while (death.Phase == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceCorpseRotsAway)
    {
        corpseRottingCalls++;
        MotherBrainRainbowBeamAttackStepResult rot = death.Step(
            bus,
            phase3Samus,
            enemyFrameCounter: 0,
            mainEnemyExecutionCounter: unchecked((ushort)(corpseRottingCalls - 1)),
            randomNumberSeed: 0x1234,
            nextRandomNumber: deathRandom.NextRandom);

        foreach (MotherBrainCorpseDustRequest dust in rot.CorpseDustRequests)
        {
            firstCorpseDust ??= dust;
            lastCorpseDust = dust;
            corpseDustCount++;
        }

        if (corpseRottingCalls < 118)
        {
            AssertEqual(6, rot.CorpseRottingVramTransfers.Count,
                $"active corpse rot call {corpseRottingCalls} queues all six DMA records");
            ushort[] sizes = [0x0060, 0x00a0, 0x00c0, 0x00c0, 0x00e0, 0x00e0];
            uint[] sources = [0x7e9040, 0x7e9100, 0x7e91c0, 0x7e92a0, 0x7e9380, 0x7e9460];
            ushort[] destinations = [0x7a80, 0x7b70, 0x7c60, 0x7d60, 0x7e60, 0x7f60];
            for (int transferIndex = 0; transferIndex < 6; transferIndex++)
            {
                AssertEqual(
                    new MotherBrainSpriteTileTransferRequest(
                        unchecked((ushort)transferIndex),
                        sizes[transferIndex],
                        sources[transferIndex],
                        destinations[transferIndex]),
                    rot.CorpseRottingVramTransfers[transferIndex],
                    $"corpse rot DMA {transferIndex} on call {corpseRottingCalls}");
            }
            AssertTrue(!rot.MusicStopQueued && !rot.EscapeMusicQueued,
                "active corpse rot does not queue escape music early");
        }
        else
        {
            corpseFinished = rot;
            AssertEqual(0, rot.CorpseRottingVramTransfers.Count,
                "carry-clear corpse completion skips `$E1F4` VRAM records");
        }

        AssertTrue(corpseRottingCalls <= 118, "corpse rotting reaches final table entry");
    }
    AssertEqual(118, corpseRottingCalls, "48 staggered corpse rows finish on exact call 118");
    AssertEqual((uint)118, death.CorpseRotting.ProcessCallCount,
        "corpse processor publishes native call count");
    AssertEqual(48, corpseDustCount, "every rot entry runs Mother Brain's dust hook once");
    AssertEqual((uint)48, death.CorpseRotting.FinishedEntryCount,
        "corpse processor publishes all finished entries");
    AssertEqual(
        new MotherBrainCorpseDustRequest(0, 0x0054, 0x00d4, 0x000a, true, 0x0010),
        firstCorpseDust!.Value,
        "entry zero finishes on call one using sampled RNG and execution-counter SFX gate");
    AssertEqual(
        new MotherBrainCorpseDustRequest(47, 0x0054, 0x00d4, 0x000a, false, 0x0010),
        lastCorpseDust!.Value,
        "entry 47 finishes last without advancing sampled RNG");
    AssertTrue(corpseFinished.MusicStopQueued && corpseFinished.EscapeMusicQueued,
        "rotting completion queues `$0000` then `$FF24`");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.Phase3DeathSequence20FrameDelay,
        death.Phase, "rotting completion installs `$B211` delay function");
    AssertEqual((ushort)0x0013, death.FunctionTimer,
        "completion falls through and decrements freshly loaded `$14`");
    AssertEqual((ushort)0x0500, death.BrainProperties,
        "completion preserves `$0400`, sets `$0100`, and clears `$2000`");
    AssertEqual((ushort)0, death.BrainProperties2,
        "completion clears brain property word two");

    // All eligible bitplane rows must eventually be moved out and cleared. Check the entire
    // `$540`-byte staging buffer so a missing column gate or one-plane clear cannot hide.
    for (int byteOffset = 0; byteOffset < MotherBrainCorpseRottingState.GraphicsBufferSize; byteOffset++)
    {
        AssertEqual(
            (byte)0,
            bus.ReadByte(MotherBrainCorpseRottingState.GraphicsBufferAddress + byteOffset),
            $"fully rotted corpse graphics byte ${byteOffset:X3}");
    }

    int postRotDelayCalls = 0;
    while (death.Phase == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequence20FrameDelay)
    {
        death.Step(bus, phase3Samus, 0, 0);
        postRotDelayCalls++;
        AssertTrue(postRotDelayCalls <= 20, "post-rot `$14` delay reaches escape tile load");
    }
    AssertEqual(20, postRotDelayCalls,
        "same-call first decrement leaves exactly twenty later delay calls");
    AssertEqual((ushort)0, death.BrainXPosition, "post-rot delay parks brain X at zero");
    AssertEqual((ushort)0, death.BrainYPosition, "post-rot delay parks brain Y at zero");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceLoadEscapeTimerTiles,
        death.Phase, "post-rot delay reaches explicit escape-timer tile seam");

    MotherBrainSpriteTileTransferRequest[] expectedEscapeTimerTransfers =
    [
        new(0, 0x0200, 0xb0c000, 0x7e00),
        new(1, 0x0120, 0xb0c200, 0x7f00),
        new(2, 0x0200, 0xb7da00, 0x7820),
        new(3, 0x0200, 0xb7dc00, 0x7920),
        new(4, 0x0200, 0xb7de00, 0x7a20),
        new(5, 0x0200, 0xb7e000, 0x7b20),
        new(6, 0x0100, 0xb7e200, 0x7c20),
    ];
    for (int transferIndex = 0; transferIndex < expectedEscapeTimerTransfers.Length; transferIndex++)
    {
        MotherBrainRainbowBeamAttackStepResult escapeTiles =
            death.Step(bus, phase3Samus, 0, 0);
        AssertEqual(expectedEscapeTimerTransfers[transferIndex],
            escapeTiles.EscapeSequenceTileTransfers[0],
            $"escape timer tile DMA {transferIndex}");
        if (transferIndex < expectedEscapeTimerTransfers.Length - 1)
        {
            AssertEqual(1, escapeTiles.EscapeSequenceTileTransfers.Count,
                "nonfinal escape-timer call emits exactly one record");
        }
        else
        {
            // `$B26A` falls through: final timer text and first exploded-door page share
            // one AI call even though both use the same global transfer-list cursor.
            AssertEqual(2, escapeTiles.EscapeSequenceTileTransfers.Count,
                "final escape-timer call also emits first exploded-door record");
            AssertEqual(new MotherBrainSpriteTileTransferRequest(0, 0x0200, 0xabf400, 0x7000),
                escapeTiles.EscapeSequenceTileTransfers[1],
                "same-call first exploded-door DMA");
        }
    }
    AssertEqual((ushort)7, death.EscapeTimerTileTransferIndex,
        "escape-timer list consumes seven NTSC records");
    AssertEqual((ushort)1, death.ExplodedDoorTileTransferIndex,
        "escape-timer fallthrough consumes exploded-door record zero");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceStartEscape,
        death.Phase, "first door record leaves `$B26D` active");

    MotherBrainRainbowBeamAttackStepResult escapeStarted =
        death.Step(bus, phase3Samus, 0, 0);
    AssertEqual(1, escapeStarted.EscapeSequenceTileTransfers.Count,
        "second start-escape call emits one door record");
    AssertEqual(new MotherBrainSpriteTileTransferRequest(1, 0x0200, 0xabf600, 0x7100),
        escapeStarted.EscapeSequenceTileTransfers[0],
        "second exploded-door DMA");
    AssertTrue(escapeStarted.ExplodedDoorPaletteRequested,
        "door-list terminator requests fourteen-color exploded-door palette copy");
    AssertTrue(escapeStarted.EscapeMusicTrackQueued,
        "door-list terminator queues escape music track seven");
    AssertEqual((ushort)5, death.EarthquakeType, "escape start selects earthquake type five");
    AssertEqual((ushort)0xffff, death.EarthquakeTimer,
        "escape start holds earthquake with `$FFFF`");
    AssertEqual(4, escapeStarted.EscapePaletteFxRequests.Count,
        "escape start spawns all four Tourian red-flash palette objects");
    AssertEqual((ushort)0xffc9, escapeStarted.EscapePaletteFxRequests[0],
        "escape palette FX begins with shutter-red object");
    AssertEqual((ushort)0xffd5, escapeStarted.EscapePaletteFxRequests[3],
        "escape palette FX ends with Arkanoid/red-orb object");
    AssertTrue(!death.MotherBrainUnpauseHookEnabled,
        "escape typewriter disables Mother Brain unpause hook");
    AssertTrue(escapeStarted.EscapeTypewriterSetupRequested,
        "escape start requests native Zebes typewriter setup");
    AssertEqual((ushort)0x0020, death.FunctionTimer,
        "escape text handoff loads `$20` subtitle/typewriter timer");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceTypeOutZebesEscapeText,
        death.Phase, "default NTSC text selection reaches explicit `$B2E3` seam");

    MotherBrainRainbowBeamAttackStepResult typing = death.Step(bus, phase3Samus, 0, 0);
    AssertTrue(typing.TypewriterStepRequested,
        "`$B2E3` requests one external typewriter step on every call");
    AssertEqual<ushort?>(0x2610, typing.TypewriterTextPointer,
        "`$B2E3` publishes exact Zebes escape text-list pointer");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceTypeOutZebesEscapeText,
        death.Phase, "clear typewriter carry keeps `$B2E3` active");

    MotherBrainRainbowBeamAttackStepResult typewriterComplete = death.Step(
        bus,
        phase3Samus,
        0,
        0,
        typewriterFinished: true);
    AssertTrue(typewriterComplete.TypewriterStepRequested,
        "completion call still records the `$2610` typewriter invocation");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceDoorExplodingStartTimer,
        death.Phase, "typewriter carry installs door-explosion countdown");
    AssertEqual((ushort)0x0020, death.FunctionTimer,
        "typewriter completion reloads exact `$20` door timer");

    // `$B346` advances global RNG only when its shared interval underflows. Alternate the
    // two sides of the literal `$4000` comparison, then verify the four-position descending
    // cycle independently from however much of the earlier death interval remained.
    int escapeRngCalls = 0;
    int doorTimerCalls = 0;
    var emittedDoorExplosions = new List<MotherBrainEscapeDoorExplosionRequest>();
    MotherBrainRainbowBeamAttackStepResult doorTimerResult = default;
    while (death.Phase == MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceDoorExplodingStartTimer)
    {
        doorTimerResult = death.Step(
            bus,
            phase3Samus,
            0,
            0,
            nextRandomNumber: () =>
            {
                ushort sampled = (escapeRngCalls & 1) == 0 ? (ushort)0x3fff : (ushort)0x4000;
                escapeRngCalls++;
                return sampled;
            });
        if (doorTimerResult.EscapeDoorExplosion is { } emitted)
            emittedDoorExplosions.Add(emitted);
        doorTimerCalls++;
        AssertTrue(doorTimerCalls <= 33, "door timer reaches underflow within 33 calls");
    }
    AssertEqual(33, doorTimerCalls, "`$20` door countdown accepts zero and expires on call 33");
    AssertEqual(escapeRngCalls, emittedDoorExplosions.Count,
        "door producer advances RNG exactly once per emitted projectile");
    AssertTrue(emittedDoorExplosions.Count >= 6,
        "33-call door countdown exposes repeated five-call explosion cadence");
    for (int explosionIndex = 0; explosionIndex < emittedDoorExplosions.Count; explosionIndex++)
    {
        MotherBrainEscapeDoorExplosionRequest emitted = emittedDoorExplosions[explosionIndex];
        ushort expectedPattern = unchecked((ushort)(
            (emittedDoorExplosions[0].PatternIndex - explosionIndex) & 3));
        AssertEqual(expectedPattern, emitted.PatternIndex,
            $"door explosion {explosionIndex} cycles 3,2,1,0");
        AssertEqual(explosionIndex % 2 == 0 ? (ushort)0x000c : (ushort)0x0003,
            emitted.ProjectileParameter,
            $"door explosion {explosionIndex} honors `$4000` RNG boundary");
        AssertEqual((ushort)0x0024, emitted.SoundEffect,
            $"door explosion {explosionIndex} queues sound `$24`");
    }
    AssertTrue(doorTimerResult.TimerHandlingEnableRequested,
        "timer expiry publishes Samus command `$0F`");
    AssertTrue(doorTimerResult.MotherBrainEscapeTimerStartRequested,
        "timer expiry publishes TimerStatus `$0002`");
    AssertTrue(doorTimerResult.MotherBrainBossBitRequested,
        "timer expiry publishes current-area mini-boss bit `$02`");
    AssertTrue(doorTimerResult.ZebesTimebombEventRequested,
        "timer expiry publishes event `$0E`");
    AssertEqual((ushort)0, death.DeathExplosionIntervalTimer,
        "timer expiry clears reused explosion interval");
    AssertEqual((ushort)0, death.EscapeDoorIndex,
        "timer expiry clears door explosion index");

    MotherBrainRainbowBeamAttackStepResult blownDoor = death.Step(bus, phase3Samus, 0, 0);
    AssertEqual(8, blownDoor.EscapeDoorParticleSpawns.Count,
        "door blow-up attempts all eight particle allocations in one call");
    for (ushort parameter = 0; parameter < 8; parameter++)
    {
        AssertEqual(new MotherBrainEscapeDoorParticleSpawnRequest(parameter),
            blownDoor.EscapeDoorParticleSpawns[parameter],
            $"door fragment spawn parameter {parameter}");
    }
    AssertEqual(new MotherBrainEscapeDoorPlmRequest(0x00, 0x06, 0xb677),
        blownDoor.EscapeDoorPlm!.Value,
        "door blow-up requests native hardcoded PLM location and entry");
    AssertEqual(MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceKeepEarthquakeGoing,
        death.Phase, "door blow-up reaches final Mother Brain body function");

    MotherBrainRainbowBeamAttackStepResult nonzeroQuake = death.Step(
        bus, phase3Samus, 0, 0, globalEarthquakeTimer: 1);
    AssertTrue(!nonzeroQuake.EarthquakeTimerRefreshed,
        "final body function leaves nonzero global earthquake timer alone");
    AssertEqual((ushort)1, death.EarthquakeTimer,
        "final body function mirrors nonzero global quake sample");
    MotherBrainRainbowBeamAttackStepResult zeroQuake = death.Step(
        bus, phase3Samus, 0, 0, globalEarthquakeTimer: 0);
    AssertTrue(zeroQuake.EarthquakeTimerRefreshed,
        "final body function changes visible zero to `$FFFF`");
    AssertEqual((ushort)0xffff, death.EarthquakeTimer,
        "final body function holds earthquake indefinitely");

    Console.WriteLine(
        "  Baby Metroid: death movement, corpse rotting, escape DMA, timer handoff, and door blow-up agree.");
}

/// <summary>
/// Verifies `$A9:9F00`'s phase-three head bytecode and `$86:CB59`'s complete bomb lifecycle:
/// initialization, 8.8 motion, nine bounce-table stages, animation, both deletion paths,
/// afterburn/dust/sound requests, and the Mother Brain body-owned active-bomb counter.
/// </summary>
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
    AssertEqual((ushort)0x9f00, motherBrain.HeadInstructionPointer,
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
    AssertEqual<ushort?>(0x006f, headSoundLibraryTwo,
        "phase-three bomb head consumes library-two cry operand");
    AssertEqual((ushort)0x9f28, spawnHead.InstructionPointerAfter,
        "spawn call loads the 32-frame open-mouth record after both opcodes");

    // Finish the close-mouth tail. `$9B14` must re-enable the neck, jump to `$9CB9`, and
    // load the first neutral frame during the same interpreter call.
    for (int call = 0; call < 52; call++)
        motherBrain.StepHeadAnimation(bus, samus, baby: null, randomNumberSeed: 0x0100);
    AssertEqual((ushort)0x9cbd, motherBrain.HeadInstructionPointer,
        "bomb tail returns to first phase-three neutral frame without a blank call");
    AssertEqual((ushort)1, motherBrain.NeckMovementEnabled,
        "bomb tail re-enables neck movement before entering neutral list");

    // The retail `$9D0D` contains an unconditional BRA over a tempting cry branch. Low
    // twelve-bit RNG below `$EC0` loops to `$9CD1`; exactly `$EC0` falls through to `$9CDB`.
    for (int call = 0; call < 44; call++)
        motherBrain.StepHeadAnimation(bus, samus, baby: null, randomNumberSeed: 0x0ebf);
    AssertEqual((ushort)0x9cd5, motherBrain.HeadInstructionPointer,
        "neutral low-RNG branch reloads `$9CD1` frame");
    for (int call = 0; call < 16; call++)
        motherBrain.StepHeadAnimation(bus, samus, baby: null, randomNumberSeed: 0x0ec0);
    AssertEqual((ushort)0x9cdf, motherBrain.HeadInstructionPointer,
        "neutral RNG `$EC0` boundary falls through to `$9CDB` frame");

    var projectiles = new MotherBrainEnemyProjectileSystem();
    AssertEqual<int?>(17, projectiles.SpawnBomb(motherBrain, spawnHead.BombSpawn.Value),
        "Mother Brain bomb uses highest free shared slot");
    AssertEqual((ushort)1, motherBrain.BombCounter, "bomb initializer increments body counter");
    MotherBrainEnemyProjectileSlot bomb = projectiles.Slots[17];
    AssertEqual((ushort)0x004c, bomb.XPosition, "bomb initializes at brain X plus twelve");
    AssertEqual((ushort)0x0070, bomb.YPosition, "bomb initializes at brain Y plus sixteen");
    AssertEqual((ushort)0x0001, bomb.XSubposition,
        "eight-bit initializer preserves afterburn count in low X-subposition byte");

    MotherBrainEnemyProjectileFrameResult first = projectiles.StepFrame(
        bus, motherBrain, baby: null, samus, layer1X: 0);
    AssertEqual((ushort)0x004c, bomb.XPosition, "first `$00.DE` X move remains subpixel");
    AssertEqual((ushort)0xde01, bomb.XSubposition,
        "first X move updates high fraction without destroying low afterburn byte");
    AssertEqual((ushort)0x0071, bomb.YPosition, "first `$01.07` Y move advances one pixel");
    AssertEqual((ushort)0x0700, bomb.YSubposition, "first Y move retains `$07` fraction");
    AssertEqual((ushort)0x00de, bomb.XVelocity, "pre-bounce friction subtracts exactly two");
    AssertEqual((ushort)0x0107, bomb.YVelocity, "first gravity stage adds seven");
    AssertEqual((ushort)0x82dc, bomb.SpritemapPointer, "spawn frame loads bomb spritemap zero");
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
        AssertEqual((ushort)0x00d0, bounceEvents[bounceIndex].YPosition,
            $"bounce {bounceIndex + 1} clamps to floor Y `$D0`");
    }
    AssertTrue(expired.HasValue, "zero acceleration-table entry naturally expires bomb");
    AssertEqual<ushort?>(1, expired!.Value.AfterburnCount,
        "natural expiry publishes preserved low-byte afterburn count");
    AssertEqual((ushort)3, expired.Value.DustParameter, "natural expiry requests misc dust three");
    AssertEqual<ushort?>(0x0013, expired.Value.QueuedSoundLibraryThree,
        "natural expiry queues library-three sound `$13`");
    AssertTrue(!expired.Value.EnemyDropRequested, "natural expiry does not spawn enemy drops");
    AssertEqual((ushort)0, motherBrain.BombCounter, "natural expiry decrements body counter");

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
    var room = new RoomLevelData(
        roomWidth,
        roomHeight,
        emptyBlocks,
        new byte[emptyBlocks.Length],
        new ushort[emptyBlocks.Length],
        new byte[roomWidth]);
    var bombSamus = new SamusState
    {
        Pose = SamusState.MorphBallGroundRightPose,
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
    AssertEqual((ushort)9, destroyed.DustParameter, "collision path requests misc dust nine");
    AssertTrue(destroyed.EnemyDropRequested, "collision path requests Mother Brain head drops");
    AssertEqual<ushort?>(null, destroyed.AfterburnCount,
        "collision path suppresses natural afterburn spawn");
    AssertEqual<ushort?>(null, destroyed.QueuedSoundLibraryThree,
        "collision path suppresses natural expiry sound");
    AssertEqual((ushort)0, motherBrain.BombCounter, "collision deletion decrements body counter");
    MotherBrainEnemyProjectileSlot collisionDust = collisionProjectiles.Slots[17];
    AssertEqual(MotherBrainEnemyProjectileSystem.MiscDustDefinition,
        collisionDust.ProjectileId,
        "collision deletion replaces the source bomb with parameter-nine dust");
    AssertEqual((ushort)0x0005, collisionDust.InstructionTimer,
        "same-slot collision dust loads its first duration immediately");
    AssertEqual((ushort)0x9a5a, collisionDust.SpritemapPointer,
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
    AssertEqual((ushort)0x40a0, bomb.Properties, "bomb definition properties");
    AssertEqual((ushort)0x3000, breath.Properties, "purple-breath definition properties");
    AssertEqual((ushort)0x0046, breath.XPosition, "purple breath initializes at brain X plus six");
    AssertEqual((ushort)0x0070, breath.YPosition, "purple breath initializes at brain Y plus sixteen");

    mixedPool.StepFrame(bus, motherBrain, baby: null, samus, layer1X: 0);
    AssertEqual((ushort)0x954f, breath.SpritemapPointer,
        "purple breath spawn call loads first bank-$8D spritemap");
    AssertEqual((ushort)0x82dc, bomb.SpritemapPointer,
        "bomb spawn call loads first bank-$8D spritemap");

    var oam = new OamBuffer();
    oam.BeginFrame();
    mixedPool.DrawHighPriority(bus, oam, layer1X: 0, layer1Y: 0);
    AssertEqual(4, oam.NextByteOffset, "high pass emits only purple breath");
    OamEntry high = oam.GetEntry(0);
    AssertEqual(0x047, high.X, "high-pass breath screen X plus spritemap offset");
    AssertEqual((byte)0x72, high.Y, "high-pass breath screen Y plus spritemap offset");
    AssertTrue(high.IsLarge, "high-pass breath preserves large OBJ bit");
    AssertEqual(0x011, high.TileNumber, "high-pass breath tile");

    mixedPool.DrawLowPriority(bus, oam, layer1X: 0, layer1Y: 0);
    AssertEqual(8, oam.NextByteOffset, "low pass appends only bomb after high pass");
    OamEntry low = oam.GetEntry(1);
    AssertEqual(0x04a, low.X, "low-pass bomb signed X offset");
    AssertEqual((byte)0x70, low.Y, "low-pass bomb negative Y offset");
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
            AssertEqual((ushort)0x0046, timedBreath.XPosition,
                $"purple breath remains stationary in X on call {call}");
            AssertEqual((ushort)0x0070, timedBreath.YPosition,
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
    AssertEqual((ushort)0x1000, dust.Properties, "misc dust uses high-priority pass");
    AssertEqual((ushort)3, dust.SpawnParameter, "misc-dust animation parameter retained");
    AssertEqual((ushort)0xe138, dust.InstructionPointer,
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
            AssertEqual((ushort)0x0064, dust.XPosition,
                $"misc dust remains stationary in X on call {call}");
            AssertEqual((ushort)0x0050, dust.YPosition,
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
        AssertEqual((ushort)0x0010, slot.XPosition, $"door fragment {parameter} initial X");
        AssertEqual(unchecked((ushort)(0x0080 + expectedYOffsets[parameter])),
            slot.YPosition, $"door fragment {parameter} initial Y");
        AssertEqual((ushort)0x0500, slot.XVelocity,
            $"door fragment {parameter} initial X velocity");
        AssertEqual(unchecked((ushort)expectedYVelocities[parameter]),
            slot.YVelocity, $"door fragment {parameter} initial Y velocity");
        AssertEqual((ushort)0x0020, slot.Lifetime,
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
    AssertEqual((ushort)0x0014, firstFragment.XPosition,
        "first fragment applies slowed `$04.F0` whole X movement");
    AssertEqual((ushort)0xf000, firstFragment.XSubposition,
        "first fragment retains `$F0` X velocity fraction in high subposition byte");
    AssertEqual((ushort)0x005e, firstFragment.YPosition,
        "first fragment applies gravity then signed `$FE.20` Y movement");
    AssertEqual((ushort)0x2000, firstFragment.YSubposition,
        "first fragment retains `$20` Y fraction");
    AssertEqual((ushort)0x04f0, firstFragment.XVelocity,
        "first fragment X friction subtracts `$10`");
    AssertEqual((ushort)0xfe20, firstFragment.YVelocity,
        "first fragment gravity adds `$20`");
    AssertEqual((ushort)0x969b, firstFragment.SpritemapPointer,
        "spawn frame loads exploded-door spritemap zero");
    AssertEqual((ushort)0x001f, firstFragment.Lifetime,
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
        AssertEqual((ushort)0x0009, dust.ProjectileParameter, "terminal fragment dust parameter");
    AssertTrue(!projectiles.Slots[10].IsActive,
        "lowest slot's downward fragment dust deletes off-screen during the same pass");
    for (int slotIndex = 11; slotIndex < MotherBrainEnemyProjectileSystem.SlotCount; slotIndex++)
    {
        MotherBrainEnemyProjectileSlot terminalDust = projectiles.Slots[slotIndex];
        AssertEqual(MotherBrainEnemyProjectileSystem.MiscDustDefinition,
            terminalDust.ProjectileId,
            $"terminal fragment slot {slotIndex} now contains misc dust");
        AssertEqual((ushort)0x0005, terminalDust.InstructionTimer,
            $"same-pass terminal dust slot {slotIndex} loads first duration");
        AssertEqual((ushort)0x9abc, terminalDust.SpritemapPointer,
            $"same-pass terminal dust slot {slotIndex} loads first spritemap");
        AssertEqual((ushort)0xe1b4, terminalDust.InstructionPointer,
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
    AssertEqual((ushort)0x970b, subtitle.SpritemapPointer,
        "alternate subtitle loads Japanese text spritemap");
    subtitlePool.StepFrame(bus, motherBrain, baby: null, samus, layer1X: 0);
    AssertEqual((ushort)0x0080, subtitle.XPosition,
        "alternate subtitle pre-instruction repins X");
    AssertEqual((ushort)0x00c0, subtitle.YPosition,
        "alternate subtitle pre-instruction repins Y");
    AssertEqual((ushort)0, subtitle.XVelocity,
        "alternate subtitle pre-instruction clears X velocity");
    AssertEqual((ushort)0, subtitle.YVelocity,
        "alternate subtitle pre-instruction clears Y velocity");
    AssertTrue(subtitle.IsActive, "alternate subtitle sleep keeps projectile alive");

    Console.WriteLine(
        "  Mother Brain projectiles: shared allocation, subtitle pinning, door-fragment motion, animation, and dust agree.");
}

static void VerifySamusAerialMovement()
{
    var bus = new TestAddressSpace();

    // Minimal literal pose records for the verified right-facing route. Byte one is the
    // movement dispatcher index, byte four is the signed graphics offset, and byte six is
    // the collision radius. All values mirror the retail definitions.
    bus.WriteBytes(0x91b881, [0x08, 0x02, 0xff, 0x02, 0x03, 0x00, 0x13, 0x00]); // $4B
    bus.WriteBytes(0x91b891, [0x08, 0x02, 0xff, 0x02, 0x08, 0x00, 0x13, 0x00]); // $4D
    bus.WriteBytes(0x91bb49, [0x08, 0x00, 0xff, 0x02, 0x03, 0x00, 0x15, 0x00]); // $A4
    bus.WriteBytes(0x91b631, [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]); // $01

    // Animation streams are byte-indexed. $4B spends one frame as a transition then FD
    // publishes $4D. $4D's first two frames support the rising-frame hold assertion below.
    // $A4 reaches F8 and publishes standing-right pose $01.
    WriteTestWord(bus, 0x91b0a6, 0xc100); // pose $4B pointer
    WriteTestWord(bus, 0x91b0aa, 0xc110); // pose $4D pointer
    WriteTestWord(bus, 0x91b158, 0xc120); // pose $A4 pointer
    WriteTestWord(bus, 0x91b012, 0xc130); // pose $01 pointer
    bus.WriteBytes(0x91c100, [0x01, 0xfd, 0x4d]);
    bus.WriteBytes(0x91c110, [0x02, 0x03, 0x03, 0x03, 0x03, 0x50, 0xfe, 0x01]);
    bus.WriteBytes(0x91c120, [0x04, 0x02, 0xf8, 0x01]);
    bus.WriteBytes(0x91c130, [0x0a]);

    // Dry-air physics constants come from the same bank-$90 words used by production.
    WriteTestWord(bus, 0x909eb9, 0x0004);
    WriteTestWord(bus, 0x909ebf, 0xe000);
    WriteTestWord(bus, 0x909ea1, 0x2800);
    WriteTestWord(bus, 0x909ea7, 0x0000);

    // Type two reads normal-air base $9F55 + 2*12 = $9F6D. With no direction input the
    // native handler clears the tentative acceleration again, but seed all six words so
    // the pointer and arithmetic remain real rather than relying on sparse-bus zeroes.
    WriteTestWord(bus, 0x909f6d, 0x0000);
    WriteTestWord(bus, 0x909f6f, 0x1000);
    WriteTestWord(bus, 0x909f71, 0x0001);
    WriteTestWord(bus, 0x909f73, 0x0000);
    WriteTestWord(bus, 0x909f75, 0x0000);
    WriteTestWord(bus, 0x909f77, 0x1000);

    const int width = 8;
    const int height = 8;
    var foreground = new ushort[width * height];
    for (int x = 0; x < width; x++)
        foreground[6 * width + x] = 0x8000; // ordinary solid floor begins at Y=96
    var level = new RoomLevelData(
        width,
        height,
        foreground,
        new byte[foreground.Length],
        new ushort[foreground.Length],
        new byte[8]);

    var samus = new SamusState
    {
        Pose = SamusState.FacingRightNormalPose,
        XPosition = 48,
        YPosition = 77, // radius 19 will place jump-pose feet at floor Y=96
    };
    samus.ApplyOrdinaryJumpTransition(bus, SamusState.NeutralJumpTransitionRightPose);
    AssertEqual((ushort)0x0004, samus.Kinematics.YSpeed, "jump reads initial whole Y speed");
    AssertEqual((ushort)0xe000, samus.Kinematics.YSubspeed, "jump reads initial fractional Y speed");
    AssertEqual((ushort)1, samus.Kinematics.YDirection, "jump begins upward");

    // The $4B transition movement deliberately does not consume the initialized 4.E000.
    AerialMovementResult transitionFrame = SamusAerialMovement.StepNormalJump(
        bus, level, samus, (ushort)SnesButton.A, nmiFrameCounter: 0);
    AssertTrue(transitionFrame.Vertical is null, "neutral-jump transition skips vertical movement");
    AssertEqual((ushort)77, samus.YPosition, "neutral-jump transition preserves Y");
    samus.AnimateNoFx(bus);
    AssertEqual((byte)0xfd, samus.LastAnimationDelayCommand!.Value, "neutral-jump transition reaches FD");
    AssertTrue(samus.ApplyPendingVerifiedAnimationTransition(bus), "FD installs neutral jump pose");
    AssertEqual((byte)0x4d, samus.Pose, "FD target pose");

    // The first real airborne frame moves by OLD 4.E000, then stores 4.B800 after gravity.
    AerialMovementResult firstRise = SamusAerialMovement.StepNormalJump(
        bus, level, samus, (ushort)SnesButton.A, nmiFrameCounter: 1);
    AssertTrue(firstRise.Vertical is { Collided: false }, "first rise remains in air");
    AssertEqual((ushort)72, samus.YPosition, "first rise old-speed whole displacement");
    AssertEqual((ushort)0x2000, samus.Kinematics.YSubposition, "first rise old-speed fraction");
    AssertEqual((ushort)0x0004, samus.Kinematics.YSpeed, "first rise stored whole speed");
    AssertEqual((ushort)0xb800, samus.Kinematics.YSubspeed, "first rise subtracts gravity afterward");

    // Releasing jump cuts velocity before the common routine copies it. The frame has no
    // displacement, changes direction to down, and only primes 0.2800 for the next frame.
    ushort releaseY = samus.YPosition;
    ushort releaseSubY = samus.Kinematics.YSubposition;
    SamusAerialMovement.StepNormalJump(bus, level, samus, controllerInput: 0, nmiFrameCounter: 2);
    AssertEqual((ushort)2, samus.Kinematics.YDirection, "jump release starts falling");
    AssertEqual(releaseY, samus.YPosition, "jump release stationary whole Y frame");
    AssertEqual(releaseSubY, samus.Kinematics.YSubposition, "jump release stationary fractional Y frame");
    AssertEqual((ushort)0x2800, samus.Kinematics.YSubspeed, "jump release primes falling gravity");

    // Continue the native recurrence until the solid floor clips a downward displacement.
    // This is bounded well above the roughly 50 frames needed by the synthetic room.
    AerialMovementResult result = default;
    for (int frame = 3; frame < 200 && !result.Landed; frame++)
        result = SamusAerialMovement.StepNormalJump(bus, level, samus, 0, (ushort)frame);
    AssertTrue(result.Landed, "neutral jump eventually reports solid-floor landing");
    AssertEqual((ushort)77, samus.YPosition, "aerial radius rests at floor before pose expansion");

    samus.ApplyAerialLanding(bus, wasSpinning: false);
    AssertEqual((byte)0xa4, samus.Pose, "normal right-facing landing pose");
    AssertEqual((ushort)75, samus.YPosition, "landing radius expansion keeps feet fixed");
    AssertEqual((ushort)0, samus.Kinematics.YDirection, "landing clears vertical direction");

    // Four ticks plus two ticks reach $F8 at byte index two; its operand returns to $01.
    for (int tick = 0; tick < 6; tick++)
        samus.AnimateNoFx(bus);
    AssertEqual((byte)0xf8, samus.LastAnimationDelayCommand!.Value, "landing reaches F8");
    AssertTrue(samus.ApplyPendingVerifiedAnimationTransition(bus), "landing F8 transition applies");
    AssertEqual((byte)0x01, samus.Pose, "landing animation returns to standing");

    // Aerial mode two accelerates because $90:9B22 tests only bit zero. This guards the
    // counterintuitive distinction from the grounded deceleration-allowed routine.
    var aerialSpeed = new SamusHorizontalSpeedState { AccelerationMode = 2 };
    aerialSpeed.SelectNormalAirSpeedTable();
    AerialBaseSpeedResult accelerated =
        aerialSpeed.CalculateBaseSpeedDecelerationDisallowed(bus, movementType: 2);
    AssertEqual(0x00001000u, accelerated.Speed, "aerial mode two accelerates");
    AssertTrue(!accelerated.ReachedMaximum, "aerial sub-cap call clears carry");

    // Mirror the launch through the left-facing spin-jump route. This checks that mode two
    // uses the pose direction normally, and that the type-three speed entry (not running's
    // type-one entry) supplies the in-air cap/acceleration.
    bus.WriteBytes(0x91b6f9, [0x04, 0x03, 0xff, 0xff, 0x00, 0x00, 0x0c, 0x00]); // $1A
    WriteTestWord(bus, 0x91b044, 0xc140);
    bus.WriteBytes(0x91c140, [0x03, 0x03, 0x02, 0x03, 0x02, 0x03, 0x02, 0x03, 0x02, 0xfe, 0x08]);
    WriteTestWord(bus, 0x909f79, 0x0000);
    WriteTestWord(bus, 0x909f7b, 0x2000);
    WriteTestWord(bus, 0x909f7d, 0x0001);
    WriteTestWord(bus, 0x909f7f, 0x6000);
    WriteTestWord(bus, 0x909f81, 0x0000);
    WriteTestWord(bus, 0x909f83, 0x1000);
    var spinLeft = new SamusState
    {
        Pose = SamusState.MovingLeftNormalPose,
        XPosition = 48,
        YPosition = 77,
    };
    spinLeft.HorizontalSpeed.BaseSpeed = 1;
    spinLeft.ApplyOrdinaryJumpTransition(bus, SamusState.SpinJumpLeftPose);
    AerialMovementResult spinFrame = SamusAerialMovement.StepSpinJump(
        bus,
        level,
        spinLeft,
        (ushort)(SnesButton.Left | SnesButton.A),
        nmiFrameCounter: 0);
    AssertEqual(-0x00012000, spinFrame.Horizontal.AcceptedDisplacement, "left spin jump displacement");
    AssertEqual((ushort)2, spinLeft.HorizontalSpeed.AccelerationMode, "spin jump selects aerial mode two");
    AssertEqual((ushort)46, spinLeft.XPosition, "left spin jump whole X");
    AssertEqual((ushort)0xe000, spinLeft.Kinematics.XSubposition, "left spin jump fractional X");

    // Walking off a ledge is the movement-type-six entry point. $91:E8F2 selects pose $2A
    // from the old left-facing direction and command five begins with a stationary falling
    // frame before gravity produces displacement. Landing from type six uses normal $A5.
    bus.WriteBytes(0x91b779, [0x04, 0x06, 0xff, 0x07, 0x08, 0x00, 0x13, 0x00]); // $2A
    bus.WriteBytes(0x91bb51, [0x04, 0x00, 0xff, 0x07, 0x03, 0x00, 0x15, 0x00]); // $A5
    bus.WriteBytes(0x91b639, [0x04, 0x00, 0xff, 0x07, 0x06, 0x00, 0x15, 0x00]); // $02
    WriteTestWord(bus, 0x91b064, 0xc150);
    WriteTestWord(bus, 0x91b15a, 0xc160);
    WriteTestWord(bus, 0x91b014, 0xc170);
    bus.WriteBytes(0x91c150, [0x05, 0x04, 0x04, 0xfe, 0x01, 0x06, 0x10, 0xfe, 0x01]);
    bus.WriteBytes(0x91c160, [0x04, 0x02, 0xf8, 0x02]);
    bus.WriteBytes(0x91c170, [0x0a]);
    WriteTestWord(bus, 0x909f9d, 0x0000); // type-six entry at $9F55 + 6*12
    WriteTestWord(bus, 0x909f9f, 0x1000);
    WriteTestWord(bus, 0x909fa1, 0x0001);
    WriteTestWord(bus, 0x909fa3, 0x0000);
    WriteTestWord(bus, 0x909fa5, 0x0000);
    WriteTestWord(bus, 0x909fa7, 0x1000);
    var fallLeft = new SamusState
    {
        Pose = SamusState.FacingLeftNormalPose,
        XPosition = 48,
        YPosition = 77,
    };
    fallLeft.ApplyWalkedOffFloorTransition(bus, SamusState.FallingLeftPose);
    AssertEqual((byte)0x2a, fallLeft.Pose, "walk-off chooses left falling pose");
    AerialMovementResult firstFall = SamusAerialMovement.StepFalling(
        bus, level, fallLeft, controllerInput: 0, nmiFrameCounter: 0);
    AssertEqual(0, firstFall.Vertical!.Value.AcceptedDisplacement, "walk-off starts with stationary fall frame");
    AssertEqual((ushort)0x2800, fallLeft.Kinematics.YSubspeed, "first fall frame primes gravity");
    AerialMovementResult fallResult = default;
    for (int frame = 1; frame < 200 && !fallResult.Landed; frame++)
        fallResult = SamusAerialMovement.StepFalling(bus, level, fallLeft, 0, (ushort)frame);
    AssertTrue(fallResult.Landed, "left falling pose reaches floor");
    fallLeft.ApplyAerialLanding(bus, wasSpinning: false);
    AssertEqual((byte)0xa5, fallLeft.Pose, "left fall selects normal landing pose");

    Console.WriteLine("  Samus aerial: FD launch, exact 16.16 arc, jump cut, floor landing, radius, and F8 agree.");
}

/// <summary>
/// Exercises the equipment-aware spin initializer, every edge of the dry-air Space Jump
/// velocity window, Screw Attack contact/wall frames, and both bank-$91/bank-$9B palette
/// indirections. These checks deliberately keep Space Jump physics independent from the
/// visible Screw Attack pose used when both retail item bits are equipped.
/// </summary>
static void VerifySamusSpaceJumpAndScrewAttack()
{
    var bus = new TestAddressSpace();

    // All six stable spin records share movement type three and radius twelve. The generic
    // `$19/$1A` records are transition-table outputs; the four equipment records are the
    // actual bodies selected by `$91:F624` after that table lookup.
    foreach ((byte pose, byte direction) in new (byte, byte)[]
    {
        (SamusState.SpinJumpRightPose, 8),
        (SamusState.SpinJumpLeftPose, 4),
        (SamusState.SpaceJumpRightPose, 8),
        (SamusState.SpaceJumpLeftPose, 4),
        (SamusState.ScrewAttackRightPose, 8),
        (SamusState.ScrewAttackLeftPose, 4),
    })
    {
        bus.WriteBytes(0x91b629 + pose * 8, [direction, 3, 0xff, 0xff, 0, 0, 12, 0]);
        ushort stream = unchecked((ushort)(0xc000 + pose * 0x20));
        WriteTestWord(bus, 0x91b010 + pose * 2, stream);
        for (int frame = 0; frame < 31; frame++)
            bus.WriteByte(0x910000 | unchecked((ushort)(stream + frame)), 4);
    }

    // Running sources and spin-landing endpoints are sufficient to prove equipment
    // substitution and Screw palette restoration without mocking pose metadata in code.
    bus.WriteBytes(0x91b629 + SamusState.MovingRightNormalPose * 8,
        [8, 1, 0xff, 2, 0, 0, 21, 0]);
    bus.WriteBytes(0x91b629 + SamusState.MovingLeftNormalPose * 8,
        [4, 1, 0xff, 7, 0, 0, 21, 0]);
    bus.WriteBytes(0x91b629 + SamusState.SpinLandingRightPose * 8,
        [8, 0, 0xff, 2, 0, 0, 21, 0]);
    bus.WriteBytes(0x91b629 + SamusState.SpinLandingLeftPose * 8,
        [4, 0, 0xff, 7, 0, 0, 21, 0]);
    WriteTestWord(bus, 0x91b010 + SamusState.SpinLandingRightPose * 2, 0xc800);
    WriteTestWord(bus, 0x91b010 + SamusState.SpinLandingLeftPose * 2, 0xc810);
    bus.WriteByte(0x91c800, 4);
    bus.WriteByte(0x91c810, 4);

    // Dry-air Samus_InitJump and gravity words. The type-three horizontal record is zeroed
    // intentionally so the assertions isolate the vertical 8.8 gate from X acceleration.
    WriteTestWord(bus, 0x909eb9, 4);
    WriteTestWord(bus, 0x909ebf, 0xe000);
    WriteTestWord(bus, 0x909ea1, 0x2800);
    WriteTestWord(bus, 0x909ea7, 0);
    for (int byteOffset = 0; byteOffset < SpeedTableEntry.ByteCount; byteOffset += 2)
        WriteTestWord(bus, 0x909f79 + byteOffset, 0);

    var empty = new RoomLevelData(
        16,
        16,
        new ushort[16 * 16],
        new byte[16 * 16],
        new ushort[16 * 16],
        new byte[8]);

    var spaceLaunch = new SamusState
    {
        Pose = SamusState.MovingRightNormalPose,
        EquippedItems = 0x0200,
        XPosition = 128,
        YPosition = 128,
    };
    spaceLaunch.ApplyOrdinaryJumpTransition(bus, SamusState.SpinJumpRightPose);
    AssertEqual(SamusState.SpaceJumpRightPose, spaceLaunch.Pose,
        "Space Jump substitutes right spin pose");

    var screwLaunch = new SamusState
    {
        Pose = SamusState.MovingLeftNormalPose,
        EquippedItems = 0x0208,
        XPosition = 128,
        YPosition = 128,
    };
    screwLaunch.ApplyOrdinaryJumpTransition(bus, SamusState.SpinJumpLeftPose);
    AssertEqual(SamusState.ScrewAttackLeftPose, screwLaunch.Pose,
        "Screw Attack takes priority over Space Jump pose");

    // `$81/$82` have their own retail input tables, so an opposite-direction match may
    // publish the specialized target directly rather than generic `$19/$1A`. The common
    // F624 initializer must accept that record, preserve Screw's equipment priority, and
    // still start a direction change at animation frame one.
    screwLaunch.ApplySpinJumpDirectionTransition(bus, SamusState.ScrewAttackRightPose);
    AssertEqual(SamusState.ScrewAttackRightPose, screwLaunch.Pose,
        "direct Screw table target preserves equipped art");
    AssertEqual((ushort)1, screwLaunch.AnimationFrame,
        "direct Screw direction transition starts at frame one");

    static SamusState CreateFallingSpin(byte pose, ushort items, ushort speed, ushort subspeed) => new()
    {
        Pose = pose,
        EquippedItems = items,
        XPosition = 128,
        YPosition = 128,
        Kinematics =
        {
            YDirection = 2,
            YSpeed = speed,
            YSubspeed = subspeed,
            YAcceleration = 0,
            YSubacceleration = 0x2800,
            XRadius = 5,
            YRadius = 12,
        },
    };

    // `$0280` is inclusive. A fresh edge restarts at 4.E000, moves upward by that OLD
    // magnitude, then stores 4.B800 after the shared spin routine subtracts gravity.
    SamusState minimum = CreateFallingSpin(
        SamusState.SpaceJumpRightPose, 0x0200, speed: 2, subspeed: 0x8000);
    SamusAerialMovement.StepSpinJump(
        bus,
        empty,
        minimum,
        (ushort)SnesButton.A,
        nmiFrameCounter: 0,
        controllerNewInput: (ushort)SnesButton.A);
    AssertEqual((ushort)1, minimum.Kinematics.YDirection, "Space Jump minimum velocity restarts upward");
    AssertEqual((ushort)4, minimum.Kinematics.YSpeed, "Space Jump restart whole speed");
    AssertEqual((ushort)0xb800, minimum.Kinematics.YSubspeed, "Space Jump restart applies gravity after movement");
    AssertEqual((ushort)123, minimum.YPosition, "Space Jump restart moves by old 4.E000 magnitude");

    SamusState below = CreateFallingSpin(
        SamusState.SpaceJumpRightPose, 0x0200, speed: 2, subspeed: 0x7fff);
    SamusAerialMovement.StepSpinJump(
        bus, empty, below, (ushort)SnesButton.A, 0, (ushort)SnesButton.A);
    AssertEqual((ushort)2, below.Kinematics.YDirection, "Space Jump rejects velocity $027F");

    SamusState maximum = CreateFallingSpin(
        SamusState.SpaceJumpRightPose, 0x0200, speed: 5, subspeed: 0);
    SamusAerialMovement.StepSpinJump(
        bus, empty, maximum, (ushort)SnesButton.A, 0, (ushort)SnesButton.A);
    AssertEqual((ushort)2, maximum.Kinematics.YDirection, "Space Jump maximum $0500 is exclusive");

    SamusState heldOnly = CreateFallingSpin(
        SamusState.SpaceJumpRightPose, 0x0200, speed: 3, subspeed: 0);
    SamusAerialMovement.StepSpinJump(bus, empty, heldOnly, (ushort)SnesButton.A, 0);
    AssertEqual((ushort)2, heldOnly.Kinematics.YDirection, "Space Jump requires a fresh Jump edge");

    // Both item bits retain Space Jump physics while Screw Attack owns art and damage.
    SamusState screwRepeat = CreateFallingSpin(
        SamusState.ScrewAttackRightPose, 0x0208, speed: 3, subspeed: 0);
    SamusAerialMovement.StepSpinJump(
        bus, empty, screwRepeat, (ushort)SnesButton.A, 0, (ushort)SnesButton.A);
    AssertEqual((ushort)1, screwRepeat.Kinematics.YDirection,
        "Screw Attack with Space Jump repeats upward");
    AssertEqual((ushort)3, screwRepeat.HorizontalSpeed.ContactDamageIndex,
        "Screw Attack republishes contact damage index three");

    // `$84:CE91-$CEA3` explicitly admits poses `$81/$82`, independently of the boost-stage
    // branch above them. Place a BTS-4 permanent 1x1 block under a falling Screw body so the
    // shared vertical movement must allocate the room PLM and continue through new air.
    var screwBombForeground = new ushort[16 * 16];
    var screwBombBts = new byte[screwBombForeground.Length];
    int screwBombIndex = 8 * 16 + 8;
    screwBombForeground[screwBombIndex] = 0xf123;
    screwBombBts[screwBombIndex] = 4;
    var screwBombLevel = new RoomLevelData(
        16,
        16,
        screwBombForeground,
        screwBombBts,
        new ushort[screwBombForeground.Length],
        new byte[8]);
    var screwBombPlms = new RoomPlmSystem();
    SamusState screwBombSamus = CreateFallingSpin(
        SamusState.ScrewAttackRightPose, 0x0008, speed: 3, subspeed: 0);
    AerialMovementResult screwBombFrame = SamusAerialMovement.StepSpinJump(
        bus,
        screwBombLevel,
        screwBombSamus,
        controllerInput: 0,
        nmiFrameCounter: 0,
        controllerNewInput: 0,
        plms: screwBombPlms);
    AssertTrue(screwBombFrame.Vertical is { Collided: false },
        "Screw collision-bomb setup returns carry clear and preserves vertical travel");
    AssertEqual(screwBombIndex,
        screwBombFrame.Vertical!.Value.BrokenBombBlock!.Value.Index,
        "Screw vertical collision publishes the exact BTS-4 block");
    AssertEqual((ushort)0x0123,
        screwBombLevel.GetCollisionBlockByIndex(screwBombIndex).LevelWord,
        "Screw setup clears only the collision nibble before the PLM handler");
    AssertEqual(1, screwBombPlms.ActiveCount,
        "Screw pose installs the BTS-4 bank-$84 lifecycle in the room owner");

    // A fully charged beam makes ordinary/Space-Jump spin damaging only in dry physics.
    // Full submersion suppresses that contact mode and, on animation frames zero/eight's
    // final tick, emits the literal library-one sound $2F instead.
    SamusState chargedSpin = CreateFallingSpin(
        SamusState.SpaceJumpRightPose, 0x0200, speed: 3, subspeed: 0);
    chargedSpin.ProjectileFlareCounter = 0x003c;
    SamusAerialMovement.StepSpinJump(bus, empty, chargedSpin, 0, 0, 0);
    AssertEqual((ushort)4, chargedSpin.HorizontalSpeed.ContactDamageIndex,
        "fully charged dry spin publishes contact damage index four");

    SamusState submergedSpin = CreateFallingSpin(
        SamusState.SpaceJumpRightPose, 0x0200, speed: 3, subspeed: 0);
    submergedSpin.ProjectileFlareCounter = 0x003c;
    submergedSpin.InitializeAnimation(bus, initialFrame: 0);
    for (int tick = 0; tick < 3; tick++)
        submergedSpin.AnimateNoFx(bus);
    submergedSpin.LiquidPhysics.ConfigureWater(surfaceY: 100);
    submergedSpin.LiquidPhysics.BeginFrameSoundRequests();
    SamusAerialMovement.StepSpinJump(bus, empty, submergedSpin, 0, 0, 0);
    AssertEqual((ushort)0, submergedSpin.HorizontalSpeed.ContactDamageIndex,
        "full liquid physics suppresses charged-spin contact damage");
    AssertEqual(1, submergedSpin.LiquidPhysics.SoundRequests.Count,
        "underwater Space Jump sound frame publishes once");
    AssertEqual(new SamusSoundRequest(1, 0x2f, 6), submergedSpin.LiquidPhysics.SoundRequests[0],
        "underwater Space Jump uses library-one sound $2F max six");

    screwRepeat.AnimationFrame = 4;
    screwRepeat.ApplyWallContactAnimationRewind();
    AssertEqual((ushort)0x1a, screwRepeat.AnimationFrame,
        "early Screw wall contact rewinds to frame 26");
    minimum.AnimationFrame = 4;
    minimum.ApplyWallContactAnimationRewind();
    AssertEqual((ushort)0x0a, minimum.AnimationFrame,
        "Space Jump wall contact uses ordinary frame 10 rewind");

    // Three suit-list entries exist in retail; this fixture exercises Power Suit offset
    // zero and gives all six Screw frames unique first colors to prove wrapping order.
    WriteTestWord(bus, 0x91d727, 0x9400);
    WriteTestWord(bus, 0x9b9400, 0x0111);
    WriteTestWord(bus, 0x91da4a, 0xd000);
    for (int frame = 0; frame < 6; frame++)
    {
        ushort palette = unchecked((ushort)(0xe000 + frame * 0x20));
        WriteTestWord(bus, 0x91d000 + frame * 2, palette);
        WriteTestWord(bus, 0x9b0000 | palette, unchecked((ushort)(0x1200 + frame)));
    }

    var palettes = new SamusHorizontalSpeedState();
    var cgram = new SnesCgram();
    AssertTrue(palettes.UpdateSpeedBoosterPalette(
        bus, cgram, movementType: 3, animationFrame: 1, equippedItems: 0x0008),
        "early Screw frame copies normal suit palette");
    AssertEqual((ushort)0x0111, cgram.Colors[192], "early Screw frame normal palette");
    for (int frame = 0; frame < 6; frame++)
    {
        AssertTrue(palettes.UpdateSpeedBoosterPalette(
            bus, cgram, movementType: 3, animationFrame: 0x1b, equippedItems: 0x0008),
            $"Screw palette frame {frame} copies");
        AssertEqual(unchecked((ushort)(0x1200 + frame)), cgram.Colors[192],
            $"Screw palette frame {frame} ROM color");
    }
    AssertEqual((ushort)0, palettes.SpecialPaletteFrame, "six Screw palettes wrap to offset zero");

    // A Screw landing requests the same normal palette reload performed by `$91:F433`.
    screwRepeat.ApplyAerialLanding(bus, wasSpinning: true);
    AssertEqual(SamusState.SpinLandingRightPose, screwRepeat.Pose, "Screw Attack lands through spin landing");
    AssertTrue(screwRepeat.HorizontalSpeed.NormalSuitPaletteRestoreRequested,
        "Screw Attack landing requests normal palette restore");

    Console.WriteLine(
        "  Space Jump/Screw Attack: pose priority, repeat window, collision PLMs, charged/Screw damage, underwater sound, palette cycle, and landing agree.");
}

/// <summary>
/// Exercises the deliberately different top/bottom/bottom-minus-one liquid boundaries,
/// all three ROM jump/gravity/X-table selections, Gravity Suit bypass, submerged dash
/// behavior, and the water-specific Space Jump velocity window.
/// </summary>
static void VerifySamusLiquidPhysics()
{
    var bus = new TestAddressSpace();

    // Seed exactly the adjacent word tables read by `$90:98BC/$90:9C5B`. Distinct values
    // make a wrong byte offset or accidental host constant immediately observable.
    ushort[] launchWhole = [4, 1, 2];
    ushort[] launchFraction = [0xe000, 0xc000, 0xc000];
    ushort[] hiWhole = [6, 2, 3];
    ushort[] hiFraction = [0, 0x8000, 0x8000];
    ushort[] gravityFraction = [0x1c00, 0x0800, 0x0900];
    for (int medium = 0; medium < 3; medium++)
    {
        WriteTestWord(bus, 0x909eb9 + medium * 2, launchWhole[medium]);
        WriteTestWord(bus, 0x909ebf + medium * 2, launchFraction[medium]);
        WriteTestWord(bus, 0x909ec5 + medium * 2, hiWhole[medium]);
        WriteTestWord(bus, 0x909ecb + medium * 2, hiFraction[medium]);
        WriteTestWord(bus, 0x909ea1 + medium * 2, gravityFraction[medium]);
        WriteTestWord(bus, 0x909ea7 + medium * 2, 0);
    }

    var sample = new SamusState { XPosition = 64, YPosition = 100 };
    sample.Kinematics.YRadius = 12; // top 88, bottom 112
    sample.LiquidPhysics.ConfigureWater(surfaceY: 111);
    AssertEqual(SamusLiquidPhysicsState.Water,
        sample.LiquidPhysics.DetermineMovementMedium(sample),
        "water surface one pixel above bottom affects movement");
    AssertTrue(!sample.LiquidPhysics.IsTopBoundarySubmerged(sample),
        "partially submerged body leaves top above water");

    sample.LiquidPhysics.ConfigureWater(surfaceY: 112);
    AssertEqual(SamusLiquidPhysicsState.Air,
        sample.LiquidPhysics.DetermineMovementMedium(sample),
        "liquid equality is not submerged");
    sample.LiquidPhysics.ConfigureWater(surfaceY: 111, liquidOptions: 4);
    AssertEqual(SamusLiquidPhysicsState.Air,
        sample.LiquidPhysics.DetermineMovementMedium(sample),
        "water option bit two disables physics");

    sample.LiquidPhysics.ConfigureLavaAcid(surfaceY: 111);
    AssertEqual(SamusLiquidPhysicsState.LavaAcid,
        sample.LiquidPhysics.DetermineMovementMedium(sample),
        "negative general FX Y selects lava/acid surface");
    sample.EquippedItems = SamusLiquidPhysicsState.GravitySuitItem;
    AssertEqual(SamusLiquidPhysicsState.Air,
        sample.LiquidPhysics.DetermineMovementMedium(sample),
        "Gravity Suit bypasses liquid movement physics");

    // Normal and Hi-Jump launch tables are orthogonal to medium selection. Gravity Suit
    // forces the air entry even while the raw water surface still contains Samus's feet.
    sample.EquippedItems = 0;
    sample.LiquidPhysics.ConfigureWater(surfaceY: 111);
    SamusAerialMovement.InitializeJump(bus, sample);
    AssertEqual((ushort)1, sample.Kinematics.YSpeed, "water normal-jump whole speed");
    AssertEqual((ushort)0xc000, sample.Kinematics.YSubspeed, "water normal-jump fraction");
    AssertEqual((ushort)0x0800, sample.Kinematics.YSubacceleration, "water gravity fraction");

    sample.EquippedItems = 0x0100;
    SamusAerialMovement.InitializeJump(bus, sample);
    AssertEqual((ushort)2, sample.Kinematics.YSpeed, "water Hi-Jump whole speed");
    AssertEqual((ushort)0x8000, sample.Kinematics.YSubspeed, "water Hi-Jump fraction");

    sample.EquippedItems = SamusLiquidPhysicsState.GravitySuitItem | 0x0100;
    SamusAerialMovement.InitializeJump(bus, sample);
    AssertEqual((ushort)6, sample.Kinematics.YSpeed, "Gravity Suit forces air Hi-Jump entry");
    AssertEqual((ushort)0x1c00, sample.Kinematics.YSubacceleration,
        "Gravity Suit forces air acceleration");

    sample.EquippedItems = 0;
    sample.LiquidPhysics.ConfigureLavaAcid(surfaceY: 111);
    SamusAerialMovement.InitializeJump(bus, sample);
    AssertEqual((ushort)2, sample.Kinematics.YSpeed, "lava normal-jump whole speed");
    AssertEqual((ushort)0x0900, sample.Kinematics.YSubacceleration, "lava gravity fraction");

    var speed = sample.HorizontalSpeed;
    speed.SelectEnvironmentSpeedTable(SamusLiquidPhysicsState.Air);
    AssertEqual(SamusHorizontalSpeedState.NormalAirSpeedTableBaseAddress,
        speed.ActiveSpeedTableBaseAddress, "air X table base");
    speed.SelectEnvironmentSpeedTable(SamusLiquidPhysicsState.Water);
    AssertEqual(SamusHorizontalSpeedState.WaterSpeedTableBaseAddress,
        speed.ActiveSpeedTableBaseAddress, "water X table base");
    speed.SelectEnvironmentSpeedTable(SamusLiquidPhysicsState.LavaAcid);
    AssertEqual(SamusHorizontalSpeedState.LavaAcidSpeedTableBaseAddress,
        speed.ActiveSpeedTableBaseAddress, "lava X table base");

    // Submersion reaches `$90:9808` before the running/Dash test. It cannot establish new
    // momentum, but a pre-existing momentum flag preserves the accumulated pair exactly.
    var submergedDash = new SamusHorizontalSpeedState();
    submergedDash.HandleExtraRunSpeed(
        movementType: 1,
        controllerInput: (ushort)SnesButton.B,
        speedBoosterEquipped: false,
        liquidImpeded: true);
    AssertTrue(!submergedDash.HasRunningMomentum, "submerged Dash cannot establish momentum");
    AssertEqual((ushort)0, submergedDash.ExtraRunSubspeed, "submerged no-momentum Dash stays zero");
    submergedDash.HandleExtraRunSpeed(1, (ushort)SnesButton.B, false);
    ushort carriedFraction = submergedDash.ExtraRunSubspeed;
    submergedDash.HandleExtraRunSpeed(
        movementType: 1,
        controllerInput: (ushort)SnesButton.B,
        speedBoosterEquipped: false,
        liquidImpeded: true);
    AssertTrue(submergedDash.HasRunningMomentum, "existing Dash momentum survives submersion");
    AssertEqual(carriedFraction, submergedDash.ExtraRunSubspeed,
        "submersion freezes rather than clears existing extra speed");

    // Pose-change animation samples Y+radius-1. Continuous FX animation samples the full
    // bottom boundary and updates remembered `$0AD2`; prove both edges independently.
    sample.EquippedItems = 0;
    sample.XSpeedDivisor = 7;
    sample.LiquidPhysics.ConfigureWater(surfaceY: 111);
    AssertEqual((ushort)7, sample.LiquidPhysics.DeterminePoseChangeAnimationBuffer(sample),
        "pose-change surface equality uses speed divisor");
    sample.LiquidPhysics.ConfigureWater(surfaceY: 110);
    AssertEqual((ushort)3, sample.LiquidPhysics.DeterminePoseChangeAnimationBuffer(sample),
        "pose-change water delay below bottom-minus-one");
    sample.LiquidPhysics.PrepareAnimationFrame(bus, sample);
    AssertEqual((ushort)3, sample.AnimationFrameBuffer, "continuous water animation delay");
    AssertEqual(SamusLiquidPhysicsState.Water, sample.LiquidPhysics.LiquidPhysicsType,
        "continuous water animation remembers medium");
    sample.EquippedItems = SamusLiquidPhysicsState.GravitySuitItem;
    sample.LiquidPhysics.PrepareAnimationFrame(bus, sample);
    AssertEqual((ushort)0, sample.AnimationFrameBuffer, "Gravity Suit cancels submerged frame delay");

    // Lava's FX handler performs the retail speed-boost cancellation before checking
    // Gravity Suit; acid enters the shared delay/damage tail without touching momentum.
    sample.EquippedItems = SamusLiquidPhysicsState.GravitySuitItem;
    sample.HorizontalSpeed.HandleExtraRunSpeed(
        movementType: 1,
        controllerInput: (ushort)SnesButton.B,
        speedBoosterEquipped: false);
    sample.HorizontalSpeed.SpeedBoostCounter = 0x0401;
    sample.HorizontalSpeed.ExtraRunSpeed = 3;
    sample.HorizontalSpeed.ExtraRunSubspeed = 0x4000;
    sample.LiquidPhysics.ConfigureLavaAcid(surfaceY: 111);
    sample.LiquidPhysics.PrepareAnimationFrame(bus, sample);
    AssertTrue(!sample.HorizontalSpeed.HasRunningMomentum,
        "lava cancels momentum even with Gravity Suit");
    AssertEqual((ushort)0, sample.HorizontalSpeed.SpeedBoostCounter,
        "lava clears speed-boost timer/counter");
    AssertEqual((ushort)0, sample.HorizontalSpeed.ExtraRunSpeed,
        "lava explicitly clears extra whole speed");
    AssertEqual((ushort)0, sample.HorizontalSpeed.ExtraRunSubspeed,
        "lava explicitly clears extra fractional speed");

    sample.EquippedItems = 0;
    sample.HorizontalSpeed.HandleExtraRunSpeed(
        movementType: 1,
        controllerInput: (ushort)SnesButton.B,
        speedBoosterEquipped: false);
    sample.HorizontalSpeed.SpeedBoostCounter = 0x0201;
    sample.HorizontalSpeed.ExtraRunSpeed = 1;
    sample.LiquidPhysics.ConfigureLavaAcid(surfaceY: 111, acid: true);
    sample.LiquidPhysics.PrepareAnimationFrame(bus, sample);
    AssertTrue(sample.HorizontalSpeed.HasRunningMomentum, "acid preserves running momentum");
    AssertEqual((ushort)0x0201, sample.HorizontalSpeed.SpeedBoostCounter,
        "acid preserves speed-boost timer/counter");
    AssertEqual((ushort)1, sample.HorizontalSpeed.ExtraRunSpeed,
        "acid preserves extra run speed");

    // `$9B:C4BE` has its own intentionally narrow definition of grapple liquid physics.
    // It ignores option bit two, samples only general FX Y, and clears on release-function
    // entry so the just-finished swing retains its prior flag for exactly one handler call.
    sample.EquippedItems = 0;
    sample.Grapple.Phase = GrapplePhase.ConnectedSwinging;
    sample.LiquidPhysics.ConfigureWater(surfaceY: 111, liquidOptions: 4);
    SamusGrappleMovement.RefreshLiquidPhysicsFlag(sample);
    AssertTrue(sample.Grapple.Submerged, "grapple liquid flag ignores water option bit two");
    sample.Grapple.Phase = GrapplePhase.ReleaseFromSwing;
    SamusGrappleMovement.RefreshLiquidPhysicsFlag(sample);
    AssertTrue(!sample.Grapple.Submerged, "grapple release function clears liquid flag");

    // Seed three distinguishable standalone `$90:9F31/$9F3D/$9F49` records. Mode two makes
    // the release handler subtract the selected fractional deceleration from 2.0000.
    int[] grappleReleaseRecords = [0x909f31, 0x909f3d, 0x909f49];
    ushort[] grappleReleaseDeceleration = [0x1000, 0x2000, 0x3000];
    for (int medium = 0; medium < grappleReleaseRecords.Length; medium++)
    {
        int address = grappleReleaseRecords[medium];
        for (int byteOffset = 0; byteOffset < SpeedTableEntry.ByteCount; byteOffset += 2)
            WriteTestWord(bus, address + byteOffset, 0);
        WriteTestWord(bus, address + 10, grappleReleaseDeceleration[medium]);
    }

    for (int medium = 0; medium < 3; medium++)
    {
        var releaseRoom = new RoomLevelData(
            16, 16,
            new ushort[16 * 16],
            new byte[16 * 16],
            new ushort[16 * 16],
            new byte[8]);
        var released = new SamusState
        {
            Pose = SamusState.NormalJumpForwardRightPose,
            XPosition = 128,
            YPosition = 128,
            Kinematics =
            {
                XRadius = 5,
                YRadius = 12,
                YDirection = 1,
                YSpeed = 1,
            },
        };
        bus.WriteBytes(0x91b629 + released.Pose * 8, [8, 2, 0xff, 0xff, 0, 0, 12, 0]);
        released.HorizontalSpeed.BaseSpeed = 2;
        released.Grapple.ReleasedMovementActive = true;
        if (medium == SamusLiquidPhysicsState.Water)
            released.LiquidPhysics.ConfigureWater(surfaceY: 127);
        else if (medium == SamusLiquidPhysicsState.LavaAcid)
            released.LiquidPhysics.ConfigureLavaAcid(surfaceY: 127);
        SamusAerialMovement.ConfigureEnvironmentGravity(bus, released);
        SamusAerialMovement.StepReleasedFromGrapple(
            bus,
            releaseRoom,
            released,
            controllerInput: 0,
            nmiFrameCounter: 0);
        AssertEqual(unchecked((ushort)(0 - grappleReleaseDeceleration[medium])),
            released.HorizontalSpeed.BaseSubspeed,
            $"grapple release medium {medium} standalone X record");
        AssertEqual((ushort)1, released.HorizontalSpeed.BaseSpeed,
            $"grapple release medium {medium} borrow into whole X speed");
        AssertTrue(released.Grapple.ReleasedMovementActive,
            $"grapple release medium {medium} handler survives before apex/collision");
    }

    // Give the spin routine authentic metadata plus zero horizontal records in all three
    // tables. Water's remembered medium lowers only the inclusive minimum from $0280 to $0080.
    bus.WriteBytes(0x91b629 + SamusState.SpaceJumpRightPose * 8,
        [8, 3, 0xff, 0xff, 0, 0, 12, 0]);
    WriteTestWord(bus, 0x91b010 + SamusState.SpaceJumpRightPose * 2, 0xc000);
    for (int frame = 0; frame < 32; frame++)
        bus.WriteByte(0x91c000 + frame, 4);
    foreach (int tableBase in new[] { 0x909f55, 0x90a08d, 0x90a1dd })
    {
        int spinEntry = tableBase + 3 * SpeedTableEntry.ByteCount;
        for (int byteOffset = 0; byteOffset < SpeedTableEntry.ByteCount; byteOffset += 2)
            WriteTestWord(bus, spinEntry + byteOffset, 0);
    }
    var empty = new RoomLevelData(
        16, 16,
        new ushort[16 * 16],
        new byte[16 * 16],
        new ushort[16 * 16],
        new byte[8]);

    var partialWaterSpaceJump = new SamusState
    {
        Pose = SamusState.SpaceJumpRightPose,
        EquippedItems = 0x0200,
        XPosition = 128,
        YPosition = 128,
        Kinematics =
        {
            XRadius = 5,
            YRadius = 12,
            YDirection = 2,
            YSpeed = 0,
            YSubspeed = 0x8000,
        },
    };
    partialWaterSpaceJump.LiquidPhysics.ConfigureWater(surfaceY: 128);
    partialWaterSpaceJump.LiquidPhysics.InitializeRememberedMedium(partialWaterSpaceJump);
    SamusAerialMovement.ConfigureEnvironmentGravity(bus, partialWaterSpaceJump);
    SamusAerialMovement.StepSpinJump(
        bus,
        empty,
        partialWaterSpaceJump,
        (ushort)SnesButton.A,
        nmiFrameCounter: 0,
        controllerNewInput: (ushort)SnesButton.A);
    AssertEqual((ushort)1, partialWaterSpaceJump.Kinematics.YDirection,
        "partially submerged Space Jump accepts water minimum $0080");
    AssertEqual((ushort)1, partialWaterSpaceJump.Kinematics.YSpeed,
        "water Space Jump reloads water launch whole speed");

    var fullySubmergedScrew = new SamusState
    {
        Pose = SamusState.ScrewAttackRightPose,
        EquippedItems = 0x0208,
        XPosition = 128,
        YPosition = 128,
        Kinematics =
        {
            XRadius = 5,
            YRadius = 12,
            YDirection = 2,
            YSpeed = 3,
        },
    };
    bus.WriteBytes(0x91b629 + SamusState.ScrewAttackRightPose * 8,
        [8, 3, 0xff, 0xff, 0, 0, 12, 0]);
    fullySubmergedScrew.LiquidPhysics.ConfigureWater(surfaceY: 100);
    fullySubmergedScrew.LiquidPhysics.InitializeRememberedMedium(fullySubmergedScrew);
    SamusAerialMovement.ConfigureEnvironmentGravity(bus, fullySubmergedScrew);
    SamusAerialMovement.StepSpinJump(
        bus,
        empty,
        fullySubmergedScrew,
        (ushort)SnesButton.A,
        nmiFrameCounter: 0,
        controllerNewInput: (ushort)SnesButton.A);
    AssertEqual((ushort)2, fullySubmergedScrew.Kinematics.YDirection,
        "fully submerged non-Gravity Space Jump cannot restart");
    AssertEqual((ushort)0, fullySubmergedScrew.HorizontalSpeed.ContactDamageIndex,
        "fully submerged Screw Attack does not publish contact damage");

    Console.WriteLine(
        "  Samus liquids: boundaries, ROM tables, gravity, Dash/lava cancellation, grapple, animation, Space Jump, and Gravity Suit agree.");
}

/// <summary>
/// Checks the producer, packed-slot interpreter, OAM path, sound publications, and periodic
/// fixed-point damage translated from `$90:8000-$82DB/$8A4C-$8C1E/$A3E5/$E9CE`.
/// </summary>
static void VerifySamusAtmosphericEffects()
{
    var bus = new TestAddressSpace();

    // Pose `$01` is sufficient to expose the literal movement type and X direction used by
    // all producer branches. Its delay metadata also lets the footstep fixture land exactly
    // on running frame two with timer one, matching `$90:A3EE-$A401`.
    bus.WriteBytes(0x91b629 + SamusState.FacingRightNormalPose * 8,
        [8, 1, 0xff, 0, 0, 0, 12, 0]);
    WriteTestWord(bus, 0x91b010 + SamusState.FacingRightNormalPose * 2, 0xc000);
    bus.WriteBytes(0x91c000, [1, 1, 1, 1]);

    // Seed only the bank-$90 table records exercised below. Distinct attributes and timers
    // prove the interpreter follows ROM pointers rather than a duplicated C# animation list.
    WriteTestWord(bus, 0x908b93 + 4 * 2, 0x9000);
    WriteTestWord(bus, 0x909000, 2);
    WriteTestWord(bus, 0x909002, 3);
    WriteTestWord(bus, 0x908bef + 4 * 2, 4);
    WriteTestWord(bus, 0x908bff + 4 * 2, 0x9100);
    WriteTestWord(bus, 0x909100, 0x2a48);
    WriteTestWord(bus, 0x909102, 0x2a49);

    var samus = new SamusState
    {
        Pose = SamusState.FacingRightNormalPose,
        XPosition = 100,
        YPosition = 100,
    };
    samus.RefreshCollisionRadii(bus);
    samus.InitializeAnimation(bus);

    // Movement type one is a diving splash according to the real `$81A4` table. Keep NMI
    // away from the 128-frame bubble cadence so entry has exactly one sound request.
    bus.WriteByte(0x9081a4 + 1, 0);
    samus.LiquidPhysics.ConfigureWater(surfaceY: 111);
    samus.LiquidPhysics.PrepareAnimationFrame(bus, samus, nmiFrameCounter: 1);
    SamusAtmosphericEffectSlot entrySplash = samus.LiquidPhysics.AtmosphericEffects.Slots[0];
    AssertEqual((byte)3, entrySplash.Type, "water entry selects diving-splash type");
    AssertEqual((ushort)2, entrySplash.AnimationTimer, "diving splash initial timer");
    AssertEqual((ushort)100, entrySplash.XPosition, "diving splash Samus X");
    AssertEqual((ushort)111, entrySplash.YPosition, "diving splash surface Y");
    AssertEqual(new SamusSoundRequest(2, 0x0d, 6),
        samus.LiquidPhysics.SoundRequests.Single(), "water-entry library-two sound");

    // Leaving water produces sound `$0E` and replaces the slot through the same movement-
    // type table. A spin/wall-jump-only `$30` request is correctly absent for type one.
    samus.LiquidPhysics.ConfigureWater(surfaceY: 112);
    samus.LiquidPhysics.PrepareAnimationFrame(bus, samus, nmiFrameCounter: 2);
    AssertEqual(new SamusSoundRequest(2, 0x0e, 6),
        samus.LiquidPhysics.SoundRequests.Single(), "water-exit library-two sound");

    // Bubble admission samples top-24, every 128th accepted NMI, and slot two availability.
    // Re-enter on frame one first so the cadence call below is an already-submerged pass.
    samus.LiquidPhysics.ConfigureWater(surfaceY: 50);
    samus.LiquidPhysics.PrepareAnimationFrame(bus, samus, nmiFrameCounter: 1);
    var bubbleSystem = new Bank80SystemState(0x0061);
    samus.LiquidPhysics.PrepareAnimationFrame(bus, samus, nmiFrameCounter: 128, bubbleSystem);
    SamusAtmosphericEffectSlot bubble = samus.LiquidPhysics.AtmosphericEffects.Slots[2];
    AssertEqual((byte)5, bubble.Type, "128-frame submerged cadence creates bubbles");
    AssertEqual((ushort)94, bubble.YPosition, "bubble origin is Samus top plus six");
    AssertTrue(samus.LiquidPhysics.SoundRequests.Any(request =>
        request.Library == 2 && request.SoundId is 0x0f or 0x11),
        "bubble RNG publishes one of the two native sounds");

    // Surface spray uses four type-four slots and exact asymmetric X positions. A half-unit
    // lava rate then borrows from fractional health on the same `$E9CE` consumer call.
    WriteTestWord(bus, 0x909e8b, 0x8000);
    WriteTestWord(bus, 0x909e8d, 0);
    samus.LiquidPhysics.ConfigureLavaAcid(surfaceY: 111);
    samus.Health = 99;
    samus.SubunitHealth = 0;
    samus.EquippedItems = 0;
    samus.LiquidPhysics.PrepareAnimationFrame(bus, samus, nmiFrameCounter: 1);
    AssertTrue(samus.LiquidPhysics.AtmosphericEffects.Slots.All(slot => slot.Type == 4),
        "partial lava submersion fills four surface-spray slots");
    AssertEqual((ushort)0x8000, samus.LiquidPhysics.PeriodicSubDamage,
        "lava fractional damage accumulates from ROM");
    samus.LiquidPhysics.ApplyPeriodicDamage(samus, timeIsFrozen: false);
    AssertEqual((ushort)98, samus.Health, "fractional lava subtraction borrows one energy");
    AssertEqual((ushort)0x8000, samus.SubunitHealth, "fractional energy retains wrapped half");
    AssertEqual((ushort)0, samus.LiquidPhysics.PeriodicSubDamage,
        "periodic consumer clears fractional accumulator");

    // Type-four slot zero begins with timer two. First update decrements/draws frame zero and
    // applies +1/-1 motion before clipping. The next call reloads ROM timer zero, advances to
    // frame one, and consumes its distinct `$2A49` attribute word.
    var effects = new SamusAtmosphericEffectsState();
    effects.SetSlot(0, type: 4, animationFrame: 0, animationTimer: 2, worldX: 104, worldY: 104);
    var oam = new OamBuffer();
    oam.BeginFrame();
    effects.UpdateAndDraw(bus, oam, cameraX: 0, cameraY: 0, fxYPosition: 0);
    AssertEqual((ushort)1, effects.Slots[0].AnimationTimer, "atmospheric positive timer decrements");
    AssertEqual((ushort)105, effects.Slots[0].XPosition, "lava slot zero drifts right");
    AssertEqual((ushort)103, effects.Slots[0].YPosition, "lava spray rises one pixel");
    AssertEqual(0x048, oam.GetEntry(0).TileNumber, "lava frame zero direct OAM tile");
    AssertEqual(5, oam.GetEntry(0).Palette, "lava direct OAM retains ROM palette");

    oam.BeginFrame();
    effects.UpdateAndDraw(bus, oam, cameraX: 0, cameraY: 0, fxYPosition: 0);
    AssertEqual((byte)1, effects.Slots[0].AnimationFrame,
        "timer zero reloads old frame then advances packed word");
    AssertEqual((ushort)2, effects.Slots[0].AnimationTimer,
        "frame advance reloads literal ROM duration");
    AssertEqual(0x049, oam.GetEntry(0).TileNumber, "lava frame one direct OAM tile");

    // `$8002` is not a large positive delay: one call reaches `$8001` and does nothing;
    // the next reaches `$8000`, reloads the current ROM duration, moves, and draws.
    effects.Clear();
    effects.SetSlot(1, type: 4, animationFrame: 0, animationTimer: 0x8002, worldX: 100, worldY: 100);
    oam.BeginFrame();
    effects.UpdateAndDraw(bus, oam, 0, 0, 0);
    AssertEqual((ushort)100, effects.Slots[1].XPosition, "$8001 suppresses atmospheric motion");
    AssertEqual(0, oam.NextByteOffset, "$8001 suppresses atmospheric drawing");
    effects.UpdateAndDraw(bus, oam, 0, 0, 0);
    AssertEqual((ushort)101, effects.Slots[1].XPosition, "$8000 reload call begins motion");
    AssertEqual(4, oam.NextByteOffset, "$8000 reload call emits one OAM record");

    // Running frame two with stage four produces dry-room type-seven dust. The first slot
    // is delayed and the second immediate, exactly like `$90:EE64-$EEE3`.
    var runner = new SamusState
    {
        Pose = SamusState.FacingRightNormalPose,
        XPosition = 100,
        YPosition = 100,
    };
    runner.RefreshCollisionRadii(bus);
    runner.InitializeAnimation(bus, initialFrame: 2);
    runner.HorizontalSpeed.SpeedBoostCounter = 0x0400;
    bus.WriteByte(0x90a424 + 2, 1);
    runner.LiquidPhysics.PrepareAnimationFrame(bus, runner, nmiFrameCounter: 1);
    AssertEqual((byte)7, runner.LiquidPhysics.AtmosphericEffects.Slots[0].Type,
        "boost-stage-four foot contact creates dust");
    AssertEqual((ushort)0x8002,
        runner.LiquidPhysics.AtmosphericEffects.Slots[0].AnimationTimer,
        "first foot effect uses delayed-start sentinel");
    AssertEqual((ushort)3,
        runner.LiquidPhysics.AtmosphericEffects.Slots[1].AnimationTimer,
        "second foot effect starts immediately");

    // `$91:F046` runs before animation and before landing clears Y velocity. Norfair selects
    // type-six dust in slots two/three; spin termination precedes the soft-impact request in
    // the native sound queues. The source pose is deliberately ordinary spin rather than a
    // landing pose because `$0A20` has not been replaced yet at this collision seam.
    var landing = new SamusState
    {
        Pose = 0x19,
        XPosition = 100,
        YPosition = 100,
    };
    bus.WriteBytes(0x91b629 + 0x19 * 8, [8, 3, 0xff, 0xff, 0, 0, 12, 0]);
    landing.RefreshCollisionRadii(bus);
    landing.Kinematics.YSpeed = 4;
    landing.Kinematics.YSubspeed = 0;
    landing.LiquidPhysics.AreaIndex = 2;
    landing.LiquidPhysics.BeginFrameSoundRequests();
    landing.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(
        bus, landing, previousMovementType: 3, previousPose: 0x19,
        landing.Kinematics.YSpeed, landing.Kinematics.YSubspeed);
    AssertEqual(2, landing.LiquidPhysics.SoundRequests.Count,
        "spin landing publishes termination and impact sounds");
    AssertEqual(new SamusSoundRequest(1, 0x32, 6),
        landing.LiquidPhysics.SoundRequests[0], "ordinary spin termination sound");
    AssertEqual(new SamusSoundRequest(3, 0x05, 6),
        landing.LiquidPhysics.SoundRequests[1], "sub-five soft landing sound");
    AssertEqual((byte)6, landing.LiquidPhysics.AtmosphericEffects.Slots[2].Type,
        "Norfair landing creates right dust slot");
    AssertEqual((byte)6, landing.LiquidPhysics.AtmosphericEffects.Slots[3].Type,
        "Norfair landing creates left dust slot");
    AssertEqual((ushort)108, landing.LiquidPhysics.AtmosphericEffects.Slots[2].XPosition,
        "landing dust right X offset");
    AssertEqual((ushort)92, landing.LiquidPhysics.AtmosphericEffects.Slots[3].XPosition,
        "landing dust left X offset");
    AssertEqual((ushort)112, landing.LiquidPhysics.AtmosphericEffects.Slots[2].YPosition,
        "landing dust uses current bottom boundary");

    // Whole speed five changes only the impact sound to hard `$04`; Screw Attack changes
    // only the preceding library-one termination to `$34`. A cinematic suppresses both
    // sounds but Norfair's graphics handler still creates dust, exactly as the separate
    // native checks dictate.
    landing.Kinematics.YSpeed = 5;
    landing.LiquidPhysics.BeginFrameSoundRequests();
    landing.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(
        bus, landing, previousMovementType: 0x14, previousPose: 0x81,
        landing.Kinematics.YSpeed, landing.Kinematics.YSubspeed);
    AssertEqual(new SamusSoundRequest(1, 0x34, 6),
        landing.LiquidPhysics.SoundRequests[0], "Screw Attack termination sound");
    AssertEqual(new SamusSoundRequest(3, 0x04, 6),
        landing.LiquidPhysics.SoundRequests[1], "speed-five hard landing sound");
    landing.LiquidPhysics.CinematicFunctionActive = true;
    landing.LiquidPhysics.BeginFrameSoundRequests();
    landing.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(
        bus, landing, previousMovementType: 3, previousPose: 0x19,
        landing.Kinematics.YSpeed, landing.Kinematics.YSubspeed);
    AssertEqual(0, landing.LiquidPhysics.SoundRequests.Count,
        "cinematic landing suppresses both sound libraries");
    AssertEqual((byte)6, landing.LiquidPhysics.AtmosphericEffects.Slots[2].Type,
        "Norfair cinematic still dispatches landing dust");
    landing.LiquidPhysics.CinematicFunctionActive = false;

    // Active liquid returns without touching the landing slots. This is intentionally not
    // deletion: seed an unrelated type-seven record and prove its packed word survives.
    landing.LiquidPhysics.AtmosphericEffects.SetSlot(
        2, type: 7, animationFrame: 2, animationTimer: 9, worldX: 77, worldY: 88);
    landing.LiquidPhysics.ConfigureWater(surfaceY: 111);
    landing.LiquidPhysics.BeginFrameSoundRequests();
    landing.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(
        bus, landing, previousMovementType: 6, previousPose: 0x29,
        landing.Kinematics.YSpeed, landing.Kinematics.YSubspeed);
    AssertEqual((byte)7, landing.LiquidPhysics.AtmosphericEffects.Slots[2].Type,
        "submerged landing leaves prior atmospheric slot untouched");
    AssertEqual((ushort)9, landing.LiquidPhysics.AtmosphericEffects.Slots[2].AnimationTimer,
        "submerged return does not clear stale timer");

    // Ceres/debug deletion writes only frame/type. Position and timer are observable stale
    // WRAM. Zero vertical speed returns even earlier and therefore does not delete anything.
    landing.LiquidPhysics.AreaIndex = 6;
    landing.LiquidPhysics.FxYPosition = ushort.MaxValue;
    landing.Kinematics.YSpeed = 1;
    landing.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(
        bus, landing, previousMovementType: 6, previousPose: 0x29,
        landing.Kinematics.YSpeed, landing.Kinematics.YSubspeed);
    AssertEqual((ushort)0, landing.LiquidPhysics.AtmosphericEffects.Slots[2].FrameAndType,
        "Ceres deletes landing packed word");
    AssertEqual((ushort)9, landing.LiquidPhysics.AtmosphericEffects.Slots[2].AnimationTimer,
        "Ceres delete preserves atmospheric timer");
    AssertEqual((ushort)77, landing.LiquidPhysics.AtmosphericEffects.Slots[2].XPosition,
        "Ceres delete preserves atmospheric X");
    landing.LiquidPhysics.AtmosphericEffects.SetSlot(
        2, type: 7, animationFrame: 0, animationTimer: 3, worldX: 1, worldY: 2);
    landing.Kinematics.YSpeed = 0;
    landing.Kinematics.YSubspeed = 0;
    landing.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(
        bus, landing, previousMovementType: 6, previousPose: 0x29,
        landing.Kinematics.YSpeed, landing.Kinematics.YSubspeed);
    AssertEqual((byte)7, landing.LiquidPhysics.AtmosphericEffects.Slots[2].Type,
        "zero-speed grounding returns before graphics dispatch");

    // Crateria reads its literal inline room byte. Landing Site flag one requires exact FX
    // type `$000A`; with a dry surface it creates type-one splashes at +4/-3 and bottom-4.
    bus.WriteByte(0x91f0f3, 1);
    landing.LiquidPhysics.AreaIndex = 0;
    landing.LiquidPhysics.RoomIndex = 0;
    landing.LiquidPhysics.FxType = 0x000a;
    landing.LiquidPhysics.FxYPosition = ushort.MaxValue;
    landing.Kinematics.YSubspeed = 1;
    landing.LiquidPhysics.HandleLandingSoundEffectsAndGraphics(
        bus, landing, previousMovementType: 6, previousPose: 0x29,
        landing.Kinematics.YSpeed, landing.Kinematics.YSubspeed);
    AssertEqual((byte)1, landing.LiquidPhysics.AtmosphericEffects.Slots[2].Type,
        "Landing Site type-A FX selects splash");
    AssertEqual((ushort)104, landing.LiquidPhysics.AtmosphericEffects.Slots[2].XPosition,
        "landing splash right X offset");
    AssertEqual((ushort)97, landing.LiquidPhysics.AtmosphericEffects.Slots[3].XPosition,
        "landing splash left X offset");
    AssertEqual((ushort)108, landing.LiquidPhysics.AtmosphericEffects.Slots[2].YPosition,
        "landing splash rises four pixels above feet");

    Console.WriteLine(
        "  Samus atmosphere: liquid FX, footsteps, landing impact, packed timers, sound, and OAM agree.");
}

/// <summary>
/// Verifies the cartridge's complete ten-way jump/fall turn selectors and the block-only
/// wall-jump route. Every expectation below is a literal bank-$90/$91 table value; the test
/// intentionally does not calculate a mirrored target from facing.
/// </summary>
static void VerifySamusAerialTurnsAndWallJump()
{
    var bus = new TestAddressSpace();

    const int width = 8;
    const int height = 8;
    var foreground = new ushort[width * height];
    for (int x = 0; x < width; x++)
        foreground[6 * width + x] = 0x8000;
    for (int y = 1; y < 6; y++)
        foreground[y * width + 4] = 0x8000; // vertical wall begins at world X=64
    var level = new RoomLevelData(
        width,
        height,
        foreground,
        new byte[foreground.Length],
        new ushort[foreground.Length],
        new byte[8]);

    // Source poses are real retail pose numbers covering shot directions zero through nine.
    // Compact down-aim records `$17/$18/$2D/$2E` use radius ten; every other source uses 19.
    byte[] jumpSources = [0x15, 0x69, 0x51, 0x6b, 0x17, 0x18, 0x6c, 0x52, 0x6a, 0x16];
    byte[] fallSources = [0x2b, 0x6d, 0x29, 0x6f, 0x2d, 0x2e, 0x70, 0x2a, 0x6e, 0x2c];
    byte[] jumpTargets = [0x8f, 0x9e, 0x2f, 0x91, 0x91, 0x92, 0x92, 0x30, 0x9f, 0x90];
    byte[] fallTargets = [0x93, 0xa0, 0x87, 0x95, 0x95, 0x96, 0x96, 0x88, 0xa1, 0x94];

    for (int direction = 0; direction < 10; direction++)
    {
        VerifySelector(jumping: true, jumpSources[direction], jumpTargets[direction], direction);
        VerifySelector(jumping: false, fallSources[direction], fallTargets[direction], direction);
    }

    void VerifySelector(bool jumping, byte sourcePose, byte expectedPose, int shotDirection)
    {
        bool sourceFacesLeft = shotDirection >= 5;
        byte sourceXDirection = sourceFacesLeft ? (byte)4 : (byte)8;
        byte sourceRadius = shotDirection is 4 or 5 ? (byte)10 : (byte)19;
        byte sourceMovementType = jumping ? (byte)2 : (byte)6;
        bus.WriteBytes(0x91b629 + sourcePose * 8, [
            sourceXDirection, sourceMovementType, 0xff, (byte)shotDirection,
            0, 0, sourceRadius, 0,
        ]);

        bool targetFacesLeft = shotDirection < 5;
        byte targetXDirection = targetFacesLeft ? (byte)4 : (byte)8;
        byte targetMovementType = jumping ? (byte)0x17 : (byte)0x18;
        bus.WriteBytes(0x91b629 + expectedPose * 8, [
            targetXDirection, targetMovementType, 0xff, 0xfb,
            8, 0, 19, 0,
        ]);
        WriteTestWord(bus, 0x91b010 + expectedPose * 2, 0xc000);
        bus.WriteBytes(0x91c000, [2]);

        var samus = new SamusState
        {
            Pose = sourcePose,
            XPosition = 32,
            YPosition = 48,
        };
        samus.RefreshCollisionRadii(bus);
        samus.HorizontalSpeed.BaseSpeed = 1;
        samus.HorizontalSpeed.ExtraRunSubspeed = 0x8000;
        byte genericTarget = jumping
            ? targetFacesLeft ? SamusState.TurningRightToLeftJumpPose : SamusState.TurningLeftToRightJumpPose
            : targetFacesLeft ? SamusState.TurningRightToLeftFallingPose : SamusState.TurningLeftToRightFallingPose;
        AssertTrue(
            samus.TryApplyAerialTurn(bus, level, genericTarget, nmiFrameCounter: 0),
            $"{(jumping ? "jump" : "fall")} turn direction {shotDirection} fits");
        AssertEqual(expectedPose, samus.Pose, $"{(jumping ? "jump" : "fall")} selector direction {shotDirection}");
        AssertEqual(0x00018000u, samus.HorizontalSpeed.BaseFixed, "aerial selector folds extra speed");
        AssertEqual((ushort)1, samus.HorizontalSpeed.AccelerationMode, "aerial selector starts turn mode");
    }

    // Isolate one diagonal-up jumping turn with its real three-frame `$F8,$6A` stream.
    bus.WriteBytes(0x91b629 + 0x69 * 8, [8, 2, 0xff, 1, 8, 0, 19, 0]);
    bus.WriteBytes(0x91b629 + 0x9e * 8, [4, 0x17, 0xff, 0xfb, 8, 0, 19, 0]);
    bus.WriteBytes(0x91b629 + 0x6a * 8, [4, 2, 0xff, 8, 8, 0, 19, 0]);
    WriteTestWord(bus, 0x91b010 + 0x9e * 2, 0xc100);
    WriteTestWord(bus, 0x91b010 + 0x6a * 2, 0xc110);
    bus.WriteBytes(0x91c100, [2, 2, 2, 0xf8, 0x6a]);
    bus.WriteBytes(0x91c110, [3]);
    WriteSpeedRecord(bus, movementType: 0x17, accelerationSub: 0, maximumSpeed: 2, decelerationSub: 0x1000);
    WriteTestWord(bus, 0x909ea1, 0x2800);
    WriteTestWord(bus, 0x909ea7, 0);
    var turn = new SamusState { Pose = 0x69, XPosition = 32, YPosition = 48 };
    turn.RefreshCollisionRadii(bus);
    turn.HorizontalSpeed.BaseSpeed = 1;
    turn.HorizontalSpeed.ExtraRunSubspeed = 0x8000;
    turn.Kinematics.YDirection = 1;
    turn.Kinematics.YSpeed = 2;
    turn.Kinematics.YSubacceleration = 0x2800;
    AssertTrue(turn.TryApplyAerialTurn(bus, level, 0x2f, 0), "diagonal-up aerial turn installs");
    AerialMovementResult turnFrame = SamusAerialMovement.StepTurningInAir(bus, level, turn, 0);
    AssertEqual(0x00017000, turnFrame.Horizontal.AcceptedDisplacement, "turn retains old rightward momentum");
    AssertEqual((ushort)33, turn.XPosition, "turn moves in old direction despite new facing");
    for (int tick = 0; tick < 6; tick++)
        turn.AnimateNoFx(bus);
    AssertEqual((byte)0xf8, turn.LastAnimationDelayCommand!.Value, "aerial turn reaches F8");
    AssertTrue(turn.ApplyPendingVerifiedAnimationTransition(bus), "aerial turn F8 applies");
    AssertEqual((byte)0x6a, turn.Pose, "aerial turn preserves diagonal-up aim endpoint");

    // Ordinary spin art, wall-jump art, and both dry launch table pairs.
    bus.WriteBytes(0x91b629 + 0x19 * 8, [8, 3, 0xff, 0xff, 0, 0, 12, 0]);
    bus.WriteBytes(0x91b629 + 0x83 * 8, [8, 0x14, 0x19, 0xff, 8, 0, 19, 0]);
    WriteTestWord(bus, 0x91b010 + 0x19 * 2, 0xc200);
    WriteTestWord(bus, 0x91b010 + 0x83 * 2, 0xc220);
    bus.WriteBytes(0x91c200, [2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 2, 0xff]);
    bus.WriteBytes(0x91c220, [4, 4, 0xfb, 2, 2, 2, 2, 2, 2, 2, 2, 0xfe, 8]);
    WriteSpeedRecord(bus, movementType: 3, accelerationSub: 0x2000, maximumSpeed: 1, decelerationSub: 0x1000);
    WriteSpeedRecord(bus, movementType: 0x14, accelerationSub: 0x1000, maximumSpeed: 1, decelerationSub: 0x1000);
    WriteTestWord(bus, 0x909ed1, 4);
    WriteTestWord(bus, 0x909ed7, 0xa000);

    var earlyContact = CreateSpinSamus(animationFrame: 0);
    AerialMovementResult contactFrame = SamusAerialMovement.StepSpinJump(
        bus, level, earlyContact, (ushort)(SnesButton.Left | SnesButton.A), 0, 0);
    AssertTrue(contactFrame.WallContact && !contactFrame.WallJumpTriggered, "early wall chord contacts without launch");
    AssertEqual((ushort)0x0a, earlyContact.AnimationFrame, "early wall contact rewinds to frame A");

    var eligible = CreateSpinSamus(animationFrame: 0x0b);
    ushort beforeTriggerY = eligible.YPosition;
    AerialMovementResult triggerFrame = SamusAerialMovement.StepSpinJump(
        bus,
        level,
        eligible,
        (ushort)(SnesButton.Left | SnesButton.A),
        0,
        (ushort)SnesButton.A);
    AssertTrue(triggerFrame.WallJumpTriggered, "eligible fresh jump press triggers wall jump");
    AssertTrue(triggerFrame.Vertical is null, "wall trigger carry skips vertical movement");
    AssertEqual(beforeTriggerY, eligible.YPosition, "wall trigger frame preserves Y");
    AssertEqual((ushort)7, triggerFrame.WallDistance, "wall trigger reports clipped seven-pixel distance");

    // Repeat the same eligible chord with a native solid-enemy snapshot in front of the
    // terrain. `$90:9E64` must publish that exact slot for enemy AI's shake response; a
    // terrain-backed wall jump deliberately does not write this word.
    var enemyEligible = CreateSpinSamus(animationFrame: 0x0b);
    enemyEligible.Kinematics.InteractiveEnemies =
    [
        new SolidEnemyCollisionBody(
            Index: 0x0240,
            XPosition: 69,
            YPosition: 48,
            XRadius: 5,
            YRadius: 5,
            FreezeTimer: 0,
            Properties: 0x8000),
    ];
    AerialMovementResult enemyTrigger = SamusAerialMovement.StepSpinJump(
        bus,
        level,
        enemyEligible,
        (ushort)(SnesButton.Left | SnesButton.A),
        0,
        (ushort)SnesButton.A);
    AssertTrue(enemyTrigger.WallJumpTriggered, "solid enemy can trigger ordinary wall jump");
    AssertEqual((ushort)7, enemyTrigger.WallDistance, "enemy wall jump retains directional gap");
    AssertEqual((ushort)0x0240, enemyEligible.EnemyIndexToShake,
        "enemy wall jump publishes contacted slot for shake");

    // Bank $91:F2D3 clears only base speed; bank $90:9949 installs Y launch speed and
    // likewise leaves the Dash pair/flag alone. Seed a visible fractional value here so
    // an over-broad wall-jump cleanup cannot masquerade as a harmless zero-state write.
    eligible.HorizontalSpeed.ExtraRunSpeed = 1;
    eligible.HorizontalSpeed.ExtraRunSubspeed = 0x7000;
    eligible.HorizontalSpeed.HasRunningMomentum = true;
    eligible.LiquidPhysics.BeginFrameSoundRequests();
    eligible.ApplyWallJumpTrigger(bus);
    AssertEqual((byte)0x83, eligible.Pose, "right-facing spin selects right wall-jump pose");
    AssertEqual((ushort)4, eligible.Kinematics.YSpeed, "wall jump reads whole launch speed");
    AssertEqual((ushort)0xa000, eligible.Kinematics.YSubspeed, "wall jump reads fractional launch speed");
    AssertEqual((ushort)1, eligible.HorizontalSpeed.ExtraRunSpeed, "wall jump preserves Dash whole speed");
    AssertEqual((ushort)0x7000, eligible.HorizontalSpeed.ExtraRunSubspeed, "wall jump preserves Dash fraction");
    AssertTrue(eligible.HorizontalSpeed.HasRunningMomentum, "wall jump preserves Dash momentum flag");
    AssertEqual(new SamusSoundRequest(3, 0x05, 6), eligible.LiquidPhysics.SoundRequests.Single(),
        "ordinary wall trigger queues library-three sound five max six");
    for (int tick = 0; tick < 8; tick++)
        eligible.AnimateNoFx(bus);
    AssertEqual((byte)0xfb, eligible.LastAnimationDelayCommand!.Value, "wall animation reaches FB");
    AssertEqual((ushort)3, eligible.AnimationFrame, "ordinary dry wall animation selects frame three");

    eligible.ProjectileFlareCounter = 0x003c;
    ushort wallStartY = eligible.YPosition;
    AerialMovementResult wallFrame = SamusAerialMovement.StepWallJump(
        bus,
        level,
        eligible,
        (ushort)(SnesButton.Right | SnesButton.A),
        1);
    AssertTrue(wallFrame.Vertical is { Collided: false }, "wall launch remains airborne");
    AssertEqual((ushort)(wallStartY - 5), eligible.YPosition, "wall launch moves by old 4.A000 speed");
    AssertEqual((ushort)4, eligible.HorizontalSpeed.ContactDamageIndex,
        "charged wall-jump frames three through 22 publish damage index four");

    eligible.HorizontalSpeed.ContactDamageIndex = 0;
    eligible.AnimationFrame = 0x17;
    SamusAerialMovement.StepWallJump(
        bus,
        level,
        eligible,
        (ushort)(SnesButton.Right | SnesButton.A),
        2);
    AssertEqual((ushort)3, eligible.HorizontalSpeed.ContactDamageIndex,
        "wall-jump frame 23 publishes Screw-style damage index three");

    SamusState CreateSpinSamus(ushort animationFrame)
    {
        var samus = new SamusState { Pose = 0x19, XPosition = 52, YPosition = 48 };
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus, 0);
        samus.AnimationFrame = animationFrame;
        samus.Kinematics.YDirection = 1;
        samus.Kinematics.YSpeed = 2;
        samus.Kinematics.YSubacceleration = 0x2800;
        return samus;
    }

    void WriteSpeedRecord(
        TestAddressSpace addressSpace,
        byte movementType,
        ushort accelerationSub,
        ushort maximumSpeed,
        ushort decelerationSub)
    {
        int address = 0x909f55 + movementType * 12;
        WriteTestWord(addressSpace, address + 0, 0);
        WriteTestWord(addressSpace, address + 2, accelerationSub);
        WriteTestWord(addressSpace, address + 4, maximumSpeed);
        WriteTestWord(addressSpace, address + 6, 0);
        WriteTestWord(addressSpace, address + 8, 0);
        WriteTestWord(addressSpace, address + 10, decelerationSub);
    }

    Console.WriteLine("  Samus aerial turns/wall jump: selectors, momentum, sounds, contact damage, wall gate, FB, and launch agree.");
}

/// <summary>
/// Verifies command-seven bottom alignment, movement types five/$0F, $FD completion, and
/// the rejected stand-up case where ceiling and floor leave room for crouch but not stand.
/// </summary>
/// <summary>
/// Verifies the dry-air, ordinary-body branch of the retail hurt handler and its hidden
/// damage-boost exit. The fixture bytes below are literal records from banks $90/$91;
/// the production path still reads them through <see cref="ISnesAddressSpace"/> instead of
/// embedding friendly host-side velocities or pose metadata.
/// </summary>
static void VerifySamusKnockbackAndDamageBoost()
{
    var bus = new TestAddressSpace();

    // Pose definitions live at `$91:B629 + pose * 8`. Only the fields consumed by this
    // slice are nonzero: X direction, movement type, vertical graphics offset, and radius.
    // `$53/$54` are type-$0A hurt poses; `$4F/$50` are type-$19 damage-boost poses.
    bus.WriteBytes(0x91b631, [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]); // $01
    bus.WriteBytes(0x91b771, [0x08, 0x06, 0xff, 0x07, 0x08, 0x00, 0x13, 0x00]); // $29
    bus.WriteBytes(0x91b891, [0x08, 0x02, 0xff, 0x02, 0x08, 0x00, 0x13, 0x00]); // $4D
    bus.WriteBytes(0x91b8a1, [0x08, 0x19, 0x4e, 0xff, 0x08, 0x00, 0x13, 0x00]); // $4F
    bus.WriteBytes(0x91b8a9, [0x04, 0x19, 0x4d, 0xff, 0x08, 0x00, 0x13, 0x00]); // $50
    bus.WriteBytes(0x91b8c1, [0x08, 0x0a, 0xff, 0xff, 0x06, 0x00, 0x15, 0x00]); // $53
    bus.WriteBytes(0x91b8c9, [0x04, 0x0a, 0xff, 0xff, 0x06, 0x00, 0x15, 0x00]); // $54
    bus.WriteBytes(0x91bb51, [0x04, 0x00, 0xff, 0x07, 0x03, 0x00, 0x15, 0x00]); // $A5

    // One representative pose for each retail Morph/Spring movement type admitted by
    // `$90:DF15/$91:EE27`. Alternating facings make the pose-direction-only knockback
    // selection independently observable from the enemy's X-side word.
    bus.WriteBytes(0x91b629 + SamusState.MorphBallGroundRightPose * 8,
        [0x08, 0x04, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    bus.WriteBytes(0x91b629 + SamusState.MorphBallFallingLeftPose * 8,
        [0x04, 0x08, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    bus.WriteBytes(0x91b629 + SamusState.SpringBallGroundRightPose * 8,
        [0x08, 0x11, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    bus.WriteBytes(0x91b629 + SamusState.SpringBallJumpLeftPose * 8,
        [0x04, 0x12, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    bus.WriteBytes(0x91b629 + SamusState.SpringBallFallingRightPose * 8,
        [0x08, 0x13, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);

    // Animation-pointer table entries are `$91:B010 + pose * 2`. One long ordinary
    // delay is enough to keep animation unrelated to this movement-focused assertion.
    WriteTestWord(bus, 0x91b062, 0xc100); // $29
    WriteTestWord(bus, 0x91b0ae, 0xc110); // $4F
    WriteTestWord(bus, 0x91b0b0, 0xc120); // $50
    WriteTestWord(bus, 0x91b0b6, 0xc130); // $53
    WriteTestWord(bus, 0x91b0b8, 0xc140); // $54
    WriteTestWord(bus, 0x91b15a, 0xc150); // $A5
    bus.WriteBytes(0x91c100, [0x10]);
    bus.WriteBytes(0x91c110, [0x08]);
    bus.WriteBytes(0x91c120, [0x08]);
    bus.WriteBytes(0x91c130, [0x02]);
    bus.WriteBytes(0x91c140, [0x02]);
    bus.WriteBytes(0x91c150, [0x04]);

    // Every ball pose shares this synthetic rolling stream. Starting at frame three proves
    // the same-pose knockback transition does not accidentally call InitializeAnimation.
    foreach (byte pose in new byte[]
    {
        SamusState.MorphBallGroundRightPose,
        SamusState.MorphBallFallingLeftPose,
        SamusState.SpringBallGroundRightPose,
        SamusState.SpringBallJumpLeftPose,
        SamusState.SpringBallFallingRightPose,
    })
    {
        WriteTestWord(bus, 0x91b010 + pose * 2, 0xc160);
    }
    bus.WriteBytes(0x91c160, [0x09, 0x09, 0x09, 0x09, 0x09, 0x09, 0xff]);

    // `$90:99D6` selects dry-air knockback magnitude 5.0000. Damage boost subsequently
    // calls Make_Samus_Jump, whose independent dry-air value is 4.E000. Both share the
    // same 0.2800 gravity record in this no-water/no-lava fixture.
    WriteTestWord(bus, 0x909ee9, 0x0005);
    WriteTestWord(bus, 0x909eef, 0x0000);
    WriteTestWord(bus, 0x909eb9, 0x0004);
    WriteTestWord(bus, 0x909ebf, 0xe000);
    WriteTestWord(bus, 0x909ea1, 0x2800);
    WriteTestWord(bus, 0x909ea7, 0x0000);

    // Knockback's type-$0A normal-air record begins at `$90:9FCD`. A small 0.4000
    // acceleration makes the first frame's direction and fixed-point displacement exact.
    WriteTestWord(bus, 0x909fcd, 0x0000);
    WriteTestWord(bus, 0x909fcf, 0x4000);
    WriteTestWord(bus, 0x909fd1, 0x0005);
    WriteTestWord(bus, 0x909fd3, 0x0000);
    WriteTestWord(bus, 0x909fd5, 0x0000);
    WriteTestWord(bus, 0x909fd7, 0x1000);

    // Type `$19` really indexes the same generic table rather than using a damage-boost
    // special case. Its entry may remain zero because no direction is held in the first
    // translated damage-boost frame below.
    for (int address = 0x90a081; address < 0x90a08d; address += 2)
        WriteTestWord(bus, address, 0);

    const int width = 16;
    const int height = 16;
    var foreground = new ushort[width * height];
    var empty = new RoomLevelData(
        width,
        height,
        foreground,
        new byte[foreground.Length],
        new ushort[foreground.Length],
        new byte[8]);

    var samus = new SamusState
    {
        Pose = SamusState.FacingRightNormalPose,
        XPosition = 96,
        YPosition = 96,
    };

    // A source to Samus's left publishes X direction one (move right). With no forward
    // input, `$91:EDB0` chooses up-right direction two and `$90:99D6` installs 5.0000.
    SamusKnockbackMovement.Start(bus, samus, controllerInput: 0, knockbackXDirection: 1);
    AssertEqual(SamusState.KnockbackRightPose, samus.Pose, "right-facing knockback pose");
    AssertEqual((ushort)2, samus.KnockbackDirection, "up-right knockback direction");
    AssertEqual((ushort)1, samus.KnockbackXDirection, "knockback X direction publication");
    AssertEqual((ushort)5, samus.KnockbackTimer, "enemy hurt timer publication");
    AssertTrue(samus.KnockbackActive, "special knockback handler installed");
    AssertEqual((ushort)5, samus.Kinematics.YSpeed, "knockback dry-air whole speed");
    AssertEqual((ushort)0, samus.Kinematics.YSubspeed, "knockback dry-air subspeed");

    KnockbackMovementResult hurtFrame = SamusKnockbackMovement.Step(bus, empty, samus, 0);
    // Movement consumes the current timer; gameplay state eight then calls `$A0:9169`
    // after drawing/room work. Keep that distinct owner visible in this direct subsystem test.
    samus.DecrementHurtTimers();
    AssertEqual((ushort)4, samus.KnockbackTimer, "first hurt frame decrements timer");
    AssertEqual(0x00004000, hurtFrame.Horizontal!.Value.AcceptedDisplacement,
        "knockback moves in bank-$A0 X direction");
    AssertEqual(unchecked((int)0xfffb0000), hurtFrame.Vertical!.Value.AcceptedDisplacement,
        "knockback moves by old 5.0000 vertical speed");
    AssertEqual((ushort)91, samus.YPosition, "knockback upward whole position");
    AssertEqual((ushort)4, samus.Kinematics.YSpeed, "knockback gravity next whole speed");
    AssertEqual((ushort)0xd800, samus.Kinematics.YSubspeed, "knockback gravity next subspeed");

    // Held forward selects down-right direction four. Place the radius-21 body flush with a
    // square floor so `$90:923F`'s no-speed down probe collides immediately. The special
    // handler clears live velocity after collision, while its result must retain the 5.0000
    // magnitude seen by `$91:F078` for hard-landing sound selection.
    var downForeground = new ushort[width * height];
    for (int x = 0; x < width; x++)
        downForeground[6 * width + x] = 0x8000;
    var floorLevel = new RoomLevelData(
        width,
        height,
        downForeground,
        new byte[downForeground.Length],
        new ushort[downForeground.Length],
        new byte[8]);
    var downKnockback = new SamusState
    {
        Pose = SamusState.FacingRightNormalPose,
        XPosition = 96,
        YPosition = 75,
    };
    downKnockback.RefreshCollisionRadii(bus);
    SamusKnockbackMovement.Start(
        bus,
        downKnockback,
        controllerInput: (ushort)SnesButton.Right,
        knockbackXDirection: 0);
    KnockbackMovementResult downImpact = SamusKnockbackMovement.Step(
        bus, floorLevel, downKnockback, nmiFrameCounter: 0);
    AssertTrue(downImpact.Landed, "downward knockback collision publishes landing");
    AssertEqual((ushort)5, downImpact.ImpactYSpeed,
        "downward knockback retains pre-clear whole impact speed");
    AssertEqual((ushort)0, downImpact.ImpactYSubspeed,
        "downward knockback retains pre-clear fractional impact speed");
    AssertEqual((ushort)0, downKnockback.Kinematics.YSpeed,
        "downward knockback clears live whole speed after snapshot");

    // `$53` plus Left+Jump (`$0280`) selects `$50`. The pose-family crossing runs the native normal
    // input initializer, clears the special handler, and starts a fresh 4.E000 jump.
    SamusKnockbackMovement.ApplyDamageBoostTransition(
        bus,
        samus,
        SamusState.DamageBoostRightPose);
    AssertEqual(SamusState.DamageBoostRightPose, samus.Pose, "damage-boost entry pose");
    AssertEqual((byte)0x19, samus.ReadMovementType(bus), "damage-boost movement type");
    AssertTrue(!samus.KnockbackActive, "damage boost restores normal handler");
    AssertEqual((ushort)0, samus.KnockbackDirection, "damage boost clears knockback direction");
    AssertEqual((ushort)0, samus.KnockbackTimer, "damage boost clears hurt timer");
    AssertEqual((ushort)4, samus.Kinematics.YSpeed, "damage boost fresh jump whole speed");
    AssertEqual((ushort)0xe000, samus.Kinematics.YSubspeed, "damage boost fresh jump subspeed");

    AerialMovementResult boostFrame = SamusAerialMovement.StepDamageBoost(
        bus,
        empty,
        samus,
        (ushort)SnesButton.A,
        nmiFrameCounter: 1);
    AssertEqual(unchecked((int)0xfffb2000), boostFrame.Vertical!.Value.AcceptedDisplacement,
        "damage boost reuses ordinary old-speed jumping movement");
    AssertEqual((ushort)4, samus.Kinematics.YSpeed, "damage boost gravity next whole speed");
    AssertEqual((ushort)0xb800, samus.Kinematics.YSubspeed, "damage boost gravity next subspeed");

    // Jump alone (`$0080`) exits right-facing `$50` to neutral-jump `$4D`. This is an ordinary pose
    // change inside movement type two, so the already-live 16.16 trajectory must survive.
    ushort preservedYSpeed = samus.Kinematics.YSpeed;
    ushort preservedYSubspeed = samus.Kinematics.YSubspeed;
    SamusKnockbackMovement.ApplyDamageBoostPoseTransition(
        bus,
        samus,
        SamusState.NeutralJumpRightPose);
    AssertEqual(SamusState.NeutralJumpRightPose, samus.Pose, "damage-boost neutral exit pose");
    AssertEqual(preservedYSpeed, samus.Kinematics.YSpeed, "damage-boost exit preserves whole Y speed");
    AssertEqual(preservedYSubspeed, samus.Kinematics.YSubspeed, "damage-boost exit preserves Y subspeed");

    // Damage-boost pose definitions store shot direction `$FF`. `$91:E95D` handles that
    // sentinel before the aimed-landing table and selects ordinary `$A4/$A5` from X
    // direction. `$50` intentionally stores reversed X direction four, producing `$A5`;
    // this guards the exact real-ROM landing branch found by the scripted run.
    var boostLanding = new SamusState
    {
        Pose = SamusState.DamageBoostRightPose,
        XPosition = 96,
        YPosition = 96,
    };
    boostLanding.RefreshCollisionRadii(bus);
    boostLanding.ApplyAerialLanding(bus, wasSpinning: false);
    AssertEqual(SamusState.NormalLandingLeftPose, boostLanding.Pose,
        "damage-boost FF shot direction selects ordinary metadata-direction landing");
    AssertEqual((ushort)94, boostLanding.YPosition,
        "damage-boost landing radius expansion preserves feet");

    // A separate uninterrupted fixture proves the timer owns the special handler's exact
    // five movement frames. The sixth call chooses falling `$29` without another move.
    var expires = new SamusState
    {
        Pose = SamusState.FacingRightNormalPose,
        XPosition = 96,
        YPosition = 96,
    };
    SamusKnockbackMovement.Start(bus, expires, 0, knockbackXDirection: 1);
    for (int frame = 0; frame < 5; frame++)
    {
        AssertTrue(!SamusKnockbackMovement.Step(bus, empty, expires, (ushort)frame).Ended,
            $"hurt movement frame {frame + 1} remains active");
        expires.DecrementHurtTimers();
    }
    ushort humanoidYBeforeCompletion = expires.YPosition;
    KnockbackMovementResult expired = SamusKnockbackMovement.Step(bus, empty, expires, 5);
    AssertTrue(expired.Ended, "zero hurt timer ends special handler");
    AssertEqual(SamusState.FallingRightPose, expires.Pose, "expired right knockback selects falling right");
    AssertTrue(!expires.KnockbackActive, "expired knockback restores normal handler");
    AssertEqual((ushort)0, expires.KnockbackDirection, "expired knockback clears direction");

    // `$90:DF15` republishes a ball's current pose rather than substituting `$53/$54`.
    // `$91:EE27` then ignores the hit side and held-forward rule when selecting vertical
    // direction, while `$90:8EDF` still uses the enemy-produced X side for horizontal travel.
    foreach ((byte pose, ushort hitSide, ushort expectedDirection) in new[]
    {
        (SamusState.MorphBallGroundRightPose, (ushort)0, (ushort)2),
        (SamusState.MorphBallFallingLeftPose, (ushort)1, (ushort)1),
        (SamusState.SpringBallGroundRightPose, (ushort)0, (ushort)2),
        (SamusState.SpringBallJumpLeftPose, (ushort)1, (ushort)1),
        (SamusState.SpringBallFallingRightPose, (ushort)0, (ushort)2),
    })
    {
        var ball = new SamusState
        {
            Pose = pose,
            XPosition = 96,
            YPosition = 96,
            BombJumpDirection = 0x0802,
            MorphBallBounceState = 0x0602,
        };
        ball.RefreshCollisionRadii(bus);
        ball.InitializeAnimation(bus, initialFrame: 3);
        ball.HorizontalSpeed.ContactDamageIndex = 3;
        ushort preservedFrame = ball.AnimationFrame;
        ushort preservedTimer = ball.AnimationFrameTimer;

        SamusKnockbackMovement.Start(
            bus,
            ball,
            controllerInput: (ushort)(SnesButton.Left | SnesButton.Right),
            knockbackXDirection: hitSide);
        AssertEqual(pose, ball.Pose, $"morphed type ${ball.ReadMovementType(bus):X2} retains pose");
        AssertEqual(expectedDirection, ball.KnockbackDirection,
            $"morphed pose ${pose:X2} chooses direction from facing only");
        AssertEqual(preservedFrame, ball.AnimationFrame,
            $"morphed pose ${pose:X2} retains rolling animation frame");
        AssertEqual(preservedTimer, ball.AnimationFrameTimer,
            $"morphed pose ${pose:X2} retains rolling animation timer");
        AssertEqual((ushort)0, ball.BombJumpDirection,
            $"morphed pose ${pose:X2} start clears pending bomb jump");
        AssertEqual((ushort)0, ball.HorizontalSpeed.ContactDamageIndex,
            $"morphed pose ${pose:X2} start clears contact damage");
        AssertEqual((ushort)0x0602, ball.MorphBallBounceState,
            $"morphed pose ${pose:X2} start leaves bounce state until completion");
    }

    // Let a sixth ball fixture reach the shared `$91:F31D` completion handler. Unlike the
    // humanoid path it must keep both pose and animation, clear bounce/velocity, and publish
    // downward direction two so normal ball physics resumes on the following frame.
    var ballExpires = new SamusState
    {
        Pose = SamusState.MorphBallGroundRightPose,
        XPosition = 96,
        YPosition = 96,
        MorphBallBounceState = 2,
    };
    ballExpires.RefreshCollisionRadii(bus);
    ballExpires.InitializeAnimation(bus, initialFrame: 3);
    SamusKnockbackMovement.Start(
        bus,
        ballExpires,
        controllerInput: (ushort)SnesButton.Right,
        knockbackXDirection: 0);
    ushort retainedBallFrame = ballExpires.AnimationFrame;
    ushort retainedBallTimer = ballExpires.AnimationFrameTimer;
    for (int frame = 0; frame < 5; frame++)
    {
        SamusKnockbackMovement.Step(bus, empty, ballExpires, unchecked((ushort)frame));
        ballExpires.DecrementHurtTimers();
    }
    KnockbackMovementResult ballExpired = SamusKnockbackMovement.Step(
        bus,
        empty,
        ballExpires,
        nmiFrameCounter: 5);
    AssertTrue(ballExpired.Ended, "zero hurt timer ends morphed special handler");
    AssertEqual(SamusState.MorphBallGroundRightPose, ballExpires.Pose,
        "expired morphed knockback retains current ball pose");
    AssertEqual(retainedBallFrame, ballExpires.AnimationFrame,
        "expired morphed knockback retains rolling frame");
    AssertEqual(retainedBallTimer, ballExpires.AnimationFrameTimer,
        "expired morphed knockback retains rolling timer");
    AssertEqual((ushort)0, ballExpires.MorphBallBounceState,
        "expired morphed knockback clears bounce state");
    AssertEqual((ushort)0, ballExpires.Kinematics.YSpeed,
        "expired morphed knockback clears whole Y speed");
    AssertEqual((ushort)0, ballExpires.Kinematics.YSubspeed,
        "expired morphed knockback clears fractional Y speed");
    AssertEqual((ushort)2, ballExpires.Kinematics.YDirection,
        "expired morphed knockback resumes with direction two");

    // The same command-one cleanup also corrects the existing humanoid route: `$53` radius
    // 21 becomes `$29` radius 19, so center Y moves down two pixels to preserve the feet.
    AssertEqual((ushort)0, expires.Kinematics.YSpeed,
        "expired humanoid knockback clears whole Y speed");
    AssertEqual((ushort)0, expires.Kinematics.YSubspeed,
        "expired humanoid knockback clears fractional Y speed");
    AssertEqual((ushort)2, expires.Kinematics.YDirection,
        "expired humanoid knockback publishes falling direction two");
    AssertEqual(unchecked((ushort)(humanoidYBeforeCompletion + 2)), expires.YPosition,
        "expired humanoid knockback aligns radius-19 falling body to radius-21 feet");

    Console.WriteLine("  Samus knockback: humanoid/ball starts, timer, 16.16 hurt arc, same-pose animation, cleanup, and damage-boost handoff agree.");
}

/// <summary>
/// Exercises the bank-$9B/$94 connected-pendulum order with deliberately tiny ROM tables.
/// Hard-coded positions and velocities make this independent of production helper formulas.
/// </summary>
static void VerifySamusGrappleSwingAndRelease()
{
    var bus = new TestAddressSpace();

    // Pose definitions are literal eight-byte records. Only X direction, movement type,
    // graphics offset, and radii matter to this isolated route. $B2 is right-facing/type
    // $16; $B3 is its left-facing mirror. Release poses $51/$52 return to type two.
    bus.WriteBytes(0x91b629 + SamusState.GrappleSwingRightPose * 8,
        [0x08, 0x16, 0xff, 0x02, 0x00, 0x00, 0x05, 0x15]);
    bus.WriteBytes(0x91b629 + SamusState.GrappleSwingLeftPose * 8,
        [0x04, 0x16, 0xff, 0x07, 0x00, 0x00, 0x05, 0x15]);
    bus.WriteBytes(0x91b629 + SamusState.NormalJumpForwardRightPose * 8,
        [0x08, 0x02, 0xff, 0x02, 0x00, 0x00, 0x05, 0x15]);
    bus.WriteBytes(0x91b629 + SamusState.NormalJumpForwardLeftPose * 8,
        [0x04, 0x02, 0xff, 0x07, 0x00, 0x00, 0x05, 0x15]);

    // The close-collision routes use a second family of type-$16 records. These bytes are
    // the retail pose definitions, not convenient test metadata: `$B6/$B7` are locked
    // crouching-down poses, `$B8/$B9` are the two wall contacts, and `$83/$84` are the
    // ordinary wall-jump launch poses selected one function call later. The dropped route
    // below uses compact diagonal-down `$74`; locked cancellation uses stable crouch `$27`.
    bus.WriteBytes(0x91b629 + SamusState.GrappleCrouchingDownRightPose * 8,
        [0x08, 0x16, 0x27, 0x03, 0x00, 0x00, 0x10, 0x00]);
    bus.WriteBytes(0x91b629 + SamusState.GrappleCrouchingDownLeftPose * 8,
        [0x04, 0x16, 0x28, 0x06, 0x00, 0x00, 0x10, 0x00]);
    bus.WriteBytes(0x91b629 + SamusState.GrappleWallContactLeftPose * 8,
        [0x08, 0x16, 0xff, 0x03, 0x00, 0x00, 0x10, 0x00]);
    bus.WriteBytes(0x91b629 + SamusState.GrappleWallContactRightPose * 8,
        [0x04, 0x16, 0xff, 0x06, 0x00, 0x00, 0x10, 0x00]);
    bus.WriteBytes(0x91b629 + SamusState.WallJumpRightPose * 8,
        [0x08, 0x14, 0x19, 0xff, 0x08, 0x00, 0x13, 0x00]);
    bus.WriteBytes(0x91b629 + SamusState.WallJumpLeftPose * 8,
        [0x04, 0x14, 0x1a, 0xff, 0x08, 0x00, 0x13, 0x00]);
    bus.WriteBytes(0x91b629 + SamusState.CrouchingRightPose * 8,
        [0x08, 0x05, 0x27, 0x02, 0x00, 0x00, 0x10, 0x00]);
    bus.WriteBytes(0x91b629 + SamusState.CrouchingAimDiagonalDownLeftPose * 8,
        [0x04, 0x05, 0x28, 0x06, 0x00, 0x00, 0x10, 0x00]);

    // `$9B:B9D9-$BA29` can install every one of these six additional type-$16 poses when
    // a stationary firing beam connects. These are literal retail pose-definition bytes:
    // `$A8-$AB` are full-height standing locks, while `$B4/$B5` are the two crouching
    // diagonal-up locks. `$B6/$B7` above complete the crouching family.
    bus.WriteBytes(0x91b629 + 0xa8 * 8,
        [0x08, 0x16, 0x01, 0x02, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b629 + 0xa9 * 8,
        [0x04, 0x16, 0x02, 0x07, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b629 + 0xaa * 8,
        [0x08, 0x16, 0x07, 0x03, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b629 + 0xab * 8,
        [0x04, 0x16, 0x08, 0x06, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b629 + 0xb4 * 8,
        [0x08, 0x16, 0x27, 0x02, 0x00, 0x00, 0x10, 0x00]);
    bus.WriteBytes(0x91b629 + 0xb5 * 8,
        [0x04, 0x16, 0x28, 0x07, 0x00, 0x00, 0x10, 0x00]);

    // All four poses point at a harmless ordinary delay list so the public connection and
    // release initializers can execute their real animation initialization seam.
    foreach (byte pose in new byte[]
    {
        SamusState.GrappleSwingRightPose,
        SamusState.GrappleSwingLeftPose,
        SamusState.NormalJumpForwardRightPose,
        SamusState.NormalJumpForwardLeftPose,
        SamusState.GrappleCrouchingDownRightPose,
        SamusState.GrappleCrouchingDownLeftPose,
        SamusState.GrappleWallContactLeftPose,
        SamusState.GrappleWallContactRightPose,
        SamusState.WallJumpRightPose,
        SamusState.WallJumpLeftPose,
        SamusState.CrouchingRightPose,
        SamusState.CrouchingAimDiagonalDownLeftPose,
        0xa8,
        0xa9,
        0xaa,
        0xab,
        0xb4,
        0xb5,
    })
    {
        WriteTestWord(bus, 0x91b010 + pose * 2, 0xbf00);
    }
    bus.WriteBytes(0x91bf00, [0x05, 0xff]);

    // Dry-air wall-jump table zero is 4.A000. A grapple wall jump reaches the same
    // `$90:9949` initializer as an ordinary spin wall jump after its reversed pose choice.
    WriteTestWord(bus, 0x909ed1, 0x0004);
    WriteTestWord(bus, 0x909ed7, 0xa000);

    // Firing source pose $29 is a retail right-facing fall with shot direction two. The
    // four bank-$9B table groups below are seeded with their literal direction-two values:
    // +11.F4 X velocity, zero Y velocity, rightward angle $C000, and (+2,+2) origin.
    bus.WriteBytes(0x91b629 + SamusState.FallingRightPose * 8,
        [0x08, 0x06, 0xff, 0x02, 0x00, 0x00, 0x05, 0x15]);
    WriteTestWord(bus, 0x9bc0db + 2 * 2, 0x0bf4);
    WriteTestWord(bus, 0x9bc0ef + 2 * 2, 0x0000);
    WriteTestWord(bus, 0x9bc104 + 2 * 2, 0xc000);
    WriteTestWord(bus, 0x9bc122 + 2 * 2, 0x0002);
    WriteTestWord(bus, 0x9bc136 + 2 * 2, 0x0002);
    WriteTestWord(bus, 0x9bc14a + 2 * 2, 0x0002);
    WriteTestWord(bus, 0x9bc15e + 2 * 2, 0x0002);

    // The connection selector reads two literal words per direction. Seed all thirty ROM
    // records, not just the one used by the first fixture, so exhaustive routing below can
    // detect direction-order mistakes and the crouching table's intentional `$AB` entries.
    (ushort Function, ushort Handler)[] defaultConnections =
    [
        (0xc79d, 0xb9d9), (0xc79d, 0xb9d9), (0xc77e, 0xb9ea), (0xc77e, 0xb9f3),
        (0xc77e, 0xb9fc), (0xc77e, 0xb9fc), (0xc77e, 0xb9fc), (0xc77e, 0xba05),
        (0xc79d, 0xb9e2), (0xc79d, 0xb9e2),
    ];
    (ushort Function, ushort Handler)[] verticalConnections =
    [
        (0xc79d, 0xb9d9), (0xc79d, 0xb9d9), (0xc79d, 0xb9d9), (0xc79d, 0xb9d9),
        (0xc79d, 0xb9d9), (0xc79d, 0xb9e2), (0xc79d, 0xb9e2), (0xc79d, 0xb9e2),
        (0xc79d, 0xb9e2), (0xc79d, 0xb9e2),
    ];
    (ushort Function, ushort Handler)[] crouchingConnections =
    [
        (0xc79d, 0xb9d9), (0xc79d, 0xb9d9), (0xc77e, 0xba0e), (0xc77e, 0xba17),
        (0xc77e, 0xb9fc), (0xc77e, 0xb9fc), (0xc77e, 0xba20), (0xc77e, 0xba29),
        (0xc79d, 0xb9e2), (0xc79d, 0xb9e2),
    ];
    foreach ((int table, (ushort Function, ushort Handler)[] records) in new[]
    {
        (0x9bc3c6, defaultConnections),
        (0x9bc3ee, verticalConnections),
        (0x9bc416, crouchingConnections),
    })
    {
        for (int direction = 0; direction < records.Length; direction++)
        {
            WriteTestWord(bus, table + direction * 4, records[direction].Function);
            WriteTestWord(bus, table + direction * 4 + 2, records[direction].Handler);
        }
    }

    // A type-$E/BTS-$00 block is persistent grapple PLM $D0D8 and returns flags $41.
    // Put it at (3,3): the first frame's four 16.16 substeps end at X=45, then frame two's
    // first substep reaches X=48 and must center the accepted endpoint at (56,56).
    var firingBlocks = new ushort[8 * 8];
    firingBlocks[3 * 8 + 3] = 0xe000;
    var firingLevel = new RoomLevelData(
        8,
        8,
        firingBlocks,
        new byte[firingBlocks.Length],
        new ushort[firingBlocks.Length],
        new byte[8]);

    // Samus minus anchor is (-24,-8), which $A0:C0B1 approximates as angle byte $CA.
    // Give that byte deterministic sine/art data so connection publishes observable state.
    WriteTestWord(bus, 0xa0b3c3 + 0xca * 2, 0x0000);
    WriteTestWord(bus, 0xa0b3c3 + (0xca + 64) * 2, 0xff00);
    bus.WriteByte(0x9bc1c2 + 0xca, 5);
    bus.WriteBytes(0x9bc302 + 5 * 2, [0x00, 0x00]);

    var firingSamus = new SamusState
    {
        Pose = SamusState.FallingRightPose,
        XPosition = 32,
        YPosition = 48,
    };
    firingSamus.Kinematics.YSpeed = 1; // selects moving-vertically connection table $C3EE
    SamusGrappleMovement.BeginFiring(bus, firingSamus);
    AssertEqual(GrapplePhase.Firing, firingSamus.Grapple.Phase, "grapple firing phase");
    AssertEqual((short)0x0bf4, firingSamus.Grapple.ExtensionXVelocity,
        "grapple X extension velocity comes from ROM");
    AssertEqual((ushort)34, firingSamus.Grapple.AnchorX, "grapple initial endpoint X");
    AssertEqual((ushort)50, firingSamus.Grapple.AnchorY, "grapple initial endpoint Y");

    GrappleMovementResult extending = SamusGrappleMovement.StepFiring(
        bus, firingLevel, firingSamus, (ushort)SnesButton.X);
    AssertTrue(extending.Fired && !extending.Connected && !extending.OwnsMovement,
        "unobstructed firing remains live without stealing ordinary body movement");
    AssertEqual((ushort)12, firingSamus.Grapple.RopeLength, "firing length grows by twelve");
    AssertEqual((ushort)45, firingSamus.Grapple.AnchorX,
        "four fractional collision substeps publish the exact first-frame endpoint");

    GrappleMovementResult connected = SamusGrappleMovement.StepFiring(
        bus, firingLevel, firingSamus, (ushort)SnesButton.X);
    AssertTrue(connected.Connected && connected.OwnsMovement,
        "persistent grapple block establishes connected movement");
    AssertEqual(GrapplePhase.ConnectedSwinging, firingSamus.Grapple.Phase,
        "block acquisition installs swinging function");
    AssertEqual((ushort)55, firingSamus.Grapple.AnchorX,
        "accepted grapple block centers X then applies negative-rope side bias");
    AssertEqual((ushort)56, firingSamus.Grapple.AnchorY, "accepted grapple block centers Y");
    AssertEqual((ushort)0xca00, firingSamus.Grapple.Angle,
        "connection angle uses bank-$A0 integer octant calculation");
    AssertEqual(SamusState.GrappleSwingRightPose, firingSamus.Pose,
        "right-half airborne shot selects clockwise grapple pose $B2");
    AssertEqual((ushort)31, firingSamus.Grapple.RopeStartX,
        "accepted connection publishes native rope Start X");
    AssertEqual((ushort)56, firingSamus.Grapple.RopeStartY,
        "accepted connection publishes native rope Start Y");
    AssertEqual(firingSamus.Grapple.RopeStartX, firingSamus.Grapple.BeamStartX,
        "swing command copies rope Start X into flare/draw X");
    AssertEqual(firingSamus.Grapple.RopeStartY, firingSamus.Grapple.BeamStartY,
        "swing command copies rope Start Y into flare/draw Y");
    AssertEqual((ushort)0, firingSamus.Kinematics.YSpeed,
        "connection common tail clears whole Y speed");
    AssertEqual((ushort)32, connected.CameraPreviousX!.Value,
        "connection common tail retains in-range camera previous X");
    AssertEqual((ushort)48, connected.CameraPreviousY!.Value,
        "connection common tail retains in-range camera previous Y");

    // BTS one must use the same accepted-connection path, but setup CFB5 also creates an
    // independent bank-$84 object and clears BTS before returning flags $41. This direct
    // firing test guards the integration seam; the complete ROM instruction timeline is
    // verified separately below.
    var breakableFiringBlocks = new ushort[8 * 8];
    var breakableFiringBts = new byte[breakableFiringBlocks.Length];
    breakableFiringBlocks[3 * 8 + 3] = 0xe000;
    breakableFiringBts[3 * 8 + 3] = 1;
    var breakableFiringLevel = new RoomLevelData(
        8,
        8,
        breakableFiringBlocks,
        breakableFiringBts,
        new ushort[breakableFiringBlocks.Length],
        new byte[8]);
    var breakableFiringSamus = new SamusState
    {
        Pose = SamusState.FallingRightPose,
        XPosition = 32,
        YPosition = 48,
    };
    breakableFiringSamus.Kinematics.YSpeed = 1;
    var breakableFiringPlms = new RoomPlmSystem();
    SamusGrappleMovement.BeginFiring(bus, breakableFiringSamus);
    SamusGrappleMovement.StepFiring(
        bus,
        breakableFiringLevel,
        breakableFiringSamus,
        (ushort)SnesButton.X,
        breakableFiringPlms);
    GrappleMovementResult breakableConnection = SamusGrappleMovement.StepFiring(
        bus,
        breakableFiringLevel,
        breakableFiringSamus,
        (ushort)SnesButton.X,
        breakableFiringPlms);
    AssertTrue(breakableConnection.Connected,
        "BTS-one grapple firing connects through PLM setup");
    AssertEqual(1, breakableFiringPlms.ActiveCount,
        "BTS-one acquisition installs one independent room PLM");
    AssertEqual((byte)0, breakableFiringLevel.GetCollisionBlock(3, 3).Behavior,
        "BTS-one acquisition synchronously clears low BTS byte");

    // Bank $94 does not treat extension blocks as collision results of their own. Instead,
    // type $5 adds signed BTS directly to the linear block index and dispatches the block
    // found there. Keep the beam physically inside (3,3), but make that cell point right to
    // a persistent type-$E target at (4,3). Connecting proves the indirection is followed;
    // ending in the physical (3,3) cell, rather than (4,3), proves the visible endpoint stays in the
    // extension cell exactly as GrappleCollision_XBlock/YBlock do in the original routine.
    var horizontalExtensionBlocks = new ushort[8 * 8];
    var horizontalExtensionBts = new byte[horizontalExtensionBlocks.Length];
    horizontalExtensionBlocks[3 * 8 + 3] = 0x5000;
    horizontalExtensionBts[3 * 8 + 3] = 1;
    horizontalExtensionBlocks[3 * 8 + 4] = 0xe000;
    var horizontalExtensionLevel = new RoomLevelData(
        8,
        8,
        horizontalExtensionBlocks,
        horizontalExtensionBts,
        new ushort[horizontalExtensionBlocks.Length],
        new byte[8]);
    var horizontalExtensionSamus = new SamusState
    {
        Pose = SamusState.FallingRightPose,
        XPosition = 32,
        YPosition = 48,
    };
    horizontalExtensionSamus.Kinematics.YSpeed = 1;
    SamusGrappleMovement.BeginFiring(bus, horizontalExtensionSamus);
    SamusGrappleMovement.StepFiring(
        bus, horizontalExtensionLevel, horizontalExtensionSamus, (ushort)SnesButton.X);
    GrappleMovementResult horizontalExtensionConnection = SamusGrappleMovement.StepFiring(
        bus, horizontalExtensionLevel, horizontalExtensionSamus, (ushort)SnesButton.X);
    AssertTrue(horizontalExtensionConnection.Connected,
        "horizontal extension BTS dispatches its referenced grapple block");
    AssertEqual((ushort)55, horizontalExtensionSamus.Grapple.AnchorX,
        "horizontal extension keeps physical endpoint block X");

    // Type $D uses the same signed byte but multiplies it by RoomWidthBlocks. A +1 BTS at
    // (3,3) therefore dispatches (3,4), while the accepted endpoint still centers in (3,3).
    // This test is intentionally separate from the horizontal case: confusing the two
    // formulas is easy and can appear correct in rooms whose nearby cells happen to be air.
    var verticalExtensionBlocks = new ushort[8 * 8];
    var verticalExtensionBts = new byte[verticalExtensionBlocks.Length];
    verticalExtensionBlocks[3 * 8 + 3] = 0xd000;
    verticalExtensionBts[3 * 8 + 3] = 1;
    verticalExtensionBlocks[4 * 8 + 3] = 0xe000;
    var verticalExtensionLevel = new RoomLevelData(
        8,
        8,
        verticalExtensionBlocks,
        verticalExtensionBts,
        new ushort[verticalExtensionBlocks.Length],
        new byte[8]);
    var verticalExtensionSamus = new SamusState
    {
        Pose = SamusState.FallingRightPose,
        XPosition = 32,
        YPosition = 48,
    };
    verticalExtensionSamus.Kinematics.YSpeed = 1;
    SamusGrappleMovement.BeginFiring(bus, verticalExtensionSamus);
    SamusGrappleMovement.StepFiring(
        bus, verticalExtensionLevel, verticalExtensionSamus, (ushort)SnesButton.X);
    GrappleMovementResult verticalExtensionConnection = SamusGrappleMovement.StepFiring(
        bus, verticalExtensionLevel, verticalExtensionSamus, (ushort)SnesButton.X);
    AssertTrue(verticalExtensionConnection.Connected,
        "vertical extension BTS dispatches its referenced grapple block");
    AssertEqual((ushort)56, verticalExtensionSamus.Grapple.AnchorY,
        "vertical extension keeps physical endpoint block Y");

    // Ordinary solid-family blocks return carry with overflow clear. That is not a rope
    // connection: firing moves to the one-call cancellation function, matching $94:A8E5.
    var solidBlocks = new ushort[8 * 8];
    solidBlocks[3 * 8 + 3] = 0x8000;
    var solidLevel = new RoomLevelData(
        8,
        8,
        solidBlocks,
        new byte[solidBlocks.Length],
        new ushort[solidBlocks.Length],
        new byte[8]);
    var solidCollisionSamus = new SamusState
    {
        Pose = SamusState.FallingRightPose,
        XPosition = 32,
        YPosition = 48,
    };
    SamusGrappleMovement.BeginFiring(bus, solidCollisionSamus);
    SamusGrappleMovement.StepFiring(
        bus, solidLevel, solidCollisionSamus, (ushort)SnesButton.X);
    GrappleMovementResult solidCancellation = SamusGrappleMovement.StepFiring(
        bus, solidLevel, solidCollisionSamus, (ushort)SnesButton.X);
    AssertTrue(solidCancellation.CancelQueued && !solidCancellation.Connected,
        "solid block queues grapple firing cancellation");

    // Type `$A` does not use the generic solid result during grapple firing. `$94:A7FD`
    // spawns one of sixteen bank-$84 entries: ordinary nonnegative BTS values run the
    // carry-set/overflow-clear setup and cancel, while BTS three is Draygon's broken turret
    // and queues one whole periodic-damage unit before returning carry+overflow to connect.
    foreach ((byte behavior, bool expectedConnection, bool expectedCancellation, ushort expectedDamage) in new[]
    {
        ((byte)0x00, false, true, (ushort)0),
        ((byte)0x03, true, false, (ushort)1),
        ((byte)0x83, false, false, (ushort)0),
    })
    {
        var spikeBlocks = new ushort[8 * 8];
        var spikeBts = new byte[spikeBlocks.Length];
        spikeBlocks[3 * 8 + 3] = 0xa000;
        spikeBts[3 * 8 + 3] = behavior;
        var spikeLevel = new RoomLevelData(
            8,
            8,
            spikeBlocks,
            spikeBts,
            new ushort[spikeBlocks.Length],
            new byte[8]);
        var spikeSamus = new SamusState
        {
            Pose = SamusState.FallingRightPose,
            XPosition = 32,
            YPosition = 48,
        };
        spikeSamus.Kinematics.YSpeed = 1;
        SamusGrappleMovement.BeginFiring(bus, spikeSamus);
        SamusGrappleMovement.StepFiring(
            bus, spikeLevel, spikeSamus, (ushort)SnesButton.X);
        GrappleMovementResult spikeReaction = SamusGrappleMovement.StepFiring(
            bus, spikeLevel, spikeSamus, (ushort)SnesButton.X);

        AssertEqual(expectedConnection, spikeReaction.Connected,
            $"firing spike BTS ${behavior:X2} connection flag");
        AssertEqual(expectedCancellation, spikeReaction.CancelQueued,
            $"firing spike BTS ${behavior:X2} cancellation flag");
        AssertEqual(expectedDamage, spikeSamus.LiquidPhysics.PeriodicDamage,
            $"firing spike BTS ${behavior:X2} periodic damage");
    }

    // Length grows before collision checks. Values 12..120 receive their four probes, but
    // the next addition produces 132 and queues cancellation without moving the endpoint.
    var emptyWideBlocks = new ushort[16 * 8];
    var emptyWideLevel = new RoomLevelData(
        16,
        8,
        emptyWideBlocks,
        new byte[emptyWideBlocks.Length],
        new ushort[emptyWideBlocks.Length],
        new byte[16]);
    var rangeLimitedSamus = new SamusState
    {
        Pose = SamusState.FallingRightPose,
        XPosition = 32,
        YPosition = 48,
    };
    SamusGrappleMovement.BeginFiring(bus, rangeLimitedSamus);
    for (int firingFrame = 0; firingFrame < 10; firingFrame++)
    {
        GrappleMovementResult liveRange = SamusGrappleMovement.StepFiring(
            bus, emptyWideLevel, rangeLimitedSamus, (ushort)SnesButton.X);
        AssertTrue(liveRange.Fired, $"grapple range frame {firingFrame} remains live");
    }
    AssertEqual((ushort)120, rangeLimitedSamus.Grapple.RopeLength,
        "last collision-tested grapple firing length");
    ushort endpointBeforeRangeCancellation = rangeLimitedSamus.Grapple.AnchorX;
    GrappleMovementResult rangeCancellation = SamusGrappleMovement.StepFiring(
        bus, emptyWideLevel, rangeLimitedSamus, (ushort)SnesButton.X);
    AssertTrue(rangeCancellation.CancelQueued,
        "grapple queues cancellation when pre-collision length reaches 128");
    AssertEqual(endpointBeforeRangeCancellation, rangeLimitedSamus.Grapple.AnchorX,
        "range cancellation performs no endpoint substep");

    // Release-of-Shoot is checked before extension. Cancellation remains queued for one
    // function call, mirroring the bank-$9B pointer change rather than disappearing early.
    var cancelledSamus = new SamusState
    {
        Pose = SamusState.FallingRightPose,
        XPosition = 32,
        YPosition = 48,
    };
    SamusGrappleMovement.BeginFiring(bus, cancelledSamus);
    GrappleMovementResult cancelQueued = SamusGrappleMovement.StepFiring(
        bus, firingLevel, cancelledSamus, controllerInput: 0);
    AssertTrue(cancelQueued.CancelQueued && !cancelQueued.Cancelled,
        "released firing queues cancellation");
    GrappleMovementResult cancelled =
        SamusGrappleMovement.CompleteFiringCancellation(bus, cancelledSamus);
    AssertTrue(cancelled.Cancelled && cancelledSamus.Grapple.Phase == GrapplePhase.Inactive,
        "queued firing cancellation clears on following call");

    // Exercise all three native connection tables through the public BeginFiring/StepFiring
    // route. Filling this isolated room with persistent type-$E blocks makes every zero-
    // velocity endpoint connect on its first substep, while each authored direction still
    // selects its own four-byte {function, handler} record and pose.
    var connectionBlocks = Enumerable.Repeat((ushort)0xe000, 8 * 8).ToArray();
    var connectionLevel = new RoomLevelData(
        8,
        8,
        connectionBlocks,
        new byte[connectionBlocks.Length],
        new ushort[connectionBlocks.Length],
        new byte[8]);
    byte[][] expectedConnectionPoses =
    [
        // Default stationary table `$C3C6`.
        [0xb2, 0xb2, 0xa8, 0xaa, 0xab, 0xab, 0xab, 0xa9, 0xb3, 0xb3],
        // Crouching stationary table `$C416`; directions four/five literally use `$AB`.
        [0xb2, 0xb2, 0xb4, 0xb6, 0xab, 0xab, 0xb7, 0xb5, 0xb3, 0xb3],
        // Any nonzero vertical speed half overrides posture and selects `$C3EE`.
        [0xb2, 0xb2, 0xb2, 0xb2, 0xb2, 0xb3, 0xb3, 0xb3, 0xb3, 0xb3],
    ];

    // Zero extension velocity means the endpoint is the pose-authored origin. Give the raw
    // no-run Origin and Flare tables distinct values so locked command 10 cannot pass by
    // accidentally treating the flare/draw coordinate as the physical rope Start pair.
    for (int direction = 0; direction < 10; direction++)
    {
        int tableOffset = direction * 2;
        WriteTestWord(bus, 0x9bc0db + tableOffset, 0);
        WriteTestWord(bus, 0x9bc0ef + tableOffset, 0);
        WriteTestWord(bus, 0x9bc104 + tableOffset, unchecked((ushort)(direction << 8)));
        WriteTestWord(bus, 0x9bc122 + tableOffset, unchecked((ushort)(direction + 1)));
        WriteTestWord(bus, 0x9bc136 + tableOffset, unchecked((ushort)(direction + 2)));
        WriteTestWord(bus, 0x9bc14a + tableOffset, unchecked((ushort)(direction + 20)));
        WriteTestWord(bus, 0x9bc15e + tableOffset, unchecked((ushort)(direction + 30)));
    }

    for (int family = 0; family < expectedConnectionPoses.Length; family++)
    {
        for (byte direction = 0; direction < 10; direction++)
        {
            byte sourceMovementType = family == 1 ? (byte)5 : (byte)6;
            bus.WriteBytes(0x91b629 + SamusState.FallingRightPose * 8,
                [0x08, sourceMovementType, 0xff, direction, 0x00, 0x00, 0x05, 0x15]);

            var connectionSamus = new SamusState
            {
                Pose = SamusState.FallingRightPose,
                XPosition = 40,
                YPosition = 40,
            };
            if (family == 2)
            {
                // A nonzero fractional half alone must select the vertical table and must
                // be cleared by the shared special-pose-command tail after connection.
                connectionSamus.Kinematics.YSubspeed = 1;
            }
            connectionSamus.HorizontalSpeed.AccelerationMode = 7;
            connectionSamus.HorizontalSpeed.BaseSpeed = 1;
            connectionSamus.HorizontalSpeed.BaseSubspeed = 2;
            connectionSamus.HorizontalSpeed.ExtraRunSpeed = 3;
            connectionSamus.HorizontalSpeed.ExtraRunSubspeed = 4;

            SamusGrappleMovement.BeginFiring(bus, connectionSamus);
            GrappleMovementResult tableConnection = SamusGrappleMovement.StepFiring(
                bus,
                connectionLevel,
                connectionSamus,
                (ushort)SnesButton.X);

            byte expectedPose = expectedConnectionPoses[family][direction];
            bool expectedLocked = expectedPose is not (0xb2 or 0xb3);
            AssertTrue(tableConnection.Connected,
                $"connection family {family} direction {direction} connects through runtime path");
            AssertEqual(expectedPose, connectionSamus.Pose,
                $"connection family {family} direction {direction} pose");
            AssertEqual(
                expectedLocked ? GrapplePhase.ConnectedLocked : GrapplePhase.ConnectedSwinging,
                connectionSamus.Grapple.Phase,
                $"connection family {family} direction {direction} function phase");
            AssertEqual(expectedLocked, tableConnection.LockedInPlace,
                $"connection family {family} direction {direction} locked result");
            AssertTrue(tableConnection.CameraPreviousX.HasValue && tableConnection.CameraPreviousY.HasValue,
                $"connection family {family} direction {direction} publishes camera clamp");

            AssertEqual((ushort)0, connectionSamus.HorizontalSpeed.BaseSpeed,
                "connection clears whole X base speed");
            AssertEqual((ushort)0, connectionSamus.HorizontalSpeed.BaseSubspeed,
                "connection clears fractional X base speed");
            AssertEqual((ushort)0, connectionSamus.HorizontalSpeed.ExtraRunSpeed,
                "connection clears whole extra run speed");
            AssertEqual((ushort)0, connectionSamus.HorizontalSpeed.ExtraRunSubspeed,
                "connection clears fractional extra run speed");
            AssertEqual((ushort)7, connectionSamus.HorizontalSpeed.AccelerationMode,
                "connection leaves acceleration mode untouched");
            AssertEqual((ushort)0, connectionSamus.Kinematics.YSpeed,
                "connection clears whole Y speed");
            AssertEqual((ushort)0, connectionSamus.Kinematics.YSubspeed,
                "connection clears fractional Y speed");

            if (expectedLocked)
            {
                short rawOriginX = unchecked((short)(direction + 1));
                short rawOriginY = unchecked((short)(direction + 2));
                short rawFlareX = unchecked((short)(direction + 20));
                short rawFlareY = unchecked((short)(direction + 30));
                AssertEqual(
                    unchecked((ushort)(connectionSamus.Grapple.RopeStartX - rawOriginX)),
                    connectionSamus.XPosition,
                    "locked command positions Samus X from physical rope Start");
                AssertEqual(
                    unchecked((ushort)(connectionSamus.Grapple.RopeStartY - rawOriginY)),
                    connectionSamus.YPosition,
                    "locked command positions Samus Y from physical rope Start");
                AssertEqual(
                    unchecked((ushort)(connectionSamus.XPosition + rawFlareX)),
                    connectionSamus.Grapple.BeamStartX,
                    "locked command independently publishes flare/draw X");
                AssertEqual(
                    unchecked((ushort)(connectionSamus.YPosition + rawFlareY)),
                    connectionSamus.Grapple.BeamStartY,
                    "locked command independently publishes flare/draw Y");
            }
            else
            {
                AssertEqual(connectionSamus.Grapple.RopeStartX, connectionSamus.Grapple.BeamStartX,
                    "swing command aliases physical Start and flare X");
                AssertEqual(connectionSamus.Grapple.RopeStartY, connectionSamus.Grapple.BeamStartY,
                    "swing command aliases physical Start and flare Y");
            }
        }
    }

    // Preserve the original fixture record for the remaining grapple tests in this method.
    bus.WriteBytes(0x91b629 + SamusState.FallingRightPose * 8,
        [0x08, 0x06, 0xff, 0x02, 0x00, 0x00, 0x05, 0x15]);

    // The production code follows $94's long loads into the signed sine table at $A0:B3C3.
    // Seed only the entries
    // touched by this fixture. At $8000 the rope points 50 pixels left; after one positive
    // $010C step the next high-byte sample gives X=-49 and Y=+1 by magnitude truncation.
    WriteTestWord(bus, 0xa0b3c3 + 128 * 2, 0x0000);
    WriteTestWord(bus, 0xa0b3c3 + 192 * 2, 0xff00);
    WriteTestWord(bus, 0xa0b3c3 + 129 * 2, 0x0006);
    WriteTestWord(bus, 0xa0b3c3 + 193 * 2, 0xff01);
    WriteTestWord(bus, 0xa0b3c3 + 132 * 2, 0x0019);

    // Angle bytes $80/$81 select art frames three/four. Right-pose origin corrections are
    // (+2,+5) and (+4,-3), making the expected body centers easy to audit by inspection.
    bus.WriteByte(0x9bc1c2 + 0x80, 3);
    bus.WriteByte(0x9bc1c2 + 0x81, 4);
    bus.WriteBytes(0x9bc302 + 3 * 2, [0x02, 0x05]);
    bus.WriteBytes(0x9bc302 + 4 * 2, [0x04, 0xfd]);

    // The already-connected pendulum fixture uses an intentionally empty 32x16 room. Its
    // six-point sweep must stay in bounds while proving that no invented terrain response
    // disturbs the pre-existing unobstructed numbers.
    var swingBlocks = new ushort[32 * 16];
    var swingLevel = new RoomLevelData(
        32,
        16,
        swingBlocks,
        new byte[swingBlocks.Length],
        new ushort[swingBlocks.Length],
        new byte[32]);

    var samus = new SamusState { XPosition = 10, YPosition = 20 };
    SamusGrappleMovement.ConnectUnobstructedSwing(
        bus,
        samus,
        anchorX: 200,
        anchorY: 100,
        ropeLength: 50,
        angle: 0x8000,
        angularVelocity: 0,
        faceRight: true);

    AssertEqual(SamusState.GrappleSwingRightPose, samus.Pose, "grapple connection pose");
    AssertEqual((byte)0x16, samus.ReadMovementType(bus), "grapple movement type from pose record");
    AssertEqual((ushort)149, samus.Grapple.BeamStartX, "initial grapple beam-start X");
    AssertEqual((ushort)104, samus.Grapple.BeamStartY, "initial grapple beam-start Y");
    AssertEqual((ushort)151, samus.XPosition, "initial grapple art-corrected X");
    AssertEqual((ushort)109, samus.YPosition, "initial grapple art-corrected Y");
    AssertEqual((ushort)3, samus.AnimationFrame, "initial grapple angle art frame");

    // Left held at exact $8000 first applies the native +$0100 kick, then +12 input.
    // Gravity is exactly zero on that axis, so angle advances by $010C to $810C.
    GrappleMovementResult swung = SamusGrappleMovement.Step(
        bus,
        swingLevel,
        samus,
        (ushort)(SnesButton.X | SnesButton.Left),
        newlyPressedInput: 0);
    AssertEqual(GrapplePhase.ConnectedSwinging, swung.Phase, "held-shot grapple phase");
    AssertEqual((short)0x010c, samus.Grapple.AngularVelocity, "bottom kick plus input acceleration");
    AssertEqual((ushort)0x810c, samus.Grapple.Angle, "unobstructed angle integration");
    AssertEqual((ushort)150, samus.Grapple.BeamStartX, "advanced grapple beam-start X");
    AssertEqual((ushort)105, samus.Grapple.BeamStartY, "advanced grapple beam-start Y");
    AssertEqual((ushort)154, samus.XPosition, "advanced grapple art-corrected X");
    AssertEqual((ushort)102, samus.YPosition, "advanced grapple art-corrected Y");
    AssertEqual((ushort)4, samus.AnimationFrame, "advanced grapple angle art frame");

    // UpdateGrappleBeamTiles reads one 32-byte endpoint source and one angle-selected
    // 128-byte segment source from bank-$9B pointer tables, but queues bank $9A as the DMA
    // source. Give this angle unique pointers so a hard-coded host tile cannot pass.
    WriteTestWord(bus, 0x9bc342, 0x1234);
    WriteTestWord(bus, 0x9bc344, 0x1434);
    int foldedAngleOffset = (samus.Grapple.Angle >> 9) & 0xfe;
    WriteTestWord(bus, 0x9bc346 + foldedAngleOffset, 0x5678);

    // $94:AFBA recalculates its angle from endpoint minus flare. Here (49,-1) selects
    // angle byte $40 after the native coarse division, so sine index $80 produces a
    // visible +7 X step while negative-cosine index $40 leaves Y unchanged. Overwriting
    // index $80 now is safe: it was consumed earlier by pendulum positioning.
    WriteTestWord(bus, 0xa0b3c3 + 0x40 * 2, 0x0000);
    WriteTestWord(bus, 0xa0b3c3 + 0x80 * 2, 0x00ff);

    var grappleOam = new OamBuffer();
    var grappleVramWrites = new VramWriteQueue();
    grappleOam.BeginFrame();
    SamusGrappleMovement.DrawConnectedBeam(
        bus,
        samus.Grapple,
        grappleOam,
        grappleVramWrites,
        layer1X: 100,
        layer1Y: 50);

    AssertEqual(2, grappleVramWrites.Entries.Count, "grapple queues endpoint and segment tiles");
    AssertEqual(new VramWriteEntry(0x20, 0x9a1234, 0x6200), grappleVramWrites.Entries[0],
        "grapple endpoint tile DMA record");
    AssertEqual(new VramWriteEntry(0x80, 0x9a5678, 0x6210), grappleVramWrites.Entries[1],
        "grapple angle-selected segment DMA record");

    // Fifty pixels yields six body pieces because $94:AFBA uses (length / 8) before drawing
    // the endpoint. Instruction slots descend 15..10, so GrappleFunc_AF87's phases are
    // $24,$23,$22,$21,$24,$23 rather than one shared guessed animation tile.
    AssertEqual(28, grappleOam.NextByteOffset, "six grapple segments plus endpoint OAM bytes");
    int[] expectedTiles = [0x24, 0x23, 0x22, 0x21, 0x24, 0x23];
    for (int segment = 0; segment < expectedTiles.Length; segment++)
    {
        OamEntry entry = grappleOam.GetEntry(segment);
        AssertEqual(46 + segment * 7, entry.X, $"grapple segment {segment} X step");
        AssertEqual((byte)51, entry.Y, $"grapple segment {segment} Y step");
        AssertEqual(expectedTiles[segment], entry.TileNumber, $"grapple segment {segment} staggered tile");
        AssertEqual(5, entry.Palette, $"grapple segment {segment} palette");
        AssertEqual(3, entry.Priority, $"grapple segment {segment} priority");
        AssertTrue(entry.FlipX && !entry.FlipY, $"grapple segment {segment} angle flip");
        AssertTrue(!entry.IsLarge, $"grapple segment {segment} is small OBJ");
    }
    OamEntry grappleEndpoint = grappleOam.GetEntry(6);
    AssertEqual(95, grappleEndpoint.X, "grapple endpoint screen X");
    AssertEqual((byte)50, grappleEndpoint.Y, "grapple endpoint screen Y");
    AssertEqual(0x20, grappleEndpoint.TileNumber, "grapple endpoint tile");

    // Releasing Shoot runs $9B:CA65 now but queues $9B:CB8B for the next call. With signed
    // cosine -255 and doubled angular velocity 536, vertical magnitude is $000215E8.
    GrappleMovementResult queued = SamusGrappleMovement.Step(bus, swingLevel, samus, 0, 0);
    AssertTrue(queued.ReleaseQueued && !queued.Released, "grapple release is one-frame queued");
    AssertEqual(GrapplePhase.ReleaseFromSwing, samus.Grapple.Phase, "release function pointer phase");
    AssertEqual((ushort)2, samus.Kinematics.YSpeed, "grapple release whole Y speed");
    AssertEqual((ushort)0x15e8, samus.Kinematics.YSubspeed, "grapple release fractional Y speed");
    AssertEqual((ushort)1, samus.Kinematics.YDirection, "positive swing with negative cosine launches up");
    AssertEqual((ushort)0, samus.HorizontalSpeed.BaseSpeed, "grapple release whole X speed");
    AssertEqual((ushort)0x3458, samus.HorizontalSpeed.BaseSubspeed, "grapple release fractional X speed");
    AssertEqual((ushort)2, samus.HorizontalSpeed.AccelerationMode, "release selects deceleration mode");

    GrappleMovementResult released = SamusGrappleMovement.Step(bus, swingLevel, samus, 0, 0);
    AssertTrue(released.Released && !released.ReleaseQueued, "queued grapple release completes");
    AssertEqual(GrapplePhase.Inactive, samus.Grapple.Phase, "completed release clears grapple phase");
    AssertEqual(SamusState.NormalJumpForwardLeftPose, samus.Pose,
        "nonnegative angular velocity selects left-facing release pose $52");
    AssertEqual((ushort)2, samus.Kinematics.YSpeed, "release pose preserves whole Y velocity");
    AssertEqual((ushort)0x15e8, samus.Kinematics.YSubspeed, "release pose preserves fractional Y velocity");

    // Build a deliberately axis-aligned bank-$94 swing table around angle $40. At this
    // angle the radial vector points right: sine is +256 and negative cosine is zero. The
    // next whole angle byte ($41) is kept axis-aligned too, making all six probe coordinates
    // auditable without relying on the production scaler's trigonometric approximation.
    WriteTestWord(bus, 0xa0b3c3 + 0x40 * 2, 0x0000);
    WriteTestWord(bus, 0xa0b3c3 + 0x80 * 2, 0x0100);
    WriteTestWord(bus, 0xa0b3c3 + 0x41 * 2, 0x0000);
    WriteTestWord(bus, 0xa0b3c3 + 0x81 * 2, 0x0100);
    bus.WriteByte(0x9bc1c2 + 0x40, 0);
    bus.WriteBytes(0x9bc302, [0x00, 0x00]);

    // Anchor (128,128) is biased to (136,136). With length 32, candidate angle $41's
    // nearest probe is 40 pixels from the anchor at (176,136), block (11,8). Gravity adds
    // $18 to initial velocity $100, so collision must preserve last-safe angle $40.80 and
    // transform velocity $118 into -($118 >> 1) = -$8C while opening a 16-frame kick gate.
    var angularCollisionBlocks = new ushort[16 * 16];
    angularCollisionBlocks[8 * 16 + 11] = 0x8000;
    var angularCollisionLevel = new RoomLevelData(
        16,
        16,
        angularCollisionBlocks,
        new byte[angularCollisionBlocks.Length],
        new ushort[angularCollisionBlocks.Length],
        new byte[16]);
    var angularCollisionSamus = new SamusState();
    SamusGrappleMovement.ConnectUnobstructedSwing(
        bus,
        angularCollisionSamus,
        anchorX: 128,
        anchorY: 128,
        ropeLength: 32,
        angle: 0x4000,
        angularVelocity: 0x0100,
        faceRight: true);
    GrappleMovementResult angularCollision = SamusGrappleMovement.Step(
        bus,
        angularCollisionLevel,
        angularCollisionSamus,
        (ushort)SnesButton.X,
        newlyPressedInput: 0);
    AssertTrue(angularCollision.TerrainCollided, "grapple angular sweep reports terrain collision");
    AssertEqual(6, angularCollision.CollisionDistanceFromFeet,
        "nearest of six grapple body probes collides first");
    AssertEqual((ushort)0x4080, angularCollisionSamus.Grapple.Angle,
        "grapple collision restores last-safe angle plus half fraction");
    AssertEqual((short)-0x008c, angularCollisionSamus.Grapple.AngularVelocity,
        "grapple collision negates arithmetic half velocity");
    AssertEqual((ushort)16, angularCollisionSamus.Grapple.CollisionBounceTimer,
        "grapple collision opens sixteen-frame kick window");

    // The same six-point radial sweep has two damage-producing dispatcher entries. Solid
    // spike BTS zero/one queue `$003C/$0010` and still collide. Every other table entry is
    // literal zero, while a negative BTS skips the table. Use a fresh Samus for each row so
    // `$18A8` does not mask the next fixture's first contact.
    foreach ((byte behavior, ushort expectedDamage) in new[]
    {
        ((byte)0x00, (ushort)0x003c),
        ((byte)0x01, (ushort)0x0010),
        ((byte)0x02, (ushort)0x0000),
        ((byte)0x80, (ushort)0x0000),
    })
    {
        var spikeBlockWords = new ushort[16 * 16];
        var spikeBlockBts = new byte[spikeBlockWords.Length];
        spikeBlockWords[8 * 16 + 11] = 0xa000;
        spikeBlockBts[8 * 16 + 11] = behavior;
        var spikeBlockLevel = new RoomLevelData(
            16,
            16,
            spikeBlockWords,
            spikeBlockBts,
            new ushort[spikeBlockWords.Length],
            new byte[16]);
        var spikeBlockSamus = new SamusState();
        SamusGrappleMovement.ConnectUnobstructedSwing(
            bus,
            spikeBlockSamus,
            anchorX: 128,
            anchorY: 128,
            ropeLength: 32,
            angle: 0x4000,
            angularVelocity: 0x0100,
            faceRight: true);
        GrappleMovementResult spikeBlockContact = SamusGrappleMovement.Step(
            bus,
            spikeBlockLevel,
            spikeBlockSamus,
            (ushort)SnesButton.X,
            newlyPressedInput: 0);

        AssertTrue(spikeBlockContact.TerrainCollided,
            $"solid grapple spike BTS ${behavior:X2} remains collision");
        AssertEqual(expectedDamage, spikeBlockSamus.LiquidPhysics.PeriodicDamage,
            $"solid grapple spike BTS ${behavior:X2} damage table");
        AssertEqual(expectedDamage == 0 ? (ushort)0 : (ushort)0x003c,
            spikeBlockSamus.InvincibilityTimer,
            $"solid grapple spike BTS ${behavior:X2} invincibility publication");
        AssertEqual(expectedDamage == 0 ? (ushort)0 : (ushort)0x000a,
            spikeBlockSamus.KnockbackTimer,
            $"solid grapple spike BTS ${behavior:X2} knockback-timer publication");
    }

    // Spike-air never stops the pendulum. Only BTS two has nonzero damage; because the
    // axis-aligned fixture samples block (11,8) more than once, an exact `$0010` result also
    // proves the first sample's invincibility timer suppresses later samples in this frame.
    foreach ((byte behavior, ushort expectedDamage) in new[]
    {
        ((byte)0x00, (ushort)0x0000),
        ((byte)0x02, (ushort)0x0010),
        ((byte)0x82, (ushort)0x0000),
    })
    {
        var spikeAirWords = new ushort[16 * 16];
        var spikeAirBts = new byte[spikeAirWords.Length];
        spikeAirWords[8 * 16 + 11] = 0x2000;
        spikeAirBts[8 * 16 + 11] = behavior;
        var spikeAirLevel = new RoomLevelData(
            16,
            16,
            spikeAirWords,
            spikeAirBts,
            new ushort[spikeAirWords.Length],
            new byte[16]);
        var spikeAirSamus = new SamusState();
        SamusGrappleMovement.ConnectUnobstructedSwing(
            bus,
            spikeAirSamus,
            anchorX: 128,
            anchorY: 128,
            ropeLength: 32,
            angle: 0x4000,
            angularVelocity: 0x0100,
            faceRight: true);
        GrappleMovementResult spikeAirContact = SamusGrappleMovement.Step(
            bus,
            spikeAirLevel,
            spikeAirSamus,
            (ushort)SnesButton.X,
            newlyPressedInput: 0);

        AssertTrue(!spikeAirContact.TerrainCollided,
            $"grapple spike-air BTS ${behavior:X2} remains noncollision");
        AssertEqual(expectedDamage, spikeAirSamus.LiquidPhysics.PeriodicDamage,
            $"grapple spike-air BTS ${behavior:X2} damage table");
        AssertEqual(expectedDamage == 0 ? (ushort)0 : (ushort)0x003c,
            spikeAirSamus.InvincibilityTimer,
            $"grapple spike-air BTS ${behavior:X2} invincibility publication");
        AssertEqual(expectedDamage == 0 ? (ushort)0 : (ushort)0x000a,
            spikeAirSamus.KnockbackTimer,
            $"grapple spike-air BTS ${behavior:X2} knockback-timer publication");
        if (expectedDamage != 0)
        {
            spikeAirSamus.DecrementHurtTimers();
            AssertEqual((ushort)0x003b, spikeAirSamus.InvincibilityTimer,
                "gameplay tail ages grapple-created invincibility timer");
            AssertEqual((ushort)0x0009, spikeAirSamus.KnockbackTimer,
                "gameplay tail ages grapple-created knockback timer");
        }
    }

    // Move the reflected pendulum into empty terrain and press Jump during the kick window.
    // Gravity/correction update -$8C to -$6F, then $9B:BD44 adds -$300 extra velocity.
    // Total -$36F moves $40.80 to $3D.11; success ages the timer and damps only the extra
    // word from -$300 to -$2FA. This is the real collision-assisted grapple kick, not an
    // ordinary aerial jump applied to Samus's X/Y velocity.
    GrappleMovementResult afterAngularCollision = SamusGrappleMovement.Step(
        bus,
        swingLevel,
        angularCollisionSamus,
        (ushort)(SnesButton.X | SnesButton.B),
        newlyPressedInput: (ushort)SnesButton.B);
    AssertTrue(!afterAngularCollision.TerrainCollided,
        "collision kick crosses three clear angle-byte terrain sweeps");
    AssertEqual((ushort)0x3d11, angularCollisionSamus.Grapple.Angle,
        "grapple collision kick advances exact reflected-plus-extra angle");
    AssertEqual((short)-0x006f, angularCollisionSamus.Grapple.AngularVelocity,
        "grapple kick keeps gravity-corrected base angular velocity");
    AssertEqual((short)-0x02fa, angularCollisionSamus.Grapple.JumpImpulse,
        "successful grapple kick damps extra angular velocity by six");
    AssertEqual((ushort)15, angularCollisionSamus.Grapple.CollisionBounceTimer,
        "successful post-bounce movement ages kick window");

    // Rope growth uses a different radial frontier: candidate length 33 plus 56 pixels puts
    // the probe at X=225, block 14. Make that cell a horizontal extension to solid block 15.
    // Correct per-pixel length handling must reject 33, retain 32, and leave +2 active to
    // retry next frame; treating the extension itself as air would incorrectly grow the rope.
    var ropeCollisionBlocks = new ushort[16 * 16];
    var ropeCollisionBts = new byte[ropeCollisionBlocks.Length];
    ropeCollisionBlocks[8 * 16 + 14] = 0x5000;
    ropeCollisionBts[8 * 16 + 14] = 1;
    ropeCollisionBlocks[8 * 16 + 15] = 0x8000;
    var ropeCollisionLevel = new RoomLevelData(
        16,
        16,
        ropeCollisionBlocks,
        ropeCollisionBts,
        new ushort[ropeCollisionBlocks.Length],
        new byte[16]);
    var ropeCollisionSamus = new SamusState();
    SamusGrappleMovement.ConnectUnobstructedSwing(
        bus,
        ropeCollisionSamus,
        anchorX: 128,
        anchorY: 128,
        ropeLength: 32,
        angle: 0x4000,
        angularVelocity: 0,
        faceRight: true);
    GrappleMovementResult ropeCollision = SamusGrappleMovement.Step(
        bus,
        ropeCollisionLevel,
        ropeCollisionSamus,
        (ushort)(SnesButton.X | SnesButton.Down),
        newlyPressedInput: (ushort)SnesButton.Down);
    AssertTrue(ropeCollision.RopeLengthBlocked,
        "grapple rope growth follows extension BTS to solid collision");
    AssertEqual((ushort)32, ropeCollisionSamus.Grapple.RopeLength,
        "blocked grapple rope keeps last accepted length");
    AssertEqual((short)2, ropeCollisionSamus.Grapple.RopeLengthDelta,
        "blocked grapple rope retains signed retry delta");

    // The already-connected public seam normally skips block validation. Opt this fixture
    // into the same flag installed by real firing, then present air at the stored anchor.
    // Gravity creates nonzero momentum, selecting the translated release path rather than
    // the still-untranslated zero-speed dropped handler.
    var disconnectedAnchorSamus = new SamusState();
    SamusGrappleMovement.ConnectUnobstructedSwing(
        bus,
        disconnectedAnchorSamus,
        anchorX: 128,
        anchorY: 128,
        ropeLength: 32,
        angle: 0x4000,
        angularVelocity: 0,
        faceRight: true);
    disconnectedAnchorSamus.Grapple.ValidateAnchorBlock = true;
    GrappleMovementResult disconnectedAnchor = SamusGrappleMovement.Step(
        bus,
        new RoomLevelData(
            16,
            16,
            new ushort[16 * 16],
            new byte[16 * 16],
            new ushort[16 * 16],
            new byte[16]),
        disconnectedAnchorSamus,
        (ushort)SnesButton.X,
        newlyPressedInput: 0);
    AssertTrue(disconnectedAnchor.AnchorDisconnected && disconnectedAnchor.ReleaseQueued,
        "air replacing a validated grapple anchor queues moving release");

    // Rebuild the small-angle neighborhood around retail special angle `$6A80`. Both the
    // current `$6A` sample and candidate `$6B` point right, so the initial eight-pixel rope
    // places Samus at (144,136), while the first body probe reaches solid block (9,8).
    // Bank $94 must stop at `$6A80`; bank $9B must then consume record four's literal
    // `$B9`, (+24,+16), `$C814` tuple rather than inventing a host-side angle range.
    WriteTestWord(bus, 0xa0b3c3 + 0x6a * 2, 0x0000);
    WriteTestWord(bus, 0xa0b3c3 + 0xaa * 2, 0x0100);
    WriteTestWord(bus, 0xa0b3c3 + 0x6b * 2, 0x0000);
    WriteTestWord(bus, 0xa0b3c3 + 0xab * 2, 0x0100);
    bus.WriteByte(0x9bc1c2 + 0x6a, 0);
    bus.WriteBytes(0x9bc302, [0x00, 0x00]);
    int wallGrabRecord = 0x9bc43e + 4 * 10;
    WriteTestWord(bus, wallGrabRecord, 0x6a80);
    WriteTestWord(bus, wallGrabRecord + 2, SamusState.GrappleWallContactRightPose);
    WriteTestWord(bus, wallGrabRecord + 4, 24);
    WriteTestWord(bus, wallGrabRecord + 6, 16);
    WriteTestWord(bus, wallGrabRecord + 8, 0xc814);

    var specialBlocks = new ushort[16 * 16];
    specialBlocks[8 * 16 + 9] = 0x8000; // candidate `$6B`, nearest radial probe
    specialBlocks[8 * 16 + 8] = 0x8000; // `$B9`'s later 16-pixel left wall probe
    specialBlocks[9 * 16 + 8] = 0x8000;
    specialBlocks[10 * 16 + 8] = 0x8000;
    var specialLevel = new RoomLevelData(
        16,
        16,
        specialBlocks,
        new byte[specialBlocks.Length],
        new ushort[specialBlocks.Length],
        new byte[16]);
    var wallGrabSamus = new SamusState();
    SamusGrappleMovement.ConnectUnobstructedSwing(
        bus,
        wallGrabSamus,
        anchorX: 128,
        anchorY: 128,
        ropeLength: 8,
        angle: 0x6a00,
        angularVelocity: 0x0200,
        faceRight: true);
    GrappleMovementResult wallGrab = SamusGrappleMovement.Step(
        bus,
        specialLevel,
        wallGrabSamus,
        (ushort)SnesButton.X,
        newlyPressedInput: 0);
    AssertTrue(wallGrab.TerrainCollided && wallGrab.SpecialAngleHandled && wallGrab.WallGrabEntered,
        "close grapple collision enters ROM-selected wall-grab function");
    AssertEqual(6, wallGrab.CollisionDistanceFromFeet,
        "wall-grab special route requires nearest radial probe");
    AssertEqual(GrapplePhase.WallGrab, wallGrabSamus.Grapple.Phase,
        "special record installs `$C814` wall-grab phase");
    AssertEqual(SamusState.GrappleWallContactRightPose, wallGrabSamus.Pose,
        "special record installs literal wall-contact pose `$B9`");
    AssertEqual((ushort)160, wallGrabSamus.XPosition,
        "wall-grab snap applies anchor-relative +24 X");
    AssertEqual((ushort)152, wallGrabSamus.YPosition,
        "wall-grab snap applies anchor-relative +16 Y");
    AssertEqual<ushort?>(148, wallGrab.CameraPreviousX,
        "special snap clamps previous camera X to twelve pixels");
    AssertEqual<ushort?>(140, wallGrab.CameraPreviousY,
        "special snap clamps previous camera Y to twelve pixels");

    GrappleMovementResult heldWall = SamusGrappleMovement.Step(
        bus, specialLevel, wallGrabSamus, (ushort)SnesButton.X, newlyPressedInput: 0);
    AssertTrue(heldWall.WallGrabEntered && wallGrabSamus.Grapple.Phase == GrapplePhase.WallGrab,
        "held Shoot retains frozen wall-grab pose");
    GrappleMovementResult wallReleased = SamusGrappleMovement.Step(
        bus, specialLevel, wallGrabSamus, controllerInput: 0, newlyPressedInput: 0);
    AssertTrue(wallReleased.WallJumpWindowOpened,
        "wall-grab release opens native thirty-check wall-jump window");
    AssertEqual((ushort)30, wallGrabSamus.Grapple.WallJumpTimer,
        "wall-grab release seeds decimal thirty before decrementing");

    // `$B9` faces left (pose direction four), so `$90:9CAC` probes left even though the
    // later launch selects right-facing wall-jump pose `$83`. Jump must be a fresh A edge;
    // the grapple function queues `$C9CE` now and performs the launch on the next call.
    GrappleMovementResult wallJumpQueued = SamusGrappleMovement.Step(
        bus,
        specialLevel,
        wallGrabSamus,
        controllerInput: (ushort)SnesButton.A,
        newlyPressedInput: (ushort)SnesButton.A);
    AssertTrue(wallJumpQueued.WallProbeCollided && wallJumpQueued.WallJumpQueued,
        "fresh Jump plus wall probe queues grapple wall jump");
    AssertEqual((ushort)29, wallGrabSamus.Grapple.WallJumpTimer,
        "first eligible wall-jump check decrements timer to twenty-nine");

    // `$9B:C9CE` mirrors the ordinary route's selective cleanup: it zeros base speed but
    // preserves the Dash pair and `$0B3C`. Seed the state after the queueing call so this
    // assertion isolates the launch function itself from earlier grapple-swing behavior.
    wallGrabSamus.HorizontalSpeed.ExtraRunSpeed = 1;
    wallGrabSamus.HorizontalSpeed.ExtraRunSubspeed = 0x7000;
    wallGrabSamus.HorizontalSpeed.HasRunningMomentum = true;
    wallGrabSamus.ProjectileFlareCounter = 0x003c;
    wallGrabSamus.LiquidPhysics.BeginFrameSoundRequests();
    GrappleMovementResult wallJumpStarted = SamusGrappleMovement.Step(
        bus, specialLevel, wallGrabSamus, controllerInput: 0, newlyPressedInput: 0);
    AssertTrue(wallJumpStarted.WallJumpStarted,
        "queued grapple wall jump starts on following function call");
    AssertEqual(GrapplePhase.Inactive, wallGrabSamus.Grapple.Phase,
        "grapple wall jump clears connected function");
    AssertEqual(SamusState.WallJumpRightPose, wallGrabSamus.Pose,
        "left-facing `$B9` contact reverses to wall-jump pose `$83`");
    AssertEqual((ushort)4, wallGrabSamus.Kinematics.YSpeed,
        "grapple wall jump reads dry whole speed from ROM");
    AssertEqual((ushort)0xa000, wallGrabSamus.Kinematics.YSubspeed,
        "grapple wall jump reads dry fractional speed from ROM");
    AssertEqual((ushort)1, wallGrabSamus.Kinematics.YDirection,
        "grapple wall jump launches upward");
    AssertEqual((ushort)1, wallGrabSamus.HorizontalSpeed.ExtraRunSpeed,
        "grapple wall jump preserves Dash whole speed");
    AssertEqual((ushort)0x7000, wallGrabSamus.HorizontalSpeed.ExtraRunSubspeed,
        "grapple wall jump preserves Dash fraction");
    AssertTrue(wallGrabSamus.HorizontalSpeed.HasRunningMomentum,
        "grapple wall jump preserves Dash momentum flag");
    AssertEqual((ushort)0, wallGrabSamus.ProjectileFlareCounter,
        "grapple wall jump clears the active projectile flare counter");
    AssertEqual(new SamusSoundRequest(1, 0x07, 15), wallGrabSamus.LiquidPhysics.SoundRequests.Single(),
        "grapple wall jump queues generic library-one sound seven");
    AssertEqual((ushort)0, wallGrabSamus.Grapple.RopeLength,
        "grapple wall jump removes rope state");

    // Repeat the same authentic entry but do not press Jump. DEC/BPL permits exactly thirty
    // wall checks (timer 29 through zero); the thirty-first call wraps to `$FFFF` and queues
    // `$C8C5`. Pose `$B9` has direction six and radius sixteen, so table `$C9C4[6]` must
    // choose compact diagonal-down-left `$74`, not a generic falling approximation.
    var expiredWallGrabSamus = new SamusState();
    SamusGrappleMovement.ConnectUnobstructedSwing(
        bus,
        expiredWallGrabSamus,
        anchorX: 128,
        anchorY: 128,
        ropeLength: 8,
        angle: 0x6a00,
        angularVelocity: 0x0200,
        faceRight: true);
    SamusGrappleMovement.Step(
        bus, specialLevel, expiredWallGrabSamus, (ushort)SnesButton.X, newlyPressedInput: 0);
    SamusGrappleMovement.Step(
        bus, specialLevel, expiredWallGrabSamus, controllerInput: 0, newlyPressedInput: 0);
    for (int eligibleCheck = 0; eligibleCheck < 30; eligibleCheck++)
    {
        GrappleMovementResult grace = SamusGrappleMovement.Step(
            bus, specialLevel, expiredWallGrabSamus, controllerInput: 0, newlyPressedInput: 0);
        AssertEqual(GrapplePhase.WallGrabRelease, grace.Phase,
            $"wall-grab grace check {eligibleCheck + 1} remains live");
    }
    GrappleMovementResult dropQueued = SamusGrappleMovement.Step(
        bus, specialLevel, expiredWallGrabSamus, controllerInput: 0, newlyPressedInput: 0);
    AssertTrue(dropQueued.DropQueued && dropQueued.Phase == GrapplePhase.Dropped,
        "wall-grab grace underflow queues dropped function");
    bus.WriteByte(0x9bc9c4 + 6, 0x74); // literal compact dropped-pose table entry
    GrappleMovementResult dropped = SamusGrappleMovement.Step(
        bus, specialLevel, expiredWallGrabSamus, controllerInput: 0, newlyPressedInput: 0);
    AssertTrue(dropped.Dropped && expiredWallGrabSamus.Grapple.Phase == GrapplePhase.Inactive,
        "queued dropped handler clears grapple on following call");
    AssertEqual(SamusState.CrouchingAimDiagonalDownLeftPose, expiredWallGrabSamus.Pose,
        "compact dropped table preserves `$B9` diagonal-down aim");
    AssertEqual((ushort)0, expiredWallGrabSamus.Kinematics.YSpeed,
        "dropped handler clears whole vertical speed");
    AssertEqual((ushort)0, expiredWallGrabSamus.Kinematics.YSubspeed,
        "dropped handler clears fractional vertical speed");

    // Record zero is the locked `$D680 -> $B6` route. Candidate `$D7` again points right
    // into block (9,8); a two-byte angular step guarantees that sample is visited after
    // quadrant gravity. Releasing Shoot queues `$C856`, whose movement-type-$16` fallback
    // byte in the literal `$B6` definition selects stable crouch `$27` one call later.
    WriteTestWord(bus, 0xa0b3c3 + 0xd6 * 2, 0x0000);
    WriteTestWord(bus, 0xa0b3c3 + 0x16 * 2, 0x0100);
    WriteTestWord(bus, 0xa0b3c3 + 0xd7 * 2, 0x0000);
    WriteTestWord(bus, 0xa0b3c3 + 0x17 * 2, 0x0100);
    bus.WriteByte(0x9bc1c2 + 0xd6, 0);
    int lockedRecord = 0x9bc43e;
    WriteTestWord(bus, lockedRecord, 0xd680);
    WriteTestWord(bus, lockedRecord + 2, SamusState.GrappleCrouchingDownRightPose);
    WriteTestWord(bus, lockedRecord + 4, unchecked((ushort)-30));
    WriteTestWord(bus, lockedRecord + 6, unchecked((ushort)-24));
    WriteTestWord(bus, lockedRecord + 8, 0xc77e);
    var lockedSamus = new SamusState();
    SamusGrappleMovement.ConnectUnobstructedSwing(
        bus,
        lockedSamus,
        anchorX: 128,
        anchorY: 128,
        ropeLength: 8,
        angle: 0xd600,
        angularVelocity: 0x0200,
        faceRight: true);
    GrappleMovementResult locked = SamusGrappleMovement.Step(
        bus, specialLevel, lockedSamus, (ushort)SnesButton.X, newlyPressedInput: 0);
    AssertTrue(locked.SpecialAngleHandled && locked.LockedInPlace,
        "close collision enters ROM-selected locked function");
    AssertEqual(GrapplePhase.ConnectedLocked, lockedSamus.Grapple.Phase,
        "special record installs `$C77E` locked phase");
    AssertEqual(SamusState.GrappleCrouchingDownRightPose, lockedSamus.Pose,
        "locked special record installs pose `$B6`");
    AssertEqual((ushort)106, lockedSamus.XPosition,
        "locked snap applies signed -30 X offset");
    AssertEqual((ushort)112, lockedSamus.YPosition,
        "locked snap applies signed -24 Y offset");
    GrappleMovementResult lockedHeld = SamusGrappleMovement.Step(
        bus, specialLevel, lockedSamus, (ushort)SnesButton.X, newlyPressedInput: 0);
    AssertTrue(lockedHeld.LockedInPlace,
        "held Shoot preserves special locked body without pendulum integration");
    GrappleMovementResult lockedCancelQueued = SamusGrappleMovement.Step(
        bus, specialLevel, lockedSamus, controllerInput: 0, newlyPressedInput: 0);
    AssertTrue(lockedCancelQueued.CancelQueued && lockedCancelQueued.OwnsMovement,
        "locked release queues connected-pose cancellation");
    GrappleMovementResult lockedCancelled =
        SamusGrappleMovement.CompleteFiringCancellation(bus, lockedSamus);
    AssertTrue(lockedCancelled.Cancelled && lockedCancelled.OwnsMovement,
        "connected cancellation owns pose-fallback frame");
    AssertEqual(SamusState.CrouchingRightPose, lockedSamus.Pose,
        "locked `$B6` cancellation follows definition fallback `$27`");
    AssertEqual((ushort)0, lockedSamus.Grapple.RopeLength,
        "locked cancellation clears rope state");

    AssertThrows<ArgumentOutOfRangeException>(
        () => SamusGrappleMovement.ConnectUnobstructedSwing(
            bus, new SamusState(), 0, 0, ropeLength: 7, angle: 0, angularVelocity: 0, faceRight: true),
        "grapple rejects a rope shorter than retail connected minimum");

    Console.WriteLine("  Samus grapple: firing, swing collision, locked/wall-grab specials, wall jump, dropped pose, beam OAM, and release agree.");
}

static void VerifyBreakableGrapplePlms()
{
    var bus = new TestAddressSpace();

    // These bytes are the literal $84:CD6A and $84:CDA9 instruction streams. Production
    // code interprets these ROM words; the fixture does not inject a friendly C# timeline.
    bus.WriteBytes(0x84cd6a, [
        0xf0, 0x00, 0xf9, 0xa4,
        0x10, 0x8c, 0x0a,
        0x04, 0x00, 0xff, 0xa4,
        0x04, 0x00, 0x05, 0xa5,
        0x04, 0x00, 0x0b, 0xa5,
        0x06, 0x00, 0x11, 0xa5,
        0x04, 0x00, 0x0b, 0xa5,
        0x04, 0x00, 0x05, 0xa5,
        0x04, 0x00, 0xff, 0xa4,
        0x93, 0xcd,
        0x17, 0x8b,
        0xbc, 0x86,
    ]);
    bus.WriteBytes(0x84cda9, [
        0x78, 0x00, 0xf9, 0xa4,
        0x10, 0x8c, 0x0a,
        0x04, 0x00, 0xff, 0xa4,
        0x04, 0x00, 0x05, 0xa5,
        0x04, 0x00, 0x0b, 0xa5,
        0x01, 0x00, 0x11, 0xa5,
        0xbc, 0x86,
    ]);

    // Each native draw instruction is `{one block, complete level word, terminator}`.
    bus.WriteBytes(0x84a4f9, [0x01, 0x00, 0xb7, 0xe0, 0x00, 0x00]);
    bus.WriteBytes(0x84a4ff, [0x01, 0x00, 0x53, 0x00, 0x00, 0x00]);
    bus.WriteBytes(0x84a505, [0x01, 0x00, 0x54, 0x00, 0x00, 0x00]);
    bus.WriteBytes(0x84a50b, [0x01, 0x00, 0x55, 0x00, 0x00, 0x00]);
    bus.WriteBytes(0x84a511, [0x01, 0x00, 0xff, 0x00, 0x00, 0x00]);

    const int width = 8;
    const int height = 8;
    const int blockIndex = 3 * width + 3;
    var definitions = new byte[0x400 * 8];
    WriteDefinitionWord(definitions, 0xb7, 0, 0x1111);
    WriteDefinitionWord(definitions, 0xb7, 1, 0x2222);
    WriteDefinitionWord(definitions, 0xb7, 2, 0x3333);
    WriteDefinitionWord(definitions, 0xb7, 3, 0x4444);

    static RoomLevelData CreateLevel(byte behavior, byte[] blockDefinitions)
    {
        var foreground = new ushort[width * height];
        var bts = new byte[foreground.Length];
        foreground[blockIndex] = 0xe123;
        bts[blockIndex] = behavior;
        return new RoomLevelData(
            width,
            height,
            foreground,
            bts,
            new ushort[foreground.Length],
            blockDefinitions);
    }

    static void StepMany(
        RoomPlmSystem plms,
        TestAddressSpace addressSpace,
        RoomLevelData level,
        BackgroundTilemapStreamer streamer,
        int count)
    {
        for (int frame = 0; frame < count; frame++)
            plms.Step(addressSpace, level, streamer, 0, 0, 0);
    }

    // BTS one waits 240 handler calls, breaks through four exact visual words, reverses
    // through the same words, restores both the saved level word and BTS one, then deletes
    // on the following pass because DrawPLMBlock deliberately seeded timer one.
    RoomLevelData respawning = CreateLevel(1, definitions);
    BackgroundTilemapStreamer respawningStreamer = respawning.CreateBackgroundStreamer();
    var respawningPlms = new RoomPlmSystem();
    AssertTrue(respawningPlms.TrySpawnBreakableGrappleBlock(respawning, blockIndex, 1),
        "respawning grapple PLM occupies a native slot");
    AssertEqual((byte)0, respawning.GetCollisionBlockByIndex(blockIndex).Behavior,
        "CFB5 clears breakable grapple BTS immediately");
    AssertEqual((ushort)0xe123, respawning.GetCollisionBlockByIndex(blockIndex).LevelWord,
        "CFB5 retains original level word until handler");

    IReadOnlyList<PlmTilemapUpdate> firstDraw = respawningPlms.Step(
        bus, respawning, respawningStreamer, 0, 0, 0);
    AssertEqual((ushort)0xe0b7, respawning.GetCollisionBlockByIndex(blockIndex).LevelWord,
        "connection-frame PLM pass draws grapple frame zero");
    AssertEqual(1, firstDraw.Count, "visible grapple mutation emits one VRAM redraw");
    AssertEqual((ushort)0x50c6, firstDraw[0].TopRowDestination,
        "PLM redraw targets block (3,3) in left BG1 ring screen");
    AssertEqual((ushort)0x1111, firstDraw[0].TopRow[0],
        "PLM redraw expands ROM-selected visual block definition");

    StepMany(respawningPlms, bus, respawning, respawningStreamer, 239);
    AssertEqual((ushort)0xe0b7, respawning.GetCollisionBlockByIndex(blockIndex).LevelWord,
        "respawning block remains grapple terrain through timer 240 minus one");
    respawningPlms.Step(bus, respawning, respawningStreamer, 0, 0, 0);
    AssertEqual((ushort)0x0053, respawning.GetCollisionBlockByIndex(blockIndex).LevelWord,
        "timer 240 expiry draws first air frame");
    AssertEqual(1, respawningPlms.SoundRequests.Count,
        "break transition queues one sound request");
    AssertEqual(new PlmSoundRequest(2, 0x0a, 6), respawningPlms.SoundRequests[0],
        "break transition uses library two sound $0A with maximum six");

    StepMany(respawningPlms, bus, respawning, respawningStreamer, 4);
    AssertEqual((ushort)0x0054, respawning.GetCollisionBlockByIndex(blockIndex).LevelWord,
        "respawning break frame one advances after four");
    StepMany(respawningPlms, bus, respawning, respawningStreamer, 4);
    AssertEqual((ushort)0x0055, respawning.GetCollisionBlockByIndex(blockIndex).LevelWord,
        "respawning break frame two advances after four");
    StepMany(respawningPlms, bus, respawning, respawningStreamer, 6);
    AssertEqual((ushort)0x00ff, respawning.GetCollisionBlockByIndex(blockIndex).LevelWord,
        "respawning break reaches blank frame after six");
    StepMany(respawningPlms, bus, respawning, respawningStreamer, 4 + 4 + 4 + 4);
    AssertEqual((ushort)0xe123, respawning.GetCollisionBlockByIndex(blockIndex).LevelWord,
        "DrawPLMBlock restores the complete saved grapple word");
    AssertEqual((byte)1, respawning.GetCollisionBlockByIndex(blockIndex).Behavior,
        "respawning instruction restores BTS one before terrain");
    AssertEqual(1, respawningPlms.ActiveCount,
        "restoration pass retains PLM for timer-one delete delay");
    respawningPlms.Step(bus, respawning, respawningStreamer, 0, 0, 0);
    AssertEqual(0, respawningPlms.ActiveCount,
        "restored grapple PLM deletes on following handler pass");

    // BTS two uses the shorter 120-frame delay and never restores the saved word/BTS.
    RoomLevelData permanent = CreateLevel(2, definitions);
    BackgroundTilemapStreamer permanentStreamer = permanent.CreateBackgroundStreamer();
    var permanentPlms = new RoomPlmSystem();
    AssertTrue(permanentPlms.TrySpawnBreakableGrappleBlock(permanent, blockIndex, 2),
        "nonrespawning grapple PLM occupies a native slot");
    permanentPlms.Step(bus, permanent, permanentStreamer, 0, 0, 0);
    StepMany(permanentPlms, bus, permanent, permanentStreamer, 119);
    AssertEqual((ushort)0xe0b7, permanent.GetCollisionBlockByIndex(blockIndex).LevelWord,
        "nonrespawning block retains grapple terrain through timer 120 minus one");
    permanentPlms.Step(bus, permanent, permanentStreamer, 0, 0, 0);
    AssertEqual((ushort)0x0053, permanent.GetCollisionBlockByIndex(blockIndex).LevelWord,
        "nonrespawning timer 120 begins break sequence");
    StepMany(permanentPlms, bus, permanent, permanentStreamer, 4 + 4 + 4);
    AssertEqual((ushort)0x00ff, permanent.GetCollisionBlockByIndex(blockIndex).LevelWord,
        "nonrespawning sequence ends on blank-air visual word");
    permanentPlms.Step(bus, permanent, permanentStreamer, 0, 0, 0);
    AssertEqual(0, permanentPlms.ActiveCount,
        "nonrespawning sequence deletes one frame after timer-one blank draw");
    AssertEqual((byte)0, permanent.GetCollisionBlockByIndex(blockIndex).Behavior,
        "nonrespawning sequence leaves cleared BTS");

    // The following bytes are the exact BTS-3/BTS-6 collision heads and their shared
    // bank-$84 tails. Together they exercise every structural feature that the bomb-block
    // family adds over grapple: queue-cap three, GotoY, a signed-offset second row, a true
    // vertical record, linked-block restoration, and the respawning/permanent split.
    bus.WriteBytes(0x84ccb7, [0x46, 0x8c, 0x06, 0x24, 0x87, 0xc1, 0xcc]);
    bus.WriteBytes(0x84ccc1, [
        0x04, 0x00, 0x9d, 0xa3,
        0x04, 0x00, 0xad, 0xa3,
        0x04, 0x00, 0xbd, 0xa3,
        0x80, 0x01, 0xcd, 0xa3,
        0x04, 0x00, 0xbd, 0xa3,
        0x04, 0x00, 0xad, 0xa3,
        0x04, 0x00, 0x9d, 0xa3,
        0x01, 0x00, 0xd7, 0xa4,
        0xbc, 0x86,
    ]);
    bus.WriteBytes(0x84cd1b, [0x46, 0x8c, 0x06, 0x24, 0x87, 0x25, 0xcd]);
    bus.WriteBytes(0x84cd25, [
        0x04, 0x00, 0x7d, 0xa3,
        0x04, 0x00, 0x85, 0xa3,
        0x04, 0x00, 0x8d, 0xa3,
        0x01, 0x00, 0x95, 0xa3,
        0xbc, 0x86,
    ]);

    // `$A39D-$A3DC`: 2x2 animation draw records. Each begins with a two-word horizontal
    // row, then signed offset bytes `{0,+1}`, another two-word row, and a zero terminator.
    static void Write2x2Draw(TestAddressSpace fixtureBus, int address, ushort levelWord)
    {
        fixtureBus.WriteBytes(address, [
            0x02, 0x00,
            unchecked((byte)levelWord), unchecked((byte)(levelWord >> 8)),
            unchecked((byte)levelWord), unchecked((byte)(levelWord >> 8)),
            0x00, 0x01,
            0x02, 0x00,
            unchecked((byte)levelWord), unchecked((byte)(levelWord >> 8)),
            unchecked((byte)levelWord), unchecked((byte)(levelWord >> 8)),
            0x00, 0x00,
        ]);
    }
    Write2x2Draw(bus, 0x84a39d, 0x0053);
    Write2x2Draw(bus, 0x84a3ad, 0x0054);
    Write2x2Draw(bus, 0x84a3bd, 0x0055);
    Write2x2Draw(bus, 0x84a3cd, 0x00ff);
    bus.WriteBytes(0x84a4d7, [
        0x02, 0x00, 0x58, 0xf0, 0x58, 0x50,
        0x00, 0x01,
        0x02, 0x00, 0x58, 0xd0, 0x58, 0xd0,
        0x00, 0x00,
    ]);

    // `$A37D-$A39C`: vertical two-word records used by 1x2 blocks. The high count bit is
    // significant—the second level word advances by room width, never by one column.
    static void Write1x2Draw(TestAddressSpace fixtureBus, int address, ushort levelWord)
    {
        fixtureBus.WriteBytes(address, [
            0x02, 0x80,
            unchecked((byte)levelWord), unchecked((byte)(levelWord >> 8)),
            unchecked((byte)levelWord), unchecked((byte)(levelWord >> 8)),
            0x00, 0x00,
        ]);
    }
    Write1x2Draw(bus, 0x84a37d, 0x0053);
    Write1x2Draw(bus, 0x84a385, 0x0054);
    Write1x2Draw(bus, 0x84a38d, 0x0055);
    Write1x2Draw(bus, 0x84a395, 0x00ff);

    static RoomLevelData CreateBombLevel(byte behavior, byte[] blockDefinitions)
    {
        var foreground = new ushort[width * height];
        var bts = new byte[foreground.Length];
        foreground[blockIndex] = 0xf321;
        bts[blockIndex] = behavior;
        return new RoomLevelData(
            width,
            height,
            foreground,
            bts,
            new ushort[foreground.Length],
            blockDefinitions);
    }

    RoomLevelData respawning2x2 = CreateBombLevel(3, definitions);
    BackgroundTilemapStreamer respawning2x2Streamer =
        respawning2x2.CreateBackgroundStreamer();
    var respawning2x2Plms = new RoomPlmSystem();
    AssertTrue(respawning2x2Plms.TrySpawnCollisionBombBlock(
        respawning2x2, blockIndex, behavior: 3),
        "BTS-3 collision setup occupies a native PLM slot");
    AssertEqual((ushort)0x0321, respawning2x2.GetCollisionBlockByIndex(blockIndex).LevelWord,
        "CE83 synchronously removes only the type-F collision nibble");
    IReadOnlyList<PlmTilemapUpdate> first2x2Draw = respawning2x2Plms.Step(
        bus, respawning2x2, respawning2x2Streamer, 0, 0, 0);
    AssertEqual(new PlmSoundRequest(2, 0x06, 3), respawning2x2Plms.SoundRequests[0],
        "collision break queues library-two sound six with native maximum three");
    AssertEqual(4, first2x2Draw.Count,
        "2x2 draw record emits one debugger-visible redraw for each mutated level word");
    AssertEqual((ushort)0x0053,
        respawning2x2.GetCollisionBlock(3, 3).LevelWord,
        "2x2 first row begins at PLM origin");
    AssertEqual((ushort)0x0053,
        respawning2x2.GetCollisionBlock(4, 4).LevelWord,
        "signed {0,+1} record draws the 2x2 lower-right block");

    StepMany(respawning2x2Plms, bus, respawning2x2, respawning2x2Streamer, 12);
    AssertEqual((ushort)0x00ff, respawning2x2.GetCollisionBlock(4, 4).LevelWord,
        "BTS-3 reaches its four-block blank frame after three four-frame transitions");
    StepMany(respawning2x2Plms, bus, respawning2x2, respawning2x2Streamer, 384 + 12);
    AssertEqual((ushort)0xf058, respawning2x2.GetCollisionBlock(3, 3).LevelWord,
        "BTS-3 restores the type-F parent after the exact 384-frame blank hold");
    AssertEqual((ushort)0x5058, respawning2x2.GetCollisionBlock(4, 3).LevelWord,
        "BTS-3 restores the type-5 horizontal extension");
    AssertEqual((ushort)0xd058, respawning2x2.GetCollisionBlock(3, 4).LevelWord,
        "BTS-3 restores the type-D vertical extension row");
    AssertEqual(1, respawning2x2Plms.ActiveCount,
        "timer-one restored frame keeps BTS-3 PLM alive through this handler pass");
    respawning2x2Plms.Step(bus, respawning2x2, respawning2x2Streamer, 0, 0, 0);
    AssertEqual(0, respawning2x2Plms.ActiveCount,
        "BTS-3 deletes on the handler pass after linked-block restoration");

    RoomLevelData permanent1x2 = CreateBombLevel(6, definitions);
    BackgroundTilemapStreamer permanent1x2Streamer =
        permanent1x2.CreateBackgroundStreamer();
    var permanent1x2Plms = new RoomPlmSystem();
    AssertTrue(permanent1x2Plms.TrySpawnCollisionBombBlock(
        permanent1x2, blockIndex, behavior: 6),
        "BTS-6 collision setup occupies a native PLM slot");
    permanent1x2Plms.Step(bus, permanent1x2, permanent1x2Streamer, 0, 0, 0);
    AssertEqual((ushort)0x0053, permanent1x2.GetCollisionBlock(3, 3).LevelWord,
        "vertical draw writes BTS-6 origin");
    AssertEqual((ushort)0x0053, permanent1x2.GetCollisionBlock(3, 4).LevelWord,
        "vertical draw advances its second word by one room row");
    AssertEqual((ushort)0, permanent1x2.GetCollisionBlock(4, 3).LevelWord,
        "vertical draw does not accidentally advance into the neighboring column");
    StepMany(permanent1x2Plms, bus, permanent1x2, permanent1x2Streamer, 12);
    AssertEqual((ushort)0x00ff, permanent1x2.GetCollisionBlock(3, 4).LevelWord,
        "permanent BTS-6 finishes on two vertical blank-air words");
    permanent1x2Plms.Step(bus, permanent1x2, permanent1x2Streamer, 0, 0, 0);
    AssertEqual(0, permanent1x2Plms.ActiveCount,
        "permanent BTS-6 deletes one frame after its timer-one blank draw");

    Console.WriteLine("  Movement PLMs: grapple and collision-bomb ROM timing, multi-block terrain, sound, VRAM, and respawn agree.");
}

static void WriteDefinitionWord(byte[] definitions, int block, int tile, ushort value)
{
    int offset = block * 8 + tile * 2;
    definitions[offset] = unchecked((byte)value);
    definitions[offset + 1] = unchecked((byte)(value >> 8));
}

static void VerifySamusPostureMovement()
{
    var bus = new TestAddressSpace();
    bus.WriteBytes(0x91b631, [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]); // $01
    bus.WriteBytes(0x91b639, [0x04, 0x00, 0xff, 0x07, 0x06, 0x00, 0x15, 0x00]); // $02
    bus.WriteBytes(0x91b641, [0x08, 0x00, 0x01, 0x00, 0x06, 0x00, 0x15, 0x00]); // $03
    bus.WriteBytes(0x91b649, [0x04, 0x00, 0x02, 0x09, 0x06, 0x00, 0x15, 0x00]); // $04
    bus.WriteBytes(0x91b651, [0x08, 0x00, 0x01, 0x01, 0x06, 0x00, 0x15, 0x00]); // $05
    bus.WriteBytes(0x91b659, [0x04, 0x00, 0x02, 0x08, 0x06, 0x00, 0x15, 0x00]); // $06
    bus.WriteBytes(0x91b661, [0x08, 0x00, 0x01, 0x03, 0x06, 0x00, 0x15, 0x00]); // $07
    bus.WriteBytes(0x91b669, [0x04, 0x00, 0x02, 0x06, 0x06, 0x00, 0x15, 0x00]); // $08
    bus.WriteBytes(0x91b761, [0x08, 0x05, 0x27, 0x02, 0x00, 0x00, 0x10, 0x00]); // $27
    bus.WriteBytes(0x91b769, [0x04, 0x05, 0x28, 0x07, 0x00, 0x00, 0x10, 0x00]); // $28
    bus.WriteBytes(0x91b7d1, [0x08, 0x0f, 0xff, 0x02, 0x00, 0x00, 0x10, 0x00]); // $35
    bus.WriteBytes(0x91b801, [0x08, 0x0f, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]); // $3B
    bus.WriteBytes(0x91b881, [0x08, 0x02, 0xff, 0x02, 0x03, 0x00, 0x13, 0x00]); // $4B
    bus.WriteBytes(0x91b889, [0x04, 0x02, 0xff, 0x07, 0x03, 0x00, 0x13, 0x00]); // $4C
    bus.WriteBytes(0x91b9b1, [0x08, 0x05, 0x27, 0x01, 0x00, 0x00, 0x10, 0x00]); // $71
    bus.WriteBytes(0x91b9b9, [0x04, 0x05, 0x28, 0x08, 0x00, 0x00, 0x10, 0x00]); // $72
    bus.WriteBytes(0x91b9c1, [0x08, 0x05, 0x27, 0x03, 0x00, 0x00, 0x10, 0x00]); // $73
    bus.WriteBytes(0x91b9c9, [0x04, 0x05, 0x28, 0x06, 0x00, 0x00, 0x10, 0x00]); // $74
    bus.WriteBytes(0x91ba51, [0x08, 0x05, 0x27, 0x00, 0x00, 0x00, 0x10, 0x00]); // $85
    bus.WriteBytes(0x91ba59, [0x04, 0x05, 0x28, 0x09, 0x00, 0x00, 0x10, 0x00]); // $86

    // These twelve literal records prove the aimed transition art retains command seven's
    // radius semantics: `$F1-$F6` already carry radius 16, while `$F7-$FC` carry radius 21.
    byte[] aimedCrouchTransitions = [0xf1, 0xf2, 0xf3, 0xf4, 0xf5, 0xf6];
    byte[] aimedStandTransitions = [0xf7, 0xf8, 0xf9, 0xfa, 0xfb, 0xfc];
    byte[] transitionDirections = [0x08, 0x04, 0x08, 0x04, 0x08, 0x04];
    byte[] transitionShots = [0x00, 0x09, 0x01, 0x08, 0x03, 0x06];
    for (int index = 0; index < 6; index++)
    {
        int crouchAddress = 0x91b629 + aimedCrouchTransitions[index] * 8;
        bus.WriteBytes(crouchAddress, [
            transitionDirections[index], 0x0f, 0xff, transitionShots[index],
            0x08, 0x00, 0x10, 0x00,
        ]);
        int standAddress = 0x91b629 + aimedStandTransitions[index] * 8;
        bus.WriteBytes(standAddress, [
            transitionDirections[index], 0x0f, 0xff, transitionShots[index],
            0x03, 0x00, 0x15, 0x00,
        ]);
    }
    WriteTestWord(bus, 0x91b012, 0xc100);
    WriteTestWord(bus, 0x91b05e, 0xc110);
    WriteTestWord(bus, 0x91b07a, 0xc120);
    WriteTestWord(bus, 0x91b086, 0xc130);
    bus.WriteBytes(0x91c100, [0x0a, 0x0a, 0x0a, 0x0a, 0xf6]);
    bus.WriteBytes(0x91c110, [0x0a, 0x0a, 0x0a, 0x0a, 0xf6]);
    bus.WriteBytes(0x91c120, [0x02, 0xfd, 0x27]);
    bus.WriteBytes(0x91c130, [0x02, 0xfd, 0x01]);
    WriteTestWord(bus, 0x91b010 + SamusState.NeutralJumpTransitionRightPose * 2, 0xc140);
    WriteTestWord(bus, 0x91b010 + SamusState.NeutralJumpTransitionLeftPose * 2, 0xc150);
    bus.WriteBytes(0x91c140, [0x01, 0xfd, SamusState.NeutralJumpRightPose]);
    bus.WriteBytes(0x91c150, [0x01, 0xfd, SamusState.NeutralJumpLeftPose]);

    // Dry-air table-zero values used by Make_Samus_Jump and normal-air gravity. Keeping
    // these as literal ROM words makes a crouch jump observable beyond merely changing pose.
    bus.WriteBytes(0x909eb9, [0x04, 0x00]);
    bus.WriteBytes(0x909ebf, [0x00, 0xe0]);
    bus.WriteBytes(0x909ea1, [0x00, 0x1c]);
    bus.WriteBytes(0x909ea7, [0x00, 0x00]);

    // Give every aimed transition its own command-$FD stream. Distinct stream pointers
    // catch accidental pose reuse; the literal target arrays mirror `$91:B518-$91:B53B`.
    byte[] aimedCrouchTargets = [0x85, 0x86, 0x71, 0x72, 0x73, 0x74];
    byte[] aimedStandTargets = [0x03, 0x04, 0x05, 0x06, 0x07, 0x08];
    for (int index = 0; index < 6; index++)
    {
        ushort crouchStream = unchecked((ushort)(0xc200 + index * 0x10));
        WriteTestWord(bus, 0x91b010 + aimedCrouchTransitions[index] * 2, crouchStream);
        bus.WriteBytes(0x910000 | crouchStream, [0x02, 0xfd, aimedCrouchTargets[index]]);
        ushort standStream = unchecked((ushort)(0xc260 + index * 0x10));
        WriteTestWord(bus, 0x91b010 + aimedStandTransitions[index] * 2, standStream);
        bus.WriteBytes(0x910000 | standStream, [0x02, 0xfd, aimedStandTargets[index]]);
    }
    byte[] stablePosturePoses = [
        0x02, 0x03, 0x04, 0x05, 0x06, 0x07, 0x08,
        0x28, 0x71, 0x72, 0x73, 0x74, 0x85, 0x86,
    ];
    for (int index = 0; index < stablePosturePoses.Length; index++)
    {
        ushort stream = unchecked((ushort)(0xc300 + index * 0x10));
        WriteTestWord(bus, 0x91b010 + stablePosturePoses[index] * 2, stream);
        bus.WriteBytes(0x910000 | stream, [0x10, 0xff]);
    }

    const int width = 8;
    const int height = 8;
    var floor = new ushort[width * height];
    for (int x = 0; x < width; x++)
        floor[4 * width + x] = 0x8000;
    var level = new RoomLevelData(
        width,
        height,
        floor,
        new byte[floor.Length],
        new ushort[floor.Length],
        new byte[8]);

    var samus = new SamusState
    {
        Pose = SamusState.FacingRightNormalPose,
        XPosition = 48,
        YPosition = 43, // standing bottom is pixel 63, immediately above row-four floor
    };
    samus.RefreshCollisionRadii(bus);
    samus.InitializeAnimation(bus);
    AssertTrue(
        samus.TryApplyPostureTransition(
            bus, level, SamusState.CrouchingTransitionRightPose, nmiFrameCounter: 0),
        "standing begins crouch transition");
    AssertEqual((ushort)16, samus.Kinematics.YRadius, "crouch transition radius");
    AssertEqual((ushort)48, samus.YPosition, "command seven moves crouch center down five");

    for (int tick = 0; tick < 2; tick++)
        samus.AnimateNoFx(bus);
    AssertEqual((byte)0xfd, samus.LastAnimationDelayCommand!.Value, "crouch transition reaches FD");
    AssertTrue(samus.ApplyPendingVerifiedAnimationTransition(bus), "crouch FD applies");
    AssertEqual((byte)0x27, samus.Pose, "crouch transition target");
    GroundedMovementResult crouchFrame = SamusPostureMovement.StepCrouching(
        bus, level, samus, nmiFrameCounter: 0);
    AssertTrue(crouchFrame.Vertical.Collided, "crouch performs grounded probe");

    AssertTrue(
        samus.TryApplyPostureTransition(
            bus, level, SamusState.StandingTransitionRightPose, nmiFrameCounter: 1),
        "crouch begins standing transition");
    AssertEqual((ushort)21, samus.Kinematics.YRadius, "standing transition radius");
    AssertEqual((ushort)43, samus.YPosition, "floor-constrained expansion moves center up five");
    for (int tick = 0; tick < 2; tick++)
        samus.AnimateNoFx(bus);
    AssertTrue(samus.ApplyPendingVerifiedAnimationTransition(bus), "standing FD applies");
    AssertEqual((byte)0x01, samus.Pose, "standing transition target");

    // Exercise all six facing/direction variants through both radius-changing halves. This
    // is intentionally a table test because the retail transition tables route every one
    // through the same two native collision commands but different `$FD` targets.
    for (int index = 0; index < 6; index++)
    {
        bool facesLeft = (index & 1) != 0;
        var aimedSamus = new SamusState
        {
            Pose = facesLeft ? SamusState.FacingLeftNormalPose : SamusState.FacingRightNormalPose,
            XPosition = 48,
            YPosition = 43,
        };
        aimedSamus.RefreshCollisionRadii(bus);
        aimedSamus.InitializeAnimation(bus);
        AssertTrue(
            aimedSamus.TryApplyPostureTransition(
                bus, level, aimedCrouchTransitions[index], unchecked((ushort)index)),
            $"aimed crouch transition ${aimedCrouchTransitions[index]:X2} begins");
        AssertEqual((ushort)16, aimedSamus.Kinematics.YRadius, "aimed crouch transition radius");
        AssertEqual((ushort)48, aimedSamus.YPosition, "aimed crouch keeps feet aligned");
        SamusPostureMovement.StepCrouchStandTransition(
            bus, level, aimedSamus, unchecked((ushort)index));
        aimedSamus.AnimateNoFx(bus);
        aimedSamus.AnimateNoFx(bus);
        AssertTrue(aimedSamus.ApplyPendingVerifiedAnimationTransition(bus), "aimed crouch FD applies");
        AssertEqual(aimedCrouchTargets[index], aimedSamus.Pose, "aimed crouch FD target");
        AssertEqual(
            facesLeft ? (byte)0x28 : (byte)0x27,
            aimedSamus.ReadNoInputFallbackPose(bus),
            "aimed crouch definition fallback");
        GroundedMovementResult aimedCrouchFrame = SamusPostureMovement.StepCrouching(
            bus, level, aimedSamus, unchecked((ushort)index));
        AssertTrue(aimedCrouchFrame.Vertical.Collided, "aimed crouch remains grounded");

        AssertTrue(
            aimedSamus.TryApplyPostureTransition(
                bus, level, aimedStandTransitions[index], unchecked((ushort)index)),
            $"aimed stand transition ${aimedStandTransitions[index]:X2} begins");
        AssertEqual((ushort)21, aimedSamus.Kinematics.YRadius, "aimed standing transition radius");
        AssertEqual((ushort)43, aimedSamus.YPosition, "aimed stand keeps feet aligned");
        aimedSamus.AnimateNoFx(bus);
        aimedSamus.AnimateNoFx(bus);
        AssertTrue(aimedSamus.ApplyPendingVerifiedAnimationTransition(bus), "aimed stand FD applies");
        AssertEqual(aimedStandTargets[index], aimedSamus.Pose, "aimed stand FD target");
    }

    // Equal-radius live shoulder changes never route through pose-change collision. Verify
    // both facing families and the definition-byte-two return to ordinary crouch.
    var crouchAim = new SamusState
    {
        Pose = SamusState.CrouchingRightPose,
        XPosition = 48,
        YPosition = 48,
    };
    crouchAim.RefreshCollisionRadii(bus);
    crouchAim.InitializeAnimation(bus);
    foreach (byte target in new byte[] {
        SamusState.CrouchingAimUpRightPose,
        SamusState.CrouchingAimDiagonalUpRightPose,
        SamusState.CrouchingAimDiagonalDownRightPose,
        SamusState.CrouchingRightPose,
    })
    {
        crouchAim.ApplyGroundedAimTransition(bus, target);
        AssertEqual((ushort)16, crouchAim.Kinematics.YRadius, $"right crouch aim ${target:X2} radius");
    }
    AssertThrows<NotSupportedException>(
        () => crouchAim.ApplyGroundedAimTransition(bus, SamusState.CrouchingAimUpLeftPose),
        "crouch aim cannot cross facing families");

    // Releasing Down while retaining the facing direction matches `$91:A6A0`'s direct
    // `$27 -> $01` record. This is not the `$3B` standing animation: radius expansion and
    // floor alignment happen immediately at the post-input pose-change seam.
    var directStand = new SamusState
    {
        Pose = SamusState.CrouchingRightPose,
        XPosition = 48,
        YPosition = 48,
    };
    directStand.RefreshCollisionRadii(bus);
    directStand.InitializeAnimation(bus);
    AssertTrue(
        directStand.TryApplyDirectCrouchToStandingTransition(
            bus, level, SamusState.FacingRightNormalPose, nmiFrameCounter: 0),
        "direct crouch-to-standing record applies");
    AssertEqual((byte)0x01, directStand.Pose, "direct crouch exit target");
    AssertEqual((ushort)21, directStand.Kinematics.YRadius, "direct crouch exit radius");
    AssertEqual((ushort)43, directStand.YPosition, "direct crouch exit keeps feet aligned");

    var directStandLeft = new SamusState
    {
        Pose = SamusState.CrouchingLeftPose,
        XPosition = 48,
        YPosition = 48,
    };
    directStandLeft.RefreshCollisionRadii(bus);
    directStandLeft.InitializeAnimation(bus);
    AssertTrue(
        directStandLeft.TryApplyDirectCrouchToStandingTransition(
            bus, level, SamusState.FacingLeftNormalPose, nmiFrameCounter: 1),
        "mirrored direct crouch-to-standing record applies");
    AssertEqual((byte)0x02, directStandLeft.Pose, "mirrored direct crouch exit target");
    AssertEqual((ushort)43, directStandLeft.YPosition, "mirrored direct crouch exit alignment");

    // `$91:FC66` first accepts the 16 -> 19 radius expansion, moving the center up three
    // against the floor, then subtracts ten more pixels only when PreviousPose is exactly
    // ordinary crouch `$27/$28`. Make_Samus_Jump follows with the ROM's 4.E000 velocity.
    var crouchJump = new SamusState
    {
        Pose = SamusState.CrouchingRightPose,
        XPosition = 48,
        YPosition = 48,
    };
    crouchJump.RefreshCollisionRadii(bus);
    crouchJump.InitializeAnimation(bus);
    AssertTrue(
        crouchJump.TryApplyCrouchJumpTransition(
            bus, level, SamusState.NeutralJumpTransitionRightPose, nmiFrameCounter: 0),
        "ordinary crouch jump applies");
    AssertEqual((byte)0x4b, crouchJump.Pose, "ordinary crouch jump transition pose");
    AssertEqual((ushort)19, crouchJump.Kinematics.YRadius, "ordinary crouch jump radius");
    AssertEqual((ushort)35, crouchJump.YPosition, "ordinary crouch jump collision plus FC8A offset");
    AssertEqual((ushort)4, crouchJump.Kinematics.YSpeed, "ordinary crouch jump Y speed");
    AssertEqual((ushort)0xe000, crouchJump.Kinematics.YSubspeed, "ordinary crouch jump Y subspeed");
    AssertEqual((ushort)1, crouchJump.Kinematics.YDirection, "ordinary crouch jump rises");

    var crouchJumpLeft = new SamusState
    {
        Pose = SamusState.CrouchingLeftPose,
        XPosition = 48,
        YPosition = 48,
    };
    crouchJumpLeft.RefreshCollisionRadii(bus);
    crouchJumpLeft.InitializeAnimation(bus);
    AssertTrue(
        crouchJumpLeft.TryApplyCrouchJumpTransition(
            bus, level, SamusState.NeutralJumpTransitionLeftPose, nmiFrameCounter: 1),
        "mirrored ordinary crouch jump applies");
    AssertEqual((byte)0x4c, crouchJumpLeft.Pose, "mirrored crouch jump transition pose");
    AssertEqual((ushort)35, crouchJumpLeft.YPosition, "mirrored crouch jump Y adjustment");

    // The native literal-pose comparison intentionally excludes aimed crouches. They use
    // the same `$4B` art and jump velocity but receive only the three-pixel floor-alignment
    // adjustment from pose-change collision.
    var aimedCrouchJump = new SamusState
    {
        Pose = SamusState.CrouchingAimDiagonalUpRightPose,
        XPosition = 48,
        YPosition = 48,
    };
    aimedCrouchJump.RefreshCollisionRadii(bus);
    aimedCrouchJump.InitializeAnimation(bus);
    AssertTrue(
        aimedCrouchJump.TryApplyCrouchJumpTransition(
            bus, level, SamusState.NeutralJumpTransitionRightPose, nmiFrameCounter: 1),
        "aimed crouch jump applies");
    AssertEqual((ushort)45, aimedCrouchJump.YPosition, "aimed crouch jump omits FC8A offset");

    // Ceiling row one ends at pixel 31. A crouched body occupies 32..63 exactly, while a
    // standing body would need 27..63. Both five-pixel probes collide, so native pose
    // collision rejects the larger pose and retains crouch.
    var tunnelBlocks = (ushort[])floor.Clone();
    for (int x = 0; x < width; x++)
        tunnelBlocks[1 * width + x] = 0x8000;
    var tunnel = new RoomLevelData(
        width,
        height,
        tunnelBlocks,
        new byte[tunnelBlocks.Length],
        new ushort[tunnelBlocks.Length],
        new byte[8]);
    var tunnelSamus = new SamusState
    {
        Pose = SamusState.CrouchingRightPose,
        XPosition = 48,
        YPosition = 48,
    };
    tunnelSamus.RefreshCollisionRadii(bus);
    tunnelSamus.InitializeAnimation(bus);
    AssertTrue(
        !tunnelSamus.TryApplyPostureTransition(
            bus, tunnel, SamusState.StandingTransitionRightPose, nmiFrameCounter: 0),
        "low tunnel rejects standing radius expansion");
    AssertEqual((byte)0x27, tunnelSamus.Pose, "rejected stand retains crouch pose");
    AssertEqual((ushort)48, tunnelSamus.YPosition, "rejected stand preserves center Y");

    var tunnelJump = new SamusState
    {
        Pose = SamusState.CrouchingAimUpRightPose,
        XPosition = 48,
        YPosition = 48,
    };
    tunnelJump.RefreshCollisionRadii(bus);
    tunnelJump.InitializeAnimation(bus);
    AssertTrue(
        !tunnelJump.TryApplyCrouchJumpTransition(
            bus, tunnel, SamusState.NeutralJumpTransitionRightPose, nmiFrameCounter: 1),
        "low tunnel rejects crouch-jump radius expansion");
    AssertEqual((byte)0x27, tunnelJump.Pose, "boxed aimed jump falls back to ordinary crouch");
    AssertEqual((ushort)16, tunnelJump.Kinematics.YRadius, "boxed aimed jump retains crouch radius");
    AssertEqual((ushort)0, tunnelJump.Kinematics.YSpeed, "boxed aimed jump does not call Make_Samus_Jump");

    Console.WriteLine("  Samus posture: animated/direct exits, crouch jumps, radii, movement, FD targets, and low-ceiling fallback agree.");
}

/// <summary>
/// Verifies all six stationary aim poses against their literal pose-definition bytes,
/// movement-type-zero grounding, same-facing transitions, and no-controller fallbacks.
/// </summary>
static void VerifySamusStandingAimMovement()
{
    var bus = new TestAddressSpace();

    // These eight records are copied from `$91:B631-$91:B669`. Byte two is especially
    // important: `$03/$05/$07 -> $01` and `$04/$06/$08 -> $02` when the entire controller
    // word becomes zero. Direction/shot metadata differs, while movement type and radius do not.
    bus.WriteBytes(0x91b631, [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b639, [0x04, 0x00, 0xff, 0x07, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b641, [0x08, 0x00, 0x01, 0x00, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b649, [0x04, 0x00, 0x02, 0x09, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b651, [0x08, 0x00, 0x01, 0x01, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b659, [0x04, 0x00, 0x02, 0x08, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b661, [0x08, 0x00, 0x01, 0x03, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b669, [0x04, 0x00, 0x02, 0x06, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b671, [0x08, 0x01, 0x01, 0x02, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b679, [0x04, 0x01, 0x02, 0x07, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b691, [0x08, 0x01, 0x01, 0x00, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b699, [0x04, 0x01, 0x02, 0x09, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b6a1, [0x08, 0x01, 0x01, 0x01, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b6a9, [0x04, 0x01, 0x02, 0x08, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b6b1, [0x08, 0x01, 0x01, 0x03, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b6b9, [0x04, 0x01, 0x02, 0x06, 0x06, 0x00, 0x15, 0x00]);

    // Running movement type one reads the normal-air record at `$90:9F61`.
    bus.WriteBytes(0x909f61, [
        0x00, 0x00, 0x00, 0x30,
        0x02, 0x00, 0x00, 0xc0,
        0x00, 0x00, 0x00, 0x80,
    ]);

    // Every transition must resolve and restart its own bank-$91 delay list. Distinct
    // pointers make an accidental reuse of the source pose observable even though the
    // synthetic frame-zero delay is deliberately identical.
    for (int pose = 1; pose <= 0x12; pose++)
    {
        ushort stream = unchecked((ushort)(0xc100 + pose * 0x10));
        WriteTestWord(bus, 0x91b010 + pose * 2, stream);
        bus.WriteBytes(0x910000 | stream, [0x0a, 0xf6]);
    }

    const int width = 8;
    const int height = 8;
    var blocks = new ushort[width * height];
    for (int x = 0; x < width; x++)
        blocks[4 * width + x] = 0x8000;
    var level = new RoomLevelData(
        width,
        height,
        blocks,
        new byte[blocks.Length],
        new ushort[blocks.Length],
        new byte[8]);

    var samus = new SamusState
    {
        Pose = SamusState.FacingRightNormalPose,
        XPosition = 48,
        YPosition = 43,
    };
    samus.RefreshCollisionRadii(bus);
    samus.InitializeAnimation(bus);

    byte[] rightAimRoute = [
        SamusState.StandingAimUpRightPose,
        SamusState.StandingAimDiagonalUpRightPose,
        SamusState.StandingAimDiagonalDownRightPose,
        SamusState.FacingRightNormalPose,
    ];
    foreach (byte target in rightAimRoute)
    {
        samus.HorizontalSpeed.BaseSpeed = 3;
        samus.ApplyGroundedAimTransition(bus, target);
        GroundedMovementResult result = SamusGroundedMovement.StepStandingRight(
            bus, level, samus, nmiFrameCounter: 0);
        AssertTrue(result.Vertical.Collided, $"right aim pose ${target:X2} remains grounded");
        AssertEqual((ushort)0, samus.HorizontalSpeed.BaseSpeed, $"right aim pose ${target:X2} clears base speed");
    }
    samus.ApplyGroundedAimTransition(bus, SamusState.StandingAimUpRightPose);
    AssertEqual((byte)0x01, samus.ReadNoInputFallbackPose(bus), "right aimed no-input fallback");

    // The aimed running records execute the same movement-type-one dispatcher while their
    // own animation and shot-direction metadata remain selected. Cross the post-movement
    // transition seam in both directions to prove speed survives running-to-running aim
    // changes and standing clears it only on the following movement frame.
    samus.ApplyGroundedAimTransition(bus, SamusState.RunningAimDiagonalUpRightPose);
    ushort rightXBefore = samus.XPosition;
    for (ushort frame = 0; frame < 4; frame++)
        SamusGroundedMovement.StepRunningRight(bus, level, samus, frame);
    AssertTrue(samus.XPosition > rightXBefore, "right aimed run moves right");
    uint rightRunSpeed = samus.HorizontalSpeed.BaseFixed;
    samus.ApplyGroundedAimTransition(bus, SamusState.RunningAimDiagonalDownRightPose);
    AssertEqual(rightRunSpeed, samus.HorizontalSpeed.BaseFixed, "right running aim change preserves speed");
    samus.ApplyGroundedAimTransition(bus, SamusState.RunningAimUpRightPose);
    SamusGroundedMovement.StepRunningRight(bus, level, samus, nmiFrameCounter: 1);
    samus.ApplyGroundedAimTransition(bus, SamusState.StandingAimUpRightPose);
    SamusGroundedMovement.StepStandingRight(bus, level, samus, nmiFrameCounter: 0);
    AssertEqual(0u, samus.HorizontalSpeed.BaseFixed, "right aimed running-to-standing cleanup");

    // Install left-facing normal through a fresh state so the transition helper never
    // crosses families; native left records are an independent mirrored family.
    var leftSamus = new SamusState
    {
        Pose = SamusState.FacingLeftNormalPose,
        XPosition = 48,
        YPosition = 43,
    };
    leftSamus.RefreshCollisionRadii(bus);
    leftSamus.InitializeAnimation(bus);
    byte[] leftAimRoute = [
        SamusState.StandingAimUpLeftPose,
        SamusState.StandingAimDiagonalUpLeftPose,
        SamusState.StandingAimDiagonalDownLeftPose,
        SamusState.FacingLeftNormalPose,
    ];
    foreach (byte target in leftAimRoute)
    {
        leftSamus.ApplyGroundedAimTransition(bus, target);
        GroundedMovementResult result = SamusGroundedMovement.StepStandingLeft(
            bus, level, leftSamus, nmiFrameCounter: 1);
        AssertTrue(result.Vertical.Collided, $"left aim pose ${target:X2} remains grounded");
    }
    leftSamus.ApplyGroundedAimTransition(bus, SamusState.StandingAimUpLeftPose);
    AssertEqual((byte)0x02, leftSamus.ReadNoInputFallbackPose(bus), "left aimed no-input fallback");

    leftSamus.ApplyGroundedAimTransition(bus, SamusState.RunningAimDiagonalUpLeftPose);
    ushort leftXBefore = leftSamus.XPosition;
    for (ushort frame = 0; frame < 4; frame++)
        SamusGroundedMovement.StepRunningLeft(bus, level, leftSamus, frame);
    AssertTrue(leftSamus.XPosition < leftXBefore, "left aimed run moves left");
    uint leftRunSpeed = leftSamus.HorizontalSpeed.BaseFixed;
    leftSamus.ApplyGroundedAimTransition(bus, SamusState.RunningAimDiagonalDownLeftPose);
    AssertEqual(leftRunSpeed, leftSamus.HorizontalSpeed.BaseFixed, "left running aim change preserves speed");
    leftSamus.ApplyGroundedAimTransition(bus, SamusState.RunningAimUpLeftPose);
    SamusGroundedMovement.StepRunningLeft(bus, level, leftSamus, nmiFrameCounter: 0);
    leftSamus.ApplyGroundedAimTransition(bus, SamusState.StandingAimUpLeftPose);
    SamusGroundedMovement.StepStandingLeft(bus, level, leftSamus, nmiFrameCounter: 1);
    AssertEqual(0u, leftSamus.HorizontalSpeed.BaseFixed, "left aimed running-to-standing cleanup");

    AssertThrows<NotSupportedException>(
        () => leftSamus.ApplyGroundedAimTransition(
            bus, SamusState.StandingAimUpRightPose),
        "standing aim transition cannot cross facing families");

    Console.WriteLine("  Samus grounded aim: stationary/running ROM poses, movement, transitions, and fallbacks agree.");
}

/// <summary>
/// Verifies the equal-radius aimed jump/fall family, command-$FD launch, shot-direction
/// landing table, command-$F8 completion, walk-off selection, and velocity preservation.
/// </summary>
static void VerifySamusAimedAerialMovement()
{
    var bus = new TestAddressSpace();

    // Literal pose records used by this route. Ordinary aimed airborne bodies have radius
    // 19; straight-down `$17/$2D` use radius 10, and landing expands to radius 21.
    bus.WriteBytes(0x91b631, [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]); // $01
    bus.WriteBytes(0x91b641, [0x08, 0x00, 0x01, 0x00, 0x06, 0x00, 0x15, 0x00]); // $03
    bus.WriteBytes(0x91b651, [0x08, 0x00, 0x01, 0x01, 0x06, 0x00, 0x15, 0x00]); // $05
    bus.WriteBytes(0x91b661, [0x08, 0x00, 0x01, 0x03, 0x06, 0x00, 0x15, 0x00]); // $07
    bus.WriteBytes(0x91b6d1, [0x08, 0x02, 0x51, 0x00, 0x08, 0x00, 0x13, 0x00]); // $15
    bus.WriteBytes(0x91b6e1, [0x08, 0x02, 0xff, 0x04, 0x06, 0x00, 0x0a, 0x00]); // $17
    bus.WriteBytes(0x91b6e9, [0x04, 0x02, 0xff, 0x05, 0x06, 0x00, 0x0a, 0x00]); // $18
    bus.WriteBytes(0x91b761, [0x08, 0x05, 0x27, 0x02, 0x00, 0x00, 0x10, 0x00]); // $27
    bus.WriteBytes(0x91b771, [0x08, 0x06, 0xff, 0x02, 0x08, 0x00, 0x13, 0x00]); // $29
    bus.WriteBytes(0x91b781, [0x08, 0x06, 0x29, 0x00, 0x08, 0x00, 0x13, 0x00]); // $2B
    bus.WriteBytes(0x91b791, [0x08, 0x06, 0xff, 0x04, 0x06, 0x00, 0x0a, 0x00]); // $2D
    bus.WriteBytes(0x91b799, [0x04, 0x06, 0xff, 0x05, 0x06, 0x00, 0x0a, 0x00]); // $2E
    bus.WriteBytes(0x91b8b1, [0x08, 0x02, 0xff, 0x02, 0x08, 0x00, 0x13, 0x00]); // $51
    bus.WriteBytes(0x91b8e1, [0x08, 0x02, 0xff, 0x01, 0x03, 0x00, 0x13, 0x00]); // $57
    bus.WriteBytes(0x91b971, [0x08, 0x02, 0x51, 0x01, 0x08, 0x00, 0x13, 0x00]); // $69
    bus.WriteBytes(0x91b981, [0x08, 0x02, 0x51, 0x03, 0x08, 0x00, 0x13, 0x00]); // $6B
    bus.WriteBytes(0x91b989, [0x04, 0x02, 0x52, 0x06, 0x08, 0x00, 0x13, 0x00]); // $6C
    bus.WriteBytes(0x91b991, [0x08, 0x06, 0x29, 0x01, 0x08, 0x00, 0x13, 0x00]); // $6D
    bus.WriteBytes(0x91b9a1, [0x08, 0x06, 0x29, 0x03, 0x08, 0x00, 0x13, 0x00]); // $6F
    bus.WriteBytes(0x91bb49, [0x08, 0x00, 0xff, 0x02, 0x03, 0x00, 0x15, 0x00]); // $A4
    bus.WriteBytes(0x91bb51, [0x04, 0x00, 0xff, 0x07, 0x03, 0x00, 0x15, 0x00]); // $A5
    bus.WriteBytes(0x91bd39, [0x08, 0x00, 0xff, 0x01, 0x03, 0x00, 0x15, 0x00]); // $E2
    bus.WriteBytes(0x91bd49, [0x08, 0x00, 0xff, 0x03, 0x03, 0x00, 0x15, 0x00]); // $E4

    // Synthetic delay streams expose both command-three seams independently. `$6D` uses
    // the retail `$02,$F0,$10,$FE,$01` sequence verbatim so the active animation-command
    // no-op is checked in the movement family that actually consumes it.
    (byte Pose, ushort Stream, byte[] Bytes)[] animations = [
        (0x01, 0xc100, [0x0a, 0xf6]),
        (0x03, 0xc110, [0x0a, 0xf6]),
        (0x05, 0xc120, [0x0a, 0xf6]),
        (0x07, 0xc130, [0x0a, 0xf6]),
        (0x15, 0xc140, [0x02, 0xff]),
        (0x17, 0xc148, [0x02, 0xff]),
        (0x18, 0xc14c, [0x02, 0xff]),
        (0x27, 0xc14e, [0x10, 0xff]),
        (0x29, 0xc150, [0x02, 0xff]),
        (0x2b, 0xc160, [0x02, 0xff]),
        (0x2d, 0xc168, [0x02, 0xff]),
        (0x2e, 0xc16c, [0x02, 0xff]),
        (0x51, 0xc170, [0x02, 0xff]),
        (0x57, 0xc180, [0x01, 0xfd, 0x69]),
        (0x69, 0xc190, [0x02, 0xff]),
        (0x6b, 0xc1a0, [0x02, 0xff]),
        (0x6c, 0xc1a8, [0x02, 0xff]),
        (0x6d, 0xc1b0, [0x02, 0xf0, 0x10, 0xfe, 0x01]),
        (0x6f, 0xc1c0, [0x02, 0xff]),
        (0xe2, 0xc1d0, [0x01, 0xf8, 0x05]),
        (0xe4, 0xc1e0, [0x01, 0xf8, 0x07]),
        (0xa4, 0xc1f0, [0x02, 0xf8, 0x01]),
        (0xa5, 0xc1f8, [0x02, 0xf8, 0x02]),
    ];
    foreach ((byte pose, ushort stream, byte[] bytes) in animations)
    {
        WriteTestWord(bus, 0x91b010 + pose * 2, stream);
        bus.WriteBytes(0x910000 | stream, bytes);
    }

    bus.WriteBytes(0x909eb9, [0x04, 0x00]);
    bus.WriteBytes(0x909ebf, [0x00, 0xe0]);
    bus.WriteBytes(0x909ea1, [0x00, 0x1c]);
    bus.WriteBytes(0x909ea7, [0x00, 0x00]);
    bus.WriteBytes(0x909f6d, [
        0x00, 0x00, 0x00, 0x30,
        0x02, 0x00, 0x00, 0xc0,
        0x00, 0x00, 0x00, 0x80,
    ]);
    bus.WriteBytes(0x909f9d, [
        0x00, 0x00, 0x00, 0x30,
        0x02, 0x00, 0x00, 0xc0,
        0x00, 0x00, 0x00, 0x80,
    ]);

    const int width = 8;
    const int height = 20;
    var blocks = new ushort[width * height];
    for (int x = 0; x < width; x++)
        blocks[12 * width + x] = 0x8000;
    var level = new RoomLevelData(
        width,
        height,
        blocks,
        new byte[blocks.Length],
        new ushort[blocks.Length],
        // Visual block definitions are ROM-native eight-byte records. The collision
        // test does not render them, but the room container deliberately enforces
        // that physical format even for synthetic fixtures.
        new byte[24]);

    var samus = new SamusState
    {
        Pose = SamusState.StandingAimDiagonalUpRightPose,
        XPosition = 48,
        YPosition = 171,
    };
    samus.RefreshCollisionRadii(bus);
    samus.InitializeAnimation(bus);
    samus.ApplyOrdinaryJumpTransition(
        bus, SamusState.NormalJumpTransitionAimDiagonalUpRightPose);
    AssertEqual((ushort)19, samus.Kinematics.YRadius, "aimed jump transition radius");
    ushort launchY = samus.YPosition;
    AerialMovementResult transitionFrame = SamusAerialMovement.StepNormalJump(
        bus, level, samus, (ushort)SnesButton.A, nmiFrameCounter: 0);
    AssertEqual(launchY, samus.YPosition, "aimed transition frame does not move vertically");
    AssertTrue(transitionFrame.Vertical is null, "aimed transition omits normal vertical pass");

    samus.AnimateNoFx(bus);
    AssertEqual((byte)0xfd, samus.LastAnimationDelayCommand!.Value, "aimed transition reaches FD");
    AssertTrue(samus.ApplyPendingVerifiedAnimationTransition(bus), "aimed transition FD applies");
    AssertEqual((byte)0x69, samus.Pose, "aimed transition FD target");

    ushort speedBeforeAimChange = samus.Kinematics.YSpeed;
    samus.ApplyAerialAimTransition(bus, SamusState.NormalJumpAimDiagonalDownRightPose);
    AssertEqual(speedBeforeAimChange, samus.Kinematics.YSpeed, "air aim change preserves Y speed");

    // `$17` is not merely alternate art: Down shrinks the live body from radius 19 to
    // radius 10. Shrinking never probes blocks and does not move the center. Releasing to
    // another aimed-jump pose expands through `$91:FDAE`; these few upward frames put the
    // body in open air so neither the upper nor lower probe needs to displace the center.
    for (ushort index = 0; index < 4; index++)
        SamusAerialMovement.StepNormalJump(bus, level, samus, (ushort)SnesButton.A, index);
    ushort compactCenterY = samus.YPosition;
    uint compactVelocity = samus.Kinematics.VerticalSpeedFixed;
    ushort compactDirection = samus.Kinematics.YDirection;
    AssertThrows<NotSupportedException>(
        () => samus.ApplyAerialAimTransition(bus, SamusState.NormalJumpAimDownRightPose),
        "compact aim requires room-aware collision route");
    AssertTrue(
        samus.TryApplyCompactAerialTransition(
            bus, level, SamusState.NormalJumpAimDownRightPose, nmiFrameCounter: 4),
        "jump enters compact down-right pose");
    AssertEqual((byte)0x17, samus.Pose, "compact jump pose");
    AssertEqual((ushort)10, samus.Kinematics.YRadius, "compact jump radius");
    AssertEqual(compactCenterY, samus.YPosition, "shrinking compact jump preserves center");
    AssertEqual(compactVelocity, samus.Kinematics.VerticalSpeedFixed, "compact entry preserves 16.16 velocity");
    AssertEqual(compactDirection, samus.Kinematics.YDirection, "compact entry preserves vertical direction");
    AssertTrue(
        samus.TryApplyCompactAerialTransition(
            bus, level, SamusState.NormalJumpAimDiagonalDownRightPose, nmiFrameCounter: 5),
        "jump exits compact down-right pose");
    AssertEqual((ushort)19, samus.Kinematics.YRadius, "compact jump exit radius");
    AssertEqual(compactCenterY, samus.YPosition, "open-air compact expansion preserves center");
    AssertEqual(compactVelocity, samus.Kinematics.VerticalSpeedFixed, "compact exit preserves 16.16 velocity");

    AerialMovementResult frame = default;
    for (int index = 0; index < 160; index++)
    {
        frame = SamusAerialMovement.StepNormalJump(
            bus, level, samus, (ushort)SnesButton.A, unchecked((ushort)index));
        if (frame.Landed)
            break;
    }
    AssertTrue(frame.Landed, "aimed normal jump lands");
    samus.ApplyAerialLanding(bus, wasSpinning: false);
    AssertEqual((byte)0xe4, samus.Pose, "shot direction three selects E4 landing");
    AssertEqual((ushort)21, samus.Kinematics.YRadius, "aimed landing expands radius");
    AssertEqual((ushort)171, samus.YPosition, "aimed landing retains floor-aligned feet");
    samus.AnimateNoFx(bus);
    AssertTrue(samus.ApplyPendingVerifiedAnimationTransition(bus), "aimed landing F8 applies");
    AssertEqual((byte)0x07, samus.Pose, "E4 landing returns to down-right aim");

    // Falling `$2D` uses the movement-type-six dispatcher with the same radius-ten body.
    // Let collision align its bottom to the floor first, then apply shot-direction four's
    // `$A4` landing entry. The 11-pixel upward center correction keeps that exact boundary
    // fixed while collision command five clears all vertical and horizontal motion.
    var compactFall = new SamusState
    {
        Pose = SamusState.FallingAimDownRightPose,
        XPosition = 48,
        YPosition = 170,
    };
    compactFall.RefreshCollisionRadii(bus);
    compactFall.InitializeAnimation(bus);
    compactFall.Kinematics.YSpeed = 1;
    compactFall.Kinematics.YSubspeed = 0x4000;
    compactFall.Kinematics.YDirection = 2;
    SamusAerialMovement.ConfigureDryAirGravity(bus, compactFall);
    compactFall.HorizontalSpeed.BaseSpeed = 1;
    compactFall.HorizontalSpeed.BaseSubspeed = 0x8000;
    AerialMovementResult compactFallFrame = default;
    for (ushort index = 0; index < 40 && !compactFallFrame.Landed; index++)
        compactFallFrame = SamusAerialMovement.StepFalling(bus, level, compactFall, 0, index);
    AssertTrue(compactFallFrame.Landed, "compact falling pose reaches floor");
    ushort compactBottom = unchecked((ushort)(compactFall.YPosition + compactFall.Kinematics.YRadius));
    AssertTrue(
        compactFall.TryApplyCompactAerialLanding(bus, level, nmiFrameCounter: 0),
        "compact down-right landing expands successfully");
    AssertEqual((byte)0xa4, compactFall.Pose, "shot direction four selects A4 landing");
    AssertEqual((ushort)21, compactFall.Kinematics.YRadius, "compact landing radius");
    AssertEqual(
        compactBottom,
        unchecked((ushort)(compactFall.YPosition + compactFall.Kinematics.YRadius)),
        "compact landing preserves floor boundary");
    AssertEqual(0u, compactFall.Kinematics.VerticalSpeedFixed, "compact landing clears vertical speed");
    AssertEqual((ushort)0, compactFall.Kinematics.YDirection, "compact landing clears vertical direction");
    AssertEqual(0u, compactFall.HorizontalSpeed.BaseFixed, "compact landing clears horizontal speed");

    // The mirrored definitions carry direction four and shot direction five. Exercise the
    // same family guard in open air so a future right-only shortcut cannot silently pass.
    var compactLeft = new SamusState
    {
        Pose = SamusState.NormalJumpAimDownLeftPose,
        XPosition = 48,
        YPosition = 100,
    };
    compactLeft.RefreshCollisionRadii(bus);
    compactLeft.InitializeAnimation(bus);
    AssertTrue(
        compactLeft.TryApplyCompactAerialTransition(
            bus, level, SamusState.NormalJumpAimDiagonalDownLeftPose, nmiFrameCounter: 1),
        "mirrored compact jump expands");
    AssertEqual((byte)0x6c, compactLeft.Pose, "mirrored compact jump target");
    AssertEqual((ushort)19, compactLeft.Kinematics.YRadius, "mirrored compact jump radius");

    var compactLeftLanding = new SamusState
    {
        Pose = SamusState.FallingAimDownLeftPose,
        XPosition = 48,
        YPosition = 170,
    };
    compactLeftLanding.RefreshCollisionRadii(bus);
    compactLeftLanding.InitializeAnimation(bus);
    compactLeftLanding.Kinematics.YSpeed = 1;
    compactLeftLanding.Kinematics.YSubspeed = 0x4000;
    compactLeftLanding.Kinematics.YDirection = 2;
    SamusAerialMovement.ConfigureDryAirGravity(bus, compactLeftLanding);
    AerialMovementResult compactLeftFallFrame = default;
    for (ushort index = 0; index < 40 && !compactLeftFallFrame.Landed; index++)
    {
        compactLeftFallFrame = SamusAerialMovement.StepFalling(
            bus, level, compactLeftLanding, 0, index);
    }
    AssertTrue(compactLeftFallFrame.Landed, "mirrored compact falling pose reaches floor");
    ushort compactLeftBottom = unchecked((ushort)(
        compactLeftLanding.YPosition + compactLeftLanding.Kinematics.YRadius));
    AssertTrue(
        compactLeftLanding.TryApplyCompactAerialLanding(bus, level, nmiFrameCounter: 1),
        "compact down-left landing expands successfully");
    AssertEqual((byte)0xa5, compactLeftLanding.Pose, "shot direction five selects A5 landing");
    AssertEqual(
        compactLeftBottom,
        unchecked((ushort)(compactLeftLanding.YPosition + compactLeftLanding.Kinematics.YRadius)),
        "mirrored compact landing preserves floor boundary");

    // Put radius-ten `$17` between a row-nine ceiling and row-twelve floor. The body itself
    // fits, but both independent nine-pixel probes collide. `$91:FFA7` therefore selects
    // ordinary `$27` and `$91:FFD4-$91:FFE9` moves the center up by 10-16 = -6 pixels.
    var boxedBlocks = (ushort[])blocks.Clone();
    for (int x = 0; x < width; x++)
        boxedBlocks[9 * width + x] = 0x8000;
    var boxedLevel = new RoomLevelData(
        width,
        height,
        boxedBlocks,
        new byte[boxedBlocks.Length],
        new ushort[boxedBlocks.Length],
        new byte[24]);
    var boxedCompact = new SamusState
    {
        Pose = SamusState.NormalJumpAimDownRightPose,
        XPosition = 48,
        YPosition = 176,
    };
    boxedCompact.RefreshCollisionRadii(bus);
    boxedCompact.InitializeAnimation(bus);
    AssertTrue(
        !boxedCompact.TryApplyCompactAerialTransition(
            bus, boxedLevel, SamusState.NormalJumpAimDiagonalDownRightPose, nmiFrameCounter: 0),
        "boxed compact expansion is rejected");
    AssertEqual((byte)0x27, boxedCompact.Pose, "boxed compact expansion selects crouch");
    AssertEqual((ushort)16, boxedCompact.Kinematics.YRadius, "boxed compact fallback radius");
    AssertEqual((ushort)170, boxedCompact.YPosition, "boxed compact fallback applies FFD4 offset");

    // Grounded shot direction one walks off into `$6D`; falling aim changes preserve
    // gravity state and zero-input fallback selects ordinary `$29`.
    samus.Pose = SamusState.StandingAimDiagonalUpRightPose;
    samus.RefreshCollisionRadii(bus);
    samus.InitializeAnimation(bus);
    AssertEqual((byte)0x6d, samus.SelectFallingPoseForCurrentAim(bus), "up-right walk-off target");
    samus.ApplyWalkedOffFloorTransition(bus, SamusState.FallingAimDiagonalUpRightPose);
    AssertEqual((ushort)2, samus.Kinematics.YDirection, "aimed walk-off starts downward");

    // `$90:8324-$8345` leaves both the command index and zero timer untouched when `$F0`
    // is reached. One frame later DEC wraps zero to `$FFFF`; the negative result advances
    // over `$F0` and loads the literal `$10` delay. This seemingly odd two-frame sequence
    // is why treating an unknown command as a normal delay—or simply skipping it—drifts.
    samus.AnimateNoFx(bus);
    AssertEqual((ushort)1, samus.AnimationFrameTimer, "aimed-fall initial delay counts down");
    samus.AnimateNoFx(bus);
    AssertEqual((ushort)1, samus.AnimationFrame, "aimed-fall reaches F0 command index");
    AssertEqual((ushort)0, samus.AnimationFrameTimer, "F0 leaves expired timer at zero");
    AssertEqual((byte)0xf0, samus.LastAnimationDelayCommand!.Value, "aimed-fall records F0 command");
    samus.AnimateNoFx(bus);
    AssertEqual((ushort)2, samus.AnimationFrame, "timer underflow advances beyond F0");
    AssertEqual((ushort)16, samus.AnimationFrameTimer, "post-F0 frame loads literal delay");

    samus.ApplyAerialAimTransition(bus, SamusState.FallingAimDiagonalDownRightPose);
    AssertEqual((byte)0x29, samus.ReadNoInputFallbackPose(bus), "aimed fall fallback target");
    samus.ApplyAerialAimTransition(bus, SamusState.FallingRightPose);
    AssertEqual((byte)0x29, samus.Pose, "aimed fall applies unaimed fallback");

    Console.WriteLine("  Samus aimed air: FD jump, live aim, F0 cadence, compact hitboxes/landing, walk-off, and fall fallback agree.");
}

/// <summary>
/// Locks the complete ordinary-play horizontal-fire body family to literal bank-$91 pose
/// records and transition semantics. These checks intentionally separate body-pose work
/// from projectile ownership: a fired beam is a bank-$90 system, while `$0B/$13/$67` and
/// their mirrors are bank-$91 movement/animation states selected by the same controller word.
/// </summary>
static void VerifySamusGunExtendedMovement()
{
    var bus = new TestAddressSpace();

    // PoseDefinitions begins at `$91:B629` and each entry is exactly eight bytes:
    // X direction, movement type, no-input fallback, shot direction, collision command,
    // unused byte, Y radius, unused byte. These are direct retail bytes, not host fixtures
    // inferred from nearby poses.
    (byte Pose, byte[] Definition)[] definitions = [
        (0x01, [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]),
        (0x02, [0x04, 0x00, 0xff, 0x07, 0x06, 0x00, 0x15, 0x00]),
        (0x09, [0x08, 0x01, 0x01, 0x02, 0x06, 0x00, 0x15, 0x00]),
        (0x0a, [0x04, 0x01, 0x02, 0x07, 0x06, 0x00, 0x15, 0x00]),
        (0x0b, [0x08, 0x01, 0x01, 0x02, 0x06, 0x00, 0x15, 0x00]),
        (0x0c, [0x04, 0x01, 0x02, 0x07, 0x06, 0x00, 0x15, 0x00]),
        (0x13, [0x08, 0x02, 0xff, 0x02, 0x08, 0x00, 0x13, 0x00]),
        (0x14, [0x04, 0x02, 0xff, 0x07, 0x08, 0x00, 0x13, 0x00]),
        (0x4d, [0x08, 0x02, 0xff, 0x02, 0x08, 0x00, 0x13, 0x00]),
        (0x4e, [0x04, 0x02, 0xff, 0x07, 0x08, 0x00, 0x13, 0x00]),
        (0x29, [0x08, 0x06, 0xff, 0x02, 0x08, 0x00, 0x13, 0x00]),
        (0x2a, [0x04, 0x06, 0xff, 0x07, 0x08, 0x00, 0x13, 0x00]),
        (0x67, [0x08, 0x06, 0xff, 0x02, 0x08, 0x00, 0x13, 0x00]),
        (0x68, [0x04, 0x06, 0xff, 0x07, 0x08, 0x00, 0x13, 0x00]),
        (0xa4, [0x08, 0x00, 0xff, 0x02, 0x03, 0x00, 0x15, 0x00]),
        (0xa5, [0x04, 0x00, 0xff, 0x07, 0x03, 0x00, 0x15, 0x00]),
        (0xe6, [0x08, 0x00, 0xff, 0x02, 0x03, 0x00, 0x15, 0x00]),
        (0xe7, [0x04, 0x00, 0xff, 0x07, 0x03, 0x00, 0x15, 0x00]),
    ];
    foreach ((byte pose, byte[] definition) in definitions)
        bus.WriteBytes(0x91b629 + pose * 8, definition);

    // Synthetic storage keeps the tests compact, but each stream's bytes reproduce the
    // relevant retail control flow: running loops ten frames; jump/fall loop; firing
    // landings reach literal command `$F8,$01/$02`.
    (byte Pose, ushort Stream, byte[] Bytes)[] animations = [
        (0x01, 0xd000, [0x0a, 0xff]),
        (0x02, 0xd010, [0x0a, 0xff]),
        (0x09, 0xd020, [0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0xff]),
        (0x0a, 0xd020, [0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0xff]),
        (0x0b, 0xd020, [0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0xff]),
        (0x0c, 0xd020, [0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0xff]),
        (0x13, 0xd040, [0x02, 0x10, 0xfe, 0x01]),
        (0x14, 0xd040, [0x02, 0x10, 0xfe, 0x01]),
        (0x4d, 0xd050, [0x02, 0x03, 0xfe, 0x01]),
        (0x4e, 0xd050, [0x02, 0x03, 0xfe, 0x01]),
        (0x29, 0xd060, [0x08, 0x06, 0x06, 0xfe, 0x01, 0x08, 0x10, 0xfe, 0x01]),
        (0x2a, 0xd060, [0x08, 0x06, 0x06, 0xfe, 0x01, 0x08, 0x10, 0xfe, 0x01]),
        (0x67, 0xd080, [0x08, 0x06, 0x06, 0xfe, 0x01, 0x08, 0x10, 0xfe, 0x01]),
        (0x68, 0xd080, [0x08, 0x06, 0x06, 0xfe, 0x01, 0x08, 0x10, 0xfe, 0x01]),
        (0xa4, 0xd0a0, [0x05, 0x02, 0xf8, 0x01]),
        (0xa5, 0xd0b0, [0x05, 0x02, 0xf8, 0x02]),
        (0xe6, 0xd0c0, [0x01, 0xf8, 0x01]),
        (0xe7, 0xd0d0, [0x01, 0xf8, 0x02]),
    ];
    foreach ((byte pose, ushort stream, byte[] bytes) in animations)
    {
        WriteTestWord(bus, 0x91b010 + pose * 2, stream);
        bus.WriteBytes(0x910000 | stream, bytes);
    }

    // Minimal transition programs retain the literal held masks from `$91:A1F8`,
    // `$91:A2F6`, and `$91:A70C`. They prove that Shot+forward, Shot in neutral jump,
    // and Shot while falling select the three distinct extended-gun movement families.
    WriteTestWord(bus, 0x919ef4, 0xd100); // pose $09 pointer
    bus.WriteBytes(0x91d100, [0x00, 0x00, 0x40, 0x01, 0x0b, 0x00, 0xff, 0xff]);
    WriteTestWord(bus, 0x919f7c, 0xd110); // pose $4D pointer
    bus.WriteBytes(0x91d110, [0x00, 0x00, 0x40, 0x00, 0x13, 0x00, 0xff, 0xff]);
    WriteTestWord(bus, 0x919f34, 0xd120); // pose $29 pointer
    bus.WriteBytes(0x91d120, [0x00, 0x00, 0x40, 0x00, 0x67, 0x00, 0xff, 0xff]);

    AssertEqual((ushort)0x0b,
        SamusPoseTransitionTable.Find(bus, 0x09, 0x0140, 0)!.Value.ProspectivePose,
        "Shot+Right selects running gun extension");
    AssertEqual((ushort)0x13,
        SamusPoseTransitionTable.Find(bus, 0x4d, 0x0040, 0)!.Value.ProspectivePose,
        "Shot selects neutral-jump gun extension");
    AssertEqual((ushort)0x67,
        SamusPoseTransitionTable.Find(bus, 0x29, 0x0040, 0)!.Value.ProspectivePose,
        "Shot selects falling gun extension");

    // `$91:F50C` preserves the animation phase across movement-type-one arm changes. Begin
    // on a nonzero index so resetting to frame zero cannot accidentally satisfy the check.
    var running = new SamusState { Pose = SamusState.MovingRightNormalPose };
    running.RefreshCollisionRadii(bus);
    running.InitializeAnimation(bus, initialFrame: 4);
    ushort runningFrame = running.AnimationFrame;
    ushort runningTimer = running.AnimationFrameTimer;
    int runningDelayList = running.AnimationDelayListAddress;
    running.ApplyGroundedAimTransition(bus, SamusState.MovingRightGunExtendedPose);
    AssertEqual((byte)0x0b, running.Pose, "running gun extension installs pose $0B");
    AssertEqual(runningFrame, running.AnimationFrame, "running gun extension preserves frame");
    AssertEqual(runningTimer, running.AnimationFrameTimer, "running gun extension preserves timer");
    AssertEqual(runningDelayList, running.AnimationDelayListAddress, "running gun extension retains shared delay list");

    // Same-radius airborne arm changes must not perturb the live 16.16 velocity or direction.
    var jumping = new SamusState { Pose = SamusState.NeutralJumpRightPose };
    jumping.RefreshCollisionRadii(bus);
    jumping.InitializeAnimation(bus);
    jumping.Kinematics.YSpeed = 3;
    jumping.Kinematics.YSubspeed = 0x4567;
    jumping.Kinematics.YDirection = 1;
    jumping.ApplyAerialAimTransition(bus, SamusState.NormalJumpGunExtendedRightPose);
    AssertEqual((byte)0x13, jumping.Pose, "neutral jump installs gun extension $13");
    AssertEqual(0x00034567u, jumping.Kinematics.VerticalSpeedFixed, "jump gun extension preserves velocity");
    AssertEqual((ushort)1, jumping.Kinematics.YDirection, "jump gun extension preserves direction");

    var falling = new SamusState { Pose = SamusState.FallingRightPose, YPosition = 100 };
    falling.RefreshCollisionRadii(bus);
    falling.InitializeAnimation(bus);
    falling.Kinematics.YSpeed = 2;
    falling.Kinematics.YSubspeed = 0xabcd;
    falling.Kinematics.YDirection = 2;
    falling.HorizontalSpeed.BaseSpeed = 1;
    falling.ApplyAerialAimTransition(bus, SamusState.FallingGunExtendedRightPose);
    AssertEqual((byte)0x67, falling.Pose, "fall installs gun extension $67");
    AssertEqual(0x0002abcdu, falling.Kinematics.VerticalSpeedFixed, "fall gun extension preserves velocity");

    // `$91:E99B` samples HELD Shot, not newly pressed Shot. Holding X at horizontal impact
    // selects `$E6`; releasing it selects `$A4` from the same source metadata.
    falling.ApplyAerialLanding(bus, wasSpinning: false, (ushort)SnesButton.X);
    AssertEqual((byte)0xe6, falling.Pose, "held Shot selects firing landing $E6");
    AssertEqual((ushort)21, falling.Kinematics.YRadius, "firing landing expands to standing radius");
    AssertEqual(0u, falling.Kinematics.VerticalSpeedFixed, "firing landing clears Y speed");
    AssertEqual(0u, falling.HorizontalSpeed.BaseFixed, "firing landing clears X speed");
    falling.AnimateNoFx(bus);
    AssertEqual((byte)0xf8, falling.LastAnimationDelayCommand!.Value, "$E6 reaches F8 command");
    AssertTrue(falling.ApplyPendingVerifiedAnimationTransition(bus), "$E6 F8 transition applies");
    AssertEqual((byte)0x01, falling.Pose, "$E6 returns to standing right");

    var releasedLanding = new SamusState { Pose = SamusState.FallingGunExtendedRightPose };
    releasedLanding.RefreshCollisionRadii(bus);
    releasedLanding.InitializeAnimation(bus);
    releasedLanding.ApplyAerialLanding(bus, wasSpinning: false, controllerInput: 0);
    AssertEqual((byte)0xa4, releasedLanding.Pose, "released Shot selects ordinary landing $A4");

    var mirroredLanding = new SamusState { Pose = SamusState.FallingGunExtendedLeftPose };
    mirroredLanding.RefreshCollisionRadii(bus);
    mirroredLanding.InitializeAnimation(bus);
    mirroredLanding.ApplyAerialLanding(bus, wasSpinning: false, (ushort)SnesButton.X);
    AssertEqual((byte)0xe7, mirroredLanding.Pose, "held Shot selects mirrored firing landing $E7");

    AssertTrue(SamusState.IsRightFacingRunningPose(0x0b), "$0B is admitted by running dispatcher");
    AssertTrue(SamusState.IsRightFacingNormalJumpPose(0x13), "$13 is admitted by jump dispatcher");
    AssertTrue(SamusState.IsRightFacingFallingPose(0x67), "$67 is admitted by falling dispatcher");
    AssertTrue(SamusState.IsRightFacingLandingPose(0xe6), "$E6 is admitted by landing dispatcher");
    AssertTrue(SamusState.IsLeftFacingLandingPose(0xe7), "$E7 is admitted by mirrored landing dispatcher");

    Console.WriteLine("  Samus horizontal fire: ROM selection, six extended bodies, run-phase preservation, and firing landings agree.");
}

/// <summary>
/// Fixes the two ROM tables and signed formulas used by Landing Site's actual BTS-$12
/// non-square floor path at <c>$94:84D6</c> and <c>$94:87F4</c>.
/// </summary>
static void VerifySamusSlopePhysics()
{
    var bus = new TestAddressSpace();

    // Shape $12 selects multiplier-table word index 2*$12+1 = $25. Retail ROM stores
    // $00C0 there, causing a grounded 1.0 displacement to become 0.C000.
    WriteTestWord(
        bus,
        SamusSlopePhysics.HorizontalMultiplierTableAddress + (2 * 0x12 + 1) * 2,
        0x00c0);
    AssertEqual(
        0x0000c000,
        SamusSlopePhysics.ScaleGroundedHorizontalDisplacement(bus, 0x12, 0x00010000, verticalSpeed: 0),
        "BTS $12 grounded positive slope scaling");
    AssertEqual(
        unchecked((int)0xffff4000),
        SamusSlopePhysics.ScaleGroundedHorizontalDisplacement(bus, 0x12, -0x00010000, verticalSpeed: 0),
        "BTS $12 grounded negative slope scaling");

    // Ceiling bit $80 and any nonzero Y speed are independent native early returns.
    AssertEqual(
        0x00010000,
        SamusSlopePhysics.ScaleGroundedHorizontalDisplacement(bus, 0x92, 0x00010000, verticalSpeed: 0),
        "ceiling slope does not scale horizontal speed");
    AssertEqual(
        0x00010000,
        SamusSlopePhysics.ScaleGroundedHorizontalDisplacement(bus, 0x12, 0x00010000, verticalSpeed: 1),
        "airborne Samus does not receive grounded slope scaling");

    // Seed two explicit samples in shape $12's 16-byte row. BTS bit $40 mirrors X=0 to
    // sample 15, proving the profile selection comes from ROM rather than a line formula.
    int shapeTwelveRow = SamusSlopePhysics.AlignmentHeightTableAddress + 16 * 0x12;
    bus.WriteByte(shapeTwelveRow + 0, 8);
    bus.WriteByte(shapeTwelveRow + 15, 3);
    AssertEqual((byte)8, SamusSlopePhysics.ReadAlignmentHeight(bus, 0x12, 0), "slope unmirrored height sample");
    AssertEqual((byte)3, SamusSlopePhysics.ReadAlignmentHeight(bus, 0x52, 0), "slope mirrored height sample");

    // A type-1/BTS-$12 block occupies (0,1). At center Y=21 with radius 5, Samus's bottom
    // is Y=25 (low nibble 9). Height 8 yields correction 8-9-1 = -2, so $94:87F4 moves
    // her center upward to 19 and marks slope adjustment for the later grounding branch.
    var level = new RoomLevelData(
        widthInBlocks: 2,
        heightInBlocks: 2,
        foregroundEntries: [0x0000, 0x0000, 0x1000, 0x0000],
        behaviorBytes: [0x00, 0x00, 0x12, 0x00],
        backgroundEntries: [0, 0, 0, 0],
        blockDefinitions: new byte[8]);
    SlopeAlignmentResult aligned = SamusSlopePhysics.AlignYPosition(
        bus,
        level,
        xPosition: 0,
        yPosition: 21,
        yRadius: 5);
    AssertEqual((ushort)19, aligned.YPosition, "non-square floor slope whole-pixel Y correction");
    AssertTrue(aligned.Adjusted, "non-square floor slope sets adjusted flag");
    AssertEqual((byte)0x12, aligned.FloorBlock!.Value.Behavior, "non-square floor reports source BTS");

    SlopeAlignmentResult disabled = SamusSlopePhysics.AlignYPosition(
        bus,
        level,
        xPosition: 0,
        yPosition: 21,
        yRadius: 5,
        horizontalSlopeCollisionEnabled: false);
    AssertEqual((ushort)21, disabled.YPosition, "disabled horizontal slope collision preserves Y");
    AssertTrue(!disabled.Adjusted, "disabled horizontal slope collision preserves adjusted flag");

    Console.WriteLine("  Samus slopes: ROM multiplier, mirrored height samples, and non-square Y alignment agree.");
}

/// <summary>
/// Exercises the native block-span scans and supported type-0/type-1/type-8 reactions
/// surrounding the isolated slope primitives.
/// </summary>
static void VerifySamusBlockCollision()
{
    var bus = new TestAddressSpace();
    WriteTestWord(
        bus,
        SamusSlopePhysics.HorizontalMultiplierTableAddress + (2 * 0x12 + 1) * 2,
        0x00c0);
    bus.WriteByte(SamusSlopePhysics.AlignmentHeightTableAddress + 16 * 0x12, 8);

    const int width = 4;
    const int height = 4;
    var foreground = new ushort[width * height];
    var behavior = new byte[foreground.Length];

    // Row one contains a non-square slope in column one and an ordinary type-8 solid in
    // column two. Every other cell is dispatcher type zero air.
    foreground[1 * width + 1] = 0x1000;
    behavior[1 * width + 1] = 0x12;
    foreground[1 * width + 2] = 0x8000;
    var level = new RoomLevelData(
        width,
        height,
        foreground,
        behavior,
        new ushort[foreground.Length],
        new byte[8]);

    // At Y=21/radius5 the body penetrates shape-$12's height-8 sample by two pixels.
    // Horizontal type-1 processing first scales 1.0 to 0.C000, then $94:87F4 raises Y.
    var slopeBody = new SamusKinematicsState
    {
        XPosition = 16,
        YPosition = 21,
        XRadius = 5,
        YRadius = 5,
    };
    BlockMoveResult slopeMove = SamusBlockCollision.MoveHorizontal(
        bus,
        level,
        slopeBody,
        displacement: 0x00010000);
    AssertEqual(0x0000c000, slopeMove.AcceptedDisplacement, "horizontal scan applies BTS $12 multiplier");
    AssertEqual((ushort)16, slopeBody.XPosition, "subpixel slope move preserves whole X");
    AssertEqual((ushort)0xc000, slopeBody.XSubposition, "subpixel slope move updates X fraction");
    AssertEqual((ushort)19, slopeBody.YPosition, "post-horizontal scan aligns non-square floor Y");
    AssertTrue(slopeMove.PositionAdjustedBySlope, "horizontal scan reports slope adjustment");

    // Resting at center Y=19 puts the bottom at 23. The native +1.0 grounding probe
    // targets bottom 24; height 8 yields correction -1 and clips the accepted move to zero.
    var groundedBody = new SamusKinematicsState
    {
        XPosition = 16,
        YPosition = 19,
        XRadius = 5,
        YRadius = 5,
    };
    BlockMoveResult groundProbe = SamusBlockCollision.MoveVertical(
        bus,
        level,
        groundedBody,
        displacement: 0x00010000,
        scanLeftToRight: true);
    AssertTrue(groundProbe.Collided, "vertical non-square grounding probe collides");
    AssertEqual(0, groundProbe.AcceptedDisplacement, "vertical non-square grounding probe clips to zero");
    AssertEqual((ushort)19, groundedBody.YPosition, "grounding collision preserves resting center Y");
    AssertEqual((byte)0x12, groundProbe.CollisionBlock!.Value.Behavior, "grounding collision reports slope BTS");

    // Moving two pixels right from X=26 would enter the type-8 block at X=32. The solid
    // formula permits one pixel, writes subposition $FFFF, and stops center X at 27.FFFF.
    var wallBody = new SamusKinematicsState
    {
        XPosition = 26,
        YPosition = 24,
        XRadius = 5,
        YRadius = 5,
        HorizontalSlopeCollisionEnable = 0,
    };
    BlockMoveResult wallMove = SamusBlockCollision.MoveHorizontal(
        bus,
        level,
        wallBody,
        displacement: 0x00020000);
    AssertTrue(wallMove.Collided, "horizontal type-8 solid collision flag");
    AssertEqual(0x00010000, wallMove.AcceptedDisplacement, "horizontal type-8 solid clipped amount");
    AssertEqual((ushort)27, wallBody.XPosition, "horizontal type-8 solid whole X");
    AssertEqual((ushort)0xffff, wallBody.XSubposition, "horizontal type-8 solid right-wall fraction");

    // An air-only downward move preserves all 16.16 bits and has no collision record.
    var airBody = new SamusKinematicsState
    {
        // X=56 with radius five spans pixels 51..60, entirely inside air column three.
        XPosition = 56,
        YPosition = 19,
        XRadius = 5,
        YRadius = 5,
        YSubposition = 0x4000,
    };
    BlockMoveResult airMove = SamusBlockCollision.MoveVertical(
        bus,
        level,
        airBody,
        displacement: 0x00008000,
        scanLeftToRight: false);
    AssertTrue(!airMove.Collided, "vertical type-0 air has no collision");
    AssertEqual((ushort)19, airBody.YPosition, "vertical air fractional move whole Y");
    AssertEqual((ushort)0xc000, airBody.YSubposition, "vertical air fractional move subposition");

    // Shape four has all four $94:8E54 quadrant bytes set. Unlike an ordinary solid, its
    // clipping boundary is the leading 8-pixel half, although this fixture reaches the
    // block's first half so the permitted amount is one whole pixel.
    var squareForeground = new ushort[width * height];
    var squareBehavior = new byte[squareForeground.Length];
    squareForeground[1 * width + 2] = 0x1000;
    squareBehavior[1 * width + 2] = 0x04;
    squareForeground[2 * width + 1] = 0x1000;
    squareBehavior[2 * width + 1] = 0x04;
    var squareLevel = new RoomLevelData(
        width,
        height,
        squareForeground,
        squareBehavior,
        new ushort[squareForeground.Length],
        new byte[8]);

    var squareWallBody = new SamusKinematicsState
    {
        XPosition = 26,
        YPosition = 24,
        XRadius = 5,
        YRadius = 5,
        HorizontalSlopeCollisionEnable = 0,
    };
    BlockMoveResult squareWall = SamusBlockCollision.MoveHorizontal(
        bus,
        squareLevel,
        squareWallBody,
        displacement: 0x00020000);
    AssertTrue(squareWall.Collided, "horizontal fully-solid square slope collision");
    AssertEqual(0x00010000, squareWall.AcceptedDisplacement, "horizontal square-slope 8-pixel clipping");
    AssertEqual((ushort)0xffff, squareWallBody.XSubposition, "horizontal square slope writes right-wall fraction");

    var squareFloorBody = new SamusKinematicsState
    {
        XPosition = 16,
        YPosition = 26,
        XRadius = 5,
        YRadius = 5,
    };
    BlockMoveResult squareFloor = SamusBlockCollision.MoveVertical(
        bus,
        squareLevel,
        squareFloorBody,
        displacement: 0x00020000,
        scanLeftToRight: true);
    AssertTrue(squareFloor.Collided, "vertical fully-solid square slope collision");
    AssertEqual(0x00010000, squareFloor.AcceptedDisplacement, "vertical square-slope 8-pixel clipping");
    AssertTrue(squareFloorBody.PositionAdjustedBySlope, "downward square slope sets adjusted flag");
    AssertEqual((ushort)0xffff, squareFloorBody.YSubposition, "downward square slope writes floor fraction");

    Console.WriteLine("  Samus blocks: spans, air, square/non-square slopes, and solid clipping agree.");
}

/// <summary>
/// Verifies the bank-$90 ordering around the already-isolated speed and bank-$94 scans:
/// running accelerates, moves horizontally, then probes down; standing probes and clears.
/// </summary>
static void VerifySamusGroundedMovement()
{
    var bus = new TestAddressSpace();

    // Running movement type one reads the normal-air entry at $90:9F61. These are the real
    // retail words already asserted independently by VerifySamusHorizontalSpeed.
    WriteTestWord(bus, 0x909f61, 0x0000); // acceleration whole
    WriteTestWord(bus, 0x909f63, 0x3000); // acceleration fraction
    WriteTestWord(bus, 0x909f65, 0x0002); // maximum whole
    WriteTestWord(bus, 0x909f67, 0xc000); // maximum fraction
    WriteTestWord(bus, 0x909f69, 0x0000); // deceleration whole
    WriteTestWord(bus, 0x909f6b, 0x8000); // deceleration fraction

    // Shape $12 scales grounded horizontal displacement by $00C0/256 = 3/4 and exposes an
    // eight-pixel surface at X nibble zero. Row one is an uninterrupted two-block floor so
    // the radius scan can visit either column without introducing another dispatcher type.
    WriteTestWord(
        bus,
        SamusSlopePhysics.HorizontalMultiplierTableAddress + (2 * 0x12 + 1) * 2,
        0x00c0);
    bus.WriteByte(SamusSlopePhysics.AlignmentHeightTableAddress + 16 * 0x12, 8);
    var level = new RoomLevelData(
        widthInBlocks: 2,
        heightInBlocks: 3,
        foregroundEntries: [0, 0, 0x1000, 0x1000, 0, 0],
        behaviorBytes: [0, 0, 0x12, 0x12, 0, 0],
        backgroundEntries: new ushort[6],
        blockDefinitions: new byte[8]);

    var running = new SamusState { Pose = SamusState.MovingRightNormalPose };
    running.Kinematics.XPosition = 16;
    running.Kinematics.YPosition = 19;
    running.Kinematics.XRadius = 5;
    running.Kinematics.YRadius = 5;
    GroundedMovementResult first = SamusGroundedMovement.StepRunningRight(
        bus,
        level,
        running,
        nmiFrameCounter: 0);

    AssertEqual((ushort)0x0000, running.HorizontalSpeed.BaseSpeed, "running first acceleration whole speed");
    AssertEqual((ushort)0x3000, running.HorizontalSpeed.BaseSubspeed, "running first acceleration fractional speed");
    AssertEqual(0x00002400, first.Horizontal.AcceptedDisplacement, "running slope-scaled horizontal amount");
    AssertEqual((ushort)0x2400, running.Kinematics.XSubposition, "running horizontal amount reaches X subposition");
    AssertTrue(first.Vertical.Collided, "running total-speed-plus-one grounding probe collides");
    AssertEqual(0, first.Vertical.AcceptedDisplacement, "running grounding probe clips at surface");

    // A second frame proves that acceleration state persists and that odd-NMI scan order
    // produces the same single-surface result in this deliberately symmetric fixture.
    GroundedMovementResult second = SamusGroundedMovement.StepRunningRight(
        bus,
        level,
        running,
        nmiFrameCounter: 1);
    AssertEqual((ushort)0x6000, running.HorizontalSpeed.BaseSubspeed, "running second acceleration accumulates");
    AssertEqual(0x00004800, second.Horizontal.AcceptedDisplacement, "second running slope multiplier");
    AssertEqual((ushort)0x6c00, running.Kinematics.XSubposition, "second running displacement accumulates");
    AssertTrue(second.Vertical.Collided, "odd-frame grounding scan collides");

    // Momentum routine one selects acceleration mode two after Right is released. The next
    // $90:9A7E pass subtracts $0000.8000; from $0000.6000 this underflows the signed high
    // word and therefore clears both base halves and restores mode zero.
    running.HorizontalSpeed.AccelerationMode = 2;
    GroundedMovementResult decelerated = SamusGroundedMovement.StepRunningRight(
        bus,
        level,
        running,
        nmiFrameCounter: 0);
    AssertEqual((ushort)0, running.HorizontalSpeed.BaseSpeed, "running deceleration underflow clears whole speed");
    AssertEqual((ushort)0, running.HorizontalSpeed.BaseSubspeed, "running deceleration underflow clears fraction");
    AssertEqual((ushort)0, running.HorizontalSpeed.AccelerationMode, "running deceleration underflow restores acceleration mode");
    AssertEqual(0, decelerated.Horizontal.AcceptedDisplacement, "cleared deceleration frame has no X displacement");

    // Standing executes its zero-base MoveX/grounding calls before clearing momentum. Base
    // speed itself is not included in Move_NoBaseSpeed_X, so the body remains on the same X.
    var standing = new SamusState { Pose = SamusState.FacingRightNormalPose };
    standing.Kinematics.XPosition = 16;
    standing.Kinematics.YPosition = 19;
    standing.Kinematics.XRadius = 5;
    standing.Kinematics.YRadius = 5;
    standing.HorizontalSpeed.BaseSpeed = 2;
    standing.HorizontalSpeed.BaseSubspeed = 0xc000;
    GroundedMovementResult idle = SamusGroundedMovement.StepStandingRight(
        bus,
        level,
        standing,
        nmiFrameCounter: 0);
    AssertEqual(0, idle.Horizontal.AcceptedDisplacement, "standing no-base horizontal amount");
    AssertTrue(idle.Vertical.Collided, "standing one-pixel grounding probe collides");
    AssertEqual((ushort)0, standing.HorizontalSpeed.BaseSpeed, "standing clears base speed whole");
    AssertEqual((ushort)0, standing.HorizontalSpeed.BaseSubspeed, "standing clears base speed fraction");

    // The front-view branch returns before every ordinary standing movement call. Seed
    // deliberately stale motion/collision values to prove the sole native write is the
    // vertical-result clear and that no host convenience cleanup leaked into this path.
    var forward = new SamusState
    {
        Pose = SamusState.ForwardFacingPowerSuitPose,
        XPosition = 0x1234,
        YPosition = 0x5678,
        SolidVerticalCollisionResult = 9,
    };
    forward.HorizontalSpeed.BaseSpeed = 2;
    forward.HorizontalSpeed.BaseSubspeed = 0x3456;
    forward.HorizontalSpeed.ExtraRunSpeed = 1;
    BlockMoveResult? stationaryForward = SamusGroundedMovement.StepFacingForward(
        bus,
        level,
        forward,
        nmiFrameCounter: 0);
    AssertTrue(stationaryForward is null, "zero elevator status skips the vertical scan");
    AssertEqual((ushort)0, forward.SolidVerticalCollisionResult, "forward movement clears vertical collision result");
    AssertEqual((ushort)0x1234, forward.XPosition, "forward movement does not scan or move X");
    AssertEqual((ushort)0x5678, forward.YPosition, "stationary forward movement does not move Y");
    AssertEqual((ushort)2, forward.HorizontalSpeed.BaseSpeed, "forward dispatcher retains stale base speed");
    AssertEqual((ushort)0x3456, forward.HorizontalSpeed.BaseSubspeed, "forward dispatcher retains stale base subspeed");
    AssertEqual((ushort)1, forward.HorizontalSpeed.ExtraRunSpeed, "forward dispatcher retains stale extra speed");

    // `$90:A392` consumes actor-owned elevator status but performs Samus's movement itself.
    // Put a valid solid-enemy candidate exactly one pixel below an all-air room body: the
    // ordinary MoveVertical wrapper would stop on it, whereas `$94:9763` must deliberately
    // ignore it and accept the complete 1.0000 downward displacement.
    var elevatorLevel = new RoomLevelData(
        widthInBlocks: 8,
        heightInBlocks: 8,
        foregroundEntries: new ushort[64],
        behaviorBytes: new byte[64],
        backgroundEntries: new ushort[64],
        blockDefinitions: new byte[8]);
    forward.XPosition = 100;
    forward.YPosition = 100;
    forward.Kinematics.XSubposition = 0x1234;
    forward.Kinematics.YSubposition = 0x5678;
    forward.Kinematics.XRadius = 5;
    forward.Kinematics.YRadius = 10;
    forward.Kinematics.InteractiveEnemies =
    [
        new SolidEnemyCollisionBody(
            Index: 0x01c0,
            XPosition: 100,
            YPosition: 116,
            XRadius: 5,
            YRadius: 5,
            FreezeTimer: 0,
            Properties: 0x8000),
    ];
    forward.SolidVerticalCollisionResult = 9;
    BlockMoveResult? elevatorMove = SamusGroundedMovement.StepFacingForward(
        bus,
        elevatorLevel,
        forward,
        nmiFrameCounter: 1,
        elevatorIsMoving: true);
    AssertTrue(elevatorMove is { Collided: false }, "elevator block scan accepts clear air");
    BlockMoveResult acceptedElevatorMove = elevatorMove ?? throw new InvalidOperationException(
        "A nonzero elevator status must execute the one-pixel vertical scan.");
    AssertTrue(acceptedElevatorMove.EnemyCollision is null,
        "elevator `$94:9763` path skips a colliding solid enemy");
    AssertEqual(0x00010000, acceptedElevatorMove.AcceptedDisplacement,
        "elevator requests exact one-pixel downward displacement");
    AssertEqual((ushort)101, forward.YPosition, "elevator moves forward-facing Samus down one pixel");
    AssertEqual((ushort)0x5678, forward.Kinematics.YSubposition,
        "whole-pixel elevator motion preserves Y fraction");
    AssertEqual((ushort)0, forward.SolidVerticalCollisionResult,
        "elevator path clears vertical collision result after movement");

    Console.WriteLine("  Samus movement: forward/elevator, standing, and running speed/X/slope/grounding order agree.");
}

/// <summary>
/// Locks down the newly translated leftward and grounded-reversal routes independently of
/// WinForms: literal pose bytes, command $F8 timing, mode-one direction inversion, and the
/// mode-clear boundary are all observable here under a normal debugger.
/// </summary>
static void VerifySamusGroundedReversal()
{
    var bus = new TestAddressSpace();

    // Both ordinary running directions use movement type one's $90:9F61 record. The turn
    // handler uses movement type $0E and therefore reads $90:9FFD. These fixture words are
    // shaped like the retail records but deliberately simple enough to audit by inspection.
    WriteTestWord(bus, 0x909f61, 0x0000);
    WriteTestWord(bus, 0x909f63, 0x3000);
    WriteTestWord(bus, 0x909f65, 0x0002);
    WriteTestWord(bus, 0x909f67, 0xc000);
    WriteTestWord(bus, 0x909f69, 0x0000);
    WriteTestWord(bus, 0x909f6b, 0x8000);
    WriteTestWord(bus, 0x909ffd, 0x0000);
    WriteTestWord(bus, 0x909fff, 0x3000);
    WriteTestWord(bus, 0x90a001, 0x0002);
    WriteTestWord(bus, 0x90a003, 0xc000);
    WriteTestWord(bus, 0x90a005, 0x0000);
    WriteTestWord(bus, 0x90a007, 0x4000);

    // A broad row of ordinary type-$8 solids removes slope scaling from this test. Center
    // Y=11 with radius 5 rests exactly on the row-one top edge at physical Y=16.
    const int width = 8;
    var foreground = new ushort[width * 3];
    for (int x = 0; x < width; x++)
        foreground[width + x] = 0x8000;
    var level = new RoomLevelData(
        width,
        3,
        foreground,
        new byte[foreground.Length],
        new ushort[foreground.Length],
        new byte[8]);

    var movingLeft = new SamusState { Pose = SamusState.MovingLeftNormalPose };
    movingLeft.Kinematics.XPosition = 64;
    movingLeft.Kinematics.YPosition = 11;
    movingLeft.Kinematics.XRadius = 5;
    movingLeft.Kinematics.YRadius = 5;
    GroundedMovementResult leftFrame = SamusGroundedMovement.StepRunningLeft(
        bus,
        level,
        movingLeft,
        nmiFrameCounter: 0);
    AssertEqual(-0x00003000, leftFrame.Horizontal.AcceptedDisplacement, "running-left signed displacement");
    AssertEqual((ushort)63, movingLeft.XPosition, "running-left borrows into whole X");
    AssertEqual((ushort)0xd000, movingLeft.Kinematics.XSubposition, "running-left fractional X");
    AssertTrue(leftFrame.Vertical.Collided, "running-left grounding probe collides");

    // The production movement port now also validates the pose's literal dispatcher byte,
    // so seed the two ordinary turn definitions before isolating their displacement.
    bus.WriteBytes(0x91b751, [0x04, 0x0e, 0xff, 0xfb, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b759, [0x08, 0x0e, 0xff, 0xfb, 0x06, 0x00, 0x15, 0x00]);

    // Pose $25 is already facing left ($04), but mode one makes $90:8EA9 choose the
    // opposite/right helper while its $0E speed record decelerates old momentum.
    var turnTowardLeft = new SamusState { Pose = SamusState.TurningRightToLeftPose };
    turnTowardLeft.Kinematics.XPosition = 64;
    turnTowardLeft.Kinematics.YPosition = 11;
    turnTowardLeft.Kinematics.XRadius = 5;
    turnTowardLeft.Kinematics.YRadius = 5;
    turnTowardLeft.HorizontalSpeed.BaseSpeed = 1;
    turnTowardLeft.HorizontalSpeed.AccelerationMode = 1;
    GroundedMovementResult carriedRight = SamusGroundedMovement.StepTurningOnGround(
        bus,
        level,
        turnTowardLeft,
        nmiFrameCounter: 0);
    AssertEqual((ushort)0xc000, turnTowardLeft.HorizontalSpeed.BaseSubspeed, "left-turn deceleration amount");
    AssertEqual(0x0000c000, carriedRight.Horizontal.AcceptedDisplacement, "left-turn carries rightward momentum");

    // Pose $26 is the mirror: it displays a right-facing turn but carries old momentum to
    // the left. This is not a host sign choice; it is the other branch of $90:8EA9.
    var turnTowardRight = new SamusState { Pose = SamusState.TurningLeftToRightPose };
    turnTowardRight.Kinematics.XPosition = 64;
    turnTowardRight.Kinematics.YPosition = 11;
    turnTowardRight.Kinematics.XRadius = 5;
    turnTowardRight.Kinematics.YRadius = 5;
    turnTowardRight.HorizontalSpeed.BaseSpeed = 1;
    turnTowardRight.HorizontalSpeed.AccelerationMode = 1;
    GroundedMovementResult carriedLeft = SamusGroundedMovement.StepTurningOnGround(
        bus,
        level,
        turnTowardRight,
        nmiFrameCounter: 1);
    AssertEqual(-0x0000c000, carriedLeft.Horizontal.AcceptedDisplacement, "right-turn carries leftward momentum");

    // Underflow clears speed and mode inside $90:9A7E before direction dispatch. The last
    // turn frame consequently requests exactly zero rather than crossing into new motion.
    var exhaustedTurn = new SamusState { Pose = SamusState.TurningRightToLeftPose };
    exhaustedTurn.Kinematics.XPosition = 64;
    exhaustedTurn.Kinematics.YPosition = 11;
    exhaustedTurn.Kinematics.XRadius = 5;
    exhaustedTurn.Kinematics.YRadius = 5;
    exhaustedTurn.HorizontalSpeed.BaseSubspeed = 0x2000;
    exhaustedTurn.HorizontalSpeed.AccelerationMode = 1;
    GroundedMovementResult stopped = SamusGroundedMovement.StepTurningOnGround(
        bus,
        level,
        exhaustedTurn,
        nmiFrameCounter: 0);
    AssertEqual((ushort)0, exhaustedTurn.HorizontalSpeed.BaseSubspeed, "turn underflow clears speed");
    AssertEqual((ushort)0, exhaustedTurn.HorizontalSpeed.AccelerationMode, "turn underflow clears mode one");
    AssertEqual(0, stopped.Horizontal.AcceptedDisplacement, "turn underflow does not reverse early");

    // Seed the exact retail pose metadata and delay bytecode needed by $09->$25->$02.
    // The compact standing/running streams only provide valid frame-zero delays because
    // this test's subject is the turn stream itself.
    bus.WriteBytes(0x91b639, [0x04, 0x00, 0xff, 0x07, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b671, [0x08, 0x01, 0x01, 0x02, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b679, [0x04, 0x01, 0x02, 0x07, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b751, [0x04, 0x0e, 0xff, 0xfb, 0x06, 0x00, 0x15, 0x00]);
    WriteTestWord(bus, 0x91b014, 0xc100); // pose $02
    WriteTestWord(bus, 0x91b022, 0xc110); // pose $09
    WriteTestWord(bus, 0x91b024, 0xc120); // pose $0A
    WriteTestWord(bus, 0x91b05a, 0xc130); // pose $25
    bus.WriteBytes(0x91c100, [0x0a]);
    bus.WriteBytes(0x91c110, [0x02]);
    bus.WriteBytes(0x91c120, [0x02]);
    bus.WriteBytes(0x91c130, [0x02, 0x02, 0x02, 0xf8, 0x02]);

    var animatedTurn = new SamusState { Pose = SamusState.MovingRightNormalPose };
    animatedTurn.HorizontalSpeed.BaseSubspeed = 0x8000;
    animatedTurn.HorizontalSpeed.ExtraRunSubspeed = 0x4000;
    animatedTurn.ApplyGroundedTurn(bus, SamusState.TurningRightToLeftPose);
    AssertEqual((byte)0x25, animatedTurn.Pose, "input reversal installs pose $25");
    AssertEqual((ushort)0xc000, animatedTurn.HorizontalSpeed.BaseSubspeed, "turn setup folds extra into base speed");
    AssertEqual((ushort)0, animatedTurn.HorizontalSpeed.ExtraRunSubspeed, "turn setup consumes extra speed");
    AssertEqual((ushort)1, animatedTurn.HorizontalSpeed.AccelerationMode, "turn setup selects mode one");

    // Three two-tick art frames lead to byte index three, command $F8. Its operand $02 is
    // pending until the transition seam; applying it initializes pose $02 frame zero.
    for (int tick = 0; tick < 6; tick++)
        animatedTurn.AnimateNoFx(bus);
    AssertEqual((byte)0xf8, animatedTurn.LastAnimationDelayCommand!.Value, "turn reaches command $F8");
    AssertEqual((byte)0x02, animatedTurn.PendingTransitionalPose!.Value, "turn $F8 publishes left-standing pose");
    AssertTrue(animatedTurn.ApplyPendingVerifiedAnimationTransition(bus), "turn animation transition applies");
    AssertEqual((byte)0x02, animatedTurn.Pose, "turn animation ends facing left");
    AssertEqual((ushort)0, animatedTurn.AnimationFrame, "turn completion resets animation frame");
    AssertEqual((ushort)10, animatedTurn.AnimationFrameTimer, "left-standing delay initializes from ROM stream");

    animatedTurn.ApplyStandingLeftToRunningLeft(bus);
    AssertEqual((byte)0x0a, animatedTurn.Pose, "held left starts ordinary left run");
    animatedTurn.ApplyRunningLeftToStandingLeft(bus);
    AssertEqual((byte)0x02, animatedTurn.Pose, "left run no-button fallback stands left");

    // The retail initializer does not blindly accept the generic `$25/$26` produced by
    // the input transition table. It reads byte three of the previous pose definition and
    // indexes `$91:F9C2`, preserving straight-up, diagonal-up, or diagonal-down aim. Seed
    // the six source definitions literally so this test will fail if either their native
    // metadata or the selector mapping is accidentally changed.
    (byte SourcePose, byte[] Definition)[] aimedSources =
    [
        (SamusState.StandingAimUpRightPose, [0x08, 0x00, 0x01, 0x00, 0x06, 0x00, 0x15, 0x00]),
        (SamusState.StandingAimUpLeftPose, [0x04, 0x00, 0x02, 0x09, 0x06, 0x00, 0x15, 0x00]),
        (SamusState.StandingAimDiagonalUpRightPose, [0x08, 0x00, 0x01, 0x01, 0x06, 0x00, 0x15, 0x00]),
        (SamusState.StandingAimDiagonalUpLeftPose, [0x04, 0x00, 0x02, 0x08, 0x06, 0x00, 0x15, 0x00]),
        (SamusState.StandingAimDiagonalDownRightPose, [0x08, 0x00, 0x01, 0x03, 0x06, 0x00, 0x15, 0x00]),
        (SamusState.StandingAimDiagonalDownLeftPose, [0x04, 0x00, 0x02, 0x06, 0x06, 0x00, 0x15, 0x00]),
    ];
    foreach ((byte sourcePose, byte[] definition) in aimedSources)
    {
        bus.WriteBytes(0x91b629 + sourcePose * 8, definition);

        // A ten-tick frame-zero stream is sufficient after `$F8` installs the standing
        // destination. No later target animation frame is observed by this focused test.
        ushort streamAddress = (ushort)(0xc200 + sourcePose * 2);
        WriteTestWord(bus, 0x91b010 + sourcePose * 2, streamAddress);
        bus.WriteByte(0x910000 + streamAddress, 0x0a);
    }

    // Each turn definition also comes directly from bank $91. `$FA` and `$FC` are the
    // aimed-turn shot-direction markers; they are intentionally preserved here rather
    // than normalized to ordinary direction bytes.
    (byte TurnPose, byte[] Definition)[] aimedTurns =
    [
        (SamusState.TurningRightToLeftAimUpPose, [0x04, 0x0e, 0xff, 0xfa, 0x06, 0x00, 0x15, 0x00]),
        (SamusState.TurningLeftToRightAimUpPose, [0x08, 0x0e, 0xff, 0xfa, 0x06, 0x00, 0x15, 0x00]),
        (SamusState.TurningRightToLeftAimDiagonalDownPose, [0x04, 0x0e, 0xff, 0xfc, 0x06, 0x00, 0x15, 0x00]),
        (SamusState.TurningLeftToRightAimDiagonalDownPose, [0x08, 0x0e, 0xff, 0xfc, 0x06, 0x00, 0x15, 0x00]),
        (SamusState.TurningRightToLeftAimDiagonalUpPose, [0x04, 0x0e, 0xff, 0xfa, 0x06, 0x00, 0x15, 0x00]),
        (SamusState.TurningLeftToRightAimDiagonalUpPose, [0x08, 0x0e, 0xff, 0xfa, 0x06, 0x00, 0x15, 0x00]),
    ];
    foreach ((byte turnPose, byte[] definition) in aimedTurns)
        bus.WriteBytes(0x91b629 + turnPose * 8, definition);

    // NTSC uses three two-tick art frames for every aimed grounded turn. `$F8` then
    // installs the corresponding opposite-facing standing-aim pose shown in this table.
    (byte SourcePose, byte GenericTurn, byte SelectedTurn, byte Destination)[] aimedTurnCases =
    [
        (SamusState.StandingAimUpRightPose, SamusState.TurningRightToLeftPose,
            SamusState.TurningRightToLeftAimUpPose, SamusState.StandingAimUpLeftPose),
        (SamusState.StandingAimUpLeftPose, SamusState.TurningLeftToRightPose,
            SamusState.TurningLeftToRightAimUpPose, SamusState.StandingAimUpRightPose),
        (SamusState.StandingAimDiagonalUpRightPose, SamusState.TurningRightToLeftPose,
            SamusState.TurningRightToLeftAimDiagonalUpPose, SamusState.StandingAimDiagonalUpLeftPose),
        (SamusState.StandingAimDiagonalUpLeftPose, SamusState.TurningLeftToRightPose,
            SamusState.TurningLeftToRightAimDiagonalUpPose, SamusState.StandingAimDiagonalUpRightPose),
        (SamusState.StandingAimDiagonalDownRightPose, SamusState.TurningRightToLeftPose,
            SamusState.TurningRightToLeftAimDiagonalDownPose, SamusState.StandingAimDiagonalDownLeftPose),
        (SamusState.StandingAimDiagonalDownLeftPose, SamusState.TurningLeftToRightPose,
            SamusState.TurningLeftToRightAimDiagonalDownPose, SamusState.StandingAimDiagonalDownRightPose),
    ];

    // Real standing/turn poses have radius 21, unlike the compact radius-five fixture used
    // above to isolate ordinary displacement arithmetic. Place their center at Y=27 over
    // a row-three floor: top Y=6 remains inside the room and bottom Y=48 touches that floor.
    var aimedForeground = new ushort[width * 5];
    for (int x = 0; x < width; x++)
        aimedForeground[width * 3 + x] = 0x8000;
    var aimedLevel = new RoomLevelData(
        width,
        5,
        aimedForeground,
        new byte[aimedForeground.Length],
        new ushort[aimedForeground.Length],
        new byte[8]);

    for (int caseIndex = 0; caseIndex < aimedTurnCases.Length; caseIndex++)
    {
        var testCase = aimedTurnCases[caseIndex];

        // Give every turn its own bytecode location. This catches swapped destinations
        // independently rather than allowing two cases to share a forgiving stream.
        ushort turnStreamAddress = (ushort)(0xc300 + caseIndex * 0x10);
        WriteTestWord(bus, 0x91b010 + testCase.SelectedTurn * 2, turnStreamAddress);
        bus.WriteBytes(
            0x910000 + turnStreamAddress,
            [0x02, 0x02, 0x02, 0xf8, testCase.Destination]);

        var aimedTurn = new SamusState { Pose = testCase.SourcePose };
        aimedTurn.Kinematics.XPosition = 64;
        aimedTurn.Kinematics.YPosition = 27;
        aimedTurn.Kinematics.XRadius = 5;
        aimedTurn.Kinematics.YRadius = 21;

        // `$91:F931` performs a 32-bit fixed-point add. These values force a fractional
        // carry, proving that the extra run component is folded before it is cleared.
        aimedTurn.HorizontalSpeed.BaseSpeed = 1;
        aimedTurn.HorizontalSpeed.BaseSubspeed = 0xd000;
        aimedTurn.HorizontalSpeed.ExtraRunSubspeed = 0x5000;
        aimedTurn.ApplyGroundedTurn(bus, testCase.GenericTurn);
        AssertEqual(testCase.SelectedTurn, aimedTurn.Pose, $"aimed turn selector case {caseIndex}");
        AssertEqual((ushort)2, aimedTurn.HorizontalSpeed.BaseSpeed, $"aimed turn speed carry case {caseIndex}");
        AssertEqual((ushort)0x2000, aimedTurn.HorizontalSpeed.BaseSubspeed, $"aimed turn folded fraction case {caseIndex}");
        AssertEqual((ushort)0, aimedTurn.HorizontalSpeed.ExtraRunSubspeed, $"aimed turn consumes extra fraction case {caseIndex}");
        AssertEqual((ushort)1, aimedTurn.HorizontalSpeed.AccelerationMode, $"aimed turn mode one case {caseIndex}");

        GroundedMovementResult carriedMomentum = SamusGroundedMovement.StepTurningOnGround(
            bus,
            aimedLevel,
            aimedTurn,
            nmiFrameCounter: (ushort)caseIndex);
        bool beganFacingRight = (caseIndex & 1) == 0;
        AssertTrue(
            beganFacingRight
                ? carriedMomentum.Horizontal.AcceptedDisplacement > 0
                : carriedMomentum.Horizontal.AcceptedDisplacement < 0,
            $"aimed turn preserves old momentum direction case {caseIndex}");

        for (int tick = 0; tick < 6; tick++)
            aimedTurn.AnimateNoFx(bus);
        AssertEqual((byte)0xf8, aimedTurn.LastAnimationDelayCommand!.Value, $"aimed turn reaches $F8 case {caseIndex}");
        AssertEqual(testCase.Destination, aimedTurn.PendingTransitionalPose!.Value, $"aimed turn publishes destination case {caseIndex}");
        AssertTrue(aimedTurn.ApplyPendingVerifiedAnimationTransition(bus), $"aimed turn transition applies case {caseIndex}");
        AssertEqual(testCase.Destination, aimedTurn.Pose, $"aimed turn destination case {caseIndex}");
        AssertEqual((ushort)0, aimedTurn.AnimationFrame, $"aimed turn target frame zero case {caseIndex}");
        AssertEqual((ushort)10, aimedTurn.AnimationFrameTimer, $"aimed turn target timer case {caseIndex}");
    }

    // Crouched aim turns are the deliberate type-$17 oddity in this family. Give that
    // movement type the same conspicuous synthetic speed record as type `$0E`; selecting
    // the wrong table address would otherwise read zero-filled fixture memory and stop.
    WriteTestWord(bus, 0x90a069, 0x0000);
    WriteTestWord(bus, 0x90a06b, 0x3000);
    WriteTestWord(bus, 0x90a06d, 0x0002);
    WriteTestWord(bus, 0x90a06f, 0xc000);
    WriteTestWord(bus, 0x90a071, 0x0000);
    WriteTestWord(bus, 0x90a073, 0x4000);

    (byte SourcePose, byte[] Definition)[] crouchedSources =
    [
        (SamusState.CrouchingRightPose, [0x08, 0x05, 0x27, 0x02, 0x00, 0x00, 0x10, 0x00]),
        (SamusState.CrouchingLeftPose, [0x04, 0x05, 0x28, 0x07, 0x00, 0x00, 0x10, 0x00]),
        (SamusState.CrouchingAimUpRightPose, [0x08, 0x05, 0x27, 0x00, 0x00, 0x00, 0x10, 0x00]),
        (SamusState.CrouchingAimUpLeftPose, [0x04, 0x05, 0x28, 0x09, 0x00, 0x00, 0x10, 0x00]),
        (SamusState.CrouchingAimDiagonalUpRightPose, [0x08, 0x05, 0x27, 0x01, 0x00, 0x00, 0x10, 0x00]),
        (SamusState.CrouchingAimDiagonalUpLeftPose, [0x04, 0x05, 0x28, 0x08, 0x00, 0x00, 0x10, 0x00]),
        (SamusState.CrouchingAimDiagonalDownRightPose, [0x08, 0x05, 0x27, 0x03, 0x00, 0x00, 0x10, 0x00]),
        (SamusState.CrouchingAimDiagonalDownLeftPose, [0x04, 0x05, 0x28, 0x06, 0x00, 0x00, 0x10, 0x00]),
    ];
    foreach ((byte sourcePose, byte[] definition) in crouchedSources)
    {
        bus.WriteBytes(0x91b629 + sourcePose * 8, definition);
        ushort streamAddress = (ushort)(0xc600 + sourcePose * 2);
        WriteTestWord(bus, 0x91b010 + sourcePose * 2, streamAddress);
        bus.WriteByte(0x910000 + streamAddress, 0x0a);
    }

    // `$43/$44` really are movement type `$0E`; the six aimed records really are `$17`.
    // Keeping those literal bytes in the fixture protects the strange native dispatcher
    // split from a future cleanup that might look attractive but be historically wrong.
    (byte TurnPose, byte[] Definition)[] crouchedTurns =
    [
        (SamusState.TurningRightToLeftCrouchingPose, [0x04, 0x0e, 0xff, 0xfb, 0x00, 0x00, 0x10, 0x00]),
        (SamusState.TurningLeftToRightCrouchingPose, [0x08, 0x0e, 0xff, 0xfb, 0x00, 0x00, 0x10, 0x00]),
        (SamusState.TurningRightToLeftCrouchingAimUpPose, [0x04, 0x17, 0x28, 0xfa, 0x00, 0x00, 0x10, 0x00]),
        (SamusState.TurningLeftToRightCrouchingAimUpPose, [0x08, 0x17, 0x28, 0xfa, 0x00, 0x00, 0x10, 0x00]),
        (SamusState.TurningRightToLeftCrouchingAimDiagonalDownPose, [0x04, 0x17, 0x28, 0xfc, 0x00, 0x00, 0x10, 0x00]),
        (SamusState.TurningLeftToRightCrouchingAimDiagonalDownPose, [0x08, 0x17, 0x28, 0xfc, 0x00, 0x00, 0x10, 0x00]),
        (SamusState.TurningRightToLeftCrouchingAimDiagonalUpPose, [0x04, 0x17, 0x28, 0xfa, 0x00, 0x00, 0x10, 0x00]),
        (SamusState.TurningLeftToRightCrouchingAimDiagonalUpPose, [0x08, 0x17, 0x28, 0xfa, 0x00, 0x00, 0x10, 0x00]),
    ];
    foreach ((byte turnPose, byte[] definition) in crouchedTurns)
        bus.WriteBytes(0x91b629 + turnPose * 8, definition);

    (byte SourcePose, byte GenericTurn, byte SelectedTurn, byte Destination)[] crouchedTurnCases =
    [
        (SamusState.CrouchingRightPose, SamusState.TurningRightToLeftCrouchingPose,
            SamusState.TurningRightToLeftCrouchingPose, SamusState.CrouchingLeftPose),
        (SamusState.CrouchingLeftPose, SamusState.TurningLeftToRightCrouchingPose,
            SamusState.TurningLeftToRightCrouchingPose, SamusState.CrouchingRightPose),
        (SamusState.CrouchingAimUpRightPose, SamusState.TurningRightToLeftCrouchingPose,
            SamusState.TurningRightToLeftCrouchingAimUpPose, SamusState.CrouchingAimUpLeftPose),
        (SamusState.CrouchingAimUpLeftPose, SamusState.TurningLeftToRightCrouchingPose,
            SamusState.TurningLeftToRightCrouchingAimUpPose, SamusState.CrouchingAimUpRightPose),
        (SamusState.CrouchingAimDiagonalUpRightPose, SamusState.TurningRightToLeftCrouchingPose,
            SamusState.TurningRightToLeftCrouchingAimDiagonalUpPose, SamusState.CrouchingAimDiagonalUpLeftPose),
        (SamusState.CrouchingAimDiagonalUpLeftPose, SamusState.TurningLeftToRightCrouchingPose,
            SamusState.TurningLeftToRightCrouchingAimDiagonalUpPose, SamusState.CrouchingAimDiagonalUpRightPose),
        (SamusState.CrouchingAimDiagonalDownRightPose, SamusState.TurningRightToLeftCrouchingPose,
            SamusState.TurningRightToLeftCrouchingAimDiagonalDownPose, SamusState.CrouchingAimDiagonalDownLeftPose),
        (SamusState.CrouchingAimDiagonalDownLeftPose, SamusState.TurningLeftToRightCrouchingPose,
            SamusState.TurningLeftToRightCrouchingAimDiagonalDownPose, SamusState.CrouchingAimDiagonalDownRightPose),
    ];
    for (int caseIndex = 0; caseIndex < crouchedTurnCases.Length; caseIndex++)
    {
        var testCase = crouchedTurnCases[caseIndex];
        ushort turnStreamAddress = (ushort)(0xc800 + caseIndex * 0x10);
        WriteTestWord(bus, 0x91b010 + testCase.SelectedTurn * 2, turnStreamAddress);
        bus.WriteBytes(
            0x910000 + turnStreamAddress,
            [0x02, 0x02, 0x02, 0xf8, testCase.Destination]);

        var crouchedTurn = new SamusState { Pose = testCase.SourcePose };
        crouchedTurn.Kinematics.XPosition = 64;
        crouchedTurn.Kinematics.YPosition = 32;
        crouchedTurn.Kinematics.XRadius = 5;
        crouchedTurn.Kinematics.YRadius = 16;
        crouchedTurn.HorizontalSpeed.BaseSpeed = 1;
        crouchedTurn.HorizontalSpeed.BaseSubspeed = 0xd000;
        crouchedTurn.HorizontalSpeed.ExtraRunSubspeed = 0x5000;
        crouchedTurn.ApplyGroundedTurn(bus, testCase.GenericTurn);
        AssertEqual(testCase.SelectedTurn, crouchedTurn.Pose, $"crouched turn selector case {caseIndex}");
        AssertEqual((ushort)16, crouchedTurn.Kinematics.YRadius, $"crouched turn radius case {caseIndex}");
        AssertEqual((ushort)1, crouchedTurn.HorizontalSpeed.AccelerationMode, $"crouched turn mode one case {caseIndex}");

        GroundedMovementResult crouchedMomentum = SamusGroundedMovement.StepTurningOnGround(
            bus,
            aimedLevel,
            crouchedTurn,
            nmiFrameCounter: (ushort)caseIndex);
        bool beganFacingRight = (caseIndex & 1) == 0;
        AssertTrue(
            beganFacingRight
                ? crouchedMomentum.Horizontal.AcceptedDisplacement > 0
                : crouchedMomentum.Horizontal.AcceptedDisplacement < 0,
            $"crouched turn preserves old momentum direction case {caseIndex}");
        AssertTrue(crouchedMomentum.Vertical.Collided, $"crouched turn grounding branch case {caseIndex}");

        for (int tick = 0; tick < 6; tick++)
            crouchedTurn.AnimateNoFx(bus);
        AssertEqual((byte)0xf8, crouchedTurn.LastAnimationDelayCommand!.Value, $"crouched turn reaches $F8 case {caseIndex}");
        AssertEqual(testCase.Destination, crouchedTurn.PendingTransitionalPose!.Value, $"crouched turn publishes destination case {caseIndex}");
        AssertTrue(crouchedTurn.ApplyPendingVerifiedAnimationTransition(bus), $"crouched turn transition applies case {caseIndex}");
        AssertEqual(testCase.Destination, crouchedTurn.Pose, $"crouched turn destination case {caseIndex}");
    }

    Console.WriteLine("  Samus reversal: standing/crouched selectors, mode-one carry, grounded type-$17, and $F8 agree.");
}

/// <summary>
/// Locks movement type `$10` to the literal bank-$90/$91 behavior: pose direction is the
/// travel direction despite opposite-facing art, the options word gates entry, definition
/// byte two exits on zero input, and `$BF-$C4` remain grounded until their `$F8` command or
/// transition table starts a real jump.
/// </summary>
static void VerifySamusMoonwalking()
{
    var bus = new TestAddressSpace();

    // `$90:9F55 + 10h * 0Ch = $90:A015`. Use an unmistakable quarter-pixel acceleration
    // with a two-pixel cap and eighth-pixel deceleration; all stable moonwalk records must
    // select this entry instead of borrowing running's type-one record.
    WriteTestWord(bus, 0x90a015, 0x0000);
    WriteTestWord(bus, 0x90a017, 0x4000);
    WriteTestWord(bus, 0x90a019, 0x0002);
    WriteTestWord(bus, 0x90a01b, 0x0000);
    WriteTestWord(bus, 0x90a01d, 0x0000);
    WriteTestWord(bus, 0x90a01f, 0x2000);

    // `$BF-$C4` dispatch through ordinary grounded-turn movement type `$0E`, whose own
    // deceleration record remains independently visible during their three art frames.
    WriteTestWord(bus, 0x909ffd, 0x0000);
    WriteTestWord(bus, 0x909fff, 0x4000);
    WriteTestWord(bus, 0x90a001, 0x0002);
    WriteTestWord(bus, 0x90a003, 0x0000);
    WriteTestWord(bus, 0x90a005, 0x0000);
    WriteTestWord(bus, 0x90a007, 0x2000);

    (byte Pose, byte[] Definition, byte Fallback, int Direction)[] stable =
    [
        (SamusState.MoonwalkFacingLeftPose,
            [0x08, 0x10, 0x02, 0x07, 0x06, 0x00, 0x15, 0x00], 0x02, 1),
        (SamusState.MoonwalkFacingRightPose,
            [0x04, 0x10, 0x01, 0x02, 0x06, 0x00, 0x15, 0x00], 0x01, -1),
        (SamusState.MoonwalkAimUpLeftPose,
            [0x08, 0x10, 0x06, 0x08, 0x06, 0x00, 0x15, 0x00], 0x06, 1),
        (SamusState.MoonwalkAimUpRightPose,
            [0x04, 0x10, 0x05, 0x01, 0x06, 0x00, 0x15, 0x00], 0x05, -1),
        (SamusState.MoonwalkAimDownLeftPose,
            [0x08, 0x10, 0x08, 0x06, 0x06, 0x00, 0x15, 0x00], 0x08, 1),
        (SamusState.MoonwalkAimDownRightPose,
            [0x04, 0x10, 0x07, 0x03, 0x06, 0x00, 0x15, 0x00], 0x07, -1),
    ];
    foreach ((byte pose, byte[] definition, _, _) in stable)
        bus.WriteBytes(0x91b629 + pose * 8, definition);

    // The standing definitions are the actual sources and no-button destinations for the
    // six records above. Their one-byte streams are enough because these focused checks do
    // not advance standing animation.
    (byte Pose, byte[] Definition)[] standing =
    [
        (0x01, [0x08, 0x00, 0x01, 0x02, 0x06, 0x00, 0x15, 0x00]),
        (0x02, [0x04, 0x00, 0x02, 0x07, 0x06, 0x00, 0x15, 0x00]),
        (0x05, [0x08, 0x00, 0x01, 0x01, 0x06, 0x00, 0x15, 0x00]),
        (0x06, [0x04, 0x00, 0x02, 0x08, 0x06, 0x00, 0x15, 0x00]),
        (0x07, [0x08, 0x00, 0x01, 0x03, 0x06, 0x00, 0x15, 0x00]),
        (0x08, [0x04, 0x00, 0x02, 0x06, 0x06, 0x00, 0x15, 0x00]),
    ];
    foreach ((byte pose, byte[] definition) in standing)
    {
        bus.WriteBytes(0x91b629 + pose * 8, definition);
        ushort stream = (ushort)(0xc000 + pose);
        WriteTestWord(bus, 0x91b010 + pose * 2, stream);
        bus.WriteByte(0x910000 + stream, 10);
    }

    // The disabled-option substitution enters ordinary right-to-left `$25`, including the
    // initializer's momentum fold and mode-one selection. `$25`'s short stream need not
    // complete here; merely initializing it proves the candidate was not retained.
    bus.WriteBytes(0x91b751, [0x04, 0x0e, 0xff, 0xfb, 0x06, 0x00, 0x15, 0x00]);
    WriteTestWord(bus, 0x91b05a, 0xc100);
    bus.WriteBytes(0x91c100, [0x02, 0x02, 0x02, 0xf8, 0x02]);
    var disabled = new SamusState { Pose = SamusState.FacingRightNormalPose };
    disabled.HorizontalSpeed.BaseSubspeed = 0x4000;
    disabled.HorizontalSpeed.ExtraRunSubspeed = 0x2000;
    disabled.ApplyMoonwalkPoseChange(bus, SamusState.MoonwalkFacingRightPose, moonwalkEnabled: false);
    AssertEqual(SamusState.TurningRightToLeftPose, disabled.Pose,
        "disabled Moonwalk option substitutes ordinary turn");
    AssertEqual((ushort)0x6000, disabled.HorizontalSpeed.BaseSubspeed,
        "disabled Moonwalk substitution folds extra momentum");
    AssertEqual((ushort)1, disabled.HorizontalSpeed.AccelerationMode,
        "disabled Moonwalk substitution starts mode one");

    // Enabled entry must preserve every exact candidate, not merely the unaimed pair.
    foreach ((byte target, _, byte fallback, _) in stable)
    {
        var candidate = new SamusState { Pose = fallback };
        ushort stream = (ushort)(0xc200 + target);
        WriteTestWord(bus, 0x91b010 + target * 2, stream);
        bus.WriteByte(0x910000 + stream, 2);
        candidate.ApplyMoonwalkPoseChange(bus, target, moonwalkEnabled: true);
        AssertEqual(target, candidate.Pose, $"enabled Moonwalk retains candidate ${target:X2}");

        // Command two's zero-controller fallback is immediate for movement type `$10` and
        // uses the target record's byte two. This helper applies that already-read byte at
        // the normal end-of-frame transition seam.
        candidate.ApplyMoonwalkPoseChange(bus, fallback, moonwalkEnabled: true);
        AssertEqual(fallback, candidate.Pose, $"moonwalk ${target:X2} fallback byte");
    }

    // A flat row of type-$8 solids isolates horizontal sign and the shared downward probe.
    const int width = 12;
    var foreground = new ushort[width * 3];
    for (int x = 0; x < width; x++)
        foreground[width + x] = 0x8000;
    var floor = new RoomLevelData(
        width,
        3,
        foreground,
        new byte[foreground.Length],
        new ushort[foreground.Length],
        new byte[8]);
    foreach ((byte pose, _, _, int direction) in stable)
    {
        var walker = new SamusState { Pose = pose, XPosition = 80, YPosition = 11 };
        walker.Kinematics.XRadius = 5;
        walker.Kinematics.YRadius = 5;
        GroundedMovementResult movement = SamusGroundedMovement.StepMoonwalking(
            bus, floor, walker, nmiFrameCounter: 0);
        AssertEqual(direction * 0x4000, movement.Horizontal.AcceptedDisplacement,
            $"moonwalk ${pose:X2} uses literal reversed direction");
        AssertTrue(movement.Vertical.Collided, $"moonwalk ${pose:X2} probes floor");
    }

    // Seed the six literal turn/jump records and delay streams. Each stream contains three
    // two-tick frames followed by `$F8,$1A/$19`, exactly `$91:B45B-$B478` for NTSC.
    (byte Source, byte Target, byte[] Definition, byte SpinTarget)[] turns =
    [
        (0x4a, 0xbf, [0x04, 0x0e, 0xff, 0xfb, 0x06, 0x00, 0x15, 0x00], 0x1a),
        (0x49, 0xc0, [0x08, 0x0e, 0xff, 0xfb, 0x06, 0x00, 0x15, 0x00], 0x19),
        (0x76, 0xc1, [0x04, 0x0e, 0xff, 0xfa, 0x08, 0x00, 0x15, 0x00], 0x1a),
        (0x75, 0xc2, [0x08, 0x0e, 0xff, 0xfa, 0x08, 0x00, 0x15, 0x00], 0x19),
        (0x78, 0xc3, [0x04, 0x0e, 0xff, 0xfc, 0x08, 0x00, 0x15, 0x00], 0x1a),
        (0x77, 0xc4, [0x08, 0x0e, 0xff, 0xfc, 0x08, 0x00, 0x15, 0x00], 0x19),
    ];
    foreach ((_, byte target, byte[] definition, byte spinTarget) in turns)
    {
        bus.WriteBytes(0x91b629 + target * 8, definition);
        ushort stream = (ushort)(0xc300 + (target - 0xbf) * 8);
        WriteTestWord(bus, 0x91b010 + target * 2, stream);
        bus.WriteBytes(0x910000 + stream, [0x02, 0x02, 0x02, 0xf8, spinTarget]);
    }

    // Spin endpoints and dry-air constants are read through production code after `$F8`.
    bus.WriteBytes(0x91b6f1, [0x08, 0x03, 0xff, 0xff, 0x00, 0x00, 0x0c, 0x00]);
    bus.WriteBytes(0x91b6f9, [0x04, 0x03, 0xff, 0xff, 0x00, 0x00, 0x0c, 0x00]);
    WriteTestWord(bus, 0x91b042, 0xc400);
    WriteTestWord(bus, 0x91b044, 0xc401);
    bus.WriteByte(0x91c400, 2);
    bus.WriteByte(0x91c401, 2);
    WriteTestWord(bus, 0x909eb9, 0x0004);
    WriteTestWord(bus, 0x909ebf, 0xe000);
    WriteTestWord(bus, 0x909ea1, 0x2800);
    WriteTestWord(bus, 0x909ea7, 0x0000);

    foreach ((byte source, byte target, _, _) in turns)
    {
        var turn = new SamusState { Pose = source, XPosition = 80, YPosition = 27 };
        turn.RefreshCollisionRadii(bus);
        turn.HorizontalSpeed.BaseSpeed = 1;
        turn.HorizontalSpeed.ExtraRunSubspeed = 0x4000;
        turn.ApplyMoonwalkTurnJump(bus, target);
        AssertEqual(target, turn.Pose, $"moonwalk ${source:X2} selects exact turn ${target:X2}");
        AssertEqual((ushort)1, turn.HorizontalSpeed.AccelerationMode,
            $"moonwalk turn ${target:X2} preserves reversal mode");
        AssertEqual((ushort)0, turn.Kinematics.YDirection,
            $"moonwalk turn ${target:X2} remains grounded before completion");
    }

    // Exercise the terminal command on one mirrored route. Six decrements consume three
    // two-tick art frames; applying `$F8,$1A` then creates the real 4.E000 upward launch.
    var animated = new SamusState { Pose = SamusState.MoonwalkFacingRightPose };
    animated.ApplyMoonwalkTurnJump(bus, SamusState.MoonwalkTurnJumpLeftPose);
    for (int tick = 0; tick < 6; tick++)
        animated.AnimateNoFx(bus);
    AssertEqual((byte)0xf8, animated.LastAnimationDelayCommand!.Value,
        "moonwalk turn reaches command $F8");
    AssertEqual(SamusState.SpinJumpLeftPose, animated.PendingTransitionalPose!.Value,
        "moonwalk turn publishes literal spin-left operand");
    AssertTrue(animated.ApplyPendingVerifiedAnimationTransition(bus),
        "moonwalk terminal spin transition applies");
    AssertEqual(SamusState.SpinJumpLeftPose, animated.Pose,
        "moonwalk terminal command enters spin jump");
    AssertEqual((ushort)4, animated.Kinematics.YSpeed,
        "moonwalk terminal command loads dry-air jump speed");
    AssertEqual((ushort)0xe000, animated.Kinematics.YSubspeed,
        "moonwalk terminal command loads dry-air jump subspeed");
    AssertEqual((ushort)1, animated.Kinematics.YDirection,
        "moonwalk terminal command begins upward motion");

    Console.WriteLine("  Moonwalk: option gate, six stable routes, reversed X, fallback, and $BF-$C4 jump art agree.");
}

/// <summary>
/// Verifies movement type `$15` and `$91:EADE`'s block-only producer, including the
/// otherwise easy-to-erase one-pixel arm-pump movement performed for prospective runs.
/// </summary>
static void VerifySamusRanIntoWall()
{
    var bus = new TestAddressSpace();

    (byte Pose, byte[] Definition)[] wallPoses =
    [
        (SamusState.RanIntoWallRightPose,
            [0x08, 0x15, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]),
        (SamusState.RanIntoWallLeftPose,
            [0x04, 0x15, 0xff, 0x07, 0x06, 0x00, 0x15, 0x00]),
        (SamusState.RanIntoWallAimUpRightPose,
            [0x08, 0x15, 0x89, 0x01, 0x06, 0x00, 0x15, 0x00]),
        (SamusState.RanIntoWallAimUpLeftPose,
            [0x04, 0x15, 0x8a, 0x08, 0x06, 0x00, 0x15, 0x00]),
        (SamusState.RanIntoWallAimDownRightPose,
            [0x08, 0x15, 0x89, 0x03, 0x06, 0x00, 0x15, 0x00]),
        (SamusState.RanIntoWallAimDownLeftPose,
            [0x04, 0x15, 0x8a, 0x06, 0x06, 0x00, 0x15, 0x00]),
    ];
    foreach ((byte pose, byte[] definition) in wallPoses)
    {
        bus.WriteBytes(0x91b629 + pose * 8, definition);
        WriteTestWord(bus, 0x91b010 + pose * 2, 0xc500);
    }
    bus.WriteBytes(0x91c500, [0x10, 0xff]);

    // Minimal current/prospective definitions make direction and movement-type selection
    // auditable without copying unrelated animation data into this focused fixture.
    bus.WriteBytes(0x91b631, [0x08, 0x00, 0x01, 0x02, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b639, [0x04, 0x00, 0x02, 0x07, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b671, [0x08, 0x01, 0x01, 0x02, 0x06, 0x00, 0x15, 0x00]);
    bus.WriteBytes(0x91b679, [0x04, 0x01, 0x02, 0x07, 0x06, 0x00, 0x15, 0x00]);

    byte[] selectedByShotDirection =
    [
        SamusState.StandingAimUpRightPose,
        SamusState.RanIntoWallAimUpRightPose,
        SamusState.RanIntoWallRightPose,
        SamusState.RanIntoWallAimDownRightPose,
        SamusState.RanIntoWallRightPose,
        SamusState.RanIntoWallLeftPose,
        SamusState.RanIntoWallAimDownLeftPose,
        SamusState.RanIntoWallLeftPose,
        SamusState.RanIntoWallAimUpLeftPose,
        SamusState.StandingAimUpLeftPose,
    ];
    for (byte direction = 0; direction < selectedByShotDirection.Length; direction++)
    {
        bus.WriteByte(0x91b629 + SamusState.MovingRightNormalPose * 8 + 3, direction);
        AssertEqual(
            selectedByShotDirection[direction],
            SamusState.SelectRanIntoWallPose(bus, SamusState.MovingRightNormalPose),
            $"ran-into-wall shot selector {direction}");
    }
    bus.WriteByte(0x91b629 + SamusState.MovingRightNormalPose * 8 + 3, 2);

    const int width = 12;
    var openForeground = new ushort[width * 4];
    for (int x = 0; x < width; x++)
        openForeground[width * 2 + x] = 0x8000;
    var openFloor = new RoomLevelData(
        width,
        4,
        openForeground,
        new byte[openForeground.Length],
        new ushort[openForeground.Length],
        new byte[8]);

    // A clear prospective run really moves one pixel; it is not merely a collision query.
    var armPump = new SamusState
    {
        Pose = SamusState.FacingRightNormalPose,
        XPosition = 80,
        YPosition = 27,
    };
    armPump.Kinematics.XRadius = 5;
    armPump.Kinematics.YRadius = 5;
    byte? clearResult = armPump.CheckProspectiveRunningPoseForWall(
        bus,
        openFloor,
        SamusState.MovingRightNormalPose,
        currentXSpeedKilledByBlock: false,
        out BlockMoveResult? clearProbe);
    AssertTrue(clearResult is null, "clear arm-pump probe keeps prospective run");
    AssertTrue(clearProbe is { Collided: false }, "clear arm-pump probe reports no wall");
    AssertEqual((ushort)81, armPump.XPosition, "clear arm-pump probe retains one-pixel move");

    // Put a two-block-high wall immediately at X=96. Center 91/radius five has a current
    // right boundary at 95; the same +1.0000 request advances the sampled boundary to 96.
    var blockedForeground = (ushort[])openForeground.Clone();
    blockedForeground[6] = 0x8000;
    blockedForeground[width + 6] = 0x8000;
    var blockedFloor = new RoomLevelData(
        width,
        4,
        blockedForeground,
        new byte[blockedForeground.Length],
        new ushort[blockedForeground.Length],
        new byte[8]);
    var blocked = new SamusState
    {
        Pose = SamusState.FacingRightNormalPose,
        XPosition = 91,
        YPosition = 27,
    };
    blocked.Kinematics.XRadius = 5;
    blocked.Kinematics.YRadius = 5;
    byte? blockedResult = blocked.CheckProspectiveRunningPoseForWall(
        bus,
        blockedFloor,
        SamusState.MovingRightNormalPose,
        currentXSpeedKilledByBlock: false,
        out BlockMoveResult? blockedProbe);
    AssertEqual((byte?)SamusState.RanIntoWallRightPose, blockedResult,
        "blocked prospective run selects $89");
    AssertTrue(blockedProbe is { Collided: true }, "blocked arm-pump probe reports wall");
    AssertEqual((ushort)91, blocked.XPosition, "blocked arm-pump probe retains last-safe X");

    // A killed type-one move uses the CURRENT shot direction and performs no second probe.
    var killed = new SamusState { Pose = SamusState.MovingRightNormalPose };
    byte? killedResult = killed.CheckProspectiveRunningPoseForWall(
        bus,
        openFloor,
        prospectivePose: null,
        currentXSpeedKilledByBlock: true,
        out BlockMoveResult? killedProbe);
    AssertEqual((byte?)SamusState.RanIntoWallRightPose, killedResult,
        "killed running speed selects current wall pose");
    AssertTrue(killedProbe is null, "killed running speed skips one-pixel probe");

    // Every type-$15 pose executes no-base X, the shared grounding probe, and then clears
    // all five horizontal momentum words unconditionally.
    foreach ((byte pose, _) in wallPoses)
    {
        var stopped = new SamusState { Pose = pose, XPosition = 80, YPosition = 27 };
        stopped.Kinematics.XRadius = 5;
        stopped.Kinematics.YRadius = 5;
        stopped.HorizontalSpeed.BaseSpeed = 1;
        stopped.HorizontalSpeed.BaseSubspeed = 0x4000;
        GroundedMovementResult movement = SamusGroundedMovement.StepRanIntoWall(
            bus,
            openFloor,
            stopped,
            nmiFrameCounter: 0);
        AssertTrue(movement.Vertical.Collided, $"wall pose ${pose:X2} remains grounded");
        AssertEqual(0u, stopped.HorizontalSpeed.BaseFixed,
            $"wall pose ${pose:X2} clears base speed");
        AssertEqual((ushort)0, stopped.HorizontalSpeed.AccelerationMode,
            $"wall pose ${pose:X2} clears acceleration mode");
    }

    Console.WriteLine("  Ran into wall: ten-way selector, arm-pump pixel, six stable poses, grounding, and cleanup agree.");
}

/// <summary>
/// Sends a synthetic planar tile through DMA, OAM, OBSEL, and CGRAM so this tests the
/// complete sprite-to-pixel path rather than a helper decoder in isolation.
/// </summary>
static void VerifyObjRendering()
{
    var bus = new TestAddressSpace();
    var vram = new SnesVram();
    var cgram = new SnesCgram();
    var oam = new OamBuffer();

    // A single palette-index-1 pixel at the tile's upper-left corner. SNES 4-bpp plane 0
    // uses bit 7 of byte 0 for (0,0); every omitted byte reads as zero in this fixture.
    bus.WriteByte(0x828000, 0x80);
    vram.ExecuteQueuedWrite(bus, 0x828000, sizeInBytes: 32, encodedDestination: 0x0000);

    // One small tile-zero OBJ at (10,20), with caller-selected palette 2. Palette index
    // 128 + 2*16 + 1 is loaded with maximum red in native BGR555.
    bus.WriteBytes(0x818000, [0x01, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00]);
    cgram.SetColor(128 + 2 * 16 + 1, 0x001f);
    oam.BeginFrame();
    oam.AddOnScreenSpritemap(bus, 0x818000, originX: 10, originY: 20, paletteBits: 0x0400);
    oam.FinalizeFrame();

    var pixels = SnesObjRenderer.Render(oam, vram, cgram, obsel: 0);
    AssertEqual(new SuperMetroid.Core.Assets.Rgba32(255, 0, 0), pixels[20 * 256 + 10], "OBJ colored planar pixel");
    AssertEqual(new SuperMetroid.Core.Assets.Rgba32(0, 0, 0, 0), pixels[20 * 256 + 11], "OBJ color zero transparency");

    // The production timer's first tile is $1E0 under OBSEL=$03. This assertion fixes
    // the important reverse-engineered relationship: character data starts at word $7E00.
    AssertEqual(0xfc00, SnesObjRenderer.ResolveTileByteAddress(0x1e0, 0x03), "timer tile $1E0 VRAM byte address");
    AssertEqual(0xff00, SnesObjRenderer.ResolveTileByteAddress(0x1f8, 0x03), "timer tile $1F8 VRAM byte address");

    Console.WriteLine("  OBJ: OBSEL addressing, planar pixels, CGRAM, and transparency agree.");
}

/// <summary>
/// Verifies the HUD ROM-template mutation, authentic WRAM queue source, 2-bpp tilemap
/// interpretation, palette selection, and final pixel without relying on the private ROM.
/// </summary>
static void VerifyHudStateAndBg3Rendering()
{
    var bus = new TestAddressSpace();

    // Seed the three-row ROM template entirely with the canonical blank HUD tile $2C0F.
    // The synthetic digit table uses conspicuous character names $100-$109.
    for (int tile = 0; tile < HudState.MutableTileCount; tile++)
        WriteTestWord(bus, 0x8098cb + tile * 2, 0x2c0f);
    for (int digit = 0; digit < 10; digit++)
    {
        WriteTestWord(bus, 0x809dbf + digit * 2, (ushort)(0x2c00 | (0x100 + digit)));
        WriteTestWord(bus, 0x809dd3 + digit * 2, (ushort)(0x2c00 | (0x100 + digit)));
    }

    var hud = new HudState();
    hud.Initialize(bus, HudSnapshot.CeresDebug);
    AssertEqual((ushort)0x2d09, hud.Tiles[0x8c / 2], "HUD health tens digit from ROM table");
    AssertEqual((ushort)0x2d09, hud.Tiles[0x8e / 2], "HUD health ones digit from ROM table");

    // Area zero points to a synthetic two-screen-wide Crateria map. Give every map tile a
    // character equal to its SNES-layout index and mark every coordinate as existing; the
    // expected HUD words then prove room origin + Samus screen coordinates, 5x3 centering,
    // exploration palette, and the map-station/unexplored palette independently.
    bus.WriteByte(0x82964a, 0x00);
    bus.WriteByte(0x82964b, 0x80);
    bus.WriteByte(0x82964c, 0xb5);
    WriteTestWord(bus, 0x829717, 0x9000);
    for (int index = 0; index < 0x100; index++)
        bus.WriteByte(0x829000 + index, 0xff);
    for (int index = 0; index < 0x800; index++)
        WriteTestWord(bus, 0xb58000 + index * 2, (ushort)(index & 0x03ff));

    hud.UpdateMinimap(
        bus,
        areaIndex: 0,
        roomMapX: 0x17,
        roomMapY: 0,
        roomWidthInBlocks: 9 * 16,
        roomHeightInBlocks: 5 * 16,
        samusX: 0x0440,
        samusY: 0x04bb,
        nmiFrameCounter: 8,
        hasAreaMap: true);
    AssertEqual((byte)27, hud.MinimapCenterX, "minimap Landing Site absolute X");
    AssertEqual((byte)5, hud.MinimapCenterY, "minimap Landing Site absolute Y");
    AssertEqual((ushort)0x2c99, hud.Tiles[26], "minimap top-left unvisited map-station tile");
    AssertEqual((ushort)0x28bb, hud.Tiles[60], "minimap explored center tile");

    hud.UpdateMinimap(
        bus,
        areaIndex: 0,
        roomMapX: 0x17,
        roomMapY: 0,
        roomWidthInBlocks: 9 * 16,
        roomHeightInBlocks: 5 * 16,
        samusX: 0x0440,
        samusY: 0x04bb,
        nmiFrameCounter: 0,
        hasAreaMap: false);
    AssertEqual((ushort)0x3cbb, hud.Tiles[60], "minimap blinking center palette");
    AssertEqual((ushort)0x2c1f, hud.Tiles[26], "minimap hides unvisited tile without map station");

    var queue = new VramWriteQueue();
    var vram = new SnesVram();
    hud.QueueUpload(bus, queue);
    AssertEqual(HudState.MutableByteCount, (int)queue.Entries[0].SizeInBytes, "HUD queue transfer size");
    AssertEqual(HudState.WorkRamAddress, queue.Entries[0].SourceAddress, "HUD queue WRAM source");
    AssertEqual(HudState.VramDestination, queue.Entries[0].EncodedVramDestination, "HUD queue VRAM destination");
    queue.DrainTo(vram, bus);
    AssertEqual((byte)0x0f, vram.ReadByte(HudState.VramDestination * 2), "HUD first tilemap low byte reaches VRAM");
    AssertEqual((byte)0x2c, vram.ReadByte(HudState.VramDestination * 2 + 1), "HUD first tilemap high byte reaches VRAM");

    // Isolate the BG3 renderer at a harmless tilemap base. Entry tile 2 / palette 1 points
    // at a tile whose upper-left plane-0 bit is set; CGRAM 5 is maximum green.
    bus.WriteBytes(0x818000, [0x02, 0x04]);
    vram.ExecuteQueuedWrite(bus, 0x818000, 2, encodedDestination: 0x0100);
    bus.WriteByte(0x828000, 0x80);
    vram.ExecuteQueuedWrite(bus, 0x828000, 16, encodedDestination: 0x0010);
    var cgram = new SnesCgram();
    cgram.SetColor(5, 0x03e0);
    var pixels = SnesBgTilemapRenderer.Render2Bpp(vram, cgram, tilemapBaseWord: 0x0100, characterBaseWord: 0, rowCount: 1);
    AssertEqual(new SuperMetroid.Core.Assets.Rgba32(0, 255, 0), pixels[0], "BG3 tile/palette pixel");

    Console.WriteLine("  HUD: inventory, live minimap coordinates/blink, WRAM upload, and BG3 pixels agree.");
}

static void WriteTestWord(TestAddressSpace bus, int address, ushort value)
{
    bus.WriteByte(address, (byte)value);
    bus.WriteByte(address + 1, (byte)(value >> 8));
}

static void SeedMotherBrainWalkProgram(
    TestAddressSpace bus,
    ushort listAddress,
    ushort duration,
    bool forward)
{
    // All four speed variants share command topology; only each visible frame duration
    // differs. These words are transcribed from `$A9:9730-$9972`, with harmless synthetic
    // spritemap operands because movement verification never draws the extended map.
    ushort[] commands = forward
        ? [0x95fc, 0x960c, 0x961c, 0x9622, 0x9638, 0x9648, 0x9658, 0x9668]
        : [0x96f0, 0x96e0, 0x96d0, 0x96ba, 0x96aa, 0x96a4, 0x9694, 0x967e];
    var words = new List<ushort> { 0x9708, duration, 0x1000 };
    for (int command = 0; command < commands.Length; command++)
    {
        words.Add(commands[command]);
        if (command == commands.Length - 1)
            words.Add(0x9700);
        words.Add(duration);
        words.Add(unchecked((ushort)(0x1001 + command)));
    }
    words.Add(0x812f);
    for (int index = 0; index < words.Count; index++)
        WriteTestWord(bus, 0xa90000 | unchecked((ushort)(listAddress + index * 2)), words[index]);
}

static void SeedMotherBrainCrouchFastProgram(TestAddressSpace bus)
{
    ushort[] words =
    [
        0x9718, 0x0008, 0x1200,
        0x95de, 0x0002, 0x1201,
        0x95e8, 0x0002, 0x1202,
        0x95f2, 0x9710, 0x0008, 0x1203,
        0x812f,
    ];
    for (int index = 0; index < words.Length; index++)
        WriteTestWord(bus, 0xa99a26 + index * 2, words[index]);
}

static void SeedBabyCeilingToSamusRoute(TestAddressSpace bus)
{
    // Exact `$A9:CA24-$CA65` words. Records are eight bytes even though the AI reads a
    // fifth word at +8: for records zero through six that read aliases the following X
    // target, while final record `$CA5C` aliases `$CA64`'s negative `$CA66` function.
    ushort[] words =
    [
        0x00a0, 0x0078, 0x0000, 0xf466,
        0x0130, 0x007a, 0x0000, 0xf466,
        0x00c0, 0x0040, 0x0000, 0xf466,
        0x00c0, 0x0070, 0x0000, 0xf466,
        0x00e0, 0x0080, 0x0000, 0xf466,
        0x00cd, 0x0090, 0x0000, 0xf45f,
        0x00cc, 0x00a0, 0x0000, 0xf45f,
        0x00cb, 0x00b0, 0x0000, 0xf45f,
        0xca66,
    ];
    for (int index = 0; index < words.Length; index++)
        WriteTestWord(bus, 0xa9ca24 + index * 2, words[index]);
}

/// <summary>Checks both edges and relative tile-step movement of the temporary host camera.</summary>
static void VerifyDebugRoomCamera()
{
    var camera = new DebugRoomCamera(roomWidth: 1024, roomHeight: 512, viewportWidth: 256, viewportHeight: 192);
    camera.MoveTo(-50, -20);
    AssertEqual(0, camera.X, "debug camera clamps negative X");
    AssertEqual(0, camera.Y, "debug camera clamps negative Y");

    camera.MoveTo(5000, 5000);
    AssertEqual(768, camera.X, "debug camera clamps right edge");
    AssertEqual(320, camera.Y, "debug camera clamps bottom edge");

    camera.MoveBy(-16, -16);
    AssertEqual(752, camera.X, "debug camera relative block X");
    AssertEqual(304, camera.Y, "debug camera relative block Y");
    Console.WriteLine("  Camera: host viewport movement and room-edge clamps agree.");
}

/// <summary>
/// Exercises the exact 50-byte room loader plus all four directional bank-$80 handlers at
/// internal red boundaries and physical room edges.
/// </summary>
static void VerifyRoomScrollGridAndBoundaryCamera()
{
    var bus = new TestAddressSpace();
    const int source = 0x808000;
    for (int index = 0; index < RoomScrollGrid.StorageByteCount; index++)
        bus.WriteByte(source + index, 1);

    // The logical 3x2 grid begins fully blue. Give the five copied padding bytes distinct
    // values to prove LoadExplicit does not synthesize zeroes after the sixth logical cell.
    for (int index = 6; index < RoomScrollGrid.StorageByteCount; index++)
        bus.WriteByte(source + index, (byte)(0x80 + index));

    RoomScrollGrid grid = RoomScrollGrid.LoadExplicit(bus, source, widthInScreens: 3, heightInScreens: 2);
    AssertEqual((byte)0x86, grid.Storage[6], "scroll loader retains first nonlogical byte");
    AssertEqual((byte)0xb1, grid.Storage[49], "scroll loader retains fiftieth byte");
    AssertEqual((byte)0x86, bus.ReadByte(RoomScrollGrid.WorkRamAddress + 6), "scroll loader mirrors WRAM padding");

    var camera = new ScrollBoundaryCamera(grid);

    // Right: the screen to the right is red, so the attempted +16 is rejected and the
    // routine's deliberate extra two-pixel retreat clamps the signed underflow to zero.
    grid.SetLogicalCell(1, 0, 0);
    camera.SetPosition(0, 0);
    camera.MoveRight(16);
    AssertEqual((ushort)0, camera.XPosition, "$80:A641 red boundary moving right");

    // Left: entering a red current cell from its right edge produces the symmetric +2
    // retreat described by $80:A719-$80:A72B.
    camera.SetPosition(272, 0);
    camera.MoveLeft(16);
    AssertEqual((ushort)274, camera.XPosition, "$80:A6BB red boundary moving left");

    // Down: a blue current cell over a red lower cell rejects the move and retreats two
    // pixels above the pre-move position (224 -> proposed 240 -> result 222).
    grid.SetLogicalCell(0, 0, 1);
    grid.SetLogicalCell(0, 1, 0);
    camera.SetPosition(0, 224);
    camera.MoveDown(16);
    AssertEqual((ushort)222, camera.YPosition, "$80:A893 red boundary moving down");

    // Up: starting inside that red lower cell gives the corresponding two-pixel retreat.
    camera.SetPosition(0, 272);
    camera.MoveUp(16);
    AssertEqual((ushort)274, camera.YPosition, "$80:A936 red boundary moving up");

    // Physical right edge is (width-1)*$100 regardless of the scroll padding bytes.
    grid.SetLogicalCell(1, 0, 1);
    camera.SetPosition(0x01f8, 0);
    camera.MoveRight(16);
    AssertEqual((ushort)0x0200, camera.XPosition, "scroll camera physical room maximum");

    Console.WriteLine("  Scrolls: 50-byte load and four directional boundary handlers agree.");
}

/// <summary>
/// Checks bank-$90 moved-axis target selection and 16.16 "distance + 1" arithmetic before
/// those results enter the already-verified bank-$80 boundary handlers.
/// </summary>
static void VerifyMovedSamusCameraTracking()
{
    var bus = new TestAddressSpace();
    const int source = 0x818000;
    for (int index = 0; index < RoomScrollGrid.StorageByteCount; index++)
        bus.WriteByte(source + index, 1);
    RoomScrollGrid grid = RoomScrollGrid.LoadExplicit(bus, source, widthInScreens: 3, heightInScreens: 2);
    var camera = new ScrollBoundaryCamera(grid);

    // Facing right, normal forward movement, distance slot zero targets Samus X-$60.
    // Samus moved four pixels, and $90:96C0 intentionally adds 1.0, so camera advances 5.
    camera.SetPosition(100, 0);
    var previous = new SamusCameraPoint(200, 0, 100, 0);
    var current = new SamusCameraPoint(204, 0, 100, 0);
    camera.TrackMovedSamusHorizontally(
        previous,
        current,
        new HorizontalCameraContext(0, MovementType: 0, XAccelerationMode: 0, PoseXDirection: 8, CameraDistanceIndex: 0));
    AssertEqual((ushort)108, camera.IdealXPosition, "camera facing-right ideal X");
    AssertEqual((ushort)5, camera.CameraXSpeed, "camera X distance includes one pixel");
    AssertEqual((ushort)105, camera.XPosition, "camera X follows by calculated speed");

    // The complete fixed-point difference matters: 200.8000 -> 201.4000 is +0.C000;
    // adding 1.0 yields a camera delta of 1.C000.
    camera.SetPosition(100, 0);
    previous = previous with { XPosition = 200, XSubposition = 0x8000 };
    current = current with { XPosition = 201, XSubposition = 0x4000 };
    camera.TrackMovedSamusHorizontally(
        previous,
        current,
        new HorizontalCameraContext(0, 0, 0, PoseXDirection: 8, CameraDistanceIndex: 0));
    AssertEqual((ushort)1, camera.CameraXSpeed, "camera fixed X speed integer");
    AssertEqual((ushort)0xc000, camera.CameraXSubspeed, "camera fixed X speed fraction");
    AssertEqual((ushort)101, camera.XPosition, "camera fixed X position integer");
    AssertEqual((ushort)0xc000, camera.XSubposition, "camera fixed X position fraction");

    // Downward Samus movement uses up_scroller. 210-$64 gives ideal 110; a two-pixel move
    // becomes camera speed three and advances layer Y from 100 to 103 without overshoot.
    camera.SetPosition(0, 100);
    previous = new SamusCameraPoint(0, 0, 208, 0);
    current = new SamusCameraPoint(0, 0, 210, 0);
    camera.TrackMovedSamusVertically(
        previous,
        current,
        new VerticalCameraContext(YDirection: 2, UpScroller: 100, DownScroller: 112));
    AssertEqual((ushort)110, camera.IdealYPosition, "camera downward ideal Y");
    AssertEqual((ushort)3, camera.CameraYSpeed, "camera Y distance includes one pixel");
    AssertEqual((ushort)103, camera.YPosition, "camera Y follows by calculated speed");

    // Equal integer coordinates take $80:A528/$80:A731. With identical fixed-point
    // samples, the bank-$90 distance routine still produces speed 1; autoscroll then adds
    // its own two pixels. A red current cell with a blue neighbor therefore drifts +3.
    grid.SetLogicalCell(0, 0, 0);
    grid.SetLogicalCell(1, 0, 1);
    camera.SetPosition(0x0020, 0);
    var stationary = new SamusCameraPoint(200, 0, 200, 0);
    camera.TrackMovedSamusHorizontally(
        stationary,
        stationary,
        new HorizontalCameraContext(0, 0, 0, PoseXDirection: 8, CameraDistanceIndex: 0));
    AssertEqual((ushort)1, camera.CameraXSpeed, "stationary camera X speed still includes one");
    AssertEqual((ushort)0x0023, camera.XPosition, "$80:A528 red-cell rightward drift");

    // Red on both sides cancels that drift by rounding back to the current screen edge.
    grid.SetLogicalCell(1, 0, 0);
    camera.SetPosition(0x0020, 0);
    camera.TrackMovedSamusHorizontally(
        stationary,
        stationary,
        new HorizontalCameraContext(0, 0, 0, PoseXDirection: 8, CameraDistanceIndex: 0));
    AssertEqual((ushort)0, camera.XPosition, "$80:A528 adjacent-red horizontal rounding");

    // The time-frozen flag short-circuits autoscrolling after bank $90 has calculated the
    // speed. It is intentionally not a blanket prohibition on moved-axis handling.
    grid.SetLogicalCell(1, 0, 1);
    camera.SetPosition(0x0020, 0);
    camera.TrackMovedSamusHorizontally(
        stationary,
        stationary,
        new HorizontalCameraContext(0, 0, 0, PoseXDirection: 8, CameraDistanceIndex: 0),
        timeIsFrozen: true);
    AssertEqual((ushort)0x0020, camera.XPosition, "$80:A528 time-frozen return");

    // A 9x5 room at its padded bottom can select scroll index 50 after centered camera X
    // advances into screen five. Native WRAM continues into ExploredMapTiles at $CD52;
    // a zero byte there makes the blue bottom row clamp to Y=$0400 without an array error.
    var bottomEdgeBus = new TestAddressSpace();
    for (int index = 0; index < RoomScrollGrid.StorageByteCount; index++)
        bottomEdgeBus.WriteByte(source + index, 1);
    RoomScrollGrid bottomEdgeGrid = RoomScrollGrid.LoadExplicit(
        bottomEdgeBus,
        source,
        widthInScreens: 9,
        heightInScreens: 5);
    var bottomEdgeCamera = new ScrollBoundaryCamera(bottomEdgeGrid);
    bottomEdgeCamera.SetPosition(0x0492, 0x0415);
    bottomEdgeCamera.TrackMovedSamusVertically(
        stationary,
        stationary,
        new VerticalCameraContext(YDirection: 0, UpScroller: 0x70, DownScroller: 0xa0));
    AssertEqual((ushort)0x0400, bottomEdgeCamera.YPosition, "$80:A731 index-$32 adjacent-WRAM bottom clamp");

    // Vertical autoscrolling uses the centered X cell and the same speed+2 drift. This
    // three-row grid leaves the cell below blue so the candidate remains unrounded.
    var verticalBus = new TestAddressSpace();
    for (int index = 0; index < RoomScrollGrid.StorageByteCount; index++)
        verticalBus.WriteByte(source + index, 1);
    RoomScrollGrid verticalGrid = RoomScrollGrid.LoadExplicit(
        verticalBus,
        source,
        widthInScreens: 3,
        heightInScreens: 3);
    verticalGrid.SetLogicalCell(0, 0, 0);
    verticalGrid.SetLogicalCell(0, 1, 1);
    var verticalCamera = new ScrollBoundaryCamera(verticalGrid);
    verticalCamera.SetPosition(0, 0x0020);
    verticalCamera.TrackMovedSamusVertically(
        stationary,
        stationary,
        new VerticalCameraContext(YDirection: 0, UpScroller: 0, DownScroller: 0));
    AssertEqual((ushort)1, verticalCamera.CameraYSpeed, "stationary camera Y speed still includes one");
    AssertEqual((ushort)0x0023, verticalCamera.YPosition, "$80:A731 red-cell downward drift");

    Console.WriteLine("  Camera: bank $90 tracking and bank $80 stationary autoscroll agree.");
}

/// <summary>
/// Checks the exact parallax multiply, scroll-register wrapping, signed block conversion,
/// and row/column selection at $80:A2F9-$80:A527.
/// </summary>
static void VerifyBackgroundScrollState()
{
    var state = new BackgroundScrollState
    {
        Layer1XPosition = 0x1234,
        Layer1YPosition = 0x0200,
        Layer2ScrollX = 0x80, // 128/256 = one-half parallax.
        Layer2ScrollY = 0,
        Bg1XOffset = 0x0010,
        Bg1YOffset = 0xfff0,
        Bg2XOffset = 3,
        Bg2YOffset = 4,
    };

    state.PrimePreviousBlocks();
    IReadOnlyList<BackgroundUpdateRequest> requests = state.StepScrolling();
    AssertEqual((ushort)0x1244, state.Bg1HorizontalScroll, "$80:A3B7 BG1 X plus offset");
    AssertEqual((ushort)0x01f0, state.Bg1VerticalScroll, "$80:A3C0 BG1 Y wrapping offset");
    AssertEqual((ushort)0x091a, state.Layer2XPosition, "$80:A2F9 half-speed X parallax");
    AssertEqual((ushort)0x0200, state.Layer2YPosition, "$80:A33A zero mode copies layer 1");
    AssertEqual((ushort)0x091d, state.Bg2HorizontalScroll, "$80:A3CF BG2 X plus offset");
    AssertEqual(0, requests.Count, "primed scrolling emits no unchanged block updates");

    // Crossing one 16-pixel boundary right/down creates requests in native order: level
    // column, background column, level row, background row. Coordinate offsets are the
    // literal +$10 and +$0F selected by the assembly.
    state.Layer1XPosition = 0x1244;
    state.Layer1YPosition = 0x0210;
    requests = state.StepScrolling();
    AssertEqual(4, requests.Count, "four scrolling update requests");
    AssertEqual(
        new BackgroundUpdateRequest(BackgroundLayer.Level, BackgroundUpdateAxis.Column, 0x0134, 0x0021, 0x0135, 0x0020),
        requests[0],
        "rightward level column request");
    AssertEqual(BackgroundLayer.Background, requests[1].Layer, "second request is BG2 column");
    AssertEqual(BackgroundUpdateAxis.Column, requests[1].Axis, "second request axis");
    AssertEqual(
        new BackgroundUpdateRequest(BackgroundLayer.Level, BackgroundUpdateAxis.Row, 0x0124, 0x0030, 0x0125, 0x002f),
        requests[2],
        "downward level row request");
    AssertEqual(BackgroundLayer.Background, requests[3].Layer, "fourth request is BG2 row");
    AssertEqual(BackgroundUpdateAxis.Row, requests[3].Axis, "fourth request axis");

    // Mode one preserves both the layer-2 position and its PPU scroll mirror, and its odd
    // low bit suppresses both BG2 stream directions.
    ushort oldLayer2X = state.Layer2XPosition;
    ushort oldBg2X = state.Bg2HorizontalScroll;
    state.Layer2ScrollX = 1;
    state.Layer2ScrollY = 1;
    state.Layer1XPosition = 0x1300;
    state.Layer1YPosition = 0x0300;
    requests = state.StepScrolling();
    AssertEqual(oldLayer2X, state.Layer2XPosition, "fixed BG2 X position remains unchanged");
    AssertEqual(oldBg2X, state.Bg2HorizontalScroll, "fixed BG2 X register remains unchanged");
    AssertEqual(2, requests.Count, "fixed BG2 modes emit only level updates");

    // World coordinates are sign-extended after division by 16, whereas PPU registers are
    // always logically shifted. $FFF0 therefore means block -1 ($FFFF), not $0FFF.
    state.Layer1XPosition = 0xfff0;
    state.PrimePreviousBlocks();
    AssertEqual((ushort)0xffff, state.Layer1XBlock, "$80:A4CD signed layer block");

    ushort frozenBg1X = state.Bg1HorizontalScroll;
    state.Layer1XPosition = 0x4444;
    requests = state.StepScrolling(timeIsFrozen: true);
    AssertEqual(frozenBg1X, state.Bg1HorizontalScroll, "$80:A3AB frozen scroll registers");
    AssertEqual(0, requests.Count, "$80:A3AB frozen update list");

    Console.WriteLine("  BG scroll: parallax, signed blocks, and row/column dispatch agree.");
}

/// <summary>Checks all four branches shared by $80:AA95 and $80:AC57.</summary>
static void VerifyLevelBlockTilemapExpansion()
{
    // Put the definition at index one so the test also exercises the level entry's ten-bit
    // lookup. Existing child flip/palette bits are retained and XORed, never reconstructed.
    var definitions = new byte[16];
    BinaryPrimitives.WriteUInt16LittleEndian(definitions.AsSpan(8), 0x0123);
    BinaryPrimitives.WriteUInt16LittleEndian(definitions.AsSpan(10), 0x4567);
    BinaryPrimitives.WriteUInt16LittleEndian(definitions.AsSpan(12), 0x89ab);
    BinaryPrimitives.WriteUInt16LittleEndian(definitions.AsSpan(14), 0xcdef);

    AssertEqual(
        new ExpandedBlockTiles(0x0123, 0x4567, 0x89ab, 0xcdef),
        LevelBlockTilemapExpander.Expand(0x0001, definitions),
        "unflipped block expansion");
    AssertEqual(
        new ExpandedBlockTiles(0x0567, 0x4123, 0x8def, 0xc9ab),
        LevelBlockTilemapExpander.Expand(0x0401, definitions),
        "horizontally flipped block expansion");
    AssertEqual(
        new ExpandedBlockTiles(0x09ab, 0x4def, 0x8123, 0xc567),
        LevelBlockTilemapExpander.Expand(0x0801, definitions),
        "vertically flipped block expansion");
    AssertEqual(
        new ExpandedBlockTiles(0x0def, 0x49ab, 0x8567, 0xc123),
        LevelBlockTilemapExpander.Expand(0x0c01, definitions),
        "doubly flipped block expansion");

    AssertThrows<InvalidDataException>(
        () => LevelBlockTilemapExpander.Expand(0x0002, definitions),
        "block definition bounds check");

    Console.WriteLine("  BG stream: all 16x16 block flip expansions agree.");
}

/// <summary>
/// Verifies the shared row-major BG1/BTS model that will feed bank-$94 collision while
/// continuing to construct the already-verified bank-$80 visual streamer.
/// </summary>
static void VerifyRoomLevelData()
{
    const int width = 3;
    const int height = 2;
    ushort[] foreground = [
        0x0000, 0x1123, 0x8567,
        0xc001, 0xe002, 0xf003,
    ];
    byte[] behavior = [0x00, 0x45, 0x80, 0x11, 0x22, 0x33];
    ushort[] background = [0, 1, 2, 3, 4, 5];
    var definitions = new byte[8 * 4];
    var level = new RoomLevelData(width, height, foreground, behavior, background, definitions);

    RoomCollisionBlock block = level.GetCollisionBlock(blockX: 1, blockY: 1);
    AssertEqual(4, block.Index, "room collision row-major index");
    AssertEqual((ushort)0xe002, block.LevelWord, "room collision level word");
    AssertEqual((byte)0x22, block.Behavior, "room collision parallel BTS byte");
    AssertEqual((byte)0x0e, block.CollisionType, "room collision high-nibble dispatcher type");
    AssertEqual((ushort)2, block.VisualBlockIndex, "room collision visual block index");

    // Pixel (31,17) is block (1,1); shifts must occur before multiplication/indexing.
    AssertEqual(block, level.GetCollisionBlockAtPixel(31, 17), "room pixel-to-block conversion");
    AssertThrows<ArgumentOutOfRangeException>(
        () => level.GetCollisionBlock(width, 0),
        "room collision rejects X beyond header width");
    AssertThrows<ArgumentOutOfRangeException>(
        () => level.GetCollisionBlock(0, height),
        "room collision rejects Y beyond header height");

    BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
    AssertTrue(streamer is not null, "room level constructs visual streamer from same allocation");

    Console.WriteLine("  Room level: shared BG1/BTS indexing, collision type, and pixel conversion agree.");
}

/// <summary>Checks $80:A9DE-$80:AD17 staging geometry and $80:8CD8 NMI destinations.</summary>
static void VerifyBackgroundTilemapStreamer()
{
    const int width = 32;
    var level = new ushort[width * 32];
    var background = new ushort[level.Length];
    var definitions = new byte[8];
    BinaryPrimitives.WriteUInt16LittleEndian(definitions.AsSpan(0), 0x0001);
    BinaryPrimitives.WriteUInt16LittleEndian(definitions.AsSpan(2), 0x0002);
    BinaryPrimitives.WriteUInt16LittleEndian(definitions.AsSpan(4), 0x0003);
    BinaryPrimitives.WriteUInt16LittleEndian(definitions.AsSpan(6), 0x0004);
    var streamer = new BackgroundTilemapStreamer(width, level, background, definitions);

    // X block $11 selects the second BG1 screen base ($53E0); Y block 5 splits a column
    // into 22 unwrapped words and 10 wrapped words for each of its left/right halves.
    var columnRequest = new BackgroundUpdateRequest(
        BackgroundLayer.Level,
        BackgroundUpdateAxis.Column,
        SourceXBlock: 2,
        SourceYBlock: 3,
        VramXBlock: 0x11,
        VramYBlock: 5);
    TilemapStreamUpdate column = streamer.Build(columnRequest)!
        ?? throw new InvalidOperationException("Non-Mode-7 column was incorrectly skipped.");
    AssertEqual(32, column.FirstHalves.Length, "column left staging words");
    AssertEqual((ushort)0x0001, column.FirstHalves[0], "column top-left tile");
    AssertEqual((ushort)0x0003, column.FirstHalves[1], "column bottom-left tile");
    AssertEqual(4, column.Segments.Count, "wrapped column DMA count");
    AssertEqual(22, column.Segments[0].WordCount, "column unwrapped word count");
    AssertEqual(10, column.Segments[2].WordCount, "column wrapped word count");
    AssertEqual((ushort)0x5542, column.Segments[0].VramWordDestination, "column unwrapped destination");
    AssertEqual((ushort)0x5402, column.Segments[2].VramWordDestination, "column wrapped destination");
    AssertEqual(TilemapDmaDirection.Column, column.Segments[0].Direction, "column VMAIN mode");
    var streamedVram = new SnesVram();
    column.ExecuteTo(streamedVram);
    AssertEqual((ushort)0x0001, streamedVram.ReadWord(0x5542), "column DMA left top word");
    AssertEqual((ushort)0x0002, streamedVram.ReadWord(0x5543), "column DMA right top word");
    AssertEqual((ushort)0x0003, streamedVram.ReadWord(0x5562), "column DMA left bottom word");

    // X within-screen 5 yields 22 unwrapped and 12 wrapped row words. The bottom half uses
    // the same destination with bit $20 set, exactly as the NMI routine does.
    var rowRequest = new BackgroundUpdateRequest(
        BackgroundLayer.Level,
        BackgroundUpdateAxis.Row,
        SourceXBlock: 2,
        SourceYBlock: 3,
        VramXBlock: 5,
        VramYBlock: 6);
    TilemapStreamUpdate row = streamer.Build(rowRequest)!
        ?? throw new InvalidOperationException("Non-Mode-7 row was incorrectly skipped.");
    AssertEqual(34, row.FirstHalves.Length, "row top staging words");
    AssertEqual((ushort)0x0001, row.FirstHalves[0], "row top-left tile");
    AssertEqual((ushort)0x0002, row.FirstHalves[1], "row top-right tile");
    AssertEqual(22, row.Segments[0].WordCount, "row unwrapped word count");
    AssertEqual(12, row.Segments[2].WordCount, "row wrapped word count");
    AssertEqual((ushort)0x518a, row.Segments[0].VramWordDestination, "row unwrapped destination");
    AssertEqual((ushort)0x51aa, row.Segments[1].VramWordDestination, "row bottom destination");
    AssertEqual((ushort)0x5580, row.Segments[2].VramWordDestination, "row wrapped destination");
    AssertEqual(TilemapDmaDirection.Row, row.Segments[0].Direction, "row VMAIN mode");
    streamedVram.Clear();
    row.ExecuteTo(streamedVram);
    AssertEqual((ushort)0x0001, streamedVram.ReadWord(0x518a), "row DMA top-left word");
    AssertEqual((ushort)0x0002, streamedVram.ReadWord(0x518b), "row DMA top-right word");
    AssertEqual((ushort)0x0003, streamedVram.ReadWord(0x51aa), "row DMA bottom-left word");

    AssertEqual<TilemapStreamUpdate?>(null, streamer.Build(rowRequest, mode7Enabled: true), "$80:AB78 Mode 7 return");
    Console.WriteLine("  BG stream: row/column staging splits and NMI destinations agree.");
}

/// <summary>Checks Mode-1 4-bpp BG pixels, palette selection, transparency, and H flip.</summary>
static void VerifyFourBitBackgroundRendering()
{
    var vram = new SnesVram();
    var cgram = new SnesCgram();

    // Character zero has only its top-left plane-zero bit set, producing color index one.
    var character = new byte[32];
    character[0] = 0x80;
    vram.LoadBytes(0, character);
    cgram.SetColor(2 * 16 + 1, 0x001f); // Full SNES red in BG palette two.

    ushort[] mapEntry = [0x0800]; // Character zero, palette two, no flips.
    vram.ExecuteWordTransfer(mapEntry, destinationWord: 0x5000, wordIncrement: 1);
    Rgba32[] pixels = SnesBgTilemapRenderer.Render4BppViewport(
        vram, cgram, 0x5000, 0, 0, 0, width: 8, height: 8);
    AssertEqual(new Rgba32(255, 0, 0), pixels[0], "4-bpp BG palette pixel");
    AssertEqual((byte)0, pixels[1].A, "4-bpp BG color zero transparency");

    mapEntry[0] |= 0x4000;
    vram.ExecuteWordTransfer(mapEntry, destinationWord: 0x5000, wordIncrement: 1);
    pixels = SnesBgTilemapRenderer.Render4BppViewport(
        vram, cgram, 0x5000, 0, 0, 0, width: 8, height: 1);
    AssertEqual((byte)0, pixels[0].A, "4-bpp H-flip old pixel becomes transparent");
    AssertEqual(new Rgba32(255, 0, 0), pixels[7], "4-bpp H-flip mirrored pixel");

    // BG2SC=$4A means a 32x64-tile map rooted at word $4800. Once VOFS bit eight
    // is set, the PPU selects its second $400-word screen at $4C00 rather than wrapping
    // into the first screen. Landing Site relies on this exact vertical arrangement.
    vram.ExecuteWordTransfer([0x0800], destinationWord: 0x4c00, wordIncrement: 1);
    pixels = SnesBgTilemapRenderer.Render4BppViewport(
        vram, cgram, 0x4800, 0, 0, 0x0100, width: 8, height: 1,
        tilemapWidthInTiles: 32, tilemapHeightInTiles: 64);
    AssertEqual(new Rgba32(255, 0, 0), pixels[0], "4-bpp BGSC vertical second screen");

    // The force-blank room fill is exactly 17 level columns when layer-2 X mode is odd.
    var scroll = new BackgroundScrollState
    {
        Layer1XPosition = 0x0120,
        Layer1YPosition = 0x0230,
        Layer2ScrollX = 0x81,
        Layer2ScrollY = 1,
    };
    IReadOnlyList<BackgroundUpdateRequest> initial = scroll.BuildInitialViewportRequests();
    AssertEqual(17, initial.Count, "$80:A176 initial BG1 column count");
    AssertEqual((ushort)0x0012, initial[0].SourceXBlock, "initial first source column");
    AssertEqual((ushort)0x0022, initial[16].SourceXBlock, "initial seventeenth source column");

    Console.WriteLine("  BG render: 4-bpp pixels, BGSC geometry, and 17-column initial fill agree.");
}

/// <summary>
/// Guards the physical-scanline relationship between the host-composited terrain and live
/// Samus/OAM. The gameplay IRQ hides BG1 behind the HUD; it does not rewind BG1VOFS when
/// BG1 becomes visible again on line 32.
/// </summary>
static void VerifyHostRoomViewportAlignment()
{
    const int roomWidth = 300;
    const int roomHeight = 260;
    const int cameraX = 7;
    const int cameraY = 11;
    var room = new Rgba32[roomWidth * roomHeight];

    // Give every source row a unique red component. The first gameplay output pixel must
    // come from world row cameraY+32; cameraY would expose the old 32-pixel alignment bug.
    for (int y = 0; y < roomHeight; y++)
    {
        for (int x = 0; x < roomWidth; x++)
            room[y * roomWidth + x] = new Rgba32((byte)y, (byte)x, 0);
    }

    var vram = new SnesVram();
    var cgram = new SnesCgram();
    var oam = new OamBuffer();
    Rgba32[] frame = SnesGameplayFrameRenderer.RenderHudRoomAndObjs(
        vram,
        cgram,
        oam,
        room,
        roomWidth,
        roomHeight,
        cameraX,
        cameraY);

    AssertEqual(
        room[(cameraY + SnesGameplayFrameRenderer.HudHeight) * roomWidth + cameraX],
        frame[SnesGameplayFrameRenderer.HudHeight * SnesGameplayFrameRenderer.Width],
        "host terrain begins at physical scanline 32");
    AssertEqual(
        room[(cameraY + SnesGameplayFrameRenderer.Height - 1) * roomWidth + cameraX + 255],
        frame[^1],
        "host terrain bottom-right physical coordinate");

    Console.WriteLine("  Host terrain: room crop remains aligned with physical BG1 scanlines and live OAM.");
}

/// <summary>
/// Proves that desktop power-bomb composition consumes the same bank-$88 radius frame and
/// half-profile orientation as the original indirect-HDMA window builder.
/// </summary>
static void VerifyPowerBombColorMathWindow()
{
    var bus = new TestAddressSpace();

    // These are the literal `$88:A266-$88:A2A5` horizontal samples and vertical band
    // boundaries used by `$88:8CC6/$8D04/$8D46`. Keeping the fixture independent of the
    // production loop ensures a transposed table or rounded product cannot self-validate.
    bus.WriteBytes(0x88a266, [
        0x00, 0x0c, 0x19, 0x25, 0x31, 0x3e, 0x4a, 0x56,
        0x61, 0x6d, 0x78, 0x83, 0x8e, 0x98, 0xa2, 0xab,
        0xb5, 0xbd, 0xc5, 0xcd, 0xd4, 0xdb, 0xe1, 0xe7,
        0xec, 0xf1, 0xf4, 0xf8, 0xfb, 0xfd, 0xfe, 0xff,
    ]);
    bus.WriteBytes(0x88a286, [
        0xbf, 0xbf, 0xbe, 0xbd, 0xba, 0xb8, 0xb6, 0xb2,
        0xaf, 0xab, 0xa6, 0xa2, 0x9c, 0x96, 0x90, 0x8a,
        0x84, 0x7d, 0x75, 0x6e, 0x66, 0x5e, 0x56, 0x4d,
        0x45, 0x3c, 0x33, 0x2a, 0x20, 0x17, 0x0d, 0x04,
    ]);

    // All sixteen pre-explosion color entries use the same unmistakable fixed color in
    // this fixture. The state still performs its authentic radius-derived table lookup;
    // repeating the value merely keeps these geometry assertions focused and readable.
    for (int color = 0; color < 16; color++)
        bus.WriteBytes(0x889079 + color * 3, [0x01, 0x02, 0x03]);

    const ushort centerX = 100;
    const ushort centerY = 100;
    var explosion = new SamusPowerBombExplosionState();
    explosion.Arm();
    explosion.Spawn(centerX, centerY);

    // Spawn happens after the native HDMA pass. Even though status is already `$8000`,
    // the host must not invent an explosion table on this frame.
    Rgba32[] spawnFrame = CreateOpaqueBlackGameplayFrame();
    SnesGameplayFrameRenderer.ApplyPowerBombColorMath(spawnFrame, bus, explosion, 0, 0);
    AssertEqual(new Rgba32(0, 0, 0, 255),
        spawnFrame[centerY * SnesGameplayFrameRenderer.Width + centerX],
        "power-bomb spawn frame has no premature HDMA window");

    // The first pre-instruction draws radius `$04.00`, then advances the live state to
    // `$34.00`. `$04 * $BF >> 8` gives a two-line vertical extent and `$04 * $FF >> 8`
    // gives a three-pixel center half-width. Testing beyond both extents catches use of
    // the already-updated `$34.00` radius as well as floating-point ellipse substitution.
    explosion.StepFrame(bus);
    AssertEqual((ushort)0x0400, explosion.RenderedPreExplosionRadius,
        "power-bomb renderer retains pre-update radius");
    AssertEqual((ushort)0x3400, explosion.PreExplosionRadius,
        "power-bomb logic advances next-frame radius");
    Rgba32[] scaledFrame = CreateOpaqueBlackGameplayFrame();
    SnesGameplayFrameRenderer.ApplyPowerBombColorMath(scaledFrame, bus, explosion, 0, 0);
    Rgba32 fixedColor = new(8, 16, 24, 255);
    AssertEqual(fixedColor,
        scaledFrame[centerY * SnesGameplayFrameRenderer.Width + centerX + 3],
        "scaled power-bomb center uses truncated final horizontal sample");
    AssertEqual(new Rgba32(0, 0, 0, 255),
        scaledFrame[centerY * SnesGameplayFrameRenderer.Width + centerX + 4],
        "scaled power-bomb center excludes first outside pixel");
    AssertEqual(new Rgba32(0, 0, 0, 255),
        scaledFrame[(centerY + 3) * SnesGameplayFrameRenderer.Width + centerX],
        "scaled power-bomb vertical extent uses ROM boundary table");

    // A pre-scaled record is a center-outward half-profile. Native HDMA mirrors it around
    // the explosion Y coordinate; it is not indexed by adding a screen-space midpoint.
    while (explosion.Phase == PowerBombExplosionPhase.PreExplosionWhite)
        explosion.StepFrame(bus);
    bus.WriteBytes(0x889f06, [0x07, 0x03, 0x00]);
    explosion.StepFrame(bus);
    AssertEqual(PowerBombExplosionPhase.PreExplosionYellow, explosion.RenderedPhase,
        "first pre-scaled yellow frame is retained for composition");
    Rgba32[] shapeFrame = CreateOpaqueBlackGameplayFrame();
    SnesGameplayFrameRenderer.ApplyPowerBombColorMath(shapeFrame, bus, explosion, 0, 0);
    AssertEqual(fixedColor,
        shapeFrame[centerY * SnesGameplayFrameRenderer.Width + centerX + 7],
        "pre-scaled profile byte zero draws center half-width");
    AssertEqual(fixedColor,
        shapeFrame[(centerY + 1) * SnesGameplayFrameRenderer.Width + centerX + 3],
        "pre-scaled profile byte one draws mirrored adjacent line");
    AssertEqual(new Rgba32(0, 0, 0, 255),
        shapeFrame[(centerY + 2) * SnesGameplayFrameRenderer.Width + centerX],
        "pre-scaled zero terminates shape extent");

    Console.WriteLine("  Power bomb: ROM curve bands, rendered-frame timing, and center-outward shapes agree.");
}

static Rgba32[] CreateOpaqueBlackGameplayFrame()
{
    var frame = new Rgba32[
        SnesGameplayFrameRenderer.Width * SnesGameplayFrameRenderer.Height];
    Array.Fill(frame, new Rgba32(0, 0, 0, 255));
    return frame;
}

/// <summary>Checks bank-$88 sky fixed-point bands and four queued circular-map rows.</summary>
static void VerifyScrollingSkyState()
{
    var sky = new ScrollingSkyState();
    var writes = new VramWriteQueue();
    sky.ProcessFrame(layer1YPosition: 0x041f, timeIsFrozen: false, writes);

    AssertEqual((ushort)0x041f, sky.VerticalScroll, "$88:AFB2 BG2 vertical scroll");
    AssertEqual(4, writes.Entries.Count, "$88:AFA3 four sky transfers");
    AssertEqual(new VramWriteEntry(0x0040, 0x8ad1c0, 0x4820), writes.Entries[0], "sky upper first row");
    AssertEqual(new VramWriteEntry(0x0040, 0x8ad200, 0x4840), writes.Entries[1], "sky upper second row");

    // Chunk index five is the intentional table-adjacency wrap to tilemap zero.
    AssertEqual(new VramWriteEntry(0x0040, 0x8ab1c0, 0x4c20), writes.Entries[2], "sky wrapped lower first row");
    AssertEqual(new VramWriteEntry(0x0040, 0x8ab200, 0x4c40), writes.Entries[3], "sky wrapped lower second row");

    // At the cutscene's camera Y=0, the unsigned subtraction produces $FFF0 and Y=$01FE.
    // The 65C816 consequently reads a word at $88:AF9A, 510 bytes beyond $88:AD9C.
    // Populate only the two ROM words this fixture needs: declared chunk zero ($B180) and
    // the actual adjacent instruction bytes interpreted as pointer $ADA6.
    var bank88Rom = new byte[0x048000];
    int chunkZeroOffset = SuperMetroidAddressSpace.ToRomOffset(0x88ad9c);
    bank88Rom[chunkZeroOffset] = 0x80;
    bank88Rom[chunkZeroOffset + 1] = 0xb1;
    int wrappedPointerOffset = SuperMetroidAddressSpace.ToRomOffset(0x88af9a);
    bank88Rom[wrappedPointerOffset] = 0xa6;
    bank88Rom[wrappedPointerOffset + 1] = 0xad;
    var topSky = new ScrollingSkyState(new SuperMetroidAddressSpace(bank88Rom));
    var topWrites = new VramWriteQueue();
    topSky.ProcessFrame(layer1YPosition: 0, timeIsFrozen: false, topWrites);
    AssertEqual(new VramWriteEntry(0x0040, 0x8ab526, 0x4fc0), topWrites.Entries[0],
        "sky Y=0 wrapped ROM pointer read");
    AssertEqual(new VramWriteEntry(0x0040, 0x8ab900, 0x4bc0), topWrites.Entries[2],
        "sky Y=0 lower row remains in chunk zero");

    // The $02E0 section aliases data slot eight, which already received the $0238 row's
    // +0.8000. Its additional +0.C000 produces integer 1 after only one frame.
    AssertEqual((ushort)1, sky.GetDataSlotPosition(8), "sky aliased fast HDMA slot");
    ushort[] lines = sky.BuildGameplayHorizontalScrolls(layer1YPosition: 0x02c0, lineCount: 1);
    AssertEqual((ushort)1, lines[0], "sky scanline resolves aliased HDMA slot");

    int tailBeforeFreeze = writes.TailInBytes;
    sky.ProcessFrame(layer1YPosition: 0x041f, timeIsFrozen: true, writes);
    AssertEqual(false, sky.HdmaEnabled, "frozen sky terminates HDMA table");
    AssertEqual(tailBeforeFreeze, writes.TailInBytes, "frozen sky queues no rows");

    Console.WriteLine("  Sky: HDMA bands, circular uploads, and the Y=0 ROM overread agree.");
}

/// <summary>
/// Exercises bank-$90 firing and movement, bank-$93 animation, and bank-$94 solid collision
/// for an uncharged power beam without sharing implementation code with the production path.
/// </summary>
static void VerifySamusPowerBeamProjectiles()
{
    var bus = new TestAddressSpace();

    // Construct the literal ROM records consumed by `$90:B887`, `$93:8000`, and
    // `$93:81E9`. Every direction points at one deliberately shared animation record; the
    // direction-table lookup itself is still exercised because all ten pointer cells must
    // be populated for the loop below to succeed.
    WriteTestWord(bus, 0x9383c1, 0x8431);
    WriteTestWord(bus, 0x938431, 0x0014);
    for (int direction = 0; direction < 10; direction++)
        WriteTestWord(bus, 0x938433 + direction * 2, 0x9000);
    WriteTestWord(bus, 0x939000, 0x000f);
    WriteTestWord(bus, 0x939002, 0xa000);
    bus.WriteByte(0x939004, 8);
    bus.WriteByte(0x939005, 4);
    WriteTestWord(bus, 0x939006, 0);
    WriteTestWord(bus, 0x939008, 0x8239);
    WriteTestWord(bus, 0x93900a, 0x9000);

    // Missile family one selects `$93:8641`; the non-beam table is indexed by type high
    // nibble before its ten direction records are selected.
    WriteTestWord(bus, 0x9383f3, 0x8641);
    WriteTestWord(bus, 0x938641, 0x0064);
    for (int direction = 0; direction < 10; direction++)
        WriteTestWord(bus, 0x938643 + direction * 2, 0x9200);
    bus.WriteBytes(0x939200, [
        0x0f, 0x00, 0x20, 0xa0, 0x04, 0x04, 0x00, 0x00,
        0x39, 0x82, 0x00, 0x92,
    ]);

    // Super Missiles use the adjacent non-beam family plus an invisible `$93:866D` link.
    // The link's empty spritemap is intentional: it exists for collision continuity and
    // shared-slot accounting, not as a second visible rocket.
    WriteTestWord(bus, 0x9383f5, 0x8657);
    WriteTestWord(bus, 0x938657, 0x012c);
    for (int direction = 0; direction < 10; direction++)
        WriteTestWord(bus, 0x938659 + direction * 2, 0x9240);
    bus.WriteBytes(0x939240, [
        0x0f, 0x00, 0x30, 0xa0, 0x08, 0x08, 0x00, 0x00,
        0x39, 0x82, 0x40, 0x92,
    ]);
    WriteTestWord(bus, 0x93842f, 0x866d);
    WriteTestWord(bus, 0x93866d, 0x012c);
    WriteTestWord(bus, 0x93866f, 0x9280);
    bus.WriteBytes(0x939280, [
        0x0f, 0x00, 0x40, 0xa0, 0x08, 0x08, 0x00, 0x00,
        0x39, 0x82, 0x80, 0x92,
    ]);

    // The animation record's `$A000` pointer is a bank-$93 spritemap, not merely an opaque
    // animation token. One literal entry makes the draw path observable independently of
    // the separate flare spritemap family seeded below.
    WriteTestWord(bus, 0x93a000, 1);
    WriteTestWord(bus, 0x93a002, 0);
    bus.WriteByte(0x93a004, 0);
    WriteTestWord(bus, 0x93a005, 0x2c20);
    // Both compact explosion programs below intentionally share `$A010`; give that pointer
    // its own visible one-OBJ map so the early explosion draw pass is tested, not inferred
    // from a nonzero animation pointer.
    WriteTestWord(bus, 0x93a010, 1);
    WriteTestWord(bus, 0x93a012, 0);
    bus.WriteByte(0x93a014, 0);
    WriteTestWord(bus, 0x93a015, 0x2c30);
    WriteTestWord(bus, 0x93a020, 1);
    WriteTestWord(bus, 0x93a022, 0);
    bus.WriteByte(0x93a024, 0);
    WriteTestWord(bus, 0x93a025, 0x2a44);
    WriteTestWord(bus, 0x93a030, 1);
    WriteTestWord(bus, 0x93a032, 0);
    bus.WriteByte(0x93a034, 0);
    WriteTestWord(bus, 0x93a035, 0x2a45);
    WriteTestWord(bus, 0x93a040, 0);

    // Charge-only power beam uses the parallel `$93:83D9` pointer family. Keep its
    // direction records shared but give it unmistakable damage so release cannot pass by
    // accidentally reusing the uncharged table.
    WriteTestWord(bus, 0x9383d9, 0x8460);
    WriteTestWord(bus, 0x938460, 0x0064);
    for (int direction = 0; direction < 10; direction++)
        WriteTestWord(bus, 0x938462 + direction * 2, 0x9000);

    // Fill the remaining eleven entries of both bank-$93 beam-data pointer tables with
    // distinct damage words. All directions deliberately share the already valid `$9000`
    // animation stream: these fixtures isolate the low-nibble table index without replacing
    // the production instruction interpreter or inventing host-side projectile art.
    for (int beamType = 1; beamType < 12; beamType++)
    {
        ushort unchargedData = unchecked((ushort)(0x8800 + beamType * 0x20));
        ushort chargedData = unchecked((ushort)(0x8a00 + beamType * 0x20));
        WriteTestWord(bus, 0x9383c1 + beamType * 2, unchargedData);
        WriteTestWord(bus, 0x9383d9 + beamType * 2, chargedData);
        WriteTestWord(bus, 0x930000 | unchargedData, unchecked((ushort)(0x0020 + beamType)));
        WriteTestWord(bus, 0x930000 | chargedData, unchecked((ushort)(0x0100 + beamType)));
        for (int direction = 0; direction < 10; direction++)
        {
            WriteTestWord(bus, 0x930000 | unchecked((ushort)(unchargedData + 2 + direction * 2)), 0x9000);
            WriteTestWord(bus, 0x930000 | unchecked((ushort)(chargedData + 2 + direction * 2)), 0x9000);
        }
    }

    // `$90:B5BB/$B609` select two independent trail instruction streams from the beam's
    // low six type bits. Ordinary power selects two empty lists; charged power selects the
    // long ice-style left list and an empty right list. These are genuine table relationships,
    // not a verifier-specific particle definition.
    WriteTestWord(bus, 0x90b5bb, 0xb4c9);
    WriteTestWord(bus, 0x90b609, 0xb4c9);
    WriteTestWord(bus, 0x90b5db, 0xb4cb);
    WriteTestWord(bus, 0x90b629, 0xb4c9);
    WriteTestWord(bus, 0x90b5fb, 0xb5a1);
    WriteTestWord(bus, 0x90b649, 0xb4c9);
    WriteTestWord(bus, 0x90b5fd, 0xb5a1);
    WriteTestWord(bus, 0x90b64b, 0xb4c9);
    WriteTestWord(bus, 0x90b4c9, 0x0000);
    bus.WriteBytes(0x90b5a1, [
        0x04, 0x00, 0x48, 0x2a,
        0x04, 0x00, 0x49, 0x2a,
        0x04, 0x00, 0x4a, 0x2a,
        0x04, 0x00, 0x4b, 0x2a,
        0x00, 0x00,
    ]);

    // Retain enough of retail `$90:B4CB` to prove repeated one-frame tiles and the embedded
    // `$B525` position command. The production interpreter remains ROM-driven and continues
    // through the full list when the real cartridge data is mounted.
    ushort chargedTrailInstruction = 0xb4cb;
    void WriteChargedTrailWord(ushort value)
    {
        WriteTestWord(bus, 0x900000 | chargedTrailInstruction, value);
        chargedTrailInstruction = unchecked((ushort)(chargedTrailInstruction + 2));
    }
    for (int record = 0; record < 4; record++)
    {
        WriteChargedTrailWord(1);
        WriteChargedTrailWord(0x2c38);
    }
    for (int record = 0; record < 2; record++)
    {
        WriteChargedTrailWord(1);
        WriteChargedTrailWord(0x2c39);
    }
    WriteChargedTrailWord(0xb525);
    WriteChargedTrailWord(1);
    WriteChargedTrailWord(0x2c39);
    WriteChargedTrailWord(0);

    // `$9B:A4B3/$A4CB` first choose a beam-combination family, then a direction-specific
    // offset list. Plain power's offsets are all zero, so `$9B:A3CC` positions the 8x8 trail
    // exactly four pixels above and left of the projectile's pre-movement center.
    WriteTestWord(bus, 0x9ba4b3, 0xa50b);
    WriteTestWord(bus, 0x9ba4cb, 0xa98f);
    for (int direction = 0; direction < 10; direction++)
    {
        WriteTestWord(bus, 0x9ba50b + direction * 2, 0xa56f);
        WriteTestWord(bus, 0x9ba98f + direction * 2, 0xaa07);
    }
    bus.WriteBytes(0x9ba56f, new byte[32]);
    bus.WriteBytes(0x9baa07, new byte[32]);

    // Collision swaps to this two-frame explosion record. Its following delete opcode
    // proves that damage remains occupied during the explosion and decrements the separate
    // projectile counter only when `$93:822F` finally clears the slot.
    WriteTestWord(bus, 0x9383ff, 0x9100);
    WriteTestWord(bus, 0x939100, 0x0002);
    WriteTestWord(bus, 0x939102, 0xa010);
    bus.WriteByte(0x939104, 8);
    bus.WriteByte(0x939105, 8);
    WriteTestWord(bus, 0x939106, 0);
    WriteTestWord(bus, 0x939108, 0x822f);

    // `$93:867F` is the missile-explosion instruction pointer consumed by `$93:80CF`.
    WriteTestWord(bus, 0x93867f, 0x9300);
    bus.WriteBytes(0x939300, [
        0x02, 0x00, 0x10, 0xa0, 0x08, 0x08, 0x00, 0x00,
        0x2f, 0x82,
    ]);
    WriteTestWord(bus, 0x938693, 0x9340);
    bus.WriteBytes(0x939340, [
        0x02, 0x00, 0x10, 0xa0, 0x08, 0x08, 0x00, 0x00,
        0x2f, 0x82,
    ]);

    bus.WriteByte(0x90c254, 0x0f);
    bus.WriteByte(0x90c264, 0x1e);
    WriteTestWord(bus, 0x90c28f, 0x000b);
    WriteTestWord(bus, 0x90c2a7, 0x0017);
    for (int beamType = 1; beamType < 12; beamType++)
    {
        // Distinct fixture bytes/words make an accidental entry-zero read immediately
        // observable. Charged cooldown indices begin at `$10`; auto-fire has its own twelve
        // byte table even though retail happens to store `$19` in every entry.
        bus.WriteByte(0x90c254 + beamType, unchecked((byte)(10 + beamType)));
        bus.WriteByte(0x90c264 + beamType, unchecked((byte)(30 + beamType)));
        bus.WriteByte(0x90c283 + beamType, unchecked((byte)(40 + beamType)));
        WriteTestWord(bus, 0x90c28f + beamType * 2, unchecked((ushort)(0x0030 + beamType)));
        WriteTestWord(bus, 0x90c2a7 + beamType * 2, unchecked((ushort)(0x0050 + beamType)));
    }
    WriteTestWord(bus, 0x90c2d1, 0x0400);
    WriteTestWord(bus, 0x90c2d3, 0x02ab);
    WriteTestWord(bus, 0x90c3b1, 0x8000);
    WriteTestWord(bus, 0x90c3c9, 0xc3e1);
    for (int index = 0; index < 0x100; index++)
        bus.WriteByte(0x9a8000 + index, unchecked((byte)(index ^ 0x5a)));
    for (int index = 0; index < 16; index++)
        WriteTestWord(bus, 0x90c3e1 + index * 2, unchecked((ushort)(0x0100 + index)));

    // Minimal but structurally authentic flare tables let the verifier exercise bank
    // `$90:BAFC` -> `$81:8A37` without copying production animation logic. Every possible
    // early table index selects the same one-entry spritemap; timing still comes from the
    // three independently addressed delay lists.
    for (int index = 0; index < 0x36; index++)
        WriteTestWord(bus, 0x93a1a1 + index * 2, 0xa500);
    WriteTestWord(bus, 0x93a500, 1);
    WriteTestWord(bus, 0x93a502, 0);
    bus.WriteByte(0x93a504, 0);
    WriteTestWord(bus, 0x93a505, 0x2c30);
    WriteTestWord(bus, 0x90c481, 0xc487);
    WriteTestWord(bus, 0x90c483, 0xc4a7);
    WriteTestWord(bus, 0x90c485, 0xc4ae);
    for (int index = 0; index < 30; index++)
        bus.WriteByte(0x90c487 + index, 3);
    bus.WriteByte(0x90c4a7, 5);
    bus.WriteByte(0x90c4a8, 0xff);
    bus.WriteByte(0x90c4ae, 4);
    bus.WriteByte(0x90c4af, 0xff);
    for (int direction = 0; direction < 10; direction++)
    {
        // These are the retail power-beam accelerations at `$90:C353/$C367`.
        short xAcceleration = direction switch
        {
            1 or 2 or 3 => 0x0010,
            6 or 7 or 8 => -0x0010,
            _ => 0,
        };
        short yAcceleration = direction switch
        {
            0 or 1 or 8 or 9 => -0x0010,
            3 or 4 or 5 or 6 => 0x0010,
            _ => 0,
        };
        WriteTestWord(bus, 0x90c353 + direction * 2, unchecked((ushort)xAcceleration));
        WriteTestWord(bus, 0x90c367 + direction * 2, unchecked((ushort)yAcceleration));
        WriteTestWord(bus, 0x90c204 + direction * 2, 0);
        WriteTestWord(bus, 0x90c218 + direction * 2, 0);
        WriteTestWord(bus, 0x90c22c + direction * 2, 0);
        WriteTestWord(bus, 0x90c240 + direction * 2, 0);

        // Missiles do not borrow the beam's tiny per-frame acceleration table. The native
        // initializer at `$90:B2F6` reads two signed words per direction from `$90:C303`:
        // cardinal axes use `$0040`, diagonals use `$0036`, and the opposite half-plane is
        // represented by two's-complement negatives. Keeping the literal ROM arrangement in
        // this fixture catches both direction-index mistakes and accidental host-vector math.
        short missileXAcceleration = direction switch
        {
            1 or 3 => 0x0036,
            2 => 0x0040,
            6 or 8 => -0x0036,
            7 => -0x0040,
            _ => 0,
        };
        short missileYAcceleration = direction switch
        {
            0 or 9 => -0x0040,
            1 or 8 => -0x0036,
            3 or 6 => 0x0036,
            4 or 5 => 0x0040,
            _ => 0,
        };
        WriteTestWord(bus, 0x90c303 + direction * 4, unchecked((ushort)missileXAcceleration));
        WriteTestWord(bus, 0x90c305 + direction * 4, unchecked((ushort)missileYAcceleration));
        short superXAcceleration = direction switch
        {
            1 or 3 => 0x00b6,
            2 => 0x0100,
            6 or 8 => -0x00b6,
            7 => -0x0100,
            _ => 0,
        };
        short superYAcceleration = direction switch
        {
            0 or 9 => -0x0100,
            1 or 8 => -0x00b6,
            3 or 6 => 0x00b6,
            4 or 5 => 0x0100,
            _ => 0,
        };
        WriteTestWord(bus, 0x90c32b + direction * 4, unchecked((ushort)superXAcceleration));
        WriteTestWord(bus, 0x90c32d + direction * 4, unchecked((ushort)superYAcceleration));
    }

    const int width = 32;
    const int height = 16;
    RoomLevelData air = new(
        width,
        height,
        new ushort[width * height],
        new byte[width * height],
        new ushort[width * height],
        new byte[8]);

    // Update_Beam_Tiles_and_Palette queues exactly $100 bytes to VRAM word $6300 and
    // writes sprite palette six. Drain the ordinary queue so this also validates the same
    // hardware path used by runtime room setup, not a verifier-only direct memory copy.
    var beamVram = new SnesVram();
    var beamCgram = new SnesCgram();
    var beamWrites = new VramWriteQueue();
    var beamGraphics = new SamusProjectileSystem();
    beamGraphics.QueueBeamTilesAndLoadPalette(bus, beamWrites, beamCgram, equippedBeams: 0);
    AssertEqual(1, beamWrites.Entries.Count, "power beam queues one tile DMA");
    beamWrites.DrainTo(beamVram, bus);
    AssertEqual((byte)0x5a, beamVram.ReadByte(0x6300 * 2),
        "power beam tiles begin at VRAM word $6300");
    AssertEqual((byte)0xa5, beamVram.ReadByte(0x6300 * 2 + 0xff),
        "power beam tile DMA copies exactly $100 source bytes");
    AssertEqual((ushort)0x0100, beamCgram.Colors[0xe0],
        "power beam palette begins at OBJ palette six");
    AssertEqual((ushort)0x010f, beamCgram.Colors[0xef],
        "power beam palette copies sixteen colors");

    // `$90:BA56` accepts exactly ten low-nibble direction values. Exercise every pointer,
    // horizontal/vertical/diagonal base-speed choice, acceleration sign, immediate movement,
    // animation selection, cooldown, sound, and live-slot counter in isolation.
    for (byte direction = 0; direction < 10; direction++)
    {
        byte pose = unchecked((byte)(0x20 + direction));
        bus.WriteBytes(
            0x91b629 + pose * 8,
            [0x08, 0x00, 0x00, direction, 0x00, 0x00, 0x00, 0x00]);
        var samus = new SamusState
        {
            Pose = pose,
            XPosition = 128,
            YPosition = 96,
            EquippedBeams = 0,
            SelectedHudItem = 0,
        };
        var bombs = new SamusBombProjectileSystem();
        var projectiles = new SamusProjectileSystem();
        bombs.StepFrame(bus, air, samus, 0, 0);
        SamusProjectileFrameResult result = projectiles.StepFrame(
            bus,
            air,
            samus,
            (ushort)SnesButton.X,
            (ushort)SnesButton.X,
            0,
            0,
            bombs);

        AssertEqual((int?)0, result.FiredSlot, $"power beam direction {direction} allocates slot zero");
        AssertEqual((ushort)0x000b, result.QueuedSoundEffect,
            $"power beam direction {direction} queues ROM sound");
        AssertEqual((ushort)1, projectiles.ProjectileCounter,
            $"power beam direction {direction} increments counter");
        AssertEqual((ushort)0x000f, bombs.CooldownTimer,
            $"power beam direction {direction} installs cooldown");
        AssertEqual((ushort)direction, projectiles.Slots[0].Direction,
            $"power beam direction {direction} survives initialization");
        AssertEqual((ushort)0x0014, projectiles.Slots[0].Damage,
            $"power beam direction {direction} loads damage");
        AssertEqual((ushort)0xa000, projectiles.Slots[0].SpritemapPointer,
            $"power beam direction {direction} selects first art record");
        AssertEqual((ushort)8, projectiles.Slots[0].XRadius,
            $"power beam direction {direction} loads X radius");
        AssertEqual((ushort)4, projectiles.Slots[0].YRadius,
            $"power beam direction {direction} loads Y radius");
    }

    // Charge Beam fires an ordinary shot on the initial held frame, counts to sixty while
    // the muzzle flare becomes visible at fifteen, and emits the charged data family only
    // when Shoot is released. This sequence mirrors `$90:B80D` frame by frame.
    const byte rightPose = 1;
    bus.WriteBytes(
        0x91b629 + rightPose * 8,
        [0x08, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00]);

    // Every valid low-nibble combination must index its own projectile data, cooldown, and
    // sound cells. The dispatch split is equally data-significant: even entries stop on
    // terrain, low wave entries 1/3 reload trail timer three, and wave entries 5/7/9/11
    // reload four. Spazer/plasma remain one slot because their width lives in spritemap art.
    for (ushort beamType = 1; beamType < 12; beamType++)
    {
        var combinedSamus = new SamusState
        {
            Pose = rightPose,
            XPosition = 128,
            YPosition = 96,
            EquippedBeams = beamType,
        };
        var combinedBombs = new SamusBombProjectileSystem();
        var combinedProjectiles = new SamusProjectileSystem();
        combinedBombs.StepFrame(bus, air, combinedSamus, 0, 0);
        SamusProjectileFrameResult combinedResult = combinedProjectiles.StepFrame(
            bus,
            air,
            combinedSamus,
            (ushort)SnesButton.X,
            (ushort)SnesButton.X,
            0,
            0,
            combinedBombs);

        SamusProjectileSlot combinedSlot = combinedProjectiles.Slots[0];
        AssertEqual((int?)0, combinedResult.FiredSlot,
            $"beam combination {beamType} allocates one ordinary slot");
        AssertEqual(unchecked((ushort)(0x0020 + beamType)), combinedSlot.Damage,
            $"beam combination {beamType} indexes uncharged data pointer");
        AssertEqual(unchecked((ushort)(10 + beamType)), combinedBombs.CooldownTimer,
            $"beam combination {beamType} indexes uncharged cooldown");
        AssertEqual(unchecked((ushort)(0x0030 + beamType)), combinedResult.QueuedSoundEffect,
            $"beam combination {beamType} indexes uncharged sound");
        AssertEqual((ushort)10, combinedProjectiles.ProjectileInvincibilityTimer,
            $"beam combination {beamType} publishes native invincibility timer");

        SamusProjectilePreInstruction expectedPreInstruction = (beamType & 1) == 0
            ? SamusProjectilePreInstruction.NoWaveBeam
            : beamType < 4
                ? SamusProjectilePreInstruction.WaveBeamThreeFrameTrail
                : SamusProjectilePreInstruction.WaveBeamFourFrameTrail;
        AssertEqual(expectedPreInstruction, combinedSlot.PreInstruction,
            $"beam combination {beamType} selects native pre-instruction family");
    }

    // Spazer/plasma art takes `$93:8275` rather than the ordinary power/ice/wave flicker
    // branch. Host slot zero is visible while NMI bit one is clear and suppressed while it
    // is set—the opposite comparison and a different counter bit from entry-zero power.
    var spazerFlickerSamus = new SamusState
    {
        Pose = rightPose,
        XPosition = 128,
        YPosition = 96,
        EquippedBeams = 4,
    };
    var spazerFlickerBombs = new SamusBombProjectileSystem();
    var spazerFlickerProjectiles = new SamusProjectileSystem();
    spazerFlickerBombs.StepFrame(bus, air, spazerFlickerSamus, 0, 0);
    spazerFlickerProjectiles.StepFrame(
        bus,
        air,
        spazerFlickerSamus,
        (ushort)SnesButton.X,
        (ushort)SnesButton.X,
        0,
        0,
        spazerFlickerBombs);
    var spazerFlickerOam = new OamBuffer();
    spazerFlickerOam.BeginFrame();
    spazerFlickerProjectiles.DrawLiveProjectiles(
        bus, spazerFlickerOam, 0, 0, nmiFrameCounter: 0);
    AssertTrue(spazerFlickerOam.NextByteOffset != 0,
        "even-slot Spazer draws while NMI bit one is clear");
    spazerFlickerOam.BeginFrame();
    spazerFlickerProjectiles.DrawLiveProjectiles(
        bus, spazerFlickerOam, 0, 0, nmiFrameCounter: 2);
    AssertEqual(0, spazerFlickerOam.NextByteOffset,
        "even-slot Spazer suppresses while NMI bit one is set");

    // The initial firing pass changes timer four to three. Three more wave passes cause the
    // first trail allocation and expose the only cadence distinction in `$B0C3/$B0E4`.
    foreach ((ushort beamType, ushort expectedReload) in new (ushort, ushort)[]
    {
        (1, 3), // Uncharged power+wave uses `$90:B0E4`.
        (3, 3), // Uncharged ice+wave uses the same low-family routine.
        (5, 4), // Spazer+wave uses the common `$90:B0C3` routine.
        (9, 4), // Plasma+wave also uses the common routine.
    })
    {
        var cadenceSamus = new SamusState
        {
            Pose = rightPose,
            XPosition = 128,
            YPosition = 96,
            EquippedBeams = beamType,
        };
        var cadenceBombs = new SamusBombProjectileSystem();
        var cadenceProjectiles = new SamusProjectileSystem();
        cadenceBombs.StepFrame(bus, air, cadenceSamus, 0, 0);
        cadenceProjectiles.StepFrame(
            bus, air, cadenceSamus, (ushort)SnesButton.X, (ushort)SnesButton.X, 0, 0, cadenceBombs);
        for (int frame = 0; frame < 3; frame++)
        {
            cadenceBombs.StepFrame(bus, air, cadenceSamus, 0, 0);
            cadenceProjectiles.StepFrame(bus, air, cadenceSamus, 0, 0, 0, 0, cadenceBombs);
        }
        AssertEqual(expectedReload, cadenceProjectiles.Slots[0].TrailTimer,
            $"wave combination {beamType} reloads native trail cadence");
    }

    var chargeSamus = new SamusState
    {
        Pose = rightPose,
        XPosition = 128,
        YPosition = 96,
        EquippedBeams = 0x1000,
    };
    var chargeBombs = new SamusBombProjectileSystem();
    var chargeProjectiles = new SamusProjectileSystem();
    var flareOam = new OamBuffer();
    bool flareBecameVisible = false;
    for (int frame = 0; frame < 60; frame++)
    {
        chargeBombs.StepFrame(bus, air, chargeSamus, 0, 0);
        SamusProjectileFrameResult chargeFrame = chargeProjectiles.StepFrame(
            bus,
            air,
            chargeSamus,
            (ushort)SnesButton.X,
            frame == 0 ? (ushort)SnesButton.X : (ushort)0,
            0,
            0,
            chargeBombs);
        if (frame == 0)
            AssertEqual((int?)0, chargeFrame.FiredSlot, "charge press fires initial ordinary shot");

        flareOam.BeginFrame();
        chargeProjectiles.HandleChargeFlareAndDraw(bus, flareOam, chargeSamus, 0, 0);
        // Runtime's later `$93:82F7` draw phase must run on every simulated gameplay frame.
        // The ordinary power beam periodically allocates two empty streams; `$90:B6A9`
        // consumes their zero terminators immediately instead of leaving timer-one slots.
        chargeProjectiles.HandleTrailsAndDraw(bus, flareOam, 0, 0, timeIsFrozen: false);
        flareBecameVisible |= flareOam.NextByteOffset != 0;
    }
    AssertEqual((ushort)60, chargeProjectiles.FlareCounter,
        "charge held frames reach armed threshold");
    AssertTrue(flareBecameVisible, "charge flare becomes visible from ROM spritemap table");

    chargeBombs.StepFrame(bus, air, chargeSamus, 0, 0);
    SamusProjectileFrameResult chargedRelease = chargeProjectiles.StepFrame(
        bus, air, chargeSamus, 0, 0, 0, 0, chargeBombs);
    AssertTrue(chargedRelease.FiredSlot is not null, "charged release allocates projectile");
    SamusProjectileSlot chargedSlot = chargeProjectiles.Slots[chargedRelease.FiredSlot!.Value];
    AssertEqual((ushort)0x0064, chargedSlot.Damage, "charged release uses charged data pointer");
    AssertEqual((ushort)0x0010, unchecked((ushort)(chargedSlot.Type & 0x0010)),
        "charged release sets charged type bit");
    AssertEqual((ushort)0x0017, chargedRelease.QueuedSoundEffect,
        "charged power beam queues ROM sound");
    AssertEqual((ushort)0x001e, chargeBombs.CooldownTimer,
        "charged power beam installs charged cooldown");
    AssertEqual((ushort)4, chargeProjectiles.ChargedShotGlowTimer,
        "charged release installs glow timer");
    AssertEqual((ushort)0, chargeProjectiles.FlareCounter,
        "charged release clears flare counter");

    // Charged combinations index the parallel pointer/sound range and cooldown bytes
    // `$10-$1B`. Even a low-family wave now uses the common four-frame wave routine; the
    // special three-frame reload belongs only to uncharged types one and three.
    foreach (ushort beamType in new ushort[] { 1, 4, 5, 9, 11 })
    {
        var combinedChargeSamus = new SamusState
        {
            Pose = rightPose,
            XPosition = 128,
            YPosition = 96,
            EquippedBeams = unchecked((ushort)(0x1000 | beamType)),
        };
        var combinedChargeBombs = new SamusBombProjectileSystem();
        var combinedChargeProjectiles = new SamusProjectileSystem();
        for (int frame = 0; frame < 60; frame++)
        {
            combinedChargeBombs.StepFrame(bus, air, combinedChargeSamus, 0, 0);
            combinedChargeProjectiles.StepFrame(
                bus,
                air,
                combinedChargeSamus,
                (ushort)SnesButton.X,
                frame == 0 ? (ushort)SnesButton.X : (ushort)0,
                0,
                0,
                combinedChargeBombs);
        }

        combinedChargeBombs.StepFrame(bus, air, combinedChargeSamus, 0, 0);
        SamusProjectileFrameResult combinedChargedRelease = combinedChargeProjectiles.StepFrame(
            bus, air, combinedChargeSamus, 0, 0, 0, 0, combinedChargeBombs);
        AssertTrue(combinedChargedRelease.FiredSlot is not null,
            $"charged beam combination {beamType} allocates on release");
        SamusProjectileSlot combinedChargedSlot =
            combinedChargeProjectiles.Slots[combinedChargedRelease.FiredSlot!.Value];
        AssertEqual(unchecked((ushort)(0x0100 + beamType)), combinedChargedSlot.Damage,
            $"charged beam combination {beamType} indexes charged data pointer");
        AssertEqual(unchecked((ushort)(30 + beamType)), combinedChargeBombs.CooldownTimer,
            $"charged beam combination {beamType} indexes charged cooldown");
        AssertEqual(unchecked((ushort)(0x0050 + beamType)), combinedChargedRelease.QueuedSoundEffect,
            $"charged beam combination {beamType} indexes charged sound");
        AssertEqual(
            (beamType & 1) == 0
                ? SamusProjectilePreInstruction.NoWaveBeam
                : SamusProjectilePreInstruction.WaveBeamFourFrameTrail,
            combinedChargedSlot.PreInstruction,
            $"charged beam combination {beamType} selects native wave dispatch");
    }

    // Hyper Beam ignores the equipped combination for projectile identity and forces
    // charged-Plasma type `$9018`. Bank `$93` supplies its direction art/radii, after which
    // the producer overwrites damage with literal 1000 and arms the descending flare state.
    var hyperSamus = new SamusState
    {
        Pose = rightPose,
        XPosition = 128,
        YPosition = 96,
        EquippedBeams = 0x1009,
        HyperBeam = 0x8000,
    };
    var hyperBombs = new SamusBombProjectileSystem();
    var hyperProjectiles = new SamusProjectileSystem();
    hyperBombs.StepFrame(bus, air, hyperSamus, 0, 0);
    SamusProjectileFrameResult hyperResult = hyperProjectiles.StepFrame(
        bus,
        air,
        hyperSamus,
        (ushort)SnesButton.X,
        (ushort)SnesButton.X,
        0,
        0,
        hyperBombs);
    SamusProjectileSlot hyperSlot = hyperProjectiles.Slots[0];
    AssertEqual((int?)0, hyperResult.FiredSlot, "Hyper Beam allocates ordinary slot zero");
    AssertEqual((ushort)0x9018, hyperSlot.Type, "Hyper Beam forces literal type `$9018`");
    AssertEqual((ushort)1000, hyperSlot.Damage, "Hyper Beam overwrites ROM-table damage with 1000");
    AssertEqual(SamusProjectilePreInstruction.HyperBeam, hyperSlot.PreInstruction,
        "Hyper Beam selects trail-free Wave movement");
    AssertEqual((ushort)0, hyperSlot.TrailTimer, "Hyper Beam does not arm a projectile trail");
    AssertEqual((ushort)21, hyperBombs.CooldownTimer, "Hyper Beam installs literal cooldown 21");
    AssertEqual((ushort)0x0058, hyperResult.QueuedSoundEffect,
        "Hyper Beam indexes charged sound entry eight");
    AssertEqual((ushort)0x8014, hyperProjectiles.ChargedShotGlowTimer,
        "Hyper Beam installs signed palette/glow phase `$8014`");
    AssertEqual((ushort)0x8000, hyperProjectiles.FlareCounter,
        "Hyper Beam arms descending flare sentinel");

    // Its three components begin at frames 29/5/5 with timer three. Fast sparks (component
    // two) own completion, so exactly fifteen draw calls count five records down to zero.
    var hyperFlareOam = new OamBuffer();
    for (int call = 0; call < 15; call++)
    {
        hyperFlareOam.BeginFrame();
        hyperProjectiles.HandleChargeFlareAndDraw(bus, hyperFlareOam, hyperSamus, 0, 0);
        AssertTrue(hyperFlareOam.NextByteOffset != 0,
            $"Hyper Beam descending flare call {call + 1} draws ROM spritemaps");
    }
    AssertEqual((ushort)0, hyperProjectiles.FlareCounter,
        "Hyper Beam fast-spark frame one clears flare sentinel");

    // `$93:8268` exempts charged-family bit `$0010` from ordinary beam flicker. Slot zero
    // would be suppressed on even NMI under the uncharged rule, making this a direct guard
    // against accidentally applying that branch to the charged projectile.
    var chargedOam = new OamBuffer();
    chargedOam.BeginFrame();
    chargeProjectiles.DrawLiveProjectiles(bus, chargedOam, 0, 0, nmiFrameCounter: 0);
    AssertTrue(chargedOam.NextByteOffset != 0,
        "charged power beam bypasses ordinary alternating-frame flicker");

    // The release frame has already changed trail timer 4 to 3. Three more alpha passes
    // reach zero and allocate native trail byte index `$22` before the third pass moves the
    // projectile. Handling the draw immediately must consume only the populated left stream.
    chargeBombs.StepFrame(bus, air, chargeSamus, 0, 0);
    chargeProjectiles.StepFrame(bus, air, chargeSamus, 0, 0, 0, 0, chargeBombs);
    chargeBombs.StepFrame(bus, air, chargeSamus, 0, 0);
    chargeProjectiles.StepFrame(bus, air, chargeSamus, 0, 0, 0, 0, chargeBombs);
    ushort trailSourceX = chargedSlot.XPosition;
    ushort trailSourceY = chargedSlot.YPosition;
    chargeBombs.StepFrame(bus, air, chargeSamus, 0, 0);
    chargeProjectiles.StepFrame(bus, air, chargeSamus, 0, 0, 0, 0, chargeBombs);

    AssertEqual(1, chargeProjectiles.ActiveTrailCount,
        "charged power allocates one of eighteen trail slots every fourth alpha pass");
    SamusProjectileTrailSlot chargedTrail =
        chargeProjectiles.TrailSlots[SamusProjectileSystem.TrailSlotCount - 1];
    AssertTrue(chargedTrail.IsActive, "trail allocation scans downward from native index $22");
    AssertEqual(unchecked((ushort)(trailSourceX - 4)), chargedTrail.Left.XPosition,
        "trail samples projectile X before movement and subtracts four");
    AssertEqual(unchecked((ushort)(trailSourceY - 4)), chargedTrail.Left.YPosition,
        "trail samples projectile Y before movement and subtracts four");

    var trailOam = new OamBuffer();
    trailOam.BeginFrame();
    chargeProjectiles.HandleTrailsAndDraw(bus, trailOam, 0, 0, timeIsFrozen: false);
    AssertEqual(4, trailOam.NextByteOffset, "charged power's left stream emits one raw OBJ");
    AssertEqual((ushort)0, chargedTrail.Right.InstructionTimer,
        "charged power's empty right stream terminates without drawing");
    OamEntry firstTrailObj = trailOam.GetEntry(0);
    AssertEqual(0x038, firstTrailObj.TileNumber, "charged trail reads first `$2C38` tile");
    AssertEqual(6, firstTrailObj.Palette, "charged trail retains packed OBJ palette six");
    AssertTrue(!firstTrailObj.IsLarge, "projectile trail is an explicit small OBJ");

    // Time freeze bypasses DEC and command parsing but not OAM emission. The same record and
    // instruction pointer must remain visible and unchanged for an arbitrary frozen frame.
    ushort frozenTrailPointer = chargedTrail.Left.InstructionPointer;
    ushort frozenTrailTimer = chargedTrail.Left.InstructionTimer;
    trailOam.BeginFrame();
    chargeProjectiles.HandleTrailsAndDraw(bus, trailOam, 0, 0, timeIsFrozen: true);
    AssertEqual(frozenTrailPointer, chargedTrail.Left.InstructionPointer,
        "frozen trail retains instruction pointer");
    AssertEqual(frozenTrailTimer, chargedTrail.Left.InstructionTimer,
        "frozen trail retains instruction timer");
    AssertEqual(4, trailOam.NextByteOffset, "frozen active trail still draws");

    // Five more records reach the inline `$B525` opcode on the sixth call after the first
    // draw. It mutates world Y, then falls through to the following timed tile in one pass.
    ushort beforeTrailCommandY = chargedTrail.Left.YPosition;
    for (int call = 0; call < 6; call++)
    {
        trailOam.BeginFrame();
        chargeProjectiles.HandleTrailsAndDraw(bus, trailOam, 0, 0, timeIsFrozen: false);
    }
    AssertEqual(unchecked((ushort)(beforeTrailCommandY + 1)), chargedTrail.Left.YPosition,
        "inline `$B525` command moves the left trail down one pixel");
    AssertEqual(0x039, trailOam.GetEntry(0).TileNumber,
        "position command falls through to the following `$2C39` timed record");

    // Isolate horizontal fixed-point motion and collision against an authentic type-eight
    // solid column. The first rightward frame uses velocity `$0400+$0010`, producing four
    // whole pixels and subposition `$1000`; repeated alpha passes eventually install the
    // ROM explosion without freeing the slot early.
    var wallWords = new ushort[width * height];
    for (int y = 0; y < height; y++)
        wallWords[y * width + 6] = 0x8000;
    RoomLevelData wall = new(
        width,
        height,
        wallWords,
        new byte[wallWords.Length],
        new ushort[wallWords.Length],
        new byte[8]);

    // Wave collision routines still scan the projectile's full radius but return carry
    // clear after every block reaction. Drive power+wave through the same strict type-eight
    // wall that kills the no-wave fixture below; its center must emerge beyond the column
    // without ever entering the explosion family.
    var waveWallSamus = new SamusState
    {
        Pose = rightPose,
        XPosition = 64,
        YPosition = 96,
        EquippedBeams = 1,
    };
    var waveWallBombs = new SamusBombProjectileSystem();
    var waveWallProjectiles = new SamusProjectileSystem();
    bool waveReportedExplosion = false;
    for (int frame = 0; frame < 12; frame++)
    {
        waveWallBombs.StepFrame(bus, wall, waveWallSamus, 0, 0);
        SamusProjectileFrameResult waveFrame = waveWallProjectiles.StepFrame(
            bus,
            wall,
            waveWallSamus,
            frame == 0 ? (ushort)SnesButton.X : (ushort)0,
            frame == 0 ? (ushort)SnesButton.X : (ushort)0,
            0,
            0,
            waveWallBombs);
        waveReportedExplosion |= waveFrame.CollisionStartedExplosion;
    }
    AssertTrue(!waveReportedExplosion, "wave beam never converts on a type-eight wall");
    AssertTrue(waveWallProjectiles.Slots[0].XPosition > 112,
        "wave beam advances completely through the solid column");
    AssertEqual(SamusProjectilePreInstruction.WaveBeamThreeFrameTrail,
        waveWallProjectiles.Slots[0].PreInstruction,
        "power+wave remains in its native pass-through pre-instruction");

    bus.WriteBytes(
        0x91b629 + rightPose * 8,
        [0x08, 0x00, 0x00, 0x02, 0x00, 0x00, 0x00, 0x00]);
    var wallSamus = new SamusState
    {
        Pose = rightPose,
        XPosition = 64,
        YPosition = 96,
    };
    var wallBombs = new SamusBombProjectileSystem();
    var wallProjectiles = new SamusProjectileSystem();
    wallBombs.StepFrame(bus, wall, wallSamus, 0, 0);
    wallProjectiles.StepFrame(
        bus,
        wall,
        wallSamus,
        (ushort)SnesButton.X,
        (ushort)SnesButton.X,
        0,
        0,
        wallBombs);
    AssertEqual((ushort)68, wallProjectiles.Slots[0].XPosition,
        "right beam first frame moves four whole pixels");
    AssertEqual((ushort)0x1000, wallProjectiles.Slots[0].XSubposition,
        "right beam first frame retains one-sixteenth pixel");

    SamusProjectileFrameResult wallResult = default;
    for (int frame = 0; frame < 16 && !wallResult.CollisionStartedExplosion; frame++)
    {
        wallBombs.StepFrame(bus, wall, wallSamus, 0, 0);
        wallResult = wallProjectiles.StepFrame(
            bus, wall, wallSamus, 0, 0, 0, 0, wallBombs);
    }
    AssertTrue(wallResult.CollisionStartedExplosion, "power beam reaches type-eight wall");
    AssertEqual((ushort)0x0700,
        unchecked((ushort)(wallProjectiles.Slots[0].Type & 0x0f00)),
        "wall collision installs beam-explosion family");
    AssertEqual((ushort)1, wallProjectiles.ProjectileCounter,
        "beam explosion retains ordinary slot count");
    AssertEqual((ushort)0xa010, wallProjectiles.Slots[0].SpritemapPointer,
        "collision frame selects first explosion art");

    for (int frame = 0; frame < 2; frame++)
    {
        wallBombs.StepFrame(bus, wall, wallSamus, 0, 0);
        wallProjectiles.StepFrame(bus, wall, wallSamus, 0, 0, 0, 0, wallBombs);
    }
    AssertEqual((ushort)0, wallProjectiles.ProjectileCounter,
        "explosion delete decrements ordinary counter");
    AssertTrue(!wallProjectiles.Slots[0].IsActive,
        "explosion delete clears ordinary slot");

    // Replace the inert type-eight column with the two native shootable collision nibbles.
    // This is an end-to-end producer test: a fired projectile must reach bank-$94's radius
    // scanner, publish the correct bank-$84 PLM, run CE6B's synchronous terrain mutation,
    // and retain the collision nibble's carry result. Filling the entire column makes the
    // fixture independent of the pose-authored cannon Y offset while still requiring the
    // projectile to travel from Samus to block column six.
    var solidShotWords = new ushort[width * height];
    var solidShotBehaviors = new byte[solidShotWords.Length];
    for (int y = 0; y < height; y++)
        solidShotWords[y * width + 6] = 0xc000;
    RoomLevelData solidShotWall = new(
        width,
        height,
        solidShotWords,
        solidShotBehaviors,
        new ushort[solidShotWords.Length],
        new byte[8]);
    var solidShotSamus = new SamusState
    {
        Pose = rightPose,
        XPosition = 64,
        YPosition = 96,
    };
    var solidShotBombs = new SamusBombProjectileSystem();
    var solidShotProjectiles = new SamusProjectileSystem();
    var solidShotPlms = new RoomPlmSystem();
    SamusProjectileFrameResult solidShotResult = default;
    for (int frame = 0; frame < 16 && !solidShotResult.CollisionStartedExplosion; frame++)
    {
        solidShotBombs.StepFrame(bus, solidShotWall, solidShotSamus, 0, 0);
        solidShotResult = solidShotProjectiles.StepFrame(
            bus,
            solidShotWall,
            solidShotSamus,
            frame == 0 ? (ushort)SnesButton.X : (ushort)0,
            frame == 0 ? (ushort)SnesButton.X : (ushort)0,
            0,
            0,
            solidShotBombs,
            roomPlms: solidShotPlms);
    }
    AssertTrue(solidShotResult.CollisionStartedExplosion,
        "type-C shootable block retains solid shot collision");
    AssertTrue(solidShotPlms.ActiveCount > 0,
        "ordinary beam collision allocates bank-$84 shot-block PLM");
    int synthesizedSolidShotBlocks = 0;
    for (int y = 0; y < height; y++)
    {
        if (solidShotWall.GetCollisionBlock(6, y).LevelWord == 0x8052)
            synthesizedSolidShotBlocks++;
    }
    AssertTrue(synthesizedSolidShotBlocks > 0,
        "CE6B synchronously converts contacted type-C block to synthesized $8052");

    // Type four calls the very same setup but returns carry clear. Wave compounds that rule:
    // `$94:A352` must run every block side effect and then discard even a carry-set reaction.
    // Crossing the whole column without an explosion proves neither the new PLM publication
    // nor the temporary `$0052` word accidentally turned Wave into a clipping projectile.
    var waveShotWords = new ushort[width * height];
    var waveShotBehaviors = new byte[waveShotWords.Length];
    for (int y = 0; y < height; y++)
        waveShotWords[y * width + 6] = 0x4000;
    RoomLevelData waveShotWall = new(
        width,
        height,
        waveShotWords,
        waveShotBehaviors,
        new ushort[waveShotWords.Length],
        new byte[8]);
    var waveShotSamus = new SamusState
    {
        Pose = rightPose,
        XPosition = 64,
        YPosition = 96,
        EquippedBeams = 1,
    };
    var waveShotBombs = new SamusBombProjectileSystem();
    var waveShotProjectiles = new SamusProjectileSystem();
    var waveShotPlms = new RoomPlmSystem();
    bool waveShotReportedExplosion = false;
    for (int frame = 0; frame < 12; frame++)
    {
        waveShotBombs.StepFrame(bus, waveShotWall, waveShotSamus, 0, 0);
        SamusProjectileFrameResult waveShotFrame = waveShotProjectiles.StepFrame(
            bus,
            waveShotWall,
            waveShotSamus,
            frame == 0 ? (ushort)SnesButton.X : (ushort)0,
            frame == 0 ? (ushort)SnesButton.X : (ushort)0,
            0,
            0,
            waveShotBombs,
            roomPlms: waveShotPlms);
        waveShotReportedExplosion |= waveShotFrame.CollisionStartedExplosion;
    }
    AssertTrue(!waveShotReportedExplosion,
        "Wave remains alive after publishing type-four shot-block reaction");
    AssertTrue(waveShotProjectiles.Slots[0].XPosition > 112,
        "Wave crosses the complete shootable-air column");
    AssertTrue(waveShotPlms.ActiveCount > 0,
        "Wave scan allocates bank-$84 shot-block PLM");
    int synthesizedAirShotBlocks = 0;
    for (int y = 0; y < height; y++)
    {
        if (waveShotWall.GetCollisionBlock(6, y).LevelWord == 0x0052)
            synthesizedAirShotBlocks++;
    }
    AssertTrue(synthesizedAirShotBlocks > 0,
        "CE6B synchronously converts contacted type-four block to synthesized $0052");

    // `$90:BE62` shares the ordinary five-slot array with beams but selects a completely
    // different bank-$93 data family and bank-$90 pre-instruction. Fire a rightward missile
    // into the same type-eight column so producer state, first-frame ignition, persistent
    // trail, point collision, and missile-specific explosion are all observed in one route.
    var missileSamus = new SamusState
    {
        Pose = rightPose,
        XPosition = 64,
        YPosition = 96,
        SelectedHudItem = 1,
        Missiles = 3,
    };
    var missileBombs = new SamusBombProjectileSystem();
    var missileProjectiles = new SamusProjectileSystem();
    missileBombs.StepFrame(bus, wall, missileSamus, 0, 0);
    SamusProjectileFrameResult missileFired = missileProjectiles.StepFrame(
        bus,
        wall,
        missileSamus,
        (ushort)SnesButton.X,
        (ushort)SnesButton.X,
        0,
        0,
        missileBombs);
    SamusProjectileSlot missile = missileProjectiles.Slots[0];
    AssertEqual((int?)0, missileFired.FiredSlot, "missile fresh press allocates slot zero");
    AssertEqual((ushort)3, missileFired.QueuedSoundEffect,
        "missile producer queues library-one effect three");
    AssertEqual((ushort)2, missileSamus.Missiles, "missile producer consumes exactly one ammo");
    AssertEqual((ushort)1, missileSamus.SelectedHudItem,
        "nonempty missile reserve remains HUD-selected");
    AssertEqual((ushort)1, missileProjectiles.ProjectileCounter,
        "missile increments the shared ordinary-projectile counter");
    AssertEqual((ushort)10, missileBombs.CooldownTimer,
        "missile producer installs literal ten-frame shared cooldown");
    AssertEqual((ushort)20, missileProjectiles.ProjectileInvincibilityTimer,
        "missile producer installs literal projectile invincibility timer twenty");
    AssertEqual((ushort)0x8100, missile.Type, "missile uses active type word `$8100`");
    AssertEqual((ushort)0x0064, missile.Damage, "missile reads damage from `$93:8641`");
    AssertEqual(SamusProjectilePreInstruction.Missile, missile.PreInstruction,
        "missile selects `$90:AF68` pre-instruction family");
    AssertEqual((ushort)0x0100, missile.Variable,
        "first alpha pass crosses `$0100` ignition threshold");
    AssertEqual((short)0x0100, missile.XVelocity,
        "right missile begins at one pixel per frame after ignition");
    AssertEqual((ushort)65, missile.XPosition,
        "right missile moves one whole pixel on its ignition frame");
    AssertEqual((ushort)0xa020, missile.SpritemapPointer,
        "missile instruction handler selects first bank-$93 art record");

    var missileOam = new OamBuffer();
    missileOam.BeginFrame();
    missileProjectiles.DrawLiveProjectiles(bus, missileOam, 0, 0, nmiFrameCounter: 0);
    AssertEqual(4, missileOam.NextByteOffset,
        "missile family bypasses ordinary beam alternating-frame flicker");
    AssertEqual(0x044, missileOam.GetEntry(0).TileNumber,
        "missile draw consumes its `$2A44` fixture OBJ");

    // The firing alpha pass changed trail timer four to three. Exactly three further alpha
    // passes allocate native trail entry `$20`; the subsequent draw parses `$90:B5A1` and
    // publishes its first four-frame `$2A48` record while the empty right stream terminates.
    for (int frame = 0; frame < 3; frame++)
    {
        missileBombs.StepFrame(bus, wall, missileSamus, 0, 0);
        missileProjectiles.StepFrame(bus, wall, missileSamus, 0, 0, 0, 0, missileBombs);
    }
    AssertEqual(1, missileProjectiles.ActiveTrailCount,
        "missile allocates one persistent trail every fourth alpha pass");
    SamusProjectileTrailSlot missileTrail =
        missileProjectiles.TrailSlots[SamusProjectileSystem.TrailSlotCount - 1];
    missileOam.BeginFrame();
    missileProjectiles.HandleTrailsAndDraw(bus, missileOam, 0, 0, timeIsFrozen: false);
    AssertEqual(4, missileOam.NextByteOffset, "missile left trail emits one small OBJ");
    AssertEqual(0x048, missileOam.GetEntry(0).TileNumber,
        "missile trail starts at retail tile `$2A48`");
    AssertEqual((ushort)4, missileTrail.Left.InstructionTimer,
        "missile trail retains its four-frame record duration");
    AssertEqual((ushort)0, missileTrail.Right.InstructionTimer,
        "missile's empty right trail stream terminates immediately");

    SamusProjectileFrameResult missileImpact = default;
    for (int frame = 0; frame < 32 && !missileImpact.CollisionStartedExplosion; frame++)
    {
        missileBombs.StepFrame(bus, wall, missileSamus, 0, 0);
        missileImpact = missileProjectiles.StepFrame(
            bus, wall, missileSamus, 0, 0, 0, 0, missileBombs);
    }
    AssertTrue(missileImpact.CollisionStartedExplosion,
        "accelerating missile reaches the type-eight wall");
    AssertEqual((ushort)0x0800, unchecked((ushort)(missile.Type & 0x0f00)),
        "missile collision installs missile-explosion family `$0800`");
    AssertEqual((ushort)1, missileProjectiles.ProjectileCounter,
        "missile explosion retains its shared ordinary slot count");
    AssertEqual((ushort)0xa010, missile.SpritemapPointer,
        "missile collision frame selects first explosion art");
    missileOam.BeginFrame();
    missileProjectiles.DrawExplosions(bus, missileOam, 0, 0);
    AssertEqual(4, missileOam.NextByteOffset,
        "missile explosion participates in the early explosion draw pass");

    for (int frame = 0; frame < 2; frame++)
    {
        missileBombs.StepFrame(bus, wall, missileSamus, 0, 0);
        missileProjectiles.StepFrame(bus, wall, missileSamus, 0, 0, 0, 0, missileBombs);
    }
    AssertEqual((ushort)0, missileProjectiles.ProjectileCounter,
        "missile explosion delete decrements ordinary counter");
    AssertTrue(!missile.IsActive, "missile explosion delete clears its slot");

    // Point missiles use a deliberately different slope route from radius-spanning beams.
    // Put the muzzle directly inside one synthetic type-one block and compare points above
    // and inside the exact cartridge height. This locks `$94:A58F`'s shape-row indexing and
    // `height <= y` comparison independently of the Landing Site cartridge smoke test.
    bus.WriteBytes(0x948b2b + 0x12 * 16, [
        0x10, 0x0f, 0x0e, 0x0d, 0x0c, 0x0b, 0x0a, 0x09,
        0x08, 0x07, 0x06, 0x05, 0x04, 0x03, 0x02, 0x01,
    ]);
    bus.WriteBytes(0x948e54, [0x00, 0x00, 0x80, 0x80]);

    RoomLevelData BuildPointSlopeRoom(byte behavior)
    {
        var words = new ushort[width * height];
        var behaviors = new byte[words.Length];
        int muzzleBlock = 6 * width + 4;
        words[muzzleBlock] = 0x1000;
        behaviors[muzzleBlock] = behavior;
        return new RoomLevelData(
            width,
            height,
            words,
            behaviors,
            new ushort[words.Length],
            new byte[8]);
    }

    SamusProjectileFrameResult FirePointMissile(RoomLevelData terrain, ushort yPosition)
    {
        var pointSamus = new SamusState
        {
            Pose = rightPose,
            XPosition = 64,
            YPosition = yPosition,
            SelectedHudItem = 1,
            Missiles = 1,
        };
        var pointBombs = new SamusBombProjectileSystem();
        var pointProjectiles = new SamusProjectileSystem();
        pointBombs.StepFrame(bus, terrain, pointSamus, 0, 0);
        return pointProjectiles.StepFrame(
            bus,
            terrain,
            pointSamus,
            (ushort)SnesButton.X,
            (ushort)SnesButton.X,
            0,
            0,
            pointBombs);
    }

    RoomLevelData nonSquareSlope = BuildPointSlopeRoom(0x12);
    AssertTrue(!FirePointMissile(nonSquareSlope, 96).CollisionStartedExplosion,
        "non-square point above ROM height remains air");
    AssertTrue(FirePointMissile(nonSquareSlope, 111).CollisionStartedExplosion,
        "non-square point at ROM height collides");

    // Shape zero is the retail half-height square: top-left/top-right are air and both
    // bottom quadrants are solid. These two centers differ only in bit three of Y, proving
    // `$94:A66A`'s perpendicular quadrant XOR used during horizontal missile movement.
    RoomLevelData squareSlope = BuildPointSlopeRoom(0x00);
    AssertTrue(!FirePointMissile(squareSlope, 96).CollisionStartedExplosion,
        "square-slope top half remains air");
    AssertTrue(FirePointMissile(squareSlope, 104).CollisionStartedExplosion,
        "square-slope bottom half collides");

    // Super Missiles share `$BE62` but differ in every animation-adjacent constant: HUD item
    // two, type `$8200`, sound four, cooldown twenty, `$012C` damage, acceleration `$0100`,
    // two-frame exhaust after the initial delay, an invisible linked slot, and a larger quake-
    // producing explosion. Keep those distinctions together so a speed-only implementation
    // cannot satisfy the regression.
    var superSamus = new SamusState
    {
        Pose = rightPose,
        XPosition = 64,
        YPosition = 96,
        SelectedHudItem = 2,
        SuperMissiles = 3,
    };
    var superBombs = new SamusBombProjectileSystem();
    var superProjectiles = new SamusProjectileSystem();
    superBombs.StepFrame(bus, wall, superSamus, 0, 0);
    SamusProjectileFrameResult superFired = superProjectiles.StepFrame(
        bus,
        wall,
        superSamus,
        (ushort)SnesButton.X,
        (ushort)SnesButton.X,
        0,
        0,
        superBombs);
    SamusProjectileSlot super = superProjectiles.Slots[0];
    SamusProjectileSlot superLink = superProjectiles.Slots[1];
    AssertEqual((int?)0, superFired.FiredSlot, "super fresh press allocates owner slot zero");
    AssertEqual((ushort)4, superFired.QueuedSoundEffect,
        "super producer queues library-one effect four");
    AssertEqual((ushort)2, superSamus.SuperMissiles,
        "super producer consumes exactly one ammo");
    AssertEqual((ushort)20, superBombs.CooldownTimer,
        "super producer installs literal twenty-frame cooldown");
    AssertEqual((ushort)2, superProjectiles.ProjectileCounter,
        "super ignition counts visible owner plus invisible link");
    AssertEqual((ushort)0x8200, super.Type, "super owner uses active type `$8200`");
    AssertEqual((ushort)0x012c, super.Damage, "super owner reads retail 300 damage");
    AssertEqual((short)0x0100, super.XVelocity,
        "super ignition begins at one pixel per frame");
    AssertEqual((ushort)0x0102, super.Variable,
        "super variable combines initialized high byte and link byte index two");
    AssertEqual(SamusProjectilePreInstruction.SuperMissile, super.PreInstruction,
        "super owner selects `$90:AFE5`");
    AssertEqual((ushort)0x8200, superLink.Type, "super link retains family `$0200`");
    AssertEqual((ushort)0x012c, superLink.Damage, "super link carries native damage sentinel");
    AssertEqual(SamusProjectilePreInstruction.SuperMissileLink, superLink.PreInstruction,
        "super link selects stationary `$90:B075`");
    AssertEqual(super.XPosition, superLink.XPosition,
        "slow horizontal link follows owner center after ignition movement");

    // The link instruction list is intentionally invisible; only the owner contributes an
    // OBJ after its own bank-$93 program selects `$A030`.
    var superOam = new OamBuffer();
    superOam.BeginFrame();
    superProjectiles.DrawLiveProjectiles(bus, superOam, 0, 0, nmiFrameCounter: 0);
    AssertEqual(4, superOam.NextByteOffset, "super owner draws while link spritemap is empty");
    AssertEqual(0x045, superOam.GetEntry(0).TileNumber,
        "super owner consumes its distinct `$2A45` OBJ");

    for (int frame = 0; frame < 3; frame++)
    {
        superBombs.StepFrame(bus, wall, superSamus, 0, 0);
        superProjectiles.StepFrame(bus, wall, superSamus, 0, 0, 0, 0, superBombs);
    }
    AssertEqual(1, superProjectiles.ActiveTrailCount,
        "super's initial four-count allocates first exhaust trail");
    AssertEqual((ushort)2, super.TrailTimer,
        "super exhaust reloads two instead of missile four");

    SamusProjectileFrameResult superImpact = default;
    for (int frame = 0; frame < 24 && !superImpact.CollisionStartedExplosion; frame++)
    {
        superBombs.StepFrame(bus, wall, superSamus, 0, 0);
        superImpact = superProjectiles.StepFrame(
            bus, wall, superSamus, 0, 0, 0, 0, superBombs);
    }
    AssertTrue(superImpact.CollisionStartedExplosion,
        "accelerating super reaches the type-eight wall");
    AssertEqual((ushort)0x8800, super.Type,
        "super impact preserves active bit and selects family `$0800`");
    AssertEqual((ushort)0x9348, super.InstructionPointer,
        "super collision consumes first record of `$93:9340` explosion fixture");
    AssertEqual((ushort)20, superProjectiles.EarthquakeType,
        "super impact publishes quake type `$14`");
    AssertEqual((ushort)30, superProjectiles.EarthquakeTimer,
        "super impact publishes thirty-frame quake timer");
    AssertTrue(!superLink.IsActive, "super owner impact clears invisible linked slot");
    AssertEqual((ushort)1, superProjectiles.ProjectileCounter,
        "super explosion retains only its visible owner count");

    // Drive a second real Super Missile into type-$C/BTS-A rather than calling the PLM
    // owner directly. `$90:B00E`'s invisible linked point probe and the visible owner both
    // carry family `$0200`; whichever reaches the column first must publish `$84:D08C`, run
    // CF67 synchronously, and leave `$809F` behind for the normal same-frame PLM pass.
    var integratedSuperWords = new ushort[width * height];
    var integratedSuperBehaviors = new byte[integratedSuperWords.Length];
    for (int y = 0; y < height; y++)
    {
        int index = y * width + 6;
        integratedSuperWords[index] = 0xc000;
        integratedSuperBehaviors[index] = 10;
    }
    RoomLevelData integratedSuperWall = new(
        width,
        height,
        integratedSuperWords,
        integratedSuperBehaviors,
        new ushort[integratedSuperWords.Length],
        new byte[8]);
    var integratedSuperSamus = new SamusState
    {
        Pose = rightPose,
        XPosition = 64,
        YPosition = 96,
        SelectedHudItem = 2,
        SuperMissiles = 1,
    };
    var integratedSuperBombs = new SamusBombProjectileSystem();
    var integratedSuperProjectiles = new SamusProjectileSystem();
    var integratedSuperPlms = new RoomPlmSystem();
    SamusProjectileFrameResult integratedSuperResult = default;
    for (int frame = 0; frame < 32 && !integratedSuperResult.CollisionStartedExplosion; frame++)
    {
        integratedSuperBombs.StepFrame(
            bus, integratedSuperWall, integratedSuperSamus, 0, 0);
        integratedSuperResult = integratedSuperProjectiles.StepFrame(
            bus,
            integratedSuperWall,
            integratedSuperSamus,
            frame == 0 ? (ushort)SnesButton.X : (ushort)0,
            frame == 0 ? (ushort)SnesButton.X : (ushort)0,
            0,
            0,
            integratedSuperBombs,
            roomPlms: integratedSuperPlms);
    }
    AssertTrue(integratedSuperPlms.ActiveCount > 0,
        "live Super Missile publishes weapon-gated block PLM");
    int integratedSuperMutations = 0;
    for (int y = 0; y < height; y++)
    {
        if (integratedSuperWall.GetCollisionBlock(6, y).LevelWord == 0x809f)
            integratedSuperMutations++;
    }
    AssertTrue(integratedSuperMutations > 0,
        "live Super Missile collision runs CF67 and synthesizes $809F");

    Console.WriteLine(
        "  Samus beams/missiles: producers, charge flare, linked supers, trails, shootable-block PLMs, motion, collision, and explosions agree.");
}

/// <summary>
/// Exercises ordinary Morph-Ball entry, `$F9` endpoint selection, rolling momentum,
/// walk-off, both automatic rebounds, grounded recovery, and blocked unmorph expansion.
/// Every table byte below is copied from the corresponding retail-ROM structure rather
/// than replaced with a host animation or physics constant.
/// </summary>
static void VerifySamusMorphBallMovement()
{
    var bus = new TestAddressSpace();

    // Pose definitions `$91:B711-$91:B819/$91:B831`. Only fields consumed by the current
    // slice are populated, but the complete eight-byte records make direction, movement,
    // fallback, graphics offset, and collision radius independently observable.
    void WritePose(byte pose, byte[] definition) =>
        bus.WriteBytes(0x91b629 + pose * 8, definition);
    WritePose(SamusState.CrouchingRightPose, [0x08, 0x05, 0x27, 0x02, 0x00, 0x00, 0x10, 0x00]);
    WritePose(SamusState.CrouchingLeftPose, [0x04, 0x05, 0x28, 0x07, 0x00, 0x00, 0x10, 0x00]);
    WritePose(SamusState.MorphBallGroundRightPose, [0x08, 0x04, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusState.MorphBallMovingRightPose, [0x08, 0x04, 0x1d, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusState.MorphBallMovingLeftPose, [0x04, 0x04, 0x41, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusState.MorphBallFallingRightPose, [0x08, 0x08, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusState.MorphBallFallingLeftPose, [0x04, 0x08, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusState.MorphingTransitionRightPose, [0x08, 0x0f, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusState.MorphingTransitionLeftPose, [0x04, 0x0f, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusState.UnmorphingTransitionRightPose, [0x08, 0x0f, 0xff, 0xff, 0x00, 0x00, 0x10, 0x00]);
    WritePose(SamusState.UnmorphingTransitionLeftPose, [0x04, 0x0f, 0xff, 0xff, 0x00, 0x00, 0x10, 0x00]);
    WritePose(SamusState.MorphBallGroundLeftPose, [0x04, 0x04, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusState.SpringBallGroundRightPose, [0x08, 0x11, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusState.SpringBallGroundLeftPose, [0x04, 0x11, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusState.SpringBallMovingRightPose, [0x08, 0x11, 0x79, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusState.SpringBallMovingLeftPose, [0x04, 0x11, 0x7a, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusState.SpringBallFallingRightPose, [0x08, 0x13, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusState.SpringBallFallingLeftPose, [0x04, 0x13, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusState.SpringBallJumpRightPose, [0x08, 0x12, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);
    WritePose(SamusState.SpringBallJumpLeftPose, [0x04, 0x12, 0xff, 0xff, 0x00, 0x00, 0x07, 0x00]);

    // Stable ordinary-ball poses all point to `$91:B378`. Separate synthetic storage keeps
    // the production pointer lookup real while making the expected command stream concise.
    const ushort sharedBallDelay = 0xc400;
    foreach (byte pose in new byte[] {
        SamusState.MorphBallGroundRightPose,
        SamusState.MorphBallMovingRightPose,
        SamusState.MorphBallMovingLeftPose,
        SamusState.MorphBallFallingRightPose,
        SamusState.MorphBallFallingLeftPose,
        SamusState.MorphBallGroundLeftPose,
        SamusState.SpringBallGroundRightPose,
        SamusState.SpringBallGroundLeftPose,
        SamusState.SpringBallMovingRightPose,
        SamusState.SpringBallMovingLeftPose,
        SamusState.SpringBallFallingRightPose,
        SamusState.SpringBallFallingLeftPose,
        SamusState.SpringBallJumpRightPose,
        SamusState.SpringBallJumpLeftPose,
    })
    {
        WriteTestWord(bus, 0x91b010 + pose * 2, sharedBallDelay);
    }
    bus.WriteBytes(0x910000 | sharedBallDelay, [0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0x02, 0xff]);

    // `$37/$38`: two visible frames, then `$F9 $0002 ground air springGround springAir`.
    WriteTestWord(bus, 0x91b010 + SamusState.MorphingTransitionRightPose * 2, 0xc420);
    WriteTestWord(bus, 0x91b010 + SamusState.MorphingTransitionLeftPose * 2, 0xc430);
    bus.WriteBytes(0x91c420, [0x02, 0x02, 0xf9, 0x02, 0x00, 0x1d, 0x31, 0x79, 0x7d]);
    bus.WriteBytes(0x91c430, [0x02, 0x02, 0xf9, 0x02, 0x00, 0x41, 0x32, 0x7a, 0x7e]);

    // `$3D/$3E` finish through the already translated `$FD pp` command-three seam.
    WriteTestWord(bus, 0x91b010 + SamusState.UnmorphingTransitionRightPose * 2, 0xc440);
    WriteTestWord(bus, 0x91b010 + SamusState.UnmorphingTransitionLeftPose * 2, 0xc450);
    WriteTestWord(bus, 0x91b010 + SamusState.CrouchingRightPose * 2, 0xc460);
    WriteTestWord(bus, 0x91b010 + SamusState.CrouchingLeftPose * 2, 0xc470);
    bus.WriteBytes(0x91c440, [0x02, 0x02, 0xfd, 0x27]);
    bus.WriteBytes(0x91c450, [0x02, 0x02, 0xfd, 0x28]);
    bus.WriteBytes(0x91c460, [0x10, 0xff]);
    bus.WriteBytes(0x91c470, [0x10, 0xff]);

    // Normal-air movement type four receives visible acceleration and deceleration words;
    // type eight remains zero horizontally for isolated bounce assertions.
    bus.WriteBytes(0x909f85, [
        0x00, 0x00, 0x00, 0x80, // acceleration 0.8000
        0x01, 0x00, 0x00, 0x00, // maximum 1.0000
        0x00, 0x00, 0x00, 0x40, // deceleration 0.4000
    ]);
    bus.WriteBytes(0x909f55 + 0x11 * 12, [
        0x00, 0x00, 0x00, 0x80, // Spring-ground acceleration 0.8000
        0x01, 0x00, 0x00, 0x00, // maximum 1.0000
        0x00, 0x00, 0x00, 0x40, // deceleration 0.4000
    ]);
    bus.WriteBytes(0x909eb9, [0x04, 0x00]); // Dry-air jump whole speed 4.
    bus.WriteBytes(0x909ebf, [0x00, 0xe0]); // Dry-air jump subspeed E000.
    bus.WriteBytes(0x909ea1, [0x00, 0x28]); // Dry-air gravity subspeed 2800.
    bus.WriteBytes(0x909ea7, [0x00, 0x00]); // Dry-air gravity whole word.
    bus.WriteBytes(0x909eb5, [0x01, 0x00]); // Whole bounce speed.
    bus.WriteBytes(0x909eb7, [0x00, 0x10]); // Fractional bounce speed.

    // Bomb jumps do not use the normal movement-type speed table. `$90:8EF4` passes
    // the standalone `$90:9F25` record directly to `$90:9A7E`, while `$90:9A2C`
    // reads the dry-air bomb-jump magnitude from `$90:9EF5/$90:9EFB`.
    bus.WriteBytes(0x909f25, [
        0x01, 0x00, 0x00, 0x00, // diagonal acceleration 1.0000
        0x02, 0x00, 0x00, 0x00, // diagonal maximum 2.0000
        0x00, 0x00, 0x00, 0x80, // post-apex deceleration 0.8000
    ]);
    bus.WriteBytes(0x909ef5, [0x02, 0x00]); // Bomb-jump whole speed 2.
    bus.WriteBytes(0x909efb, [0x00, 0xc0]); // Bomb-jump subspeed C000.

    const int width = 8;
    const int height = 8;
    var floorBlocks = new ushort[width * height];
    for (int x = 0; x < width; x++)
        floorBlocks[4 * width + x] = 0x8000;
    RoomLevelData floor = new(
        width,
        height,
        floorBlocks,
        new byte[floorBlocks.Length],
        new ushort[floorBlocks.Length],
        new byte[8]);

    var samus = new SamusState
    {
        Pose = SamusState.CrouchingRightPose,
        XPosition = 48,
        YPosition = 48,
    };
    samus.RefreshCollisionRadii(bus);
    samus.InitializeAnimation(bus);

    AssertTrue(
        !samus.TryApplyMorphTransition(
            bus, floor, SamusState.MorphingTransitionRightPose, nmiFrameCounter: 0),
        "morph entry is rejected without item bit $0004");
    AssertEqual(SamusState.CrouchingRightPose, samus.Pose, "rejected morph retains crouch");
    AssertEqual((ushort)48, samus.YPosition, "rejected morph retains center Y");

    samus.EquippedItems = 0x0004;
    AssertTrue(
        samus.TryApplyMorphTransition(
            bus, floor, SamusState.MorphingTransitionRightPose, nmiFrameCounter: 0),
        "equipped Morph Ball begins entry transition");
    AssertEqual((ushort)7, samus.Kinematics.YRadius, "morph transition radius from ROM");
    AssertEqual((ushort)57, samus.YPosition, "command seven moves center down nine");

    // Delay 2 at frame zero, delay 2 at frame one, then command F9 at frame two.
    for (int tick = 0; tick < 4; tick++)
        samus.AnimateNoFx(bus);
    AssertEqual((byte)0xf9, samus.LastAnimationDelayCommand!.Value, "morph transition reaches F9");
    AssertEqual(SamusState.MorphBallGroundRightPose, samus.PendingTransitionalPose!.Value,
        "F9 selects no-spring grounded endpoint");
    AssertTrue(samus.ApplyPendingVerifiedAnimationTransition(bus), "grounded F9 endpoint applies");
    AssertEqual(SamusState.MorphBallGroundRightPose, samus.Pose, "morph entry reaches stable ball");

    // Changing to the moving record preserves the shared rolling frame/timer. Movement
    // type four then reads its literal 0.8000 acceleration and remains floor-constrained.
    ushort timerBeforeRoll = samus.AnimationFrameTimer;
    samus.ApplyMorphBallPoseChange(bus, SamusState.MorphBallMovingRightPose);
    AssertEqual(timerBeforeRoll, samus.AnimationFrameTimer, "ball direction change preserves timer");
    MorphBallMovementResult rolling = SamusMorphBallMovement.StepGrounded(
        bus, floor, samus, nmiFrameCounter: 0);
    AssertTrue(rolling.Vertical.Collided, "rolling ball retains floor contact");
    AssertEqual((ushort)0x8000, samus.HorizontalSpeed.BaseSubspeed, "type-four acceleration uses ROM table");

    samus.ApplyMorphBallPoseChange(bus, SamusState.MorphBallMovingLeftPose);
    AssertEqual((ushort)1, samus.HorizontalSpeed.AccelerationMode, "ball reversal selects momentum mode one");
    AssertEqual((ushort)0x8000, samus.HorizontalSpeed.BaseSubspeed, "ball reversal preserves base magnitude");

    // Remove the floor and execute the stable grounded handler. Its failed +1 probe drives
    // the explicit `$1D/$41 -> $31/$32` walk-off transition and starts downward gravity.
    RoomLevelData empty = new(
        width,
        height,
        new ushort[width * height],
        new byte[width * height],
        new ushort[width * height],
        new byte[8]);
    samus.ApplyMorphBallPoseChange(bus, SamusState.MorphBallGroundLeftPose);
    samus.HorizontalSpeed.AccelerationMode = 0;
    MorphBallMovementResult unsupported = SamusMorphBallMovement.StepGrounded(
        bus, empty, samus, nmiFrameCounter: 1);
    AssertTrue(!unsupported.Vertical.Collided, "grounded ball detects missing floor");
    samus.ApplyMorphBallWalkOff(bus);
    AssertEqual(SamusState.MorphBallFallingLeftPose, samus.Pose, "left ball walk-off endpoint");
    AssertEqual((ushort)2, samus.Kinematics.YDirection, "ball walk-off starts falling");

    // Put the airborne body three pixels above the floor and give it a hard downward
    // magnitude. The collision launches bounce one with the two constants at `$90:9EB5`.
    samus.XPosition = 48;
    samus.YPosition = 55;
    samus.Kinematics.YDirection = 2;
    samus.Kinematics.YSpeed = 3;
    samus.Kinematics.YSubspeed = 0;
    samus.Kinematics.YAcceleration = 0;
    samus.Kinematics.YSubacceleration = 0;
    MorphBallMovementResult hardLanding = SamusMorphBallMovement.StepFalling(
        bus, floor, samus, controllerInput: 0, nmiFrameCounter: 0);
    AssertTrue(hardLanding.Landed, "hard ball fall collides with floor");
    AssertTrue(!samus.ApplyMorphBallLanding(bus), "hard landing launches first rebound");
    AssertEqual((ushort)1, samus.MorphBallBounceState, "first rebound state");
    AssertEqual((ushort)1, samus.Kinematics.YSpeed, "first rebound whole speed from ROM");
    AssertEqual((ushort)0x1000, samus.Kinematics.YSubspeed, "first rebound subspeed from ROM");
    AssertEqual((ushort)1, samus.Kinematics.YDirection, "first rebound moves upward");

    // Isolate the two later collision handlers at the exact floor boundary. State one
    // launches the smaller second rebound; state two finally installs stable ground art.
    samus.YPosition = 57;
    samus.Kinematics.YDirection = 2;
    samus.Kinematics.YSpeed = 1;
    samus.Kinematics.YSubspeed = 0;
    AssertTrue(!samus.ApplyMorphBallLanding(bus), "first-bounce collision launches second rebound");
    AssertEqual((ushort)2, samus.MorphBallBounceState, "second rebound state");
    AssertEqual((ushort)0, samus.Kinematics.YSpeed, "second rebound decrements whole constant");

    samus.Kinematics.YDirection = 2;
    samus.Kinematics.YSpeed = 1;
    AssertTrue(samus.ApplyMorphBallLanding(bus), "second-bounce collision grounds ball");
    AssertEqual(SamusState.MorphBallGroundLeftPose, samus.Pose, "bounce recovery uses facing-left ground pose");
    AssertEqual((ushort)0, samus.MorphBallBounceState, "grounding clears bounce state");

    // Unmorphing against only the floor succeeds and moves center up nine, preserving the
    // bottom boundary. A ceiling in row two makes both initial expansion probes collide;
    // radius-seven `$91:FFA7` must retain the ball instead of selecting crouch.
    AssertTrue(
        samus.TryApplyMorphTransition(
            bus, floor, SamusState.UnmorphingTransitionLeftPose, nmiFrameCounter: 0),
        "floor-constrained unmorph succeeds");
    AssertEqual((ushort)16, samus.Kinematics.YRadius, "unmorph transition radius");
    AssertEqual((ushort)48, samus.YPosition, "unmorph expansion keeps bottom boundary");

    var tunnelBlocks = (ushort[])floorBlocks.Clone();
    for (int x = 0; x < width; x++)
        tunnelBlocks[2 * width + x] = 0x8000;
    RoomLevelData tunnel = new(
        width,
        height,
        tunnelBlocks,
        new byte[tunnelBlocks.Length],
        new ushort[tunnelBlocks.Length],
        new byte[8]);
    var boxedBall = new SamusState
    {
        Pose = SamusState.MorphBallGroundRightPose,
        EquippedItems = 0x0004,
        XPosition = 48,
        YPosition = 57,
    };
    boxedBall.RefreshCollisionRadii(bus);
    boxedBall.InitializeAnimation(bus);
    AssertTrue(
        !boxedBall.TryApplyMorphTransition(
            bus, tunnel, SamusState.UnmorphingTransitionRightPose, nmiFrameCounter: 1),
        "boxed Morph Ball rejects unmorph");
    AssertEqual(SamusState.MorphBallGroundRightPose, boxedBall.Pose, "boxed unmorph retains ball pose");
    AssertEqual((ushort)7, boxedBall.Kinematics.YRadius, "boxed unmorph retains ball radius");
    AssertEqual((ushort)57, boxedBall.YPosition, "boxed unmorph retains center");

    // Re-run entry with a nonzero vertical word to prove F9 uses its airborne operand, not
    // current collision radius or pose name. Spring Ball remains unequipped, selecting $31.
    var airborneEntry = new SamusState
    {
        Pose = SamusState.CrouchingRightPose,
        EquippedItems = 0x0004,
        XPosition = 48,
        YPosition = 48,
    };
    airborneEntry.RefreshCollisionRadii(bus);
    airborneEntry.InitializeAnimation(bus);
    AssertTrue(
        airborneEntry.TryApplyMorphTransition(
            bus, floor, SamusState.MorphingTransitionRightPose, nmiFrameCounter: 0),
        "airborne F9 fixture begins morph");
    airborneEntry.Kinematics.YSubspeed = 1;
    for (int tick = 0; tick < 4; tick++)
        airborneEntry.AnimateNoFx(bus);
    AssertEqual(SamusState.MorphBallFallingRightPose, airborneEntry.PendingTransitionalPose!.Value,
        "F9 nonzero Y subspeed selects airborne endpoint");

    // Repeat entry with Spring Ball bit `$0002`. F9 must use its equipped grounded operand,
    // then movement type `$11` reads its own speed record rather than ordinary type four.
    var spring = new SamusState
    {
        Pose = SamusState.CrouchingRightPose,
        EquippedItems = 0x0006,
        XPosition = 48,
        YPosition = 48,
    };
    spring.RefreshCollisionRadii(bus);
    spring.InitializeAnimation(bus);
    AssertTrue(
        spring.TryApplyMorphTransition(
            bus, floor, SamusState.MorphingTransitionRightPose, nmiFrameCounter: 0),
        "Spring Ball fixture begins morph entry");
    for (int tick = 0; tick < 4; tick++)
        spring.AnimateNoFx(bus);
    AssertEqual(SamusState.SpringBallGroundRightPose, spring.PendingTransitionalPose!.Value,
        "F9 equipped endpoint selects Spring Ball ground");
    AssertTrue(spring.ApplyPendingVerifiedAnimationTransition(bus), "Spring Ball F9 applies");
    spring.ApplyMorphBallPoseChange(bus, SamusState.SpringBallMovingRightPose);
    MorphBallMovementResult springRoll = SamusMorphBallMovement.StepGrounded(
        bus, floor, spring, nmiFrameCounter: 0);
    AssertTrue(springRoll.Vertical.Collided, "Spring Ball roll retains floor contact");
    AssertEqual((ushort)0x8000, spring.HorizontalSpeed.BaseSubspeed,
        "type-$11 acceleration uses its ROM record");

    // `$79 -> $7F` initializes the literal dry-air 4.E000 jump. Releasing Jump on its
    // first movement frame invokes the shared variable-height cutoff before displacement.
    spring.ApplyMorphBallPoseChange(bus, SamusState.SpringBallGroundRightPose);
    spring.ApplySpringBallJump(bus, SamusState.SpringBallJumpRightPose);
    AssertEqual((ushort)4, spring.Kinematics.YSpeed, "Spring Ball launch whole speed");
    AssertEqual((ushort)0xe000, spring.Kinematics.YSubspeed, "Spring Ball launch subspeed");
    SamusMorphBallMovement.StepSpringBallInAir(
        bus, empty, spring, controllerInput: 0, nmiFrameCounter: 0);
    AssertEqual((ushort)2, spring.Kinematics.YDirection, "released Spring Ball jump cuts upward arc");

    // The no-Jump hard impact stores the distinctive `$0601` state. A held-Jump impact
    // instead clears that state and immediately relaunches through Make_Samus_Jump.
    spring.ApplyMorphBallPoseChange(bus, SamusState.SpringBallFallingRightPose);
    spring.Kinematics.YSpeed = 3;
    spring.Kinematics.YSubspeed = 0;
    spring.Kinematics.YDirection = 2;
    AssertTrue(!spring.ApplySpringBallLanding(bus, controllerInput: 0),
        "Spring Ball hard impact rebounds");
    AssertEqual((ushort)0x0601, spring.MorphBallBounceState, "Spring Ball first bounce state");
    AssertTrue(!spring.ApplySpringBallLanding(bus, (ushort)SnesButton.A),
        "held Jump immediately relaunches Spring Ball");
    AssertEqual((ushort)0, spring.MorphBallBounceState, "held-Jump relaunch clears bounce state");
    AssertEqual(SamusState.SpringBallJumpRightPose, spring.Pose, "held-Jump relaunch pose");

    // Bank-$93 projectile fixtures copied byte-for-byte from the normal-bomb pointer/data
    // records and its slow, fast, and explosion instruction lists. The production code
    // must follow these pointers; no test-facing constructor is allowed to inject damage,
    // radii, frame durations, or animation endpoints directly into a slot.
    WriteTestWord(bus, 0x9383fb, 0x8675); // Non-beam type five -> normal bomb data.
    bus.WriteBytes(0x938675, [0x1e, 0x00, 0xbf, 0x9f]);
    WriteTestWord(bus, 0x938683, 0xa06b); // Bomb-explosion instruction pointer.
    bus.WriteBytes(0x939fbf, [
        0x05, 0x00, 0x45, 0xad, 0x04, 0x04, 0x00, 0x00,
        0x05, 0x00, 0x4c, 0xad, 0x04, 0x04, 0x00, 0x00,
        0x05, 0x00, 0x53, 0xad, 0x04, 0x04, 0x00, 0x00,
        0x05, 0x00, 0x5a, 0xad, 0x04, 0x04, 0x00, 0x00,
        0x39, 0x82, 0xbf, 0x9f,
    ]);
    bus.WriteBytes(0x939fe3, [
        0x01, 0x00, 0x45, 0xad, 0x04, 0x04, 0x00, 0x00,
        0x01, 0x00, 0x4c, 0xad, 0x04, 0x04, 0x00, 0x00,
        0x01, 0x00, 0x53, 0xad, 0x04, 0x04, 0x00, 0x00,
        0x01, 0x00, 0x5a, 0xad, 0x04, 0x04, 0x00, 0x00,
        0x39, 0x82, 0xe3, 0x9f,
    ]);
    bus.WriteBytes(0x93a06b, [
        0x02, 0x00, 0x3e, 0xa8, 0x08, 0x08, 0x00, 0x00,
        0x02, 0x00, 0x54, 0xa8, 0x0c, 0x0c, 0x00, 0x00,
        0x02, 0x00, 0x6a, 0xa8, 0x10, 0x10, 0x00, 0x00,
        0x02, 0x00, 0x80, 0xa8, 0x10, 0x10, 0x00, 0x00,
        0x02, 0x00, 0x96, 0xa8, 0x10, 0x10, 0x00, 0x00,
        0x2f, 0x82,
    ]);

    // Power-bomb type three follows its own literal pointer/data record and three-frame
    // slow/fast loops. These bytes are ROM `$93:83F7/$8671/$9F87-$9FBE`; in particular,
    // damage `$00C8` and pointer `$9F87` are not host-selected stand-ins.
    WriteTestWord(bus, 0x9383f7, 0x8671);
    bus.WriteBytes(0x938671, [0xc8, 0x00, 0x87, 0x9f]);
    bus.WriteBytes(0x939f87, [
        0x05, 0x00, 0x97, 0xab, 0x04, 0x04, 0x00, 0x00,
        0x05, 0x00, 0x9e, 0xab, 0x04, 0x04, 0x00, 0x00,
        0x05, 0x00, 0xa5, 0xab, 0x04, 0x04, 0x00, 0x00,
        0x39, 0x82, 0x87, 0x9f,
    ]);
    bus.WriteBytes(0x939fa3, [
        0x01, 0x00, 0x97, 0xab, 0x04, 0x04, 0x00, 0x00,
        0x01, 0x00, 0x9e, 0xab, 0x04, 0x04, 0x00, 0x00,
        0x01, 0x00, 0xa5, 0xab, 0x04, 0x04, 0x00, 0x00,
        0x39, 0x82, 0xa3, 0x9f,
    ]);

    // `$88:9079` and `$88:8D85` are the fixed-color tables indexed by the high byte of
    // the two authentic 8.8 radii. Keeping the retail bytes makes phase/color assertions
    // detect an incorrect radius update as well as a merely incorrect phase enum.
    bus.WriteBytes(0x889079, [
        0x10, 0x10, 0x10, 0x04, 0x04, 0x04, 0x06, 0x06, 0x06,
        0x08, 0x08, 0x08, 0x0a, 0x0a, 0x0a, 0x0c, 0x0c, 0x0c,
        0x0e, 0x0e, 0x0a, 0x10, 0x10, 0x08, 0x12, 0x12, 0x08,
        0x14, 0x14, 0x08, 0x16, 0x16, 0x08, 0x18, 0x18, 0x08,
        0x1a, 0x1a, 0x0a, 0x18, 0x18, 0x08, 0x16, 0x16, 0x06,
        0x14, 0x14, 0x04,
    ]);
    bus.WriteBytes(0x888d85, [
        0x0e, 0x0e, 0x0a, 0x0f, 0x0f, 0x09, 0x10, 0x10, 0x08,
        0x11, 0x11, 0x07, 0x12, 0x12, 0x06, 0x13, 0x13, 0x05,
        0x14, 0x14, 0x04, 0x15, 0x15, 0x03, 0x16, 0x16, 0x02,
        0x17, 0x17, 0x01, 0x18, 0x18, 0x00, 0x19, 0x19, 0x00,
        0x1a, 0x1a, 0x00, 0x1a, 0x1a, 0x00, 0x1a, 0x1a, 0x1a,
        0x1a, 0x1a, 0x1a, 0x1b, 0x1b, 0x1b,
    ]);

    // The bombable-terrain integration below intentionally supplies the literal bank-$84
    // BTS-zero reaction head and complete shared 1x1 respawn tail. `$84:CEDA` advances a
    // normal bomb's live pointer by three, so the leading `$8C46,$0A` sound instruction is
    // present in ROM but must be skipped. This makes a mistaken collision-list substitution
    // or invented host-side timer immediately observable.
    bus.WriteBytes(0x84cc3c, [
        0x46, 0x8c, 0x0a,       // Reaction-only sound $0A, skipped by normal bomb setup.
        0x04, 0x00, 0x45, 0xa3, // Four frames: visual block $053.
        0x04, 0x00, 0x4b, 0xa3, // Four frames: visual block $054.
        0x04, 0x00, 0x51, 0xa3, // Four frames: visual block $055.
        0x80, 0x01, 0x57, 0xa3, // 384 frames: blank-air visual block $0FF.
        0x04, 0x00, 0x51, 0xa3, // Reverse animation: $055.
        0x04, 0x00, 0x4b, 0xa3, // Reverse animation: $054.
        0x04, 0x00, 0x45, 0xa3, // Reverse animation: $053.
        0x17, 0x8b,             // DrawPLMBlock restores PLM_Vars and seeds timer one.
        0xbc, 0x86,             // Delete on the following PLM handler pass.
    ]);
    bus.WriteBytes(0x84a345, [0x01, 0x00, 0x53, 0x00, 0x00, 0x00]);
    bus.WriteBytes(0x84a34b, [0x01, 0x00, 0x54, 0x00, 0x00, 0x00]);
    bus.WriteBytes(0x84a351, [0x01, 0x00, 0x55, 0x00, 0x00, 0x00]);
    bus.WriteBytes(0x84a357, [0x01, 0x00, 0xff, 0x00, 0x00, 0x00]);

    // Additional bomb-reaction lists used later in this method. These bytes are literal
    // transcriptions of `$84:CADF`, `$84:C91C`, `$84:C922`, `$84:C8FE`, and `$84:C928`.
    // Keeping the instruction words in the sparse bus means the production interpreter—not
    // a test-only animation shortcut—still owns every duration, draw pointer, and deletion.
    bus.WriteBytes(0x84cadf, [
        0x79, 0x8c, 0x0a,       // Queue sound library 2, maximum 1: sound $0A.
        0x04, 0x00, 0x45, 0xa3, // Forward shot-break frames `$053,$054,$055`.
        0x04, 0x00, 0x4b, 0xa3,
        0x04, 0x00, 0x51, 0xa3,
        0x80, 0x01, 0x57, 0xa3, // Blank `$0FF` hold for exactly 384 frames.
        0x04, 0x00, 0x51, 0xa3, // Reverse frames before restoring PLM_Vars.
        0x04, 0x00, 0x4b, 0xa3,
        0x04, 0x00, 0x45, 0xa3,
        0x17, 0x8b,
        0xbc, 0x86,
    ]);

    // `$84:D084/$D08C` retain these complete power-bomb and Super-Missile respawning
    // programs after their weapon-family setup succeeds. They differ only in the sound
    // opcode's native entry point: `$8C7C` and `$8C10` both consume the odd byte `$0A`, but
    // queue at maximum one and maximum six respectively. The shared draw pointers below are
    // cartridge-authored shot-break art, and DrawPLMBlock restores synthesized `$x057/$x09F`.
    bus.WriteBytes(0x84cb71, [
        0x10, 0x8c, 0x0a,
        0x04, 0x00, 0x45, 0xa3,
        0x04, 0x00, 0x4b, 0xa3,
        0x04, 0x00, 0x51, 0xa3,
        0x80, 0x01, 0x57, 0xa3,
        0x04, 0x00, 0x51, 0xa3,
        0x04, 0x00, 0x4b, 0xa3,
        0x04, 0x00, 0x45, 0xa3,
        0x17, 0x8b,
        0xbc, 0x86,
    ]);
    bus.WriteBytes(0x84cb94, [
        0x7c, 0x8c, 0x0a,
        0x04, 0x00, 0x45, 0xa3,
        0x04, 0x00, 0x4b, 0xa3,
        0x04, 0x00, 0x51, 0xa3,
        0x80, 0x01, 0x57, 0xa3,
        0x04, 0x00, 0x51, 0xa3,
        0x04, 0x00, 0x4b, 0xa3,
        0x04, 0x00, 0x45, 0xa3,
        0x17, 0x8b,
        0xbc, 0x86,
    ]);

    // The adjacent `$D090/$D088` headers select the permanent variants. Their shortened
    // 4/4/4 or 3/2/1 forward animation ends on one blank frame and deletes without executing
    // DrawPLMBlock, so the cleared terrain never grows back.
    bus.WriteBytes(0x84cc0b, [
        0x10, 0x8c, 0x0a,
        0x04, 0x00, 0x45, 0xa3,
        0x04, 0x00, 0x4b, 0xa3,
        0x04, 0x00, 0x51, 0xa3,
        0x01, 0x00, 0x57, 0xa3,
        0xbc, 0x86,
    ]);
    bus.WriteBytes(0x84cc20, [
        0x7c, 0x8c, 0x0a,
        0x03, 0x00, 0x45, 0xa3,
        0x02, 0x00, 0x4b, 0xa3,
        0x01, 0x00, 0x51, 0xa3,
        0x01, 0x00, 0x57, 0xa3,
        0xbc, 0x86,
    ]);
    bus.WriteBytes(0x84c91c, [0x01, 0x00, 0xe7, 0xa4, 0xbc, 0x86]);
    bus.WriteBytes(0x84c922, [0x01, 0x00, 0xed, 0xa4, 0xbc, 0x86]);
    bus.WriteBytes(0x84c8fe, [0x01, 0x00, 0xb1, 0xa4, 0xbc, 0x86]);
    bus.WriteBytes(0x84c928, [0x01, 0x00, 0xf3, 0xa4, 0xbc, 0x86]);
    bus.WriteBytes(0x84aae3, [0xbc, 0x86]); // PLMEntries_nothing: delete immediately.

    // The reveal draw data is likewise retail-authored. `$A4B1` contains two horizontal
    // two-word records joined by signed relative offset `(0,+1)`; the other three are
    // ordinary single-record lists terminated by a zero offset pair.
    bus.WriteBytes(0x84a4b1, [
        0x02, 0x00, 0xbc, 0xb0, 0xbc, 0x50, 0x00, 0x01,
        0x02, 0x00, 0xbc, 0xd0, 0xbc, 0xd0, 0x00, 0x00,
    ]);
    bus.WriteBytes(0x84a4e7, [0x01, 0x00, 0x57, 0xc0, 0x00, 0x00]);
    bus.WriteBytes(0x84a4ed, [0x01, 0x00, 0x9f, 0xc0, 0x00, 0x00]);
    bus.WriteBytes(0x84a4f3, [0x01, 0x00, 0xb6, 0xb0, 0x00, 0x00]);

    var noBombItemSamus = new SamusState
    {
        Pose = SamusState.MorphBallGroundRightPose,
        EquippedItems = 0x0004,
        XPosition = 48,
        YPosition = 57,
    };
    noBombItemSamus.RefreshCollisionRadii(bus);
    var noBombItemSystem = new SamusBombProjectileSystem();
    BombProjectileFrameResult rejectedBomb = noBombItemSystem.StepFrame(
        bus,
        floor,
        noBombItemSamus,
        (ushort)SnesButton.X,
        (ushort)SnesButton.X);
    AssertEqual<int?>(null, rejectedBomb.PlacedSlot, "bomb item bit gates placement");

    var bombProjectileSamus = new SamusState
    {
        Pose = SamusState.MorphBallGroundRightPose,
        EquippedItems = 0x1004,
        XPosition = 48,
        YPosition = 57,
    };
    bombProjectileSamus.RefreshCollisionRadii(bus);
    var bombs = new SamusBombProjectileSystem();
    BombProjectileFrameResult placement = bombs.StepFrame(
        bus,
        floor,
        bombProjectileSamus,
        (ushort)SnesButton.X,
        (ushort)SnesButton.X);
    AssertEqual<int?>(0, placement.PlacedSlot, "first normal bomb uses physical slot zero");
    AssertEqual((ushort)1, bombs.BombCounter, "placement increments native bomb counter");
    AssertEqual((ushort)0x0010, bombs.CooldownTimer, "normal bomb loads cooldown table entry five");
    AssertEqual((ushort)59, bombs.Slots[0].BombTimer, "placement frame immediately decrements timer 60 to 59");
    AssertEqual((ushort)0x001e, bombs.Slots[0].Damage, "bomb damage follows bank-$93 data pointer");
    AssertEqual((ushort)0xad45, bombs.Slots[0].SpritemapPointer, "first instruction selects ROM bomb spritemap");
    AssertEqual((ushort)4, bombs.Slots[0].XRadius, "first instruction publishes ROM X radius");
    AssertEqual((ushort)4, bombs.Slots[0].YRadius, "first instruction publishes ROM Y radius");

    // A new edge during the active low-byte cooldown is rejected without incrementing the
    // aggregate. Release is a separate frame so ControllerInputState-like edge semantics
    // are represented explicitly in this direct subsystem test.
    bombs.StepFrame(bus, floor, bombProjectileSamus, 0, 0);
    BombProjectileFrameResult cooldownRejected = bombs.StepFrame(
        bus,
        floor,
        bombProjectileSamus,
        (ushort)SnesButton.X,
        (ushort)SnesButton.X);
    AssertEqual<int?>(null, cooldownRejected.PlacedSlot, "active bomb cooldown rejects another edge");
    AssertEqual((ushort)1, bombs.BombCounter, "rejected edge does not alter bomb counter");

    // Run to timer nine, then prove bank-$A0's three X comparisons at timer eight using
    // three independent slots/lifecycles. Distances remain inside the strict radius sum.
    static (SamusBombProjectileSystem System, SamusState Samus) MakeDirectionFixture(
        TestAddressSpace fixtureBus,
        RoomLevelData fixtureFloor)
    {
        var fixtureSamus = new SamusState
        {
            Pose = SamusState.MorphBallGroundRightPose,
            EquippedItems = 0x1004,
            XPosition = 48,
            YPosition = 57,
        };
        fixtureSamus.RefreshCollisionRadii(fixtureBus);
        var fixtureSystem = new SamusBombProjectileSystem();
        fixtureSystem.StepFrame(
            fixtureBus,
            fixtureFloor,
            fixtureSamus,
            (ushort)SnesButton.X,
            (ushort)SnesButton.X);
        while (fixtureSystem.Slots[0].BombTimer > 9)
            fixtureSystem.StepFrame(fixtureBus, fixtureFloor, fixtureSamus, 0, 0);
        return (fixtureSystem, fixtureSamus);
    }

    foreach ((ushort samusX, byte expectedDirection) in new (ushort, byte)[]
    {
        (47, 1),
        (48, 2),
        (49, 3),
    })
    {
        (SamusBombProjectileSystem directionSystem, SamusState directionSamus) =
            MakeDirectionFixture(bus, floor);
        directionSamus.XPosition = samusX;
        BombProjectileFrameResult timerEight = directionSystem.StepFrame(
            bus,
            floor,
            directionSamus,
            0,
            0);
        AssertEqual(expectedDirection, timerEight.PublishedBombJumpDirection,
            $"timer-eight bomb direction at Samus X {samusX}");
        AssertEqual((ushort)expectedDirection, directionSamus.BombJumpDirection,
            "bank-$A0 publishes low byte without command bit");
        AssertTrue(!directionSamus.BombJumpStarting,
            "timer-eight overlap does not start movement in same frame");
    }

    // Continue the original straight fixture through timer zero. It must enter fast art at
    // fifteen, publish direction at eight, emit a five-block no-op reaction cross at zero,
    // animate all five explosion records, execute delete, and decrement BombCounter.
    while (bombs.Slots[0].BombTimer > 15)
        bombs.StepFrame(bus, floor, bombProjectileSamus, 0, 0);
    AssertTrue(bombs.Slots[0].InstructionPointer >= 0x9fe3,
        "timer fifteen advances the live instruction pointer into fast animation");
    while (bombs.Slots[0].BombTimer > 8)
        bombs.StepFrame(bus, floor, bombProjectileSamus, 0, 0);
    AssertEqual((ushort)2, bombProjectileSamus.BombJumpDirection,
        "same-X timer-eight overlap publishes straight direction");
    AssertTrue(bombProjectileSamus.TrySetupPublishedMorphedBombJump(),
        "following alpha consumes published morphed bomb jump");
    AssertEqual((ushort)0x0802, bombProjectileSamus.BombJumpDirection,
        "morphed setup adds command-three bit on following frame");

    BombProjectileFrameResult explosion = default;
    while (!explosion.ExplosionStarted)
        explosion = bombs.StepFrame(bus, floor, bombProjectileSamus, 0, 0);
    AssertTrue(bombs.Slots[0].IsExploding, "timer zero selects bomb explosion list");
    AssertEqual((ushort)0x0501, bombs.Slots[0].Type, "first explosion pass marks block cross handled");
    AssertEqual(5, explosion.BlockReactions!.Count, "bomb explosion visits center/up/right/left/down");
    AssertEqual((byte)8, explosion.BlockReactions[4].CollisionType,
        "bottom reaction reaches fixture solid floor without inventing a PLM");

    for (int tick = 0; tick < 20 && bombs.BombCounter != 0; tick++)
        bombs.StepFrame(bus, floor, bombProjectileSamus, 0, 0);
    AssertEqual((ushort)0, bombs.BombCounter, "explosion delete decrements bomb counter");
    AssertTrue(!bombs.Slots[0].IsActive, "delete opcode clears complete bomb slot");

    // Selected HUD item three takes the power-bomb branch even without the normal Bomb
    // item bit. Placement consumes one round, locks `$0CEA`, initializes type `$0300` from
    // the literal bank-$93 record, and installs cooldown table entry three (`$28`).
    var powerBombSamus = new SamusState
    {
        Pose = SamusState.MorphBallGroundRightPose,
        EquippedItems = 0x0004,
        SelectedHudItem = 3,
        PowerBombs = 2,
        XPosition = 48,
        YPosition = 48,
    };
    powerBombSamus.RefreshCollisionRadii(bus);
    var powerBombs = new SamusBombProjectileSystem();
    BombProjectileFrameResult powerBombPlacement = powerBombs.StepFrame(
        bus,
        floor,
        powerBombSamus,
        (ushort)SnesButton.X,
        (ushort)SnesButton.X);
    AssertEqual<int?>(0, powerBombPlacement.PlacedSlot,
        "selected power bomb uses first physical bomb slot");
    AssertEqual((ushort)1, powerBombSamus.PowerBombs,
        "power-bomb placement decrements ammo exactly once");
    AssertEqual((ushort)3, powerBombSamus.SelectedHudItem,
        "remaining power-bomb ammo retains HUD selection");
    AssertEqual((ushort)0x0028, powerBombs.CooldownTimer,
        "power bomb loads non-beam cooldown table entry three");
    AssertEqual((ushort)0x0300, powerBombs.Slots[0].Type,
        "power-bomb projectile family is HUD index in high byte");
    AssertEqual((ushort)0x00c8, powerBombs.Slots[0].Damage,
        "power-bomb damage follows bank-$93 type-three data");
    AssertEqual((ushort)0xab97, powerBombs.Slots[0].SpritemapPointer,
        "power-bomb placement selects first retail slow-list spritemap");
    AssertTrue(powerBombs.PowerBombExplosion.IsArmed,
        "placement sets negative native power-bomb flag");
    AssertTrue(!powerBombs.PowerBombExplosion.IsActive,
        "bank-$88 explosion waits for projectile fuse");

    // `$90:C157` shares the normal bomb's 60->15 timing and adds `$1C` to the live
    // instruction pointer. A second selected-item edge is rejected by the armed flag
    // before helper two can perturb either aggregate counter.
    powerBombs.StepFrame(bus, floor, powerBombSamus, 0, 0);
    BombProjectileFrameResult armedRejected = powerBombs.StepFrame(
        bus,
        floor,
        powerBombSamus,
        (ushort)SnesButton.X,
        (ushort)SnesButton.X);
    AssertEqual<int?>(null, armedRejected.PlacedSlot,
        "negative power-bomb flag rejects a second placement");
    AssertEqual((ushort)1, powerBombs.BombCounter,
        "armed rejection preserves bomb aggregate");
    while (powerBombs.Slots[0].BombTimer > 15)
        powerBombs.StepFrame(bus, floor, powerBombSamus, 0, 0);
    AssertTrue(powerBombs.Slots[0].InstructionPointer >= 0x9fa3,
        "power-bomb timer fifteen enters retail fast list");

    BombProjectileFrameResult powerBombFuse = default;
    while (!powerBombFuse.ExplosionStarted)
        powerBombFuse = powerBombs.StepFrame(bus, floor, powerBombSamus, 0, 0);
    AssertEqual((ushort)0, powerBombs.Slots[0].BombTimer,
        "$FFFF fuse sentinel is consumed by first mode-three collision call");
    AssertEqual(PowerBombExplosionPhase.PreExplosionWhite,
        powerBombs.PowerBombExplosion.Phase,
        "fuse expiry executes $88:8B14 setup");
    AssertEqual((ushort)0x0400, powerBombs.PowerBombExplosion.PreExplosionRadius,
        "pre-explosion begins at retail 4.00-pixel radius");
    AssertEqual((ushort)0x8000, powerBombs.PowerBombExplosion.Status,
        "normal explosion publishes active status $8000");
    AssertEqual(0, powerBombFuse.BlockReactions!.Count,
        "fuse-expiration sentinel frame does not scan terrain");

    // HDMA executes before the next projectile pass. White pre-flash grows 4.00 by 48.00,
    // then the still-zero damaging radius scans the one-block rectangle's four duplicated
    // corners in bank-$94 top/left/bottom/right order.
    BombProjectileFrameResult firstPowerBombRadius = powerBombs.StepFrame(
        bus, floor, powerBombSamus, 0, 0);
    AssertEqual((ushort)0x3400, powerBombs.PowerBombExplosion.PreExplosionRadius,
        "first white pre-explosion frame applies $3000 speed");
    AssertEqual((ushort)0x2f80, powerBombs.PowerBombExplosion.RadiusSpeed,
        "white pre-explosion subtracts $0080 acceleration");
    AssertEqual(4, firstPowerBombRadius.BlockReactions!.Count,
        "zero damaging radius scans four inclusive one-block edges");
    AssertTrue(firstPowerBombRadius.BlockReactions.All(reaction =>
            reaction.BlockX == 3 && reaction.BlockY == 3),
        "zero-radius border duplicates the center exactly four times");

    int whiteFlashFrames = 1;
    while (powerBombs.PowerBombExplosion.Phase == PowerBombExplosionPhase.PreExplosionWhite)
    {
        powerBombs.StepFrame(bus, floor, powerBombSamus, 0, 0);
        whiteFlashFrames++;
        AssertTrue(whiteFlashFrames < 32, "white pre-explosion phase terminates");
    }
    AssertEqual(PowerBombExplosionPhase.PreExplosionYellow,
        powerBombs.PowerBombExplosion.Phase,
        "white threshold advances to yellow shape phase");
    AssertTrue(powerBombs.PowerBombExplosion.PreExplosionRadius >= 0x9200,
        "white phase crosses literal $9200 radius threshold");
    AssertEqual((ushort)0x9f06, powerBombs.PowerBombExplosion.ShapeDefinitionPointer,
        "yellow pre-explosion starts at shape table $9F06");

    for (int frame = 0; frame < 4; frame++)
        powerBombs.StepFrame(bus, floor, powerBombSamus, 0, 0);
    AssertEqual(PowerBombExplosionPhase.ExplosionYellow,
        powerBombs.PowerBombExplosion.Phase,
        "four 192-byte yellow shapes advance to damaging explosion");
    AssertEqual((ushort)0x0400, powerBombs.PowerBombExplosion.ExplosionRadius,
        "damaging yellow explosion restarts at 4.00 pixels");
    AssertEqual((ushort)0, powerBombs.PowerBombExplosion.RadiusSpeed,
        "damaging yellow explosion restarts with zero speed");

    int yellowExplosionFrames = 0;
    while (powerBombs.PowerBombExplosion.Phase == PowerBombExplosionPhase.ExplosionYellow)
    {
        powerBombs.StepFrame(bus, floor, powerBombSamus, 0, 0);
        yellowExplosionFrames++;
        AssertTrue(yellowExplosionFrames < 128, "yellow explosion reaches $8600 threshold");
    }
    AssertEqual(PowerBombExplosionPhase.ExplosionWhite,
        powerBombs.PowerBombExplosion.Phase,
        "yellow radius threshold advances to white shape phase");
    AssertTrue(powerBombs.PowerBombExplosion.ExplosionRadius >= 0x8600,
        "yellow explosion crosses literal $8600 threshold");
    AssertEqual((ushort)0x9246, powerBombs.PowerBombExplosion.ShapeDefinitionPointer,
        "white explosion starts at first retail ellipse table");

    int whiteExplosionFrames = 0;
    while (powerBombs.PowerBombExplosion.Phase == PowerBombExplosionPhase.ExplosionWhite)
    {
        powerBombs.StepFrame(bus, floor, powerBombSamus, 0, 0);
        whiteExplosionFrames++;
        AssertTrue(whiteExplosionFrames < 32, "white explosion shape sequence terminates");
    }
    AssertEqual(17, whiteExplosionFrames,
        "$9246-$9F06 white phase contains seventeen 192-byte shapes");
    AssertEqual(PowerBombExplosionPhase.Afterglow,
        powerBombs.PowerBombExplosion.Phase,
        "white shapes advance to stage-five afterglow");

    // Moving one pixel guarantees cleanup cannot begin Crystal Flash. The 32-step byte
    // counter performs 31 fades four frames apart, then `$88:8B4E` clears status/radii and
    // releases the flag; the same projectile pass sees flag zero and deletes the slot.
    powerBombSamus.XPosition++;
    int afterglowFrames = 0;
    BombProjectileFrameResult cleanupFrame = default;
    while (powerBombs.PowerBombExplosion.IsActive)
    {
        cleanupFrame = powerBombs.StepFrame(bus, floor, powerBombSamus, 0, 0);
        afterglowFrames++;
        AssertTrue(afterglowFrames < 160, "power-bomb afterglow reaches cleanup");
    }
    AssertEqual(125, afterglowFrames,
        "afterglow uses wrapping timer zero then 31 four-frame waits");
    AssertTrue(cleanupFrame.ProjectileDeleted,
        "cleanup frame deletes released power-bomb projectile");
    AssertEqual((ushort)0, powerBombs.BombCounter,
        "power-bomb cleanup decrements shared bomb counter");
    AssertTrue(!powerBombs.PowerBombExplosion.IsArmed,
        "failed Crystal Flash cleanup releases power-bomb flag");
    AssertEqual((ushort)0, powerBombs.PowerBombExplosion.ExplosionRadius,
        "cleanup clears damaging radius");

    // Put the explosion center on a type-$5 horizontal extension whose signed BTS $FF
    // redirects one column left to a BTS-zero type-$F parent. `$94:9CF4` visits center
    // before left, so the extension must spawn exactly one PLM and synchronously turn the
    // parent into temporary type-$8 terrain; the later left-arm visit sees that mutation.
    var reactionWords = new ushort[width * height];
    var reactionBts = new byte[reactionWords.Length];
    const int reactionParentIndex = 3 * width + 2;
    const int reactionExtensionIndex = 3 * width + 3;
    reactionWords[reactionParentIndex] = 0xf321;
    reactionBts[reactionParentIndex] = 0;
    reactionWords[reactionExtensionIndex] = 0x5058;
    reactionBts[reactionExtensionIndex] = 0xff;
    var reactionDefinitions = new byte[0x400 * 8];
    RoomLevelData reactionLevel = new(
        width,
        height,
        reactionWords,
        reactionBts,
        new ushort[reactionWords.Length],
        reactionDefinitions);
    BackgroundTilemapStreamer reactionStreamer = reactionLevel.CreateBackgroundStreamer();
    var reactionPlms = new RoomPlmSystem();
    var reactionBombs = new SamusBombProjectileSystem();
    var reactionSamus = new SamusState
    {
        Pose = SamusState.MorphBallGroundRightPose,
        EquippedItems = 0x1004,
        XPosition = 48,
        YPosition = 48,
    };
    reactionSamus.RefreshCollisionRadii(bus);
    reactionBombs.StepFrame(
        bus,
        reactionLevel,
        reactionSamus,
        (ushort)SnesButton.X,
        (ushort)SnesButton.X,
        reactionPlms);

    BombProjectileFrameResult reactionExplosion = default;
    while (!reactionExplosion.ExplosionStarted)
    {
        reactionExplosion = reactionBombs.StepFrame(
            bus,
            reactionLevel,
            reactionSamus,
            0,
            0,
            reactionPlms);
    }

    AssertEqual((byte)5, reactionExplosion.BlockReactions![0].CollisionType,
        "bomb cross records the visited horizontal extension");
    AssertEqual((byte)0xff, reactionExplosion.BlockReactions[0].Behavior,
        "bomb cross preserves the extension's signed redirect BTS");
    AssertEqual((byte)8, reactionExplosion.BlockReactions[3].CollisionType,
        "later left-arm reaction observes the synchronously mutated parent");
    AssertEqual(1, reactionPlms.ActiveCount,
        "extension and later parent visit produce one native reaction PLM");
    AssertEqual((ushort)0x8058,
        reactionLevel.GetCollisionBlockByIndex(reactionParentIndex).LevelWord,
        "CEDA keeps a type-F bomb block temporarily solid through movement beta");

    // PLM_Handler runs after movement beta in the same gameplay frame. Normal-bomb setup
    // began at `$CC3F`, so this first handler pass draws air `$0053` without queueing the
    // reaction head's sound $0A. The bomb explosion itself remains the sound owner.
    reactionPlms.Step(bus, reactionLevel, reactionStreamer, 0, 0, 0);
    AssertEqual((ushort)0x0053,
        reactionLevel.GetCollisionBlockByIndex(reactionParentIndex).LevelWord,
        "same-frame PLM pass draws the first bomb-block air frame");
    AssertEqual(0, reactionPlms.SoundRequests.Count,
        "normal bomb setup skips reaction PLM sound $0A");

    // Walk the exact forward, 384-frame blank hold, reverse, restore, and delayed-delete
    // timeline. The restored word is deliberately `$F058`, not the original `$F321`:
    // setup CEDA synthesized PLM_Vars by replacing all twelve low bits with `$058`.
    for (int frame = 0; frame < 12; frame++)
        reactionPlms.Step(bus, reactionLevel, reactionStreamer, 0, 0, 0);
    AssertEqual((ushort)0x00ff,
        reactionLevel.GetCollisionBlockByIndex(reactionParentIndex).LevelWord,
        "bomb reaction reaches blank air after three four-frame transitions");
    for (int frame = 0; frame < 384 + 12; frame++)
        reactionPlms.Step(bus, reactionLevel, reactionStreamer, 0, 0, 0);
    AssertEqual((ushort)0xf058,
        reactionLevel.GetCollisionBlockByIndex(reactionParentIndex).LevelWord,
        "bomb reaction restores synthesized type-F parent after native hold");
    AssertEqual(1, reactionPlms.ActiveCount,
        "DrawPLMBlock retains reaction PLM through its timer-one restore pass");
    reactionPlms.Step(bus, reactionLevel, reactionStreamer, 0, 0, 0);
    AssertEqual(0, reactionPlms.ActiveCount,
        "bomb reaction PLM deletes on the handler pass after restoration");

    // Exercise the public bomb pipeline as well as the setup methods below. A BTS-eight
    // type-$C parent remains unchanged during CF2E setup, then the same gameplay frame's
    // PLM pass reveals `$C057`. This catches an omitted/wrong collision-type dispatcher
    // branch even if the isolated bank-$84 setup tests continue to pass.
    var integratedRevealWords = new ushort[width * height];
    var integratedRevealBts = new byte[integratedRevealWords.Length];
    const int integratedRevealIndex = 3 * width + 3;
    integratedRevealWords[integratedRevealIndex] = 0xc000;
    integratedRevealBts[integratedRevealIndex] = 8;
    RoomLevelData integratedRevealLevel = new(
        width,
        height,
        integratedRevealWords,
        integratedRevealBts,
        new ushort[integratedRevealWords.Length],
        reactionDefinitions);
    BackgroundTilemapStreamer integratedRevealStreamer =
        integratedRevealLevel.CreateBackgroundStreamer();
    var integratedRevealPlms = new RoomPlmSystem();
    var integratedRevealBombs = new SamusBombProjectileSystem();
    var integratedRevealSamus = new SamusState
    {
        Pose = SamusState.MorphBallGroundRightPose,
        EquippedItems = 0x1004,
        XPosition = 48,
        YPosition = 48,
    };
    integratedRevealSamus.RefreshCollisionRadii(bus);
    integratedRevealBombs.StepFrame(
        bus,
        integratedRevealLevel,
        integratedRevealSamus,
        (ushort)SnesButton.X,
        (ushort)SnesButton.X,
        integratedRevealPlms);
    BombProjectileFrameResult integratedRevealExplosion = default;
    while (!integratedRevealExplosion.ExplosionStarted)
    {
        integratedRevealExplosion = integratedRevealBombs.StepFrame(
            bus,
            integratedRevealLevel,
            integratedRevealSamus,
            0,
            0,
            integratedRevealPlms);
    }
    AssertEqual((byte)12, integratedRevealExplosion.BlockReactions![0].CollisionType,
        "bomb dispatcher records center shootable-solid parent");
    AssertEqual(1, integratedRevealPlms.ActiveCount,
        "bomb dispatcher installs shootable reveal PLM");
    AssertEqual((ushort)0xc000,
        integratedRevealLevel.GetCollisionBlockByIndex(integratedRevealIndex).LevelWord,
        "CF2E leaves required-weapon parent unchanged through movement beta");
    integratedRevealPlms.Step(
        bus,
        integratedRevealLevel,
        integratedRevealStreamer,
        0,
        0,
        0);
    AssertEqual((ushort)0xc057,
        integratedRevealLevel.GetCollisionBlockByIndex(integratedRevealIndex).LevelWord,
        "same-frame PLM pass reveals required power-bomb block");

    // Shootable reaction setup `$84:CE6B` is easy to get subtly wrong because it does not
    // preserve the original low twelve bits. Prove that a type-C/BTS-zero parent becomes
    // temporary `$8052`, begins at the ROM's `$0053` air frame, queues sound `$0A` through
    // the max-one opcode, and ultimately restores synthesized `$C052` rather than `$C321`.
    var shotWords = new ushort[width * height];
    const int shotIndex = 3 * width + 3;
    shotWords[shotIndex] = 0xc321;
    RoomLevelData shotLevel = new(
        width,
        height,
        shotWords,
        new byte[shotWords.Length],
        new ushort[shotWords.Length],
        reactionDefinitions);
    BackgroundTilemapStreamer shotStreamer = shotLevel.CreateBackgroundStreamer();
    var shotPlms = new RoomPlmSystem();
    AssertTrue(
        shotPlms.TrySpawnBombedShootableBlock(shotLevel, shotIndex, 0, 0x0500),
        "normal bomb allocates respawning shot-block PLM");
    AssertEqual((ushort)0x8052,
        shotLevel.GetCollisionBlockByIndex(shotIndex).LevelWord,
        "CE6B installs synthesized temporary shot-block word");
    shotPlms.Step(bus, shotLevel, shotStreamer, 0, 0, 0);
    AssertEqual((ushort)0x0053,
        shotLevel.GetCollisionBlockByIndex(shotIndex).LevelWord,
        "respawning shot block begins with retail air frame");
    AssertEqual(1, shotPlms.SoundRequests.Count,
        "shot-block head queues one sound request");
    AssertEqual((byte)0x0a, shotPlms.SoundRequests[0].SoundId,
        "shot-block head queues crumble sound $0A");
    AssertEqual((byte)1, shotPlms.SoundRequests[0].MaximumQueued,
        "$84:8C79 uses sound-library-two maximum one");
    for (int frame = 0; frame < 12; frame++)
        shotPlms.Step(bus, shotLevel, shotStreamer, 0, 0, 0);
    AssertEqual((ushort)0x00ff,
        shotLevel.GetCollisionBlockByIndex(shotIndex).LevelWord,
        "respawning shot block reaches its blank hold word");
    for (int frame = 0; frame < 384 + 12; frame++)
        shotPlms.Step(bus, shotLevel, shotStreamer, 0, 0, 0);
    AssertEqual((ushort)0xc052,
        shotLevel.GetCollisionBlockByIndex(shotIndex).LevelWord,
        "respawning shot block restores CE6B's synthesized parent");
    shotPlms.Step(bus, shotLevel, shotStreamer, 0, 0, 0);
    AssertEqual(0, shotPlms.ActiveCount,
        "respawning shot block deletes one handler pass after restoration");

    // `$84:D084` is the respawning power-bomb block. Its CF2E setup accepts family `$0300`,
    // synthesizes `$C057`, and executes `$CB94`; the latter begins with the direct `$8C7C`
    // max-one sound opcode rather than ordinary shot block `$8C79`. Following the entire
    // reverse animation proves RestoreLevelWord is the setup-produced `$C057`, never the
    // fixture's original visual block `$C321`.
    RoomLevelData powerBombShotLevel = new(
        width,
        height,
        shotWords,
        new byte[shotWords.Length],
        new ushort[shotWords.Length],
        reactionDefinitions);
    BackgroundTilemapStreamer powerBombShotStreamer =
        powerBombShotLevel.CreateBackgroundStreamer();
    var powerBombShotPlms = new RoomPlmSystem();
    AssertTrue(
        powerBombShotPlms.TrySpawnProjectileShotBlock(
            powerBombShotLevel,
            shotIndex,
            behavior: 8,
            projectileType: 0x0300,
            solidBlock: true),
        "power bomb allocates BTS-eight respawning block PLM");
    AssertEqual((ushort)0x8057,
        powerBombShotLevel.GetCollisionBlockByIndex(shotIndex).LevelWord,
        "CF2E installs synthesized temporary power-bomb word");
    powerBombShotPlms.Step(
        bus, powerBombShotLevel, powerBombShotStreamer, 0, 0, 0);
    AssertEqual((ushort)0x0053,
        powerBombShotLevel.GetCollisionBlockByIndex(shotIndex).LevelWord,
        "power-bomb block begins with retail air frame");
    AssertEqual(new PlmSoundRequest(2, 0x0a, 1), powerBombShotPlms.SoundRequests[0],
        "$8C7C queues power-bomb breakup sound with maximum one");
    for (int frame = 0; frame < 12 + 384 + 12; frame++)
        powerBombShotPlms.Step(bus, powerBombShotLevel, powerBombShotStreamer, 0, 0, 0);
    AssertEqual((ushort)0xc057,
        powerBombShotLevel.GetCollisionBlockByIndex(shotIndex).LevelWord,
        "respawning power-bomb block restores synthesized $C057 parent");
    powerBombShotPlms.Step(
        bus, powerBombShotLevel, powerBombShotStreamer, 0, 0, 0);
    AssertEqual(0, powerBombShotPlms.ActiveCount,
        "respawning power-bomb block deletes after restoration");

    // `$84:D08C` is the corresponding Super Missile block. CF67 accepts family `$0200`,
    // synthesizes `$C09F`, and retains `$CB71`'s max-six sound plus the same 384-frame
    // blank hold. An ordinary missile must fail that setup without touching terrain or
    // leaving an apparently occupied host PLM slot.
    RoomLevelData superShotLevel = new(
        width,
        height,
        shotWords,
        new byte[shotWords.Length],
        new ushort[shotWords.Length],
        reactionDefinitions);
    BackgroundTilemapStreamer superShotStreamer = superShotLevel.CreateBackgroundStreamer();
    var superShotPlms = new RoomPlmSystem();
    AssertTrue(!superShotPlms.TrySpawnProjectileShotBlock(
            superShotLevel,
            shotIndex,
            behavior: 10,
            projectileType: 0x0100,
            solidBlock: true),
        "ordinary missile is rejected by Super Missile block setup");
    AssertEqual((ushort)0xc321,
        superShotLevel.GetCollisionBlockByIndex(shotIndex).LevelWord,
        "rejected missile leaves Super Missile block word untouched");
    AssertEqual(0, superShotPlms.ActiveCount,
        "rejected missile leaves no live PLM header");
    AssertTrue(superShotPlms.TrySpawnProjectileShotBlock(
            superShotLevel,
            shotIndex,
            behavior: 10,
            projectileType: 0x0200,
            solidBlock: true),
        "Super Missile allocates BTS-A respawning block PLM");
    AssertEqual((ushort)0x809f,
        superShotLevel.GetCollisionBlockByIndex(shotIndex).LevelWord,
        "CF67 installs synthesized temporary Super Missile word");
    superShotPlms.Step(bus, superShotLevel, superShotStreamer, 0, 0, 0);
    AssertEqual(new PlmSoundRequest(2, 0x0a, 6), superShotPlms.SoundRequests[0],
        "$CB71 queues Super Missile breakup sound with maximum six");
    for (int frame = 0; frame < 12 + 384 + 12; frame++)
        superShotPlms.Step(bus, superShotLevel, superShotStreamer, 0, 0, 0);
    AssertEqual((ushort)0xc09f,
        superShotLevel.GetCollisionBlockByIndex(shotIndex).LevelWord,
        "respawning Super Missile block restores synthesized $C09F parent");
    superShotPlms.Step(bus, superShotLevel, superShotStreamer, 0, 0, 0);
    AssertEqual(0, superShotPlms.ActiveCount,
        "respawning Super Missile block deletes after restoration");

    // BTS nine and B select the two permanent lists. Run both beyond their final timer so
    // `$CC20`'s asymmetric 3/2/1 timing and `$CC0B`'s ordinary 4/4/4 timing must each parse,
    // publish blank `$00FF`, and delete without executing DrawPLMBlock.
    foreach ((byte behavior, ushort projectileType, string familyName) in new[]
    {
        ((byte)9, (ushort)0x0300, "power-bomb"),
        ((byte)11, (ushort)0x0200, "Super-Missile"),
    })
    {
        RoomLevelData permanentWeaponLevel = new(
            width,
            height,
            shotWords,
            new byte[shotWords.Length],
            new ushort[shotWords.Length],
            reactionDefinitions);
        BackgroundTilemapStreamer permanentWeaponStreamer =
            permanentWeaponLevel.CreateBackgroundStreamer();
        var permanentWeaponPlms = new RoomPlmSystem();
        AssertTrue(permanentWeaponPlms.TrySpawnProjectileShotBlock(
                permanentWeaponLevel,
                shotIndex,
                behavior,
                projectileType,
                solidBlock: true),
            $"{familyName} allocates permanent weapon-gated block PLM");
        for (int frame = 0; frame < 20; frame++)
        {
            permanentWeaponPlms.Step(
                bus, permanentWeaponLevel, permanentWeaponStreamer, 0, 0, 0);
        }
        AssertEqual((ushort)0x00ff,
            permanentWeaponLevel.GetCollisionBlockByIndex(shotIndex).LevelWord,
            $"permanent {familyName} block ends on retail blank word");
        AssertEqual(0, permanentWeaponPlms.ActiveCount,
            $"permanent {familyName} block deletes without restoration");
    }

    // The final four nonnegative table entries all point to PLMEntries_nothing. Negative BTS
    // has a collision-nibble asymmetry instead: type C allocates an area-table no-op, while
    // type four returns before Spawn_PLM. These slots are visually invisible, but retaining
    // their one-handler lifetime prevents the finite 40-slot pool from behaving differently.
    RoomLevelData noOpShotLevel = new(
        width,
        height,
        shotWords,
        new byte[shotWords.Length],
        new ushort[shotWords.Length],
        reactionDefinitions);
    BackgroundTilemapStreamer noOpShotStreamer = noOpShotLevel.CreateBackgroundStreamer();
    var noOpShotPlms = new RoomPlmSystem();
    AssertTrue(noOpShotPlms.TrySpawnProjectileShotBlock(
            noOpShotLevel,
            shotIndex,
            behavior: 12,
            projectileType: 0,
            solidBlock: true),
        "BTS-C allocates retail no-op shot PLM");
    AssertTrue(noOpShotPlms.TrySpawnProjectileShotBlock(
            noOpShotLevel,
            shotIndex,
            behavior: 0x80,
            projectileType: 0,
            solidBlock: true),
        "negative type-C BTS allocates area-table no-op shot PLM");
    AssertTrue(!noOpShotPlms.TrySpawnProjectileShotBlock(
            noOpShotLevel,
            shotIndex,
            behavior: 0x80,
            projectileType: 0,
            solidBlock: false),
        "negative type-four BTS exits before shot PLM allocation");
    AssertEqual(2, noOpShotPlms.ActiveCount,
        "only the two solid/no-op reactions occupy native slots");
    AssertEqual((ushort)0xc321,
        noOpShotLevel.GetCollisionBlockByIndex(shotIndex).LevelWord,
        "all no-op shot reactions preserve terrain");
    noOpShotPlms.Step(bus, noOpShotLevel, noOpShotStreamer, 0, 0, 0);
    AssertEqual(0, noOpShotPlms.ActiveCount,
        "no-op shot PLMs delete on their first handler pass");

    // Normal bombs striking BTS 8 and A do not break their weapon-gated blocks. CF2E/CF67
    // redirect the PLM pointer to one-frame diagnostic reveals. The complete level word is
    // drawn by the normal bank-$84 draw parser and remains after the slot deletes.
    var revealWords = new ushort[width * height];
    const int powerRevealIndex = 2 * width + 2;
    const int superRevealIndex = 2 * width + 4;
    const int areaNoOpIndex = 4 * width + 2;
    revealWords[powerRevealIndex] = 0x4000;
    revealWords[superRevealIndex] = 0xc000;
    revealWords[areaNoOpIndex] = 0xc222;
    RoomLevelData revealLevel = new(
        width,
        height,
        revealWords,
        new byte[revealWords.Length],
        new ushort[revealWords.Length],
        reactionDefinitions);
    BackgroundTilemapStreamer revealStreamer = revealLevel.CreateBackgroundStreamer();
    var revealPlms = new RoomPlmSystem();
    AssertTrue(
        revealPlms.TrySpawnBombedShootableBlock(revealLevel, powerRevealIndex, 8, 0x0500),
        "normal bomb allocates power-bomb reveal PLM");
    AssertTrue(
        revealPlms.TrySpawnBombedShootableBlock(revealLevel, superRevealIndex, 10, 0x0500),
        "normal bomb allocates super-missile reveal PLM");
    AssertTrue(
        revealPlms.TrySpawnBombedShootableBlock(revealLevel, areaNoOpIndex, 0x80, 0x0500),
        "negative type-C BTS allocates area-table no-op PLM");
    revealPlms.Step(bus, revealLevel, revealStreamer, 0, 0, 0);
    AssertEqual((ushort)0xc057,
        revealLevel.GetCollisionBlockByIndex(powerRevealIndex).LevelWord,
        "normal bomb reveals visible power-bomb block word");
    AssertEqual((ushort)0xc09f,
        revealLevel.GetCollisionBlockByIndex(superRevealIndex).LevelWord,
        "normal bomb reveals visible super-missile block word");
    AssertEqual(0, revealPlms.SoundRequests.Count,
        "weapon-required reveal lists do not queue shot-break sound");
    AssertEqual((ushort)0xc222,
        revealLevel.GetCollisionBlockByIndex(areaNoOpIndex).LevelWord,
        "area-dependent shootable no-op leaves terrain unchanged");
    AssertEqual(2, revealPlms.ActiveCount,
        "area no-op deletes while both one-frame reveal PLMs remain timed");
    revealPlms.Step(bus, revealLevel, revealStreamer, 0, 0, 0);
    AssertEqual(0, revealPlms.ActiveCount,
        "weapon-required reveal PLMs delete after their one-frame draw");

    // `$94:9DA4` chooses a two-by-two crumble reveal for BTS three. The draw record is two
    // horizontal rows with a signed `(0,+1)` continuation; asserting all four words guards
    // both the special dispatch and the generic multi-record parser. Brinstar's negative
    // BTS `$82` instead selects the area-specific speed-block reveal at `$84:C928`.
    var specialWords = new ushort[width * height];
    const int crumbleIndex = 2 * width + 2;
    const int speedIndex = 5 * width + 5;
    specialWords[crumbleIndex] = 0xb000;
    specialWords[speedIndex] = 0xb000;
    RoomLevelData specialLevel = new(
        width,
        height,
        specialWords,
        new byte[specialWords.Length],
        new ushort[specialWords.Length],
        reactionDefinitions);
    BackgroundTilemapStreamer specialStreamer = specialLevel.CreateBackgroundStreamer();
    var specialPlms = new RoomPlmSystem();
    AssertTrue(
        specialPlms.TrySpawnBombedSpecialBlock(specialLevel, crumbleIndex, 3, 0, 0x0500),
        "normal bomb allocates two-by-two crumble reveal");
    AssertTrue(
        specialPlms.TrySpawnBombedSpecialBlock(specialLevel, speedIndex, 0x82, 1, 0x0500),
        "Brinstar negative BTS two allocates speed-block reveal");
    specialPlms.Step(bus, specialLevel, specialStreamer, 0, 0, 0);
    AssertEqual((ushort)0xb0bc,
        specialLevel.GetCollisionBlockByIndex(crumbleIndex).LevelWord,
        "crumble reveal writes parent word");
    AssertEqual((ushort)0x50bc,
        specialLevel.GetCollisionBlockByIndex(crumbleIndex + 1).LevelWord,
        "crumble reveal writes right extension");
    AssertEqual((ushort)0xd0bc,
        specialLevel.GetCollisionBlockByIndex(crumbleIndex + width).LevelWord,
        "crumble reveal writes lower vertical extension");
    AssertEqual((ushort)0xd0bc,
        specialLevel.GetCollisionBlockByIndex(crumbleIndex + width + 1).LevelWord,
        "crumble reveal writes lower-right vertical extension");
    AssertEqual((ushort)0xb0b6,
        specialLevel.GetCollisionBlockByIndex(speedIndex).LevelWord,
        "Brinstar area table reveals speed-booster block");
    specialPlms.Step(bus, specialLevel, specialStreamer, 0, 0, 0);
    AssertEqual(0, specialPlms.ActiveCount,
        "special reveal PLMs delete after their one-frame draw");

    // Bank `$A0:97E2-$A0:984E` decides direction from bomb-versus-Samus X. Its bank-$91
    // command-three handoff must retain the stable ball pose and arm `$0801-$0803`; it
    // must also reject non-ball callers instead of silently inventing a normal jump.
    var bombJump = new SamusState
    {
        Pose = SamusState.MorphBallGroundRightPose,
        EquippedItems = 0x0004,
        XPosition = 48,
        YPosition = 48,
    };
    bombJump.RefreshCollisionRadii(bus);
    bombJump.InitializeAnimation(bus);
    bombJump.Kinematics.YAcceleration = 0;
    bombJump.Kinematics.YSubacceleration = 0x4000;
    AssertThrows<ArgumentOutOfRangeException>(
        () => bombJump.RequestMorphedBombJump(0),
        "bomb-jump direction zero rejected");

    bombJump.RequestMorphedBombJump(3);
    AssertEqual((ushort)0x0803, bombJump.BombJumpDirection, "right bomb jump command word");
    ushort startX = bombJump.XPosition;
    ushort startY = bombJump.YPosition;
    BombJumpMovementResult start = SamusBombJumpMovement.Start(bus, bombJump);
    AssertTrue(start.Started && !start.Ended, "bomb-jump start handler reports initialization");
    AssertEqual(startX, bombJump.XPosition, "bomb-jump start frame has no horizontal displacement");
    AssertEqual(startY, bombJump.YPosition, "bomb-jump start frame has no vertical displacement");
    AssertEqual((ushort)2, bombJump.Kinematics.YSpeed, "bomb-jump whole speed comes from $90:9EF5");
    AssertEqual((ushort)0xc000, bombJump.Kinematics.YSubspeed, "bomb-jump subspeed comes from $90:9EFB");
    AssertEqual((ushort)1, bombJump.Kinematics.YDirection, "bomb jump starts upward");

    // The first diagonal handler frame accelerates by exactly the literal 1.0000 record,
    // moves right one pixel, moves upward by the pre-gravity 2.C000 magnitude, then stores
    // the reduced 2.8000 magnitude for the following frame.
    BombJumpMovementResult diagonal = SamusBombJumpMovement.Step(
        bus, empty, bombJump, nmiFrameCounter: 0);
    AssertTrue(!diagonal.Ended, "unobstructed diagonal bomb jump remains active");
    AssertEqual((ushort)(startX + 1), bombJump.XPosition, "right bomb jump uses $90:9F25 displacement");
    AssertEqual((ushort)2, bombJump.Kinematics.YSpeed, "bomb-jump gravity stores next whole speed");
    AssertEqual((ushort)0x8000, bombJump.Kinematics.YSubspeed, "bomb-jump gravity stores next subspeed");

    // `$90:8F1B` interprets a wrapped whole word as signed underflow. It switches to down,
    // installs diagonal deceleration mode two, and `$90:E032` relinquishes control before
    // making another vertical move. The normal type-four ball handler owns the descent.
    bombJump.Kinematics.YSpeed = 0xffff;
    bombJump.Kinematics.YSubspeed = 0;
    BombJumpMovementResult apex = SamusBombJumpMovement.Step(
        bus, empty, bombJump, nmiFrameCounter: 1);
    AssertTrue(apex.Ended, "signed bomb-jump apex ends special handler");
    AssertEqual((ushort)0, bombJump.BombJumpDirection, "bomb-jump apex clears command word");
    AssertEqual((ushort)2, bombJump.Kinematics.YDirection, "bomb-jump apex hands off downward direction");
    AssertEqual((ushort)2, bombJump.HorizontalSpeed.AccelerationMode, "diagonal apex selects mode two");

    // Direction two deliberately omits horizontal calculation. A ceiling collision ends
    // the handler on that same frame and zeros vertical magnitude before normal movement
    // resumes, matching `$90:E077-$90:E094`.
    var ceilingBlocks = new ushort[width * height];
    for (int x = 0; x < width; x++)
        ceilingBlocks[2 * width + x] = 0x8000;
    RoomLevelData ceiling = new(
        width,
        height,
        ceilingBlocks,
        new byte[ceilingBlocks.Length],
        new ushort[ceilingBlocks.Length],
        new byte[8]);
    var straightBombJump = new SamusState
    {
        Pose = SamusState.MorphBallGroundLeftPose,
        EquippedItems = 0x0004,
        XPosition = 48,
        YPosition = 56,
    };
    straightBombJump.RefreshCollisionRadii(bus);
    straightBombJump.InitializeAnimation(bus);
    straightBombJump.Kinematics.YAcceleration = 0;
    straightBombJump.Kinematics.YSubacceleration = 0x4000;
    straightBombJump.RequestMorphedBombJump(2);
    SamusBombJumpMovement.Start(bus, straightBombJump);
    BombJumpMovementResult ceilingHit = SamusBombJumpMovement.Step(
        bus, ceiling, straightBombJump, nmiFrameCounter: 0);
    AssertTrue(ceilingHit.Ended && ceilingHit.Vertical is { Collided: true },
        "straight bomb jump terminates on ceiling");
    AssertTrue(ceilingHit.Horizontal is null, "straight bomb jump performs no horizontal move");
    AssertEqual((ushort)48, straightBombJump.XPosition, "straight bomb jump preserves X");
    AssertEqual((ushort)0, straightBombJump.Kinematics.YSpeed, "ceiling hit clears vertical speed");

    // A grounded pose is intentional after the native special handler: morphed setup
    // preserved it. Prove that its normal falling collision can enter the ordinary bounce
    // state instead of throwing solely because the art still names a grounded ball.
    straightBombJump.YPosition = 55;
    straightBombJump.Kinematics.YDirection = 2;
    straightBombJump.Kinematics.YSpeed = 3;
    straightBombJump.Kinematics.YSubspeed = 0;
    straightBombJump.Kinematics.YAcceleration = 0;
    straightBombJump.Kinematics.YSubacceleration = 0;
    MorphBallMovementResult bombLanding = SamusMorphBallMovement.StepGrounded(
        bus, floor, straightBombJump, nmiFrameCounter: 0);
    AssertTrue(bombLanding.Landed, "post-bomb-jump grounded art collides with floor");
    AssertTrue(!straightBombJump.ApplyMorphBallLanding(bus), "post-bomb-jump landing launches bounce");
    AssertEqual((ushort)1, straightBombJump.MorphBallBounceState, "post-bomb-jump landing enters bounce one");

    Console.WriteLine("  Morph Ball: entry, bomb jump, bombable/shootable/special reaction PLMs, bounce, and tunnel collision agree.");
}

static ushort ReferenceNextRandom(ushort seed)
{
    int hardwareProductLow = (seed & 0xff) * 5;
    byte stackLow = (byte)hardwareProductLow;
    byte stackHigh = (byte)(hardwareProductLow >> 8);

    int hardwareProductHigh = ((seed >> 8) & 0xff) * 5;
    int eightBitAdc = stackHigh + (hardwareProductHigh & 0xff) + 1;
    stackHigh = (byte)eightBitAdc;
    int carry = eightBitAdc > 0xff ? 1 : 0;

    int restoredAccumulator = stackLow | (stackHigh << 8);
    return unchecked((ushort)(restoredAccumulator + 0x0011 + carry));
}

static void AssertTrue(bool condition, string context)
{
    if (!condition)
        throw new InvalidOperationException($"Verification failed: {context}.");
}

static void AssertEqual<T>(T expected, T actual, string context)
{
    // EqualityComparer<T>.Default handles primitives, records, nullable values, and enums.
    // Constraining T to IEquatable<T> looks attractive but incorrectly excludes C# enums.
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
        throw new InvalidOperationException($"Verification failed: {context}; expected {expected}, got {actual}.");
}

static void AssertThrows<TException>(Action action, string context)
    where TException : Exception
{
    try
    {
        action();
    }
    catch (TException)
    {
        return;
    }

    throw new InvalidOperationException($"Verification failed: {context}; expected {typeof(TException).Name}.");
}

/// <summary>
/// Sparse CPU-bus fixture. Unwritten addresses read as zero, mirroring cleared memory and
/// making every byte relevant to a transfer visible in the setup directly above it.
/// </summary>
sealed class TestAddressSpace : ISnesAddressSpace
{
    private readonly Dictionary<int, byte> _bytes = [];

    public byte ReadByte(int address)
    {
        if ((uint)address > 0x00ff_ffff)
            throw new ArgumentOutOfRangeException(nameof(address));

        return _bytes.GetValueOrDefault(address);
    }

    public void WriteByte(int address, byte value)
    {
        if ((uint)address > 0x00ff_ffff)
            throw new ArgumentOutOfRangeException(nameof(address));

        _bytes[address] = value;
    }

    public void WriteBytes(int startAddress, ReadOnlySpan<byte> values)
    {
        for (int index = 0; index < values.Length; index++)
            WriteByte(startAddress + index, values[index]);
    }
}

/// <summary>
/// Windows process policy used only by this console host. `SEM_FAILCRITICALERRORS`,
/// `SEM_NOGPFAULTERRORBOX`, and `SEM_NOOPENFILEERRORBOX` keep failures non-interactive;
/// .NET still writes the exception and stack trace to stderr and returns a failing code.
/// </summary>
static class NativeConsoleProcess
{
    [DllImport("kernel32.dll")]
    internal static extern uint SetErrorMode(uint errorMode);
}
