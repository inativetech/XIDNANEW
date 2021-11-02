using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XISystem;
using xiEnumSystem;

namespace XICore
{
    public class XIMatrix
    {
        //TO DO
        //Remove refmatrixactionid add smatrixcode
        //IsMatrix checkboxes on step, xilink, 1-clicks etc
        // FK on code not on Action ID for refMatrixAction
        public CResult MatrixAction(string sMatrixActionCode, EnumMatrixAction eMatrixType, string sXIObjectName, long iOIID, long iItemInstanceID, string sName, List<CNV> nParams)
        {
            CResult oCR = new CResult();
            XIInfraCache oCache = new XIInfraCache();
            var sUserID = nParams.Where(m => m.sName.ToLower() == XIConstant.Param_UserID.ToLower()).Select(m => m.sValue).FirstOrDefault();
            var sEmail = nParams.Where(m => m.sName.ToLower() == "Email".ToLower()).Select(m => m.sValue).FirstOrDefault();
            var sDescription = nParams.Where(m => m.sName.ToLower() == "Description".ToLower()).Select(m => m.sValue).FirstOrDefault();
            //Check appliction setting for matrix action is ON
            XIIXI oXII = new XIIXI();
            XIIBO oBOI = new XIIBO();
            List<CNV> oWhrParams = new List<CNV>();
            oWhrParams.Add(new CNV { sName = "sCode", sValue = "MatrixTracking" });
            oBOI = oXII.BOI("xiorgapplicationsettings", null, null, oWhrParams);
            if (oBOI != null)
            {

            }
            var sValue = oBOI.AttributeI("svalue").sValue;
            if (sValue.ToLower() == "y" || sValue.ToLower() == "yes" || sValue.ToLower() == "true")
            {
                //XID1Click o1ClickD = new XID1Click();
                //o1ClickD = (XID1Click)oCache.GetObjectFromCache(XIConstant.Cache1Click, "XIrefMatrixActionCache");
                //XID1Click o1ClickC = new XID1Click();
                //o1ClickC = (XID1Click)o1ClickD.Clone(o1ClickD);
                //var XIrefMatrixActions = o1ClickC.OneClick_Execute();
                ////var refMatrixActionID = string.Empty;
                //XIIBO refMatrixAction = null;
                //if (XIrefMatrixActions !=null && XIrefMatrixActions.Values.Count() > 0)
                //{
                //    var Actions = XIrefMatrixActions.Values.ToList();
                //    refMatrixAction = Actions.Where(m => m.Attributes["sname".ToLower()].sValue.ToLower() == sMatrixActionCode.ToLower()).FirstOrDefault();
                //    refMatrixActionID = refMatrixAction.AttributeI("id").sValue;
                //}
                XIIBO oTransI = new XIIBO();
                oTransI.BOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, "ximatrixtransaction");
                //oTransI.SetAttribute("refMatrixActionID", refMatrixActionID);
                //if (!string.IsNullOrEmpty(sName))
                //{
                //    oTransI.SetAttribute("sName", sName);
                //}
                //else
                //{
                //    oTransI.SetAttribute("sName", refMatrixAction.AttributeI("sname").sValue);
                //}
                oTransI.SetAttribute("sName", sName);
                oTransI.SetAttribute("sCode", sMatrixActionCode);
                var iActionType = (int)Enum.Parse(typeof(EnumMatrixAction), eMatrixType.ToString());
                if (!string.IsNullOrEmpty(sXIObjectName))
                {
                    var ObjectBOD = (XIDBO)oCache.GetObjectFromCache(XIConstant.CacheBO, sXIObjectName);
                    oTransI.SetAttribute("ODID", ObjectBOD.BOID.ToString());
                }
                oTransI.SetAttribute("OIID", iOIID.ToString());
                oTransI.SetAttribute("iItemInstanceID", iItemInstanceID.ToString());
                oTransI.SetAttribute("iActionType", iActionType.ToString());
                oTransI.SetAttribute("FKsUserID", sUserID);
                oTransI.SetAttribute("sDescription", sDescription);
                oTransI.SetAttribute("sEmail", sEmail);
                oCR = oBOI.Save(oTransI);
                if (oCR.bOK)
                {

                }



            }


            //Load refMatrixAction
            //Create a new MatrixAction Object instance
            //Assign User
            //Send in the action type and the refMatrixAction
            //EnumMatrixAction
            //Pass in itemInstance, XIObjectDefinitionID and XIObjectInstanceID

            return oCR;
        }
    }
}