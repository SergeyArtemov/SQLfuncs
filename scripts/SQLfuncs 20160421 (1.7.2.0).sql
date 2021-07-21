use GateStore_test
GO


exec sp_configure 'clr enabled', 1
reconfigure
GO
ALTER DATABASE CURRENT  SET TRUSTWORTHY ON
GO


IF OBJECT_ID ('CLR_HttpGetBlob', 'FS') IS NOT NULL  
   DROP FUNCTION [dbo].[CLR_HttpGetBlob]
GO
IF OBJECT_ID ('CLR_HttpGetBlobEx', 'FS') IS NOT NULL  
   DROP FUNCTION [dbo].[CLR_HttpGetBlobEx]
GO

IF OBJECT_ID ('CLR_HttpGet', 'FS') IS NOT NULL  
   DROP FUNCTION [dbo].[CLR_HttpGet]
GO

IF OBJECT_ID ('CLR_HttpGetString', 'FS') IS NOT NULL  
   DROP FUNCTION [dbo].[CLR_HttpGetString]
GO

IF OBJECT_ID ('CLR_HttpGetBinary', 'FS') IS NOT NULL  
   DROP FUNCTION [dbo].[CLR_HttpGetBinary]
GO

IF OBJECT_ID ('CLR_HttpGetStringAuth', 'FS') IS NOT NULL  
   DROP FUNCTION [dbo].[CLR_HttpGetStringAuth]
GO

IF OBJECT_ID ('CLR_HttpGetBinaryAuth', 'FS') IS NOT NULL  
   DROP FUNCTION [dbo].[CLR_HttpGetBinaryAuth]
GO


IF OBJECT_ID ('CLR_HttpGetBlobPost', 'FS') IS NOT NULL 
   DROP FUNCTION [dbo].[CLR_HttpGetBlobPost]
GO
IF OBJECT_ID ('CLR_HttpGetBlobPostEx', 'FS') IS NOT NULL 
   DROP FUNCTION [dbo].[CLR_HttpGetBlobPostEx]
GO
IF OBJECT_ID ('CLR_HttpGetBlobMethod', 'FS') IS NOT NULL --ReadBlobMethod
   DROP FUNCTION [dbo].[CLR_HttpGetBlobMethod]
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
IF OBJECT_ID ('CLR_EmarsysRequestEx', 'FT') IS NOT NULL -- ATTENTION on Type of function --
   DROP FUNCTION [dbo].[CLR_EmarsysRequestEx]
GO

IF OBJECT_ID ('CLR_SaveToFile', 'FS') IS NOT NULL 
   DROP FUNCTION [dbo].[CLR_SaveToFile]
GO

IF OBJECT_ID ('CLR_Levis_PingPong', 'FS') IS NOT NULL 
   DROP FUNCTION [dbo].[CLR_Levis_PingPong]
GO

IF OBJECT_ID ('CLR_Levis_GetPackage', 'FS') IS NOT NULL 
   DROP FUNCTION [dbo].[CLR_Levis_GetPackage]
GO

IF OBJECT_ID ('CLR_Levis_PutPackage', 'FS') IS NOT NULL 
   DROP FUNCTION [dbo].[CLR_Levis_PutPackage]
GO


if Exists(SELECT * FROM  sys.assemblies where Name='SQLfuncs.XmlSerializers')
   DROP ASSEMBLY [SQLfuncs.XmlSerializers]
GO
if Exists(SELECT * FROM  sys.assemblies where Name='SQLfuncs')
   DROP ASSEMBLY [SQLfuncs]
GO
if Exists(SELECT * FROM  sys.assemblies where Name='newtonsoft.json')
   DROP ASSEMBLY [newtonsoft.json]
GO
if Exists(SELECT * FROM  sys.assemblies where Name='System.Runtime.Serialization')
   DROP ASSEMBLY [System.Runtime.Serialization]
GO

 print '---- ��������� �������� ������� � ������ -----'
 GO


