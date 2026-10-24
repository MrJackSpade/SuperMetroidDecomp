using System.Buffers.Binary;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemy BG2 image a boss's fight attaches before its body frames draw (#1406), for checks that draw those frames
/// alone and compare them with the cartridge's VRAM.
/// </summary>
internal static class EnemyBg2ImageFixtures
{
    /// <summary>Attaches a BG2 image whose blank tile is zero, as fresh native VRAM is.</summary>
    internal static EnemyBg2Image AttachBlank(EnemyPresentation presentation)
    {
        var image = new EnemyBg2Image(0);
        presentation.AttachBg2Image(image);
        return image;
    }

    /// <summary>A VRAM-sized image of <paramref name="image"/>'s words laid out from word <paramref name="firstWord"/>, where the cartridge's VRAM holds them.</summary>
    internal static byte[] AsVram(EnemyBg2Image image, int firstWord, int vramByteCount)
    {
        byte[] shown = new byte[vramByteCount];
        ushort[] words = [.. image.Publish().Words];
        for (int word = 0; word < words.Length; word++)
            BinaryPrimitives.WriteUInt16LittleEndian(shown.AsSpan((firstWord + word) * sizeof(ushort)), words[word]);
        return shown;
    }
}
