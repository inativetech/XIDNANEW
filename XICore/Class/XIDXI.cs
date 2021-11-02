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
using Dapper;
using System.Configuration;
using System.Web;
using XIDatabase;
using XISystem;
using System.Data;
using System.Text.RegularExpressions;
using xiEnumSystem;

namespace XICore
{
    public class XIDXI : XIDefinitionBase
    {
        public string sCoreDatabase { get; set; }
        public string sOrgDatabase { get; set; }
        public int iOrgID { get; set; }
        public string sAppName { get; set; }
        public string sUserID { get; set; }
        public string sTypeCyption { get; set; }
        public int FKiApplicationID { get; set; }
        Dictionary<int, string> DataSrcs = new Dictionary<int, string>();
        XIDBAPI Connection = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString);
        XIDBAPI XIEnvConnection = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIEnvironmentDbContext"].ConnectionString);
        #region BOMethods

        public CResult Get_BODefinition(string sBOName = "", string sUID = "")
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
                XIDBO oBOD = new XIDBO();
                long iID = 0;
                if (sBOName != "" || !string.IsNullOrEmpty(sUID))
                {
                    //Load BO Definition
                    oBOD = Load_BO(sBOName, sUID);
                }
                else if (long.TryParse(sUID, out iID))
                {
                }
                else
                {
                }

