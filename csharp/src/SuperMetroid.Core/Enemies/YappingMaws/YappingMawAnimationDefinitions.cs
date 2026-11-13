namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>Animation selection for the Yapping Maw.</summary>
internal static class YappingMawAnimationDefinitions
{
    /// <summary>The eight attack bites in the clockwise octant order, starting facing up.</summary>
    internal static readonly Dictionary<YappingMawAim, YappingMawSegment> Attacks = new()
    {
        [YappingMawAim.Up] = YappingMawSegment.AttackingUp,
        [YappingMawAim.UpRight] = YappingMawSegment.AttackingUpRight,
        [YappingMawAim.Right] = YappingMawSegment.AttackingRight,
        [YappingMawAim.DownRight] = YappingMawSegment.AttackingDownRight,
        [YappingMawAim.Down] = YappingMawSegment.AttackingDown,
        [YappingMawAim.DownLeft] = YappingMawSegment.AttackingDownLeft,
        [YappingMawAim.Left] = YappingMawSegment.AttackingLeft,
        [YappingMawAim.UpLeft] = YappingMawSegment.AttackingUpLeft,
    };

    /// <summary>The aim sectors clockwise from straight up, each a thirty-second of a turn either side of its direction.</summary>
    internal static readonly IdentitySequence<YappingMawAim> Aims = new(
        YappingMawAim.Up, YappingMawAim.UpRight, YappingMawAim.Right, YappingMawAim.DownRight,
        YappingMawAim.Down, YappingMawAim.DownLeft, YappingMawAim.Left, YappingMawAim.UpLeft);
}
