using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using XISystem;
using XIDatabase;
using XICore;
using static XIDatabase.XIDBAPI;
using System.Diagnostics;
using System.Data;
using System.Globalization;
using System.Configuration;

namespace XICore
{
    [Table("XIAPPUsers_AU_T")]
    public class XIInfraUsers : XIDefinitionBase
    {
        [Key]
        public int UserID { get; set; }
        public int FKiApplicationID { get; set; }
        public int FKiOrganisationID { get; set; }
        public string sUserName { get; set; }
        public string sPasswordHash { get; set; }
        public string sDatabaseName { get; set; }
        public string sCoreDatabaseName { get; set; }
        public string sAppName { get; set; }
        public string sLocation { get; set; }
        public string sPhoneNumber { get; set; }
        public string sEmail { get; set; }
        public string sFirstName { get; set; }
        public string sLastName { get; set; }
        public string sCol0 { get; set; }
        public string sRow1 { get; set; }
        public string sRow2 { get; set; }
        public string sRow3 { get; set; }
        public string sRow4 { get; set; }
        public string sRow5 { get; set; }
        public int iReportTo { get; set; }
        public int iPaginationCount { get; set; }
        public string sMenu { get; set; }
        public int iInboxRefreshTime { get; set; }
        public string SecurityStamp { get; set; }
        public bool EmailConfirmed { get; set; }
        public bool PhoneNumberConfirmed { get; set; }
        public bool TwoFactorEnabled { get; set; }
        public DateTime LockoutEndDateUtc { get; set; }
        public bool LockoutEnabled { get; set; }
        public int AccessFailedCount { get; set; }
        public string sTemporaryPasswordHash { get; set; }
        public string sAccessCode { get; set; }
        public string sHierarchy { get; set; }
        public string sInsertDefaultCode { get; set; }
        public string sUpdateHierarchy { get; set; }
        public string sViewHierarchy { get; set; }
        public string sDeleteHierarchy { get; set; }
        public int FKiTeamID { get; set; }
        public string sOTP { get; set; }
        public int iLevel { get; set; }
        [DapperIgnore]
        public string sUserHierarchy { get; set; }
        [DapperIgnore]
        public string sUserUpdateHierarchy { get; set; }
        [DapperIgnore]
        public string sUserViewHierarchy { get; set; }
        [DapperIgnore]
        public string sUserDeleteHierarchy { get; set; }
        [DapperIgnore]
        public string sTeamHierarchy { get; set; }
        [DapperIgnore]
        public string sPassword { get; set; }
        [DapperIgnore]
        public string sQuery { get; set; }
        public DateTime dtLastLogin { get { return DateTime.Now; } set { } }
        [DapperIgnore]
        public string bIsDisableClient { get; set; }
        [DapperIgnore]
        public bool bIsActive { get; set; }
        [DapperIgnore]
        public string CheckAccessCode { get; set; }
        [DapperIgnore]
        public string sCustomerRefNo { get; set; }
        [NotMapped]
        public XIDropDown oXIDrop { get; set; }
        [NotMapped]
        private XIInfraUserRoles oMyRoleID;
        [NotMapped]
        public XIInfraUserRoles RoleID
        {
            get
            {
                return oMyRoleID;
            }
            set
            {
                oMyRoleID = value;
            }
        }

        [NotMapped]
        private XIInfraRoles oMyRole;
        [NotMapped]
        public XIInfraRoles Role
        {
            get
            {
                return oMyRole;
            }
            set
            {
                oMyRole = value;
            }
        }
        [NotMapped]
        private XIInfraActorsMapping oMyActorID;
        [NotMapped]
        public XIInfraActorsMapping ActorID
        {
            get
            {
                return oMyActorID;
            }
            set
            {
                oMyActorID = value;
            }
        }
        [NotMapped]
        private XIInfraUserSetting oMySettings;
        [NotMapped]
        public XIInfraUserSetting Settings
        {
            get
            {
                return oMySettings;
            }
            set
            {
                oMySettings = value;
            }
        }

