namespace SuperMetroid.ResourceAudit;

/// <summary>Display-ID projection without an artwork request; not a frame-availability proof.</summary>
internal static class NativeDisplaySelectorClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.EnemyExtendedFrameCatalog", "extended-display-id-projection-no-resource-read", ["GetDisplayPointer"],
            [new("csharp/src/SuperMetroid.Core/Assets/EnemyExtendedFrameCatalog.cs", "D658FAC93CBEA1A4697CEB0D0C5C02E935272372673A2C2290DEE18B3679B25B")],
            "GetDisplayPointer only probes the already installed binding dictionary and projects its value to ushort; an unbound key returns nativePointer. It does not index frame artwork or throw for missing selectors, so no resource identity is required for this operation. TryGetDisplay has a separate artwork-availability rule; downstream BG2/OAM availability is not covered by this projection rule. This is not a drawing, binding-selection, placement or pixel proof."),
    ];
}
