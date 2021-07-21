

declare @uni nvarchar(max)
declare @utf nvarchar(max)
select @uni=[dbo].[CLR_UTF8ToUnicode]('Привет, это проверка of function')
select @utf=[dbo].[CLR_UnicodeToUTF8](@uni)
select @utf, @uni
SELECT [dbo].[CLR_HttpGetBlob] ('http://static-maps.yandex.ru/1.x/?ll=37.511275,55.718654&size=541,450&z=15&l=map&pt=37.511275,55.718654,pm2grl')

declare @src_email		varchar(max)	= 'user_New@post.su'
declare @src_phone		varchar(20)		= '9991234567'
declare @src_birthday	datetime		= '20000714'
declare @src_firstName	varchar(max)	= 'Пупкин'
declare @src_lastName	varchar(max)	= 'Епифан'

declare @email		varchar(max)	= @src_email
declare @password	varchar(max)	= dbo.[CLR_UTF8ToUnicode]('DerПароль')--LOWER(SUBSTRING(CAST(newid() as varchar(100)),25,12))
declare @birthday	varchar(10)		= CONVERT(varchar(10),@src_birthday,105)
declare @phone		varchar(20)		= @src_phone
declare @firstName	varchar(max)	= dbo.[CLR_UTF8ToUnicode](@src_firstName)
declare @lastName	varchar(max)	= dbo.[CLR_UTF8ToUnicode](@src_lastName)
declare @srcURL		nvarchar(max)	= N'http://www.my220-kupivip.ru/account/coregistration'
declare @srcParam	nvarchar(max)	= N'email='+@email+'&password='+@password+'&phone='+@phone+'&birthday='+@birthday+'&sex=m&firstName='+@firstName+'&lastName='+@lastName
declare @srcEnc		nvarchar(max)	= N'application/x-www-form-urlencoded'
declare @resJSON	nvarchar(max)	= ''
--select @srcParam
select @resJSON=[dbo].[CLR_UnicodeToUTF8](convert(varchar(max), [dbo].[CLR_HttpGetBlobPost] (@srcURL ,@srcParam, @srcEnc)))
select @resJSON


/*
declare @email		varchar(max)	= 'user_New1@post.su'
declare @phone		varchar(20)		= '9991234567'
declare @birthday	datetime		= null--'20000714'
declare @firstName	varchar(max)	= 'Пупкин'
declare @lastName	varchar(max)	= 'Епифан'
declare @sex		varchar			= 'm'
declare @orderno	int				= 1239876
declare @res		int
exec @res=[dbo].[SRM_SiteRegistrationWithNotify2] @email,@phone,@birthday,@firstName,@lastName,@sex,@orderno
select @res
--select * from mo.dbo.Email_Head where Recipients = 'shoptime2052@qip.ru'
*/

   