﻿using System;
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

namespace XICore
{
    public class XIDValue : XIDefinitionBase
    {
        private string sMyDefaultValue;
        public string sDefaultValue
        {
            get
            {
                return sMyDefaultValue;
            }
            set
            {
                sMyDefaultValue = value;
            }
        }
    }
}