if not Exists(SELECT * FROM  sys.assemblies where Name='System.Runtime.Serialization')
   begin
   CREATE ASSEMBLY [System.Runtime.Serialization] AUTHORIZATION [dbo] FROM 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.Runtime.Serialization.dll' WITH PERMISSION_SET = UNSAFE
   print '--> ������ System.Runtime.Serialization ������� �������'
   end
   else
   BEGIN TRY
   ALTER ASSEMBLY [System.Runtime.Serialization] FROM 'C:\Windows\Microsoft.NET\Framework\v4.0.30319\System.Runtime.Serialization.dll' WITH PERMISSION_SET = UNSAFE
   print '--> ������ System.Runtime.Serialization ������� ���������'
   END TRY
   BEGIN CATCH
   print '--> ������ System.Runtime.Serialization ��������� ���������� � �� ������� ����������'
   END CATCH 
GO

if not Exists(SELECT * FROM  sys.assemblies where Name='newtonsoft.json')
   begin
   CREATE ASSEMBLY [newtonsoft.json] FROM 'C:\CLR\Newtonsoft.Json.dll' WITH PERMISSION_SET = UNSAFE
   print '--> ������ Newtonsoft.Json ������� �������'
   end
   else
   BEGIN TRY
   ALTER ASSEMBLY [newtonsoft.json] FROM 'C:\CLR\Newtonsoft.Json.dll' WITH PERMISSION_SET = UNSAFE
   print '--> ������ Newtonsoft.Json ������� ���������'
   END TRY
   BEGIN CATCH
   print '--> ������ Newtonsoft.Json ��������� ���������� � �� ������� ����������'
   END CATCH 
GO


if not Exists(SELECT * FROM  sys.assemblies where Name='SQLfuncs')
   begin
   CREATE ASSEMBLY [SQLfuncs] FROM 'C:\CLR\SQLfuncs.dll' WITH PERMISSION_SET = UNSAFE
   print '--> ������ SQLfuncs ������� �������'
   end
   else 
   BEGIN TRY
   ALTER ASSEMBLY [SQLfuncs] FROM 'C:\CLR\SQLfuncs.dll' WITH PERMISSION_SET = UNSAFE
   print '--> ������ SQLfuncs ������� ���������'
   END TRY
   BEGIN CATCH
   print '--> ������ SQLfuncs ��������� ���������� � �� ������� ����������'
   END CATCH
GO


if not Exists(SELECT * FROM  sys.assemblies where Name='SQLfuncs.XmlSerializers')
   begin
   CREATE ASSEMBLY [SQLfuncs.XmlSerializers] FROM 'C:\CLR\SQLfuncs.XmlSerializers.dll' WITH PERMISSION_SET = UNSAFE
   print '--> ������ SQLfuncs.XmlSerializers ������� �������'
   end
   else 
   BEGIN TRY
   ALTER ASSEMBLY [SQLfuncs.XmlSerializers] FROM 'C:\CLR\SQLfuncs.XmlSerializers.dll' WITH PERMISSION_SET = UNSAFE
   print '--> ������ SQLfuncs.XmlSerializers ������� ���������'
   END TRY
   BEGIN CATCH
   print '--> ������ SQLfuncs.XmlSerializers ��������� ���������� � �� ������� ����������'
   END CATCH
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

CREATE FUNCTION [dbo].[CLR_HttpGetBlobEx](@url [nvarchar](max), @headers [nvarchar](max))
RETURNS  varbinary(max)
 WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SQLfuncs].[SQLfuncs.ReadWriteBlob].[ReadBlobEx]
GO
IF OBJECT_ID ('CLR_HttpGetBlobEx', 'FS') IS NOT NULL 
 print 'CLR_HttpGetBlobEx ready'
GO


