//
// DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
//
// @Authors:
//       christiank, timop
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
using SourceGrid;
using GNU.Gettext;
using Ict.Common;
using Ict.Common.Controls;
using Ict.Petra.Shared;
using Ict.Petra.Shared.Interfaces.MPartner;
using Ict.Petra.Client.App.Core;
using Ict.Petra.Client.MPartner;
using Ict.Petra.Shared.MPartner;
using Ict.Petra.Client.CommonForms;
using Ict.Petra.Client.CommonControls;
using Ict.Petra.Shared.MPartner.Partner.Data;
using Ict.Petra.Client.App.Gui;
using Ict.Petra.Client.MCommon;
using Ict.Petra.Client.MFinance.Gui.Gift;

namespace Ict.Petra.Client.MPartner.Gui
{
    /// <summary>
    /// Description of TUC_PartnerFind_ByPartnerDetails.
    /// </summary>
    public partial class TUC_PartnerFind_ByPartnerDetails : UserControl
    {
        /// <summary>DataTable containing the search criteria</summary>
        private DataTable FCriteriaData;

        /// <summary>DataTable that holds all Pages of data (also empty ones that are not retrieved yet!)</summary>
        private DataTable FPagedDataTable;

        /// <summary>The number of the current Row in the Find Results Grid</summary>
        /// <remarks>Somehow, the Grid's own method of determining the current Position
        /// (Grid.Selection.ActivePosition) has sometimes problems, that's why we need
        /// to keep track of things manually  :-(</remarks>
        private int FCurrentGridRow = -1;

        /// <summary>Last PartnerKey for which the Partner Info Panel was opened.</summary>
        private Int64 FLastPartnerKeyInfoPanelOpened = -1;

        /// <summary>Last LocationKey for which the Partner Info Panel was opened.</summary>
        private TLocationPK FLastLocationPKInfoPanelOpened = new TLocationPK();

        /// <summary>Tells the screen whether it should still wait for the Server's result</summary>
        private Boolean FKeepUpSearchFinishedCheck;

        /// <summary>Tells whether the last search operation was done with 'Detailed Results' enabled</summary>
        private Boolean FLastSearchWasDetailedSearch;

        /// <summary>Tells whether any change occured in the Search Criteria since the last search operation</summary>
        private Boolean FCriteriaContentChanged;

        // <summary>If the Form should set the Focus to the LocationKey field, set the LocationKey to this value</summary>
// TODO        private Int32 FPassedLocationKey;
        /// <summary>Object that holds the logic for this screen</summary>
        private TPartnerFindScreenLogic FLogic;

        /// <summary>The Proxy System.Object of the Serverside UIConnector</summary>
        private IPartnerUIConnectorsPartnerFind FPartnerFindObject;

//TODO        private Int32 FSplitterDistForm;
//TODO        private Int32 FSplitterDistFindByDetails;
        private bool FPartnerInfoPaneOpen = false;
        private bool FPartnerTasksPaneOpen = false;
        private TUC_PartnerInfo FPartnerInfoUC;

        private Boolean FRunningInsideModalForm;

        /// <summary>
        /// event for when the search result changes, and more or less partners match the search criteria
        /// </summary>
        public delegate void TPartnerAvailableChangeEventHandler(TPartnerAvailableEventArgs e);
        /// <summary>
        /// event for when the search result changes, and more or less partners match the search criteria
        /// </summary>
        public event TPartnerAvailableChangeEventHandler PartnerAvailable;

        /// <summary>
        /// event that is triggered when the search starts and when it finishes
        /// </summary>
        public delegate void TSearchOperationStateChangeEventHandler(TSearchOperationStateChangeEventArgs e);
        /// <summary>
        /// event that is triggered when the search starts and when it finishes
        /// </summary>
        public event TSearchOperationStateChangeEventHandler SearchOperationStateChange;

        /// <summary>todoComment</summary>
        public event System.EventHandler PartnerInfoPaneCollapsed;

        /// <summary>todoComment</summary>
        public event System.EventHandler PartnerInfoPaneExpanded;

        /// <summary>todoComment</summary>
        public event System.EventHandler EnableAcceptButton;

        /// <summary>todoComment</summary>
        public event System.EventHandler DisableAcceptButton;

        private void OnPartnerInfoPaneCollapsed()
        {
            if (PartnerInfoPaneCollapsed != null)
            {
                PartnerInfoPaneCollapsed(this, new EventArgs());
            }
        }

        private void OnPartnerInfoPaneExpanded()
        {
            if (PartnerInfoPaneExpanded != null)
            {
                PartnerInfoPaneExpanded(this, new EventArgs());
            }
        }

        private void OnPartnerAvailable(bool AAvailable)
        {
            if (PartnerAvailable != null)
            {
                PartnerAvailable(new TPartnerAvailableEventArgs(AAvailable));
            }
        }

        private void OnSearchOperationStateChange(bool ASearchOperationIsRunning)
        {
            if (SearchOperationStateChange != null)
            {
                SearchOperationStateChange(new TSearchOperationStateChangeEventArgs(ASearchOperationIsRunning));
            }
        }

        private void OnEnableAcceptButton()
        {
            if (EnableAcceptButton != null)
            {
                EnableAcceptButton(this, new EventArgs());
            }
        }

        private void OnDisableAcceptButton()
        {
            if (DisableAcceptButton != null)
            {
                DisableAcceptButton(this, new EventArgs());
            }
        }

        /// <summary>
        /// read the partner key of the currently selected partner
        /// </summary>
        public Int64 PartnerKey
        {
            get
            {
                return FLogic.PartnerKey;
            }
        }

        /// <summary>
        /// is the partner info pane opened?
        /// </summary>
        public bool PartnerInfoPaneOpen
        {
            get
            {
                return FPartnerInfoPaneOpen;
            }
        }

        /// <summary>
        /// access to the currently selected partners
        /// </summary>
        public DataRowView[] SelectedDataRowsAsDataRowView
        {
            get
            {
                return grdResult.SelectedDataRowsAsDataRowView;
            }
        }

        /// <summary>
        /// Whether this UserControl is running inside a Modal Form, or not.
        /// </summary>
        public bool RunnningInsideModalForm
        {
            get
            {
                return FRunningInsideModalForm;
            }

            set
            {
                FRunningInsideModalForm = value;
            }
        }

        /// <summary>
        /// constructor
        /// </summary>
        public TUC_PartnerFind_ByPartnerDetails()
        {
            //
            // The InitializeComponent() call is required for Windows Forms designer support.
            //
            InitializeComponent();
            #region CATALOGI18N

            // this code has been inserted by GenerateI18N, all changes in this region will be overwritten by GenerateI18N
            this.btnSearch.Text = Catalog.GetString(" &Search");
            this.chkDetailedResults.Text = Catalog.GetString("Detailed Results");
            this.btnClearCriteria.Text = Catalog.GetString("Clea&r");
            this.grpCriteria.Text = Catalog.GetString("Find Criteria");
            this.btnCustomCriteriaDemo.Text = Catalog.GetString("Custom Criteria Demo");
            this.ucoPartnerInfo.Text = Catalog.GetString("Partner Info");
            this.grpResult.Text = Catalog.GetString("Fin&d Result");
            this.lblSearchInfo.Text = Catalog.GetString("Searching...");
            #endregion

            // on Mono: we need to change the AutoSize so that the results will be displayed
            if (Ict.Common.Utilities.DetermineExecutingCLR() == TExecutingCLREnum.eclrMono)
            {
                this.pnlPartnerInfoContainer.AutoSize = false;
            }

            // Define the screen's Logic
            FLogic = new TPartnerFindScreenLogic();
            FLogic.ParentForm = this;
            FLogic.DataGrid = grdResult;

            lblSearchInfo.Text = "";
            grdResult.SendToBack();
        }

        private TFrmPetraUtils FPetraUtilsObject;

        /// Doesn't do anything, but needs to be present as the Template requires this Method to be present...
        public void GetDataFromControls()
        {
            // Doesn't do anything, but needs to be present as the Template requires this Method to be present...
        }

