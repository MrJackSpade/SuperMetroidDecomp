using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>The order the file-select world map draws its area labels in.</summary>
public static class FileSelectMapAreaOrder
{
    /// <summary>The six Zebes areas in the order the world map draws their labels.</summary>
    public static IReadOnlyList<AreaId> Labels { get; } =
    [
        AreaId.Crateria, AreaId.WreckedShip, AreaId.Tourian, AreaId.Brinstar, AreaId.Maridia, AreaId.Norfair,
    ];
}
