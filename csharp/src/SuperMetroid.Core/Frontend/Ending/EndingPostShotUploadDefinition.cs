namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>One part the post-shot transition shows: which artwork, from where in it, and how many bytes of it.</summary>
/// <param name="Length">The bytes the part shows.</param>
/// <param name="Offset">The bytes into its artwork the part starts at (#1416: replaces the cartridge work-RAM source).</param>
/// <param name="Source">Which artwork the part shows.</param>
internal readonly record struct EndingPostShotUploadDefinition(
    int Length, int Offset, EndingPostShotUploadSource Source);
