using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyBeamCallbackTables()
    {
        var retail = SuperMetroidAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (bool charged in new[] { false, true })
        {
            ReadOnlySpan<SamusBeamCallbackDefinition> definitions = charged
                ? SamusBeamCallbackDefinitions.Charged
                : SamusBeamCallbackDefinitions.Uncharged;
            AssertEqual(SamusBeamCallbackDefinitions.CombinationCount, definitions.Length,
                $"charged={charged} beam callback definition count");
            int table = charged
                ? SamusBeamPreInstructionCodes.ChargedTable
                : SamusBeamPreInstructionCodes.UnchargedTable;
            for (int combination = 0; combination < definitions.Length; combination++)
            {
                AssertEqual(
                    ReadBeamCallbackWord(retail, table + combination * sizeof(ushort)),
                    definitions[combination].NativePointer,
                    $"charged={charged} callback {combination:X1} matches cartridge");
                AssertEqual(
                    definitions[combination],
                    SamusBeamCallbackDefinitions.Resolve(charged, combination),
                    $"charged={charged} callback {combination:X1} resolves by identity");
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
            var projectiles = new SamusProjectileSystem();
            var shared = new SamusBombProjectileSystem();
            if (charged)
            {
                for (int frame = 0; frame < 60; frame++)
                {
                    shared.StepFrame(bus, room, samus, 0, 0);
                    projectiles.StepFrame(bus, room, samus, (ushort)SnesButton.X,
                        frame == 0 ? (ushort)SnesButton.X : (ushort)0, 0, 0, shared);
                }
            }
            SamusProjectileFrameResult result = projectiles.StepFrame(
                bus,
                room,
                samus,
                charged ? (ushort)0 : (ushort)SnesButton.X,
                charged ? (ushort)0 : (ushort)SnesButton.X,
                0,
                0,
                shared);
            AssertTrue(result.FiredSlot.HasValue,
                $"charged={charged} beam {beamType:X1} fires");
            SamusBeamCallbackDefinition expected =
                SamusBeamCallbackDefinitions.Resolve(charged, beamType);
            AssertEqual(
                expected.Translated!.Value,
                projectiles.Slots[result.FiredSlot!.Value].PreInstruction,
                $"charged={charged} beam {beamType:X1} installs compiled callback");
        }
        Console.WriteLine(
            "Beam callback definitions: all 32 low-nibble words and 24 retail firing paths pass with both source ranges forbidden.");
    }

    private static ushort ReadBeamCallbackWord(SuperMetroidAddressSpace bus, int address) =>
        unchecked((ushort)(bus.ReadByte(address) | (bus.ReadByte(address + 1) << 8)));
}