CREATE FUNCTION [dbo].[CLR_HttpGet](@url [nvarchar](max), @headers [nvarchar](max))
RETURNS  varbinary(max)
 WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SQLfuncs].[SQLfuncs.ReadWriteBlob].[HttpGet]
GO
IF OBJECT_ID ('CLR_HttpGet', 'FS') IS NOT NULL 
 print 'CLR_HttpGet'
GO

CREATE FUNCTION [dbo].[CLR_HttpGetString](@url [nvarchar](max))
RETURNS  [nvarchar](max)
 WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SQLfuncs].[SQLfuncs.ReadWriteBlob].[HttpGetString]
GO
IF OBJECT_ID ('CLR_HttpGetString', 'FS') IS NOT NULL 
 print 'CLR_HttpGetString ready'
GO

CREATE FUNCTION [dbo].[CLR_HttpGetBinary](@url [nvarchar](max))
RETURNS  varbinary(max)
 WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SQLfuncs].[SQLfuncs.ReadWriteBlob].[HttpGetBinary]
GO
IF OBJECT_ID ('CLR_HttpGetBinary', 'FS') IS NOT NULL 
 print 'CLR_HttpGetBinary ready'
GO

CREATE FUNCTION [dbo].[CLR_HttpGetStringAuth](@url [nvarchar](max),@user [nvarchar](max),@password [nvarchar](max))
RETURNS  [nvarchar](max)
 WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SQLfuncs].[SQLfuncs.ReadWriteBlob].[HttpGetStringAuth]
GO
IF OBJECT_ID ('CLR_HttpGetStringAuth', 'FS') IS NOT NULL 
 print 'CLR_HttpGetStringAuth ready'
GO

CREATE FUNCTION [dbo].[CLR_HttpGetBinaryAuth](@url [nvarchar](max),@user [nvarchar](max),@password [nvarchar](max))
RETURNS  varbinary(max)
 WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SQLfuncs].[SQLfuncs.ReadWriteBlob].[HttpGetBinaryAuth]
GO
IF OBJECT_ID ('CLR_HttpGetBinaryAuth', 'FS') IS NOT NULL 
 print 'CLR_HttpGetBinaryAuth ready'
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

CREATE FUNCTION [dbo].[CLR_HttpGetBlobPostEx](@url [nvarchar](max),@headers [nvarchar](max),@parameters [nvarchar](max),@contenttype [nvarchar](max))
RETURNS varbinary(max)
 WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SQLfuncs].[SQLfuncs.ReadWriteBlobPost].[ReadBlobPostEx]
GO
IF OBJECT_ID ('CLR_HttpGetBlobPostEx', 'FS') IS NOT NULL 
 print 'CLR_HttpGetBlobPostEx ready'
GO

CREATE FUNCTION [dbo].[CLR_HttpGetBlobMethod](@url [nvarchar](max),@method [nvarchar](max),@headers [nvarchar](max),@parameters [nvarchar](max),@contenttype [nvarchar](max))
RETURNS varbinary(max)
 WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SQLfuncs].[SQLfuncs.ReadWriteBlobPost].[ReadBlobMethod]
GO
IF OBJECT_ID ('CLR_HttpGetBlobMethod', 'FS') IS NOT NULL 
 print 'CLR_HttpGetBlobMethod ready'
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


CREATE FUNCTION [dbo].[CLR_EmarsysRequestEx](@Method [nvarchar](4), @User [nvarchar](256), @Password [nvarchar](256), @URI [nvarchar](1024), @json [nvarchar](max))
RETURNS TABLE (result nvarchar(max), error nvarchar(max))
 WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SQLfuncs].[SQLfuncs.StringFunctions].[RequestEx]
GO
IF OBJECT_ID ('CLR_EmarsysRequestEx', 'FT') IS NOT NULL -- ATTENTION on Type of function --
 print 'CLR_EmarsysRequestEx ready'
GO

