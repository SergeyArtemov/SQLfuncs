EXEC dbo.sp_changedbowner @loginame = N'dvv', @map = false
GO
ALTER DATABASE CURRENT  SET TRUSTWORTHY ON
GO
DROP FUNCTION [dbo].[CLR_HttpGetBlob]
GO
DROP FUNCTION [dbo].[CLR_HttpGetBlobPost]
GO
/*20150624 begin*/
DROP FUNCTION [dbo].[CLR_UnicodeToUTF8]
GO
DROP FUNCTION [dbo].[CLR_UTF8ToUnicode]
GO
/*20150624 end*/
DROP ASSEMBLY [SQLfuncs]
GO
CREATE ASSEMBLY [SQLfuncs]
FROM 'D:\MSSQL11.PS_SQL00_01A\MSSQL\Binn\SQLfuncs.dll' 
WITH PERMISSION_SET = EXTERNAL_ACCESS
GO
CREATE FUNCTION [dbo].[CLR_HttpGetBlob](@url [nvarchar](max))
RETURNS  varbinary(max)
 WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SQLfuncs].[ReadWriteBlob].[ReadBlob]
GO
CREATE FUNCTION [dbo].[CLR_HttpGetBlobPost](@url [nvarchar](max),@parameters [nvarchar](max),@contenttype [nvarchar](max))
RETURNS  varbinary(max)
 WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SQLfuncs].[ReadWriteBlobPost].[ReadBlobPost]
GO
/*20150624 begin*/
CREATE FUNCTION [dbo].[CLR_UnicodeToUTF8](@source [varchar](max))
RETURNS  varchar(max)
 WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SQLfuncs].[StringFunctions].[UnicodeToUTF8]
GO
CREATE FUNCTION [dbo].[CLR_UTF8ToUnicode](@source [varchar](max))
RETURNS  varchar(max)
 WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SQLfuncs].[StringFunctions].[UTF8ToUnicode]
GO

--SELECT [dbo].[CLR_HttpGetBlob] ('http://static-maps.yandex.ru/1.x/?ll=37.511275,55.718654&size=541,450&z=15&l=map&pt=37.511275,55.718654,pm2grl')