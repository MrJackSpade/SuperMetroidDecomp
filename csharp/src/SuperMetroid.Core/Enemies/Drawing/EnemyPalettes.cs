using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.Core.Enemies.Drawing;

/// <summary>Typed view of the enemy and boss CGRAM ranges (#630).</summary>
[PpuLayoutBoundary("rendering view: enemy and boss CGRAM ranges")]
public readonly struct EnemyPalettes(SnesCgram cgram)
{
    /// <summary>Torizo body row.</summary>
    public CgramPalette TorizoBody => new(cgram, EnemyPaletteLayout.TorizoBody);

    /// <summary>The ninja Space Pirate's gold palette row.</summary>
    public CgramPalette NinjaSpacePirate => new(cgram, EnemyPaletteLayout.NinjaSpacePirate);
    /// <summary>Torizo belly row.</summary>
    public CgramPalette TorizoBelly => new(cgram, EnemyPaletteLayout.TorizoBelly);
    /// <summary>First shared Torizo row.</summary>
    public CgramPalette TorizoSharedFirst => new(cgram, EnemyPaletteLayout.TorizoSharedFirst);
    /// <summary>Second shared Torizo row.</summary>
    public CgramPalette TorizoSharedSecond => new(cgram, EnemyPaletteLayout.TorizoSharedSecond);
    /// <summary>Botwoon's body palette.</summary>
    public CgramPalette BotwoonBody => new(cgram, EnemyPaletteLayout.BotwoonBody);
    /// <summary>OBJ palette 7 colours cleared at Ceres Ridley start.</summary>
    public CgramPalette CeresRidleyRevealedObjects => new(cgram, EnemyPaletteLayout.CeresRidleyRevealedObjects);
    /// <summary>The cutscene Baby Metroid palette.</summary>
    public CgramPalette BabyMetroidCutscene => new(cgram, EnemyPaletteLayout.BabyMetroidCutscene);
    /// <summary>Crocomire's fight-body colours.</summary>
    public CgramPalette CrocomireFightBody => new(cgram, SuperMetroid.Core.Bosses.Artwork.BossCgramLayout.CrocomireFightBody);
    /// <summary>The dead sidehopper's palette.</summary>
    public CgramPalette DeadSidehopper => new(cgram, EnemyPaletteLayout.DeadSidehopper);
    /// <summary>Kraid's room backdrop row.</summary>
    public CgramPalette KraidRoomBackdrop => new(cgram, EnemyPaletteLayout.KraidRoomBackdrop);
    /// <summary>Kraid's BG2 body row.</summary>
    public CgramPalette KraidBody => new(cgram, EnemyPaletteLayout.KraidBody);
    /// <summary>Kraid's eye colours.</summary>
    public CgramPalette KraidEye => new(cgram, EnemyPaletteLayout.KraidEye);
    /// <summary>Kraid's initial sprite row.</summary>
    public CgramPalette KraidSprites => new(cgram, EnemyPaletteLayout.KraidSprites);
    /// <summary>Kraid's secondary row.</summary>
    public CgramPalette KraidSecondary => new(cgram, EnemyPaletteLayout.KraidSecondary);
    /// <summary>Mother Brain's body colours.</summary>
    public CgramPalette MotherBrainBody => new(cgram, EnemyPaletteLayout.MotherBrainBody);
    /// <summary>Mother Brain's brain colours.</summary>
    public CgramPalette MotherBrainBrain => new(cgram, EnemyPaletteLayout.MotherBrainBrain);
    /// <summary>Mother Brain's leg colours.</summary>
    public CgramPalette MotherBrainLegs => new(cgram, EnemyPaletteLayout.MotherBrainLegs);
    /// <summary>Mother Brain's corpse colours.</summary>
    public CgramPalette MotherBrainCorpse => new(cgram, EnemyPaletteLayout.MotherBrainCorpse);
    /// <summary>The escape typewriter's source colours.</summary>
    public CgramPalette EscapeTypewriterSource => new(cgram, EnemyPaletteLayout.EscapeTypewriterSource);
    /// <summary>The escape typewriter's text colours.</summary>
    public CgramPalette EscapeTypewriterText => new(cgram, EnemyPaletteLayout.EscapeTypewriterText);
    /// <summary>Phantoon's body palette.</summary>
    public CgramPalette PhantoonBody => new(cgram, EnemyPaletteLayout.PhantoonBody);
    /// <summary>The Wrecked Ship power-on room colours.</summary>
    public CgramPalette WreckedShipPowerOn => new(cgram, EnemyPaletteLayout.WreckedShipPowerOn);
    /// <summary>Norfair Ridley's body colours.</summary>
    public CgramPalette NorfairRidleyBody => new(cgram, EnemyPaletteLayout.NorfairRidleyBody);
    /// <summary>Norfair Ridley's tail and wing colours.</summary>
    public CgramPalette NorfairRidleyTail => new(cgram, EnemyPaletteLayout.NorfairRidleyTail);
    /// <summary>The Shitroid's cycling membrane colours.</summary>
    public CgramPalette ShitroidMembrane => new(cgram, EnemyPaletteLayout.ShitroidMembrane);
    /// <summary>Spore Spawn's spore palette.</summary>
    public CgramPalette SporeSpawnSpores => new(cgram, EnemyPaletteLayout.SporeSpawnSpores);
    /// <summary>Spore Spawn's body palette.</summary>
    public CgramPalette SporeSpawnBody => new(cgram, EnemyPaletteLayout.SporeSpawnBody);

    /// <summary>The Shitroid encounter's target row for <paramref name="target"/>.</summary>
    public CgramPalette ShitroidTarget(ShitroidColorTarget target) => new(cgram, target switch
    {
        ShitroidColorTarget.Sidehopper => EnemyPaletteLayout.ShitroidSidehopper,
        ShitroidColorTarget.Shitroid => EnemyPaletteLayout.ShitroidBody,
        ShitroidColorTarget.DeadSidehopper => EnemyPaletteLayout.ShitroidDeadSidehopper,
        _ => throw new ArgumentOutOfRangeException(nameof(target), target, "Undefined Shitroid colour target."),
    });

    /// <summary>The row Spore Spawn's death sequence fades for <paramref name="layer"/>.</summary>
    public CgramPalette SporeSpawnDeath(SporeSpawnDeathPaletteLayer layer) => new(cgram, layer switch
    {
        SporeSpawnDeathPaletteLayer.Sprite => EnemyPaletteLayout.SporeSpawnBody,
        SporeSpawnDeathPaletteLayer.Level => EnemyPaletteLayout.SporeSpawnDeathLevel,
        SporeSpawnDeathPaletteLayer.Background => EnemyPaletteLayout.SporeSpawnDeathBackground,
        _ => throw new ArgumentOutOfRangeException(nameof(layer), layer, "Undefined Spore Spawn death palette layer."),
    });
}
