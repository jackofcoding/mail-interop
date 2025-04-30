namespace MailInterop.Mailbox;

public class MailboxAuthenticationException : Exception
{
  public string Username { get; }

  public MailboxAuthenticationException(string username, Exception exception) : base($"Failed to connect with username {username}", exception)
  {
    Username = username;
  }
}
