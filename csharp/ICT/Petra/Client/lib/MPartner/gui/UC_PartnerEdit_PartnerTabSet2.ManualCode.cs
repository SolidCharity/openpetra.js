/*************************************************************************
 *
 * DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
 *
 * @Authors:
 *       christiank
 *
 * Copyright 2004-2010 by OM International
 *
 * This file is part of OpenPetra.org.
 *
 * OpenPetra.org is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * OpenPetra.org is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with OpenPetra.org.  If not, see <http://www.gnu.org/licenses/>.
 *
 ************************************************************************/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Windows.Forms;

using Ict.Common;
using Ict.Common.Controls;
using Ict.Petra.Shared;
using Ict.Petra.Shared.Interfaces.MPartner.Partner.UIConnectors;
using Ict.Petra.Shared.MPartner;
using Ict.Petra.Shared.MPartner.Mailroom.Data;
using Ict.Petra.Shared.MPartner.Partner.Data;
using Ict.Petra.Shared.MPartner.Partner;
using Ict.Petra.Client.App.Core;
using Ict.Petra.Client.App.Gui;
using Ict.Petra.Client.MCommon;

namespace Ict.Petra.Client.MPartner.Gui
{
    public partial class TUC_PartnerEdit_PartnerTabSet2
    {
        #region TODO ResourceStrings

        /// <summary>todoComment</summary>
        public const String StrAddressesTabHeader = "Addresses";

        /// <summary>todoComment</summary>
        public const String StrSubscriptionsTabHeader = "Subscriptions";

        /// <summary>todoComment</summary>
        public const String StrSpecialTypesTabHeader = "Special Types";

        /// <summary>todoComment</summary>
        public const String StrFamilyMembersTabHeader = "Family Members";

        /// <summary>todoComment</summary>
        public const String StrFamilyTabHeader = "Family";

        /// <summary>todoComment</summary>
        public const String StrInterestsTabHeader = "Interests";

        /// <summary>todoComment</summary>
        public const String StrNotesTabHeader = "Notes";

        /// <summary>todoComment</summary>
        public const String StrAddressesSingular = "Address";

        /// <summary>todoComment</summary>
        public const String StrSubscriptionsSingular = "Subscription";

        /// <summary>todoComment</summary>
        public const String StrTabHeaderCounterTipSingular = "{0} {2}, of which {1} is ";

        /// <summary>todoComment</summary>
        public const String StrTabHeaderCounterTipPlural = "{0} {2}, of which {1} are ";

        #endregion

        #region Fields

        /// <summary>holds a reference to the Proxy System.Object of the Serverside UIConnector</summary>
        private IPartnerUIConnectorsPartnerEdit FPartnerEditUIConnector;

        private TPartnerEditTabPageEnum FInitiallySelectedTabPage = TPartnerEditTabPageEnum.petpDefault;

        private TPartnerEditTabPageEnum FCurrentlySelectedTabPage;

        private String FPartnerClass;

        private Boolean FUserControlInitialised;

        #endregion

        #region Public Events

        /// <summary>todoComment</summary>
        public event THookupDataChangeEventHandler HookupDataChange;

        /// <summary>todoComment</summary>
        public event THookupPartnerEditDataChangeEventHandler HookupPartnerEditDataChange;

        /// <summary>todoComment</summary>
        public event TEnableDisableScreenPartsEventHandler EnableDisableOtherScreenParts;

        #endregion

        #region Properties

        /// <summary>used for passing through the Clientside Proxy for the UIConnector</summary>
        public IPartnerUIConnectorsPartnerEdit PartnerEditUIConnector
        {
            get
            {
                return FPartnerEditUIConnector;
            }

            set
            {
                FPartnerEditUIConnector = value;
            }
        }

        /// <summary>todoComment</summary>
        public TPartnerEditTabPageEnum InitiallySelectedTabPage
        {
            get
            {
                return FInitiallySelectedTabPage;
            }

            set
            {
                FInitiallySelectedTabPage = value;
            }
        }

