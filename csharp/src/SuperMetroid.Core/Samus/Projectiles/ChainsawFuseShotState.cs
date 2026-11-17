namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The Power Bomb fuse the Chainsaw shot runs over itself (#2346), created when the shot is fired.</summary>
internal sealed class ChainsawFuseShotState : SamusShotKindState
{
    /// <summary>The fuse, from zero when the shot is fired.</summary>
    internal WrappingCountdown Fuse { get; set; } = WrappingCountdown.Start(0);
}
