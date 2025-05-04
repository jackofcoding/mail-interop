namespace MailInterop.Mailbox;

public class MailboxConnectionNotOnlineException : Exception
{
  public MailboxConnectionNotOnlineException() :base("Connection with mailbox is not online.")
  { }
}
