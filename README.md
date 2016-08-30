Genesys Web Services Client Library for .NET
============================================

This library will allow you to easily build clients that make use of the Genesys Web Services API.

It features the following libraries:

- `Genesys.WebServicesClient`: Allows to easily set up a connection with the Genesys Web Services, send requests, handle responses and receive events through a CometD channel.

- `Genesys.WebServicesClient.Components`: Provides a set of components that you can use in your Windows Forms or WPF applications.


How to use this library
=======================

Do a clone of this repository.

This library makes use of a [fork of Oyatel's CometD.NET libraries](https://github.com/ErnestoGarciaGenesys/CometD.NET). Do a clone of that repository as well.

and include this library's source code into your project

Include In your solution, add the following projects:

- `cometd`, from the CometD.NET fork
- `Genesys.WebServicesClient`, from this repository
- `Genesys.WebServicesClient.Components`, from this repository

Other dependencies are configured via NuGet.


Using Genesys.WebServicesClient.Components
==========================================

If you include 

From a WPF Application
----------------------

As a sample you can use the application in `Genesys.WebServicesClient.Sample.Agent.WPF`.


