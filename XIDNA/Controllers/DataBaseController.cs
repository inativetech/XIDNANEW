using System;
using System.Web.Mvc;
using XIDNA.Models;
using System.Linq;
using XIDNA.ViewModels;
using System.Collections.Generic;
using System.Web.Script.Services;
using System.Web.Script.Serialization;
using System.Text;
using XICore;
using System.Data;
using System.Data.SqlClient;
using XISystem;
using System.Configuration;
using System.Reflection;
using System.Collections.Specialized;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using XIDatabase;
using System.Diagnostics;

namespace XIDNA.Controllers
{
    public class DataBaseController : Controller
    {
        XIDatabaseSchema oSchema = new XIDatabaseSchema();
        ModelDbContext dbContext = new ModelDbContext();
        XIDefinitionBase oXID = new XIDefinitionBase();
        #region Source_and_Target_Compare
        //public ActionResult Index()
        //{
        //    XIDatabaseInfo res = new XIDatabaseInfo();
        //    //var Databases = oSchema.GetDatabaseConnections();
        //    //ViewBag.DataBases = Databases;
        //    res.Application = dbContext.XIApplications.ToList();
        //    res.Environment = dbContext.XIEnvironment.ToList();
        //    //res.Application = fds;
        //    return View(res);
        //}
        [HttpPost]
        public ActionResult GetDatabases(XIDatabaseInfo DBInfo)
        {
            var DataBases = DBInfo.GetDataBases();

            return Json(DataBases, JsonRequestBehavior.AllowGet);
        }
        [HttpPost]
        public ActionResult CheckConnection(XIDatabaseInfo DBInfo)
        {
            try
            {
                bool check = DBInfo.CheckConnection();
                return Json(new { bIsConnected = check, DBInfo.ConnectionString }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpPost]
        public ActionResult SchemaCompare(string sSource, string sTarget, string[] BONames = null)
        {
            try
            {
                //sSource = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SourceDatabase;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
                //sTarget = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=TargetDatabase;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
                ViewBag.IsFromBO = false;
                CResult result = new CResult();
                if (BONames != null)
                {
                    ViewBag.IsFromBO = true;
                    result = oSchema.NewBOStructureTablesSchemaCompare(sSource, BONames, true);
                }
                else
                    result = oSchema.NewDatabasesCompare(sSource, sTarget, BONames);
                return PartialView("_CompareDatabases", JsonSerialize(result.oResult));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpPost]
        public ActionResult GenerateScript(string SelectedTables, string sSource, string sTarget)
        {
            try
            {
                //sSource = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SourceDatabase;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
                //sTarget = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=TargetDatabase;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
                string sScript = string.Empty;
                if (!string.IsNullOrEmpty(SelectedTables) && !string.IsNullOrEmpty(sSource) && !string.IsNullOrEmpty(sTarget))
                {
                    var serializer = new JavaScriptSerializer();
                    serializer.MaxJsonLength = Int32.MaxValue;
                    var Schema = serializer.Deserialize<List<XIDatabaseSchema>>(SelectedTables);
                    if (Schema.Count > 0)
                    {
                        sScript = "/************************** " + sTarget + " ************************/\n\n";
                        oSchema.ConnectionString = sSource;
                        sScript += oSchema.CreateAndDropTableScript(Schema.Where(m => m.iActionType == 10).Select(m => m).ToList(), 10);
                        //var ddsScript = oSchema.AlterTablesFromDatabase(Schema.Where(m => m.iActionType == 20).Select(m => m.Columns).ToList();

                        var oAlter = Schema.Where(m => m.iActionType == 20).Select((m) => new { sAlterScript = m.GenerateAlterTablesScript() }).Select(m => m.sAlterScript).ToList();
                        sScript += string.Join("", oAlter);
                        oSchema.ConnectionString = sTarget;
                        sScript += oSchema.CreateAndDropTableScript(Schema.Where(m => m.iActionType == 30).Select(m => m).ToList(), 30);
                    }

                }
                return PartialView("_SchemaPreview", sScript);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion Source_and_Target_Compare

        #region BO's_and_Selft_Database_compare
        public ActionResult SelfDatabaseCompare(int iApplicationID = 0)
        {
            try
            {
                ModelDbContext DbContext = new ModelDbContext();
                if (iApplicationID > 0)
                {
                    var DDDataBases = DbContext.XIDataSources.Where(m => m.FKiApplicationID == iApplicationID).ToList().Select(s => new VMDropDown { ID = s.ID, text = s.sName }).ToList();
                    return Json(DDDataBases, JsonRequestBehavior.AllowGet);
                }
                var DDApplications = DbContext.XIApplications.Select(s => new VMDropDown { ID = s.ID, text = s.sApplicationName }).ToList();
                return View(DDApplications);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public ActionResult BOSchemaCompare(int iDataSource)
        {
            try
            {
                ViewBag.IsFromBO = true;
                var result = oSchema.BOSchemaCompare(iDataSource);
                return PartialView("_CompareDatabases", JsonSerialize(result));
            }
            catch (Exception ex)
            {
                throw ex;
            }

        }
        [HttpPost]
        public ActionResult GenerateBOScript(string SelectedTables, int InstanceID, string sBoName, string sDBType)
        {
            try
            {
                var serializer = new JavaScriptSerializer();
                serializer.MaxJsonLength = Int32.MaxValue;
                var Schema = serializer.Deserialize<List<XIDatabaseSchema>>(SelectedTables);
                var dicTables = Schema.GroupBy(m => m.iActionType).ToDictionary(m => m.Key, m => m.ToList());
                string sScript = oSchema.GenerateBOScript(dicTables);
                if (!string.IsNullOrEmpty(sScript))
                {

                    XIIXI oIXI = new XIIXI();
                    var oBOI = oIXI.BOI(sBoName, InstanceID.ToString());
                    if (oBOI != null && oBOI.Attributes.ContainsKey("sScript"))
                    {
                        //oBOI.Attributes["sScript"].sValue = sScript;
                        //oBOI.Attributes["sScript"].bDirty = true;
                        //oBOI.Save(oBOI);
                        XIEnvironmentTemplate TemplateSaving = new XIEnvironmentTemplate();
                        TemplateSaving.FKiTemplateID = InstanceID;
                        TemplateSaving.sTemplateType = sDBType;
                        TemplateSaving.sScript = sScript;

                        XIConfigs Saving = new XIConfigs();
                        Saving.sBOName = "Master Environment";
                        //Saving.sBOName = sBoName;
                        Saving.Save_EnvironmentTemplate(TemplateSaving);

                    }
                    return PartialView("_SchemaPreview", sScript);
                }
                else
                    return Json(null, JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion BO's_and_Selft_Database_compare

        #region Validate_and_Execute_Script

        [HttpPost]
        public ActionResult ValidateScript(string sScript, string sTarget = null, int iDataSource = 0)
        {
            try
            {
                return Json(oSchema.ValidateScript(sScript, sTarget, iDataSource), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpPost]
        public ActionResult Execute(string sScript, string sTarget = null, int iDataSource = 0)
        {
            try
            {
                return Json(oSchema.Execute(sScript, sTarget, iDataSource), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion Validate_and_Execute_Script

        #region DataBase_Data_Compare
        [HttpGet]
        public ActionResult GetBOStructureDetails(string sSource, int ID = 0)
        {
            try
            {
                return Json(oSchema.GetBOStructureDetails(sSource, ID), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public ActionResult GetBOStructures(string sSource, int ID = 0)
        {
            try
            {
                return Json(oSchema.GetBOStructures(sSource, ID), JsonRequestBehavior.AllowGet);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpPost]
        public ActionResult GetBOTreeStructure(string sSource, int ID)
        {
            try
            {
                return PartialView("_GetBOTreeStructure", oSchema.GetTreeStructure(sSource, ID));
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        //[HttpPost]
        //public ActionResult DataCompareWithBONames(string[] BONames)
        //{
        //    try
        //    {
        //        return PartialView("_DataCompare", oSchema.DataCompareWithBONames(BONames));
        //    }
        //    catch(Exception ex)
        //    {
        //        throw ex;
        //    }
        //}
        [HttpPost]
        public ActionResult DataCompare(jQueryDataTableParamModels Params, string sSource, string sTarget, string CompareWith, string ParentIDs = null, string[] BONames = null, string StructureID = null, int iInstanceID = 0, string sStructureName = null, int iBOID = 0)
        {
            try
            {
                //sSource = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=SourceCheckData;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
                //sTarget = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=TargetCheckData;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";
                //if (BONames != null)
                //{
                //    sSource = "Data Source=192.168.7.222;initial catalog=XIDNAQA;User Id=cruser; Password=cruser; MultipleActiveResultSets=True;";
                //    sTarget = "Data Source=192.168.7.222;initial catalog=XIDNAQAV2;User Id=cruser; Password=cruser; MultipleActiveResultSets=True;";
                //}
                var RecordCounts = oSchema.NewDataCompare(Params, sSource, sTarget, CompareWith, BONames, iInstanceID, sStructureName, iBOID, ParentIDs);
                return Json(RecordCounts);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpPost]
        public ActionResult TableSelection(string sSource, bool AllBos = false)
        {
            try
            {
                List<string> TableList = new List<string>();
                var RecordCounts = oSchema.NewTableSelection(sSource, AllBos);
                var jsonResult = Json(oSchema.NewTableSelection(sSource, AllBos), JsonRequestBehavior.AllowGet);
                jsonResult.MaxJsonLength = int.MaxValue;
                return jsonResult;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpPost]
        public ActionResult GetTableRecords(string sTableName, string sActionType,string sDBType,string iInstanceID)
        {
            try
            {
                var Result = oSchema.NewGetTableRecords(sTableName, sActionType, sDBType);
                if (!string.IsNullOrEmpty(iInstanceID))
                {
                XIEnvironmentTemplate TemplateSaving = new XIEnvironmentTemplate();
                TemplateSaving.FKiTemplateID = Convert.ToInt32(iInstanceID);
                TemplateSaving.sTemplateType = sDBType;
                TemplateSaving.sScript = Result.oResult.ToString();

                XIConfigs Saving = new XIConfigs();
                Saving.sBOName = "Master Environment";
                //Saving.sBOName = sBoName;
                Saving.Save_EnvironmentTemplate(TemplateSaving);
                 }
                var JsonResult = Json(Result.oResult, JsonRequestBehavior.AllowGet);
                JsonResult.MaxJsonLength = int.MaxValue;
                return JsonResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        #endregion DataBase_Data_Compare

        #region Database_Data_Insert_Update
        public ActionResult InsertUpdateTarget(string iInstanceID)
        {
            try
            {
                var Result = oSchema.NewInsertUpdateTarget();
                if (!string.IsNullOrEmpty(iInstanceID))
                {
                    XIEnvironmentTemplate TemplateSaving = new XIEnvironmentTemplate();
                    TemplateSaving.FKiTemplateID = Convert.ToInt32(iInstanceID);
                    TemplateSaving.sTemplateType = "Data";
                    TemplateSaving.sScript = Result.oResult.ToString();

                    XIConfigs Saving = new XIConfigs();
                    Saving.sBOName = "Master Environment";
                    //Saving.sBOName = sBoName;
                    Saving.Save_EnvironmentTemplate(TemplateSaving);
                }
                return PartialView("_SchemaPreview", Result.oResult.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        [HttpPost]
        public ActionResult UpdateSourceTarget(string sTableName, List<string> GUIDs, string sActionType, string sDBType,string iInstanceID)
        {
            try
            {
                var ScriptResult = oSchema.NewUpdateSourceTarget(sTableName, GUIDs, sActionType, sDBType);
                if (!string.IsNullOrEmpty(iInstanceID))
                {
                    XIEnvironmentTemplate TemplateSaving = new XIEnvironmentTemplate();
                    TemplateSaving.FKiTemplateID = Convert.ToInt32(iInstanceID);
                    TemplateSaving.sTemplateType = sDBType;
                    TemplateSaving.sScript = ScriptResult.oResult.ToString();

                    XIConfigs Saving = new XIConfigs();
                    Saving.sBOName = "Master Environment";
                    //Saving.sBOName = sBoName;
                    Saving.Save_EnvironmentTemplate(TemplateSaving);
                }
                //}
                return PartialView("_SchemaPreview", ScriptResult.oResult.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        #endregion Database_Data_Insert_Update

        public string JsonSerialize(object obj)
        {
            try
            {
                var serializer = new JavaScriptSerializer();
                serializer.MaxJsonLength = Int32.MaxValue;
                return serializer.Serialize(obj);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public CResult DBReset(List<CNV> oParams)
        {
            CResult oCResult = new CResult();
            XIInfraCache oCache = new XIInfraCache();
            XIDefinitionBase oXID = new XIDefinitionBase();
            try
            {
                string sBOName = oParams.Where(x => x.sName.ToLower() == "sBOName".ToLower()).Select(t => t.sValue).First();
                string sGroupName = oParams.Where(x => x.sName.ToLower() == "sGroupName".ToLower()).Select(t => t.sValue).First();
                XIIBO oEnvironmentI = new XIIBO();
                XIDBO oEnvironmentBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, sBOName, null);
                oEnvironmentI.BOD = oEnvironmentBOD;
                oEnvironmentI.LoadBOI(sGroupName);
                XIIBO oEnvironmentDetails = new XIIBO();
                oEnvironmentDetails.BOD = oEnvironmentBOD;
                foreach (var item in oEnvironmentI.Attributes)
                {
                    oEnvironmentDetails.SetAttribute(item.Key, oParams.Where(x => x.sName.ToLower() == item.Key.ToLower()).Select(t => t.sValue).First());
                }
                oCResult = oEnvironmentDetails.Save(oEnvironmentDetails);
                string sTargetDB = oParams.Where(x => x.sName.ToLower() == "sDestinationLocation".ToLower()).Select(t => t.sValue).FirstOrDefault();
                string sSourceDB = oParams.Where(x => x.sName.ToLower() == "sSourceLocation".ToLower()).Select(t => t.sValue).FirstOrDefault();
                XIConfigs oXIConfigs = new XIConfigs();
                oXIConfigs.sAppName = sTargetDB;
                oXIConfigs.iAppID = Convert.ToInt32(oParams.Where(x => x.sName.ToLower() == "FKiApplicationID".ToLower()).Select(t => t.sValue).FirstOrDefault());
                oXIConfigs.Save_DataSource(oParams);
                string FileName = null; string[] Tables = null;
                DataSet ds = new DataSet();
                string sDataBaseServer = ConfigurationManager.AppSettings["DataBaseServer"];
                string sDataBaseUser = ConfigurationManager.AppSettings["DataBaseUser"];
                string sDataBasePassword = ConfigurationManager.AppSettings["DataBasePassword"];
                string connectionString = "Data Source=" + sDataBaseServer + ";Initial Catalog=" + sTargetDB + ";User Id=" + sDataBaseUser + "; Password=" + sDataBasePassword + "";  //;Integrated Security=True;Persist Security Info=True
                SqlConnection con = new SqlConnection(connectionString);
                //render table name from database
                //string sqlTable = "SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='BASE TABLE'";
                //con.Open();
                //SqlDataAdapter da = new SqlDataAdapter();
                //SqlCommand cmd = new SqlCommand(sqlTable, con);
                //cmd.CommandType = CommandType.Text;
                //da.SelectCommand = cmd;
                //da.Fill(ds);
                //con.Close();
                StringBuilder sb = new StringBuilder();
                StringBuilder sbCreate = new StringBuilder();

                Server srv = new Server(new Microsoft.SqlServer.Management.Common.ServerConnection(sDataBaseServer, sDataBaseUser, sDataBasePassword));
                Database dbs = srv.Databases[sSourceDB];
                ScriptingOptions options = new ScriptingOptions();
                options.ScriptData = true;
                options.ScriptDrops = false;
                //options.FileName = FileName;
                //options.EnforceScriptingOptions = true;
                options.ScriptSchema = true;
                options.IncludeHeaders = false;
                options.AppendToFile = false;
                options.Indexes = true;
                options.WithDependencies = true;
                con.Open();

                foreach (Table myTable in dbs.Tables)
                {
                    /* Generating IF EXISTS and DROP command for tables */
                    //var tableScripts = myTable.EnumScript(options);
                    //foreach (string script in tableScripts)
                    //{
                    //    sb.Append(script);
                    //}

                    /* Generating CREATE TABLE command */
                    var CreatetableScripts = myTable.EnumScript(options);
                    sbCreate = new StringBuilder();
                    foreach (string script in CreatetableScripts)
                    {
                        sbCreate.Append(script + "\\");
                    }
                    //StringCollection stringCollection = new StringCollection();
                    //stringCollection.AddRange(sb.ToString().Split(' ').ToArray());
                    StringCollection CreatestringCollection = new StringCollection();
                    CreatestringCollection.AddRange(sbCreate.ToString().Split('\\').ToArray());
                    Server server = new Server(new ServerConnection(con));
                    //server.ConnectionContext.ExecuteNonQuery(stringCollection);
                    server.ConnectionContext.ExecuteNonQuery(CreatestringCollection);
                }
                con.Close();
                oCResult.oResult = "Success";
                oCResult.sMessage = "Copied successfully.";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                oCResult.LogToFile();
                //oXID.SaveErrortoDB(oCResult, 0);
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error While Copying Data base process" });
                oCResult.sMessage = "ERROR:  " + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                oXID.SaveErrortoDB(oCResult, 0);
            }
            return oCResult;
        }
        public ActionResult GetApplications()
        {
            var model = dbContext.XIApplications.Select(x => new { x.ID, x.sApplicationName }).ToList();
            return Json(model, JsonRequestBehavior.AllowGet);
        }


        //***************** Schema Coompare Button ******************************
        [HttpPost]
        public ActionResult NewDataSchemaComapare(List<XIIAttribute> oNVParams, string sBoName)
        {
            var result = new object();
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Schema Comparison for two Data bases";//expalin about this method logic
            try
            {
                oTrace.oParams.Add(new CNV { sName = "sBoName", sValue = sBoName });
                if (oNVParams != null && !string.IsNullOrEmpty(sBoName))//check mandatory params are passed or not
                {
                    string InstanceID = oNVParams.Where(x => x.sName.ToLower() == "id".ToLower()).Select(t => t.sValue).First();
                    string sApplicationID = oNVParams.Where(x => x.sName.ToLower() == "FKiApplicationID".ToLower()).Select(t => t.sValue).First();
                    string sSourceID = oNVParams.Where(x => x.sName.ToLower() == "FKiSourceEnvID".ToLower()).Select(t => t.sValue).First();
                    string sTargetID = oNVParams.Where(x => x.sName.ToLower() == "FKiTargetEnvID".ToLower()).Select(t => t.sValue).First();
                    string sTargetDB = oNVParams.Where(x => x.sName.ToLower() == "FKiTargetDB".ToLower()).Select(t => t.sValue).First();
                    string sSourceDB = oNVParams.Where(x => x.sName.ToLower() == "FKiSourceDB".ToLower()).Select(t => t.sValue).First();
                    XIInfraCache oCache = new XIInfraCache();
                    XIIXI oIXI = new XIIXI();
                    //************Getting Sorce and Target ConnectionString from "XIEnvironment DataBases" BO***********
                    var sSourceDetails = oIXI.BOI("XIEnvironment DataBases", sSourceDB);
                    var sTargetDetails = oIXI.BOI("XIEnvironment DataBases", sTargetDB);
                    var sSource = sSourceDetails.Attributes.Values.Where(n => n.sName.ToLower() == "sConnectionString".ToLower()).Select(i => i.sValue).FirstOrDefault(); //"Data Source=" + SourceDataSourceID + ";initial catalog=" + sSourceDBCon + ";User Id=" + SourceUserID + "; Password=" + SourcePassword + "; MultipleActiveResultSets=True;";
                    var sTarget = sTargetDetails.Attributes.Values.Where(n => n.sName.ToLower() == "sConnectionString".ToLower()).Select(i => i.sValue).FirstOrDefault();//"Data Source=" + TargetDataSourceID + ";initial catalog=" + sTargetDataBase + ";User Id=" + TargetUserID + "; Password=" + TargetPassword + "; MultipleActiveResultSets=True;";

                    ViewBag.IsFromBo = true;
                    ViewBag.iInstanceID = InstanceID;
                    ViewBag.sBoName = sBoName;
                    oCR = oSchema.NewDatabasesCompare(sSource, sTarget, null, false);
                    if (oCR.bOK && oCR.oResult != null)
                    {
                        oTrace.oTrace.Add(oCR.oTrace);
                        if (oCR.bOK && oCR.oResult != null)
                        {
                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                            oCResult= oCR;
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
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory Param: oNVParams = " + oNVParams + " or sBoName = " + sBoName + " is missing";
                }
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString();
                oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXID.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return PartialView("_CompareDatabases", JsonSerialize(oCResult.oResult));

        }


        //*********************** Data Compare button ********************************************
        [HttpPost]
        public ActionResult NewDBDataCompare(jQueryDataTableParamModels Params, string sSourceDB, string sTargetDB, string sSourceEnvID, string sTargetEnvID, string sApplicationID, string ParentID, string iInstanceID, string StructureName, string Boid,string BONames)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Two Data Base data compare";//expalin about this method logic
           // VMDTResponses RecordCounts = new VMDTResponses();
            try
            {
                oTrace.oParams.Add(new CNV { sName = "sSourceDB", sValue = sSourceDB });
                oTrace.oParams.Add(new CNV { sName = "sTargetDB", sValue = sTargetDB });
                oTrace.oParams.Add(new CNV { sName = "sSourceEnvID", sValue = sSourceEnvID });
                oTrace.oParams.Add(new CNV { sName = "sTargetEnvID", sValue = sTargetEnvID });
                oTrace.oParams.Add(new CNV { sName = "sApplicationID", sValue = sApplicationID });
                oTrace.oParams.Add(new CNV { sName = "ParentID", sValue = ParentID });
                oTrace.oParams.Add(new CNV { sName = "iInstanceID", sValue = iInstanceID });
                oTrace.oParams.Add(new CNV { sName = "StructureName", sValue = StructureName });
                oTrace.oParams.Add(new CNV { sName = "Boid", sValue = Boid });
                if (!string.IsNullOrEmpty(sSourceDB) && !string.IsNullOrEmpty(sTargetDB) && !string.IsNullOrEmpty(sSourceEnvID) && !string.IsNullOrEmpty(sTargetEnvID))//check mandatory params are passed or not
                {
                  string CompareWith = "XIGUID";
                    XIInfraCache oCache = new XIInfraCache();
                    XIIXI oIXI = new XIIXI();
                    var sSourceDetails = oIXI.BOI("XIEnvironment DataBases", sSourceDB);

                    var sTargetDetails = oIXI.BOI("XIEnvironment DataBases", sTargetDB);


                    var sSource = sSourceDetails.Attributes.Values.Where(n => n.sName.ToLower() == "sConnectionString".ToLower()).Select(i => i.sValue).FirstOrDefault(); //"Data Source=" + SourceDataSourceID + ";initial catalog=" + sSourceDBCon + ";User Id=" + SourceUserID + "; Password=" + SourcePassword + "; MultipleActiveResultSets=True;";
                    var sTarget = sTargetDetails.Attributes.Values.Where(n => n.sName.ToLower() == "sConnectionString".ToLower()).Select(i => i.sValue).FirstOrDefault();//"Data Source=" + TargetDataSourceID + ";initial catalog=" + sTargetDataBase + ";User Id=" + TargetUserID + "; Password=" + TargetPassword + "; MultipleActiveResultSets=True;";
                    
                    if (Params.iDisplayLength == 0)
                        Params.iDisplayLength = 10;
                    string[] sBoNames=null;
                    if (!string.IsNullOrEmpty(BONames))
                        sBoNames = BONames.Split(',');
                    oCResult = oSchema.NewDataCompare(Params, sSource, sTarget, CompareWith, sBoNames, Convert.ToInt32(iInstanceID), StructureName, Convert.ToInt32(Boid), ParentID);
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory Param: sSourceDB = " + sSourceDB + " or sTargetDB = " + sTargetDB + " or sSourceEnvID = " + sSourceEnvID + " or sTargetEnvID = " + sTargetEnvID + " or sApplicationID = " + sApplicationID + " or ParentID = " + ParentID + " or iInstanceID = " + iInstanceID + " or StructureName = " + StructureName + " or Boid = " + Boid + " is missing";
                }
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString();
                oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXID.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            return Json(oCResult.oResult);
        }

        //******** This method is used for loadded BOs and BO's againest Structure**********
        [HttpPost]
        public ActionResult NewDBTableSelection(List<XIIAttribute> oNVParams, bool AllBos = false)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Structure Data Comparison for selected Tables";//expalin about this method logic
            try
            {
                oTrace.oParams.Add(new CNV { sName = "AllBos", sValue = AllBos.ToString() });
                if (oNVParams!=null)//check mandatory params are passed or not
                {

                    XIIXI oIXI = new XIIXI();
                    string sApplicationID = oNVParams.Where(x => x.sName.ToLower() == "FKiApplicationID".ToLower()).Select(t => t.sValue).First();
                    string sSourceID = oNVParams.Where(x => x.sName.ToLower() == "FKiSourceEnvID".ToLower()).Select(t => t.sValue).First();
                    string sSourceDB = oNVParams.Where(x => x.sName.ToLower() == "FKiSourceDB".ToLower()).Select(t => t.sValue).First();

                    var sSourceDetails = oIXI.BOI("XIEnvironment DataBases", sSourceDB);
                    var sSource = sSourceDetails.Attributes.Values.Where(n => n.sName.ToLower() == "sConnectionString".ToLower()).Select(i => i.sValue).FirstOrDefault(); //"Data Source=" + SourceDataSourceID + ";initial catalog=" + sSourceDBCon + ";User Id=" + SourceUserID + "; Password=" + SourcePassword + "; MultipleActiveResultSets=True;";
                    oCR = oSchema.NewTableSelection(sSource, AllBos);
                    if (oCR.bOK && oCR.oResult != null)
                    {
                        oTrace.oTrace.Add(oCR.oTrace);
                        if (oCR.bOK && oCR.oResult != null)
                        {
                            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiSuccess;
                            oCResult = oCR;
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
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory Param: AllBos = " + AllBos + " is missing";
                }
            }
            catch (Exception ex)
            {
                oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiError;
                int line = (new StackTrace(ex, true)).GetFrame(0).GetFileLineNumber();
                oTrace.sMessage = "Line No:" + line + " - " + ex.ToString();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.sCategory = ex.GetType().ToString();
                oCResult.iCriticality = (int)xiEnumSystem.EnumXIErrorCriticality.Exception;
                oXID.SaveErrortoDB(oCResult);
            }
            watch.Stop();
            oTrace.iLapsedTime = TimeSpan.FromMilliseconds(watch.ElapsedMilliseconds).TotalSeconds;
            oCResult.oTrace = oTrace;
            var jsonResult = Json(oCResult.oResult, JsonRequestBehavior.AllowGet);
            jsonResult.MaxJsonLength = int.MaxValue;
            return jsonResult;


        }


        [HttpPost]
        //******** loading Tree Structure *********
        public ActionResult DBGetBOTreeStructure(string sSource, int ID)
        {
            try
            {
                XIIXI oIXI = new XIIXI();
                var sSourceDetails = oIXI.BOI("XIEnvironment DataBases", sSource);
                var sSourceCon = sSourceDetails.Attributes.Values.Where(n => n.sName.ToLower() == "sConnectionString".ToLower()).Select(i => i.sValue).FirstOrDefault(); //"Data Source=" + SourceDataSourceID + ";initial catalog=" + sSourceDBCon + ";User Id=" + SourceUserID + "; Password=" + SourcePassword + "; MultipleActiveResultSets=True;";
                var StructureResult = oSchema.NewGetTreeStructure(sSourceCon, ID);
                return PartialView("_GetBOTreeStructure", (List<XIDStructure>)StructureResult.oResult);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}