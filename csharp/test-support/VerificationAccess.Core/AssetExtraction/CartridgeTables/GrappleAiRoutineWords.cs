using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Grapple;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>The cartridge words naming Samus's grapple AI routines, which only verification's header decode reads (#1411).</summary>
[EncodingOwner("grapple AI routine words")]
internal static class GrappleAiRoutineWords
{
    /// <summary>The bank-$A0 routine an enemy header's grapple AI word names; zero names none.</summary>
    internal static readonly NativeWordIdentities<GrappleAiRoutine> Routines = new(new Dictionary<GrappleAiRoutine, ushort>
    {
        [GrappleAiRoutine.None] = 0x0000,
        [GrappleAiRoutine.NoInteraction] = 0x8000,
        [GrappleAiRoutine.Attach] = 0x8005,
        [GrappleAiRoutine.Kill] = 0x800a,
        [GrappleAiRoutine.Cancel] = 0x800f,
        [GrappleAiRoutine.AttachWithoutInvincibility] = 0x8014,
        [GrappleAiRoutine.AttachAndParalyze] = 0x8019,
        [GrappleAiRoutine.HurtSamus] = 0x801e,
        [GrappleAiRoutine.ElevatorMainAi] = 0x952a,
    });
}
