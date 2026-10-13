using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using static RetailRuntimeFixtureVerification;
using static VerificationHelpersVerification;

/// <summary>Checks native projectile cooldown selection through beam, combo, and held-fire producers.</summary>
internal static class ProjectileCooldownsVerification
{
    /// <summary>Compares compiled bank $90 bytes and exact shared-timer behavior with source reads blocked.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for projectile cooldowns")]
    internal static void VerifyProjectileCooldowns()
    {
        var retail = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var bus = NewProjectileCooldownReadGuard(retail);
        for (int address = 0x90c254; address < 0x90c28f; address++)
            AssertEqual(retail.ReadCartridgeByte(address), SamusProjectileCooldownDefinitions.ReadByte(address),
                "Compiled cooldown byte matches native address identity");
        AssertEqual(retail.ReadCartridgeByte(SamusProjectileCooldownDefinitions.SpacetimeBeamCooldownAddress),
            SamusProjectileCooldownDefinitions.ReadByte(
                SamusProjectileCooldownDefinitions.SpacetimeBeamCooldownAddress),
            "bounded SpaceTime setup retains its exact adjacent native cooldown observation");
        foreach (int address in new[] { 0x908000, 0x90c253, 0x90c28f, 0x90c290, 0x90c292, 0x90ffff })
            AssertThrows<InvalidDataException>(
                () => SamusProjectileCooldownDefinitions.ReadByte(address),
                "Unknown cooldown address fails instead of reading adjacent presentation data");
        var room = CreateRoom(new(32, 16), new ushort[512], new byte[512]);
        var fire = typeof(BeamFiring).GetMethod("TryFireBeam", BindingFlags.NonPublic | BindingFlags.Instance)!;
        for (ushort beam = 0; beam < 12; beam++)
        foreach (bool charged in new[] { false, true })
        foreach (ushort edge in new ushort[] { 0, (ushort)SnesButton.X })
        {
            var projectiles = CreateProjectileFixture();
            var shared = CreateBombFixture();
            var samus = new SamusState { Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)), EquippedBeams = beam };
            var result = ((int? Slot, ushort Sound))fire.Invoke(projectiles.Firing,
                new object?[] { bus, room, samus, edge, shared, null, charged })!;
            AssertEqual((int?)0, result.Slot, "Cooldown check exercises the actual successful producer");
            int address = charged ? 0x90c264 + beam : edge == 0 ? 0x90c283 + beam : 0x90c254 + beam;
            AssertEqual((ushort)retail.ReadCartridgeByte(address), shared.CooldownTimer,
                "Native charged/fresh/held selection installs exact shared delay");
        }
        // Original source 2778 releases an uncharged shot via the pose handoff,
        // with Charge equipped and neither current nor previous Shoot edge set.
        foreach (bool chargeEquipped in new[] { true, false })
        {
            var projectiles = CreateProjectileFixture();
            var shared = CreateBombFixture();
            var samus = new SamusState { Pose = SamusPoseId.NormalJumpGunExtendedLeftPose,
                Position = FixedPosition<RoomPixels>.FromWhole(new(211, 288)), EquippedBeams = (ushort)(chargeEquipped ? 0x1009 : 9) };
            typeof(SamusState).GetProperty(nameof(SamusState.PoseTransitionShotDirection))!
                .SetValue(samus, (ushort)0x8007);
            SamusProjectileSlotObservation slotsBefore = projectiles.ObserveSlots();
            projectiles.StepFrame(new SamusTerrain { Bus = bus, Level = room }, samus, new ProjectileButtonInput { ControllerInput = (ushort)SnesButton.X, ControllerNewInput = 0, ControllerPreviousNewInput = chargeEquipped ? (ushort)0 : (ushort)SnesButton.X }, new(0, 0), shared);
            AssertEqual((int?)0, slotsBefore.FiredSlot(projectiles),
                "native handoff/previous-edge shot is produced");
            AssertEqual((ushort)15, shared.CooldownTimer, "Charge equipment or previous Shoot edge selects ordinary cooldown");
        }
        foreach (ushort beam in new ushort[] { 0, 10 })
        {
            var projectiles = CreateProjectileFixture();
            var shared = CreateBombFixture();
            var samus = new SamusState { Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)), EquippedBeams = beam };
            var frames = new List<int>();
            for (int frame = 0; frame < 70; frame++)
            {
                // Actual shared cooldown owner advances before humanoid fire dispatch.
                shared.StepFrame(new SamusTerrain { Bus = bus, Level = room }, samus, (ushort)SnesButton.X, frame == 0 ? (ushort)SnesButton.X : (ushort)0);
                projectiles.StepFrame(new SamusTerrain { Bus = bus, Level = room }, samus, new ProjectileButtonInput { ControllerInput = (ushort)SnesButton.X, ControllerNewInput = frame == 0 ? (ushort)SnesButton.X : (ushort)0 }, new(0, 0), shared);
                if (projectiles.LastFiredProjectileSnapshot is not null) frames.Add(frame);
            }
            int firstDelay = beam == 10 ? 12 : 15;
            AssertTrue(frames.SequenceEqual(new[] { 0, firstDelay, firstDelay + 25, firstDelay + 50 }),
                "Held input fires at the native initial delay, then exact 25-frame auto-fire intervals");
        }
        foreach (ushort beam in new ushort[] { 1, 2, 4, 8 })
        {
            var projectiles = CreateProjectileFixture();
            var shared = CreateBombFixture();
            var samus = new SamusState { Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
                EquippedBeams = beam, SelectedHudItem = 3, PowerBombs = 10 };
            AssertTrue(projectiles.Combos.TryActivateCombo(bus, samus, shared, out _),
                "Each native special beam attack reaches its actual cooldown publisher");
            AssertEqual((ushort)retail.ReadCartridgeByte(0x90c254 + (projectiles.Slots[0].Type & 0x3f)),
                shared.CooldownTimer, "Combo retains six-bit index into padding/non-beam neighbors");
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
