namespace SuperMetroid.ResourceAudit;

/// <summary>Display-ID projection without an artwork request; not a frame-availability proof.</summary>
internal static class NativeDisplaySelectorClosedContractDefinitions
{
    /// <summary>Fingerprint set bounding display-ID projection without requesting frame artwork.</summary>
    internal static readonly ClosedPresentationContract[] All =
    [
        new("SuperMetroid.Core.Assets.EnemyExtendedFrameCatalog", "extended-display-id-projection-no-resource-read", ["GetDisplayPointer"],
            [new("csharp/src/SuperMetroid.Core/Assets/EnemyExtendedFrameCatalog.cs", "207C15764374EBFED588708E9334392D74087F834542C102F703ACF548330006")]),
    ];
}
