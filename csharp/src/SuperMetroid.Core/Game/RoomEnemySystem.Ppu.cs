using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    /// <summary>The bound CGRAM's enemy and boss ranges.</summary>
    private EnemyPalettes BossPalettes => new(_cgram!);

    /// <summary>The bound CGRAM's hardware banks and rows, for actors whose palette row is selected at run time.</summary>
    private CgramPalettes CgramRows => new(_cgram!);
}
