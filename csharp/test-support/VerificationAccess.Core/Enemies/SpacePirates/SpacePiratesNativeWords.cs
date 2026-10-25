using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// Verification's native view of the SpacePirates identities (#1412): the cartridge routine words naming each member. Core's
/// values are arbitrary; a native snapshot or ROM oracle translates through these tables.
/// </summary>
internal static class SpacePiratesNativeWords
{
    /// <summary>The routine word naming each <see cref="NinjaSpacePirateFunction"/>.</summary>
    internal static readonly NativeWordIdentities<NinjaSpacePirateFunction> NinjaSpacePirateFunctions = new(new Dictionary<NinjaSpacePirateFunction, ushort>
    {
        [NinjaSpacePirateFunction.NoOperation] = 0x804b,
        [NinjaSpacePirateFunction.Initial] = 0xf6a9,
        [NinjaSpacePirateFunction.Active] = 0xf6e4,
        [NinjaSpacePirateFunction.SpinJumpLeftRising] = 0xf817,
        [NinjaSpacePirateFunction.SpinJumpLeftFalling] = 0xf84c,
        [NinjaSpacePirateFunction.SpinJumpRightRising] = 0xf890,
        [NinjaSpacePirateFunction.SpinJumpRightFalling] = 0xf8c5,
        [NinjaSpacePirateFunction.ReadyToDivekick] = 0xf909,
        [NinjaSpacePirateFunction.DivekickLeftJump] = 0xf985,
        [NinjaSpacePirateFunction.DivekickLeftDive] = 0xf9c1,
        [NinjaSpacePirateFunction.DivekickLeftWalkToPost] = 0xfa15,
        [NinjaSpacePirateFunction.DivekickRightJump] = 0xfa59,
        [NinjaSpacePirateFunction.DivekickRightDive] = 0xfa95,
        [NinjaSpacePirateFunction.DivekickRightWalkToPost] = 0xfae9,
    });

    /// <summary>The routine word naming each <see cref="WalkingSpacePirateFunction"/>.</summary>
    internal static readonly NativeWordIdentities<WalkingSpacePirateFunction> WalkingSpacePirateFunctions = new(new Dictionary<WalkingSpacePirateFunction, ushort>
    {
        [WalkingSpacePirateFunction.NoOperation] = 0x804b,
        [WalkingSpacePirateFunction.WalkingLeft] = 0xfd44,
        [WalkingSpacePirateFunction.WalkingRight] = 0xfdce,
        [WalkingSpacePirateFunction.AnimationOwnedNoOperation] = 0xfe4a,
    });

    /// <summary>The routine word naming each <see cref="WallSpacePirateFunction"/>.</summary>
    internal static readonly NativeWordIdentities<WallSpacePirateFunction> WallSpacePirateFunctions = new(new Dictionary<WallSpacePirateFunction, ushort>
    {
        [WallSpacePirateFunction.ClimbingLeftWall] = 0xf034,
        [WallSpacePirateFunction.AttackAnimationOnLeftWall] = 0xf04f,
        [WallSpacePirateFunction.WallJumpingRight] = 0xf050,
        [WallSpacePirateFunction.ClimbingRightWall] = 0xf0c8,
        [WallSpacePirateFunction.AttackAnimationOnRightWall] = 0xf0e3,
        [WallSpacePirateFunction.WallJumpingLeft] = 0xf0e4,
    });
}
