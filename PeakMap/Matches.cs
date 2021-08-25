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

namespace PeakMap
{
    //[DefaultPropertyAttribute("Name")]  
    public class Matches 
    {
        public enum PeakType { Photopeak, SingleEscape, DoubleEscape, Sum };

        readonly DataSet matches;
        SpectralData specData;

        double yeildLimit = 0.1;
        double scoreLimit = .001;
        double PDhalfLifeLimit = 1.0;
        double halfLifeScoreConstant = -0.0051;
        double lineDeviationContant = -0.5;
        double unmatchedLineConstant = 1;
        double sumPeakPenalty = 0.1;
        bool enableHalfLifeScore = true;
        readonly IDataLibrary lib;


        /// <summary>
        /// Gets or sets the Lower limit of photon yeild in the search
        /// </summary>
        [Category("Limit Settings"),
        DisplayName("Yield Limit"),
        Description("Lower limit of photon yield (%)"),
        DefaultValue(0.1)]
        public double YeildLimit { get { return yeildLimit; } set { yeildLimit = value; } }
        /// <summary>
        /// Gets or sets the lower limit of score
        /// </summary>
        [Category("Limit Settings"),
        DisplayName("Score Limit"),
        Description("Lower limit of the score"),
        DefaultValue(.001)]
        public double ScoreLimit { get { return scoreLimit; } set { scoreLimit = value; } }
        /// <summary>
        /// Gets or sets the ratio of parent to daughter half lives multiplier to exclude short-lived parents
        /// </summary>
        [Category("Limit Settings"),
        DisplayName("Parent/Daughter Half-Life Ratio"),
        Description("Ratio of parent to daughter half-lives multiplier to exclude short-lived parents"),
        DefaultValue(1.0)]
        public double PDHalfLifeRatio { get { return PDhalfLifeLimit; } set { PDhalfLifeLimit = value; } }
        /// <summary>
        /// Gets or sets the constant used in the half life scoring algorithm
        /// </summary>
        [Category("Algorithm Settings"),
        DisplayName("Half-Life Constant"),
        Description("Constant used in the half-life scoring algorithm"),
        DefaultValue(-0.0051)]
        public double HalfLifeScoreConstant { get { return halfLifeScoreConstant; } set { halfLifeScoreConstant = value; } }
        /// <summary>
        /// Gets or sets the constant used in the line Deviation scoring algorithm
        /// </summary>
        [Category("Algorithm Settings"),
        DisplayName("Line Deviation Constant"),
        Description("Constant used in the line Deviation scoring algorithm"),
        DefaultValue(-0.5)]
        public double LineDeviationContant { get { return lineDeviationContant; } set { lineDeviationContant = value; } }
        /// <summary>
        /// Gets or sets the constant used in the unmatched line scoring algorithm
        /// </summary>
        [Category("Algorithm Settings"),
        DisplayName("Unmatched Line Constant"),
        Description("Constant used in the unmatched line scoring algorithm"),
        DefaultValue(1.0)]
        public double UnmatchedLineConstant { get { return unmatchedLineConstant; } set { unmatchedLineConstant = value; } }
        /// <summary>
        /// Gets or sets wether to include half-life scoring in the algorithm
        /// </summary>
        [Category("Algorithm Settings"),
        DisplayName("Enable Half-Life Score"),
        Description("Enable half-life scoring algorithm"),
        DefaultValue(true)]
        public bool EnableHalfLifeScore { get { return enableHalfLifeScore; } set { enableHalfLifeScore = value; } }
        /// <summary>
        /// Gets or sets the sumPeakoenalty
        /// </summary>
        [Category("Algorithm Settings"),
        DisplayName("Sum Peak Penalty"),
        Description("Score penalty for not finding sum peak of the two largest peaks"),
        DefaultValue(true)]
        public double SumPeakPenalty { get { return sumPeakPenalty; } set { sumPeakPenalty = value; } }

        /// <summary>
        /// Constructor for LineMatches
        /// </summary>
        /// <param name="library">Libaray DataTable</param>
        public Matches(IDataLibrary library)
        {
            lib = library;
            matches = new DataSet();
            matches.ReadXmlSchema(Path.Combine(Environment.CurrentDirectory, "MatchesSchema.xsd"));
            matches.Tables["PEAKS"].RowChanged += new DataRowChangeEventHandler(DataTable_RowChanged);
        }

        /// <summary>
        /// Handle the row changed event on datatable
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void DataTable_RowChanged(object sender, DataRowChangeEventArgs e)
        {
            //score when the dataRow is added upon
            if (e.Action == DataRowAction.Add)
                sumPeakPenalty = ScoreSumPeaks();
            if (e.Action == DataRowAction.Change)
                sumPeakPenalty = ScoreSumPeaks();
        }

