﻿//
// DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
//
// @Authors:
//       AlanP
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
using System.Globalization;
using System.Windows.Forms;
using System.Diagnostics;

using NUnit.Extensions.Forms;
using NUnit.Framework;

using Ict.Testing.NUnitForms;
using Ict.Testing.NUnitPetraClient;
//using Ict.Testing.NUnitPetraServer;
using Ict.Testing.NUnitTools;

using Ict.Common;
using Ict.Common.Data;
using Ict.Common.DB;
using Ict.Common.IO;
using Ict.Common.Verification;
using Ict.Petra.Client.App.Core.RemoteObjects;
using Ict.Petra.Client.CommonForms;
using Ict.Petra.Client.MFinance.Gui.Setup;
using Ict.Petra.Shared.MFinance.Account.Data;
using Ict.Petra.Shared.MFinance.Gift.Data;
using Ict.Petra.Shared.Interfaces.MCommon;
//using Ict.Petra.Server.App.Core;
using Ict.Common.Remoting.Shared;
using Ict.Common.Remoting.Client;
//using Ict.Common.Remoting.Server;

namespace Tests.MFinance.Client.ExchangeRates
{
    public partial class TDailyExchangeRateTest
    {
        #region FMainDS Class Definition

        private class FMainDS : SerialisableDS
        {
            public static ADailyExchangeRateTable ADailyExchangeRate;

            public static void LoadAll()
            {
                ADailyExchangeRate = new ADailyExchangeRateTable();
                SerialisableDS.LoadAll(ADailyExchangeRate, ADailyExchangeRateTable.GetTableDBName());
            }

            public static bool SaveChanges()
            {
                TTypedDataTable TableChanges = ADailyExchangeRate.GetChangesTyped();

                return SerialisableDS.SaveChanges(ADailyExchangeRate, TableChanges, ADailyExchangeRateTable.GetTableDBName());
            }

            public static void DeleteAllRows()
            {
                if (FMainDS.ADailyExchangeRate == null)
                {
                    // Not created yet
                    return;
                }

                DataView view = FMainDS.ADailyExchangeRate.DefaultView;

                for (int i = view.Count - 1; i >= 0; i--)
                {
                    view[i].Delete();
                }
            }

            public static void InsertStandardRows()
            {
                for (int i = 0; i <= StandardData.GetUpperBound(0); i++)
                {
                    AddARow(StandardData[i, 0].ToString(),
                        StandardData[i, 1].ToString(),
                        DateTime.Parse(StandardData[i, 2].ToString(), CultureInfo.InvariantCulture),
                        Convert.ToDecimal(StandardData[i, 3]));
                    Console.WriteLine("Inserted standard data to row {0}: {1}->{2} {3} @ {4}",
                        i + 1,
                        StandardData[i, FFromCurrencyId],
                        StandardData[i, FToCurrencyId],
                        StandardData[i, FDateEffectiveId],
                        StandardData[i, FRateOfExchangeId]);
                }
            }

            public static void InsertStandardModalRows()
            {
                for (int i = 0; i <= StandardModalData.GetUpperBound(0); i++)
                {
                    AddARow(StandardModalData[i, 0].ToString(),
                        StandardModalData[i, 1].ToString(),
                        DateTime.Parse(StandardModalData[i, 2].ToString(), CultureInfo.InvariantCulture),
                        Convert.ToDecimal(StandardModalData[i, 3]));
                    Console.WriteLine("Inserted standard data to row {0}: {1}->{2} {3} @ {4}",
                        i + 1,
                        StandardModalData[i, FFromCurrencyId],
                        StandardModalData[i, FToCurrencyId],
                        StandardModalData[i, FDateEffectiveId],
                        StandardModalData[i, FRateOfExchangeId]);
                }
            }

