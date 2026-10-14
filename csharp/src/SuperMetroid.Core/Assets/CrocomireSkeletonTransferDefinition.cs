namespace SuperMetroid.Core.Assets;

/// <summary>One native Crocomire skeleton-character upload, with fixed VRAM placement.</summary>
internal readonly record struct CrocomireSkeletonTransferDefinition(
    int SourceAddress, ushort DestinationOffset);
