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
using System.Windows.Forms;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel;

namespace PeakMap
{
    public partial class Settings : Form
    {
        /// <summary>
        /// Load with Line mathc settings
        /// </summary>
        /// <param name="lineMatch"></param>
        public Settings(LineMatch lineMatch)
        {
            InitializeComponent();
            //propertyGrid.AutoScaleMode = AutoScaleMode.Font;
            propertyGrid.SelectedObject = lineMatch;
        }
        /// <summary>
        /// Load with spectralData
        /// </summary>
        /// <param name="data"></param>
        public Settings(SpectralData data)
        {
            InitializeComponent();
            propertyGrid.AutoScaleMode = AutoScaleMode.Font;
            propertyGrid.SelectedObject = data;
            SetupWindowForCalGraph();
            data.CalibrationParamtersRefreshed += Data_CalibrationParamtersRefreshed;
        }

        public Settings(SpectralLibraryGenerator lib)
        {
            InitializeComponent();
            propertyGrid.AutoScaleMode = AutoScaleMode.Font;
            propertyGrid.SelectedObject = lib;
        }

        private void SetupWindowForCalGraph() 
        {
            
            //double the height to make room for the graph
            int gridBottom = (propertyGrid.Location.Y + propertyGrid.Height);
            int seperation = closeBtn.Location.Y - gridBottom;
            this.Height += canvas.Height + 2*seperation;
            canvas.Location = new Point(canvas.Location.X, gridBottom + seperation);
            
            closeBtn.Location = new Point(closeBtn.Location.X, 2*seperation + gridBottom + canvas.Height);
            canvas.Anchor =  AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            propertyGrid.Anchor = AnchorStyles.Bottom | AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            //DrawGraph();
        }
        /// <summary>
        /// Handle the propertyu value changed for cal model
        /// </summary>
        /// <param name="s"></param>
        /// <param name="e"></param>
        private void PropertyGrid_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            if (e.ChangedItem.PropertyDescriptor.Name == "CalModel" || e.ChangedItem.PropertyDescriptor.Name == "Order")
            {
                SpectralData data = (SpectralData)((PropertyGrid)s).SelectedObject;
                try
                {
                    data.RefreshCalibrationParameters();
                }
                catch (Exception ex) 
                {
                    MessageBox.Show(ex.Message, "Exception", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
        }

        private void Data_CalibrationParamtersRefreshed(object sender, EventArgs e)
        {
            DrawGraph();
        }

        private void DrawGraph()
        {
            if (!(propertyGrid.SelectedObject is SpectralData))
                return;

            SpectralData data = (SpectralData) propertyGrid.SelectedObject;

            EfficiencyMeasurement[] effpts = data.EfficiencyPoints.ToArray();

            int numPoints = 500;
            int ticks = 10;
            canvas.BackColor = Color.LightGray;
            //get the max an min of the efficieny points
            double maxOrd = effpts.Max(e => e.Efficiency);
            double minOrd = effpts.Min(e => e.Efficiency);

            //get ordinate graph bounds that look nice
            double ordTickRange = GetTickRange(maxOrd - minOrd, ticks);
            double ordTickMag = Math.Log10(ordTickRange) > 0 ? Math.Ceiling(Math.Log10(ordTickRange)): Math.Floor(Math.Log10(ordTickRange));
            maxOrd = ordTickRange * Math.Round(1+maxOrd/ordTickRange);
            minOrd =  ordTickRange * (Math.Round(minOrd / ordTickRange - 1));

            double minAbsc = 0;// Properties.Settings.Default.LOWERELIMT * 0.75;
            double maxAbsc = Properties.Settings.Default.UPPERELIMIT;

            //get abscissa graph bounds that look nice
            double abscTickRange = GetTickRange(maxAbsc - minAbsc, ticks);
            double abscTickMag = Math.Log10(abscTickRange) > 0 ? Math.Ceiling(Math.Log10(abscTickRange)) : Math.Floor(Math.Log10(abscTickRange));
            maxAbsc = abscTickRange * Math.Round(maxAbsc / abscTickRange);
            minAbsc = abscTickRange * (Math.Round(minAbsc / abscTickRange));

            Graphics graph = canvas.CreateGraphics();
            graph.Clear(canvas.BackColor);
            Pen pen = new Pen(Color.DarkSlateGray);
            int seperation = (canvas.Location.Y - (propertyGrid.Location.Y + propertyGrid.Height))*2;
            Rectangle chart = new Rectangle(7 * seperation, seperation, (int)(0.8F * canvas.Width), (int)(0.8F * canvas.Height));
            //graph.DrawRectangle(pen, chart);


            //get the energy limit
            double lowE = minAbsc;// Properties.Settings.Default.LOWERELIMT * 0.75;
            double highE = maxAbsc;
            
            //get the position converstion factors.
            double xconv = (chart.Left - chart.Right) / (lowE- highE);
            double xoff = chart.Right - (xconv * highE);

            double yconv = (chart.Bottom  - chart.Top ) / (minOrd - maxOrd);
            double yoff = chart.Bottom - (yconv * minOrd);

            //build a formatting string
            StringBuilder format = new StringBuilder("0");
            format.Append(ordTickMag >= 0 ? "" : ".");
            for (int j = 0; j < Math.Abs(ordTickMag); j++)
                format.Append(0);
            //loop throuhg and add lables
            int tickLength = seperation;
            double ord = minOrd, absc = minAbsc;
            while (Math.Round(ord,(int)Math.Abs(ordTickMag)) <= maxOrd)
            {
                //draw the y-axis
                int y = (int)(yconv * (ord) + yoff);
                graph.DrawLine(pen, chart.Left - tickLength, y, chart.Right, y);
                string label = (ord).ToString(format.ToString());
                SizeF labelSize = graph.MeasureString(label, propertyGrid.Font);
                graph.DrawString(label, propertyGrid.Font, pen.Brush, new PointF(chart.Left - tickLength - labelSize.Width, y - labelSize.Height / 2));
                ord += ordTickRange;
            }
            while (Math.Round(absc, (int)Math.Abs(abscTickMag)) <= maxAbsc) { 
                //draw the x-axis
                int x = (int)(xconv * (absc) + xoff);
                graph.DrawLine(pen, x, chart.Bottom + tickLength, x, chart.Top);
                string label = (absc).ToString();
                SizeF labelSize = graph.MeasureString(label, propertyGrid.Font);
                graph.DrawString(label, propertyGrid.Font, pen.Brush, new PointF(x - labelSize.Width / 2, chart.Bottom + labelSize.Height));
                absc += abscTickRange;
            }
            //fill the an array of points that represents the curve
            double eJump = (highE - lowE) / numPoints;
            Point[] curvePoints = new Point[numPoints];
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


                curvePoints[i] = new Point(x,(int)y);
            }
            //make the graph thick
            Pen graphPen = pen;
            graphPen.Width *= 3;
            graph.DrawCurve(graphPen, curvePoints);

            //put the efficiency points on the chart
            for (int i = 0; i < effpts.Length; i++)
            {
                int x = Convert.ToInt32(effpts[i].Energy * xconv + xoff);
                int y = Convert.ToInt32(yoff + effpts[i].Efficiency * yconv);
                int ptSize = Convert.ToInt32(graphPen.Width);
                graph.DrawRectangle(graphPen, new Rectangle(x - ptSize, y - ptSize, 2 * ptSize, 2 * ptSize));
            }
        }
        private static double GetTickRange(double range, int ticks) 
        {
            double unrndTickSize = (range) / (ticks - 1);
            double tickRange = Math.Pow(10, Math.Ceiling(Math.Log10(unrndTickSize) - 1));
            return tickRange == 0? 1 : (Math.Ceiling(unrndTickSize / tickRange) * tickRange);
        } 
        /// <summary>
        /// Close the form when close button is pressed
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CloseBtn_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void Canvas_Paint(object sender, PaintEventArgs e)
        {
            DrawGraph();
        }

        private void Canvas_Resize(object sender, EventArgs e)
        {
            DrawGraph();
        }
    }
}
