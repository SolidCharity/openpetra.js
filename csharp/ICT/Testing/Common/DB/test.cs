﻿/*************************************************************************
 *
 * DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
 *
 * @Authors:
 *       timop
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
using NUnit.Framework;
using System.Threading;
using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Xml;
using System.Data;
using Ict.Tools.CodeGeneration;
using Ict.Common;
using Ict.Common.DB;
using Npgsql;

namespace Ict.Common.DB.Testing
{
    ///  This is a testing program for Ict.Common.DB.dll
    [TestFixture]
    public class TTestCommonDB
    {
        private TAppSettingsManager settings;

        /// <summary>
        /// modified version taken from Ict.Petra.Server.App.Main::TServerManager
        /// </summary>
        private void EstablishDBConnection()
        {
            TLogging.Log("  Connecting to Database...");

            DBAccess.GDBAccessObj = new TDataBase();
            DBAccess.GDBAccessObj.DebugLevel = settings.GetInt16("Server.DebugLevel", 10);
            try
            {
                DBAccess.GDBAccessObj.EstablishDBConnection(CommonTypes.ParseDBType(settings.GetValue("Server.RDBMSType")),
                    settings.GetValue("Server.PostgreSQLServer"),
                    settings.GetValue("Server.PostgreSQLServerPort"),
                    settings.GetValue("Server.PostgreSQLDatabaseName"),
                    settings.GetValue("Server.PostgreSQLUserName"),
                    settings.GetValue("Server.Credentials"),
                    "");
            }
            catch (Exception)
            {
                throw;
            }

            TLogging.Log("  Connected to Database.");
        }

        [SetUp]
        public void Init()
        {
            new TLogging("test.log");
            settings = new TAppSettingsManager("Tests.Common.DB.dll.config");

            EstablishDBConnection();
        }

        [TearDown]
        public void TearDown()
        {
            DBAccess.GDBAccessObj.CloseDBConnection();
            TLogging.Log("  Database disconnected.");
        }

        /// <summary>
        /// this method will try to create a gift for a new gift batch before creating the gift batch.
        /// if the constraints would be checked only when committing the transaction, everything would be fine.
        /// but usually you get a violation of foreign key constraint a_gift_fk1
        /// </summary>
        private void WrongOrderSqlStatements()
        {
            TDBTransaction t;
            string sql;

            try
            {
                // setup test scenario: a gift batch, with 2 gifts, each with 2 gift details
                t = DBAccess.GDBAccessObj.BeginTransaction(IsolationLevel.Serializable);
                sql = "INSERT INTO a_gift(a_ledger_number_i, a_batch_number_i, a_gift_transaction_number_i) " +
                      "VALUES(43, 99999999, 1)";
                DBAccess.GDBAccessObj.ExecuteNonQuery(sql, t, false);
                sql =
                    "INSERT INTO a_gift_batch(a_ledger_number_i, a_batch_number_i, a_bank_account_code_c, a_batch_year_i, a_currency_code_c, a_bank_cost_centre_c) "
                    +
                    "VALUES(43, 99999999, '6000', 1, 'EUR', '4300')";
                DBAccess.GDBAccessObj.ExecuteNonQuery(sql, t, false);
                DBAccess.GDBAccessObj.CommitTransaction();
            }
            catch
            {
                DBAccess.GDBAccessObj.RollbackTransaction();
                throw;
            }

            // UNDO the test
            t = DBAccess.GDBAccessObj.BeginTransaction(IsolationLevel.Serializable);
            sql = "DELETE FROM a_gift" +
                  " WHERE a_ledger_number_i = 43 AND a_batch_number_i = 99999999 AND a_gift_transaction_number_i = 1";
            DBAccess.GDBAccessObj.ExecuteNonQuery(sql, t, false);
            sql = "DELETE FROM a_gift_batch" +
                  " WHERE a_ledger_number_i = 43 AND a_batch_number_i = 99999999";
            DBAccess.GDBAccessObj.ExecuteNonQuery(sql, t, false);
            DBAccess.GDBAccessObj.CommitTransaction();
        }

        [Test]
        public void TestOrderStatementsInTransaction()
        {
            // see http://nunit.net/blogs/?p=63, we expect an exception to be thrown
            // also http://nunit.org/index.php?p=exceptionAsserts&r=2.5
            Assert.Throws(Is.InstanceOf(typeof(Exception)), new TestDelegate(WrongOrderSqlStatements));
            Assert.Throws <Npgsql.NpgsqlException>(new TestDelegate(WrongOrderSqlStatements));
        }
    }
}