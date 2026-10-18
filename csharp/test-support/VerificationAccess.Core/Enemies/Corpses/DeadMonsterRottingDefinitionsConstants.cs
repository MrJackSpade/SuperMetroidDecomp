using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>Cartridge values and helpers of <see cref="DeadMonsterRottingDefinitions"/> that only verification reads.</summary>
internal static class DeadMonsterRottingDefinitionsConstants
{
    /// <summary>Decodes one bank-$A9 corpse row-offset table before calculating its tile-row stride.</summary>
    internal static ushort RotationOffset(ushort table, ushort yOffset) =>
        DeadMonsterRottingDefinitions.RotationOffset(ClosedNativeWords.Decode<CorpseRotationTable>(table, "compiled corpse rotation table"), yOffset);
}
