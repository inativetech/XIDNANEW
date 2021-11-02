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
using XIDatabase;
using System.Configuration;
using static XIDatabase.XIDBAPI;
using System.Data.SqlClient;
using Dapper;
using XISystem;
using xiEnumSystem;
using System.Web;
using System.Text.RegularExpressions;

namespace XICore
{
    public class XIIQS : XIInstanceBase
    {
        public int ID { get; set; }
        public int FKiQSDefinitionID { get; set; }
        public int iCurrentStepID { get; set; }
        [DapperIgnore]
        public string sCurrentStepName { get; set; }
        [DapperIgnore]
        public string sQSType { get; set; }
        public string sQSName { get; set; }
        public string FKiUserCookieID { get; set; }
        public int FKiBODID { get; set; }
        public int iBOIID { get; set; }
        public int FKiOriginID { get; set; }
        [DapperIgnore]
        public int iActiveStepID { get; set; }
        [DapperIgnore]
        public string sOrgDatabase { get; set; }
        [DapperIgnore]
        public string sMode { get; set; }
        [DapperIgnore]
        public string sGUID { get; set; }
        public DateTime CreatedTime { get; set; }
        public int FKiSourceID { get; set; }
        public string sExternalRefID { get; set; }
        public int FKiClassID { get; set; }
        public int XIDeleted { get; set; }
        public bool bAdminTakeOver { get; set; }
        public int iStage { get; set; }
        public int? iOverrideStep { get; set; }
        public int FKiOrgID { get; set; }
        [DapperIgnore]
        public int iLeadStatus { get; set; }
        //[DapperIgnore]
        //private string osMode { get; set; }
        //[DapperIgnore]
        //public string sMode
        //{
        //    get
        //    {
        //        return osMode;
        //    }
        //    set
        //    {
        //        osMode = "QuestionSet";
        //    }
        //}
        public List<int> History { get; set; }
        public CResult oCResult = new CResult();
        private XIDQS oMyDefinition;
        public XIDQS QSDefinition
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
        public object oDynamicObject { get; set; }
        [DapperIgnore]
        public string sHtmlPage { get; set; }
        private Dictionary<string, XIIQSStep> oMySteps = new Dictionary<string, XIIQSStep>();

        public Dictionary<string, XIIQSStep> Steps
        {
            get
            {
                return oMySteps;
            }
            set
            {
                oMySteps = value;
            }
        }
        private Dictionary<string, XIIValue> oMyXIValues = new Dictionary<string, XIIValue>(StringComparer.CurrentCultureIgnoreCase);

        public Dictionary<string, XIIValue> XIValues
        {
            get
            {
                return oMyXIValues;
            }
            set
            {
                oMyXIValues = value;
            }
        }
        private Dictionary<string, List<XIIBO>> oStructureInstance = new Dictionary<string, List<XIIBO>>();

        public XIIQSStep StepI(string sStepName)
        {
            XIIQSStep oThisStepI = null/* TODO Change to default(_) if this is not a reference type */;

            // The steps of this QS must be loaded

            sStepName = sStepName.ToLower();

            if (oMySteps.ContainsKey(sStepName) == false)
            {
            }

            if (oMySteps.ContainsKey(sStepName))
            {
            }
            else
            {
            }

            return oThisStepI;
        }

        public Dictionary<string, XIIBO> Get_Collection(string sStepName = "", string sSectionName = "")
        {
            Dictionary<string, XIIBO> oResults = new Dictionary<string, XIIBO>();

            // TO DO - run the 1-click which is on this step (in a property on the 1-click component)


            return oResults;
        }

