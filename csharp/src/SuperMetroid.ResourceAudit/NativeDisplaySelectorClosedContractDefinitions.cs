namespace SuperMetroid.ResourceAudit;

/// <summary>Display-ID projection without an artwork request; not a frame-availability proof.</summary>
internal static class NativeDisplaySelectorClosedContractDefinitions
{
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.EnemyExtendedFrameCatalog", "extended-display-id-projection-no-resource-read", ["GetDisplayPointer"],
            [new("csharp/src/SuperMetroid.Core/Assets/EnemyExtendedFrameCatalog.cs", "514C6D8749981E5CEF09DF3E1637C8E9443A6404F22DA0C9646832FD1FBB862D")]),
    ];
}
