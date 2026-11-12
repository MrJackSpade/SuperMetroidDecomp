using SuperMetroid.AssetExtraction.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>
/// The ending's object characters, from the component's import data (#1636): the clouds', the explosion's, the waiting,
/// shooting and suitless Samus scenes', the transformation's and the final logo's. The import composes the ending's pictures
/// and sheets from them; the installation never holds them.
/// </summary>
/// <param name="Clouds">$4000-byte atmospheric-cloud OBJ character sheet.</param>
/// <param name="Explosion">$6000-byte Zebes-explosion OBJ character sheet.</param>
/// <param name="WaitingSamus">$4000-byte waiting-Samus character sheet, also used for suited rewards.</param>
/// <param name="ShootingScreen">$4000-byte Samus-shooting OBJ sheet.</param>
/// <param name="SuitlessSamus">$4000-byte suitless reward OBJ sheet.</param>
/// <param name="PostCreditsFragmentA">$0100-byte BG3 transformation-character transfer.</param>
/// <param name="PostShotLogoTiles">$2000-byte logo character stream divided into four staged uploads.</param>
public sealed record EndingObjectCharacters(RoomCharacterAtlas Clouds, RoomCharacterAtlas Explosion, RoomCharacterAtlas WaitingSamus,
    RoomCharacterAtlas ShootingScreen, RoomCharacterAtlas SuitlessSamus, RoomCharacterAtlas PostCreditsFragmentA,
    RoomCharacterAtlas PostShotLogoTiles);
