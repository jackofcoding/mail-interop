using MailKit;
using MailKit.Net.Imap;

namespace MailInterop.Mailbox;

public sealed class MailboxFolder : IAsyncDisposable, IDisposable
{
  private readonly string _name;
  private readonly string _path;
  private readonly ImapClient _client;
  private readonly IMailFolder _folder;

  internal MailboxFolder(string name, string path, ImapClient client, IMailFolder folder)
  {
    _name = name;
    _path = path;
    _client = client;
    _folder = folder;
  }
  
  public static async Task<MailboxFolder> Create(string path, MailboxConnection connection, CancellationToken cancellationToken) => await connection.GetMailboxFolder(path, cancellationToken).ConfigureAwait(false);

  
  #region Disposing

  public async ValueTask DisposeAsync()
  {
    if (_folder.IsOpen)
      await _folder.CloseAsync(true).ConfigureAwait(false);

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
    
    if (_folder.IsOpen)
      _folder.Close(true);

    if (_client.IsConnected)
      _client.Disconnect(true);

    _client.Dispose();
  }

  #endregion
}
