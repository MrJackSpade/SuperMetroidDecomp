using Godot;

namespace Engine.Ui;

/// <summary>
/// A group of drawn things shown through one grading of its colours: whatever is put under it is drawn together,
/// and every colour of the result is shown as the colour of a ramp nearest it in brightness. It is how a scene is
/// given one look, as a filter on a camera gives one, with nothing in the scene knowing of it. With no ramp given
/// the group shows its things as they are. It is see-through where its things are, and its own tint still tints it.
/// </summary>
public partial class GradedGroup : CanvasGroup
{
    /// <summary>The most colours a ramp may have.</summary>
    public const int MostColours = 16;

    private const string Code = """
        shader_type canvas_item;
        render_mode unshaded;
        uniform sampler2D drawn : hint_screen_texture, repeat_disable, filter_nearest;
        uniform int ramp_count = 0;
        uniform vec3 ramp[16];
        const vec3 WEIGHTS = vec3(0.299, 0.587, 0.114);
        void fragment() {
            // What the group drew is held with its colours already multiplied by how solid they are.
            vec4 shown = textureLod(drawn, SCREEN_UV, 0.0);
            if (shown.a > 0.0001) {
                shown.rgb /= shown.a;
            }
            float brightness = dot(shown.rgb, WEIGHTS);
            float nearest = 10.0;
            vec3 graded = shown.rgb;
            for (int i = 0; i < 16; i++) {
                if (i >= ramp_count) {
                    break;
                }
                float apart = abs(dot(ramp[i], WEIGHTS) - brightness);
                if (apart < nearest) {
                    nearest = apart;
                    graded = ramp[i];
                }
            }
            COLOR *= vec4(graded, shown.a);
        }
        """;

    private ShaderMaterial? _grading;

    /// <summary>
    /// Shows the group's things through <paramref name="ramp"/>: each colour as the ramp's colour nearest it in
    /// brightness. An empty ramp shows them as they are.
    /// </summary>
    /// <exception cref="ArgumentException">The ramp has more colours than a grading can hold.</exception>
    public void Grade(IReadOnlyList<Color> ramp)
    {
        ArgumentNullException.ThrowIfNull(ramp);
        if (ramp.Count > MostColours)
            throw new ArgumentException($"A grading holds at most {MostColours} colours, and this ramp has {ramp.Count}.", nameof(ramp));
        if (ramp.Count == 0)
        {
            Material = null;
            return;
        }

        var colours = new Vector3[MostColours];
        for (int index = 0; index < ramp.Count; index++)
            colours[index] = new Vector3(ramp[index].R, ramp[index].G, ramp[index].B);
        _grading ??= new ShaderMaterial { Shader = new Shader { Code = Code } };
        _grading.SetShaderParameter("ramp_count", ramp.Count);
        _grading.SetShaderParameter("ramp", colours);
        Material = _grading;
    }
}
