namespace SuperMetroid.Core.Assets;

/// <summary>A pre-coloured sprite sheet the installation provides, one per drawn family and standard appearance.</summary>
public enum SpriteSheetId
{
    /// <summary>The escape countdown's label and digits.</summary>
    EscapeTimer,
    /// <summary>The grapple beam's endpoint and rope.</summary>
    Grapple,
    /// <summary>Samus's body frames that show the top half's characters, or neither half's.</summary>
    SamusBodyTop,
    /// <summary>Samus's body frames that show only the bottom half's characters.</summary>
    SamusBodyBottom,
    /// <summary>The arm cannon's covers and the chest cover drawn over Samus's body.</summary>
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
    /// <summary>The Ceres destruction scene's actors over the Ceres scene's OBJ characters.</summary>
    CeresDestructionSprites,
    /// <summary>The Ceres destruction scene's actors over the Zebes reveal's OBJ characters.</summary>
    ZebesRevealSprites,

    /// <summary>The ending's escape clouds.</summary>
    EndingCloudSprites,

    /// <summary>The ending's Zebes explosion, completion message and escape pod.</summary>
    EndingExplosionSprites,

    /// <summary>The post-credits Samus in her suit, as the helmetless and armoured endings show her.</summary>
    EndingSuitedRewardSprites,

    /// <summary>The post-credits Samus out of her suit.</summary>
    EndingSuitlessRewardSprites,

    /// <summary>The falling, landing and shooting Samus and the stars over the shooting scene's characters.</summary>
    EndingShootingSprites,

    /// <summary>The shooting scene once the first quarter of the logo's characters has replaced its own.</summary>
    EndingShootingLogoQuarterSprites,

    /// <summary>The shooting scene once half of the logo's characters have replaced its own.</summary>
    EndingShootingLogoHalfSprites,

    /// <summary>The shooting scene once three quarters of the logo's characters have replaced its own.</summary>
    EndingShootingLogoThreeQuartersSprites,

    /// <summary>The assembling logo, the stars and the last of the shooting scene once the logo's characters are in.</summary>
    EndingLogoSprites,
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
    /// <summary>The menu's world foreground characters under Crateria's world-map palette (#1392).</summary>
    AreaMapForegroundCrateria,
    /// <summary>The menu's world foreground characters under Brinstar's world-map palette (#1392).</summary>
    AreaMapForegroundBrinstar,
    /// <summary>The menu's world foreground characters under Norfair's world-map palette (#1392).</summary>
    AreaMapForegroundNorfair,
    /// <summary>The menu's world foreground characters under WreckedShip's world-map palette (#1392).</summary>
    AreaMapForegroundWreckedShip,
    /// <summary>The menu's world foreground characters under Maridia's world-map palette (#1392).</summary>
    AreaMapForegroundMaridia,
    /// <summary>The menu's world foreground characters under Tourian's world-map palette (#1392).</summary>
    AreaMapForegroundTourian,
    /// <summary>The menu's world background characters under Crateria's world-map palette (#1392).</summary>
    AreaMapBackgroundCrateria,
    /// <summary>The menu's world background characters under Brinstar's world-map palette (#1392).</summary>
    AreaMapBackgroundBrinstar,
    /// <summary>The menu's world background characters under Norfair's world-map palette (#1392).</summary>
    AreaMapBackgroundNorfair,
    /// <summary>The menu's world background characters under WreckedShip's world-map palette (#1392).</summary>
    AreaMapBackgroundWreckedShip,
    /// <summary>The menu's world background characters under Maridia's world-map palette (#1392).</summary>
    AreaMapBackgroundMaridia,
    /// <summary>The menu's world background characters under Tourian's world-map palette (#1392).</summary>
    AreaMapBackgroundTourian,
    /// <summary>The map characters under the pause palette (#1392).</summary>
    PauseMapTiles,
    /// <summary>The pause interface characters under the pause palette (#1392).</summary>
    PauseInterfaceTiles,
    /// <summary>The HUD's images and the cleared cell pause shows on its HUD plane, under the pause palette (#1392, #1466).</summary>
    PauseHud,
    /// <summary>The opening cinematic's scene characters under its palette (#1392).</summary>
    IntroSceneTiles,
    /// <summary>The opening cinematic's narration font under its palette (#1392).</summary>
    IntroFontTiles,
    /// <summary>The Ceres cinematics' background pictures: the caption's letters, the katakana caption and the Zebes planet (#1467).</summary>
    CeresCinematicPictures,
    /// <summary>The credits roll's staff line pictures under the credits palette (#1467).</summary>
    EndingCreditsRows,
    /// <summary>The post-credits pictures under the post-credits palette: the waiting backdrop, the logo, the panels and the typed glyphs (#1467).</summary>
    EndingPostCreditsPictures,
    /// <summary>The wavy Samus transformation's four-colour picture under the post-credits palette (#1467).</summary>
    EndingWavySamusPicture,
    /// <summary>The Zebes explosion's four maps' pictures under the explosion palette (#1467).</summary>
    EndingExplosionPictures,
    /// <summary>The title's turning floor with each Baby Metroid page shown, under its palette (#1392, #1467).</summary>
    TitleFloor,
    /// <summary>The Ceres flight's and destruction's turning floor in each of their shots, under the flight's palette (#1392, #1467).</summary>
    CeresFloor,
    /// <summary>The ending's first escape backdrop's turning floor under the escape palette (#1392, #1467).</summary>
    EndingEscapeAFloor,
    /// <summary>The ending's second escape backdrop's turning floor under the escape palette (#1392, #1467).</summary>
    EndingEscapeBFloor,
    /// <summary>The ending's planet explosion's turning floor under the explosion palette (#1392, #1467).</summary>
    EndingPlanetExplosionFloor,
    /// <summary>The flyaway gunship's turning floor under the escape palette (#1392, #1467).</summary>
    EndingFlyawayFloor,
    /// <summary>The reward icon's turning floor under the post-credits palette (#1392, #1467).</summary>
    EndingRewardIconFloor,
}