            public static void AddARow(String FromCurrency, String ToCurrency, DateTime EffectiveDate, decimal Rate)
            {
                ADailyExchangeRateRow newRow = FMainDS.ADailyExchangeRate.NewRowTyped();

                newRow.FromCurrencyCode = FromCurrency;
                newRow.ToCurrencyCode = ToCurrency;
                newRow.DateEffectiveFrom = EffectiveDate;
                newRow.TimeEffectiveFrom = 7200;
                newRow.RateOfExchange = Rate;
                FMainDS.ADailyExchangeRate.Rows.Add(newRow);
            }
        }

        #endregion

        #region Standard Exchange Rate Data stored in FMainDS

        /// <summary>
        /// The standard data creates 1 pair of currencies both ways
        /// There are two rates for 1900 and 2 rates for 2999 (when I won't be here!)
        /// The array items are specified oldest first, but on screen they will be newest first
        /// Also USD as To is specified before GBP but on screen it will be the other way round
        /// This is IMPORTANT for the test
        /// </summary>
        private static object[, ] StandardData =
        {
            { "GBP", "USD", "1900-06-01", 0.50m },
            { "GBP", "USD", "1900-07-01", 0.51m },
            { "GBP", "USD", "2999-06-01", 0.52m },
            { "GBP", "USD", "2999-07-01", 0.53m },
            { "USD", "GBP", "1900-06-01", 2.00m },
            { "USD", "GBP", "1900-07-01", 1.9607843137m },
            { "USD", "GBP", "2999-06-01", 1.9230769231m },
            { "USD", "GBP", "2999-07-01", 1.8867924528m }
        };

        private static object[, ] StandardModalData =
        {
            { "GBP", STANDARD_TEST_CURRENCY, "1900-06-01", 0.50m },
            { "GBP", STANDARD_TEST_CURRENCY, "1900-07-01", 0.51m },
            { "GBP", STANDARD_TEST_CURRENCY, "2999-06-01", 0.52m },
            { "GBP", STANDARD_TEST_CURRENCY, "2999-07-01", 0.53m },
            { STANDARD_TEST_CURRENCY, "GBP", "1900-06-01", 2.00m },
            { STANDARD_TEST_CURRENCY, "GBP", "1900-07-01", 1.9607843137m },
            { STANDARD_TEST_CURRENCY, "GBP", "2999-06-01", 1.9230769231m },
            { STANDARD_TEST_CURRENCY, "GBP", "2999-07-01", 1.8867924528m }
        };

        private const int FFromCurrencyId = 0;
        private const int FToCurrencyId = 1;
        private const int FDateEffectiveId = 2;
        private const int FRateOfExchangeId = 3;
        private const int FAllRowCount = 8;
        private const int FHiddenRowCount = 4;

        private int FCurrentDataId = 7;

        private int Row2DataId(int AGridRow)
        {
            // Based on our standard 8 rows: grid row 1->data row 7, grid row 2->data row 6 etc...
            return StandardData.GetUpperBound(0) - AGridRow + 1;
        }

        private void SelectRowInGrid(int AGridRow, int ADataRow)
        {
            TSgrdDataGridPagedTester grdTester = new TSgrdDataGridPagedTester("grdDetails");

            grdTester.SelectRow(AGridRow);
            FCurrentDataId = ADataRow;
        }

        private void SelectRowInGrid(int AGridRow)
        {
            SelectRowInGrid(AGridRow, Row2DataId(AGridRow));
        }

        private string EffectiveCurrency(int ACurrencyId)
        {
            return StandardData[FCurrentDataId, ACurrencyId].ToString();
        }

        private DateTime EffectiveDate()
        {
            return DateTime.Parse(StandardData[FCurrentDataId, FDateEffectiveId].ToString(), CultureInfo.InvariantCulture);
        }

        private decimal EffectiveRate()
        {
            return Convert.ToDecimal(StandardData[FCurrentDataId, FRateOfExchangeId]);
        }

        #endregion

        #region FCorporateDS Class Definition

        private class FCorporateDS : SerialisableDS
        {
            public static ACorporateExchangeRateTable ACorporateRateTable;

            private static void LoadAll()
            {
                ACorporateRateTable = new ACorporateExchangeRateTable();
                SerialisableDS.LoadAll(ACorporateRateTable, ACorporateExchangeRateTable.GetTableDBName());
            }

