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
/*20150724 begin*/
DROP FUNCTION [dbo].[CLR_JSONtoXML]
GO
/*20150724 end*/

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
RETURNS varbinary(max)
 WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SQLfuncs].[ReadWriteBlobPost].[ReadBlobPost]
GO
/*20150713 begin*/
CREATE FUNCTION [dbo].[CLR_UnicodeToUTF8](@source [nvarchar](max))
RETURNS nvarchar(max)
 WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SQLfuncs].[StringFunctions].[UnicodeToUTF8]
GO
CREATE FUNCTION [dbo].[CLR_UTF8ToUnicode](@source [nvarchar](max))
RETURNS nvarchar(max)
 WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SQLfuncs].[StringFunctions].[UTF8ToUnicode]
GO
/*20150713 end*/

CREATE FUNCTION [dbo].[CLR_JSONtoXML](@source [nvarchar](max))
RETURNS nvarchar(max)
 WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SQLfuncs].[StringFunctions].[JSONtoXML]
GO