using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Map.Artwork;

/// <summary>Presentation-only schema: no reveal masks, callbacks, save indexes or exploration commands.</summary>
public sealed record MapPresentationDocument
{
    /// <summary>Schema revision for the area's lowercase-name <c>.json</c> file; loading currently requires version 1.</summary>
    public required int Version { get; init; }
    /// <summary>Case-sensitive <see cref="AreaId"/> name that must match the area of the supplied exploration rules.</summary>
    public required string Area { get; init; }
    /// <summary>Exactly 2048 presentation cells in 64-column, 32-row order, rather than the cartridge's two adjacent 32-by-32 pages.</summary>
    public required MapPresentationCell[] Cells { get; init; }
}