        XIDBAPI Connection = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString);
        public XIIQS Save(XIIQS oQSInstance, string sCurrentGuestUser)
        {
            // TO DO - return type should be an object

            // TO DO - this method means, there is a full object model in memory with steps and maybe sections
            // and this code needs to persist this into the DB
            XIIQS oQSIns = null;
            Dictionary<string, object> Params = new Dictionary<string, object>();
            //Params["FKiQSDefinitionID"] = oQSInstance.FKiQSDefinitionID;
            if (oQSInstance.FKiBODID > 0)
            {
                Params["FKiBODID"] = oQSInstance.FKiBODID;
                Params["iBOIID"] = oQSInstance.iBOIID;
                oQSIns = Connection.Select<XIIQS>("XIQSInstance_T", Params).FirstOrDefault();
            }
            else
            {
                if (oQSInstance.ID > 0)
                {
                    Params["ID"] = oQSInstance.ID;
                    oQSIns = Connection.Select<XIIQS>("XIQSInstance_T", Params).FirstOrDefault();
                }
            }

            if (oQSIns == null)
            {
                oQSIns = new XIIQS();
                oQSIns.sQSName = oQSInstance.QSDefinition.sName;
                oQSIns.iCurrentStepID = oQSInstance.iCurrentStepID;
                oQSIns.FKiQSDefinitionID = oQSInstance.FKiQSDefinitionID;
                oQSIns.FKiUserCookieID = sCurrentGuestUser;
                oQSIns.FKiBODID = oQSInstance.FKiBODID;
                oQSIns.iBOIID = oQSInstance.iBOIID;
                oQSIns.FKiClassID = oQSInstance.QSDefinition.FKiClassID;
                oQSIns.CreatedTime = DateTime.Now;
                oQSIns.FKiOrgID = oQSInstance.FKiOrgID;
                var oStep = oQSInstance.QSDefinition.Steps.Values.Where(m => m.ID == oQSInstance.iCurrentStepID).FirstOrDefault();
                oQSIns.iStage = oStep.iStage;
                oQSIns = Connection.Insert<XIIQS>(oQSIns, "XIQSInstance_T", "ID");
                oQSInstance.ID = oQSIns.ID;
                //InsertIntoAggregations(oQSIns.ID, sDatabase);
            }
            else
            {
                var oStep = oQSInstance.QSDefinition.Steps.Values.Where(m => m.ID == oQSInstance.iCurrentStepID).FirstOrDefault();
                if (oQSIns.iStage < oStep.iStage)
                {
                    oQSIns.iStage = oStep.iStage;
                }
                else if (oQSIns.iStage > oStep.iStage && oStep.iCutStage <= oQSIns.iStage && oQSIns.iStage < oStep.iLockStage)
                {
                    oQSIns.iStage = oStep.iCutStage;
                }
                oQSIns.sQSName = oQSInstance.sQSName;
                oQSIns.iCurrentStepID = oQSInstance.iCurrentStepID;
                oQSIns.FKiQSDefinitionID = oQSInstance.FKiQSDefinitionID;
                oQSIns.FKiOrgID = oQSInstance.FKiOrgID;
                oQSIns = Connection.Update<XIIQS>(oQSIns, "XIQSInstance_T", "ID");
                oQSInstance.ID = oQSIns.ID;
                oQSInstance.iStage = oQSIns.iStage;
            }
            foreach (var oStep in oQSInstance.Steps.Values.Where(m => m.bIsCurrentStep == true))
            {
                List<XIIBO> oBulkBO = new List<XIIBO>();
                XIInfraCache oCache = new XIInfraCache();
                XIIBO BulkInsert = new XIIBO();
                XIIQSStep oQSStepIns;
                XIInfraEncryption oEncrypt = new XIInfraEncryption();
                Dictionary<string, object> StepParams = new Dictionary<string, object>();
                StepParams["FKiQSInstanceID"] = oQSIns.ID;
                StepParams["FKiQSStepDefinitionID"] = oStep.FKiQSStepDefinitionID;
                oQSStepIns = Connection.Select<XIIQSStep>("XIQSStepInstance_T", StepParams).FirstOrDefault(); ;// dbContext.QSStepInstance.Where(m => m.FKiQSInstanceID == oQSIns.ID && m.FKiQSStepDefinitionID == oStep.FKiQSStepDefinitionID).FirstOrDefault();
                if (oQSStepIns == null)
                {
                    oQSStepIns = new XIIQSStep();
                    oQSStepIns.FKiQSInstanceID = oQSIns.ID;
                    oQSStepIns.FKiQSStepDefinitionID = oStep.FKiQSStepDefinitionID;
                    oQSStepIns.FKiOrgID = oQSIns.FKiOrgID;
                    oQSStepIns = Connection.Insert<XIIQSStep>(oQSStepIns, "XIQSStepInstance_T", "ID");
                    oStep.ID = oQSStepIns.ID;
                }
                //var AllFVlaueInstances = dbContext.XIFieldInstance.Where(m => m.FKiQSInstanceID == oQSIns.ID && m.FKiQSStepDefinitionID == oStep.FKiQSStepDefinitionID).ToList();
                //dbContext.XIFieldInstance.RemoveRange(AllFVlaueInstances);
                //dbContext.SaveChanges();

                if (oStep.Sections != null && oStep.Sections.Values.Count() > 0)
                {
                    foreach (var sec in oStep.Sections)
                    {
                        XIIQSSection oSecIns = new XIIQSSection();
                        Dictionary<string, object> SecParams = new Dictionary<string, object>();
                        SecParams["FKiStepSectionDefinitionID"] = sec.Value.FKiStepSectionDefinitionID;
                        SecParams["FKiStepInstanceID"] = oStep.ID;
                        oSecIns = Connection.Select<XIIQSSection>("XIStepSectionInstance_T", SecParams).FirstOrDefault();
                        //oSecIns = dbContext.StepSectionInstance.Where(m => m.FKiStepSectionDefinitionID == sec.FKiStepSectionDefinitionID && m.FKiStepInstanceID == oStep.ID).FirstOrDefault();
                        if (oSecIns == null)
                        {
                            oSecIns = new XIIQSSection();
                            oSecIns.FKiStepSectionDefinitionID = sec.Value.FKiStepSectionDefinitionID;
                            oSecIns.FKiStepInstanceID = oQSStepIns.ID;
                            oSecIns.FKiOrgID = oQSStepIns.FKiOrgID;
                            oSecIns = Connection.Insert<XIIQSSection>(oSecIns, "XIStepSectionInstance_T", "ID");
                            sec.Value.ID = oSecIns.ID;
                            sec.Value.FKiStepInstanceID = oQSStepIns.ID;
                        }

                        if (sec.Value.XIValues != null && sec.Value.XIValues.Count() > 0)
                        {
                            //var SecFValueInstances = dbContext.XIFieldInstance.Where(m => m.FKiQSInstanceID == oQSIns.ID && m.FKiQSSectionDefinitionID == sec.FKiStepSectionDefinitionID).ToList();
                            //dbContext.XIFieldInstance.RemoveRange(SecFValueInstances);
                            //dbContext.SaveChanges();
                            foreach (var items in sec.Value.XIValues)
                            {
                                items.Value.sValue = items.Value.sValue == null ? null : items.Value.sValue.Trim();
                                XIIValue oFIns = new XIIValue();
                                var StepDef = oQSInstance.QSDefinition.Steps.Values.Where(m => m.ID == oStep.FKiQSStepDefinitionID).FirstOrDefault();
                                var SecDef = StepDef.Sections.Values.Where(m => m.ID == sec.Value.FKiStepSectionDefinitionID).FirstOrDefault();
                                var FieldOrigin = SecDef.FieldDefs.Values.Where(m => m.ID == items.Value.FKiFieldDefinitionID).FirstOrDefault().FieldOrigin;
                                if (FieldOrigin.bIsMandatory && items.Value.sValue == null)
                                {
                                    //critical failure and write hack alert to Log
                                    var oCR = new CResult();
                                    oCR.sCode = "Hack";
                                    oCR.sMessage = "Critical Failure: [XIIQS_Save()] there is a mandatory field with no value in it. FieldOrigin ID:" + FieldOrigin.ID + " - FieldOrigin.Name:" + FieldOrigin.sName;
                                    SaveErrortoDB(oCR);
                                }
                                if (FieldOrigin.bIsOptionList)
                                {
                                    items.Value.sDerivedValue = FieldOrigin.FieldOptionList.Where(m => m.sOptionValue == items.Value.sValue).Select(m => m.sOptionName).FirstOrDefault();
                                }
                                Dictionary<string, object> fieldParams = new Dictionary<string, object>();
                                fieldParams["FKiQSInstanceID"] = oQSIns.ID;
                                //fieldParams["FKiQSSectionDefinitionID"] = sec.Value.FKiStepSectionDefinitionID;
                                fieldParams["FKiFieldOriginID"] = items.Value.FKiFieldOriginID;
                                oFIns = Connection.Select<XIIValue>("XIFieldInstance_T", fieldParams).FirstOrDefault();

                                //oFIns = dbContext.XIFieldInstance.Where(m => m.FKiQSInstanceID == oQSIns.ID && m.FKiQSSectionDefinitionID == sec.FKiStepSectionDefinitionID && m.FKiFieldDefinitionID == items.FKiFieldDefinitionID).FirstOrDefault();
                                if (oFIns != null)
                                {
                                    oFIns.FKiStepInstanceID = oStep.ID;
                                    oFIns.FKiSectionInstanceID = oSecIns.ID;
                                    oFIns.FKiQSSectionDefinitionID = sec.Value.FKiStepSectionDefinitionID;
                                    oFIns.FKiQSStepDefinitionID = oStep.FKiQSStepDefinitionID;
                                    oFIns.iStage = StepDef.iStage;
                                    oFIns.bIsDisplay = FieldOrigin.bIsDisplay;
                                    oFIns.bIsModify = FieldOrigin.bIsModify;
                                    oFIns.FKiOrgID = oSecIns.FKiOrgID;
                                    if (FieldOrigin.DataType.sBaseDataType.ToLower() == "int")
                                    {
                                        int ival;
                                        if (int.TryParse(items.Value.sValue, out ival))
                                        {
                                            oFIns.iValue = ival;
                                        }
                                        else
                                        {
                                            oFIns.iValue = 0;
                                        }
                                    }
                                    else if (FieldOrigin.DataType.sBaseDataType.ToLower() == "datetime")
                                    {
                                        //if (!string.IsNullOrEmpty(items.Value.sValue))
                                        //{
                                        //    oFIns.dValue = Convert.ToDateTime(items.Value.sValue);
                                        //}
                                    }
                                    else if (FieldOrigin.DataType.sBaseDataType.ToLower() == "decimal")
                                    {
                                        decimal rval;
                                        if (decimal.TryParse(items.Value.sValue, out rval))
                                        {
                                            oFIns.rValue = rval;
                                        }
                                        else
                                        {
                                            oFIns.rValue = 0;
                                        }
                                    }
                                    else if (FieldOrigin.DataType.sBaseDataType.ToLower() == "boolean")
                                    {
                                        if (items.Value.sValue == "on")
                                        {
                                            oFIns.bValue = true;
                                        }
                                        else
                                        {
                                            oFIns.bValue = false;
                                        }
                                    }
                                    oFIns.sDerivedValue = items.Value.sDerivedValue;
                                    oFIns.sValue = items.Value.sValue;
                                    if (FieldOrigin.bIsEncrypt)
                                    {
                                        oFIns.sValue = oEncrypt.EncryptData(oFIns.sValue, true, oFIns.ID.ToString());
                                    }
                                    if (string.IsNullOrEmpty(oFIns.sDerivedValue))
                                    {
                                        oFIns.sDerivedValue = items.Value.sValue;
                                    }
                                    oFIns.dValue = DateTime.Now;
                                    //oFIns.dValue =Convert.ToDateTime(items.Value.sValue);
                                    //oFIns.sDerivedValue = items.Value.sValue;    
                                    oFIns.XIDeleted = 0;
                                    oFIns = Connection.Update<XIIValue>(oFIns, "XIFieldInstance_T", "ID");
                                }

                                else  /*TO DO INSERT BUIL XIIBO List Object here*/
                                {

                                    XIIBO oxiibo = new XIIBO();
                                    oFIns = new XIIValue();
                                    if (FieldOrigin.DataType.sBaseDataType.ToLower() == "int")
                                    {
                                        int ival;
                                        if (int.TryParse(items.Value.sValue, out ival))
                                        {
                                            oFIns.iValue = ival;
                                        }
                                        else
                                        {
                                            oFIns.iValue = 0;
                                        }
                                    }
                                    else if (FieldOrigin.DataType.sBaseDataType.ToLower() == "datetime")
                                    {
                                        //if (!string.IsNullOrEmpty(items.Value.sValue))
                                        //{
                                        //    oFIns.dValue = Convert.ToDateTime(items.Value.sValue);
                                        //}
                                    }
                                    else if (FieldOrigin.DataType.sBaseDataType.ToLower() == "decimal")
                                    {
                                        decimal rval;
                                        if (decimal.TryParse(items.Value.sValue, out rval))
                                        {
                                            oFIns.rValue = rval;
                                        }
                                        else
                                        {
                                            oFIns.rValue = 0;
                                        }
                                    }
                                    else if (FieldOrigin.DataType.sBaseDataType.ToLower() == "boolean")
                                    {
                                        if (items.Value.sValue == "on")
                                        {
                                            oFIns.bValue = true;
                                        }
                                        else
                                        {
                                            oFIns.bValue = false;
                                        }

                                    }
                                    oxiibo.SetAttribute("rValue", oFIns.rValue.ToString());
                                    oxiibo.SetAttribute("bValue", oFIns.bValue.ToString());
                                    oxiibo.SetAttribute("iValue", oFIns.iValue.ToString());
                                    //if (oFIns.dValue.ToString() == "1/1/0001 12:00:00 AM")
                                    //{
                                    //    oFIns.dValue = Convert.ToDateTime("1/1/1900 12:00:00 AM");
                                    //}

                                    oxiibo.SetAttribute("sDerivedValue", items.Value.sDerivedValue);
                                    oxiibo.SetAttribute("sValue", items.Value.sValue);
                                    oxiibo.SetAttribute("FKiQSSectionDefinitionID", sec.Value.FKiStepSectionDefinitionID.ToString());
                                    oxiibo.SetAttribute("FKiQSInstanceID", oQSIns.ID.ToString());
                                    oxiibo.SetAttribute("FKiFieldDefinitionID", items.Value.FKiFieldDefinitionID.ToString());
                                    oxiibo.SetAttribute("FKiQSStepDefinitionID", oStep.FKiQSStepDefinitionID.ToString());
                                    oxiibo.SetAttribute("FKiStepInstanceID", oStep.ID.ToString());
                                    oxiibo.SetAttribute("FKiSectionInstanceID", oSecIns.ID.ToString());
                                    oxiibo.SetAttribute("FKiFieldOriginID", FieldOrigin.ID.ToString());
                                    oxiibo.SetAttribute("iStage", StepDef.iStage.ToString());
                                    oxiibo.SetAttribute("bIsDisplay", FieldOrigin.bIsDisplay.ToString());
                                    oxiibo.SetAttribute("bIsModify", FieldOrigin.bIsModify.ToString());
                                    oxiibo.SetAttribute("dValue", DateTime.Now.ToString());
                                    oxiibo.SetAttribute("FKiOrgID", oSecIns.FKiOrgID.ToString());
                                    oFIns.FKiOrgID = oSecIns.FKiOrgID;
                                    oFIns.sDerivedValue = items.Value.sDerivedValue;
                                    oFIns.sValue = items.Value.sValue;
                                    if (string.IsNullOrEmpty(oFIns.sDerivedValue))
                                    {
                                        if (FieldOrigin.DataType.sBaseDataType.ToLower() == "boolean")
                                        {
                                            oFIns.sDerivedValue = oFIns.bValue.ToString();
                                            oxiibo.SetAttribute("sDerivedValue", oFIns.bValue.ToString());
                                        }
                                        else
                                        {
                                            oFIns.sDerivedValue = items.Value.sValue;
                                            oxiibo.SetAttribute("sDerivedValue", items.Value.sValue);
                                        }
                                    }

                                    oFIns.FKiQSSectionDefinitionID = sec.Value.FKiStepSectionDefinitionID;//oSecIns.ID;
                                    oFIns.FKiQSInstanceID = oQSIns.ID;
                                    oFIns.FKiFieldDefinitionID = items.Value.FKiFieldDefinitionID;
                                    oFIns.FKiQSStepDefinitionID = oStep.FKiQSStepDefinitionID;// oStep.ID;
                                    oFIns.FKiStepInstanceID = oStep.ID;
                                    oFIns.FKiSectionInstanceID = oSecIns.ID;
                                    oFIns.FKiFieldOriginID = FieldOrigin.ID;
                                    oFIns.iStage = StepDef.iStage;
                                    oFIns.bIsDisplay = FieldOrigin.bIsDisplay;
                                    oFIns.bIsModify = FieldOrigin.bIsModify;
                                    oFIns.dValue = DateTime.Now;
                                    //oFIns = Connection.Insert<XIIValue>(oFIns, "XIFieldInstance_T", "ID");
                                    oBulkBO.Add(oxiibo);
                                } //end for INSERT
                                items.Value.ID = oFIns.ID;
                                items.Value.FKiQSStepDefinitionID = oStep.FKiQSStepDefinitionID;
                                items.Value.FKiQSSectionDefinitionID = sec.Value.FKiStepSectionDefinitionID;

                            } // end for foreach

                            // Execute bulk Query Here Create Data Table and Load Defination 

                            /* TO DO NEED TO UNCOMMENT THE CODE AND NEED TO TEST FOR SQL BULK COPY INSERTION*/

                        }
                        else
                        {
                        }
                    }
                    if (oBulkBO != null && oBulkBO.Count() > 0)
                    {
                        var BoD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIFieldInstance_T", null);
                        oBulkBO.ForEach(f => f.BOD = BoD);
                        var MakeDatatble = BulkInsert.MakeBulkSqlTable(oBulkBO);
                        BulkInsert.SaveBulk(MakeDatatble, BoD.iDataSource, "XIFieldInstance_T");
                        foreach (var obo in oBulkBO)
                        {
                            Dictionary<string, object> fieldParams = new Dictionary<string, object>();
                            fieldParams["FKiQSInstanceID"] = oQSIns.ID;
                            //fieldParams["FKiQSSectionDefinitionID"] = sec.Value.FKiStepSectionDefinitionID;
                            fieldParams["FKiFieldOriginID"] = obo.Attributes["FKiFieldOriginID"].sValue;
                            var oFIns = Connection.Select<XIIValue>("XIFieldInstance_T", fieldParams).FirstOrDefault();
                            if (oFIns != null)
                            {
                                var StepDef = oQSInstance.QSDefinition.Steps.Values.Where(m => m.ID == oStep.FKiQSStepDefinitionID).FirstOrDefault();
                                var SecDef = StepDef.Sections.Values.Where(m => m.ID == oFIns.FKiQSSectionDefinitionID).FirstOrDefault();
                                var FieldOrigin = SecDef.FieldDefs.Values.Where(m => m.ID == oFIns.FKiFieldDefinitionID).FirstOrDefault().FieldOrigin;
                                if (FieldOrigin.bIsEncrypt)
                                {
                                    oFIns.sValue = oEncrypt.EncryptData(oFIns.sValue, true, oFIns.ID.ToString());
                                    oFIns.sDerivedValue = oFIns.sValue;
                                    oFIns = Connection.Update<XIIValue>(oFIns, "XIFieldInstance_T", "ID");
                                }
                            }
                        }
                    }
                }
                else
                {
                    if (oStep.XIValues != null)
                    {
                        foreach (var items in oStep.XIValues.Values)
                        {
                            var StepDef = oQSInstance.QSDefinition.Steps.Values.Where(m => m.ID == oStep.FKiQSStepDefinitionID).FirstOrDefault();
                            var FieldOrigin = StepDef.FieldDefs.Values.Where(m => m.ID == items.FKiFieldDefinitionID).FirstOrDefault().FieldOrigin;
                            XIIValue oFIns = new XIIValue();
                            if (FieldOrigin.bIsOptionList)
                            {
                                oFIns.sDerivedValue = FieldOrigin.FieldOptionList.Where(m => m.sOptionValue == items.sValue).Select(m => m.sOptionName).FirstOrDefault();
                            }
                            if (FieldOrigin.DataType.sBaseDataType.ToLower() == "int")
                            {
                                oFIns.iValue = Convert.ToInt32(items.sValue);
                            }
                            else if (FieldOrigin.DataType.sBaseDataType.ToLower() == "datetime")
                            {
                                //if (items.sValue != null)
                                //{
                                //    oFIns.dValue = Convert.ToDateTime(items.sValue);
                                //}
                            }
                            else if (FieldOrigin.DataType.sBaseDataType.ToLower() == "decimal")
                            {
                                oFIns.rValue = Convert.ToDecimal(items.sValue);
                            }
                            oFIns.sValue = items.sValue;
                            oFIns.FKiQSStepDefinitionID = oStep.FKiQSStepDefinitionID;// oQSStepIns.ID;
                            oFIns.FKiQSInstanceID = oQSIns.ID;
                            oFIns.FKiFieldDefinitionID = items.FKiFieldDefinitionID;
                            oFIns.FKiStepInstanceID = oStep.ID;
                            oFIns.iStage = StepDef.iStage;
                            oFIns.bIsDisplay = FieldOrigin.bIsDisplay;
                            oFIns.bIsModify = FieldOrigin.bIsModify;
                            oFIns.dValue = DateTime.Now;
                            oFIns = Connection.Insert<XIIValue>(oFIns, "XIFieldInstance_T", "ID");

                            //dbContext.XIFieldInstance.Add(oFIns);
                            //dbContext.SaveChanges();
                            items.ID = oFIns.ID;
                            items.FKiQSStepDefinitionID = oStep.FKiQSStepDefinitionID;// oStep.ID;
                        }
                    }
                    else
                    {

                    }
                }
                if (oStep.FieldOriginIDs != null && oStep.FieldOriginIDs.Count() > 0)
                {
                    foreach (var FieldOriginID in oStep.FieldOriginIDs)
                    {
                        Dictionary<string, object> fieldParams = new Dictionary<string, object>();
                        fieldParams["FKiQSInstanceID"] = oQSIns.ID;
                        fieldParams["FKiFieldOriginID"] = FieldOriginID;
                        var oFieldInstance = Connection.Select<XIIValue>("XIFieldInstance_T", fieldParams).FirstOrDefault();
                        oFieldInstance.XIDeleted = 1;
                        oFieldInstance = Connection.Update<XIIValue>(oFieldInstance, "XIFieldInstance_T", "ID");
                    }
                }
            }
            return oQSInstance;
        }

