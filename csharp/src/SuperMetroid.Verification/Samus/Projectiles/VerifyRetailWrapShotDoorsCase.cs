using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Verification.Samus.Projectiles;

/// <summary>The room, x, y, target, beams and hit of one verify retail wrap shot doors case.</summary>
/// <param name="Room">The room.</param>
/// <param name="X">The x.</param>
/// <param name="Y">The y.</param>
/// <param name="Target">The target.</param>
/// <param name="Beams">The beams.</param>
/// <param name="Hit">The hit.</param>
internal readonly record struct VerifyRetailWrapShotDoorsCase(RoomHeaderDefinition Room, int X, int Y, int Target, int Beams, int Hit);
