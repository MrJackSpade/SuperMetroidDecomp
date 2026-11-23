using SuperMetroid.Core.Enemies.Animals;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.Animals;

/// <summary>An escape Etecoon set up in the first place of its enemy system, with the state its setup built.</summary>
/// <param name="Enemies">The enemy system.</param>
/// <param name="State">The state the Etecoon's setup built, which its animation runs on.</param>
internal readonly record struct EscapeEtecoonProgramSystem(RoomEnemySystem Enemies, EscapeEtecoonEnemyState State);
