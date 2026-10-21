using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.World;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Samus.Abilities;

/// <summary>The bus, level, plms and block index of one create speed booster block fixture result.</summary>
/// <param name="Bus">The bus.</param>
/// <param name="Level">The level.</param>
/// <param name="Plms">The plms.</param>
/// <param name="BlockIndex">The block index.</param>
internal readonly record struct CreateSpeedBoosterBlockFixtureResult(TestAddressSpace Bus, RoomLevelData Level, RoomPlmSystem Plms, int BlockIndex);
