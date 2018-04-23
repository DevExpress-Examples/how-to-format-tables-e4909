﻿using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using DevExpress.Spreadsheet;
using DevExpress.XtraEditors.Controls;

namespace WindowsFormsApplication1 {
    public partial class Form1 : Form {
        IWorkbook workbook;
        Table table;
        public Form1() {
            InitializeComponent();
            workbook = spreadsheetControl1.Document;

            ImportDataTable(workbook);
            table = CreateTable(workbook);
            PopulateComboBoxWithBuiltInTableStyles(workbook);
            comboBuiltInTableStyles.SelectedIndex = 0;
        }


        private void comboBuiltInTableStyles_SelectedIndexChanged(object sender, EventArgs e) {
            workbook.BeginUpdate();
            try {
                // Get the name of the table style to be applied to a table
                // (from the combo box that lists all built-in table styles).
                string styleName = comboBuiltInTableStyles.SelectedItem.ToString();

                ApplyBuiltInTableStyle(workbook, table, styleName);
            }
            finally {
                workbook.EndUpdate();
            }
        }

        private void btnChangeDefaultStyle_Click(object sender, EventArgs e) {
            string styleName = comboBuiltInTableStyles.SelectedItem.ToString();
            ChangeDefaultTableStyle(workbook, styleName);
            labelControl3.Text = workbook.TableStyles.DefaultStyle.Name;
        }

        private void btnCreateNewTable_Click(object sender, EventArgs e) {
            // Create a new table including the cell range that is currently selected in the spreadsheet control.
            // The default style (workbook.TableStyles.DefaultStyle) is automatically applied to the created table.
            Table newTable = workbook.Worksheets.ActiveWorksheet.Tables.Add(spreadsheetControl1.Selection, true);
        }

        private void btnDuplicateTableStyle_Click(object sender, EventArgs e) {
            workbook.BeginUpdate();
            try {
                // Get the name of the table style to be duplicated 
                // (from the combo box that lists all built-in table styles).
                string styleName = comboBuiltInTableStyles.SelectedItem.ToString();

                TableStyle newTableStyle = DuplicateAndModifyTableStyle(workbook, styleName);
                table.Style = newTableStyle;
            }
            finally {
                workbook.EndUpdate();
            }   
        }

        private void btnCreateCustomTableStyle_Click(object sender, EventArgs e) {
            workbook.BeginUpdate();
            try {
                TableStyle customStyle = CreateCustomTableStyle(workbook, table);
                table.Style = customStyle;
                table.ShowTableStyleRowStripes = true;
            }
            finally {
                workbook.EndUpdate();
            }
        }

        #region #PopulateComboBoxWithBuiltInTableStyles
        void PopulateComboBoxWithBuiltInTableStyles(IWorkbook workbook) {
            // Add names of all built-in table styles contained in the workbook to the combo box.
            ComboBoxItemCollection comboBoxItems = comboBuiltInTableStyles.Properties.Items;
            comboBoxItems.BeginUpdate();
            try {
                Array builtInTableStyleIds = Enum.GetValues(typeof(BuiltInTableStyleId));
                for (int i = 0; i < builtInTableStyleIds.Length; i++) {
                    comboBoxItems.Add(workbook.TableStyles[(BuiltInTableStyleId)builtInTableStyleIds.GetValue(i)].Name);
                }
            }
            finally {
                comboBoxItems.EndUpdate();
            }
        }
        #endregion #PopulateComboBoxWithBuiltInTableStyles

        #region #ImportDataTable
        void ImportDataTable(IWorkbook workbook) {

            Worksheet worksheet = workbook.Worksheets[0];

            // Create the "Products" DataTable object with four columns.
            DataTable sourceTable = new DataTable("Products");
            sourceTable.Columns.Add("Product", typeof(string));
            sourceTable.Columns.Add("Price", typeof(float));
            sourceTable.Columns.Add("Quantity", typeof(Int32));
            sourceTable.Columns.Add("Discount", typeof(float));

            sourceTable.Rows.Add("Chocolade", 5, 15, 0.03);
            sourceTable.Rows.Add("Konbu", 9, 55, 0.1);
            sourceTable.Rows.Add("Geitost", 15, 70, 0.07);

            // Import data from the data table into the worksheet and insert it, starting with the B2 cell.
            worksheet.Import(sourceTable, true, 1, 1);
        }
        #endregion #ImportDataTable

