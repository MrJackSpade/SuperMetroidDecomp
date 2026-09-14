namespace SuperMetroid.Core.Assets;

/// <summary>Appearance-bearing timed records in the four bank-$90 trail lists, excluding commands and terminators.</summary>
public static class ProjectileTrailVisualDefinitions
{
    public const string FileName = "projectile-trails.json";
    public const int Version = 1;
    /// <summary>$90:B4CB/B52D ice lists, $B58F wave list and $B5A1 missile list: timed record addresses.</summary>
    public static ReadOnlySpan<ushort> Frames =>
    [
        0xb4cb, 0xb4cf, 0xb4d3, 0xb4d7, 0xb4db, 0xb4df, 0xb4e5, 0xb4e9,
        0xb4ef, 0xb4f5, 0xb4fb, 0xb501, 0xb507, 0xb50d, 0xb513, 0xb519, 0xb51f,
        0xb52d, 0xb531, 0xb535, 0xb539, 0xb53d, 0xb541, 0xb547, 0xb54b,
        0xb551, 0xb557, 0xb55d, 0xb563, 0xb569, 0xb56f, 0xb575, 0xb57b, 0xb581,
        0xb58f, 0xb593, 0xb597, 0xb59b, 0xb5a1, 0xb5a5, 0xb5a9, 0xb5ad,
    ];
    public static string Name(ushort frame) => $"trail_{frame:X4}";
}