        /// <summary>
        /// Clear the data from the matches
        /// </summary>
        public void ClearMatches()
        {
            //ClearTenativeMatches();
            if (matches.Tables["MATCHEDNUCLIDES"] != null && matches.Tables["MATCHEDNUCLIDES"].Rows.Count > 0)
            {
                string sort = matches.Tables["MATCHEDNUCLIDES"].DefaultView.Sort;
                matches.Tables["MATCHEDNUCLIDES"].Clear();
                matches.Tables["MATCHEDNUCLIDES"].DefaultView.Sort = sort;
            }

            ClearLines();
        }
        /// <summary>
        /// Clear the Lines 
        /// </summary>
        public void ClearLines()
        {
            if (matches.Tables["MATCHEDLINES"] != null && matches.Tables["MATCHEDLINES"].Rows.Count > 0)
            {
                string sort = matches.Tables["MATCHEDLINES"].DefaultView.Sort;
                matches.Tables["MATCHEDLINES"].Clear();
                matches.Tables["MATCHEDLINES"].DefaultView.Sort = sort;
            }
        }
        /// <summary>
        /// Clear the lines a nuclide with the given name
        /// </summary>
        /// <param name="nuc">Name of a nuclide</param>
        public void ClearMatches(DataRow nuc)
        {
            //delete the nuclide
            if (nuc != null)
                nuc.Delete();
            //get and delete the lines
            DataRow[] lines = matches.Tables["MATCHEDLINES"].Select("NAME = '" + nuc["NAME"] + "'");
            foreach (DataRow line in lines)
                line.Delete();
            ClearTenativeMatches((string)nuc["NAME"]);
            //commit the deletions
            matches.AcceptChanges();
        }
        /// <summary>
        /// Clear the matches based on a name
        /// </summary>
        /// <param name="name">Name of the nuclide</param>
        public void ClearMatches(string name)
        {
            //get the the nuclid
            DataRow[] nucs = matches.Tables["MATCHEDNUCLIDES"].Select("NAME = '" + name + "'");
            foreach (DataRow nuc in nucs)
            {
                //get and delete the lines
                DataRow[] lines = matches.Tables["MATCHEDLINES"].Select("NAME = '" + nuc["NAME"] + "'");
                foreach (DataRow line in lines)
                    line.Delete();
                //mark for deletion
                nuc.Delete();
            }
            ClearTenativeMatches(name);
            //commit the deletions
            matches.AcceptChanges();
        }
        /// <summary>
        /// Clear all the data from the dataset
        /// </summary>
        public void Clear()
        {
            ClearTenativeMatches();
            ClearMatches();
            //matches.Clear();
            if (matches.Tables["PEAKS"] != null && matches.Tables["PEAKS"].Rows.Count > 0)
            {
                string sort = matches.Tables["PEAKS"].DefaultView.Sort;
                matches.Tables["PEAKS"].Clear();
                matches.Tables["PEAKS"].DefaultView.Sort = sort;
            }
            matches.Tables["PEAKS"].Columns["ID"].AutoIncrementSeed = -1;
            matches.Tables["PEAKS"].Columns["ID"].AutoIncrementStep = -1;

            matches.Tables["PEAKS"].Columns["ID"].AutoIncrementSeed = 0;
            matches.Tables["PEAKS"].Columns["ID"].AutoIncrementStep = 1;
            matches.AcceptChanges();
        }

