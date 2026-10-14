using System.Text.Json.Serialization;

namespace SuperMetroid.Core.Assets;

/// <summary>Encoded X/size word, signed-wrap Y byte, and complete OBJ attribute word.</summary>
public readonly record struct SamusSpritePart(
    [property: JsonRequired] ushort X,
    [property: JsonRequired] byte Y,
    [property: JsonRequired] ushort Attributes);
