// The access classes imported everywhere this folder is compiled (#2554): each names, as extension members, the parts of
// the game production hands what runs in it (the loaded room, Samus's ground, a species' state, a frontend step, a shot's
// kind state), so a check reaches them wherever it reaches their owner. None reaches a member by name (#2602).
global using static SuperMetroid.Core.World.LoadedRoomAccess;
global using static SuperMetroid.Core.Samus.SamusGroundAccess;
global using static SuperMetroid.Core.Enemies.EnemySpeciesStateAccess;
global using static SuperMetroid.Core.Frontend.FrontendSessionStepAccess;
global using static SuperMetroid.Core.Frontend.FrontendSessionEntryAccess;
global using static SuperMetroid.Core.Samus.Projectiles.SamusShotKindStateAccess;