        /// <summary>
        /// Set the Nuclide matches where the energy in the libary is +/- the tolerance
        /// </summary>
        /// <param name="energy">Energy of the line in keV</param>
        /// <param name="elapsed">elapsed time in days</param>
        /// <param name="tol">Energy tolerance</param>
        public void SetNuclides(double energy, TimeSpan elapsedWait, double tol = 1, PeakType type = PeakType.Photopeak)
        {

            if (lib == null)
                throw new NullReferenceException("Library is not defined");

            double tolerance = tol;
            //convert the time span to days
            double elapsed = elapsedWait.TotalDays;

            //cut down on redundant calls by only doing this the first time this method is called.
            if (type == PeakType.Photopeak)
            {
                //get escape peaks
                DataRow[] SE = MatchSE(energy, tolerance);

                //check if this is a single escape peak
                if (SE.Length > 0)
                {
                    foreach (DataRow SEPeak in SE)
                        SetNuclides((double)SEPeak["ENERGY"], elapsedWait, (double)SEPeak["FWHM"], PeakType.SingleEscape);
                }

                //check if this is a double escape peak
                DataRow[] DE = MatchDE(energy, tolerance);
                if (DE.Length > 0)
                {
                    foreach (DataRow DEPeak in DE)
                        SetNuclides((double)DEPeak["ENERGY"], elapsedWait, (double)DEPeak["FWHM"], PeakType.DoubleEscape);
                }
            }
            //check for sum peaks
            MatchRandom(energy, tolerance);

            //get the matched nuclides
            string expression = "ENERGY > " + (energy - tolerance).ToString() + " AND ENERGY < " + (energy + tolerance).ToString() + " AND YIELD > " + yeildLimit;
            DataTable tmatches = lib.Select("PHOTONS", expression);
            //DataTable tmatches = lib.Tables["PHOTONS"].Select(expression).CopyToDataTable();
            tmatches.Columns.Add("DIFFERENCE", typeof(double), energy + "- ENERGY");


            //generate a string to fill the MATCHEDNUCLIDES table
            StringBuilder nucList = new StringBuilder();
            foreach (DataRow row in tmatches.Rows)
                nucList.Append("'" + row["NAME"] + "',");

            nucList.Remove(nucList.Length - 1, 1);

            //get the data and fill the table
            DataTable nucs = lib.Select("NUCLIDES", "NAME IN (" + nucList.ToString() + ")");

            int id = matches.Tables["MATCHEDNUCLIDES"].Rows.Count;
            foreach (DataRow nuc in nucs.Rows)
            {
                double score = 1;
                //Get the basis line
                DataRow basis = tmatches.Select("NAME = '" + nuc["NAME"] + "'")[0];

                //if it is a single escape peak
                if (type == PeakType.SingleEscape)
                {
                    score *= (double)basis["YIELD"] / 100;
                    score *= ScoreHalfLife((double)nuc["HALF_LIFE"], (string)nuc["HALF_LIFE_UNIT"], elapsed);
                    score *= ScoreDeviation(basis, tolerance);
                    if (score < scoreLimit)
                        continue;
                    matches.Tables["MATCHEDNUCLIDES"].Rows.Add(id, "S.E.", 1 * score, nuc["NAME"], nuc["HALF_LIFE"], nuc["HALF_LIFE_UNIT"],
                     basis["ENERGY"], basis["YIELD"], type.ToString(), basis["DIFFERENCE"], nuc["BRANCHING"]);

                    id++;
                }
                //if it is a double escape peak
                else if (type == PeakType.DoubleEscape)
                {
                    score *= (double)basis["YIELD"] / 100;
                    score *= ScoreHalfLife((double)nuc["HALF_LIFE"], (string)nuc["HALF_LIFE_UNIT"], elapsed);
                    score *= ScoreDeviation(basis,  tolerance);
                    if (score < scoreLimit)
                        continue;
                    matches.Tables["MATCHEDNUCLIDES"].Rows.Add(id, "D.E.", 1 * score, nuc["NAME"], nuc["HALF_LIFE"], nuc["HALF_LIFE_UNIT"],
                     basis["ENERGY"], basis["YIELD"], type.ToString(), basis["DIFFERENCE"], nuc["BRANCHING"]);

                    id++;
                }
                //if it is a sum peak
                else if (type == PeakType.Sum)
                {
                    matches.Tables["MATCHEDNUCLIDES"].Rows.Add(id, "S.E.", 1 * score, nuc["NAME"], nuc["HALF_LIFE"], nuc["HALF_LIFE_UNIT"],
                    basis["ENERGY"], basis["YIELD"], basis["TYPE"], basis["DIFFERENCE"], nuc["BRANCHING"]);
                }
                //when there is no parent
                else if (nuc["PARENT"].Equals("NONE"))
                {
                    //include the yeild weight
                    score *= (double)basis["YIELD"] / 100;
                    score *= ScoreHalfLife((double)nuc["HALF_LIFE"], (string)nuc["HALF_LIFE_UNIT"], elapsed);
                    score *= ScoreDeviation(basis, tolerance);
                    if (score < scoreLimit)
                        continue;
                    matches.Tables["MATCHEDNUCLIDES"].Rows.Add(id, nuc["NAME"], 1 * score, nuc["NAME"], nuc["HALF_LIFE"], nuc["HALF_LIFE_UNIT"],
                        basis["ENERGY"], basis["YIELD"], basis["TYPE"], basis["DIFFERENCE"], nuc["BRANCHING"]);

                    id++;
                }
                //add a decay chain if there is one
                else if (nuc["PARENT"].ToString().Contains('+'))
                {
                    //get the first nuclide
                    DataTable topNucs = lib.Select("NUCLIDES", "NAME =  '" + nuc["PARENT"].ToString().Trim('+')
                        + "' AND PARENT = '" + nuc["PARENT"].ToString() + "'");

                    if (topNucs == null || topNucs.Rows.Count < 1)
                        continue;
                    //it's alwasys the first row
                    DataRow topNuc = topNucs.Rows[0];
                    score *= ScoreHalfLife((double)topNuc["HALF_LIFE"], (string)topNuc["HALF_LIFE_UNIT"], elapsed);
                    score *= ScoreDeviation(basis, tolerance);
                    basis["YIELD"] = (double)basis["YIELD"] * (double)nuc["BRANCHING"];
                    score *= (double)basis["YIELD"] / 100;
                    if (score < scoreLimit)
                        continue;

                    matches.Tables["MATCHEDNUCLIDES"].Rows.Add(id, topNuc["PARENT"], 1 * score, nuc["NAME"], topNuc["HALF_LIFE"], topNuc["HALF_LIFE_UNIT"],
                          basis["ENERGY"], basis["YIELD"], basis["TYPE"], basis["DIFFERENCE"], nuc["BRANCHING"]);
                    id++;
                }
                //add the parent if there is one
                else
                {
                    //only add parents who's daughters have shorter half lives
                    double parentHalfLife = ICRPData.ConvertHalfLifeToDays((string)nuc["PARENT_HALF_LIFE_UNIT"], (double)nuc["PARENT_HALF_LIFE"]);
                    double daughterHalfLife = ICRPData.ConvertHalfLifeToDays((string)nuc["HALF_LIFE_UNIT"], (double)nuc["HALF_LIFE"]);

                    //skip parents with shorter half lives and already existing rows
                    if (PDhalfLifeLimit * parentHalfLife > daughterHalfLife)
                    {
                        //include the yeild weight
                        score *= (double)basis["YIELD"] / 100;
                        score *= ScoreHalfLife((double)nuc["PARENT_HALF_LIFE"], (string)nuc["PARENT_HALF_LIFE_UNIT"], elapsed);
                        score *= ScoreDeviation(basis, tolerance);
                        if (score < scoreLimit)
                            continue;
                        //serarch for repeats
                        if (!matches.Tables["MATCHEDNUCLIDES"].AsEnumerable().Any(row => (string)nuc["NAME"] == row.Field<string>("NUCLIDE")
                                                                                      && (double)nuc["HALF_LIFE"] == row.Field<double>("HALF_LIFE")
                                                                                      && (string)nuc["HALF_LIFE_UNIT"] == row.Field<string>("HALF_LIFE_UNIT")))
                        {
                            matches.Tables["MATCHEDNUCLIDES"].Rows.Add(id, nuc["NAME"], 1 * score, nuc["NAME"], nuc["HALF_LIFE"], nuc["HALF_LIFE_UNIT"],
                                basis["ENERGY"], basis["YIELD"], basis["TYPE"], basis["DIFFERENCE"], nuc["BRANCHING"]);
                            id++;
                        }
                        matches.Tables["MATCHEDNUCLIDES"].Rows.Add(id, nuc["PARENT"], 1 * score, nuc["NAME"], nuc["PARENT_HALF_LIFE"], nuc["PARENT_HALF_LIFE_UNIT"],
                            basis["ENERGY"], (double)basis["YIELD"] * (double)nuc["BRANCHING"], basis["TYPE"], basis["DIFFERENCE"], nuc["BRANCHING"]);
                        id++;
                    }
                    //add just the match avoiding repeats
                    else if (!matches.Tables["MATCHEDNUCLIDES"].AsEnumerable().Any(row => (string)nuc["NAME"] == row.Field<string>("NAME")))
                    {
                        score *= (double)basis["YIELD"] / 100;
                        score *= ScoreHalfLife((double)nuc["HALF_LIFE"], (string)nuc["HALF_LIFE_UNIT"], elapsed);
                        score *= ScoreDeviation(basis, tolerance);
                        if (score < scoreLimit)
                            continue;

                        matches.Tables["MATCHEDNUCLIDES"].Rows.Add(id, nuc["NAME"], 1 * score, nuc["NAME"], nuc["HALF_LIFE"], nuc["HALF_LIFE_UNIT"],
                            basis["ENERGY"], basis["YIELD"], basis["TYPE"], basis["DIFFERENCE"], nuc["BRANCHING"]);
                        id++;
                    }
                }
            }
        }

