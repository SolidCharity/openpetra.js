//
// DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
//
// @Authors:
//       christiank
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

namespace Ict.Petra.Shared
{
    /// <summary>
    /// constants that are used all over the place
    /// </summary>
    public class SharedConstants
    {
        /// <summary>Registry Keys for Petra</summary>
        public const String PETRA_REGISTRY_MAIN_KEY = "Software\\ICT\\Petra\\";

        /// <summary>Registry Keys for Petra</summary>
        public const String PETRA_REGISTRY_USERS_KEY = "Users";

        /// <summary>Registry Keys for Petra</summary>
        public const String PETRA_REGISTRY_POSITIONS_KEY = "Positions";

        /// <summary>OpenPetra Modules Conference Module</summary>
        public const String PETRAMODULE_CONFERENCE = "CONFERENCE";

        /// <summary>Financial Development User Level Access</summary>
        public const String PETRAMODULE_DEVUSER = "DEVUSER";

        /// <summary>Financial Development Administrative Access</summary>
        public const String PETRAMODULE_DEVADMIN = "DEVADMIN";

        /// <summary>Finance Exchange Rates</summary>
        public const String PETRAMODULE_FINEXRATE = "FIN-EX-RATE";

        /// <summary>Finance  Basic User</summary>
        public const String PETRAMODULE_FINANCE1 = "FINANCE-1";

        /// <summary>Finance  Intermediate User</summary>
        public const String PETRAMODULE_FINANCE2 = "FINANCE-2";

        /// <summary>Finance  Advanced User</summary>
        public const String PETRAMODULE_FINANCE3 = "FINANCE-3";

        /// <summary>Personnel Module</summary>
        public const String PETRAMODULE_PERSONNEL = "PERSONNEL";

        /// <summary>Partner Administrative Access</summary>
        public const String PETRAMODULE_PTNRADMIN = "PTNRADMIN";

        /// <summary>Partner User Level Access</summary>
        public const String PETRAMODULE_PTNRUSER = "PTNRUSER";

        /// <summary>System Manager</summary>
        public const String PETRAMODULE_SYSADMIN = "SYSMAN";

        /// <summary>OpenPetra Groups Conference User</summary>
        public const String PETRAGROUP_CONFUSER = "CONF-USER";

        /// <summary>Development User</summary>
        public const String PETRAGROUP_DEVUSER = "DEVUSER";

        /// <summary>Basic Finance</summary>
        public const String PETRAGROUP_FINANCE1 = "FINANCE-1";

        /// <summary>Intermediate Finance</summary>
        public const String PETRAGROUP_FINANCE2 = "FINANCE-2";

        /// <summary>Advanced Finance</summary>
        public const String PETRAGROUP_FINANCE3 = "FINANCE-3";

        /// <summary>OpenPetra Read Only Access</summary>
        public const String PETRAGROUP_GUEST = "GUEST";

        /// <summary>Personnel Administrator</summary>
        public const String PETRAGROUP_PERSADMIN = "PERS-ADMIN";

        /// <summary>Basic Personnel Access</summary>
        public const String PETRAGROUP_PERSUSER = "PERS-USER";

        /// <summary>Personnel View Mode</summary>
        public const String PETRAGROUP_PERSVIEW = "PERS-VIEW";

        /// <summary>Partner Administrator</summary>
        public const String PETRAGROUP_PTNRADMIN = "PTNRADMIN";

        /// <summary>Partner User</summary>
        public const String PETRAGROUP_PTNRUSER = "PTNRUSER";

        /// <summary>System Administrator</summary>
        public const String PETRAGROUP_SYSADMIN = "SYSADMIN";

        /// <summary>General OpenPetra Access</summary>
        public const String PETRAGROUP_USER = "USER";

        /// <summary>Key-Min used in Units</summary>
        public const String UNIT_TYPE_KEYMIN = "key-min";

        /// <summary>Address View/Edit for CANs</summary>
        public const String PETRAGROUP_ADDRESSCAN = "ADDRESSCAN";

        /// <summary>Remoting URL Identifiers</summary>
        public const String REMOTINGURL_IDENTIFIER_MSYSMAN = "MSysMan";

