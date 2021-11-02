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
    public class XIInfraMultiRowComponent : XIDefinitionBase
    {

        public string sBOName { get; set; }
        public string sGroupName { get; set; }
        public string sLockGroup { get; set; }
        public string sHiddenGroup { get; set; }
        public string sSessionID { get; set; }
        public string sGUID { get; set; }
        public string sOverrideGroup { get; set; }

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
                long iInstanceID = 0; string GroupID = string.Empty;
                sSessionID = oParams.Where(m => m.sName == XIConstant.Param_SessionID).Select(m => m.sValue).FirstOrDefault();
                sGUID = oParams.Where(m => m.sName == XIConstant.Param_GUID).Select(m => m.sValue).FirstOrDefault();
                if (!string.IsNullOrEmpty(sGUID))
                {
                    oCache.sSessionID = sSessionID;
                    var ParentGUID = oCache.GetParentGUIDRecurrsive(sGUID);
                    sGUID = ParentGUID;
                }
                XIBODisplay oBODisplay = new XIBODisplay();
                //First set all properties by extracting from oParams
                var ActiveBO = string.Empty;
                sGroupName = oParams.Where(m => m.sName == XIConstant.Param_Group).Select(m => m.sValue).FirstOrDefault();
                sLockGroup = oParams.Where(m => m.sName.ToLower() == XIConstant.Param_LockGroup.ToLower()).Select(m => m.sValue).FirstOrDefault();
                sHiddenGroup = oParams.Where(m => m.sName.ToLower() == XIConstant.Param_HiddenGroup.ToLower()).Select(m => m.sValue).FirstOrDefault();
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
                //if (WatchParam != null && WatchParam.Count() > 0)
                //{
                if (WrapperParms != null && WrapperParms.Count() > 0)
                {
                    ActiveBO = WrapperParms.Where(m => m.sName == XIConstant.XIP_ActiveBO).Select(m => m.sValue).FirstOrDefault();// oXIAPI.Get_ParamVal(sSessionID, sGUID, null, "{XIP|ActiveBO}");
                    var XIPBOID = WrapperParms.Where(m => m.sName == XIConstant.Param_InstanceID).Select(m => m.sValue).FirstOrDefault(); // oXIAPI.Get_ParamVal(sSessionID, sGUID, null, Prm); //oParams.Where(m => m.sName.ToLower() == Prm.ToLower()).Select(m => m.sValue).FirstOrDefault();
                    if (XIPBOID != null)
                    {
                        iInstanceID = Convert.ToInt32(XIPBOID);
                    }
                    else
                    {
                        iInstanceID = 0;
                    }
                    if (!(string.IsNullOrEmpty(ActiveBO)))
                    {
                        sBOName = ActiveBO;
                    }
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
                    string sInstanceID = oParams.Where(m => m.sName == XIConstant.Param_InstanceID).Select(m => m.sValue).FirstOrDefault();
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
                    oXII.BOD = oBOD;
                    if (iInstanceID == -1)
                    {
                        iInstanceID = 0;
                    }
                    if (!string.IsNullOrEmpty(GroupID))
                    {
                        int iGroupID = Convert.ToInt32(GroupID);
                        iInstanceID = 0;
                        sGroupName = oBOD.Groups.Where(x => x.Value.ID == iGroupID).Select(x => x.Value.GroupName).FirstOrDefault();
                    }
                    if (!string.IsNullOrEmpty(iInstanceID.ToString()) && iInstanceID != 0)
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
                        oBOI = oXII.BOI(sBOName, iInstanceID.ToString(), sGroupName);
                        //}
                        if (oBOI != null)
                        {
                            oBOI.iBODID = oBOI.BOD.BOID;
                            //oBOI.ResloveFKFields();
                            //oBOI.FormatAttrs();
                        }
                        var oFileAttrs = oBOD.Attributes.Values.Where(m => m.FKiFileTypeID > 0).ToList();
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
                        oBODisplay.BOInstance = oBOI;
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
                    }
                    string OverrideAttribute = oParams.Where(m => m.sName.ToLower() == "OverrideAttribute".ToLower()).Select(m => m.sValue).FirstOrDefault();
                    if (!string.IsNullOrEmpty(OverrideAttribute))
                    {
                        var OverrideFields = OverrideAttribute.Split('_');
                        if (OverrideFields.Count() > 1)
                        {
                            var iACPolicyID = oCache.Get_ParamVal(sSessionID, sGUID, null, "{XIP|" + OverrideFields[1] + "}");
                            var OverrideValue = oCache.Get_ParamVal(sSessionID, sGUID, null, "{XIP|" + OverrideFields[0] + "}");
                            if(OverrideValue!=null)
                            {
                                oCache.Set_ParamVal(sSessionID, sGUID, null, "{XIP|" + OverrideFields[1] + "}", OverrideValue.ToString(),null,null);
                            }
                            // oGUIDParams.NMyInstance.Where(x => x.Key == OverrideFields[0]).ToList().ForEach(x => x.Value.sValue = OverrideValue(OverrideFields[2]).sValue);
                        }
                        XICacheInstance oGUIDParams = oCache.GetAllParamsUnderGUID(sSessionID, sGUID, null);

                    }
                    var FKAttributes = oBOD.Attributes.Where(m => m.Value.FKiType > 0 && !string.IsNullOrEmpty(m.Value.sFKBOName) && m.Value.iOneClickID > 0).ToList();
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
                            sBODataSource = oXID.GetBODataSource(FKBOD.iDataSource,FKBOD.FKiApplicationID);
                            oBOD.Attributes[item.Value.Name.ToLower()].sFKBOSize = FKBOD.sSize;
                            oBOD.Attributes[item.Value.Name.ToLower()].sFKBOName = FKBOD.Name;
                            if (FKBOD.sSize == "10")//maximum number of results in dropdown -- To Do
                            {
                                var Con = new XIDBAPI(sBODataSource);
                                if (FKBOD.sType != null && (FKBOD.sType.ToLower() == "reference" || FKBOD.sType.ToLower() == "XISystem".ToLower()))
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
                    var MergeFields = oBOD.Attributes.Values.Where(m => m.iMergeFieldID > 0).ToList();
                    var DependentFields = oBOD.Attributes.Values.Where(m => m.iDependentFieldID > 0).ToList();
                    if (MergeFields != null && MergeFields.Count() > 0)
                    {
                        foreach (var item in MergeFields)
                        {
                            var oMergeBOD = oBOD.Attributes.Values.Where(m => m.ID == item.iMergeFieldID).FirstOrDefault();
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
                            var oDependentBOD = oBOD.Attributes.Values.Where(m => m.ID == item.iDependentFieldID).FirstOrDefault();
                            if (oBOI.Attributes.Values.Select(m => m.sName).ToList().Contains(oDependentBOD.Name))
                            {
                                oBOI.DependentFields.Add(oDependentBOD.Name);
                            }
                        }
                    }
                    if (!string.IsNullOrEmpty(sGroupName))
                    {
                        var GroupFields = oBOI.BOD.GroupD(sGroupName.ToLower()).BOFieldNames;//oXIAPI.GetBOGroupFields(sBOName, sLockGroup, iUserID, sDatabase);
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
                                    var oVisualTypeID = oBOD.Attributes.Values.ToList().Where(m => m.Name.ToLower() == oVisualisation.sName.ToLower()).Select(m => m.TypeID).FirstOrDefault();
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
                                        var oVisualTypeID = oBOD.Attributes.Values.ToList().Where(m => m.Name.ToLower() == oVisualisation.sName.ToLower()).Select(m => m.TypeID).FirstOrDefault();
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
                                        oBODisplay.BOInstance.Attributes[oNV.sName] = new XIIAttribute { sName = oNV.sName, sValue = oNV.sValue, bDirty = true, bIsHidden = true };
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
                    string sDisplayMode = oParams.Where(m => m.sName.ToLower() == "DisplayMode".ToLower()).Select(m => m.sValue).FirstOrDefault();
                    if (!string.IsNullOrEmpty(sDisplayMode))
                    {
                        var OverrideFields = sDisplayMode.Split('_');
                        var Attribute = oBOI.BOD.Attributes.Where(x => x.Key == OverrideFields[1].ToLower()).Select(t => t.Value).FirstOrDefault();
                        var iACPolicyID = oCache.Get_ParamVal(sSessionID, sGUID, null, "{XIP|" + OverrideFields[1] + "}");
                        var OverrideValue = oXII.BOI(Attribute.sFKBOName, iACPolicyID, OverrideFields[2]);
                        oBOI.Attributes.Where(x => x.Key == OverrideFields[0]).ToList().ForEach(x => x.Value.sValue = OverrideValue.AttributeI(OverrideFields[2]).sValue);
                    }
                    oBODisplay.BOInstance.iBODID = oBOD.BOID;
                    oBODisplay.BOInstance.BOD = null;
                }

                oCResult.xiStatus = xiEnumSystem.xiFuncResult.xiSuccess;
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