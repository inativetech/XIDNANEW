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
using XISystem;
using XIDatabase;
using System.Configuration;

namespace XICore
{
    public class XIDQSStep : XIDefinitionBase
    {
        public int ID { get; set; }
        public int FKiQSDefintionID { get; set; }
        public string sName { get; set; }
        public string sDisplayName { get; set; }
        public decimal iOrder { get; set; }
        public string sCode { get; set; }
        public int iDisplayAs { get; set; }
        public int XILinkID { get; set; }
        public int FKiContentID { get; set; }
        public int iXIComponentID { get; set; }
        public int i1ClickID { get; set; }
        public string HTMLContent { get; set; }
        public bool bIsSaveNext { get; set; }
        public bool bIsSave { get; set; }
        public bool bIsBack { get; set; }
        public bool bIsContinue { get; set; }
        public bool bInMemoryOnly { get; set; }
        public int iLayoutID { get; set; }
        public string sIsHidden { get; set; }
        public bool bIsHistory { get; set; }
        public bool bIsCopy { get; set; }
        public string sSaveBtnLabel { get; set; }
        public string sBackBtnLabel { get; set; }
        public bool bIsReload { get; set; }
        public bool bIsMerge { get; set; }
        public int iStage { get; set; }
        public int iLockStage { get; set; }
        public int iCutStage { get; set; }
        public string sImage { get; set; }
        public string s1ClickIDs { get; set; }
        public List<XIDropDown> XIFields { get; set; }
        public Dictionary<string, string> XILinks { get; set; }//auto complete fields
        public List<XIDropDown> ddlContent { get; set; }
        public List<string> XIFieldValues { get; set; }
        public List<XIDropDown> ddlLayouts { get; set; }
        public Dictionary<string, string> XICodes { get; set; }  //Auto Complete XILink Codes
        public int FKiAppID { get; set; }
        public int iOrgID { get; set; }
        public string sSaveBtnLabelSaveNext { get; set; }
        public string sSaveBtnLabelSave { get; set; }
        public bool bIsHidden { get; set; }
        public List<XIDropDown> oQSddl { get; set; } //All QuestionSet DropDowns
        public int iOverrideStep { get; set; }
        public int FKiParentStepID { get; set; }
        public Guid XIGUID { get; set; }
        XIDBAPI Connection = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString);

        private XIDComponent oMyCompoent { get; set; }
        public XIDComponent ComponentDefinition
        {
            get { return oMyCompoent; }
            set { oMyCompoent = value; }
        }

        private XIDLayout oMyLayout;
        public XIDLayout Layout
        {
            get
            {
                return oMyLayout;
            }
            set
            {
                oMyLayout = value;
            }
        }

        private Dictionary<string, XIDValue> oMyXIValues = new Dictionary<string, XIDValue>();

        public Dictionary<string, XIDValue> XIValues
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
        public XIDValue XIValueD(string sValueName)
        {
            XIDValue oThisValueD = null/* TODO Change to default(_) if this is not a reference type */;

            // Ravi should have done a lot of this


            sValueName = sValueName.ToLower();


            if (oMyXIValues.ContainsKey(sValueName))
            {
            }
            else
            {
            }

            return oThisValueD;
        }

        public Dictionary<string, XIDQSSection> oMySections = new Dictionary<string, XIDQSSection>();
        public Dictionary<string, XIDQSSection> Sections
        {
            get
            {
                return oMySections;
            }
            set
            {
                oMySections = value;
            }
        }
        public XIDQSSection SectionD(string sSectionName)
        {
            XIDQSSection oThisSectionD = null/* TODO Change to default(_) if this is not a reference type */;

            // The sections of this Step must be loaded

            // TO DO - make this work on numbers also - eg 1 is an index position of 1

            sSectionName = sSectionName.ToLower();

            if (oMySections.ContainsKey(sSectionName) == false)
            {
            }

            if (oMySections.ContainsKey(sSectionName))
            {
            }
            else
            {
            }

            return oThisSectionD;
        }

        private Dictionary<string, XIDQSStepNavigations> oMyNavigations = new Dictionary<string, XIDQSStepNavigations>();
        public Dictionary<string, XIDQSStepNavigations> Navigations
        {
            get
            {
                return oMyNavigations;
            }
            set
            {
                oMyNavigations = value;
            }
        }

        private Dictionary<string, XIDFieldDefinition> oMyFieldDefs = new Dictionary<string, XIDFieldDefinition>();
        public Dictionary<string, XIDFieldDefinition> FieldDefs
        {
            get
            {
                return oMyFieldDefs;
            }
            set
            {
                oMyFieldDefs = value;
            }
        }

        private Dictionary<string, XIDScript> oMyScripts = new Dictionary<string, XIDScript>();
        public Dictionary<string, XIDScript> Scripts
        {
            get
            {
                return oMyScripts;
            }
            set
            {
                oMyScripts = value;
            }
        }

        private Dictionary<string, XIQSLink> oMyQSLinks = new Dictionary<string, XIQSLink>();
        public Dictionary<string, XIQSLink> QSLinks
        {
            get
            {
                return oMyQSLinks;
            }
            set
            {
                oMyQSLinks = value;
            }
        }

