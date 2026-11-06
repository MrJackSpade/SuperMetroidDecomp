using SuperMetroid.Core.Progression;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Presentation-only schema of one area's map: no reveal masks, callbacks, save indexes or exploration commands.</summary>
public sealed record MapPresentationDocument
{
    /// <summary>Schema revision for the area's lowercase-name <c>.json</c> file; loading requires the supported revision, version 2 since cells are named by what they show (#1513).</summary>
    public required int Version { get; init; }
    /// <summary>Case-sensitive <see cref="AreaId"/> name that must match the area of the supplied exploration rules.</summary>
    public required string Area { get; init; }
    /// <summary>Exactly 2048 cells in 64-column, 32-row order, rather than the cartridge's two adjacent 32-by-32 pages.</summary>
    public required AreaMapDocumentCell[] Cells { get; init; }
}
