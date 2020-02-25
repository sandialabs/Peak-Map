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
using System.Text;
using System.Threading.Tasks;

namespace PeakMap
{
    class TextData : SpectralData
    {
        public enum EnergyUnits
        {
            keV,
            eV,
            MeV
        }
        public enum InputColumns
        {
            Energy,
            FWHM,
            Area,
            AreaUnc,
            TotalCounts,
            Other
        }

        char delimiter;
        List<InputColumns> columnOrder;
        EnergyUnits units = EnergyUnits.keV;


        /// <summary>
        /// Gets or sets the column delimiter
        /// </summary>
        [Category("Parser"),
        DisplayName("Delimiter"),
        Description("Delimeter of column seperation")]
        public char Delimiter { get { return delimiter; } set { delimiter = value; } }
        /// <summary>
        /// Gets or sets the column order
        /// </summary>
        [Category("Parser"),
        DisplayName("Column Order"),
        Description("Enter all of the peak columns in order")]
        public List<InputColumns> ColumnOrder { get { return columnOrder; } set { columnOrder = value; } }
        /// <summary>
        /// Gets or sets the column order
        /// </summary>
        [Category("Parser"),
        DisplayName("Energy Units"),
        Description("The units of energy and FWHM")]
        public EnergyUnits Units { get { return units; } set { units = value; } }

        [Browsable(false)]
        DataTable data;

        public TextData(string inpfile)
        {
            //calParams = new List<double> { -1.0e-4, 1.4e-17, -8.7e-19 };
            effMeas = new System.Collections.ObjectModel.ObservableCollection<EfficiencyMeasurement>();
            double eff = 0.9999999999999;
            double effunc = 0.000000000005;
            effMeas.Add(new EfficiencyMeasurement(Properties.Settings.Default.LOWERELIMT, eff, effunc));
            effMeas.Add(new EfficiencyMeasurement((Properties.Settings.Default.LOWERELIMT + Properties.Settings.Default.UPPERELIMIT) / 2, eff, effunc));
            effMeas.Add(new EfficiencyMeasurement(Properties.Settings.Default.UPPERELIMIT, eff, effunc));
            order = 2;
            effMeas.CollectionChanged += EffMeas_CollectionChanged;
            //define the defult delimiter
            delimiter = ',';
            //define the defult column order
            columnOrder = new List<InputColumns>()
            { InputColumns.Energy, InputColumns.Area, InputColumns.AreaUnc, InputColumns.Other, InputColumns.FWHM, InputColumns.Other,
            InputColumns.Other, InputColumns.TotalCounts};

            acqTime = DateTime.MinValue;
            collTime = DateTime.MinValue;
            elapsedWait = collTime - acqTime;
            countTime = 0.0;
            file = inpfile;
        }

        /// <summary>
        /// Loads the peak data from the specified file and loads in into the peaks datatable
        /// </summary>
        /// <param name="peaks">container for the peak data</param>
        public async override Task LoadDataAsync(DataTable peaks)
        {

            //check for files
            if (!File.Exists(file))
                return;
            //get the file stream
            FileStream stream = new FileStream(file, FileMode.Open, FileAccess.Read);
            string[] lines;
            //get the input lines 
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
                lines = (await reader.ReadToEndAsync())
                    .ToString().Split(new[] { Environment.NewLine }, StringSplitOptions.None);

            if (lines == null || lines.Length < 1)
                throw new ArgumentException("Input file is not readable");
            //create a container
            string[][] peakText = new string[lines.Length][];
            //loop through the lines and split on the delimiter
            for (int i = 0; i < lines.Length; i++)
            {
                peakText[i] = lines[i].Split(delimiter);
            }
            //fill the data table
            Fill(peaks, peakText);
        }

        /// <summary>
        /// Fills the peaks data Table
        /// </summary>
        /// <param name="peaks">Container for the peaks data table</param>
        private void Fill(DataTable peaks, string[][] peakText)
        {
            //loop through the lines
            foreach (string[] row in peakText)
            {
                double temp;
                int energyIndex = columnOrder.IndexOf(InputColumns.Energy);
                //check if there are titles and skip them if there are
                if (double.TryParse(row[columnOrder.IndexOf(InputColumns.Energy)], out temp))
                {
                    DataRow peak = peaks.NewRow();
                    peak["ENERGY"] = temp;
                    peak["FWHM"] = double.TryParse(row[columnOrder.IndexOf(InputColumns.FWHM)], out temp) ? temp : 0.0;
                    peak["AREA"] = double.TryParse(row[columnOrder.IndexOf(InputColumns.Area)], out temp) ? temp : 0.0;
                    peak["AREAUNC"] = double.TryParse(row[columnOrder.IndexOf(InputColumns.Area)], out temp) ? temp : 0.0;
                    peak["CONTINUUM"] = double.TryParse(row[columnOrder.IndexOf(InputColumns.TotalCounts)], out temp) ? temp - (double)peak["AREA"] : 0.0;
                    peaks.Rows.Add(peak);
                }
                else
                {
                    TryGetColumnHeader(row);
                    continue;
                }

            }
            data = peaks;
        }
        /// <summary>
        /// Try to get the column order
        /// </summary>
        /// <param name="header"></param>
        private void TryGetColumnHeader(string[] header)
        {

            //int energyIndex = header.Any(s => s.IndexOf("energy", StringComparison.OrdinalIgnoreCase) > -1 ||
            //    s.Trim('.').IndexOf("ene", StringComparison.OrdinalIgnoreCase) > -1||
            //    s.IndexOf("centroid", StringComparison.OrdinalIgnoreCase)  > -1||
            //    s.Trim('.').IndexOf("centrd", StringComparison.OrdinalIgnoreCase) > -1
            //    )? 
        }
        //TODO Implement GetWidth
        protected override int GetWidth(int ch)
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Get the counts in a region of the spectrum by linear interpolation
        /// </summary>
        /// <param name="energy">Energy at the region</param>
        /// <returns>Number of counts in the region</returns>
        public override double GetRegion(double energy)
        {
            if (data == null)
                return 0.0;
            //get the peaks above and blow the energy
            DataRow[] peaksAbove = data.Select("ENERGY >= '" + energy + "'", "ENERGY ASC");
            DataRow[] peaksBelow = data.Select("ENERGY <= '" + energy + "'", "ENERGY DESC");
            //there is no information just return a zero
            if (peaksAbove.Length < 1 && peaksBelow.Length < 1)
                return 0.0;
            //fist peak in the spectrum return the first peak continuum
            else if (peaksAbove.Length >= 1)
                return (double)peaksAbove[0]["CONTINUUM"];
            //last peak in the spectrum return the last peak continuum
            else if (peaksBelow.Length >= 1)
                return (double)peaksBelow[0]["CONTINUUM"];
            else
            {
                DataRow above = peaksAbove[0];
                DataRow below = peaksBelow[0];
                //linearly interpolate
                return (double)below["CONTINUUM"] + ((double)energy - (double)below["ENERGY"]) *
                    ((double)above["CONTINUUM"] - (double)below["CONTINUUM"]) / ((double)above["ENERGY"] - (double)below["ENERGY"]);
            }
        }
        /// <summary>
        /// Closes the specral file
        /// </summary>
        public override void CloseFile() { }
    }
}
