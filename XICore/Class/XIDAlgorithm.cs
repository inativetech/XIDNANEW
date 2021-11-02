using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using XISystem;

namespace XICore
{
    public class XIDAlgorithm
    {
        public int ID { get; set; }
        public string sName { get; set; }
        public string sDescription { get; set; }
        public string sAlgorithm { get; set; }

        public List<XIDAlgorithmLine> Lines { get; set; }

        public CResult Execute_XIAlgorithm(string sSessionID, string sGUID)
        {

            CResult oCRCompile = new CResult();
            CResult oCRRun = new CResult();
            CAlgorithmDefinition oAlgo = new CAlgorithmDefinition();
            CAlgorithmInstance oAlgoI = new CAlgorithmInstance();
            string sParameterValues = sSessionID + "," + sGUID;
            if (Lines != null && Lines.Count() > 0)
            {
                sAlgorithm = string.Join("&", Lines.OrderBy(m=>m.iOrder).Select(m => m.sScript).ToList());
            }
            if (!string.IsNullOrEmpty(sAlgorithm))
            {
                oCRCompile = oAlgoI.Compile_FromText(sAlgorithm);
                oAlgoI.Definition = (CAlgorithmDefinition)oCRCompile.oResult;
                //remove sessionid and GUID

                //CXIAPI_RT oCXIAPI = new CXIAPI_RT();
                //oCXIAPI.sSessionID = sSessionID;
                //oCXIAPI.sGUID = sGUID;


                //CXIAPI_EXE ocXIAPI = new CXIAPI_EXE();
                //ocXIAPI.qsid = 1234;

                //oCRRun = oAlgoI.Execute_OM(sParameterValues, ocXIAPI);


                oCRRun = oAlgoI.Execute_OM(sParameterValues, sSessionID, sGUID);
            }
            return oCRRun;
        }
    }

    public class XIDAlgorithmLine
    {
        public int ID { get; set; }
        public string sName { get; set; }
        public string sScript { get; set; }
        public int FKiAlgorithmID { get; set; }
        public int iOrder { get; set; }
    }
}