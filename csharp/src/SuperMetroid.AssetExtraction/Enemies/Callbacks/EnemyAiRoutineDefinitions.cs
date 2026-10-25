using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Enemies.Callbacks;

/// <summary>Named cartridge enemy interaction callbacks, grouped by their native bank.</summary>
internal static class EnemyAiRoutineDefinitions
{
    /// <summary>Shared bank-$A0 enemy interaction callbacks.</summary>
    public static class BankA0
    {
        /// <summary>The shared enemy bank, $A0, whose common prologue callbacks every enemy header can name.</summary>
        public const byte Bank = 0xa0;

        /// <summary><c>NormalEnemyTouchAI</c> at $A0:8023.</summary>
        public static readonly SnesAddress NormalEnemyTouch = new(Bank, 0x8023);
        /// <summary><c>NormalEnemyShotAI</c> at $A0:802D.</summary>
        public static readonly SnesAddress NormalEnemyShot = new(Bank, 0x802d);
        /// <summary>Shared dud-shot callback at $A0:8046.</summary>
        public static readonly SnesAddress DudShot = new(Bank, 0x8046);
        /// <summary>Shared no-op interaction callback at $A0:804C.</summary>
        public static readonly SnesAddress NoOp = new(Bank, 0x804c);

        /// <summary>RTS_A0804B at $A0:804B, the shared short-return no-op identity rejected by collision dispatch.</summary>
        public static readonly SnesAddress NoOpShortReturn = new(Bank, 0x804b);
    }

    /// <summary>Bank-$A2 enemy interaction callbacks.</summary>
    public static class BankA2
    {
        /// <summary>The enemy bank, $A2, whose code these callbacks run in.</summary>
        public const byte Bank = 0xa2;

        /// <summary>Owtch shot callback at $A2:A579.</summary>
        public static readonly SnesAddress OwtchShot = new(Bank, 0xa579);
        /// <summary>Dragon touch callback at $A2:E7C8.</summary>
        public static readonly SnesAddress DragonTouch = new(Bank, 0xe7c8);
        /// <summary>Dragon shot callback at $A2:E7CE.</summary>
        public static readonly SnesAddress DragonShot = new(Bank, 0xe7ce);
        /// <summary>Dragon Power Bomb callback at $A2:E7D4.</summary>
        public static readonly SnesAddress DragonPowerBomb = new(Bank, 0xe7d4);
        /// <summary>Shared G/Ripper 2 shot callback at $A2:E3A9.</summary>
        public static readonly SnesAddress GRipperRipper2Shot = new(Bank, 0xe3a9);
        /// <summary>Mama Turtle touch callback at $A2:9281.</summary>
        public static readonly SnesAddress MamaTurtleTouch = new(Bank, 0x9281);
        /// <summary>Baby Turtle touch callback at $A2:929F.</summary>
        public static readonly SnesAddress BabyTurtleTouch = new(Bank, 0x929f);
        /// <summary>Baby Turtle shot callback at $A2:930F.</summary>
        public static readonly SnesAddress BabyTurtleShot = new(Bank, 0x930f);
        /// <summary><c>MaridiaLargeSnailDamagingTouchAI</c> at $A2:D388.</summary>
        public static readonly SnesAddress MaridiaLargeSnailDamagingTouch = new(Bank, 0xd388);
        /// <summary><c>MaridiaLargeSnailNonDamagingTouchAI</c> at $A2:D38C.</summary>
        public static readonly SnesAddress MaridiaLargeSnailNonDamagingTouch = new(Bank, 0xd38c);
        /// <summary><c>MaridiaLargeSnailShotAI</c> at $A2:D3B4.</summary>
        public static readonly SnesAddress MaridiaLargeSnailShot = new(Bank, 0xd3b4);
        /// <summary>Rinka touch callback at $A2:B947.</summary>
        public static readonly SnesAddress RinkaTouch = new(Bank, 0xb947);
        /// <summary>Rinka shot callback at $A2:B94D.</summary>
        public static readonly SnesAddress RinkaShot = new(Bank, 0xb94d);
        /// <summary>Rinka Power Bomb callback at $A2:B953.</summary>
        public static readonly SnesAddress RinkaPowerBomb = new(Bank, 0xb953);
        /// <summary>Vertical shutter touch callback at $A2:F09D.</summary>
        public static readonly SnesAddress VerticalShutterTouch = new(Bank, 0xf09d);
        /// <summary>Shootable vertical shutter shot callback at $A2:F0A2.</summary>
        public static readonly SnesAddress ShootableVerticalShutterShot = new(Bank, 0xf0a2);
        /// <summary>Destroyable vertical shutter shot callback at $A2:F0AA.</summary>
        public static readonly SnesAddress DestroyableVerticalShutterShot = new(Bank, 0xf0aa);
        /// <summary>Vertical shutter Power Bomb callback at $A2:F0B6.</summary>
        public static readonly SnesAddress VerticalShutterPowerBomb = new(Bank, 0xf0b6);
        /// <summary>Horizontal shutter touch callback at $A2:F3D8.</summary>
        public static readonly SnesAddress HorizontalShutterTouch = new(Bank, 0xf3d8);
        /// <summary>Horizontal shutter shot callback at $A2:F40E.</summary>
        public static readonly SnesAddress HorizontalShutterShot = new(Bank, 0xf40e);
        /// <summary>Horizontal shutter Power Bomb callback at $A2:F41A.</summary>
        public static readonly SnesAddress HorizontalShutterPowerBomb = new(Bank, 0xf41a);
    }

