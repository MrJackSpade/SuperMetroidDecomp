using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Runtime;

/// <summary>Ordinary production enemy contact for room-local movement comparisons.</summary>
internal static class MovementContactFixture
{
    public static void ApplyZoomerTouch(SuperMetroidRuntime runtime, SuperMetroidAddressSpace bus, ushort input, int xOffset)
    {
        var samus = runtime.Samus ?? throw new InvalidOperationException("Contact fixture requires Samus.");
        var level = runtime.LevelData ?? throw new InvalidOperationException("Contact fixture requires room data.");
        var selected = new PopulationSelectionAddressSpace(bus,
            [new RoomEnemyPopulationRecord(DownbackFixtureData.ZoomerDefinition, (ushort)(samus.XPosition + xOffset), samus.YPosition,
                0, (ushort)EnemyProperties.ProcessInstructions, 0, 0, 0)]);
        runtime.Enemies.Load(selected, PopulationSelectionAddressSpace.PopulationPointer,
            PopulationSelectionAddressSpace.TilesetPointer, new SnesVram(), new SnesCgram(), () => 0, samus: samus);
        // Initialize the real map/interaction list without a Samus collision, then
        // restore the authored overlap before executing ordinary production contact.
        runtime.Enemies.StepFrame((ushort)(samus.XPosition - 128), (ushort)(samus.YPosition - 128), false, level: level);
        runtime.Enemies.Slots[0].XPosition = (ushort)(samus.XPosition + xOffset);
        runtime.Enemies.Slots[0].YPosition = samus.YPosition;
        if (!runtime.Enemies.ResolveOrdinarySamusContact(samus, input, level))
            throw new InvalidDataException("Movement fixture Zoomer contact did not overlap Samus.");
        foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
    }
}
