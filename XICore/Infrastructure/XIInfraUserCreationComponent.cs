using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Web;
using XISystem;

namespace XICore
{
    public class XIInfraUserCreationComponent
    {
        public CResult XILoad(List<CNV> oParams)
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
                var sCoreDB = oParams.Where(m => m.sName.ToLower() == "sCoreDatabase".ToLower()).Select(m => m.sValue).FirstOrDefault();
                var sUserID = oParams.Where(m => m.sName.ToLower() == "iUserID".ToLower()).Select(m => m.sValue).FirstOrDefault();
                int iUserID = 0;
                int.TryParse(sUserID, out iUserID);
                var sOrgID = oParams.Where(m => m.sName.ToLower() == "iorgid").Select(m => m.sValue).FirstOrDefault();
                int iOrgID = 0;
                int.TryParse(sOrgID, out iOrgID);               
                oTrace.oParams.Add(new CNV { sName = "iUserID", sValue = iUserID.ToString() });
                Dictionary<string, object> Data = new Dictionary<string, object>();
                XIInfraRoles oRole = new XIInfraRoles();
                var Roles = oRole.Get_RolesTree(sCoreDB, iOrgID);
                Data["Roles"] = Roles.oResult;
                if (iUserID > 0)//check mandatory params are passed or not
                {
                    XIInfraUsers xifuser = new XIInfraUsers();
                    xifuser.UserID = iUserID;
                    xifuser = (XIInfraUsers)xifuser.Get_UserDetails(sCoreDB).oResult;
                    Data["User"] = xifuser;
                }
                else
                {
                    
                }
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                oCResult.oResult = Data;
                oCResult.xiStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                //else
                //{
                //    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                //    oTrace.sMessage = "Mandatory Param:  is missing";
                //}
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString();
                oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                //SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }
    }
}