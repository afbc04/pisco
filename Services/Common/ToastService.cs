namespace Pisco.Services;

public enum ToastKind { Success, Error, Warning, Info }

public sealed record Toast(string Message, ToastKind Kind, string? Title = null);

public class ToastService
{
    public event Action<Toast>? Shown;

    public void Success(string message, string? title = null) => Shown?.Invoke(new Toast(message, ToastKind.Success, title));
    public void Error(string message, string? title = null) => Shown?.Invoke(new Toast(message, ToastKind.Error, title));
    public void Warning(string message, string? title = null) => Shown?.Invoke(new Toast(message, ToastKind.Warning, title));
    public void Info(string message, string? title = null) => Shown?.Invoke(new Toast(message, ToastKind.Info, title));
}
