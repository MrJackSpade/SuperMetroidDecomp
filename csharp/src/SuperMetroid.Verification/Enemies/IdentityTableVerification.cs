using SuperMetroid.Core;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.MotherBrain.Artwork;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Cacatac;
using SuperMetroid.Core.Enemies.Crawlers;
using SuperMetroid.Core.Enemies.Sbug;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies;

/// <summary>
/// Confirms the enemy and boss identity tables of #1418 select what the enums' numeric values used to select: each
/// authored sequence or parameter table pairs every number with the member that number named before Core stopped
/// reading enum values, and Phantoon's word F keeps its function and number views apart while a held function still
/// reads as a completed fade.
/// </summary>
internal static class IdentityTableVerification
{
    /// <summary>Checks every table against the numbering the casts it replaced relied on.</summary>
    internal static void VerifyIdentityTables()
    {
        SequenceMatchesValues(BabyMetroidInitialPalette.ColorOrder, 15, value => (BabyMetroidInitialColorSlot)value, "Baby Metroid colour order");
        SequenceMatchesValues(MotherBrainTurretDefinitions.Ring, 8, value => (MotherBrainTurretDirection)value, "Mother Brain turret ring");
        SequenceMatchesValues(MotherBrainFallingTubePopulationDefinitions.Pieces, 5, value => (MotherBrainTubePiece)value, "Mother Brain tube pieces");
        SequenceMatchesValues(CrawlerAnimationDefinitions.Orientations, 4, value => (CrawlerSurfaceOrientation)value, "crawler orientations");
        SequenceMatchesValues(SbugMovementDefinitions.ActivationBehaviors, 7, value => (SbugActivationBehavior)value, "Sbug activation behaviours");

        ParametersMatchValues(CeresSteamDefinitions.Variants, [0, 1, 2, 3, 4, 5], value => (CeresSteamVariant)value, "Ceres steam variants");
        ParametersMatchValues(CacatacProjectileDefinitions.Directions, [0, 1, 2], value => (CacatacDirection)value, "Cacatac directions");
        ParametersMatchValues(CacatacProjectileDefinitions.SpikeSelectors, [0, 2, 4, 6, 8, 10, 12, 14, 16, 18],
            value => (CacatacSpikeDirection)value, "Cacatac spike selectors");

        int[] breakupOrder = [12, 10, 8, 6, 4, 2, 0, 14, 16, 20, 18, 22];
        var spawned = new List<int>();
        RidleyBreakup.SpawnInBreakupOrder(spawned.Add);
        AssertEqual(breakupOrder.Length, spawned.Count, "Ridley breakup spawns twelve parts");
        for (int index = 0; index < breakupOrder.Length; index++)
            AssertEqual(breakupOrder[index], spawned[index], $"Ridley breakup spawn {index} carries its authored parameter");

        var eye = new PhantoonPartState();
        eye.Function = PhantoonAiFunction.SpawnStartingFlames;
        AssertTrue(eye.FadeComplete, "a part whose word F holds a function reads as a completed fade");
        eye.FadeComplete = false;
        AssertTrue(!eye.FadeComplete, "clearing the fade replaces the held function");
        AssertEqual(0, eye.RageWaveCount, "the cleared word F counts no rage waves");
        eye.RageWaveCount = 3;
        AssertEqual(3, eye.RageWaveCount, "word F keeps the rage-wave count");
        AssertTrue(eye.FadeComplete, "a nonzero count also reads as a completed fade");
        Console.WriteLine("Identity tables: sequences, parameter tables, breakup order and Phantoon's word F select as the numbering did.");
    }

    /// <summary>Asserts position <c>n</c> of <paramref name="sequence"/> holds the member valued <c>n</c>.</summary>
    private static void SequenceMatchesValues<T>(IdentitySequence<T> sequence, int count, Func<int, T> member, string name)
        where T : struct, Enum
    {
        AssertEqual(count, sequence.Count, $"{name} count");
        for (int position = 0; position < count; position++)
        {
            AssertEqual(member(position), sequence.At(position), $"{name} position {position}");
            AssertEqual(position, sequence.PositionOf(member(position)), $"{name} member {member(position)}");
        }
        AssertThrows<ArgumentOutOfRangeException>(() => sequence.At(count), $"{name} rejects a position past its end");
    }

    /// <summary>Asserts each authored value of <paramref name="table"/> selects the member valued the same.</summary>
    private static void ParametersMatchValues<T>(ParameterVariants<T> table, int[] values, Func<int, T> member, string name)
        where T : struct, Enum
    {
        foreach (int value in values)
        {
            AssertEqual(member(value), table.Decode(value), $"{name} value {value}");
            AssertEqual(value, table.Parameter(member(value)), $"{name} member {member(value)}");
        }
        AssertTrue(!table.TryDecode(values[^1] + 1, out _), $"{name} rejects an unauthored value");
    }

    /// <summary>Runs the focused identity-table check.</summary>
    [VerificationCommand("--identity-tables-1418")]
    internal static int IdentityTablesCommand()
    {
        VerifyIdentityTables();
        return 0;
    }
}
