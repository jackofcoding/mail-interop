namespace MailInterop.Mailbox;

public class MailboxFolderNotOpenedException : Exception
{
  public string Name { get; }

  public MailboxFolderNotOpenedException(string name) : base("Mailbox folder is not opened.")
  {
    Name = name;
  }
}
