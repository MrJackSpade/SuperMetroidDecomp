namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The reactions of the bosses' and the areas' own enemies that their definitions name (#2558), declared beside the
/// definitions so the definitions reach no boss or area: each species' reaction class names the same reaction, and its
/// entity answers it.
/// </summary>
internal static partial class SpeciesOwnReactions
{
    /// <summary>The reaction of the Baby Metroid's own to Samus's touch in Mother Brain's cutscene, which its entity answers.</summary>
    internal static EnemyReaction BabyMetroidCutsceneTouch { get; } = EnemyReaction.OfSpecies("BabyMetroid.CutsceneTouch");

    /// <summary>The reaction of the Baby Metroid's own to her shots and bombs, which its entity answers.</summary>
    internal static EnemyReaction BabyMetroidShot { get; } = EnemyReaction.OfSpecies("BabyMetroid.Shot");

    /// <summary>The reaction of the Baby Metroid's own to Samus's touch, which its entity answers.</summary>
    internal static EnemyReaction BabyMetroidTouch { get; } = EnemyReaction.OfSpecies("BabyMetroid.Touch");

    /// <summary>The reaction of Botwoon's own to her shots and bombs, which its entity answers.</summary>
    internal static EnemyReaction BotwoonShot { get; } = EnemyReaction.OfSpecies("Botwoon.Shot");

    /// <summary>The reaction of Botwoon's own to Samus's touch, which its entity answers.</summary>
    internal static EnemyReaction BotwoonTouch { get; } = EnemyReaction.OfSpecies("Botwoon.Touch");

    /// <summary>The steam's touch reaction, which every visible steam frame's hitbox names.</summary>
    internal static EnemyReaction CeresSteamTouch { get; } = EnemyReaction.OfSpecies("CeresSteam.Touch");

    /// <summary>
    /// The shot reaction his definition names (#2558): his parts collide by their frames' hitboxes, whose reactions
    /// run, so a shot or bomb that meets him by his radius has no response of his.
    /// </summary>
    internal static EnemyReaction CrocomireShot { get; } = EnemyReaction.OfSpecies("Crocomire.Shot");

    /// <summary>Draygon's shot reaction.</summary>
    internal static EnemyReaction DraygonShot { get; } = EnemyReaction.OfSpecies("Draygon.Shot");

    /// <summary>Draygon's touch reaction.</summary>
    internal static EnemyReaction DraygonTouch { get; } = EnemyReaction.OfSpecies("Draygon.Touch");

    /// <summary>The arm's touch reaction, which pushes Samus away.</summary>
    internal static EnemyReaction KraidArmTouch { get; } = EnemyReaction.OfSpecies("Kraid.ArmTouch");

    /// <summary>The touch reaction of Kraid's body and feet: the hitboxes his arm's part and his foot hold over his body (#2221).</summary>
    internal static EnemyReaction KraidBodyTouch { get; } = EnemyReaction.OfSpecies("Kraid.BodyTouch");

    /// <summary>Fake Kraid's own shot reaction, which his definition names (#2558).</summary>
    internal static EnemyReaction KraidFakeKraidShot { get; } = EnemyReaction.OfSpecies("FakeKraid.Shot");

    /// <summary>Fake Kraid's own touch reaction, which his definition names (#2558).</summary>
    internal static EnemyReaction KraidFakeKraidTouch { get; } = EnemyReaction.OfSpecies("FakeKraid.Touch");

    /// <summary>The touch reaction of his nails' own, which their definitions name (#2558).</summary>
    internal static EnemyReaction KraidNailTouch { get; } = EnemyReaction.OfSpecies("Kraid.NailTouch");

    /// <summary>The body's shot reaction.</summary>
    internal static EnemyReaction MotherBrainBodyShot { get; } = EnemyReaction.OfSpecies("MotherBrain.BodyShot");

    /// <summary>The head's shot reaction.</summary>
    internal static EnemyReaction MotherBrainHeadShot { get; } = EnemyReaction.OfSpecies("MotherBrain.HeadShot");

    /// <summary>The head's touch reaction.</summary>
    internal static EnemyReaction MotherBrainHeadTouch { get; } = EnemyReaction.OfSpecies("MotherBrain.HeadTouch");

    /// <summary>Phantoon's body shot reaction.</summary>
    internal static EnemyReaction PhantoonShot { get; } = EnemyReaction.OfSpecies("Phantoon.Shot");

    /// <summary>Phantoon's body touch reaction.</summary>
    internal static EnemyReaction PhantoonTouch { get; } = EnemyReaction.OfSpecies("Phantoon.Touch");

    /// <summary>Ridley's shot reaction.</summary>
    internal static EnemyReaction RidleyShot { get; } = EnemyReaction.OfSpecies("Ridley.Shot");

    /// <summary>The reaction of a Rinka's own to her shots and bombs, which its entity answers.</summary>
    internal static EnemyReaction RinkaShot { get; } = EnemyReaction.OfSpecies("Rinka.Shot");

    /// <summary>The reaction of a Rinka's own to Samus's touch, which its entity answers.</summary>
    internal static EnemyReaction RinkaTouch { get; } = EnemyReaction.OfSpecies("Rinka.Touch");

    /// <summary>Spore Spawn's shot reaction.</summary>
    internal static EnemyReaction SporeSpawnShot { get; } = EnemyReaction.OfSpecies("SporeSpawn.Shot");

    /// <summary>Spore Spawn's touch reaction.</summary>
    internal static EnemyReaction SporeSpawnTouch { get; } = EnemyReaction.OfSpecies("SporeSpawn.Touch");

    /// <summary>Golden Torizo's own shot reaction, which its contact takes by its radius.</summary>
    internal static EnemyReaction TorizoGoldenShot { get; } = EnemyReaction.OfSpecies("Torizo.GoldenShot");

    /// <summary>Bomb Torizo's shot reaction, which Golden Torizo's ordinary body hitboxes also name.</summary>
    internal static EnemyReaction TorizoShot { get; } = EnemyReaction.OfSpecies("Torizo.Shot");

    /// <summary>The Torizos' touch reaction.</summary>
    internal static EnemyReaction TorizoTouch { get; } = EnemyReaction.OfSpecies("Torizo.Touch");

    /// <summary>The reaction of a Zebetite's own to her shots and bombs, which its entity answers.</summary>
    internal static EnemyReaction ZebetiteShot { get; } = EnemyReaction.OfSpecies("Zebetite.Shot");

    /// <summary>The reaction of a Zebetite's own to Samus's touch, which its entity answers.</summary>
    internal static EnemyReaction ZebetiteTouch { get; } = EnemyReaction.OfSpecies("Zebetite.Touch");
}
