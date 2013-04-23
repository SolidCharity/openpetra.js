//
// DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
//
// @Authors:
//       timop
//       Tim Ingham
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
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Threading;
using Ict.Petra.Shared;
using System.Resources;
using System.Collections.Specialized;
using GNU.Gettext;
using Ict.Common;
using Ict.Common.Controls;
using Ict.Common.Conversion;
using Ict.Petra.Client.App.Core;
using Ict.Petra.Client.CommonForms;
using Ict.Petra.Client.MPartner.Gui;
using Ict.Petra.Shared.MPartner;
using Ict.Petra.Client.App.Core.RemoteObjects;
using Ict.Petra.Shared.Interfaces.MFinance;
using Ict.Petra.Shared.MFinance.AP.Data;
using Ict.Petra.Shared.MPartner.Partner.Data;
using Ict.Petra.Shared.MFinance.Account.Data;
using System.Globalization;
using System.Timers;
using System.Collections.Generic;
using Ict.Common.Verification;
using Ict.Common.Remoting.Client;
using Ict.Petra.Client.MFinance.Gui.GL;
using Ict.Petra.Client.MFinance.Logic;

namespace Ict.Petra.Client.MFinance.Gui.AP
{
    public partial class TFrmAPMain
    {
        private IAPUIConnectorsFind FSupplierFindObject = null;
        private IAPUIConnectorsFind FInvoiceFindObject = null;
        private bool FKeepUpSearchFinishedCheck = false;
        private bool FSearchForSuppliers = false;

        /// <summary>DataTables that holds all Pages of data (also empty ones that are not retrieved yet!)</summary>
        private DataTable FSupplierTable;
        private AccountsPayableGUITDSInvoiceListTable FInvoiceTable;
        private ALedgerRow FLedgerInfo;

        private Int32 FLedgerNumber;

        /// <summary>
        /// AP is opened in this ledger
        /// </summary>
        public Int32 LedgerNumber
        {
            set
            {
                FLedgerNumber = value;
                FSupplierFindObject = TRemote.MFinance.AP.UIConnectors.Find();
                // Register Object with the TEnsureKeepAlive Class so that it doesn't get GC'd
                TEnsureKeepAlive.Register(FSupplierFindObject);


                FInvoiceFindObject = TRemote.MFinance.AP.UIConnectors.Find();
                // Register Object with the TEnsureKeepAlive Class so that it doesn't get GC'd
                TEnsureKeepAlive.Register(FInvoiceFindObject);


                ALedgerTable Tbl = FSupplierFindObject.GetLedgerInfo(FLedgerNumber);
                FLedgerInfo = Tbl[0];

                this.Text += " - " + TFinanceControls.GetLedgerNumberAndName(FLedgerNumber);

                // Now I've got a ledger number, I can set up the menu and toolbar.
                TabChange(null, null);
            }
        }

        private String GetLedgerCurrency(Int32 ALedgerNumber)
        {
            return FLedgerInfo.BaseCurrency;
        }

