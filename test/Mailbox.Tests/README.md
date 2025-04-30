This test project uses user secrets.
You must set these up before you can run the tests.

 - `ConnectAsync:Host` - The host name of IMAP server
 - `ConnectAsync:Username` - The username to connect to the IMAP server
 - `ConnectAsync:Password` - The password to connect to the IMAP server

You can set user secrets by navigating to this project, and then run  
`dotnet user-secrets set <name> <value>`  
Where you replace `<name>` and `<value>` without the `<>` included.
