namespace SuperMetroid.ResourceAudit;

/// <summary>Display-ID projection without an artwork request; not a frame-availability proof.</summary>
internal static class NativeDisplaySelectorClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.EnemyExtendedFrameCatalog", "extended-display-id-projection-no-resource-read", ["GetDisplayPointer"],
            [new("csharp/src/SuperMetroid.Core/Assets/EnemyExtendedFrameCatalog.cs", "2A8DCB47F4C1E1B16847B2B64259F04C85FC4C57A54DBF9608023B6E335F379D")],
            "GetDisplayPointer only probes the already installed binding dictionary and projects its value to ushort; an unbound key returns nativePointer. It does not index frame artwork or throw for missing selectors, so no resource identity is required for this operation. TryGetDisplay has a separate artwork-availability rule; downstream BG2/OAM availability is not covered by this projection rule. This is not a drawing, binding-selection, placement or pixel proof."),
    ];
}
