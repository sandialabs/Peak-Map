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
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PeakMap
{
    public class TextLibrary : SpectralLibraryGenerator
    {

        //List<string> libNucs;
        readonly bool canWrite;
        /// <summary>
        /// Initilize TextLibray Object
        /// </summary>
        public TextLibrary()
        {
            performLineComb = true;
            InitilizeLib();
            messageDisplay = new MessageDisplay();
        }
        /// <summary>
        /// Initilize TextLibray Object
        /// </summary>
        /// <param name="file">The file name of the libary</param>
        public TextLibrary(string fileName)
        {
            performLineComb = true;
            InitilizeLib();
            file = fileName;
            canWrite = true;
        }
        /// <summary>
        /// Initilize TextLibray Object
        /// </summary>
        /// <param name="fileName">The file name of the libary</param>
        /// <param name="display">The messagebox display initilization</param>
        public TextLibrary(string fileName, IDisplay display)
        {
            performLineComb = true;
            InitilizeLib();
            file = fileName;
            messageDisplay = display;
        }
        /// <summary>
        /// Initilize TextLibray Object
        /// </summary>
        /// <param name="display">The messagebox display initilization</param>
        public TextLibrary(IDisplay display)
        {
            performLineComb = true;
            InitilizeLib();
            messageDisplay = display;
        }
        private void InitilizeLib()
        {
            lib = new DataSet();
            lib.ReadXmlSchema(Path.Combine(Environment.CurrentDirectory, "MatchesSchema.xsd"));
        }
        /// <summary>
        /// Get the precision of a duplicated line
        /// </summary>
        /// <param name="num">THe number to get the precision of</param>
        /// <returns>precision</returns>
        protected override double GetPrecision(double num)
        {
            return Math.Pow(10, Math.Truncate(Math.Log10(num) - 4));
        }
        
        /// <summary>
        /// Writes the dataset to a file
        /// </summary>
        private void WriteDataToFile()
        {

            StringBuilder strBuilder = new StringBuilder();
            //clear the file first
            if (File.Exists(file))
            {
                File.Delete(file);
            }

            //add the title
            strBuilder.AppendLine(String.Format("{0,-11}{1,-11} ± {2,-13} {3,-12} {4,-10} {5,-12} {6,-10}",
                    "NUCLIDE", "HAlF LIFE", "UNC (1σ)", "ENERGY (keV)", "UNC (1σ)", "YIELD(%)", "UNC (1σ)"));

            foreach (DataRow nuclide in lib.Tables["MATCHEDNUCLIDES"].Rows)
            {
                //format the nuclide
                double tunc = GetUncertainty((double)nuclide["HALF_LIFE"]);
                strBuilder.Append(String.Format("{0,-10}{1,12:E4} ± {2,9:E2}{3,2}",
                    nuclide["NAME"], nuclide["HALF_LIFE"], tunc, nuclide["HALF_LIFE_UNIT"]));

                DataRow[] linesArr = lib.Tables["MATCHEDLINES"].Select("NAME = '" + nuclide["NAME"] + "'", "ENERGY ASC");
                //if there are no lines to write, write the title an nuclide and move on
                if (linesArr.Length < 1)
                {
                    strBuilder.Append(Environment.NewLine);
                    continue;
                }


                DataTable writeLines = linesArr.CopyToDataTable();

                //int keyLine = (int)GetKeyLine(writeLines)["LINENUMBER"];
                double yUnc, Eunc = 0.0;

                //write the first line
                DataRow firstLine = writeLines.Rows[0];
                Eunc = firstLine["ENERGYUNC"] == DBNull.Value ? GetUncertainty((double)firstLine["ENERGY"]) : (double)firstLine["ENERGYUNC"];
                yUnc = firstLine["YIELDUNC"] == DBNull.Value ? GetUncertainty((double)firstLine["YIELD"]) : (double)firstLine["YIELDUNC"];
                string keyLineMarker = (bool)firstLine["ISKEY"] ? "*" : " ";
                strBuilder.AppendLine(String.Format(" {0,0}{1,12:E4} {2,10:E2} {3,12:E4} {4,10:E2}",
                    keyLineMarker, firstLine["ENERGY"], Eunc, firstLine["YIELD"], yUnc));

                //loop through the lines in the writelines
                for (int i = 1; i < writeLines.Rows.Count; i++)
                {
                    DataRow line = writeLines.Rows[i];
                    Eunc = line["ENERGYUNC"] == DBNull.Value ? GetUncertainty((double)line["ENERGY"]) : (double)line["ENERGYUNC"];
                    yUnc = line["YIELDUNC"] == DBNull.Value ? GetUncertainty((double)line["YIELD"]) : (double)line["YIELDUNC"];
                    keyLineMarker = (bool)line["ISKEY"] ? "*" : " ";
                    strBuilder.AppendLine(String.Format("{0,36} {1,0}{2,12:E4} {3,10:E2} {4,12:E4} {5,10:E2}",
                        " ", keyLineMarker, line["ENERGY"], Eunc, line["YIELD"], yUnc));
                }
                //write the data
                File.WriteAllText(file, strBuilder.ToString());
            }
        }
        /// <summary>
        /// Saves the library file
        /// </summary>
        public override void SaveFile()
        {
            if (File.Exists(file))
                return;
            WriteDataToFile();
        }
        /// <summary>
        /// Loads the Library
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public async override Task LoadLibraryAsync(string file)
        {
            //if the file dosen't exist, create it
            if (!File.Exists(file))
                return;

            if (lib == null)
                InitilizeLib();

            string[] lines;
            //get all the lines for the data field
            using (var reader = File.OpenText(file))
            {
                string lineUnSplit = await reader.ReadToEndAsync();
                lines = lineUnSplit.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            }
            int ln = 0;
            int id = 0;
            string nucName = "";
            for (int i = 1; i < lines.Length; i++) //(string line in lines)
            {
                string line = lines[i];
                //nuclide line
                if (string.IsNullOrWhiteSpace(line.Substring(0, 7)))
                {
                    DataRow nucLine = lib.Tables["MATCHEDLINES"].NewRow();
                    nucLine["NAME"] = nucName;
                    nucLine["LINENUMBER"] = ln;
                    nucLine["ENERGY"] = double.Parse(line.Substring(38, 12));
                    nucLine["ENERGYUNC"] = double.Parse(line.Substring(52, 9));
                    nucLine["YIELD"] = double.Parse(line.Substring(62, 12));
                    nucLine["YIELDUNC"] = double.Parse(line.Substring(75, 9));
                    lib.Tables["MATCHEDLINES"].Rows.Add(nucLine);
                    ln++;
                }
                //nuclide
                else
                {
                    ln = 0;
                    DataRow nuc = lib.Tables["MATCHEDNUCLIDES"].NewRow();
                    nuc["ID"] = id;
                    nuc["NAME"] = line.Substring(0, 7).Trim();
                    nuc["HALF_LIFE"] = line.Substring(11, 11);
                    nuc["HALF_LIFE_UNIT"] = line.Substring(35, 2).Trim();
                    nucName = (string)nuc["NAME"];
                    lib.Tables["MATCHEDNUCLIDES"].Rows.Add(nuc);
                    //add the line on the nuclide
                    DataRow nucLine = lib.Tables["MATCHEDLINES"].NewRow();
                    nucLine["NAME"] = nucName;
                    nucLine["LINENUMBER"] = ln;
                    nucLine["ENERGY"] = double.Parse(line.Substring(38, 12));
                    nucLine["ENERGYUNC"] = double.Parse(line.Substring(52, 9));
                    nucLine["YIELD"] = double.Parse(line.Substring(62, 12));
                    nucLine["YIELDUNC"] = double.Parse(line.Substring(75, 9));
                    lib.Tables["MATCHEDLINES"].Rows.Add(nucLine);
                    ln++;
                    id++;
                }
            }

        }

        public override string FileName { get { return file; } }
        public override bool CanWrite { get { return canWrite; } }
        /// <summary>
    }
}
