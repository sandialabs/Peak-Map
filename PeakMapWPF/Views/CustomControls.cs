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
using System.Collections;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using System.Data;

namespace PeakMapWPF.Views
{
    class CustomDataGrid : DataGrid , INotifyPropertyChanged
    {

        public CustomDataGrid()
        {
            this.SelectionChanged += CustomDataGrid_SelectionChanged;
        }


        /// <summary>
        /// Selection changed event handler
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void CustomDataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetCurrentValue(SelectedItemsListProperty, SelectedItems);

        }

        //create the dependencyProperty
        public static readonly DependencyProperty SelectedItemsListProperty =
            DependencyProperty.Register("SelectedItemsList", typeof(IList), typeof(CustomDataGrid), 
                new PropertyMetadata(OnSelectedItemsChanged));


        private static void OnSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CustomDataGrid customDataGrid = d as CustomDataGrid;
            customDataGrid.OnPropertyChanged("SelectedItems");
            customDataGrid.OnSelectedItemsChanged(e);
        }

        private void OnSelectedItemsChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
                return;

            IEnumerable newVals = ((IEnumerable)e.NewValue).OfType<Object>().ToArray();

            foreach (var item in newVals)
            {
                SelectedItems.Add(item);
            }

            SetCurrentValue(SelectedItemsListProperty, SelectedItems);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged(string propertyName) 
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        //Selected Items List
        public IList SelectedItemsList
        {
            get{ return (IList)this.GetValue(SelectedItemsListProperty); }
            set{ this.SetValue(SelectedItemsListProperty, value); }
        }
    }
}
