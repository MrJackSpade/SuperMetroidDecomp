namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Enemy interaction callbacks (touch, shot, Power Bomb and extended-hitbox reactions) that the port names. Extraction
/// decodes each header's and hitbox's bank-qualified callback words to these identities.
/// </summary>
public enum EnemyInteractionCallback
{
    /// <summary><c>NormalEnemyTouchAI</c> at $A0:8023.</summary>
    NormalEnemyTouch,
    /// <summary><c>NormalEnemyShotAI</c> at $A0:802D.</summary>
    NormalEnemyShot,
    /// <summary>Shared dud-shot callback at $A0:8046.</summary>
    DudShot,
    /// <summary>Shared no-op interaction callback at $A0:804C.</summary>
    NoOp,
    /// <summary>RTS_A0804B at $A0:804B, the shared short-return no-op identity rejected by collision dispatch.</summary>
    NoOpShortReturn,
    /// <summary>Owtch shot callback at $A2:A579.</summary>
    OwtchShot,
    /// <summary>Dragon touch callback at $A2:E7C8.</summary>
    DragonTouch,
    /// <summary>Dragon shot callback at $A2:E7CE.</summary>
    DragonShot,
    /// <summary>Dragon Power Bomb callback at $A2:E7D4.</summary>
    DragonPowerBomb,
    /// <summary>Shared G/Ripper 2 shot callback at $A2:E3A9.</summary>
    GRipperRipper2Shot,
    /// <summary>Mama Turtle touch callback at $A2:9281.</summary>
    MamaTurtleTouch,
    /// <summary>Baby Turtle touch callback at $A2:929F.</summary>
    BabyTurtleTouch,
    /// <summary>Baby Turtle shot callback at $A2:930F.</summary>
    BabyTurtleShot,
    /// <summary><c>MaridiaLargeSnailDamagingTouchAI</c> at $A2:D388.</summary>
    MaridiaLargeSnailDamagingTouch,
    /// <summary><c>MaridiaLargeSnailNonDamagingTouchAI</c> at $A2:D38C.</summary>
    MaridiaLargeSnailNonDamagingTouch,
    /// <summary><c>MaridiaLargeSnailShotAI</c> at $A2:D3B4.</summary>
    MaridiaLargeSnailShot,
    /// <summary>Rinka touch callback at $A2:B947.</summary>
    RinkaTouch,
    /// <summary>Rinka shot callback at $A2:B94D.</summary>
    RinkaShot,
    /// <summary>Rinka Power Bomb callback at $A2:B953.</summary>
    RinkaPowerBomb,
    /// <summary>Vertical shutter touch callback at $A2:F09D.</summary>
    VerticalShutterTouch,
    /// <summary>Shootable vertical shutter shot callback at $A2:F0A2.</summary>
    ShootableVerticalShutterShot,
    /// <summary>Destroyable vertical shutter shot callback at $A2:F0AA.</summary>
    DestroyableVerticalShutterShot,
    /// <summary>Vertical shutter Power Bomb callback at $A2:F0B6.</summary>
    VerticalShutterPowerBomb,
    /// <summary>Horizontal shutter touch callback at $A2:F3D8.</summary>
    HorizontalShutterTouch,
    /// <summary>Horizontal shutter shot callback at $A2:F40E.</summary>
    HorizontalShutterShot,
    /// <summary>Horizontal shutter Power Bomb callback at $A2:F41A.</summary>
    HorizontalShutterPowerBomb,
    /// <summary>Skree shot callback at $A3:C7F5.</summary>
    SkreeShot,
    /// <summary>Metaree shot callback at $A3:8B0F.</summary>
    MetareeShot,
    /// <summary>Fireflea touch callback at $A3:8E6B.</summary>
    FirefleaTouch,
    /// <summary>Fireflea Power Bomb callback at $A3:8E83.</summary>
    FirefleaPowerBomb,
    /// <summary>Fireflea shot callback at $A3:8E89.</summary>
    FirefleaShot,
    /// <summary>Mochtroid touch callback at $A3:A953.</summary>
    MochtroidTouch,
    /// <summary>Mochtroid shot callback at $A3:A9A8.</summary>
    MochtroidShot,
    /// <summary>Metroid touch callback at $A3:EDEB.</summary>
    MetroidTouch,
    /// <summary>Metroid shot callback at $A3:EF07.</summary>
    MetroidShot,
    /// <summary>Metroid Power Bomb callback at $A3:F042.</summary>
    MetroidPowerBomb,
    /// <summary>Yard touch callback at $A3:D3B0.</summary>
    YardTouch,
    /// <summary>Yard shot callback at $A3:D469.</summary>
    YardShot,
    /// <summary>Platform no-op touch callback at $A3:9F07.</summary>
    PlatformNoOpTouch,
    /// <summary>Tripper shot callback at $A3:9F08.</summary>
    TripperShot,
    /// <summary>Crocomire header touch callback at $A4:B950.</summary>
    CrocomireHeaderTouch,
    /// <summary>Crocomire claw touch callback at $A4:B93D.</summary>
    CrocomireClawTouch,
    /// <summary>Crocomire no-op hitbox shot callback at $A4:B951.</summary>
    CrocomireNoOpHitboxShot,
    /// <summary>Crocomire dust hitbox shot callback at $A4:B968.</summary>
    CrocomireDustHitboxShot,
    /// <summary>Crocomire mouth shot callback at $A4:BA05.</summary>
    CrocomireMouthShot,
    /// <summary>Alternate Crocomire dust hitbox shot callback at $A4:BAB4.</summary>
    CrocomireAlternateDustHitboxShot,
    /// <summary>Crocomire Power Bomb callback at $A4:B992.</summary>
    CrocomirePowerBomb,
    /// <summary>Spore Spawn touch callback at $A5:EDEC.</summary>
    SporeSpawnTouch,
    /// <summary>Spore Spawn shot callback at $A5:ED5A.</summary>
    SporeSpawnShot,
    /// <summary>Draygon touch callback at $A5:95EA.</summary>
    DraygonTouch,
    /// <summary>Draygon shot callback at $A5:95F0.</summary>
    DraygonShot,
    /// <summary>Draygon Power Bomb callback at $A5:9607.</summary>
    DraygonPowerBomb,
    /// <summary>Fake Kraid touch callback at $A6:9C22.</summary>
    FakeKraidTouch,
    /// <summary>Fake Kraid shot callback at $A6:9C39.</summary>
    FakeKraidShot,
    /// <summary>Ceres steam touch callback at $A6:F03F.</summary>
    CeresSteamTouch,
    /// <summary>Ridley extended-spritemap touch callback at $A6:DF59.</summary>
    RidleyExtendedTouch,
    /// <summary>Ridley shot callback at $A6:DF8A.</summary>
    RidleyShot,
    /// <summary>Ridley Power Bomb callback at $A6:DFB2.</summary>
    RidleyPowerBomb,
    /// <summary>Zebetite touch callback at $A6:FDA7.</summary>
    ZebetiteTouch,
    /// <summary>Zebetite shot callback at $A6:FDAC.</summary>
    ZebetiteShot,
    /// <summary>Kraid background/foot touch callback at $A7:948B.</summary>
    KraidBackgroundTouch,
    /// <summary>Kraid arm touch callback at $A7:9490.</summary>
    KraidArmTouch,
    /// <summary>Kraid no-op shot callback at $A7:94B5.</summary>
    KraidNoOpShot,
    /// <summary>Kraid arm shot callback at $A7:94B6.</summary>
    KraidArmShot,
    /// <summary>EnemyTouch_KraidNail at $A7:BCCF: normal touch followed by enemy death.</summary>
    KraidNailTouch,
    /// <summary>EnemyTouch_KraidNailBad at $A7:BCDE: normal touch followed by enemy death.</summary>
    KraidBadNailTouch,
    /// <summary>Evir touch callback at $A8:8B06.</summary>
    EvirTouch,
    /// <summary>Evir Power Bomb callback at $A8:8B0C.</summary>
    EvirPowerBomb,
    /// <summary>Evir shot callback at $A8:8B12.</summary>
    EvirShot,
    /// <summary>Magdollite Power Bomb callback at $A8:B400.</summary>
    MagdollitePowerBomb,
    /// <summary>Magdollite touch callback at $A8:B406.</summary>
    MagdolliteTouch,
    /// <summary>Magdollite shot callback at $A8:B40C.</summary>
    MagdolliteShot,
    /// <summary>Beetom touch callback at $A8:BE2E.</summary>
    BeetomTouch,
    /// <summary>Beetom shot callback at $A8:BEAC.</summary>
    BeetomShot,
    /// <summary>Powamp touch callback at $A8:C5BE.</summary>
    PowampTouch,
    /// <summary>Powamp shot callback at $A8:C5EF.</summary>
    PowampShot,
    /// <summary>Powamp Power Bomb callback at $A8:C63F.</summary>
    PowampPowerBomb,
    /// <summary>Work robot touch callback at $A8:D174.</summary>
    WorkRobotTouch,
    /// <summary>Work robot no-power shot callback at $A8:D18D.</summary>
    WorkRobotNoPowerShot,
    /// <summary>Work robot powered shot callback at $A8:D192.</summary>
    WorkRobotShot,
    /// <summary>Bull shot callback at $A8:DB14.</summary>
    BullShot,
    /// <summary>Kago shot callback at $A8:AB83.</summary>
    KagoShot,
    /// <summary>KiHunter shot callback at $A8:F701.</summary>
    KiHunterShot,
    /// <summary>Spark shot callback at $A8:E70E.</summary>
    SparkShot,
    /// <summary><c>BlueBrinstarFaceBlockShotAI</c> at $A8:E91D.</summary>
    BlueBrinstarFaceBlockShot,
    /// <summary>Yapping Maw touch callback at $A8:A799.</summary>
    YappingMawTouch,
    /// <summary>Yapping Maw shot callback at $A8:A7BD.</summary>
    YappingMawShot,
    /// <summary>Mother Brain body shot callback at $A9:B503.</summary>
    MotherBrainBodyShot,
    /// <summary>Mother Brain head shot callback at $A9:B507.</summary>
    MotherBrainHeadShot,
    /// <summary>Mother Brain body touch callback at $A9:B5C5, a bare RTL.</summary>
    MotherBrainBodyTouch,
    /// <summary>Mother Brain head touch callback at $A9:B5C6.</summary>
    MotherBrainHeadTouch,
    /// <summary>Dead Torizo touch/shot callback at $A9:D433.</summary>
    DeadTorizoTouchAndShot,
    /// <summary>Dead Torizo Power Bomb callback at $A9:D42A.</summary>
    DeadTorizoPowerBomb,
    /// <summary>Dead Sidehopper touch callback at $A9:DD44.</summary>
    DeadSidehopperTouch,
    /// <summary>Dead Sidehopper shot callback at $A9:DD1D.</summary>
    DeadSidehopperShot,
    /// <summary>Dead Sidehopper Power Bomb callback at $A9:D8CC.</summary>
    DeadSidehopperPowerBomb,
    /// <summary>Shitroid touch callback at $A9:F789.</summary>
    ShitroidTouch,
    /// <summary>Shitroid shot callback at $A9:F842.</summary>
    ShitroidShot,
    /// <summary>Shitroid Power Bomb callback at $A9:EFBA.</summary>
    ShitroidPowerBomb,
    /// <summary>Bomb Torizo touch callback at $AA:C977.</summary>
    BombTorizoTouch,
    /// <summary>Bomb Torizo shot callback at $AA:C97C.</summary>
    BombTorizoShot,
    /// <summary>Torizo stand-up/sit-down shot callback at $AA:C9C2.</summary>
    TorizoStandUpSitDownShot,
    /// <summary>Golden Torizo shot callback at $AA:D667.</summary>
    GoldenTorizoShot,
    /// <summary>Shaktool touch callback at $AA:DF2F.</summary>
    ShaktoolTouch,
    /// <summary>Shaktool shot callback at $AA:DF34.</summary>
    ShaktoolShot,
    /// <summary>Space Pirate Power Bomb callback at $B2:8767.</summary>
    SpacePiratePowerBomb,
    /// <summary>Space Pirate touch callback at $B2:876C.</summary>
    SpacePirateTouch,
    /// <summary>Space Pirate shot callback at $B2:8779.</summary>
    SpacePirateShot,
    /// <summary>Gold ninja Space Pirate vulnerable-hitbox shot callback at $B2:87C8.</summary>
    GoldNinjaVulnerableHitboxShot,
    /// <summary>Gold ninja Space Pirate invincible-hitbox shot callback at $B2:883E.</summary>
    GoldNinjaInvincibleHitboxShot,
    /// <summary>Botwoon touch callback at $B3:9FFF.</summary>
    BotwoonTouch,
    /// <summary>Botwoon shot callback at $B3:A016.</summary>
    BotwoonShot,
    /// <summary>Botwoon Power Bomb callback at $B3:A041.</summary>
    BotwoonPowerBomb,
    /// <summary>Phantoon body touch callback at $A7:DD95.</summary>
    PhantoonTouch,
    /// <summary>Phantoon body shot callback at $A7:DD9B.</summary>
    PhantoonShot,
    /// <summary>Dead Zoomer's touch and shot rot trigger at $A9:DCF8.</summary>
    DeadZoomerTouchAndShot,
    /// <summary>Dead Zoomer's Power Bomb rot trigger at $A9:DCED.</summary>
    DeadZoomerPowerBomb,
    /// <summary>Dead Ripper's touch and shot rot trigger at $A9:DD08.</summary>
    DeadRipperTouchAndShot,
    /// <summary>Dead Ripper's Power Bomb rot trigger at $A9:DCFD.</summary>
    DeadRipperPowerBomb,
    /// <summary>Dead Skree's touch and shot rot trigger at $A9:DD18.</summary>
    DeadSkreeTouchAndShot,
    /// <summary>Dead Skree's Power Bomb rot trigger at $A9:DD0D.</summary>
    DeadSkreePowerBomb,
    /// <summary><c>RTL_A6F920</c>, the Ceres door shot callback at $A6:F920.</summary>
    CeresDoorNoOpShot,
    /// <summary><c>RTL_AAE7DC</c>, the Chozo statue shot callback at $AA:E7DC.</summary>
    ChozoNoOpShot,
}
