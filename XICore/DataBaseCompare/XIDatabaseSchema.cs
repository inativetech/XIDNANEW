using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;
using XIDatabase;
using XISystem;

namespace XICore
{
    public class XIDatabaseSchema : XIDatabaseInfo
    {
        public int iActionType { get; set; } //***********Add =10, Changes = 20, Delete = 30***********
        public Dictionary<string, List<XITableColumns>> DTableColumns { get; set; }
        public string sTableName { get; set; }
        public List<XITableColumns> Columns { get; set; }
        public List<XITableColumns> AllColumns { get; set; }
        List<XIDStructure> oXIDStructureData = new List<XIDStructure>();
        public string MainTableName { get; set; }
        XIDXI oXID = new XIDXI();
        List<Dictionary<string, List<XIIBO>>> ListofoInstance = new List<Dictionary<string, List<XIIBO>>>();
        string[] SqlStringDataTypes = new string[] { "varchar", "nvarchar", "nchar", "char" };
        #region Get_Database_Connection_from_Configurations

        public object GetDatabaseConnections()
        {
            CResult oCResult = new CResult();
            try
            {
                XID1Click oClick = new XID1Click();
                string sConnection = ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString;
                oClick.sConnectionString = sConnection;
                oClick.Query = "SELECT * FROM XIDatabaseConnections_T";
                List<XIIBO> oRecords = oClick.OneClick_Run().Values.ToList();
                var result = oRecords.Select(m => m.Attributes).Select(m => new
                {
                    Name = m["sName"].sValue,
                    Value = m["sConnectionString"].sValue,
                    bIsConfigDatabase = m["bIsConfigDatabase"].sValue
                }).ToList();
                return result;

            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                SaveErrortoDB(oCResult);
                throw;
            }
        }

        #endregion Get_Database_Connection_from_Configurations

        #region DataBase_Compare_and_Script_Generation
        /*::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
          :: Get Source and Target Tables Column Properties as Datatable and Convert to List  ::
          :: and Compare the Soruce and Target Lists are equla or not                         :: 
          ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::*/


