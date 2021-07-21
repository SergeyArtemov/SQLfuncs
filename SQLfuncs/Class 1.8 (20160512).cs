using System;
using System.Collections;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.Net;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;
using System.Windows.Forms;
using SQLfuncs.LevisWS;
using System.Web.Services;
//using System.Security.Cryptography.X509Certificates; //for ssl use
using System.Web.Services.Protocols;
using System.Xml; // -- XMLtoJSON
using Newtonsoft.Json; // -- JSONtoXML, XMLtoJSON
//using System.Drawing; //-- HtmlGetter
//using TheArtOfDev; //-- HtmlGetter
using System.IO.Compression;

/// Новое для 1.8.3 на 20160831 -------------------------------------------------
/// 1. Emarsys.Send расширенная обработка Exception (ошибка + | + сообщение от сервиса)
/// Новое для 1.8.2 на 20160816 -------------------------------------------------
/// 1. StringFunctions.HMACSHA1Encode
/// Новое для 1.8.1 на 20160520 -------------------------------------------------
/// 1. StringFunctions.SaveBinaryToFile
/// 2. StringFunctions.GZipToString
/// Новое для 1.8.0.0 на 20160512 ---------------------------------------------- 
/// 1. StringFunctions.XMLtoJSON
/// 2. добавлен класс Navision
/// 3. HTMLGetter.html_DecodeFromUTF8
/// 4. HTMLGetter.html_DecodeFromKOI8R
/// 5. HTMLGetter : есть закомментаренные функции


namespace SQLfuncs
{
    // NOTE:
    // http://www.diogonunes.com/blog/webclient-vs-httpclient-vs-httpwebrequest/ (HttpWebRequest, WebClient, HttpClient)
    // --------------------------------------------------------------------------
    // about collations
    // https://technet.microsoft.com/en-us/library/ms131091%28v=sql.110%29.aspx



    // https://documentation.emarsys.com/resource/developers/api/getting-started/authentication/c-sharp-sample/
// -- begin of "Emarsys" class ---------------------------------------------------------------------------
    class Emarsys
    {
        private readonly string baseURI;
        private readonly string key;
        private readonly string secret;

        public Emarsys(string baseURI, string key, string secret)
        {
            this.baseURI = baseURI;
            this.key = key;
            this.secret = secret;
        }


        public byte[] Send(string method, string uri, string postData, out string err)
        {
            err = "";
            byte[] bytes = new byte[0];
            try
            {
                var nonce = GetRandomString(32);
                var timestamp = DateTime.UtcNow.ToString("o");
                var passwordDigest = System.Convert.ToBase64String(Encoding.UTF8.GetBytes(Sha1(nonce + timestamp + secret)));
                var authHeader = String.Format("Username=\"{0}\", PasswordDigest=\"{1}\", Nonce=\"{2}\", Created=\"{3}\"", key, passwordDigest, nonce, timestamp);
                var httpRequest = (HttpWebRequest)WebRequest.Create(this.baseURI + uri);
                httpRequest.Method = method;
                //httpRequest.Timeout = 100000;
                httpRequest.Headers.Add("X-WSSE: UsernameToken " + authHeader);
                if ((method.Equals("POST"))||(method.Equals("PUT")))
                {
                    var data = Encoding.UTF8.GetBytes(postData);
                    httpRequest.ContentType = "application/json;charset=\"utf-8\"";
                    httpRequest.ContentLength = data.Length;
                    using (var stream = httpRequest.GetRequestStream())
                    {
                        stream.Write(data, 0, data.Length);
                    }
                }
                HttpWebResponse res = (HttpWebResponse)httpRequest.GetResponse();
                Stream ReceiveStream = res.GetResponseStream();
                bytes = GetStreamBytes(ReceiveStream);
                return bytes;
            }
            catch (Exception e)
            {
                //err = e.Message;
                //return bytes;
                //----------------------------------------
                try
                {
                    WebResponse res = (e as System.Net.WebException).Response;
                    byte[] errbytes = null;
                    using (Stream ReceiveStream = res.GetResponseStream())
                    {
                        using (MemoryStream ms = new MemoryStream())
                        {
                            ReceiveStream.CopyTo(ms);
                            ms.Position = 0;
                            errbytes = new byte[ms.Length];
                            errbytes = ms.ToArray();
                        }
                    }
                    StringBuilder result = new StringBuilder(e.Message + '|');
                    if (errbytes.Length != 0) {result.Append(System.Text.Encoding.UTF8.GetString(errbytes));};
                    err = result.ToString();
                    return bytes;
                }
                catch (Exception exc)
                {
                    bytes = Encoding.GetEncoding(1251).GetBytes(exc.Message);
                    return bytes;
                }
                //-----------------------------------------
            }
        }