        /// <summary>
        /// Search button was clicked
        /// </summary>
        public void DoSearch(object sender, EventArgs e)
        {
            if (FKeepUpSearchFinishedCheck)
            {
                // don't run several searches at the same time
                return;
            }

            FSearchForSuppliers = tpgSuppliers.Visible;

            DataTable CriteriaTable = new DataTable();
            CriteriaTable.Columns.Add("LedgerNumber", typeof(Int32));
            CriteriaTable.Columns.Add("SupplierId", typeof(string));
            CriteriaTable.Columns.Add("DaysPlus", typeof(decimal));

            decimal DaysPlus = -1;

            if (chkDueFuture.Checked)  // Calculate the future date to send to the server
            {
                DaysPlus = nudNumberTimeUnits.Value;

                if (cmbTimeUnit.SelectedText == "Months")
                {
                    DaysPlus *= 31;
                }
                else if (cmbTimeUnit.SelectedText == "Weeks")
                {
                    DaysPlus *= 7;
                }
            }
            else if (chkDueToday.Checked)
            {
                DaysPlus = 0;
            }

            DataRow row = CriteriaTable.NewRow();
            row["DaysPlus"] = DaysPlus;
            row["SupplierId"] = cmbSupplierCode.Text;
            row["LedgerNumber"] = FLedgerNumber;
            CriteriaTable.Rows.Add(row);

            // Start the asynchronous search operation on the PetraServer
            if (FSearchForSuppliers)
            {
                grdSupplierResult.DataSource = null;
                FSupplierTable = null;
                FSupplierFindObject.FindSupplier(CriteriaTable);
            }
            else
            {
                grdInvoiceResult.DataSource = null;
                FInvoiceTable = null;
                FInvoiceFindObject.FindInvoices(CriteriaTable);
            }

            // Start thread that checks for the end of the search operation on the PetraServer
            FKeepUpSearchFinishedCheck = true;
            Thread FinishedCheckThread = new Thread(new ThreadStart(SearchFinishedCheckThread));
            FinishedCheckThread.Start();
        }

        private delegate void SimpleDelegate();

        /// <summary>
        /// Thread for the search operation. Monitor's the Server System.Object's
        /// AsyncExecProgress.ProgressState and invokes UI updates from that.
        ///
        /// </summary>
        /// <returns>void</returns>
        private void SearchFinishedCheckThread()
        {
            // Check whether this thread should still execute
            while (FKeepUpSearchFinishedCheck)
            {
                TAsyncExecProgressState ThreadStatus;

                if (FSearchForSuppliers)
                {
                    ThreadStatus = FSupplierFindObject.AsyncExecProgress.ProgressState;
                }
                else
                {
                    ThreadStatus = FInvoiceFindObject.AsyncExecProgress.ProgressState;
                }

                /* The next line of code calls a function on the PetraServer
                 * > causes a bit of data traffic everytime! */
                switch (ThreadStatus)
                {
                    case TAsyncExecProgressState.Aeps_Finished:
                        FKeepUpSearchFinishedCheck = false;

                        // see also http://stackoverflow.com/questions/6184/how-do-i-make-event-callbacks-into-my-win-forms-thread-safe
                        if (InvokeRequired)
                        {
                            Invoke(new SimpleDelegate(FinishThread));
                        }
                        else
                        {
                            FinishThread();
                        }

                        break;

                    case TAsyncExecProgressState.Aeps_Stopped:
                        FKeepUpSearchFinishedCheck = false;
                        EnableDisableUI(true);
                        return;
                }

                // Sleep a bit, then loop...
                Thread.Sleep(200);
            }

            EnableDisableUI(true);
        }

        private void InitialiseGrid()
        {
            if (tpgSuppliers.Visible)
            {
                grdSupplierResult.Columns.Clear();
                grdSupplierResult.AddTextColumn("Supplier Key", FSupplierTable.Columns[0], 90);
                grdSupplierResult.AddTextColumn("Supplier Name", FSupplierTable.Columns[1], 150);
                grdSupplierResult.AddTextColumn("Currency", FSupplierTable.Columns[2], 85);
            }
            else
            {
                grdInvoiceResult.Columns.Clear();
                grdInvoiceResult.AddCheckBoxColumn("", FInvoiceTable.ColumnSelected, 20, false);
                grdInvoiceResult.AddTextColumn("AP#", FInvoiceTable.ColumnApNumber, 55);
                grdInvoiceResult.AddTextColumn("Inv#", FInvoiceTable.ColumnDocumentCode, 90);
                grdInvoiceResult.AddTextColumn("Supplier", FInvoiceTable.ColumnPartnerShortName, 150);
                grdInvoiceResult.AddCurrencyColumn("Amount", FInvoiceTable.ColumnTotalAmount, 2);
                grdInvoiceResult.AddCurrencyColumn("Outstanding", FInvoiceTable.ColumnOutstandingAmount, 2);
                grdInvoiceResult.AddTextColumn("Currency", FInvoiceTable.ColumnCurrencyCode, 70);
                grdInvoiceResult.AddDateColumn("Due Date", FInvoiceTable.ColumnDateDue);
                grdInvoiceResult.AddTextColumn("Status", FInvoiceTable.ColumnDocumentStatus, 100);
                grdInvoiceResult.AddDateColumn("Issued", FInvoiceTable.ColumnDateIssued);
//              grdInvoiceResult.AddTextColumn("Discount", FInvoiceTable.ColumnDiscountMsg, 150);

                grdInvoiceResult.Columns[4].Width = 90;  // Only the text columns can have their widths set while
                grdInvoiceResult.Columns[5].Width = 90;  // they're being added.
                grdInvoiceResult.Columns[7].Width = 110; // For these currency and date columns,
                grdInvoiceResult.Columns[9].Width = 110; // I need to set the width afterwards. (THIS WILL GO WONKY IF EXTRA FIELDS ARE ADDED ABOVE.)

                grdInvoiceResult.MouseClick += new MouseEventHandler(grdInvoiceResult_Click);
            }
        }

