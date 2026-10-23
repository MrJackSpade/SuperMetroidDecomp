using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;

namespace SuperMetroid.Verification.Harness;

/// <summary>Shared assertion and sparse-cartridge-fixture helpers for byte-accurate movement, rendering and instruction checks.</summary>
internal static class VerificationHelpersVerification
{
    /// <summary>Compares two memory images, reporting the first differing byte offset and values.</summary>
    internal static void AssertSameBytes(ReadOnlySpan<byte> expected, ReadOnlySpan<byte> actual, string context)
    {
        AssertEqual(expected.Length, actual.Length, $"{context} length");
        int first = expected.CommonPrefixLength(actual);
        if (first != expected.Length)
            throw new InvalidOperationException(
                $"Verification failed: {context}; first difference at offset ${first:X5}: expected ${expected[first]:X2}, got ${actual[first]:X2}.");
    }

    /// <summary>
    /// Compares ordered results while reporting the first bad element, rather than reducing
    /// a useful failure to "the sequences differ". Materializing once also makes this safe
    /// for one-shot iterators returned by render and projectile pipelines.
    /// </summary>
    internal static void AssertSequenceEqual<T>(
        IEnumerable<T> expected,
        IEnumerable<T> actual,
        string context)
    {
        Ensure.NotNull(expected);
        Ensure.NotNull(actual);

        T[] expectedItems = expected.ToArray();
        T[] actualItems = actual.ToArray();
        AssertEqual(expectedItems.Length, actualItems.Length, $"{context} count");

        for (int index = 0; index < expectedItems.Length; index++)
        {
            AssertEqual(
                expectedItems[index],
                actualItems[index],
                $"{context} element {index}");
        }
    }

    /// <summary>
    /// Asserts both whole-pixel Samus coordinates and identifies the bad axis in failures.
    /// Position pairs are one domain value throughout movement tests; keeping this check
    /// together prevents a new fixture from accidentally validating only one axis.
    /// </summary>
    internal static void AssertSamusPosition(
        Position16<RoomPixels> expected,
        SamusState actual,
        string context)
    {
        Ensure.NotNull(actual);
        AssertEqual(expected, actual.Position.Whole, context);
    }

    /// <summary>
    /// Asserts every field of one hardware OAM projection. Exact record comparison is
    /// intentional: tile, palette, priority, flips, and size are all visually meaningful,
    /// and partial OBJ assertions have repeatedly allowed rendering regressions to hide.
    /// </summary>
    internal static void AssertOamEntry(OamEntry expected, OamEntry actual, string context) =>
        AssertEqual(expected, actual, context);

