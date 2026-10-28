using SuperMetroid.Core.Frontend.FileSelect.Artwork;
using SuperMetroid.Core.Frontend.Menus.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;

/// <summary>What the file-select sheet's compositions are recognised from: each border's heading and the helmet's menu atlas cells.</summary>
internal static class FileSelectSpriteDefinitions
{
    /// <summary>The menu heading a bordered heading draws.</summary>
    internal static readonly IReadOnlyDictionary<FileSelectBorder, MenuHeading> Headings =
        new Dictionary<FileSelectBorder, MenuHeading>
        {
            [FileSelectBorder.Main] = MenuHeading.FileSelectMain,
            [FileSelectBorder.Copy] = MenuHeading.FileSelectCopy,
            [FileSelectBorder.Clear] = MenuHeading.FileSelectClear,
        };

    /// <summary>$82:CAE9: the helmet's first 24x24 body block begins at menu atlas cell $D0.</summary>
    internal const int HelmetFirstBodyCell = 0xd0;

    /// <summary>$82:CB2B: the helmet's first two-cell visor strip begins at menu atlas cell $D9.</summary>
    internal const int HelmetFirstVisorCell = 0xd9;
}
