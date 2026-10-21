using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Verification.Rooms.Plms;

/// <summary>The header and source of one verify downward gate header definitions case.</summary>
/// <param name="Header">The header.</param>
/// <param name="Source">The source.</param>
internal readonly record struct VerifyDownwardGateHeaderDefinitionsCase(PlmHeaderId Header, int Source);
