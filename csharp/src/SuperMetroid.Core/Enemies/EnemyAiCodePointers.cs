namespace SuperMetroid.Core.Enemies;

/// <summary>Named cartridge enemy interaction callbacks, grouped by their native bank.</summary>
internal static class EnemyAiCodePointers
{
    /// <summary>Shared bank-$A0 enemy interaction callbacks.</summary>
    public static class BankA0
    {
        /// <summary>The shared enemy bank, $A0, whose common prologue callbacks every enemy header can name.</summary>
        public const byte Bank = 0xa0;

        /// <summary><c>NormalEnemyTouchAI</c> at $A0:8023.</summary>
        public const ushort NormalEnemyTouch = 0x8023;
        /// <summary><c>NormalEnemyShotAI</c> at $A0:802D.</summary>
        public const ushort NormalEnemyShot = 0x802d;
        /// <summary>Shared dud-shot callback at $A0:8046.</summary>
        public const ushort DudShot = 0x8046;
        /// <summary>Shared no-op interaction callback at $A0:804C.</summary>
        public const ushort NoOp = (ushort)EnemyNoOpRoutine.LongReturn;

        /// <summary>RTS_A0804B at $A0:804B, the shared short-return no-op identity rejected by collision dispatch.</summary>
        public const ushort NoOpShortReturn = (ushort)EnemyNoOpRoutine.ShortReturn;
    }

    /// <summary>Bank-$A2 enemy interaction callbacks.</summary>
    public static class BankA2
    {
        /// <summary>Owtch shot callback at $A2:A579.</summary>
        public const ushort OwtchShot = 0xa579;
        /// <summary>Dragon touch callback at $A2:E7C8.</summary>
        public const ushort DragonTouch = 0xe7c8;
        /// <summary>Dragon shot callback at $A2:E7CE.</summary>
        public const ushort DragonShot = 0xe7ce;
        /// <summary>Dragon Power Bomb callback at $A2:E7D4.</summary>
        public const ushort DragonPowerBomb = 0xe7d4;
        /// <summary>Shared G/Ripper 2 shot callback at $A2:E3A9.</summary>
        public const ushort GRipperRipper2Shot = 0xe3a9;
        /// <summary>Mama Turtle touch callback at $A2:9281.</summary>
        public const ushort MamaTurtleTouch = 0x9281;
        /// <summary>Baby Turtle touch callback at $A2:929F.</summary>
        public const ushort BabyTurtleTouch = 0x929f;
        /// <summary>Baby Turtle shot callback at $A2:930F.</summary>
        public const ushort BabyTurtleShot = 0x930f;
        /// <summary><c>MaridiaLargeSnailDamagingTouchAI</c> at $A2:D388.</summary>
        public const ushort MaridiaLargeSnailDamagingTouch = 0xd388;
        /// <summary><c>MaridiaLargeSnailNonDamagingTouchAI</c> at $A2:D38C.</summary>
        public const ushort MaridiaLargeSnailNonDamagingTouch = 0xd38c;
        /// <summary><c>MaridiaLargeSnailShotAI</c> at $A2:D3B4.</summary>
        public const ushort MaridiaLargeSnailShot = 0xd3b4;
        /// <summary>Rinka touch callback at $A2:B947.</summary>
        public const ushort RinkaTouch = 0xb947;
        /// <summary>Rinka shot callback at $A2:B94D.</summary>
        public const ushort RinkaShot = 0xb94d;
        /// <summary>Rinka Power Bomb callback at $A2:B953.</summary>
        public const ushort RinkaPowerBomb = 0xb953;
        /// <summary>Vertical shutter touch callback at $A2:F09D.</summary>
        public const ushort VerticalShutterTouch = 0xf09d;
        /// <summary>Shootable vertical shutter shot callback at $A2:F0A2.</summary>
        public const ushort ShootableVerticalShutterShot = 0xf0a2;
        /// <summary>Destroyable vertical shutter shot callback at $A2:F0AA.</summary>
        public const ushort DestroyableVerticalShutterShot = 0xf0aa;
        /// <summary>Vertical shutter Power Bomb callback at $A2:F0B6.</summary>
        public const ushort VerticalShutterPowerBomb = 0xf0b6;
        /// <summary>Horizontal shutter touch callback at $A2:F3D8.</summary>
        public const ushort HorizontalShutterTouch = 0xf3d8;
        /// <summary>Horizontal shutter shot callback at $A2:F40E.</summary>
        public const ushort HorizontalShutterShot = 0xf40e;
        /// <summary>Horizontal shutter Power Bomb callback at $A2:F41A.</summary>
        public const ushort HorizontalShutterPowerBomb = 0xf41a;
    }