        /// <summary>
        /// // Doesn't do anything, but needs to be present as the Template requires this Method to be present...
        /// </summary>
        /// <param name="ARecordChangeVerification"></param>
        /// <param name="AProcessAnyDataValidationErrors"></param>
        /// <returns></returns>
        public bool ValidateAllData(bool ARecordChangeVerification, bool AProcessAnyDataValidationErrors)
        {
            // Doesn't do anything, but needs to be present as the Template requires this Method to be present...

            return true;
        }

        /// <summary>
        /// this provides general functionality for edit screens
        /// </summary>
        public TFrmPetraUtils PetraUtilsObject
        {
            get
            {
                return FPetraUtilsObject;
            }
            set
            {
                FPetraUtilsObject = value;

// todo: no resourcestrings
                FPetraUtilsObject.SetStatusBarText(btnSearch, MPartnerResourcestrings.StrSearchButtonHelpText);
                FPetraUtilsObject.SetStatusBarText(btnClearCriteria, MPartnerResourcestrings.StrClearCriteriaButtonHelpText);
                FPetraUtilsObject.SetStatusBarText(grdResult,
                    MPartnerResourcestrings.StrResultGridHelpText + MPartnerResourcestrings.StrPartnerFindSearchTargetText);
                FPetraUtilsObject.SetStatusBarText(chkDetailedResults, MPartnerResourcestrings.StrDetailedResultsHelpText);
            }
        }

        /// <summary>
        /// needed for generated code
        /// </summary>
        public void InitUserControl()
        {
            ucoPartnerInfo.HostedControlKind = THostedControlKind.hckUserControl;
            ucoPartnerInfo.UserControlClass = "TUC_PartnerInfo";          // TUC_PartnerInfo
            ucoPartnerInfo.UserControlNamespace = "Ict.Petra.Client.MPartner.Gui";
        }

        /// <summary>
        /// close or open the partner info pane depending on user defaults
        /// </summary>
        public void SetupPartnerInfoPane()
        {
            // Set up Partner Info pane
            FPartnerInfoPaneOpen = TUserDefaults.GetBooleanDefault(
                TUserDefaults.PARTNER_FIND_PARTNERDETAILS_OPEN, false);

            if (!FPartnerInfoPaneOpen)
            {
                ClosePartnerInfoPane();
            }
            else
            {
                OpenPartnerInfoPane();
            }
        }

        private void GrdResult_MouseDown(System.Object sender, System.Windows.Forms.MouseEventArgs e)
        {
            int PointY = -1;

            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                PointY = new Point(e.X, e.Y).Y;

                //                TLogging.Log("GrdResult_MouseDown: grdResult.Rows.RowAtPoint: " + grdResult.Rows.RowAtPoint(PointY).Value.ToString());

                grdResult.Selection.ResetSelection(false);
                grdResult.Selection.SelectRow(grdResult.Rows.RowAtPoint(PointY).Value, true);

                DataGrid_FocusRowEntered(this,
                    new RowEventArgs(grdResult.Rows.RowAtPoint(PointY).Value));

                // show the context menu:
                // TODO mnuPartnerFindContext.Show(grdResult, new Point(e.X, e.Y));
            }
        }

        private void ChkDetailedResults_CheckedChanged(System.Object sender, System.EventArgs e)
        {
            ArrayList FieldList;

            if (FPagedDataTable != null)
            {
                // prepare a list of columns
                FieldList = new ArrayList();

                foreach (String eachField in ucoPartnerFindCriteria.CriteriaFieldsLeft)
                {
                    FieldList.Add(eachField);
                }

                foreach (String eachField in ucoPartnerFindCriteria.CriteriaFieldsRight)
                {
                    FieldList.Add(eachField);
                }

                if (!FCriteriaContentChanged)
                {
                    if (chkDetailedResults.Checked == false)
                    {
                        // Reduce the columns that are shown in the Grid
                        FLogic.CreateColumns(FPagedDataTable, false, FCriteriaData.Rows[0]["PartnerStatus"].ToString() != "ACTIVE", FieldList);
                        SetupDataGridVisualAppearance();
                        grdResult.Selection.SelectRow(FCurrentGridRow, true);
                    }
                    else
                    {
                        if (FLastSearchWasDetailedSearch)
                        {
                            // Show all columns in the Grid
                            FLogic.CreateColumns(FPagedDataTable, true, FCriteriaData.Rows[0]["PartnerStatus"].ToString() != "ACTIVE", FieldList);
                            SetupDataGridVisualAppearance();
                            grdResult.Selection.SelectRow(FCurrentGridRow, true);
                        }
                        else
                        {
                            BtnSearch_Click(this, null);
                        }
                    }
                }
                else
                {
                    BtnSearch_Click(this, null);
                }
            }
        }

        private void GrdResult_DoubleClickCell(System.Object Sender, SourceGrid.CellContextEventArgs e)
        {
            if (FRunningInsideModalForm == true)
            {
                BtnAccept_Click(this, null);
            }
            else
            {
                OpenPartnerEditScreen(TPartnerEditTabPageEnum.petpAddresses);
            }
        }

        private void GrdResult_EnterKeyPressed(System.Object Sender, SourceGrid.RowEventArgs e)
        {
            if (FRunningInsideModalForm == true)
            {
                BtnAccept_Click(this, null);
            }
            else
            {
                OpenPartnerEditScreen(TPartnerEditTabPageEnum.petpAddresses);
            }
        }

        private void BtnAccept_Click(System.Object sender, System.EventArgs e)
        {
            // TODO: call the form BtnAccept_Click, by using an interface for the partnerfind class???
            ParentForm.DialogResult = System.Windows.Forms.DialogResult.OK;
            ParentForm.Close();
        }

        private void GrdResult_SpaceKeyPressed(System.Object Sender, SourceGrid.RowEventArgs e)
        {
            TogglePartnerInfoPane();
        }

        private void GrdResult_KeyPressed(System.Object Sender, KeyEventArgs e)
        {
            //            MessageBox.Show(e.KeyCode.ToString());
            if (e.KeyCode == Keys.Apps)
            {
                //                grdResult.Focus();
                //                Position ActivePos = grdResult.Selection.ActivePosition;

                //                MessageBox.Show("ActivePos.Row: " + ActivePos.Row.ToString());

                /*
                 * Show the context menu:
                 *   X coordinate: 2/3 of the Width of the Grid
                 *   Y coordinate: Top of the current Row + 3/4 of the Height of the Row
                 */

                // TODO context menu
#if TODO
                mnuPartnerFindContext.Show(grdResult,
                    new Point((grdResult.Width - (grdResult.Width / 3)),
                        (grdResult.Rows.GetTop(FCurrentGridRow) +
                         (grdResult.Rows.GetHeight(FCurrentGridRow) / 4) * 3)));
#endif
            }
        }

        private void GrdResult_DataPageLoading(System.Object Sender, TDataPageLoadEventArgs e)
        {
            // MessageBox.Show('DataPageLoading:  Page: ' + e.DataPage.ToString);
            if (e.DataPage > 0)
            {
                this.Cursor = Cursors.WaitCursor;
                FPetraUtilsObject.WriteToStatusBar(MPartnerResourcestrings.StrTransferringDataForPageText + e.DataPage.ToString() + ')');
                FPetraUtilsObject.SetStatusBarText(grdResult, MPartnerResourcestrings.StrTransferringDataForPageText + e.DataPage.ToString() + ')');
            }
        }

        private void GrdResult_DataPageLoaded(System.Object Sender, TDataPageLoadEventArgs e)
        {
            //          MessageBox.Show("DataPageLoaded:  Page: " + e.DataPage.ToString());
            if (e.DataPage > 0)
            {
                this.Cursor = Cursors.Default;
                FPetraUtilsObject.WriteToStatusBar(
                    MPartnerResourcestrings.StrResultGridHelpText + MPartnerResourcestrings.StrPartnerFindSearchTargetText);
                FPetraUtilsObject.SetStatusBarText(grdResult,
                    MPartnerResourcestrings.StrResultGridHelpText + MPartnerResourcestrings.StrPartnerFindSearchTargetText);
            }
        }

