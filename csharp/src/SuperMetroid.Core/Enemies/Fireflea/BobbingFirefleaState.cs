namespace SuperMetroid.Core.Enemies.Fireflea;

/// <summary>The flight of a Fireflea that bobs up and down between two rows about where it started.</summary>
public sealed partial class BobbingFirefleaState : FirefleaFlightState
{
    /// <inheritdoc/>
    internal override void React<TContext>(FirefleaFlightStateReactions<TContext> reactions, TContext context) => Ensure.NotNull(reactions).BobbingFirefleaState(this, context);

    /// <summary>Starts the bobbing flight its spawn settings chose, <paramref name="definition"/>.</summary>
    internal BobbingFirefleaState(FirefleaFlight.Bobbing definition) => Definition = Ensure.NotNull(definition);

    /// <summary>The bobbing flight its spawn settings chose (#2476): the speed it moves at and the way it starts.</summary>
    internal FirefleaFlight.Bobbing Definition { get; }

    /// <summary>Upper extent of the motion, the spawn Y minus the radius. Passing above it reverses the Fireflea's way after movement.</summary>
    public int MinimumY { get; internal set; }

    /// <summary>Lower extent of the motion, the spawn Y plus the radius. Reaching or passing it reverses the Fireflea's way without clamping.</summary>
    public int MaximumY { get; internal set; }

    /// <summary>Which way the Fireflea moves at its speed (#2369); it reverses at each extent.</summary>
    internal VerticalDirection Direction { get; set; }

    /// <summary>The Fireflea's vertical velocity: its <see cref="Definition"/>'s speed toward its <see cref="Direction"/>.</summary>
    internal SignedSixteenSixteen Velocity => Definition.Speed.Toward(Direction);
}
