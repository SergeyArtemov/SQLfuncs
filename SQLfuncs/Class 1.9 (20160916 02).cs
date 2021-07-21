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
//using System.Windows.Forms;
using SQLfuncs.LevisWS;
using System.Web.Services;
//using System.Security.Cryptography.X509Certificates; //for ssl use
using System.Web.Services.Protocols;
using System.Xml; // -- XMLtoJSON
using Newtonsoft.Json; // -- JSONtoXML, XMLtoJSON
//using System.Drawing; //-- HtmlGetter
//using TheArtOfDev; //-- HtmlGetter
using System.IO.Compression; // -- GZip
using System.Text.RegularExpressions; // -- Regex
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;



/// Новое для 1.9.0 на 20160915 -------------------------------------------------
/// . Новый класс [HTTPFunctions]  - обединены классы [ReadWriteBlob] и [ReadWriteBlobPost] 
/// . Перенесен прямой вызов сборщика мусора в начало процедур и фунций (паранойя, эмпирика на KUPIVIP00000\KHS_SQLSERVER.TEST via ProcessExplorer)
/// . ReadWriteBlobPost.ReadBlobMethodEx (возвращает таблицу)
/// . [SQLFuncs].[SQLFuncs.TestFunctions].[TableReturnFunction] (тестовая таблица с разнородными данными)
/// . [SQLFuncs].[SQLFuncs.StringFunctions].[UnicodeToUTF8] !удалена 
/// . [SQLFuncs].[SQLFuncs.StringFunctions].[UTF8ToUnicode] !удалена
/// . [SQLFuncs].[SQLFuncs.StringFunctions].[AsWin1251] добавлена (аналог UnicodeToUTF8 на её замену в дальнейшем)
/// . [SQLFuncs].[SQLFuncs.StringFunctions].[AsUTF8] добавлена (аналог UTF8ToUnicode на её замену в дальнейшем)
/// . [SQLFuncs].[SQLFuncs.Emarsys].[Send] - изменена
/// . [SQLFuncs].[SQLFuncs.HTTPFunctions].[ReadBlob] - изменена
/// . [SQLFuncs].[SQLFuncs.HTTPFunctions].[ReadBlobAsync] - добавлена (см. описание)
/// . [SQLFuncs].[SQLFuncs.HTTPFunctions].[ReadBlobEx] !удалена
/// . [SQLFuncs].[SQLFuncs.HTTPFunctions].[HttpGet] - изменена
/// . [SQLFuncs].[SQLFuncs.HTTPFunctions].[HttpGetString] !удалена
/// . [SQLFuncs].[SQLFuncs.HTTPFunctions].[HttpGetBinary] !удалена
/// . [SQLFuncs].[SQLFuncs.HTTPFunctions].[HttpGetStringAuth] !удалена
/// . [SQLFuncs].[SQLFuncs.HTTPFunctions].[HttpGetBinaryAuth] !удалена
/// . [SQLFuncs].[SQLFuncs.HTTPFunctions].[ReadBlobPost] !удалена
/// . [SQLFuncs].[SQLFuncs.HTTPFunctions].[ReadBlobPostEx] !удалена
/// . [SQLFuncs].[SQLFuncs.HTTPFunctions].[ReadBlobMethod] - изменена
/// . [SQLFuncs].[SQLFuncs.HTTPFunctions].[ReadBlobMethodEx] - изменена
/// . Функционал Emarsys и Levis перенесен в HTTPFunctions
/// Добавлен новый класс "TechFunctions" для отладочных, тестовых и других подобных функциях
/// . [SQLFuncs].[SQLFuncs.TechFunctions].[GetUsedDB] процедура получения и отправки сервера и базы данных текущего соединения
/// . [SQLFuncs].[SQLFuncs.TechFunctions].[TryClearMemory] процедура попытки очистки памяти
/// Добавлен новый класс "CommonFunctions" для общих для всех иных классов функций
/// . CommonFunctions.SendSQLDebugMessage - отправка сообщений в текущем соединении (типа print)
/// . CommonFunctions.NormalizeFileName - нормализация системного пути к файлу или папке
/// . CommonFunctions.GetLogFileName - получение имени файла журнала по серверу,БД и части дня (до и после обеда)
/// . CommonFunctions.WriteLog - запись строки в файл журнала
/// 

    
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
    public class CommonFunctions
    {
        public static void SendSQLDebugMessage(string aMessage)
        {
            GC.Collect();
            SqlContext.Pipe.Send(aMessage);
        }

        private static string NormalizeFileName(string aPath)
        {
            GC.Collect();
            string regexSearch = new string(Path.GetInvalidFileNameChars()) + new string(Path.GetInvalidPathChars());
            Regex r = new Regex(string.Format("[{0}]", Regex.Escape(regexSearch)));
            return r.Replace(aPath, "_");
        }

        public static string GetLogFileName()
        {
            GC.Collect();
            StringBuilder res = new StringBuilder();
            string DayPart;
            if ((DateTime.Now.Hour >= 0) && (DateTime.Now.Hour < 12)) DayPart = "AM"; else DayPart = "PM";
            using (SqlConnection connection = new SqlConnection("context connection=true"))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("select @@SERVERNAME, DB_NAME()", connection);
                using (SqlDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                        res.AppendFormat("{0}{1}({2}).txt", @"C:\tmp\", NormalizeFileName(String.Format("{0}.{1}",reader.GetSqlString(0).Value, reader.GetSqlString(1).Value)), DayPart);
                    else 
                        res.Append(@"C:\tmp\CLR_WorkLog.txt");
                }
                connection.Close();
                command = null;
            }
            return res.ToString();
        }

        public static void WriteLog(string aMessage)
        {
            GC.Collect();
            try 
            {
                string LogFileName = GetLogFileName();
                StringBuilder line  = new StringBuilder(DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss.fff"));
                line.AppendFormat(" {0}",aMessage.Replace("\r"," ").Replace("\n",""));
                    using (FileStream fs = new FileStream(LogFileName, FileMode.Append,FileAccess.Write))
                    {
                        using (StreamWriter sw = new StreamWriter(fs))
                        {
                            sw.WriteLine(line.ToString());
                            sw.Close();
                        }
                        fs.Close();
                    }
                }
           catch
           {
           }
        }


        public static void StreamToBytes(ref Stream aStream, out byte[] Result)
        {
            GC.Collect();
            Result = null;
            try
            {
                using (MemoryStream ms = new MemoryStream())
                {
                    aStream.CopyTo(ms);
                    //ms.Position = 0;
                    Result = new byte[ms.Length];
                    Result = ms.ToArray();
                }
            }
            catch
            {
            }
        }

        public static string Sha1(string input)
        {
            var hashInBytes = new SHA1CryptoServiceProvider().ComputeHash(Encoding.UTF8.GetBytes(input));
            return string.Join(string.Empty, Array.ConvertAll(hashInBytes, b => b.ToString("x2")));
        }


    
    } // --> CommonFunctions END ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

      
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

        private static string GetRandomString(int length)
        {
            // Guid.NewGuid().ToString().ToLower().Replace("-", ""); // 32 символа через GUID (0-9,a-f)
            var random = new Random();
            string[] chars = new string[] { "0", "2", "3", "4", "5", "6", "8", "9", "a", "b", "c", "d", "e", "f", "g", "h", "j", "k", "m", "n", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z" };
            var sb = new StringBuilder();
            for (int i = 0; i < length; i++) sb.Append(chars[random.Next(chars.Length)]);
            return sb.ToString();
        }

        public byte[] Send(string method, string uri, string postData, out string err) /*1.9.0*/
        {
            GC.Collect();
            err = "";
            byte[] bytes = new byte[0];
            try
            {
                string nonce = GetRandomString(32);// Guid.NewGuid().ToString().ToLower().Replace("-", "");
                string timestamp = DateTime.UtcNow.ToString("o");
                string passwordDigest = System.Convert.ToBase64String(Encoding.UTF8.GetBytes(CommonFunctions.Sha1(nonce + timestamp + secret)));
                string authHeader = String.Format("Username=\"{0}\", PasswordDigest=\"{1}\", Nonce=\"{2}\", Created=\"{3}\"", key, passwordDigest, nonce, timestamp);
                HttpWebRequest httpRequest = (HttpWebRequest)WebRequest.Create(this.baseURI + uri);
                httpRequest.Method = method;
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
                using (HttpWebResponse res = (HttpWebResponse)httpRequest.GetResponse())
                {
                    Stream ReceiveStream = res.GetResponseStream();
                    CommonFunctions.StreamToBytes(ref ReceiveStream, out bytes);
                    ReceiveStream = null;
                }
                return bytes;
            }
            catch (Exception e)
            {
                try
                {
                    using (WebResponse res = (e as System.Net.WebException).Response)
                    {
                        byte[] errbytes = null;
                        Stream ReceiveStream = res.GetResponseStream();
                        CommonFunctions.StreamToBytes(ref ReceiveStream, out errbytes);
                        ReceiveStream = null;
                        StringBuilder result = new StringBuilder(e.Message + '|');
                        if (errbytes.Length != 0) {result.Append(System.Text.Encoding.UTF8.GetString(errbytes));};
                        err = result.ToString();
                    }
                    return bytes;
                }
                catch (Exception exc)
                {
                    return Encoding.GetEncoding(1251).GetBytes(exc.Message);
                }
                //-----------------------------------------
            }
        }


        
      
    }
    // --> Emarsys END ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    class Levis 
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

         private Kupi GetWS(out String error)
        {
            try
            {
                /*method 1*///ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(TrustAllCertificateCallback);
                /*method 2*/  ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => { return true; };
                Kupi KupiInst = new Kupi();
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
            catch (Exception e){return e.Message + ".URL: " + KupiInst.Url;}
            }
        }
  
    }    // --> Levis END ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~




    delegate RequestResult GetRequestResult();

    class RequestData
    {
        public string url;// { get { return url; } set { url = value; } }
        public string method;// { get { return method; } set { method = value; } }
        
        public RequestData(string aUrl, string aMethod)
        {
            this.url = aUrl;
            this.method = aMethod;
        }


    }

    class RequestResult
    {
        public int resCode;
        public string resMessage;

        public RequestResult()
        { 
        
        }
    }

    public class AsyncRequest 
    {

       

        

        internal RequestResult RR() 
        {
           RequestResult result = new RequestResult();
           StringBuilder sb = new StringBuilder(String.Format("{0}\r\n", ""));
            for (int i=0; i<10000000; i++){ sb.AppendFormat("{0}, ", i); }
            result.resCode = new Random().Next(100500);
            //using (SqlConnection connection = new SqlConnection("context connection=true"))
            //{
            //    connection.Open();
            //    SqlCommand command = new SqlCommand("select @@SERVERNAME \"Server\", DB_NAME() \"DataBase\"", connection);
            //    SqlDataReader reader = command.ExecuteReader();
            //    using (reader)
            //    {
            //        while (reader.Read())
            //        {
            //            sb.AppendLine(reader.GetSqlString(0).Value);
            //            sb.AppendLine(reader.GetSqlString(1).Value);
            //        }
            //    }
            //}
            result.resMessage = sb.ToString();
            return result;
        } 




        async Task<RequestResult> TaskOfT_MethodAsync(string url, string method)
        {
            CommonFunctions.WriteLog(String.Format("TaskOfT_MethodAsync: {0}", "START"));
               return await Task<RequestResult>.Factory.StartNew((prm) =>
                    {
                        CommonFunctions.WriteLog(String.Format("TaskOfT_MethodAsync: {0}", "ENTER"));
                        StringBuilder sb = new StringBuilder();
                        RequestResult local = new RequestResult();
                        sb.AppendLine(String.Format("url: {0}", (prm as RequestData).url));
                        sb.AppendLine(String.Format("method: {0}", (prm as RequestData).method));
                        sb.Append("\r\n");
                        for (int i = 0; i < 100; i++) { sb.AppendFormat("{0}, ", i); }
                        local.resCode = new Random().Next(100500);
                        local.resMessage = sb.ToString();
                        CommonFunctions.WriteLog(String.Format("TaskOfT_MethodAsync: {0}", "EXIT"));
                        return local;
                    }, new RequestData(url, method));

            
            
            // --work
            //string result = await Task.FromResult<string>(DateTime.Now.DayOfWeek.ToString());
            //RequestResult res = new RequestResult();
            //res.resMessage = result;
            //return res;
        }


        internal async void AsyncCaller(string url, string method, string headers, string data, string contenttype, string dbProps)
        {
            // new DownloadDataCompletedEventHandler(AsyncHttpMessageHandler)
            try
            {
                CommonFunctions.WriteLog("AsyncCaller: ENTER");
                RequestResult res = await TaskOfT_MethodAsync(url,method);
                CommonFunctions.WriteLog(String.Format("AsyncCaller: {0}, {1}", res.resCode, res.resMessage));
            }
            catch (Exception E) { CommonFunctions.WriteLog(String.Format(" AsyncExecuteRequest: {0}, {1}", E.HResult, E.Message)); }
        }
      
    } // --> AsyncRequest END ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    public class HTTPFunctions
    {

        public static void FillHeaders(String aHeaders, WebHeaderCollection Dest)
        {
            if (aHeaders != "")
            {
                string[] dophdr = aHeaders.Split(';');
                for (int i = 0; i < dophdr.Length; i++)
                {
                    try
                    {
                        Dest.Add(dophdr[i].Trim());
                    }
                    catch { }
                }
            }
        }

        public static void FillHeaders(String aHeaders, System.Net.Http.Headers.HttpRequestHeaders Dest)
        {
            if (aHeaders != "")
            {
                string[] dophdr = aHeaders.Split(';');
                string[] item;
                for (int i = 0; i < dophdr.Length; i++)
                {
                    try
                    {
                        item=dophdr[i].Trim().Split('=');
                        Dest.Add(item[0].Trim(),item[1].Trim());
                    }
                    catch { }
                }
            }
        }


        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static void AsyncReadBlobMethod(SqlString url, SqlString method, SqlString headers, SqlString data, SqlString contenttype, SqlString dbProps) /*1.9.0*/
        {
            GC.Collect();
            try 
            {
                CommonFunctions.WriteLog(String.Format("{0}:{1}", "AsyncReadBlobMethod", 0));
             AsyncRequest AR = new AsyncRequest();
                CommonFunctions.WriteLog(String.Format("{0}:{1}", "AsyncReadBlobMethod", 1));
             AR.AsyncCaller(url.Value, method.Value, headers.Value, data.Value, contenttype.Value, dbProps.Value);
                
            
            }
            catch (Exception E) { CommonFunctions.WriteLog(String.Format(" AsyncReadBlobMethod: {0}, {1}", E.HResult, E.Message)); }
        }

        /// <summary>
        /// Проблеммы возникают (NULL), когда идет первый запрос, потом , видимо, берется из кэша. 
        /// В этом случае требуется работа в паре с ReadBlob.
        /// Теоретически удобно для получения больших данных, которые не меняются во времени на одной и той же ссылке.
        /// Но это мало вероятно.
        /// Хорошо использовать в оконных приложениях (там и тестировалось).
        /// </summary>
        static byte[] ReadBlobAsyncResult = null;
        private static void DownloadDataCallback(Object sender, DownloadDataCompletedEventArgs e)
        {
            ReadBlobAsyncResult = e.Result;
        }
        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlBytes ReadBlobAsync(SqlString url) /*1.9.0*/
        {
            GC.Collect();
            Uri uri = new Uri(url.Value);
            using (WebClient client = new WebClient())
            {
                client.DownloadDataCompleted += new DownloadDataCompletedEventHandler(DownloadDataCallback);
                client.DownloadDataAsync(uri);
                uri = null;
                return new SqlBytes(ReadBlobAsyncResult);
            }
        }


        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlBytes ReadBlob(SqlString url) /*1.9.0*///, SqlBoolean NeedLog = SqlBoolean.False
        {
            GC.Collect();
            SqlBytes blob = null;
            try
            {
                HttpWebRequest http = (HttpWebRequest)WebRequest.Create(url.Value);
                using (WebResponse response = http.GetResponse())
                {
                    Stream stream = response.GetResponseStream();
                    byte[] bytes = null;
                    CommonFunctions.StreamToBytes(ref stream, out bytes);
                    stream = null;
                    blob = new SqlBytes(bytes);
                    bytes = null;
                    http = null;
                }
            }
            catch {return null;}
            //catch (Exception E) {blob =  new SqlBytes(Encoding.GetEncoding(1251).GetBytes(E.Message)); }
            return blob;
        }


        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlBytes HttpGet(SqlString url, SqlString headers) /*1.9.0*/
        {
            GC.Collect();
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
                FillHeaders(headers.Value, req.Headers);
                using (WebResponse res = req.GetResponse())
                {
                    byte[] bytes = null;
                    Stream ReceiveStream = res.GetResponseStream();
                    CommonFunctions.StreamToBytes(ref ReceiveStream, out bytes);
                    ReceiveStream = null;
                    blob = new SqlBytes(bytes);
                    bytes = null;
                }
                req = null;
            }
            catch(Exception E)
            {
                Errors = E.Message;
                blob = null;
            }
            return blob;
        }


        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlBytes ReadBlobMethod(SqlString url, SqlString Method, SqlString headers, SqlString parameters, SqlString contenttype) /*1.9.0*/
        {
            GC.Collect();
            SqlBytes blob = null;
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
                Stream ReceiveStream = res.GetResponseStream();
                CommonFunctions.StreamToBytes(ref ReceiveStream, out bytes);
                ReceiveStream = null;
                if (bytes.Length == 0)
                    blob = new SqlBytes(new byte[] { 0x00});
                 else 
                    blob = new SqlBytes(bytes);
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
                    Stream ReceiveStream = res.GetResponseStream();
                    CommonFunctions.StreamToBytes(ref ReceiveStream, out bytes);
                    ReceiveStream = null;
                    res = null;
                    StringBuilder result = new StringBuilder(e.Message + '|');
                    if (bytes.Length != 0) { result.Append(System.Text.Encoding.UTF8.GetString(bytes)); };
                    bytes = null;
                    blob = new SqlBytes(Encoding.GetEncoding(1251).GetBytes(result.ToString()));
                }
                catch (Exception exc) 
                {
                    blob = new SqlBytes(Encoding.GetEncoding(1251).GetBytes(exc.Message));
                }
            }
            return blob;
        }



        private class ReadBlobMethodExsResult
        {
            public SqlBytes Result;
            public SqlString Debug;

            public ReadBlobMethodExsResult(SqlBytes result, SqlString debug)
            {
                Result = result;
                Debug = debug;
            }
        }
        public static void ReadBlobMethodEx_FillRow(object resultCollection, out SqlBytes result, out SqlString debug)
        {
            ReadBlobMethodExsResult OneRecord = (ReadBlobMethodExsResult)resultCollection;
            result = OneRecord.Result;
            debug = OneRecord.Debug;
        }

        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read
                                              , FillRowMethodName = "ReadBlobMethodEx_FillRow"
                                              , TableDefinition = "result nvarchar(max), debug nvarchar(max)")]
        public static IEnumerable ReadBlobMethodEx(SqlString url, SqlString Method, SqlString headers, SqlString parameters, SqlString contenttype) /*1.9.0*/
        {
            GC.Collect();
            SqlBytes blob = null;
            ArrayList resultCollection = new ArrayList();
            StringBuilder debugSB = new StringBuilder("<times>\r\n");
            debugSB.AppendFormat("<item step=\"{0}\" time=\"{1}\" />", "Start", DateTime.Now.ToString("HH:mm:ss.ffffff"));
            if (url.IsNull == true) { resultCollection.Add(new ReadBlobMethodExsResult(blob, "")); return resultCollection; }//{ return blob; }
            try
            {
                string HttpData = "";
                if (parameters.IsNull == false) { HttpData = parameters.Value; }
                HttpWebRequest req = (HttpWebRequest)HttpWebRequest.Create(url.Value);
                debugSB.AppendFormat("<item step=\"{0}\" time=\"{1}\" />", "Create HttpWebRequest", DateTime.Now.ToString("HH:mm:ss.ffffff"));
                ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, sslPolicyErrors) => { return true; };
                debugSB.AppendFormat("<item step=\"{0}\" time=\"{1}\" />", "Check Sertificate", DateTime.Now.ToString("HH:mm:ss.ffffff"));
                req.Method = Method.Value;
                req.Timeout = 100000;
                if (contenttype.IsNull == true)
                { req.ContentType = ""; }
                else
                { req.ContentType = contenttype.Value; }
                debugSB.AppendFormat("<item step=\"{0}\" time=\"{1}\" />", "Fill HttpWebRequest", DateTime.Now.ToString("HH:mm:ss.ffffff"));
                FillHeaders(headers.Value, req.Headers);
                debugSB.AppendFormat("<item step=\"{0}\" time=\"{1}\" />", "Fill headers", DateTime.Now.ToString("HH:mm:ss.ffffff"));
                if (req.Method.ToUpper() != "GET")
                {
                    byte[] sentData = Encoding.GetEncoding(1251).GetBytes(HttpData);
                    req.ContentLength = sentData.Length;
                    Stream sendStream = req.GetRequestStream();
                    sendStream.Write(sentData, 0, sentData.Length);
                    sentData = null;
                }
                debugSB.AppendFormat("<item step=\"{0}\" time=\"{1}\" />", "Fill method", DateTime.Now.ToString("HH:mm:ss.ffffff"));
                WebResponse res = req.GetResponse();
                debugSB.AppendFormat("<item step=\"{0}\" time=\"{1}\" />", "GetResponse", DateTime.Now.ToString("HH:mm:ss.ffffff"));
                byte[] bytes = null;

                Stream ReceiveStream = res.GetResponseStream();
                CommonFunctions.StreamToBytes(ref ReceiveStream, out bytes);
                ReceiveStream = null;
                if (bytes.Length == 0)
                    blob = new SqlBytes(new byte[] { 0x00 });
                else
                    blob = new SqlBytes(bytes);
                debugSB.AppendFormat("<item step=\"{0}\" time=\"{1}\" />", "GetResponseStream", DateTime.Now.ToString("HH:mm:ss.ffffff"));
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
                    Stream ReceiveStream = res.GetResponseStream();
                    CommonFunctions.StreamToBytes(ref ReceiveStream, out bytes);
                    ReceiveStream = null;
                    blob = new SqlBytes(bytes);
                    StringBuilder result = new StringBuilder(e.Message + '|');
                    if (bytes.Length != 0) { result.Append(System.Text.Encoding.UTF8.GetString(bytes)); };
                    bytes = null;
                    blob = new SqlBytes(Encoding.GetEncoding(1251).GetBytes(result.ToString()));
                    debugSB.AppendFormat("<item step=\"{0}\" time=\"{1}\" />", "err_0", DateTime.Now.ToString("HH:mm:ss.ffffff"));
                }
                catch (Exception exc)
                {
                    blob = new SqlBytes(Encoding.GetEncoding(1251).GetBytes(exc.Message));
                    debugSB.AppendFormat("<item step=\"{0}\" time=\"{1}\" />", "err_1", DateTime.Now.ToString("HH:mm:ss.ffffff"));
                }
            }
            debugSB.Append("</times>");
            resultCollection.Add(new ReadBlobMethodExsResult(blob, debugSB.ToString()));
            return resultCollection;
        }

        // -- Emarsys classes and procedures BEGIN ----------------------------------------------------------
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
            GC.Collect();
            string err = "";
            string res = "";
            Emarsys ES = null;
            ArrayList resultCollection = new ArrayList();
            byte[] bytes;
            try
            {
                ES = new Emarsys("https://suite10.emarsys.net/api/v2/", "kupivip_tr002", "EPDE6c2dFfCRkMDwMspV");
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
            GC.Collect();
            string err = "";
            string res = "";
            Emarsys ES = null;
            ArrayList resultCollection = new ArrayList();
            byte[] bytes;
            try
            {
                ES = new Emarsys("https://suite10.emarsys.net/api/v2/", User.Value, Password.Value);
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
            }
        }
        public static void Request_FillRow(object resultCollection, out SqlString result, out SqlString error)
        {
            EmarsysResult emarsysresult = (EmarsysResult)resultCollection;
            result = emarsysresult.Result;
            error = emarsysresult.Error;
        }


        // -- Emarsys classes and procedures END ----------------------------------------------------------

        // -- Levis classes and procedures BEGIN ----------------------------------------------------------
        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlString Levis_PingPong()
        {
            GC.Collect();
            try
            {
                return new Levis().DoPingPong();
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlString Levis_GetPackage(SqlString package)
        {
            GC.Collect();
            try
            {
                return new Levis().DoGetPackage(package.Value);
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }

        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlString Levis_PutPackage(SqlString package)
        {
            GC.Collect();
            try
            {
                return new Levis().DoPutPackage(package.Value);
            }
            catch (Exception e)
            {
                return e.Message;
            }
        }
        // -- Levis classes and procedures END ----------------------------------------------------------  





    } // --> HTTPFunctions  END ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

    public class StringFunctions 
    {
        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlString AsWin1251(SqlString source) /*1.9.0*/
        {
            GC.Collect();
            if (source.IsNull)
                return ""; 
            else
                return Encoding.UTF8.GetString(Encoding.GetEncoding(1251).GetBytes(source.Value));
        }

        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlString AsUTF8(SqlString source) /*1.9.0*/
        {
            GC.Collect();
            if (source.IsNull) 
                return ""; 
            else
                return Encoding.GetEncoding(1251).GetString(Encoding.UTF8.GetBytes(source.Value));
        }

        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlString JSONtoXML(SqlString value) /*1.9.0*/ // -- realise from 20160906, prev see in "Class 1.8.1(...).cs"
        {
            GC.Collect();
            if (value.IsNull == true) {return "";}
            try
            {
              return Newtonsoft.Json.JsonConvert.DeserializeXmlNode("{\"ITEM\":" + value.Value + "}", "LIST").InnerXml;
            }
            catch {return ""; }
        }

        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlString XMLtoJSON(SqlString value)
        {
            GC.Collect();
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
            GC.Collect();
            var res = "";
            if (source.IsNull == true) { return res; }
            var srcStr = source.Value;
            try
            {
            //return HttpUtility.UrlEncode(srcStr); // in assembly System.Web
            res=Uri.EscapeDataString(srcStr);
            }
            catch{}
            return res;
        }

        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlString URLDecode(SqlString source)// декодирование : %D0%BF%D1%80%D0%B8%D0%B2%D0%B5%D1%82 -> привет     
        {
            GC.Collect();
            var res = "";
            if (source.IsNull == true) { return res; }
            var srcStr = source.Value;
            //return HttpUtility.UrlDecode(srcStr); // in assembly System.Web
            try { res = Uri.UnescapeDataString(srcStr); }
            catch {}
            return res;
        }
 
        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlString SaveToFile(SqlString Data, SqlString Filename)
        {
            GC.Collect();
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
            GC.Collect();
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
            GC.Collect();
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
                          int buffsize = 262144;//256k 1048576;//1024k(1M) // роли не играет....
                          var buffer = new Byte[buffsize];
                          int h;
                          while ((h = gz.Read(buffer, 0, buffer.Length)) > 0) { ms.Write(buffer, 0, h); }
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
                blob = new SqlBytes(Encoding.GetEncoding(1251).GetBytes(e.Message));
            }
            return blob;
        }

       
        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static SqlBytes HMACSHA1Encode(SqlString basestring, SqlString keystring)
        {
            GC.Collect();
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
                hashValue = null;
                stream = null;
                myhmacsha1 = null;
            }
            catch (Exception e)
            {
                blob = new SqlBytes(Encoding.GetEncoding(1251).GetBytes(e.Message));
            }
            key = null;
            input = null;
            return blob;
            //using (HMACSHA1 myhmacsha1 = new HMACSHA1(key);
            //byte[] byteArray = Encoding.ASCII.GetBytes(input);
            //MemoryStream stream = new MemoryStream(byteArray);
            //return myhmacsha1.ComputeHash(stream).Aggregate("", (s, e) => s + String.Format("{0:x2}",e), s => s );
        }





    }// --> StringFunctions END ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

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

    // ---- НЕ УДАЛЯТЬ!!! ---------------------------------------------------------------------------------------------------
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
    //        GC.Collect();
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



    public class TestFunctions
    {
        private class OneRecord
        {
            public SqlDateTime DateCreate;
            public SqlBytes Body;
            public SqlString Message;
            public OneRecord(SqlDateTime aDateCreate, SqlBytes aBody, SqlString aMessage) 
            {
                DateCreate = aDateCreate;
                Body = aBody;
                Message = aMessage;
            }
        }

        public static void FillOneRecord(object aOneRecordObject, out SqlDateTime aDateTime, out SqlBytes aBody, out SqlString aMessage)
        {
            OneRecord OneRecordInstance = (OneRecord)aOneRecordObject;
            aDateTime = OneRecordInstance.DateCreate;
            aBody = OneRecordInstance.Body;
            aMessage = OneRecordInstance.Message;
        }

        // TestFunctions.TableReturnFunction "DateCreate datetime, Body varbinary(max), Message nvarchar(max)"
        [Microsoft.SqlServer.Server.SqlFunction(
                                                DataAccess = DataAccessKind.Read
                                              , FillRowMethodName = "FillOneRecord"
                                              , TableDefinition = "DateCreate datetime, Body varbinary(max), Message nvarchar(max)"
                                              )]
        public static IEnumerable TableReturnFunction(SqlString parameter)
        {
            GC.Collect();
            ArrayList records = new ArrayList();
            DateTime _DateCreate = DateTime.Now;
            byte[] _Body = null;
            string _Message = "";
            if (parameter.IsNull == true) { records.Add(new OneRecord(new SqlDateTime(_DateCreate), new SqlBytes(_Body), _Message)); return records; }//{ return blob; }
            try
            {
                for (int cnt = 0; cnt <= 100; cnt++)
                {
                    _Message = DateTime.Now.ToString("o");
                    _Body = Encoding.GetEncoding(1251).GetBytes(_Message);
                    records.Add(new OneRecord(new SqlDateTime(DateTime.Now.AddDays(cnt)), new SqlBytes(_Body), _Message));
                    _Body= null;
                }
            }
            catch (Exception E)
            {
                _DateCreate = DateTime.Now;
                StringBuilder ErrStrBuilder = new StringBuilder(String.Format("Error Data ({0})", E.Data.Count)); 
                foreach (DictionaryEntry de in E.Data) {ErrStrBuilder.AppendFormat("{0}\r\n", de.ToString());}
                _Body = Encoding.GetEncoding(1251).GetBytes(ErrStrBuilder.ToString());
                _Message = E.Message;
                records.Add(new OneRecord(new SqlDateTime(_DateCreate), new SqlBytes(_Body), _Message));
            }
          return records;
        } // -- end of TableReturnFunction


   
    }// -- end of TestFunctions



    public class TechFunctions 
    {
       
        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static void GetUsedDB()
        {
            StringBuilder res = new StringBuilder();
            using (SqlConnection connection = new SqlConnection("context connection=true"))
            {
                connection.Open();
                SqlCommand command = new SqlCommand("select @@SERVERNAME \"Server\", DB_NAME() \"DataBase\"", connection);
                SqlDataReader reader = command.ExecuteReader();
                using (reader)
                {
                    while (reader.Read())
                    {
                        res.AppendLine(reader.GetSqlString(0).Value);
                        res.AppendLine(reader.GetSqlString(1).Value);
                    }
                }
            }
            CommonFunctions.SendSQLDebugMessage(res.ToString());
            SqlContext.Pipe.ExecuteAndSend(new SqlCommand("select @@SERVERNAME \"Server\", DB_NAME() \"DataBase\""));
        }

        [Microsoft.SqlServer.Server.SqlFunction(DataAccess = DataAccessKind.Read)]
        public static void TryClearMemory(out SqlInt64 before, out SqlInt64 after)
        {
            //CommonFunctions.SendSQLDebugMessage(String.Format("Start at {0}",DateTime.Now.ToString("HH:mm:ss.ffffff")));
            before = GC.GetTotalMemory(false);
            GC.Collect();
            after = GC.GetTotalMemory(true);
            //CommonFunctions.SendSQLDebugMessage(CommonFunctions.GetLogFileName());
        }

        


    } // end of TechFunctions


} // -- end of namespace -----------------------------------------------------


