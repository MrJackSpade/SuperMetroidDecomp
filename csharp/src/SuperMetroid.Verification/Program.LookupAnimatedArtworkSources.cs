using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifySimpleAnimationArtworkSources(SuperMetroidAddressSpace rom)
    {
        int total = 0;
        foreach (var original in OriginalSimpleAnimationLoops(rom))
        {
            AssertTrue(RoomFxAnimatedTileMechanicsDefinitions.TryResolve(original.Object, out var definition),
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
                    AssertEqual(source, RoomFxAnimatedTileArtworkDefinitions.SourceAddress(definition, cursor),
                        "Original artwork source operand");
                else
                    AssertThrows<InvalidDataException>(
                        () => RoomFxAnimatedTileArtworkDefinitions.SourceAddress(definition, cursor),
                        "Non-frame cursor rejects including odd/source/loop words");
            }
        }
        AssertEqual(26, total, "Complete original simple artwork source domain");
        AssertThrows<ArgumentNullException>(() => RoomFxAnimatedTileArtworkDefinitions.SourceAddress(null!, 0),
            "Null descriptor rejects");
        var unknown = new RoomFxAnimatedTileObjectDefinition(0, 0x8000, 32, 0, 1, 10);
        AssertThrows<InvalidDataException>(() => RoomFxAnimatedTileArtworkDefinitions.SourceAddress(unknown, 0x8000),
            "Unknown object cannot acquire an artwork mapping");
    }
}