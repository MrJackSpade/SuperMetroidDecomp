using SuperMetroid.Core.Enemies.Alcoon;
using SuperMetroid.Core.Enemies.FuneNamihe;
using SuperMetroid.Core.Enemies.FuneNamihe.Artwork;
using SuperMetroid.Core.Enemies.Alcoon.Artwork;
using SuperMetroid.Core.Enemies.Atomic;
using SuperMetroid.Core.Enemies.Boulder;
using SuperMetroid.Core.Enemies.Boyon;
using SuperMetroid.Core.Enemies.Elevator;
using SuperMetroid.Core.Enemies.FaceBlocks;
using SuperMetroid.Core.Enemies.Platforms;
using SuperMetroid.Core.Enemies.Shutters;
using SuperMetroid.Core.Enemies.Projectiles;

namespace SuperMetroid.Core.Enemies.Artwork;

/// <summary>Development-tool members of <see cref="CompiledEnemyVisualSelectors"/>; never linked by player hosts.</summary>
internal static class CompiledEnemyVisualSelectorsTooling
{
    /// <summary>Presentation operands calculated by the individual projectile and enemy instruction catalogs.</summary>
    internal static int CalculatedCount => AlcoonFireballFrameDefinitions.PoseCount + FuneNamiheFireballFrameDefinitions.PoseCount;
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
        if (index < AlcoonFireballFrameDefinitions.PoseCount)
        {
            AlcoonFireballFrame fireball = new(index);
            return new(0x860000 | EnemyProjectileFrames.ProgramFrameKey(AlcoonFireballFrameDefinitions.ShownProjectileFrames[fireball]), AlcoonFireballFrameDefinitions.Spritemap(fireball));
        }
        index -= AlcoonFireballFrameDefinitions.PoseCount;
        if (index < FuneNamiheFireballFrameDefinitions.PoseCount)
        {
            FuneNamiheFireballFrame fune = new(index);
            return new(0x860000 | EnemyProjectileFrames.ProgramFrameKey(FuneNamiheFireballFrameDefinitions.ShownProjectileFrames[fune]), FuneNamiheFireballFrameDefinitions.Spritemap(fune));
        }
        throw new IndexOutOfRangeException();
    }
}
