using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XICore
{
    public class XIDBODefault
    {
        public long ID { get; set; }
        public int FKiBOID { get; set; }
        public int i1ClickID { get; set; }
        public int iLayoutID { get; set; }
        public int iXIComponentID { get; set; }
        public int iStructureID { get; set; }
        public int iPopupID { get; set; }
        public int iType { get; set; }//For difference between popup,inline and dialog
    }
}