    /// <summary>
    /// Compares a contiguous CGRAM range while preserving the absolute failing color index.
    /// This replaces loops whose copy/pasted index arithmetic was harder to audit than the
    /// palette behavior they were meant to prove.
    /// </summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal static void AssertPaletteSlice(
        IPaletteColors actual,
        int destinationIndex,
        ReadOnlySpan<ushort> expected,
        string context)
    {
        Ensure.NotNull(actual);
        Ensure.BetweenInclusive(0, SnesCgram.ColorCount - expected.Length, destinationIndex);

        for (int color = 0; color < expected.Length; color++)
        {
            AssertEqual(
                expected[color],
                actual.Colors[destinationIndex + color],
                $"{context} color {destinationIndex + color}");
        }
    }

    /// <summary>
    /// Seeds one byte of the sparse SNES-bus fixture, which stores exactly the CPU address
    /// written and synthesizes no mirrors. Tests use it to construct synthetic cartridge
    /// records and programs field by field.
    /// </summary>
    [CpuBusBoundary("exact bus test: seeds one byte of the sparse synthetic CPU address space fixture")]
    [CpuBusAccess]
    internal static void WriteTestByte(TestAddressSpace bus, int address, byte value)
    {
        Ensure.NotNull(bus);
        bus.WriteByte(address, value);
    }

    /// <summary>
    /// Writes one native little-endian word into the sparse SNES-bus fixture. Tests should
    /// describe cartridge words as words; repeating two byte stores obscures both width and
    /// byte order.
    /// </summary>
    [CpuBusBoundary("exact bus test: seeds one little-endian word of the sparse synthetic CPU address space fixture")]
    [CpuBusAccess]
    internal static void WriteTestWord(TestAddressSpace bus, int address, ushort value)
    {
        WriteTestByte(bus, address, (byte)value);
        WriteTestByte(bus, address + 1, SnesLittleEndian.HighByte(value));
    }

    /// <summary>
    /// Writes consecutive native words without repeating address-stride arithmetic in every
    /// fixture. Each element still routes through the endian-aware primitive above.
    /// </summary>
    [CpuBusBoundary("exact bus test: seeds consecutive little-endian words of the sparse synthetic CPU address space fixture")]
    [CpuBusAccess]
    internal static void WriteTestWords(TestAddressSpace bus, int address, params ushort[] values)
    {
        Ensure.NotNull(values);
        for (int index = 0; index < values.Length; index++)
            WriteTestWord(bus, address + index * sizeof(ushort), values[index]);
    }

    /// <summary>
    /// Installs one complete eight-byte bank-$91 pose definition. The payload deliberately
    /// stays raw: direction, movement type, shot direction, graphics offset, radius, and
    /// still-unknown bytes must remain byte-for-byte fixture evidence. This helper owns only
    /// the verified table base, record stride, and record-width invariant.
    /// </summary>
    [CpuBusBoundary("exact bus test: synthetic cartridge fixture for one bank-$91 Samus pose definition record")]
    internal static void WritePoseDefinition(
        TestAddressSpace bus,
        int pose,
        ReadOnlySpan<byte> definition)
    {
        if ((uint)pose > byte.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(pose));
        Ensure.LengthEqual(definition, 8);

        bus.WriteBytes(
            SamusMovementRomData.Poses.Definitions + pose * SamusMovementRomData.Poses.DefinitionByteCount,
            definition);
    }

    /// <summary>
    /// Replaces one byte of a previously installed pose definition. This is used only when
    /// a test intentionally varies one discriminator while retaining the rest of its fixture.
    /// </summary>
    [CpuBusBoundary("exact bus test: synthetic cartridge fixture for one byte of a bank-$91 Samus pose definition record")]
    internal static void WritePoseDefinitionByte(TestAddressSpace bus, int pose, int byteOffset, byte value)
    {
        if ((uint)pose > byte.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(pose));
        if ((uint)byteOffset >= 8)
            throw new ArgumentOutOfRangeException(nameof(byteOffset));

        WriteTestByte(
            bus,
            SamusMovementRomData.Poses.Definitions + pose * SamusMovementRomData.Poses.DefinitionByteCount + byteOffset,
            value);
    }

    /// <summary>
    /// Points one pose's entry in the bank-$91 animation-delay pointer table
    /// (<c>$91:B010</c>, one same-bank word per pose) at a synthetic delay stream.
    /// </summary>
    [CpuBusBoundary("exact bus test: synthetic cartridge fixture for one bank-$91 Samus animation-delay pointer")]
    internal static void WritePoseAnimationDelayPointer(TestAddressSpace bus, SamusPoseId pose, ushort stream) =>
        WriteTestWord(bus, SamusAnimationDelayDefinitions.PointerTableAddress + (int)pose * sizeof(ushort), stream);

    /// <summary>Installs one synthetic animation-delay stream at a bank-$91 offset.</summary>
    [CpuBusBoundary("exact bus test: synthetic cartridge fixture for one bank-$91 Samus animation-delay stream")]
    internal static void WriteAnimationDelayStream(TestAddressSpace bus, ushort stream, ReadOnlySpan<byte> delays)
    {
        Ensure.NotNull(bus);
        bus.WriteBytes(SamusMovementRomData.Banks.Pose | stream, delays);
    }

    /// <summary>
    /// Installs a pose's animation-delay pointer and the stream it selects together, the common
    /// fixture shape for a pose whose delay bytes no other pose shares.
    /// </summary>
    internal static void WritePoseAnimationDelays(
        TestAddressSpace bus,
        SamusPoseId pose,
        ushort stream,
        ReadOnlySpan<byte> delays)
    {
        WritePoseAnimationDelayPointer(bus, pose, stream);
        WriteAnimationDelayStream(bus, stream, delays);
    }

    /// <summary>
    /// Creates a synthetic room while supplying cleared planes that the individual test does
    /// not care about. Foreground and BTS remain mandatory because silently inventing either
    /// collision input would make physics fixtures ambiguous.
    /// </summary>
    internal static RoomLevelData CreateRoom(
        Extent16<RoomBlocks> size,
        ReadOnlySpan<ushort> foreground,
        ReadOnlySpan<byte> behavior,
        ReadOnlySpan<ushort> background = default,
        ReadOnlySpan<byte> blockDefinitions = default)
    {
        int blockCount = checked(size.X * size.Y);
        ushort[] clearedBackground = background.IsEmpty
            ? new ushort[blockCount]
            : background.ToArray();

        // One cleared definition is sufficient when a fixture never expands a nonzero
        // visual block. Rendering tests pass their cartridge-authored definition table.
        byte[] clearedDefinitions = blockDefinitions.IsEmpty
            ? new byte[8]
            : blockDefinitions.ToArray();

        return new RoomLevelData(size, new RoomLevelPlanes(foreground, behavior, clearedBackground), clearedDefinitions);
    }

    /// <summary>
    /// Creates a completely empty collision room. Dimensions stay explicit at the call site
    /// because room boundaries are frequently part of the behavior under test.
    /// </summary>
    internal static RoomLevelData CreateEmptyRoom(Extent16<RoomBlocks> size)
    {
        int blockCount = checked(size.X * size.Y);
        return CreateRoom(
            size,
            new ushort[blockCount],
            new byte[blockCount]);
    }

    /// <summary>
    /// Builds the common verifier starting point without hiding movement state. Only pose
    /// and whole-pixel position are assigned; all other fields retain production defaults
    /// unless the test changes them explicitly afterward.
    /// </summary>
    internal static SamusState CreateSamus(SamusPoseId pose, Position16<RoomPixels> position) => new()
    {
        Pose = pose,
        Position = FixedPosition<RoomPixels>.FromWhole(position),
    };

    /// <summary>
    /// Runs an exact number of verifier frames. Passing the index preserves native timelines
    /// that intentionally change input or inspect state on a particular tick.
    /// </summary>
    internal static void StepFrames(int frameCount, Action<int> step)
    {
        Ensure.AtLeastZero(frameCount);
        Ensure.NotNull(step);

        for (int frame = 0; frame < frameCount; frame++)
            step(frame);
    }

    /// <summary>
    /// Advances a bounded state machine until its observable completion predicate is true.
    /// A missed transition fails at this shared boundary instead of silently continuing
    /// after an arbitrary loop limit.
    /// </summary>
    internal static int StepUntil(
        Func<bool> isComplete,
        Action<int> step,
        int maximumFrames,
        string context)
    {
        Ensure.NotNull(isComplete);
        Ensure.NotNull(step);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maximumFrames);

        int frames = 0;
        while (!isComplete() && frames < maximumFrames)
        {
            step(frames);
            frames++;
        }

        AssertTrue(isComplete(), $"{context} completes within {maximumFrames} frames");
        return frames;
    }
}
