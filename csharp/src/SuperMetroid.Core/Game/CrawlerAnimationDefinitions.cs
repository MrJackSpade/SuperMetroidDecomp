namespace SuperMetroid.Core.Game;

/// <summary>Mutually exclusive crawler animation families with distinct initial tables.</summary>
internal enum CrawlerAnimationFamily
{
    Shared,
    Viola,
    Sciser,
    Zero,
    HZoomer,
}

/// <summary>The four physical surfaces represented by crawler animation lists.</summary>
internal enum CrawlerSurfaceOrientation : ushort
{
    UpsideRight = 0,
    UpsideLeft = 1,
    UpsideDown = 2,
    UpsideUp = 3,
}

/// <summary>Four surface-oriented instruction lists for one crawler family or species.</summary>
internal readonly record struct CrawlerAnimationDefinition(
    ushort UpsideRight,
    ushort UpsideLeft,
    ushort UpsideDown,
    ushort UpsideUp)
{
    internal ushort ForOrientation(CrawlerSurfaceOrientation orientation) => orientation switch
    {
        CrawlerSurfaceOrientation.UpsideRight => UpsideRight,
        CrawlerSurfaceOrientation.UpsideLeft => UpsideLeft,
        CrawlerSurfaceOrientation.UpsideDown => UpsideDown,
        CrawlerSurfaceOrientation.UpsideUp => UpsideUp,
        _ => throw new InvalidDataException(
            $"Crawler surface orientation {(ushort)orientation} exceeds four authored values."),
    };
}

/// <summary>Compiled initial and shared surface animation selectors for crawler enemies.</summary>
internal static class CrawlerAnimationDefinitions
{
    /// <summary>
    /// The four-word initial tables at <c>$A3:E2CC</c>, <c>$B667</c>, <c>$96DB</c>,
    /// <c>$992B</c>, and <c>$E03B</c>, indexed by <see cref="CrawlerAnimationFamily"/>.
    /// </summary>
    private static readonly CrawlerAnimationDefinition[] InitialFamilies =
    [
        new(0xe25c, 0xe278, 0xe294, 0xe2b0),
        new(0xb5e3, 0xb5eb, 0xb5d3, 0xb5db),
        new(
            SciserInstructionProgramDefinitions.UpsideRight,
            SciserInstructionProgramDefinitions.UpsideLeft,
            SciserInstructionProgramDefinitions.UpsideDown,
            SciserInstructionProgramDefinitions.UpsideUp),
        new(
            ZeroInstructionProgramDefinitions.UpsideRight,
            ZeroInstructionProgramDefinitions.UpsideLeft,
            ZeroInstructionProgramDefinitions.UpsideDown,
            ZeroInstructionProgramDefinitions.UpsideUp),
        new(
            HZoomerInstructionProgramDefinitions.UpsideRight,
            HZoomerInstructionProgramDefinitions.UpsideLeft,
            HZoomerInstructionProgramDefinitions.UpsideDown,
            HZoomerInstructionProgramDefinitions.UpsideUp),
    ];

    /// <summary>
    /// The six species rows transposed from the parallel tables at
    /// <c>$A3:E630-$E65F</c>. Rows zero through two intentionally share lists.
    /// </summary>
    private static readonly CrawlerAnimationDefinition[] SurfaceSpecies =
    [
        new(0xe25c, 0xe278, 0xe294, 0xe2b0),
        new(0xe25c, 0xe278, 0xe294, 0xe2b0),
        new(0xe25c, 0xe278, 0xe294, 0xe2b0),
        new(0xb5e3, 0xb5eb, 0xb5d3, 0xb5db),
        new(
            SciserInstructionProgramDefinitions.UpsideRight,
            SciserInstructionProgramDefinitions.UpsideLeft,
            SciserInstructionProgramDefinitions.UpsideDown,
            SciserInstructionProgramDefinitions.UpsideUp),
        new(
            ZeroInstructionProgramDefinitions.UpsideRight,
            ZeroInstructionProgramDefinitions.UpsideLeft,
            ZeroInstructionProgramDefinitions.UpsideDown,
            ZeroInstructionProgramDefinitions.UpsideUp),
    ];

    /// <summary>Returns one family-specific initial instruction list.</summary>
    internal static ushort InitialInstruction(
        CrawlerAnimationFamily family,
        CrawlerSurfaceOrientation orientation)
    {
        int index = (int)family;
        if ((uint)index >= InitialFamilies.Length)
        {
            throw new InvalidDataException(
                $"Crawler animation family {index} exceeds five authored tables.");
        }

        return InitialFamilies[index].ForOrientation(orientation);
    }

    /// <summary>Returns one shared surface list using the native even species offset.</summary>
    internal static ushort SurfaceInstruction(
        ushort speciesByteOffset,
        CrawlerSurfaceOrientation orientation)
    {
        if ((speciesByteOffset & 1) != 0 || speciesByteOffset > 10)
        {
            throw new InvalidDataException(
                $"Crawler instruction-table offset ${speciesByteOffset:X4} is invalid.");
        }

        return SurfaceSpecies[speciesByteOffset >> 1].ForOrientation(orientation);
    }
}
