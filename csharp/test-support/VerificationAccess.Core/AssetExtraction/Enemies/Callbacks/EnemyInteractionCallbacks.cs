using System.Collections.Frozen;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Enemies.Callbacks;

/// <summary>
/// Decodes enemy header and hitbox callback words to the reactions the simulation names. The shared prologue entries
/// every enemy bank repeats at the same offset are named by their bank-$A0 copy and admitted from any enemy bank. Every
/// callback that returns at once, the engine's shared ones and a species' own alike, decodes to
/// <see cref="EnemyReaction.Ignore"/>; whether the header's word is the engine's own is decoded separately, as the
/// header's <see cref="EnemyHitboxScan"/>.
/// </summary>
internal static class EnemyInteractionCallbacks
{
    /// <summary>The native long address of each named reaction's callback.</summary>
    private static readonly IReadOnlyDictionary<EnemyReaction, SnesAddress> Addresses =
        new Dictionary<EnemyReaction, SnesAddress>
    {
        [EnemyReaction.NormalEnemyTouch] = EnemyAiRoutineDefinitions.BankA0.NormalEnemyTouch,
        [EnemyReaction.NormalEnemyShot] = EnemyAiRoutineDefinitions.BankA0.NormalEnemyShot,
        [EnemyReaction.DudShot] = EnemyAiRoutineDefinitions.BankA0.DudShot,
        [EnemyReaction.OwtchShot] = EnemyAiRoutineDefinitions.BankA2.OwtchShot,
        [EnemyReaction.DragonTouch] = EnemyAiRoutineDefinitions.BankA2.DragonTouch,
        [EnemyReaction.DragonShot] = EnemyAiRoutineDefinitions.BankA2.DragonShot,
        [EnemyReaction.DragonPowerBomb] = EnemyAiRoutineDefinitions.BankA2.DragonPowerBomb,
        [EnemyReaction.GRipperRipper2Shot] = EnemyAiRoutineDefinitions.BankA2.GRipperRipper2Shot,
        [EnemyReaction.MamaTurtleTouch] = EnemyAiRoutineDefinitions.BankA2.MamaTurtleTouch,
        [EnemyReaction.BabyTurtleTouch] = EnemyAiRoutineDefinitions.BankA2.BabyTurtleTouch,
        [EnemyReaction.BabyTurtleShot] = EnemyAiRoutineDefinitions.BankA2.BabyTurtleShot,
        [EnemyReaction.MaridiaLargeSnailDamagingTouch] = EnemyAiRoutineDefinitions.BankA2.MaridiaLargeSnailDamagingTouch,
        [EnemyReaction.MaridiaLargeSnailNonDamagingTouch] = EnemyAiRoutineDefinitions.BankA2.MaridiaLargeSnailNonDamagingTouch,
        [EnemyReaction.MaridiaLargeSnailShot] = EnemyAiRoutineDefinitions.BankA2.MaridiaLargeSnailShot,
        [EnemyReaction.RinkaTouch] = EnemyAiRoutineDefinitions.BankA2.RinkaTouch,
        [EnemyReaction.RinkaShot] = EnemyAiRoutineDefinitions.BankA2.RinkaShot,
        [EnemyReaction.RinkaPowerBomb] = EnemyAiRoutineDefinitions.BankA2.RinkaPowerBomb,
        [EnemyReaction.VerticalShutterTouch] = EnemyAiRoutineDefinitions.BankA2.VerticalShutterTouch,
        [EnemyReaction.ShootableVerticalShutterShot] = EnemyAiRoutineDefinitions.BankA2.ShootableVerticalShutterShot,
        [EnemyReaction.DestroyableVerticalShutterShot] = EnemyAiRoutineDefinitions.BankA2.DestroyableVerticalShutterShot,
        [EnemyReaction.VerticalShutterPowerBomb] = EnemyAiRoutineDefinitions.BankA2.VerticalShutterPowerBomb,
        [EnemyReaction.SkreeShot] = EnemyAiRoutineDefinitions.BankA3.SkreeShot,
        [EnemyReaction.MetareeShot] = EnemyAiRoutineDefinitions.BankA3.MetareeShot,
        [EnemyReaction.FirefleaTouch] = EnemyAiRoutineDefinitions.BankA3.FirefleaTouch,
        [EnemyReaction.FirefleaPowerBomb] = EnemyAiRoutineDefinitions.BankA3.FirefleaPowerBomb,
        [EnemyReaction.FirefleaShot] = EnemyAiRoutineDefinitions.BankA3.FirefleaShot,
        [EnemyReaction.MochtroidTouch] = EnemyAiRoutineDefinitions.BankA3.MochtroidTouch,
        [EnemyReaction.MochtroidShot] = EnemyAiRoutineDefinitions.BankA3.MochtroidShot,
        [EnemyReaction.MetroidTouch] = EnemyAiRoutineDefinitions.BankA3.MetroidTouch,
        [EnemyReaction.MetroidShot] = EnemyAiRoutineDefinitions.BankA3.MetroidShot,
        [EnemyReaction.MetroidPowerBomb] = EnemyAiRoutineDefinitions.BankA3.MetroidPowerBomb,
        [EnemyReaction.YardTouch] = EnemyAiRoutineDefinitions.BankA3.YardTouch,
        [EnemyReaction.YardShot] = EnemyAiRoutineDefinitions.BankA3.YardShot,
        [EnemyReaction.TripperShot] = EnemyAiRoutineDefinitions.BankA3.TripperShot,
        [EnemyReaction.CrocomireClawTouch] = EnemyAiRoutineDefinitions.BankA4.ClawTouch,
        [EnemyReaction.CrocomireHitCountShot] = EnemyAiRoutineDefinitions.BankA4.HitCountShot,
        [EnemyReaction.CrocomireDustHitboxShot] = EnemyAiRoutineDefinitions.BankA4.DustHitboxShot,
        [EnemyReaction.CrocomireMouthShot] = EnemyAiRoutineDefinitions.BankA4.MouthShot,
        [EnemyReaction.CrocomireAlternateDustHitboxShot] = EnemyAiRoutineDefinitions.BankA4.AlternateDustHitboxShot,
        [EnemyReaction.CrocomirePowerBomb] = EnemyAiRoutineDefinitions.BankA4.PowerBomb,
        [EnemyReaction.SporeSpawnTouch] = EnemyAiRoutineDefinitions.BankA5.SporeSpawnTouch,
        [EnemyReaction.SporeSpawnShot] = EnemyAiRoutineDefinitions.BankA5.SporeSpawnShot,
        [EnemyReaction.DraygonTouch] = EnemyAiRoutineDefinitions.BankA5.DraygonTouch,
        [EnemyReaction.DraygonShot] = EnemyAiRoutineDefinitions.BankA5.DraygonShot,
        [EnemyReaction.DraygonPowerBomb] = EnemyAiRoutineDefinitions.BankA5.DraygonPowerBomb,
        [EnemyReaction.FakeKraidTouch] = EnemyAiRoutineDefinitions.BankA6.FakeKraidTouch,
        [EnemyReaction.FakeKraidShot] = EnemyAiRoutineDefinitions.BankA6.FakeKraidShot,
        [EnemyReaction.CeresSteamTouch] = EnemyAiRoutineDefinitions.BankA6.CeresSteamTouch,
        [EnemyReaction.RidleyFrameHitboxTouch] = EnemyAiRoutineDefinitions.BankA6.RidleyFrameHitboxTouch,
        [EnemyReaction.RidleyShot] = EnemyAiRoutineDefinitions.BankA6.RidleyShot,
        [EnemyReaction.RidleyPowerBomb] = EnemyAiRoutineDefinitions.BankA6.RidleyPowerBomb,
        [EnemyReaction.ZebetiteTouch] = EnemyAiRoutineDefinitions.BankA6.ZebetiteTouch,
        [EnemyReaction.ZebetiteShot] = EnemyAiRoutineDefinitions.BankA6.ZebetiteShot,
        [EnemyReaction.KraidBackgroundTouch] = EnemyAiRoutineDefinitions.BankA7.KraidBackgroundTouch,
        [EnemyReaction.KraidArmTouch] = EnemyAiRoutineDefinitions.BankA7.KraidArmTouch,
        [EnemyReaction.KraidArmShot] = EnemyAiRoutineDefinitions.BankA7.KraidArmShot,
        [EnemyReaction.KraidNailTouch] = EnemyAiRoutineDefinitions.BankA7.KraidNailTouch,
        [EnemyReaction.KraidBadNailTouch] = EnemyAiRoutineDefinitions.BankA7.KraidBadNailTouch,
        [EnemyReaction.EvirTouch] = EnemyAiRoutineDefinitions.BankA8.EvirTouch,
        [EnemyReaction.EvirPowerBomb] = EnemyAiRoutineDefinitions.BankA8.EvirPowerBomb,
        [EnemyReaction.EvirShot] = EnemyAiRoutineDefinitions.BankA8.EvirShot,
        [EnemyReaction.MagdollitePowerBomb] = EnemyAiRoutineDefinitions.BankA8.MagdollitePowerBomb,
        [EnemyReaction.MagdolliteTouch] = EnemyAiRoutineDefinitions.BankA8.MagdolliteTouch,
        [EnemyReaction.MagdolliteShot] = EnemyAiRoutineDefinitions.BankA8.MagdolliteShot,
        [EnemyReaction.BeetomTouch] = EnemyAiRoutineDefinitions.BankA8.BeetomTouch,
        [EnemyReaction.BeetomShot] = EnemyAiRoutineDefinitions.BankA8.BeetomShot,
        [EnemyReaction.PowampTouch] = EnemyAiRoutineDefinitions.BankA8.PowampTouch,
        [EnemyReaction.PowampShot] = EnemyAiRoutineDefinitions.BankA8.PowampShot,
        [EnemyReaction.PowampPowerBomb] = EnemyAiRoutineDefinitions.BankA8.PowampPowerBomb,
        [EnemyReaction.WorkRobotTouch] = EnemyAiRoutineDefinitions.BankA8.WorkRobotTouch,
        [EnemyReaction.WorkRobotNoPowerShot] = EnemyAiRoutineDefinitions.BankA8.WorkRobotNoPowerShot,
        [EnemyReaction.WorkRobotShot] = EnemyAiRoutineDefinitions.BankA8.WorkRobotShot,
        [EnemyReaction.BullShot] = EnemyAiRoutineDefinitions.BankA8.BullShot,
        [EnemyReaction.KagoShot] = EnemyAiRoutineDefinitions.BankA8.KagoShot,
        [EnemyReaction.KiHunterShot] = EnemyAiRoutineDefinitions.BankA8.KiHunterShot,
        [EnemyReaction.SparkShot] = EnemyAiRoutineDefinitions.BankA8.SparkShot,
        [EnemyReaction.BlueBrinstarFaceBlockShot] = EnemyAiRoutineDefinitions.BankA8.BlueBrinstarFaceBlockShot,
        [EnemyReaction.YappingMawTouch] = EnemyAiRoutineDefinitions.BankA8.YappingMawTouch,
        [EnemyReaction.YappingMawShot] = EnemyAiRoutineDefinitions.BankA8.YappingMawShot,
        [EnemyReaction.MotherBrainBodyShot] = EnemyAiRoutineDefinitions.BankA9.MotherBrainBodyShot,
        [EnemyReaction.MotherBrainHeadShot] = EnemyAiRoutineDefinitions.BankA9.MotherBrainHeadShot,
        [EnemyReaction.MotherBrainHeadTouch] = EnemyAiRoutineDefinitions.BankA9.MotherBrainHeadTouch,
        [EnemyReaction.DeadTorizoTouchAndShot] = EnemyAiRoutineDefinitions.BankA9.DeadTorizoTouchAndShot,
        [EnemyReaction.DeadTorizoPowerBomb] = EnemyAiRoutineDefinitions.BankA9.DeadTorizoPowerBomb,
        [EnemyReaction.DeadSidehopperTouch] = EnemyAiRoutineDefinitions.BankA9.DeadSidehopperTouch,
        [EnemyReaction.DeadSidehopperShot] = EnemyAiRoutineDefinitions.BankA9.DeadSidehopperShot,
        [EnemyReaction.DeadSidehopperPowerBomb] = EnemyAiRoutineDefinitions.BankA9.DeadSidehopperPowerBomb,
        [EnemyReaction.ShitroidTouch] = EnemyAiRoutineDefinitions.BankA9.ShitroidTouch,
        [EnemyReaction.ShitroidShot] = EnemyAiRoutineDefinitions.BankA9.ShitroidShot,
        [EnemyReaction.ShitroidPowerBomb] = EnemyAiRoutineDefinitions.BankA9.ShitroidPowerBomb,
        [EnemyReaction.BombTorizoTouch] = EnemyAiRoutineDefinitions.BankAA.BombTorizoTouch,
        [EnemyReaction.BombTorizoShot] = EnemyAiRoutineDefinitions.BankAA.BombTorizoShot,
        [EnemyReaction.TorizoStandUpSitDownShot] = EnemyAiRoutineDefinitions.BankAA.TorizoStandUpSitDownShot,
        [EnemyReaction.GoldenTorizoShot] = EnemyAiRoutineDefinitions.BankAA.GoldenTorizoShot,
        [EnemyReaction.ShaktoolTouch] = EnemyAiRoutineDefinitions.BankAA.ShaktoolTouch,
        [EnemyReaction.ShaktoolShot] = EnemyAiRoutineDefinitions.BankAA.ShaktoolShot,
        [EnemyReaction.SpacePiratePowerBomb] = EnemyAiRoutineDefinitions.BankB2.PowerBomb,
        [EnemyReaction.SpacePirateTouch] = EnemyAiRoutineDefinitions.BankB2.Touch,
        [EnemyReaction.SpacePirateShot] = EnemyAiRoutineDefinitions.BankB2.Shot,
        [EnemyReaction.GoldNinjaVulnerableHitboxShot] = EnemyAiRoutineDefinitions.BankB2.GoldNinjaVulnerableHitboxShot,
        [EnemyReaction.GoldNinjaInvincibleHitboxShot] = EnemyAiRoutineDefinitions.BankB2.GoldNinjaInvincibleHitboxShot,
        [EnemyReaction.BotwoonTouch] = EnemyAiRoutineDefinitions.BankB3.BotwoonTouch,
        [EnemyReaction.BotwoonShot] = EnemyAiRoutineDefinitions.BankB3.BotwoonShot,
        [EnemyReaction.BotwoonPowerBomb] = EnemyAiRoutineDefinitions.BankB3.BotwoonPowerBomb,
        [EnemyReaction.PhantoonTouch] = EnemyAiRoutineDefinitions.BankA7.PhantoonTouch,
        [EnemyReaction.PhantoonShot] = EnemyAiRoutineDefinitions.BankA7.PhantoonShot,
        [EnemyReaction.DeadZoomerTouchAndShot] = EnemyAiRoutineDefinitions.BankA9.DeadZoomerTouchAndShot,
        [EnemyReaction.DeadZoomerPowerBomb] = EnemyAiRoutineDefinitions.BankA9.DeadZoomerPowerBomb,
        [EnemyReaction.DeadRipperTouchAndShot] = EnemyAiRoutineDefinitions.BankA9.DeadRipperTouchAndShot,
        [EnemyReaction.DeadRipperPowerBomb] = EnemyAiRoutineDefinitions.BankA9.DeadRipperPowerBomb,
        [EnemyReaction.DeadSkreeTouchAndShot] = EnemyAiRoutineDefinitions.BankA9.DeadSkreeTouchAndShot,
        [EnemyReaction.DeadSkreePowerBomb] = EnemyAiRoutineDefinitions.BankA9.DeadSkreePowerBomb,
    };

