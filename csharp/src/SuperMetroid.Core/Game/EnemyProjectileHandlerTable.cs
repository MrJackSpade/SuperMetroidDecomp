using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>Everything a bank-$86 pre-instruction receives for one projectile in one projectile pass.</summary>
internal readonly record struct EnemyProjectilePreInstructionContext(
    RoomEnemyProjectileSlot Projectile,
    RoomLevelData Level,
    SamusState? Samus,
    Position16<RoomPixels> Camera,
    byte NmiFrameCounter8,
    SamusBombProjectileSystem? SamusBombs);

/// <summary>One translated bank-$86 enemy-projectile pre-instruction.</summary>
internal delegate void EnemyProjectilePreInstructionHandler(EnemyProjectilePreInstructionContext context);

/// <summary>How the interpreter proceeds after a registered instruction.</summary>
internal enum EnemyProjectileInstructionOutcome
{
    /// <summary>No registered handler applies; the engine's own opcodes decide.</summary>
    NotHandled,
    /// <summary>The instruction advanced the cursor; the interpreter reads the next word.</summary>
    Continue,
    /// <summary>The instruction ended this projectile's instruction pass (for example by deleting it).</summary>
    Stop,
}

/// <summary>One translated species-owned bank-$86 instruction; it advances <paramref name="cursor"/> past its operands.</summary>
internal delegate EnemyProjectileInstructionOutcome EnemyProjectileInstructionHandler(
    RoomEnemyProjectileSlot projectile, SamusState? samus, Position16<RoomPixels> camera, ushort word, ref ushort cursor);

/// <summary>
/// Claims a projectile whose pre-instruction word lies outside the closed bank-$86 set (for example the Tourian
/// statue effects); returns true when it ran the pre-instruction.
/// </summary>
internal delegate bool EnemyProjectilePreInstructionInterceptor(RoomEnemyProjectileSlot projectile);

/// <summary>
/// Claims an instruction word outside the closed bank-$86 set, or a kind-specific reading of one; returns true
/// after executing it and advancing <paramref name="cursor"/> past its operands.
/// </summary>
internal delegate bool EnemyProjectileInstructionInterceptor(RoomEnemyProjectileSlot projectile, ushort word, ref ushort cursor);

/// <summary>Carries out one cue a typed projectile animation published on <paramref name="projectile"/>.</summary>
internal delegate void EnemyProjectileCueHandler<in TCue>(RoomEnemyProjectileSlot projectile, SamusState? samus, TCue cue);

