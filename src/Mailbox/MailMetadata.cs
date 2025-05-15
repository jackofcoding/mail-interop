using MailKit;

namespace MailInterop.Mailbox;

public sealed class MailMetadata
{
  private readonly MailId _id;
  private readonly string _messageId;
  private readonly DateTimeOffset _date;
  private readonly bool _isSeen;
  private readonly string _subject;

  public MailMetadata(uint id, uint folderId, string messageId, DateTimeOffset date, bool isSeen, string subject)
  {
    _id = new(id, folderId);
    _messageId = messageId;
    _date = date;
    _isSeen = isSeen;
    _subject = subject;
  }

  internal MailMetadata(IMessageSummary summary)
  {
    _id = new(summary.UniqueId.Id, summary.UniqueId.Validity);
    _messageId = summary.Envelope.MessageId;
    _date = summary.Envelope.Date ?? DateTimeOffset.MinValue;
    _isSeen = (summary.Flags & MessageFlags.Seen) != 0;
    _subject = summary.Envelope.Subject;
  }
}
