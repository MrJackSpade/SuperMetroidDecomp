using SuperMetroid.Core.Frontend.Ending;

namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>The file name, hours and target of one verify post credits character artwork case.</summary>
/// <param name="FileName">The file name.</param>
/// <param name="Hours">The hours.</param>
/// <param name="Target">The target.</param>
internal readonly record struct VerifyPostCreditsCharacterArtworkCase(string FileName, ushort Hours, EndingCreditsPhase Target);