        public CResult Preview()
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
                XIInfraCache oCache = new XIInfraCache();
                XIDQSStep oStepD = new XIDQSStep();
                oStepD = (XIDQSStep)oDefintion;
                XIDQSStep oStepC = new XIDQSStep();
                oStepC = (XIDQSStep)oStepD.Clone(oStepD);
                if (oStepC.iLayoutID > 0)
                {
                    XIDLayout oLayoutD = new XIDLayout();
                    oLayoutD.ID = oStepC.iLayoutID;
                    var oLayContent = oLayoutD.Preview();
                    if (oLayContent.bOK && oLayContent.oResult != null)
                    {
                        oStepC.oContent[XIConstant.ContentLayout] = (XIDLayout)(oLayContent.oResult);
                    }
                }
                else
                {
                    if (oStepC.Sections != null && oStepC.Sections.Count() > 0)
                    {
                        Dictionary<string, XIDQSSection> Sections = new Dictionary<string, XIDQSSection>();
                        foreach (var sec in oStepC.Sections)
                        {
                            XIDQSSection oSecD = new XIDQSSection();
                            oSecD.oDefintion = sec.Value;
                            var oSecContent = oSecD.Preview();
                            if (oSecContent.bOK && oSecContent.oResult != null)
                            {
                                Sections[sec.Value.ID.ToString()] = (XIDQSSection)oSecContent.oResult;
                            }
                        }
                        oStepC.Sections = Sections;
                    }
                }
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                oCResult.oResult = oStepC;
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


        #region QSStepConfigComponent

        public CResult Get_QSStepConfigDef()
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
                XIDQSStep oQSStep = new XIDQSStep();
                Dictionary<string, object> Params = new Dictionary<string, object>();
                if (ID != 0)
                {
                    Params["ID"] = ID;
                    oQSStep = Connection.Select<XIDQSStep>("XIQSStepDefinition_T", Params).FirstOrDefault();
                }
                //All QuestionSet DropDowns
                List<XIDropDown> oQSDDL = new List<XIDropDown>();
                Dictionary<string, object> oQSParam = new Dictionary<string, object>();
                //oQSParam["FKiApplicationID"] = FKiAppID;
                //oQSParam["FKiOrganisationID"] = iOrgID;
                var oQSDef = Connection.Select<XIDQS>("XIQSDefinition_T", oQSParam).ToList();
                oQSDDL = oQSDef.Select(m => new XIDropDown { Value = m.ID, text = m.sName }).ToList();
                oQSDDL.Insert(0, new XIDropDown
                {
                    text = "--Select--",
                    Value = 0
                });
                oQSStep.oQSddl = oQSDDL;

                //FieldOrigins DropDowns
                List<XIDropDown> oFieldDDL = new List<XIDropDown>();
                Dictionary<string, object> oFO = new Dictionary<string, object>();
                oFO["FKiApplicationID"] = FKiAppID;
                oFO["FKiOrganisationID"] = iOrgID;
                var oFODef = Connection.Select<XIDFieldOrigin>("XIFieldOrigin_T", oFO).ToList();
                oFieldDDL = oFODef.Select(m => new XIDropDown { Value = m.ID, text = m.sName }).ToList();
                oQSStep.XIFields = oFieldDDL;

                //AutoComplete For Xilinks
                Dictionary<string, string> XiLinks = new Dictionary<string, string>();
                Dictionary<string, object> oLinks = new Dictionary<string, object>();
                oLinks["FKiApplicationID"] = FKiAppID;
                oLinks["OrganisationID"] = iOrgID;
                var oXiLinkDef = Connection.Select<XILink>("XILink_T", oLinks).ToList();
                foreach (var items in oXiLinkDef)
                {
                    XiLinks[items.Name] = items.Name;
                }
                oQSStep.XILinks = XiLinks;

                //AutoComplete For QSLink
                Dictionary<string, string> QSLink = new Dictionary<string, string>();
                Dictionary<string, object> oQSLinks = new Dictionary<string, object>();
                var QSLinkDef = Connection.Select<XIQSLink>("XIQSLinkDefinition_T", oQSLinks).ToList();
                var oQSLinkDef = QSLinkDef.ToList().Select(m => m.sCode).Distinct();
                foreach (var items in oQSLinkDef)
                {
                    QSLink[items] = items;
                }
                oQSStep.XICodes = QSLink;

                //FieldDefination DropDowns
                List<string> FieldValues = new List<string>();
                Dictionary<string, object> oFields = new Dictionary<string, object>();
                oFields["FKiApplicationID"] = FKiAppID;
                oFields["OrganisationID"] = iOrgID;
                oFields["FKiXIStepDefinitionID"] = ID;
                oFields["FKiStepSectionID"] = 0;
                var oFieldDef = Connection.Select<XIDFieldDefinition>("XIFieldDefinition_T", oFields).ToList();
                var lFieldDef = oFieldDef.Select(m => m.FKiXIFieldOriginID).ToList();
                foreach (var item in lFieldDef)
                {
                    var lXiFields = oFODef.Where(m => m.ID == item).ToList();
                    foreach (var items in lXiFields)
                    {
                        FieldValues.Add(items.sName);
                    }
                }
                oQSStep.XIFieldValues = FieldValues;

                //XILayout DropDowns
                List<XIDropDown> oLayoutDDL = new List<XIDropDown>();
                Dictionary<string, object> oLayout = new Dictionary<string, object>();
                oFO["FKiApplicationID"] = FKiAppID;
                oFO["OrganisationID"] = iOrgID;
                var oLayDef = Connection.Select<XIDLayout>("XILayout_T", oLayout).ToList();
                oLayoutDDL = oLayDef.Select(m => new XIDropDown { Value = m.ID, text = m.LayoutName }).ToList();
                oLayoutDDL.Insert(0, new XIDropDown
                {
                    text = "--Select--",
                    Value = 0
                });
                oQSStep.ddlLayouts = oLayoutDDL;
                oQSStep.ddlContent = new List<XIDropDown>();

                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                oCResult.oResult = oQSStep;
            }
            catch (Exception ex)
            {
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
            }
            return oCResult;
        }

        #endregion QSStepConfigComponent

    }

}