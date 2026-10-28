using SuperMetroid.Core.Hardware;

using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Core.Fx;

/// <summary>BG3 planes for render tests that set BG3 up in VRAM (#1348).</summary>
internal static class Layer3PlaneFixtures
{
    /// <summary>The plane a test wrote into the gameplay BG3 tilemap's VRAM words, <c>$5800</c> onward.</summary>
    [PpuLayoutBoundary("test fixture: reads the BG3 tilemap a render test wrote into VRAM")]
    internal static Layer3Plane FromVram(SnesVram vram)
    {
        var words = new ushort[Layer3Plane.Rows * Layer3Plane.Columns];
        for (int word = 0; word < words.Length; word++)
            words[word] = vram.ReadWord(GameplayVramLayoutConstants.HudTilemapWord + word);
        return new Layer3Plane(words);
    }

    /// <summary>
    /// Writes the BG3 plane a room effect shows, under a blank HUD, into a test's VRAM BG3 tilemap, as the room load once
    /// did, for oracles that read BG3 from VRAM.
    /// </summary>
    [PpuLayoutBoundary("test fixture: writes the room effect's BG3 plane where an oracle reads it")]
    internal static void WriteEffectPlane(SnesVram vram, RoomLayer3FxState fx)
    {
        Layer3Plane plane = Layer3Plane.Compose(new ushort[Layer3Plane.HudRows * Layer3Plane.Columns], fx.EffectPageWords());
        vram.ExecuteWordTransfer(plane.Words[(Layer3Plane.HudRows * Layer3Plane.Columns)..].ToArray(),
            checked(GameplayVramLayoutConstants.HudTilemapWord + Layer3Plane.HudRows * Layer3Plane.Columns), 1);
    }

    /// <summary>The plane in a captured VRAM image's BG3 tilemap words.</summary>
    [PpuLayoutBoundary("test fixture: reads the BG3 tilemap a render test wrote into VRAM")]
    internal static Layer3Plane FromVram(ReadOnlySpan<byte> vram)
    {
        var words = new ushort[Layer3Plane.Rows * Layer3Plane.Columns];
        for (int word = 0; word < words.Length; word++)
        {
            int at = (GameplayVramLayoutConstants.HudTilemapWord + word) * 2;
            words[word] = SnesLittleEndian.Word(vram[at], vram[at + 1]);
        }
        return new Layer3Plane(words);
    }
}
