using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms.Artwork;

/// <summary>
/// The values a room's appearance inputs take now (#1466): what gameplay showed over the room's load, the frames its
/// animations selected, and the looks its encounters show, which only the Ceres Ridley arena's blocks
/// follow.
/// </summary>
/// <param name="Room">What gameplay showed over the room's load.</param>
/// <param name="Frames">The frame each of the room's animations shows.</param>
/// <param name="Encounters">The looks the room's encounters show (#1538).</param>
public readonly record struct RoomAppearanceView(RoomAppearance Room, ArtworkFrameSelections Frames, IEncounterLookView Encounters)
{
    /// <summary>The value <paramref name="input"/> takes now, as a picture's name spells it.</summary>
    public string ValueOf(AppearanceInput input) => Ensure.NotNull(input).ValueIn(this);
}
