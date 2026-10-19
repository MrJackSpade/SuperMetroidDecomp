using SuperMetroid.Core.Bosses.MotherBrain.Artwork;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>One of Mother Brain's body poses, an index into <see cref="MotherBrainBodyVisualDefinitions"/>.</summary>
/// <param name="Index">The pose's position in the body-frame catalog.</param>
public readonly record struct MotherBrainBodyFrame(int Index)
{
    /// <summary>Standing upright.</summary>
    internal static MotherBrainBodyFrame Standing => new(0);
    /// <summary>The crouched endpoint.</summary>
    internal static MotherBrainBodyFrame Crouched => new(9);
    /// <summary>Halfway between crouched and leaning down.</summary>
    internal static MotherBrainBodyFrame Uncrouching => new(10);
    /// <summary>Leaning down.</summary>
    internal static MotherBrainBodyFrame LeaningDown => new(11);
    /// <summary>The initial dummy's hitbox frame.</summary>
    internal static MotherBrainBodyFrame InitialDummy => new(12);

    /// <summary>Walking pose <paramref name="step"/> of eight.</summary>
    internal static MotherBrainBodyFrame Walking(int step) => new(1 + Ensure.BetweenInclusive(0, 7, step));

    /// <summary>Hand-beam pose <paramref name="step"/> of four.</summary>
    internal static MotherBrainBodyFrame HandBeam(int step) => new(13 + Ensure.BetweenInclusive(0, 3, step));

}
