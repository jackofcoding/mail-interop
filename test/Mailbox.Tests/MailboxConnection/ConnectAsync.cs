using Microsoft.Extensions.Configuration;

namespace MailInterop.Mailbox.Tests.MailboxConnection;

public class ConnectAsync
{
  private static string? _host;

  [Before(Class)]
  public static ValueTask GetSecrets()
  {
    var builder = new ConfigurationBuilder()
      .AddUserSecrets<ConnectAsync>();
    var configuration = builder.Build();

    _host = configuration["ConnectAsync:Host"];

    return ValueTask.CompletedTask;
  }

  [Test]
  public async Task ValidConfiguration_ConnectsToServer()
  {
    var configuration = new MailboxConfiguration(_host!, 993, true);
    var mailboxConnection = new Mailbox.MailboxConnection(configuration);

    await mailboxConnection.ConnectAsync(CancellationToken.None);

    await Assert.That(mailboxConnection.IsOnline).IsTrue();
  }
}
