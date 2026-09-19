using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using System.Buffers.Binary;

internal static partial class Program
{
    /// <summary>Proves the replay seed and every raw controller bit survive serialization.</summary>
    private static void VerifyControllerInputRecording()
    {
        byte[] digest = Enumerable.Range(0, 32).Select(index => (byte)index).ToArray();
        byte[] saveRam = new byte[SuperMetroidAddressSpace.SaveRamByteCount];
        for (int index = 0; index < saveRam.Length; index++)
            saveRam[index] = unchecked((byte)(index * 37));
        ushort[] inputs = [0, 0x0080, 0x0280, 0xffff, 0x1000];
        var expected = new ControllerInputRecording
        {
            StartedUtc = new DateTimeOffset(2026, 9, 1, 12, 34, 56, TimeSpan.Zero),
            RomSha256 = digest,
            InitialSaveRam = saveRam,
            GameOptions = new SuperMetroidGameOptions
            {
                SkipOpeningCinematic = true,
                Invincibility = true,
                InfiniteAmmo = true,
                PreventEscapeTimeout = true,
                EndingTimeOverrideMinutes = 0,
                MapReveal = MapRevealMode.Secret,
            },
            ControllerInputs = inputs,
        };

        using var bytes = new MemoryStream();
        expected.Write(bytes);
        AssertEqual(1u, BinaryPrimitives.ReadUInt32LittleEndian(bytes.ToArray().AsSpan(8)),
            "legacy input recording retains version-one bytes");
        bytes.Position = 0;
        ControllerInputRecording actual = ControllerInputRecording.Read(bytes);

        AssertEqual(expected.StartedUtc, actual.StartedUtc, "input recording UTC seed");
        AssertTrue(actual.GameOptions.SkipOpeningCinematic, "input recording host option");
        AssertTrue(actual.GameOptions.Invincibility, "input recording invincibility option");
        AssertTrue(actual.GameOptions.InfiniteAmmo, "input recording infinite-ammo option");
        AssertTrue(actual.GameOptions.PreventEscapeTimeout, "input recording escape floor option");
        AssertEqual((ushort?)0, actual.GameOptions.EndingTimeOverrideMinutes, "recording preserves zero-minute ending override");
        AssertEqual(MapRevealMode.Secret, actual.GameOptions.MapReveal,
            "input recording map-reveal mode");
        AssertTrue(digest.SequenceEqual(actual.RomSha256), "input recording ROM digest");
        AssertTrue(actual.ContentIdentity is null, "version-one input recording has no content identity");
        AssertTrue(saveRam.SequenceEqual(actual.InitialSaveRam), "input recording SRAM seed");
        AssertTrue(inputs.SequenceEqual(actual.ControllerInputs), "input recording frame words");

        // The frame count is an integrity boundary, not merely a preallocation hint. A
        // partially flushed file must fail loudly instead of replaying an incomplete tail.
        byte[] truncated = bytes.ToArray()[..^1];
        try
        {
            ControllerInputRecording.Read(new MemoryStream(truncated));
            throw new InvalidOperationException("Truncated input recording was accepted.");
        }
        catch (InvalidDataException)
        {
            // Expected strict-length rejection.
        }

        var identity = new GameContentIdentitySnapshot
        {
            FormatVersion = 7,
            CompiledDefinitionsBuildId = Guid.Parse("01234567-89ab-cdef-0123-456789abcdef"),
            AudioContentSha256 = Enumerable.Repeat((byte)0x11, 32).ToArray(),
            MapContentSha256 = Enumerable.Repeat((byte)0x22, 32).ToArray(),
            ProjectileContentSha256 = Enumerable.Repeat((byte)0x33, 32).ToArray(),
            CompositeSha256 = Enumerable.Repeat((byte)0x44, 32).ToArray(),
        };
        ControllerInputRecording expectedV2 = expected with { ContentIdentity = identity };
        using var v2Bytes = new MemoryStream();
        expectedV2.Write(v2Bytes);
        AssertEqual(2u, BinaryPrimitives.ReadUInt32LittleEndian(v2Bytes.ToArray().AsSpan(8)),
            "identified input recording uses version two");
        v2Bytes.Position = 0;
        ControllerInputRecording actualV2 = ControllerInputRecording.Read(v2Bytes);
        GameContentIdentitySnapshot actualIdentity = actualV2.ContentIdentity
            ?? throw new InvalidOperationException("Version-two input recording lost its content identity.");
        AssertEqual(identity.FormatVersion, actualIdentity.FormatVersion,
            "input recording content-identity format");
        AssertEqual(identity.CompiledDefinitionsBuildId, actualIdentity.CompiledDefinitionsBuildId,
            "input recording compiled-definition build");
        AssertTrue(identity.AudioContentSha256.SequenceEqual(actualIdentity.AudioContentSha256),
            "input recording audio identity");
        AssertTrue(identity.MapContentSha256.SequenceEqual(actualIdentity.MapContentSha256),
            "input recording map identity");
        AssertTrue(identity.ProjectileContentSha256.SequenceEqual(actualIdentity.ProjectileContentSha256),
            "input recording projectile identity");
        AssertTrue(identity.CompositeSha256.SequenceEqual(actualIdentity.CompositeSha256),
            "input recording aggregate identity");

        AssertThrows<InvalidDataException>(
            () => (expected with
            {
                ContentIdentity = identity with { AudioContentSha256 = new byte[31] },
            }).Write(new MemoryStream()),
            "input recording rejects malformed content digest");

        byte[] truncatedV2 = v2Bytes.ToArray()[..67];
        AssertThrows<EndOfStreamException>(
            () => ControllerInputRecording.Read(new MemoryStream(truncatedV2)),
            "input recording rejects a truncated version-two identity header");

        Console.WriteLine(
            "  Controller input recording: v1 compatibility, v2 installed identity, " +
            "deterministic seed, words, and strict truncation guards pass.");
    }
}
