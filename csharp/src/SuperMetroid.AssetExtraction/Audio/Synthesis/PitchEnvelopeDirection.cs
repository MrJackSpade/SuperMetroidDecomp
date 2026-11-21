namespace SuperMetroid.AssetExtraction.Audio.Synthesis;

/// <summary>Which way a music voice's note-on pitch envelope runs (#2560).</summary>
internal enum PitchEnvelopeDirection
{
    /// <summary>The note starts offset by the envelope's slide and moves to its own pitch.</summary>
    From,
    /// <summary>The note starts at its own pitch and moves by the envelope's slide.</summary>
    To,
}
