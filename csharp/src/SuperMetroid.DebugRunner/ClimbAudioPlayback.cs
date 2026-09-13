using SuperMetroid.Core.Audio;

/// <summary>Mixed playback and a death-request-muted counterfactual for #16.</summary>
internal sealed class ClimbAudioPlayback(string directory, ClimbNativeAudioReference? native = null)
{
    private readonly ExtractedAudioAssetCatalog assets = ExtractedAudioAssetCatalog.Load(directory);
    private readonly ManagedSpcPlayer actual = new();
    private readonly ManagedSpcPlayer control = new();
    private readonly short[] actualPcm = new short[1600], controlPcm = new short[1600];
    public int ChangedFrames { get; private set; }
    public int MaximumDifference { get; private set; }
    public CartridgeAudioAcknowledgements Acknowledgements =>
        new(actual.ReadPort(0), actual.ReadPort(1), actual.ReadPort(2), actual.ReadPort(3));

    public void Step(IReadOnlyList<CartridgeAudioCommand> commands)
    {
        foreach (var command in commands)
        {
            native?.Apply(command, assets);
            if (command.Kind == CartridgeAudioCommandKind.Upload)
            {
                var stream = assets.GetUpload(command.UploadAddress).ToArray();
                actual.Upload(stream);
                control.Upload(stream);
                actual.SetSampleBank(assets.GetSampleBank(command.UploadAddress));
                control.SetSampleBank(assets.GetSampleBank(command.UploadAddress));
            }
            else
            {
                actual.WritePort(command.Port, command.Value);
                // Preserve every other command, including impact, music and port clears.
                // Only remove the reported death cue, not its entire sound library.
                if (command.Port != 2 || command.Value != 0x24)
                    control.WritePort(command.Port, command.Value);
            }
        }
        actual.GenerateFrame(actualPcm);
        native?.Verify(actualPcm, actual);
        control.GenerateFrame(controlPcm);
        if (!actualPcm.AsSpan().SequenceEqual(controlPcm)) ChangedFrames++;
        for (int i = 0; i < actualPcm.Length; i++)
            MaximumDifference = Math.Max(MaximumDifference, Math.Abs(actualPcm[i] - controlPcm[i]));
    }
}