        /// <summary>
        /// Sets the lines with an assoicated nuclide
        /// </summary>
        /// <param name="nuc">the nuclide</param>
        /// <param name="Peak">the peak </param>
        public void SetLines(DataRow nuc, DataRow peak)
        {
            if (lib == null)
                throw new NullReferenceException("Library is not defined");
            if (specData == null)
                throw new NullReferenceException("Spectal Data is not defined");



            double area = double.TryParse(peak["AREA"].ToString(), out area) ? area : 0.0;
            double mda = double.TryParse(peak["CRITLEVEL"].ToString(), out mda) ? 2.71 + 2*mda : 0.0;

            //matches.Tables["MATCHEDLINES"].Clear();
            ClearLines();
            //get the daughters line and its daughters.
            Dictionary<string, double> daughters = lib.GetDaughters((string)nuc["NAME"]);
            //generate a string for the nuclides incuding the daughters 
            StringBuilder nucList = new StringBuilder();
            nucList.Append("'" + nuc["NAME"] + "',");
            foreach (string key in daughters.Keys)
            {
                nucList.Append("'" + key + "',");
            }
            nucList.Remove(nucList.Length - 1, 1);

            //get the lines for the nucs
            DataTable lines = lib.Select("PHOTONS", "NAME IN (" + nucList.ToString() + ") AND YIELD > '" + yeildLimit + "'");
            foreach (DataRow line in lines.Rows)
            {

                double eff = specData.GetEfficiency((double)line["ENERGY"]);

                //get the yelds of the daughters.
                double yieldMult = (daughters.TryGetValue((string)line["NAME"], out double branchRatio)) ? branchRatio : 1;

                //double yieldMult = isParent ? (double)nuc["BRANCHING"] : 1;
                //get the basis line
                if (Math.Abs((double)line["ENERGY"] - (double)nuc["ENERGY"]) < 1e-6)
                {
                    matches.Tables["MATCHEDLINES"].Rows.Add(line["NAME"], line["LINENUMBER"], line["ENERGY"], yieldMult * (double)line["YIELD"], line["TYPE"],
                        eff, area, mda, true, (int)peak["ID"], nuc["DIFFERENCE"]);
                }
                else
                {
                    //reject anything that falls below the yield limit
                    double y2 = (double)line["YIELD"] * yieldMult;
                    if (y2 < yeildLimit)
                        continue;
                    //DataRow basis = temp.FirstOrDefault(c => c["NAME"].Equals(line["NAME"]));
                    double e2 = specData.GetEfficiency((double)nuc["ENERGY"]);
                    //set the MDA's it is possible this fails so throw it in a try catch
                    double m2 = 2.71;
                    try
                    {
                        m2 = 2.71 + 3.29*Math.Sqrt(specData.GetRegion(double.Parse(line["ENERGY"].ToString())));
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine(ex.Message+":\n"+ex.StackTrace);
                    }
                    //double e1 = CAMData.Efficiency(float.Parse(nuc["ENERGY"].ToString()), calParameters);
                    double y1 = (double)nuc["YIELD"];

                    matches.Tables["MATCHEDLINES"].Rows.Add(line["NAME"], line["LINENUMBER"], line["ENERGY"], y2, line["TYPE"], eff, area * e2 * y2 / (eff * y1), m2);

                }
            }
        }

