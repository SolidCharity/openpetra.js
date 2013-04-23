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
using Ict.Petra.Client.MFinance.Logic;
using Ict.Petra.Client.MReporting.Logic;
using Ict.Petra.Shared.MReporting;

namespace Ict.Petra.Client.MReporting.Gui.MFinance
{
    public partial class TFrmIncomeExpenseStmt
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

                uco_CostCentreSettings.InitialiseCostCentreList(FLedgerNumber);
                uco_GeneralSettings.InitialiseLedger(FLedgerNumber);

                FPetraUtilsObject.LoadDefaultSettings();
                uco_GeneralSettings.DisableToPeriod();
            }
        }

        private void ReadControlsManual(TRptCalculator ACalc, TReportActionEnum AReportAction)
        {
            ACalc.AddParameter("param_ledger_number_i", FLedgerNumber);
        }

        private void SetControlsManual(TParameterList AParameters)
        {
        }
    }
}