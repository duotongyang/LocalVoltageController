﻿//******************************************************************************************************
//  VoltVarControllerAdapter.cs
//
//  Copyright © 2016, Duotong Yang  All Rights Reserved.
//
//  Code Modification History:
//  ----------------------------------------------------------------------------------------------------
//  11/09/2016 - Duotong Yang
//       Generated original version of source code.


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.IO;
using System.Collections;
using LVC.VcControlDevice;
using LVC.VcSubRoutines;
using ECAClientFramework;
using LVC.Model.test;


namespace LVC.Adapters
{
    [Serializable()]
    public class VoltVarControllerAdapter
    {
        #region [ Private Members ]
        private VoltVarController m_inputFrame;
        private string m_configurationPathName;
        private string m_logMessage;
        #endregion

        #region[ Properties ] 

        [XmlIgnore()]
        public VoltVarController InputFrame
        {
            get
            {
                return m_inputFrame;
            }
            set
            {
                m_inputFrame = value;
            }
        }

        [XmlIgnore()]
        public string ConfigurationPathName
        {
            get
            {
                return m_configurationPathName;
            }
            set
            {
                m_configurationPathName = value;
            }
        }

        [XmlAttribute("LogMessage")]
        public string LogMessage
        {
            get
            {
                return m_logMessage;
            }
            set
            {
                m_logMessage = value;
            }
        }

        #endregion

        #region [ Private Methods  ]

        private void InitializeInputFrame()
        {

            m_inputFrame = new VoltVarController();
            
        }

        #endregion

        #region [ Public Methods ]

        public void Initialize()
        {
            m_inputFrame = VoltVarController.DeserializeFromXml(m_configurationPathName);
            m_logMessage = null;
        }

        public void GetData(Input inputData, _InputMeta inputMeta, VoltVarController PreviousFrame)
        {
            #region [ openECA inputData Extraction ]
            // Extract inputData from openECA
            //m_inputFrame.ControlTransformers[0].TapV = inputData.TapVTx4;
            m_inputFrame.ControlTransformers[0].MwV = inputData.MwVTx4;
            m_inputFrame.ControlTransformers[0].MvrV = inputData.MvrVTx4;
            m_inputFrame.ControlTransformers[0].VoltsV = inputData.VoltsVTx4;

            //m_inputFrame.ControlTransformers[1].TapV = inputData.TapVTx5;
            m_inputFrame.ControlTransformers[1].MwV = inputData.MwVTx5;
            m_inputFrame.ControlTransformers[1].MvrV = inputData.MvrVTx5;
            m_inputFrame.ControlTransformers[1].VoltsV = inputData.VoltsVTx5;

            m_inputFrame.ControlCapacitorBanks[0].BusBkrV = inputData.BusBkrVCap1;
            //m_inputFrame.ControlCapacitorBanks[0].CapBkrV = inputData.CapBkrVCap1;
            m_inputFrame.ControlCapacitorBanks[0].LockvV = inputData.LocKvVCap1;

            m_inputFrame.ControlCapacitorBanks[1].BusBkrV = inputData.BusBkrVCap2;
            //m_inputFrame.ControlCapacitorBanks[1].CapBkrV = inputData.CapBkrVCap2;
            m_inputFrame.ControlCapacitorBanks[1].LockvV = inputData.LocKvVCap2;

            m_inputFrame.SubstationInformation.G1Mw = inputData.G1Mw;
            m_inputFrame.SubstationInformation.G1Mvr = inputData.G1Mvr;
            m_inputFrame.SubstationInformation.G2Mw = inputData.G2Mw;
            m_inputFrame.SubstationInformation.G2Mvr = inputData.G2Mvr;

            #endregion
            
            SubRoutine sub = new SubRoutine();
            ReadCurrentControl ReadCurrentCon = new ReadCurrentControl();
            
            VoltVarController Frame = new VoltVarController();

            #region [ Measurements Mapping ]

            m_inputFrame.OnNewMeasurements();

            #endregion

            #region [ Read The Previous Run ]

            m_inputFrame.ReadPreviousRun(PreviousFrame);

            #endregion

            #region[ Verify Program Controls ]

            ReadCurrentCon.VerifyProgramControl(m_inputFrame.SubstationAlarmDevice.LtcProgram);

            #endregion

            #region[ Adjust Control Delay Counters ]

            //#-----------------------------------------------------------------------#
            //# adjust the cap bank control delay counter, which is used to ensure:	#
            //# a. we don't do two cap bank control within 30 minutes of each other.	#
            //# b. we don't do a tap control within a minute of a cap bank control.	#
            //#-----------------------------------------------------------------------#

            if (m_inputFrame.SubstationInformation.Ncdel < m_inputFrame.SubstationInformation.Zcdel)
            {
                m_inputFrame.SubstationInformation.Ncdel = m_inputFrame.SubstationInformation.Ncdel + 1;
            }


            //#-----------------------------------------------------------------------#
            //# Adjust the tap control delay counter, which is used to ensure we	#
            //# don't do a cap bank control within a minute of a tap control.		#
            //#-----------------------------------------------------------------------#


            if (m_inputFrame.SubstationInformation.Ntdel < m_inputFrame.SubstationInformation.Zdel)
            {
                m_inputFrame.SubstationInformation.Ntdel = m_inputFrame.SubstationInformation.Ntdel + 1;
            }


            #endregion

            #region [ Read Curren Tx Values and Voltages ]

            m_inputFrame = ReadCurrentCon.ReadCurrentTransformerValuesAndVoltages(m_inputFrame);

            #endregion

            #region [ Check if the Previous Control Reults can Meet Our Expectation ]

            m_inputFrame = ReadCurrentCon.CheckPreviousControlResults(m_inputFrame);

            #endregion

            #region [ Call Sub Taps ]

            m_inputFrame = sub.Taps(m_inputFrame);

            #endregion

            #region [CapBank]

            m_inputFrame = sub.CapBank(m_inputFrame);

            #endregion

            #region [ Save before Exit ]

            m_logMessage = ReadCurrentCon.MessageInput;
            m_logMessage += sub.MessageInput;
            m_inputFrame.LtcStatus.Avv = 0;
            m_inputFrame.LtcStatus.Nins = 0;
            m_inputFrame.LtcStatus.MinVar = 99999;
            m_inputFrame.LtcStatus.MaxVar = -99999;

            #endregion

        }

        public void SerializeToXml(string pathName)
        {
            try
            {
                // Create an XmlSerializer with the type of NetworkModel
                XmlSerializer serializer = new XmlSerializer(typeof(VoltVarControllerAdapter));

                // Open a connection to the file and path.
                TextWriter writer = new StreamWriter(pathName);

                // Serialize this instance of NetworkModel
                serializer.Serialize(writer, this);

                // Close the connection
                writer.Close();
            }
            catch (Exception exception)
            {
                throw new Exception("Failed to Serialize the NetworkModel to the Configuration File: " + exception.ToString());
            }
        }

        #endregion
    }
}
