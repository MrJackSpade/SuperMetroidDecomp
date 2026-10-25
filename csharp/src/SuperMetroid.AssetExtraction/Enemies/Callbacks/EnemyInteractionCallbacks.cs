using System.Collections.Frozen;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction.Enemies.Callbacks;

/// <summary>
/// Decodes enemy header and hitbox callback words to the interaction callbacks the port names. The shared prologue
/// entries every enemy bank repeats at the same offset are named by their bank-$A0 copy and admitted from any enemy bank.
/// </summary>
internal static class EnemyInteractionCallbacks
{
    /// <summary>The native long address of each named callback.</summary>
    internal static readonly IReadOnlyDictionary<EnemyInteractionCallback, SnesAddress> Addresses =
        new Dictionary<EnemyInteractionCallback, SnesAddress>
    {
        [EnemyInteractionCallback.NormalEnemyTouch] = EnemyAiRoutineDefinitions.BankA0.NormalEnemyTouch,
        [EnemyInteractionCallback.NormalEnemyShot] = EnemyAiRoutineDefinitions.BankA0.NormalEnemyShot,
        [EnemyInteractionCallback.DudShot] = EnemyAiRoutineDefinitions.BankA0.DudShot,
        [EnemyInteractionCallback.NoOp] = EnemyAiRoutineDefinitions.BankA0.NoOp,
        [EnemyInteractionCallback.NoOpShortReturn] = EnemyAiRoutineDefinitions.BankA0.NoOpShortReturn,
        [EnemyInteractionCallback.OwtchShot] = EnemyAiRoutineDefinitions.BankA2.OwtchShot,
        [EnemyInteractionCallback.DragonTouch] = EnemyAiRoutineDefinitions.BankA2.DragonTouch,
        [EnemyInteractionCallback.DragonShot] = EnemyAiRoutineDefinitions.BankA2.DragonShot,
        [EnemyInteractionCallback.DragonPowerBomb] = EnemyAiRoutineDefinitions.BankA2.DragonPowerBomb,
        [EnemyInteractionCallback.GRipperRipper2Shot] = EnemyAiRoutineDefinitions.BankA2.GRipperRipper2Shot,
        [EnemyInteractionCallback.MamaTurtleTouch] = EnemyAiRoutineDefinitions.BankA2.MamaTurtleTouch,
        [EnemyInteractionCallback.BabyTurtleTouch] = EnemyAiRoutineDefinitions.BankA2.BabyTurtleTouch,
        [EnemyInteractionCallback.BabyTurtleShot] = EnemyAiRoutineDefinitions.BankA2.BabyTurtleShot,
        [EnemyInteractionCallback.MaridiaLargeSnailDamagingTouch] = EnemyAiRoutineDefinitions.BankA2.MaridiaLargeSnailDamagingTouch,
        [EnemyInteractionCallback.MaridiaLargeSnailNonDamagingTouch] = EnemyAiRoutineDefinitions.BankA2.MaridiaLargeSnailNonDamagingTouch,
        [EnemyInteractionCallback.MaridiaLargeSnailShot] = EnemyAiRoutineDefinitions.BankA2.MaridiaLargeSnailShot,
        [EnemyInteractionCallback.RinkaTouch] = EnemyAiRoutineDefinitions.BankA2.RinkaTouch,
        [EnemyInteractionCallback.RinkaShot] = EnemyAiRoutineDefinitions.BankA2.RinkaShot,
        [EnemyInteractionCallback.RinkaPowerBomb] = EnemyAiRoutineDefinitions.BankA2.RinkaPowerBomb,
        [EnemyInteractionCallback.VerticalShutterTouch] = EnemyAiRoutineDefinitions.BankA2.VerticalShutterTouch,
        [EnemyInteractionCallback.ShootableVerticalShutterShot] = EnemyAiRoutineDefinitions.BankA2.ShootableVerticalShutterShot,
        [EnemyInteractionCallback.DestroyableVerticalShutterShot] = EnemyAiRoutineDefinitions.BankA2.DestroyableVerticalShutterShot,
        [EnemyInteractionCallback.VerticalShutterPowerBomb] = EnemyAiRoutineDefinitions.BankA2.VerticalShutterPowerBomb,
        [EnemyInteractionCallback.HorizontalShutterTouch] = EnemyAiRoutineDefinitions.BankA2.HorizontalShutterTouch,
        [EnemyInteractionCallback.HorizontalShutterShot] = EnemyAiRoutineDefinitions.BankA2.HorizontalShutterShot,
        [EnemyInteractionCallback.HorizontalShutterPowerBomb] = EnemyAiRoutineDefinitions.BankA2.HorizontalShutterPowerBomb,
        [EnemyInteractionCallback.SkreeShot] = EnemyAiRoutineDefinitions.BankA3.SkreeShot,
        [EnemyInteractionCallback.MetareeShot] = EnemyAiRoutineDefinitions.BankA3.MetareeShot,
        [EnemyInteractionCallback.FirefleaTouch] = EnemyAiRoutineDefinitions.BankA3.FirefleaTouch,
        [EnemyInteractionCallback.FirefleaPowerBomb] = EnemyAiRoutineDefinitions.BankA3.FirefleaPowerBomb,
        [EnemyInteractionCallback.FirefleaShot] = EnemyAiRoutineDefinitions.BankA3.FirefleaShot,
        [EnemyInteractionCallback.MochtroidTouch] = EnemyAiRoutineDefinitions.BankA3.MochtroidTouch,
        [EnemyInteractionCallback.MochtroidShot] = EnemyAiRoutineDefinitions.BankA3.MochtroidShot,
        [EnemyInteractionCallback.MetroidTouch] = EnemyAiRoutineDefinitions.BankA3.MetroidTouch,
        [EnemyInteractionCallback.MetroidShot] = EnemyAiRoutineDefinitions.BankA3.MetroidShot,
        [EnemyInteractionCallback.MetroidPowerBomb] = EnemyAiRoutineDefinitions.BankA3.MetroidPowerBomb,
        [EnemyInteractionCallback.YardTouch] = EnemyAiRoutineDefinitions.BankA3.YardTouch,
        [EnemyInteractionCallback.YardShot] = EnemyAiRoutineDefinitions.BankA3.YardShot,
        [EnemyInteractionCallback.PlatformNoOpTouch] = EnemyAiRoutineDefinitions.BankA3.PlatformNoOpTouch,
        [EnemyInteractionCallback.TripperShot] = EnemyAiRoutineDefinitions.BankA3.TripperShot,
        [EnemyInteractionCallback.CrocomireHeaderTouch] = EnemyAiRoutineDefinitions.BankA4.HeaderTouch,
        [EnemyInteractionCallback.CrocomireClawTouch] = EnemyAiRoutineDefinitions.BankA4.ClawTouch,
        [EnemyInteractionCallback.CrocomireNoOpHitboxShot] = EnemyAiRoutineDefinitions.BankA4.NoOpHitboxShot,
        [EnemyInteractionCallback.CrocomireDustHitboxShot] = EnemyAiRoutineDefinitions.BankA4.DustHitboxShot,
        [EnemyInteractionCallback.CrocomireMouthShot] = EnemyAiRoutineDefinitions.BankA4.MouthShot,
        [EnemyInteractionCallback.CrocomireAlternateDustHitboxShot] = EnemyAiRoutineDefinitions.BankA4.AlternateDustHitboxShot,
        [EnemyInteractionCallback.CrocomirePowerBomb] = EnemyAiRoutineDefinitions.BankA4.PowerBomb,
        [EnemyInteractionCallback.SporeSpawnTouch] = EnemyAiRoutineDefinitions.BankA5.SporeSpawnTouch,
        [EnemyInteractionCallback.SporeSpawnShot] = EnemyAiRoutineDefinitions.BankA5.SporeSpawnShot,
        [EnemyInteractionCallback.DraygonTouch] = EnemyAiRoutineDefinitions.BankA5.DraygonTouch,
        [EnemyInteractionCallback.DraygonShot] = EnemyAiRoutineDefinitions.BankA5.DraygonShot,
        [EnemyInteractionCallback.DraygonPowerBomb] = EnemyAiRoutineDefinitions.BankA5.DraygonPowerBomb,
        [EnemyInteractionCallback.FakeKraidTouch] = EnemyAiRoutineDefinitions.BankA6.FakeKraidTouch,
        [EnemyInteractionCallback.FakeKraidShot] = EnemyAiRoutineDefinitions.BankA6.FakeKraidShot,
        [EnemyInteractionCallback.CeresSteamTouch] = EnemyAiRoutineDefinitions.BankA6.CeresSteamTouch,
        [EnemyInteractionCallback.RidleyExtendedTouch] = EnemyAiRoutineDefinitions.BankA6.RidleyExtendedTouch,
        [EnemyInteractionCallback.RidleyShot] = EnemyAiRoutineDefinitions.BankA6.RidleyShot,
        [EnemyInteractionCallback.RidleyPowerBomb] = EnemyAiRoutineDefinitions.BankA6.RidleyPowerBomb,
        [EnemyInteractionCallback.ZebetiteTouch] = EnemyAiRoutineDefinitions.BankA6.ZebetiteTouch,
        [EnemyInteractionCallback.ZebetiteShot] = EnemyAiRoutineDefinitions.BankA6.ZebetiteShot,
        [EnemyInteractionCallback.KraidBackgroundTouch] = EnemyAiRoutineDefinitions.BankA7.KraidBackgroundTouch,
        [EnemyInteractionCallback.KraidArmTouch] = EnemyAiRoutineDefinitions.BankA7.KraidArmTouch,
        [EnemyInteractionCallback.KraidNoOpShot] = EnemyAiRoutineDefinitions.BankA7.KraidNoOpShot,
        [EnemyInteractionCallback.KraidArmShot] = EnemyAiRoutineDefinitions.BankA7.KraidArmShot,
        [EnemyInteractionCallback.KraidNailTouch] = EnemyAiRoutineDefinitions.BankA7.KraidNailTouch,
        [EnemyInteractionCallback.KraidBadNailTouch] = EnemyAiRoutineDefinitions.BankA7.KraidBadNailTouch,
        [EnemyInteractionCallback.EvirTouch] = EnemyAiRoutineDefinitions.BankA8.EvirTouch,
        [EnemyInteractionCallback.EvirPowerBomb] = EnemyAiRoutineDefinitions.BankA8.EvirPowerBomb,
        [EnemyInteractionCallback.EvirShot] = EnemyAiRoutineDefinitions.BankA8.EvirShot,
        [EnemyInteractionCallback.MagdollitePowerBomb] = EnemyAiRoutineDefinitions.BankA8.MagdollitePowerBomb,
        [EnemyInteractionCallback.MagdolliteTouch] = EnemyAiRoutineDefinitions.BankA8.MagdolliteTouch,
        [EnemyInteractionCallback.MagdolliteShot] = EnemyAiRoutineDefinitions.BankA8.MagdolliteShot,
        [EnemyInteractionCallback.BeetomTouch] = EnemyAiRoutineDefinitions.BankA8.BeetomTouch,
        [EnemyInteractionCallback.BeetomShot] = EnemyAiRoutineDefinitions.BankA8.BeetomShot,
        [EnemyInteractionCallback.PowampTouch] = EnemyAiRoutineDefinitions.BankA8.PowampTouch,
        [EnemyInteractionCallback.PowampShot] = EnemyAiRoutineDefinitions.BankA8.PowampShot,
        [EnemyInteractionCallback.PowampPowerBomb] = EnemyAiRoutineDefinitions.BankA8.PowampPowerBomb,
        [EnemyInteractionCallback.WorkRobotTouch] = EnemyAiRoutineDefinitions.BankA8.WorkRobotTouch,
        [EnemyInteractionCallback.WorkRobotNoPowerShot] = EnemyAiRoutineDefinitions.BankA8.WorkRobotNoPowerShot,
        [EnemyInteractionCallback.WorkRobotShot] = EnemyAiRoutineDefinitions.BankA8.WorkRobotShot,
        [EnemyInteractionCallback.BullShot] = EnemyAiRoutineDefinitions.BankA8.BullShot,
        [EnemyInteractionCallback.KagoShot] = EnemyAiRoutineDefinitions.BankA8.KagoShot,
        [EnemyInteractionCallback.KiHunterShot] = EnemyAiRoutineDefinitions.BankA8.KiHunterShot,
        [EnemyInteractionCallback.SparkShot] = EnemyAiRoutineDefinitions.BankA8.SparkShot,
        [EnemyInteractionCallback.BlueBrinstarFaceBlockShot] = EnemyAiRoutineDefinitions.BankA8.BlueBrinstarFaceBlockShot,
        [EnemyInteractionCallback.YappingMawTouch] = EnemyAiRoutineDefinitions.BankA8.YappingMawTouch,
        [EnemyInteractionCallback.YappingMawShot] = EnemyAiRoutineDefinitions.BankA8.YappingMawShot,
        [EnemyInteractionCallback.MotherBrainBodyShot] = EnemyAiRoutineDefinitions.BankA9.MotherBrainBodyShot,
        [EnemyInteractionCallback.MotherBrainHeadShot] = EnemyAiRoutineDefinitions.BankA9.MotherBrainHeadShot,
        [EnemyInteractionCallback.MotherBrainBodyTouch] = EnemyAiRoutineDefinitions.BankA9.MotherBrainBodyTouch,
        [EnemyInteractionCallback.MotherBrainHeadTouch] = EnemyAiRoutineDefinitions.BankA9.MotherBrainHeadTouch,
        [EnemyInteractionCallback.DeadTorizoTouchAndShot] = EnemyAiRoutineDefinitions.BankA9.DeadTorizoTouchAndShot,
        [EnemyInteractionCallback.DeadTorizoPowerBomb] = EnemyAiRoutineDefinitions.BankA9.DeadTorizoPowerBomb,
        [EnemyInteractionCallback.DeadSidehopperTouch] = EnemyAiRoutineDefinitions.BankA9.DeadSidehopperTouch,
        [EnemyInteractionCallback.DeadSidehopperShot] = EnemyAiRoutineDefinitions.BankA9.DeadSidehopperShot,
        [EnemyInteractionCallback.DeadSidehopperPowerBomb] = EnemyAiRoutineDefinitions.BankA9.DeadSidehopperPowerBomb,
        [EnemyInteractionCallback.ShitroidTouch] = EnemyAiRoutineDefinitions.BankA9.ShitroidTouch,
        [EnemyInteractionCallback.ShitroidShot] = EnemyAiRoutineDefinitions.BankA9.ShitroidShot,
        [EnemyInteractionCallback.ShitroidPowerBomb] = EnemyAiRoutineDefinitions.BankA9.ShitroidPowerBomb,
        [EnemyInteractionCallback.BombTorizoTouch] = EnemyAiRoutineDefinitions.BankAA.BombTorizoTouch,
        [EnemyInteractionCallback.BombTorizoShot] = EnemyAiRoutineDefinitions.BankAA.BombTorizoShot,
        [EnemyInteractionCallback.TorizoStandUpSitDownShot] = EnemyAiRoutineDefinitions.BankAA.TorizoStandUpSitDownShot,
        [EnemyInteractionCallback.GoldenTorizoShot] = EnemyAiRoutineDefinitions.BankAA.GoldenTorizoShot,
        [EnemyInteractionCallback.ShaktoolTouch] = EnemyAiRoutineDefinitions.BankAA.ShaktoolTouch,
        [EnemyInteractionCallback.ShaktoolShot] = EnemyAiRoutineDefinitions.BankAA.ShaktoolShot,
        [EnemyInteractionCallback.SpacePiratePowerBomb] = EnemyAiRoutineDefinitions.BankB2.PowerBomb,
        [EnemyInteractionCallback.SpacePirateTouch] = EnemyAiRoutineDefinitions.BankB2.Touch,
        [EnemyInteractionCallback.SpacePirateShot] = EnemyAiRoutineDefinitions.BankB2.Shot,
        [EnemyInteractionCallback.GoldNinjaVulnerableHitboxShot] = EnemyAiRoutineDefinitions.BankB2.GoldNinjaVulnerableHitboxShot,
        [EnemyInteractionCallback.GoldNinjaInvincibleHitboxShot] = EnemyAiRoutineDefinitions.BankB2.GoldNinjaInvincibleHitboxShot,
        [EnemyInteractionCallback.BotwoonTouch] = EnemyAiRoutineDefinitions.BankB3.BotwoonTouch,
        [EnemyInteractionCallback.BotwoonShot] = EnemyAiRoutineDefinitions.BankB3.BotwoonShot,
        [EnemyInteractionCallback.BotwoonPowerBomb] = EnemyAiRoutineDefinitions.BankB3.BotwoonPowerBomb,
        [EnemyInteractionCallback.PhantoonTouch] = EnemyAiRoutineDefinitions.BankA7.PhantoonTouch,
        [EnemyInteractionCallback.PhantoonShot] = EnemyAiRoutineDefinitions.BankA7.PhantoonShot,
        [EnemyInteractionCallback.DeadZoomerTouchAndShot] = EnemyAiRoutineDefinitions.BankA9.DeadZoomerTouchAndShot,
        [EnemyInteractionCallback.DeadZoomerPowerBomb] = EnemyAiRoutineDefinitions.BankA9.DeadZoomerPowerBomb,
        [EnemyInteractionCallback.DeadRipperTouchAndShot] = EnemyAiRoutineDefinitions.BankA9.DeadRipperTouchAndShot,
        [EnemyInteractionCallback.DeadRipperPowerBomb] = EnemyAiRoutineDefinitions.BankA9.DeadRipperPowerBomb,
        [EnemyInteractionCallback.DeadSkreeTouchAndShot] = EnemyAiRoutineDefinitions.BankA9.DeadSkreeTouchAndShot,
        [EnemyInteractionCallback.DeadSkreePowerBomb] = EnemyAiRoutineDefinitions.BankA9.DeadSkreePowerBomb,
        [EnemyInteractionCallback.CeresDoorNoOpShot] = EnemyShotCallbackDefinitions.CeresDoorNoOp,
        [EnemyInteractionCallback.ChozoNoOpShot] = EnemyShotCallbackDefinitions.ChozoNoOp,
    };

