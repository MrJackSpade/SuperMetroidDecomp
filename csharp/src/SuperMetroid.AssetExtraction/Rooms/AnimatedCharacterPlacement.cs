using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The characters one animation frame paints over a background layer's (#1406). Extraction pictures the layer's tile
/// words over them under the frame's identity, as over any artwork bound there, and declares the run of characters the
/// animation paints, so the room shows the picture of the frame its animation selected without knowing where the frame
/// would have been copied.
/// </summary>
/// <param name="Frame">The frame.</param>
/// <param name="Set">The characters it paints.</param>
/// <param name="FirstCharacter">The first character it paints.</param>
/// <param name="Bytes">The frame's characters, whole characters of the set.</param>
internal sealed record AnimatedCharacterPlacement(ArtworkFrame Frame, BgCharacterSet Set, int FirstCharacter, ReadOnlyMemory<byte> Bytes)
{
    /// <summary>The frame's characters as artwork bound under the frame's identity, which the layer's pictures are composed over.</summary>
    internal BgCharacterUpload Placement => new(Frame.Artwork, Set, FirstCharacter, Bytes);

    /// <summary>The run of characters the frame's animation paints.</summary>
    /// <exception cref="InvalidDataException">The frame does not cover whole characters of the set.</exception>
    internal AnimatedCharacterRun Run => Bytes.Length > 0 && Bytes.Length % Set.CharacterBytes() == 0
        ? new AnimatedCharacterRun(Frame.Animation, FirstCharacter, Bytes.Length / Set.CharacterBytes())
        : throw new InvalidDataException($"{Frame} does not cover whole {Set} characters.");

    /// <summary>Frame <paramref name="frame"/>, whose characters the cartridge copies to <paramref name="firstWord"/>.</summary>
    internal static AnimatedCharacterPlacement At(ArtworkFrame frame, int firstWord, ReadOnlyMemory<byte> bytes)
    {
        BgCharacterUpload placement = BgCharacterWords.Place(frame.Artwork, firstWord, bytes);
        return new AnimatedCharacterPlacement(frame, placement.Set, placement.FirstCharacter, bytes);
    }
}
