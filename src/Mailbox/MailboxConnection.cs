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
    if (!_client.IsConnected)
      await _client.ConnectAsync(_configuration.Host, 993, SecureSocketOptions.SslOnConnect, cancellationToken).ConfigureAwait(false);

    if (!_client.IsAuthenticated)
      await _client.AuthenticateAsync(_configuration.Username, _configuration.Password, cancellationToken).ConfigureAwait(false);
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
