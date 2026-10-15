using SuperMetroid.Core.Bosses.Phantoon;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Bank-qualified enemy interaction callbacks (touch, shot, Power Bomb and extended-hitbox
/// reactions) that the port names, valued by native long address. The shared prologue
/// entries every enemy bank repeats at the same offset are valued at their bank-$A0 copy;
/// <see cref="EnemyInteractionCallbacks.TryDecode"/> admits them from any enemy bank.
/// </summary>
internal enum EnemyInteractionCallback
{
    /// <summary><c>NormalEnemyTouchAI</c> at $A0:8023.</summary>
    NormalEnemyTouch = 0xa00000 | EnemyAiCodePointers.BankA0.NormalEnemyTouch,
    /// <summary><c>NormalEnemyShotAI</c> at $A0:802D.</summary>
    NormalEnemyShot = 0xa00000 | EnemyAiCodePointers.BankA0.NormalEnemyShot,
    /// <summary>Shared dud-shot callback at $A0:8046.</summary>
    DudShot = 0xa00000 | EnemyAiCodePointers.BankA0.DudShot,
    /// <summary>Shared no-op interaction callback at $A0:804C.</summary>
    NoOp = 0xa00000 | EnemyAiCodePointers.BankA0.NoOp,
    /// <summary>RTS_A0804B at $A0:804B, the shared short-return no-op identity rejected by collision dispatch.</summary>
    NoOpShortReturn = 0xa00000 | EnemyAiCodePointers.BankA0.NoOpShortReturn,
    /// <summary>Owtch shot callback at $A2:A579.</summary>
    OwtchShot = 0xa20000 | EnemyAiCodePointers.BankA2.OwtchShot,
    /// <summary>Dragon touch callback at $A2:E7C8.</summary>
    DragonTouch = 0xa20000 | EnemyAiCodePointers.BankA2.DragonTouch,
    /// <summary>Dragon shot callback at $A2:E7CE.</summary>
    DragonShot = 0xa20000 | EnemyAiCodePointers.BankA2.DragonShot,
    /// <summary>Dragon Power Bomb callback at $A2:E7D4.</summary>
    DragonPowerBomb = 0xa20000 | EnemyAiCodePointers.BankA2.DragonPowerBomb,
    /// <summary>Shared G/Ripper 2 shot callback at $A2:E3A9.</summary>
    GRipperRipper2Shot = 0xa20000 | EnemyAiCodePointers.BankA2.GRipperRipper2Shot,
    /// <summary>Mama Turtle touch callback at $A2:9281.</summary>
    MamaTurtleTouch = 0xa20000 | EnemyAiCodePointers.BankA2.MamaTurtleTouch,
    /// <summary>Baby Turtle touch callback at $A2:929F.</summary>
    BabyTurtleTouch = 0xa20000 | EnemyAiCodePointers.BankA2.BabyTurtleTouch,
    /// <summary>Baby Turtle shot callback at $A2:930F.</summary>
    BabyTurtleShot = 0xa20000 | EnemyAiCodePointers.BankA2.BabyTurtleShot,
    /// <summary><c>MaridiaLargeSnailDamagingTouchAI</c> at $A2:D388.</summary>
    MaridiaLargeSnailDamagingTouch = 0xa20000 | EnemyAiCodePointers.BankA2.MaridiaLargeSnailDamagingTouch,
    /// <summary><c>MaridiaLargeSnailNonDamagingTouchAI</c> at $A2:D38C.</summary>
    MaridiaLargeSnailNonDamagingTouch = 0xa20000 | EnemyAiCodePointers.BankA2.MaridiaLargeSnailNonDamagingTouch,
    /// <summary><c>MaridiaLargeSnailShotAI</c> at $A2:D3B4.</summary>
    MaridiaLargeSnailShot = 0xa20000 | EnemyAiCodePointers.BankA2.MaridiaLargeSnailShot,
    /// <summary>Rinka touch callback at $A2:B947.</summary>
    RinkaTouch = 0xa20000 | EnemyAiCodePointers.BankA2.RinkaTouch,
    /// <summary>Rinka shot callback at $A2:B94D.</summary>
    RinkaShot = 0xa20000 | EnemyAiCodePointers.BankA2.RinkaShot,
    /// <summary>Rinka Power Bomb callback at $A2:B953.</summary>
    RinkaPowerBomb = 0xa20000 | EnemyAiCodePointers.BankA2.RinkaPowerBomb,
    /// <summary>Vertical shutter touch callback at $A2:F09D.</summary>
    VerticalShutterTouch = 0xa20000 | EnemyAiCodePointers.BankA2.VerticalShutterTouch,
    /// <summary>Shootable vertical shutter shot callback at $A2:F0A2.</summary>
    ShootableVerticalShutterShot = 0xa20000 | EnemyAiCodePointers.BankA2.ShootableVerticalShutterShot,
    /// <summary>Destroyable vertical shutter shot callback at $A2:F0AA.</summary>
    DestroyableVerticalShutterShot = 0xa20000 | EnemyAiCodePointers.BankA2.DestroyableVerticalShutterShot,
    /// <summary>Vertical shutter Power Bomb callback at $A2:F0B6.</summary>
    VerticalShutterPowerBomb = 0xa20000 | EnemyAiCodePointers.BankA2.VerticalShutterPowerBomb,
    /// <summary>Horizontal shutter touch callback at $A2:F3D8.</summary>
    HorizontalShutterTouch = 0xa20000 | EnemyAiCodePointers.BankA2.HorizontalShutterTouch,
    /// <summary>Horizontal shutter shot callback at $A2:F40E.</summary>
    HorizontalShutterShot = 0xa20000 | EnemyAiCodePointers.BankA2.HorizontalShutterShot,
    /// <summary>Horizontal shutter Power Bomb callback at $A2:F41A.</summary>
    HorizontalShutterPowerBomb = 0xa20000 | EnemyAiCodePointers.BankA2.HorizontalShutterPowerBomb,
    /// <summary>Skree shot callback at $A3:C7F5.</summary>
    SkreeShot = 0xa30000 | EnemyAiCodePointers.BankA3.SkreeShot,
    /// <summary>Metaree shot callback at $A3:8B0F.</summary>
    MetareeShot = 0xa30000 | EnemyAiCodePointers.BankA3.MetareeShot,
    /// <summary>Fireflea touch callback at $A3:8E6B.</summary>
    FirefleaTouch = 0xa30000 | EnemyAiCodePointers.BankA3.FirefleaTouch,
    /// <summary>Fireflea Power Bomb callback at $A3:8E83.</summary>
    FirefleaPowerBomb = 0xa30000 | EnemyAiCodePointers.BankA3.FirefleaPowerBomb,
    /// <summary>Fireflea shot callback at $A3:8E89.</summary>
    FirefleaShot = 0xa30000 | EnemyAiCodePointers.BankA3.FirefleaShot,
    /// <summary>Mochtroid touch callback at $A3:A953.</summary>
    MochtroidTouch = 0xa30000 | EnemyAiCodePointers.BankA3.MochtroidTouch,
    /// <summary>Mochtroid shot callback at $A3:A9A8.</summary>
    MochtroidShot = 0xa30000 | EnemyAiCodePointers.BankA3.MochtroidShot,
    /// <summary>Metroid touch callback at $A3:EDEB.</summary>
    MetroidTouch = 0xa30000 | EnemyAiCodePointers.BankA3.MetroidTouch,
    /// <summary>Metroid shot callback at $A3:EF07.</summary>
    MetroidShot = 0xa30000 | EnemyAiCodePointers.BankA3.MetroidShot,
    /// <summary>Metroid Power Bomb callback at $A3:F042.</summary>
    MetroidPowerBomb = 0xa30000 | EnemyAiCodePointers.BankA3.MetroidPowerBomb,
    /// <summary>Yard touch callback at $A3:D3B0.</summary>
    YardTouch = 0xa30000 | EnemyAiCodePointers.BankA3.YardTouch,
    /// <summary>Yard shot callback at $A3:D469.</summary>
    YardShot = 0xa30000 | EnemyAiCodePointers.BankA3.YardShot,
    /// <summary>Platform no-op touch callback at $A3:9F07.</summary>
    PlatformNoOpTouch = 0xa30000 | EnemyAiCodePointers.BankA3.PlatformNoOpTouch,
    /// <summary>Tripper shot callback at $A3:9F08.</summary>
    TripperShot = 0xa30000 | EnemyAiCodePointers.BankA3.TripperShot,
    /// <summary>Crocomire header touch callback at $A4:B950.</summary>
    CrocomireHeaderTouch = 0xa40000 | EnemyAiCodePointers.BankA4.HeaderTouch,
    /// <summary>Crocomire claw touch callback at $A4:B93D.</summary>
    CrocomireClawTouch = 0xa40000 | EnemyAiCodePointers.BankA4.ClawTouch,
    /// <summary>Crocomire no-op hitbox shot callback at $A4:B951.</summary>
    CrocomireNoOpHitboxShot = 0xa40000 | EnemyAiCodePointers.BankA4.NoOpHitboxShot,
    /// <summary>Crocomire dust hitbox shot callback at $A4:B968.</summary>
    CrocomireDustHitboxShot = 0xa40000 | EnemyAiCodePointers.BankA4.DustHitboxShot,
    /// <summary>Crocomire mouth shot callback at $A4:BA05.</summary>
    CrocomireMouthShot = 0xa40000 | EnemyAiCodePointers.BankA4.MouthShot,
    /// <summary>Alternate Crocomire dust hitbox shot callback at $A4:BAB4.</summary>
    CrocomireAlternateDustHitboxShot = 0xa40000 | EnemyAiCodePointers.BankA4.AlternateDustHitboxShot,
    /// <summary>Crocomire Power Bomb callback at $A4:B992.</summary>
    CrocomirePowerBomb = 0xa40000 | EnemyAiCodePointers.BankA4.PowerBomb,
    /// <summary>Spore Spawn touch callback at $A5:EDEC.</summary>
    SporeSpawnTouch = 0xa50000 | EnemyAiCodePointers.BankA5.SporeSpawnTouch,
    /// <summary>Spore Spawn shot callback at $A5:ED5A.</summary>
    SporeSpawnShot = 0xa50000 | EnemyAiCodePointers.BankA5.SporeSpawnShot,
    /// <summary>Draygon touch callback at $A5:95EA.</summary>
    DraygonTouch = 0xa50000 | EnemyAiCodePointers.BankA5.DraygonTouch,
    /// <summary>Draygon shot callback at $A5:95F0.</summary>
    DraygonShot = 0xa50000 | EnemyAiCodePointers.BankA5.DraygonShot,
    /// <summary>Draygon Power Bomb callback at $A5:9607.</summary>
    DraygonPowerBomb = 0xa50000 | EnemyAiCodePointers.BankA5.DraygonPowerBomb,
    /// <summary>Fake Kraid touch callback at $A6:9C22.</summary>
    FakeKraidTouch = 0xa60000 | EnemyAiCodePointers.BankA6.FakeKraidTouch,
    /// <summary>Fake Kraid shot callback at $A6:9C39.</summary>
    FakeKraidShot = 0xa60000 | EnemyAiCodePointers.BankA6.FakeKraidShot,
    /// <summary>Ceres steam touch callback at $A6:F03F.</summary>
    CeresSteamTouch = 0xa60000 | EnemyAiCodePointers.BankA6.CeresSteamTouch,
    /// <summary>Ridley extended-spritemap touch callback at $A6:DF59.</summary>
    RidleyExtendedTouch = 0xa60000 | EnemyAiCodePointers.BankA6.RidleyExtendedTouch,
    /// <summary>Ridley shot callback at $A6:DF8A.</summary>
    RidleyShot = 0xa60000 | EnemyAiCodePointers.BankA6.RidleyShot,
    /// <summary>Ridley Power Bomb callback at $A6:DFB2.</summary>
    RidleyPowerBomb = 0xa60000 | EnemyAiCodePointers.BankA6.RidleyPowerBomb,
    /// <summary>Zebetite touch callback at $A6:FDA7.</summary>
    ZebetiteTouch = 0xa60000 | EnemyAiCodePointers.BankA6.ZebetiteTouch,
    /// <summary>Zebetite shot callback at $A6:FDAC.</summary>
    ZebetiteShot = 0xa60000 | EnemyAiCodePointers.BankA6.ZebetiteShot,
    /// <summary>Kraid background/foot touch callback at $A7:948B.</summary>
    KraidBackgroundTouch = 0xa70000 | EnemyAiCodePointers.BankA7.KraidBackgroundTouch,
    /// <summary>Kraid arm touch callback at $A7:9490.</summary>
    KraidArmTouch = 0xa70000 | EnemyAiCodePointers.BankA7.KraidArmTouch,
    /// <summary>Kraid no-op shot callback at $A7:94B5.</summary>
    KraidNoOpShot = 0xa70000 | EnemyAiCodePointers.BankA7.KraidNoOpShot,
    /// <summary>Kraid arm shot callback at $A7:94B6.</summary>
    KraidArmShot = 0xa70000 | EnemyAiCodePointers.BankA7.KraidArmShot,
    /// <summary>EnemyTouch_KraidNail at $A7:BCCF: normal touch followed by enemy death.</summary>
    KraidNailTouch = 0xa70000 | EnemyAiCodePointers.BankA7.KraidNailTouch,
    /// <summary>EnemyTouch_KraidNailBad at $A7:BCDE: normal touch followed by enemy death.</summary>
    KraidBadNailTouch = 0xa70000 | EnemyAiCodePointers.BankA7.KraidBadNailTouch,
    /// <summary>Evir touch callback at $A8:8B06.</summary>
    EvirTouch = 0xa80000 | EnemyAiCodePointers.BankA8.EvirTouch,
    /// <summary>Evir Power Bomb callback at $A8:8B0C.</summary>
    EvirPowerBomb = 0xa80000 | EnemyAiCodePointers.BankA8.EvirPowerBomb,
    /// <summary>Evir shot callback at $A8:8B12.</summary>
    EvirShot = 0xa80000 | EnemyAiCodePointers.BankA8.EvirShot,
    /// <summary>Magdollite Power Bomb callback at $A8:B400.</summary>
    MagdollitePowerBomb = 0xa80000 | EnemyAiCodePointers.BankA8.MagdollitePowerBomb,
    /// <summary>Magdollite touch callback at $A8:B406.</summary>
    MagdolliteTouch = 0xa80000 | EnemyAiCodePointers.BankA8.MagdolliteTouch,
    /// <summary>Magdollite shot callback at $A8:B40C.</summary>
    MagdolliteShot = 0xa80000 | EnemyAiCodePointers.BankA8.MagdolliteShot,
    /// <summary>Beetom touch callback at $A8:BE2E.</summary>
    BeetomTouch = 0xa80000 | EnemyAiCodePointers.BankA8.BeetomTouch,
    /// <summary>Beetom shot callback at $A8:BEAC.</summary>
    BeetomShot = 0xa80000 | EnemyAiCodePointers.BankA8.BeetomShot,
    /// <summary>Powamp touch callback at $A8:C5BE.</summary>
    PowampTouch = 0xa80000 | EnemyAiCodePointers.BankA8.PowampTouch,
    /// <summary>Powamp shot callback at $A8:C5EF.</summary>
    PowampShot = 0xa80000 | EnemyAiCodePointers.BankA8.PowampShot,
    /// <summary>Powamp Power Bomb callback at $A8:C63F.</summary>
    PowampPowerBomb = 0xa80000 | EnemyAiCodePointers.BankA8.PowampPowerBomb,
    /// <summary>Work robot touch callback at $A8:D174.</summary>
    WorkRobotTouch = 0xa80000 | EnemyAiCodePointers.BankA8.WorkRobotTouch,
    /// <summary>Work robot no-power shot callback at $A8:D18D.</summary>
    WorkRobotNoPowerShot = 0xa80000 | EnemyAiCodePointers.BankA8.WorkRobotNoPowerShot,
    /// <summary>Work robot powered shot callback at $A8:D192.</summary>
    WorkRobotShot = 0xa80000 | EnemyAiCodePointers.BankA8.WorkRobotShot,
    /// <summary>Bull shot callback at $A8:DB14.</summary>
    BullShot = 0xa80000 | EnemyAiCodePointers.BankA8.BullShot,
    /// <summary>Kago shot callback at $A8:AB83.</summary>
    KagoShot = 0xa80000 | EnemyAiCodePointers.BankA8.KagoShot,
    /// <summary>KiHunter shot callback at $A8:F701.</summary>
    KiHunterShot = 0xa80000 | EnemyAiCodePointers.BankA8.KiHunterShot,
    /// <summary>Spark shot callback at $A8:E70E.</summary>
    SparkShot = 0xa80000 | EnemyAiCodePointers.BankA8.SparkShot,
    /// <summary><c>BlueBrinstarFaceBlockShotAI</c> at $A8:E91D.</summary>
    BlueBrinstarFaceBlockShot = 0xa80000 | EnemyAiCodePointers.BankA8.BlueBrinstarFaceBlockShot,
    /// <summary>Yapping Maw touch callback at $A8:A799.</summary>
    YappingMawTouch = 0xa80000 | EnemyAiCodePointers.BankA8.YappingMawTouch,
    /// <summary>Yapping Maw shot callback at $A8:A7BD.</summary>
    YappingMawShot = 0xa80000 | EnemyAiCodePointers.BankA8.YappingMawShot,
    /// <summary>Mother Brain body shot callback at $A9:B503.</summary>
    MotherBrainBodyShot = 0xa90000 | EnemyAiCodePointers.BankA9.MotherBrainBodyShot,
    /// <summary>Mother Brain head shot callback at $A9:B507.</summary>
    MotherBrainHeadShot = 0xa90000 | EnemyAiCodePointers.BankA9.MotherBrainHeadShot,
    /// <summary>Mother Brain body touch callback at $A9:B5C5, a bare RTL.</summary>
    MotherBrainBodyTouch = 0xa90000 | EnemyAiCodePointers.BankA9.MotherBrainBodyTouch,
    /// <summary>Mother Brain head touch callback at $A9:B5C6.</summary>
    MotherBrainHeadTouch = 0xa90000 | EnemyAiCodePointers.BankA9.MotherBrainHeadTouch,
    /// <summary>Dead Torizo touch/shot callback at $A9:D433.</summary>
    DeadTorizoTouchAndShot = 0xa90000 | EnemyAiCodePointers.BankA9.DeadTorizoTouchAndShot,
    /// <summary>Dead Torizo Power Bomb callback at $A9:D42A.</summary>
    DeadTorizoPowerBomb = 0xa90000 | EnemyAiCodePointers.BankA9.DeadTorizoPowerBomb,
    /// <summary>Dead Sidehopper touch callback at $A9:DD44.</summary>
    DeadSidehopperTouch = 0xa90000 | EnemyAiCodePointers.BankA9.DeadSidehopperTouch,
    /// <summary>Dead Sidehopper shot callback at $A9:DD1D.</summary>
    DeadSidehopperShot = 0xa90000 | EnemyAiCodePointers.BankA9.DeadSidehopperShot,
    /// <summary>Dead Sidehopper Power Bomb callback at $A9:D8CC.</summary>
    DeadSidehopperPowerBomb = 0xa90000 | EnemyAiCodePointers.BankA9.DeadSidehopperPowerBomb,
    /// <summary>Shitroid touch callback at $A9:F789.</summary>
    ShitroidTouch = 0xa90000 | EnemyAiCodePointers.BankA9.ShitroidTouch,
    /// <summary>Shitroid shot callback at $A9:F842.</summary>
    ShitroidShot = 0xa90000 | EnemyAiCodePointers.BankA9.ShitroidShot,
    /// <summary>Shitroid Power Bomb callback at $A9:EFBA.</summary>
    ShitroidPowerBomb = 0xa90000 | EnemyAiCodePointers.BankA9.ShitroidPowerBomb,
    /// <summary>Bomb Torizo touch callback at $AA:C977.</summary>
    BombTorizoTouch = 0xaa0000 | EnemyAiCodePointers.BankAA.BombTorizoTouch,
    /// <summary>Bomb Torizo shot callback at $AA:C97C.</summary>
    BombTorizoShot = 0xaa0000 | EnemyAiCodePointers.BankAA.BombTorizoShot,
    /// <summary>Torizo stand-up/sit-down shot callback at $AA:C9C2.</summary>
    TorizoStandUpSitDownShot = 0xaa0000 | EnemyAiCodePointers.BankAA.TorizoStandUpSitDownShot,
    /// <summary>Golden Torizo shot callback at $AA:D667.</summary>
    GoldenTorizoShot = 0xaa0000 | EnemyAiCodePointers.BankAA.GoldenTorizoShot,
    /// <summary>Shaktool touch callback at $AA:DF2F.</summary>
    ShaktoolTouch = 0xaa0000 | EnemyAiCodePointers.BankAA.ShaktoolTouch,
    /// <summary>Shaktool shot callback at $AA:DF34.</summary>
    ShaktoolShot = 0xaa0000 | EnemyAiCodePointers.BankAA.ShaktoolShot,
    /// <summary>Space Pirate Power Bomb callback at $B2:8767.</summary>
    SpacePiratePowerBomb = 0xb20000 | EnemyAiCodePointers.BankB2.PowerBomb,
    /// <summary>Space Pirate touch callback at $B2:876C.</summary>
    SpacePirateTouch = 0xb20000 | EnemyAiCodePointers.BankB2.Touch,
    /// <summary>Space Pirate shot callback at $B2:8779.</summary>
    SpacePirateShot = 0xb20000 | EnemyAiCodePointers.BankB2.Shot,
    /// <summary>Gold ninja Space Pirate vulnerable-hitbox shot callback at $B2:87C8.</summary>
    GoldNinjaVulnerableHitboxShot = 0xb20000 | EnemyAiCodePointers.BankB2.GoldNinjaVulnerableHitboxShot,
    /// <summary>Gold ninja Space Pirate invincible-hitbox shot callback at $B2:883E.</summary>
    GoldNinjaInvincibleHitboxShot = 0xb20000 | EnemyAiCodePointers.BankB2.GoldNinjaInvincibleHitboxShot,
    /// <summary>Botwoon touch callback at $B3:9FFF.</summary>
    BotwoonTouch = 0xb30000 | EnemyAiCodePointers.BankB3.BotwoonTouch,
    /// <summary>Botwoon shot callback at $B3:A016.</summary>
    BotwoonShot = 0xb30000 | EnemyAiCodePointers.BankB3.BotwoonShot,
    /// <summary>Botwoon Power Bomb callback at $B3:A041.</summary>
    BotwoonPowerBomb = 0xb30000 | EnemyAiCodePointers.BankB3.BotwoonPowerBomb,
    /// <summary>Phantoon body touch callback at $A7:DD95.</summary>
    PhantoonTouch = 0xa70000 | PhantoonCollisionDefinitions.TouchAi,
    /// <summary>Phantoon body shot callback at $A7:DD9B.</summary>
    PhantoonShot = 0xa70000 | PhantoonCollisionDefinitions.ShotAi,
}