        /// <summary>
        /// Gets the nuclides similar to the provided name
        /// </summary>
        /// <param name="name"></param>
        /// <param name="id"></param>
        /// <param name="exactName"></param>
        public void SetNuclides(string name, int id = 0,bool exactName = false)
        {
            //matches.Tables["MATCHEDNUCLIDES"].Clear();
            ClearMatches();
            //convert the nuclide name to ensure it is good. 
            name = exactName ? name : lib.GetNuclideName(name);

            if (String.IsNullOrEmpty(name))
                throw new ArgumentException("A nuclide must be entered");
            DataTable nucs;
            if (!exactName)
                nucs = lib.Select("NUCLIDES", "NAME LIKE '%" + name + "%' OR PARENT LIKE '%" + name + "%'");
            else
                nucs = lib.Select("NUCLIDES", "NAME = '" + name.Trim() + "'");

            foreach (DataRow nuc in nucs.Rows)
            {
                double score = 1;
                //DataRow key = lib.Select("MAX(YIELD) FROM PHOTONS WHERE NAME = '" + nuc["NAME"] +"'").Rows[0];
                //whene there is no parent
                if (nuc["PARENT"].Equals("NONE"))
                {
                    //include the yeild weight
                    matches.Tables["MATCHEDNUCLIDES"].Rows.Add(id, nuc["NAME"], 1 * score, nuc["NAME"], nuc["HALF_LIFE"], nuc["HALF_LIFE_UNIT"]);
                    //key["ENERGY"], key["YIELD"], key["TYPE"], key["DIFFERENCE"], nuc["BRANCHING"]);

                    id++;
                }
                //add the parent if there is one
                else
                {
                    //check for repeats
                    bool repeatNuc = matches.Tables["MATCHEDNUCLIDES"].AsEnumerable().Any(row => (string)nuc["NAME"] == row.Field<string>("NAME"));
                    bool repeatParent = matches.Tables["MATCHEDNUCLIDES"].AsEnumerable().Any(row => (string)nuc["PARENT"] == row.Field<string>("NAME"));
                    bool repeat = matches.Tables["MATCHEDNUCLIDES"].AsEnumerable().Any(row => (string)nuc["NAME"] == row.Field<string>("NAME") &&
                                                                                              (double)nuc["HALF_LIFE"] == row.Field<double>("HALF_LIFE") &&
                                                                                              (string)nuc["HALF_LIFE_UNIT"] == row.Field<string>("HALF_LIFE_UNIT"));
                    //only add parents who's daughters have shorter half lives
                    double parentHalfLife = ICRPData.ConvertHalfLifeToDays((string)nuc["PARENT_HALF_LIFE_UNIT"], (double)nuc["PARENT_HALF_LIFE"]);
                    double daughterHalfLife = ICRPData.ConvertHalfLifeToDays((string)nuc["HALF_LIFE_UNIT"], (double)nuc["HALF_LIFE"]);

                    if (PDhalfLifeLimit * parentHalfLife > daughterHalfLife)
                    {
                        string nucName = nuc["NAME"].ToString();
                        //check for repeats
                        if (!repeat)
                        {
                            matches.Tables["MATCHEDNUCLIDES"].Rows.Add(id, nucName, 1 * score, nuc["NAME"], nuc["HALF_LIFE"], nuc["HALF_LIFE_UNIT"]);
                            id++;
                        }
                        if (!repeatParent)
                        {
                            matches.Tables["MATCHEDNUCLIDES"].Rows.Add(id, nuc["PARENT"], 1 * score, nuc["NAME"], nuc["PARENT_HALF_LIFE"], nuc["PARENT_HALF_LIFE_UNIT"]);
                        }
                        id++;

                    }
                    //add the others
                    else if (!repeatNuc)
                    {
                        //also add the daughter
                        matches.Tables["MATCHEDNUCLIDES"].Rows.Add(id, nuc["NAME"], 1 * score, nuc["NAME"], nuc["HALF_LIFE"], nuc["HALF_LIFE_UNIT"]);
                        id++;

                    }

                }
            }
        }

