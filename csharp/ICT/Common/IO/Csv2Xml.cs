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
using System.IO;
using System.Data;
using System.Xml;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using Ict.Common;
using Ict.Common.IO;
using GNU.Gettext;
using OfficeOpenXml;

namespace Ict.Common.IO
{
    /// provides methods for converting CSV file to and from XML;
    /// this helps with adding values, rearranging columns etc;
    /// expects a header line with column names
    public class TCsv2Xml
    {
        /// needed for writing CSV file header;
        /// also collects the node to avoid another recursion
        private static void GetAllAttributesAndNodes(XmlNode ANode, ref List <string>AAllAttributes, ref List <XmlNode>AAllNodes)
        {
            // don't use attributes from the root node
            if ((ANode.ParentNode != null) && (ANode.ParentNode.ParentNode != null))
            {
                AAllNodes.Add(ANode);

                if ((ANode.Attributes.GetNamedItem("name") != null) && !AAllAttributes.Contains("name"))
                {
                    AAllAttributes.Add("name");
                }

                if ((ANode.FirstChild != null) && (ANode.FirstChild.Name == ANode.Name) && !AAllAttributes.Contains("childOf"))
                {
                    AAllAttributes.Add("childOf");
                }

                foreach (XmlAttribute attr in ANode.Attributes)
                {
                    if (!AAllAttributes.Contains(attr.Name))
                    {
                        AAllAttributes.Add(attr.Name);
                    }
                }
            }

            foreach (XmlNode childNode in ANode.ChildNodes)
            {
                GetAllAttributesAndNodes(childNode, ref AAllAttributes, ref AAllNodes);
            }
        }

        /// <summary>
        /// format the XML into CSV so that it can be opened as a spreadsheet;
        /// this only works for quite simple files;
        /// hierarchical structures are flattened (using childOf column)
        /// </summary>
        public static bool Xml2Csv(XmlDocument ADoc, string AOutCSVFile)
        {
            StreamWriter sw = new StreamWriter(AOutCSVFile);

            sw.Write(Xml2CsvString(ADoc));

            sw.Close();

            return true;
        }

        /// <summary>
        /// format the XML into CSV so that it can be opened as a spreadsheet;
        /// this only works for quite simple files;
        /// hierarchical structures are flattened (using childOf column)
        /// </summary>
        public static string Xml2CsvString(XmlDocument ADoc)
        {
            // first write the header of the csv file
            List <string>AllAttributes = new List <string>();
            List <XmlNode>AllNodes = new List <XmlNode>();
            GetAllAttributesAndNodes(ADoc.DocumentElement, ref AllAttributes, ref AllNodes);

            string separator = TAppSettingsManager.GetValue("CSVSeparator",
                System.Globalization.CultureInfo.CurrentCulture.TextInfo.ListSeparator);

            string headerLine = "";

            foreach (string attrName in AllAttributes)
            {
                if (headerLine.Length > 0)
                {
                    headerLine += separator;
                }

                headerLine += "\"" + attrName + "\"";
            }

            string result = headerLine + Environment.NewLine;

            foreach (XmlNode node in AllNodes)
            {
                string line = "";

                foreach (string attrName in AllAttributes)
                {
                    if (attrName == "childOf")
                    {
                        line = StringHelper.AddCSV(line, TXMLParser.GetAttribute(node.ParentNode, "name"), separator);
                    }
                    else
                    {
                        line = StringHelper.AddCSV(line, TXMLParser.GetAttribute(node, attrName), separator);
                    }
                }

                result += line + Environment.NewLine;
            }

            return result;
        }

