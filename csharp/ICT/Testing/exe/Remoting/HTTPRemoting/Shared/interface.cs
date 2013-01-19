//
// DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
//
// @Authors:
//       timop
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
using Ict.Common.Remoting.Shared;

namespace Tests.HTTPRemoting.Interface
{
    /// a simple service for testing purposes
    public interface IMyService : IInterface
    {
        /// print hello world
        string HelloWorld(string msg);

        /// some tests for remoting DateTime objects
        DateTime TestDateTime(DateTime date, out DateTime outDate);

        /// get a subnamespace
        IMySubNamespace SubNamespace
        {
            get;
        }
    }

    /// simple test of UIConnectors
    public interface IMyUIConnector : IInterface
    {
        /// test
        string HelloWorldUIConnector();
    }

    /// <summary>
    /// sub namespace
    /// </summary>
    public interface IMySubNamespace : IInterface
    {
        /// get the UIConnector
        IMyUIConnector MyUIConnector();

        /// print hello sub world
        string HelloSubWorld(string msg);
    }
}