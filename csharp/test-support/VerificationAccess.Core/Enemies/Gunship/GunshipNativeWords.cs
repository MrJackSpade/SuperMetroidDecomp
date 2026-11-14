using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// Verification's native view of the Gunship identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class GunshipNativeWords
{
    /// <summary>The routine word naming each <see cref="GunshipPhase"/>.</summary>
    internal static readonly NativeWordIdentities<GunshipPhase> GunshipPhases = new(new Dictionary<GunshipPhase, ushort>
    {
        [GunshipPhase.DescendAfterCeres] = 0xa80c,
        [GunshipPhase.ApplyLandingBrakes] = 0xa8d0,
        [GunshipPhase.WaitForLandingEntranceToOpen] = 0xa942,
        [GunshipPhase.FinishLanding] = 0xa987,
        [GunshipPhase.EjectSamus] = 0xa950,
        [GunshipPhase.Idle] = 0xa9bd,
        [GunshipPhase.WaitForEntranceToOpen] = 0xaa4f,
        [GunshipPhase.LowerSamus] = 0xaa5d,
        [GunshipPhase.WaitForEntranceToClose] = 0xaa94,
        [GunshipPhase.BeginLiftoffOrRestoreSamus] = 0xaaa2,
        [GunshipPhase.HandleSaveConfirmation] = 0xab1f,
        [GunshipPhase.WaitForExitPadToOpen] = 0xab60,
        [GunshipPhase.RaiseSamus] = 0xab6e,
        [GunshipPhase.FinishSamusExit] = 0xaba5,
        [GunshipPhase.ReadyTakeoffEngines] = 0xabc7,
        [GunshipPhase.FireUpEngines] = 0xac1b,
        [GunshipPhase.SteadyLiftoff] = 0xacd7,
        [GunshipPhase.AcceleratingLiftoff] = 0xad0e,
        [GunshipPhase.MoveAccelerating] = 0xad2d,
    });
}
