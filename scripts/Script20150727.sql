EXEC dbo.sp_changedbowner @loginame = N'sa', @map = false
GO

exec sp_configure 'clr enabled', 1
reconfigure
GO
ALTER DATABASE CURRENT  SET TRUSTWORTHY ON
GO


IF OBJECT_ID ('CLR_HttpGetBlob', 'FS') IS NOT NULL  
   DROP FUNCTION [dbo].[CLR_HttpGetBlob]
GO
IF OBJECT_ID ('CLR_HttpGetBlobPost', 'FS') IS NOT NULL 
   DROP FUNCTION [dbo].[CLR_HttpGetBlobPost]
GO
IF OBJECT_ID ('CLR_UnicodeToUTF8', 'FS') IS NOT NULL 
   DROP FUNCTION [dbo].[CLR_UnicodeToUTF8]
GO
IF OBJECT_ID ('CLR_UTF8ToUnicode', 'FS') IS NOT NULL 
   DROP FUNCTION [dbo].[CLR_UTF8ToUnicode]
GO
IF OBJECT_ID ('CLR_JSONtoXML', 'FS') IS NOT NULL 
   DROP FUNCTION [dbo].[CLR_JSONtoXML]
GO

if not Exists(SELECT * FROM  sys.assemblies where Name='System.Runtime.Serialization')
begin
CREATE ASSEMBLY [System.Runtime.Serialization]
AUTHORIZATION [dbo]
FROM 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.Runtime.Serialization.dll'
WITH PERMISSION_SET = UNSAFE
end
GO

if Exists(SELECT * FROM  sys.assemblies where Name='SQLfuncs')
   DROP ASSEMBLY [SQLfuncs]
GO
if Exists(SELECT * FROM  sys.assemblies where Name='newtonsoft.json')
   DROP ASSEMBLY [newtonsoft.json]
GO


CREATE ASSEMBLY [newtonsoft.json]
FROM 'C:\CLR_DLL\Newtonsoft.Json.dll' 
WITH PERMISSION_SET = UNSAFE
GO
declare @clr_Name varchar(500)
SELECT @clr_name=clr_name FROM  sys.assemblies where name='newtonsoft.json'
print @clr_name
GO

CREATE ASSEMBLY [SQLfuncs]
FROM 'C:\CLR_DLL\SQLfuncs.dll' 
WITH PERMISSION_SET = UNSAFE
GO
declare @clr_Name varchar(500)
SELECT @clr_name=clr_name FROM  sys.assemblies where name='SQLfuncs'
print @clr_name
GO


CREATE FUNCTION [dbo].[CLR_HttpGetBlob](@url [nvarchar](max))
RETURNS  varbinary(max)
 WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SQLfuncs].[SQLfuncs.ReadWriteBlob].[ReadBlob]
GO
IF OBJECT_ID ('CLR_HttpGetBlob', 'FS') IS NOT NULL 
 print 'CLR_HttpGetBlob ready'
GO

CREATE FUNCTION [dbo].[CLR_HttpGetBlobPost](@url [nvarchar](max),@parameters [nvarchar](max),@contenttype [nvarchar](max))
RETURNS varbinary(max)
 WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SQLfuncs].[SQLfuncs.ReadWriteBlobPost].[ReadBlobPost]
GO
IF OBJECT_ID ('CLR_HttpGetBlobPost', 'FS') IS NOT NULL 
 print 'CLR_HttpGetBlobPost ready'
GO


CREATE FUNCTION [dbo].[CLR_UnicodeToUTF8](@source [nvarchar](max))
RETURNS nvarchar(max)
 WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SQLfuncs].[SQLfuncs.StringFunctions].[UnicodeToUTF8]
GO
IF OBJECT_ID ('CLR_UnicodeToUTF8', 'FS') IS NOT NULL 
 print 'CLR_UnicodeToUTF8 ready'
GO

CREATE FUNCTION [dbo].[CLR_UTF8ToUnicode](@source [nvarchar](max))
RETURNS nvarchar(max)
 WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SQLfuncs].[SQLfuncs.StringFunctions].[UTF8ToUnicode]
GO
IF OBJECT_ID ('CLR_UTF8ToUnicode', 'FS') IS NOT NULL 
 print 'CLR_UTF8ToUnicode ready'
GO

CREATE FUNCTION [dbo].[CLR_JSONtoXML](@source [nvarchar](max))
RETURNS nvarchar(max)
 WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SQLfuncs].[SQLfuncs.StringFunctions].[JSONtoXML]
GO
IF OBJECT_ID ('CLR_JSONtoXML', 'FS') IS NOT NULL 
 print 'CLR_JSONtoXML ready'
GO