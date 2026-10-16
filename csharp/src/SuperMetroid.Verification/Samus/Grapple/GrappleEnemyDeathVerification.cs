using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Enemies.EnemyDropsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus.Grapple;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Samus.Grapple;
using SuperMetroid.Verification.Harness;

/// <summary>Checks grapple-killed enemy death, explosion, drops, sound, and grapple cleanup.</summary>
internal static class GrappleEnemyDeathVerification
{
    /// <summary>Exercises the native kill route through its visible projectile animation and pickup conversion.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native OAM tables")]
    internal static void VerifyGrappleEnemyDeath()
    {
        Suite(nameof(VerifyGrappleDeathCleanupOrdinaryPoses), () => VerifyGrappleDeathCleanupOrdinaryPoses());
        var samus = CreateDropTestSamus();
        samus.Health = 50;
        samus.Position = samus.Position.WithWhole(new(220, 180));
        var fixture = CreateEnemyDropFixture(samus, [1]);
        fixture.System.TileArtwork = InstalledFixtures.EnemyTiles;
        SeedGrappleEnemyDeathRetailBanks(fixture.Bus);
        const ushort header = 0x9000;
        SeedEnemyDropChancePointer(fixture.Bus, header);
        var enemy = fixture.System.SlotTable.Slots[0];
        enemy.Bind((EnemyDefinitionId)header, enemy.Definition);
        enemy.Bind(enemy.EnemyDefinitionPointer, default(RoomEnemyDefinition) with
        {
            GrappleAiPointer = GrappleAiRoutine.Kill,
            DeathAnimation = 4,
        });
        enemy.Position = enemy.Position.WithWhole(new(100, 100));
        enemy.Radius = new(8, 8);
        enemy.Health = 500;
        fixture.System.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = true, Samus = samus, Level = fixture.Level });
        var contact = fixture.System.GrappleBehavior.ResolveGrappleEndpoint(new(100, 100));
        AssertEqual(GrappleEnemyReaction.Kill, contact.Reaction, "header selects grapple kill independently of enemy health");
        samus.Grapple.Phase = GrapplePhase.Firing;
        fixture.System.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = false, Samus = samus, Level = fixture.Level });
        var explosion = fixture.System.Projectiles.EnemyProjectiles.Single(p => p.IsActive);
        AssertEqual(RoomEnemyProjectileKind.EnemyDeathExplosion, explosion.Kind, "grapple kill creates the ordinary death actor");
        AssertEqual(1, fixture.System.RoomStatus.EnemiesKilled, "grapple death increments kill count once");
        AssertEqual(GrapplePhase.Dropped, samus.Grapple.Phase, "native death routine drops the active grapple");
        AssertEqual(100, explosion.Position.X.Whole, "death explosion preserves enemy X");
        AssertEqual(100, explosion.Position.Y.Whole, "death explosion preserves enemy Y");
        AssertEqual((EnemyDefinitionId)header, explosion.EnemyHeaderPointer, "death actor retains drop-table identity");
        var spriteFrames = new HashSet<string>();
        bool sawPickup = false;
        bool heardDeath = false;
        for (int frame = 0; frame < 100; frame++)
        {
            fixture.System.StepEnemyProjectiles(fixture.Level, samus);
            // This focused projectile pass publishes the legacy sound field; the
            // full enemy-frame adapter forwards it to library two with Max1.
            heardDeath |= fixture.System.DeathsAndDrops.LastEnemyDeathSoundEffectLibrary2 == 9;
            if (explosion.Variable0 == (ushort)EnemyPickupKind.SmallEnergy * 2)
            {
                AssertEqual(31, frame, "small explosion uses the cartridge's complete 31-frame animation");
                sawPickup = true;
                break;
            }
            var oam = new OamBuffer();
            fixture.System.DrawEnemyProjectiles(oam, Position16<RoomPixels>.Zero);
            AssertTrue(oam.NextByteOffset > 0, $"grapple death explosion is visible on frame {frame}");
            spriteFrames.Add(Convert.ToHexString(oam.LowTable[..oam.NextByteOffset]));
        }
        AssertEqual(6, spriteFrames.Count, "grapple death displays all six retail small-explosion frames");
        AssertTrue(sawPickup, "retail death instruction list converts the explosion into a pickup");
        AssertTrue(heardDeath,
            "grapple death emits the retail enemy-killed sound from its instruction list");
        Console.WriteLine("  Grapple enemy death: visible animated explosion, drop conversion, position and kill counter agree.");
    }

    /// <summary>Reads a retail cartridge byte while seeding the focused synthetic death fixture.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for grapple enemy death")]
    private static byte GrappleEnemyDeathRomByte(ISnesAddressSpace rom, int address) => rom.ReadCartridgeByte(address);

    /// <summary>Copies the retail enemy-projectile ($86) and sprite-map ($8D) banks into the synthetic cartridge.</summary>
    [CpuBusBoundary("exact bus test: synthetic cartridge fixture copying retail banks $86 and $8D for grapple enemy death")]
    private static void SeedGrappleEnemyDeathRetailBanks(TestAddressSpace bus)
    {
        var retail = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (int bank in new[] { 0x860000, 0x8d0000 })
        {
            var data = new byte[0x8000];
            for (int i = 0; i < data.Length; i++) data[i] = GrappleEnemyDeathRomByte(retail, bank + 0x8000 + i);
            bus.WriteBytes(bank + 0x8000, data);
        }
    }

    /// <summary>Checks grapple release and movement cleanup for the reported standing, aiming, and jumping poses.</summary>
    private static void VerifyGrappleDeathCleanupOrdinaryPoses()
    {
        var retail = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (SamusPoseId pose in new[] { SamusPoseId.StandingAimDiagonalDownRightPose,
            SamusPoseId.NormalJumpForwardRightPose, SamusPoseId.FacingRightNormalPose })
        {
            var samus = CreateDropTestSamus();
            samus.Pose = pose;
            samus.PoseMetadata.RefreshCollisionRadii();
            // The logged 51->01 request used the standing-size live radius, retained
            // across a pose handoff; C8C5 branches on that word, not pose metadata.
            samus.Kinematics.Radius = samus.Kinematics.Radius.WithY(17);
            samus.Position = samus.Position.WithWhole(new(220, 100));
            var fixture = CreateEnemyDropFixture(samus, [1]);
        fixture.System.TileArtwork = InstalledFixtures.EnemyTiles;
            var enemy = fixture.System.SlotTable.Slots[0];
            enemy.Bind((EnemyDefinitionId)0x9000, enemy.Definition);
            enemy.Bind(enemy.EnemyDefinitionPointer, default(RoomEnemyDefinition) with
            {
                GrappleAiPointer = GrappleAiRoutine.Kill,
            });
            enemy.Position = enemy.Position.WithWhole(new(100, 100));
            enemy.Radius = new(8, 8);
            enemy.Health = 500;
            fixture.System.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = true, Samus = samus, Level = fixture.Level });
            fixture.System.GrappleBehavior.ResolveGrappleEndpoint(new(100, 100));
            samus.Grapple.Phase = GrapplePhase.Firing;
            fixture.System.StepFrame(new RoomEnemyFrame { Camera = new(0, 0), TimeIsFrozen = false, Samus = samus, Level = fixture.Level });
            AssertEqual(GrapplePhase.Dropped, samus.Grapple.Phase, "enemy death queues drop while ordinary pose remains active");
            var movementRoom = CreateRoom(new(32, 32), new ushort[32 * 32], new byte[32 * 32]);
            var result = SamusGrappleMovement.Step(new SamusTerrain { Bus = retail, Level = movementRoom }, samus, 0, 0);
            AssertEqual(GrapplePhase.Inactive, result.Phase, $"reported ordinary pose {(int)pose:X2} completes grapple cleanup");
            AssertEqual(GrapplePhase.Inactive, samus.Grapple.Phase, "cleanup releases grapple ownership");
            AssertEqual(pose == SamusPoseId.StandingAimDiagonalDownRightPose ? pose : SamusPoseId.FacingRightNormalPose,
                samus.Pose, "native dropped table selects aim or standing body");
            AssertEqual(0, samus.Kinematics.YSpeed, "drop clears vertical speed");
            AssertEqual(0, samus.HorizontalSpeed.BaseSpeed, "drop clears base horizontal speed");
        }
        Console.WriteLine("  Grapple enemy-death cleanup: reported 07->07, 51->01, and 01->01 routes pass.");
    }

    /// <summary>Dispatches the focused grapple enemy death verification command.</summary>
    [VerificationCommand("--grapple-enemy-death")]
    internal static int GrappleEnemyDeathCommand()
    {
        Suite(nameof(VerifyGrappleEnemyDeath), () => VerifyGrappleEnemyDeath());
        return 0;
    }
}