        #region QSNotations
        public string GetQuestionSetParamValue(XIIQS oQSInstance, string sQSNotation)
        {
            string sReplacetext = sQSNotation.Replace("{{", "").Replace("}}", "");
            string CommText = ""; string sReturnValue = "";
            List<string[]> Rows = new List<string[]>();
            string sValue = "";
            string sSelectType = ""; int iStepID = 0;
            int FieldID = 0;
            IDictionary<string, string> DictionaryList = new Dictionary<string, string>();
            if (!string.IsNullOrEmpty(sReplacetext))
            {
                if (!sReplacetext.Contains("."))
                {
                    sSelectType = Convert.ToString(oQSInstance.ID);
                    DictionaryList.Add("FKiQSInstanceID", sSelectType);
                    sReplacetext = "XIField(" + sReplacetext + ").sValue";
                }
                string[] sQSNotations = sReplacetext.Split('.');
                if (sQSNotations != null)
                {
                    foreach (var str in sQSNotations)
                    {
                        string[] sType = str.Split('(');
                        if (sType.Count() == 2)
                        {
                            if (sType[0] == "QS")
                            {
                                sType[1] = sType[1].TrimEnd(')');
                                if (sType[1] == "iMyQSID")
                                {
                                    sSelectType = Convert.ToString(oQSInstance.ID);
                                    DictionaryList.Add("FKiQSInstanceID", sSelectType);
                                }
                            }
                            if (sType[0] == "StepName")
                            {
                                sType[1] = sType[1].TrimEnd(')');
                                sType[1] = sType[1].Trim('\'');
                                iStepID = oQSInstance.QSDefinition.Steps.Where(x => x.Value.sName == sType[1]).Select(x => x.Value.ID).FirstOrDefault();
                                sSelectType = Convert.ToString(iStepID);
                                DictionaryList.Add("FKiQSStepDefinitionID", sSelectType);
                            }
                            if (sType[0] == "XIField")
                            {
                                sType[1] = sType[1].TrimEnd(')');
                                sType[1] = sType[1].Trim('\'');
                                foreach (var sec in oQSInstance.QSDefinition.Steps)
                                {
                                    FieldID = sec.Value.FieldDefs.Where(m => m.Value.FieldOrigin.sName == sType[1]).Select(m => m.Value.ID).FirstOrDefault();
                                    if (FieldID > 0)
                                    {
                                        sSelectType = Convert.ToString(FieldID);
                                        DictionaryList.Add("FKiFieldDefinitionID", sSelectType);
                                        break;
                                    }
                                }
                            }
                        }
                        else
                        {
                            sSelectType = sType[0];
                            DictionaryList.Add("sSelectValue", sSelectType);
                            CommText = "select " + sType[0] + " from XIFieldInstance_T where 1=1";
                        }

                    }
                    string s = sSelectType;
                }
            }

            if (DictionaryList.ContainsKey("FKiFieldDefinitionID"))
            {
                sValue = GetQuestionSetParamWithValues(oQSInstance, DictionaryList);
                //Get Field Options list Values  if field have Options
                string OptionsValue = FieldHasOptions(FieldID, sValue);
                if (!string.IsNullOrEmpty(OptionsValue))
                {
                    sValue = OptionsValue;
                }
            }
            return sValue;
        }

        public string GetQuestionSetParamWithValues(XIIQS oQSInstance, IDictionary<string, string> DictionaryList)
        {
            XIDXI oDXI = new XIDXI();

            oDXI.sOrgDatabase = sOrgDatabase;
            string str = ""; string sSelectString = ""; string sReturnValue = ""; string sReturnType = "";
            var sBODataSource = string.Empty;
            foreach (var item in DictionaryList)
            {
                if (item.Key != "sSelectValue")
                {
                    str += " and " + item.Key + "=" + item.Value + "";
                }
                else
                {
                    sSelectString = "select CONVERT(varchar," + item.Value + "),SQL_VARIANT_PROPERTY(" + item.Value + ",'BaseType') AS 'Base Type' from XIFieldInstance_T where 1=1";
                }

            }
            using (SqlConnection Con = new SqlConnection(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "";
                cmd.Connection = Con;
                Con.Open();
                cmd.CommandText = str;
                cmd.CommandText = cmd.CommandText.Insert(0, sSelectString);
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    //sReturnValue = reader.GetString(0);
                    //sReturnType = reader.GetString(1);
                    sReturnValue = reader.IsDBNull(0) ? "" : reader.GetValue(0).ToString();
                    sReturnType = reader.IsDBNull(1) ? null : reader.GetValue(1).ToString();
                }
                Con.Dispose();
                if (sReturnType == "datetime")
                {
                    DateTime dtvalue = Convert.ToDateTime(sReturnValue);
                    sReturnValue = dtvalue.ToString("MM/dd/yyyy");
                }
                return sReturnValue;
            }
        }
        public string FieldHasOptions(int FieldID, string sValue)
        {
            string Value = string.Empty;
            string Query = "SELECT  sOptionName FROM XIFieldOptionList_T WHERE FKiQSFieldID = (SELECT FKiXIFieldOriginID  FROM XIFieldDefinition_T WHERE id =" + FieldID + " ) AND sOptionValue ='" + sValue + "'";
            using (SqlConnection Con = new SqlConnection(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString))
            {
                SqlCommand cmd = new SqlCommand();
                cmd.CommandText = "";
                cmd.Connection = Con;
                Con.Open();
                cmd.CommandText = Query;
                SqlDataReader reader = cmd.ExecuteReader();
                if (reader.Read())
                {
                    Value = reader["sOptionName"].ToString();
                }
                Con.Dispose();
            }
            return Value;
        }
        #endregion
        //public XIIQS QSI(string sInstanceID, XIDStructure oStructure)
        //{


        //    int iInstanceID = 0; XIIQS oQsInstance = new XIIQS();
        //    XIDStructure oStruture = new XIDStructure();
        //    if (!string.IsNullOrEmpty(sInstanceID))
        //    {
        //        iInstanceID = Convert.ToInt32(sInstanceID);
        //        if (iInstanceID > 0)
        //        {
        //            XIIXI oXII = new XIIXI();
        //            var oQSD = oXII.GetQSInstanceByID(iInstanceID);
        //            oQsInstance = oXII.GetQuestionSetInstanceByID(oQSD.FKiQSDefinitionID, iInstanceID, null, 0, 0, null);
        //            if (oQsInstance != null)
        //            {
        //                Dictionary<string, XIIValue> oXIIValues = (Dictionary<string, XIIValue>)oStruture.GetQSNamevaluepairs(oQsInstance);
        //                oQsInstance.XIValues = oXIIValues;
        //            }

        //        }
        //    }
        //    if (oStructure != null)
        //    {
        //        var oStructureInstance = (Dictionary<string, List<XIIBO>>)oStructure.oParent;
        //        oQsInstance.oStructureInstance = oStructureInstance;

