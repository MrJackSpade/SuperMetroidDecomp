namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>Cartridge values and helpers of <see cref="GameInstallationLayout"/> that only verification reads.</summary>
internal static class GameInstallationLayoutConstants
{
    /// <summary>
    /// The picture store (#1635): every picture the sheets show, each written once, on shelf-packed pages that every sheet
    /// manifest names its frames' pictures on (<see cref="PictureStoreFiles"/>).
    /// </summary>
    public const string PictureStoreDirectoryName = PictureStoreFiles.DirectoryName;
}
