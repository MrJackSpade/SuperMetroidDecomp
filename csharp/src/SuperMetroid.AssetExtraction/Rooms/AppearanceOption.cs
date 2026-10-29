using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// One value an appearance input can take and the artwork it shows over a room's characters (#1466): a collectible in a
/// slot, Kraid's backdrop, an animation's frame or an encounter's look. Extraction pictures what reads those characters
/// for each value; gameplay names only the input and its value.
/// </summary>
/// <param name="Input">The input.</param>
/// <param name="Value">The value, as a picture's name spells it.</param>
/// <param name="Upload">The artwork the value shows and where it lands.</param>
internal sealed record AppearanceOption(AppearanceInput Input, string Value, BgCharacterUpload Upload);
