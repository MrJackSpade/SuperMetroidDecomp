using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.CartridgeTables;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Samus.Projectiles;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

/// <summary>Checks native projectile cooldown selection through beam, combo, and held-fire producers.</summary>
internal static class ProjectileCooldownsVerification
{
    /// <summary>Compares compiled bank $90 bytes and exact shared-timer behavior with source reads blocked.</summary>
    internal static void VerifyProjectileCooldowns()
    {
        var retail = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var bus = NewProjectileCooldownReadGuard(retail);
        for (int address = 0x90c254; address < 0x90c28f; address++)
            AssertEqual(retail.ReadCartridgeByte(address), SamusProjectileCooldowns.ReadByte(address - SamusSourceDefinitionsConstants.FiringTables.Value),
                "Compiled cooldown byte matches native address identity");
        AssertEqual(retail.ReadCartridgeByte(SamusSourceDefinitionsConstants.FiringTables.Value + SamusFiringTableDefinitions.SpacetimeBeamCooldown),
            SamusProjectileCooldowns.ReadByte(
                SamusFiringTableDefinitions.SpacetimeBeamCooldown),
            "bounded SpaceTime setup retains its exact adjacent native cooldown observation");
        foreach (int address in new[] { 0x908000, 0x90c253, 0x90c28f, 0x90c290, 0x90c292, 0x90ffff })
            AssertThrows<InvalidDataException>(
                () => SamusProjectileCooldowns.ReadByte(address - SamusSourceDefinitionsConstants.FiringTables.Value),
                "Unknown cooldown address fails instead of reading adjacent presentation data");
        var room = CreateRoom(new(32, 16), new RoomTerrainBlock[512], new RoomBlockBehavior[512]);
        for (ushort beam = 0; beam < 12; beam++)
        foreach (bool charged in new[] { false, true })
        foreach (ControllerButton edge in new ControllerButton[] { 0, ControllerButton.X })
        {
            var projectiles = CreateProjectileFixture();
            var shared = CreateBombFixture();
            var samus = new SamusState(InstalledFixtures.AnimationContinuation) { Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)), EquippedBeams = (SamusBeamFlags)beam };
            var result = projectiles.Firing.TryFireBeam(room, samus, edge, shared, null, charged);
            AssertEqual(0, result.Slot, "Cooldown check exercises the actual successful producer");
            int address = charged ? 0x90c264 + beam : edge == 0 ? 0x90c283 + beam : 0x90c254 + beam;
            AssertEqual(retail.ReadCartridgeByte(address), shared.CooldownTimer.RawValue,
                "Native charged/fresh/held selection installs exact shared delay");
        }
        // Original source 2778 releases an uncharged shot via the pose handoff,
        // with Charge equipped and neither current nor previous Shoot edge set.
        foreach (bool chargeEquipped in new[] { true, false })
        {
            var projectiles = CreateProjectileFixture();
            var shared = CreateBombFixture();
            var samus = new SamusState(InstalledFixtures.AnimationContinuation) { Pose = SamusPoseId.NormalJumpGunExtendedLeftPose,
                Position = FixedPosition<RoomPixels>.FromWhole(new(211, 288)), EquippedBeams = (SamusBeamFlags)((ushort)(chargeEquipped ? 0x1009 : 9)) };
            samus.PendingTransitionShot = new SamusTransitionShot(7);
            SamusProjectileSlotObservation slotsBefore = projectiles.ObserveSlots();
            projectiles.StepFrame(new SamusTerrain { Level = room }, samus, new ProjectileButtonInput { ControllerInput = ControllerButton.X, ControllerNewInput = 0, ControllerPreviousNewInput = chargeEquipped ? 0 : ControllerButton.X }, new(0, 0), shared);
            AssertEqual(0, slotsBefore.FiredSlot(projectiles),
                "native handoff/previous-edge shot is produced");
            AssertEqual((ushort)15, shared.CooldownTimer.RawValue, "Charge equipment or previous Shoot edge selects ordinary cooldown");
        }
        foreach (ushort beam in new ushort[] { 0, 10 })
        {
            var projectiles = CreateProjectileFixture();
            var shared = CreateBombFixture();
            var samus = new SamusState(InstalledFixtures.AnimationContinuation) { Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)), EquippedBeams = (SamusBeamFlags)beam };
            var frames = new List<int>();
            for (int frame = 0; frame < 70; frame++)
            {
                // Actual shared cooldown owner advances before humanoid fire dispatch.
                shared.StepFrame(new SamusTerrain { Level = room }, samus, ControllerButton.X, frame == 0 ? ControllerButton.X : 0);
                projectiles.StepFrame(new SamusTerrain { Level = room }, samus, new ProjectileButtonInput { ControllerInput = ControllerButton.X, ControllerNewInput = frame == 0 ? ControllerButton.X : 0 }, new(0, 0), shared);
                if (projectiles.LastFrameResult.QueuedSoundEffect is not null) frames.Add(frame);
            }
            int firstDelay = beam == 10 ? 12 : 15;
            AssertTrue(frames.SequenceEqual(new[] { 0, firstDelay, firstDelay + 25, firstDelay + 50 }),
                "Held input fires at the native initial delay, then exact 25-frame auto-fire intervals");
        }
        foreach (ushort beam in new ushort[] { 1, 2, 4, 8 })
        {
            var projectiles = CreateProjectileFixture();
            var shared = CreateBombFixture();
            var samus = new SamusState(InstalledFixtures.AnimationContinuation) { Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
                EquippedBeams = (SamusBeamFlags)beam, SelectedHudItem = HudItem.PowerBomb, PowerBombs = 10 };
            AssertTrue(projectiles.Combos.TryActivateCombo(samus, shared, out _),
                "Each native special beam attack reaches its actual cooldown publisher");
            AssertEqual(retail.ReadCartridgeByte(0x90c254 + projectiles.Slots[0].Type.BeamTableRow),
                shared.CooldownTimer.RawValue, "Combo reads the cooldown at its beam-table row, into padding/non-beam neighbors");
        }
        Console.WriteLine("Projectile cooldowns: 59 native bytes, bounded SpaceTime observation, loud non-catalog rejection, 48 producer selections, four special attacks and two 70-frame held-fire sequences pass with cooldown ROM reads forbidden.");
    }

    /// <summary>Blocks migrated cooldown bytes during actual projectile firing.</summary>
    private static MutableCartridgeReadGuard NewProjectileCooldownReadGuard(ISnesAddressSpace source)
    {
        var guard = new MutableCartridgeReadGuard(source);
        guard.Forbid(address => address is >= 0x90c254 and < 0x90c28f,
            address => $"Compiled cooldown read ROM ${address:X6}.");
        return guard;
    }

    /// <summary>Runs the projectile cooldown producer and cadence checks.</summary>
    [VerificationCommand("--projectile-cooldowns")]
    internal static int ProjectileCooldownsCommand()
    {
        Suite(nameof(VerifyProjectileCooldowns), () => VerifyProjectileCooldowns());
        return 0;
    }
}
