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
using System.ComponentModel;

using CAMInputOutput;

namespace PeakMap
{
    public class CAMSpecData : SpectralData
    {
        [Browsable(false)]
        readonly CAMIO camfile;
        //[Browsable(false)]
        //uint[] spectrum;
        [Browsable(false)]
        double[] energyCoeff;
        [Browsable(false)]
        double[] shapeCoeff;

        public CAMSpecData()
        {
            camfile = new CAMIO();
        }

        public CAMSpecData(string inpfile)
        {
            //open and read the file
            camfile = new CAMIO();
            file = inpfile;
        }

        public override void CloseFile()
        {

        }
        /// <summary>
        /// Gets spectral the region around and energy 
        /// </summary>
        /// <param name="energy"></param>
        /// <returns></returns>
        public override double GetRegion(double energy)
        {
            double cts = 0;
            double width = GetWidthofRegion(energy);
            int start = GetChannel(energy - width + 1);
            int end = GetChannel(energy + width);
            //dp some error checking
            if (start > spectrum.Length || end > spectrum.Length)
                return 1;
            if (start < 1 || end < 1)
                throw new ArgumentOutOfRangeException("Energy: " + energy + " is beyond the range of the spectum");
            //loop through the spectra and sum up the channels
            for (int i = start;  i < end + 1; i++) 
            {
                cts += spectrum[i];
            }
            //return at least 1
            return cts == 0 ? 1 : cts;
        }
        /// <summary>
        /// Get the width in channels 
        /// </summary>
        /// <param name="ch">The channel number</param>
        /// <returns>the width in channels</returns>
        protected override int GetWidth(int ch) 
        {
            double energy = GetEnergy(ch);
            double width = GetWidthofRegion(energy);
            return GetChannels(width, ch);
        }
        /// <summary>
        /// Get the channels given and energy range.
        /// </summary>
        /// <param name="energyRange">an energy range</param>
        /// <param name="ch">The channel</param>
        /// <returns>The number of channels for the energy range</returns>
        private int GetChannels(double energyRange, int ch) 
        {
            if (energyCoeff == null || energyCoeff.Length < 2)
                throw new ArgumentNullException("There are no valid energy coefficients");
            else if (energyCoeff[2] == 0)
                return (int)Math.Round(energyRange/energyCoeff[1]);
            else if (energyCoeff[3] == 0)
                return (int)Math.Round(energyRange/(energyCoeff[2]*ch+energyCoeff[1]));
            else
                throw new ArgumentOutOfRangeException("Only Linear and Quadriatic energy calibrations are accepted");
        }
        /// <summary>
        /// Get the width of a region given an energy
        /// </summary>
        /// <param name="energy">Energy</param>
        /// <returns>The FWHM at that point</returns>
        private double GetWidthofRegion(double energy) 
        {
            if (shapeCoeff == null || shapeCoeff.Length < 2)
                throw new ArgumentNullException("There are no valid shape coefficients");
            return shapeCoeff[0] + shapeCoeff[1] * Math.Sqrt(energy);
        }
        /// <summary>
        /// Get the channel number given an energy
        /// </summary>
        /// <param name="energy">Energy</param>
        /// <returns>cahnnel at that energy</returns>
        private int GetChannel(double energy) 
        {
            if (energyCoeff == null || energyCoeff.Length < 2)
                throw new ArgumentNullException("There are no valid energy coefficients");
            else if (energyCoeff[2] == 0)
                return (int)Math.Round((energy - energyCoeff[0]) / energyCoeff[1]);
            else if (energyCoeff[3] == 0)
                return (int)Math.Round((-1 * energyCoeff[1] + Math.Sqrt(energyCoeff[1] * energyCoeff[1] - 4 * energyCoeff[0] * energyCoeff[2])) / 2 * energyCoeff[0]);
            else
                throw new ArgumentOutOfRangeException("Only Linear and Quadriatic energy calibrations are accepted");
        }
        /// <summary>
        /// Get the energy given a channel
        /// </summary>
        /// <param name="cahnnel">channel</param>
        /// <returns>energy at a channel</returns>
        private double GetEnergy(int channel)
        {
            if (energyCoeff == null || energyCoeff.Length < 2)
                throw new ArgumentNullException("There are no valid energy coefficients");
            else if (energyCoeff[2] == 0)
                return energyCoeff[1] * channel + energyCoeff[0];
            else if (energyCoeff[3] == 0)
                return energyCoeff[2]*Math.Pow(channel,2)+ energyCoeff[1] * channel + energyCoeff[0];
            else
                throw new ArgumentOutOfRangeException("Only Linear and Quadriatic energy calibrations are accepted");
        }
        /// <summary>
        /// Gets the peak inforamtion async
        /// </summary>
        /// <param name="peaks"></param>
        /// <returns></returns>
        public override async Task LoadDataAsync(DataTable peaks)
        {

            /**********get the data async******/
            camfile.ReadFile(file);
            //time
            Task<DateTime> acqTask = Task.Run(() => camfile.GetAqusitionTime());
            Task<DateTime> sampTask = Task.Run(() => camfile.GetSampleTime());
            Task<double> liveTask = Task.Run(() => camfile.GetLiveTime());
            //sepectrum
            Task<uint[]> specTask = Task.Run(() => camfile.GetSpectrum());
            //peaks
            Task<Peak[]> peakTask = Task.Run(() => camfile.GetPeaks().ToArray());
            //calibrations
            Task<double[]> ecalTask = Task.Run(() => camfile.GetEnergyCalibration());
            Task<double[]> shapeCalTask = Task.Run(() => camfile.GetShapeCalibration());
            Task<EfficiencyPoint[]> effCalTask = Task.Run(() => camfile.GetEfficiencyPoints().ToArray());

            //assign variables
            collTime = await sampTask;
            acqTime = await acqTask;
            elapsedWait = collTime - acqTime;
            countTime = await liveTask;
            spectrum = await specTask;


            //assign the calibration info
            energyCoeff = await ecalTask;
            shapeCoeff = await shapeCalTask;
            EfficiencyPoint[] effpts = await effCalTask;

            //assign the efficiency points
            effMeas = new ObservableEntityCollection<EfficiencyMeasurement>();
            for (int i =0; i< effpts.Length;i++ ) 
                effMeas.Add( new EfficiencyMeasurement(effpts[i].Energy, effpts[i].Efficiency, effpts[i].EfficiencyUncertainty));
            effMeas.CollectionChanged += EffMeas_CollectionChanged;
            //assign the peaks
            Peak[] filePeaks = await peakTask;
            //add the peaks to the datatable
            foreach (Peak pk in filePeaks)
            {
                DataRow row = peaks.NewRow();
                row["ENERGY"] = pk.Energy;
                row["FWHM"] = pk.FullWidthAtHalfMaximum;
                row["AREA"] = pk.Area;
                row["AREAUNC"] = pk.AreaUncertainty;
                row["CONTINUUM"] = pk.Continuum;
                row["CRITLEVEL"] = pk.CriticalLevel;
                peaks.Rows.Add(row);
            }
            order = effMeas.Count > 7 ? 5 : effMeas.Count > 2 ? effMeas.Count - 2 : 3;
            calParams = GetCalibrationParameters();
        }
    }
}
