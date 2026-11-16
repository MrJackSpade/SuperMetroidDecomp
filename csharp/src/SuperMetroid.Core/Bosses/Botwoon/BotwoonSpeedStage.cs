namespace SuperMetroid.Core.Bosses.Botwoon;

/// <summary>
/// One of Botwoon's three speed stages (#2266): how fast his head moves and spits and how far apart his body segments
/// trail it. He refreshes his stage from his health each time he chooses a walk outside a hole.
/// </summary>
public sealed class BotwoonSpeedStage
{
    /// <summary>Defines a stage.</summary>
    /// <param name="movementSpeed">Pixels per direct move toward a hole, or path steps taken per authored-path update.</param>
    /// <param name="segmentSpacingEntries">How many head-history entries apart successive body segments trail.</param>
    /// <param name="spitSpeed">The speed his spits fly at.</param>
    /// <param name="name">The stage's name.</param>
    internal BotwoonSpeedStage(int movementSpeed, int segmentSpacingEntries, int spitSpeed, string name)
    {
        MovementSpeed = movementSpeed;
        SegmentSpacingEntries = segmentSpacingEntries;
        SpitSpeed = spitSpeed;
        Name = name;
    }

    /// <summary>Pixels per direct move toward a hole, or path steps taken per authored-path update.</summary>
    public int MovementSpeed { get; }

    /// <summary>How many head-history entries apart successive body segments trail.</summary>
    public int SegmentSpacingEntries { get; }

    /// <summary>The speed his spits fly at.</summary>
    public int SpitSpeed { get; }

    /// <summary>The stage's name.</summary>
    public string Name { get; }

    /// <inheritdoc/>
    public override string ToString() => Name;
}
