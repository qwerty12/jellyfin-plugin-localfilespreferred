using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Controller.Session;
using Microsoft.Extensions.Hosting;

namespace q12.JellyfinPlugin.LocalFilesPreferred;

public sealed class SessionWatcher : IHostedService
{
    private readonly ISessionManager _sessionManager;

    public SessionWatcher(ISessionManager sessionManager)
    {
        _sessionManager = sessionManager;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _sessionManager.SessionStarted += SessionManagerOnSessionStarted;
        _sessionManager.SessionEnded += SessionManagerOnSessionEnded;

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _sessionManager.SessionEnded -= SessionManagerOnSessionEnded;
        _sessionManager.SessionStarted -= SessionManagerOnSessionStarted;

        return Task.CompletedTask;
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
