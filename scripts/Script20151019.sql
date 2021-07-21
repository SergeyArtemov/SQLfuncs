set nocount on
--set datefirst 1

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
IF OBJECT_ID ('CLR_URLEncode', 'FS') IS NOT NULL 
   DROP FUNCTION [dbo].[CLR_URLEncode]
GO
IF OBJECT_ID ('CLR_URLDecode', 'FS') IS NOT NULL 
   DROP FUNCTION [dbo].[CLR_URLDecode]
GO
IF OBJECT_ID ('CLR_EmarsysRequest', 'FT') IS NOT NULL -- ATTENTION on Type of function --
   DROP FUNCTION [dbo].[CLR_EmarsysRequest]
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


CREATE ASSEMBLY [SQLfuncs]
FROM 'C:\CLR_DLL\SQLfuncs.dll' 
WITH PERMISSION_SET = UNSAFE
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

CREATE FUNCTION [dbo].[CLR_HttpGetBlobPost](@url [nvarchar](max),@headers [nvarchar](max),@parameters [nvarchar](max),@contenttype [nvarchar](max))
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

CREATE FUNCTION [dbo].[CLR_URLEncode](@source [nvarchar](max))
RETURNS nvarchar(max)
 WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SQLfuncs].[SQLfuncs.StringFunctions].[URLEncode]
GO
IF OBJECT_ID ('CLR_URLEncode', 'FS') IS NOT NULL 
 print 'CLR_URLEncode ready'
GO

CREATE FUNCTION [dbo].[CLR_URLDecode](@source [nvarchar](max))
RETURNS nvarchar(max)
 WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SQLfuncs].[SQLfuncs.StringFunctions].[URLDecode]
GO
IF OBJECT_ID ('CLR_URLDecode', 'FS') IS NOT NULL 
 print 'CLR_URLDecode ready'
GO

CREATE FUNCTION [dbo].[CLR_EmarsysRequest](@Method [nvarchar](4), @URI [nvarchar](1024), @json [nvarchar](max))
RETURNS TABLE (result nvarchar(max), error nvarchar(max))
 WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SQLfuncs].[SQLfuncs.StringFunctions].[Request]
GO
IF OBJECT_ID ('CLR_EmarsysRequest', 'FT') IS NOT NULL -- ATTENTION on Type of function --
 print 'CLR_EmarsysRequest ready'
GO
-- check code --
-- declare @data nvarchar(max) = '{"keyId": "3","keyValues": ["developer.vulgaris_@yandex.ru"]}';
-- select * from dbo.CLR_EmarsysRequest('POST','contact/getdata',@data)
--------------------------------------------------------------------------------------
select @@version
select 
 SO.name
,SO.id
,SO.crdate
,SO.refdate
,AM.assembly_id
,AM.assembly_class -- ����� � ������
,AM.assembly_method -- ���������� ����� ������
,A.name
,A.clr_name
,A.create_date
--,A.modify_date -- ��������� � A.create_date
,A.permission_set
,A.permission_set_desc
,A.is_visible
,A.is_user_defined
,AF.name
,AF.file_id
,AF.content
from dbo.sysobjects SO
join sys.assembly_modules AM on AM.object_id = SO.id
join sys.assemblies A on A.assembly_id = AM.assembly_id
join sys.assembly_files AF on AF.assembly_id = AM.assembly_id
where SO.xtype in ('FS','FT')
order by SO.crdate desc, SO.id desc, SO.name



