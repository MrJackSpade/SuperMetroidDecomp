using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>The draw and runs of one plm content identity frame.</summary>
/// <param name="Draw">The draw.</param>
/// <param name="Runs">The runs.</param>
internal readonly record struct PlmContentIdentityFrame(PlmDraw Draw, ushort[][] Runs);
