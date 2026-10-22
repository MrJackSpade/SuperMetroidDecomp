using SuperMetroid.Core.Programs;
using System.Collections.Immutable;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>Cartridge values and helpers of <see cref="DeadMonsterRottingDefinitions"/> that only verification reads.</summary>
internal static class DeadMonsterRottingDefinitionsConstants
{
    /// <summary>Decodes one bank-$A9 corpse row-offset table before calculating its tile-row stride.</summary>
    internal static ushort RotationOffset(ushort table, ushort yOffset) =>
        DeadMonsterRottingDefinitionsConstants.RotationOffset(ClosedNativeWords.Decode<CorpseRotationTable>(table, "compiled corpse rotation table"), yOffset);
    /// <summary>Converts an eight-pixel corpse row offset into its species-specific staged tile byte offset.</summary>
    internal static ushort RotationOffset(CorpseRotationTable table, ushort yOffset)
    {
        (int count, int tiles) = DeadMonsterRottingDefinitions.RotationExtent(table);
        int row = yOffset >> 3;
        if (row >= count) throw new ArgumentOutOfRangeException(nameof(yOffset));
        return (ushort)(row * tiles * 32);
    }
}
