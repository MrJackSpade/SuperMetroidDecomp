using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Grapple;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>The cartridge words naming the enemies' grapple reactions, which only verification's header decode reads (#1411).</summary>
[EncodingOwner("enemy grapple response words")]
internal static class EnemyGrappleResponseWords
{
    /// <summary>The bank-$A0 routine an enemy header's grapple word names for each reaction.</summary>
    internal static readonly NativeWordIdentities<GrappleEnemyReaction> Reactions = new(new Dictionary<GrappleEnemyReaction, ushort>
    {
        [GrappleEnemyReaction.None] = 0x8000,
        [GrappleEnemyReaction.Attach] = 0x8005,
        [GrappleEnemyReaction.Kill] = 0x800a,
        [GrappleEnemyReaction.Cancel] = 0x800f,
        [GrappleEnemyReaction.AttachWithoutInvincibility] = 0x8014,
        [GrappleEnemyReaction.AttachAndParalyze] = 0x8019,
        [GrappleEnemyReaction.HurtSamus] = 0x801e,
    });

    /// <summary>The grapple words naming no reaction (#2283): zero, and the elevator's own routine, whose behaviour carries on.</summary>
    private static readonly HashSet<ushort> NoReaction = [0x0000, 0x952a];

    /// <summary>The reaction an enemy header's grapple word names, or null for a word that names none.</summary>
    internal static GrappleEnemyReaction? Decode(ushort word) => NoReaction.Contains(word) ? null : Reactions.Decode(word, "grapple response");
}
