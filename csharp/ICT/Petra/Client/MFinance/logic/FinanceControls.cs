//
// DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
//
// @Authors:
//       timop
//
// Copyright 2004-2010 by OM International
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
using System.Collections.Specialized;
using System.Data;

using Ict.Common;
using Ict.Common.Controls;
using Ict.Common.Data;
using Ict.Petra.Client.App.Core;
using Ict.Petra.Client.App.Core.RemoteObjects;
using Ict.Petra.Client.CommonControls;
using Ict.Petra.Shared.MFinance;
using Ict.Petra.Shared.MFinance.Account.Data;
using Ict.Petra.Shared.MFinance.Gift.Data;
using Ict.Petra.Shared.MFinance.GL.Data;
using Ict.Petra.Shared.MPartner;
using Ict.Petra.Shared.MPartner.Mailroom.Data;
using Ict.Petra.Shared.MPartner.Partner.Data;

namespace Ict.Petra.Client.MFinance.Logic
{
    /// <summary>
    /// this provides some static functions that initialise
    /// comboboxes and other controls with static values or cached values for the finance module
    /// this helps to make similar controls look the same throughout the application
    /// </summary>
    public class TFinanceControls
    {
        /// <summary>
        /// returns a filter for cost centre cached table
        /// </summary>
        /// <param name="APostingOnly"></param>
        /// <param name="AExcludePosting"></param>
        /// <param name="AActiveOnly"></param>
        /// <param name="ALocalOnly"></param>
        private static string PrepareCostCentreFilter(bool APostingOnly, bool AExcludePosting, bool AActiveOnly, bool ALocalOnly)
        {
            string Filter = "";

            if (APostingOnly)
            {
                Filter += ACostCentreTable.GetPostingCostCentreFlagDBName() + " = true";
            }
            else if (AExcludePosting)
            {
                Filter += ACostCentreTable.GetPostingCostCentreFlagDBName() + " = false";
            }

            if (AActiveOnly)
            {
                if (Filter.Length > 0)
                {
                    Filter += " AND ";
                }

                Filter += ACostCentreTable.GetCostCentreActiveFlagDBName() + " = true";
            }

            if (ALocalOnly)
            {
                if (Filter.Length > 0)
                {
                    Filter += " AND ";
                }

                Filter += ACostCentreTable.GetCostCentreTypeDBName() + " = 'Local'";
            }

            return Filter;
        }

        /// <summary>
        /// returns a filter for accounts cached table
        /// </summary>
        /// <param name="APostingOnly"></param>
        /// <param name="AExcludePosting"></param>
        /// <param name="AActiveOnly"></param>
        /// <param name="ABankAccountOnly"></param>
        private static string PrepareAccountFilter(bool APostingOnly, bool AExcludePosting, bool AActiveOnly, bool ABankAccountOnly)
        {
            string Filter = "";

            if (APostingOnly)
            {
                Filter += AAccountTable.GetPostingStatusDBName() + " = true";
            }
            else if (AExcludePosting)
            {
                Filter += AAccountTable.GetPostingStatusDBName() + " = false";
            }

            if (AActiveOnly)
            {
                if (Filter.Length > 0)
                {
                    Filter += " AND ";
                }

                Filter += AAccountTable.GetAccountActiveFlagDBName() + " = true";
            }

            // GetCacheableFinanceTable returns a DataTable with a bank flag
            if (ABankAccountOnly)
            {
                if (Filter.Length > 0)
                {
                    Filter += " AND ";
                }

                Filter += GLSetupTDSAAccountTable.GetBankAccountFlagDBName() + " = true";
            }

            return Filter;
        }

