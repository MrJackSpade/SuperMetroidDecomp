using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Checks compiled palette-FX definitions, production spawn setup, and room-load
    /// selections while preventing runtime code from rereading the metadata under test.
    /// </summary>
    /// <param name="rom">Cartridge address space supplying the retail definitions and area lists.</param>
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
            ushort nativeList = Word(pointerAddress);
            for (int bit = 0; bit < RoomPaletteFxDefinitions.DefinitionsPerArea; bit++)
            {
                int entryAddress = RoomFxRomDataBanksTooling.RoomDefinitions |
                    unchecked((ushort)(nativeList + bit * 2));
                forbidden.Add(entryAddress);
                forbidden.Add(entryAddress + 1);
            }
        }

        var guardedRom = new RoomPaletteFxDefinitionReadGuard(rom, forbidden);
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
                PaletteFxSlotWord(ActivePaletteFxSlot(system), "Id"),
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

    /// <summary>
    /// Confirms production dispatch initializes each definition's slot and initial instruction list.
    /// </summary>
    /// <param name="guardedRom">Address space that rejects reads from compiled palette-FX metadata.</param>
    /// <param name="pointers">Definition pointers whose production spawn setup is checked.</param>
    private static void VerifyPaletteFxDispatchSpawns(ISnesAddressSpace guardedRom, IEnumerable<ushort> pointers)
    {
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

    }

    /// <summary>Enumerates aligned definition pointers from the first address through the last.</summary>
    /// <param name="first">Address of the first definition in the inclusive range.</param>
    /// <param name="last">Address of the final definition to include.</param>
    /// <returns>Each definition address, advanced by one compiled record's byte count.</returns>
    private static IEnumerable<ushort> DefinitionRange(ushort first, ushort last)
    {
        for (int pointer = first; pointer <= last; pointer +=
            RoomPaletteFxDefinitions.DefinitionByteCount)
        {
            yield return unchecked((ushort)pointer);
        }
    }

    /// <summary>Finds the active slot with the highest index, matching the system's reverse slot scan.</summary>
    /// <param name="system">Palette-FX system whose private slot array is inspected.</param>
    /// <returns>The active slot object used by the verification assertions.</returns>
    /// <exception cref="InvalidOperationException">No slot has a nonzero definition ID.</exception>
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

    /// <summary>Reads a named slot property as a 16-bit word for setup-state assertions.</summary>
    /// <param name="slot">Palette-FX slot instance returned by the system.</param>
    /// <param name="name">Property name identifying the word to inspect.</param>
    /// <returns>The property's value converted to an unsigned 16-bit word.</returns>
    private static ushort PaletteFxSlotWord(object slot, string name) =>
        (ushort)slot.GetType().GetProperty(name)!.GetValue(slot)!;

    /// <summary>
    /// Wraps the cartridge address space so verification can fail if runtime code reads
    /// any setup metadata address recorded in <paramref name="forbidden"/>.
    /// </summary>
    /// <param name="source">Underlying address space used for allowed reads and all writes.</param>
    /// <param name="forbidden">Addresses belonging to definitions or area-list metadata.</param>
    private sealed class RoomPaletteFxDefinitionReadGuard(
        ISnesAddressSpace source,
        HashSet<int> forbidden) : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Rejects compiled-metadata reads before forwarding an ordinary bus read.</summary>
        /// <param name="address">Bus address requested by the runtime system.</param>
        /// <returns>The source byte when the address is not guarded.</returns>
        /// <exception cref="InvalidOperationException">The address belongs to guarded metadata.</exception>
        public byte ReadByte(int address)
        {
            RejectMetadataRead(address);
            return source.ReadByte(address);
        }

        /// <summary>Applies the metadata guard before forwarding a cartridge-data read.</summary>
        /// <param name="address">Cartridge address requested by the runtime system.</param>
        /// <returns>The source cartridge byte when the address is not guarded.</returns>
        /// <exception cref="InvalidOperationException">The address is guarded or the source lacks cartridge data.</exception>
        public byte ReadCartridgeByte(int address)
        {
            RejectMetadataRead(address);
            return (source as IImportCartridgeSource ?? throw new InvalidOperationException(
                "Palette-FX test source requires cartridge data.")).ReadCartridgeByte(address);
        }

        /// <summary>Fails verification when runtime execution requests a compiled metadata byte.</summary>
        /// <param name="address">Address checked against the forbidden metadata set.</param>
        /// <exception cref="InvalidOperationException">The address is in the forbidden set.</exception>
        private void RejectMetadataRead(int address)
        {
            if (forbidden.Contains(address))
                throw new InvalidOperationException(
                    $"Palette-FX runtime reread compiled metadata byte ${address:X6}.");
        }

        /// <summary>Forwards writes to the underlying address space without changing their values.</summary>
        /// <param name="address">Bus address receiving the write.</param>
        /// <param name="value">Byte value to write.</param>
        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