    /// <summary>The named reaction whose callback lies at each native long address.</summary>
    private static readonly FrozenDictionary<SnesAddress, EnemyReaction> ByAddress =
        Addresses.ToFrozenDictionary(pair => pair.Value, pair => pair.Key);

    /// <summary>The reaction a header or hitbox touch word names in <paramref name="bank"/>'s code, or null for a zero word.</summary>
    internal static EnemyReaction? DecodeTouch(int bank, int pointer) =>
        Decode(bank, pointer, EnemyTouchCallbackDefinitions.LiteralNoOps);

    /// <summary>The reaction a header or hitbox shot word names in <paramref name="bank"/>'s code, or null for a zero word.</summary>
    internal static EnemyReaction? DecodeShot(int bank, int pointer) =>
        Decode(bank, pointer, EnemyShotCallbackDefinitions.LiteralNoOps);

    /// <summary>
    /// The reaction a header's Power Bomb word names in <paramref name="bank"/>'s code, or null for a zero word, which
    /// selects the common Power Bomb damage.
    /// </summary>
    internal static EnemyReaction? DecodePowerBomb(int bank, int pointer) =>
        Decode(bank, pointer, EnemyPowerBombCallbackDefinitions.LiteralNoOps);

    /// <summary>
    /// Whether a header's touch or shot word lets the multibox walkers read the enemy's extended hitboxes. The walkers
    /// compare the header's word with the engine's two shared returns before they read any hitbox and withhold the
    /// contact on a match; a species' own return does not match and still lets the hitboxes decide.
    /// </summary>
    internal static EnemyHitboxScan HitboxScan(int pointer) =>
        IsEngineReturn(pointer) ? EnemyHitboxScan.Withheld : EnemyHitboxScan.Scanned;

