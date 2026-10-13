using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling;
using SuperMetroid.Core.Rom;
using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static LookupPaletteFxAreasVerification;
using static LookupPaletteFxDispatchVerification;
using static SuiteTimingVerification;

internal static class RoomPaletteFxDefinitionsVerification
{
    [CpuBusBoundary("exact bus test: retail ROM oracle for room palette FX definitions")]
    internal static void VerifyRoomPaletteFxDefinitions(SuperMetroidAddressSpace rom)
    {

        ushort[] pointers =
        [
            .. DefinitionRange(
                RoomPaletteFxDefinitions.CinematicDefinitionsBegin,
                RoomPaletteFxDefinitions.CinematicDefinitionsEnd),
            .. DefinitionRange(
                RoomPaletteFxDefinitions.RoomDefinitionsBegin,
                RoomPaletteFxDefinitions.RoomDefinitionsEnd),
            .. DefinitionRange(
                RoomPaletteFxDefinitions.TourianDefinitionsBegin,
                RoomPaletteFxDefinitions.TourianDefinitionsEnd),
        ];
        Suite(nameof(VerifyPaletteFxDispatchDomain), () => VerifyPaletteFxDispatchDomain());
        Suite(nameof(VerifyPaletteFxSetupSelection), () => VerifyPaletteFxSetupSelection(rom));
        Suite(nameof(VerifyPaletteFxInitialListSelection), () => VerifyPaletteFxInitialListSelection(rom));
        AssertEqual(63, pointers.Length, "compiled palette-FX definition count");

        var forbidden = new HashSet<int>();
        int genericCompiledPrograms = 0;
        int specializedCompiledPrograms = 0;
        foreach (ushort pointer in pointers)
        {
            int address = RoomFxRomData.Banks.PaletteFx | pointer;
            for (int index = 0; index < RoomPaletteFxDefinitions.DefinitionByteCount; index++)
                forbidden.Add(address + index);

            RoomPaletteFxDefinition definition = RoomPaletteFxDefinitions.Get(pointer);
            if (definition.InitialInstructionList ==
                HyperBeamPaletteFxProgramDefinitions.InitialInstructionPointer)
            {
                specializedCompiledPrograms++;
            }
            else
            {
                AssertTrue(RoomPaletteFxProgramMechanicsDefinitions.TryReadMechanicsWord(
                        definition.InitialInstructionList,
                        out _),
                    $"palette-FX ${pointer:X4} begins at compiled mechanics word " +
                    $"$8D:{definition.InitialInstructionList:X4}");
                genericCompiledPrograms++;
            }
        }
        AssertEqual(62, genericCompiledPrograms,
            "generic palette-FX definitions with compiled initial mechanics");
        AssertEqual(1, specializedCompiledPrograms,
            "specialized Hyper Beam palette-FX definition count");

        Suite(nameof(VerifyPaletteFxAreaListPointers), () => VerifyPaletteFxAreaListPointers(rom));
        Suite(nameof(VerifyPaletteFxAreaSelections), () => VerifyPaletteFxAreaSelections(rom));
        for (int area = 0; area < RoomPaletteFxDefinitions.AreaCount; area++)
        {
            int pointerAddress = RoomFxRomData.Tables.AreaPaletteFxObjectListPointers + area * 2;
            forbidden.Add(pointerAddress);
            forbidden.Add(pointerAddress + 1);
            ushort nativeList = CartridgeImportSource.Require(rom).ReadCartridgeWord(pointerAddress);
            for (int bit = 0; bit < RoomPaletteFxDefinitions.DefinitionsPerArea; bit++)
            {
                int entryAddress = RoomFxRomDataBanksTooling.RoomDefinitions |
                    unchecked((ushort)(nativeList + bit * 2));
                forbidden.Add(entryAddress);
                forbidden.Add(entryAddress + 1);
            }
        }

        var guardedRom = NewRoomPaletteFxDefinitionGuard(rom, forbidden);
        Suite(nameof(VerifyPaletteFxDispatchSpawns), () => VerifyPaletteFxDispatchSpawns(guardedRom, pointers));

        const ushort fxPointer = 0x9000;
        for (int area = 0; area < AreaIds.RetailCount; area++)
        for (int bit = 0; bit < RoomPaletteFxDefinitions.DefinitionsPerArea; bit++)
        {
            var definition = new RoomFxRecordDefinition(fxPointer, 0, 0, 0, 0,
                0, 0, 0, 0, 0, unchecked((byte)(1 << bit)), 0, 0);
            var system = new RoomPaletteFxSystem();
            system.LoadDefinition(guardedRom, definition, (AreaId)area,
                equippedItems: 0, areaMiniBossDefeated: false);
            AssertEqual(
                RoomPaletteFxDefinitions.GetAreaDefinition(area, bit),
                ActivePaletteFxSlot(system).Id,
                $"area {area} bit {bit} production room load");
        }

        var hyperBeam = new HyperBeamPaletteFxState();
        hyperBeam.Spawn();
        var hyperColors = SuperMetroid.Core.Assets.HyperBeamFxColorCatalog.Load(
            new MemoryStream(SuperMetroid.AssetExtraction.HyperBeamFxColorExtractor.Extract(rom)));
        hyperBeam.Step(guardedRom, new SnesCgram(), hyperColors);
        AssertEqual(0, hyperBeam.CurrentFrameIndex,
            "specialized Hyper Beam owner executes with its definition bytes forbidden");

        AssertThrows<InvalidDataException>(
            () => RoomPaletteFxDefinitions.Get(0),
            "zero palette-FX definition is outside the authored domain");
        AssertThrows<InvalidDataException>(
            () => RoomPaletteFxDefinitions.Get(
                unchecked((ushort)(RoomPaletteFxDefinitions.RoomDefinitionsBegin + 1))),
            "unaligned palette-FX definition is outside the authored domain");
        AssertThrows<ArgumentOutOfRangeException>(
            () => RoomPaletteFxDefinitions.GetAreaDefinition(-1, 0),
            "negative palette-FX area index");
        AssertThrows<ArgumentOutOfRangeException>(
            () => RoomPaletteFxDefinitions.GetAreaDefinition(0, 8),
            "out-of-range palette-FX bit index");

        Console.WriteLine(
            "Palette-FX definitions: 63 setup/list records, eight native area lists, " +
            "64 area selections, every production spawn, all retail room-load selections, " +
            "62 compiled generic program entries, and the specialized Hyper Beam owner pass " +
            "with fixed metadata reads forbidden.");
    }

