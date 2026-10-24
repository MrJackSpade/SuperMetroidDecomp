using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hardware;

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
        [EnemyInteractionCallback.NormalEnemyTouch] = EnemyAiCodePointers.BankA0.NormalEnemyTouch,
        [EnemyInteractionCallback.NormalEnemyShot] = EnemyAiCodePointers.BankA0.NormalEnemyShot,
        [EnemyInteractionCallback.DudShot] = EnemyAiCodePointers.BankA0.DudShot,
        [EnemyInteractionCallback.NoOp] = EnemyAiCodePointers.BankA0.NoOp,
        [EnemyInteractionCallback.NoOpShortReturn] = EnemyAiCodePointers.BankA0.NoOpShortReturn,
        [EnemyInteractionCallback.OwtchShot] = EnemyAiCodePointers.BankA2.OwtchShot,
        [EnemyInteractionCallback.DragonTouch] = EnemyAiCodePointers.BankA2.DragonTouch,
        [EnemyInteractionCallback.DragonShot] = EnemyAiCodePointers.BankA2.DragonShot,
        [EnemyInteractionCallback.DragonPowerBomb] = EnemyAiCodePointers.BankA2.DragonPowerBomb,
        [EnemyInteractionCallback.GRipperRipper2Shot] = EnemyAiCodePointers.BankA2.GRipperRipper2Shot,
        [EnemyInteractionCallback.MamaTurtleTouch] = EnemyAiCodePointers.BankA2.MamaTurtleTouch,
        [EnemyInteractionCallback.BabyTurtleTouch] = EnemyAiCodePointers.BankA2.BabyTurtleTouch,
        [EnemyInteractionCallback.BabyTurtleShot] = EnemyAiCodePointers.BankA2.BabyTurtleShot,
        [EnemyInteractionCallback.MaridiaLargeSnailDamagingTouch] = EnemyAiCodePointers.BankA2.MaridiaLargeSnailDamagingTouch,
        [EnemyInteractionCallback.MaridiaLargeSnailNonDamagingTouch] = EnemyAiCodePointers.BankA2.MaridiaLargeSnailNonDamagingTouch,
        [EnemyInteractionCallback.MaridiaLargeSnailShot] = EnemyAiCodePointers.BankA2.MaridiaLargeSnailShot,
        [EnemyInteractionCallback.RinkaTouch] = EnemyAiCodePointers.BankA2.RinkaTouch,
        [EnemyInteractionCallback.RinkaShot] = EnemyAiCodePointers.BankA2.RinkaShot,
        [EnemyInteractionCallback.RinkaPowerBomb] = EnemyAiCodePointers.BankA2.RinkaPowerBomb,
        [EnemyInteractionCallback.VerticalShutterTouch] = EnemyAiCodePointers.BankA2.VerticalShutterTouch,
        [EnemyInteractionCallback.ShootableVerticalShutterShot] = EnemyAiCodePointers.BankA2.ShootableVerticalShutterShot,
        [EnemyInteractionCallback.DestroyableVerticalShutterShot] = EnemyAiCodePointers.BankA2.DestroyableVerticalShutterShot,
        [EnemyInteractionCallback.VerticalShutterPowerBomb] = EnemyAiCodePointers.BankA2.VerticalShutterPowerBomb,
        [EnemyInteractionCallback.HorizontalShutterTouch] = EnemyAiCodePointers.BankA2.HorizontalShutterTouch,
        [EnemyInteractionCallback.HorizontalShutterShot] = EnemyAiCodePointers.BankA2.HorizontalShutterShot,
        [EnemyInteractionCallback.HorizontalShutterPowerBomb] = EnemyAiCodePointers.BankA2.HorizontalShutterPowerBomb,
        [EnemyInteractionCallback.SkreeShot] = EnemyAiCodePointers.BankA3.SkreeShot,
        [EnemyInteractionCallback.MetareeShot] = EnemyAiCodePointers.BankA3.MetareeShot,
        [EnemyInteractionCallback.FirefleaTouch] = EnemyAiCodePointers.BankA3.FirefleaTouch,
        [EnemyInteractionCallback.FirefleaPowerBomb] = EnemyAiCodePointers.BankA3.FirefleaPowerBomb,
        [EnemyInteractionCallback.FirefleaShot] = EnemyAiCodePointers.BankA3.FirefleaShot,
        [EnemyInteractionCallback.MochtroidTouch] = EnemyAiCodePointers.BankA3.MochtroidTouch,
        [EnemyInteractionCallback.MochtroidShot] = EnemyAiCodePointers.BankA3.MochtroidShot,
        [EnemyInteractionCallback.MetroidTouch] = EnemyAiCodePointers.BankA3.MetroidTouch,
        [EnemyInteractionCallback.MetroidShot] = EnemyAiCodePointers.BankA3.MetroidShot,
        [EnemyInteractionCallback.MetroidPowerBomb] = EnemyAiCodePointers.BankA3.MetroidPowerBomb,
        [EnemyInteractionCallback.YardTouch] = EnemyAiCodePointers.BankA3.YardTouch,
        [EnemyInteractionCallback.YardShot] = EnemyAiCodePointers.BankA3.YardShot,
        [EnemyInteractionCallback.PlatformNoOpTouch] = EnemyAiCodePointers.BankA3.PlatformNoOpTouch,
        [EnemyInteractionCallback.TripperShot] = EnemyAiCodePointers.BankA3.TripperShot,
        [EnemyInteractionCallback.CrocomireHeaderTouch] = EnemyAiCodePointers.BankA4.HeaderTouch,
        [EnemyInteractionCallback.CrocomireClawTouch] = EnemyAiCodePointers.BankA4.ClawTouch,
        [EnemyInteractionCallback.CrocomireNoOpHitboxShot] = EnemyAiCodePointers.BankA4.NoOpHitboxShot,
        [EnemyInteractionCallback.CrocomireDustHitboxShot] = EnemyAiCodePointers.BankA4.DustHitboxShot,
        [EnemyInteractionCallback.CrocomireMouthShot] = EnemyAiCodePointers.BankA4.MouthShot,
        [EnemyInteractionCallback.CrocomireAlternateDustHitboxShot] = EnemyAiCodePointers.BankA4.AlternateDustHitboxShot,
        [EnemyInteractionCallback.CrocomirePowerBomb] = EnemyAiCodePointers.BankA4.PowerBomb,
        [EnemyInteractionCallback.SporeSpawnTouch] = EnemyAiCodePointers.BankA5.SporeSpawnTouch,
        [EnemyInteractionCallback.SporeSpawnShot] = EnemyAiCodePointers.BankA5.SporeSpawnShot,
        [EnemyInteractionCallback.DraygonTouch] = EnemyAiCodePointers.BankA5.DraygonTouch,
        [EnemyInteractionCallback.DraygonShot] = EnemyAiCodePointers.BankA5.DraygonShot,
        [EnemyInteractionCallback.DraygonPowerBomb] = EnemyAiCodePointers.BankA5.DraygonPowerBomb,
        [EnemyInteractionCallback.FakeKraidTouch] = EnemyAiCodePointers.BankA6.FakeKraidTouch,
        [EnemyInteractionCallback.FakeKraidShot] = EnemyAiCodePointers.BankA6.FakeKraidShot,
        [EnemyInteractionCallback.CeresSteamTouch] = EnemyAiCodePointers.BankA6.CeresSteamTouch,
        [EnemyInteractionCallback.RidleyExtendedTouch] = EnemyAiCodePointers.BankA6.RidleyExtendedTouch,
        [EnemyInteractionCallback.RidleyShot] = EnemyAiCodePointers.BankA6.RidleyShot,
        [EnemyInteractionCallback.RidleyPowerBomb] = EnemyAiCodePointers.BankA6.RidleyPowerBomb,
        [EnemyInteractionCallback.ZebetiteTouch] = EnemyAiCodePointers.BankA6.ZebetiteTouch,
        [EnemyInteractionCallback.ZebetiteShot] = EnemyAiCodePointers.BankA6.ZebetiteShot,
        [EnemyInteractionCallback.KraidBackgroundTouch] = EnemyAiCodePointers.BankA7.KraidBackgroundTouch,
        [EnemyInteractionCallback.KraidArmTouch] = EnemyAiCodePointers.BankA7.KraidArmTouch,
        [EnemyInteractionCallback.KraidNoOpShot] = EnemyAiCodePointers.BankA7.KraidNoOpShot,
        [EnemyInteractionCallback.KraidArmShot] = EnemyAiCodePointers.BankA7.KraidArmShot,
        [EnemyInteractionCallback.KraidNailTouch] = EnemyAiCodePointers.BankA7.KraidNailTouch,
        [EnemyInteractionCallback.KraidBadNailTouch] = EnemyAiCodePointers.BankA7.KraidBadNailTouch,
        [EnemyInteractionCallback.EvirTouch] = EnemyAiCodePointers.BankA8.EvirTouch,
        [EnemyInteractionCallback.EvirPowerBomb] = EnemyAiCodePointers.BankA8.EvirPowerBomb,
        [EnemyInteractionCallback.EvirShot] = EnemyAiCodePointers.BankA8.EvirShot,
        [EnemyInteractionCallback.MagdollitePowerBomb] = EnemyAiCodePointers.BankA8.MagdollitePowerBomb,
        [EnemyInteractionCallback.MagdolliteTouch] = EnemyAiCodePointers.BankA8.MagdolliteTouch,
        [EnemyInteractionCallback.MagdolliteShot] = EnemyAiCodePointers.BankA8.MagdolliteShot,
        [EnemyInteractionCallback.BeetomTouch] = EnemyAiCodePointers.BankA8.BeetomTouch,
        [EnemyInteractionCallback.BeetomShot] = EnemyAiCodePointers.BankA8.BeetomShot,
        [EnemyInteractionCallback.PowampTouch] = EnemyAiCodePointers.BankA8.PowampTouch,
        [EnemyInteractionCallback.PowampShot] = EnemyAiCodePointers.BankA8.PowampShot,
        [EnemyInteractionCallback.PowampPowerBomb] = EnemyAiCodePointers.BankA8.PowampPowerBomb,
        [EnemyInteractionCallback.WorkRobotTouch] = EnemyAiCodePointers.BankA8.WorkRobotTouch,
        [EnemyInteractionCallback.WorkRobotNoPowerShot] = EnemyAiCodePointers.BankA8.WorkRobotNoPowerShot,
        [EnemyInteractionCallback.WorkRobotShot] = EnemyAiCodePointers.BankA8.WorkRobotShot,
        [EnemyInteractionCallback.BullShot] = EnemyAiCodePointers.BankA8.BullShot,
        [EnemyInteractionCallback.KagoShot] = EnemyAiCodePointers.BankA8.KagoShot,
        [EnemyInteractionCallback.KiHunterShot] = EnemyAiCodePointers.BankA8.KiHunterShot,
        [EnemyInteractionCallback.SparkShot] = EnemyAiCodePointers.BankA8.SparkShot,
        [EnemyInteractionCallback.BlueBrinstarFaceBlockShot] = EnemyAiCodePointers.BankA8.BlueBrinstarFaceBlockShot,
        [EnemyInteractionCallback.YappingMawTouch] = EnemyAiCodePointers.BankA8.YappingMawTouch,
        [EnemyInteractionCallback.YappingMawShot] = EnemyAiCodePointers.BankA8.YappingMawShot,
        [EnemyInteractionCallback.MotherBrainBodyShot] = EnemyAiCodePointers.BankA9.MotherBrainBodyShot,
        [EnemyInteractionCallback.MotherBrainHeadShot] = EnemyAiCodePointers.BankA9.MotherBrainHeadShot,
        [EnemyInteractionCallback.MotherBrainBodyTouch] = EnemyAiCodePointers.BankA9.MotherBrainBodyTouch,
        [EnemyInteractionCallback.MotherBrainHeadTouch] = EnemyAiCodePointers.BankA9.MotherBrainHeadTouch,
        [EnemyInteractionCallback.DeadTorizoTouchAndShot] = EnemyAiCodePointers.BankA9.DeadTorizoTouchAndShot,
        [EnemyInteractionCallback.DeadTorizoPowerBomb] = EnemyAiCodePointers.BankA9.DeadTorizoPowerBomb,
        [EnemyInteractionCallback.DeadSidehopperTouch] = EnemyAiCodePointers.BankA9.DeadSidehopperTouch,
        [EnemyInteractionCallback.DeadSidehopperShot] = EnemyAiCodePointers.BankA9.DeadSidehopperShot,
        [EnemyInteractionCallback.DeadSidehopperPowerBomb] = EnemyAiCodePointers.BankA9.DeadSidehopperPowerBomb,
        [EnemyInteractionCallback.ShitroidTouch] = EnemyAiCodePointers.BankA9.ShitroidTouch,
        [EnemyInteractionCallback.ShitroidShot] = EnemyAiCodePointers.BankA9.ShitroidShot,
        [EnemyInteractionCallback.ShitroidPowerBomb] = EnemyAiCodePointers.BankA9.ShitroidPowerBomb,
        [EnemyInteractionCallback.BombTorizoTouch] = EnemyAiCodePointers.BankAA.BombTorizoTouch,
        [EnemyInteractionCallback.BombTorizoShot] = EnemyAiCodePointers.BankAA.BombTorizoShot,
        [EnemyInteractionCallback.TorizoStandUpSitDownShot] = EnemyAiCodePointers.BankAA.TorizoStandUpSitDownShot,
        [EnemyInteractionCallback.GoldenTorizoShot] = EnemyAiCodePointers.BankAA.GoldenTorizoShot,
        [EnemyInteractionCallback.ShaktoolTouch] = EnemyAiCodePointers.BankAA.ShaktoolTouch,
        [EnemyInteractionCallback.ShaktoolShot] = EnemyAiCodePointers.BankAA.ShaktoolShot,
        [EnemyInteractionCallback.SpacePiratePowerBomb] = EnemyAiCodePointers.BankB2.PowerBomb,
        [EnemyInteractionCallback.SpacePirateTouch] = EnemyAiCodePointers.BankB2.Touch,
        [EnemyInteractionCallback.SpacePirateShot] = EnemyAiCodePointers.BankB2.Shot,
        [EnemyInteractionCallback.GoldNinjaVulnerableHitboxShot] = EnemyAiCodePointers.BankB2.GoldNinjaVulnerableHitboxShot,
        [EnemyInteractionCallback.GoldNinjaInvincibleHitboxShot] = EnemyAiCodePointers.BankB2.GoldNinjaInvincibleHitboxShot,
        [EnemyInteractionCallback.BotwoonTouch] = EnemyAiCodePointers.BankB3.BotwoonTouch,
        [EnemyInteractionCallback.BotwoonShot] = EnemyAiCodePointers.BankB3.BotwoonShot,
        [EnemyInteractionCallback.BotwoonPowerBomb] = EnemyAiCodePointers.BankB3.BotwoonPowerBomb,
        [EnemyInteractionCallback.PhantoonTouch] = EnemyAiCodePointers.BankA7.PhantoonTouch,
        [EnemyInteractionCallback.PhantoonShot] = EnemyAiCodePointers.BankA7.PhantoonShot,
        [EnemyInteractionCallback.DeadZoomerTouchAndShot] = EnemyAiCodePointers.BankA9.DeadZoomerTouchAndShot,
        [EnemyInteractionCallback.DeadZoomerPowerBomb] = EnemyAiCodePointers.BankA9.DeadZoomerPowerBomb,
        [EnemyInteractionCallback.DeadRipperTouchAndShot] = EnemyAiCodePointers.BankA9.DeadRipperTouchAndShot,
        [EnemyInteractionCallback.DeadRipperPowerBomb] = EnemyAiCodePointers.BankA9.DeadRipperPowerBomb,
        [EnemyInteractionCallback.DeadSkreeTouchAndShot] = EnemyAiCodePointers.BankA9.DeadSkreeTouchAndShot,
        [EnemyInteractionCallback.DeadSkreePowerBomb] = EnemyAiCodePointers.BankA9.DeadSkreePowerBomb,
        [EnemyInteractionCallback.CeresDoorNoOpShot] = EnemyShotCallbackDefinitions.CeresDoorNoOp,
        [EnemyInteractionCallback.ChozoNoOpShot] = EnemyShotCallbackDefinitions.ChozoNoOp,
    };

    /// <summary>The named callback at each native long address.</summary>
    private static readonly IReadOnlyDictionary<SnesAddress, EnemyInteractionCallback> ByAddress =
        Addresses.ToDictionary(pair => pair.Value, pair => pair.Key);

    /// <summary>Names the callback a word selects in <paramref name="bank"/>'s code, or null when the port names none.</summary>
    internal static EnemyInteractionCallback? TryDecode(byte bank, ushort pointer)
    {
        if (ByAddress.TryGetValue(new SnesAddress(EnemyAiCodePointers.BankA0.Bank, pointer), out EnemyInteractionCallback common) &&
            common is EnemyInteractionCallback.NormalEnemyTouch or EnemyInteractionCallback.NormalEnemyShot or
                EnemyInteractionCallback.DudShot or EnemyInteractionCallback.NoOp or EnemyInteractionCallback.NoOpShortReturn)
            return common;
        return ByAddress.TryGetValue(new SnesAddress(bank, pointer), out EnemyInteractionCallback local) ? local : null;
    }

    /// <summary>Whether a header's initialization or main-AI word names the shared no-op or the species' routine.</summary>
    internal static EnemyAiRoutine AiRoutine(byte bank, ushort pointer) =>
        EnemyAiCodePointers.CommonNoOpAiRoutines.Contains(new SnesAddress(bank, pointer))
            ? EnemyAiRoutine.CommonNoOp
            : EnemyAiRoutine.SpeciesRoutine;

    /// <summary>
    /// Whether a header's hurt-AI word names the shared no-op or the species' routine. Every enemy bank repeats the
    /// shared long return at the same offset, so the word is read as its bank-$A0 copy.
    /// </summary>
    internal static EnemyAiRoutine HurtRoutine(ushort pointer) =>
        new SnesAddress(EnemyAiCodePointers.BankA0.Bank, pointer) == EnemyAiCodePointers.BankA0.NoOp
            ? EnemyAiRoutine.CommonNoOp
            : EnemyAiRoutine.SpeciesRoutine;

    /// <summary>What a callback word does: nothing to call, a routine, or an immediate return.</summary>
    internal static EnemyCallbackBody Body(ushort pointer, bool literalNoOp) =>
        pointer == 0 ? EnemyCallbackBody.None
        : literalNoOp ? EnemyCallbackBody.ImmediateReturn
        : EnemyCallbackBody.Routine;

    /// <summary>What <paramref name="bank"/>'s code runs for the shared no-op callback.</summary>
    internal static EnemyCallbackBody NoOpCallbackBody(byte bank) =>
        IsLiteralNoOpShot(bank, EnemyAiCodePointers.BankA0.NoOp.Offset)
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
