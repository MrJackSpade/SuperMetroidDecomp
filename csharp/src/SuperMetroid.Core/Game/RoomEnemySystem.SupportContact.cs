namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    /// <summary>
    /// Exact asymmetric support test from CheckIfEnemyIsTouchingSamusFromBelow,
    /// $A0:ABE7. Horizontal tangency is excluded; vertical tangency after the
    /// three-pixel bias is included. Both Kraid platforms and work robots call it.
    /// </summary>
    private static bool IsEnemyTouchingSamusFromBelow(RoomEnemySlot enemy, SamusState samus)
    {
        ushort xDistance = WrappedMagnitude(unchecked((ushort)(samus.Position.X.Whole - enemy.XPosition)));
        ushort horizontalGap = unchecked((ushort)(xDistance - samus.Kinematics.Radius.X));
        bool horizontalOverlap = xDistance < samus.Kinematics.Radius.X ||
            horizontalGap < enemy.XRadius;
        if (!horizontalOverlap)
            return false;

        ushort biasedYDifference = unchecked((ushort)(samus.Position.Y.Whole + 3 - enemy.YPosition));
        if (unchecked((short)biasedYDifference) >= 0)
            return false;
        ushort yDistance = unchecked((ushort)-biasedYDifference);
        ushort verticalGap = unchecked((ushort)(yDistance - samus.Kinematics.Radius.Y));
        return yDistance < samus.Kinematics.Radius.Y || verticalGap <= enemy.YRadius;
    }
}
