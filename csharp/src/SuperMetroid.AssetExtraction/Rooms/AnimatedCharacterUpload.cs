using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The characters one animation frame paints over a background layer (#1406). Extraction pictures the layer's tile words
/// over them under the frame's identity, as over any artwork bound there, and declares the run of characters the
/// animation paints, so the room shows the picture of the frame its animation selected without knowing where the frame
/// would have been copied.
/// </summary>
/// <param name="Frame">The frame.</param>
/// <param name="FirstWord">The first VRAM word the frame's characters would occupy.</param>
/// <param name="Bytes">The frame's characters, low byte first, two per word.</param>
internal sealed record AnimatedCharacterUpload(ArtworkFrame Frame, int FirstWord, ReadOnlyMemory<byte> Bytes)
{
    /// <summary>The frame's characters as artwork bound under the frame's identity, which the layer's pictures are composed over.</summary>
    internal BgCharacterUpload Upload => new(Frame.Artwork, FirstWord, Bytes);

    /// <summary>
    /// The run of characters the frame's animation paints, numbered as tile words name them from the layer's character
    /// zero at VRAM word <paramref name="characterBase"/>, <paramref name="characterWords"/> words per character.
    /// </summary>
    /// <exception cref="InvalidDataException">The frame does not cover whole characters of the layer.</exception>
    [PpuLayoutBoundary("codec: the characters of a layer a frame's VRAM words would occupy, from the layer's character base")]
    internal AnimatedCharacterRun RunFrom(int characterBase, int characterWords)
    {
        int offset = FirstWord - characterBase, words = Bytes.Length / sizeof(ushort);
        if (offset < 0 || offset % characterWords != 0 || words == 0 || words % characterWords != 0)
            throw new InvalidDataException($"{Frame} does not cover whole characters of the layer whose characters begin at VRAM word ${characterBase:X4}.");
        return new AnimatedCharacterRun(Frame.Animation, offset / characterWords, words / characterWords);
    }
}
