using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// Verification's native view of the SpacePirates identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class SpacePiratesNativeWords
{
    /// <summary>
    /// The routine word naming each <see cref="NinjaSpacePirateAction"/> and, for a move that goes one way, its heading
    /// (#2165), the cartridge giving each heading a routine of its own.
    /// </summary>
    internal static readonly NativeWordIdentities<(NinjaSpacePirateAction Action, HorizontalDirection? Heading)> NinjaSpacePirateActions =
        new(new Dictionary<(NinjaSpacePirateAction Action, HorizontalDirection? Heading), ushort>
        {
            [(NinjaSpacePirateAction.StandingStill, null)] = 0x804b,
            [(NinjaSpacePirateAction.Initial, null)] = 0xf6a9,
            [(NinjaSpacePirateAction.Active, null)] = 0xf6e4,
            [(NinjaSpacePirateAction.SpinJumpRising, HorizontalDirection.Left)] = 0xf817,
            [(NinjaSpacePirateAction.SpinJumpFalling, HorizontalDirection.Left)] = 0xf84c,
            [(NinjaSpacePirateAction.SpinJumpRising, HorizontalDirection.Right)] = 0xf890,
            [(NinjaSpacePirateAction.SpinJumpFalling, HorizontalDirection.Right)] = 0xf8c5,
            [(NinjaSpacePirateAction.ReadyToDivekick, null)] = 0xf909,
            [(NinjaSpacePirateAction.DivekickJump, HorizontalDirection.Left)] = 0xf985,
            [(NinjaSpacePirateAction.DivekickDive, HorizontalDirection.Left)] = 0xf9c1,
            [(NinjaSpacePirateAction.DivekickWalkToPost, HorizontalDirection.Left)] = 0xfa15,
            [(NinjaSpacePirateAction.DivekickJump, HorizontalDirection.Right)] = 0xfa59,
            [(NinjaSpacePirateAction.DivekickDive, HorizontalDirection.Right)] = 0xfa95,
            [(NinjaSpacePirateAction.DivekickWalkToPost, HorizontalDirection.Right)] = 0xfae9,
        });

    /// <summary>The routine word naming each <see cref="WalkingSpacePirateAction"/> and, for a walk, its heading (#2165).</summary>
    /// <remarks>The shared return stub and the pirate's own empty routine both hold it while its animation runs (#1837).</remarks>
    internal static readonly NativeWordIdentities<(WalkingSpacePirateAction Action, HorizontalDirection? Heading)> WalkingSpacePirateActions =
        new(new Dictionary<ushort, (WalkingSpacePirateAction Action, HorizontalDirection? Heading)>
        {
            [0x804b] = (WalkingSpacePirateAction.StandingStill, null),
            [0xfd44] = (WalkingSpacePirateAction.Walking, HorizontalDirection.Left),
            [0xfdce] = (WalkingSpacePirateAction.Walking, HorizontalDirection.Right),
            [0xfe4a] = (WalkingSpacePirateAction.StandingStill, null),
        });

    /// <summary>The climb-direction word naming each <see cref="WallSpacePirateClimbDirection"/>.</summary>
    internal static readonly NativeWordIdentities<WallSpacePirateClimbDirection> WallClimbDirections = new(new Dictionary<WallSpacePirateClimbDirection, ushort>
    {
        [WallSpacePirateClimbDirection.Down] = 0,
        [WallSpacePirateClimbDirection.Up] = 1,
    });

    /// <summary>
    /// The routine word naming each <see cref="WallSpacePirateAction"/> and, for a cling or a jump, its wall (#2165), the
    /// cartridge giving each wall a routine of its own.
    /// </summary>
    /// <remarks>Each wall's empty attack routine holds the pirate while its attack animation runs (#1837).</remarks>
    internal static readonly NativeWordIdentities<(WallSpacePirateAction Action, HorizontalDirection? Wall)> WallSpacePirateActions =
        new(new Dictionary<ushort, (WallSpacePirateAction Action, HorizontalDirection? Wall)>
        {
            [0xf034] = (WallSpacePirateAction.Climbing, HorizontalDirection.Left),
            [0xf04f] = (WallSpacePirateAction.Attacking, null),
            [0xf050] = (WallSpacePirateAction.WallJumping, HorizontalDirection.Right),
            [0xf0c8] = (WallSpacePirateAction.Climbing, HorizontalDirection.Right),
            [0xf0e3] = (WallSpacePirateAction.Attacking, null),
            [0xf0e4] = (WallSpacePirateAction.WallJumping, HorizontalDirection.Left),
        });
}
