using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Threading.Tasks;

namespace PeakMap
{
    public class UserSpecData : SpectralData
    {

        [Browsable(false)]
        DataTable data;

        public UserSpecData(string inpfile)
        {
            //calParams = new List<double> { -1.0e-4, 1.4e-17, -8.7e-19 };
            effMeas = new ObservableEntityCollection<EfficiencyMeasurement>();
            double eff = 0.9999999999999;
            double effunc = 0.000000000005;
            effMeas.Add(new EfficiencyMeasurement(Properties.Settings.Default.LOWERELIMT, eff, effunc));
            effMeas.Add(new EfficiencyMeasurement((Properties.Settings.Default.LOWERELIMT + Properties.Settings.Default.UPPERELIMIT) / 2, eff, effunc));
            effMeas.Add(new EfficiencyMeasurement(Properties.Settings.Default.UPPERELIMIT, eff, effunc));
            order = 2;
            GetCalibrationParameters();

            effMeas.CollectionChanged += EffMeas_CollectionChanged;
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
        public override Task LoadDataAsync(DataTable peaks)
        {
            //DataRow row = peaks.Rows[0];
            data = peaks;
            data.ColumnChanged += Data_ColumnChanged;
            return Task.FromResult(0);
        }

        /// <summary>
        /// Handle the DataColumnChanged event on the datatable
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Data_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            //calcuated the MDA 
            if(e.Column.ColumnName == "CONTINUUM")
            {
                e.Row["CRITLEVEL"] = (double)e.Row["CRITLEVEL"] == 0 ? 1.645 * Math.Sqrt((double)e.Row["CONTINUUM"]): e.Row["CRITLEVEL"];
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ch"></param>
        /// <returns></returns>
        protected override int GetWidth(int ch)
        {
            //TODO implement GetWidth User Data
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get the counts in a region of the spectrum 
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
        public override void CloseFile() 
        {
            data.Clear();
        }
    }
}
