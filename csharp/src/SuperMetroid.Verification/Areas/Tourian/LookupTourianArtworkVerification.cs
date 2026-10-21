using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Verification.Areas.Tourian;

/// <summary>Checks Tourian statue animation artwork operands against decoded native instructions.</summary>
internal static class LookupTourianArtworkVerification
{
    /// <summary>Enumerates native timed statue frames and their bank-$87 artwork source addresses.</summary>
    internal static IEnumerable<(ushort Operand, int Source)> OriginalTourianFrames(
        ISnesAddressSpace rom, ushort header)
    {
        foreach (var instruction in OriginalTourianInstructions(rom, header))
            if (instruction.Code < 0x8000)
                yield return ((ushort)(instruction.Cursor + 2),
                    0x870000 | ReadVerificationWord(rom, 0x870000 | (instruction.Cursor + 2)));
    }

    /// <summary>Decodes native statue instruction widths through both delete commands, rejecting unknown opcodes.</summary>
    internal static IEnumerable<(ushort Cursor, ushort Code, int Width)> OriginalTourianInstructions(
        ISnesAddressSpace rom, ushort header)
    {
        int cursor = ReadVerificationWord(rom, 0x870000 | header);
        int deletes = 0;
        for (int count = 0; deletes < 2; count++)
        {
            AssertTrue(count < 64, "Original statue program has bounded instruction count");
            ushort code = ReadVerificationWord(rom, 0x870000 | cursor);
            if (code < 0x8000)
            {
                yield return ((ushort)cursor, code, 4);
                cursor += 4;
                continue;
            }
            if (code == 0x80b2) deletes++;
            int width = code switch
            {
                0x80b2 => 2,
                0x813f or 0x8303 => 6,
                0x80b7 or 0x8150 or 0x8320 or 0x832f or 0x833e or
                    0x8349 or 0x8352 or 0x835b or 0x8372 or 0x837f => 4,
                _ => throw new InvalidDataException($"Unexpected native statue instruction {code:X4}."),
            };
            yield return ((ushort)cursor, code, width);
            cursor += width;
        }
    }

    /// <summary>
    /// Checks that each statue's pictures are exactly the distinct artwork windows its nine native timed frames
    /// select; the program oracle checks which picture each frame shows.
    /// </summary>
    internal static void VerifyTourianStatueArtworkSources(ISnesAddressSpace rom)
    {
        int frames = 0;
        int pictures = 0;
        foreach (ushort header in LookupTourianDescriptorsVerification.OriginalTourianObjects)
        {
            AssertTrue(TourianStatueAnimatedTileMechanicsDefinitions.TryResolveObjectHeader((AnimatedTileObject)header,
                out var definition), "Original statue object resolves");
            TourianStatueAnimatedTileProgramDefinition resolved = definition ??
                throw new InvalidOperationException("Verification requires definition.");
            int[] native = [.. OriginalTourianFrames(rom, header).Select(frame => frame.Source)];
            AssertEqual(9, native.Length, "Nine original timed statue frames");
            int[] produced = [.. TourianStatueTileStripDefinitions.Pictures.Select(picture =>
                TourianStatueTileStripDefinitions.SourceAddress(resolved.ObjectPointer, picture))];
            AssertEqual(produced.Length, produced.Distinct().Count(), "Each statue picture is its own artwork window");
            AssertTrue(native.ToHashSet().SetEquals(produced),
                "Every native frame source is one statue picture and every picture is shown");
            frames += native.Length;
            pictures += produced.Length;
        }
        AssertEqual(36, frames, "Complete native statue frame domain");
        AssertEqual(16, pictures, "Complete statue picture domain");
    }
}