        /// <summary>todoComment</summary>
        public TPartnerEditTabPageEnum CurrentlySelectedTabPage
        {
            get
            {
                return FCurrentlySelectedTabPage;
            }

            set
            {
                FCurrentlySelectedTabPage = value;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Initialization of Manual Code logic.
        /// </summary>
        public void InitializeManualCode()
        {
            if (FTabPageEvent == null)
            {
                FTabPageEvent += this.TabPageEventHandler;
            }
        }

        /// <summary>
        /// todoComment
        /// </summary>
        public void SpecialInitUserControl()
        {
            ArrayList TabsToHide;

            OnDataLoadingStarted();

            // Determine which Tabs to show in the ucoPartnerTabSet
            FPartnerClass = FMainDS.PPartner[0].PartnerClass;
            TabsToHide = new ArrayList();

            if (FPartnerClass == "PERSON")
            {
                TabsToHide.Add("tpgFoundationDetails");

                // instead of 'Family Members (?)'
                tpgFamilyMembers.Text = "Family";

                // instead of 'FamilyMembers.ico'
                tpgFamilyMembers.ImageIndex = 4;
            }
            else if (FPartnerClass == "FAMILY")
            {
                // TabsToHide.Add('tbpFamily');
                TabsToHide.Add("tpgFoundationDetails");
            }
            else if (FPartnerClass == "CHURCH")
            {
                TabsToHide.Add("tpgFamilyMembers");
                TabsToHide.Add("tpgFoundationDetails");
            }
            else if (FPartnerClass == "ORGANISATION")
            {
                TabsToHide.Add("tpgFamilyMembers");

                if (!FMainDS.POrganisation[0].Foundation)
                {
                    TabsToHide.Add("tpgFoundationDetails");
                }
                else
                {
                    if (!TSecurity.CheckFoundationSecurity(
                            FMainDS.MiscellaneousData[0].FoundationOwner1Key,
                            FMainDS.MiscellaneousData[0].FoundationOwner2Key))
                    {
                        tpgFoundationDetails.Enabled = false;
                    }

                    if (!CheckSecurityOKToAccessNotesTab())
                    {
                        tpgNotes.Enabled = false;
                    }
                }
            }
            else if (FPartnerClass == "UNIT")
            {
                TabsToHide.Add("tpgFamilyMembers");
                TabsToHide.Add("tpgFoundationDetails");
            }
            else if (FPartnerClass == "BANK")
            {
                TabsToHide.Add("tpgFamilyMembers");
                TabsToHide.Add("tpgFoundationDetails");
            }
            else if (FPartnerClass == "VENUE")
            {
                TabsToHide.Add("tpgFamilyMembers");
                TabsToHide.Add("tpgFoundationDetails");
            }

            if (!FMainDS.MiscellaneousData[0].OfficeSpecificDataLabelsAvailable)
            {
                TabsToHide.Add("tbpOfficeSpecific");
            }

            // for the time beeing, we always hide these Tabs that don't do anything yet...
#if  SHOWUNFINISHEDTABS
#else
            TabsToHide.Add("tbpRelationships");
            TabsToHide.Add("tbpContacts");
            TabsToHide.Add("tbpReminders");
            TabsToHide.Add("tbpInterests");
#endif
            ControlsUtilities.HideTabs(tabPartners, TabsToHide);
            FUserControlInitialised = true;

//            this.tabPartners.SelectedIndexChanged += new EventHandler(this.tabPartners_SelectedIndexChanged);

            SelectTabPage(FInitiallySelectedTabPage);

            CalculateTabHeaderCounters(this);

            OnDataLoadingFinished();
        }

        /// <summary>
        /// Gets the data from all controls on this TabControl.
        /// The data is stored in the DataTables/DataColumns to which the Controls
        /// are mapped.
        /// </summary>
        public void GetDataFromControls()
        {
            TUC_PartnerDetails_Family2 UCPartnerDetailsFamily;

            UCPartnerDetailsFamily = (TUC_PartnerDetails_Family2)FTabSetup[TDynamicLoadableUserControls.dlucPartnerDetails];

            if (FTabSetup.ContainsKey(TDynamicLoadableUserControls.dlucPartnerDetails))
            {
                UCPartnerDetailsFamily.GetDataFromControls2();
            }
        }

        /// <summary>
        /// Tells whether a specific dynamically loadable Tab has been set up.
        /// </summary>
        /// <param name="ADynamicLoadableUserControl">The Tab.</param>
        /// <returns>True if it has been set up, otherwise false.</returns>
        public bool IsDynamicallyLoadableTabSetUp(TDynamicLoadableUserControls ADynamicLoadableUserControl)
        {
            if (FTabSetup.ContainsKey(ADynamicLoadableUserControl))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// todoComment
        /// </summary>
        public void SetUpPartnerAddress()
        {
            TUCPartnerAddresses UCAddresses;

            UCAddresses = (TUCPartnerAddresses)DynamicLoadUserControl(TDynamicLoadableUserControls.dlucAddresses);

            if (TClientSettings.DelayedDataLoading)
            {
                // Signalise the user that data is beeing loaded
                this.Cursor = Cursors.AppStarting;
            }

            UCAddresses.MainDS = FMainDS;
            UCAddresses.PetraUtilsObject = FPetraUtilsObject;
            UCAddresses.PartnerEditUIConnector = FPartnerEditUIConnector;
            UCAddresses.HookupDataChange += new THookupDataChangeEventHandler(this.Uco_HookupDataChange);
            UCAddresses.InitialiseUserControl();

            // MessageBox.Show('TabSetupPartnerAddresses finished');
            this.Cursor = Cursors.Default;
        }

        /// <summary>
        /// todoComment
        /// </summary>
        public void DisableNewButtonOnAutoCreatedAddress()
        {
            TUCPartnerAddresses UCAddresses;

            UCAddresses = (TUCPartnerAddresses)FTabSetup[TDynamicLoadableUserControls.dlucAddresses];

            UCAddresses.DisableNewButtonOnAutoCreatedAddress();
        }

        /// <summary>
        /// todoComment
        /// </summary>
        public void CleanupRecordsBeforeMerge()
        {
            TUCPartnerAddresses UCAddresses;

            UCAddresses = (TUCPartnerAddresses)FTabSetup[TDynamicLoadableUserControls.dlucAddresses];

            UCAddresses.CleanupRecordsBeforeMerge();
        }

        /// <summary>
        /// todoComment
        /// </summary>
        public void RefreshRecordsAfterMerge()
        {
            TUCPartnerAddresses UCAddresses;

            UCAddresses = (TUCPartnerAddresses)FTabSetup[TDynamicLoadableUserControls.dlucAddresses];

            UCAddresses.RefreshRecordsAfterMerge();
        }

        /// <summary>
        /// todoComment
        /// </summary>
        /// <param name="AParameterDT"></param>
        public void ProcessServerResponseSimilarLocations(PartnerAddressAggregateTDSSimilarLocationParametersTable AParameterDT)
        {
            TUCPartnerAddresses UCAddresses;

            UCAddresses = (TUCPartnerAddresses)FTabSetup[TDynamicLoadableUserControls.dlucAddresses];

            UCAddresses.ProcessServerResponseSimilarLocations(AParameterDT);
        }

        /// <summary>
        /// todoComment
        /// </summary>
        /// <param name="AAddedOrChangedPromotionDT"></param>
        /// <param name="AParameterDT"></param>
        public void ProcessServerResponseAddressAddedOrChanged(
            PartnerAddressAggregateTDSAddressAddedOrChangedPromotionTable AAddedOrChangedPromotionDT,
            PartnerAddressAggregateTDSChangePromotionParametersTable AParameterDT)
        {
            TUCPartnerAddresses UCAddresses;

            UCAddresses = (TUCPartnerAddresses)FTabSetup[TDynamicLoadableUserControls.dlucAddresses];

            UCAddresses.ProcessServerResponseAddressAddedOrChanged(AAddedOrChangedPromotionDT, AParameterDT);
        }

        #endregion

        #region Private Methods

        private void TabPageEventHandler(object sender, TTabPageEventArgs ATabPageEventArgs)
        {
            Ict.Petra.Client.MPartner.Gui.TUCPartnerTypes UCSpecialTypes;
            Ict.Petra.Client.MCommon.TUCPartnerAddresses UCAddresses;

            if (ATabPageEventArgs.Event == "FurtherInit")
            {
                if (ATabPageEventArgs.Tab == tpgAddresses)
                {
                    FCurrentlySelectedTabPage = TPartnerEditTabPageEnum.petpAddresses;

                    UCAddresses = (Ict.Petra.Client.MCommon.TUCPartnerAddresses)ATabPageEventArgs.UserControlOnTabPage;

                    // Hook up EnableDisableOtherScreenParts Event that is fired by UserControls on Tabs
                    UCAddresses.EnableDisableOtherScreenParts += new TEnableDisableScreenPartsEventHandler(
                        UcoTab_EnableDisableOtherScreenParts);

                    // Hook up RecalculateScreenParts Event
                    UCAddresses.RecalculateScreenParts += new TRecalculateScreenPartsEventHandler(RecalculateTabHeaderCounters);

                    UCAddresses.PartnerEditUIConnector = FPartnerEditUIConnector;
                    UCAddresses.HookupDataChange += new THookupDataChangeEventHandler(Uco_HookupDataChange);

                    UCAddresses.InitialiseUserControl();
                }
                else if (ATabPageEventArgs.Tab == tpgPartnerDetails)
                {
                    FCurrentlySelectedTabPage = TPartnerEditTabPageEnum.petpDetails;

                    // TODO
                }
                else if (ATabPageEventArgs.Tab == tpgFoundationDetails)
                {
                    FCurrentlySelectedTabPage = TPartnerEditTabPageEnum.petpFoundationDetails;

                    // TODO
                }
                else if (ATabPageEventArgs.Tab == tpgSubscriptions)
                {
                    FCurrentlySelectedTabPage = TPartnerEditTabPageEnum.petpSubscriptions;

                    // TODO
                }
                else if (ATabPageEventArgs.Tab == tpgPartnerTypes)
                {
                    FCurrentlySelectedTabPage = TPartnerEditTabPageEnum.petpPartnerTypes;

                    UCSpecialTypes = (Ict.Petra.Client.MPartner.Gui.TUCPartnerTypes)ATabPageEventArgs.UserControlOnTabPage;

                    // Hook up RecalculateScreenParts Event
                    UCSpecialTypes.RecalculateScreenParts += new TRecalculateScreenPartsEventHandler(RecalculateTabHeaderCounters);

                    UCSpecialTypes.PartnerEditUIConnector = FPartnerEditUIConnector;
                    UCSpecialTypes.HookupDataChange += new THookupPartnerEditDataChangeEventHandler(Uco_HookupPartnerEditDataChange);

                    UCSpecialTypes.SpecialInitUserControl();
                }
                else if (ATabPageEventArgs.Tab == tpgFamilyMembers)
                {
                    FCurrentlySelectedTabPage = TPartnerEditTabPageEnum.petpFamilyMembers;

                    // TODO
                }
                else if (ATabPageEventArgs.Tab == tpgNotes)
                {
                    FCurrentlySelectedTabPage = TPartnerEditTabPageEnum.petpNotes;

                    // TODO
                }
                else if (ATabPageEventArgs.Tab == tpgOfficeSpecific)
                {
                    FCurrentlySelectedTabPage = TPartnerEditTabPageEnum.petpOfficeSpecific;

                    // TODO
                }
            }
        }

        private void RecalculateTabHeaderCounters(System.Object sender, TRecalculateScreenPartsEventArgs e)
        {
            // MessageBox.Show('TUC_PartnerEdit_PartnerTabSet2.RecalculateTabHeaderCounters');
            if (e.ScreenPart == TScreenPartEnum.spCounters)
            {
                CalculateTabHeaderCounters(sender);
            }
        }

        private void CalculateTabHeaderCounters(System.Object ASender)
        {
            DataView TmpDV;
            string DynamicTabTitle;
            string DynamicToolTipPart1;
            Int32 CountAll;
            Int32 CountActive;

            if ((ASender is TUC_PartnerEdit_PartnerTabSet2) || (ASender is TUCPartnerAddresses))
            {
                if (FMainDS.Tables.Contains(PLocationTable.GetTableName()))
                {
                    Calculations.CalculateTabCountsAddresses(FMainDS.PPartnerLocation, out CountAll, out CountActive);
                }
                else
                {
                    CountAll = FMainDS.MiscellaneousData[0].ItemsCountAddresses;
                    CountActive = FMainDS.MiscellaneousData[0].ItemsCountAddressesActive;
                }

                if ((CountAll == 0) || (CountAll > 1))
                {
                    DynamicToolTipPart1 = StrAddressesTabHeader;
                }
                else
                {
                    DynamicToolTipPart1 = StrAddressesSingular;
                }

                if (CountActive == 0)
                {
                    tpgAddresses.Text = String.Format(StrAddressesTabHeader + " ({0}!)", CountActive);
                    tpgAddresses.ToolTipText = String.Format(StrTabHeaderCounterTipPlural + "current", CountAll, CountActive, DynamicToolTipPart1);
                }
                else
                {
                    tpgAddresses.Text = String.Format(StrAddressesTabHeader + " ({0})", CountActive);

                    if (CountActive > 1)
                    {
                        tpgAddresses.ToolTipText = String.Format(StrTabHeaderCounterTipPlural + "current", CountAll, CountActive, DynamicToolTipPart1);
                    }
                    else
                    {
                        tpgAddresses.ToolTipText = String.Format(StrTabHeaderCounterTipSingular + "current",
                            CountAll,
                            CountActive,
                            DynamicToolTipPart1);
                    }
                }
            }

            if ((ASender is TUC_PartnerEdit_PartnerTabSet2) || (ASender is TUCPartnerSubscriptions))
            {
                if (FMainDS.Tables.Contains(PSubscriptionTable.GetTableName()))
                {
                    Calculations.CalculateTabCountsSubscriptions(FMainDS.PSubscription, out CountAll, out CountActive);
                    tpgSubscriptions.Text = String.Format(StrSubscriptionsTabHeader + " ({0})", CountActive);
                }
                else
                {
                    CountAll = FMainDS.MiscellaneousData[0].ItemsCountSubscriptions;
                    CountActive = FMainDS.MiscellaneousData[0].ItemsCountSubscriptionsActive;
                }

                if ((CountAll == 0) || (CountAll > 1))
                {
                    DynamicToolTipPart1 = StrSubscriptionsTabHeader;
                }
                else
                {
                    DynamicToolTipPart1 = StrSubscriptionsSingular;
                }

                tpgSubscriptions.Text = String.Format(StrSubscriptionsTabHeader + " ({0})", CountActive);

                if ((CountActive == 0) || (CountActive > 1))
                {
                    tpgSubscriptions.ToolTipText = String.Format(StrTabHeaderCounterTipPlural + "active", CountAll, CountActive, DynamicToolTipPart1);
                }
                else
                {
                    tpgSubscriptions.ToolTipText = String.Format(StrTabHeaderCounterTipSingular + "active",
                        CountAll,
                        CountActive,
                        DynamicToolTipPart1);
                }
            }

            if ((ASender is TUC_PartnerEdit_PartnerTabSet2) || (ASender is TUCPartnerTypes))
            {
                if (FMainDS.Tables.Contains(PPartnerTypeTable.GetTableName()))
                {
                    TmpDV = new DataView(FMainDS.PPartnerType, "", "", DataViewRowState.CurrentRows);
                    tpgPartnerTypes.Text = StrSpecialTypesTabHeader + " (" + TmpDV.Count.ToString() + ')';
                }
                else
                {
                    tpgPartnerTypes.Text = StrSpecialTypesTabHeader + " (" + FMainDS.MiscellaneousData[0].ItemsCountPartnerTypes.ToString() + ')';
                }
            }

            if ((ASender is TUC_PartnerEdit_PartnerTabSet2) || (ASender is TUC_FamilyMembers))
            {
                // determine Tab Title
                if (FMainDS.PPartner[0].PartnerClass == SharedTypes.PartnerClassEnumToString(TPartnerClass.FAMILY))
                {
                    DynamicTabTitle = StrFamilyMembersTabHeader;
                }
                else
                {
                    DynamicTabTitle = StrFamilyTabHeader;
                }

                if (FMainDS.Tables.Contains(PartnerEditTDSFamilyMembersTable.GetTableName()))
                {
                    TmpDV = new DataView(FMainDS.FamilyMembers, "", "", DataViewRowState.CurrentRows);
                    tpgFamilyMembers.Text = DynamicTabTitle + " (" + TmpDV.Count.ToString() + ')';
                }
                else
                {
                    tpgFamilyMembers.Text = DynamicTabTitle + " (" + FMainDS.MiscellaneousData[0].ItemsCountFamilyMembers.ToString() + ')';
                }
            }

#if TODO
            if ((ASender is TUC_PartnerEdit_PartnerTabSet2) || (ASender is TUCPartnerInterests))
            {
                if (FMainDS.Tables.Contains(PPartnerInterestTable.GetTableName()))
                {
                    TmpDV = new DataView(FMainDS.PPartnerInterest, "", "", DataViewRowState.CurrentRows);
                    tpgInterests.Text = StrInterestsTabHeader + " (" + TmpDV.Count.ToString() + ')';
                }
                else
                {
                    tpgInterests.Text = StrInterestsTabHeader + " (" + FMainDS.MiscellaneousData[0].ItemsCountInterests.ToString() + ')';
                }
            }
#endif

            if ((ASender is TUC_PartnerEdit_PartnerTabSet2) || (ASender is TUC_PartnerNotes))
            {
                if ((FMainDS.PPartner[0].IsCommentNull()) || (FMainDS.PPartner[0].Comment == ""))
                {
                    tpgNotes.Text = String.Format(StrNotesTabHeader + " ({0})", 0);
                    tpgNotes.ToolTipText = "No Notes entered";
                }
                else
                {
                    // 8730 = 'square root symbol' in Verdana
                    tpgNotes.Text = String.Format(StrNotesTabHeader + " ({0})", (char)8730);
                    tpgNotes.ToolTipText = "Notes are entered";
                }
            }
        }

        private void Uco_HookupDataChange(System.Object sender, System.EventArgs e)
        {
            if (HookupDataChange != null)
            {
                HookupDataChange(this, e);
            }
        }

        private void Uco_HookupPartnerEditDataChange(System.Object sender, THookupPartnerEditDataChangeEventArgs e)
        {
            if (HookupPartnerEditDataChange != null)
            {
                HookupPartnerEditDataChange(this, e);
            }
        }

        private void UcoTab_EnableDisableOtherScreenParts(System.Object sender, TEnableDisableEventArgs e)
        {
            // MessageBox.Show('TUC_PartnerEdit_PartnerTabSet2.ucoTab_EnableDisableOtherScreenParts(' + e.Enable.ToString + ')');
            // Simply fire OnEnableDisableOtherScreenParts event again so that the PartnerEdit screen can catch it
            OnEnableDisableOtherScreenParts(e);

            // Disable all TabPages except the current one (and reverse that)
            tabPartners.EnableDisableAllOtherTabPages(e.Enable);
        }

        private void OnEnableDisableOtherScreenParts(TEnableDisableEventArgs e)
        {
            if (EnableDisableOtherScreenParts != null)
            {
                EnableDisableOtherScreenParts(this, e);
            }
        }

        private Boolean CheckSecurityOKToAccessNotesTab()
        {
            Boolean ReturnValue;

            ReturnValue = false;

            if ((FMainDS.MiscellaneousData[0].FoundationOwner1Key == 0) && (FMainDS.MiscellaneousData[0].FoundationOwner2Key == 0))
            {
                // MessageBox.Show('Notes Tab: None of the Owners is set.');
                if (UserInfo.GUserInfo.IsInModule(SharedConstants.PETRAMODULE_DEVADMIN))
                {
                    // MessageBox.Show('Notes Tab: User is member of DEVADMIN Module');
                    ReturnValue = true;
                }
            }
            else
            {
                // MessageBox.Show('Notes Tab: One of the Owners is set!');
                if (UserInfo.GUserInfo.IsInModule(SharedConstants.PETRAMODULE_DEVADMIN))
                {
                    // MessageBox.Show('Notes Tab: User is member of DEVADMIN Module');
                    ReturnValue = true;
                }
                else
                {
                    // MessageBox.Show('Notes Tab: User is NOT member of DEVADMIN Module');
                    if ((UserInfo.GUserInfo.PetraIdentity.PartnerKey == FMainDS.MiscellaneousData[0].FoundationOwner1Key)
                        || (UserInfo.GUserInfo.PetraIdentity.PartnerKey == FMainDS.MiscellaneousData[0].FoundationOwner2Key))
                    {
                        // MessageBox.Show('Notes Tab: User is Owner1 or Owner2');
                        ReturnValue = true;
                    }
                }
            }

            return ReturnValue;
        }

        /// <summary>
        /// todoComment
        /// </summary>
        /// <param name="ATabPage"></param>
        public void SelectTabPage(TPartnerEditTabPageEnum ATabPage)
        {
            TabPage SelectedTabPageBeforeReSelecting;

            if (!FUserControlInitialised)
            {
                throw new ApplicationException("SelectTabPage must not be called if the UserControl is not yet initialised");
            }

            OnDataLoadingStarted();

            // supress detection changing
            SelectedTabPageBeforeReSelecting = tabPartners.SelectedTab;

            switch (ATabPage)
            {
                case TPartnerEditTabPageEnum.petpAddresses:
                case TPartnerEditTabPageEnum.petpDefault:
                    tabPartners.SelectedTab = tpgAddresses;
                    break;

                case TPartnerEditTabPageEnum.petpDetails:
                    tabPartners.SelectedTab = tpgPartnerDetails;
                    break;

                case TPartnerEditTabPageEnum.petpFoundationDetails:
                    tabPartners.SelectedTab = tpgFoundationDetails;
                    break;

                case TPartnerEditTabPageEnum.petpSubscriptions:
                    tabPartners.SelectedTab = tpgSubscriptions;
                    break;

                case TPartnerEditTabPageEnum.petpPartnerTypes:
                    tabPartners.SelectedTab = tpgPartnerTypes;
                    break;

                case TPartnerEditTabPageEnum.petpFamilyMembers:
                    tabPartners.SelectedTab = tpgFamilyMembers;
                    break;

                case TPartnerEditTabPageEnum.petpOfficeSpecific:
                    tabPartners.SelectedTab = tpgOfficeSpecific;
                    break;

#if TODO
                case TPartnerEditTabPageEnum.petpInterests:
                    tabPartners.SelectedTab = tpgInterests;
                    break;

                case TPartnerEditTabPageEnum.petpReminders:
                    tabPartners.SelectedTab = tpgReminders;
                    break;

                case TPartnerEditTabPageEnum.petpRelationships:
                    tabPartners.SelectedTab = tpgRelationships;
                    break;

                case TPartnerEditTabPageEnum.petpContacts:
                    tabPartners.SelectedTab = tpgContacts;
                    break;
#endif
                case TPartnerEditTabPageEnum.petpNotes:

                    if (tpgNotes.Enabled)
                    {
                        tabPartners.SelectedTab = tpgNotes;
                    }
                    else
                    {
                        tabPartners.SelectedTab = tpgAddresses;
                    }

                    break;
            }

            // Check if the selected TabPage actually changed...
            if (SelectedTabPageBeforeReSelecting == tabPartners.SelectedTab)
            {
                // Tab was already selected; therefore raise the SelectedIndexChanged Event 'manually', which did not get raised by selecting the Tab again!
                TabSelectionChanged(this, new System.EventArgs());
            }

            OnDataLoadingFinished();
        }

        #endregion
    }
}