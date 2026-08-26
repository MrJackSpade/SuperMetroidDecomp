using System.Buffers.Binary;
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
VerifySamusStoredShineAndShinespark();
VerifySamusCrystalFlash();
VerifySamusDrainedController();
VerifyMotherBrainRainbowBeamSamusMovement();
VerifySamusSolidEnemyCollision();
VerifySamusAerialMovement();
VerifySamusSpaceJumpAndScrewAttack();
VerifySamusLiquidPhysics();
VerifySamusAerialTurnsAndWallJump();
VerifySamusKnockbackAndDamageBoost();
VerifySamusGrappleSwingAndRelease();
VerifySamusPostureMovement();
VerifySamusMorphBallMovement();
VerifySamusStandingAimMovement();
VerifySamusAimedAerialMovement();
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
VerifyScrollingSkyState();

Console.WriteLine("All bank $80 verification checks passed.");

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

    Console.WriteLine("  OAM: spritemap packing, clipping, high bits, and finalization agree.");
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

    Console.WriteLine("  Samus: standing/running pose tables, palette, split tile DMA, position, and OAM agree.");
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

    ShinesparkMovementResult first = horizontal.Shinespark.Step(
        bus, empty, horizontal, nmiFrameCounter: 4);
    AssertTrue(first.Horizontal is { Collided: false } && first.Vertical is null,
        "horizontal spark uses only block X movement");
    AssertEqual(sparkBombBlockIndex, first.Horizontal!.Value.BrokenBombBlock!.Value.Index,
        "horizontal spark publishes the broken BTS-7 block");
    AssertEqual((ushort)0x0123, empty.ForegroundEntries.Span[sparkBombBlockIndex],
        "bomb-block setup clears only the collision nibble");
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

    Console.WriteLine("  Crystal Flash: prerequisites, handlers, resources, and ROM animation agree.");
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
        "controller three publishes palette-FX producer seam");

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
    unable.Drained.DisableRainbowAndStartStandingAnimation(unable);
    AssertEqual((ushort)13, unable.AnimationFrame, "command $17 resumes standing animation at frame thirteen");
    AssertEqual((ushort)1, unable.AnimationFrameTimer, "command $17 resume timer");

    Console.WriteLine("  Drained Samus: rainbow commands, timer handlers, controllers, fall, releases, and hyper beam agree.");
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
        "  Space Jump/Screw Attack: pose priority, repeat window, damage, wall frames, palette cycle, and landing agree.");
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
    eligible.ApplyWallJumpTrigger(bus);
    AssertEqual((byte)0x83, eligible.Pose, "right-facing spin selects right wall-jump pose");
    AssertEqual((ushort)4, eligible.Kinematics.YSpeed, "wall jump reads whole launch speed");
    AssertEqual((ushort)0xa000, eligible.Kinematics.YSubspeed, "wall jump reads fractional launch speed");
    AssertEqual((ushort)1, eligible.HorizontalSpeed.ExtraRunSpeed, "wall jump preserves Dash whole speed");
    AssertEqual((ushort)0x7000, eligible.HorizontalSpeed.ExtraRunSubspeed, "wall jump preserves Dash fraction");
    AssertTrue(eligible.HorizontalSpeed.HasRunningMomentum, "wall jump preserves Dash momentum flag");
    for (int tick = 0; tick < 8; tick++)
        eligible.AnimateNoFx(bus);
    AssertEqual((byte)0xfb, eligible.LastAnimationDelayCommand!.Value, "wall animation reaches FB");
    AssertEqual((ushort)3, eligible.AnimationFrame, "ordinary dry wall animation selects frame three");

    ushort wallStartY = eligible.YPosition;
    AerialMovementResult wallFrame = SamusAerialMovement.StepWallJump(
        bus,
        level,
        eligible,
        (ushort)(SnesButton.Right | SnesButton.A),
        1);
    AssertTrue(wallFrame.Vertical is { Collided: false }, "wall launch remains airborne");
    AssertEqual((ushort)(wallStartY - 5), eligible.YPosition, "wall launch moves by old 4.A000 speed");

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

    Console.WriteLine("  Samus aerial turns/wall jump: selectors, momentum, F8, wall gate, FB, and launch agree.");
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
    AssertEqual((ushort)4, samus.KnockbackTimer, "first hurt frame decrements timer");
    AssertEqual(0x00004000, hurtFrame.Horizontal!.Value.AcceptedDisplacement,
        "knockback moves in bank-$A0 X direction");
    AssertEqual(unchecked((int)0xfffb0000), hurtFrame.Vertical!.Value.AcceptedDisplacement,
        "knockback moves by old 5.0000 vertical speed");
    AssertEqual((ushort)91, samus.YPosition, "knockback upward whole position");
    AssertEqual((ushort)4, samus.Kinematics.YSpeed, "knockback gravity next whole speed");
    AssertEqual((ushort)0xd800, samus.Kinematics.YSubspeed, "knockback gravity next subspeed");

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
        AssertTrue(!SamusKnockbackMovement.Step(bus, empty, expires, (ushort)frame).Ended,
            $"hurt movement frame {frame + 1} remains active");
    KnockbackMovementResult expired = SamusKnockbackMovement.Step(bus, empty, expires, 5);
    AssertTrue(expired.Ended, "zero hurt timer ends special handler");
    AssertEqual(SamusState.FallingRightPose, expires.Pose, "expired right knockback selects falling right");
    AssertTrue(!expires.KnockbackActive, "expired knockback restores normal handler");
    AssertEqual((ushort)0, expires.KnockbackDirection, "expired knockback clears direction");

    Console.WriteLine("  Samus knockback: timer, 16.16 hurt arc, damage boost, and normal-jump handoff agree.");
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

    // Synthetic delay streams expose both command-three seams independently.
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
        (0x6d, 0xc1b0, [0x02, 0xff]),
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
    samus.ApplyAerialAimTransition(bus, SamusState.FallingAimDiagonalDownRightPose);
    AssertEqual((byte)0x29, samus.ReadNoInputFallbackPose(bus), "aimed fall fallback target");
    samus.ApplyAerialAimTransition(bus, SamusState.FallingRightPose);
    AssertEqual((byte)0x29, samus.Pose, "aimed fall applies unaimed fallback");

    Console.WriteLine("  Samus aimed air: FD jump, live aim, compact hitboxes/landing, walk-off, and fall fallback agree.");
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

    Console.WriteLine("  Samus movement: standing clear and running speed/X/slope/grounding order agree.");
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
/// A deliberately register-oriented RNG reference. Unlike the production method, this
/// mutates a two-byte emulated stack value and an explicit carry flag in assembly order.
/// </summary>
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

    Console.WriteLine("  Morph Ball: ordinary/Spring entry, bomb jump, bounce, and tunnel collision agree.");
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
