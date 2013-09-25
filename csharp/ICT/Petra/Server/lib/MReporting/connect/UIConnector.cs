//
// DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
//
// @Authors:
//       timop
//
// Copyright 2004-2013 by OM International
//
// This file is part of OpenPetra.org.
//
// OpenPetra.org is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// OpenPetra.org is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with OpenPetra.org.  If not, see <http://www.gnu.org/licenses/>.
//
using System;
using System.Data;
using System.IO;
using System.Xml;
using System.Drawing.Printing;
using System.Collections.Generic;
using Ict.Common.Remoting.Shared;
using Ict.Common.Remoting.Server;
using Ict.Petra.Shared;
using Ict.Petra.Shared.Interfaces.MReporting;
using Ict.Petra.Server.MCommon;
using Ict.Petra.Shared.MReporting;
using Ict.Petra.Server.MReporting;
using Ict.Petra.Server.MReporting.Calculator;
using Ict.Petra.Server.MReporting.MFinance;
using System.Threading;
using Ict.Common;
using Ict.Common.DB;
using Ict.Common.IO;
using Ict.Common.Printing;
using Ict.Petra.Shared.MCommon;

namespace Ict.Petra.Server.MReporting.UIConnectors
{
    /// <summary>
    /// the connector for the report generation
    /// </summary>
    public class TReportGeneratorUIConnector : IReportingUIConnectorsReportGenerator
    {
        private TAsynchronousExecutionProgress FAsyncExecProgress;
        private TRptDataCalculator FDatacalculator;
        private TResultList FResultList;
        private TParameterList FParameterList;
        private String FErrorMessage;
        private Boolean FSuccess;

        /// constructor needed for the interface
        public TReportGeneratorUIConnector()
        {
        }

        /// <summary>
        /// to show the progress of the report calculation;
        /// prints the current id of the row that is being calculated
        /// </summary>
        public IAsynchronousExecutionProgress AsyncExecProgress
        {
            get
            {
                return null; // TODORemoting
            }
        }

        /// <summary>
        /// to show the progress of the report calculation;
        /// prints the current id of the row that is being calculated;
        /// this is not remoting the progress. useful for unit tests
        /// </summary>
        [NoRemoting]
        public IAsynchronousExecutionProgress AsyncExecProgressServerSide
        {
            get
            {
                return FAsyncExecProgress;
            }
        }


        /// <summary>
        /// Calculates the report, which is specified in the parameters table
        ///
        /// </summary>
        /// <returns>void</returns>
        public void Start(System.Data.DataTable AParameters)
        {
            TRptUserFunctionsFinance.FlushSqlCache();
            this.FAsyncExecProgress = new TAsynchronousExecutionProgress();
            this.FAsyncExecProgress.ProgressState = TAsyncExecProgressState.Aeps_Executing;
            FParameterList = new TParameterList();
            FParameterList.LoadFromDataTable(AParameters);
            FSuccess = false;
            String PathStandardReports = TAppSettingsManager.GetValue("Reporting.PathStandardReports");
            String PathCustomReports = TAppSettingsManager.GetValue("Reporting.PathCustomReports");
            FDatacalculator = new TRptDataCalculator(DBAccess.GDBAccessObj, PathStandardReports, PathCustomReports);

            // setup the logging to go to the FAsyncExecProgress.ProgressInformation
            TLogging.SetStatusBarProcedure(new TLogging.TStatusCallbackProcedure(WriteToStatusBar));
            Thread TheThread = new Thread(new ThreadStart(Run));
            TheThread.CurrentCulture = Thread.CurrentThread.CurrentCulture;
            TheThread.Start();
        }

        /// <summary>
        /// cancel the report calculation
        /// </summary>
        public void Cancel()
        {
            // This variable will be picked up regularly during generation, in TRptDataCalcLevel.calculate in Ict.Petra.Server.MReporting.Calculation
            FParameterList.Add("CancelReportCalculation", new TVariant(true));
        }

        /// <summary>
        /// run the report
        /// </summary>
        private void Run()
        {
            try
            {
                if (FParameterList.Get("IsolationLevel").ToString().ToLower() == "readuncommitted")
                {
                    // for long reports, that should not take out locks;
                    // the data does not need to be consistent or will most likely not be changed during the generation of the report
                    DBAccess.GDBAccessObj.BeginTransaction(IsolationLevel.ReadUncommitted);
                }
                else if (FParameterList.Get("IsolationLevel").ToString().ToLower() == "repeatableread")
                {
                    // for financial reports: it is important to have consistent data; e.g. for totals
                    DBAccess.GDBAccessObj.BeginTransaction(IsolationLevel.RepeatableRead);
                }
                else if (FParameterList.Get("IsolationLevel").ToString().ToLower() == "serializable")
                {
                    // for creating extracts: we need to write to the database
                    DBAccess.GDBAccessObj.BeginTransaction(IsolationLevel.Serializable);
                }
                else
                {
                    // default behaviour for normal reports
                    DBAccess.GDBAccessObj.BeginTransaction(IsolationLevel.ReadCommitted);
                }

                FSuccess = false;

                if (FDatacalculator.GenerateResult(ref FParameterList, ref FResultList, ref FErrorMessage))
                {
                    FSuccess = true;
                }
                else
                {
                    TLogging.Log(FErrorMessage);
                }
            }
            catch (Exception e)
            {
                TLogging.Log("problem calculating report: " + e.Message);
                TLogging.Log(e.StackTrace, TLoggingType.ToLogfile);
            }
            DBAccess.GDBAccessObj.RollbackTransaction();
            FAsyncExecProgress.ProgressState = TAsyncExecProgressState.Aeps_Finished;
        }

