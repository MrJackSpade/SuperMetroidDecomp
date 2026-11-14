using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>The cartridge words naming the enemies' grapple responses, which only verification's header decode reads (#1411).</summary>
[EncodingOwner("enemy grapple response words")]
internal static class EnemyGrappleResponseWords
{
    /// <summary>The bank-$A0 routine an enemy header's grapple word names; zero names none.</summary>
    internal static readonly NativeWordIdentities<EnemyGrappleResponse> Responses = new(new Dictionary<EnemyGrappleResponse, ushort>
    {
        [EnemyGrappleResponse.None] = 0x0000,
        [EnemyGrappleResponse.NoInteraction] = 0x8000,
        [EnemyGrappleResponse.Attach] = 0x8005,
        [EnemyGrappleResponse.Kill] = 0x800a,
        [EnemyGrappleResponse.Cancel] = 0x800f,
        [EnemyGrappleResponse.AttachWithoutInvincibility] = 0x8014,
        [EnemyGrappleResponse.AttachAndParalyze] = 0x8019,
        [EnemyGrappleResponse.HurtSamus] = 0x801e,
        [EnemyGrappleResponse.SpeciesOwn] = 0x952a,
    });
}
