using SuperMetroid.Core.Assets;
namespace SuperMetroid.Core.Bosses.Artwork;

/// <summary>The colour runs the <see cref="BossColours"/> view names (#1405).</summary>
public static class BossColourDefinitions
{
    /// <summary>Draygon opening colours.</summary>
    public static readonly SpriteColourRun DraygonIntro = new(1, 0, 25);

    /// <summary>Draygon background material.</summary>
    public static readonly BackgroundColourRun DraygonBackground = new(5, 0, 16);

    /// <summary>Draygon sprite material.</summary>
    public static readonly SpriteColourRun DraygonSprite = new(7, 0, 16);

    /// <summary>Draygon health band.</summary>
    public static readonly BackgroundColourRun DraygonHealth = new(5, 9, 4);

    /// <summary>Crocomire fight body.</summary>
    public static readonly BackgroundColourRun CrocomireFightBody = new(7, 0, 8);

    /// <summary>Crocomire initial wall.</summary>
    public static readonly SpriteColourRun CrocomireInitialWall = new(2, 0, 17);

    /// <summary>Crocomire initial projectile.</summary>
    public static readonly SpriteColourRun CrocomireInitialProjectile = new(5, 0, 17);

    /// <summary>Crocomire skeleton arm.</summary>
    public static readonly SpriteColourRun CrocomireSkeletonArm = new(1, 0, 16);

    /// <summary>Crocomire wall spikes.</summary>
    public static readonly SpriteColourRun CrocomireWallSpikes = new(3, 0, 16);

    /// <summary>Torizo body row.</summary>
    public static readonly SpriteColourRun TorizoBody = new(1, 0, 16);

    /// <summary>Torizo belly row.</summary>
    public static readonly SpriteColourRun TorizoBelly = new(2, 0, 16);

    /// <summary>First shared Torizo row.</summary>
    public static readonly SpriteColourRun TorizoSharedFirst = new(3, 0, 16);

    /// <summary>Second shared Torizo row.</summary>
    public static readonly SpriteColourRun TorizoSharedSecond = new(7, 0, 16);

    /// <summary>Botwoon's body palette.</summary>
    public static readonly SpriteColourRun BotwoonBody = new(7, 0, 16);

    /// <summary>OBJ palette 7 colours cleared at Ceres Ridley start.</summary>
    public static readonly SpriteColourRun CeresRidleyRevealedObjects = new(7, 1, 15);

    /// <summary>The cutscene Baby Metroid palette.</summary>
    public static readonly SpriteColourRun BabyMetroidCutscene = new(7, 1, 15);

    /// <summary>Kraid's room backdrop row.</summary>
    public static readonly BackgroundColourRun KraidRoomBackdrop = new(6, 0, 16);

    /// <summary>Kraid's BG2 body row.</summary>
    public static readonly BackgroundColourRun KraidBody = new(7, 0, 16);

    /// <summary>Kraid's eye colours.</summary>
    public static readonly BackgroundColourRun KraidEye = new(7, 1, 3);

    /// <summary>Kraid's initial sprite row.</summary>
    public static readonly SpriteColourRun KraidSprites = new(3, 0, 16);

    /// <summary>Kraid's secondary row.</summary>
    public static readonly SpriteColourRun KraidSecondary = new(7, 0, 16);

    /// <summary>Mother Brain's body colours.</summary>
    public static readonly BackgroundColourRun MotherBrainBody = new(4, 1, 14);

    /// <summary>Mother Brain's brain colours.</summary>
    public static readonly SpriteColourRun MotherBrainBrain = new(1, 1, 15);

    /// <summary>Mother Brain's leg colours.</summary>
    public static readonly SpriteColourRun MotherBrainLegs = new(3, 1, 14);

    /// <summary>Mother Brain's corpse colours.</summary>
    public static readonly SpriteColourRun MotherBrainCorpse = new(7, 1, 15);

    /// <summary>Phantoon's body palette.</summary>
    public static readonly BackgroundColourRun PhantoonBody = new(7, 0, 16);

    /// <summary>Norfair Ridley's body colours.</summary>
    public static readonly BackgroundColourRun NorfairRidleyBody = new(7, 1, 15);

    /// <summary>Norfair Ridley's tail and wing colours.</summary>
    public static readonly SpriteColourRun NorfairRidleyTail = new(7, 1, 15);

    /// <summary>The Sidehopper the Shitroid drains.</summary>
    public static readonly SpriteColourRun ShitroidSidehopper = new(1, 0, 16);

    /// <summary>The Shitroid's body.</summary>
    public static readonly SpriteColourRun ShitroidBody = new(2, 0, 16);

    /// <summary>The drained Sidehopper's corpse.</summary>
    public static readonly SpriteColourRun ShitroidDeadSidehopper = new(7, 0, 16);

    /// <summary>The Shitroid's cycling membrane colours.</summary>
    public static readonly SpriteColourRun ShitroidMembrane = new(2, 5, 4);

    /// <summary>Spore Spawn's spore palette.</summary>
    public static readonly SpriteColourRun SporeSpawnSpores = new(7, 0, 16);

    /// <summary>Spore Spawn's body palette.</summary>
    public static readonly SpriteColourRun SporeSpawnBody = new(1, 0, 16);

    /// <summary>The room's level colours Spore Spawn's death fades.</summary>
    public static readonly BackgroundColourRun SporeSpawnDeathLevel = new(4, 0, 16);

    /// <summary>The room's background colours Spore Spawn's death fades.</summary>
    public static readonly BackgroundColourRun SporeSpawnDeathBackground = new(7, 0, 16);
}
