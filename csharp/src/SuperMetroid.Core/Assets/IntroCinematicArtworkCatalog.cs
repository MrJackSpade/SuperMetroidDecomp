namespace SuperMetroid.Core.Assets;

/// <summary>
/// Installed palette-indexed characters for the opening cinematic's BG and two OBJ
/// transfers. Their VRAM destinations and scene timing remain compiled game logic.
/// </summary>
public sealed class IntroCinematicArtworkCatalog
{
    public IntroCinematicArtworkCatalog(RoomCharacterAtlas backgroundCharacters,
        RoomCharacterAtlas introObjectCharacters, RoomCharacterAtlas cinematicObjectCharacters)
    {
        BackgroundCharacters = backgroundCharacters ?? throw new ArgumentNullException(nameof(backgroundCharacters));
        IntroObjectCharacters = introObjectCharacters ?? throw new ArgumentNullException(nameof(introObjectCharacters));
        CinematicObjectCharacters = cinematicObjectCharacters ?? throw new ArgumentNullException(nameof(cinematicObjectCharacters));
    }

    /// <summary>BG characters uploaded to VRAM byte $0000.</summary>
    public RoomCharacterAtlas BackgroundCharacters { get; }
    /// <summary>Fixed-bank intro OBJ characters uploaded to VRAM byte $C000.</summary>
    public RoomCharacterAtlas IntroObjectCharacters { get; }
    /// <summary>Decompressed cinematic OBJ characters uploaded to VRAM byte $DC00.</summary>
    public RoomCharacterAtlas CinematicObjectCharacters { get; }
}

/// <summary>File identities and physical 4-bpp transfer lengths for the opening scene.</summary>
public static class IntroCinematicArtworkFormat
{
    public const string ManifestFileName = "intro-artwork.json";
    public const string BackgroundFileName = "intro-background-characters.png";
    public const string IntroObjectFileName = "intro-object-characters.png";
    public const string CinematicObjectFileName = "intro-cinematic-object-characters.png";
    public const int BackgroundByteCount = 0x8000;
    public const int IntroObjectByteCount = 0x2000;
    public const int CinematicObjectByteCount = 0x2400;
    public const int TileColumns = 32;
}
