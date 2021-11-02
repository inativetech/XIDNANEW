using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace XICore
{
    public class XIDFieldDefinition
    {
        public int ID { get; set; }
        public int FKiXIFieldOriginID { get; set; }
        public int FKiXIStepDefinitionID { get; set; }
        public int FKiStepSectionID { get; set; }

        private XIDFieldOrigin oMyFieldOrigin = new XIDFieldOrigin();
        public XIDFieldOrigin FieldOrigin
        {
            get
            {
                return oMyFieldOrigin;
            }
            set
            {
                oMyFieldOrigin = value;
            }
        }
    }
}