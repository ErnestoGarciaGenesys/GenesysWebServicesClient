Genesys Web Services Client Library for .NET
============================================

This library will allow you to easily build clients that make use of the Genesys Web Services API.

It features the following libraries:

- `Genesys.WebServicesClient`: Allows to easily set up a connection with the Genesys Web Services, send requests, handle responses and receive events through a CometD channel.

- `Genesys.WebServicesClient.Components`: Provides a set of components that you can use in your Windows Forms or WPF applications.


How to use this library
=======================

Include this library's source code into your .NET solution. In order to do that:

Do a clone of this repository.

This library makes use of a [fork of Oyatel's CometD.NET libraries](https://github.com/ErnestoGarciaGenesys/CometD.NET). Do a clone of that repository as well.

In your solution, add the following projects:

- `cometd`, from the CometD.NET fork

- `Genesys.WebServicesClient`, from this repository.  
  Make sure it references the `cometd` project properly. 
  
- `Genesys.WebServicesClient.Components`, from this repository.  
  Make sure it references the `Genesys.WebServicesClient` project properly.

(Dependencies on other 3rd party libraries are already configured with NuGet).


Sample Code
===========

In order to learn how to use this library, it is best if you take a look at the sample applications. These applications only contain one significant source code object, so it is easy to see what is going on.

- `Genesys.WebServicesClient.Sample.Agent.WPF`: This is the WPF sample. Take a look into `MainWindow`.
- `Genesys.WebServicesClient.Sample.Agent.WinForms`: This is the Windows Forms sample. Take a look into `MainForm`.

You will learn how to:

- Create the structure of Genesys components needed for your application
- Set up and start the connection with Genesys Web Services
- Bind those components to UI Labels, DataGrids, enablement of Buttons, etc.
- Implement the actions of Buttons
