using SuperMetroid.Core.Enemies.BossBodies;

namespace SuperMetroid.Core.Gameplay;

/// <summary>A boss's body while the boss owns the background (#1352, #1466, #1514), over whatever the room would show there.</summary>
public sealed record BossBodyBackground : ShownRoomBackground
{
    /// <summary>Keeps the body's pictures, placed where they show.</summary>
    /// <param name="body">The body's pictures, placed where they show.</param>
    public BossBodyBackground(ShownBossBody body) => Body = Ensure.NotNull(body);

    /// <summary>The body's pictures, placed where they show.</summary>
    private ShownBossBody Body { get; }

    /// <inheritdoc/>
    public override TResult Map<TResult>(IShownRoomBackgroundMapping<TResult> mapping) => Ensure.NotNull(mapping).BossBody(Body);
}
