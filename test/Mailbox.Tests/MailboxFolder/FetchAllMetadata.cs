using MailInterop.Mailbox.Tests.MailboxConnection;
using Microsoft.Extensions.Configuration;

namespace MailInterop.Mailbox.Tests.MailboxFolder;

public class FetchAllMetadata
{
  private static Mailbox.MailboxFolder _folder = null!;
  
  [Before(Class)]
  public static async ValueTask GetSecrets(CancellationToken cancellationToken)
  {
    var builder = new ConfigurationBuilder()
      .AddUserSecrets<ConnectAsync>();
    var configuration = builder.Build();

    var host = configuration["ConnectAsync:Host"] ?? throw new ArgumentNullException("host");
    var username = configuration["ConnectAsync:Username"] ?? throw new ArgumentNullException("username");
    var password = configuration["ConnectAsync:Password"] ?? throw new ArgumentNullException("password");
    
    var mailboxConfiguration = new MailboxConfiguration(host, username, password);
    var mailboxConnection = new Mailbox.MailboxConnection(mailboxConfiguration);
    await mailboxConnection.ConnectAsync(cancellationToken).ConfigureAwait(false);

    var connection = mailboxConnection;
    _folder = await connection.GetMailboxFolder("INBOX.UnitTestMails", cancellationToken).ConfigureAwait(false);
  }
  
  [After(Class)]
  public static async ValueTask DisconnectAsync(CancellationToken cancellationToken)
  {
    await _folder.DisposeAsync().ConfigureAwait(false);
  }
  
  [Test]
  public async Task FolderNotOpened_ThrowsError(CancellationToken cancellationToken)
  {
    var exception = await Assert.ThrowsAsync<MailboxFolderNotOpenedException>(async () => await _folder.FetchAllMetadata(cancellationToken).ConfigureAwait(false));
    await Assert.That(exception.Name).IsEqualTo("UnitTestMails");
  }

  [Test]
  [DependsOn(nameof(FolderNotOpened_ThrowsError))]
  public async Task Open(CancellationToken cancellationToken)
  {
    await _folder.Open(true, cancellationToken).ConfigureAwait(false);
  }
  
  [Test]
  [DependsOn(nameof(Open))]
  public async Task FetchAllMetadata_ReturnsMailMetadata(CancellationToken cancellationToken)
  {
    var metadata = await _folder.FetchAllMetadata(cancellationToken).ConfigureAwait(false);
    var metadataList = metadata.ToList();
    
    await Assert.That(metadataList).HasCount(3);
  }
}
