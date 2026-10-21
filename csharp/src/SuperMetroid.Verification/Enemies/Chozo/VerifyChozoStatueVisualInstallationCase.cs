using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Verification.Enemies.Chozo;

/// <summary>The header, physical and visual of one verify chozo statue visual installation case.</summary>
/// <param name="Header">The header.</param>
/// <param name="Physical">The physical.</param>
/// <param name="Visual">The visual.</param>
internal readonly record struct VerifyChozoStatueVisualInstallationCase(PlmHeaderId Header, ushort Physical, ushort Visual);
