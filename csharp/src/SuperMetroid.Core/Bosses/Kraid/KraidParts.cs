using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's eight parts in his room, which he holds by reference: his body, his arm, his top, middle and bottom lint, his
/// foot and his two fingernails. Each part joins its own reference when it initializes (#1877, #2146), and the fight
/// reaches it through that. A part's reference outlives the part's occupant: a deleted part leaves its last values there
/// and a killed part is emptied, so a read of a part that has been killed finds what the kill left
/// (<see cref="RoomEnemyPlace.FoundState{T}"/>).
/// </summary>
[RoomLifetime]
internal sealed class KraidParts
{
    /// <summary>His body's reference, which runs first.</summary>
    internal KraidPartReference BodyReference { get; } = new("body");

    /// <summary>His arm's reference.</summary>
    internal KraidPartReference ArmReference { get; } = new("arm");

    /// <summary>His top lint's reference.</summary>
    internal KraidPartReference TopLintReference { get; } = new("top lint");

    /// <summary>His middle lint's reference.</summary>
    internal KraidPartReference MiddleLintReference { get; } = new("middle lint");

    /// <summary>His bottom lint's reference.</summary>
    internal KraidPartReference BottomLintReference { get; } = new("bottom lint");

    /// <summary>His foot's reference.</summary>
    internal KraidPartReference FootReference { get; } = new("foot");

    /// <summary>His first fingernail's reference.</summary>
    internal KraidPartReference FirstNailReference { get; } = new("first fingernail");

    /// <summary>His second fingernail's reference, which runs last.</summary>
    internal KraidPartReference SecondNailReference { get; } = new("second fingernail");

    /// <summary>His body, which runs first.</summary>
    internal RoomEnemyPlace Body => BodyReference.Place;

    /// <summary>His arm.</summary>
    internal RoomEnemyPlace Arm => ArmReference.Place;

    /// <summary>His top lint.</summary>
    internal RoomEnemyPlace TopLint => TopLintReference.Place;

    /// <summary>His middle lint.</summary>
    internal RoomEnemyPlace MiddleLint => MiddleLintReference.Place;

    /// <summary>His bottom lint.</summary>
    internal RoomEnemyPlace BottomLint => BottomLintReference.Place;

    /// <summary>His foot.</summary>
    internal RoomEnemyPlace Foot => FootReference.Place;

    /// <summary>His first fingernail.</summary>
    internal RoomEnemyPlace FirstNail => FirstNailReference.Place;

    /// <summary>His second fingernail, which runs last.</summary>
    internal RoomEnemyPlace SecondNail => SecondNailReference.Place;

    /// <summary>The state <paramref name="state"/> keeps for whichever of Kraid's parts <paramref name="place"/> is.</summary>
    /// <exception cref="InvalidOperationException"><paramref name="place"/> has not initialized as one of his parts.</exception>
    internal KraidPartState StateOf(KraidEnemyState state, RoomEnemyPlace place) =>
        BodyReference.Holds(place) ? state.BodyState
        : ArmReference.Holds(place) ? state.ArmState
        : TopLintReference.Holds(place) ? state.TopLintState
        : MiddleLintReference.Holds(place) ? state.MiddleLintState
        : BottomLintReference.Holds(place) ? state.BottomLintState
        : FootReference.Holds(place) ? state.FootState
        : FirstNailReference.Holds(place) ? state.FirstNailState
        : SecondNailReference.Holds(place) ? state.SecondNailState
        : throw new InvalidOperationException($"Enemy {place.Ordinal} is not one of Kraid's parts.");

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

    /// <summary>Places each lint at its height beside <paramref name="body"/> as Kraid thinks between attacks.</summary>
    internal void PlaceLintsAtBodyHeights(RoomEnemyPlace body)
    {
        foreach ((KraidLintPart part, int height) in KraidArenaDefinitions.LintHeights)
            Lint(part).Position = Lint(part).Position.WithWrappedWholeY(body.Position.Y.Whole + height);
    }

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