        private static string Sha1(string input)
        {
            var hashInBytes = new SHA1CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(input));
            return string.Join(string.Empty, Array.ConvertAll(hashInBytes, b => b.ToString("x2")));
        }

        private static string GetRandomString(int length)
        {
            var random = new Random();
            string[] chars = new string[] { "0", "2", "3", "4", "5", "6", "8", "9", "a", "b", "c", "d", "e", "f", "g", "h", "j", "k", "m", "n", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };
            var sb = new StringBuilder();
            for (int i = 0; i < length; i++) sb.Append(chars[random.Next(chars.Length)]);
            return sb.ToString();
        }

        public static byte[] GetStreamBytes(Stream srcStream)
        {
            MemoryStream ms = new MemoryStream();
            srcStream.CopyTo(ms);
            long len = ms.Length;
            ms.Position = 0;
            byte[] readBuffer = new byte[len];
            List<byte> outputBytes = new List<byte>();
            int offset = 0;
            while (true)
            {
                int bytesRead = ms.Read(readBuffer, 0, readBuffer.Length);
                if (bytesRead == 0)
                {
                    break;
                }
                else if (bytesRead == readBuffer.Length)
                {
                    outputBytes.AddRange(readBuffer);
                }
                else
                {
                    byte[] tempBuf = new byte[bytesRead];
                    Array.Copy(readBuffer, tempBuf, bytesRead);
                    outputBytes.AddRange(tempBuf);
                    break;
                }
                offset += bytesRead;
            }
            return outputBytes.ToArray();
        }


        private static void Print(object data, string prefix = "")
        {
            if (data is Dictionary<string, object>)
            {
                var dict = data as Dictionary<string, object>;
                foreach (var key in dict.Keys)
                {
                    Console.WriteLine(prefix + "-" + key + ":");
                    Print(dict[key], prefix + "  ");
                }
            }
            else if (data is System.Collections.IEnumerable && !(data is string))
            {
                foreach (var item in data as System.Collections.IEnumerable)
                {
                    Print(item, prefix + "  ");
                }
            }
            else
            {
                Console.WriteLine(prefix + data.ToString());
            }
        }
    }
// -- end of "Emarsys" class -----------------------------------------------------------------------------------------------------------

// -- begin of "Levis" class -----------------------------------------------------------------------------------------------------------
    public class Levis 
    {
        private String EndPoint;
        private String login;
        private String password;

        public Levis(string EndPoint = "" , string login = "" , string password = "")
        {
            if ("" == EndPoint)
            { this.EndPoint = SQLfuncs.Properties.Settings.Default.SQLfuncs_LevisWS_Kupi; } /*LEVIS URL*/ // можно тут подменить URL
            else
            { this.EndPoint = EndPoint; }
            if ("" == login)
            { this.login = "wms_ecom"; }
            else
            { this.login = login; }
            if ("" == password)
            { this.password = "XfrYjhhbc!"; }
            else
            { this.password = password; }
        }

        //private static bool TrustAllCertificateCallback(object sender, X509Certificate cert, X509Chain chain, System.Net.Security.SslPolicyErrors errors)
        //{
        //    return true;
        //}

        private Kupi GetWS(out String error)
        {
            try
            {
                /*method 1*///ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(TrustAllCertificateCallback);
                /*method 2*/  ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => { return true; };
                Kupi KupiInst = new Kupi();
                //KupiInst.Url=KupiInst.Url.Replace("89.17.48.79", "89.17.48.79:61454");
                //-------- http://stackoverflow.com/questions/4945420/how-can-i-add-a-basic-auth-header-to-my-soap -------------------------------------------
                // Create the network credentials and assign them to the service credentials
                NetworkCredential netCredential = new NetworkCredential(login, password);
                Uri uri = new Uri(KupiInst.Url);
                ICredentials credentials = netCredential.GetCredential(uri, "Basic");
                KupiInst.Credentials = credentials;
                KupiInst.PreAuthenticate = true;
                //--------------------------------------------------------------------------------------------------------------------------------------------
                KupiInst.Timeout = 10*60*1000;
                error = "";
                return KupiInst;
            }
            catch(Exception E)
            {
                error = E.Message;
                return null; 
            }


        }
    
        public String DoPingPong()
        {
            String error = "";
            using (Kupi KupiInst = GetWS(out error))
            {
            if ((null == KupiInst) || ("" != error))
                { throw new Exception(error); }
            try{return KupiInst.PingPong();}
            catch (Exception e){return e.Message+".URL: "+KupiInst.Url;}
            }
        }

        public String DoGetPackage(String package)
        {
            String error = "";
            using (Kupi KupiInst = GetWS(out error))
            {
                if ((null == KupiInst) || ("" != error))
                { throw new Exception(error); }
            try
            {
                String result;
                String resPackage = package;
                StringBuilder StrBld = new StringBuilder();
                result = KupiInst.GetPackage(out resPackage);
                StrBld.Append(resPackage);
                StrBld.Append(result);
                return StrBld.ToString();
            }
            catch (Exception e){return e.Message + ".URL: " + KupiInst.Url;}
           }
        }

        public String DoPutPackage(String package)
        {
            String error = "";
            using (Kupi KupiInst = GetWS(out error))
            {
            if ((null == KupiInst) || ("" != error))
                { throw new Exception(error); }
            try{return KupiInst.PutPackage(package);}
            // into UTF8 //try { return KupiInst.PutPackage(Encoding.UTF8.GetString(Encoding.GetEncoding(1251).GetBytes(package))); }
            catch (Exception e){return e.Message + ".URL: " + KupiInst.Url;}
            }
        }
    
    }

