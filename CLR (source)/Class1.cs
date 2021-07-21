using System;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.Net;
using System.IO;
using System.Collections.Generic;

public class ReadWriteBlob
{
    [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
    public static SqlBytes ReadBlob(SqlString url)
    {
        string HttpUrl = url.Value;
        Uri urlCheck = new Uri(HttpUrl);
        WebClient wc = new WebClient();
        wc.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
        Stream data = wc.OpenRead(urlCheck);
        MemoryStream ms = new MemoryStream();
        data.CopyTo(ms);
        SqlBytes blob = new SqlBytes(ms);
        return blob;
    }
}