        // Called from a timer, below, so that the default processing of
        // the grid control completes before I get called.
        private void RefreshSumTagged(Object Sender, EventArgs e)
        {
            // If I was called from a timer, kill that now:
            if (Sender != null)
            {
                ((System.Windows.Forms.Timer)Sender).Stop();
            }

            // Add up all the selected Items  ** I can only sum items that are in my currency! **
            String MyCurrency = GetLedgerCurrency(FLedgerNumber);

            Int32 CountTaggedDocuments = 0;
            bool TaggedInvoicesPostable = false;
            bool TaggedInvoicesPayable = false;
            bool TaggedInvoicesReversable = false;
            bool TaggedInvoicesDeletable = false;
            Decimal TotalSelected = 0;
            bool ListHasItems = false;

            //grdInvoiceResult.PagedDataTable
            if (grdInvoiceResult.IsInitialised) // I may be called before the first search.
            {
                if (grdInvoiceResult.PagedDataTable.Rows.Count > 0)
                {
                    ListHasItems = true;
                }

                foreach (DataRow Row in grdInvoiceResult.PagedDataTable.Rows)
                {
                    if (Row["Selected"].Equals(true))
                    {
                        if (Row["CurrencyCode"].Equals(MyCurrency))
                        {
                            if (Row["CreditNoteFlag"].Equals(true))
                            {
                                TotalSelected -= (Decimal)(Row["TotalAmount"]);
                            }
                            else
                            {
                                TotalSelected += (Decimal)(Row["TotalAmount"]);
                            }
                        }

                        String BarStatus = "|" + Row["DocumentStatus"].ToString() + "|";

                        //
                        // While I'm in this loop, I'll also check whether to enable the "Pay", "Post", "Reverse" and "Delete" buttons.
                        //

                        CountTaggedDocuments++;

                        if ("|POSTED|PARTPAID|".IndexOf(BarStatus) >= 0)
                        {
                            TaggedInvoicesPayable = true;
                        }

                        if ("|POSTED|PARTPAID|PAID|".IndexOf(BarStatus) < 0)
                        {
                            TaggedInvoicesPostable = true;
                            TaggedInvoicesDeletable = true;
                        }

                        if ("|POSTED" == BarStatus)
                        {
                            TaggedInvoicesReversable = true;
                        }
                    }
                }
            }

            txtSumTagged.Text = TotalSelected.ToString("n2") + " " + MyCurrency;

            ActionEnabledEvent(null, new ActionEventArgs("actOpenTagged", (CountTaggedDocuments > 0)));
            ActionEnabledEvent(null, new ActionEventArgs("actPayTagged", TaggedInvoicesPayable));
            ActionEnabledEvent(null, new ActionEventArgs("actPostTagged", TaggedInvoicesPostable));
            ActionEnabledEvent(null, new ActionEventArgs("actReverseTagged", TaggedInvoicesReversable));
            ActionEnabledEvent(null, new ActionEventArgs("actDeleteTagged", TaggedInvoicesDeletable));
            ActionEnabledEvent(null, new ActionEventArgs("actTagAllPostable", ListHasItems));
            ActionEnabledEvent(null, new ActionEventArgs("actTagAllPayable", ListHasItems));
            ActionEnabledEvent(null, new ActionEventArgs("actUntagAll", ListHasItems));
        }

