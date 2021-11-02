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
using System.Data.SqlClient;
using System.Configuration;
using Dapper;
using XISystem;
using XIDatabase;
using System.Data;
using System.Web;
using XIDataBase;
using MongoDB.Bson;

namespace XICore
{
    public class XIIXI : XIInstanceBase
    {
        public int iSwitchDataSrcID = 0;
        public CResult oCResult = new CResult();
        XIDBAPI Connection = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString);
        public XIIBO BOI(string sBODName = "", string sUID = "", string sGroupName = "", List<CNV> oWhrParams = null, bool bIsCopy = false)
        {
            CResult oCR = new CResult();
            XIIBO oBOI = new XIIBO();
            try
            {
                long iID = 0;
                int iOrgID = 0;
                int iAppID = 0;
                string sPKValue = string.Empty;

                if (long.TryParse(sUID, out iID))
                {

                }
                else
                {

                }
                CUserInfo oInfo = new CUserInfo();
                XIInfraUsers oUser = new XIInfraUsers();
                oInfo = oUser.Get_UserInfo();
                if (oInfo.iUserID == 0)
                {
                    if (HttpContext.Current != null && HttpContext.Current.Session != null)
                    {
                        var OrgID = HttpContext.Current.Session["OrganizationID"];
                        if (OrgID != null)
                            int.TryParse(OrgID.ToString(), out iOrgID);
                        var AppID = HttpContext.Current.Session["ApplicationID"];
                        if (AppID != null)
                            int.TryParse(AppID.ToString(), out iAppID);
                    }
                }
                else
                {
                    iOrgID = oInfo.iOrganizationID;
                    iAppID = oInfo.iApplicationID;
                }
                List<SqlParameter> SqlParams = new List<SqlParameter>();
                XIInfraCache oCache = new XIInfraCache();
                var oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, sBODName);
                var WhiteListCheck = System.Configuration.ConfigurationManager.AppSettings["WhitelistCheck"];
                if (WhiteListCheck == "yes")
                {
                    if (oBOD != null && oBOD.Name.ToLower() != "BO WhiteList".ToLower() && oBOD.BOID != 2459)
                    {
                        oCR = oBOI.Check_Whitelist(oBOD.BOID, oInfo.iRoleID, iOrgID, iAppID, "read", oBOD.iLevel, oInfo.iLevel);
                        if (oCR.bOK && oCR.oResult != null)
                        {
                            var bUNAuth = (bool)oCR.oResult;
                            if (bUNAuth)
                            {
                                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                                return new XIIBO();
                            }
                        }
                    }
                }
                var iDatasource = oBOD.iDataSource;
                XIDXI oXID = new XIDXI();
                var oDataSource = (XIDataSource)oCache.GetObjectFromCache(XIConstant.CacheDataSource, null, oBOD.iDataSource.ToString());
                if (!string.IsNullOrEmpty(oDataSource.sQueryType))
                {
                    if (oDataSource.sQueryType.ToLower() == "mssql")
                    {
                        oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Started BO Instance Loading" });
                        QueryEngine oQE = new QueryEngine();
                        List<XIWhereParams> oWParams = new List<XIWhereParams>();
                        if (!string.IsNullOrEmpty(sUID))
                        {
                            XIWhereParams oWP = new XIWhereParams();
                            oWP.sField = "PK";
                            oWP.sOperator = "=";
                            oWP.sValue = sUID;
                            oWParams.Add(oWP);
                            SqlParameter osqlparam = new SqlParameter();
                            if (oBOD.bUID)
                            {
                                osqlparam.ParameterName = "@XIGUID";
                            }
                            else
                            {
                                osqlparam.ParameterName = "@" + oBOD.sPrimaryKey;
                            }
                            osqlparam.Value = sUID;
                            SqlParams.Add(osqlparam);
                        }
                        if (oWhrParams != null && oWhrParams.Count() > 0)
                        {
                            oWParams.AddRange(oWhrParams.Select(m => new XIWhereParams { sField = m.sName, sValue = m.sValue, sOperator = "=" }));
                            foreach (var prm in oWhrParams)
                            {
                                SqlParameter osqlparam = new SqlParameter();
                                osqlparam.ParameterName = "@" + prm.sName;
                                osqlparam.Value = prm.sValue;
                                SqlParams.Add(osqlparam);
                            }
                        }

                        oQE.AddBO(sBODName, sGroupName, oWParams);
                        oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "BO definition added successfully to the QueryEngine" });
                        CResult oCresult = oQE.BuildQuery();
                        oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "BO Instance query build successfully" });
                        if (oCresult.bOK && oCresult.oResult != null)
                        {
                            var sSql = (string)oCresult.oResult;
                            ExecutionEngine oEE = new ExecutionEngine();
                            oInfo = oUser.Get_UserInfo();
                            if (oBOD.TableName.ToLower() == "Organizations".ToLower() || oBOD.TableName.ToLower() == "XIBOWhiteList_T".ToLower() || oBOD.TableName.ToLower() == "XIAppRoles_AR_T".ToLower())
                            {
                                if (oInfo.sCoreDataBase != null)
                                {
                                    var DataSource = oXID.Get_DataSourceDefinition(oInfo.sCoreDataBase);
                                    var BODS = ((XIDataSource)DataSource.oResult);
                                    oDataSource = (XIDataSource)oCache.GetObjectFromCache(XIConstant.CacheDataSource, null, BODS.ID.ToString());
                                    oEE.XIDataSource = oDataSource;
                                }
                                else
                                {
                                    oDataSource = (XIDataSource)oCache.GetObjectFromCache(XIConstant.CacheDataSource, null, iDatasource.ToString());
                                    oEE.XIDataSource = oDataSource;
                                }
                            }
                            else if ( oBOD.TableName == "RefTraceStage_T" || oBOD.TableName == "refValidTrace_T" || oBOD.TableName == "refLeadQuality_T" || oBOD.TableName == "TraceTransactions_T")
                            {                                
                                if (oInfo.sDatabaseName != null)
                                {
                                    var DataSource = oXID.Get_DataSourceDefinition(oInfo.sDatabaseName);
                                    var BODS = ((XIDataSource)DataSource.oResult);
                                    oDataSource = (XIDataSource)oCache.GetObjectFromCache(XIConstant.CacheDataSource, null, BODS.ID.ToString());
                                    oEE.XIDataSource = oDataSource;
                                }
                                else
                                {
                                    oDataSource = (XIDataSource)oCache.GetObjectFromCache(XIConstant.CacheDataSource, null, iDatasource.ToString());
                                    oEE.XIDataSource = oDataSource;
                                }
                            }
                            else if (iSwitchDataSrcID > 0)
                            {
                                oDataSource = (XIDataSource)oCache.GetObjectFromCache(XIConstant.CacheDataSource, null, iSwitchDataSrcID.ToString());
                                oEE.XIDataSource = oDataSource;
                            }
                            else
                            {
                                oEE.XIDataSource = oQE.XIDataSource;
                            }
                            oEE.sSQL = sSql;
                            oEE.SqlParams = SqlParams;
                            var oQResult = oEE.Execute();
                            if (oQResult.bOK && oQResult.oResult != null)
                            {
                                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "BO Instance query executed successfully" });
                                oBOI = ((Dictionary<string, XIIBO>)oQResult.oResult).Values.FirstOrDefault();
                                if (oBOI != null)
                                {
                                    oBOI.BOD = oQE.QParams.FirstOrDefault().BOD;
                                    //foreach (var oAttr in oBOI.Attributes)
                                    //{
                                    //    oAttr.Value.sDisplayName = oBOI.BOD.Attributes.Where(x => x.Key.ToLower() == oAttr.Key.ToLower()).Select(t => t.Value.LabelName).FirstOrDefault();
                                    //}
                                }
                            }
                        }
                    }
                    else if (oDataSource.sQueryType.ToLower() == "mongodb")
                    {
                        //XIDBMongoDB oMongoDB = new XIDBMongoDB();
                        //oMongoDB.sTable = oBOD.TableName;
                        //oMongoDB.sServer = oDataSource.sServer;
                        //oMongoDB.sDatabase = oDataSource.sDatabase;
                        //oMongoDB.sUID = sUID;
                        //oMongoDB.oWhrParams = oWhrParams;
                        //oMongoDB.sPrimaryKey = oBOD.sPrimaryKey;
                        //oCR = oMongoDB.Get_Data();
                        //if (oCR.bOK && oCR.oResult != null)
                        //{
                        //    var Data = ((List<BsonDocument>)oCR.oResult).FirstOrDefault();
                        //    var Attrs = Data.Names.ToList();
                        //    var Values = Data.Values.ToList();
                        //    oBOI = new XIIBO();
                        //    Dictionary<string, XIIAttribute> dictionary = new Dictionary<string, XIIAttribute>();

                        //    for (var i = 0; i < Attrs.Count(); i++)
                        //    {
                        //        dictionary[Attrs[i]] = new XIIAttribute() { sName = Attrs[i], sValue = Values[i].RawValue.ToString() };
                        //    }
                        //    oBOI.Attributes = dictionary;
                        //    oBOI.BOD = oBOD;
                        //}
                    }
                }
                //var BODD = oXID.Get_BODefinition("acpolicy_t");

                if (sBODName == "QS Instance")
                {
                    string iInsatnceID = oBOI.Attributes["id"].sValue;
                    XIIQS oQsInstance = new XIIQS();
                    if (!string.IsNullOrEmpty(iInsatnceID))
                    {
                        XIIXI oXII = new XIIXI();
                        XIDStructure oStructure = new XIDStructure();
                        //oQsInstance = oXII.GetQuestionSetInstanceByID(0, Convert.ToInt32(iInsatnceID));
                        oQsInstance = oXII.GetQSXIValuesByQSIID(Convert.ToInt32(iInsatnceID));
                        if (oQsInstance != null)
                        {
                            oBOI.XIIValues = oQsInstance.XIValues;
                            foreach (var item in oBOI.XIIValues)
                            {
                                oBOI.BOD.Attributes[item.Key.ToLower()] = new XIDAttribute { IsOptionList = item.Value.IsOptionList, OptionList = item.Value.FieldOptionList, iEnumBOD = item.Value.iEnumBOD };
                                if (!bIsCopy)
                                {
                                    oBOI.Attributes[item.Key.ToLower()] = new XIIAttribute { sName = item.Key.ToLower(), sValue = item.Value.sValue };
                                }
                            }
                        }
                    }
                }
                if (oBOI != null)
                {
                    if (oBOI.iBODID == 0 && oBOI.BOD != null)
                    {
                        oBOI.iBODID = oBOI.BOD.BOID;
                    }
                    oBOI.oParent = oBOI;
                    oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                }
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                SaveErrortoDB(oCResult);
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - BOName: " + sBODName + " - ID: " + sUID + " - Group Name: " + sGroupName;
                SaveErrortoDB(oCResult);
            }
            return oBOI;
        }

        public XIIQS QSI(string sUID, string sLoadSteps = "")
        {
            XIIQS oQSI = new XIIQS();
            long iID = 0;
            XIDQS oQSD = new XIDQS();

            // sLoadSteps - if "" then load all. otherwise for example:
            // Step1,Step3,Step9,Step10
            // load steps keyed by name (which must be unique and if it gets same name again just dont add in)
            // the steps are instances
            // KEY BY LOWER CASE ONLY

            // TO DO - get oQSD from the cache

            if (long.TryParse(sUID, out iID))
            {
            }
            else
            {
            }

            if (oQSI != null)
            {
                oQSI.oDefintion = oQSD;  // RAVI: if we reference it here, how will it load it again when it is in the cache? 

                oQSI.oParent = this;
            }

            return oQSI;
        }

        public CResult CreateQSI(string sQSName, int iQSID = 0, string sSteps = "", string sMode = "", int iBODID = 0, int iBOIID = 0, string sCurrentUserGUID = "", int FKiQSSourceID = 0, string sExternalRefID = "", int iOriginID = 0, string sGUID = null)
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
            //in the case of
            //xiEnumSystem.xiFuncResult.xiLogicalError
            oCResult.sMessage = "someone tried to do something they shouldnt";

            //tracing
            // if tracing is on (which is a config switch for this user) then
            //   oCResult.Trace.Write
            //ALL OK?

            try
            {
                XIIQS oQSI = null;
                long iID;

                // TO DO - if no steps specified it assumes all steps in the QS defintion

                // TO DO - insert this QS and return its UID 
                // TO DO - instance the specified steps and FK them to this instance
                if (!string.IsNullOrEmpty(sQSName) || iQSID > 0)
                {
                    var sSessionID = string.Empty;
                    if (HttpContext.Current == null)
                    {

                    }
                    else
                    {
                        sSessionID = HttpContext.Current.Session.SessionID;
                    }
                    XIInfraCache oCache = new XIInfraCache();
                    var oQSD = (XIDQS)oCache.GetObjectFromCache(XIConstant.CacheQuestionSet, null, iQSID.ToString(), sSessionID, sGUID);

                    XIIQS oQSIN = null;
                    //Dictionary<string, object> Params = new Dictionary<string, object>();
                    //Params["FKiQSDefinitionID"] = iQSID;
                    //Params["FKiUserCookieID"] = sCurrentUserGUID;
                    //oQSIN = Connection.Select<XIIQS>("XIQSInstance_T", Params).FirstOrDefault();

                    if (oQSIN == null && !oQSD.bInMemoryOnly)
                    {
                        oQSIN = new XIIQS();
                        oQSIN.FKiQSDefinitionID = iQSID;
                        oQSIN.FKiSourceID = FKiQSSourceID;
                        oQSIN.sExternalRefID = sExternalRefID;
                        oQSIN.FKiUserCookieID = sCurrentUserGUID;
                        oQSIN.sQSName = oQSD.sName;
                        oQSIN.iCurrentStepID = 0;
                        oQSIN.CreatedTime = DateTime.Now;
                        oQSIN.FKiClassID = oQSD.FKiClassID;
                        oQSIN.XIDeleted = 0;
                        oQSIN.FKiOriginID = iOriginID;
                        cConnectionString oConString = new cConnectionString();
                        //string sConString = oConString.ConnectionString(sCoreDatabase);
                        //XIDBAPI Connection = new XIDBAPI(sConString);
                        oQSIN = Connection.Insert<XIIQS>(oQSIN, "XIQSInstance_T", "ID");
                        oQSI = oQSIN;
                    }
                    else
                    {
                        oQSIN = new XIIQS();
                        oQSIN.FKiQSDefinitionID = iQSID;
                        oQSIN.FKiUserCookieID = sCurrentUserGUID;
                        oQSIN.sQSName = oQSD.sName;
                        oQSIN.iCurrentStepID = 0;
                        oQSI = oQSIN;

                        //SaveQSInstance();
                        //oQSI = GetQuestionSetInstanceByID(iQSID, oQSIN.ID, sMode, iBODID, iBOIID, sCurrentUserGUID);
                    }
                }
                oCResult.oResult = oQSI;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
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

        public XIIQS GetQSInstanceByID(int iQSIID)
        {
            XIIQS oQSIns = new XIIQS();
            Dictionary<string, object> StepParams = new Dictionary<string, object>();
            StepParams["ID"] = iQSIID;
            oQSIns = Connection.Select<XIIQS>("XIQSInstance_T", StepParams).FirstOrDefault();
            //oQSIns = dbContext.QSInstance.Find(iQSIID);
            return oQSIns;
        }

        public XIIQS GetQuestionSetInstanceByID_old(int iQSID, int iQSIID, string sMode, int iBODID, int iInstanceID, string sCurrentUserGUID)
        {
            XIIQS oQSInstance = new XIIQS();
            oQSInstance.FKiQSDefinitionID = iQSID;
            XIDXI oDXI = new XIDXI();
            //var oQSDefinition = oDXI.GetQuestionSetDefinitionByID(null, iQSID.ToString());
            //oQSInstance.QSDefinition = oQSDefinition;

            //Load QS Instance
            string sQSInstanceQry = "select * from XIQSInstance_T QSI " +
              "left join XIQSStepInstance_T QSSI on QSI.ID = QSSI.FKiQSInstanceID " +
              "left join XIQSStepDefinition_T QSSD on QSSI.FKiQSStepDefinitionID = QSSD.ID " +
              "left join XIFieldInstance_T FI on QSSI.FKiQSStepDefinitionID = FI.FKiQSStepDefinitionID " +
              "left join XIFieldDefinition_T FD on FD.ID = FI.FKiFieldDefinitionID " +
              "left join XIFieldOrigin_T FO on FO.ID = FD.FKiXIFieldOriginID " +
                "WHERE QSI.FKiQSDefinitionID = @id";

            var param = new
            {
                id = iQSID,
                CurrentGuestUser = sCurrentUserGUID,
                BODID = iBODID,
                BOIID = iInstanceID,
                QSIID = iQSIID
            };
            if (iQSIID > 0)
            {
                sQSInstanceQry = sQSInstanceQry + " and QSI.ID = @QSIID and FI.FKiQSInstanceID=@QSIID";
            }
            if (!string.IsNullOrEmpty(sCurrentUserGUID))
            {
                sQSInstanceQry = sQSInstanceQry + " and QSI.FKiUserCookieID = @CurrentGuestUser";
            }
            if (!string.IsNullOrEmpty(sMode) && iBODID > 0 && iInstanceID > 0)
            {
                if (sMode.ToLower() == "Popup".ToLower())
                {
                    sQSInstanceQry = sQSInstanceQry + " and QSI.FKiBODID = @BODID and QSI.iBOIID = @BOIID";
                }
            }
            var lookupQSIns = new Dictionary<int, XIIQS>();
            using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString))
            {
                Conn.Open();

                var lookupStepIns = new Dictionary<int, XIIQSStep>();
                var lookupFieldIns = new Dictionary<long, XIIValue>();
                Conn.Query<XIIQS, XIIQSStep, XIDQSStep, XIIValue, XIDFieldDefinition, XIDFieldOrigin, XIIQS>(sQSInstanceQry,
                    (QS, StepIns, StepD, FieldInstance, FieldD, FieldOrigin) =>
                    {
                        XIIQS oQSIns = new XIIQS();
                        if (QS != null)
                        {
                            if (!lookupQSIns.TryGetValue(QS.ID, out oQSIns))
                            {
                                lookupQSIns.Add(QS.ID, oQSIns = QS);
                            }
                            XIIQSStep oQSSIns;
                            if (StepIns != null)
                            {
                                if (!lookupStepIns.TryGetValue(StepIns.ID, out oQSSIns))
                                {
                                    lookupStepIns.Add(StepIns.ID, oQSSIns = StepIns);
                                    if (oQSIns.History == null)
                                    {
                                        oQSIns.History = new List<int>();
                                    }
                                    oQSIns.History.Add(StepD.ID);
                                    oQSIns.Steps[StepD.sName] = oQSSIns;
                                }
                                if (FieldInstance != null)
                                {
                                    if (FieldOrigin != null)
                                    {
                                        XIIValue oFVInstance;
                                        if (!lookupFieldIns.TryGetValue(FieldInstance.ID, out oFVInstance))
                                        {
                                            lookupFieldIns.Add(FieldInstance.ID, oFVInstance = FieldInstance);
                                            oQSSIns.XIValues[FieldOrigin.sName] = FieldInstance;
                                        }
                                    }
                                }
                            }
                        }
                        return oQSIns;
                    },
                    param
                    ).AsQueryable();
            }
            var oQSInsFinal = lookupQSIns.Values.FirstOrDefault();
            if (oQSInsFinal != null && oQSInsFinal.Steps != null && oQSInsFinal.Steps.Count() > 0)
            {
                oQSInstance.iCurrentStepID = oQSInsFinal.iCurrentStepID;
                oQSInstance.Steps = oQSInsFinal.Steps;
                var StepIns = GetSectionInstances(iQSID, iQSIID, sMode, iBODID, iInstanceID, sCurrentUserGUID).Steps;

                foreach (var items in StepIns)
                {
                    var oQSST = oQSInsFinal.Steps.Where(m => m.Value.FKiQSStepDefinitionID == items.Value.FKiQSStepDefinitionID).FirstOrDefault();
                    if (oQSST.Value != null)
                    {
                        oQSInsFinal.Steps.Where(m => m.Value.FKiQSStepDefinitionID == items.Value.FKiQSStepDefinitionID).FirstOrDefault().Value.Sections = items.Value.Sections;
                    }

                }
                oQSInsFinal.QSDefinition = oDXI.GetQuestionSetDefinitionByID(null, iQSID.ToString());
            }
            else
            {
                oQSInsFinal = new XIIQS();
                oQSInsFinal.QSDefinition = oDXI.GetQuestionSetDefinitionByID(null, iQSID.ToString());
                oQSInsFinal.FKiQSDefinitionID = oQSInsFinal.QSDefinition.ID;
                oQSInsFinal = oQSInsFinal.Save(oQSInsFinal, sCurrentUserGUID);
            }

            return oQSInsFinal;
        }

        private XIIQS GetSectionInstances(int iQSID, int iQSIID, string sMode, int iBODID, int iInstanceID, string sCurrentUserGUID)
        {

            XIIQS oQSInstance = new XIIQS();
            oQSInstance.FKiQSDefinitionID = iQSID;

            //Load QS Instance
            string sQSInstanceQry = "select * from XIQSInstance_T QSI " +
                "inner join XIQSStepInstance_T QSSI on QSI.ID = QSSI.FKiQSInstanceID " +
                "left join XIStepSectionInstance_T SecI on QSSI.ID = SecI.FKiStepInstanceID " +
                "left join XIFieldInstance_T SFI on SecI.FKiStepSectionDefinitionID = SFI.FKiQSSectionDefinitionID " +
                "left join XIFieldDefinition_T XIFD on SFI.FKiFieldDefinitionID = XIFD.ID " +
                "left join XIFieldOrigin_T XIO on XIFD.FKiXIFieldOriginID = XIO.ID " +
                //"left join XIFullAddress_T FA on SecI.ID = FA.FKiSectionInstanceID " +
                "WHERE (QSI.ID = SFI.FKiQSInstanceID or SFI.FKiQSInstanceID is null) and QSI.FKiQSDefinitionID = @id";

            var param = new
            {
                id = iQSID,
                CurrentGuestUser = sCurrentUserGUID,
                BODID = iBODID,
                BOIID = iInstanceID,
                QSIID = iQSIID
            };
            if (iQSIID > 0)
            {
                sQSInstanceQry = sQSInstanceQry + " and QSI.ID = @QSIID";
            }
            if (!string.IsNullOrEmpty(sCurrentUserGUID))
            {
                sQSInstanceQry = sQSInstanceQry + " and QSI.FKiUserCookieID = @CurrentGuestUser";
            }
            if (!string.IsNullOrEmpty(sMode) && iBODID > 0 && iInstanceID > 0)
            {
                if (sMode.ToLower() == "Popup".ToLower())
                {
                    sQSInstanceQry = sQSInstanceQry + " and QSI.FKiBODID = @BODID and QSI.iBOIID = @BOIID";
                }
            }
            var lookupQSIns = new Dictionary<int, XIIQS>();
            using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString))
            {
                Conn.Open();
                var lookupStepIns = new Dictionary<int, XIIQSStep>();
                var lookupSectionIns = new Dictionary<int, XIIQSSection>();
                var lookupFieldIns = new Dictionary<long, XIIValue>();
                Conn.Query<XIIQS, XIIQSStep, XIIQSSection, XIIValue, XIDFieldDefinition, XIDFieldOrigin, XIIQS>(sQSInstanceQry,
                    (QS, StepIns, SectionInstance, FieldInstance, FieldDefinition, FieldOrigin) =>
                    {
                        XIIQS oQSIns = new XIIQS();
                        if (QS != null)
                        {
                            if (!lookupQSIns.TryGetValue(QS.ID, out oQSIns))
                            {
                                lookupQSIns.Add(QS.ID, oQSIns = QS);
                            }
                            XIIQSStep oQSSIns;
                            if (!lookupStepIns.TryGetValue(StepIns.ID, out oQSSIns))
                            {
                                lookupStepIns.Add(StepIns.ID, oQSSIns = StepIns);
                                oQSIns.Steps[oQSSIns.ID.ToString()] = oQSSIns;
                            }

                            XIIQSSection oStepSectionIns;
                            if (SectionInstance != null)
                            {
                                if (!lookupSectionIns.TryGetValue(SectionInstance.ID, out oStepSectionIns))
                                {
                                    lookupSectionIns.Add(SectionInstance.ID, oStepSectionIns = SectionInstance);
                                    oQSSIns.Sections[oStepSectionIns.FKiStepSectionDefinitionID.ToString() + "_Sec"] = oStepSectionIns;
                                }

                                XIIValue oFVInstance;
                                if (FieldInstance != null)
                                {
                                    if (!lookupFieldIns.TryGetValue(FieldInstance.ID, out oFVInstance))
                                    {
                                        if (FieldOrigin != null)
                                        {
                                            lookupFieldIns.Add(FieldInstance.ID, oFVInstance = FieldInstance);
                                            oStepSectionIns.XIValues[FieldOrigin.sName] = FieldInstance;
                                        }
                                    }
                                }
                            }
                        }
                        return oQSIns;
                    },
                    param
                    ).AsQueryable();
            }
            var oQSInsFinal = lookupQSIns.Values.FirstOrDefault();

            return oQSInsFinal;
        }

        public string ConvertToDateTime(string InputString, string sFormatType)
        {
            try
            {
                //return String.Format("{0:" + InputString + "}", InputValue);
                CultureInfo provider = CultureInfo.InvariantCulture;
                string MyDate = "";
                // DateTime dt = DateTime.ParseExact(InputString, "dd-MMM-yyyy", CultureInfo.InvariantCulture);
                // string MyDate = dt.ToString("MM-dd-yyyy");
                string[] formats = {
                  "yyyy-MM-dd", "yyyy-MMM-dd", "yyyy.MM.dd","yyyy/MM/dd","yyyy/MMM/dd","yyyy.MMM.dd",
                  "dd-MM-yyyy","dd.MM.yyyy", "dd/MM/yyyy", "dd-MMM-yyyy", "dd.MMM.yyyy",
                  "MMM-dd-yyyy","MM-dd-yyyy", "MM.dd.yyyy", "MMM.dd.yyyy", "MM/dd/yyyy"
              };
                DateTime dateValue;
                // var dt = "26.May.1975";
                bool IsValidDate = DateTime.TryParseExact(InputString, formats, provider, DateTimeStyles.None, out dateValue);
                if (!IsValidDate)
                {
                    dateValue = DateTime.MinValue;
                    MyDate = null;
                }
                else
                {
                    if (!string.IsNullOrEmpty(sFormatType))
                    {
                        MyDate = dateValue.ToString(sFormatType);
                    }
                    else
                    {
                        MyDate = dateValue.Date.ToShortDateString();
                    }

                }
                return MyDate;
            }
            catch (Exception ex)
            {
                // ServiceUtil.LogError(ex);
                throw ex;
            }
        }
        public bool IsValidDate(string InputString)
        {
            CultureInfo provider = CultureInfo.InvariantCulture;
            string[] formats = {
                  "yyyy-MM-dd", "yyyy-MMM-dd", "yyyy.MM.dd","yyyy/MM/dd","yyyy/MMM/dd","yyyy.MMM.dd",
                  "dd-MM-yyyy","dd.MM.yyyy", "dd/MM/yyyy", "dd-MMM-yyyy", "dd.MMM.yyyy",
                  "MMM-dd-yyyy","MM-dd-yyyy", "MM.dd.yyyy", "MMM.dd.yyyy", "MM/dd/yyyy","M/dd/yyyy","MM/d/yyyy","M/dd/yyyy hh:mm:ss a"
              };
            DateTime dateValue;
            // var dt = "26.May.1975";
            bool IsValidDate = DateTime.TryParseExact(InputString, formats, provider, DateTimeStyles.None, out dateValue);
            return IsValidDate;
        }
        public static string ConvertToString(string InputString, string InputValue)
        {
            try
            {
                //DateTime dt = DateTime.ParseExact(InputValue, "dd-mm-yyyy hh:mm:ss", CultureInfo.InvariantCulture);
                DateTime dt = Convert.ToDateTime(InputValue);
                return dt.ToString("dd-MMM-yyyy");
            }
            catch (Exception ex)
            {
                // ServiceUtil.LogError(ex);
                throw ex;
            }
        }
        public XIIQS GetQuestionSetInstanceByID(int iQSDID = 0, int iQSIID = 0, string sMode = "", int iBODID = 0, int iBOIID = 0, string sCurrentUserGUID = "", int iParentQSIID = 0)
        {
            XIInfraCache oCache = new XIInfraCache();
            XIIQS oQSInstance = new XIIQS();
            oQSInstance.FKiQSDefinitionID = iQSDID;
            XIDXI oDXI = new XIDXI();
            XIDQS oQSD = new XIDQS();
            XIInfraEncryption oEncrypt = new XIInfraEncryption();
            if (iQSDID == 0 && iQSIID > 0)
            {
                var oQSI = GetQSInstanceByID(iQSIID);
                iQSDID = oQSI.FKiQSDefinitionID;
            }
            var oQSDefinition = (XIDQS)oCache.GetObjectFromCache(XIConstant.CacheQuestionSet, null, iQSDID.ToString());//oDXI.GetQuestionSetDefinitionByID(null, iQSID.ToString());
            var oQSDefinitionC = (XIDQS)oQSDefinition.Clone(oQSDefinition);
            oQSInstance.QSDefinition = oQSDefinitionC;
            try
            {
                //var param = new
                //{
                //    id = iQSDID,
                //    CurrentGuestUser = sCurrentUserGUID,
                //    BODID = iBODID,
                //    BOIID = iBOIID,
                //    QSIID = iQSIID
                //};
                Dictionary<string, object> Params = new Dictionary<string, object>();
                if (iQSDID > 0)
                {
                    Params["FKiQSDefinitionID"] = iQSDID;
                }
                if (iQSIID > 0)
                {
                    Params["ID"] = iQSIID;
                }
                if (!string.IsNullOrEmpty(sCurrentUserGUID))
                {
                    Params["FKiUserCookieID"] = sCurrentUserGUID;
                }
                if (!string.IsNullOrEmpty(sMode) && iBODID > 0 && iBOIID > 0)
                {
                    if (sMode.ToLower() == "Popup".ToLower())
                    {
                        Params["FKiBODID"] = iBODID;
                    }
                }
                XIDXI oXID = new XIDXI();
                oQSInstance = Connection.Select<XIIQS>("XIQSInstance_T ", Params).FirstOrDefault();
                if (oQSInstance != null)
                {
                    Dictionary<string, object> QSInsParams = new Dictionary<string, object>();
                    var oStepD = new XIDQSStep();
                    var QSFieldInstan = new List<XIIValue>();
                    var QSFieldOrigin = new XIDFieldOrigin();
                    if (iParentQSIID > 0) // Mostly It will be useful in Run-MTA Case
                    {
                        QSInsParams["FKiQSInstanceID"] = iParentQSIID;
                    }
                    else
                    {
                        QSInsParams["FKiQSInstanceID"] = oQSInstance.ID;
                    }

                    var QSStepInstan = Connection.Select<XIIQSStep>("XIQSStepInstance_T", QSInsParams).ToList();
                    foreach (var oStepI in QSStepInstan)
                    {
                        oStepD = oQSDefinition.Steps.Values.Where(m => m.ID == oStepI.FKiQSStepDefinitionID).FirstOrDefault();
                        Dictionary<string, object> StepSectionParams = new Dictionary<string, object>();
                        StepSectionParams["FKiStepInstanceID"] = oStepI.ID;
                        var QSSections = Connection.Select<XIIQSSection>("XIStepSectionInstance_T", StepSectionParams).ToList();
                        foreach (var oSectionI in QSSections)
                        {
                            Dictionary<string, object> FieldInsParams = new Dictionary<string, object>();
                            FieldInsParams["FKiSectionInstanceID"] = oSectionI.ID;
                            if (iParentQSIID > 0)
                            {
                                FieldInsParams["FKiQSInstanceID"] = iParentQSIID;
                            }
                            QSFieldInstan = Connection.Select<XIIValue>("XIFieldInstance_T", FieldInsParams).ToList();
                            foreach (var oFieldI in QSFieldInstan)
                            {
                                Dictionary<string, object> FieldDefParams = new Dictionary<string, object>();
                                FieldDefParams["ID"] = oFieldI.FKiFieldDefinitionID;
                                var oFieldD = Connection.Select<XIDFieldDefinition>("XIFieldDefinition_T", FieldDefParams).FirstOrDefault();
                                if (oFieldD != null)
                                {
                                    Dictionary<string, object> FieldOrgParams = new Dictionary<string, object>();
                                    FieldOrgParams["ID"] = oFieldD.FKiXIFieldOriginID;
                                    QSFieldOrigin = Connection.Select<XIDFieldOrigin>("XIFieldOrigin_T", FieldOrgParams).FirstOrDefault();
                                    if (QSFieldOrigin.bIsEncrypt && !string.IsNullOrEmpty(oFieldI.sValue))
                                    {
                                        oFieldI.sValue = oEncrypt.DecryptData(oFieldI.sValue, true, oFieldI.ID.ToString());
                                        oFieldI.sDerivedValue = oFieldI.sValue;
                                    }
                                    oFieldI.sResolvedValue = oFieldI.sValue;
                                    // oSectionI.XIValues[QSFieldOrigin.sName] = oFieldI;
                                    if (QSFieldOrigin.bIsOptionList || QSFieldOrigin.FK1ClickID > 0)
                                    {
                                        oFieldI.IsOptionList = QSFieldOrigin.bIsOptionList;
                                        QSFieldOrigin.FieldOptionList = oQSDefinition.XIDFieldOrigin.Where(x => x.Key.ToLower() == QSFieldOrigin.sName.ToLower()).Select(x => x.Value.FieldOptionList).FirstOrDefault();
                                        if (QSFieldOrigin.FieldOptionList != null)
                                        {
                                            oFieldI.sResolvedValue = QSFieldOrigin.FieldOptionList.Where(m => m.sOptionValue == oFieldI.sValue).Select(m => m.sOptionName).FirstOrDefault();
                                            oFieldI.sResolvedValueCode = QSFieldOrigin.FieldOptionList.Where(m => m.sOptionValue == oFieldI.sValue).Select(m => m.sOptionCode).FirstOrDefault();
                                        }
                                        //oFieldI.FieldOptionList = QSFieldOrigin.FieldOptionList.Select(m => new XIDOptionList
                                        //{
                                        //    ID = m.ID,
                                        //    sOptionName = m.sOptionName,
                                        //    sValues = m.sOptionValue
                                        //}).ToList();
                                        //oFieldI.iEnumBOD = QSFieldOrigin.FK1ClickID;
                                        //XIValues.sValue = FieldDefinition.Value.FieldOrigin.FieldOptionList.Where(m => m.sOptionValue == FieldInstance.Value.sValue).Select(m => m.sOptionName).FirstOrDefault();
                                    }
                                    else if (QSFieldOrigin.FKiBOID > 0)
                                    {
                                        var oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, null, QSFieldOrigin.FKiBOID.ToString());
                                        var GroupD = new XIDGroup();
                                        if (oBOD.Groups.TryGetValue("label", out GroupD))
                                        {
                                            XIDBAPI Myconntection = new XIDBAPI(oXID.GetBODataSource(oBOD.iDataSource, oBOD.FKiApplicationID));
                                            if (oBOD != null && oBOD.Groups.Any(group => group.Key.ToLower() == "label") && (oBOD.sSize == "30" || oBOD.sSize == "20") && !string.IsNullOrEmpty(oFieldI.sValue) && oFieldI.sValue != "0")
                                            {
                                                var BODParams = new Dictionary<string, object>();
                                                BODParams[oBOD.sPrimaryKey] = oFieldI.sValue;
                                                string FinalString = oBOD.GroupD("label").ConcatanateGroupFields(" ");//concatenate the string with join String 
                                                if (!string.IsNullOrEmpty(FinalString))
                                                {
                                                    var Result = Myconntection.Select<string>(oBOD.TableName, BODParams, FinalString + " As Result ").FirstOrDefault();
                                                    oFieldI.sResolvedValue = Result;
                                                }
                                            }
                                        }
                                    }
                                    oFieldI.sDisplayName = QSFieldOrigin.sDisplayName;
                                    oQSInstance.XIValues[QSFieldOrigin.sName] = oFieldI;
                                    oSectionI.XIValues[QSFieldOrigin.sName] = oFieldI;
                                }
                            }
                            oStepI.Sections[oSectionI.FKiStepSectionDefinitionID.ToString() + "_Sec"] = oSectionI;
                        }
                        if (oQSInstance.History == null)
                        {
                            oQSInstance.History = new List<int>();
                        }
                        oQSInstance.History.Add(oStepD.ID);
                        oQSInstance.Steps[oStepD.sName] = oStepI;
                    }
                }
                else
                {
                    oQSInstance = new XIIQS();
                    oQSInstance.QSDefinition = oQSDefinition;
                    oQSInstance.FKiQSDefinitionID = oQSInstance.QSDefinition.ID;
                    oQSInstance = oQSInstance.Save(oQSInstance, sCurrentUserGUID);
                }
                oQSInstance.QSDefinition = oQSDefinition;
                return oQSInstance;
            }
            catch (Exception ex)
            {
                var st = new StackTrace(ex, true);
                var frame = st.GetFrame(st.FrameCount - 1);
                var line = frame.GetFileLineNumber();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - Line Number:" + line + " Message: " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
                throw ex;
            }
        }

        public XIIQS GetXIValuesByQSIID(int iQSIID = 0)
        {
            XIIQS oQSInstance = new XIIQS();
            try
            {
                if (iQSIID > 0)
                {
                    var QSFieldInstan = new List<XIIValue>();
                    var QSFieldOrigin = new XIDFieldOrigin();
                    Dictionary<string, object> FieldInsParams = new Dictionary<string, object>();
                    FieldInsParams["FKiQSInstanceID"] = iQSIID;
                    QSFieldInstan = Connection.Select<XIIValue>("XIFieldInstance_T", FieldInsParams).ToList();
                    if (QSFieldInstan != null && QSFieldInstan.Count() > 0)
                    {
                        var XIValues = QSFieldInstan.ToDictionary(x => x.FKiFieldOriginID.ToString(), x => x);
                        oQSInstance.XIValues = XIValues;
                    }
                }
                return oQSInstance;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public XIIQS GetQSXIValuesByQSIID(int iQSIID)
        {
            XIIQS oQSInstance = new XIIQS();
            try
            {
                Dictionary<int, XIDBAPI> oDataSource = new Dictionary<int, XIDBAPI>();
                Dictionary<int, XIDBO> oBODList = new Dictionary<int, XIDBO>();
                XIInfraCache oCache = new XIInfraCache();
                XIDXI oXID = new XIDXI();

                //Load QS Instance
                string sQSInstanceQry = "select * from XIQSInstance_T QSI " +
                    "left join XIFieldInstance_T FI on QSI.ID = FI.FKiQSInstanceID " +
                    "left join XIFieldOrigin_T XIO on FI.FKiFieldOriginID = XIO.ID and FI." + XIConstant.Key_XIDeleted + " = 0";

                var param = new
                {
                    QSIID = iQSIID
                };
                if (iQSIID > 0)
                {
                    sQSInstanceQry = sQSInstanceQry + " where QSI.ID = @QSIID ";
                }
                sQSInstanceQry = sQSInstanceQry + " order by QSI.ID desc";
                var oQSI = GetQSInstanceByID(iQSIID);
                int iQSDID = oQSI.FKiQSDefinitionID;
                var oQSDefinition = (XIDQS)oCache.GetObjectFromCache(XIConstant.CacheQuestionSet, null, iQSDID.ToString());
                var lookupQSIns = new Dictionary<int, XIIQS>();
                using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString))
                {
                    Conn.Open();
                    var lookupFieldIns = new Dictionary<long, XIIValue>();
                    Conn.Query<XIIQS, XIIValue, XIDFieldOrigin, XIIQS>(sQSInstanceQry,
                        (QS, FieldInstance, FieldOrigin) =>
                        {
                            XIIQS oQSIns = new XIIQS();
                            if (QS != null)
                            {
                                if (!lookupQSIns.TryGetValue(QS.ID, out oQSIns))
                                {
                                    lookupQSIns.Add(QS.ID, oQSIns = QS);
                                }
                                XIIValue oXIValue;
                                if (FieldOrigin != null)
                                {
                                    if (!lookupFieldIns.TryGetValue(FieldInstance.ID, out oXIValue))
                                    {
                                        if (FieldOrigin.bIsEncrypt && !string.IsNullOrEmpty(FieldInstance.sValue))
                                        {
                                            XIInfraEncryption oEncrypt = new XIInfraEncryption();
                                            FieldInstance.sValue = oEncrypt.DecryptData(FieldInstance.sValue, true, FieldInstance.ID.ToString());
                                            FieldInstance.sDerivedValue = FieldInstance.sValue;
                                        }
                                        lookupFieldIns.Add(FieldInstance.ID, oXIValue = FieldInstance);
                                        FieldInstance.sResolvedValue = FieldInstance.sValue;

                                        if (FieldOrigin.bIsOptionList)
                                        {
                                            try
                                            {
                                                FieldInstance.IsOptionList = FieldOrigin.bIsOptionList;
                                                FieldOrigin.FieldOptionList = oQSDefinition.XIDFieldOrigin.Where(x => x.Key.ToLower() == FieldOrigin.sName.ToLower()).Select(x => x.Value.FieldOptionList).FirstOrDefault();
                                                if (FieldOrigin.FieldOptionList != null)
                                                {
                                                    FieldInstance.sResolvedValue = FieldOrigin.FieldOptionList.Where(m => m.sOptionValue == FieldInstance.sValue).Select(m => m.sOptionName).FirstOrDefault();
                                                    FieldInstance.sResolvedValueCode = FieldOrigin.FieldOptionList.Where(m => m.sOptionValue == FieldInstance.sValue).Select(m => m.sOptionCode).FirstOrDefault();
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                oCResult.sMessage = "Critical Error while loading optionlist QSInstance: " + iQSIID;
                                                SaveErrortoDB(oCResult);
                                            }

                                            //oFieldI.FieldOptionList = QSFieldOrigin.FieldOptionList.Select(m => new XIDOptionList
                                            //{
                                            //    ID = m.ID,
                                            //    sOptionName = m.sOptionName,
                                            //    sValues = m.sOptionValue
                                            //}).ToList();
                                            //oFieldI.iEnumBOD = QSFieldOrigin.FK1ClickID;
                                            //XIValues.sValue = FieldDefinition.Value.FieldOrigin.FieldOptionList.Where(m => m.sOptionValue == FieldInstance.Value.sValue).Select(m => m.sOptionName).FirstOrDefault();
                                        }
                                        else if (FieldOrigin.FKiBOID > 0)
                                        {
                                            try
                                            {
                                                XIDBO oBOD = new XIDBO();
                                                XIInfraCache oCahce = new XIInfraCache();
                                                if (oBODList.ContainsKey(FieldOrigin.FKiBOID))
                                                {
                                                    oBOD = oBODList[FieldOrigin.FKiBOID];
                                                }
                                                else
                                                {
                                                    oBOD = (XIDBO)oCahce.GetObjectFromCache(XIConstant.CacheBO, null, FieldOrigin.FKiBOID.ToString());
                                                    oBODList.Add(FieldOrigin.FKiBOID, oBOD);
                                                }
                                                var GroupD = new XIDGroup();
                                                if (oBOD.Groups.TryGetValue("label", out GroupD))
                                                {
                                                    XIDBAPI Myconntection = new XIDBAPI();
                                                    if (oDataSource.ContainsKey(oBOD.iDataSource))
                                                    {
                                                        Myconntection = oDataSource[oBOD.iDataSource];
                                                    }
                                                    else
                                                    {
                                                        Myconntection = new XIDBAPI(oXID.GetBODataSource(oBOD.iDataSource, oBOD.FKiApplicationID));
                                                        oDataSource.Add(oBOD.iDataSource, Myconntection);
                                                    }

                                                    if (oBOD != null && oBOD.Groups.Any(group => group.Key.ToLower() == "label") && (oBOD.sSize == "30" || oBOD.sSize == "20") && !string.IsNullOrEmpty(FieldInstance.sValue) && FieldInstance.sValue != "0")
                                                    {
                                                        var BODParams = new Dictionary<string, object>();
                                                        BODParams[oBOD.sPrimaryKey] = FieldInstance.sValue;
                                                        string FinalString = oBOD.GroupD("label").ConcatanateGroupFields(" ");//concatenate the string with join String 
                                                        if (!string.IsNullOrEmpty(FinalString))
                                                        {
                                                            var Result = Myconntection.Select<string>(oBOD.TableName, BODParams, FinalString + " As Result ").FirstOrDefault();
                                                            FieldInstance.sResolvedValue = Result;
                                                        }
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                oCResult.sMessage = "Critical Error while loading Label group data for QSInstance: " + iQSIID + " and BOID " + FieldOrigin.FKiBOID;
                                                SaveErrortoDB(oCResult);
                                            }
                                        }
                                        FieldInstance.sDisplayName = FieldOrigin.sDisplayName;
                                        oQSIns.XIValues[FieldOrigin.sName] = FieldInstance;
                                    }
                                }
                            }
                            return oQSIns;
                        },
                        param
                        ).AsQueryable();
                }
                oQSInstance = lookupQSIns.Values.FirstOrDefault();
                oQSInstance = oQSInstance == null ? new XIIQS() : oQSInstance;
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "Critical ERROR While loading QSInstance: " + iQSIID;
                var st = new StackTrace(ex, true);
                var frame = st.GetFrame(st.FrameCount - 1);
                var line = frame.GetFileLineNumber();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - Line Number:" + line + " Message: " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oQSInstance;
        }
        public string XIGetENVSetting(string sScript, List<CNV> oParams)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            var sResult = string.Empty;
            try
            {
                XIInfraCache oCache = new XIInfraCache();
                oTrace.oTrace.Add(oCR.oTrace);
                string sGUID = Guid.NewGuid().ToString();
                string sSessionID = HttpContext.Current.Session.SessionID;
                oCache.SetXIParams(oParams, sGUID, sSessionID);
                XIDScript oXIScript = new XIDScript();
                oXIScript.sScript = "xi.s|{" + sScript + "}";
                oCResult = oXIScript.Execute_Script(sGUID, sSessionID);
                if (oCResult.bOK && oCResult.oResult != null)
                {
                    sResult = (string)oCResult.oResult;
                }
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error While Updating User and customer data" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return sResult;
        }
    }
}