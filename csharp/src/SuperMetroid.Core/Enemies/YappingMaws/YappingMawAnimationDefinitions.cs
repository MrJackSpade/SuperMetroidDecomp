namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>Animation selection for the Yapping Maw.</summary>
internal static class YappingMawAnimationDefinitions
{
    /// <summary>The eight attack bites in the native clockwise octant order, starting facing up.</summary>
    internal static readonly Dictionary<YappingMawAim, YappingMawProgram> Attacks = new()
    {
        [YappingMawAim.Up] = YappingMawProgram.AttackingUp,
        [YappingMawAim.UpRight] = YappingMawProgram.AttackingUpRight,
        [YappingMawAim.Right] = YappingMawProgram.AttackingRight,
        [YappingMawAim.DownRight] = YappingMawProgram.AttackingDownRight,
        [YappingMawAim.Down] = YappingMawProgram.AttackingDown,
        [YappingMawAim.DownLeft] = YappingMawProgram.AttackingDownLeft,
        [YappingMawAim.Left] = YappingMawProgram.AttackingLeft,
        [YappingMawAim.UpLeft] = YappingMawProgram.AttackingUpLeft,
    };

    /// <summary>The aim sectors clockwise from straight up, each a thirty-second of a turn either side of its direction.</summary>
    internal static readonly IdentitySequence<YappingMawAim> Aims = new(
        YappingMawAim.Up, YappingMawAim.UpRight, YappingMawAim.Right, YappingMawAim.DownRight,
        YappingMawAim.Down, YappingMawAim.DownLeft, YappingMawAim.Left, YappingMawAim.UpLeft);
}