        /// <summary>
        /// Event Handler for FocusRowEntered Event of the Grid
        /// </summary>
        /// <param name="ASender">The Grid</param>
        /// <param name="AEventArgs">RowEventArgs as specified by the Grid (use Row property to
        /// get the Grid Row for which this Event fires)
        /// </param>
        /// <returns>void</returns>
        private void DataGrid_FocusRowEntered(System.Object ASender, RowEventArgs AEventArgs)
        {
            FCurrentGridRow = AEventArgs.Row;

            FLogic.DetermineCurrentPartnerKey();

            //            MessageBox.Show("PartnerKey of newly selected Row: " + FLogic.PartnerKey.ToString());
            //            TLogging.Log("DataGrid_FocusRowEntered: PartnerKey of newly selected Row: " + FLogic.PartnerKey.ToString());

            // Update 'Partner Info' if this Panel is shown
            UpdatePartnerInfoPanel();
        }

        #region Menu/ToolBar command handling

        /// <summary>
        /// Menu/ToolBar command handling: call functions for each menu item/toolbar button
        /// </summary>
        /// <param name="ATopLevelMenuItem"></param>
        /// <param name="AToolStripItem"></param>
        /// <param name="ARunAsModalForm"></param>
        public void HandleMenuItemOrToolBarButton(ToolStripMenuItem ATopLevelMenuItem, ToolStripItem AToolStripItem, bool ARunAsModalForm)
        {
            if (ATopLevelMenuItem.Name == "mniFile")
            {
                HandleFileMenuItemOrToolBarButton(AToolStripItem, ARunAsModalForm);
            }
            else if (ATopLevelMenuItem.Name == "mniEdit")
            {
                HandleEditMenuItemOrToolBarButton(AToolStripItem);
            }
            else if (ATopLevelMenuItem.Name == "mniMaintain")
            {
                HandleMaintainMenuItemOrToolBarButton(AToolStripItem);
            }
            else if (ATopLevelMenuItem.Name == "mniMailing")
            {
                HandleMailingMenuItemOrToolBarButton(AToolStripItem);
            }
            else if (ATopLevelMenuItem.Name == "mniTools")
            {
                HandleToolsMenuItemOrToolBarButton(AToolStripItem);
            }
            else if (ATopLevelMenuItem.Name == "mniView")
            {
                HandleViewMenuItemOrToolBarButton(AToolStripItem);
            }
        }

        void HandleFileMenuItemOrToolBarButton(ToolStripItem AToolStripItem, bool ARunAsModalForm)
        {
            if ((AToolStripItem.Name == "mniFileNewPartner")
                || (AToolStripItem.Name == "tbbNewPartner"))
            {
                OpenNewPartnerEditScreen(ARunAsModalForm);
            }
            else if ((AToolStripItem.Name == "mniFileEditPartner")
                     || (AToolStripItem.Name == "tbbEditPartner"))
            {
                OpenPartnerEditScreen(TPartnerEditTabPageEnum.petpAddresses);
            }
            else if (AToolStripItem.Name == "mniFileDeletePartner")
            {
                TPartnerMain.DeletePartner(FLogic.PartnerKey);
            }
            else if ((AToolStripItem.Name == "mniFileWorkWithLastPartner")
                     || (AToolStripItem.Name == "mniFileWorkWithLastPartner"))
            {
                TPartnerMain.OpenLastUsedPartnerEditScreen(this.ParentForm);
            }
            else
            {
                throw new NotImplementedException();
            }

#if TODO
            String ClickedMenuItemText;

            ClickedMenuItemText = ((MenuItem)sender).Text;

            if (ClickedMenuItemText == mniFileSearch.Text)
            {
                BtnSearch_Click(this, new EventArgs());
            }
            else if (ClickedMenuItemText == mniFileMergePartners.Text)
            {
                TMenuFunctions.MergePartners();
            }
            else if (ClickedMenuItemText == mniFileDeletePartner.Text)
            {
                TMenuFunctions.DeletePartner();
            }
            else if (ClickedMenuItemText == mniFileCopyAddress.Text)
            {
                OpenCopyAddressToClipboardScreen();
            }
            else if (ClickedMenuItemText == mniFileCopyPartnerKey.Text)
            {
                TMenuFunctions.CopyPartnerKeyToClipboard();
            }
            else if (ClickedMenuItemText == mniFileSendEmail.Text)
            {
                TMenuFunctions.SendEmailToPartner();
            }
            else if (ClickedMenuItemText == mniFilePrintPartner.Text)
            {
                TMenuFunctions.PrintPartner();
            }
            else if (ClickedMenuItemText == mniFileExportPartner.Text)
            {
                TMenuFunctions.ExportPartner();
            }
            else if (ClickedMenuItemText == mniFileImportPartner.Text)
            {
                TMenuFunctions.ImportPartner();
            }
            else if (ClickedMenuItemText == mniFileClose.Text)
            {
                base.MniFile_Click(sender, e);
            }
            else if (ClickedMenuItemText == mniFileRecentPartners.Text)
            {
                SetupRecentlyUsedPartnersMenu();
            }
            else
            {
                NotifyFunctionNotYetImplemented(ClickedMenuItemText);
            }
#endif
        }

        void HandleEditMenuItemOrToolBarButton(ToolStripItem AToolStripItem)
        {
            if (AToolStripItem.Name == "mniEditSearch")
            {
                BtnSearch_Click(this, new EventArgs());
            }
            else if (AToolStripItem.Name == "mniMaintainDonorHistory")
            {
                Ict.Petra.Client.MFinance.Gui.Gift.TFrmDonorRecipientHistory.OpenWindowDonorRecipientHistory(AToolStripItem.Name,
                    PartnerKey,
                    FPetraUtilsObject.GetForm());
            }
            else if (AToolStripItem.Name == "mniMaintainRecipientHistory")
            {
                Ict.Petra.Client.MFinance.Gui.Gift.TFrmDonorRecipientHistory.OpenWindowDonorRecipientHistory(AToolStripItem.Name,
                    PartnerKey,
                    FPetraUtilsObject.GetForm());
            }
            else
            {
                throw new NotImplementedException();
            }

#if TODO
            String ClickedMenuItemText;

            ClickedMenuItemText = ((MenuItem)sender).Text;

            if (ClickedMenuItemText == mniEditCopyAddress.Text)
            {
                OpenCopyAddressToClipboardScreen();
            }
            else if (ClickedMenuItemText == mniEditCopyPartnerKey.Text)
            {
                FMenuFunctions.CopyPartnerKeyToClipboard();
            }
            else if (ClickedMenuItemText == mniEditCopyEmailAddress.Text)
            {
                FMenuFunctions.CopyEmailAddressToClipboard();
            }
#endif
        }

