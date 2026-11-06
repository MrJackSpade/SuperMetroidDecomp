using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's eight parts in his room. His room lists them in one order, which is the order the enemy pass runs them in:
/// his body, his arm, his top, middle and bottom lint, his foot, then his two fingernails. Every reference reads the
/// room's enemy in that part's place, whatever it currently holds, so a part that has died or been deleted is still
/// the one reached, as the fight expects.
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

    /// <summary>Reaches Kraid's parts among <paramref name="enemies"/>.</summary>
    internal KraidParts(EnemySlotTable enemies)
    {
        _enemies = enemies;
    }

    /// <summary>His body, which runs first.</summary>
    internal RoomEnemySlot Body => _enemies.Slots[BodyPlace];

    /// <summary>His arm.</summary>
    internal RoomEnemySlot Arm => _enemies.Slots[ArmPlace];

    /// <summary>His top lint.</summary>
    internal RoomEnemySlot TopLint => _enemies.Slots[TopLintPlace];

    /// <summary>His middle lint.</summary>
    internal RoomEnemySlot MiddleLint => _enemies.Slots[MiddleLintPlace];

    /// <summary>His bottom lint.</summary>
    internal RoomEnemySlot BottomLint => _enemies.Slots[BottomLintPlace];

    /// <summary>His foot.</summary>
    internal RoomEnemySlot Foot => _enemies.Slots[FootPlace];

    /// <summary>His first fingernail.</summary>
    internal RoomEnemySlot FirstNail => _enemies.Slots[FirstNailPlace];

    /// <summary>His second fingernail, which runs last.</summary>
    internal RoomEnemySlot SecondNail => _enemies.Slots[SecondNailPlace];

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