                if (oBOD != null)
                    oBOD.oParent = this;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                oCResult.oResult = oBOD;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while loading BO definition" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
            }
            return oCResult; // always

            //return oURL;
        }

        public CResult Get_BODefinitionALL(string sBOName = "", string sUID = "")
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
                XIDBO oBOD = new XIDBO();
                long iID = 0;
                if (!string.IsNullOrEmpty(sUID))
                {
                    if (long.TryParse(sUID, out iID))
                    {

                    }
                }
                if (!string.IsNullOrEmpty(sBOName) || !string.IsNullOrEmpty(sUID) && iID > 0)
                {
                    XIInfraCache oCache = new XIInfraCache();
                    //Load BO Definition
                    //oBOD = Load_BO(sBOName, sUID);
                    oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, sBOName, sUID);
                    var FKAttributes = oBOD.Attributes.Where(m => m.Value.FKiType > 0 && !string.IsNullOrEmpty(m.Value.sFKBOName)).ToList();
                    foreach (var item in FKAttributes)
                    {
                        var oFKBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, item.Value.sFKBOName);
                        string sBODataSource = string.Empty;
                        var sFKBOName = item.Value.sFKBOName;
                        Dictionary<string, object> Params = new Dictionary<string, object>();
                        Params["Name"] = sFKBOName;
                        if (oFKBOD != null && oFKBOD.FKiApplicationID > 0 && oFKBOD.TableName != "Organizations" && oFKBOD.TableName != "XIApplication_T" && oFKBOD.TableName != "XIAppRoles_AR_T" && oFKBOD.TableName != "XIWidget_T")
                        {
                            Params["FKiApplicationID"] = oFKBOD.FKiApplicationID.ToString();
                        }
                        string sSelectFields = string.Empty;
                        sSelectFields = "Name,BOID,iDataSource,sSize,TableName,sPrimaryKey,sType";
                        var FKBOD = Connection.Select<XIDBO>("XIBO_T_N", Params, sSelectFields).FirstOrDefault();
                        //var FKBOD = Load_BO(FKBO.Name, FKBO.BOID);
                        //var BO = AllBOs.Where(m => m.TableName == sTableName).FirstOrDefault();
                        CUserInfo oInfo = new CUserInfo();
                        XIInfraUsers oUser = new XIInfraUsers();
                        //if (SessionItems == null)
                        //{

                        //}
                        if (oFKBOD.TableName == "XIAPPUsers_AU_T" || oFKBOD.TableName == "XIAppRoles_AR_T" || oFKBOD.TableName == "Organizations")
                        {
                            oInfo = oUser.Get_UserInfo();
                            if (oInfo.sCoreDataBase != null)
                            {
                                var DataSource = Get_DataSourceDefinition(oInfo.sCoreDataBase);
                                var BODS = ((XIDataSource)DataSource.oResult);
                                sBODataSource = GetBODataSource(BODS.ID, oBOD.FKiApplicationID);
                            }
                            else
                            {
                                sBODataSource = GetBODataSource(FKBOD.iDataSource, oBOD.FKiApplicationID);
                                DataSrcs[FKBOD.iDataSource] = sBODataSource;
                            }
                        }
                        if (oFKBOD.TableName == "RefTraceStage_T" || oFKBOD.TableName == "refValidTrace_T" || oFKBOD.TableName == "refLeadQuality_T" || oFKBOD.TableName == "TraceTransactions_T")
                        {
                            oInfo = oUser.Get_UserInfo();
                            if (oInfo.sDatabaseName != null)
                            {
                                var DataSource = Get_DataSourceDefinition(oInfo.sDatabaseName);
                                var BODS = ((XIDataSource)DataSource.oResult);
                                sBODataSource = GetBODataSource(BODS.ID, oBOD.FKiApplicationID);
                            }
                            else
                            {
                                sBODataSource = GetBODataSource(FKBOD.iDataSource, oBOD.FKiApplicationID);
                                DataSrcs[FKBOD.iDataSource] = sBODataSource;
                            }
                        }
                        else if (DataSrcs.ContainsKey(FKBOD.iDataSource))
                        {
                            sBODataSource = DataSrcs[FKBOD.iDataSource];
                        }
                        else
                        {
                            sBODataSource = GetBODataSource(FKBOD.iDataSource, oBOD.FKiApplicationID);
                            DataSrcs[FKBOD.iDataSource] = sBODataSource;
                        }

                        oBOD.Attributes[item.Value.Name.ToLower()].sFKBOSize = FKBOD.sSize;
                        oBOD.Attributes[item.Value.Name.ToLower()].sFKBOName = FKBOD.Name;
                        //Get BO Default Popup
                        Dictionary<string, object> DefParams = new Dictionary<string, object>();
                        DefParams["FKiBOID"] = FKBOD.BOID;
                        DefParams["XIDeleted"] = "0";
                        var BODefaults = Connection.Select<XIDBODefault>("XIBOUIDefault_T", DefParams).FirstOrDefault();
                        if (BODefaults != null)
                        {
                            if (BODefaults.iPopupID > 0)
                            {
                                oBOD.Attributes[item.Value.Name.ToLower()].iDefaultPopupID = BODefaults.iPopupID;
                            }
                        }
                        if (FKBOD.sSize == "10")//maximum number of results in dropdown -- To Do
                        {
                            var Con = new XIDBAPI(sBODataSource);
                            if (item.Value.iOneClickID > 0)
                            {
                                string suid = "1click-" + Convert.ToString(item.Value.iOneClickID);
                                var oResult = Get_AutoCompleteList(suid, sBOName);
                                List<XIDropDown> FKDDL = new List<XIDropDown>();
                                if (oResult.bOK && oResult.oResult != null)
                                {
                                    if (FKBOD.sSize == "10")
                                    {
                                        var DDL = (List<XIDFieldOptionList>)oResult.oResult;
                                        FKDDL = DDL.Select(m => new XIDropDown { text = m.sOptionValue, Expression = m.sOptionName }).ToList();
                                    }
                                    else
                                    {
                                        var DDL = (Dictionary<string, string>)oResult.oResult;
                                        FKDDL = DDL.Select(m => new XIDropDown { text = m.Key, Expression = m.Value }).ToList();
                                    }

                                }
                                oBOD.Attributes[item.Value.Name.ToLower()].FieldDDL = FKDDL;
                            }
                            else if (FKBOD != null)
                            {
                                Dictionary<string, object> GrpParams = new Dictionary<string, object>();
                                GrpParams["BOID"] = FKBOD.BOID;
                                GrpParams["GroupName"] = "Label";
                                var FKBOLabelG = Connection.Select<XIDGroup>("XIBOGroup_T_N", GrpParams).FirstOrDefault();
                                var LabelGroup = FKBOLabelG.BOFieldNames;
                                if (!string.IsNullOrEmpty(LabelGroup))
                                {
                                    Dictionary<string, string> DDL = Con.SelectDDL(LabelGroup, FKBOD.TableName);
                                    var FKDDL = DDL.Select(m => new XIDropDown { text = m.Key, Expression = m.Value }).ToList();
                                    oBOD.Attributes[item.Value.Name.ToLower()].FieldDDL = FKDDL;
                                }
                            }
                        }
                        else if (FKBOD.sSize == "20")
                        {
                            //var Con = new XIDBAPI(sBODataSource);
                            //if (FKBOD != null)
                            //{
                            //    Dictionary<string, object> GrpParams = new Dictionary<string, object>();
                            //    GrpParams["BOID"] = FKBOD.BOID;
                            //    GrpParams["GroupName"] = "Label";
                            //    var FKBOLabelG = Connection.Select<XIDGroup>("XIBOGroup_T_N", GrpParams).FirstOrDefault();
                            //    var LabelGroup = FKBOLabelG.BOFieldNames;
                            //    if (!string.IsNullOrEmpty(LabelGroup))
                            //    {

                            //        string FinalString = FKBOLabelG.ConcatanateFields(LabelGroup, " ");
                            //        FinalString = FKBOD.sPrimaryKey + "," + FinalString;
                            //        Dictionary<string, string> DDL = Con.SelectDDL(FinalString, FKBOD.TableName);
                            //        var FKDDL = DDL.Select(m => new XIDropDown { text = m.Key, Expression = m.Value }).ToList();
                            //        oBOD.Attributes[item.Value.Name.ToLower()].FieldDDL = FKDDL;
                            //    }
                            //}
                        }

                    }
                    FKAttributes = oBOD.Attributes.Where(m => m.Value.bIsScript == true).ToList();
                    foreach (var item in FKAttributes)
                    {
                        Dictionary<string, object> DefParams = new Dictionary<string, object>();
                        DefParams["FKiBOID"] = item.Value.BOID;
                        var BODefaults = Connection.Select<XIDBODefault>("XIBOUIDefault_T", DefParams).FirstOrDefault();
                        if (BODefaults != null)
                        {
                            if (BODefaults.iPopupID > 0)
                            {
                                oBOD.Attributes[item.Value.Name.ToLower()].iDefaultPopupID = BODefaults.iPopupID;
                            }
                        }
                    }
                }
                else if (long.TryParse(sUID, out iID))
                {
                }
                else
                {
                }
                if (!string.IsNullOrEmpty(sUID) && iID > 0)
                {
                    oBOD.FKiApplicationID = oBOD.FKiApplicationID;
                }
                else
                {
                    oBOD.FKiApplicationID = FKiApplicationID;
                }
                if (oBOD != null)
                    oBOD.oParent = this;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                oCResult.oResult = oBOD;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while loading BO definition" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
            }
            return oCResult; // always

            //return oURL;
        }
        public List<XIDropDown> Get_ddlBOFieldAttributes(string iBOID = "")
        {
            List<XIDropDown> oXIDAttr = new List<XIDropDown>();
            Dictionary<string, object> Params = new Dictionary<string, object>();
            if (iBOID != "0")
            {
                Params["BOID"] = iBOID;
                var AllAttrs = Connection.Select<XIDAttribute>("XIBOAttribute_T_N", Params).ToList();
                oXIDAttr = AllAttrs.Select(m => new XIDropDown { text = m.Name, Value = m.ID }).ToList();
            }
            return oXIDAttr;
        }
        public List<XIDropDown> Get_BOAttributes(string iBOID = "")
        {
            List<XIDropDown> oXIDAttr = new List<XIDropDown>();
            Dictionary<string, object> Params = new Dictionary<string, object>();
            if (iBOID != "0")
            {
                Params["BOID"] = iBOID;
                var AllAttrs = Connection.Select<XIDAttribute>("XIBOAttribute_T_N", Params).ToList();
                oXIDAttr = AllAttrs.Select(m => new XIDropDown { text = m.Name, Value = m.ID }).ToList();
            }
            return oXIDAttr;
        }

        public XIDGroup Get_GroupDefinition(string iBOID, string sGroupName)
        {
            Dictionary<string, object> GrpParams = new Dictionary<string, object>();
            GrpParams["BOID"] = iBOID;
            GrpParams["GroupName"] = sGroupName;
            var oGroupD = Connection.Select<XIDGroup>("XIBOGroup_T_N", GrpParams).FirstOrDefault();
            return oGroupD;
        }

        public List<XIDropDown> Get_ddlBODataSourceDefinition(string iBOID = "")
        {
            List<XIDropDown> AllDataSources = new List<XIDropDown>();
            Dictionary<string, object> Params = new Dictionary<string, object>();
            List<XIDataSource> DataSourceDetails = new List<XIDataSource>();
            string sApplication = ConfigurationManager.AppSettings["AppName"];
            if (!string.IsNullOrEmpty(sApplication) && sApplication.ToLower() == "motorhome")
            {
                DataSourceDetails = Connection.Select<XIDataSource>("XIDataSource_XID_T", Params).ToList();
            }
            else
            {
                DataSourceDetails = XIEnvConnection.Select<XIDataSource>("XIDataSource_XID_T", Params).ToList();
            }
            //var DataSourceDetails = XIEnvConnection.Select<XIDataSource>("XIDataSource_XID_T", Params).ToList();
            foreach (var item in DataSourceDetails)
            {
                AllDataSources.Add(new XIDropDown
                {
                    text = item.sName,
                    Value = item.ID
                });
            }
            AllDataSources.Insert(0, new XIDropDown
            {
                text = "Organisation DB",
                Value = -2
            });
            AllDataSources.Insert(0, new XIDropDown
            {
                text = "Application DB",
                Value = -1
            });
            return AllDataSources;
        }

        private XIDBO Load_BO(string sBOName, string sUID)
        {
            try
            {
                long iID = 0;
                if (long.TryParse(sUID, out iID))
                {
                }
                else
                {
                }
                XIDBO oBOD = new XIDBO();
                Dictionary<string, object> Params = new Dictionary<string, object>();
                if (iID > 0)
                {
                    Params["BOID"] = iID;
                }
                else if (iID == 0 && !string.IsNullOrEmpty(sUID) && sUID != "0")
                {
                    Params["xiguid"] = sUID;
                }
                else if (!string.IsNullOrEmpty(sBOName))
                {
                    Params["Name"] = sBOName;
                }

                oBOD = Connection.Select<XIDBO>("XIBO_T_N ", Params).FirstOrDefault();
                if (oBOD != null)
                {
                    Dictionary<string, object> NVParams = new Dictionary<string, object>();
                    NVParams["BOID"] = oBOD.BOID;
                    var AttrButes = Connection.Select<XIDAttribute>("XIBOAttribute_T_N", NVParams).ToList();
                    var Groups = Connection.Select<XIDGroup>("XIBOGroup_T_N", NVParams).ToList();
                    Dictionary<string, object> ValuePirs = new Dictionary<string, object>();
                    ValuePirs["FKiBOID"] = oBOD.BOID;
                    var Scripts = Connection.Select<XIDScript>("XIBOScript_T", ValuePirs).ToList();
                    var BoUiDetails = Connection.Select<XIDBOUI>("XIBOUIDetails_T", ValuePirs).ToList();
                    var BoStructures = Connection.Select<XIDStructure>("XIBOStructure_T", NVParams).ToList();
                    Dictionary<string, object> ContentPairs = new Dictionary<string, object>();
                    ContentPairs["BO"] = oBOD.BOID;
                    var BOTemplets = Connection.Select<XIContentEditors>("XITemplate_T", ContentPairs).ToList();

                    if (Groups.Count > 0)
                    {
                        Groups.ForEach(m =>
                        {
                            oBOD.Groups[m.GroupName.ToLower()] = m;
                        });
                    }
                    if (Scripts.Count > 0)
                    {
                        Scripts.ForEach(m =>
                        {
                            oBOD.Scripts[m.sName.ToLower()] = m;
                        });
                    }
                    if (BoStructures.Count > 0)
                    {
                        BoStructures.ForEach(m =>
                        {
                            oBOD.Structures[m.sStructureName.ToLower()] = m;
                        });
                    }
                    if (BOTemplets.Count > 0)
                    {
                        BOTemplets.ForEach(m =>
                        {
                            oBOD.Templates[m.Name.ToLower()] = m;
                        });
                    }

                    if (AttrButes.Count > 0)
                    {
                        AttrButes.ForEach(m =>
                        {
                            if (m.IsOptionList)
                            {
                                Dictionary<string, object> AttrNvs = new Dictionary<string, object>();
                                AttrNvs["BOFieldID"] = m.ID;
                                //AttrNvs["StatusTypeID"] = "10";
                                var optionList = Connection.Select<XIDOptionList>("XIBOOptionList_T_N ", AttrNvs).ToList();
                                m.OptionList = optionList;
                                oBOD.Attributes[m.Name.ToLower()] = m;
                            }
                            else if (m.iMasterDataID > 0)
                            {
                                XIInfraCache oCache = new XIInfraCache();
                                var FKBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "Master Data", null);
                                var sBODataSource = GetBODataSource(FKBOD.iDataSource, FKBOD.FKiApplicationID);
                                var Con = new XIDBAPI(sBODataSource);
                                Dictionary<string, object> GrpParams = new Dictionary<string, object>();
                                GrpParams["BOID"] = FKBOD.BOID;
                                GrpParams["GroupName"] = "Label";
                                var FKBOLabelG = Connection.Select<XIDGroup>("XIBOGroup_T_N", GrpParams).FirstOrDefault();
                                var LabelGroup = FKBOLabelG.BOFieldNames;
                                if (!string.IsNullOrEmpty(LabelGroup))
                                {
                                    string sWhrcondition = "code=" + m.iMasterDataID;
                                    Dictionary<string, string> DDL = Con.SelectDDL(LabelGroup, FKBOD.TableName, sWhrcondition);
                                    var FKDDL = DDL.Select(n => new XIDOptionList { sOptionName = n.Value, sValues = n.Key }).ToList();
                                    m.OptionList = FKDDL;
                                    m.IsOptionList = true;
                                    oBOD.Attributes[m.Name.ToLower()] = m;
                                }
                            }
                            else
                            {
                                oBOD.Attributes[m.Name.ToLower()] = m;
                            }
                        });
                        //foreach (var oAttr in AttrButes)
                        //{
                        //    if (oAttr.TypeID == 150)
                        //    {
                        //        var oResult = SetDefaultDateRange(oAttr.sMinDate, oAttr.sMaxDate);
                        //        if (oResult != null && oResult.Count() > 0)
                        //        {
                        //            if (oResult.ContainsKey("sMinDate"))
                        //            {
                        //                oAttr.sMinDate = oResult["sMinDate"];
                        //            }
                        //            if (oResult.ContainsKey("sMaxDate"))
                        //            {
                        //                oAttr.sMaxDate = oResult["sMaxDate"];
                        //            }
                        //        }
                        //    }
                        //}
                    }
                    if (BoUiDetails != null)
                    {
                        oBOD.BOUID = BoUiDetails.FirstOrDefault();
                    }
                    if (oBOD.Scripts.Count() > 0)
                    {
                        Dictionary<string, object> ListAttributeScriptResults = new Dictionary<string, object>();
                        foreach (var item in oBOD.Scripts)
                        {
                            var ScriptResult = new List<XIDScriptResult>();
                            ListAttributeScriptResults["FKiScriptID"] = item.Value.ID;
                            //var resultforScrResult = Connection.Select<XIDScriptResult>("XIBOScriptResult_T", ListAttributeScriptResults).ToList();
                            ScriptResult.AddRange(Connection.Select<XIDScriptResult>("XIBOScriptResult_T", ListAttributeScriptResults).ToList());
                            if (ScriptResult != null && ScriptResult.Count() > 0)
                                oBOD.Scripts[item.Key.ToLower()].ScriptResults.AddRange(ScriptResult);
                        }
                    }
                }

                return oBOD;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }


        private XIDBO Load_BO_OLD(string sBOName, string sUID)
        {
            try
            {
                long iID = 0;
                if (long.TryParse(sUID, out iID))
                {
                }
                else
                {
                }
                XIDBO oBOD = new XIDBO();
                string sBODefinition = "select * from XIBO_T_N BOD " +
              "inner join XIBOAttribute_T_N Attr on BOD.BOID = Attr.BOID " +
              "left join XIBOGroup_T_N Grp on BOD.BOID = Grp.BOID " +
              "left join XIBOScript_T Scr on BOD.BOID = Scr.FKiBOID " +
              "left join XIBOUIDetails_T BOUI on BOD.BOID = BOUI.FKiBOID " +
              "left join XIBOStructure_T Strct on BOD.BOID = Strct.BOID " +
              "left join XIBOOptionList_T_N Opt on Attr.ID = Opt.BOFieldID " +
                "";

                if (iID > 0)
                {
                    sBODefinition = sBODefinition + "WHERE BOD.BOID = @iBODID";
                }
                else if (iID == 0 && !string.IsNullOrEmpty(sUID) && sUID != "0")
                {
                    sBODefinition = sBODefinition + "WHERE BOD.xiguid = @xiguid";
                }
                else if (!string.IsNullOrEmpty(sBOName))
                {
                    sBODefinition = sBODefinition + "WHERE BOD.Name = @Name";
                }
                var param = new
                {
                    Name = sBOName,
                    iBODID = iID,
                    xiguid = sUID
                };
                var lookupBOs = new Dictionary<int, XIDBO>();
                using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString))
                {
                    Conn.Open();
                    var lookupAttrs = new Dictionary<int, XIDAttribute>();
                    var lookupGroups = new Dictionary<int, XIDGroup>();
                    var lookupScripts = new Dictionary<long, XIDScript>();
                    var lookupStructs = new Dictionary<long, XIDStructure>();
                    var lookupOpts = new Dictionary<int, XIDOptionList>();
                    Conn.Query<XIDBO, XIDAttribute, XIDGroup, XIDScript, XIDBOUI, XIDStructure, XIDOptionList, XIDBO>(sBODefinition,
                                        (oBO, oAttribure, oGroup, oScript, oBOUI, oStructure, oOptionList) =>
                                        {
                                            oBOD = new XIDBO();
                                            if (oBO != null)
                                            {
                                                if (!lookupBOs.TryGetValue(oBO.BOID, out oBOD))
                                                {
                                                    lookupBOs.Add(oBO.BOID, oBOD = oBO);
                                                }
                                                XIDAttribute oAttr;
                                                if (oAttribure != null)
                                                {
                                                    if (!lookupAttrs.TryGetValue(oAttribure.ID, out oAttr))
                                                    {
                                                        lookupAttrs.Add(oAttribure.ID, oAttr = oAttribure);
                                                        oBOD.Attributes[oAttr.Name.ToLower()] = oAttr;
                                                    }
                                                    XIDOptionList oOpt;
                                                    if (oOptionList != null)
                                                    {
                                                        if (!lookupOpts.TryGetValue(oOptionList.ID, out oOpt))
                                                        {
                                                            lookupOpts.Add(oOptionList.ID, oOpt = oOptionList);
                                                            if (oAttr.OptionList == null)
                                                            {
                                                                oAttr.OptionList = new List<XIDOptionList>();
                                                                oAttr.OptionList.Add(oOpt);
                                                            }
                                                            else
                                                            {
                                                                oAttr.OptionList.Add(oOpt);
                                                            }
                                                        }
                                                    }
                                                }
                                                if (oGroup != null)
                                                {
                                                    XIDGroup oGrp;
                                                    if (!lookupGroups.TryGetValue(oGroup.ID, out oGrp))
                                                    {
                                                        lookupGroups.Add(oGroup.ID, oGrp = oGroup);
                                                        oBOD.Groups[oGrp.GroupName.ToLower()] = oGrp;
                                                    }
                                                }
                                                if (oScript != null)
                                                {
                                                    XIDScript oSrpt;
                                                    if (!lookupScripts.TryGetValue(oScript.ID, out oSrpt))
                                                    {
                                                        lookupScripts.Add(oScript.ID, oSrpt = oScript);
                                                        oBOD.Scripts[oSrpt.sName.ToLower()] = oSrpt;
                                                    }
                                                }
                                                if (oBOUI != null)
                                                {
                                                    oBOD.BOUID = oBOUI;
                                                }
                                                if (oStructure != null)
                                                {
                                                    XIDStructure oStrct;
                                                    if (!lookupStructs.TryGetValue(oStructure.ID, out oStrct))
                                                    {
                                                        lookupStructs.Add(oStructure.ID, oStrct = oStructure);
                                                        oBOD.Structures[oStructure.sStructureName.ToLower()] = oStrct;
                                                    }
                                                }

                                            }
                                            return oBOD;
                                        },
                                        param
                                        ).AsQueryable();
                }
                oBOD = lookupBOs.Values.FirstOrDefault();
                if (oBOD.Scripts.Count() > 0)
                {
                    Dictionary<string, object> ListAttributeScriptResults = new Dictionary<string, object>();
                    foreach (var item in oBOD.Scripts)
                    {
                        var ScriptResult = new List<XIDScriptResult>();
                        ListAttributeScriptResults["FKiScriptID"] = item.Value.ID;
                        //var resultforScrResult = Connection.Select<XIDScriptResult>("XIBOScriptResult_T", ListAttributeScriptResults).ToList();
                        ScriptResult.AddRange(Connection.Select<XIDScriptResult>("XIBOScriptResult_T", ListAttributeScriptResults).ToList());
                        if (ScriptResult != null && ScriptResult.Count() > 0)
                            oBOD.Scripts[item.Key].ScriptResults.AddRange(ScriptResult);
                    }
                }
                return oBOD;
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }

        public string GetBODataSource(int iDataSourceID, int iApplicationID, string sType = "")
        {
            cConnectionString oConString = new cConnectionString();
            var sBODataSource = string.Empty;
            if (iDataSourceID != 0)
            {
                if (iDataSourceID == -1)
                {
                    sBODataSource = oConString.ConnectionString(sCoreDatabase);
                }
                else if (iDataSourceID == -2)
                {
                    sBODataSource = oConString.ConnectionString(sOrgDatabase);
                }
                else
                {
                    XIInfraCache oCache = new XIInfraCache();
                    var SrcID = iDataSourceID.ToString();
                    XIEncryption oEncrypt = new XIEncryption();
                    if (!string.IsNullOrEmpty(sType))
                    {
                        SrcID = sType + "-" + SrcID;
                    }
                    var DataSource = (XIDataSource)oCache.GetObjectFromCache(XIConstant.CacheDataSource, null, SrcID.ToString(), null, null, iApplicationID); //Connection.Select<XIDataSource>("XIDataSource_XID_T", Params).FirstOrDefault();// XIContext.XIDataSources.Find(SrcID);
                    sBODataSource = oEncrypt.DecryptData(DataSource.sConnectionString, true, DataSource.ID.ToString());
                    //sBODataSource = DataSource.sConnectionString;
                }
            }
            else
            {
                if (iOrgID > 0)
                {
                    sBODataSource = oConString.ConnectionString(sOrgDatabase);
                }
                else
                {
                    sBODataSource = oConString.ConnectionString(sCoreDatabase);
                }
            }
            return sBODataSource;
        }

        public CResult RunBOGUID(XIDBOGUID xiBOGUID)
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
                bool bIsSuccess = Connection.ExecuteSP(xiBOGUID.sTableName, xiBOGUID.bIsChangeFK);
                oCResult.oResult = bIsSuccess;
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


        public CResult Get_BOsDDL()
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
                //Dictionary<string, string> DDL = Connection.SelectDDL("name, tablename", "XIBO_T_N");
                //var BOsDDL = DDL.Select(m => new XIDropDown { text = m.Key, Expression = m.Value }).ToList();

                Dictionary<string, string> DDL = Connection.SelectDDL("ID, Name", "XIBO_T_N");
                var BOsDDL = DDL.Select(m => new XIDropDown { text = m.Value, Value = Convert.ToInt32(m.Key) }).ToList();

                oCResult.oResult = BOsDDL;
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

        public CResult Get_DataSourcesDDL()
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
                Dictionary<string, string> DDL = Connection.SelectDDL("ID, sName", "XIBO_T");
                var DataSourceDDL = DDL.Select(m => new XIDropDown { Value = Convert.ToInt32(m.Key), text = m.Value }).ToList();
                DataSourceDDL.Insert(0, new XIDropDown
                {
                    text = "Organisation DB",
                    Value = -2
                });
                DataSourceDDL.Insert(0, new XIDropDown
                {
                    text = "Application DB",
                    Value = -1
                });
                oCResult.oResult = DataSourceDDL;
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

        //Get the Attributes name related to BO
        public XIDBO CopyBOByID(int ID, string sDatabase)
        {
            XIDBO oBOD = new XIDBO();
            XIInfraCache oCache = new XIInfraCache();
            string sCol_Name = "";
            string Data_Type = "";
            string sMax_Length = "";
            string sIS_NULL = "";
            string Col_Details = "";
            var lCmptCol = new List<string>();
            var sColDetails = new List<string>();
            oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, null, ID.ToString());
            var sBODataSource = GetBODataSource(oBOD.iDataSource, oBOD.FKiApplicationID);
            var sNewCol_Details = "";
            using (SqlConnection Con = new SqlConnection(sBODataSource))
            {
                Con.Open();
                SqlCommand SqlCmd = new SqlCommand();
                SqlCmd.Connection = Con;
                SqlCmd.CommandText = "SELECT COLUMN_NAME,DATA_TYPE,CHARACTER_MAXIMUM_LENGTH,IS_NULLABLE FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME= '" + oBOD.TableName + "'";
                SqlDataReader DReader = SqlCmd.ExecuteReader();
                while (DReader.Read())
                {
                    sCol_Name = DReader.IsDBNull(0) ? null : DReader.GetValue(0).ToString();
                    Data_Type = DReader.IsDBNull(1) ? null : DReader.GetValue(1).ToString();
                    sMax_Length = DReader.IsDBNull(2) ? null : DReader.GetValue(2).ToString();
                    sIS_NULL = DReader.IsDBNull(3) ? null : DReader.GetValue(3).ToString();

                    string sTwoLtrOfColumn = sCol_Name.Substring(0, 2);
                    string sNewDatatype = "";
                    string sNewColDetails = "";
                    if (sTwoLtrOfColumn == "CM")
                    {
                        string sGetCmptdColDef = "SELECT definition FROM sys.computed_columns WHERE [name] = '" + sCol_Name + "' AND [object_id] = OBJECT_ID('" + oBOD.TableName + "')";
                        //Conn.Open();
                        SqlCommand SqlcmdColDef = new SqlCommand(sGetCmptdColDef, Con);
                        SqlDataReader DReaderChckDtype = SqlcmdColDef.ExecuteReader();
                        var sDefinition = "";
                        while (DReaderChckDtype.Read())
                        {
                            sDefinition = DReaderChckDtype.IsDBNull(0) ? null : DReaderChckDtype.GetValue(0).ToString();
                        }
                        //Conn.Close();
                        int iDefnLength = sDefinition.Length;
                        var sNewDefinition = sDefinition.Substring(1, iDefnLength - 2);
                        string sGetColumnUsed = String.Join(",", Regex.Matches(sNewDefinition, @"\[(.+?)\]").Cast<Match>().Select(m => m.Groups[1].Value));
                        var lColumnList = new List<string>();
                        var lColumnExists = new List<string>();
                        lColumnList = sGetColumnUsed.Split(',').ToList();
                        string sColumns = "";
                        foreach (var ColNmes in lColumnList)
                        {
                            string sCheckColmn = @"IF NOT EXISTS(SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_CATALOG='" + sDatabase + "' AND TABLE_NAME='" + oBOD.TableName + "' AND COLUMN_NAME ='" + ColNmes + "') SELECT 0 ELSE SELECT 1";
                            // Conn.Open();
                            SqlCommand Sqlcmd = new SqlCommand(sCheckColmn, Con);
                            int iColExist = Convert.ToInt32(Sqlcmd.ExecuteScalar());
                            // Conn.Close();
                            if (iColExist == 1)
                            {
                                lColumnExists.Add(ColNmes);
                            }
                            sColumns = string.Join(",", lColumnExists.ToArray());
                        }
                        //Col_Details =Col_Details+ sCol_Name+",{"+ sColumns + "} AS " + sNewDefinition + "\n";
                        //take as a sperate string.
                        lCmptCol.Add(sCol_Name + ",{" + sColumns + "} AS " + sNewDefinition + "\n");
                    }
                    else if (sTwoLtrOfColumn == "FK")
                    {
                        var PK_Table = "";
                        var PK_ColName = "";
                        //get the details on foreign key here add extra PKcolumn name and table name 
                        //SqlConnection TempConn = new SqlConnection(ConfigurationManager.ConnectionStrings["XIDNAClientDbContext"].ConnectionString);

                        SqlCommand SqlCmdTemp = new SqlCommand();
                        SqlCmdTemp.Connection = Con;
                        SqlCmdTemp.CommandText = "SELECT FK_Table = FK.TABLE_NAME,FK_Column = CU.COLUMN_NAME, PK_Table = PK.TABLE_NAME, PK_Column = PT.COLUMN_NAME, Constraint_Name = C.CONSTRAINT_NAME FROM INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS C INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS FK ON C.CONSTRAINT_NAME = FK.CONSTRAINT_NAME INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS PK ON C.UNIQUE_CONSTRAINT_NAME = PK.CONSTRAINT_NAME INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE CU ON C.CONSTRAINT_NAME = CU.CONSTRAINT_NAME INNER JOIN (SELECT i1.TABLE_NAME, i2.COLUMN_NAME FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS i1 INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE i2 ON i1.CONSTRAINT_NAME = i2.CONSTRAINT_NAME WHERE i1.CONSTRAINT_TYPE = 'PRIMARY KEY') PT ON PT.TABLE_NAME = PK.TABLE_NAME WHERE FK.TABLE_NAME= '" + oBOD.TableName + "' AND CU.COLUMN_Name = '" + sCol_Name + "'";
                        SqlDataReader DReaderTemp = SqlCmdTemp.ExecuteReader();
                        while (DReaderTemp.Read())
                        {
                            PK_Table = DReaderTemp.IsDBNull(2) ? null : DReaderTemp.GetValue(2).ToString();
                            PK_ColName = DReaderTemp.IsDBNull(3) ? null : DReaderTemp.GetValue(3).ToString();
                        }
                        string NewNullVal = "NOT NULL";
                        if (sIS_NULL == "NO" || sIS_NULL == "no")
                        {
                            NewNullVal = "NOT NULL";
                        }
                        else
                        {
                            NewNullVal = "NULL";
                        }
                        if (PK_Table == "" && PK_ColName == "")
                        {
                            if (sMax_Length == "NULL")
                            {
                                sNewColDetails = sCol_Name + "," + Data_Type + "," + NewNullVal;
                            }
                            else
                            {
                                sNewColDetails = sCol_Name + "," + Data_Type + "(" + sMax_Length + ")," + NewNullVal;
                            }
                        }
                        else
                        {
                            if (sMax_Length == "NULL")
                            {
                                // sNewColDetails = sCol_Name + "," + Data_Type + "," + NewNullVal + "," + PK_Table + "," + PK_ColName;
                                sNewColDetails = sCol_Name + "," + PK_Table + "," + PK_ColName;
                            }
                            else
                            {
                                //sNewColDetails = sCol_Name + "," + Data_Type + "(" + sMax_Length + ")," + NewNullVal + "," + PK_Table + "," + PK_ColName;
                                sNewColDetails = sCol_Name + "," + PK_Table + "," + PK_ColName;
                            }
                        }
                        sColDetails.Add(sNewColDetails);
                        if (Col_Details == "")
                        {
                            Col_Details = sNewColDetails + "\n";
                        }
                        else
                        {
                            Col_Details = Col_Details + sNewColDetails + "\n";
                        }
                    }
                    else
                    {
                        if (Data_Type == "int")
                        {
                            sNewDatatype = "i";

                        }
                        else if (Data_Type == "varchar")
                        {
                            sNewDatatype = "s";
                        }
                        else if (Data_Type == "nvarchar")
                        {
                            sNewDatatype = "n";
                        }
                        else if (Data_Type == "datetime")
                        {
                            sNewDatatype = "d";
                        }
                        else if (Data_Type == "float")
                        {
                            sNewDatatype = "r";
                        }

                        //string NewMaxLen = "0";
                        //if(sMax_Length=="NULL"&&sMax_Length=="null")
                        //{
                        //    NewMaxLen = "0";
                        //}
                        //else
                        //{
                        //    NewMaxLen = sMax_Length;
                        //}

                        //Is Null
                        string NewNullVal = "NOT NULL";
                        if (sIS_NULL == "NO" || sIS_NULL == "no")
                        {
                            NewNullVal = "NOT NULL";
                        }
                        else
                        {
                            NewNullVal = "NULL";
                        }
                        string sStartCharacter = sCol_Name.Substring(0, 1);
                        if (sCol_Name == "ID")
                        {
                            //sNewColDetails = sCol_Name + "," + Data_Type + "," + NewNullVal;
                            sNewColDetails = sCol_Name;
                        }
                        else if (sStartCharacter == sNewDatatype)
                        {
                            if (sMax_Length == "NULL")
                            {
                                //sNewColDetails = sCol_Name + "," + Data_Type + "," + NewNullVal;
                                sNewColDetails = sCol_Name;
                            }
                            else
                            {
                                //sNewColDetails = sCol_Name + "," + Data_Type + "(" + sMax_Length + ")," + NewNullVal;
                                sNewColDetails = sCol_Name;
                            }
                        }
                        else
                        {
                            //consider the column name directly
                            if (sMax_Length == "NULL")
                            {
                                sNewColDetails = sCol_Name + "," + Data_Type + "," + NewNullVal;
                            }
                            else
                            {
                                sNewColDetails = sCol_Name + "," + Data_Type + "(" + sMax_Length + ")," + NewNullVal;
                            }
                            //sNewColDetails = sNewDatatype + sCol_Name + "_" + Data_Type + "_" + NewMaxLen + "_" + NewNullVal;
                        }
                        //  sNewColDetails = sNewDatatype + sCol_Name + "_" + Data_Type + "_" + sIS_NULL;
                        sColDetails.Add(sNewColDetails);
                        if (Col_Details == "")
                        {
                            Col_Details = sNewColDetails + "\n";
                        }
                        else
                        {
                            Col_Details = Col_Details + sNewColDetails + "\n";
                        }
                    }
                }
                var sRemoveNewLineAtEnd = Col_Details.TrimEnd('\n');
                var sRemoveEmptyParenthesis = sRemoveNewLineAtEnd.Replace("()", "");

                string sNewCmpCol = string.Join(",", lCmptCol.ToArray());
                string sCmptdCols = sNewCmpCol.Replace("\n,", "\n");
                string sFnlCmptdCol = sCmptdCols.TrimEnd('\n');
                if (sFnlCmptdCol != "")
                {
                    sNewCol_Details = sRemoveEmptyParenthesis + "\n" + sFnlCmptdCol;
                }
                else
                {
                    sNewCol_Details = sRemoveEmptyParenthesis;
                }

                Con.Close();
            }
            var NewBO = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, null, ID.ToString());
            NewBO.sColumns = sNewCol_Details;
            return NewBO;
        }

        #endregion BOMethods
        // Applications
        // Master

        // XI
        // - Applications
        // - XYZ application
        // - BOs
        // - XYZ BO   
        // - Attributes   
        // - XYZ Attribute
        // - Groups   
        // - Scripts 
        // - Class Attributes
        // - Structure
        // - Actions
        // - Security
        // - BO UI [1:1]
        // - Links
        // - XYZ XILink
        // - LinkGroups   
        // - Params
        // - QuestionSets
        // - Step
        // - Section
        // - Script
        // - Outcome
        // - Navigate
        // - Menus
        // - Visualisers
        // - Components
        // - 1-Clicks (application)
        // - Layouts
        // - BOs
        // - Library
        // - BO 
        // - Attr
        // - Script
        // - QS
        // - Steps
        // - Organisations
        // - 1-Click Searches (Org)
        // - Datasources
        // - 1-Click Searches (Application)


        public CResult Get_OrgDefinition(string sOrgName = "", string sUID = "")
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
                long iID = 0;
                if (long.TryParse(sUID, out iID))
                {
                }
                else
                {
                }

                string PKColumn = string.Empty;
                string PKValue = string.Empty;
                if (iID > 0)
                {
                    PKColumn = "ID";
                    PKValue = iID.ToString();
                }
                else if (iID == 0 && !string.IsNullOrEmpty(sUID) && sUID != "0")
                {
                    PKColumn = "xiguid";
                    PKValue = sUID;
                }
                else if (!string.IsNullOrEmpty(sOrgName))
                {
                    PKColumn = "Name";
                    PKValue = sOrgName;
                }
                XIDOrganisation oOrg = null;
                cConnectionString oConString = new cConnectionString();
                string sConString = oConString.ConnectionString(sOrgDatabase);
                XIDBAPI Connection = new XIDBAPI(sConString);
                Dictionary<string, object> Params = new Dictionary<string, object>();
                Params[PKColumn] = PKValue;
                oOrg = Connection.Select<XIDOrganisation>("Organizations", Params).FirstOrDefault();
                oCResult.oResult = oOrg;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while loading Organisation definition" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oCResult; // always
        }

        public CResult Get_LayoutDefinition(string LayoutName = "", string sUID = "")
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
                long iID = 0;
                if (long.TryParse(sUID, out iID))
                {
                }
                else
                {
                }


                string PKColumn = string.Empty;
                string PKValue = string.Empty;
                if (iID > 0)
                {
                    PKColumn = "ID";
                    PKValue = iID.ToString();
                }
                else if (iID == 0 && !string.IsNullOrEmpty(sUID) && sUID != "0" && sUID != "0")
                {
                    PKColumn = "xiguid";
                    PKValue = sUID;
                }
                else if (!string.IsNullOrEmpty(LayoutName))
                {
                    PKColumn = "LayoutName";
                    PKValue = LayoutName;
                }
                XIDLayout oLayout = null;
                Dictionary<string, object> Params = new Dictionary<string, object>();
                Params[PKColumn] = PKValue;
                oLayout = Connection.Select<XIDLayout>("XILayout_T", Params).FirstOrDefault();
                if (oLayout != null)
                {
                    Dictionary<string, object> DParams = new Dictionary<string, object>();
                    DParams["LayoutID"] = oLayout.ID;
                    oLayout.LayoutDetails = Connection.Select<XIDLayoutDetails>("XILayoutDetail_T", DParams).ToList();
                }
                if (oLayout.iThemeID > 0)
                {
                    Dictionary<string, object> UserParams = new Dictionary<string, object>();
                    UserParams["ID"] = oLayout.iThemeID;
                    oLayout.sThemeName = Connection.Select<XIDMasterData>("XIMasterData_T", UserParams).FirstOrDefault().FileName;
                }
                Dictionary<string, object> MapParams = new Dictionary<string, object>();
                MapParams["PopupLayoutID"] = oLayout.ID;
                MapParams["StatusTypeID"] = "10";
                oLayout.LayoutMappings = Connection.Select<XIDLayoutMapping>("XILayoutMapping_T", MapParams).ToList();

                if (oLayout.LayoutType.ToLower() == "inline" || oLayout.LayoutType.ToLower() == "template")
                {
                    oLayout.sGUID = Guid.NewGuid().ToString();
                }
                oCResult.oResult = oLayout;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while loading Layout definition" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oCResult; // always
        }

        //private string AddXiParametersToCache(int LayoutID, int XiParameterID, int ID, int BOID, string sNewGUID)
        //{
        //    XIParameter Parameter = new XIParameter();
        //    Dictionary<string, object> Params = new Dictionary<string, object>();
        //    Params["XiParameterID"] = XiParameterID;
        //    Parameter = Connection.Select<XIParameter>("XILayoutDetail_T", Params).FirstOrDefault();
        //    var sSessionID = HttpContext.Current.Session.SessionID;
        //    string sGUID = sNewGUID;
        //    if (!string.IsNullOrEmpty(sGUID))
        //    {
        //        //XICache oCache = new XICache();
        //        var sParentGUID = oCache.Get_ParamVal(sSessionID, sGUID, null, "{XIP|ParentGUID}");
        //        if (!string.IsNullOrEmpty(sParentGUID))
        //        {
        //            sGUID = sParentGUID;
        //        }
        //        else
        //        {
        //            sParentGUID = oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|ParentGUID}", sGUID.ToString(), null, null);
        //        }
        //    }

        //    //if (ID > 0)
        //    //{
        //    string sBOName = string.Empty;
        //    var oBO = Load_BO("", BOID);
        //    if (oBO != null)
        //    {
        //        sBOName = oBO.Name;
        //    }
        //    //CInstance oCache = Cacheobj.Get_XICache();

        //    var BOFields = oBO.Attributes.Values.Where(m => m.FKTableName == sBOName).ToList();
        //    if (BOFields != null && BOFields.Count() > 0)
        //    {
        //        var ActiveFK = BOFields.FirstOrDefault().Name;
        //        oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|ActiveFK}", ActiveFK.ToString(), null, null);
        //    }
        //    oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|ActiveBO}", sBOName.ToString(), null, null);
        //    if (ID > 0)
        //    {
        //        oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|" + sBOName + ".id}", ID.ToString(), null, null);
        //    }

        //    oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|BODID}", BOID.ToString(), null, null);

        //    return sGUID;
        //}

        public CResult Get_ApplicationDefinition(string sApplicationName = "", string sUID = "")
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
                long iID = 0;
                if (long.TryParse(sUID, out iID))
                {
                }
                else
                {
                }
                XIDApplication oAPP = null;
                Dictionary<string, object> Params = new Dictionary<string, object>();
                if (iID > 0)
                {
                    Params["ID"] = iID;
                }
                if (!string.IsNullOrEmpty(sApplicationName))
                {
                    Params["sApplicationName"] = sApplicationName;
                }
                if (iID > 0 || !string.IsNullOrEmpty(sApplicationName))
                {
                    oAPP = Connection.Select<XIDApplication>("XIApplication_T", Params).FirstOrDefault();
                    oCResult.oResult = oAPP;
                    oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                }
                else
                {
                    oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiLogicalError;
                }

            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while loading Application definition" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oCResult; // always

            //return oURL;
        }

        public CResult Get_URLDefinition(string sUrlName = "")
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
                XIURLMappings oURL = null;
                Dictionary<string, object> Params = new Dictionary<string, object>();
                Params["sUrlName"] = sUrlName;
                oURL = Connection.Select<XIURLMappings>("XIUrlMappings_T", Params).FirstOrDefault();
                oCResult.oResult = oURL;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while loading URL definition" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oCResult; // always

            //return oURL;
        }

        public CResult Get_XILinkDefinition(string sUID = "", string sName = "")
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
                long iID = 0;
                if (long.TryParse(sUID, out iID))
                {
                }
                else
                {
                }
                string PKColumn = string.Empty;
                string PKValue = string.Empty;
                if (iID > 0)
                {
                    PKColumn = "XiLinkID";
                    PKValue = iID.ToString();
                }
                else if (!string.IsNullOrEmpty(sName))
                {
                    PKColumn = "Name";
                    PKValue = sName;
                }
                else if (iID == 0 && !string.IsNullOrEmpty(sUID) && sUID != "0")
                {
                    PKColumn = "xiguid";
                    PKValue = sUID;
                }
                XILink oXiLink = null;
                Dictionary<string, object> Params = new Dictionary<string, object>();
                Params[PKColumn] = PKValue;
                oXiLink = Connection.Select<XILink>("XILink_T", Params).FirstOrDefault();
                if (oXiLink != null)
                {
                    Dictionary<string, object> NVParams = new Dictionary<string, object>();
                    NVParams["XiLinkID"] = oXiLink.XiLinkID;
                    oXiLink.XiLinkNVs = Connection.Select<XiLinkNV>("XILinkNV_T", NVParams).ToList();
                }
                if (oXiLink != null)
                {
                    Dictionary<string, object> ListParams = new Dictionary<string, object>();
                    ListParams["XiLinkID"] = oXiLink.XiLinkID;
                    oXiLink.XiLinkLists = Connection.Select<XiLinkList>("XILinkList_T", ListParams).ToList();
                }
                oCResult.oResult = oXiLink;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while loading XILink definition ID: " + sUID + " Name: " + sName });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oCResult; // always

            //return oURL;
        }

        public CResult Get_1ClickDefinition(string sName = "", string sUID = "", string sStructureCode = "")
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
                long iID = 0;
                if (long.TryParse(sUID, out iID))
                {
                }
                else
                {
                }
                string PKColumn = string.Empty;
                string PKValue = string.Empty;
                if (iID > 0)
                {
                    PKColumn = "ID";
                    PKValue = iID.ToString();
                }
                else if (!String.IsNullOrEmpty(sName))
                {
                    PKColumn = "Name";
                    PKValue = sName;
                }
                else if (iID == 0 && !string.IsNullOrEmpty(sUID) && sUID != "0")
                {
                    PKColumn = "xiguid";
                    PKValue = sUID;
                }
                XID1Click o1Click = null;
                Dictionary<string, object> Params = new Dictionary<string, object>();
                Params[PKColumn] = PKValue;
                o1Click = Connection.Select<XID1Click>("XI1Click_T", Params).FirstOrDefault();
                if (o1Click != null)
                {
                    List<XID1ClickLink> oLinks = null;
                    Dictionary<string, object> LinkParams = new Dictionary<string, object>();
                    LinkParams["fki1clickid"] = o1Click.ID;
                    oLinks = Connection.Select<XID1ClickLink>("XI1ClickLink_T", LinkParams).ToList();
                    o1Click.MyLinks = oLinks;
                    List<XID1ClickParameter> oParameter = null;
                    Dictionary<string, object> oClickParameters = new Dictionary<string, object>();
                    oClickParameters["fki1clickid"] = o1Click.ID;
                    oParameter = Connection.Select<XID1ClickParameter>("XI1ClickParameter_T", oClickParameters).ToList();
                    o1Click.oOneClickParameters = oParameter;
                    if (o1Click.FKiComponentID > 0)
                    {
                        XIInfraCache oCache = new XIInfraCache();
                        XIDComponent oXICompD = new XIDComponent();
                        oXICompD = (XIDComponent)oCache.GetObjectFromCache(XIConstant.CacheComponent, null, o1Click.FKiComponentID.ToString());
                        var oXICompC = (XIDComponent)oXICompD.Clone(oXICompD);
                        oXICompC.GetParamsByContext(oXICompC, "OneClick", o1Click.ID);
                        o1Click.XIComponent = oXICompC;
                    }
                    //List<XI1ClickSummary> oSummary = null;
                    //Dictionary<string, object> oClickSummary = new Dictionary<string, object>();
                    //oClickSummary["FKi1ClickID"] = o1Click.ID;
                    //oSummary = Connection.Select<XI1ClickSummary>("XI1ClickSummary_T", oClickSummary).ToList();
                    //o1Click.XI1ClickSummary = oSummary;
                    List<XID1ClickPermission> oPermission = null;
                    Dictionary<string, object> o1ClickPermission = new Dictionary<string, object>();
                    o1ClickPermission["FKi1ClickID"] = o1Click.ID;
                    oPermission = Connection.Select<XID1ClickPermission>("XI1ClickPermission_T", o1ClickPermission).ToList();
                    o1Click.RoleIDs = oPermission.Select(x => x.FKiRoleID).ToList();
                    if (o1Click.BOID > 0)
                    {
                        string sSelectFields = string.Empty;
                        sSelectFields = "Name,TableName";
                        Dictionary<string, object> BOParams = new Dictionary<string, object>();
                        BOParams["BOID"] = o1Click.BOID;
                        var FKBOD = Connection.Select<XIDBO>("XIBO_T_N", BOParams, sSelectFields).FirstOrDefault();
                        if (FKBOD != null)
                        {
                            o1Click.sBOName = FKBOD.Name;
                        }
                    }
                    List<XID1Click> sub1Clicks = null;
                    Dictionary<string, object> oSubParams = new Dictionary<string, object>();
                    oSubParams["ParentID"] = o1Click.ID;
                    sub1Clicks = Connection.Select<XID1Click>("XI1Click_T", oSubParams).ToList();
                    o1Click.Sub1Clicks = sub1Clicks;
                    if (!string.IsNullOrEmpty(o1Click.FKi1ClickScriptID))
                    {
                        List<XID1ClickScripts> oScripts = null;
                        Dictionary<string, object> oScriptParams = new Dictionary<string, object>();
                        oScriptParams["FKiOneClickID"] = o1Click.ID;
                        oScripts = Connection.Select<XID1ClickScripts>("XI1ClickScripts_T", oScriptParams).ToList();
                        o1Click.oScripts = oScripts;
                    }
                    List<XID1ClickAction> oAction = null;
                    oClickParameters = new Dictionary<string, object>();
                    oClickParameters["fki1clickid"] = o1Click.ID;
                    oClickParameters[XIConstant.Key_XIDeleted] = "0";
                    oAction = Connection.Select<XID1ClickAction>("XI1ClickAction_T", oClickParameters).ToList();
                    o1Click.Actions = oAction;
                }
                oCResult.oResult = o1Click;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while loading 1-Click definition" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oCResult; // always

            //return oURL;
        }

        public CResult Get_DialogDefinition(string sUID = "")
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
                long iID = 0;
                if (long.TryParse(sUID, out iID))
                {
                }
                else
                {
                }


                string PKColumn = string.Empty;
                string PKValue = string.Empty;
                if (iID > 0)
                {
                    PKColumn = "ID";
                    PKValue = iID.ToString();
                }
                else if (iID == 0 && !string.IsNullOrEmpty(sUID))
                {
                    PKColumn = "xiguid";
                    PKValue = sUID;
                }
                XIDDialog oDialog = new XIDDialog();
                Dictionary<string, object> Params = new Dictionary<string, object>();
                Params[PKColumn] = PKValue;
                if (PKValue != "0")
                {
                    oDialog = Connection.Select<XIDDialog>("XIDialog_T", Params).FirstOrDefault();
                }
                List<XIDLayout> oXIDLayout = null;
                Dictionary<string, object> Params1 = new Dictionary<string, object>();
                Params1["LayoutType"] = "Dialog";
                oXIDLayout = Connection.Select<XIDLayout>("XILayout_T", Params1).ToList();
                var oXIDLayoutDDL = oXIDLayout.Select(m => new XIDropDown { Value = Convert.ToInt32(m.ID), text = m.LayoutName }).ToList();
                oXIDLayoutDDL.Insert(0, new XIDropDown
                {
                    text = "--Select--",
                    Value = 0
                });
                oDialog.Layouts = oXIDLayoutDDL;
                if (iID == 0)
                {
                    oDialog.FKiApplicationID = FKiApplicationID;
                }
                oCResult.oResult = oDialog;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while loading Dialog definition" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oCResult; // always

            //return oURL;
        }

        public CResult Get_PopupDefinition(string sUID = "")
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
                long iID = 0;
                if (long.TryParse(sUID, out iID))
                {
                }
                else
                {
                }


                string PKColumn = string.Empty;
                string PKValue = string.Empty;
                if (iID > 0)
                {
                    PKColumn = "ID";
                    PKValue = iID.ToString();
                }
                else if (iID == 0 && !string.IsNullOrEmpty(sUID))
                {
                    PKColumn = "xiguid";
                    PKValue = sUID;
                }
                XIDPopup oPopup = null;
                Dictionary<string, object> Params = new Dictionary<string, object>();
                Params[PKColumn] = PKValue;
                oPopup = Connection.Select<XIDPopup>("XIPopup_T", Params).FirstOrDefault();
                oCResult.oResult = oPopup;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while loading Dialog definition" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oCResult; // always

            //return oURL;
        }

        public CResult Get_DataSourceDefinition(string sName, string sUID = "", int iApplicationID = 0)
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
                string sType = string.Empty;
                if (sUID.Contains('-'))
                {
                    var Split = sUID.Split('-').ToList();
                    sUID = Split[1];
                    sType = Split[0];
                }
                long iID = 0;
                if (long.TryParse(sUID, out iID))
                {
                }
                else
                {
                }


                string PKColumn = string.Empty;
                string PKValue = string.Empty;
                if (iID > 0)
                {
                    PKColumn = "ID";
                    PKValue = iID.ToString();
                }
                else if (iID == 0 && !string.IsNullOrEmpty(sUID) && sUID != "0")
                {
                    PKColumn = "xiguid";
                    PKValue = sUID;
                }
                else if (!string.IsNullOrEmpty(sName))
                {
                    PKColumn = "sName";
                    PKValue = sName;
                }
                XIDataSource oDataSourceD = new XIDataSource();
                if (!string.IsNullOrEmpty(PKColumn) && !string.IsNullOrEmpty(PKValue))
                {
                    Dictionary<string, object> Params = new Dictionary<string, object>();
                    Params[PKColumn] = PKValue;
                    //string sApplication = ConfigurationManager.AppSettings["AppName"];

                    if (iApplicationID > 0 && iApplicationID == 15 && string.IsNullOrEmpty(sType))
                    {
                        oDataSourceD = Connection.Select<XIDataSource>("XIDataSource_XID_T", Params).FirstOrDefault();
                    }
                    else
                    {
                        oDataSourceD = XIEnvConnection.Select<XIDataSource>("XIDataSource_XID_T", Params).FirstOrDefault();
                    }
                    if (sTypeCyption == "Decrypt")
                    {
                        XIEncryption oXIAPI = new XIEncryption();
                        oDataSourceD.sConnectionString = oXIAPI.DecryptData(oDataSourceD.sConnectionString, true, oDataSourceD.ID.ToString());
                    }
                }
                if (string.IsNullOrEmpty(PKColumn) && string.IsNullOrEmpty(PKValue))
                {
                    oDataSourceD.FKiApplicationID = FKiApplicationID;
                }
                oCResult.oResult = oDataSourceD;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while loading Data Source definition" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oCResult; // always

            //return oURL;
        }

        public CResult Get_XIParameterDefinition(string sUID = "")
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
                long iID = 0;
                if (long.TryParse(sUID, out iID))
                {
                }
                else
                {
                }
                string PKColumn = string.Empty;
                string PKValue = string.Empty;
                if (iID > 0)
                {
                    PKColumn = "XiParameterID";
                    PKValue = iID.ToString();
                }
                else if (iID == 0 && !string.IsNullOrEmpty(sUID) && sUID != "0")
                {
                    PKColumn = "xiguid";
                    PKValue = sUID;
                }
                XIParameter oXiParam = null;
                Dictionary<string, object> Params = new Dictionary<string, object>();
                Params[PKColumn] = PKValue;
                oXiParam = Connection.Select<XIParameter>("XIParameters", Params).FirstOrDefault();
                if (oXiParam != null)
                {
                    Dictionary<string, object> NVParams = new Dictionary<string, object>();
                    NVParams["XiParameterID"] = PKValue;
                    oXiParam.XiParameterNVs = Connection.Select<XiParameterNVs>("XIParameterNVs", NVParams).ToList();
                }
                if (oXiParam != null)
                {
                    Dictionary<string, object> ListParams = new Dictionary<string, object>();
                    ListParams["XiParameterID"] = PKValue;
                    oXiParam.XiParameterLists = Connection.Select<XiParameterLists>("XiParameterLists", ListParams).ToList();
                }
                if (oXiParam.XiParameterID == 0)
                {
                    oXiParam.FKiApplicationID = FKiApplicationID;
                }
                oCResult.oResult = oXiParam;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while loading XILink definition" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oCResult; // always

            //return oURL;
        }

        public CResult Get_VisualisationDefinition(string sUID = "", string sName = "")
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
                long iID = 0;
                if (long.TryParse(sUID, out iID))
                {
                }
                else
                {
                }
                string PKColumn = string.Empty;
                string PKValue = string.Empty;
                if (iID > 0)
                {
                    PKColumn = "XiVisualID";
                    PKValue = iID.ToString();
                }
                else if (!String.IsNullOrEmpty(sName))
                {
                    PKColumn = "Name";
                    PKValue = sName;
                }
                else if (iID == 0 && !string.IsNullOrEmpty(sUID) && sUID != "0")
                {
                    PKColumn = "xiguid";
                    PKValue = sUID;
                }
                XIVisualisation oXivisualisation = null;
                Dictionary<string, object> Params = new Dictionary<string, object>();
                Params[PKColumn] = PKValue;
                oXivisualisation = Connection.Select<XIVisualisation>("XiVisualisations", Params).FirstOrDefault();
                if (oXivisualisation != null)
                {
                    Dictionary<string, object> NVParams = new Dictionary<string, object>();
                    NVParams["XiVisualID"] = oXivisualisation.XiVisualID;
                    oXivisualisation.XiVisualisationNVs = Connection.Select<XIVisualisationNV>("XiVisualisationNVs", NVParams).ToList();
                }
                if (oXivisualisation != null)
                {
                    Dictionary<string, object> ListParams = new Dictionary<string, object>();
                    ListParams["XiVisualID"] = oXivisualisation.XiVisualID;
                    oXivisualisation.XiVisualisationLists = Connection.Select<XIVisualisationList>("XiVisualisationLists", ListParams).ToList();
                }
                oCResult.oResult = oXivisualisation;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while loading XILink definition" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oCResult; // always

            //return oURL;
        }

        public CResult Get_StructureDefinition(string sUID = "")
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
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Started ContentDefinition Loading" });
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
                long iID = 0;
                if (long.TryParse(sUID, out iID))
                {
                }
                else
                {
                }
                string PKColumn = string.Empty;
                string PKValue = string.Empty;
                if (iID > 0)
                {
                    PKColumn = "ID";
                    PKValue = iID.ToString();
                }
                XIDStructure oStructD = new XIDStructure();
                if (PKValue != "0" && PKValue != "")
                {
                    Dictionary<string, object> Params = new Dictionary<string, object>();
                    Params[PKColumn] = PKValue;
                    oStructD = Connection.Select<XIDStructure>("XIBOStructure_T", Params).FirstOrDefault();
                }
                //string PKoColumn = string.Empty;
                //string PKoValue = string.Empty;
                //if (iID > 0)
                //{
                //    PKoColumn = "FKiStructureID";
                //    PKoValue = iID.ToString();
                //}
                //if (PKoValue != "0" && PKoValue != "")
                //{
                //    Dictionary<string, object> oParams = new Dictionary<string, object>();
                //    oParams[PKoColumn] = PKoValue;
                //    oStructD.oStructureDetails = Connection.Select<XIDStructureDetail>("XIBOStructureDetail_T", oParams).FirstOrDefault();
                //}
                //All BO DropDowns
                List<XIDropDown> oBOD = new List<XIDropDown>();
                Dictionary<string, object> oQSParam = new Dictionary<string, object>();
                //oQSParam["FKiApplicationID"] = FKiAppID;
                //oQSParam["FKiOrganisationID"] = iOrgID;
                var oQSDef = Connection.Select<XIDBO>("XIBO_T_N", oQSParam).ToList();
                oBOD = oQSDef.Select(m => new XIDropDown { Value = m.BOID, text = m.Name }).ToList();
                oStructD.BOList = oBOD;

                Dictionary<string, object> Params5 = new Dictionary<string, object>();
                Dictionary<string, string> QSSteps = new Dictionary<string, string>();
                var oAllBOs = Connection.Select<XIDQSStep>("XIQSStepDefinition_T", Params5).ToList();
                foreach (var items in oAllBOs)
                {
                    QSSteps[items.sName] = items.ID.ToString();
                }
                oStructD.AllQSSteps = QSSteps;
                oCResult.oResult = oStructD;
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
        public CResult Get_StructureNodes(string sName = "")
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
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Started ContentDefinition Loading" });
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
                string PKColumn = string.Empty;
                string PKValue = string.Empty;
                if (!string.IsNullOrEmpty(sName))
                {
                    PKColumn = "scode";
                    PKValue = sName;
                }
                List<XIDStructure> oStructD = new List<XIDStructure>();
                if (PKValue != "0" && PKValue != "")
                {
                    Dictionary<string, object> Params = new Dictionary<string, object>();
                    Params[PKColumn] = PKValue;
                    oStructD = Connection.Select<XIDStructure>("XIBOStructure_T", Params).ToList();
                }
                oCResult.oResult = oStructD;
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
        public CResult Get_InboxDefinition(string sUID = "", string iInboxID = "")
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
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Started ContentDefinition Loading" });
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
                long iID = 0;
                if (long.TryParse(sUID, out iID))
                {
                }
                else
                {
                }
                string PKColumn = string.Empty;
                string PKValue = string.Empty;
                if (iID > 0)
                {
                    PKColumn = "RoleID";
                    PKValue = iID.ToString();
                }
                else if (!string.IsNullOrEmpty(iInboxID))
                {
                    PKColumn = "id";
                    PKValue = iInboxID;
                }
                List<XIDInbox> oInboxD = new List<XIDInbox>();
                Dictionary<string, object> Params = new Dictionary<string, object>();
                Params[PKColumn] = PKValue;
                oInboxD = Connection.Select<XIDInbox>("XIInbox_T", Params).ToList();
                oCResult.oResult = oInboxD;
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

        public CResult Get_MenuNodeDefinition(string sUID = "", string sRootName = "")
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
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Started ContentDefinition Loading" });
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
                long iID = 0;
                if (long.TryParse(sUID, out iID))
                {
                }
                else
                {
                }
                string PKColumn = string.Empty;
                string PKValue = string.Empty;
                if (iID > 0)
                {
                    PKColumn = "ID";
                    PKValue = iID.ToString();
                }
                else if (!string.IsNullOrEmpty(sRootName))
                {
                    PKColumn = "rootname";
                    PKValue = sRootName;
                }
                List<XIMenu> oMenuD = new List<XIMenu>();
                Dictionary<string, object> Params = new Dictionary<string, object>();
                Params[PKColumn] = PKValue;
                oMenuD = Connection.Select<XIMenu>("XIMenu_T", Params).ToList();
                oCResult.oResult = oMenuD;
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

        #region QuestionSet 

        public CResult Get_QSDefinition(string sQSName = "", string sUID = "", string sSessionID = null, string sGUID = null, int iOrgID = 0)
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
                XIDQS oDQS = new XIDQS();
                long iID;
                XIIBO oBOI = null/* TODO Change to default(_) if this is not a reference type */;


                // sLoadSteps - if "" then load all. otherwise for example:
                // Step1,Step3,Step9,Step10
                // load steps keyed by name (which must be unique and if it gets same name again just dont add in)
                // the steps are instances
                // KEY BY LOWER CASE ONLY

                // TO DO - get oQSD from the cache
                if (!string.IsNullOrEmpty(sQSName) || !string.IsNullOrEmpty(sUID))
                {
                    oDQS = GetQuestionSetDefinitionByID(sQSName, sUID, sSessionID, sGUID, iOrgID);
                    if (oDQS != null)
                    {
                        var oPartialQSD = GetPartialQS(oDQS.ID);
                        if (oPartialQSD != null && oPartialQSD.PartialQS != null && oPartialQSD.PartialQS.Count > 0)
                        {
                            foreach (var oPartialQs in oPartialQSD.PartialQS)
                            {
                                var oQSD = oPartialQs.Value.oQSD.FirstOrDefault().Value;
                                var CurrentStepOrder = oDQS.Steps.Where(m => m.Value.ID == oPartialQs.Value.FkiStepID).Select(m => m.Value.iOrder).FirstOrDefault();
                                if (oQSD.Steps != null && oQSD.Steps.Count > 0)
                                {
                                    foreach (var oPartialQSstep in oQSD.Steps)
                                    {
                                        CurrentStepOrder += oPartialQs.Value.iOrderGap;
                                        oPartialQSstep.Value.iOrder = CurrentStepOrder;
                                        oDQS.Steps[oPartialQSstep.Key] = oPartialQSstep.Value;
                                    }
                                }
                            }
                        }
                    }
                }

                if (oDQS != null)
                {
                    oDQS.BOI = oBOI;
                    oDQS.oParent = this;
                }
                oCResult.oResult = oDQS;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while loading Question Set definition" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oCResult; // always

            //return oURL;
        }

        public XIDQS GetQuestionSetDefinitionByID(string sQSName, string sUID, string sSessionID = null, string sGUID = null, int iOrgID = 0)
        {
            XIInfraCache oCache = new XIInfraCache();
            string sQSDefinitionQry = "select * from XIQSDefinition_T QSDef " +
              "left join XIQSStepDefinition_T QSSDef on QSDef.ID = QSSDef.FKiQSDefintionID " +
              "left join XIFieldDefinition_T XIFD on QSSDef.ID = XIFD.FKiXIStepDefinitionID " +
              "left join XIFieldOrigin_T XIO on XIFD.FKiXIFieldOriginID = XIO.ID " +
              "left join XIDataType_T XIDT on XIO.FKiDataType = XIDT.id " +
              "left join XIQSNavigation_T NAV on QSSDef.ID = NAV.FKiStepDefinitionID " +
              "left join XIFieldOptionList_T OPT on XIO.ID = OPT.FKiQSFieldID ";

            long iID = 0;
            if (long.TryParse(sUID, out iID))
            {
            }
            else
            {
            }

            if (!string.IsNullOrEmpty(sQSName))
            {
                sQSDefinitionQry = sQSDefinitionQry + "WHERE QSDef.sName = @Name";
            }
            else if (iID > 0)
            {
                sQSDefinitionQry = sQSDefinitionQry + "WHERE QSDef.ID = @id";
            }
            else if (iID == 0 && !string.IsNullOrEmpty(sUID) && sUID != "0")
            {
                sQSDefinitionQry = sQSDefinitionQry + "WHERE QSDef.xiguid = @xiguid";
            }
            if (iOrgID > 0)
            {
                //sQSDefinitionQry = sQSDefinitionQry + " and QSDef.FKiOrgID= @FKiOrgID";
            }
            sQSDefinitionQry = sQSDefinitionQry + ";";
            var param = new
            {
                id = iID,
                Name = sQSName,
                xiguid = sUID,
                //FKiOrgID = iOrgID
            };
            var lookup = new Dictionary<int, XIDQS>();
            using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString))
            {
                Conn.Open();
                var lookup2 = new Dictionary<int, XIDQSStep>();
                var lookup3 = new Dictionary<int, XIDFieldDefinition>();
                var lookup4 = new Dictionary<int, XIDFieldOrigin>();
                var lookup5 = new Dictionary<int, XIDQSStepNavigations>();
                var lookup6 = new Dictionary<int, XIDFieldOptionList>();
                Conn.Query<XIDQS, XIDQSStep, XIDFieldDefinition, XIDFieldOrigin, XIDDataType, XIDQSStepNavigations, XIDFieldOptionList, XIDQS>(sQSDefinitionQry,
                    (QS, Step, FieldDefinition, FieldOrigin, DataType, Navigations, OptionList) =>
                    {
                        XIDQS oQSDefinition;
                        if (!lookup.TryGetValue(QS.ID, out oQSDefinition))
                        {
                            lookup.Add(QS.ID, oQSDefinition = QS);
                        }
                        XIDQSStep oStepDefinition;
                        if (Step != null && Step.StatusTypeID == (int)xiEnumSystem.xistatus.xiactive)
                        {
                            if (!lookup2.TryGetValue(Step.ID, out oStepDefinition))
                            {
                                lookup2.Add(Step.ID, oStepDefinition = Step);
                                oQSDefinition.Steps[oStepDefinition.sName] = oStepDefinition;
                            }
                            XIDFieldDefinition oFieldDefintion;
                            if (FieldDefinition != null)
                            {
                                if (!lookup3.TryGetValue(FieldDefinition.ID, out oFieldDefintion))
                                {
                                    lookup3.Add(FieldDefinition.ID, oFieldDefintion = FieldDefinition);
                                    oStepDefinition.FieldDefs[FieldOrigin.sName] = oFieldDefintion;

                                }
                                XIDFieldOrigin oXIFieldOrigin;
                                if (FieldOrigin != null)
                                {

                                    if (!lookup4.TryGetValue(FieldOrigin.ID, out oXIFieldOrigin))
                                    {

                                        if (FieldOrigin.iMasterDataID > 0)
                                        {
                                            //FieldOrigin.ddlFieldOptionList = dbContext.Types.Where(m => m.Code == FieldOrigin.iMasterDataID).ToList().Select(m => new XIDFieldOptionList { sOptionName = m.Expression, sOptionValue = m.ID.ToString() }).ToList();
                                        }
                                        else if (FieldOrigin.FK1ClickID > 0)
                                        {
                                            var o1ClickD = (XID1Click)oCache.GetObjectFromCache(XIConstant.Cache1Click, null, FieldOrigin.FK1ClickID.ToString());
                                            XIDBO oBOD = new XIDBO();
                                            oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, null, o1ClickD.BOID.ToString());
                                            if (oBOD != null)
                                            {
                                                FieldOrigin.sBOSize = oBOD.sSize;
                                            }
                                        }
                                        else if (FieldOrigin.FKiBOID > 0)
                                        {
                                            var oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, null, FieldOrigin.FKiBOID.ToString());
                                            if (oBOD != null)
                                            {
                                                FieldOrigin.sBOSize = oBOD.sSize;
                                                if (oBOD.sSize == "10")
                                                {
                                                    string sBODataSource = GetBODataSource(oBOD.iDataSource, oBOD.FKiApplicationID);
                                                    if (oBOD.Groups.ContainsKey("label"))
                                                    {
                                                        var oGroupD = oBOD.Groups["label"];
                                                        var Con = new XIDBAPI(sBODataSource);
                                                        var LabelGroup = oGroupD.BOFieldNames;
                                                        if (!string.IsNullOrEmpty(LabelGroup))
                                                        {
                                                            FieldOrigin.sBOSize = oBOD.sSize;
                                                            //string FinalString = oGroupD.ConcatanateFields(LabelGroup, " ");
                                                            //FinalString = oBOD.sPrimaryKey + "," + FinalString;
                                                            string FinalString = LabelGroup;
                                                            Dictionary<string, string> DDL = Con.SelectDDL(FinalString, oBOD.TableName);
                                                            var FKDDL = DDL.Select(m => new XIDFieldOptionList { sOptionName = m.Value, sOptionValue = m.Key }).ToList();
                                                            FieldOrigin.FieldDynamicOptionList = new List<XIDFieldOptionList>();
                                                            FieldOrigin.FieldDynamicOptionList = FKDDL;
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        lookup4.Add(FieldOrigin.ID, oXIFieldOrigin = FieldOrigin);
                                        oXIFieldOrigin = FieldOrigin;
                                    }

                                    XIDFieldOptionList oOptions;
                                    if (OptionList != null)
                                    {
                                        oOptions = OptionList;
                                        if (!lookup6.TryGetValue(OptionList.ID, out oOptions))
                                        {
                                            lookup6.Add(OptionList.ID, oOptions = OptionList);
                                            if (oXIFieldOrigin.FieldOptionList != null && oXIFieldOrigin.FieldOptionList.Count() > 0)
                                            {
                                                oXIFieldOrigin.FieldOptionList.Add(oOptions);
                                            }
                                            else
                                            {
                                                oXIFieldOrigin.FieldOptionList = new List<XIDFieldOptionList>();
                                                oXIFieldOrigin.FieldOptionList.Add(oOptions);
                                            }
                                        }
                                    }
                                    oFieldDefintion.FieldOrigin = oXIFieldOrigin;
                                    FieldOrigin.DataType = DataType;
                                    //if (!oQSDefinition.XIDFieldOrigin.ContainsKey(oXIFieldOrigin.sName))
                                    //{
                                    //    if (FieldOrigin.DataType.sName.ToLower() == "date")
                                    //    {
                                    //        var oResult = SetDefaultDateRange(FieldOrigin.sMinDate, FieldOrigin.sMaxDate);
                                    //        if (oResult != null && oResult.Count() > 0)
                                    //        {
                                    //            if (oResult.ContainsKey("sMinDate"))
                                    //            {
                                    //                FieldOrigin.sMinDate = oResult["sMinDate"];
                                    //            }
                                    //            if (oResult.ContainsKey("sMaxDate"))
                                    //            {
                                    //                FieldOrigin.sMaxDate = oResult["sMaxDate"];
                                    //            }
                                    //        }
                                    //    }
                                    //    oQSDefinition.XIDFieldOrigin[oXIFieldOrigin.sName] = oXIFieldOrigin;
                                    //}
                                }


                                //if (oFieldInstance.FieldDefinitions != null && oFieldInstance.FieldDefinitions.Count() > 0)
                                //{
                                //    oFieldInstance.FieldDefinitions.Add(FieldDefinition);
                                //}
                                //else
                                //{
                                //    oFieldInstance.FieldDefinitions = new List<cXIFieldDefinition>();
                                //    oFieldInstance.FieldDefinitions.Add(FieldDefinition);
                                //}
                            }
                            if (Navigations != null)
                            {
                                XIDQSStepNavigations nNavs;
                                if (!lookup5.TryGetValue(Navigations.ID, out nNavs))
                                {
                                    lookup5.Add(Navigations.ID, nNavs = Navigations);
                                    oStepDefinition.Navigations[nNavs.sName] = nNavs;
                                }
                            }
                        }
                        return oQSDefinition;
                    },
                    param
                    ).AsQueryable();
            }
            var oQSDef = lookup.Values.FirstOrDefault();
            var FKSteps = oQSDef.Steps.Values.ToList().Where(m => m.FKiParentStepID > 0).ToList();
            if (FKSteps != null && FKSteps.Count() > 0)
            {
                foreach (var Step in FKSteps)
                {
                    var oFKStep = GetStepDefinitionByID(Step.FKiParentStepID.ToString());
                    if (oFKStep != null)
                    {
                        var StepID = Step.ID;
                        oFKStep.ID = StepID;
                        oQSDef.Steps[Step.sName] = oFKStep;
                    }
                }
            }


            var Sections = GetStepSectionDefinitions(oQSDef.ID, sSessionID, sGUID);
            if (Sections != null)
            {
                foreach (var items in Sections.Steps)
                {
                    var Secs = items.Value.Sections.OrderBy(m => m.Value.iOrder).ToDictionary(t => t.Key, t => t.Value);
                    oQSDef.Steps.Where(m => m.Value.ID == items.Value.ID).FirstOrDefault().Value.Sections = Secs;
                    //oQSInstance.nStepInstances.Where(m => m.FKiQSStepDefinitionID == items.ID).FirstOrDefault().nSections = items.Sections;
                }
                foreach (var sec in Sections.XIDFieldOrigin)
                {
                    if (!oQSDef.XIDFieldOrigin.ContainsKey(sec.Key))
                    {
                        oQSDef.XIDFieldOrigin[sec.Key] = sec.Value;
                    }
                }
                if (Sections.XIDFieldOrigin.Values != null && Sections.XIDFieldOrigin.Values.Count() > 0)
                {
                    oQSDef.XIDFieldOrigin = Sections.XIDFieldOrigin;
                }
                if (Sections.DependentFields != null && Sections.DependentFields.Count() > 0)
                {
                    oQSDef.DependentFields = Sections.DependentFields;
                }
            }
            var QSLinks = GetStepQSLinks(Convert.ToInt32(iID));

            if (QSLinks != null)
            {
                foreach (var items in QSLinks.Steps)
                {
                    oQSDef.Steps.Values.Where(m => m.ID == items.Value.ID).FirstOrDefault().QSLinks = items.Value.QSLinks;
                }
            }
            oQSDef.Visualisation = GetQSVisualisations(oQSDef.iVisualisationID);
            oQSDef.QSVisualisations = GetQSFiledVisualisations(iID);
            if (oQSDef.Steps != null)
            {
                foreach (var items in oQSDef.Steps)
                {
                    if (items.Value.iLayoutID > 0)
                    {
                        items.Value.Layout = (XIDLayout)Get_LayoutDefinition(null, items.Value.iLayoutID.ToString()).oResult;
                    }
                    //Get Scripts Linked to Step
                    List<XIQSScript> oStepScripts = null;
                    Dictionary<string, object> Params = new Dictionary<string, object>();
                    Params["FKiStepDefinitionID"] = items.Value.ID;
                    oStepScripts = Connection.Select<XIQSScript>("XIQSScript_T", Params).ToList();
                    if (oStepScripts != null && oStepScripts.Count() > 0)
                    {
                        foreach (var scr in oStepScripts)
                        {
                            CResult oScr = Get_ScriptDefinition(null, scr.FKiScriptID.ToString());
                            if (oScr.bOK && oScr.oResult != null)
                            {
                                XIDScript oScriptID = (XIDScript)oScr.oResult;
                                oQSDef.Steps[items.Value.sName].Scripts[oScriptID.sName] = oScriptID;
                            }
                        }
                    }
                }
            }
            return oQSDef;
        }

        public CResult Get_ScriptDefinition(string sScriptName = "", string sUID = "")
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
                long iID = 0;
                if (long.TryParse(sUID, out iID))
                {
                }
                else
                {
                }


                string PKColumn = string.Empty;
                string PKValue = string.Empty;
                if (iID > 0)
                {
                    PKColumn = "ID";
                    PKValue = iID.ToString();
                }
                else if (iID == 0 && !string.IsNullOrEmpty(sUID) && sUID != "0" && sUID != "0")
                {
                    PKColumn = "xiguid";
                    PKValue = sUID;
                }
                else if (!string.IsNullOrEmpty(sScriptName))
                {
                    PKColumn = "sName";
                    PKValue = sScriptName;
                }
                XIDScript oScript = null;
                Dictionary<string, object> Params = new Dictionary<string, object>();
                Params[PKColumn] = PKValue;
                oScript = Connection.Select<XIDScript>("XIBOScript_T", Params).FirstOrDefault();
                oCResult.oResult = oScript;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while loading Layout definition" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oCResult; // always
        }

        public XIDQS GetStepSectionDefinitions(int iQSID, string sSessionID = null, string sGUID = null)
        {
            XIInfraCache oCache = new XIInfraCache();
            string sQSDefinitionQry = "select * from XIQSDefinition_T QSDef " +
                "inner join XIQSStepDefinition_T QSSDef on QSDef.ID = QSSDef.FKiQSDefintionID " +
                "inner join XIStepSectionDefinition_T Sec on QSSDef.ID = Sec.FKiStepDefinitionID " +
                "left join XIFieldDefinition_T XIFD on Sec.ID = XIFD.FKiStepSectionID " +
                "left join XIFieldOrigin_T XIO on XIFD.FKiXIFieldOriginID = XIO.ID " +
                "left join XIDataType_T XIDT on XIO.FKiDataType = XIDT.id " +
                "left join XIFieldOptionList_T OPT on XIO.ID = OPT.FKiQSFieldID " +
                "WHERE QSDef.ID = @id;";
            var param = new
            {
                id = iQSID
            };
            XIIXI oXI = new XIIXI();
            List<string> DependentFields = new List<string>();
            var lookup = new Dictionary<int, XIDQS>();
            using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString))
            {
                Conn.Open();
                var lookup2 = new Dictionary<int, XIDQSStep>();
                var lookup3 = new Dictionary<int, XIDFieldDefinition>();
                var lookup4 = new Dictionary<int, XIDFieldOrigin>();
                var lookup6 = new Dictionary<int, XIDFieldOptionList>();
                var lookup7 = new Dictionary<int, XIDQSSection>();
                Conn.Query<XIDQS, XIDQSStep, XIDQSSection, XIDFieldDefinition, XIDFieldOrigin, XIDDataType, XIDFieldOptionList, XIDQS>(sQSDefinitionQry,
                    (QS, Step, SectionDef, FieldDefinition, FieldOrigin, DataType, OptionList) =>
                    {
                        XIDQS oQSDefinition;
                        if (!lookup.TryGetValue(QS.ID, out oQSDefinition))
                        {
                            lookup.Add(QS.ID, oQSDefinition = QS);
                        }
                        XIDQSStep oStepDefinition;
                        if (Step != null && Step.StatusTypeID == (int)xiEnumSystem.xistatus.xiactive)
                        {
                            if (!lookup2.TryGetValue(Step.ID, out oStepDefinition))
                            {
                                lookup2.Add(Step.ID, oStepDefinition = Step);
                                oQSDefinition.Steps[oStepDefinition.sName] = oStepDefinition;
                            }
                            XIDQSSection oSection;
                            if (SectionDef != null)
                            {
                                if (!lookup7.TryGetValue(SectionDef.ID, out oSection))
                                {
                                    lookup7.Add(SectionDef.ID, oSection = SectionDef);
                                    oStepDefinition.Sections[SectionDef.ID.ToString() + "_Sec"] = SectionDef;
                                }
                                XIDFieldDefinition oFieldDefintion;
                                if (FieldDefinition != null)
                                {
                                    XIDFieldOrigin oXIFieldOrigin;
                                    if (FieldOrigin != null)
                                    {
                                        if (!lookup3.TryGetValue(FieldDefinition.ID, out oFieldDefintion))
                                        {
                                            lookup3.Add(FieldDefinition.ID, oFieldDefintion = FieldDefinition);
                                            oSection.FieldDefs[FieldOrigin.sName] = oFieldDefintion;
                                        }
                                        if (!lookup4.TryGetValue(FieldOrigin.ID, out oXIFieldOrigin))
                                        {
                                            if (FieldOrigin.iMasterDataID > 0)
                                            {
                                                //FieldOrigin.ddlFieldOptionList = dbContext.Types.Where(m => m.Code == FieldOrigin.iMasterDataID).ToList().Select(m => new XIDFieldOptionList { sOptionName = m.Expression, sOptionValue = m.ID.ToString() }).ToList();
                                            }
                                            else if (FieldOrigin.FK1ClickID > 0)
                                            {
                                                var o1ClickD = (XID1Click)oCache.GetObjectFromCache(XIConstant.Cache1Click, null, FieldOrigin.FK1ClickID.ToString());
                                                XIDBO oBOD = new XIDBO();
                                                oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, null, o1ClickD.BOID.ToString());
                                                int iDataSource = oBOD.iDataSource;
                                                XIDXI oXID = new XIDXI();
                                                string sConntection = oXID.GetBODataSource(iDataSource, oBOD.FKiApplicationID);
                                                var Connect = new XIDBAPI(sConntection);
                                                o1ClickD.BOD = oBOD;
                                                o1ClickD.Get_1ClickHeadings();
                                                var PrimaryKey = "";
                                                if (o1ClickD.bIsMultiBO == true)
                                                {
                                                    var fields = "[" + o1ClickD.FromBos + "].";
                                                    fields = fields + string.Join(" [" + o1ClickD.FromBos + "].", o1ClickD.TableColumns.ToList());
                                                    var TableColumns = fields.Split(' ');
                                                    o1ClickD.TableColumns = TableColumns.ToList();
                                                    PrimaryKey = "[" + o1ClickD.FromBos + "]." + oBOD.sPrimaryKey;
                                                }
                                                var SelFields = string.Join(", ", o1ClickD.TableColumns.ToList());
                                                XIDGroup oGroupD = new XIDGroup();
                                                string FinalString = oGroupD.ConcatanateFields(SelFields, " ");
                                                FinalString = (o1ClickD.bIsMultiBO == true ? PrimaryKey : oBOD.sPrimaryKey) + "," + FinalString;
                                                var FinalQuery = o1ClickD.AddSelectPart(o1ClickD.Query, FinalString);
                                                if (!FinalQuery.Contains("{XIP|"))
                                                {
                                                    Dictionary<string, string> DDL = Connect.GetDDLItems(CommandType.Text, FinalQuery, null);
                                                    var FKDDL = DDL.Select(m => new XIDFieldOptionList { sOptionName = m.Value, sOptionValue = m.Key }).ToList();
                                                    var DDList = FKDDL.Where(m => !string.IsNullOrEmpty(m.sOptionName)).GroupBy(m => m.sOptionName).Select(m => m.FirstOrDefault()).ToList().Select(p => new { key = p.sOptionName, value = p.sOptionValue })
                                                        .ToDictionary(x => x.key, x => x.value);
                                                    FieldOrigin.FieldDynamicOptionList = FKDDL;
                                                    FieldOrigin.sBOSize = oBOD.sSize;
                                                }
                                                else if (FinalQuery.Contains("{XIP|-"))
                                                {
                                                    XICacheInstance oGUIDParams = oCache.GetAllParamsUnderGUID(sSessionID, sGUID, null);
                                                    List<CNV> nParams = new List<CNV>();
                                                    nParams = oGUIDParams.NMyInstance.Select(m => new CNV { sName = m.Key, sValue = m.Value.sValue, sType = m.Value.sType }).ToList();
                                                    XID1Click oC1ClickD = (XID1Click)o1ClickD.Clone(o1ClickD);
                                                    oC1ClickD.Query = FinalQuery;
                                                    oC1ClickD.ReplaceFKExpressions(nParams);
                                                    FinalQuery = oC1ClickD.Query;
                                                    Dictionary<string, string> DDL = Connect.GetDDLItems(CommandType.Text, FinalQuery, null);
                                                    if (FieldOrigin.FK1ClickID == 8462)
                                                    {
                                                        var iUserOrg = nParams.Where(m => m.sName == "{XIP|iUserOrgID}").Select(m => m.sValue).FirstOrDefault();
                                                        var iNannoGroupID = nParams.Where(m => m.sName == "{XIP|iNannoGroupID}").Select(m => m.sValue).FirstOrDefault();
                                                        var iNannoAppInstID = nParams.Where(m => m.sName == "{XIP|-iNannoAppInstID}").Select(m => m.sValue).FirstOrDefault();
                                                        var iRoleID = nParams.Where(m => m.sName == "iRoleID").Select(m => m.sValue).FirstOrDefault();
                                                        foreach (var obj in DDL)
                                                        {
                                                            var iOrgObjectTypeID = obj.Key.ToString();
                                                            var WhrPrms = new List<CNV>();
                                                            WhrPrms.Add(new CNV { sName = "FKiOrgID", sValue = iUserOrg });
                                                            WhrPrms.Add(new CNV { sName = "FKiNannoGroupID", sValue = iNannoGroupID });
                                                            var shareRole = oXI.BOI("XOrgGroup", null, null, WhrPrms);
                                                            if (shareRole != null && shareRole.Attributes.Count() > 0)
                                                            {
                                                                var ShareRoleID = shareRole.AttributeI("FKiShareRoleID").sValue;
                                                                var Perm1Click = new XID1Click();
                                                                Perm1Click.BOID = 1391;
                                                                if (ShareRoleID == "0" || string.IsNullOrEmpty(ShareRoleID))
                                                                {
                                                                    Perm1Click.Query = "select * from XINannoPermission_T where FKiRoleID =" + iRoleID + " and FKiOrgObjectTypeID=" + iOrgObjectTypeID + " and FKiNannoAppInstID=" + iNannoAppInstID;
                                                                }
                                                                else
                                                                {
                                                                    Perm1Click.Query = "select * from XINannoPermission_T where FKiShareRoleID =" + ShareRoleID + " and FKiOrgObjectTypeID=" + iOrgObjectTypeID + " and FKiNannoAppInstID=" + iNannoAppInstID;
                                                                }

                                                                var Res = Perm1Click.OneClick_Run();
                                                                if (Res != null && Res.Count() > 0)
                                                                {
                                                                    var FKDDL = new List<XIDFieldOptionList>();
                                                                    Dictionary<string, string> Perm = new Dictionary<string, string>();
                                                                    foreach (var perm in Res.Values.ToList())
                                                                    {
                                                                        if (perm.AttributeI("iType").sValue == "30" && perm.AttributeI("iPermission").sValue == "10")
                                                                        {
                                                                            FKDDL.Add(new XIDFieldOptionList { sOptionName = obj.Value, sOptionValue = obj.Key });
                                                                        }
                                                                        //Perm[perm.AttributeI("iType").sValue] = perm.AttributeI("iPermission").sValue;
                                                                    }
                                                                    //Data["Permission"] = Perm;
                                                                    FieldOrigin.FieldDynamicOptionList = FKDDL;
                                                                    FieldOrigin.sBOSize = oBOD.sSize;
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        var FKDDL = DDL.Select(m => new XIDFieldOptionList { sOptionName = m.Value, sOptionValue = m.Key }).ToList();
                                                        var DDList = FKDDL.Where(m => !string.IsNullOrEmpty(m.sOptionName)).GroupBy(m => m.sOptionName).Select(m => m.FirstOrDefault()).ToList().Select(p => new { key = p.sOptionName, value = p.sOptionValue })
                                                            .ToDictionary(x => x.key, x => x.value);
                                                        FieldOrigin.FieldDynamicOptionList = FKDDL;
                                                        FieldOrigin.sBOSize = oBOD.sSize;
                                                    }

                                                }
                                            }
                                            else if (FieldOrigin.FKiBOID > 0)
                                            {
                                                var oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, null, FieldOrigin.FKiBOID.ToString());
                                                string sBODataSource = string.Empty;
                                                if (oBOD != null)
                                                {
                                                    FieldOrigin.sBOSize = oBOD.sSize;
                                                    if (oBOD.sSize == "10")
                                                    {
                                                        if (DataSrcs.ContainsKey(oBOD.iDataSource))
                                                        {
                                                            sBODataSource = DataSrcs[oBOD.iDataSource];
                                                        }
                                                        else
                                                        {
                                                            sBODataSource = GetBODataSource(oBOD.iDataSource, oBOD.FKiApplicationID);
                                                            DataSrcs[oBOD.iDataSource] = sBODataSource;
                                                        }
                                                        if (oBOD.Groups.ContainsKey("label"))
                                                        {
                                                            var oGroupD = oBOD.Groups["label"];
                                                            var Con = new XIDBAPI(sBODataSource);
                                                            var LabelGroup = oGroupD.BOFieldNames;
                                                            if (!string.IsNullOrEmpty(LabelGroup))
                                                            {
                                                                FieldOrigin.sBOSize = oBOD.sSize;
                                                                //string FinalString = oGroupD.ConcatanateFields(LabelGroup, " ");
                                                                //FinalString = oBOD.sPrimaryKey + "," + FinalString;
                                                                string FinalString = LabelGroup;
                                                                Dictionary<string, string> DDL = Con.SelectDDL(FinalString, oBOD.TableName);
                                                                var FKDDL = DDL.Select(m => new XIDFieldOptionList { sOptionName = m.Value, sOptionValue = m.Key }).ToList();
                                                                FieldOrigin.FieldDynamicOptionList = new List<XIDFieldOptionList>();
                                                                FieldOrigin.FieldDynamicOptionList = FKDDL;
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            if (FieldOrigin.bIsFieldDependent)
                                            {
                                                if (FieldOrigin.sDependentFieldID.Contains(','))
                                                {
                                                    var allflds = FieldOrigin.sDependentFieldID.Split(',').ToList();
                                                    DependentFields.AddRange(allflds);
                                                }
                                                else
                                                {
                                                    DependentFields.Add(FieldOrigin.sDependentFieldID);
                                                }
                                            }
                                            if (FieldOrigin.bIsFieldMerge)
                                            {
                                                DependentFields.Add(FieldOrigin.iMergeFieldID.ToString());
                                            }
                                            lookup4.Add(FieldOrigin.ID, oXIFieldOrigin = FieldOrigin);
                                            oXIFieldOrigin = FieldOrigin;
                                        }

                                        XIDFieldOptionList oOptions;
                                        if (OptionList != null)
                                        {
                                            oOptions = OptionList;
                                            if (!lookup6.TryGetValue(OptionList.ID, out oOptions))
                                            {
                                                lookup6.Add(OptionList.ID, oOptions = OptionList);
                                                if (oXIFieldOrigin.FieldOptionList != null && oXIFieldOrigin.FieldOptionList.Count() > 0)
                                                {
                                                    oXIFieldOrigin.FieldOptionList.Add(oOptions);
                                                }
                                                else
                                                {
                                                    oXIFieldOrigin.FieldOptionList = new List<XIDFieldOptionList>();
                                                    oXIFieldOrigin.FieldOptionList.Add(oOptions);
                                                }
                                            }
                                        }
                                        oFieldDefintion.FieldOrigin = oXIFieldOrigin;
                                        oQSDefinition.XIDFieldOrigin[oXIFieldOrigin.sName.ToLower()] = oXIFieldOrigin;
                                        FieldOrigin.DataType = DataType;
                                        //if (!oQSDefinition.XIDFieldOrigin.ContainsKey(oXIFieldOrigin.sName))
                                        //{
                                        //    if (FieldOrigin.DataType.sName.ToLower() == "date")
                                        //    {
                                        //        //mindate setting
                                        //        var oResult = SetDefaultDateRange(FieldOrigin.sMinDate, FieldOrigin.sMaxDate);
                                        //        if (oResult != null && oResult.Count() > 0)
                                        //        {
                                        //            if (oResult.ContainsKey("sMinDate"))
                                        //            {
                                        //                FieldOrigin.sMinDate = oResult["sMinDate"];
                                        //            }
                                        //            if (oResult.ContainsKey("sMaxDate"))
                                        //            {
                                        //                FieldOrigin.sMaxDate = oResult["sMaxDate"];
                                        //            }
                                        //        }
                                        //    }
                                        //    oQSDefinition.XIDFieldOrigin[oXIFieldOrigin.sName] = oXIFieldOrigin;
                                        //}
                                    }
                                    //if (oFieldInstance.FieldDefinitions != null && oFieldInstance.FieldDefinitions.Count() > 0)
                                    //{
                                    //    oFieldInstance.FieldDefinitions.Add(FieldDefinition);
                                    //}
                                    //else
                                    //{
                                    //    oFieldInstance.FieldDefinitions = new List<cXIFieldDefinition>();
                                    //    oFieldInstance.FieldDefinitions.Add(FieldDefinition);
                                    //}
                                }
                            }
                        }
                        oQSDefinition.DependentFields = DependentFields;
                        return oQSDefinition;
                    },
                    param
                    ).AsQueryable();
            }
            var Sections = lookup.Values.FirstOrDefault();

            var QSLinks = GetSectionQSLinks(iQSID);

            if (QSLinks != null)
            {
                foreach (var items in QSLinks.Steps)
                {
                    foreach (var item in items.Value.Sections)
                    {
                        if (item.Value.QSLinks != null)
                        {
                            Sections.Steps.Where(m => m.Value.ID == items.Value.ID).FirstOrDefault().Value.Sections.Values.Where(m => m.ID == item.Value.ID).FirstOrDefault().QSLinks = item.Value.QSLinks;
                        }
                    }
                }
            }


            var SectionContent = GetComponentParams(iQSID);
            if (SectionContent != null)
            {
                foreach (var items in SectionContent.Steps)
                {
                    foreach (var item in items.Value.Sections)
                    {
                        if (item.Value.ComponentDefinition != null)
                        {
                            var AllSections = Sections.Steps.Where(m => m.Value.ID == items.Value.ID).FirstOrDefault();
                            AllSections.Value.ComponentDefinition = item.Value.ComponentDefinition;
                        }
                        //Get Scripts Linked to Section
                        List<XIQSScript> oStepScripts = null;
                        Dictionary<string, object> Params = new Dictionary<string, object>();
                        Params["FKiSectionDefinitionID"] = items.Value.ID;
                        oStepScripts = Connection.Select<XIQSScript>("XIQSScript_T", Params).ToList();
                        if (oStepScripts != null && oStepScripts.Count() > 0)
                        {
                            foreach (var scr in oStepScripts)
                            {
                                CResult oScr = Get_ScriptDefinition(null, scr.FKiScriptID.ToString());
                                if (oScr.bOK && oScr.oResult != null)
                                {
                                    XIDScript oScriptID = (XIDScript)oScr.oResult;
                                    Sections.Steps.Where(m => m.Value.ID == items.Value.ID).FirstOrDefault().Value.Sections.Values.Where(m => m.ID == item.Value.ID).FirstOrDefault().Scripts[oScriptID.sName] = oScriptID;
                                    //items.Value.Scripts[oScriptID.sName] = oScriptID;
                                }
                            }
                        }
                    }
                }
            }
            return Sections;
        }

        public XIDQS GetPartialQS(int iQSID)
        {
            XIInfraCache oCache = new XIInfraCache();
            string sQSDefinitionQry = "select * from XIQSDefinition_T QSDef " +
                "inner join XIDPartialQS_T PartialQS on QSDef.ID = PartialQS.FkiMainQSID " +
                "WHERE QSDef.ID = @id;";
            var param = new
            {
                id = iQSID
            };
            List<string> DependentFields = new List<string>();
            var lookup = new Dictionary<int, XIDQS>();
            using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString))
            {
                Conn.Open();
                var lookup2 = new Dictionary<int, XIDPartialQS>();
                var lookup3 = new Dictionary<int, XIDFieldDefinition>();
                var lookup4 = new Dictionary<int, XIDFieldOrigin>();
                var lookup6 = new Dictionary<int, XIDFieldOptionList>();
                var lookup7 = new Dictionary<int, XIDQSSection>();
                Conn.Query<XIDQS, XIDPartialQS, XIDQS>(sQSDefinitionQry,
                    (QS, PartialQS) =>
                    {
                        XIDQS oQSDefinition;
                        if (!lookup.TryGetValue(QS.ID, out oQSDefinition))
                        {
                            lookup.Add(QS.ID, oQSDefinition = QS);
                        }
                        XIDPartialQS oPartialQSDefinition;
                        if (PartialQS != null)
                        {
                            if (!lookup2.TryGetValue(PartialQS.ID, out oPartialQSDefinition))
                            {
                                var oPartialQSD = GetQuestionSetDefinitionByID("", PartialQS.FKiPartialQSID.ToString());
                                PartialQS.oQSD[oPartialQSD.sName] = oPartialQSD;
                                lookup2.Add(PartialQS.ID, oPartialQSDefinition = PartialQS);
                                oQSDefinition.PartialQS[oPartialQSD.sName] = oPartialQSDefinition;
                            }
                        }
                        return oQSDefinition;
                    },
                    param
                    ).AsQueryable();
            }
            var oQSDef = lookup.Values.FirstOrDefault();
            return oQSDef;
        }

        private XIDQS GetStepQSLinks(int iQSID)
        {

            string sQSDefinitionQry = "select * from XIQSDefinition_T QSD " +
                "inner join XIQSStepDefinition_T QSSD on QSD.ID = QSSD.FKiQSDefintionID " +
                "left join XIQSLink_T QSlink on QSSD.ID = QSlink.FKiStepDefinitionID " +
                "left join XIQSLinkDefinition_T link on QSlink.sCode = link.sCode " +
                "left join XILink_T Xilink on link.FKiXILinkID = Xilink.XiLinkID " +
                "left join XILinkNV_T Xilinknv on Xilink.XILinkID = Xilinknv.XiLinkID " +
                "where QSD.ID = @id and QSSD.StatusTypeID = 10 and QSlink.iStatus=10;";
            var param = new
            {
                id = iQSID
            };
            var lookup = new Dictionary<int, XIDQS>();
            using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString))
            {
                Conn.Open();
                var lookup2 = new Dictionary<int, XIDQSStep>();
                var lookup4 = new Dictionary<long, XIQSLink>();
                var lookup5 = new Dictionary<long, XIQSLinkDefintion>();
                var lookup6 = new Dictionary<int, XILink>();
                Conn.Query<XIDQS, XIDQSStep, XIQSLink, XIQSLinkDefintion, XILink, XiLinkNV, XIDQS>(sQSDefinitionQry,
                    (QS, Step, QSLink, Link, XILink, XILinkNV) =>
                    {
                        XIDQS oQSDefinition;
                        if (!lookup.TryGetValue(QS.ID, out oQSDefinition))
                        {
                            lookup.Add(QS.ID, oQSDefinition = QS);
                        }
                        XIDQSStep oStepDefinition;
                        if (Step != null && Step.StatusTypeID == (int)xiEnumSystem.xistatus.xiactive)
                        {
                            if (!lookup2.TryGetValue(Step.ID, out oStepDefinition))
                            {
                                lookup2.Add(Step.ID, oStepDefinition = Step);
                                oQSDefinition.Steps[oStepDefinition.sName] = oStepDefinition;
                            }
                            XIQSLink oSecQSLink;
                            if (QSLink != null)
                            {
                                XIQSLinkDefintion oXIDLink;
                                if (Link != null)
                                {
                                    if (!lookup4.TryGetValue(QSLink.ID, out oSecQSLink))
                                    {
                                        lookup4.Add(QSLink.ID, oSecQSLink = QSLink);
                                        oStepDefinition.QSLinks[XILink.Name] = QSLink;
                                    }
                                    oXIDLink = Link;
                                    oSecQSLink.XiLink[Link.sName] = Link;
                                    XILink oXILink;
                                    if (XILink != null)
                                    {
                                        if (!lookup5.TryGetValue(Link.ID, out oXIDLink))
                                        {
                                            lookup5.Add(Link.ID, oXIDLink = Link);

                                            oXILink = XILink;
                                            oXIDLink.XiLink[XILink.Name] = XILink;
                                            if (XILinkNV != null)
                                            {
                                                if (oXILink.XiLinkNVs != null && oXILink.XiLinkNVs.Count() > 0)
                                                {
                                                    oXILink.XiLinkNVs.Add(XILinkNV);
                                                }
                                                else
                                                {
                                                    oXILink.XiLinkNVs = new List<XiLinkNV>();
                                                    oXILink.XiLinkNVs.Add(XILinkNV);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        return oQSDefinition;
                    },
                    param,
                     splitOn: "id,id,id,id,xilinkid,id"
                    ).AsQueryable();
            }
            var SectionContent = lookup.Values.FirstOrDefault();
            return SectionContent;
        }

        private XIDQS GetSectionQSLinks(int iQSID)
        {

            string sQSDefinitionQry = "select * from XIQSDefinition_T QSD " +
                "inner join XIQSStepDefinition_T QSSD on QSD.ID = QSSD.FKiQSDefintionID " +
                "inner join XIStepSectionDefinition_T Sec on QSSD.ID = Sec.FKiStepDefinitionID " +
                "left join XIQSLink_T QSlink on sec.ID = QSlink.FKiSectionDefinitionID " +
                "left join XIQSLinkDefinition_T link on QSlink.sCode = link.sCode " +
                "left join XILink_T Xilink on link.FKiXILinkID = Xilink.XiLinkID " +
                "left join XILinkNV_T Xilinknv on Xilink.XILinkID = Xilinknv.XiLinkID " +
                "where QSD.ID = @id and QSlink.iStatus=10;";
            var param = new
            {
                id = iQSID
            };
            var lookup = new Dictionary<int, XIDQS>();
            using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString))
            {
                Conn.Open();
                var lookup2 = new Dictionary<int, XIDQSStep>();
                var lookup3 = new Dictionary<int, XIDQSSection>();
                var lookup4 = new Dictionary<long, XIQSLink>();
                var lookup5 = new Dictionary<long, XIQSLinkDefintion>();
                var lookup6 = new Dictionary<int, XILink>();
                Conn.Query<XIDQS, XIDQSStep, XIDQSSection, XIQSLink, XIQSLinkDefintion, XILink, XiLinkNV, XIDQS>(sQSDefinitionQry,
                    (QS, Step, SectionDef, QSLink, Link, XILink, XILinkNV) =>
                    {
                        XIDQS oQSDefinition;
                        if (!lookup.TryGetValue(QS.ID, out oQSDefinition))
                        {
                            lookup.Add(QS.ID, oQSDefinition = QS);
                        }
                        XIDQSStep oStepDefinition;
                        if (Step != null && Step.StatusTypeID == (int)xiEnumSystem.xistatus.xiactive)
                        {
                            if (!lookup2.TryGetValue(Step.ID, out oStepDefinition))
                            {
                                lookup2.Add(Step.ID, oStepDefinition = Step);
                                oQSDefinition.Steps[oStepDefinition.sName] = oStepDefinition;
                            }
                            XIDQSSection oSection;
                            if (SectionDef != null)
                            {
                                if (!lookup3.TryGetValue(SectionDef.ID, out oSection))
                                {
                                    lookup3.Add(SectionDef.ID, oSection = SectionDef);
                                    oStepDefinition.Sections[SectionDef.ID.ToString() + "_Sec"] = SectionDef;
                                }
                                XIQSLink oSecQSLink;
                                if (QSLink != null)
                                {
                                    XIQSLinkDefintion oXIDLink;
                                    if (Link != null)
                                    {
                                        if (!lookup4.TryGetValue(QSLink.ID, out oSecQSLink))
                                        {
                                            lookup4.Add(QSLink.ID, oSecQSLink = QSLink);
                                            oSection.QSLinks[XILink.Name] = QSLink;
                                        }
                                        oXIDLink = Link;
                                        oSecQSLink.XiLink[Link.sName] = Link;
                                        if (XILink != null)
                                        {
                                            XILink oXILink;
                                            oXILink = XILink;
                                            oXIDLink.XiLink[XILink.Name] = XILink;
                                            if (XILinkNV != null)
                                            {
                                                if (oXILink.XiLinkNVs != null && oXILink.XiLinkNVs.Count() > 0)
                                                {
                                                    oXILink.XiLinkNVs.Add(XILinkNV);
                                                }
                                                else
                                                {
                                                    oXILink.XiLinkNVs = new List<XiLinkNV>();
                                                    oXILink.XiLinkNVs.Add(XILinkNV);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        return oQSDefinition;
                    },
                    param,
                     splitOn: "id,id,id,id,xilinkid,id"
                    ).AsQueryable();
            }
            var SectionContent = lookup.Values.FirstOrDefault();
            return SectionContent;
        }

        private XIDQS GetComponentParams(int iQSID)
        {

            string sQSDefinitionQry = "select * from XIQSDefinition_T QSD " +
                "inner join XIQSStepDefinition_T QSSD on QSD.ID = QSSD.FKiQSDefintionID " +
                "inner join XIStepSectionDefinition_T Sec on QSSD.ID = Sec.FKiStepDefinitionID " +
                "inner join XIComponentParams_T NVs on sec.ID = NVs.iStepSectionID " +
                "inner join XIComponents_XC_T XC on NVs.FKiComponentID = XC.ID " +
                "where QSD.ID = @id;";
            var param = new
            {
                id = iQSID
            };
            var lookup = new Dictionary<int, XIDQS>();
            using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString))
            {
                Conn.Open();
                var lookup2 = new Dictionary<int, XIDQSStep>();
                var lookup3 = new Dictionary<int, XIDQSSection>();
                var lookup4 = new Dictionary<int, XIDComponent>();
                Conn.Query<XIDQS, XIDQSStep, XIDQSSection, XIDComponentParam, XIDComponent, XIDQS>(sQSDefinitionQry,
                    (QS, Step, SectionDef, ComponentParams, Component) =>
                    {
                        XIDQS oQSDefinition;
                        if (!lookup.TryGetValue(QS.ID, out oQSDefinition))
                        {
                            lookup.Add(QS.ID, oQSDefinition = QS);
                        }
                        XIDQSStep oStepDefinition;
                        if (Step != null && Step.StatusTypeID == (int)xiEnumSystem.xistatus.xiactive)
                        {
                            if (!lookup2.TryGetValue(Step.ID, out oStepDefinition))
                            {
                                lookup2.Add(Step.ID, oStepDefinition = Step);
                                oQSDefinition.Steps[oStepDefinition.sName] = oStepDefinition;
                            }
                            XIDQSSection oSection;
                            if (SectionDef != null)
                            {
                                if (!lookup3.TryGetValue(SectionDef.ID, out oSection))
                                {
                                    lookup3.Add(SectionDef.ID, oSection = SectionDef);
                                    oStepDefinition.Sections[SectionDef.ID.ToString() + "_Sec"] = SectionDef;
                                }
                                XIDComponent oComponent;
                                if (Component != null)
                                {
                                    if (!lookup4.TryGetValue(SectionDef.ID, out oComponent))
                                    {
                                        lookup4.Add(SectionDef.ID, oComponent = Component);
                                        oSection.ComponentDefinition = Component;
                                    }
                                    if (ComponentParams != null)
                                    {
                                        if (oSection.ComponentDefinition.Params != null && oSection.ComponentDefinition.Params.Count() > 0)
                                        {
                                            oSection.ComponentDefinition.Params.Add(ComponentParams);
                                        }
                                        else
                                        {
                                            oSection.ComponentDefinition.Params = new List<XIDComponentParam>();
                                            oSection.ComponentDefinition.Params.Add(ComponentParams);
                                        }
                                    }
                                }
                            }
                        }
                        return oQSDefinition;
                    },
                    param
                    ).AsQueryable();
            }
            var SectionContent = lookup.Values.FirstOrDefault();
            return SectionContent;
        }

        private XIVisualisation GetQSVisualisations(int iVisualID)
        {
            XIVisualisation oXiVis = null;
            Dictionary<string, object> Params = new Dictionary<string, object>();
            Params["XiVisualID"] = iVisualID;
            oXiVis = Connection.Select<XIVisualisation>("XiVisualisations", Params).FirstOrDefault();
            if (oXiVis != null)
            {
                List<XIVisualisationNV> oXIVisNV = null;
                Dictionary<string, object> NVParams = new Dictionary<string, object>();
                NVParams["XiVisualID"] = iVisualID;
                oXIVisNV = Connection.Select<XIVisualisationNV>("XiVisualisationNVs", NVParams).ToList();
                if (oXIVisNV != null && oXIVisNV.Count() > 0)
                {
                    oXiVis.XiVisualisationNVs = oXIVisNV;
                }
            }
            return oXiVis;
        }

        private List<XIQSVisualisation> GetQSFiledVisualisations(long iQSID)
        {
            List<XIQSVisualisation> oXiVis = null;
            Dictionary<string, object> Params = new Dictionary<string, object>();
            Params["FKiQSDefinitionID"] = iQSID;
            oXiVis = Connection.Select<XIQSVisualisation>("XIQSVisualisation_T", Params).ToList();
            return oXiVis;
        }

        public CResult Get_StepDefinition(string sStepName = "", string sUID = "")
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
                XIDQSStep oDQS = new XIDQSStep();
                long iID;
                XIIBO oBOI = null/* TODO Change to default(_) if this is not a reference type */;


                // sLoadSteps - if "" then load all. otherwise for example:
                // Step1,Step3,Step9,Step10
                // load steps keyed by name (which must be unique and if it gets same name again just dont add in)
                // the steps are instances
                // KEY BY LOWER CASE ONLY

                // TO DO - get oQSD from the cache
                if (!string.IsNullOrEmpty(sStepName) || !string.IsNullOrEmpty(sUID))
                {
                    oDQS = GetStepDefinitionByID(sUID, sStepName);
                }

                if (oDQS != null)
                {
                    oDQS.BOI = oBOI;
                    oDQS.oParent = this;
                }
                oCResult.oResult = oDQS;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while loading Question Set definition" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oCResult; // always

            //return oURL;
        }


        public XIDQSStep GetStepDefinitionByID(string sUID = "", string sStepName = "")
        {
            string sQSDefinitionQry = "select * from XIQSStepDefinition_T QSSDef " +
                "left join XIFieldDefinition_T XIFD on QSSDef.ID = XIFD.FKiXIStepDefinitionID " +
                "left join XIFieldOrigin_T XIO on XIFD.FKiXIFieldOriginID = XIO.ID " +
                "left join XIDataType_T XIDT on XIO.FKiDataType = XIDT.id " +
                "left join XIQSNavigation_T NAV on QSSDef.ID = NAV.FKiStepDefinitionID " +
                "left join XIFieldOptionList_T OPT on XIO.ID = OPT.FKiQSFieldID ";

            long iID = 0;
            if (long.TryParse(sUID, out iID))
            {
            }
            else
            {
            }

            if (!string.IsNullOrEmpty(sStepName))
            {
                sQSDefinitionQry = sQSDefinitionQry + "WHERE QSSDef.sName = @StepName;";
            }
            else if (iID > 0)
            {
                sQSDefinitionQry = sQSDefinitionQry + "WHERE QSSDef.ID = @id;";
            }
            else if (iID == 0 && !string.IsNullOrEmpty(sUID) && sUID != "0")
            {
                sQSDefinitionQry = sQSDefinitionQry + "WHERE QSDef.xiguid = @xiguid;";
            }

            var param = new
            {
                id = iID,
                StepName = sStepName
            };
            XIInfraCache oCache = new XIInfraCache();
            //if (iStepID > 0)
            //{
            //    sQSDefinitionQry = sQSDefinitionQry + "WHERE QSSDef.ID = @id;";
            //}
            //if (!string.IsNullOrEmpty(sStepName))
            //{
            //    sQSDefinitionQry = sQSDefinitionQry + "WHERE QSSDef.sName = @StepName;";
            //}
            var lookup2 = new Dictionary<int, XIDQSStep>();
            using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString))
            {
                Conn.Open();
                var lookup = new Dictionary<int, XIDQS>();
                var lookup3 = new Dictionary<int, XIDFieldDefinition>();
                var lookup4 = new Dictionary<int, XIDFieldOrigin>();
                var lookup5 = new Dictionary<int, XIDQSStepNavigations>();
                var lookup6 = new Dictionary<int, XIDFieldOptionList>();
                Conn.Query<XIDQSStep, XIDFieldDefinition, XIDFieldOrigin, XIDDataType, XIDQSStepNavigations, XIDFieldOptionList, XIDQSStep>(sQSDefinitionQry,
                    (Step, FieldDefinition, FieldOrigin, DataType, Navigations, OptionList) =>
                    {
                        //XIDQS oQSDefinition;
                        //if (!lookup.TryGetValue(QS.ID, out oQSDefinition))
                        //{
                        //    lookup.Add(QS.ID, oQSDefinition = QS);
                        //}
                        XIDQSStep oStepDefinition;
                        //if (Step != null)
                        //{
                        if (!lookup2.TryGetValue(Step.ID, out oStepDefinition))
                        {
                            lookup2.Add(Step.ID, oStepDefinition = Step);
                        }
                        //if (!lookup2.TryGetValue(Step.ID, out oStepDefinition))
                        //{
                        //    lookup2.Add(Step.ID, oStepDefinition = Step);
                        //    if (oQSDefinition.QSSteps != null && oQSDefinition.QSSteps.Count() > 0)
                        //    {
                        //        oQSDefinition.QSSteps.Add(oStepDefinition);
                        //    }
                        //    else
                        //    {
                        //        oQSDefinition.QSSteps = new List<XIDQSStep>();
                        //        oQSDefinition.QSSteps.Add(oStepDefinition);
                        //    }
                        //}
                        XIDFieldDefinition oFieldDefintion;
                        if (FieldDefinition != null)
                        {

                            XIDFieldOrigin oXIFieldOrigin;
                            if (FieldOrigin != null)
                            {
                                if (!lookup3.TryGetValue(FieldDefinition.ID, out oFieldDefintion))
                                {
                                    lookup3.Add(FieldDefinition.ID, oFieldDefintion = FieldDefinition);
                                    oStepDefinition.FieldDefs[FieldOrigin.sName] = oFieldDefintion;
                                }
                                if (!lookup4.TryGetValue(FieldOrigin.ID, out oXIFieldOrigin))
                                {

                                    if (FieldOrigin.iMasterDataID > 0)
                                    {
                                        //FieldOrigin.ddlFieldOptionList = dbContext.Types.Where(m => m.Code == FieldOrigin.iMasterDataID).ToList().Select(m => new XIDFieldOptionList { sOptionName = m.Expression, sOptionValue = m.ID.ToString() }).ToList();
                                    }
                                    else if (FieldOrigin.FK1ClickID > 0)
                                    {
                                        var o1ClickD = (XID1Click)oCache.GetObjectFromCache(XIConstant.Cache1Click, null, FieldOrigin.FK1ClickID.ToString());
                                        XIDBO oBOD = new XIDBO();
                                        oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, null, o1ClickD.BOID.ToString());
                                        int iDataSource = oBOD.iDataSource;
                                        XIDXI oXID = new XIDXI();
                                        string sConntection = oXID.GetBODataSource(iDataSource, oBOD.FKiApplicationID);
                                        var Connect = new XIDBAPI(sConntection);
                                        o1ClickD.BOD = oBOD;
                                        o1ClickD.Get_1ClickHeadings();
                                        //var bIsPKExists = o1ClickD.TableColumns.ConvertAll(m => m.ToLower()).Contains(oBOD.sPrimaryKey.ToLower());
                                        //if (!bIsPKExists)
                                        //{

                                        //}                                        
                                        var SelFields = string.Join(", ", o1ClickD.TableColumns.ToList());
                                        XIDGroup oGroupD = new XIDGroup();
                                        string FinalString = oGroupD.ConcatanateFields(SelFields, " ");
                                        FinalString = oBOD.sPrimaryKey + "," + FinalString;
                                        var FinalQuery = o1ClickD.AddSelectPart(o1ClickD.Query, FinalString);
                                        //var oBOIns = (DataTable)Connection.ExecuteQuery(o1ClickD.Query);
                                        Dictionary<string, string> DDL = Connect.GetDDLItems(CommandType.Text, FinalQuery, null);
                                        var FKDDL = DDL.Select(m => new XIDFieldOptionList { sOptionName = m.Value, sOptionValue = m.Key }).ToList();
                                        //List<Person> List1 = new List<Person>();
                                        var DDList = FKDDL.Where(m => !string.IsNullOrEmpty(m.sOptionName)).GroupBy(m => m.sOptionName).Select(m => m.FirstOrDefault()).ToList().Select(p => new { key = p.sOptionName, value = p.sOptionValue })
                                                .ToDictionary(x => x.key, x => x.value);
                                        FieldOrigin.sBOSize = oBOD.sSize;
                                    }
                                    lookup4.Add(FieldOrigin.ID, oXIFieldOrigin = FieldOrigin);
                                    oXIFieldOrigin = FieldOrigin;
                                }

                                XIDFieldOptionList oOptions;
                                if (OptionList != null)
                                {
                                    oOptions = OptionList;
                                    if (!lookup6.TryGetValue(OptionList.ID, out oOptions))
                                    {
                                        lookup6.Add(OptionList.ID, oOptions = OptionList);
                                        if (oXIFieldOrigin.FieldOptionList != null && oXIFieldOrigin.FieldOptionList.Count() > 0)
                                        {
                                            oXIFieldOrigin.FieldOptionList.Add(oOptions);
                                        }
                                        else
                                        {
                                            oXIFieldOrigin.FieldOptionList = new List<XIDFieldOptionList>();
                                            oXIFieldOrigin.FieldOptionList.Add(oOptions);
                                        }
                                    }
                                }
                                oFieldDefintion.FieldOrigin = oXIFieldOrigin;
                            }

                            FieldOrigin.DataType = DataType;

                            //if (oFieldInstance.FieldDefinitions != null && oFieldInstance.FieldDefinitions.Count() > 0)
                            //{
                            //    oFieldInstance.FieldDefinitions.Add(FieldDefinition);
                            //}
                            //else
                            //{
                            //    oFieldInstance.FieldDefinitions = new List<cXIFieldDefinition>();
                            //    oFieldInstance.FieldDefinitions.Add(FieldDefinition);
                            //}
                        }
                        if (Navigations != null)
                        {
                            XIDQSStepNavigations nNavs;
                            if (!lookup5.TryGetValue(Navigations.ID, out nNavs))
                            {
                                oStepDefinition.Navigations[Navigations.sName] = nNavs;
                            }
                        }
                        //}
                        return oStepDefinition;
                    },
                    param
                    ).AsQueryable();
            }
            var oQSStepDef = lookup2.Values.FirstOrDefault();
            var QSLinks = GetStepQSLinks(oQSStepDef.FKiQSDefintionID);

            if (QSLinks != null)
            {
                foreach (var items in QSLinks.Steps)
                {
                    oQSStepDef.QSLinks = items.Value.QSLinks;
                }
            }
            var oStepSections = GetStepSectionDefinitionsByStep(oQSStepDef.ID);
            if (oStepSections != null)
            {
                oQSStepDef.Sections = oStepSections.Sections;
            }
            if (oQSStepDef.iLayoutID > 0)
            {
                oQSStepDef.Layout = (XIDLayout)Get_LayoutDefinition(null, oQSStepDef.iLayoutID.ToString()).oResult; //Common.GetLayoutDetails(oQSStepDef.iLayoutID, 0, 0, 0, null, iUserID, sOrgName, sDatabase);
            }
            return oQSStepDef;
        }

        public XIDQSStep GetStepSectionDefinitionsByStep(int iStepID)
        {
            XIInfraCache oCache = new XIInfraCache();
            string sQSDefinitionQry = "select * from XIQSStepDefinition_T QSSDef " +
                "left join XIStepSectionDefinition_T Sec on QSSDef.ID = Sec.FKiStepDefinitionID " +
                "left join XIFieldDefinition_T XIFD on Sec.ID = XIFD.FKiStepSectionID " +
                "left join XIFieldOrigin_T XIO on XIFD.FKiXIFieldOriginID = XIO.ID " +
                "left join XIDataType_T XIDT on XIO.FKiDataType = XIDT.id " +
                "left join XIFieldOptionList_T OPT on XIO.ID = OPT.FKiQSFieldID " +
                "WHERE QSSDef.ID = @id;";
            var param = new
            {
                id = iStepID
            };
            var lookup = new Dictionary<int, XIDQSStep>();
            using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString))
            {
                Conn.Open();
                var lookup2 = new Dictionary<int, XIDQSSection>();
                var lookup3 = new Dictionary<int, XIDFieldDefinition>();
                var lookup4 = new Dictionary<int, XIDFieldOrigin>();
                var lookup5 = new Dictionary<int, XIDQSStepNavigations>();
                var lookup6 = new Dictionary<int, XIDFieldOptionList>();
                Conn.Query<XIDQSStep, XIDQSSection, XIDFieldDefinition, XIDFieldOrigin, XIDDataType, XIDFieldOptionList, XIDQSStep>(sQSDefinitionQry,
                    (Step, SectionDef, FieldDefinition, FieldOrigin, DataType, OptionList) =>
                    {
                        XIDQSStep oStepDefinition;
                        if (!lookup.TryGetValue(Step.ID, out oStepDefinition))
                        {
                            lookup.Add(Step.ID, oStepDefinition = Step);
                        }
                        XIDQSSection oSection;
                        if (SectionDef != null)
                        {
                            if (!lookup2.TryGetValue(SectionDef.ID, out oSection))
                            {
                                lookup2.Add(SectionDef.ID, oSection = SectionDef);
                                if (SectionDef.sName != null)
                                {
                                    oStepDefinition.Sections[SectionDef.sName] = SectionDef;
                                }
                                else
                                {
                                    oStepDefinition.Sections[SectionDef.ID.ToString() + "_Sec"] = SectionDef;
                                }
                            }
                            XIDFieldDefinition oFieldDefintion;
                            if (FieldDefinition != null)
                            {
                                XIDFieldOrigin oXIFieldOrigin;
                                if (FieldOrigin != null)
                                {
                                    if (!lookup3.TryGetValue(FieldDefinition.ID, out oFieldDefintion))
                                    {
                                        lookup3.Add(FieldDefinition.ID, oFieldDefintion = FieldDefinition);
                                        oSection.FieldDefs[FieldOrigin.sName] = oFieldDefintion;
                                    }

                                    if (!lookup4.TryGetValue(FieldOrigin.ID, out oXIFieldOrigin))
                                    {

                                        if (FieldOrigin.iMasterDataID > 0)
                                        {
                                            //FieldOrigin.ddlFieldOptionList = dbContext.Types.Where(m => m.Code == FieldOrigin.iMasterDataID).ToList().Select(m => new XIDFieldOptionList { sOptionName = m.Expression, sOptionValue = m.ID.ToString() }).ToList();
                                        }
                                        else if (FieldOrigin.FK1ClickID > 0)
                                        {
                                            var o1ClickD = (XID1Click)oCache.GetObjectFromCache(XIConstant.Cache1Click, null, FieldOrigin.FK1ClickID.ToString());
                                            XIDBO oBOD = new XIDBO();
                                            oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, null, o1ClickD.BOID.ToString());
                                            int iDataSource = oBOD.iDataSource;
                                            XIDXI oXID = new XIDXI();
                                            string sConntection = oXID.GetBODataSource(iDataSource, oBOD.FKiApplicationID);
                                            var Connect = new XIDBAPI(sConntection);
                                            o1ClickD.BOD = oBOD;
                                            o1ClickD.Get_1ClickHeadings();
                                            //var bIsPKExists = o1ClickD.TableColumns.ConvertAll(m => m.ToLower()).Contains(oBOD.sPrimaryKey.ToLower());
                                            //if (!bIsPKExists)
                                            //{

                                            //}                                        
                                            var SelFields = string.Join(", ", o1ClickD.TableColumns.ToList());
                                            XIDGroup oGroupD = new XIDGroup();
                                            string FinalString = oGroupD.ConcatanateFields(SelFields, " ");
                                            FinalString = oBOD.sPrimaryKey + "," + FinalString;
                                            var FinalQuery = o1ClickD.AddSelectPart(o1ClickD.Query, FinalString);
                                            //var oBOIns = (DataTable)Connection.ExecuteQuery(o1ClickD.Query);
                                            Dictionary<string, string> DDL = Connect.GetDDLItems(CommandType.Text, FinalQuery, null);
                                            var FKDDL = DDL.Select(m => new XIDFieldOptionList { sOptionName = m.Value, sOptionValue = m.Key }).ToList();
                                            //List<Person> List1 = new List<Person>();
                                            var DDList = FKDDL.Where(m => !string.IsNullOrEmpty(m.sOptionName)).GroupBy(m => m.sOptionName).Select(m => m.FirstOrDefault()).ToList().Select(p => new { key = p.sOptionName, value = p.sOptionValue })
                                                    .ToDictionary(x => x.key, x => x.value);
                                            FieldOrigin.sBOSize = oBOD.sSize;
                                        }
                                        else if (FieldOrigin.FKiBOID > 0)
                                        {
                                            var oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, null, FieldOrigin.FKiBOID.ToString());
                                            string sBODataSource = string.Empty;
                                            if (oBOD != null)
                                            {
                                                FieldOrigin.sBOSize = oBOD.sSize;
                                                //sBODataSource = GetBODataSource(oBOD.iDataSource);
                                                //if (oBOD.Groups.ContainsKey("label"))
                                                //{
                                                //    var oGroupD = oBOD.Groups["label"];
                                                //    var Con = new XIDBAPI(sBODataSource);
                                                //    var LabelGroup = oGroupD.BOFieldNames;
                                                //    if (!string.IsNullOrEmpty(LabelGroup) && oBOD.sSize == "20")
                                                //    {
                                                //        FieldOrigin.sBOSize = oBOD.sSize;
                                                //        string FinalString = oGroupD.ConcatanateFields(LabelGroup, " ");
                                                //        FinalString = oBOD.sPrimaryKey + "," + FinalString;
                                                //        Dictionary<string, string> DDL = Con.SelectDDL(FinalString, oBOD.TableName);
                                                //        var FKDDL = DDL.Select(m => new XIDFieldOptionList { sOptionName = m.Value, sOptionValue = m.Key }).ToList();
                                                //        FieldOrigin.FieldDynamicOptionList = new List<XIDFieldOptionList>();
                                                //        FieldOrigin.FieldDynamicOptionList = FKDDL;
                                                //    }
                                                //}
                                            }
                                        }
                                        lookup4.Add(FieldOrigin.ID, oXIFieldOrigin = FieldOrigin);
                                        oXIFieldOrigin = FieldOrigin;
                                    }

                                    XIDFieldOptionList oOptions;
                                    if (OptionList != null)
                                    {
                                        oOptions = OptionList;
                                        if (!lookup6.TryGetValue(OptionList.ID, out oOptions))
                                        {
                                            lookup6.Add(OptionList.ID, oOptions = OptionList);
                                            if (oXIFieldOrigin.FieldOptionList != null && oXIFieldOrigin.FieldOptionList.Count() > 0)
                                            {
                                                oXIFieldOrigin.FieldOptionList.Add(oOptions);
                                            }
                                            else
                                            {
                                                oXIFieldOrigin.FieldOptionList = new List<XIDFieldOptionList>();
                                                oXIFieldOrigin.FieldOptionList.Add(oOptions);
                                            }
                                        }
                                    }
                                    oFieldDefintion.FieldOrigin = oXIFieldOrigin;
                                    FieldOrigin.DataType = DataType;
                                }
                            }
                        }
                        return oStepDefinition;
                    },
                    param
                    ).AsQueryable();
            }
            var StepDef = lookup.Values.FirstOrDefault();
            var QSLinks = GetSectionQSLinks(StepDef.FKiQSDefintionID);
            if (QSLinks != null)
            {
                foreach (var item in QSLinks.Steps.Where(m => m.Value.ID == iStepID).ToList())
                {
                    if (item.Value != null && item.Value.Sections != null && item.Value.Sections.Count() > 0)
                    {
                        foreach (var osec in item.Value.Sections)
                        {
                            if (osec.Value.QSLinks != null && osec.Value.QSLinks.Count() > 0)
                            {
                                StepDef.Sections.Where(m => m.Value.ID == osec.Value.ID).FirstOrDefault().Value.QSLinks = osec.Value.QSLinks;
                            }
                        }
                    }
                }
            }

            var SectionContent = GetComponentParamsByStep(iStepID);
            if (SectionContent != null)
            {
                foreach (var item in StepDef.Sections)
                {
                    if (item.Value.ComponentDefinition != null)
                    {
                        StepDef.Sections.Values.Where(m => m.ID == item.Value.ID).FirstOrDefault().ComponentDefinition = item.Value.ComponentDefinition;
                    }
                }
            }
            return StepDef;
        }

        private XIDQSStep GetComponentParamsByStep(int iStepID)
        {
            string sQSDefinitionQry = "select * from XIQSStepDefinition_T QSSD " +
                "inner join XIStepSectionDefinition_T Sec on QSSD.ID = Sec.FKiStepDefinitionID " +
                "inner join XIComponentParams_T NVs on sec.ID = NVs.iStepSectionID " +
                "inner join XIComponents_XC_T XC on NVs.FKiComponentID = XC.ID " +
                "where QSSD.ID = @id;";
            var param = new
            {
                id = iStepID
            };
            var lookup2 = new Dictionary<int, XIDQSStep>();
            using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString))
            {
                Conn.Open();
                var lookup = new Dictionary<int, XIDQS>();
                var lookup3 = new Dictionary<int, XIDQSSection>();
                var lookup4 = new Dictionary<int, XIDComponent>();
                Conn.Query<XIDQSStep, XIDQSSection, XIDComponentParam, XIDComponent, XIDQSStep>(sQSDefinitionQry,
                    (Step, SectionDef, ComponentParams, Component) =>
                    {
                        XIDQSStep oStepDefinition;
                        if (!lookup2.TryGetValue(Step.ID, out oStepDefinition))
                        {
                            lookup2.Add(Step.ID, oStepDefinition = Step);
                        }
                        XIDQSSection oSection;
                        if (SectionDef != null)
                        {
                            if (!lookup3.TryGetValue(SectionDef.ID, out oSection))
                            {
                                lookup3.Add(SectionDef.ID, oSection = SectionDef);
                                if (SectionDef.sName != null)
                                {
                                    oStepDefinition.Sections[SectionDef.sName] = SectionDef;
                                }
                                else
                                {
                                    oStepDefinition.Sections[SectionDef.ID.ToString() + "_Sec"] = SectionDef;
                                }

                            }
                            XIDComponent oComponent;
                            if (Component != null)
                            {
                                if (!lookup4.TryGetValue(SectionDef.ID, out oComponent))
                                {
                                    lookup4.Add(SectionDef.ID, oComponent = Component);
                                    oSection.ComponentDefinition = Component;
                                }
                                if (ComponentParams != null)
                                {
                                    if (oSection.ComponentDefinition.Params != null && oSection.ComponentDefinition.Params.Count() > 0)
                                    {
                                        oSection.ComponentDefinition.Params.Add(ComponentParams);
                                    }
                                    else
                                    {
                                        oSection.ComponentDefinition.Params = new List<XIDComponentParam>();
                                        oSection.ComponentDefinition.Params.Add(ComponentParams);
                                    }
                                }
                            }
                        }
                        return oStepDefinition;
                    },
                    param
                    ).AsQueryable();
            }
            var SectionContent = lookup2.Values.FirstOrDefault();
            return SectionContent;
        }

        private XIDQSStep GetQSLinksByStep(int iStepID)
        {
            string sQSDefinitionQry = "select * from XIQSStepDefinition_T QSSD " +
                "inner join XIStepSectionDefinition_T Sec on QSSD.ID = Sec.FKiStepDefinitionID " +
                "left join XIQSLink_T QSlink on sec.ID = QSlink.FKiSectionDefinitionID " +
                "left join XIQSLinkDefinition_T link on QSlink.sCode = link.sCode " +
                "left join XILink_T Xilink on link.FKiXILinkID = Xilink.XiLinkID " +
                "left join XILinkNV_T Xilinknv on Xilink.XILinkID = Xilinknv.XiLinkID " +
                "where QSSD.ID = @id;";
            var param = new
            {
                id = iStepID
            };
            var lookup2 = new Dictionary<int, XIDQSStep>();
            using (SqlConnection Conn = new SqlConnection(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString))
            {
                Conn.Open();
                var lookup = new Dictionary<int, XIDQS>();
                var lookup3 = new Dictionary<int, XIDQSSection>();
                var lookup4 = new Dictionary<long, XIQSLink>();
                var lookup5 = new Dictionary<long, XIQSLinkDefintion>();
                var lookup6 = new Dictionary<int, XILink>();
                Conn.Query<XIDQSStep, XIDQSSection, XIQSLink, XIQSLinkDefintion, XILink, XiLinkNV, XIDQSStep>(sQSDefinitionQry,
                    (Step, SectionDef, QSLink, Link, XILink, XILinkNV) =>
                    {
                        XIDQSStep oStepDefinition = new XIDQSStep();
                        if (Step != null && Step.StatusTypeID == (int)xiEnumSystem.xistatus.xiactive)
                        {
                            if (!lookup2.TryGetValue(Step.ID, out oStepDefinition))
                            {
                                lookup2.Add(Step.ID, oStepDefinition = Step);
                                //oQSDefinition.Steps[oStepDefinition.sName] = oStepDefinition;
                            }
                            XIDQSSection oSection;
                            if (SectionDef != null)
                            {
                                if (!lookup3.TryGetValue(SectionDef.ID, out oSection))
                                {
                                    lookup3.Add(SectionDef.ID, oSection = SectionDef);
                                    oStepDefinition.Sections[SectionDef.ID.ToString() + "_Sec"] = SectionDef;
                                }
                                XIQSLink oSecQSLink;
                                if (QSLink != null)
                                {
                                    XIQSLinkDefintion oXIDLink;
                                    if (Link != null)
                                    {
                                        if (!lookup4.TryGetValue(QSLink.ID, out oSecQSLink))
                                        {
                                            lookup4.Add(QSLink.ID, oSecQSLink = QSLink);
                                            oStepDefinition.QSLinks[XILink.Name] = QSLink;
                                        }
                                        oXIDLink = Link;
                                        oSecQSLink.XiLink[Link.sName] = Link;
                                        if (XILink != null)
                                        {
                                            XILink oXILink;
                                            oXILink = XILink;
                                            oXIDLink.XiLink[XILink.Name] = XILink;
                                            if (XILinkNV != null)
                                            {
                                                if (oXILink.XiLinkNVs != null && oXILink.XiLinkNVs.Count() > 0)
                                                {
                                                    oXILink.XiLinkNVs.Add(XILinkNV);
                                                }
                                                else
                                                {
                                                    oXILink.XiLinkNVs = new List<XiLinkNV>();
                                                    oXILink.XiLinkNVs.Add(XILinkNV);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                        return oStepDefinition;
                    },
                    param,
                     splitOn: "id,id,id,xilinkid,id"
                    ).AsQueryable();
            }
            var SectionContent = lookup2.Values.FirstOrDefault();
            return SectionContent;
        }

        #endregion QuestionSet

        #region Component

        public CResult Get_ComponentDefinition(string sName = "", string sUID = "")
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
                long iID = 0;
                if (long.TryParse(sUID, out iID))
                {
                }
                else
                {
                }


                string PKColumn = string.Empty;
                string PKValue = string.Empty;
                if (iID > 0)
                {
                    PKColumn = "ID";
                    PKValue = iID.ToString();
                }
                else if (iID == 0 && !string.IsNullOrEmpty(sUID) && sUID != "0")
                {
                    PKColumn = "xiguid";
                    PKValue = sUID;
                }
                else if (!string.IsNullOrEmpty(sName))
                {
                    PKColumn = "sName";
                    PKValue = sName;
                }

                XIDComponent oComponent = null;
                Dictionary<string, object> Params = new Dictionary<string, object>();
                Params[PKColumn] = PKValue;
                oComponent = Connection.Select<XIDComponent>("XIComponents_XC_T", Params).FirstOrDefault();
                if (oComponent != null)
                {
                    Dictionary<string, object> NVParams = new Dictionary<string, object>();
                    NVParams["FKiComponentID"] = oComponent.ID;
                    oComponent.NVs = Connection.Select<XIDComponentsNV>("XIComponentNVs_T", NVParams).ToList();
                }
                if (oComponent != null)
                {
                    Dictionary<string, object> ListParams = new Dictionary<string, object>();
                    ListParams["FKiComponentID"] = oComponent.ID;
                    oComponent.Params = Connection.Select<XIDComponentParam>("XIComponentParams_T", ListParams).ToList();
                }
                if (oComponent != null)
                {
                    Dictionary<string, object> ListParams = new Dictionary<string, object>();
                    ListParams["FKiComponentID"] = oComponent.ID;
                    oComponent.Triggers = Connection.Select<XIDComponentTrigger>("XIComponentTriggers_XCT_T", ListParams).ToList();
                }
                if (iID == 0 && !string.IsNullOrEmpty(sUID) && sUID != "0")
                {
                    oComponent.FKiApplicationID = FKiApplicationID;
                }
                oCResult.oResult = oComponent;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while loading Component definition" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oCResult; // always

            //return oURL;
        }

        public CResult Get_ComponentParamsByStep(int iStepID = 0)
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
                int iContextID = 0;
                //Get Section Details
                XIDQSSection oSectionD = null;
                Dictionary<string, object> SecParams = new Dictionary<string, object>();
                SecParams["ID"] = iStepID;//FKiStepDefinitionID
                oSectionD = Connection.Select<XIDQSSection>("XIStepSectionDefinition_T", SecParams).FirstOrDefault();
                Dictionary<string, object> ListParams = new Dictionary<string, object>();
                if (oSectionD != null)
                {
                    iContextID = oSectionD.ID;
                    ListParams["iStepSectionID"] = iContextID;
                }
                else
                {
                    iContextID = iStepID;
                    ListParams["iStepDefinitionID"] = iContextID;
                }
                List<XIDComponentParam> oParams = new List<XIDComponentParam>();
                oParams = Connection.Select<XIDComponentParam>("XIComponentParams_T", ListParams).ToList();
                oCResult.oResult = oParams;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while loading Component Params By Step" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oCResult; // always

            //return oURL;
        }

        public CResult Get_DataTypeDefinition(string sUID = "")
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
                long iID = 0;
                if (long.TryParse(sUID, out iID))
                {
                }
                XIDDataType oComponent = new XIDDataType();
                if (iID > 0)
                {
                    Dictionary<string, object> Params = new Dictionary<string, object>();
                    Params["ID"] = iID;
                    oComponent = Connection.Select<XIDDataType>("XIDataType_T", Params).FirstOrDefault();
                }
                if (iID == 0)
                {
                    oComponent.FKiApplicationID = FKiApplicationID;
                }
                oCResult.oResult = oComponent;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while loading Component definition" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oCResult; // always

            //return oURL;
        }

        #endregion Component
        #region Document
        public CResult Get_ContentDefinition(int iTemplateID = 0, string sTemplateName = "", int iBOID = 0, int iContentType = 0)
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
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Started ContentDefinition Loading" });
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
                XIContentEditors oDocumentContent = null;
                List<XIContentEditors> oDocumentContentDef = new List<XIContentEditors>();
                Dictionary<string, object> Params = new Dictionary<string, object>();
                if (iTemplateID != 0)
                {
                    Params["ID"] = iTemplateID;
                }
                if (!string.IsNullOrEmpty(sTemplateName))
                {
                    Params["Name"] = sTemplateName;
                }
                if (iBOID != 0)
                {
                    Params["BO"] = iBOID;
                }
                if (iContentType != 0)
                {
                    Params["Type"] = iContentType;
                }
                //Params["OrganizationID"] = iOrgID;
                //Params["ID"] = iContentID;
                oDocumentContent = Connection.Select<XIContentEditors>("XITemplate_T", Params).FirstOrDefault();
                Params = new Dictionary<string, object>();
                if (oDocumentContent != null && oDocumentContent.ID != 0 && iTemplateID == 0)
                {
                    iTemplateID = oDocumentContent.ID;
                }
                Params["iParentID"] = iTemplateID;
                oDocumentContentDef = Connection.Select<XIContentEditors>("XITemplate_T", Params).ToList();
                List<XIContentEditors> oContentDef = new List<XIContentEditors>();
                if (oDocumentContentDef != null && oDocumentContentDef.Count() > 0)
                {
                    oDocumentContentDef.Insert(0, oDocumentContent);
                }
                else
                {
                    oDocumentContentDef = new List<XIContentEditors>();
                    oDocumentContentDef.Add(oDocumentContent);
                }
                oCResult.oResult = oDocumentContentDef;
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
        #endregion

        #region IOServerDetails

        public CResult Get_IOSServerDetails(int iOrgID, int iServerID = 0)
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
                XIIOServerDetails oIOServerDetails = null;
                Dictionary<string, object> Params = new Dictionary<string, object>();
                if (iServerID > 0)
                {
                    Params["ID"] = iServerID;
                }
                else
                {
                    Params["OrganizationID"] = iOrgID;
                }
                oIOServerDetails = Connection.Select<XIIOServerDetails>("XIIOServerDetails_T", Params).FirstOrDefault();
                oCResult.oResult = oIOServerDetails;
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

        #endregion IOServerDetails
        #region RightMenuDetails
        public CResult Get_RightMenuDefinition(string MenuName = "")
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
                List<XIMenu> oRightMenus = new List<XIMenu>();

                if (!string.IsNullOrEmpty(MenuName))
                {
                    string sRoleID = string.Empty;
                    if (!string.IsNullOrEmpty(sUserID))
                    {
                        Dictionary<string, object> UserParams = new Dictionary<string, object>();
                        UserParams["UserID"] = sUserID;
                        cConnectionString oConString = new cConnectionString();
                        string sConString = oConString.ConnectionString(sCoreDatabase);
                        XIDBAPI sConnection = new XIDBAPI(sConString);
                        sRoleID = sConnection.SelectString("RoleID", "XIAppUserRoles_AUR_T", UserParams).ToString();
                    }

                    XIMenu oRightMenuTrees = null;
                    Dictionary<string, object> Params = new Dictionary<string, object>();
                    Params["RootName"] = MenuName;
                    Params["ParentID"] = "#";
                    if (!string.IsNullOrEmpty(sRoleID))
                    {
                        Params["RoleID"] = sRoleID;
                    }
                    oRightMenuTrees = Connection.Select<XIMenu>("XIMenu_T", Params).FirstOrDefault();
                    string MainID = oRightMenuTrees.MenuID;
                    Params = new Dictionary<string, object>();
                    Params["ParentID"] = MainID;
                    Params["StatusTypeID"] = "10";
                    Params["RootName"] = MenuName;
                    if (!string.IsNullOrEmpty(sRoleID))
                    {
                        Params["OrgID"] = iOrgID.ToString();
                        Params["RoleID"] = sRoleID;
                    }
                    oRightMenus = Connection.Select<XIMenu>("XIMenu_T", Params).ToList();
                    oRightMenus = Countdata(oRightMenus, sCoreDatabase, MenuName);
                }

                oCResult.oResult = oRightMenus;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while loading Menu definition" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oCResult; // always
        }
        public List<XIMenu> Countdata(List<XIMenu> Menus, string sCoreDatabase, string sMenuName)
        {
            foreach (var items in Menus)
            {
                var ID = items.MenuID;
                Dictionary<string, object> Params = new Dictionary<string, object>();
                Params["ParentID"] = ID;
                Params["StatusTypeID"] = "10";
                if (!string.IsNullOrEmpty(sMenuName))
                {
                    Params["RootName"] = sMenuName;
                }
                items.SubGroups = Connection.Select<XIMenu>("XIMenu_T", Params).ToList();
                if (items.SubGroups.Count() > 0)
                {
                    Countdata(items.SubGroups, sCoreDatabase, sMenuName);
                }
            }
            return Menus;
        }
        public List<XIDInbox> InboxCountdata(List<XIDInbox> Menus, string sCoreDatabase)
        {
            foreach (var items in Menus)
            {
                var ID = items.ID;
                Dictionary<string, object> Params = new Dictionary<string, object>();
                Params["ParentID"] = ID;
                Params["StatusTypeID"] = "10";
                items.SubGroups = Connection.Select<XIDInbox>("XIInbox_T", Params).ToList();
                if (items.SubGroups.Count() > 0)
                {
                    InboxCountdata(items.SubGroups, sCoreDatabase);
                }
            }
            return Menus;
        }
        #endregion

        public CResult Get_AutoCompleteList(string sUID, string sBOName, List<CNV> oParams = null, int iBOSize = 0)
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
                XIInfraCache oCache = new XIInfraCache();
                var oBOD = new XIDBO();
                string sBODID = string.Empty;
                string s1ClickID = string.Empty;
                string sBO = string.Empty;
                var sType = sUID.Split('-');
                if (sType[0] == "bo")
                {
                    sBODID = sType[1];
                }
                else if (sType[0] == "1click")
                {
                    s1ClickID = sType[1];
                }
                int iBODID;
                int.TryParse(sBODID, out iBODID);
                if (!string.IsNullOrEmpty(sBOName) && sBOName.Split('-').Length > 1)
                {
                    sBO = sBOName.Split('-')[1];
                }
                int i1ClickID;
                int.TryParse(s1ClickID, out i1ClickID);
                XID1Click o1ClickD = new XID1Click();
                if (iBODID > 0)
                {
                    oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, null, iBODID.ToString());
                }
                else if (!string.IsNullOrEmpty(sBO))
                {
                    oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, sBO, "0");
                }
                else if (i1ClickID > 0)
                {
                    o1ClickD = (XID1Click)oCache.GetObjectFromCache(XIConstant.Cache1Click, null, i1ClickID.ToString());
                    oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, null, o1ClickD.BOID.ToString());
                }
                string sBODataSource = string.Empty;
                if (oBOD != null && (iBODID > 0 || !string.IsNullOrEmpty(sBO)))
                {
                    sBODataSource = GetBODataSource(oBOD.iDataSource, oBOD.FKiApplicationID);
                    if (oBOD.Groups.ContainsKey("label"))
                    {
                        List<XIDFieldOptionList> FKDDL = new List<XIDFieldOptionList>();
                        var oGroupD = oBOD.Groups["label"];
                        var Con = new XIDBAPI(sBODataSource);
                        var LabelGroup = oGroupD.BOFieldNames;
                        if (!string.IsNullOrEmpty(LabelGroup))
                        {
                            string FinalString = oGroupD.ConcatanateFields(LabelGroup, " ");
                            FinalString = oBOD.sPrimaryKey + "," + FinalString;
                            Dictionary<string, string> DDL = Con.SelectDDL(FinalString, oBOD.TableName);
                            FKDDL = DDL.Select(m => new XIDFieldOptionList { sOptionName = m.Value, sOptionValue = m.Key }).ToList();
                        }
                        if (oBOD.sSize == "10")
                        {
                            oCResult.oResult = FKDDL;
                            oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                        }
                        else if (oBOD.sSize == "20")
                        {
                            var DDList = FKDDL.Where(m => !string.IsNullOrEmpty(m.sOptionName)).GroupBy(m => m.sOptionName).Select(m => m.FirstOrDefault()).ToList().Select(p => new { key = p.sOptionName, value = p.sOptionValue })
                                .ToDictionary(x => x.key, x => x.value);
                            oCResult.oResult = DDList;
                            oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                        }
                    }
                }
                else
                {
                    int iDataSource = oBOD.iDataSource;
                    XIDXI oXID = new XIDXI();
                    string sConntection = oXID.GetBODataSource(iDataSource, oBOD.FKiApplicationID);
                    var Connect = new XIDBAPI(sConntection);
                    o1ClickD.BOD = oBOD;
                    o1ClickD.Get_1ClickHeadings();
                    //var bIsPKExists = o1ClickD.TableColumns.ConvertAll(m => m.ToLower()).Contains(oBOD.sPrimaryKey.ToLower());
                    //if (!bIsPKExists)
                    //{

                    //}    
                    var PrimaryKey = "";
                    if (o1ClickD.bIsMultiBO == true)
                    {
                        var fields = "[" + o1ClickD.FromBos + "].";
                        fields = string.Join(" [" + o1ClickD.FromBos + "].", o1ClickD.TableColumns.ToList());
                        var TableColumns = fields.Split(' ');
                        o1ClickD.TableColumns = TableColumns.ToList();
                        PrimaryKey = "[" + o1ClickD.FromBos + "]." + oBOD.sPrimaryKey;
                    }
                    var SelFields = string.Join(", ", o1ClickD.TableColumns.ToList());
                    XIDGroup oGroupD = new XIDGroup();
                    string FinalString = oGroupD.ConcatanateFields(SelFields, " ");
                    FinalString = (o1ClickD.bIsMultiBO == true ? PrimaryKey : "[" + oBOD.TableName + "]." + oBOD.sPrimaryKey) + "," + FinalString;
                    var FinalQuery = o1ClickD.AddSelectPart(o1ClickD.Query, FinalString);
                    XIDStructure oXIDStructure = new XIDStructure();
                    if (oParams != null && oParams.Count() > 0)
                    {
                        FinalQuery = oXIDStructure.ReplaceExpressionWithCacheValue(FinalQuery, oParams);
                    }
                    //var oBOIns = (DataTable)Connection.ExecuteQuery(o1ClickD.Query);
                    Dictionary<string, string> DDL = Connect.GetDDLItems(CommandType.Text, FinalQuery, null);
                    List<XIDFieldOptionList> FKDDL = new List<XIDFieldOptionList>();
                    FKDDL = DDL.Select(m => new XIDFieldOptionList { sOptionName = m.Value, sOptionValue = m.Key }).ToList();
                    if (iBOSize != 0)
                    {
                        oBOD.sSize = Convert.ToString(iBOSize);
                    }
                    if (oBOD.sSize == "10")
                    {
                        oCResult.oResult = FKDDL;
                        oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                    }
                    else if (oBOD.sSize == "20")
                    {
                        var DDList = FKDDL.Where(m => !string.IsNullOrEmpty(m.sOptionName)).GroupBy(m => m.sOptionName).Select(m => m.FirstOrDefault()).ToList().Select(p => new { key = p.sOptionName, value = p.sOptionValue })
                        .ToDictionary(x => x.key, x => x.value);
                        oCResult.oResult = DDList;
                        oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                    }
                }
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
        public CResult Get_DependencyAutoCompleteSearchList(string sAttribute, string sUID, string sBOName, string sValue, string sParentBO, int iBOSize = 0)
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
                XIInfraCache oCache = new XIInfraCache();
                var oBOD = new XIDBO();
                string sBODID = string.Empty;
                string s1ClickID = string.Empty;
                //string sBO = string.Empty;
                var sType = sUID.Split('-');
                sBODID = sType[1];
                int iBODID;
                int.TryParse(sBODID, out iBODID);
                //if (!string.IsNullOrEmpty(sBOName) && sBOName.Split('-').Length > 1)
                //{
                //    sBO = sBOName.Split('-')[1];
                //}
                XID1Click o1ClickD = new XID1Click();
                if (iBODID > 0)
                {
                    oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, null, iBODID.ToString());
                }
                else if (!string.IsNullOrEmpty(sBOName))
                {
                    oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, sBOName, "0");
                }
                string sBODataSource = string.Empty;
                if (oBOD != null)
                {
                    var oAttr = oBOD.Attributes.Values.Where(m => m.sFKBOName == sParentBO).FirstOrDefault();
                    if (oAttr != null)
                    {
                        string sWhrCl = string.Empty;
                        if (!string.IsNullOrEmpty(sValue) && sValue != "-1")
                        {
                            sWhrCl = oAttr.Name + " = " + sValue;
                        }
                        sBODataSource = GetBODataSource(oBOD.iDataSource, oBOD.FKiApplicationID);
                        if (oBOD.Groups.ContainsKey("label"))
                        {
                            List<XIDFieldOptionList> FKDDL = new List<XIDFieldOptionList>();
                            var oGroupD = oBOD.Groups["label"];
                            var Con = new XIDBAPI(sBODataSource);
                            var LabelGroup = oGroupD.BOFieldNames;
                            if (!string.IsNullOrEmpty(LabelGroup))
                            {
                                string FinalString = oGroupD.ConcatanateFields(LabelGroup, " ");
                                FinalString = oBOD.sPrimaryKey + "," + FinalString;
                                Dictionary<string, string> DDL = Con.SelectDDL(FinalString, oBOD.TableName, sWhrCl);
                                FKDDL = DDL.Select(m => new XIDFieldOptionList { sOptionName = m.Value, sOptionValue = m.Key }).ToList();
                            }
                            if (oBOD.sSize == "10")
                            {
                                oCResult.oResult = FKDDL;
                                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                            }
                            else if (oBOD.sSize == "20")
                            {
                                var DDList = FKDDL.Where(m => !string.IsNullOrEmpty(m.sOptionName)).GroupBy(m => m.sOptionName).Select(m => m.FirstOrDefault()).ToList().Select(p => new { key = p.sOptionName, value = p.sOptionValue })
                                    .ToDictionary(x => x.key, x => x.value);
                                oCResult.oResult = DDList;
                                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                            }
                        }
                    }
                }
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

        public CResult Get_SourceDefinition(string sUID = "")
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
                long iID = 0;
                if (long.TryParse(sUID, out iID))
                {
                }
                else
                {
                }


                string PKColumn = string.Empty;
                string PKValue = string.Empty;
                if (iID > 0)
                {
                    PKColumn = "ID";
                    PKValue = iID.ToString();
                }
                else if (iID == 0 && !string.IsNullOrEmpty(sUID) && sUID != "0")
                {
                    PKColumn = "xiguid";
                    PKValue = sUID;
                }
                XIDSource oSourceD = null;
                Dictionary<string, object> Params = new Dictionary<string, object>();
                Params[PKColumn] = PKValue;
                oSourceD = Connection.Select<XIDSource>("XISource_T", Params).FirstOrDefault();
                oCResult.oResult = oSourceD;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while loading Source definition" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oCResult; // always

            //return oURL;
        }

        public CResult Get_ClassDefinition(string sUID = "")
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
                long iID = 0;
                if (long.TryParse(sUID, out iID))
                {
                }
                else
                {
                }


                string PKColumn = string.Empty;
                string PKValue = string.Empty;
                if (iID > 0)
                {
                    PKColumn = "ID";
                    PKValue = iID.ToString();
                }
                else if (iID == 0 && !string.IsNullOrEmpty(sUID) && sUID != "0")
                {
                    PKColumn = "xiguid";
                    PKValue = sUID;
                }
                XIDClass oSourceD = null;
                Dictionary<string, object> Params = new Dictionary<string, object>();
                Params[PKColumn] = PKValue;
                oSourceD = Connection.Select<XIDClass>("XIClass_T", Params).FirstOrDefault();
                oCResult.oResult = oSourceD;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while loading Class definition" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oCResult; // always

            //return oURL;
        }

        #region BOStructureDefination

        public CResult Get_XIBOStructureDefinition(int iBOID, long iStructureID, string sType)
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
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Started ContentDefinition Loading" });
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
                List<XIDStructure> XIStrTrees = new List<XIDStructure>();
                List<XIDStructure> Tree = new List<XIDStructure>();

                //QuestionSet Steps DropDown
                Dictionary<string, object> Params5 = new Dictionary<string, object>();
                Dictionary<string, string> QSSteps = new Dictionary<string, string>();
                var oStepDef = Connection.Select<XIDQSStep>("XIQSStepDefinition_T", Params5).ToList();

                //All BO DropDowns
                List<XIDropDown> oBOD = new List<XIDropDown>();
                Dictionary<string, object> oBOParam = new Dictionary<string, object>();
                var oBODef = Connection.Select<XIDBO>("XIBO_T_N", oBOParam).ToList();

                Dictionary<string, object> oAttrParam = new Dictionary<string, object>();
                var oAttrDef = Connection.Select<XIDAttribute>("XIBOAttribute_T_N", oAttrParam).ToList();

                Dictionary<string, object> Params = new Dictionary<string, object>();
                if (sType != "Create")
                {
                    Params["FKiParentID"] = '#';
                    if (iStructureID > 0)
                    {
                        Params["ID"] = iStructureID;
                        XIStrTrees = Connection.Select<XIDStructure>("XIBOStructure_T", Params).ToList();
                    }

                    if (iBOID > 0)
                    {
                        Params["BOID"] = iBOID;
                        XIStrTrees = Connection.Select<XIDStructure>("XIBOStructure_T", Params).ToList();
                    }

                    if (Tree.Count() == 0)
                    {
                        Tree.Add(new XIDStructure());
                    }

                    foreach (var item in XIStrTrees)
                    {
                        item.FKiStepDefinitionName = oStepDef.Where(m => m.ID == Convert.ToInt32(item.FKiStepDefinitionID)).Select(m => m.sName).FirstOrDefault();
                    }

                    if (XIStrTrees != null && XIStrTrees.Count() > 0)
                    {
                        Tree = XITree(XIStrTrees, new List<XIDStructure>());
                    }
                }

                if (sType == "Create")
                {
                    List<XIDAttribute> BOFldList = new List<XIDAttribute>();
                    var oBO = oBODef.Where(m => m.BOID == iBOID).FirstOrDefault();
                    BOFldList = oAttrDef.Where(m => m.FKTableName == oBO.TableName).ToList();
                    Tree.Add(new XIDStructure { ID = oBO.BOID, FKiParentID = "#", sName = oBO.LabelName, sBO = oBO.Name, BOID = iBOID });
                    foreach (var items in BOFldList)
                    {
                        var BO = oBODef.Where(m => m.BOID == items.BOID).FirstOrDefault();
                        if (BO != null)
                        {
                            if (Tree.Where(m => m.ID == items.BOID).FirstOrDefault() == null)
                            {
                                if (BO.sType != xiBOTypes.Reference.ToString() && BO.sType != xiBOTypes.Enum.ToString() && BO.sType != xiBOTypes.XISystem.ToString() && BO.sType != xiBOTypes.Technical.ToString())
                                {
                                    XIDStructure oStru = new XIDStructure();
                                    oStru.ID = items.BOID;
                                    oStru.sBO = BO.Name;
                                    oStru.sName = BO.LabelName;
                                    oStru.FKiParentID = iBOID.ToString();
                                    oStru.BOID = BO.BOID;
                                    Tree.Add(oStru);
                                }
                            }
                        }
                    }
                }

                //Adding All Steps's to Tree
                foreach (var items in oStepDef)
                {
                    QSSteps[items.sName] = items.ID.ToString();
                }
                Tree.FirstOrDefault().AllQSSteps = QSSteps;
                //Adding All BO's to Tree
                oBOD = oBODef.Select(m => new XIDropDown { Value = m.BOID, text = m.Name }).ToList();
                Tree.FirstOrDefault().BOList = oBOD;

                oCResult.oResult = Tree;
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

        public List<XIDStructure> XITree(List<XIDStructure> XIStrTrees, List<XIDStructure> Tree)
        {
            foreach (var items in XIStrTrees)
            {
                Tree.Add(items);
                var ID = items.ID;
                Dictionary<string, object> Params = new Dictionary<string, object>();
                Params["FKiParentID"] = ID.ToString();
                var oStruDef = Connection.Select<XIDStructure>("XIBOStructure_T", Params).ToList();
                var SubXITreeNodes = oStruDef.OrderBy(m => m.iOrder).ToList();
                if (SubXITreeNodes.Count() > 0)
                {
                    XITree(SubXITreeNodes, Tree);
                }
            }
            return Tree;
        }

        #endregion BOStructureDefination

        public CResult Get_FieldOriginDefinition(string sName, string sUID = "")
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
                long iID = 0;
                if (long.TryParse(sUID, out iID))
                {
                }
                else
                {
                }


                string PKColumn = string.Empty;
                string PKValue = string.Empty;
                if (iID > 0)
                {
                    PKColumn = "ID";
                    PKValue = iID.ToString();
                }
                else if (iID == 0 && !string.IsNullOrEmpty(sUID))
                {
                    PKColumn = "xiguid";
                    PKValue = sUID;
                }
                else if (!string.IsNullOrEmpty(sName))
                {
                    PKColumn = "sName";
                    PKValue = sName;
                }
                XIDFieldOrigin oFOrgin = new XIDFieldOrigin();
                Dictionary<string, object> Params = new Dictionary<string, object>();
                Params[PKColumn] = PKValue;
                if (PKValue != "0")
                {
                    oFOrgin = Connection.Select<XIDFieldOrigin>("XIFieldOrigin_T", Params).FirstOrDefault();
                }
                oCResult.oResult = oFOrgin;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while loading field origin definition" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oCResult; // always

            //return oURL;
        }

        #region Section Definition

        public CResult Get_QSSectionDefinition(string sUID = "")
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
                long iID = 0;
                if (long.TryParse(sUID, out iID))
                {
                }
                else
                {
                }


                string PKColumn = string.Empty;
                string PKValue = string.Empty;
                if (iID > 0)
                {
                    PKColumn = "ID";
                    PKValue = iID.ToString();
                }
                else if (iID == 0 && !string.IsNullOrEmpty(sUID))
                {
                    PKColumn = "xiguid";
                    PKValue = sUID;
                }
                XIDQSSection oSecD = new XIDQSSection();
                Dictionary<string, object> Params = new Dictionary<string, object>();
                Params[PKColumn] = PKValue;
                if (PKValue != "0")
                {
                    oSecD = Connection.Select<XIDQSSection>("XIStepSectionDefinition_T", Params).FirstOrDefault();
                }
                oCResult.oResult = oSecD;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while loading field origin definition" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oCResult; // always

            //return oURL;
        }

        public CResult Get_QSSectionsAll()
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
                List<XIDQSSection> oSecD = new List<XIDQSSection>();
                Dictionary<string, object> Params = new Dictionary<string, object>();
                Params[XIConstant.Key_XIDeleted] = "0";
                oSecD = Connection.Select<XIDQSSection>("XIStepSectionDefinition_T", Params).ToList();
                oCResult.oResult = oSecD;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while loading field origin definition" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oCResult; // always

            //return oURL;
        }

        #endregion Section Definition

        #region BOAction

        public CResult Get_BOActionDefinition(string sName, string sUID = "")
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
                long iID = 0;
                if (long.TryParse(sUID, out iID))
                {
                }
                else
                {
                }


                string PKColumn = string.Empty;
                string PKValue = string.Empty;
                if (iID > 0)
                {
                    PKColumn = "ID";
                    PKValue = iID.ToString();
                }
                else if (iID == 0 && !string.IsNullOrEmpty(sUID))
                {
                    PKColumn = "xiguid";
                    PKValue = sUID;
                }
                else if (!string.IsNullOrEmpty(sName))
                {
                    PKColumn = "sName";
                    PKValue = sName;
                }
                XIDBOAction oBOAction = new XIDBOAction();
                Dictionary<string, object> Params = new Dictionary<string, object>();
                Params[PKColumn] = PKValue;
                if (PKValue != "0")
                {
                    oBOAction = Connection.Select<XIDBOAction>("XIBOAction_T", Params).FirstOrDefault();
                    if (oBOAction.ID > 0)
                    {
                        Params = new Dictionary<string, object>();
                        Params["FKiBOActionID"] = oBOAction.ID;
                        var oBOActionNV = Connection.Select<XIDBOActionNV>("XIBOActionNV_T", Params).ToList();
                        if (oBOActionNV != null && oBOActionNV.Count() > 0)
                        {
                            oBOAction.ActionNV = oBOActionNV;
                        }
                    }
                }
                oCResult.oResult = oBOAction;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while loading field origin definition" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oCResult; // always

            //return oURL;
        }

        #endregion BOAction

        #region BODefault

        public CResult Get_BODefault(string sName, string sUID = "")
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
                long iID = 0;
                if (long.TryParse(sUID, out iID))
                {
                }
                else
                {
                }


                string PKColumn = string.Empty;
                string PKValue = string.Empty;
                if (iID > 0)
                {
                    PKColumn = "FKiBOID";
                    PKValue = iID.ToString();
                }
                else if (iID == 0 && !string.IsNullOrEmpty(sUID))
                {
                    PKColumn = "xiguid";
                    PKValue = sUID;
                }
                else if (!string.IsNullOrEmpty(sName))
                {
                    PKColumn = "sName";
                    PKValue = sName;
                }
                XIDBODefault BODefault = new XIDBODefault();
                Dictionary<string, object> Params = new Dictionary<string, object>();
                Params[PKColumn] = PKValue;
                if (PKValue != "0")
                {
                    BODefault = Connection.Select<XIDBODefault>("XIBOUIDefault_T", Params).FirstOrDefault();
                }
                oCResult.oResult = BODefault;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while loading field origin definition" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oCResult; // always

            //return oURL;
        }

        #endregion BODefault

        #region XIWidget

        public CResult Get_WidgetDefinition(string sName, string sUID = "")
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
                long iID = 0;
                if (long.TryParse(sUID, out iID))
                {
                }
                else
                {
                }


                string PKColumn = string.Empty;
                string PKValue = string.Empty;
                if (iID > 0)
                {
                    PKColumn = "ID";
                    PKValue = iID.ToString();
                }
                else if (iID == 0 && !string.IsNullOrEmpty(sUID))
                {
                    PKColumn = "xiguid";
                    PKValue = sUID;
                }
                else if (!string.IsNullOrEmpty(sName))
                {
                    PKColumn = "sName";
                    PKValue = sName;
                }
                XIDWidget oWidgetD = new XIDWidget();
                Dictionary<string, object> Params = new Dictionary<string, object>();
                Params[PKColumn] = PKValue;
                if (PKValue != "0")
                {
                    oWidgetD = Connection.Select<XIDWidget>("XIWidget_T", Params).FirstOrDefault();
                }
                oCResult.oResult = oWidgetD;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while loading field origin definition" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oCResult; // always

            //return oURL;
        }

        #endregion XIWidget

        #region Algorithm

        public CResult Get_XIAlgorithmDefinition(string sName, string sUID = "")
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
                long iID = 0;
                if (long.TryParse(sUID, out iID))
                {
                }
                else
                {
                }


                string PKColumn = string.Empty;
                string PKValue = string.Empty;
                if (iID > 0)
                {
                    PKColumn = "ID";
                    PKValue = iID.ToString();
                }
                else if (iID == 0 && !string.IsNullOrEmpty(sUID))
                {
                    PKColumn = "xiguid";
                    PKValue = sUID;
                }
                else if (!string.IsNullOrEmpty(sName))
                {
                    PKColumn = "sName";
                    PKValue = sName;
                }
                XIDAlgorithm oAlgoD = new XIDAlgorithm();
                Dictionary<string, object> Params = new Dictionary<string, object>();
                Params[PKColumn] = PKValue;
                if (PKValue != "0")
                {
                    oAlgoD = Connection.Select<XIDAlgorithm>("XIAlgorithm_T", Params).FirstOrDefault();
                    if (oAlgoD.ID > 0)
                    {
                        Params = new Dictionary<string, object>();
                        Params["FKiAlgorithmID"] = oAlgoD.ID;
                        Params[XIConstant.Key_XIDeleted] = 0;
                        var Lines = Connection.Select<XIDAlgorithmLine>("XIAlgorithmLines_T", Params).ToList();
                        if (Lines != null && Lines.Count() > 0)
                        {
                            oAlgoD.Lines = new List<XIDAlgorithmLine>();
                            oAlgoD.Lines = Lines;
                        }
                    }
                }
                oCResult.oResult = oAlgoD;
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while loading xialgorithm definition" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oCResult; // always

            //return oURL;
        }

        #endregion Algorithm

        #region Autocomplete

        public CResult Get_XILinks(int iApplicationID)
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
                oTrace.oParams.Add(new CNV { sName = "iApplicationID", sValue = iApplicationID.ToString() });
                if (iApplicationID > 0)//check mandatory params are passed or not
                {
                    Dictionary<string, object> oLinks = new Dictionary<string, object>();
                    oLinks["FKiApplicationID"] = iApplicationID;
                    Dictionary<string, string> XiLinks = new Dictionary<string, string>();
                    var oXiLinkDef = Connection.Select<XILink>("XILink_T", oLinks).ToList();
                    foreach (var items in oXiLinkDef)
                    {
                        XiLinks[items.Name] = items.Name;
                    }
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                    oCResult.oResult = XiLinks;
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory Param: ApplicationID is missing";
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

        public CResult Get_QuestionSets(int iApplicationID)
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
                oTrace.oParams.Add(new CNV { sName = "iApplicationID", sValue = iApplicationID.ToString() });
                if (iApplicationID > 0)//check mandatory params are passed or not
                {
                    List<XIDropDown> DDL = new List<XIDropDown>();
                    Dictionary<string, object> oLinks = new Dictionary<string, object>();
                    oLinks["FKiApplicationID"] = iApplicationID;
                    var oXiLinkDef = Connection.Select<XIDQS>("XIQSDefinition_T", oLinks).ToList();
                    foreach (var items in oXiLinkDef)
                    {
                        DDL.Add(new XIDropDown { text = items.sName, Value = items.ID });
                    }
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                    oCResult.oResult = DDL;
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory Param: ApplicationID is missing";
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

        public CResult Get_QSSteps(int iQSDID)
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
                oTrace.oParams.Add(new CNV { sName = "iQSDID", sValue = iQSDID.ToString() });
                if (iQSDID > 0)//check mandatory params are passed or not
                {
                    List<XIDropDown> DDL = new List<XIDropDown>();
                    Dictionary<string, object> oLinks = new Dictionary<string, object>();
                    oLinks["FKiQSDefintionID"] = iQSDID;
                    var oXiLinkDef = Connection.Select<XIDQSStep>("XIQSStepDefinition_T", oLinks).ToList();
                    foreach (var items in oXiLinkDef)
                    {
                        DDL.Add(new XIDropDown { text = items.sName, Value = items.ID });
                    }
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                    oCResult.oResult = DDL;
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory Param: QSDID is missing";
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

        public CResult Get_QSSections(int iStepID)
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
                oTrace.oParams.Add(new CNV { sName = "iStepID", sValue = iStepID.ToString() });
                if (iStepID > 0)//check mandatory params are passed or not
                {
                    List<XIDropDown> DDL = new List<XIDropDown>();
                    Dictionary<string, object> oLinks = new Dictionary<string, object>();
                    oLinks["FKiStepDefinitionID"] = iStepID;
                    var oXiLinkDef = Connection.Select<XIDQSSection>("XIStepSectionDefinition_T", oLinks).ToList();
                    foreach (var items in oXiLinkDef)
                    {
                        if (!string.IsNullOrEmpty(items.sName))
                        {
                            DDL.Add(new XIDropDown { text = items.sName, Value = items.ID });
                        }
                        else
                        {
                            DDL.Add(new XIDropDown { text = items.ID.ToString(), Value = items.ID });
                        }
                    }
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                    oCResult.oResult = DDL;
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory Param: iStepID is missing";
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

        public CResult Get_QSLinks(int iApplicationID)
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
                oTrace.oParams.Add(new CNV { sName = "iApplicationID", sValue = iApplicationID.ToString() });
                if (iApplicationID > 0)//check mandatory params are passed or not
                {
                    List<XIDropDown> DDL = new List<XIDropDown>();
                    Dictionary<string, object> oLinks = new Dictionary<string, object>();
                    oLinks["FKiApplicationID"] = iApplicationID;
                    oLinks[XIConstant.Key_XIDeleted] = "0";
                    var oXiLinkDef = Connection.Select<XIQSLinkDefintion>("XIQSLinkDefinition_T", oLinks).ToList();
                    foreach (var items in oXiLinkDef)
                    {
                        DDL.Add(new XIDropDown { text = items.sName, Value = Convert.ToInt32(items.ID) });
                    }
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                    oCResult.oResult = DDL;
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory Param: ApplicationID is missing";
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

        #endregion Autocomplete

        #region WhiteList

        public CResult Get_WhiteList()
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
                Dictionary<string, object> Data = new Dictionary<string, object>();
                var o1ClickD = (XID1Click)oCache.GetObjectFromCache(XIConstant.Cache1Click, "BO WhiteList Cache");
                var o1ClickC = (XID1Click)o1ClickD.Clone(o1ClickD);
                var Result = o1ClickC.OneClick_Run();
                if (Result != null && Result.Count() > 0)
                {
                    foreach (var items in Result.Values.ToList())
                    {
                        var iBODID = items.AttributeI("FKiBODID").sValue;
                        var iRoleID = items.AttributeI("FKiRoleID").sValue;
                        var iOrgID = items.AttributeI("FKiOrgID").sValue;
                        var iAppID = items.AttributeI("FKiAppID").sValue;
                        if (!string.IsNullOrEmpty(iBODID) && !string.IsNullOrEmpty(iRoleID) && !string.IsNullOrEmpty(iOrgID) && !string.IsNullOrEmpty(iAppID))
                        {
                            Data[iBODID + "_" + iRoleID + "_" + iOrgID + "_" + iAppID] = items;
                        }
                    }
                }
                oCResult.oResult = Data;
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        #endregion WhiteList

        #region Config Setting

        public CResult Get_ConfigSetting(string sUID)
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
                int iAppID = 0;
                int iOrgID = 0;
                string sKey = string.Empty;
                var Splits = sUID.Split('_');
                if (Splits.Count() == 3)
                {
                    var AppID = Splits[0];
                    var OrgID = Splits[1];
                    sKey = Splits[2];
                    int.TryParse(AppID, out iAppID);
                    int.TryParse(OrgID, out iOrgID);
                }
                if (iAppID > 0 && iOrgID > 0 && !string.IsNullOrEmpty(sKey))
                {
                    XIIXI oXII = new XIIXI();
                    //List<CNV> oWhrPrms = new List<CNV>();
                    //oWhrPrms.Add(new CNV { sName = "FKiAppID", sValue = iAppID.ToString() });
                    //oWhrPrms.Add(new CNV { sName = "FKiOrgID", sValue = iOrgID.ToString() });
                    //oWhrPrms.Add(new CNV { sName = "sKey", sValue = sKey });
                    Dictionary<string, object> oWhrPrms = new Dictionary<string, object>();
                    oWhrPrms["FKiAppID"] = iAppID;
                    oWhrPrms["FKiOrgID"] = iOrgID;
                    oWhrPrms["sKey"] = sKey;
                    var sValue = Connection.SelectString("svalue", "XIConfig_T", oWhrPrms);
                    //var oBOI = oXII.BOI("XIConfig_T", null, null, oWhrPrms);
                    if (!string.IsNullOrEmpty(sValue))
                    {
                        oCResult.oResult = sValue;
                        oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
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
                SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        #endregion Config Setting

        #region 1Link Access

        public CResult Get_1LinkAccess()
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Get list of Link access data to add into cache";//expalin about this method logic
            try
            {
                XIInfraCache oCache = new XIInfraCache();
                Dictionary<string, object> Data = new Dictionary<string, object>();
                var o1ClickD = (XID1Click)oCache.GetObjectFromCache(XIConstant.Cache1Click, "1Link Access Cache");
                var o1ClickC = (XID1Click)o1ClickD.Clone(o1ClickD);
                var Result = o1ClickC.OneClick_Run();
                if (Result != null && Result.Count() > 0)
                {
                    foreach (var items in Result.Values.ToList())
                    {
                        var iXILinkID = items.AttributeI("FKiXILinkID").sValue;
                        var iRoleID = items.AttributeI("FKiRoleID").sValue;
                        var iOrgID = items.AttributeI("FKiOrgID").sValue;
                        var iAppID = items.AttributeI("FKiAppID").sValue;
                        if (!string.IsNullOrEmpty(iXILinkID) && !string.IsNullOrEmpty(iRoleID) && !string.IsNullOrEmpty(iOrgID) && !string.IsNullOrEmpty(iAppID))
                        {
                            Data[iXILinkID + "_" + iRoleID + "_" + iOrgID + "_" + iAppID] = items;
                        }
                    }
                }
                oCResult.oResult = Data;
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        #endregion 1Link Access

        #region 1Query Access

        public CResult Get_1QueryAccess()
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Get list of 1Query access data to add into cache";//expalin about this method logic
            try
            {
                XIInfraCache oCache = new XIInfraCache();
                Dictionary<string, object> Data = new Dictionary<string, object>();
                var o1ClickD = (XID1Click)oCache.GetObjectFromCache(XIConstant.Cache1Click, "1Query Permission Cache");
                var o1ClickC = (XID1Click)o1ClickD.Clone(o1ClickD);
                var Result = o1ClickC.OneClick_Run();
                if (Result != null && Result.Count() > 0)
                {
                    foreach (var items in Result.Values.ToList())
                    {
                        var i1ClickID = items.AttributeI("FKi1ClickID").sValue;
                        var iRoleID = items.AttributeI("FKiRoleID").sValue;
                        var iOrgID = items.AttributeI("FKiOrgID").sValue;
                        var iAppID = items.AttributeI("FKiAppID").sValue;
                        if (!string.IsNullOrEmpty(i1ClickID) && !string.IsNullOrEmpty(iRoleID) && !string.IsNullOrEmpty(iOrgID) && !string.IsNullOrEmpty(iAppID))
                        {
                            Data[i1ClickID + "_" + iRoleID + "_" + iOrgID + "_" + iAppID] = items;
                        }
                    }
                }
                oCResult.oResult = Data;
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString(); oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return oCResult;
        }

        #endregion 1Query Access

        #region SendGrid
        public CResult Get_SendGridAccountDetails(string sSendGridAccountID)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            long iTraceLevel = 10;
            oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiInProcess;
            if (iTraceLevel > 0)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Started Execution" });
            }
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
                if (!string.IsNullOrEmpty(sSendGridAccountID))
                {
                    Dictionary<string, object> oParams = new Dictionary<string, object>();
                    oParams["iSGADID"] = sSendGridAccountID;
                    XIDBAPI Connection = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString);
                    var oResult = Connection.Select<XIDSendGridAccountDetails>("SendGridAccountDetails_T", oParams);
                    if (oResult.Count() > 0)
                    {
                        oCResult.oResult = oResult.FirstOrDefault();
                        oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;

                    }
                    else
                        oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;

                }

            }
            catch (Exception ex)
            {
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString();
                oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                SaveErrortoDB(oCResult);
            }
            return oCResult;
        }

        public CResult Get_SendGridTemplate(string sTemplateName)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            long iTraceLevel = 10;
            oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiInProcess;
            if (iTraceLevel > 0)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Started Execution" });
            }
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
                if (!string.IsNullOrEmpty(sTemplateName))
                {

                    var oParams = new Dictionary<string, object>();
                    oParams["sTemplateName"] = sTemplateName;
                    //oParams["FKiSGADID"] = SendGridAccountID;
                    var oTemplateResult = Connection.Select<XIDSendGridTemplate>("SendGridTemplate_T", oParams);
                    if (oTemplateResult.Count() > 0)
                    {
                        oCResult.oResult = oTemplateResult.FirstOrDefault();
                        oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                    }
                    else

                        oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;


                }
                else
                    oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;

            }
            catch (Exception ex)
            {
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString();
                oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                SaveErrortoDB(oCResult);
            }
            return oCResult;
        }
        #endregion SendGrid

    }
}