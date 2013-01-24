//
// DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
//
// @Authors:
//       timop
//
// Copyright 2004-2012 by OM International
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
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Windows.Forms;
using System.Drawing;
using System.Xml;
using Ict.Tools.CodeGeneration;
using Ict.Common.IO;
using Ict.Common;
using Ict.Tools.DBXML;

namespace Ict.Tools.CodeGeneration.Winforms
{
    /// <summary>
    /// Generator for label
    /// </summary>
    public class LabelGenerator : TControlGenerator
    {
        bool FRightAlign = false;

        /// <summary>
        /// constructor
        /// </summary>
        public LabelGenerator()
            : base("lbl", typeof(Label))
        {
            FAutoSize = true;
            FGenerateLabel = false;
            // when the top margin is 5 pixel, and the overall height should be 22, then 17 is the height for the label
            FDefaultHeight = 17;
        }

        /// <summary>
        /// should the label be right aligned
        /// </summary>
        public bool RightAlign
        {
            get
            {
                return FRightAlign;
            }

            set
            {
                FRightAlign = true;
            }
        }

        /// <summary>
        /// get the name for the label from the name of the associated control
        /// </summary>
        /// <param name="controlName"></param>
        /// <returns></returns>
        public string CalculateName(string controlName)
        {
            return "lbl" + controlName.Substring(3);
        }

        /// <summary>write the code for the designer file where the properties of the control are written</summary>
        public override ProcessTemplate SetControlProperties(TFormWriter writer, TControlDef ctrl)
        {
            // TODO this does not work yet. see EventRole Maintain screen
            if (!ctrl.HasAttribute("Align"))
            {
                ctrl.SetAttribute("Stretch", "horizontally");
            }

            base.SetControlProperties(writer, ctrl);

            // stretch at design time, but do not align to the right
            writer.SetControlProperty(ctrl, "Anchor",
                "((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)))");

            string labelText = "";

            if (TYml2Xml.HasAttribute(ctrl.xmlNode, "Text"))
            {
                labelText = TYml2Xml.GetAttribute(ctrl.xmlNode, "Text");
            }
            else
            {
                labelText = ctrl.Label + ":";
            }

            int width = PanelLayoutGenerator.MeasureTextWidth(labelText) + 5;

            if (Convert.ToInt32(ctrl.GetAttribute("Width", "1")) < width)
            {
                ctrl.SetAttribute("Width", width.ToString());
            }

            writer.SetControlProperty(ctrl, "Text", "\"" + labelText + "\"");
            writer.SetControlProperty(ctrl, "Padding", "new System.Windows.Forms.Padding(0, 5, 0, 0)");

            if (FRightAlign)
            {
                writer.SetControlProperty(ctrl, "TextAlign", "System.Drawing.ContentAlignment.TopRight");
            }

            return writer.FTemplate;
        }
    }

    /// <summary>
    /// Generator for label
    /// </summary>
    public class LinkLabelGenerator : TControlGenerator
    {
        /// <summary>
        /// constructor
        /// </summary>
        public LinkLabelGenerator()
            : base("llb", typeof(LinkLabel))
        {
            FAutoSize = true;
            FGenerateLabel = false;
        }

        /// <summary>
        /// get the name for the LinkLabel from the name of the associated control
        /// </summary>
        /// <param name="controlName"></param>
        /// <returns></returns>
        public string CalculateName(string controlName)
        {
            return "llb" + controlName.Substring(3);
        }

        /// <summary>write the code for the designer file where the properties of the control are written</summary>
        public override ProcessTemplate SetControlProperties(TFormWriter writer, TControlDef ctrl)
        {
            base.SetControlProperties(writer, ctrl);

            writer.SetControlProperty(ctrl, "Text", "\"" + ctrl.Label + "\"");
            writer.SetControlProperty(ctrl, "Margin", "new System.Windows.Forms.Padding(3, 7, 3, 0)");

            return writer.FTemplate;
        }
    }

    /// <summary>
    /// generator for buttons
    /// </summary>
    public class ButtonGenerator : TControlGenerator
    {
        /// <summary>constructor</summary>
        public ButtonGenerator()
            : base("btn", typeof(Button))
        {
            FAutoSize = true;
            FGenerateLabel = false;
            FDefaultWidth = 80;
        }

