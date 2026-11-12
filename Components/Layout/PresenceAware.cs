using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using Pisco.Services;

namespace Pisco.Components.Layout;

public abstract class PresenceAware : ComponentBase, IDisposable
{
    [Inject] protected AdminPresence Presence { get; set; } = default!;
    [Inject] protected IStringLocalizer<Resources> R { get; set; } = default!;
    [Inject] private ConfigService Config { get; set; } = default!;

    [Parameter] public bool SignedIn { get; set; }

    protected string? AdminName { get; private set; }

    private Timer? clock;

    protected string PresenceState => SignedIn ? "is-self" : Presence.IsOnline ? "is-online" : "is-offline";

    protected string? DotState => SignedIn ? "is-soft" : Presence.IsOnline ? null : "is-off";

    protected string StatusText =>
        SignedIn ? R["Presence_Online"]
        : Presence.IsOnline ? R["Presence_OnlineNow"]
        : agoText() is { } ago ? R["Presence_LastSeen", ago]
        : R["Presence_Offline"];

    protected string ShortText =>
        SignedIn || Presence.IsOnline ? R["Presence_Online"]
        : agoText() is { } ago ? R["Presence_Ago", ago]
        : R["Presence_Offline"];

    private string? agoText()
    {
        if (Presence.LastSeen is not { } seen)
            return null;
        var ago = DateTimeOffset.UtcNow - seen;
        return ago.TotalMinutes < 1 ? R["Presence_JustNow"]
            : ago.TotalHours < 1 ? R["Presence_Minutes", (int)ago.TotalMinutes]
            : ago.TotalDays < 1 ? R["Presence_Hours", (int)ago.TotalHours]
            : R["Presence_Days", (int)ago.TotalDays];
    }

    protected override async Task OnInitializedAsync()
    {
        Presence.Changed += refresh;
        AdminName = (await Config.GetAsync())?.Username;
    }

    protected override void OnAfterRender(bool firstRender)
    {
        if (firstRender && !SignedIn)
            clock = new Timer(_ => refresh(), null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    private void refresh() => InvokeAsync(StateHasChanged);

    public virtual void Dispose()
    {
        Presence.Changed -= refresh;
        clock?.Dispose();
    }
}
