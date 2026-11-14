using System.Collections.Frozen;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies.MaridiaSnails;
using SuperMetroid.Core.Enemies.SpacePirates;

namespace SuperMetroid.AssetExtraction.Enemies.Callbacks;

/// <summary>
/// Decodes enemy header and hitbox callback words to the reactions the simulation names (#1759). The shared prologue
/// entries every enemy bank repeats at the same offset are named by their bank-$A0 copy and admitted from any enemy bank.
/// Every callback that returns at once, the engine's shared ones and a species' own alike, decodes to
/// <see cref="EnemyReaction.Ignore"/>; whether the header's word is the engine's own is decoded separately, as the
/// header's <see cref="EnemyHitboxScan"/>. A header names only the engine's shared reactions: any other routine is the
/// species' own, which the definition leaves to the species' entity and so names no reaction. A hitbox may name one of
/// its species' own reactions, which decodes to that species' identity.
/// </summary>
internal static class EnemyInteractionCallbacks
{
    /// <summary>The native long address of each shared reaction's and each hitbox-named species reaction's callback.</summary>
    private static readonly IReadOnlyDictionary<EnemyReaction, SnesAddress> Addresses =
        new Dictionary<EnemyReaction, SnesAddress>
    {
        [EnemyReaction.NormalEnemyTouch] = EnemyAiRoutineDefinitions.BankA0.NormalEnemyTouch,
        [EnemyReaction.NormalEnemyShot] = EnemyAiRoutineDefinitions.BankA0.NormalEnemyShot,
        [EnemyReaction.DudShot] = EnemyAiRoutineDefinitions.BankA0.DudShot,
        [MaridiaLargeSnailReactions.DamagingTouch] = EnemyAiRoutineDefinitions.BankA2.MaridiaLargeSnailDamagingTouch,
        [MaridiaLargeSnailReactions.NonDamagingTouch] = EnemyAiRoutineDefinitions.BankA2.MaridiaLargeSnailNonDamagingTouch,
        [MaridiaLargeSnailReactions.Shot] = EnemyAiRoutineDefinitions.BankA2.MaridiaLargeSnailShot,
        [CrocomireReactions.ClawTouch] = EnemyAiRoutineDefinitions.BankA4.ClawTouch,
        [CrocomireReactions.HitCountShot] = EnemyAiRoutineDefinitions.BankA4.HitCountShot,
        [CrocomireReactions.DustHitboxShot] = EnemyAiRoutineDefinitions.BankA4.DustHitboxShot,
        [CrocomireReactions.MouthShot] = EnemyAiRoutineDefinitions.BankA4.MouthShot,
        [CrocomireReactions.AlternateDustHitboxShot] = EnemyAiRoutineDefinitions.BankA4.AlternateDustHitboxShot,
        [SporeSpawnReactions.Touch] = EnemyAiRoutineDefinitions.BankA5.SporeSpawnTouch,
        [SporeSpawnReactions.Shot] = EnemyAiRoutineDefinitions.BankA5.SporeSpawnShot,
        [DraygonReactions.Touch] = EnemyAiRoutineDefinitions.BankA5.DraygonTouch,
        [DraygonReactions.Shot] = EnemyAiRoutineDefinitions.BankA5.DraygonShot,
        [CeresSteamReactions.Touch] = EnemyAiRoutineDefinitions.BankA6.CeresSteamTouch,
        [RidleyReactions.FrameHitboxTouch] = EnemyAiRoutineDefinitions.BankA6.RidleyFrameHitboxTouch,
        [RidleyReactions.Shot] = EnemyAiRoutineDefinitions.BankA6.RidleyShot,
        [KraidReactions.BackgroundTouch] = EnemyAiRoutineDefinitions.BankA7.KraidBackgroundTouch,
        [KraidReactions.ArmTouch] = EnemyAiRoutineDefinitions.BankA7.KraidArmTouch,
        [KraidReactions.ArmShot] = EnemyAiRoutineDefinitions.BankA7.KraidArmShot,
        [MotherBrainReactions.BodyShot] = EnemyAiRoutineDefinitions.BankA9.MotherBrainBodyShot,
        [MotherBrainReactions.HeadShot] = EnemyAiRoutineDefinitions.BankA9.MotherBrainHeadShot,
        [MotherBrainReactions.HeadTouch] = EnemyAiRoutineDefinitions.BankA9.MotherBrainHeadTouch,
        [TorizoReactions.Touch] = EnemyAiRoutineDefinitions.BankAA.BombTorizoTouch,
        [TorizoReactions.Shot] = EnemyAiRoutineDefinitions.BankAA.BombTorizoShot,
        [TorizoReactions.StandUpSitDownShot] = EnemyAiRoutineDefinitions.BankAA.TorizoStandUpSitDownShot,
        [TorizoReactions.GoldenShot] = EnemyAiRoutineDefinitions.BankAA.GoldenTorizoShot,
        [SpacePirateReactions.Touch] = EnemyAiRoutineDefinitions.BankB2.Touch,
        [SpacePirateReactions.Shot] = EnemyAiRoutineDefinitions.BankB2.Shot,
        [SpacePirateReactions.GoldNinjaVulnerableHitboxShot] = EnemyAiRoutineDefinitions.BankB2.GoldNinjaVulnerableHitboxShot,
        [SpacePirateReactions.GoldNinjaInvincibleHitboxShot] = EnemyAiRoutineDefinitions.BankB2.GoldNinjaInvincibleHitboxShot,
        [PhantoonReactions.Touch] = EnemyAiRoutineDefinitions.BankA7.PhantoonTouch,
        [PhantoonReactions.Shot] = EnemyAiRoutineDefinitions.BankA7.PhantoonShot,
    };