        /// <summary>
        ///  Sets the lines with an assoicated nuclide
        /// </summary>
        /// <param name="nuc"></param>
        public void SetLines(DataRow nuc)
        {
            if (lib == null)
                throw new NullReferenceException("Library is not defined");

            if (nuc["NAME"].GetType() == typeof(DBNull))
                return;

            matches.Tables["MATCHEDLINES"].Clear();
            //get the daughters line and its daughters.
            Dictionary<string, double> daughters = lib.GetDaughters((string)nuc["NAME"]);
            //generate a string for the nuclides incuding the daughters 
            StringBuilder nucList = new StringBuilder();
            nucList.Append("'" + nuc["NAME"] + "',");
            //nucList.Append("'" + nuc["NUCLIDE"] + "',");
            foreach (string key in daughters.Keys)
            {
                nucList.Append("'" + key + "',");
            }
            nucList.Remove(nucList.Length - 1, 1);


            //get the lines for the nucs
            //DataRow[] lines = lib.Tables["PHOTONS"].Select("NAME IN (" + nucList.ToString() + ") AND YIELD > '" + yeildLimit + "'");
            DataTable lines = lib.Select("PHOTONS", "NAME IN (" + nucList.ToString() + ") AND YIELD > '" + yeildLimit + "'");
            foreach (DataRow line in lines.Rows)
            {

                //get the yelds of the daughters.
                double yieldMult = (daughters.TryGetValue((string)line["NAME"], out double branchRatio)) ? branchRatio : 1;


                //reject anything that falls below the yield limit
                double y2 = (double)line["YIELD"] * yieldMult;
                if (y2 < yeildLimit)
                    continue;

                matches.Tables["MATCHEDLINES"].Rows.Add(line["NAME"], line["LINENUMBER"], line["ENERGY"], y2, line["TYPE"]);//, eff, area * e2 * y2 / (eff * y1), m2);


            }
        }

        /// <summary>
        /// Get the peak that matches an associated line
        /// </summary>
        /// <param name="line"></param>
        /// <param name="tolerance"></param>
        public DataRow[] GetPeakMatches(DataRow line, double tolerance = 1)
        {
            DataRow[] foundPeaks = matches.Tables["Peaks"].Select("ENERGY" + "  > " + ((double)line["ENERGY"] - tolerance) + " AND " + "ENERGY" + "  < " + ((double)line["ENERGY"] + tolerance));
            if (foundPeaks != null && foundPeaks.Length != 0)
            {
                line["MATCHED"] = true;
                line["PEAKNUM"] = foundPeaks[0]["ID"];
                line["AREA"] = foundPeaks[0]["AREA"];
                line["DIFFERENCE"] = double.Parse(foundPeaks[0]["ENERGY"].ToString()) - (double)line["ENERGY"];
            }
            return foundPeaks;
        }

        /// <summary>
        /// Set the peak that matches an associated line
        /// </summary>
        /// <param name="line">Line to find peaks</param>
        /// <param name="MatchName">Name of the match</param>
        /// <param name="tolerance">energy tolerance</param>
        /// <param name="tenative">indicates if the match is tenative</param>
        public void SetPeakMatches(DataRow line, string matchName, double tolerance = 1, bool tenative=true)
        {
            if (!line.Table.Columns.Contains("MATCHED"))
                throw new ArgumentException("The DataRow does not match the required schema");

            //get the peaks that matche the line
            DataRow[] foundPeaks = matches.Tables["Peaks"].Select("ENERGY" + "  > " + ((double)line["ENERGY"] - tolerance) + " AND " + "ENERGY" + "  < " + ((double)line["ENERGY"] + tolerance));
            //set the line info on the peak
            if (foundPeaks != null && foundPeaks.Length != 0)
            {
                line["MATCHED"] = true;
                line["PEAKNUM"] = foundPeaks[0]["ID"];
                line["AREA"] = foundPeaks[0]["AREA"];
                line["DIFFERENCE"] = double.Parse(foundPeaks[0]["ENERGY"].ToString()) - (double)line["ENERGY"];
            }
            //set the tenativematch flag
            foreach (DataRow peak in foundPeaks)
            {
                //add only the matchname only if the match is null
                if (peak["MATCHNAME"] == DBNull.Value)
                {
                    peak["MATCHNAME"] = matchName;
                    peak["TENTATIVEMATCH"] = tenative;
                }
                //avoid duplicates
                else if (!peak["MATCHNAME"].ToString().Contains(matchName))
                {
                    peak["MATCHNAME"] = ((string)peak["MATCHNAME"]).Trim() + "," + matchName;
                    peak["TENTATIVEMATCH"] = tenative;
                }
            }
        }
        /// <summary>
        /// Clear the tenative matches of a given nuclide
        /// <param name="name">Nuclide to clear tenative match</param>
        /// </summary>
        public void ClearTenativeMatches(string name) 
        {
            //clear out the old tenative matches
            DataRow[] oldMatches = matches.Tables["Peaks"].Select("TENTATIVEMATCH = true");
            foreach (DataRow peak in oldMatches)
            {
                peak["TENTATIVEMATCH"] = false;
                //remove the only the tenative nuclide
                //peak["MATCHNAME"] = peak["MATCHNAME"].ToString().Replace(name,"");
                string matchName = peak["MATCHNAME"].ToString();
                if (matchName.EndsWith(name))
                {
                    peak["MATCHNAME"] = matchName.Remove(matchName.Length - name.Length);
                }

                peak["MATCHNAME"] = peak["MATCHNAME"].ToString().TrimEnd(',');
                //if we have cleared out all the nucldes then set it to DBNull
                if (peak["MATCHNAME"].ToString() == "")
                    peak["MATCHNAME"] = DBNull.Value;
            }

        }
        /// <summary>
        /// Clear the tenative matches
        /// </summary>
        public void ClearTenativeMatches() 
        {
            //clear out the old tenative matches
            DataRow[] oldMatches = matches.Tables["Peaks"].Select("TENTATIVEMATCH = true");
            foreach (DataRow peak in oldMatches)
            {
                peak["TENTATIVEMATCH"] = false;
                //remove the only the tenative nuclide
                peak["MATCHNAME"] = DBNull.Value;
            }
        }
        /// <summary>
        /// Set the Matches to persist (i.e. MATCHNAME != DBnull and TENATIVEMATCH = false;
        /// </summary>
        public void SetPersistentMatches() 
        {
            DataRow[] oldMatches = matches.Tables["Peaks"].Select("TENTATIVEMATCH = true");
            foreach (DataRow peak in oldMatches)
                peak["TENTATIVEMATCH"] = false;
        }
        /// <summary>
        /// Clear all the persistant matches
        /// </summary>
        public void ClearPersistentMatches() 
        {
            foreach(DataRow peak in Peaks.Rows) 
            {
                ClearPersistentMatch(peak);
            }
        }
        /// <summary>
        /// Clear a persistent Match
        /// </summary>
        /// <param name="peak">The peak to cear the match</param>
        /// <param name="matchName">The matchName to clear</param>
        public void ClearPersistentMatch(DataRow peak, string matchName) 
        {
            if (!peak.Table.Columns.Contains("FWHM"))
                throw new ArgumentException("The DataRow does not match the required schema");

            peak["MATCHNAME"] = peak["MATCHNAME"] == DBNull.Value || String.IsNullOrEmpty((string)peak["MATCHNAME"]) ?
                "" : (string)peak["MATCHNAME"].ToString().Replace(matchName,"");

            peak["MATCHNAME"] = peak["MATCHNAME"].ToString().TrimEnd(',');
            peak["MATCHNAME"] = peak["MATCHNAME"].ToString().TrimStart(',');
            //ensure empty values are DBNull.Value
            if (String.IsNullOrEmpty((string)peak["MATCHNAME"]))
                peak["MATCHNAME"] = DBNull.Value;

        }
        /// <summary>
        /// Clear the peak of all persistent matches
        /// </summary>
        /// <param name="peak">The peak to cear the match</param>
        public void ClearPersistentMatch(DataRow peak)
        {
            if (!peak.Table.Columns.Contains("FWHM"))
                throw new ArgumentException("The DataRow does not match the required schema");

            peak["MATCHNAME"] = DBNull.Value;

        }
        /// <summary>
        /// Find the peaks that could result in a single escape peak at the entered energy
        /// </summary>
        /// <param name="energy">Energy of the possible S.E. peak</param>
        /// <param name="tolerance">Search tolerance</param>
        /// <returns>array of lines rows that could be the parent</returns>
        public DataRow[] MatchSE(double energy, double tolerance = 1.0)
        {
            //get the expected energy of the parent nuclide
            double expEnergy = energy + 510.99895;
            DataRow[] possible;
            //check if it is possible to have pair prodution
            if (expEnergy > 1021.9979)
            {
                //if so get the peaks
                string expression = "ENERGY > " + (expEnergy - tolerance).ToString() + " AND ENERGY < " + (expEnergy + tolerance).ToString();
                possible = matches.Tables["PEAKS"].Select(expression);
            }
            else
                possible = new DataRow[0];

            return possible;
        }

