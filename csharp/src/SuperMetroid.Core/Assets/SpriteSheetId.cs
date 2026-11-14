namespace SuperMetroid.Core.Assets;

/// <summary>A pre-coloured sprite sheet the installation provides, one per drawn family and standard appearance.</summary>
public enum SpriteSheetId
{
    /// <summary>The escape countdown's label and digits.</summary>
    EscapeTimer,
    /// <summary>The grapple beam's endpoint and rope.</summary>
    Grapple,
    /// <summary>Samus's full-body frames, one per body an update draws.</summary>
    SamusBody,
    /// <summary>The arm cannon's covers drawn over Samus's body.</summary>
    SamusOverlays,
    /// <summary>Samus's footstep splashes, lava spray, dust, diving splash and bubbles.</summary>
    SamusAtmosphere,
    /// <summary>Samus's suit explosion as she dies.</summary>
    SamusDeath,
    /// <summary>The game-over screen's Baby, egg and answer cursor.</summary>
    GameOver,
    /// <summary>File select's page borders, cursor, helmets and file-copy arrows.</summary>
    FileSelect,
    /// <summary>The options menu's page headings and cursor.</summary>
    GameOptions,
    /// <summary>The file-select and pause maps' markers, elevator and area labels, arrows and position indicator.</summary>
    MapSprites,
    /// <summary>The pause menu's equipment selectors and reserve-tank strip.</summary>
    PauseSprites,
    /// <summary>The title's year, Nintendo and Super Metroid logo reveals and copyright.</summary>
    Title,
    /// <summary>The intro's cinematic actors, Mother Brain and her explosions, and the narration caret.</summary>
    IntroSprites,
    /// <summary>The Ceres approach's stars and rear-view gunship and station.</summary>
    CeresFlightSprites,
    /// <summary>The Ceres destruction scene's actors as they look before the Zebes reveal, in the Ceres station scene's artwork.</summary>
    CeresDestructionSprites,
    /// <summary>The Ceres destruction scene's actors as they look once the Zebes reveal shows its own artwork.</summary>
    ZebesRevealSprites,

    /// <summary>The ending's escape clouds.</summary>
    EndingCloudSprites,

    /// <summary>The ending's Zebes explosion, completion message and escape pod.</summary>
    EndingExplosionSprites,

    /// <summary>The post-credits Samus in her suit, as the helmetless and armoured endings show her.</summary>
    EndingSuitedRewardSprites,

    /// <summary>The post-credits Samus out of her suit.</summary>
    EndingSuitlessRewardSprites,

    /// <summary>The falling, landing and shooting Samus, the stars and the logo the shooting scene ends on.</summary>
    EndingShootingSprites,

