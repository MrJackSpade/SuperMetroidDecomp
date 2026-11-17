namespace SuperMetroid.Core.Enemies.EffectActors;

/// <summary>
/// A run of a room effect actor's animation (#2275): each kind's own run, and the cycles a Metroid's electricity and shell
/// loop after their first pass. Their beats are <see cref="RoomEffectActorAnimations"/>; its values are arbitrary.
/// </summary>
internal enum RoomEffectActorSegment
{
    /// <summary>A small explosion: a spark swelling into a fireball, a ring and scattering embers.</summary>
    SmallExplosion,
    /// <summary>Smoke rising and fading.</summary>
    Smoke,
    /// <summary>A dud shot's flash: a dot opening into a ring that thins and fades.</summary>
    DudShot,
    /// <summary>A puff of dust rising.</summary>
    RisingDustPuff,
    /// <summary>A small puff of smoke rising and fading.</summary>
    SmokePuff,
    /// <summary>A big explosion: a fireball opening into a wide ring that fades.</summary>
    BigExplosion,
    /// <summary>A big dust cloud's first half: a puff swelling until it billows.</summary>
    ShortBigDustCloud,
    /// <summary>A big dust cloud: a puff swelling, billowing, thinning and fading into wisps.</summary>
    BigDustCloud,
    /// <summary>Draygon's breath bubbles rising.</summary>
    DraygonBreathBubbles,
    /// <summary>A Nuclear Waffle body link turning round, looping, placed by the Nuclear Waffle that owns it.</summary>
    NuclearWaffleBody,
    /// <summary>A Nuclear Waffle's explosion bursting to the right.</summary>
    NuclearWaffleRightExplosion,
    /// <summary>A Nuclear Waffle's explosion bursting to the left.</summary>
    NuclearWaffleLeftExplosion,
    /// <summary>A Nuclear Waffle's splash, which shows beside either explosion.</summary>
    NuclearWaffleSplash,
    /// <summary>A falling spark's fading afterimage.</summary>
    FallingSparkTrail,
    /// <summary>A Metroid's electricity's first pass.</summary>
    MetroidElectricity,
    /// <summary>A Metroid's electricity's cycle, which loops after its first pass.</summary>
    MetroidElectricityCycle,
    /// <summary>A Metroid's shell's first pass.</summary>
    MetroidShell,
    /// <summary>A Metroid's shell's cycle, which loops after its first pass.</summary>
    MetroidShellCycle,
    /// <summary>The base of a Yapping Maw that reaches down, held eight pixels above it.</summary>
    YappingMawBaseFacingDown,
    /// <summary>The base of a Yapping Maw that reaches up, held eight pixels below it.</summary>
    YappingMawBaseFacingUp,
    /// <summary>An Evir facing left, its legs cycling.</summary>
    EvirFacingLeft,
    /// <summary>An Evir facing right, its legs cycling.</summary>
    EvirFacingRight,
    /// <summary>Foam rising from Draygon's mouth.</summary>
    DraygonFoam,
    /// <summary>The hit burst on an enemy a shot or bomb hurts while it is not frozen.</summary>
    HitBurst,
}
