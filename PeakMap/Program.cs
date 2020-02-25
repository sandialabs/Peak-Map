/********************************************************************************************************************************************************************************************************************************************************                          
NOTICE:

For five (5) years from 1/21/2020 the United States Government is granted for itself and others acting on its behalf a paid-up, nonexclusive, irrevocable worldwide license in this data to reproduce, prepare derivative works, and perform 
publicly and display publicly, by or on behalf of the Government. There is provision for the possible extension of the term of this license. Subsequent to that period or any extension granted, the United States Government is granted for itself
and others acting on its behalf a paid-up, nonexclusive, irrevocable worldwide license in this data to reproduce, prepare derivative works, distribute copies to the public, perform publicly and display publicly, and to permit others to do so. The
specific term of the license can be identified by inquiry made to National Technology and Engineering Solutions of Sandia, LLC or DOE.
 
NEITHER THE UNITED STATES GOVERNMENT, NOR THE UNITED STATES DEPARTMENT OF ENERGY, NOR NATIONAL TECHNOLOGY AND ENGINEERING SOLUTIONS OF SANDIA, LLC, NOR ANY OF THEIR EMPLOYEES, MAKES ANY WARRANTY, 
EXPRESS OR IMPLIED, OR ASSUMES ANY LEGAL RESPONSIBILITY FOR THE ACCURACY, COMPLETENESS, OR USEFULNESS OF ANY INFORMATION, APPARATUS, PRODUCT, OR PROCESS DISCLOSED, OR REPRESENTS THAT ITS USE WOULD
NOT INFRINGE PRIVATELY OWNED RIGHTS.
 
Any licensee of this software has the obligation and responsibility to abide by the applicable export control laws, regulations, and general prohibitions relating to the export of technical data. Failure to obtain an export control license or other 
authority from the Government may result in criminal liability under U.S. laws.
 
                                             (End of Notice)
*********************************************************************************************************************************************************************************************************************************************************/
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PeakMap
{
    static class Program
    {

        [DllImport("Shcore.dll")]
        static extern int SetProcessDpiAwareness(int PROCESS_DPI_AWARENESS);

        // According to https://msdn.microsoft.com/en-us/library/windows/desktop/dn280512(v=vs.85).aspx
        private enum DpiAwareness
        {
            None = 0,
            SystemAware = 1,
            PerMonitorAware = 2
        }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            SetProcessDpiAwareness((int)DpiAwareness.PerMonitorAware);
            Application.Run(new Main());
            #region xmlBuilder
            //string path = @"C:\Users\eleonar\Documents\DataClasses.txt";
            //using (StreamWriter sw = File.AppendText(path)) { 
            //    foreach (CanberraDataAccessLib.ClassCodes dataCalss in Enum.GetValues(typeof(CanberraDataAccessLib.ClassCodes)))
            //    {
            //        sw.WriteLine(dataCalss);
            //    }
            //}
            //string[] tables = CAMMap.CAM_Tables;
            //DataSet CAMset = new DataSet();
            //foreach (string table in tables)
            //{
            //    DataTable data = new DataTable(table);
            //    string[] columns = CAMMap.GetColumnList(table);
            //    string first = columns.First();
            //    foreach (string columnName in columns)
            //    {
            //        if (columnName.Equals(first))
            //            continue;

            //        string dataChar = columnName.Substring(4, 1);
            //        DataColumn column;
            //        switch (dataChar)
            //        {
            //            case "T":
            //                column = new DataColumn(columnName, typeof(string));
            //                column.AllowDBNull = true;
            //                data.Columns.Add(column);
            //                break;
            //            case "F":
            //                column = new DataColumn(columnName, typeof(float));
            //                column.AllowDBNull = true;
            //                data.Columns.Add(column);
            //                break;
            //            case "G":
            //                column = new DataColumn(columnName, typeof(float));
            //                column.AllowDBNull = true;
            //                data.Columns.Add(column);
            //                break;
            //            case "L":
            //                column = new DataColumn(columnName, typeof(decimal));
            //                column.AllowDBNull = true;
            //                data.Columns.Add(column);
            //                break;
            //            case "X":
            //                column = new DataColumn(columnName, typeof(DateTime));
            //                column.AllowDBNull = true;
            //                data.Columns.Add(column);
            //                break;
            //            default:
            //                column = new DataColumn(columnName, typeof(int));
            //                column.AllowDBNull = false;
            //                data.Columns.Add(column);
            //                break;
            //        }
            //        if (columnName.Equals("CAMENTRY"))
            //        {
            //            data.PrimaryKey = new DataColumn[] { data.Columns["CAMRECORD"], data.Columns["CAMENTRY"] };
            //        }
            //        if (columnName.Equals("CAMRECORD"))
            //            data.PrimaryKey = new DataColumn[] { data.Columns["CAMRECORD"] };
            //    }
            //    CAMset.Tables.Add(data);
            //    data.Clear();
            //}
            //foreach (DataTable table in CAMset.Tables)
            //{
            //    string[] nameParts = table.TableName.Split('_');
            //    string tableType = nameParts[2];
            //    string shortName;// = nameParts[1];
            //    if (nameParts.Length > 3)
            //        shortName = nameParts[1] + "_" + nameParts[3];
            //    else
            //        shortName = nameParts[1];

            //    if (tableType.Equals("RTABULAR") && shortName.Equals("MOREPEAK"))
            //        CAMset.Relations.Add(nameParts[1], new DataColumn[] { CAMset.Tables["CAM_PEAK_RECORD"].Columns[0] }, new DataColumn[] { CAMset.Tables["CAM_" + shortName + "_RTABULAR"].Columns[0] }, true);
            //    else if (tableType.Equals("RTABULAR")) {
            //        CAMset.Relations.Add(shortName, new DataColumn[] { CAMset.Tables["CAM_" + nameParts[1] + "_RECORD"].Columns[0] }, new DataColumn[] { CAMset.Tables[table.TableName].Columns[0] }, true);
            //    }
            //}
            //CAMset.WriteXmlSchema(@"C:\Users\eleonar\Documents\CAMSchema.xml");
            #endregion
        }
    }
}
