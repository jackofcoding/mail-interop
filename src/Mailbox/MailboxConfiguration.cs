namespace MailInterop.Mailbox;

public record MailboxConfiguration(string Host, int Port, bool UseSSL);
