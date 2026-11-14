using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Fx;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Fx.LookupAnimatedFramesVerification;
using SuperMetroid.Core.Fx;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Fx;

/// <summary>Checks every authored simple animated-tile frame operand against
/// the cartridge and rejects non-frame cursors across the ushort domain.</summary>
internal static class LookupAnimatedArtworkSourcesVerification
{
    /// <summary>Compares compiled artwork source addresses with original bank-$87
    /// operands and verifies unknown objects cannot acquire a visual mapping.</summary>
    internal static void VerifySimpleAnimationArtworkSources(CartridgeImportAddressSpace rom)
    {
        int total = 0;
        foreach (var original in OriginalSimpleAnimationLoops(rom))
        {
            AssertTrue(RoomFxBlockAnimations.TryResolve(FxNativeWords.AnimatedTileObjects.Decode(original.Object, "animated-tile object"), out var definition),
                "Original animation resolves for artwork");
            var expected = new Dictionary<ushort, int>();
            for (int cursor = original.First; cursor < original.End; cursor += 4)
            {
                expected.Add((ushort)cursor, 0x870000 | ReadVerificationWord(rom, 0x870000 | (cursor + 2)));
                total++;
            }
            RoomFxBlockAnimationLoop loop = definition ?? throw new InvalidOperationException("Verification requires definition.");
            // The artwork source of a cursor is a pure function of the immutable loop, so the whole cursor domain is
            // swept across the cores. Each cursor is classified through the lookup's own frame test; the lookup rejects
            // exactly the cursors that test refuses, which the odd cursor after the first frame confirms once per loop
            // instead of raising an exception for each of the domain's non-frame cursors.
            ParallelSweep.For(0, ushort.MaxValue + 1, value =>
            {
                ushort cursor = (ushort)value;
                bool resolved = AnimatedTileImageSources.TryOfCursor(loop, cursor, out int resolvedSource);
                if (expected.TryGetValue(cursor, out int source))
                {
                    AssertTrue(resolved, "Original artwork frame cursor resolves");
                    AssertEqual(source, resolvedSource, "Original artwork source operand");
                }
                else
                {
                    AssertTrue(!resolved, "Non-frame cursor rejects including odd/source/loop words");
                }
            });
            AssertThrows<InvalidDataException>(
                () => AnimatedTileImageSources.OfCursor(loop, (ushort)(original.First + 1)),
                "Non-frame cursor rejects including odd/source/loop words");
        }
        AssertEqual(30, total, "Complete original simple artwork source domain");
        // The import's frame-to-picture fold rejects an object the atlas holds no pictures of.
        var unknown = new RoomFxBlockAnimationLoop(0, 1, 10);
        AssertThrows<InvalidDataException>(() => AnimatedTileImageSources.Of(unknown.Image(0)),
            "Unknown object cannot acquire an artwork mapping");
    }
}
