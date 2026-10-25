using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Hud;
using static SuperMetroid.Verification.Frontend.Recording.ControllerInputRecordingVerification;
using static SuperMetroid.Verification.Hardware.DmaSourceRoutingVerification;
using static SuperMetroid.Verification.Areas.Escape.EscapeTimerFloorVerification;
using static SuperMetroid.Verification.Hardware.QueuedVramAssetsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.AssertionsVerification;
using static SuperMetroid.Verification.Hardware.ImportedSpritemapsVerification;
using static SuperMetroid.Verification.Frontend.Map.MapSpritesVerification;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.VerificationOracles;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Hardware;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Escape;

/// <summary>Checks hardware math, timing, controller input, memory, DMA, and OAM contracts.</summary>
internal static class HardwareVerification
{

/// <summary>
/// Compares the production port against a mechanically different description of the
/// original 65C816 register operations for every possible seed.
/// </summary>
internal static void VerifyRandomNumberGeneratorExhaustively()
{
    for (int seed = ushort.MinValue; seed <= ushort.MaxValue; seed++)
    {
        var state = new SharedGameState();
        state.SetRandomNumber((ushort)seed);
        ushort expected = ReferenceNextRandom((ushort)seed);
        int actual = state.NextRandom();
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
internal static void VerifyKnownRandomSequence()
{
    int[] expected = [
        0x02f6, 0x0fdf, 0x506c, 0x932d,
        0xe0f2, 0x65cb, 0xfe08, 0xf739,
    ];

    var state = new SharedGameState();
    int[] actual = Enumerable.Range(0, expected.Length)
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
internal static void VerifyTimedHeldInputTimeline()
{
    const ControllerButton button = ControllerButton.A;
    var state = new SharedGameState();

    // Frame 0: the button is new, so TRB removes it from the held-only sample.
    state.UpdateHeldInput(timerReset: 3, controllerInput: button, controllerNewInput: button);
    AssertEqual(ControllerButton.None, state.TimedHeldInput, "new press is not held input");

    // Frame 1: held-only input changes from zero to the button and reloads the timer.
    state.UpdateHeldInput(3, button, 0);
    AssertEqual(3, state.TimedHeldInputTimer.RawValue, "changed input reloads timer");
    AssertEqual(ControllerButton.None, state.TimedHeldInput, "changed input stays suppressed");

    // Frames 2-4: DEC yields 2, 1, and 0. BPL keeps taking the suppression path.
    for (ushort expectedTimer = 2; ; expectedTimer--)
    {
        state.UpdateHeldInput(3, button, 0);
        AssertEqual(expectedTimer, state.TimedHeldInputTimer.RawValue, "stable-input countdown");
        AssertEqual(ControllerButton.None, state.TimedHeldInput, "countdown suppression");
        if (expectedTimer == 0)
            break;
    }

    // Frame 5: zero wraps to $FFFF, which is negative to BPL, and activates the button.
    state.UpdateHeldInput(3, button, 0);
    AssertEqual(0, state.TimedHeldInputTimer.RawValue, "expired timer is pinned to zero");
    AssertEqual(button, state.TimedHeldInput, "held input activates after underflow");
    AssertEqual(button, state.NewlyTimedHeldInput, "activation creates a rising-edge pulse");

    // Frame 6: the active input remains present, while its rising-edge output clears.
    state.UpdateHeldInput(3, button, 0);
    AssertEqual(button, state.TimedHeldInput, "stable expired input remains active");
    AssertEqual(ControllerButton.None, state.NewlyTimedHeldInput, "rising-edge pulse lasts one update");

    // Releasing changes the held sample, reloads the timer, and suppresses the output.
    state.UpdateHeldInput(3, 0, 0);
    AssertEqual(3, state.TimedHeldInputTimer.RawValue, "release reloads timer");
    AssertEqual(ControllerButton.None, state.TimedHeldInput, "release clears filtered input");
    AssertEqual(ControllerButton.None, state.NewlyTimedHeldInput, "release is not a new held press");

    Console.WriteLine("  Input: delayed-held timeline and edge pulse agree.");
}

/// <summary>
/// Exercises every allocated event bit and verifies its exact containing byte. This catches
/// swapped shift/index operations as well as clear-mask width errors.
/// </summary>
internal static void VerifyEventBitfield()
{
    var state = new SharedGameState();
    for (int eventNumber = 0; eventNumber < SharedGameState.EventByteCount * 8; eventNumber++)
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
internal static void VerifyBossBitfield()
{
    var state = new SharedGameState();
    for (int area = 0; area < SharedGameState.AreaCount; area++)
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
        () => BossBitMasksConstants.FromCartridge(0x80, "constructed selector"),
        "unnamed cartridge boss mask rejected");
    Console.WriteLine("  Boss state: all eight area bytes preserve independent masks.");
}

/// <summary>
/// The full Cartesian product would require 2^32 cases. Iterating all left operands against
/// carefully selected right operands covers every input bit, carry boundary, and high-word
/// behavior in a fraction of the time.
/// </summary>
internal static void VerifyMultiplicationExhaustively()
{
    ushort[] rightOperands = [0, 1, 2, 3, 0x00ff, 0x0100, 0x7fff, 0x8000, 0xffff];
    foreach (ushort right in rightOperands)
    {
        for (int left = ushort.MinValue; left <= ushort.MaxValue; left++)
        {
            uint expected = (uint)left * right;
            uint actual = SharedGameState.Multiply16By16((ushort)left, right);
            AssertEqual(expected, actual, $"multiply ${left:X4} by ${right:X4}");
        }
    }

    Console.WriteLine("  Multiply: 589,824 boundary-spanning products agree.");
}

/// <summary>
/// Verifies both VMAIN modes used by the queue, packed-tail accounting, ordered overwrite behavior, the drain's
/// refusal of a native-source record no installed artwork resolves, and the NMI consumer's unconditional queue reset.
/// </summary>
[PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
[CpuBusBoundary("exact bus test: a full DAS-zero channel DMA's A-bus source reads that wrap within WRAM bank $7E")]
internal static void VerifyVramWriteQueue()
{
    var vram = new SnesVram();
    var queue = new VramWriteQueue();

    queue.EnqueueWords(sizeInBytes: 4, nativeSourceAddress: 0x7e1000, [0x1110, 0x1312], VramDmaDestination.FromNative(0x0001));
    AssertEqual(VramWriteQueue.EntryByteCount, queue.TailInBytes, "one packed VRAM queue tail");

    // Setting destination bit 15 selects VMAIN=$81. The physical start remains word one,
    // but the next word lands 32 words later at word $0021.
    queue.EnqueueWords(sizeInBytes: 4, nativeSourceAddress: 0x7e2000, [0x2120, 0x2322], VramDmaDestination.FromNative(0x8001));
    AssertEqual(VramWriteQueue.EntryByteCount * 2, queue.TailInBytes, "two packed VRAM queue tails");

    queue.DrainTo(vram, null, null, BgArtworkFixtures.For(vram));

    // The later column transfer intentionally overwrites the first pair at word one. This
    // proves insertion order is preserved, while word two retains the ordinary transfer.
    AssertEqual(0x20, vram.ReadByte(0x0001 * 2), "column transfer low byte at first word");
    AssertEqual(0x21, vram.ReadByte(0x0001 * 2 + 1), "column transfer high byte at first word");
    AssertEqual(0x12, vram.ReadByte(0x0002 * 2), "linear transfer increments one word");
    AssertEqual(0x13, vram.ReadByte(0x0002 * 2 + 1), "linear transfer high byte");
    AssertEqual(0x22, vram.ReadByte(0x0021 * 2), "column transfer increments 32 words");
    AssertEqual(0x23, vram.ReadByte(0x0021 * 2 + 1), "column transfer second high byte");

    // A native-source record names cartridge artwork; without installed artwork that resolves it, the drain fails
    // rather than reading memory.
    var unresolved = new VramWriteQueue();
    unresolved.Enqueue(sizeInBytes: 2, sourceAddress: 0x7e1000, destination: VramDmaDestination.FromNative(0x0100));
    AssertThrows<InvalidOperationException>(() => unresolved.DrainTo(vram, null, null, BgArtworkFixtures.For(vram)), "an unresolved native-source record fails at the drain");

    AssertEqual(0, queue.TailInBytes, "drain clears packed tail");
    AssertEqual(0, queue.Entries.Count, "drain clears typed records");
    AssertThrows<ArgumentOutOfRangeException>(() => queue.Enqueue(0, 0x808000, VramDmaDestination.FromNative(0)), "zero-size VRAM entry rejected");
    AssertThrows<ArgumentOutOfRangeException>(() => queue.Enqueue(1, 0x1000000, VramDmaDestination.FromNative(0)), "25-bit VRAM source rejected");
    AssertEqual(VramWriteEntry.MaximumSizeInBytes, new VramWriteEntry(VramWriteEntry.MaximumSizeInBytes, 0, VramDmaDestination.FromNative(0)).SizeInBytes,
        "the largest sixteen-bit DMA byte count is a queued size");
    AssertThrows<ArgumentOutOfRangeException>(() => new VramWriteEntry(VramWriteEntry.MaximumSizeInBytes + 1, 0, VramDmaDestination.FromNative(0)),
        "a queued size beyond the sixteen-bit DMA byte count is rejected");
    AssertThrows<ArgumentOutOfRangeException>(() => new VramWriteEntry(-1, 0, VramDmaDestination.FromNative(0)), "a negative queued size is rejected");

    // A direct channel DAS of zero is not the queue's zero-word terminator. Hardware
    // decrements the sixteen-bit counter through all $10000 values, while A1T and VMADD
    // wrap independently inside their fixed banks/address widths. Start at xx:FFFE so the
    // first four bytes prove the A-bus wrap, then mark the final source pair as well.
    var bus = new TestAddressSpace();
    var fullDmaVram = new SnesVram();
    var dirtyVram = new byte[SnesVram.ByteCount];
    Array.Fill(dirtyVram, (byte)0x7e);
    fullDmaVram.LoadBytes(0, dirtyVram);
    // A full 64-KiB transfer must stay in a fully mapped source bank. The
    // separate routing check below covers a ROM-bank wrap into the WRAM mirror.
    bus.WriteByte(0x7efffe, 0xa0);
    bus.WriteByte(0x7effff, 0xa1);
    bus.WriteByte(0x7e0000, 0xa2);
    bus.WriteByte(0x7e0001, 0xa3);
    bus.WriteByte(0x7efffc, 0xfe);
    bus.WriteByte(0x7efffd, 0xff);
    fullDmaVram.ExecuteHardwareMemoryDmaWrite(
        bus,
        sourceAddress: 0x7efffe,
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
        fullQueue.Enqueue(1, 0x808000, VramDmaDestination.FromNative(0));
    AssertThrows<InvalidOperationException>(() => fullQueue.Enqueue(1, 0x808000, VramDmaDestination.FromNative(0)), "VRAM queue overflow rejected");

    Console.WriteLine("  VRAM: write queue, VMAIN stepping, bank/VMADD wrap, DAS zero, and reset agree.");
}

/// <summary>
/// Checks every valid two-digit BCD input against ordinary decimal subtraction for the two
/// correction values the ROM table uses.
/// </summary>
internal static void VerifyEscapeTimerBcd()
{
    for (int decimalValue = 0; decimalValue <= 99; decimalValue++)
    {
        byte packed = PackBcd(decimalValue);
        for (byte decrement = 1; decrement <= 2; decrement++)
        {
            byte actual = new PackedBcd(packed).Subtract(new PackedBcd(decrement), out bool actualBorrow).Raw;
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
internal static void VerifyEscapeTimerStateMachine()
{
    var timer = new EscapeTimer();
    timer.RequestCeresStart();
    AssertTrue(!timer.Process(0), "Ceres initialization does not report expiration");
    AssertEqual(EscapeTimerState.InitialDelay, timer.State, "Ceres enters initial delay");
    AssertEqual(0x01, timer.Minutes.Raw, "Ceres starts at one minute");
    AssertEqual(0x8000, timer.Position.X, "timer starts at X $80.00");
    AssertEqual(0x8000, timer.Position.Y, "timer starts at Y $80.00");

    // State 3 increments the low X byte from $00 through $10 without decrementing time.
    for (ushort frame = 1; frame <= 16; frame++)
        AssertTrue(!timer.Process(frame), $"initial-delay frame {frame}");
    AssertEqual(EscapeTimerState.RunningMovementDelayed, timer.State, "initial delay advances at counter $10");
    AssertEqual(0x8010, timer.Position.X, "X subpixel byte contains delay counter $10");
    AssertEqual(0x00, timer.Seconds.Raw, "initial delay did not decrement seconds");

    // State 4 continues the same aliased counter to $60, then zeroes only its low byte.
    for (ushort frame = 17; frame <= 96; frame++)
        timer.Process(frame);
    AssertEqual(EscapeTimerState.RunningMovingIntoPlace, timer.State, "movement delay advances at counter $60");
    AssertEqual(0x8000, timer.Position.X, "movement transition clears X fraction only");

    // X reaches its clamp in 106 frames; Y requires 107, so the state advances on 107.
    for (ushort frame = 97; frame <= 203; frame++)
        timer.Process(frame);
    AssertEqual(EscapeTimerState.RunningInPlace, timer.State, "timer reaches stationary running state");
    AssertEqual(0xdc00, timer.Position.X, "timer X clamps at pixel 220");
    AssertEqual(0x3000, timer.Position.Y, "timer Y clamps at pixel 48");

    var borrowSecond = CreateRunningTimer(0x00, 0x01, 0x00);
    AssertTrue(!borrowSecond.Process(1), "00:01.00 remains nonzero after decrement");
    AssertEqual(0x00, borrowSecond.Seconds.Raw, "centisecond borrow decrements seconds");
    AssertEqual(0x98, borrowSecond.Centiseconds.Raw, "table value two wraps centiseconds to 98");

    var borrowMinute = CreateRunningTimer(0x01, 0x00, 0x00);
    AssertTrue(!borrowMinute.Process(0), "01:00.00 remains nonzero after decrement");
    AssertEqual(0x00, borrowMinute.Minutes.Raw, "second borrow decrements minutes");
    AssertEqual(0x59, borrowMinute.Seconds.Raw, "minute borrow reloads seconds to 59");
    AssertEqual(0x99, borrowMinute.Centiseconds.Raw, "table value one wraps centiseconds to 99");

    var expiration = CreateRunningTimer(0x00, 0x00, 0x01);
    AssertTrue(expiration.Process(0), "00:00.01 expires on a decrement of one");
    AssertEqual(0, expiration.Centiseconds.Raw, "expiration saturates centiseconds");

    var underflow = CreateRunningTimer(0x00, 0x00, 0x00);
    AssertTrue(underflow.Process(1), "zero timer remains expired after attempted decrement");
    AssertEqual(0, underflow.Minutes.Raw, "underflow saturates minutes");
    AssertEqual(0, underflow.Seconds.Raw, "underflow saturates seconds");
    AssertEqual(0, underflow.Centiseconds.Raw, "underflow saturates centiseconds");

    var motherBrain = new EscapeTimer();
    motherBrain.RequestMotherBrainStart();
    motherBrain.Process(0);
    AssertEqual(0x03, motherBrain.Minutes.Raw, "Mother Brain starts at three minutes");
    AssertTrue(motherBrain.IsActive && motherBrain.State == EscapeTimerState.InitialDelay, "a started timer runs from its initial delay");

    Console.WriteLine("  Timer: initialization, animation, BCD borrow, and expiration agree.");
}

/// <summary>Reaches the timer's running state through its real startup path before setting BCD time.</summary>
static EscapeTimer CreateRunningTimer(byte minutes, byte seconds, byte centiseconds)
{
    // Advancing the startup path is preferable to adding a production-only raw status
    // setter: this fixture reaches the real state-$8006 path exactly as gameplay does.
    var timer = new EscapeTimer();
    timer.RequestCeresStart();
    timer.Process(0);
    for (ushort frame = 1; timer.State != EscapeTimerState.RunningInPlace; frame++)
        timer.Process(frame);
    timer.SetTime(new PackedBcd(minutes), new PackedBcd(seconds), new PackedBcd(centiseconds));
    return timer;
}

/// <summary>Packs a decimal value into the two-digit BCD form used by the escape timer.</summary>
static byte PackBcd(int decimalValue) => (byte)(((decimalValue / 10) << 4) | (decimalValue % 10));

/// <summary>
/// Exercises rising edges, releases, multi-frame holds, and the otherwise-unused repeat
/// path from the NMI controller routine.
/// </summary>
internal static void VerifyControllerInputLatch()
{
    ControllerButton button = ControllerButton.A;
    var input = new ControllerInputState(initialRepeatDelay: 2, subsequentRepeatDelay: 1);

    input.Latch(button);
    AssertEqual(button, input.Current, "controller current on first press");
    AssertEqual(button, input.NewlyPressed, "controller rising edge on first press");
    AssertEqual(button, input.NewlyPressedWithRepeat, "repeat output includes real first press");
    AssertEqual(2, input.RepeatTimer.RawValue, "first press loads initial repeat delay");

    input.Latch(button);
    AssertEqual(ControllerButton.None, input.NewlyPressed, "stable hold has no new edge");
    AssertEqual(ControllerButton.None, input.NewlyPressedWithRepeat, "first held frame has no repeat");
    AssertEqual(1, input.RepeatTimer.RawValue, "held input decrements repeat timer");

    input.Latch(button);
    AssertEqual(button, input.NewlyPressedWithRepeat, "repeat timer zero emits held input");
    AssertEqual(1, input.RepeatTimer.RawValue, "repeat pulse loads subsequent delay");

    input.Latch(0);
    AssertEqual(ControllerButton.None, input.NewlyPressed, "release is not a rising edge");
    AssertEqual(2, input.RepeatTimer.RawValue, "release reloads initial repeat delay");

    // Direction-sensitive enemy AI masks only the horizontal pair, so unrelated face
    // buttons must not prevent an exact-Right decision while Left+Right must.
    var directional = new ControllerInputState();
    directional.Latch((ControllerButton.Right | ControllerButton.A));
    AssertEqual(
        ControllerButton.Right | ControllerButton.A,
        directional.NewlyPressed,
        "typed newly-pressed view preserves every rising edge in a chord");
    AssertTrue(
        (directional.CurrentButtons & ControllerButtons.HorizontalDirections) == ControllerButton.Right,
        "exact-Right comparison ignores unrelated action buttons");
    directional.Latch((ControllerButton.Right | ControllerButton.Left | ControllerButton.A));
    AssertTrue(
        (directional.CurrentButtons & ControllerButtons.HorizontalDirections) != ControllerButton.Right,
        "exact-Right comparison rejects opposed horizontal input");
    AssertEqual(ControllerButton.Left, directional.NewlyPressed,
        "typed newly-pressed view reports only the added direction");

    // A real SNES controller cannot close both contacts, but adapter/SMV words can. The
    // cartridge latches that raw chord without normalizing it to neutral. Added directions
    // remain independent rising edges, which is observable in L+R movement techniques.
    ControllerButton opposedDirections = (ControllerButton.Left | ControllerButton.Right);
    var simultaneous = new ControllerInputState();
    simultaneous.Latch(opposedDirections);
    AssertEqual(opposedDirections, simultaneous.Current,
        "simultaneous Left+Right survives the raw controller latch");
    AssertEqual(opposedDirections, simultaneous.NewlyPressed,
        "simultaneous first press publishes both direction edges");
    simultaneous.Latch(opposedDirections);
    AssertEqual(ControllerButton.None, simultaneous.NewlyPressed,
        "stable simultaneous directions do not repeat as new input");
    simultaneous.Latch((opposedDirections | ControllerButton.Up));
    AssertEqual(ControllerButton.Up, simultaneous.NewlyPressed,
        "elevator direction remains a new edge while Left+Right stays held");

    var leftThenBoth = new ControllerInputState();
    leftThenBoth.Latch(ControllerButton.Left);
    leftThenBoth.Latch(opposedDirections);
    AssertEqual(ControllerButton.Right, leftThenBoth.NewlyPressed,
        "adding Right to held Left publishes only the Right edge");

    var rightThenBoth = new ControllerInputState();
    rightThenBoth.Latch(ControllerButton.Right);
    rightThenBoth.Latch(opposedDirections);
    AssertEqual(ControllerButton.Left, rightThenBoth.NewlyPressed,
        "adding Left to held Right publishes only the Left edge");

    Console.WriteLine("  Input: NMI latch, rising edges, and repeat countdown agree.");
}

/// <summary>
/// Confirms the integrated frame seam preserves NMI ordering, accepted/lagged counters,
/// controller latching, VRAM draining, and timer dispatch.
/// </summary>
[PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
internal static void VerifyFrameRuntime()
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
    AssertEqual(2, infiniteAmmoSamus.Missiles,
        "infinite ammo lends a frame-local final missile");
    AssertEqual(1, infiniteAmmoSamus.SuperMissiles,
        "infinite ammo repairs an exhausted unlocked type before selection");
    AssertEqual(0, infiniteAmmoSamus.PowerBombs,
        "infinite ammo does not unlock a missing upgrade");

    // Consume the lent missile exactly as the cartridge firing routine does. Completion
    // must keep it at one instead of allowing the native zero-ammo auto-cancel branch.
    infiniteAmmoSamus.Missiles--;
    ammoGuard.Complete(infiniteAmmoSamus);
    AssertEqual(1, infiniteAmmoSamus.Missiles,
        "infinite ammo preserves selection after the final missile");
    AssertEqual(1, infiniteAmmoSamus.SuperMissiles,
        "infinite ammo retains the unlocked one-unit floor");
    AssertEqual(0, infiniteAmmoSamus.PowerBombs,
        "infinite ammo leaves locked ammunition at zero");
    ammoGuard.Complete(infiniteAmmoSamus);
    AssertEqual(1, infiniteAmmoSamus.Missiles,
        "repeated frame-exit reconciliation cannot remove the one-unit floor");

    var pickupDuringLoan = new SamusState { Missiles = 1, MaxMissiles = 5 };
    HostInfiniteAmmoFrameGuard pickupGuard =
        HostInfiniteAmmoFrameGuard.Begin(enabled: true, pickupDuringLoan);
    pickupDuringLoan.Missiles += 2;
    pickupGuard.Complete(pickupDuringLoan);
    pickupGuard.Complete(pickupDuringLoan);
    AssertEqual(3, pickupDuringLoan.Missiles,
        "idempotent loan reconciliation preserves ammunition collected during the frame");

    var runtime = CreateRetailRuntimeFixture();
    // Gameplay frames run in a loaded room, entered after the HUD initializes as new-game setup does.
    runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
    runtime.RoomLoader.InitializeStartingCeresRoom();
    runtime.EscapeTimer.RequestCeresStart();

    // `$89:ACC3` writes Mode 7 shadow registers during room main. They must become visible
    // only when an accepted NMI reaches `$80:95A7`, at the same boundary as displayed OAM.
    var firstMode7 = new SamusMode7Transform(0x00f0, 0x0010, -0x0010, new Position16<RoomPixels>(0x0080, 0x03f0));
    runtime.Display.ActiveSamusMode7Transform = firstMode7;

    runtime.StepFrame(ControllerButton.Start);
    AssertEqual(1, runtime.Clock.Counter, "first accepted runtime frame");
    AssertEqual(ControllerButton.Start, runtime.Controller1.NewlyPressed, "runtime latches controller before logic");
    AssertEqual(EscapeTimerState.InitialDelay, runtime.EscapeTimer.State, "runtime dispatches timer after NMI");
    AssertEqual(CeresPerspectiveFloors.ElevatorShaft(firstMode7, runtime.Display.DisplayedGameplayPpu.Bg1Scroll),
        runtime.Display.DisplayedPerspectiveFloor, "the accepted frame shows the shaft's transform");

    var secondMode7 = new SamusMode7Transform(0x00e0, 0x0020, -0x0020, new Position16<RoomPixels>(0x0080, 0x03f0));
    runtime.Display.ActiveSamusMode7Transform = secondMode7;
    runtime.StepFrame(ControllerButton.Start);
    AssertEqual(ControllerButton.None, runtime.Controller1.NewlyPressed, "second runtime frame sees stable hold");
    AssertEqual(2, runtime.Clock.Counter, "accepted NMI counter advances twice");
    AssertEqual((ulong)2, runtime.FrameBoundary.CompletedGameplayAudioPublication,
        "complete gameplay passes publish distinct audio generations");
    AssertEqual(CeresPerspectiveFloors.ElevatorShaft(secondMode7, runtime.Display.DisplayedGameplayPpu.Bg1Scroll),
        runtime.Display.DisplayedPerspectiveFloor, "the following accepted frame shows the changed transform");

    // Door/pause frontend coroutines accept NMIs without running the gameplay owner list.
    // They must not make the frontend consume the preceding enemy/PLM audio publication
    // again, or a door waiting for unread SFX entries can refill its own queue forever.
    runtime.RunBlankGameplayFrame(controllerInput: 0);
    AssertEqual((ulong)2, runtime.FrameBoundary.CompletedGameplayAudioPublication,
        "NMI-only frontend frame does not republish gameplay audio");

    Console.WriteLine("  Runtime: frame seam, NMI order, and lag accounting agree.");
}

/// <summary>
/// Checks FastROM/slow-bank LoROM mirrors, high-bank offsets, WRAM mirrors, unmapped SRAM windows,
/// and deliberate failures for unimplemented hardware and ROM writes.
/// </summary>
[CpuBusBoundary("exact bus test: LoROM and WRAM mapping, unmapped SRAM, mirror and rejection behaviour of the address-space implementation")]
internal static void VerifySuperMetroidAddressSpace()
{
    var rom = new byte[SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace.RetailRomByteCount];
    rom[0x000000] = 0x80;
    rom[0x008000] = 0x81;
    rom[0x200000] = 0xc0;
    var bus = new SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace(rom);

    // Banks $00 and $80 are timing mirrors of the first physical 32 KiB ROM bank.
    AssertEqual(0x80, bus.ReadCartridgeByte(0x008000), "slow-bank first LoROM byte");
    AssertEqual(0x80, bus.ReadCartridgeByte(0x808000), "FastROM mirror first LoROM byte");
    AssertEqual(0x81, bus.ReadCartridgeByte(0x818000), "next FastROM bank advances $8000 bytes");
    AssertEqual(0xc0, bus.ReadCartridgeByte(0xc08000), "bank C0 maps to physical ROM $200000");
    var cartridge = bus;
    AssertEqual(0x80, cartridge.ReadCartridgeByte(0x808000),
        "import-only ROM reader returns the pinned upper-window byte");
    AssertThrows<ArgumentOutOfRangeException>(
        () => cartridge.ReadCartridgeByte(0x7e8000),
        "import-only ROM reader rejects the physical WRAM bank");
    AssertThrows<ArgumentOutOfRangeException>(
        () => cartridge.ReadCartridgeByte(0x001234),
        "import-only ROM reader rejects lower-window RAM mirrors");
    AssertEqual(0x200000, SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace.ToRomOffset(0xc08000), "native RomPtr mask mapping");

    bus.WriteByte(0x7e1234, 0x55);
    AssertEqual(0x55, bus.ReadWorkRamByte(0x001234), "bank 00 low WRAM mirror");
    AssertEqual(0x55, bus.ReadWorkRamByte(0x801234), "bank 80 low WRAM mirror");
    bus.WriteByte(0x7f1234, 0x66);
    AssertEqual(0x66, bus.ReadWorkRamByte(0x7f1234), "second physical WRAM bank");
    AssertEqual(0x55, bus.ReadWorkRamByte(0x7e1234), "WRAM banks remain independent");
    var mutable = bus;
    AssertEqual(0x55, mutable.ReadWorkRamByte(0x7e1234),
        "typed WRAM read returns the physical first bank");
    AssertEqual(0x66, mutable.ReadWorkRamByte(0x7f1234),
        "typed WRAM read returns the physical second bank");
    AssertThrows<ArgumentOutOfRangeException>(
        () => mutable.ReadWorkRamByte(0x808000),
        "typed WRAM read rejects cartridge ROM");
    AssertEqual(0x55, mutable.ReadWorkRamByte(0x001234),
        "typed WRAM read preserves the low bank-$00 mirror");
    AssertEqual(0x55, mutable.ReadWorkRamByte(0x801234),
        "typed WRAM read preserves the low bank-$80 mirror");
    AssertThrows<ArgumentOutOfRangeException>(
        () => mutable.ReadWorkRamByte(0x002000),
        "typed WRAM read rejects hardware registers outside the mirror");

    // Saves are SaveSlotStore state: the SRAM windows are outside the runtime address map.
    AssertThrows<InvalidOperationException>(() => bus.WriteByte(0x700123, 0x77), "SRAM window writes are unmapped");
    AssertEqual(SnesDmaSourceKind.Unmapped, SnesDmaSourceMap.Classify(SnesAddress.FromBusAddress(0x700123)),
        "DMA cannot read an SRAM window");

    var noCartridge = SuperMetroidAddressSpace.CreateWithoutCartridge();
    noCartridge.WriteByte(0x7e1234, 0x98);
    AssertEqual(0x98, ((ISnesMutableMemory)noCartridge).ReadWorkRamByte(0x7e1234),
        "typed WRAM read works without a cartridge allocation");
    AssertTrue(noCartridge is not IImportCartridgeSource,
        "runtime memory does not implement the import-only cartridge capability");
    AssertThrows<ArgumentException>(
        () => noCartridge.ReadCartridgeByte(0x808000),
        "missing cartridge cannot be masked by the import-only reader");

    AssertThrows<InvalidOperationException>(() => bus.WriteByte(0x808000, 0), "ROM writes rejected");
    AssertEqual(SnesDmaSourceKind.Unmapped, SnesDmaSourceMap.Classify(SnesAddress.FromBusAddress(0x004000)),
        "unimplemented register/expansion window is outside the runtime address map");
    AssertThrows<ArgumentOutOfRangeException>(() => bus.ReadWorkRamByte(0x004000), "unimplemented register/expansion read rejected as WRAM");
    AssertThrows<ArgumentOutOfRangeException>(() => bus.ReadCartridgeByte(0x004000), "unimplemented register/expansion read rejected as ROM");
    AssertThrows<ArgumentOutOfRangeException>(() => SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpace.ToRomOffset(0x800000), "lower LoROM offset rejected");

    Console.WriteLine("  Bus: LoROM, WRAM mirrors, unmapped SRAM and protection agree.");
}

/// <summary>
/// Builds synthetic packed spritemaps to verify signed/modular offsets, shared high-OAM
/// pairs, palette replacement, clipping, finalization, and the exact 544-byte upload size.
/// </summary>
[PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
internal static void VerifyOamSpritemapPacking()
{
    var bus = new TestAddressSpace();
    var oam = new OamBuffer();

    // Two entries at $80:8000. Entry 0 is large, +5 X, -2 Y. Entry 1 is small,
    // -16 X, +32 Y. Attribute palettes are intentionally replaced by caller palette 5.
    SeedTwoEntryCommonSpritemap(bus);

    oam.BeginFrame();
    DrawImportedSpritemap(bus, oam, 0x808000, origin: new(0x00fe, 0x0001), paletteBits: 0x0a00);

    OamEntry first = oam.GetEntry(0);
    AssertEqual(0x103, first.Position.X, "OAM first 9-bit X");
    AssertEqual(0xff, first.Position.Y, "OAM negative Y wraps above screen");
    AssertTrue(first.IsLarge, "OAM size bit extracted from encoded X word");
    AssertEqual(5, first.Palette, "OAM caller palette replaces source palette");
    AssertEqual(0x0aa, first.TileNumber, "OAM tile number retained");

    OamEntry second = oam.GetEntry(1);
    AssertEqual(0x0ee, second.Position.X, "OAM signed 9-bit negative X offset");
    AssertEqual(0x21, second.Position.Y, "OAM positive Y offset");
    AssertTrue(!second.IsLarge, "OAM second size bit clear");
    AssertEqual(0x03, oam.HighTable[0], "OAM four-sprite shared high byte");

    oam.FinalizeFrame();
    AssertEqual(2, oam.LastFinalizedSpriteCount, "OAM finalized used count");
    AssertEqual(0xf0, oam.GetEntry(2).Position.Y, "OAM first unused sprite parked offscreen");
    AssertEqual(0, oam.NextByteOffset, "OAM finalization resets stack pointer");
    AssertEqual(OamBufferConstants.UploadByteCount, oam.CreateUploadPayload().Length, "OAM DMA payload size");

    // A positive Y offset reaching $E0 takes the explicit X=$180/Y=$E0 hide path.
    SeedVerticallyClippedCommonSpritemap(bus);
    oam.BeginFrame();
    DrawImportedSpritemap(bus, oam, 0x808100, origin: new(0, 0x00c0), paletteBits: 0);
    OamEntry clipped = oam.GetEntry(0);
    AssertEqual(0x180, clipped.Position.X, "vertically clipped OAM X park position");
    AssertEqual(0xe0, clipped.Position.Y, "vertically clipped OAM Y park position");

    // Enemy projectiles use the distinct bank-$8D loader at `$81:8C0A/$81:8C7F`.
    // The first entry is large/+5 X/-2 Y; the second is small/-16 X/+2 Y. Source
    // attribute `$21FE + $04` deliberately carries from tile number into the high byte,
    // proving that the native routine uses ADC before ORing graphics-index palette bits.
    SeedTwoEntryEnemyProjectileSpritemap(bus);
    oam.BeginFrame();
    DrawImportedEnemyProjectileSpritemap(
        bus, oam,
        bank8dSpritemapPointer: 0x9800,
        origin: new(0x00fe, 0x0001),
        graphicsIndex: 0x0a04,
        originYIsOnScreen: true);
    OamEntry enemyFirst = oam.GetEntry(0);
    AssertEqual(0x103, enemyFirst.Position.X, "enemy-projectile complete encoded X addition");
    AssertEqual(0xf0, enemyFirst.Position.Y, "on-screen origin hides uncrossed negative Y");
    AssertTrue(enemyFirst.IsLarge, "enemy-projectile size comes from encoded X bit fifteen");
    AssertEqual(0x013, enemyFirst.TileNumber, "enemy-projectile base tile uses addition");
    AssertEqual(5, enemyFirst.Palette, "enemy-projectile graphics palette OR");
    AssertEqual(2, enemyFirst.Priority, "enemy-projectile source priority survives palette OR");
    OamEntry enemySecond = oam.GetEntry(1);
    AssertEqual(0x0ee, enemySecond.Position.X, "enemy-projectile signed nine-bit negative X");
    AssertEqual(0x03, enemySecond.Position.Y, "on-screen origin retains uncrossed positive Y");
    AssertTrue(!enemySecond.IsLarge, "enemy-projectile small size bit");
    AssertEqual(0x002, enemySecond.TileNumber, "enemy-projectile ADC tile carry");

    // An origin at Y=$FFFF selects `$81:8C7F`'s opposite carry rule. The negative piece
    // remains above screen and is parked, while +2 crosses into visible Y=$01.
    oam.BeginFrame();
    DrawImportedEnemyProjectileSpritemap(
        bus, oam,
        bank8dSpritemapPointer: 0x9800,
        origin: new(0, 0xffff),
        graphicsIndex: 0,
        originYIsOnScreen: false);
    AssertEqual(0xf0, oam.GetEntry(0).Position.Y,
        "off-screen origin hides negative piece that remains above screen");
    AssertEqual(0x01, oam.GetEntry(1).Position.Y,
        "off-screen origin admits positive piece crossing into screen");

    // Ordinary enemies use a third, deliberately separate common writer at $81:8AB8.
    // Its direct pointer lives in the enemy's own data bank; no off-screen Y parking is
    // performed, and the graphics-set tile base is added to the complete attribute word
    // before the selected OBJ palette is ORed. The second entry crosses tile $1FF so its
    // carry into priority is observable rather than masked away by this fixture.
    SeedTwoEntryRoomEnemySpritemap(bus);
    oam.BeginFrame();
    DrawImportedEnemySpritemap(bus, oam, new NativeEnemySpritemapDraw { Bank = 0xa2, Pointer = 0x9000, Origin = new(0x00fe, 0x0001), PaletteBits = 0x0e00, BaseTileIndex = 4 });
    OamEntry roomEnemyFirst = oam.GetEntry(0);
    AssertEqual(0x103, roomEnemyFirst.Position.X, "room-enemy complete encoded X addition");
    AssertEqual(0xff, roomEnemyFirst.Position.Y, "room-enemy negative Y wraps without parking");
    AssertTrue(roomEnemyFirst.IsLarge, "room-enemy size comes from encoded X bit fifteen");
    AssertEqual(0x014, roomEnemyFirst.TileNumber, "room-enemy graphics-set tile-base addition");
    AssertEqual(7, roomEnemyFirst.Palette, "room-enemy selected OBJ palette OR");
    AssertEqual(2, roomEnemyFirst.Priority, "room-enemy source priority survives palette OR");
    OamEntry roomEnemySecond = oam.GetEntry(1);
    AssertEqual(0x0ee, roomEnemySecond.Position.X, "room-enemy signed nine-bit negative X");
    AssertEqual(0x03, roomEnemySecond.Position.Y, "room-enemy positive Y offset");
    AssertEqual(0x002, roomEnemySecond.TileNumber, "room-enemy tile overflow wraps at nine bits");
    AssertEqual(2, roomEnemySecond.Priority,
        "room-enemy tile-base ADC carry reaches the attribute high byte");

    // Extended enemy components use `$81:8B22/$81:8B96` rather than the ordinary
    // `$81:8AB8` writer. The same two-piece fixture therefore exposes both complementary
    // boundary rules: an on-screen origin parks a negative piece that failed to carry,
    // while an off-screen origin admits only the positive piece that did carry to Y=$01.
    oam.BeginFrame();
    DrawImportedEnemySpritemap(bus, oam, new NativeEnemySpritemapDraw { Bank = 0xa2, Pointer = 0x9000, Origin = new(0, 0x0001), PaletteBits = 0, BaseTileIndex = 0, ClipVerticalWrap = true, OriginYIsOnScreen = true });
    AssertEqual(0xf0, oam.GetEntry(0).Position.Y,
        "extended enemy on-screen origin parks uncrossed negative piece");
    AssertEqual(0x03, oam.GetEntry(1).Position.Y,
        "extended enemy on-screen origin retains positive piece");

    oam.BeginFrame();
    DrawImportedEnemySpritemap(bus, oam, new NativeEnemySpritemapDraw { Bank = 0xa2, Pointer = 0x9000, Origin = new(0, 0xffff), PaletteBits = 0, BaseTileIndex = 0, ClipVerticalWrap = true, OriginYIsOnScreen = false });
    AssertEqual(0xf0, oam.GetEntry(0).Position.Y,
        "extended enemy off-screen origin parks negative piece that remains above screen");
    AssertEqual(0x01, oam.GetEntry(1).Position.Y,
        "extended enemy off-screen origin admits positive piece crossing onto screen");

    Console.WriteLine(
        "  OAM: imported spritemap packing, enemy arithmetic, clipping, wrap, and finalization agree.");
}

/// <summary>Seeds a two-part bank-$80 spritemap with distinct OAM positions and attributes.</summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for the two-entry common spritemap at $80:8000")]
static void SeedTwoEntryCommonSpritemap(TestAddressSpace bus)
{
    bus.WriteBytes(0x808000, [
        0x02, 0x00,
        0x05, 0x80, 0xfe, 0xaa, 0x64,
        0xf0, 0x01, 0x20, 0x55, 0x21,
    ]);
}

/// <summary>Seeds a common spritemap whose piece crosses the vertical screen boundary.</summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for the vertically clipped common spritemap at $80:8100")]
static void SeedVerticallyClippedCommonSpritemap(TestAddressSpace bus)
{
    bus.WriteBytes(0x808100, [0x01, 0x00, 0x00, 0x00, 0x20, 0x01, 0x00]);
}

/// <summary>Seeds the bank-$8D projectile spritemap used to verify imported OAM packing.</summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for the two-entry enemy-projectile spritemap at $8D:9800")]
static void SeedTwoEntryEnemyProjectileSpritemap(TestAddressSpace bus)
{
    bus.WriteBytes(0x8d9800, [
        0x02, 0x00,
        0x05, 0x80, 0xfe, 0x0f, 0x20,
        0xf0, 0x01, 0x02, 0xfe, 0x21,
    ]);
}

/// <summary>Seeds the bank-$A2 enemy spritemap used to verify imported OAM packing.</summary>
[CpuBusBoundary("exact bus test: synthetic cartridge fixture for the two-entry room-enemy spritemap at $A2:9000")]
static void SeedTwoEntryRoomEnemySpritemap(TestAddressSpace bus)
{
    bus.WriteBytes(0xa29000, [
        0x02, 0x00,
        0x05, 0x80, 0xfe, 0x10, 0x20,
        0xf0, 0x01, 0x02, 0xfe, 0x21,
    ]);
}

    /// <summary>Runs the mutable address-space boundary check.</summary>
    [VerificationCommand("--mutable-memory-boundary")]
    internal static int MutableMemoryBoundaryCommand()
    {
     VerifySuperMetroidAddressSpace(); return 0; 
    }

    /// <summary>Runs the OAM spritemap source and packing check.</summary>
    [VerificationCommand("--oam-source-routing")]
    internal static int OamSourceRoutingCommand()
    {
        Suite(nameof(VerifyOamSpritemapPacking), () => VerifyOamSpritemapPacking());
        return 0;
    }

    /// <summary>Checks VRAM queue semantics, source routing, and queued artwork.</summary>
    [VerificationCommand("--dma-source-routing")]
    internal static int DmaSourceRoutingCommand()
    {
        Suite(nameof(VerifyVramWriteQueue), () => VerifyVramWriteQueue());
        Suite(nameof(VerifyDmaSourceRouting), () => VerifyDmaSourceRouting());
        Suite(nameof(VerifyQueuedVramAssets), () => VerifyQueuedVramAssets());
        return 0;
    }

    /// <summary>Checks BCD timer behavior, state transitions, floor timing, and input recording.</summary>
    [VerificationCommand("--escape-timer")]
    internal static int EscapeTimerCommand()
    {
        Suite(nameof(VerifyEscapeTimerBcd), () => VerifyEscapeTimerBcd());
        Suite(nameof(VerifyEscapeTimerStateMachine), () => VerifyEscapeTimerStateMachine());
        Suite(nameof(VerifyEscapeTimerFloor), () => VerifyEscapeTimerFloor());
        Suite(nameof(VerifyControllerInputRecording), () => VerifyControllerInputRecording());
        return 0;
    }

/// <summary>Runs the frame runtime's NMI, controller, Mode 7 and VRAM-drain checks.</summary>
[VerificationCommand("--frame-runtime")]
internal static int FrameRuntimeCommand()
{
    Suite(nameof(VerifyFrameRuntime), () => VerifyFrameRuntime());
    return 0;
}
}
