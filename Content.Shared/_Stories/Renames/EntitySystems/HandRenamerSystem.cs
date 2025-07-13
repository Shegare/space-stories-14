using Robust.Shared.GameStates;

namespace Content.Shared._Stories.Renames;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(SharedHandRenamerSystem))]
public sealed partial class HandRenamerComponent : Component
{
    [DataField, AutoNetworkedField]
    public int Charge = 5;

    [DataField, AutoNetworkedField]
    public float LifeTime = 180f; // 3 minutes
}
