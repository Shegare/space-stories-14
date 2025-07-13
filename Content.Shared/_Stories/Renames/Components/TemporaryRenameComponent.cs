using Robust.Shared.GameStates;

namespace Content.Shared._Stories.Renames;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(TemporaryRenameSystem))]
public sealed partial class TemporaryRenameComponent : Component
{
    [DataField, AutoNetworkedField]
    public string? CurrentName { get; set; }

    [DataField, AutoNetworkedField]
    public float? LifeTime;
}
