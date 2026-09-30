namespace SuperMetroid.Core.Assets;

public sealed partial class SamusBodyArtworkCatalog
{
    private string CreateContentIdentity() => SelectedPresentationHash.Create(nameof(SamusBodyArtworkCatalog), content =>
    {
        content.AppendWords("top pointers", topPointers);
        content.AppendWords("bottom pointers", bottomPointers);
        content.AppendWords("pose pointers", posePointers);
        content.Append("graphics y offsets", graphicsYOffsets.Select(value => unchecked((byte)value)).ToArray());
        content.AppendWords("landing y offsets", landingYOffsets);
        content.Append("posture y offsets", postureYOffsets.Select(value => unchecked((byte)value)).ToArray());
        content.Append("drained y offsets", drainedYOffsets.Select(value => unchecked((byte)value)).ToArray());
        foreach (SamusBodyFrameSelection frame in frames)
        {
            content.Append("top set", frame.TopSet);
            content.Append("top position", frame.TopPosition);
            content.Append("bottom set", frame.BottomSet);
            content.Append("bottom position", frame.BottomPosition);
        }
        foreach ((int address, SamusBodyTileDefinition definition) in definitionsByAddress.OrderBy(pair => pair.Key))
        {
            content.Append("definition address", address);
            content.Append("source address", definition.SourceAddress);
            content.Append("first transfer size", definition.FirstSize);
            content.Append("second transfer size", definition.SecondSize);
            content.Append("characters", definition.Planar.Span);
        }
        content.Append("spritemaps", Convert.FromHexString(Spritemaps.ContentIdentity));
        content.Append("atmosphere", Convert.FromHexString(Atmosphere.ContentIdentity));
        content.Append("death palettes", Convert.FromHexString(DeathPalettes.ContentIdentity));
        content.Append("death tiles", Convert.FromHexString(DeathTiles.ContentIdentity));
        content.Append("arm cannon", Convert.FromHexString(ArmCannon.ContentIdentity));
    });
}
