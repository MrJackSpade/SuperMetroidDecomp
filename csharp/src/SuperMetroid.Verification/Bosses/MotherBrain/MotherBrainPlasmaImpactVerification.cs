using SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Core.Hardware;
using System.Reflection;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Mother Brain head damage and shot removal after a charged Plasma impact.</summary>
internal static class MotherBrainPlasmaImpactVerification
{
    /// <summary>Confirms one accepted hit, recoil, immunity, first-form restriction, and later Hyper Beam behavior.</summary>
    internal static int VerifyMotherBrainPlasmaImpact()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var enemies = new RoomEnemySystem();
        enemies.BindFixtureBus(bus);
        var head = enemies.SlotTable.Slots[1];
        head.Bind(EnemyDefinitionId.MotherBrainHead, head.Definition);
        head.Bind(head.EnemyDefinitionPointer, RoomEnemyDefinitionCatalog.Get(head.EnemyDefinitionPointer));
        head.Properties = head.Properties.With(EnemyProperties.BlocksPlasmaBeam);
        head.Position = head.Position.WithWhole(new(128, 128));
        head.Radius = new(16, 16);
        head.SpritemapPointer = 1;
        head.Health = 18000;
        var state = new MotherBrainEnemyState(enemies.SlotTable.Slots[0]) { Head = head, Form = 2, WalkCounter = 0x1000 };
        enemies.MotherBrainEncounter.MotherBrain = state;
        ((List<ushort>)enemies.ProcessingWindow._interactiveEnemyIndexes).Add(head.NativeIndex);
        var samus = new SamusState();
        var installation = RepositoryInstallation.Installation;
        var shots = new SamusProjectileSystem { Animation = { FrameBindings = InstalledFixtures.Projectiles.FrameBindings } };
        var bombs = new SamusBombProjectileSystem();
        var shot = shots.Slots[0];
        SeedChargedPlasma();
        AssertEqual(1, Hit(), "Charged Plasma produces one accepted head impact");
        AssertEqual((ushort)17550, head.Health, "Native charged vulnerability applies exactly one 450-damage hit");
        // The bank-$A0 walker only marks the shot; the next projectile pass removes it (#1269).
        AssertTrue(shot.PackedDirection.HasLowByteLifecycleState,
            "Mother Brain plasma-blocking property marks the shot for removal");
        AssertEqual((ushort)0xf00, state.WalkCounter, "Accepted beam preserves native recoil bookkeeping");
        AssertEqual(EnemyShotTiming.PlasmaInvincibilityFrames, head.InvincibilityTimer.RawValue,
            "The no-death damage tail grants Plasma's hit immunity");
        var level = CreateRoom(new(16, 16), new ushort[256], new byte[256]);
        for (int frame = 0; frame < 3; frame++)
        {
            shots.StepFrame(new SamusTerrain { Bus = bus, Level = level }, samus, new ProjectileButtonInput { ControllerInput = 0, ControllerNewInput = 0 }, Position16<RoomPixels>.Zero, bombs, projectileProducerEnabled: false);
            AssertEqual(0, Hit(), "The same shot cannot damage the head on a subsequent frame");
            AssertEqual((ushort)17550, head.Health, "Later overlap preserves health after the one impact");
            AssertEqual((ushort)0xf00, state.WalkCounter, "Later overlap does not repeat recoil");
        }
        // Later checks are new shots after that immunity has run out.
        head.InvincibilityTimer = NativeCountdown.Start(0);
        state.Form = 0;
        SeedChargedPlasma();
        Hit();
        AssertEqual((ushort)17550, head.Health, "First form retains its missile-only damage restriction");
        head.InvincibilityTimer = NativeCountdown.Start(0);
        state.Form = 4;
        state.RainbowBeamSequence = new MotherBrainRainbowBeamAttackSequence();
        state.RainbowBeamSequence.Body.Form = 4;
        SeedChargedPlasma();
        shot.Type = 0x9018; shot.Damage = 1000; shot.PreInstruction = SamusProjectilePreInstruction.HyperBeam;
        // The first case's Plasma hit left the head invincible; $A0:A17C skips it until the
        // timer expires, so this case starts once that immunity has run out.
        head.InvincibilityTimer = NativeCountdown.Start(0);
        AssertEqual(1, Hit(), "Hyper Beam still accepts its later-form damage callback");
        AssertEqual((ushort)16550, head.Health, "Hyper Beam retains its native charged damage selection");
        AssertEqual(MotherBrainPhase3NeckPhase.SetupHyperBeamRecoil, state.RainbowBeamSequence.Phase3NeckPhase,
            "Hyper Beam retains its phase-three recoil transition");
        AssertTrue(shot.PackedDirection.HasLowByteLifecycleState, "Hyper Beam also obeys the head collision property");
        Console.WriteLine("Mother Brain Plasma: one charged impact/damage event, no later-frame repeats, recoil and first-form restriction passed.");
        return 0;

        int Hit() => enemies.Collision.ResolveOrdinaryProjectileHits(shots, bombs, samus);
        void SeedChargedPlasma()
        {
            shot.Type = 0x8018; shot.Damage = 450; shot.Direction = 2;
            shot.Position = shot.Position.WithWhole(new(128, 128)); shot.Radius = new(4, 4);
            shot.Cursor = ProjectileFixtureCursors.Active; shot.InstructionTimer = NativeCountdown.Start(1);
            shot.PreInstruction = SamusProjectilePreInstruction.NoWaveBeam;
        }
    }

    /// <summary>Runs the focused Mother Brain Plasma collision reproduction.</summary>
    [VerificationCommand("--mother-brain-plasma-impact")]
    internal static int MotherBrainPlasmaImpactCommand()
    {
        return VerifyMotherBrainPlasmaImpact();
    }
}
