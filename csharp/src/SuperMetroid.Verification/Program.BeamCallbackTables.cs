using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Checks beam callback table words against the cartridge and verifies their installation on production firing paths.</summary>
    /// <param name="initializeOnly">When <see langword="true"/>, checks projectile initialization without advancing charged or uncharged shots through firing.</param>
    private static void VerifyBeamCallbackTables(bool initializeOnly = false)
    {
        var retail = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (bool charged in new[] { false, true })
        {
            int table = charged
                ? SamusBeamPreInstructionCodes.ChargedTable
                : SamusBeamPreInstructionCodes.UnchargedTable;
            for (int combination = 0; combination < SamusBeamCallbackDefinitions.CombinationCount; combination++)
            {
                AssertEqual(
                    ReadBeamCallbackWord(retail, table + combination * sizeof(ushort)),
                    SamusBeamCallbackDefinitions.Resolve(charged, combination).NativePointer,
                    $"charged={charged} callback {combination:X1} matches cartridge");

            }
        }

        AssertThrows<ArgumentOutOfRangeException>(
            () => SamusBeamCallbackDefinitions.Resolve(false, 16),
            "beam callback rejects index beyond low nibble");

        var room = new RoomLevelData(
            16,
            16,
            new ushort[256],
            new byte[256],
            new ushort[256],
            new byte[8]);
        foreach (bool charged in new[] { false, true })
        foreach (int beamType in Enumerable.Range(0, 12))
        {
            var bus = new BeamSpeedRowAddressSpace(retail);
            var samus = new SamusState
            {
                Pose = 1,
                XPosition = 128,
                YPosition = 128,
                EquippedBeams = unchecked((ushort)(beamType | (charged ? 0x1000 : 0))),
            };
            var projectiles = CreateProjectileFixture();
            var shared = CreateBombFixture();
            if (initializeOnly)
            {
                typeof(SamusProjectileSystem).GetMethod("TryFireBeam",
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)!
                    .Invoke(projectiles, new object?[] { bus, room, samus, (ushort)SnesButton.X, shared, null, charged });
                AssertTrue(projectiles.Slots[0].Type != 0 || projectiles.Slots[0].InstructionPointer != 0,
                    $"charged={charged} beam {beamType:X1} initializes");
                AssertEqual(SamusBeamCallbackDefinitions.Resolve(charged, beamType).Translated!.Value,
                    projectiles.Slots[0].PreInstruction,
                    $"charged={charged} beam {beamType:X1} initializer installs compiled callback");
                continue;
            }
            if (charged)
            {
                for (int frame = 0; frame < 60; frame++)
                {
                    shared.StepFrame(bus, room, samus, 0, 0);
                    projectiles.StepFrame(bus, room, samus, (ushort)SnesButton.X,
                        frame == 0 ? (ushort)SnesButton.X : (ushort)0, 0, 0, shared);
                }
            }
            projectiles.StepFrame(
                bus,
                room,
                samus,
                charged ? (ushort)0 : (ushort)SnesButton.X,
                charged ? (ushort)0 : (ushort)SnesButton.X,
                0,
                0,
                shared);
            AssertTrue(projectiles.LastFiredProjectileSnapshot is not null,
                $"charged={charged} beam {beamType:X1} fires");
            // The fixture starts with every ordinary slot empty, so the fired shot is the
            // lowest live slot after the release frame.
            SamusProjectileSlot fired = projectiles.Slots.First(slot => slot.IsActive);
            SamusBeamCallbackDefinition expected =
                SamusBeamCallbackDefinitions.Resolve(charged, beamType);
            AssertEqual(
                expected.Translated!.Value,
                fired.PreInstruction,
                $"charged={charged} beam {beamType:X1} installs compiled callback");
        }
        Console.WriteLine(
            "Beam callback definitions: all 32 low-nibble words and 24 production firing paths pass with both source ranges forbidden.");
    }

    /// <summary>Reads one little-endian 16-bit callback pointer from the supplied cartridge address space.</summary>
    /// <param name="bus">Address space containing the retail callback table.</param>
    /// <param name="address">Cartridge address of the pointer's low byte.</param>
    /// <returns>The two bytes at <paramref name="address"/> and the following address, combined as a word.</returns>
    private static ushort ReadBeamCallbackWord(SuperMetroidAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}