        void HandleMaintainMenuItemOrToolBarButton(ToolStripItem AToolStripItem)
        {
            string ClickedMenuItemName = AToolStripItem.Name;

            if (ClickedMenuItemName == "mniMaintainAddresses")
            {
                OpenPartnerEditScreen(TPartnerEditTabPageEnum.petpAddresses);
            }
            else if (ClickedMenuItemName == "mniMaintainPartnerDetails")
            {
                OpenPartnerEditScreen(TPartnerEditTabPageEnum.petpDetails);
            }
            else if (ClickedMenuItemName == "mniMaintainFoundationDetails")
            {
                OpenPartnerEditScreen(TPartnerEditTabPageEnum.petpFoundationDetails);
            }
            else if (ClickedMenuItemName == "mniMaintainSubscriptions")
            {
                OpenPartnerEditScreen(TPartnerEditTabPageEnum.petpSubscriptions);
            }
            else if (ClickedMenuItemName == "mniMaintainSpecialTypes")
            {
                OpenPartnerEditScreen(TPartnerEditTabPageEnum.petpPartnerTypes);
            }
            else if (ClickedMenuItemName == "mniMaintainContacts")
            {
                throw new NotImplementedException();
            }
            else if (ClickedMenuItemName == "mniMaintainFamilyMembers")
            {
                OpenPartnerEditScreen(TPartnerEditTabPageEnum.petpFamilyMembers);
            }
            else if (ClickedMenuItemName == "mniMaintainRelationships")
            {
                OpenPartnerEditScreen(TPartnerEditTabPageEnum.petpPartnerRelationships);
            }
            else if (ClickedMenuItemName == "mniMaintainInterests")
            {
                throw new NotImplementedException();
            }
            else if (ClickedMenuItemName == "mniMaintainReminders")
            {
                throw new NotImplementedException();
            }
            else if (ClickedMenuItemName == "mniMaintainNotes")
            {
                OpenPartnerEditScreen(TPartnerEditTabPageEnum.petpNotes);
            }
            else if (ClickedMenuItemName == "mniMaintainLocalPartnerData")
            {
                OpenPartnerEditScreen(TPartnerEditTabPageEnum.petpOfficeSpecific);
            }
            else if (ClickedMenuItemName == "mniMaintainWorkerField")
            {
                TFrmPersonnelStaffData staffDataForm = new TFrmPersonnelStaffData(FPetraUtilsObject.GetForm());

                staffDataForm.PartnerKey = FLogic.PartnerKey;
                staffDataForm.Show();
            }
            else if (AToolStripItem.Name == MPartnerResourcestrings.StrPersonnelPersonMenuItemText)
            {
                if (FLogic.DetermineCurrentPartnerClass() == SharedTypes.PartnerClassEnumToString(TPartnerClass.PERSON))
                {
                    OpenPartnerEditScreen(TPartnerEditTabPageEnum.petpPersonnelIndividualData);
                }
                else
                {
                    throw new NotImplementedException();
                }
            }
            else if (ClickedMenuItemName == "mniMaintainDonorHistory")
            {
//              TMenuFunctions.OpenDonorGiftHistory(this);
                Ict.Petra.Client.MFinance.Gui.Gift.TFrmDonorRecipientHistory.OpenWindowDonorRecipientHistory("mniMaintainDonorHistory",
                    PartnerKey,
                    FPetraUtilsObject.GetForm());
            }
            else if (ClickedMenuItemName == "mniMaintainRecipientHistory")
            {
//              TMenuFunctions.OpenRecipientGiftHistory(this);
                Ict.Petra.Client.MFinance.Gui.Gift.TFrmDonorRecipientHistory.OpenWindowDonorRecipientHistory("mniMaintainRecipientHistory",
                    PartnerKey,
                    FPetraUtilsObject.GetForm());
            }
            else if (ClickedMenuItemName == "mniMaintainFinanceReports")
            {
                throw new NotImplementedException();
            }
            else if (ClickedMenuItemName == "mniMaintainBankAccounts")
            {
                throw new NotImplementedException();
            }
            else if (ClickedMenuItemName == "mniMaintainGiftReceipting")
            {
                throw new NotImplementedException();
            }
            else if (ClickedMenuItemName == "mniMaintainFinanceDetails")
            {
                OpenPartnerEditScreen(TPartnerEditTabPageEnum.petpFinanceDetails);
            }
        }

        void HandleMailingMenuItemOrToolBarButton(ToolStripItem AToolStripItem)
        {
            throw new NotImplementedException();
#if TODO
            String ClickedMenuItemText;

            ClickedMenuItemText = ((MenuItem)sender).Text;

            if (ClickedMenuItemText == mniMailingExtracts.Text)
            {
                TMenuFunctions.OpenExtracts();
            }
            else if (ClickedMenuItemText == mniMailingDuplicateAddressCheck.Text)
            {
                TMenuFunctions.DuplicateAddressCheck();
            }
            else if (ClickedMenuItemText == mniMailingMergeAddresses.Text)
            {
                TMenuFunctions.MergeAddresses();
            }
            else if (ClickedMenuItemText == mniMailingPartnersAtLocation.Text)
            {
                OpenPartnerFindForLocation();
            }
            else if (ClickedMenuItemText == mniMailingSubscriptionCancellation.Text)
            {
                TMenuFunctions.SubscriptionCancellation();
            }
            else if (ClickedMenuItemText == mniMailingSubscriptionExpNotice.Text)
            {
                TMenuFunctions.SubscriptionExpiryNotices();
            }
            else if (ClickedMenuItemText == mniMailingGenerateExtract.Text)
            {
                CreateNewExtractFromFoundPartners();
            }
#endif
        }

        void HandleToolsMenuItemOrToolBarButton(ToolStripItem AToolStripItem)
        {
            throw new NotImplementedException();
        }

        void HandleViewMenuItemOrToolBarButton(ToolStripItem AToolStripItem)
        {
            if ((AToolStripItem.Name == "mniViewPartnerInfo")
                || (AToolStripItem.Name == "tbbPartnerInfo"))
            {
                if (!FPartnerInfoPaneOpen)
                {
                    OpenPartnerInfoPane();
                }
                else
                {
                    ClosePartnerInfoPane();
                }
            }
            else
            {
                throw new NotImplementedException();
            }

#if TODO
            else if (AToolStripItem.Name == "mniViewPartnerTasks")
            {
                if (!FPartnerTasksPaneOpen)
                {
                    OpenPartnerTasksPane();
                }
                else
                {
                    ClosePartnerTasksPane();
                }
            }
#endif
        }

        #endregion


        #region PartnerInfoPanel

        /// <summary>
        /// Updates the 'Partner Info' Panel if this Panel is shown and
        /// if there is a Find Result.
        /// </summary>
        /// <remarks>To avoid unneccessary roundtrips to the PetraServer, the
        /// 'Partner Info' Panel is only updated if the Partner/Location record
        /// combination has changed from the last time this Method was called.</remarks>
        void UpdatePartnerInfoPanel()
        {
            TLocationPK CurrentLocationPK;

            if (!ucoPartnerInfo.IsCollapsed)
            {
                CurrentLocationPK = FLogic.DetermineCurrentLocationPK();

                //                MessageBox.Show("Current PartnerKey: " + FLogic.PartnerKey.ToString() + Environment.NewLine +
                //                                "FLastPartnerKeyInfoPanelOpened: " + FLastPartnerKeyInfoPanelOpened.ToString() + Environment.NewLine +
                //                                "CurrentLocationPK: " + CurrentLocationPK.SiteKey.ToString() + ", " + CurrentLocationPK.LocationKey.ToString() + Environment.NewLine +
                //                                "FLastLocationPKInfoPanelOpened: " + FLastLocationPKInfoPanelOpened.SiteKey.ToString() + ", " + FLastLocationPKInfoPanelOpened.LocationKey.ToString());

                if ((FLogic.CurrentDataRow != null)
                    && (((FLastPartnerKeyInfoPanelOpened == FLogic.PartnerKey)
                         && ((FLastLocationPKInfoPanelOpened.SiteKey != CurrentLocationPK.SiteKey)
                             || (FLastLocationPKInfoPanelOpened.LocationKey != CurrentLocationPK.LocationKey)))
                        || (FLastPartnerKeyInfoPanelOpened != FLogic.PartnerKey)))
                {
                    FLastPartnerKeyInfoPanelOpened = FLogic.PartnerKey;
                    FLastLocationPKInfoPanelOpened = CurrentLocationPK;

                    if ((chkDetailedResults.Checked)
                        || ((!chkDetailedResults.Checked)
                            && FLastSearchWasDetailedSearch))
                    {
                        // We have Location data available
                        FPartnerInfoUC.PassPartnerDataPartialWithLocation(FLogic.PartnerKey, FLogic.CurrentDataRow);
                    }
                    else
                    {
                        // We don't have Location data available
                        FPartnerInfoUC.PassPartnerDataPartialWithoutLocation(FLogic.PartnerKey, FLogic.CurrentDataRow);
                    }
                }
            }
        }

