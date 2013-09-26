//
// DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
//
// @Authors:
//       berndr
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
using System.Windows.Forms;
using GNU.Gettext;
using Ict.Common;
using Ict.Common.Verification;
using Ict.Petra.Client.MReporting.Gui.MFinance;
using Ict.Petra.Client.MFinance.Logic;
using Ict.Petra.Client.MReporting.Logic;
using Ict.Petra.Client.App.Core.RemoteObjects;
using Ict.Petra.Shared.MReporting;

namespace Ict.Petra.Client.MReporting.Gui.MFinDev
{
    public partial class TFrmTopDonorReport
    {
        private Int32 FLedgerNumber;

        /// <summary>
        /// the report should be run for this ledger
        /// </summary>
        public Int32 LedgerNumber
        {
            set
            {
                FLedgerNumber = value;

                lblLedger.Text = Catalog.GetString("Ledger: ") + FLedgerNumber.ToString();
            }
        }

        private void DonorTypeChanged(object Sender, EventArgs e)
        {
            if ((rbtTopDonor.Checked)
                || (rbtBottomDonor.Checked))
            {
                txtToPercentage.NumberValueInt = 0;
                txtToPercentage.Enabled = false;
            }
            else if (rbtMiddleDonor.Checked)
            {
                txtToPercentage.Enabled = true;
            }
        }

        private void ReadControlsVerify(TRptCalculator ACalc, TReportActionEnum AReportAction)
        {
            if (!dtpEndDate.ValidDate(false)
                || !dtpStartDate.ValidDate(false))
            {
                TVerificationResult VerificationResult = new TVerificationResult(
                    Catalog.GetString("No valid date."),
                    Catalog.GetString("Please enter a valid date."),
                    TResultSeverity.Resv_Critical);
                FPetraUtilsObject.AddVerificationResult(VerificationResult);
            }
            else
            {
                if (dtpStartDate.Date > dtpEndDate.Date)
                {
                    TVerificationResult VerificationResult = new TVerificationResult(
                        Catalog.GetString("From date is later than to date."),
                        Catalog.GetString("Please change from date or to date."),
                        TResultSeverity.Resv_Critical);
                    FPetraUtilsObject.AddVerificationResult(VerificationResult);
                }
            }

            if (rbtMiddleDonor.Checked)
            {
                int From = Convert.ToInt32(txtPercentage.Text);
                int To = Convert.ToInt32(txtToPercentage.Text);

                if (To > From)
                {
                    // From must be biggern than to
                    String TmpString = txtPercentage.Text;
                    txtPercentage.Text = txtToPercentage.Text;
                    txtToPercentage.Text = TmpString;
                }
            }
        }

        private void ReadControlsManual(TRptCalculator ACalc, TReportActionEnum AReportAction)
        {
            ACalc.AddParameter("param_ledger_number_i", FLedgerNumber);

            ACalc.AddParameter("param_all_partners", rbtAllPartners.Checked);
            ACalc.AddParameter("param_extract", rbtExtract.Checked);

            if (rbtExtract.Checked)
            {
                ACalc.AddParameter("param_extract_name", txtExtract.Text);
            }

            ACalc.AddParameter("param_recipientkey", txtRecipient.Text);

            if ((txtMotivationDetail.Text.Length == 0)
                || (txtMotivationDetail.Text == "*"))
            {
                ACalc.AddParameter("param_motivation_detail", "%");
            }
            else
            {
                ACalc.AddParameter("param_motivation_detail", txtMotivationDetail.Text.Replace('*', '%'));
            }

            if ((txtMotivationGroup.Text.Length == 0)
                || (txtMotivationGroup.Text == "*"))
            {
                ACalc.AddParameter("param_motivation_group", "%");
            }
            else
            {
                ACalc.AddParameter("param_motivation_group", txtMotivationGroup.Text.Replace('*', '%'));
            }

            if (rbtBottomDonor.Checked)
            {
                int Percent = Convert.ToInt32(txtPercentage.Text);

                ACalc.AddParameter("param_percentage", 100);
                ACalc.AddParameter("param_to_percentage", 100 - Percent);
                ACalc.AddParameter("param_donor_type", "bottom");
            }
            else if (rbtMiddleDonor.Checked)
            {
                ACalc.AddParameter("param_donor_type", "middle");
            }
            else if (rbtTopDonor.Checked)
            {
                ACalc.AddParameter("param_donor_type", "top");
            }

            // Use these 7 predefined columns in the report
            ACalc.AddParameter("param_calculation", "Donor Key", 0);
            ACalc.AddParameter("ColumnWidth", 2.2, 0);

            ACalc.AddParameter("param_calculation", "Partner Class", 1);
            ACalc.AddParameter("ColumnWidth", 2.0, 1);

            ACalc.AddParameter("param_calculation", "Donor Name", 2);
            ACalc.AddParameter("ColumnWidth", 4.5, 2);

            ACalc.AddParameter("param_calculation", "Total Gifts", 3);
            ACalc.AddParameter("ColumnWidth", 2.0, 3);

            ACalc.AddParameter("param_calculation", "%of Total", 4);
            ACalc.AddParameter("ColumnWidth", 1.5, 4);

            ACalc.AddParameter("param_calculation", "Cumulative %", 5);
            ACalc.AddParameter("ColumnWidth", 1.8, 5);

            ACalc.AddParameter("param_calculation", "Address", 6);
            ACalc.AddParameter("ColumnWidth", 12.0, 6);

            ACalc.AddParameter("MaxDisplayColumns", 7);
        }

        private void SetControlsManual(TParameterList AParameters)
        {
            rbtExtract.Checked = AParameters.Get("param_extract").ToBool();
            rbtAllPartners.Checked = AParameters.Get("param_all_partners").ToBool();
            txtExtract.Text = AParameters.Get("param_extract_name").ToString();
            txtRecipient.Text = AParameters.Get("param_recipientkey").ToString();

            txtMotivationGroup.Text = AParameters.Get("param_motivation_group").ToString().Replace('%', '*');
            txtMotivationDetail.Text = AParameters.Get("param_motivation_detail").ToString().Replace('%', '*');

            rbtTopDonor.Checked = (AParameters.Get("param_donor_type").ToString() == "top");
            rbtMiddleDonor.Checked = (AParameters.Get("param_donor_type").ToString() == "middle");
            rbtBottomDonor.Checked = (AParameters.Get("param_donor_type").ToString() == "bottom");

            if (rbtBottomDonor.Checked)
            {
                txtPercentage.NumberValueInt = 100 - AParameters.Get("param_to_percentage").ToInt();
                txtToPercentage.NumberValueInt = 0;
            }
        }
    }
}