        /// <summary>
        /// store the data into Excel format, Open Office XML, .xlsx
        ///
        /// this makes use of the EPPlus library
        /// http://epplus.codeplex.com/
        /// </summary>
        private static void Xml2ExcelWorksheet(XmlDocument ADoc, ExcelWorksheet AWorksheet, bool AWithHashInCaption = true)
        {
            Int32 rowCounter = 1;
            Int16 colCounter = 1;

            // first write the header of the csv file
            List <string>AllAttributes = new List <string>();
            List <XmlNode>AllNodes = new List <XmlNode>();
            GetAllAttributesAndNodes(ADoc.DocumentElement, ref AllAttributes, ref AllNodes);

            foreach (string attrName in AllAttributes)
            {
                if (AWithHashInCaption)
                {
                    AWorksheet.Cells[rowCounter, colCounter].Value = "#" + attrName;
                }
                else
                {
                    AWorksheet.Cells[rowCounter, colCounter].Value = attrName;
                }

                colCounter++;
            }

            rowCounter++;
            colCounter = 1;

            foreach (XmlNode node in AllNodes)
            {
                foreach (string attrName in AllAttributes)
                {
                    if (attrName == "childOf")
                    {
                        AWorksheet.Cells[rowCounter, colCounter].Value = TXMLParser.GetAttribute(node.ParentNode, "name");
                    }
                    else
                    {
                        string value = TXMLParser.GetAttribute(node, attrName);

                        if (value.StartsWith(eVariantTypes.eDateTime.ToString() + ":"))
                        {
                            AWorksheet.Cells[rowCounter, colCounter].Value = TVariant.DecodeFromString(value).ToDate();
                            AWorksheet.Cells[rowCounter, colCounter].Style.Numberformat.Format = "dd/mm/yyyy";
                        }
                        else if (value.StartsWith(eVariantTypes.eInteger.ToString() + ":"))
                        {
                            AWorksheet.Cells[rowCounter, colCounter].Value = TVariant.DecodeFromString(value).ToInt64();
                        }
                        else
                        {
                            AWorksheet.Cells[rowCounter, colCounter].Value = value;
                        }
                    }

                    colCounter++;
                }

                rowCounter++;
                colCounter = 1;
            }
        }

        /// <summary>
        /// store the data into Excel format, Open Office XML, .xlsx
        ///
        /// this makes use of the EPPlus library
        /// http://epplus.codeplex.com/
        /// </summary>
        public static bool Xml2ExcelStream(XmlDocument ADoc, MemoryStream AStream, bool AWithHashInCaption = true)
        {
            try
            {
                ExcelPackage pck = new ExcelPackage();

                ExcelWorksheet worksheet = pck.Workbook.Worksheets.Add("Data Export");

                Xml2ExcelWorksheet(ADoc, worksheet, AWithHashInCaption);

                pck.SaveAs(AStream);

                return true;
            }
            catch (Exception e)
            {
                TLogging.Log(e.ToString());
                return false;
            }
        }

        /// <summary>
        /// store the data into Excel format, Open Office XML, .xlsx
        ///
        /// this makes use of the EPPlus library
        /// http://epplus.codeplex.com/
        ///
        /// this overload stores several worksheets
        /// </summary>
        public static bool Xml2ExcelStream(SortedList <string, XmlDocument>ADocs, MemoryStream AStream, bool AWithHashInCaption = true)
        {
            try
            {
                ExcelPackage pck = new ExcelPackage();

                foreach (string WorksheetTitle in ADocs.Keys)
                {
                    ExcelWorksheet worksheet = pck.Workbook.Worksheets.Add(WorksheetTitle);

                    Xml2ExcelWorksheet(ADocs[WorksheetTitle], worksheet, AWithHashInCaption);
                }

                pck.SaveAs(AStream);
                return true;
            }
            catch (Exception e)
            {
                TLogging.Log(e.ToString());
                return false;
            }
        }

        /// <summary>
        /// store the data into Excel format, Open Office XML, .xlsx
        ///
        /// this makes use of the EPPlus library
        /// http://epplus.codeplex.com/
        /// </summary>
        private static void DataTable2ExcelWorksheet(DataTable table, ExcelWorksheet AWorksheet, bool AWithHashInCaption = true)
        {
            Int32 rowCounter = 1;
            Int16 colCounter = 1;

            // first write the header of the csv file
            foreach (DataColumn col in table.Columns)
            {
                if (AWithHashInCaption)
                {
                    AWorksheet.Cells[rowCounter, colCounter].Value = "#" + col.ColumnName;
                }
                else
                {
                    AWorksheet.Cells[rowCounter, colCounter].Value = col.ColumnName;
                }

                colCounter++;
            }

            rowCounter++;
            colCounter = 1;

            foreach (DataRow row in table.Rows)
            {
                foreach (DataColumn col in table.Columns)
                {
                    if (row.IsNull(col) || (row[col] == null))
                    {
                        AWorksheet.Cells[rowCounter, colCounter].Value = "";
                        colCounter++;
                        continue;
                    }

                    object value = row[col];

                    if (value is DateTime)
                    {
                        AWorksheet.Cells[rowCounter, colCounter].Value = (DateTime)value;
                        AWorksheet.Cells[rowCounter, colCounter].Style.Numberformat.Format = "dd/mm/yyyy";
                    }
                    else if (value is Int32 || value is Int64 || value is Int16)
                    {
                        AWorksheet.Cells[rowCounter, colCounter].Value = Convert.ToInt64(value);
                    }
                    else
                    {
                        AWorksheet.Cells[rowCounter, colCounter].Value = value.ToString();
                    }

                    colCounter++;
                }

                rowCounter++;
                colCounter = 1;
            }
        }

