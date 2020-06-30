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
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace PeakMap
{

    public abstract class SpectralLibraryGenerator
    {
        protected string file;
        public enum LibraryType { nlb, tlb };
        protected abstract double GetPrecision(double num);
        protected abstract void WriteDataToFile();
        public abstract Task LoadLibraryAsync(string file);
        [Browsable(false)]
        public abstract string FileName { get; set; }
        [Browsable(false)]
        public abstract bool CanWrite { get; }
        [Browsable(false)]
        protected DataSet lib;
        [Browsable(false)]
        public DataSet NuclideLibrary { get { return lib; } }

        public delegate bool? CombineLinesCallBack(string energies);
    

        protected bool performLineComb;
        protected double resolutionLimt;
        protected double keyLimit;
        protected IDisplay messageDisplay;
        /// <summary>
        /// Gets or sets wether perfrom line combining
        /// </summary>
        [Category("Line Combinations Settings"),
        DisplayName("Line Combination"),
        Description("Decide whether to combine unresolvable lines"),
        DefaultValue(true)]
        public bool PerfomLineCombination { get { return performLineComb; } set { performLineComb = value; } }
        /// <summary>
        /// Gets or sets the resolution Limit
        /// </summary>
        [Category("Line Combinations Settings"),
        DisplayName("Resolution Limit"),
        Description("The difference between lines (keV) where they may be unresolvable"),
        DefaultValue(0.5)]
        public double ResolutionLimit { get { return resolutionLimt; } set { resolutionLimt = value; } }
        /// <summary>
        /// Gets or sets the lower limit where a line cannot be a key line if a line is within this limit
        /// </summary>
        [Category("Line Combinations Settings"),
         DisplayName("Key Line Interference"),
         Description("The symmetrical band limit (keV) where there may be and interference and to not set as the key line for that nuclide"),
         DefaultValue(3)]
        public double KeyInterferenceLimit { get { return keyLimit; } set { keyLimit = value; } }
        /// <summary>
        /// Returns the uncertainty of a number that is half of the last non-zero digit
        /// </summary>
        /// <param name="number">The number</param>
        /// <returns>The uncertainty</returns>
        protected double GetUncertainty(double number)
        {
            //convert to scientific notation
            string num = number.ToString("E5");
            int index = 0; //= num.LastIndexOf("0", 2, 5,StringComparison.InvariantCultureIgnoreCase);
            //loop through the string starting at the 10th place
            for (int i = 2; i < 7; i++)
            {
                if (num[i] != '0')
                    index = i;
            }
            //get the power of the original number
            int power = num[8] == '+' ? int.Parse(num.Substring(9, 3)) : int.Parse(num.Substring(8, 4));
            //if Index is 0 there is no precision, use 0.5.
            if (index == 0)
                index = power < 0 ? 1 : power + 1;
            //retrun the uncertaitny
            return double.Parse("5.0E" + (power - index));
        }
        /// <summary>
        /// Merge lines that are unresolvable
        /// </summary>
        /// <param name="writeLines">DataTable of lines to check</param>
        /// <param name="resolutionLimt">The lower FWHM where lines are resolved</param>
        protected void MergeUnresolvableLines(DataTable writeLines, CombineLinesCallBack combineLinesCallBack,  double resLimit = 0.5)
        {
            resolutionLimt = resLimit - resolutionLimt < 1e-6 ? ResolutionLimit : resLimit;
            int j = 0;
            while (writeLines.Rows.Count > j)
            {
                DataRow row = writeLines.Rows[j];
                string expression = "ENERGY > '" + ((double)row["ENERGY"] - resolutionLimt) +
                    "' AND ENERGY < '" + ((double)row["ENERGY"] + resolutionLimt) + "'";
                DataRow[] unresLines = writeLines.Select(expression);
                //only do this if there are more than one lines
                if ((unresLines.Length < 2))
                {
                    j++;
                    continue;
                }
                //see if the user wishes to combine energies
                StringBuilder energies = new StringBuilder();
                foreach (DataRow srow in unresLines)
                    energies.Append(srow["ENERGY"].ToString() + ", ");
                energies.Remove(energies.Length - 2, 2);

                //if the display is not initilized 
                if (messageDisplay == null)
                    messageDisplay = new MessageDisplay();

                bool? result = combineLinesCallBack(energies.ToString());

                //DialogResult result = MessageBox.Show("Lines of energies: " + energies + " keV are possibly unresovable, do you wish to combine them?", "Possible Unresolvable Lines", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == false)
                    return;
                else if (result == null)
                {
                    j++;
                    continue;
                }

                //do the combining
                double num = 0.0, denom = 0.0, uncnum = 0.0, Ebar = 0.0, Eunc = 0.0, yield = 0.0, yunc = 0.0;
                for (int i = 0; i < unresLines.Length; i++)
                {
                    num += (double)unresLines[i]["ENERGY"] * (double)unresLines[i]["YIELD"];
                    denom += (double)unresLines[i]["YIELD"];
                    double unc = GetUncertainty((double)unresLines[i]["YIELD"]);
                    yunc += Math.Pow(unc, 2);
                }
                Ebar = num / denom;
                yield = denom;
                for (int i = 0; i < unresLines.Length; i++)
                {
                    uncnum += (double)unresLines[i]["YIELD"] * Math.Pow(((double)unresLines[i]["ENERGY"] - Ebar), 2);
                    //delete all lines that arn't the row. 
                    if (unresLines[i] != row)
                        unresLines[i].Delete();
                }
                //finshup by combining and taking the square root
                Eunc = Math.Sqrt(uncnum / ((((double)unresLines.Length - 1.0) / (double)unresLines.Length) * denom));
                yunc = Math.Sqrt(yunc);
                //if the energy uncertainty is zero do standard uncertainty
                if (Math.Abs(Eunc) < 1e-9)
                    Eunc = GetUncertainty(Ebar);

                writeLines.AcceptChanges();
                //add the numbers 

                row["ENERGY"] = Ebar;
                row["YIELD"] = denom;
                row["ENERGYUNC"] = Eunc;
                row["YIELDUNC"] = yunc;
                j++;
            }
        }
        /// <summary>
        /// Get the highest yield line and set it as the key line
        /// </summary>
        /// <param name="writeLines"></param>
        /// <returns></returns>
        protected DataRow GetKeyLine(DataTable writeLines)
        {
            //get the largest yield 
            DataRow[] Lines = writeLines.Select("LINENUMBER > -1", "YIELD ASC");
            //DataRow keyLine = writeLines.Select("LINENUMBER > -1", "YIELD ASC").LastOrDefault();

            //loop through lines and set the key line to the largest yield that doesn't have intefearnences
            foreach (DataRow line in Lines)
            {
                if (!HasLineInterferences(line))
                    return line;
            }
            //if it dosen't find a line just use the largest yield
            return Lines.FirstOrDefault();

        }
        /// <summary>
        /// Get the highest yield line and set it as the key line
        /// </summary>
        /// <param name="writeLines"></param>
        protected void SetKeyLine(DataTable writeLines)
        {
            //get the largest yield 
            DataRow[] Lines = writeLines.Select("LINENUMBER > -1", "YIELD DESC");
            //DataRow keyLine = writeLines.Select("LINENUMBER > -1", "YIELD ASC").LastOrDefault();
            bool keyLineSet = false;
            //loop through lines and set the key line to the largest yield that doesn't have intefearnences
            foreach (DataRow line in Lines) 
            {
                if (!HasLineInterferences(line))
                {
                    line["ISKEY"] = true;
                    keyLineSet = true;
                    break;
                }
            }
            //if it dosen't find a line just use the largest yield
            if (!keyLineSet)
                Lines.FirstOrDefault()["ISKEY"] = true;
        }
        /// <summary>
        /// Check if a line has an intefearing line
        /// </summary>
        /// <param name="line"></param>
        /// <returns></returns>
        protected bool HasLineInterferences(DataRow line) 
        {
            return lib.Tables["MATCHEDLINES"].AsEnumerable().Any(row => (double)line["ENERGY"] < row.Field<double>("ENERGY") + keyLimit
                                                                     && (double)line["ENERGY"] > row.Field<double>("ENERGY") - keyLimit);
        }
        /// <summary>
        /// Gets the type of Library Data file to use
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static Type GetSpectralDataType(string file)
        {
            //covert to enum
            if (Enum.TryParse(Path.GetExtension(file).Trim('.'), true, out LibraryType sType))
            {
                switch (sType)
                {
                    case LibraryType.nlb:
                        return typeof(CAMLibrary);
                    case LibraryType.tlb:
                        return typeof(TextLibrary);
                        //case SpecType.csv:
                        //    return typeof(TextData);
                }
            }
            throw new ArgumentException("File type is not recognized as library to write");
        }
        /// <summary>
        /// Gets the nuclides ready to write
        /// </summary>
        /// <param name="nuclide"></param>
        /// <param name="writeLines"></param>
        public void WriteNuclide(DataRow nuclide, DataTable writeLines, CombineLinesCallBack combineLinesCallBack) 
        {
            if ((string)nuclide["NAME"] == "D.E." || (string)nuclide["NAME"] == "S.E." || ((string)nuclide["NAME"]).Contains("Sum"))
                return;
            //check for nuclide duplicates
            IEnumerable<DataRow> nucDup = lib.Tables["MATCHEDNUCLIDES"].AsEnumerable().Where(row => nuclide["NAME"].ToString().Trim().Equals(row.Field<string>("NAME").Trim())) ;
            if (nucDup.Count() > 0)
            {
                //check for line duplicates
                int ln = lib.Tables["MATCHEDLINES"].Select("NAME = '" + (string)nuclide["NAME"] + "'").Length;
                int keyLine = (int)GetKeyLine(writeLines)["LINENUMBER"];
                foreach (DataRow writeLine in writeLines.Rows)
                {
                    IEnumerable<DataRow> lineDup = lib.Tables["MATCHEDLINES"].AsEnumerable().Where(row => Math.Abs((double)writeLine["ENERGY"] - row.Field<double>("ENERGY")) < GetPrecision((double)writeLine["ENERGY"])
                                                                                                   && Math.Abs((double)writeLine["YIELD"] - row.Field<double>("YIELD")) < GetPrecision((double)writeLine["YIELD"]));
                    if (lineDup.Count() > 0)
                        continue;

                    ln++;
                    DataRow addedLine = lib.Tables["MATCHEDLINES"].NewRow();
                    addedLine.ItemArray = writeLine.ItemArray;
                    addedLine["LINENUMBER"] = ln;
                    addedLine["ISKEY"] = ln == keyLine;
                    lib.Tables["MATCHEDLINES"].Rows.Add(addedLine);
                }
            }
            //if there are no duplicates
            else
            {
                if (performLineComb)
                    MergeUnresolvableLines(writeLines, combineLinesCallBack);

                DataRow nuc = lib.Tables["MATCHEDNUCLIDES"].NewRow();
                nuc.ItemArray = nuclide.ItemArray;
                while (lib.Tables["MATCHEDNUCLIDES"].Select("ID = " + nuc["ID"]).Length > 0)
                    nuc["ID"] =+ 1;

                lib.Tables["MATCHEDNUCLIDES"].Rows.Add(nuc);
                SetKeyLine(writeLines);
                int ln = 0;
                foreach (DataRow line in writeLines.Rows)
                {
                    ln++;
                    DataRow writeLine = lib.Tables["MATCHEDLINES"].NewRow();
                    writeLine.ItemArray = line.ItemArray;
                    writeLine["LINENUMBER"] = ln;
                    if (!(writeLine["NAME"].ToString().Equals(nuc["NAME"].ToString())))
                        writeLine["NAME"] = nuc["NAME"];
                    
                    lib.Tables["MATCHEDLINES"].Rows.Add(writeLine);
                }
            }
            lib.AcceptChanges();
        }
        /// <summary>
        /// Clears the nuclide and its lines from the library
        /// </summary>
        /// <param name="nuclide">nuclide to clear</param>
        public void ClearNuclide(string nuclide) 
        {
            //remove pending state changes
            lib.Tables["MATCHEDLINES"].AcceptChanges();
            lib.Tables["MATCHEDNUCLIDES"].AcceptChanges();

            //delete the lines
            DataRow[] deleteLines = lib.Tables["MATCHEDLINES"].Select("NAME = '" + nuclide + "'");
            foreach(DataRow row in deleteLines) 
                row.Delete();

            //delete the rows
            DataRow[] deleteNucs = lib.Tables["MATCHEDNUCLIDES"].Select("NAME = '" + nuclide + "'");
            foreach (DataRow row in deleteNucs)
                row.Delete();

            //accpet the changes
            lib.Tables["MATCHEDLINES"].AcceptChanges();
            lib.Tables["MATCHEDNUCLIDES"].AcceptChanges();
        }
        /// <summary>
        /// Clears the lines of energy from the library
        /// </summary>
        /// <param name="energy">energy line to clear</param>
        public void ClearLines(double energy) 
        {
            //remove pending state changes
            lib.Tables["MATCHEDLINES"].AcceptChanges();

            //Get the lines and delete them
            foreach (DataRow line in lib.Tables["MATCHEDLINES"].Rows.OfType<DataRow>().Where(r => Math.Abs(energy - (double)r["ENERGY"]) < ResolutionLimit))
                line.Delete();
            //accpet the changes
            lib.Tables["MATCHEDLINES"].AcceptChanges();

        }
        /// <summary>
        /// Clears the nuclide and its lines from the library
        /// </summary>
        /// <param name="nuclide">nuclide to clear</param>
        public void ClearNuclide(DataRow nuclide)
        {
            ClearNuclide((string)nuclide["MATCHNAME"]);
        }
        /// <summary>
        /// Save the library file
        /// </summary>
        public void SaveFile() 
        {
            //if the file exists it will be overwirtten 
            if (File.Exists(file)) 
                File.Delete(file);
            
            WriteDataToFile();
        }


    }
}
