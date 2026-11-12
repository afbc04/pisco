namespace Pisco.Services;

public static class AppRoutes
{
    public const string Home = "";
    public const string Demo = "demo";
    public const string Parameters = "parameters";

    public const string LoginParam = "login";
    public const string LoginFailed = "failed";
    public const string ReturnUrlParam = "returnUrl";

    public static string Login(string? returnUrl, bool failed = false) =>
        $"/?{LoginParam}={(failed ? LoginFailed : "1")}&{ReturnUrlParam}={Uri.EscapeDataString(LocalOrHome(returnUrl))}";

    public static string LocalOrHome(string? url) =>
        url is ['/', not '/' and not '\\', ..] or "/" ? url : "/";
}
