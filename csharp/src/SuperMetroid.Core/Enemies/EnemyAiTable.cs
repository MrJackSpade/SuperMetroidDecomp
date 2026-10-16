namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The enemy AI dispatch table. A native routine pointer can be shared by several definitions, so each routine
/// keeps an ordered list of definition-guarded handlers; an unguarded handler must be the last for its routine.
/// </summary>
internal sealed class EnemyAiTable
{
    /// <summary>Initialization handlers grouped by native routine, each with an optional enemy-definition guard.</summary>
    private readonly Dictionary<EnemyAiRoutine, List<(Func<RoomEnemySlot, bool>? Applies, EnemyInitializationAiHandler Handler)>> _initialization = [];
    /// <summary>Main AI handlers grouped by native routine in registration order.</summary>
    private readonly Dictionary<EnemyAiRoutine, List<(Func<RoomEnemySlot, bool>? Applies, EnemyMainAiHandler Handler)>> _main = [];
    /// <summary>Owners whose enemies run typed animations (#641).</summary>
    private readonly List<(Func<RoomEnemySlot, bool> Matches, Action<RoomEnemySlot, EnemyInstructionContext> Advance)> _typedAnimations = [];

    /// <summary>Registers a typed animation that advances matching enemies.</summary>
    internal void AddTypedAnimation(Func<RoomEnemySlot, bool> matches, Action<RoomEnemySlot, EnemyInstructionContext> advance) =>
        _typedAnimations.Add((Ensure.NotNull(matches), Ensure.NotNull(advance)));

    /// <summary>The routine a banked header pointer names, or null when this build does not translate it.</summary>
    /// <param name="bank">The header's AI bank.</param>
    /// <param name="pointer">The bank-local routine pointer.</param>
    internal static EnemyAiRoutine? Routine(byte bank, ushort pointer)
    {
        var routine = (EnemyAiRoutine)(int)new SnesAddress(bank, pointer);
        return Enum.IsDefined(routine) ? routine : null;
    }

    /// <summary>The first registered typed animation whose owner matches <paramref name="slot"/>, or null.</summary>
    internal Action<RoomEnemySlot, EnemyInstructionContext>? FindTypedAnimation(RoomEnemySlot slot)
    {
        foreach ((Func<RoomEnemySlot, bool> matches, Action<RoomEnemySlot, EnemyInstructionContext> advance) in _typedAnimations)
        {
            if (matches(slot))
                return advance;
        }
        return null;
    }

    /// <summary>Species-specific hurt handlers, with first matching entry taking precedence.</summary>
    private readonly List<(Func<RoomEnemySlot, bool> Matches, EnemyHurtAiHandler Handler)> _hurt = [];
    /// <summary>Frozen private tails that may consume an enemy's frame before common thaw.</summary>
    private readonly List<(Func<RoomEnemySlot, bool> Matches, EnemyFrozenPreemption Preemption)> _frozenPreemptions = [];
    /// <summary>Frozen AI callbacks run after the common thaw step.</summary>
    private readonly List<(Func<RoomEnemySlot, bool> Matches, Action<RoomEnemySlot> Handler)> _frozen = [];

    /// <summary>Adds a hurt AI; the first registered hurt AI whose match accepts the enemy runs.</summary>
    internal void AddHurt(Func<RoomEnemySlot, bool> matches, EnemyHurtAiHandler handler) =>
        _hurt.Add((Ensure.NotNull(matches), Ensure.NotNull(handler)));

    /// <summary>The hurt AI of <paramref name="slot"/>, or null when it has none.</summary>
    internal EnemyHurtAiHandler? FindHurt(RoomEnemySlot slot)
    {
        foreach (var (matches, handler) in _hurt)
        {
            if (matches(slot))
                return handler;
        }
        return null;
    }

    /// <summary>Adds a frozen enemy's private tail; the first registered tail whose match accepts the enemy decides.</summary>
    internal void AddFrozenPreemption(Func<RoomEnemySlot, bool> matches, EnemyFrozenPreemption preemption) =>
        _frozenPreemptions.Add((Ensure.NotNull(matches), Ensure.NotNull(preemption)));

