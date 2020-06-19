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
using System.Threading.Tasks;
using System.Linq;
using Accord.Math;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Collections.Specialized;

namespace PeakMap
{

    public class EfficiencyMeasurement : INotifyPropertyChanged
    {
        private  double ene;
        private  double eff;
        private  double effUnc;

        public event PropertyChangedEventHandler PropertyChanged;

        [Category("Efficiency"),Description("Energy of measuremnt")]
        public double Energy { get { return ene; }
            set 
            { 
                ene = value;
                OnPropertyChanged("Energy");
            } 
        }
        [Category("Efficiency"), Description("Efficiciency at energy")]
        public double Efficiency { get { return eff; } 
            set 
            {
                eff = value;
                OnPropertyChanged("Efficiency");
            } 
        }
        [Category("Efficiency"), Description("Uncertainty in efficiciency at energy")]
        public double EfficiencyUncertainty { get { return effUnc; } 
            set 
            { 
                effUnc = value;
                OnPropertyChanged("EfficiencyUncertainty");
            } 
        }
        /// <summary>
        /// Constructor of EfficiencyMeasurement object
        /// </summary>
        /// <param name="energy"></param>
        /// <param name="efficiency"></param>
        /// <param name="efficiencyUncertainty"></param>
        public EfficiencyMeasurement(double energy, double efficiency, double efficiencyUncertainty) 
        {
            ene = energy;
            eff = efficiency;
            effUnc = efficiencyUncertainty;
        }
        /// <summary>
        /// returns the efficiency and energy
        /// </summary>
        /// <returns>String of the efficiency and energy</returns>
        public override string ToString()
        {
            return ene.ToString("E2") +"," +eff.ToString("E2");
        }
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
    /// <summary>
    /// extension to ObeservaleCollection when an entity changes
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ObservableEntityCollection<T> : ObservableCollection<T> where T : INotifyPropertyChanged
    {
        public ObservableEntityCollection() : base()
        {
            this.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(ObservableEntityCollection_CollectionChanged);
        }

        private void ObservableEntityCollection_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (T item in e.OldItems)
                    item.PropertyChanged -= EntityViewModelPropertyChanged;
            }
            else if (e.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (T item in e.NewItems)
                    item.PropertyChanged += EntityViewModelPropertyChanged;
            }
        }

