using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyProjectileCooldowns()
    {
        var retail = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var bus = new ProjectileCooldownReadGuard(retail);
        for (int address = 0x90c254; address < 0x90c28f; address++)
            AssertEqual(retail.ReadByte(address), SamusProjectileCooldownDefinitions.ReadByte(address),
                "Compiled cooldown byte matches native address identity");
        AssertEqual(retail.ReadByte(SamusProjectileCooldownDefinitions.SpacetimeBeamCooldownAddress),
            SamusProjectileCooldownDefinitions.ReadByte(
                SamusProjectileCooldownDefinitions.SpacetimeBeamCooldownAddress),
            "bounded SpaceTime setup retains its exact adjacent native cooldown observation");
        foreach (int address in new[] { 0x908000, 0x90c253, 0x90c28f, 0x90c290, 0x90c292, 0x90ffff })
            AssertThrows<InvalidDataException>(
                () => SamusProjectileCooldownDefinitions.ReadByte(address),
                "Unknown cooldown address fails instead of reading adjacent presentation data");
        var room = CreateRoom(32, 16, new ushort[512], new byte[512]);
        var fire = typeof(SamusProjectileSystem).GetMethod("TryFireBeam", BindingFlags.NonPublic | BindingFlags.Instance)!;
        for (ushort beam = 0; beam < 12; beam++)
        foreach (bool charged in new[] { false, true })
        foreach (ushort edge in new ushort[] { 0, (ushort)SnesButton.X })
        {
            var projectiles = CreateProjectileFixture();
            var shared = CreateBombFixture();
            var samus = new SamusState { Pose = 1, XPosition = 128, YPosition = 128, EquippedBeams = beam };
            var result = ((int? Slot, ushort Sound))fire.Invoke(projectiles,
                new object?[] { bus, room, samus, edge, shared, null, charged })!;
            AssertEqual((int?)0, result.Slot, "Cooldown check exercises the actual successful producer");
            int address = charged ? 0x90c264 + beam : edge == 0 ? 0x90c283 + beam : 0x90c254 + beam;
            AssertEqual((ushort)retail.ReadByte(address), shared.CooldownTimer,
                "Native charged/fresh/held selection installs exact shared delay");
        }
        // Original source 2778 releases an uncharged shot via the pose handoff,
        // with Charge equipped and neither current nor previous Shoot edge set.
        foreach (bool chargeEquipped in new[] { true, false })
        {
            var projectiles = CreateProjectileFixture();
            var shared = CreateBombFixture();
            var samus = new SamusState { Pose = SamusPoseIds.NormalJumpGunExtendedLeftPose,
                XPosition = 211, YPosition = 288, EquippedBeams = (ushort)(chargeEquipped ? 0x1009 : 9) };
            typeof(SamusState).GetProperty(nameof(SamusState.PoseTransitionShotDirection))!
                .SetValue(samus, (ushort)0x8007);
            SamusProjectileSlotObservation slotsBefore = projectiles.ObserveSlots();
            projectiles.StepFrame(bus, room, samus, (ushort)SnesButton.X,
                0, 0, 0, shared, controllerPreviousNewInput: chargeEquipped ? (ushort)0 : (ushort)SnesButton.X);
            AssertEqual((int?)0, slotsBefore.FiredSlot(projectiles),
                "native handoff/previous-edge shot is produced");
            AssertEqual((ushort)15, shared.CooldownTimer, "Charge equipment or previous Shoot edge selects ordinary cooldown");
        }
        foreach (ushort beam in new ushort[] { 0, 10 })
        {
            var projectiles = CreateProjectileFixture();
            var shared = CreateBombFixture();
            var samus = new SamusState { Pose = 1, XPosition = 128, YPosition = 128, EquippedBeams = beam };
            var frames = new List<int>();
            for (int frame = 0; frame < 70; frame++)
            {
                // Actual shared cooldown owner advances before humanoid fire dispatch.
                shared.StepFrame(bus, room, samus, (ushort)SnesButton.X,
                    frame == 0 ? (ushort)SnesButton.X : (ushort)0);
                projectiles.StepFrame(bus, room, samus, (ushort)SnesButton.X,
                    frame == 0 ? (ushort)SnesButton.X : (ushort)0, 0, 0, shared);
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
            var samus = new SamusState { Pose = 1, XPosition = 128, YPosition = 128,
                EquippedBeams = beam, SelectedHudItem = 3, PowerBombs = 10 };
            AssertTrue(projectiles.TryActivateCombo(bus, samus, shared, out _),
                "Each native special beam attack reaches its actual cooldown publisher");
            AssertEqual((ushort)retail.ReadByte(0x90c254 + (projectiles.Slots[0].Type & 0x3f)),
                shared.CooldownTimer, "Combo retains six-bit index into padding/non-beam neighbors");
        }
        Console.WriteLine("Projectile cooldowns: 59 native bytes, bounded SpaceTime observation, loud non-catalog rejection, 48 producer selections, four special attacks and two 70-frame held-fire sequences pass with cooldown ROM reads forbidden.");
    }

    private sealed class ProjectileCooldownReadGuard(ISnesAddressSpace source) : ISnesAddressSpace, IImportCartridgeSource, ISnesMutableMemory
    {
        public byte ReadCartridgeByte(int address) => ReadByte(address);
        public byte ReadWorkRamByte(int address) => ((ISnesMutableMemory)source).ReadWorkRamByte(address);
        public byte ReadSaveRamByte(int address) => ((ISnesMutableMemory)source).ReadSaveRamByte(address);

        public byte ReadByte(int address)
        {
            if (address is >= 0x90c254 and < 0x90c28f)
                throw new InvalidOperationException($"Compiled cooldown read ROM ${address:X6}.");
            return source.ReadByte(address);
        }
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
