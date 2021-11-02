using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XICore
{
    public class XIQSVisualisation
    {
        public int ID { get; set; }
        public int FKiQSDefinitionID { get; set; }
        public int FKiQSStepDefinitionID { get; set; }
        public int FKiSectionDefinitionID { get; set; }
        public int FKiFieldOriginID { get; set; }
        public string sVisualisation { get; set; }
        public int FKiApplicationID { get; set; }
    }
}