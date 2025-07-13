using Content.Shared.NameModifier.EntitySystems;

namespace Content.Shared._Stories.Renames;

public sealed class TemporaryRenameSystem : EntitySystem
{
    [Dependency] private readonly NameModifierSystem _nameModifier = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TemporaryRenameComponent, MapInitEvent>(OnLabelCompMapInit);
        SubscribeLocalEvent<TemporaryRenameComponent, RefreshNameModifiersEvent>(OnRefreshNameModifiers);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<TemporaryRenameComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.LifeTime == null || comp.LifeTime <= 0)
            {
                comp.CurrentName = null;
                _nameModifier.RefreshNameModifiers(uid);
                RemComp<TemporaryRenameComponent>(uid);
            }
            else
                comp.LifeTime -= frameTime;
        }
    }

    private void OnLabelCompMapInit(Entity<TemporaryRenameComponent> ent, ref MapInitEvent args)
    {
        if (!string.IsNullOrEmpty(ent.Comp.CurrentName))
        {
            ent.Comp.CurrentName = Loc.GetString(ent.Comp.CurrentName);
            Dirty(ent);
        }

        _nameModifier.RefreshNameModifiers(ent.Owner);
    }

    public void Rename(EntityUid uid, string? text, float lifeTime)
    {
        var rename = EnsureComp<TemporaryRenameComponent>(uid);

        rename.LifeTime = lifeTime;
        rename.CurrentName = text;
        _nameModifier.RefreshNameModifiers(uid);

        Dirty(uid, rename);
    }

    private void OnRefreshNameModifiers(Entity<TemporaryRenameComponent> entity, ref RefreshNameModifiersEvent args)
    {
        if (!string.IsNullOrEmpty(entity.Comp.CurrentName))
            args.AddModifier(entity.Comp.CurrentName);
    }
}
