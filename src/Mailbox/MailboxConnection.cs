using System.Net.Sockets;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Security;

namespace MailInterop.Mailbox;

public sealed class MailboxConnection : IAsyncDisposable, IDisposable
{
  private readonly MailboxConfiguration _configuration;
  private readonly ImapClient _client;

  public bool IsOnline => _client.IsAuthenticated;

  public MailboxConnection(MailboxConfiguration configuration)
  {
    _configuration = configuration;
    _client = new ImapClient();
  }

  public async Task ConnectAsync(CancellationToken cancellationToken)
  {
    try
    {
      if (!_client.IsConnected)
        await _client.ConnectAsync(_configuration.Host, 993, SecureSocketOptions.SslOnConnect, cancellationToken).ConfigureAwait(false);
    }
    catch (Exception exception)
    {
      if (exception is not SocketException { SocketErrorCode: SocketError.Success })
        throw new MailboxConnectionException(_configuration.Host, exception);
    }

    try
    {
      if (!_client.IsAuthenticated)
        await _client.AuthenticateAsync(_configuration.Username, _configuration.Password, cancellationToken).ConfigureAwait(false);
    }
    catch (Exception exception)
    {
      throw new MailboxAuthenticationException(_configuration.Username, exception);
    }
  }

  public async Task<MailboxFolder> GetMailboxFolder(string[] pathParts, CancellationToken cancellationToken)
  {
    if (!IsOnline)
      throw new MailboxConnectionNotOnlineException();
    
    var path = String.Join(_client.Inbox.DirectorySeparator, pathParts);
    return await GetMailboxFolder(path, cancellationToken).ConfigureAwait(false);
  }

  public async Task<MailboxFolder> GetMailboxFolder(string path, CancellationToken cancellationToken)
  {
    if (!IsOnline)
      throw new MailboxConnectionNotOnlineException();

    IMailFolder folder;
    try
    {
      folder = await _client.GetFolderAsync(path, cancellationToken).ConfigureAwait(false);
    }
    catch (FolderNotFoundException exception)
    {
      throw new MailboxFolderNotfoundException(path, exception);
    }

    var pathSpan = path.AsSpan();
    var lastDirectorySeparatorIndex = pathSpan.LastIndexOf(folder.DirectorySeparator);
    var name = lastDirectorySeparatorIndex == -1
      ? path
      : new String(pathSpan[(lastDirectorySeparatorIndex + 1)..]);

    return new MailboxFolder(name, path, _client, folder);
  }

  #region Disposing

  public async ValueTask DisposeAsync()
  {
    if (_client.IsConnected)
      await _client.DisconnectAsync(true).ConfigureAwait(false);

    _client.Dispose();

    Dispose(false);
  }

  public void Dispose() => Dispose(true);

  private void Dispose(bool disposing)
  {
    if (!disposing)
      return;

    if (_client.IsConnected)
      _client.Disconnect(true);

    _client.Dispose();
  }

  #endregion
}
