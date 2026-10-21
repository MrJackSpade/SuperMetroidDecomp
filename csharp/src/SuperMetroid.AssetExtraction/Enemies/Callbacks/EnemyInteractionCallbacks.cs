using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Hardware;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Decodes enemy header and hitbox callback words to the interaction callbacks the port names. The shared prologue
/// entries every enemy bank repeats at the same offset are named by their bank-$A0 copy and admitted from any enemy bank.
/// </summary>
internal static class EnemyInteractionCallbacks
{
    /// <summary>The native long address of each named callback.</summary>
    internal static readonly IReadOnlyDictionary<EnemyInteractionCallback, int> Addresses =
        new Dictionary<EnemyInteractionCallback, int>
    {
        [EnemyInteractionCallback.NormalEnemyTouch] = 0xa00000 | EnemyAiCodePointers.BankA0.NormalEnemyTouch,
        [EnemyInteractionCallback.NormalEnemyShot] = 0xa00000 | EnemyAiCodePointers.BankA0.NormalEnemyShot,
        [EnemyInteractionCallback.DudShot] = 0xa00000 | EnemyAiCodePointers.BankA0.DudShot,
        [EnemyInteractionCallback.NoOp] = 0xa00000 | EnemyAiCodePointers.BankA0.NoOp,
        [EnemyInteractionCallback.NoOpShortReturn] = 0xa00000 | EnemyAiCodePointers.BankA0.NoOpShortReturn,
        [EnemyInteractionCallback.OwtchShot] = 0xa20000 | EnemyAiCodePointers.BankA2.OwtchShot,
        [EnemyInteractionCallback.DragonTouch] = 0xa20000 | EnemyAiCodePointers.BankA2.DragonTouch,
        [EnemyInteractionCallback.DragonShot] = 0xa20000 | EnemyAiCodePointers.BankA2.DragonShot,
        [EnemyInteractionCallback.DragonPowerBomb] = 0xa20000 | EnemyAiCodePointers.BankA2.DragonPowerBomb,
        [EnemyInteractionCallback.GRipperRipper2Shot] = 0xa20000 | EnemyAiCodePointers.BankA2.GRipperRipper2Shot,
        [EnemyInteractionCallback.MamaTurtleTouch] = 0xa20000 | EnemyAiCodePointers.BankA2.MamaTurtleTouch,
        [EnemyInteractionCallback.BabyTurtleTouch] = 0xa20000 | EnemyAiCodePointers.BankA2.BabyTurtleTouch,
        [EnemyInteractionCallback.BabyTurtleShot] = 0xa20000 | EnemyAiCodePointers.BankA2.BabyTurtleShot,
        [EnemyInteractionCallback.MaridiaLargeSnailDamagingTouch] = 0xa20000 | EnemyAiCodePointers.BankA2.MaridiaLargeSnailDamagingTouch,
        [EnemyInteractionCallback.MaridiaLargeSnailNonDamagingTouch] = 0xa20000 | EnemyAiCodePointers.BankA2.MaridiaLargeSnailNonDamagingTouch,
        [EnemyInteractionCallback.MaridiaLargeSnailShot] = 0xa20000 | EnemyAiCodePointers.BankA2.MaridiaLargeSnailShot,
        [EnemyInteractionCallback.RinkaTouch] = 0xa20000 | EnemyAiCodePointers.BankA2.RinkaTouch,
        [EnemyInteractionCallback.RinkaShot] = 0xa20000 | EnemyAiCodePointers.BankA2.RinkaShot,
        [EnemyInteractionCallback.RinkaPowerBomb] = 0xa20000 | EnemyAiCodePointers.BankA2.RinkaPowerBomb,
        [EnemyInteractionCallback.VerticalShutterTouch] = 0xa20000 | EnemyAiCodePointers.BankA2.VerticalShutterTouch,
        [EnemyInteractionCallback.ShootableVerticalShutterShot] = 0xa20000 | EnemyAiCodePointers.BankA2.ShootableVerticalShutterShot,
        [EnemyInteractionCallback.DestroyableVerticalShutterShot] = 0xa20000 | EnemyAiCodePointers.BankA2.DestroyableVerticalShutterShot,
        [EnemyInteractionCallback.VerticalShutterPowerBomb] = 0xa20000 | EnemyAiCodePointers.BankA2.VerticalShutterPowerBomb,
        [EnemyInteractionCallback.HorizontalShutterTouch] = 0xa20000 | EnemyAiCodePointers.BankA2.HorizontalShutterTouch,
        [EnemyInteractionCallback.HorizontalShutterShot] = 0xa20000 | EnemyAiCodePointers.BankA2.HorizontalShutterShot,
        [EnemyInteractionCallback.HorizontalShutterPowerBomb] = 0xa20000 | EnemyAiCodePointers.BankA2.HorizontalShutterPowerBomb,
        [EnemyInteractionCallback.SkreeShot] = 0xa30000 | EnemyAiCodePointers.BankA3.SkreeShot,
        [EnemyInteractionCallback.MetareeShot] = 0xa30000 | EnemyAiCodePointers.BankA3.MetareeShot,
        [EnemyInteractionCallback.FirefleaTouch] = 0xa30000 | EnemyAiCodePointers.BankA3.FirefleaTouch,
        [EnemyInteractionCallback.FirefleaPowerBomb] = 0xa30000 | EnemyAiCodePointers.BankA3.FirefleaPowerBomb,
        [EnemyInteractionCallback.FirefleaShot] = 0xa30000 | EnemyAiCodePointers.BankA3.FirefleaShot,
        [EnemyInteractionCallback.MochtroidTouch] = 0xa30000 | EnemyAiCodePointers.BankA3.MochtroidTouch,
        [EnemyInteractionCallback.MochtroidShot] = 0xa30000 | EnemyAiCodePointers.BankA3.MochtroidShot,
        [EnemyInteractionCallback.MetroidTouch] = 0xa30000 | EnemyAiCodePointers.BankA3.MetroidTouch,
        [EnemyInteractionCallback.MetroidShot] = 0xa30000 | EnemyAiCodePointers.BankA3.MetroidShot,
        [EnemyInteractionCallback.MetroidPowerBomb] = 0xa30000 | EnemyAiCodePointers.BankA3.MetroidPowerBomb,
        [EnemyInteractionCallback.YardTouch] = 0xa30000 | EnemyAiCodePointers.BankA3.YardTouch,
        [EnemyInteractionCallback.YardShot] = 0xa30000 | EnemyAiCodePointers.BankA3.YardShot,
        [EnemyInteractionCallback.PlatformNoOpTouch] = 0xa30000 | EnemyAiCodePointers.BankA3.PlatformNoOpTouch,
        [EnemyInteractionCallback.TripperShot] = 0xa30000 | EnemyAiCodePointers.BankA3.TripperShot,
        [EnemyInteractionCallback.CrocomireHeaderTouch] = 0xa40000 | EnemyAiCodePointers.BankA4.HeaderTouch,
        [EnemyInteractionCallback.CrocomireClawTouch] = 0xa40000 | EnemyAiCodePointers.BankA4.ClawTouch,
        [EnemyInteractionCallback.CrocomireNoOpHitboxShot] = 0xa40000 | EnemyAiCodePointers.BankA4.NoOpHitboxShot,
        [EnemyInteractionCallback.CrocomireDustHitboxShot] = 0xa40000 | EnemyAiCodePointers.BankA4.DustHitboxShot,
        [EnemyInteractionCallback.CrocomireMouthShot] = 0xa40000 | EnemyAiCodePointers.BankA4.MouthShot,
        [EnemyInteractionCallback.CrocomireAlternateDustHitboxShot] = 0xa40000 | EnemyAiCodePointers.BankA4.AlternateDustHitboxShot,
        [EnemyInteractionCallback.CrocomirePowerBomb] = 0xa40000 | EnemyAiCodePointers.BankA4.PowerBomb,
        [EnemyInteractionCallback.SporeSpawnTouch] = 0xa50000 | EnemyAiCodePointers.BankA5.SporeSpawnTouch,
        [EnemyInteractionCallback.SporeSpawnShot] = 0xa50000 | EnemyAiCodePointers.BankA5.SporeSpawnShot,
        [EnemyInteractionCallback.DraygonTouch] = 0xa50000 | EnemyAiCodePointers.BankA5.DraygonTouch,
        [EnemyInteractionCallback.DraygonShot] = 0xa50000 | EnemyAiCodePointers.BankA5.DraygonShot,
        [EnemyInteractionCallback.DraygonPowerBomb] = 0xa50000 | EnemyAiCodePointers.BankA5.DraygonPowerBomb,
        [EnemyInteractionCallback.FakeKraidTouch] = 0xa60000 | EnemyAiCodePointers.BankA6.FakeKraidTouch,
        [EnemyInteractionCallback.FakeKraidShot] = 0xa60000 | EnemyAiCodePointers.BankA6.FakeKraidShot,
        [EnemyInteractionCallback.CeresSteamTouch] = 0xa60000 | EnemyAiCodePointers.BankA6.CeresSteamTouch,
        [EnemyInteractionCallback.RidleyExtendedTouch] = 0xa60000 | EnemyAiCodePointers.BankA6.RidleyExtendedTouch,
        [EnemyInteractionCallback.RidleyShot] = 0xa60000 | EnemyAiCodePointers.BankA6.RidleyShot,
        [EnemyInteractionCallback.RidleyPowerBomb] = 0xa60000 | EnemyAiCodePointers.BankA6.RidleyPowerBomb,
        [EnemyInteractionCallback.ZebetiteTouch] = 0xa60000 | EnemyAiCodePointers.BankA6.ZebetiteTouch,
        [EnemyInteractionCallback.ZebetiteShot] = 0xa60000 | EnemyAiCodePointers.BankA6.ZebetiteShot,
        [EnemyInteractionCallback.KraidBackgroundTouch] = 0xa70000 | EnemyAiCodePointers.BankA7.KraidBackgroundTouch,
        [EnemyInteractionCallback.KraidArmTouch] = 0xa70000 | EnemyAiCodePointers.BankA7.KraidArmTouch,
        [EnemyInteractionCallback.KraidNoOpShot] = 0xa70000 | EnemyAiCodePointers.BankA7.KraidNoOpShot,
        [EnemyInteractionCallback.KraidArmShot] = 0xa70000 | EnemyAiCodePointers.BankA7.KraidArmShot,
        [EnemyInteractionCallback.KraidNailTouch] = 0xa70000 | EnemyAiCodePointers.BankA7.KraidNailTouch,
        [EnemyInteractionCallback.KraidBadNailTouch] = 0xa70000 | EnemyAiCodePointers.BankA7.KraidBadNailTouch,
        [EnemyInteractionCallback.EvirTouch] = 0xa80000 | EnemyAiCodePointers.BankA8.EvirTouch,
        [EnemyInteractionCallback.EvirPowerBomb] = 0xa80000 | EnemyAiCodePointers.BankA8.EvirPowerBomb,
        [EnemyInteractionCallback.EvirShot] = 0xa80000 | EnemyAiCodePointers.BankA8.EvirShot,
        [EnemyInteractionCallback.MagdollitePowerBomb] = 0xa80000 | EnemyAiCodePointers.BankA8.MagdollitePowerBomb,
        [EnemyInteractionCallback.MagdolliteTouch] = 0xa80000 | EnemyAiCodePointers.BankA8.MagdolliteTouch,
        [EnemyInteractionCallback.MagdolliteShot] = 0xa80000 | EnemyAiCodePointers.BankA8.MagdolliteShot,
        [EnemyInteractionCallback.BeetomTouch] = 0xa80000 | EnemyAiCodePointers.BankA8.BeetomTouch,
        [EnemyInteractionCallback.BeetomShot] = 0xa80000 | EnemyAiCodePointers.BankA8.BeetomShot,
        [EnemyInteractionCallback.PowampTouch] = 0xa80000 | EnemyAiCodePointers.BankA8.PowampTouch,
        [EnemyInteractionCallback.PowampShot] = 0xa80000 | EnemyAiCodePointers.BankA8.PowampShot,
        [EnemyInteractionCallback.PowampPowerBomb] = 0xa80000 | EnemyAiCodePointers.BankA8.PowampPowerBomb,
        [EnemyInteractionCallback.WorkRobotTouch] = 0xa80000 | EnemyAiCodePointers.BankA8.WorkRobotTouch,
        [EnemyInteractionCallback.WorkRobotNoPowerShot] = 0xa80000 | EnemyAiCodePointers.BankA8.WorkRobotNoPowerShot,
        [EnemyInteractionCallback.WorkRobotShot] = 0xa80000 | EnemyAiCodePointers.BankA8.WorkRobotShot,
        [EnemyInteractionCallback.BullShot] = 0xa80000 | EnemyAiCodePointers.BankA8.BullShot,
        [EnemyInteractionCallback.KagoShot] = 0xa80000 | EnemyAiCodePointers.BankA8.KagoShot,
        [EnemyInteractionCallback.KiHunterShot] = 0xa80000 | EnemyAiCodePointers.BankA8.KiHunterShot,
        [EnemyInteractionCallback.SparkShot] = 0xa80000 | EnemyAiCodePointers.BankA8.SparkShot,
        [EnemyInteractionCallback.BlueBrinstarFaceBlockShot] = 0xa80000 | EnemyAiCodePointers.BankA8.BlueBrinstarFaceBlockShot,
        [EnemyInteractionCallback.YappingMawTouch] = 0xa80000 | EnemyAiCodePointers.BankA8.YappingMawTouch,
        [EnemyInteractionCallback.YappingMawShot] = 0xa80000 | EnemyAiCodePointers.BankA8.YappingMawShot,
        [EnemyInteractionCallback.MotherBrainBodyShot] = 0xa90000 | EnemyAiCodePointers.BankA9.MotherBrainBodyShot,
        [EnemyInteractionCallback.MotherBrainHeadShot] = 0xa90000 | EnemyAiCodePointers.BankA9.MotherBrainHeadShot,
        [EnemyInteractionCallback.MotherBrainBodyTouch] = 0xa90000 | EnemyAiCodePointers.BankA9.MotherBrainBodyTouch,
        [EnemyInteractionCallback.MotherBrainHeadTouch] = 0xa90000 | EnemyAiCodePointers.BankA9.MotherBrainHeadTouch,
        [EnemyInteractionCallback.DeadTorizoTouchAndShot] = 0xa90000 | EnemyAiCodePointers.BankA9.DeadTorizoTouchAndShot,
        [EnemyInteractionCallback.DeadTorizoPowerBomb] = 0xa90000 | EnemyAiCodePointers.BankA9.DeadTorizoPowerBomb,
        [EnemyInteractionCallback.DeadSidehopperTouch] = 0xa90000 | EnemyAiCodePointers.BankA9.DeadSidehopperTouch,
        [EnemyInteractionCallback.DeadSidehopperShot] = 0xa90000 | EnemyAiCodePointers.BankA9.DeadSidehopperShot,
        [EnemyInteractionCallback.DeadSidehopperPowerBomb] = 0xa90000 | EnemyAiCodePointers.BankA9.DeadSidehopperPowerBomb,
        [EnemyInteractionCallback.ShitroidTouch] = 0xa90000 | EnemyAiCodePointers.BankA9.ShitroidTouch,
        [EnemyInteractionCallback.ShitroidShot] = 0xa90000 | EnemyAiCodePointers.BankA9.ShitroidShot,
        [EnemyInteractionCallback.ShitroidPowerBomb] = 0xa90000 | EnemyAiCodePointers.BankA9.ShitroidPowerBomb,
        [EnemyInteractionCallback.BombTorizoTouch] = 0xaa0000 | EnemyAiCodePointers.BankAA.BombTorizoTouch,
        [EnemyInteractionCallback.BombTorizoShot] = 0xaa0000 | EnemyAiCodePointers.BankAA.BombTorizoShot,
        [EnemyInteractionCallback.TorizoStandUpSitDownShot] = 0xaa0000 | EnemyAiCodePointers.BankAA.TorizoStandUpSitDownShot,
        [EnemyInteractionCallback.GoldenTorizoShot] = 0xaa0000 | EnemyAiCodePointers.BankAA.GoldenTorizoShot,
        [EnemyInteractionCallback.ShaktoolTouch] = 0xaa0000 | EnemyAiCodePointers.BankAA.ShaktoolTouch,
        [EnemyInteractionCallback.ShaktoolShot] = 0xaa0000 | EnemyAiCodePointers.BankAA.ShaktoolShot,
        [EnemyInteractionCallback.SpacePiratePowerBomb] = 0xb20000 | EnemyAiCodePointers.BankB2.PowerBomb,
        [EnemyInteractionCallback.SpacePirateTouch] = 0xb20000 | EnemyAiCodePointers.BankB2.Touch,
        [EnemyInteractionCallback.SpacePirateShot] = 0xb20000 | EnemyAiCodePointers.BankB2.Shot,
        [EnemyInteractionCallback.GoldNinjaVulnerableHitboxShot] = 0xb20000 | EnemyAiCodePointers.BankB2.GoldNinjaVulnerableHitboxShot,
        [EnemyInteractionCallback.GoldNinjaInvincibleHitboxShot] = 0xb20000 | EnemyAiCodePointers.BankB2.GoldNinjaInvincibleHitboxShot,
        [EnemyInteractionCallback.BotwoonTouch] = 0xb30000 | EnemyAiCodePointers.BankB3.BotwoonTouch,
        [EnemyInteractionCallback.BotwoonShot] = 0xb30000 | EnemyAiCodePointers.BankB3.BotwoonShot,
        [EnemyInteractionCallback.BotwoonPowerBomb] = 0xb30000 | EnemyAiCodePointers.BankB3.BotwoonPowerBomb,
        [EnemyInteractionCallback.PhantoonTouch] = 0xa70000 | EnemyAiCodePointers.BankA7.PhantoonTouch,
        [EnemyInteractionCallback.PhantoonShot] = 0xa70000 | EnemyAiCodePointers.BankA7.PhantoonShot,
        [EnemyInteractionCallback.DeadZoomerTouchAndShot] = 0xa90000 | EnemyAiCodePointers.BankA9.DeadZoomerTouchAndShot,
        [EnemyInteractionCallback.DeadZoomerPowerBomb] = 0xa90000 | EnemyAiCodePointers.BankA9.DeadZoomerPowerBomb,
        [EnemyInteractionCallback.DeadRipperTouchAndShot] = 0xa90000 | EnemyAiCodePointers.BankA9.DeadRipperTouchAndShot,
        [EnemyInteractionCallback.DeadRipperPowerBomb] = 0xa90000 | EnemyAiCodePointers.BankA9.DeadRipperPowerBomb,
        [EnemyInteractionCallback.DeadSkreeTouchAndShot] = 0xa90000 | EnemyAiCodePointers.BankA9.DeadSkreeTouchAndShot,
        [EnemyInteractionCallback.DeadSkreePowerBomb] = 0xa90000 | EnemyAiCodePointers.BankA9.DeadSkreePowerBomb,
        [EnemyInteractionCallback.CeresDoorNoOpShot] = EnemyShotCallbackDefinitions.CeresDoorNoOp,
        [EnemyInteractionCallback.ChozoNoOpShot] = EnemyShotCallbackDefinitions.ChozoNoOp,
    };

