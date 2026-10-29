using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Enemies.Corpses.Artwork;
namespace SuperMetroid.Core.Bosses.Artwork;

/// <summary>Typed view of the other bosses' and set pieces' colours (#630, #1405): runs of gameplay's background and sprite colour sets.</summary>
public readonly struct BossColours(GameplayColours colours)
{
    /// <summary>Draygon opening colours.</summary>
    public ColourRun DraygonIntro => BossColourDefinitions.DraygonIntro.In(colours.SpriteSets);
    /// <summary>Draygon background material.</summary>
    public ColourRun DraygonBackground => BossColourDefinitions.DraygonBackground.In(colours.BackgroundSets);
    /// <summary>Draygon sprite material.</summary>
    public ColourRun DraygonSprite => BossColourDefinitions.DraygonSprite.In(colours.SpriteSets);
    /// <summary>Draygon health band.</summary>
    public ColourRun DraygonHealth => BossColourDefinitions.DraygonHealth.In(colours.BackgroundSets);
    /// <summary>Crocomire fight body.</summary>
    public ColourRun CrocomireFightBody => BossColourDefinitions.CrocomireFightBody.In(colours.BackgroundSets);
    /// <summary>Crocomire initial wall.</summary>
    public ColourRun CrocomireInitialWall => BossColourDefinitions.CrocomireInitialWall.In(colours.SpriteSets);
    /// <summary>Crocomire initial projectile.</summary>
    public ColourRun CrocomireInitialProjectile => BossColourDefinitions.CrocomireInitialProjectile.In(colours.SpriteSets);
    /// <summary>Crocomire skeleton arm.</summary>
    public ColourRun CrocomireSkeletonArm => BossColourDefinitions.CrocomireSkeletonArm.In(colours.SpriteSets);
    /// <summary>Crocomire wall spikes.</summary>
    public ColourRun CrocomireWallSpikes => BossColourDefinitions.CrocomireWallSpikes.In(colours.SpriteSets);

    /// <summary>Torizo body row.</summary>
    public ColourRun TorizoBody => BossColourDefinitions.TorizoBody.In(colours.SpriteSets);
    /// <summary>Torizo belly row.</summary>
    public ColourRun TorizoBelly => BossColourDefinitions.TorizoBelly.In(colours.SpriteSets);
    /// <summary>First shared Torizo row.</summary>
    public ColourRun TorizoSharedFirst => BossColourDefinitions.TorizoSharedFirst.In(colours.SpriteSets);
    /// <summary>Second shared Torizo row.</summary>
    public ColourRun TorizoSharedSecond => BossColourDefinitions.TorizoSharedSecond.In(colours.SpriteSets);
    /// <summary>Botwoon's body palette.</summary>
    public ColourRun BotwoonBody => BossColourDefinitions.BotwoonBody.In(colours.SpriteSets);
    /// <summary>OBJ palette 7 colours cleared at Ceres Ridley start.</summary>
    public ColourRun CeresRidleyRevealedObjects => BossColourDefinitions.CeresRidleyRevealedObjects.In(colours.SpriteSets);
    /// <summary>The cutscene Baby Metroid palette.</summary>
    public ColourRun BabyMetroidCutscene => BossColourDefinitions.BabyMetroidCutscene.In(colours.SpriteSets);
    /// <summary>Kraid's room backdrop row.</summary>
    public ColourRun KraidRoomBackdrop => BossColourDefinitions.KraidRoomBackdrop.In(colours.BackgroundSets);
    /// <summary>Kraid's BG2 body row.</summary>
    public ColourRun KraidBody => BossColourDefinitions.KraidBody.In(colours.BackgroundSets);
    /// <summary>Kraid's eye colours.</summary>
    public ColourRun KraidEye => BossColourDefinitions.KraidEye.In(colours.BackgroundSets);
    /// <summary>Kraid's initial sprite row.</summary>
    public ColourRun KraidSprites => BossColourDefinitions.KraidSprites.In(colours.SpriteSets);
    /// <summary>Kraid's secondary row.</summary>
    public ColourRun KraidSecondary => BossColourDefinitions.KraidSecondary.In(colours.SpriteSets);
    /// <summary>Mother Brain's body colours.</summary>
    public ColourRun MotherBrainBody => BossColourDefinitions.MotherBrainBody.In(colours.BackgroundSets);
    /// <summary>Mother Brain's brain colours.</summary>
    public ColourRun MotherBrainBrain => BossColourDefinitions.MotherBrainBrain.In(colours.SpriteSets);
    /// <summary>Mother Brain's leg colours.</summary>
    public ColourRun MotherBrainLegs => BossColourDefinitions.MotherBrainLegs.In(colours.SpriteSets);
    /// <summary>Mother Brain's corpse colours.</summary>
    public ColourRun MotherBrainCorpse => BossColourDefinitions.MotherBrainCorpse.In(colours.SpriteSets);
    /// <summary>Phantoon's body palette.</summary>
    public ColourRun PhantoonBody => BossColourDefinitions.PhantoonBody.In(colours.BackgroundSets);
    /// <summary>Norfair Ridley's body colours.</summary>
    public ColourRun NorfairRidleyBody => BossColourDefinitions.NorfairRidleyBody.In(colours.BackgroundSets);
    /// <summary>Norfair Ridley's tail and wing colours.</summary>
    public ColourRun NorfairRidleyTail => BossColourDefinitions.NorfairRidleyTail.In(colours.SpriteSets);
    /// <summary>The Shitroid's cycling membrane colours.</summary>
    public ColourRun ShitroidMembrane => BossColourDefinitions.ShitroidMembrane.In(colours.SpriteSets);
    /// <summary>Spore Spawn's spore palette.</summary>
    public ColourRun SporeSpawnSpores => BossColourDefinitions.SporeSpawnSpores.In(colours.SpriteSets);
    /// <summary>Spore Spawn's body palette.</summary>
    public ColourRun SporeSpawnBody => BossColourDefinitions.SporeSpawnBody.In(colours.SpriteSets);
    /// <summary>The Shitroid encounter's target row for <paramref name="target"/>.</summary>
    public ColourRun ShitroidTarget(ShitroidColorTarget target) => target switch
    {
        ShitroidColorTarget.Sidehopper => BossColourDefinitions.ShitroidSidehopper.In(colours.SpriteSets),
        ShitroidColorTarget.Shitroid => BossColourDefinitions.ShitroidBody.In(colours.SpriteSets),
        ShitroidColorTarget.DeadSidehopper => TourianCorpseColourDefinitions.DrainedCorpses.In(colours.SpriteSets),
        _ => throw new ArgumentOutOfRangeException(nameof(target), target, "Undefined Shitroid colour target."),
    };
    /// <summary>The row Spore Spawn's death sequence fades for <paramref name="layer"/>.</summary>
    public ColourRun SporeSpawnDeath(SporeSpawnDeathPaletteLayer layer) => layer switch
    {
        SporeSpawnDeathPaletteLayer.Sprite => BossColourDefinitions.SporeSpawnBody.In(colours.SpriteSets),
        SporeSpawnDeathPaletteLayer.Level => BossColourDefinitions.SporeSpawnDeathLevel.In(colours.BackgroundSets),
        SporeSpawnDeathPaletteLayer.Background => BossColourDefinitions.SporeSpawnDeathBackground.In(colours.BackgroundSets),
        _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, "Undefined Spore Spawn death palette layer."),
    };
}
