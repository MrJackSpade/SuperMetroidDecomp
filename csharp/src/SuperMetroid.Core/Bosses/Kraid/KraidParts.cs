using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Kraid;

/// <summary>
/// Kraid's eight parts in his room, by place (#1877, #2146, #2554): his body, which heads the other seven, and the places
/// his room lists right after it, in order: his arm, his top, middle and bottom lint, his foot and his two fingernails. The
/// population load hands his body their places as it places it, and the fight reaches each part through them. A part's
/// place outlives the part's occupant: a deleted part leaves its last values there and a killed part is emptied, so a read
/// of a part that has been killed finds what the kill left (<see cref="RoomEnemyPlace.FoundEnemy"/>).
/// </summary>
internal sealed class KraidParts
{
    /// <summary>The places of his body and the seven parts his room lists right after it.</summary>
    /// <param name="body">His body's place.</param>
    /// <param name="parts">The places of his arm, top, middle and bottom lint, foot and first and second fingernail.</param>
    /// <exception cref="InvalidDataException">The room lists other than seven parts after his body.</exception>
    internal KraidParts(RoomEnemyPlace body, IReadOnlyList<RoomEnemyPlace> parts)
    {
        Body = Ensure.NotNull(body);
        Ensure.NotNull(parts);
        if (parts.Count != PartCount)
            throw new InvalidDataException($"Kraid heads {PartCount} parts, but {parts.Count} places were handed to him.");
        Arm = parts[0];
        TopLint = parts[1];
        MiddleLint = parts[2];
        BottomLint = parts[3];
        Foot = parts[4];
        FirstNail = parts[5];
        SecondNail = parts[6];
    }

    /// <summary>How many parts his room lists right after his body.</summary>
    internal const int PartCount = 7;

    /// <summary>His body, which runs first.</summary>
    internal RoomEnemyPlace Body { get; }

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