        void UcoPartnerInfo_Collapsed(object sender, EventArgs e)
        {
            //            MessageBox.Show("UcoPartnerInfo_Collapsed");
            ClosePartnerInfoPane(true);
        }

        void UcoPartnerInfo_Expanded(object sender, EventArgs e)
        {
            //            MessageBox.Show("UcoPartnerInfo_Expanded");

            OpenPartnerInfoPane(true);
        }

        void TogglePartnerInfoPane()
        {
            if (!FPartnerInfoPaneOpen)
            {
                OpenPartnerInfoPane();
            }
            else
            {
                ClosePartnerInfoPane();
            }
        }

        private void OpenPartnerInfoPane(bool AUserControlIsArleadyExpanded = false)
        {
            OnPartnerInfoPaneExpanded();

            if (!AUserControlIsArleadyExpanded)
            {
                ucoPartnerInfo.Expand();
            }

            if (FPartnerInfoUC == null)
            {
                FPartnerInfoUC = ((TUC_PartnerInfo)(ucoPartnerInfo.UserControlInstance));
            }

            UpdatePartnerInfoPanel();

            //            MessageBox.Show("FCurrentGridRow: " + FCurrentGridRow.ToString());
            if (FCurrentGridRow != -1)
            {
                // Scroll the selected Row into view
                // FIXME: this has an undesired side-effect if the selected Row is not hidden
                // behind the Partner Info pane, but is scrolled off the top of the Grid.
                //                bool CellWasVisible = grdResult.ShowCell(new Position(FCurrentGridRow, 0), false);

                /*
                 * TODO: Ideally. we would not have row displayed as the top row, but the bottm row.
                 * The approach below works - unless grdResult.ShowCell didn't put the row as the
                 * top row. No solution to that yet...
                 */

                //                if (!CellWasVisible)
                //                {
                //                    // Scrolling was necessary. Now the Row is displayed as the top row,
                //                    // but we want it to be displayed as the middle row. Therefore we need
                //                    // to scroll the Grid up until it is the middle row...
                ////                    MessageBox.Show("Scrolled FCurrentGridRow: " + FCurrentGridRow.ToString() + " into view.");
//
                //                    int ScrollAdditionalRows = (grdResult.Rows.LastVisibleScrollableRow.Value -
                //                        grdResult.Rows.FirstVisibleScrollableRow.Value) / 2;
                //                    MessageBox.Show("ScrollAdditionalRows: " + ScrollAdditionalRows.ToString());
//
                //                    for (int Counter = 1; Counter < ScrollAdditionalRows; Counter++)
                //                    {
                ////                        MessageBox.Show("about to scroll one line up...");
                //                        grdResult.CustomScrollLineUp();
                //                    }
                //                }
            }

            FPartnerInfoPaneOpen = true;
        }

        private void ClosePartnerInfoPane(bool AUserControlIsArleadyCollapsed = false)
        {
            OnPartnerInfoPaneCollapsed();

            if (!AUserControlIsArleadyCollapsed)
            {
                ucoPartnerInfo.Collapse();
            }

            FPartnerInfoPaneOpen = false;
        }

        #endregion

        #region Main functionality

        /// <summary>
        /// Starts the Search operation.
        ///
        /// </summary>
        /// <returns>void</returns>
        private void PerformSearch()
        {
            Thread FinishedCheckThread;

            // Start the asynchronous search operation on the PetraServer
            FPartnerFindObject.PerformSearch(FCriteriaData, chkDetailedResults.Checked);

            // Start thread that checks for the end of the search operation on the PetraServer
            FinishedCheckThread = new Thread(new ThreadStart(SearchFinishedCheckThread));
            FinishedCheckThread.Start();
        }

        /// <summary>
        /// Returns the values of the found partner.
        /// </summary>
        /// <param name="APartnerKey">Partner key</param>
        /// <param name="AShortName">Partner short name</param>
        /// <param name="ALocationPK">Location key</param>
        /// <returns></returns>
        public Boolean GetReturnedParameters(out Int64 APartnerKey, out String AShortName, out TLocationPK ALocationPK)
        {
            DataRowView[] SelectedGridRow = grdResult.SelectedDataRowsAsDataRowView;

            if (SelectedGridRow.Length <= 0)
            {
                // no Row is selected
                APartnerKey = -1;
                AShortName = "";
                ALocationPK = new TLocationPK(-1, -1);
            }
            else
            {
                // MessageBox.Show(SelectedGridRow[0]['p_partner_key_n'].ToString);
                APartnerKey = Convert.ToInt64(SelectedGridRow[0][PPartnerTable.GetPartnerKeyDBName()]);
                AShortName = Convert.ToString(SelectedGridRow[0][PPartnerTable.GetPartnerShortNameDBName()]);
                ALocationPK =
                    new TLocationPK(Convert.ToInt64(SelectedGridRow[0][PPartnerLocationTable.GetSiteKeyDBName()]),
                        Convert.ToInt32(SelectedGridRow[0][PPartnerLocationTable.GetLocationKeyDBName()]));
            }

            return true;
        }

        /// <summary>
        /// Event procedure that acts on ucoPartnerFindCriteria's OnCriteriaContentChanged
        /// event.
        ///
        /// </summary>
        /// <returns>void</returns>
        private void UcoPartnerFindCriteria_CriteriaContentChanged(System.Object ASender, EventArgs AArgs)
        {
            btnSearch.Enabled = true;
            btnClearCriteria.Enabled = true;
            FCriteriaContentChanged = true;
        }

        #endregion

        #region Setup SourceDataGrid

        /// <summary>
        /// Sets up the DataBinding of the Grid.
        ///
        /// </summary>
        /// <returns>void</returns>
        private void SetupDataGridDataBinding()
        {
            DataView FindResultPagedDV;

            FindResultPagedDV = FPagedDataTable.DefaultView;
            FindResultPagedDV.AllowNew = false;
            FindResultPagedDV.AllowDelete = false;
            FindResultPagedDV.AllowEdit = false;
            grdResult.DataSource = new DevAge.ComponentModel.BoundDataView(FindResultPagedDV);

            // Hook up event that fires when a different Row is selected
            grdResult.Selection.FocusRowEntered += new RowEventHandler(this.DataGrid_FocusRowEntered);
        }

        /// <summary>
        /// Sets up the visual appearance of the Grid.
        ///
        /// </summary>
        /// <returns>void</returns>
        private void SetupDataGridVisualAppearance()
        {
            // Make PartnerClass and PartnerName fixed columns
            grdResult.FixedColumns = 2;
            grdResult.AutoSizeCells();
        }

        #endregion

        #region Form events

        /// <summary>
        /// set the criteria from user default
        /// </summary>
        public void InitialisePartnerFindCriteria()
        {
            ucoPartnerFindCriteria.PetraUtilsObject = FPetraUtilsObject;
            ucoPartnerFindCriteria.InitUserControl();

            // Load items that should go into the left and right columns from User Defaults
            ucoPartnerFindCriteria.CriteriaFieldsLeft =
                new ArrayList(TUserDefaults.GetStringDefault(TUserDefaults.PARTNER_FINDOPTIONS_CRITERIAFIELDSLEFT,
                        TFindOptionsForm.PARTNER_FINDOPTIONS_CRITERIAFIELDSLEFT_DEFAULT).Split(new Char[] { (';') }));
            ucoPartnerFindCriteria.CriteriaFieldsRight =
                new ArrayList(TUserDefaults.GetStringDefault(TUserDefaults.PARTNER_FINDOPTIONS_CRITERIAFIELDSRIGHT,
                        TFindOptionsForm.PARTNER_FINDOPTIONS_CRITERIAFIELDSRIGHT_DEFAULT).Split(new Char[] { (';') }));

            ucoPartnerFindCriteria.DisplayCriteriaFieldControls();
            ucoPartnerFindCriteria.InitialiseCriteriaFields();

            // Load match button settings
            ucoPartnerFindCriteria.LoadMatchButtonSettings();

            // Register Event Handler for the OnCriteriaContentChanged event
            ucoPartnerFindCriteria.OnCriteriaContentChanged += new EventHandler(this.UcoPartnerFindCriteria_CriteriaContentChanged);
        }

