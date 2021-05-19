# Trace
A C# Ms Windows service that traces MS SSAS Tabular 2012 transactions based on 2012 version of [ASTrace](https://github.com/microsoft/Analysis-Services/tree/master/AsTrace).
This was developed by me in order to have a version of the service that allowed me to do the inserts in the database by use of a stored procedure (more secure).
Please note that I do not care for the actual query being executed, so i do not store it. This can be easily changed.

## Projects

### TraceBackend
A Library with the all the logic so that its easier to test without installing the service.
Make sure to configure the properties in the settings:
* Schema: The schema of the stored procedure to be executed for each event the trace captures. See "About the Stored Procedure" for more information.
* SP_Name: The name of the stored procedure to be executed for each event.
* DB_Name: The name of the database where the stored procedure is located.

### TraceConsole
Just a console application used to test the logic in TraceBackend.
Make sure to configure the properties in the settings:
* TraceServer: The MS SSAS Tabular 2012 server the service will connect to capture events.
* DatabaseServer: The MS SQL Server 2012 server the service will connect to execute the stored procedure defined in TraceBackend for each of the events captured.
* TraceTemplateFileName: The MS SQL Server Profiler template to be used by the service. See "About the Trace Template" for more information.

### TraceService
The actual MS Windows Service. 
Make sure to configure the properties in the settings:
* TraceServer: The MS SSAS Tabular 2012 server the service will connect to capture events.
* DatabaseServer: The MS SQL Server 2012 server the service will connect to execute the stored procedure defined in TraceBackend for each of the events captured.
* TraceTemplateFileName: The MS SQL Server Profiler template to be used by the service. See "About the Trace Template" for more information.
* LogLevel: The log level of the events registered in the MS Event Viewer by the service.

## About the Service Account
The service account for the service must have rights to both run traces on the _TraceServer_ as execution rights on the _DatabaseServer_.
*Make sure you provide the credentials correctly.*

## About the Stored Procedure
The stored procedure needs to have the following structure:
```sql
CREATE PROCEDURE [schema_name].[sp_name]
(
	@event_class nvarchar(255),
	@event_sub_class int,
	@nt_user_name nvarchar(255),
	@application_name nvarchar(255),
	@start_time datetime2(0),
	@duration bigint,
	@database_name nvarchar(255)
) AS
BEGIN
	--Do what you want with the data and return 0 if everything went fine...
	RETURN 0;
END
```
## About the Trace Template
The trace template needs to be an SSAS template with the following fields:
* SPID
* EventSubClass
* DatabaseName
* NTUserName
* ApplicationName
* TextData
* StartTime
* Duration

It shouldn't be a problem to include more fields, but they wont be captured.

## Install Instructions
After a full build of the solution, copy the following files to any folder you want:
* TraceBackend.dll
* TraceService.exe
* TraceService.exe.config

Inside said folder, create a new one called "Templates" and include there the template of the trace to be used by the service.

Then, run the following command with administrator rights from a terminal:
> installUtil "TraceService.exe"

And provide both user and password from the service account the service will use to run.

_(Make sure that the installUtil location is included in the PATH of the terminal)_

## Uninstall Instructions
Run the following command with administrator rights from a terminal:
> installUtil /u "TraceService.exe"

_(Make sure that the installUtil location is included in the PATH of the terminal)_

## Additional requirements.
The TraceBackend makes use of the API from MS SQL Server 2012.
* Microsoft.SqlServer.ConnectionInfo
* Microsoft.SqlServer.ConnectionInfoExtended

Both of these get installed with MS SQL Management Studio 2012. Be sure to have it installed wherever the service will run.