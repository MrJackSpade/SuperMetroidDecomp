namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The projectile engine's dispatch table: each frame behaviour maps to exactly one handler.
/// Registering an identity twice fails immediately with both registrations' identities.
/// </summary>
internal sealed class EnemyProjectileHandlerTable
{
    /// <summary>Each frame behaviour and its sole registered implementation.</summary>
    private readonly Dictionary<EnemyProjectileFrameBehavior, EnemyProjectileFrameBehaviorHandler> _frameBehaviors = [];

    /// <summary>Registers <paramref name="handler"/> for every listed frame behaviour.</summary>
    internal void AddFrameBehavior(EnemyProjectileFrameBehaviorHandler handler, params EnemyProjectileFrameBehavior[] ids)
    {
        Ensure.NotNull(handler);
        foreach (EnemyProjectileFrameBehavior id in ids)
        {
            if (!_frameBehaviors.TryAdd(id, handler))
                throw new InvalidOperationException($"Enemy projectile frame behaviour {id} is registered twice.");
        }
    }

    /// <summary>Typed animations a shot projectile of each kind starts.</summary>
    private readonly Dictionary<RoomEnemyProjectileKind, Func<EnemyProjectileAnimation>> _shotAnimations = [];

    /// <summary>Typed animations a projectile of each kind starts on touching Samus.</summary>
    private readonly Dictionary<RoomEnemyProjectileKind, Func<EnemyProjectileAnimation>> _touchAnimations = [];

    /// <summary>Registers the typed animation a projectile of <paramref name="kind"/> starts when it touches Samus.</summary>
    internal void AddTouchAnimation(RoomEnemyProjectileKind kind, Func<EnemyProjectileAnimation> animation)
    {
        if (!_touchAnimations.TryAdd(kind, Ensure.NotNull(animation)))
            throw new InvalidOperationException($"Enemy projectile {kind} has two touch animations.");
    }

    /// <summary>The typed touch animation registered for <paramref name="kind"/>, if its family has one.</summary>
    internal bool TryGetTouchAnimation(RoomEnemyProjectileKind kind, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Func<EnemyProjectileAnimation>? animation) =>
        _touchAnimations.TryGetValue(kind, out animation);

    /// <summary>Registers the typed animation a shot projectile of <paramref name="kind"/> starts.</summary>
    internal void AddShotAnimation(RoomEnemyProjectileKind kind, Func<EnemyProjectileAnimation> animation)
    {
        if (!_shotAnimations.TryAdd(kind, Ensure.NotNull(animation)))
            throw new InvalidOperationException($"Enemy projectile {kind} has two shot animations.");
    }

    /// <summary>The typed shot animation registered for <paramref name="kind"/>, if its family has one.</summary>
    internal bool TryGetShotAnimation(RoomEnemyProjectileKind kind, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Func<EnemyProjectileAnimation>? animation) =>
        _shotAnimations.TryGetValue(kind, out animation);

    /// <summary>Each typed timeline family's cue handler, keyed by its cue type.</summary>
    private readonly Dictionary<Type, Delegate> _cues = [];

    /// <summary>Registers the handler that carries out every cue of type <typeparamref name="TCue"/>.</summary>
    internal void AddCue<TCue>(EnemyProjectileCueHandler<TCue> handler)
    {
        if (!_cues.TryAdd(typeof(TCue), Ensure.NotNull(handler)))
            throw new InvalidOperationException($"Enemy projectile cue {typeof(TCue).Name} is registered twice.");
    }

    /// <summary>Registers the handler that carries out every cue of type <typeparamref name="TCue"/> and may redirect its animation.</summary>
    internal void AddBranchingCue<TSegment, TCue>(EnemyProjectileBranchingCueHandler<TSegment, TCue> handler)
        where TSegment : struct
    {
        if (!_cues.TryAdd(typeof(TCue), Ensure.NotNull(handler)))
            throw new InvalidOperationException($"Enemy projectile cue {typeof(TCue).Name} is registered twice.");
    }

    /// <summary>
    /// Carries out <paramref name="cue"/> of a typed animation with its family's registered handler, returning the
    /// segment a branching handler redirects to, or null.
    /// </summary>
    internal TSegment? PublishAnimationCue<TSegment, TCue>(EnemyProjectileCueContext context, TCue cue)
        where TSegment : struct
    {
        if (!_cues.TryGetValue(typeof(TCue), out Delegate? handler))
            throw new InvalidOperationException($"Enemy projectile cue {typeof(TCue).Name}.{cue} has no registered handler.");
        switch (handler)
        {
            case EnemyProjectileBranchingCueHandler<TSegment, TCue> branching:
                return branching(context, cue);
            case EnemyProjectileCueHandler<TCue> plain:
                plain(context, cue);
                return null;
            default:
                throw new InvalidOperationException($"Enemy projectile cue {typeof(TCue).Name}'s handler does not run {typeof(TSegment).Name} programs.");
        }
    }

    /// <summary>The handler registered for <paramref name="id"/>, if any.</summary>
    internal bool TryGetFrameBehavior(EnemyProjectileFrameBehavior id, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out EnemyProjectileFrameBehaviorHandler? handler) =>
        _frameBehaviors.TryGetValue(id, out handler);
}
