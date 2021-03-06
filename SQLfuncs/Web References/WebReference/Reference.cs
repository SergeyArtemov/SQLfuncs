//------------------------------------------------------------------------------
// <auto-generated>
//     Этот код создан программой.
//     Исполняемая версия:4.0.30319.42000
//
//     Изменения в этом файле могут привести к неправильной работе и будут потеряны в случае
//     повторной генерации кода.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// Этот исходный текст был создан автоматически: Microsoft.VSDesigner, версия: 4.0.30319.42000.
// 
#pragma warning disable 1591

namespace SOAP_Levis.WebReference {
    using System;
    using System.Web.Services;
    using System.Diagnostics;
    using System.Web.Services.Protocols;
    using System.Xml.Serialization;
    using System.ComponentModel;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Web.Services.WebServiceBindingAttribute(Name="KupiSoapBinding", Namespace="https://89.17.48.79:61454")]
    public partial class Kupi : System.Web.Services.Protocols.SoapHttpClientProtocol {
        
        private System.Threading.SendOrPostCallback PingPongOperationCompleted;
        
        private System.Threading.SendOrPostCallback GetPackageOperationCompleted;
        
        private System.Threading.SendOrPostCallback PutPackageOperationCompleted;
        
        private bool useDefaultCredentialsSetExplicitly;
        
        /// <remarks/>
        public Kupi() {
            this.Url = global::SOAP_Levis.Properties.Settings.Default.SOAP_Levis_WebReference_Kupi;
            if ((this.IsLocalFileSystemWebService(this.Url) == true)) {
                this.UseDefaultCredentials = true;
                this.useDefaultCredentialsSetExplicitly = false;
            }
            else {
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        public new string Url {
            get {
                return base.Url;
            }
            set {
                if ((((this.IsLocalFileSystemWebService(base.Url) == true) 
                            && (this.useDefaultCredentialsSetExplicitly == false)) 
                            && (this.IsLocalFileSystemWebService(value) == false))) {
                    base.UseDefaultCredentials = false;
                }
                base.Url = value;
            }
        }
        
        public new bool UseDefaultCredentials {
            get {
                return base.UseDefaultCredentials;
            }
            set {
                base.UseDefaultCredentials = value;
                this.useDefaultCredentialsSetExplicitly = true;
            }
        }
        
        /// <remarks/>
        public event PingPongCompletedEventHandler PingPongCompleted;
        
        /// <remarks/>
        public event GetPackageCompletedEventHandler GetPackageCompleted;
        
        /// <remarks/>
        public event PutPackageCompletedEventHandler PutPackageCompleted;
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("https://89.17.48.79:61454#Kupi:PingPong", RequestNamespace="https://89.17.48.79:61454", ResponseNamespace="https://89.17.48.79:61454", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("return")]
        public string PingPong() {
            object[] results = this.Invoke("PingPong", new object[0]);
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void PingPongAsync() {
            this.PingPongAsync(null);
        }
        
        /// <remarks/>
        public void PingPongAsync(object userState) {
            if ((this.PingPongOperationCompleted == null)) {
                this.PingPongOperationCompleted = new System.Threading.SendOrPostCallback(this.OnPingPongOperationCompleted);
            }
            this.InvokeAsync("PingPong", new object[0], this.PingPongOperationCompleted, userState);
        }
        
        private void OnPingPongOperationCompleted(object arg) {
            if ((this.PingPongCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.PingPongCompleted(this, new PingPongCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("https://89.17.48.79:61454#Kupi:GetPackage", RequestNamespace="https://89.17.48.79:61454", ResponseNamespace="https://89.17.48.79:61454", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("return")]
        public string GetPackage(out string Пакет) {
            object[] results = this.Invoke("GetPackage", new object[0]);
            Пакет = ((string)(results[1]));
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void GetPackageAsync() {
            this.GetPackageAsync(null);
        }
        
        /// <remarks/>
        public void GetPackageAsync(object userState) {
            if ((this.GetPackageOperationCompleted == null)) {
                this.GetPackageOperationCompleted = new System.Threading.SendOrPostCallback(this.OnGetPackageOperationCompleted);
            }
            this.InvokeAsync("GetPackage", new object[0], this.GetPackageOperationCompleted, userState);
        }
        
        private void OnGetPackageOperationCompleted(object arg) {
            if ((this.GetPackageCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.GetPackageCompleted(this, new GetPackageCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        [System.Web.Services.Protocols.SoapDocumentMethodAttribute("https://89.17.48.79:61454#Kupi:PutPackage", RequestNamespace="https://89.17.48.79:61454", ResponseNamespace="https://89.17.48.79:61454", Use=System.Web.Services.Description.SoapBindingUse.Literal, ParameterStyle=System.Web.Services.Protocols.SoapParameterStyle.Wrapped)]
        [return: System.Xml.Serialization.XmlElementAttribute("return")]
        public string PutPackage(string Пакет) {
            object[] results = this.Invoke("PutPackage", new object[] {
                        Пакет});
            return ((string)(results[0]));
        }
        
        /// <remarks/>
        public void PutPackageAsync(string Пакет) {
            this.PutPackageAsync(Пакет, null);
        }
        
        /// <remarks/>
        public void PutPackageAsync(string Пакет, object userState) {
            if ((this.PutPackageOperationCompleted == null)) {
                this.PutPackageOperationCompleted = new System.Threading.SendOrPostCallback(this.OnPutPackageOperationCompleted);
            }
            this.InvokeAsync("PutPackage", new object[] {
                        Пакет}, this.PutPackageOperationCompleted, userState);
        }
        
        private void OnPutPackageOperationCompleted(object arg) {
            if ((this.PutPackageCompleted != null)) {
                System.Web.Services.Protocols.InvokeCompletedEventArgs invokeArgs = ((System.Web.Services.Protocols.InvokeCompletedEventArgs)(arg));
                this.PutPackageCompleted(this, new PutPackageCompletedEventArgs(invokeArgs.Results, invokeArgs.Error, invokeArgs.Cancelled, invokeArgs.UserState));
            }
        }
        
        /// <remarks/>
        public new void CancelAsync(object userState) {
            base.CancelAsync(userState);
        }
        
        private bool IsLocalFileSystemWebService(string url) {
            if (((url == null) 
                        || (url == string.Empty))) {
                return false;
            }
            System.Uri wsUri = new System.Uri(url);
            if (((wsUri.Port >= 1024) 
                        && (string.Compare(wsUri.Host, "localHost", System.StringComparison.OrdinalIgnoreCase) == 0))) {
                return true;
            }
            return false;
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")]
    public delegate void PingPongCompletedEventHandler(object sender, PingPongCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class PingPongCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal PingPongCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")]
    public delegate void GetPackageCompletedEventHandler(object sender, GetPackageCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class GetPackageCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal GetPackageCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
        
        /// <remarks/>
        public string Пакет {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[1]));
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")]
    public delegate void PutPackageCompletedEventHandler(object sender, PutPackageCompletedEventArgs e);
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("System.Web.Services", "4.6.1055.0")]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    public partial class PutPackageCompletedEventArgs : System.ComponentModel.AsyncCompletedEventArgs {
        
        private object[] results;
        
        internal PutPackageCompletedEventArgs(object[] results, System.Exception exception, bool cancelled, object userState) : 
                base(exception, cancelled, userState) {
            this.results = results;
        }
        
        /// <remarks/>
        public string Result {
            get {
                this.RaiseExceptionIfNecessary();
                return ((string)(this.results[0]));
            }
        }
    }
}

#pragma warning restore 1591