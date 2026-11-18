namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>The Power Bomb fuse the Chainsaw shot runs over itself (#2346), created when the shot is fired.</summary>
internal sealed class ChainsawFuseShotState : SamusShotKindState
{
    /// <summary>The updates the fuse has left, from zero when the shot is fired: a fuse already burned out (#2458).</summary>
    internal int FuseUpdatesLeft
    {
        get;
        set => field = Ensure.AtLeastZero(value);
    }
}