/// <summary>An owner of enemy-projectile behavior that registers its handlers with the projectile engine.</summary>
internal interface IEnemyProjectileHandlerSource
{
    /// <summary>Registers every projectile handler this owner implements.</summary>
    void RegisterEnemyProjectileHandlers(EnemyProjectileHandlerTable table);
}

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
    /// <summary>Kind-specific instruction hooks evaluated before closed native dispatch.</summary>
    private readonly List<EnemyProjectileInstructionInterceptor> _instructionInterceptors = [];

    /// <summary>Adds a pre-instruction interceptor; interceptors run in registration order before the closed decode.</summary>
    internal void InterceptPreInstructions(EnemyProjectilePreInstructionInterceptor interceptor) =>
        _preInstructionInterceptors.Add(Ensure.NotNull(interceptor));

    /// <summary>Adds an instruction interceptor; interceptors run in registration order before the closed decode.</summary>
    internal void InterceptInstructions(EnemyProjectileInstructionInterceptor interceptor) =>
        _instructionInterceptors.Add(Ensure.NotNull(interceptor));

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

    /// <summary>Whether an interceptor executed <paramref name="word"/>.</summary>
    internal bool TryInterceptInstruction(RoomEnemyProjectileSlot projectile, ushort word, ref ushort cursor)
    {
        foreach (EnemyProjectileInstructionInterceptor interceptor in _instructionInterceptors)
        {
            if (interceptor(projectile, word, ref cursor))
                return true;
        }
        return false;
    }

    /// <summary>Registers <paramref name="handler"/> for every listed pre-instruction.</summary>
    internal void RegisterPreInstruction(EnemyProjectilePreInstructionHandler handler, params EnemyProjectilePreInstruction[] ids)
    {
        Ensure.NotNull(handler);
        foreach (EnemyProjectilePreInstruction id in ids)
        {
            if (!_preInstructions.TryAdd(id, handler))
                throw new InvalidOperationException($"Enemy projectile pre-instruction {id} is registered twice.");
        }
    }

    /// <summary>Ordered species handlers for each native instruction, with optional projectile guards.</summary>
    private readonly Dictionary<EnemyProjectileInstruction, List<(Func<RoomEnemyProjectileSlot, bool>? Applies, EnemyProjectileInstructionHandler Handler)>> _instructions = [];

    /// <summary>
    /// Registers a species instruction. Handlers for one instruction are tried in registration order; a guard
    /// selects the projectile kinds it interprets, and an unguarded handler must be the last for its instruction.
    /// </summary>
    internal void RegisterInstruction(EnemyProjectileInstruction id, Func<RoomEnemyProjectileSlot, bool>? applies, EnemyProjectileInstructionHandler handler)
    {
        Ensure.NotNull(handler);
        if (!_instructions.TryGetValue(id, out var entries))
            _instructions[id] = entries = [];
        if (entries.Count > 0 && entries[^1].Applies is null)
            throw new InvalidOperationException($"Enemy projectile instruction {id} already has an unconditional handler.");
        entries.Add((applies, handler));
    }

    /// <summary>Runs the first registered handler that applies to <paramref name="projectile"/>.</summary>
    internal EnemyProjectileInstructionOutcome TryExecuteInstruction(
        EnemyProjectileInstruction id, RoomEnemyProjectileSlot projectile, SamusState? samus, Position16<RoomPixels> camera,
        ushort word, ref ushort cursor)
    {
        if (!_instructions.TryGetValue(id, out var entries))
            return EnemyProjectileInstructionOutcome.NotHandled;
        foreach (var (applies, handler) in entries)
        {
            if (applies is null || applies(projectile))
                return handler(projectile, samus, camera, word, ref cursor);
        }
        return EnemyProjectileInstructionOutcome.NotHandled;
    }

    /// <summary>Typed animations a shot projectile of each kind starts in place of its native shot list.</summary>
    private readonly Dictionary<RoomEnemyProjectileKind, Func<EnemyProjectileAnimation>> _shotAnimations = [];

    /// <summary>Typed animations a projectile of each kind starts on touching Samus, in place of its native touch list.</summary>
    private readonly Dictionary<RoomEnemyProjectileKind, Func<EnemyProjectileAnimation>> _touchAnimations = [];

    /// <summary>Registers the typed animation a projectile of <paramref name="kind"/> starts when it touches Samus.</summary>
    internal void RegisterTouchAnimation(RoomEnemyProjectileKind kind, Func<EnemyProjectileAnimation> animation)
    {
        if (!_touchAnimations.TryAdd(kind, Ensure.NotNull(animation)))
            throw new InvalidOperationException($"Enemy projectile {kind} has two touch animations.");
    }

    /// <summary>The typed touch animation registered for <paramref name="kind"/>, if its family has one.</summary>
    internal bool TryGetTouchAnimation(RoomEnemyProjectileKind kind, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out Func<EnemyProjectileAnimation>? animation) =>
        _touchAnimations.TryGetValue(kind, out animation);

    /// <summary>Registers the typed animation a shot projectile of <paramref name="kind"/> starts.</summary>
    internal void RegisterShotAnimation(RoomEnemyProjectileKind kind, Func<EnemyProjectileAnimation> animation)
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
    internal void RegisterCue<TCue>(EnemyProjectileCueHandler<TCue> handler)
    {
        if (!_cues.TryAdd(typeof(TCue), Ensure.NotNull(handler)))
            throw new InvalidOperationException($"Enemy projectile cue {typeof(TCue).Name} is registered twice.");
    }

    /// <summary>Carries out <paramref name="cue"/> with its family's registered handler.</summary>
    internal void PublishCue<TCue>(RoomEnemyProjectileSlot projectile, SamusState? samus, TCue cue)
    {
        if (!_cues.TryGetValue(typeof(TCue), out Delegate? handler))
            throw new InvalidOperationException($"Enemy projectile cue {typeof(TCue).Name}.{cue} has no registered handler.");
        ((EnemyProjectileCueHandler<TCue>)handler)(projectile, samus, cue);
    }

    /// <summary>The handler registered for <paramref name="id"/>, if any.</summary>
    internal bool TryGetPreInstruction(EnemyProjectilePreInstruction id, [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out EnemyProjectilePreInstructionHandler? handler) =>
        _preInstructions.TryGetValue(id, out handler);
}