    /// <summary>The named callback at each native long address.</summary>
    private static readonly FrozenDictionary<SnesAddress, EnemyInteractionCallback> ByAddress =
        Addresses.ToFrozenDictionary(pair => pair.Value, pair => pair.Key);

    /// <summary>Names the callback a word selects in <paramref name="bank"/>'s code, or null when the port names none.</summary>
    internal static EnemyInteractionCallback? TryDecode(byte bank, ushort pointer)
    {
        if (ByAddress.TryGetValue(new SnesAddress(EnemyAiRoutineDefinitions.BankA0.Bank, pointer), out EnemyInteractionCallback common) &&
            common is EnemyInteractionCallback.NormalEnemyTouch or EnemyInteractionCallback.NormalEnemyShot or
                EnemyInteractionCallback.DudShot or EnemyInteractionCallback.NoOp or EnemyInteractionCallback.NoOpShortReturn)
            return common;
        return ByAddress.TryGetValue(new SnesAddress(bank, pointer), out EnemyInteractionCallback local) ? local : null;
    }

    /// <summary>Whether a header's initialization or main-AI word names the shared no-op or the species' routine.</summary>
    internal static EnemyAiRoutine AiRoutine(byte bank, ushort pointer) =>
        EnemyAiRoutineDefinitions.CommonNoOpAiRoutines.Contains(new SnesAddress(bank, pointer))
            ? EnemyAiRoutine.CommonNoOp
            : EnemyAiRoutine.SpeciesRoutine;

