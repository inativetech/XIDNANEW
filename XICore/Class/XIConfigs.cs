using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using XIDatabase;
using XISystem;
using xiEnumSystem;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;

namespace XICore
{
    public class XIConfigs
    {
        XIDefinitionBase oXIDef = null;
        private iSiganlR oSignalR = null;
        CResult oSigCR = null;
        public XIConfigs()
        {
        }
        public XIConfigs(iSiganlR oSignalRI)
        {
            oSigCR = new CResult();
            oSigCR.oStack = new BindingList<CNV>();
            oSigCR.oStack.ListChanged += new ListChangedEventHandler(listOfParts_ListChanged);
            oSignalR = oSignalRI;
            //oXIDef = new XIDefinitionBase(oSignalR);
        }
        public int iAppID;
        public int iOrgID { get; set; }
        public int iUserID { get; set; }
        public int iBODID;
        public string sAppName { get; set; }
        public string sDatabase { get; set; }
        public int iCoreDataSourceID;
        public int iSharedDataSourceID;
        public string iCreateGroupID { get; set; }
        //public int i1ClickID { get; set; }
        public string sTableName { get; set; }
        public string sConfigDatabase { get; set; }
        public string sLabelName { get; set; }
        public string sBOName { get; set; }
        public string sPrimaryKey { get; set; }
        public string sStructureName { get; set; }
        public string sCode { get; set; }
        public string sSessionID { get; set; }
        int iQSID = 0;
        int iRowXilinkID = 0;
        public string iOrganisationID { get; set; }

        XIInfraCache oCache = new XIInfraCache();
        XIInfraEncryption oEncry = new XIInfraEncryption();
        XIDXI oXID = new XIDXI();
        XIDBAPI Connection = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString);

        #region Application

        public CResult Save_Application(XIDApplication model)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            try
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Application insertion started" });
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "Application insertion started" });
                }
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XI Application");
                oBOI.BOD = oBOD;
                string sType = model.ID > 0 ? "Edit" : "Add";
                oBOI.SetAttribute("ID", model.ID.ToString());
                oBOI.SetAttribute("sApplicationName", model.sApplicationName);
                oBOI.SetAttribute("sLogo", model.sLogo);
                oBOI.SetAttribute("sDescription", model.sDescription);
                oBOI.SetAttribute("sDatabaseName", model.sApplicationName + "_Core");
                oBOI.SetAttribute("StatusTypeID", model.StatusTypeID.ToString());
                oBOI.SetAttribute("CreatedBy", iUserID.ToString());
                oBOI.SetAttribute("CreatedTime", DateTime.Now.ToString());
                oBOI.SetAttribute("CreatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                oBOI.SetAttribute("UpdatedBy", iUserID.ToString());
                oBOI.SetAttribute("UpdatedTime", DateTime.Now.ToString());
                oBOI.SetAttribute("UpdatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                oBOI.SetAttribute("sConnectionString", model.sConnectionString);
                oBOI.SetAttribute("sTheme", model.sTheme == "0" ? XIConstant.DefaultThemeID : model.sTheme);
                oBOI.SetAttribute("sUserName", model.XIAppUserName);
                oBOI.SetAttribute("sOTP", model.sOTP);
                oCR = oBOI.Save(oBOI);
                if (oCR.bOK && oCR.oResult != null)
                {
                    if (oSigCR != null)
                    {
                        oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "Application insertion completed" });
                    }
                    if (model.ID == 0)
                    {
                        oBOI = (XIIBO)oCR.oResult;
                        int iAppID = 0;
                        var sAppID = oBOI.Attributes.Where(m => m.Key.ToLower() == "id").Select(s => s.Value).Select(m => m.sValue).FirstOrDefault();
                        var sScriptsList = oBOI.BOD.sScripts.ToList().Where(m => m.IsSuccess == false).ToList();
                        if (sScriptsList.Count() == 0)
                        {
                            int.TryParse(sAppID, out iAppID);
                            //Create URL for Application
                            XIURLMappings oURL = new XIURLMappings();
                            oURL.sUrlName = model.sApplicationName;
                            oURL.sActualUrl = model.sApplicationName;
                            oURL.FKiApplicationID = iAppID;
                            oURL.sType = "Application";
                            oURL.FKiSourceID = 0;
                            oURL.StatusTypeID = 10;
                            oURL.OrganisationID = iOrgID;
                            var oCRes = Save_URLMapping(oURL);
                            if (oCRes.bOK && oCRes.oResult != null)
                            {
                                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "URL Created for Application - " + model.sApplicationName });
                            }
                            else
                            {
                                oCResult.oTraceStack.Add(new CNV { sName = "Error", sValue = "URL insertion failed" });
                            }
                        }
                        else
                        {
                            oCResult.oTraceStack.Add(new CNV { sName = "Error", sValue = "Validation scripts returned errors while inserting application" });
                        }
                    }
                }
                else
                {
                    oCResult.oTraceStack.Add(new CNV { sName = "Error", sValue = "Error While Inserting Application" });
                }
                oCResult = oCR;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error While Inserting Application" });
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Application insertion failed" });
                oSigCR.oStack.Add(new CNV { sName = "Error Message", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            return oCResult;
        }

        public CResult Build_Application(List<CNV> oParams)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oCResult.sMessage = "someone tried to do something they shouldnt";
            //Update database name for the created application
            //Save URL Mapping for the application
            //Save DataSources for core and shared databases
            //Create Core and Shared Databases
            //Create User tables in Core Database
            //Create Organisation
            //Create Application Login
            //Create IDE Login for Application
            //Create Layout for Application Login
            //Update layout and theme to application login Role
            //Create Menu for application
            //Add Reference Data Menu
            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "Application insertion completed" });
                }
                string sBO = oParams.Where(m => m.sName.ToLower() == XIConstant.Param_BO.ToLower()).Select(m => m.sValue).FirstOrDefault();
                string sAppID = oParams.Where(m => m.sName.ToLower() == XIConstant.Param_InstanceID.ToLower()).Select(m => m.sValue).FirstOrDefault();
                int.TryParse(sAppID, out iAppID);
                oTrace.oParams.Add(new CNV { sName = "sBO", sValue = sBO });
                oTrace.oParams.Add(new CNV { sName = "iAppID", sValue = iAppID.ToString() });
                if (!string.IsNullOrEmpty(sBO) && iAppID > 0)
                {
                    string bDefaults = string.Empty;
                    XIInfraCache oCache = new XIInfraCache();
                    XIDBO oBOD = new XIDBO();
                    oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, sBO, null);
                    XIIXI oXI = new XIIXI();
                    List<CNV> oWhrParams = new List<CNV>();
                    oWhrParams.Add(new CNV { sName = oBOD.sPrimaryKey, sValue = iAppID.ToString() });
                    var oBOI = oXI.BOI(sBO, null, null, oWhrParams);
                    var iParentMenuID = "0";
                    var iCreateParentMenuID = "0";
                    if (oBOI != null && oBOI.Attributes.Values.Count() > 0)
                    {
                        if (oBOI.Attributes.ContainsKey("bDefaults"))
                        {
                            bDefaults = oBOI.Attributes["bDefaults"].sValue;
                        }
                        if (bDefaults == "False" || string.IsNullOrEmpty(bDefaults))
                        {
                            if (oBOI.Attributes.ContainsKey("sApplicationName"))
                            {
                                sAppName = oBOI.Attributes["sApplicationName"].sValue;
                                string sTheme = oBOI.Attributes["sTheme"].sValue;
                                string sUserName = oBOI.Attributes["sUserName"].sValue;
                                string sPassword = oBOI.Attributes["sPassword"].sValue;
                                XIInfraEncryption oEncrypt = new XIInfraEncryption();
                                string sDecrptVal = oEncrypt.DecryptData(sPassword, true, iAppID.ToString());
                                sPassword = sDecrptVal;
                                string sURL = sAppName;
                                if (sAppName.Length > 16)
                                {
                                    sAppName = sAppName.Substring(0, 15);
                                    sURL = sAppName.Substring(0, 15);
                                }
                                sAppName = sAppName.Replace(" ", "");
                                oBOI.SetAttribute("sDatabaseName", sAppName + "_Core");
                                var oCloneBOD = (XIDBO)oBOD.Clone(oBOD);
                                //oCloneBOD.Attributes = oCloneBOD.Attributes.ToDictionary(dic => dic.Key, dic => dic.Value);
                                oCloneBOD.Scripts = new Dictionary<string, XIDScript>();
                                oBOI.BOD = oCloneBOD;
                                oCR = oBOI.SaveV2(oBOI);
                                oTrace.oTrace.Add(oCR.oTrace);
                                if (oCR.bOK && oCR.oResult != null)
                                {
                                    XIURLMappings oURL = new XIURLMappings();
                                    oURL.sUrlName = sURL;
                                    oURL.sActualUrl = sURL;
                                    oURL.FKiApplicationID = iAppID;
                                    oURL.sType = "Application";
                                    oURL.FKiSourceID = 0;
                                    oURL.StatusTypeID = 10;
                                    oURL.OrganisationID = iOrgID;
                                    oCR = Save_URLMapping(oURL);
                                    oTrace.oTrace.Add(oCR.oTrace);
                                    if (oCR.bOK && oCR.oResult != null)
                                    {
                                        List<CNV> mParams = new List<CNV>();
                                        mParams.Add(new CNV { sName = XIConstant.DB_Type, sValue = XIConstant.DB_Core });
                                        oCR = Save_DataSource(mParams);
                                        oTrace.oTrace.Add(oCR.oTrace);
                                        if (oCR.bOK && oCR.oResult != null)
                                        {
                                            var oDS = (XIIBO)oCR.oResult;
                                            var sDataSourceID = oDS.Attributes.Where(m => m.Key.ToLower() == "id").Select(m => m.Value).Select(m => m.sValue).FirstOrDefault();
                                            int.TryParse(sDataSourceID, out iCoreDataSourceID);
                                        }
                                        else
                                        {
                                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                        }
                                        mParams = new List<CNV>();
                                        mParams.Add(new CNV { sName = XIConstant.DB_Type, sValue = XIConstant.DB_Shared });
                                        oCR = Save_DataSource(mParams);
                                        oTrace.oTrace.Add(oCR.oTrace);
                                        if (oCR.bOK && oCR.oResult != null)
                                        {
                                            var oDS = (XIIBO)oCR.oResult;
                                            var sDataSourceID = oDS.Attributes.Where(m => m.Key.ToLower() == "id").Select(m => m.Value).Select(m => m.sValue).FirstOrDefault();
                                            int.TryParse(sDataSourceID, out iSharedDataSourceID);
                                        }
                                        else
                                        {
                                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                        }
                                        string iRoleID = string.Empty;
                                        int iNewOrgID = 0;
                                        if (oTrace.iStatus != (int)xiEnumSystem.xiFuncResult.xiError)
                                        {
                                            //Create Application config admin
                                            XIDApplication oXIApp = new XIDApplication();
                                            oXIApp.sApplicationName = sAppName;
                                            oXIApp.ID = iAppID;
                                            oXIApp.sUserName = "App" + sUserName;
                                            oXIApp.XIAppPassword = sPassword;
                                            //oXIApp.FKiOrganisationID = iNewOrgID;
                                            string sAdminRole = "AppAdmin";
                                            oCR = Save_ApplicationUser(oXIApp, sAdminRole);
                                            oTrace.oTrace.Add(oCR.oTrace);
                                            if (oCR.bOK && oCR.oResult != null)
                                            {
                                                var oRoleDef = (XIIBO)oCR.oResult;
                                                iRoleID = oRoleDef.Attributes.Where(m => m.Key.ToLower() == "roleid").Select(m => m.Value).Select(m => m.sValue).FirstOrDefault();
                                            }
                                            else
                                            {
                                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                            }
                                            //Creating Organisation with Application Data
                                            XIDOrganisation oOrg = new XIDOrganisation();
                                            oOrg.FKiApplicationID = iAppID;
                                            oOrg.Name = sAppName;
                                            oOrg.Email = sUserName;
                                            oOrg.Password = sPassword;
                                            oOrg.Description = sAppName;
                                            var iAppThemeID = 0;
                                            int.TryParse(sTheme, out iAppThemeID);
                                            oOrg.ThemeID = iAppThemeID;
                                            oCR = Save_Organisation(oOrg);
                                            oTrace.oTrace.Add(oCR.oTrace);
                                            if (oCR.bOK && oCR.oResult != null)
                                            {
                                                var oOr = (XIIBO)oCR.oResult;
                                                var sOrgID = oOr.Attributes.Where(m => m.Key.ToLower() == "id").Select(m => m.Value).Select(m => m.sValue).FirstOrDefault();
                                                int.TryParse(sOrgID, out iNewOrgID);

                                                //Creating User with Admin Role


                                            }
                                            else
                                            {
                                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                            }

                                            if (oTrace.iStatus != (int)xiEnumSystem.xiFuncResult.xiError)
                                            {
                                                //Creating User with SuperAdmin Role
                                                oXIApp = new XIDApplication();
                                                oXIApp.sApplicationName = sAppName;
                                                oXIApp.ID = iAppID;
                                                oXIApp.sUserName = "ide" + sUserName;
                                                oXIApp.XIAppPassword = sPassword;
                                                oXIApp.FKiOrganisationID = 0;
                                                string sSuperAdminRole = "XISuperAdmin";
                                                oCR = Save_ApplicationUser(oXIApp, sSuperAdminRole);
                                                oTrace.oTrace.Add(oCR.oTrace);
                                                if (oCR.bOK && oCR.oResult != null)
                                                {
                                                    var oRoleDef = (XIIBO)oCR.oResult;
                                                    var iSuperRoleID = oRoleDef.Attributes.Where(m => m.Key.ToLower() == "roleid").Select(m => m.Value).Select(m => m.sValue).FirstOrDefault();
                                                    XIIBO oBOI1 = new XIIBO();
                                                    XIDBO oBOD1 = new XIDBO();
                                                    oBOD1 = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIAppRoles_AR_T");
                                                    oBOI1.BOD = oBOD1;
                                                    oBOD1.iDataSource = iCoreDataSourceID;
                                                    oBOI1.SetAttribute("RoleID", iSuperRoleID.ToString());
                                                    oBOI1.SetAttribute("iLayoutID", "2475");
                                                    oBOI1.SetAttribute("iThemeID", sTheme);
                                                    oCR = oBOI1.SaveV2(oBOI1);
                                                    oTrace.oTrace.Add(oCR.oTrace);
                                                    if (oCR.bOK && oCR.oResult != null)
                                                    {

                                                    }
                                                    else
                                                    {
                                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                    }
                                                }
                                                else
                                                {
                                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                }
                                            }
                                        }
                                        if (oTrace.iStatus != (int)xiEnumSystem.xiFuncResult.xiError)
                                        {
                                            mParams = new List<CNV>();
                                            mParams.Add(new CNV { sName = XIConstant.Param_Layout, sValue = XIConstant.Auto3LayoutHTML });
                                            XIDLayout oLayout = new XIDLayout();
                                            oLayout.LayoutName = "AppDefaultLayout";
                                            oLayout.LayoutType = "Inline";
                                            oLayout.LayoutCode = XIConstant.Auto3LayoutHTML;
                                            oLayout.FKiApplicationID = iAppID;
                                            oLayout.Authentication = "Authenticated";
                                            oLayout.LayoutLevel = "OrganisationLevel";
                                            oLayout.StatusTypeID = 10;
                                            oLayout.bIsTaskBar = true;
                                            oLayout.sTaskBarPosition = "Left";
                                            string sMenu = sAppName;
                                            oCR = Save_ApplicationDefaultLayout(oLayout, sMenu);
                                            oTrace.oTrace.Add(oCR.oTrace);
                                            if (oCR.bOK && oCR.oResult != null)
                                            {
                                                var oLayoutDef = (XIIBO)oCR.oResult;
                                                var iLayoutID = oLayoutDef.Attributes.Where(m => m.Key.ToLower() == "id").Select(m => m.Value).Select(m => m.sValue).FirstOrDefault();
                                                XIIBO oBOI1 = new XIIBO();
                                                XIDBO oBOD1 = new XIDBO();
                                                oBOD1 = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIAppRoles_AR_T");
                                                oBOI1.BOD = oBOD1;
                                                oBOD1.iDataSource = iCoreDataSourceID;
                                                oBOI1.SetAttribute("RoleID", iRoleID.ToString());
                                                oBOI1.SetAttribute("iLayoutID", iLayoutID.ToString());
                                                oBOI1.SetAttribute("iThemeID", sTheme);
                                                oCR = oBOI1.SaveV2(oBOI1);
                                                if (oCR.bOK && oCR.oResult != null)
                                                {
                                                    //oConfig.AutoMenuCreationForApplication();
                                                }
                                                else
                                                {
                                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                }
                                            }
                                            else
                                            {
                                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                            }
                                        }

                                        if (oTrace.iStatus != (int)xiEnumSystem.xiFuncResult.xiError)
                                        {
                                            XIMenu oMenu = new XIMenu();
                                            oMenu.FKiApplicationID = iAppID;
                                            oMenu.OrgID = iNewOrgID;
                                            oMenu.Name = sAppName;
                                            oMenu.RootName = sAppName;
                                            oMenu.ParentID = "#";
                                            oMenu.ActionType = 0;
                                            oMenu.StatusTypeID = 10;
                                            oMenu.XiLinkID = 0;
                                            oCR = Save_Menu(oMenu);
                                            oTrace.oTrace.Add(oCR.oTrace);
                                            if (oCR.bOK && oCR.oResult != null)
                                            {
                                                var oMenuI = (XIIBO)oCR.oResult;
                                                iParentMenuID = oMenuI.Attributes.Where(m => m.Key.ToLower() == "id").Select(m => m.Value).Select(m => m.sValue).FirstOrDefault();
                                            }
                                            else
                                            {
                                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                            }
                                            XIAssignMenu oAM = new XIAssignMenu();
                                            oAM.FKiApplicationID = iAppID;
                                            oAM.Name = sAppName;
                                            oAM.RootName = sAppName;
                                            oAM.ParentID = "#";
                                            oAM.sType = "AssignMenu";
                                            var oCreateMenu = Save_AssignMenu(oAM);
                                            if (oCreateMenu.bOK && oCreateMenu.oResult != null)
                                            {
                                                var oMenuI = (XIIBO)oCreateMenu.oResult;
                                                iCreateParentMenuID = oMenuI.Attributes.Where(m => m.Key.ToLower() == "id").Select(m => m.Value).Select(m => m.sValue).FirstOrDefault();
                                            }
                                            else
                                            {
                                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                        oCResult.oTraceStack.Add(new CNV { sName = "Error", sValue = "URL insertion failed" });
                                    }
                                }
                                else
                                {
                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                }
                                if (oTrace.iStatus != (int)xiEnumSystem.xiFuncResult.xiError)
                                {
                                    oParams = new List<CNV>();
                                    //Create Reference Data Menu
                                    oParams = new List<CNV>();
                                    oParams.Add(new CNV { sName = "LayoutName", sValue = sAppName + " Reference Data Layout" });
                                    oParams.Add(new CNV { sName = "DialogName", sValue = sAppName + " Reference Data Dialog" });
                                    oParams.Add(new CNV { sName = "XilinkName", sValue = sAppName + " Reference Data Xilink" });
                                    oParams.Add(new CNV { sName = "sParentID", sValue = iParentMenuID });
                                    oParams.Add(new CNV { sName = "sCreateParentID", sValue = iCreateParentMenuID });
                                    oParams.Add(new CNV { sName = "sApplicationName", sValue = sAppName });
                                    oParams.Add(new CNV { sName = "IsRowClick", sValue = "0" });
                                    oParams.Add(new CNV { sName = XIConstant.Param_ApplicationID, sValue = iAppID.ToString() });
                                    oCR = Save_ReferenceData(oParams);
                                    oTrace.oTrace.Add(oCR.oTrace);
                                    if (oCR.bOK && oCR.oResult != null)
                                    {
                                        var i1ClickID = (int)oCR.oResult;
                                        //var i1ClickID = o1ClickDef.Attributes.Where(m => m.Key.ToLower() == "id").Select(m => m.Value).Select(m => m.sValue).FirstOrDefault();
                                        //Create Reference Data RowClick
                                        oParams = new List<CNV>();
                                        oParams.Add(new CNV { sName = "LayoutName", sValue = sAppName + " Reference Data RowClick Layout" });
                                        oParams.Add(new CNV { sName = "DialogName", sValue = sAppName + " Reference Data RowClick Dialog" });
                                        oParams.Add(new CNV { sName = "XilinkName", sValue = sAppName + " Reference Data RowClick Xilink" });
                                        oParams.Add(new CNV { sName = "IsRowClick", sValue = "1" });
                                        oParams.Add(new CNV { sName = "i1ClickID", sValue = i1ClickID.ToString() });
                                        oParams.Add(new CNV { sName = XIConstant.Param_ApplicationID, sValue = iAppID.ToString() });
                                        oCR = Save_ReferenceData(oParams);
                                        oTrace.oTrace.Add(oCR.oTrace);
                                        if (oCR.bOK && oCR.oResult != null)
                                        {

                                        }
                                        else
                                        {
                                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                        }
                                    }
                                    else
                                    {
                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                    }
                                }
                            }
                            else
                            {
                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                            }
                            if (oTrace.iStatus != (int)xiEnumSystem.xiFuncResult.xiError)
                            {
                                oBOI.SetAttribute("bDefaults", "1");
                                oCR = oBOI.SaveV2(oBOI);
                                oTrace.oTrace.Add(oCR.oTrace);
                                if (oCR.bOK && oCR.oResult != null)
                                {
                                    oCResult = oCR;
                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                                    oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                                }
                                else
                                {
                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                }
                            }
                            else
                            {
                                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                            }
                        }
                    }
                    else
                    {
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                    }
                }
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        void listOfParts_ListChanged(object sender, ListChangedEventArgs e)
        {
            BindingList<CNV> oMsg = (BindingList<CNV>)sender;
            if (oMsg.Count() > 0)
            {
                if (oMsg.LastOrDefault().sName.ToLower() == "error" || oMsg.LastOrDefault().sName.ToLower() == "error message")
                {
                    oSignalR.ShowSignalRMsg("<span class=\"SigRError\">" + oMsg.LastOrDefault().sName + " : " + oMsg.LastOrDefault().sValue + "</span>");
                }
                else if (oMsg.LastOrDefault().sName.ToLower() == "success")
                {
                    oSignalR.ShowSignalRMsg("<span class=\"SigRSuccess\">" + oMsg.LastOrDefault().sName + " : " + oMsg.LastOrDefault().sValue + "</span>");
                }
                else
                {
                    oSignalR.ShowSignalRMsg(oMsg.LastOrDefault().sName + " : " + oMsg.LastOrDefault().sValue);
                }
            }
        }

        #endregion Application

        #region URL Mapping

        public CResult Save_URLMapping(XIURLMappings oUrl)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            var sAPPName = oUrl.sUrlName;
            try
            {
                oTrace.oParams.Add(new CNV { sName = "sAPPName", sValue = sAPPName });
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "URL insertion started for Application " + sAPPName });
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "URL insertion started for Application " + sAPPName });
                }
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "UrlMapping");
                oBOI.BOD = oBOD;
                oBOI.Attributes["ID".ToLower()] = new XIIAttribute { sName = "ID", sValue = oUrl.ID.ToString(), bDirty = true };
                oBOI.Attributes["FKiApplicationID".ToLower()] = new XIIAttribute { sName = "FKiApplicationID", sValue = oUrl.FKiApplicationID.ToString(), bDirty = true };
                oBOI.Attributes["sUrlName".ToLower()] = new XIIAttribute { sName = "sUrlName", sValue = oUrl.sUrlName.Replace(" ", ""), bDirty = true };
                oBOI.Attributes["sActualUrl".ToLower()] = new XIIAttribute { sName = "sActualUrl", sValue = oUrl.sActualUrl, bDirty = true };
                oBOI.Attributes["sType".ToLower()] = new XIIAttribute { sName = "sType", sValue = oUrl.sType, bDirty = true };
                oBOI.Attributes["FKiSourceID".ToLower()] = new XIIAttribute { sName = "FKiSourceID", sValue = oUrl.FKiSourceID.ToString(), bDirty = true };
                oBOI.Attributes["StatusTypeID".ToLower()] = new XIIAttribute { sName = "StatusTypeID", sValue = oUrl.StatusTypeID.ToString(), bDirty = true };
                oBOI.Attributes["OrganisationID".ToLower()] = new XIIAttribute { sName = "OrganisationID", sValue = oUrl.OrganisationID.ToString(), bDirty = true };
                if (oUrl.ID == 0)
                {
                    oBOI.Attributes["CreatedBy".ToLower()] = new XIIAttribute { sName = "CreatedBy", sValue = iUserID.ToString(), bDirty = true };
                    oBOI.Attributes["CreatedTime".ToLower()] = new XIIAttribute { sName = "CreatedTime", sValue = DateTime.Now.ToString(), bDirty = true };
                    oBOI.Attributes["CreatedBySYSID".ToLower()] = new XIIAttribute { sName = "CreatedBySYSID", sValue = Dns.GetHostAddresses(Dns.GetHostName())[1].ToString(), bDirty = true };
                    oBOI.Attributes["UpdatedBy".ToLower()] = new XIIAttribute { sName = "UpdatedBy", sValue = iUserID.ToString(), bDirty = true };
                    oBOI.Attributes["UpdatedTime".ToLower()] = new XIIAttribute { sName = "UpdatedTime", sValue = DateTime.Now.ToString(), bDirty = true };
                    oBOI.Attributes["UpdatedBySYSID".ToLower()] = new XIIAttribute { sName = "UpdatedBySYSID", sValue = Dns.GetHostAddresses(Dns.GetHostName())[1].ToString(), bDirty = true };
                }
                else
                {
                    oBOI.Attributes["UpdatedBy".ToLower()] = new XIIAttribute { sName = "UpdatedBy", sValue = iUserID.ToString(), bDirty = true };
                    oBOI.Attributes["UpdatedTime".ToLower()] = new XIIAttribute { sName = "UpdatedTime", sValue = DateTime.Now.ToString(), bDirty = true };
                    oBOI.Attributes["UpdatedBySYSID".ToLower()] = new XIIAttribute { sName = "UpdatedBySYSID", sValue = Dns.GetHostAddresses(Dns.GetHostName())[1].ToString(), bDirty = true };
                }
                oCR = oBOI.SaveV2(oBOI);
                oTrace.oTrace.Add(oCR.oTrace);
                if (oCR.bOK && oCR.oResult != null)
                {
                    if (oSigCR != null)
                    {
                        oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "URL insertion completed for Application - " + sAPPName });
                    }
                    oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                    oCResult.oResult = oBOI;
                }
                else
                {
                    oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "URL insertion failed for Application - " + sAPPName });
                }
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        #endregion URL Mapping

        #region Data Source

        public CResult Save_DataSource(List<CNV> oParams)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            try
            {
                var sDBType = oParams.Where(m => m.sName.ToLower() == XIConstant.DB_Type.ToLower()).Select(m => m.sValue).FirstOrDefault();
                string sDBName = string.Empty;
                if (!string.IsNullOrEmpty(sDBType))
                {
                    if (sDBType.ToLower() == XIConstant.DB_Core.ToLower())
                    {
                        sDBName = sAppName + "_" + XIConstant.DB_Core;
                    }
                    else if (sDBType.ToLower() == XIConstant.DB_Shared.ToLower())
                    {
                        sDBName = sAppName + "_" + XIConstant.DB_Shared;
                    }
                    else if (sDBType.ToLower() == XIConstant.DB_Nanno.ToLower())
                    {
                        sDBName = sAppName + "_" + XIConstant.DB_Nanno;
                    }
                }
                else
                {
                    sDBName = sAppName;
                }
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "Datasource insertion started for database - " + sDBName });
                }
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Database name - " + sDBName });
                if (!string.IsNullOrEmpty(sDBName))
                {
                    XIIBO oBOI = new XIIBO();
                    XIDBO oBOD = new XIDBO();
                    oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XI DataSources");
                    oBOI.BOD = oBOD;
                    oBOI.SetAttribute("ID", "0");
                    oBOI.SetAttribute("sType", "Application");
                    oBOI.SetAttribute("sName", sDBName);
                    oBOI.SetAttribute("FKiOrgID", iOrgID.ToString());
                    oBOI.SetAttribute("sDescription", "Application Database");
                    oBOI.SetAttribute("FKiApplicationID", iAppID.ToString());
                    oBOI.SetAttribute("StatusTypeID", "10");
                    oBOI.SetAttribute("CreatedBy", iUserID.ToString());
                    oBOI.SetAttribute("CreatedTime", DateTime.Now.ToString());
                    oBOI.SetAttribute("CreatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                    oBOI.SetAttribute("UpdatedBy", iUserID.ToString());
                    oBOI.SetAttribute("UpdatedTime", DateTime.Now.ToString());
                    oBOI.SetAttribute("UpdatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                    oBOI.SetAttribute("sQueryType", "MSSQL");
                    oBOI.SetAttribute("sDataSourceType", "Database");
                    oBOI.SetAttribute("OrganisationID", iOrgID.ToString());
                    oCR = oBOI.SaveV2(oBOI);
                    oTrace.oTrace.Add(oCR.oTrace);
                    if (oCR.bOK && oCR.oResult != null)
                    {
                        if (oSigCR != null)
                        {
                            oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "Datasource insertion completed for database " + sDBName });
                        }
                        int iDataSourceID = 0;
                        var oDataSourceD = (XIIBO)oCR.oResult;
                        var sDataSourceID = oDataSourceD.Attributes.Where(m => m.Key.ToLower() == "id").Select(m => m.Value).Select(m => m.sValue).FirstOrDefault();
                        int.TryParse(sDataSourceID, out iDataSourceID);
                        if (iDataSourceID > 0)
                        {
                            if (!string.IsNullOrEmpty(sDBType))
                            {
                                if (sDBType.ToLower() == XIConstant.DB_Core.ToLower())
                                {
                                    iCoreDataSourceID = iDataSourceID;
                                }
                                else if (sDBType.ToLower() == XIConstant.DB_Shared.ToLower())
                                {
                                    iSharedDataSourceID = iDataSourceID;
                                }
                            }
                            else
                            {
                                iSharedDataSourceID = iDataSourceID;
                            }
                            var DBServer = System.Configuration.ConfigurationManager.AppSettings["DataBaseServer"];
                            var DBServerUN = System.Configuration.ConfigurationManager.AppSettings["DataBaseUser"];
                            var DBServerPWD = System.Configuration.ConfigurationManager.AppSettings["DataBasePassword"];
                            oBOI.SetAttribute("ID", iDataSourceID.ToString());
                            if (oSigCR != null)
                            {
                                oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "Connection string encryption started for database " + sDBName });
                            }
                            //var sConnString = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString).ToString();
                            //var sConnString = "Data Source=192.168.7.222;initial catalog=" + sAppName + "_Core" + ";User Id=cruser; Password=cruser; MultipleActiveResultSets=True"; // For 183
                            var sConnString = "Data Source=" + DBServer + ";initial catalog=" + sDBName + ";User Id=" + DBServerUN + "; Password=" + DBServerUN + "; MultipleActiveResultSets=True";
                            var sEncrypted = oEncry.EncryptData(sConnString, true, iDataSourceID.ToString());
                            oTrace.oParams.Add(new CNV { sName = "sConnString", sValue = sConnString });
                            oBOI.SetAttribute("sConnectionString", sEncrypted);
                            oCR = oBOI.SaveV2(oBOI);
                            oTrace.oTrace.Add(oCR.oTrace);
                            if (oCR.bOK && oCR.oResult != null)
                            {

                                if (oSigCR != null)
                                {
                                    oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "Encrypted database connection string is updated to database - " + sDBName });
                                }
                                oCR = Create_Database(oParams);
                                oTrace.oTrace.Add(oCR.oTrace);
                                if (oCR.bOK && oCR.oResult != null)
                                {
                                    oCResult.oResult = oDataSourceD;
                                    oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                                }
                                else
                                {
                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                    oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                                }
                            }
                            else
                            {
                                oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "Encrypted database connection string updation failed for database " + sDBName });
                                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - Encryted connection string is not updated to Datasource for Data Source=" + DBServer + ";initial catalog=" + sDBName + ";User Id=" + DBServerUN + "; Password=" + DBServerUN;
                                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                                oXIDef.SaveErrortoDB(oCResult);
                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                            }
                        }
                    }
                    else
                    {
                        oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Datasource insertion failed for database " + sDBName });
                        oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - Data not inserted into DataSource_T table for Database " + sDBName;
                        oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                        oXIDef.SaveErrortoDB(oCResult);
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                    }
                }
                else
                {
                    oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Datasource insertion failed for database " + sDBName });
                    oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - Database name is not resolved for application " + sAppName;
                    oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                    oXIDef.SaveErrortoDB(oCResult);
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                }
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        #endregion Data Source

        #region Create Database

        public CResult Create_Database(List<CNV> oParams)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Create Database";
            try
            {
                var sDBType = oParams.Where(m => m.sName.ToLower() == XIConstant.DB_Type.ToLower()).Select(m => m.sValue).FirstOrDefault();
                string sDatabase = string.Empty;
                if (!string.IsNullOrEmpty(sDBType))
                {
                    if (sDBType.ToLower() == XIConstant.DB_Core.ToLower())
                    {
                        sDatabase = sAppName + "_" + XIConstant.DB_Core;
                    }
                    else if (sDBType.ToLower() == XIConstant.DB_Shared.ToLower())
                    {
                        sDatabase = sAppName + "_" + XIConstant.DB_Shared;
                    }
                    else if (sDBType.ToLower() == XIConstant.DB_Nanno.ToLower())
                    {
                        sDatabase = sAppName + "_" + XIConstant.DB_Nanno;
                    }
                }
                else
                {
                    sDatabase = sAppName;
                }
                oTrace.oParams.Add(new CNV { sName = "sDatabase", sValue = sDatabase });
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "Database creation started for " + sDatabase });
                }
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Database creation started" });
                using (SqlConnection DBCon = new SqlConnection(ConfigurationManager.ConnectionStrings["XIDNAAdminContext"].ConnectionString))
                {
                    DBCon.Open();
                    SqlCommand dbcmd = new SqlCommand();
                    dbcmd.Connection = DBCon;
                    string CreateDatabase = " CREATE DATABASE " + sDatabase;
                    dbcmd.CommandText = CreateDatabase;
                    dbcmd.ExecuteNonQuery();
                    DBCon.Close();
                }
                //Create User For Database
                Server sqlInstance = new Server(
     new Microsoft.SqlServer.Management.Common.ServerConnection(
     new System.Data.SqlClient.SqlConnection(ConfigurationManager.ConnectionStrings["XIDNAAdminContext"].ConnectionString))); //connects to the local server
                                                                                                                              //initialize new object from database  adventureworks
                Database database = sqlInstance.Databases[sDatabase];
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "Database creation completed for " + sDatabase });
                }
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Database Created" });
                //creates user RamyMahrous on database adventureworks
                string DataBaseUser = ConfigurationManager.AppSettings["DataBaseUser"];
                if (DataBaseUser != null)
                {
                    oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Database user creation started" });
                    oCR = Create_DatabaseUser(database, DataBaseUser, sDatabase);
                    oTrace.oTrace.Add(oCR.oTrace);
                    if (oCR.bOK && oCR.oResult != null)
                    {
                        oCResult.oTraceStack.Add(new CNV { sName = "Success", sValue = "Database user creation completed" });
                        if (sDBType != null && sDBType.ToLower() == XIConstant.DB_Core.ToLower())
                        {
                            oCR = Create_CoreDBTables();
                            oTrace.oTrace.Add(oCR.oTrace);
                            if (oCR.bOK && oCR.oResult != null)
                            {
                                oCResult.oResult = "Success";
                                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                            }
                            else
                            {
                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                            }
                        }
                        else
                        {
                            oCResult.oResult = "Success";
                            oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                        }
                    }
                    else
                    {
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                        oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Database user creation failed" });
                    }
                }
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        private CResult Create_DatabaseUser(Database database, String username, string sDatabase)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Create Database User";
            try
            {
                oTrace.oParams.Add(new CNV { sName = "sDatabase", sValue = sDatabase });
                oTrace.oParams.Add(new CNV { sName = "username", sValue = username });
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "Database user creation started for " + sDatabase });
                }
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Database user creation started - " + username });
                // initializes new User object and we say to which database it belongs
                // and its name
                Microsoft.SqlServer.Management.Smo.User sqlServerUser = new Microsoft.SqlServer.Management.Smo.User(database, username);
                sqlServerUser.UserType = UserType.SqlLogin; //SqlLogin not anything else
                                                            //associated the user to login name, login name should be valid login name
                string DataBaseUser = ConfigurationManager.AppSettings["DataBaseUser"];
                sqlServerUser.Login = DataBaseUser;
                // here's we create the user on the database and till now the user
                // don't have any permission on database objects
                sqlServerUser.Create();
                //or any role like db_databasereader, db_databasewriter,...
                sqlServerUser.AddToRole("db_owner");
                oCResult.oResult = "Success";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "Database user creation completed for " + sDatabase });
                }
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        #endregion  Create Database

        #region Core Tables

        public CResult Create_CoreDBTables()
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Create XIAPPUsers_AU_T, XIAppRoles_AR_T, XIAppUserRoles_AUR_T Tables in Core Database";
            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "User tables creation started" });
                }
                var XIAppUsers = @" IF (OBJECT_ID('XIAPPUsers_AU_T', 'U') IS NULL) BEGIN CREATE TABLE[dbo].[XIAPPUsers_AU_T]([UserID][int] IDENTITY(1, 1) NOT NULL,
    [FKiApplicationID] [int] NULL,	[FKiOrganisationID] [int] NULL,[sUserName][varchar](128) NULL,[sPasswordHash][varchar](256) NULL,
	[sDatabaseName] [varchar](32) NULL,[sCoreDatabaseName] [varchar](32) NULL,[sAppName] [varchar](32) NULL,
	[sLocation] [varchar](256) NULL,[sPhoneNumber] [varchar](32) NULL,	[sEmail] [varchar](128) NULL,[sFirstName]
        [nvarchar](max) NULL,[sLastName] [nvarchar](max) NULL,[sCol0][nvarchar](128) NULL,[sRow1][nvarchar](128) NULL,
	[sRow2][nvarchar](128) NULL,[sRow3][nvarchar](128) NULL,[sRow4][nvarchar](128) NULL,[sRow5][nvarchar](128) NULL,
	[iReportTo][int] NULL,[iPaginationCount][int] NULL,[sMenu][nvarchar](max) NULL,[iInboxRefreshTime][int] NULL,
	[SecurityStamp] [nvarchar](max) NULL,[PhoneNumberConfirmed][bit] NULL,[TwoFactorEnabled][bit] NULL,[LockoutEndDateUtc]
        [datetime] NULL,[LockoutEnabled]  [bit] NULL,[AccessFailedCount] [int] NULL,[EmailConfirmed] [bit] NULL,[StatusTypeID]  [int] NOT NULL Default 10,[CreatedBy] [int] NULL,[CreatedTime] [datetime]        NULL,	[CreatedBySYSID]        [varchar](512) NULL,
	[UpdatedBy] [int] NULL,[UpdatedTime] [datetime] NULL,[UpdatedBySYSID] [varchar](512) NULL,
	[sTemporaryPasswordHash] [varchar](256) NULL,[sAccessCode] [varchar](32) NULL,[dtLastLogin]
        [datetime]  NULL,["+XIConstant.Key_XICrtdBy+"] [varchar](15) NULL,["+XIConstant.Key_XICrtdWhn+"] [datetime] NULL,["+XIConstant.Key_XIUpdtdBy+ "] [varchar](15) NULL,[" + XIConstant.Key_XIUpdtdWhn + "] [datetime] NULL,[" + XIConstant.Key_XIDeleted+ "][int] NULL,[sHierarchy] [varchar](256) NULL,[XIGUID] [uniqueidentifier] NOT NULL DEFAULT newid(),[sInsertDefaultCode][varchar](64) NULL,[sUpdateHierarchy][varchar](1024) NULL,[sViewHierarchy] [varchar](1024) NULL,[sDeleteHierarchy][varchar](1024) NULL,[sOTP][varchar](15), [iLevel] [int] NULL, [FKiTeamID] [int] NULL) END";

                var XIAppRoles = @" IF (OBJECT_ID('XIAppRoles_AR_T', 'U') IS NULL) BEGIN CREATE TABLE[dbo].[XIAppRoles_AR_T]([RoleID][int] IDENTITY(1, 1) NOT NULL, [iParentID] [int] NOT NULL,
    [sRoleName] [nvarchar](max) NOT NULL,[FKiOrganizationID] [int] NOT NULL,[iLayoutID] [int] NULL,	[iThemeID]  [int] NULL,	[bDBAccess] [bit] NULL,
    [StatusTypeID] [int] NULL DEFAULT 10, [" + XIConstant.Key_XICrtdBy + "] [varchar](15) NULL,[" + XIConstant.Key_XICrtdWhn + "] [datetime] NULL, [" + XIConstant.Key_XIUpdtdBy + "] [varchar](15) NULL,[" + XIConstant.Key_XIUpdtdWhn + "] [datetime] NULL,[" + XIConstant.Key_XIDeleted + "][int] NULL,[XIGUID] [uniqueidentifier] NOT NULL DEFAULT newid(),[bSignalR] [bit] NOT NULL DEFAULT 0) END";

                var XIAppUserRoles = @" IF (OBJECT_ID('XIAppUserRoles_AUR_T', 'U') IS NULL) BEGIN CREATE TABLE[dbo].[XIAppUserRoles_AUR_T]([ID] [int] IDENTITY(1,1) NOT NULL,[UserID] [int] NULL,[RoleID][int] NULL, [" + XIConstant.Key_XICrtdBy + "] [varchar](15) NULL,[" + XIConstant.Key_XICrtdWhn + "] [datetime] NULL, [" + XIConstant.Key_XIUpdtdBy + "] [varchar](15) NULL,[" + XIConstant.Key_XIUpdtdWhn + "] [datetime] NULL,[" + XIConstant.Key_XIDeleted + "][int] NULL,[XIGUID] [uniqueidentifier] NOT NULL DEFAULT newid()) END";
                var Organisation = @" IF (OBJECT_ID('Organizations', 'U') IS NULL) BEGIN CREATE TABLE [dbo].[Organizations](	[ID] [int] IDENTITY(1,1) NOT NULL,	[Name] [varchar](128) NOT NULL,	[TypeID] [int] NOT NULL,	[Description] [varchar](1024) NULL,	[Logo] [varchar](1024) NULL,	[NoOfUsers] [int] NOT NULL,	[Address] [varchar](1024) NULL,	[PhoneNumber] [varchar](64) NULL,	[Email] [varchar](128) NOT NULL,	[PostCode] [varchar](128) NULL,	[Website] [varchar](1024) NULL,	[DatabaseName] [varchar](32) NULL,	[StatusTypeID] [int] NULL,	[CreatedByID] [int] NULL,	[CreatedByName] [varchar](32) NULL,	[CreatedBySYSID] [varchar](32) NULL,	[CreatedTime] [datetime] NULL,	[ModifiedByID] [int] NULL,	[ModifiedByName] [varchar](32) NULL,	[ModifiedTime] [datetime] NULL,	[Password] [nvarchar](max) NULL,	[DatabaseType] [varchar](50) NULL,	[ThemeID] [int] NULL,	[" + XIConstant.Key_XIDeleted + "] [int] NULL,	[" + XIConstant.Key_XICrtdBy + "] [varchar](32) NULL,	[" + XIConstant.Key_XICrtdWhn + "] [datetime] NULL,	[" + XIConstant.Key_XIUpdtdBy + "] [varchar](32) NULL,	[" + XIConstant.Key_XIUpdtdWhn + "] [datetime] NULL,	[FKiApplicationID] [int] NOT NULL,	[bNannoApp] [bit] NULL,[XIGUID] [uniqueidentifier] NOT NULL DEFAULT newid()) END";
                var BOWhiteList = @" IF (OBJECT_ID('XIBOWhiteList_T', 'U') IS NULL) BEGIN CREATE TABLE [dbo].[XIBOWhiteList_T](	[ID] [int] IDENTITY(1,1) NOT NULL, 	[sName] [varchar](256) NULL, [sCode] [varchar](256) NULL,	[iTypeID] [int] NULL, [iStatus] [int] NULL,	[sDescription] [varchar](256) NULL,	[FKiRoleID] [int] NULL, [bUNAuthorize] [bit] NULL, [FKiBODID] [int] NULL,  [bCreate] [bit] NULL, [bRead] [bit] NULL, [bUpdate] [bit] NULL, [bDelete] [bit] NULL, [bAction] [bit] NULL, [b1Query] [bit] NULL, [FKiOrgID] [int] NULL, [FKiAppID] [int] NULL,	[" + XIConstant.Key_XIDeleted + "] [int] NULL,	[" + XIConstant.Key_XICrtdBy + "] [varchar](32) NULL,	[" + XIConstant.Key_XICrtdWhn + "] [datetime] NULL,	[" + XIConstant.Key_XIUpdtdBy + "] [varchar](32) NULL,	[" + XIConstant.Key_XIUpdtdWhn + "] [datetime] NULL,	[sHierarchy] [varchar](256) NULL, [XIGUID] [uniqueidentifier] NOT NULL DEFAULT newid()) END";

                string sBODataSource = oXID.GetBODataSource(iCoreDataSourceID, iAppID, "IDE");
                XIDBAPI Connection = new XIDBAPI(sBODataSource);
                Connection.ExecuteQuery(XIAppUsers);
                Connection.ExecuteQuery(XIAppRoles);
                Connection.ExecuteQuery(XIAppUserRoles);
                Connection.ExecuteQuery(Organisation);
                Connection.ExecuteQuery(BOWhiteList);
                oCResult.oResult = "Success";
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "User tables creation completed" });
                }
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        public CResult Create_SharedDBTables()
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";
            try
            {
                //var ShareDB = @" IF (OBJECT_ID(" + sTableName + ", 'U') IS NULL) BEGIN CREATE TABLE[dbo].[" + sTableName + "]([ID][int] IDENTITY(1, 1) NOT NULL, [sName] [nvarchar](max) NOT NULL,[sDescription] [nvarchar](max) NOT NULL,[iStatus] [int] NULL, ["+XIConstant.Key_XICrtdBy+"] [varchar](15) NULL,["+XIConstant.Key_XICrtdWhn+"] [datetime] NULL, ["+XIConstant.Key_XIUpdtdBy+"] [varchar](15) NULL,[" + XIConstant.Key_XIUpdtdWhn + "] [datetime] NULL,["+XIConstant.Key_XIDeleted+"][int] NULL) END";
                //var oBODef = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, null, iBOID.ToString());
                //string sBODataSource = oXID.GetBODataSource(iSharedDataSourceID);
                //XIDBAPI Connection = new XIDBAPI(sBODataSource);
                //Connection.ExecuteQuery(ShareDB);
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Creating Shared Table" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            return oCResult;
        }

        #endregion Core Tables

        #region User

        public CResult Save_ApplicationUser(XIDApplication oApp, string sRoleName)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Create Application User";
            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "Application user creation started" });
                }
                XIIBO oBOI = new XIIBO();
                XIIBO oRoleI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIAPPUsers_AU_T");
                oBOI.BOD = oBOD;
                oBOD.iDataSource = iCoreDataSourceID;
                XIInfraUsers oUser = new XIInfraUsers();
                oUser.FKiApplicationID = oApp.ID;
                oUser.FKiOrganisationID = oApp.FKiOrganisationID;
                oUser.sUserName = oApp.sUserName;
                oUser.sPassword = oApp.XIAppPassword;
                oUser.sEmail = oApp.sUserName;
                oUser.sCoreDatabaseName = oApp.sDatabaseName;
                oUser.sDatabaseName = oApp.sDescription;
                oUser.sFirstName = oApp.sApplicationName;
                oUser.sLastName = oApp.sApplicationName;
                oUser.LockoutEndDateUtc = DateTime.Now;
                oCR = Save_User(oUser, iCoreDataSourceID);
                oTrace.oTrace.Add(oCR.oTrace);
                if (oCR.bOK && oCR.oResult != null)
                {
                    int iUserID = 0;
                    oBOI = (XIIBO)oCR.oResult;
                    var sUserID = oBOI.Attributes.Where(m => m.Key.ToLower() == "userid").Select(s => s.Value).Select(m => m.sValue).FirstOrDefault();
                    int.TryParse(sUserID, out iUserID);
                    if (iUserID > 0)
                    {
                        XIInfraRoles oRole = new XIInfraRoles();
                        oRole.iParentID = 2;
                        //oRole.sRoleName = "Admin";
                        oRole.sRoleName = sRoleName;
                        oRole.iThemeID = Convert.ToInt32(XIConstant.DefaultThemeID);
                        if (!string.IsNullOrEmpty(sRoleName) && sRoleName.ToLower() == "orgide")
                        {
                            oRole.iLayoutID = 2475;
                            oRole.iParentID = 0;
                        }
                        else if (sRoleName.ToLower() == "orgnanno")
                        {
                            oRole.iLayoutID = 6675;
                            oRole.iThemeID = 162;
                        }
                        else if (sRoleName.ToLower() == "orgadmin")
                        {
                            oRole.iLayoutID = 8377;
                        }
                        oRole.FKiOrganizationID = oApp.FKiOrganisationID;
                        oCR = Save_Role(oRole, iCoreDataSourceID);
                        oTrace.oTrace.Add(oCR.oTrace);
                        if (oCR.bOK && oCR.oResult != null)
                        {
                            int iRoleID = 0;
                            oRoleI = (XIIBO)oCR.oResult;
                            var sRoleID = oRoleI.Attributes.Where(m => m.Key.ToLower() == "roleid").Select(s => s.Value).Select(m => m.sValue).FirstOrDefault();
                            int.TryParse(sRoleID, out iRoleID);
                            if (iRoleID > 0)
                            {
                                XIInfraUserRoles oUserRole = new XIInfraUserRoles();
                                oUserRole.RoleID = iRoleID;
                                oUserRole.UserID = iUserID;
                                oCR = Save_UserRole(oUserRole, iCoreDataSourceID);
                                oTrace.oTrace.Add(oCR.oTrace);
                                if (oCR.bOK && oCR.oResult != null)
                                {
                                    oCResult.oResult = oRoleI;
                                    oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                                }
                                else
                                {
                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                }
                            }
                        }
                        else
                        {
                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                        }
                    }
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                    oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Application user creation failed" });
                }
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        public CResult Save_User(XIInfraUsers oUser, int iDataSourceID)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Create User";

            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "User insertion started" });
                }
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIAPPUsers_AU_T");
                oBOI.BOD = oBOD;
                oBOD.iDataSource = iDataSourceID;
                oBOI.SetAttribute("UserID", oUser.UserID.ToString());
                oBOI.SetAttribute("FKiApplicationID", oUser.FKiApplicationID.ToString());
                oBOI.SetAttribute("sUserName", oUser.sUserName);
                oBOI.SetAttribute("sDatabaseName", oUser.sDatabaseName);
                oBOI.SetAttribute("sAppName", oUser.sAppName);
                oBOI.SetAttribute("LockoutEndDateUtc", DateTime.Now.ToString());
                oBOI.SetAttribute("sCoreDatabaseName", oUser.sCoreDatabaseName);
                oBOI.SetAttribute("FKiOrganisationID", oUser.FKiOrganisationID.ToString());
                oBOI.SetAttribute("LockoutEnabled", "0");
                oBOI.SetAttribute("iReportTo", "0");
                oBOI.SetAttribute("sEmail", oUser.sEmail);
                oBOI.SetAttribute("iPaginationCount", "0");
                oBOI.SetAttribute("sPhoneNumber", oUser.sPhoneNumber);
                oBOI.SetAttribute("iInboxRefreshTime", "0");
                oBOI.SetAttribute("AccessFailedCount", "0"); oBOI.SetAttribute("EmailConfirmed", "0");
                oBOI.SetAttribute("LockoutEndDateUtc", DateTime.Now.ToString());
                oBOI.SetAttribute("TwoFactorEnabled", "0"); oBOI.SetAttribute("TwoFactorEnabled", "0");
                oBOI.SetAttribute("sFirstName", oUser.sFirstName);
                oBOI.SetAttribute("sLastName", oUser.sLastName);
                oBOI.SetAttribute("PhoneNumberConfirmed", "0");
                oBOI.SetAttribute("sOTP", oUser.sOTP);
                oCR = oBOI.SaveV2(oBOI);
                oTrace.oTrace.Add(oCR.oTrace);
                if (oCR.bOK && oCR.oResult != null)
                {
                    int iUserID = 0;
                    if (oSigCR != null)
                    {
                        oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "User insertion completed" });
                    }
                    oBOI = (XIIBO)oCR.oResult;
                    var sUserID = oBOI.Attributes.Where(m => m.Key.ToLower() == "userid").Select(s => s.Value).Select(m => m.sValue).FirstOrDefault();
                    int.TryParse(sUserID, out iUserID);
                    if (iUserID > 0)
                    {
                        var EncryPass = oEncry.EncryptData(oUser.sPassword, true, iUserID.ToString());
                        oBOI.SetAttribute("sPasswordHash", EncryPass);
                        oCR = oBOI.SaveV2(oBOI);
                        oTrace.oTrace.Add(oCR.oTrace);
                        if (oCR.bOK && oCR.oResult != null)
                        {
                            oCResult.oResult = oBOI;
                            oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                        }
                        else
                        {
                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                            oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Password Encryption failed" });
                        }
                    }
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                    oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "User insertion failed" });
                }
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        public CResult Save_Role(XIInfraRoles oRole, int iDataSourceID)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Create Role";

            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "Role insertion started" });
                }
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIAppRoles_AR_T");
                oBOI.BOD = oBOD;
                oBOD.iDataSource = iDataSourceID;
                oBOI.SetAttribute("iParentID", oRole.iParentID.ToString());
                oBOI.SetAttribute("sRoleName", oRole.sRoleName);
                oBOI.SetAttribute("FKiOrganizationID", oRole.FKiOrganizationID.ToString());
                oBOI.SetAttribute("iLayoutID", oRole.iLayoutID.ToString());
                oBOI.SetAttribute("iThemeID", oRole.iThemeID.ToString());
                oBOI.SetAttribute("StatusTypeID", "10");
                oBOI.SetAttribute("bDBAccess", oRole.bDBAccess.ToString());
                //oBOI.SetAttribute("CreatedBy", iUserID.ToString());
                //oBOI.SetAttribute("CreatedTime", DateTime.Now.ToString());
                //oBOI.SetAttribute("CreatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                //oBOI.SetAttribute("UpdatedBy", iUserID.ToString());
                //oBOI.SetAttribute("UpdatedTime", DateTime.Now.ToString());
                //oBOI.SetAttribute("UpdatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                oCR = oBOI.SaveV2(oBOI);
                oTrace.oTrace.Add(oCR.oTrace);
                if (oCR.bOK && oCR.oResult != null)
                {
                    if (oSigCR != null)
                    {
                        oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "Role insertion completed" });
                    }
                    oBOI = (XIIBO)oCR.oResult;
                    oCResult.oResult = oBOI;
                    oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                }
                else
                {
                    oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Role insertion failed" });
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                }

            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        public CResult Save_UserRole(XIInfraUserRoles oUserRole, int iDataSourceID)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Save User Role";
            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "User Role insertion started" });
                }
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIAppUserRoles_AUR_T");
                oBOI.BOD = oBOD;
                oBOD.iDataSource = iDataSourceID;
                oBOI.SetAttribute("UserID", oUserRole.UserID.ToString());
                oBOI.SetAttribute("RoleID", oUserRole.RoleID.ToString());
                oCR = oBOI.SaveV2(oBOI);
                oTrace.oTrace.Add(oCR.oTrace);
                if (oCR.bOK && oCR.oResult != null)
                {
                    oBOI = (XIIBO)oCR.oResult;
                    oCResult.oResult = oBOI;
                    if (oSigCR != null)
                    {
                        oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "User Role insertion completed" });
                    }
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                    oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                }
                else
                {
                    oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "User Role insertion failed" });
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                }

            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        #endregion Create_AppUsers

        #region Organisation

        public CResult Save_Organisation(XIDOrganisation model)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Create Organisation";
            try
            {
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "Organisations");
                oBOI.BOD = oBOD;
                if (model.FKiApplicationID > 0)
                {
                    var oAPPD = (XIDApplication)oCache.GetObjectFromCache(XIConstant.CacheApplication, null, model.FKiApplicationID.ToString());
                    string sAppName = oAPPD.sApplicationName;
                    if (sAppName.Length > 16)
                    {
                        sAppName = sAppName.Substring(0, 15);
                    }
                    sAppName = sAppName.Replace(" ", "");
                    var sCoreDB = sAppName + "_core";
                    var DataSource = (XIDataSource)oCache.GetObjectFromCache(XIConstant.CacheDataSource, sCoreDB);
                    oBOD.iDataSource = DataSource.ID;
                }
                oBOI.SetAttribute("ID", model.ID.ToString());
                oBOI.SetAttribute("FKiApplicationID", model.FKiApplicationID.ToString());
                oBOI.SetAttribute("Name", model.Name + "_Org");
                oBOI.SetAttribute("TypeID", "1");
                oBOI.SetAttribute("Description", model.Description + "_Org");
                oBOI.SetAttribute("Logo", model.Logo);
                oBOI.SetAttribute("NoOfUsers", "1000");
                oBOI.SetAttribute("Address", "");
                oBOI.SetAttribute("PhoneNumber", "");
                oBOI.SetAttribute("Email", model.Email);
                oBOI.SetAttribute("PostCode", "");
                oBOI.SetAttribute("Website", "");
                oBOI.SetAttribute("bNannoApp", model.bNannoApp.ToString());
                oBOI.SetAttribute("DatabaseName", model.Name + "_Shared");
                if (model.bNannoApp)
                {
                    oBOI.SetAttribute("DatabaseName", model.Name + "_Nanno");
                }
                oBOI.SetAttribute("StatusTypeID", "10");
                oBOI.SetAttribute("CreatedByID", iUserID.ToString()); oBOI.SetAttribute("CreatedByName", iUserID.ToString()); oBOI.SetAttribute("CreatedTime", DateTime.Now.ToString()); oBOI.SetAttribute("CreatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                oBOI.SetAttribute("ModifiedByID", iUserID.ToString()); oBOI.SetAttribute("ModifiedByName", iUserID.ToString()); oBOI.SetAttribute("ModifiedTime", DateTime.Now.ToString());
                oBOI.SetAttribute("Password", model.Password);
                oBOI.SetAttribute("DatabaseType", model.DatabaseType);
                oBOI.SetAttribute("ThemeID", model.ThemeID.ToString());
                oCR = oBOI.SaveV2(oBOI);
                oTrace.oTrace.Add(oCR.oTrace);
                if (oCR.bOK && oCR.oResult != null)
                {
                    oBOI = (XIIBO)oCR.oResult;
                    oCResult.oResult = oBOI;
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                    oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                }
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }


        public CResult Build_Organisation(List<CNV> oParams)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Build organisation, create user for org login";
            try
            {
                var oAppD = new XIDApplication();
                var oOrgD = new XIDOrganisation();
                var sOrgID = oParams.Where(m => m.sName.ToLower() == XIConstant.Param_InstanceID.ToLower()).Select(m => m.sValue).FirstOrDefault();
                int iOrgID = 0;
                int.TryParse(sOrgID, out iOrgID);
                if (iOrgID > 0)
                {
                    var AppID = oParams.Where(m => m.sName.ToLower() == XIConstant.Param_ApplicationID.ToLower()).Select(m => m.sValue).FirstOrDefault();
                    var iAppID = 0;
                    int.TryParse(AppID, out iAppID);
                    if(iAppID > 0)
                    {
                        oAppD= (XIDApplication)oCache.GetObjectFromCache(XIConstant.CacheApplication, null, iAppID.ToString());
                        if(oAppD.ID > 0)
                        {
                            var CoreDB = oAppD.sDatabaseName;
                            XIDXI oXID = new XIDXI();
                            oXID.sOrgDatabase = CoreDB;
                            oCR= oXID.Get_OrgDefinition(null, iOrgID.ToString());
                            oTrace.oTrace.Add(oCR.oTrace);
                            if (oCR.bOK && oCR.oResult != null)
                            {
                                oOrgD = (XIDOrganisation)oCR.oResult;

                            }
                            else
                            {

                            }
                        }
                    }
                    //XIIXI oXI = new XIIXI();
                    //var oBOI = oXI.BOI("Organisations", iOrgID.ToString());
                    if (oAppD != null && oOrgD !=null)
                    {
                        //var sAppName = string.Empty;
                        var iTheme = string.Empty;
                        var sCoreDB = string.Empty;
                        var iAPPID = oOrgD.FKiApplicationID;
                        var sOrgName = oOrgD.Name;
                        var sOrgEmail = oOrgD.Email;
                        var sPassword = oOrgD.Password;
                        var sDBType = oOrgD.DatabaseType;
                        var sOrgDB = oOrgD.DatabaseName;
                        var bNannoApp = false;
                        var sNannoApp = oOrgD.bNannoApp.ToString();
                        bool.TryParse(sNannoApp, out bNannoApp);
                        //var oAppI = oXI.BOI("XI Application", iAPPID);
                        if (oAppD != null)
                        {
                            sAppName = oAppD.sApplicationName;
                            sAppName = sAppName.Replace(" ", "");
                            iTheme = oAppD.sTheme;
                            sCoreDB = oAppD.sDatabaseName;
                        }
                        XIInfraCache oCache = new XIInfraCache();
                        if (bNannoApp)
                        {
                            var DataSource = (XIDataSource)oCache.GetObjectFromCache(XIConstant.CacheDataSource, "Nanno_Core");
                            iCoreDataSourceID = DataSource.ID;
                            iTheme = "162";
                        }
                        else
                        {
                            var DataSource = (XIDataSource)oCache.GetObjectFromCache(XIConstant.CacheDataSource, sCoreDB);
                            iCoreDataSourceID = DataSource.ID;
                        }

                        oTrace.oParams.Add(new CNV { sName = "iOrgID", sValue = iOrgID.ToString() });
                        string sRoleName = "OrgIDE";
                        if (bNannoApp)
                        {
                            sRoleName = "OrgNanno";
                        }
                        //Save URL for Organisation
                        XIURLMappings oURL = new XIURLMappings();
                        oURL.FKiApplicationID = Convert.ToInt32(iAPPID);
                        oURL.OrganisationID = iOrgID;
                        oURL.sUrlName = sOrgName;
                        oURL.sActualUrl = sAppName;
                        oURL.sType = "Application";
                        oURL.StatusTypeID = 10;
                        oCR = Save_URLMapping(oURL);
                        oTrace.oTrace.Add(oCR.oTrace);
                        if (oCR.bOK && oCR.oResult != null)
                        {
                            XIDApplication oAPPD = new XIDApplication();
                            oAPPD.FKiOrganisationID = iOrgID;
                            oAPPD.ID = Convert.ToInt32(iAPPID);
                            oAPPD.sUserName = "CS" + sOrgEmail;
                            if (bNannoApp)
                            {
                                oAPPD.sUserName = "OrgNanno" + sOrgEmail;
                            }
                            oAPPD.XIAppPassword = sPassword;
                            oAPPD.sApplicationName = sAppName;
                            oAPPD.sDatabaseName = sCoreDB;
                            oAPPD.sDescription = sOrgDB;
                            oCR = Save_ApplicationUser(oAPPD, sRoleName);
                            oTrace.oTrace.Add(oCR.oTrace);
                            if (oCR.bOK && oCR.oResult != null)
                            {
                                sRoleName = "OrgAdmin";
                                oAPPD.sUserName = "DS" + sOrgEmail;
                                oCR = Save_ApplicationUser(oAPPD, sRoleName);
                                oTrace.oTrace.Add(oCR.oTrace);
                                if (oCR.bOK && oCR.oResult != null)
                                {
                                    sRoleName = "OrgUser";
                                    oAPPD.sUserName = sOrgEmail;
                                    oCR = Save_ApplicationUser(oAPPD, sRoleName);
                                    oTrace.oTrace.Add(oCR.oTrace);
                                    if (oCR.bOK && oCR.oResult != null)
                                    {
                                        var oRoleI = (XIIBO)oCR.oResult;
                                        var iRoleID = oRoleI.AttributeI("roleid").sValue;
                                        List<CNV> mParams = new List<CNV>();
                                        mParams.Add(new CNV { sName = XIConstant.Param_Layout, sValue = XIConstant.Auto3LayoutHTML });
                                        XIDLayout oLayout = new XIDLayout();
                                        oLayout.LayoutName = "AppDefaultLayout";
                                        oLayout.LayoutType = "Inline";
                                        oLayout.LayoutCode = XIConstant.Auto3LayoutHTML;
                                        oLayout.FKiApplicationID = Convert.ToInt32(iAPPID);
                                        oLayout.Authentication = "Authenticated";
                                        oLayout.LayoutLevel = "OrganisationLevel";
                                        oLayout.StatusTypeID = 10;
                                        oLayout.bIsTaskBar = true;
                                        oLayout.sTaskBarPosition = "Left";
                                        oLayout.OrganizationID = iOrgID;
                                        string sMenu = sOrgName + "OrgMenu";
                                        oCR = Save_ApplicationDefaultLayout(oLayout, sMenu);
                                        oTrace.oTrace.Add(oCR.oTrace);
                                        if (oCR.bOK && oCR.oResult != null)
                                        {

                                            var oLayoutDef = (XIIBO)oCR.oResult;
                                            var iLayoutID = oLayoutDef.Attributes.Where(m => m.Key.ToLower() == "id").Select(m => m.Value).Select(m => m.sValue).FirstOrDefault();
                                            XIIBO oBOI1 = new XIIBO();
                                            XIDBO oBOD1 = new XIDBO();
                                            oBOD1 = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIAppRoles_AR_T");
                                            oBOI1.BOD = oBOD1;
                                            oBOD1.iDataSource = iCoreDataSourceID;
                                            oBOI1.SetAttribute("RoleID", iRoleID.ToString());
                                            oBOI1.SetAttribute("iLayoutID", iLayoutID.ToString());
                                            oBOI1.SetAttribute("iThemeID", iTheme);
                                            oCR = oBOI1.SaveV2(oBOI1);
                                            if (oCR.bOK && oCR.oResult != null)
                                            {
                                                XIMenu oMenu = new XIMenu();
                                                oMenu.FKiApplicationID = Convert.ToInt32(iAPPID);
                                                oMenu.OrgID = iOrgID;
                                                oMenu.Name = sOrgName + "OrgMenu";
                                                oMenu.RootName = sOrgName + "OrgMenu";
                                                oMenu.ParentID = "#";
                                                oMenu.ActionType = 0;
                                                oMenu.StatusTypeID = 10;
                                                oMenu.XiLinkID = 0;
                                                oCR = Save_Menu(oMenu);
                                                oTrace.oTrace.Add(oCR.oTrace);
                                                if (oCR.bOK && oCR.oResult != null)
                                                {
                                                    var oMenuI = (XIIBO)oCR.oResult;
                                                    var iParentMenuID = oMenuI.Attributes.Where(m => m.Key.ToLower() == "id").Select(m => m.Value).Select(m => m.sValue).FirstOrDefault();
                                                }
                                                else
                                                {
                                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                }
                                                //oConfig.AutoMenuCreationForApplication();
                                            }
                                            else
                                            {
                                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                            }
                                        }
                                        else
                                        {
                                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                        }
                                    }
                                    else
                                    {
                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                    }
                                }
                                else
                                {
                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                }
                            }
                            else
                            {
                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                            }
                        }
                        else
                        {
                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                        }
                        if (oTrace.iStatus != (int)xiEnumSystem.xiFuncResult.xiError)
                        {
                            if (sDBType == "Specific")
                            {
                                XIInfraRoles oRole = new XIInfraRoles();
                                oRole.iParentID = 2;
                                oRole.sRoleName = "External";
                                //if (!string.IsNullOrEmpty(sRoleName) && sRoleName.ToLower() == "orgide")
                                //{
                                //    oRole.iLayoutID = 2475;
                                //}
                                oRole.iThemeID = Convert.ToInt32(XIConstant.DefaultThemeID);
                                oRole.FKiOrganizationID = iOrgID;
                                oCR = Save_Role(oRole, iCoreDataSourceID);
                                oTrace.oTrace.Add(oCR.oTrace);
                                if (oCR.bOK && oCR.oResult != null)
                                {

                                }
                                else
                                {
                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                }
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
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        #endregion Organisation

        #region BO 

        public CResult Save_BO(XIDBO model)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Save BO";
            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "BO insertion started for " + model.Name });
                }
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIBO");
                oBOI.BOD = oBOD;
                oBOI.SetAttribute("BOID", model.BOID.ToString());
                oBOI.SetAttribute("FKiApplicationID", iAppID.ToString());
                oBOI.SetAttribute("Name", model.Name);
                oBOI.SetAttribute("sNameAttribute", model.sNameAttribute);
                oBOI.SetAttribute("OrganizationID", iOrgID.ToString());
                oBOI.SetAttribute("TypeID", model.TypeID.ToString());
                oBOI.SetAttribute("FieldCount", "0");
                oBOI.SetAttribute("Description", model.Description);
                oBOI.SetAttribute("TableName", model.TableName);
                oBOI.SetAttribute("ClassName", null);
                oBOI.SetAttribute("IsClassEnabled", "False");
                oBOI.SetAttribute("sColumns", model.sColumns);
                oBOI.SetAttribute("sVersion", model.sVersion);
                oBOI.SetAttribute("sSize", model.sSize);
                oBOI.SetAttribute("sPrimaryKey", model.sPrimaryKey);
                oBOI.SetAttribute("sTimeStamped", null);
                oBOI.SetAttribute("sDeleteRule", "10");
                oBOI.SetAttribute("sSearchType", null);
                oBOI.SetAttribute("iDataSource", model.iDataSource.ToString());
                if (!string.IsNullOrEmpty(model.TableName) && model.iDataSource > 0)
                {
                    iSharedDataSourceID = model.iDataSource;
                }
                oBOI.SetAttribute("sNotes", model.sNotes);
                oBOI.SetAttribute("sHelpItem", "");
                oBOI.SetAttribute("sAudit", "No");
                oBOI.SetAttribute("bUID", null);
                oBOI.SetAttribute("iUpdateCount", "20");
                oBOI.SetAttribute("sSection", "20");
                oBOI.SetAttribute("LabelName", model.LabelName);
                oBOI.SetAttribute("StatusTypeID", "10");
                oBOI.SetAttribute("bIsAutoIncrement", model.bIsAutoIncrement.ToString());
                oBOI.SetAttribute("iTransactionEnable", "10");
                oBOI.SetAttribute("sAuditBOName", null);
                oBOI.SetAttribute("sUpdateVersion", GetVersionForBO("1"));
                oBOI.SetAttribute("sType", model.sType);
                if (model.BOID == 0)
                {
                    oBOI.SetAttribute("CreatedBy", iUserID.ToString());
                    oBOI.SetAttribute("CreatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                    oBOI.SetAttribute("CreatedTime", DateTime.Now.ToString());
                }
                oBOI.SetAttribute("UpdatedBy", iUserID.ToString());
                oBOI.SetAttribute("UpdatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                oBOI.SetAttribute("UpdatedTime", DateTime.Now.ToString());
                oBOI.SetAttribute("bIsHierarchy", model.bIsHierarchy.ToString());
                oBOI.SetAttribute("bIsEncrypt", model.bIsEncrypt.ToString());
                oCR = oBOI.Save(oBOI);
                oTrace.oTrace.Add(oCR.oTrace);
                if (oCR.bOK && oCR.oResult != null)
                {
                    oCResult = oCR;
                    if (oSigCR != null)
                    {
                        oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "BO insertion completed for " + model.Name });
                    }
                    oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                    oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "BO insertion failed for " + model.Name });
                }
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        /// <summary>
        /// Save BO Default Configs
        /// </summary>
        public CResult Build_BO(List<CNV> oParams)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Save BO Defualts like Popup, Menu, 1Click, Groups etc";
            string sBO = oParams.Where(m => m.sName.ToLower() == XIConstant.Param_BO.ToLower()).Select(m => m.sValue).FirstOrDefault();
            try
            {
                int iOrgID = 0;
                string sBODID = oParams.Where(m => m.sName.ToLower() == XIConstant.Param_InstanceID.ToLower()).Select(m => m.sValue).FirstOrDefault();
                iBODID = 0;
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "BO insertion completed for " + sBO });
                }
                int.TryParse(sBODID, out iBODID);
                oTrace.oParams.Add(new CNV { sName = "iBODID", sValue = iBODID.ToString() });
                string sDashboardType = string.Empty;
                string sType = string.Empty;
                if (iBODID > 0)
                {
                    XIIBO oBOI = new XIIBO();
                    XIDBO oBOD = new XIDBO();
                    oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIBO");
                    if (oBOD != null && oBOD.Attributes.Count() > 0)
                    {
                        var sBOType = string.Empty;
                        XIDBO oBO = new XIDBO();
                        XIIXI oXI = new XIIXI();
                        oBOI = oXI.BOI("XIBO", iBODID.ToString());
                        var bDefaults = string.Empty;
                        if (oBOI != null && oBOI.Attributes.Count() > 0)
                        {
                            if (oBOI.Attributes.ContainsKey("Name"))
                            {
                                sBOName = oBOI.Attributes["Name"].sValue;
                                sLabelName = oBOI.Attributes["LabelName"].sValue;
                            }
                            if (oBOI.Attributes.ContainsKey("TableName"))
                            {
                                oBO.TableName = oBOI.Attributes["TableName"].sValue;
                                sTableName = oBOI.Attributes["TableName"].sValue;
                            }
                            if (oBOI.Attributes.ContainsKey("sColumns"))
                            {
                                oBO.sColumns = oBOI.Attributes["sColumns"].sValue;
                            }

                            if (oBOI.Attributes.ContainsKey("bIsEncrypt"))
                            {
                                oBO.bIsEncrypt = bool.Parse(oBOI.Attributes["bIsEncrypt"].sValue);
                            }
                            if (oBOI.Attributes.ContainsKey("FKiApplicationID"))
                            {
                                var sAppID = oBOI.Attributes["FKiApplicationID"].sValue;
                                int.TryParse(sAppID, out iAppID);
                            }
                            if (oBOI.Attributes.ContainsKey("sType"))
                            {
                                sBOType = oBOI.Attributes["sType"].sValue;
                                oBO.sType = sBOType;
                            }
                            if (oBOI.Attributes.ContainsKey("iDataSource"))
                            {
                                var sDataSource = oBOI.Attributes["iDataSource"].sValue;
                                int iDataSource = 0;
                                int.TryParse(sDataSource, out iDataSource);
                                oBO.iDataSource = iDataSource;
                            }
                            if (oBOI.Attributes.ContainsKey("bDefaults"))
                            {
                                bDefaults = oBOI.Attributes["bDefaults"].sValue;
                            }
                            if (oBOI.Attributes.ContainsKey("sType"))
                            {
                                sType = oBOI.Attributes["sType"].sValue;
                            }
                            if (oBOI.Attributes.ContainsKey("sDashBoardType"))
                            {
                                sDashboardType = oBOI.Attributes["sDashBoardType"].sValue;
                            }
                            if (oBOI.Attributes.ContainsKey("OrganizationID"))
                            {
                                var OrgID = oBOI.Attributes["OrganizationID"].sValue;
                                int.TryParse(OrgID, out iOrgID);
                            }
                            //if (iBODID > 0)
                            if (iBODID > 0 && (bDefaults == "False" || string.IsNullOrEmpty(bDefaults)))
                            {
                                oCR = Parse_BOAttributes(oBO);
                                oTrace.oTrace.Add(oCR.oTrace);
                                if (oCR.bOK && oCR.oResult != null)
                                {
                                    XIDGroup oGroup = new XIDGroup();
                                    oGroup.BOID = iBODID;
                                    oCR = Save_BOGroups(oGroup);
                                    oTrace.oTrace.Add(oCR.oTrace);
                                    if (oCR.bOK && oCR.oResult != null)
                                    {
                                        var iGrpID = (int)oCR.oResult;
                                        int iStructureID = 0;
                                        List<XIDStructure> oStru = new List<XIDStructure>();
                                        XIDStructure oStr = new XIDStructure();
                                        oStr.sSavingType = "Add";
                                        oStr.FKiParentID = "#";
                                        oStr.sStructureName = sBOName + " Conf";
                                        oStr.sCode = sBOName + " Conf";
                                        oStr.sName = "Details";
                                        oStr.BOID = iBODID;
                                        oStr.sMode = "Single";
                                        oStr.sType = "Main BO";
                                        oStr.StatusTypeID = 10;
                                        oStr.sLinkingType = "PtoC";
                                        oStr.FKiStepDefinitionID = "0";
                                        oStru.Add(oStr);
                                        oCR = Save_BOStructure(oStru);
                                        oTrace.oTrace.Add(oCR.oTrace);
                                        if (oCR.bOK && oCR.oResult != null)
                                        {
                                            var oStrID = (string)oCR.oResult;
                                            int.TryParse(oStrID, out iStructureID);

                                            int i1ClickID = 0;
                                            XID1Click o1Click = new XID1Click();
                                            o1Click.FromBos = sTableName;
                                            o1Click.Name = o1Click.Code = sLabelName; o1Click.Description = sBOName + " Default 1-Click";
                                            o1Click.BOID = iBODID;
                                            o1Click.DisplayAs = 50; o1Click.IsFilterSearch = true;
                                            o1Click.IsCreate = true;
                                            o1Click.IsRefresh = false;
                                            if (sBOType == "Reference")
                                            {
                                                o1Click.CategoryID = 5;
                                            }
                                            o1Click.sCreateType = "inlinetop";
                                            o1Click.FKiSearchGroup = iGrpID;
                                            o1Click.OrganizationID = iOrgID;
                                            oCR = Save_1Click(o1Click);
                                            oTrace.oTrace.Add(oCR.oTrace);
                                            if (oCR.bOK && oCR.oResult != null)
                                            {
                                                i1ClickID = (int)oCR.oResult;
                                                List<CNV> mParams = new List<CNV>();
                                                mParams.Add(new CNV { sName = XIConstant.Param_1ClickID, sValue = i1ClickID.ToString() });
                                                mParams.Add(new CNV { sName = XIConstant.Param_BO, sValue = sBOName });
                                                mParams.Add(new CNV { sName = XIConstant.Param_ApplicationID, sValue = iAppID.ToString() });
                                                mParams.Add(new CNV { sName = XIConstant.Param_BODID, sValue = iBODID.ToString() });
                                                mParams.Add(new CNV { sName = XIConstant.Param_OrgID, sValue = iOrgID.ToString() });
                                                mParams.Add(new CNV { sName = "BOLabel", sValue = sLabelName });
                                                oCR = Save_BOMenu(mParams);
                                                oTrace.oTrace.Add(oCR.oTrace);
                                                if (oCR.bOK && oCR.oResult != null)
                                                {
                                                    oCR = Save_BOPopup(mParams);
                                                    oTrace.oTrace.Add(oCR.oTrace);
                                                    //oCRes = Save_PopupQuestionSet(sLabelName, FKiAppID);
                                                    if (oCR.bOK && oCR.oResult != null)
                                                    {
                                                        var iQSID = (int)oCR.oResult;
                                                        //iQSID = Convert.ToInt32(oQSDef.Attributes.Where(m => m.Key.ToLower() == "id").Select(m => m.Value).Select(m => m.sValue).FirstOrDefault());
                                                        //Create BOUIDetails
                                                        XIDBOUI oBOUI = new XIDBOUI();
                                                        oBOUI.FKiBOID = iBODID;
                                                        oBOUI.FKiStructureID = iStructureID;
                                                        oBOUI.iPopupID = iRowXilinkID;
                                                        oCR = Save_BOUIDetails(oBOUI);
                                                        oTrace.oTrace.Add(oCR.oTrace);
                                                        if (oCR.bOK && oCR.oResult != null)
                                                        {
                                                            //Create BOUIDefault
                                                            XIDBODefault oDefault = new XIDBODefault();
                                                            oDefault.FKiBOID = iBODID;
                                                            oDefault.i1ClickID = i1ClickID;
                                                            oDefault.iPopupID = iRowXilinkID;
                                                            oDefault.iStructureID = iStructureID;
                                                            oCR = Save_BOUIDefault(oDefault);
                                                            oTrace.oTrace.Add(oCR.oTrace);
                                                            if (oCR.bOK && oCR.oResult != null)
                                                            {
                                                                var MasterBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "Master Data");
                                                                XIIBO oMasterBOI = new XIIBO();
                                                                oMasterBOI.BOD = MasterBOD;
                                                                oMasterBOI.SetAttribute("Name", "QuickSearchBos");
                                                                oMasterBOI.SetAttribute("Expression", sLabelName);
                                                                oMasterBOI.SetAttribute("FileName", sBOName);
                                                                oMasterBOI.SetAttribute("FKiApplicationID", iAppID.ToString());
                                                                oMasterBOI.SetAttribute("Status", "10");
                                                                oMasterBOI.SetAttribute("Value", i1ClickID.ToString());
                                                                oCR = oMasterBOI.SaveV2(oMasterBOI);
                                                                oTrace.oTrace.Add(oCR.oTrace);
                                                                if (oCR.bOK && oCR.oResult != null)
                                                                {

                                                                }
                                                                if (!string.IsNullOrEmpty(sType) && sType.ToLower() == "masterentity" && !string.IsNullOrEmpty(sDashboardType))
                                                                {
                                                                    List<CNV> oPrms = new List<CNV>();
                                                                    oPrms.Add(new CNV { sName = "LayoutName", sValue = sBOName + " Default BO Data Layout" });
                                                                    oPrms.Add(new CNV { sName = "DialogName", sValue = sBOName + " Default BO Data" });
                                                                    oPrms.Add(new CNV { sName = "XilinkName", sValue = sBOName + " Default BO Data Xilink" });
                                                                    //oParams.Add(new CNV { sName = "sParentID", sValue = "1346" }); //2705
                                                                    //oParams.Add(new CNV { sName = "sApplicationName", sValue = oUser.sDatabaseName });
                                                                    oPrms.Add(new CNV { sName = "irowxilinkid", sValue = iRowXilinkID.ToString() });
                                                                    oPrms.Add(new CNV { sName = XIConstant.Param_ApplicationID, sValue = iAppID.ToString() });
                                                                    oPrms.Add(new CNV { sName = "dialogname", sValue = "" });
                                                                    oPrms.Add(new CNV { sName = XIConstant.Param_OrgID, sValue = iOrgID.ToString() });
                                                                    oCR = Save_BODashBoards(oPrms, sDashboardType);
                                                                    oTrace.oTrace.Add(oCR.oTrace);
                                                                    if (oCR.bOK && oCR.oResult != null)
                                                                    {

                                                                    }
                                                                    else
                                                                    {
                                                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                                    }
                                                                }
                                                                if (oTrace.iStatus != (int)xiEnumSystem.xiFuncResult.xiError)
                                                                {
                                                                    var oCloneBOD = (XIDBO)oBOD.Clone(oBOD);
                                                                    oCloneBOD.Attributes = oCloneBOD.Attributes.ToDictionary(dic => dic.Key, dic => dic.Value);
                                                                    oCloneBOD.Scripts = new Dictionary<string, XIDScript>();
                                                                    oBOI.SetAttribute("bdefaults", "1");
                                                                    oBOI.BOD = oCloneBOD;
                                                                    oCR = oBOI.SaveV2(oBOI);
                                                                    oTrace.oTrace.Add(oCR.oTrace);
                                                                    if (oCR.bOK && oCR.oResult != null)
                                                                    {

                                                                    }
                                                                    else
                                                                    {
                                                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                                    }
                                                                }
                                                            }
                                                            else
                                                            {
                                                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                            }
                                                        }
                                                        else
                                                        {
                                                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                    }
                                                }
                                                else
                                                {
                                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                }
                                            }
                                            else
                                            {
                                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                            }
                                        }
                                        else
                                        {
                                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                        }
                                    }
                                    else
                                    {
                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                    }
                                }
                                else
                                {
                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                }
                            }
                            if (oTrace.iStatus != (int)xiEnumSystem.xiFuncResult.xiError)
                            {
                                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                                oCResult.oResult = "Success";
                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                            }
                        }
                        else
                        {
                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                            oTrace.sMessage = "BOI Load failed for sBO: XIBO and iBOIID:" + iBODID;
                        }
                    }
                    else
                    {
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                        oTrace.sMessage = "BO Definition is not loaded from Cache BO Name: XIBO";
                    }
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory Param: iBODID is missing";
                }
            }
            catch (Exception ex)
            {
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            oTrace.sProcessID = Guid.NewGuid().ToString().Substring(0, 12);
            oTrace.sParentID = "#";
            Save_CodeLog(oTrace);
            oCResult.sCode = iRowXilinkID.ToString();
            return oCResult;
        }

        public CResult Interpret_BO(string XICode)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Interpret BO based on text";

            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "BO interpretation started" });
                }
                string XITask = string.Empty;
                string sBOName = string.Empty;
                int iBODID = 0;
                if (!string.IsNullOrEmpty(XICode))
                {
                    var CodeLines = XICode.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (CodeLines != null && CodeLines.Count() > 0)
                    {
                        var XIKeyWord = CodeLines.FirstOrDefault();
                        if (XIKeyWord.StartsWith("XI."))
                        {
                            XITask = XIKeyWord.Split('.')[1];
                        }
                        if (XITask.Contains('-'))
                        {
                            sBOName = XITask.Split('-')[1].Trim();
                        }
                        CodeLines.RemoveAt(0);
                        if (!string.IsNullOrEmpty(XITask) && XITask.ToLower().StartsWith("bo"))
                        {
                            XIDBO oBOD = new XIDBO();
                            oBOD.Name = sBOName;
                            oBOD.TableName = sBOName + "_T";
                            oBOD.Description = sBOName;
                            oBOD.FKiApplicationID = iAppID;
                            oBOD.LabelName = sBOName;
                            oBOD.bIsAutoIncrement = true;
                            oBOD.sSize = "10";
                            oBOD.sPrimaryKey = "ID";
                            foreach (var items in CodeLines)
                            {
                                if (items.Contains(',') && items.StartsWith("\t\t"))
                                {
                                    var columns = items.Replace("\t\t", "");
                                    oBOD.sColumns = columns;
                                }
                                else if (items.StartsWith("\t"))
                                {
                                    var DB = items.Replace("\t", "");
                                    var oDataSrc = (XIDataSource)oCache.GetObjectFromCache(XIConstant.CacheDataSource, DB);
                                    if (oDataSrc != null)
                                    {
                                        oBOD.iDataSource = oDataSrc.ID;
                                        oBOD.FKiApplicationID = oDataSrc.FKiApplicationID;
                                        iAppID = oDataSrc.FKiApplicationID;
                                    }
                                }
                            }
                            oCR = Save_BO(oBOD);
                            if (oCR.bOK && oCR.oResult != null)
                            {
                                if (oSigCR != null)
                                {
                                    oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "BO interpretation completed" });
                                }
                                var oBOI = (XIIBO)oCR.oResult;
                                var sPrimaryKey = oBOI.BOD.sPrimaryKey;
                                var sID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == sPrimaryKey.ToLower()).Select(m => m.sValue).FirstOrDefault();
                                int.TryParse(sID, out iBODID);
                                if (iBODID > 0)
                                {
                                    List<CNV> oParams = new List<CNV>();
                                    oParams.Add(new CNV { sName = XIConstant.Param_BO, sValue = sBOName });
                                    oParams.Add(new CNV { sName = XIConstant.Param_InstanceID, sValue = iBODID.ToString() });
                                    //oCR = Build_BO(oParams);
                                    if (oCR.bOK && oCR.oResult != null)
                                    {

                                    }
                                }
                            }
                        }
                    }
                }
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "BO interpretaion failed" });
                oSigCR.oStack.Add(new CNV { sName = "Error Message", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting Xilink" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            return oCResult;
        }

        public string GetVersionForBO(string sUpdatedVersion)
        {
            float rUpdatedVersion = float.Parse(sUpdatedVersion, CultureInfo.InvariantCulture.NumberFormat);
            rUpdatedVersion += 0.1F;
            return rUpdatedVersion.ToString();
        }

        #endregion BO

        #region BO Menu

        public CResult Save_BOMenu(List<CNV> oParams)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Save BO Menu and Popup for 1Click";
            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "BO default menu insertion started" });
                }
                long iBOID = 0;
                long i1ClickID = 0;
                int FKiAppID = 0;
                int iOrgID = 0;
                var s1ClickID = oParams.Where(m => m.sName.ToLower() == XIConstant.Param_1ClickID.ToLower()).Select(m => m.sValue).FirstOrDefault();
                long.TryParse(s1ClickID, out i1ClickID);
                var sBO = oParams.Where(m => m.sName.ToLower() == XIConstant.Param_BO.ToLower()).Select(m => m.sValue).FirstOrDefault();
                var sBOLabel = oParams.Where(m => m.sName.ToLower() == "BOLabel".ToLower()).Select(m => m.sValue).FirstOrDefault();
                var sAppID = oParams.Where(m => m.sName.ToLower() == XIConstant.Param_ApplicationID.ToLower()).Select(m => m.sValue).FirstOrDefault();
                int.TryParse(sAppID, out FKiAppID);
                var oAPPD = (XIDApplication)oCache.GetObjectFromCache(XIConstant.CacheApplication, null, FKiAppID.ToString());
                var sBOID = oParams.Where(m => m.sName.ToLower() == XIConstant.Param_BODID).Select(m => m.sValue).FirstOrDefault();
                long.TryParse(sBOID, out iBOID);
                var OrgID = oParams.Where(m => m.sName.ToLower() == XIConstant.Param_OrgID.ToLower()).Select(m => m.sValue).FirstOrDefault();
                int.TryParse(OrgID, out iOrgID);
                string sOrgName = string.Empty;
                if (iOrgID > 0)
                {

                    XIIXI oXI = new XIIXI();
                    var oBOI = oXI.BOI("Organisations", iOrgID.ToString());
                    if (oBOI != null && oBOI.Attributes.Count() > 0)
                    {
                        sOrgName = oBOI.AttributeI("Name").sValue;
                    }
                }
                if (i1ClickID > 0)
                {
                    XIIBO oBOI = new XIIBO();
                    int iLayoutID = 0;
                    XIDLayout oLayout = new XIDLayout();
                    oLayout.FKiApplicationID = FKiAppID;
                    oLayout.LayoutName = sBO + " 1CS Layout";
                    oLayout.LayoutCode = XIConstant.DefaultLayout;
                    oLayout.LayoutType = "Dialog";
                    oLayout.LayoutLevel = "OrganisationLevel";
                    oLayout.Authentication = "Authenticated";
                    oLayout.StatusTypeID = 10;
                    oLayout.OrganizationID = iOrgID;
                    oCR = Save_Layout(oLayout);
                    oTrace.oTrace.Add(oCR.oTrace);
                    if (oCR.bOK && oCR.oResult != null)
                    {
                        oBOI = (XIIBO)oCR.oResult;
                        var sLayoutID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "id").Select(m => m.sValue).FirstOrDefault();
                        int.TryParse(sLayoutID, out iLayoutID);
                        if (iLayoutID > 0)
                        {
                            int iPlaceHolderID = 0;
                            XIDLayoutDetails oDetail = new XIDLayoutDetails();
                            oDetail.PlaceHolderID = 0;
                            oDetail.FKiApplicationID = FKiAppID;
                            oDetail.LayoutID = iLayoutID;
                            oDetail.PlaceholderName = oLayout.LayoutName + " Content";
                            oDetail.PlaceholderArea = "div1";
                            oDetail.PlaceholderUniqueName = (oLayout.LayoutName + " Content").Replace(" ", "");
                            oDetail.PlaceholderClass = "col-md-12";
                            oCR = Save_LayoutDetail(oDetail);
                            oTrace.oTrace.Add(oCR.oTrace);
                            if (oCR.bOK && oCR.oResult != null)
                            {
                                oBOI = (XIIBO)oCR.oResult;
                                var sPlaceHolderID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "PlaceHolderID".ToLower()).Select(m => m.sValue).FirstOrDefault();
                                int.TryParse(sPlaceHolderID, out iPlaceHolderID);
                                if (iPlaceHolderID > 0)
                                {
                                    int iDialogID = 0;
                                    XIDDialog oDialog = new XIDDialog();
                                    oDialog.FKiBOID = iBOID;
                                    oDialog.FKiApplicationID = FKiAppID;
                                    oDialog.DialogName = sBO + " 1CS Dialog";
                                    oDialog.PopupSize = "Default";
                                    oDialog.LayoutID = iLayoutID;
                                    oDialog.StatusTypeID = 10;
                                    oDialog.OrganizationID = iOrgID;
                                    oCR = Save_Dialog(oDialog);
                                    oTrace.oTrace.Add(oCR.oTrace);
                                    if (oCR.bOK && oCR.oResult != null)
                                    {
                                        oBOI = (XIIBO)oCR.oResult;
                                        var sDialogID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "ID".ToLower()).Select(m => m.sValue).FirstOrDefault();
                                        int.TryParse(sDialogID, out iDialogID);
                                        if (iDialogID > 0)
                                        {
                                            int iLayoutMappingID = 0;
                                            XIDLayoutMapping oMap = new XIDLayoutMapping();
                                            oMap.PopupID = iDialogID;
                                            oMap.PopupLayoutID = iLayoutID;
                                            oMap.PlaceHolderID = iPlaceHolderID;
                                            oMap.FKiApplicationID = FKiAppID;
                                            oMap.ContentType = "XIComponent";
                                            oMap.StatusTypeID = 10;
                                            oMap.XiLinkID = 3;
                                            oCR = Save_LayoutMapping(oMap);
                                            oTrace.oTrace.Add(oCR.oTrace);
                                            if (oCR.bOK && oCR.oResult != null)
                                            {
                                                oBOI = (XIIBO)oCR.oResult;
                                                var sLayoutMappingID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "ID".ToLower()).Select(m => m.sValue).FirstOrDefault();
                                                int.TryParse(sLayoutMappingID, out iLayoutMappingID);
                                                if (iLayoutMappingID > 0)
                                                {
                                                    XIDComponentParam oParam = new XIDComponentParam();
                                                    oParam.sName = XIConstant.Param_1ClickID;
                                                    oParam.sValue = i1ClickID.ToString();
                                                    oParam.iLayoutMappingID = iPlaceHolderID;
                                                    oParam.FKiComponentID = 3;
                                                    oCR = Save_ComponentParam(oParam);
                                                    oTrace.oTrace.Add(oCR.oTrace);
                                                    if (oCR.bOK && oCR.oResult != null)
                                                    {

                                                    }
                                                    else
                                                    {
                                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                    }
                                                    oParam = new XIDComponentParam();
                                                    oParam.sName = "Visualisation";
                                                    oParam.sValue = "OneClickVisibility";
                                                    oParam.iLayoutMappingID = iPlaceHolderID;
                                                    oParam.FKiComponentID = 3;
                                                    oCR = Save_ComponentParam(oParam);
                                                    oTrace.oTrace.Add(oCR.oTrace);
                                                    if (oCR.bOK && oCR.oResult != null)
                                                    {

                                                    }
                                                    else
                                                    {
                                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                            }
                                            //Menu XILink
                                            int iXILinkID = 0;
                                            XILink oXILink = new XILink();
                                            oXILink.Name = sBO + " 1CS Menu";
                                            oXILink.URL = "XILink";
                                            oXILink.sActive = "Y";
                                            oXILink.StatusTypeID = 10;
                                            oXILink.FKiApplicationID = FKiAppID;
                                            oXILink.OrganisationID = iOrgID;
                                            oCR = Save_XILink(oXILink);
                                            oTrace.oTrace.Add(oCR.oTrace);
                                            if (oCR.bOK && oCR.oResult != null)
                                            {
                                                oBOI = (XIIBO)oCR.oResult;
                                                var sXILinkID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "XILinkID".ToLower()).Select(m => m.sValue).FirstOrDefault();
                                                int.TryParse(sXILinkID, out iXILinkID);
                                                if (iXILinkID > 0)
                                                {
                                                    XiLinkNV oNV = new XiLinkNV();
                                                    oNV.XiLinkID = iXILinkID;
                                                    oNV.Name = "StartAction";
                                                    oNV.Value = "Dialog";
                                                    oCR = Save_XILinkNV(oNV);
                                                    oTrace.oTrace.Add(oCR.oTrace);
                                                    oTrace.oTrace.Add(oCR.oTrace);
                                                    oNV = new XiLinkNV();
                                                    oNV.XiLinkID = iXILinkID;
                                                    oNV.Name = "DialogID";
                                                    oNV.Value = iDialogID.ToString();
                                                    oCR = Save_XILinkNV(oNV);
                                                    oTrace.oTrace.Add(oCR.oTrace);
                                                    if (oCR.bOK && oCR.oResult != null)
                                                    {

                                                    }
                                                    else
                                                    {
                                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                    }
                                                }

                                                XIIXI oXI = new XIIXI();
                                                List<CNV> oWhereParams = new List<CNV>();
                                                if (iOrgID > 0)
                                                {
                                                    oWhereParams.Add(new CNV { sName = "rootname", sValue = sOrgName.Replace(" ", "") + "OrgMenu" });
                                                }
                                                else
                                                {
                                                    oWhereParams.Add(new CNV { sName = "rootname", sValue = oAPPD.sApplicationName.Replace(" ", "") });
                                                }
                                                oWhereParams.Add(new CNV { sName = "parentid", sValue = "#" });
                                                oWhereParams.Add(new CNV { sName = "fkiapplicationid", sValue = FKiAppID.ToString() });
                                                var oMenuD = oXI.BOI("XI Menu", null, null, oWhereParams);
                                                var iMenuID = "0";
                                                if (oMenuD != null && oMenuD.Attributes.Count() > 0)
                                                {
                                                    iMenuID = oMenuD.Attributes["ID"].sValue;
                                                }

                                                //var oMenuD = (List<XIMenu>)oCache.GetObjectFromCache(XIConstant.CacheMenu, oAPPD.sApplicationName, null);
                                                //var ParentMenu = oMenuD.FirstOrDefault().ParentID;
                                                XIMenu oMenu = new XIMenu();
                                                oMenu.FKiApplicationID = FKiAppID;
                                                oMenu.Name = sBOLabel;
                                                if (iOrgID > 0)
                                                {
                                                    oMenu.RootName = sOrgName.Replace(" ", "") + "OrgMenu";
                                                }
                                                else
                                                {
                                                    oMenu.RootName = oAPPD.sApplicationName.Replace(" ", "");
                                                }
                                                oMenu.ParentID = iMenuID;
                                                oMenu.ActionType = 30;
                                                oMenu.XiLinkID = iXILinkID;
                                                oMenu.StatusTypeID = 10;
                                                oMenu.OrgID = iOrgID;
                                                oCR = Save_Menu(oMenu);
                                                oTrace.oTrace.Add(oCR.oTrace);
                                                if (oCR.bOK && oCR.oResult != null)
                                                {

                                                }
                                                else
                                                {
                                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                }
                                                //Creating Menus in XIMenuMapping_T
                                                //XIIXI oXIM = new XIIXI();
                                                //List<CNV> oWhereParamsm = new List<CNV>();
                                                //oWhereParamsm.Add(new CNV { sName = "rootname", sValue = oAPPD.sApplicationName.Replace(" ", "") });
                                                //oWhereParamsm.Add(new CNV { sName = "parentid", sValue = "#" });
                                                //oWhereParamsm.Add(new CNV { sName = "fkiapplicationid", sValue = FKiAppID.ToString() });
                                                //var oMenuM = oXIM.BOI("AssignMenu", null, null, oWhereParamsm);
                                                //var iMapMenuID = "0";
                                                //if (oMenuM != null && oMenuM.Attributes.Count() > 0)
                                                //{
                                                //    iMapMenuID = oMenuM.Attributes["ID"].sValue;
                                                //}

                                                //XIAssignMenu oAS = new XIAssignMenu();
                                                //oAS.FKiApplicationID = FKiAppID;
                                                //oAS.Name = sBO;
                                                //oAS.RootName = oAPPD.sApplicationName;
                                                //oAS.ParentID = iMapMenuID;
                                                //oAS.ActionType = 30;
                                                //oAS.XiLinkID = iXILinkID;
                                                //oAS.sType = "AssignMenu";
                                                //oAS.OrganizationID = iOrgID;
                                                //oCR = Save_AssignMenu(oAS);
                                                //oTrace.oTrace.Add(oCR.oTrace);
                                                //var iCreatedMenuID = 0;
                                                //if (oCR.bOK && oCR.oResult != null)
                                                //{
                                                //    var oMenuDef = (XIIBO)oCR.oResult;
                                                //    var sMenuID = oMenuDef.Attributes.Where(m => m.Key.ToLower() == "id").Select(m => m.Value).Select(m => m.sValue).FirstOrDefault();
                                                //    int.TryParse(sMenuID, out iCreatedMenuID);
                                                //}
                                                //else
                                                //{
                                                //    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                //}
                                                ////Menu Mapping
                                                //XIAssignMenu oASM = new XIAssignMenu();
                                                //oASM.Name = sBO;
                                                //oASM.ActionType = 30;
                                                //oASM.XiLinkID = iXILinkID;
                                                //oASM.FKiApplicationID = FKiAppID;
                                                //oASM.sType = "CreateMenu";
                                                //oASM.OrganizationID = iOrgID;
                                                //oCR = Save_AssignMenu(oASM);
                                                //oTrace.oTrace.Add(oCR.oTrace);
                                                //if (oCR.bOK && oCR.oResult != null)
                                                //{
                                                //    var oMenuDef = (XIIBO)oCR.oResult;
                                                //    var sMenuID = oMenuDef.Attributes.Where(m => m.Key.ToLower() == "id").Select(m => m.Value).Select(m => m.sValue).FirstOrDefault();

                                                //    //Update FKiMenuID for Menu in XIMenuMappings_T Table
                                                //    XIIBO oBOIS = new XIIBO();
                                                //    XIDBO oBOD = new XIDBO();
                                                //    oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "AssignMenu");
                                                //    oBOIS.BOD = oBOD;
                                                //    oBOIS.SetAttribute("ID", iCreatedMenuID.ToString());
                                                //    oBOIS.SetAttribute("FKiMenuID", sMenuID);
                                                //    oCR = oBOIS.SaveV2(oBOIS);
                                                //    oTrace.oTrace.Add(oCR.oTrace);
                                                //    if (oCR.bOK && oCR.oResult != null)
                                                //    {

                                                //    }
                                                //    else
                                                //    {
                                                //        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                //    }
                                                //}
                                                //else
                                                //{
                                                //    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                //}
                                            }
                                            else
                                            {
                                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                    }
                                }
                            }
                            else
                            {
                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                            }
                        }
                    }
                    else
                    {
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                    }
                }
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "success", sValue = "BO default menu insertion completed" });
                }
                if (oTrace.iStatus != (int)xiEnumSystem.xiFuncResult.xiError)
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                    oCResult.oResult = "Success";
                    oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                }
            }
            catch (Exception ex)
            {
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "BO default menu insertion failed" });
                oSigCR.oStack.Add(new CNV { sName = "Error Message", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting BO" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        #endregion BO Menu

        #region BO Attribute

        public CResult Parse_BOAttributes(XIDBO oBO)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name; ;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            //tracing
            // if tracing is on (which is a config switch for this user) then
            //   oCResult.Trace.Write
            //ALL OK?
            try
            {
                XIInfraEncryption oEnc = new XIInfraEncryption();
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "BO Attributes insertion started" });
                }
                List<string> sDefaultColumns = Regex.Split(XIConstant.DefaultTableAttributes, "\r\n").ToList();
                if (!string.IsNullOrEmpty(oBO.sColumns) || oBO.sColumns == "")
                {
                    List<string> AttrDetails = new List<string>();
                    if (oBO.sColumns.Contains("\r\n") || oBO.sColumns.Contains(',') || oBO.sColumns.Count() > 0)
                    {
                        if (oBO.sColumns.Contains("\r\n") || oBO.sColumns.StartsWith("FK") || oBO.sColumns.StartsWith("ref"))
                        {
                            AttrDetails = Regex.Split(oBO.sColumns, "\r\n").ToList();
                        }
                        else if (oBO.sColumns.Contains(','))
                        {
                            AttrDetails = Regex.Split(oBO.sColumns, ",").ToList();
                        }
                        else
                        {
                            AttrDetails = Regex.Split(oBO.sColumns, ",").ToList();
                        }
                        AttrDetails = sDefaultColumns.Union(AttrDetails).ToList();
                    }
                    else if (string.IsNullOrEmpty(oBO.sColumns))
                    {
                        AttrDetails = sDefaultColumns;
                    }
                    if (AttrDetails.Where(m => m.ToLower() == "id").FirstOrDefault() == null)
                    {
                        AttrDetails.Add("ID");
                    }
                    var oAttrs = new List<XIDAttribute>();
                    foreach (string Attr in AttrDetails)
                    {
                        XIDAttribute oXIAttrD = new XIDAttribute();
                        var sAttr = Attr;
                        var sCol = Attr;
                        //oXIAttrD.bIsEncrypt = oBO.bIsEncrypt;
                        string sOptionList = string.Empty;
                        if (sAttr.Contains(':'))
                        {
                            string sSplitAttr = sAttr;
                            sAttr = sSplitAttr.Split(':')[0];
                            sCol = sSplitAttr.Split(':')[0];
                            sOptionList = sSplitAttr.Split(':')[1];
                        }
                        else if (sAttr.Contains(','))
                        {
                            string sSplitAttr = sAttr;
                            sCol = sSplitAttr.Split(',')[0];
                        }
                        XIDXI oXIDs = new XIDXI();
                            oXIAttrD.Name = sAttr;
                        int iDetailsCount = Regex.Matches(sAttr, ",").Count;
                        if (iDetailsCount == 0)
                        {
                            var Char = sAttr.Select(c => char.IsUpper(c)).ToList();
                            var Position = Char.IndexOf(true);
                            if (Position == 0 || sAttr.ToLower() == "id")
                            {
                                if (sAttr.ToLower() == "ID".ToLower())
                                {
                                    oXIAttrD.LabelName = sAttr;
                                    oXIAttrD.TypeID = 10;
                                    oXIAttrD.MaxLength = "0";
                                    oXIAttrD.DataType = "int IDENTITY(1,1) PRIMARY KEY";
                                }
                                else if (sAttr.ToLower() == "xiguid")
                                {
                                    oXIAttrD.LabelName = sAttr;
                                    oXIAttrD.TypeID = 250;
                                    oXIAttrD.MaxLength = "0";
                                    oXIAttrD.DataType = "uniqueidentifier NOT NULL DEFAULT newid()";
                                }
                            }
                            else if (Position == 1)
                            {
                                string sFirstLetter = sAttr.Substring(0, 1);
                                if (sFirstLetter == "i")
                                {
                                    var sLabelName = sAttr.Substring(1);
                                    oXIAttrD.LabelName = sLabelName;
                                    oXIAttrD.TypeID = 60;
                                    oXIAttrD.MaxLength = "0";
                                    oXIAttrD.DataType = "int NULL";
                                }
                                else if (sFirstLetter == "s")
                                {
                                    var length = "256";
                                    var AttrName = sAttr;
                                    var sLabelName = sAttr.Substring(1);
                                    if (sAttr.Contains("("))
                                    {
                                        var index = sAttr.IndexOf("(");
                                        AttrName = sAttr.Substring(0, index);
                                        sLabelName = AttrName.Substring(1);
                                        length = sAttr.Replace(AttrName, "").Replace("(", "").Replace(")", "");
                                    }

                                    oXIAttrD.Name = AttrName;
                                    oXIAttrD.LabelName = sLabelName;
                                    oXIAttrD.TypeID = 180;
                                    oXIAttrD.MaxLength = length;
                                    oXIAttrD.DataType = "varchar(256) NOT NULL";
                                }
                                else if (sFirstLetter == "d")
                                {
                                    var sLabelName = sAttr.Substring(1);
                                    oXIAttrD.LabelName = sLabelName;
                                    oXIAttrD.TypeID = 150;
                                    oXIAttrD.MaxLength = "11";
                                    oXIAttrD.DataType = "datetime NULL";
                                }
                                else if (sFirstLetter == "r" || sFirstLetter == "f")
                                {
                                    var sLabelName = sAttr.Substring(1);
                                    oXIAttrD.LabelName = sLabelName;
                                    oXIAttrD.TypeID = 90;
                                    oXIAttrD.MaxLength = "15";
                                    oXIAttrD.DataType = "float NULL";
                                }
                                else if (sFirstLetter == "n")
                                {
                                    oXIAttrD.Name = sAttr;
                                    var sLabelName = sAttr.Substring(1);
                                    oXIAttrD.LabelName = sLabelName;
                                    oXIAttrD.TypeID = 180;
                                    oXIAttrD.MaxLength = "MAX";
                                    oXIAttrD.DataType = "nvarchar(MAX) NOT NULL";
                                }
                                else if (sFirstLetter == "b")
                                {
                                    var sLabelName = sAttr.Substring(1);
                                    oXIAttrD.LabelName = sLabelName;
                                    oXIAttrD.TypeID = 20;
                                    oXIAttrD.MaxLength = "0";
                                    oXIAttrD.DataType = "bit NULL";
                                }
                                else if (sFirstLetter == "t")
                                {
                                    var sLabelName = sAttr.Substring(1);
                                    oXIAttrD.LabelName = sLabelName;
                                    oXIAttrD.TypeID = 180;
                                    oXIAttrD.MaxLength = "50";
                                    oXIAttrD.DataType = "varchar(50) NULL";
                                }
                            }
                            else if (Position == 2)
                            {
                                string sFrstTwoLetter = sAttr.Substring(0, 2);
                                if (sFrstTwoLetter == "dt" || sFrstTwoLetter == "d")
                                {
                                    var sLabelName = sAttr.Substring(2);
                                    oXIAttrD.LabelName = sLabelName;
                                    oXIAttrD.TypeID = 150;
                                    oXIAttrD.MaxLength = "20";
                                    oXIAttrD.DataType = "datetime NULL";
                                }
                                else if (sFrstTwoLetter == "iz" && sAttr.ToLower() == XIConstant.Key_XIDeleted.ToLower())
                                {
                                    var sLabelName = sAttr.Substring(1);
                                    oXIAttrD.LabelName = sLabelName;
                                    oXIAttrD.TypeID = 60;
                                    oXIAttrD.MaxLength = "0";
                                    oXIAttrD.DataType = "int NULL";
                                }
                            }
                        }
                        else if (iDetailsCount == 1)
                        {
                            string[] sSplitColDetails = Regex.Split(sAttr, ",");
                            var sColName = sSplitColDetails[0];
                            oXIAttrD.Name = sColName;
                            var sPK_Table = string.Empty;
                            var Char = sColName.Select(c => char.IsUpper(c)).ToList();
                            var Position = Char.IndexOf(true);
                            if (sSplitColDetails.Count() > 1)
                            {
                                sPK_Table = sSplitColDetails[1];
                            }
                            if (sSplitColDetails.Count() > 2)
                            {
                                var sPK_Column = sSplitColDetails[2];
                            }
                            if (Position == 0)
                            {
                                string sFirstTwoLetters = sColName.Substring(0, 2);
                                if (sFirstTwoLetters == "FK")
                                {
                                    var sLabelName = sColName.Substring(3);
                                    if (!string.IsNullOrEmpty(sPK_Table))
                                    {
                                        var FKBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, sPK_Table);
                                        if (FKBOD != null && FKBOD.BOID > 0)
                                        {
                                            oXIAttrD.FKTableName = FKBOD.TableName;
                                            oXIAttrD.sFKBOName = FKBOD.Name;
                                        }
                                    }
                                    string ThirdLetter = sColName.Substring(2, 1);
                                    if (ThirdLetter == "i")
                                    {
                                        oXIAttrD.LabelName = sLabelName;
                                        oXIAttrD.MaxLength = "9";
                                        oXIAttrD.TypeID = 10;
                                        oXIAttrD.DataType = "int NULL";
                                        oXIAttrD.FKiType = 10;
                                    }
                                    else if (ThirdLetter == "s")
                                    {
                                        oXIAttrD.LabelName = sLabelName;
                                        oXIAttrD.MaxLength = "256";
                                        oXIAttrD.TypeID = 180;
                                        oXIAttrD.DataType = "varchar(256) NOT NULL";
                                        oXIAttrD.FKiType = 10;
                                    }
                                }
                            }
                            else if (Position == 3)
                            {
                                var Prefix = sColName.Substring(0, 3);
                                if (Prefix == "ref")
                                {
                                    if (!string.IsNullOrEmpty(sPK_Table))
                                    {
                                        oXIAttrD.FKTableName = sPK_Table;
                                    }
                                    var sLabelName = sColName.Substring(3);
                                    oXIAttrD.LabelName = sLabelName;
                                    oXIAttrD.MaxLength = "9";
                                    oXIAttrD.TypeID = 10;
                                    oXIAttrD.DataType = "int NULL";
                                    oXIAttrD.FKiType = 10;
                                }
                            }
                        }
                        //}
                        if (!string.IsNullOrEmpty(sOptionList))
                        {
                            oXIAttrD.OptionList = new List<XIDOptionList>();
                            var OptionList = sOptionList.Split(',').ToList();
                            int optValue = 10;
                            foreach (var opt in OptionList)
                            {
                                XIDOptionList oOption = new XIDOptionList();
                                oOption.BOID = oXIAttrD.BOID;
                                oOption.Name = oXIAttrD.Name;
                                if (opt.Contains("="))
                                {
                                    var sONV = opt.Split('=').ToList();
                                    oOption.sOptionName = sONV[0];
                                    if (sONV.Count() == 2)
                                    {
                                        oOption.sValues = sONV[1];
                                    }
                                }
                                else
                                {
                                    oOption.sOptionName = opt;
                                    oOption.sValues = optValue.ToString();
                                }
                                oOption.StatusTypeID = 10;
                                optValue = optValue + 10;
                                oXIAttrD.IsOptionList = true;
                                oXIAttrD.OptionList.Add(oOption);
                            }
                        }
                        oXIAttrD.IsWhere = true;
                        oXIAttrD.IsVisible = true;
                        oXIAttrD.IsGroupBy = true;
                        oXIAttrD.IsOrderBy = true;
                        oAttrs.Add(oXIAttrD);
                    }
                    if (oAttrs.Count() > 0)
                    {
                        XIIXI oXII = new XIIXI();
                        foreach (var items in oAttrs)
                        {
                            oCR = Save_BOAttribute(items);
                            oTrace.oTrace.Add(oCR.oTrace);
                            if (oCR.bOK && oCR.oResult != null)
                            {
                                var oAttrI = (XIIBO)oCR.oResult;
                                int iAttrID = 0;
                                var sPK = oAttrI.BOD.sPrimaryKey;
                                var sAttrID = oAttrI.Attributes.Values.Where(m => m.sName.ToLower() == sPK.ToLower()).Select(m => m.sValue).FirstOrDefault();
                                int.TryParse(sAttrID, out iAttrID);
                                if (oSigCR != null)
                                {
                                    oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "BO Attribute insertion completed for Attribute " + items.Name });
                                }
                                if (items.IsOptionList && items.OptionList != null && items.OptionList.Count() > 0)
                                {
                                    foreach (var opt in items.OptionList)
                                    {
                                        opt.BOID = iBODID;
                                        opt.BOFieldID = iAttrID;
                                        oCR = Save_BOOptionList(opt);
                                        oTrace.oTrace.Add(oCR.oTrace);
                                        if (oCR.bOK && oCR.oResult != null)
                                        {
                                            if (oTrace.iStatus != (int)xiEnumSystem.xiFuncResult.xiError || oTrace.iStatus != (int)xiEnumSystem.xiFuncResult.xiLogicalError)
                                            {
                                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                                            }
                                        }
                                        else
                                        {
                                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                        }
                                    }
                                }
                                else
                                {
                                    oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                                }
                            }
                            else
                            {
                                if (oSigCR != null)
                                {
                                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "BO Attribute insertion failed for Attribute " + items.Name });
                                }
                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                            }
                        }
                    }
                    else
                    {
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    }
                }
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting BO Attributes" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            if (oTrace.iStatus != (int)xiEnumSystem.xiFuncResult.xiError || oTrace.iStatus != (int)xiEnumSystem.xiFuncResult.xiLogicalError)
            {
                oCResult.oResult = "Success";
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        public CResult Save_BOOptionList(XIDOptionList Option)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name; ;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "Option list insertion started" });
                }
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD1 = new XIDBO();
                oBOD1 = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIBOOptionList");
                oBOI.BOD = oBOD1;
                oTrace.oParams.Add(new CNV { sName = "BOFieldID", sValue = Option.BOFieldID.ToString() });
                if (Option.BOFieldID != 0)
                {
                    oBOI.SetAttribute("ID", Option.ID.ToString());
                    oBOI.SetAttribute("BOID", Option.BOID.ToString());
                    oBOI.SetAttribute("BOFieldID", Option.BOFieldID.ToString());
                    oBOI.SetAttribute("Name", Option.Name);
                    oBOI.SetAttribute("sOptionName", Option.sOptionName);
                    oBOI.SetAttribute("sValues", Option.sValues);
                    oBOI.SetAttribute("StatusTypeID", Option.StatusTypeID.ToString());
                    oBOI.SetAttribute("sOptionCode", Option.sOptionCode);
                    oBOI.SetAttribute("sShowField", Option.sShowField);
                    oBOI.SetAttribute("sHideField", Option.sHideField);
                    oBOI.SetAttribute("iType", Option.iType.ToString());
                    oCR = oBOI.SaveV2(oBOI);
                    oTrace.oTrace.Add(oCR.oTrace);
                    if (oCR.bOK && oCR.oResult != null)
                    {
                        oBOI = (XIIBO)oCR.oResult;
                        oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                        if (oSigCR != null)
                        {
                            oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "Option list insertion completed" });
                        }
                        oCResult.oResult = oBOI;
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                    }
                    else
                    {
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                        oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Option list insertion failed" });
                    }
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory param BO Field ID is Missing ";
                }

            }
            catch (Exception ex)
            {
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Option list insertion failed" });
                oSigCR.oStack.Add(new CNV { sName = "Error Message", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while inserting option list" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        public CResult Save_BOAttribute(XIDAttribute oAttr)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name; ;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            //tracing
            // if tracing is on (which is a config switch for this user) then
            //   oCResult.Trace.Write
            //ALL OK?
            try
            {
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIBOAttribute");
                oBOI.BOD = oBOD;
                oTrace.oParams.Add(new CNV { sName = "Name", sValue = oAttr.Name });
                if (oAttr.Name != null)
                {
                    oBOI.SetAttribute("ID", oAttr.ID.ToString());
                    oBOI.SetAttribute("BOID", iBODID.ToString());
                    oBOI.SetAttribute("FieldCreatedID", oAttr.FieldCreatedID.ToString());
                    oBOI.SetAttribute("OrganizationID", iOrgID.ToString());
                    oBOI.SetAttribute("Name", oAttr.Name);
                    oBOI.SetAttribute("LabelName", oAttr.LabelName);
                    oBOI.SetAttribute("TypeID", oAttr.TypeID.ToString());
                    oBOI.SetAttribute("Format", oAttr.Format);
                    if (oAttr.FKTableName == null)
                    {
                        oBOI.SetAttribute("FKTableName", oAttr.FKTableName);
                    }
                    else
                    {
                        oBOI.SetAttribute("FKTableName", oAttr.FKTableName);
                    }
                    oBOI.SetAttribute("Script", oAttr.Script);
                    oBOI.SetAttribute("ScriiptExecutionType", oAttr.ScriiptExecutionType);
                    oBOI.SetAttribute("MaxLength", oAttr.MaxLength);
                    oBOI.SetAttribute("IsVisible", "1");
                    oBOI.SetAttribute("IsWhere", oAttr.IsWhere.ToString());
                    oBOI.SetAttribute("IsTotal", oAttr.IsTotal.ToString());
                    oBOI.SetAttribute("IsGroupBy", oAttr.IsGroupBy.ToString());
                    oBOI.SetAttribute("IsOrderBy", oAttr.IsOrderBy.ToString());
                    oBOI.SetAttribute("IsExpression", oAttr.IsExpression.ToString());
                    oBOI.SetAttribute("IsMail", oAttr.IsMail.ToString());
                    oBOI.SetAttribute("IsRunTime", oAttr.IsRunTime.ToString());
                    oBOI.SetAttribute("IsTextArea", oAttr.IsTextArea.ToString());
                    oBOI.SetAttribute("IsDBValue", "0");
                    oBOI.SetAttribute("DBQuery", oAttr.DBQuery);
                    oBOI.SetAttribute("IsWhereExpression", oAttr.IsWhereExpression.ToString());
                    oBOI.SetAttribute("WhereExpression", oAttr.WhereExpression);
                    oBOI.SetAttribute("WhereExpreValue", oAttr.WhereExpreValue);
                    oBOI.SetAttribute("IsDate", oAttr.IsDate.ToString());
                    oBOI.SetAttribute("DateExpression", oAttr.DateExpression);
                    oBOI.SetAttribute("DateValue", oAttr.DateValue);
                    oBOI.SetAttribute("ExpressionText", oAttr.ExpressionText);
                    oBOI.SetAttribute("ExpressionValue", oAttr.ExpressionValue);
                    oBOI.SetAttribute("Description", oAttr.Description);
                    oBOI.SetAttribute("FieldClass", oAttr.FieldClass);
                    oBOI.SetAttribute("FieldClassID", oAttr.FieldClassID.ToString());
                    oBOI.SetAttribute("Value", oAttr.Value);
                    oBOI.SetAttribute("DefaultValue", oAttr.DefaultValue);
                    oBOI.SetAttribute("IsOptionList", oAttr.IsOptionList.ToString());
                    oBOI.SetAttribute("FKiType", oAttr.FKiType.ToString());
                    oBOI.SetAttribute("iOneClickID", oAttr.iOneClickID.ToString());
                    oBOI.SetAttribute("iMasterDataID", oAttr.iMasterDataID.ToString());
                    oBOI.SetAttribute("FKWhere", oAttr.FKWhere);
                    oBOI.SetAttribute("sHelpText", oAttr.sHelpText);
                    oBOI.SetAttribute("sNarrowBar", oAttr.sNarrowBar);
                    oBOI.SetAttribute("FKiFileTypeID", oAttr.FKiFileTypeID.ToString());
                    oBOI.SetAttribute("IsNull", oAttr.IsNull.ToString());
                    oBOI.SetAttribute("sXMLDataType", oAttr.sXMLDataType);
                    oBOI.SetAttribute("sPassword", oAttr.sPassword);
                    oBOI.SetAttribute("sPrecision", oAttr.sPrecision);
                    oBOI.SetAttribute("sVirtualColumn", oAttr.sVirtualColumn);
                    oBOI.SetAttribute("sLock", oAttr.sLock);
                    oBOI.SetAttribute("sCase", oAttr.sCase);
                    oBOI.SetAttribute("ForeignTable", oAttr.ForeignTable);
                    oBOI.SetAttribute("ForeignColumn", oAttr.ForeignColumn);
                    oBOI.SetAttribute("iOutputLength", oAttr.iOutputLength.ToString());
                    oBOI.SetAttribute("sNotes", oAttr.sNotes);
                    oBOI.SetAttribute("sEncrypted", oAttr.sEncrypted);
                    oBOI.SetAttribute("sFKBOName", oAttr.sFKBOName);
                    oBOI.SetAttribute("sMinDate", oAttr.sMinDate);
                    oBOI.SetAttribute("sMaxDate", oAttr.sMaxDate);
                    oBOI.SetAttribute("sPlaceHolder", oAttr.sPlaceHolder);
                    oBOI.SetAttribute("bIsFileAliasName", oAttr.bIsFileAliasName.ToString());
                    oBOI.SetAttribute("bIsFieldDependent", oAttr.bIsFieldDependent.ToString());
                    oBOI.SetAttribute("iDependentFieldID", oAttr.iDependentFieldID.ToString());
                    oBOI.SetAttribute("bIsFieldMerge", oAttr.bIsFieldMerge.ToString());
                    oBOI.SetAttribute("iMergeFieldID", oAttr.iMergeFieldID.ToString());
                    oBOI.SetAttribute("bIsMandatory", oAttr.bIsMandatory.ToString());
                    oBOI.SetAttribute("sDefaultDate", oAttr.sDefaultDate);
                    oBOI.SetAttribute("bIsHidden", oAttr.bIsHidden.ToString());
                    oBOI.SetAttribute("iAttributeType", oAttr.iAttributeType.ToString());
                    oBOI.SetAttribute("sEventHandler", oAttr.sEventHandler);
                    oBOI.SetAttribute("sDepBOFieldIDs", oAttr.sDepBOFieldIDs);
                    oBOI.SetAttribute("bIsFileProcess", oAttr.bIsFileProcess.ToString());
                    oBOI.SetAttribute("iFileProcessType", oAttr.iFileProcessType.ToString());
                    oBOI.SetAttribute("StatusTypeID", oAttr.StatusTypeID.ToString());
                    oBOI.SetAttribute("CreatedByName", iUserID.ToString());
                    oBOI.SetAttribute("CreatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                    oBOI.SetAttribute("CreatedByID", iUserID.ToString());
                    oBOI.SetAttribute("CreatedTime", DateTime.Now.ToString());
                    oBOI.SetAttribute("bIsEncrypt", oAttr.bIsEncrypt.ToString());
                    oCR = oBOI.SaveV2(oBOI);
                    oTrace.oTrace.Add(oCR.oTrace);
                    if (oCR.bOK && oCR.oResult != null)
                    {
                        //if (oAttr.bIsEncrypt == true)
                        //{
                        //    XIInfraEncryption oEnc = new XIInfraEncryption();
                        //    var value = ((XIIBO)oCR.oResult).Attributes.Where(u => u.Key.ToLower() == "id").Select(y => y.Value.sValue).FirstOrDefault();
                        //    var EncName = oEnc.EncryptionData(oAttr.Name, true, value, iUserID.ToString());
                        //    oBOI.SetAttribute("Name", EncName);
                        //    var sValue = oEnc.DecryptionData(EncName, true, value, iUserID.ToString());
                        //    oCR = oBOI.SaveV2(oBOI);
                        //}
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                        oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                        oCResult = oCR;
                    }
                    else
                    {
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                    }
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory param Attribute Name is Missing";
                }

            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting BOAttributes" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        public CResult Save_BOAttr(XIDAttribute oAttr)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            //tracing
            // if tracing is on (which is a config switch for this user) then
            //   oCResult.Trace.Write
            //ALL OK?
            try
            {
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIBOAttribute");
                oBOI.BOD = oBOD;
                oBOI.SetAttribute("ID", oAttr.ID.ToString());
                oBOI.SetAttribute("BOID", oAttr.BOID.ToString());
                oBOI.SetAttribute("Name", oAttr.Name);
                oBOI.SetAttribute("LabelName", oAttr.LabelName);
                oBOI.SetAttribute("Format", oAttr.Format);
                oBOI.SetAttribute("FKTableName", oAttr.FKTableName);
                oBOI.SetAttribute("sFKBOName", oAttr.sFKBOName);
                oBOI.SetAttribute("Script", oAttr.Script);
                if (oAttr.MaxLength == "-1" || oAttr.MaxLength == "max")
                {
                    oBOI.SetAttribute("MaxLength", "MAX");
                }
                else
                {
                    if (oAttr.MaxLength != null)
                    {
                        oBOI.SetAttribute("MaxLength", oAttr.MaxLength);
                    }
                    else
                    {
                        oBOI.SetAttribute("MaxLength", "0");
                    }
                }
                oBOI.SetAttribute("TypeID", oAttr.TypeID.ToString());
                oBOI.SetAttribute("IsVisible", oAttr.IsVisible.ToString());
                oBOI.SetAttribute("IsWhere", oAttr.IsWhere.ToString());
                oBOI.SetAttribute("IsTotal", oAttr.IsTotal.ToString());
                oBOI.SetAttribute("IsGroupBy", oAttr.IsGroupBy.ToString());
                oBOI.SetAttribute("IsOrderBy", oAttr.IsOrderBy.ToString());
                oBOI.SetAttribute("IsExpression", oAttr.IsExpression.ToString());
                oBOI.SetAttribute("IsMail", oAttr.IsMail.ToString());
                oBOI.SetAttribute("StatusTypeID", oAttr.StatusTypeID.ToString());
                oBOI.SetAttribute("Description", oAttr.Description);
                oBOI.SetAttribute("CreatedByName", oAttr.CreatedByName);
                oBOI.SetAttribute("CreatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                oBOI.SetAttribute("CreatedByID", iUserID.ToString());
                oBOI.SetAttribute("CreatedTime", DateTime.Now.ToString());
                if (oAttr.IsWhere == false)
                {
                    oBOI.SetAttribute("IsRunTime", false.ToString());
                    oBOI.SetAttribute("IsDate", false.ToString());
                    oBOI.SetAttribute("DateExpression", null);
                    oBOI.SetAttribute("DateValue", null);
                    oBOI.SetAttribute("IsWhereExpression", false.ToString());
                    oBOI.SetAttribute("WhereExpression", null);
                    oBOI.SetAttribute("WhereExpreValue", null);
                    oBOI.SetAttribute("IsDBValue", false.ToString());
                    oBOI.SetAttribute("DBQuery", null);
                    oBOI.SetAttribute("ExpressionText", null);
                    oBOI.SetAttribute("ExpressionValue", null);
                }
                else
                {
                    oBOI.SetAttribute("IsRunTime", oAttr.IsRunTime.ToString());
                    oBOI.SetAttribute("IsDate", oAttr.IsDate.ToString());
                    oBOI.SetAttribute("DateExpression", oAttr.DateExpression);
                    oBOI.SetAttribute("DateValue", oAttr.DateValue);
                    oBOI.SetAttribute("IsWhereExpression", oAttr.IsWhereExpression.ToString());
                    oBOI.SetAttribute("WhereExpression", oAttr.WhereExpression);
                    oBOI.SetAttribute("WhereExpreValue", oAttr.ExpressionValue);
                    oBOI.SetAttribute("IsDBValue", oAttr.IsDBValue.ToString());
                    oBOI.SetAttribute("DBQuery", oAttr.DBQuery);
                    oBOI.SetAttribute("ExpressionText", oAttr.ExpressionText);
                    oBOI.SetAttribute("ExpressionValue", oAttr.ExpressionValue);
                }
                var XiBO = oBOI.SaveV2(oBOI);
                if (XiBO.bOK && XiBO.oResult != null)
                {
                }
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                oCResult = XiBO;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting BOAttributes" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            return oCResult;
        }

        #endregion BOAttribute 

        #region BO Group

        public CResult Save_BOGroup(XIDGroup oGroup)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            //tracing
            // if tracing is on (which is a config switch for this user) then
            //   oCResult.Trace.Write
            //ALL OK?
            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "BO default groups insertion started" });
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "BO group insertion started for " + oGroup.GroupName });
                }
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIBOGroup");
                oBOI.BOD = oBOD;
                oTrace.oParams.Add(new CNV { sName = "GroupName", sValue = oGroup.GroupName });
                if (!string.IsNullOrEmpty(oGroup.GroupName))
                {
                    oBOI.SetAttribute("ID", oGroup.ID.ToString());
                    oBOI.SetAttribute("BOID", oGroup.BOID.ToString());
                    oBOI.SetAttribute("BOFieldIDs", oGroup.BOFieldIDs);
                    oBOI.SetAttribute("BOSqlFieldNames", oGroup.BOFieldNames);
                    oBOI.SetAttribute("BOFieldNames", oGroup.BOFieldNames);
                    oBOI.SetAttribute("GroupName", oGroup.GroupName);
                    oBOI.SetAttribute("TypeID", "0");
                    oBOI.SetAttribute("IsMultiColumnGroup", "1");
                    oBOI.SetAttribute("Description", oGroup.Description);
                    oBOI.SetAttribute("StatusTypeID", "10");
                    oBOI.SetAttribute("CreatedByID", iUserID.ToString());
                    oBOI.SetAttribute("CreatedByName", iUserID.ToString());
                    oBOI.SetAttribute("CreatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                    oBOI.SetAttribute("CreatedTime", DateTime.Now.ToString());
                    oBOI.SetAttribute("bIsCrtdBy", iUserID.ToString());
                    oBOI.SetAttribute("bIsCrtdWhn", DateTime.Now.ToString());
                    oBOI.SetAttribute("bIsUpdtdBy", iUserID.ToString());
                    oBOI.SetAttribute("bIsUpdtdWhn", DateTime.Now.ToString());
                    oCR = oBOI.SaveV2(oBOI);
                    oTrace.oTrace.Add(oCR.oTrace);
                    if (oCR.bOK && oCR.oResult != null)
                    {
                        oCResult = oCR;
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                        oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                        if (oSigCR != null)
                        {
                            oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "BO group insertion completed for " + oGroup.GroupName });
                        }
                    }
                    else
                    {
                        oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "BO group insertion failed for " + oGroup.GroupName });
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                    }
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory param GroupName is Missing";
                }

            }
            catch (Exception ex)
            {
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "BO group insertion failed" });
                oSigCR.oStack.Add(new CNV { sName = "Error Message", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting BO" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        public CResult Save_BOGroups(XIDGroup oGroup)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name; ;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            //tracing
            // if tracing is on (which is a config switch for this user) then
            //   oCResult.Trace.Write
            //ALL OK?
            int iSearchGroupID = 0;
            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "BO default groups insertion started" });
                }
                string fieldids = ""; string fieldnames = "";
                string sPopFIDs = string.Empty; string sPopFNames = string.Empty;
                if (oGroup.BOID > 0 && oGroup.ID == 0)
                {
                    var oAttrIDs = oXID.Get_BOAttributes(oGroup.BOID.ToString());
                    foreach (var items in oAttrIDs)
                    {
                        if (items.text.ToLower() != XIConstant.Key_XIGUID.ToLower() && items.text.ToLower() != XIConstant.Key_Hierarchy.ToLower())
                        {
                            fieldids = fieldids + items.Value + ", ";
                            fieldnames = fieldnames + items.text + ", ";
                        }
                        if (items.text.ToLower() == XIConstant.Key_ID.ToLower() || items.text.ToLower() == XIConstant.Key_Name.ToLower())
                        {
                            sPopFIDs = sPopFIDs + items.Value + ", ";
                            sPopFNames = sPopFNames + items.text + ", ";
                        }
                    }
                }
                else if (oGroup.BOID > 0 && oGroup.ID > 0)
                {
                    var oGroupDef = oGroup.Get_GroupDetails();
                    fieldids = oGroupDef.BOFieldIDs + ", " + oGroup.BOFieldIDs + ", ";
                    fieldnames = oGroupDef.BOFieldNames + ", " + oGroup.BOFieldNames + ", ";
                }

                fieldids = fieldids.Substring(0, fieldids.Length - 2);
                fieldnames = fieldnames.Substring(0, fieldnames.Length - 2);
                oGroup.BOFieldIDs = fieldids;
                oGroup.BOFieldNames = fieldnames;
                var oFList = new List<XIDGroup>();
                if (string.IsNullOrEmpty(oGroup.GroupName))
                {
                    oFList = new List<XIDGroup> {
                new XIDGroup {GroupName="Create",Description="Create"},
                new XIDGroup {GroupName="Details1",Description="Details1"},
                new XIDGroup {GroupName="Description",Description="Description"},
                new XIDGroup {GroupName="List",Description="List"},
                new XIDGroup {GroupName="Label",Description="Label"},
                new XIDGroup {GroupName="Search",Description="Search"},
                new XIDGroup {GroupName="Quick Search",Description="Quick Search"}
            };
                }
                else
                {
                    oFList = new List<XIDGroup> {
                new XIDGroup {GroupName=oGroup.GroupName,Description=oGroup.GroupName}
                };
                }

                foreach (var items in oFList)
                {
                    oGroup.GroupName = items.GroupName;
                    oGroup.Description = items.Description;
                    oCR = Save_BOGroup(oGroup);
                    oTrace.oTrace.Add(oCR.oTrace);
                    if (oCR.bOK && oCR.oResult != null)
                    {
                        if (items.GroupName.ToLower() == "search")
                        {
                            var GroupI = (XIIBO)oCR.oResult;
                            var ID = GroupI.AttributeI("ID").sValue;
                            int.TryParse(ID, out iSearchGroupID);
                        }
                    }
                    else
                    {
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                    }
                }
                //Create Popup group with ID and sName
                XIDGroup oPopGrp = new XIDGroup();
                oPopGrp.BOFieldIDs = sPopFIDs.Substring(0, sPopFIDs.Length - 2);
                oPopGrp.BOFieldNames = sPopFNames.Substring(0, sPopFNames.Length - 2);
                oPopGrp.GroupName = "Popup";
                oPopGrp.Description = "Popup";
                oPopGrp.IsMultiColumnGroup = true;
                oPopGrp.BOSqlFieldNames = sPopFNames.Substring(0, sPopFNames.Length - 2);
                oPopGrp.BOID = oGroup.BOID;
                oCR = Save_BOGroup(oPopGrp);
                oTrace.oTrace.Add(oCR.oTrace);
                if (oCR.bOK && oCR.oResult != null)
                {
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                }
            }
            catch (Exception ex)
            {
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "BO group insertion failed" });
                oSigCR.oStack.Add(new CNV { sName = "Error Message", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting BO" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            if (oTrace.iStatus != (int)xiEnumSystem.xiFuncResult.xiError)
            {
                oCResult.oResult = iSearchGroupID;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        #endregion BO Group

        #region BO Structure

        public CResult Save_BOStructure(List<XIDStructure> oStructure)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name; ;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            var sParent = string.Empty;
            try
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "BO Structure insertion started" });
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "BO Structure insertion started" });
                }
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XI BOStructure");
                oBOI.BOD = oBOD;
                var iParentID = oStructure.Where(m => m.FKiParentID == "#").Select(m => m.ID).FirstOrDefault();
                Dictionary<string, object> BOSParam = new Dictionary<string, object>();
                if (iParentID > 0)
                {
                    BOSParam["FKiParentID"] = iParentID.ToString();
                    var oStruList = Connection.Select<XIDStructure>("XIBOStructure_T", BOSParam).ToList();
                    var ExistIDs = oStruList.Select(m => m.ID).ToList();
                    var NewIDs = oStructure.Select(m => m.ID).ToList();
                    var RemoveIDs = ExistIDs.Except(NewIDs).ToList();
                    if (RemoveIDs.Count() > 0)
                    {
                        foreach (var Remove in RemoveIDs)
                        {
                            Dictionary<string, object> Param = new Dictionary<string, object>();
                            Param["ID"] = Remove.ToString();
                            var oRemove = Connection.Select<XIDStructure>("XIBOStructure_T", Param).FirstOrDefault();
                            Connection.Delete<XIDStructure>(oRemove, "XIBOStructure_T", "ID");
                        }
                    }
                }
                Dictionary<string, object> BOParam = new Dictionary<string, object>();
                var oBOs = Connection.Select<XIDBO>("XIBO_T_N", BOParam).ToList();
                if (oBOs != null && oBOs.Count() > 0)
                {

                }
                else
                {
                    oBOs = new List<XIDBO>();
                }
                foreach (var Stru in oStructure)
                {
                    oTrace.oParams.Add(new CNV { sName = "sStructureName", sValue = Stru.sStructureName });
                    if (Stru.sStructureName != null)
                    {
                        var SaveIDs = oStructure.Select(m => m.ID).ToList();
                        sBOName = oBOs.Where(m => m.BOID == Stru.BOID).Select(m => m.Name).FirstOrDefault();
                        if (Stru.sSavingType == "Add")
                        {
                            oBOI.SetAttribute("ID", "0");
                        }
                        else
                        {
                            oBOI.SetAttribute("ID", Stru.ID.ToString());
                        }
                        if (Stru.FKiParentID == "#")
                        {
                            oBOI.SetAttribute("FKiParentID", "#");
                        }
                        else
                        {
                            oBOI.SetAttribute("FKiParentID", sParent);
                        }
                        oBOI.SetAttribute("sStructureName", Stru.sStructureName);
                        oBOI.SetAttribute("sCode", Stru.sCode);
                        oBOI.SetAttribute("sName", Stru.sName);
                        oBOI.SetAttribute("BOID", Stru.BOID.ToString());
                        oBOI.SetAttribute("sBO", sBOName);
                        oBOI.SetAttribute("sMode", Stru.sMode);
                        oBOI.SetAttribute("FKi1ClickID", "0");
                        oBOI.SetAttribute("FKiXIApplicationID", iAppID.ToString());
                        oBOI.SetAttribute("bMasterEntity", Stru.bMasterEntity.ToString());
                        oBOI.SetAttribute("sType", Stru.sType);
                        oBOI.SetAttribute("bIsAutoCreateDone", Stru.bIsAutoCreateDone.ToString());
                        oBOI.SetAttribute("iOrder", "0");
                        oBOI.SetAttribute("bIsVisible", "false");
                        oBOI.SetAttribute("sParentFKColumn", Stru.sParentFKColumn);
                        oBOI.SetAttribute("sLinkingType", Stru.sLinkingType);
                        oBOI.SetAttribute("FKiStepDefinitionID", Stru.FKiStepDefinitionID);
                        oBOI.SetAttribute("sOutputArea", Stru.sOutputArea);
                        oBOI.SetAttribute("OrganisationID", iOrgID.ToString());
                        oBOI.SetAttribute("StatusTypeID", Stru.StatusTypeID.ToString());
                        oBOI.SetAttribute("CreatedBy", iUserID.ToString());
                        oBOI.SetAttribute("CreatedTime", DateTime.Now.ToString());
                        oBOI.SetAttribute("CreatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                        oBOI.SetAttribute("UpdatedBy", iUserID.ToString());
                        oBOI.SetAttribute("UpdatedTime", DateTime.Now.ToString());
                        oBOI.SetAttribute("UpdatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                        oCR = oBOI.SaveV2(oBOI);
                        oTrace.oTrace.Add(oCR.oTrace);
                        if (oCR.bOK && oCR.oResult != null)
                        {
                            if (oSigCR != null)
                            {
                                oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "BO structure insertion completed for " + sBOName });
                            }
                            oBOI = (XIIBO)oCR.oResult;
                            sParent = oBOI.AttributeI("FKiParentID").sValue;
                            if (sParent == "#")
                            {
                                sParent = oBOI.AttributeI(oBOD.sPrimaryKey).sValue;
                            }
                            //sStructureName = oBOI.Attributes.Where(m => m.Key.ToLower() == "sname").Select(m => m.Value).Select(m => m.sValue).FirstOrDefault();
                            sStructureName = oBOI.Attributes.Where(m => m.Key.ToLower() == "scode").Select(m => m.Value).Select(m => m.sValue).FirstOrDefault();
                            if (oTrace.iStatus != (int)xiEnumSystem.xiFuncResult.xiError)
                            {
                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                            }
                        }
                        else
                        {
                            oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "BO structure insertion failed for " + sBOName });
                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                            oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                        }
                    }
                    else
                    {
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                        oTrace.sMessage = "Mandatory param sStructureName is missing";
                    }
                }

                //oCResult.oResult = oBOI;
            }
            catch (Exception ex)
            {
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "BO structure insertion failed" });
                oSigCR.oStack.Add(new CNV { sName = "Error Message", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting BOStructure" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            if (oTrace.iStatus != (int)xiEnumSystem.xiFuncResult.xiError)
            {
                oCResult.oResult = sParent;
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        #endregion BO Structure

        #region BO Script

        public CResult Save_BOScript(XIDScript oScript)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            try
            {
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XiScript_T");
                oBOI.BOD = oBOD;
                oBOI.SetAttribute("ID", oScript.ID.ToString());
                oBOI.SetAttribute("FKiBOID", oScript.FKiBOID.ToString());
                oBOI.SetAttribute("FKiBOAttributeID", oScript.FKiBOAttributeID.ToString());
                oBOI.SetAttribute("sName", oScript.sName);
                oBOI.SetAttribute("sDescription", oScript.sDescription);
                oBOI.SetAttribute("sType", oScript.sType);
                oBOI.SetAttribute("sLanguage", oScript.sLanguage);
                oBOI.SetAttribute("sScript", oScript.sScript);
                oBOI.SetAttribute("sCategory", oScript.sCategory);
                oBOI.SetAttribute("sClassification", oScript.sClassification);
                oBOI.SetAttribute("sLevel", oScript.sLevel);
                oBOI.SetAttribute("sVersion", oScript.sVersion);
                oBOI.SetAttribute("sMethodName", oScript.sMethodName);
                oBOI.SetAttribute("FKiApplicationID", iAppID.ToString());
                oBOI.SetAttribute("OrganisationID", iOrgID.ToString());
                oBOI.SetAttribute("StatusTypeID", oScript.StatusTypeID.ToString());
                oBOI.SetAttribute("CreatedBy", iUserID.ToString());
                oBOI.SetAttribute("CreatedTime", DateTime.Now.ToString());
                oBOI.SetAttribute("CreatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                oBOI.SetAttribute("UpdatedBy", iUserID.ToString());
                oBOI.SetAttribute("UpdatedTime", DateTime.Now.ToString());
                oBOI.SetAttribute("UpdatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                var XiBO = oBOI.SaveV2(oBOI);
                if (oScript.ScriptResults != null && oScript.ScriptResults.Count() > 0)
                {
                    oBOI = (XIIBO)XiBO.oResult;
                    var iScriptID = oBOI.Attributes.Where(m => m.Key.ToLower() == "id").Select(m => m.Value).Select(m => m.sValue).FirstOrDefault();
                    Dictionary<string, object> OptParam = new Dictionary<string, object>();
                    OptParam["FKiScriptID"] = oScript.ID;
                    var ExistingResults = Connection.Select<XIDScriptResult>("XIBOScriptResult_T", OptParam).ToList();
                    if (ExistingResults.Count() > 0)
                    {
                        //dbContext.BOScriptResults.RemoveRange(ExistingResults);
                        //dbContext.SaveChanges();
                    }
                    foreach (var script in oScript.ScriptResults)
                    {
                        if (script.sResultCode != null && script.iAction > 0 && script.iType > 0)
                        {
                            XIIBO oBOI1 = new XIIBO();
                            XIDBO oBOD1 = new XIDBO();
                            oBOD1 = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIBOScriptResult");
                            oBOI1.BOD = oBOD1;
                            oBOI1.SetAttribute("ID", script.ID.ToString());
                            oBOI1.SetAttribute("FKiScriptID", iScriptID);
                            oBOI1.SetAttribute("sResultCode", script.sResultCode);
                            oBOI1.SetAttribute("iType", script.iType.ToString());
                            oBOI1.SetAttribute("iAction", script.iAction.ToString());
                            oBOI1.SetAttribute("sUserError", script.sUserError);
                            var XiBO1 = oBOI1.SaveV2(oBOI1);
                        }
                    }
                    oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                    oCResult = XiBO;
                }
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting BOScripts" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            return oCResult;
        }

        #endregion BO Script

        #region Layout

        public CResult Save_Layout(XIDLayout oLayout)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name; ;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "Layout insertion started" });
                }
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XI Layout");
                oBOI.BOD = oBOD;
                oTrace.oParams.Add(new CNV { sName = "LayoutType", sValue = oLayout.LayoutType });
                if (!string.IsNullOrEmpty(oLayout.LayoutName))
                {
                    oBOI.SetAttribute("ID", oLayout.ID.ToString());
                    oBOI.SetAttribute("OrganizationID", oLayout.OrganizationID.ToString());
                    oBOI.SetAttribute("FKiApplicationID", oLayout.FKiApplicationID.ToString());
                    oBOI.SetAttribute("LayoutName", oLayout.LayoutName);
                    oBOI.SetAttribute("LayoutType", oLayout.LayoutType);
                    oBOI.SetAttribute("LayoutCode", oLayout.LayoutCode);//XIConstant.FormLayoutTemplate
                    oBOI.SetAttribute("XiParameterID", oLayout.XiParameterID.ToString());
                    oBOI.SetAttribute("LayoutLevel", oLayout.LayoutLevel);
                    oBOI.SetAttribute("Authentication", oLayout.Authentication);
                    oBOI.SetAttribute("iThemeID", oLayout.iThemeID.ToString());// "157"
                    oBOI.SetAttribute("bUseParentGUID", oLayout.bUseParentGUID.ToString());
                    oBOI.SetAttribute("StatusTypeID", oLayout.StatusTypeID.ToString());
                    oBOI.SetAttribute("CreatedBy", iUserID.ToString());
                    oBOI.SetAttribute("CreatedTime", DateTime.Now.ToString());
                    oBOI.SetAttribute("CreatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                    oBOI.SetAttribute("UpdatedBy", iUserID.ToString());
                    oBOI.SetAttribute("UpdatedTime", DateTime.Now.ToString());
                    oBOI.SetAttribute("UpdatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                    oBOI.SetAttribute("sSiloAccess", oLayout.sSiloAccess);
                    oBOI.SetAttribute("bIsTaskBar", oLayout.bIsTaskBar.ToString());
                    oBOI.SetAttribute("sTaskBarPosition", oLayout.sTaskBarPosition);
                    oBOI.SetAttribute("bAddToParentTaskbar", oLayout.bAddToParentTaskbar.ToString());
                    oCR = oBOI.SaveV2(oBOI);
                    oTrace.oTrace.Add(oCR.oTrace);
                    if (oCR.bOK && oCR.oResult != null)
                    {
                        if (oSigCR != null)
                        {
                            oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "Layout insertion completed" });
                        }
                        oBOI = (XIIBO)oCR.oResult;
                        oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                        oCResult.oResult = oBOI;
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                    }
                    else
                    {
                        oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Layout insertion failed" });
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                    }
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory param LayoutName is Missing";
                }

            }
            catch (Exception ex)
            {
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Layout insertion failed" });
                oSigCR.oStack.Add(new CNV { sName = "Error Message", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting Layout" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        public CResult Save_LayoutDetail(XIDLayoutDetails oDetail)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name; ;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "Layout detail insertion started" });
                }
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XILayoutDetails");
                oBOI.BOD = oBOD;
                oTrace.oParams.Add(new CNV { sName = "LayoutID", sValue = oDetail.LayoutID.ToString() });
                if (oDetail.LayoutID != 0)
                {
                    oBOI.SetAttribute("PlaceHolderID", oDetail.PlaceHolderID.ToString());
                    oBOI.SetAttribute("FKiApplicationID", oDetail.FKiApplicationID.ToString());
                    oBOI.SetAttribute("LayoutID", oDetail.LayoutID.ToString());
                    oBOI.SetAttribute("PlaceholderName", oDetail.PlaceholderName);
                    oBOI.SetAttribute("PlaceholderArea", oDetail.PlaceholderArea);
                    oBOI.SetAttribute("PlaceholderUniqueName", oDetail.PlaceholderUniqueName);
                    oBOI.SetAttribute("PlaceholderClass", oDetail.PlaceholderClass);
                    oCR = oBOI.SaveV2(oBOI);
                    oTrace.oTrace.Add(oCR.oTrace);
                    if (oCR.bOK && oCR.oResult != null)
                    {
                        if (oSigCR != null)
                        {
                            oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "Layout detail insertion completed" });
                        }
                        oBOI = (XIIBO)oCR.oResult;
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                        oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                        oCResult.oResult = oBOI;
                    }
                    else
                    {
                        oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Layout detail insertion failed" });
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                    }
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory param LayoutID is Missing";
                }

            }
            catch (Exception ex)
            {
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Layout detail insertion failed" });
                oSigCR.oStack.Add(new CNV { sName = "Error Message", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting Layout Details" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        public CResult Save_LayoutMapping(XIDLayoutMapping oMap, string sPlaceholderName = "")
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name; ;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "Layout mapping insertion started" });
                }
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XI LayoutMapping");
                oBOI.BOD = oBOD;
                oTrace.oParams.Add(new CNV { sName = "PopupID", sValue = oMap.PopupID.ToString() });
                if (oMap.PopupLayoutID != 0)
                {
                    oBOI.SetAttribute("ID", oMap.ID.ToString());
                    oBOI.SetAttribute("FKiApplicationID", oMap.FKiApplicationID.ToString());
                    oBOI.SetAttribute("PopupID", oMap.PopupID.ToString());
                    oBOI.SetAttribute("PopupLayoutID", oMap.PopupLayoutID.ToString());
                    oBOI.SetAttribute("PlaceHolderID", oMap.PlaceHolderID.ToString());
                    oBOI.SetAttribute("XiLinkID", oMap.XiLinkID.ToString());
                    if (!string.IsNullOrEmpty(sPlaceholderName))
                    {
                        if (sPlaceholderName.ToLower() == "inbox")
                        {
                            oBOI.SetAttribute("XiLinkID", "16");
                        }
                        else if (sPlaceholderName.ToLower() == "menu")
                        {
                            oBOI.SetAttribute("XiLinkID", "15");
                            //oBOI.SetAttribute("XiLinkID", "23");
                        }
                    }
                    oBOI.SetAttribute("Type", oMap.Type);
                    oBOI.SetAttribute("StatusTypeID", oMap.StatusTypeID.ToString());
                    oBOI.SetAttribute("ContentType", oMap.ContentType);
                    oBOI.SetAttribute("HTMLCode", oMap.HTMLCode);
                    oBOI.SetAttribute("IsValueSet", "0");
                    oBOI.SetAttribute("CreatedBy", iUserID.ToString());
                    oBOI.SetAttribute("CreatedTime", DateTime.Now.ToString());
                    oBOI.SetAttribute("CreatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                    oBOI.SetAttribute("UpdatedBy", iUserID.ToString());
                    oBOI.SetAttribute("UpdatedTime", DateTime.Now.ToString());
                    oBOI.SetAttribute("UpdatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                    oBOI.SetAttribute("OrganisationID", iOrgID.ToString());
                    var XiBO = oBOI.SaveV2(oBOI);
                    oTrace.oTrace.Add(oCR.oTrace);
                    if (XiBO.bOK && XiBO.oResult != null)
                    {
                        oBOI = (XIIBO)XiBO.oResult;
                        if (oSigCR != null)
                        {
                            oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "Layout mapping insertion completed" });
                        }
                        oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                        oCResult.oResult = oBOI;
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                    }
                    else
                    {
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                        oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Layout mapping insertion failed" });
                    }
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory param PopupID is Missing";
                }

            }
            catch (Exception ex)
            {
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Layout mapping insertion failed" });
                oSigCR.oStack.Add(new CNV { sName = "Error Message", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while loading BO definition" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        public CResult Copy_Layout(XIDLayout oLayout, bool bUseParent)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name; ;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            try
            {
                XIDLayout oXIDL = new XIDLayout();
                List<XIDLayoutDetails> oDetails = new List<XIDLayoutDetails>();
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XI Layout");
                oBOI.BOD = oBOD;
                oLayout.bUseParentGUID = bUseParent;
                oLayout.ID = 0;
                oCR = Save_Layout(oLayout);
                oTrace.oTrace.Add(oCR.oTrace);
                if (oCR.bOK && oCR.oResult != null)
                {
                    oBOI = (XIIBO)oCR.oResult;
                    var sLayoutID = oBOI.Attributes.Where(m => m.Key.ToLower() == "id").Select(s => s.Value).Select(m => m.sValue).FirstOrDefault();
                    XIIBO oBOI1 = new XIIBO();
                    XIDBO oBOD1 = new XIDBO();
                    if (oLayout.LayoutDetails != null && oLayout.LayoutDetails.Count() > 0)
                    {

                        oBOD1 = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XILayoutDetails");
                        oBOI1.BOD = oBOD1;
                        foreach (var oDetail in oLayout.LayoutDetails)
                        {
                            oDetail.PlaceHolderID = 0;
                            oDetail.LayoutID = Convert.ToInt32(sLayoutID);
                            oCR = Save_LayoutDetail(oDetail);
                            oTrace.oTrace.Add(oCR.oTrace);
                            if (oCR.bOK && oCR.oResult != null)
                            {
                                oBOI1 = (XIIBO)oCR.oResult;
                                var sPlaceHolderID = oBOI1.Attributes.Where(m => m.Key.ToLower() == "placeholderid").Select(s => s.Value).Select(m => m.sValue).FirstOrDefault();
                                if (oTrace.iStatus != (int)xiEnumSystem.xiFuncResult.xiError)
                                {
                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                                    oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                                    oCResult.oResult = oBOI;

                                }
                            }
                            else
                            {
                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                            }
                        }
                    }
                    else
                    {
                        oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                        oCResult.oResult = oBOI;
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                    }
                }
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting Layout" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        #endregion Layout

        #region Dialog

        public CResult Save_Dialog(XIDDialog oDialog)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name; ;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "Dialog insertion started" });
                }
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XI Dialog");
                oBOI.BOD = oBOD;
                oTrace.oParams.Add(new CNV { sName = "DialogName", sValue = oDialog.DialogName + " Dialog" });
                if (!string.IsNullOrEmpty(oDialog.DialogName))
                {
                    oBOI.SetAttribute("ID", oDialog.ID.ToString());
                    oBOI.SetAttribute("DialogName", oDialog.DialogName);
                    oBOI.SetAttribute("StatusTypeID", oDialog.StatusTypeID.ToString());
                    oBOI.SetAttribute("LayoutID", oDialog.LayoutID.ToString());
                    oBOI.SetAttribute("CreatedTime", DateTime.Now.ToString());
                    oBOI.SetAttribute("CreatedBy", iUserID.ToString());
                    oBOI.SetAttribute("CreatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                    oBOI.SetAttribute("UpdatedBy", iUserID.ToString());
                    oBOI.SetAttribute("UpdatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                    oBOI.SetAttribute("UpdatedTime", DateTime.Now.ToString());
                    oBOI.SetAttribute("IsResizable", oDialog.IsResizable.ToString());
                    oBOI.SetAttribute("IsCloseIcon", oDialog.IsCloseIcon.ToString());
                    oBOI.SetAttribute("IsMinimiseIcon", oDialog.IsMinimiseIcon.ToString());
                    oBOI.SetAttribute("IsPinIcon", oDialog.IsPinIcon.ToString());
                    oBOI.SetAttribute("IsMaximiseIcon", oDialog.IsMaximiseIcon.ToString());
                    oBOI.SetAttribute("IsGrouping", oDialog.IsGrouping.ToString());
                    //oBOI.SetAttribute("BarPosition", oDialog.BarPosition);
                    oBOI.SetAttribute("DialogWidth", oDialog.DialogWidth.ToString());
                    oBOI.SetAttribute("DialogHeight", oDialog.DialogHeight.ToString());
                    oBOI.SetAttribute("PopupSize", oDialog.PopupSize);
                    oBOI.SetAttribute("DialogMy1", oDialog.DialogMy1);
                    oBOI.SetAttribute("DialogMy2", oDialog.DialogMy2);
                    oBOI.SetAttribute("DialogAt1", oDialog.DialogAt1);
                    oBOI.SetAttribute("DialogAt2", oDialog.DialogAt2);
                    oBOI.SetAttribute("FKiQSDefinitionID", oDialog.FKiQSDefinitionID.ToString());
                    oBOI.SetAttribute("iTransparency", oDialog.iTransparency.ToString());
                    oBOI.SetAttribute("FKiApplicationID", oDialog.FKiApplicationID.ToString());
                    oBOI.SetAttribute("OrganizationID", oDialog.OrganizationID.ToString());
                    oBOI.SetAttribute("FKiBOID", oDialog.FKiBOID.ToString());
                    if (oDialog.IsGrouping == false && oDialog.ID != 0)
                    {
                        oBOI.Attributes["BarPosition".ToLower()] = new XIIAttribute { sName = "BarPosition", sValue = null, bDirty = true };
                    }
                    else
                    {
                        oBOI.Attributes["BarPosition".ToLower()] = new XIIAttribute { sName = "BarPosition", sValue = oDialog.BarPosition, bDirty = true };
                    }
                    oCR = oBOI.SaveV2(oBOI);
                    oTrace.oTrace.Add(oCR.oTrace);
                    if (oCR.bOK && oCR.oResult != null)
                    {
                        oBOI = (XIIBO)oCR.oResult;
                        if (oSigCR != null)
                        {
                            oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "Dialog insertion completed" });
                        }
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                        oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                        oCResult.oResult = oBOI;
                    }
                    else
                    {
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                        oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Dialog insertion failed" });
                    }
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory param DialogName is Missing";
                }
            }
            catch (Exception ex)
            {
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Dialog insertion failed" });
                oSigCR.oStack.Add(new CNV { sName = "Error Messaged", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        #endregion Dialog

        #region XILink

        public CResult Interpret_XILink(string XICode)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name; ;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "XILink building started" });
                }
                string XITask = string.Empty;
                string sXILinkName = string.Empty;
                int iXILinkID = 0;
                if (!string.IsNullOrEmpty(XICode))
                {
                    var CodeLines = XICode.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (CodeLines != null && CodeLines.Count() > 0)
                    {
                        var XIKeyWord = CodeLines.FirstOrDefault();
                        if (XIKeyWord.StartsWith("XI."))
                        {
                            XITask = XIKeyWord.Split('.')[1];
                        }
                        if (XITask.Contains('-'))
                        {
                            sXILinkName = XITask.Split('-')[1].Trim();
                        }
                        CodeLines.RemoveAt(0);
                        if (!string.IsNullOrEmpty(XITask) && XITask.ToLower().StartsWith("xilink"))
                        {
                            XILink oXILink = new XILink();
                            oXILink.FKiApplicationID = iAppID;
                            oXILink.Name = sXILinkName;
                            oXILink.URL = "XILink";
                            oXILink.sActive = "Y";
                            oXILink.StatusTypeID = 10;
                            oCR = Save_XILink(oXILink);
                            oTrace.oTrace.Add(oCR.oTrace);
                            if (oCR.bOK && oCR.oResult != null)
                            {
                                var oXILinkI = (XIIBO)oCR.oResult;
                                var sXILinkID = oXILinkI.Attributes.Values.Where(m => m.sName.ToLower() == "XILinkID".ToLower()).Select(m => m.sValue).FirstOrDefault();
                                int.TryParse(sXILinkID, out iXILinkID);
                                if (iXILinkID > 0)
                                {
                                    for (int k = 0; k < CodeLines.Count(); k++)
                                    {
                                        if (!string.IsNullOrEmpty(CodeLines[k]))
                                        {
                                            if (CodeLines[k].StartsWith("\t\t"))
                                            { }
                                            else if (CodeLines[k].StartsWith("\t"))
                                            { }
                                            else
                                            { }
                                        }
                                    }
                                    XiLinkNV oNV = new XiLinkNV();
                                    oNV.XiLinkID = iXILinkID;
                                    oNV.Name = "StartAction";
                                    oNV.Value = "QuestionSet";
                                    oCR = Save_XILinkNV(oNV);
                                    oTrace.oTrace.Add(oCR.oTrace);
                                    if (oCR.bOK && oCR.oResult != null)
                                    {
                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                                    }
                                    else
                                    {
                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                    }
                                    oNV = new XiLinkNV();
                                    oNV.XiLinkID = iXILinkID;
                                    oNV.Name = "QuestionSetID";
                                    //oNV.Value = iQSDID.ToString();
                                    oCR = Save_XILinkNV(oNV);
                                    oTrace.oTrace.Add(oCR.oTrace);
                                    if (oCR.bOK && oCR.oResult != null)
                                    {
                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                                    }
                                    else
                                    {
                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                    }

                                }
                            }
                            else
                            {
                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                            }
                        }
                    }
                }
                if (oTrace.iStatus != (int)xiEnumSystem.xiFuncResult.xiError)
                {
                    oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                }

            }
            catch (Exception ex)
            {
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "XILink insertion failed" });
                oSigCR.oStack.Add(new CNV { sName = "Error Message", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting Xilink" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        public CResult Save_XILink(XILink oXILink)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name; ;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "XILink insertion Started" });
                }
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XI Link");
                oBOI.BOD = oBOD;
                oTrace.oParams.Add(new CNV { sName = "Name", sValue = oXILink.Name });
                if (!string.IsNullOrEmpty(oXILink.Name))
                {
                    oBOI.SetAttribute("XiLinkID", oXILink.XiLinkID.ToString());
                    oBOI.SetAttribute("Name", oXILink.Name);
                    oBOI.SetAttribute("URL", oXILink.URL);
                    oBOI.SetAttribute("FKiComponentID", oXILink.FKiComponentID.ToString());
                    oBOI.SetAttribute("OneClickID", oXILink.OneClickID.ToString());
                    oBOI.SetAttribute("StatusTypeID", oXILink.StatusTypeID.ToString());
                    oBOI.SetAttribute("CreatedBy", iUserID.ToString());
                    oBOI.SetAttribute("CreatedTime", DateTime.Now.ToString());
                    oBOI.SetAttribute("CreatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                    oBOI.SetAttribute("UpdatedBy", iUserID.ToString());
                    oBOI.SetAttribute("UpdatedTime", DateTime.Now.ToString());
                    oBOI.SetAttribute("UpdatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                    oBOI.SetAttribute("FKiApplicationID", oXILink.FKiApplicationID.ToString());
                    oBOI.SetAttribute("sActive", oXILink.sActive);
                    oBOI.SetAttribute("sType", oXILink.sType);
                    oBOI.SetAttribute("OrganisationID", oXILink.OrganisationID.ToString());
                    oCR = oBOI.SaveV2(oBOI);
                    oTrace.oTrace.Add(oCR.oTrace);
                    if (oCR.bOK && oCR.oResult != null)
                    {
                        oBOI = (XIIBO)oCR.oResult;
                        if (oSigCR != null)
                        {
                            oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "XILink insertion completed" });
                        }
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                        oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                        oCResult.oResult = oBOI;
                    }
                    else
                    {
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                        oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "XILink insertion failed" });
                    }
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory param XILink Name is Missing";
                }
            }
            catch (Exception ex)
            {
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "XILink insertion failed" });
                oSigCR.oStack.Add(new CNV { sName = "Error Message", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting Xilink" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        public CResult Save_XILinkNV(XiLinkNV oXILinkNV)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name; ;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "XILink NV insertion Started" });
                }
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XILinkNVs");
                oBOI.BOD = oBOD;
                oTrace.oParams.Add(new CNV { sName = "XiLinkID", sValue = oXILinkNV.XiLinkID.ToString() });
                if (oXILinkNV.XiLinkID != 0)
                {
                    oBOI.SetAttribute("ID", oXILinkNV.ID.ToString());
                    oBOI.SetAttribute("XiLinkID", oXILinkNV.XiLinkID.ToString());
                    oBOI.SetAttribute("XiLinkListID", "0");
                    oBOI.SetAttribute("Name", oXILinkNV.Name);
                    oBOI.SetAttribute("Value", oXILinkNV.Value);
                    oBOI.SetAttribute("StatusTypeID", "10");
                    oBOI.SetAttribute("CreatedBy", iUserID.ToString());
                    oBOI.SetAttribute("CreatedTime", DateTime.Now.ToString());
                    oBOI.SetAttribute("CreatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                    oBOI.SetAttribute("UpdatedBy", iUserID.ToString());
                    oBOI.SetAttribute("UpdatedTime", DateTime.Now.ToString());
                    oBOI.SetAttribute("UpdatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                    oCR = oBOI.SaveV2(oBOI);
                    oTrace.oTrace.Add(oCR.oTrace);
                    if (oCR.bOK && oCR.oResult != null)
                    {
                        oBOI = (XIIBO)oCR.oResult;
                        if (oSigCR != null)
                        {
                            oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "XILink NV insertion completed" });
                        }
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                        oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                        oCResult.oResult = oBOI;
                    }
                    else
                    {
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                        oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "XILink NV insertion failed" });
                    }
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory param XiLinkID is Missing";
                }

            }
            catch (Exception ex)
            {
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "XILink NV insertion failed" });
                oSigCR.oStack.Add(new CNV { sName = "Error Message", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting Xilink" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        #endregion XILink

        #region 1-Click

        public CResult Save_1Click(XID1Click o1Click)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Save 1Click";
            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "Deafult 1-Click insertion started for " + sBOName });
                }
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XI1Click");
                oBOI.BOD = oBOD;
                oTrace.oParams.Add(new CNV { sName = "Name", sValue = o1Click.Name });
                if (!string.IsNullOrEmpty(o1Click.Name))
                {
                    oBOI.SetAttribute("ID", o1Click.ID.ToString());
                    oBOI.SetAttribute("OrganizationID", o1Click.OrganizationID.ToString());
                    oBOI.SetAttribute("iLevel", "0");
                    if (o1Click.OrganizationID > 0)
                    {
                        oBOI.SetAttribute("iLevel", "10");
                    }
                    oBOI.SetAttribute("CategoryID", o1Click.CategoryID.ToString());
                    oBOI.SetAttribute("BOID", o1Click.BOID.ToString());
                    oBOI.SetAttribute("ParentID", "0");
                    oBOI.SetAttribute("Name", o1Click.Name);
                    oBOI.SetAttribute("Title", o1Click.Name);
                    oBOI.SetAttribute("Code", o1Click.Code);
                    oBOI.SetAttribute("TypeID", "1");
                    oBOI.SetAttribute("IsCreate", o1Click.IsCreate.ToString());
                    var oBODDef = oXID.Get_BODefinition(null, iBODID.ToString());
                    XIDBO oBODef = new XIDBO();
                    if (oBODDef.bOK && oBODDef.oResult != null)
                    {
                        oBODef = (XIDBO)oBODDef.oResult;
                    }
                    var iGroupID = oBODef.Groups.Where(m => m.Key.ToLower() == XIConstant.CreateGroup.ToLower()).Select(m => m.Value).Select(m => m.ID).FirstOrDefault();
                    if (iGroupID > 0)
                    {
                        oBOI.SetAttribute("CreateGroupID", iGroupID.ToString());
                    }
                    var BOFieldNames = oBODef.Groups.Where(m => m.Key.ToLower() == XIConstant.SearchGroup.ToLower()).Select(m => m.Value).Select(m => m.BOFieldNames).FirstOrDefault();
                    if (!string.IsNullOrEmpty(BOFieldNames))
                    {
                        oBOI.SetAttribute("SearchFields", BOFieldNames);
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(oBODef.sPrimaryKey))
                        {
                            sPrimaryKey = oBODef.sPrimaryKey;
                        }
                        oBOI.SetAttribute("SearchFields", sPrimaryKey);
                    }
                    //var DescriptionFields = oBODef.Groups.Where(m => m.Key.ToLower() == XIConstant.ListGroup.ToLower()).Select(m => m.Value).Select(m => m.BOFieldNames).FirstOrDefault();
                    var sQuery = "Select {List} from " + sTableName + " order by " + oBODef.sPrimaryKey + " desc";
                    if (o1Click.Query != null)
                    {
                        oBOI.SetAttribute("Query", o1Click.Query);
                        oBOI.SetAttribute("VisibleQuery", o1Click.Query);
                    }
                    else
                    {
                        oBOI.SetAttribute("Query", sQuery);
                        oBOI.SetAttribute("VisibleQuery", sQuery);
                    }
                    oBOI.SetAttribute("FromBos", o1Click.FromBos);
                    oBOI.SetAttribute("DisplayAs", o1Click.DisplayAs.ToString());
                    oBOI.SetAttribute("ResultListDisplayType", "1");
                    oBOI.SetAttribute("Class", "43");
                    oBOI.SetAttribute("IsDynamic", "0");
                    oBOI.SetAttribute("IsStoredProcedure", "0");
                    oBOI.SetAttribute("InnerReportID", "0");
                    oBOI.SetAttribute("ActionFields", null);
                    oBOI.SetAttribute("ActionFieldValue", null);
                    oBOI.SetAttribute("ViewFields", null);
                    oBOI.SetAttribute("SelectFields", o1Click.SelectFields);
                    oBOI.SetAttribute("WhereFields", o1Click.WhereFields);
                    oBOI.SetAttribute("GroupFields", o1Click.GroupFields);
                    oBOI.SetAttribute("OrderFields", o1Click.OrderFields);
                    oBOI.SetAttribute("EditableFields", null);
                    oBOI.SetAttribute("NonEditableFields", null);
                    oBOI.SetAttribute("Description", o1Click.Description);
                    oBOI.SetAttribute("IsFilterSearch", o1Click.IsFilterSearch.ToString());
                    oBOI.SetAttribute("IsNaturalSearch", o1Click.IsNaturalSearch.ToString());
                    oBOI.SetAttribute("IsParent", "0");
                    oBOI.SetAttribute("IsExport", "0");
                    oBOI.SetAttribute("IsRowClick", o1Click.IsRowClick.ToString());
                    oBOI.SetAttribute("RowXiLinkID", o1Click.RowXiLinkID.ToString());
                    oBOI.SetAttribute("OnRowClickType", null);
                    oBOI.SetAttribute("OnRowClickValue", "0");
                    oBOI.SetAttribute("IsColumnClick", o1Click.IsColumnClick.ToString());
                    oBOI.SetAttribute("ColumnXiLinkID", o1Click.ColumnXiLinkID.ToString());
                    oBOI.SetAttribute("OnClickColumn", null);
                    oBOI.SetAttribute("OnColumnClickType", null);
                    oBOI.SetAttribute("OnClickParameter", null);
                    oBOI.SetAttribute("OnColumnClickValue", "0");
                    oBOI.SetAttribute("IsCellClick", o1Click.IsCellClick.ToString());
                    oBOI.SetAttribute("CellXiLinkID", o1Click.CellXiLinkID.ToString());
                    oBOI.SetAttribute("OnClickCell", o1Click.OnClickCell);
                    oBOI.SetAttribute("OnCellClickType", null);
                    oBOI.SetAttribute("OnCellClickValue", "0");
                    oBOI.SetAttribute("IsRowTotal", "0");
                    oBOI.SetAttribute("IsColumnTotal", "0");
                    oBOI.SetAttribute("ResultIn", null);
                    oBOI.SetAttribute("PopupType", null);
                    oBOI.SetAttribute("PopupLeft", null);
                    oBOI.SetAttribute("PopupTop", null);
                    oBOI.SetAttribute("PopupWidth", null);
                    oBOI.SetAttribute("PopupHeight", null);
                    oBOI.SetAttribute("DialogMy1", null);
                    oBOI.SetAttribute("DialogMy2", null);
                    oBOI.SetAttribute("DialogAt1", null);
                    oBOI.SetAttribute("DialogAt2", null);
                    oBOI.SetAttribute("IsEdit", o1Click.IsEdit.ToString());
                    oBOI.SetAttribute("IsDelete", o1Click.IsDelete.ToString());
                    oBOI.SetAttribute("CreateRoleID", o1Click.CreateRoleID.ToString());
                    oBOI.SetAttribute("EditRoleID", o1Click.EditRoleID.ToString());
                    oBOI.SetAttribute("DeleteRoleID", o1Click.DeleteRoleID.ToString());
                    oBOI.SetAttribute("EditGroupID", o1Click.EditGroupID.ToString());
                    oBOI.SetAttribute("iLayoutID", o1Click.iLayoutID.ToString());
                    oBOI.SetAttribute("StatusTypeID", o1Click.StatusTypeID.ToString());
                    oBOI.SetAttribute("sSystemType", o1Click.sSystemType);
                    oBOI.SetAttribute("FKiApplicationID", iAppID.ToString());
                    oBOI.SetAttribute("FKiComponentID", o1Click.FKiComponentID.ToString());
                    oBOI.SetAttribute("RepeaterType", o1Click.RepeaterType.ToString());
                    oBOI.SetAttribute("RepeaterComponentID", o1Click.RepeaterComponentID.ToString());
                    oBOI.SetAttribute("iCreateXILinkID", o1Click.iCreateXILinkID.ToString());
                    oBOI.SetAttribute("sAddLabel", o1Click.sAddLabel);
                    oBOI.SetAttribute("sCreateType", o1Click.sCreateType);
                    oBOI.SetAttribute("IsRefresh", o1Click.IsRefresh.ToString());
                    oBOI.SetAttribute("bIsMultiBO", o1Click.bIsMultiBO.ToString());
                    oBOI.SetAttribute("bIsCheckbox", o1Click.bIsCheckbox.ToString());
                    oBOI.SetAttribute("bIsView", o1Click.bIsView.ToString());
                    oBOI.SetAttribute("bIsCopy", o1Click.bIsCopy.ToString());
                    oBOI.SetAttribute("sRowXiLinkType", o1Click.sRowXiLinkType);
                    oBOI.SetAttribute("bIsLockToUser", o1Click.bIsLockToUser.ToString());
                    oBOI.SetAttribute("iPaginationCount", o1Click.iPaginationCount.ToString());
                    //oBOI.SetAttribute("bIsStaticView", o1Click.bIsStaticView.ToString());
                    oBOI.SetAttribute("bIsPreview", o1Click.bIsPreview.ToString());
                    oBOI.SetAttribute("bIsExport", o1Click.bIsExport.ToString());
                    oBOI.SetAttribute("sFileExtension", o1Click.sFileExtension);
                    oBOI.SetAttribute("sTotalColumns", o1Click.sTotalColumns);
                    oBOI.SetAttribute("FKiVisualisationID", o1Click.FKiVisualisationID.ToString());
                    oBOI.SetAttribute("FKi1ClickScriptID", o1Click.FKi1ClickScriptID);
                    oBOI.SetAttribute("bIsRecordLock", o1Click.bIsRecordLock.ToString());
                    oBOI.SetAttribute("bIsMultiSearch", o1Click.bIsMultiSearch.ToString());
                    oBOI.SetAttribute("bIs"+ XIConstant.Key_XICrtdBy, o1Click.bIsXICreatedBy.ToString());
                    oBOI.SetAttribute("FKiCrtd1ClickID", o1Click.FKiCrtd1ClickID.ToString());
                    oBOI.SetAttribute("bIs" + XIConstant.Key_XIUpdtdBy, o1Click.bIsXIUpdatedBy.ToString());
                    oBOI.SetAttribute("FKiUpdtd1ClickID", o1Click.FKiUpdtd1ClickID.ToString());
                    oBOI.SetAttribute("sLastUpdate", o1Click.sLastUpdate);
                    oBOI.SetAttribute("sLog", o1Click.sLog);
                    oBOI.SetAttribute("FKiSearchGroup", o1Click.FKiSearchGroup.ToString());
                    oBOI.SetAttribute("FKiListGroup", o1Click.FKiListGroup.ToString());
                    oCR = oBOI.SaveV2(oBOI);
                    oTrace.oTrace.Add(oCR.oTrace);
                    if (oCR.bOK && oCR.oResult != null)
                    {
                        if (oSigCR != null)
                        {
                            oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "Deafult 1-Click insertion completed for " + sBOName });
                        }
                        oBOI = (XIIBO)oCR.oResult;
                        var sID = oBOI.AttributeI(oBOD.sPrimaryKey).sValue;
                        int i1ClickID = 0;
                        int.TryParse(sID, out i1ClickID);
                        oCResult.oResult = i1ClickID;
                        oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                    }
                    else
                    {
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                        oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Deafult 1-Click insertion failed for " + sBOName });
                    }
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory param OneClick Name is Missing";
                }
            }
            catch (Exception ex)
            {
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Deafult 1-Click insertion failed" });
                oSigCR.oStack.Add(new CNV { sName = "Error Message", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting 1-Click" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        #endregion 1-Click

        #region Menu

        public CResult Save_Menu(XIMenu oMenu)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Save Menu";

            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "Menu insertion started" });
                }

                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XI Menu");
                oBOI.BOD = oBOD;
                if (!string.IsNullOrEmpty(oMenu.Name))
                {
                    oBOI.SetAttribute("ID", oMenu.ID.ToString());
                    oBOI.SetAttribute("MenuID", oMenu.MenuID);
                    oBOI.SetAttribute("ParentID", oMenu.ParentID);
                    oBOI.SetAttribute("ActionType", oMenu.ActionType.ToString());
                    oBOI.SetAttribute("XiLinkID", oMenu.XiLinkID.ToString());
                    oBOI.SetAttribute("Priority", "0");
                    oBOI.SetAttribute("OrgID", oMenu.OrgID.ToString());
                    oBOI.SetAttribute("RoleID", oMenu.RoleID.ToString());
                    oBOI.SetAttribute("Name", oMenu.Name);
                    oBOI.SetAttribute("MenuController", null);
                    oBOI.SetAttribute("MenuAction", null);
                    oBOI.SetAttribute("FKiApplicationID", oMenu.FKiApplicationID.ToString());
                    oBOI.SetAttribute("RootName", oMenu.RootName);
                    oBOI.SetAttribute("StatusTypeID", oMenu.StatusTypeID.ToString());
                    oBOI.SetAttribute("CreatedBy", iUserID.ToString());
                    oBOI.SetAttribute("CreatedTime", DateTime.Now.ToString());
                    oBOI.SetAttribute("CreatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                    oBOI.SetAttribute("UpdatedBy", iUserID.ToString());
                    oBOI.SetAttribute("UpdatedTime", DateTime.Now.ToString());
                    oBOI.SetAttribute("UpdatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                    if (oMenu.XIDeleted == 1)
                    {
                        oBOI.SetAttribute(XIConstant.Key_XIDeleted, oMenu.XIDeleted.ToString());
                    }
                    oCR = oBOI.SaveV2(oBOI);
                    oTrace.oParams.Add(new CNV { sName = "ParentID", sValue = oMenu.ParentID });
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                    oTrace.oTrace.Add(oCR.oTrace);
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory param menu name is missing";
                }

                if (oCR.bOK && oCR.oResult != null)
                {
                    if (oSigCR != null)
                    {
                        oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "Menu insertion completed" });
                    }
                    var oMenuDef = (XIIBO)oCR.oResult;
                    var sMenuID = oMenuDef.Attributes.Where(m => m.Key.ToLower() == "id").Select(m => m.Value).Select(m => m.sValue).FirstOrDefault();
                    oBOI.SetAttribute("MenuID", sMenuID);
                    oCR = oBOI.SaveV2(oBOI);
                    oTrace.oTrace.Add(oCR.oTrace);
                    if (oCR.bOK && oCR.oResult != null)
                    {
                        oCResult.oResult = oBOI;
                        oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                    }
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                    oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Menu insertion failed" });
                }
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        public CResult Interpret_Menu(string XICode)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name; ;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Interpret and save Menu based on text";
            try
            {

                oTrace.oParams.Add(new CNV { sName = "XICode", sValue = XICode });
                string XITask = string.Empty;
                string sRootmenu = string.Empty;
                string MenuName = string.Empty;
                string XiLink = string.Empty;
                var CodeLines = XICode.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (CodeLines != null && CodeLines.Count() > 0)
                {
                    var XIKeyWord = CodeLines.FirstOrDefault();
                    if (XIKeyWord.StartsWith("XI."))
                    {
                        XITask = XIKeyWord.Split('.')[1];
                    }
                    if (XITask.Contains('-'))
                    {
                        sRootmenu = XITask.Split('-')[1].Trim();
                    }
                    CodeLines.RemoveAt(0);
                    if (CodeLines.Count() > 1)
                    {
                        MenuName = CodeLines[1].Split('-')[0];
                        XiLink = CodeLines[1].Split('-')[1];
                    }
                    if (!string.IsNullOrEmpty(XITask) && XITask.ToLower().StartsWith("menu"))
                    {
                        //Layout
                        XIDLayout oLayout = new XIDLayout();
                        oLayout.FKiApplicationID = iAppID;
                        oLayout.LayoutName = MenuName + " Menu Layout";
                        oLayout.LayoutCode = XIConstant.DefaultLayout;
                        oLayout.LayoutType = "Dialog";
                        oLayout.StatusTypeID = 10;
                        oCR = Save_Layout(oLayout);
                        oTrace.oTrace.Add(oCR.oTrace);
                        if (oCR.bOK && oCR.oResult != null)
                        {
                            var BOI = (XIIBO)oCR.oResult;
                            var sID = BOI.AttributeI("id").sValue;
                            var iLayoutID = 0;
                            int.TryParse(sID, out iLayoutID);
                            if (iLayoutID > 0)
                            {
                                XIDLayoutDetails oDetail = new XIDLayoutDetails();
                                oDetail.PlaceHolderID = 0;
                                oDetail.LayoutID = iLayoutID;
                                oDetail.PlaceholderName = oLayout.LayoutName + " Content";
                                oDetail.PlaceholderArea = "div1";
                                oDetail.PlaceholderUniqueName = (oLayout.LayoutName + " Content").Replace(" ", "");
                                oDetail.PlaceholderClass = "col-md-12";
                                oCR = Save_LayoutDetail(oDetail);
                                oTrace.oTrace.Add(oCR.oTrace);
                                if (oCR.bOK && oCR.oResult != null)
                                {
                                    BOI = (XIIBO)oCR.oResult;
                                    sID = BOI.AttributeI("PlaceHolderID").sValue;
                                    var iPlaceholderID = 0;
                                    int.TryParse(sID, out iPlaceholderID);
                                    //Dialog
                                    XIDDialog oDialog = new XIDDialog();
                                    oDialog.FKiApplicationID = iAppID;
                                    oDialog.DialogName = MenuName + " Menu Dialog";
                                    oDialog.PopupSize = "Default";
                                    oDialog.LayoutID = iLayoutID;
                                    oDialog.StatusTypeID = 10;
                                    oCR = Save_Dialog(oDialog);
                                    oTrace.oTrace.Add(oCR.oTrace);
                                    if (oCR.bOK && oCR.oResult != null)
                                    {
                                        BOI = (XIIBO)oCR.oResult;
                                        sID = BOI.AttributeI("id").sValue;
                                        var iDialogID = 0;
                                        int.TryParse(sID, out iDialogID);
                                        if (iDialogID > 0)
                                        {
                                            XIDLayoutMapping oMap = new XIDLayoutMapping();
                                            oMap.PopupID = iDialogID;
                                            oMap.PopupLayoutID = iLayoutID;
                                            oMap.PlaceHolderID = iPlaceholderID;
                                            oMap.XiLinkID = Convert.ToInt32(XiLink);
                                            oMap.ContentType = "XILink";
                                            oMap.StatusTypeID = 10;
                                            oCR = Save_LayoutMapping(oMap);
                                            oTrace.oTrace.Add(oCR.oTrace);
                                            if (oCR.bOK && oCR.oResult != null)
                                            {
                                                //XILink
                                                int iXILinkID = 0;
                                                XILink oXILink = new XILink();
                                                oXILink.Name = MenuName + " XiLink ";
                                                oXILink.URL = "XILink";
                                                oXILink.sActive = "Y";
                                                oXILink.StatusTypeID = 10;
                                                oXILink.FKiApplicationID = iAppID;
                                                oCR = Save_XILink(oXILink);
                                                oTrace.oTrace.Add(oCR.oTrace);
                                                if (oCR.bOK && oCR.oResult != null)
                                                {
                                                    BOI = (XIIBO)oCR.oResult;
                                                    var sXILinkID = BOI.Attributes.Values.Where(m => m.sName.ToLower() == "XILinkID".ToLower()).Select(m => m.sValue).FirstOrDefault();
                                                    int.TryParse(sXILinkID, out iXILinkID);
                                                    iRowXilinkID = iXILinkID;
                                                    if (iXILinkID > 0)
                                                    {
                                                        XiLinkNV oNV = new XiLinkNV();
                                                        oNV.XiLinkID = iXILinkID;
                                                        oNV.Name = "StartAction";
                                                        oNV.Value = "Dialog";
                                                        oCR = Save_XILinkNV(oNV);
                                                        oTrace.oTrace.Add(oCR.oTrace);
                                                        if (oCR.bOK && oCR.oResult != null)
                                                        { }
                                                        else
                                                        {
                                                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                        }
                                                        oNV = new XiLinkNV();
                                                        oNV.XiLinkID = iXILinkID;
                                                        oNV.Name = "DialogID";
                                                        oNV.Value = iDialogID.ToString();
                                                        oCR = Save_XILinkNV(oNV);
                                                        oTrace.oTrace.Add(oCR.oTrace);
                                                        if (oCR.bOK && oCR.oResult != null)
                                                        { }
                                                        else
                                                        {
                                                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                        }



                                                        XIIXI oXI = new XIIXI();
                                                        List<CNV> oWhereParams = new List<CNV>();
                                                        oWhereParams.Add(new CNV { sName = "rootname", sValue = sRootmenu });
                                                        oWhereParams.Add(new CNV { sName = "parentid", sValue = "#" });
                                                        oWhereParams.Add(new CNV { sName = "fkiapplicationid", sValue = iAppID.ToString() });
                                                        var oMenuD = oXI.BOI("XI Menu", null, null, oWhereParams);
                                                        var iMenuID = oMenuD.Attributes["ID"].sValue;

                                                        XIMenu oMenu = new XIMenu();
                                                        oMenu.FKiApplicationID = iAppID;
                                                        oMenu.Name = MenuName;
                                                        oMenu.RootName = sRootmenu;
                                                        oMenu.ParentID = iMenuID;
                                                        oMenu.ActionType = 30;
                                                        oMenu.StatusTypeID = 10;
                                                        oMenu.XiLinkID = iXILinkID;
                                                        oCR = Save_Menu(oMenu);
                                                        oTrace.oTrace.Add(oCR.oTrace);
                                                        if (oCR.bOK && oCR.oResult != null)
                                                        {

                                                        }
                                                        else
                                                        {
                                                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                }
                                            }
                                            else
                                            {
                                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                            }
                                        }
                                    }
                                    else
                                    {
                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                    }
                                }
                                else
                                {
                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                }
                            }
                        }
                        else
                        {
                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        #endregion Menu

        #region InboxMenu

        public CResult Save_InboxMenu(XIDInbox oInbox)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "Inbox insertion started" });
                }
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIInbox");
                oBOI.BOD = oBOD;
                oBOI.SetAttribute("ID", oInbox.ID.ToString());
                oBOI.SetAttribute("OrganizationID", oInbox.OrganizationID.ToString());
                oBOI.SetAttribute("ParentID", oInbox.ParentID);
                oBOI.SetAttribute("StatusTypeID", oInbox.StatusTypeID.ToString());
                oBOI.SetAttribute("CreatedByID", iUserID.ToString());
                oBOI.SetAttribute("CreatedTime", DateTime.Now.ToString());
                oBOI.SetAttribute("CreatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                oBOI.SetAttribute("ModifiedByID", iUserID.ToString());
                oBOI.SetAttribute("ModifiedTime", DateTime.Now.ToString());
                oBOI.SetAttribute("FKiApplicationID", oInbox.FKiApplicationID.ToString());
                oBOI.SetAttribute("FKiXILinkID", oInbox.FKiXILinkID.ToString());
                oBOI.SetAttribute("Location", "40");
                oBOI.SetAttribute("DisplayAs", "50");
                oBOI.SetAttribute("Name", oInbox.Name);
                //oBOI.SetAttribute(XIConstant.Key_XIDeleted, oInbox.XIDeleted.ToString());
                // oBOI.SetAttribute("UpdatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                if (oInbox.XIDeleted == 1)
                {
                    oBOI.SetAttribute(XIConstant.Key_XIDeleted, oInbox.XIDeleted.ToString());
                }
                oCR = oBOI.SaveV2(oBOI);
                if (oCR.bOK && oCR.oResult != null)
                {
                    if (oSigCR != null)
                    {
                        oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "Inbox insertion completed" });
                    }
                    var oMenuDef = (XIIBO)oCR.oResult;
                    //var sMenuID = oMenuDef.Attributes.Where(m => m.Key.ToLower() == "id").Select(m => m.Value).Select(m => m.sValue).FirstOrDefault();
                    //oBOI.SetAttribute("MenuID", sMenuID);
                    //oCR = oBOI.Save(oBOI);
                    if (oCR.bOK && oCR.oResult != null)
                    {

                    }
                }
                else
                {
                    oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Menu insertion failed" });
                }
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                oCResult.oResult = oBOI;
            }
            catch (Exception ex)
            {
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Menu insertion failed" });
                oSigCR.oStack.Add(new CNV { sName = "Error Message", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting Menu" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            return oCResult;
        }

        #endregion InboxMenu

        #region Generate Popup

        public CResult Save_BOPopup(List<CNV> oParams)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name; ;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "BO popup insertion started" });
                }
                int iOrgID = 0;
                var OrgID = oParams.Where(m => m.sName.ToLower() == XIConstant.Param_OrgID.ToLower()).Select(m => m.sValue).FirstOrDefault();
                int.TryParse(OrgID, out iOrgID);
                int iQSDID = 0;
                var sBO = oParams.Where(m => m.sName.ToLower() == XIConstant.Param_BO.ToLower()).Select(m => m.sValue).FirstOrDefault();
                var BOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, sBO);
                var sAppID = oParams.Where(m => m.sName.ToLower() == XIConstant.Param_ApplicationID.ToLower()).Select(m => m.sValue).FirstOrDefault();
                var i1ClickID = oParams.Where(m => m.sName.ToLower() == XIConstant.Param_1ClickID.ToLower()).Select(m => m.sValue).FirstOrDefault();
                int FKiAppID = 0;
                int.TryParse(sAppID, out FKiAppID);
                var oAPPD = (XIDApplication)oCache.GetObjectFromCache(XIConstant.CacheApplication, null, FKiAppID.ToString());
                XIIBO oQSI = new XIIBO();
                //Save Question Set
                XIDQS oQSD = new XIDQS();
                oQSD.sName = sBO + " Popup Question Set";
                oQSD.sDescription = sBO + " Popup Question Set";
                oQSD.StatusTypeID = 10;
                oQSD.bInMemoryOnly = true;
                oQSD.FKiApplicationID = iAppID;
                oQSD.FKiOrganisationID = iOrgID;
                oQSD.bIsStage = false;
                oQSD.SaveType = "Save at End";
                oCR = Save_QuestionSet(oQSD);
                oTrace.oTrace.Add(oCR.oTrace);
                if (oCR.bOK && oCR.oResult != null)
                {
                    int iBOViewStepID = 0;
                    int iTabStepID = 0;
                    int iBOStepID = 0;
                    oQSI = (XIIBO)oCR.oResult;
                    iQSDID = Convert.ToInt32(oQSI.Attributes.Where(m => m.Key.ToLower() == "id").Select(m => m.Value).Select(m => m.sValue).FirstOrDefault());
                    if (iQSDID > 0)
                    {
                        //Step1 with Layout
                        XIDLayout oXLs = new XIDLayout();
                        oXLs.LayoutName = XIConstant.PopupLayoutName;
                        var Layout2 = oXLs.Get_XILayoutDetails();
                        Layout2.OrganizationID = iOrgID;
                        var iStepLayoutID = 0;
                        if (Layout2 != null)
                        {
                            oCR = Copy_Layout(Layout2, true);
                            oTrace.oTrace.Add(oCR.oTrace);
                            if (oCR.bOK && oCR.oResult != null)
                            {
                                var NewStepDef = (XIIBO)oCR.oResult;
                                var sNewStepLayoutID = NewStepDef.Attributes.Where(m => m.Key.ToLower() == "id").Select(s => s.Value).Select(m => m.sValue).FirstOrDefault();
                                iStepLayoutID = Convert.ToInt32(sNewStepLayoutID);
                            }
                            else
                            {
                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                            }
                        }
                        //var oStep1 = AutoCreateQuestionSetStep("Step1 with Layout", iQSID, null, null, iStepLayoutID);


                        XIDQSStep oStep = new XIDQSStep();
                        oStep.sName = "Step1 with Layout";
                        oStep.FKiQSDefintionID = iQSDID;
                        oStep.iLayoutID = iStepLayoutID;
                        oStep.StatusTypeID = 10;
                        oStep.iDisplayAs = 10;
                        oCR = Save_QuestionSetStep(oStep);
                        oTrace.oTrace.Add(oCR.oTrace);
                        if (oCR.bOK && oCR.oResult != null)
                        {

                        }
                        else
                        {
                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                        }

                        //Step with BO Component View
                        oStep = new XIDQSStep();
                        oStep.sName = "Step with BO Component View";
                        oStep.FKiQSDefintionID = iQSDID;
                        oStep.iLayoutID = 0;
                        oStep.StatusTypeID = 10;
                        oStep.iDisplayAs = 20;
                        oCR = Save_QuestionSetStep(oStep);
                        oTrace.oTrace.Add(oCR.oTrace);
                        if (oCR.bOK && oCR.oResult != null)
                        {
                            XIIBO oStepD = (XIIBO)oCR.oResult;
                            var sStepID = oStepD.Attributes.Values.Where(m => m.sName.ToLower() == "id".ToLower()).Select(m => m.sValue).FirstOrDefault();
                            int.TryParse(sStepID, out iBOViewStepID);
                            if (iBOViewStepID > 0)
                            {
                                XIDQSSection oSection = new XIDQSSection();
                                oSection.FKiStepDefinitionID = iBOViewStepID;
                                oSection.iDisplayAs = 40;
                                oSection.iXIComponentID = 2;
                                oCR = Save_QuestionSetStepSection(oSection);
                                oTrace.oTrace.Add(oCR.oTrace);
                                if (oCR.bOK && oCR.oResult != null)
                                {
                                    XIIBO oSectionD = (XIIBO)oCR.oResult;
                                    int iSectionID = 0;
                                    var sSectionID = oSectionD.Attributes.Values.Where(m => m.sName.ToLower() == "id".ToLower()).Select(m => m.sValue).FirstOrDefault();
                                    int.TryParse(sSectionID, out iSectionID);
                                    if (iSectionID > 0)
                                    {
                                        List<CNV> nParams = new List<CNV>();
                                        nParams.Add(new CNV { sName = "BO", sValue = sBOName });
                                        nParams.Add(new CNV { sName = "Group", sValue = "Description" });
                                        nParams.Add(new CNV { sName = "iInstanceID", sValue = "{-iInstanceID}" });
                                        nParams.Add(new CNV { sName = "Visualisation", sValue = "Mode" });
                                        foreach (var Param in nParams)
                                        {
                                            XIDComponentParam oParam = new XIDComponentParam();
                                            oParam.sName = Param.sName;
                                            oParam.sValue = Param.sValue;
                                            oParam.iStepSectionID = iSectionID;
                                            oParam.FKiComponentID = 2;
                                            oCR = Save_ComponentParam(oParam);
                                            oTrace.oTrace.Add(oCR.oTrace);
                                            if (oCR.bOK && oCR.oResult != null)
                                            {

                                            }
                                            else
                                            {
                                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                }
                            }
                        }
                        else
                        {
                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                        }

                        //Step with Tab Component
                        oStep = new XIDQSStep();
                        oStep.sName = "Step with Tab Component";
                        oStep.FKiQSDefintionID = iQSDID;
                        oStep.iLayoutID = 0;
                        oStep.StatusTypeID = 10;
                        oStep.iDisplayAs = 20;
                        oCR = Save_QuestionSetStep(oStep);
                        oTrace.oTrace.Add(oCR.oTrace);
                        if (oCR.bOK && oCR.oResult != null)
                        {
                            XIIBO oStepD = (XIIBO)oCR.oResult;
                            var sStepID = oStepD.Attributes.Values.Where(m => m.sName.ToLower() == "id".ToLower()).Select(m => m.sValue).FirstOrDefault();
                            int.TryParse(sStepID, out iTabStepID);
                            if (iTabStepID > 0)
                            {
                                XIDQSSection oSection = new XIDQSSection();
                                oSection.FKiStepDefinitionID = iTabStepID;
                                oSection.iDisplayAs = 40;
                                oSection.iXIComponentID = 11;
                                oCR = Save_QuestionSetStepSection(oSection);
                                oTrace.oTrace.Add(oCR.oTrace);
                                if (oCR.bOK && oCR.oResult != null)
                                {
                                    XIIBO oSectionD = (XIIBO)oCR.oResult;
                                    int iSectionID = 0;
                                    var sSectionID = oSectionD.Attributes.Values.Where(m => m.sName.ToLower() == "id".ToLower()).Select(m => m.sValue).FirstOrDefault();
                                    int.TryParse(sSectionID, out iSectionID);
                                    if (iSectionID > 0)
                                    {
                                        List<CNV> nParams = new List<CNV>();
                                        nParams.Add(new CNV { sName = "StructureCode", sValue = sStructureName });//Keerthi--Adding dynamic structurcode //sValue = sBOName + " Conf" 
                                        nParams.Add(new CNV { sName = "iBODID", sValue = BOD.BOID.ToString() });
                                        nParams.Add(new CNV { sName = "OutputContent", sValue = "PopupTabContentArea" });
                                        nParams.Add(new CNV { sName = "Tabclickparamname", sValue = "-TabInstance" });
                                        foreach (var Param in nParams)
                                        {
                                            XIDComponentParam oParam = new XIDComponentParam();
                                            oParam.sName = Param.sName;
                                            oParam.sValue = Param.sValue;
                                            oParam.iStepSectionID = iSectionID;
                                            oParam.FKiComponentID = 11;// 11
                                            oCR = Save_ComponentParam(oParam);
                                            oTrace.oTrace.Add(oCR.oTrace);
                                            if (oCR.bOK && oCR.oResult != null)
                                            {

                                            }
                                            else
                                            {
                                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                }
                            }
                        }
                        else
                        {
                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                        }
                        //Step with BO Component
                        oStep = new XIDQSStep();
                        oStep.sName = "Step with BO Component";
                        oStep.FKiQSDefintionID = iQSDID;
                        oStep.iLayoutID = 0;
                        oStep.StatusTypeID = 10;
                        oStep.iDisplayAs = 20;
                        oCR = Save_QuestionSetStep(oStep);
                        oTrace.oTrace.Add(oCR.oTrace);
                        if (oCR.bOK && oCR.oResult != null)
                        {
                            XIIBO oStepD = (XIIBO)oCR.oResult;
                            var sStepID = oStepD.Attributes.Values.Where(m => m.sName.ToLower() == "id".ToLower()).Select(m => m.sValue).FirstOrDefault();
                            int.TryParse(sStepID, out iBOStepID);
                            if (iBOStepID > 0)
                            {
                                XIDQSSection oSection = new XIDQSSection();
                                oSection.FKiStepDefinitionID = iBOStepID;
                                oSection.iDisplayAs = 40;
                                oSection.iXIComponentID = 2;
                                oCR = Save_QuestionSetStepSection(oSection);
                                oTrace.oTrace.Add(oCR.oTrace);
                                if (oCR.bOK && oCR.oResult != null)
                                {
                                    XIIBO oSectionD = (XIIBO)oCR.oResult;
                                    int iSectionID = 0;
                                    var sSectionID = oSectionD.Attributes.Values.Where(m => m.sName.ToLower() == "id".ToLower()).Select(m => m.sValue).FirstOrDefault();
                                    int.TryParse(sSectionID, out iSectionID);
                                    if (iSectionID > 0)
                                    {
                                        List<CNV> nParams = new List<CNV>();
                                        nParams.Add(new CNV { sName = "BO", sValue = sBOName });
                                        nParams.Add(new CNV { sName = "Group", sValue = "Details1" });
                                        nParams.Add(new CNV { sName = "iInstanceID", sValue = "{-iInstanceID}" });
                                        foreach (var Param in nParams)
                                        {
                                            XIDComponentParam oParam = new XIDComponentParam();
                                            oParam.sName = Param.sName;
                                            oParam.sValue = Param.sValue;
                                            oParam.iStepSectionID = iSectionID;
                                            oParam.FKiComponentID = 2;
                                            oCR = Save_ComponentParam(oParam);
                                            oTrace.oTrace.Add(oCR.oTrace);
                                            if (oCR.bOK && oCR.oResult != null)
                                            {

                                            }
                                            else
                                            {
                                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                }
                            }
                        }
                        else
                        {
                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                        }
                        //Step with 1-Click Component
                        oStep = new XIDQSStep();
                        oStep.sName = "Step with 1-Click Component";
                        oStep.FKiQSDefintionID = iQSDID;
                        oStep.iLayoutID = 0;
                        oStep.StatusTypeID = 10;
                        oStep.iDisplayAs = 20;
                        oCR = Save_QuestionSetStep(oStep);
                        oTrace.oTrace.Add(oCR.oTrace);
                        if (oCR.bOK && oCR.oResult != null)
                        {
                            XIIBO oStepD = (XIIBO)oCR.oResult;
                            int iStepID = 0;
                            var sStepID = oStepD.Attributes.Values.Where(m => m.sName.ToLower() == "id".ToLower()).Select(m => m.sValue).FirstOrDefault();
                            int.TryParse(sStepID, out iStepID);
                            if (iStepID > 0)
                            {
                                XIDQSSection oSection = new XIDQSSection();
                                oSection.FKiStepDefinitionID = iStepID;
                                oSection.iDisplayAs = 40;
                                oSection.iXIComponentID = 3;
                                oCR = Save_QuestionSetStepSection(oSection);
                                oTrace.oTrace.Add(oCR.oTrace);
                                if (oCR.bOK && oCR.oResult != null)
                                {
                                    XIIBO oSectionD = (XIIBO)oCR.oResult;
                                    int iSectionID = 0;
                                    var sSectionID = oSectionD.Attributes.Values.Where(m => m.sName.ToLower() == "id".ToLower()).Select(m => m.sValue).FirstOrDefault();
                                    int.TryParse(sSectionID, out iSectionID);
                                    if (iSectionID > 0)
                                    {
                                        List<CNV> nParams = new List<CNV>();
                                        nParams.Add(new CNV { sName = "1ClickID", sValue = "{XIP|i1ClickID}" });
                                        nParams.Add(new CNV { sName = "watchparam1", sValue = "-TabInstance" });
                                        foreach (var Param in nParams)
                                        {
                                            XIDComponentParam oParam = new XIDComponentParam();
                                            oParam.sName = Param.sName;
                                            oParam.sValue = Param.sValue;
                                            oParam.iStepSectionID = iSectionID;
                                            oParam.FKiComponentID = 3;
                                            oCR = Save_ComponentParam(oParam);
                                            oTrace.oTrace.Add(oCR.oTrace);
                                            if (oCR.bOK && oCR.oResult != null)
                                            {

                                            }
                                            else
                                            {
                                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                }
                            }
                        }
                        else
                        {
                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                        }

                        var oLayoutD = (XIDLayout)oCache.GetObjectFromCache(XIConstant.CacheLayout, null, iStepLayoutID.ToString());
                        if (oLayoutD != null && oLayoutD.ID > 0)
                        {
                            for (int i = 0; i < oLayoutD.LayoutDetails.Count(); i++)
                            {
                                XIDLayoutMapping oMap = new XIDLayoutMapping();
                                oMap.PopupLayoutID = oLayoutD.ID;
                                oMap.PlaceHolderID = oLayoutD.LayoutDetails[i].PlaceHolderID;
                                if (i == 0)
                                {
                                    oMap.XiLinkID = iBOViewStepID;
                                }
                                else if (i == 1)
                                {
                                    oMap.XiLinkID = iTabStepID;
                                }
                                else if (i == 2)
                                {
                                    oMap.XiLinkID = iBOStepID;
                                }
                                oMap.ContentType = "Step";
                                oMap.StatusTypeID = 10;
                                oCR = Save_LayoutMapping(oMap);
                                oTrace.oTrace.Add(oCR.oTrace);
                                if (oCR.bOK && oCR.oResult != null)
                                {

                                }
                                else
                                {
                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                }
                            }
                        }
                    }
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                }

                //Create Layout
                XIIBO oBOI = new XIIBO();
                int iLayoutID = 0;
                XIDLayout oLayout = new XIDLayout();
                oLayout.FKiApplicationID = FKiAppID;
                oLayout.LayoutName = sBO + " QS Layout";
                oLayout.LayoutCode = XIConstant.DefaultLayout;
                oLayout.LayoutType = "Dialog";
                oLayout.LayoutLevel = "OrganisationLevel";
                oLayout.Authentication = "Authenticated";
                oLayout.bAddToParentTaskbar = true;
                oLayout.OrganizationID = iOrgID;
                oLayout.StatusTypeID = 10;
                oCR = Save_Layout(oLayout);
                oTrace.oTrace.Add(oCR.oTrace);
                if (oCR.bOK && oCR.oResult != null)
                {
                    oBOI = (XIIBO)oCR.oResult;
                    var sLayoutID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "id").Select(m => m.sValue).FirstOrDefault();
                    int.TryParse(sLayoutID, out iLayoutID);
                    if (iLayoutID > 0)
                    {
                        int iPlaceHolderID = 0;
                        XIDLayoutDetails oDetail = new XIDLayoutDetails();
                        oDetail.PlaceHolderID = 0;
                        oDetail.FKiApplicationID = FKiAppID;
                        oDetail.LayoutID = iLayoutID;
                        oDetail.PlaceholderName = oLayout.LayoutName + " Content";
                        oDetail.PlaceholderArea = "div1";
                        oDetail.PlaceholderUniqueName = (oLayout.LayoutName + " Content").Replace(" ", "");
                        oDetail.PlaceholderClass = "col-md-12";
                        oCR = Save_LayoutDetail(oDetail);
                        oTrace.oTrace.Add(oCR.oTrace);
                        if (oCR.bOK && oCR.oResult != null)
                        {
                            oBOI = (XIIBO)oCR.oResult;
                            var sPlaceHolderID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "PlaceHolderID".ToLower()).Select(m => m.sValue).FirstOrDefault();
                            int.TryParse(sPlaceHolderID, out iPlaceHolderID);
                            if (iPlaceHolderID > 0)
                            {
                                int iDialogID = 0;
                                XIDDialog oDialog = new XIDDialog();
                                oDialog.FKiApplicationID = FKiAppID;
                                oDialog.DialogName = sBO + " 1CS Popup Dialog";
                                oDialog.PopupSize = "Default";
                                oDialog.LayoutID = iLayoutID;
                                oDialog.StatusTypeID = 10;
                                oDialog.FKiBOID = BOD.BOID;
                                oDialog.OrganizationID = iOrgID;
                                oCR = Save_Dialog(oDialog);
                                oTrace.oTrace.Add(oCR.oTrace);
                                if (oCR.bOK && oCR.oResult != null)
                                {
                                    oBOI = (XIIBO)oCR.oResult;
                                    var sDialogID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "ID".ToLower()).Select(m => m.sValue).FirstOrDefault();
                                    int.TryParse(sDialogID, out iDialogID);
                                    if (iDialogID > 0)
                                    {
                                        XIDLayoutMapping oMap = new XIDLayoutMapping();
                                        oMap.PopupID = iDialogID;
                                        oMap.PopupLayoutID = iLayoutID;
                                        oMap.PlaceHolderID = iPlaceHolderID;
                                        oMap.XiLinkID = 8;
                                        oMap.ContentType = "XIComponent";
                                        oMap.StatusTypeID = 10;
                                        oCR = Save_LayoutMapping(oMap);
                                        oTrace.oTrace.Add(oCR.oTrace);
                                        if (oCR.bOK && oCR.oResult != null)
                                        {
                                            List<CNV> nParams = new List<CNV>();
                                            nParams.Add(new CNV { sName = "iQSDID", sValue = iQSDID.ToString() });
                                            nParams.Add(new CNV { sName = "sMode", sValue = "Popup" });
                                            foreach (var Param in nParams)
                                            {
                                                XIDComponentParam oParam = new XIDComponentParam();
                                                oParam.sName = Param.sName;
                                                oParam.sValue = Param.sValue;
                                                oParam.iLayoutMappingID = iPlaceHolderID;
                                                oParam.FKiComponentID = 8;
                                                oCR = Save_ComponentParam(oParam);
                                                oTrace.oTrace.Add(oCR.oTrace);
                                                if (oCR.bOK && oCR.oResult != null)
                                                {

                                                }
                                                else
                                                {
                                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                }
                                            }
                                            //Menu XILink
                                            int iXILinkID = 0;
                                            XILink oXILink = new XILink();
                                            oXILink.Name = sBO + " 1CS Row Click ";
                                            oXILink.URL = "XILink";
                                            oXILink.sActive = "Y";
                                            oXILink.StatusTypeID = 10;
                                            oXILink.FKiApplicationID = FKiAppID;
                                            oXILink.OrganisationID = iOrgID;
                                            oCR = Save_XILink(oXILink);
                                            oTrace.oTrace.Add(oCR.oTrace);
                                            if (oCR.bOK && oCR.oResult != null)
                                            {
                                                oBOI = (XIIBO)oCR.oResult;
                                                var sXILinkID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "XILinkID".ToLower()).Select(m => m.sValue).FirstOrDefault();
                                                int.TryParse(sXILinkID, out iXILinkID);
                                                iRowXilinkID = iXILinkID;
                                                if (iXILinkID > 0)
                                                {
                                                    XiLinkNV oNV = new XiLinkNV();
                                                    oNV.XiLinkID = iXILinkID;
                                                    oNV.Name = "StartAction";
                                                    oNV.Value = "Dialog";
                                                    oCR = Save_XILinkNV(oNV);
                                                    oTrace.oTrace.Add(oCR.oTrace);
                                                    if (oCR.bOK && oCR.oResult != null)
                                                    { }
                                                    else
                                                    {
                                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                    }
                                                    oNV = new XiLinkNV();
                                                    oNV.XiLinkID = iXILinkID;
                                                    oNV.Name = "DialogID";
                                                    oNV.Value = iDialogID.ToString();
                                                    oCR = Save_XILinkNV(oNV);
                                                    oTrace.oTrace.Add(oCR.oTrace);
                                                    if (oCR.bOK && oCR.oResult != null)
                                                    { }
                                                    else
                                                    {
                                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                    }
                                                }

                                                XIIXI oXI = new XIIXI();
                                                List<CNV> oWhereParams = new List<CNV>();
                                                oWhereParams.Add(new CNV { sName = "id", sValue = i1ClickID.ToString() });
                                                var o1ClickI = oXI.BOI("XI1Click", null, null, oWhereParams);
                                                o1ClickI.SetAttribute("IsRowClick", "true");
                                                o1ClickI.SetAttribute("RowXiLinkID", iXILinkID.ToString());
                                                oCR = o1ClickI.SaveV2(o1ClickI);
                                                oTrace.oTrace.Add(oCR.oTrace);
                                                if (oCR.bOK && oCR.oResult != null)
                                                {

                                                }
                                                else
                                                {
                                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                        }
                                    }
                                }
                                else
                                {
                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                }
                            }
                        }
                        else
                        {
                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                        }
                    }
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                    oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Layout insertion failed for - " + oLayout.LayoutName });
                }
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "BO popup insertion completed" });
                }
                if (oTrace.iStatus != (int)xiEnumSystem.xiFuncResult.xiError)
                {
                    oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                    oCResult.oResult = iQSDID;
                }
            }
            catch (Exception ex)
            {
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "BO popup insertion failed" });
                oSigCR.oStack.Add(new CNV { sName = "Error Message", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting Menu" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        #endregion Generate Popup

        #region Generate BOUIDetails

        public CResult Save_BOUIDetails(XIDBOUI oBOUI)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name; ;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            //tracing
            // if tracing is on (which is a config switch for this user) then
            //   oCResult.Trace.Write
            //ALL OK?
            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "BOUIDetails insertion started for " + sBOName });
                }
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIBOUIDetails");
                oBOI.BOD = oBOD;
                oTrace.oParams.Add(new CNV { sName = "FKiBOID", sValue = oBOUI.FKiBOID.ToString() });
                if (oBOUI.FKiBOID != 0)
                {
                    oBOI.SetAttribute("ID", oBOUI.ID.ToString());
                    oBOI.SetAttribute("FKiBOID", oBOUI.FKiBOID.ToString());
                    oBOI.SetAttribute("i1ClickID", oBOUI.i1ClickID.ToString());
                    oBOI.SetAttribute("iLayoutID", oBOUI.iLayoutID.ToString());
                    oBOI.SetAttribute("FKiStructureID", oBOUI.FKiStructureID.ToString());
                    oBOI.SetAttribute("iPopupID", oBOUI.iPopupID.ToString());
                    oBOI.SetAttribute("StatusTypeID", "10");
                    oBOI.SetAttribute("CreatedBy", iUserID.ToString());
                    oBOI.SetAttribute("CreatedTime", DateTime.Now.ToString());
                    oBOI.SetAttribute("CreatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                    oBOI.SetAttribute("UpdatedBy", iUserID.ToString());
                    oBOI.SetAttribute("UpdatedTime", DateTime.Now.ToString());
                    oBOI.SetAttribute("UpdatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                    oCR = oBOI.SaveV2(oBOI);
                    oTrace.oTrace.Add(oCR.oTrace);
                    if (oCR.bOK && oCR.oResult != null)
                    {
                        if (oSigCR != null)
                        {
                            oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "BOUIDetails insertion Success for " + sBOName });
                        }
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                        oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                        oCResult.oResult = oBOI;
                    }
                    else
                    {
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                        oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "BOUIDetails insertion failed for " + sBOName });
                    }
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory param FKiBOID is Missing";
                }

            }
            catch (Exception ex)
            {
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "BOUIDetails insertion failed" });
                oSigCR.oStack.Add(new CNV { sName = "Error Message", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting BOUIDetails" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        #endregion Generate BOUIDefault Popup

        #region Generate BOUIDefault Popup

        public CResult Save_BOUIDefault(XIDBODefault oDefault)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name; ;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            //tracing
            // if tracing is on (which is a config switch for this user) then
            //   oCResult.Trace.Write
            //ALL OK?
            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "BOUIDefault insertion started for " + sBOName });
                }
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIBOUIDefault");
                oBOI.BOD = oBOD;
                oTrace.oParams.Add(new CNV { sName = "FKiBOID", sValue = oDefault.FKiBOID.ToString() });
                if (oDefault.FKiBOID != 0)
                {
                    oBOI.SetAttribute("ID", oDefault.ID.ToString());
                    oBOI.SetAttribute("FKiBOID", oDefault.FKiBOID.ToString());
                    oBOI.SetAttribute("i1ClickID", oDefault.i1ClickID.ToString());
                    oBOI.SetAttribute("iLayoutID", oDefault.iLayoutID.ToString());
                    oBOI.SetAttribute("iXIComponentID", oDefault.iXIComponentID.ToString());
                    oBOI.SetAttribute("iStructureID", oDefault.iStructureID.ToString());
                    oBOI.SetAttribute("iPopupID", oDefault.iPopupID.ToString());
                    oBOI.SetAttribute("iType", oDefault.iType.ToString());
                    oBOI.SetAttribute("StatusTypeID", "10");
                    oBOI.SetAttribute("CreatedBy", iUserID.ToString());
                    oBOI.SetAttribute("CreatedTime", DateTime.Now.ToString());
                    oBOI.SetAttribute("CreatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                    oBOI.SetAttribute("UpdatedBy", iUserID.ToString());
                    oBOI.SetAttribute("UpdatedTime", DateTime.Now.ToString());
                    oBOI.SetAttribute("UpdatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                    oCR = oBOI.SaveV2(oBOI);
                    oTrace.oTrace.Add(oCR.oTrace);
                    if (oCR.bOK && oCR.oResult != null)
                    {
                        if (oSigCR != null)
                        {
                            oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "BOUIDefault insertion Success for " + sBOName });
                        }
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                        oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                        oCResult.oResult = oBOI;
                    }
                    else
                    {
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                        oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "BOUIDefault insertion failed for " + sBOName });
                    }
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory param FKiBOID is Missing";
                }
            }
            catch (Exception ex)
            {
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "BOUIDefault insertion failed" });
                oSigCR.oStack.Add(new CNV { sName = "Error Message", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting BOUIDefault" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        #endregion Generate BOUIDefault Popup

        #region QuestionSet

        public CResult Interpret_QuestionSet(string XICode)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "Question Set building started" });
                }
                XIDXI oXID = new XIDXI();
                string XITask = string.Empty;
                string sQSName = string.Empty;
                int iQSDID = 0;
                if (!string.IsNullOrEmpty(XICode))
                {
                    var CodeLines = XICode.Split(new[] { "\n" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    if (CodeLines != null && CodeLines.Count() > 0)
                    {
                        var XIKeyWord = CodeLines.FirstOrDefault();
                        if (XIKeyWord.StartsWith("XI."))
                        {
                            XITask = XIKeyWord.Split('.')[1];
                        }
                        if (XITask.Contains('-'))
                        {
                            sQSName = XITask.Split('-')[1].Trim();
                        }
                        CodeLines.RemoveAt(0);
                        int i = 0;
                        if (!string.IsNullOrEmpty(XITask) && XITask.ToLower().StartsWith("questionset"))
                        {
                            XIDQS oQS = new XIDQS();
                            oQS.FKiApplicationID = iAppID;
                            oQS.sName = sQSName;
                            oQS.sDescription = sQSName;
                            oQS.SaveType = "Save as Populated";
                            oQS.iVisualisationID = Convert.ToInt32(XIConstant.DefaultQSVisualisationID);
                            oQS.StatusTypeID = 10;
                            oCR = Save_QuestionSet(oQS);
                            XIIBO oQSI = new XIIBO();
                            if (oCR.bOK && oCR.oResult != null)
                            {
                                oQSI = (XIIBO)oCR.oResult;
                                var oQSD = oQSI.BOD;
                                var sPrimaryKey = oQSD.sPrimaryKey;
                                var sQSDID = oQSI.Attributes.Values.Where(m => m.sName.ToLower() == sPrimaryKey.ToLower()).Select(m => m.sValue).FirstOrDefault();
                                int.TryParse(sQSDID, out iQSDID);
                                if (iQSDID > 0)
                                {
                                    int iOrder = 0;
                                    int iSecOrder = 0;
                                    string sStep = string.Empty;
                                    string sSection = string.Empty;
                                    XIIBO oStepI = new XIIBO();
                                    XIIBO oSecI = new XIIBO();
                                    int iStepID = 0;
                                    int iSecID = 0;
                                    string sSecName = string.Empty;
                                    for (int k = 0; k < CodeLines.Count(); k++)
                                    {
                                        if (!string.IsNullOrEmpty(CodeLines[k]))
                                        {
                                            if (CodeLines[k].StartsWith("\t\t"))
                                            {
                                                var SecContent = CodeLines[k].Replace("\t\t", "");
                                                if (SecContent.Contains('.') && SecContent.StartsWith("c."))
                                                {
                                                    var XIComponent = SecContent.Replace("c.", ""); //SecContent.Split(new[] { "c." }, StringSplitOptions.RemoveEmptyEntries).ToList()[1];
                                                    if (XIComponent.ToLower().StartsWith("boform"))
                                                    {
                                                        XIDQSSection oQSSec = new XIDQSSection();
                                                        oCR = oXID.Get_QSSectionDefinition(iSecID.ToString());
                                                        if (oCR.bOK && oCR.oResult != null)
                                                        {
                                                            oQSSec = (XIDQSSection)oCR.oResult;
                                                        }
                                                        oQSSec.iDisplayAs = 40;
                                                        oQSSec.iXIComponentID = 2;
                                                        oCR = Save_QuestionSetStepSection(oQSSec);
                                                        if (oCR.bOK && oCR.oResult != null)
                                                        {
                                                            if (XIComponent.Contains(':'))
                                                            {
                                                                var sParams = XIComponent.Split(':')[1];
                                                                var Params = sParams.Split(',').ToList();
                                                                foreach (var sParam in Params)
                                                                {
                                                                    if (sParam.Contains('='))
                                                                    {
                                                                        XIDComponentParam oPram = new XIDComponentParam();
                                                                        oPram.sName = sParam.Split('=')[0];
                                                                        oPram.sValue = sParam.Split('=')[1];
                                                                        oPram.iStepSectionID = oQSSec.ID;
                                                                        oPram.FKiComponentID = 2;
                                                                        oCR = Save_ComponentParam(oPram);
                                                                        if (oCR.bOK && oCR.oResult != null)
                                                                        {

                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                                else if (SecContent.Contains(','))
                                                {
                                                    XIDQSSection oQSSec = new XIDQSSection();
                                                    oCR = oXID.Get_QSSectionDefinition(iSecID.ToString());
                                                    if (oCR.bOK && oCR.oResult != null)
                                                    {
                                                        oQSSec = (XIDQSSection)oCR.oResult;
                                                    }
                                                    oQSSec.iDisplayAs = 30;
                                                    oCR = Save_QuestionSetStepSection(oQSSec);
                                                    if (oCR.bOK && oCR.oResult != null)
                                                    {

                                                    }
                                                    var Fields = SecContent.Split(',').ToList();
                                                    foreach (var Field in Fields)
                                                    {
                                                        var oFieldOrigin = (XIDFieldOrigin)oCache.GetObjectFromCache(XIConstant.CacheFieldOrigin, Field);
                                                        if (oFieldOrigin != null && oFieldOrigin.ID > 0)
                                                        {
                                                            XIDFieldDefinition oFDef = new XIDFieldDefinition();
                                                            oFDef.FKiXIStepDefinitionID = iStepID;
                                                            oFDef.FKiStepSectionID = iSecID;
                                                            oFDef.FKiXIFieldOriginID = oFieldOrigin.ID;
                                                            oCR = Save_FieldDefinition(oFDef);
                                                            if (oCR.bOK && oCR.oResult != null)
                                                            {

                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            else if (CodeLines[k].StartsWith("\t"))
                                            {
                                                sSection = CodeLines[k].Replace("\t", "");
                                                sSecName = sSection;
                                                XIDQSSection oSec = new XIDQSSection();
                                                oSec.FKiStepDefinitionID = iStepID;
                                                oSec.sName = sSection;
                                                oSec.iOrder = iSecOrder;
                                                oSec.sIsHidden = "off";
                                                oCR = Save_QuestionSetStepSection(oSec);
                                                if (oCR.bOK && oCR.oResult != null)
                                                {
                                                    oSecI = (XIIBO)oCR.oResult;
                                                    var oSecD = oSecI.BOD;
                                                    sPrimaryKey = oSecD.sPrimaryKey;
                                                    var sSecID = oSecI.Attributes.Values.Where(m => m.sName.ToLower() == sPrimaryKey.ToLower()).Select(m => m.sValue).FirstOrDefault();
                                                    int.TryParse(sSecID, out iSecID);
                                                    if (iSecID > 0)
                                                    {
                                                        iSecOrder++;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                sStep = CodeLines[k];
                                                iSecOrder = 0;
                                                XIDQSStep oStep = new XIDQSStep();
                                                oStep.FKiQSDefintionID = iQSDID;
                                                oStep.sName = sStep;
                                                oStep.sDisplayName = sStep;
                                                oStep.StatusTypeID = 10;
                                                oStep.bIsBack = true;
                                                oStep.bIsSaveNext = true;
                                                oStep.iDisplayAs = 20;
                                                oStep.iOrder = iOrder;
                                                oStep.bIsHistory = true;
                                                oCR = Save_QuestionSetStep(oStep);
                                                if (oCR.bOK && oCR.oResult != null)
                                                {
                                                    oStepI = (XIIBO)oCR.oResult;
                                                    var oStepD = oStepI.BOD;
                                                    sPrimaryKey = oStepD.sPrimaryKey;
                                                    var sStepID = oStepI.Attributes.Values.Where(m => m.sName.ToLower() == sPrimaryKey.ToLower()).Select(m => m.sValue).FirstOrDefault();
                                                    int.TryParse(sStepID, out iStepID);
                                                    if (iStepID > 0)
                                                    {
                                                        iOrder++;
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    //Create URL for QuestionSet
                                    int iXILinkID = 0;
                                    XILink oXILink = new XILink();
                                    oXILink.Name = sQSName + "-XILink";
                                    oXILink.URL = "XILink";
                                    oXILink.sActive = "Y";
                                    oXILink.StatusTypeID = 10;
                                    oCR = Save_XILink(oXILink);
                                    if (oCR.bOK && oCR.oResult != null)
                                    {
                                        var oXILinkI = (XIIBO)oCR.oResult;
                                        var sXILinkID = oXILinkI.Attributes.Values.Where(m => m.sName.ToLower() == "XILinkID".ToLower()).Select(m => m.sValue).FirstOrDefault();
                                        int.TryParse(sXILinkID, out iXILinkID);
                                        if (iXILinkID > 0)
                                        {
                                            XiLinkNV oNV = new XiLinkNV();
                                            oNV.XiLinkID = iXILinkID;
                                            oNV.Name = "StartAction";
                                            oNV.Value = "QuestionSet";
                                            Save_XILinkNV(oNV);
                                            oNV = new XiLinkNV();
                                            oNV.XiLinkID = iXILinkID;
                                            oNV.Name = "QuestionSetID";
                                            oNV.Value = iQSDID.ToString();
                                            Save_XILinkNV(oNV);
                                        }
                                    }

                                    int iLayoutID = 0;
                                    XIDLayout oLayout = new XIDLayout();
                                    oLayout.FKiApplicationID = iAppID;
                                    oLayout.LayoutName = sQSName;
                                    oLayout.LayoutCode = XIConstant.DefaultLayout;
                                    oLayout.iThemeID = Convert.ToInt32(XIConstant.DefaultQSThemeID);
                                    oLayout.LayoutType = "Inline";
                                    oLayout.LayoutLevel = "OrganisationLevel";
                                    oLayout.Authentication = "NonAuthenticated";
                                    oLayout.StatusTypeID = 10;
                                    oCR = Save_Layout(oLayout);
                                    if (oCR.bOK && oCR.oResult != null)
                                    {
                                        var oLayoutI = (XIIBO)oCR.oResult;
                                        var sLayoutID = oLayoutI.Attributes.Values.Where(m => m.sName.ToLower() == "id").Select(m => m.sValue).FirstOrDefault();
                                        int.TryParse(sLayoutID, out iLayoutID);
                                        if (iLayoutID > 0)
                                        {
                                            int iPlaceHolderID = 0;
                                            XIDLayoutDetails oDetail = new XIDLayoutDetails();
                                            oDetail.PlaceHolderID = 0;
                                            oDetail.FKiApplicationID = iAppID;
                                            oDetail.LayoutID = iLayoutID;
                                            oDetail.PlaceholderName = oLayout.LayoutName + " Content";
                                            oDetail.PlaceholderArea = "div1";
                                            oDetail.PlaceholderUniqueName = (oLayout.LayoutName + " Content").Replace(" ", "");
                                            oDetail.PlaceholderClass = "col-md-12";
                                            oCR = Save_LayoutDetail(oDetail);
                                            if (oCR.bOK && oCR.oResult != null)
                                            {
                                                var oLayoutDetailI = (XIIBO)oCR.oResult;
                                                var sPlaceHolderID = oLayoutDetailI.Attributes.Values.Where(m => m.sName.ToLower() == "PlaceHolderID".ToLower()).Select(m => m.sValue).FirstOrDefault();
                                                int.TryParse(sPlaceHolderID, out iPlaceHolderID);
                                                if (iPlaceHolderID > 0)
                                                {
                                                    XIDLayoutMapping oMap = new XIDLayoutMapping();
                                                    oMap.PopupID = 0;
                                                    oMap.PopupLayoutID = iLayoutID;
                                                    oMap.PlaceHolderID = iPlaceHolderID;
                                                    oMap.XiLinkID = iXILinkID;
                                                    oMap.ContentType = "XiLink";
                                                    oMap.StatusTypeID = 10;
                                                    oCR = Save_LayoutMapping(oMap);
                                                }
                                            }
                                        }
                                    }


                                    //Create QS URL
                                    XIURLMappings oURL = new XIURLMappings();
                                    oURL.sUrlName = sQSName;
                                    oURL.sActualUrl = "/zeeinsurance/devorg/" + sQSName;
                                    oURL.FKiApplicationID = iAppID;
                                    oURL.sType = "QuestionSet";
                                    oURL.FKiSourceID = 0;
                                    oURL.StatusTypeID = 10;
                                    oURL.OrganisationID = iOrgID;
                                    oCR = Save_URLMapping(oURL);
                                    if (oCR.bOK && oCR.oResult != null)
                                    {

                                    }
                                }
                            }
                        }
                    }
                }
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "Questionset building completed" });
                }
            }
            catch (Exception ex)
            {
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Question Set Building failed" });
                oSigCR.oStack.Add(new CNV { sName = "Error Message", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting Question Set" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            return oCResult;
        }

        public CResult Save_QuestionSet(XIDQS oQSD)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name; ;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "Question Set insertion started" });
                }
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIQSDefinition");
                oBOI.BOD = oBOD;
                oTrace.oParams.Add(new CNV { sName = "sHTMLPage", sValue = oQSD.sHTMLPage });
                if (!string.IsNullOrEmpty(oQSD.sName))
                {
                    oBOI.SetAttribute("ID", oQSD.ID.ToString());
                    oBOI.SetAttribute("FKiApplicationID", oQSD.FKiApplicationID.ToString());
                    oBOI.SetAttribute("FKiOrganisationID", oQSD.FKiOrganisationID.ToString());
                    oBOI.SetAttribute("sName", oQSD.sName);
                    oBOI.SetAttribute("sDescription", oQSD.sDescription);
                    oBOI.SetAttribute("iVisualisationID", oQSD.iVisualisationID.ToString());
                    oBOI.SetAttribute("SaveType", oQSD.SaveType);
                    oBOI.SetAttribute("bIsTemplate", oQSD.bIsTemplate.ToString());
                    oBOI.SetAttribute("sHTMLPage", oQSD.sHTMLPage);
                    oBOI.SetAttribute("iLayoutID", oQSD.iLayoutID.ToString());
                    oBOI.SetAttribute("StatusTypeID", oQSD.StatusTypeID.ToString());
                    oBOI.SetAttribute("CreatedBy", iUserID.ToString());
                    oBOI.SetAttribute("CreatedTime", DateTime.Now.ToString());
                    oBOI.SetAttribute("CreatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                    oBOI.SetAttribute("UpdatedBy", iUserID.ToString());
                    oBOI.SetAttribute("UpdatedTime", DateTime.Now.ToString());
                    oBOI.SetAttribute("UpdatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                    oBOI.SetAttribute("bInMemoryOnly", oQSD.bInMemoryOnly.ToString());
                    oBOI.SetAttribute("FKiParameterID", oQSD.FKiParameterID.ToString());
                    oBOI.SetAttribute("FKiBOStructureID", oQSD.FKiBOStructureID.ToString());
                    oBOI.SetAttribute("sMode", oQSD.sMode);
                    oBOI.SetAttribute("FKiOriginID", oQSD.FKiOriginID.ToString());
                    oBOI.SetAttribute("FKiClassID", oQSD.FKiClassID.ToString());
                    oBOI.SetAttribute("FKiSourceID", oQSD.FKiSourceID.ToString());
                    oBOI.SetAttribute("bIsStage", oQSD.bIsStage.ToString());
                    oBOI.SetAttribute("iMaxStage", oQSD.iMaxStage.ToString());
                    oCR = oBOI.SaveV2(oBOI);
                    oTrace.oTrace.Add(oCR.oTrace);
                    if (oCR.bOK && oCR.oResult != null)
                    {
                        oBOI = (XIIBO)oCR.oResult;
                        oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                        if (oSigCR != null)
                        {
                            oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "Question Set insertion Completed" });
                        }
                        oCResult.oResult = oBOI;
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                    }
                    else
                    {
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                        oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Question Set insertion failed" });
                    }
                }

                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory param QS Name is Missing";
                }
            }
            catch (Exception ex)
            {
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Question Set insertion failed" });
                oSigCR.oStack.Add(new CNV { sName = "Error Message", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting Question Set" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        public CResult Save_QuestionSetStep(XIDQSStep oQSDStep)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name; ;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "Question Set Step insertion started" });
                }
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIQSStepDefinition");
                oBOI.BOD = oBOD;
                oTrace.oParams.Add(new CNV { sName = "FKiQSDefintionID", sValue = oQSDStep.FKiQSDefintionID.ToString() });
                if (oQSDStep.FKiQSDefintionID != 0)
                {
                    oBOI.SetAttribute("ID", oQSDStep.ID.ToString());
                    oBOI.SetAttribute("FKiQSDefintionID", oQSDStep.FKiQSDefintionID.ToString());
                    oBOI.SetAttribute("sName", oQSDStep.sName);
                    oBOI.SetAttribute("sDisplayName", oQSDStep.sDisplayName);
                    oBOI.SetAttribute("iOrder", oQSDStep.iOrder.ToString());
                    oBOI.SetAttribute("sCode", oQSDStep.sCode);
                    oBOI.SetAttribute("iDisplayAs", oQSDStep.iDisplayAs.ToString());
                    oBOI.SetAttribute("XILinkID", oQSDStep.XILinkID.ToString());
                    oBOI.SetAttribute("FKiContentID", oQSDStep.FKiContentID.ToString());
                    oBOI.SetAttribute("iXIComponentID", oQSDStep.iXIComponentID.ToString());
                    oBOI.SetAttribute("i1ClickID", oQSDStep.i1ClickID.ToString());
                    oBOI.SetAttribute("HTMLContent", oQSDStep.HTMLContent);
                    oBOI.SetAttribute("bIsSaveNext", oQSDStep.bIsSaveNext.ToString());
                    oBOI.SetAttribute("bIsSave", oQSDStep.bIsSave.ToString());
                    oBOI.SetAttribute("bIsBack", oQSDStep.bIsBack.ToString());
                    oBOI.SetAttribute("bInMemoryOnly", oQSDStep.bInMemoryOnly.ToString());
                    oBOI.SetAttribute("iLayoutID", oQSDStep.iLayoutID.ToString());
                    oBOI.SetAttribute("sIsHidden", oQSDStep.sIsHidden);
                    oBOI.SetAttribute("StatusTypeID", oQSDStep.StatusTypeID.ToString());
                    oBOI.SetAttribute("CreatedBy", iUserID.ToString());
                    oBOI.SetAttribute("CreatedTime", DateTime.Now.ToString());
                    oBOI.SetAttribute("CreatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                    oBOI.SetAttribute("UpdatedBy", iUserID.ToString());
                    oBOI.SetAttribute("UpdatedTime", DateTime.Now.ToString());
                    oBOI.SetAttribute("UpdatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                    oBOI.SetAttribute("bIsContinue", oQSDStep.bIsContinue.ToString());
                    oBOI.SetAttribute("bIsHistory", oQSDStep.bIsHistory.ToString());
                    oBOI.SetAttribute("bIsCopy", oQSDStep.bIsCopy.ToString());
                    oBOI.SetAttribute("sSaveBtnLabel", oQSDStep.sSaveBtnLabel);
                    oBOI.SetAttribute("sBackBtnLabel", oQSDStep.sBackBtnLabel);
                    oBOI.SetAttribute("bIsReload", oQSDStep.bIsReload.ToString());
                    oBOI.SetAttribute("bIsMerge", oQSDStep.bIsMerge.ToString());
                    oBOI.SetAttribute("iStage", oQSDStep.iStage.ToString());
                    oBOI.SetAttribute("iLockStage", oQSDStep.iLockStage.ToString());
                    oBOI.SetAttribute("iCutStage", oQSDStep.iCutStage.ToString());
                    oBOI.SetAttribute("OrganisationID", iOrgID.ToString());
                    oBOI.SetAttribute("s1ClickIDs", oQSDStep.s1ClickIDs);
                    oCR = oBOI.SaveV2(oBOI);
                    oTrace.oTrace.Add(oCR.oTrace);
                    if (oCR.bOK && oCR.oResult != null)
                    {
                        if (oSigCR != null)
                        {
                            oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "Question Set Step insertion completed" });
                        }
                        oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                        oCResult.oResult = oBOI;
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                    }
                    else
                    {
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                        oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Question Set Step insertion failed" });
                    }
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory param FKiQSDefintionID is Missing";
                }
            }
            catch (Exception ex)
            {
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Question Set Step insertion failed" });
                oSigCR.oStack.Add(new CNV { sName = "Error Message", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting LayoutMappings" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        public CResult Save_QuestionSetStepSection(XIDQSSection oQSDSection)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name; ;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "Question Set Step Section insertion started" });
                }
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIQSSectionDefinition");
                oBOI.BOD = oBOD;
                oTrace.oParams.Add(new CNV { sName = "FKiStepDefinitionID", sValue = oQSDSection.FKiStepDefinitionID.ToString() });
                if (oQSDSection.FKiStepDefinitionID != 0)
                {
                    oBOI.SetAttribute("ID", oQSDSection.ID.ToString());
                    oBOI.SetAttribute("FKiStepDefinitionID", oQSDSection.FKiStepDefinitionID.ToString());
                    oBOI.SetAttribute("iDisplayAs", oQSDSection.iDisplayAs.ToString());
                    oBOI.SetAttribute("sName", oQSDSection.sName);
                    oBOI.SetAttribute("sCode", oQSDSection.sCode);
                    oBOI.SetAttribute("iOrder", oQSDSection.iOrder.ToString());
                    oBOI.SetAttribute("iXIComponentID", oQSDSection.iXIComponentID.ToString());
                    oBOI.SetAttribute("i1ClickID", oQSDSection.i1ClickID.ToString());
                    oBOI.SetAttribute("HTMLContent", oQSDSection.HTMLContent);
                    oBOI.SetAttribute("bIsGroup", oQSDSection.bIsGroup.ToString());
                    oBOI.SetAttribute("sGroupDescription", oQSDSection.sGroupDescription);
                    oBOI.SetAttribute("sGroupLabel", oQSDSection.sGroupLabel);
                    oBOI.SetAttribute("sIsHidden", oQSDSection.sIsHidden);
                    oBOI.SetAttribute("FKiApplicationID", iAppID.ToString());
                    oBOI.SetAttribute("OrganisationID", iOrgID.ToString());
                    oCR = oBOI.SaveV2(oBOI);
                    oTrace.oTrace.Add(oCR.oTrace);
                    if (oCR.bOK && oCR.oResult != null)
                    {
                        if (oSigCR != null)
                        {
                            oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "Question Set Step Section insertion Completed" });
                        }
                        oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                        oCResult.oResult = oBOI;
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                    }
                    else
                    {
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                        oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Question Set Step Section insertion failed" });
                    }
                }

                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory param FKiStepDefinitionID is Missing";
                }
            }
            catch (Exception ex)
            {
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Question Set Step Section insertion failed" });
                oSigCR.oStack.Add(new CNV { sName = "Error Message", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting LayoutMappings" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        public CResult Save_FieldDefinition(XIDFieldDefinition oFieldDef)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "Field Definition insertion started" });
                }
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIFieldDefinition_T");
                oBOI.BOD = oBOD;
                oBOI.SetAttribute("ID", oFieldDef.ID.ToString());
                oBOI.SetAttribute("FKiXIStepDefinitionID", oFieldDef.FKiXIStepDefinitionID.ToString());
                oBOI.SetAttribute("FKiStepSectionID", oFieldDef.FKiStepSectionID.ToString());
                oBOI.SetAttribute("FKiXIFieldOriginID", oFieldDef.FKiXIFieldOriginID.ToString());
                oCR = oBOI.SaveV2(oBOI);
                if (oCR.bOK && oCR.oResult != null)
                {
                    if (oSigCR != null)
                    {
                        oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "Field Definition insertion Completed" });
                    }
                }
                else
                {
                    oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Field Definition insertion failed" });
                }
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                oCResult.oResult = oBOI;
            }
            catch (Exception ex)
            {
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Field Definition insertion failed" });
                oSigCR.oStack.Add(new CNV { sName = "Error Message", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting Field Definition" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            return oCResult;
        }

        #endregion QuestionSet

        #region Default Layout

        public CResult Save_ApplicationDefaultLayout(XIDLayout oLayout, string sMenu)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Save default layout and assign components to layout";
            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "Default Layout insertion started" });
                }
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Default Layout Insertoin Started" });
                oCR = Save_Layout(oLayout);
                oTrace.oTrace.Add(oCR.oTrace);
                if (oCR.bOK && oCR.oResult != null)
                {
                    int iLayoutID = 0;
                    XIIBO oLayI = new XIIBO();
                    oLayI = (XIIBO)oCR.oResult;
                    var sLayoutID = oLayI.Attributes.Where(m => m.Key.ToLower() == "id").Select(s => s.Value).Select(m => m.sValue).FirstOrDefault();
                    int.TryParse(sLayoutID, out iLayoutID);
                    oTrace.oParams.Add(new CNV { sName = "iLayoutID", sValue = iLayoutID.ToString() });
                    if (iLayoutID > 0)
                    {
                        XIIBO oBOI = new XIIBO();
                        if (oSigCR != null)
                        {
                            oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "Default Layout insertion completed" });
                        }
                        oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Default Layout inserted successfully" });
                        if (oSigCR != null)
                        {
                            oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "Default Layout details insertion started" });
                        }
                        oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Default Layout details insertion started" });
                        XIDLayoutDetails oLayDetail = new XIDLayoutDetails();
                        oLayDetail.LayoutID = iLayoutID;
                        oLayDetail.PlaceholderName = "Inbox";
                        oLayDetail.PlaceholderArea = "div1";
                        oLayDetail.PlaceholderUniqueName = "Inbox" + sLayoutID;
                        oLayDetail.PlaceholderClass = "col-md-2";
                        oCR = Save_LayoutDetail(oLayDetail);
                        var sPlaceHolderID = string.Empty;
                        oTrace.oTrace.Add(oCR.oTrace);
                        if (oCR.bOK && oCR.oResult != null)
                        {
                            int iPlaceHolderID = 0;
                            oBOI = (XIIBO)oCR.oResult;
                            sPlaceHolderID = oBOI.Attributes.Where(m => m.Key.ToLower() == "placeholderid").Select(s => s.Value).Select(m => m.sValue).FirstOrDefault();
                            int.TryParse(sPlaceHolderID, out iPlaceHolderID);
                            XIDLayoutMapping oMap = new XIDLayoutMapping();
                            oMap.PopupLayoutID = Convert.ToInt32(sLayoutID);
                            oMap.PlaceHolderID = iPlaceHolderID;
                            oMap.Type = "Inline";
                            oMap.ContentType = "XIComponent";
                            oMap.XiLinkID = 16;
                            oMap.StatusTypeID = 10;
                            oCR = Save_LayoutMapping(oMap, oLayDetail.PlaceholderName);
                            oTrace.oTrace.Add(oCR.oTrace);
                            if (oCR.bOK && oCR.oResult != null)
                            {
                                oBOI = (XIIBO)oCR.oResult;
                                int iLayoutMappingID = 0;
                                var sLayoutMappingID = oBOI.Attributes.Where(m => m.Key.ToLower() == "ID".ToLower()).Select(s => s.Value).Select(m => m.sValue).FirstOrDefault();
                                int.TryParse(sLayoutMappingID, out iLayoutMappingID);
                                XIDComponentParam oCParam = new XIDComponentParam();
                                oCParam.sName = "iRoleID";
                                oCParam.sValue = "{XIP|iRoleID}";
                                oCParam.FKiComponentID = 16;
                                oCParam.iLayoutMappingID = iPlaceHolderID;
                                oCParam.iXiLinkID = 0;
                                oCParam.iStepDefinitionID = 0;
                                oCParam.iStepSectionID = 0;
                                oCParam.iQueryID = 0;
                                oCR = Save_ComponentParam(oCParam);
                                oTrace.oTrace.Add(oCR.oTrace);
                                if (oCR.bOK && oCR.oResult != null)
                                {

                                }
                                else
                                {
                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                }
                            }
                            else
                            {
                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                            }
                        }
                        else
                        {
                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                        }
                        oLayDetail = new XIDLayoutDetails();
                        oLayDetail.LayoutID = iLayoutID;
                        oLayDetail.PlaceholderName = "Content";
                        oLayDetail.PlaceholderArea = "div2";
                        oLayDetail.PlaceholderUniqueName = "oInsBase";
                        oLayDetail.PlaceholderClass = "col-md-8";
                        oCR = Save_LayoutDetail(oLayDetail);
                        oTrace.oTrace.Add(oCR.oTrace);
                        if (oCR.bOK && oCR.oResult != null)
                        {
                            oBOI = (XIIBO)oCR.oResult;
                            sPlaceHolderID = oBOI.Attributes.Where(m => m.Key.ToLower() == "placeholderid").Select(s => s.Value).Select(m => m.sValue).FirstOrDefault();
                            XIDLayoutMapping oMap = new XIDLayoutMapping();
                            oMap.PopupLayoutID = Convert.ToInt32(sLayoutID);
                            oMap.PlaceHolderID = Convert.ToInt32(sPlaceHolderID);
                            oMap.StatusTypeID = 10;
                            oCR = Save_LayoutMapping(oMap, oLayDetail.PlaceholderName);
                            oTrace.oTrace.Add(oCR.oTrace);
                            if (oCR.bOK && oCR.oResult != null)
                            {

                            }
                            else
                            {
                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                            }
                        }
                        else
                        {
                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                        }
                        oLayDetail = new XIDLayoutDetails();
                        oLayDetail.LayoutID = iLayoutID;
                        oLayDetail.PlaceholderName = "Menu";
                        oLayDetail.PlaceholderArea = "div3";
                        oLayDetail.PlaceholderUniqueName = "Menu" + sLayoutID;
                        oLayDetail.PlaceholderClass = "col-md-2";
                        oCR = Save_LayoutDetail(oLayDetail);
                        oTrace.oTrace.Add(oCR.oTrace);
                        if (oCR.bOK && oCR.oResult != null)
                        {
                            int iPlaceHolderID = 0;
                            oBOI = (XIIBO)oCR.oResult;
                            sPlaceHolderID = oBOI.Attributes.Where(m => m.Key.ToLower() == "placeholderid").Select(s => s.Value).Select(m => m.sValue).FirstOrDefault();
                            int.TryParse(sPlaceHolderID, out iPlaceHolderID);
                            XIDLayoutMapping oMap = new XIDLayoutMapping();
                            oMap.PopupLayoutID = Convert.ToInt32(sLayoutID);
                            oMap.PlaceHolderID = iPlaceHolderID;
                            oMap.Type = "Inline";
                            oMap.ContentType = "XIComponent";
                            oMap.XiLinkID = 15;
                            oMap.StatusTypeID = 10;
                            oCR = Save_LayoutMapping(oMap, oLayDetail.PlaceholderName);
                            oTrace.oTrace.Add(oCR.oTrace);
                            if (oCR.bOK && oCR.oResult != null)
                            {
                                oBOI = (XIIBO)oCR.oResult;
                                int iLayoutMappingID = 0;
                                var sLayoutMappingID = oBOI.Attributes.Where(m => m.Key.ToLower() == "ID".ToLower()).Select(s => s.Value).Select(m => m.sValue).FirstOrDefault();
                                int.TryParse(sLayoutMappingID, out iLayoutMappingID);
                                XIDComponentParam oCParam = new XIDComponentParam();
                                oCParam.sName = "MenuName";
                                oCParam.sValue = sMenu;
                                oCParam.FKiComponentID = 15;
                                oCParam.iLayoutMappingID = iPlaceHolderID;
                                oCParam.iXiLinkID = 0;
                                oCParam.iStepDefinitionID = 0;
                                oCParam.iStepSectionID = 0;
                                oCParam.iQueryID = 0;
                                oCR = Save_ComponentParam(oCParam);
                                oTrace.oTrace.Add(oCR.oTrace);
                                if (oCR.bOK && oCR.oResult != null)
                                {

                                }
                                else
                                {
                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                }
                            }
                            else
                            {
                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                            }
                        }
                        else
                        {
                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                        }
                        if (oSigCR != null)
                        {
                            oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "Layout mappings insertion completed" });
                        }
                        oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Layout mappings inserted successfully" });
                    }
                    else
                    {
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                    }
                    if (oTrace.iStatus != (int)xiEnumSystem.xiFuncResult.xiError)
                    {
                        oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                        oCResult.oResult = oLayI;
                    }
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                    oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Layout insertion failed" });
                    oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Layout insertion failed" });
                }
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        //Delete
        public CResult CreateLayoutMappings(string sLayoutID, string sPlaceHolderID, string PlaceHolderName)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            try
            {
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XI LayoutMapping");
                oBOI.BOD = oBOD;

                oBOI.SetAttribute("ID", "0");
                oBOI.SetAttribute("FKiApplicationID", iAppID.ToString());
                oBOI.SetAttribute("PopupID", "0");
                oBOI.SetAttribute("PopupLayoutID", sLayoutID);
                oBOI.SetAttribute("PlaceHolderID", sPlaceHolderID);
                if (PlaceHolderName == "Inbox")
                {
                    oBOI.SetAttribute("XiLinkID", "16");
                }
                else if (PlaceHolderName == "Menu")
                {
                    //oBOI.SetAttribute("XiLinkID", "15");
                    oBOI.SetAttribute("XiLinkID", "15");
                }
                oBOI.SetAttribute("Type", "Inline");
                oBOI.SetAttribute("StatusTypeID", "10");
                oBOI.SetAttribute("ContentType", "XIComponent");
                oBOI.SetAttribute("HTMLCode", null);
                oBOI.SetAttribute("IsValueSet", "0");
                oBOI.SetAttribute("CreatedBy", iUserID.ToString());
                oBOI.SetAttribute("CreatedTime", DateTime.Now.ToString());
                oBOI.SetAttribute("CreatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                oBOI.SetAttribute("UpdatedBy", iUserID.ToString());
                oBOI.SetAttribute("UpdatedTime", DateTime.Now.ToString());
                oBOI.SetAttribute("UpdatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                oBOI.SetAttribute("OrganisationID", iOrgID.ToString());

                var XiBO = oBOI.Save(oBOI);
                if (XiBO.bOK && XiBO.oResult != null)
                {
                    oBOI = (XIIBO)XiBO.oResult;
                    var sLayoutMappingID = oBOI.Attributes.Where(m => m.Key.ToLower() == "id").Select(s => s.Value).Select(m => m.sValue).FirstOrDefault();
                    XIIBO oBOI1 = new XIIBO();
                    XIDBO oBOD1 = new XIDBO();
                    oBOD1 = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIComponentParams");
                    oBOI1.BOD = oBOD1;
                    oBOI1.SetAttribute("ID", "0");
                    if (PlaceHolderName == "Inbox")
                    {
                        oBOI1.SetAttribute("sName", "iRoleID");
                    }
                    else if (PlaceHolderName == "Menu")
                    {
                        oBOI1.SetAttribute("sName", "MenuName");
                    }
                    if (PlaceHolderName == "Inbox")
                    {
                        oBOI1.SetAttribute("sValue", "{XIP|iRoleID}");
                    }
                    else if (PlaceHolderName == "Menu")
                    {
                        oBOI1.SetAttribute("sValue", sAppName);
                    }
                    if (PlaceHolderName == "Inbox")
                    {
                        oBOI1.SetAttribute("FKiComponentID", "16");
                    }
                    else if (PlaceHolderName == "Menu")
                    {
                        oBOI1.SetAttribute("FKiComponentID", "15");
                        //oBOI1.SetAttribute("FKiComponentID", "23");
                    }
                    oBOI1.SetAttribute("iLayoutMappingID", sPlaceHolderID);
                    oBOI1.SetAttribute("iXiLinkID", "0");
                    oBOI1.SetAttribute("iStepDefinitionID", "0");
                    oBOI1.SetAttribute("iStepSectionID", "0");
                    oBOI1.SetAttribute("iQueryID", "0");
                    var XiBO1 = oBOI1.Save(oBOI1);
                }
                oCResult.oResult = oBOI;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting LayoutMappings" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            return oCResult;
        }

        #endregion Default Layout

        #region Component Param

        public CResult Save_ComponentParam(XIDComponentParam oParam)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name; ;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "Component params insertion started" });
                }
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD1 = new XIDBO();
                oBOD1 = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIComponentParams");
                oBOI.BOD = oBOD1;
                oTrace.oParams.Add(new CNV { sName = "FKiComponentID", sValue = oParam.FKiComponentID.ToString() });
                if (oParam.FKiComponentID != 0)
                {
                    oBOI.SetAttribute("ID", oParam.ID.ToString());
                    oBOI.SetAttribute("sName", oParam.sName);
                    oBOI.SetAttribute("sValue", oParam.sValue);
                    oBOI.SetAttribute("FKiComponentID", oParam.FKiComponentID.ToString());
                    oBOI.SetAttribute("iLayoutMappingID", oParam.iLayoutMappingID.ToString());
                    oBOI.SetAttribute("iXiLinkID", oParam.iXiLinkID.ToString());
                    oBOI.SetAttribute("iStepDefinitionID", oParam.iStepDefinitionID.ToString());
                    oBOI.SetAttribute("iStepSectionID", oParam.iStepSectionID.ToString());
                    oBOI.SetAttribute("iQueryID", oParam.iQueryID.ToString());
                    oCR = oBOI.SaveV2(oBOI);
                    oTrace.oTrace.Add(oCR.oTrace);
                    if (oCR.bOK && oCR.oResult != null)
                    {
                        oBOI = (XIIBO)oCR.oResult;
                        oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                        if (oSigCR != null)
                        {
                            oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "Component params insertion completed" });
                        }
                        oCResult.oResult = oBOI;
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                    }
                    else
                    {
                        oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Component params insertion failed" });
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                    }
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory param FKiComponentID is Missing";
                }
            }
            catch (Exception ex)
            {
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Component params insertion failed" });
                oSigCR.oStack.Add(new CNV { sName = "Error Message", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting LayoutMappings" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        #endregion Component Param

        #region Field Option List

        public CResult Save_FieldOptionList(int ID, string[] NVPairs)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            try
            {
                Dictionary<string, object> OptParam = new Dictionary<string, object>();
                OptParam["FKiQSFieldID"] = ID;
                var oOptionLists = Connection.Select<XIDFieldOptionList>("XIFieldOptionList_T", OptParam).ToList();
                foreach (var items in NVPairs)
                {
                    var Pair = items.Split('^').ToList();
                    var sPairName = Pair[0];
                    var bExists = oOptionLists.Where(m => m.sOptionName == sPairName).FirstOrDefault();
                    if (bExists == null)
                    {
                        XIIBO oBOI = new XIIBO();
                        XIDBO oBOD = new XIDBO();
                        oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIFieldOptionList_T");
                        oBOI.BOD = oBOD;
                        oBOI.SetAttribute("ID", bExists.ID.ToString());
                        oBOI.SetAttribute("FKiQSFieldID", ID.ToString());
                        oBOI.SetAttribute("sOptionName", Pair[0]);
                        oBOI.SetAttribute("sOptionValue", Pair[1]);
                        oBOI.SetAttribute("iType", Pair[2]);
                        if (!string.IsNullOrEmpty(Pair[3]))
                        {
                            oBOI.SetAttribute("sShowField", Pair[3]);
                        }
                        if (!string.IsNullOrEmpty(Pair[4]))
                        {
                            oBOI.SetAttribute("sHideField", Pair[4]);
                        }
                        if (!string.IsNullOrEmpty(Pair[5]))
                        {
                            oBOI.SetAttribute("sOptionCode", Pair[5]);
                        }
                        var XiBO1 = oBOI.SaveV2(oBOI);
                    }
                }
                //oCResult.oResult = oBOI;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting Field OptionList" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            return oCResult;
        }

        #endregion Field Option List      

        #region Sync BO

        public CResult Check_TableExists(string sTableName, int iDataSourceID)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            try
            {
                oTrace.oParams.Add(new CNV { sName = "sTableName", sValue = sTableName });
                oTrace.oParams.Add(new CNV { sName = "iDataSourceID", sValue = iDataSourceID.ToString() });
                bool bExist = false;
                string cmdText = @"IF EXISTS(SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME='" + sTableName + "') SELECT 1 ELSE SELECT 0";
                var sBODataSources = oXID.GetBODataSource(iDataSourceID, 0);
                SqlConnection SC = new SqlConnection(sBODataSources);
                SC.Open();
                SqlCommand cmds = new SqlCommand();
                cmds.Connection = SC;
                cmds = new SqlCommand(cmdText, SC);
                SqlCommand TableCheck = new SqlCommand(cmdText, SC);
                var value = TableCheck.ExecuteScalar().ToString();
                int iFalg = 0;
                int.TryParse(value, out iFalg);

                if (iFalg == 0)
                {
                    bExist = false;
                }
                else
                {
                    bExist = true;
                }
                oCResult.oResult = bExist;
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        public CResult Check_ColumnExists(string sTableName, string sAttr, int iDataSourceID)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            try
            {
                oTrace.oParams.Add(new CNV { sName = "sTableName", sValue = sTableName });
                oTrace.oParams.Add(new CNV { sName = "iDataSourceID", sValue = iDataSourceID.ToString() });
                bool bExist = false;

                var sDBName = System.Configuration.ConfigurationManager.AppSettings["DataBaseServer"];
                string sCheckColmn = @"IF EXISTS(SELECT column_name from INFORMATION_SCHEMA.columns where TABLE_NAME='" + sTableName + "' AND COLUMN_NAME ='" + sAttr + "') SELECT 1 ELSE SELECT 0";
                var sBODataSources1 = oXID.GetBODataSource(iDataSourceID, 0);
                SqlConnection SC = new SqlConnection(sBODataSources1);
                SC.Open();
                SqlCommand cmds1 = new SqlCommand();
                cmds1.Connection = SC;
                cmds1 = new SqlCommand(sCheckColmn, SC);
                SqlCommand ColCheck = new SqlCommand(sCheckColmn, SC);
                var value = ColCheck.ExecuteScalar().ToString();
                int iFalg = 0;
                int.TryParse(value, out iFalg);

                if (iFalg == 0)
                {
                    bExist = false;
                }
                else
                {
                    bExist = true;
                }
                oCResult.oResult = bExist;
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        public CResult Sync_BO(string sTableName, List<XIDAttribute> oAttrs, int iDataSourceID)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "Table Creation started for : " + sTableName });
                }
                oCR = Check_TableExists(sTableName, iDataSourceID);
                oTrace.oTrace.Add(oCR.oTrace);
                if (oCR.bOK && oCR.oResult != null)
                {
                    var bTabExist = (bool)oCR.oResult;
                    string sQuery = string.Empty;
                    if (!bTabExist)
                    {
                        sQuery = "create table [" + sTableName + "] (";
                    }
                    else
                    {
                        sQuery = "alter table [" + sTableName + "] add ";
                    }
                    foreach (var sAttr in oAttrs)
                    {
                        oCR = Check_ColumnExists(sTableName, sAttr.Name, iDataSourceID);
                        oTrace.oTrace.Add(oCR.oTrace);
                        if (oCR.bOK && oCR.oResult != null)
                        {
                            var bColExist = (bool)oCR.oResult;
                            if (!bColExist)
                            {
                                if (sAttr.Name.ToLower() == "id" || sAttr.Name.ToLower() == "boid" || sAttr.Name.ToLower() == "xilinkid")
                                {
                                    sQuery = sQuery + " " + sAttr.Name + " int IDENTITY(1, 1) PRIMARY KEY" + ",";
                                }
                                else
                                {
                                    if (sAttr.TypeID > 0 && string.IsNullOrEmpty(sAttr.DataType))
                                    {
                                        sAttr.DataType = ((BODatatypes)sAttr.TypeID).ToString();
                                    }
                                    if (sAttr.TypeID == 60)
                                    {
                                        sQuery = sQuery + " " + sAttr.Name + " " + sAttr.DataType + ",";
                                    }
                                    else
                                    {
                                        if (!string.IsNullOrEmpty(sAttr.DataType) && (sAttr.DataType.ToLower() == "bigint" || sAttr.DataType.ToLower() == "datetime" || sAttr.DataType.ToLower() == "date" || sAttr.DataType.ToLower() == "time" || sAttr.DataType.ToLower() == "float" || sAttr.DataType.ToLower() == "bit"))
                                        {
                                            sQuery = sQuery + " " + sAttr.Name + " " + sAttr.DataType + ",";
                                        }
                                        else if (sAttr.DataType.ToLower() == "UniqueIdentifier".ToLower())
                                        {
                                            sQuery = sQuery + " " + sAttr.Name + " " + sAttr.DataType + " NOT NULL DEFAULT newid(),";
                                        }
                                        else
                                        {
                                            sQuery = sQuery + " " + sAttr.Name + " " + sAttr.DataType + "(" + sAttr.MaxLength + ")" + ",";
                                        }
                                    }
                                }
                                if (!bTabExist)
                                {
                                    sQuery = sQuery + "[" + XIConstant.Key_XICrtdBy + "][varchar](15) NOT NULL,[" + XIConstant.Key_XICrtdWhn + "] [datetime] NOT NULL, [" + XIConstant.Key_XIUpdtdBy + "] [varchar](15) NOT NULL,[" + XIConstant.Key_XIUpdtdWhn + "] [datetime] NOT NULL,[" + XIConstant.Key_XIDeleted + "] [int] NULL,[sHierarchy] [varchar](256) NULL,[XIGUID] [uniqueidentifier] NOT NULL DEFAULT newid()";

                                    sQuery = sQuery + ")";
                                }
                                else if (bTabExist)
                                {
                                    sQuery = sQuery.Substring(0, sQuery.Length - 1);
                                }
                                string sBODataSource = oXID.GetBODataSource(iDataSourceID, 0);
                                XIDBAPI Connection = new XIDBAPI(sBODataSource);
                                Connection.ExecuteQuery(sQuery);
                                if (oSigCR != null)
                                {
                                    oSigCR.oStack.Add(new CNV { sName = "success", sValue = "Table creation completed for : " + sTableName });
                                }
                                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                            }
                            else
                            {
                                if (oSigCR != null)
                                    oSigCR.oStack.Add(new CNV { sName = "success", sValue = "Table Update completed for : " + sTableName });
                                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                            }
                        }


                    }
                }
                oCResult.oResult = "Success";
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Table creation failed for : " + sTableName });
                oSigCR.oStack.Add(new CNV { sName = "Error Message", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Creation of SharedTable" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        #endregion Sync BO

        #region BO Drop Down

        public List<XIDropDown> Get_AllBOs()
        {
            List<XIDropDown> oddlBOs = new List<XIDropDown>();
            Dictionary<string, object> BOParams = new Dictionary<string, object>();
            var oBODef = Connection.Select<XIDBO>("XIBO_T_N", BOParams).ToList();
            oddlBOs = oBODef.Select(m => new XIDropDown { Value = m.BOID, text = m.Name }).ToList();
            oddlBOs.Insert(0, new XIDropDown { Value = 0, text = "--Select--" });
            return oddlBOs;
        }

        #endregion BO Drop Down

        #region Application Drop Down

        public List<XIDropDown> Get_AllApplications()
        {
            List<XIDropDown> ddlApps = new List<XIDropDown>();
            Dictionary<string, object> AppParams = new Dictionary<string, object>();
            var oAppDef = Connection.Select<XIDApplication>("XIApplication_T", AppParams).ToList();
            ddlApps = oAppDef.Select(m => new XIDropDown { Value = m.ID, text = m.sApplicationName }).ToList();
            ddlApps.Insert(0, new XIDropDown { Value = 0, text = "--Select--" });
            return ddlApps;
        }

        public CResult Get_ParentDDL(List<CNV> oParams)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            try
            {
                var sBO = oParams.Where(m => m.sName.ToLower() == XIConstant.Param_BO.ToLower()).Select(m => m.sValue).FirstOrDefault();
                if (!string.IsNullOrEmpty(sBO))
                {
                    XIDXI oXID = new XIDXI();
                    XIInfraCache oCache = new XIInfraCache();
                    var FKBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, sBO);
                    var sBODataSource = oXID.GetBODataSource(FKBOD.iDataSource, FKBOD.FKiApplicationID);
                    var Con = new XIDBAPI(sBODataSource);
                    Dictionary<string, object> GrpParams = new Dictionary<string, object>();
                    GrpParams["BOID"] = FKBOD.BOID;
                    GrpParams["GroupName"] = "Label";
                    var FKBOLabelG = Connection.Select<XIDGroup>("XIBOGroup_T_N", GrpParams).FirstOrDefault();
                    var LabelGroup = FKBOLabelG.BOFieldNames;
                    if (!string.IsNullOrEmpty(LabelGroup))
                    {
                        Dictionary<string, string> DDL = Con.SelectDDL(LabelGroup, FKBOD.TableName);
                        var FKDDL = DDL.Select(n => new XIDOptionList { sOptionName = n.Value, sValues = n.Key }).ToList();
                        oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                        oCResult.oResult = FKDDL;
                    }
                }
            }
            catch (Exception ex)
            {
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Component params insertion started" });
                oSigCR.oStack.Add(new CNV { sName = "Error Message", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting LayoutMappings" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            return oCResult;
        }

        #endregion Application Drop Down

        #region Reference Data Menu

        public CResult Save_ReferenceData(List<CNV> oParams)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Save Reference Data menu to application login";
            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "Reference Data insertion started" });
                }
                var sDialogName = oParams.Where(m => m.sName.ToLower() == "dialogname").Select(m => m.sValue).FirstOrDefault();
                var sLayoutName = oParams.Where(m => m.sName.ToLower() == "layoutname").Select(m => m.sValue).FirstOrDefault();
                var sXilinkName = oParams.Where(m => m.sName.ToLower() == "xilinkname").Select(m => m.sValue).FirstOrDefault();
                var sApplicationName = oParams.Where(m => m.sName.ToLower() == "sapplicationname").Select(m => m.sValue).FirstOrDefault();
                var sParentID = oParams.Where(m => m.sName.ToLower() == "sparentid").Select(m => m.sValue).FirstOrDefault();
                var sCreateParentID = oParams.Where(m => m.sName.ToLower() == "screateparentid").Select(m => m.sValue).FirstOrDefault();
                var IsRowClick = oParams.Where(m => m.sName.ToLower() == "isrowclick").Select(m => m.sValue).FirstOrDefault();
                var s1ClickID = oParams.Where(m => m.sName.ToLower() == "i1clickid").Select(m => m.sValue).FirstOrDefault();
                var sAppID = oParams.Where(m => m.sName.ToLower() == XIConstant.Param_ApplicationID.ToLower()).Select(m => m.sValue).FirstOrDefault();
                int iAppID = 0;
                int.TryParse(sAppID, out iAppID);
                var i1ClickID = 0;
                int iOneClickID = 0;
                int.TryParse(s1ClickID, out iOneClickID);
                //Create Layout
                XIIBO oBOI = new XIIBO();
                int iLayoutID = 0;
                XIDLayout oLayout = new XIDLayout();
                oLayout.FKiApplicationID = iAppID;
                oLayout.LayoutName = sLayoutName;
                oLayout.LayoutCode = XIConstant.DefaultLayout;
                oLayout.LayoutType = "Dialog";
                oLayout.LayoutLevel = "OrganisationLevel";
                oLayout.Authentication = "Authenticated";
                oLayout.StatusTypeID = 10;
                oCR = Save_Layout(oLayout);
                oTrace.oTrace.Add(oCR.oTrace);
                if (oCR.bOK && oCR.oResult != null)
                {
                    oBOI = (XIIBO)oCR.oResult;
                    var sLayoutID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "id").Select(m => m.sValue).FirstOrDefault();
                    int.TryParse(sLayoutID, out iLayoutID);
                    if (iLayoutID > 0)
                    {
                        //Create Layout Details
                        int iPlaceHolderID = 0;
                        XIDLayoutDetails oDetail = new XIDLayoutDetails();
                        oDetail.PlaceHolderID = 0;
                        oDetail.FKiApplicationID = iAppID;
                        oDetail.LayoutID = iLayoutID;
                        oDetail.PlaceholderName = oLayout.LayoutName + " Content";
                        oDetail.PlaceholderArea = "div1";
                        oDetail.PlaceholderUniqueName = (oLayout.LayoutName + " Content").Replace(" ", "");
                        oDetail.PlaceholderClass = "col-md-12";
                        oCR = Save_LayoutDetail(oDetail);
                        oTrace.oTrace.Add(oCR.oTrace);
                        if (oCR.bOK && oCR.oResult != null)
                        {
                            oBOI = (XIIBO)oCR.oResult;
                            var sPlaceHolderID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "PlaceHolderID".ToLower()).Select(m => m.sValue).FirstOrDefault();
                            int.TryParse(sPlaceHolderID, out iPlaceHolderID);
                            if (iPlaceHolderID > 0)
                            {
                                //Create Dialog
                                int iDialogID = 0;
                                XIDDialog oDialog = new XIDDialog();
                                oDialog.FKiApplicationID = iAppID;
                                oDialog.DialogName = sDialogName;
                                oDialog.PopupSize = "Default";
                                oDialog.LayoutID = iLayoutID;
                                oDialog.StatusTypeID = 10;
                                oCR = Save_Dialog(oDialog);
                                oTrace.oTrace.Add(oCR.oTrace);
                                if (oCR.bOK && oCR.oResult != null)
                                {
                                    oBOI = (XIIBO)oCR.oResult;
                                    var sDialogID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "ID".ToLower()).Select(m => m.sValue).FirstOrDefault();
                                    int.TryParse(sDialogID, out iDialogID);
                                    if (iDialogID > 0)
                                    {
                                        //Create Layout Mappings
                                        XIDLayoutMapping oMap = new XIDLayoutMapping();
                                        oMap.PopupID = iDialogID;
                                        oMap.PopupLayoutID = iLayoutID;
                                        oMap.PlaceHolderID = iPlaceHolderID;
                                        oMap.XiLinkID = 3;
                                        oMap.ContentType = "XIComponent";
                                        oMap.Type = "Dialog";
                                        oMap.StatusTypeID = 10;
                                        oMap.FKiApplicationID = iAppID;
                                        oCR = Save_LayoutMapping(oMap);
                                        oTrace.oTrace.Add(oCR.oTrace);
                                        if (oCR.bOK && oCR.oResult != null)
                                        {
                                            //Create XILink
                                            int iXILinkID = 0;
                                            XILink oXILink = new XILink();
                                            oXILink.FKiApplicationID = iAppID;
                                            oXILink.Name = sXilinkName;
                                            oXILink.URL = "XILink";
                                            oXILink.sActive = "Y";
                                            oXILink.StatusTypeID = 10;
                                            oCR = Save_XILink(oXILink);
                                            oTrace.oTrace.Add(oCR.oTrace);
                                            if (oCR.bOK && oCR.oResult != null)
                                            {
                                                oBOI = (XIIBO)oCR.oResult;
                                                var sXILinkID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "XILinkID".ToLower()).Select(m => m.sValue).FirstOrDefault();
                                                int.TryParse(sXILinkID, out iXILinkID);
                                                if (iXILinkID > 0)
                                                {
                                                    XiLinkNV oNV = new XiLinkNV();
                                                    oNV.XiLinkID = iXILinkID;
                                                    oNV.Name = "StartAction";
                                                    oNV.Value = "Dialog";
                                                    oCR = Save_XILinkNV(oNV);
                                                    oTrace.oTrace.Add(oCR.oTrace);
                                                    if (oCR.bOK && oCR.oResult != null)
                                                    {

                                                    }
                                                    else
                                                    {
                                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                    }
                                                    oNV = new XiLinkNV();
                                                    oNV.XiLinkID = iXILinkID;
                                                    oNV.Name = "DialogID";
                                                    oNV.Value = iDialogID.ToString();
                                                    oCR = Save_XILinkNV(oNV);
                                                    oTrace.oTrace.Add(oCR.oTrace);
                                                    if (oCR.bOK && oCR.oResult != null)
                                                    {

                                                    }
                                                    else
                                                    {
                                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                    }
                                                }
                                                if (IsRowClick == "1")
                                                {
                                                    //Update RowXilinkID for 1Click
                                                    XIIBO oBOIS = new XIIBO();
                                                    XIDBO oBOD = new XIDBO();
                                                    oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XI1Click");
                                                    oBOIS.BOD = oBOD;
                                                    oBOIS.SetAttribute("ID", iOneClickID.ToString());
                                                    oBOIS.SetAttribute("IsRowClick", "1");
                                                    oBOIS.SetAttribute("RowXiLinkID", iXILinkID.ToString());
                                                    oCR = oBOIS.SaveV2(oBOIS);
                                                    oTrace.oTrace.Add(oCR.oTrace);
                                                    if (oCR.bOK && oCR.oResult != null)
                                                    {

                                                    }
                                                    else
                                                    {
                                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                    }
                                                    //Create ComponentParams for RowClick
                                                    List<CNV> nParams = new List<CNV>();
                                                    nParams.Add(new CNV { sName = "1ClickID", sValue = "{XIP|1ClickID}" });
                                                    foreach (var Param in nParams)
                                                    {
                                                        XIDComponentParam oParam = new XIDComponentParam();
                                                        oParam.sName = Param.sName;
                                                        oParam.sValue = Param.sValue;
                                                        oParam.FKiComponentID = 3;
                                                        oParam.iLayoutMappingID = iPlaceHolderID;
                                                        oCR = Save_ComponentParam(oParam);
                                                        oTrace.oTrace.Add(oCR.oTrace);
                                                        if (oCR.bOK && oCR.oResult != null)
                                                        {

                                                        }
                                                        else
                                                        {
                                                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    //Create 1-Click
                                                    XID1Click o1Click = new XID1Click();
                                                    o1Click.Name = o1Click.Code = o1Click.Description = o1Click.Title = "Reference Data Default 1-Click";
                                                    o1Click.Query = "SELECT [XI1Click_T].ID, [XI1Click_T].Name FROM XI1Click_T where CategoryID = 5 and FKiApplicationID = " + iAppID + " ORDER BY [XI1Click_T].ID DESC";
                                                    o1Click.BOID = 828;
                                                    o1Click.DisplayAs = 50;
                                                    o1Click.CategoryID = 3;
                                                    o1Click.FromBos = "XI1Click_T";
                                                    o1Click.SelectFields = "[XI1Click_T].ID, [XI1Click_T].Name";
                                                    oCR = Save_1Click(o1Click);
                                                    oTrace.oTrace.Add(oCR.oTrace);
                                                    if (oCR.bOK && oCR.oResult != null)
                                                    {

                                                        i1ClickID = (int)oCR.oResult;
                                                        //var sClickID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "id".ToLower()).Select(m => m.sValue).FirstOrDefault();
                                                        //int.TryParse(sClickID, out i1ClickID);
                                                        if (i1ClickID > 0)
                                                        {
                                                            List<CNV> nParams = new List<CNV>();
                                                            nParams.Add(new CNV { sName = "1ClickID", sValue = i1ClickID.ToString() });
                                                            nParams.Add(new CNV { sName = "Visualisation", sValue = "OneClickVisibility" });
                                                            foreach (var Param in nParams)
                                                            {
                                                                XIDComponentParam oParam = new XIDComponentParam();
                                                                oParam.sName = Param.sName;
                                                                oParam.sValue = Param.sValue;
                                                                oParam.FKiComponentID = 3;
                                                                oParam.iLayoutMappingID = iPlaceHolderID;
                                                                oCR = Save_ComponentParam(oParam);
                                                                oTrace.oTrace.Add(oCR.oTrace);
                                                                if (oCR.bOK && oCR.oResult != null)
                                                                {

                                                                }
                                                                else
                                                                {
                                                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                                }
                                                            }
                                                        }
                                                        //Create 1Click Parameter for 1Click
                                                        XID1ClickParameter oParameter = new XID1ClickParameter();
                                                        oParameter.FKi1ClickID = i1ClickID;
                                                        oParameter.sName = "ID";
                                                        oParameter.sValue = "{XIP|1ClickID}";
                                                        oParameter.iType = 10;
                                                        oCR = Save_1ClickParameter(oParameter);
                                                        oTrace.oTrace.Add(oCR.oTrace);
                                                        if (oCR.bOK && oCR.oResult != null)
                                                        {

                                                        }
                                                        else
                                                        {
                                                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                    }

                                                    //Create Menu
                                                    XIMenu oMenu = new XIMenu();
                                                    oMenu.Name = "Reference Data";
                                                    oMenu.XiLinkID = iXILinkID;
                                                    oMenu.ParentID = sParentID;
                                                    oMenu.FKiApplicationID = iAppID;
                                                    oMenu.RootName = sApplicationName;
                                                    oMenu.ActionType = 30;
                                                    oMenu.StatusTypeID = 10;
                                                    oCR = Save_Menu(oMenu);
                                                    oTrace.oTrace.Add(oCR.oTrace);
                                                    if (oCR.bOK && oCR.oResult != null)
                                                    {

                                                    }
                                                    else
                                                    {
                                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                    }
                                                    //XI Create Menu
                                                    XIAssignMenu oAS = new XIAssignMenu();
                                                    oAS.Name = "Reference Data";
                                                    oAS.ParentID = sCreateParentID;
                                                    oAS.FKiApplicationID = iAppID;
                                                    oAS.RootName = sApplicationName;
                                                    oAS.ActionType = 30;
                                                    oAS.sType = "AssignMenu";
                                                    oCR = Save_AssignMenu(oAS);
                                                    oTrace.oTrace.Add(oCR.oTrace);
                                                    var iCreatedMenuID = 0;
                                                    if (oCR.bOK && oCR.oResult != null)
                                                    {
                                                        var oMenuDef = (XIIBO)oCR.oResult;
                                                        var sMenuID = oMenuDef.Attributes.Where(m => m.Key.ToLower() == "id").Select(m => m.Value).Select(m => m.sValue).FirstOrDefault();
                                                        int.TryParse(sMenuID, out iCreatedMenuID);
                                                    }
                                                    else
                                                    {
                                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                    }

                                                    //Menu Mapping
                                                    XIAssignMenu oAM = new XIAssignMenu();
                                                    oAM.Name = "Reference Data";
                                                    oAM.XiLinkID = iXILinkID;
                                                    oAM.ActionType = 30;
                                                    oAM.sType = "CreateMenu";
                                                    oCR = Save_AssignMenu(oAM);
                                                    oTrace.oTrace.Add(oCR.oTrace);
                                                    if (oCR.bOK && oCR.oResult != null)
                                                    {
                                                        var oMenuDef = (XIIBO)oCR.oResult;
                                                        var sMenuID = oMenuDef.Attributes.Where(m => m.Key.ToLower() == "id").Select(m => m.Value).Select(m => m.sValue).FirstOrDefault();

                                                        //Update FKiMenuID for Menu in XIMenuMappings_T Table
                                                        XIIBO oBOIS = new XIIBO();
                                                        XIDBO oBOD = new XIDBO();
                                                        oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "AssignMenu");
                                                        oBOIS.BOD = oBOD;
                                                        oBOIS.SetAttribute("ID", iCreatedMenuID.ToString());
                                                        oBOIS.SetAttribute("FKiMenuID", sMenuID);
                                                        oCR = oBOIS.SaveV2(oBOIS);
                                                        oTrace.oTrace.Add(oCR.oTrace);
                                                        if (oCR.bOK && oCR.oResult != null)
                                                        {

                                                        }
                                                        else
                                                        {
                                                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                        }
                                                    }
                                                    else
                                                    {
                                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                            }
                                        }
                                        else
                                        {
                                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                        }
                                    }
                                }
                                else
                                {
                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                }
                            }
                        }
                        else
                        {
                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                        }
                    }
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                    oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Reference Data insertion failed" });
                }
                if (oTrace.iStatus != (int)xiEnumSystem.xiFuncResult.xiError)
                {
                    oCResult.oResult = i1ClickID;
                    oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                }

            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        #endregion Reference Data Menu

        #region 1ClickParameter

        public CResult Save_1ClickParameter(XID1ClickParameter oParameter)
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
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "1ClickParameter insertion started" });
                }
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XI1ClickParameter");
                oBOI.BOD = oBOD;
                oBOI.SetAttribute("ID", oParameter.ID.ToString());
                oBOI.SetAttribute("FKi1ClickID", oParameter.FKi1ClickID.ToString());
                oBOI.SetAttribute("sName", oParameter.sName);
                oBOI.SetAttribute("sDefault", oParameter.sDefault);
                oBOI.SetAttribute("sValue", oParameter.sValue);
                oBOI.SetAttribute("iType", "10");
                //oBOI.SetAttribute("CreatedBy", iUserID.ToString());
                //oBOI.SetAttribute("CreatedTime", DateTime.Now.ToString());
                oBOI.SetAttribute("CreatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                //oBOI.SetAttribute("UpdatedBy", iUserID.ToString());
                //oBOI.SetAttribute("UpdatedTime", DateTime.Now.ToString());
                oBOI.SetAttribute("UpdatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                oCR = oBOI.SaveV2(oBOI);
                oTrace.oTrace.Add(oCR.oTrace);
                if (oCR.bOK && oCR.oResult != null)
                {
                    oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                    oCResult.oResult = oBOI;
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                }
                else
                {
                    oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "1ClickParameter insertion failed" });
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                }

            }
            catch (Exception ex)
            {
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "1ClickParameter insertion failed" });
                oSigCR.oStack.Add(new CNV { sName = "Error Message", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting 1ClickParameter" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        #endregion 1ClickParameter

        #region Build_BOAttribute

        public CResult Build_BOAttribute(List<CNV> oParams)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            try
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "BO Attribute insertion started" });
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "BO Attribute insertion started" });
                }
                int iAttrID = 0;
                string sAttr = string.Empty;
                string sTableName = string.Empty;
                int iDataSourceID = 0;
                string sBO = oParams.Where(m => m.sName.ToLower() == XIConstant.Param_BO.ToLower()).Select(m => m.sValue).FirstOrDefault();
                string sAttrID = oParams.Where(m => m.sName.ToLower() == XIConstant.Param_InstanceID.ToLower()).Select(m => m.sValue).FirstOrDefault();
                int.TryParse(sAttrID, out iAttrID);
                if (!string.IsNullOrEmpty(sBO) && iAttrID > 0)
                {
                    var oAttrDef = new XIDAttribute();
                    var oAttrList = new List<XIDAttribute>();
                    XIInfraCache oCache = new XIInfraCache();
                    XIDBO oBOD = new XIDBO();
                    oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, sBO, null);
                    XIIXI oXI = new XIIXI();
                    List<CNV> oWhrParams = new List<CNV>();
                    oWhrParams.Add(new CNV { sName = oBOD.sPrimaryKey, sValue = iAttrID.ToString() });
                    var oBOI = oXI.BOI(sBO, null, null, oWhrParams);
                    if (oBOI != null)
                    {
                        if (oBOI.Attributes.ContainsKey("BOID"))
                        {
                            var sBOID = oBOI.Attributes["BOID"].sValue;
                            int.TryParse(sBOID, out iBODID);
                        }
                        if (oBOI.Attributes.ContainsKey("Name"))
                        {
                            oAttrDef.Name = oBOI.Attributes["Name"].sValue;
                            sAttr = oAttrDef.Name;
                        }
                        if (oBOI.Attributes.ContainsKey("LabelName"))
                        {
                            oAttrDef.LabelName = oBOI.Attributes["LabelName"].sValue;
                        }
                        if (oBOI.Attributes.ContainsKey("TypeID"))
                        {
                            oAttrDef.TypeID = Convert.ToInt32(oBOI.Attributes["TypeID"].sValue);
                            if (Enum.IsDefined(typeof(BODatatypes), oAttrDef.TypeID))
                            {
                                oAttrDef.DataType = ((BODatatypes)oAttrDef.TypeID).ToString();
                            }
                        }
                        if (oBOI.Attributes.ContainsKey("MaxLength"))
                        {
                            oAttrDef.MaxLength = oBOI.Attributes["MaxLength"].sValue;
                        }
                        if (oBOI.Attributes.ContainsKey("FKTableName"))
                        {
                            oAttrDef.FKTableName = oBOI.Attributes["FKTableName"].sValue;
                        }
                        if (oBOI.Attributes.ContainsKey("sFKBOName"))
                        {
                            oAttrDef.sFKBOName = oBOI.Attributes["sFKBOName"].sValue;
                        }
                        if (oBOI.Attributes.ContainsKey("bFKGUID"))
                        {
                            var bFKGUID = oBOI.Attributes["bFKGUID"].sValue;
                            if (!string.IsNullOrEmpty(bFKGUID))
                            {
                                if (bFKGUID == "True")
                                {
                                    oAttrDef.bFKGUID = true;
                                }
                            }
                        }
                        if (oBOI.Attributes.ContainsKey("iSystemType"))
                        {
                            int iSystemType = 0;
                            var SystemType = oBOI.Attributes["iSystemType"].sValue;
                            int.TryParse(SystemType, out iSystemType);
                            oAttrDef.iSystemType = iSystemType;
                        }
                        if (oBOI.Attributes.ContainsKey("bKPI"))
                        {
                            bool bKPI = false;
                            var KPI = oBOI.Attributes["bKPI"].sValue;
                            if (KPI == "True")
                            {
                                bKPI = true;
                            }
                            oAttrDef.bKPI = bKPI;
                        }
                        if (oBOI.Attributes.ContainsKey("BOID"))
                        {
                            oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, null, oBOI.Attributes["BOID"].sValue);
                            if (oBOD != null)
                            {
                                sTableName = oBOD.TableName;
                                iDataSourceID = oBOD.iDataSource;
                                iAppID = oBOD.FKiApplicationID;
                            }
                        }
                        oAttrList.Add(oAttrDef);

                        //Checking Column exists in Table or Not
                        oCR = Check_ColumnExists(sTableName, oAttrDef.Name, iDataSourceID);
                        oTrace.oTrace.Add(oCR.oTrace);
                        if (oCR.bOK && oCR.oResult != null)
                        {
                            var bColExist = (bool)oCR.oResult;
                            if (!bColExist)
                            {
                                oCR = Sync_BO(sTableName, oAttrList, iDataSourceID);
                                oTrace.oTrace.Add(oCR.oTrace);
                                if (oCR.bOK && oCR.oResult != null)
                                {
                                    oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "BO Attribute insertion completed" });

                                    //Amending AttributeID to Group
                                    var oGroups = new List<XIDGroup> {
                                                                new XIDGroup {GroupName="Details1"},
                                                                new XIDGroup {GroupName="Description"},
                                                                new XIDGroup {GroupName="List"},
                                                                new XIDGroup {GroupName="Create"},
                                                                new XIDGroup {GroupName="Label"},
                                                                new XIDGroup {GroupName="Search"},
                                                                new XIDGroup {GroupName="Quick Search"}
                                                              };
                                    XIDXI oXID = new XIDXI();
                                    foreach (var group in oGroups)
                                    {
                                        var oGroupD = oXID.Get_GroupDefinition(oBOD.BOID.ToString(), group.GroupName);
                                        if (oGroupD != null)
                                        {
                                            var ExistingIDs = oGroupD.BOFieldIDs.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                                            if (!ExistingIDs.Contains(iAttrID.ToString()))
                                            {
                                                if (oAttrDef.Name.ToLower() == XIConstant.Key_XIGUID.ToLower() && oAttrDef.Name.ToLower() == XIConstant.Key_Hierarchy.ToLower())
                                                {
                                                    oGroupD.BOFieldNames = oGroupD.BOFieldNames + ", " + oAttrDef.Name;
                                                    oGroupD.BOFieldIDs = oGroupD.BOFieldIDs + ", " + iAttrID.ToString();
                                                    oCR = Save_BOGroup(oGroupD);
                                                    oTrace.oTrace.Add(oCR.oTrace);
                                                    if (oCR.bOK && oCR.oResult != null)
                                                    {

                                                    }
                                                    else
                                                    {
                                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                }
                            }
                        }
                        else
                        {
                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                        }
                        if (oTrace.iStatus != (int)xiEnumSystem.xiFuncResult.xiError)
                        {
                            if (oAttrDef.iSystemType > 0 && oAttrDef.bKPI)
                            {
                                oCR = Generate_KPI(oAttrDef.iSystemType.ToString(), oAttrDef.Name);
                                oTrace.oTrace.Add(oCR.oTrace);
                                if (oCR.bOK && oCR.oResult != null)
                                {

                                }
                                else
                                {
                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                }
                            }
                            if (oAttrDef.bFKGUID)
                            {
                                oAttrDef.Name = oAttrDef.Name + "XIGUID";
                                oCR = Check_ColumnExists(sTableName, oAttrDef.Name, iDataSourceID);
                                oTrace.oTrace.Add(oCR.oTrace);
                                if (oCR.bOK && oCR.oResult != null)
                                {
                                    var bColExist = (bool)oCR.oResult;
                                    if (!bColExist)
                                    {
                                        oAttrDef.TypeID = 250;
                                        oAttrDef.MaxLength = "0";
                                        oAttrDef.DataType = "uniqueidentifier NULL DEFAULT newid()";
                                        oCR = Save_BOAttribute(oAttrDef);
                                        if (oCR.bOK && oCR.oResult != null)
                                        {
                                            oCR = Assign_FKGUID(iBODID, sAttr, oAttrDef.Name);
                                        }
                                        else
                                        {
                                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                        }
                                    }
                                    else
                                    {
                                        oCR = Assign_FKGUID(iBODID, sAttr, oAttrDef.Name);
                                    }
                                }
                            }
                        }
                    }

                }
                if (oTrace.iStatus != (int)xiEnumSystem.xiFuncResult.xiError)
                {
                    oCResult = oCR;
                    oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                }
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error While Inserting BO Attribute" });
                //if (oSigCR != null)
                //{
                //    oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "BO Attribute insertion failed" });
                //    oSigCR.oStack.Add(new CNV { sName = "Error Message", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                //}
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            return oCResult;
        }

        public CResult Assign_FKGUID(int iBODID, string sAttr, string sFKAttr)
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
                XIIXI oXI = new XIIXI();
                XIInfraCache oCache = new XIInfraCache();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, null, iBODID.ToString());
                var sQuery = "select * from " + oBOD.TableName;
                XID1Click o1Click = new XID1Click();
                o1Click.BOID = iBODID;
                o1Click.Query = sQuery;
                var oResult = o1Click.OneClick_Run();
                if (oResult != null && oResult.Count() > 0)
                {
                    var oAttrD = oBOD.AttributeD(sAttr);
                    var FKBOName = oAttrD.sFKBOName;
                    Dictionary<string, object> Params = new Dictionary<string, object>();
                    Params["Name"] = FKBOName;
                    string sSelectFields = string.Empty;
                    sSelectFields = "Name,BOID,iDataSource,sSize,TableName,sPrimaryKey,sType";
                    var FKBOD = Connection.Select<XIDBO>("XIBO_T_N", Params, sSelectFields).FirstOrDefault();
                    foreach (var BOI in oResult.Values.ToList())
                    {
                        var sFKValue = BOI.AttributeI(sFKAttr).sValue;
                        if (string.IsNullOrEmpty(sFKValue))
                        {
                            var sValue = BOI.AttributeI(sAttr).sValue;
                            var FKBOI = oXI.BOI(FKBOD.Name, sValue);
                            if (FKBOI != null && FKBOI.Attributes.Count() > 0)
                            {
                                var FKGUID = FKBOI.AttributeI("xiguid").sValue;
                                BOI.SetAttribute(sFKAttr, FKGUID);
                                BOI.BOD = oBOD;
                                oCR = BOI.SaveV2(BOI);
                                oTrace.oTrace.Add(oCR.oTrace);
                                if (oCR.bOK && oCR.oResult != null)
                                {

                                }
                                else
                                {
                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                }
                            }
                        }
                    }
                }

                if (oTrace.iStatus != (int)xiEnumSystem.xiFuncResult.xiError)
                {
                    oCResult.oResult = "Success";
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                }

            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        #endregion Build_BOAttribute


        #region AssignMenu

        public CResult Save_AssignMenu(XIAssignMenu oMenu)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "Menu insertion started for " + oMenu.Name });
                }

                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                if (oMenu.sType == "AssignMenu")
                {
                    oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "AssignMenu");
                    oBOI.BOD = oBOD;
                    oBOI.SetAttribute("ID", oMenu.ID.ToString());
                    oBOI.SetAttribute("MenuID", oMenu.MenuID);
                    oBOI.SetAttribute("ParentID", oMenu.ParentID);
                    oBOI.SetAttribute("ActionType", oMenu.ActionType.ToString());
                    oBOI.SetAttribute("XiLinkID", oMenu.XiLinkID.ToString());
                    oBOI.SetAttribute("Priority", "0");
                    oBOI.SetAttribute("OrgID", oMenu.OrgID.ToString());
                    oBOI.SetAttribute("RoleID", oMenu.RoleID.ToString());
                    oBOI.SetAttribute("Name", oMenu.Name);
                    oBOI.SetAttribute("MenuController", oMenu.MenuController);
                    oBOI.SetAttribute("MenuAction", oMenu.MenuAction);
                    oBOI.SetAttribute("FKiApplicationID", iAppID.ToString());
                    oBOI.SetAttribute("RootName", oMenu.RootName);
                    oBOI.SetAttribute("StatusTypeID", "10");
                    oBOI.SetAttribute("FKiMenuID", oMenu.FKiMenuID.ToString());
                    oBOI.SetAttribute("CreatedBy", iUserID.ToString());
                    oBOI.SetAttribute("CreatedTime", DateTime.Now.ToString());
                    oBOI.SetAttribute("CreatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                    oBOI.SetAttribute("UpdatedBy", iUserID.ToString());
                    oBOI.SetAttribute("UpdatedTime", DateTime.Now.ToString());
                    oBOI.SetAttribute("UpdatedBySYSID", Dns.GetHostAddresses(Dns.GetHostName())[1].ToString());
                    oCR = oBOI.Save(oBOI);
                    if (oCR.bOK && oCR.oResult != null)
                    {
                        if (oSigCR != null)
                        {
                            oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "Menu insertion completed for " + oMenu.Name });
                        }
                        var oMenuDef = (XIIBO)oCR.oResult;
                        var sMenuID = oMenuDef.Attributes.Where(m => m.Key.ToLower() == "id").Select(m => m.Value).Select(m => m.sValue).FirstOrDefault();
                        oBOI.SetAttribute("MenuID", sMenuID);
                        oCR = oBOI.Save(oBOI);
                        if (oCR.bOK && oCR.oResult != null)
                        {

                        }
                    }
                    else
                    {
                        oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Menu insertion failed for " + oMenu.Name });
                    }
                }
                else if (oMenu.sType == "CreateMenu")
                {
                    oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "CreateMenu");
                    oBOI.BOD = oBOD;
                    oBOI.SetAttribute("ID", oMenu.ID.ToString());
                    oBOI.SetAttribute("sName", oMenu.Name);
                    oBOI.SetAttribute("iActionType", oMenu.ActionType.ToString());
                    oBOI.SetAttribute("sMenuController", oMenu.MenuController);
                    oBOI.SetAttribute("sMenuAction", oMenu.MenuAction);
                    oBOI.SetAttribute("iXiLinkID", oMenu.XiLinkID.ToString());
                    oBOI.SetAttribute("iStatusTypeID", "10");
                    oCR = oBOI.Save(oBOI);
                    if (oCR.bOK && oCR.oResult != null)
                    {
                        if (oSigCR != null)
                        {
                            oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "Menu insertion completed for " + oMenu.Name });
                        }
                    }
                    else
                    {
                        oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Menu insertion failed for " + oMenu.Name });
                    }
                }
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                oCResult.oResult = oBOI;
            }
            catch (Exception ex)
            {
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Menu insertion failed for " + oMenu.Name });
                oSigCR.oStack.Add(new CNV { sName = "Error Message", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting Menu for " + oMenu.Name });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            return oCResult;
        }

        #endregion AssignMenu

        #region BOUIDetails

        public CResult Get_BOUIDetails(XIDBOUI oUIDet)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "BOStructure Details insertion started for " + oUIDet.sBOName });
                }
                XIDLayout oLayDef = new XIDLayout();
                oLayDef.ID = oUIDet.iLayoutID;
                oLayDef = oLayDef.Get_XILayoutDetails();

                XIDXI oXID = new XIDXI();
                var oXIDDef = oXID.Get_XIBOStructureDefinition(oUIDet.FKiBOID, Convert.ToInt32(oUIDet.FKiStructureID), null);
                List<XIDStructure> Tree = new List<XIDStructure>();
                if (oXIDDef.bOK && oXIDDef.oResult != null)
                {
                    Tree = (List<XIDStructure>)oXIDDef.oResult;
                    sStructureName = Tree.FirstOrDefault().sCode;
                }
                var MainBO = Tree.FirstOrDefault().sBO;
                var MainBODID = Tree.FirstOrDefault().BOID;
                var iBODID = oUIDet.FKiBOID;
                var iStructureID = oUIDet.FKiStructureID;


                if (oUIDet.sSavingType.ToLower() == "save")
                {
                    Dictionary<string, object> Params = new Dictionary<string, object>();
                    Params["FKiBOID"] = iBODID;
                    Params["FKiStructureID"] = iStructureID;
                    var BOUI = Connection.Select<XIDBOUI>("XIBOUIDetails_T", Params).FirstOrDefault();
                    if (BOUI != null && BOUI.ID > 0)
                    {
                        if (oLayDef.LayoutName.ToLower() == "TabLayoutTemplate".ToLower())
                        {
                            foreach (var BO in Tree)
                            {
                                if (MainBO.ToLower() != BO.sBO.ToLower())
                                {
                                    Dictionary<string, object> Param = new Dictionary<string, object>();
                                    Param["iParentStructureID"] = BO.FKiParentID;
                                    Param["FKiStructureID"] = BO.ID;
                                    var oStrDetail = Connection.Select<XIDStructure>("XIBOStructureDetail_T", Param).FirstOrDefault();
                                    if (oStrDetail != null && oStrDetail.ID > 0)
                                    {

                                    }
                                    else
                                    {
                                        XIDBO oBODef = new XIDBO();
                                        oBODef = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, BO.sBO);
                                        int i1ClickID = 0;
                                        XID1Click o1Click = new XID1Click();
                                        o1Click.FromBos = oBODef.TableName;
                                        sTableName = oBODef.TableName;
                                        sBOName = oBODef.Name; o1Click.Name = o1Click.Code = oBODef.Name; o1Click.Description = oBODef.Name + " Default 1-Click";
                                        iBODID = o1Click.BOID = oBODef.BOID;
                                        o1Click.DisplayAs = 50; o1Click.IsFilterSearch = true;
                                        o1Click.IsCreate = true;
                                        o1Click.IsRefresh = false;
                                        o1Click.CategoryID = 1;
                                        o1Click.sCreateType = "inlinetop";
                                        if (MainBODID != oBODef.BOID)
                                        {
                                            if (!string.IsNullOrEmpty(BO.sParentFKColumn))
                                            {
                                                o1Click.Query = "select {List} from " + oBODef.TableName + " where " + BO.sParentFKColumn + "={XIP|" + MainBO + ".id} order by " + oBODef.sPrimaryKey + " desc";
                                            }
                                            else
                                            {
                                                o1Click.Query = "select {List} from " + oBODef.TableName + " order by " + oBODef.sPrimaryKey + " desc";
                                            }
                                            Params = new Dictionary<string, object>();
                                            Params["FKiBOID"] = iBODID;
                                            var BODefault = Connection.Select<XIDBOUI>("XIBOUIDefault_T", Params).FirstOrDefault();
                                            if (BODefault != null && BODefault.ID > 0)
                                            {
                                                o1Click.IsRowClick = true;
                                                o1Click.RowXiLinkID = BODefault.iPopupID;
                                            }
                                        }
                                        var o1ClickDef = Save_1Click(o1Click);
                                        if (o1ClickDef.bOK && o1ClickDef.oResult != null)
                                        {
                                            i1ClickID = (int)o1ClickDef.oResult;
                                            // var s1ClickID = o1ClickDe.Attributes.Where(m => m.Key.ToLower() == "id").Select(m => m.Value).Select(m => m.sValue).FirstOrDefault();
                                            //int.TryParse(s1ClickID, out i1ClickID);
                                            XIDStructureDetail oBODet = new XIDStructureDetail();
                                            oBODet.i1ClickID = i1ClickID;
                                            oBODet.iParentStructureID = Tree.FirstOrDefault().ID;
                                            oBODet.FKiStructureID = BO.ID;
                                            var oSaveDet = Save_BOStrucuteDetails(oBODet);
                                            oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else if (oUIDet.sSavingType.ToLower() == "generate")
                {
                    int i = 0;
                    //var bIsMainBODone = false;
                    XIDBO RootBODef = new XIDBO();
                    RootBODef = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, MainBO);
                    //var RootName = Tree.FirstOrDefault().sName;
                    //bIsMainBODone = Tree.FirstOrDefault().bIsAutoCreateDone;
                    var iParentStructID = Tree.FirstOrDefault().ID;
                    //var BOStructIDs = Tree.Where(m => m.FKiParentID != "#").Select(m => m.ID).ToList();
                    XIDStructureDetail oStDetails = new XIDStructureDetail();
                    List<XIDStructureDetail> StructDetails = new List<XIDStructureDetail>();
                    oStDetails.iParentStructureID = iParentStructID;

                    var oStructDef = oStDetails.Get_XIStructureDetailsDefinition("");
                    if (oStructDef.bOK && oStructDef.oResult != null)
                    {
                        StructDetails = (List<XIDStructureDetail>)oStructDef.oResult;
                        if (StructDetails.Count() > 0)
                        {
                            foreach (var Remove in StructDetails)
                            {
                                Dictionary<string, object> Param = new Dictionary<string, object>();
                                Param["ID"] = Remove.ID;
                                var oRemove = Connection.Select<XIDStructure>("XIBOStructureDetail_T", Param).FirstOrDefault();
                                Connection.Delete<XIDStructure>(oRemove, "XIBOStructureDetail_T", "ID");
                            }
                        }
                    }

                    if (oLayDef.LayoutName.ToLower() == "Left Tree Layout".ToLower())
                    {
                        foreach (var BO in Tree)
                        {
                            XIDBO oBODef = new XIDBO();
                            oBODef = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, BO.sBO);
                            if (oBODef != null)
                            {
                                int i1ClickID = 0;
                                XID1Click o1Click = new XID1Click();
                                o1Click.FromBos = oBODef.TableName;
                                sTableName = oBODef.TableName;
                                sBOName = oBODef.Name; o1Click.Name = o1Click.Code = oBODef.Name; o1Click.Description = oBODef.Name + " Default 1-Click";
                                iBODID = o1Click.BOID = oBODef.BOID;
                                o1Click.DisplayAs = 50; o1Click.IsFilterSearch = true;
                                o1Click.IsCreate = true;
                                o1Click.IsRefresh = false;
                                o1Click.CategoryID = 1;
                                o1Click.sCreateType = "inlinetop";
                                var o1ClickDef = Save_1Click(o1Click);
                                if (o1ClickDef.bOK && o1ClickDef.oResult != null)
                                {
                                    var o1ClickDe = (XIIBO)o1ClickDef.oResult;
                                    var s1ClickID = o1ClickDe.Attributes.Where(m => m.Key.ToLower() == "id").Select(m => m.Value).Select(m => m.sValue).FirstOrDefault();
                                    int.TryParse(s1ClickID, out i1ClickID);
                                }
                                List<CNV> mParams = new List<CNV>();
                                mParams.Add(new CNV { sName = XIConstant.Param_1ClickID, sValue = i1ClickID.ToString() });
                                mParams.Add(new CNV { sName = XIConstant.Param_BO, sValue = oBODef.Name });
                                mParams.Add(new CNV { sName = XIConstant.Param_ApplicationID, sValue = iAppID.ToString() });
                                mParams.Add(new CNV { sName = XIConstant.Param_BODID, sValue = RootBODef.BOID.ToString() });
                                mParams.Add(new CNV { sName = "structurecode", sValue = Tree.FirstOrDefault().sCode });
                                var oBOPopupDef = Save_BOTreePopup(mParams);
                                if (oBOPopupDef.bOK && oBOPopupDef.oResult != null)
                                {
                                    var oQSDef = (XIIBO)oBOPopupDef.oResult;
                                    iQSID = Convert.ToInt32(oQSDef.Attributes.Where(m => m.Key.ToLower() == "id").Select(m => m.Value).Select(m => m.sValue).FirstOrDefault());
                                }

                                //Create BOUIDefault
                                XIDBODefault oDefault = new XIDBODefault();
                                oDefault.FKiBOID = oBODef.BOID;
                                oDefault.iPopupID = iRowXilinkID;
                                var oBODefault = Save_BOUIDefault(oDefault);
                                if (oBODefault.bOK && oBODefault.oResult != null)
                                {

                                }

                                //Inserting Structure Details into BOStructure Details
                                XIDStructureDetail oBODet = new XIDStructureDetail();
                                oBODet.i1ClickID = i1ClickID;
                                oBODet.iParentStructureID = iParentStructID;
                                oBODet.FKiStructureID = BO.ID;
                                var oSaveDet = Save_BOStrucuteDetails(oBODet);
                                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                            }
                            i++;
                        }
                    }
                    else if (oLayDef.LayoutName.ToLower() == "TabLayoutTemplate".ToLower())
                    {
                        foreach (var BO in Tree)
                        {
                            XIDBO oBODef = new XIDBO();
                            oBODef = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, BO.sBO);
                            if (oBODef != null)
                            {
                                int i1ClickID = 0;
                                XID1Click o1Click = new XID1Click();
                                o1Click.FromBos = oBODef.TableName;
                                sTableName = oBODef.TableName;
                                sBOName = oBODef.Name; o1Click.Name = o1Click.Code = oBODef.Name; o1Click.Description = oBODef.Name + " Default 1-Click";
                                iBODID = o1Click.BOID = oBODef.BOID;
                                o1Click.DisplayAs = 50; o1Click.IsFilterSearch = true;
                                o1Click.IsCreate = true;
                                o1Click.IsRefresh = false;
                                o1Click.CategoryID = 1;
                                o1Click.sCreateType = "inlinetop";
                                if (MainBODID != oBODef.BOID)
                                {
                                    if (!string.IsNullOrEmpty(BO.sParentFKColumn))
                                    {
                                        o1Click.Query = "select {List} from " + oBODef.TableName + " where " + BO.sParentFKColumn + "={XIP|" + MainBO + ".id} order by " + oBODef.sPrimaryKey + " desc";
                                    }
                                    else
                                    {
                                        o1Click.Query = "select {List} from " + oBODef.TableName + " order by " + oBODef.sPrimaryKey + " desc";
                                    }
                                    Dictionary<string, object> Params = new Dictionary<string, object>();
                                    Params["FKiBOID"] = iBODID;
                                    var BODefault = Connection.Select<XIDBOUI>("XIBOUIDefault_T", Params).FirstOrDefault();
                                    if (BODefault != null && BODefault.ID > 0)
                                    {
                                        o1Click.IsRowClick = true;
                                        o1Click.RowXiLinkID = BODefault.iPopupID;
                                    }
                                }
                                var o1ClickDef = Save_1Click(o1Click);
                                if (o1ClickDef.bOK && o1ClickDef.oResult != null)
                                {
                                    i1ClickID = (int)o1ClickDef.oResult;
                                    // var s1ClickID = o1ClickDe.Attributes.Where(m => m.Key.ToLower() == "id").Select(m => m.Value).Select(m => m.sValue).FirstOrDefault();
                                    //int.TryParse(s1ClickID, out i1ClickID);
                                }
                                if (MainBODID == oBODef.BOID)
                                {
                                    List<CNV> mParams = new List<CNV>();
                                    mParams.Add(new CNV { sName = XIConstant.Param_1ClickID, sValue = i1ClickID.ToString() });
                                    mParams.Add(new CNV { sName = XIConstant.Param_BO, sValue = oBODef.Name });
                                    mParams.Add(new CNV { sName = XIConstant.Param_ApplicationID, sValue = iAppID.ToString() });
                                    var oBOPopupDef = Save_BOPopup(mParams);
                                    if (oBOPopupDef.bOK && oBOPopupDef.oResult != null)
                                    {
                                        iQSID = (int)oBOPopupDef.oResult;
                                        //iQSID = Convert.ToInt32(oQSDef.Attributes.Where(m => m.Key.ToLower() == "id").Select(m => m.Value).Select(m => m.sValue).FirstOrDefault());
                                    }

                                    //Create BOUIDefault
                                    XIDBODefault oDefault = new XIDBODefault();
                                    oDefault.FKiBOID = oBODef.BOID;
                                    oDefault.iPopupID = iRowXilinkID;
                                    oDefault.iType = 20;
                                    oDefault.iStructureID = Convert.ToInt32(oUIDet.FKiStructureID);
                                    var oBODefault = Save_BOUIDefault(oDefault);
                                    if (oBODefault.bOK && oBODefault.oResult != null)
                                    {

                                    }
                                    XIDBOUI oBOUI = new XIDBOUI();
                                    oBOUI.FKiBOID = oBODef.BOID;
                                    oBOUI.FKiStructureID = oUIDet.FKiStructureID;
                                    oBOUI.i1ClickID = i1ClickID;
                                    oCR = Save_BOUIDetails(oBOUI);
                                    //oTrace.oTrace.Add(oCR.oTrace);
                                    if (oCR.bOK && oCR.oResult != null)
                                    {

                                    }
                                }

                                //Inserting Structure Details into BOStructure Details
                                XIDStructureDetail oBODet = new XIDStructureDetail();
                                oBODet.i1ClickID = i1ClickID;
                                oBODet.iParentStructureID = iParentStructID;
                                oBODet.FKiStructureID = BO.ID;
                                var oSaveDet = Save_BOStrucuteDetails(oBODet);
                                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                            }
                            i++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "BOStructure Details insertion failed for " + oUIDet.sBOName });
                oSigCR.oStack.Add(new CNV { sName = "Error Message", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting BOStructure Details for " + oUIDet.sBOName });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            return oCResult;
        }

        #endregion BOUIDetails

        public CResult Save_BOStrucuteDetails(XIDStructureDetail oSD)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            //tracing
            // if tracing is on (which is a config switch for this user) then
            //   oCResult.Trace.Write
            //ALL OK?
            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "BOStructure Details insertion started for " + sBOName });
                }
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "BO StructureDetails");
                oBOI.BOD = oBOD;
                oBOI.SetAttribute("ID", oSD.ID.ToString());
                oBOI.SetAttribute("iParentStructureID", oSD.iParentStructureID.ToString());
                oBOI.SetAttribute("FKiStructureID", oSD.FKiStructureID.ToString());
                oBOI.SetAttribute("iTabXiLinkID", oSD.iTabXiLinkID.ToString());
                oBOI.SetAttribute("i1ClickID", oSD.i1ClickID.ToString());
                oBOI.SetAttribute("iCreateDialogID", oSD.iCreateDialogID.ToString());
                oBOI.SetAttribute("iEditDialogID", oSD.iEditDialogID.ToString());
                oBOI.SetAttribute("iCreateFormXiLinkID", oSD.iCreateFormXiLinkID.ToString());
                oBOI.SetAttribute("iEditFormXiLinkID", oSD.iEditFormXiLinkID.ToString());
                oBOI.SetAttribute("iCreateLayoutID", oSD.iCreateLayoutID.ToString());
                oBOI.SetAttribute("iEditLayoutID", oSD.iEditLayoutID.ToString());
                oCR = oBOI.Save(oBOI);
                if (oCR.bOK && oCR.oResult != null)
                {
                    if (oSigCR != null)
                    {
                        oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "BOStructure Details insertion Success for " + sBOName });
                    }
                }
                else
                {
                    oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "BOStructure Details insertion failed for " + sBOName });
                }
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                oCResult.oResult = oBOI;
            }
            catch (Exception ex)
            {
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "BOStructure Details insertion failed" });
                oSigCR.oStack.Add(new CNV { sName = "Error Message", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting BOStructure Details" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            return oCResult;
        }

        #region Generate TreePopup

        public CResult Save_BOTreePopup(List<CNV> oParams)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "BO Tree popup insertion started" });
                }
                int iQSDID = 0;
                var sBO = oParams.Where(m => m.sName.ToLower() == XIConstant.Param_BO.ToLower()).Select(m => m.sValue).FirstOrDefault();
                var sBODID = oParams.Where(m => m.sName.ToLower() == XIConstant.Param_BODID.ToLower()).Select(m => m.sValue).FirstOrDefault();
                var sAppID = oParams.Where(m => m.sName.ToLower() == XIConstant.Param_ApplicationID.ToLower()).Select(m => m.sValue).FirstOrDefault();
                var i1ClickID = oParams.Where(m => m.sName.ToLower() == XIConstant.Param_1ClickID.ToLower()).Select(m => m.sValue).FirstOrDefault();
                int iAppID = 0;
                int.TryParse(sAppID, out iAppID);
                int iBOID = 0;
                int.TryParse(sBODID, out iBOID);
                var sStructureCode = oParams.Where(m => m.sName.ToLower() == "structurecode").Select(m => m.sValue).FirstOrDefault();
                var oAPPD = (XIDApplication)oCache.GetObjectFromCache(XIConstant.CacheApplication, null, iAppID.ToString());
                XIIBO oQSI = new XIIBO();
                //Save Question Set
                XIDQS oQSD = new XIDQS();
                oQSD.sName = sBO + " Tree Popup Question Set";
                oQSD.sDescription = sBO + " Tree Popup Question Set";
                oQSD.StatusTypeID = 10;
                oQSD.FKiApplicationID = iAppID;
                oQSD.FKiOrganisationID = 0;
                oQSD.bInMemoryOnly = true;
                oQSD.bIsStage = false;
                oQSD.SaveType = "Save at End";
                oCR = Save_QuestionSet(oQSD);
                if (oCR.bOK && oCR.oResult != null)
                {
                    int iBOViewStepID = 0;
                    int iTabStepID = 0;
                    int iBOStepID = 0;
                    oQSI = (XIIBO)oCR.oResult;
                    iQSDID = Convert.ToInt32(oQSI.Attributes.Where(m => m.Key.ToLower() == "id").Select(m => m.Value).Select(m => m.sValue).FirstOrDefault());
                    if (iQSDID > 0)
                    {
                        //Step1 with Layout
                        XIDLayout oXLs = new XIDLayout();
                        oXLs.LayoutName = XIConstant.LeftTreeLayout;
                        var Layout2 = oXLs.Get_XILayoutDetails();
                        CResult NewStepLayout = new CResult();
                        var iStepLayoutID = 0;
                        if (Layout2 != null)
                        {
                            NewStepLayout = Copy_Layout(Layout2, true);
                            if (NewStepLayout.bOK && NewStepLayout.oResult != null)
                            {
                                var NewStepDef = (XIIBO)NewStepLayout.oResult;
                                var sNewStepLayoutID = NewStepDef.Attributes.Where(m => m.Key.ToLower() == "id").Select(s => s.Value).Select(m => m.sValue).FirstOrDefault();
                                iStepLayoutID = Convert.ToInt32(sNewStepLayoutID);
                            }
                        }

                        //Step with Layout
                        XIDQSStep oStep = new XIDQSStep();
                        oStep.sName = "Step1 with Layout";
                        oStep.FKiQSDefintionID = iQSDID;
                        oStep.iLayoutID = iStepLayoutID;
                        oStep.StatusTypeID = 10;
                        oStep.iDisplayAs = 10;
                        oCR = Save_QuestionSetStep(oStep);
                        if (oCR.bOK && oCR.oResult != null)
                        {

                        }

                        //Step with Left Tree
                        oStep = new XIDQSStep();
                        oStep.sName = "Step2 Left Tree";
                        oStep.FKiQSDefintionID = iQSDID;
                        oStep.iLayoutID = 0;
                        oStep.StatusTypeID = 10;
                        oStep.iDisplayAs = 20;
                        oCR = Save_QuestionSetStep(oStep);
                        if (oCR.bOK && oCR.oResult != null)
                        {
                            XIIBO oStepD = (XIIBO)oCR.oResult;
                            var sStepID = oStepD.Attributes.Values.Where(m => m.sName.ToLower() == "id".ToLower()).Select(m => m.sValue).FirstOrDefault();
                            int.TryParse(sStepID, out iBOViewStepID);
                            if (iBOViewStepID > 0)
                            {
                                XIDQSSection oSection = new XIDQSSection();
                                oSection.FKiStepDefinitionID = iBOViewStepID;
                                oSection.iDisplayAs = 40;
                                oSection.iXIComponentID = 5;
                                oCR = Save_QuestionSetStepSection(oSection);
                                if (oCR.bOK && oCR.oResult != null)
                                {
                                    XIIBO oSectionD = (XIIBO)oCR.oResult;
                                    int iSectionID = 0;
                                    var sSectionID = oSectionD.Attributes.Values.Where(m => m.sName.ToLower() == "id".ToLower()).Select(m => m.sValue).FirstOrDefault();
                                    int.TryParse(sSectionID, out iSectionID);
                                    if (iSectionID > 0)
                                    {
                                        List<CNV> nParams = new List<CNV>();
                                        nParams.Add(new CNV { sName = "iBODID", sValue = iBOID.ToString() });
                                        nParams.Add(new CNV { sName = "sCode", sValue = sStructureCode });
                                        nParams.Add(new CNV { sName = "sMode", sValue = "Popup" });
                                        nParams.Add(new CNV { sName = "watchparam1", sValue = "-listinstance" });
                                        nParams.Add(new CNV { sName = "Nodeclickparamname", sValue = "-oneclickid" });
                                        nParams.Add(new CNV { sName = "register", sValue = "yes" });

                                        foreach (var Param in nParams)
                                        {
                                            XIDComponentParam oParam = new XIDComponentParam();
                                            oParam.sName = Param.sName;
                                            oParam.sValue = Param.sValue;
                                            oParam.iStepSectionID = iSectionID;
                                            oParam.FKiComponentID = 5;
                                            oCR = Save_ComponentParam(oParam);
                                            if (oCR.bOK && oCR.oResult != null)
                                            {

                                            }
                                        }
                                    }
                                }
                            }
                        }

                        //Step with Main BO Form
                        oStep = new XIDQSStep();
                        oStep.sName = "Step3 with Main BO Form";
                        oStep.FKiQSDefintionID = iQSDID;
                        oStep.iLayoutID = 0;
                        oStep.StatusTypeID = 10;
                        oStep.iDisplayAs = 20;
                        oCR = Save_QuestionSetStep(oStep);
                        if (oCR.bOK && oCR.oResult != null)
                        {
                            XIIBO oStepD = (XIIBO)oCR.oResult;
                            var sStepID = oStepD.Attributes.Values.Where(m => m.sName.ToLower() == "id".ToLower()).Select(m => m.sValue).FirstOrDefault();
                            int.TryParse(sStepID, out iTabStepID);
                            if (iTabStepID > 0)
                            {
                                XIDQSSection oSection = new XIDQSSection();
                                oSection.FKiStepDefinitionID = iTabStepID;
                                oSection.iDisplayAs = 40;
                                oSection.iXIComponentID = 11;
                                oCR = Save_QuestionSetStepSection(oSection);
                                if (oCR.bOK && oCR.oResult != null)
                                {
                                    XIIBO oSectionD = (XIIBO)oCR.oResult;
                                    int iSectionID = 0;
                                    var sSectionID = oSectionD.Attributes.Values.Where(m => m.sName.ToLower() == "id".ToLower()).Select(m => m.sValue).FirstOrDefault();
                                    int.TryParse(sSectionID, out iSectionID);
                                    if (iSectionID > 0)
                                    {
                                        List<CNV> nParams = new List<CNV>();
                                        nParams.Add(new CNV { sName = "BO", sValue = sBO });
                                        nParams.Add(new CNV { sName = "Group", sValue = "Details1" });
                                        nParams.Add(new CNV { sName = "LockGroup", sValue = "Lock" });
                                        nParams.Add(new CNV { sName = "iInstanceID", sValue = "{-iInstanceID}" });
                                        foreach (var Param in nParams)
                                        {
                                            XIDComponentParam oParam = new XIDComponentParam();
                                            oParam.sName = Param.sName;
                                            oParam.sValue = Param.sValue;
                                            oParam.iStepSectionID = iSectionID;
                                            oParam.FKiComponentID = 2;
                                            oCR = Save_ComponentParam(oParam);
                                            if (oCR.bOK && oCR.oResult != null)
                                            {

                                            }
                                        }
                                    }
                                }
                            }
                        }
                        //Step with BO Component
                        oStep = new XIDQSStep();
                        oStep.sName = "Step4 with Summary BO Form";
                        oStep.FKiQSDefintionID = iQSDID;
                        oStep.iLayoutID = 0;
                        oStep.StatusTypeID = 10;
                        oStep.iDisplayAs = 20;
                        oCR = Save_QuestionSetStep(oStep);
                        if (oCR.bOK && oCR.oResult != null)
                        {
                            XIIBO oStepD = (XIIBO)oCR.oResult;
                            var sStepID = oStepD.Attributes.Values.Where(m => m.sName.ToLower() == "id".ToLower()).Select(m => m.sValue).FirstOrDefault();
                            int.TryParse(sStepID, out iBOStepID);
                            if (iBOStepID > 0)
                            {
                                XIDQSSection oSection = new XIDQSSection();
                                oSection.FKiStepDefinitionID = iBOStepID;
                                oSection.iDisplayAs = 40;
                                oSection.iXIComponentID = 2;
                                oCR = Save_QuestionSetStepSection(oSection);
                                if (oCR.bOK && oCR.oResult != null)
                                {
                                    XIIBO oSectionD = (XIIBO)oCR.oResult;
                                    int iSectionID = 0;
                                    var sSectionID = oSectionD.Attributes.Values.Where(m => m.sName.ToLower() == "id".ToLower()).Select(m => m.sValue).FirstOrDefault();
                                    int.TryParse(sSectionID, out iSectionID);
                                    if (iSectionID > 0)
                                    {
                                        List<CNV> nParams = new List<CNV>();
                                        nParams.Add(new CNV { sName = "BO", sValue = sBO });
                                        nParams.Add(new CNV { sName = "Group", sValue = "Description" });
                                        nParams.Add(new CNV { sName = "DisplayMode", sValue = "View" });
                                        nParams.Add(new CNV { sName = "iInstanceID", sValue = "{-iInstanceID}" });
                                        foreach (var Param in nParams)
                                        {
                                            XIDComponentParam oParam = new XIDComponentParam();
                                            oParam.sName = Param.sName;
                                            oParam.sValue = Param.sValue;
                                            oParam.iStepSectionID = iSectionID;
                                            oParam.FKiComponentID = 2;
                                            oCR = Save_ComponentParam(oParam);
                                            if (oCR.bOK && oCR.oResult != null)
                                            {

                                            }
                                        }
                                    }
                                }
                            }
                        }
                        //Step with 1-Click Component
                        oStep = new XIDQSStep();
                        oStep.sName = "Step5 with Layout";
                        oStep.FKiQSDefintionID = iQSDID;
                        oStep.iLayoutID = 0;
                        oStep.StatusTypeID = 10;
                        oStep.iDisplayAs = 20;
                        oCR = Save_QuestionSetStep(oStep);
                        if (oCR.bOK && oCR.oResult != null)
                        {
                            XIIBO oStepD = (XIIBO)oCR.oResult;
                            int iStepID = 0;
                            var sStepID = oStepD.Attributes.Values.Where(m => m.sName.ToLower() == "id".ToLower()).Select(m => m.sValue).FirstOrDefault();
                            int.TryParse(sStepID, out iStepID);
                            if (iStepID > 0)
                            {
                                XIDQSSection oSection = new XIDQSSection();
                                oSection.FKiStepDefinitionID = iStepID;
                                oSection.iDisplayAs = 40;
                                oSection.iXIComponentID = 3;
                                oCR = Save_QuestionSetStepSection(oSection);
                                if (oCR.bOK && oCR.oResult != null)
                                {
                                    XIIBO oSectionD = (XIIBO)oCR.oResult;
                                    int iSectionID = 0;
                                    var sSectionID = oSectionD.Attributes.Values.Where(m => m.sName.ToLower() == "id".ToLower()).Select(m => m.sValue).FirstOrDefault();
                                    int.TryParse(sSectionID, out iSectionID);
                                    if (iSectionID > 0)
                                    {
                                        List<CNV> nParams = new List<CNV>();
                                        nParams.Add(new CNV { sName = "1ClickID", sValue = "{XIP|i1ClickID}" });
                                        nParams.Add(new CNV { sName = "Listclickparamname", sValue = "-listinstance" });
                                        nParams.Add(new CNV { sName = "watchparam1", sValue = "-oneclickid" });
                                        nParams.Add(new CNV { sName = "register", sValue = "yes" });
                                        foreach (var Param in nParams)
                                        {
                                            XIDComponentParam oParam = new XIDComponentParam();
                                            oParam.sName = Param.sName;
                                            oParam.sValue = Param.sValue;
                                            oParam.iStepSectionID = iSectionID;
                                            oParam.FKiComponentID = 3;
                                            oCR = Save_ComponentParam(oParam);
                                            if (oCR.bOK && oCR.oResult != null)
                                            {

                                            }
                                        }
                                    }
                                }
                            }
                        }

                        //Step with BO Component
                        oStep = new XIDQSStep();
                        oStep.sName = "Step7 with BO Component";
                        oStep.FKiQSDefintionID = iQSDID;
                        oStep.iLayoutID = 0;
                        oStep.StatusTypeID = 10;
                        oStep.iDisplayAs = 20;
                        oCR = Save_QuestionSetStep(oStep);
                        if (oCR.bOK && oCR.oResult != null)
                        {
                            XIIBO oStepD = (XIIBO)oCR.oResult;
                            var sStepID = oStepD.Attributes.Values.Where(m => m.sName.ToLower() == "id".ToLower()).Select(m => m.sValue).FirstOrDefault();
                            int.TryParse(sStepID, out iBOStepID);
                            if (iBOStepID > 0)
                            {
                                XIDQSSection oSection = new XIDQSSection();
                                oSection.FKiStepDefinitionID = iBOStepID;
                                oSection.iDisplayAs = 40;
                                oSection.iXIComponentID = 2;
                                oCR = Save_QuestionSetStepSection(oSection);
                                if (oCR.bOK && oCR.oResult != null)
                                {
                                    XIIBO oSectionD = (XIIBO)oCR.oResult;
                                    int iSectionID = 0;
                                    var sSectionID = oSectionD.Attributes.Values.Where(m => m.sName.ToLower() == "id".ToLower()).Select(m => m.sValue).FirstOrDefault();
                                    int.TryParse(sSectionID, out iSectionID);
                                    if (iSectionID > 0)
                                    {
                                        List<CNV> nParams = new List<CNV>();
                                        nParams.Add(new CNV { sName = "BO", sValue = "{XIP|ActiveBO}" });
                                        nParams.Add(new CNV { sName = "Group", sValue = "Details1" });
                                        nParams.Add(new CNV { sName = "watchparam1", sValue = "-listinstance" });
                                        nParams.Add(new CNV { sName = "register", sValue = "yes" });
                                        foreach (var Param in nParams)
                                        {
                                            XIDComponentParam oParam = new XIDComponentParam();
                                            oParam.sName = Param.sName;
                                            oParam.sValue = Param.sValue;
                                            oParam.iStepSectionID = iSectionID;
                                            oParam.FKiComponentID = 2;
                                            oCR = Save_ComponentParam(oParam);
                                            if (oCR.bOK && oCR.oResult != null)
                                            {

                                            }
                                        }
                                    }
                                }
                            }
                        }

                        var oLayoutD = (XIDLayout)oCache.GetObjectFromCache(XIConstant.CacheLayout, null, iStepLayoutID.ToString());
                        if (oLayoutD != null && oLayoutD.ID > 0)
                        {
                            for (int i = 0; i < oLayoutD.LayoutDetails.Count(); i++)
                            {
                                XIDLayoutMapping oMap = new XIDLayoutMapping();
                                oMap.PopupLayoutID = oLayoutD.ID;
                                oMap.PlaceHolderID = oLayoutD.LayoutDetails[i].PlaceHolderID;
                                if (i == 0)
                                {
                                    oMap.XiLinkID = iBOViewStepID;
                                }
                                else if (i == 1)
                                {
                                    oMap.XiLinkID = iTabStepID;
                                }
                                else if (i == 2)
                                {
                                    oMap.XiLinkID = iBOStepID;
                                }
                                oMap.ContentType = "Step";
                                oMap.StatusTypeID = 10;
                                oCR = Save_LayoutMapping(oMap);
                                if (oCR.bOK && oCR.oResult != null)
                                {

                                }
                            }
                        }
                    }
                }

                //Create Layout
                XIIBO oBOI = new XIIBO();
                int iLayoutID = 0;
                XIDLayout oLayout = new XIDLayout();
                oLayout.FKiApplicationID = iAppID;
                oLayout.LayoutName = sBO + " QS Tree Layout";
                oLayout.LayoutCode = XIConstant.DefaultLayout;
                oLayout.LayoutType = "Dialog";
                oLayout.LayoutLevel = "OrganisationLevel";
                oLayout.Authentication = "Authenticated";
                oLayout.StatusTypeID = 10;
                oCR = Save_Layout(oLayout);
                if (oCR.bOK && oCR.oResult != null)
                {
                    oBOI = (XIIBO)oCR.oResult;
                    var sLayoutID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "id").Select(m => m.sValue).FirstOrDefault();
                    int.TryParse(sLayoutID, out iLayoutID);
                    if (iLayoutID > 0)
                    {
                        int iPlaceHolderID = 0;
                        XIDLayoutDetails oDetail = new XIDLayoutDetails();
                        oDetail.PlaceHolderID = 0;
                        oDetail.FKiApplicationID = iAppID;
                        oDetail.LayoutID = iLayoutID;
                        oDetail.PlaceholderName = oLayout.LayoutName + " Content";
                        oDetail.PlaceholderArea = "div1";
                        oDetail.PlaceholderUniqueName = (oLayout.LayoutName + " Content").Replace(" ", "");
                        oDetail.PlaceholderClass = "col-md-12";
                        oCR = Save_LayoutDetail(oDetail);
                        if (oCR.bOK && oCR.oResult != null)
                        {
                            oBOI = (XIIBO)oCR.oResult;
                            var sPlaceHolderID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "PlaceHolderID".ToLower()).Select(m => m.sValue).FirstOrDefault();
                            int.TryParse(sPlaceHolderID, out iPlaceHolderID);
                            if (iPlaceHolderID > 0)
                            {
                                int iDialogID = 0;
                                XIDDialog oDialog = new XIDDialog();
                                oDialog.FKiApplicationID = iAppID;
                                oDialog.DialogName = sBO + " 1CS Dialog";
                                oDialog.PopupSize = "Default";
                                oDialog.LayoutID = iLayoutID;
                                oDialog.StatusTypeID = 10;
                                oCR = Save_Dialog(oDialog);
                                if (oCR.bOK && oCR.oResult != null)
                                {
                                    oBOI = (XIIBO)oCR.oResult;
                                    var sDialogID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "ID".ToLower()).Select(m => m.sValue).FirstOrDefault();
                                    int.TryParse(sDialogID, out iDialogID);
                                    if (iDialogID > 0)
                                    {
                                        XIDLayoutMapping oMap = new XIDLayoutMapping();
                                        oMap.PopupID = iDialogID;
                                        oMap.PopupLayoutID = iLayoutID;
                                        oMap.PlaceHolderID = iPlaceHolderID;
                                        oMap.XiLinkID = 8;
                                        oMap.ContentType = "XIComponent";
                                        oMap.StatusTypeID = 10;
                                        oCR = Save_LayoutMapping(oMap);
                                        if (oCR.bOK && oCR.oResult != null)
                                        {
                                            List<CNV> nParams = new List<CNV>();
                                            nParams.Add(new CNV { sName = "iQSDID", sValue = iQSDID.ToString() });
                                            nParams.Add(new CNV { sName = "sMode", sValue = "Popup" });
                                            foreach (var Param in nParams)
                                            {
                                                XIDComponentParam oParam = new XIDComponentParam();
                                                oParam.sName = Param.sName;
                                                oParam.sValue = Param.sValue;
                                                oParam.iLayoutMappingID = iPlaceHolderID;
                                                oParam.FKiComponentID = 8;
                                                oCR = Save_ComponentParam(oParam);
                                                if (oCR.bOK && oCR.oResult != null)
                                                {

                                                }
                                            }
                                            //Menu XILink
                                            int iXILinkID = 0;
                                            XILink oXILink = new XILink();
                                            oXILink.Name = sBO + " 1CS Row Click ";
                                            oXILink.URL = "XILink";
                                            oXILink.sActive = "Y";
                                            oXILink.StatusTypeID = 10;
                                            oCR = Save_XILink(oXILink);
                                            if (oCR.bOK && oCR.oResult != null)
                                            {
                                                oBOI = (XIIBO)oCR.oResult;
                                                var sXILinkID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "XILinkID".ToLower()).Select(m => m.sValue).FirstOrDefault();
                                                int.TryParse(sXILinkID, out iXILinkID);
                                                iRowXilinkID = iXILinkID;
                                                if (iXILinkID > 0)
                                                {
                                                    XiLinkNV oNV = new XiLinkNV();
                                                    oNV.XiLinkID = iXILinkID;
                                                    oNV.Name = "StartAction";
                                                    oNV.Value = "Dialog";
                                                    Save_XILinkNV(oNV);
                                                    oNV = new XiLinkNV();
                                                    oNV.XiLinkID = iXILinkID;
                                                    oNV.Name = "DialogID";
                                                    oNV.Value = iDialogID.ToString();
                                                    Save_XILinkNV(oNV);
                                                }

                                                XIIXI oXI = new XIIXI();
                                                List<CNV> oWhereParams = new List<CNV>();
                                                oWhereParams.Add(new CNV { sName = "id", sValue = i1ClickID.ToString() });
                                                var o1ClickI = oXI.BOI("XI1Click", null, null, oWhereParams);
                                                o1ClickI.SetAttribute("IsRowClick", "true");
                                                o1ClickI.SetAttribute("RowXiLinkID", iXILinkID.ToString());
                                                oCR = o1ClickI.SaveV2(o1ClickI);
                                                if (oCR.bOK && oCR.oResult != null)
                                                {

                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
                else
                {
                    oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Layout insertion failed for - " + oLayout.LayoutName });
                }
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "BO popup insertion completed" });
                }
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                oCResult.oResult = oBOI;
            }
            catch (Exception ex)
            {
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "BO popup insertion failed" });
                oSigCR.oStack.Add(new CNV { sName = "Error Message", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting Menu" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            return oCResult;
        }

        #endregion Generate TreePopup

        public CResult UpdateQsInstance(List<CNV> oParams)
        {
            //create pop,inline and dailog for simple1click based on conditions
            //update rowxilinkid in xi1click
            //update qsinstanceid for navigation in simple1click
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";
            int iQSInstanceID = 0;
            //CResult oCResult = new CResult();
            XIDefinitionBase oXID = new XIDefinitionBase();
            try
            {
                XIIBO oBOI = new XIIBO();
                XIDBO oBODs = new XIDBO();
                oBODs = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XI1Click");
                string QSInstanceID = oParams.Where(m => m.sName.ToLower() == "iQSInstanceID".ToLower()).Select(m => m.sValue).FirstOrDefault();
                string OneClickID = oParams.Where(m => m.sName.ToLower() == "1ClickID".ToLower()).Select(m => m.sValue).FirstOrDefault();
                var o1ClickD = (XID1Click)oCache.GetObjectFromCache(XIConstant.Cache1Click, null, OneClickID.ToString());
                XIDBOUI oUIDet = new XIDBOUI();
                XIDBAPI Connection = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString);
                Dictionary<string, object> DefParams = new Dictionary<string, object>();
                DefParams["FKiBOID"] = o1ClickD.BOID;
                int FKiStructureID = 0;
                var structureid = Convert.ToString(o1ClickD.FKiStructureID);
                int.TryParse(structureid, out FKiStructureID);

                //int FKiStructureID = Convert.ToInt32(o1ClickD.FKiStructureID);
                // XIDBODefault StructDetpop = new XIDBODefault(); ;
                if (FKiStructureID > 0)
                {
                    var StructDetpop = Connection.Select<XIDBODefault>("XIBOUIDefault_T", DefParams).Where(s => s.FKiBOID == o1ClickD.BOID && s.iType == 20 && s.iStructureID == FKiStructureID).FirstOrDefault();
                    Dictionary<string, object> SelParams = new Dictionary<string, object>();
                    sStructureName = Connection.Select<XIDStructure>("XIBOStructure_T", SelParams).Where(s => s.ID == Convert.ToInt64(o1ClickD.FKiStructureID)).Select(s => s.sCode).FirstOrDefault();

                    if (o1ClickD.OnRowClickType == "20" && StructDetpop == null)
                    {
                        XIDLayout oXLs = new XIDLayout();
                        oXLs.LayoutName = XIConstant.PopupLayoutName;
                        var Layout2 = oXLs.Get_XILayoutDetails();
                        oUIDet.iLayoutID = Layout2.ID;
                        oUIDet.FKiBOID = o1ClickD.BOID;
                        oUIDet.FKiStructureID = FKiStructureID;
                        Get_BOUIDetails(oUIDet);

                    }
                }
                var StructDetIn = Connection.Select<XIDBODefault>("XIBOUIDefault_T", DefParams).Where(s => s.FKiBOID == o1ClickD.BOID && s.iType == 10).FirstOrDefault();
                if (StructDetIn == null && o1ClickD.OnRowClickType == "10")
                {
                    XIDXI XiLs = new XIDXI();
                    var xilink = (XILink)oCache.GetObjectFromCache(XIConstant.CacheXILink, XIConstant.SInlineXilink, "");
                    XIIBO oBOIStr = new XIIBO();
                    XIDBO oBODsStr = new XIDBO();
                    oBOIStr.BOD = oBODsStr;
                    oBODsStr = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIBOUIDefault");
                    oBOIStr.Attributes["FKiBOID".ToLower()] = new XIIAttribute { sName = "FKiBOID", sValue = o1ClickD.BOID.ToString(), bDirty = true };
                    // oBOIStr.Attributes["ID".ToLower()] = new XIIAttribute { sName = "ID", sValue = "0", bDirty = true };
                    oBOIStr.Attributes["iPopupID".ToLower()] = new XIIAttribute { sName = "iPopupID", sValue = xilink.XiLinkID.ToString(), bDirty = true };
                    oBOIStr.Attributes["iType".ToLower()] = new XIIAttribute { sName = "iType", sValue = 10.ToString(), bDirty = true };
                    oBOIStr.Attributes["i1ClickID".ToLower()] = new XIIAttribute { sName = "i1ClickID", sValue = OneClickID, bDirty = true };

                    oBOIStr.Attributes["ID".ToLower()] = new XIIAttribute { sName = "ID", sValue = "0", bDirty = true };
                    oBOIStr.BOD = oBODsStr;
                    oCResult = oBOIStr.Save(oBOIStr);
                }
                var StructDetDai = Connection.Select<XIDBODefault>("XIBOUIDefault_T", DefParams).Where(s => s.FKiBOID == o1ClickD.BOID && s.iType == 30).FirstOrDefault();
                if (StructDetDai == null && o1ClickD.OnRowClickType == "30")
                {
                    Create_Simple1ClickDialog(o1ClickD.FromBos, o1ClickD.BOID);
                }
                int RowXiLinkID = 0;
                if (o1ClickD.IsRowClick == true)
                {

                    DefParams["FKiBOID"] = o1ClickD.BOID;
                    if (o1ClickD.OnRowClickType == "10")
                    {
                        RowXiLinkID = Connection.Select<XIDBODefault>("XIBOUIDefault_T", DefParams).Where(s => s.iType == 10).Select(s => s.iPopupID).FirstOrDefault();
                    }
                    else if (o1ClickD.OnRowClickType == "20")
                    {
                        RowXiLinkID = Connection.Select<XIDBODefault>("XIBOUIDefault_T", DefParams).Where(s => s.FKiBOID == o1ClickD.BOID && s.iType == 20 && s.iStructureID == FKiStructureID).Select(s => s.iPopupID).FirstOrDefault();
                    }
                    else
                    {
                        RowXiLinkID = Connection.Select<XIDBODefault>("XIBOUIDefault_T", DefParams).Where(s => s.iType == 30).Select(s => s.iPopupID).FirstOrDefault(); ;
                    }
                }
                oBOI.Attributes["FKiQSInstanceID".ToLower()] = new XIIAttribute { sName = "FKiQSInstanceID", sValue = QSInstanceID, bDirty = true };
                oBOI.Attributes["ID".ToLower()] = new XIIAttribute { sName = "ID", sValue = OneClickID, bDirty = true };
                oBOI.Attributes["RowXiLinkID".ToLower()] = new XIIAttribute { sName = "RowXiLinkID", sValue = RowXiLinkID.ToString(), bDirty = true };
                string sGUID = oParams.Where(m => m.sName.ToLower() == "sGUID".ToLower()).Select(m => m.sValue).FirstOrDefault();
                var sSessionID = HttpContext.Current.Session.SessionID;
                oBOI.BOD = oBODs;
                oCResult = oBOI.Save(oBOI);
                oCache.Set_ParamVal(sSessionID, sGUID, "", "{XIP|1ClickID}", OneClickID, "", null);
                var skey = oCache.CacheKeyBuilder(XIConstant.Cache1Click, "", "") + "_" + OneClickID;
                XIInfraCache.RemoveCacheWithKey(skey);
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting BOStructure Details" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }

            return oCResult;
        }

        public CResult Create_Simple1ClickDialog(string sBO, int BOID)
        {
            //creating dailog for simple1click
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";
            try
            {
                //Create Layout
                XIIBO oBOI = new XIIBO();
                int iLayoutID = 0;
                XIDLayout oLayout = new XIDLayout();
                oLayout.FKiApplicationID = iAppID;
                oLayout.LayoutName = sBO + " Dailog Layout";
                oLayout.LayoutCode = XIConstant.DefaultLayout;
                oLayout.LayoutType = "Dialog";
                oLayout.LayoutLevel = "OrganisationLevel";
                oLayout.Authentication = "Authenticated";
                oLayout.StatusTypeID = 10;
                oCR = Save_Layout(oLayout);
                if (oCR.bOK && oCR.oResult != null)
                {
                    oBOI = (XIIBO)oCR.oResult;
                    var sLayoutID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "id").Select(m => m.sValue).FirstOrDefault();
                    int.TryParse(sLayoutID, out iLayoutID);
                    if (iLayoutID > 0)
                    {
                        int iPlaceHolderID = 0;
                        XIDLayoutDetails oDetail = new XIDLayoutDetails();
                        oDetail.PlaceHolderID = 0;
                        oDetail.FKiApplicationID = iAppID;
                        oDetail.LayoutID = iLayoutID;
                        oDetail.PlaceholderName = oLayout.LayoutName + " Content";
                        oDetail.PlaceholderArea = "div1";
                        oDetail.PlaceholderUniqueName = (oLayout.LayoutName + " Content").Replace(" ", "");
                        oDetail.PlaceholderClass = "col-md-12";
                        oCR = Save_LayoutDetail(oDetail);
                        if (oCR.bOK && oCR.oResult != null)
                        {
                            oBOI = (XIIBO)oCR.oResult;
                            var sPlaceHolderID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "PlaceHolderID".ToLower()).Select(m => m.sValue).FirstOrDefault();
                            int.TryParse(sPlaceHolderID, out iPlaceHolderID);
                            if (iPlaceHolderID > 0)
                            {
                                int iDialogID = 0;
                                XIDDialog oDialog = new XIDDialog();
                                oDialog.FKiApplicationID = iAppID;
                                oDialog.DialogName = sBO + " 1CS Dialog";
                                oDialog.PopupSize = "Default";
                                oDialog.LayoutID = iLayoutID;
                                oDialog.StatusTypeID = 10;
                                oCR = Save_Dialog(oDialog);
                                if (oCR.bOK && oCR.oResult != null)
                                {
                                    oBOI = (XIIBO)oCR.oResult;
                                    var sDialogID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "ID".ToLower()).Select(m => m.sValue).FirstOrDefault();
                                    int.TryParse(sDialogID, out iDialogID);
                                    if (iDialogID > 0)
                                    {
                                        XIDLayoutMapping oMap = new XIDLayoutMapping();
                                        oMap.PopupID = iDialogID;
                                        oMap.PopupLayoutID = iLayoutID;
                                        oMap.PlaceHolderID = iPlaceHolderID;
                                        oMap.XiLinkID = 2;
                                        oMap.ContentType = "XIComponent";
                                        oMap.StatusTypeID = 10;
                                        oCR = Save_LayoutMapping(oMap);
                                        if (oCR.bOK && oCR.oResult != null)
                                        {
                                            List<CNV> nParams = new List<CNV>();
                                            nParams.Add(new CNV { sName = "BO", sValue = sBO });
                                            nParams.Add(new CNV { sName = "Group", sValue = "Create" });
                                            nParams.Add(new CNV { sName = "iInstanceID", sValue = "{XIP|iInstanceID}" });

                                            foreach (var Param in nParams)
                                            {
                                                XIDComponentParam oParam = new XIDComponentParam();
                                                oParam.sName = Param.sName;
                                                oParam.sValue = Param.sValue;
                                                oParam.iLayoutMappingID = iPlaceHolderID;
                                                oParam.FKiComponentID = 2;
                                                oCR = Save_ComponentParam(oParam);
                                                if (oCR.bOK && oCR.oResult != null)
                                                {

                                                }
                                            }
                                            //Menu XILink
                                            int iXILinkID = 0;
                                            XILink oXILink = new XILink();
                                            oXILink.Name = sBO + " 1CS Row Click ";
                                            oXILink.URL = "XILink";
                                            oXILink.sActive = "Y";
                                            oXILink.StatusTypeID = 10;
                                            oCR = Save_XILink(oXILink);
                                            if (oCR.bOK && oCR.oResult != null)
                                            {
                                                oBOI = (XIIBO)oCR.oResult;
                                                var sXILinkID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "XILinkID".ToLower()).Select(m => m.sValue).FirstOrDefault();
                                                int.TryParse(sXILinkID, out iXILinkID);
                                                iRowXilinkID = iXILinkID;
                                                if (iXILinkID > 0)
                                                {
                                                    XiLinkNV oNV = new XiLinkNV();
                                                    oNV.XiLinkID = iXILinkID;
                                                    oNV.Name = "StartAction";
                                                    oNV.Value = "Dialog";
                                                    Save_XILinkNV(oNV);
                                                    oNV = new XiLinkNV();
                                                    oNV.XiLinkID = iXILinkID;
                                                    oNV.Name = "DialogID";
                                                    oNV.Value = iDialogID.ToString();
                                                    Save_XILinkNV(oNV);
                                                }
                                                //Create BOUIDefault
                                                XIDBODefault oDefault = new XIDBODefault();
                                                oDefault.FKiBOID = BOID;
                                                oDefault.iPopupID = iRowXilinkID;
                                                oDefault.iType = 30;
                                                var oBODefault = Save_BOUIDefault(oDefault);
                                                if (oBODefault.bOK && oBODefault.oResult != null)
                                                {

                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting BOStructure Details" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }

            return oCResult;
        }

        #region BoDashBoardCharts
        /*:::::::::::::::::::::::::::::::::Creating BoDashBoards for BO :::::::::::::::::::::::::::::
         * :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::*/

        public CResult Generate_KPI(string sSystemType, string sAttr)
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
                XICore.XIBoDefaultDashboard ListCount = new XICore.XIBoDefaultDashboard();
                string i1ClickID = string.Empty;
                var sRowXilinkID = string.Empty;
                if (!string.IsNullOrEmpty(sSystemType) && sSystemType != "0")
                {
                    /* get DashBoard count in XIBODashBoardCharts_T table */
                    oCR = DashboardCount(iBODID, "");
                    if (oCR.bOK && oCR.oResult != null)
                    {
                        ListCount = (XICore.XIBoDefaultDashboard)oCR.oResult;
                        if (ListCount != null)
                        {
                            string DBName = System.Configuration.ConfigurationManager.AppSettings["CoreApplicatoinDatabase"];
                            oCR = DefaultBoChartsXilink(sSystemType, sAttr, ListCount.iRowXilinkID);
                            if (oCR.bOK && oCR.oResult != null)
                            {
                                sRowXilinkID = (string)oCR.oResult;
                                ListCount.iRowXilinkID = Convert.ToInt32(sRowXilinkID);
                                oCR = CreateXISystemDefaultBODashboards1Click(iBODID, sAttr, sSystemType, oCResult.sCode, DBName);
                                if (oCR.bOK && oCR.oResult != null)
                                {
                                    i1ClickID = (string)oCR.oResult;
                                    ListCount.FKiOneClickID = Convert.ToInt32(i1ClickID);
                                    if (ListCount.sChartType == "AM-Charts")
                                    {
                                        if (sSystemType == "100" || sSystemType == "80")
                                        {
                                            ListCount.FKiComponentTypeID = XIConstant.DefaultAM4BarChartComponentID;
                                        }
                                        else if (sSystemType == "10")
                                        {

                                            ListCount.FKiComponentTypeID = XIConstant.DefaultAM4PieChartComponentID;
                                        }
                                    }
                                    else
                                    {
                                        if (sSystemType == "100" || sSystemType == "80")
                                        {
                                            ListCount.FKiComponentTypeID = XIConstant.DefaultBarChartComponentID;
                                        }
                                        else if (sSystemType == "10")
                                        {

                                            ListCount.FKiComponentTypeID = XIConstant.DefaultPieChartComponentID;
                                        }
                                    }
                                }
                                else
                                {
                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                }
                            }
                            else
                            {
                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                            }
                        }
                    }
                    else
                    {
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                    }
                }
                if (oTrace.iStatus != (int)xiEnumSystem.xiFuncResult.xiError)
                {
                    oCR = AddingChart2Layout(ListCount);
                }

            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        public CResult Save_BODashBoards(List<CNV> oParams, string sDashBoardType)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "";//expalin about this method logic

            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

            oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiInProcess;

            if (iTraceLevel > 0)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Started Execution" });
            }

            if (oCR.xiStatus == xiEnumSystem.xiFuncResult.xiError)
            {
                oCResult.xiStatus = oCR.xiStatus;
            }
            oCResult.sMessage = "someone tried to do something they shouldnt";

            try
            {
                //oTrace.oParams.Add(new CNV { sName = "", sValue = "" });
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "Reference Data insertion started" });
                }
                int iOrgID = 0;
                var OrgID= oParams.Where(m => m.sName.ToLower() == XIConstant.Param_OrgID).Select(m => m.sValue).FirstOrDefault();
                int.TryParse(OrgID, out iOrgID);
                var sOrgName = string.Empty;
                if (iOrgID > 0)
                {

                    XIIXI oXI = new XIIXI();
                    var oOrgI = oXI.BOI("Organisations", iOrgID.ToString());
                    if (oOrgI != null && oOrgI.Attributes.Count() > 0)
                    {
                        sOrgName = oOrgI.AttributeI("Name").sValue;
                    }
                }
                var sDialogName = oParams.Where(m => m.sName.ToLower() == "dialogname").Select(m => m.sValue).FirstOrDefault();
                var sLayoutName = oParams.Where(m => m.sName.ToLower() == "layoutname").Select(m => m.sValue).FirstOrDefault();
                var sXilinkName = oParams.Where(m => m.sName.ToLower() == "xilinkname").Select(m => m.sValue).FirstOrDefault();
                var sApplicationName = oParams.Where(m => m.sName.ToLower() == "sapplicationname").Select(m => m.sValue).FirstOrDefault();
                var sParentID = oParams.Where(m => m.sName.ToLower() == "sparentid").Select(m => m.sValue).FirstOrDefault();
                var sCreateParentID = oParams.Where(m => m.sName.ToLower() == "screateparentid").Select(m => m.sValue).FirstOrDefault();
                var IsRowClick = oParams.Where(m => m.sName.ToLower() == "irowxilinkid").Select(m => m.sValue).FirstOrDefault();
                var sAppID = oParams.Where(m => m.sName.ToLower() == XIConstant.Param_ApplicationID.ToLower()).Select(m => m.sValue).FirstOrDefault();
                int iAppID = 0;
                int.TryParse(sAppID, out iAppID);
                int iXiLinkIDForBO = 0;
                var oAppD = (XIDApplication)oCache.GetObjectFromCache(XIConstant.CacheApplication, null, iAppID.ToString());
                sApplicationName = oAppD.sApplicationName;
                //Create Layout
                XIIBO oBOI = new XIIBO();
                int iLayoutID = 0;
                int iDialogID = 0;
                XIDLayout oLayout = new XIDLayout();
                oLayout.FKiApplicationID = iAppID;
                oLayout.LayoutName = sLayoutName;
                oLayout.LayoutCode = XIConstant.Auto1LayoutHTML;
                oLayout.LayoutType = XIConstant.DefaultDialog; //"Dialog";
                oLayout.LayoutLevel = XIConstant.OrganisationLevel;//"OrganisationLevel";
                oLayout.Authentication = XIConstant.DefaultAuthenticated; //"Authenticated";
                oLayout.StatusTypeID = XIConstant.DefaultStatusTypeID; //10;
                oCR = Save_Layout(oLayout);
                oTrace.oTrace.Add(oCR.oTrace);
                if (oCR.bOK && oCR.oResult != null)
                {
                    oBOI = (XIIBO)oCR.oResult;
                    var sLayoutID = "";
                    var sPrimaryKey = oBOI.BOD.sPrimaryKey;
                    if (sPrimaryKey == null)
                    {
                        sLayoutID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "id").Select(m => m.sValue).FirstOrDefault();
                    }
                    else
                    {
                        sLayoutID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == sPrimaryKey.ToLower()).Select(m => m.sValue).FirstOrDefault();
                    }

                    int.TryParse(sLayoutID, out iLayoutID);
                    if (iLayoutID > 0)
                    {
                        //Create Layout Details
                        int iPlaceHolderID = 0;
                        XIDLayoutDetails oDetail = new XIDLayoutDetails();
                        oDetail.PlaceHolderID = 0;
                        oDetail.FKiApplicationID = iAppID;
                        oDetail.LayoutID = iLayoutID;
                        oDetail.PlaceholderName = sBOName;
                        oDetail.PlaceholderArea = XIConstant.PlaceHolderArea;
                        oDetail.PlaceholderUniqueName = "BO" + sBOName.Replace(" ", "");
                        oDetail.PlaceholderClass = XIConstant.PlaceHolderClass;
                        oCR = Save_LayoutDetail(oDetail);
                        oTrace.oTrace.Add(oCR.oTrace);
                        if (oCR.bOK && oCR.oResult != null)
                        {
                            oBOI = (XIIBO)oCR.oResult;
                            var sPlaceHolderID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == oBOI.BOD.sPrimaryKey.ToLower()).Select(m => m.sValue).FirstOrDefault();
                            int.TryParse(sPlaceHolderID, out iPlaceHolderID);
                            if (iPlaceHolderID > 0)
                            {
                                //Create Dialog
                                XIDDialog oDialog = new XIDDialog();
                                oDialog.FKiApplicationID = iAppID;
                                oDialog.DialogName = sDialogName;
                                oDialog.PopupSize = "Default";
                                oDialog.LayoutID = iLayoutID;
                                oDialog.StatusTypeID = XIConstant.DefaultStatusTypeID; //10;
                                oCR = Save_Dialog(oDialog);
                                oTrace.oTrace.Add(oCR.oTrace);
                                if (oCR.bOK && oCR.oResult != null)
                                {
                                    oBOI = (XIIBO)oCR.oResult;
                                    var sDialogID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == oBOI.BOD.sPrimaryKey.ToLower()).Select(m => m.sValue).FirstOrDefault();
                                    int.TryParse(sDialogID, out iDialogID);
                                    if (iDialogID > 0)
                                    {
                                        //Create Layout Mappings
                                        XIDLayoutMapping oMap = new XIDLayoutMapping();
                                        oMap.PopupID = iDialogID;
                                        oMap.PopupLayoutID = iLayoutID;
                                        oMap.PlaceHolderID = iPlaceHolderID;
                                        oMap.ContentType = XIConstant.DefaultXiComponent; //"XIComponent";
                                        oMap.Type = XIConstant.DefaultDialog;//"Dialog"
                                        oMap.StatusTypeID = XIConstant.DefaultStatusTypeID; //10;
                                        oMap.FKiApplicationID = iAppID;
                                        oCR = Save_LayoutMapping(oMap);
                                        oTrace.oTrace.Add(oCR.oTrace);
                                        if (oCR.bOK && oCR.oResult != null)
                                        {
                                            //Create XILink
                                            int iXILinkID = 0;
                                            XILink oXILink = new XILink();
                                            oXILink.Name = sXilinkName;
                                            oXILink.URL = XIConstant.DefaultURL; //"XILink";
                                            oXILink.sActive = XIConstant.DefaultActiveXilink; //"Y"
                                            oXILink.FKiApplicationID = iAppID;
                                            oXILink.sType = XIConstant.DefaultXiLinkType; //"Content"
                                            oXILink.StatusTypeID = XIConstant.DefaultStatusTypeID; //10
                                            oCR = Save_XILink(oXILink);
                                            oTrace.oTrace.Add(oCR.oTrace);
                                            if (oCR.bOK && oCR.oResult != null)
                                            {
                                                oBOI = (XIIBO)oCR.oResult;
                                                var sXILinkID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == oBOI.BOD.sPrimaryKey.ToLower()).Select(m => m.sValue).FirstOrDefault();
                                                int.TryParse(sXILinkID, out iXILinkID);
                                                iXiLinkIDForBO = iXILinkID;
                                                if (iXILinkID > 0)
                                                {
                                                    XiLinkNV oNV = new XiLinkNV();
                                                    oNV.XiLinkID = iXILinkID;
                                                    oNV.Name = XIConstant.DefaultStartAction; //"StartAction"
                                                    oNV.Value = XIConstant.DefaultDialog; //"Dialog"
                                                    Save_XILinkNV(oNV);
                                                    oTrace.oTrace.Add(oCR.oTrace);
                                                    if (oCR.bOK && oCR.oResult != null)
                                                    {
                                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                                                        oCResult.oResult = "Success";
                                                    }
                                                    else
                                                    {
                                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                    }
                                                    oNV = new XiLinkNV();
                                                    oNV.XiLinkID = iXILinkID;
                                                    oNV.Name = XIConstant.DefaultDialogID; //"DialogID";
                                                    oNV.Value = iDialogID.ToString();
                                                    Save_XILinkNV(oNV);
                                                    oTrace.oTrace.Add(oCR.oTrace);
                                                    if (oCR.bOK && oCR.oResult != null)
                                                    {
                                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                                                        oCResult.oResult = "Success";
                                                    }
                                                    else
                                                    {
                                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                    }
                                                }
                                                XIBoDefaultDashboard defaultDashBoard = new XIBoDefaultDashboard();
                                                defaultDashBoard.FKiBOID = iBODID;
                                                // defaultDashBoard.FKiPlaceholderID = iPlaceHolderID;
                                                defaultDashBoard.iRowXilinkID = Convert.ToInt32(IsRowClick);
                                                defaultDashBoard.FKiLayoutID = iLayoutID;
                                                defaultDashBoard.FKiDialogID = iDialogID;
                                                defaultDashBoard.sContentType = XIConstant.DefaultXiComponent;
                                                defaultDashBoard.sChartType = sDashBoardType;
                                                var SavingDashBoardsParams = Save_BODashBoardCharts(defaultDashBoard);
                                                oTrace.oTrace.Add(oCR.oTrace);
                                                if (oCR.bOK && oCR.oResult != null)
                                                {
                                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                                                    oCResult.oResult = "Success";
                                                }
                                                else
                                                {
                                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                }
                                            }
                                            else
                                            {
                                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                            }
                                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                                        }
                                        else
                                        {
                                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                        }
                                    }

                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                                }
                                else
                                {
                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                }
                            }
                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                        }
                        else
                        {
                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                        }
                        if (string.IsNullOrEmpty(sParentID))
                        {
                            XIIXI oXI = new XIIXI();
                            List<CNV> oWhereParams = new List<CNV>();
                            oWhereParams.Add(new CNV { sName = "rootname", sValue = oAppD.sApplicationName.Replace(" ", "") });
                            oWhereParams.Add(new CNV { sName = "parentid", sValue = "#" });
                            oWhereParams.Add(new CNV { sName = "fkiapplicationid", sValue = iAppID.ToString() });
                            var oMenuD = oXI.BOI("XI Menu", null, null, oWhereParams);
                            sParentID = oMenuD.Attributes["ID"].sValue;
                        }
                        //Create Menu
                        XIMenu oMenu = new XIMenu();
                        oMenu.Name = sBOName + " BO Default DashBoards";
                        oMenu.XiLinkID = iXiLinkIDForBO;
                        oMenu.ParentID = sParentID;
                        oMenu.FKiApplicationID = iAppID;
                        oMenu.RootName = sApplicationName.Replace(" ", "");
                        oMenu.RoleID = 6;
                        oMenu.iOrgID = 5;
                        oMenu.ActionType = 20;
                        oMenu.StatusTypeID = 10;
                        Save_Menu(oMenu);
                        oTrace.oTrace.Add(oCR.oTrace);
                        if (oCR.bOK && oCR.oResult != null)
                        {
                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                            oCResult.oResult = "Success";
                        }
                        else
                        {
                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                        }

                        //XI Create Menu
                        XIAssignMenu oAS = new XIAssignMenu();
                        oAS.Name = sBOName + " BO Default DashBoards";
                        oAS.ParentID = sCreateParentID;
                        oAS.FKiApplicationID = iAppID;
                        oAS.RootName = sApplicationName;
                        oAS.ActionType = 20;
                        oAS.iOrgID = 5;
                        oAS.RoleID = 6;
                        oAS.sType = "AssignMenu";
                        var oAssignMenu = Save_AssignMenu(oAS);
                        var iCreatedMenuID = 0;
                        if (oAssignMenu.bOK && oAssignMenu.oResult != null)
                        {
                            var oMenuDef = (XIIBO)oAssignMenu.oResult;
                            var sMenuID = oMenuDef.Attributes.Where(m => m.Key.ToLower() == "id").Select(m => m.Value).Select(m => m.sValue).FirstOrDefault();
                            int.TryParse(sMenuID, out iCreatedMenuID);
                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                        }
                        else
                        {
                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                        }
                        //Menu Mapping
                        XIAssignMenu oAM = new XIAssignMenu();
                        oAM.Name = sBOName + " BO Default DashBoards";
                        oAM.XiLinkID = iXiLinkIDForBO;
                        oAM.FKiApplicationID = iAppID;
                        oAM.iOrgID = 5;
                        oAM.ActionType = 20;
                        oAM.sType = "CreateMenu";
                        oAM.RoleID = 6;
                        var oCreaMenu = Save_AssignMenu(oAM);
                        if (oCreaMenu.bOK && oCreaMenu.oResult != null)
                        {
                            var oMenuDef = (XIIBO)oCreaMenu.oResult;
                            var sMenuID = oMenuDef.Attributes.Where(m => m.Key.ToLower() == oMenuDef.BOD.sPrimaryKey.ToLower()).Select(m => m.Value).Select(m => m.sValue).FirstOrDefault();

                            //Update FKiMenuID for Menu in XIMenuMappings_T Table
                            XIIBO oBOIS = new XIIBO();
                            XIDBO oBOD = new XIDBO();
                            oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "AssignMenu");
                            oBOIS.BOD = oBOD;
                            oBOIS.SetAttribute("ID", iCreatedMenuID.ToString());
                            oBOIS.SetAttribute("FKiMenuID", sMenuID);
                            oBOIS.SaveV2(oBOIS);
                            oTrace.oTrace.Add(oCR.oTrace);
                            if (oCreaMenu.bOK && oCreaMenu.oResult != null)
                            {
                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                                oCResult.oResult = "Success";
                            }
                            else
                            {
                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                            }
                        }
                        else
                        {
                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                        }
                    }
                }
                else
                {
                    oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Reference Data insertion failed" });
                }
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                oCResult.oResult = oBOI;
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Reference Data insertion failed" });
                oSigCR.oStack.Add(new CNV { sName = "Error Message", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting Reference Data" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }
        public CResult Save_BoDefaultLayout(XIDLayout oLayout)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "Default Layout insertion started" });
                }
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Default Layout Insertoin Started" });
                oCR = Save_Layout(oLayout);
                if (oCR.bOK && oCR.oResult != null)
                {
                    int iLayoutID = 0;
                    XIIBO oLayI = new XIIBO();
                    oLayI = (XIIBO)oCR.oResult;
                    var sLayoutID = oLayI.Attributes.Where(m => m.Key.ToLower() == oLayI.BOD.sPrimaryKey.ToLower()).Select(s => s.Value).Select(m => m.sValue).FirstOrDefault();
                    int.TryParse(sLayoutID, out iLayoutID);
                    if (iLayoutID > 0)
                    {
                        for (int i = 0; i <= 5; i++)
                        {
                            XIIBO oBOI = new XIIBO();
                            if (oSigCR != null)
                            {
                                oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "Default Layout insertion completed" });
                            }
                            oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Default Layout inserted successfully" });
                            if (oSigCR != null)
                            {
                                oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "Default Layout details insertion started" });
                            }
                            oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Default Layout details insertion started" });
                            XIDLayoutDetails oLayDetail = new XIDLayoutDetails();
                            oLayDetail.LayoutID = iLayoutID;
                            oLayDetail.PlaceholderName = "BOChart" + i;
                            oLayDetail.PlaceholderArea = "div" + i;
                            oLayDetail.PlaceholderUniqueName = "BOChart" + i;
                            oLayDetail.PlaceholderClass = "col-md-12";
                            oCR = Save_LayoutDetail(oLayDetail);
                            var sPlaceHolderID = string.Empty;
                            if (oCR.bOK && oCR.oResult != null)
                            {

                            }
                        }
                        if (oSigCR != null)
                        {
                            oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "Layout mappings insertion completed" });
                        }
                        oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Layout mappings inserted successfully" });
                    }
                    oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                    oCResult.oResult = oLayI;
                    //AutoMenuCreationForApplication(sAppName);//Creating the Menu For Application With AppllicationName
                }
                else
                {
                    oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Layout insertion failed" });
                    oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Layout insertion failed" });
                }
            }
            catch (Exception ex)
            {
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Layout insertion failed" });
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting Layout" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            return oCResult;
        }
        //Creating Xilink for DashBoard BO's Chart
        public CResult DefaultBoChartsXilink(string sType = "", string SearchType = null, int iRowClickID = 0)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Default XILink Insertion in DefaultBoChartsXilink method";//expalin about this method logic
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            string DefaultXilinkID = "";
            try
            {
                XIIBO oBOI = new XIIBO();
                int iLayoutID = 0;
                XIDLayout oLayout = new XIDLayout();
                oLayout.FKiApplicationID = iAppID;
                oLayout.LayoutName = sBOName + " Default" + sType + " Layout";
                oLayout.LayoutCode = XIConstant.DefaultLayout;
                oLayout.LayoutType = XIConstant.DefaultDialog; //"Dialog";
                oLayout.LayoutLevel = XIConstant.OrganisationLevel;//"OrganisationLevel";
                                                                   //oLayout.RootName = "Zeeinsurance";
                oLayout.Authentication = XIConstant.DefaultAuthenticated; //"Authenticated";
                oLayout.StatusTypeID = XIConstant.DefaultStatusTypeID; //10;
                oCR = Save_Layout(oLayout);
                oTrace.oTrace.Add(oCR.oTrace);
                if (oCR.bOK && oCR.oResult != null)
                {
                    oBOI = (XIIBO)oCR.oResult;
                    var sLayoutID = "";
                    var sPrimaryKey = oBOI.BOD.sPrimaryKey;
                    if (sPrimaryKey == null)
                    {
                        sLayoutID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == "id").Select(m => m.sValue).FirstOrDefault();
                    }
                    else
                    {
                        sLayoutID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == sPrimaryKey.ToLower()).Select(m => m.sValue).FirstOrDefault();
                    }
                    int.TryParse(sLayoutID, out iLayoutID);
                    if (iLayoutID > 0)
                    {
                        //Create Layout Details
                        int iPlaceHolderID = 0;
                        XIDLayoutDetails oDetail = new XIDLayoutDetails();
                        oDetail.PlaceHolderID = 0;
                        oDetail.FKiApplicationID = iAppID;
                        oDetail.LayoutID = iLayoutID;
                        oDetail.PlaceholderName = sBOName + " Place Holder";
                        oDetail.PlaceholderArea = XIConstant.PlaceHolderArea;
                        oDetail.PlaceholderUniqueName = sBOName + "PlaceHolder".Replace(" ", "");
                        oDetail.PlaceholderClass = XIConstant.PlaceHolderClass;
                        oCR = Save_LayoutDetail(oDetail);
                        oTrace.oTrace.Add(oCR.oTrace);
                        if (oCR.bOK && oCR.oResult != null)
                        {
                            oBOI = (XIIBO)oCR.oResult;
                            var sPlaceHolderID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == oBOI.BOD.sPrimaryKey.ToLower()).Select(m => m.sValue).FirstOrDefault();
                            int.TryParse(sPlaceHolderID, out iPlaceHolderID);
                            if (iPlaceHolderID > 0)
                            {
                                //Create Dialog
                                int iDialogID = 0;
                                XIDDialog oDialog = new XIDDialog();
                                oDialog.FKiApplicationID = iAppID;
                                oDialog.DialogName = sBOName + " Default" + sType;
                                oDialog.PopupSize = "Default";
                                oDialog.LayoutID = iLayoutID;
                                oDialog.StatusTypeID = XIConstant.DefaultStatusTypeID; //10;
                                oCR = Save_Dialog(oDialog);
                                oTrace.oTrace.Add(oCR.oTrace);
                                if (oCR.bOK && oCR.oResult != null)
                                {
                                    oBOI = (XIIBO)oCR.oResult;
                                    var sDialogID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == oBOI.BOD.sPrimaryKey.ToLower()).Select(m => m.sValue).FirstOrDefault();
                                    int.TryParse(sDialogID, out iDialogID);
                                    if (iDialogID > 0)
                                    {
                                        //Create Layout Mappings
                                        XIDLayoutMapping oMap = new XIDLayoutMapping();
                                        oMap.PopupID = iDialogID;
                                        oMap.PopupLayoutID = iLayoutID;
                                        oMap.PlaceHolderID = iPlaceHolderID;
                                        oMap.XiLinkID = XIConstant.DefaultOneClickComponentID; //3 //oneclick;
                                        oMap.ContentType = XIConstant.DefaultXiComponent; //"XIComponent";
                                        oMap.Type = XIConstant.DefaultDialog;//"Dialog"
                                        oMap.StatusTypeID = XIConstant.DefaultStatusTypeID; //10;
                                        oMap.FKiApplicationID = iAppID;
                                        oCR = Save_LayoutMapping(oMap);
                                        oTrace.oTrace.Add(oCR.oTrace);
                                        if (oCR.bOK && oCR.oResult != null)
                                        {
                                            XILink DefaultXilink = new XILink();
                                            int iXILinkID = 0;
                                            XILink oXILink = new XILink();
                                            oXILink.Name = sBOName + " Default Bo " + sType + " Xilink";
                                            oXILink.URL = "XILink";
                                            oXILink.sActive = "Y";
                                            oXILink.StatusTypeID = 10;
                                            oCR = Save_XILink(oXILink);
                                            oTrace.oTrace.Add(oCR.oTrace);
                                            if (oCR.bOK && oCR.oResult != null)
                                            {
                                                XIIBO oBOI1 = (XIIBO)oCR.oResult;
                                                var sXILinkID = oBOI1.Attributes.Values.Where(m => m.sName.ToLower() == oBOI1.BOD.sPrimaryKey.ToLower()).Select(m => m.sValue).FirstOrDefault();
                                                DefaultXilinkID = sXILinkID;
                                                int.TryParse(sXILinkID, out iXILinkID);
                                                if (iXILinkID > 0)
                                                {
                                                    XiLinkNV oNV = new XiLinkNV();
                                                    oNV.XiLinkID = iXILinkID;
                                                    oNV.Name = "StartAction";
                                                    oNV.Value = "Dialog";
                                                    Save_XILinkNV(oNV);
                                                    if (oCR.bOK && oCR.oResult != null)
                                                    {
                                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                                                    }
                                                    else
                                                    {
                                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                    }
                                                    oNV = new XiLinkNV();
                                                    oNV.XiLinkID = iXILinkID;
                                                    oNV.Name = "DialogID";
                                                    oNV.Value = iDialogID.ToString();
                                                    var cvf = Save_XILinkNV(oNV);
                                                    if (oCR.bOK && oCR.oResult != null)
                                                    {
                                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                                                    }
                                                    else
                                                    {
                                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                    }
                                                }
                                                oCResult.sCode = sXILinkID;
                                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                                            }
                                            else
                                            {
                                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                            }
                                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                                        }
                                        else
                                        {
                                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                        }
                                        List<CNV> nParams = new List<CNV>();
                                        //Create OneClick for BO List
                                        string CoreDatabase = System.Configuration.ConfigurationManager.AppSettings["CoreApplicatoinDatabase"];
                                        oCResult = DefaultBOChartsResultOneClick(iBODID, sType, SearchType, iRowClickID, CoreDatabase);
                                        nParams.Add(new CNV { sName = "1ClickID", sValue = oCResult.sCode });
                                        foreach (var Param in nParams)
                                        {
                                            XIDComponentParam oParam = new XIDComponentParam();
                                            oParam.sName = Param.sName;
                                            oParam.sValue = Param.sValue;
                                            oParam.FKiComponentID = XIConstant.DefaultOneClickComponentID;
                                            oParam.iLayoutMappingID = iPlaceHolderID;
                                            oCR = Save_ComponentParam(oParam);
                                            if (oCR.bOK && oCR.oResult != null)
                                            {
                                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                                            }
                                            else
                                            {
                                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                            }
                                        }
                                    }
                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                                }
                                else
                                {
                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                }
                            }
                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                        }
                        else
                        {
                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                        }
                    }
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                }
                if (oTrace.iStatus == (int)xiEnumSystem.xiFuncResult.xiSuccess)
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                    oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                    oCResult.oResult = DefaultXilinkID;
                }
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Layout insertion failed" });
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting Layout" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }


        public CResult Save_BODashBoardCharts(XIBoDefaultDashboard oDashoard)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always  
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "Dialog insertion started" });
                }
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                //oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIBODashBoardCharts");
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "DefaultDashboard");
                oBOI.BOD = oBOD;
                oBOI.SetAttribute("ID", oDashoard.ID.ToString());
                oBOI.SetAttribute("FKiBOID", oDashoard.FKiBOID.ToString());
                oBOI.SetAttribute("FKiOneClickID", oDashoard.FKiOneClickID.ToString());
                oBOI.SetAttribute("bFlag", oDashoard.bFlag.ToString());
                oBOI.SetAttribute("FKiPlaceholderID", oDashoard.FKiPlaceholderID.ToString());
                oBOI.SetAttribute("iRowXilinkID", oDashoard.iRowXilinkID.ToString());
                oBOI.SetAttribute("FKiLayoutID", oDashoard.FKiLayoutID.ToString());
                oBOI.SetAttribute("sAttrName", oDashoard.sAttrName);
                oBOI.SetAttribute("FKiDialogID", oDashoard.FKiDialogID.ToString());
                oBOI.SetAttribute("sType", oDashoard.sType);
                oBOI.SetAttribute("FKiComponentTypeID", oDashoard.FKiComponentTypeID.ToString());
                oBOI.SetAttribute("sContentType", oDashoard.sContentType);
                oBOI.SetAttribute("sChartType", oDashoard.sChartType);
                oCR = oBOI.SaveV2(oBOI);
                oTrace.oTrace.Add(oCR.oTrace);
                if (oCR.bOK && oCR.oResult != null)
                {
                    oBOI = (XIIBO)oCR.oResult;
                    if (oSigCR != null)
                    {
                        oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "Dialog insertion completed" });
                    }
                    oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                    oCResult.oResult = oBOI;
                }
                else
                {
                    oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Dialog insertion failed" });
                }
            }
            catch (Exception ex)
            {
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Dialog insertion failed" });
                oSigCR.oStack.Add(new CNV { sName = "Error Messaged", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting Dialog" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }


        /*:::::::::::::::::::Default BO's Charts Result OneClick :::::::::::::::::::::::::
         :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::*/
        public CResult DefaultBOChartsResultOneClick(int BOID, string sType, string SearchType, int iRowClickID, string DBName)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Adding new layout for default dashboard";//expalin about this method logic
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            int iResult1ClickID = 0;
            try
            {
                XIDBO oBODef = new XIDBO();
                oBODef = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, null, BOID.ToString());
                if (oBODef != null)
                {
                    XID1Click o1Click = new XID1Click();
                    o1Click.FromBos = oBODef.TableName;
                    var sBoTableName = oBODef.TableName;
                    sBOName = oBODef.Name;
                    /* :::::Create Pie Chart Result Query::::*/
                    if (sType == "10")
                    {
                        o1Click.Name = o1Click.Code = o1Click.Description = oBODef.Name + " List count by status";
                        o1Click.Query = "select {List} from " + sBoTableName + " inner join " + DBName + ".dbo.XIBOOptionList_T_N  ON " + sBoTableName + "." + SearchType + " = " + DBName + ".[dbo].[XIBOOptionList_T_N].sValues where " + DBName + ".[dbo].[XIBOOptionList_T_N].Name = '" + SearchType + "' and BOID=" + iBODID + " and " + DBName + ".[dbo].[XIBOOptionList_T_N].sOptionName={XIP|sStatusName}";
                    }
                    /* :::::Create Bar Chart Current Year Result Query::::*/
                    if (sType == "100")
                    {
                        o1Click.Name = o1Click.Code = o1Click.Description = oBODef.Name + " current Year list by status";
                        //o1Click.Query = "select {List} from " + sBoTableName + " Where year(" + SearchType + ") = {XIP|sStatusName} ";
                        o1Click.Query = "select {List} from " + sBoTableName + " inner join " + DBName + ".dbo.XIBOOptionList_T_N  ON " + sBoTableName + ".iStatus = " + DBName + ".[dbo].[XIBOOptionList_T_N].sValues where " + DBName + ".[dbo].[XIBOOptionList_T_N].Name = 'iStatus' and BOID=" + iBODID + " and " + DBName + ".[dbo].[XIBOOptionList_T_N].sOptionName={XIP|sStatusName} and YEAR(" + sBoTableName + "." + SearchType + ")=YEAR(GETDATE())";
                    }
                    /* :::::Create Bar Chart Current Month Result Query::::*/
                    if (sType == "80")
                    {
                        o1Click.Name = o1Click.Code = o1Click.Description = oBODef.Name + " current Month list count by status";
                        //o1Click.Query = "select {List} from " + sBoTableName + " inner join xidnaqa_old1.dbo.XIBOOptionList_T_N  ON " + sBoTableName + ".iStatus = xidnaqa_old1.[dbo].[XIBOOptionList_T_N].sValues where xidnaqa_old1.[dbo].[XIBOOptionList_T_N].Name = 'iStatus' and BOID=" + iBOID + " and xidnaqa_old1.[dbo].[XIBOOptionList_T_N].sOptionName={XIP|sStatusName} ";
                        //o1Click.Query = "select {List} from " + sBoTableName + " where month(" + SearchType + ")=month(CURRENT_TIMESTAMP) and  DATENAME(month, " + SearchType + ")={XIP|sStatusName} ";
                        o1Click.Query = "select {List} from " + sBoTableName + " inner join " + DBName + ".dbo.XIBOOptionList_T_N  ON " + sBoTableName + ".iStatus = " + DBName + ".[dbo].[XIBOOptionList_T_N].sValues where " + DBName + ".[dbo].[XIBOOptionList_T_N].Name = 'iStatus' and BOID=" + iBODID + " and " + DBName + ".[dbo].[XIBOOptionList_T_N].sOptionName={XIP|sStatusName} and MONTH(" + sBoTableName + "." + SearchType + ")=MONTH(GETDATE())";
                    }
                    o1Click.IsRowClick = true;
                    o1Click.RowXiLinkID = iRowClickID;
                    iBODID = o1Click.BOID = oBODef.BOID;
                    o1Click.DisplayAs = 50; o1Click.IsFilterSearch = false;
                    o1Click.IsCreate = false;
                    o1Click.IsRefresh = false;
                    o1Click.CategoryID = 1;
                    o1Click.sLastUpdate = "XISystem";
                    o1Click.FKiAppID = iAppID;
                    var o1ClickDef = Save_1Click(o1Click);
                    var Query = o1ClickDef.sQuery;
                    if (o1ClickDef.bOK && o1ClickDef.oResult != null)
                    {
                        iResult1ClickID = (int)o1ClickDef.oResult;
                        oCResult.sCode = Convert.ToString(iResult1ClickID);
                        return oCResult;
                    }
                }
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error While Inserting Application" });
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Application insertion failed" });
                oSigCR.oStack.Add(new CNV { sName = "Error Message", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }


        /* :::::Boid DashBoard Count::::*/
        public CResult DashboardCount(int BOID, string sAttribute)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "DashBoardCount in DashboardCount method";//expalin about this method logic
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";
            XIBoDefaultDashboard DefDashBoard = new XIBoDefaultDashboard();

            try
            {
                //CResult vcf = new CResult();
                Dictionary<string, object> DefParams = new Dictionary<string, object>();
                DefParams["FKiBOID"] = BOID;
                DefDashBoard = Connection.Select<XIBoDefaultDashboard>("XIBODashBoardCharts_T", DefParams).FirstOrDefault();
                oCResult.oResult = DefDashBoard;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error While Inserting Application" });
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Application insertion failed" });
                oSigCR.oStack.Add(new CNV { sName = "Error Message", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;

            return oCResult;
        }

        /* :::::Adding Layout ::::*/
        public CResult AddingChart2Layout(XIBoDefaultDashboard Params)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Adding new layout for default dashboard";//expalin about this method logic
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            try
            {
                XIBoDefaultDashboard test = new XIBoDefaultDashboard();
                Dictionary<string, object> LayParams = new Dictionary<string, object>();
                LayParams["ID"] = Params.FKiLayoutID;
                var oLayout = Connection.Select<XIDLayout>("XILayout_T", LayParams).FirstOrDefault();
                var AddingLayoutCode = oLayout.LayoutCode;
                // Remove last character </div> from a string  
                string AddingDivTag = AddingLayoutCode.Remove(AddingLayoutCode.Length - 6, 6);
                oLayout.LayoutCode = AddingDivTag + " <div class=\"col-md-6 col-sm-12\" id=\"" + Params.FKiOneClickID + "\"></div></div>";
                var UpdateLayout = Save_Layout(oLayout);
                if (Params.FKiLayoutID > 0)
                {
                    //Create Layout Details
                    int iPlaceHolderID = 0;
                    XIDLayoutDetails oDetail = new XIDLayoutDetails();
                    oDetail.PlaceHolderID = 0;
                    oDetail.FKiApplicationID = iAppID;
                    oDetail.LayoutID = Params.FKiLayoutID;
                    oDetail.PlaceholderName = sBOName + Params.FKiOneClickID;
                    oDetail.PlaceholderArea = "div" + Params.FKiOneClickID;
                    oDetail.PlaceholderUniqueName = "BO" + Params.FKiOneClickID + sBOName.Replace(" ", "");
                    oDetail.PlaceholderClass = XIConstant.PlaceHolderClass;
                    oCR = Save_LayoutDetail(oDetail);
                    oTrace.oTrace.Add(oCR.oTrace);
                    if (oCR.bOK && oCR.oResult != null)
                    {
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                        oCResult.oResult = "Success";
                        var oBOI = (XIIBO)oCR.oResult;
                        var sPlaceHolderID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == oBOI.BOD.sPrimaryKey.ToLower()).Select(m => m.sValue).FirstOrDefault();
                        //var xcd= oBOI.Attributes.Values.Where(m => m.sName.ToLower() == oBOI.BOD.sPrimaryKey.ToLower()).Select(m => m.sValue).Take(5);
                        int.TryParse(sPlaceHolderID, out iPlaceHolderID);
                        if (iPlaceHolderID > 0)
                        {
                            if (Params.FKiDialogID > 0)
                            {
                                //Create Layout Mappings
                                XIDLayoutMapping oMap = new XIDLayoutMapping();
                                oMap.PopupID = Params.FKiDialogID;
                                oMap.PopupLayoutID = Params.FKiLayoutID;
                                oMap.PlaceHolderID = iPlaceHolderID;
                                oMap.XiLinkID = Params.FKiComponentTypeID;
                                oMap.ContentType = Params.sContentType; //"XIComponent";
                                oMap.Type = XIConstant.DefaultDialog;//"Dialog"
                                oMap.StatusTypeID = XIConstant.DefaultStatusTypeID; //10;
                                oMap.FKiApplicationID = iAppID;
                                oCR = Save_LayoutMapping(oMap);
                                oTrace.oTrace.Add(oCR.oTrace);
                                if (oCR.bOK && oCR.oResult != null)
                                {
                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                                    oCResult.oResult = "Success";
                                    //Create XILink
                                    int iXILinkID = 0;
                                    XILink oXILink = new XILink();
                                    oXILink.Name = "Default Xilink " + Params.sType;
                                    oXILink.URL = XIConstant.DefaultURL; //"XILink";
                                    oXILink.sActive = XIConstant.DefaultActiveXilink; //"Y"
                                    oXILink.FKiApplicationID = iAppID;
                                    oXILink.FKiComponentID = Params.FKiComponentTypeID;
                                    oXILink.sType = XIConstant.DefaultXiLinkType; //"Content"
                                    oXILink.StatusTypeID = XIConstant.DefaultStatusTypeID; //10
                                    oCR = Save_XILink(oXILink);
                                    oTrace.oTrace.Add(oCR.oTrace);
                                    if (oCR.bOK && oCR.oResult != null)
                                    {
                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                                        oCResult.oResult = "Success";
                                        oBOI = (XIIBO)oCR.oResult;
                                        var sXILinkID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == oBOI.BOD.sPrimaryKey.ToLower()).Select(m => m.sValue).FirstOrDefault();
                                        int.TryParse(sXILinkID, out iXILinkID);
                                        //iXiLinkIDForBO = iXILinkID;
                                        if (iXILinkID > 0)
                                        {
                                            XiLinkNV oNV = new XiLinkNV();
                                            oNV.XiLinkID = iXILinkID;
                                            oNV.Name = XIConstant.DefaultStartAction; //"StartAction"
                                            oNV.Value = XIConstant.DefaultDialog; //"Dialog"
                                            Save_XILinkNV(oNV);
                                            oTrace.oTrace.Add(oCR.oTrace);
                                            if (oCR.bOK && oCR.oResult != null)
                                            {
                                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                                            }
                                            else
                                            {
                                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                            }
                                            oNV = new XiLinkNV();
                                            oNV.XiLinkID = iXILinkID;
                                            oNV.Name = XIConstant.DefaultDialogID; //"DialogID";
                                            oNV.Value = Params.FKiDialogID.ToString();
                                            oCR = Save_XILinkNV(oNV);
                                            oTrace.oTrace.Add(oCR.oTrace);
                                            if (oCR.bOK && oCR.oResult != null)
                                            {
                                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                                            }
                                            else
                                            {
                                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                            }
                                        }
                                        if (Params.FKiOneClickID != 0)
                                        {
                                            int i1ClickID = Params.FKiOneClickID;
                                            //int.TryParse(OneClickID, out i1ClickID);
                                            if (i1ClickID > 0)
                                            {
                                                //Create xilink
                                                // var sxilink = DefaultBoChartsXilink("Pie");
                                                List<CNV> nParams = new List<CNV>();
                                                nParams.Add(new CNV { sName = "1ClickID", sValue = i1ClickID.ToString() });
                                                nParams.Add(new CNV { sName = "RowXilinkID", sValue = Params.iRowXilinkID.ToString() });
                                                foreach (var Param in nParams)
                                                {
                                                    XIDComponentParam oParam1 = new XIDComponentParam();
                                                    oParam1.sName = Param.sName;
                                                    oParam1.sValue = Param.sValue;
                                                    oParam1.FKiComponentID = Params.FKiComponentTypeID;
                                                    oParam1.iLayoutMappingID = iPlaceHolderID;
                                                    oCR = Save_ComponentParam(oParam1);
                                                    oTrace.oTrace.Add(oCR.oTrace);
                                                    if (oCR.bOK && oCR.oResult != null)
                                                    {
                                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                                                        oCResult.oResult = "Success";
                                                    }
                                                    else
                                                    {
                                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                    }
                                }
                                else
                                {
                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                }
                            }
                        }
                    }
                    else
                    {
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                    }
                }
                if (oTrace.iStatus != (int)xiEnumSystem.xiFuncResult.xiError)
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                }
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error While Inserting Application" });
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Application insertion failed" });
                oSigCR.oStack.Add(new CNV { sName = "Error Message", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }
        /*:::::::::::::::::::::::::Create XiSystem Type 1Click::::::::::::::::::::::::::::::::::
          :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::*/
        public CResult CreateXISystemDefaultBODashboards1Click(int BOID, string SearchType, string sSystemType, string IsRowXilinkID, string DBName)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Adding new layout for default dashboard";//expalin about this method logic
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            int io1ClickID = 0;
            try
            {
                XIDBO oBODef = new XIDBO();
                oBODef = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, null, BOID.ToString());
                if (oBODef != null)
                {
                    XID1Click o1Click = new XID1Click();
                    o1Click.FromBos = oBODef.TableName;
                    sTableName = oBODef.TableName;
                    sBOName = oBODef.Name;
                    /* :::::Create Bar Chart current Year Query::::*/
                    if (sSystemType == "100")
                    {
                        o1Click.Name = o1Click.Code = o1Click.Description = o1Click.Title = oBODef.Name + "  current Year count by status";
                        //o1Click.Query = "SELECT  op1.sOptionName as Col into #temp FROM " + sTableName + " bo1 INNER JOIN " + DBName + ".dbo.XIBOOptionList_T_N op1 ON bo1.iStatus = op1.sValues and op1.boid = " + BOID + " and op1.name = 'iStatus' group by op1.sOptionName  declare @Columns nvarchar(max)declare @IsNullColumns nvarchar(max)  select @Columns = STUFF((select ',[' + Col + ']' from #temp for xml path('')), 1, 1, '')  select @IsNullColumns = STUFF((select ',IsNull([' + Col + '],0) AS [' + Col + ']' from #temp for xml path('')), 1, 1, '')  DECLARE @query NVARCHAR(4000) SET @query = N'SELECT year,' + @IsNullColumns + ' FROM (SELECT  op.sOptionName as Type1, convert(nvarchar, year(bo." + SearchType + ")) as year, COUNT(bo." + SearchType + ") as count1 FROM  " + sTableName + " bo  INNER JOIN " + DBName + ".dbo.XIBOOptionList_T_N op  ON bo.iStatus = op.sValues and op.boid = " + BOID + " and op.name = ''iStatus'' Where year(bo." + SearchType + ")=year(getdate()) group by bo." + SearchType + ", op.sOptionName, year(bo." + SearchType + ")) p PIVOT  ( max(count1)  FOR type1 IN ('+ @Columns + ' )) AS pvt ; ' EXECUTE(@query) drop table #temp";
                        o1Click.Query = "select a.sOptionName as StatusType, count(l." + SearchType + ") as DataCount from " + DBName + ".dbo.XIBOOptionList_T_N a inner join  " + sTableName + " as l on l.iStatus = a.sValues WHERE a.boid = " + BOID + " and a.Name ='iStatus' and l." + SearchType + " is not null and Year(l." + SearchType + ")=Year(GETDATE()) group by a.sOptionName, l.iStatus order by sOptionName";
                    }
                    /* :::::Create Bar Chart current Month Query::::*/
                    else if (sSystemType == "80")
                    {
                        o1Click.Name = o1Click.Code = o1Click.Description = o1Click.Title = oBODef.Name + " current Month count by status";
                        //o1Click.Query = "SELECT  op1.sOptionName as Col into #temp FROM " + sTableName + " bo1 INNER JOIN " + DBName + ".dbo.XIBOOptionList_T_N op1 ON bo1.iStatus = op1.sValues and op1.boid = " + BOID + " and op1.name = 'iStatus' group by op1.sOptionName  declare @Columns nvarchar(max) declare @IsNullColumns nvarchar(max)  select @Columns = STUFF((select ',[' + Col + ']' from #temp for xml path('')), 1, 1, '')  select @IsNullColumns = STUFF((select ',IsNull([' + Col + '],0) AS [' + Col + ']' from #temp for xml path('')), 1, 1, '')  DECLARE @query NVARCHAR(4000) SET @query = N'SELECT MonthName,' + @IsNullColumns + ' FROM (SELECT  op.sOptionName as Type1, DateName(month, DateAdd(month, month(bo." + SearchType + "), 0) - 1) as MonthName, Month(bo." + SearchType + ") as MonthNumber, COUNT(bo." + SearchType + ") as count1 FROM  " + sTableName + " bo  INNER JOIN " + DBName + ".dbo.XIBOOptionList_T_N op  ON bo.iStatus = op.sValues Where op.boid = " + BOID + " and op.name = ''iStatus'' and month(current_timestamp)=month(bo." + SearchType + ") group by bo." + SearchType + ", op.sOptionName, month(bo." + SearchType + ")) p PIVOT  ( max(count1)  FOR type1 IN ('+ @Columns + ' )) AS pvt Order By MonthNumber ; ' EXECUTE(@query) drop table #temp";
                        o1Click.Query = "select a.sOptionName as StatusType, count(l." + SearchType + ") as DataCount from " + DBName + ".dbo.XIBOOptionList_T_N a inner join  " + sTableName + " as l on l.iStatus = a.sValues WHERE a.boid = " + BOID + " and a.Name ='iStatus' and l." + SearchType + " is not null and Month(l." + SearchType + ")=MONTH(GETDATE())  group by a.sOptionName, l.iStatus order by sOptionName";
                    }
                    /* :::::Create Pie Chart Query::::*/
                    else if (sSystemType == "10")
                    {
                        o1Click.Name = o1Click.Code = o1Click.Description = o1Click.Title = oBODef.Name + " Count by status";
                        o1Click.Query = "select a.sOptionName as StatusType, count(l." + SearchType + ") as DataCount from " + DBName + ".dbo.XIBOOptionList_T_N a inner join  " + sTableName + " as l on l." + SearchType + " = a.sValues WHERE a.boid = " + BOID + " and a.Name ='" + SearchType + "' and l." + SearchType + " is not null group by a.sOptionName, l." + SearchType + " order by sOptionName";
                    }
                    o1Click.IsRowClick = true;
                    o1Click.RowXiLinkID = Convert.ToInt32(IsRowXilinkID);
                    iBODID = o1Click.BOID = oBODef.BOID;
                    o1Click.DisplayAs = 50; o1Click.IsFilterSearch = false;
                    o1Click.IsCreate = false;
                    o1Click.IsRefresh = false;
                    o1Click.CategoryID = 1;
                    o1Click.sLastUpdate = "XISystem";
                    o1Click.FKiAppID = iAppID;
                    var o1ClickDef = Save_1Click(o1Click);
                    if (o1ClickDef.bOK && o1ClickDef.oResult != null)
                    {
                        io1ClickID = (int)o1ClickDef.oResult;
                        oCResult.oResult = Convert.ToString(o1ClickDef.oResult);
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                        oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                        return oCResult;
                    }
                }
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error While Inserting Application" });
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Application insertion failed" });
                oSigCR.oStack.Add(new CNV { sName = "Error Message", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        #endregion BoDashBoardCharts

        #region CreateIF

        public CResult Save_Project(List<CNV> oParams)
        {
            var iID = oParams.Where(m => m.sName == XIConstant.Param_InstanceID).Select(m => m.sValue).FirstOrDefault();
            int ID = 0;
            int.TryParse(iID, out ID);
            if (ID > 0)
            {
                XIInfraCache oCache = new XIInfraCache();
                XIIXI oXI = new XIIXI();
                var BOI = oXI.BOI("xidocumenttree", ID.ToString());
                if (BOI != null && BOI.Attributes.Count() > 0)
                {
                    var sType = BOI.AttributeI("stype").sValue;
                    var bIsProject = BOI.AttributeI("bproject").sValue;
                    var WhrParms = new List<CNV>();
                    WhrParms.Add(new CNV { sName = "fkinodeid", sValue = BOI.AttributeI("id").sValue });
                    var oProjI = oXI.BOI("project", null, null, WhrParms);
                    if (bIsProject == "True" && sType == "10")
                    {
                        if (oProjI != null && oProjI.Attributes.Count() > 0)
                        {
                            oProjI.SetAttribute("sname", BOI.AttributeI("sname").sValue);
                            //oProjI.SetAttribute(XIConstant.Key_XIDeleted, "0");
                            var oCR = oProjI.Save(oProjI);
                        }
                        else
                        {
                            var BOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "Project");
                            XIIBO oBOI = new XIIBO();
                            oBOI.SetAttribute("sname", BOI.AttributeI("sname").sValue);
                            oBOI.SetAttribute("fkinodeid", BOI.AttributeI("id").sValue);
                            //oBOI.SetAttribute(XIConstant.Key_XIDeleted, "0");
                            oBOI.BOD = BOD;
                            var oCR = oBOI.Save(oBOI);
                        }
                    }
                    else
                    {
                        if (oProjI != null && oProjI.Attributes.Count() > 0)
                        {
                            oProjI.SetAttribute("sname", BOI.AttributeI("sname").sValue);
                            oProjI.SetAttribute(XIConstant.Key_XIDeleted, "1");
                            var oCR = oProjI.Save(oProjI);
                        }
                    }

                }
            }
            return null;
        }

        public CResult DisapproveDocument(List<CNV> oParams)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "";
            try
            {

                oTrace.oParams.Add(new CNV { sName = "", sValue = "" });
                var iID = oParams.Where(m => m.sName == XIConstant.Param_InstanceID).Select(m => m.sValue).FirstOrDefault();
                var sSessionID = oParams.Where(m => m.sName == "sSessionID").Select(m => m.sValue).FirstOrDefault();
                var sGUID = oParams.Where(m => m.sName == "sGUID").Select(m => m.sValue).FirstOrDefault();
                var sAcionType = oParams.Where(m => m.sName == "sActionType").Select(m => m.sValue).FirstOrDefault();
                int ID = 0;
                int.TryParse(iID, out ID);
                if (ID > 0)
                {
                    XIInfraCache oCache = new XIInfraCache();
                    XICacheInstance oGUIDParams = oCache.GetAllParamsUnderGUID(sSessionID, sGUID, null);
                    var sRole = string.Empty;
                    if (oGUIDParams.NMyInstance != null && oGUIDParams.NMyInstance.ContainsKey("sRole"))
                    {
                        sRole = oGUIDParams.NMyInstance["sRole"].sValue;
                        if (sAcionType == null)
                        {
                            sAcionType = oGUIDParams.NMyInstance["sActionType"].sValue;
                        }
                    }

                    var oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "xidocumenttree");
                    var oCloneBOD = (XIDBO)oBOD.Clone(oBOD);
                    oCloneBOD.Attributes = oCloneBOD.Attributes.ToDictionary(dic => dic.Key, dic => dic.Value);
                    oCloneBOD.Scripts = new Dictionary<string, XIDScript>();
                    XIIXI oXI = new XIIXI();
                    var oDocI = oXI.BOI("xidocumenttree", ID.ToString());

                    if (oDocI != null && oDocI.Attributes.Count() > 0)
                    {
                        if (!string.IsNullOrEmpty(sRole) && (sRole.ToLower() == "CreateIFUser".ToLower() || sRole.ToLower() == "Approver".ToLower()))
                        {
                            var iAppStatus = oDocI.AttributeI("iApprovalStatus").sValue;
                            var sVersion = oDocI.AttributeI("sDocType").sValue;
                            if (!string.IsNullOrEmpty(sAcionType) && sAcionType.ToLower() == "Save".ToLower())
                            {
                                var sComment = oDocI.AttributeI("sComment").sValue;
                                if (!string.IsNullOrEmpty(sComment) && sVersion != "Version")
                                {
                                    if (iAppStatus == "20")
                                    {
                                        oDocI.SetAttribute("iApprovalStatus", "30");
                                    }
                                    else if (iAppStatus == "10")
                                    {
                                        oDocI.SetAttribute("iApprovalStatus", "20");
                                    }
                                    else if (iAppStatus == "30")
                                    {
                                        oDocI.SetAttribute("iApprovalStatus", "20");
                                    }
                                    oDocI.BOD = oCloneBOD;
                                    oDocI.SetAttribute("sComment", "");
                                    oCR = oDocI.Save(oDocI);
                                    if (oCR.bOK && oCR.oResult != null)
                                    {
                                        var iStatus = oDocI.AttributeI("iApprovalStatus").sValue;
                                        var oCBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "commenthistory");
                                        XIIBO oCHI = new XIIBO();
                                        oCHI.BOD = oCBOD;
                                        oCHI.SetAttribute("FKiDocID", oDocI.AttributeI("iVersionBatchID").sValue);
                                        oCHI.SetAttribute("sComment", sComment);
                                        if (iStatus == "10")
                                        {
                                            oCHI.SetAttribute("sDescription", "Green");
                                        }
                                        else if (iStatus == "20")
                                        {
                                            oCHI.SetAttribute("sDescription", "Red");
                                        }
                                        else if (iStatus == "30")
                                        {
                                            oCHI.SetAttribute("sDescription", "Amber");
                                        }
                                        oCR = oCHI.Save(oCHI);
                                        if (oCR.bOK && oCR.oResult != null)
                                        {

                                        }
                                    }
                                }
                                else if (iAppStatus == "30")
                                {

                                }

                                //if (!string.IsNullOrEmpty(sRole) && sRole.ToLower() == "CreateIFAdmin".ToLower())
                                //{
                                //    var iApprStatus = BOI.AttributeI("iApprovalStatus").sValue;
                                //    if (iApprStatus == "60")
                                //    {
                                //        BOI.SetAttribute("dtReapproval", DateTime.Now.ToString());
                                //        BOI.BOD = oCloneBOD;
                                //        oCR = BOI.Save(BOI);
                                //        if (oCR.bOK && oCR.oResult != null)
                                //        {

                                //        }
                                //    }
                                //}
                                //else if (!string.IsNullOrEmpty(sRole) && sRole.ToLower() == "CreateIFUser".ToLower())
                                //{
                                //    BOI.SetAttribute("iApprovalStatus", "50");
                                //    BOI.SetAttribute("dtDeclineApproval", DateTime.Now.ToString());
                                //    BOI.BOD = oCloneBOD;
                                //    oCR = BOI.Save(BOI);
                                //    if (oCR.bOK && oCR.oResult != null)
                                //    {

                                //    }
                                //}
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
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        public CResult ApproveDocument(List<CNV> oParams)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "";
            try
            {
                oTrace.oParams.Add(new CNV { sName = "", sValue = "" });
                var iID = oParams.Where(m => m.sName == XIConstant.Param_InstanceID).Select(m => m.sValue).FirstOrDefault();
                var sSessionID = oParams.Where(m => m.sName == "sSessionID").Select(m => m.sValue).FirstOrDefault();
                var sGUID = oParams.Where(m => m.sName == "sGUID").Select(m => m.sValue).FirstOrDefault();
                var sAcionType = oParams.Where(m => m.sName == "sActionType").Select(m => m.sValue).FirstOrDefault();
                var sRole = oParams.Where(m => m.sName == "srolename").Select(m => m.sValue).FirstOrDefault();
                int ID = 0;
                int.TryParse(iID, out ID);
                if (ID > 0)
                {
                    XIInfraCache oCache = new XIInfraCache();
                    var oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "xidocumenttree");
                    var oCloneBOD = (XIDBO)oBOD.Clone(oBOD);
                    oCloneBOD.Attributes = oCloneBOD.Attributes.ToDictionary(dic => dic.Key, dic => dic.Value);
                    oCloneBOD.Scripts = new Dictionary<string, XIDScript>();
                    XIIXI oXI = new XIIXI();
                    var oDocI = oXI.BOI("xidocumenttree", ID.ToString());

                    if (oDocI != null && oDocI.Attributes.Count() > 0)
                    {
                        if (!string.IsNullOrEmpty(sRole) && (sRole.ToLower() == "Approver".ToLower()))
                        {
                            if (!string.IsNullOrEmpty(sAcionType) && sAcionType.ToLower() == "Button".ToLower())
                            {
                                oDocI.SetAttribute("iApprovalStatus", "10");
                                oDocI.SetAttribute("sComment", "");
                                oDocI.BOD = oCloneBOD;
                                oCR = oDocI.Save(oDocI);
                                if (oCR.bOK && oCR.oResult != null)
                                {
                                    oCResult.oResult = "Success";
                                    oDocI.SetAttribute(oBOD.sPrimaryKey, "0");
                                    int iVersion = 0;
                                    var sVersion = oDocI.AttributeI("sversion").sValue;
                                    var sName = oDocI.AttributeI("sName").sValue;
                                    if (sVersion != "0")
                                    {
                                        int.TryParse(sVersion, out iVersion);
                                        iVersion = iVersion + 1;
                                    }
                                    else if (sVersion == "0")
                                    {
                                        iVersion = 1;
                                    }
                                    sName = "v" + iVersion + "_" + sName + " Approved ";
                                    oDocI.SetAttribute("sName", sName);
                                    oDocI.SetAttribute(XIConstant.Key_XIDeleted, "1");
                                    oDocI.SetAttribute("sDocType", "Version");
                                    oDocI.Attributes.Values.ToList().ForEach(m => m.bDirty = true);
                                    oCR = oDocI.Save(oDocI);
                                    if (oCR.bOK && oCR.oResult != null)
                                    {

                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                oCResult.oResult = "Failure";
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        #endregion CreateIF

        #region CodeLog

        List<XIIBO> oBulkBO = new List<XIIBO>();

        public CResult Save_CodeLog(CTraceStack oCodeTrace)
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
                Build_CodeLogObject(oCodeTrace);
                var BOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XICodeLog");
                oBulkBO.ForEach(f => f.BOD = BOD);
                XIIBO BulkInsert = new XIIBO();
                var MakeDatatble = BulkInsert.MakeBulkSqlTable(oBulkBO);
                BulkInsert.SaveBulk(MakeDatatble, BOD.iDataSource, BOD.TableName);
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                oCResult.oResult = "Success";
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        public void Build_CodeLogObject(CTraceStack oTrace)
        {
            XIIBO oBOI = new XIIBO();
            oBOI.SetAttribute("sName", oTrace.sName);
            oBOI.SetAttribute("sParentID", oTrace.sParentID);
            oBOI.SetAttribute("sProcessID", oTrace.sProcessID);
            oBOI.SetAttribute("sClass", oTrace.sClass);
            oBOI.SetAttribute("sMethod", oTrace.sMethod);
            oBOI.SetAttribute("sCode", oTrace.sCode);
            oBOI.SetAttribute("iStatus", oTrace.iStatus.ToString());
            oBOI.SetAttribute("sMessage", oTrace.sMessage);
            oBOI.SetAttribute("sQuery", oTrace.sQuery);
            oBOI.SetAttribute("sQueryParams", oTrace.sQueryParams);
            oBOI.SetAttribute("iLapsedTime", Math.Round(oTrace.iLapsedTime, 3).ToString());
            oBOI.SetAttribute("iAlert", oTrace.iAlert.ToString());
            List<string> Params = new List<string>();
            Params.Add(oTrace.oParams.Select(m => m.sName + "=" + m.sValue).FirstOrDefault());
            oBOI.SetAttribute("sParams", string.Join(",", Params));
            oBulkBO.Add(oBOI);
            if (oTrace.oTrace != null && oTrace.oTrace.Count() > 0)
            {
                foreach (var oTr in oTrace.oTrace)
                {
                    oTr.sParentID = oTrace.sProcessID;
                    oTr.sProcessID = Guid.NewGuid().ToString().Substring(0, 11);
                    Build_CodeLogObject(oTr);
                }
            }
        }

        #endregion CodeLog
        #region QSLink

        public CResult Save_QSLink(XIQSLinkDefintion oQSLink)
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
                var BOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIQSLinkDefinition");
                XIIBO oBOI = new XIIBO();
                oBOI.BOD = BOD;
                foreach (var NV in oQSLink.NVs)
                {
                    oBOI.SetAttribute("ID", NV.ID.ToString());
                    oBOI.SetAttribute("FKiApplicationID", oQSLink.FKiApplicationID.ToString());
                    oBOI.SetAttribute("sName", oQSLink.sName);
                    oBOI.SetAttribute("sCode", oQSLink.sCode);
                    if (!string.IsNullOrEmpty(NV.XiLinkName))
                    {
                        var oXILink = (XILink)oCache.GetObjectFromCache(XIConstant.CacheXILink, NV.XiLinkName);
                        oBOI.SetAttribute("FKiXILinkID", oXILink.XiLinkID.ToString());
                    }
                    oBOI.SetAttribute("sType", NV.sType);
                    oBOI.SetAttribute("rOrder", NV.rOrder.ToString());
                    oBOI.SetAttribute("sRunType", NV.sRunType);
                    oBOI.SetAttribute("FKIXIScriptID", NV.FKIXIScriptID.ToString());
                    oBOI.SetAttribute("StatusTypeID", oQSLink.StatusTypeID.ToString());
                    oCR = oBOI.SaveV2(oBOI);
                    oTrace.oTrace.Add(oCR.oTrace);
                    if (oCR.bOK && oCR.oResult != null)
                    {
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                        oCResult.oResult = "Success";
                    }
                    else
                    {
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                    }
                }
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        public CResult Delete_QSLink(int ID)
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
                var BOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIQSLinkDefinition");
                XIIXI oXI = new XIIXI();
                var oBOI = oXI.BOI("XIQSLinkDefinition", ID.ToString());
                if (oBOI != null && oBOI.Attributes.Count() > 0)
                {
                    oBOI.BOD = BOD;
                    oBOI.SetAttribute("ID", ID.ToString());
                    oBOI.SetAttribute(XIConstant.Key_XIDeleted, "1");
                    oCR = oBOI.SaveV2(oBOI);
                    oTrace.oTrace.Add(oCR.oTrace);
                    if (oCR.bOK && oCR.oResult != null)
                    {
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                        oCResult.oResult = "Success";
                    }
                    else
                    {
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                    }
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                }
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        public CResult Save_QSLinkMapping(XIQSLink oMapping)
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
                var BOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIQSLink");
                var oBOI = new XIIBO();
                oBOI.BOD = BOD;
                oBOI.SetAttribute("ID", oMapping.ID.ToString());
                oBOI.SetAttribute("sCode", oMapping.sCode.ToString());
                oBOI.SetAttribute("FKiQSDefinitionID", oMapping.FKiQSDefinitionID.ToString());
                oBOI.SetAttribute("FKiStepDefinitionID", oMapping.FKiStepDefinitionID.ToString());
                oBOI.SetAttribute("FKiSectionDefinitionID", oMapping.FKiSectionDefinitionID.ToString());
                oCR = oBOI.SaveV2(oBOI);
                oTrace.oTrace.Add(oCR.oTrace);
                if (oCR.bOK && oCR.oResult != null)
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                    oCResult.oResult = "Success";
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                }
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        public CResult Delete_QSLinkMapping(int SectionID, string sCode)
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
                var BOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIQSLink");
                XIIXI oXI = new XIIXI();
                List<CNV> Parms = new List<CNV>();
                Parms.Add(new CNV { sName = "FKiSectionDefinitionID", sValue = SectionID.ToString() });
                Parms.Add(new CNV { sName = "sCode", sValue = sCode });
                var oBOI = oXI.BOI("XIQSLink", null, null, Parms);
                if (oBOI != null && oBOI.Attributes.Count() > 0)
                {
                    oBOI.SetAttribute(XIConstant.Key_XIDeleted, "1");
                    oCR = oBOI.SaveV2(oBOI);
                    oTrace.oTrace.Add(oCR.oTrace);
                    if (oCR.bOK && oCR.oResult != null)
                    {
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                        oCResult.oResult = "Success";
                    }
                    else
                    {
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                    }
                }
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        #endregion QSLink

        #region NannoApp

        public CResult Build_NannoApp(List<CNV> oParams)
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
                XIIXI oXI = new XIIXI();
                int iID = 0;
                string sBO = oParams.Where(m => m.sName.ToLower() == XIConstant.Param_BO.ToLower()).Select(m => m.sValue).FirstOrDefault();
                string sID = oParams.Where(m => m.sName.ToLower() == XIConstant.Param_InstanceID.ToLower()).Select(m => m.sValue).FirstOrDefault();
                int.TryParse(sID, out iID);
                oTrace.oParams.Add(new CNV { sName = "sBO", sValue = sBO });
                oTrace.oParams.Add(new CNV { sName = "iID", sValue = iID.ToString() });
                int iNannoAppID = 0;
                if (!string.IsNullOrEmpty(sBO) && iID > 0)
                {
                    //get nanno app instance data
                    var oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "Nanno App Instance");
                    var oNannoIns = oXI.BOI("Nanno App Instance", iID.ToString());
                    if (oNannoIns != null && oNannoIns.Attributes.Count() > 0)
                    {
                        var NannoAppID = oNannoIns.AttributeI("FKiNannoAppID").sValue;
                        int.TryParse(NannoAppID, out iNannoAppID);
                        if (iNannoAppID > 0)
                        {
                            //get nanno app data
                            var oNannoApp = oXI.BOI("Nanno App", iNannoAppID.ToString());
                            var NannoName = oNannoIns.AttributeI("sName").sValue;
                            var AppID = oNannoApp.AttributeI("FKiApplicationID").sValue;
                            int iApplicationID = 0;
                            int.TryParse(AppID, out iApplicationID);
                            var oAppI = oXI.BOI("XI Application", iApplicationID.ToString());
                            string sTheme = string.Empty;
                            if (oAppI != null && oAppI.Attributes.Count() > 0)
                            {
                                sTheme = oAppI.AttributeI("sTheme").sValue;
                            }
                            if (NannoName.Length > 16)
                            {
                                NannoName = NannoName.Substring(0, 15);
                            }
                            NannoName = NannoName.Replace(" ", "");
                            sAppName = NannoName;
                            List<CNV> mParams = new List<CNV>();
                            mParams.Add(new CNV { sName = XIConstant.DB_Type, sValue = XIConstant.DB_Nanno });
                            //create db for nanno app instance
                            oCR = Save_DataSource(mParams);
                            oTrace.oTrace.Add(oCR.oTrace);
                            if (oCR.bOK && oCR.oResult != null)
                            {
                                var oDS = (XIIBO)oCR.oResult;
                                var sDataSourceID = oDS.Attributes.Where(m => m.Key.ToLower() == "id").Select(m => m.Value).Select(m => m.sValue).FirstOrDefault();
                                var iNannoOrgID = 0;
                                XIDOrganisation oOrg = new XIDOrganisation();
                                oOrg.FKiApplicationID = iApplicationID;
                                oOrg.Name = NannoName;
                                oOrg.Email = NannoName + "@Nanno.com";
                                oOrg.Password = "Nanno.123";
                                oOrg.Description = NannoName;
                                oOrg.DatabaseName = NannoName + "_Nanno";
                                oOrg.DatabaseType = "Specific";
                                var iAppThemeID = 0;
                                int.TryParse(sTheme, out iAppThemeID);
                                oOrg.ThemeID = 162;
                                oOrg.bNannoApp = true;
                                //create organisation for nanno app instance
                                oCR = Save_Organisation(oOrg);
                                oTrace.oTrace.Add(oCR.oTrace);
                                if (oCR.bOK && oCR.oResult != null)
                                {
                                    var oOr = (XIIBO)oCR.oResult;
                                    var sOrgID = oOr.Attributes.Where(m => m.Key.ToLower() == "id").Select(m => m.Value).Select(m => m.sValue).FirstOrDefault();
                                    int.TryParse(sOrgID, out iNannoOrgID);
                                    var oCloneBOD = (XIDBO)oBOD.Clone(oBOD);
                                    oCloneBOD.Scripts = new Dictionary<string, XIDScript>();
                                    oNannoIns.BOD = oCloneBOD;
                                    //update organisatonid aganist newly created nanno app instance
                                    oNannoIns.SetAttribute("FKiOrgID", iNannoOrgID.ToString());
                                    oCR = oNannoIns.SaveV2(oNannoIns);
                                    oTrace.oTrace.Add(oCR.oTrace);
                                    if (oCR.bOK && oCR.oResult != null)
                                    {
                                        mParams = new List<CNV>();
                                        mParams.Add(new CNV { sName = "iNannoAppID", sValue = iNannoAppID.ToString() });
                                        mParams.Add(new CNV { sName = XIConstant.Param_DataSource, sValue = sDataSourceID });
                                        //add Nanno App defined objects to nanno app instance DB
                                        oCR = Sync_NannoObject(mParams);
                                        oTrace.oTrace.Add(oCR.oTrace);
                                        if (oCR.bOK && oCR.oResult != null)
                                        {
                                            oCResult.oResult = "Success";
                                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                                        }
                                        else
                                        {
                                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                        }
                                    }
                                    else
                                    {
                                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                    }
                                }
                                else
                                {
                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                }
                            }
                            else
                            {
                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                            }
                        }

                    }
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory Param: sBO or iID is missing";
                }
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        public CResult Sync_NannoObject(List<CNV> oParams)
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
                int iNannoAppID = 0;
                int iDataSourceID = 0;
                string sDataSource = oParams.Where(m => m.sName.ToLower() == XIConstant.Param_DataSource.ToLower()).Select(m => m.sValue).FirstOrDefault();
                int.TryParse(sDataSource, out iDataSourceID);
                string sNannoAppID = oParams.Where(m => m.sName.ToLower() == "iNannoAppID".ToLower()).Select(m => m.sValue).FirstOrDefault();
                int.TryParse(sNannoAppID, out iNannoAppID);
                oTrace.oParams.Add(new CNV { sName = "iDataSourceID", sValue = iDataSourceID.ToString() });
                oTrace.oParams.Add(new CNV { sName = "iNannoAppID", sValue = iNannoAppID.ToString() });
                if (iDataSourceID > 0 && iNannoAppID > 0)//check mandatory params are passed or not
                {
                    //Get all nanno app objects of the selected nanno app
                    XID1Click o1ClickD = new XID1Click();
                    o1ClickD.BOID = 1351;
                    o1ClickD.Query = "Select * from orgobjecttype_t where fkinannoappid=" + iNannoAppID + " and fkinannoappinstid is null";
                    var oRes = o1ClickD.OneClick_Run();
                    if (oRes != null && oRes.Count() > 0)
                    {
                        foreach (var items in oRes.Values.ToList())
                        {
                            //get the object definition to check newly added BO or attribute
                            var iBODID = items.AttributeI("FKiBODID").sValue;
                            var oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, null, iBODID);
                            foreach (var Attr in oBOD.Attributes.Values.ToList())
                            {
                                List<XIDAttribute> oAttr = new List<XIDAttribute>();
                                oAttr.Add(Attr);
                                //Sync newly add bo or attribute to nanno app instance organisation DB
                                oCR = Sync_BO(oBOD.TableName, oAttr, iDataSourceID);
                                oTrace.oTrace.Add(oCR.oTrace);
                                if (oCR.bOK && oCR.oResult != null)
                                {
                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                                    oCResult.oResult = "Success";
                                }
                                else
                                {
                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                }
                            }
                        }
                    }
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory Param: iDataSourceID or iNannoAppID is missing";
                }
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        public CResult Check_NannoObject(List<CNV> oParams)
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
                XIDXI oXID = new XIDXI();
                oXID.sOrgDatabase = "Nanno_Core";
                int iNannoAppID = 0;
                string sNannoAppID = oParams.Where(m => m.sName.ToLower() == "iNannoAppID".ToLower()).Select(m => m.sValue).FirstOrDefault();
                int.TryParse(sNannoAppID, out iNannoAppID);
                oTrace.oParams.Add(new CNV { sName = "iNannoAppID", sValue = iNannoAppID.ToString() });
                if (iNannoAppID > 0)//check mandatory params are passed or not
                {
                    //Get all nanno app instances
                    XID1Click o1ClickD = new XID1Click();
                    o1ClickD.BOID = 1390;
                    o1ClickD.Query = "Select * from XINannoAppInstance_T where fkinannoappid=" + iNannoAppID;
                    var AppInstance = o1ClickD.OneClick_Run();
                    if (AppInstance != null && AppInstance.Count() > 0)
                    {
                        foreach (var Instance in AppInstance.Values.ToList())
                        {
                            //get the org definition to know the database of the org
                            var iOrgID = Instance.AttributeI("FKiOrgID").sValue;
                            oCR = oXID.Get_OrgDefinition(null, iOrgID);
                            oTrace.oTrace.Add(oCR.oTrace);
                            if (oCR.bOK && oCR.oResult != null)
                            {
                                XIDOrganisation oOrgD = (XIDOrganisation)oCR.oResult;
                                var oDataSrcD = (XIDataSource)oCache.GetObjectFromCache(XIConstant.CacheDataSource, oOrgD.DatabaseName);
                                List<CNV> mParams = new List<CNV>();
                                mParams.Add(new CNV { sName = "iNannoAppID", sValue = iNannoAppID.ToString() });
                                mParams.Add(new CNV { sName = XIConstant.Param_DataSource, sValue = oDataSrcD.ID.ToString() });
                                //Sync newly add bo or attribute to nanno app instance organisation DB
                                oCR = Sync_NannoObject(mParams);
                                oTrace.oTrace.Add(oCR.oTrace);
                                if (oCR.bOK && oCR.oResult != null)
                                {
                                    oCResult.oResult = "Success";
                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                                }
                                else
                                {
                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                }
                            }
                            else
                            {
                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                            }
                        }
                    }
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory param iNannoAppID is missing";
                }
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        public CResult Register_NannoUser(List<CNV> oParams)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Register user with the nanno app";//expalin about this method logic
            try
            {
                XIIXI oXI = new XIIXI();
                var iUserID = 0;
                var iNannoAppInstID = 0;
                var sUser = oParams.Where(m => m.sName.ToLower() == "iUserID".ToLower()).Select(m => m.sValue).FirstOrDefault();
                int.TryParse(sUser, out iUserID);
                var NannoAppInst = oParams.Where(m => m.sName.ToLower() == "iNannoAppInstID".ToLower()).Select(m => m.sValue).FirstOrDefault();
                int.TryParse(NannoAppInst, out iNannoAppInstID);
                if (iUserID > 0 && iNannoAppInstID > 0)
                {
                    List<CNV> WhrPrms = new List<CNV>();
                    WhrPrms.Add(new CNV { sName = "FKiUserID", sValue = iUserID.ToString() });
                    WhrPrms.Add(new CNV { sName = "FKiNannoAppInstID", sValue = iNannoAppInstID.ToString() });
                    var NannoUser = oXI.BOI("NannoUser", null, null, WhrPrms);
                    if (NannoUser != null && NannoUser.Attributes.Count() > 0)
                    {
                        oCResult.oResult = "Already Exist";
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                    }
                    else
                    {
                        var oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "NannoUser");
                        XIIBO oBOI = new XIIBO();
                        oBOI.BOD = oBOD;
                        oBOI.SetAttribute("FKiUserID", iUserID.ToString());
                        oBOI.SetAttribute("FKiNannoAppInstID", iNannoAppInstID.ToString());
                        oCR = oBOI.SaveV2(oBOI);
                        oTrace.oTrace.Add(oCR.oTrace);
                        if (oCR.bOK && oCR.oResult != null)
                        {
                            oCResult.oResult = "Success";
                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                        }
                        else
                        {
                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                        }
                    }
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory Param: UserID or NannoAppInstID is missing";
                }
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        public CResult Migrate_NannoUser(List<CNV> oParams)
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
                Dictionary<string, string> Users = new Dictionary<string, string>();
                Dictionary<string, string> Roles = new Dictionary<string, string>();
                XIIXI oXI = new XIIXI();
                int iApplicationID = 0;
                var AppID = oParams.Where(m => m.sName.ToLower() == XIConstant.Param_ApplicationID.ToLower()).Select(m => m.sValue).FirstOrDefault();
                int.TryParse(AppID, out iApplicationID);
                oTrace.oParams.Add(new CNV { sName = "iApplicationID", sValue = iApplicationID.ToString() });
                if (iApplicationID > 0)//check mandatory params are passed or not
                {
                    var oAppD = (XIDApplication)oCache.GetObjectFromCache(XIConstant.CacheApplication, null, iApplicationID.ToString());
                    var AppCoreDB = oAppD.sDatabaseName;
                    var NannoCoreDB = (XIDataSource)oCache.GetObjectFromCache(XIConstant.CacheDataSource, "Nanno_Core");
                    var oUserBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIAPPUsers_AU_T");
                    var oUserBODC = (XIDBO)oUserBOD.Clone(oUserBOD);
                    oUserBODC.iDataSource = NannoCoreDB.ID;
                    var oRoleBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIAppRoles_AR_T");
                    var oRoleBODC = (XIDBO)oRoleBOD.Clone(oRoleBOD);
                    oRoleBODC.iDataSource = NannoCoreDB.ID;
                    var oUserRoleBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIAppUserRoles_AUR_T");
                    var oUserRoleBODC = (XIDBO)oUserRoleBOD.Clone(oUserRoleBOD);
                    oUserRoleBOD.iDataSource = NannoCoreDB.ID;
                    XID1Click oUsers = new XID1Click();
                    oUsers.BOID = 667;
                    oUsers.Query = "select * from XIAPPUsers_AU_T";
                    var oRes = oUsers.OneClick_Run();
                    if (oRes != null && oRes.Count() > 0)
                    {
                        foreach (var User in oRes.Values.ToList())
                        {
                            var iUserID = User.AttributeI(oUserBODC.sPrimaryKey).sValue;
                            User.BOD = oUserBODC;
                            User.SetAttribute(oUserBODC.sPrimaryKey, "");
                            User.SetAttribute("sCoreDatabaseName", "Nanno_Core");
                            User.Attributes.Values.ToList().ForEach(m => m.bDirty = true);
                            oCR = User.SaveV2(User);
                            oTrace.oTrace.Add(oCR.oTrace);
                            if (oCR.bOK && oCR.oResult != null)
                            {
                                var NewUser = (XIIBO)oCR.oResult;
                                var iNewUserID = NewUser.AttributeI(oUserBODC.sPrimaryKey).sValue;
                                Users[iUserID] = iNewUserID;
                            }
                            else
                            {
                                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                            }
                        }
                    }
                    if (oTrace.iStatus != (int)xiEnumSystem.xiFuncResult.xiError)
                    {
                        XID1Click oRoles = new XID1Click();
                        oRoles.BOID = 757;
                        oRoles.Query = "select * from XIAppRoles_AR_T";
                        var oAllRoles = oRoles.OneClick_Run();
                        if (oAllRoles != null && oAllRoles.Count() > 0)
                        {
                            foreach (var Role in oAllRoles.Values.ToList())
                            {
                                var iRoleID = Role.AttributeI(oRoleBODC.sPrimaryKey).sValue;
                                Role.BOD = oRoleBODC;
                                Role.SetAttribute(oRoleBODC.sPrimaryKey, "");
                                Role.Attributes.Values.ToList().ForEach(m => m.bDirty = true);
                                oCR = Role.SaveV2(Role);
                                oTrace.oTrace.Add(oCR.oTrace);
                                if (oCR.bOK && oCR.oResult != null)
                                {
                                    var NewRole = (XIIBO)oCR.oResult;
                                    Roles[iRoleID] = NewRole.AttributeI(oRoleBODC.sPrimaryKey).sValue;
                                }
                                else
                                {
                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                }
                            }
                        }
                    }
                    if (oTrace.iStatus != (int)xiEnumSystem.xiFuncResult.xiError)
                    {
                        XID1Click oUserRoles = new XID1Click();
                        oUserRoles.BOID = 758;
                        oUserRoles.Query = "select * from XIAppUserRoles_AUR_T";
                        var oAllUserRoles = oUserRoles.OneClick_Run();
                        if (oAllUserRoles != null && oAllUserRoles.Count() > 0)
                        {
                            foreach (var UserRole in oAllUserRoles.Values.ToList())
                            {
                                var UserID = UserRole.AttributeI("UserID").sValue;
                                var NewUserID = Users[UserID];
                                var RoleID = UserRole.AttributeI("RoleID").sValue;
                                var NewRoleID = Roles[RoleID];
                                UserRole.SetAttribute("UserID", NewUserID);
                                UserRole.SetAttribute("RoleID", NewRoleID);
                                UserRole.BOD = oUserRoleBODC;
                                UserRole.SetAttribute(oUserRoleBODC.sPrimaryKey, "");
                                UserRole.Attributes.Values.ToList().ForEach(m => m.bDirty = true);
                                oCR = UserRole.SaveV2(UserRole);
                                oTrace.oTrace.Add(oCR.oTrace);
                                if (oCR.bOK && oCR.oResult != null)
                                {

                                }
                                else
                                {
                                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                                }
                            }
                        }
                    }
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory Param: iApplicationID is missing";
                }
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        #endregion NannoApp
        public CResult Save_EnvironmentTemplate(XIEnvironmentTemplate oET)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            long iTraceLevel = 10;

            //get iTraceLevel from ??somewhere fast - cache against user??

            oCResult.sClassName = oCResult.Get_Class(); //AUTO-DERIVE
            oCResult.sFunctionName = System.Reflection.MethodBase.GetCurrentMethod().Name;

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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            //tracing
            // if tracing is on (which is a config switch for this user) then
            //   oCResult.Trace.Write
            //ALL OK?
            try
            {
                if (oSigCR != null)
                {
                    oSigCR.oStack.Add(new CNV { sName = "Stage", sValue = "Environment Template  Details insertion started for " + sBOName });
                }
                XIIBO oBOI = new XIIBO();
                XIDBO oBOD = new XIDBO();
                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, sBOName);
                oBOI.BOD = oBOD;
                oBOI.SetAttribute("ID", "0" );
                oBOI.SetAttribute("FKiTemplateID", oET.FKiTemplateID.ToString());
                oBOI.SetAttribute("iTemplateType", oET.iTemplateType.ToString());
                oBOI.SetAttribute("sTemplateType", oET.sTemplateType.ToString());
                oBOI.SetAttribute("sScript", oET.sScript.ToString());
                oCR = oBOI.Save(oBOI);
                if (oCR.bOK && oCR.oResult != null)
                {
                    if (oSigCR != null)
                    {
                        oSigCR.oStack.Add(new CNV { sName = "Success", sValue = "Environment Template Details insertion Success for " + sBOName });
                    }
                }
                else
                {
                    oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Environment Template Details insertion failed for " + sBOName });
                }
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                oCResult.oResult = oBOI;
            }
            catch (Exception ex)
            {
                oSigCR.oStack.Add(new CNV { sName = "Error", sValue = "Environment Template Details insertion failed" });
                oSigCR.oStack.Add(new CNV { sName = "Error Message", sValue = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n" });
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while Inserting Environment Template Details" });
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXIDef.SaveErrortoDB(oCResult);
            }
            return oCResult;
        }

    }
}