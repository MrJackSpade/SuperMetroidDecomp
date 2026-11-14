using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Verification.Rooms.Artwork;

/// <summary>
/// The characters of one set that a room's animations paint (#1406), with the animations of its tileset's appearance options
/// that paint each one in the order the checks consult them: by token, as the options list them where tokens tie. Built
/// once per room, so a check reading each character's bytes looks its animations up instead of rescanning every option.
/// </summary>
internal sealed class RoomAnimatedCharacters
{
    /// <summary>The characters the animations paint over.</summary>
    private readonly BgCharacterSet set;

    /// <summary>The installed characters of the animations' frames.</summary>
    private readonly RoomFxBlockAnimationAtlas animatedTiles;

    /// <summary>Each character an animation paints, with the animations that paint it and their uploads, in consulting order.</summary>
    private readonly Dictionary<int, List<AppearanceOption>> painting = [];

    /// <summary>
    /// The animations among <paramref name="options"/> that paint <paramref name="set"/>'s characters, whose frames
    /// <paramref name="animatedTiles"/> holds.
    /// </summary>
    internal RoomAnimatedCharacters(IReadOnlyList<AppearanceOption> options, BgCharacterSet set, RoomFxBlockAnimationAtlas animatedTiles)
    {
        this.set = set;
        this.animatedTiles = animatedTiles;
        foreach (AppearanceOption option in options.Where(option => option.Input is AnimationAppearance)
            .OrderBy(option => option.Input.Token, StringComparer.Ordinal))
        {
            BgCharacterPlacement placement = option.Upload.Placement;
            for (int character = placement.FirstCharacter; character < placement.FirstCharacter + placement.CharacterCount; character++)
            {
                if (!placement.Covers(set, character))
                    continue;
                if (!painting.TryGetValue(character, out List<AppearanceOption>? animations))
                    painting[character] = animations = [];
                animations.Add(option);
            }
        }
    }

    /// <summary>
    /// The bytes of character <paramref name="character"/> as the room shows it: the frame the first animation painting it
    /// selected in <paramref name="frames"/>, which no VRAM image holds, or else its bytes in <paramref name="characters"/>,
    /// the set's characters from its first.
    /// </summary>
    internal ReadOnlySpan<byte> Bytes(ArtworkFrameSelections frames, int character, ReadOnlySpan<byte> characters, out bool animated)
    {
        int characterWords = set.CharacterBytes() / sizeof(ushort);
        if (painting.TryGetValue(character, out List<AppearanceOption>? animations))
        {
            foreach (AppearanceOption option in animations)
            {
                if (!frames.TryShown(((AnimationAppearance)option.Input).Animation, out ArtworkFrame frame))
                    continue;
                animated = true;
                int offset = (character - option.Upload.FirstCharacter) * characterWords;
                return AnimatedTileFrameArtwork.Bytes(BgArtworkIdentity.Frame(frame), animatedTiles).Span[(offset * sizeof(ushort))..];
            }
        }
        animated = false;
        return characters[(character * characterWords * sizeof(ushort))..];
    }
}
