using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Bosses;

/// <summary>
/// Kraid's two visual BG2 source maps. These are tile references, not the boss's
/// attack, collision, or growth instructions; the engine composes the working map.
/// </summary>
public sealed class KraidBackgroundArtwork
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("enemy-kraid-background-v3", content =>
        {
            content.AppendTiles("upper", Upper.Tiles.Span);
            content.AppendTiles("lower", Lower.Tiles.Span);
            content.Append("background-tiles", RoomBackgroundTiles.Characters.Span);
            foreach ((int mouthStage, KraidHeadTilemapAtlas head) in heads.OrderBy(pair => pair.Key))
            {
                content.Append("head-mouth-stage", mouthStage);
                content.AppendTiles("head-tiles", head.Tiles.Span);
            }
        });

    /// <summary>Immutable head tilemaps keyed by how far the head's mouth is open.</summary>
    private readonly Dictionary<int, KraidHeadTilemapAtlas> heads;

    /// <summary>Groups installed Kraid BG2 maps, head frames, and revealed room characters without composing or uploading the boss's mutable working tilemap.</summary>
    /// <param name="upper">Upper-body source atlas with the native $1000-byte decompressed layout.</param>
    /// <param name="lower">Lower-body source atlas with the native $1000-byte decompressed layout.</param>
    /// <param name="heads">Head-frame atlases keyed by mouth stage, as <see cref="KraidHeadFrame.MouthStage"/> counts it. The dictionary is copied; immutable atlas objects are shared.</param>
    /// <param name="roomBackgroundTiles">The $0200-byte 4-bpp character stream revealed after growth or defeat, separate from the boss's BG2 tile references.</param>
    /// <remarks>The caller supplies already validated atlases. Construction retains the upper, lower, and character objects and does not validate their lengths or the completeness of the head-frame set.</remarks>
    public KraidBackgroundArtwork(RoomBackgroundTilemapAtlas upper,
        RoomBackgroundTilemapAtlas lower,
        IReadOnlyDictionary<int, KraidHeadTilemapAtlas> heads,
        RoomCharacterAtlas roomBackgroundTiles)
    {
        Upper = upper;
        Lower = lower;
        this.heads = new Dictionary<int, KraidHeadTilemapAtlas>(heads);
        RoomBackgroundTiles = roomBackgroundTiles;
    }

    /// <summary>Gets the immutable $1000-byte source from $B9:FA38, Background_Brinstar_1A_Kraid_Upper; $A7:AAC6 copies its first 32-by-32 page into the working map with priority cleared.</summary>
    public RoomBackgroundTilemapAtlas Upper { get; }
    /// <summary>Gets the immutable $1000-byte source from $B9:FE3E, Background_Brinstar_1A_Kraid_Lower_0; the engine seeds the working map from it, then relocates its first $0300 words into the lower half.</summary>
    public RoomBackgroundTilemapAtlas Lower { get; }

    /// <summary>Characters revealed behind Kraid after growth or defeat.</summary>
    public RoomCharacterAtlas RoomBackgroundTiles { get; }

    /// <summary>The installed head image <paramref name="frame"/> shows, without changing its timing or hitboxes.</summary>
    internal ReadOnlySpan<BackgroundTile> HeadTiles(KraidHeadFrame frame) =>
        heads.TryGetValue(frame.MouthStage, out KraidHeadTilemapAtlas? image)
            ? image.Tiles.Span
            : throw new InvalidDataException(
                $"Kraid head mouth stage {frame.MouthStage} has no installed tilemap.");
}