        /// <summary>
        /// save a generic DataTable to an Excel file
        /// </summary>
        public static bool DataTable2ExcelStream(DataTable table, MemoryStream AStream, bool AWithHashInCaption = true)
        {
            try
            {
                ExcelPackage pck = new ExcelPackage();

                ExcelWorksheet worksheet = pck.Workbook.Worksheets.Add(table.TableName);

                DataTable2ExcelWorksheet(table, worksheet, AWithHashInCaption);

                pck.SaveAs(AStream);

                return true;
            }
            catch (Exception e)
            {
                TLogging.Log(e.ToString());
                return false;
            }
        }

        /// <summary>
        /// convert a CSV file to an XmlDocument.
        /// the first line is expected to contain the column names/captions
        /// the separator is read from the header line, and the captions require # at the start of each caption
        /// </summary>
        public static XmlDocument ParseCSV2Xml(string ACSVFilename)
        {
            return ParseCSV2Xml(ACSVFilename, String.Empty);
        }

        /// <summary>
        /// convert a CSV file to an XmlDocument.
        /// the first line is expected to contain the column names/captions, in quotes.
        /// from the header line, the separator can be determined, if the parameter ASeparator is empty
        /// </summary>
        public static XmlDocument ParseCSV2Xml(string ACSVFilename, string ASeparator, Encoding AEncoding = null)
        {
            XmlDocument myDoc = TYml2Xml.CreateXmlDocument();

            StreamReader sr = new StreamReader(ACSVFilename, TTextFile.GetFileEncoding(ACSVFilename, AEncoding), false);

            try
            {
                string headerLine = sr.ReadLine();
                string separator = ASeparator;

                if (ASeparator == string.Empty)
                {
                    if (!headerLine.StartsWith("\""))
                    {
                        throw new Exception(Catalog.GetString("Cannot open CSV file, because it is missing the header line.") +
                            Environment.NewLine +
                            Catalog.GetString("There must be a row with the column captions, at least the first caption must be in quotes."));
                    }
                    else
                    {
                        // read separator from header line. at least the first column needs to be quoted
                        separator = headerLine[StringHelper.FindMatchingQuote(headerLine, 0) + 2].ToString();
                    }
                }

                List <string>AllAttributes = new List <string>();

                while (headerLine.Length > 0)
                {
                    string attrName = StringHelper.GetNextCSV(ref headerLine, separator);

                    if (attrName.Length == 0)
                    {
                        TLogging.Log("Csv2Xml: found empty column header, will not consider any following columns");
                        break;
                    }

                    if (attrName.Length > 1)
                    {
                        attrName = attrName[0] + StringHelper.UpperCamelCase(attrName, ' ', false, false).Substring(1);
                    }

                    AllAttributes.Add(attrName);
                }

                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();

                    if (line.Trim().Length > 0)
                    {
                        SortedList <string, string>AttributePairs = new SortedList <string, string>();

                        foreach (string attrName in AllAttributes)
                        {
                            AttributePairs.Add(attrName, StringHelper.GetNextCSV(ref line, separator));
                        }

                        string rowName = "Element";

                        if (AttributePairs.ContainsKey("name"))
                        {
                            rowName = AttributePairs["name"];
                        }

                        XmlNode newNode = myDoc.CreateElement("", rowName, "");

                        if (AttributePairs.ContainsKey("childOf"))
                        {
                            XmlNode parentNode = TXMLParser.FindNodeRecursive(myDoc.DocumentElement, AttributePairs["childOf"]);

                            if (parentNode == null)
                            {
                                parentNode = myDoc.DocumentElement;
                            }

                            parentNode.AppendChild(newNode);
                        }
                        else
                        {
                            myDoc.DocumentElement.AppendChild(newNode);
                        }

                        foreach (string attrName in AllAttributes)
                        {
                            if ((attrName != "name") && (attrName != "childOf"))
                            {
                                XmlAttribute attr = myDoc.CreateAttribute(attrName);
                                attr.Value = AttributePairs[attrName];
                                newNode.Attributes.Append(attr);
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                sr.Close();
                throw;
            }

            return myDoc;
        }
    }
}