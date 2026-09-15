/// <summary>Distinct controller-earned native temporary-boost comparison matrices.</summary>
internal enum TemporaryBlueAuditKind { Retention, Carry, Bounce, Cancellation, Sand, Terrain, Chain, Menu, DraygonDeath, DraygonGrab, DraygonEcho, DraygonMidair }

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
        TemporaryBlueAuditKind.Chain => (16, 1000, "B5097BFAA561907059B142478386125AD185DCC969F250FD2086A27987334957"),
        TemporaryBlueAuditKind.Menu => (12, 560, "5713EBE32CAC36423538C7126C312D82103629D83109A3AD6D73B083D83CC2CE"),
        TemporaryBlueAuditKind.DraygonDeath => (24, 400, "88E6A9B40CDD192C81BA4012795FA2A6CFF5E247989D773343233D99D112EEEE"),
        TemporaryBlueAuditKind.DraygonGrab => (16, 400, "0238D4101D520512886F8D4D7B5FBD5F2A3D10C666CD83570EF77C4D9EF0E42F"),
        TemporaryBlueAuditKind.DraygonEcho => (24, 400, "74A3D1BCC56539A43BB71E4B3A01FE2AAC97B7738CB2F6BAD35874292E59D505"),
        TemporaryBlueAuditKind.DraygonMidair => (16, 400, "D3AE80258C864D7FC8A4ADF416459D17607BFF1EE6284D41F53C56FEE1509C4A"),
        _ => throw new ArgumentOutOfRangeException(nameof(kind)),
    };
}
