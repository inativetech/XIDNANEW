using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using XICore;
using XIDatabase;
using XISystem;

namespace XICore
{
    public class XIInfraFormComponent : XIDefinitionBase
    {

        public string sBOName { get; set; }
        public string sGroupName { get; set; }
        public string sLockGroup { get; set; }
        public string sHiddenGroup { get; set; }
        public string sSessionID { get; set; }
        public string sGUID { get; set; }
        public string sOverrideGroup { get; set; }
        public string sBOActionCode { get; set; }
        public string sBOUpdateAction { get; set; }

        XIInfraCache oCache = new XIInfraCache();
        XIDBAPI Connection = new XIDBAPI(ConfigurationManager.ConnectionStrings["XIDNADbContext"].ConnectionString);

        public CResult XILoad(List<CNV> oParams)
        {
            CResult oCResult = new CResult(); // always
            CResult oCR = new CResult(); // always
            long iTraceLevel = 10;
            string sParGUID = string.Empty;
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
                long iInstanceID = 0; string GroupID = string.Empty; string sInstanceID = string.Empty;
                sSessionID = oParams.Where(m => m.sName == XIConstant.Param_SessionID).Select(m => m.sValue).FirstOrDefault();
                sGUID = oParams.Where(m => m.sName == XIConstant.Param_GUID).Select(m => m.sValue).FirstOrDefault();
                if (!string.IsNullOrEmpty(sGUID))
                {
                    oCache.sSessionID = sSessionID;
                    var ParentGUID = oCache.GetParentGUIDRecurrsive(sGUID);
                    sGUID = ParentGUID;
                }
                XIBODisplay oBODisplay = new XIBODisplay();
                var iDataSourceID = 0;
                //First set all properties by extracting from oParams
                var ActiveBO = string.Empty;
                sGroupName = oParams.Where(m => m.sName == XIConstant.Param_Group).Select(m => m.sValue).FirstOrDefault();
                sLockGroup = oParams.Where(m => m.sName.ToLower() == XIConstant.Param_LockGroup.ToLower()).Select(m => m.sValue).FirstOrDefault();
                sHiddenGroup = oParams.Where(m => m.sName.ToLower() == XIConstant.Param_HiddenGroup.ToLower()).Select(m => m.sValue).FirstOrDefault();
                sBOActionCode = oParams.Where(m => m.sName.ToLower() == XIConstant.Param_BOActionCode.ToLower()).Select(m => m.sValue).FirstOrDefault();
                sBOUpdateAction = oParams.Where(m => m.sName.ToLower() == XIConstant.Param_BOUpdateAction.ToLower()).Select(m => m.sValue).FirstOrDefault();
                var FormMode = oParams.Where(m => m.sName.ToLower() == XIConstant.Param_DisplayMode.ToLower()).Select(m => m.sValue).FirstOrDefault();
                var sCallHierarchy = oParams.Where(m => m.sName == XIConstant.Param_CallHierarchy).Select(m => m.sValue).FirstOrDefault();
                var sCoreDB = oParams.Where(m => m.sName.ToLower() == "sCoreDatabase".ToLower()).Select(m => m.sValue).FirstOrDefault();
                if (oParams.Where(m => m.sName.ToLower() == XIConstant.Param_BO.ToLower()).Select(m => m.sValue).FirstOrDefault() != null)
                {
                    if (!oParams.Where(m => m.sName.ToLower() == XIConstant.Param_BO.ToLower()).Select(m => m.sValue).FirstOrDefault().StartsWith("{XIP|"))
                    {
                        sBOName = oParams.Where(m => m.sName == XIConstant.Param_BO).Select(m => m.sValue).FirstOrDefault();
                    }
                }
                if (oParams.Where(m => m.sName == XIConstant.Param_InstanceID).Select(m => m.sValue).FirstOrDefault() == "-MainDriverID")
                {
                    var ID = oCache.Get_ParamVal(sSessionID, sGUID, null, "-MainDriverID"); //oParams.Where(m => m.sName.ToLower() == Prm.ToLower()).Select(m => m.sValue).FirstOrDefault();
                    oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|Driver_T.id}", ID, null, null);
                    ActiveBO = sBOName;
                }
                var WrapperParms = new List<CNV>();
                var WatchParam = oParams.Where(m => m.sName.ToLower().Contains("watchparam1".ToLower())).ToList();
                if (WatchParam != null && WatchParam.Count() > 0)
                {
                    foreach (var items in WatchParam)
                    {

                        if (!string.IsNullOrEmpty(items.sValue))
                        {
                            var Prams = oCache.Get_Paramobject(sSessionID, sGUID, null, items.sValue); //oParams.Where(m => m.sName == items.sValue).FirstOrDefault();
                            if (Prams != null)
                            {
                                WrapperParms = Prams.nSubParams;
                            }
                        }
                    }
                }
                List<XIVisualisation> Visualisations = new List<XIVisualisation>();
                var IDEParams = oCache.Get_Paramobject(sSessionID, sGUID, null, "IDEParams");
                if (IDEParams != null && IDEParams.nSubParams != null && IDEParams.nSubParams.Count() > 0)
                {
                    var sParentBO = IDEParams.nSubParams.Where(m => m.sName.ToLower() == XIConstant.Param_ParentBO.ToLower()).Select(m => m.sValue).FirstOrDefault();
                    if (!string.IsNullOrEmpty(sParentBO))
                    {
                        int iParentBOIID = 0;
                        var sParentBOIID = IDEParams.nSubParams.Where(m => m.sName.ToLower() == XIConstant.Param_ParentBOIID.ToLower()).Select(m => m.sValue).FirstOrDefault();
                        int.TryParse(sParentBOIID, out iParentBOIID);
                        List<XIVisualisationNV> XiVisualisationNVs = new List<XIVisualisationNV>();
                        if (iParentBOIID > 0)
                        {
                            XiVisualisationNVs.Add(new XIVisualisationNV { sName = "sAskFK", sValue = "No" });
                        }
                        else
                        {
                            XiVisualisationNVs.Add(new XIVisualisationNV { sName = "sAskFK", sValue = "Yes" });
                        }
                        var sParentFKCol = IDEParams.nSubParams.Where(m => m.sName.ToLower() == XIConstant.Param_ParentFKCol.ToLower()).Select(m => m.sValue).FirstOrDefault();
                        var sParentName = IDEParams.nSubParams.Where(m => m.sName.ToLower() == XIConstant.Param_ParentName.ToLower()).Select(m => m.sValue).FirstOrDefault();
                        XiVisualisationNVs.Add(new XIVisualisationNV { sName = "sFKBO", sValue = sParentBO });
                        XiVisualisationNVs.Add(new XIVisualisationNV { sName = "sFKCol", sValue = sParentFKCol });
                        XiVisualisationNVs.Add(new XIVisualisationNV { sName = "sFKVal", sValue = iParentBOIID.ToString() });
                        XiVisualisationNVs.Add(new XIVisualisationNV { sName = "sFKName", sValue = sParentName });
                        Visualisations.Add(new XIVisualisation { Name = "ASKFK", NVs = XiVisualisationNVs });
                    }
                }
                List<CNV> MergeAttrs = new List<CNV>();
                //if (WatchParam != null && WatchParam.Count() > 0)
                //{
                if (WrapperParms != null && WrapperParms.Count() > 0)
                {
                    ActiveBO = WrapperParms.Where(m => m.sName == XIConstant.XIP_ActiveBO).Select(m => m.sValue).FirstOrDefault();// oXIAPI.Get_ParamVal(sSessionID, sGUID, null, "{XIP|ActiveBO}");
                    var XIPBOID = WrapperParms.Where(m => m.sName == XIConstant.Param_InstanceID).Select(m => m.sValue).FirstOrDefault(); // oXIAPI.Get_ParamVal(sSessionID, sGUID, null, Prm); //oParams.Where(m => m.sName.ToLower() == Prm.ToLower()).Select(m => m.sValue).FirstOrDefault();
                    if (!string.IsNullOrEmpty(XIPBOID))
                    {
                        //iInstanceID = Convert.ToInt32(XIPBOID);
                        sInstanceID = XIPBOID;
                    }
                    else
                    {
                        iInstanceID = 0;
                    }
                    if (!(string.IsNullOrEmpty(ActiveBO)))
                    {
                        sBOName = ActiveBO;
                    }
                    MergeAttrs = WrapperParms.Where(m => m.sType.ToLower() == "merge").ToList();
                }
                else
                {
                    if (oParams.Where(m => m.sName.ToLower() == XIConstant.Param_BO.ToLower()).Select(m => m.sValue).FirstOrDefault() != null)
                    {
                        sBOName = oParams.Where(m => m.sName.ToLower() == XIConstant.Param_BO.ToLower()).Select(m => m.sValue).FirstOrDefault();
                    }
                    if (sBOName != null && (sBOName.StartsWith("{XIP|") || sBOName.StartsWith("-") || sBOName.StartsWith("{-")))
                    {
                        sBOName = oCache.Get_ParamVal(sSessionID, sGUID, null, sBOName);
                    }
                    sInstanceID = oParams.Where(m => m.sName == XIConstant.Param_InstanceID).Select(m => m.sValue).FirstOrDefault();
                    if (sInstanceID != null && (sInstanceID.StartsWith("{XIP|") || sInstanceID.StartsWith("-") || sInstanceID.StartsWith("{-")))
                    {
                        sInstanceID = oCache.Get_ParamVal(sSessionID, sGUID, null, sInstanceID);
                    }
                    if (!string.IsNullOrEmpty(sInstanceID))
                    {
                        //var iInsID = oCache.Get_ParamVal(sSessionID, sGUID, null, sInstanceID);
                        if (long.TryParse(sInstanceID, out iInstanceID))
                        {

                        }
                    }
                    GroupID = oCache.Get_ParamVal(sSessionID, sGUID, null, "{XIP|iGroupID}");// oParams.Where(m => m.sName.ToLower() == "{XIP|ActiveBO}".ToLower()).Select(m => m.sValue).FirstOrDefault();
                    var iNannoOrgID = 0;
                    var NannoOrgID = oCache.Get_ParamVal(sSessionID, sGUID, null, "iNannoOrgID");
                    int.TryParse(NannoOrgID, out iNannoOrgID);
                    if (iNannoOrgID > 0)
                    {
                        XIDXI oXID = new XIDXI();
                        oXID.sOrgDatabase = sCoreDB;
                        oCR = oXID.Get_OrgDefinition("", iNannoOrgID.ToString());
                        if (oCR.bOK && oCR.oResult != null)
                        {
                            var oOrgD = (XIDOrganisation)oCR.oResult;
                            if (oOrgD.bNannoApp)
                            {
                                var sDatabaseName = oOrgD.DatabaseName;
                                oCR = oXID.Get_DataSourceDefinition(sDatabaseName);
                                if (oCR.bOK && oCR.oResult != null)
                                {
                                    var DataSource = (XIDataSource)oCR.oResult;
                                    iDataSourceID = DataSource.ID;
                                }
                            }
                        }
                    }
                    //if (string.IsNullOrEmpty(ActiveBO))
                    //{
                    //    ActiveBO = oCache.Get_ParamVal(sSessionID, sGUID, null, "{XIP|ActiveBO}");// oParams.Where(m => m.sName.ToLower() == "{XIP|ActiveBO}".ToLower()).Select(m => m.sValue).FirstOrDefault();
                    //}
                    //var Prm = "{XIP|" + ActiveBO + ".id}";
                    //var XIPBOIID = oCache.Get_ParamVal(sSessionID, sGUID, null, Prm); //oParams.Where(m => m.sName.ToLower() == Prm.ToLower()).Select(m => m.sValue).FirstOrDefault();
                    //if (ActiveBO == null)
                    //{
                    //    Prm = "{XIP|" + sBOName + ".id}";
                    //    XIPBOIID = oCache.Get_ParamVal(sSessionID, sGUID, null, Prm);
                    //}
                    //if (XIPBOIID != null)
                    //{
                    //    if (long.TryParse(XIPBOIID, out iInstanceID))
                    //    {

                    //    }
                    //}
                    //else
                    //{
                    //    iInstanceID = 0;
                    //}
                    //if (!(string.IsNullOrEmpty(ActiveBO)) && sBOName == "{XIP|ActiveBO}")
                    //{
                    //    sBOName = ActiveBO;
                    //}
                    ////string sInstanceID = oParams.Where(m => m.sName == "iInstanceID").Select(m => m.sValue).FirstOrDefault();
                    //if (!string.IsNullOrEmpty(sInstanceID) && iInstanceID == 0)
                    //{
                    //    //var iInsID = oCache.Get_ParamVal(sSessionID, sGUID, null, sInstanceID);
                    //    if (long.TryParse(sInstanceID, out iInstanceID))
                    //    {

                    //    }
                    //}
                }
                //}
                XIDBO oBOD = new XIDBO();
                if (!string.IsNullOrEmpty(sBOName) && !sBOName.StartsWith("{XIP|"))
                {
                    oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Params loaded successfully for Form Component" });
                    XIIXI oXII = new XIIXI();
                    XIIBO oBOI = new XIIBO();
                    XIDXI oXID = new XIDXI();
                    oBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO_All, sBOName);
                    XIDBO oBODCopy = (XIDBO)oBOD.Clone(oBOD);
                    oXII.BOD = oBODCopy;
                    if (iInstanceID == -1)
                    {
                        iInstanceID = 0;
                        sInstanceID = "";
                    }
                    if (!string.IsNullOrEmpty(GroupID))
                    {
                        int iGroupID = Convert.ToInt32(GroupID);
                        iInstanceID = 0;
                        sGroupName = oBODCopy.Groups.Where(x => x.Value.ID == iGroupID).Select(x => x.Value.GroupName).FirstOrDefault();
                    }
                    if (!string.IsNullOrEmpty(oBODCopy.sTraceAttribute))
                    {
                        var TraceAttrs = oBODCopy.sTraceAttribute.Split(',');
                        foreach (var TraceAttr in TraceAttrs)
                        {
                            if ((oBOD.BOID == 717 || (oBOD.Attributes.ContainsKey(TraceAttr) && oBOD.AttributeD(TraceAttr).bIsTrace))/* && !string.IsNullOrEmpty(oBOD.sTraceAttribute)*/)
                            {

                                var sTraceID = string.Empty;
                                var iAttrID = oBOD.AttributeD(TraceAttr).ID;
                                var sAttrName = oBOD.AttributeD(TraceAttr).Name;
                                if (oBOD.BOID == 717)
                                {
                                    var oBOTrace = oXII.BOI(oBOD.Name, iInstanceID.ToString());
                                    if (oBOTrace != null && oBOTrace.Attributes.Count() > 0)
                                    {
                                        sTraceID = oBOTrace.AttributeI("fkivalidtraceid").sValue;
                                    }
                                }
                                else
                                {
                                    List<CNV> oNVs = new List<CNV>();
                                    oXII.iSwitchDataSrcID = oBOD.iDataSource;
                                    oNVs.Add(new CNV { sName = "FKiBOID", sValue = oBOD.BOID.ToString() });
                                    oNVs.Add(new CNV { sName = "FKiAttrID", sValue = iAttrID.ToString() });
                                    oNVs.Add(new CNV { sName = "iInstanceID", sValue = iInstanceID.ToString() });
                                    var oBOTrace = oXII.BOI("TraceTransactions", "", "", oNVs);
                                    //sTraceID = "40";
                                    if (oBOTrace != null && oBOTrace.Attributes.Count() > 0)
                                    {
                                        sTraceID = oBOTrace.AttributeI("fkivalidtraceid").sValue;
                                    }
                                }

                                if (!string.IsNullOrEmpty(sTraceID))
                                {
                                    oXII.iSwitchDataSrcID = oBOD.iDataSource;
                                    var oTraceI = oXII.BOI("refValidTrace_T", sTraceID);
                                    if (oTraceI != null && oTraceI.Attributes.Count() > 0)
                                    {
                                        var bButtons = oTraceI.AttributeI("bShowButtons").sValue;
                                        var sTraceVisibleGroup = oTraceI.AttributeI("sVisibleGroup").sValue;
                                        var sTraceLockGroup = oTraceI.AttributeI("sLockGroup").sValue;
                                        var sTraceSummaryGroup = oTraceI.AttributeI("sSummaryGroup").sValue;
                                        if (!string.IsNullOrEmpty(sTraceVisibleGroup))
                                        {
                                            sGroupName = sTraceVisibleGroup;
                                        }
                                        if (!string.IsNullOrEmpty(sTraceLockGroup))
                                        {
                                            sLockGroup = sTraceLockGroup;
                                        }
                                        if (!string.IsNullOrEmpty(bButtons) && bButtons.ToLower() == "true")
                                        {
                                            var sCurrentTrace = oTraceI.AttributeI("sName").sValue;
                                            var iCount = sCurrentTrace.Count(m => m == '_');
                                            XID1Click o1Click = new XID1Click();
                                            o1Click.BOID = oTraceI.BOD.BOID;
                                            //     XIDXI oXID = new XIDXI();
                                            //oXID.sOrgDatabase = sCoreDB;
                                            var oDataSource = (XIDataSource)oCache.GetObjectFromCache(XIConstant.CacheDataSource, null, oBODCopy.iDataSource.ToString());
                                            o1Click.sSwitchDB = oDataSource.sName;
                                            o1Click.Query = "select * from refValidTrace_T where sname like '" + sCurrentTrace + "%' and FKiBOID=" + oBODCopy.BOID + " and FKiAttrID=" + iAttrID;
                                            var Response = o1Click.OneClick_Run();
                                            if (Response != null && Response.Count() > 0)
                                            {
                                                foreach (var BOI in Response.Values.ToList())
                                                {
                                                    var sNextTrace = BOI.AttributeI("sName").sValue;
                                                    var iNextCount = sNextTrace.Count(m => m == '_');
                                                    if (sNextTrace != sCurrentTrace && iNextCount == (iCount + 1))
                                                    {
                                                        var index = sNextTrace.LastIndexOf("_");
                                                        var Code = sNextTrace.Substring(index, sNextTrace.Length - index);
                                                        if (!string.IsNullOrEmpty(Code))
                                                        {
                                                            List<CNV> oParam = new List<CNV>();
                                                            oParam.Add(new CNV { sName = "sName", sValue = Code.Replace("_", "") });
                                                            oParam.Add(new CNV { sName = "FKiBOID", sValue = oBODCopy.BOID.ToString() });
                                                            oParam.Add(new CNV { sName = "FKiAttrID", sValue = iAttrID.ToString() });
                                                            var oStageI = oXII.BOI("RefTraceStage", null, null, oParam);
                                                            if (oStageI != null && oStageI.Attributes.Count() > 0)
                                                            {
                                                                var Name = oStageI.AttributeI("sDescription").sValue;
                                                                var FKiXILinkID = oStageI.AttributeI("FKiXILinkID").sValue;
                                                                int iXILinkID = 0;
                                                                int.TryParse(FKiXILinkID, out iXILinkID);
                                                                if (Visualisations.Count() == 0)
                                                                {
                                                                    Visualisations.Add(new XIVisualisation() { Name = "tracebuttons" });
                                                                    Visualisations.FirstOrDefault().NVs = new List<XIVisualisationNV>();
                                                                }
                                                                var Vis = Visualisations.FirstOrDefault().NVs.Where(m => m.sName.ToLower() == Name.ToLower()).FirstOrDefault();
                                                                if (Vis == null)
                                                                {
                                                                    Visualisations.FirstOrDefault().NVs.Add(new XIVisualisationNV { sName = Name + "-" + sAttrName, sType = "TraceBtn", sValue = iXILinkID + "-" + oStageI.AttributeI("iStatusValue1").sValue });
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
                        }
                    }
                    if (!string.IsNullOrEmpty(sInstanceID))
                            {
                                //if(!string.IsNullOrEmpty(ActiveFK))
                                //{
                                //    List<CNV> oNVParams = new List<CNV>();
                                //    CNV oCNV = new CNV();
                                //    oCNV.sName = ActiveFK;
                                //    oCNV.sValue = iInstanceID.ToString();
                                //    oNVParams.Add(oCNV);
                                //    oBOI = oXII.BOI(sBOName, null, sGroupName, oNVParams);
                                //}
                                //else
                                //{
                                if (iDataSourceID > 0)
                                {
                                    oXII.iSwitchDataSrcID = iDataSourceID;
                                }
                                oBOI = oXII.BOI(sBOName, sInstanceID, sGroupName);
                                //}
                                if (oBOI != null)
                                {
                                    oBOI.iBODID = oBOI.BOD.BOID;
                                    //oBOI.ResloveFKFields();
                                    //oBOI.FormatAttrs();
                                }
                                var oFileAttrs = oBODCopy.Attributes.Values.Where(m => m.FKiFileTypeID > 0).ToList();
                                List<XIDropDown> ImagePathDetails = new List<XIDropDown>();
                                foreach (var oAttr in oFileAttrs)
                                {
                                    var sName = oAttr.Name;
                                    var sFileID = oBOI.Attributes.Values.Where(m => m.sName.ToLower() == sName.ToLower()).Select(m => m.sValue).FirstOrDefault();
                                    if (!string.IsNullOrEmpty(sFileID))
                                    {
                                        List<XIDropDown> sPDFPathDetails = new List<XIDropDown>();
                                        var NewFileID = sFileID.Split(',').ToList();
                                        foreach (var item in NewFileID)
                                        {
                                            if (!string.IsNullOrEmpty(item.ToString()))
                                            {
                                                XIInfraDocTypes oXIDocTypes = new XIInfraDocTypes();
                                                XIInfraDocs oDocs = new XIInfraDocs();
                                                if (oAttr.FKiFileTypeID == 1)
                                                {
                                                    int iDocID = 0;
                                                    int.TryParse(item, out iDocID);
                                                    if (iDocID > 0)
                                                    {
                                                        oDocs.ID = Convert.ToInt32(item);
                                                        var sImagePathDetails = (List<XIDropDown>)oDocs.Get_FilePathDetails().oResult;
                                                        if (sImagePathDetails != null)
                                                        {
                                                            ImagePathDetails.AddRange(sImagePathDetails);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        oXIDocTypes.ID = oAttr.FKiFileTypeID;
                                                        var oXIDocDetails = (XIInfraDocTypes)oXIDocTypes.Get_FileDocTypes().oResult;
                                                        if (oXIDocDetails != null)
                                                        {
                                                            int pos = item.LastIndexOf("\\") + 1;
                                                            string sFileName = item.Substring(pos, item.Length - pos);
                                                            oXIDocDetails.Path = oXIDocDetails.Path.Replace("~", "");
                                                            sPDFPathDetails.Add(new XIDropDown { Expression = oXIDocDetails.Path + "//" + item, text = sFileName });
                                                            ImagePathDetails.AddRange(sPDFPathDetails);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    int iDocID = 0;
                                                    int.TryParse(item, out iDocID);
                                                    if (iDocID > 0)
                                                    {
                                                        oDocs.ID = Convert.ToInt32(item);
                                                        var sImagePathDetails = (List<XIDropDown>)oDocs.Get_FilePathDetails().oResult;
                                                        if (sImagePathDetails != null)
                                                        {
                                                            ImagePathDetails.AddRange(sImagePathDetails);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        oXIDocTypes.ID = oAttr.FKiFileTypeID;
                                                        var oXIDocDetails = (XIInfraDocTypes)oXIDocTypes.Get_FileDocTypes().oResult;
                                                        if (oXIDocDetails != null)
                                                        {
                                                            int pos = item.LastIndexOf("\\") + 1;
                                                            string sFileName = item.Substring(pos, item.Length - pos);
                                                            oXIDocDetails.Path = oXIDocDetails.Path.Replace("~", "");
                                                            sPDFPathDetails.Add(new XIDropDown { Expression = oXIDocDetails.Path + "//" + item, text = sFileName });
                                                            ImagePathDetails.AddRange(sPDFPathDetails);
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    oBOI.Attributes.Values.Where(x => x.sName.ToLower() == sName.ToLower()).ToList().ForEach(x => x.ImagePathDetails = ImagePathDetails);
                                    //oBOD.Attributes.Values.Where(s => s.Name == sName).Where(s => s.ID == oAttr.ID).Select(s => { s.ImagePathDetails = ImagePathDetails; return s; }).ToList();
                                }
                                oBOI.BOD = oBOD;
                                if (oBODCopy.Name == "XIAlgorithmLines_T" && sGroupName == "Create")
                                {
                                    oBOI.Attributes.Values.Where(x => x.sName != "sIndent" && x.sName != "iOrder").ToList().ForEach(m => m.sValue = "");
                                    if (oParams.Where(m => m.sName == "sOperator").Select(m => m.sValue).FirstOrDefault() == "+")
                                    {
                                        oBOI.Attributes.Values.Where(x => x.sName == "iOrder").ToList().ForEach(m => m.sValue = (Convert.ToInt32(m.sValue) + 1).ToString());
                                    }
                                    else if (oParams.Where(m => m.sName == "sOperator").Select(m => m.sValue).FirstOrDefault() == "-")
                                    {
                                        oBOI.Attributes.Values.Where(x => x.sName == "iOrder").ToList().ForEach(m => m.sValue = (Convert.ToInt32(m.sValue) - 1).ToString());
                                    }
                                }
                                if (oBODCopy.bUID)
                                {
                                    oBOI.SetAttribute("XIGUID", sInstanceID);
                                }
                                oBODisplay.BOInstance = oBOI;
                        var DependencyFieldsList = oBODCopy.Attributes.Values.Where(m => !string.IsNullOrEmpty(m.sDepBOFieldIDs)).ToList();
                        if (DependencyFieldsList != null && DependencyFieldsList.Count > 0)
                        {
                            foreach (var DependecyField in DependencyFieldsList)
                            {
                                if (!string.IsNullOrEmpty(DependecyField.sEventHandler) && DependecyField.sEventHandler.Contains("onload") && oBOI.Attributes.ContainsKey(DependecyField.Name) && !string.IsNullOrEmpty(oBOI.Attributes[DependecyField.Name].sValue))
                                {
                                    var ParentIID = oBOI.Attributes[DependecyField.Name].sValue;
                                    var sDepBOFieldIDs = DependecyField.sDepBOFieldIDs.Split(',');
                                    foreach (var Field in sDepBOFieldIDs)
                                    {
                                        int iFieldID = 0;
                                        if (int.TryParse(Field, out iFieldID))
                                        {
                                            var oFieldD = oBODCopy.Attributes.Values.Where(m => m.ID == iFieldID).FirstOrDefault();
                                            if (oFieldD != null && oBOD.Attributes.ContainsKey(oFieldD.Name) && !string.IsNullOrEmpty(oBOD.Attributes[oFieldD.Name].sFKBOName))
                                            {
                                                var sBOName = oBOD.Attributes[oFieldD.Name].sFKBOName;
                                                Dictionary<string, object> Params = new Dictionary<string, object>();
                                                Params["Name"] = sBOName;
                                                string sSelectFields = string.Empty;
                                                sSelectFields = "Name,BOID,iDataSource,sSize,TableName,sPrimaryKey,sType";
                                                var FKBOD = Connection.Select<XIDBO>("XIBO_T_N", Params, sSelectFields).FirstOrDefault();
                                                //var FKBOD = Load_BO(FKBO.Name, FKBO.BOID);
                                                //var BO = AllBOs.Where(m => m.TableName == sTableName).FirstOrDefault();
                                                //sBODataSource = oXID.GetBODataSource(FKBOD.iDataSource, oBOD.FKiApplicationID);
                                                if (FKBOD.sSize == "10")//maximum number of results in dropdown -- To Do
                                                {
                                                    var oResult = oXID.Get_DependencyAutoCompleteSearchList(Field, sGUID, FKBOD.Name, ParentIID, DependecyField.sFKBOName, 10).oResult;
                                                    var DDL = (List<XIDFieldOptionList>)oResult;
                                                    if (DDL != null)
                                                    {
                                                        List<XIDropDown> FKDDL = new List<XIDropDown>();
                                                        FKDDL = DDL.Select(m => new XIDropDown { text = m.sOptionValue, Expression = m.sOptionName }).ToList();
                                                        if (oBOI.Attributes.ContainsKey(oFieldD.Name.ToLower()))
                                                        {
                                                            oBOI.Attributes[oFieldD.Name.ToLower()].FieldDDL = FKDDL;
                                                            oBOI.BOD.Attributes[oFieldD.Name.ToLower()].FieldDDL = FKDDL;
                                                            oBOI.BOD.Attributes[oFieldD.Name.ToLower()].bIsHidden = false;
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
                            else
                            {
                                //Load bo instance with group
                                oBOI.BOD = oBOD;
                                if (!string.IsNullOrEmpty(sGroupName))
                                {
                                    oBOI.LoadBOI(sGroupName);
                                    oBODisplay.BOInstance = oBOI;
                                    Dictionary<string, XIIAttribute> attributes = new Dictionary<string, XIIAttribute>();
                                    foreach (var item in oBOI.Attributes)
                                    {
                                        if (!string.IsNullOrEmpty(item.Value.sDefaultDate))
                                        {
                                            item.Value.sValue = Utility.GetDefaultDateResolvedValue(item.Value.sDefaultDate, item.Value.Format);
                                        }
                                        attributes.Add(item.Key, item.Value);
                                    }
                                    //var WatchParam2 = oParams.Where(m => m.sName.ToLower().Contains("watchparam2".ToLower())).ToList();
                                    var sGroupOverride = oCache.Get_Paramobject(sSessionID, sGUID, null, "{XIP|sGroupOverride}");
                                    if (!string.IsNullOrEmpty(sGroupOverride.sValue))
                                    {
                                        oBOI.LoadBOI(sGroupOverride.sValue);
                                        oBODisplay.BOInstance = oBOI;
                                        foreach (var item in oBOI.Attributes)
                                        {
                                            if (!string.IsNullOrEmpty(item.Value.sDefaultDate))
                                            {
                                                item.Value.sDefaultDate = Utility.GetDefaultDateResolvedValue(item.Value.sDefaultDate, item.Value.Format);
                                            }
                                            attributes.Add(item.Key, item.Value);
                                        }
                                        oBODisplay.BOInstance.Attributes = attributes;
                                    }
                                    var value = oCache.Get_Paramobject(sSessionID, sGUID, null, "{XIP|sTransCode}");
                                    oBODisplay.BOInstance.Attributes.Values.Where(m => m.sName.ToLower() == "sTransCode".ToLower()).ToList().ForEach(m => m.sValue = value.sValue);
                        }
                        if (!oBODisplay.BOInstance.Attributes.ContainsKey("xiguid") && oBOI.BOD.Attributes.ContainsKey("xiguid"))
                        {
                            var GUIDAttr = oBOI.BOD.AttributeD("xiguid");
                            oBODisplay.BOInstance.Attributes.Add(GUIDAttr.Name, new XIIAttribute { sName = GUIDAttr.Name, Format = GUIDAttr.Format, sDefaultDate = GUIDAttr.sDefaultDate, iOneClickID = GUIDAttr.iOneClickID, sValue = null, bDirty = false });
                        }
                    }
                            var FKAttributes = oBODCopy.Attributes.Where(m => m.Value.FKiType > 0 && !string.IsNullOrEmpty(m.Value.sFKBOName) && m.Value.iOneClickID > 0).ToList();
                            foreach (var item in FKAttributes)
                            {
                                if (item.Value.iOneClickID > 0)
                                {
                                    string sBODataSource = string.Empty;
                                    var sBOName = item.Value.sFKBOName;
                                    Dictionary<string, object> Params = new Dictionary<string, object>();
                                    Params["Name"] = sBOName;
                                    string sSelectFields = string.Empty;
                                    sSelectFields = "Name,BOID,iDataSource,sSize,TableName,sPrimaryKey,sType";
                                    var FKBOD = Connection.Select<XIDBO>("XIBO_T_N", Params, sSelectFields).FirstOrDefault();
                                    //var FKBOD = Load_BO(FKBO.Name, FKBO.BOID);
                                    //var BO = AllBOs.Where(m => m.TableName == sTableName).FirstOrDefault();
                                    sBODataSource = oXID.GetBODataSource(FKBOD.iDataSource, oBODCopy.FKiApplicationID);
                            oBOD.Attributes[item.Value.Name.ToLower()].sFKBOSize = FKBOD.sSize;
                            oBOD.Attributes[item.Value.Name.ToLower()].sFKBOName = FKBOD.Name;
                                    if (FKBOD.sSize == "10")//maximum number of results in dropdown -- To Do
                                    {
                                        var Con = new XIDBAPI(sBODataSource);
                                        if (FKBOD.sType != null && FKBOD.sType.ToLower() == "reference")
                                        {
                                            string suid = "1click-" + Convert.ToString(item.Value.iOneClickID);
                                            XICacheInstance oGUIDParams = oCache.GetAllParamsUnderGUID(sSessionID, sGUID, null);
                                            List<CNV> nParms = new List<CNV>();
                                            nParms = oGUIDParams.NMyInstance.Select(m => new CNV { sName = m.Key, sValue = m.Value.sValue, sType = m.Value.sType }).ToList();
                                            var oResult = oXID.Get_AutoCompleteList(suid, "", nParms);
                                            List<XIDropDown> FKDDL = new List<XIDropDown>();
                                            if (oResult.bOK && oResult.oResult != null)
                                            {
                                                var DDL = (List<XIDFieldOptionList>)oResult.oResult;
                                                FKDDL = DDL.Select(m => new XIDropDown { text = m.sOptionValue, Expression = m.sOptionName }).ToList();
                                            }
                                            if (oBOI.Attributes.ContainsKey(item.Value.Name.ToLower()))
                                            {
                                                oBOI.Attributes[item.Value.Name.ToLower()].FieldDDL = FKDDL;
                                            }
                                        }
                                        else
                                        {
                                            string suid = "1click-" + Convert.ToString(item.Value.iOneClickID);
                                            XICacheInstance oGUIDParams = oCache.GetAllParamsUnderGUID(sSessionID, sGUID, null);
                                            List<CNV> nParms = new List<CNV>();
                                            nParms = oGUIDParams.NMyInstance.Select(m => new CNV { sName = m.Key, sValue = m.Value.sValue, sType = m.Value.sType }).ToList();
                                            var oResult = oXID.Get_AutoCompleteList(suid, "", nParms);
                                            List<XIDropDown> FKDDL = new List<XIDropDown>();
                                            if (oResult.bOK && oResult.oResult != null)
                                            {
                                                var DDL = (List<XIDFieldOptionList>)oResult.oResult;
                                                FKDDL = DDL.Select(m => new XIDropDown { text = m.sOptionValue, Expression = m.sOptionName }).ToList();
                                            }
                                            if (oBOI.Attributes.ContainsKey(item.Value.Name.ToLower()))
                                            {
                                                oBOI.Attributes[item.Value.Name.ToLower()].FieldDDL = FKDDL;
                                            }
                                        }
                                    }
                                }
                            }

                            var FKAttribute = oBODCopy.Attributes.Where(m => m.Value.FKiType > 0 && string.IsNullOrEmpty(m.Value.sFKBOName) && m.Value.iOneClickID > 0).ToList();
                            foreach (var item in FKAttribute)
                            {
                                if (item.Value.iOneClickID > 0)
                                {
                                    string sBODataSource = string.Empty;
                                    var i1ClickID = item.Value.iOneClickID;
                                    XID1Click o1Def = (XID1Click)oCache.GetObjectFromCache(XIConstant.Cache1Click, null, i1ClickID.ToString());
                                    var iBOID = o1Def.BOID;
                                    XIDBO oBODef = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, null, iBOID.ToString());
                                    sBODataSource = oBODef.iDataSource == 0 ? "" : oXID.GetBODataSource(oBODef.iDataSource, oBODef.FKiApplicationID);
                                    oBOD.Attributes[item.Value.Name.ToLower()].sFKBOSize = oBODef.sSize;
                                    oBOD.Attributes[item.Value.Name.ToLower()].sFKBOName = oBODef.Name;
                                    if (oBODef.sSize == "10")//maximum number of results in dropdown -- To Do
                                    {
                                        var Con = new XIDBAPI(sBODataSource);
                                        if (oBODef.sType != null && oBODef.sType.ToLower() == "reference")
                                        {
                                            string suid = "1click-" + Convert.ToString(item.Value.iOneClickID);
                                            XICacheInstance oGUIDParams = oCache.GetAllParamsUnderGUID(sSessionID, sGUID, null);
                                            List<CNV> nParms = new List<CNV>();
                                            nParms = oGUIDParams.NMyInstance.Select(m => new CNV { sName = m.Key, sValue = m.Value.sValue, sType = m.Value.sType }).ToList();
                                            var oResult = oXID.Get_AutoCompleteList(suid, "", nParms);
                                            List<XIDropDown> FKDDL = new List<XIDropDown>();
                                            if (oResult.bOK && oResult.oResult != null)
                                            {
                                                var DDL = (List<XIDFieldOptionList>)oResult.oResult;
                                                FKDDL = DDL.Select(m => new XIDropDown { text = m.sOptionValue, Expression = m.sOptionName }).ToList();
                                            }
                                            if (oBOI.Attributes.ContainsKey(item.Value.Name.ToLower()))
                                            {
                                                oBOI.Attributes[item.Value.Name.ToLower()].FieldDDL = FKDDL;
                                            }
                                        }
                                        else
                                        {
                                            string suid = "1click-" + Convert.ToString(item.Value.iOneClickID);
                                            XICacheInstance oGUIDParams = oCache.GetAllParamsUnderGUID(sSessionID, sGUID, null);
                                            List<CNV> nParms = new List<CNV>();
                                            nParms = oGUIDParams.NMyInstance.Select(m => new CNV { sName = m.Key, sValue = m.Value.sValue, sType = m.Value.sType }).ToList();
                                            var oResult = oXID.Get_AutoCompleteList(suid, "", nParms);
                                            List<XIDropDown> FKDDL = new List<XIDropDown>();
                                            if (oResult.bOK && oResult.oResult != null)
                                            {
                                                var DDL = (List<XIDFieldOptionList>)oResult.oResult;
                                                FKDDL = DDL.Select(m => new XIDropDown { text = m.sOptionValue, Expression = m.sOptionName }).ToList();
                                            }
                                            if (oBOI.Attributes.ContainsKey(item.Value.Name.ToLower()))
                                            {
                                                oBOI.Attributes[item.Value.Name.ToLower()].FieldDDL = FKDDL;
                                            }
                                        }
                                    }
                                }
                            }
                            oBOI.DependentFields = new List<string>();
                            var MergeFields = oBODCopy.Attributes.Values.Where(m => m.iMergeFieldID > 0).ToList();
                            var DependentFields = oBODCopy.Attributes.Values.Where(m => m.iDependentFieldID > 0).ToList();
                            if (MergeFields != null && MergeFields.Count() > 0)
                            {
                                foreach (var item in MergeFields)
                                {
                                    var oMergeBOD = oBODCopy.Attributes.Values.Where(m => m.ID == item.iMergeFieldID).FirstOrDefault();
                                    if (oBOI.Attributes.Values.Select(m => m.sName).ToList().Contains(oMergeBOD.Name))
                                    {
                                        oBOI.DependentFields.Add(oMergeBOD.Name);
                                    }
                                }
                            }
                            if (DependentFields != null && DependentFields.Count() > 0)
                            {
                                foreach (var item in DependentFields)
                                {
                                    var oDependentBOD = oBODCopy.Attributes.Values.Where(m => m.ID == item.iDependentFieldID).FirstOrDefault();
                                    if (oBOI.Attributes.Values.Select(m => m.sName).ToList().Contains(oDependentBOD.Name))
                                    {
                                        oBOI.DependentFields.Add(oDependentBOD.Name);
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(sGroupName))
                            {
                                var GroupFields = oBOI.BOD.GroupD(sGroupName).BOFieldNames;//oXIAPI.GetBOGroupFields(sBOName, sLockGroup, iUserID, sDatabase);
                                var oGrpD = oBOI.BOD.GroupD(sGroupName);
                                var oAllGroupFields = Utility.GetBOGroupFields(GroupFields, oGrpD.bIsCrtdBy, oGrpD.bIsCrtdWhn, oGrpD.bIsUpdtdBy, oGrpD.bIsUpdtdWhn);
                                if (!string.IsNullOrEmpty(oAllGroupFields))
                                {
                                    var oGrpFields = oAllGroupFields.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList().ConvertAll(m => m.ToLower());
                                    oBODisplay.BOInstance.Attributes.Values.Where(m => oGrpFields.Any(n => n == m.sName.ToLower())).ToList().ForEach(m => m.bDirty = true);
                                }
                            }
                            else if (oBOI.Attributes.Values.Count() > 0)
                            {
                                oBODisplay.BOInstance.Attributes.Values.ToList().ForEach(m => m.bDirty = true);
                            }
                            //oBODisplay = oXIAPI.GetFormData(sBOName, sGroupName, iInstanceID, string.Empty, iUserID, sOrgName, sDatabase, null);

                            List<XIVisualisation> oXIVisualisations = new List<XIVisualisation>();
                            string DisplayMode = string.Empty;
                            string sVisualisation = oParams.Where(m => m.sName.ToLower() == "Visualisation".ToLower()).Select(m => m.sValue).FirstOrDefault();
                            if (!string.IsNullOrEmpty(sVisualisation) && sVisualisation != "0")
                            {
                                int sVisualisationID = 0;
                                if (int.TryParse(sVisualisation, out sVisualisationID))
                                {
                                    if (sVisualisationID != 0)
                                    {
                                        sVisualisation = "";
                                    }
                                }
                                var oXIvisual = (XIVisualisation)oCache.GetObjectFromCache(XIConstant.CacheVisualisation, sVisualisation, sVisualisationID.ToString());
                                var oXIDVisual = (XIVisualisation)oXIvisual.GetCopy();
                                if (oXIDVisual.XiVisualisationNVs.ToList().Where(m => m.sName.ToLower() == "fkipolicyid".ToLower()).Select(m => m.sValue).FirstOrDefault() != null)
                                {
                                    var sPolicyVisualValue = oXIDVisual.XiVisualisationNVs.ToList().Where(m => m.sName.ToLower() == "fkipolicyid".ToLower()).Select(m => m.sValue).FirstOrDefault();
                                    var iACPolicyID = oCache.Get_ParamVal(sSessionID, sGUID, null, sPolicyVisualValue);
                                    var oPolDef = oXII.BOI("ACPolicy_T", iACPolicyID.ToString());
                                    if (!string.IsNullOrEmpty(oPolDef.Attributes["sPolicyNo"].sValue))
                                    {
                                        oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|sPolicyNo}", oPolDef.Attributes["sPolicyNo"].sValue, null, null);
                                    }
                                }
                                if (oXIDVisual != null)
                                {

                                    foreach (var oVisualisation in oXIDVisual.XiVisualisationNVs)
                                    {
                                        if (oVisualisation.sName.ToLower() == "displaymode")
                                        {
                                            DisplayMode = oVisualisation.sValue;
                                        }
                                        if (oVisualisation.sName != null && oVisualisation.sName.ToLower() == "LockGroup".ToLower() && string.IsNullOrEmpty(sLockGroup))
                                        {
                                            sLockGroup = oVisualisation.sValue;
                                        }
                                        if (oVisualisation.sName != null && oVisualisation.sName.ToLower() == "HiddenGroup".ToLower() && string.IsNullOrEmpty(sHiddenGroup))
                                        {
                                            sHiddenGroup = oVisualisation.sValue;
                                        }
                                        if (oVisualisation.sValue.StartsWith("xi."))
                                        {
                                            XIDScript oXIScript = new XIDScript();
                                            oXIScript.sScript = oVisualisation.sValue.ToString();
                                            oCR = oXIScript.Execute_Script(sGUID, sSessionID);
                                            if (oCR.bOK && oCR.oResult != null)
                                            {
                                                oVisualisation.sValue = (string)oCR.oResult;
                                            }
                                        }
                                        else if (oVisualisation.sValue.Contains("xi.s") || oVisualisation.sValue.Contains("xi.r"))
                                        {
                                            XIDScript oXIScript = new XIDScript();
                                            oXIScript.sScript = oVisualisation.sValue.ToString();
                                            oCR = oXIScript.Execute_Script(sGUID, sSessionID);
                                            if (oCR.bOK && oCR.oResult != null)
                                            {
                                                oVisualisation.sValue = (string)oCR.oResult;
                                            }
                                        }
                                        if (oVisualisation.sValue != null && oVisualisation.sValue.IndexOf("{XIP") >= 0)
                                        {
                                            var oVisualTypeID = oBODCopy.Attributes.Values.ToList().Where(m => m.Name.ToLower() == oVisualisation.sName.ToLower()).Select(m => m.TypeID).FirstOrDefault();
                                            if (oVisualTypeID == 150)
                                            {
                                                var sPreviousVisualValue = oVisualisation.sValue;
                                                var oVisualSplit = oVisualisation.sValue.Contains("+") ? oVisualisation.sValue.Split(new string[] { " + " }, StringSplitOptions.RemoveEmptyEntries).ToList() : oVisualisation.sValue.Contains("-") ? oVisualisation.sValue.Split(new string[] { " - " }, StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>();
                                                oVisualisation.sValue = oVisualSplit[0];
                                                oVisualisation.sValue = oCache.Get_ParamVal(sSessionID, sGUID, null, oVisualisation.sValue);
                                                oVisualisation.sValue = sPreviousVisualValue.Replace(sPreviousVisualValue.Split('+').FirstOrDefault(), oVisualisation.sValue);
                                                oVisualisation.sValue = Utility.GetDateResolvedValue(oVisualisation.sValue, XIConstant.Date_Format);
                                                if (oBODisplay.BOInstance.Attributes.ContainsKey(oVisualisation.sName.ToLower()) || string.IsNullOrEmpty(oVisualisation.sValue))
                                                {
                                                    oBODisplay.BOInstance.Attributes[oVisualisation.sName.ToLower()] = new XIIAttribute { sName = oVisualisation.sName, sValue = oVisualisation.sValue, sPreviousValue = oVisualisation.sValue, bDirty = true };
                                                }
                                            }
                                            else
                                            {
                                                oVisualisation.sValue = oCache.Get_ParamVal(sSessionID, sGUID, null, oVisualisation.sValue);
                                                if (oBODisplay.BOInstance.Attributes.ContainsKey(oVisualisation.sName) && string.IsNullOrEmpty(oBODisplay.BOInstance.AttributeI(oVisualisation.sName).sValue))
                                                {
                                                    oBODisplay.BOInstance.Attributes[oVisualisation.sName] = new XIIAttribute { sName = oVisualisation.sName, sValue = oVisualisation.sValue, sPreviousValue = oVisualisation.sValue, bDirty = true };
                                                }
                                            }
                                            //if (oVisualisation.sValue.Contains("+") || oVisualisation.sValue.Contains("-"))
                                            //{
                                            //    var sPreviousVisualValue = oVisualisation.sValue;
                                            //    var oVisualSplit = oVisualisation.sValue.Contains("+") ? oVisualisation.sValue.Split(new string[] { " + " }, StringSplitOptions.RemoveEmptyEntries).ToList() : oVisualisation.sValue.Contains("-") ? oVisualisation.sValue.Split(new string[] { " - " }, StringSplitOptions.RemoveEmptyEntries).ToList() : new List<string>();
                                            //    oVisualisation.sValue = oVisualSplit[0];
                                            //    int iAddDays = 0;
                                            //    if (oVisualSplit.Count() > 1)
                                            //    {
                                            //        iAddDays = Convert.ToInt32(oVisualSplit[1]);
                                            //    }
                                            //    string sFormat = "yyyy-MM-dd";
                                            //    oVisualisation.sValue = oCache.Get_ParamVal(sSessionID, sGUID, null, oVisualisation.sValue);
                                            //    DateTime oVisualDate = Convert.ToDateTime(oVisualisation.sValue);
                                            //    if (sPreviousVisualValue.Contains("+"))
                                            //    {
                                            //        oVisualisation.sValue = oVisualDate.AddDays(iAddDays).Date.ToString(sFormat);
                                            //    }
                                            //    else
                                            //    {
                                            //        oVisualisation.sValue = oVisualDate.AddDays(-iAddDays).Date.ToString(sFormat);
                                            //    }
                                            //    if (oBODisplay.BOInstance.Attributes.ContainsKey(oVisualisation.sName) && string.IsNullOrEmpty(oBODisplay.BOInstance.AttributeI(oVisualisation.sName).sValue))
                                            //    {
                                            //        oBODisplay.BOInstance.Attributes[oVisualisation.sName] = new XIIAttribute { sName = oVisualisation.sName, sValue = oVisualisation.sValue, sPreviousValue = oVisualisation.sValue, bDirty = true };
                                            //    }
                                            //}
                                            //else
                                            //{
                                            //    oVisualisation.sValue = oCache.Get_ParamVal(sSessionID, sGUID, null, oVisualisation.sValue);
                                            //    if (oBODisplay.BOInstance.Attributes.ContainsKey(oVisualisation.sName) && string.IsNullOrEmpty(oBODisplay.BOInstance.AttributeI(oVisualisation.sName).sValue))
                                            //    {
                                            //        oBODisplay.BOInstance.Attributes[oVisualisation.sName] = new XIIAttribute { sName = oVisualisation.sName, sValue = oVisualisation.sValue, sPreviousValue = oVisualisation.sValue, bDirty = true };
                                            //    }
                                            //}
                                        }
                                        else
                                        {
                                            if (oVisualisation.sName.ToLower() == XIConstant.Key_XICrtdWhn.ToLower() || oVisualisation.sName.ToLower() == XIConstant.Key_XIUpdtdWhn.ToLower())
                                            {
                                                if (!string.IsNullOrEmpty(oBODisplay.BOInstance.AttributeI(oVisualisation.sName).sValue))
                                                {
                                                    string sValue = oBODisplay.BOInstance.AttributeI(oVisualisation.sName).sValue;
                                                    string sFormat = XIConstant.DateTime_Format; //"dd-MMM-yyyy HH:mm";
                                                    string sFormattedValue = String.Format("{0:" + sFormat + "}", Convert.ToDateTime(sValue));
                                                    oBODisplay.BOInstance.AttributeI(oVisualisation.sName).sValue = sFormattedValue;
                                                    oBODisplay.BOInstance.AttributeI(oVisualisation.sName).sPreviousValue = sFormattedValue;
                                                }
                                                oVisualisation.sValue = Utility.GetDateResolvedValue(oVisualisation.sValue, XIConstant.DateTime_Format);
                                            }
                                            else
                                            {
                                                var oVisualTypeID = oBODCopy.Attributes.Values.ToList().Where(m => m.Name.ToLower() == oVisualisation.sName.ToLower()).Select(m => m.TypeID).FirstOrDefault();
                                                if (oVisualTypeID == 150)
                                                {
                                                    oVisualisation.sValue = Utility.GetDateResolvedValue(oVisualisation.sValue, XIConstant.Date_Format); //"dd-MMM-yyyy"
                                                    if (oBODisplay.BOInstance.Attributes.ContainsKey(oVisualisation.sName.ToLower()) || string.IsNullOrEmpty(oVisualisation.sValue))
                                                    {
                                                        oBODisplay.BOInstance.Attributes[oVisualisation.sName.ToLower()] = new XIIAttribute { sName = oVisualisation.sName, sValue = oVisualisation.sValue, sPreviousValue = oVisualisation.sValue, bDirty = true };
                                                    }
                                                    if (!string.IsNullOrEmpty(oBODisplay.BOInstance.AttributeI(oVisualisation.sName).sValue))
                                                    {
                                                        string sValue = oBODisplay.BOInstance.AttributeI(oVisualisation.sName).sValue;
                                                        string sFormat = XIConstant.Date_Format;
                                                        string sFormattedValue = String.Format("{0:" + sFormat + "}", Convert.ToDateTime(sValue));
                                                    }
                                                }
                                            }
                                            if (oBODisplay.BOInstance.Attributes.ContainsKey(oVisualisation.sName) && string.IsNullOrEmpty(oBODisplay.BOInstance.AttributeI(oVisualisation.sName).sValue))
                                            {
                                                oBODisplay.BOInstance.Attributes[oVisualisation.sName] = new XIIAttribute { sName = oVisualisation.sName, sValue = oVisualisation.sValue, sPreviousValue = oVisualisation.sValue, bDirty = true };
                                            }
                                        }
                                        if (oVisualisation.sName != null && oVisualisation.sName.ToLower() == XIConstant.Param_OverrideGroup.ToLower())
                                        {
                                            sOverrideGroup = oVisualisation.sValue;
                                        }
                                        if (!string.IsNullOrEmpty(sOverrideGroup))
                                        {
                                            var oGroupD = oBOD.GroupD(sOverrideGroup);
                                            if (oGroupD != null && !string.IsNullOrEmpty(oGroupD.BOFieldNames))
                                            {
                                                var oAllGroupFields = Utility.GetBOGroupFields(oGroupD.BOFieldNames, oGroupD.bIsCrtdBy, oGroupD.bIsCrtdWhn, oGroupD.bIsUpdtdBy, oGroupD.bIsUpdtdWhn);
                                                var GrpFields = oAllGroupFields.Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                                                foreach (var items in GrpFields)
                                                {
                                                    if (!oBODisplay.BOInstance.Attributes.ContainsKey(items))
                                                    {
                                                        oBODisplay.BOInstance.Attributes[items] = new XIIAttribute() { sName = items, bDirty = true };
                                                    }
                                                }
                                            }
                                        }
                                    }
                                    foreach (var oVisualisationList in oXIDVisual.XiVisualisationLists)
                                    {
                                        if (!string.IsNullOrEmpty(oVisualisationList.ListName) && oVisualisationList.ListName.ToLower() == "adautosave")
                                        {
                                            var oNVs = oXIDVisual.XiVisualisationNVs.Where(m => m.XiVisualListID == oVisualisationList.XiVisualListID).ToList();
                                            foreach (var oNV in oNVs)
                                            {
                                                oBODisplay.BOInstance.Attributes[oNV.sName] = new XIIAttribute { sName = oNV.sName, sValue = oNV.sValue, sPreviousValue = oNV.sValue, bDirty = true, bIsHidden = true };
                                            }
                                        }
                                    }
                                    oXIVisualisations.Add(oXIDVisual);
                                }
                            }
                            if (!string.IsNullOrEmpty(sLockGroup))
                            {
                                string sPrimaryKey = string.Empty;
                                sPrimaryKey = oBOI.BOD.sPrimaryKey;
                                var GroupFields = oBOI.BOD.GroupD(sLockGroup).BOFieldNames;//oXIAPI.GetBOGroupFields(sBOName, sLockGroup, iUserID, sDatabase);
                                var oGroupD = oBOI.BOD.GroupD(sLockGroup);
                                var oAllGroupFields = Utility.GetBOGroupFields(GroupFields, oGroupD.bIsCrtdBy, oGroupD.bIsCrtdWhn, oGroupD.bIsUpdtdBy, oGroupD.bIsUpdtdWhn);
                                if (!string.IsNullOrEmpty(oAllGroupFields))
                                {
                                    var oGrpFields = oAllGroupFields.ToLower().Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                                    oBODisplay.BOInstance.Attributes.Values.Where(m => oGrpFields.Any(n => n == m.sName.ToLower())).ToList().ForEach(c => c.bLock = true);
                                    oBODisplay.BOInstance.Attributes.Values.Where(m => m.sName == sPrimaryKey).ToList().ForEach(m => m.bLock = true);
                                }
                            }
                            if (!string.IsNullOrEmpty(sHiddenGroup))
                            {
                                string sPrimaryKey = string.Empty;
                                sPrimaryKey = oBOI.BOD.sPrimaryKey;
                                var GroupFields = oBOI.BOD.GroupD(sHiddenGroup).BOFieldNames;//oXIAPI.GetBOGroupFields(sBOName, sLockGroup, iUserID, sDatabase);
                                var oGroupD = oBOI.BOD.GroupD(sHiddenGroup);
                                var oAllGroupFields = Utility.GetBOGroupFields(GroupFields, oGroupD.bIsCrtdBy, oGroupD.bIsCrtdWhn, oGroupD.bIsUpdtdBy, oGroupD.bIsUpdtdWhn);
                                if (!string.IsNullOrEmpty(oAllGroupFields))
                                {
                                    var oGrpFields = oAllGroupFields.ToLower().Split(new string[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                                    oBODisplay.BOInstance.Attributes.Values.Where(m => oGrpFields.Any(n => n == m.sName.ToLower())).ToList().ForEach(c => c.bIsHidden = true);
                                }
                            }
                            string OverrideAttribute = oParams.Where(m => m.sName.ToLower() == "OverrideAttribute".ToLower()).Select(m => m.sValue).FirstOrDefault();
                            if (!string.IsNullOrEmpty(OverrideAttribute))
                            {
                                var OverrideFields = OverrideAttribute.Split('_');
                                if (OverrideFields.Count() > 1)
                                {
                                    var Attribute = oBOI.BOD.Attributes.Where(x => x.Key == OverrideFields[1].ToLower()).Select(t => t.Value).FirstOrDefault();
                                    var iACPolicyID = oCache.Get_ParamVal(sSessionID, sGUID, null, "{XIP|" + OverrideFields[1] + "}");
                                    var OverrideValue = oXII.BOI(Attribute.sFKBOName, iACPolicyID, OverrideFields[2]);
                                    oBOI.Attributes.Where(x => x.Key == OverrideFields[0]).ToList().ForEach(x => x.Value.sValue = OverrideValue.AttributeI(OverrideFields[2]).sValue);
                                }
                            }
                            var HiddenAttrs = oBOI.BOD.Attributes.Values.Where(m => m.bIsHidden == true).ToList();
                            if (HiddenAttrs != null && HiddenAttrs.Count() > 0)
                            {
                                foreach (var Attr in HiddenAttrs)
                                {
                                    if (oBODisplay.BOInstance.Attributes.ContainsKey(Attr.Name.ToLower()))
                                    {
                                        oBODisplay.BOInstance.Attributes[Attr.Name.ToLower()].bIsHidden = true;
                                    }
                                }
                            }
                            if (MergeAttrs != null && MergeAttrs.Count() > 0)
                            {

                                foreach (var Attr in MergeAttrs)
                                {
                                    if (oBODisplay.BOInstance.Attributes.ContainsKey(Attr.sName))
                                    {
                                        oBODisplay.BOInstance.Attributes[Attr.sName].sValue = Attr.sValue;
                                        oBODisplay.BOInstance.Attributes[Attr.sName].bIsHidden = true;
                                    }
                                    else
                                    {
                                        oBODisplay.BOInstance.Attributes.Add(Attr.sName, new XIIAttribute() { sName = Attr.sName, sValue = Attr.sValue, bIsHidden = true, bDirty = true });
                                    }
                                }
                            }
                            if (!string.IsNullOrEmpty(sBOActionCode))
                            {
                                Dictionary<string, object> NVs = new Dictionary<string, object>();
                                NVs["FKiBOID"] = oBODCopy.BOID;
                                var Actions = Connection.Select<XIDBOAction>("XIBOAction_T", NVs).ToList();
                                if (Actions != null && Actions.Count() > 0)
                                {
                                    var MatchedActions = Actions.Where(m => m.sCode.ToLower().Contains(sBOActionCode.ToLower())).ToList();
                                    oBODisplay.BOInstance.Actions = MatchedActions;
                                }
                            }
                            if (!string.IsNullOrEmpty(sBOUpdateAction))
                            {
                                Visualisations = null;
                                Visualisations = Visualisations ?? new List<XIVisualisation>();
                                if (Visualisations.Count() == 0)
                                {
                                    Visualisations.Add(new XIVisualisation() { Name = "updateaction", NVs = new List<XIVisualisationNV>() });
                                }
                                Visualisations.FirstOrDefault().NVs.Add(new XIVisualisationNV { sName = "sUpdateAction", sValue = sBOUpdateAction });
                            }
                            if (!string.IsNullOrEmpty(FormMode) && FormMode.ToLower() == "inline")
                            {
                                oBODisplay.BOInstance.Actions = oBODisplay.BOInstance.Actions ?? new List<XIDBOAction>();
                                var ID = oCache.Get_ParamVal(sSessionID, sGUID, null, "1ClickID");
                                if (!string.IsNullOrEmpty(ID))
                                {
                                    var o1ClickD = (XID1Click)oCache.GetObjectFromCache(XIConstant.Cache1Click, null, ID);
                                    if (o1ClickD.Actions != null && o1ClickD.Actions.Count() > 0)
                                    {
                                        var InlineActions = o1ClickD.Actions.Where(m => m.iType == 20).ToList();
                                        if (InlineActions != null && InlineActions.Count() > 0)
                                        {
                                            foreach (var action in InlineActions)
                                            {
                                                var oActionD = (XIDBOAction)oCache.GetObjectFromCache(XIConstant.CacheBOAction, null, action.FKiActionID.ToString());
                                                oBODisplay.BOInstance.Actions.Add(oActionD);
                                            }
                                        }
                                    }

                                }
                            }
                            oBODisplay.BOInstance.iBODID = oBODCopy.BOID;
                            oBODisplay.BOInstance.BOD = null;
                        }
                else
                {
                            oCR.sMessage = "Config Error: XIInfraFormComponent_XILoad() : BO Name is not passed as parameter - Call Hierarchy: " + sCallHierarchy;
                            oCR.sCode = "Config Error";
                            SaveErrortoDB(oCR);
                        }

                        oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
                        oBODisplay.BOInstance.sHierarchy = oParams.Where(m => m.sName == XIConstant.Param_Hierarchy).Select(m => m.sValue).FirstOrDefault();
                        oBODisplay.Visualisations = Visualisations;
                        oCResult.oResult = oBODisplay;
                    }
            catch (Exception ex)
            {
                oCResult.oTraceStack.Add(new CNV { sName = "Stage", sValue = "Error While Executing Form Component" });
                oCResult.sMessage = "ERROR: [" + oCResult.Get_Class() + "." + System.Reflection.MethodBase.GetCurrentMethod().Name + "] - " + ex.Message + " - Trace: " + ex.StackTrace + "\r\n";
                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiError;
                oCResult.LogToFile();
                SaveErrortoDB(oCResult);
            }
            return oCResult;
        }
    }
}