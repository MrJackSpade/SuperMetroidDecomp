using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.BossBodies;

/// <summary>Every boss's installed body sheet (#1466), by boss.</summary>
public sealed class BossBodySheets
{
    /// <summary>The sheets by boss.</summary>
    private readonly IReadOnlyDictionary<BossBodyOwner, BossBodySheet> sheets;

    /// <summary>Holds one sheet for each boss.</summary>
    /// <exception cref="InvalidDataException">A boss has no sheet, or two.</exception>
    public BossBodySheets(IEnumerable<BossBodySheet> sheets)
    {
        var byOwner = new Dictionary<BossBodyOwner, BossBodySheet>();
        foreach (BossBodySheet sheet in Ensure.NotNull(sheets))
        {
            if (!byOwner.TryAdd(sheet.Owner, sheet))
                throw new InvalidDataException($"{sheet.Owner} has two body sheets.");
        }
        foreach (BossBodyOwner owner in Enum.GetValues<BossBodyOwner>())
        {
            if (!byOwner.ContainsKey(owner))
                throw new InvalidDataException($"{owner} has no body sheet.");
        }
        this.sheets = byOwner;
    }

    /// <summary><paramref name="owner"/>'s sheet.</summary>
    public BossBodySheet Of(BossBodyOwner owner) => sheets[owner];

    /// <summary>The identity of every sheet's pixels and frames, in boss order.</summary>
    public string ContentIdentity => SelectedPresentationHash.Create(nameof(BossBodySheets), content =>
    {
        foreach (BossBodyOwner owner in Enum.GetValues<BossBodyOwner>())
        {
            content.AppendName("boss", owner.ToString());
            sheets[owner].Sheet.AppendIdentity(content);
        }
    });
}
