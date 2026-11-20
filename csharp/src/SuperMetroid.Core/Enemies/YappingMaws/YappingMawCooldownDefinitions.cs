namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>
/// The retracted animation a Yapping Maw cools down in after a grab, by its aim: the right and left sectors have dedicated
/// art, while every other sector shares its attack direction's animation.
/// </summary>
internal static class YappingMawCooldownDefinitions
{
    /// <summary>The cooldown animation of a Maw that attacks upward, by its aim.</summary>
    internal static IdentityTable<YappingMawAim, YappingMawSegment> Upward { get; } = new(new Dictionary<YappingMawAim, YappingMawSegment>
    {
        [YappingMawAim.Up] = YappingMawSegment.CooldownUp,
        [YappingMawAim.UpRight] = YappingMawSegment.CooldownUp,
        [YappingMawAim.Right] = YappingMawSegment.CooldownUpRight,
        [YappingMawAim.DownRight] = YappingMawSegment.CooldownUp,
        [YappingMawAim.Down] = YappingMawSegment.CooldownUp,
        [YappingMawAim.DownLeft] = YappingMawSegment.CooldownUp,
        [YappingMawAim.Left] = YappingMawSegment.CooldownUpLeft,
        [YappingMawAim.UpLeft] = YappingMawSegment.CooldownUp,
    });

    /// <summary>The cooldown animation of a Maw that attacks downward, by its aim.</summary>
    internal static IdentityTable<YappingMawAim, YappingMawSegment> Downward { get; } = new(new Dictionary<YappingMawAim, YappingMawSegment>
    {
        [YappingMawAim.Up] = YappingMawSegment.CooldownDown,
        [YappingMawAim.UpRight] = YappingMawSegment.CooldownDown,
        [YappingMawAim.Right] = YappingMawSegment.CooldownDownRight,
        [YappingMawAim.DownRight] = YappingMawSegment.CooldownDown,
        [YappingMawAim.Down] = YappingMawSegment.CooldownDown,
        [YappingMawAim.DownLeft] = YappingMawSegment.CooldownDown,
        [YappingMawAim.Left] = YappingMawSegment.CooldownDownLeft,
        [YappingMawAim.UpLeft] = YappingMawSegment.CooldownDown,
    });
}
