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
using System.Windows.Input;
using PeakMap;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows.Media;
using System.Windows;
using PeakMapWPF.Models;
using System.Collections.Generic;

namespace PeakMapWPF.ViewModels
{
    class SpectralSettingsViewModel : IDialogRequestClose, INotifyPropertyChanged
    {
        public event EventHandler<DialogCloseRequestEventArgs> CloseRequested;
        public event PropertyChangedEventHandler PropertyChanged;

        private readonly SpectralData data;
        public RelayCommand CopyCommand { get; private set; }

        public SpectralSettingsViewModel(SpectralData data)
        {
            OkCommand = new RelayCommand(P => CloseRequested?.Invoke(this, new DialogCloseRequestEventArgs(true)));
            CancelCommand = new RelayCommand(P => CloseRequested?.Invoke(this, new DialogCloseRequestEventArgs(false)));

            UpDownCommand = new RelayCommand(UpDownCommand_Execute, CanUpDownExecute);
            CopyCommand = new RelayCommand(CopyCommand_Execute, CanCopyExecute);

            this.data = data;
            RectangleItems = new ObservableCollection<Rectangle>();
            LineItems = new ObservableCollection<Line>();
            TextItems = new ObservableCollection<ChartText>();

            EfficiencyMeasurements.CollectionChanged += EfficiencyMeasurements_CollectionChanged;
            WriteEfficiencyEquation();

        }


        #region properties
        public ICommand OkCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand UpDownCommand { get; }


        private ObservableCollection<Rectangle> _rectangleItems;
        /// <summary>
        /// Collection of the rectangles in the chart
        /// </summary>
        public ObservableCollection<Rectangle> RectangleItems
        {
            get { return _rectangleItems; }
            set { _rectangleItems = value; }
        }

        private ObservableCollection<Line> _lineItems;
        /// <summary>
        /// Collection of the lines in the chart
        /// </summary>
        public ObservableCollection<Line> LineItems
        {
            get { return _lineItems; }
            set { _lineItems = value; }
        }

        private ObservableCollection<ChartText> _textItems;
        /// <summary>
        /// Collection of the text in the chart
        /// </summary>
        public ObservableCollection<ChartText> TextItems
        {
            get { return _textItems; }
            set { _textItems = value; }
        }

        private PointCollection _curvePoints;
        /// <summary>
        /// The points on the fitted curve
        /// </summary>
        public PointCollection CurvePoints
        {
            get { return _curvePoints; }
            set
            {
                _curvePoints = value;
                OnPropertyChanged("CurvePoints");
            }
        }
        /// <summary>
        /// collectionof efficiency measurments
        /// </summary>
        public ObservableEntityCollection<EfficiencyMeasurement> EfficiencyMeasurements
        {
            get { return data.EfficiencyPoints; }

            set
            {
                OnPropertyChanged("EfficiencyMeasurements");
                DrawGraph();
                //WriteEfficiencyEquation();
            }
        }

        private string _efficiencyEquation;
        /// <summary>
        /// String representing the efficiency equation
        /// </summary>
        public string EfficiencyEquation
        {
            get { return _efficiencyEquation; }
            set
            {
                _efficiencyEquation = value;
                OnPropertyChanged("EfficiencyEquation");
            }
        }
        /// <summary>
        /// The available efficiency models
        /// </summary>
        public IEnumerable<SpectralData.CalibrationModel> EfficiencyModels
        {
            get { return Enum.GetValues(typeof(SpectralData.CalibrationModel)).Cast<SpectralData.CalibrationModel>(); }

        }
        /// <summary>
        /// The calibration model in use
        /// </summary>
        public SpectralData.CalibrationModel CurrentModel
        {
            get { return data.CalModel; }
            set
            {
                data.CalModel = value;
                OnPropertyChanged("CurrentModel");
                DrawGraph();
                //WriteEfficiencyEquation();
            }
        }
        /// <summary>
        /// Order of the calibration equation
        /// </summary>
        public int Order
        {
            get { return data.Order; }
            set
            {
                data.Order = value;
                OnPropertyChanged("Order");
                DrawGraph();

            }
        }

        private double _canvasWidth;
        /// <summary>
        /// Width of the avaliable drawing space
        /// </summary>
        public double CanvasWidth
        {
            private get { return _canvasWidth; }
            set
            {
                _canvasWidth = value;
                OnPropertyChanged("CanvasWidth");
                DrawGraph();
            }
        }

