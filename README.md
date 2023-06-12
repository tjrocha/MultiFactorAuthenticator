# MultiFactorAuthenticator
Compiles an executable that forces Smart Card authentication on Windows. Intent is to reference this project in our other Windows desktop applications to force Smart Card authentication for MFA compliance.

The executable from this codebase can be added to any programming workflow via the steps below. This project was programmed as an executable by design since we have a lot of custom tools and programs in different programming and scripting languages that would benefit from having an external routine that will enforce Smart Card authentication.
1. Invoke the executable to force Smart Card authentication from the user before initializing/instantiating the desired program/software/tool
2. If authentication succeeds (executable returns **_true_**), continue into the desired program/software/tool
3. ...
4. Profit!

-- Jon