    /// <summary>Samus's projectiles, bombs, charge flare and trails as the intro's flashbacks draw them.</summary>
    IntroProjectiles,
    /// <summary>Samus's projectiles, bombs, charge flare and trails with the power beam equipped.</summary>
    ProjectilesPower,
    /// <summary>Samus's projectiles, bombs, charge flare and trails with the wave beam equipped.</summary>
    ProjectilesWave,
    /// <summary>Samus's projectiles, bombs, charge flare and trails with the ice beam equipped.</summary>
    ProjectilesIce,
    /// <summary>Samus's projectiles, bombs, charge flare and trails with the ice, wave beam equipped.</summary>
    ProjectilesIceWave,
    /// <summary>Samus's projectiles, bombs, charge flare and trails with the spazer beam equipped.</summary>
    ProjectilesSpazer,
    /// <summary>Samus's projectiles, bombs, charge flare and trails with the spazer, wave beam equipped.</summary>
    ProjectilesSpazerWave,
    /// <summary>Samus's projectiles, bombs, charge flare and trails with the spazer, ice beam equipped.</summary>
    ProjectilesSpazerIce,
    /// <summary>Samus's projectiles, bombs, charge flare and trails with the spazer, ice, wave beam equipped.</summary>
    ProjectilesSpazerIceWave,
    /// <summary>Samus's projectiles, bombs, charge flare and trails with the plasma beam equipped.</summary>
    ProjectilesPlasma,
    /// <summary>Samus's projectiles, bombs, charge flare and trails with the plasma, wave beam equipped.</summary>
    ProjectilesPlasmaWave,
    /// <summary>Samus's projectiles, bombs, charge flare and trails with the plasma, ice beam equipped.</summary>
    ProjectilesPlasmaIce,
    /// <summary>Samus's projectiles, bombs, charge flare and trails with the plasma, ice, wave beam equipped.</summary>
    ProjectilesPlasmaIceWave,
    /// <summary>Samus's projectiles, bombs, charge flare and trails with the spazer, plasma, wave beam equipped.</summary>
    ProjectilesSpazerPlasmaWave,
    /// <summary>Samus's projectiles, bombs, charge flare and trails with the spazer, plasma, ice beam equipped.</summary>
    ProjectilesSpazerPlasmaIce,
    /// <summary>The menus' pictures under the file-select palette: each menu page and the pictures placed over it (#1467).</summary>
    MenuPictures,
    /// <summary>The room maps' pictures under the file-select palette: each map cell picture and each area's room frame (#1467).</summary>
    RoomMapPictures,
    /// <summary>The pause screen's pictures under the pause palette: each map cell picture, each area's backdrop, the button rows and labels, and the equipment page's parts (#1467).</summary>
    PausePictures,
    /// <summary>The file-select world map's foreground picture in Crateria's world-map colours (#1392).</summary>
    AreaMapForegroundCrateria,
    /// <summary>The file-select world map's foreground picture in Brinstar's world-map colours (#1392).</summary>
    AreaMapForegroundBrinstar,
    /// <summary>The file-select world map's foreground picture in Norfair's world-map colours (#1392).</summary>
    AreaMapForegroundNorfair,
    /// <summary>The file-select world map's foreground picture in Wrecked Ship's world-map colours (#1392).</summary>
    AreaMapForegroundWreckedShip,
    /// <summary>The file-select world map's foreground picture in Maridia's world-map colours (#1392).</summary>
    AreaMapForegroundMaridia,
    /// <summary>The file-select world map's foreground picture in Tourian's world-map colours (#1392).</summary>
    AreaMapForegroundTourian,
    /// <summary>The file-select world map's background picture in Crateria's world-map colours (#1392).</summary>
    AreaMapBackgroundCrateria,
    /// <summary>The file-select world map's background picture in Brinstar's world-map colours (#1392).</summary>
    AreaMapBackgroundBrinstar,
    /// <summary>The file-select world map's background picture in Norfair's world-map colours (#1392).</summary>
    AreaMapBackgroundNorfair,
    /// <summary>The file-select world map's background picture in Wrecked Ship's world-map colours (#1392).</summary>
    AreaMapBackgroundWreckedShip,
    /// <summary>The file-select world map's background picture in Maridia's world-map colours (#1392).</summary>
    AreaMapBackgroundMaridia,
    /// <summary>The file-select world map's background picture in Tourian's world-map colours (#1392).</summary>
    AreaMapBackgroundTourian,
    /// <summary>The HUD's images and the cleared cell pause shows on its HUD plane, under the pause palette (#1392, #1466).</summary>
    PauseHud,
    /// <summary>The opening cinematic's scene pictures: its scene pages, Samus's portrait and the portrait's eye frames (#1467).</summary>
    IntroScenePictures,
    /// <summary>The opening narration's pictures: its first card, its final line and each letter in each glow colour (#1467).</summary>
    IntroNarrationPictures,
    /// <summary>The Ceres cinematics' background pictures: the caption's letters, the katakana caption and the Zebes planet (#1467).</summary>
    CeresCinematicPictures,
    /// <summary>The credits roll's staff line pictures under the credits palette (#1467).</summary>
    EndingCreditsRows,
    /// <summary>The post-credits pictures under the post-credits palette: the waiting backdrop, the logo, the panels and the typed glyphs (#1467).</summary>
    EndingPostCreditsPictures,
    /// <summary>The wavy Samus transformation's picture under the post-credits palette (#1467).</summary>
    EndingWavySamusPicture,
    /// <summary>The Zebes explosion's four maps' pictures under the explosion palette (#1467).</summary>
    EndingExplosionPictures,
    /// <summary>The title's turning floor with each Baby Metroid page shown, under its palette (#1392, #1467).</summary>
    TitleFloor,
    /// <summary>The Ceres flight's and destruction's turning floor in each of their shots, under the flight's palette (#1392, #1467).</summary>
    CeresFloor,
    /// <summary>The ending's side-cloud escape's turning floor under the escape palette (#1392, #1467).</summary>
    EndingSideCloudEscapeFloor,
    /// <summary>The ending's top-and-bottom-cloud escape's turning floor under the escape palette (#1392, #1467).</summary>
    EndingTopAndBottomCloudEscapeFloor,
    /// <summary>The ending's planet explosion's turning floor under the explosion palette (#1392, #1467).</summary>
    EndingPlanetExplosionFloor,
    /// <summary>The flyaway gunship's turning floor under the escape palette (#1392, #1467).</summary>
    EndingFlyawayFloor,
    /// <summary>The reward icon's turning floor under the post-credits palette (#1392, #1467).</summary>
    EndingRewardIconFloor,
}
