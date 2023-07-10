using System.Threading.Tasks;
using MediaBrowser.Controller.Plugins;
using MediaBrowser.Controller.Session;

namespace q12.JellyfinPlugin.LocalFilesPreferred;

public sealed class SessionWatcher : IServerEntryPoint
{
    private readonly ISessionManager _sessionManager;

    public SessionWatcher(ISessionManager sessionManager)
    {
        _sessionManager = sessionManager;
    }

    public Task RunAsync()
    {
        _sessionManager.SessionStarted += SessionManagerOnSessionStarted;
        _sessionManager.SessionEnded += SessionManagerOnSessionEnded;

        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _sessionManager.SessionEnded -= SessionManagerOnSessionEnded;
        _sessionManager.SessionStarted -= SessionManagerOnSessionStarted;
    }

    private static void SessionManagerOnSessionStarted(object? sender, SessionEventArgs e)
    {
        if (e.SessionInfo.Client == "Kodi")
        {
            LocalFilesPreferredPlugin.SendRealPath = false;
        }
    }

    private static void SessionManagerOnSessionEnded(object? sender, SessionEventArgs e)
    {
        if (e.SessionInfo.Client == "Kodi")
        {
            LocalFilesPreferredPlugin.SendRealPath = true;
        }
    }
}