            private static bool SaveChanges()
            {
                TTypedDataTable TableChanges = ACorporateRateTable.GetChangesTyped();

                return SerialisableDS.SaveChanges(ACorporateRateTable, TableChanges, ACorporateExchangeRateTable.GetTableDBName());
            }

            public static void CreateCorporateRate(string FromCurrencyCode, string ToCurrencyCode, DateTime EffectiveDate, decimal RateOfExchange)
            {
                FCorporateDS.LoadAll();

                if (FCorporateDS.ACorporateRateTable.Rows.Find(new object[] { FromCurrencyCode, ToCurrencyCode, EffectiveDate }) != null)
                {
                    // exists already
                    return;
                }

                ACorporateExchangeRateRow newRow = FCorporateDS.ACorporateRateTable.NewRowTyped(true);
                newRow.FromCurrencyCode = FromCurrencyCode;
                newRow.ToCurrencyCode = ToCurrencyCode;
                newRow.TimeEffectiveFrom = 0;
                newRow.DateEffectiveFrom = EffectiveDate;
                newRow.RateOfExchange = RateOfExchange;

                FCorporateDS.ACorporateRateTable.Rows.Add(newRow);
                FCorporateDS.SaveChanges();
            }

            public static void DeleteAllCorporateRates()
            {
                if (FCorporateDS.ACorporateRateTable == null)
                {
                    // we did not create one yet
                    return;
                }

                DataView dv = new DataView(FCorporateDS.ACorporateRateTable, null, null, DataViewRowState.CurrentRows);

                for (int i = dv.Count - 1; i >= 0; i--)
                {
                    // exists already
                    DataRowView row = dv[i];
                    row.Delete();
                }

                FCorporateDS.SaveChanges();
            }
        }

        #endregion

        #region FLedgerDS Class Definition

        private class FLedgerDS : SerialisableDS
        {
            public static ALedgerTable ALedger;

            private static void LoadAll()
            {
                ALedger = new ALedgerTable();
                SerialisableDS.LoadAll(ALedger, ALedgerTable.GetTableDBName());
            }

            private static bool SaveChanges()
            {
                TTypedDataTable TableChanges = ALedger.GetChangesTyped();

                return SerialisableDS.SaveChanges(ALedger, TableChanges, ALedgerTable.GetTableDBName());
            }

            public static void CreateTestLedger()
            {
                FLedgerDS.LoadAll();
                DataView dv = new DataView(FLedgerDS.ALedger,
                    "a_ledger_number_i=" + STANDARD_TEST_LEDGER_NUMBER.ToString(), null, DataViewRowState.CurrentRows);

                if (dv.Count > 0)
                {
                    // exists already
                    return;
                }

                int NewLedgerNumber = STANDARD_TEST_LEDGER_NUMBER;
                ALedgerRow newRow = FLedgerDS.ALedger.NewRowTyped(true);
                newRow.LedgerNumber = NewLedgerNumber;
                newRow.LedgerName = "TestLedger";
                newRow.BaseCurrency = STANDARD_TEST_CURRENCY;
                newRow.ForexGainsLossesAccount = "Trash";
                newRow.PartnerKey = NewLedgerNumber * 10000;

                FLedgerDS.ALedger.Rows.Add(newRow);
                FLedgerDS.SaveChanges();
            }

            public static void DeleteTestLedgerIfExists()
            {
                if (FLedgerDS.ALedger == null)
                {
                    // we did not create one yet
                    return;
                }

                DataView dv = new DataView(FLedgerDS.ALedger,
                    "a_ledger_number_i=" + STANDARD_TEST_LEDGER_NUMBER.ToString(), null, DataViewRowState.CurrentRows);

                if (dv.Count > 0)
                {
                    // exists already
                    DataRowView row = dv[0];
                    row.Delete();
                    FLedgerDS.SaveChanges();
                }
            }
        }

        #endregion

        #region FJournalDS Class Definition

        private class FJournalDS : SerialisableDS
        {
            public static AJournalTable AJournal;

