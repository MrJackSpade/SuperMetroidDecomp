using SuperMetroid.Core.Enemies.YappingMaws;

namespace SuperMetroid.Verification.Enemies.YappingMaws;

/// <summary>The entry, loop, calls, x, y and name of one cooldown.</summary>
/// <param name="Entry">The entry.</param>
/// <param name="Loop">The loop.</param>
/// <param name="Calls">The calls.</param>
/// <param name="X">The x.</param>
/// <param name="Y">The y.</param>
/// <param name="Name">The name.</param>
internal readonly record struct Cooldown(YappingMawProgram Entry, YappingMawProgram Loop, int Calls, short X, short Y, string Name);
