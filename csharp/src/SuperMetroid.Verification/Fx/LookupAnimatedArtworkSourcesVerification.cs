using SuperMetroid.AssetExtraction.Cartridge;
using static SuperMetroid.Verification.Rooms.Doors.DoorDefinitionsVerification;
using static SuperMetroid.Verification.Fx.LookupAnimatedFramesVerification;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Verification.Fx;

/// <summary>Checks every authored simple animated-tile frame operand against
/// the cartridge and rejects non-frame cursors across the ushort domain.</summary>
internal static class LookupAnimatedArtworkSourcesVerification
{
    /// <summary>Compares compiled artwork source addresses with original bank-$87
    /// operands and verifies unknown objects cannot acquire a visual mapping.</summary>
    internal static void VerifySimpleAnimationArtworkSources(SuperMetroidAddressSpace rom)
    {
        int total = 0;
        foreach (var original in OriginalSimpleAnimationLoops(rom))
        {
            AssertTrue(RoomFxAnimatedTiles.TryResolve(FxNativeWords.AnimatedTileObjects.Decode(original.Object, "animated-tile object"), out var definition),
                "Original animation resolves for artwork");
            var expected = new Dictionary<ushort, int>();
            for (int cursor = original.First; cursor < original.End; cursor += 4)
            {
                expected.Add((ushort)cursor, 0x870000 | ReadVerificationWord(rom, 0x870000 | (cursor + 2)));
                total++;
            }
            for (int value = 0; value <= ushort.MaxValue; value++)
            {
                ushort cursor = (ushort)value;
                if (expected.TryGetValue(cursor, out int source))
                    AssertEqual(source, AnimatedTileImageSources.OfCursor((definition ?? throw new InvalidOperationException("Verification requires definition.")), cursor),
                        "Original artwork source operand");
                else
                    AssertThrows<InvalidDataException>(
                        () => AnimatedTileImageSources.OfCursor((definition ?? throw new InvalidOperationException("Verification requires definition.")), cursor),
                        "Non-frame cursor rejects including odd/source/loop words");
            }
        }
        AssertEqual(30, total, "Complete original simple artwork source domain");
        var unknown = new RoomFxAnimatedTileLoop(0, 32, 1, 10, 1);
        AssertThrows<InvalidDataException>(() => AnimatedTileImageSources.Of(unknown.Image(unknown.Frames[0])),
            "Unknown object cannot acquire an artwork mapping");
    }
}
