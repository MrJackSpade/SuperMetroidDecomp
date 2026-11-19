using SuperMetroid.Core.Enemies.MaridiaSnails;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Verification.Enemies.MaridiaSnails;

/// <summary>The context, level, phase, start, expected, expected phase and expected settled of one verify Oum bounce record case.</summary>
/// <param name="Context">The context.</param>
/// <param name="Level">The level.</param>
/// <param name="Phase">The phase.</param>
/// <param name="Start">The start.</param>
/// <param name="Expected">The expected.</param>
/// <param name="ExpectedPhase">The expected phase.</param>
/// <param name="ExpectedSettled">The expected settled.</param>
internal readonly record struct VerifyOumBounceRecordCase(string Context, RoomLevelData Level, OumBouncePhase Phase, int Start, int Expected, OumBouncePhase ExpectedPhase, bool ExpectedSettled);
