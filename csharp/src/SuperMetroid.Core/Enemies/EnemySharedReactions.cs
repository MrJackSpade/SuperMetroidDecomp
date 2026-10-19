namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The reactions and AI-state handlers several species share, each list first-match in registration order: hurt, frozen,
/// touch, shot, bomb and Power Bomb. Species register into it once per enemy system; the common participation answers
/// from it for every participation an entity does not own.
/// </summary>
internal sealed class EnemySharedReactions
{
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
}