        /// <summary>
        /// fill checkedlistbox values with cost centre list
        /// </summary>
        /// <param name="AControl"></param>
        /// <param name="ALedgerNumber"></param>
        /// <param name="APostingOnly"></param>
        /// <param name="AExcludePosting"></param>
        /// <param name="AActiveOnly"></param>
        /// <param name="ALocalOnly">Local Costcentres only; otherwise foreign costcentres (ie from other legal entities) are included)</param>
        public static void InitialiseCostCentreList(ref TClbVersatile AControl,
            Int32 ALedgerNumber,
            bool APostingOnly,
            bool AExcludePosting,
            bool AActiveOnly,
            bool ALocalOnly)
        {
            string CheckedMember = "CHECKED";
            string DisplayMember = ACostCentreTable.GetCostCentreNameDBName();
            string ValueMember = ACostCentreTable.GetCostCentreCodeDBName();

            DataTable Table = TDataCache.TMFinance.GetCacheableFinanceTable(TCacheableFinanceTablesEnum.CostCentreList, ALedgerNumber);
            DataView view = new DataView(Table);

            view.RowFilter = PrepareCostCentreFilter(APostingOnly, AExcludePosting, AActiveOnly, ALocalOnly);

            DataTable NewTable = view.ToTable(true, new string[] { ValueMember, DisplayMember });
            NewTable.Columns.Add(new DataColumn(CheckedMember, typeof(bool)));

            AControl.Columns.Clear();
            AControl.AddCheckBoxColumn("", NewTable.Columns[CheckedMember], 17, false);
            AControl.AddTextColumn(Catalog.GetString("Code"), NewTable.Columns[ValueMember], 60);
            AControl.AddTextColumn(Catalog.GetString("Cost Centre Description"), NewTable.Columns[DisplayMember], 200);
            AControl.DataBindGrid(NewTable, ValueMember, CheckedMember, ValueMember, DisplayMember, false, true, false);
        }

        /// <summary>
        /// fill checkedlistbox values with account codes
        /// </summary>
        /// <param name="AControl"></param>
        /// <param name="ALedgerNumber"></param>
        /// <param name="APostingOnly"></param>
        /// <param name="AExcludePosting"></param>
        /// <param name="AActiveOnly"></param>
        /// <param name="ABankAccountOnly"></param>
        public static void InitialiseAccountList(ref TClbVersatile AControl,
            Int32 ALedgerNumber,
            bool APostingOnly,
            bool AExcludePosting,
            bool AActiveOnly,
            bool ABankAccountOnly)
        {
            string CheckedMember = "CHECKED";
            string DisplayMember = AAccountTable.GetAccountCodeShortDescDBName();
            string ValueMember = AAccountTable.GetAccountCodeDBName();

            DataTable Table = TDataCache.TMFinance.GetCacheableFinanceTable(TCacheableFinanceTablesEnum.AccountList, ALedgerNumber);
            DataView view = new DataView(Table);

            view.RowFilter = PrepareAccountFilter(APostingOnly, AExcludePosting, AActiveOnly, ABankAccountOnly);

            DataTable NewTable = view.ToTable(true, new string[] { ValueMember, DisplayMember });
            NewTable.Columns.Add(new DataColumn(CheckedMember, typeof(bool)));

            AControl.Columns.Clear();
            AControl.AddCheckBoxColumn("", NewTable.Columns[CheckedMember], 17, false);
            AControl.AddTextColumn(Catalog.GetString("Code"), NewTable.Columns[ValueMember], 60);
            AControl.AddTextColumn(Catalog.GetString("Account Description"), NewTable.Columns[DisplayMember], 200);
            AControl.DataBindGrid(NewTable, ValueMember, CheckedMember, ValueMember, DisplayMember, false, true, false);
        }

        /// <summary>
        /// fill combobox values with cost centre list
        /// </summary>
        /// <param name="AControl"></param>
        /// <param name="ALedgerNumber"></param>
        /// <param name="APostingOnly"></param>
        /// <param name="AExcludePosting"></param>
        /// <param name="AActiveOnly"></param>
        /// <param name="ALocalOnly">Local Costcentres only; otherwise foreign costcentres (ie from other legal entities) are included)</param>
        public static void InitialiseCostCentreList(ref TCmbAutoPopulated AControl,
            Int32 ALedgerNumber,
            bool APostingOnly,
            bool AExcludePosting,
            bool AActiveOnly,
            bool ALocalOnly)
        {
            DataTable Table = TDataCache.TMFinance.GetCacheableFinanceTable(TCacheableFinanceTablesEnum.CostCentreList, ALedgerNumber);

            AControl.InitialiseUserControl(Table,
                ACostCentreTable.GetCostCentreCodeDBName(),
                ACostCentreTable.GetCostCentreNameDBName(),
                null);
            AControl.AppearanceSetup(new int[] { -1, 150 }, -1);

            AControl.Filter = PrepareCostCentreFilter(APostingOnly, AExcludePosting, AActiveOnly, ALocalOnly);
        }

