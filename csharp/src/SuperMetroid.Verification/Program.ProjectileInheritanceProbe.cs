using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Desktop;

internal static partial class Program
{
    // The original initialized-WRAM reproduction is retained alongside live producers.
    /// <summary>Checks projectile velocity inheritance from native WRAM layouts and from live movement and runtime shots.</summary>
    private static void ProbeProjectileVelocityInheritance()
    {
        var initialize = typeof(SamusProjectileSystem)
            .GetMethod("InitializePowerBeamVelocity", BindingFlags.Static | BindingFlags.NonPublic)!
            .CreateDelegate<Action<ISnesAddressSpace, SamusProjectileSlot>>();
        var bus = new ProjectileInheritanceProbeBus();
        int failures = 0;
        // Snapshots are CameraYSubSpeed followed by the four native integer/fraction
        // pairs at $0DAA..0DB9. Deliberately preserve the cartridge's word ordering.
        ushort[][] snapshots =
        [
            [0, 0, 0, 0, 0, 0, 0, 0, 0],
            [0, 0, 0, 3, 0, 0, 0, 0, 0],
            [0, 0xfffd, 0, 0, 0, 0, 0, 0, 0],
            [0, 0, 0, 0, 0, 0xfffc, 0, 0, 0],
            [0xabcd, 0, 0x1234, 0, 0x5678, 0, 0x9abc, 0, 0],
        ];
        for (int sample = 0; sample < snapshots.Length; sample++)
        {
            for (int i = 0; i < snapshots[sample].Length; i++)
                bus.Word(0x0da8 + i * 2, snapshots[sample][i]);
            for (ushort direction = 0; direction < 10; direction++)
            {
                int baseSpeed = direction is 1 or 3 or 6 or 8 ? 0x02ab : 0x0400;
                var slot = new SamusProjectileSlot(0) { Direction = direction };
                initialize(bus, slot);
                // Direct transcription of loads/LSR/ORA/ADC at $90:B218..B2F5.
                // Do not replace these overlapping reads with idealized fixed-point math.
                ushort upWord = bus.Word(0x0db1);
                int up = (upWord & 0xff00) == 0 ? 0 : (upWord >> 2) | 0xc000;
                short expectedX = unchecked((short)(direction switch
                {
                    1 or 2 or 3 => baseSpeed + bus.Word(0x0dad),
                    6 or 7 or 8 => -baseSpeed + bus.Word(0x0da9),
                    _ => 0,
                }));
                short expectedY = unchecked((short)(direction switch
                {
                    0 or 1 or 8 or 9 => -baseSpeed + up,
                    3 or 4 or 5 or 6 => baseSpeed + bus.Word(0x0db5),
                    _ => 0,
                }));
                if (slot.XVelocity == expectedX && slot.YVelocity == expectedY) continue;
                failures++;
                Console.WriteLine($"Snapshot {sample}, direction {direction}: native ({expectedX:X4},{expectedY:X4}), port ({slot.XVelocity:X4},{slot.YVelocity:X4})");
            }
        }
        AssertEqual(0, failures, "Projectile initialization must inherit native movement/camera overlapping words");
        Suite(nameof(VerifyProjectileInheritanceMovement), () => VerifyProjectileInheritanceMovement());
        for (ushort weapon = 0; weapon < 3; weapon++) VerifyProjectileInheritanceRuntime(weapon);
        Console.WriteLine("Projectile inheritance: original 50 cases, live movement writes and preceding-frame runtime shot pass.");
    }

