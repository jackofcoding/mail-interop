namespace MailInterop.Mailbox;

public class MailboxFolderNotfoundException : Exception
{
  public string Path { get; }

  public MailboxFolderNotfoundException(string path, Exception exception) : base($"Folder could not found with path {path}", exception)
  {
    Path = path;
  }
}
