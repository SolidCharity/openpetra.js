//
// DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
//
// @Authors:
//       timop
//       Tim Ingham
//
// Copyright 2004-2012
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
using System.Collections.Generic;
using System.Windows.Forms;
using GNU.Gettext;
using Ict.Common;
using Ict.Common.Data;
using Ict.Common.Conversion;
using Ict.Common.Verification;
using Ict.Petra.Client.App.Core.RemoteObjects;
using Ict.Petra.Client.MFinance.Logic;
using Ict.Petra.Client.MFinance.Gui.GL;
using Ict.Petra.Shared;
using Ict.Petra.Shared.MPartner;
using Ict.Petra.Shared.MFinance.Account.Data;
using Ict.Petra.Shared.MFinance.AP.Data;
using Ict.Petra.Shared.MFinance;
using Ict.Petra.Client.MReporting.Gui.MFinance;

namespace Ict.Petra.Client.MFinance.Gui.AP
{
    public partial class TFrmAPPayment
    {
        AccountsPayableTDS FMainDS = null;
        Int32 FLedgerNumber = -1;
        ALedgerRow FLedgerRow = null;
        AccountsPayableTDSAApPaymentRow FSelectedPaymentRow = null;
        AccountsPayableTDSAApDocumentPaymentRow FSelectedDocumentRow = null;


        private void RunOnceOnActivationManual()
        {
            rbtPayFullOutstandingAmount.CheckedChanged += new EventHandler(EnablePartialPayment);
            chkClaimDiscount.Visible = false;
            txtExchangeRate.TextChanged += new EventHandler(UpdateTotalAmount);
        }

        private void ShowDataManual()
        {
            TFinanceControls.InitialiseAccountList(ref cmbBankAccount, FMainDS.AApDocument[0].LedgerNumber, true, false, true, true, "");

//          grdDetails.AddTextColumn("AP No", FMainDS.AApDocumentPayment.ColumnApNumber, 50);
            grdDetails.AddTextColumn("Invoice No", FMainDS.AApDocumentPayment.ColumnDocumentCode, 80);
            grdDetails.AddTextColumn("Type", FMainDS.AApDocumentPayment.ColumnDocType, 80);
//          grdDetails.AddTextColumn("Discount used", FMainDS.AApDocumentPayment.ColumnUseDiscount, 80);
            grdDetails.AddCurrencyColumn("Amount", FMainDS.AApDocumentPayment.ColumnAmount);
//          grdDetails.AddTextColumn("Currency", FMainDS.AApPayment.ColumnCurrencyCode, 50);  // There's no currencyCode in DocumentPayment!

            grdPayments.AddTextColumn("Supplier", FMainDS.AApPayment.ColumnListLabel);

            FMainDS.AApDocumentPayment.DefaultView.AllowNew = false;
            FMainDS.AApDocumentPayment.DefaultView.AllowEdit = false;
            FMainDS.AApPayment.DefaultView.AllowNew = false;
            grdPayments.DataSource = new DevAge.ComponentModel.BoundDataView(FMainDS.AApPayment.DefaultView);
            grdPayments.Refresh();
            grdPayments.Selection.SelectRow(1, true);
            FocusedRowChanged(null, null);

            // If this payment has a payment number, it's because it's already been paid, so I need to display it read-only.
            if ((FMainDS.AApPayment.Rows.Count > 0) && (FMainDS.AApPayment[0].PaymentNumber > 0))
            {
                txtAmountToPay.Enabled = false;
                txtChequeNumber.Enabled = false;
                txtCurrency.Enabled = false;
                txtExchangeRate.Enabled = false;
                txtExchangeRate.NumberValueDecimal = FMainDS.AApPayment[0].ExchangeRateToBase;

                txtReference.Enabled = false;
                txtTotalAmount.Enabled = false;
                cmbBankAccount.Enabled = false;
                cmbPaymentType.Enabled = false;

                grdDetails.Enabled = false;
                grdPayments.Enabled = false;

                tbbMakePayment.Enabled = false;

                rgrAmountToPay.Enabled = false;
                tbbPrintReport.Enabled = true;
                chkPrintRemittance.Enabled = true;
                chkClaimDiscount.Enabled = false;
                chkPrintCheque.Enabled = false;
                chkPrintLabel.Enabled = false;
            }
            else
            {
                tbbPrintReport.Enabled = false;
            }
        }

