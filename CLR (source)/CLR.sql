EXEC dbo.sp_changedbowner @loginame = N'dvv', @map = false
GO
ALTER DATABASE CURRENT  SET TRUSTWORTHY ON
GO
DROP FUNCTION [dbo].[CLR_HttpGetBlob]
GO
DROP ASSEMBLY [HttpGetBlob]
GO
CREATE ASSEMBLY [HttpGetBlob]
FROM 'D:\MSSQL11.PS_SQL00_01A\MSSQL\Binn\HttpGetBlob.dll' 
WITH PERMISSION_SET = EXTERNAL_ACCESS
GO
CREATE FUNCTION [dbo].[CLR_HttpGetBlob](@url [nvarchar](max))
RETURNS  varbinary(max)
 WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [HttpGetBlob].[ReadWriteBlob].[ReadBlob]
GO

SELECT [dbo].[CLR_HttpGetBlob] ('http://static-maps.yandex.ru/1.x/?ll=37.511275,55.718654&size=541,450&z=15&l=map&pt=37.511275,55.718654,pm2grl')