using Engine.Shared;

namespace Engine.World;

/// <summary>
/// Moving along a set path: a list of points visited in order at a set speed. At the last point the path either ends,
/// starts again from the first, or is walked back the way it came. Platforms, carriers and anything else that follows
/// a path share this.
/// </summary>
public sealed class PathFollower
{
    /// <summary>What happens at the last point of a path.</summary>
    public enum AtEnd
    {
        /// <summary>The mover stops there.</summary>
        Stops,

        /// <summary>The mover goes straight on to the first point again.</summary>
        Loops,

        /// <summary>The mover walks the path back the way it came, and so on.</summary>
        Returns,
    }

    private readonly IReadOnlyList<PixelPosition> _points;
    private Pixels _speed;
    private readonly AtEnd _atEnd;
    private int _next = 1;
    private int _direction = 1;

    /// <summary>A follower of <paramref name="points"/>, starting at the first, moving <paramref name="speed"/> each update.</summary>
    /// <exception cref="ArgumentException">The path has fewer than two points, or the speed is not positive.</exception>
    public PathFollower(IReadOnlyList<PixelPosition> points, Pixels speed, AtEnd atEnd)
    {
        if (points.Count < 2)
            throw new ArgumentException("A path has at least two points.", nameof(points));
        if (speed <= Pixels.Zero)
            throw new ArgumentException("A path is followed at a speed above nothing.", nameof(speed));
        _points = points;
        _speed = speed;
        _atEnd = atEnd;
        Position = points[0];
    }

    /// <summary>How far the mover goes each update.</summary>
    /// <exception cref="ArgumentException">The speed is not positive.</exception>
    public Pixels Speed
    {
        get => _speed;
        set => _speed = value > Pixels.Zero ? value : throw new ArgumentException("A path is followed at a speed above nothing.", nameof(value));
    }

    /// <summary>Where the mover is on the path.</summary>
    public PixelPosition Position { get; private set; }

    /// <summary>Whether a path that stops at its end has been walked to its end.</summary>
    public bool IsFinished { get; private set; }

    /// <summary>Moves one update along the path and returns how far that moved the mover.</summary>
    public PixelOffset Step()
    {
        if (IsFinished)
            return PixelOffset.Zero;
        PixelPosition before = Position;
        Pixels left = _speed;
        while (left > Pixels.Zero && !IsFinished)
        {
            PixelPosition target = _points[_next];
            PixelOffset toTarget = target - Position;
            // Paths run along one axis at a time; a diagonal leg is walked across first, then down.
            if (!toTarget.X.IsZero)
            {
                Pixels moved = toTarget.X.Abs() < left ? toTarget.X.Abs() : left;
                Position = Position with { X = Position.X + (toTarget.X.IsNegative ? -moved : moved) };
                left -= moved;
            }
            else if (!toTarget.Y.IsZero)
            {
                Pixels moved = toTarget.Y.Abs() < left ? toTarget.Y.Abs() : left;
                Position = Position with { Y = Position.Y + (toTarget.Y.IsNegative ? -moved : moved) };
                left -= moved;
            }

            if (Position == target)
                Advance();
        }

        return Position - before;
    }

    /// <summary>Chooses the point to head for after reaching one.</summary>
    private void Advance()
    {
        int following = _next + _direction;
        if (following >= 0 && following < _points.Count)
        {
            _next = following;
            return;
        }

        switch (_atEnd)
        {
            case AtEnd.Stops:
                IsFinished = true;
                break;
            case AtEnd.Loops:
                _next = 0;
                break;
            case AtEnd.Returns:
                _direction = -_direction;
                _next += _direction;
                break;
            default:
                throw new InvalidOperationException($"Undefined path ending {_atEnd}.");
        }
    }
}