        [NotMapped]
        private XIInfraActors oMyActor;
        [NotMapped]
        public XIInfraActors Actor
        {
            get
            {
                return oMyActor;
            }
            set
            {
                oMyActor = value;
            }
        }

        public CResult Get_UserDetails(string sCoreDatabase, int iUserID = 0)
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
            oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            try
            {
                XIInfraUsers xifUser = null;
                cConnectionString oConString = new cConnectionString();
                string sConString = oConString.ConnectionString(sCoreDatabase);
                XIDBAPI Connection = new XIDBAPI(sConString);
                Dictionary<string, object> Params = new Dictionary<string, object>();
                if (!string.IsNullOrEmpty(sEmail))
                {
                    Params["sEmail"] = sEmail;
                }
                else if (UserID > 0)
                {
                    Params["UserID"] = UserID;
                }
                else if (!string.IsNullOrEmpty(sUserName))
                {
                    Params["sUserName"] = sUserName;
                }
                else if (iUserID > 0)
                {
                    Params["UserID"] = iUserID;
                }
                if (CheckAccessCode == "True")
                {
                    Params["sAccessCode"] = sAccessCode;
                }
                if (FKiOrganisationID > 0)
                {
                    Params["FKiOrganisationID"] = FKiOrganisationID;
                }
                if (Params.Count() == 0)
                {
                    oCResult.oResult = xifUser;
                    return oCResult;
                }
                xifUser = Connection.Select<XIInfraUsers>("XIAPPUsers_AU_T", Params).FirstOrDefault();
                if (xifUser != null)
                {
                    XIIXI oXI = new XIIXI();
                    XIInfraUserRoles xifURole = new XIInfraUserRoles();
                    xifURole.UserID = xifUser.UserID;
                    var oURoleData = xifURole.Get_UserRoles(sCoreDatabase);
                    if (oURoleData.bOK)
                    {
                        xifURole = ((List<XIInfraUserRoles>)oURoleData.oResult).FirstOrDefault();
                        xifUser.RoleID = xifURole;
                        if (xifURole.RoleID > 0)
                        {
                            XIInfraRoles xifRole = new XIInfraRoles();
                            xifRole.RoleID = xifURole.RoleID;
                            var oRoleData = xifRole.Get_RoleDefinition(sCoreDatabase);
                            if (oRoleData.bOK)
                            {
                                xifRole = (XIInfraRoles)oRoleData.oResult;
                                xifUser.Role = xifRole;
                            }
                        }
                        if (xifUser.Role.sRoleName.ToLower() == xiEnumSystem.EnumRoles.WebUsers.ToString().ToLower())
                        {
                            string Customer = string.Empty;
                            cConnectionString oSharedConString = new cConnectionString();
                            string sSharedConString = oSharedConString.ConnectionString(xifUser.sDatabaseName);
                            XIDBAPI SharedConnection = new XIDBAPI(sSharedConString);
                            //Dictionary<string, object> CustParams = new Dictionary<string, object>();
                            //CustParams["fkiuserid"] = xifUser.UserID;
                            List<CNV> oWhereParams = new List<CNV>();
                            oWhereParams.Add(new CNV { sName = "fkiuserid", sValue = Convert.ToString(xifUser.UserID) });
                            Dictionary<string, object> CustParams = new Dictionary<string, object>();
                            CustParams["sEmail"] = sUserName;
                            var bDisableClient = SharedConnection.SelectString("bDisableClientLogin", "Customer_T", CustParams);
                            if (!string.IsNullOrEmpty(bDisableClient))
                            {
                                if (bDisableClient == "True")
                                {
                                    xifUser.bIsDisableClient = "Deactive";
                                    xifUser.bIsActive = false;
                                }
                                else
                                {
                                    xifUser.bIsDisableClient = "Active";
                                    xifUser.bIsActive = true;
                                }
                            }
                            else
                            {
                                if (xifUser.StatusTypeID == 10)
                                {
                                    xifUser.bIsActive = true;
                                }
                                else
                                {
                                    xifUser.bIsActive = false;
                                }
                            }
                            xifUser.sCustomerRefNo = SharedConnection.SelectString("sReference", "Customer_T", CustParams);
                            //var oCustomer = oXI.BOI("customer_t", null, null, oWhereParams);
                            //if (oCustomer != null)
                            //{
                            //    if (oCustomer.Attributes.ContainsKey("bDisableClientLogin"))
                            //    {
                            //        Customer = oCustomer.AttributeI("bDisableClientLogin").sValue;
                            //    }
                            //    if (oCustomer.Attributes.ContainsKey("sReference"))
                            //    {
                            //        xifUser.sCustomerRefNo = oCustomer.AttributeI("sReference").sValue;
                            //    }
                            //}
                            //var Customer = SharedConnection.SelectString("iStatus", "customer_t", CustParams);//.Select<XIInfraUsers>("XIAPPUsers_AU_T", Params).FirstOrDefault();
                            //if (Customer == null)
                            //{
                            //    if (xifUser.StatusTypeID == 10)
                            //    {
                            //        xifUser.bIsActive = true;
                            //    }
                            //    else
                            //    {
                            //        xifUser.bIsActive = false;
                            //    }

                            //}
                            //else
                            //{
                            //    //int Custact = 1;
                            //    //if (int.TryParse(Customer, out Custact))
                            //    //{
                            //    xifUser.bIsActive = Customer == "True" ? false : true;
                            //    //}
                            //}
                        }
                        else
                        {
                            if (xifUser.StatusTypeID == 10 || xifUser.StatusTypeID == 0)
                            {
                                xifUser.bIsActive = true;
                            }
                            else
                            {
                                xifUser.bIsActive = false;
                            }
                        }
                    }
                    /* Hierarchy */
                    xifUser.sUserHierarchy = GetHierarchy(xifUser.sHierarchy);
                    xifUser.sUserUpdateHierarchy = GetHierarchy(xifUser.sUpdateHierarchy);
                    xifUser.sUserViewHierarchy = GetHierarchy(xifUser.sViewHierarchy);
                    xifUser.sUserDeleteHierarchy = GetHierarchy(xifUser.sDeleteHierarchy);
                    /* Team */
                    string sUserTeam = string.Empty;
                    if (xifUser.FKiTeamID > 0)
                    {
                        var TeamDetails = oXI.BOI("XITeams_T", xifUser.FKiTeamID.ToString());
                        var sTeamHierarchy = TeamDetails.AttributeI("sHierarchy").sValue;
                        var sTeamHierarchiesList = sTeamHierarchy.Contains('_') ? sTeamHierarchy.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>();
                        foreach (var itemTeamHierarchy in sTeamHierarchiesList)
                        {
                            int iValue = 0;
                            int.TryParse(itemTeamHierarchy, out iValue);
                            if (iValue == 0)
                            {
                                sUserTeam = string.IsNullOrEmpty(sUserTeam) ? itemTeamHierarchy : sUserTeam + "_" + itemTeamHierarchy;
                            }
                            else
                            {
                                TeamDetails = oXI.BOI("XITeams_T", itemTeamHierarchy);
                                sUserTeam = string.IsNullOrEmpty(sUserTeam) ? TeamDetails.AttributeI("sCode").sValue : sUserTeam + "_" + TeamDetails.AttributeI("sCode").sValue;
                            }
                        }
                        xifUser.sTeamHierarchy = sUserTeam;
                    }

                    XIInfraActorsMapping oXIActorMapping = new XIInfraActorsMapping();
                    oXIActorMapping.FKiUserID = xifUser.UserID;
                    var oUActorData = oXIActorMapping.Get_UserActors(sCoreDatabase);
                    if (oUActorData.bOK)
                    {
                        oXIActorMapping = ((List<XIInfraActorsMapping>)oUActorData.oResult).FirstOrDefault();
                        xifUser.ActorID = oXIActorMapping;
                        if (oXIActorMapping != null && oXIActorMapping.FKiActorID > 0)
                        {
                            XIInfraActors xifActor = new XIInfraActors();
                            xifActor.ID = oXIActorMapping.FKiActorID;
                            var oActorData = xifActor.Get_ActorDefinition(sCoreDatabase);
                            if (oActorData.bOK)
                            {
                                xifActor = (XIInfraActors)oActorData.oResult;
                                xifUser.Actor = xifActor;
                            }
                        }
                        else
                        {
                            xifUser.Actor = new XIInfraActors();
                            xifUser.ActorID = new XIInfraActorsMapping();
                        }
                    }
                    else
                    {
                        xifUser.Actor = new XIInfraActors();
                        xifUser.ActorID = new XIInfraActorsMapping();
                    }
                    XIInfraUserSetting oXIUserSetting = new XIInfraUserSetting();
                    oXIUserSetting.FKiUserID = xifUser.UserID;
                    var oUSettingData = oXIUserSetting.Get_UserSettings(sCoreDatabase);
                    if (oUSettingData.bOK)
                    {
                        oXIUserSetting = ((XIInfraUserSetting)oUSettingData.oResult);
                        xifUser.Settings = oXIUserSetting;
                    }
                    else
                    {
                        xifUser.Settings = new XIInfraUserSetting();
                    }
                    if (string.IsNullOrEmpty(xifUser.sAppName) && xifUser.FKiApplicationID > 0)
                    {
                        //List<CNV> oWhereParams = new List<CNV>();
                        //oWhereParams.Add(new CNV { sName = "id", sValue = xifUser.FKiApplicationID.ToString() });
                        Dictionary<string, object> CustParams = new Dictionary<string, object>();
                        CustParams["id"] = xifUser.FKiApplicationID.ToString();
                        XIDBAPI XIConnection = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString);
                        xifUser.sAppName = XIConnection.SelectString("sApplicationName", "XIApplication_T", CustParams);
                        //var oAppD = oXI.BOI("XI Application", null, null, oWhereParams);
                        //if (oAppD != null)
                        //{
                        //    if (oAppD.Attributes.ContainsKey("sApplicationName"))
                        //    {
                        //        xifUser.sAppName = oAppD.AttributeI("sApplicationName").sValue;
                        //    }
                        //}
                    }
                }
                oCResult.oResult = xifUser;
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oCResult; // always
        }
        public CResult Get_AllUserDetails(string sCoreDatabase, int iUserID = 0, string sUserName = null)
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
            oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            try
            {
                List<XIInfraUsers> xifUser = new List<XIInfraUsers>();
                cConnectionString oConString = new cConnectionString();
                string sConString = oConString.ConnectionString(sCoreDatabase);
                XIDBAPI Connection = new XIDBAPI(sConString);
                Dictionary<string, object> Params = new Dictionary<string, object>();
                //List<CNV> Params = new List<CNV>();
                if (UserID != 0)
                {
                    Params.Add("UserID", iUserID.ToString());
                }
                else
                {
                    Params.Add("sUserName", sUserName);
                }
                xifUser = Connection.Select<XIInfraUsers>("XIAPPUsers_AU_T", Params).ToList();
                oCResult.oResult = xifUser;
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oCResult; // always
        }

