namespace SuperMetroid.ResourceAudit.PlmPrograms;

/// <summary>Finite closure of immutable PLM records. Never runs a PLM or opens a ROM.</summary>
internal sealed class PlmProgramAuditReport
{
    /// <summary>Decoded instruction and timed-draw records reached by the static walk.</summary>
    public int Records { get; set; }
    /// <summary>Timed PLM draw records among the decoded records.</summary>
    public int TimedDraws { get; set; }
    /// <summary>Word operands required by decoded instruction formats.</summary>
    public int WordOperands { get; set; }
    /// <summary>Packed byte operands required by decoded instruction formats.</summary>
    public int ByteOperands { get; set; }
    /// <summary>Cartridge-source VRAM transfers that require an installed artwork provider.</summary>
    public int ArtworkTransfers { get; set; }
    /// <summary>Missing words, draws, targets, or transfer ownership found by the walk.</summary>
    public List<PlmProgramFinding> Findings { get; } = [];
    /// <summary>Roots classified as execution owned by a typed PLM path.</summary>
    public List<PlmProgramClassification> Classifications { get; } = [];
}
