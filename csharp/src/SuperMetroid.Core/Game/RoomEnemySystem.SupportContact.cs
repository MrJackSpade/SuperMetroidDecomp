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
        ushort xDistance = WrappedMagnitude(unchecked((ushort)(samus.XPosition - enemy.XPosition)));
        ushort horizontalGap = unchecked((ushort)(xDistance - samus.Kinematics.XRadius));
        bool horizontalOverlap = xDistance < samus.Kinematics.XRadius ||
            horizontalGap < enemy.XRadius;
        if (!horizontalOverlap)
            return false;

        ushort biasedYDifference = unchecked((ushort)(samus.YPosition + 3 - enemy.YPosition));
        if (unchecked((short)biasedYDifference) >= 0)
            return false;
        ushort yDistance = unchecked((ushort)-biasedYDifference);
        ushort verticalGap = unchecked((ushort)(yDistance - samus.Kinematics.YRadius));
        return yDistance < samus.Kinematics.YRadius || verticalGap <= enemy.YRadius;
    }
}
