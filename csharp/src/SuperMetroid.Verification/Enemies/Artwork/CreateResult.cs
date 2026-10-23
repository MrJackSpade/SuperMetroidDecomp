using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>The enemies and cgram of one create result.</summary>
/// <param name="Enemies">The enemies.</param>
/// <param name="Cgram">The cgram.</param>
internal readonly record struct CreateResult(RoomEnemySystem Enemies, SamusColourRig Cgram);
