using MailKit.Net.Imap;

namespace MailInterop.Mailbox;

public sealed class MailboxConnection : IAsyncDisposable, IDisposable
{
  private readonly MailboxConfiguration _configuration;
  private readonly ImapClient _client;

  public bool IsOnline => _client.IsConnected;

  public MailboxConnection(MailboxConfiguration configuration)
  {
    _configuration = configuration;
    _client = new ImapClient();
  }

  public async Task ConnectAsync(CancellationToken cancellationToken)
  {
    if (!_client.IsConnected)
      await _client.ConnectAsync(_configuration.Host, _configuration.Port, _configuration.UseSSL, cancellationToken);
  }

  #region Disposing

  public async ValueTask DisposeAsync()
  {
    if (_client.IsConnected)
      await _client.DisconnectAsync(true).ConfigureAwait(false);

    _client.Dispose();

    Dispose(false);
    GC.SuppressFinalize(this);
  }

  public void Dispose()
  {
    Dispose(true);
    GC.SuppressFinalize(this);
  }

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