    /// <summary>Bank-$A3 enemy interaction callbacks.</summary>
    public static class BankA3
    {
        /// <summary>Skree shot callback at $A3:C7F5.</summary>
        public const ushort SkreeShot = 0xc7f5;
        /// <summary>Metaree shot callback at $A3:8B0F.</summary>
        public const ushort MetareeShot = 0x8b0f;
        /// <summary>Fireflea touch callback at $A3:8E6B.</summary>
        public const ushort FirefleaTouch = 0x8e6b;
        /// <summary>Fireflea Power Bomb callback at $A3:8E83.</summary>
        public const ushort FirefleaPowerBomb = 0x8e83;
        /// <summary>Fireflea shot callback at $A3:8E89.</summary>
        public const ushort FirefleaShot = 0x8e89;
        /// <summary>Mochtroid touch callback at $A3:A953.</summary>
        public const ushort MochtroidTouch = 0xa953;
        /// <summary>Mochtroid shot callback at $A3:A9A8.</summary>
        public const ushort MochtroidShot = 0xa9a8;
        /// <summary>Metroid touch callback at $A3:EDEB.</summary>
        public const ushort MetroidTouch = 0xedeb;
        /// <summary>Metroid shot callback at $A3:EF07.</summary>
        public const ushort MetroidShot = 0xef07;
        /// <summary>Metroid Power Bomb callback at $A3:F042.</summary>
        public const ushort MetroidPowerBomb = 0xf042;
        /// <summary>Yard touch callback at $A3:D3B0.</summary>
        public const ushort YardTouch = 0xd3b0;
        /// <summary>Yard shot callback at $A3:D469.</summary>
        public const ushort YardShot = 0xd469;
        /// <summary>Platform no-op touch callback at $A3:9F07.</summary>
        public const ushort PlatformNoOpTouch = 0x9f07;
        /// <summary>Tripper shot callback at $A3:9F08.</summary>
        public const ushort TripperShot = 0x9f08;
    }

    /// <summary>Bank-$A4 Crocomire interaction callbacks.</summary>
    public static class BankA4
    {
        /// <summary>Crocomire header touch callback at $A4:B950.</summary>
        public const ushort HeaderTouch = 0xb950;
        /// <summary>Crocomire claw touch callback at $A4:B93D.</summary>
        public const ushort ClawTouch = 0xb93d;
        /// <summary>Crocomire no-op hitbox shot callback at $A4:B951.</summary>
        public const ushort NoOpHitboxShot = 0xb951;
        /// <summary>Crocomire dust hitbox shot callback at $A4:B968.</summary>
        public const ushort DustHitboxShot = 0xb968;
        /// <summary>Crocomire mouth shot callback at $A4:BA05.</summary>
        public const ushort MouthShot = 0xba05;
        /// <summary>Alternate Crocomire dust hitbox shot callback at $A4:BAB4.</summary>
        public const ushort AlternateDustHitboxShot = 0xbab4;
        /// <summary>Crocomire Power Bomb callback at $A4:B992.</summary>
        public const ushort PowerBomb = 0xb992;
    }

    /// <summary>Bank-$A5 boss interaction callbacks.</summary>
    public static class BankA5
    {
        /// <summary>Spore Spawn touch callback at $A5:EDEC.</summary>
        public const ushort SporeSpawnTouch = 0xedec;
        /// <summary>Spore Spawn shot callback at $A5:ED5A.</summary>
        public const ushort SporeSpawnShot = 0xed5a;
        /// <summary>Draygon touch callback at $A5:95EA.</summary>
        public const ushort DraygonTouch = 0x95ea;
        /// <summary>Draygon shot callback at $A5:95F0.</summary>
        public const ushort DraygonShot = 0x95f0;
        /// <summary>Draygon Power Bomb callback at $A5:9607.</summary>
        public const ushort DraygonPowerBomb = 0x9607;
    }