        /// <summary>
        /// fill combobox values with account codes
        /// </summary>
        /// <param name="AControl"></param>
        /// <param name="ALedgerNumber"></param>
        /// <param name="APostingOnly"></param>
        /// <param name="AExcludePosting"></param>
        /// <param name="AActiveOnly"></param>
        /// <param name="ABankAccountOnly"></param>
        public static void InitialiseAccountList(ref TCmbAutoPopulated AControl,
            Int32 ALedgerNumber,
            bool APostingOnly,
            bool AExcludePosting,
            bool AActiveOnly,
            bool ABankAccountOnly)
        {
            DataTable Table = TDataCache.TMFinance.GetCacheableFinanceTable(TCacheableFinanceTablesEnum.AccountList, ALedgerNumber);

            AControl.InitialiseUserControl(Table,
                AAccountTable.GetAccountCodeDBName(),
                AAccountTable.GetAccountCodeShortDescDBName(),
                null);
            AControl.AppearanceSetup(new int[] { -1, 150 }, -1);

            AControl.Filter = PrepareAccountFilter(APostingOnly, AExcludePosting, AActiveOnly, ABankAccountOnly);
        }

        /// <summary>
        /// fill combobox values with list of transaction types
        /// </summary>
        /// <param name="AControl"></param>
        /// <param name="ALedgerNumber"></param>
        /// <param name="ASubSystemCode"></param>
        public static void InitialiseTransactionTypeList(ref TCmbAutoPopulated AControl, Int32 ALedgerNumber, string ASubSystemCode)
        {
            // TODO: use cached table for transaction types? use filter to get only appropriate types for subsystem?
            TTypedDataTable Table;

            TRemote.MCommon.DataReader.GetData(TTypedDataTable.GetTableNameSQL(ATransactionTypeTable.TableId),
                new TSearchCriteria[] {
                    new TSearchCriteria(TTypedDataTable.GetColumnNameSQL(ATransactionTypeTable.TableId,
                            ATransactionTypeTable.ColumnLedgerNumberId), ALedgerNumber),
                    new TSearchCriteria(TTypedDataTable.GetColumnNameSQL(ATransactionTypeTable.TableId,
                            ATransactionTypeTable.ColumnSubSystemCodeId), ASubSystemCode)
                },
                out Table);

            AControl.InitialiseUserControl(
                Table,
                ATransactionTypeTable.GetTransactionTypeCodeDBName(),
                ATransactionTypeTable.GetTransactionTypeDescriptionDBName(),
                null);

            AControl.AppearanceSetup(new int[] { -1, 150 }, -1);
        }

        /// <summary>
        /// fill combobox values with motivation group list
        /// </summary>
        /// <param name="AControl"></param>
        /// <param name="ALedgerNumber"></param>
        /// <param name="AActiveOnly"></param>
        public static void InitialiseMotivationGroupList(ref TCmbAutoPopulated AControl,
            Int32 ALedgerNumber,
            bool AActiveOnly)
        {
            AMotivationGroupTable groupTable = new AMotivationGroupTable();

            DataTable detailTable = TDataCache.TMFinance.GetCacheableFinanceTable(TCacheableFinanceTablesEnum.MotivationList, ALedgerNumber);

            // since we get the details, we have duplicates for group; remove the duplicates
            StringCollection groups = new StringCollection();

            foreach (AMotivationDetailRow detail in detailTable.Rows)
            {
                if (((AActiveOnly && detail.MotivationStatus) || !AActiveOnly)
                    && !groups.Contains(detail.MotivationGroupCode))
                {
                    groups.Add(detail.MotivationGroupCode);
                    AMotivationGroupRow newGroup = groupTable.NewRowTyped(true);
                    newGroup.MotivationGroupCode = detail.MotivationGroupCode;

                    // also assign: description for group?

                    groupTable.Rows.Add(newGroup);
                }
            }

            AControl.InitialiseUserControl(groupTable,
                AMotivationGroupTable.GetMotivationGroupCodeDBName(),
                AMotivationGroupTable.GetMotivationGroupCodeDBName(),
                null);
            AControl.AppearanceSetup(new int[] { -1, 150 }, -1);
        }

