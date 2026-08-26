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
VerifySamusAerialMovement();
VerifySamusPostureMovement();
VerifySamusStandingAimMovement();
VerifySamusAimedAerialMovement();
VerifySamusSlopePhysics();
VerifySamusBlockCollision();
VerifySamusGroundedMovement();
VerifySamusGroundedReversal();
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

    Console.WriteLine("  Samus speed: normal-air pointer, 16.16 acceleration, deceleration, divisor, and clamp agree.");
}

/// <summary>
/// Exercises the complete no-equipment neutral-jump route: ROM pose transition bytecode,
/// jump initialization constants, old-speed displacement ordering, variable-height cut,
/// falling acceleration, solid-floor landing, radius alignment, and landing animation.
/// </summary>
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
/// Verifies command-seven bottom alignment, movement types five/$0F, $FD completion, and
/// the rejected stand-up case where ceiling and floor leave room for crouch but not stand.
/// </summary>
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

    // Literal pose records used by this route. All admitted airborne bodies have radius
    // 19; aimed landing expands to 21 and shifts the center up two pixels.
    bus.WriteBytes(0x91b631, [0x08, 0x00, 0xff, 0x02, 0x06, 0x00, 0x15, 0x00]); // $01
    bus.WriteBytes(0x91b641, [0x08, 0x00, 0x01, 0x00, 0x06, 0x00, 0x15, 0x00]); // $03
    bus.WriteBytes(0x91b651, [0x08, 0x00, 0x01, 0x01, 0x06, 0x00, 0x15, 0x00]); // $05
    bus.WriteBytes(0x91b661, [0x08, 0x00, 0x01, 0x03, 0x06, 0x00, 0x15, 0x00]); // $07
    bus.WriteBytes(0x91b6d1, [0x08, 0x02, 0x51, 0x00, 0x08, 0x00, 0x13, 0x00]); // $15
    bus.WriteBytes(0x91b771, [0x08, 0x06, 0xff, 0x02, 0x08, 0x00, 0x13, 0x00]); // $29
    bus.WriteBytes(0x91b781, [0x08, 0x06, 0x29, 0x00, 0x08, 0x00, 0x13, 0x00]); // $2B
    bus.WriteBytes(0x91b8b1, [0x08, 0x02, 0xff, 0x02, 0x08, 0x00, 0x13, 0x00]); // $51
    bus.WriteBytes(0x91b8e1, [0x08, 0x02, 0xff, 0x01, 0x03, 0x00, 0x13, 0x00]); // $57
    bus.WriteBytes(0x91b971, [0x08, 0x02, 0x51, 0x01, 0x08, 0x00, 0x13, 0x00]); // $69
    bus.WriteBytes(0x91b981, [0x08, 0x02, 0x51, 0x03, 0x08, 0x00, 0x13, 0x00]); // $6B
    bus.WriteBytes(0x91b991, [0x08, 0x06, 0x29, 0x01, 0x08, 0x00, 0x13, 0x00]); // $6D
    bus.WriteBytes(0x91b9a1, [0x08, 0x06, 0x29, 0x03, 0x08, 0x00, 0x13, 0x00]); // $6F
    bus.WriteBytes(0x91bd39, [0x08, 0x00, 0xff, 0x01, 0x03, 0x00, 0x15, 0x00]); // $E2
    bus.WriteBytes(0x91bd49, [0x08, 0x00, 0xff, 0x03, 0x03, 0x00, 0x15, 0x00]); // $E4

    // Synthetic delay streams expose both command-three seams independently.
    (byte Pose, ushort Stream, byte[] Bytes)[] animations = [
        (0x01, 0xc100, [0x0a, 0xf6]),
        (0x03, 0xc110, [0x0a, 0xf6]),
        (0x05, 0xc120, [0x0a, 0xf6]),
        (0x07, 0xc130, [0x0a, 0xf6]),
        (0x15, 0xc140, [0x02, 0xff]),
        (0x29, 0xc150, [0x02, 0xff]),
        (0x2b, 0xc160, [0x02, 0xff]),
        (0x51, 0xc170, [0x02, 0xff]),
        (0x57, 0xc180, [0x01, 0xfd, 0x69]),
        (0x69, 0xc190, [0x02, 0xff]),
        (0x6b, 0xc1a0, [0x02, 0xff]),
        (0x6d, 0xc1b0, [0x02, 0xff]),
        (0x6f, 0xc1c0, [0x02, 0xff]),
        (0xe2, 0xc1d0, [0x01, 0xf8, 0x05]),
        (0xe4, 0xc1e0, [0x01, 0xf8, 0x07]),
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

    Console.WriteLine("  Samus aimed air: FD jump, live aim, landing table/F8, walk-off, and fall fallback agree.");
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
