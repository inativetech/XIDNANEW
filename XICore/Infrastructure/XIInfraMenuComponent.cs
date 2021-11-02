using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using XICore;
using XISystem;

namespace XICore
{
    public class XIInfraMenuComponent : XIDefinitionBase
    {
        public string sMenuName { get; set; }
        public string sSessionID { get; set; }
        public string sGUID { get; set; }
        public int iUserID { get; set; }
        public string sCoreDatabase { get; set; }
        public int iOrgID { get; set; }
        public string RoleID { get; set; }

        XIInfraCache oCache = new XIInfraCache();
        public CResult XILoad(List<CNV> oParams)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = oCResult.Get_MethodName();

            oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiInProcess;

            if (iTraceLevel > 0)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Started Execution" });
            }
            //if iTraceLevel>0 then 
            //oCResult.oTraceStack.Trace("Stage", "Begin query build",milliseconds)
            //oCResult.oTraceStack.Trace("Stage",sError)
            //end if

            if (oCR.xiStatus == xiEnumSystem.xiFuncResult.xiError)
            {
                oCResult.xiStatus = oCR.xiStatus;
                //oCResult.oTraceStack.Trace("Stage",sError)
            }
            try
            {
                XIInfraUsers oUser = new XIInfraUsers();
                CUserInfo oInfo = new CUserInfo();
                oInfo = oUser.Get_UserInfo();
                //oInfo = oInfo.GetUserInfo();
                int iApplicationID = 0;
                int iOrgID = 0;
                sSessionID = oParams.Where(m => m.sName == XIConstant.Param_SessionID).Select(m => m.sValue).FirstOrDefault();
                sGUID = oParams.Where(m => m.sName == XIConstant.Param_GUID).Select(m => m.sValue).FirstOrDefault();
                sMenuName = oParams.Where(m => m.sName == XIConstant.Param_MenuName).Select(m => m.sValue).FirstOrDefault();
                RoleID = oParams.Where(m => m.sName == "iRoleID").Select(m => m.sValue).FirstOrDefault();
                var sAppID = oParams.Where(m => m.sName == XIConstant.Param_ApplicationID).Select(m => m.sValue).FirstOrDefault();
                int.TryParse(sAppID, out iApplicationID);
                var sOrgID = oParams.Where(m => m.sName == "iOrgID").Select(m => m.sValue).FirstOrDefault();
                int.TryParse(sOrgID, out iOrgID);
                var sCallHierarchy = oParams.Where(m => m.sName == XIConstant.Param_CallHierarchy).Select(m => m.sValue).FirstOrDefault();
                iUserID = oInfo.iUserID; //Convert.ToInt32(oParams.Where(m => m.sName == "iUserID").Select(m => m.sValue).FirstOrDefault());
                sCoreDatabase = oInfo.sCoreDataBase;
                iOrgID = oInfo.iOrganizationID; //Convert.ToInt32(oParams.Where(m => m.sName == "iOrganizationID").Select(m => m.sValue).FirstOrDefault());
                List<XIMenu> oRightMenus = new List<XIMenu>();
                List<XIMenu> oRightMenuC = new List<XIMenu>();
                XIDXI oXID = new XIDXI();
                oXID.sCoreDatabase = sCoreDatabase;
                oXID.iOrgID = iOrgID;
                oXID.sUserID = iUserID.ToString();
                if (!string.IsNullOrEmpty(sMenuName))
                {
                    oRightMenus = (List<XIMenu>)oCache.GetObjectFromCache(XIConstant.CacheMenu, sMenuName, null);
                }
                else if (!string.IsNullOrEmpty(RoleID))
                {
                    string sQuery = "select * from XIMenu_T xi join XIRoleMenus_T xr on xi.ID=xr.MenuID where xr.StatusTypeID=10 and xr.RoleID=" + RoleID + " and xr.FKiApplicationID=" + iApplicationID + " and xr.OrgID=" + iOrgID + " order by xr.id asc";
                    string sReturnValue = string.Empty;
                    XID1Click oXI1Click = new XID1Click();
                    oXI1Click.Query = sQuery;
                    oXI1Click.Name = "XI Menu";
                    var QueryResult = oXI1Click.Execute_Query();
                    if (QueryResult.Rows.Count > 0)
                    {
                        oRightMenus = (from DataRow row in QueryResult.Rows
                                       select new XIMenu
                                       {
                                           ID = Convert.ToInt32(row["ID"]),
                                           MenuID = row["MenuID"].ToString(),
                                           Name = row["Name"].ToString(),
                                           OrgID = Convert.ToInt32(row["OrgID"]),
                                           RoleID = Convert.ToInt32(row["RoleID"]),
                                           ParentID = Convert.ToString(row["ParentID"]),
                                           Priority = Convert.ToInt32(row["Priority"]),
                                           FKiApplicationID = Convert.ToInt32(row["FKiApplicationID"]),
                                           RootName = Convert.ToString("RootName"),
                                           XiLinkID = Convert.ToInt32(row["XiLinkID"]),
                                           ActionType = Convert.ToInt32(row["ActionType"]),
                                           sIcon = row["sIcon"].ToString(),
                                       }).ToList();
                    }
                    XIDXI XIDXI = new XIDXI();
                    oRightMenuC = XIDXI.Countdata(oRightMenus, sCoreDatabase, null);
                }
                else
                {
                    oCR.sMessage = "Config Error: XIInfraMenuComponent_XILoad() : Menu Name is not passed as parameter - Call Hierarchy: " + sCallHierarchy;
                    oCR.sCode = "Config Error";
                    SaveErrortoDB(oCR);
                }
                XID1Click oD1Click = new XID1Click();
                oRightMenuC = (List<XIMenu>)oD1Click.Clone(oRightMenus);
                foreach (var Menu in oRightMenuC)
                {
                    if (Menu.XiLinkID > 0)
                    {
                        var oXiLink = (XILink)oCache.GetObjectFromCache(XIConstant.CacheXILink, "", Menu.XiLinkID.ToString());
                        if (oXiLink != null && !string.IsNullOrEmpty(oXiLink.sActive) && oXiLink.sActive.IndexOf("xi.s") >= 0)
                        {
                            //XIDScript oXIScript = new XIDScript();
                            //oXIScript.sScript = oXiLink.sActive;
                            //oCR = oXIScript.Execute_Script(sGUID, sSessionID);
                            oCR = RunScript(oXiLink.sActive, sGUID, sSessionID);
                            //  xi.s|{xi.a|'ACPolicy_T',{xi.p|iACPolicyID},'iStatus','10',''}
                            // xi.s|{if|{eq|{xi.a|'ACPolicy_T',{xi.p|iACPolicyID},'iStatus'},'190'},'Y','N'}  Cancellation 
                            // xi.s|{if|{eq|{xi.a|'ACPolicy_T',{xi.p|iACPolicyID},'iStatus'},'200'},'Y','N'}  Revoke 
                            // xi.s|{if|{gt|{xi.a|'ACPolicy_T',{xi.p|iACPolicyID},'rBalance'},'0'},'Y','N'}   Premium Finance Menus
                            if (oCR.bOK && oCR.oResult != null)
                            {
                                string sValue = (string)oCR.oResult;
                                if (!string.IsNullOrEmpty(sValue) && sValue.ToLower() == "n")
                                {
                                    Menu.isHide = true;
                                }
                            }
                        }
                        foreach (var subMenu in Menu.SubGroups)
                        {
                            var oXiLink1 = (XILink)oCache.GetObjectFromCache(XIConstant.CacheXILink, "", Menu.XiLinkID.ToString());
                            var oXi1ClickC = (XILink)oD1Click.Clone(oXiLink1);
                            if (oXi1ClickC != null && !string.IsNullOrEmpty(oXi1ClickC.sActive) && oXi1ClickC.sActive.IndexOf("xi.s") >= 0)
                            {
                                XIDScript oXIScript1 = new XIDScript();
                                oXIScript1.sScript = oXi1ClickC.sActive;
                                oCR = oXIScript1.Execute_Script(sGUID, sSessionID);
                                if (oCR.bOK && oCR.oResult != null)
                                {
                                    string sValue = (string)oCR.oResult;
                                    if (!string.IsNullOrEmpty(sValue) && sValue.ToLower() == "n")
                                    {
                                        subMenu.isHide = true;
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        foreach (var subMenu in Menu.SubGroups)
                        {
                            if (subMenu.XiLinkID > 0)
                            {
                                var oXiLink1 = (XILink)oCache.GetObjectFromCache(XIConstant.CacheXILink, "", subMenu.XiLinkID.ToString());
                                var oXi1ClickC = (XILink)oD1Click.Clone(oXiLink1);
                                if (oXi1ClickC != null && !string.IsNullOrEmpty(oXi1ClickC.sActive) && oXi1ClickC.sActive.IndexOf("xi.s") >= 0)
                                {
                                    XIDScript oXIScript1 = new XIDScript();
                                    oXIScript1.sScript = oXi1ClickC.sActive;
                                    oCR = oXIScript1.Execute_Script(sGUID, sSessionID);
                                    if (oCR.bOK && oCR.oResult != null)
                                    {
                                        string sValue = (string)oCR.oResult;
                                        if (!string.IsNullOrEmpty(sValue) && sValue.ToLower() == "n")
                                        {
                                            subMenu.isHide = true;
                                        }
                                    }
                                }

                            }
                        }
                    }
                }
                //var oResult = oXID.Get_RightMenuDefinition(sMenuName);
                //if (oResult.bOK && oResult.oResult != null)
                //{
                //    oRightMenus = (List<XIMenu>)oResult.oResult;
                //}
                oCResult.oResult = oRightMenuC;
                if (oRightMenuC != null && oRightMenuC.Count() > 0)
                {
                    oRightMenuC = oRightMenuC.OrderBy(m => m.Priority).ToList();
                    var Data = MenusByPriority(oRightMenuC);
                    oCResult.oResult = oRightMenuC;
                }
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error While Executing Menu Component" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oCResult;
        }
        public List<XIMenu> MenusByPriority(List<XIMenu> Menus)
        {
            foreach (var Menu in Menus)
            {
                Menu.SubGroups = Menu.SubGroups.OrderBy(m => m.Priority).ToList();
                if (Menu.SubGroups.Count() > 0)
                {
                    MenusByPriority(Menu.SubGroups);
                }
            }
            return Menus;
        }
        public CResult RunScript(string sScript, string sGUID, string sSessionID)
        {
            CResult oCResult = new CResult();
            try
            {
                if (sScript.IndexOf("xi.s") >= 0)
                {
                    XIDScript oXIScript = new XIDScript();
                    oXIScript.sScript = sScript;
                    var oCR = oXIScript.Execute_Script(sGUID, sSessionID);
                    if (oCR.bOK && oCR.oResult != null)
                    {
                        string sSubScript = (string)oCR.oResult;
                        if (sSubScript.IndexOf("xi.s") >= 0)
                        {
                            oCR = RunScript(sSubScript, sGUID, sSessionID);
                            oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                            oCResult.oResult = oCR.oResult;
                        }
                        else
                        {
                            oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                            oCResult.oResult = sSubScript;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error While Executing Menu Component" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oCResult;
        }
    }
}