    /// <summary>The named reaction whose callback lies at each native long address.</summary>
    private static readonly FrozenDictionary<SnesAddress, EnemyReaction> ByAddress =
        Addresses.ToFrozenDictionary(pair => pair.Value, pair => pair.Key);

    /// <summary>
    /// The shared reaction a header's touch word names in <paramref name="bank"/>'s code, or null for a zero word or the
    /// species' own routine.
    /// </summary>
    internal static EnemyReaction? DecodeTouch(int bank, int pointer) =>
        DecodeHeader(bank, pointer, EnemyTouchCallbackDefinitions.LiteralNoOps);

    /// <summary>
    /// The shared reaction a header's shot word names in <paramref name="bank"/>'s code, or null for a zero word or the
    /// species' own routine.
    /// </summary>
    internal static EnemyReaction? DecodeShot(int bank, int pointer) =>
        DecodeHeader(bank, pointer, EnemyShotCallbackDefinitions.LiteralNoOps);

    /// <summary>
    /// The shared reaction a header's Power Bomb word names in <paramref name="bank"/>'s code, or null for a zero word,
    /// which selects the common Power Bomb damage, or for the species' own routine.
    /// </summary>
    internal static EnemyReaction? DecodePowerBomb(int bank, int pointer) =>
        DecodeHeader(bank, pointer, EnemyPowerBombCallbackDefinitions.LiteralNoOps);

    /// <summary>The reaction a hitbox's touch word names in <paramref name="bank"/>'s code: shared, or its species' own.</summary>
    /// <exception cref="InvalidDataException">The word names a species routine no hitbox reaction is declared for.</exception>
    internal static EnemyReaction? DecodeHitboxTouch(int bank, int pointer) =>
        DecodeHitbox(bank, pointer, EnemyTouchCallbackDefinitions.LiteralNoOps);

    /// <summary>The reaction a hitbox's shot word names in <paramref name="bank"/>'s code: shared, or its species' own.</summary>
    /// <exception cref="InvalidDataException">The word names a species routine no hitbox reaction is declared for.</exception>
    internal static EnemyReaction? DecodeHitboxShot(int bank, int pointer) =>
        DecodeHitbox(bank, pointer, EnemyShotCallbackDefinitions.LiteralNoOps);

