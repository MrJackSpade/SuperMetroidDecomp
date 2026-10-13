namespace SuperMetroid.Core.Game;

/// <summary>Mutually exclusive crawler animation families with distinct initial tables.</summary>
internal enum CrawlerAnimationFamily
{
    /// <summary>Common surface lists used by the first three native species offsets.</summary>
    Shared,
    /// <summary>Viola's distinct initial surface table and later species selection.</summary>
    Viola,
    /// <summary>Sciser's distinct initial surface table and later species selection.</summary>
    Sciser,
    /// <summary>Zero's distinct initial surface table and later species selection.</summary>
    Zero,
    /// <summary>H-Zoomer's initial table; later shared-surface selection does not use this family.</summary>
    HZoomer,
}

/// <summary>The four physical surfaces represented by crawler animation lists.</summary>
internal enum CrawlerSurfaceOrientation : ushort
{
    /// <summary>Native table index zero, top facing right on a vertical surface.</summary>
    UpsideRight = 0,
    /// <summary>Native table index one, top facing left on a vertical surface.</summary>
    UpsideLeft = 1,
    /// <summary>Native table index two, ceiling attachment.</summary>
    UpsideDown = 2,
    /// <summary>Native table index three, floor attachment.</summary>
    UpsideUp = 3,
}

/// <summary>Domain-owned transitions over <see cref="CrawlerSurfaceOrientation"/>.</summary>
internal static class CrawlerSurfaceOrientations
{
    /// <summary>The surface at <paramref name="index"/> of a four-entry surface-ordered table.</summary>
    internal static CrawlerSurfaceOrientation AtTableIndex(int index) => index switch
    {
        0 => CrawlerSurfaceOrientation.UpsideRight,
        1 => CrawlerSurfaceOrientation.UpsideLeft,
        2 => CrawlerSurfaceOrientation.UpsideDown,
        3 => CrawlerSurfaceOrientation.UpsideUp,
        _ => throw new InvalidOperationException($"Crawler surface table index {index} has no {nameof(CrawlerSurfaceOrientation)}."),
    };
}

/// <summary>Four surface-oriented instruction lists for one crawler family or species.</summary>
internal readonly record struct CrawlerAnimationDefinition(
    ushort UpsideRight,
    ushort UpsideLeft,
    ushort UpsideDown,
    ushort UpsideUp)
{
    /// <summary>Selects a native instruction-list pointer by one of the four physical surface orientations.</summary>
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
