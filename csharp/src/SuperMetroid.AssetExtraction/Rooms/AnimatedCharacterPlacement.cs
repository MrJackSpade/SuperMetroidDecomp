using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The characters one animation frame paints over a background layer's (#1406, #1466). Extraction pictures what reads
/// those characters for the frame as the value of its animation's appearance input, so the room shows the picture of the
/// frame its animation selected without knowing where the frame would have been copied.
/// </summary>
/// <param name="Frame">The frame.</param>
/// <param name="Set">The characters it paints.</param>
/// <param name="FirstCharacter">The first character it paints.</param>
/// <param name="Bytes">The frame's characters, whole characters of the set.</param>
internal sealed record AnimatedCharacterPlacement(ArtworkFrame Frame, BgCharacterSet Set, int FirstCharacter, ReadOnlyMemory<byte> Bytes)
{
    /// <summary>The frame's characters as artwork placed under the frame's identity, which pictures are composed over.</summary>
    internal BgCharacterUpload Placement => new(BgArtworkIdentity.Frame(Frame), Set, FirstCharacter, Bytes);

    /// <summary>The frame as the value of its animation's appearance input, showing its characters.</summary>
    internal AppearanceOption Option => new(new AnimationAppearance(Frame.Animation), AnimationAppearance.ValueOf(Frame), Placement);

    /// <summary>Frame <paramref name="frame"/>, whose characters the cartridge copies to <paramref name="firstWord"/>.</summary>
    internal static AnimatedCharacterPlacement At(ArtworkFrame frame, int firstWord, ReadOnlyMemory<byte> bytes)
    {
        BgCharacterUpload placement = BgCharacterWords.Place(BgArtworkIdentity.Frame(frame), firstWord, bytes);
        return new AnimatedCharacterPlacement(frame, placement.Set, placement.FirstCharacter, bytes);
    }
}
