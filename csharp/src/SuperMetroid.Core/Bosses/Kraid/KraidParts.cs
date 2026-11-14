using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's eight parts in his room, which he holds by reference: his body, his arm, his top, middle and bottom lint, his
/// foot and his two fingernails. Each part registers itself as the part its definition names when it initializes
/// (#1877), and the fight reaches it through that registration. A part's reference outlives the part's occupant: a
/// deleted part leaves its last values there and a killed part is emptied, so a read of a part that has been killed
/// goes through the rule that states what the fight reads of it (<see cref="KilledFingernailStops"/>).
/// </summary>
[RoomLifetime]
internal sealed class KraidParts
{
    /// <summary>Each part that has initialized, by the part its definition names.</summary>
    private readonly Dictionary<KraidPart, RoomEnemySlot> _registered = [];

    /// <summary>
    /// Registers <paramref name="slot"/> as Kraid's <paramref name="part"/>, as it initializes. A part that initializes
    /// again, as a respawned fingernail does, registers the same enemy again.
    /// </summary>
    /// <exception cref="InvalidDataException">Another enemy already initialized as <paramref name="part"/>.</exception>
    internal void Register(KraidPart part, RoomEnemySlot slot)
    {
        Ensure.NotNull(slot);
        if (_registered.TryGetValue(part, out RoomEnemySlot? held) && !ReferenceEquals(held, slot))
            throw new InvalidDataException($"Kraid's {part} initialized as enemy {slot.SlotIndex}, but enemy {held.SlotIndex} already is.");
        _registered[part] = slot;
    }

    /// <summary>The enemy registered as <paramref name="part"/>.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="part"/> has not initialized.</exception>
    private RoomEnemySlot Registered(KraidPart part) =>
        _registered.TryGetValue(part, out RoomEnemySlot? slot)
            ? slot
            : throw new InvalidOperationException($"Kraid's {part} is reached before it initialized.");

    /// <summary>His body, which runs first.</summary>
    internal RoomEnemySlot Body => Registered(KraidPart.Kraid);

    /// <summary>His arm.</summary>
    internal RoomEnemySlot Arm => Registered(KraidPart.Arm);

    /// <summary>His top lint.</summary>
    internal RoomEnemySlot TopLint => Registered(KraidPart.LintTop);

    /// <summary>His middle lint.</summary>
    internal RoomEnemySlot MiddleLint => Registered(KraidPart.LintMiddle);

    /// <summary>His bottom lint.</summary>
    internal RoomEnemySlot BottomLint => Registered(KraidPart.LintBottom);

    /// <summary>His foot.</summary>
    internal RoomEnemySlot Foot => Registered(KraidPart.Foot);

    /// <summary>His first fingernail.</summary>
    internal RoomEnemySlot FirstNail => Registered(KraidPart.Nail);

    /// <summary>His second fingernail, which runs last.</summary>
    internal RoomEnemySlot SecondNail => Registered(KraidPart.NailBad);

    /// <summary>Which of Kraid's parts <paramref name="slot"/> is.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="slot"/> has not initialized as one of his parts.</exception>
    internal KraidPart PartAt(RoomEnemySlot slot)
    {
        foreach ((KraidPart part, RoomEnemySlot registered) in _registered)
        {
            if (ReferenceEquals(registered, slot))
                return part;
        }
        throw new InvalidOperationException($"Enemy {slot.SlotIndex} is not one of Kraid's parts.");
    }

    /// <summary>The state <paramref name="state"/> keeps for whichever of Kraid's parts <paramref name="slot"/> is.</summary>
    internal KraidPartState StateOf(KraidEnemyState state, RoomEnemySlot slot) => state.Part(PartAt(slot));

    /// <summary>The state <paramref name="state"/> keeps for whichever of Kraid's lints <paramref name="slot"/> is.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="slot"/> is not one of his lints.</exception>
    internal KraidLintState LintStateOf(KraidEnemyState state, RoomEnemySlot slot) =>
        ReferenceEquals(slot, TopLint) ? state.TopLintState
        : ReferenceEquals(slot, MiddleLint) ? state.MiddleLintState
        : ReferenceEquals(slot, BottomLint) ? state.BottomLintState
        : throw new InvalidOperationException($"Enemy {slot.SlotIndex} is not one of Kraid's lints.");

    /// <summary>The state <paramref name="state"/> keeps for whichever of Kraid's fingernails <paramref name="slot"/> is.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="slot"/> is not one of his fingernails.</exception>
    internal KraidNailState NailStateOf(KraidEnemyState state, RoomEnemySlot slot) =>
        ReferenceEquals(slot, FirstNail) ? state.FirstNailState
        : ReferenceEquals(slot, SecondNail) ? state.SecondNailState
        : throw new InvalidOperationException($"Enemy {slot.SlotIndex} is not one of Kraid's fingernails.");

    /// <summary>His lint part <paramref name="part"/>.</summary>
    internal RoomEnemySlot Lint(KraidLintPart part) => part switch
    {
        KraidLintPart.Top => TopLint,
        KraidLintPart.Middle => MiddleLint,
        KraidLintPart.Bottom => BottomLint,
        _ => throw new InvalidOperationException($"Undefined {nameof(KraidLintPart)} {part}."),
    };

    /// <summary>His three lints, top first.</summary>
    internal IEnumerable<RoomEnemySlot> Lints
    {
        get
        {
            yield return TopLint;
            yield return MiddleLint;
            yield return BottomLint;
        }
    }

    /// <summary>His two fingernails, first first.</summary>
    internal IEnumerable<RoomEnemySlot> Nails
    {
        get
        {
            yield return FirstNail;
            yield return SecondNail;
        }
    }

    /// <summary>His body, arm, lints and foot, in running order: every part but the fingernails.</summary>
    internal IEnumerable<RoomEnemySlot> AllButNails
    {
        get
        {
            yield return Body;
            yield return Arm;
            foreach (RoomEnemySlot lint in Lints)
                yield return lint;
            yield return Foot;
        }
    }

    /// <summary>All eight parts in running order.</summary>
    internal IEnumerable<RoomEnemySlot> All
    {
        get
        {
            foreach (RoomEnemySlot part in AllButNails)
                yield return part;
            foreach (RoomEnemySlot nail in Nails)
                yield return nail;
        }
    }

}