        private void grdInvoiceResult_Click(object sender, EventArgs e)
        {
            // I want to update the total tagged field,
            // but it needs to be performed AFTER the default processing so I'm using a timer.
            System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

            timer.Tick += new EventHandler(RefreshSumTagged);
            timer.Interval = 100;
            timer.Start();
        }

        private void FinishThread()
        {
            // Fetch the first page of data
            try
            {
                if (FSearchForSuppliers)
                {
                    grdSupplierResult.LoadFirstDataPage(@GetDataPagedResult);
                }
                else
                {
                    grdInvoiceResult.LoadFirstDataPage(@GetDataPagedResult);
                }
            }
            catch (Exception E)
            {
                MessageBox.Show(E.ToString());
                return;
            }
            InitialiseGrid();

            if (FSearchForSuppliers)
            {
                DataView myDataView = FSupplierTable.DefaultView;
                myDataView.AllowNew = false;
                grdSupplierResult.DataSource = new DevAge.ComponentModel.BoundDataView(myDataView);
                grdSupplierResult.Visible = true;
                SetSupplierFilters(null, null);

                if (grdSupplierResult.TotalPages > 0)
                {
                    grdSupplierResult.BringToFront();

                    // Highlight first Row
                    grdSupplierResult.Selection.SelectRow(1, true);

                    // Make the Grid respond on updown keys
                    grdSupplierResult.Focus();
                }

                ActionEnabledEvent(null, new ActionEventArgs("cndSelectedSupplier", grdSupplierResult.TotalPages > 0));
            }
            else
            {
                DataView myDataView = grdInvoiceResult.PagedDataTable.DefaultView;
                myDataView.AllowNew = false;
                grdInvoiceResult.DataSource = new DevAge.ComponentModel.BoundDataView(myDataView);
                grdInvoiceResult.Visible = true;

                if (grdInvoiceResult.TotalPages > 0)
                {
                    grdInvoiceResult.BringToFront();

                    // Highlight first Row
                    grdInvoiceResult.Selection.SelectRow(1, true);

                    // Make the Grid respond on updown keys
                    grdInvoiceResult.Focus();
                }
            }

            TabChange(null, null);
        }

        private void InitializeManualCode()
        {
            this.cmbSupplierCurrency.cmbCombobox.TextChanged += new System.EventHandler(this.SetSupplierFilters);
        }

