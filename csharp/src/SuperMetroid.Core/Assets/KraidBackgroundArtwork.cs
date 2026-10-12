namespace SuperMetroid.Core.Assets;

/// <summary>
/// Kraid's two visual BG2 source maps. These are tile references, not the boss's
/// attack, collision, or growth instructions; the engine composes the working map.
/// </summary>
public sealed class KraidBackgroundArtwork
{
    /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create("enemy-kraid-background-v1", content =>
        {
            content.Append("upper", Upper.Transfer.Span);
            content.Append("lower", Lower.Transfer.Span);
            content.Append("background-tiles", RoomBackgroundTiles.Transfer.Span);
            foreach ((ushort pointer, KraidHeadTilemapAtlas head) in heads.OrderBy(pair => pair.Key))
            {
                content.Append("head-frame", pointer);
                content.AppendWords("head-words", head.Words.Span);
            }
        });

    private readonly Dictionary<ushort, KraidHeadTilemapAtlas> heads;

    /// <summary>Groups installed Kraid BG2 maps, head frames, and revealed room characters without composing or uploading the boss's mutable working tilemap.</summary>
    /// <param name="upper">Upper-body source atlas with the native $1000-byte decompressed layout.</param>
    /// <param name="lower">Lower-body source atlas with the native $1000-byte decompressed layout.</param>
    /// <param name="heads">Head-frame atlases keyed by bank-$A7 source pointer. The dictionary is copied; immutable atlas objects are shared.</param>
    /// <param name="roomBackgroundTiles">The $0200-byte 4-bpp character stream revealed after growth or defeat, separate from the boss's BG2 tile references.</param>
    /// <remarks>The caller supplies already validated atlases. Construction retains the upper, lower, and character objects and does not validate their lengths or the completeness of the head-frame set.</remarks>
    public KraidBackgroundArtwork(RoomBackgroundTilemapAtlas upper,
        RoomBackgroundTilemapAtlas lower,
        IReadOnlyDictionary<ushort, KraidHeadTilemapAtlas> heads,
        RoomCharacterAtlas roomBackgroundTiles)
    {
        Upper = upper;
        Lower = lower;
        this.heads = new Dictionary<ushort, KraidHeadTilemapAtlas>(heads);
        RoomBackgroundTiles = roomBackgroundTiles;
    }

    /// <summary>Gets the immutable $1000-byte source from $B9:FA38, Background_Brinstar_1A_Kraid_Upper; $A7:AAC6 copies its first 32-by-32 page into the working map with priority cleared.</summary>
    public RoomBackgroundTilemapAtlas Upper { get; }
    /// <summary>Gets the immutable $1000-byte source from $B9:FE3E, Background_Brinstar_1A_Kraid_Lower_0; the engine seeds the working map from it, then relocates its first $0300 words into the lower half.</summary>
    public RoomBackgroundTilemapAtlas Lower { get; }

    /// <summary>Characters revealed behind Kraid after growth or defeat.</summary>
    public RoomCharacterAtlas RoomBackgroundTiles { get; }

    /// <summary>Resolves one cartridge-selected frame without changing its timing or hitboxes.</summary>
    public ReadOnlySpan<ushort> HeadWords(ushort sourcePointer) =>
        heads.TryGetValue(sourcePointer, out KraidHeadTilemapAtlas? frame)
            ? frame.Words.Span
            : throw new InvalidDataException(
                $"Kraid head frame $A7:{sourcePointer:X4} has no installed tilemap.");
}

/// <summary>Stable installed filenames for Kraid's private BG2 source maps.</summary>
public static class KraidBackgroundArtworkFormat
{
    /// <summary>Installation-relative JSON filename for the upper-body BG2 source, not the engine's composed live tilemap.</summary>
    public const string UpperFileName = "kraid-upper-bg2.json";
    /// <summary>Installation-relative JSON filename for the lower-body BG2 source, including the tail retained by the working-map composition.</summary>
    public const string LowerFileName = "kraid-lower-bg2.json";
    /// <summary>Installation-relative indexed PNG filename for $A7:A716, Tiles_KraidRoomBackground: sixteen 4-bpp characters uploaded to VRAM word $3F00 after growth or defeat.</summary>
    public const string RoomBackgroundFileName = "kraid-room-background.png";

    /// <summary>Formats the installation-relative JSON filename for one 32-by-11 head tilemap, using its bank-$A7 source pointer as an uppercase four-digit hexadecimal identity.</summary>
    /// <param name="sourcePointer">Native head-art pointer selected by Kraid's instruction lists; the formatter does not validate that a corresponding installed frame exists.</param>
    /// <returns>The filename kraid-head-XXXX.json; instruction timing and hitboxes remain outside this visual file.</returns>
    public static string HeadFileName(ushort sourcePointer) =>
        $"kraid-head-{sourcePointer:X4}.json";
}