    internal static void VerifyPaletteFxDispatchSpawns(ISnesAddressSpace guardedRom, IEnumerable<ushort> pointers)
    {
        foreach (ushort pointer in pointers)
        {
            RoomPaletteFxDefinition definition = RoomPaletteFxDefinitions.Get(pointer);
            var system = new RoomPaletteFxSystem();
            system.SpawnDefinition(guardedRom, pointer, equippedItems: 0);
            var slot = ActivePaletteFxSlot(system);
            AssertEqual(pointer, slot.Id, $"${pointer:X4} production ID");
            AssertEqual((ushort)1, slot.InstructionTimer.RawValue,
                $"${pointer:X4} production timer");
            AssertEqual(
                definition.SetupCallback == PaletteFxSetup.Intro
                    ? (ushort)PaletteFxPreInstruction.Intro
                    : (ushort)PaletteFxPreInstruction.Null,
                slot.PreInstruction,
                $"${pointer:X4} production pre-instruction");
            AssertEqual(
                definition.SetupCallback == PaletteFxSetup.Norfair
                    ? PaletteFxInstructionListPointers.NorfairPowerSuit
                    : definition.InitialInstructionList,
                slot.InstructionPointer,
                $"${pointer:X4} production initial list");
        }

    }

    private static IEnumerable<ushort> DefinitionRange(ushort first, ushort last)
    {
        for (int pointer = first; pointer <= last; pointer +=
            RoomPaletteFxDefinitions.DefinitionByteCount)
        {
            yield return unchecked((ushort)pointer);
        }
    }

    private static RoomPaletteFxSystem.PaletteFxSlot ActivePaletteFxSlot(RoomPaletteFxSystem system)
    {
        var slots = system.slots;
        for (int index = slots.Length - 1; index >= 0; index--)
        {
            var slot = slots[index];
            if (slot.Id != 0)
                return slot;
        }
        throw new InvalidOperationException("Expected one active palette-FX slot.");
    }

    internal static CartridgeReadGuard NewRoomPaletteFxDefinitionGuard(ISnesAddressSpace source, HashSet<int> forbidden) =>
        new CartridgeReadGuard(source).Forbid(forbidden.Contains,
            address => $"Palette-FX runtime reread compiled metadata byte ${address:X6}.");

    [VerificationCommand("--room-palette-fx-definitions")]
    internal static int RoomPaletteFxDefinitionsCommand()
    {
        Suite(nameof(VerifyRoomPaletteFxDefinitions), () => VerifyRoomPaletteFxDefinitions(
            SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"))));
        return 0;
    }
}