    /// <summary>Verifies that live collision movement publishes the directional fixed-point words consumed by projectile initialization.</summary>
    private static void VerifyProjectileInheritanceMovement()
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var samus = new SamusState { XPosition = 64, YPosition = 64 };
        samus.Kinematics.XRadius = 5;
        samus.Kinematics.YRadius = 5;
        var level = CreateRoom(16, 16, new ushort[256], new byte[256]);
        SamusProjectileInheritance.ClearMovement(bus);
        SamusBlockCollision.MoveHorizontal(bus, level, samus.Kinematics, 0x00031234);
        AssertEqual((short)0x0300, SamusProjectileInheritance.ReadVelocity(bus, 2, 0).X,
            "Live movement publishes right integer displacement");
        SamusBlockCollision.MoveHorizontal(bus, level, samus.Kinematics, -0x00018000);
        AssertEqual(unchecked((short)0xfe00), SamusProjectileInheritance.ReadVelocity(bus, 7, 0).X,
            "Negative movement preserves two's-complement integer half");
        AssertEqual((short)0x0380, SamusProjectileInheritance.ReadVelocity(bus, 2, 0).X,
            "Opposite direction fraction leaks into rightward velocity, not net displacement");
        SamusBlockCollision.MoveHorizontal(bus, level, samus.Kinematics, 0x00020000);
        AssertEqual((short)0x0280, SamusProjectileInheritance.ReadVelocity(bus, 2, 0).X,
            "Second move overwrites one directional record instead of accumulating");
        byte[] beforeProbe = bus.WorkRam.Slice(0x0da8, 18).ToArray();
        SamusBlockCollision.ProbeWallHorizontal(bus, level, samus.Kinematics, 1 << 16);
        AssertTrue(beforeProbe.AsSpan().SequenceEqual(bus.WorkRam.Slice(0x0da8, 18)),
            "Wall observation cannot replace live movement inheritance");
        level.SetForegroundEntry(4 * 16 + 5, RoomLevelWord.Create(0, 0, RoomCollisionType.SolidBlock).Raw);
        var clipped = SamusBlockCollision.MoveHorizontal(bus, level, samus.Kinematics, 20 << 16);
        AssertTrue(clipped.Collided, "Inheritance fixture actually reaches its solid wall");
        AssertEqual(unchecked((ushort)(clipped.AcceptedDisplacement >> 16)),
            (ushort)(bus.ReadByte(0x0dae) | bus.ReadByte(0x0daf) << 8),
            "Collision publishes clipped displacement, not the twenty-pixel request");
        SamusProjectileInheritance.PublishCameraYSubspeed(bus, 0xabcd);
        SamusProjectileInheritance.ClearMovement(bus);
        AssertEqual(unchecked((short)0xfcab), SamusProjectileInheritance.ReadVelocity(bus, 7, 0x400).X,
            "Alpha reset preserves camera fractional high byte");
        AssertTrue(bus.WorkRam.Slice(0x0daa, 16).ToArray().All(value => value == 0),
            "Alpha reset clears exactly all eight directional words");
    }

    /// <summary>Confirms a runtime shot inherits the preceding frame's movement and retains that behavior after runtime restoration.</summary>
    /// <param name="weapon">The selected HUD item identifier used for the projectile launch comparison.</param>
    private static void VerifyProjectileInheritanceRuntime(ushort weapon)
    {
        var bus = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var runtime = CreateRetailRuntimeFixture(bus);
        runtime.InitializeHud(HudSnapshot.CeresDebug);
        runtime.InitializeStartingCeresRoom();
        runtime.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.LandingSite);
        var level = runtime.LevelData!;
        for (int y = 16; y < 36; y++)
        for (int x = 16; x < 48; x++)
        {
            int index = y * level.WidthInBlocks + x;
            level.SetForegroundEntry(index, RoomLevelWord.Create(0, 0,
                y >= 32 ? RoomCollisionType.SolidBlock : RoomCollisionType.Air).Raw);
            level.SetBehavior(index, 0);
        }
        var samus = runtime.Samus!;
        samus.InputLocked = false;
        samus.SelectedHudItem = weapon;
        samus.Missiles = samus.MaxMissiles = 10;
        samus.SuperMissiles = samus.MaxSuperMissiles = 10;
        samus.Pose = SamusPoseIds.FacingRightNormalPose;
        samus.XPosition = 512;
        samus.YPosition = 490;
        runtime.Camera!.SetPosition(400, 350);
        samus.RefreshCollisionRadii(bus);
        samus.InitializeAnimation(bus);
        for (int frame = 0; frame < 20; frame++) runtime.StepFrame((ushort)SnesButton.Right);
        ushort Word(int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
        ushort inherited = Word(0x0dad);
        AssertTrue((inherited & 0xff00) != 0, "Controller-driven beta publishes nonzero preceding-frame movement");
        short expected = unchecked((short)(inherited +
            (weapon == 0 ? Word(0x90c2d1) + Word(0x90c357) : 0x0100)));
        using var stateStream = new MemoryStream();
        DebuggerObjectGraphSerializer.Serialize(stateStream, runtime);
        stateStream.Position = 0;
        var restored = DebuggerObjectGraphSerializer.Deserialize<SuperMetroidRuntime>(stateStream);
        // The host rebinds nonserialized presentation assets after debugger restore.
        BindRetailRuntimeFixture(restored);
        runtime.StepFrame((ushort)((ushort)SnesButton.Right | runtime.ControllerBindings.Shoot));
        restored.StepFrame((ushort)((ushort)SnesButton.Right | restored.ControllerBindings.Shoot));
        var shot = runtime.Projectiles!.Slots.First(slot => slot.IsActive);
        AssertEqual(expected, shot.XVelocity,
            "Next alpha fires using prior beta displacement before reset and this-frame movement");
        AssertEqual(expected, restored.Projectiles!.Slots.First(slot => slot.IsActive).XVelocity,
            "Debugger graph round trip retains native WRAM inheritance for resumed launch");
        AssertEqual(runtime.Camera!.CameraYSubspeed, Word(0x0da8),
            "Runtime publishes camera Y subspeed into the native shared record");
        byte[] beforeFreeze = bus.WorkRam.Slice(0x0daa, 16).ToArray();
        runtime.GameplayTimeFrozen = true;
        runtime.StepFrame(0);
        AssertTrue(beforeFreeze.AsSpan().SequenceEqual(bus.WorkRam.Slice(0x0daa, 16)),
            "Frozen gameplay skips the projectile alpha and preserves directional records");
        runtime.GameplayTimeFrozen = false;
        samus.InputLocked = true;
        runtime.StepFrame(0);
        AssertTrue(bus.WorkRam.Slice(0x0daa, 16).ToArray().All(value => value == 0),
            "Input-locked projectile alpha still clears directional movement records");
    }

    /// <summary>Stores sparse address-space bytes for reproducing the native projectile-initialization WRAM reads.</summary>
    private sealed class ProjectileInheritanceProbeBus : ISnesAddressSpace, IImportCartridgeSource, ISnesMutableMemory
    {
        /// <summary>Backing bytes keyed by their complete bus address.</summary>
        private readonly Dictionary<int, byte> _bytes = new();

        /// <summary>Routes cartridge reads to the probe's address-space byte lookup.</summary>
        /// <param name="address">The bus address to read.</param>
        /// <returns>The stored byte, or zero when the probe has no value at that address.</returns>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Returns the stored byte for a bus address, defaulting uninitialized probe memory to zero.</summary>
        /// <param name="address">The bus address to read.</param>
        /// <returns>The stored byte, or zero when no value was written.</returns>
        public byte ReadByte(int address) => _bytes.GetValueOrDefault(address);

        /// <summary>Reads a WRAM byte from the same sparse address map used by the probe.</summary>
        /// <param name="address">The WRAM address to read.</param>
        /// <returns>The stored byte, or zero when no value was written.</returns>
        public byte ReadWorkRamByte(int address) => ReadByte(address);

        /// <summary>Rejects save-RAM access because the projectile inheritance reproduction uses only WRAM.</summary>
        /// <param name="address">The save-RAM address the probe was asked to read.</param>
        /// <returns>This method never returns.</returns>
        /// <exception cref="InvalidOperationException">The probe must not read save RAM.</exception>
        public byte ReadSaveRamByte(int address) => throw new InvalidOperationException(
            "Projectile inheritance probe must not read SRAM.");

        /// <summary>Stores a byte at its complete bus address in the probe's sparse memory map.</summary>
        /// <param name="address">The bus address to write.</param>
        /// <param name="value">The byte to store.</param>
        public void WriteByte(int address, byte value) => _bytes[address] = value;

        /// <summary>Reads two adjacent bytes as a little-endian word.</summary>
        /// <param name="address">The address of the word's low byte.</param>
        /// <returns>The combined 16-bit value.</returns>
        public ushort Word(int address) => (ushort)(ReadByte(address) | ReadByte(address + 1) << 8);

        /// <summary>Writes a word as two adjacent little-endian bytes.</summary>
        /// <param name="address">The address of the word's low byte.</param>
        /// <param name="value">The 16-bit value to store.</param>
        public void Word(int address, ushort value)
        {
            WriteByte(address, (byte)value);
            WriteByte(address + 1, (byte)(value >> 8));
        }
    }
}
