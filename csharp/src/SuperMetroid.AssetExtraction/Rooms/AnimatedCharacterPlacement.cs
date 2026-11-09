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
/// <param name="Placement">
/// The frame's characters as artwork placed under the frame's identity, which pictures are composed over; held once, since
/// a body's composition reads it for every character it places (#1573).
/// </param>
internal sealed record AnimatedCharacterPlacement(ArtworkFrame Frame, BgCharacterUpload Placement)
{
    /// <summary>The characters the frame paints.</summary>
    internal BgCharacterSet Set => Placement.Set;

    /// <summary>The frame as the value of its animation's appearance input, showing its characters.</summary>
    internal AppearanceOption Option => new(new AnimationAppearance(Frame.Animation), AnimationAppearance.ValueOf(Frame), Placement);

    /// <summary>Frame <paramref name="frame"/>'s characters <paramref name="bytes"/>, painted from <paramref name="firstCharacter"/> of <paramref name="set"/>.</summary>
    internal static AnimatedCharacterPlacement Of(ArtworkFrame frame, BgCharacterSet set, int firstCharacter, ReadOnlyMemory<byte> bytes) =>
        new(frame, new BgCharacterUpload(BgArtworkIdentity.Frame(frame), set, firstCharacter, bytes));

    /// <summary>Frame <paramref name="frame"/>, whose characters the cartridge copies to <paramref name="firstWord"/>.</summary>
    internal static AnimatedCharacterPlacement At(ArtworkFrame frame, int firstWord, ReadOnlyMemory<byte> bytes)
    {
        BgCharacterUpload placement = BgCharacterWords.Place(BgArtworkIdentity.Frame(frame), firstWord, bytes);
        return Of(frame, placement.Set, placement.FirstCharacter, bytes);
    }
}
