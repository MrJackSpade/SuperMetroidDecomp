using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.Gunship;

/// <summary>
/// Verification's native view of the Gunship identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class GunshipNativeWords
{
    /// <summary>The routine word naming each <see cref="GunshipFunction"/>.</summary>
    internal static readonly NativeWordIdentities<GunshipFunction> GunshipFunctions = new(new Dictionary<GunshipFunction, ushort>
    {
        [GunshipFunction.NoOperation] = 0x804c,
        [GunshipFunction.DescendAfterCeres] = 0xa80c,
        [GunshipFunction.ApplyLandingBrakes] = 0xa8d0,
        [GunshipFunction.WaitForLandingEntranceToOpen] = 0xa942,
        [GunshipFunction.FinishLanding] = 0xa987,
        [GunshipFunction.EjectSamus] = 0xa950,
        [GunshipFunction.Idle] = 0xa9bd,
        [GunshipFunction.WaitForEntranceToOpen] = 0xaa4f,
        [GunshipFunction.LowerSamus] = 0xaa5d,
        [GunshipFunction.WaitForEntranceToClose] = 0xaa94,
        [GunshipFunction.BeginLiftoffOrRestoreSamus] = 0xaaa2,
        [GunshipFunction.HandleSaveConfirmation] = 0xab1f,
        [GunshipFunction.WaitForExitPadToOpen] = 0xab60,
        [GunshipFunction.RaiseSamus] = 0xab6e,
        [GunshipFunction.FinishSamusExit] = 0xaba5,
        [GunshipFunction.LoadLiftoffDustTiles] = 0xabc7,
        [GunshipFunction.FireUpEngines] = 0xac1b,
        [GunshipFunction.SteadyLiftoff] = 0xacd7,
        [GunshipFunction.AcceleratingLiftoff] = 0xad0e,
        [GunshipFunction.MoveAccelerating] = 0xad2d,
    });
}
