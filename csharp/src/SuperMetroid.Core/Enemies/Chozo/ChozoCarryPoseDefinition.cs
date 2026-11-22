namespace SuperMetroid.Core.Enemies.Chozo;

/// <summary>How a carrying Chozo statue moves and holds Samus in one of its poses, for a statue facing right.</summary>
/// <param name="Speed">How far the statue walks each update in the pose, toward the way it faces; zero for a still pose.</param>
/// <param name="Hold">Where Samus is held from the statue's origin: X toward the way it faces, Y downward.</param>
internal readonly partial record struct ChozoCarryPoseDefinition(SignedEightEight Speed, Offset16<RoomPixels> Hold);