            public static void LoadAll()
            {
                AJournal = new AJournalTable();
                SerialisableDS.LoadAll(AJournal, AJournalTable.GetTableDBName());
            }
        }

        #endregion

        #region FGiftBatchDS Class Definition

        private class FGiftBatchDS : SerialisableDS
        {
            public static AGiftBatchTable AGiftBatch;

            public static void LoadAll()
            {
                AGiftBatch = new AGiftBatchTable();
                SerialisableDS.LoadAll(AGiftBatch, AGiftBatchTable.GetTableDBName());
            }
        }

        #endregion

        #region Journal and Gift Batch data tables - using SQL scripts

        private class FGiftAndJournal
        {
            /// <summary>
            /// Remove all data
            /// </summary>
            public static void Clean()
            {
                ConnectToDatabase();
                RunSQL("clean.sql");
                CloseDatabaseConnection();
            }

            /// <summary>
            /// Set up the required additional tables
            /// </summary>
            public static void InitialiseData(String ASQLInsertFile)
            {
                ConnectToDatabase();
                RunSQL("clean.sql");
                RunSQL("init.sql");
                RunSQL(ASQLInsertFile);
                CloseDatabaseConnection();
            }

            /// <summary>
            /// Helper method to connect direct to the server for access to the GDBAccessObj
            /// This will allow us to run SQL statements directly
            /// </summary>
            private static void ConnectToDatabase()
            {
                new TAppSettingsManager("../../etc/TestServer.config");
                DBAccess.GDBAccessObj = new TDataBase();

                try
                {
                    DBAccess.GDBAccessObj.EstablishDBConnection(CommonTypes.ParseDBType(TAppSettingsManager.GetValue("Server.RDBMSType")),
                        TAppSettingsManager.GetValue("Server.DBHostOrFile"),
                        TAppSettingsManager.GetValue("Server.DBPort"),
                        TAppSettingsManager.GetValue("Server.DBName"),
                        TAppSettingsManager.GetValue("Server.DBUserName"),
                        TAppSettingsManager.GetValue("Server.DBPassword"),
                        "");
                }
                catch (Exception ex)
                {
                    Assert.Fail("Failed to connect to database for Gift Batch and Journal data: " + ex.Message);
                }

                // restore the AppSettings to the client ones again in case they are needed
                new TAppSettingsManager("../../etc/TestClient.config");
            }

            private static void CloseDatabaseConnection()
            {
                try
                {
                    DBAccess.GDBAccessObj.CloseDBConnection();
                }
                catch (Exception)
                {
                }
            }

            /// <summary>
            /// Helper to run a specific sql file as ExecuteNonQuery()
            /// </summary>
            /// <param name="AFileName"></param>
            private static void RunSQL(string AFileName)
            {
                string sql = String.Empty;

                // Work out the path to the file
                string TestFile = Path.GetFullPath(TAppSettingsManager.GetValue("Testing.Path") + "/lib/MFinance/ExchangeRates/sql/" + AFileName);

                Assert.IsTrue(File.Exists(TestFile));

                // Read the content
                using (StreamReader sr = new StreamReader(TestFile))
                {
                    sql = sr.ReadToEnd();

                    sr.Close();
                }

                // Execute the query
                int nRowsAffected = 0;

                if (sql != String.Empty)
                {
                    Boolean IsNewTransaction;
                    TDBTransaction WriteTransaction = DBAccess.GDBAccessObj.GetNewOrExistingTransaction(IsolationLevel.Serializable,
                        out IsNewTransaction);
                    nRowsAffected = DBAccess.GDBAccessObj.ExecuteNonQuery(sql, WriteTransaction, null, IsNewTransaction);
                }

                // Did we do anything?
                if (!AFileName.Contains("clean.sql"))
                {
                    // We should have inserted rows (The 'clean' script may have nothing to do if it is run twice)
                    Assert.AreNotEqual(0, nRowsAffected, "Failed to run the SQL insertion script: " + Path.GetFileNameWithoutExtension(AFileName));
                }
            }
        }

        #endregion
    }
}