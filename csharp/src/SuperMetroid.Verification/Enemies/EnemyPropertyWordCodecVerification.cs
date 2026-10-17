using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Verification.Enemies;
using SuperMetroid.Verification.Harness;

/// <summary>
/// Checks the one codec between the typed enemy property values and their words: every flag and every family or
/// untranslated bit crosses it unchanged in both directions, and the respawn flag reaches the death scratch word as the
/// radius the power-bomb walker reads.
/// </summary>
internal static class EnemyPropertyWordCodecVerification
{
    /// <summary>Round-trips every family byte and every untranslated bit, and checks the respawn scratch word.</summary>
    internal static void VerifyEnemyPropertyWordCodec()
    {
        EnemyProperties[] flags =
        [
            EnemyProperties.Invisible, EnemyProperties.Deleted, EnemyProperties.IgnoreSamusCollision,
            EnemyProperties.ProcessOffScreen, EnemyProperties.BlocksPlasmaBeam, EnemyProperties.ProcessInstructions,
            EnemyProperties.RespawnIfKilled, EnemyProperties.SolidToSamus,
        ];
        for (int familyData = 0; familyData <= byte.MaxValue; familyData++)
        {
            foreach (EnemyProperties flag in flags)
            {
                var value = new EnemyPropertyWord(flag, checked((byte)familyData));
                AssertEqual(familyData, value.FamilyData, "family data keeps its byte value");
                AssertEqual(value, EnemyPropertyWord.FromNativeWord(value.ToNativeWord()), "property word round trips");
                AssertEqual(familyData, value.With(EnemyProperties.Deleted).Without(EnemyProperties.Deleted | flag).FamilyData,
                    "flag operations keep the family data");
            }
        }

        // Every bit outside the translated extra flags survives decoding and encoding unchanged.
        for (int bit = 0; bit < 16; bit++)
        {
            ushort word = checked((ushort)(1 << bit));
            EnemyExtraPropertyWord extra = EnemyExtraPropertyWord.FromNativeWord(word);
            AssertEqual(word, extra.ToNativeWord(), $"extra-property bit {bit} round trips");
            bool translated = extra.HasAny(EnemyExtraProperties.UsesExtendedSpritemap | EnemyExtraProperties.NewInstructionFrame);
            AssertEqual(translated ? 0 : (int)word, extra.UntranslatedBits, $"extra-property bit {bit} lands in the right part");
        }
        EnemyExtraPropertyWord combined = EnemyExtraPropertyWord.FromNativeWord(ushort.MaxValue);
        AssertEqual(ushort.MaxValue, combined.With(EnemyExtraProperties.NewInstructionFrame).ToNativeWord(),
            "the full extra-property word keeps every bit through a flag operation");

        // The death scratch word is the respawn flag's word, which the power-bomb walker then reads as its radius.
        var respawning = new RoomEnemySlot(0, EnemySlotFixtures.Registry) { Properties = new EnemyPropertyWord(EnemyProperties.RespawnIfKilled) };
        var ordinary = new RoomEnemySlot(1, EnemySlotFixtures.Registry) { Properties = new EnemyPropertyWord(EnemyProperties.ProcessInstructions) };
        AssertEqual(16384, EnemyDeathsAndDrops.RespawnScratchWord(respawning), "a respawning death publishes radius 16384");
        AssertEqual(0, EnemyDeathsAndDrops.RespawnScratchWord(ordinary), "an ordinary death publishes radius zero");

        Console.WriteLine(
            "Enemy property word codec: every family byte, flag and untranslated bit round trips; the respawn scratch word is the flag's word.");
    }

    /// <summary>Runs the focused enemy property word codec verification command.</summary>
    [VerificationCommand("--enemy-property-word-codec")]
    internal static int EnemyPropertyWordCodecCommand()
    {
        Suite(nameof(VerifyEnemyPropertyWordCodec), () => VerifyEnemyPropertyWordCodec());
        return 0;
    }
}
