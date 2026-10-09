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
        var bus = new ProjectileSoundRoutingForbiddenBus(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc")));
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
        var projectiles = CreateProjectileFixture();
        var shared = CreateBombFixture();

        HoldCharge(bus, level, samus, projectiles, shared);

        var pause = CreateRetailPauseFixture(
            bus,
            samus,
            new Bank80SystemState(),
            AreaId.Crateria,
            0,
            0);
        EnterPauseEquipment(pause);
        SelectPauseBoots(pause);
        pause.Step(0, (ushort)(SnesButton.Left | SnesButton.A));
        AssertEqual((ushort)0x100f, samus.EquippedBeams,
            "same-frame Boots Left+A equips all four beams while retaining Charge");

        shared.StepFrame(bus, level, samus, 0, 0);
        SamusProjectileSlotObservation slotsBeforeFire = projectiles.ObserveSlots();
        SamusProjectileFrameResult fired = projectiles.StepFrame(
            bus,
            level,
            samus,
            0,
            0,
            0,
            0,
            shared);
        AssertEqual((int?)0, slotsBeforeFire.FiredSlot(projectiles),
            "charged-left Murder Beam allocates slot zero");
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

        Suite(nameof(VerifyMissileAutoCancelMurderBeam), () => VerifyMissileAutoCancelMurderBeam(bus, level));
        Suite(nameof(VerifyUnsafeMurderBeamDirection), () => VerifyUnsafeMurderBeamDirection(bus, level));
        Suite(nameof(VerifyUnsafeUnchargedMurderBeam), () => VerifyUnsafeUnchargedMurderBeam(bus, level));
        Console.WriteLine(
            "Murder Beam: precharge and missile auto-cancel setups, left-facing zero-list " +
            "persistence through Mother Brain's phase boundary, repeated 200-point damage " +
            "and Ice substitution match bounded cartridge evidence.");
    }

    /// <summary>Advances both projectile systems until the beam charge reaches the native fully charged threshold.</summary>
    /// <param name="bus">The address space used by the projectile and shared-bomb updates.</param>
    /// <param name="level">The level data supplied to each projectile update.</param>
    /// <param name="samus">Samus state whose beam charge is advanced.</param>
    /// <param name="projectiles">The projectile system receiving the held-fire input.</param>
    /// <param name="shared">The shared bomb-projectile system updated alongside the beam.</param>
    /// <param name="shootAlreadyHeld">Whether the first update should omit a new press edge while keeping fire held.</param>
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

    /// <summary>Confirms the unsupported right-facing Murder Beam callback fails explicitly instead of becoming a normal shot.</summary>
    /// <param name="bus">The address space used during the projectile update.</param>
    /// <param name="level">The level supplied to the projectile update.</param>
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
        var projectiles = CreateProjectileFixture();
        var shared = CreateBombFixture();
        HoldCharge(bus, level, samus, projectiles, shared);
        samus.EquippedBeams = 0x100f;
        AssertThrows<NotSupportedException>(() =>
        {
            shared.StepFrame(bus, level, samus, 0, 0);
            projectiles.StepFrame(bus, level, samus, 0, 0, 0, 0, shared);
        }, "unsafe right-facing Murder Beam does not silently become an ordinary projectile");
    }

    /// <summary>Checks that missile auto-cancel and cooldown preserve the charged Murder Beam release path.</summary>
    /// <param name="bus">The address space used for the missile and beam updates.</param>
    /// <param name="level">The level supplied to each projectile update.</param>
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
        var projectiles = CreateProjectileFixture();
        var shared = CreateBombFixture();
        ushort select = (ushort)SnesButton.Select;
        ushort cancel = (ushort)SnesButton.Y;
        AssertTrue(samus.HandleHudSelection(
                unchecked((ushort)(select | cancel)),
                select).Changed,
            "held Item Cancel plus Select chooses Missiles for auto-cancel");
        AssertEqual((ushort)1, samus.AutoCancelHudItemIndex,
            "missile auto-cancel request retains the selected HUD index");

        shared.StepFrame(bus, level, samus, (ushort)SnesButton.X, (ushort)SnesButton.X);
        projectiles.StepFrame(
            bus,
            level,
            samus,
            (ushort)SnesButton.X,
            (ushort)SnesButton.X,
            0,
            0,
            shared);
        AssertTrue(projectiles.LastFiredProjectileSnapshot is not null,
            "auto-cancel setup fires one Missile");
        AssertEqual((ushort)0, samus.SelectedHudItem,
            "successful Missile auto-cancels back to beams");
        AssertEqual((ushort)0, samus.AutoCancelHudItemIndex,
            "successful Missile consumes its auto-cancel request");

        HoldCharge(bus, level, samus, projectiles, shared, shootAlreadyHeld: true);
        shared.StepFrame(bus, level, samus, 0, 0);
        SamusProjectileSlotObservation slotsBeforeMurder = projectiles.ObserveSlots();
        projectiles.StepFrame(
            bus, level, samus, 0, 0, 0, 0, shared);
        int? murderSlot = slotsBeforeMurder.FiredSlot(projectiles);
        AssertTrue(murderSlot is not null,
            "missile cooldown suppresses the unsafe uncharged shot and permits charged release");
        AssertEqual((ushort)0x901f, projectiles.Slots[murderSlot!.Value].Type,
            "missile auto-cancel alternative produces Murder Beam");
    }

    /// <summary>Confirms an uncharged all-beams release with the unsupported Murder Beam callback is rejected.</summary>
    /// <param name="bus">The address space used by the projectile update.</param>
    /// <param name="level">The level supplied to the projectile update.</param>
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
        AssertThrows<NotSupportedException>(() => CreateProjectileFixture().StepFrame(
            bus,
            level,
            samus,
            (ushort)SnesButton.X,
            (ushort)SnesButton.X,
            0,
            0,
            CreateBombFixture()),
            "unsafe uncharged Murder Beam does not silently become an ordinary projectile");
    }
}
