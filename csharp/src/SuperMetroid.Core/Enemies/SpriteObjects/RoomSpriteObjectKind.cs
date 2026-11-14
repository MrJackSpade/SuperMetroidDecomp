namespace SuperMetroid.Core.Enemies.SpriteObjects;

/// <summary>
/// The room sprite object an owner spawns. Its values are arbitrary; verification's catalog names each kind's
/// object number.
/// </summary>
public enum RoomSpriteObjectKind
{
    /// <summary>Finite small explosion used by Spore Spawn, Draygon, and escape effects.</summary>
    SporeSpawnDyingExplosion,
    /// <summary>The same small explosion, as the generic enemy-death explosions spawn it.</summary>
    SmallExplosion = SporeSpawnDyingExplosion,
    /// <summary>Finite smoke the generic enemy-death explosions scatter.</summary>
    Smoke,
    /// <summary>Finite impact flash for shots rejected by enemies' collision responses.</summary>
    EnemyProjectileDud,
    /// <summary>Finite dust animation spawned at a Ninja Pirate's landing.</summary>
    NinjaPirateLandingDust,
    /// <summary>Small finite flash used as one of Botwoon's death-explosion variants.</summary>
    BotwoonSmallExplosion,
    /// <summary>Large finite explosion used by Botwoon's death sequence.</summary>
    BotwoonLargeExplosion,
    /// <summary>Finite cloud among the escape's explosions.</summary>
    ShortBigDustCloud,
    /// <summary>Finite cloud shared by impacts, burial smoke, and acid effects.</summary>
    DustCloud,
    /// <summary>The same big dust cloud, used for Crocomire's acid smoke.</summary>
    CrocomireAcidSmoke = DustCloud,
    /// <summary>Finite breath bubbles emitted during Draygon's flight.</summary>
    DraygonBreathBubble,
    /// <summary>Looping Nuclear Waffle body-segment art, positioned by the owning enemy.</summary>
    NuclearWaffleBody,
    /// <summary>Finite right-turn burst selected for Nuclear Waffle turn variant zero.</summary>
    NuclearWaffleTurnClockwise,
    /// <summary>Finite left-turn burst selected for the other Nuclear Waffle turn variant.</summary>
    NuclearWaffleTurnCounterClockwise,
    /// <summary>Finite splash overlay spawned alongside the directional Nuclear Waffle turn burst.</summary>
    NuclearWaffleTurnOverlay,
    /// <summary>Finite afterimage left behind by a falling spark.</summary>
    FallingSparkTrail,
    /// <summary>Looping electrical outer-body art, allocated and repositioned by the Metroid owner.</summary>
    MetroidOuterBodyA,
    /// <summary>Looping shell layer paired with the Metroid's electrical sprite object.</summary>
    MetroidOuterBodyB,
    /// <summary>Held root art placed eight pixels above a Yapping Maw that reaches down.</summary>
    YappingMawRootVariantZero,
    /// <summary>Held root art placed eight pixels below a Yapping Maw that reaches up.</summary>
    YappingMawRootVariantOne,
    /// <summary>Looping left-facing Evir art used for both Draygon's opening dance and burial.</summary>
    DraygonIntroEvir,
    /// <summary>Looping right-facing Evir art moved by Draygon's burial sequence.</summary>
    DraygonDeathEvirFacingRight,
    /// <summary>Finite mouth-foam animation emitted during the carry spiral and fatal drift.</summary>
    DraygonSpiralFoam,
    /// <summary>Finite burst centred on an enemy a shot or bomb hurts while it is not frozen.</summary>
    EnemyHit,
}
