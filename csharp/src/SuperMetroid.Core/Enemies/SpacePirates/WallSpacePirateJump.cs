namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>How a wall Space Pirate's jump between the walls turns (#2182).</summary>
/// <param name="RightTarget">The angle at which a jump to the right wall lands.</param>
/// <param name="LeftTarget">The angle at which a jump to the left wall lands.</param>
/// <param name="TurnPerUpdate">How far the jump's angle turns each update.</param>
internal sealed partial record WallSpacePirateJump(RotationAngle RightTarget, RotationAngle LeftTarget, RotationAngle TurnPerUpdate);
