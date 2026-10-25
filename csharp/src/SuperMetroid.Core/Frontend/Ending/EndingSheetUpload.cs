namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>A transfer of a whole installed sheet: the escape clouds, the explosion objects, a font or the credits art.</summary>
/// <param name="Kind">The sheet's transfer.</param>
internal sealed record EndingSheetUpload(EndingUploadKind Kind) : EndingUpload(Kind);
