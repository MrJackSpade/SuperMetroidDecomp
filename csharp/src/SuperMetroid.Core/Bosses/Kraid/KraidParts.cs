using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's eight parts' places in his room: his body, his arm, his top, middle and bottom lint, his foot and his two
/// fingernails, which his body finds as his room lists them after it when it sets up (<see cref="KraidPartListingDefinitions"/>,
/// #2554), as the game reaches them at fixed places after his body's. A place outlives its part's occupant: a deleted part
/// leaves its last values there and a killed part is emptied, so a read of a part that has been killed finds what the kill
/// left (<see cref="RoomEnemyPlace.FoundEntity"/>).
/// </summary>
internal sealed class KraidParts
{
    /// <summary>Finds the parts his room lists with <paramref name="body"/>.</summary>
    /// <param name="places">The room's enemy places.</param>
    /// <param name="body">His body's place.</param>
    /// <exception cref="InvalidDataException">The room lists no enemy where one of his parts belongs.</exception>
    internal KraidParts(EnemyPlaces places, RoomEnemyPlace body)
    {
        RoomEnemyPlace Part(EnemyDefinitionId part) =>
            Ensure.NotNull(places).Member(body, KraidPartListingDefinitions.Listing, EnemyDefinitionId.Kraid, part);
        Body = Ensure.NotNull(body);
        Arm = Part(EnemyDefinitionId.KraidArm);
        TopLint = Part(EnemyDefinitionId.KraidLintTop);
        MiddleLint = Part(EnemyDefinitionId.KraidLintMiddle);
        BottomLint = Part(EnemyDefinitionId.KraidLintBottom);
        Foot = Part(EnemyDefinitionId.KraidFoot);
        FirstNail = Part(EnemyDefinitionId.KraidFirstNail);
        SecondNail = Part(EnemyDefinitionId.KraidSecondNail);
    }

    /// <summary>His body, which runs first.</summary>
    internal RoomEnemyPlace Body { get; }

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
    internal RoomEnemyPlace Arm { get; }

    /// <summary>His top lint.</summary>
    internal RoomEnemyPlace TopLint { get; }

    /// <summary>His middle lint.</summary>
    internal RoomEnemyPlace MiddleLint { get; }

    /// <summary>His bottom lint.</summary>
    internal RoomEnemyPlace BottomLint { get; }

    /// <summary>His foot.</summary>
    internal RoomEnemyPlace Foot { get; }

    /// <summary>His first fingernail.</summary>
    internal RoomEnemyPlace FirstNail { get; }

    /// <summary>His second fingernail, which runs last.</summary>
    internal RoomEnemyPlace SecondNail { get; }

    /// <summary>His lint part <paramref name="part"/>.</summary>
    private RoomEnemyPlace Lint(KraidLintPart part) => part switch
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

    /// <summary>
    /// The fingernails last spawned in their places, which Kraid's death kills whether they hold their places or have left
    /// them; a place no fingernail was ever spawned in has none to kill.
    /// </summary>
    internal IEnumerable<RoomEnemyOccupant> LatestNails => LatestIn(Nails);

    /// <summary>The lints last spawned in their places, which Kraid's death kills.</summary>
    internal IEnumerable<RoomEnemyOccupant> LatestLints => LatestIn(Lints);

    /// <summary>The occupants last spawned in <paramref name="places"/>.</summary>
    private static IEnumerable<RoomEnemyOccupant> LatestIn(IEnumerable<RoomEnemyPlace> places)
    {
        foreach (RoomEnemyPlace place in places)
        {
            if (place.History is PopulatedEnemyPlace populated)
                yield return populated.LatestOccupant;
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
