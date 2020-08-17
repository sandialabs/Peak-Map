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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CAMInputOutput;
using System.IO;
using PeakMap.Properties;

namespace PeakMap
{
    public class CAMLibrary : SpectralLibraryGenerator
    {
        readonly CAMIO camfile;
        /// <summary>
        /// Constructor for CAMLibrary
        /// </summary>
        public CAMLibrary() 
        {
            performLineComb = true;
            camfile = new CAMIO();
            resolutionLimt = 0.5;

        }
        /// <summary>
        /// Constructor for CAMLibrary
        /// </summary>
        /// <param name="inpfile"></param>
        public CAMLibrary(string inpfile) 
        {
            //open and read the file
            
            file = inpfile;
            camfile = new CAMIO();
            performLineComb = true;
            InitilizeLib();
            resolutionLimt = 0.5;

        }

        /// <summary>
        /// Initilize library
        /// </summary>
        private void InitilizeLib()
        {
           
            lib = new DataSet();
            lib.ReadXmlSchema(Path.Combine(Environment.CurrentDirectory, "MatchesSchema.xsd"));
        }
        /// <summary>
        /// The file name
        /// </summary>
        public override string FileName { 
            get { return file; }
            set { file = value; }
        }
        /// <summary>
        /// Can the file be written
        /// </summary>
        public override bool CanWrite 
        {
            get { return true; }
        }

        /// <summary>
        /// Write the data to a CAM file
        /// </summary>
        protected override void WriteDataToFile()
        {
            //check if there are things to even write
            if (lib.Tables["MATCHEDNUCLIDES"].Rows.Count < 1 && lib.Tables["MATCHEDLINES"].Rows.Count < 1)
                return;

            //clear the file first
            if (File.Exists(file))
            {
                File.Delete(file);
            }
            int nucID = 1;
            foreach (DataRow nuclide in lib.Tables["MATCHEDNUCLIDES"].Rows)
            {
                DataRow[] nucLines = lib.Tables["MATCHEDLINES"].Select("NAME = '" + nuclide["NAME"] + "'", "ENERGY ASC");
                //always do the lines first
                
                foreach (DataRow nucLine in nucLines)
                {
                    Line line = new Line
                    {
                        Abundance = (double)nucLine["YIELD"],
                        Energy = (double)nucLine["ENERGY"],
                        EnergyUncertainty = nucLine["ENERGYUNC"] == DBNull.Value ? GetUncertainty((double)nucLine["ENERGY"]) : (double)nucLine["ENERGYUNC"],
                        AbundanceUncertainty = nucLine["YIELDUNC"] == DBNull.Value ? GetUncertainty((double)nucLine["YIELD"]) : (double)nucLine["YIELDUNC"],
                        IsKeyLine = (bool)nucLine["ISKEY"],
                        NuclideIndex = nucID
                    };
                    //add the line
                    camfile.AddLine(line);
                }
                //now do the nuclide
                Nuclide nuc = new Nuclide
                {
                    Name = ((string)nuclide["NAME"]).Trim(),
                    HalfLife = (double)nuclide["HALF_LIFE"],
                    HalfLifeUnit = ((string)nuclide["HALF_LIFE_UNIT"]).Trim(),
                    HalfLifeUncertainty = GetUncertainty((double)nuclide["HALF_LIFE"]),
                    Index = nucID
                };
                //add the nuclide
                camfile.AddNuclide(nuc);
                nucID++;
            }
            camfile.CreateFile(file);
            
        }
        /// <summary>
        /// async load a library from file
        /// </summary>
        /// <param name="file">The path to the file</param>
        /// <returns></returns>
        public override async Task LoadLibraryAsync(string file)
        {
            //if the file dosen't exist
            if (!File.Exists(file))
                return;

            if (lib == null)
                InitilizeLib();

            //read the file
            camfile.ReadFile(file);

            //get the data from the file async
            Task<Nuclide[]> nucTask = Task.Run(() => camfile.GetNuclides().ToArray());
            Task<Line[]> linesTask = Task.Run(() => camfile.GetLines().ToArray());

            //parse the nuclides
            Nuclide[] nucs = await nucTask;
            foreach (Nuclide nuc in nucs) 
            {
                DataRow nucRow = lib.Tables["MATCHEDNUCLIDES"].NewRow();
                nucRow["ID"] = nuc.Index;
                nucRow["NAME"] = nuc.Name;
                nucRow["HALF_LIFE"] = nuc.HalfLife;
                nucRow["HALF_LIFE_UNIT"] = nuc.HalfLifeUnit;
                lib.Tables["MATCHEDNUCLIDES"].Rows.Add(nucRow);
            }
            //parse the lines
            Line[] lines = await linesTask;
            int ln = 0; string name = "";
            foreach (Line line in lines) 
            {
                //check for bad lines
                if (line.Abundance < Settings.Default.LOWERYIELD || line.Energy < Settings.Default.LOWERELIMT)
                    continue;

                DataRow nucRow = lib.Tables["MATCHEDLINES"].NewRow();
                //check if the name has changed, if so reset the line number
                //ln = name != nucs[line.NuclideIndex-1].Name ? 0 : ln;
                name = nucs.Where(r => r.Index == line.NuclideIndex).First().Name;
                nucRow["NAME"] = name;
                nucRow["LINENUMBER"] = ln ;
                nucRow["ENERGY"] = line.Energy;
                nucRow["ENERGYUNC"] = line.Energy;
                nucRow["YIELD"] = line.Abundance;
                nucRow["YIELDUNC"] = line.AbundanceUncertainty;
                lib.Tables["MATCHEDLINES"].Rows.Add(nucRow);
                ln++;
            }
        }
        /// <summary>
        /// Get the precision of a duplicated line
        /// </summary>
        /// <param name="num">THe number to get the precision of</param>
        /// <returns>percision</returns>
        protected override double GetPrecision(double num)
        {
            return 1e-5;
        }

    }
}
