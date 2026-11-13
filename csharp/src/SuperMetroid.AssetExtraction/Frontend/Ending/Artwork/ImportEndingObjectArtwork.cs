using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>
/// The ending object artwork as the import reads it (#1636): the installed maps and sprite compositions with the object
/// characters from the component's import data, which the ending's pictures and sheets are composed from. The game reads
/// none of the characters.
/// </summary>
public sealed class ImportEndingObjectArtwork : EndingObjectSourceArtwork
{
    /// <summary>Validates and retains the installed documents and the import's characters.</summary>
    /// <param name="parts">The ending's maps and sprite presentations.</param>
    /// <param name="characters">The ending's object characters.</param>
    /// <exception cref="ArgumentNullException">A part or a character atlas is null.</exception>
    /// <exception cref="InvalidDataException">A transfer has an incorrect byte length, or the fragment sequence is malformed.</exception>
    public ImportEndingObjectArtwork(EndingObjectArtworkParts parts, EndingObjectCharacters characters)
        : base(parts)
    {
        Ensure.NotNull(characters);
        Clouds = Ensure.NotNull(characters.Clouds);
        Explosion = Ensure.NotNull(characters.Explosion);
        WaitingSamus = Ensure.NotNull(characters.WaitingSamus);
        ShootingScreen = Ensure.NotNull(characters.ShootingScreen);
        SuitlessSamus = Ensure.NotNull(characters.SuitlessSamus);
        PostCreditsFragmentA = Ensure.NotNull(characters.PostCreditsFragmentA);
        PostShotLogoTiles = Ensure.NotNull(characters.PostShotLogoTiles);
        if (!HasRequiredLength(Clouds, EndingObjectTransfer.Clouds) ||
            !HasRequiredLength(Explosion, EndingObjectTransfer.Explosion) ||
            !HasRequiredLength(WaitingSamus, EndingObjectTransfer.WaitingSamus) ||
            !HasRequiredLength(ShootingScreen, EndingObjectTransfer.ShootingScreen) ||
            !HasRequiredLength(SuitlessSamus, EndingObjectTransfer.SuitlessSamus) ||
            !HasRequiredLength(PostCreditsFragmentA, EndingObjectTransfer.PostCreditsFragmentA) ||
            !HasRequiredLength(PostShotLogoTiles, EndingObjectTransfer.PostShotLogoTiles))
            throw new InvalidDataException("Ending OBJ catalog has an incorrect native transfer length.");
    }

    /// <summary>Checks an atlas transfer against the cartridge's required byte count.</summary>
    private static bool HasRequiredLength(RoomCharacterAtlas atlas, EndingObjectTransfer transfer) =>
        atlas.Characters.Length == EndingObjectArtworkFormat.ByteCount(transfer);

    /// <summary>Compiled 4-bpp atmospheric-cloud OBJ sheet imported from <c>$99:A56F</c> and uploaded at VRAM byte $C000.</summary>
    public RoomCharacterAtlas Clouds { get; }
    /// <summary>Compiled 4-bpp explosion OBJ sheet imported from <c>$98:8304</c> and uploaded at VRAM byte $8000; separate tilemap fragments occupy $E000-$FFFF.</summary>
    public RoomCharacterAtlas Explosion { get; }
    /// <summary>BG2 waiting scene, also reused by both suited reward variants.</summary>
    public RoomCharacterAtlas WaitingSamus { get; }
    /// <summary>Post-credits shooting scene OBJ sheet.</summary>
    public RoomCharacterAtlas ShootingScreen { get; }
    /// <summary>Under-three-hour suitless reward sheet.</summary>
    public RoomCharacterAtlas SuitlessSamus { get; }
    /// <summary>BG3 transformation characters imported from <c>$99:DA9F</c> and uploaded at VRAM byte $4000; separate from the transformation map.</summary>
    public RoomCharacterAtlas PostCreditsFragmentA { get; }
    /// <summary>Four post-shot logo tile chunks uploaded to BG/OBJ VRAM.</summary>
    public RoomCharacterAtlas PostShotLogoTiles { get; }
}