        //    }
        //    return oQsInstance;
        //}
        public XIIQS QSI(XIBOInstance oStructure)
        {
            XIIQS oQsInstance = new XIIQS();
            try
            {
                string sInstanceID = oStructure.BOI.AttributeI("id").sValue;
                int iInstanceID = 0;
                XIDStructure oStruture = new XIDStructure();
                if (!string.IsNullOrEmpty(sInstanceID))
                {
                    iInstanceID = Convert.ToInt32(sInstanceID);
                    if (iInstanceID > 0)
                    {
                        XIIXI oXII = new XIIXI();
                        var oQSD = oXII.GetQSInstanceByID(iInstanceID);
                        if (oQSD != null)
                        {
                            oQsInstance = oXII.GetQuestionSetInstanceByID(oQSD.FKiQSDefinitionID, iInstanceID, null, 0, 0, null);
                        }
                        if (oQsInstance != null)
                        {
                            var oQSNVPairs = oStruture.GetQSNamevaluepairs(oQsInstance);
                            if (oQSNVPairs.xiStatus == 0 && oQSNVPairs.oResult != null)
                            {
                                Dictionary<string, XIIValue> oXIIValues = (Dictionary<string, XIIValue>)oQSNVPairs.oResult;
                                oQsInstance.XIValues = oXIIValues;
                            }
                            oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                        }
                    }
                }
                if (oStructure != null)
                {
                    var oStructureInstance = (Dictionary<string, List<XIIBO>>)oStructure.oParent;
                    oQsInstance.oStructureInstance = oStructureInstance;
                }
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                SaveErrortoDB(oCResult);
            }
            return oQsInstance;
        }
        public string XIIValues(string sName)
        {
            var oQsInstance = this; string sReturnValue = "";
            //XIDStructure oStruture = new XIDStructure();
            if (oQsInstance != null)
            {
                Dictionary<string, XIIValue> oXIIValues = new Dictionary<string, XIIValue>(StringComparer.CurrentCultureIgnoreCase);
                oXIIValues = this.XIValues;
                if (oXIIValues.ContainsKey(sName))
                {
                    sReturnValue = oXIIValues[sName].sValue;
                }
            }
            return sReturnValue;
        }
        public XIIValue GetXIIValue(string sName)
        {
            XIIValue xivalue = new XIIValue();
            var oQsInstance = this;
            if (oQsInstance != null)
            {
                Dictionary<string, XIIValue> oXIIValues = new Dictionary<string, XIIValue>(StringComparer.CurrentCultureIgnoreCase);
                oXIIValues = this.XIValues;
                if (oXIIValues.ContainsKey(sName))
                {
                    xivalue = oXIIValues[sName];
                }
            }
            return xivalue;
        }
        //public Dictionary<string, string> GetNotationValue(Dictionary<string, XIIValue> oXIValues, Dictionary<string, List<XIIBO>> oSubStructuresList, Dictionary<string, string> sNotationList)
        //{
        //    string sReturnValue = "";
        //    Dictionary<string, string> sNotationsResults = sNotationList;
        //    XIIQS oXIQS = new XIIQS();
        //    // Dictionary<string, XIIValue> oXIIValues = (Dictionary<string, XIIValue>)XILoad;
        //    foreach (var sNotation in sNotationList.ToList())
        //    {
        //        if (string.IsNullOrEmpty(sNotation.Value))
        //        {
        //            string sString = sNotation.Key.Replace("{{", "").Replace("}}", "").Replace("\"", "");
        //            if (sString.Contains(","))
        //            {
        //                string sKey = sString.Split(',')[0];
        //                string sAttributeName = sString.Split(',')[1];
        //                if (sKey == "QS Instance")
        //                {
        //                    if (oXIValues.ContainsKey(sAttributeName))
        //                    {
        //                        sReturnValue = oXIValues[sAttributeName].sValue;
        //                        string OptionsValue = oXIQS.FieldHasOptions(oXIValues[sAttributeName].FKiFieldDefinitionID, sReturnValue);
        //                        if (!string.IsNullOrEmpty(OptionsValue))
        //                        {
        //                            sReturnValue = OptionsValue;
        //                        }
        //                        sNotationsResults[sNotation.Key] = sReturnValue;
        //                    }
        //                    else
        //                    {
        //                        if (sAttributeName.Contains("Collection"))
        //                        {
        //                            string sKeyname = sAttributeName.Split(' ')[0];
        //                            var oSubstructureInstance = oSubStructuresList.Values.FirstOrDefault().Select(x => x.SubChildI).FirstOrDefault();
        //                            if (oSubstructureInstance.ContainsKey(sKeyname))
        //                            {
        //                                sReturnValue = ReplaceHtmlContentWithData(null, null, oSubstructureInstance[sKeyname]);
        //                                sNotationsResults[sNotation.Key] = sReturnValue;
        //                            }
        //                            else
        //                            {
        //                                XIIQS oXIIQS = new XIIQS();
        //                                oXIIQS.XIValues = oXIValues;
        //                                oXIIQS.oStructureInstance = oSubstructureInstance;
        //                                GetNotationValue(oXIValues, oSubStructuresList, sNotationList);
        //                            }
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    if (sAttributeName.Contains("Collection"))
        //                    {
        //                        string sKeyname = sAttributeName.Split(' ')[0];
        //                        var oSubstructureInstance = oXIQS.oStructureInstance.Values.FirstOrDefault().Select(x => x.SubChildI).FirstOrDefault();
        //                        if (oSubstructureInstance.ContainsKey(sKeyname))
        //                        {
        //                            sReturnValue = ReplaceHtmlContentWithData(null, null, oSubstructureInstance[sKeyname]);
        //                            sNotationsResults[sNotation.Key] = sReturnValue;
        //                        }
        //                        else
        //                        {
        //                            XIIQS oXIIQS = new XIIQS();
        //                            oXIIQS.XIValues = oXIValues;
        //                            oXIIQS.oStructureInstance = oSubstructureInstance;
        //                            GetNotationValue(oXIValues, oSubStructuresList, sNotationList);
        //                        }
        //                    }
        //                    else
        //                    {
        //                        if (oXIValues.ContainsKey(sAttributeName))
        //                        {
        //                            sReturnValue = oXIValues[sAttributeName].sValue;
        //                            string OptionsValue = oXIQS.FieldHasOptions(oXIValues[sAttributeName].FKiFieldDefinitionID, sReturnValue);
        //                            if (!string.IsNullOrEmpty(OptionsValue))
        //                            {
        //                                sReturnValue = OptionsValue;
        //                            }
        //                            sNotationsResults[sNotation.Key] = sReturnValue;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //    //foreach (var sNotation in sNotationList.ToList())
        //    //{
        //    //    if (string.IsNullOrEmpty(sNotation.Value))
        //    //    {
        //    //        string sString = sNotation.Key.Replace("{{", "").Replace("}}", "");
        //    //        if (sString.Contains(","))
        //    //        {
        //    //            string sKey = sString.Split(',')[0];
        //    //            string sAttributeName = sString.Split(',')[1];
        //    //            //if(sAttributeName.Contains("("))
        //    //            //{ }
        //    //            if (XILoad.ContainsKey(sKey))
        //    //            {
        //    //                var oAttributes = XILoad.Values.FirstOrDefault().Select(x => x.Attributes).FirstOrDefault();
        //    //                if (oAttributes.ContainsKey(sAttributeName))
        //    //                {
        //    //                    sReturnValue = oAttributes.Where(x => x.Key.ToLower() == sAttributeName.ToLower()).Select(x => x.Value.sValue).FirstOrDefault();
        //    //                    sNotationsResults[sNotation.Key] = sReturnValue;
        //    //                }
        //    //            }
        //    //            else
        //    //            {
        //    //                if(XILoad.Count()>0)
        //    //                {
        //    //                    Dictionary<string, List<XIIBO>> sSubstructureInstance = XILoad.Values.FirstOrDefault().Select(x => x.Class).FirstOrDefault();
        //    //                    if (sSubstructureInstance.ContainsKey(sKey))
        //    //                    {
        //    //                        var oAttributes = sSubstructureInstance.Values.FirstOrDefault().Select(x => x.Attributes).FirstOrDefault();
        //    //                        if (oAttributes.ContainsKey(sAttributeName))
        //    //                        {
        //    //                            sReturnValue = oAttributes.Where(x => x.Key.ToLower() == sAttributeName.ToLower()).Select(x => x.Value.sValue).FirstOrDefault();
        //    //                            sNotationsResults[sNotation.Key] = sReturnValue;
        //    //                        }
        //    //                    }
        //    //                    else
        //    //                    {
        //    //                        NotationValue(sSubstructureInstance, sNotationsResults);
        //    //                    }
        //    //                }
        //    //            }
        //    //        }
        //    //    }
        //    //}
        //    return sNotationsResults;
        //}
        //public string ReplaceHtmlContentWithData(string sHtmlContent, string sReplaceString, List<XIIBO> oResult)
        //{
        //        //here we get html content string with data
        //        int i = 0;
        //        string sFinalHtmlContent = "<table style='width:100%;' cellpadding='4' cellspacing='0' border='1'>";
        //        foreach (var item in oResult)
        //        {
        //            if (i == 0)
        //            {
        //                sFinalHtmlContent += "<tr>";
        //                // sFinalHtmlContent += "<thead>";
        //                foreach (var attribute in item.Attributes)
        //                {

        //                    sFinalHtmlContent += "<th style='background-color:#e7e7e7;'>" + attribute.Key + "</th>";
        //                }
        //                sFinalHtmlContent += "</tr>";
        //            }

