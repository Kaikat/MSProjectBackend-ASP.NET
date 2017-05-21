# TamuyalBackend-ASP.NET

In order to get this code to work on your computer you need to go into the root project directory that contains nuget.exe and run the following line in the Command Prompt.
```
nuget.exe install WebApplication1\packages.config -OutputDirectory packages
```

API calls can be accessed in the following format. portNumber is currently set to 50099.
```
http://localhost:{portNumber}/api/{controllerName}
```
