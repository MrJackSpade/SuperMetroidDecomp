namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// The colours the Chainsaw beam (Spazer, Plasma and Wave together) is drawn in (#1318, #1500). The beam palette loader
/// picks a combination's palette by indexing a table of palette places past its twelve entries for this combination, and
/// the place it reads lies just before the start of Samus's animation code: the first colour's low byte comes from an
/// unconnected region, which answers with the last byte the processor fetched, and the rest are that code's instruction
/// bytes read as colours. The import pipeline decodes the sixteen colours once.
/// </summary>
/// <remarks>
/// Consulted whenever the equipped beams' palette is loaded with the Chainsaw combination equipped: on equipping it in the
/// pause screen, on entering a room, and when composing the Chainsaw beam's projectile sheet at installation. Evidence:
/// the Chainsaw beam graphics check decodes the original palette read, open-bus byte and instruction bytes, and compares
/// every colour with this service (owner decision on #1318, row 2).
/// </remarks>
/// <typeparam name="TColour">A colour.</typeparam>
[CompatibilityService("the Chainsaw beam's palette is read from the code past the beam palette table")]
public sealed class ChainsawBeamColours<TColour>
{
    /// <summary>The number of colours a beam palette holds.</summary>
    public const int ColourCount = 16;

    /// <summary>Keeps the decoded colours, in palette order.</summary>
    /// <param name="colours">The sixteen colours the Chainsaw beam's palette holds, in palette order.</param>
    /// <exception cref="ArgumentException">The palette does not hold sixteen colours.</exception>
    public ChainsawBeamColours(IReadOnlyList<TColour> colours)
    {
        Ensure.NotNull(colours);
        if (colours.Count != ColourCount)
            throw new ArgumentException($"The Chainsaw beam's palette holds {ColourCount} colours, not {colours.Count}.", nameof(colours));
        Colours = colours;
    }

    /// <summary>The sixteen colours the Chainsaw beam's palette holds, in palette order.</summary>
    public IReadOnlyList<TColour> Colours { get; }
}
