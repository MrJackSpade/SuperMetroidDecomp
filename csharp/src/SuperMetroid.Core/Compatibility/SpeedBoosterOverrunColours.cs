namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The suit colours Gravity Speed Booster copies from past its cycle (#1411, #1474). Speed Booster's cycle step is shared
/// with Screw Attack, so Gravity Speed Booster can inherit a step past the end of its own list; the original game then
/// reads the next words as palette places and copies sixteen colours from there. One place lies in an unconnected region
/// that yields black; the other is the Grapple handler's code. The import pipeline decodes both colour runs once.
/// </summary>
/// <remarks>
/// Consulted when the active Speed Booster palette timer expires on one of the two inherited steps, during Samus's
/// palette update. Evidence: the Speed Booster overrun check, which decodes the original reads and compares them with this
/// service, and the full-body palette pointer check.
/// </remarks>
/// <typeparam name="TPhase">The inherited step past the cycle.</typeparam>
/// <typeparam name="TColour">A suit colour.</typeparam>
[CompatibilityService("Gravity Speed Booster's steps past its cycle copy colours from what follows the cycle list")]
public sealed class SpeedBoosterOverrunColours<TPhase, TColour>
    where TPhase : notnull
{
    /// <summary>The colours each inherited step copies, in suit palette order.</summary>
    private readonly IReadOnlyDictionary<TPhase, IReadOnlyList<TColour>> _colours;

    /// <summary>Keeps the decoded colours of every inherited step.</summary>
    /// <param name="colours">The colours each inherited step copies, in suit palette order.</param>
    public SpeedBoosterOverrunColours(IReadOnlyDictionary<TPhase, IReadOnlyList<TColour>> colours) =>
        _colours = Ensure.NotNull(colours);

    /// <summary>The colours step <paramref name="phase"/> copies into the suit, in suit palette order.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The installation decoded no colours for the step.</exception>
    public IReadOnlyList<TColour> Colours(TPhase phase) =>
        _colours.TryGetValue(phase, out IReadOnlyList<TColour>? colours)
            ? colours
            : throw new ArgumentOutOfRangeException(nameof(phase), phase, "No colours were decoded for this Speed Booster step.");
}
