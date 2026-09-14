/// <summary>Distinct controller-earned native temporary-boost comparison matrices.</summary>
internal enum TemporaryBlueAuditKind { Retention, Carry, Bounce }

/// <summary>Accepted original-CPU trace identities and complete matrix dimensions.</summary>
internal static class TemporaryBlueAuditDefinitions
{
    public static (int Cases, int Frames, string Hash) Describe(TemporaryBlueAuditKind kind) => kind switch
    {
        TemporaryBlueAuditKind.Retention => (32, 400, "017B7A761CA8CBDA8BA14CAF7886F5FFC4760B869C9FD6BFA8C37C91AE6A8E39"),
        TemporaryBlueAuditKind.Carry => (80, 620, "C7486E42C4A0247665E4ABC1FD076A9E1D4D6305056E72C181136B6F5BB2B414"),
        TemporaryBlueAuditKind.Bounce => (16, 800, "1A930ABCDBD78434CC0CA6220F512E2DAC139AEB939C3A82BC0F38472A446296"),
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };
}
