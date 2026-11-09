using SuperMetroid.TestSupport.Assertions;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The ROM oracle of the typed terrain changes (#1554). It plays a native bank-$84 terrain-change list the way the
/// original interpreter ran it, counting its loop and following its gotos and its fall-through into the next list, and
/// compares every frame the list draws (its duration, its draw list, and the origin move and sound that come before it)
/// and the delete that ends it with the typed change's frames. Only verification reads cartridge lists; the port runs
/// <see cref="TerrainChangeObject"/>.
/// </summary>
internal static class NativeTerrainChanges
{
    /// <summary>The PLM program bank, as a long-address base.</summary>
    private const int ProgramBank = 0x840000;

    /// <summary>Bit 15 marks an instruction word; a clear bit makes the word a draw's duration.</summary>
    private const ushort InstructionBit = 0x8000;

    /// <summary>The bytes of a copy-from-RAM-to-VRAM instruction: its word, size, long source and destination.</summary>
    private const int CopyFromRamToVramLength = 9;

    /// <summary>The most instructions one list may run before it must end.</summary>
    private const int InstructionLimit = 4096;

    /// <summary>
    /// What one family instruction does before the next frame draws: a move of the origin, a sound, or the opening a
    /// change makes as its first frame starts.
    /// </summary>
    /// <param name="Move">The blocks the origin moves.</param>
    /// <param name="Sound">The sound requested, or null.</param>
    /// <param name="Begins">Whether this is the change's opening, which must come before its first frame.</param>
    /// <param name="Length">The instruction's native length in bytes.</param>
    /// <param name="Priority">The sound's request priority.</param>
    /// <param name="Effect">A family effect the step makes beyond a move or sound, named for the comparison; null for none.</param>
    internal readonly record struct NativeStep(Offset16<RoomBlocks> Move, SoundEffectId? Sound, bool Begins, int Length,
        SoundRequestPriority Priority = SoundRequestPriority.Normal, string? Effect = null);

    /// <summary>Decodes the family instruction at <paramref name="cursor"/>, or returns false when the family has none there.</summary>
    internal delegate bool StepReader(IImportCartridgeSource cartridge, ushort cursor, out NativeStep step);

    /// <summary>The reader of families whose lists hold only draws, loops and their delete.</summary>
    internal static StepReader NoSteps { get; } = (IImportCartridgeSource _, ushort _, out NativeStep step) =>
    {
        step = default;
        return false;
    };

    /// <summary>
    /// Plays the native list at <paramref name="entry"/> beside <paramref name="change"/> and returns the frames compared;
    /// <paramref name="began"/> says whether the list made its opening before its first frame.
    /// </summary>
    internal static int Compare(IImportCartridgeSource cartridge, TerrainChange change, ushort entry, StepReader steps, string family,
        out bool began)
    {
        int frames = Compare(cartridge, change, entry, steps, family, out began, out IReadOnlyList<NativeStep> atEnd);
        VerificationAssertions.AssertEqual(0, atEnd.Count, $"{family} list $84:{entry:X4} does nothing as it ends");
        return frames;
    }

    /// <summary>
    /// <see cref="Compare(IImportCartridgeSource, TerrainChange, ushort, StepReader, string, out bool)"/> for a change
    /// that acts as it ends: <paramref name="atEnd"/> is every step after its last frame other than a move, which an
    /// object that is then gone makes unseen.
    /// </summary>
    internal static int Compare(IImportCartridgeSource cartridge, TerrainChange change, ushort entry, StepReader steps, string family,
        out bool began, out IReadOnlyList<NativeStep> atEnd)
    {
        var sinceFrame = new List<NativeStep>();
        ArgumentNullException.ThrowIfNull(cartridge);
        ArgumentNullException.ThrowIfNull(change);
        ArgumentNullException.ThrowIfNull(steps);
        ushort cursor = entry;
        int loop = 0;
        int frame = 0;
        Offset16<RoomBlocks> move = Offset16<RoomBlocks>.Zero;
        SoundEffectId? sound = null;
        began = false;
        for (int command = 0; command < InstructionLimit; command++)
        {
            string at = $"{family} list $84:{entry:X4} frame {frame} at $84:{cursor:X4}";
            ushort word = Word(cartridge, cursor);
            if ((word & InstructionBit) == 0)
            {
                VerificationAssertions.AssertTrue(frame < change.Frames.Count, $"{at}: the typed change ends before the native list draws");
                TerrainChangeFrame expected = change.Frames[frame];
                VerificationAssertions.AssertEqual((int)word, expected.Updates, $"{at}: updates the frame stays");
                VerificationAssertions.AssertEqual((int)Word(cartridge, cursor + 2), (int)RoomObjectDrawAddressCodec.Of(expected.Draw), $"{at}: draw");
                VerificationAssertions.AssertEqual(move, expected.MoveBefore, $"{at}: origin move before the frame");
                VerificationAssertions.AssertEqual(sound, expected.SoundBefore, $"{at}: sound with the frame");
                foreach (NativeStep before in sinceFrame)
                {
                    VerificationAssertions.AssertTrue(before.Effect is null, $"{at}: no family effect comes before a frame");
                    if (before.Sound is not null)
                        VerificationAssertions.AssertEqual(before.Priority, expected.SoundPriority, $"{at}: sound priority");
                }
                sinceFrame.Clear();
                move = Offset16<RoomBlocks>.Zero;
                sound = null;
                frame++;
                cursor = unchecked((ushort)(cursor + 4));
                continue;
            }
            var instruction = (RoomObjectInstruction)word;
            if (instruction == RoomObjectInstruction.Delete)
            {
                // An origin move made just before the delete moves an object that is then gone: nothing sees it.
                VerificationAssertions.AssertEqual(change.Frames.Count, frame, $"{at}: the native list ends after the typed change's frames");
                atEnd = [.. sinceFrame.Where(step => step.Sound is not null || step.Effect is not null)];
                return frame;
            }
            if (instruction == RoomObjectInstruction.Goto)
            {
                cursor = Word(cartridge, cursor + 2);
                continue;
            }
            if (instruction == RoomObjectInstruction.SetEightBitTimer)
            {
                loop = Word(cartridge, cursor + 2) & 0xff;
                cursor = unchecked((ushort)(cursor + 3));
                continue;
            }
            if (instruction == RoomObjectInstruction.DecrementTimerAndGoto)
            {
                loop--;
                cursor = loop != 0 ? Word(cartridge, cursor + 2) : unchecked((ushort)(cursor + 4));
                continue;
            }
            if (instruction == RoomObjectInstruction.CopyFromRamToVram)
            {
                // A character load the typed changes picture instead (#1406): no frame, no time.
                cursor = unchecked((ushort)(cursor + CopyFromRamToVramLength));
                continue;
            }
            if (!steps(cartridge, cursor, out NativeStep step))
                throw new InvalidOperationException($"Verification failed: {at}: native instruction ${word:X4} is a known terrain-change step.");
            if (step.Begins)
            {
                VerificationAssertions.AssertEqual(0, frame, $"{at}: the opening comes before the first frame");
                began = true;
            }
            sinceFrame.Add(step);
            move = new Offset16<RoomBlocks>(move.X + step.Move.X, move.Y + step.Move.Y);
            if (step.Sound is { } requested)
            {
                VerificationAssertions.AssertEqual<SoundEffectId?>(null, sound, $"{at}: one sound per frame");
                sound = requested;
            }
            cursor = unchecked((ushort)(cursor + step.Length));
        }
        throw new InvalidDataException($"{family} list $84:{entry:X4} ran past {InstructionLimit} instructions without its delete.");
    }

    /// <summary>The native word at a bank-$84 offset.</summary>
    private static ushort Word(IImportCartridgeSource cartridge, int address) => cartridge.ReadCartridgeWord(ProgramBank | address);
}