        /// <summary>write the code for the designer file where the properties of the control are written</summary>
        public override ProcessTemplate SetControlProperties(TFormWriter writer, TControlDef ctrl)
        {
            if (!ctrl.HasAttribute("Width"))
            {
                ctrl.SetAttribute("Width", (PanelLayoutGenerator.MeasureTextWidth(ctrl.Label) + 15).ToString());
            }

            if (!ctrl.HasAttribute("Height"))
            {
                ctrl.SetAttribute("Height", FDefaultHeight.ToString());
            }

            base.SetControlProperties(writer, ctrl);

            if (ctrl.GetAttribute("AcceptButton").ToLower() == "true")
            {
                writer.Template.AddToCodelet("INITUSERCONTROLS", "this.AcceptButton = " + ctrl.controlName + ";" + Environment.NewLine);
            }

            if (ctrl.HasAttribute("NoLabel") && (ctrl.GetAttribute("NoLabel").ToLower() == "true"))
            {
                writer.SetControlProperty(ctrl, "Text", "\"\"");
            }
            else
            {
                writer.SetControlProperty(ctrl, "Size", "new System.Drawing.Size(" +
                    ctrl.GetAttribute("Width").ToString() + ", " + ctrl.GetAttribute("Height").ToString() + ")");
                writer.SetControlProperty(ctrl, "Text", "\"" + ctrl.Label + "\"");
            }

            if (ctrl.GetAction() != null)
            {
                string img = ctrl.GetAction().actionImage;

                if (img.Length > 0)
                {
                    ctrl.SetAttribute("Width", (Convert.ToInt32(ctrl.GetAttribute("Width")) +
                                                Convert.ToInt32(ctrl.GetAttribute("IconWidth", "15"))).ToString());
                    writer.SetControlProperty(ctrl, "Size", "new System.Drawing.Size(" +
                        ctrl.GetAttribute("Width").ToString() + ", " + ctrl.GetAttribute("Height").ToString() + ")");

                    if (writer.GetControlProperty(ctrl.controlName, "Text") == "\"\"")
                    {
                        writer.SetControlProperty(ctrl, "ImageAlign", "System.Drawing.ContentAlignment.MiddleCenter");
                    }
                    else
                    {
                        writer.SetControlProperty(ctrl, "ImageAlign", "System.Drawing.ContentAlignment.MiddleLeft");
                    }

                    writer.SetControlProperty(ctrl, "TextAlign", "System.Drawing.ContentAlignment.MiddleRight");
                }
            }

            return writer.FTemplate;
        }
    }

    /// <summary>
    /// generator for a single radio button
    /// </summary>
    public class RadioButtonGenerator : TControlWithDependantControlsGenerator
    {
        /// <summary>constructor</summary>
        public RadioButtonGenerator()
            : base("rbt", typeof(RadioButton))
        {
            FAutoSize = true;
            FGenerateLabel = false;
            FDefaultHeight = 25;
            FChangeEventName = "CheckedChanged";
        }

        /// <summary>write the code for the designer file where the properties of the control are written</summary>
        public override ProcessTemplate SetControlProperties(TFormWriter writer, TControlDef ctrl)
        {
            // Support NoLabel=true
            FGenerateLabel = true;

            if (GenerateLabel(ctrl))
            {
                writer.SetControlProperty(ctrl, "Text", "\"" + ctrl.Label + "\"");
                ctrl.SetAttribute("Width", (PanelLayoutGenerator.MeasureTextWidth(ctrl.Label) + 30).ToString());
            }
            else
            {
                ctrl.SetAttribute("Width", "15");
            }

            base.SetControlProperties(writer, ctrl);

            FGenerateLabel = false;

            if (TYml2Xml.HasAttribute(ctrl.xmlNode, "RadioChecked"))
            {
                writer.SetControlProperty(ctrl,
                    "Checked",
                    TYml2Xml.GetAttribute(ctrl.xmlNode, "RadioChecked"));
            }

            return writer.FTemplate;
        }

        /// <summary>
        /// how to assign a value to the control
        /// </summary>
        protected override string AssignValue(TControlDef ctrl, string AFieldOrNull, string AFieldTypeDotNet)
        {
            if (AFieldOrNull == null)
            {
                return ctrl.controlName + ".Checked = false;";
            }

            return ctrl.controlName + ".Checked = " + AFieldOrNull + ";";
        }

        /// <summary>
        /// how to undo the change of a value of a control
        /// </summary>
        protected override string UndoValue(TControlDef ctrl, string AFieldOrNull, string AFieldTypeDotNet)
        {
            return ctrl.controlName + ".Checked = (bool)" + AFieldOrNull + ";";
        }

        /// <summary>
        /// how to get the value from the control
        /// </summary>
        protected override string GetControlValue(TControlDef ctrl, string AFieldTypeDotNet)
        {
            if (AFieldTypeDotNet == null)
            {
                return null;
            }

            return ctrl.controlName + ".Checked";
        }
    }

