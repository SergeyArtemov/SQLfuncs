USE [MO]
GO

/****** Object:  UserDefinedFunction [dbo].[CLR_AsUTF8]    Script Date: 21.07.2021 13:04:41 ******/
SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER OFF
GO

CREATE FUNCTION [dbo].[CLR_AsUTF8](@source [nvarchar](max))
RETURNS [nvarchar](max) WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SQLfuncs].[SQLfuncs.StringFunctions].[AsUTF8]
GO


-----

SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER OFF
GO

CREATE FUNCTION [dbo].[CLR_AsWin1251](@source [nvarchar](max))
RETURNS [nvarchar](max) WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SQLfuncs].[SQLfuncs.StringFunctions].[AsWin1251]
GO


-----

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
ALTER FUNCTION [dbo].[CLR_JSONtoXML](@source [nvarchar](max))
RETURNS [nvarchar](max) WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SQLfuncs].[SQLfuncs.StringFunctions].[JSONtoXML]

----

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
ALTER FUNCTION [dbo].[CLR_XMLtoJSON](@source [nvarchar](max))
RETURNS [nvarchar](max) WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SQLfuncs].[SQLfuncs.StringFunctions].[XMLtoJSON]
-----

SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER OFF
GO

CREATE FUNCTION [dbo].[CLR_UTF8ToUnicode](@source [nvarchar](max))
RETURNS [nvarchar](max) WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SQLfuncs].[SQLfuncs.StringFunctions].[AsUTF8]
GO

-----

SET ANSI_NULLS OFF
GO

SET QUOTED_IDENTIFIER OFF
GO

CREATE FUNCTION [dbo].[CLR_UnicodeToUTF8](@source [nvarchar](max))
RETURNS [nvarchar](max) WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SQLfuncs].[SQLfuncs.StringFunctions].[AsWin1251]
GO

----

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
ALTER FUNCTION [dbo].[CLR_URLDecode](@source [nvarchar](max))
RETURNS [nvarchar](max) WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SQLfuncs].[SQLfuncs.StringFunctions].[URLDecode]

---

SET ANSI_NULLS OFF
GO
SET QUOTED_IDENTIFIER OFF
GO
ALTER FUNCTION [dbo].[CLR_URLEncode](@source [nvarchar](max))
RETURNS [nvarchar](max) WITH EXECUTE AS CALLER
AS 
EXTERNAL NAME [SQLfuncs].[SQLfuncs.StringFunctions].[URLEncode]

