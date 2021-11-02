using System.Collections.Generic;

namespace XISystem
{
    public class CNV
    {
        public int ID { get; set; }
        private string Name = "";
        private string Value = "";
        private string Type = "";
        private string Context = "";

        private List<CNV> SubParams = new List<CNV>();

        public List<CNV> nSubParams
        {
            get
            {
                return SubParams;
            }
            set
            {
                SubParams = value;
            }
        }

        private Dictionary<string, CNV> oNNVs = new Dictionary<string, CNV>();

        public Dictionary<string, CNV> NNVs
        {
            get
            {
                return oNNVs;
            }
            set
            {
                oNNVs = value;
            }
        }

        public string sName
        {
            get
            {
                return Name;
            }
            set
            {
                Name = value;
            }
        }

        public string sValue
        {
            get
            {
                return Value;
            }
            set
            {
                Value = value;
            }
        }

        public string sType
        {
            get
            {
                return Type;
            }
            set
            {
                Type = value;
            }
        }

        public string sContext
        {
            get
            {
                return Context;
            }
            set
            {
                Context = value;
            }
        }

        public string sPreviousValue { get; set; }
    }
}