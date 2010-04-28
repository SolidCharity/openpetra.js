/* auto generated with nant generateWinforms from UC_PartnerEdit_TopPart.yaml
 *
 * DO NOT edit manually, DO NOT edit with the designer
 * use a user control if you need to modify the screen content
 *
 */
/*************************************************************************
 *
 * DO NOT REMOVE COPYRIGHT NOTICES OR THIS FILE HEADER.
 *
 * @Authors:
 *       auto generated
 *
 * Copyright 2004-2009 by OM International
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
using System.Windows.Forms;
using Mono.Unix;
using Ict.Common.Controls;
using Ict.Petra.Client.CommonControls;

namespace Ict.Petra.Client.MPartner.Gui
{
    partial class TUC_PartnerEdit_TopPart
    {
        /// <summary>
        /// Designer variable used to keep track of non-visual components.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Disposes resources used by the form.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        /// <summary>
        /// This method is required for Windows Forms designer support.
        /// Do not change the method contents inside the source code editor. The Forms designer might
        /// not be able to load this method if it was changed manually.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TUC_PartnerEdit_TopPart));

            this.pnlContent = new System.Windows.Forms.Panel();
            this.grpCollapsible = new System.Windows.Forms.GroupBox();
            this.pnlLeft = new System.Windows.Forms.Panel();
            this.pnlPartnerInfo = new System.Windows.Forms.Panel();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.txtPartnerKey = new Ict.Petra.Client.CommonControls.TtxtAutoPopulatedButtonLabel();
            this.lblPartnerKey = new System.Windows.Forms.Label();
            this.lblEmpty2 = new System.Windows.Forms.Label();
            this.txtPartnerClass = new System.Windows.Forms.TextBox();
            this.lblPartnerClass = new System.Windows.Forms.Label();
            this.pnlFamily = new System.Windows.Forms.Panel();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.txtFamilyTitle = new System.Windows.Forms.TextBox();
            this.lblFamilyTitle = new System.Windows.Forms.Label();
            this.txtFirstName = new System.Windows.Forms.TextBox();
            this.txtFamilyName = new System.Windows.Forms.TextBox();
            this.lblEmpty = new System.Windows.Forms.Label();
            this.cmbAddresseeTypeCode = new Ict.Petra.Client.CommonControls.TCmbAutoPopulated();
            this.lblAddresseeTypeCode = new System.Windows.Forms.Label();
            this.chkNoSolicitations = new System.Windows.Forms.CheckBox();
            this.pnlAdditionalInfo = new System.Windows.Forms.Panel();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.txtLastGift = new System.Windows.Forms.TextBox();
            this.lblLastGift = new System.Windows.Forms.Label();
            this.pnlRight = new System.Windows.Forms.Panel();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.btnWorkerField = new System.Windows.Forms.Button();
            this.txtWorkerField = new System.Windows.Forms.TextBox();
            this.cmbPartnerStatus = new Ict.Petra.Client.CommonControls.TCmbAutoPopulated();
            this.lblPartnerStatus = new System.Windows.Forms.Label();
            this.txtStatusUpdated = new System.Windows.Forms.TextBox();
            this.lblStatusUpdated = new System.Windows.Forms.Label();
            this.txtLastContact = new System.Windows.Forms.TextBox();
            this.lblLastContact = new System.Windows.Forms.Label();

            this.pnlContent.SuspendLayout();
            this.grpCollapsible.SuspendLayout();
            this.pnlLeft.SuspendLayout();
            this.pnlPartnerInfo.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.pnlFamily.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.pnlAdditionalInfo.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.pnlRight.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();

            //
            // pnlContent
            //
            this.pnlContent.Name = "pnlContent";
            this.pnlContent.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlContent.AutoSize = true;
            this.pnlContent.Controls.Add(this.grpCollapsible);
            //
            // grpCollapsible
            //
            this.grpCollapsible.Name = "grpCollapsible";
            this.grpCollapsible.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpCollapsible.AutoSize = true;
            this.grpCollapsible.Controls.Add(this.pnlLeft);
            this.grpCollapsible.Controls.Add(this.pnlRight);
            //
            // pnlLeft
            //
            this.pnlLeft.Name = "pnlLeft";
            this.pnlLeft.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlLeft.AutoSize = true;
            this.pnlLeft.Controls.Add(this.pnlAdditionalInfo);
            this.pnlLeft.Controls.Add(this.pnlFamily);
            this.pnlLeft.Controls.Add(this.pnlPartnerInfo);
            //
            // pnlPartnerInfo
            //
            this.pnlPartnerInfo.Name = "pnlPartnerInfo";
            this.pnlPartnerInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlPartnerInfo.AutoSize = true;
            //
            // tableLayoutPanel1
            //
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.AutoSize = true;
            this.pnlPartnerInfo.Controls.Add(this.tableLayoutPanel1);
            //
            // txtPartnerKey
            //
            this.txtPartnerKey.Location = new System.Drawing.Point(2,2);
            this.txtPartnerKey.Name = "txtPartnerKey";
            this.txtPartnerKey.TabStop = false;
            this.txtPartnerKey.Size = new System.Drawing.Size(80, 28);
            this.txtPartnerKey.ReadOnly = true;
            this.txtPartnerKey.TabStop = false;
            this.txtPartnerKey.ShowLabel = false;
            this.txtPartnerKey.ASpecialSetting = true;
            this.txtPartnerKey.ButtonTextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.txtPartnerKey.ListTable = TtxtAutoPopulatedButtonLabel.TListTableEnum.PartnerKey;
            this.txtPartnerKey.PartnerClass = "";
            this.txtPartnerKey.MaxLength = 32767;
            this.txtPartnerKey.Tag = "CustomDisableAlthoughInvisible";
            this.txtPartnerKey.TextBoxWidth = 80;
            this.txtPartnerKey.ButtonWidth = 0;
            this.txtPartnerKey.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtPartnerKey.Padding = new System.Windows.Forms.Padding(0, 2, 0, 0);
            //
            // lblPartnerKey
            //
            this.lblPartnerKey.Location = new System.Drawing.Point(2,2);
            this.lblPartnerKey.Name = "lblPartnerKey";
            this.lblPartnerKey.AutoSize = true;
            this.lblPartnerKey.Text = "Key:";
            this.lblPartnerKey.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
            this.lblPartnerKey.Dock = System.Windows.Forms.DockStyle.Right;
            //
            // lblEmpty2
            //
            this.lblEmpty2.Location = new System.Drawing.Point(2,2);
            this.lblEmpty2.Name = "lblEmpty2";
            this.lblEmpty2.AutoSize = true;
            this.lblEmpty2.Text = "Empty2:";
            this.lblEmpty2.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
            //
            // txtPartnerClass
            //
            this.txtPartnerClass.Location = new System.Drawing.Point(2,2);
            this.txtPartnerClass.Name = "txtPartnerClass";
            this.txtPartnerClass.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtPartnerClass.Margin = new System.Windows.Forms.Padding(0, 7, 0, 0);
            this.txtPartnerClass.Size = new System.Drawing.Size(150, 28);
            this.txtPartnerClass.ReadOnly = true;
            this.txtPartnerClass.TabStop = false;
            //
            // lblPartnerClass
            //
            this.lblPartnerClass.Location = new System.Drawing.Point(2,2);
            this.lblPartnerClass.Name = "lblPartnerClass";
            this.lblPartnerClass.AutoSize = true;
            this.lblPartnerClass.Text = "Class:";
            this.lblPartnerClass.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
            this.lblPartnerClass.Dock = System.Windows.Forms.DockStyle.Right;
            this.tableLayoutPanel1.ColumnCount = 5;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Controls.Add(this.lblPartnerKey, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.txtPartnerKey, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblEmpty2, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblPartnerClass, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.txtPartnerClass, 4, 0);
            //
            // pnlFamily
            //
            this.pnlFamily.Name = "pnlFamily";
            this.pnlFamily.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlFamily.AutoSize = true;
            //
            // tableLayoutPanel2
            //
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.AutoSize = true;
            this.pnlFamily.Controls.Add(this.tableLayoutPanel2);
            //
            // txtFamilyTitle
            //
            this.txtFamilyTitle.Location = new System.Drawing.Point(2,2);
            this.txtFamilyTitle.Name = "txtFamilyTitle";
            this.txtFamilyTitle.Size = new System.Drawing.Size(90, 28);
            //
            // lblFamilyTitle
            //
            this.lblFamilyTitle.Location = new System.Drawing.Point(2,2);
            this.lblFamilyTitle.Name = "lblFamilyTitle";
            this.lblFamilyTitle.AutoSize = true;
            this.lblFamilyTitle.Text = "Title/Na&me:";
            this.lblFamilyTitle.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
            this.lblFamilyTitle.Dock = System.Windows.Forms.DockStyle.Right;
            //
            // txtFirstName
            //
            this.txtFirstName.Location = new System.Drawing.Point(2,2);
            this.txtFirstName.Name = "txtFirstName";
            this.txtFirstName.Size = new System.Drawing.Size(150, 28);
            //
            // txtFamilyName
            //
            this.txtFamilyName.Location = new System.Drawing.Point(2,2);
            this.txtFamilyName.Name = "txtFamilyName";
            this.txtFamilyName.Size = new System.Drawing.Size(150, 28);
            //
            // lblEmpty
            //
            this.lblEmpty.Location = new System.Drawing.Point(2,2);
            this.lblEmpty.Name = "lblEmpty";
            this.lblEmpty.Size = new System.Drawing.Size(90, 28);
            this.lblEmpty.Text = "Empty:";
            this.lblEmpty.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
            //
            // cmbAddresseeTypeCode
            //
            this.cmbAddresseeTypeCode.Location = new System.Drawing.Point(2,2);
            this.cmbAddresseeTypeCode.Name = "cmbAddresseeTypeCode";
            this.cmbAddresseeTypeCode.Size = new System.Drawing.Size(105, 28);
            this.cmbAddresseeTypeCode.ListTable = TCmbAutoPopulated.TListTableEnum.AddresseeTypeList;
            //
            // lblAddresseeTypeCode
            //
            this.lblAddresseeTypeCode.Location = new System.Drawing.Point(2,2);
            this.lblAddresseeTypeCode.Name = "lblAddresseeTypeCode";
            this.lblAddresseeTypeCode.AutoSize = true;
            this.lblAddresseeTypeCode.Text = "&Addressee Type:";
            this.lblAddresseeTypeCode.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
            this.lblAddresseeTypeCode.Dock = System.Windows.Forms.DockStyle.Right;
            //
            // chkNoSolicitations
            //
            this.chkNoSolicitations.Location = new System.Drawing.Point(2,2);
            this.chkNoSolicitations.Name = "chkNoSolicitations";
            this.chkNoSolicitations.AutoSize = true;
            this.chkNoSolicitations.Text = "No Solicitations";
            this.chkNoSolicitations.Margin = new System.Windows.Forms.Padding(3, 5, 3, 0);
            this.tableLayoutPanel2.ColumnCount = 5;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.RowCount = 2;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.Controls.Add(this.lblFamilyTitle, 0, 0);
            this.tableLayoutPanel2.SetColumnSpan(this.lblEmpty, 2);
            this.tableLayoutPanel2.Controls.Add(this.lblEmpty, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.txtFamilyTitle, 1, 0);
            this.tableLayoutPanel2.SetColumnSpan(this.txtFirstName, 2);
            this.tableLayoutPanel2.Controls.Add(this.txtFirstName, 2, 0);
            this.tableLayoutPanel2.Controls.Add(this.lblAddresseeTypeCode, 2, 1);
            this.tableLayoutPanel2.Controls.Add(this.cmbAddresseeTypeCode, 3, 1);
            this.tableLayoutPanel2.Controls.Add(this.txtFamilyName, 4, 0);
            this.tableLayoutPanel2.Controls.Add(this.chkNoSolicitations, 4, 1);
            //
            // pnlAdditionalInfo
            //
            this.pnlAdditionalInfo.Name = "pnlAdditionalInfo";
            this.pnlAdditionalInfo.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlAdditionalInfo.AutoSize = true;
            //
            // tableLayoutPanel3
            //
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel3.AutoSize = true;
            this.pnlAdditionalInfo.Controls.Add(this.tableLayoutPanel3);
            //
            // txtLastGift
            //
            this.txtLastGift.Location = new System.Drawing.Point(2,2);
            this.txtLastGift.Name = "txtLastGift";
            this.txtLastGift.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtLastGift.Margin = new System.Windows.Forms.Padding(0, 7, 0, 0);
            this.txtLastGift.Size = new System.Drawing.Size(420, 28);
            this.txtLastGift.ReadOnly = true;
            this.txtLastGift.TabStop = false;
            //
            // lblLastGift
            //
            this.lblLastGift.Location = new System.Drawing.Point(2,2);
            this.lblLastGift.Name = "lblLastGift";
            this.lblLastGift.AutoSize = true;
            this.lblLastGift.Text = "Last Gift:";
            this.lblLastGift.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
            this.lblLastGift.Dock = System.Windows.Forms.DockStyle.Right;
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel3.Controls.Add(this.lblLastGift, 0, 0);
            this.tableLayoutPanel3.Controls.Add(this.txtLastGift, 1, 0);
            //
            // pnlRight
            //
            this.pnlRight.Name = "pnlRight";
            this.pnlRight.Dock = System.Windows.Forms.DockStyle.Right;
            this.pnlRight.AutoSize = true;
            //
            // tableLayoutPanel4
            //
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel4.AutoSize = true;
            this.pnlRight.Controls.Add(this.tableLayoutPanel4);
            //
            // btnWorkerField
            //
            this.btnWorkerField.Location = new System.Drawing.Point(2,2);
            this.btnWorkerField.Name = "btnWorkerField";
            this.btnWorkerField.AutoSize = true;
            this.btnWorkerField.Click += new System.EventHandler(this.MaintainWorkerField);
            this.btnWorkerField.Image = ((System.Drawing.Bitmap)resources.GetObject("btnWorkerField.Glyph"));
            this.btnWorkerField.Text = "&Worker Field...";
            //
            // txtWorkerField
            //
            this.txtWorkerField.Location = new System.Drawing.Point(2,2);
            this.txtWorkerField.Name = "txtWorkerField";
            this.txtWorkerField.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtWorkerField.Margin = new System.Windows.Forms.Padding(0, 7, 0, 0);
            this.txtWorkerField.Size = new System.Drawing.Size(115, 28);
            this.txtWorkerField.ReadOnly = true;
            this.txtWorkerField.TabStop = false;
            //
            // cmbPartnerStatus
            //
            this.cmbPartnerStatus.Location = new System.Drawing.Point(2,2);
            this.cmbPartnerStatus.Name = "cmbPartnerStatus";
            this.cmbPartnerStatus.Size = new System.Drawing.Size(100, 28);
            this.cmbPartnerStatus.ListTable = TCmbAutoPopulated.TListTableEnum.PartnerStatusList;
            //
            // lblPartnerStatus
            //
            this.lblPartnerStatus.Location = new System.Drawing.Point(2,2);
            this.lblPartnerStatus.Name = "lblPartnerStatus";
            this.lblPartnerStatus.AutoSize = true;
            this.lblPartnerStatus.Text = "Partner &Status:";
            this.lblPartnerStatus.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
            this.lblPartnerStatus.Dock = System.Windows.Forms.DockStyle.Right;
            //
            // txtStatusUpdated
            //
            this.txtStatusUpdated.Location = new System.Drawing.Point(2,2);
            this.txtStatusUpdated.Name = "txtStatusUpdated";
            this.txtStatusUpdated.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtStatusUpdated.Margin = new System.Windows.Forms.Padding(0, 7, 0, 0);
            this.txtStatusUpdated.Size = new System.Drawing.Size(115, 28);
            this.txtStatusUpdated.ReadOnly = true;
            this.txtStatusUpdated.TabStop = false;
            //
            // lblStatusUpdated
            //
            this.lblStatusUpdated.Location = new System.Drawing.Point(2,2);
            this.lblStatusUpdated.Name = "lblStatusUpdated";
            this.lblStatusUpdated.AutoSize = true;
            this.lblStatusUpdated.Text = "Status Updated:";
            this.lblStatusUpdated.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
            this.lblStatusUpdated.Dock = System.Windows.Forms.DockStyle.Right;
            //
            // txtLastContact
            //
            this.txtLastContact.Location = new System.Drawing.Point(2,2);
            this.txtLastContact.Name = "txtLastContact";
            this.txtLastContact.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtLastContact.Margin = new System.Windows.Forms.Padding(0, 7, 0, 0);
            this.txtLastContact.Size = new System.Drawing.Size(115, 28);
            this.txtLastContact.ReadOnly = true;
            this.txtLastContact.TabStop = false;
            //
            // lblLastContact
            //
            this.lblLastContact.Location = new System.Drawing.Point(2,2);
            this.lblLastContact.Name = "lblLastContact";
            this.lblLastContact.AutoSize = true;
            this.lblLastContact.Text = "Last Contact:";
            this.lblLastContact.Margin = new System.Windows.Forms.Padding(3, 7, 3, 0);
            this.lblLastContact.Dock = System.Windows.Forms.DockStyle.Right;
            this.tableLayoutPanel4.ColumnCount = 3;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel4.RowCount = 4;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel4.SetColumnSpan(this.btnWorkerField, 2);
            this.tableLayoutPanel4.Controls.Add(this.btnWorkerField, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.lblPartnerStatus, 0, 1);
            this.tableLayoutPanel4.Controls.Add(this.lblStatusUpdated, 0, 2);
            this.tableLayoutPanel4.Controls.Add(this.lblLastContact, 0, 3);
            this.tableLayoutPanel4.SetColumnSpan(this.cmbPartnerStatus, 2);
            this.tableLayoutPanel4.Controls.Add(this.cmbPartnerStatus, 1, 1);
            this.tableLayoutPanel4.SetColumnSpan(this.txtStatusUpdated, 2);
            this.tableLayoutPanel4.Controls.Add(this.txtStatusUpdated, 1, 2);
            this.tableLayoutPanel4.SetColumnSpan(this.txtLastContact, 2);
            this.tableLayoutPanel4.Controls.Add(this.txtLastContact, 1, 3);
            this.tableLayoutPanel4.Controls.Add(this.txtWorkerField, 2, 0);
            this.grpCollapsible.Text = "Key Partner Data";

            //
            // TUC_PartnerEdit_TopPart
            //
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(700, 500);
            // this.rpsForm.SetRestoreLocation(this, false);  for the moment false, to avoid problems with size
            this.Controls.Add(this.pnlContent);
            this.Name = "TUC_PartnerEdit_TopPart";
            this.Text = "";

	
            this.tableLayoutPanel4.ResumeLayout(false);
            this.pnlRight.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.pnlAdditionalInfo.ResumeLayout(false);
            this.tableLayoutPanel2.ResumeLayout(false);
            this.pnlFamily.ResumeLayout(false);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.pnlPartnerInfo.ResumeLayout(false);
            this.pnlLeft.ResumeLayout(false);
            this.grpCollapsible.ResumeLayout(false);
            this.pnlContent.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();
        }
        private System.Windows.Forms.Panel pnlContent;
        private System.Windows.Forms.GroupBox grpCollapsible;
        private System.Windows.Forms.Panel pnlLeft;
        private System.Windows.Forms.Panel pnlPartnerInfo;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private Ict.Petra.Client.CommonControls.TtxtAutoPopulatedButtonLabel txtPartnerKey;
        private System.Windows.Forms.Label lblPartnerKey;
        private System.Windows.Forms.Label lblEmpty2;
        private System.Windows.Forms.TextBox txtPartnerClass;
        private System.Windows.Forms.Label lblPartnerClass;
        private System.Windows.Forms.Panel pnlFamily;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.TextBox txtFamilyTitle;
        private System.Windows.Forms.Label lblFamilyTitle;
        private System.Windows.Forms.TextBox txtFirstName;
        private System.Windows.Forms.TextBox txtFamilyName;
        private System.Windows.Forms.Label lblEmpty;
        private Ict.Petra.Client.CommonControls.TCmbAutoPopulated cmbAddresseeTypeCode;
        private System.Windows.Forms.Label lblAddresseeTypeCode;
        private System.Windows.Forms.CheckBox chkNoSolicitations;
        private System.Windows.Forms.Panel pnlAdditionalInfo;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TextBox txtLastGift;
        private System.Windows.Forms.Label lblLastGift;
        private System.Windows.Forms.Panel pnlRight;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Button btnWorkerField;
        private System.Windows.Forms.TextBox txtWorkerField;
        private Ict.Petra.Client.CommonControls.TCmbAutoPopulated cmbPartnerStatus;
        private System.Windows.Forms.Label lblPartnerStatus;
        private System.Windows.Forms.TextBox txtStatusUpdated;
        private System.Windows.Forms.Label lblStatusUpdated;
        private System.Windows.Forms.TextBox txtLastContact;
        private System.Windows.Forms.Label lblLastContact;
    }
}