        private void EnableDisableUI(bool AEnable)
        {
            // TODO: autogenerate?
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="ANeededPage"></param>
        /// <param name="APageSize"></param>
        /// <param name="ATotalRecords"></param>
        /// <param name="ATotalPages"></param>
        /// <returns></returns>
        private DataTable GetDataPagedResult(Int16 ANeededPage, Int16 APageSize, out Int32 ATotalRecords, out Int16 ATotalPages)
        {
            ATotalRecords = 0;
            ATotalPages = 0;

            if (tpgSuppliers.Visible)
            {
                if (FSupplierFindObject != null)
                {
                    DataTable NewPage = FSupplierFindObject.GetDataPagedResult(ANeededPage, APageSize, out ATotalRecords, out ATotalPages);

                    if (FSupplierTable == null)
                    {
                        FSupplierTable = NewPage;
                    }

                    return NewPage;
                }
            }
            else
            {
                if (FInvoiceFindObject != null)
                {
                    DataTable NewPage = FInvoiceFindObject.GetDataPagedResult(ANeededPage, APageSize, out ATotalRecords, out ATotalPages);

                    if (FInvoiceTable == null)
                    {
                        FInvoiceTable = (AccountsPayableGUITDSInvoiceListTable)NewPage;
                    }

                    return NewPage;
                }
            }

            return null;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="Tbl"></param>
        /// <param name="APartnerKey"></param>
        /// <returns></returns>
        public static AApSupplierRow GetSupplier(AApSupplierTable Tbl, Int64 APartnerKey)
        {
            Tbl.DefaultView.Sort = "p_partner_key_n";

            int indexSupplier = Tbl.DefaultView.Find(APartnerKey);

            if (indexSupplier == -1)
            {
                return null;
            }

            return Tbl[indexSupplier];
        }

        /// <summary>
        /// get the partner key of the currently selected supplier in the grid
        /// </summary>
        /// <returns></returns>
        private Int64 GetCurrentlySelectedSupplier()
        {
            DataRowView[] SelectedGridRow = grdSupplierResult.SelectedDataRowsAsDataRowView;
            Int64 SupplierKey = -1;

            if (SelectedGridRow.Length >= 1)
            {
                Object Cell = SelectedGridRow[0]["PartnerKey"];

                if (Cell.GetType() == typeof(Int64))
                {
                    SupplierKey = Convert.ToInt64(Cell);
                }
            }

            return SupplierKey;
        }

        private Int32 GetCurrentlySelectedDocumentId()
        {
            DataRowView[] SelectedGridRow = grdInvoiceResult.SelectedDataRowsAsDataRowView;
            Int32 InvoiceNum = -1;

            if (SelectedGridRow.Length >= 1)
            {
                Object Cell = SelectedGridRow[0]["DocumentId"];

                if (Cell.GetType() == typeof(Int32))
                {
                    InvoiceNum = Convert.ToInt32(Cell);
                }
            }

            return InvoiceNum;
        }

        /// <summary>
        /// open the transactions of the selected supplier
        /// </summary>
        public void SupplierTransactions(object sender, EventArgs e)
        {
            Int64 SelectedSupplier = GetCurrentlySelectedSupplier();

            if (SelectedSupplier != -1)
            {
                TFrmAPSupplierTransactions frm = new TFrmAPSupplierTransactions(this);

                frm.LoadSupplier(FLedgerNumber, SelectedSupplier);
                frm.Show();
            }
        }

        /// <summary>
        /// Open the selected invoice
        /// </summary>
        public void ShowInvoice(object sender, EventArgs e)
        {
            Int32 SelectedInvoice = GetCurrentlySelectedDocumentId();

            if (SelectedInvoice > 0)
            {
                TFrmAPEditDocument frm = new TFrmAPEditDocument(this);

                if (frm.LoadAApDocument(FLedgerNumber, SelectedInvoice))
                {
                    frm.Show();
                }
            }
        }

        /// <summary>
        /// create a new supplier
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void NewSupplier(object sender, EventArgs e)
        {
            Int64 PartnerKey = -1;
            String ResultStringLbl;
            TLocationPK ResultLocationPK;

            // the user has to select an existing partner to make that partner a supplier
            if (TPartnerFindScreenManager.OpenModalForm("",
                    out PartnerKey,
                    out ResultStringLbl,
                    out ResultLocationPK,
                    this))
            {
                TFrmAPEditSupplier frm = new TFrmAPEditSupplier(this);
                frm.LedgerNumber = FLedgerNumber;
                frm.CreateNewSupplier(PartnerKey);
                frm.Show();
            }
        }

        /// <summary>
        /// edit an existing supplier
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void EditSupplier(object sender, EventArgs e)
        {
            Int64 PartnerKey = GetCurrentlySelectedSupplier();

            if (PartnerKey != -1)
            {
                TFrmAPEditSupplier frm = new TFrmAPEditSupplier(this);
                frm.LedgerNumber = FLedgerNumber;
                frm.EditSupplier(PartnerKey);
                frm.Show();
            }
        }

        /// <summary>
        /// create a new invoice
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateInvoice(object sender, EventArgs e)
        {
            Int64 PartnerKey = GetCurrentlySelectedSupplier();

            if (PartnerKey != -1)
            {
                TFrmAPEditDocument frm = new TFrmAPEditDocument(this);

                frm.CreateAApDocument(FLedgerNumber, PartnerKey, false);
                frm.Show();
            }
        }

        /// <summary>
        /// create a new credit note
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CreateCreditNote(object sender, EventArgs e)
        {
            Int64 PartnerKey = GetCurrentlySelectedSupplier();

            if (PartnerKey != -1)
            {
                TFrmAPEditDocument frm = new TFrmAPEditDocument(this);

                frm.CreateAApDocument(FLedgerNumber, PartnerKey, true);
                frm.Show();
            }
        }

        private void SetSupplierFilters(object sender, EventArgs e)
        {
            String CurrencyRowFilter = "";
            String SupplierActiveRowFilter = "";

            String CurrencyCode = cmbSupplierCurrency.cmbCombobox.Text;

            if (CurrencyCode != "")
            {
                CurrencyRowFilter = String.Format("CurrencyCode='{0}'", CurrencyCode);
            }

            if (chkHideInactiveSuppliers.CheckState == CheckState.Checked)
            {
                SupplierActiveRowFilter = "StatusCode='ACTIVE'";
            }

            String SupplierRowFilter = CurrencyRowFilter;

            if ((CurrencyRowFilter != "") && (SupplierActiveRowFilter != ""))
            {
                SupplierRowFilter += " AND ";
            }

            SupplierRowFilter += SupplierActiveRowFilter;

            if (grdSupplierResult.IsInitialised)
            {
                grdSupplierResult.PagedDataTable.DefaultView.RowFilter = SupplierRowFilter;
            }
        }

        private void TagAllPostable(object sender, EventArgs e)
        {
            foreach (DataRow Row in grdInvoiceResult.PagedDataTable.Rows)
            {
                if ("|POSTED|PARTPAID|PAID|".IndexOf("|" + Row["DocumentStatus"].ToString()) < 0)
                {
                    Row["Selected"] = true;
                }
            }

            RefreshSumTagged(null, null);
        }

        private void TagAllPayable(object sender, EventArgs e)
        {
            foreach (DataRow Row in grdInvoiceResult.PagedDataTable.Rows)
            {
                if ("|POSTED|PARTPAID|".IndexOf("|" + Row["DocumentStatus"].ToString()) >= 0)
                {
                    Row["Selected"] = true;
                }
            }

            RefreshSumTagged(null, null);
        }

        private void UntagAll(object sender, EventArgs e)
        {
            foreach (DataRow Row in grdInvoiceResult.PagedDataTable.Rows)
            {
                Row["Selected"] = false;
            }

            RefreshSumTagged(null, null);
        }

        private AccountsPayableTDS LoadTaggedDocuments()
        {
            AccountsPayableTDS LoadDs = new AccountsPayableTDS();

            foreach (DataRow Row in grdInvoiceResult.PagedDataTable.Rows)
            {
                if (Row["Selected"].Equals(true))
                {
                    LoadDs.Merge(TRemote.MFinance.AP.WebConnectors.LoadAApDocument(FLedgerNumber, (int)Row["DocumentId"]));
                }
            }

            return LoadDs;
        }

        private void ReverseAllTagged(object sender, EventArgs e)
        {
            // I can only reverse invoices that are POSTED.
            List <int>ReverseTheseDocs = new List <int>();

            foreach (DataRow Row in grdInvoiceResult.PagedDataTable.Rows)
            {
                if (Row["Selected"].Equals(true))
                {
                    if ("POSTED" == Row["DocumentStatus"].ToString())
                    {
                        ReverseTheseDocs.Add((int)Row["DocumentId"]);
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show(Catalog.GetString("Only posted documents can be reversed."),
                            Catalog.GetString("Document reversal failed"));
                    }
                }
            }

            if (ReverseTheseDocs.Count > 0)
            {
                TVerificationResultCollection Verifications;
                TDlgGLEnterDateEffective dateEffectiveDialog = new TDlgGLEnterDateEffective(
                    FLedgerNumber,
                    Catalog.GetString("Select reversal date"),
                    Catalog.GetString("The date effective for this reversal") + ":");

                if (dateEffectiveDialog.ShowDialog() != DialogResult.OK)
                {
                    MessageBox.Show(Catalog.GetString("Reversal was cancelled."), Catalog.GetString(
                            "No Success"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                DateTime PostingDate = dateEffectiveDialog.SelectedDate;

                if (TRemote.MFinance.AP.WebConnectors.PostAPDocuments(
                        FLedgerNumber,
                        ReverseTheseDocs,
                        PostingDate,
                        true,
                        out Verifications))
                {
                    System.Windows.Forms.MessageBox.Show("Invoice reversed to Approved status.", Catalog.GetString("Reversal"));
                    DoSearch(null, null);
                    return;
                }
                else
                {
                    string ErrorMessages = Verifications.BuildVerificationResultString();
                    MessageBox.Show(ErrorMessages, Catalog.GetString("Reverse Invoice"));
                }
            }
        }

        private void DeleteAllTagged(object sender, EventArgs e)
        {
            // I can only delete invoices that are not posted already.
            List <int>DeleteTheseDocs = new List <int>();

            foreach (DataRow Row in grdInvoiceResult.PagedDataTable.Rows)
            {
                if (Row["Selected"].Equals(true))
                {
                    if ("|POSTED|PARTPAID|PAID|".IndexOf("|" + Row["DocumentStatus"].ToString()) < 0)
                    {
                        DeleteTheseDocs.Add((int)Row["DocumentId"]);
                    }
                    else
                    {
                        System.Windows.Forms.MessageBox.Show(Catalog.GetString("Can't delete posted documents. Reverse the document first."),
                            Catalog.GetString("Document Deletion failed"));
                    }
                }
            }

            if (DeleteTheseDocs.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show(Catalog.GetString("No tagged invoices can be deleted."),
                    Catalog.GetString("Document Deletion failed"));
                return;
            }

            TVerificationResultCollection Verifications;

            if (TRemote.MFinance.AP.WebConnectors.DeleteAPDocuments(FLedgerNumber, DeleteTheseDocs, out Verifications))
            {
                MessageBox.Show(Catalog.GetString("Document(s) deleted successfully!"));
                DoSearch(null, null);
            }
            else
            {
                string ErrorMessages = Verifications.BuildVerificationResultString();
                MessageBox.Show(ErrorMessages, Catalog.GetString("Document Deletion"));
            }
        }

        private void OpenAllTagged(object sender, EventArgs e)
        {
            foreach (DataRow Row in grdInvoiceResult.PagedDataTable.Rows)
            {
                if (Row["Selected"].Equals(true))
                {
                    TFrmAPEditDocument frm = new TFrmAPEditDocument(this);

                    if (frm.LoadAApDocument(FLedgerNumber, (int)Row["DocumentId"]))
                    {
                        frm.Show();
                    }
                }
            }
        }

        private void PayAllTagged(object sender, EventArgs e)
        {
            AccountsPayableTDS TempDS = LoadTaggedDocuments();
            TFrmAPPayment PaymentScreen = new TFrmAPPayment(this);

            List <int>PayTheseDocs = new List <int>();

            foreach (DataRow Row in grdInvoiceResult.PagedDataTable.Rows)
            {
                if ((Row["Selected"].Equals(true)
                     && ("|POSTED|PARTPAID|".IndexOf("|" + Row["DocumentStatus"].ToString() + "|") >= 0)))
                {
                    PayTheseDocs.Add((int)Row["DocumentId"]);
                }
            }

            if (PayTheseDocs.Count > 0)
            {
                if (PaymentScreen.AddDocumentsToPayment(TempDS, FLedgerNumber, PayTheseDocs))
                {
                    PaymentScreen.Show();
                }
            }
        }

        private void PostAllTagged(object sender, EventArgs e)
        {
            AccountsPayableTDS TempDS = LoadTaggedDocuments();

            List <int>PostTheseDocs = new List <int>();
            TempDS.AApDocument.DefaultView.Sort = AApDocumentDetailTable.GetApDocumentIdDBName();

            foreach (DataRow Row in grdInvoiceResult.PagedDataTable.Rows)
            {
                if ((Row["Selected"].Equals(true) && ("|POSTED|PARTPAID|PAID|".IndexOf(Row["DocumentStatus"].ToString()) < 0)))
                {
                    int DocId = (int)Row["DocumentId"];

                    int RowIdx = TempDS.AApDocument.DefaultView.Find(DocId);

                    if (RowIdx >= 0)
                    {
                        AApDocumentRow DocumentRow = (AApDocumentRow)TempDS.AApDocument.DefaultView[RowIdx].Row;

                        if (TFrmAPEditDocument.ApDocumentCanPost(TempDS, DocumentRow)) // This will produce an message box if there's a problem.
                        {
                            PostTheseDocs.Add(DocId);
                        }
                    }
                }
            }

            if (PostTheseDocs.Count > 0)
            {
                if (TFrmAPEditDocument.PostApDocumentList(TempDS, FLedgerNumber, PostTheseDocs))
                {
                    // TODO: print reports on successfully posted batch
                    MessageBox.Show(Catalog.GetString("The tagged documents have been posted successfully!"));
                    DoSearch(null, null);

                    // TODO: show posting register of GL Batch?
                }
            }
        }

        private void TabChange(object sender, EventArgs e)
        {
            if (tabSearchResult.SelectedTab == tpgOutstandingInvoices)
            {
                mniInvoice.Visible = true;
                mniSupplier.Visible = false;

                tbbEditSupplier.Visible = false;
                tbbTransactions.Visible = false;
                tbbNewSupplier.Visible = false;
                tbbCreateInvoice.Visible = false;
                tbbCreateCreditNote.Visible = false;
                tbbSeparator0.Visible = false;
                tbbSeparator1.Visible = false;
                tbbOpenTagged.Visible = true;
                tbbPostTagged.Visible = true;
                tbbPayTagged.Visible = true;
                tbbReverseTagged.Visible = true;
                pnlInvSearchFilter.Visible = true;
                pnlSupplierSearchFilter.Visible = false;
            }
            else // Suppliers
            {
                mniSupplier.Visible = true;
                mniInvoice.Visible = false;

                tbbEditSupplier.Visible = true;
                tbbTransactions.Visible = true;
                tbbNewSupplier.Visible = true;
                tbbCreateInvoice.Visible = true;
                tbbCreateCreditNote.Visible = true;
                tbbSeparator0.Visible = true;
                tbbSeparator1.Visible = true;
                tbbOpenTagged.Visible = false;
                tbbPostTagged.Visible = false;
                tbbPayTagged.Visible = false;
                tbbReverseTagged.Visible = false;
                pnlInvSearchFilter.Visible = false;
                pnlSupplierSearchFilter.Visible = true;
            }

            RefreshSumTagged(null, null);
        }

        private void Form_Closed(object sender, EventArgs e)
        {
            if (FSupplierFindObject != null)
            {
                // UnRegister Object from the TEnsureKeepAlive Class so that the Object can get GC'd on the PetraServer
                TEnsureKeepAlive.UnRegister(FSupplierFindObject);
                FSupplierFindObject = null;
            }

            if (FInvoiceFindObject != null)
            {
                // UnRegister Object from the TEnsureKeepAlive Class so that the Object can get GC'd on the PetraServer
                TEnsureKeepAlive.UnRegister(FInvoiceFindObject);
                FInvoiceFindObject = null;
            }
        }
    }
}