    /// <summary>Bank-$A6 Ceres, Norfair boss, and Zebetite callbacks.</summary>
    public static class BankA6
    {
        /// <summary>Fake Kraid touch callback at $A6:9C22.</summary>
        public const ushort FakeKraidTouch = 0x9c22;
        /// <summary>Fake Kraid shot callback at $A6:9C39.</summary>
        public const ushort FakeKraidShot = 0x9c39;
        /// <summary>Ceres steam touch callback at $A6:F03F.</summary>
        public const ushort CeresSteamTouch = 0xf03f;
        /// <summary>Ridley extended-spritemap touch callback at $A6:DF59.</summary>
        public const ushort RidleyExtendedTouch = 0xdf59;
        /// <summary>Ridley shot callback at $A6:DF8A.</summary>
        public const ushort RidleyShot = 0xdf8a;
        /// <summary>Ridley Power Bomb callback at $A6:DFB2.</summary>
        public const ushort RidleyPowerBomb = 0xdfb2;
        /// <summary>Zebetite touch callback at $A6:FDA7.</summary>
        public const ushort ZebetiteTouch = 0xfda7;
        /// <summary>Zebetite shot callback at $A6:FDAC.</summary>
        public const ushort ZebetiteShot = 0xfdac;
    }

    /// <summary>Bank-$A7 Kraid interaction callbacks.</summary>
    public static class BankA7
    {
        /// <summary>Kraid background/foot touch callback at $A7:948B.</summary>
        public const ushort KraidBackgroundTouch = 0x948b;
        /// <summary>Kraid arm touch callback at $A7:9490.</summary>
        public const ushort KraidArmTouch = 0x9490;
        /// <summary>Kraid no-op shot callback at $A7:94B5.</summary>
        public const ushort KraidNoOpShot = 0x94b5;
        /// <summary>Kraid arm shot callback at $A7:94B6.</summary>
        public const ushort KraidArmShot = 0x94b6;
        /// <summary>EnemyTouch_KraidNail at $A7:BCCF: normal touch followed by enemy death.</summary>
        public const ushort KraidNailTouch = 0xbccf;
        /// <summary>EnemyTouch_KraidNailBad at $A7:BCDE: normal touch followed by enemy death.</summary>
        public const ushort KraidBadNailTouch = 0xbcde;
    }

    /// <summary>Bank-$A8 ordinary enemy interaction callbacks.</summary>
    public static class BankA8
    {
        /// <summary>Evir touch callback at $A8:8B06.</summary>
        public const ushort EvirTouch = 0x8b06;
        /// <summary>Evir Power Bomb callback at $A8:8B0C.</summary>
        public const ushort EvirPowerBomb = 0x8b0c;
        /// <summary>Evir shot callback at $A8:8B12.</summary>
        public const ushort EvirShot = 0x8b12;
        /// <summary>Magdollite Power Bomb callback at $A8:B400.</summary>
        public const ushort MagdollitePowerBomb = 0xb400;
        /// <summary>Magdollite touch callback at $A8:B406.</summary>
        public const ushort MagdolliteTouch = 0xb406;
        /// <summary>Magdollite shot callback at $A8:B40C.</summary>
        public const ushort MagdolliteShot = 0xb40c;
        /// <summary>Beetom touch callback at $A8:BE2E.</summary>
        public const ushort BeetomTouch = 0xbe2e;
        /// <summary>Beetom shot callback at $A8:BEAC.</summary>
        public const ushort BeetomShot = 0xbeac;
        /// <summary>Powamp touch callback at $A8:C5BE.</summary>
        public const ushort PowampTouch = 0xc5be;
        /// <summary>Powamp shot callback at $A8:C5EF.</summary>
        public const ushort PowampShot = 0xc5ef;
        /// <summary>Powamp Power Bomb callback at $A8:C63F.</summary>
        public const ushort PowampPowerBomb = 0xc63f;
        /// <summary>Work robot touch callback at $A8:D174.</summary>
        public const ushort WorkRobotTouch = 0xd174;
        /// <summary>Work robot no-power shot callback at $A8:D18D.</summary>
        public const ushort WorkRobotNoPowerShot = 0xd18d;
        /// <summary>Work robot powered shot callback at $A8:D192.</summary>
        public const ushort WorkRobotShot = 0xd192;
        /// <summary>Bull shot callback at $A8:DB14.</summary>
        public const ushort BullShot = 0xdb14;
        /// <summary>Kago shot callback at $A8:AB83.</summary>
        public const ushort KagoShot = 0xab83;
        /// <summary>KiHunter shot callback at $A8:F701.</summary>
        public const ushort KiHunterShot = 0xf701;
        /// <summary>Spark shot callback at $A8:E70E.</summary>
        public const ushort SparkShot = 0xe70e;
        /// <summary><c>BlueBrinstarFaceBlockShotAI</c> at $A8:E91D.</summary>
        public const ushort BlueBrinstarFaceBlockShot = 0xe91d;
        /// <summary>Yapping Maw touch callback at $A8:A799.</summary>
        public const ushort YappingMawTouch = 0xa799;
        /// <summary>Yapping Maw shot callback at $A8:A7BD.</summary>
        public const ushort YappingMawShot = 0xa7bd;
    }