    /// <summary>Bank-$A3 enemy interaction callbacks.</summary>
    public static class BankA3
    {
        /// <summary>The enemy bank, $A3, whose code these callbacks run in.</summary>
        public const byte Bank = 0xa3;

        /// <summary>Skree shot callback at $A3:C7F5.</summary>
        public static readonly SnesAddress SkreeShot = new(Bank, 0xc7f5);
        /// <summary>Metaree shot callback at $A3:8B0F.</summary>
        public static readonly SnesAddress MetareeShot = new(Bank, 0x8b0f);
        /// <summary>Fireflea touch callback at $A3:8E6B.</summary>
        public static readonly SnesAddress FirefleaTouch = new(Bank, 0x8e6b);
        /// <summary>Fireflea Power Bomb callback at $A3:8E83.</summary>
        public static readonly SnesAddress FirefleaPowerBomb = new(Bank, 0x8e83);
        /// <summary>Fireflea shot callback at $A3:8E89.</summary>
        public static readonly SnesAddress FirefleaShot = new(Bank, 0x8e89);
        /// <summary>Mochtroid touch callback at $A3:A953.</summary>
        public static readonly SnesAddress MochtroidTouch = new(Bank, 0xa953);
        /// <summary>Mochtroid shot callback at $A3:A9A8.</summary>
        public static readonly SnesAddress MochtroidShot = new(Bank, 0xa9a8);
        /// <summary>Metroid touch callback at $A3:EDEB.</summary>
        public static readonly SnesAddress MetroidTouch = new(Bank, 0xedeb);
        /// <summary>Metroid shot callback at $A3:EF07.</summary>
        public static readonly SnesAddress MetroidShot = new(Bank, 0xef07);
        /// <summary>Metroid Power Bomb callback at $A3:F042.</summary>
        public static readonly SnesAddress MetroidPowerBomb = new(Bank, 0xf042);
        /// <summary>Yard touch callback at $A3:D3B0.</summary>
        public static readonly SnesAddress YardTouch = new(Bank, 0xd3b0);
        /// <summary>Yard shot callback at $A3:D469.</summary>
        public static readonly SnesAddress YardShot = new(Bank, 0xd469);
        /// <summary>Platform no-op touch callback at $A3:9F07.</summary>
        public static readonly SnesAddress PlatformNoOpTouch = new(Bank, 0x9f07);
        /// <summary>Tripper shot callback at $A3:9F08.</summary>
        public static readonly SnesAddress TripperShot = new(Bank, 0x9f08);
    }

    /// <summary>Bank-$A4 Crocomire interaction callbacks.</summary>
    public static class BankA4
    {
        /// <summary>The enemy bank, $A4, whose code these callbacks run in.</summary>
        public const byte Bank = 0xa4;