CREATE FUNCTION [dbo].[CLR_SaveToFile](@Data [nvarchar](max), @Filename [nvarchar](max))
RETURNS  nvarchar(max)
 WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SQLfuncs].[SQLfuncs.StringFunctions].[SaveToFile]
GO
IF OBJECT_ID ('CLR_SaveToFile', 'FS') IS NOT NULL -- ATTENTION on Type of function --
 print 'CLR_SaveToFile ready'
GO


CREATE FUNCTION [dbo].[CLR_Levis_PingPong]()
RETURNS  nvarchar(max)
 WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SQLfuncs].[SQLfuncs.StringFunctions].[Levis_PingPong]
GO
IF OBJECT_ID ('CLR_Levis_PingPong', 'FS') IS NOT NULL -- ATTENTION on Type of function --
 print 'CLR_Levis_PingPong'
GO


CREATE FUNCTION [dbo].[CLR_Levis_GetPackage](@package [nvarchar](max))
RETURNS  nvarchar(max)
 WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SQLfuncs].[SQLfuncs.StringFunctions].[Levis_GetPackage]
GO
IF OBJECT_ID ('CLR_Levis_GetPackage', 'FS') IS NOT NULL -- ATTENTION on Type of function --
 print 'CLR_Levis_GetPackage'
GO


CREATE FUNCTION [dbo].[CLR_Levis_PutPackage](@package [nvarchar](max))
RETURNS  nvarchar(max)
 WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SQLfuncs].[SQLfuncs.StringFunctions].[Levis_PutPackage]
GO
IF OBJECT_ID ('CLR_Levis_PutPackage', 'FS') IS NOT NULL -- ATTENTION on Type of function --
 print 'CLR_Levis_PutPackage'
GO

GRANT EXECUTE ON [dbo].[CLR_Levis_GetPackage] TO [nav_gate]
GO
GRANT EXECUTE ON [dbo].[CLR_Levis_PingPong] TO [nav_gate]
GO
GRANT EXECUTE ON [dbo].[CLR_Levis_PutPackage] TO [nav_gate]
GO
GRANT EXECUTE ON [dbo].[CLR_HttpGetBlob] TO [mo2]
GO


--------------------------------------------------------------------------------------
--select @@version
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
,A.modify_date -- ��������� � A.create_date ��� CREATE, �� ���������� ��� ALTER 
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

select * from sys.assemblies
-------------------------------------------------------------

-- ������� ������������� ------------------------------------

/*
declare @url varchar(max) = 'http://mig3.sovietwarplanes.com/mig3/white57.jpg'
--declare @url varchar(max) = 'https://www.gismeteo.ru/city/daily/11938/'
declare @data varbinary(max)
--
set @data=0x
select @data=dbo.CLR_GetBitmapScale(@url,200,200)
select @data,DATALENGTH(@data) as datalength
--
set @data=0x
select @data=dbo.CLR_HttpGetBlob(@url)
select @data,DATALENGTH(@data) as datalength
-- � ��� ���������� �������� ������ � �� NULL ��� ���������� � ���������
set @data=0x
select @data=dbo.CLR_HttpGetBlobEx(@url,'')
if DATALENGTH(@data)<256
  select CAST(@data as varchar(max))
  else
  select @data,DATALENGTH(@data) as datalength
-- 
set @data=0x
select @data=dbo.CLR_HttpGet(@url,null)
select @data,DATALENGTH(@data) as datalength
--
select @data=CAST(CAST(dbo.CLR_HttpGetString(@url) as varchar(max)) as varbinary(max))
select @data,CAST(@data as varchar(max)),DATALENGTH(@data) as datalength
-- 
set @data=0x
select @data=dbo.CLR_HttpGetBinary(@url)
select @data,DATALENGTH(@data) as datalength
*/


-- check code --
-- declare @data nvarchar(max) = '{"keyId": "3","keyValues": ["developer.vulgaris_@yandex.ru"]}';
-- select * from dbo.CLR_EmarsysRequest('POST','contact/getdata',@data)


