namespace SuperMetroid.Core.Assets;

/// <summary>Calculated source-page and OBJ-placement identities, without stored transfer rows.</summary>
internal readonly record struct CrocomireSkeletonTransferSequence(int Length)
{
    /// <summary>Computes the selected native source page and VRAM destination without storing transfer rows.</summary>
    internal CrocomireSkeletonTransferDefinition this[int index] =>
        CrocomireSkeletonTransferDefinitions.Frame(index);
}
