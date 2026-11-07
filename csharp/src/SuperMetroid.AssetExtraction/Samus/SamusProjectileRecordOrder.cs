using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The order the import walks the Samus projectiles' timed records in (#1301, #1517): the frame bindings file, the sprite
/// locations and the record table each list every record in this order.
/// </summary>
internal static class SamusProjectileRecordOrder
{
    /// <summary>Every timed record, program by program in program order.</summary>
    internal static IReadOnlyList<SamusProjectileFrame> Frames { get; } =
        [.. Enum.GetValues<SamusProjectileProgram>().SelectMany(program =>
            Enumerable.Range(0, SamusProjectileRecordDefinitions.ByProgram[program].Length).Select(show => new SamusProjectileFrame(program, show)))];
}
