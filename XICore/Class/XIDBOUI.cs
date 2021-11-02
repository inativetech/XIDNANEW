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
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using XISystem;
using static XIDatabase.XIDBAPI;
using XIDatabase;
using System.Configuration;

namespace XICore
{
    public class XIDBOUI : XIDefinitionBase
    {
        [Key]
        public long ID { get; set; }
        public int FKiBOID { get; set; }
        public int i1ClickID { get; set; }
        public int iLayoutID { get; set; }
        public long FKiStructureID { get; set; }
        public int FKiQSTemplateID { get; set; }
        public int FKiQSStepTemplateID { get; set; }
        public int FKiApplicationID { get; set; }
        public int iPopupID { get; set; }
        [DapperIgnore]
        public string sBOName { get; set; }
        [DapperIgnore]
        public string sSavingType { get; set; }
        [DapperIgnore]
        public List<XIDropDown> ddl1Clicks { get; set; }
        [DapperIgnore]
        public List<XIDropDown> ddlLayouts { get; set; }
        [DapperIgnore]
        public List<XIDropDown> ddlQSTemplates { get; set; }
        [DapperIgnore]
        public List<XIDropDown> ddlQSStepTemplates { get; set; }
        [DapperIgnore]
        public int iQSDID { get; set; }
        [DapperIgnore]
        public List<XIDropDown> Structures { get; set; }
        [NotMapped]
        private string sMyDefaultLayout;
        [NotMapped]
        public string sDefaultLayout
        {
            get
            {
                return sMyDefaultLayout;
            }
            set
            {
                sMyDefaultLayout = value;
            }
        }

        XIDBAPI Connection = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString);

        public CResult Build(XIDBOUI oBOUI)
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
                XIDStructure oStructure = new XIDStructure();
                List<XIDStructure> oStructD = new List<XIDStructure>();
                oStructD = oStructure.GetXIStructureTreeDetails(oBOUI.FKiBOID, null, oBOUI.FKiStructureID);

