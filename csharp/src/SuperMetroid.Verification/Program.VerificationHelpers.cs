using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// Compares ordered results while reporting the first bad element, rather than reducing
    /// a useful failure to "the sequences differ". Materializing once also makes this safe
    /// for one-shot iterators returned by render and projectile pipelines.
    /// </summary>
    static void AssertSequenceEqual<T>(
        IEnumerable<T> expected,
        IEnumerable<T> actual,
        string context)
    {
        ArgumentNullException.ThrowIfNull(expected);
        ArgumentNullException.ThrowIfNull(actual);

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
    static void AssertSamusPosition(
        ushort expectedX,
        ushort expectedY,
        SamusState actual,
        string context)
    {
        ArgumentNullException.ThrowIfNull(actual);
        AssertEqual(expectedX, actual.XPosition, $"{context} X");
        AssertEqual(expectedY, actual.YPosition, $"{context} Y");
    }

    /// <summary>
    /// Asserts every field of one hardware OAM projection. Exact record comparison is
    /// intentional: tile, palette, priority, flips, and size are all visually meaningful,
    /// and partial OBJ assertions have repeatedly allowed rendering regressions to hide.
    /// </summary>
    static void AssertOamEntry(OamEntry expected, OamEntry actual, string context) =>
        AssertEqual(expected, actual, context);

    /// <summary>
    /// Compares a contiguous CGRAM range while preserving the absolute failing color index.
    /// This replaces loops whose copy/pasted index arithmetic was harder to audit than the
    /// palette behavior they were meant to prove.
    /// </summary>
    static void AssertPaletteSlice(
        SnesCgram actual,
        int destinationIndex,
        ReadOnlySpan<ushort> expected,
        string context)
    {
        ArgumentNullException.ThrowIfNull(actual);
        if (destinationIndex < 0 || destinationIndex > SnesCgram.ColorCount - expected.Length)
            throw new ArgumentOutOfRangeException(nameof(destinationIndex));

        for (int color = 0; color < expected.Length; color++)
        {
            AssertEqual(
                expected[color],
                actual.Colors[destinationIndex + color],
                $"{context} color {destinationIndex + color}");
        }
    }

    /// <summary>
    /// Writes one native little-endian word into the sparse SNES-bus fixture. Tests should
    /// describe cartridge words as words; repeating two byte stores obscures both width and
    /// byte order.
    /// </summary>
    static void WriteTestWord(TestAddressSpace bus, int address, ushort value)
    {
        ArgumentNullException.ThrowIfNull(bus);
        bus.WriteByte(address, (byte)value);
        bus.WriteByte(address + 1, (byte)(value >> 8));
    }

    /// <summary>
    /// Writes consecutive native words without repeating address-stride arithmetic in every
    /// fixture. Each element still routes through the endian-aware primitive above.
    /// </summary>
    static void WriteTestWords(TestAddressSpace bus, int address, params ushort[] values)
    {
        ArgumentNullException.ThrowIfNull(values);
        for (int index = 0; index < values.Length; index++)
            WriteTestWord(bus, address + index * sizeof(ushort), values[index]);
    }

    /// <summary>
    /// Installs one complete eight-byte bank-$91 pose definition. The payload deliberately
    /// stays raw: direction, movement type, shot direction, graphics offset, radius, and
    /// still-unknown bytes must remain byte-for-byte fixture evidence. This helper owns only
    /// the verified table base, record stride, and record-width invariant.
    /// </summary>
    static void WritePoseDefinition(
        TestAddressSpace bus,
        int pose,
        ReadOnlySpan<byte> definition)
    {
        if ((uint)pose > byte.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(pose));
        if (definition.Length != 8)
        {
            throw new ArgumentException(
                "A native Samus pose definition is exactly eight bytes.",
                nameof(definition));
        }

        bus.WriteBytes(0x91b629 + pose * 8, definition);
    }

    /// <summary>
    /// Replaces one byte of a previously installed pose definition. This is used only when
    /// a test intentionally varies one discriminator while retaining the rest of its fixture.
    /// </summary>
    static void WritePoseDefinitionByte(TestAddressSpace bus, int pose, int byteOffset, byte value)
    {
        if ((uint)pose > byte.MaxValue)
            throw new ArgumentOutOfRangeException(nameof(pose));
        if ((uint)byteOffset >= 8)
            throw new ArgumentOutOfRangeException(nameof(byteOffset));

        bus.WriteByte(0x91b629 + pose * 8 + byteOffset, value);
    }

    /// <summary>
    /// Creates a synthetic room while supplying cleared planes that the individual test does
    /// not care about. Foreground and BTS remain mandatory because silently inventing either
    /// collision input would make physics fixtures ambiguous.
    /// </summary>
    static RoomLevelData CreateRoom(
        int widthInBlocks,
        int heightInBlocks,
        ReadOnlySpan<ushort> foreground,
        ReadOnlySpan<byte> behavior,
        ReadOnlySpan<ushort> background = default,
        ReadOnlySpan<byte> blockDefinitions = default)
    {
        int blockCount = checked(widthInBlocks * heightInBlocks);
        ushort[] clearedBackground = background.IsEmpty
            ? new ushort[blockCount]
            : background.ToArray();

        // One cleared definition is sufficient when a fixture never expands a nonzero
        // visual block. Rendering tests pass their cartridge-authored definition table.
        byte[] clearedDefinitions = blockDefinitions.IsEmpty
            ? new byte[8]
            : blockDefinitions.ToArray();

        return new RoomLevelData(
            widthInBlocks,
            heightInBlocks,
            foreground,
            behavior,
            clearedBackground,
            clearedDefinitions);
    }

    /// <summary>
    /// Creates a completely empty collision room. Dimensions stay explicit at the call site
    /// because room boundaries are frequently part of the behavior under test.
    /// </summary>
    static RoomLevelData CreateEmptyRoom(int widthInBlocks, int heightInBlocks)
    {
        int blockCount = checked(widthInBlocks * heightInBlocks);
        return CreateRoom(
            widthInBlocks,
            heightInBlocks,
            new ushort[blockCount],
            new byte[blockCount]);
    }

    /// <summary>
    /// Builds the common verifier starting point without hiding movement state. Only pose
    /// and whole-pixel position are assigned; all other fields retain production defaults
    /// unless the test changes them explicitly afterward.
    /// </summary>
    static SamusState CreateSamus(byte pose, ushort xPosition, ushort yPosition) => new()
    {
        Pose = pose,
        XPosition = xPosition,
        YPosition = yPosition,
    };

    /// <summary>
    /// Runs an exact number of verifier frames. Passing the index preserves native timelines
    /// that intentionally change input or inspect state on a particular tick.
    /// </summary>
    static void StepFrames(int frameCount, Action<int> step)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(frameCount);
        ArgumentNullException.ThrowIfNull(step);

        for (int frame = 0; frame < frameCount; frame++)
            step(frame);
    }

    /// <summary>
    /// Advances a bounded state machine until its observable completion predicate is true.
    /// A missed transition fails at this shared boundary instead of silently continuing
    /// after an arbitrary loop limit.
    /// </summary>
    static int StepUntil(
        Func<bool> isComplete,
        Action<int> step,
        int maximumFrames,
        string context)
    {
        ArgumentNullException.ThrowIfNull(isComplete);
        ArgumentNullException.ThrowIfNull(step);
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