        /// <summary>Crocomire header touch callback at $A4:B950.</summary>
        public static readonly SnesAddress HeaderTouch = new(Bank, 0xb950);
        /// <summary>Crocomire claw touch callback at $A4:B93D.</summary>
        public static readonly SnesAddress ClawTouch = new(Bank, 0xb93d);
        /// <summary>Crocomire no-op hitbox shot callback at $A4:B951.</summary>
        public static readonly SnesAddress NoOpHitboxShot = new(Bank, 0xb951);
        /// <summary>Crocomire dust hitbox shot callback at $A4:B968.</summary>
        public static readonly SnesAddress DustHitboxShot = new(Bank, 0xb968);
        /// <summary>Crocomire mouth shot callback at $A4:BA05.</summary>
        public static readonly SnesAddress MouthShot = new(Bank, 0xba05);
        /// <summary>Alternate Crocomire dust hitbox shot callback at $A4:BAB4.</summary>
        public static readonly SnesAddress AlternateDustHitboxShot = new(Bank, 0xbab4);
        /// <summary>Crocomire Power Bomb callback at $A4:B992.</summary>
        public static readonly SnesAddress PowerBomb = new(Bank, 0xb992);
    }

    /// <summary>Bank-$A5 boss interaction callbacks.</summary>
    public static class BankA5
    {
        /// <summary>The enemy bank, $A5, whose code these callbacks run in.</summary>
        public const byte Bank = 0xa5;

        /// <summary>Spore Spawn touch callback at $A5:EDEC.</summary>
        public static readonly SnesAddress SporeSpawnTouch = new(Bank, 0xedec);
        /// <summary>Spore Spawn shot callback at $A5:ED5A.</summary>
        public static readonly SnesAddress SporeSpawnShot = new(Bank, 0xed5a);
        /// <summary>Draygon touch callback at $A5:95EA.</summary>
        public static readonly SnesAddress DraygonTouch = new(Bank, 0x95ea);
        /// <summary>Draygon shot callback at $A5:95F0.</summary>
        public static readonly SnesAddress DraygonShot = new(Bank, 0x95f0);
        /// <summary>Draygon Power Bomb callback at $A5:9607.</summary>
        public static readonly SnesAddress DraygonPowerBomb = new(Bank, 0x9607);
    }

    /// <summary>Bank-$A6 Ceres, Norfair boss, and Zebetite callbacks.</summary>
    public static class BankA6
    {
        /// <summary>The enemy bank, $A6, whose code these callbacks run in.</summary>
        public const byte Bank = 0xa6;

        /// <summary>Fake Kraid touch callback at $A6:9C22.</summary>
        public static readonly SnesAddress FakeKraidTouch = new(Bank, 0x9c22);
        /// <summary>Fake Kraid shot callback at $A6:9C39.</summary>
        public static readonly SnesAddress FakeKraidShot = new(Bank, 0x9c39);
        /// <summary>Ceres steam touch callback at $A6:F03F.</summary>
        public static readonly SnesAddress CeresSteamTouch = new(Bank, 0xf03f);
        /// <summary>Ridley extended-spritemap touch callback at $A6:DF59.</summary>
        public static readonly SnesAddress RidleyExtendedTouch = new(Bank, 0xdf59);
        /// <summary>Ridley shot callback at $A6:DF8A.</summary>
        public static readonly SnesAddress RidleyShot = new(Bank, 0xdf8a);
        /// <summary>Ridley Power Bomb callback at $A6:DFB2.</summary>
        public static readonly SnesAddress RidleyPowerBomb = new(Bank, 0xdfb2);
        /// <summary>Zebetite touch callback at $A6:FDA7.</summary>
        public static readonly SnesAddress ZebetiteTouch = new(Bank, 0xfda7);
        /// <summary>Zebetite shot callback at $A6:FDAC.</summary>
        public static readonly SnesAddress ZebetiteShot = new(Bank, 0xfdac);
    }

    /// <summary>Bank-$A7 Kraid interaction callbacks.</summary>
    public static class BankA7
    {
        /// <summary>The enemy bank, $A7, whose code these callbacks run in.</summary>
        public const byte Bank = 0xa7;

        /// <summary>Phantoon body touch callback.</summary>
        public static readonly SnesAddress PhantoonTouch = new(Bank, 0xdd95);

