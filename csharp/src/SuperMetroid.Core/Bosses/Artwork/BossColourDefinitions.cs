using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Bosses.Artwork;

/// <summary>The colour runs the <see cref="BossColours"/> view names (#1405).</summary>
public static class BossColourDefinitions
{
    /// <summary>
    /// Draygon opening colours: the first enemy store's sixteen, then the second's first nine, which hold Draygon's goop
    /// and their clear colour.
    /// </summary>
    public static readonly ImmutableArray<SpriteColourRun> DraygonIntro =
        [new(SpriteColourStore.EnemiesFirst, 0, 16), new(SpriteColourStore.EnemiesSecond, 0, 9)];

    /// <summary>Draygon background material.</summary>
    public static readonly BackgroundColourRun DraygonBackground = new(BackgroundColourSet.TilesFourth, 0, 16);

    /// <summary>Draygon sprite material.</summary>
    public static readonly SpriteColourRun DraygonSprite = new(SpriteColourStore.EnemiesFourth, 0, 16);

    /// <summary>
    /// The colours Draygon's goop draws in: the eight surface colours its opening transfer copies after its own sixteen
    /// and their clear colour.
    /// </summary>
    public static readonly SpriteColourRun DraygonGoop = new(SpriteColourStore.EnemiesSecond, 1, 8);

    /// <summary>Draygon health band.</summary>
    public static readonly BackgroundColourRun DraygonHealth = new(BackgroundColourSet.TilesFourth, 9, 4);

    /// <summary>Crocomire fight body.</summary>
    public static readonly BackgroundColourRun CrocomireFightBody = new(BackgroundColourSet.TilesSixth, 0, 8);

    /// <summary>The colours Crocomire's wall and its collapsing bridge's fragments draw in.</summary>
    public static readonly SpriteColourRun CrocomireWall = new(SpriteColourStore.EnemiesSecond, 0, 16);

    /// <summary>
    /// Crocomire initial wall: the wall's sixteen colours, then the third enemy store's clear colour, onto which the
    /// cartridge's transfer runs.
    /// </summary>
    public static readonly ImmutableArray<SpriteColourRun> CrocomireInitialWall =
        [CrocomireWall, new(SpriteColourStore.EnemiesThird, 0, 1)];

    /// <summary>
    /// Crocomire initial projectile: the enemy projectiles' store. The cartridge's transfer runs one colour further, onto
    /// the Samus beams store's clear colour, which no draw shows, so the port does not carry it (#1436).
    /// </summary>
    public static readonly SpriteColourRun CrocomireInitialProjectile = new(SpriteColourStore.EnemyProjectiles, 0, 16);

    /// <summary>Crocomire skeleton arm.</summary>
    public static readonly SpriteColourRun CrocomireSkeletonArm = new(SpriteColourStore.EnemiesFirst, 0, 16);

    /// <summary>Crocomire wall spikes.</summary>
    public static readonly SpriteColourRun CrocomireWallSpikes = new(SpriteColourStore.EnemiesThird, 0, 16);

    /// <summary>The Torizos' body colours.</summary>
    public static readonly SpriteColourRun TorizoBody = new(SpriteColourStore.EnemiesFirst, 0, 16);

    /// <summary>The Torizos' belly colours.</summary>
    public static readonly SpriteColourRun TorizoBelly = new(SpriteColourStore.EnemiesSecond, 0, 16);

    /// <summary>The body and belly colours a Torizo's body colouring loads and fades, body first.</summary>
    public static readonly ImmutableArray<SpriteColourRun> TorizoBodyAndBelly = [TorizoBody, TorizoBelly];

    /// <summary>The orb and flame colours both Torizos share.</summary>
    public static readonly SpriteColourRun TorizoOrbsAndFlames = new(SpriteColourStore.EnemiesThird, 0, 16);

    /// <summary>The statue-shading colours both Torizos share.</summary>
    public static readonly SpriteColourRun TorizoStatueShading = new(SpriteColourStore.EnemiesFourth, 0, 16);

    /// <summary>The colours both Torizos share, loaded together at initialization: orbs and flames, then statue shading.</summary>
    public static readonly ImmutableArray<SpriteColourRun> TorizoShared = [TorizoOrbsAndFlames, TorizoStatueShading];

