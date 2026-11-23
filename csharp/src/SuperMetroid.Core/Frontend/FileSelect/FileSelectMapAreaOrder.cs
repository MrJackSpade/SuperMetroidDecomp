using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>The order the file-select world map draws its area labels in.</summary>
public static partial class FileSelectMapAreaOrder
{
    /// <summary>The six Zebes areas in the order the world map draws their labels.</summary>
    public static IReadOnlyList<ZebesArea> Labels { get; } =
    [
        ZebesArea.Crateria, ZebesArea.WreckedShip, ZebesArea.Tourian, ZebesArea.Brinstar, ZebesArea.Maridia, ZebesArea.Norfair,
    ];
}