    /// <summary>
    /// generator for a tree view
    /// </summary>
    public class TreeViewGenerator : TControlGenerator
    {
        /// <summary>constructor</summary>
        public TreeViewGenerator()
            : base("trv", "Ict.Common.Controls.TTrvTreeView")
        {
        }
    }

    /// <summary>
    /// generator for a checkbox
    /// </summary>
    public class CheckBoxGenerator : TControlWithDependantControlsGenerator
    {
        /// <summary>constructor</summary>
        public CheckBoxGenerator()
            : base("chk", typeof(CheckBox))
        {
            base.FGenerateLabel = true;
            this.FChangeEventName = "CheckedChanged";
            FDefaultHeight = 22;
        }

        /// <summary>
        /// make sure there is a label or not
        /// </summary>
        public override bool GenerateLabel(TControlDef ctrl)
        {
            if (ctrl.HasAttribute("CheckBoxAttachedLabel")
                && ((ctrl.GetAttribute("CheckBoxAttachedLabel").ToLower() == "left")
                    || (ctrl.GetAttribute("CheckBoxAttachedLabel").ToLower() == "right")))
            {
                ctrl.hasLabel = false;
                return false;
            }

            return base.GenerateLabel(ctrl);
        }

        /// <summary>write the code for the designer file where the properties of the control are written</summary>
        public override ProcessTemplate SetControlProperties(TFormWriter writer, TControlDef ctrl)
        {
            if ((ctrl.HasAttribute("CheckBoxAttachedLabel"))
                && ((ctrl.GetAttribute("CheckBoxAttachedLabel").ToLower() == "left")
                    || (ctrl.GetAttribute("CheckBoxAttachedLabel").ToLower() == "right")))
            {
                base.FAutoSize = true;

                if (ctrl.HasAttribute("NoLabel") && (ctrl.GetAttribute("NoLabel").ToLower() == "true"))
                {
                    writer.SetControlProperty(ctrl, "Text", "\"\"");
                }
                else
                {
                    writer.SetControlProperty(ctrl, "Text", "\"" + ctrl.Label + "\"");

                    if (ctrl.GetAttribute("CheckBoxAttachedLabel").ToLower() == "left")
                    {
                        writer.SetControlProperty(ctrl, "CheckAlign", "System.Drawing.ContentAlignment.MiddleRight");
                    }
                    else
                    {
                        writer.SetControlProperty(ctrl, "CheckAlign", "System.Drawing.ContentAlignment.MiddleLeft");
                    }

                    writer.SetControlProperty(ctrl, "Margin", "new System.Windows.Forms.Padding(3, 4, 3, 0)");

                    ctrl.SetAttribute("Width", (PanelLayoutGenerator.MeasureTextWidth(ctrl.Label) + 30).ToString());
                    ctrl.SetAttribute("Height", "17");
                }

                base.SetControlProperties(writer, ctrl);
            }
            else
            {
                base.FAutoSize = false;
                ctrl.SetAttribute("Width", 30.ToString ());

                base.SetControlProperties(writer, ctrl);

                writer.SetControlProperty(ctrl, "Text", "\"\"");
                writer.SetControlProperty(ctrl, "Margin", "new System.Windows.Forms.Padding(3, 0, 3, 0)");
            }

            return writer.FTemplate;
        }

        /// <summary>
        /// how to assign a value to the control
        /// </summary>
        protected override string AssignValue(TControlDef ctrl, string AFieldOrNull, string AFieldTypeDotNet)
        {
            if (AFieldOrNull == null)
            {
                return ctrl.controlName + ".Checked = false;";
            }

            return ctrl.controlName + ".Checked = " + AFieldOrNull + ";";
        }

        /// <summary>
        /// how to undo the change of a value of a control
        /// </summary>
        protected override string UndoValue(TControlDef ctrl, string AFieldOrNull, string AFieldTypeDotNet)
        {
            return ctrl.controlName + ".Checked = (bool)" + AFieldOrNull + ";";
        }

        /// <summary>
        /// how to get the value from the control
        /// </summary>
        protected override string GetControlValue(TControlDef ctrl, string AFieldTypeDotNet)
        {
            if (AFieldTypeDotNet == null)
            {
                return null;
            }

            return ctrl.controlName + ".Checked";
        }
    }

    /// <summary>
    /// generator for a versatile checked list box
    /// </summary>
    public class TClbVersatileGenerator : TControlGenerator
    {
        /// <summary>constructor</summary>
        public TClbVersatileGenerator()
            : base("clb", "Ict.Common.Controls.TClbVersatile")
        {
            FDefaultHeight = 100;
        }

