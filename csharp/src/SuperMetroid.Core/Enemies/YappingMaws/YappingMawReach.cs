using SuperMetroid.Core.Enemies.EffectActors;

namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>
/// The way a Yapping Maw reaches from its root (#2557), up or down, and what it shows reaching that way: its body's
/// facing, its root's kind, the bite it starts with, and its cooldowns after a bite to either side or straight along it.
/// </summary>
/// <param name="Body">The animation its body's links show.</param>
/// <param name="Root">The kind of root it spawns.</param>
/// <param name="Bite">The bite it starts with.</param>
/// <param name="RightCooldown">Its cooldown after biting to the right.</param>
/// <param name="LeftCooldown">Its cooldown after biting to the left.</param>
/// <param name="StraightCooldown">Its cooldown after a bite at any other aim, which every other sector shares.</param>
internal sealed record YappingMawReach(YappingMawBodyProjectileSegment Body, RoomEffectActorKind Root, YappingMawSegment Bite,
    YappingMawSegment RightCooldown, YappingMawSegment LeftCooldown, YappingMawSegment StraightCooldown)
{
    /// <summary>A Maw reaching up.</summary>
    internal static YappingMawReach Up { get; } = new(YappingMawBodyProjectileSegment.FacingUp, RoomEffectActorKind.YappingMawBaseFacingUp,
        YappingMawSegment.AttackingUp, YappingMawSegment.CooldownUpRight, YappingMawSegment.CooldownUpLeft, YappingMawSegment.CooldownUp);

    /// <summary>A Maw reaching down.</summary>
    internal static YappingMawReach Down { get; } = new(YappingMawBodyProjectileSegment.FacingDown, RoomEffectActorKind.YappingMawBaseFacingDown,
        YappingMawSegment.AttackingDown, YappingMawSegment.CooldownDownRight, YappingMawSegment.CooldownDownLeft, YappingMawSegment.CooldownDown);

    /// <summary>The reach of a Maw attacking <paramref name="attack"/>.</summary>
    internal static YappingMawReach Of(VerticalDirection attack) => attack == VerticalDirection.Down ? Down : Up;

    /// <summary>The cooldown after a bite at <paramref name="aim"/>: the right and left sectors have their own, every other the straight one.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="aim"/> is not an aim.</exception>
    internal YappingMawSegment CooldownAfter(YappingMawAim aim) => aim switch
    {
        YappingMawAim.Right => RightCooldown,
        YappingMawAim.Left => LeftCooldown,
        YappingMawAim.Up or YappingMawAim.UpRight or YappingMawAim.DownRight or YappingMawAim.Down or
            YappingMawAim.DownLeft or YappingMawAim.UpLeft => StraightCooldown,
        _ => throw new InvalidOperationException($"Yapping Maw aim {aim} is not an aim."),
    };
}
