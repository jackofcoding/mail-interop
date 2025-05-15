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

  public async Task Open(bool write, CancellationToken cancellationToken)
  {
    var desiredAccess = write ? FolderAccess.ReadWrite : FolderAccess.ReadOnly;
    switch (_folder.IsOpen)
    {
      case false:
        await _folder.OpenAsync(desiredAccess, cancellationToken).ConfigureAwait(false);
        break;
      
      case true when _folder.Access != desiredAccess:
        throw new NotImplementedException("Switching between read and write access is not implemented.");
    }
  }
  
  public async Task<IEnumerable<MailMetadata>> FetchAllMetadata(CancellationToken cancellationToken)
  {
    if (!_folder.IsOpen)
      throw new MailboxFolderNotOpenedException(_name);
    
    const MessageSummaryItems flags = MessageSummaryItems.UniqueId | MessageSummaryItems.Envelope | MessageSummaryItems.Flags;
    IList<IMessageSummary>? summaries = await _folder.FetchAsync(0, -1, flags, cancellationToken: cancellationToken).ConfigureAwait(false);
    
    return summaries
      .Where(x => (x.Flags & MessageFlags.Deleted) == 0)
      .Select(x => new MailMetadata(x));
  }
  
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
