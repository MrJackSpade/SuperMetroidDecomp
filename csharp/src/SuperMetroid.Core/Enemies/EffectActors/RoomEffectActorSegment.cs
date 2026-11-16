namespace SuperMetroid.Core.Enemies.EffectActors;

/// <summary>
/// A run of beats a room effect actor's animation plays whole (#2276): each kind's own, and the cycle a Metroid's
/// electricity and shell loop after their opening. Their beats are <see cref="RoomEffectActorAnimations"/>; their values
/// are arbitrary.
/// </summary>
internal enum RoomEffectActorSegment
{
    /// <summary>The beats <see cref="RoomEffectActorKind.DraygonFoam"/> shows.</summary>
    DraygonFoam,
    /// <summary>The beats <see cref="RoomEffectActorKind.SmallExplosion"/> shows.</summary>
    SmallExplosion,
    /// <summary>The beats <see cref="RoomEffectActorKind.DudShot"/> shows.</summary>
    DudShot,
    /// <summary>The beats <see cref="RoomEffectActorKind.SmokePuff"/> shows.</summary>
    SmokePuff,
    /// <summary>The beats <see cref="RoomEffectActorKind.RisingDustPuff"/> shows.</summary>
    RisingDustPuff,
    /// <summary>The beats <see cref="RoomEffectActorKind.Smoke"/> shows.</summary>
    Smoke,
    /// <summary>The beats <see cref="RoomEffectActorKind.BigExplosion"/> shows.</summary>
    BigExplosion,
    /// <summary>The beats <see cref="RoomEffectActorKind.ShortBigDustCloud"/> shows.</summary>
    ShortBigDustCloud,
    /// <summary>The beats <see cref="RoomEffectActorKind.BigDustCloud"/> shows.</summary>
    BigDustCloud,
    /// <summary>The beats <see cref="RoomEffectActorKind.DraygonBreathBubbles"/> shows.</summary>
    DraygonBreathBubbles,
    /// <summary>The beats <see cref="RoomEffectActorKind.NuclearWaffleBody"/> shows.</summary>
    NuclearWaffleBody,
    /// <summary>The beats <see cref="RoomEffectActorKind.NuclearWaffleRightExplosion"/> shows.</summary>
    NuclearWaffleRightExplosion,
    /// <summary>The beats <see cref="RoomEffectActorKind.NuclearWaffleLeftExplosion"/> shows.</summary>
    NuclearWaffleLeftExplosion,
    /// <summary>The beats <see cref="RoomEffectActorKind.NuclearWaffleSplash"/> shows.</summary>
    NuclearWaffleSplash,
    /// <summary>The beats <see cref="RoomEffectActorKind.FallingSparkTrail"/> shows.</summary>
    FallingSparkTrail,
    /// <summary>The beats <see cref="RoomEffectActorKind.MetroidElectricity"/> shows, its opening before its cycle.</summary>
    MetroidElectricity,
    /// <summary>The cycle <see cref="RoomEffectActorKind.MetroidElectricity"/> loops after its opening.</summary>
    MetroidElectricityCycle,
    /// <summary>The beats <see cref="RoomEffectActorKind.MetroidShell"/> shows, its opening before its cycle.</summary>
    MetroidShell,
    /// <summary>The cycle <see cref="RoomEffectActorKind.MetroidShell"/> loops after its opening.</summary>
    MetroidShellCycle,
    /// <summary>The beats <see cref="RoomEffectActorKind.YappingMawBaseFacingDown"/> shows.</summary>
    YappingMawBaseFacingDown,
    /// <summary>The beats <see cref="RoomEffectActorKind.YappingMawBaseFacingUp"/> shows.</summary>
    YappingMawBaseFacingUp,
    /// <summary>The beats <see cref="RoomEffectActorKind.EvirFacingLeft"/> shows.</summary>
    EvirFacingLeft,
    /// <summary>The beats <see cref="RoomEffectActorKind.EvirFacingRight"/> shows.</summary>
    EvirFacingRight,
    /// <summary>The beats <see cref="RoomEffectActorKind.HitBurst"/> shows.</summary>
    HitBurst,
}