        private void RestoreSplitterSettings()
        {
            spcPartnerFindByDetails.SplitterDistance = TUserDefaults.GetInt32Default(
                TUserDefaults.PARTNER_FIND_SPLITPOS_FINDBYDETAILS, 233);
            // TODO FSplitterDistFindByDetails = spcPartnerFindByDetails.SplitterDistance;
        }

        /// <summary>
        /// search for the partners with the currently entered criteria
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void BtnSearch_Click(System.Object sender, System.EventArgs e)
        {
            if (!FKeepUpSearchFinishedCheck)
            {
                FPartnerInfoUC = ((TUC_PartnerInfo)(ucoPartnerInfo.UserControlInstance));

                // get the data from the currently edited control;
                // otherwise there are not the right search criteria when hitting the enter key
                ucoPartnerFindCriteria.CompleteBindings();

                FCriteriaData = ucoPartnerFindCriteria.CriteriaData;

                if (!ucoPartnerFindCriteria.HasSearchCriteria())
                {
                    MessageBox.Show(MPartnerResourcestrings.StrNoCriteriaSpecified);
                    return;
                }

                ucoPartnerFindCriteria.CriteriaData.Rows[0]["ExactPartnerKeyMatch"] = TUserDefaults.GetBooleanDefault(
                    TUserDefaults.PARTNER_FINDOPTIONS_EXACTPARTNERKEYMATCHSEARCH,
                    true);

                // used to destory server object here
                // Update UI
                grdResult.SendToBack();
                grpResult.Text = MPartnerResourcestrings.StrSearchResult;

                if (FPartnerInfoUC != null)
                {
                    FPartnerInfoUC.ClearControls();
                }

                lblSearchInfo.Text = MPartnerResourcestrings.StrSearching;
                FPetraUtilsObject.SetStatusBarText(btnSearch, MPartnerResourcestrings.StrSearching);

                //                stbMain.Panels[stbMain.Panels.IndexOf(stpInfo)].Text = Resourcestrings.StrSearching;

                this.Cursor = Cursors.AppStarting;
                FKeepUpSearchFinishedCheck = true;
                EnableDisableUI(false);
                Application.DoEvents();

/*
 *              // If ctrl held down, show the dataset
 *              if (System.Windows.Forms.Form.ModifierKeys == Keys.Control)
 *              {
 *                  MessageBox.Show(ucoPartnerFindCriteria.CriteriaData.DataSet.GetXml().ToString());
 *                  MessageBox.Show(ucoPartnerFindCriteria.CriteriaData.DataSet.GetChanges().GetXml().ToString());
 *              }
 */
                // Clear result table
                try
                {
                    if (FPagedDataTable != null)
                    {
                        FPagedDataTable.Clear();
                    }
                }
                catch (Exception)
                {
                    // don't do anything since this happens if the DataTable has no data yet
                }

                // Reset internal status variables
                FLastSearchWasDetailedSearch = chkDetailedResults.Checked;
                FCriteriaContentChanged = false;
                FLastPartnerKeyInfoPanelOpened = -1;
                FLastLocationPKInfoPanelOpened = new TLocationPK(-1, -1);

                // Start asynchronous search operation
                this.PerformSearch();
            }
            else
            {
                // Asynchronous search operation is being interrupted
                btnSearch.Enabled = false;
                lblSearchInfo.Text = MPartnerResourcestrings.StrStoppingSearch;
                FPetraUtilsObject.WriteToStatusBar(MPartnerResourcestrings.StrStoppingSearch);
                FPetraUtilsObject.SetStatusBarText(btnSearch, MPartnerResourcestrings.StrStoppingSearch);

                Application.DoEvents();

                // Stop asynchronous search operation
                FPartnerFindObject.AsyncExecProgress.Cancel();
            }
        }

        private void BtnClearCriteria_Click(System.Object sender, System.EventArgs e)
        {
            ucoPartnerFindCriteria.ResetSearchCriteriaValuesToDefault();

            if (FPartnerInfoUC != null)
            {
                FPartnerInfoUC.ClearControls();
            }

            lblSearchInfo.Text = "";
            grpResult.Text = MPartnerResourcestrings.StrSearchResult;
            grdResult.SendToBack();

            OnPartnerAvailable(false);
            OnDisableAcceptButton();

            ucoPartnerFindCriteria.Focus();

            grdResult.DataSource = null;
            FPagedDataTable = null;

            FCurrentGridRow = -1;
        }

        /// <summary>
        /// Opens the Partner Edit Screen.
        /// </summary>
        /// <param name="AShowTabPage">Tab Page to open the Partner Edit screen on.</param>
        private void OpenPartnerEditScreen(TPartnerEditTabPageEnum AShowTabPage)
        {
            OpenPartnerEditScreen(AShowTabPage, FLogic.PartnerKey, false);
        }

        /// <summary>
        /// Opens the Partner Edit Screen.
        /// </summary>
        /// <param name="AShowTabPage">Tab Page to open the Partner Edit screen on.</param>
        /// <param name="APartnerKey">PartnerKey for which the Partner Edit screen should be openened.</param>
        /// <param name="AOpenOnBestLocation">Set to true to open the Partner with the 'Best Address' selected (affects the Addresses Tab only).</param>
        public void OpenPartnerEditScreen(TPartnerEditTabPageEnum AShowTabPage, Int64 APartnerKey, bool AOpenOnBestLocation)
        {
            FPetraUtilsObject.WriteToStatusBar("Opening Partner in Partner Edit screen...");
            FPetraUtilsObject.SetStatusBarText(grdResult, "Opening Partner in Partner Edit screen...");
            this.Cursor = Cursors.WaitCursor;

            // Set Partner to be the "Last Used Partner"
            TUserDefaults.SetDefault(TUserDefaults.USERDEFAULT_LASTPARTNERMAILROOM, APartnerKey);

            try
            {
                TFrmPartnerEdit frm = new TFrmPartnerEdit(FPetraUtilsObject.GetForm());

                if (!AOpenOnBestLocation)
                {
                    frm.SetParameters(TScreenMode.smEdit, APartnerKey,
                        FLogic.DetermineCurrentLocationPK().SiteKey, FLogic.DetermineCurrentLocationPK().LocationKey, AShowTabPage);
                }
                else
                {
                    frm.SetParameters(TScreenMode.smEdit, APartnerKey);
                }

                frm.Show();
            }
            finally
            {
                this.Cursor = Cursors.Default;
                FPetraUtilsObject.SetStatusBarText(grdResult,
                    MPartnerResourcestrings.StrResultGridHelpText + MPartnerResourcestrings.StrPartnerFindSearchTargetText);
            }
        }