        private double _canvasHeight;
        /// <summary>
        /// Height of the avaliable drawing space
        /// </summary>
        public double CanvasHeight
        {
            private get { return _canvasHeight; }
            set
            {
                _canvasHeight = value;
                OnPropertyChanged("CanvasWidth");
                DrawGraph();
            }
        }
        public DateTime SampleDate
        {
            get { return data.CollectionTime; }
            set { 
                data.CollectionTime = value;
                OnPropertyChanged("SampleDate");
            }
        }

        #endregion
        /// <summary>
        /// Can the UpDownCommand execute
        /// </summary>
        /// <param name="obj">Command paramter</param>
        /// <returns></returns>
        private bool CanUpDownExecute(object obj)
        {
            if (obj.ToString().Equals("up"))
                return Order < data.EfficiencyPoints.Count-1;
            else if (obj.ToString().Equals("down"))
                return Order > 2;
            return false;
        }
        /// <summary>
        /// Exectute the UpDownCommand
        /// </summary>
        /// <param name="obj">Command Paramter: string either 'up' or 'down'</param>
        private void UpDownCommand_Execute(object obj)
        {
            if (obj.ToString().Equals("up"))
                Order++;
            else if (obj.ToString().Equals("down"))
                Order--;
        }
        /// <summary>
        /// Check to see if data can be copied
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        private bool CanCopyExecute(object obj)
        {
            string param = obj.ToString().ToLowerInvariant();
            if (param.Contains("coefficients"))
            {
                return data.CalibrationParams != null && data.CalibrationParams.Count > 0;
            }
            else if (param.Contains("points")) 
            {
                return data.EfficiencyPoints != null && data.EfficiencyPoints.Count > 0;
            }
            else 
            {
                return (data.CalibrationParams != null && data.CalibrationParams.Count > 0) && 
                    (data.EfficiencyPoints != null&& data.EfficiencyPoints.Count > 0);
            }
        }
        /// <summary>
        /// Copy data to the clipboard
        /// </summary>
        /// <param name="obj"></param>
        private void CopyCommand_Execute(object obj)
        {
            string param = obj.ToString().ToLowerInvariant();
            if (param.Contains("coefficients"))
            {
                StringBuilder copyString = new StringBuilder();
                foreach (double calParam in this.data.CalibrationParams)
                {
                    copyString.AppendLine(calParam.ToString());
                }
                Clipboard.SetText(copyString.ToString());
            }
            else if (param.Contains("points"))
            {
                StringBuilder copyString = new StringBuilder();
                foreach (EfficiencyMeasurement measurement in this.data.EfficiencyPoints)
                {
                    copyString.AppendLine($"{measurement.Energy}\t{measurement.Efficiency}\t{measurement.EfficiencyUncertainty}");
                }
                Clipboard.SetText(copyString.ToString());
            }
            else
            {
                StringBuilder copyString = new StringBuilder();
                foreach (double calParam in this.data.CalibrationParams)
                {
                    copyString.Append($"{calParam}\t");
                }
                copyString.Append(Environment.NewLine);
                foreach (EfficiencyMeasurement measurement in this.data.EfficiencyPoints)
                {
                    copyString.AppendLine($"{measurement.Energy}\t{measurement.Efficiency}\t{measurement.EfficiencyUncertainty}");
                }
                Clipboard.SetText(copyString.ToString());

            }
        }