        /// <summary>
        /// Set which payments should be paid; initialises the data of this screen
        /// </summary>
        /// <param name="ADataset"></param>
        /// <param name="ALedgerNumber"></param>
        /// <param name="ADocumentsToPay"></param>
        public void AddDocumentsToPayment(AccountsPayableTDS ADataset, Int32 ALedgerNumber, List <Int32>ADocumentsToPay)
        {
            FMainDS = ADataset;
            FLedgerNumber = ALedgerNumber;

            if (FMainDS.AApPayment == null)
            {
                FMainDS.Merge(new AccountsPayableTDSAApPaymentTable()); // Because of these lines, AddDocumentsToPayment may only be called once per payment.
            }
            else
            {
                FMainDS.AApPayment.Clear();
            }

            if (FMainDS.AApDocumentPayment == null)
            {
                FMainDS.Merge(new AccountsPayableTDSAApDocumentPaymentTable());
            }
            else
            {
                FMainDS.AApDocumentPayment.Clear();
            }

            TRemote.MFinance.AP.WebConnectors.CreatePaymentTableEntries(ref FMainDS, ALedgerNumber, ADocumentsToPay);
            chkPrintRemittance.Checked = true;
            chkClaimDiscount.Enabled = false;
            chkPrintCheque.Enabled = false;
            chkPrintLabel.Enabled = false;
            ShowDataManual();
        }

        private void UpdateTotalAmount(Object sender, EventArgs e)
        {
            txtTotalAmount.NumberValueDecimal = FSelectedPaymentRow.Amount;

            if (txtExchangeRate.NumberValueDecimal.HasValue)
            {
                Decimal ExchangeRate = txtExchangeRate.NumberValueDecimal.Value;

                if (ExchangeRate != 0)
                {
                    FSelectedPaymentRow.ExchangeRateToBase = ExchangeRate;
                    txtBaseAmount.NumberValueDecimal = FSelectedPaymentRow.Amount / FSelectedPaymentRow.ExchangeRateToBase;
                }
            }
        }

        private void CalculateTotalPayment()
        {
            FMainDS.AApDocumentPayment.DefaultView.RowFilter = String.Format("{0}={1}",
                AApDocumentPaymentTable.GetPaymentNumberDBName(), FSelectedPaymentRow.PaymentNumber);

            FSelectedPaymentRow.Amount = 0m;

            foreach (DataRowView rv in FMainDS.AApDocumentPayment.DefaultView)
            {
                AccountsPayableTDSAApDocumentPaymentRow DocPaymentRow = (AccountsPayableTDSAApDocumentPaymentRow)rv.Row;
                FSelectedPaymentRow.Amount += DocPaymentRow.Amount;
            }

            UpdateTotalAmount(null, null);
        }

        private void EnablePartialPayment(object sender, EventArgs e)
        {
            //
            // If this invoice is already partpaid, the outstanding amount box
            // should show the amount remaining to be paid.
            //
            txtAmountToPay.Enabled = rbtPayAPartialAmount.Checked;

            FSelectedDocumentRow.PayFullInvoice = rbtPayFullOutstandingAmount.Checked;

            if (rbtPayFullOutstandingAmount.Checked)
            {
                FSelectedDocumentRow.Amount = FSelectedDocumentRow.InvoiceTotal;
            }

            txtAmountToPay.Text = FSelectedDocumentRow.Amount.ToString("N2");
            CalculateTotalPayment();
        }

