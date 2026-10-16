using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Enemies;
using SuperMetroid.TestSupport.CpuBusTestDoubles;

namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>One stationary retail Ripper in a constructed population; its AI/header remain ROM-authored.</summary>
[CpuBusBoundary("exact bus test: constructed enemy population fixture overlaying one Ripper's population and tileset bytes on a live bus")]
internal sealed class CrystalFlashContactPopulation(ISnesAddressSpace inner, Position16<RoomPixels> position) :
    ISnesAddressSpace, IRoomEnemyDefinitionSource
{
    /// <summary>Uses the retail enemy header for the constructed Ripper population.</summary>
    public RoomEnemyDefinition ReadEnemyDefinition(EnemyDefinitionId pointer) =>
        RoomEnemyDefinitionCatalog.Get(pointer);

    /// <summary>Places one stationary Ripper at Samus's contact position for the damage frame.</summary>
    public RoomEnemyPopulationDefinition ReadEnemyPopulation(ushort pointer)
    {
        if (pointer != CrystalFlashContactDefinitions.Pointer)
            throw new ArgumentOutOfRangeException(nameof(pointer));
        return new RoomEnemyPopulationDefinition(pointer,
        [
            new RoomEnemyPopulationRecord(CrystalFlashContactDefinitions.RipperHeader, position, 0, CrystalFlashContactDefinitions.Properties, 0, 0, 0),
        ], 0);
    }

    /// <summary>Associates the constructed population with its retail Ripper graphics header.</summary>
    public RoomEnemyGraphicsSetDefinition ReadEnemyGraphicsSet(ushort pointer)
    {
        if (pointer != CrystalFlashContactDefinitions.Pointer)
            throw new ArgumentOutOfRangeException(nameof(pointer));
        return new RoomEnemyGraphicsSetDefinition(pointer,
        [
            new RoomEnemyGraphicsSetHeader(CrystalFlashContactDefinitions.RipperHeader, 0),
        ]);
    }

    /// <summary>Uses the compiled falling-tube placement; this fixture constructs none.</summary>
    public RoomEnemyPopulationRecord ReadFallingTubePopulation(SuperMetroid.Core.Bosses.MotherBrain.MotherBrainTubePiece piece) =>
        CompiledRoomEnemyDefinitions.Instance.ReadFallingTubePopulation(piece);

    /// <summary>Overlays synthetic population and tileset records while delegating other addresses to the retail bus.</summary>
    public byte ReadByte(int address)
    {
        ReadOnlySpan<ushort> population = [(ushort)CrystalFlashContactDefinitions.RipperHeader, position.X, position.Y, 0,
            CrystalFlashContactDefinitions.Properties, 0, 0, 0, 0xffff, 0];
        ReadOnlySpan<ushort> tileset = [(ushort)CrystalFlashContactDefinitions.RipperHeader, 0, 0xffff];
        int offset = address - CrystalFlashContactDefinitions.PopulationAddress;
        if ((uint)offset < population.Length * 2)
            return (byte)(population[offset / 2] >> ((offset & 1) * 8));
        offset = address - CrystalFlashContactDefinitions.TilesetAddress;
        if ((uint)offset < tileset.Length * 2)
            return (byte)(tileset[offset / 2] >> ((offset & 1) * 8));
        return CpuBusRegionRead.Read(inner, address);
    }
    /// <summary>Forwards mutable bus writes outside the fixture's read-only population overlay.</summary>
    public void WriteByte(int address, byte value) => inner.WriteByte(address, value);
}