        /// <summary>
        /// Event handlert when the efficiency measurment collection changed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void EfficiencyMeasurements_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            DrawGraph();
            //WriteEfficiencyEquation();
        }
        /// <summary>
        /// Generates the efficiency equaiton string
        /// </summary>
        private void WriteEfficiencyEquation() 
        {
            if (data.CalibrationParams == null || data.CalibrationParams.Count < 1)
                return;
            StringBuilder eqBuilder = new StringBuilder();
            switch (data.CalModel) 
            {
                case (SpectralData.CalibrationModel.Linear):
                    eqBuilder.Append(String.Format("log(ε) = {0:e6} • E", data.CalibrationParams[0]));
                    if(data.CalibrationParams.Count > 1)
                        eqBuilder.Append(String.Format(" + {0:e6}", data.CalibrationParams[1]));
                    for (int i = 2; i < data.Order; i++)
                    {
                        string pow;
                        if (i == 2)
                            pow = Char.Parse("\xB2").ToString();
                        else if (i == 3)
                            pow = Char.Parse("\xB3").ToString();
                        else
                        {
                            string t_pow = $"207{i}";
                            pow = char.ConvertFromUtf32(int.Parse(t_pow, System.Globalization.NumberStyles.HexNumber));
                        }
                        eqBuilder.Append(String.Format(" + {0:e6} • (1/E){1}", data.CalibrationParams[i],pow));
                    }
                    break;
                case (SpectralData.CalibrationModel.Natural):
                    eqBuilder.Append(String.Format("ln(ε) = {0:e6}", data.CalibrationParams[0]));
                    for (int i = 1; i < data.Order; i++)
                    {
                        string pow;
                        if (i == 1)
                            pow = Char.Parse("\xB9").ToString();
                        else if (i == 2)
                            pow = Char.Parse("\xB2").ToString();
                        else if (i == 3)
                            pow = Char.Parse("\xB3").ToString();
                        else
                        {
                            string t_pow = $"207{i}";
                            pow = char.ConvertFromUtf32(int.Parse(t_pow, System.Globalization.NumberStyles.HexNumber));
                        }
                        eqBuilder.Append(String.Format(" + {0:E6} • ln(E){1}", data.CalibrationParams[i], pow));
                    }
                    break;
                case (SpectralData.CalibrationModel.Emperical):
                    double num  = (0+ Properties.Settings.Default.UPPERENERGY)/2;
                    eqBuilder.Append(String.Format("ln(ε) = {0:e6}", data.CalibrationParams[0]));
                    for (int i = 1; i < data.Order; i++)
                    {
                        string pow;
                        if (i == 1)
                            pow = Char.Parse("\xB9").ToString();
                        else if (i == 2)
                            pow = Char.Parse("\xB2").ToString();
                        else if (i == 3)
                            pow = Char.Parse("\xB3").ToString();
                        else
                        {
                            string t_pow = $"207{i}";
                            pow = char.ConvertFromUtf32(int.Parse(t_pow, System.Globalization.NumberStyles.HexNumber));
                        }
                        eqBuilder.Append(String.Format(" + {0:E6}• ({1:e2}/E){2}", data.CalibrationParams[i],num, pow));
                    }
                    break;
            }
            EfficiencyEquation = eqBuilder.ToString();
        }


        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
        /// <summary>
        /// Draws the graph
        /// </summary>
        private void DrawGraph()
        {


            if (CanvasHeight == 0 || CanvasWidth == 0)
                return;

            //clear the containers
            RectangleItems.Clear();
            LineItems.Clear();
            TextItems.Clear();
            PointCollection points = new PointCollection();

            //set some stuff
            int numPoints = 100;
            int ticks = 8;

            //get the max an min of the efficieny points
            double maxOrd = EfficiencyMeasurements.Max(e => e.Efficiency);
            double minOrd = EfficiencyMeasurements.Min(e => e.Efficiency);

            //get ordinate graph bounds that look nice
            double ordTickRange = GetTickRange(maxOrd - minOrd, ticks);
            double ordTickMag = Math.Log10(ordTickRange) > 0 ? Math.Ceiling(Math.Log10(ordTickRange)) : Math.Floor(Math.Log10(ordTickRange));
            maxOrd = ordTickRange * Math.Round(1 + maxOrd / ordTickRange);
            minOrd = ordTickRange * (Math.Round(minOrd / ordTickRange - 1));

            double minAbsc = 0;// Properties.Settings.Default.LOWERELIMT * 0.75;
            double maxAbsc = Properties.Settings.Default.UPPERENERGY;

            //get abscissa graph bounds that look nice
            double abscTickRange = GetTickRange(maxAbsc - minAbsc, ticks);
            double abscTickMag = Math.Log10(abscTickRange) > 0 ? Math.Ceiling(Math.Log10(abscTickRange)) : Math.Floor(Math.Log10(abscTickRange));
            maxAbsc = abscTickRange * Math.Round(maxAbsc / abscTickRange);
            minAbsc = abscTickRange * (Math.Round(minAbsc / abscTickRange));


            int seperation = 20;
            Rectangle chart = new Rectangle(0.2*CanvasWidth-seperation, seperation, (int)(0.8F * CanvasWidth), (int)(0.8F * CanvasHeight));
            //Rectangle chart = new Rectangle();
            RectangleItems.Add(chart);

            //get the energy limit
            double lowE = minAbsc;// Properties.Settings.Default.LOWERELIMT * 0.75;
            double highE = maxAbsc;

            //get the position converstion factors.
            double xconv = chart.Width / (highE - lowE );
            double xoff = chart.Right - (xconv * highE);

            double yconv = chart.Height / (minOrd - maxOrd);
            double yoff = chart.Bottom - (yconv * minOrd);

            
            //build a formatting string
            StringBuilder format = new StringBuilder("0");
            format.Append(ordTickMag >= 0 ? "" : ".");
            for (int j = 0; j < Math.Abs(ordTickMag); j++)
                format.Append(0);
            //loop through and add lables
            int tickLength = 10;
            double ord = minOrd, absc = minAbsc;
            
            while (Math.Round(ord, (int)Math.Abs(ordTickMag)) <= maxOrd)
            {
                //draw the y-axis
                double y;// = (yconv * (ord) + yoff);
                if (Math.Abs(ord - minOrd) < 1e-6)
                    y = chart.Bottom - chart.StrokeWeight/2;
                else if (Math.Abs(ord- maxOrd) < 1e-6)
                    y = chart.Top + chart.StrokeWeight / 2;
                else
                {
                    y = (yconv * (ord) + yoff);
                    LineItems.Add(new Line(chart.Left, y, chart.Right, y)
                    {
                        LineDashArray = { 2, 1 },
                    });
                }
              
                LineItems.Add(new Line(chart.Left - tickLength, y, chart.Left + tickLength, y));
                //create the label
                string label = (ord).ToString(format.ToString());
                ChartText labelText = new ChartText(label)
                {
                    //position the label
                    //X = chart.Left - 2 - 0.2*CanvasWidth,
                    X =0,
                    Y = y + (ordTickRange * yconv)/2,
                    Width = chart.Left-tickLength-2,
                    Height = Math.Abs(ordTickRange*yconv),
                    TextHorizontalAlignment = HorizontalAlignment.Right,
                    TextVerticalAlignment = VerticalAlignment.Center
                };
            
                TextItems.Add(labelText);
                ord += ordTickRange;
            }
            while (Math.Round(absc, (int)Math.Abs(abscTickMag)) <= maxAbsc)
            {
                //draw the x-axis
                double x;// = (xconv * (absc) + xoff);
                if (Math.Abs(absc- minAbsc) < 1e-6)
                    x = chart.Left+ chart.StrokeWeight / 2;
                else if (Math.Abs(absc- maxAbsc ) < 1e-6)
                    x = chart.Right - chart.StrokeWeight / 2;
                else
                {
                    x = (xconv * (absc) + xoff);
                    LineItems.Add(new Line(x, chart.Bottom, x, chart.Top)
                    {
                        LineDashArray = { 2, 1 },
                    });
                }

                LineItems.Add(new Line(x, chart.Bottom + tickLength, x, chart.Bottom - tickLength));

                string label = (absc).ToString();
                ChartText labelText = new ChartText(label)
                {
                    Width = abscTickRange * xconv,
                    Height = 0.2 * CanvasHeight-tickLength- 2 - seperation,
                    //position the label
                    X = x- (abscTickRange * xconv) / 2,
                    Y = chart.Bottom + tickLength + 2,
                    TextHorizontalAlignment = HorizontalAlignment.Center,
                    TextVerticalAlignment = VerticalAlignment.Top
                };
            
                TextItems.Add(labelText);
                absc += abscTickRange;
            }
            
            //fill the an array of points that represents the curve
            double eJump = (highE - lowE) / numPoints;
            //CurvePoints = new Point[numPoints];
            double ene = lowE;
            for (int i = 0; i < numPoints; i++)
            {
                ene += eJump;
                double eff = data.GetEfficiency(ene);
                int x = Convert.ToInt32(ene * xconv + xoff);
                double y = yoff + eff * yconv;
                if (y > chart.Bottom || double.IsNaN(y))
                    y = chart.Bottom;
                else if (y < chart.Top || double.IsInfinity(y))
                    y = chart.Top;

                points.Add(new Point(x, y));

            }
            this.CurvePoints = points;
            //make the graph thick
            //put the efficiency points on the chart
            for (int i = 0; i < EfficiencyMeasurements.Count; i++)
            {
                int x = Convert.ToInt32(EfficiencyMeasurements[i].Energy * xconv + xoff);
                int y = Convert.ToInt32(yoff + EfficiencyMeasurements[i].Efficiency * yconv);
                int ptSize = 2;
                RectangleItems.Add(new Rectangle(x - ptSize, y - ptSize, 2 * ptSize, 2 * ptSize));
                //add the error bars
                LineItems.Add(new Line(x, y - EfficiencyMeasurements[i].EfficiencyUncertainty * yconv,
                    x, y + EfficiencyMeasurements[i].EfficiencyUncertainty * yconv));

            }
            WriteEfficiencyEquation();
        }
        /// <summary>
        /// Compute the range between tickes
        /// </summary>
        /// <param name="range">data range</param>
        /// <param name="ticks">number of ticks</param>
        /// <returns>space between ticks in chart cooridinates</returns>
        private static double GetTickRange(double range, int ticks)
        {
            double unrndTickSize = (range) / (ticks - 1);
            double tickRange = Math.Pow(10, Math.Ceiling(Math.Log10(unrndTickSize) - 1));
            return tickRange == 0 ? 1 : (Math.Ceiling(unrndTickSize / tickRange) * tickRange);
        }


    }
 

}