    /// <summary>The frozen enemy's private tail, if it has one; when it returns true it consumed the slot's frame.</summary>
    internal EnemyFrozenPreemption? FindFrozenPreemption(RoomEnemySlot slot)
    {
        foreach (var (matches, preemption) in _frozenPreemptions)
        {
            if (matches(slot))
                return preemption;
        }
        return null;
    }

    /// <summary>Adds an AI that runs for a frozen enemy after the common thaw step.</summary>
    internal void AddFrozen(Func<RoomEnemySlot, bool> matches, Action<RoomEnemySlot> handler) =>
        _frozen.Add((Ensure.NotNull(matches), Ensure.NotNull(handler)));

    /// <summary>Every frozen AI whose match accepts the enemy, in registration order.</summary>
    internal IReadOnlyList<Action<RoomEnemySlot>> FrozenHandlers(RoomEnemySlot slot) =>
        [.. _frozen.Where(entry => entry.Matches(slot)).Select(entry => entry.Handler)];

    /// <summary>Touch reactions selected by the first matching enemy predicate.</summary>
    private readonly List<(Func<RoomEnemySlot, bool> Matches, EnemyTouchHandler Handler)> _touch = [];

    /// <summary>Adds a touch reaction; the first registered reaction whose match accepts the enemy runs.</summary>
    internal void AddTouch(Func<RoomEnemySlot, bool> matches, EnemyTouchHandler handler) =>
        _touch.Add((Ensure.NotNull(matches), Ensure.NotNull(handler)));

    /// <summary>The translated touch reaction of <paramref name="slot"/>, or null when it has none.</summary>
    internal EnemyTouchHandler? FindTouch(RoomEnemySlot slot)
    {
        foreach (var (matches, handler) in _touch)
        {
            if (matches(slot))
                return handler;
        }
        return null;
    }

    /// <summary>Shot reactions selected by the first matching enemy predicate.</summary>
    private readonly List<EnemyShotReaction> _shot = [];

    /// <summary>Adds a shot reaction; the first registered reaction that matches the enemy runs.</summary>
    internal void AddShot(EnemyShotReaction reaction) => _shot.Add(Ensure.NotNull(reaction));

    /// <summary>The translated shot reaction of <paramref name="enemy"/>, or null when it has none.</summary>
    internal EnemyShotReaction? FindShot(RoomEnemySlot enemy)
    {
        foreach (EnemyShotReaction reaction in _shot)
        {
            if (reaction.Matches(enemy))
                return reaction;
        }
        return null;
    }

    /// <summary>Private bomb callbacks checked before the selected literal no-op gate.</summary>
    private readonly List<(Func<EnemyBombContext, bool> Matches, Action<EnemyBombContext> Handler)> _bombSpecials = [];
    /// <summary>Private bomb callbacks checked after the selected literal no-op gate.</summary>
    private readonly List<(Func<EnemyBombContext, bool> Matches, Action<EnemyBombContext> Handler)> _bombSpecialsAfterNoOp = [];
    /// <summary>Species reactions that surround common bomb damage.</summary>
    private readonly List<EnemyBombReaction> _bombCommon = [];

    /// <summary>Adds a private bomb callback consulted before the selected literal no-op gate.</summary>
    internal void AddBombSpecial(Func<EnemyBombContext, bool> matches, Action<EnemyBombContext> handler) =>
        _bombSpecials.Add((Ensure.NotNull(matches), Ensure.NotNull(handler)));

    /// <summary>Adds a private bomb callback consulted after the selected literal no-op gate.</summary>
    internal void AddBombSpecialAfterNoOp(Func<EnemyBombContext, bool> matches, Action<EnemyBombContext> handler) =>
        _bombSpecialsAfterNoOp.Add((Ensure.NotNull(matches), Ensure.NotNull(handler)));

    /// <summary>Adds a species reaction around common bomb damage.</summary>
    internal void AddBombCommon(EnemyBombReaction reaction) => _bombCommon.Add(Ensure.NotNull(reaction));

    /// <summary>The first private bomb callback before the no-op gate that matches, or null.</summary>
    internal Action<EnemyBombContext>? FindBombSpecial(EnemyBombContext context) => Find(_bombSpecials, context);

