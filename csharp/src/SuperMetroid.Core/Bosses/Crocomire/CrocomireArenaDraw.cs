namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>The five bank-<c>$84</c> Crocomire arena block-draw lists.</summary>
internal enum CrocomireArenaDraw : ushort
{
    /// <summary><c>$84:9B5B</c>: ten cleared bridge blocks.</summary>
    ClearBridge = 0x9b5b,
    /// <summary><c>$84:9B73</c>: one crumbling bridge block.</summary>
    CrumbleBridgeBlock = 0x9b73,
    /// <summary><c>$84:9B79</c>: one cleared bridge block.</summary>
    ClearBridgeBlock = 0x9b79,
    /// <summary><c>$84:9B7F</c>: three columns of clear wall blocks.</summary>
    ClearInvisibleWall = 0x9b7f,
    /// <summary><c>$84:9BBB</c>: three columns of solid wall blocks.</summary>
    CreateInvisibleWall = 0x9bbb,
}
