using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Verification.Rooms;

/// <summary>The pointer, planar and palette bytes of one verify room enemy loading case.</summary>
/// <param name="Pointer">The pointer.</param>
/// <param name="Planar">The planar.</param>
/// <param name="PaletteBytes">The palette bytes.</param>
internal readonly record struct VerifyRoomEnemyLoadingCase(EnemyDefinitionId Pointer, byte[] Planar, byte[] PaletteBytes);
