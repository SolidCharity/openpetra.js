﻿//
// DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
//
// @Authors:
//       wolfgangb
//
// Copyright 2004-2012 by OM International
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
using System.IO;
using System.Windows.Forms;

using GNU.Gettext;
using Ict.Common;
using Ict.Common.Verification;
using Ict.Petra.Client.App.Core;
using Ict.Petra.Client.App.Core.RemoteObjects;
using Ict.Petra.Client.MFinance.Logic;
using Ict.Petra.Client.MFinance.Gui.Setup;
using Ict.Petra.Shared;
using Ict.Petra.Shared.MFinance;
using Ict.Petra.Shared.MFinance.GL.Data;
using Ict.Petra.Shared.MFinance.Account.Data;

namespace Ict.Petra.Client.MFinance.Gui.GL
{
    /// <summary>
    /// Manual code for the GL Batch export
    /// </summary>
    public partial class TFrmRecurringGLBatchSubmit
    {
        /// <summary>
        /// Initialize values
        /// </summary>
        public void InitializeManualCode()
        {
            dtpEffectiveDate.Date = DateTime.Now;
        }

        private Boolean FResult;
        private GLBatchTDS FMainDS;
        private Int32 FLedgerNumber;
        //private Int32 FBatchNumber;
        //private Int32 FJournalNumber;
        private ARecurringBatchRow batchRow;
        private ARecurringJournalRow journalRow;

        private const Decimal DEFAULT_CURRENCY_EXCHANGE = 1.0m;

        /// Batch number for the recurring batch to be submitted
        public ARecurringBatchRow BatchRow
        {
            set
            {
                batchRow = value;
                //FBatchNumber = batchRow.BatchNumber;

                txtExchangeRateToBase.BackColor = Color.Empty;

                lblExchangeRateToBase.Hide();
                txtExchangeRateToBase.Hide();
                lblCurrencyCodeFrom.Hide();
                txtCurrencyCodeFrom.Hide();
                lblCurrencyCodeTo.Hide();
                txtCurrencyCodeTo.Hide();
                btnGetSetExchangeRate.Hide();

                this.Text = String.Format(Catalog.GetString("Submit recurring Batch {0}"), batchRow.BatchNumber);
            }
        }

        /// Journal number for the recurring batch to be submitted
        public ARecurringJournalRow JournalRow
        {
            set
            {
                journalRow = value;

                lblExchangeRateToBase.Show();
                txtExchangeRateToBase.Show();
                lblCurrencyCodeFrom.Show();
                txtCurrencyCodeFrom.Show();
                lblCurrencyCodeTo.Show();
                txtCurrencyCodeTo.Show();
                btnGetSetExchangeRate.Show();

                this.Text = String.Format(Catalog.GetString(
                        "Submit recurring Batch {0} - Journal {1}"), journalRow.BatchNumber, journalRow.JournalNumber);
                lblExchangeRateToBase.Text = String.Format(Catalog.GetString("Exchange Rate for Journal {0}:"), journalRow.JournalNumber);

//                FBatchNumber = journalRow.BatchNumber;
//                FJournalNumber = journalRow.JournalNumber;
                txtExchangeRateToBase.Text = TExchangeRateCache.GetDailyExchangeRate(
                    FMainDS.ALedger[0].BaseCurrency,
                    journalRow.TransactionCurrency,
                    DateTime.Now).ToString();
                txtCurrencyCodeFrom.Text = journalRow.TransactionCurrency;

                if (journalRow.TransactionCurrency == FMainDS.ALedger[0].BaseCurrency)
                {
                    txtExchangeRateToBase.Enabled = false;
                    txtExchangeRateToBase.BackColor = Color.LightPink;
                }
                else
                {
                    txtExchangeRateToBase.Enabled = true;
                    txtExchangeRateToBase.BackColor = Color.Empty;
                }
            }
        }

        DateTime FStartDateCurrentPeriod;
        DateTime FEndDateLastForwardingPeriod;
        /// dataset for the whole screen
        public GLBatchTDS MainDS
        {
            set
            {
                FMainDS = value;

                FLedgerNumber = FMainDS.ALedger[0].LedgerNumber;
                txtCurrencyCodeTo.Text = FMainDS.ALedger[0].BaseCurrency;

                DateTime DefaultDate;
                TLedgerSelection.GetCurrentPostingRangeDates(FLedgerNumber,
                    out FStartDateCurrentPeriod,
                    out FEndDateLastForwardingPeriod,
                    out DefaultDate);
                lblValidDateRange.Text = String.Format(Catalog.GetString("Valid between {0} and {1}"),
                    FStartDateCurrentPeriod.ToShortDateString(), FEndDateLastForwardingPeriod.ToShortDateString());
            }
        }


