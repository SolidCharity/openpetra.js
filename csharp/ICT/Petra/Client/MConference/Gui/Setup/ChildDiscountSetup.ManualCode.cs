﻿//
// DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
//
// @Authors:
//       peters
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
using System.Data;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Xml;
using GNU.Gettext;
using Ict.Common.Verification;
using Ict.Common;
using Ict.Common.IO;
using Ict.Petra.Client.App.Core;
using Ict.Petra.Client.App.Core.RemoteObjects;
using Ict.Petra.Shared;
using Ict.Petra.Shared.MConference;
using Ict.Petra.Shared.MConference.Data;
using Ict.Petra.Shared.MConference.Validation;

namespace Ict.Petra.Client.MConference.Gui.Setup
{
    public partial class TFrmChildDiscountSetup
    {
        /// PartnerKey for selected conference to be set from outside
        public static Int64 FPartnerKey {
            private get; set;
        }

        private void InitializeManualCode()
        {
            string ConferenceName;
            TPartnerClass PartnerClass;

            // display the conference name in the title bar and in a text box at the top of the screen
            TRemote.MPartner.Partner.ServerLookups.WebConnectors.GetPartnerShortName(FPartnerKey, out ConferenceName, out PartnerClass);
            this.Text = this.Text + " [" + ConferenceName + "]";
            txtConferenceName.Text = ConferenceName;
        }

        private void NewRowManual(ref PcDiscountRow ARow)
        {
            string DiscountCriteriaCode = "CHILD";
            string CostType = "CONFERENCE";
            string Validity = "ALWAYS";
            int NewAge = 0;  // starts at 0 years
            int i = 0;

            // if PK already exists, find the next available
            while (FMainDS.PcDiscount.Rows.Find(new object[] { FPartnerKey, DiscountCriteriaCode, CostType, Validity, NewAge + i }) != null)
            {
                if (CostType == "CONFERENCE")
                {
                    CostType = "ACCOMMODATION";
                }
                else
                {
                    CostType = "CONFERENCE";
                    i++;
                }
            }

            NewAge += i;

            // set default values for new row
            ARow.DiscountCriteriaCode = DiscountCriteriaCode;
            ARow.CostTypeCode = CostType;
            ARow.Validity = Validity;
            ARow.UpToAge = NewAge;
            ARow.Percentage = true;
            ARow.Discount = 0;

            // set the conference key
            ARow.ConferenceKey = FPartnerKey;
        }

        private void NewRecord(Object sender, EventArgs e)
        {
            CreateNewPcDiscount();
        }

        private void ShowDetailsManual(PcDiscountRow ARow)
        {
            // converts the numeric value in table to int
            if (ARow != null)
            {
                if (ARow.IsDiscountNull())
                {
                    txtDetailDiscount.NumberValueInt = null;
                }
                else
                {
                    txtDetailDiscount.NumberValueInt = Convert.ToInt32(ARow.Discount);
                }

                EnableOrDisableCmb(ARow);
            }
        }

        private void UpdateCostTypeCode(object sender, EventArgs e)
        {
            PcDiscountRow ARow = GetSelectedDetailRow();

            // if txtDetailUpToDate has just been changed for a row, select available Cost Type Code (if any)
            // and enable or diable cmb as appropriate
            if (sender.Equals(txtDetailUpToAge) && (txtDetailUpToAge.NumberValueInt != ARow.UpToAge))
            {
                if (FMainDS.PcDiscount.Rows.Find(new object[] { FPartnerKey, ARow.DiscountCriteriaCode, "ACCOMMODATION", ARow.Validity,
                                                                txtDetailUpToAge.NumberValueInt })
                    != null)
                {
                    cmbDetailCostTypeCode.SelectedItem = "CONFERENCE";
                    cmbDetailCostTypeCode.Enabled = false;
                }
                else if (FMainDS.PcDiscount.Rows.Find(new object[] { FPartnerKey, ARow.DiscountCriteriaCode, "CONFERENCE", ARow.Validity,
                                                                     txtDetailUpToAge.NumberValueInt })
                         != null)
                {
                    cmbDetailCostTypeCode.SelectedItem = "ACCOMMODATION";
                    cmbDetailCostTypeCode.Enabled = false;
                }
                else
                {
                    cmbDetailCostTypeCode.Enabled = true;
                }
            }
        }

        // enables or disables the combo box depending on the availability of the two Cost Type Codes for selected UpToAge
        private void EnableOrDisableCmb(PcDiscountRow ARow)
        {
            if (ARow.CostTypeCode == "CONFERENCE")
            {
                if (FMainDS.PcDiscount.Rows.Find(new object[] { FPartnerKey, ARow.DiscountCriteriaCode, "ACCOMMODATION", ARow.Validity,
                                                                ARow.UpToAge }) != null)
                {
                    cmbDetailCostTypeCode.Enabled = false;
                }
                else
                {
                    cmbDetailCostTypeCode.Enabled = true;
                }
            }
            else if (ARow.CostTypeCode == "ACCOMMODATION")
            {
                if (FMainDS.PcDiscount.Rows.Find(new object[] { FPartnerKey, ARow.DiscountCriteriaCode, "CONFERENCE", ARow.Validity,
                                                                ARow.UpToAge }) != null)
                {
                    cmbDetailCostTypeCode.Enabled = false;
                }
                else
                {
                    cmbDetailCostTypeCode.Enabled = true;
                }
            }
        }

        private void ValidateDataDetailsManual(PcDiscountRow ARow)
        {
            if (txtDetailDiscount.Text == "")
            {
                ARow.Discount = 0;
            }

            // check that default data exists in database
            TVerificationResultCollection VerificationResultCollection = FPetraUtilsObject.VerificationResultCollection;

            TValidationControlsData ValidationControlsData;
            TScreenVerificationResult VerificationResult = null;
            DataColumn ValidationColumn;

            if (ARow.RowState != DataRowState.Deleted)
            {
                if (!TRemote.MConference.Conference.WebConnectors.CheckDiscountCriteriaCodeExists(new string[] { ARow.DiscountCriteriaCode }))
                {
                    ValidationColumn = ARow.Table.Columns[PcDiscountTable.ColumnDiscountCriteriaCodeId];

                    // displays a warning message
                    VerificationResult = new TScreenVerificationResult(new TVerificationResult(this, ErrorCodes.GetErrorInfo(
                                PetraErrorCodes.ERR_DISCOUNT_CRITERIA_CODE_DOES_NOT_EXIST)),
                        ValidationColumn, ValidationControlsData.ValidationControl);

                    // Handle addition to/removal from TVerificationResultCollection
                    VerificationResultCollection.Auto_Add_Or_AddOrRemove(this, VerificationResult, ValidationColumn);
                }

                if (!TRemote.MConference.Conference.WebConnectors.CheckCostTypeExists(ARow.CostTypeCode))
                {
                    ValidationColumn = ARow.Table.Columns[PcDiscountTable.ColumnCostTypeCodeId];

                    // displays a warning message
                    VerificationResult = new TScreenVerificationResult(new TVerificationResult(this, ErrorCodes.GetErrorInfo(
                                PetraErrorCodes.ERR_COST_TYPE_CODE_DOES_NOT_EXIST)),
                        ValidationColumn, ValidationControlsData.ValidationControl);

                    // Handle addition to/removal from TVerificationResultCollection
                    VerificationResultCollection.Auto_Add_Or_AddOrRemove(this, VerificationResult, ValidationColumn);
                }
            }

            EnableOrDisableCmb(ARow);
        }
    }
}