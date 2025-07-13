using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Interaction;
using Content.Shared.Labels.Components;
using Content.Shared.Popups;
using Content.Shared.Whitelist;
using Robust.Shared.Network;

namespace Content.Shared._Stories.Renames;

public abstract class SharedHandRenamerSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelistSystem = default!;
    [Dependency] private readonly TemporaryRenameSystem _renameSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HandRenamerComponent, AfterInteractEvent>(AfterInteractOn);
    }

    private void AfterInteractOn(EntityUid uid, HandRenamerComponent handRenamer, AfterInteractEvent args)
    {
        TryComp<HandLabelerComponent>(uid, out var handLabeler);
        if (handLabeler == null)
            return;

        if (args.Target is not { Valid: true } target || _whitelistSystem.IsWhitelistFail(handLabeler.Whitelist, target) || !args.CanReach)
            return;

        Renaming(uid, target, args.User, handRenamer);
    }

    private void Renaming(EntityUid uid, EntityUid target, EntityUid user, HandRenamerComponent handRenamer)
    {
        AddNewNameTo(uid, handRenamer, target, out var result);
        if (result == null)
            return;

        _popupSystem.PopupClient(result, user, user);

        // Log renaming
        _adminLogger.Add(LogType.Action, LogImpact.Low,
            $"{ToPrettyString(user):user} renamed {ToPrettyString(target):target} with {ToPrettyString(uid):labeler}");
    }

    private void AddNewNameTo(EntityUid uid, HandRenamerComponent? handRenamer, EntityUid target, out string? result)
    {
        if (handRenamer == null || handRenamer.Charge <= 0)
        {
            RemComp<HandRenamerComponent>(uid);
            result = null;
            return;
        }

        TryComp<HandLabelerComponent>(uid, out var handLabeler);
        if (handLabeler == null || !Resolve(uid, ref handLabeler))
        {
            result = null;
            return;
        }

        if (handLabeler.AssignedLabel == string.Empty)
        {
            if (_netManager.IsServer)
                _renameSystem.Rename(target, null, handRenamer.RenameLifeTime);
            result = Loc.GetString("stories-hand-renamer-successfully-removed");
            return;
        }

        if (_netManager.IsServer)
        {
            handRenamer.Charge -= 1;
            _renameSystem.Rename(target, handLabeler.AssignedLabel, handRenamer.RenameLifeTime);
        }

        Dirty(uid, handRenamer);
        result = Loc.GetString("stories-hand-renamer-successfully-applied", ("charge", handRenamer.Charge - 1));
    }
}