        /// <summary>Phantoon body shot callback.</summary>
        public static readonly SnesAddress PhantoonShot = new(Bank, 0xdd9b);

        /// <summary>Kraid background/foot touch callback at $A7:948B.</summary>
        public static readonly SnesAddress KraidBackgroundTouch = new(Bank, 0x948b);
        /// <summary>Kraid arm touch callback at $A7:9490.</summary>
        public static readonly SnesAddress KraidArmTouch = new(Bank, 0x9490);
        /// <summary>Kraid no-op shot callback at $A7:94B5.</summary>
        public static readonly SnesAddress KraidNoOpShot = new(Bank, 0x94b5);
        /// <summary>Kraid arm shot callback at $A7:94B6.</summary>
        public static readonly SnesAddress KraidArmShot = new(Bank, 0x94b6);
        /// <summary>EnemyTouch_KraidNail at $A7:BCCF: normal touch followed by enemy death.</summary>
        public static readonly SnesAddress KraidNailTouch = new(Bank, 0xbccf);
        /// <summary>EnemyTouch_KraidNailBad at $A7:BCDE: normal touch followed by enemy death.</summary>
        public static readonly SnesAddress KraidBadNailTouch = new(Bank, 0xbcde);
    }

    /// <summary>Bank-$A8 ordinary enemy interaction callbacks.</summary>
    public static class BankA8
    {
        /// <summary>The enemy bank, $A8, whose code these callbacks run in.</summary>
        public const byte Bank = 0xa8;

        /// <summary>Evir touch callback at $A8:8B06.</summary>
        public static readonly SnesAddress EvirTouch = new(Bank, 0x8b06);
        /// <summary>Evir Power Bomb callback at $A8:8B0C.</summary>
        public static readonly SnesAddress EvirPowerBomb = new(Bank, 0x8b0c);
        /// <summary>Evir shot callback at $A8:8B12.</summary>
        public static readonly SnesAddress EvirShot = new(Bank, 0x8b12);
        /// <summary>Magdollite Power Bomb callback at $A8:B400.</summary>
        public static readonly SnesAddress MagdollitePowerBomb = new(Bank, 0xb400);
        /// <summary>Magdollite touch callback at $A8:B406.</summary>
        public static readonly SnesAddress MagdolliteTouch = new(Bank, 0xb406);
        /// <summary>Magdollite shot callback at $A8:B40C.</summary>
        public static readonly SnesAddress MagdolliteShot = new(Bank, 0xb40c);
        /// <summary>Beetom touch callback at $A8:BE2E.</summary>
        public static readonly SnesAddress BeetomTouch = new(Bank, 0xbe2e);
        /// <summary>Beetom shot callback at $A8:BEAC.</summary>
        public static readonly SnesAddress BeetomShot = new(Bank, 0xbeac);
        /// <summary>Powamp touch callback at $A8:C5BE.</summary>
        public static readonly SnesAddress PowampTouch = new(Bank, 0xc5be);
        /// <summary>Powamp shot callback at $A8:C5EF.</summary>
        public static readonly SnesAddress PowampShot = new(Bank, 0xc5ef);
        /// <summary>Powamp Power Bomb callback at $A8:C63F.</summary>
        public static readonly SnesAddress PowampPowerBomb = new(Bank, 0xc63f);
        /// <summary>Work robot touch callback at $A8:D174.</summary>
        public static readonly SnesAddress WorkRobotTouch = new(Bank, 0xd174);
        /// <summary>Work robot no-power shot callback at $A8:D18D.</summary>
        public static readonly SnesAddress WorkRobotNoPowerShot = new(Bank, 0xd18d);
        /// <summary>Work robot powered shot callback at $A8:D192.</summary>
        public static readonly SnesAddress WorkRobotShot = new(Bank, 0xd192);
        /// <summary>Bull shot callback at $A8:DB14.</summary>
        public static readonly SnesAddress BullShot = new(Bank, 0xdb14);
        /// <summary>Kago shot callback at $A8:AB83.</summary>
        public static readonly SnesAddress KagoShot = new(Bank, 0xab83);
        /// <summary>KiHunter shot callback at $A8:F701.</summary>
        public static readonly SnesAddress KiHunterShot = new(Bank, 0xf701);
        /// <summary>Spark shot callback at $A8:E70E.</summary>
        public static readonly SnesAddress SparkShot = new(Bank, 0xe70e);
        /// <summary><c>BlueBrinstarFaceBlockShotAI</c> at $A8:E91D.</summary>
        public static readonly SnesAddress BlueBrinstarFaceBlockShot = new(Bank, 0xe91d);
        /// <summary>Yapping Maw touch callback at $A8:A799.</summary>
        public static readonly SnesAddress YappingMawTouch = new(Bank, 0xa799);
        /// <summary>Yapping Maw shot callback at $A8:A7BD.</summary>
        public static readonly SnesAddress YappingMawShot = new(Bank, 0xa7bd);
    }

