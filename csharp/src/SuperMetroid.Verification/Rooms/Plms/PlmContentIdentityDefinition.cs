using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>The draw and word of one plm content identity definition.</summary>
/// <param name="Draw">The draw.</param>
/// <param name="Word">The word.</param>
internal readonly record struct PlmContentIdentityDefinition(PlmDraw Draw, ushort Word);