// -- end of "Levis" class -----------------------------------------------------------------------------------------------------------

    public class ReadWriteBlob
    {
        // -- old ------
        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlBytes ReadBlob(SqlString url)
        {          
            SqlBytes blob = null;
            if ((url.IsNull == true) || (url.Value == "")) { return blob; }
            try
            {
                Uri urlCheck = new Uri(url.Value);
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
                    wc.Headers.Clear();
                }
                urlCheck = null;
            }
            catch {blob = null; }
            GC.Collect();
            return blob;
        }
        // -- old ------
        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlBytes ReadBlobEx(SqlString url, SqlString headers)
        {
            SqlBytes blob = null;
            if (url.IsNull == true) { return blob; }
            try
            {
                Uri urlCheck = new Uri(url.Value);
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add("user-agent", "Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)");
                    if ((!headers.IsNull) && (headers != ""))// { wc.Headers.Add(headers.Value); }
                        {
                            string[] dophdr = headers.Value.Split(';');
                            for (int i = 0; i < dophdr.Length; i++)
                            {
                                wc.Headers.Add(dophdr[i]);
                            }
                        }
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
                    wc.Headers.Clear();
                }
                urlCheck = null;
            }
            catch(Exception e) 
            {
                byte[] errbytes;
                errbytes = Encoding.GetEncoding(1251).GetBytes(e.Message);
                blob = new SqlBytes(errbytes);
                errbytes = null;
            }
            GC.Collect();
            return blob;
        }
        // -- new ------
        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlBytes HttpGet(SqlString url, SqlString headers)
        {
            SqlBytes blob = null;
            string Errors = "";
            if ((url.IsNull == true) || (url.Value == "")) { Errors = "Empty URL"; return blob; }
            try
            {
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url.Value);
                req.Method = "GET";
                req.Timeout = 1000;
                req.ContentType = "";
                req.Accept = "text/html, application/xhtml+xml, */*";
                req.UserAgent="Mozilla/4.0 (compatible; MSIE 6.0; Windows NT 5.2; .NET CLR 1.0.3705;)";
                
                if ((!headers.IsNull)&&(headers!=""))
                    //{req.Headers.Add(headers.Value);}
                {
                    string[] dophdr = headers.Value.Split(';');
                    for (int i = 0; i < dophdr.Length; i++)
                    {
                        req.Headers.Add(dophdr[i]);
                    }
                }
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
                res = null;
                req = null;
            }
            catch(Exception E)
            {
                Errors = E.Message;
                blob = null;
            }
            GC.Collect();
            return blob;
        }
        // -- new ------
        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlString HttpGetString(SqlString url)
        {
            SqlString result = null;
            if ((url.IsNull == true) || (url.Value == "")) { return result; }
            try
            {
                using (WebClient wc = new WebClient())
                {
                    result = wc.DownloadString(url.Value);

                }
            }
            catch { result = null; }
            GC.Collect();
            return result;
        }
        // -- new ------
        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlBytes HttpGetBinary(SqlString url)
        {
            SqlBytes result = null;
            if ((url.IsNull == true) || (url.Value == "")) { return result; }
            try
            {
                using (WebClient wc = new WebClient())
                {
                     byte[] bytes = wc.DownloadData(url.Value);
                     result = new SqlBytes(bytes);
                     bytes = null;
                }
            }
            catch { result = null; }
            GC.Collect();
            return result;
        }
        // -- 20151111 ------
        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlString HttpGetStringAuth(SqlString url, SqlString user, SqlString password)
        {
            SqlString result = null;
            if ((url.IsNull == true) || (url.Value == "")) { return result; }
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Credentials = new NetworkCredential(user.Value, password.Value);
                    result = wc.DownloadString(url.Value);
                }
            }
            catch { result = null; }
            GC.Collect();
            return result;
        }
        // -- 20151111 ------
        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlBytes HttpGetBinaryAuth(SqlString url, SqlString user, SqlString password)
        {
            SqlBytes result = null;
            if ((url.IsNull == true) || (url.Value == "")) { return result; }
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Credentials = new NetworkCredential(user.Value, password.Value);
                    byte[] bytes = wc.DownloadData(url.Value);
                    result = new SqlBytes(bytes);
                    bytes = null;
                }
            }
            catch { result = null; }
            GC.Collect();
            return result;
        }



        
    } // --> end of "ReadWriteBlob" class ------------------------------------------------------------------
    public class ReadWriteBlobPost
    {

        //public static SqlBytes GetSqlBytes(Stream srcStream)
        //{
        //    SqlBytes blob = null;
        //    try
        //    {
        //        using (MemoryStream ms = new MemoryStream())
        //        {
        //            srcStream.CopyTo(ms);
        //            ms.Position = 0;
        //            byte[] bytes = new byte[ms.Length];
        //            bytes = ms.ToArray();
        //            blob = new SqlBytes(bytes);
        //        }

        //    }
        //    catch { blob = null; }
        //    return blob;
        //}

        public static void FillHeaders(String aHeaders, WebHeaderCollection Dest)
        {
            if (aHeaders != "")
            {
                string[] dophdr = aHeaders.Split(';');
                for (int i = 0; i < dophdr.Length; i++)
                {
                    try
                    {
                        Dest.Add(dophdr[i]);
                    }
                    catch { }
                }
            }
        }



        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlBytes ReadBlobPost(SqlString url, SqlString parameters, SqlString contenttype)
        {
            SqlBytes blob = null;
            if (url.IsNull == true) { return blob; }
            try
            {
                string HttpData = "";
                if (parameters.IsNull == false) {HttpData = parameters.Value;}
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url.Value);
                req.Method = "POST";
                req.Timeout = 100000;
                if (contenttype.IsNull == true)
                { req.ContentType = ""; } 
                else 
                {req.ContentType = contenttype.Value;}
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
                res = null;
                sentData = null;
                req = null;
            }
            catch
            {
                blob = null; 
            }
            GC.Collect();
            return blob;
        }

        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlBytes ReadBlobPostEx(SqlString url, SqlString headers, SqlString parameters, SqlString contenttype)
        {
            SqlBytes blob = null;
            if (url.IsNull == true) { return blob; }
            try
            {
                string HttpData = "";
                if (parameters.IsNull == false) { HttpData = parameters.Value; }
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url.Value);
                /*ATTENTION*/
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => { return true; };
                //https://yandex.ru/search/?text=ServicePointManager.ServerCertificateValidationCallback%20%2B%3D%20%28sender%2C%20certificate%2C%20chain%2C%20sslPolicyErrors%29%20%3D%3E%20true%3B&lr=213&clid=1923018
                //http://stackoverflow.com/questions/12506575/how-to-ignore-the-certificate-check-when-ssl
                //http://www.khalidabuhakmeh.com/validate-ssl-certificate-with-servicepointmanager
                //https://msdn.microsoft.com/en-us/library/system.net.servicepointmanager.servercertificatevalidationcallback%28v=vs.110%29.aspx
                /*ATTENTION*/
                req.Method = "POST";
                req.Timeout = 100000;
                if (contenttype.IsNull == true)
                { req.ContentType = ""; }
                else
                { req.ContentType = contenttype.Value; }
                byte[] sentData = Encoding.GetEncoding(1251).GetBytes(HttpData);
                ////req.Headers.Add(headers.Value);
                //if ((!headers.IsNull) && (headers != ""))
                //{
                //    string[] dophdr = headers.Value.Split(';');
                //    for (int i = 0; i < dophdr.Length; i++)
                //    {
                //        req.Headers.Add(dophdr[i]);
                //    }
                //}
                FillHeaders(headers.Value, req.Headers);
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
                res = null;
                sentData = null;
                req = null;
            }
            catch (Exception e)
            {
                byte[] errbytes;
                errbytes = Encoding.GetEncoding(1251).GetBytes(e.Message);
                blob = new SqlBytes(errbytes);
                errbytes = null;
            }
            GC.Collect();
            return blob;
        }

        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlBytes ReadBlobMethod(SqlString url, SqlString Method, SqlString headers, SqlString parameters, SqlString contenttype)
        {
            SqlBytes blob = null;
            if (url.IsNull == true) { return blob; }
            try
            {
                string HttpData = "";
                if (parameters.IsNull == false) { HttpData = parameters.Value; }
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url.Value);
                /*ATTENTION*/
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => { return true; };
                //https://yandex.ru/search/?text=ServicePointManager.ServerCertificateValidationCallback%20%2B%3D%20%28sender%2C%20certificate%2C%20chain%2C%20sslPolicyErrors%29%20%3D%3E%20true%3B&lr=213&clid=1923018
                //http://stackoverflow.com/questions/12506575/how-to-ignore-the-certificate-check-when-ssl
                //http://www.khalidabuhakmeh.com/validate-ssl-certificate-with-servicepointmanager
                //https://msdn.microsoft.com/en-us/library/system.net.servicepointmanager.servercertificatevalidationcallback%28v=vs.110%29.aspx
                /*ATTENTION*/
                req.Method = Method.Value;
                req.Timeout = 100000;
                if (contenttype.IsNull == true)
                { req.ContentType = ""; }
                else
                { req.ContentType = contenttype.Value; }
                //req.Headers.Add(headers.Value);
                //if ((!headers.IsNull) && (headers != ""))
                //{
                //    string[] dophdr = headers.Value.Split(';');
                //    for (int i = 0; i < dophdr.Length; i++)
                //    {
                //        if ("" != dophdr[i])
                //            try
                //            {
                //                req.Headers.Add(dophdr[i]);
                //            }
                //            catch { }
                //    }
                //}
                FillHeaders(headers.Value, req.Headers);
                if (req.Method.ToUpper() != "GET")
                {
                    byte[] sentData = Encoding.GetEncoding(1251).GetBytes(HttpData);
                    req.ContentLength = sentData.Length;
                    Stream sendStream = req.GetRequestStream();
                    sendStream.Write(sentData, 0, sentData.Length);
                    sentData = null;
                }
                WebResponse res = req.GetResponse();
                byte[] bytes = null;
                using (Stream ReceiveStream = res.GetResponseStream())
                {
                    //blob = GetSqlBytes(ReceiveStream);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        ReceiveStream.CopyTo(ms);
                        ms.Position = 0;
                        bytes = new byte[ms.Length];
                        bytes = ms.ToArray();
                        if (bytes.Length == 0)
                            blob = new SqlBytes(new byte[] { 0x00});
                         else 
                            blob = new SqlBytes(bytes);
                    }
                }
                bytes = null;
                res = null;   
                req = null;
            }
            catch (Exception e)
            {

                try
                {
                    WebResponse res = (e as System.Net.WebException).Response;
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
                    StringBuilder result = new StringBuilder(e.Message + '|');
                    if (bytes.Length != 0) { result.Append(System.Text.Encoding.UTF8.GetString(bytes)); };
                    byte[] errbytes;
                    errbytes = Encoding.GetEncoding(1251).GetBytes(result.ToString());
                    blob = new SqlBytes(errbytes);
                    errbytes = null;
                }
                catch (Exception exc) 
                {
                    byte[] errbytes;
                    errbytes = Encoding.GetEncoding(1251).GetBytes(exc.Message);
                    blob = new SqlBytes(errbytes);
                    errbytes = null;
                }
            }
            GC.Collect();            
            return blob;
        }

        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlBytes ReadBlobMethodEx(SqlString url, SqlString Method, SqlString headers, SqlString parameters, SqlString contenttype, out SqlString debug)
        {
            SqlBytes blob = null;
            debug.Value = "";
            if (url.IsNull == true) { return blob; }
            try
            {
                string HttpData = "";
                if (parameters.IsNull == false) { HttpData = parameters.Value; }
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url.Value);
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => { return true; };
                req.Method = Method.Value;
                req.Timeout = 100000;
                if (contenttype.IsNull == true)
                { req.ContentType = ""; }
                else
                { req.ContentType = contenttype.Value; }
                //req.Headers.Add(headers.Value);
                //if ((!headers.IsNull) && (headers != ""))
                //{
                //    string[] dophdr = headers.Value.Split(';');
                //    for (int i = 0; i < dophdr.Length; i++)
                //    {
                //        if ("" != dophdr[i])
                //            try
                //            {
                //                req.Headers.Add(dophdr[i]);
                //            }
                //            catch { }
                //    }
                //}
                FillHeaders(headers.Value, req.Headers);
                if (req.Method.ToUpper() != "GET")
                {
                    byte[] sentData = Encoding.GetEncoding(1251).GetBytes(HttpData);
                    req.ContentLength = sentData.Length;
                    Stream sendStream = req.GetRequestStream();
                    sendStream.Write(sentData, 0, sentData.Length);
                    sentData = null;
                }
                WebResponse res = req.GetResponse();
                byte[] bytes = null;
                using (Stream ReceiveStream = res.GetResponseStream())
                {
                    //blob = GetSqlBytes(ReceiveStream);
                    using (MemoryStream ms = new MemoryStream())
                    {
                        ReceiveStream.CopyTo(ms);
                        ms.Position = 0;
                        bytes = new byte[ms.Length];
                        bytes = ms.ToArray();
                        if (bytes.Length == 0)
                            blob = new SqlBytes(new byte[] { 0x00 });
                        else
                            blob = new SqlBytes(bytes);
                    }
                }
                bytes = null;
                res = null;
                req = null;
            }
            catch (Exception e)
            {

                try
                {
                    WebResponse res = (e as System.Net.WebException).Response;
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
                    StringBuilder result = new StringBuilder(e.Message + '|');
                    if (bytes.Length != 0) { result.Append(System.Text.Encoding.UTF8.GetString(bytes)); };
                    byte[] errbytes;
                    errbytes = Encoding.GetEncoding(1251).GetBytes(result.ToString());
                    blob = new SqlBytes(errbytes);
                    errbytes = null;
                }
                catch (Exception exc)
                {
                    byte[] errbytes;
                    errbytes = Encoding.GetEncoding(1251).GetBytes(exc.Message);
                    blob = new SqlBytes(errbytes);
                    errbytes = null;
                }
            }
            GC.Collect();
            return blob;
        }

        
    } // --> class end
    public class StringFunctions
    {
        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlString UnicodeToUTF8(SqlString source) // правильнее будет AsWin1251(SqlString source)
        {
            if (source.IsNull == true) { return ""; }
            var srcStr = source.Value;
            Encoding src = Encoding.GetEncoding(1251);
            GC.Collect();
            return Encoding.UTF8.GetString(src.GetBytes(srcStr));
        }
        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlString UTF8ToUnicode(SqlString source) // правильнее будет AsUTF8(SqlString source)
        {
            if (source.IsNull == true) { return ""; }
            var srcStr = source.Value;
            Encoding src = Encoding.UTF8;
            GC.Collect();
            return Encoding.GetEncoding(1251).GetString(src.GetBytes(srcStr));
        }
        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlString JSONtoXML(SqlString value)
        {
            if (value.IsNull == true) { return ""; }
            string xml = "";
            try
            {
                string json = value.Value;
                json = "{\"ITEM\":" + json.Replace("{", "{") + "}"; // это наименование отдельного объекта
                using (MemoryStream stream = new MemoryStream())
                {
                    var doc = Newtonsoft.Json.JsonConvert.DeserializeXmlNode(json, "LIST"); // это root, необходимо для этой реализации
                    // HELP here:
                    //http://www.newtonsoft.com/json/help/html/M_Newtonsoft_Json_JsonConvert_DeserializeXmlNode_1.htm
                    // SOURCE here:
                    //https://github.com/JamesNK/Newtonsoft.Json/blob/master/Src/Newtonsoft.Json/JsonConvert.cs 
                    //public static XmlDocument DeserializeXmlNode(string value, string deserializeRootElementName, bool writeArrayAttribute)
                    //https://github.com/JamesNK/Newtonsoft.Json/blob/master/Src/Newtonsoft.Json/Converters/XmlNodeConverter.cs
                    //public string DeserializeRootElementName { get; set; }
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
            //catch(Exception e) {xml = e.Message;}
            catch { xml = ""; }
            return xml;
        }
        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlString XMLtoJSON(SqlString value)
        {
            if ((value.IsNull == true)||("" == value.Value)) { return ""; }
            string json = "";
            XmlDocument doc = new XmlDocument();
            try
            {
                doc.LoadXml(value.Value);
                json = JsonConvert.SerializeXmlNode(doc);
            }
            catch { json = "{}"; }
            finally { doc = null; }
            return json;
        }


        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlString URLEncode(SqlString source)// кодирование : привет ->  %D0%BF%D1%80%D0%B8%D0%B2%D0%B5%D1%82    
        {
            var res = "";
            if (source.IsNull == true) { return res; }
            var srcStr = source.Value;
            try
            {
            //return HttpUtility.UrlEncode(srcStr); // in assembly System.Web
            res=Uri.EscapeDataString(srcStr);
            }
            catch{}
            GC.Collect();
            return res;
        }
        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlString URLDecode(SqlString source)// декодирование : %D0%BF%D1%80%D0%B8%D0%B2%D0%B5%D1%82 -> привет     
        {
            var res = "";
            if (source.IsNull == true) { return res; }
            var srcStr = source.Value;
            //return HttpUtility.UrlDecode(srcStr); // in assembly System.Web
            try { res = Uri.UnescapeDataString(srcStr); }
            catch {}
            GC.Collect();
            return res;
        }
  // -- Emarsys classes and procedures BEGIN ----------------------------------------------------------
        // about CLR with table return :  https://msdn.microsoft.com/en-us/library/ms131103.aspx
        private class EmarsysResult
        {
            public SqlString Result;
            public SqlString Error;

            public EmarsysResult(SqlString result, SqlString error)
            {
                Result = result;
                Error = error;
            }
        }
        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read
                                               , FillRowMethodName = "Request_FillRow"
                                               , TableDefinition = "result nvarchar(max), error nvarchar(max)")]
        public static IEnumerable Request(SqlString Method, SqlString URI, SqlString json)
        {

            string err  = "";
            string res  = "";
            Emarsys ES  = null;
            ArrayList resultCollection = new ArrayList();
            byte[] bytes;
            try
            {                                                       
                /// старая //ES = new Emarsys("https://suite10.emarsys.net/api/v2/", "kupivip_ru001", "br1W6j57behQnuSQVXib"); 
                /*new letter from 20160323*/ //ES = new Emarsys("https://suite10.emarsys.net/api/v2/", "kupivip_ru005", "QJGP67LMDRmeTet4vFZJ"); 
                /*new letter from 20160323*/  ES = new Emarsys("https://suite10.emarsys.net/api/v2/", "kupivip_tr002", "EPDE6c2dFfCRkMDwMspV"); 
                // https://suite10.emarsys.net/api/v2/ 217.175.192.4
                // https://api.emarsys.net/api/v2/ 217.175.192.10
                bytes = ES.Send(Method.Value, URI.Value, json.Value, out err);
                res = System.Text.Encoding.UTF8.GetString(bytes);
                resultCollection.Add(new EmarsysResult(res, err));
                return resultCollection;
            }
            catch (Exception e)
            {
                resultCollection.Add(new EmarsysResult("", e.Message));
                return resultCollection;
            }
            finally
            {
                bytes = null;
                ES = null;
            }
        }
        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read
                                               , FillRowMethodName = "Request_FillRow"
                                               , TableDefinition = "result nvarchar(max), error nvarchar(max)")]
        public static IEnumerable RequestEx(SqlString Method, SqlString User, SqlString Password, SqlString URI, SqlString json)
        {

            string err = "";
            string res = "";
            Emarsys ES = null;
            ArrayList resultCollection = new ArrayList();
            byte[] bytes;
            try
            {
                ES = new Emarsys("https://suite10.emarsys.net/api/v2/", User.Value, Password.Value); // or baseURI = https://api.emarsys.net/api/v2/
                // https://suite10.emarsys.net/api/v2/ 217.175.192.4 
                // https://api.emarsys.net/api/v2/ 217.175.192.10
                // "kupivip_ru001", "br1W6j57behQnuSQVXib"
                // "kupivip_ru005", "QJGP67LMDRmeTet4vFZJ"
                bytes = ES.Send(Method.Value, URI.Value, json.Value, out err);
                res = System.Text.Encoding.UTF8.GetString(bytes);
                if (string.IsNullOrEmpty(res)) { res = "empty"; };
                resultCollection.Add(new EmarsysResult(res, err));
                return resultCollection;
            }
            catch (Exception e)
            {
                resultCollection.Add(new EmarsysResult("", e.Message));
                return resultCollection;
            }
            finally
            {
                bytes = null;
                ES = null;
                GC.Collect();
            }
        }
        public static void Request_FillRow(object resultCollection, out SqlString result, out SqlString error)
        {
            EmarsysResult emarsysresult = (EmarsysResult)resultCollection;
            result = emarsysresult.Result;
            error = emarsysresult.Error;
        }
        // -- Emarsys classes and procedures END ----------------------------------------------------------
        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlString SaveToFile(SqlString Data, SqlString Filename)
        {
            StringBuilder s = new StringBuilder("Start.");
            SqlString result = Filename.Value;
            try
            {
                s.Append("FileStream.");
                using (FileStream fs = new FileStream(result.Value, FileMode.Create))
                {
                    s.Append("StreamWriter.");
                    using (StreamWriter sw = new StreamWriter(fs))
                    {
                        s.Append("StreamWriter.WriteLine.");
                        sw.WriteLine(Data.Value);
                        s.Append("StreamWriter.Close.");
                        sw.Close();
                    }
                    s.Append("FileStream.Close.");
                    fs.Close();
                }
            }
            catch (Exception e)
            {
                result = String.Format("ERROR ({0}): {1}",s.ToString(),e.Message);
            }
            s.Clear();
            s = null;
            return result;
        }


        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlString SaveBinaryToFile(SqlBinary Data, SqlString Filename)
        {
            StringBuilder s = new StringBuilder("Start.");
            SqlString result = Filename.Value;
            try
            {
                s.Append("FileStream.");
                using (FileStream fs = new FileStream(result.Value, FileMode.Create))
                {
                    fs.Write(Data.Value,0,Data.Value.Length);
                    s.Append("FileStream.Close.");
                    fs.Close();
                }
            }
            catch (Exception e)
            {
                result = String.Format("ERROR ({0}): {1}", s.ToString(), e.Message);
            }
            s.Clear();
            s = null;
            return result;
        }

        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlBytes GZipToString(SqlBinary Data)
        {
            SqlBytes blob = null;
            if (Data.IsNull==true) return null;
            SqlString result = "";
            try
            {
                using (MemoryStream mem = new MemoryStream(Data.Value))
                {
                    using (GZipStream gz = new GZipStream(mem, CompressionMode.Decompress))
                    {
                        using (var ms = new MemoryStream())
                        {
                          int buffsize = 1048576;
                          var buffer = new Byte[buffsize];
                          int h;
                          while ((h = gz.Read(buffer, 0, buffer.Length)) > 0)
                          {
                              ms.Write(buffer, 0, h);
                          }
                          ms.Position = 0;
                          byte[] res = new byte[ms.Length];
                          res = ms.ToArray();
                          blob = new SqlBytes(res);
                          res = null;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                byte[] errbytes;
                errbytes = Encoding.GetEncoding(1251).GetBytes(e.Message);
                blob = new SqlBytes(errbytes);
                errbytes = null;
            }
            return blob;
        }


        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlString Levis_PingPong()
        {
            try
            {
                Levis levis = new Levis();
                return levis.DoPingPong();
            }
            catch (Exception e)
            { 
                return e.Message; 
            }
        }

        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlString Levis_GetPackage(SqlString package)
        {
            try
            {
                Levis levis = new Levis();
                return levis.DoGetPackage(package.Value);
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlString Levis_PutPackage(SqlString package)
        {
            try
            {
                Levis levis = new Levis();
                return levis.DoPutPackage(package.Value);
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }


        
        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlBytes HMACSHA1Encode(SqlString basestring, SqlString keystring)
        {
            string input = basestring.Value;
            byte[] key=Encoding.ASCII.GetBytes(keystring.Value);
            SqlBytes blob = null;
            try
            {
                HMACSHA1 myhmacsha1 = new HMACSHA1(key);
                byte[] byteArray = Encoding.ASCII.GetBytes(input);
                MemoryStream stream = new MemoryStream(byteArray);
                byte[] hashValue = myhmacsha1.ComputeHash(stream);
                blob = new SqlBytes(hashValue);
            }
            catch (Exception e)
            {
                byte[] errbytes;
                errbytes = Encoding.GetEncoding(1251).GetBytes(e.Message);
                blob = new SqlBytes(errbytes);
                errbytes = null;
            }
            return blob;
            //using (HMACSHA1 myhmacsha1 = new HMACSHA1(key);
            //byte[] byteArray = Encoding.ASCII.GetBytes(input);
            //MemoryStream stream = new MemoryStream(byteArray);
            //return myhmacsha1.ComputeHash(stream).Aggregate("", (s, e) => s + String.Format("{0:x2}",e), s => s );
        }

/// ----------------------------------------------------------------------------------------------------------- 


    }// --> end of class StringFunctions

    public class HTMLGetter {
 
        public static string html_DecodeFromUTF8(string srcStr)
        {
            Encoding dest = Encoding.GetEncoding("windows-1251");
            Encoding from = Encoding.GetEncoding("utf-8");
            string result = from.GetString(dest.GetBytes(srcStr));
            dest = null;
            from = null;
            return result;
        }

        public static string html_DecodeFromKOI8R(string srcStr)
        {
            Encoding dest = Encoding.GetEncoding("windows-1251");
            Encoding from = Encoding.GetEncoding("koi8r");
            string result = from.GetString(dest.GetBytes(srcStr));
            dest = null;
            from = null;
            return result;
        }
    
    //    public static bool html_HttpGetStringEx(string url, out string Charset, out string Content)
    //    {
    //        Charset = "";
    //        Content = "";
    //        bool res = false;
    //        if (url == "") { return res; }
    //        try
    //        {
    //            using (WebClient wc = new WebClient())
    //            {
    //                Content = wc.DownloadString(url);
    //                res = (Content != "");
    //                try
    //                {
    //                    Charset = wc.ResponseHeaders[HttpResponseHeader.ContentType];
    //                    if (Charset.ToUpper().Contains("=UTF-8"))
    //                    { Content = html_DecodeFromUTF8(Content); }
    //                    else
    //                        if (Charset.ToUpper().Contains("=KOI8-R"))
    //                        { Content = html_DecodeFromKOI8R(Content); }
    //                }
    //                catch (Exception E) { Charset = string.Format("ERROR: {0}.({1})", E.Message, E.TargetSite); }
    //            }
    //        }
    //        catch (Exception E) { Charset = string.Format("ERROR: {0}.({1})", E.Message, E.TargetSite); res = false; }
    //        return res;
    //    }
    //
    //    [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
    //    public static void GetTextHTML(SqlString URL, out SqlString charset, out SqlString content)
    //    {
    //        charset = "";
    //        content = "";
    //        string Charset = "";
    //        string Content = "";
    //        if (URL.IsNull == true) { return ; }
    //        html_HttpGetStringEx(URL.Value, out Charset, out Content);
    //        charset = Charset;
    //        content = Content;
    //    }
    }// --> end of class HTMLGetter



} // -- end of namespace -----------------------------------------------------