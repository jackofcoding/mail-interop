namespace MailInterop.Mailbox;

public class MailboxConnectionException : Exception
{
  public string Host { get; }
  
  public MailboxConnectionException(string host, Exception exception) : base($"Failed to connect to {host}", exception)
  {
    Host = host;
  }
}
