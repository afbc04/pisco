using System.Globalization;
using Pisco.Domain;

namespace Pisco.Components.Finance;

public static class WalletMoney
{
    public static string Badge(WalletStatus status)
    {
        var color = WalletEditor.StatusColor(status);
        return $"badge rounded-pill border text-{color}-emphasis bg-{color}-subtle border-{color}-subtle";
    }

    public static string Format(long cents) => (cents / 100m).ToString("N2", CultureInfo.CurrentCulture) + " €";
}