        private void FocusedRowChanged(System.Object sender, SourceGrid.RowEventArgs e)
        {
            DataRowView[] SelectedGridRow = grdPayments.SelectedDataRowsAsDataRowView;

            if (SelectedGridRow.Length >= 1)
            {
                FSelectedPaymentRow = (AccountsPayableTDSAApPaymentRow)SelectedGridRow[0].Row;

                if (!FSelectedPaymentRow.IsSupplierKeyNull())
                {
                    AApSupplierRow supplier = TFrmAPMain.GetSupplier(FMainDS.AApSupplier, FSelectedPaymentRow.SupplierKey);
                    txtCurrency.Text = supplier.CurrencyCode;
                    ALedgerTable Tbl = TRemote.MFinance.AP.WebConnectors.GetLedgerInfo(FLedgerNumber);
                    FLedgerRow = Tbl[0];

                    decimal CurrentRate = TExchangeRateCache.GetDailyExchangeRate(supplier.CurrencyCode, FLedgerRow.BaseCurrency, DateTime.Now);
                    txtExchangeRate.NumberValueDecimal = CurrentRate;
                }

                cmbBankAccount.SetSelectedString(FSelectedPaymentRow.BankAccount);

                FMainDS.AApDocumentPayment.DefaultView.RowFilter = AccountsPayableTDSAApDocumentPaymentTable.GetPaymentNumberDBName() +
                                                                   " = " + FSelectedPaymentRow.PaymentNumber.ToString();

                grdDetails.DataSource = new DevAge.ComponentModel.BoundDataView(FMainDS.AApDocumentPayment.DefaultView);
                grdDetails.Refresh();
                grdDetails.Selection.SelectRow(1, true);
                FocusedRowChangedDetails(null, null);
            }
        }

        private void FocusedRowChangedDetails(System.Object sender, SourceGrid.RowEventArgs e)
        {
            DataRowView[] SelectedGridRow = grdDetails.SelectedDataRowsAsDataRowView;

            if (FSelectedDocumentRow != null)  // unload amount to pay into currently selected record
            {
                FSelectedDocumentRow.Amount = Decimal.Parse(txtAmountToPay.Text);
            }

            FSelectedDocumentRow = (AccountsPayableTDSAApDocumentPaymentRow)SelectedGridRow[0].Row;
            rbtPayFullOutstandingAmount.Checked = FSelectedDocumentRow.PayFullInvoice;
            rbtPayAPartialAmount.Checked = !rbtPayFullOutstandingAmount.Checked;

            EnablePartialPayment(null, null);
        }

        private void PrintPaymentReport(object sender, EventArgs e)
        {
            //
            // I need to find the min and max payment numbers, which have been returned from PostAPPayments..
            //
            Int32 MinPaymentNumber = FMainDS.AApPayment[0].PaymentNumber;
            Int32 MaxPaymentNumber = MinPaymentNumber;

            foreach (AccountsPayableTDSAApPaymentRow PaymentRow in FMainDS.AApPayment.Rows)
            {
                if (PaymentRow.PaymentNumber < MinPaymentNumber)
                {
                    MinPaymentNumber = PaymentRow.PaymentNumber;
                }

                if (PaymentRow.PaymentNumber > MaxPaymentNumber)
                {
                    MaxPaymentNumber = PaymentRow.PaymentNumber;
                }
            }

            Int32 LedgerNumber = FMainDS.AApPayment[0].LedgerNumber;

            // Print Payment report..
            TFrmAP_PaymentReport.CreateReportNoGui(LedgerNumber, MinPaymentNumber, MaxPaymentNumber, this);
        }

        private void PrintRemittanceAdvice()
        {
            if (chkPrintRemittance.Checked)
            {
                TFrmAP_RemittanceAdvice PreviewFrame = new TFrmAP_RemittanceAdvice(this);
                PreviewFrame.Show();
                PreviewFrame.PrintRemittanceAdvice(FMainDS.AApPayment[0].PaymentNumber, FMainDS.AApPayment[0].LedgerNumber);
            }
        }

