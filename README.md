# YDKK.Client.Jobcan
Unofficial Jobcan client library for .NET

It can obtain current status from Jobcan webpage.

[![#](https://img.shields.io/nuget/v/YDKK.Client.Jobcan.svg)](https://www.nuget.org/packages/YDKK.Client.Jobcan/)

## Usage
```cs
using YDKK.Client;


var jobcan = await Jobcan.LoginAsync(clientId, email, password);

//If you want to keep session, use keepSession flag
//(It will post empty noop request every 10 minutes like Jobcan webpage
var jobcan = await Jobcan.LoginAsync(clientId, email, password, true);

//Get status
var status = await jobcan.GetStatusAsync();
```

## Will it support SET status action?
Probably, not. (Because I don't need such function now.