    /// <summary>
    /// Whether a header's touch or shot word lets the multibox walkers read the enemy's extended hitboxes. The walkers
    /// compare the header's word with the engine's two shared returns before they read any hitbox and withhold the
    /// contact on a match; a species' own return does not match and still lets the hitboxes decide.
    /// </summary>
    internal static EnemyHitboxScan HitboxScan(int pointer) =>
        IsEngineReturn(pointer) ? EnemyHitboxScan.Withheld : EnemyHitboxScan.Scanned;

    /// <summary>
    /// Decodes a header's callback word: zero names nothing, a return (the engine's shared ones or one of
    /// <paramref name="returns"/>) is <see cref="EnemyReaction.Ignore"/>, a shared routine is its reaction, and any other
    /// routine is the species' own, which the definition leaves to the species.
    /// </summary>
    private static EnemyReaction? DecodeHeader(int bank, int pointer, IReadOnlySet<SnesAddress> returns) =>
        pointer == 0 ? null
        : IsEngineReturn(pointer) || returns.Contains(SnesAddress.InBank(bank, pointer)) ? EnemyReaction.Ignore
        : TryNameShared(pointer);

    /// <summary>
    /// Decodes a hitbox's callback word: zero names nothing, a return is <see cref="EnemyReaction.Ignore"/>, a shared
    /// routine is its reaction, and a species routine is the species' own reaction its hitboxes name.
    /// </summary>
    private static EnemyReaction? DecodeHitbox(int bank, int pointer, IReadOnlySet<SnesAddress> returns) =>
        pointer == 0 ? null
        : IsEngineReturn(pointer) || returns.Contains(SnesAddress.InBank(bank, pointer)) ? EnemyReaction.Ignore
        : TryNameShared(pointer) ??
          (ByAddress.TryGetValue(SnesAddress.InBank(bank, pointer), out EnemyReaction? local)
              ? local
              : throw new InvalidDataException(
                  $"Hitbox callback ${bank:X2}:{pointer:X4} names a species routine no hitbox reaction is declared for."));

    /// <summary>Whether a word names one of the engine's two shared returns, which every enemy bank repeats.</summary>
    private static bool IsEngineReturn(int pointer)
    {
        SnesAddress common = SnesAddress.InBank(EnemyAiRoutineDefinitions.BankA0.Bank, pointer);
        return common == EnemyAiRoutineDefinitions.BankA0.NoOp || common == EnemyAiRoutineDefinitions.BankA0.NoOpShortReturn;
    }

    /// <summary>The shared reaction a routine word selects through its bank-$A0 copy, or null for a species routine.</summary>
    private static EnemyReaction? TryNameShared(int pointer)
    {
        if (ByAddress.TryGetValue(SnesAddress.InBank(EnemyAiRoutineDefinitions.BankA0.Bank, pointer), out EnemyReaction? common) &&
            (common == EnemyReaction.NormalEnemyTouch || common == EnemyReaction.NormalEnemyShot || common == EnemyReaction.DudShot))
            return common;
        return null;
    }

    /// <summary>Whether a header's initialization or main-AI word names the shared no-op rather than the species' routine.</summary>
    internal static bool NamesSharedNoOp(int bank, int pointer) =>
        EnemyAiRoutineDefinitions.CommonNoOpAiRoutines.Contains(SnesAddress.InBank(bank, pointer));

    /// <summary>
    /// The hurt response a header's hurt-AI word names: the shared no-op or the species' routine. Every enemy bank repeats
    /// the shared long return at the same offset, so the word is read as its bank-$A0 copy.
    /// </summary>
    internal static EnemyHurtResponse HurtResponse(int pointer) =>
        SnesAddress.InBank(EnemyAiRoutineDefinitions.BankA0.Bank, pointer) == EnemyAiRoutineDefinitions.BankA0.NoOp
            ? EnemyHurtResponse.Inert
            : EnemyHurtResponse.SpeciesOwn;
}
