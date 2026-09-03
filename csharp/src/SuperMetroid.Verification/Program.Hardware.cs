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

/// <summary>Core hardware, timing, bus, OAM, and system-state verification.</summary>
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
    ushort[] actual = Enumerable.Range(0, expected.Length)
        .Select(_ => state.NextRandom())
        .ToArray();
    AssertSequenceEqual(expected, actual, "known RNG sequence");

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
    AssertEqual(0, state.TimedHeldInput, "new press is not held input");

    // Frame 1: held-only input changes from zero to the button and reloads the timer.
    state.UpdateHeldInput(3, button, 0);
    AssertEqual(3, state.TimedHeldInputTimer, "changed input reloads timer");
    AssertEqual(0, state.TimedHeldInput, "changed input stays suppressed");

    // Frames 2-4: DEC yields 2, 1, and 0. BPL keeps taking the suppression path.
    for (ushort expectedTimer = 2; ; expectedTimer--)
    {
        state.UpdateHeldInput(3, button, 0);
        AssertEqual(expectedTimer, state.TimedHeldInputTimer, "stable-input countdown");
        AssertEqual(0, state.TimedHeldInput, "countdown suppression");
        if (expectedTimer == 0)
            break;
    }

    // Frame 5: zero wraps to $FFFF, which is negative to BPL, and activates the button.
    state.UpdateHeldInput(3, button, 0);
    AssertEqual(0, state.TimedHeldInputTimer, "expired timer is pinned to zero");
    AssertEqual(button, state.TimedHeldInput, "held input activates after underflow");
    AssertEqual(button, state.NewlyTimedHeldInput, "activation creates a rising-edge pulse");

    // Frame 6: the active input remains present, while its rising-edge output clears.
    state.UpdateHeldInput(3, button, 0);
    AssertEqual(button, state.TimedHeldInput, "stable expired input remains active");
    AssertEqual(0, state.NewlyTimedHeldInput, "rising-edge pulse lasts one update");

    // Releasing changes the held sample, reloads the timer, and suppresses the output.
    state.UpdateHeldInput(3, 0, 0);
    AssertEqual(3, state.TimedHeldInputTimer, "release reloads timer");
    AssertEqual(0, state.TimedHeldInput, "release clears filtered input");
    AssertEqual(0, state.NewlyTimedHeldInput, "release is not a new held press");

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
        state.SetEventRaw(eventNumber);
        AssertTrue(state.HasEventRaw(eventNumber), $"event ${eventNumber:X2} set");

        int expectedByteIndex = eventNumber >> 3;
        byte expectedMask = (byte)(1 << (eventNumber & 7));
        AssertEqual(expectedMask, state.GetEventByteRaw(expectedByteIndex), $"event ${eventNumber:X2} byte/mask");

        state.ClearEventRaw(eventNumber);
        AssertTrue(!state.HasEventRaw(eventNumber), $"event ${eventNumber:X2} clear");
        AssertEqual(0, state.GetEventByteRaw(expectedByteIndex), $"event ${eventNumber:X2} cleared byte");
    }

    AssertThrows<ArgumentOutOfRangeException>(() => state.SetEventRaw(-1), "negative raw event rejected");
    AssertThrows<ArgumentOutOfRangeException>(() => state.SetEventRaw(64), "raw event past allocation rejected");
    AssertThrows<ArgumentOutOfRangeException>(
        () => state.SetEvent((EventNumber)0x3f),
        "unnamed event rejected by named API");
    state.SetEvent(EventNumber.TourianUnlocked);
    AssertTrue(state.HasEvent(EventNumber.TourianUnlocked), "named retail event set");
    state.ClearEvent(EventNumber.TourianUnlocked);
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
        AssertEqual(BossBits.AreaBoss | BossBits.AreaTorizo, state.GetBossBits(area),
            $"area {area} typed combined mask");
        AssertEqual(0x05, state.GetBossBitsRaw(area), $"area {area} combined raw mask");

        state.ClearBossBits(area, BossBits.AreaTorizo);
        AssertEqual(0x01, state.GetBossBitsRaw(area), $"area {area} selective clear");
    }

    AssertThrows<ArgumentOutOfRangeException>(() => state.SetBossBits(-1, BossBits.AreaBoss), "negative area rejected");
    AssertThrows<ArgumentOutOfRangeException>(() => state.SetBossBits(8, BossBits.AreaBoss), "area past allocation rejected");
    AssertThrows<ArgumentOutOfRangeException>(
        () => state.SetBossBits(0, (BossBits)0x80),
        "unnamed boss bit rejected by typed API");
    AssertThrows<ArgumentOutOfRangeException>(
        () => BossBitMasks.FromCartridge(0x80, "constructed selector"),
        "unnamed cartridge boss mask rejected");
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
    AssertEqual(0x20, vram.ReadByte(0x0001 * 2), "column transfer low byte at first word");
    AssertEqual(0x21, vram.ReadByte(0x0001 * 2 + 1), "column transfer high byte at first word");
    AssertEqual(0x12, vram.ReadByte(0x0002 * 2), "linear transfer increments one word");
    AssertEqual(0x13, vram.ReadByte(0x0002 * 2 + 1), "linear transfer high byte");
    AssertEqual(0x22, vram.ReadByte(0x0021 * 2), "column transfer increments 32 words");
    AssertEqual(0x23, vram.ReadByte(0x0021 * 2 + 1), "column transfer second high byte");
    AssertEqual(0xa0, vram.ReadByte(0x0100 * 2), "bank-wrap transfer first byte");
    AssertEqual(0xa1, vram.ReadByte(0x0100 * 2 + 1), "bank-wrap transfer wrapped byte");

    AssertEqual(0, queue.TailInBytes, "drain clears packed tail");
    AssertEqual(0, queue.Entries.Count, "drain clears typed records");
    AssertThrows<ArgumentOutOfRangeException>(() => queue.Enqueue(0, 0x808000, 0), "zero-size VRAM entry rejected");
    AssertThrows<ArgumentOutOfRangeException>(() => queue.Enqueue(1, 0x1000000, 0), "25-bit VRAM source rejected");

    // A direct channel DAS of zero is not the queue's zero-word terminator. Hardware
    // decrements the sixteen-bit counter through all $10000 values, while A1T and VMADD
    // wrap independently inside their fixed banks/address widths. Start at xx:FFFE so the
    // first four bytes prove the A-bus wrap, then mark the final source pair as well.
    var fullDmaVram = new SnesVram();
    var dirtyVram = new byte[SnesVram.ByteCount];
    Array.Fill(dirtyVram, (byte)0x7e);
    fullDmaVram.LoadBytes(0, dirtyVram);
    bus.WriteByte(0x80fffe, 0xa0);
    bus.WriteByte(0x80ffff, 0xa1);
    bus.WriteByte(0x800000, 0xa2);
    bus.WriteByte(0x800001, 0xa3);
    bus.WriteByte(0x80fffc, 0xfe);
    bus.WriteByte(0x80fffd, 0xff);
    fullDmaVram.ExecuteHardwareDmaWrite(
        bus,
        sourceAddress: 0x80fffe,
        dmaSize: 0,
        encodedDestination: 0);
    AssertEqual(0xa0, fullDmaVram.ReadByte(0), "DAS-zero first source byte");
    AssertEqual(0xa1, fullDmaVram.ReadByte(1), "DAS-zero second source byte");
    AssertEqual(0xa2, fullDmaVram.ReadByte(2), "DAS-zero A-bus offset wrap low byte");
    AssertEqual(0xa3, fullDmaVram.ReadByte(3), "DAS-zero A-bus offset wrap high byte");
    AssertEqual(0xfe, fullDmaVram.ReadByte(0xfffe), "DAS-zero penultimate source byte");
    AssertEqual(0xff, fullDmaVram.ReadByte(0xffff), "DAS-zero final source byte");

    // Fill a fresh queue to its real table boundary. The next seven-byte record would
    // leave insufficient space for the two-byte terminator and must be rejected.
    var fullQueue = new VramWriteQueue();
    while (fullQueue.TailInBytes + VramWriteQueue.EntryByteCount + 2 <= VramWriteQueue.StorageByteCount)
        fullQueue.Enqueue(1, 0x808000, 0);
    AssertThrows<InvalidOperationException>(() => fullQueue.Enqueue(1, 0x808000, 0), "VRAM queue overflow rejected");

    Console.WriteLine("  VRAM: write queue, VMAIN stepping, bank/VMADD wrap, DAS zero, and reset agree.");
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
    AssertEqual(0x01, timer.MinutesBcd, "Ceres starts at one minute");
    AssertEqual(0x8000, timer.XPositionFixed, "timer starts at X $80.00");
    AssertEqual(0x8000, timer.YPositionFixed, "timer starts at Y $80.00");

    // State 3 increments the low X byte from $00 through $10 without decrementing time.
    for (ushort frame = 1; frame <= 16; frame++)
        AssertTrue(!timer.Process(frame), $"initial-delay frame {frame}");
    AssertEqual(EscapeTimerState.RunningMovementDelayed, timer.State, "initial delay advances at counter $10");
    AssertEqual(0x8010, timer.XPositionFixed, "X subpixel byte contains delay counter $10");
    AssertEqual(0x00, timer.SecondsBcd, "initial delay did not decrement seconds");

    // State 4 continues the same aliased counter to $60, then zeroes only its low byte.
    for (ushort frame = 17; frame <= 96; frame++)
        timer.Process(frame);
    AssertEqual(EscapeTimerState.RunningMovingIntoPlace, timer.State, "movement delay advances at counter $60");
    AssertEqual(0x8000, timer.XPositionFixed, "movement transition clears X fraction only");

    // X reaches its clamp in 106 frames; Y requires 107, so the state advances on 107.
    for (ushort frame = 97; frame <= 203; frame++)
        timer.Process(frame);
    AssertEqual(EscapeTimerState.RunningInPlace, timer.State, "timer reaches stationary running state");
    AssertEqual(0xdc00, timer.XPositionFixed, "timer X clamps at pixel 220");
    AssertEqual(0x3000, timer.YPositionFixed, "timer Y clamps at pixel 48");

    var borrowSecond = CreateRunningTimer(0x00, 0x01, 0x00);
    AssertTrue(!borrowSecond.Process(1), "00:01.00 remains nonzero after decrement");
    AssertEqual(0x00, borrowSecond.SecondsBcd, "centisecond borrow decrements seconds");
    AssertEqual(0x98, borrowSecond.CentisecondsBcd, "table value two wraps centiseconds to 98");

    var borrowMinute = CreateRunningTimer(0x01, 0x00, 0x00);
    AssertTrue(!borrowMinute.Process(0), "01:00.00 remains nonzero after decrement");
    AssertEqual(0x00, borrowMinute.MinutesBcd, "second borrow decrements minutes");
    AssertEqual(0x59, borrowMinute.SecondsBcd, "minute borrow reloads seconds to 59");
    AssertEqual(0x99, borrowMinute.CentisecondsBcd, "table value one wraps centiseconds to 99");

    var expiration = CreateRunningTimer(0x00, 0x00, 0x01);
    AssertTrue(expiration.Process(0), "00:00.01 expires on a decrement of one");
    AssertEqual(0, expiration.CentisecondsBcd, "expiration saturates centiseconds");

    var underflow = CreateRunningTimer(0x00, 0x00, 0x00);
    AssertTrue(underflow.Process(1), "zero timer remains expired after attempted decrement");
    AssertEqual(0, underflow.MinutesBcd, "underflow saturates minutes");
    AssertEqual(0, underflow.SecondsBcd, "underflow saturates seconds");
    AssertEqual(0, underflow.CentisecondsBcd, "underflow saturates centiseconds");

    var motherBrain = new EscapeTimer();
    motherBrain.RequestMotherBrainStart();
    motherBrain.Process(0);
    AssertEqual(0x03, motherBrain.MinutesBcd, "Mother Brain starts at three minutes");
    AssertEqual(0x8003, motherBrain.RawStatus, "active timer retains status high flag");

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
    AssertEqual(2, input.RepeatTimer, "first press loads initial repeat delay");

    input.Latch(button);
    AssertEqual(0, input.NewlyPressed, "stable hold has no new edge");
    AssertEqual(0, input.NewlyPressedWithRepeat, "first held frame has no repeat");
    AssertEqual(1, input.RepeatTimer, "held input decrements repeat timer");

    input.Latch(button);
    AssertEqual(button, input.NewlyPressedWithRepeat, "repeat timer zero emits held input");
    AssertEqual(1, input.RepeatTimer, "repeat pulse loads subsequent delay");

    input.Latch(0);
    AssertEqual(0, input.NewlyPressed, "release is not a rising edge");
    AssertEqual(2, input.RepeatTimer, "release reloads initial repeat delay");

    Console.WriteLine("  Input: NMI latch, rising edges, and repeat countdown agree.");
}