        /// <summary>
        /// Find the peaks that could result in a double escape peak at the entered energy
        /// </summary>
        /// <param name="energy">Energy of the possible D.E. peak</param>
        /// <param name="tolerance">Search tolerance</param>
        /// <returns>array of lines rows that could be the parent</returns>
        public DataRow[] MatchDE(double energy, double tolerance = 1.0)
        {
            //get the expected energy of the parent nuclide
            double expEnergy = energy + 1021.9979;
            DataRow[] possible;
            //check if it is possible to have pair prodution
            if (expEnergy > 1021.9979)
            {
                //if so get the matching peak
                string expression = "ENERGY > " + (expEnergy - tolerance).ToString() + " AND ENERGY < " + (expEnergy + tolerance).ToString();
                possible = matches.Tables["PEAKS"].Select(expression);
            }
            else
                possible = new DataRow[0];

            return possible;
        }

        /// <summary>
        /// Matches random summing
        /// </summary>
        /// <param name="energy"></param>
        /// <param name="tolerance"></param>
        public void MatchRandom(double energy, double tolerance = 1.0)
        {
            //cannot have sum peaks if there are less than three
            if (matches.Tables["PEAKS"].Rows.Count < 3)
                return;

            //get all the peaks below the energy
            DataRow[] suspectedPeaks = matches.Tables["PEAKS"].Select("ENERGY < " + energy);
            if (suspectedPeaks.Length < 1)
                return;

            DataTable peaks = suspectedPeaks.CopyToDataTable();

            DataRow[] result;
            //loop through the peaks
            for (int i = 0; i < peaks.Rows.Count - 1; i++)
            {
                double estEnergy = (double)peaks.Rows[i]["ENERGY"];
                //get the possible peaks that can result in the sum
                result = (from row in peaks.AsEnumerable()
                          where
                              (row.Field<double>("ENERGY") + estEnergy) >= (energy - tolerance) &&
                              (row.Field<double>("ENERGY") + estEnergy) <= (energy + tolerance)
                          select row).ToArray();
                //add the information to the resutls
                foreach (DataRow sumPeak in result)
                {
                    //score the Deviation 
                    double score = ScoreDeviation(estEnergy + (double)sumPeak["ENERGY"], energy, tolerance, lineDeviationContant);
                    int id = matches.Tables["MATCHEDNUCLIDES"].Rows.Count;
                    //add a sum peak
                    DataRow row = matches.Tables["MATCHEDNUCLIDES"].Rows.Add(id, "Sum (" + estEnergy.ToString("f0") + "," + ((double)sumPeak["ENERGY"]).ToString("f0") + ")", 0.5 * sumPeakPenalty * score);
                    row["DIFFERENCE"] = energy - (estEnergy + (double)sumPeak["ENERGY"]);
                    row["TYPE"] = PeakType.Sum.ToString();
                }
            }

        }
        #region scoring
        /// <summary>
        /// score all the unmatched lines
        /// </summary>
        /// <param name="constant"></param>
        public double ScoreLineMatch(double constant = 1)
        {

            double num = 0.0; double denom = 0.0;
            //loop throuhg the all the lines
            foreach (DataRow line in matches.Tables["MATCHEDLINES"].Rows)
            {
                
                int delta = 1;
                //the numerator is only the matched lines
                if ((bool)line["MATCHED"])
                    num += (bool)line["MATCHED"] ? (double)line["YIELD"] * Math.Sqrt((double)line["EFFICIENCY"]) : 0;
                //determine if the not matched line is above MDA
                else
                    delta = (double)line["AREA"] > (double)line["MDA"] ? 1 : 0;
                //numerator is all the lines of a nuclide
                denom += (double)line["YIELD"] * Math.Sqrt((double)line["EFFICIENCY"]) * delta;
            }
            return constant * num / denom;
        }

