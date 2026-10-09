namespace SuperMetroid.Core.Game;

/// <summary>Mutually exclusive crawler animation families with distinct initial tables.</summary>
internal enum CrawlerAnimationFamily
{
    /// <summary>Uses the common initial animation table shared by the first crawler species.</summary>
    Shared,
    /// <summary>Uses Viola's species-specific initial surface lists.</summary>
    Viola,
    /// <summary>Uses Sciser's species-specific initial surface lists.</summary>
    Sciser,
    /// <summary>Uses Zero's species-specific initial surface lists.</summary>
    Zero,
    /// <summary>Uses the H-Zoomer species-specific initial surface lists.</summary>
    HZoomer,
}

/// <summary>The four physical surfaces represented by crawler animation lists.</summary>
internal enum CrawlerSurfaceOrientation : ushort
{
    /// <summary>Surface-facing direction used while crawling along a right-facing side.</summary>
    UpsideRight = 0,
    /// <summary>Surface-facing direction used while crawling along a left-facing side.</summary>
    UpsideLeft = 1,
    /// <summary>Surface-facing direction used while crawling upside down.</summary>
    UpsideDown = 2,
    /// <summary>Surface-facing direction used while crawling upright.</summary>
    UpsideUp = 3,
}

/// <summary>Four surface-oriented instruction lists for one crawler family or species.</summary>
/// <param name="UpsideRight">Instruction list selected for the right-facing surface orientation.</param>
/// <param name="UpsideLeft">Instruction list selected for the left-facing surface orientation.</param>
/// <param name="UpsideDown">Instruction list selected for the upside-down surface orientation.</param>
/// <param name="UpsideUp">Instruction list selected for the upright surface orientation.</param>
internal readonly record struct CrawlerAnimationDefinition(
    ushort UpsideRight,
    ushort UpsideLeft,
    ushort UpsideDown,
    ushort UpsideUp)
{
    /// <summary>Selects the instruction-list pointer corresponding to a crawler's surface orientation.</summary>
    /// <param name="orientation">One of the four authored surface directions.</param>
    /// <returns>The instruction-list address for that direction.</returns>
    /// <exception cref="InvalidDataException">The orientation value is not one of the four defined directions.</exception>
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
    private static CrawlerAnimationDefinition Family(CrawlerAnimationFamily family) => family switch
    {
        CrawlerAnimationFamily.Shared => new(
            SharedCrawlerInstructionProgramDefinitions.UpsideRight,
            SharedCrawlerInstructionProgramDefinitions.UpsideLeft,
            SharedCrawlerInstructionProgramDefinitions.UpsideDown,
            SharedCrawlerInstructionProgramDefinitions.UpsideUp),
        CrawlerAnimationFamily.Viola => new(
            ViolaInstructionProgramDefinitions.UpsideRight,
            ViolaInstructionProgramDefinitions.UpsideLeft,
            ViolaInstructionProgramDefinitions.UpsideDown,
            ViolaInstructionProgramDefinitions.UpsideUp),
        CrawlerAnimationFamily.Sciser => new(
            SciserInstructionProgramDefinitions.UpsideRight,
            SciserInstructionProgramDefinitions.UpsideLeft,
            SciserInstructionProgramDefinitions.UpsideDown,
            SciserInstructionProgramDefinitions.UpsideUp),
        CrawlerAnimationFamily.Zero => new(
            ZeroInstructionProgramDefinitions.UpsideRight,
            ZeroInstructionProgramDefinitions.UpsideLeft,
            ZeroInstructionProgramDefinitions.UpsideDown,
            ZeroInstructionProgramDefinitions.UpsideUp),
        CrawlerAnimationFamily.HZoomer => new(
            HZoomerInstructionProgramDefinitions.UpsideRight,
            HZoomerInstructionProgramDefinitions.UpsideLeft,
            HZoomerInstructionProgramDefinitions.UpsideDown,
            HZoomerInstructionProgramDefinitions.UpsideUp),
        _ => throw new InvalidDataException(
            $"Crawler animation family {(int)family} exceeds five authored tables."),
    };
    /// <summary>Returns one family-specific initial instruction list.</summary>
    internal static ushort InitialInstruction(
        CrawlerAnimationFamily family,
        CrawlerSurfaceOrientation orientation) => Family(family).ForOrientation(orientation);
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

        // $A3:E630-$E65F: the first three species share the common animation;
        // the remaining species select Viola, Sciser and Zero respectively.
        CrawlerAnimationFamily family = speciesByteOffset switch
        {
            <= 4 => CrawlerAnimationFamily.Shared,
            6 => CrawlerAnimationFamily.Viola,
            8 => CrawlerAnimationFamily.Sciser,
            _ => CrawlerAnimationFamily.Zero,
        };
        return Family(family).ForOrientation(orientation);
    }
}
