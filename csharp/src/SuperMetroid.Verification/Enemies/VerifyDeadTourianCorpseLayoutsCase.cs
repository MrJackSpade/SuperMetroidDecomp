using SuperMetroid.Core.Enemies.Corpses;

namespace SuperMetroid.Verification.Enemies;

/// <summary>The profile and species of one verify dead tourian corpse layouts case.</summary>
/// <param name="Profile">The profile.</param>
/// <param name="Species">The species.</param>
internal readonly record struct VerifyDeadTourianCorpseLayoutsCase(DeadTourianBehavior.DeadTourianCorpseProfile Profile, DeadTourianCorpseSpecies Species);
