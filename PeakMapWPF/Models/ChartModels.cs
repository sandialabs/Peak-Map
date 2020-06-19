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
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace PeakMapWPF.Models
{
    public class Rectangle
    {
        public double X { get; set; }
        public double Y { get; set; }

        public double Width { get; set; }
        public double Height { get; set; }

        public double Left { get { return X; } }
        public double Right { get { return X + Width; } }
        public double Top { get { return Y; } }
        public double Bottom { get { return Y + Height; } }

        private double _strokeWeight;

        public double StrokeWeight
        {
            get { return _strokeWeight; }
            set { _strokeWeight = value; }
        }


        private Brush _lineStroke;

        public Brush LineStroke
        {
            get { return _lineStroke; }
            set { _lineStroke = value; }
        }


        public Rectangle(double x, double y, double width, double height)
        {
            X = x;
            Y = y;
            Width = width;
            Height = height;

            _lineStroke = new SolidColorBrush()
            {
                Color = Colors.Black
            };

            _strokeWeight = 1;
        }
    }

    public class Line
    {
        private DoubleCollection _lineDashArray;

        public DoubleCollection LineDashArray
        {
            get { return _lineDashArray; }
            set { _lineDashArray = value; }
        }

        private Brush _lineStroke;

        public Brush LineStroke
        {
            get { return _lineStroke; }
            set { _lineStroke = value; }
        }

        private double _strokeWeight;

        public double StrokeWeight
        {
            get { return _strokeWeight; }
            set { _strokeWeight = value; }
        }

        public double X1 { get; set; }
        public double X2 { get; set; }

        public double Y1 { get; set; }
        public double Y2 { get; set; }


        public Line(double x1, double y1, double x2, double y2)
        {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;

            _lineStroke = new SolidColorBrush()
            {
                Color = Colors.Black
            };

            _lineDashArray = new DoubleCollection();
            _strokeWeight = 1;
        }
    }

    public class ChartText : INotifyPropertyChanged
    {
        public string Text { get; set; }

        private HorizontalAlignment _textHorizontalAlignment;

        public HorizontalAlignment TextHorizontalAlignment
        {
            get { return _textHorizontalAlignment; }
            set { _textHorizontalAlignment = value;
                OnPropertyChanged("TextHorizontalAlignment");
            }
        }

        private VerticalAlignment _textVerticalAlignment;

        public VerticalAlignment TextVerticalAlignment
        {
            get { return _textVerticalAlignment; }
            set { _textVerticalAlignment = value;
                OnPropertyChanged("TextVerticalAlignment");
            }
        }

        private double _x;
        public double X 
        { 
            get { return _x; }
            set 
            {
                _x = value;
                OnPropertyChanged("X");
            } 
        }
        private double _y;
        public double Y 
        {
            get { return _y; }
            set
            {
                _y = value;
                OnPropertyChanged("Y");
            }
        }

        private double _width;
        public double Width {
            get { return _width; }
            set 
            {
                _width = value;
                OnPropertyChanged("Width");

            } 
        }
        private double _height;

        public event PropertyChangedEventHandler PropertyChanged;

        public double Height {
            get { return _height; }
            set
            {
                _height = value;
                OnPropertyChanged("Height");

            }
        }
       
        public ChartText(string text)
        {
            Text = text;
            _textHorizontalAlignment = HorizontalAlignment.Left;
            _textVerticalAlignment = VerticalAlignment.Top;
        }
        public ChartText(string text, HorizontalAlignment horizontalAlignment, VerticalAlignment verticalAlignment)
        {
            Text = text;
            _textHorizontalAlignment = horizontalAlignment;
            _textVerticalAlignment = verticalAlignment;
            //FormattedText labelSize = new FormattedText(label, Thread.CurrentThread.CurrentCulture, TextFlowDirection, typeface, FontSize, Foreground, PixelsPerDip);


        }

        // Create the OnPropertyChanged method to raise the event
        // The calling member's name will be used as the parameter.
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }
   
}