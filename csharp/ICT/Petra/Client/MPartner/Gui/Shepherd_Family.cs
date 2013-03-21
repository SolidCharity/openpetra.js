//
// DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
//
// @Authors:
//       ChristianK
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
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;

using Ict.Common;
using Ict.Petra.Client.CommonForms;
using Ict.Petra.Client.CommonForms.Logic;
using Ict.Petra.Client.MPartner.Logic;

namespace Ict.Petra.Client.MPartner.Gui
{
    /// <summary>
    /// Shepherd for a Partner of Class FAMILY.
    /// </summary>
    public partial class TShepherdFamilyForm : TPetraShepherdConcreteForm
    {
        #region Fields

        /// <summary>Instance of this Shepherd's Logic.</summary>
        private TShepherdFamilyFormLogic FSpecificLogic;

        private bool FSkipLedgerSelectionPage = false;

        #endregion

        #region Properties

        /// <summary>
        /// TODO Comment
        /// </summary>
        public bool SkipLedgerSelectionPage
        {
            get
            {
                return FSkipLedgerSelectionPage;
            }

            set
            {
                FSkipLedgerSelectionPage = value;
            }
        }

        #endregion

        #region Constructor

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="AParentForm">Parent Form.</param>
        public TShepherdFamilyForm(Form AParentForm) : base(AParentForm)
        {
            TLogging.Log("Entering TShepherdFamilyForm Constructor...");

            YamlFile = Path.GetDirectoryName(TAppSettingsManager.GetValue("UINavigation.File")) +
                       Path.DirectorySeparatorChar + "Shepherd_Family_Definition.yaml";

            Logic = new TShepherdFamilyFormLogic(YamlFile, this);
            FSpecificLogic = (TShepherdFamilyFormLogic)Logic;

            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();
            #region CATALOGI18N

            // this code has been inserted by GenerateI18N, all changes in this region will be overwritten by GenerateI18N
            this.Text = Catalog.GetString("TShepherdFamilyForm");
            #endregion

            TLogging.Log("TShepherdFamilyForm Constructor ran.");
        }

        #endregion

        #region Protected Methods

        /// <summary>
        /// Load Event for the TShepherdFamilyForm.
        /// </summary>
        /// <param name="ASender">Sending Control (supplied by WinForms).</param>
        /// <param name="AEventArgs">Event Arguments (supplied by WinForms).</param>
        protected override void Form_Load(object ASender, EventArgs AEventArgs)
        {
            TLogging.Log("Entering TShepherdFamilyForm Form_Load...");

            base.Form_Load(ASender, AEventArgs);

            if (FSkipLedgerSelectionPage)
            {
                FSpecificLogic.SkipFirstShepherdPage();
            }

            TLogging.Log("TShepherdFamilyForm Form_Load ran.");

            try
            {
                TLogging.Log("The Family Form Printed an a valid ID: " + Logic.CurrentPage.ID);
            }
            catch (Exception)
            {
                TLogging.Log("EXCEPTION CAUGHT: testStatusMessage threw Null Exception.");
            }
        }

        #endregion
    }
}