// ----- Полезные ссылки -------------------------------------------------------------------------------------------
// --- about collations --------------------------------------------------------------------------------------------
    // https://technet.microsoft.com/en-us/library/ms131091%28v=sql.110%29.aspx
// --- Emarsys documentation ---------------------------------------------------------------------------------------
    // https://documentation.emarsys.com/resource/developers/api/getting-started/authentication/c-sharp-sample/
// -- SOAP and Auth header -----------------------------------------------------------------------------------------
    // http://stackoverflow.com/questions/4945420/how-can-i-add-a-basic-auth-header-to-my-soap 
// --- HTTP, SSL, Sertificate --------------------------------------------------------------------------------------
    // https://yandex.ru/search/?text=ServicePointManager.ServerCertificateValidationCallback%20%2B%3D%20%28sender%2C%20certificate%2C%20chain%2C%20sslPolicyErrors%29%20%3D%3E%20true%3B&lr=213&clid=1923018
    // http://stackoverflow.com/questions/12506575/how-to-ignore-the-certificate-check-when-ssl
    // http://www.khalidabuhakmeh.com/validate-ssl-certificate-with-servicepointmanager
    // https://msdn.microsoft.com/en-us/library/system.net.servicepointmanager.servercertificatevalidationcallback%28v=vs.110%29.aspx
// --- CLR with table return ---------------------------------------------------------------------------------------  
   // https://msdn.microsoft.com/en-us/library/ms131103.aspx
// --- CLR, Memory, GarbageCollection ------------------------------------------------------------------------------
    //https://msdn.microsoft.com/ru-ru/library/xe0c2357(v=vs.110).aspx
    //http://professorweb.ru/my/csharp/charp_theory/level13/13_4.php
    //GC.GetTotalMemory() Булевский параметр указывает, должен ли вызов сначала дождаться выполнения сборки мусора, прежде чем возвращать результат
// --- WebClient(slowly, simple) и HttpWebRequest(fastest, class does not block ) 
    //http://www.diogonunes.com/blog/webclient-vs-httpclient-vs-httpwebrequest/ (HttpWebRequest, WebClient, HttpClient)
    //http://www.intuit.ru/studies/courses/11184/1120/lecture/17470