using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualBasic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using XISystem;
using XIDatabase;
using System.Net;
using System.Configuration;
using System.Web;

namespace XICore
{
    public class XIDefinitionBase
    {
        private iSiganlR oSignalR = null;
        public XIDefinitionBase()
        {

        }
        public XIDefinitionBase(iSiganlR oSignalRI)
        {
            oSignalR = oSignalRI;
        }
        public int StatusTypeID { get; set; }
        public int CreatedBy { get; set; }
        public string CreatedBySYSID { get; set; }
        public DateTime CreatedTime
        {
            get
            { return DateTime.Now; }
            set { }
        }
        public int UpdatedBy { get; set; }
        public string UpdatedBySYSID { get; set; }
        public DateTime UpdatedTime
        {
            get
            { return DateTime.Now; }
            set { }
        }
        XIDBAPI Connection = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString);
        private object oMyParent; // CIBO
        public object oParent
        {
            get
            {
                return oMyParent;
            }
            set
            {
                oMyParent = value;
            }
        }// CIBO

        // ALERT - we don't need this as you can get through BOI, but a quick reference here for short
        private XIDBO oMyBOD;
        public XIDBO BOD
        {
            get
            {
                return oMyBOD;
            }
            set
            {
                oMyBOD = value;
            }
        }


        private XIIBO oMyBOI;
        public XIIBO BOI
        {
            get
            {
                return oMyBOI;
            }
            set
            {
                oMyBOI = value;
            }
        }

        private object oMyDefinition; // CIBO
        public object oDefintion
        {
            get
            {
                return oMyDefinition;
            }
            set
            {
                oMyDefinition = value;
            }
        }

        private Dictionary<string, object> oMyContent = new Dictionary<string, object>();

        public Dictionary<string, object> oContent
        {
            get
            {
                return oMyContent;
            }
            set
            {
                oMyContent = value;
            }
        }


