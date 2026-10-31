namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>The geometry of the Yapping Maw's curved attack: how far it reaches and where it holds Samus.</summary>
public static class YappingMawAttackGeometry
{
    /// <summary>
    /// The longest target length the neutral maw keeps: a shorter distance to Samus is used as it is, a longer one is
    /// capped here before the link radius is computed.
    /// </summary>
    public const int MaximumTargetDistance = 64;

    /// <summary>
    /// Where a maw holds Samus for each of its eight clockwise directions from up: an equal-step diamond of Manhattan
    /// radius sixteen, each step exchanging eight pixels between the two components, opposite directions negating both.
    /// The extension quantizes and wraps its angle before this selection.
    /// </summary>
    internal static Offset16<RoomPixels> HeldSamusOffset(int direction)
    {
        if ((uint)direction >= 8) throw new ArgumentOutOfRangeException(nameof(direction));
        static int Component(int phase) => WordWrap.Signed(8 * (2 - Math.Abs(phase - 4)));
        return new(Component((direction + 2) & 7), Component(direction));
    }
}