                if (oBOUI.iLayoutID > 0)
                {
                    XIDLayout oLayoutD = new XIDLayout();
                    oLayoutD.ID = oBOUI.iLayoutID;
                    var oLayContent = oLayoutD.Preview();
                    if (oLayContent.bOK && oLayContent.oResult != null)
                    {
                        var oDef = (XIDefinitionBase)oLayContent.oResult;
                        if (oDef.oContent.ContainsKey(XIConstant.ContentLayout))
                        {
                            oLayoutD = (XIDLayout)oDef.oContent[XIConstant.ContentLayout];
                            if (oLayoutD.LayoutMappings != null && oLayoutD.LayoutMappings.Count() < 0)
                            {
                                foreach (var Map in oLayoutD.LayoutMappings)
                                {
                                    if (Map.oContent.ContainsKey(XIConstant.ContentXIComponent))
                                    {
                                        var oCompDef = (XIDefinitionBase)Map.oContent[XIConstant.ContentXIComponent];
                                        var oXICompD = (XIDComponent)oCompDef.oContent[XIConstant.ContentXIComponent];
                                        if (oXICompD.oContent.ContainsKey(XIConstant.ContentQuestionSet))
                                        {
                                            XIDQS oQSD = new XIDQS();
                                            oQSD = (XIDQS)oXICompD.oContent[XIConstant.ContentQuestionSet];
                                            var oQSC = (XIDQS)oQSD.Clone(oQSD);
                                            XIIBO oBOI = new XIIBO();
                                            oBOI = oBOI.Get_BOInstance("xiqsdefinition", "create");
                                            oBOI = oBOI.GetProperties(oQSC, oBOI);
                                            oBOI.Attributes[oBOI.BOD.sPrimaryKey.ToLower()].sValue = "0";
                                            var oResponse = oBOI.Save(oBOI);
                                            if (oResponse.bOK && oResponse.oResult != null)
                                            {
                                                var oQSBOI = (XIIBO)oResponse.oResult;
                                                var iQSDID = oQSBOI.Attributes[oBOI.BOD.sPrimaryKey.ToLower()].sValue;
                                                foreach (var oStepD in oQSC.Steps.Values)
                                                {
                                                    if (oStepD.iLayoutID > 0)
                                                    {
                                                        var oStepLayout = oStepD.Layout;
                                                        if (oStepLayout != null && oStepLayout.LayoutMappings != null)
                                                        {
                                                            foreach (var lay in oStepLayout.LayoutMappings)
                                                            {
                                                                if (lay.oContent.ContainsKey(XIConstant.ContentStep))
                                                                {

                                                                }
                                                            }
                                                        }
                                                    }
                                                    else if (oStepD.Sections != null && oStepD.Sections.Count() > 0)
                                                    {

                                                    }
                                                }

                                            }
                                        }
                                        else
                                        {

                                        }
                                    }
                                    else if (Map.oContent.ContainsKey(XIConstant.ContentXILink))
                                    {

                                    }
                                    else if (Map.oContent.ContainsKey(XIConstant.ContentStep))
                                    {

                                    }
                                    else if (Map.oContent.ContainsKey(XIConstant.ContentHTML))
                                    {

                                    }
                                }
                            }
                        }
                    }
                }

                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                //oCResult.oResult = oDefinition;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while loading XiLink Definition" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oCResult;
        }

        public CResult Get_BOUIDetails()
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
                XIDBOUI oBOUI = new XIDBOUI();

                if (ID > 0)
                {
                    Dictionary<string, object> UIParams = new Dictionary<string, object>();
                    UIParams["ID"] = ID;
                    oBOUI = Connection.Select<XIDBOUI>("XIBOUIDetails_T", UIParams).FirstOrDefault();

                    Dictionary<string, object> StrParams = new Dictionary<string, object>();
                    StrParams["ID"] = oBOUI.FKiStructureID;
                    var oStructDef = Connection.Select<XIDStructure>("XIBOStructure_T", StrParams).FirstOrDefault();
                    FKiBOID = oStructDef.BOID;


                    if (oBOUI == null)
                    {
                        oBOUI = new XIDBOUI();
                        oBOUI.FKiBOID = oStructDef.BOID;
                        oBOUI.sBOName = oStructDef.sBO;
                    }
                    else
                    {
                        oBOUI.FKiBOID = oBOUI.FKiBOID;
                        oBOUI.sBOName = oStructDef.sBO;
                        oBOUI.i1ClickID = oBOUI.i1ClickID;
                    }
                }

                Dictionary<string, object> StParams = new Dictionary<string, object>();
                StParams["BOID"] = FKiBOID;
                StParams["FKiParentID"] = "#";
                var oStDef = Connection.Select<XIDStructure>("XIBOStructure_T", StParams).ToList();
                oBOUI.Structures = oStDef.Select(m => new XIDropDown { Value = Convert.ToInt32(m.ID), text = m.sStructureName }).ToList();

                Dictionary<string, object> ClickParams = new Dictionary<string, object>();
                ClickParams["StatusTypeID"] = 10;
                var o1ClickDef = Connection.Select<XID1Click>("XI1Click_T", ClickParams).ToList();
                oBOUI.ddl1Clicks = o1ClickDef.Select(m => new XIDropDown { Value = m.ID, text = m.Name }).ToList();

                Dictionary<string, object> LayParams = new Dictionary<string, object>();
                LayParams["LayoutType"] = "template";
                var oLayoutDef = Connection.Select<XIDLayout>("XILayout_T", LayParams).ToList();
                oBOUI.ddlLayouts = oLayoutDef.Select(m => new XIDropDown { Value = m.ID, text = m.LayoutName }).ToList();

                Dictionary<string, object> QSParams = new Dictionary<string, object>();
                QSParams["bIsTemplate"] = true;
                var oQSTemplates = Connection.Select<XIDQS>("XIQSDefinition_T", QSParams).ToList();
                oBOUI.ddlQSTemplates = oQSTemplates.Select(m => new XIDropDown { Value = m.ID, text = m.sName }).ToList();

                Dictionary<string, object> QSStepParams = new Dictionary<string, object>();
                if (iQSDID > 0)
                {
                    QSStepParams["FKiQSDefintionID"] = iQSDID;
                }
                var oQSStepTemplates = Connection.Select<XIDQSStep>("XIQSStepDefinition_T", QSStepParams).ToList();
                oBOUI.ddlQSStepTemplates = oQSStepTemplates.Select(m => new XIDropDown { Value = m.ID, text = m.sName }).ToList();

                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                oCResult.oResult = oBOUI;
            }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error while loading BO definition" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
            }
            return oCResult;
        }

    }
}