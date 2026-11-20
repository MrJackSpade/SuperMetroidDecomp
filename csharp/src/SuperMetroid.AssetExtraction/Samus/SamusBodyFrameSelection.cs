using System.Text.Json.Serialization;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// One animation frame's body-tile selection: which upper and lower definition group is used and which definition
/// within each group. Every component is a table index from 0 through 255; the body catalog rejects any other value.
/// </summary>
/// <param name="TopSet">Upper-body definition group index.</param>
/// <param name="TopPosition">Definition index counted from the selected upper group's first definition.</param>
/// <param name="BottomSet">Lower-body definition group index, or null when the frame names no lower half.</param>
/// <param name="BottomPosition">Definition index counted from the selected lower group's first definition.</param>
public readonly record struct SamusBodyFrameSelection(
    [property: JsonRequired] int TopSet,
    [property: JsonRequired] int TopPosition,
    [property: JsonRequired] int? BottomSet,
    [property: JsonRequired] int BottomPosition)
{
    /// <summary>
    /// Whether this frame names a lower half; the import composes the whole picture of a frame that names none with no
    /// lower half's characters (#1585).
    /// </summary>
    [JsonIgnore]
    public bool NamesBottom => BottomSet is not null;

    /// <summary>The lower group index with which the cartridge's frame names no lower half, the largest a selector holds.</summary>
    private const int CartridgeNoBottomSet = byte.MaxValue;

    /// <summary>
    /// The selection the cartridge's four selector bytes make (#2558): its lower group's largest index names no lower half,
    /// which the selection holds as none.
    /// </summary>
    /// <param name="topSet">The upper group byte.</param>
    /// <param name="topPosition">The upper definition byte.</param>
    /// <param name="bottomSet">The lower group byte.</param>
    /// <param name="bottomPosition">The lower definition byte.</param>
    public static SamusBodyFrameSelection FromCartridge(int topSet, int topPosition, int bottomSet, int bottomPosition) =>
        new(topSet, topPosition, bottomSet == CartridgeNoBottomSet ? null : bottomSet, bottomPosition);
}
