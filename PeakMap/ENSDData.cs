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
using System.IO;
using System.Text.RegularExpressions;
using System.Data.SQLite;

namespace PeakMap
{
    class ENSDData : IDataLibrary
    {
        private readonly string directory;
        /// <summary>
        /// Directory where the ENSD files are stored
        /// </summary>
        public string DataDirectory 
        {
            get { return directory; }
            set { DataDirectory = value; }
        }
        private DataSet library;

        public Dictionary<string, double> GetDaughters(string parent)
        {
            throw new NotImplementedException();
        }

        public string GetNuclideName(string input)
        {
            throw new NotImplementedException();
        }

        public async Task InitializeAsync()
        {
            library = new DataSet();
            library.ReadXmlSchema(Path.Combine(Environment.CurrentDirectory, "ICRPLibrary.xsd"));

            await Task.Run(() => ReadDataFiles());


        }

        public DataTable Select(string tableName, string expression)
        {
            throw new NotImplementedException();
        }

        public DataTable Select(string expression)
        {
            throw new NotImplementedException();
        }

        public void WriteToDatabase()
        {
            throw new NotImplementedException();
        }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="Directory">Directory containing the ENDSF files</param>
        public ENSDData(string Directory) 
        {
            this.directory = Directory;
        }
        /// <summary>
        /// Constructor
        /// </summary>
        public ENSDData()
        {

        }
        private void ReadDataFiles() 
        {
            string[] files = Directory.GetFiles(directory);

            //read all the files in the directory
            foreach (string file in files) 
            {
                try
                {
                    using (FileStream fs = File.OpenRead(file))
                    {
                        using (StreamReader st = new StreamReader(fs))
                        {
                            string line;
                            while ((line = st.ReadLine()) != null) 
                            {
                                string ID = line.Substring(0, 5);
                                string record = line.Substring(6, 2);
                                switch (record) 
                                {
                                    //parent record
                                    case " P":
                                        break;
                                     //Level record
                                    case " L":
                                        break;
                                }
                            }
                        }
                    }
                }
                catch (FileLoadException ex) 
                { 
                }
            }
        }


    }
}
