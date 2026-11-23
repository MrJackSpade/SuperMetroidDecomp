using Godot;

namespace Engine.Ui;

/// <summary>
/// A picture that can be shown in coarse blocks: every block of so many pixels each way takes the colour of its
/// top-left pixel. At a block width of 1 it is the picture as it is.
/// </summary>
public partial class MosaicPicture : Sprite2D
{
    private const string Code = """
        shader_type canvas_item;
        uniform float block = 1.0;
        void fragment() {
            vec2 size = 1.0 / TEXTURE_PIXEL_SIZE;
            vec2 corner = floor(UV * size / block) * block;
            COLOR = texture(TEXTURE, (corner + 0.5) / size);
        }
        """;

    private int _blockWidth = 1;

    /// <summary>How many pixels wide and tall each block is.</summary>
    [Export]
    public int BlockWidth
    {
        get => _blockWidth;
        set
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(value, 1);
            _blockWidth = value;
            (Material as ShaderMaterial)?.SetShaderParameter("block", (float)value);
        }
    }

    /// <inheritdoc/>
    public override void _Ready()
    {
        TextureFilter = TextureFilterEnum.Nearest;
        var material = new ShaderMaterial { Shader = new Shader { Code = Code } };
        material.SetShaderParameter("block", (float)_blockWidth);
        Material = material;
    }
}