        /// <summary>
        /// Apply the energy diffenertal scoring. 
        /// </summary>
        /// <param name="nuclideLine">Data row from results</param>
        /// <param name="energy"></param>
        /// <param name="tolerance"></param>
        /// <param name="constant"></param>
        public double ScoreDeviation(DataRow nuclideLine, double tolerance, double constant = -2)
        {

            if (double.TryParse(nuclideLine["DIFFERENCE"].ToString(), out double diff) && double.TryParse(nuclideLine["YIELD"].ToString(), out double yield))
                return Math.Exp(constant * (tolerance * tolerance) * (diff * diff) * yield / 100);
            else
                throw new Exception("Nuclide line input is not correctly formatted");
        }
        /// <summary>
        /// Apply peak energy differentail scoring
        /// </summary>
        /// <param name="peakEnergy">energy of the line</param>
        /// <param name="energy">"known" energy</param>
        /// <param name="tolerance">peak search tolerance</param>
        /// <param name="constant">Scaling constant</param>
        /// <returns></returns>
        public double ScoreDeviation(double peakEnergy, double energy, double tolerance, double constant = -2)
        {
            double diff = peakEnergy - energy;

            return Math.Exp(constant * (tolerance * tolerance) * (diff * diff) / 100);

        }

        /// <summary>
        /// Score the half life
        /// </summary>
        /// <param name="t12">Half Life</param>
        /// <param name="unit">Half Life unt</param>
        /// <param name="deltaT">Time epasped, in days, from sample to count</param>
        /// <param name="constant"></param>
        /// <returns></returns>
        private double ScoreHalfLife(double t12, string unit, double deltaT, double constant = -0.0051)
        {
            if (!enableHalfLifeScore)
                return 1.0;

            //convert the half lives
            double convert = 1.0;
            switch (unit)
            {
                case "ms":
                    convert = 1.15714E-8;
                    break;
                case "s":
                    convert = 1.15714E-5;
                    break;
                case "m":
                    convert = 6.94444e-4;
                    break;
                case "h":
                    convert = 4.16667e-2;
                    break;
                case "d":
                    convert = 1.0;
                    break;
                case "y":
                    convert = 365;
                    break;
            }
            t12 *= convert;
            return Math.Exp(constant * (deltaT / t12) * (deltaT / t12));
        }
        /// <summary>
        /// Look for the largest sum peak and calcuate the penalty
        /// </summary>
        /// <param name="tolerance"></param>
        /// <returns></returns>
        private double ScoreSumPeaks(double tolerance = 1.0)
        {
            //got to have at least 3 peaks to have a sum peak
            if (matches.Tables["PEAKS"].Rows.Count < 3)
                return sumPeakPenalty;
            DataRow[] t_peaksArr = matches.Tables["PEAKS"].Select("AREA > 0", "AREA DESC");
            if (t_peaksArr.Length < 2)
                return sumPeakPenalty;
            DataTable t_peaks = t_peaksArr.CopyToDataTable();

            //Get the 2 largest peaks
            double expSum = (double)t_peaks.Rows[0]["ENERGY"] + (double)t_peaks.Rows[1]["ENERGY"];
            if (t_peaks.Select("ENERGY > " + (expSum - tolerance).ToString() + " AND ENERGY < " + (expSum - tolerance).ToString()).Length > 0)
                return 1;
            else
                return sumPeakPenalty;

        }
        #endregion
        //set the unbrowsable parameters
        [Browsable(false)]
        public SpectralData SpecData { set { specData = value; } }
        [Browsable(false)]
        public DataTable Nuclides { get { return matches.Tables["MATCHEDNUCLIDES"]; } }
        [Browsable(false)]
        public DataTable Lines { get { return matches.Tables["MATCHEDLINES"]; } }
        [Browsable(false)]
        public DataTable Peaks { get { return matches.Tables["PEAKS"]; } }
    }
}