        #region #CreateTable
        Table CreateTable(IWorkbook workbook) {

            Worksheet worksheet = workbook.Worksheets[0];

            // Insert a table in the worksheet.
            Table table = worksheet.Tables.Add(worksheet["B2:F5"], true);

            // Access table columns.
            TableColumn productColumn = table.Columns[0];
            TableColumn priceColumn = table.Columns[1];
            TableColumn quantityColumn = table.Columns[2];
            TableColumn discountColumn = table.Columns[3];
            TableColumn amountColumn = table.Columns[4];

            // Set the name of the last column. 
            amountColumn.Name = "Amount";

            // Set the formula to calculate the amount per product 
            // and display results in the "Amount" column.
            amountColumn.Formula = "=[Price]*[Quantity]*(1-[Discount])";

            // Display the total row in the table.
            table.ShowTotals = true;

            // Set the label and function to display the sum of the "Amount" column.
            discountColumn.TotalRowLabel = "Total:";
            amountColumn.TotalRowFunction = TotalRowFunction.Sum;

            // Specify the number format for each column.
            priceColumn.DataRange.NumberFormat = "$#,##0.00";
            discountColumn.DataRange.NumberFormat = "0.0%";
            amountColumn.Range.NumberFormat = "$#,##0.00;$#,##0.00;\"\";@";

            // Specify horizontal alignment for header and total rows of the table.
            table.HeaderRowRange.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;
            table.TotalRowRange.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;

            // Specify horizontal alignment to display data in all columns except the first one.
            for (int i = 1; i < table.Columns.Count; i++) {
                table.Columns[i].DataRange.Alignment.Horizontal = SpreadsheetHorizontalAlignment.Center;
            }

            // Set the width of table columns.
            table.Range.ColumnWidthInCharacters = 10;

            return table;
        }
        #endregion #CreateTable

        #region #ApplyBuiltInTableStyle
        void ApplyBuiltInTableStyle(IWorkbook workbook, Table table, string styleName) {
            // Access the workbook's collection of table styles.
            TableStyleCollection tableStyles = workbook.TableStyles;

            // Access the built-in table style from the collection by its name.
            TableStyle tableStyle = tableStyles[styleName];

            // Apply the table style to the existing table.
            table.Style = tableStyle;

            // Show header and total rows.
            table.ShowHeaders = true;
            table.ShowTotals = true;

            // Apply banded column formatting to the table.
            table.ShowTableStyleRowStripes = false;
            table.ShowTableStyleColumnStripes = true;
        }
        #endregion #ApplyBuiltInTableStyle

        #region #ChangeDefaultTableStyle
        void ChangeDefaultTableStyle(IWorkbook workbook, string styleName) {
            // Set the style to be applied to created tables by default.
            workbook.TableStyles.DefaultStyle = workbook.TableStyles[styleName];
        }
        #endregion #ChangeDefaultTableStyle

        #region #DuplicateAndModifyStyle
        TableStyle DuplicateAndModifyTableStyle(IWorkbook workbook, string sourceStyleName) {
            // Get the table style to be duplicated.
            TableStyle sourceTableStyle = workbook.TableStyles[sourceStyleName];

            // Duplicate the table style.
            TableStyle newTableStyle = sourceTableStyle.Duplicate();

            // Modify the required formatting characteristics of the created table style.
            // For example, remove exisitng formatting from the header row element.
            newTableStyle.TableStyleElements[TableStyleElementType.HeaderRow].Clear();

            return newTableStyle;
        }
        #endregion #DuplicateAndModifyStyle

        #region #CreateCustomStyle
        TableStyle CreateCustomTableStyle(IWorkbook workbook, Table table) {
            String styleName = "testTableStyle";

            // If the style under the specified name already exists in the collection, return this style.
            if (workbook.TableStyles.Contains(styleName)) {
                return workbook.TableStyles[styleName];
            }
            else{
                // Add a new table style under the "testTableStyle" name to the TableStyles collection.
                TableStyle customTableStyle = workbook.TableStyles.Add("testTableStyle");

                // Modify the required formatting characteristics of the table style. 
                // Specify the format for different table elements.
                customTableStyle.BeginUpdate();
                try {
                    customTableStyle.TableStyleElements[TableStyleElementType.WholeTable].Font.Color = Color.FromArgb(107, 107, 107);

                    // Specify formatting characteristics for the table header row. 
                    TableStyleElement headerRowStyle = customTableStyle.TableStyleElements[TableStyleElementType.HeaderRow];
                    headerRowStyle.Fill.BackgroundColor = Color.FromArgb(64, 66, 166);
                    headerRowStyle.Font.Color = Color.White;
                    headerRowStyle.Font.Bold = true;

                    // Specify formatting characteristics for the table total row. 
                    TableStyleElement totalRowStyle = customTableStyle.TableStyleElements[TableStyleElementType.TotalRow];
                    totalRowStyle.Fill.BackgroundColor = Color.FromArgb(115, 193, 211);
                    totalRowStyle.Font.Color = Color.White;
                    totalRowStyle.Font.Bold = true;

                    // Specify banded row formatting for the table.
                    TableStyleElement secondRowStripeStyle = customTableStyle.TableStyleElements[TableStyleElementType.SecondRowStripe];
                    secondRowStripeStyle.Fill.BackgroundColor = Color.FromArgb(234, 234, 234);
                    secondRowStripeStyle.StripeSize = 1;
                }
                finally {
                    customTableStyle.EndUpdate();
                }
                return customTableStyle;
            }
        }
        #endregion #CreateCustomStyle

    }
}