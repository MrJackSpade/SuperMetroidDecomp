namespace SuperMetroid.Core.Enemies.EffectActors;

/// <summary>
/// A room effect actor, named for what it shows: each starts its own animation of
/// <see cref="RoomEffectActorAnimations"/>, whatever spawns it. Its values are arbitrary.
/// </summary>
public enum RoomEffectActorKind
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
    /// <summary>A Metroid's electricity crackling round its body, looping after its first pass, placed by the Metroid.</summary>
    MetroidElectricity,
    /// <summary>A Metroid's shell, looping after its first pass, paired with its electricity.</summary>
    MetroidShell,
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
