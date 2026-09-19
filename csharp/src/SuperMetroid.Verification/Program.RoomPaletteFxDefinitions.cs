using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyRoomPaletteFxDefinitions(SuperMetroidAddressSpace rom)
    {
        ushort Word(int address) => unchecked((ushort)(
            rom.ReadByte(address) | rom.ReadByte(address + 1) << 8));

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
        AssertEqual(63, pointers.Length, "compiled palette-FX definition count");

        var forbidden = new HashSet<int>();
        foreach (ushort pointer in pointers)
        {
            int address = RoomFxRomData.Banks.PaletteFx | pointer;
            for (int index = 0; index < RoomPaletteFxDefinitions.DefinitionByteCount; index++)
                forbidden.Add(address + index);

            RoomPaletteFxDefinition definition = RoomPaletteFxDefinitions.Get(pointer);
            AssertEqual(Word(address), definition.SetupCallback,
                $"palette-FX ${pointer:X4} setup callback");
            AssertEqual(Word(address + 2), definition.InitialInstructionList,
                $"palette-FX ${pointer:X4} initial list");
        }

        ReadOnlySpan<ushort> areaLists = RoomPaletteFxDefinitions.NativeAreaListPointers;
        AssertEqual(RoomPaletteFxDefinitions.AreaCount, areaLists.Length,
            "palette-FX area-list count");
        for (int area = 0; area < RoomPaletteFxDefinitions.AreaCount; area++)
        {
            int pointerAddress = RoomFxRomData.Tables.AreaPaletteFxObjectListPointers + area * 2;
            forbidden.Add(pointerAddress);
            forbidden.Add(pointerAddress + 1);
            ushort nativeList = Word(pointerAddress);
            AssertEqual(nativeList, areaLists[area], $"area {area} native palette-FX list identity");
            for (int bit = 0; bit < RoomPaletteFxDefinitions.DefinitionsPerArea; bit++)
            {
                int entryAddress = RoomFxRomData.Banks.RoomDefinitions |
                    unchecked((ushort)(nativeList + bit * 2));
                forbidden.Add(entryAddress);
                forbidden.Add(entryAddress + 1);
                AssertEqual(
                    Word(entryAddress),
                    RoomPaletteFxDefinitions.GetAreaDefinition(area, bit),
                    $"area {area} palette-FX bit {bit}");
            }
        }

        var guardedRom = new RoomPaletteFxDefinitionReadGuard(rom, forbidden);
        foreach (ushort pointer in pointers)
        {
            RoomPaletteFxDefinition definition = RoomPaletteFxDefinitions.Get(pointer);
            var system = new RoomPaletteFxSystem();
            system.SpawnDefinition(guardedRom, pointer, equippedItems: 0);
            object slot = ActivePaletteFxSlot(system);
            AssertEqual(pointer, PaletteFxSlotWord(slot, "Id"), $"${pointer:X4} production ID");
            AssertEqual((ushort)1, PaletteFxSlotWord(slot, "InstructionTimer"),
                $"${pointer:X4} production timer");
            AssertEqual(
                definition.SetupCallback == PaletteFxSetupCodes.Intro
                    ? PaletteFxPreInstructionCodes.Intro
                    : PaletteFxPreInstructionCodes.Null,
                PaletteFxSlotWord(slot, "PreInstruction"),
                $"${pointer:X4} production pre-instruction");
            AssertEqual(
                definition.SetupCallback == PaletteFxSetupCodes.Norfair
                    ? PaletteFxInstructionListPointers.NorfairPowerSuit
                    : definition.InitialInstructionList,
                PaletteFxSlotWord(slot, "InstructionPointer"),
                $"${pointer:X4} production initial list");
        }

        const ushort fxPointer = 0x9000;
        for (int area = 0; area < AreaIds.RetailCount; area++)
        for (int bit = 0; bit < RoomPaletteFxDefinitions.DefinitionsPerArea; bit++)
        {
            var bus = new TestAddressSpace();
            bus.WriteByte(RoomFxRomData.Banks.RoomDefinitions | fxPointer, 0);
            bus.WriteByte(
                RoomFxRomData.Banks.RoomDefinitions |
                    unchecked((ushort)(fxPointer + RoomFxRomData.Record.PaletteFxBitsetOffset)),
                unchecked((byte)(1 << bit)));
            var guard = new RoomPaletteFxDefinitionReadGuard(bus, forbidden);
            var system = new RoomPaletteFxSystem();
            system.LoadRoom(
                guard,
                fxPointer,
                doorPointer: 0,
                (AreaId)area,
                equippedItems: 0,
                areaMiniBossDefeated: false);
            AssertEqual(
                RoomPaletteFxDefinitions.GetAreaDefinition(area, bit),
                PaletteFxSlotWord(ActivePaletteFxSlot(system), "Id"),
                $"area {area} bit {bit} production room load");
        }

        var hyperBeam = new HyperBeamPaletteFxState();
        hyperBeam.Spawn();
        HyperBeamPaletteFxStepResult hyperStep = hyperBeam.Step(guardedRom, new SnesCgram());
        AssertTrue(hyperStep.PaletteWritten,
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
            "and the Hyper Beam owner pass with fixed metadata reads forbidden.");
    }

    private static IEnumerable<ushort> DefinitionRange(ushort first, ushort last)
    {
        for (int pointer = first; pointer <= last; pointer +=
            RoomPaletteFxDefinitions.DefinitionByteCount)
        {
            yield return unchecked((ushort)pointer);
        }
    }

    private static object ActivePaletteFxSlot(RoomPaletteFxSystem system)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var slots = (Array)typeof(RoomPaletteFxSystem).GetField("slots", flags)!.GetValue(system)!;
        for (int index = slots.Length - 1; index >= 0; index--)
        {
            object slot = slots.GetValue(index)!;
            if (PaletteFxSlotWord(slot, "Id") != 0)
                return slot;
        }
        throw new InvalidOperationException("Expected one active palette-FX slot.");
    }

    private static ushort PaletteFxSlotWord(object slot, string name) =>
        (ushort)slot.GetType().GetProperty(name)!.GetValue(slot)!;

    private sealed class RoomPaletteFxDefinitionReadGuard(
        ISnesAddressSpace source,
        HashSet<int> forbidden) : ISnesAddressSpace
    {
        public byte ReadByte(int address) => forbidden.Contains(address)
            ? throw new InvalidOperationException(
                $"Palette-FX runtime reread compiled metadata byte ${address:X6}.")
            : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