        public CResult Save_User(string sCoreDatabase)
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
                XIInfraUsers User = null;
                cConnectionString oConString = new cConnectionString();
                string sConString = oConString.ConnectionString(sCoreDatabase);
                XIDBAPI Connection = new XIDBAPI(sConString);
                User = Connection.Insert<XIInfraUsers>(this, "XIAPPUsers_AU_T", "UserID");
                oCResult.oResult = User;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oCResult; // always
        }

        public CResult Update_User(string sCoreDatabase)
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
                XIInfraUsers User = null;
                cConnectionString oConString = new cConnectionString();
                string sConString = oConString.ConnectionString(sCoreDatabase);
                XIDBAPI Connection = new XIDBAPI(sConString);
                this.LockoutEndDateUtc = DateTime.Now;
                User = Connection.Update<XIInfraUsers>(this, "XIAPPUsers_AU_T", "UserID");
                oCResult.oResult = User;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oCResult; // always
        }

        public CResult Get_NoOfUsers(string sCoreDatabase)
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
            oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            try
            {
                List<XIInfraUsers> xifUser = null;
                Dictionary<string, object> Params = new Dictionary<string, object>();
                Params["FKiOrganisationID"] = FKiOrganisationID;
                cConnectionString oConString = new cConnectionString();
                string sConString = oConString.ConnectionString(sCoreDatabase);
                XIDBAPI Connection = new XIDBAPI(sConString);
                xifUser = Connection.Select<XIInfraUsers>("XIAPPUsers_AU_T", Params).ToList();
                oCResult.oResult = xifUser.Count();
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
            }
            return oCResult; // always

            //return oURL;
        }

        //public XIInfraUsers UserDetails(string sCoreDatabase)
        //{
        //    XIInfraDbContext dbContext = new XIInfraDbContext(sCoreDatabase);
        //    XIInfraUsers oUser = null;
        //    if (!string.IsNullOrEmpty(sUserName))
        //    {
        //        oUser = dbContext.XIAppUsers.Where(m => m.sUserName == sUserName).FirstOrDefault();
        //    }
        //    else if (UserID > 0)
        //    {
        //        oUser = dbContext.XIAppUsers.Where(m => m.UserID == UserID).FirstOrDefault();
        //    }
        //    if (oUser != null)
        //    {
        //        oUser.RoleID = dbContext.XIAppUserRoles.Where(m => m.UserID == oUser.UserID).FirstOrDefault();
        //        if (oUser.RoleID != null)
        //        {
        //            oUser.Role = dbContext.XIAppRoles.Where(m => m.RoleID == oUser.RoleID.RoleID).FirstOrDefault();
        //        }
        //    }
        //    return oUser;
        //}        


        public CUserInfo Get_UserInfo(string sCoreDB = null)
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
            oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            CUserInfo oUInfo = new CUserInfo();
            int iUserID = 0;
            try
            {
                XIInfraUsers xifUser = null;
                if (HttpContext.Current != null && HttpContext.Current.Session != null && HttpContext.Current.Session["UserID"] != null)
                {

                }
                else
                {
                    if (!string.IsNullOrEmpty(sCoreDB))
                    {
                        oCR = Get_UserDetails(sCoreDB);
                        if (oCR.bOK && oCR.oResult != null)
                        {
                            xifUser = (XIInfraUsers)oCR.oResult;
                        }
                        if (xifUser != null && xifUser.UserID > 0)
                        {
                            oUInfo.sUserName = xifUser.sUserName;
                            oUInfo.sRoleName = xifUser.Role.sRoleName;
                            oUInfo.sEmail = xifUser.sEmail;
                            oUInfo.sName = xifUser.sFirstName + " " + xifUser.sLastName;
                            oUInfo.iUserID = xifUser.UserID;
                            oUInfo.AppName = xifUser.sAppName;
                            oUInfo.iOrganizationID = xifUser.FKiOrganisationID;
                            oUInfo.sOrgName = "Org";
                            oUInfo.sCoreDataBase = xifUser.sCoreDatabaseName;
                            oUInfo.iRoleID = xifUser.Role.RoleID;
                            oUInfo.bDBAccess = xifUser.Role.bDBAccess;
                            oUInfo.sHierarchy = xifUser.sHierarchy;
                            oUInfo.sInsertDefaultCode = xifUser.sInsertDefaultCode;
                            oUInfo.sViewHierarchy = xifUser.sViewHierarchy;
                            oUInfo.sDeleteHierarchy = xifUser.sDeleteHierarchy;
                            oUInfo.sUpdateHierarchy = xifUser.sUpdateHierarchy;
                            oUInfo.iApplicationID = xifUser.FKiApplicationID;
                            oUInfo.iLevel = xifUser.iLevel;
                            oUInfo.iActorID = xifUser.Actor.ID;
                            oUInfo.sActorName = xifUser.Actor.sName;
                            oUInfo.sWhereField = xifUser.Actor.sWhereField;
                            oUInfo.iWhereFieldValue = xifUser.ActorID.iInstanceID;
                        }
                    }
                    return oUInfo;
                }
                string sUserID = HttpContext.Current.Session["UserID"].ToString();
                sCoreDB = string.IsNullOrEmpty(sCoreDB) ? HttpContext.Current.Session["CoreDatabase"].ToString() : sCoreDB;
                int.TryParse(sUserID, out iUserID);
                if (iUserID > 0)
                {
                    oCR = Get_UserDetails(sCoreDB, iUserID);
                    if (oCR.bOK && oCR.oResult != null)
                    {
                        xifUser = (XIInfraUsers)oCR.oResult;
                    }
                    if (xifUser != null && xifUser.UserID > 0)
                    {
                        oUInfo.sUserName = xifUser.sUserName;
                        oUInfo.sRoleName = xifUser.Role.sRoleName;
                        oUInfo.sEmail = xifUser.sEmail;
                        oUInfo.sName = xifUser.sFirstName + " " + xifUser.sLastName;
                        oUInfo.iUserID = xifUser.UserID;
                        oUInfo.AppName = xifUser.sAppName;
                        oUInfo.iOrganizationID = xifUser.FKiOrganisationID;
                        oUInfo.sOrgName = "Org";
                        oUInfo.sCoreDataBase = xifUser.sCoreDatabaseName;
                        oUInfo.sDatabaseName = xifUser.sDatabaseName;
                        oUInfo.iRoleID = xifUser.Role.RoleID;
                        oUInfo.bDBAccess = xifUser.Role.bDBAccess;
                        oUInfo.sHierarchy = xifUser.sHierarchy;
                        oUInfo.sInsertDefaultCode = xifUser.sInsertDefaultCode;
                        oUInfo.sViewHierarchy = xifUser.sViewHierarchy;
                        oUInfo.sDeleteHierarchy = xifUser.sDeleteHierarchy;
                        oUInfo.sUpdateHierarchy = xifUser.sUpdateHierarchy;
                        oUInfo.iApplicationID = xifUser.FKiApplicationID;
                        if (xifUser.Actor != null)
                        {
                            oUInfo.iActorID = xifUser.Actor.ID;
                            oUInfo.sActorName = xifUser.Actor.sName;
                            oUInfo.sWhereField = xifUser.Actor.sWhereField;
                        }
                        if (xifUser.ActorID != null)
                            oUInfo.iWhereFieldValue = xifUser.ActorID.iInstanceID;
                        oUInfo.FKiTeamID = xifUser.FKiTeamID;
                    }
                    oCResult.oResult = oUInfo;
                }
            }
            catch (Exception ex)
            {
                StackTrace stackTrace = new StackTrace();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] iUserID - " + iUserID + " and Parent Method Name: " + stackTrace.GetFrame(1).GetMethod().Name + " - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oUInfo;
        }

        public List<XIDropDown> GetUserDetails()
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
            oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            List<XIDropDown> oUInfo = new List<XIDropDown>();
            int iUserID = 0;
            try
            {
                cConnectionString oSharedConString = new cConnectionString();
                string sSharedConString = oSharedConString.ConnectionString(sDatabaseName);
                XIDBAPI SharedConnection = new XIDBAPI(sSharedConString);
                var sTotalsQry = "";
                if (sQuery != null)
                {
                    sTotalsQry = sQuery;
                }
                var Totals = (DataTable)SharedConnection.ExecuteQuery(sTotalsQry);
                var totRows = Totals.AsEnumerable().ToList();
                var j = 0;
                foreach (var row in totRows)
                {
                    var sUserName = "";
                    var sLast = "";
                    var sFirst = row.ItemArray[0].ToString();
                    if (row.ItemArray.Count() > 1)
                    {
                        sLast = row.ItemArray[1].ToString();
                    }
                    if (sLast == "")
                    {
                        sUserName = sFirst;
                    }
                    else
                    {
                        sUserName = sFirst + " " + sLast;
                    }
                    if (sUserName != null)
                    {
                        XIDropDown oXD = new XIDropDown();
                        oXD.text = sUserName;
                        oXD.Expression = sUserName;
                        oUInfo.Add(oXD);
                    }
                }
            }
            catch (Exception ex)
            {
                StackTrace stackTrace = new StackTrace();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] iUserID - " + iUserID + " and Parent Method Name: " + stackTrace.GetFrame(1).GetMethod().Name + " - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oUInfo;
        }

        private string GetHierarchy(string sHierarchy)
        {
            string sResolvedHierarchy = string.Empty;
            XIIXI oXI = new XIIXI();
            if (!string.IsNullOrEmpty(sHierarchy))
            {
                int iHierarchy = 0;
                if (sHierarchy.Contains('|') || int.TryParse(sHierarchy, out iHierarchy))
                {
                    var Hierarchies = sHierarchy.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    foreach (var item in Hierarchies)
                    {
                        int iValue = 0;
                        int.TryParse(item, out iValue);
                        if (iValue == 0)
                        {
                            sResolvedHierarchy = string.IsNullOrEmpty(sResolvedHierarchy) ? item : sResolvedHierarchy + "_" + item;
                        }
                        else
                        {
                            var Hierarchy = oXI.BOI("XIHierarchy_T", item);
                            sHierarchy = Hierarchy.AttributeI("sHierarchy").sValue;
                            var HierarchiesList = sHierarchy.Contains('_') ? sHierarchy.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>();
                            if (iValue > 0 && HierarchiesList.Count() == 0)
                            {
                                HierarchiesList.Add(sHierarchy);
                            }
                            if (HierarchiesList.Count() > 0)
                            {
                                foreach (var itemHierarchy in HierarchiesList)
                                {
                                    Hierarchy = oXI.BOI("XIHierarchy_T", itemHierarchy);
                                    sResolvedHierarchy = string.IsNullOrEmpty(sResolvedHierarchy) ? Hierarchy.AttributeI("sCode").sValue :
                                        sResolvedHierarchy.EndsWith("|") ? sResolvedHierarchy + Hierarchy.AttributeI("sCode").sValue :
                                        sResolvedHierarchy + "_" + Hierarchy.AttributeI("sCode").sValue;
                                }
                                sResolvedHierarchy = sResolvedHierarchy + "|";
                            }
                            else
                            {
                                sResolvedHierarchy = sResolvedHierarchy + sHierarchy;
                            }
                        }
                    }
                }
                else if (sHierarchy.Contains('_'))
                {
                    var Hierarchies = sHierarchy.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                    foreach (var item in Hierarchies)
                    {
                        int iValue = 0;
                        int.TryParse(item, out iValue);
                        if (iValue == 0)
                        {
                            sResolvedHierarchy = string.IsNullOrEmpty(sResolvedHierarchy) ? item : sResolvedHierarchy + "_" + item;
                        }
                        else
                        {
                            var Hierarchy = oXI.BOI("XIHierarchy_T", item);
                            sHierarchy = Hierarchy.AttributeI("sHierarchy").sValue;
                            var HierarchiesList = sHierarchy.Split(new string[] { "_" }, StringSplitOptions.RemoveEmptyEntries).ToList();
                            foreach (var itemHierarchy in HierarchiesList)
                            {
                                Hierarchy = oXI.BOI("XIHierarchy_T", itemHierarchy);
                                sResolvedHierarchy = string.IsNullOrEmpty(sResolvedHierarchy) ? Hierarchy.AttributeI("sCode").sValue : sResolvedHierarchy + "_" + Hierarchy.AttributeI("sCode").sValue;
                            }
                            sResolvedHierarchy = sResolvedHierarchy + "|";
                        }
                    }
                }
            }
            return sResolvedHierarchy.EndsWith("|") ? sResolvedHierarchy.Substring(0, sResolvedHierarchy.Length - 1) : sResolvedHierarchy;
        }
    }

}