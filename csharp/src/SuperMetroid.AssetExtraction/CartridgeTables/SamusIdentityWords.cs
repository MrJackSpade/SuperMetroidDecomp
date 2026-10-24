using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Grapple;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>The cartridge words naming Samus's grapple AI routines and X-ray reveal commands (#1411).</summary>
[CpuBusBoundary("catalog: Samus identity words")]
internal static class SamusIdentityWords
{
    /// <summary>The bank-$A0 routine an enemy header's grapple AI word names; zero names none.</summary>
    internal static readonly NativeWordIdentities<GrappleAiRoutine> GrappleAiRoutines = new(new Dictionary<GrappleAiRoutine, ushort>
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

    /// <summary>The bank-$91 revealed-block command an X-ray reveal table entry names.</summary>
    internal static readonly NativeWordIdentities<XrayRevealCommand> XrayRevealCommands = new(new Dictionary<XrayRevealCommand, ushort>
    {
        [XrayRevealCommand.VerticalExtension] = 0xce79,
        [XrayRevealCommand.HorizontalExtension] = 0xcebb,
        [XrayRevealCommand.CopyOne] = 0xcf36,
        [XrayRevealCommand.CopyBrinstar] = 0xcf3e,
        [XrayRevealCommand.CopyWide] = 0xcf4e,
        [XrayRevealCommand.CopyTall] = 0xcf62,
        [XrayRevealCommand.CopySquare] = 0xcf6f,
    });
}
