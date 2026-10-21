using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.SporeSpawn;
namespace SuperMetroid.Core.Bosses.Artwork;

/// <summary>Typed view of the other boss and set-piece palette ranges (#630).</summary>
[PpuLayoutBoundary("rendering view: boss and set-piece palettes")]
public readonly struct BossPalettes(IPaletteColors cgram)
{
    /// <summary>Draygon opening colours.</summary>
    public CgramPalette DraygonIntro => new(cgram, BossCgramLayout.DraygonIntro);
    /// <summary>Draygon background material.</summary>
    public CgramPalette DraygonBackground => new(cgram, BossCgramLayout.DraygonBackground);
    /// <summary>Draygon sprite material.</summary>
    public CgramPalette DraygonSprite => new(cgram, BossCgramLayout.DraygonSprite);
    /// <summary>Draygon health band.</summary>
    public CgramPalette DraygonHealth => new(cgram, BossCgramLayout.DraygonHealth);
    /// <summary>Crocomire fight body.</summary>
    public CgramPalette CrocomireFightBody => new(cgram, BossCgramLayout.CrocomireFightBody);
    /// <summary>Crocomire initial wall.</summary>
    public CgramPalette CrocomireInitialWall => new(cgram, BossCgramLayout.CrocomireInitialWall);
    /// <summary>Crocomire initial projectile.</summary>
    public CgramPalette CrocomireInitialProjectile => new(cgram, BossCgramLayout.CrocomireInitialProjectile);
    /// <summary>Crocomire skeleton arm.</summary>
    public CgramPalette CrocomireSkeletonArm => new(cgram, BossCgramLayout.CrocomireSkeletonArm);
    /// <summary>Crocomire wall spikes.</summary>
    public CgramPalette CrocomireWallSpikes => new(cgram, BossCgramLayout.CrocomireWallSpikes);

    /// <summary>Torizo body row.</summary>
    public CgramPalette TorizoBody => new(cgram, BossCgramLayout.TorizoBody);
    /// <summary>Torizo belly row.</summary>
    public CgramPalette TorizoBelly => new(cgram, BossCgramLayout.TorizoBelly);
    /// <summary>First shared Torizo row.</summary>
    public CgramPalette TorizoSharedFirst => new(cgram, BossCgramLayout.TorizoSharedFirst);
    /// <summary>Second shared Torizo row.</summary>
    public CgramPalette TorizoSharedSecond => new(cgram, BossCgramLayout.TorizoSharedSecond);
    /// <summary>Botwoon's body palette.</summary>
    public CgramPalette BotwoonBody => new(cgram, BossCgramLayout.BotwoonBody);
    /// <summary>OBJ palette 7 colours cleared at Ceres Ridley start.</summary>
    public CgramPalette CeresRidleyRevealedObjects => new(cgram, BossCgramLayout.CeresRidleyRevealedObjects);
    /// <summary>The cutscene Baby Metroid palette.</summary>
    public CgramPalette BabyMetroidCutscene => new(cgram, BossCgramLayout.BabyMetroidCutscene);
    /// <summary>Kraid's room backdrop row.</summary>
    public CgramPalette KraidRoomBackdrop => new(cgram, BossCgramLayout.KraidRoomBackdrop);
    /// <summary>Kraid's BG2 body row.</summary>
    public CgramPalette KraidBody => new(cgram, BossCgramLayout.KraidBody);
    /// <summary>Kraid's eye colours.</summary>
    public CgramPalette KraidEye => new(cgram, BossCgramLayout.KraidEye);
    /// <summary>Kraid's initial sprite row.</summary>
    public CgramPalette KraidSprites => new(cgram, BossCgramLayout.KraidSprites);
    /// <summary>Kraid's secondary row.</summary>
    public CgramPalette KraidSecondary => new(cgram, BossCgramLayout.KraidSecondary);
    /// <summary>Mother Brain's body colours.</summary>
    public CgramPalette MotherBrainBody => new(cgram, BossCgramLayout.MotherBrainBody);
    /// <summary>Mother Brain's brain colours.</summary>
    public CgramPalette MotherBrainBrain => new(cgram, BossCgramLayout.MotherBrainBrain);
    /// <summary>Mother Brain's leg colours.</summary>
    public CgramPalette MotherBrainLegs => new(cgram, BossCgramLayout.MotherBrainLegs);
    /// <summary>Mother Brain's corpse colours.</summary>
    public CgramPalette MotherBrainCorpse => new(cgram, BossCgramLayout.MotherBrainCorpse);
    /// <summary>Phantoon's body palette.</summary>
    public CgramPalette PhantoonBody => new(cgram, BossCgramLayout.PhantoonBody);
    /// <summary>Norfair Ridley's body colours.</summary>
    public CgramPalette NorfairRidleyBody => new(cgram, BossCgramLayout.NorfairRidleyBody);
    /// <summary>Norfair Ridley's tail and wing colours.</summary>
    public CgramPalette NorfairRidleyTail => new(cgram, BossCgramLayout.NorfairRidleyTail);
    /// <summary>The Shitroid's cycling membrane colours.</summary>
    public CgramPalette ShitroidMembrane => new(cgram, BossCgramLayout.ShitroidMembrane);
    /// <summary>Spore Spawn's spore palette.</summary>
    public CgramPalette SporeSpawnSpores => new(cgram, BossCgramLayout.SporeSpawnSpores);
    /// <summary>Spore Spawn's body palette.</summary>
    public CgramPalette SporeSpawnBody => new(cgram, BossCgramLayout.SporeSpawnBody);
    /// <summary>The Shitroid encounter's target row for <paramref name="target"/>.</summary>
    public CgramPalette ShitroidTarget(ShitroidColorTarget target) => new(cgram, target switch
    {
        ShitroidColorTarget.Sidehopper => BossCgramLayout.ShitroidSidehopper,
        ShitroidColorTarget.Shitroid => BossCgramLayout.ShitroidBody,
        ShitroidColorTarget.DeadSidehopper => BossCgramLayout.ShitroidDeadSidehopper,
        _ => throw new ArgumentOutOfRangeException(nameof(target), target, "Undefined Shitroid colour target."),
    });
    /// <summary>The row Spore Spawn's death sequence fades for <paramref name="layer"/>.</summary>
    public CgramPalette SporeSpawnDeath(SporeSpawnDeathPaletteLayer layer) => new(cgram, layer switch
    {
        SporeSpawnDeathPaletteLayer.Sprite => BossCgramLayout.SporeSpawnBody,
        SporeSpawnDeathPaletteLayer.Level => BossCgramLayout.SporeSpawnDeathLevel,
        SporeSpawnDeathPaletteLayer.Background => BossCgramLayout.SporeSpawnDeathBackground,
        _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, "Undefined Spore Spawn death palette layer."),
    });
}
