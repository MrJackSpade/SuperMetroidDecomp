using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    private static int VerifyMotherBrainPlasmaImpact()
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var enemies = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        var head = enemies.Slots[1];
        head.EnemyDefinitionPointer = 0xec3f;
        head.Definition = RoomEnemyDefinitionCatalog.Get(head.EnemyDefinitionPointer);
        head.Properties = head.Properties.With(EnemyProperties.BlocksPlasmaBeam);
        head.XPosition = head.YPosition = 128;
        head.XRadius = head.YRadius = 16;
        head.SpritemapPointer = 1;
        head.Health = 18000;
        var state = new MotherBrainEnemyState(enemies.Slots[0]) { Head = head, Form = 2, WalkCounter = 0x1000 };
        typeof(RoomEnemySystem).GetField("_motherBrain", flags)!.SetValue(enemies, state);
        ((List<ushort>)typeof(RoomEnemySystem).GetField("_interactiveEnemyIndexes", flags)!.GetValue(enemies)!).Add(head.NativeIndex);
        var samus = new SamusState();
        var installation = RepositoryInstallation.Installation;
        var shots = new SamusProjectileSystem { FrameBindings = RepositoryInstallation.Projectiles.FrameBindings };
        var bombs = new SamusBombProjectileSystem();
        var shot = shots.Slots[0];
        SeedChargedPlasma();
        AssertEqual(1, Hit(), "Charged Plasma produces one accepted head impact");
        AssertEqual((ushort)17550, head.Health, "Native charged vulnerability applies exactly one 450-damage hit");
        // The bank-$A0 walker only marks the shot; the next projectile pass removes it (#1269).
        AssertTrue(shot.PackedDirection.HasLowByteLifecycleState,
            "Mother Brain plasma-blocking property marks the shot for removal");
        AssertEqual((ushort)0xf00, state.WalkCounter, "Accepted beam preserves native recoil bookkeeping");
        AssertEqual(EnemyShotTiming.PlasmaInvincibilityFrames, head.InvincibilityTimer,
            "The no-death damage tail grants Plasma's hit immunity");
        var level = CreateRoom(16, 16, new ushort[256], new byte[256]);
        for (int frame = 0; frame < 3; frame++)
        {
            shots.StepFrame(bus, level, samus, 0, 0, 0, 0, bombs, projectileProducerEnabled: false);
            AssertEqual(0, Hit(), "The same shot cannot damage the head on a subsequent frame");
            AssertEqual((ushort)17550, head.Health, "Later overlap preserves health after the one impact");
            AssertEqual((ushort)0xf00, state.WalkCounter, "Later overlap does not repeat recoil");
        }
        // Later checks are new shots after that immunity has run out.
        head.InvincibilityTimer = 0;
        state.Form = 0;
        SeedChargedPlasma();
        Hit();
        AssertEqual((ushort)17550, head.Health, "First form retains its missile-only damage restriction");
        head.InvincibilityTimer = 0;
        state.Form = 4;
        state.RainbowBeamSequence = new MotherBrainRainbowBeamAttackSequence();
        state.RainbowBeamSequence.Body.Form = 4;
        SeedChargedPlasma();
        shot.Type = 0x9018; shot.Damage = 1000; shot.PreInstruction = SamusProjectilePreInstruction.HyperBeam;
        // The first case's Plasma hit left the head invincible; $A0:A17C skips it until the
        // timer expires, so this case starts once that immunity has run out.
        head.InvincibilityTimer = 0;
        AssertEqual(1, Hit(), "Hyper Beam still accepts its later-form damage callback");
        AssertEqual((ushort)16550, head.Health, "Hyper Beam retains its native charged damage selection");
        AssertEqual(MotherBrainPhase3NeckPhase.SetupHyperBeamRecoil, state.RainbowBeamSequence.Phase3NeckPhase,
            "Hyper Beam retains its phase-three recoil transition");
        AssertTrue(shot.PackedDirection.HasLowByteLifecycleState, "Hyper Beam also obeys the head collision property");
        Console.WriteLine("Mother Brain Plasma: one charged impact/damage event, no later-frame repeats, recoil and first-form restriction passed.");
        return 0;

        int Hit() => enemies.ResolveOrdinaryProjectileHits(bus, shots, bombs, samus);
        void SeedChargedPlasma()
        {
            shot.Type = 0x8018; shot.Damage = 450; shot.Direction = 2;
            shot.XPosition = shot.YPosition = 128; shot.XRadius = shot.YRadius = 4;
            shot.InstructionPointer = 0x9000; shot.InstructionTimer = 1;
            shot.PreInstruction = SamusProjectilePreInstruction.NoWaveBeam;
        }
    }
}
