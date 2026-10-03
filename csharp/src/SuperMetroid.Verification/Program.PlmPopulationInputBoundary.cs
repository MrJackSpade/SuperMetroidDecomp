using System.Reflection;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Desktop;

internal static partial class Program
{
    /// <summary>Verifies the identified typed-input conversion, without importing a ROM or driving gameplay.</summary>
    private static void VerifyPlmPopulationInputBoundary()
    {
        byte[] pairs = [1, (byte)RoomScrollState.Green, 0x80];
        var trigger = new RoomPlmPlacement(RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.ScrollTrigger),
            3, 3, 0x9100, pairs);
        var extension = new RoomPlmPlacement(RoomPlmHeaderDefinitions.Get(RoomPlmHeaders.RightwardsScrollExtension),
            4, 3, 0x8000);
        var second = trigger with { BlockX = 5, ScrollProgram = new byte[] { 0x80 } };
        RoomPlmPlacement[] records = [trigger, extension, second];
        var population = new RoomPlmPopulationDefinition(0xfffa, records);
        pairs[0] = 49;
        pairs[1] = 0;
        records[0] = second;
        AssertEqual((byte)1, population.Placements.Span[0].ScrollProgram.Span[0],
            "decoded population owns its scroll pairs independently of caller mutation");
        AssertEqual((byte)3, population.Placements.Span[0].BlockX,
            "decoded population owns its record ordering independently of caller mutation");

        var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        var level = new RoomLevelData(8, 8, new ushort[64], new byte[64], new ushort[64], new byte[8]);
        var plms = new RoomPlmSystem();
        var streamer = level.CreateBackgroundStreamer();
        AssertEqual(3, plms.LoadRoomPopulation(memory, level, streamer, new SnesVram(), population,
            new Bank80SystemState(), AreaId.Crateria, () => new SamusState(), () => false),
            "typed population runs the real sequential allocator using a RAM-only bus");
        AssertTrue(plms.PopulationSlots.Select(slot => slot.NativeSlotIndex).SequenceEqual(new[] { 39, 38 }),
            "extension deletion permits immediate highest-slot reuse in authored order");
        AssertEqual(RoomBlockBehaviorValues.ScrollTrigger.Value, level.GetCollisionBlock(3, 3).Behavior,
            "typed trigger setup publishes its native collision behavior");
        AssertEqual((int)RoomCollisionType.HorizontalExtension, (int)level.GetCollisionBlock(4, 3).CollisionType,
            "typed extension setup publishes its native collision type");
        var scrolls = RoomScrollGrid.CreateImplicit(memory, 2, 2, RoomScrollState.RedBoundary);
        AssertTrue(plms.TryNotifyScrollTouch(level.GetBlockIndex(3, 3)), "typed trigger wakes at its placement");
        plms.Step(memory, level, streamer, 0, 0, 0, scrolls);
        AssertEqual((byte)RoomScrollState.Green, scrolls.ReadStorage(1), "decoded pairs mutate the intended live scroll cell");
        AssertTrue(!plms.ScrollPlms[0].Triggered, "decoded trigger returns to native sleep after its terminator");

        AssertThrows<InvalidDataException>(() => new RoomPlmPopulationDefinition(0x9000,
            [trigger with { ScrollProgram = new byte[] { 0, 2 } }]), "unterminated decoded pairs are rejected before allocation");
        AssertThrows<InvalidDataException>(() => new RoomPlmPopulationDefinition(0x9000,
            [trigger with { ScrollProgram = new byte[] { 0, 3, 0x80 } }]), "invalid decoded scroll states are rejected");
        AssertThrows<InvalidDataException>(() => new RoomPlmPopulationDefinition(0x9000,
            Enumerable.Repeat(second, RoomPlmPopulationFormat.MaximumParserIterations)),
            "typed records cannot evade the native bounded terminator limit");
        AssertThrows<InvalidDataException>(() => new RoomPlmPopulationDefinition(0x9000,
            [extension with { ScrollProgram = new byte[] { 0x80 } }]), "an extension cannot own a resident scroll program");

        int count = 0;
        foreach (ushort pointer in RoomPlmPopulationDefinitions.Pointers)
            count += RoomPlmPopulationDefinition.FromCompiled(pointer).Placements.Length;
        AssertEqual(RoomPlmPopulationDefinitions.RetailRecordCount, count,
            "every compiled population converts to the typed contract before gameplay");
        Type scrollType = typeof(RoomPlmSystem).GetNestedType("ScrollPlmState", BindingFlags.NonPublic)!;
        FieldInfo[] fields = scrollType.GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
            .OrderBy(field => field.MetadataToken).ToArray();
        string[] historicalFields = ["<Triggered>k__BackingField", "<UseCompiledRetailProgram>k__BackingField", "<Program>k__BackingField"];
        foreach (int legacyCount in new[] { 1, 2, 3 })
        {
            FieldInfo[] legacy = DebuggerStateFieldMigrations.SelectSerializedFields(scrollType, fields, legacyCount);
            AssertEqual(legacyCount, legacy.Length, "historical scroll state schema remains readable");
            AssertTrue(legacy.Select(field => field.Name).SequenceEqual(historicalFields.Take(legacyCount)),
                "historical scroll fields retain their serialized order and decoded pairs when present");
        }
        Console.WriteLine("PLM input boundary: 284 compiled populations/941 records, immutable pairs, native slot reuse, RAM-only scroll execution, bounded validation and all three legacy scroll schemas pass.");
    }
}
