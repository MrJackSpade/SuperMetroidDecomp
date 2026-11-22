using SuperMetroid.Core.Progression;
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
internal sealed partial class KraidParts
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

    /// <summary>
    /// The encounter his body began as it set up, which its state holds (#2218) and each of his other parts joins as it
    /// sets up (#2420).
    /// </summary>
    /// <exception cref="InvalidOperationException">His body has not set up.</exception>
    internal KraidEnemyState Encounter => Body.RequireSpeciesState<KraidPartState>().Kraid;

    /// <summary>
    /// Joins the part at <paramref name="place"/> to the encounter his body began, which it holds from here (#2218, #2557):
    /// in a room where he is defeated the part is marked dead and joins nothing; otherwise the encounter's part
    /// <paramref name="partOf"/> names is attached there and takes his body's shading. His arm, foot and lints set up so.
    /// </summary>
    /// <returns>The part joined, or null in a room where he is defeated.</returns>
    internal TPart? JoinEncounter<TPart>(EnemyRoomBinding room, RoomEnemyPlace place, Func<KraidEnemyState, TPart> partOf)
        where TPart : KraidPartState
    {
        if (Ensure.NotNull(room).Services.Bosses.IsDefeated(DefeatedBosses.AreaBoss))
        {
            KraidEncounterAccess.MarkKraidPartDead(place);
            return null;
        }
        TPart part = Ensure.NotNull(partOf)(Encounter);
        KraidEncounterAccess.AttachKraidPart(part);
        place.Shading = Body.Shading;
        return part;
    }

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
            Lint(part).Position = Lint(part).Position.WithWholeY(body.Position.Y.Whole + height);
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

    /// <summary>The fingernails last spawned in their places, which Kraid's death kills.</summary>
    internal IEnumerable<RoomEnemyOccupant> LatestNails
    {
        get
        {
            yield return FirstNailReference.LatestOccupant;
            yield return SecondNailReference.LatestOccupant;
        }
    }

    /// <summary>The lints last spawned in their places, which Kraid's death kills.</summary>
    internal IEnumerable<RoomEnemyOccupant> LatestLints
    {
        get
        {
            yield return TopLintReference.LatestOccupant;
            yield return MiddleLintReference.LatestOccupant;
            yield return BottomLintReference.LatestOccupant;
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
