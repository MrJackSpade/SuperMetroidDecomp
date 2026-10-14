using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies.Alcoon;
using SuperMetroid.Core.Enemies.Atomic;
using SuperMetroid.Core.Enemies.Beetom;
using SuperMetroid.Core.Enemies.Boulder;
using SuperMetroid.Core.Enemies.Boyon;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.FaceBlocks;
using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.Core.Enemies.Platforms;
using SuperMetroid.Core.Enemies.Shutters;
using SuperMetroid.Core.Bosses.Kraid.Artwork;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Development-tool members of <see cref="CompiledEnemyVisualSelectors"/>; never linked by player hosts.</summary>
internal static class CompiledEnemyVisualSelectorsTooling
{
    /// <summary>Presentation operands calculated by the individual projectile and enemy instruction catalogs.</summary>
    internal static int CalculatedCount => BeetomInstructionProgramDefinitionsTooling.PresentationWordCount + PlatformInstructionProgramDefinitionsTooling.PresentationWordCount + AlcoonFireballInstructionProgramDefinitions.PresentationWordCount + FuneNamiheFireballInstructionProgramDefinitions.PresentationWordCount + BoyonInstructionProgramDefinitionsTooling.PresentationWordCount +
        FakeKraidInstructionProgramDefinitionsTooling.PresentationWordCount +
        FuneNamiheInstructionProgramDefinitionsTooling.PresentationWordCount + AlcoonInstructionProgramDefinitionsTooling.PresentationWordCount;
    /// <summary>Total visual selector entries across literal and calculated sources.</summary>
    internal static int Count => CompiledEnemyVisualSelectors.Entries.Length + CalculatedCount;
    /// <summary>Returns one selector from the bank/address-ordered merge of literal and calculated entries.</summary>
    internal static CompiledEnemyVisualSelector At(int index)
    {
        if ((uint)index >= Count) throw new IndexOutOfRangeException();
        // Calculated entries are ordered by native bank/address. Their merged rank
        // is their own ordinal plus the number of literal entries before them.
        int low = 0, high = CalculatedCount;
        while (low < high)
        {
            int middle = low + (high - low) / 2;
            CompiledEnemyVisualSelector candidate = CalculatedAt(middle);
            int literalLow = 0, literalHigh = CompiledEnemyVisualSelectors.Entries.Length;
            while (literalLow < literalHigh)
            {
                int literalMiddle = literalLow + (literalHigh - literalLow) / 2;
                if (CompiledEnemyVisualSelectors.Entries[literalMiddle].Address < candidate.Address) literalLow = literalMiddle + 1;
                else literalHigh = literalMiddle;
            }
            int rank = literalLow + middle;
            if (rank == index) return candidate;
            if (rank < index) low = middle + 1;
            else high = middle;
        }
        return CompiledEnemyVisualSelectors.Entries[index - low];
    }
    /// <summary>Calculates an indexed presentation operand and its installed visual frame across native banks.</summary>
    internal static CompiledEnemyVisualSelector CalculatedAt(int index)
    {
        if (index < AlcoonFireballInstructionProgramDefinitions.PresentationWordCount)
        {
            ushort operand = AlcoonFireballInstructionProgramDefinitions.PresentationWordAddress(index);
            return new(0x860000 | operand, EnemyProjectileSpritemapDefinitions.AlcoonFireballFrameAt(operand));
        }
        index -= AlcoonFireballInstructionProgramDefinitions.PresentationWordCount;
        if (index < FuneNamiheFireballInstructionProgramDefinitions.PresentationWordCount)
        {
            ushort operand = FuneNamiheFireballInstructionProgramDefinitions.PresentationWordAddress(index);
            return new(0x860000 | operand, EnemyProjectileSpritemapDefinitions.FuneNamiheFireballFrameAt(operand));
        }
        index -= FuneNamiheFireballInstructionProgramDefinitions.PresentationWordCount;
        if (index < BoyonInstructionProgramDefinitionsTooling.PresentationWordCount)
        {
            ushort operand = BoyonInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            return new(0xa20000 | operand, EnemySpritemapDefinitions.BoyonFrameAt(operand));
        }
        index -= BoyonInstructionProgramDefinitionsTooling.PresentationWordCount;
        if (index < PlatformInstructionProgramDefinitionsTooling.PresentationWordCount)
        {
            ushort operand = PlatformInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            return new(0xa30000 | operand, TripperKamerVisualDefinitions.FrameAt(operand));
        }
        index -= PlatformInstructionProgramDefinitionsTooling.PresentationWordCount;
        if (index < FakeKraidInstructionProgramDefinitionsTooling.PresentationWordCount)
        {
            ushort operand = FakeKraidInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            return new(0xa60000 | operand, KraidVisualDefinitions.FrameAt(EnemyDefinitionId.MiniKraid, operand));
        }
        index -= FakeKraidInstructionProgramDefinitionsTooling.PresentationWordCount;
        if (index < FuneNamiheInstructionProgramDefinitionsTooling.PresentationWordCount)
        {
            ushort operand = FuneNamiheInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            return new(0xa80000 | operand, EnemySpritemapDefinitions.FuneNamiheFrameAt(operand));
        }
        index -= FuneNamiheInstructionProgramDefinitionsTooling.PresentationWordCount;
        if (index < BeetomInstructionProgramDefinitionsTooling.PresentationWordCount)
        {
            ushort operand = BeetomInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            return new(0xa80000 | operand, EnemySpritemapDefinitions.BeetomFrameAt(operand));
        }
        index -= BeetomInstructionProgramDefinitionsTooling.PresentationWordCount;
        if (index < AlcoonInstructionProgramDefinitionsTooling.PresentationWordCount)
        {
            ushort operand = AlcoonInstructionProgramDefinitionsTooling.PresentationWordAddress(index);
            return new(0xa80000 | operand, EnemySpritemapDefinitions.AlcoonFrameAt(operand));
        }
        throw new IndexOutOfRangeException();
    }
}
