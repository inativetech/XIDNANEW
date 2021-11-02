using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web;
using XISystem;

namespace XICore
{
    public class XIDBOAction
    {
        public int ID { get; set; }
        public string sName { get; set; }
        public string sCode { get; set; }
        public int iType { get; set; }
        public int iSystemAction { get; set; }
        public int FKiBOID { get; set; }
        public int FKiActionID { get; set; }
        public int FKiScriptID { get; set; }
        public int FKiAlgorithmID { get; set; }
        public int FKiQSDefinitionID { get; set; }
        public bool bMatrix { get; set; }

        public List<XIDBOActionNV> ActionNV { get; set; }

        public CResult Execute_Action(XIIBO oBOI)
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
                oTrace.oParams.Add(new CNV { sName = "ID", sValue = ID.ToString() });
                if (ID > 0)//check mandatory params are passed or not
                {
                    XIInfraCache oCache = new XIInfraCache();
                    var sGUID = Guid.NewGuid().ToString();
                    List<CNV> oNVsList = new List<CNV>();
                    string sSessionID = string.Empty;
                    if (ActionNV != null && ActionNV.Count() > 0)
                    {
                        foreach (var items in ActionNV)
                        {
                            oCR = oBOI.Resolve_Notation(items.sValue);
                            if (oCR.bOK && oCR.oResult != null)
                            {
                                oTrace.oTrace.Add(oCR.oTrace);
                                var sResolvedVal = (string)oCR.oResult;
                                oNVsList.Add(new CNV { sName = items.sName, sValue = sResolvedVal });
                            }
                            else
                            {
                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                            }
                        }
                        if (oTrace.iStatus != (int)xiEnumSystem.xiFuncResult.xiError)
                        {
                            if (string.IsNullOrEmpty(sSessionID))
                            {
                                if (HttpContext.Current != null && HttpContext.Current.Session != null && HttpContext.Current.Session.SessionID != null)
                                {
                                    sSessionID = HttpContext.Current.Session.SessionID;
                                }
                            }
                            oNVsList.Add(new CNV { sName = "-iBOIID", sValue = oBOI.Attributes[oBOI.BOD.sPrimaryKey.ToLower()].sValue });
                            oCache.SetXIParams(oNVsList, sGUID, sSessionID);
                            XIDAlgorithm oAlogD = new XIDAlgorithm();
                            oAlogD = (XIDAlgorithm)oCache.GetObjectFromCache(XIConstant.CacheXIAlgorithm, null, "9");
                            oAlogD.Execute_XIAlgorithm(sSessionID, sGUID);
                        }
                    }
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory Param: ID is missing";
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

    public class XIDBOActionNV
    {
        public int ID { get; set; }
        public int FKiBOActionID { get; set; }
        public string sName { get; set; }
        public string sValue { get; set; }
    }
}