        /// <summary>
        /// fill combobox values with motivation detail list
        /// </summary>
        /// <param name="AControl"></param>
        /// <param name="ALedgerNumber"></param>
        /// <param name="AActiveOnly"></param>
        public static void InitialiseMotivationDetailList(ref TCmbAutoPopulated AControl,
            Int32 ALedgerNumber,
            bool AActiveOnly)
        {
            DataTable detailTable = TDataCache.TMFinance.GetCacheableFinanceTable(TCacheableFinanceTablesEnum.MotivationList, ALedgerNumber);

            AControl.InitialiseUserControl(detailTable,
                AMotivationDetailTable.GetMotivationDetailCodeDBName(),
                AMotivationDetailTable.GetMotivationDetailDescDBName(),
                null);
            AControl.AppearanceSetup(new int[] { -1, 150 }, -1);

            if (AActiveOnly)
            {
                AControl.Filter = AMotivationDetailTable.GetMotivationStatusDBName() + " = true";
            }
            else
            {
                AControl.Filter = "";
            }
        }

        /// <summary>
        /// change the filter of the motivation detail combobox when a different motivation group gets selected
        /// </summary>
        /// <param name="AControl"></param>
        /// <param name="AMotivationGroup"></param>
        public static void ChangeFilterMotivationDetailList(ref TCmbAutoPopulated AControl, String AMotivationGroup)
        {
            string newFilter = String.Empty;

            if ((AControl.Filter != null) && AControl.Filter.StartsWith(AMotivationDetailTable.GetMotivationStatusDBName() + " = true"))
            {
                newFilter = AMotivationDetailTable.GetMotivationStatusDBName() + " = true and ";
            }

            newFilter += AMotivationDetailTable.GetMotivationGroupCodeDBName() + " = '" + AMotivationGroup + "'";

            AControl.Filter = newFilter;
        }

        /// <summary>
        /// fill combobox values with method of giving list
        /// </summary>
        /// <param name="AControl"></param>
        /// <param name="AActiveOnly"></param>
        public static void InitialiseMethodOfGivingCodeList(ref TCmbAutoPopulated AControl,
            bool AActiveOnly)
        {
            DataTable Table = TDataCache.TMFinance.GetCacheableFinanceTable(TCacheableFinanceTablesEnum.MethodOfGivingList);

            AControl.InitialiseUserControl(Table,
                AMethodOfGivingTable.GetMethodOfGivingCodeDBName(),
                AMethodOfGivingTable.GetMethodOfGivingDescDBName(),
                null);
            AControl.AppearanceSetup(new int[] { -1, 150 }, -1);

            if (AActiveOnly)
            {
                AControl.Filter = AMethodOfGivingTable.GetActiveDBName() + " = true";
            }
            else
            {
                AControl.Filter = "";
            }
        }

        /// <summary>
        /// fill combobox values with method of payment list
        /// </summary>
        /// <param name="AControl"></param>
        /// <param name="AActiveOnly"></param>
        public static void InitialiseMethodOfPaymentCodeList(ref TCmbAutoPopulated AControl,
            bool AActiveOnly)
        {
            DataTable Table = TDataCache.TMFinance.GetCacheableFinanceTable(TCacheableFinanceTablesEnum.MethodOfPaymentList);

            AControl.InitialiseUserControl(Table,
                AMethodOfPaymentTable.GetMethodOfPaymentCodeDBName(),
                AMethodOfPaymentTable.GetMethodOfPaymentCodeDBName(),
                AMethodOfPaymentTable.GetMethodOfPaymentDescDBName(),
                null,
                AMethodOfPaymentTable.GetActiveDBName()
                );
            AControl.AppearanceSetup(new int[] { -1, 150 }, -1);

            if (AActiveOnly)
            {
                AControl.Filter = AMethodOfPaymentTable.GetActiveDBName() + " = true";
            }
            else
            {
                AControl.Filter = "";
            }
        }

        /// <summary>
        /// fill combobox values with the mailing codes
        /// </summary>
        /// <param name="AControl"></param>
        /// <param name="AActiveOnly"></param>
        public static void InitialisePMailingList(ref TCmbAutoPopulated AControl,
            bool AActiveOnly)
        {
            DataTable Table = TDataCache.TMPartner.GetCacheableMailingTable(TCacheableMailingTablesEnum.MailingList);

            AControl.InitialiseUserControl(Table,
                PMailingTable.GetMailingCodeDBName(),
                PMailingTable.GetMailingDescriptionDBName(),
                null);
            AControl.AppearanceSetup(new int[] { -1, 150 }, -1);

            if (AActiveOnly)
            {
                AControl.Filter = PMailingTable.GetViewableDBName() + " = true";
                //TODO Add viewable until and date comparison
            }
            else
            {
                AControl.Filter = "";
            }
        }

