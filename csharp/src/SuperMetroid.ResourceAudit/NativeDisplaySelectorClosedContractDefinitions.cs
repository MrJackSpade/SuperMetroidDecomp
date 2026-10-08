namespace SuperMetroid.ResourceAudit;

/// <summary>Display-ID projection without an artwork request; not a frame-availability proof.</summary>
internal static class NativeDisplaySelectorClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.EnemyExtendedFrameCatalog", "extended-display-id-projection-no-resource-read", ["GetDisplayPointer"],
            [new("csharp/src/SuperMetroid.Core/Assets/EnemyExtendedFrameCatalog.cs", "D658FAC93CBEA1A4697CEB0D0C5C02E935272372673A2C2290DEE18B3679B25B")]),
    ];
}
