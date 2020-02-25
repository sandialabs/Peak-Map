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
using System.Data.SQLite;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PeakMap
{
    public class ICRPData : IDataLibrary
    {

        DataSet library;
        /// <summary>
        /// Constructor
        /// </summary>
        public ICRPData()
        {
            //initialize the dataset 
            //library = new DataSet();
            //library.ReadXmlSchema(Path.Combine(Environment.CurrentDirectory, "ICRPLibrary.xsd"));
        }
        /// <summary>
        /// Initialize global data objects
        /// </summary>
        public async Task InitializeAsync()
        {
            library = new DataSet();
            library.ReadXmlSchema(Path.Combine(Environment.CurrentDirectory, "ICRPLibrary.xsd"));
            //read the data from the ICRP files
            await Task.Run(() => ReadChains());
            await Task.Run(() => ReadLibrary());

            string[] chainParents = Properties.Settings.Default.CHAINPARENT.ToString().Split(',');
            foreach (string parent in chainParents)
            {
                GetDecayChain(parent.ToUpper());
            }

            Console.WriteLine("Library Loaded");
        }

        /// <summary>
        /// Read the Library to get nuclides and lines
        /// </summary>
        private void ReadLibrary()
        {
            //get the file name of the ICRP library from the settings
            string filename = Properties.Settings.Default.LIBRARY.ToString();
            using (FileStream fs = File.OpenRead(filename))
            {
                using (StreamReader st = new StreamReader(fs))
                {
                    //read the data
                    string line;
                    string name = "";
                    int i = 0;
                    while ((line = st.ReadLine()) != null)
                    {
                        int n;
                        //nuclides
                        if (!(int.TryParse(line.Substring(0, 1), out n) || line.Substring(0, 1).Equals(" ")))
                        {
                            i = 0;
                            //DataRow row = library.Tables["NUCLIDES"].NewRow();
                            name = Regex.Match(line, @"[A-Za-z]*-\d*[a-z]?").Value.ToUpper();
                            //get the parent for the nuclide
                            DataRow[] parents = GetParent(name);
                            double halfLife = double.Parse(Regex.Match(line, @"(?<=\s)[\d]+\.?([\d]+)?(E.{1})?([\d]{1,2})?(?=[a-z])").Value);
                            string halfLifeUnit = Regex.Match(line, @"(?<!-[\d]+)(?<=[\d])[a-z]+|(?<=[\d]\.[\d]{1,5}[A-Z]-[\d]{1,2})[a-z]").Value;
                            //if there are no parents
                            if (parents == null)
                            {
                                DataRow row = library.Tables["NUCLIDES"].NewRow();
                                row["NAME"] = name;
                                row["HALF_LIFE"] = halfLife;
                                row["HALF_LIFE_UNIT"] = halfLifeUnit;
                                try
                                {
                                    library.Tables["NUCLIDES"].Rows.Add(row);
                                }
                                catch { continue; }
                                continue;
                            }
                            //add the parents
                            foreach (DataRow parent in parents)
                            {
                                DataRow row = library.Tables["NUCLIDES"].NewRow();
                                row["NAME"] = name;
                                row["HALF_LIFE"] = halfLife;
                                row["HALF_LIFE_UNIT"] = halfLifeUnit;
                                row["PARENT"] = parent["PARENT"];
                                row["PARENT_HALF_LIFE"] = parent["PARENT_HALF_LIFE"];
                                row["PARENT_HALF_LIFE_UNIT"] = parent["PARENT_HALF_LIFE_UNIT"];
                                //get the branching ratio
                                if (name.Equals(parent["DAUGHTER1"]))
                                    row["BRANCHING"] = parent["PROBABILITY1"];
                                else if (name.Equals(parent["DAUGHTER2"]))
                                    row["BRANCHING"] = parent["PROBABILITY2"];
                                else if (name.Equals(parent["DAUGHTER3"]))
                                    row["BRANCHING"] = parent["PROBABILITY3"];
                                try
                                {
                                    library.Tables["NUCLIDES"].Rows.Add(row);
                                }
                                catch { continue; }
                            }
                        }
                        //Lines
                        else if (line.Substring(0, 2).Equals(" 1") || line.Substring(0, 2).Equals(" 2"))
                        {
                            i++;
                            DataRow row = library.Tables["PHOTONS"].NewRow();
                            row["NAME"] = name;
                            MatchCollection matches = Regex.Matches(line, @"[-+]?[0-9]*\.?[0-9]+([eE][-+]?[0-9]+)");
                            row["ENERGY"] = double.Parse(matches[1].Value) * 1000;
                            row["YIELD"] = double.Parse(matches[0].Value) * 100;
                            row["TYPE"] = Regex.Match(line, @"[GX]").Value;
                            row["LINENUMBER"] = i;
                            try
                            {
                                if ((double)row["ENERGY"] > Properties.Settings.Default.LOWERELIMT && (double)row["ENERGY"] < Properties.Settings.Default.UPPERELIMIT && (double)row["YIELD"] > Properties.Settings.Default.LOWERYIELD)
                                    library.Tables["PHOTONS"].Rows.Add(row);
                            }
                            catch { continue; }
                        }

                    }
                }
            }
        }

        /// <summary>
        /// Read the decay chains 
        /// </summary>
        private void ReadChains()
        {
            string filename = Properties.Settings.Default.CHAINS.ToString();
            using (FileStream fs = File.OpenRead(filename))
            {
                using (StreamReader st = new StreamReader(fs))
                {
                    //read the data
                    string line;
                    int n = 0;
                    while ((line = st.ReadLine()) != null)
                    {
                        //skip the title row
                        if (n == 0)
                        {
                            n++;
                            continue;
                        }
                        DataRow row = library.Tables["DECAYCHAINS"].NewRow();
                        row["PARENT"] = line.Substring(0, 7).Trim().ToUpper();
                        row["PARENT_HALF_LIFE"] = line.Substring(7, 8).Trim();
                        row["PARENT_HALF_LIFE_UNIT"] = line.Substring(15, 1).Trim();
                        row["DAUGHTER1"] = line.Substring(53, 8).Trim().ToUpper();
                        row["PROBABILITY1"] = double.Parse(line.Substring(67, 10).Trim());
                        row["DAUGHTER1_STABLE"] = line.Substring(61, 5).Trim().Equals("0");
                        row["DAUGHTER2"] = line.Substring(78, 8).Trim().ToUpper();
                        row["PROBABILITY2"] = double.Parse(line.Substring(92, 10).Trim());
                        row["DAUGHTER2_STABLE"] = line.Substring(87, 5).Trim().Equals("0");
                        row["DAUGHTER3"] = line.Substring(103, 8).Trim().ToUpper();
                        row["PROBABILITY3"] = double.Parse(line.Substring(117, 10).Trim());
                        row["DAUGHTER3_STABLE"] = line.Substring(112, 5).Trim().Equals("0");
                        library.Tables["DECAYCHAINS"].Rows.Add(row);
                        n++;
                    }

                }
            }
        }
        /// <summary>
        /// Determines if a nuclide is part of a decay chain
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public bool IsPartOfChain(string name)
        {
            DataTable foundRows = this.Select("DECAYCHAINS", "PARENT = '" + name + "' OR DAUGHTER1 = '" + name + "' OR DAUGHTER2 = '" + name + "'");
            return foundRows.Rows.Count > 0;
        }
        /// <summary>
        /// get the deacy chains given a parent
        /// </summary>
        /// <param name="parents"></param>
        public void GetDecayChain(string parent)
        {
            string chainName = parent + "+";
            DataRow[] topRow = library.Tables["NUCLIDES"].Select("NAME = '" + parent + "'");
            if (topRow.Length < 1)
                return;

            GetDaughters(chainName, library.Tables["NUCLIDES"], topRow[0]);

        }

        /// <summary>
        /// Get the parent of the nuclide by name
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        private DataRow[] GetParent(string name)
        {
            DataRow[] parents = library.Tables["DECAYCHAINS"].Select("DAUGHTER1 = '" + name +
                "' OR DAUGHTER2 = '" + name +
                "' OR DAUGHTER3 = '" + name + "'");
            if (parents.Length > 0)
                return parents;
            //return parents.Select(r => r.Field<string>("PARENT")).ToArray();
            else
                return null;
        }
        /// <summary>
        /// Gets the daughters for the parent
        /// </summary>
        /// <param name="parent">Name of the parent</param>
        /// <returns>Dictionary of daughter and branching ratio</returns>
        public Dictionary<string, double> GetDaughters(string parent)
        {
            Dictionary<string, double> daughters = new Dictionary<string, double>();
            GetDaughterList(parent, daughters);
            return daughters;
        }
        /// <summary>
        /// Recrsivly get names of the daughters and branching ratio
        /// </summary>
        /// <param name="parent"></param>
        /// <param name="daughterName"></param>
        private void GetDaughterList(string parent, Dictionary<string, double> daughters, double branching = 1.0)
        {
            //trim off the plus of the parent if it is a decay series 
            if (parent.Contains('+'))
                parent = parent.Trim('+');
            //get the line for the parent
            //DataRow[] chainRows = library.Tables["DECAYCHAINS"].Select("[PARENT] = '" + parent + "'");
            DataTable chainRows = this.Select("DECAYCHAINS", "[PARENT] = '" + parent + "'");
            //stop if nothing was found
            if (chainRows.Rows.Count < 1)
                return;
            DataRow chainRow = chainRows.Rows[0];
            //get the first daugter chain
            if (!(bool)chainRow["DAUGHTER1_STABLE"] && !chainRow["DAUGHTER1"].Equals("SF"))
            {
                string name = (string)chainRow["DAUGHTER1"];
                double prob = (double)chainRow["PROBABILITY1"] * branching;
                //handle different paths convirging on a single daughter
                if (daughters.ContainsKey(name))
                    daughters[name] += prob;
                else
                    daughters.Add(name, prob);
                //Recurse
                GetDaughterList(name, daughters, prob);
            }
            //get the second daughter chain
            if (!(bool)chainRow["DAUGHTER2_STABLE"] && !chainRow["DAUGHTER2"].Equals("SF"))
            {
                string name = (string)chainRow["DAUGHTER2"];
                double prob = (double)chainRow["PROBABILITY2"] * branching;
                //handle different paths convirging on a single daughter
                if (daughters.ContainsKey(name))
                    daughters[name] += prob;
                else
                    daughters.Add(name, prob);
                //Recurse
                GetDaughterList(name, daughters, prob);
            }
            //get the thrid daughter chain
            if (!(bool)chainRow["DAUGHTER3_STABLE"] && !chainRow["DAUGHTER3"].Equals("SF"))
            {
                string name = (string)chainRow["DAUGHTER3"];
                double prob = (double)chainRow["PROBABILITY3"] * branching;
                //handle different paths convirging on a single daughter
                if (daughters.ContainsKey(name))
                    daughters[name] += prob;
                else
                    daughters.Add(name, prob);
                //Recurse
                GetDaughterList(name, daughters, prob);
            }
        }

        /// <summary>
        /// Get all the daughters given a parent nuclide
        /// </summary>
        /// <param name="name">name of the parent nuclide</param>
        /// <param name="chain">table that contains the chain</param>
        private void GetDaughters(string name, DataTable chain, DataRow topRow)
        {
            //get the row from the parent
            DataRow[] chainRows = library.Tables["DECAYCHAINS"].Select("[PARENT] = '" + name.Trim('+') + "'");
            if (chainRows.Length < 1)
                return;
            DataRow chainRow = chainRows[0];
            //add the first parent
            if (name.Contains('+'))
            {
                DataRow tempRow = chain.NewRow();
                tempRow["NAME"] = topRow["NAME"];
                tempRow["HALF_LIFE"] = topRow["HALF_LIFE"];
                tempRow["HALF_LIFE_UNIT"] = topRow["HALF_LIFE_UNIT"];
                tempRow["PARENT"] = name;
                tempRow["PARENT_HALF_LIFE"] = topRow["HALF_LIFE"];
                tempRow["PARENT_HALF_LIFE_UNIT"] = topRow["HALF_LIFE_UNIT"];
                tempRow["BRANCHING"] = 1.0;
                chain.Rows.Add(tempRow);
                topRow = tempRow;
            }

            //double t12 = (double)chainRow["PARENT_HALF_LIFE"];
            //string t12u = (string)chainRow["PARENT_HALF_LIFE_UNIT"];
            //get the first daughter chain
            bool TEST1 = !(bool)chainRow["DAUGHTER1_STABLE"];
            bool TEST2 = !chainRow["DAUGHTER1"].Equals("SF");
            if (!(bool)chainRow["DAUGHTER1_STABLE"] && !chainRow["DAUGHTER1"].Equals("SF"))
            {
                DataRow row = chain.NewRow();
                row["PARENT"] = topRow["PARENT"];
                row["PARENT_HALF_LIFE"] = topRow["PARENT_HALF_LIFE"];
                row["PARENT_HALF_LIFE_UNIT"] = topRow["PARENT_HALF_LIFE_UNIT"];
                row["NAME"] = chainRow["DAUGHTER1"];
                //get the half life of the daughter
                DataRow[] nucs = library.Tables["NUCLIDES"].Select("[NAME] = '" + chainRow["DAUGHTER1"] + "'");
                if (nucs.Length < 1 || nucs == null)
                    throw new NullReferenceException("Could not find " + chainRow["DAUGHTER1"]);
                row["HALF_LIFE"] = nucs[0]["HALF_LIFE"];
                row["HALF_LIFE_UNIT"] = nucs[0]["HALF_LIFE_UNIT"];
                //handle different paths convirging on a single daughter
                DataRow[] overlap = library.Tables["NUCLIDES"].Select("[PARENT] = '" + topRow["PARENT"] +
                    "' AND NAME = '" + chainRow["DAUGHTER1"] + "'");
                if (overlap == null || overlap.Length == 0)
                {
                    row["BRANCHING"] = chainRow["PROBABILITY1"];
                    chain.Rows.Add(row);
                }
                else
                    overlap[0]["BRANCHING"] = (double)overlap[0]["BRANCHING"] * (double)chainRow["PROBABILITY1"];

                GetDaughters((string)chainRow["DAUGHTER1"], chain, topRow);
            }
            //get the second daughter chain
            if (!(bool)chainRow["DAUGHTER2_STABLE"] && !chainRow["DAUGHTER2"].Equals("SF"))
            {
                DataRow row = chain.NewRow();
                row["PARENT"] = topRow["PARENT"];
                row["PARENT_HALF_LIFE"] = topRow["PARENT_HALF_LIFE"];
                row["PARENT_HALF_LIFE_UNIT"] = topRow["PARENT_HALF_LIFE_UNIT"];
                row["NAME"] = chainRow["DAUGHTER2"];
                //get the half life of the daughter
                DataRow[] nucs = library.Tables["NUCLIDES"].Select("[NAME] = '" + chainRow["DAUGHTER2"] + "'");
                if (nucs.Length < 1 || nucs == null)
                    throw new NullReferenceException("Could not find " + chainRow["DAUGHTER2"]);
                row["HALF_LIFE"] = nucs[0]["HALF_LIFE"];
                row["HALF_LIFE_UNIT"] = nucs[0]["HALF_LIFE_UNIT"];
                //handle different paths convirging on a single daughter
                DataRow[] overlap = library.Tables["NUCLIDES"].Select("[PARENT] = '" + topRow["PARENT"] +
                    "' AND NAME = '" + chainRow["DAUGHTER2"] + "'");
                if (overlap == null || overlap.Length == 0)
                {
                    row["BRANCHING"] = chainRow["PROBABILITY2"];
                    chain.Rows.Add(row);
                }
                else
                    overlap[0]["BRANCHING"] = (double)overlap[0]["BRANCHING"] * (double)chainRow["PROBABILITY2"];

                GetDaughters((string)chainRow["DAUGHTER1"], chain, topRow);
            }
            //get the thrid daughter chain
            if (!(bool)chainRow["DAUGHTER3_STABLE"] && !chainRow["DAUGHTER3"].Equals("SF"))
            {
                DataRow row = chain.NewRow();
                row["PARENT"] = topRow["PARENT"];
                row["PARENT_HALF_LIFE"] = topRow["PARENT_HALF_LIFE"];
                row["PARENT_HALF_LIFE_UNIT"] = topRow["PARENT_HALF_LIFE_UNIT"];
                row["NAME"] = chainRow["DAUGHTER3"];
                //get the half life of the daughter
                DataRow[] nucs = library.Tables["NUCLIDES"].Select("[NAME] = '" + chainRow["DAUGHTER3"] + "'");
                if (nucs.Length < 1 || nucs == null)
                    throw new NullReferenceException("Could not find " + chainRow["DAUGHTER3"]);
                row["HALF_LIFE"] = nucs[0]["HALF_LIFE"];
                row["HALF_LIFE_UNIT"] = nucs[0]["HALF_LIFE_UNIT"];
                //handle different paths convirging on a single daughter
                DataRow[] overlap = library.Tables["NUCLIDES"].Select("[PARENT] = '" + topRow["PARENT"] +
                    "' AND NAME = '" + chainRow["DAUGHTER3"] + "'");
                if (overlap == null || overlap.Length == 0)
                {
                    row["BRANCHING"] = chainRow["PROBABILITY3"];
                    chain.Rows.Add(row);
                }
                else
                    overlap[0]["BRANCHING"] = (double)overlap[0]["BRANCHING"] * (double)chainRow["PROBABILITY3"];

                GetDaughters((string)chainRow["DAUGHTER1"], chain, topRow);
            }
        }

        /// <summary>
        /// Convert the half life from the origianl unit to days
        /// </summary>
        /// <param name="originalUnit">original unit</param>
        /// <param name="halfLife">half life to convert</param>
        /// <returns>half life in days</returns>
        public static double ConvertHalfLifeToDays(string originalUnit, double halfLife)
        {
            switch (originalUnit)
            {
                case "us":
                    halfLife *= 1.15714E-11;
                    break;
                case "ms":
                    halfLife *= 1.15714E-8;
                    break;
                case "s":
                    halfLife *= 1.15714E-5;
                    break;
                case "m":
                    halfLife *= 6.94444e-4;
                    break;
                case "h":
                    halfLife *= 4.16667e-2;
                    break;
                case "d":
                    halfLife *= 1.0;
                    break;
                case "y":
                    halfLife *= 365;
                    break;
                default:
                    throw new ArgumentException("Unit must be {us,ms,s,m,h,d, or y}");
            }

            return halfLife;
        }
        /// <summary>
        /// Write the Dataset to the access database
        /// </summary>
        public void WriteToDatabase()
        {
            //create a sqlite file if there isn't one
            if (!File.Exists("GammaData.sqlite"))
                SQLiteConnection.CreateFile("GammaData.sqlite");
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=GammaData.sqlite;Version=3"))
            {
                conn.Open();
                //loop through the tables
                foreach (DataTable table in Library.Tables)
                //for (int j = 2; j < Library.Tables.Count; j++ )
                {
                    //DataTable table = library.Tables["NUCLIDES"];
                    ////clear the data beforehand
                    SQLiteCommand clrTableCmd = conn.CreateCommand();
                    clrTableCmd.CommandText = "DELETE FROM " + table.TableName;
                    clrTableCmd.Connection = conn;
                    clrTableCmd.ExecuteNonQuery();
                    //DataTable table = Library.Tables[j];
                    //loop through all the rows to fill out the table
                    //for (int j = 1559; j < table.Rows.Count; j++ )

                    foreach (DataRow row in table.Rows)
                    {
                        //DataRow row = table.Rows[j];
                        SQLiteCommand cmd = conn.CreateCommand();
                        //build out the command text
                        StringBuilder cmdTxt = new StringBuilder("INSERT into ");
                        //build out the values
                        StringBuilder valTxt = new StringBuilder("VALUES(");
                        cmdTxt.Append(table.TableName);
                        cmdTxt.Append("(");
                        for (int i = 0; i < table.Columns.Count; i++)
                        {
                            //no comma on the last column
                            if (i < table.Columns.Count - 1)
                            {
                                cmdTxt.Append(table.Columns[i].ColumnName + ",");
                                valTxt.Append("'" + row[i] + "',");
                            }
                            else
                            {
                                cmdTxt.Append(table.Columns[i].ColumnName);
                                valTxt.Append("'" + row[i] + "'");
                            }
                        }
                        cmdTxt.Append(")");
                        valTxt.Append(")");
                        cmdTxt.Append(valTxt);
                        //do the query business
                        cmd.CommandText = cmdTxt.ToString();
                        cmd.Connection = conn;
                        cmd.ExecuteNonQuery();
                    }
                    Console.WriteLine("Finished with " + table.TableName);
                }
                Console.WriteLine("Finished Writing Database");
            }
        }

        /// <summary>
        /// Selects data from the table with a givin expression
        /// </summary>
        /// <param name="tableName">Name of table to select from</param>
        /// <param name="expression">Filter expression</param>
        /// <returns></returns>
        public DataTable Select(string tableName, string expression)
        {
            DataTable results = new DataTable();
            string cmdTxt = "SELECT * FROM " + tableName + " WHERE " + expression;
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=GammaData.sqlite;Version=3"))
            {
                conn.Open();
                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmdTxt, conn))
                    adapter.Fill(results);
            }
            //retrun the results
            return results;
        }
        /// <summary>
        /// Converts the input string to one that can be used in queries
        /// </summary>
        /// <param name="input">input string</param>
        /// <returns>formatted string for ICRP-107 queries</returns>
        public string GetNuclideName(string input)
        {
            string output;
            //get the atomic number and isomeric state (m,n,m2,etc.)
            string atmNum = ""; string isoState = ""; string nucName; string hyphen; string numState = "";
            input = input.ToLower();
            //Match numStateMatch = new Regex(@"([\d]{1,3}[m,n]{1}[\d])|([0-9]{1,3}[m,n]{1})[^A-Za-z]|(?![A-Za-z])[\d]+.").Match(input);
            Match numStateMatch = new Regex(@"(?![A-Za-z])[\d]+[m, n]?").Match(input);
            if (numStateMatch.Success)
            {
                numState = numStateMatch.Value;
                //just the atomic number
                Match atmNumMatch = new Regex(@"[\d]{1,3}").Match(numState);
                atmNum = atmNumMatch.Success ? atmNumMatch.Value : "";
                //just the isometic state
                Match isoStateMatch = new Regex(@"[^\d]?[m,n][\d]|[^\d]?[m,n]").Match(numState);
                //covert m2 to n
                if (isoStateMatch.Success)
                    isoState = isoStateMatch.Value.Equals("m2", StringComparison.OrdinalIgnoreCase) ? "n" : isoStateMatch.Value;
            }
            //the hypon
            Match hyponMatch = new Regex(@"-").Match(input);
            hyphen = hyponMatch.Success ? hyponMatch.Value : "";

            //the nuclide name is all that is left
            nucName = String.IsNullOrEmpty(numState) ? input.Trim(hyphen.ToCharArray()) : input.Replace(numState,"").Trim(hyphen.ToCharArray());

            hyphen = (String.IsNullOrEmpty(nucName) || String.IsNullOrEmpty(numState)) ? "" : "-";

            output = nucName.ToUpper() + hyphen + atmNum + isoState.ToLower();
            //Match nuc = new Regex().Matche(input);
            return output;
        }
        /// <summary>
        /// Selects data from the table with a givin expression
        /// </summary>
        /// <param name="expression">Filter expression</param>
        /// <returns></returns>
        public DataTable Select(string expression)
        {
            DataTable results = new DataTable();
            string cmdTxt = "SELECT " + expression;
            using (SQLiteConnection conn = new SQLiteConnection("Data Source=GammaData.sqlite;Version=3"))
            {
                conn.Open();
                using (SQLiteDataAdapter adapter = new SQLiteDataAdapter(cmdTxt, conn))
                    adapter.Fill(results);
            }
            //retrun the results
            return results;
        }
        /// <summary>
        /// Library Object
        /// </summary>
        public DataSet Library { get { return library; } }

    }
}