        private void EntityViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            NotifyCollectionChangedEventArgs args = new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset);
            OnCollectionChanged(args);
        }
    }
    /// <summary>
    /// Struct for a peakSignal
    /// </summary>
    public struct PeakSignal
    {
        private int peakIndex;
        private int ch;
        private int cts;
        private double ss;

        /// <summary>
        /// Zero based index for the peak
        /// </summary>
        public int PeakIndex { get { return peakIndex; } set { peakIndex = value; } }
        /// <summary>
        /// Channel of the signal
        /// </summary>
        public int Channel { get { return ch; } set { ch = value; } }
        /// <summary>
        /// Counts in the channel
        /// </summary>
        public int Counts { get { return cts; } set { cts = value; } }
        /// <summary>
        /// Second difference at the channel
        /// </summary>
        public double SecondDifference { get { return ss; } set { ss = value; } }

        /// <summary>
        /// Constructor for a peak Signal
        /// </summary>
        /// <param name="index">the zero based index for the peak</param>
        /// <param name="channel">the channel of the signal</param>
        /// <param name="counts">the counts in the channel</param>
        /// <param name="secondDiff">the second difference at the channel</param>
        public PeakSignal(int index, int channel, int counts, double secondDiff) 
        {
            peakIndex = index;
            ch = channel;
            cts = counts;
            ss = secondDiff;
        }
    }
    public abstract class SpectralData
    {
        //enumerations
        public enum CalibrationModel { Linear, Natural, Emperical }
        public enum SpecType { CNF, txt, csv, User }
        public enum BoundaryCondition
        {
            Clamped,
            Natural
        }
        //envents
        public event EventHandler<EventArgs> CalibrationParamtersRefreshed;
        //variables
        protected CalibrationModel calModel = CalibrationModel.Linear;
        protected List<double> calParams;
        protected TimeSpan elapsedWait;
        protected DateTime acqTime;
        protected DateTime collTime;
        protected double countTime;
        protected string file;
        protected int order;
        private int numMeas;
        protected ObservableEntityCollection<EfficiencyMeasurement> effMeas;

        protected uint[] spectrum;
        private double[] splineCoeff;
        private Tuple<int, double>[] knots;
        //SpecType source;
        #region Accessors
        /// <summary>
        /// Gets or sets the Model for Calibration
        /// </summary>
        [Category("Calibration"),
        DisplayName("Calibration Model"),
        Description("Model for calibration fit"),
        DefaultValue(0.1)
        ]
        public CalibrationModel CalModel { get { return calModel; } set { calModel = value; calParams = GetCalibrationParameters(); } }
        /// <summary>
        /// Gets or sets the curve fit parameters
        /// </summary>
        [Category("Calibration"),
        ReadOnly(true),
        DisplayName("Calibration Parameters"),
        Description("Curve fitting coefficients")]
        public List<double> CalibrationParams { get { return calParams; } }

        /// <summary>
        /// The effieicny calibration points Item1=Energy, Item2=Efficiency, Item3=EfficiencyUncertainty(1σ)
        /// </summary>
        [Category("Calibration"),
        DisplayName("Efficiency Points"),
        Description("Efficiency points to fit curve to")]
        public ObservableEntityCollection<EfficiencyMeasurement> EfficiencyPoints { 
            get {
                numMeas = effMeas.Count;
                return effMeas; 
            } 
            set {
                effMeas = value;
                numMeas = effMeas.Count;
            } 
        }

        /// <summary>
        /// Gets or sets the curve fit parameters
        /// </summary>
        [Category("Calibration"),
        DisplayName("Calibration Polynomial Order"),
        Description("The order of the calibration curve polynomial")]
        public int Order { get { return order; } set { order = value; calParams = GetCalibrationParameters(); } }
        /// <summary>
        /// Gets or sets the elapsed wait time
        /// </summary>
        [Category("Time"),
        DisplayName("Sample Time"),
        Description("Time of the Sample Collection")]
        public DateTime CollectionTime
        {
            get { return collTime; }
            set
            {
                if (acqTime > value)
                    collTime = value;
                elapsedWait = acqTime - collTime;
            }
        }
        /// <summary>
        /// Gets or sets the aqusition time
        /// </summary>
        [Category("Time"),
        DisplayName("Acquisition Time"),
        Description("Time at the start of acquisition")]
        public DateTime AcquisitionTime
        {
            get { return acqTime; }
            set
            {
                if (value > collTime)
                    acqTime = value;
                elapsedWait = acqTime - collTime;
            }
        }
        /// <summary>
        /// Gets or sets the count time
        /// </summary>
        [Category("Time"),
        DisplayName("Count Time"),
        Description("Length of time the sample was counted in seconds")]
        public double CountTime { get { return countTime; } set { countTime = value; } }
        /// <summary>
        /// Gets the Specrtral File.
        /// </summary>
        [DisplayName("File"),
        Description("The input peak spectral file")]
        
        /// <summary>
        /// Gets the Elapsed Wait time span
        /// </summary>
        [Browsable(false)]
        public TimeSpan ElapsedWait { get { return elapsedWait; } }
        [Browsable(false)]
        public String SpectralFile { get { return file; } }
        #endregion

        /// <summary>
        /// Gets the type of spectral Data file to use
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        public static Type GetSpectralDataType(string file)
        {
            //covert to enum
            if (Enum.TryParse(Path.GetExtension(file).Trim('.'), true, out SpecType sType))
            {
                switch (sType)
                {
                    case SpecType.CNF:
                        return typeof(CAMSpecData);
                    case SpecType.txt:
                        return typeof(TextData);
                    case SpecType.csv:
                        return typeof(TextData);
                }
            }
            else if (file == "User")
            {
                return typeof(UserSpecData);
            }
            throw new ArgumentException("File type is not recognized as spectral peak data");
        }
        #region EfficiencyCalcs
        /// <summary>
        /// Gets the efficiency given an energy based off the calibation model
        /// </summary>
        /// <param name="energy"></param>
        /// <returns>Efficiency -1 if </returns>
        public double GetEfficiency(double energy)
        {
            double efficiency = 0;
            if (calParams == null)
                calParams = GetCalibrationParameters();
            int degree = calParams.Count;
            if (degree < 1)
                throw new ArgumentException("There are no calibration parameters. Cannot compute efficiency");

            switch (calModel)
            {
                case CalibrationModel.Natural:
                    for (int i = 0; i < degree; i++)
                    {
                        double b = calParams[i];
                        efficiency += b * Math.Pow(Math.Log(energy), i);
                    }
                    efficiency = Math.Exp(efficiency);
                    break;
                case CalibrationModel.Emperical:
                    double ca = (effMeas[0].Energy + effMeas[effMeas.Count - 1].Energy) / 2;
                    for (int i = 0; i < degree; i++)
                    {
                        double b = calParams[i ];
                        efficiency += b* Math.Pow(Math.Log(ca / energy), i);
                    }
                    efficiency = Math.Exp(efficiency);
                    break;
                default:
                    for (int i = 0; i < degree; i++)
                    {
                        double b = calParams[i];
                        efficiency += b / Math.Pow(energy, i - 1);
                    }
                    efficiency = Math.Pow(10, efficiency);
                    break;
            }
            return efficiency;
        }

        /// <summary>
        /// Gets the efficiency calibration fitting parameters from the points
        /// </summary>
        /// <returns>Efficiency calibration fitting parameters</returns>
        protected List<double> GetCalibrationParameters()
        {
            if (effMeas.Count < 2)
                throw new ArgumentException("There are not enough efficiency points to generate a curve");
            if(order  >= effMeas.Count)
                throw new ArgumentException("The order of the polnominal is too high for the number of points: "+ effMeas.Count);
            //get the default order of polynomial
            //int order = effPoints.Count > 7 ? 5 : effPoints.Count > 2 ? effPoints.Count - 2 : 3 ;
            double[,] A = new double[effMeas.Count, order];
            double[] eff = new double[effMeas.Count];

            //get the model to be used
            switch (calModel)
            {
                case CalibrationModel.Natural:
                    BuildNatEquations(effMeas, order, ref A, ref eff);
                    break;
                case CalibrationModel.Linear:
                    BuildLinEquations(effMeas, order, ref A, ref eff);
                    break;
                case CalibrationModel.Emperical:
                    BuildEmpEquations(effMeas, order, ref A, ref eff);
                    break;
                default:
                    throw new ArgumentException("Calibrarion model is not supported");
            }

            //invert and multiply
            double[,] AT = Accord.Math.Matrix.Transpose(A);
            double[,] eqs = Accord.Math.Matrix.Dot(AT, A);
            double[] var = Accord.Math.Matrix.Dot(AT, eff);
            double[,] inverse = Accord.Math.Matrix.Inverse(eqs);
            return Accord.Math.Matrix.Dot(var, inverse).ToList();
        }

        /// <summary>
        /// Build the regression equation matxix and variances
        /// </summary>
        /// <param name="points">The data to regress </param>
        /// <param name="order">The order of the polynomial</param>
        /// <param name="matValue">Matrix to fill</param>
        /// <param name="varValue">Varaicne vector to fill</param>
        private void BuildNatEquations(IList<EfficiencyMeasurement> points, int order, ref double[,] matValue, ref double[] varValue)
        {
            //matValue = new double[points.Count, order];
            //varValue = new double[points.Count];
            //row (num efficiney points)
            for (int i = 0; i < points.Count; i++)
            {
                EfficiencyMeasurement point = points[i];
                double weight = ComputeWeight(point);
                varValue[i] = weight * Math.Log(point.Efficiency);
                //columns (order)
                for (int j = 0; j < order; j++)
                    matValue[i, j] = weight * Math.Pow(Math.Log(point.Energy), j);

            }
            /**
            matValue = new double[order, order];
            varValue = new double[order];
            for (int k = 0; k < order; k++)
            {
                for (int j = 0; j < order; j++)
                {
                    foreach (EfficiencyMeasurement point in points)
                    {
                        double weight = ComputeNatWeight(point);
                        matValue[k, j] += weight * Math.Pow(Math.Log(point.Energy), j - 1) * Math.Pow(Math.Log(point.Energy), k - 1);
                        varValue[k] += j == 0 ? weight * Math.Log(point.Efficiency) * Math.Pow(Math.Log(point.Energy), k - 1) : 0;
                    }
                }
            }
            **/
        }
        /// <summary>
        /// Compute the weight
        /// </summary>
        /// <param name="point">Data point for weight</param>
        /// <returns>Weight</returns>
        private double ComputeWeight(EfficiencyMeasurement point)
        {
            return Math.Pow(point.Efficiency / point.EfficiencyUncertainty, 2);
        }
        /// <summary>
        /// Build the regression equation matxix and variances
        /// </summary>
        /// <param name="points">The data to regress </param>
        /// <param name="order">The order of the polynomial</param>
        /// <param name="matValue">Matrix to fill</param>
        /// <param name="varValue">Varaicne vector to fill</param>
        private void BuildLinEquations(IList<EfficiencyMeasurement> points, int order, ref double[,] matValue, ref double[] varValue)
        {

            //row (num efficiney points)
            for (int i = 0; i < points.Count; i++)
            {
                EfficiencyMeasurement point = points[i];
                double weight = ComputeWeight(point);
                varValue[i] = weight * Math.Log10(point.Efficiency);
                //columns (order)
                for (int j = 0; j < order; j++)
                    matValue[i, j] = weight * Math.Pow(1 / point.Energy, j-1);
                
            }
            /**
            for (int k = 0; k < order; k++)
            {
                for (int j = 0; j < order; j++)
                {
                    foreach (EfficiencyMeasurement point in points)
                    {
                        double weight = ComputeLinWeight(point);
                        matValue[k, j] += weight * Math.Pow(1 / point.Energy, j - 1) * Math.Pow(1 / point.Energy, k - 1);
                        varValue[k] += j == 0 ? weight * Math.Log10(point.Efficiency) * Math.Pow(1 / point.Energy, k - 1) : 0;
                    }
                }
            }
            **/
        }

        /// <summary>
        /// Build the regression equation matxix and variances
        /// </summary>
        /// <param name="points">The data to regress </param>
        /// <param name="order">The order of the polynomial</param>
        /// <param name="matValue">Matrix to fill</param>
        /// <param name="varValue">Varaicne vector to fill</param>
        private void BuildEmpEquations(IList<EfficiencyMeasurement> points, int order, ref double[,] matValue, ref double[] varValue)
        {
            double ca = (points[0].Energy + points[points.Count - 1].Energy) / 2;

            //row (num efficiney points)
            for (int i = 0; i < points.Count; i++)
            {
                EfficiencyMeasurement point = points[i];
                double weight = ComputeWeight(point);
                varValue[i] = weight * Math.Log(point.Efficiency);
                //columns (order)
                for (int j = 0; j < order; j++)
                    matValue[i, j] = weight * Math.Pow(Math.Log(ca / point.Energy), j);

            }
            /**
            double ca = (points[0].Energy + points[points.Count-1].Energy) / 2;
            matValue = new double[order, order];
            varValue = new double[order];
            for (int k = 0; k < order; k++)
            {
                for (int j = 0; j < order; j++)
                {

                    foreach (EfficiencyMeasurement point in points)
                    {
                        double weight = ComputeEmpWeight(point);
                        matValue[k, j] += weight * Math.Pow(ca / point.Energy, j - 1) * Math.Pow(ca / point.Energy, k - 1);
                        varValue[k] += j == 0 ? weight * Math.Log(point.Efficiency) * Math.Pow(Math.Log(ca / point.Energy), k - 1) : 0;
                    }
                }
            }
            **/
        }
        protected void EffMeas_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //ignore the rest action
            //if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            //    return;
            if(((ObservableCollection<EfficiencyMeasurement>)sender).Count == numMeas)
                RefreshCalibrationParameters();
        }
        #endregion
        /// <summary>
        /// Refreshes the calibration parameters
        /// </summary>
        public void RefreshCalibrationParameters()
        {
            calParams = GetCalibrationParameters();
            CalibrationParamtersRefreshed?.Invoke(this, new EventArgs());
        }
        /// <summary>
        /// Get the width in channels 
        /// </summary>
        /// <param name="ch">The channel number</param>
        /// <returns>the width in channels</returns>
        protected abstract int GetWidth(int ch);
        /// <summary>
        /// Loads the peak data from the specified file and loads in into the peaks datatable
        /// </summary>
        /// <param name="peaks">container for the peak data</param>
        public abstract Task LoadDataAsync(DataTable peaks);
        /// <summary>
        /// Fills the peaks data Table
        /// </summary>
        /// <param name="peaks">Container for the peaks data table</param>
        //protected abstract void Fill(DataTable peaks);
        /// <summary>
        /// Get the counts in a region of the spectrum 
        /// </summary>
        /// <param name="energy">Energy at the region</param>
        /// <returns>Number of counts in the region</returns>
        public abstract double GetRegion(double energy);
        /// <summary>
        /// Closes the specral file
        /// </summary>
        public abstract void CloseFile();
    }
    public class KnotComparer<T> : System.Collections.IComparer
    {
        public int Compare(object a, object b)
        {
            return Comparer<int>.Default.Compare(((Tuple<int,double>)a).Item1, (int)b);
        }
    }
}
