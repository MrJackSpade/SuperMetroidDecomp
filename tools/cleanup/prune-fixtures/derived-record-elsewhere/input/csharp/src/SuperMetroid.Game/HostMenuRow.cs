using SuperMetroid.Core.Menus;

namespace SuperMetroid.Game;

/// <summary>A row of the host's own menu, declared in another project than the row it derives from.</summary>
/// <param name="Number">The row's number, which nothing reads by name: comparing two rows by the base row compares it.</param>
internal sealed record HostMenuRow(int Number) : MenuRow;
