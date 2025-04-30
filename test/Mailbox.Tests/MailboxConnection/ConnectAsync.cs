using Microsoft.Extensions.Configuration;

namespace MailInterop.Mailbox.Tests.MailboxConnection;

public class ConnectAsync
{
  private static string _host = null!;
  private static string _username = null!;
  private static string _password = null!;

  [Before(Class)]
  public static ValueTask GetSecrets()
  {
    var builder = new ConfigurationBuilder()
      .AddUserSecrets<ConnectAsync>();
    var configuration = builder.Build();

    _host = configuration["ConnectAsync:Host"] ?? throw new ArgumentNullException(nameof(_host));
    _username = configuration["ConnectAsync:Username"] ?? throw new ArgumentNullException(nameof(_username));
    _password = configuration["ConnectAsync:Password"] ?? throw new ArgumentNullException(nameof(_password));

    return ValueTask.CompletedTask;
  }

  [Test]
  public async Task ValidConfiguration_ConnectsToServer()
  {
    var configuration = new MailboxConfiguration(_host, _username, _password);
    await using var mailboxConnection = new Mailbox.MailboxConnection(configuration);

    await mailboxConnection.ConnectAsync(CancellationToken.None).ConfigureAwait(false);

    await Assert.That(mailboxConnection.IsOnline).IsTrue();
  }
  
  [Test]
  public async Task InvalidHost_FailsToConnectToServer()
  {
    var configuration = new MailboxConfiguration("wrong_host", _username, _password);
    await using var mailboxConnection = new Mailbox.MailboxConnection(configuration);

    var exception = await Assert.ThrowsAsync<MailboxConnectionException>(async () => await mailboxConnection.ConnectAsync(CancellationToken.None).ConfigureAwait(false));
    await Assert.That(exception.Host).IsEqualTo(configuration.Host);
  }
  
  [Test]
  public async Task InvalidUsername_FailsToConnectToServer()
  {
    var configuration = new MailboxConfiguration(_host, "wrong_username", _password);
    await using var mailboxConnection = new Mailbox.MailboxConnection(configuration);

    var exception = await Assert.ThrowsAsync<MailboxAuthenticationException>(async () => await mailboxConnection.ConnectAsync(CancellationToken.None).ConfigureAwait(false));
    await Assert.That(exception.Username).IsEqualTo(configuration.Username);
  }
  
  [Test]
  public async Task InvalidPassword_FailsToConnectToServer()
  {
    var configuration = new MailboxConfiguration(_host, _username, "wrong_password");
    await using var mailboxConnection = new Mailbox.MailboxConnection(configuration);

    var exception = await Assert.ThrowsAsync<MailboxAuthenticationException>(async () => await mailboxConnection.ConnectAsync(CancellationToken.None).ConfigureAwait(false));
    await Assert.That(exception.Username).IsEqualTo(configuration.Username);
  }
}