        /// <summary>
        /// set the field for "Date effective" to be read only
        /// </summary>
        /// <param name="AReadOnly"></param>
        public void SetDateEffectiveReadOnly(Boolean AReadOnly = true)
        {
            dtpEffectiveDate.ReadOnly = AReadOnly;
        }

        /// <summary>
        /// return dialog result after dialog has been closed
        /// </summary>
        /// <param name="AEffectiveDate"></param>
        /// <param name="AExchangeRateToBase"></param>
        public Boolean GetResult(out DateTime AEffectiveDate, out decimal AExchangeRateToBase)
        {
            AEffectiveDate = dtpEffectiveDate.Date.Value;
            Decimal.TryParse(txtExchangeRateToBase.Text, out AExchangeRateToBase);

            return FResult;
        }

        /// <summary>
        /// return dialog result after dialog has been closed
        /// </summary>
        /// <param name="AEffectiveDate"></param>
        public Boolean GetResult(out DateTime AEffectiveDate)
        {
            AEffectiveDate = dtpEffectiveDate.Date.Value;

            return FResult;
        }

        /// <summary>
        /// this submits the given Batch number
        /// Each line starts with a type specifier, B for batch, J for journal, T for transaction
        /// </summary>
        private void SubmitBatch(object sender, EventArgs e)
        {
            decimal exchRateToBase = 0;

            if (!(Decimal.TryParse(txtExchangeRateToBase.Text, out exchRateToBase) && (exchRateToBase > 0)))
            {
                MessageBox.Show(Catalog.GetString("The exchange rate must be a number greater than 0."));
                txtExchangeRateToBase.Focus();
                txtExchangeRateToBase.SelectAll();
                return;
            }

            //check the gift batch date
            if (dtpEffectiveDate.Date < FStartDateCurrentPeriod)
            {
                MessageBox.Show(Catalog.GetString("Your date was before the allowed posting period start date: " +
                        FStartDateCurrentPeriod.ToShortDateString()));
                dtpEffectiveDate.Focus();
                dtpEffectiveDate.SelectAll();
                return;
            }

            if (dtpEffectiveDate.Date > FEndDateLastForwardingPeriod)
            {
                MessageBox.Show(Catalog.GetString("Your date was later than the allowed posting period end date: " +
                        FEndDateLastForwardingPeriod.ToShortDateString()));
                dtpEffectiveDate.Focus();
                dtpEffectiveDate.SelectAll();
                return;
            }

            FResult = true;
            Close();
        }

        private void BtnCloseClick(object sender, EventArgs e)
        {
            FResult = false;
            Close();
        }

        private void BtnHelpClick(object sender, EventArgs e)
        {
            // TODO
        }

        private void CheckBatchEffectiveDate(object sender, EventArgs e)
        {
            DateTime dateValue;
            string aDate = dtpEffectiveDate.Text;

            if (DateTime.TryParse(aDate, out dateValue))
            {
                txtExchangeRateToBase.Text = TExchangeRateCache.GetDailyExchangeRate(
                    txtCurrencyCodeTo.Text,
                    txtCurrencyCodeFrom.Text,
                    dateValue).ToString();
            }
            else
            {
                MessageBox.Show(Catalog.GetString("Invalid date entered!"));
                dtpEffectiveDate.Focus();
                dtpEffectiveDate.SelectAll();
            }
        }

        private void SetExchangeRateValue(Object sender, EventArgs e)
        {
            TFrmSetupDailyExchangeRate setupDailyExchangeRate =
                new TFrmSetupDailyExchangeRate(FPetraUtilsObject.GetForm());

            decimal selectedExchangeRate;
            DateTime selectedEffectiveDate;
            int selectedEffectiveTime;

            if (setupDailyExchangeRate.ShowDialog(
                    FLedgerNumber,
                    dtpEffectiveDate.Date.Value,
                    txtCurrencyCodeFrom.Text,
                    DEFAULT_CURRENCY_EXCHANGE,
                    out selectedExchangeRate,
                    out selectedEffectiveDate,
                    out selectedEffectiveTime) == DialogResult.Cancel)
            {
                return;
            }

//            if (FPreviouslySelectedDetailRow.ExchangeRateToBase != setupDailyExchangeRate.CurrencyExchangeRate)
//            {
//                //Enforce save needed condition
//                FPetraUtilsObject.SetChangedFlag();
//            }

            txtExchangeRateToBase.Text = selectedExchangeRate.ToString();
        }
    }
}