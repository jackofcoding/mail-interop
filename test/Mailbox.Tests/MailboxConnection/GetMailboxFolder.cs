using Microsoft.Extensions.Configuration;

namespace MailInterop.Mailbox.Tests.MailboxConnection;

public class GetMailboxFolder
{
  private static Mailbox.MailboxConnection _connection = null!;
  
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

    _connection = mailboxConnection;
  }

  [After(Class)]
  public static async ValueTask DisconnectAsync(CancellationToken cancellationToken)
  {
    await _connection.DisposeAsync().ConfigureAwait(false);
  }
  
  [Test]
  public async Task ValidPath_ReturnsMailboxFolder(CancellationToken cancellationToken)
  {
    var path = "INBOX";
    var folder = await _connection.GetMailboxFolder(path, cancellationToken).ConfigureAwait(false);
    
    await Assert.That(folder).IsNotNull();
  }

  [Test]
  public async Task InvalidPath_ThrowsError(CancellationToken cancellationToken)
  {
    var path = "DoesNotExist";
    
    var exception = await Assert.ThrowsAsync<MailboxFolderNotfoundException>(async () => await _connection.GetMailboxFolder(path, cancellationToken).ConfigureAwait(false));
    await Assert.That(exception.Path).IsEqualTo(path);
  }

  [Test]
  public async Task ValidPathParts_ReturnsMailboxFolder(CancellationToken cancellationToken)
  {
    var pathParts = new[] { "INBOX", "UnitTestMails" };
    var folder = await _connection.GetMailboxFolder(pathParts, cancellationToken).ConfigureAwait(false);
    
    await Assert.That(folder).IsNotNull();
  }

  [Test]
  public async Task InvalidPathParts_ThrowsError(CancellationToken cancellationToken)
  {
    var pathParts = new[] { "DoesNotExist", "AlsoDoesNotExist" };
    var expectedPath = "DoesNotExist.AlsoDoesNotExist";
    
    var exception = await Assert.ThrowsAsync<MailboxFolderNotfoundException>(async () => await _connection.GetMailboxFolder(pathParts, cancellationToken).ConfigureAwait(false));
    await Assert.That(exception.Path).IsEqualTo(expectedPath);
  }

  [Test]
  public async Task PartialInvalidPathParts_ThrowsError(CancellationToken cancellationToken)
  {
    var pathParts = new[] { "UnitTestMails", "DoesNotExist" };
    var expectedPath = "UnitTestMails.DoesNotExist";
    
    var exception = await Assert.ThrowsAsync<MailboxFolderNotfoundException>(async () => await _connection.GetMailboxFolder(pathParts, cancellationToken).ConfigureAwait(false));
    await Assert.That(exception.Path).IsEqualTo(expectedPath);
  }
}