    /// <summary>The named callback at each native long address.</summary>
    private static readonly IReadOnlyDictionary<int, EnemyInteractionCallback> ByAddress =
        Addresses.ToDictionary(pair => pair.Value, pair => pair.Key);

    /// <summary>Names the callback a word selects in <paramref name="bank"/>'s code, or null when the port names none.</summary>
    internal static EnemyInteractionCallback? TryDecode(byte bank, ushort pointer)
    {
        if (ByAddress.TryGetValue((int)new SnesAddress(EnemyAiCodePointers.BankA0.Bank, pointer), out EnemyInteractionCallback common) &&
            common is EnemyInteractionCallback.NormalEnemyTouch or EnemyInteractionCallback.NormalEnemyShot or
                EnemyInteractionCallback.DudShot or EnemyInteractionCallback.NoOp or EnemyInteractionCallback.NoOpShortReturn)
            return common;
        return ByAddress.TryGetValue((int)new SnesAddress(bank, pointer), out EnemyInteractionCallback local) ? local : null;
    }

    /// <summary>Whether a header's initialization or main-AI word names the shared no-op or the species' routine.</summary>
    internal static EnemyAiRoutine AiRoutine(byte bank, ushort pointer)
    {
        var routine = (EnemyCommonNoOpRoutine)(int)new SnesAddress(bank, pointer);
        return routine != EnemyCommonNoOpRoutine.None && Enum.IsDefined(routine)
            ? EnemyAiRoutine.CommonNoOp
            : EnemyAiRoutine.SpeciesRoutine;
    }

    /// <summary>Whether a header's hurt-AI word names the shared no-op or the species' routine.</summary>
    internal static EnemyAiRoutine HurtRoutine(ushort pointer) =>
        pointer == (ushort)EnemyNoOpRoutine.LongReturn ? EnemyAiRoutine.CommonNoOp : EnemyAiRoutine.SpeciesRoutine;

    /// <summary>What a callback word does: nothing to call, a routine, or an immediate return.</summary>
    internal static EnemyCallbackBody Body(ushort pointer, bool literalNoOp) =>
        pointer == 0 ? EnemyCallbackBody.None
        : literalNoOp ? EnemyCallbackBody.ImmediateReturn
        : EnemyCallbackBody.Routine;

    /// <summary>What <paramref name="bank"/>'s code runs for the shared no-op callback.</summary>
    internal static EnemyCallbackBody NoOpCallbackBody(byte bank) =>
        EnemyShotCallbackDefinitions.IsLiteralNoOp(bank, (ushort)EnemyNoOpRoutine.LongReturn)
            ? EnemyCallbackBody.ImmediateReturn
            : EnemyCallbackBody.Routine;
}