        /// <summary>
        /// get the result of the report calculation
        /// </summary>
        public DataTable GetResult()
        {
            return FResultList.ToDataTable(FParameterList);
        }

        /// <summary>
        /// get the environment variables after report calculation
        /// </summary>
        public DataTable GetParameter()
        {
            return FParameterList.ToDataTable();
        }

        /// <summary>
        /// see if the report calculation finished successfully
        /// </summary>
        public Boolean GetSuccess()
        {
            return FSuccess;
        }

        /// <summary>
        /// error message that happened during report calculation
        /// </summary>
        public String GetErrorMessage()
        {
            return FErrorMessage;
        }

        /// <summary>
        /// for displaying the progress
        /// </summary>
        /// <returns>void</returns>
        private void WriteToStatusBar(String s)
        {
            FAsyncExecProgress.ProgressInformation = s;
        }

        private bool ExportToExcelFile(string AFilename)
        {
            bool ExportOnlyLowestLevel = false;

            // Add the parameter export_only_lowest_level to the Parameters if you don't want to export the
            // higher levels. In some reports (Supporting Churches Report or Partner Contact Report) the csv
            // output looks much nicer if it doesn't contain the unnecessary higher levels.
            if (FParameterList.Exists("csv_export_only_lowest_level"))
            {
                ExportOnlyLowestLevel = FParameterList.Get("csv_export_only_lowest_level").ToBool();
            }

            XmlDocument doc = FResultList.WriteXmlDocument(FParameterList, ExportOnlyLowestLevel);

            if (doc != null)
            {
                using (FileStream fs = new FileStream(AFilename, FileMode.Create))
                {
                    if (TCsv2Xml.Xml2ExcelStream(doc, fs, false))
                    {
                        fs.Close();
                    }
                }

                return true;
            }

            return false;
        }

        private bool PrintToPDF(string AFilename, bool AWrapColumn)
        {
            PrintDocument doc = new PrintDocument();

            TPdfPrinter pdfPrinter = new TPdfPrinter(doc, TGfxPrinter.ePrinterBehaviour.eReport);
            TReportPrinterLayout layout = new TReportPrinterLayout(FResultList, FParameterList, pdfPrinter, AWrapColumn);

            pdfPrinter.Init(eOrientation.ePortrait, layout, eMarginType.ePrintableArea);

            pdfPrinter.SavePDF(AFilename);

            return true;
        }

        private bool ExportToCSVFile(string AFilename)
        {
            bool ExportOnlyLowestLevel = false;

            // Add the parameter export_only_lowest_level to the Parameters if you don't want to export the
            // higher levels. In some reports (Supporting Churches Report or Partner Contact Report) the csv
            // output looks much nicer if it doesn't contain the unnecessary higher levels.
            if (FParameterList.Exists("csv_export_only_lowest_level"))
            {
                ExportOnlyLowestLevel = FParameterList.Get("csv_export_only_lowest_level").ToBool();
            }

            return FResultList.WriteCSV(FParameterList, AFilename, ExportOnlyLowestLevel);
        }

        /// <summary>
        /// send report as email
        /// </summary>
        public Boolean SendEmail(string AEmailAddresses, bool AAttachExcelFile, bool AAttachCSVFile, bool AAttachPDF, bool AWrapColumn)
        {
            List <string>FilesToAttach = new List <string>();

            if (AAttachExcelFile)
            {
                string ExcelFile = TFileHelper.GetTempFileName(
                    FParameterList.Get("currentReport").ToString(),
                    ".xlsx");

                if (ExportToExcelFile(ExcelFile))
                {
                    FilesToAttach.Add(ExcelFile);
                }
            }

            if (AAttachCSVFile)
            {
                string CSVFile = TFileHelper.GetTempFileName(
                    FParameterList.Get("currentReport").ToString(),
                    ".csv");

                if (ExportToCSVFile(CSVFile))
                {
                    FilesToAttach.Add(CSVFile);
                }
            }

            if (AAttachPDF)
            {
                string PDFFile = TFileHelper.GetTempFileName(
                    FParameterList.Get("currentReport").ToString(),
                    ".pdf");

                if (PrintToPDF(PDFFile, AWrapColumn))
                {
                    FilesToAttach.Add(PDFFile);
                }
            }

            TSmtpSender EmailSender = new TSmtpSender();

            // TODO use the email address of the user, from s_user
            if (EmailSender.SendEmail("<" + TAppSettingsManager.GetValue("Reports.Email.Sender") + ">",
                    "OpenPetra Reports",
                    AEmailAddresses,
                    FParameterList.Get("currentReport").ToString(),
                    Catalog.GetString("Please see attachment!"),
                    FilesToAttach.ToArray()))
            {
                foreach (string file in FilesToAttach)
                {
                    File.Delete(file);
                }

                return true;
            }

            return false;
        }
    }
}