/// <summary>Distinct controller-earned native temporary-boost comparison matrices.</summary>
internal enum TemporaryBlueAuditKind { Retention, Carry, Bounce, Cancellation, Sand, Terrain }

/// <summary>Accepted original-CPU trace identities and complete matrix dimensions.</summary>
internal static class TemporaryBlueAuditDefinitions
{
    public static (int Cases, int Frames, string Hash) Describe(TemporaryBlueAuditKind kind) => kind switch
    {
        TemporaryBlueAuditKind.Retention => (32, 400, "017B7A761CA8CBDA8BA14CAF7886F5FFC4760B869C9FD6BFA8C37C91AE6A8E39"),
        TemporaryBlueAuditKind.Carry => (80, 620, "C7486E42C4A0247665E4ABC1FD076A9E1D4D6305056E72C181136B6F5BB2B414"),
        TemporaryBlueAuditKind.Bounce => (16, 800, "1A930ABCDBD78434CC0CA6220F512E2DAC139AEB939C3A82BC0F38472A446296"),
        TemporaryBlueAuditKind.Cancellation => (16, 460, "E8AD5317A45B4692FFACA07F660D38250F7A5707BD2D2DFE7427C718FAD43B7E"),
        TemporaryBlueAuditKind.Sand => (16, 401, "C9BDA4C0D7117C1B019129096EC184D3D182F0392CF37F1CBD562D45133F7DB6"),
        TemporaryBlueAuditKind.Terrain => (32, 401, "5E2E1203C611077418D168737BE24BE91CA3EF1A9336ACF6070BD159DB897D08"),
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };
}
