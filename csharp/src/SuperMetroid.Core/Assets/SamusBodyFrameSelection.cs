using System.Text.Json.Serialization;

namespace SuperMetroid.Core.Assets;

/// <summary>Four-byte native selector: upper set/position, then lower set/position.</summary>
public readonly record struct SamusBodyFrameSelection(
    [property: JsonRequired] byte TopSet,
    [property: JsonRequired] byte TopPosition,
    [property: JsonRequired] byte BottomSet,
    [property: JsonRequired] byte BottomPosition);
