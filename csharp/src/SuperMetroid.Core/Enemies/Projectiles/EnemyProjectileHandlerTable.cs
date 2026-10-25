namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// The projectile engine's dispatch table: each closed native pre-instruction maps to exactly one handler.
/// Registering an identity twice fails immediately with both registrations' identities.
/// </summary>
internal sealed class EnemyProjectileHandlerTable
{
    /// <summary>Closed native pre-instruction identities and their sole registered implementations.</summary>
    private readonly Dictionary<EnemyProjectilePreInstruction, EnemyProjectilePreInstructionHandler> _preInstructions = [];
    /// <summary>Kind-specific pre-instruction hooks evaluated before closed native dispatch.</summary>
    private readonly List<EnemyProjectilePreInstructionInterceptor> _preInstructionInterceptors = [];

    /// <summary>Adds a pre-instruction interceptor; interceptors run in registration order before the closed decode.</summary>
    internal void InterceptPreInstructions(EnemyProjectilePreInstructionInterceptor interceptor) =>
        _preInstructionInterceptors.Add(Ensure.NotNull(interceptor));

    /// <summary>Whether an interceptor ran <paramref name="projectile"/>'s pre-instruction.</summary>
    internal bool TryInterceptPreInstruction(RoomEnemyProjectileSlot projectile)
    {
        foreach (EnemyProjectilePreInstructionInterceptor interceptor in _preInstructionInterceptors)
        {
            if (interceptor(projectile))
                return true;
        }
        return false;
    }

    /// <summary>Registers <paramref name="handler"/> for every listed pre-instruction.</summary>
    internal void AddPreInstruction(EnemyProjectilePreInstructionHandler handler, params EnemyProjectilePreInstruction[] ids)
    {
        Ensure.NotNull(handler);
        foreach (EnemyProjectilePreInstruction id in ids)
        {
            if (!_preInstructions.TryAdd(id, handler))
                throw new InvalidOperationException($"Enemy projectile pre-instruction {id} is registered twice.");
        }
    }

    /// <summary>Typed animations a shot projectile of each kind starts in place of its native shot list.</summary>
    private readonly Dictionary<RoomEnemyProjectileKind, Func<EnemyProjectileAnimation>> _shotAnimations = [];

    /// <summary>Typed animations a projectile of each kind starts on touching Samus, in place of its native touch list.</summary>
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

    /// <summary>Registers the handler that carries out every cue of type <typeparamref name="TCue"/> and may redirect its program.</summary>
    internal void AddBranchingCue<TProgram, TCue>(EnemyProjectileBranchingCueHandler<TProgram, TCue> handler)
        where TProgram : struct
    {
        if (!_cues.TryAdd(typeof(TCue), Ensure.NotNull(handler)))
            throw new InvalidOperationException($"Enemy projectile cue {typeof(TCue).Name} is registered twice.");
    }

    /// <summary>
    /// Carries out <paramref name="cue"/> of a program animation with its family's registered handler, returning the
    /// program a branching handler redirects to, or null.
    /// </summary>
    internal TProgram? PublishProgramCue<TProgram, TCue>(EnemyProjectileCueContext context, TCue cue)
        where TProgram : struct
    {
        if (!_cues.TryGetValue(typeof(TCue), out Delegate? handler))
            throw new InvalidOperationException($"Enemy projectile cue {typeof(TCue).Name}.{cue} has no registered handler.");
        switch (handler)
        {
            case EnemyProjectileBranchingCueHandler<TProgram, TCue> branching:
                return branching(context, cue);
            case EnemyProjectileCueHandler<TCue> plain:
                plain(context, cue);
                return null;
            default:
                throw new InvalidOperationException($"Enemy projectile cue {typeof(TCue).Name}'s handler does not run {typeof(TProgram).Name} programs.");
        }
    }

    /// <summary>The handler registered for <paramref name="id"/>, if any.</summary>
    internal bool TryGetPreInstruction(EnemyProjectilePreInstruction id, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out EnemyProjectilePreInstructionHandler? handler) =>
        _preInstructions.TryGetValue(id, out handler);
}
