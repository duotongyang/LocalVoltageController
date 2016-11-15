﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;

namespace VoltController.VcReadPreviousControl
{
    public class ReadInfoline
    {
        #region [ Private Member ]
        private string m_infoline;

        #endregion

        [XmlAttribute("Infoline")]
        public string Infoline
        {
            get
            {
                return m_infoline;
            }
            set
            {
                Infoline = value;
            }
        }

        public ReadInfoline()
        {
            m_infoline = "Infoline";
        }

        

    }
}