        /// <summary>
        /// This function fills the available account hierarchies of a given ledger into a combobox
        /// </summary>
        /// <param name="AControl"></param>
        /// <param name="ALedgerNr"></param>
        public static void InitialiseAccountHierarchyList(ref TCmbAutoPopulated AControl, System.Int32 ALedgerNr)
        {
            DataTable Table = TDataCache.TMFinance.GetCacheableFinanceTable(TCacheableFinanceTablesEnum.AccountHierarchyList, ALedgerNr);

            AControl.InitialiseUserControl(Table,
                AAccountHierarchyTable.GetAccountHierarchyCodeDBName(),
                null,
                null);
            AControl.AppearanceSetup(new int[] { 150 }, -1);

            AControl.Filter = AAccountHierarchyTable.GetLedgerNumberDBName() + " = " + ALedgerNr.ToString();
        }

        static PUnitTable FKeyMinTable = null;
        static Int64 fieldNumber = -1;

        public static long FieldNumber {
            get
            {
                return fieldNumber;
            }
        }


        /// <summary>
        /// This function fills the combobox for the key ministry depending on the partnerkey
        /// </summary>
        /// <param name="AControl"></param>
        /// <param name="ALedgerNr"></param>
        public static void GetRecipientData(ref TCmbAutoPopulated AControl, System.Int64 APartnerKey)
        {
            if (FKeyMinTable != null)
            {
                if (FindAndSelect(ref AControl, APartnerKey))
                {
                    return;
                }
            }

            string DisplayMember = PUnitTable.GetUnitNameDBName();
            string ValueMember = PUnitTable.GetPartnerKeyDBName();
            FKeyMinTable = TRemote.MFinance.Gift.WebConnectors.LoadKeyMinistry(APartnerKey, out fieldNumber);

            FKeyMinTable.DefaultView.Sort = DisplayMember + " Desc";

            AControl.InitialiseUserControl(FKeyMinTable,
                ValueMember,
                DisplayMember,
                null,
                null);
            AControl.AppearanceSetup(new int[] { 250 }, -1);

            if (!FindAndSelect(ref AControl, APartnerKey))
            {
                //Clear the combobox
                AControl.SelectedValueCell = null;
            }
        }

        static bool FindAndSelect(ref TCmbAutoPopulated AControl, System.Int64 APartnerKey)
        {
            foreach (PUnitRow pr in FKeyMinTable.Rows)
            {
                if (pr.PartnerKey == APartnerKey)
                {
                    AControl.SelectedValueCell = APartnerKey;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// This function fills the available financial years of a given ledger into a combobox
        /// </summary>
        /// <param name="AControl"></param>
        /// <param name="ALedgerNr"></param>
        public static void InitialiseAvailableFinancialYearsList(ref TCmbAutoPopulated AControl, System.Int32 ALedgerNr)
        {
            string DisplayMember;
            string ValueMember;
            DataTable Table = TRemote.MFinance.Reporting.UIConnectors.GetAvailableFinancialYears(0, out DisplayMember, out ValueMember);

            Table.DefaultView.Sort = "YearNumber Desc";

            AControl.InitialiseUserControl(Table,
                ValueMember,
                DisplayMember,
                null,
                null);

            AControl.AppearanceSetup(new int[] { -1 }, -1);
        }

        /// <summary>
        /// return the ledger number and name for readonly text boxes
        /// </summary>
        /// <param name="ALedgerNumber"></param>
        /// <returns></returns>
        public static string GetLedgerNumberAndName(Int32 ALedgerNumber)
        {
            DataTable Table = TDataCache.TMFinance.GetCacheableFinanceTable(TCacheableFinanceTablesEnum.LedgerNameList);

            foreach (DataRow row in Table.Rows)
            {
                if (row["LedgerNumber"].ToString() == ALedgerNumber.ToString())
                {
                    return ALedgerNumber.ToString() + " " + row["LedgerName"];
                }
            }

            return "ledger " + ALedgerNumber.ToString();
        }
    }
}