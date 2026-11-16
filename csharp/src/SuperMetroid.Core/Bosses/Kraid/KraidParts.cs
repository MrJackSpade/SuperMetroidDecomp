using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's eight parts in his room, which he holds by reference: his body, his arm, his top, middle and bottom lint, his
/// foot and his two fingernails. Each part joins as the part its definition names when it initializes (#1877), and
/// the fight reaches it through that. A part's reference outlives the part's occupant: a
/// deleted part leaves its last values there and a killed part is emptied, so a read of a part that has been killed
/// finds what the kill left (<see cref="RoomEnemyPlace.FoundState{T}"/>).
/// </summary>
[RoomLifetime]
internal sealed class KraidParts
{
    /// <summary>Each part that has initialized, by the part its definition names.</summary>
    private readonly Dictionary<KraidPart, RoomEnemyPlace> _joined = [];

    /// <summary>
    /// Joins <paramref name="place"/> to Kraid as his <paramref name="part"/>, as it initializes. A part that initializes
    /// again, as a respawned fingernail does, joins the same enemy again.
    /// </summary>
    /// <exception cref="InvalidDataException">Another enemy already initialized as <paramref name="part"/>.</exception>
    internal void Join(KraidPart part, RoomEnemyPlace place)
    {
        Ensure.NotNull(place);
        if (_joined.TryGetValue(part, out RoomEnemyPlace? held) && !ReferenceEquals(held, place))
            throw new InvalidDataException($"Kraid's {part} initialized as enemy {place.Ordinal}, but enemy {held.Ordinal} already is.");
        _joined[part] = place;
    }

    /// <summary>The enemy that joined as <paramref name="part"/>.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="part"/> has not initialized.</exception>
    private RoomEnemyPlace Joined(KraidPart part) =>
        _joined.TryGetValue(part, out RoomEnemyPlace? place)
            ? place
            : throw new InvalidOperationException($"Kraid's {part} is reached before it initialized.");

    /// <summary>His body, which runs first.</summary>
    internal RoomEnemyPlace Body => Joined(KraidPart.Kraid);

    /// <summary>His arm.</summary>
    internal RoomEnemyPlace Arm => Joined(KraidPart.Arm);

    /// <summary>His top lint.</summary>
    internal RoomEnemyPlace TopLint => Joined(KraidPart.LintTop);

    /// <summary>His middle lint.</summary>
    internal RoomEnemyPlace MiddleLint => Joined(KraidPart.LintMiddle);

    /// <summary>His bottom lint.</summary>
    internal RoomEnemyPlace BottomLint => Joined(KraidPart.LintBottom);

    /// <summary>His foot.</summary>
    internal RoomEnemyPlace Foot => Joined(KraidPart.Foot);

    /// <summary>His first fingernail.</summary>
    internal RoomEnemyPlace FirstNail => Joined(KraidPart.FirstNail);

    /// <summary>His second fingernail, which runs last.</summary>
    internal RoomEnemyPlace SecondNail => Joined(KraidPart.SecondNail);

    /// <summary>Which of Kraid's parts <paramref name="place"/> is.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="place"/> has not initialized as one of his parts.</exception>
    internal KraidPart PartAt(RoomEnemyPlace place)
    {
        foreach ((KraidPart part, RoomEnemyPlace joined) in _joined)
        {
            if (ReferenceEquals(joined, place))
                return part;
        }
        throw new InvalidOperationException($"Enemy {place.Ordinal} is not one of Kraid's parts.");
    }

    /// <summary>The state <paramref name="state"/> keeps for whichever of Kraid's parts <paramref name="place"/> is.</summary>
    internal KraidPartState StateOf(KraidEnemyState state, RoomEnemyPlace place) => state.Part(PartAt(place));

    /// <summary>The state <paramref name="state"/> keeps for whichever of Kraid's lints <paramref name="place"/> is.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="place"/> is not one of his lints.</exception>
    internal KraidLintState LintStateOf(KraidEnemyState state, RoomEnemyPlace place) =>
        ReferenceEquals(place, TopLint) ? state.TopLintState
        : ReferenceEquals(place, MiddleLint) ? state.MiddleLintState
        : ReferenceEquals(place, BottomLint) ? state.BottomLintState
        : throw new InvalidOperationException($"Enemy {place.Ordinal} is not one of Kraid's lints.");

    /// <summary>The state <paramref name="state"/> keeps for whichever of Kraid's fingernails <paramref name="place"/> is.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="place"/> is not one of his fingernails.</exception>
    internal KraidNailState NailStateOf(KraidEnemyState state, RoomEnemyPlace place) =>
        ReferenceEquals(place, FirstNail) ? state.FirstNailState
        : ReferenceEquals(place, SecondNail) ? state.SecondNailState
        : throw new InvalidOperationException($"Enemy {place.Ordinal} is not one of Kraid's fingernails.");

    /// <summary>His lint part <paramref name="part"/>.</summary>
    internal RoomEnemyPlace Lint(KraidLintPart part) => part switch
    {
        KraidLintPart.Top => TopLint,
        KraidLintPart.Middle => MiddleLint,
        KraidLintPart.Bottom => BottomLint,
        _ => throw new InvalidOperationException($"Undefined {nameof(KraidLintPart)} {part}."),
    };

    /// <summary>His three lints, top first.</summary>
    internal IEnumerable<RoomEnemyPlace> Lints
    {
        get
        {
            yield return TopLint;
            yield return MiddleLint;
            yield return BottomLint;
        }
    }

    /// <summary>His two fingernails, first first.</summary>
    internal IEnumerable<RoomEnemyPlace> Nails
    {
        get
        {
            yield return FirstNail;
            yield return SecondNail;
        }
    }

    /// <summary>His body, arm, lints and foot, in running order: every part but the fingernails.</summary>
    internal IEnumerable<RoomEnemyPlace> AllButNails
    {
        get
        {
            yield return Body;
            yield return Arm;
            foreach (RoomEnemyPlace lint in Lints)
                yield return lint;
            yield return Foot;
        }
    }

    /// <summary>All eight parts in running order.</summary>
    internal IEnumerable<RoomEnemyPlace> All
    {
        get
        {
            foreach (RoomEnemyPlace part in AllButNails)
                yield return part;
            foreach (RoomEnemyPlace nail in Nails)
                yield return nail;
        }
    }

}
