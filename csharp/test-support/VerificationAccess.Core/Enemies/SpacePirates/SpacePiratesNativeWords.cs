using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// Verification's native view of the SpacePirates identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class SpacePiratesNativeWords
{
    /// <summary>The routine word naming each <see cref="NinjaSpacePirateAction"/>.</summary>
    internal static readonly NativeWordIdentities<NinjaSpacePirateAction> NinjaSpacePirateActions = new(new Dictionary<NinjaSpacePirateAction, ushort>
    {
        [NinjaSpacePirateAction.FollowingAnimation] = 0x804b,
        [NinjaSpacePirateAction.Initial] = 0xf6a9,
        [NinjaSpacePirateAction.Active] = 0xf6e4,
        [NinjaSpacePirateAction.SpinJumpLeftRising] = 0xf817,
        [NinjaSpacePirateAction.SpinJumpLeftFalling] = 0xf84c,
        [NinjaSpacePirateAction.SpinJumpRightRising] = 0xf890,
        [NinjaSpacePirateAction.SpinJumpRightFalling] = 0xf8c5,
        [NinjaSpacePirateAction.ReadyToDivekick] = 0xf909,
        [NinjaSpacePirateAction.DivekickLeftJump] = 0xf985,
        [NinjaSpacePirateAction.DivekickLeftDive] = 0xf9c1,
        [NinjaSpacePirateAction.DivekickLeftWalkToPost] = 0xfa15,
        [NinjaSpacePirateAction.DivekickRightJump] = 0xfa59,
        [NinjaSpacePirateAction.DivekickRightDive] = 0xfa95,
        [NinjaSpacePirateAction.DivekickRightWalkToPost] = 0xfae9,
    });

    /// <summary>The routine word naming each <see cref="WalkingSpacePirateAction"/>.</summary>
    /// <remarks>The shared return stub and the pirate's own empty routine both hold it while its animation runs (#1837).</remarks>
    internal static readonly NativeWordIdentities<WalkingSpacePirateAction> WalkingSpacePirateActions = new(new Dictionary<ushort, WalkingSpacePirateAction>
    {
        [0x804b] = WalkingSpacePirateAction.FollowingAnimation,
        [0xfd44] = WalkingSpacePirateAction.WalkingLeft,
        [0xfdce] = WalkingSpacePirateAction.WalkingRight,
        [0xfe4a] = WalkingSpacePirateAction.FollowingAnimation,
    });

    /// <summary>The climb-direction word naming each <see cref="WallSpacePirateClimbDirection"/>.</summary>
    internal static readonly NativeWordIdentities<WallSpacePirateClimbDirection> WallClimbDirections = new(new Dictionary<WallSpacePirateClimbDirection, ushort>
    {
        [WallSpacePirateClimbDirection.Down] = 0,
        [WallSpacePirateClimbDirection.Up] = 1,
    });

    /// <summary>The routine word naming each <see cref="WallSpacePirateAction"/>.</summary>
    /// <remarks>Each wall's empty attack routine holds the pirate while its attack animation runs (#1837).</remarks>
    internal static readonly NativeWordIdentities<WallSpacePirateAction> WallSpacePirateActions = new(new Dictionary<ushort, WallSpacePirateAction>
    {
        [0xf034] = WallSpacePirateAction.ClimbingLeftWall,
        [0xf04f] = WallSpacePirateAction.Attacking,
        [0xf050] = WallSpacePirateAction.WallJumpingRight,
        [0xf0c8] = WallSpacePirateAction.ClimbingRightWall,
        [0xf0e3] = WallSpacePirateAction.Attacking,
        [0xf0e4] = WallSpacePirateAction.WallJumpingLeft,
    });
}