        private void MakePayment(object sender, EventArgs e)
        {
            FSelectedDocumentRow.Amount = Decimal.Parse(txtAmountToPay.Text);
            FSelectedPaymentRow.BankAccount = cmbBankAccount.GetSelectedString();
            AccountsPayableTDSAApPaymentTable AApPayment = FMainDS.AApPayment;

            //
            // I want to check whether the user is paying more than the due amount on any of these payments...
            //
            foreach (AccountsPayableTDSAApPaymentRow PaymentRow in AApPayment.Rows)
            {
                FMainDS.AApDocumentPayment.DefaultView.RowFilter = String.Format("{0}={1}",
                    AApDocumentPaymentTable.GetPaymentNumberDBName(), PaymentRow.PaymentNumber);

                foreach (DataRowView rv in FMainDS.AApDocumentPayment.DefaultView)
                {
                    AccountsPayableTDSAApDocumentPaymentRow DocPaymentRow = (AccountsPayableTDSAApDocumentPaymentRow)rv.Row;

                    if (DocPaymentRow.Amount > DocPaymentRow.InvoiceTotal)
                    {
                        String strMessage =
                            String.Format(Catalog.GetString(
                                    "Payment of {0:n2} {1} to {2} is more than the due amount.\r\nPress OK to accept this amount."),
                                DocPaymentRow.Amount, PaymentRow.CurrencyCode, PaymentRow.SupplierName);

                        if (System.Windows.Forms.MessageBox.Show(strMessage, Catalog.GetString("OverPayment"), MessageBoxButtons.OKCancel)
                            == DialogResult.Cancel)
                        {
                            return;
                        }
                    }
                }
            }

            TDlgGLEnterDateEffective dateEffectiveDialog = new TDlgGLEnterDateEffective(
                FMainDS.AApDocument[0].LedgerNumber,
                Catalog.GetString("Select payment date"),
                Catalog.GetString("The date effective for the payment") + ":");

            if (dateEffectiveDialog.ShowDialog() != DialogResult.OK)
            {
                MessageBox.Show(Catalog.GetString("The payment was cancelled."), Catalog.GetString(
                        "No Success"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DateTime PaymentDate = dateEffectiveDialog.SelectedDate;
            TVerificationResultCollection Verifications;

            if (!TRemote.MFinance.AP.WebConnectors.PostAPPayments(
                    ref FMainDS,
                    PaymentDate,
                    out Verifications))
            {
                string ErrorMessages = String.Empty;

                foreach (TVerificationResult verif in Verifications)
                {
                    ErrorMessages += "[" + verif.ResultContext + "] " +
                                     verif.ResultTextCaption + ": " +
                                     verif.ResultText + Environment.NewLine;
                }

                System.Windows.Forms.MessageBox.Show(ErrorMessages, Catalog.GetString("Payment failed"));
            }
            else
            {
                PrintPaymentReport(sender, e);
                PrintRemittanceAdvice();

                // TODO: show posting register of GL Batch?

                // After the payments screen, The status of this document may have changed.

                Form Opener = FPetraUtilsObject.GetCallerForm();

                if (Opener.GetType() == typeof(TFrmAPSupplierTransactions))
                {
                    ((TFrmAPSupplierTransactions)Opener).Reload();
                }

                Close();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="ALedgerNumber"></param>
        /// <param name="APaymentNumber"></param>
        public void ReloadPayment(Int32 ALedgerNumber, Int32 APaymentNumber)
        {
            FMainDS = TRemote.MFinance.AP.WebConnectors.LoadAPPayment(ALedgerNumber, APaymentNumber);
            FLedgerNumber = FMainDS.AApPayment[0].LedgerNumber;
            ShowDataManual();
        }

        /// <summary>
        /// A payment made to a supplier needs to be reversed.
        /// It's done by creating and posting a set of matching "negatives" -
        /// In the simplest case this is a single credit note matching an invoice
        /// but it could be more complex. These negative documents are payed using
        /// a standard call to PostAPPayments.
        ///
        /// After the reversal, I'll also create and post new copies of all
        /// the invoices / credit notes that made up the original payment.
        /// </summary>
        /// <param name="ALedgerNumber"></param>
        /// <param name="APaymentNumber"></param>
        public void ReversePayment(Int32 ALedgerNumber, Int32 APaymentNumber)
        {
            AccountsPayableTDS TempDS = TRemote.MFinance.AP.WebConnectors.LoadAPPayment(ALedgerNumber, APaymentNumber);

            if (TempDS.AApPayment.Rows.Count == 0) // Invalid Payment number?
            {
                MessageBox.Show(Catalog.GetString("The referenced payment Connot be loaded."), Catalog.GetString("Error"));
                return;
            }

            TempDS.AApDocument.DefaultView.Sort = AApDocumentTable.GetApDocumentIdDBName();

            //
            // First I'll check that the amounts add up:
            //
            Decimal PaidDocumentsTotal = 0.0m;

            foreach (AApDocumentPaymentRow PaymentRow in TempDS.AApDocumentPayment.Rows)
            {
                Int32 DocIdx = TempDS.AApDocument.DefaultView.Find(PaymentRow.ApDocumentId);
                AApDocumentRow DocumentRow = TempDS.AApDocument[DocIdx];

                if (DocumentRow.CreditNoteFlag)
                {
                    PaidDocumentsTotal -= DocumentRow.TotalAmount;
                }
                else
                {
                    PaidDocumentsTotal += DocumentRow.TotalAmount;
                }
            }

            //
            // If this is a partial payment, I can't deal with that here...
            //
            if (PaidDocumentsTotal != TempDS.AApPayment[0].Amount)
            {
                String ErrorMsg =
                    String.Format(Catalog.GetString(
                            "This Payment cannot be reversed automatically because the total amount of the referenced documents ({0:n2} {1}) differs from the amount in the payment ({2:n2} {3})."),
                        PaidDocumentsTotal, TempDS.AApSupplier[0].CurrencyCode, TempDS.AApPayment[0].Amount, TempDS.AApSupplier[0].CurrencyCode);
                MessageBox.Show(ErrorMsg, Catalog.GetString("Reverse Payment"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Find out if this payment was already reversed,
            // because if it was, perhaps the user doesn't really want to
            // reverse it again?
            if (TRemote.MFinance.AP.WebConnectors.WasThisPaymentReversed(ALedgerNumber, APaymentNumber))
            {
                MessageBox.Show(Catalog.GetString("Cannot reverse Payment - there is already a matching reverse transaction."),
                    Catalog.GetString("Reverse Payment"), MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            //
            // Ask the user to confirm reversal of this payment
            //
            String PaymentMsg = Catalog.GetString("Do you want to reverse this payment?");

            PaymentMsg += ("\r\n" + String.Format("Payment made {0} to {1}\r\n\r\nRelated invoices:",
                               TDate.DateTimeToLongDateString2(TempDS.AApPayment[0].PaymentDate.Value), TempDS.PPartner[0].PartnerShortName));

            foreach (AApDocumentPaymentRow PaymentRow in TempDS.AApDocumentPayment.Rows)
            {
                Int32 DocIdx = TempDS.AApDocument.DefaultView.Find(PaymentRow.ApDocumentId);
                AApDocumentRow DocumentRow = TempDS.AApDocument[DocIdx];
                PaymentMsg += ("\r\n" + String.Format("     {2} ({3})  {0:n2} {1}",
                                   DocumentRow.TotalAmount, TempDS.AApSupplier[0].CurrencyCode, DocumentRow.DocumentCode, DocumentRow.Reference));
            }

            PaymentMsg += ("\r\n\r\n" + String.Format("Total payment {0:n2} {1}", TempDS.AApPayment[0].Amount, TempDS.AApSupplier[0].CurrencyCode));
            DialogResult YesNo = MessageBox.Show(PaymentMsg, Catalog.GetString("Reverse Payment"), MessageBoxButtons.YesNo);

            if (YesNo == DialogResult.No)
            {
                return;
            }

            TDlgGLEnterDateEffective dateEffectiveDialog = new TDlgGLEnterDateEffective(
                ALedgerNumber,
                Catalog.GetString("Select posting date"),
                Catalog.GetString("The date effective for the reversal") + ":");

            if (dateEffectiveDialog.ShowDialog() != DialogResult.OK)
            {
                MessageBox.Show(Catalog.GetString("Reversal was cancelled."), Catalog.GetString("Reverse Payment"),
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            DateTime PostingDate = dateEffectiveDialog.SelectedDate;
            TVerificationResultCollection Verifications;

            if (TRemote.MFinance.AP.WebConnectors.ReversePayment(ALedgerNumber, APaymentNumber, PostingDate, out Verifications))
            {
                // TODO: print reports on successfully posted batch
                MessageBox.Show(Catalog.GetString("The AP payment has been reversed."), Catalog.GetString("Reverse Payment"));
                Form Opener = FPetraUtilsObject.GetCallerForm();

                if (Opener.GetType() == typeof(TFrmAPSupplierTransactions))
                {
                    ((TFrmAPSupplierTransactions)Opener).Reload();
                }
            }
            else
            {
                string ErrorMessages = String.Empty;

                foreach (TVerificationResult verif in Verifications)
                {
                    ErrorMessages += "[" + verif.ResultContext + "] " +
                                     verif.ResultTextCaption + ": " +
                                     verif.ResultText + Environment.NewLine;
                }

                System.Windows.Forms.MessageBox.Show(ErrorMessages, Catalog.GetString("Reverse Payment Failed"));
            }
        }
    }
}