    /// <summary>Botwoon's body palette.</summary>
    public static readonly SpriteColourRun BotwoonBody = new(SpriteColourStore.EnemiesFourth, 0, 16);

    /// <summary>The fourth enemy store's colours cleared at Ceres Ridley start.</summary>
    public static readonly SpriteColourRun CeresRidleyRevealedObjects = new(SpriteColourStore.EnemiesFourth, 1, 15);

    /// <summary>The cutscene Baby Metroid palette.</summary>
    public static readonly SpriteColourRun BabyMetroidCutscene = new(SpriteColourStore.EnemiesFourth, 1, 15);

    /// <summary>Kraid's room backdrop row.</summary>
    public static readonly BackgroundColourRun KraidRoomBackdrop = new(BackgroundColourSet.TilesFifth, 0, 16);

    /// <summary>Kraid's BG2 body row.</summary>
    public static readonly BackgroundColourRun KraidBody = new(BackgroundColourSet.TilesSixth, 0, 16);

    /// <summary>Kraid's eye colours.</summary>
    public static readonly BackgroundColourRun KraidEye = new(BackgroundColourSet.TilesSixth, 1, 3);

    /// <summary>Kraid's initial sprite row.</summary>
    public static readonly SpriteColourRun KraidSprites = new(SpriteColourStore.EnemiesThird, 0, 16);

    /// <summary>Kraid's secondary row.</summary>
    public static readonly SpriteColourRun KraidSecondary = new(SpriteColourStore.EnemiesFourth, 0, 16);

    /// <summary>Mother Brain's body colours.</summary>
    public static readonly BackgroundColourRun MotherBrainBody = new(BackgroundColourSet.TilesThird, 1, 14);

    /// <summary>Mother Brain's brain colours.</summary>
    public static readonly SpriteColourRun MotherBrainBrain = new(SpriteColourStore.EnemiesFirst, 1, 15);

    /// <summary>Mother Brain's leg colours.</summary>
    public static readonly SpriteColourRun MotherBrainLegs = new(SpriteColourStore.EnemiesThird, 1, 14);

    /// <summary>Mother Brain's corpse colours.</summary>
    public static readonly SpriteColourRun MotherBrainCorpse = new(SpriteColourStore.EnemiesFourth, 1, 15);

    /// <summary>Phantoon's body palette.</summary>
    public static readonly BackgroundColourRun PhantoonBody = new(BackgroundColourSet.TilesSixth, 0, 16);

    /// <summary>Norfair Ridley's body colours.</summary>
    public static readonly BackgroundColourRun NorfairRidleyBody = new(BackgroundColourSet.TilesSixth, 1, 15);

    /// <summary>Norfair Ridley's tail and wing colours.</summary>
    public static readonly SpriteColourRun NorfairRidleyTail = new(SpriteColourStore.EnemiesFourth, 1, 15);

    /// <summary>The Sidehopper the Shitroid drains.</summary>
    public static readonly SpriteColourRun ShitroidSidehopper = new(SpriteColourStore.EnemiesFirst, 0, 16);

    /// <summary>The Shitroid's body.</summary>
    public static readonly SpriteColourRun ShitroidBody = new(SpriteColourStore.EnemiesSecond, 0, 16);

    /// <summary>The Shitroid's cycling membrane colours.</summary>
    public static readonly SpriteColourRun ShitroidMembrane = new(SpriteColourStore.EnemiesSecond, 5, 4);

    /// <summary>Spore Spawn's spore palette.</summary>
    public static readonly SpriteColourRun SporeSpawnSpores = new(SpriteColourStore.EnemiesFourth, 0, 16);

    /// <summary>Spore Spawn's body palette.</summary>
    public static readonly SpriteColourRun SporeSpawnBody = new(SpriteColourStore.EnemiesFirst, 0, 16);

    /// <summary>The room's level colours Spore Spawn's death fades.</summary>
    public static readonly BackgroundColourRun SporeSpawnDeathLevel = new(BackgroundColourSet.TilesThird, 0, 16);

    /// <summary>The room's background colours Spore Spawn's death fades.</summary>
    public static readonly BackgroundColourRun SporeSpawnDeathBackground = new(BackgroundColourSet.TilesSixth, 0, 16);
}