        public CResult NewDatabasesCompare(string sSource, string sTarget, string[] BONames, bool bIsForDataCompare = false)
        {
            var Result = new object();
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Data Base schema Comparison";//expalin about this method logic
            try
            {
                if (sSource != null)
                {
                    List<XITableColumns> LSourceTColumns = new List<XITableColumns>();
                    List<XITableColumns> LTargetTColumns = new List<XITableColumns>();
                    Query = sSystemTableColumnsQuery;
                    if (bIsForDataCompare)
                        Query = sTablesWithPrimaryKeyQuery;
                    //***************Get Sournce Tables,Columns and Properties********************
                    ConnectionString = sSource;
                    SqlCon = null;
                    CommandType = CommandType.Text;
                    LSourceTColumns = DataTableToList<XITableColumns>(ExecuteReaderToDataTable());

                    //**************Get Target Tables, Columns and Properties**********************
                    ConnectionString = sTarget;
                    SqlCon = null;
                    LTargetTColumns = DataTableToList<XITableColumns>(ExecuteReaderToDataTable());
                    if (bIsForDataCompare)
                    {
                        //****** each BO wise Columns Compare ******
                        Result = LSourceTColumns.Where(m => LTargetTColumns.Any(s => s.sTableName == m.sTableName)).ToList();
                        oCR.oResult = Result;
                    }
                    else
                        //******** Entire DB BO Columns Compare*******
                        oCR = NewCompare(LSourceTColumns, LTargetTColumns);
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
                    oTrace.sMessage = "Mandatory Param: sSource = " + sSource + " or sSource = " + sSource + " is missing";
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
            return oCResult;
        }

        /*::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
          :: Compare Two List of Tables Column details and Separete the Lists as Add, change and delete Types ::
          :: and build the object for tree Structure and returned it                                          ::
          ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::*/

        public CResult NewCompare(List<XITableColumns> oSourceList, List<XITableColumns> oTargetList)
        {

            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Compare Data Bases";//expalin about this method logic
            try
            {
                if (oTargetList != null && oTargetList != null)//check mandatory params are passed or not
                {
                    List<string> LSourceTableNames = oSourceList.GroupBy(m => m.sTableName.Replace(" ", "").ToLower()).Select(m => m.FirstOrDefault().sTableName).ToList();
                    List<string> LTargetTableNames = oTargetList.GroupBy(m => m.sTableName.Replace(" ", "").ToLower()).Select(m => m.FirstOrDefault().sTableName).ToList();

                    List<string> list = LSourceTableNames.Union(LTargetTableNames).ToList();

                    List<string> TargetAdd = LSourceTableNames.Where(m => !LTargetTableNames.Any(s => s.ToLower() == m.ToLower())).ToList();
                    List<string> TargetDelete = LTargetTableNames.Where(m => !LSourceTableNames.Any(s => s.ToLower() == m.ToLower())).ToList();
                    List<string> TargetChanges = LSourceTableNames.Where(m => LTargetTableNames.Any(s => s.ToLower() == m.ToLower())).ToList();

                    List<object> AddTables = new List<object>(),
                                 DeleteTables = new List<object>(),
                                 ChangeTables = new List<object>();

                    //*****************************************Add To Target : Tables Present in Sournce but not in Target***************************************
                    if (TargetAdd.Count > 0)
                    {
                        TargetAdd.OrderBy(m => m).ToList().ForEach(sTableName =>
                        {
                            string sCloneTableName = sTableName.Clone().ToString();
                            var childrens = oSourceList.GroupBy(m => m.sColumnName).Select(m => m.FirstOrDefault()).Where(m => m.sTableName.ToLower() == sCloneTableName.ToLower()).Select(m => new { text = m.sColumnName }).ToList();
                            var obj = new
                            {
                                parent = 10,
                                text = "Table",
                                icon = "fa fa-table",
                                data = new { sName = sTableName, tName = string.Empty, bIsSelected = true, iType = 10, AllColumns = oSourceList.Where(m => m.sTableName == sTableName).ToList() },
                                children = new List<object> { new { text = "Columns", children = oSourceList.Where(m => m.sTableName == sTableName).
                            Select(m => new { text = "Column", icon = "fa fa-columns", data = new { sName = m.sColumnName, tName = string.Empty, iType = 10, ColumnDetails = m },
                            children = new List<object> { new { text = "Properties", children = objectEqual(m).Keys.Select(pro => new { text = "", icon = "fa fa-wrench", data = new { sName = pro, tName = string.Empty, iType = 10 } }) } } }).ToList() } }
                            };
                            AddTables.Add(obj);
                        });
                    }

                    //***********************Change To Target :  Tables Present in both Source and Target but Column Properties are different***************************
                    if (TargetChanges.Count > 0)
                    {
                        TargetChanges.OrderBy(m => m).ToList().ForEach(sTableName =>
                        {
                            XIDatabaseSchema Schema = new XIDatabaseSchema();
                            List<XITableColumns> LColumns = new List<XITableColumns>();
                            string sCloneTableName = sTableName.Clone().ToString();
                            Schema.DTableColumns = new Dictionary<string, List<XITableColumns>>();
                            Schema.iActionType = 20;

                            List<XITableColumns> OSource = oSourceList.Where(m => m.sTableName.ToLower() == sCloneTableName.ToLower()).OrderBy(m => m.sColumnName).ToList();
                            List<XITableColumns> OTarget = oTargetList.Where(m => m.sTableName.ToLower() == sCloneTableName.ToLower()).OrderBy(m => m.sColumnName).ToList();

                            /****************************Columns Present in Source but not in Target************************************/
                            List<XITableColumns> TargetChangeAdd = OSource.GroupBy(m => m.sColumnName).Select(m => m.FirstOrDefault()).Where(m => !OTarget.Any(s => s.sColumnName.ToLower() == m.sColumnName.Clone().ToString().ToLower())).ToList();

                            TargetChangeAdd.ForEach(x =>
                            {
                                XITableColumns obj = new XITableColumns();
                                x.Properties = objectEqual(x);
                                x.iActionType = 10;
                            });
                            var AddProperties = TargetChangeAdd.Select(m => new
                            {
                                text = "Column",
                                icon = "fa fa-columns",
                                data = new { sName = m.sColumnName, tName = string.Empty, iType = 10, ColumnDetails = m },
                                children = new List<object> { new { text = "Properties",
                             children = m.Properties.Keys.Select(pro=> new { text = "", icon = "fa fa-wrench", data= new {sName= pro, tName = string.Empty, iType = 10 }  }) } }
                            }).ToList();

                            /****************************Columns Present in both Source and Target but thier Properties values are different***********************/
                            List<XITableColumns> TargetChangeDifference = OSource.GroupBy(m => m.sColumnName).Select(m => m.FirstOrDefault()).Where(m => OTarget.Any(s => s.sColumnName.ToLower() == m.sColumnName.Clone().ToString().ToLower())).ToList();
                            TargetChangeDifference.ForEach(x =>
                            {
                                XITableColumns Change = OTarget.Where(y => y.sColumnName.ToLower() == x.sColumnName.Clone().ToString().ToLower()).Select(y => y).FirstOrDefault();
                                if (!x.Equals(Change))
                                {
                                    x.iActionType = 20;
                                    LColumns.Add(x);
                                }

                            });

                            var ChangeProperties = LColumns.Select(m => new
                            {
                                text = "Column",
                                icon = "fa fa-columns",
                                data = new { sName = m.sColumnName, tName = m.sColumnName, iType = 20, ColumnDetails = m },
                                children = new List<object> { new { text = "Properties", children = m.Properties.Keys.Select(pro => new { text = "", icon = "fa fa-wrench", data = new { sName = pro, tName = pro, iType = 20 } }) } }
                            }).ToList();

                            /**********************************Columns Present in Target but not in Source*******************************/
                            List<XITableColumns> TargetChangeDelete = OTarget.Where(m => !OSource.Any(s => s.sColumnName.ToLower() == m.sColumnName.Clone().ToString().ToLower())).ToList();
                            TargetChangeDelete.ForEach(x =>
                            {
                                x.Properties = objectEqual(x);
                                x.iActionType = 30;
                            });
                            var DeleteProperties = TargetChangeDelete.Select(m => new
                            {
                                text = "Column",
                                icon = "fa fa-columns",
                                data = new { sName = string.Empty, tName = m.sColumnName, iType = 30, ColumnDetails = m },
                                children = new List<object> { new { text = "Properties", children = m.Properties.Keys.Select(pro => new { text = "", icon = "fa fa-wrench", data = new { sName = string.Empty, tName = pro, iType = 30 } }) } }
                            }).ToList();

                            List<object> AllChangesObject = new List<object>();
                            if (AddProperties.Count > 0)
                                AllChangesObject.AddRange(AddProperties);
                            if (ChangeProperties.Count > 0)
                                AllChangesObject.AddRange(ChangeProperties);
                            if (DeleteProperties.Count > 0)
                                AllChangesObject.AddRange(DeleteProperties);

                            Schema.DTableColumns.Add(sTableName, LColumns);

                            var ChangeObj = new
                            {
                                parent = 20,
                                text = "Table",
                                icon = "fa fa-table",
                                data = new { sName = sTableName, tName = sTableName, bIsSelected = true, iType = 20, AllColumns = OSource },
                                children = new List<object> { new { text = "Columns", children = AllChangesObject } }
                            };
                            if (AllChangesObject.Count > 0)
                                ChangeTables.Add(ChangeObj);
                        });
                    }

                    /*******************************************Delete in Target: Tables present in Target but not in Source****************************************/
                    if (TargetDelete.Count > 0)
                    {
                        TargetDelete.OrderBy(m => m).ToList().ForEach(sTableName =>
                        {
                            var obj = new
                            {
                                parent = 30,
                                text = "Table",
                                icon = "fa fa-table",
                                data = new { sName = string.Empty, tName = sTableName, bIsSelected = true, iType = 30 },
                                children = new List<object> { new { text = "Columns", children = oTargetList.Where(m => m.sTableName == sTableName).
                            Select(m => new { text = "Column", icon = "fa fa-columns", data = new { sName = string.Empty, tName = m.sColumnName, iType = 30, ColumnDetails = m },
                            children = new List<object> { new { text = "Properties", children = objectEqual(m).Keys.Select(pro => new { text = "", icon = "fa fa-wrench", data = new { sName = string.Empty, tName = pro, iType = 30 } }) } } }).ToList() } }
                            };
                            DeleteTables.Add(obj);
                        });
                    }
                    var Result = new List<object> { new { id = 10, text = "Add", state = new { opened = true }, children = AddTables }, new { id = 20, text = "Change", state = new { opened = true }, children = ChangeTables }, new { id = 30, text = "Delete", state = new { opened = true }, children = DeleteTables } };
                    oCResult.oResult = Result;
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory Param: oSourceList = " + oSourceList + " or oTargetList = " + oTargetList + " is missing";
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
            //oCResult.oResult = (int)xiEnumSystem.xiFuncResult.xiSuccess;
            return oCResult;



        }

        /*::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
          :: Generate Script for Create and Drop Table Query based on ActionType(add=10, change=20 and Delete=30) ::
          ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::*/
        public string CreateAndDropTableScript(List<XIDatabaseSchema> Schema, int iActionType)
        {
            CResult oCResult = new CResult();
            try
            {
                StringBuilder SBCreateTableScript = new StringBuilder();
                CommandType = CommandType.Text;
                string sQuery = string.Empty;
                if (iActionType == 10 && Schema.Count > 0)
                    SBCreateTableScript.Append("/************************** CREATE TABLES SCHEMA ************************/\n\n");
                else if (iActionType == 30 && Schema.Count > 0)
                {
                    SBCreateTableScript.Append("/************************** DROP TABLES SCHEMA ************************/\n\n");
                    sQuery = sDropQuery;
                }

                foreach (XIDatabaseSchema item in Schema)
                {
                    Query = sQuery.Replace("@@@", item.sTableName);
                    if (iActionType == 10)
                        SBCreateTableScript.Append("\t" + item.GenerateCreateTableScript() + "\n");
                    if (iActionType == 30)
                        SBCreateTableScript.Append("\t" + Query + "\n");
                }
                CloseConnection();
                return SBCreateTableScript.ToString();
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                SaveErrortoDB(oCResult);
                throw;
            }
        }
        /*:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
          :: Generate Script for Create Query based on Table Column Properties ::
          :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::*/
        public string GenerateCreateTableScript(bool bIsForTemp = false)
        {
            CResult oCResult = new CResult();
            try
            {
                StringBuilder sCreate = new StringBuilder();
                List<string> LStrings = new List<string>();
                string stblName = string.Empty;
                if (bIsForTemp)
                    stblName = "temp_" + sTableName;
                else
                    stblName = sTableName;
                sCreate.Append("CREATE TABLE [" + stblName + "] \n(\n\t");
                var ExecptColumns = Columns != null ? Columns.Where(m => m.iActionType == 10).Select(m => m.sColumnName).ToList() : new List<string>();
                foreach (XITableColumns column in AllColumns.OrderBy(m => m.iColumnOrder).GroupBy(m => m.sColumnName).Select(m => m.FirstOrDefault()).Where(m => !ExecptColumns.Contains(m.sColumnName)))
                {
                    //string sDefault = !string.IsNullOrEmpty(column.sDefaultValue) ? " DEFAULT(" + column.sDefaultValue + ")" : " ";
                    //LStrings.Add("[" + column.sColumnName + "] " + column.sDataType.ToUpper() + " " + (SqlStringDataTypes.Contains(column.sDataType) ? (column.lMaxLength == -1 ? "(MAX) " : "(" + column.lMaxLength + ") ") : " ") + (column.bIsNullable ? "NULL " : "NOT NULL ") + (column.bIsIdentity ? "PRIMARY KEY IDENTITY(1,1)" : " ") + sDefault);

                    LStrings.Add("[" + column.sColumnName + "] " + column.sDataType.ToUpper() + GenerateAlterColumnScript(column, false) + (column.bIsIdentity ? "PRIMARY KEY IDENTITY(1,1)" : " "));
                }
                sCreate.Append(string.Join(",\n\t", LStrings) + "\n)");
                return sCreate.ToString();
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                SaveErrortoDB(oCResult);
                throw;
            }
        }
        /*::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
          :: Generate Script for Inset Query based on Table Columns ::
          ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::*/
        public string GenerateInsertScript(bool bIsForTemp = false)
        {
            CResult oCResult = new CResult();
            try
            {
                StringBuilder sInsert = new StringBuilder();
                string stblName = string.Empty;
                if (bIsForTemp)
                    stblName = "temp_" + sTableName;
                else
                    stblName = sTableName;

                sInsert.Append("INSERT INTO [" + stblName + "] (");
                List<string> lColumns = new List<string>();
                var ExecptColumns = Columns != null ? Columns.Where(m => m.iActionType == 10).Select(m => m.sColumnName).ToList() : new List<string>();
                foreach (XITableColumns column in AllColumns.OrderBy(m => m.iColumnOrder).GroupBy(m => m.sColumnName).Select(m => m.FirstOrDefault()).Where(m => !ExecptColumns.Contains(m.sColumnName)))
                    lColumns.Add("[" + column.sColumnName + "]");
                sInsert.Append(string.Join(", ", lColumns) + ")\n");

                return sInsert.ToString();
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                SaveErrortoDB(oCResult);
                throw;
            }
        }
        /*:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
          :: Generate Script for Select Query based on Table Columns ::
          :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::*/
        public string GenerateSelectScript()
        {
            CResult oCResult = new CResult();
            try
            {
                StringBuilder sSelect = new StringBuilder();
                sSelect.Append("SELECT ");
                List<string> lColumns = new List<string>();
                var ExecptColumns = Columns != null ? Columns.Where(m => m.iActionType == 10).Select(m => m.sColumnName).ToList() : new List<string>();
                foreach (var column in AllColumns.OrderBy(m => m.iColumnOrder).GroupBy(m => m.sColumnName).Select(m => m.FirstOrDefault()).Where(m => !ExecptColumns.Contains(m.sColumnName)))
                    lColumns.Add("[" + column.sColumnName + "]");
                sSelect.Append(string.Join(", ", lColumns) + " FROM [" + sTableName + "]\n");
                return sSelect.ToString();
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                SaveErrortoDB(oCResult);
                throw;
            }
        }
        /*::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
          :: Generate Add, Alter and Drop Column based on iActionType ::
          :: If iActionType = 10 --> add Column                       ::
          :: if iActionType = 20 --> Alter Column                     ::  
          :: if iAcionType = 30 --> Drop Column                       ::
          ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::*/
        public string GenerateAlterTablesScript()
        {
            CResult oCResult = new CResult();
            try
            {
                StringBuilder sb = new StringBuilder();
                bool bIsNeedDrop = false;
                if (Columns.Count > 0)
                {

                    if (Columns.Where(m => m.iActionType == 20).Count() > 0)
                    {
                        var AllProperties = Columns.Where(m => m.iActionType == 20).SelectMany(m => m.Properties.Keys).ToList();
                        if (AllProperties.Contains(XIConstant.IsIdentity) || AllProperties.Contains(XIConstant.DefaultValue))
                        {
                            bIsNeedDrop = true;
                            bool bIsIdentity = false;
                            if (AllColumns.Where(m => m.bIsIdentity).FirstOrDefault() != null)
                                bIsIdentity = true;
                            sb.Append(GenerateAlterConstraintScript(bIsIdentity) + "\n");
                        }
                        else
                            foreach (var item in Columns.Where(m => m.iActionType == 20))
                                sb.Append("ALTER TABLE [" + sTableName + "] ALTER COLUMN [" + item.sColumnName + "] " + item.sDataType + GenerateAlterColumnScript(item, true) + "\n");
                    }
                    if (Columns.Where(m => m.iActionType == 10).Count() > 0)
                        foreach (var item in Columns.OrderBy(m => m.iColumnOrder).Where(m => m.iActionType == 10))
                            sb.Append("ALTER TABLE [" + sTableName + "] ADD [" + item.sColumnName + "] " + item.sDataType + GenerateAlterColumnScript(item) + "\n");

                    if (Columns.Where(m => m.iActionType == 30).Count() > 0 && !bIsNeedDrop)
                        foreach (var item in Columns.OrderBy(m => m.iColumnOrder).Where(m => m.iActionType == 30))
                            sb.Append("ALTER TABLE [" + sTableName + "] DROP COLUMN [" + item.sColumnName + "] \n");
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                SaveErrortoDB(oCResult);
                throw;
            }
        }
        /*:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
          :: Setting Size of Column, Null or NOT NULL and DEFAULT Constraint ::
          :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::*/
        public string GenerateAlterColumnScript(XITableColumns Column, bool bIsForAlter = false)
        {
            CResult oCResult = new CResult();
            try
            {
                StringBuilder sAlter = new StringBuilder();
                bool IsDependent = false;
                string sMaxLength = Column.lMaxLength.ToString();
                if (SqlStringDataTypes.Contains(Column.sDataType.ToLower()))
                {
                    if (Column.lMaxLength == -1) sMaxLength = "MAX";
                    if (Column.lMaxLength == 0) sMaxLength = "256";
                    IsDependent = true;
                    sAlter.Append("(" + sMaxLength + ")");
                }
                string sSize = string.Empty;
                if (SqlStringDataTypes.Contains(Column.sDataType) && !IsDependent) sSize = "(" + Column.lMaxLength + ")";
                sAlter.Append(sSize + (Column.bIsNullable ? " NULL " : " NOT NULL "));
                if (!string.IsNullOrEmpty(Column.sDefaultValue) && !bIsForAlter) sAlter.Append("DEFAULT" + Column.sDefaultValue);
                return sAlter.ToString();
            }

            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                SaveErrortoDB(oCResult);
                throw;
            }
        }
        /*::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
          :: Idenity On or Off Can't be Altered so we Recreate Table                                                    ::
          :: Here Recreate the table for Identity Constraint on and off (or) Default Constraint add and remove if exist ::
          :: 1.Create Temporary Table with Existing Normal Table Columns and with Properties                            ::
          :: 2.Arrange Query SET IDENTITY ON  if bIsIdentity true                                                       ::
          :: 3.Insert the Existing Table Data into Temporary Table from Select Statement of the Normal Table            ::
          :: 4.Drop the Normal Table                                                                                    ::
          :: 5.Rename the Temporary Table with Normal Table                                                             ::
          ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::*/
        public string GenerateAlterConstraintScript(bool bIsIdentity)
        {
            CResult oCResult = new CResult();
            try
            {
                StringBuilder sAlter = new StringBuilder();
                if (bIsIdentity)
                    sAlter.Append(IdentityAddQuery.Replace("#IdentityON", IdentityON).Replace("#IdentityOFF", IdentityOFF).Replace("#TempTable", GenerateCreateTableScript(true)).Replace("@@@", sTableName).Replace("#InsertTempTable", GenerateInsertScript(true)).Replace("#SelectTable", GenerateSelectScript()) + "\n");
                else
                    sAlter.Append(IdentityAddQuery.Replace("#IdentityON", "").Replace("#IdentityOFF", "").Replace("#TempTable", GenerateCreateTableScript(true)).Replace("@@@", sTableName).Replace("#InsertTempTable", GenerateInsertScript(true)).Replace("#SelectTable", GenerateSelectScript()) + "\n");

                return sAlter.ToString();
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                SaveErrortoDB(oCResult);
                throw;
            }
        }
        #endregion DataBase_Compare_and_Script_Generation

        #region BO's_and_Selft_Database_compare_and_Script_Generation
        /*:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
          :: Compare BO Tables with Specific Database Based on iDataSource ::
          :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::*/
        public object BOSchemaCompare(int iDataSource)
        {
            CResult oCResult = new CResult();
            try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString;
                Query = sBOTableColumnsQuery.Replace("@@@", iDataSource.ToString());
                CommandType = CommandType.Text;

                /************************* Get Table Columns with Properties of the BO's Tables ***********************/
                List<XITableColumns> BoColumns = DataTableToList<XITableColumns>(ExecuteReaderToDataTable());
                Query = sSystemQueryForBO + sNotInQuery;
                ConnectionString = oXID.GetBODataSource(iDataSource, 0);
                //ConnectionString = "Data Source = 192.168.7.222; initial catalog = ZeeinsuranceV2; User Id = cruser; Password = cruser; MultipleActiveResultSets = True";
                if (CheckConnection())
                {
                    /************************* Get Table Columns with Properties of Database Based on iDataSource Connection ***********************/
                    var SysTableColumns = DataTableToList<XITableColumns>(ExecuteReaderToDataTable());
                    if (BoColumns.Count > 0)
                        return NewCompare(BoColumns, SysTableColumns);
                }
                return new List<object> { new { id = 10, text = "Add" }, new { id = 20, text = "Change" }, new { id = 30, text = "Delete" } };

            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                SaveErrortoDB(oCResult);
                throw;
            }
        }

        public CResult NewBOSchemaCompare(int iDataSource)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Bo Schema Comparing for two DataBases";//expalin about this method logic
            try
            {
                ConnectionString = ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString;
                Query = sBOTableColumnsQuery.Replace("@@@", iDataSource.ToString());
                CommandType = CommandType.Text;

                /************************* Get Table Columns with Properties of the BO's Tables ***********************/
                List<XITableColumns> BoColumns = DataTableToList<XITableColumns>(ExecuteReaderToDataTable());
                Query = sSystemQueryForBO + sNotInQuery;
                ConnectionString = oXID.GetBODataSource(iDataSource, 0);
                //ConnectionString = "Data Source = 192.168.7.222; initial catalog = ZeeinsuranceV2; User Id = cruser; Password = cruser; MultipleActiveResultSets = True";
                if (CheckConnection())
                {
                    /************************* Get Table Columns with Properties of Database Based on iDataSource Connection ***********************/
                    var SysTableColumns = DataTableToList<XITableColumns>(ExecuteReaderToDataTable());
                    if (BoColumns.Count > 0)
                    {
                        //return 
                        oCR = NewCompare(BoColumns, SysTableColumns);
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
                        return oCResult;
                    }
                }
                var Result = new List<object> { new { id = 10, text = "Add" }, new { id = 20, text = "Change" }, new { id = 30, text = "Delete" } };
                oCResult.oResult = Result;

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
            return oCResult;
        }

        /*:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
          :: Generate Script for Create, alter and Drop Table Query based on ActionType(add=10, change=20 and Delete=30) ::
          :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::*/
        public string GenerateBOScript(Dictionary<int, List<XIDatabaseSchema>> Tables)
        {
            CResult oCResult = new CResult();
            try
            {
                string sScripts = string.Empty;

                if (Tables.Keys.Any(m => m == 10))
                {
                    string sComment = "/************************** CREATE TABLES ************************/\n\n";
                    StringBuilder CreateScript = new StringBuilder();
                    foreach (var item in Tables[10])
                        CreateScript.Append(item.CreateTableFromBOAttributes() + "\n");
                    if (CreateScript.Length > 0)
                        sScripts += sComment + CreateScript.ToString();
                }
                if (Tables.Keys.Any(m => m == 20))
                {
                    string sComment = "/************************** ALTER TABLES ************************/\n\n";
                    StringBuilder alterScript = new StringBuilder();
                    foreach (var item in Tables[20])
                        alterScript.Append(item.AlterTablesFromBO());
                    if (alterScript.Length > 0)
                        sScripts += sComment + alterScript.ToString();


                }
                if (Tables.Keys.Any(m => m == 30))
                {
                    string sComment = "/************************** DROP TABLES ************************/\n\n";
                    StringBuilder DropScript = new StringBuilder();
                    foreach (var item in Tables[30])
                        DropScript.Append(sDropQuery.Replace("@@@", item.sTableName) + "\n");
                    if (DropScript.Length > 0)
                        sScripts += sComment + DropScript.ToString();
                }
                return sScripts;
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                SaveErrortoDB(oCResult);
                throw;
            }
        }
        /*:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
          :: Generate Create Table Query from BO Table Column Properties ::
          :: Finally Append the Common Columns                           ::
          :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::*/
        public string CreateTableFromBOAttributes()
        {
            CResult oCResult = new CResult();
            try
            {
                StringBuilder sScript = new StringBuilder();
                List<string> List = new List<string>();
                if (AllColumns != null && AllColumns.Count > 0)
                {
                    foreach (var Attribute in AllColumns.GroupBy(m => m.sColumnName).Select(m => m.FirstOrDefault()).ToList())
                    {
                        string sLine = GetColumn(Attribute.sColumnName);
                        if (!string.IsNullOrEmpty(sLine))
                            List.Add(sLine + ",\n");
                    }
                }
                if (List.Count() > 0)
                {
                    sScript.Append("CREATE TABLE [" + sTableName + "]\n(\n\t");
                    sScript.Append(string.Join("\t", List));
                    sScript.Append("\t[zXCrtdBy] VARCHAR(15) NULL,\n\t[zXCrtdWhn] DATETIME NULL,\n\t[zXUpdtdBy] VARCHAR(15) NULL,\n\t[zXUpdtdWhn] DATETIME NULL,\n\t[izXDeleted] INT NULL,\n\t[sHierarchy] VARCHAR(256) NULL, \n\t[XIGUID] UNIQUEIDENTIFIER  NOT NULL)");
                }
                return sScript.ToString();
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                SaveErrortoDB(oCResult);
                throw;
            }
        }
        /*::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
          :: Generate Alter Query Only for Add Column and Change the Length of the Column ::
          ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::*/
        public string AlterTablesFromBO()
        {
            CResult oCResult = new CResult();
            try
            {
                StringBuilder sb = new StringBuilder();
                if (Columns != null && Columns.Count > 0)
                {
                    //if (Columns.Where(m => m.iActionType == 10).ToList().Count > 0)
                    //    foreach (var item in Columns.Where(m => m.iActionType == 10).ToList())
                    //    {
                    //        var sString = GetColumn(item.sColumnName);
                    //        if (!string.IsNullOrEmpty(sString))
                    //            sb.Append("ALTER TABLE [" + sTableName + "] ADD " + sString + "\n");
                    //    }
                    //if (Columns.Where(m => m.iActionType == 20).ToList().Count > 0)
                    //    foreach (var item in Columns.Where(m => m.iActionType == 20).ToList())
                    //        if (item.Properties != null && (item.Properties.Keys.Contains(XIConstant.MaxLength) || !string.IsNullOrEmpty(item.Properties[XIConstant.MaxLength]) || !string.IsNullOrEmpty(item.sDataType)) && SqlStringDataTypes.Contains(item.sDataType.ToLower()))
                    //        {
                    //            if (item.Properties[XIConstant.MaxLength] == "-1") item.Properties[XIConstant.MaxLength] = "MAX";
                    //            if (item.Properties[XIConstant.MaxLength] == "0") item.Properties[XIConstant.MaxLength] = "256";
                    //            sb.Append("ALTER TABLE [" + sTableName + "] ALTER COLUMN " + item.sColumnName + " " + item.sDataType.ToUpper() + "(" + item.Properties[XIConstant.MaxLength] + ")\n");
                    //        }
                    //if (Columns.Where(m => m.iActionType == 30).ToList().Count > 0)
                    //{
                    //    foreach (var item in Columns.Where(m => m.iActionType == 30).ToList())
                    //    {
                    //        sb.Append("ALTER TABLE [" + sTableName + "] DROP COLUMN [" + item.sColumnName + "]\n");
                    //    }
                    //}
                    sb.Append(GenerateAlterTablesScript());
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                SaveErrortoDB(oCResult);
                throw;
            }
        }
        /*:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
          :: Compare BO Tables with Selft Database Tables using BO Structure Tables ::
          :: and Genderate Script                                                   ::
          ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::*/
        public CResult NewBOStructureTablesSchemaCompare(string sSource, string[] BONames, bool IsForScript = false)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "BO Structure Schema Comparing for two DataBase";//expalin about this method logic

            List<XITableColumns> SysTableColumns = new List<XITableColumns>();
            try
            {
                oTrace.oParams.Add(new CNV { sName = "sSource", sValue = sSource });
                if (!string.IsNullOrEmpty(sSource))//check mandatory params are passed or not
                {
                    string sCommaTables = string.Empty;
                    BONames.ToList().ForEach(m => { sCommaTables += "'" + m + "',"; });
                    Query = sWhereTablesInQuery.Replace("@@@", sCommaTables.TrimEnd(','));
                    CommandType = CommandType.Text;
                    ConnectionString = sSource;
                    /************************* Get Table Columns with Properties of the BO's Tables ***********************/
                    List<XITableColumns> BoColumns = DataTableToList<XITableColumns>(ExecuteReaderToDataTable());

                    oCResult = NewDataCompareWithBONames(sSource, null, BONames, IsForScript);
                    if (oCResult.bOK == true && oCResult.oResult != null)
                    {
                        var DataSourceOfTable = (List<XITableColumns>)oCResult.oResult;


                        /************************* Get Table Columns with Properties of Database Based on iDataSource Connection ***********************/
                        if (DataSourceOfTable != null && DataSourceOfTable.Count > 0)
                        {
                            DataSourceOfTable.ForEach(m =>
                            {
                                Query = sSystemQueryForBO + sNotInQuery + "AND tbl.name = '" + m.sTableName + "'";
                                ConnectionString = GetConnectionWithiDataSource(sSource, m.iDataSource[0]);
                                List<XITableColumns> columns = DataTableToList<XITableColumns>(ExecuteReaderToDataTable());
                                if (columns != null && columns.Count > 0)
                                    SysTableColumns.AddRange(columns);

                            });

                        }
                    }
                    oCR = NewCompare(BoColumns, SysTableColumns);
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
                    oTrace.sMessage = "Mandatory Param: sSource = " + sSource + "  is missing";
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
            return oCResult;
        }

        #endregion BO's_and_Selft_Database_compare_and_Script_Generation 

        #region Shema_Validate_and_Execution

        /*:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
          :: Validate the Script has Synatical errors (or) Alter Command Errors in Target Database ::
          :: Execute Script with SET FMTONLY ON for validating Script                              ::
          :: Return Synatical Errors with Line Number                                              ::
          :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::*/
        public string ValidateScript(string sScript, string sTarget, int iDataSource)
        {
            CResult oCResult = new CResult();
            try
            {
                if (!string.IsNullOrEmpty(sTarget))
                    ConnectionString = sTarget;
                if (iDataSource > 0)
                {
                    ConnectionString = oXID.GetBODataSource(iDataSource, 0);
                    //ConnectionString = "Data Source = 192.168.7.222; initial catalog = iPMTest_dump1; User Id = cruser; Password = cruser; MultipleActiveResultSets = True";
                }
                //sTarget = @"Data Source=(localdb)\MSSQLLocalDB;Initial Catalog=TargetDatabase;Integrated Security=True;Connect Timeout=30;Encrypt=False;TrustServerCertificate=False;ApplicationIntent=ReadWrite;MultiSubnetFailover=False";

                StringBuilder sb = new StringBuilder();
                sb.Append("SET FMTONLY ON\n");
                sb.Append(sScript + "\n");
                sb.Append("SET FMTONLY OFF \n");
                Query = sb.ToString();
                sb = new StringBuilder();
                var oException = ExecuteScalar();

                if (oException != null)
                {
                    SqlException ex = (SqlException)oException;
                    if (ex.Errors != null && ex.Errors.Count > 0)
                        foreach (var error in ex.Errors)
                        {
                            SqlError e = ((SqlError)error);
                            sb.Append("Message: " + e.Message + "  Line Number: " + (e.LineNumber - 1) + "\n");
                        }
                }
                return sb.ToString();
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                SaveErrortoDB(oCResult);
                throw;
            }
        }
        /*::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
          :: Execute All Create, Alter and Drop Table Commnads with TRANSACTIONS              ::
          :: if Script Execution Failed it ROLLBACK other wise COMMIT the Transaction         ::
          :: Before Execution again it Validate Script has syntatical errors or not           ::
          ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::*/
        public object Execute(string sScript, string sTarget, int iDataSource)
        {
            CResult oCResult = new CResult();
            try
            {
                string sException = ValidateScript(sScript, sTarget, iDataSource);
                if (!string.IsNullOrEmpty(sException))
                    return new { from = "validate", Message = sException };
                else
                {
                    StringBuilder sb = new StringBuilder();
                    sb.Append(TransactionQuery.Replace("@@@", sScript));
                    Query = sb.ToString();
                    sb = new StringBuilder();
                    //var oErrorList = DataTableToList<dynamic>(ExecuteReaderToDataTable());
                    var oErrorList = new object() as dynamic;
                    if (oErrorList != null && oErrorList.Count > 0)
                    {
                        foreach (var error in oErrorList)
                            sb.Append("Message: " + error.Message + "  Line Number: " + (error.LineNumber - 1) + "\n");
                        return new { from = "Execute", Message = sb.ToString() };
                    }
                    else
                        return new { from = "Execute", Message = "Execute Successfully" };
                }
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                SaveErrortoDB(oCResult);
                throw;
            }
        }
        #endregion Shema_Validate_and_Execution

        #region Tables_Data_Compare
        public object GetBOStructureDetails(string sSource, int ID = 0)
        {
            CResult oCResult = new CResult();
            try
            {

                XID1Click oClick = new XID1Click();
                oClick.sConnectionString = sSource;
                string sWhereCondition = "FKiParentID = '#'";
                if (ID > 0)
                    sWhereCondition = "BOID =" + ID + " and FKiParentID = '#'";
                oClick.Query = "SELECT ID, BOID, sStructureName, sCode FROM XIBOStructure_T WHERE " + sWhereCondition + " ORDER BY sStructureName ";
                oClick.Name = "XI BOStructure"; //BOID=935
                List<XIIBO> oRecords = oClick.OneClick_Run().Values.ToList();
                var result = new object();
                if (ID > 0)
                    result = oRecords.ToList();
                else
                    result = oRecords.Select(m => m.Attributes).Select(m => new XIDropDown
                    {
                        text = m["sStructureName"].sValue,
                        Value = m["ID"].iValue,
                    }).ToList();
                return result;
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                SaveErrortoDB(oCResult);
                throw;
            }
        }

        public object GetBOStructures(string sSource, int ID = 0)
        {
            CResult oCResult = new CResult();
            try
            {

                XID1Click oClick = new XID1Click();
                oClick.sConnectionString = sSource;
                string sWhereCondition = "FKiParentID = '#'";
                if (ID > 0)
                    sWhereCondition = "ID =" + ID;
                oClick.Query = "SELECT ID, BOID, sStructureName, sCode FROM XIBOStructure_T WHERE " + sWhereCondition + " ORDER BY sStructureName ";
                oClick.Name = "XI BOStructure"; //BOID=935
                List<XIIBO> oRecords = oClick.OneClick_Run().Values.ToList();
                var result = new object();
                if (ID > 0)
                    result = oRecords.FirstOrDefault();
                else
                    result = oRecords.Select(m => m.Attributes).Select(m => new XIDropDown
                    {
                        text = m["sStructureName"].sValue,
                        Value = m["ID"].iValue,
                    }).ToList();
                return result;
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                SaveErrortoDB(oCResult);
                throw;
            }
        }
        public List<XIDStructure> GetTreeStructure(string sSource, int ID)
        {
            CResult oCResult = new CResult();
            try
            {
                List<XIDStructure> oStructure = new List<XIDStructure>();
                XIIBO oXIIBO = (XIIBO)GetBOStructures(sSource, ID);
                if (oXIIBO != null && oXIIBO.Attributes != null && oXIIBO.Attributes.Count > 0)
                {
                    int iBOID = oXIIBO.Attributes["BOID"].iValue;
                    string sCode = oXIIBO.Attributes["sCode"].sValue;
                    XIDStructure Structure = new XIDStructure();
                    oStructure = (List<XIDStructure>)Structure.GetXITreeStructure(iBOID, sCode).oResult;
                }
                return oStructure ?? new List<XIDStructure>();
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                SaveErrortoDB(oCResult);
                throw;
            }
        }

        //********* Getting Structure loading******//
        public CResult NewGetTreeStructure(string sSource, int ID)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Tree Structure Loading on OnChange Event";//expalin about this method logic
            try
            {
                oTrace.oParams.Add(new CNV { sName = "sSource", sValue = sSource });
                oTrace.oParams.Add(new CNV { sName = "ID", sValue = ID.ToString() });
                if (!string.IsNullOrEmpty(sSource) && ID>0)
                {
                    XIIBO oXIIBO = (XIIBO)GetBOStructures(sSource, ID);
                    if (oXIIBO != null && oXIIBO.Attributes != null && oXIIBO.Attributes.Count > 0)
                    {
                        int iBOID = oXIIBO.Attributes["BOID"].iValue;
                        string sCode = oXIIBO.Attributes["sCode"].sValue;
                        XIDStructure Structure = new XIDStructure();
                        oCResult.oResult = (List<XIDStructure>)Structure.GetXITreeStructure(iBOID, sCode).oResult;                        
                    }
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory Param: sSource = " + sSource + " or ID"+ID+"  is missing";
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
            return oCResult;
        }


        public CResult NewDataCompareWithBONames(string sSource, string sTarget, string[] BONames, bool IsForScript = false)
        {

            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Data Compare With BO Names";//expalin about this method logic
            try
            {
                oTrace.oParams.Add(new CNV { sName = "sSource", sValue = sSource });
                oTrace.oParams.Add(new CNV { sName = "sTarget", sValue = sTarget });
                oTrace.oParams.Add(new CNV { sName = "BONames", sValue = BONames.ToList().ToString() });
                oTrace.oParams.Add(new CNV { sName = "IsForScript", sValue = IsForScript.ToString() });
                if (!string.IsNullOrEmpty(sSource) && !string.IsNullOrEmpty(sTarget))//check mandatory params are passed or not
                {
                    //List<XITableColumns>
                    List<XITableColumns> oSource = new List<XITableColumns>();
                    if (BONames.Count() > 0)
                    {

                        XIInfraCache oCache = new XIInfraCache();
                        BONames.ToList().ForEach((Action<string>)(m =>
                        {
                            XITableColumns data = new XITableColumns();
                            XIDBO oXIDBOSource = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, m);
                            data.sTableName = oXIDBOSource.TableName;
                            data.sBOName = m;
                            data.iDataSource = new List<int>();
                            data.iDataSource.Add(oXIDBOSource.iDataSource);
                            data.sPrimaryKey = oXIDBOSource.sPrimaryKey;
                            if (!string.IsNullOrEmpty(sTarget))
                            {
                                Dictionary<string, object> Params = new Dictionary<string, object>();
                                Params["Name"] = m;
                                string sSelectFields = string.Empty;
                                sSelectFields = "iDataSource";
                                XIDBAPI Connection = new XIDBAPI(sTarget);
                                XIDBO oXIDBOTarget = Connection.Select<XIDBO>("XIBO_T_N", Params, sSelectFields).FirstOrDefault();
                                if (oXIDBOTarget != null)
                                    data.iDataSource.Add(oXIDBOTarget.iDataSource);

                            }
                            if (data.iDataSource.Count == 2 || IsForScript)
                                oSource.Add(data);
                        }));
                    }
                    oCResult.oResult = oSource;
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory Param: sSource = " + sSource + " or sTarget = " + sTarget + " is missing";
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
            // oCResult.oResult = (int)xiEnumSystem.xiFuncResult.xiSuccess;
            return oCResult;
        }

        /*======= TableWise selection ===========================*/

        public CResult NewTableSelection(string Source, bool AllBos = false)
        {

            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Working on selected Bo";//expalin about this method logic
            try
            {
                oTrace.oParams.Add(new CNV { sName = "Source", sValue = Source });
                if (!string.IsNullOrEmpty(Source))//check mandatory params are passed or not
                {
                    using (SqlConnection connection = new SqlConnection(Source))
                    {
                        connection.Open();
                        DataTable schema = connection.GetSchema("Tables");
                        List<string> TableNames = new List<string>();
                        List<string> BoNames = new List<string>();
                        foreach (DataRow row in schema.Rows)
                        {
                            TableNames.Add(row[2].ToString());
                        }
                        string sTableNames = string.Join("','", new List<string>(TableNames).ToArray());
                        XIInfraCache oCache = new XIInfraCache();
                        XID1Click oClick = new XID1Click();
                        oClick.sConnectionString = Source;
                        if (AllBos == true)
                            oClick.Query = "SELECT BOID, Name, TableName FROM XIBO_T_N";
                        else
                            oClick.Query = "SELECT BOID, Name, TableName FROM XIBO_T_N where TableName in ('" + sTableNames + "')";
                        List<XIIBO> oRecords = oClick.OneClick_Run().Values.ToList();
                        List<XIIBO> result = oRecords;// oRecords.Select(m => m.Attributes).Select(m => m["Name"].sValue).ToList();
                        oCResult.oResult = result;
                        //return result;
                    }
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory Param: Source = " + Source + " is missing";
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
            // oCResult.oResult = (int)xiEnumSystem.xiFuncResult.xiSuccess;
            return oCResult;

        }

        public List<Dictionary<string, List<XIIBO>>> StrctureData(string sStructureName, int iInstanceID, string ParentID, int iBOID)
        {
            List<Dictionary<string, List<XIIBO>>> oSUblist = new List<Dictionary<string, List<XIIBO>>>();
            XIInfraCache oCache = new XIInfraCache();
            //string[] BONames;
            XIBOInstance oInstance = new XIBOInstance();
            if (!string.IsNullOrEmpty(ParentID))
            {
                XID1Click o1ClickDe = new XID1Click();
                o1ClickDe = (XID1Click)oCache.GetObjectFromCache(XIConstant.Cache1Click, "Structure 1Click", null);
                var oCopy = (XID1Click)o1ClickDe.Clone(o1ClickDe);
                XIDStructure oXIDStructure = new XIDStructure();
                List<CNV> nParams = new List<CNV>();
                CNV nvpairs = new CNV();
                nvpairs.sName = "{XIP|Values}";
                nvpairs.sValue = ParentID;
                nParams.Add(nvpairs);
                oCopy.Query = oXIDStructure.ReplaceExpressionWithCacheValue(oCopy.Query, nParams);
                var Result = oCopy.GetList();
                if (Result.oResult != null)
                {
                    var text = (Dictionary<string, XIIBO>)Result.oResult;
                    foreach (var item in text)
                    {
                        XIDStructure oXIDStructure1 = new XIDStructure();
                        oXIDStructure1.sName = item.Value.Attributes["sName"].sValue;
                        oXIDStructure1.ID = Convert.ToInt32(item.Value.Attributes["ID"].sValue);
                        oXIDStructure1.sBO = item.Value.Attributes["sBO"].sValue;
                        oXIDStructureData.Add(oXIDStructure1);
                    }
                }
            }
            Dictionary<string, object> Params1 = new Dictionary<string, object>();
            if (!string.IsNullOrEmpty(sStructureName) && iBOID != 0 && iInstanceID != 0)
            {
                Params1["BOID"] = iBOID;
                Params1["sStructureName"] = sStructureName;
                Params1["FKiParentID"] = "#";
                XIDBAPI Connection = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString);
                var sMainNode = Connection.Select<XIDStructure>("XIBOStructure_T", Params1).FirstOrDefault();
                XIIXI oIXI = new XIIXI();
                var oLIst = oIXI.BOI(sMainNode.sBO, iInstanceID.ToString());
                if (oLIst != null)
                    oInstance = oLIst.Structure(sMainNode.sCode).XILoad();
            }
            if (oInstance.oStructureInstance != null && oInstance.oStructureInstance.Count > 0)
            {
                oSUblist.Add(oInstance.oStructureInstance);
                foreach (var item in oInstance.oStructureInstance.FirstOrDefault().Value.FirstOrDefault().SubChildI.ToList())
                {
                    Dictionary<string, List<XIIBO>> BoList = new Dictionary<string, List<XIIBO>>();
                    BoList.Add(item.Key, item.Value);
                    oSUblist.Add(BoList);
                    var SubchildData = item.Value.ToList().Select(u => u.SubChildI).ToList();
                    string Bos = string.Empty;
                    Bos = SubchildData[0].Keys.FirstOrDefault();
                    if (!string.IsNullOrEmpty(Bos))
                    {
                        Dictionary<string, List<XIIBO>> BOListParams = new Dictionary<string, List<XIIBO>>();

                        List<XIIBO> Bosn = new List<XIIBO>();
                        foreach (var item2 in SubchildData)
                        {
                            if (item2.Count > 0)
                            {
                                Bosn.AddRange(item2.Values.FirstOrDefault());
                            }
                        }
                        if (Bosn.Count > 0)
                            BOListParams.Add(Bos, Bosn);
                        if (BOListParams.Count > 0)
                            oSUblist.Add(BOListParams);
                    }
                }
            }
            return oSUblist;
        }

        public CResult NewDynamicDataCompare(string sSourceConnection, string sTargetConnection, string sCompareWith, string[] BONames, int iInstanceID, string sStructureName, int iBOID, string ParentIDs)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Data Compare";//expalin about this method logic
            try
            {
                List<XITableColumns> oTables = new List<XITableColumns>();
                XIInfraCache oCache = new XIInfraCache();
                // int i = Params.iDisplayStart + 1;
                string sCompareWithKey = sCompareWith;
                if (!string.IsNullOrEmpty(ParentIDs))
                {
                    if (!string.IsNullOrEmpty(sStructureName) && iInstanceID != 0 && !string.IsNullOrEmpty(ParentIDs) && iBOID != 0)
                    {
                        ListofoInstance = StrctureData(sStructureName, iInstanceID, ParentIDs, iBOID);
                    }
                    string StructureBos = string.Empty;
                    foreach (var oInstanceID in ListofoInstance)
                    {
                        StructureBos += oInstanceID.Keys.FirstOrDefault() + ",";
                    }
                    if (!string.IsNullOrEmpty(StructureBos))
                    {
                        StructureBos = StructureBos.TrimEnd(',');
                        BONames = StructureBos.Split(',');
                    }
                }
                //if (Params.iDisplayLength > 0)
                //{
                //    BONames = BONames.Skip(i - 1).Take(Params.iDisplayLength).ToList().ToArray();
                //}
                if (BONames != null && BONames.Count() > 0)
                {
                    oCResult = NewDataCompareWithBONames(sSourceConnection, sTargetConnection, BONames);
                    oTables = (List<XITableColumns>)oCResult.oResult;
                }
                else
                {
                    oCResult = NewDatabasesCompare(sSourceConnection, sTargetConnection, null, true);
                    oTables = (List<XITableColumns>)oCResult.oResult;
                }

                XID1Click oClick = new XID1Click();
                List<XITableRecords> oRecordsList = new List<XITableRecords>();
                List<XITableRecords> AllRecords = new List<XITableRecords>();
                List<XITableRecords> oAllTableRecords = new List<XITableRecords>();
                Dictionary<string, object> Params1 = new Dictionary<string, object>();
                XIBOInstance oInstance = new XIBOInstance();
                if (!string.IsNullOrEmpty(sStructureName) && sStructureName != "select" && iBOID != 0 && iInstanceID != 0)
                {
                    ListofoInstance = StrctureData(sStructureName, iInstanceID, null, iBOID);
                }
                int displyCount = oTables.Count();
                if (oTables.Count > 0)
                {
                    oTables.Take(10).Where(m => !string.IsNullOrEmpty(m.sPrimaryKey)).GroupBy(m => m.sTableName).Select(m => m.FirstOrDefault()).OrderBy(m => m.sTableName).ToList().ForEach(obj =>
                    {
                        var ComaparedIDs = string.Empty;
                        if (ListofoInstance.Count > 0)
                        {
                            var Compareda = ListofoInstance.Where(n => n.ContainsKey(obj.sBOName)).Select(y => y.Values).ToList();
                            foreach (var item in Compareda)
                            {
                                foreach (var item2 in item)
                                {
                                    foreach (var item3 in item2)
                                    {
                                        ComaparedIDs += item3.Attributes.Where(u => u.Key.ToLower() == obj.sPrimaryKey.ToLower()).Select(y => y.Value.sValue).FirstOrDefault() + ",";
                                    }
                                }
                            }
                        }
                        if (!string.IsNullOrEmpty(ComaparedIDs))
                        {
                            ComaparedIDs = ComaparedIDs.TrimEnd(',');
                        }
                        var SttructureID = oXIDStructureData.Where(r => r.sBO == obj.sBOName).Select(k => k.ID).FirstOrDefault();
                        XITableRecords oRecordCount = new XITableRecords();
                        XITableRecords oCacheTable = new XITableRecords();
                        if (!string.IsNullOrEmpty(ComaparedIDs))
                            oClick.Query = "SELECT * FROM [" + obj.sTableName + "] WHERE " + obj.sPrimaryKey + " in (" + ComaparedIDs + ")";
                        else if (iInstanceID != 0)

                            oClick.Query = "SELECT * FROM [" + obj.sTableName + "] WHERE " + obj.sPrimaryKey + " in (" + iInstanceID + ")";
                        else
                            oClick.Query = "SELECT * FROM [" + obj.sTableName + "]";

                        string sConnectionPool = ";Connection Timeout=300;Connection Lifetime=0;Min Pool Size=0;Max Pool Size=1500;Pooling=true;";

                        /******************************* Get Records from Table in the Sources Database **************************/
                        oClick.sConnectionString = sSourceConnection;
                        if (obj.iDataSource != null && obj.iDataSource.Count() > 0) /*********Rebuild the Connection string based on Datasource of the Table ****************/
                            oClick.sConnectionString = GetConnectionWithiDataSource(sSourceConnection, obj.iDataSource[0]);
                        //oClick.sConnectionString = oClick.sConnectionString.TrimEnd(';') + sConnectionPool;
                        DataTable oSourceDataTable = oClick.GetDataTable();

                        /***********************Get Table Column Information from Database***********************/
                        ConnectionString = oClick.sConnectionString;
                        Query = sSystemTableColumnsQuery + " WHERE tbl.name ='" + obj.sTableName + "'";
                        oCacheTable.Columns = DataTableToList<XITableColumns>(ExecuteReaderToDataTable());

                        /******************************* Get Records from Table in the Target Database **************************/
                        oClick.sConnectionString = sTargetConnection;
                        if (obj.iDataSource != null && obj.iDataSource.Count() > 0)/*********Rebuild the Connection string based on Datasource of the Table ****************/
                            oClick.sConnectionString = GetConnectionWithiDataSource(sTargetConnection, obj.iDataSource[1]);

                        // oClick.sConnectionString = oClick.sConnectionString.TrimEnd(';') + sConnectionPool;
                        DataTable oTargetDataTable = oClick.GetDataTable();
                        string sExceptColumn = (sCompareWith == XIConstant.GUID) ? obj.sPrimaryKey : "";

                        List<string> SourceColumns = oSourceDataTable.Columns.Cast<DataColumn>().Where(m => m.ColumnName.ToLower() != sExceptColumn.ToLower()).Select(m => m.ColumnName).ToList();
                        List<string> TargetColumns = oTargetDataTable.Columns.Cast<DataColumn>().Where(m => m.ColumnName.ToLower() != sExceptColumn.ToLower()).Select(m => m.ColumnName).ToList();

                        /***************************Get Primary Key or GUID Column from Table based on sCompareWith ********************************/
                        if (sCompareWith != XIConstant.GUID)
                            sCompareWithKey = SourceColumns.Where(m => m.ToLower() == obj.sPrimaryKey.ToLower()).FirstOrDefault();
                        else
                            sCompareWithKey = SourceColumns.Where(m => m.ToLower() == sCompareWith.ToLower()).FirstOrDefault();

                        if (!string.IsNullOrEmpty(sCompareWithKey) && SourceColumns.Count == TargetColumns.Count && SourceColumns.All(m => TargetColumns.Any(s => s.ToLower() == m.ToLower())) && oSourceDataTable.Rows.Count > 0)
                        {

                            /*******Get All Primary Keys or GUIDs based on ComparewithKey from Source and Target DataTables*************/
                            List<string> SourceIDs = (from Source in oSourceDataTable.AsEnumerable()
                                                      select Source[sCompareWithKey].ToString()).ToList();

                            List<string> TargetIDs = (from Target in oTargetDataTable.AsEnumerable()
                                                      select Target[sCompareWithKey].ToString()).ToList();

                            /******************Get Count of Only Present in Source and Only Present in Target ******************/
                            oRecordCount.iSourceCount = SourceIDs.Except(TargetIDs).Count();
                            oRecordCount.iTargetCount = TargetIDs.Except(SourceIDs).Count();

                            /*******Get Primary Keys or GUIDs of Only Present in Source and Target To set Cache for future use***********/
                            oCacheTable.OnlySourceIDs = SourceIDs.Except(TargetIDs);
                            oCacheTable.OnlyTargetIDs = TargetIDs.Except(SourceIDs);

                            /***********Get Equal Primary Keys or GUIDs from SourceIDs and TargetIDs**********/
                            var BothIDs = (from Source in SourceIDs
                                           join Target in TargetIDs
                                           on Source equals Target
                                           select Source
                                             ).ToList();

                            /*******************Get Primary Keys or GUIDs of Equal Column values of Source and Target DataTables *******************/
                            var SourceTargetJoinBoth = (from Source in oSourceDataTable.AsEnumerable()
                                                        join Target in oTargetDataTable.AsEnumerable()
                                                        on string.Join("\0", SourceColumns.OrderBy(m => m).Select(c => Source[c]))
                                                               equals
                                                            string.Join("\0", TargetColumns.OrderBy(m => m).Select(c => Target[c]))
                                                        select Source[sCompareWithKey].ToString()).ToList();
                            /*****************Get Count of differences of Primary Keys or GUIDs that are not equal Column Values *************************/
                            oRecordCount.iDiffCount = BothIDs.Except(SourceTargetJoinBoth).Count();

                            /************************Caching the Diff Primary Keys or GUIDs and  Source and Target Datatable************************/
                            oCacheTable.DiffIDs = BothIDs.Except(SourceTargetJoinBoth);//for cache
                            oCacheTable.Tables = new List<DataTable> { oSourceDataTable, oTargetDataTable };

                            /*****************Get Count of equality of Source and Target Records of GUIDs or Primary Keys************************/
                            oRecordCount.iEqualCount = SourceTargetJoinBoth.Count();

                            oRecordCount.sTableName = obj.sTableName;
                            oRecordCount.sBOName = obj.sBOName;
                            oRecordCount.sPrimaryKey = sCompareWithKey;
                            oRecordCount.sSource = sSourceConnection;
                            oRecordCount.sTarget = sTargetConnection;
                            oRecordCount.sStructureID = SttructureID.ToString();
                            oRecordsList.Add(oRecordCount);

                            oCacheTable.sTableName = obj.sTableName;
                            oCacheTable.sPrimaryKey = obj.sPrimaryKey;
                            oCacheTable.sBOName = obj.sBOName;
                            oCacheTable.sSource = sSourceConnection;
                            oCacheTable.sTarget = sTargetConnection;
                            oCacheTable.sStructureID = SttructureID.ToString();
                            oCacheTable.sCompareWith = sCompareWithKey;
                            oCacheTable.sTargetConnectionString = oClick.sConnectionString;


                            oAllTableRecords.Add(oCacheTable);/******Set Each Table information to cache***********/
                        }
                    });
                }
                oCache.CacheDatabaseRecords(oAllTableRecords);
                oCResult.oResult = oRecordsList;
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
            oCResult.oResult = (int)xiEnumSystem.xiFuncResult.xiSuccess;
            return oCResult;
        }


        /*:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
          :: Compare Each Table Records from Source and Target Database and                              ::
          :: Retrun the Count of Only Source, Only Target and Both Records Based on Guid and Primary key ::
          :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::*/


        public CResult NewDataCompare(jQueryDataTableParamModels Params, string sSourceConnection, string sTargetConnection, string sCompareWith, string[] BONames, int iInstanceID, string sStructureName, int iBOID, string ParentIDs)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Two Data Base Comparing";//expalin about this method logic
            try
            {
                oTrace.oParams.Add(new CNV { sName = "sSourceConnection", sValue = sSourceConnection });
                oTrace.oParams.Add(new CNV { sName = "sTargetConnection", sValue = sTargetConnection });
                oTrace.oParams.Add(new CNV { sName = "sCompareWith", sValue = sCompareWith });
                oTrace.oParams.Add(new CNV { sName = "iInstanceID", sValue = iInstanceID.ToString() });
                oTrace.oParams.Add(new CNV { sName = "sStructureName", sValue = sStructureName });
                oTrace.oParams.Add(new CNV { sName = "iBOID", sValue = iBOID.ToString() });
                oTrace.oParams.Add(new CNV { sName = "ParentIDs", sValue = ParentIDs });
                if (!string.IsNullOrEmpty(sSourceConnection) && !string.IsNullOrEmpty(sTargetConnection) && !string.IsNullOrEmpty(sCompareWith))//check mandatory params are passed or not
                {
                    List<XITableColumns> oTables = new List<XITableColumns>();
                    XIInfraCache oCache = new XIInfraCache();
                    int i = Params.iDisplayStart + 1;
                    string sCompareWithKey = sCompareWith.ToLower();
                    if (!string.IsNullOrEmpty(ParentIDs))
                    {
                        if (!string.IsNullOrEmpty(sStructureName) && iInstanceID != 0 && !string.IsNullOrEmpty(ParentIDs) && iBOID != 0)
                        {
                            //****************** Structure Loading based on Instance *******
                            ListofoInstance = StrctureData(sStructureName, iInstanceID, ParentIDs, iBOID);
                        }
                        string StructureBos = string.Empty;
                        foreach (var oInstanceID in ListofoInstance)
                        {
                            StructureBos += oInstanceID.Keys.FirstOrDefault() + ",";
                        }
                        if (!string.IsNullOrEmpty(StructureBos))
                        {
                            StructureBos = StructureBos.TrimEnd(',');
                            BONames = StructureBos.Split(',');
                        }
                    }
                    if (BONames != null && BONames.Count() > 0)
                    {
                        oCResult = NewDataCompareWithBONames(sSourceConnection, sTargetConnection, BONames);
                        oTables = (List<XITableColumns>)oCResult.oResult;
                    }
                    else
                    {
                        oCResult = NewDatabasesCompare(sSourceConnection, sTargetConnection, null, true);
                        oTables = (List<XITableColumns>)oCResult.oResult;
                    }

                    int displyCount = oTables.Count();
                    if (oTables.Count > 0)
                    {
                        oTables = oTables.Skip(i - 1).Take(Params.iDisplayLength).ToList();
                    }
                    XID1Click oClick = new XID1Click();
                    List<XITableRecords> oRecordsList = new List<XITableRecords>();
                    List<XITableRecords> AllRecords = new List<XITableRecords>();
                    List<XITableRecords> oAllTableRecords = new List<XITableRecords>();
                    if (!string.IsNullOrEmpty(sStructureName) && sStructureName != "select" && iBOID != 0 && iInstanceID != 0)
                    {
                        ListofoInstance = StrctureData(sStructureName, iInstanceID, null, iBOID);
                    }
                    if (oTables.Count > 0)
                    {
                        oTables.Where(m => !string.IsNullOrEmpty(m.sPrimaryKey)).GroupBy(m => m.sTableName).Select(m => m.FirstOrDefault()).OrderBy(m => m.sTableName).ToList().ForEach(obj =>
                        {
                            var ComaparedIDs = string.Empty;
                            if (ListofoInstance.Count > 0)
                            {
                                var Compareda = ListofoInstance.Where(n => n.ContainsKey(obj.sBOName)).Select(y => y.Values).ToList();
                                foreach (var item in Compareda)
                                {
                                    foreach (var item2 in item)
                                    {
                                        foreach (var item3 in item2)
                                        {
                                            ComaparedIDs += item3.Attributes.Where(u => u.Key.ToLower() == obj.sPrimaryKey.ToLower()).Select(y => y.Value.sValue).FirstOrDefault() + ",";
                                        }
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(ComaparedIDs))
                            {
                                ComaparedIDs = ComaparedIDs.TrimEnd(',');
                            }
                            var SttructureID = oXIDStructureData.Where(r => r.sBO == obj.sBOName).Select(k => k.ID).FirstOrDefault();
                            XITableRecords oRecordCount = new XITableRecords();
                            XITableRecords oCacheTable = new XITableRecords();
                            if (!string.IsNullOrEmpty(ComaparedIDs))
                                oClick.Query = "SELECT * FROM [" + obj.sTableName + "] WHERE " + obj.sPrimaryKey + " in (" + ComaparedIDs + ")";
                            else if (iInstanceID != 0)

                                oClick.Query = "SELECT * FROM [" + obj.sTableName + "] WHERE " + obj.sPrimaryKey + " in (" + iInstanceID + ")";
                            else
                                oClick.Query = "SELECT * FROM [" + obj.sTableName + "]";

                            string sConnectionPool = ";Connection Timeout=300;Connection Lifetime=0;Min Pool Size=0;Max Pool Size=1500;Pooling=true;";

                        /******************************* Get Records from Table in the Sources Database **************************/
                            oClick.sConnectionString = sSourceConnection;
                            if (obj.iDataSource != null && obj.iDataSource.Count() > 0) /*********Rebuild the Connection string based on Datasource of the Table ****************/
                                oClick.sConnectionString = GetConnectionWithiDataSource(sSourceConnection, obj.iDataSource[1]);
                        //oClick.sConnectionString = oClick.sConnectionString.TrimEnd(';') + sConnectionPool;
                        DataTable oSourceDataTable = oClick.GetDataTable();

                        /***********************Get Table Column Information from Database***********************/
                            ConnectionString = oClick.sConnectionString;
                            Query = sSystemTableColumnsQuery + " WHERE tbl.name ='" + obj.sTableName + "'";
                            oCacheTable.Columns = DataTableToList<XITableColumns>(ExecuteReaderToDataTable());

                        /******************************* Get Records from Table in the Target Database **************************/
                            oClick.sConnectionString = sTargetConnection;
                            if (obj.iDataSource != null && obj.iDataSource.Count() > 0)/*********Rebuild the Connection string based on Datasource of the Table ****************/
                                oClick.sConnectionString = GetConnectionWithiDataSource(sTargetConnection, obj.iDataSource[0]);

                        // oClick.sConnectionString = oClick.sConnectionString.TrimEnd(';') + sConnectionPool;
                        DataTable oTargetDataTable = oClick.GetDataTable();
                            string sExceptColumn = (sCompareWith.ToLower() == XIConstant.GUID.ToLower()) ? obj.sPrimaryKey : "";

                            List<string> SourceColumns = oSourceDataTable.Columns.Cast<DataColumn>().Where(m => m.ColumnName.ToLower() != sExceptColumn.ToLower()).Select(m => m.ColumnName).ToList();
                            List<string> TargetColumns = oTargetDataTable.Columns.Cast<DataColumn>().Where(m => m.ColumnName.ToLower() != sExceptColumn.ToLower()).Select(m => m.ColumnName).ToList();

                        /***************************Get Primary Key or GUID Column from Table based on sCompareWith ********************************/
                            if (sCompareWith.ToLower() != XIConstant.GUID.ToLower())
                                sCompareWithKey = SourceColumns.Where(m => m.ToLower() == obj.sPrimaryKey.ToLower()).FirstOrDefault();
                            else
                                sCompareWithKey = SourceColumns.Where(m => m.ToLower() == sCompareWith.ToLower()).FirstOrDefault();

                            if (!string.IsNullOrEmpty(sCompareWithKey) && SourceColumns.Count == TargetColumns.Count && SourceColumns.All(m => TargetColumns.Any(s => s.ToLower() == m.ToLower())) && oSourceDataTable.Rows.Count > 0)
                            {

                            /*******Get All Primary Keys or GUIDs based on ComparewithKey from Source and Target DataTables*************/
                                List<string> SourceIDs = (from Source in oSourceDataTable.AsEnumerable()
                                                          where Source[sCompareWith].ToString() != string.Empty
                                                          select Source[sCompareWithKey].ToString()).ToList();

                                List<string> TargetIDs = (from Target in oTargetDataTable.AsEnumerable()
                                                          where Target[sCompareWith].ToString() != string.Empty
                                                          select Target[sCompareWithKey].ToString()).ToList();

                            /******************Get Count of Only Present in Source and Only Present in Target ******************/
                                oRecordCount.iSourceCount = SourceIDs.Except(TargetIDs).Count();
                                oRecordCount.iTargetCount = TargetIDs.Except(SourceIDs).Count();

                            /*******Get Primary Keys or GUIDs of Only Present in Source and Target To set Cache for future use***********/
                                oCacheTable.OnlySourceIDs = SourceIDs.Except(TargetIDs);
                                oCacheTable.OnlyTargetIDs = TargetIDs.Except(SourceIDs);

                            /***********Get Equal Primary Keys or GUIDs from SourceIDs and TargetIDs**********/
                                var BothIDs = (from Source in SourceIDs
                                               join Target in TargetIDs
                                               on Source equals Target
                                               select Source
                                                 ).ToList();

                            /*******************Get Primary Keys or GUIDs of Equal Column values of Source and Target DataTables *******************/
                                var SourceTargetJoinBoth = (from Source in oSourceDataTable.AsEnumerable()
                                                            join Target in oTargetDataTable.AsEnumerable()
                                                            on string.Join("\0", SourceColumns.OrderBy(m => m).Select(c => Source[c]))
                                                                   equals
                                                                string.Join("\0", TargetColumns.OrderBy(m => m).Select(c => Target[c]))
                                                            select Source[sCompareWithKey].ToString()).ToList();
                            /*****************Get Count of differences of Primary Keys or GUIDs that are not equal Column Values *************************/
                                oRecordCount.iDiffCount = BothIDs.Except(SourceTargetJoinBoth).Count();

                            /************************Caching the Diff Primary Keys or GUIDs and  Source and Target Datatable************************/
                                oCacheTable.DiffIDs = BothIDs.Except(SourceTargetJoinBoth);//for cache
                            oCacheTable.Tables = new List<DataTable> { oSourceDataTable, oTargetDataTable };

                            /*****************Get Count of equality of Source and Target Records of GUIDs or Primary Keys************************/
                                oRecordCount.iEqualCount = SourceTargetJoinBoth.Count();

                                oRecordCount.sTableName = obj.sTableName;
                                oRecordCount.sBOName = obj.sBOName;
                                oRecordCount.sPrimaryKey = sCompareWithKey;
                                oRecordCount.sSource = sSourceConnection;
                                oRecordCount.sTarget = sTargetConnection;
                                oRecordCount.sStructureID = SttructureID.ToString();
                                oRecordsList.Add(oRecordCount);

                                oCacheTable.sTableName = obj.sTableName;
                                oCacheTable.sPrimaryKey = obj.sPrimaryKey;
                                oCacheTable.sBOName = obj.sBOName;
                                oCacheTable.sSource = sSourceConnection;
                                oCacheTable.sTarget = sTargetConnection;
                                oCacheTable.sStructureID = SttructureID.ToString();
                                oCacheTable.sCompareWith = sCompareWithKey;
                                oCacheTable.sTargetConnectionString = oClick.sConnectionString;


                                oAllTableRecords.Add(oCacheTable);/******Set Each Table information to cache***********/


                            }
                        });
                        //}
                    }
                    oCache.CacheDatabaseRecords(oAllTableRecords);
                    var result = from c in oRecordsList.ToList()
                                 select new[] {
                             (i++).ToString(),
                             c.sTableName,
                             c.iDiffCount.ToString(),
                             c.iSourceCount.ToString(),
                             c.iTargetCount.ToString(),
                             c.iEqualCount.ToString(),
                             c.sPrimaryKey,
                             c.sStructureID
                         };
                    // return oRecordsList;
                    VMDTResponses ResponeData = new VMDTResponses()
                    {
                        sEcho = Params.sEcho,
                        iTotalRecords = displyCount,
                        iTotalDisplayRecords = displyCount,
                        aaData = result
                    };
                    oCResult.oResult = ResponeData;
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = " Mandatory Param: sSourceConnection = " + sSourceConnection + " or sTargetConnection = " + sTargetConnection + " or sCompareWith = " + sCompareWith + " is missing";
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
            return oCResult;
        }

        /*:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
          :: Get Records of Table Based on sActionType to Display  front end ::
          :: sActions --> { Source, Difference  }                            ::  
          :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::*/

        public CResult NewGetTableRecords(string sTableName, string sActionType, string sDBType)
        {
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Gtting Database Table Records";//expalin about this method logic
            object SourceRecords = new object();
            object TargetRecords = new object();
            try
            {
                oTrace.oParams.Add(new CNV { sName = "sTableName", sValue = sTableName });
                oTrace.oParams.Add(new CNV { sName = "sActionType", sValue = sActionType });
                oTrace.oParams.Add(new CNV { sName = "sDBType", sValue = sDBType });
                if (!string.IsNullOrEmpty(sTableName) && !string.IsNullOrEmpty(sActionType))//check mandatory params are passed or not
                {

                    XIInfraCache oCache = new XIInfraCache();
                    List<XITableRecords> AllRecords = (List<XITableRecords>)oCache.GetFromCache(HttpContext.Current.Session.SessionID + "_" + XIConstant.RecordsCacheKey);
                    if (AllRecords != null && AllRecords.Any(m => m.sTableName == sTableName))
                    {

                        XITableRecords oTable = AllRecords.Where(m => m.sTableName == sTableName).FirstOrDefault();
                        List<string> Columns = oTable.Tables[1].Columns.Cast<DataColumn>().Select(m => m.ColumnName).ToList();

                        IEnumerable<string> SourceIDs = new List<string>();
                        IEnumerable<string> TargetIDs = new List<string>();
                        if (sActionType == XIConstant.Source)
                            SourceIDs = oTable.OnlySourceIDs;
                        else if (sActionType == XIConstant.Target)
                            TargetIDs = oTable.OnlyTargetIDs;
                        else if (sActionType == XIConstant.Difference)
                        {
                            SourceIDs = oTable.DiffIDs;
                            TargetIDs = oTable.DiffIDs;
                        }

                        string sExceptColumn = string.Empty;
                        if (oTable.sCompareWith.ToLower() == XIConstant.GUID.ToLower())
                            sExceptColumn = oTable.sPrimaryKey;

                        if (sActionType == XIConstant.Source || sActionType == XIConstant.Difference)
                            SourceRecords = (from Source in oTable.Tables[0].AsEnumerable()
                                             join ID in SourceIDs
                                             on Source[oTable.sCompareWith].ToString() equals ID
                                             select Enumerable.Range(0, Columns.Count).Select(m => new { sName = oTable.Tables[0].Columns[m].ToString(), sValue = Source[m].ToString() }).OrderBy(m => m.sName).ToList()).ToList();

                        if (sActionType == XIConstant.Target || sActionType == XIConstant.Difference)
                            TargetRecords = (from Target in oTable.Tables[1].AsEnumerable()
                                             join ID in TargetIDs
                                             on Target[oTable.sCompareWith].ToString() equals ID
                                             select Enumerable.Range(0, Columns.Count).Select(m => new { sName = oTable.Tables[1].Columns[m].ToString(), sValue = Target[m].ToString() }).OrderBy(m => m.sName).ToList()).ToList();
                    }
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = " Mandatory Param: sTableName = " + sTableName + " or iActionType = " + iActionType + "  is missing";
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
            oCResult.oResult = new { Source = SourceRecords, Target = TargetRecords };
            return oCResult;
        }

        #endregion  Tables_Data_Compare 

        #region Insert_Update_Table_Records
        /*:::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
         :: Generate Insert and Update records Queries for target Database of All Selected Tree Structure Tables   ::
         ::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::*/
        public string InsertUpdateTarget()
        {
            string sSchema = string.Empty;
            CResult oCResult = new CResult();
            try
            {

                XIInfraCache oCache = new XIInfraCache();
                List<XITableRecords> AllRecords = (List<XITableRecords>)oCache.GetFromCache(HttpContext.Current.Session.SessionID + "_" + XIConstant.RecordsCacheKey);
                if (AllRecords.Count > 0)
                {
                    var DeleteRecords = (from SourceIDs in AllRecords
                                         where SourceIDs.OnlyTargetIDs.Count() > 0
                                         select new { result = UpdateSourceTarget(SourceIDs.sTableName, SourceIDs.OnlyTargetIDs.ToList(), XIConstant.Target, "Data") }).Select(m => m.result).ToList();
                    sSchema = string.Join("\n\n", DeleteRecords);
                    var UpdateRecords = (from SourceIDs in AllRecords
                                         where SourceIDs.DiffIDs.Count() > 0
                                         select new { result = UpdateSourceTarget(SourceIDs.sTableName, SourceIDs.DiffIDs.ToList(), XIConstant.Difference, "Data") }).Select(m => m.result).ToList();
                    sSchema += string.Join("\n\n", UpdateRecords);
                    var InsertRecords = (from SourceIDs in AllRecords
                                         where SourceIDs.OnlySourceIDs.Count() > 0
                                         select new { result = UpdateSourceTarget(SourceIDs.sTableName, SourceIDs.OnlySourceIDs.ToList(), XIConstant.Source, "Data") }).Select(m => m.result).ToList();
                    sSchema += string.Join("\n\n", InsertRecords);

                }

            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                SaveErrortoDB(oCResult);
            }
            return sSchema;
        }

        public CResult NewInsertUpdateTarget()
        {
            string sSchema = string.Empty; CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Update Taget DB";//expalin about this method logic
            try
            {

                XIInfraCache oCache = new XIInfraCache();
                List<XITableRecords> AllRecords = (List<XITableRecords>)oCache.GetFromCache(HttpContext.Current.Session.SessionID + "_" + XIConstant.RecordsCacheKey);
                if (AllRecords.Count > 0)
                {
                    var DeleteRecords = (from SourceIDs in AllRecords
                                         where SourceIDs.OnlyTargetIDs.Count() > 0
                                         select new { result = NewUpdateSourceTarget(SourceIDs.sTableName, SourceIDs.OnlyTargetIDs.ToList(), XIConstant.Target, "Data") }).Select(m => m.result.oResult.ToString()).ToList();
                    sSchema = string.Join("\n\n", DeleteRecords);
                    var UpdateRecords = (from SourceIDs in AllRecords
                                         where SourceIDs.DiffIDs.Count() > 0
                                         select new { result = NewUpdateSourceTarget(SourceIDs.sTableName, SourceIDs.DiffIDs.ToList(), XIConstant.Difference, "Data") }).Select(m => m.result.oResult.ToString()).ToList();
                    sSchema += string.Join("\n\n", UpdateRecords);
                    var InsertRecords = (from SourceIDs in AllRecords
                                         where SourceIDs.OnlySourceIDs.Count() > 0
                                         select new { result = NewUpdateSourceTarget(SourceIDs.sTableName, SourceIDs.OnlySourceIDs.ToList(), XIConstant.Source, "Data") }).Select(m => m.result.oResult.ToString()).ToList();
                    sSchema += string.Join("\n\n", InsertRecords);

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
            oCResult.oResult = sSchema;
            return oCResult;
            //return sSchema;
        }

        /*::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::
         :: Generate Insert or Update records Queries for target Database of Individual Table   ::
         :: Based on Selected GUIDs or PrimaryKeys and ActionType                               ::
         :::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::::*/
        public string UpdateSourceTarget(string sTableName, List<string> GUIDs, string sActionType, string sType)
        {
            CResult oCResult = new CResult();
            string sSchema = string.Empty;
            try
            {
                /*********** sActionType Keys of Different Records {  SourceDiff, TargetDiff } and OnlyRecords { Source, Target} **************/
                XIInfraCache oCache = new XIInfraCache();

                string sOppositeType = string.Empty;
                List<XITableRecords> AllRecords = (List<XITableRecords>)oCache.GetFromCache(HttpContext.Current.Session.SessionID + "_" + XIConstant.RecordsCacheKey);

                XITableRecords oTable = AllRecords.Where(m => m.sTableName == sTableName).FirstOrDefault();
                Dictionary<string, List<XIIAttribute>> Records = new Dictionary<string, List<XIIAttribute>>();
                ConnectionString = oTable.sTargetConnectionString;
                string sDatabaseInfo = GetConnectionInfo();
                if (sActionType == XIConstant.Difference)
                {
                    sSchema = (from Source in oTable.Tables[0].AsEnumerable().Where(m => GUIDs.Any(s => s == m[oTable.sCompareWith].ToString()))
                               join Target in oTable.Tables[1].AsEnumerable().Where(m => GUIDs.Any(s => s == m[oTable.sCompareWith].ToString()))
                               on Source[oTable.sCompareWith].ToString() equals Target[oTable.sCompareWith].ToString()
                               select new
                               {
                                   ID = Source[oTable.sCompareWith].ToString(),
                                   value = Enumerable.Range(0, oTable.Tables[0].Columns.Count)
                                   .Where(m => Source[oTable.Tables[0].Columns[m].ColumnName].ToString() != Target[oTable.Tables[0].Columns[m].ColumnName].ToString())
                                   .Select(m => new XIIAttribute
                                   {
                                       sName = oTable.Tables[0].Columns[m].ColumnName,
                                       sValue = Source[m].ToString()
                                   }).ToList()
                               }).ToDictionary(m => m.ID, m => m.value)
                               .UpdateQuery(oTable.sPrimaryKey, oTable.sCompareWith, oTable.sTableName, oTable.Columns);
                }
                else if (sActionType == XIConstant.Source)
                {
                    sSchema = (from Source in oTable.Tables[0].AsEnumerable()
                               join IDs in oTable.OnlySourceIDs
                               on Source[oTable.sCompareWith].ToString() equals IDs.ToString()
                               select new
                               {
                                   ID = Source[oTable.sCompareWith].ToString(),
                                   value = Enumerable.Range(0, oTable.Tables[0].Columns.Count)
                                          .Select(m => new XIIAttribute { sName = oTable.Tables[0].Columns[m].ColumnName, sValue = Source[m].ToString() }).ToList()
                               }).ToDictionary(m => m.ID, m => m.value)
                               .InsertQuery(oTable.sPrimaryKey, oTable.sCompareWith, oTable.sTableName, oTable.Columns, oTable.sSource);
                }
                else if (sActionType == XIConstant.Target)
                {
                    List<string> DeleteRecords = oTable.OnlyTargetIDs.ToList();
                    sSchema = DeleteTargetRecords(DeleteRecords, oTable.sPrimaryKey, oTable.sCompareWith, oTable.sTableName, oTable.Columns);
                }
                if (!string.IsNullOrEmpty(sSchema))
                    sSchema = "/*************************************************\n" + sDatabaseInfo + "\n*************************************************/\n" + sSchema;
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                SaveErrortoDB(oCResult);
            }
            return sSchema;
        }

        public CResult NewUpdateSourceTarget(string sTableName, List<string> GUIDs, string sActionType, string sType)
        {
            string sSchema = string.Empty;
            CResult oCResult = new CResult();
            CResult oCR = new CResult();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            CTraceStack oTrace = new CTraceStack();
            oTrace.sClass = this.GetType().Name;
            oTrace.sMethod = MethodBase.GetCurrentMethod().Name;
            oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiInProcess;
            oTrace.sTask = "Update Source to target";//expalin about this method logic
            try
            {
                oTrace.oParams.Add(new CNV { sName = "sTableName", sValue = sTableName });
                if (!string.IsNullOrEmpty(sTableName) && !string.IsNullOrEmpty(sActionType))//check mandatory params are passed or not
                {
                    /*********** sActionType Keys of Different Records {  SourceDiff, TargetDiff } and OnlyRecords { Source, Target} **************/
                    XIInfraCache oCache = new XIInfraCache();

                    string sOppositeType = string.Empty;
                    List<XITableRecords> AllRecords = (List<XITableRecords>)oCache.GetFromCache(HttpContext.Current.Session.SessionID + "_" + XIConstant.RecordsCacheKey);

                    XITableRecords oTable = AllRecords.Where(m => m.sTableName == sTableName).FirstOrDefault();
                    Dictionary<string, List<XIIAttribute>> Records = new Dictionary<string, List<XIIAttribute>>();
                    ConnectionString = oTable.sTargetConnectionString;
                    string sDatabaseInfo = GetConnectionInfo();
                    if (sActionType == XIConstant.Difference)
                    {
                        sSchema = (from Source in oTable.Tables[0].AsEnumerable().Where(m => GUIDs.Any(s => s == m[oTable.sCompareWith].ToString()))
                                   join Target in oTable.Tables[1].AsEnumerable().Where(m => GUIDs.Any(s => s == m[oTable.sCompareWith].ToString()))
                                   on Source[oTable.sCompareWith].ToString() equals Target[oTable.sCompareWith].ToString()
                                   select new
                                   {
                                       ID = Source[oTable.sCompareWith].ToString(),
                                       value = Enumerable.Range(0, oTable.Tables[0].Columns.Count)
                                       .Where(m => Source[oTable.Tables[0].Columns[m].ColumnName].ToString() != Target[oTable.Tables[0].Columns[m].ColumnName].ToString())
                                       .Select(m => new XIIAttribute
                                       {
                                           sName = oTable.Tables[0].Columns[m].ColumnName,
                                           sValue = Source[m].ToString()
                                       }).ToList()
                                   }).ToDictionary(m => m.ID, m => m.value)
                                   .UpdateQuery(oTable.sPrimaryKey, oTable.sCompareWith, oTable.sTableName, oTable.Columns);
                    }
                    else if (sActionType == XIConstant.Source)
                    {
                        sSchema = (from Source in oTable.Tables[0].AsEnumerable()
                                   join IDs in oTable.OnlySourceIDs
                                   on Source[oTable.sCompareWith].ToString() equals IDs.ToString()
                                   select new
                                   {
                                       ID = Source[oTable.sCompareWith].ToString(),
                                       value = Enumerable.Range(0, oTable.Tables[0].Columns.Count)
                                              .Select(m => new XIIAttribute { sName = oTable.Tables[0].Columns[m].ColumnName, sValue = Source[m].ToString() }).ToList()
                                   }).ToDictionary(m => m.ID, m => m.value)
                                   .InsertQuery(oTable.sPrimaryKey, oTable.sCompareWith, oTable.sTableName, oTable.Columns, oTable.sSource);
                    }
                    else if (sActionType == XIConstant.Target)
                    {
                        List<string> DeleteRecords = oTable.OnlyTargetIDs.ToList();
                        sSchema = DeleteTargetRecords(DeleteRecords, oTable.sPrimaryKey, oTable.sCompareWith, oTable.sTableName, oTable.Columns);
                    }
                    if (!string.IsNullOrEmpty(sSchema))
                        sSchema = "/*************************************************\n" + sDatabaseInfo + "\n*************************************************/\n" + sSchema;
                }
                else
                {
                    oTrace.iStatus = (int)xiEnumSystem.xiFuncResult.xiLogicalError;
                    oTrace.sMessage = "Mandatory Param: sTableName = " + sTableName + " or sActionType=" + sActionType + " is missing";
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
            oCResult.oResult = sSchema;
            return oCResult;
            //return sSchema;
        }


        //public string UpdateQuery(Dictionary<string, List<XIIAttribute>> Records, string sPrimarKey, string sCompareWith, string sTableName)
        //{
        //    string sResult = string.Empty;
        //    CResult oCResult = new CResult();
        //    try
        //    {
        //        string sExceptColumn = string.Empty;
        //        if (sCompareWith.ToLower() == XIConstant.GUID.ToLower())
        //            sExceptColumn = sPrimarKey;
        //        List<string> UpdateQueries = Records.Select(m =>
        //         {
        //             StringBuilder sUpdateSchema = new StringBuilder();
        //             sUpdateSchema.Append("UPDATE [" + sTableName + "] SET ");
        //             List<string> ColumnValues = m.Value.Where(s => s.sName.ToLower() != sExceptColumn.ToLower()).Select(s => { return "[" + s.sName + "]='" + s.sValue.Replace("'", "''").Replace("\t", " ") + "'"; }).ToList();
        //             sUpdateSchema.Append(string.Join(", ", ColumnValues));
        //             sUpdateSchema.Append(" WHERE " + sPrimarKey + "='" + m.Key + "'");

        //             return sUpdateSchema.ToString();
        //         }).ToList();
        //        sResult = string.Join("\n", UpdateQueries);
        //    }
        //    catch (Exception ex)
        //    {
        //        oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
        //        oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
        //        SaveErrortoDB(oCResult);
        //    }
        //    return sResult;
        //}
        //public string InsertQuery(Dictionary<string, List<XIIAttribute>> Records, string sPrimarKey, string sCompareWith, string sTableName)
        //{
        //    string sResult = string.Empty;
        //    CResult oCResult = new CResult();
        //    try
        //    {
        //        string sExceptColumn = sPrimarKey;
        //        List<string> InsertQueries = Records.Select(m =>
        //        {
        //            StringBuilder sInsertSchema = new StringBuilder();
        //            List<string> Columns = m.Value.Where(s => s.sName.ToLower() != sExceptColumn.ToLower()).Select(s => { return "[" + s.sName + "]"; }).ToList();
        //            sInsertSchema.Append("INSERT INTO [" + sTableName + "] (" + string.Join(", ", Columns) + ") Values(");
        //            List<string> Values = m.Value.Where(s => s.sName.ToLower() != sExceptColumn.ToLower()).Select(s => { return (s.sValue == null || s.sValue == "") ? "NULL" : "'" + s.sValue.Replace("\t", " ").Replace("'", "''") + "'"; }).ToList();
        //            sInsertSchema.Append(string.Join(", ", Values) + ")");

        //            return sInsertSchema.ToString();
        //        }).ToList();
        //        sResult = string.Join("\n", InsertQueries);
        //    }
        //    catch (Exception ex)
        //    {
        //        oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
        //        oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
        //        SaveErrortoDB(oCResult);

        //    }
        //    return sResult;

        //}

        /* Deleting Target records */
        public static string DeleteTargetRecords(List<string> Records, string sPrimarKey, string sCompareWith, string sTableName, List<XITableColumns> TableColumns)
        {
            string sResult = string.Empty;
            CResult oCResult = new CResult();
            try
            {
                string sExceptColumn = string.Empty;
                if (sCompareWith.ToLower() == XIConstant.GUID.ToLower())
                    sExceptColumn = sPrimarKey;
                List<string> UpdateQueries = Records.Select(m =>
                {
                    StringBuilder sUpdateSchema = new StringBuilder();
                    sUpdateSchema.Append("Delete [" + sTableName + "] ");
                    sUpdateSchema.Append(" WHERE " + sCompareWith + "='" + m + "'");
                    return sUpdateSchema.ToString();
                }).ToList();
                sResult = string.Join("\n", UpdateQueries) + "\n";
            }
            catch (Exception ex)
            {
                XIInstanceBase oBase = new XIInstanceBase();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oBase.SaveErrortoDB(oCResult);
            }
            return sResult;
        }
        public string DeleteQuery(List<XIIBO> DeleteRecords)
        {
            CResult oCResult = new CResult();
            try
            {
                return "";
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                SaveErrortoDB(oCResult);
                throw;
            }
        }
        #endregion Insert_Update_Table_Records
        public Dictionary<string, string> objectEqual(XITableColumns obj1)
        {
            CResult oCResult = new CResult();
            try
            {
                var obj2 = new XITableColumns
                {
                    sDataType = "Default",
                    sDefaultValue = "Default",
                    lMaxLength = -1,
                    bIsNullable = !obj1.bIsNullable,
                    bIsIdentity = !obj1.bIsIdentity,
                };
                var isEqual = obj1.Equals(obj2);
                return obj1.Properties;
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                SaveErrortoDB(oCResult);
                throw;
            }
        }
        public string GetConnectionWithiDataSource(string ConnectionString, int iDataSource)
        {
            CResult oCResult = new CResult();
            try
            {
                XIDBAPI Connection = new XIDBAPI(ConnectionString);
                Dictionary<string, object> Params = new Dictionary<string, object>();
                Params["ID"] = iDataSource.ToString();
                string sSelectFields = string.Empty;
                sSelectFields = "sConnectionString";
                XIDataSource oXIDBOTarget = Connection.Select<XIDataSource>("XIDataSource_XID_T", Params, sSelectFields).FirstOrDefault();
                XIEncryption oEncrypt = new XIEncryption();
                return oEncrypt.DecryptData(oXIDBOTarget.sConnectionString, true, iDataSource.ToString());
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                SaveErrortoDB(oCResult);
                throw;
            }
        }
    }
    public static class DictinaryExtension /*******Extension method for Dictionary<string, List<XIIAttribute>> *************/
    {
        /*::::::::::::::::::::::::::::::::::::::::::::::::::::::
          :: Generate Update Queries Script Based on Records  ::
          ::::::::::::::::::::::::::::::::::::::::::::::::::::::*/
        public static string UpdateQuery(this Dictionary<string, List<XIIAttribute>> Records, string sPrimarKey, string sCompareWith, string sTableName, List<XITableColumns> TableColumns)
        {
            string sResult = string.Empty;
            CResult oCResult = new CResult();
            List<string> DateTimeColumns = TableColumns.Where(m => m.sDataType.ToLower() == "datetime").Select(m => m.sColumnName.ToLower()).ToList();
            List<string> NullColumns = TableColumns.Where(m => m.bIsNullable).Select(m => m.sColumnName.ToLower()).ToList();
            try
            {
                string sExceptColumn = string.Empty;
                if (sCompareWith.ToLower() == XIConstant.GUID.ToLower())
                    sExceptColumn = sPrimarKey;
                List<string> UpdateQueries = Records.Select(m =>
                {
                    StringBuilder sUpdateSchema = new StringBuilder();
                    sUpdateSchema.Append("UPDATE [" + sTableName + "] SET ");
                    List<string> ColumnValues = m.Value.Where(s => s.sName.ToLower() != sExceptColumn.ToLower()).Select(s =>
                    {
                        DateTime dDateTime;
                        if (DateTimeColumns.Any(d => d == s.sName.ToLower()))
                            if (DateTime.TryParse(s.sValue, out dDateTime))
                                s.sValue = dDateTime.ToString(XIConstant.SqlDateFormat);
                        if (string.IsNullOrEmpty(s.sValue) && NullColumns.Any(k => k == s.sName.ToLower()))
                            s.sValue = null;
                        return "[" + s.sName + "] = " + ((s.sValue == null) ? "NULL" : ((s.sValue == "") ? "N''" : "N'" + s.sValue.Replace("\t", " ").Replace("'", "''") + "'"));
                    }).ToList();
                    sUpdateSchema.Append(string.Join(", ", ColumnValues));
                    sUpdateSchema.Append(" WHERE " + sCompareWith + "='" + m.Key + "'");

                    return sUpdateSchema.ToString();
                }).ToList();
                sResult = string.Join("\n", UpdateQueries) + "\n";
            }
            catch (Exception ex)
            {
                XIInstanceBase oBase = new XIInstanceBase();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oBase.SaveErrortoDB(oCResult);
            }
            return sResult;
        }
        /*::::::::::::::::::::::::::::::::::::::::::::::::::::::
          :: Generate Insert Queries Script Based on Records  ::
          ::::::::::::::::::::::::::::::::::::::::::::::::::::::*/
        public static string InsertQuery(this Dictionary<string, List<XIIAttribute>> Records, string sPrimarKey, string sCompareWith, string sTableName, List<XITableColumns> TableColumns, string sSource = null)
        {
            string sResult = string.Empty;
            CResult oCResult = new CResult();
            try
            {
                string sExceptColumn = sPrimarKey;
                List<string> DateTimeColumns = TableColumns.Where(m => m.sDataType.ToLower() == "datetime").Select(m => m.sColumnName.ToLower()).ToList();
                List<string> NullColumns = TableColumns.Where(m => m.bIsNullable).Select(m => m.sColumnName.ToLower()).ToList();
                List<string> InsertQueries = Records.Select(m =>
                {
                    StringBuilder sInsertSchema = new StringBuilder();
                    List<string> Columns = m.Value/*.Where(s => s.sName.ToLower() != sExceptColumn.ToLower())*/.Select(s => { return "[" + s.sName + "]"; }).ToList();
                    sInsertSchema.Append("INSERT INTO [" + sTableName + "] (" + string.Join(", ", Columns) + ") Values(");
                    List<string> Values = m.Value/*.Where(s => s.sName.ToLower() != sExceptColumn.ToLower())*/.Select(s =>
                    {
                        DateTime dDateTime;
                        if (DateTimeColumns.Any(d => d == s.sName.ToLower()))
                            if (DateTime.TryParse(s.sValue, out dDateTime))
                                s.sValue = dDateTime.ToString(XIConstant.SqlDateFormat);
                        if (string.IsNullOrEmpty(s.sValue) && NullColumns.Any(k => k == s.sName.ToLower()))
                            s.sValue = null;
                        return (s.sValue == null) ? "NULL" : (s.sValue == "") ? "N''" : "N'" + s.sValue.Replace("\t", " ").Replace("'", "''") + "'";
                    }).ToList();
                    sInsertSchema.Append(string.Join(", ", Values) + ")");

                    return sInsertSchema.ToString();
                }).ToList();
                sResult = string.Join("\n", "SET IDENTITY_INSERT [dbo].[" + sTableName + "] ON ") + "\n";
                sResult += string.Join("\n", InsertQueries) + "\n";
                sResult += string.Join("\n", "SET IDENTITY_INSERT [dbo].[" + sTableName + "] OFF ") + "\n";
            }
            catch (Exception ex)
            {
                XIInstanceBase oBase = new XIInstanceBase();
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oBase.SaveErrortoDB(oCResult);

            }
            return sResult;

        }
    }
}