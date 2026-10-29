using System.Text.Json.Serialization;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// One animation frame's body-tile selection: which upper and lower definition group is used and which definition
/// within each group. Every component is a table index from 0 through 255; the body catalog rejects any other value.
/// </summary>
/// <param name="TopSet">Upper-body definition group index.</param>
/// <param name="TopPosition">Definition index counted from the selected upper group's first definition.</param>
/// <param name="BottomSet">Lower-body definition group index, or <see cref="NoBottomSet"/> when the frame names no lower half.</param>
/// <param name="BottomPosition">Definition index counted from the selected lower group's first definition.</param>
public readonly record struct SamusBodyFrameSelection(
    [property: JsonRequired] int TopSet,
    [property: JsonRequired] int TopPosition,
    [property: JsonRequired] int BottomSet,
    [property: JsonRequired] int BottomPosition)
{
    /// <summary>
    /// The lower group index with which a frame names no lower half: the body keeps showing the lower half an earlier
    /// frame selected. It is the largest index a selector component can hold, which no lower group uses.
    /// </summary>
    public const int NoBottomSet = byte.MaxValue;

    /// <summary>Whether this frame names a lower half rather than keeping the one already shown.</summary>
    [JsonIgnore]
    public bool NamesBottom => BottomSet != NoBottomSet;
}
