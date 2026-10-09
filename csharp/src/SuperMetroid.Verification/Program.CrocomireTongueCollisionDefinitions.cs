using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    /// <summary>
    /// Confirms that every native Crocomire tongue visual selector resolves to installed
    /// artwork without consulting ROM during selector lookup.
    /// </summary>
    /// <param name="rom">Retail address space used to read the reference selector words.</param>
    /// <param name="stock">Installed enemy artwork catalog expected to contain each selected frame.</param>
    private static void VerifyInstalledCrocomireTongueVisualSelectors(
        SuperMetroidAddressSpace rom, EnemyTileArtworkCatalog stock)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        MethodInfo selector = typeof(RoomEnemySystem).GetMethod(
            "ReadEnemyVisualSelector", flags)!;
        var denied = new CrocomireTongueNoReadBus();
        var enemies = new RoomEnemySystem { TileArtwork = stock };
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, denied);
        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = RoomEnemySystem.CrocomireTongueDefinition;
        slot.Definition = default(RoomEnemyDefinition) with
        { Bank = CrocomireTongueCollisionDefinitions.Bank };
        var seen = new HashSet<ushort>();
        for (int index = 0;
             index < CrocomireTongueInstructionProgramDefinitionsTooling.PresentationWordCount;
             index++)
        {
            ushort operand = CrocomireTongueInstructionProgramDefinitionsTooling
                .PresentationWordAddress(index);
            ushort native = (ushort)(rom.ReadByte(0xa40000 | operand) |
                rom.ReadByte(0xa40000 | unchecked((ushort)(operand + 1))) << 8);
            ushort selected = (ushort)selector.Invoke(enemies, [slot, operand])!;
            AssertEqual(native, selected,
                $"Crocomire tongue selector $A4:{operand:X4} matches native");
            AssertTrue(stock.ExtendedFrames!.TryGetDisplay(
                    CrocomireTongueCollisionDefinitions.Bank, selected, out _),
                $"Crocomire tongue selector $A4:{operand:X4} has installed art");
            seen.Add(selected);
        }
        AssertTrue(seen.SetEquals(Enumerable.Range(0, CrocomireTongueCollisionDefinitions.FrameCount).Select(CrocomireTongueCollisionDefinitions.FramePointer)),
            "Crocomire tongue's nine operands select the nine extracted frames");
        AssertEqual(0, denied.ReadAttempts,
            "installed Crocomire tongue selectors never read ROM bytes");
        Console.WriteLine("Installed Crocomire tongue: nine native visual selectors and " +
            "editable frames, no ROM reads.");
    }

    /// <summary>Checks the nine compiled frame pointers against their native instruction operands.</summary>
    /// <param name="rom">Retail address space containing the Crocomire tongue instruction list.</param>
    private static void VerifyCrocomireTongueFramePositions(SuperMetroidAddressSpace rom)
    {
        ushort[] operands = [0xbe58, 0xbe5c, 0xbe60, 0xbe64, 0xbf9a, 0xbf9e, 0xbfa2, 0xbfa6, 0xbfaa];
        AssertEqual(9, CrocomireTongueCollisionDefinitions.FrameCount, "tongue frame count");
        for (int i = 0; i < operands.Length; i++)
        {
            ushort expected = (ushort)(rom.ReadByte(0xa40000 | operands[i]) | rom.ReadByte(0xa40000 | (operands[i] + 1)) << 8);
            AssertEqual(expected, CrocomireTongueCollisionDefinitions.FramePointer(i), "tongue native selected frame identity");
        }
        AssertThrows<IndexOutOfRangeException>(() => CrocomireTongueCollisionDefinitions.FramePointer(-1), "negative tongue frame index");
        AssertThrows<IndexOutOfRangeException>(() => CrocomireTongueCollisionDefinitions.FramePointer(9), "tongue frame index past end");
    }

    /// <summary>
    /// Verifies that every selected frame has one native component with an empty hitbox list
    /// and that the compiled definitions reject addresses outside those authored cases.
    /// </summary>
    /// <param name="rom">Retail address space containing the frame and hitbox records.</param>
    private static void VerifyCrocomireTongueComponentCases(SuperMetroidAddressSpace rom)
    {
        ushort[] frames = [0xc65e, 0xc668, 0xc672, 0xc67c, 0xcace, 0xcad8, 0xcae2, 0xcaec, 0xcaf6];
        ushort ReadWord(int address) => (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
        foreach (ushort frame in frames)
        {
            var component = CrocomireTongueCollisionDefinitions.ComponentAt(frame);
            AssertEqual((ushort)1, ReadWord(0xa40000 | frame),
                $"Crocomire tongue $A4:{frame:X4} native one-component header");
            AssertEqual(ReadWord(0xa40000 | frame + 8), component.HitboxPointer,
                "Crocomire tongue native hitbox-list pointer");
            AssertEqual((ushort)0, ReadWord(0xa40000 | component.HitboxPointer), "tongue native empty hitboxes");
            AssertEqual(0, CrocomireTongueCollisionDefinitions.HitboxCountAt(component.HitboxPointer), "tongue empty hitbox case");
        }
        for (int frame = 0; frame <= ushort.MaxValue; frame++)
            AssertEqual(Array.IndexOf(frames, (ushort)frame) >= 0,
                CrocomireTongueCollisionDefinitions.HasFrame((ushort)frame), "tongue exact frame domain");
        foreach (ushort frame in new ushort[] { 0, 0xc65d, 0xc65f, 0xc686, 0xcacd, 0xcacf, 0xcb00, 0xffff })
            AssertThrows<InvalidDataException>(() => CrocomireTongueCollisionDefinitions.ComponentAt(frame), "tongue rejects non-frame identity");
        AssertThrows<InvalidDataException>(() => CrocomireTongueCollisionDefinitions.HitboxCountAt(0x8000), "tongue rejects unknown hitbox list");
    }
    /// <summary>
    /// Compares native and compiled tongue collision callbacks across all selected frames,
    /// positions, and shot modes while ensuring the installed collision path never reads ROM.
    /// </summary>
    private static void VerifyCrocomireTongueCollisionDefinitions()
    {
        var rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyCrocomireTongueFramePositions), () => VerifyCrocomireTongueFramePositions(rom));
        Suite(nameof(VerifyCrocomireTongueComponentCases), () => VerifyCrocomireTongueComponentCases(rom));
        var denied = new CrocomireTongueNoReadBus();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        MethodInfo walker = typeof(RoomEnemySystem).GetMethod(
            "TryFindExtendedHitboxCallback", flags)!;
        var compiled = new RoomEnemySystem();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(compiled, denied);
        var nativeSlot = new RoomEnemySlot(0);
        RoomEnemySlot compiledSlot = compiled.Slots[0];
        nativeSlot.Definition = compiledSlot.Definition =
            default(RoomEnemyDefinition) with
            { Bank = CrocomireTongueCollisionDefinitions.Bank };
        nativeSlot.EnemyDefinitionPointer = 0xffff;
        compiledSlot.EnemyDefinitionPointer = RoomEnemySystem.CrocomireTongueDefinition;
        AssertEqual(9, CrocomireTongueCollisionDefinitions.FrameCount,
            "all selected Crocomire tongue frames have compiled collision");

        var seenLists = new HashSet<ushort>();
        int probes = 0;
        for (int frameIndex = 0; frameIndex < CrocomireTongueCollisionDefinitions.FrameCount; frameIndex++)
        {
            ushort frame = CrocomireTongueCollisionDefinitions.FramePointer(frameIndex);
            CrocomireTongueCollisionComponent component =
                CrocomireTongueCollisionDefinitions.ComponentAt(frame);
            seenLists.Add(component.HitboxPointer);
            nativeSlot.SpritemapPointer = compiledSlot.SpritemapPointer = frame;
            foreach ((ushort x, ushort y) in new (ushort, ushort)[]
                     {
                         (0x0100, 0x0100), (0x0004, 0x0006),
                         (0xfffc, 0xfffa),
                     })
            for (int shot = 0; shot <= 1; shot++)
            {
                nativeSlot.XPosition = compiledSlot.XPosition = x;
                nativeSlot.YPosition = compiledSlot.YPosition = y;
                object?[] nativeArguments =
                    [nativeSlot, x, y, (ushort)0, (ushort)0, shot != 0, (ushort)0];
                object?[] compiledArguments =
                    [compiledSlot, x, y, (ushort)0, (ushort)0, shot != 0, (ushort)0];
                bool nativeHit = ReferenceExtendedCollision(rom, nativeArguments);
                bool compiledHit = (bool)walker.Invoke(compiled, compiledArguments)!;
                AssertEqual(nativeHit, compiledHit,
                    $"Crocomire tongue $A4:{frame:X4} overlap {x:X4},{y:X4}, shot={shot}");
                AssertEqual((ushort)nativeArguments[^1]!,
                    (ushort)compiledArguments[^1]!,
                    "Crocomire tongue native/compiled callback parity");
                AssertTrue(!compiledHit,
                    "empty native Crocomire tongue hitbox list cannot hit Samus or shots");
                probes++;
            }
        }
        AssertTrue(seenLists.SetEquals(new ushort[] { 0xcbb3, 0xcc3b }),
            "Crocomire tongue selected frames refer to exactly two native hitbox lists");
        foreach (ushort list in seenLists)
        {
            AssertEqual((ushort)0, ReadWord(0xa40000 | list),
                $"Crocomire tongue $A4:{list:X4} native hitbox list is empty");
            AssertEqual(0, CrocomireTongueCollisionDefinitions.HitboxCountAt(list),
                "Crocomire tongue compiled hitbox list is empty");
        }
        AssertThrows<InvalidDataException>(
            () => CrocomireTongueCollisionDefinitions.ComponentAt(0x8000),
            "unknown Crocomire tongue frame fails loudly");
        AssertThrows<InvalidDataException>(
            () => CrocomireTongueCollisionDefinitions.HitboxCountAt(0x8000),
            "unknown Crocomire tongue hitbox list fails loudly");
        AssertEqual(0, denied.ReadAttempts,
            "installed Crocomire tongue collision never reads ROM bytes");
        Console.WriteLine($"Crocomire tongue collision: nine frames, two empty lists, " +
            $"{probes} native-equivalent touch/shot probes; installed path reads no ROM.");

        ushort ReadWord(int address) => (ushort)(
            rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
    }

    /// <summary>
    /// Address-space sentinel for installed Crocomire tongue paths; every cartridge read or
    /// write fails immediately so the verification can detect any ROM dependency.
    /// </summary>
    private sealed class CrocomireTongueNoReadBus : ISnesAddressSpace, IImportCartridgeSource
    {
        /// <summary>Gets the number of cartridge reads attempted before the sentinel rejected them.</summary>
        internal int ReadAttempts { get; private set; }

        /// <summary>Rejects an import-time read just like any other cartridge access.</summary>
        /// <param name="address">Absolute cartridge address requested.</param>
        /// <returns>This method never returns because the sentinel rejects every read.</returns>
        /// <exception cref="InvalidOperationException">A cartridge read was attempted.</exception>
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        /// <summary>Counts and rejects a cartridge byte read.</summary>
        /// <param name="address">Absolute cartridge address requested.</param>
        /// <returns>This method never returns because all reads are forbidden.</returns>
        /// <exception cref="InvalidOperationException">A cartridge read was attempted.</exception>
        public byte ReadByte(int address)
        {
            ReadAttempts++;
            throw new InvalidOperationException(
                $"Installed Crocomire tongue read ROM byte ${address:X6}.");
        }

        /// <summary>Rejects writes because installed collision logic must not mutate cartridge space.</summary>
        /// <param name="address">Absolute cartridge address targeted by the write.</param>
        /// <param name="value">Byte the caller attempted to write.</param>
        /// <exception cref="InvalidOperationException">A cartridge write was attempted.</exception>
        public void WriteByte(int address, byte value) => throw new InvalidOperationException(
            $"Installed Crocomire tongue wrote ROM byte ${address:X6}.");
    }
}