        /// <summary>write the code for the designer file where the properties of the control are written</summary>
        public override ProcessTemplate SetControlProperties(TFormWriter writer, TControlDef ctrl)
        {
            base.SetControlProperties(writer, ctrl);
            writer.SetControlProperty(ctrl, "FixedRows", "1");
            return writer.FTemplate;
        }
    }

    /// <summary>
    /// generator for a print preview control
    /// </summary>
    public class PrintPreviewGenerator : TControlGenerator
    {
        /// <summary>constructor</summary>
        public PrintPreviewGenerator()
            : base("ppv", typeof(PrintPreviewControl))
        {
            FGenerateLabel = false;
        }
    }

    /// <summary>
    /// generator for a control for defining integer values
    /// </summary>
    public class NumericUpDownGenerator : TControlGenerator
    {
        /// <summary>constructor</summary>
        public NumericUpDownGenerator()
            : base("nud", typeof(NumericUpDown))
        {
        }

        /// <summary>
        /// how to assign a value to the control
        /// </summary>
        protected override string AssignValue(TControlDef ctrl, string AFieldOrNull, string AFieldTypeDotNet)
        {
            if (AFieldOrNull == null)
            {
                return ctrl.controlName + ".Value = 0;";
            }

            return ctrl.controlName + ".Value = " + AFieldOrNull + ";";
        }

        /// <summary>
        /// how to undo the change of a value of a control
        /// </summary>
        protected override string UndoValue(TControlDef ctrl, string AFieldOrNull, string AFieldTypeDotNet)
        {
            return ctrl.controlName + ".Value = (decimal)" + AFieldOrNull + ";";
        }

        /// <summary>
        /// how to get the value from the control
        /// </summary>
        protected override string GetControlValue(TControlDef ctrl, string AFieldTypeDotNet)
        {
            if (AFieldTypeDotNet == null)
            {
                // this control cannot have a null value
                return null;
            }

            return "(" + AFieldTypeDotNet + ")" + ctrl.controlName + ".Value";
        }

        /// <summary>write the code for the designer file where the properties of the control are written</summary>
        public override ProcessTemplate SetControlProperties(TFormWriter writer, TControlDef ctrl)
        {
            base.SetControlProperties(writer, ctrl);

            if (TYml2Xml.HasAttribute(ctrl.xmlNode, "PositiveValueActivates"))
            {
                if (ctrl.HasAttribute("OnChange"))
                {
                    throw new Exception(ctrl.controlName + " cannot have OnChange and PositiveValueActivates at the same time");
                }

                AssignEventHandlerToControl(writer, ctrl, "ValueChanged", ctrl.controlName + "ValueChanged");
                writer.CodeStorage.FEventHandlersImplementation +=
                    "private void " + ctrl.controlName + "ValueChanged" +
                    "(object sender, EventArgs e)" + Environment.NewLine +
                    "{" + Environment.NewLine +
                    "    ActionEnabledEvent(null, new ActionEventArgs(\"" + TYml2Xml.GetAttribute(ctrl.xmlNode, "PositiveValueActivates") +
                    "\", " + ctrl.controlName + ".Value > 0));" + Environment.NewLine +
                    "}" + Environment.NewLine + Environment.NewLine;
            }

            return writer.FTemplate;
        }
    }

    /// <summary>
    /// generator for embedding an user control
    /// </summary>
    public class UserControlGenerator : TControlGenerator
    {
        /// <summary>constructor</summary>
        public UserControlGenerator()
            : base("uco", typeof(System.Windows.Forms.Control))
        {
            FGenerateLabel = false;
            FAutoSize = true;
        }

        /// <summary>write the code for the designer file where the properties of the control are written</summary>
        public override ProcessTemplate SetControlProperties(TFormWriter writer, TControlDef ctrl)
        {
            string controlName = ctrl.controlName;

            FDefaultWidth = 650;
            FDefaultHeight = 386;
            base.SetControlProperties(writer, ctrl);

            // todo: use properties from yaml

            writer.Template.AddToCodelet("INITUSERCONTROLS", controlName + ".PetraUtilsObject = FPetraUtilsObject;" +
                Environment.NewLine);

            if (writer.CodeStorage.HasAttribute("DatasetType"))
            {
                writer.Template.AddToCodelet("INITUSERCONTROLS", controlName + ".MainDS = FMainDS;" + Environment.NewLine);
            }

            writer.Template.AddToCodelet("INITUSERCONTROLS", controlName + ".InitUserControl();" + Environment.NewLine);

            return writer.FTemplate;
        }
    }
}