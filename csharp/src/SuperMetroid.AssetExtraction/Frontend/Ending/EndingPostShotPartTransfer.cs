namespace SuperMetroid.AssetExtraction.Frontend.Ending;

/// <summary>One of the post-shot's subtitle and logo parts.</summary>
/// <param name="Part">The part loaded.</param>
internal sealed record EndingPostShotPartTransfer(EndingPostShotUploadDefinition Part) : EndingTransfer;
