using SuperMetroid.Core.Frontend.Ending.Artwork;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>One authored explosion OBJ fragment.</summary>
/// <param name="Fragment">The fragment loaded.</param>
internal sealed record EndingObjectFragmentUpload(EndingObjectFragmentId Fragment) : EndingUpload(EndingUploadKind.ObjectFragment);
