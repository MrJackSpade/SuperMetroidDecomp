
namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>
/// The cartridge's enemy BG2 staging image Mother Brain stages her body in, which only verification reads: the port stages
/// her body on its own canvas (#1466).
/// </summary>
internal static class MotherBrainBg2DefinitionsConstants
{
    /// <summary>EnemyBG2Tilemap at $7E:2000.</summary>
    public const int WorkAddress = 0x7e2000;

    /// <summary>The words of the staging image her initialization clears: the whole $1000-byte image.</summary>
    public const int ClearCellCount = 0x800;
}