    /// <summary>Bank-$A9 Mother Brain and dead-Tourian interaction callbacks.</summary>
    public static class BankA9
    {
        /// <summary>The enemy bank, $A9, whose code these callbacks run in.</summary>
        public const byte Bank = 0xa9;

        /// <summary>Dead Zoomer's touch and shot rot trigger at $A9:DCF8.</summary>
        public static readonly SnesAddress DeadZoomerTouchAndShot = new(Bank, 0xdcf8);
        /// <summary>Dead Zoomer's Power Bomb rot trigger at $A9:DCED.</summary>
        public static readonly SnesAddress DeadZoomerPowerBomb = new(Bank, 0xdced);
        /// <summary>Dead Ripper's touch and shot rot trigger at $A9:DD08.</summary>
        public static readonly SnesAddress DeadRipperTouchAndShot = new(Bank, 0xdd08);
        /// <summary>Dead Ripper's Power Bomb rot trigger at $A9:DCFD.</summary>
        public static readonly SnesAddress DeadRipperPowerBomb = new(Bank, 0xdcfd);
        /// <summary>Dead Skree's touch and shot rot trigger at $A9:DD18.</summary>
        public static readonly SnesAddress DeadSkreeTouchAndShot = new(Bank, 0xdd18);
        /// <summary>Dead Skree's Power Bomb rot trigger at $A9:DD0D.</summary>
        public static readonly SnesAddress DeadSkreePowerBomb = new(Bank, 0xdd0d);
        /// <summary>Mother Brain body shot callback at $A9:B503.</summary>
        public static readonly SnesAddress MotherBrainBodyShot = new(Bank, 0xb503);
        /// <summary>Mother Brain head shot callback at $A9:B507.</summary>
        public static readonly SnesAddress MotherBrainHeadShot = new(Bank, 0xb507);
        /// <summary>Mother Brain body touch callback at $A9:B5C5, a bare RTL.</summary>
        public static readonly SnesAddress MotherBrainBodyTouch = new(Bank, 0xb5c5);
        /// <summary>Mother Brain head touch callback at $A9:B5C6.</summary>
        public static readonly SnesAddress MotherBrainHeadTouch = new(Bank, 0xb5c6);
        /// <summary>Dead Torizo touch/shot callback at $A9:D433.</summary>
        public static readonly SnesAddress DeadTorizoTouchAndShot = new(Bank, 0xd433);
        /// <summary>Dead Torizo Power Bomb callback at $A9:D42A.</summary>
        public static readonly SnesAddress DeadTorizoPowerBomb = new(Bank, 0xd42a);
        /// <summary>Dead Sidehopper touch callback at $A9:DD44.</summary>
        public static readonly SnesAddress DeadSidehopperTouch = new(Bank, 0xdd44);
        /// <summary>Dead Sidehopper shot callback at $A9:DD1D.</summary>
        public static readonly SnesAddress DeadSidehopperShot = new(Bank, 0xdd1d);
        /// <summary>Dead Sidehopper Power Bomb callback at $A9:D8CC.</summary>
        public static readonly SnesAddress DeadSidehopperPowerBomb = new(Bank, 0xd8cc);
        /// <summary>Shitroid touch callback at $A9:F789.</summary>
        public static readonly SnesAddress ShitroidTouch = new(Bank, 0xf789);
        /// <summary>Shitroid shot callback at $A9:F842.</summary>
        public static readonly SnesAddress ShitroidShot = new(Bank, 0xf842);
        /// <summary>Shitroid Power Bomb callback at $A9:EFBA.</summary>
        public static readonly SnesAddress ShitroidPowerBomb = new(Bank, 0xefba);
    }

