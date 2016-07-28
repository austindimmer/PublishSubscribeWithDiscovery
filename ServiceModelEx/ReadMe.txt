You will need the Microsoft.ServiceBus.dll and the Moq.dll assemblies for this solution. 


Microsoft.ServiceBus.dll
~~~~~~~~~~~~~~~~~~~~~~~~
You can install the Azure SDK, and then add a reference to the assembly at:
C:\Program Files\Microsoft SDKs\Azure\.NET SDK\v2.8\ToolsRef\Microsoft.ServiceBus.dll


Moq.dll
~~~~~~~
You will need the Moq.dll assembly for compiling your tests. 

You can download the assembly from Nuget and install it:
http://www.nuget.org/packages/moq

You can also download Moq.dll and install it in the GAC so you could add reference to it: 
gacutil /i Moq.dll


Finally, you can get both assemblies from Nuget by placing in the root a file called packages.config
with thesereferences inside: 

<?xml version="1.0" encoding="utf-8"?>
<packages>
  <package id="WindowsAzure.ServiceBus" version="3.1.2" targetFramework="net461" />
  <package id="Moq" version="4.2.1510.2205" targetFramework="net46" />
</packages>