        //            sFinalHtmlContent += "<tr>";
        //            foreach (var attribute in item.Attributes)
        //            {
        //                if (string.IsNullOrEmpty(attribute.Value.sValue))
        //                {
        //                    sFinalHtmlContent += "<td style='background-color:#c0c0c0;'>" + "-" + "</td>";
        //                }
        //                else
        //                {
        //                    if (attribute.Key == "DOB")
        //                    {
        //                        sFinalHtmlContent += "<td style='background-color:#c0c0c0;'>" + Convert.ToDateTime(attribute.Value.sValue).Date.ToString("dd-MMM-yyyy") + "</td>";
        //                    }
        //                    else
        //                    {
        //                        sFinalHtmlContent += "<td style='background-color:#c0c0c0;'>" + attribute.Value.sValue + "</td>";
        //                    }
        //                }
        //            }
        //            sFinalHtmlContent += "</tr>";
        //            i++;
        //        }
        //        sFinalHtmlContent += "</table>";
        //        //sHtmlContent = sHtmlContent.Replace(sReplaceString, sFinalHtmlContent);
        //        return sFinalHtmlContent;
        //    }
        public XIIQS LoadStepInstance(XIIQS oQSInstance, int StepID = 0, string sGUID = "")
        {
            XIInfraCache xiCache = new XIInfraCache();
            var Step = new XIDQSStep();

            var sSessionID = string.Empty;
            if (HttpContext.Current == null)
            {

            }
            else
            {
                sSessionID = HttpContext.Current.Session.SessionID;
            }
            List<int> FieldOriginIDs = new List<int>();
            XIDQS oQSD = (XIDQS)xiCache.GetObjectFromCache(XIConstant.CacheQuestionSet, null, oQSInstance.QSDefinition.ID.ToString());
            XIDQS oQSDC = (XIDQS)oQSD.GetCopy();
            oQSInstance.QSDefinition = oQSDC;
            if (oQSInstance.QSDefinition.Steps.Count() > 0)
            {
                XIIQSStep oStepInstance = new XIIQSStep();
                if (StepID == 0)
                {
                    Step = oQSInstance.QSDefinition.Steps.Values.Where(m => m.sIsHidden == "off" || m.sIsHidden == null).OrderBy(m => m.iOrder).FirstOrDefault();
                }
                else
                {
                    Step = oQSInstance.QSDefinition.Steps.Values.Where(m => m.ID == StepID).OrderBy(m => m.iOrder).FirstOrDefault();
                }
                if (oQSInstance.Steps != null)
                {
                    oStepInstance = oQSInstance.Steps.Where(m => m.Value.FKiQSStepDefinitionID == Step.ID).FirstOrDefault().Value;
                }
                if (oStepInstance == null)
                {
                    oStepInstance = new XIIQSStep();
                }
                oStepInstance.iOverrideStep = Step.iOverrideStep;
                //Loading Fields for Step
                Dictionary<string, XIIValue> nFieldValues = new Dictionary<string, XIIValue>();
                if (((xiSectionContent)Step.iDisplayAs).ToString() == xiSectionContent.Fields.ToString())
                {
                    if (Step.FieldDefs != null)
                    {
                        var Defs = Step.FieldDefs.ToList();//.Select(m => new XIIValue { FKiFieldDefinitionID = m.ID }).ToList();
                        foreach (var def in Defs)
                        {
                            nFieldValues[def.Value.FieldOrigin.sName] = new XIIValue { FKiFieldDefinitionID = Convert.ToInt32(def.Value.ID) };
                        }
                        oStepInstance.XIValues = nFieldValues;
                    }
                }
                //Load Step Layout Content
                else if (Step.iLayoutID > 0)
                {
                    XIDLayout oLayoutD = new XIDLayout();
                    oLayoutD.ID = Step.iLayoutID;
                    oStepInstance.oDefintion = Step;
                    var oStepContent = oStepInstance.Load();
                    if (oStepContent.bOK && oStepContent.oResult != null)
                    {
                        var Instance = ((XIIQSStep)(((XIInstanceBase)oStepContent.oResult).oContent[XIConstant.ContentStep])).oContent[XIConstant.ContentLayout];
                        oStepInstance.oContent[XIConstant.ContentLayout] = Instance;//((XIDQSStep)((XIInstanceBase)oStepContent.oResult)).oContent[XIConstant.ContentStep];

                    }
                }
                //Loading Sections for Step
                else if (((xiSectionContent)Step.iDisplayAs).ToString() == xiSectionContent.Sections.ToString())
                {
                    Dictionary<string, XIIQSSection> nSecIns = new Dictionary<string, XIIQSSection>();
                    if (Step.Sections != null && Step.Sections.Count() > 0)
                    {
                        foreach (var sec in Step.Sections.Values.OrderBy(m => m.iOrder))
                        {
                            XIIQSSection oSecIns = null;
                            if (oStepInstance.Sections != null && oStepInstance.Sections.Count() > 0)
                            {
                                oSecIns = oStepInstance.Sections[sec.ID.ToString() + "_Sec"];
                            }
                            if (oSecIns == null)
                            {
                                oSecIns = new XIIQSSection();
                                oSecIns.FKiStepSectionDefinitionID = sec.ID;
                                if (((xiSectionContent)sec.iDisplayAs).ToString() == xiSectionContent.Fields.ToString())
                                {
                                    Dictionary<string, XIIValue> nSecFieldValues = new Dictionary<string, XIIValue>();
                                    if (sec.FieldDefs != null && sec.FieldDefs.Count() > 0)
                                    {
                                        var Def = sec.FieldDefs.Values.OrderBy(m => m.ID).ToList();//.Select(m => new XIIValue { FKiFieldDefinitionID = m.ID }).ToList();
                                        foreach (var def in Def)
                                        {
                                            if ((def.FieldOrigin.bIsMerge && !String.IsNullOrEmpty(def.FieldOrigin.sMergeField) && (!oQSInstance.XIValues.ContainsKey(def.FieldOrigin.sName) || (oQSInstance.XIValues.ContainsKey(def.FieldOrigin.sName) && def.FieldOrigin.bIsReload))))
                                            {
                                                if (def.FieldOrigin.sMergeField.StartsWith("{XIP"))
                                                {
                                                    XIInfraCache oCache = new XIInfraCache();
                                                    XICacheInstance oGUIDParams = oCache.GetAllParamsUnderGUID(sSessionID, sGUID, null);
                                                    nSecFieldValues[def.FieldOrigin.sName] = new XIIValue { FKiFieldDefinitionID = Convert.ToInt32(def.ID), sValue = oGUIDParams.NMyInstance.Where(x => x.Key == def.FieldOrigin.sMergeField).Select(t => t.Value.sValue).FirstOrDefault(), FKiFieldOriginID = def.FKiXIFieldOriginID, bIsDisplay = def.FieldOrigin.bIsDisplay };
                                                }
                                                else
                                                {
                                                    nSecFieldValues[def.FieldOrigin.sName] = new XIIValue { FKiFieldDefinitionID = Convert.ToInt32(def.ID), sValue = oQSInstance.XIIValues(def.FieldOrigin.sMergeField), FKiFieldOriginID = def.FKiXIFieldOriginID, bIsDisplay = def.FieldOrigin.bIsDisplay };
                                                }
                                            }
                                            else if (def.FieldOrigin.bIsMerge && !String.IsNullOrEmpty(def.FieldOrigin.sMergeBo) && !String.IsNullOrEmpty(def.FieldOrigin.sMergeBoField) && !string.IsNullOrEmpty(def.FieldOrigin.sMergeVariable) && (!oQSInstance.XIValues.ContainsKey(def.FieldOrigin.sName) || (oQSInstance.XIValues.ContainsKey(def.FieldOrigin.sName) && def.FieldOrigin.bIsReload)))
                                            {
                                                XIInfraCache oCache = new XIInfraCache();
                                                XIDBO oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, def.FieldOrigin.sMergeBo, null);
                                                string sMergeCacheField = def.FieldOrigin.sMergeVariable;
                                                var InstanceID = (string)oCache.Get_ParamVal(sSessionID, sGUID, null, sMergeCacheField);
                                                int iInstanceiD = 0;
                                                if (int.TryParse(InstanceID, out iInstanceiD))
                                                {
                                                    XIIXI oXIIXI = new XIIXI();
                                                    if (iInstanceiD != 0)
                                                    {
                                                        var oBOI = oXIIXI.BOI(oBOD.Name, iInstanceiD.ToString());
                                                        if (oBOI.Attributes.ContainsKey(def.FieldOrigin.sMergeBoField))
                                                        {
                                                            if (oBOD.Name == "EnumReconciliations_T")
                                                            {
                                                                if (oBOI.Attributes.ContainsKey(def.FieldOrigin.sMergeBoField))
                                                                {
                                                                    nSecFieldValues[def.FieldOrigin.sName] = new XIIValue { FKiFieldDefinitionID = Convert.ToInt32(def.ID), sValue = oBOI.Attributes[def.FieldOrigin.sMergeBoField].sValue + " - " + DateTime.Now.ToString(XIConstant.DateTimeFull_Format), FKiFieldOriginID = def.FKiXIFieldOriginID, bIsDisplay = def.FieldOrigin.bIsDisplay };
                                                                }
                                                            }
                                                            else
                                                            {
                                                                var sBOValue = oBOI.Attributes[def.FieldOrigin.sMergeBoField].sValue;
                                                                if (def.FieldOrigin.DataType != null && def.FieldOrigin.DataType.sBaseDataType.ToLower() == "datetime")
                                                                {
                                                                    sBOValue = Utility.GetDefaultDateResolvedValue(sBOValue, XIConstant.Date_Format);
                                                                }
                                                                nSecFieldValues[def.FieldOrigin.sName] = new XIIValue { FKiFieldDefinitionID = Convert.ToInt32(def.ID), sValue = sBOValue, FKiFieldOriginID = def.FKiXIFieldOriginID, bIsDisplay = def.FieldOrigin.bIsDisplay };
                                                            }
                                                        }
                                                        else
                                                        {
                                                            List<XIWhereParams> oWParams = new List<XIWhereParams>();
                                                            string[] operators = new string[] { "+", "-", "/", "%", "(", ")" };
                                                            List<string[]> lstItems = def.FieldOrigin.sMergeBoField.Split(',').Select(f => new { Field = f }).Select(c => c.Field.Trim().Split(operators, StringSplitOptions.RemoveEmptyEntries)).ToList();
                                                            for (int i = 0; i < lstItems.Count; i++)
                                                            {
                                                                if (lstItems[i].Count() > 1)
                                                                {
                                                                    Regex pattern = new Regex(Regex.Escape(lstItems[i][0]) + "(.+?)" + Regex.Escape(lstItems[i][1]));
                                                                    var getoperators = pattern.Matches(def.FieldOrigin.sMergeBoField).Cast<Match>().Select(s => s.Groups[1].Value).Distinct().ToList();
                                                                    if (getoperators.Count() > 0)
                                                                    {
                                                                        nSecFieldValues[def.FieldOrigin.sName] = new XIIValue
                                                                        {
                                                                            FKiFieldDefinitionID = Convert.ToInt32(def.ID),
                                                                            sValue = (oBOI.Attributes[lstItems[i][0]].doValue - oBOI.Attributes[lstItems[i][1]].doValue).ToString(),
                                                                            FKiFieldOriginID = def.FKiXIFieldOriginID,
                                                                            bIsDisplay = def.FieldOrigin.bIsDisplay
                                                                        };
                                                                    }
                                                                }
                                                                else
                                                                {
                                                                    nSecFieldValues[def.FieldOrigin.sName] = new XIIValue { FKiFieldDefinitionID = Convert.ToInt32(def.ID), sValue = "", FKiFieldOriginID = def.FKiXIFieldOriginID, bIsDisplay = def.FieldOrigin.bIsDisplay };
                                                                }
                                                            }
                                                        }
                                                    }
                                                    else
                                                    {
                                                        nSecFieldValues[def.FieldOrigin.sName] = new XIIValue { FKiFieldDefinitionID = Convert.ToInt32(def.ID), sValue = "", FKiFieldOriginID = def.FKiXIFieldOriginID, bIsDisplay = def.FieldOrigin.bIsDisplay };
                                                    }
                                                }
                                                else
                                                {
                                                    nSecFieldValues[def.FieldOrigin.sName] = new XIIValue { FKiFieldDefinitionID = Convert.ToInt32(def.ID), sValue = "", FKiFieldOriginID = def.FKiXIFieldOriginID, bIsDisplay = def.FieldOrigin.bIsDisplay };
                                                }
                                            }
                                            else
                                            {
                                                if (!string.IsNullOrEmpty(def.FieldOrigin.sDefaultDate))
                                                {
                                                    nSecFieldValues[def.FieldOrigin.sName] = new XIIValue { FKiFieldDefinitionID = Convert.ToInt32(def.ID), sValue = Utility.GetDefaultDateResolvedValue(def.FieldOrigin.sDefaultDate, XIConstant.Date_Format), FKiFieldOriginID = def.FKiXIFieldOriginID, bIsDisplay = def.FieldOrigin.bIsDisplay };
                                                }
                                                else
                                                {
                                                    if (oQSInstance.XIValues.ContainsKey(def.FieldOrigin.sName.ToString()) && Step.bIsMerge && !def.FieldOrigin.bIsReload)
                                                    {
                                                        if (def.FKiXIFieldOriginID != oQSInstance.XIValues[def.FieldOrigin.sName.ToString()].FKiFieldOriginID)
                                                        {
                                                            FieldOriginIDs.Add(oQSInstance.XIValues[def.FieldOrigin.sName.ToString()].FKiFieldOriginID);
                                                        }
                                                        nSecFieldValues[def.FieldOrigin.sName] = new XIIValue { FKiFieldDefinitionID = Convert.ToInt32(def.ID), sValue = oQSInstance.XIValues[def.FieldOrigin.sName.ToString()].sValue, FKiFieldOriginID = def.FKiXIFieldOriginID, bIsDisplay = def.FieldOrigin.bIsDisplay };
                                                    }
                                                    else
                                                    {
                                                        nSecFieldValues[def.FieldOrigin.sName] = new XIIValue { FKiFieldDefinitionID = Convert.ToInt32(def.ID), FKiFieldOriginID = def.FKiXIFieldOriginID, bIsDisplay = def.FieldOrigin.bIsDisplay };
                                                    }
                                                }
                                            }
                                            if (!string.IsNullOrEmpty(def.FieldOrigin.sMinDate) && (def.FieldOrigin.sMinDate.Contains("xi.s")))
                                            {
                                                XIDScript oXIScript = new XIDScript();
                                                oXIScript.sScript = def.FieldOrigin.sMinDate.ToString();
                                                var oCResult = oXIScript.Execute_Script(sGUID, sSessionID);
                                                if (oCResult.bOK && oCResult.oResult != null)
                                                {
                                                    var sVal = (string)oCResult.oResult;
                                                    sVal = Utility.GetDateResolvedValue(sVal, "yyyy-MM-dd");
                                                    def.FieldOrigin.sMinDate = sVal;
                                                }
                                            }
                                            if (!string.IsNullOrEmpty(def.FieldOrigin.sMaxDate) && (def.FieldOrigin.sMaxDate.Contains("xi.s")))
                                            {
                                                XIDScript oXIScript = new XIDScript();
                                                oXIScript.sScript = def.FieldOrigin.sMaxDate.ToString();
                                                var oCResult = oXIScript.Execute_Script(sGUID, sSessionID);
                                                if (oCResult.bOK && oCResult.oResult != null)
                                                {
                                                    var sVal = (string)oCResult.oResult;
                                                    def.FieldOrigin.sMaxDate = Utility.GetDateResolvedValue(sVal, "yyyy-MM-dd");
                                                    CResult oSResult = new CResult();
                                                    oSResult.sMessage = def.FieldOrigin.sName + "_" + def.FieldOrigin.ID + " : Script Value:" + sVal + " & Resolved Value:" + def.FieldOrigin.sMaxDate;
                                                    SaveErrortoDB(oSResult, oQSInstance.ID);
                                                }
                                            }
                                            if (!string.IsNullOrEmpty(def.FieldOrigin.sScript))
                                            {
                                                string sVal = string.Empty;
                                                if (nSecFieldValues.ContainsKey(def.FieldOrigin.sName))
                                                {
                                                    sVal = nSecFieldValues[def.FieldOrigin.sName].sValue;
                                                }
                                                XIDScript oScriptD = new XIDScript();
                                                oScriptD.sScript = def.FieldOrigin.sScript;
                                                if (oScriptD != null)
                                                {
                                                    var oRes = oScriptD.Execute_Script(sGUID, sSessionID);
                                                    if (oRes.bOK && oRes.oResult != null)
                                                    {
                                                        string result = (string)oRes.oResult;
                                                        if (def.FieldOrigin.iScriptType == 50)
                                                        {
                                                            if (!string.IsNullOrEmpty(result) && result.ToLower() == "true")
                                                            {
                                                                def.FieldOrigin.bIsDisable = true;
                                                            }
                                                            else
                                                            {
                                                                def.FieldOrigin.bIsDisable = false;
                                                                nSecFieldValues[def.FieldOrigin.sName] = new XIIValue { FKiFieldDefinitionID = Convert.ToInt32(def.ID), sValue = sVal, FKiFieldOriginID = def.FKiXIFieldOriginID, bIsDisplay = def.FieldOrigin.bIsDisplay };
                                                            }
                                                        }
                                                        else if (def.FieldOrigin.iScriptType == 40)
                                                        {
                                                            if (!string.IsNullOrEmpty(result) && result.ToLower() == "true")
                                                            {
                                                                def.FieldOrigin.bIsHidden = true;
                                                                def.FieldOrigin.sIsHidden = "on";
                                                                def.FieldOrigin.bIsMandatory = false;
                                                            }
                                                            else
                                                            {
                                                                def.FieldOrigin.bIsHidden = false;
                                                                def.FieldOrigin.sIsHidden = "off";
                                                                nSecFieldValues[def.FieldOrigin.sName] = new XIIValue { FKiFieldDefinitionID = Convert.ToInt32(def.ID), sValue = sVal, FKiFieldOriginID = def.FKiXIFieldOriginID, bIsDisplay = def.FieldOrigin.bIsDisplay };
                                                            }
                                                        }
                                                        if (!string.IsNullOrEmpty(def.FieldOrigin.sFieldDefaultValue))
                                                        {
                                                            nSecFieldValues[def.FieldOrigin.sName] = new XIIValue { FKiFieldDefinitionID = Convert.ToInt32(def.ID), sValue = def.FieldOrigin.sFieldDefaultValue, FKiFieldOriginID = def.FKiXIFieldOriginID, bIsDisplay = def.FieldOrigin.bIsDisplay };
                                                        }
                                                    }
                                                }
                                            }
                                            oQSInstance.XIValues[def.FieldOrigin.sName] = nSecFieldValues[def.FieldOrigin.sName];
                                            if (def.FieldOrigin.FK1ClickID > 0 && def.FieldOrigin.FKiBOID > 0)
                                            {
                                                var BOID = def.FieldOrigin.FKiBOID;
                                                Dictionary<string, object> Params = new Dictionary<string, object>();
                                                Params["BOID"] = BOID;
                                                string sSelectFields = string.Empty;
                                                sSelectFields = "Name,BOID,iDataSource,sSize,TableName,sPrimaryKey,sType";
                                                var FKBOD = Connection.Select<XIDBO>("XIBO_T_N", Params, sSelectFields).FirstOrDefault();
                                                def.FieldOrigin.sBOSize = FKBOD.sSize;
                                            }
                                        }
                                    }
                                    oSecIns.XIValues = nSecFieldValues;
                                    oStepInstance.FieldOriginIDs = FieldOriginIDs;
                                }
                            }
                            else
                            {
                                oSecIns.FKiStepSectionDefinitionID = sec.ID;
                                if (((xiSectionContent)sec.iDisplayAs).ToString() == xiSectionContent.Fields.ToString())
                                {
                                    var Oldfields = oSecIns.XIValues;
                                    if (Oldfields == null)
                                    {
                                        Oldfields = new Dictionary<string, XIIValue>();
                                    }
                                    Dictionary<string, XIIValue> nSecFieldValues = new Dictionary<string, XIIValue>();
                                    var Def = sec.FieldDefs.OrderBy(m => m.Value.ID).ToList();
                                    foreach (var items in Def)
                                    {
                                        if (Oldfields.Count() == 0 || Oldfields[items.Value.FieldOrigin.sName] == null || items.Value.FieldOrigin.bIsReload)
                                        {
                                            nSecFieldValues[items.Value.FieldOrigin.sName] = new XIIValue { FKiFieldDefinitionID = items.Value.ID, bIsDisplay = items.Value.FieldOrigin.bIsDisplay };
                                        }
                                        else
                                        {
                                            nSecFieldValues[items.Value.FieldOrigin.sName] = Oldfields[items.Value.FieldOrigin.sName];
                                        }
                                        if ((items.Value.FieldOrigin.bIsMerge && !String.IsNullOrEmpty(items.Value.FieldOrigin.sMergeField) && (Oldfields.Count() == 0 || (Oldfields.Count() > 0 && items.Value.FieldOrigin.bIsReload))))
                                        {
                                            nSecFieldValues[items.Value.FieldOrigin.sName] = new XIIValue { FKiFieldDefinitionID = Convert.ToInt32(items.Value.ID), sValue = oQSInstance.XIIValues(items.Value.FieldOrigin.sMergeField), FKiFieldOriginID = items.Value.FKiXIFieldOriginID, bIsDisplay = items.Value.FieldOrigin.bIsDisplay };
                                        }
                                        if (!string.IsNullOrEmpty(items.Value.FieldOrigin.sScript))
                                        {
                                            string sVal = string.Empty;
                                            if (nSecFieldValues.ContainsKey(items.Value.FieldOrigin.sName))
                                            {
                                                sVal = nSecFieldValues[items.Value.FieldOrigin.sName].sValue;
                                            }
                                            XIDScript oScriptD = new XIDScript();
                                            oScriptD.sScript = items.Value.FieldOrigin.sScript;
                                            if (oScriptD != null)
                                            {
                                                var oRes = oScriptD.Execute_Script(sGUID, sSessionID);
                                                if (oRes.bOK && oRes.oResult != null)
                                                {
                                                    string result = (string)oRes.oResult;
                                                    if (items.Value.FieldOrigin.iScriptType == 50)
                                                    {
                                                        if (!string.IsNullOrEmpty(result) && result.ToLower() == "true")
                                                        {
                                                            items.Value.FieldOrigin.bIsDisable = true;
                                                        }
                                                        else
                                                        {
                                                            items.Value.FieldOrigin.bIsDisable = false;
                                                            nSecFieldValues[items.Value.FieldOrigin.sName] = new XIIValue { FKiFieldDefinitionID = Convert.ToInt32(items.Value.ID), sValue = sVal, FKiFieldOriginID = items.Value.FKiXIFieldOriginID, bIsDisplay = items.Value.FieldOrigin.bIsDisplay };
                                                        }
                                                    }
                                                    else if (items.Value.FieldOrigin.iScriptType == 40)
                                                    {
                                                        if (!string.IsNullOrEmpty(result) && result.ToLower() == "true")
                                                        {
                                                            items.Value.FieldOrigin.bIsHidden = true;
                                                            items.Value.FieldOrigin.sIsHidden = "on";
                                                            items.Value.FieldOrigin.bIsMandatory = false;
                                                        }
                                                        else
                                                        {
                                                            items.Value.FieldOrigin.bIsHidden = false;
                                                            items.Value.FieldOrigin.sIsHidden = "off";
                                                            nSecFieldValues[items.Value.FieldOrigin.sName] = new XIIValue { FKiFieldDefinitionID = Convert.ToInt32(items.Value.ID), sValue = sVal, FKiFieldOriginID = items.Value.FKiXIFieldOriginID, bIsDisplay = items.Value.FieldOrigin.bIsDisplay };
                                                        }
                                                    }
                                                    if (!string.IsNullOrEmpty(items.Value.FieldOrigin.sFieldDefaultValue))
                                                    {
                                                        nSecFieldValues[items.Value.FieldOrigin.sName] = new XIIValue { FKiFieldDefinitionID = Convert.ToInt32(items.Value.ID), sValue = items.Value.FieldOrigin.sFieldDefaultValue, FKiFieldOriginID = items.Value.FKiXIFieldOriginID, bIsDisplay = items.Value.FieldOrigin.bIsDisplay };
                                                    }
                                                    else if ((items.Value.FieldOrigin.bIsMerge && !String.IsNullOrEmpty(items.Value.FieldOrigin.sMergeBo) && !String.IsNullOrEmpty(items.Value.FieldOrigin.sMergeBoField) && !string.IsNullOrEmpty(items.Value.FieldOrigin.sMergeVariable) && (Oldfields.Count() == 0 || (Oldfields.Count() > 0 && items.Value.FieldOrigin.bIsReload))))
                                                    {
                                                        XIInfraCache oCache = new XIInfraCache();
                                                        XIDBO oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, items.Value.FieldOrigin.sMergeBo, null);
                                                        string sMergeCacheField = items.Value.FieldOrigin.sMergeVariable;
                                                        var InstanceID = (string)oCache.Get_ParamVal(sSessionID, sGUID, null, sMergeCacheField);
                                                        int iInstanceiD = 0;
                                                        if (int.TryParse(InstanceID, out iInstanceiD))
                                                        {
                                                            XIIXI oXIIXI = new XIIXI();
                                                            if (iInstanceiD != 0)
                                                            {
                                                                var oBOI = oXIIXI.BOI(oBOD.Name, iInstanceiD.ToString());
                                                                if (oBOI.Attributes.ContainsKey(items.Value.FieldOrigin.sMergeBoField))
                                                                {
                                                                    if (oBOD.Name == "EnumReconciliations_T")
                                                                    {
                                                                        if (oBOI.Attributes.ContainsKey(items.Value.FieldOrigin.sMergeBoField))
                                                                        {
                                                                            nSecFieldValues[items.Value.FieldOrigin.sName] = new XIIValue { FKiFieldDefinitionID = Convert.ToInt32(items.Value.ID), sValue = oBOI.Attributes[items.Value.FieldOrigin.sMergeBoField].sValue + " - " + DateTime.Now.ToString(XIConstant.DateTimeFull_Format), FKiFieldOriginID = items.Value.FKiXIFieldOriginID, bIsDisplay = items.Value.FieldOrigin.bIsDisplay };
                                                                        }
                                                                    }
                                                                    else
                                                                    {
                                                                        nSecFieldValues[items.Value.FieldOrigin.sName] = new XIIValue { FKiFieldDefinitionID = Convert.ToInt32(items.Value.ID), sValue = oBOI.Attributes[items.Value.FieldOrigin.sMergeBoField].sValue, FKiFieldOriginID = items.Value.FKiXIFieldOriginID, bIsDisplay = items.Value.FieldOrigin.bIsDisplay };
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                        else if ((items.Value.FieldOrigin.bIsMerge && !String.IsNullOrEmpty(items.Value.FieldOrigin.sMergeBo) && !String.IsNullOrEmpty(items.Value.FieldOrigin.sMergeBoField) && !string.IsNullOrEmpty(items.Value.FieldOrigin.sMergeVariable) && (Oldfields.Count() == 0 || (Oldfields.Count() > 0 && items.Value.FieldOrigin.bIsReload))))
                                        {
                                            XIInfraCache oCache = new XIInfraCache();
                                            XIDBO oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, items.Value.FieldOrigin.sMergeBo, null);
                                            string sMergeCacheField = items.Value.FieldOrigin.sMergeVariable;
                                            var InstanceID = (string)oCache.Get_ParamVal(sSessionID, sGUID, null, sMergeCacheField);
                                            int iInstanceiD = 0;
                                            if (int.TryParse(InstanceID, out iInstanceiD))
                                            {
                                                XIIXI oXIIXI = new XIIXI();
                                                if (iInstanceiD != 0)
                                                {
                                                    var oBOI = oXIIXI.BOI(oBOD.Name, iInstanceiD.ToString());
                                                    if (oBOI.Attributes.ContainsKey(items.Value.FieldOrigin.sMergeBoField))
                                                    {
                                                        if (oBOD.Name == "EnumReconciliations_T")
                                                        {
                                                            if (oBOI.Attributes.ContainsKey(items.Value.FieldOrigin.sMergeBoField))
                                                            {
                                                                nSecFieldValues[items.Value.FieldOrigin.sName] = new XIIValue { FKiFieldDefinitionID = Convert.ToInt32(items.Value.ID), sValue = oBOI.Attributes[items.Value.FieldOrigin.sMergeBoField].sValue + " - " + DateTime.Now.ToString(XIConstant.DateTimeFull_Format), FKiFieldOriginID = items.Value.FKiXIFieldOriginID, bIsDisplay = items.Value.FieldOrigin.bIsDisplay };
                                                            }
                                                        }
                                                        else
                                                        {
                                                            nSecFieldValues[items.Value.FieldOrigin.sName] = new XIIValue { FKiFieldDefinitionID = Convert.ToInt32(items.Value.ID), sValue = oBOI.Attributes[items.Value.FieldOrigin.sMergeBoField].sValue, FKiFieldOriginID = items.Value.FKiXIFieldOriginID, bIsDisplay = items.Value.FieldOrigin.bIsDisplay };
                                                        }
                                                    }
                                                    else
                                                    {
                                                        List<XIWhereParams> oWParams = new List<XIWhereParams>();
                                                        string[] operators = new string[] { "+", "-", "/", "%", "(", ")" };
                                                        List<string[]> lstItems = items.Value.FieldOrigin.sMergeBoField.Split(',').Select(f => new { Field = f }).Select(c => c.Field.Trim().Split(operators, StringSplitOptions.RemoveEmptyEntries)).ToList();
                                                        for (int i = 0; i < lstItems.Count; i++)
                                                        {
                                                            if (lstItems[i].Count() > 1)
                                                            {
                                                                Regex pattern = new Regex(Regex.Escape(lstItems[i][0]) + "(.+?)" + Regex.Escape(lstItems[i][1]));
                                                                var getoperators = pattern.Matches(items.Value.FieldOrigin.sMergeBoField).Cast<Match>().Select(s => s.Groups[1].Value).Distinct().ToList();
                                                                if (getoperators.Count() > 0)
                                                                {
                                                                    nSecFieldValues[items.Value.FieldOrigin.sName] = new XIIValue
                                                                    {
                                                                        FKiFieldDefinitionID = Convert.ToInt32(items.Value.ID),
                                                                        sValue = (oBOI.Attributes[lstItems[i][0]].doValue - oBOI.Attributes[lstItems[i][1]].doValue).ToString(),
                                                                        FKiFieldOriginID = items.Value.FKiXIFieldOriginID,
                                                                        bIsDisplay = items.Value.FieldOrigin.bIsDisplay
                                                                    };
                                                                }
                                                            }
                                                            else
                                                            {
                                                                nSecFieldValues[items.Value.FieldOrigin.sName] = new XIIValue { FKiFieldDefinitionID = Convert.ToInt32(items.Value.ID), sValue = "", FKiFieldOriginID = items.Value.FKiXIFieldOriginID, bIsDisplay = items.Value.FieldOrigin.bIsDisplay };
                                                            }
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    nSecFieldValues[items.Value.FieldOrigin.sName] = new XIIValue { FKiFieldDefinitionID = Convert.ToInt32(items.Value.ID), sValue = "", FKiFieldOriginID = items.Value.FKiXIFieldOriginID, bIsDisplay = items.Value.FieldOrigin.bIsDisplay };
                                                }
                                            }
                                            else
                                            {
                                                nSecFieldValues[items.Value.FieldOrigin.sName] = new XIIValue { FKiFieldDefinitionID = Convert.ToInt32(items.Value.ID), sValue = "", FKiFieldOriginID = items.Value.FKiXIFieldOriginID, bIsDisplay = items.Value.FieldOrigin.bIsDisplay };
                                            }
                                        }
                                            if (!string.IsNullOrEmpty(items.Value.FieldOrigin.sMinDate) && (items.Value.FieldOrigin.sMinDate.Contains("xi.s")))
                                            {
                                                XIDScript oXIScript = new XIDScript();
                                                oXIScript.sScript = items.Value.FieldOrigin.sMinDate.ToString();
                                                var oCResult = oXIScript.Execute_Script(sGUID, sSessionID);
                                                if (oCResult.bOK && oCResult.oResult != null)
                                                {
                                                    var sVal = (string)oCResult.oResult;
                                                    sVal = Utility.GetDateResolvedValue(sVal, "yyyy-MM-dd");
                                                    items.Value.FieldOrigin.sMinDate = sVal;
                                                }
                                            }
                                            if (!string.IsNullOrEmpty(items.Value.FieldOrigin.sMaxDate) && (items.Value.FieldOrigin.sMaxDate.Contains("xi.s")))
                                            {
                                                XIDScript oXIScript = new XIDScript();
                                                oXIScript.sScript = items.Value.FieldOrigin.sMaxDate.ToString();
                                                var oCResult = oXIScript.Execute_Script(sGUID, sSessionID);
                                                if (oCResult.bOK && oCResult.oResult != null)
                                                {
                                                    var sVal = (string)oCResult.oResult;
                                                items.Value.FieldOrigin.sMaxDate = Utility.GetDateResolvedValue(sVal, "yyyy-MM-dd");
                                                CResult oSResult = new CResult();
                                                oSResult.sMessage = items.Value.FieldOrigin.sName + "_" + items.Value.FieldOrigin.ID + " : Script Value:" + sVal + " & Resolved Value:" + items.Value.FieldOrigin.sMaxDate;
                                                SaveErrortoDB(oSResult, oQSInstance.ID);
                                            }
                                        }
                                        oQSInstance.XIValues[items.Value.FieldOrigin.sName] = nSecFieldValues[items.Value.FieldOrigin.sName];
                                    }
                                    oSecIns.XIValues = nSecFieldValues;
                                }
                            }
                            if (!string.IsNullOrEmpty(sec.sName))
                            {
                                nSecIns[sec.ID.ToString() + "_Sec"] = oSecIns;
                                //nSecIns[sec.sName] = oSecIns;
                            }
                            else
                            {
                                nSecIns[sec.ID.ToString() + "_Sec"] = oSecIns;
                            }

                        }
                        //oStepInstance.nSectionInstances = Step.Sections;
                    }
                    oStepInstance.Sections = nSecIns;
                }
                else
                {

                }
                if (oQSInstance.Steps.Count() > 0)
                {
                    var StepIns = oQSInstance.Steps.Where(m => m.Value.FKiQSStepDefinitionID == Step.ID).FirstOrDefault();
                    if (StepIns.Value != null)
                    {
                        oStepInstance.ID = StepIns.Value.ID;
                    }
                }

                oStepInstance.FKiQSStepDefinitionID = Step.ID;
                //oStepInstance.nFieldInstances = nFieldValues;
                //oStepInstance.QSStepDefiniton = Step;
                if (oQSInstance.Steps != null && oQSInstance.Steps.Count() > 0)
                {
                    if (oQSInstance.Steps.ContainsKey(Step.sName))
                    {
                        oQSInstance.Steps[Step.sName] = oStepInstance;//Adding Step Instance to nInstances
                    }
                    else
                    {
                        //var StepIndex = oQSInstance.Steps.Values.FindIndex(m => m.FKiQSStepDefinitionID == Step.ID);
                        //oQSInstance.Steps.Remove(oQSInstance.Steps.Where(m => m.FKiQSStepDefinitionID == Step.ID).FirstOrDefault());
                        oQSInstance.Steps[Step.sName] = oStepInstance;
                    }
                }
                else
                {
                    oQSInstance.Steps = new Dictionary<string, XIIQSStep>();
                    if (!oQSInstance.Steps.ContainsKey(Step.sName))
                    {
                        oQSInstance.Steps[Step.sName] = oStepInstance;//Adding Step Instance to nInstances
                    }
                }
                //Setting Current Step Property to the step to be displayed
                oQSInstance.Steps.ToList().ForEach(m => m.Value.bIsCurrentStep = false);
                if (StepID > 0)
                {
                    oQSInstance.Steps.Where(m => m.Value.FKiQSStepDefinitionID == StepID).FirstOrDefault().Value.bIsCurrentStep = true;
                }
                else
                {
                    oQSInstance.Steps.FirstOrDefault().Value.bIsCurrentStep = true;
                }
                //if (oQSInstance.QSDefinition.Steps.Where(m => m.Value.ID == StepID).FirstOrDefault().Value != null)
                //{
                //    oQSInstance.QSDefinition.Steps.Where(m => m.Value.ID == StepID).FirstOrDefault().Value.sIsHidden = "off";
                //}

            }
            if (oQSInstance.QSDefinition.Steps.Where(m => m.Value.ID == Step.ID).Select(m => m.Value.bInMemoryOnly).FirstOrDefault() == false)
            {
                //SaveQSInstances(oQSInstance, sGUID);
            }

            //oCache.UpdateCacheObject("QuestionSet", sGUID, oQSInstance, sDatabase, oQSInstance.FKiQSDefinitionID);
            return oQSInstance;
        }


        public int GetActiveStepID(int iStepID, string sGUID = "")
        {
            var sSessionID = HttpContext.Current.Session.SessionID;
            string bIsActive = string.Empty; ;
            XIDQSStep oStepD = new XIDQSStep();
            if (iStepID >= 0)
            {
                if (iStepID == 0)
                {
                    oStepD = QSDefinition.Steps.Values.ToList().OrderBy(m => m.iOrder).FirstOrDefault();
                }
                else
                {
                    oStepD = QSDefinition.Steps.Values.Where(m => m.ID == iStepID).FirstOrDefault();
                }
                //oStepD = QSDefinition.Steps.Values.ToList().OrderBy(m => m.iOrder).FirstOrDefault();
                if (oStepD.Scripts != null && oStepD.Scripts.Count() > 0)
                {
                    XIDScript oScriptD = new XIDScript();
                    oScriptD = oStepD.Scripts["Step Visibility"];
                    if (oScriptD != null && oScriptD.sLanguage.ToLower() == "XIScript".ToLower())
                    {
                        var oRes = oScriptD.Execute_Script(sGUID, sSessionID);
                        if (oRes.bOK && oRes.oResult != null)
                        {
                            bIsActive = (string)oRes.oResult;
                            if (bIsActive == "false")
                            {
                                iStepID = QSDefinition.Steps.Values.ToList().OrderBy(m => m.iOrder).Where(m => m.iOrder > oStepD.iOrder).Select(m => m.ID).FirstOrDefault();
                                iActiveStepID = iStepID;
                                GetActiveStepID(iStepID, sGUID);
                            }
                            else
                            {
                                iActiveStepID = oStepD.ID;
                                //if (!string.IsNullOrEmpty(sTransactionType))
                                //{
                                //    iActiveStepID = oStepD.ID;
                                //}
                                //else
                                //{
                                //    iStepID = QSDefinition.Steps.Values.ToList().OrderBy(m => m.iOrder).Where(m => m.iOrder > oStepD.iOrder).Select(m => m.ID).FirstOrDefault();
                                //    iActiveStepID = iStepID;
                                //    GetActiveStepID(iStepID, sTransactionType);
                                //}
                            }
                        }
                    }
                }
                else
                {
                    if (oStepD.bIsContinue)
                    {
                        iStepID = QSDefinition.Steps.Values.ToList().OrderBy(m => m.iOrder).Where(m => m.iOrder > oStepD.iOrder).Select(m => m.ID).FirstOrDefault();
                        iActiveStepID = iStepID;
                    }
                    else
                    {
                        iActiveStepID = oStepD.ID;
                    }
                }
            }
            return iActiveStepID;
        }

        public XIIQSSection LoadSectionInstance(XIDQSSection oXIDQS, string sGUID = "")
        {
            var sSessionID = HttpContext.Current.Session.SessionID;
            XIDQSSection sec = oXIDQS;
            XIIQSSection oSecIns = null;
            try
            {
                if (oSecIns == null)
                {
                    oSecIns = new XIIQSSection();
                    oSecIns.FKiStepSectionDefinitionID = oXIDQS.ID;
                    if (((xiSectionContent)sec.iDisplayAs).ToString() == xiSectionContent.Fields.ToString())
                    {
                        Dictionary<string, XIIValue> nSecFieldValues = new Dictionary<string, XIIValue>();
                        if (sec.FieldDefs != null && sec.FieldDefs.Count() > 0)
                        {
                            var Def = sec.FieldDefs.Values.OrderBy(m => m.ID).ToList();//.Select(m => new XIIValue { FKiFieldDefinitionID = m.ID }).ToList();
                            foreach (var def in Def)
                            {
                                if (def.FieldOrigin.bIsMerge && !String.IsNullOrEmpty(def.FieldOrigin.sMergeField))
                                {
                                    //nSecFieldValues[def.FieldOrigin.sName] = new XIIValue { FKiFieldDefinitionID = Convert.ToInt32(def.ID), sValue = oQSInstance.XIIValues(def.FieldOrigin.sMergeField), FKiFieldOriginID = def.FKiXIFieldOriginID };
                                }
                                else if (def.FieldOrigin.bIsMerge && !String.IsNullOrEmpty(def.FieldOrigin.sMergeBo) && !String.IsNullOrEmpty(def.FieldOrigin.sMergeBoField) && !string.IsNullOrEmpty(def.FieldOrigin.sMergeVariable))
                                {
                                    XIInfraCache oCache = new XIInfraCache();
                                    XIDBO oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, def.FieldOrigin.sMergeBo, null);
                                    string sMergeCacheField = def.FieldOrigin.sMergeVariable;
                                    var InstanceID = (string)oCache.Get_ParamVal(sSessionID, sGUID, null, sMergeCacheField);
                                    int iInstanceiD = 0;
                                    if (int.TryParse(InstanceID, out iInstanceiD))
                                    {
                                        XIIXI oXIIXI = new XIIXI();
                                        var oBOI = oXIIXI.BOI(oBOD.Name, iInstanceiD.ToString());
                                        if (oBOI.Attributes.ContainsKey(def.FieldOrigin.sMergeBoField))
                                        {
                                            if (def.FieldOrigin.DataType.sName.ToLower() == "date")
                                            {
                                                string sFormat = def.FieldOrigin.sFormat;
                                                if (string.IsNullOrEmpty(sFormat))
                                                {
                                                    sFormat = XIConstant.Date_Format; //"dd-MMM-yyyy";
                                                }
                                                if (!string.IsNullOrEmpty(oBOI.Attributes[def.FieldOrigin.sMergeBoField].sValue))
                                                {
                                                    string sValue = oBOI.Attributes[def.FieldOrigin.sMergeBoField].sValue;
                                                    oBOI.Attributes[def.FieldOrigin.sMergeBoField].sValue = String.Format("{0:" + sFormat + "}", Convert.ToDateTime(sValue));
                                                }
                                            }
                                            nSecFieldValues[def.FieldOrigin.sName] = new XIIValue { FKiFieldDefinitionID = Convert.ToInt32(def.ID), sValue = oBOI.Attributes[def.FieldOrigin.sMergeBoField].sValue, FKiFieldOriginID = def.FKiXIFieldOriginID, bIsDisplay = def.FieldOrigin.bIsDisplay };
                                        }
                                        else
                                        {
                                            nSecFieldValues[def.FieldOrigin.sName] = new XIIValue { FKiFieldDefinitionID = Convert.ToInt32(def.ID), sValue = "", FKiFieldOriginID = def.FKiXIFieldOriginID, bIsDisplay = def.FieldOrigin.bIsDisplay };
                                        }
                                    }
                                    else
                                    {
                                        nSecFieldValues[def.FieldOrigin.sName] = new XIIValue { FKiFieldDefinitionID = Convert.ToInt32(def.ID), sValue = "", FKiFieldOriginID = def.FKiXIFieldOriginID, bIsDisplay = def.FieldOrigin.bIsDisplay };
                                    }
                                }
                                else
                                {
                                    if (!string.IsNullOrEmpty(def.FieldOrigin.sDefaultDate))
                                    {
                                        nSecFieldValues[def.FieldOrigin.sName] = new XIIValue { FKiFieldDefinitionID = Convert.ToInt32(def.ID), sValue = Utility.GetDefaultDateResolvedValue(def.FieldOrigin.sDefaultDate, XIConstant.Date_Format), FKiFieldOriginID = def.FKiXIFieldOriginID, bIsDisplay = def.FieldOrigin.bIsDisplay };
                                    }
                                    else
                                    {
                                        //if (oQSInstance.XIValues.ContainsKey(def.FieldOrigin.sName.ToString()))
                                        //{
                                        //    nSecFieldValues[def.FieldOrigin.sName] = new XIIValue { FKiFieldDefinitionID = Convert.ToInt32(def.ID), sValue = oQSInstance.XIValues[def.FieldOrigin.sName.ToString()].sValue, FKiFieldOriginID = def.FKiXIFieldOriginID };
                                        //}
                                        //else
                                        //{
                                        //    nSecFieldValues[def.FieldOrigin.sName] = new XIIValue { FKiFieldDefinitionID = Convert.ToInt32(def.ID), FKiFieldOriginID = def.FKiXIFieldOriginID };
                                        //}
                                    }
                                }
                                if (!string.IsNullOrEmpty(def.FieldOrigin.sScript))
                                {
                                    string sVal = string.Empty;
                                    if (nSecFieldValues.ContainsKey(def.FieldOrigin.sName))
                                    {
                                        sVal = nSecFieldValues[def.FieldOrigin.sName].sValue;
                                    }
                                    XIDScript oScriptD = new XIDScript();
                                    oScriptD.sScript = def.FieldOrigin.sScript;
                                    if (oScriptD != null)
                                    {
                                        var oRes = oScriptD.Execute_Script(sGUID, sSessionID);
                                        if (oRes.bOK && oRes.oResult != null)
                                        {
                                            string result = (string)oRes.oResult;
                                            if (!string.IsNullOrEmpty(result) && result.ToLower() == "true")
                                            {
                                                def.FieldOrigin.bIsDisable = true;
                                            }
                                            else
                                            {
                                                def.FieldOrigin.bIsDisable = false;
                                                nSecFieldValues[def.FieldOrigin.sName] = new XIIValue { FKiFieldDefinitionID = Convert.ToInt32(def.ID), sValue = sVal, FKiFieldOriginID = def.FKiXIFieldOriginID, bIsDisplay = def.FieldOrigin.bIsDisplay };
                                            }
                                            if (!string.IsNullOrEmpty(def.FieldOrigin.sFieldDefaultValue))
                                            {
                                                nSecFieldValues[def.FieldOrigin.sName] = new XIIValue { FKiFieldDefinitionID = Convert.ToInt32(def.ID), sValue = def.FieldOrigin.sFieldDefaultValue, FKiFieldOriginID = def.FKiXIFieldOriginID, bIsDisplay = def.FieldOrigin.bIsDisplay };
                                            }
                                        }
                                    }
                                }
                                //oQSInstance.XIValues[def.FieldOrigin.sName] = nSecFieldValues[def.FieldOrigin.sName];
                            }
                        }
                        oSecIns.XIValues = nSecFieldValues;
                    }
                }
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                SaveErrortoDB(oCResult);
            }
            return oSecIns;
        }

        public XIIQS SaveQSAPI(XIIQS oQSInstance)
        {
            // TO DO - return type should be an object

            // TO DO - this method means, there is a full object model in memory with steps and maybe sections
            // and this code needs to persist this into the DB
            XIIQS oQSIns = null;
            Dictionary<string, object> Params = new Dictionary<string, object>();
            //Params["FKiQSDefinitionID"] = oQSInstance.FKiQSDefinitionID;
            oQSIns = new XIIQS();
            oQSIns.sQSName = oQSInstance.QSDefinition.sName;
            oQSIns.iCurrentStepID = oQSInstance.iCurrentStepID;
            oQSIns.FKiQSDefinitionID = oQSInstance.FKiQSDefinitionID;
            oQSIns.FKiBODID = oQSInstance.FKiBODID;
            oQSIns.iBOIID = oQSInstance.iBOIID;
            oQSIns.FKiClassID = oQSInstance.QSDefinition.FKiClassID;
            oQSIns.CreatedTime = DateTime.Now;
            oQSIns = Connection.Insert<XIIQS>(oQSIns, "XIQSInstance_T", "ID");
            oQSInstance.ID = oQSIns.ID;
            XIInfraCache oCache = new XIInfraCache();
            XIIBO BulkInsert = new XIIBO();
            List<XIIBO> oBulkBO = new List<XIIBO>();
            XIIValue oFIns = new XIIValue();
            oFIns = new XIIValue();
            foreach (var items in oQSInstance.XIValues)
            {
                XIIBO oxiibo = new XIIBO();
                var FieldOrigin = oQSInstance.QSDefinition.XIDFieldOrigin.Values.Where(m => m.ID == items.Value.FKiFieldOriginID).FirstOrDefault();
                if (FieldOrigin.DataType.sBaseDataType.ToLower() == "int")
                {
                    int ival;
                    if (int.TryParse(items.Value.sValue, out ival))
                    {
                        oFIns.iValue = ival;
                    }
                    else
                    {
                        oFIns.iValue = 0;
                    }
                }
                else if (FieldOrigin.DataType.sBaseDataType.ToLower() == "datetime")
                {
                    //if (!string.IsNullOrEmpty(items.Value.sValue))
                    //{
                    //    oFIns.dValue = Convert.ToDateTime(items.Value.sValue);
                    //}
                }
                else if (FieldOrigin.DataType.sBaseDataType.ToLower() == "decimal")
                {
                    decimal rval;
                    if (decimal.TryParse(items.Value.sValue, out rval))
                    {
                        oFIns.rValue = rval;
                    }
                    else
                    {
                        oFIns.rValue = 0;
                    }
                }
                else if (FieldOrigin.DataType.sBaseDataType.ToLower() == "boolean")
                {
                    if (items.Value.sValue == "on")
                    {
                        oFIns.bValue = true;
                    }
                    else
                    {
                        oFIns.bValue = false;
                    }

                }
                oxiibo.SetAttribute("rValue", oFIns.rValue.ToString());
                oxiibo.SetAttribute("bValue", oFIns.bValue.ToString());
                oxiibo.SetAttribute("iValue", oFIns.iValue.ToString());
                //if (oFIns.dValue.ToString() == "1/1/0001 12:00:00 AM")
                //{
                //    oFIns.dValue = Convert.ToDateTime("1/1/1900 12:00:00 AM");
                //}

                oxiibo.SetAttribute("sDerivedValue", items.Value.sDerivedValue);
                oxiibo.SetAttribute("sValue", items.Value.sValue);
                oxiibo.SetAttribute("FKiQSInstanceID", oQSIns.ID.ToString());
                oxiibo.SetAttribute("FKiFieldOriginID", FieldOrigin.ID.ToString());
                oxiibo.SetAttribute("dValue", DateTime.Now.ToString());

                oFIns.sDerivedValue = items.Value.sDerivedValue;
                oFIns.sValue = items.Value.sValue;
                if (string.IsNullOrEmpty(oFIns.sDerivedValue))
                {
                    oFIns.sDerivedValue = items.Value.sValue;
                    oxiibo.SetAttribute("sDerivedValue", items.Value.sValue);
                }

                oFIns.FKiQSInstanceID = oQSIns.ID;
                oFIns.FKiFieldDefinitionID = items.Value.FKiFieldDefinitionID;
                oFIns.FKiFieldOriginID = FieldOrigin.ID;
                oFIns.dValue = DateTime.Now;
                //oFIns = Connection.Insert<XIIValue>(oFIns, "XIFieldInstance_T", "ID");
                oBulkBO.Add(oxiibo);
            }
            if (oBulkBO != null && oBulkBO.Count() > 0)
            {
                var BoD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "XIFieldInstance_T", null);
                oBulkBO.ForEach(f => f.BOD = BoD);
                var MakeDatatble = BulkInsert.MakeBulkSqlTable(oBulkBO);
                BulkInsert.SaveBulk(MakeDatatble, BoD.iDataSource, "XIFieldInstance_T");
            }
            return oQSInstance;
        }
    }
}