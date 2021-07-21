using System;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Text;
//using Newtonsoft.Json;

namespace SQLfuncs
{

    public class ReadWriteBlob
    {
        // ---- with "using" metod ----------------------
        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlBytes ReadBlob(SqlString url)
        {
            SqlBytes blob = null;
            try
            {
                string HttpUrl = url.Value;
                Uri urlCheck = new Uri(HttpUrl);
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                    using (Stream data = wc.OpenRead(urlCheck))
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            data.CopyTo(ms);
                            ms.Position = 0;
                            byte[] res = new byte[ms.Length];
                            res = ms.ToArray();
                            blob = new SqlBytes(res);
                            res = null;
                        }
                    }
                }
            }
            catch { }
            return blob;
        }
        
    

    public class ReadWriteBlobPost
    {
        // ---- with "using" metod ----------------------
        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlBytes ReadBlobPost(SqlString url, SqlString parameters, SqlString contenttype)
        {SqlBytes blob = null;
            try 
            {
            string HttpUrl = url.Value;
            string HttpData = parameters.Value;
            WebRequest req = WebRequest.Create(HttpUrl);
            req.Method = "POST";
            req.Timeout = 100000;
            req.ContentType = contenttype.Value;
            byte[] sentData = Encoding.GetEncoding(1251).GetBytes(HttpData);
            req.ContentLength = sentData.Length;
            Stream sendStream = req.GetRequestStream();
            sendStream.Write(sentData, 0, sentData.Length);
            WebResponse res = req.GetResponse();
            byte[] bytes = null;
            using (Stream ReceiveStream = res.GetResponseStream())
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        ReceiveStream.CopyTo(ms);
                        ms.Position = 0;
                        bytes = new byte[ms.Length];
                        bytes = ms.ToArray();
                        blob = new SqlBytes(bytes);
                    }
                }              
            bytes = null;
            res=null;
            sentData = null;
            req = null;
            }
            catch {}
            return blob;
        }

   
    public class StringFunctions
    {
        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlString UnicodeToUTF8(SqlString source)
        {
            var srcStr = source.Value;
            Encoding src = Encoding.GetEncoding(1251);
            return Encoding.UTF8.GetString(src.GetBytes(srcStr));
        }

        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlString UTF8ToUnicode(SqlString source)
        {
            var srcStr = source.Value;
            Encoding src = Encoding.UTF8;
            return Encoding.GetEncoding(1251).GetString(src.GetBytes(srcStr));
        }

        // ---- with "using" metods ----------------------
        public static SqlString JSONtoXML(SqlString value)
        {
            string xml = "";
            try 
            {
            string json = value.Value;
            json = json.Replace("&", "&amp;");
            json = json.Replace("<", "&lt;");
            json = json.Replace(">", "&gt;");
            json = json.Replace("'", "&apos;");
            json = "{\"ITEM\":" + json.Replace("{", "{") + "}"; // это наименование отдельного объекта
            using (MemoryStream stream = new MemoryStream())
            {
                var doc = Newtonsoft.Json.JsonConvert.DeserializeXmlNode(json, "LIST"); // это root, необходимо для этой реализации
                doc.Save(stream);
                stream.Position = 0;
                using (StreamReader sr = new StreamReader(stream))
                {
                    xml = sr.ReadToEnd();
                }
                doc = null;                
            }
            json = null;
            }
            catch{}
            return xml;
        }

        // ---- with "Dispose()" and "Close()" metods ----------------------
        public static SqlString JSONtoXML_OLD(SqlString value)
        {
            string xml = "";
            string json = value.Value;
            json = json.Replace("&", "&amp;");
            json = json.Replace("<", "&lt;");
            json = json.Replace(">", "&gt;");
            json = json.Replace("'", "&apos;");
            json = "{\"ITEM\":" + json.Replace("{", "{") + "}"; // это наименование отдельного объекта
            MemoryStream stream = new MemoryStream();
            var doc = Newtonsoft.Json.JsonConvert.DeserializeXmlNode(json, "LIST"); // это root, необходимо для этой реализации
            doc.Save(stream);
            stream.Position = 0;
            StreamReader sr = new StreamReader(stream);
            xml = sr.ReadToEnd();
            doc = null;
            sr.Close();
            sr.Dispose();
            sr = null;
            stream.Close();
            stream.Dispose();
            stream = null;
            return xml;
        }
    }
}