    /// <summary>The first private bomb callback after the no-op gate that matches, or null.</summary>
    internal Action<EnemyBombContext>? FindBombSpecialAfterNoOp(EnemyBombContext context) => Find(_bombSpecialsAfterNoOp, context);

    /// <summary>The species reaction around common bomb damage, or null for the plain common callback.</summary>
    internal EnemyBombReaction? FindBombCommon(EnemyBombContext context)
    {
        foreach (EnemyBombReaction reaction in _bombCommon)
        {
            if (reaction.Matches(context))
                return reaction;
        }
        return null;
    }

    /// <summary>Returns the first bomb callback whose predicate accepts the current collision context.</summary>
    private static Action<EnemyBombContext>? Find(
        List<(Func<EnemyBombContext, bool> Matches, Action<EnemyBombContext> Handler)> entries, EnemyBombContext context)
    {
        foreach (var (matches, handler) in entries)
        {
            if (matches(context))
                return handler;
        }
        return null;
    }

    /// <summary>Power Bomb reactions selected by the first matching enemy predicate.</summary>
    private readonly List<EnemyPowerBombReaction> _powerBomb = [];

    /// <summary>Adds a power-bomb reaction; the first registered reaction that matches the enemy runs.</summary>
    internal void AddPowerBomb(EnemyPowerBombReaction reaction) => _powerBomb.Add(Ensure.NotNull(reaction));

    /// <summary>The translated power-bomb reaction of <paramref name="enemy"/>, or null when it has none.</summary>
    internal EnemyPowerBombReaction? FindPowerBomb(RoomEnemySlot enemy)
    {
        foreach (EnemyPowerBombReaction reaction in _powerBomb)
        {
            if (reaction.Matches(enemy))
                return reaction;
        }
        return null;
    }

    /// <summary>Registers an initialization routine handler.</summary>
    internal void RegisterInitialization(EnemyAiRoutine routine, Func<RoomEnemySlot, bool>? applies, EnemyInitializationAiHandler handler) =>
        Add(_initialization, routine, applies, handler);

    /// <summary>Registers a main AI routine handler.</summary>
    internal void RegisterMain(EnemyAiRoutine routine, Func<RoomEnemySlot, bool>? applies, EnemyMainAiHandler handler) =>
        Add(_main, routine, applies, handler);

    /// <summary>The first registered initialization handler that applies to <paramref name="slot"/>.</summary>
    internal bool TryGetInitialization(EnemyAiRoutine routine, RoomEnemySlot slot,
        [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out EnemyInitializationAiHandler? handler) =>
        TryGet(_initialization, routine, slot, out handler);

    /// <summary>The first registered main handler that applies to <paramref name="slot"/>.</summary>
    internal bool TryGetMain(EnemyAiRoutine routine, RoomEnemySlot slot,
        [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out EnemyMainAiHandler? handler) =>
        TryGet(_main, routine, slot, out handler);

    /// <summary>Appends a routine handler while ensuring an unconditional handler stays last.</summary>
    private static void Add<THandler>(Dictionary<EnemyAiRoutine, List<(Func<RoomEnemySlot, bool>? Applies, THandler Handler)>> table,
        EnemyAiRoutine routine, Func<RoomEnemySlot, bool>? applies, THandler handler) where THandler : Delegate
    {
        Ensure.NotNull(handler);
        if (!table.TryGetValue(routine, out var entries))
            table[routine] = entries = [];
        if (entries.Count > 0 && entries[^1].Applies is null)
            throw new InvalidOperationException($"Enemy AI routine {routine} already has an unconditional handler.");
        entries.Add((applies, handler));
    }

    /// <summary>Finds the first routine handler whose optional guard accepts this enemy slot.</summary>
    private static bool TryGet<THandler>(Dictionary<EnemyAiRoutine, List<(Func<RoomEnemySlot, bool>? Applies, THandler Handler)>> table,
        EnemyAiRoutine routine, RoomEnemySlot slot,
        [System.Diagnostics.CodeAnalysis.NotNullWhen(true)] out THandler? handler) where THandler : Delegate
    {
        handler = null;
        if (!table.TryGetValue(routine, out var entries))
            return false;
        foreach (var (applies, candidate) in entries)
        {
            if (applies is null || applies(slot))
            {
                handler = candidate;
                return true;
            }
        }
        return false;
    }
}