    /// <summary>
    /// Whether a header's hurt-AI word names the shared no-op or the species' routine. Every enemy bank repeats the
    /// shared long return at the same offset, so the word is read as its bank-$A0 copy.
    /// </summary>
    internal static EnemyAiRoutine HurtRoutine(ushort pointer) =>
        new SnesAddress(EnemyAiRoutineDefinitions.BankA0.Bank, pointer) == EnemyAiRoutineDefinitions.BankA0.NoOp
            ? EnemyAiRoutine.CommonNoOp
            : EnemyAiRoutine.SpeciesRoutine;

    /// <summary>What a callback word does: nothing to call, a routine, or an immediate return.</summary>
    internal static EnemyCallbackBody Body(ushort pointer, bool literalNoOp) =>
        pointer == 0 ? EnemyCallbackBody.None
        : literalNoOp ? EnemyCallbackBody.ImmediateReturn
        : EnemyCallbackBody.Routine;

    /// <summary>What <paramref name="bank"/>'s code runs for the shared no-op callback.</summary>
    internal static EnemyCallbackBody NoOpCallbackBody(byte bank) =>
        IsLiteralNoOpShot(bank, EnemyAiRoutineDefinitions.BankA0.NoOp.Offset)
            ? EnemyCallbackBody.ImmediateReturn
            : EnemyCallbackBody.Routine;

    /// <summary>Whether a header or hitbox touch word in <paramref name="bank"/>'s code is a literal long return.</summary>
    internal static bool IsLiteralNoOpTouch(byte bank, ushort pointer) =>
        EnemyTouchCallbackDefinitions.LiteralNoOps.Contains(new SnesAddress(bank, pointer));

    /// <summary>Whether a header or hitbox shot word in <paramref name="bank"/>'s code is a literal long return.</summary>
    internal static bool IsLiteralNoOpShot(byte bank, ushort pointer) =>
        EnemyShotCallbackDefinitions.LiteralNoOps.Contains(new SnesAddress(bank, pointer));

    /// <summary>Whether a header's Power Bomb word in <paramref name="bank"/>'s code is a literal long return.</summary>
    internal static bool IsLiteralNoOpPowerBomb(byte bank, ushort pointer) =>
        EnemyPowerBombCallbackDefinitions.LiteralNoOps.Contains(new SnesAddress(bank, pointer));
}