    /// <summary>Bank-$AA Torizo/Shaktool interaction callbacks.</summary>
    public static class BankAA
    {
        /// <summary>The enemy bank, $AA, whose code these callbacks run in.</summary>
        public const byte Bank = 0xaa;

        /// <summary>Bomb Torizo touch callback at $AA:C977.</summary>
        public static readonly SnesAddress BombTorizoTouch = new(Bank, 0xc977);
        /// <summary>Bomb Torizo shot callback at $AA:C97C.</summary>
        public static readonly SnesAddress BombTorizoShot = new(Bank, 0xc97c);
        /// <summary>Torizo stand-up/sit-down shot callback at $AA:C9C2.</summary>
        public static readonly SnesAddress TorizoStandUpSitDownShot = new(Bank, 0xc9c2);
        /// <summary>Golden Torizo shot callback at $AA:D667.</summary>
        public static readonly SnesAddress GoldenTorizoShot = new(Bank, 0xd667);
        /// <summary>Shaktool touch callback at $AA:DF2F.</summary>
        public static readonly SnesAddress ShaktoolTouch = new(Bank, 0xdf2f);
        /// <summary>Shaktool shot callback at $AA:DF34.</summary>
        public static readonly SnesAddress ShaktoolShot = new(Bank, 0xdf34);
    }

    /// <summary>Bank-$B2 Space Pirate interaction callbacks.</summary>
    public static class BankB2
    {
        /// <summary>The enemy bank, $B2, whose code these callbacks run in.</summary>
        public const byte Bank = 0xb2;

        /// <summary>Space Pirate Power Bomb callback at $B2:8767.</summary>
        public static readonly SnesAddress PowerBomb = new(Bank, 0x8767);
        /// <summary>Space Pirate touch callback at $B2:876C.</summary>
        public static readonly SnesAddress Touch = new(Bank, 0x876c);
        /// <summary>Space Pirate shot callback at $B2:8779.</summary>
        public static readonly SnesAddress Shot = new(Bank, 0x8779);
        /// <summary>Gold ninja Space Pirate vulnerable-hitbox shot callback at $B2:87C8.</summary>
        public static readonly SnesAddress GoldNinjaVulnerableHitboxShot = new(Bank, 0x87c8);
        /// <summary>Gold ninja Space Pirate invincible-hitbox shot callback at $B2:883E.</summary>
        public static readonly SnesAddress GoldNinjaInvincibleHitboxShot = new(Bank, 0x883e);
    }

    /// <summary>Bank-$B3 Botwoon interaction callbacks.</summary>
    public static class BankB3
    {
        /// <summary>The enemy bank, $B3, whose code these callbacks run in.</summary>
        public const byte Bank = 0xb3;

        /// <summary>Botwoon touch callback at $B3:9FFF.</summary>
        public static readonly SnesAddress BotwoonTouch = new(Bank, 0x9fff);
        /// <summary>Botwoon shot callback at $B3:A016.</summary>
        public static readonly SnesAddress BotwoonShot = new(Bank, 0xa016);
        /// <summary>Botwoon Power Bomb callback at $B3:A041.</summary>
        public static readonly SnesAddress BotwoonPowerBomb = new(Bank, 0xa041);
    }

    /// <summary>
    /// The bank-local long returns at $804C that enemy headers name as their initialization or main AI to run nothing:
    /// <c>RTL_A2804C</c>, <c>RTL_A3804C</c> (the respawn placeholder) and <c>RTL_AA804C</c> (the Tourian statue ghost).
    /// Any other initialization or main-AI word is the species' own routine.
    /// </summary>
    public static readonly IReadOnlySet<SnesAddress> CommonNoOpAiRoutines = new HashSet<SnesAddress>
    {
        new(BankA2.Bank, 0x804c),
        new(BankA3.Bank, 0x804c),
        new(BankAA.Bank, 0x804c),
    };
}