        public object Clone(object obj)
        {
            try
            {
                if (obj == null)
                    return null;
                Type type = obj.GetType();

                if (type.IsValueType || type == typeof(string))
                {
                    return obj;
                }
                else if (type.IsArray)
                {
                    Type elementType = Type.GetType(
                         type.FullName.Replace("[]", string.Empty));
                    if (elementType != null)
                    {
                        var array = obj as Array;
                        Array copied = Array.CreateInstance(elementType, array.Length);
                        for (int i = 0; i < array.Length; i++)
                        {
                            copied.SetValue(Clone(array.GetValue(i)), i);
                        }
                        return Convert.ChangeType(copied, obj.GetType());
                    }
                    else
                    {
                        return null;
                    }
                }
                else if (type.IsClass)
                {

                    object toret = Activator.CreateInstance(obj.GetType());
                    FieldInfo[] fields = type.GetFields(BindingFlags.Public |
                                BindingFlags.NonPublic | BindingFlags.Instance);
                    foreach (FieldInfo field in fields)
                    {
                        object fieldValue = field.GetValue(obj);
                        if (fieldValue == null)
                            continue;
                        field.SetValue(toret, Clone(fieldValue));
                    }
                    return toret;
                }
                else
                    throw new ArgumentException("Unknown type");
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public T DeepCopy<T>(T source)
        {
            if (!typeof(T).IsSerializable)
            {
                throw new ArgumentException("The type must be serializable.", "source");
            }

            // Don't serialize a null object, simply return the default for that object
            if (Object.ReferenceEquals(source, null))
            {
                return default(T);
            }

            IFormatter formatter = new BinaryFormatter();
            Stream stream = new MemoryStream();
            using (stream)
            {
                formatter.Serialize(stream, source);
                stream.Seek(0, SeekOrigin.Begin);
                return (T)formatter.Deserialize(stream);
            }
        }

        public void SaveErrortoDB(CResult oCResult, int iQSInstanceID = 0, int iPolicyID = 0)
        {
            try
            {
                var iApplicationID = 0;
                int iUserID = 0;
                string sUser = string.Empty;
                if (HttpContext.Current != null)
                {
                    var sApplicationID = HttpContext.Current.Session["ApplicationID"].ToString();
                    int.TryParse(sApplicationID, out iApplicationID);
                    int.TryParse(HttpContext.Current.Session["UserID"].ToString(), out iUserID);
                    if (iUserID > 0)
                    {
                        sUser = HttpContext.Current.Session["sUserName"].ToString();
                    }
                    if (string.IsNullOrEmpty(sUser))
                    {
                        sUser = "Public without Login";
                    }
                }
                XIInfraCache oCache = new XIInfraCache();
                var BOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "ErrorLog");
                XIIBO oBOI = new XIIBO();
                oBOI.BOD = BOD;
                oBOI.SetAttribute("FKiApplicationID", iApplicationID.ToString());
                oBOI.SetAttribute("sCategory", oCResult.sCategory);
                oBOI.SetAttribute("iType", oCResult.iType.ToString());
                oBOI.SetAttribute("iCriticality", oCResult.iCriticality.ToString());
                oBOI.SetAttribute("CreatedTime", DateTime.Now.ToString());
                oBOI.SetAttribute("Description", oCResult.sMessage);
                oBOI.SetAttribute("CreatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                oBOI.SetAttribute("TypeID", "20");
                if (oCResult.sMessage.ToLower().StartsWith("error"))
                {
                    oBOI.SetAttribute("TypeID", "10");
                }
                oBOI.SetAttribute("sCode", oCResult.sCode);
                oBOI.SetAttribute("FKiQSInstanceID", iQSInstanceID.ToString());
                oBOI.SetAttribute("FKiPolicyID", iPolicyID.ToString());
                oBOI.SetAttribute("CreatedByID", iUserID.ToString());
                oBOI.SetAttribute("CreatedByName", sUser);
                if (!string.IsNullOrEmpty(oCResult.sMessage))
                    oBOI.Save(oBOI);
                //if (iApplicationID == 76)
                //{
                //    var oCR = XIMonitor(oCResult, iApplicationID);
                //}
                    

                if (oCResult.oTraceStack != null && oCResult.oTraceStack.Count() > 0)
                {
                    string sMessage = string.Empty;
                    if (iQSInstanceID > 0)
                    {
                        sMessage = "[QSInstanceID : " + iQSInstanceID + " ] - ";
                    }
                    if (iPolicyID > 0)
                    {
                        sMessage += "[PolicyID : " + iPolicyID + " ] - ";
                    }
                    var Messages = oCResult.oTraceStack.Select(m => m.sValue).ToArray();
                    var Msg = "TraceStack: " + sMessage + string.Join("->", Messages);
                    XIIBO oTrace = new XIIBO();
                    oTrace.BOD = BOD;
                    oTrace.SetAttribute("FKiApplicationID", iApplicationID.ToString());
                    oTrace.SetAttribute("sCategory", oCResult.sCategory);
                    oTrace.SetAttribute("CreatedTime", DateTime.Now.ToString());
                    oTrace.SetAttribute("Description", Msg);
                    oTrace.SetAttribute("FKiQSInstanceID", iQSInstanceID.ToString());
                    oTrace.SetAttribute("FKiPolicyID", iPolicyID.ToString());
                    oTrace.SetAttribute("CreatedBySYSID", Utility.GetIPAddress());
                    oTrace.SetAttribute("CreatedByID", iUserID.ToString());
                    oTrace.SetAttribute("CreatedByName", sUser);
                    oTrace.SetAttribute("TypeID", "20");
                    if (oCResult.sMessage.ToLower().StartsWith("error"))
                    {
                        oTrace.SetAttribute("TypeID", "10");
                    }
                    if (!string.IsNullOrEmpty(Msg))
                        oTrace.Save(oTrace);
                }

                //XIErrorLogs ELog = new XIErrorLogs();
                //ELog.CreatedTime = DateTime.Now;
                //ELog.Description = oCResult.sMessage;
                //ELog.CreatedBySYSID = Dns.GetHostAddresses(Dns.GetHostName())[1].ToString();
                //ELog.TypeID = 20;
                //ELog.FKiQSInstanceID = iQSInstanceID;
                //ELog.FKiPolicyID = iPolicyID;
                //if (oCResult.sMessage.ToLower().StartsWith("error"))
                //{
                //    ELog.TypeID = 10;
                //}
                //ELog.sCode = oCResult.sCode;
                //if (!string.IsNullOrEmpty(ELog.Description))
                //{
                //    Connection.Insert<XIErrorLogs>(ELog, "XIErrorLog_T", "ID");
                //}
                //if (oCResult.oTraceStack != null && oCResult.oTraceStack.Count() > 0)
                //{
                //    string sMessage = string.Empty;
                //    if (iQSInstanceID > 0)
                //    {
                //        sMessage = "[QSInstanceID : " + iQSInstanceID + " ] - ";
                //    }
                //    if (iPolicyID > 0)
                //    {
                //        sMessage += "[PolicyID : " + iPolicyID + " ] - ";
                //    }
                //    var Messages = oCResult.oTraceStack.Select(m => m.sValue).ToArray();
                //    var Msg = "TraceStack: " + sMessage + string.Join("->", Messages);
                //    XIErrorLogs Trace = new XIErrorLogs();
                //    Trace.CreatedTime = DateTime.Now;
                //    Trace.Description = Msg;
                //    Trace.FKiQSInstanceID = iQSInstanceID;
                //    Trace.FKiPolicyID = iPolicyID;
                //    Trace.CreatedBySYSID = Utility.GetIPAddress();
                //    if (HttpContext.Current != null)
                //    {
                //        int iUserID = 0;
                //        int.TryParse(HttpContext.Current.Session["UserID"].ToString(), out iUserID);
                //        string sUser = string.Empty;
                //        if (iUserID > 0)
                //        {
                //            sUser = HttpContext.Current.Session["sUserName"].ToString();
                //        }
                //        if (string.IsNullOrEmpty(sUser))
                //        {
                //            sUser = "Public without Login";
                //        }
                //        Trace.CreatedByID = iUserID;
                //        Trace.CreatedByName = sUser;
                //    }
                //    Trace.TypeID = 20;
                //    if (oCResult.sMessage.ToLower().StartsWith("error"))
                //    {
                //        Trace.TypeID = 10;
                //    }
                //    if (!string.IsNullOrEmpty(Trace.Description))
                //        Connection.Insert<XIErrorLogs>(Trace, "XIErrorLog_T", "ID");
                //}
            }
            catch (Exception ex)
            {
                XIErrorLogs Trace = new XIErrorLogs();
                if (oCResult.oTraceStack != null && oCResult.oTraceStack.Count() > 0)
                {
                    string sMessage = string.Empty;
                    if (iQSInstanceID > 0)
                    {
                        sMessage = "[QSInstanceID : " + iQSInstanceID + " ] - ";
                    }
                    if (iPolicyID > 0)
                    {
                        sMessage += "[PolicyID : " + iPolicyID + " ] - ";
                    }
                    var Messages = oCResult.oTraceStack.Select(m => m.sValue).ToArray();
                    var Msg = "TraceStack: " + sMessage + string.Join("->", Messages);
                    Trace.CreatedTime = DateTime.Now;
                    Trace.Description = Msg;
                    Trace.FKiQSInstanceID = iQSInstanceID;
                    Trace.FKiPolicyID = iPolicyID;
                    Trace.CreatedBySYSID = Utility.GetIPAddress();
                    if (!string.IsNullOrEmpty(Trace.Description))
                        Connection.Insert<XIErrorLogs>(Trace, "XIErrorLog_T", "ID");
                }
                Trace.Description = "Critial ERROR: Exception in SaveErrortoDB [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                Trace.CreatedTime = DateTime.Now;
                Trace.CreatedBySYSID = Utility.GetIPAddress();
                Trace.TypeID = 20;
                Trace.FKiQSInstanceID = iQSInstanceID;
                Trace.FKiPolicyID = iPolicyID;
                if (oCResult.sMessage.ToLower().StartsWith("error"))
                {
                    Trace.TypeID = 10;
                }
                //Connection.Insert<XIErrorLogs>(Trace, "XIErrorLog_T", "ID");
            }

        }

        public CResult XIMonitor(CResult oResult, int iApplicationID)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "";//expalin about this method logic
            try
            {
                XIInfraCache oCache = new XIInfraCache();
                var o1ClickD = (XID1Click)oCache.GetObjectFromCache(XIConstant.Cache1Click, "XIMonitorByApp");
                var o1ClickC = (XID1Click)o1ClickD.Clone(o1ClickD);
                List<CNV> nParams = new List<CNV>();
                nParams.Add(new CNV { sName = "{XIP|iAppID}", sValue = iApplicationID.ToString() });
                nParams.Add(new CNV { sName = "{XIP|iType}", sValue = oResult.iType.ToString() });
                o1ClickC.ReplaceFKExpressions(nParams);
                var oRes = o1ClickC.OneClick_Run();
                if (oRes != null && oRes.Values.Count() > 0)
                {
                    var iAction = 0;
                    foreach (var items in oRes.Values.ToList())
                    {
                        var sAction = items.AttributeI("iAction").sValue;
                        var sEmail = items.AttributeI("sEmail").sValue;
                        int.TryParse(sAction, out iAction);
                        if (iAction > 0)
                        {
                            if (iAction == (int)xiEnumSystem.EnumXIMonitorAction.SignalR)
                            {
                                if (oSignalR != null)
                                {
                                    oSignalR.ShowSignalRMsg(oResult.sMessage);
                                }
                            }
                            else if (iAction == (int)xiEnumSystem.EnumXIMonitorAction.Email)
                            {
                                XIInfraEmail oEmail = new XIInfraEmail();
                                oEmail.EmailID = sEmail;
                                oEmail.sSubject = "Regulation Exception";
                                string sContent = "<h4>Alert!!! Sanction Exception</h4><p>we found the sanction activity</p><br/><p>Please check the log for more details</p>";
                                oEmail.Sendmail(5, sContent, null);
                            }
                            else if (iAction == (int)xiEnumSystem.EnumXIMonitorAction.SMS)
                            {

                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

    }
}