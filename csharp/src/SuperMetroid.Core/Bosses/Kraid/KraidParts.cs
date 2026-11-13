using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's eight parts in his room, which he holds by reference: his body, his arm, his top, middle and bottom lint, his
/// foot and his two fingernails. His room lists them in that order, the order the enemy pass runs them in, and he takes
/// each one once from its place in that listing, the first time the fight reaches his parts, once the room's enemies are
/// in place. A part's reference is its place in the room, which outlives the part's occupant: a deleted part leaves its
/// last values there and a killed part is emptied, so a read of a part that has been killed goes through the rule that
/// states what the fight reads of it (<see cref="KilledFingernailStops"/>).
/// </summary>
[RoomLifetime]
internal sealed class KraidParts
{
    /// <summary>His body's place in his room's enemy list.</summary>
    private const int BodyPlace = 0;
    /// <summary>His arm's place, after his body.</summary>
    private const int ArmPlace = 1;
    /// <summary>His top lint's place, after his arm.</summary>
    private const int TopLintPlace = 2;
    /// <summary>His middle lint's place, after his top lint.</summary>
    private const int MiddleLintPlace = 3;
    /// <summary>His bottom lint's place, after his middle lint.</summary>
    private const int BottomLintPlace = 4;
    /// <summary>His foot's place, after his bottom lint.</summary>
    private const int FootPlace = 5;
    /// <summary>His first fingernail's place, after his foot.</summary>
    private const int FirstNailPlace = 6;
    /// <summary>His second and last fingernail's place, after his first.</summary>
    private const int SecondNailPlace = 7;

    /// <summary>The room's enemies, whose first eight are Kraid's parts.</summary>
    private readonly EnemySlotTable _enemies;

    /// <summary>His parts, taken once the room's enemies are in place; null until the fight first reaches them.</summary>
    private Held? _held;

    /// <summary>Reaches Kraid's parts among <paramref name="enemies"/>.</summary>
    internal KraidParts(EnemySlotTable enemies)
    {
        _enemies = Ensure.NotNull(enemies);
    }

    /// <summary>His eight parts, each the place his room lists it in.</summary>
    /// <param name="Body">His body.</param>
    /// <param name="Arm">His arm.</param>
    /// <param name="TopLint">His top lint.</param>
    /// <param name="MiddleLint">His middle lint.</param>
    /// <param name="BottomLint">His bottom lint.</param>
    /// <param name="Foot">His foot.</param>
    /// <param name="FirstNail">His first fingernail.</param>
    /// <param name="SecondNail">His second fingernail.</param>
    [ArchitectureReview("context-bag", "Kraid's eight parts, each the enemy place his room lists it in; the members are his body's parts, not services.")]
    private sealed record Held(
        RoomEnemySlot Body, RoomEnemySlot Arm, RoomEnemySlot TopLint, RoomEnemySlot MiddleLint, RoomEnemySlot BottomLint,
        RoomEnemySlot Foot, RoomEnemySlot FirstNail, RoomEnemySlot SecondNail);

    /// <summary>His parts, taken from the room's listing the first time they are reached.</summary>
    private Held Parts => _held ??= new(
        _enemies.Slots[BodyPlace], _enemies.Slots[ArmPlace], _enemies.Slots[TopLintPlace], _enemies.Slots[MiddleLintPlace],
        _enemies.Slots[BottomLintPlace], _enemies.Slots[FootPlace], _enemies.Slots[FirstNailPlace], _enemies.Slots[SecondNailPlace]);

    /// <summary>His body, which runs first.</summary>
    internal RoomEnemySlot Body => Parts.Body;

    /// <summary>His arm.</summary>
    internal RoomEnemySlot Arm => Parts.Arm;

    /// <summary>His top lint.</summary>
    internal RoomEnemySlot TopLint => Parts.TopLint;

    /// <summary>His middle lint.</summary>
    internal RoomEnemySlot MiddleLint => Parts.MiddleLint;

    /// <summary>His bottom lint.</summary>
    internal RoomEnemySlot BottomLint => Parts.BottomLint;

    /// <summary>His foot.</summary>
    internal RoomEnemySlot Foot => Parts.Foot;

    /// <summary>His first fingernail.</summary>
    internal RoomEnemySlot FirstNail => Parts.FirstNail;

    /// <summary>His second fingernail, which runs last.</summary>
    internal RoomEnemySlot SecondNail => Parts.SecondNail;

    /// <summary>Which of Kraid's parts <paramref name="slot"/> is.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="slot"/> is not one of his parts' places.</exception>
    internal KraidPart PartAt(RoomEnemySlot slot) =>
        ReferenceEquals(slot, Body) ? KraidPart.Kraid
        : ReferenceEquals(slot, Arm) ? KraidPart.Arm
        : ReferenceEquals(slot, TopLint) ? KraidPart.LintTop
        : ReferenceEquals(slot, MiddleLint) ? KraidPart.LintMiddle
        : ReferenceEquals(slot, BottomLint) ? KraidPart.LintBottom
        : ReferenceEquals(slot, Foot) ? KraidPart.Foot
        : ReferenceEquals(slot, FirstNail) ? KraidPart.Nail
        : ReferenceEquals(slot, SecondNail) ? KraidPart.NailBad
        : throw new InvalidOperationException($"Enemy {slot.SlotIndex} is not one of Kraid's parts.");

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

    /// <summary>Rejects a part's initialization in any place but its own.</summary>
    /// <param name="slot">The enemy initializing as the part.</param>
    /// <param name="expected">The part's own place, from this type.</param>
    /// <param name="part">The part's name for the failure message.</param>
    /// <exception cref="InvalidDataException">The room lists <paramref name="part"/> in another place.</exception>
    internal static void Require(RoomEnemySlot slot, RoomEnemySlot expected, string part)
    {
        if (!ReferenceEquals(slot, expected))
            throw new InvalidDataException($"Kraid's {part} is listed in his room as enemy {slot.SlotIndex}, not {expected.SlotIndex}.");
    }
}