    /// <summary>
    /// Decodes a callback word: zero names nothing, a return (the engine's shared ones or one of
    /// <paramref name="returns"/>) is <see cref="EnemyReaction.Ignore"/>, a named routine is its reaction, and any other
    /// routine is the species' own.
    /// </summary>
    private static EnemyReaction? Decode(int bank, int pointer, IReadOnlySet<SnesAddress> returns) =>
        pointer == 0 ? null
        : IsEngineReturn(pointer) || returns.Contains(SnesAddress.InBank(bank, pointer)) ? EnemyReaction.Ignore
        : TryName(bank, pointer) ?? EnemyReaction.SpeciesOwn;

    /// <summary>Whether a word names one of the engine's two shared returns, which every enemy bank repeats.</summary>
    private static bool IsEngineReturn(int pointer)
    {
        SnesAddress common = SnesAddress.InBank(EnemyAiRoutineDefinitions.BankA0.Bank, pointer);
        return common == EnemyAiRoutineDefinitions.BankA0.NoOp || common == EnemyAiRoutineDefinitions.BankA0.NoOpShortReturn;
    }

    /// <summary>Names the reaction a routine word selects in <paramref name="bank"/>'s code, or null when it has no name.</summary>
    private static EnemyReaction? TryName(int bank, int pointer)
    {
        if (ByAddress.TryGetValue(SnesAddress.InBank(EnemyAiRoutineDefinitions.BankA0.Bank, pointer), out EnemyReaction common) &&
            common is EnemyReaction.NormalEnemyTouch or EnemyReaction.NormalEnemyShot or EnemyReaction.DudShot)
            return common;
        return ByAddress.TryGetValue(SnesAddress.InBank(bank, pointer), out EnemyReaction local) ? local : null;
    }

    /// <summary>Whether a header's initialization or main-AI word names the shared no-op or the species' routine.</summary>
    internal static EnemyAiRoutine AiRoutine(int bank, int pointer) =>
        EnemyAiRoutineDefinitions.CommonNoOpAiRoutines.Contains(SnesAddress.InBank(bank, pointer))
            ? EnemyAiRoutine.Inert
            : EnemyAiRoutine.SpeciesRoutine;

    /// <summary>
    /// Whether a header's hurt-AI word names the shared no-op or the species' routine. Every enemy bank repeats the
    /// shared long return at the same offset, so the word is read as its bank-$A0 copy.
    /// </summary>
    internal static EnemyAiRoutine HurtRoutine(int pointer) =>
        SnesAddress.InBank(EnemyAiRoutineDefinitions.BankA0.Bank, pointer) == EnemyAiRoutineDefinitions.BankA0.NoOp
            ? EnemyAiRoutine.Inert
            : EnemyAiRoutine.SpeciesRoutine;
}
