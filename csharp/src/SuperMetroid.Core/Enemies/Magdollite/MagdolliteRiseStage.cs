using static SuperMetroid.Core.Enemies.Magdollite.MagdollitePhaseDefinitions;

namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// How far a Magdollite arm has grown (#2044): one of the nine stages of its rise, from the zero-distance setup through
/// its eighth pillar, each with its phase (<see cref="MagdollitePhaseDefinitions.Phases"/>). The arm grows a stage as it
/// rises past the next stage's threshold and loses one as it sinks back below its own; whether it grows a final stage
/// at full height is <see cref="MagdolliteApexGrowth"/>'s.
/// </summary>
public readonly record struct MagdolliteRiseStage
{
    /// <summary>The stage's place in the rise, the setup first.</summary>
    private readonly int _place;

    /// <summary>The stage at <paramref name="place"/> in the rise.</summary>
    /// <exception cref="InvalidDataException">The rise has no stage there.</exception>
    private MagdolliteRiseStage(int place) => _place = (uint)place < (uint)Phases.Length
        ? place
        : throw new InvalidDataException($"A Magdollite arm's rise has no stage past its {Phases.Length} authored phases or before its setup.");

    /// <summary>The zero-distance setup a dormant arm shows, the default stage.</summary>
    public static MagdolliteRiseStage Setup => default;

    /// <summary>The stage a waking arm starts its rise at: its first visible pillar.</summary>
    public static MagdolliteRiseStage FirstPillar => new(1);

    /// <summary>The stage's distance threshold, body animation and overlay offset.</summary>
    internal MagdollitePhaseDefinition Phase => Phases[_place];

    /// <summary>The next stage up the rise.</summary>
    /// <exception cref="InvalidDataException">This is the rise's last stage.</exception>
    public MagdolliteRiseStage Higher => new(_place + 1);

    /// <summary>The next stage down the rise.</summary>
    /// <exception cref="InvalidDataException">This is the setup.</exception>
    public MagdolliteRiseStage Lower => new(_place - 1);
}