    /// <summary>Bank-$A9 Mother Brain and dead-Tourian interaction callbacks.</summary>
    public static class BankA9
    {
        /// <summary>Mother Brain body shot callback at $A9:B503.</summary>
        public const ushort MotherBrainBodyShot = 0xb503;
        /// <summary>Mother Brain head shot callback at $A9:B507.</summary>
        public const ushort MotherBrainHeadShot = 0xb507;
        /// <summary>Mother Brain body touch callback at $A9:B5C5, a bare RTL.</summary>
        public const ushort MotherBrainBodyTouch = 0xb5c5;
        /// <summary>Mother Brain head touch callback at $A9:B5C6.</summary>
        public const ushort MotherBrainHeadTouch = 0xb5c6;
        /// <summary>Dead Torizo touch/shot callback at $A9:D433.</summary>
        public const ushort DeadTorizoTouchAndShot = 0xd433;
        /// <summary>Dead Torizo Power Bomb callback at $A9:D42A.</summary>
        public const ushort DeadTorizoPowerBomb = 0xd42a;
        /// <summary>Dead Sidehopper touch callback at $A9:DD44.</summary>
        public const ushort DeadSidehopperTouch = 0xdd44;
        /// <summary>Dead Sidehopper shot callback at $A9:DD1D.</summary>
        public const ushort DeadSidehopperShot = 0xdd1d;
        /// <summary>Dead Sidehopper Power Bomb callback at $A9:D8CC.</summary>
        public const ushort DeadSidehopperPowerBomb = 0xd8cc;
        /// <summary>Shitroid touch callback at $A9:F789.</summary>
        public const ushort ShitroidTouch = 0xf789;
        /// <summary>Shitroid shot callback at $A9:F842.</summary>
        public const ushort ShitroidShot = 0xf842;
        /// <summary>Shitroid Power Bomb callback at $A9:EFBA.</summary>
        public const ushort ShitroidPowerBomb = 0xefba;
    }

    /// <summary>Bank-$AA Torizo/Shaktool interaction callbacks.</summary>
    public static class BankAA
    {
        /// <summary>Bomb Torizo touch callback at $AA:C977.</summary>
        public const ushort BombTorizoTouch = 0xc977;
        /// <summary>Bomb Torizo shot callback at $AA:C97C.</summary>
        public const ushort BombTorizoShot = 0xc97c;
        /// <summary>Torizo stand-up/sit-down shot callback at $AA:C9C2.</summary>
        public const ushort TorizoStandUpSitDownShot = 0xc9c2;
        /// <summary>Golden Torizo shot callback at $AA:D667.</summary>
        public const ushort GoldenTorizoShot = 0xd667;
        /// <summary>Shaktool touch callback at $AA:DF2F.</summary>
        public const ushort ShaktoolTouch = 0xdf2f;
        /// <summary>Shaktool shot callback at $AA:DF34.</summary>
        public const ushort ShaktoolShot = 0xdf34;
    }

    /// <summary>Bank-$B2 Space Pirate interaction callbacks.</summary>
    public static class BankB2
    {
        /// <summary>Space Pirate Power Bomb callback at $B2:8767.</summary>
        public const ushort PowerBomb = 0x8767;
        /// <summary>Space Pirate touch callback at $B2:876C.</summary>
        public const ushort Touch = 0x876c;
        /// <summary>Space Pirate shot callback at $B2:8779.</summary>
        public const ushort Shot = 0x8779;
        /// <summary>Gold ninja Space Pirate vulnerable-hitbox shot callback at $B2:87C8.</summary>
        public const ushort GoldNinjaVulnerableHitboxShot = 0x87c8;
        /// <summary>Gold ninja Space Pirate invincible-hitbox shot callback at $B2:883E.</summary>
        public const ushort GoldNinjaInvincibleHitboxShot = 0x883e;
    }

    /// <summary>Bank-$B3 Botwoon interaction callbacks.</summary>
    public static class BankB3
    {
        /// <summary>Botwoon touch callback at $B3:9FFF.</summary>
        public const ushort BotwoonTouch = 0x9fff;
        /// <summary>Botwoon shot callback at $B3:A016.</summary>
        public const ushort BotwoonShot = 0xa016;
        /// <summary>Botwoon Power Bomb callback at $B3:A041.</summary>
        public const ushort BotwoonPowerBomb = 0xa041;
    }

}