/// <summary>
/// Confirms the integrated frame seam preserves NMI ordering, accepted/lagged counters,
/// controller latching, VRAM draining, and timer dispatch.
/// </summary>
static void VerifyFrameRuntime()
{
    var infiniteAmmoSamus = new SamusState
    {
        Missiles = 1,
        MaxMissiles = 5,
        SuperMissiles = 0,
        MaxSuperMissiles = 5,
        PowerBombs = 0,
        MaxPowerBombs = 0,
    };
    HostInfiniteAmmoFrameGuard ammoGuard =
        HostInfiniteAmmoFrameGuard.Begin(enabled: true, infiniteAmmoSamus);
    AssertEqual((ushort)2, infiniteAmmoSamus.Missiles,
        "infinite ammo lends a frame-local final missile");
    AssertEqual((ushort)1, infiniteAmmoSamus.SuperMissiles,
        "infinite ammo repairs an exhausted unlocked type before selection");
    AssertEqual((ushort)0, infiniteAmmoSamus.PowerBombs,
        "infinite ammo does not unlock a missing upgrade");

    // Consume the lent missile exactly as the cartridge firing routine does. Completion
    // must keep it at one instead of allowing the native zero-ammo auto-cancel branch.
    infiniteAmmoSamus.Missiles--;
    ammoGuard.Complete(infiniteAmmoSamus);
    AssertEqual((ushort)1, infiniteAmmoSamus.Missiles,
        "infinite ammo preserves selection after the final missile");
    AssertEqual((ushort)1, infiniteAmmoSamus.SuperMissiles,
        "infinite ammo retains the unlocked one-unit floor");
    AssertEqual((ushort)0, infiniteAmmoSamus.PowerBombs,
        "infinite ammo leaves locked ammunition at zero");

    var bus = new TestAddressSpace();
    bus.WriteBytes(0x7e1234, [0xca, 0xfe]);
    var runtime = new SuperMetroidRuntime(bus);
    runtime.VramWrites.Enqueue(2, 0x7e1234, 0x0020);
    runtime.EscapeTimer.RequestCeresStart();

    // `$89:ACC3` writes Mode 7 shadow registers during room main. They must become visible
    // only when an accepted NMI reaches `$80:95A7`, at the same boundary as displayed OAM.
    var firstMode7 = new SamusMode7Transform(0x00f0, 0x0010, 0xfff0, 0x0080, 0x03f0);
    runtime.ActiveSamusMode7Transform = firstMode7;

    RuntimeFrameResult first = runtime.StepFrame((ushort)SnesButton.Start);
    AssertEqual(1, first.FrameNumber, "first accepted runtime frame");
    AssertEqual((ushort)SnesButton.Start, first.ControllerNewInput, "runtime latches controller before logic");
    AssertEqual(EscapeTimerState.InitialDelay, first.EscapeTimerState, "runtime dispatches timer after NMI");
    AssertEqual(0xca, runtime.Vram.ReadByte(0x40), "runtime NMI drains VRAM low byte");
    AssertEqual(0xfe, runtime.Vram.ReadByte(0x41), "runtime NMI drains VRAM high byte");
    AssertEqual(0, runtime.VramWrites.TailInBytes, "runtime NMI clears VRAM queue");
    AssertEqual(firstMode7, runtime.DisplayedSamusMode7Transform,
        "accepted NMI publishes Mode 7 shadow registers");

    var secondMode7 = new SamusMode7Transform(0x00e0, 0x0020, 0xffe0, 0x0080, 0x03f0);
    runtime.ActiveSamusMode7Transform = secondMode7;
    RuntimeFrameResult second = runtime.StepFrame((ushort)SnesButton.Start);
    AssertEqual(0, second.ControllerNewInput, "second runtime frame sees stable hold");
    AssertEqual(2, runtime.NmiFrameCounter, "accepted NMI counter advances twice");
    AssertEqual(secondMode7, runtime.DisplayedSamusMode7Transform,
        "following accepted NMI publishes the changed Mode 7 matrix");

    // A lag NMI must not sample the changed input or advance accepted-frame state.
    var laggedMode7 = new SamusMode7Transform(0x00d0, 0x0030, 0xffd0, 0x0080, 0x03f0);
    runtime.ActiveSamusMode7Transform = laggedMode7;
    runtime.RunNmi((ushort)SnesButton.A, mainLoopRequestedNmi: false);
    AssertEqual((ushort)SnesButton.Start, runtime.Controller1.Current, "lag NMI skips controller read");
    AssertEqual(2, runtime.NmiFrameCounter, "lag NMI skips accepted frame counter");
    AssertEqual(1, runtime.NmiLagCounter, "lag NMI increments consecutive lag");
    AssertEqual(1, runtime.MaximumNmiLag, "lag NMI records maximum lag");
    AssertEqual(3, runtime.NmiCounterIncludingLag, "all-NMI counter includes lag");
    AssertEqual(secondMode7, runtime.DisplayedSamusMode7Transform,
        "lag NMI leaves displayed Mode 7 registers untouched");

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
    AssertEqual(0x80, bus.ReadByte(0x008000), "slow-bank first LoROM byte");
    AssertEqual(0x80, bus.ReadByte(0x808000), "FastROM mirror first LoROM byte");
    AssertEqual(0x81, bus.ReadByte(0x818000), "next FastROM bank advances $8000 bytes");
    AssertEqual(0xc0, bus.ReadByte(0xc08000), "bank C0 maps to physical ROM $200000");
    AssertEqual(0x200000, SuperMetroidAddressSpace.ToRomOffset(0xc08000), "native RomPtr mask mapping");

    bus.WriteByte(0x7e1234, 0x55);
    AssertEqual(0x55, bus.ReadByte(0x001234), "bank 00 low WRAM mirror");
    AssertEqual(0x55, bus.ReadByte(0x801234), "bank 80 low WRAM mirror");
    bus.WriteByte(0x7f1234, 0x66);
    AssertEqual(0x66, bus.ReadByte(0x7f1234), "second physical WRAM bank");
    AssertEqual(0x55, bus.ReadByte(0x7e1234), "WRAM banks remain independent");

    bus.WriteByte(0x700123, 0x77);
    AssertEqual(0x77, bus.ReadByte(0x702123), "8 KiB SRAM offset mirror");
    AssertEqual(0x77, bus.ReadByte(0xf00123), "8 KiB SRAM bank mirror");

    AssertThrows<InvalidOperationException>(() => bus.WriteByte(0x808000, 0), "ROM writes rejected");
    AssertThrows<InvalidOperationException>(() => bus.ReadByte(0x004000), "unimplemented register/expansion read rejected");
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
    AssertEqual(0xff, first.Y, "OAM negative Y wraps above screen");
    AssertTrue(first.IsLarge, "OAM size bit extracted from encoded X word");
    AssertEqual(5, first.Palette, "OAM caller palette replaces source palette");
    AssertEqual(0x0aa, first.TileNumber, "OAM tile number retained");

    OamEntry second = oam.GetEntry(1);
    AssertEqual(0x0ee, second.X, "OAM signed 9-bit negative X offset");
    AssertEqual(0x21, second.Y, "OAM positive Y offset");
    AssertTrue(!second.IsLarge, "OAM second size bit clear");
    AssertEqual(0x03, oam.HighTable[0], "OAM four-sprite shared high byte");

    oam.FinalizeFrame();
    AssertEqual(2, oam.LastFinalizedSpriteCount, "OAM finalized used count");
    AssertEqual(0xf0, oam.GetEntry(2).Y, "OAM first unused sprite parked offscreen");
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
    AssertEqual(0xe0, clipped.Y, "vertically clipped OAM Y park position");

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
    AssertEqual(0xf0, enemyFirst.Y, "on-screen origin hides uncrossed negative Y");
    AssertTrue(enemyFirst.IsLarge, "enemy-projectile size comes from encoded X bit fifteen");
    AssertEqual(0x013, enemyFirst.TileNumber, "enemy-projectile base tile uses addition");
    AssertEqual(5, enemyFirst.Palette, "enemy-projectile graphics palette OR");
    AssertEqual(2, enemyFirst.Priority, "enemy-projectile source priority survives palette OR");
    OamEntry enemySecond = oam.GetEntry(1);
    AssertEqual(0x0ee, enemySecond.X, "enemy-projectile signed nine-bit negative X");
    AssertEqual(0x03, enemySecond.Y, "on-screen origin retains uncrossed positive Y");
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
    AssertEqual(0xf0, oam.GetEntry(0).Y,
        "off-screen origin hides negative piece that remains above screen");
    AssertEqual(0x01, oam.GetEntry(1).Y,
        "off-screen origin admits positive piece crossing into screen");

    // Ordinary enemies use a third, deliberately separate common writer at $81:8AB8.
    // Its direct pointer lives in the enemy's own data bank; no off-screen Y parking is
    // performed, and the graphics-set tile base is added to the complete attribute word
    // before the selected OBJ palette is ORed. The second entry crosses tile $1FF so its
    // carry into priority is observable rather than masked away by this fixture.
    bus.WriteBytes(0xa29000, [
        0x02, 0x00,
        0x05, 0x80, 0xfe, 0x10, 0x20,
        0xf0, 0x01, 0x02, 0xfe, 0x21,
    ]);
    oam.BeginFrame();
    oam.AddEnemySpritemap(
        bus,
        bank: 0xa2,
        spritemapPointer: 0x9000,
        originX: 0x00fe,
        originY: 0x0001,
        paletteBits: 0x0e00,
        baseTileIndex: 4);
    OamEntry roomEnemyFirst = oam.GetEntry(0);
    AssertEqual(0x103, roomEnemyFirst.X, "room-enemy complete encoded X addition");
    AssertEqual(0xff, roomEnemyFirst.Y, "room-enemy negative Y wraps without parking");
    AssertTrue(roomEnemyFirst.IsLarge, "room-enemy size comes from encoded X bit fifteen");
    AssertEqual(0x014, roomEnemyFirst.TileNumber, "room-enemy graphics-set tile-base addition");
    AssertEqual(7, roomEnemyFirst.Palette, "room-enemy selected OBJ palette OR");
    AssertEqual(2, roomEnemyFirst.Priority, "room-enemy source priority survives palette OR");
    OamEntry roomEnemySecond = oam.GetEntry(1);
    AssertEqual(0x0ee, roomEnemySecond.X, "room-enemy signed nine-bit negative X");
    AssertEqual(0x03, roomEnemySecond.Y, "room-enemy positive Y offset");
    AssertEqual(0x002, roomEnemySecond.TileNumber, "room-enemy tile overflow wraps at nine bits");
    AssertEqual(2, roomEnemySecond.Priority,
        "room-enemy tile-base ADC carry reaches the attribute high byte");

    // Extended enemy components use `$81:8B22/$81:8B96` rather than the ordinary
    // `$81:8AB8` writer. The same two-piece fixture therefore exposes both complementary
    // boundary rules: an on-screen origin parks a negative piece that failed to carry,
    // while an off-screen origin admits only the positive piece that did carry to Y=$01.
    oam.BeginFrame();
    oam.AddEnemySpritemap(
        bus,
        bank: 0xa2,
        spritemapPointer: 0x9000,
        originX: 0,
        originY: 0x0001,
        paletteBits: 0,
        baseTileIndex: 0,
        clipVerticalWrap: true,
        originYIsOnScreen: true);
    AssertEqual(0xf0, oam.GetEntry(0).Y,
        "extended enemy on-screen origin parks uncrossed negative piece");
    AssertEqual(0x03, oam.GetEntry(1).Y,
        "extended enemy on-screen origin retains positive piece");

    oam.BeginFrame();
    oam.AddEnemySpritemap(
        bus,
        bank: 0xa2,
        spritemapPointer: 0x9000,
        originX: 0,
        originY: 0xffff,
        paletteBits: 0,
        baseTileIndex: 0,
        clipVerticalWrap: true,
        originYIsOnScreen: false);
    AssertEqual(0xf0, oam.GetEntry(0).Y,
        "extended enemy off-screen origin parks negative piece that remains above screen");
    AssertEqual(0x01, oam.GetEntry(1).Y,
        "extended enemy off-screen origin admits positive piece crossing onto screen");

    Console.WriteLine(
        "  OAM: spritemap packing, enemy/enemy-projectile arithmetic, extended clipping, wrap, and finalization agree.");
}

/// <summary>
/// Fixes the first Samus slice to the retail pose-$01 pointer chain and independently
/// checks palette placement, split tile DMA, position math, and native OAM attributes.
/// </summary>
}