        private void OpenNewPartnerEditScreen(bool ARunAsModalForm)
        {
            string PartnerClass = String.Empty;
            TFrmPartnerEdit frm;

            this.Cursor = Cursors.WaitCursor;

            try
            {
                if (!ARunAsModalForm)
                {
                    // Not modal, so no restrictions on valid partner classes
                }
                else
                {
                    // Modal. May have restrictions, may not.
                    // Default behavior is to allow all Partner Classes

                    if (ucoPartnerFindCriteria.RestrictedPartnerClass.Length > 0)
                    {
                        /* at least one entry so use first one */
                        PartnerClass = ucoPartnerFindCriteria.RestrictedPartnerClass[0];
                    }

                    /*
                     * Create (and remember!) a GUID that we pass to the 'Partner Edit' screen
                     * for the new Partner. This is used in Method 'ProcessFormsMessage' to
                     * determine whether the 'Form Message' received is for *this* Instance
                     * of the Modal Partner Find screen.
                     */
// TODO             FNewPartnerContext = System.Guid.NewGuid().ToString();

                    PartnerClass = PartnerClass.Replace("OM-FAM", "FAMILY");
                }

                frm = new Ict.Petra.Client.MPartner.Gui.TFrmPartnerEdit(FPetraUtilsObject.GetForm());

                frm.SetParameters(TScreenMode.smNew,
                    PartnerClass, -1, -1, String.Empty);
// TODO                frm.CallerContext = FNewPartnerContext;

                if (!ARunAsModalForm)
                {
                    frm.Show();
                }
                else
                {
                    frm.ShowDialog();
                }
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        /// <summary>
        /// Opens the "copy address" dialog
        /// </summary>
        public void OpenCopyAddressToClipboardScreen()
        {
// TODO OpenCopyAddressToClipboardScreen
#if TODO
            TLocationPK LocationPK;

            Ict.Petra.Client.MPartner.
            TCopyPartnerAddressDialogWinForm cpad = new TCopyPartnerAddressDialogWinForm();

            LocationPK = FLogic.DetermineCurrentLocationPK();

            cpad.SetParameters(FLogic.PartnerKey, LocationPK.SiteKey, LocationPK.LocationKey);
            cpad.ShowDialog();
            cpad.Dispose();
#endif
        }

        private void BtnCustomCriteriaDemo_Click(System.Object sender, System.EventArgs e)
        {
            String[] CriteraFieldArray;
            CriteraFieldArray =
                "PartnerKey;abc;Spacer;Address1;Spacer;PartnerName;PersonalName;City;Country;PersonnelCriteria".Split(new Char[] { (';') });
            ucoPartnerFindCriteria.CriteriaFieldsLeft = new ArrayList(CriteraFieldArray);
            CriteraFieldArray = "OMSSKey;PartnerClass;PostCode;Status;Spacer;MailingAddressOnly".Split(new Char[] { (';') });
            ucoPartnerFindCriteria.CriteriaFieldsRight = new ArrayList(CriteraFieldArray);
            ucoPartnerFindCriteria.DisplayCriteriaFieldControls();
        }

        #endregion

        #region Helper functions

        /// <summary>
        /// Opens the Find Options dialog.
        ///
        /// </summary>
        /// <returns>void</returns>
        private void OpenFindOptions()
        {
// TODO OpenFindOptions
#if TODO
            TFindOptionsForm OptionsDialog;

            OptionsDialog = new TFindOptionsForm();
            OptionsDialog.ShowDialog();

            if (OptionsDialog.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                if (TUserDefaults.GetStringDefault(TUserDefaults.PARTNER_FINDOPTIONS_CRITERIAFIELDSLEFT, "") != "")
                {
                    ucoPartnerFindCriteria.CriteriaFieldsLeft =
                        new ArrayList(TUserDefaults.GetStringDefault(TUserDefaults.PARTNER_FINDOPTIONS_CRITERIAFIELDSLEFT,
                                "").Split(new Char[] { (';') }));
                }
                else
                {
                    ucoPartnerFindCriteria.CriteriaFieldsLeft = new ArrayList();
                }

                if (TUserDefaults.GetStringDefault(TUserDefaults.PARTNER_FINDOPTIONS_CRITERIAFIELDSRIGHT, "") != "")
                {
                    ucoPartnerFindCriteria.CriteriaFieldsRight =
                        new ArrayList(TUserDefaults.GetStringDefault(TUserDefaults.PARTNER_FINDOPTIONS_CRITERIAFIELDSRIGHT,
                                "").Split(new Char[] { (';') }));
                }
                else
                {
                    ucoPartnerFindCriteria.CriteriaFieldsRight = new ArrayList();
                }

                BtnClearCriteria_Click(this, null);
                ucoPartnerFindCriteria.DisplayCriteriaFieldControls();
                ucoPartnerFindCriteria.ShowOrHidePartnerKeyMatchInfoText();
            }
#endif
        }

        private void OpenPartnerFindForLocation()
        {
            TPartnerFindScreen frmPF;
            TLocationPK LocationPK;

            this.Cursor = Cursors.WaitCursor;
            LocationPK = FLogic.DetermineCurrentLocationPK();
            frmPF = new TPartnerFindScreen(FPetraUtilsObject.GetForm());
            frmPF.SetParameters(true, LocationPK.LocationKey);
            frmPF.Show();
            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// Checks if the current user can access this Partner.
        /// </summary>
        /// <param name="APartnerKey">The PartnerKey to check. Pass in -1 to use the
        /// PartnerKey of the currently selected Partner in the Search Result Grid.</param>
        /// <returns>True if the Partner can be accessed, otherwise false.</returns>
        public bool CanAccessPartner(Int64 APartnerKey)
        {
            return FLogic.CanAccessPartner(APartnerKey);
        }

        /// <summary>
        /// Enables and disables the UI. Invokes setting up of the Grid after a
        /// successful search operation.
        /// </summary>
        /// <returns>void</returns>
        private void EnableDisableUI(System.Object AEnable)
        {
            object[] Args;
            TMyUpdateDelegate MyUpdateDelegate;
            String SearchTarget;
            bool UpdateUI;
            Int64 MergedPartnerKey;

            // Since this procedure is called from a separate (background) Thread, it is
            // necessary to executethis procedure in the Thread of the GUI
            if (btnSearch.InvokeRequired)
            {
                Args = new object[1];

                // messagebox.show('btnEditPartner.InvokeRequired: yes; AEnable: ' + Convert.ToBoolean(AEnable).ToString);
                try
                {
                    MyUpdateDelegate = new TMyUpdateDelegate(EnableDisableUI);
                    Args[0] = AEnable;
                    btnSearch.Invoke(MyUpdateDelegate, new object[] { AEnable });

                    // messagebox.show('Invoke finished!');
                }
                finally
                {
                    Args = new object[0];
                }
            }
            else
            {
                // Enable/disable buttons for working with found Partners
                OnPartnerAvailable(Convert.ToBoolean(AEnable));

                // Enable/disable according to how the search operation ended
                if (Convert.ToBoolean(AEnable))
                {
                    if (FPartnerFindObject.AsyncExecProgress.ProgressState != TAsyncExecProgressState.Aeps_Stopped)
                    {
                        // Search operation ended without interruption
                        if (FPagedDataTable.Rows.Count > 0)
                        {
                            // At least one result was found by the search operation
                            lblSearchInfo.Text = "";
                            this.Cursor = Cursors.Default;

                            //
                            // Setup result DataGrid
                            //
                            SetupResultDataGrid();
                            grdResult.BringToFront();

                            // Make the Grid respond on updown keys
                            grdResult.Focus();
                            DataGrid_FocusRowEntered(this, new RowEventArgs(1));

                            // Display the number of found Partners/Locations
                            if (grdResult.TotalRecords > 1)
                            {
                                SearchTarget = MPartnerResourcestrings.StrPartnerFindSearchTargetPluralText;
                            }
                            else
                            {
                                SearchTarget = MPartnerResourcestrings.StrPartnerFindSearchTargetText;
                            }

                            grpResult.Text = MPartnerResourcestrings.StrSearchResult + ": " + grdResult.TotalRecords.ToString() + ' ' +
                                             SearchTarget + ' ' +
                                             MPartnerResourcestrings.StrFoundText;
                        }
                        else
                        {
                            // Search operation has found nothing

                            /*
                             * If PartnerKey wasn't 0, check if the Partner searched for
                             * was a MERGED Partner.
                             */
                            if ((Int64)FCriteriaData.Rows[0]["PartnerKey"] != 0)
                            {
                                if (TPartnerMain.MergedPartnerHandling(
                                        (Int64)FCriteriaData.Rows[0]["PartnerKey"],
                                        out MergedPartnerKey))
                                {
                                    FCriteriaData.Rows[0]["PartnerKey"] = MergedPartnerKey;
                                    BtnSearch_Click(this, null);
                                    UpdateUI = false;
                                }
                                else
                                {
                                    UpdateUI = true;
                                }
                            }
                            else
                            {
                                UpdateUI = true;
                            }

                            if (UpdateUI)
                            {
                                this.Cursor = Cursors.Default;
                                grpResult.Text = MPartnerResourcestrings.StrSearchResult;
                                lblSearchInfo.Text = MPartnerResourcestrings.StrNoRecordsFound1Text + ' ' +
                                                     MPartnerResourcestrings.StrPartnerFindSearchTarget2Text +
                                                     MPartnerResourcestrings.StrNoRecordsFound2Text;

                                OnDisableAcceptButton();
                                OnPartnerAvailable(false);

                                // StatusBar update
                                FPetraUtilsObject.WriteToStatusBar(MCommonResourcestrings.StrGenericReady);
                                FPetraUtilsObject.SetStatusBarText(btnSearch, MPartnerResourcestrings.StrSearchButtonHelpText);

                                FCurrentGridRow = -1;
                            }
                            else
                            {
                                return;
                            }
                        }

                        /*
                         * Saves 'Mailing Addresses Only' and 'Partner Status' Criteria
                         * settings to UserDefaults.
                         */
                        ucoPartnerFindCriteria.FindCriteriaUserDefaultSave();
                    }
                    else
                    {
                        // Search operation interrupted by user
                        // used to release server System.Object here
                        this.Cursor = Cursors.Default;
                        grpResult.Text = MPartnerResourcestrings.StrSearchResult;
                        lblSearchInfo.Text = MPartnerResourcestrings.StrSearchStopped;

                        OnPartnerAvailable(false);
                        btnSearch.Enabled = true;

                        // StatusBar update

                        //                        WriteToStatusBar(CommonResourcestrings.StrGenericReady);
                        FPetraUtilsObject.SetStatusBarText(btnSearch, MPartnerResourcestrings.StrSearchButtonHelpText);

                        FCurrentGridRow = -1;
                    }
                }

                // Enable/disable remaining controls
                btnClearCriteria.Enabled = Convert.ToBoolean(AEnable);
                chkDetailedResults.Enabled = Convert.ToBoolean(AEnable);
                ucoPartnerFindCriteria.Enabled = Convert.ToBoolean(AEnable);

                // Set search button text
                if (Convert.ToBoolean(AEnable))
                {
                    btnSearch.Text = MPartnerResourcestrings.StrSearchButtonText;
                    OnSearchOperationStateChange(false);
                }
                else
                {
                    btnSearch.Text = MPartnerResourcestrings.StrSearchButtonStopText;
                    OnSearchOperationStateChange(true);
                }

                // messagebox.show('EnableDisableUI ran!');
            }
        }

        #endregion

        #region Threading helper functions

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
                /* The next line of code calls a function on the PetraServer
                 * > causes a bit of data traffic everytime! */
                switch (FPartnerFindObject.AsyncExecProgress.ProgressState)
                {
                    case TAsyncExecProgressState.Aeps_Finished:
                        FKeepUpSearchFinishedCheck = false;

                        // Fetch the first page of data
                        try
                        {
                            FPagedDataTable = grdResult.LoadFirstDataPage(@GetDataPagedResult);
                        }
                        catch (Exception E)
                        {
                            MessageBox.Show(E.ToString());
                        }
                        break;

                    case TAsyncExecProgressState.Aeps_Stopped:
                        FKeepUpSearchFinishedCheck = false;
                        EnableDisableUI(true);
                        return;
                }

                // Sleep for some time. After that, this function is called again automatically.
                Thread.Sleep(200);
            }

            EnableDisableUI(true);
        }

        #endregion

        void BtnTogglePartnerDetailsClick(object sender, EventArgs e)
        {
            TogglePartnerInfoPane();
        }

        void spcPartnerFindByDetails_SplitterMoved(System.Object sender, System.Windows.Forms.SplitterEventArgs e)
        {
            // TODO FSplitterDistFindByDetails = ((SplitContainer)sender).SplitterDistance;
        }

        private void GrpCriteria_Enter(System.Object sender, System.EventArgs e)
        {
        }

        /// <summary>
        /// Sets up paged search result DataTable with the result of the Servers query
        /// and DataBind the DataGrid.
        ///
        /// </summary>
        /// <returns>void</returns>
        private void SetupResultDataGrid()
        {
            ArrayList FieldList;

            try
            {
                if (grdResult.TotalPages > 0)
                {
                    FieldList = new ArrayList();

                    foreach (string eachField in ucoPartnerFindCriteria.CriteriaFieldsLeft)
                    {
                        FieldList.Add(eachField);
                    }

                    foreach (string eachField in ucoPartnerFindCriteria.CriteriaFieldsRight)
                    {
                        FieldList.Add(eachField);
                    }

                    // Create SourceDataGrid columns
                    FLogic.CreateColumns(FPagedDataTable, chkDetailedResults.Checked,
                        FCriteriaData.Rows[0]["PartnerStatus"].ToString() != "ACTIVE", FieldList);

                    // DataBindingrelated stuff
                    SetupDataGridDataBinding();

                    // Setup the DataGrid's visual appearance
                    SetupDataGridVisualAppearance();

                    // Select (highlight) first Row
                    grdResult.Selection.SelectRow(1, true);

                    // Scroll grid to first line (the grid might have been scrolled before to another position)
                    grdResult.ShowCell(new Position(1, 1), true);

                    OnEnableAcceptButton();
                }
                else
                {
                    OnDisableAcceptButton();
                }
            }
            catch (Exception exp)
            {
                MessageBox.Show("Exception occured in SetupResultDataGrid: " + exp.Message + exp.StackTrace);
            }
        }

        /// <summary>
        /// todoComment
        /// </summary>
        /// <param name="ANeededPage"></param>
        /// <param name="APageSize"></param>
        /// <param name="ATotalRecords"></param>
        /// <param name="ATotalPages"></param>
        /// <returns></returns>
        public DataTable GetDataPagedResult(Int16 ANeededPage, Int16 APageSize, out Int32 ATotalRecords, out Int16 ATotalPages)
        {
            ATotalRecords = 0;
            ATotalPages = 0;

            if (FPartnerFindObject != null)
            {
                return FPartnerFindObject.GetDataPagedResult(ANeededPage, APageSize, out ATotalRecords, out ATotalPages);
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// called when closing the window
        /// </summary>
        public void StoreUserDefaults()
        {
            // Save Find Criteria Match button settings
            ucoPartnerFindCriteria.SaveMatchButtonSettings();

            // Save SplitContainer settings
            ucoPartnerFindCriteria.SaveSplitterSetting();
            TUserDefaults.SetDefault(TUserDefaults.PARTNER_FIND_SPLITPOS_FINDBYDETAILS, spcPartnerFindByDetails.SplitterDistance);

            // Save Partner Info Pane and Partner Task Pane settings
            TUserDefaults.SetDefault(TUserDefaults.PARTNER_FIND_PARTNERDETAILS_OPEN, FPartnerInfoPaneOpen);
            TUserDefaults.SetDefault(TUserDefaults.PARTNER_FIND_PARTNERTASKS_OPEN, FPartnerTasksPaneOpen);
        }

        /// <summary>todoComment</summary>
        public void StopTimer()
        {
            if (FPartnerInfoUC != null)
            {
                FPartnerInfoUC.StopTimer();
            }
        }

        /// <summary>
        /// Pass on FRestrictToPartnerClasses to UC_PartnerFind_Criteria
        /// </summary>
        /// <param name="AInitiallyFocusLocationKey"></param>
        /// <param name="ARestrictToPartnerClasses">this will be an empty array
        ///     or an array of strings to determine which partner classes are allowed</param>
        /// <param name="APassedLocationKey"></param>
        public void Init(
            bool AInitiallyFocusLocationKey,
            string[] ARestrictToPartnerClasses, Int32 APassedLocationKey)
        {
            InitialisePartnerFindCriteria();
            ucoPartnerFindCriteria.RestrictedPartnerClass = ARestrictToPartnerClasses;

            if (AInitiallyFocusLocationKey)
            {
                ucoPartnerFindCriteria.FocusLocationKey(APassedLocationKey);
            }

            ucoPartnerFindCriteria.Focus();
        }

        /// <summary>todoComment</summary>
        public IPartnerUIConnectorsPartnerFind PartnerFindObject
        {
            set
            {
                FPartnerFindObject = value;
            }
        }
    }
}