        /// <summary>Remoting URL Identifiers</summary>
        public const String REMOTINGURL_IDENTIFIER_MCOMMON = "MCommon";

        /// <summary>Remoting URL Identifiers</summary>
        public const String REMOTINGURL_IDENTIFIER_MCONFERENCE = "MConference";

        /// <summary>Remoting URL Identifiers</summary>
        public const String REMOTINGURL_IDENTIFIER_MPARTNER = "MPartner";

        /// <summary>Remoting URL Identifiers</summary>
        public const String REMOTINGURL_IDENTIFIER_MPERSONNEL = "MPersonnel";

        /// <summary>Remoting URL Identifiers</summary>
        public const String REMOTINGURL_IDENTIFIER_MFINANCE = "MFinance";

        /// <summary>Remoting URL Identifiers</summary>
        public const String REMOTINGURL_IDENTIFIER_MFINDEV = "MFinDev";

        /// <summary>Remoting URL Identifiers</summary>
        public const String REMOTINGURL_IDENTIFIER_MREPORTING = "MReporting";

        /// <summary>System Defaults</summary>
        public const String SYSDEFAULT_NOT_FOUND = "###SYSTEMDEFAULT NOT FOUND###";

        /// <summary>System Defaults</summary>
        public const String SYSDEFAULT_SITEKEY = "SiteKey";

        /// <summary>System Defaults</summary>
        public const String SYSDEFAULT_DISPLAYGIFTAMOUNT = "DisplayGiftAmount";

        /// <summary>System Defaults</summary>
        public const String SYSDEFAULT_DISPLAYGIFTRECIPIENT = "DisplayGiftRecipient";

        /// <summary>System Defaults</summary>
        public const String SYSDEFAULT_DISPLAYGIFTFIELD = "DisplayGiftField";

        /// <summary>System Defaults</summary>
        public const String SYSDEFAULT_LOCALISEDCOUNTYLABEL = "LocalisedCountyLabel";

        /// <summary>System Defaults</summary>
        public const String SYSDEFAULT_LOCALISEDBRANCHCODEANDLABEL = "Loc_BranchCdeLbl";

        /// <summary>System Defaults</summary>
        public const String SYSDEFAULT_GIFTBANKACCOUNT = "GiftBankAccount";

        /// <summary>System Defaults</summary>
        public const String SYSDEFAULT_PURGEEXTRACTS = "PurgeExtracts";

        /// <summary>System Defaults</summary>
        public const String SYSDEFAULT_GLREFMANDATORY = "GLRefMandatory";

        /// <summary>ClientTask TaskGroups</summary>
        public const String CLIENTTASKGROUP_USERMESSAGE = "USERMESSAGE";

        /// <summary>ClientTask TaskGroups</summary>
        public const String CLIENTTASKGROUP_CACHEREFRESH = "CACHEREFRESH";

        /// <summary>ClientTask TaskGroups</summary>
        public const String CLIENTTASKGROUP_SYSTEMDEFAULTSREFRESH = "SYSTEMDEFAULTSREFRESH";

        /// <summary>ClientTask TaskGroups</summary>
        public const String CLIENTTASKGROUP_USERDEFAULTSREFRESH = "USERDEFAULTSREFRESH";

        /// <summary>ClientTask TaskGroups</summary>
        public const String CLIENTTASKGROUP_USERINFOREFRESH = "USERINFOREFRESH";

        /// <summary>Securityrelated</summary>
        public const String SECURITY_CAN_LOCATIONTYPE = "-CAN";

        /// <summary>Threading</summary>
        public const Int32 THREADING_WAIT_INFINITE = -1;

        /// <summary>Fixed SiteKey (used for all Tables where the SiteKey is part of the PrimaryKey, but it is currently always 0)</summary>
        public const Int32 FIXED_SITE_KEY = 0;

        /// <summary>Values for p_partner.p_restricted_i</summary>
        public const Int32 PARTNER_PRIVATE_GROUP = 1;

        /// <summary>Values for p_partner.p_restricted_i</summary>
        public const Int32 PARTNER_PRIVATE_USER = 2;

        /// <summary>Value for Comment fields, etc. for System Generated Rows in Tables</summary>
        public const String ROW_IS_SYSTEM_GENERATED = "System Generated";
    }
}