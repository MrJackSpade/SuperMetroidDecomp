namespace SuperMetroid.Core.Enemies.Hoppers;

/// <summary>Animation dispatch shared by Sidehoppers and Dessgeegas.</summary>
internal static class HopperAnimationDefinitions
{
    /// <summary>Each kind of hopper's animation for each orientation and movement phase.</summary>
    internal static readonly Dictionary<HopperAnimationKey, HopperSegment> Animations = new()
    {
        [new(HopperKind.Sidehopper, HopperSurface.Floor, HopperPhase.Jumping)] = HopperSegment.SidehopperFloorJumping,
        [new(HopperKind.Sidehopper, HopperSurface.Floor, HopperPhase.Landed)] = HopperSegment.SidehopperFloorLanded,
        [new(HopperKind.Sidehopper, HopperSurface.Ceiling, HopperPhase.Jumping)] = HopperSegment.SidehopperCeilingJumping,
        [new(HopperKind.Sidehopper, HopperSurface.Ceiling, HopperPhase.Landed)] = HopperSegment.SidehopperCeilingLanded,
        [new(HopperKind.LargeSidehopper, HopperSurface.Floor, HopperPhase.Jumping)] = HopperSegment.LargeSidehopperFloorJumping,
        [new(HopperKind.LargeSidehopper, HopperSurface.Floor, HopperPhase.Landed)] = HopperSegment.LargeSidehopperFloorLanded,
        [new(HopperKind.LargeSidehopper, HopperSurface.Ceiling, HopperPhase.Jumping)] = HopperSegment.LargeSidehopperCeilingJumping,
        [new(HopperKind.LargeSidehopper, HopperSurface.Ceiling, HopperPhase.Landed)] = HopperSegment.LargeSidehopperCeilingLanded,
        [new(HopperKind.LargeDessgeega, HopperSurface.Floor, HopperPhase.Jumping)] = HopperSegment.LargeDessgeegaFloorJumping,
        [new(HopperKind.LargeDessgeega, HopperSurface.Floor, HopperPhase.Landed)] = HopperSegment.LargeDessgeegaFloorLanded,
        [new(HopperKind.LargeDessgeega, HopperSurface.Ceiling, HopperPhase.Jumping)] = HopperSegment.LargeDessgeegaCeilingJumping,
        [new(HopperKind.LargeDessgeega, HopperSurface.Ceiling, HopperPhase.Landed)] = HopperSegment.LargeDessgeegaCeilingLanded,
        [new(HopperKind.Dessgeega, HopperSurface.Floor, HopperPhase.Jumping)] = HopperSegment.DessgeegaFloorJumping,
        [new(HopperKind.Dessgeega, HopperSurface.Floor, HopperPhase.Landed)] = HopperSegment.DessgeegaFloorLanded,
        [new(HopperKind.Dessgeega, HopperSurface.Ceiling, HopperPhase.Jumping)] = HopperSegment.DessgeegaCeilingJumping,
        [new(HopperKind.Dessgeega, HopperSurface.Ceiling, HopperPhase.Landed)] = HopperSegment.DessgeegaCeilingLanded,
    };
}
