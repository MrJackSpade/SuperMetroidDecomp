using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>
    /// Verifies the cartridge-safe left-facing Murder Beam against the bounded original-CPU
    /// probe. The unsafe directions enter unrelated native code and are asserted separately
    /// as unsupported rather than being disguised as an ordinary Wave projectile.
    /// </summary>
    private static void VerifyMurderBeam()
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var level = new RoomLevelData(
            16,
            16,
            new ushort[256],
            new byte[256],
            new ushort[256],
            new byte[8]);
        var samus = new SamusState
        {
            Pose = 2,
            XPosition = 128,
            YPosition = 128,
            CollectedItems = (ushort)SamusEquipmentFlags.HiJumpBoots,
            EquippedItems = (ushort)SamusEquipmentFlags.HiJumpBoots,
            CollectedBeams = 0x100f,
            EquippedBeams = 0x1007,
            SelectedHudItem = 0,
        };
        var projectiles = new SamusProjectileSystem();
        var shared = new SamusBombProjectileSystem();

        HoldCharge(bus, level, samus, projectiles, shared);

        var pause = new PauseMenuState(
            bus,
            samus,
            new Bank80SystemState(),
            AreaId.Crateria,
            0,
            0);
        pause.Step((ushort)SnesButton.R, (ushort)SnesButton.R);
        for (int frame = 0; frame < 32; frame++)
            pause.Step(0, 0);
        pause.Step(0, (ushort)SnesButton.Right);
        for (int step = 0; pause.SelectedCategory != 3 && step < 6; step++)
            pause.Step(0, (ushort)SnesButton.Down);
        AssertEqual(3, pause.SelectedCategory,
            "Murder Beam setup moves the equipment marker to Boots");
        pause.Step(0, (ushort)(SnesButton.Left | SnesButton.A));
        AssertEqual((ushort)0x100f, samus.EquippedBeams,
            "same-frame Boots Left+A equips all four beams while retaining Charge");

        shared.StepFrame(bus, level, samus, 0, 0);
        SamusProjectileFrameResult fired = projectiles.StepFrame(
            bus,
            level,
            samus,
            0,
            0,
            0,
            0,
            shared);
        AssertEqual((int?)0, fired.FiredSlot, "charged-left Murder Beam allocates slot zero");
        AssertTrue(fired.QueuedSoundEffect is null,
            "Murder Beam's adjacent sound-table entry is zero");

        SamusProjectileSlot shot = projectiles.Slots[0];
        AssertEqual((ushort)0x901f, shot.Type, "Murder Beam retains charged all-beams type");
        AssertEqual((ushort)200, shot.Damage, "Murder Beam uses the native 200-damage record");
        AssertEqual((ushort)SamusProjectileDirection.Left, shot.Direction,
            "safe Murder Beam faces left");
        AssertEqual((ushort)116, shot.XPosition, "Murder Beam native muzzle X");
        AssertEqual((ushort)123, shot.YPosition, "Murder Beam native muzzle Y");
        AssertEqual(SamusProjectilePreInstruction.MurderBeamMisalignedExecution,
            shot.PreInstruction, "Murder Beam retains its overread callback identity");
        AssertEqual((ushort)0, shot.InstructionPointer,
            "safe left-facing data record selects no animation list");
        AssertEqual((ushort)0, shot.XRadius, "safe Murder Beam X radius");
        AssertEqual((ushort)0, shot.YRadius, "safe Murder Beam Y radius");
        AssertEqual((short)0, shot.XVelocity, "safe Murder Beam X speed");
        AssertEqual((short)0, shot.YVelocity, "safe Murder Beam Y speed");
        AssertTrue(!shot.IsActive, "zero-list Murder Beam is not animated or drawn");
        AssertTrue(shot.HasEnemyCollisionPayload,
            "zero-list Murder Beam remains visible to bank-$A0 enemy collision");
        AssertEqual((ushort)1, projectiles.ProjectileCounter,
            "Murder Beam retains its ordinary projectile allocation");

        for (int frame = 0; frame < 16; frame++)
        {
            shared.StepFrame(bus, level, samus, 0, 0);
            projectiles.StepFrame(bus, level, samus, 0, 0, 0, 0, shared);
        }
        AssertEqual((ushort)0x901f, shot.Type, "Murder Beam persists without an instruction list");
        AssertEqual((ushort)200, shot.Damage, "persistent Murder Beam retains damage");
        AssertEqual((ushort)116, shot.XPosition, "persistent Murder Beam remains fixed in world X");
        AssertEqual((ushort)123, shot.YPosition, "persistent Murder Beam remains fixed in world Y");

        EnemyDropFixture combat = CreateEnemyDropFixture(samus, [1]);
        RoomEnemySlot target = combat.System.Slots[0];
        target.EnemyDefinitionPointer = 0x9000;
        target.Definition = default(RoomEnemyDefinition) with
        {
            Bank = 0xa3,
            ShotAiPointer = EnemyAiCodePointers.BankA0.NormalEnemyShot,
            VulnerabilityPointer = EnemyVulnerabilityDefinitions.DefaultPointer,
        };
        target.XPosition = shot.XPosition;
        target.YPosition = shot.YPosition;
        target.XRadius = target.YRadius = 16;
        target.Health = 1000;
        target.SpritemapPointer = 0x8000;
        combat.System.StepFrame(
            0,
            0,
            timeIsFrozen: true,
            samus,
            level: combat.Level);

        AssertEqual(1, combat.System.ResolveOrdinaryProjectileHits(
            combat.Bus, projectiles, shared, samus),
            "invisible Murder Beam point overlaps an ordinary enemy");
        AssertEqual((ushort)800, target.Health,
            "charged vulnerability applies one 200-point Murder Beam hit");
        AssertEqual((ushort)0x901f, shot.Type,
            "Plasma-bearing Murder Beam survives enemy impact");
        AssertEqual((ushort)0, shot.InstructionPointer,
            "Murder Beam remains zero-list after enemy impact");

        target.InvincibilityTimer = 0;
        AssertEqual(1, combat.System.ResolveOrdinaryProjectileHits(
            combat.Bus, projectiles, shared, samus),
            "persistent Murder Beam can damage again after invincibility");
        AssertEqual((ushort)600, target.Health,
            "second Murder Beam contact repeats the native damage");

        target.InvincibilityTimer = 0;
        target.FrozenTimer = 0;
        target.Health = 100;
        AssertEqual(1, combat.System.ResolveOrdinaryProjectileHits(
            combat.Bus, projectiles, shared, samus),
            "lethal Murder Beam contact reaches Ice substitution");
        AssertEqual((ushort)100, target.Health,
            "lethal Ice-bearing Murder Beam preserves health while freezing");
        AssertEqual((ushort)400, target.FrozenTimer,
            "Murder Beam freezes a vulnerable non-Norfair enemy");

        var phaseBoundary = new MotherBrainRainbowBeamAttackSequence();
        phaseBoundary.StartFinishOffSequence();
        phaseBoundary.BeginPhase3RecoveryFromBabyCutscene();
        AssertEqual(MotherBrainRainbowBeamAttackPhase.Phase3RecoverFromCutsceneMakeSomeDistance,
            phaseBoundary.Phase, "Mother Brain enters the phase-three recovery boundary");
        for (int frame = 0; frame < 120; frame++)
        {
            shared.StepFrame(bus, level, samus, 0, 0);
            projectiles.StepFrame(bus, level, samus, 0, 0, 0, 0, shared);
        }
        AssertEqual((ushort)0x901f, shot.Type,
            "Murder Beam survives the phase-two to phase-three cutscene boundary");
        AssertEqual((ushort)200, shot.Damage,
            "phase-three Murder Beam retains its damage payload");

        VerifyMissileAutoCancelMurderBeam(bus, level);
        VerifyUnsafeMurderBeamDirection(bus, level);
        VerifyUnsafeUnchargedMurderBeam(bus, level);
        Console.WriteLine(
            "Murder Beam: precharge and missile auto-cancel setups, left-facing zero-list " +
            "persistence through Mother Brain's phase boundary, repeated 200-point damage " +
            "and Ice substitution match bounded cartridge evidence.");
    }

    private static void HoldCharge(
        ISnesAddressSpace bus,
        RoomLevelData level,
        SamusState samus,
        SamusProjectileSystem projectiles,
        SamusBombProjectileSystem shared,
        bool shootAlreadyHeld = false)
    {
        for (int frame = 0; frame < SamusProjectileRomData.Beams.FullyChargedCounter; frame++)
        {
            ushort newlyPressed = frame == 0 && !shootAlreadyHeld
                ? (ushort)SnesButton.X
                : (ushort)0;
            shared.StepFrame(bus, level, samus, (ushort)SnesButton.X, newlyPressed);
            projectiles.StepFrame(
                bus,
                level,
                samus,
                (ushort)SnesButton.X,
                newlyPressed,
                0,
                0,
                shared);
        }
        AssertEqual(SamusProjectileRomData.Beams.FullyChargedCounter,
            projectiles.FlareCounter, "pre-pause beam charge reaches charged threshold");
    }

    private static void VerifyUnsafeMurderBeamDirection(
        ISnesAddressSpace bus,
        RoomLevelData level)
    {
        var samus = new SamusState
        {
            Pose = 1,
            XPosition = 128,
            YPosition = 128,
            EquippedBeams = 0x1007,
            SelectedHudItem = 0,
        };
        var projectiles = new SamusProjectileSystem();
        var shared = new SamusBombProjectileSystem();
        HoldCharge(bus, level, samus, projectiles, shared);
        samus.EquippedBeams = 0x100f;
        AssertThrows<NotSupportedException>(() =>
        {
            shared.StepFrame(bus, level, samus, 0, 0);
            projectiles.StepFrame(bus, level, samus, 0, 0, 0, 0, shared);
        }, "unsafe right-facing Murder Beam does not silently become an ordinary projectile");
    }

    private static void VerifyMissileAutoCancelMurderBeam(
        ISnesAddressSpace bus,
        RoomLevelData level)
    {
        var samus = new SamusState
        {
            Pose = 2,
            XPosition = 128,
            YPosition = 128,
            EquippedBeams = 0x100f,
            Missiles = 5,
        };
        var projectiles = new SamusProjectileSystem();
        var shared = new SamusBombProjectileSystem();
        ushort select = (ushort)SnesButton.Select;
        ushort cancel = (ushort)SnesButton.Y;
        AssertTrue(samus.HandleHudSelection(
                unchecked((ushort)(select | cancel)),
                select),
            "held Item Cancel plus Select chooses Missiles for auto-cancel");
        AssertEqual((ushort)1, samus.AutoCancelHudItemIndex,
            "missile auto-cancel request retains the selected HUD index");

        shared.StepFrame(bus, level, samus, (ushort)SnesButton.X, (ushort)SnesButton.X);
        SamusProjectileFrameResult missile = projectiles.StepFrame(
            bus,
            level,
            samus,
            (ushort)SnesButton.X,
            (ushort)SnesButton.X,
            0,
            0,
            shared);
        AssertTrue(missile.FiredSlot is not null, "auto-cancel setup fires one Missile");
        AssertEqual((ushort)0, samus.SelectedHudItem,
            "successful Missile auto-cancels back to beams");
        AssertEqual((ushort)0, samus.AutoCancelHudItemIndex,
            "successful Missile consumes its auto-cancel request");

        HoldCharge(bus, level, samus, projectiles, shared, shootAlreadyHeld: true);
        shared.StepFrame(bus, level, samus, 0, 0);
        SamusProjectileFrameResult murder = projectiles.StepFrame(
            bus, level, samus, 0, 0, 0, 0, shared);
        AssertTrue(murder.FiredSlot is not null,
            "missile cooldown suppresses the unsafe uncharged shot and permits charged release");
        AssertEqual((ushort)0x901f, projectiles.Slots[murder.FiredSlot!.Value].Type,
            "missile auto-cancel alternative produces Murder Beam");
    }

    private static void VerifyUnsafeUnchargedMurderBeam(
        ISnesAddressSpace bus,
        RoomLevelData level)
    {
        var samus = new SamusState
        {
            Pose = 2,
            XPosition = 128,
            YPosition = 128,
            EquippedBeams = 0x100f,
        };
        AssertThrows<NotSupportedException>(() => new SamusProjectileSystem().StepFrame(
            bus,
            level,
            samus,
            (ushort)SnesButton.X,
            (ushort)SnesButton.X,
            0,
            0,
            new SamusBombProjectileSystem()),
            "unsafe uncharged Murder Beam does not silently become an ordinary projectile");
    }
}
