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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using System.Data;

namespace CAMInputOutput
{
    public struct EfficiencyPoint
    {
        public int Index;
        public double Energy;
        public double Efficiency;
        public double EfficiencyUncertainty;
    }

    public struct Peak
    {
        public double Energy;
        public double Centroid;
        public double CentroidUncertainty;
        public double FullWidthAtHalfMaximum;
        public double LowTail;
        public double Area;
        public double AreaUncertainty;
        public double Continuum;
        public double CriticalLevel;
        public double CountRate;
        public double CountRateUncertainty;

        /// <summary>
        /// Peak Object
        /// </summary>
        /// <param name="energy">Energy of the peak</param>
        /// <param name="centrd">Centroid of the peak</param>
        /// <param name="centrdUnc">Uncertainty in the centroid</param>
        /// <param name="fwhm">Wdith of the peak</param>
        /// <param name="lowTail">Low tail of the peak</param>
        /// <param name="area">Area if the peak</param>
        /// <param name="areaUnc">Uncertainty in the area of the peak</param>
        /// <param name="continuum">Continuum under the peak</param>
        /// <param name="critialLevel">The peak critical level</param>
        /// <param name="cntRate">The count rate of the peak in cps</param>
        /// <param name="cntRateUnc">Uncertainty in the count rate</param>
        public Peak(double energy, double centrd, double centrdUnc, double fwhm, double lowTail, double area, double areaUnc, double continuum,double critialLevel, double cntRate, double cntRateUnc) 
        {
            Energy = energy;
            Centroid = centrd;
            CentroidUncertainty = centrdUnc;
            FullWidthAtHalfMaximum = fwhm;
            LowTail = lowTail;
            Area = area;
            AreaUncertainty = areaUnc;
            Continuum = continuum;
            CriticalLevel = critialLevel;
            CountRate = cntRate;
            CountRateUncertainty = cntRateUnc;
        }
    }
    public struct Nuclide
    {
        public string Name;
        public double HalfLife;
        public double HalfLifeUncertainty;
        public string HalfLifeUnit;
        public int Index;

        /// <summary>
        /// Nuclide object
        /// </summary>
        /// <param name="name">Name of the nuclide</param>
        /// <param name="halfLife">Half-Life</param>
        /// <param name="halfLifeUnc">Half-Life Uncertaitny</param>
        /// <param name="halfLifeUnit">HalfLifeUnit</param>
        /// <param name="nucNo">The nuclide record number</param>
        public Nuclide(string name, double halfLife, double halfLifeUnc, string halfLifeUnit, int nucNo) 
        { 
            Name = name;
            HalfLife = halfLife;
            HalfLifeUncertainty = halfLifeUnc;
            HalfLifeUnit = halfLifeUnit;
            Index = nucNo;
        }
    }
    public struct Line
    {
        public double Energy;
        public double EnergyUncertainty;
        public double Abundance;
        public double AbundanceUncertainty;
        public bool IsKeyLine;
        public int NuclideIndex;

        /// <summary>
        /// Line Object
        /// </summary>
        /// <param name="energy">The line energy in keV</param>
        /// <param name="energyUnc">The line energy uncertainty in keV</param>
        /// <param name="abundance">The line yield in percent</param>
        /// <param name="aundanceUnc">The line yield uncertainty in percent</param>
        /// <param name="nucNo">The nuclide number associated with line</param>
        /// <param name="key">true if the line is the key line</param>
        public Line(double energy, double energyUnc, double abundance, double aundanceUnc, int nucNo, bool key = false) 
        {
            Energy = energy;
            EnergyUncertainty = energyUnc;
            Abundance = abundance;
            AbundanceUncertainty = aundanceUnc;
            IsKeyLine = key;
            NuclideIndex = nucNo;
        }
    }

    public class CAMIO
    {
        protected enum CAMBlock
        {
            ACQP = 0x00012000,
            SAMP = 0x00012001,
            GEOM = 0x00012002,
            PROC = 0x00012003,
            DISP = 0x00012004,
            SPEC = 0x00012005,
            PEAK = 0x00012006,
            NUCL = 0x00012007,
            NLINES = 0x00012008,
            //PTCALIB,        
            //INTERF,
            //INTRES,
            //MOREPEAK,
            //ANALCNTL,
            //CALRESULTS,
            //CERTIF,
            //SHAPECALRES,
            //SPECIAL
        }
        protected enum RecordSize
        {
            ACQP = 0x051C,
            NUCL = 0x0237,
            NLINES = 0x0085
        }

        protected enum BlockSize
        {
            ACQP = (int)0xA00U,
            PROC = (int)0x800,
            NUCL = (int)0x4800U,
            NLINES = (int)0x4200U
        }
        protected enum PeakParameterLocation
        {
            Energy = 0x0,
            Centroid = 0x40,
            CentroidUncertainty = 0x40,
            FullWidthAtHalfMaximum = 0x10,
            LowTail = 0x50,
            Area = 0x34,
            AreaUncertainty = 0x84,
            Continuum = 0x0C,
            CriticalLevel = 0x0D1,
            CountRate = 0x18,
            CountRateUncertainty = 0x1C
        }
        protected enum NuclideParameterLocation
        {
            Name = 0x03,
            HalfLife = 0x1B,
            HalfLifeUncertainty = 0x89,
            HalfLifeUnit = 0x61
        }
        protected enum LineParameterLocation
        {
            Energy = 0x01,
            EnergyUncertainty = 0x21,
            Abundance = 0x05,
            AbundanceUncertainty = 0x39,
            IsKeyLine = 0x1D,
            NuclideIndex = 0x1B

        }
        protected enum EfficiencyPointParameterLocation 
        {
            Energy = 0x01,
            Efficiency = 0x05,
            EfficiencyUncertainty = 0x09
        }

         Lookup<CAMBlock, UInt32> blockAddresses;
         byte[] readData;

        private const UInt16 headerSize = 0x800;
        private const UInt16 blockHeaderSize = 0x30;

        IList<byte[]> lines;
        IList<byte[]> nucs;

        IList<Line> fileLines;
        IList<Nuclide> fileNuclides;

        public IList<Line> Lines
        { 
            get {
                if (fileLines != null)
                    return fileLines;
                else
                    return null;
            } 
        }
        public IList<Line> Nuclides
        {
            get
            {
                if (fileNuclides != null)
                    return fileLines;
                else
                    return null;
            }
        }

        #region defaultBytes
        private readonly byte[] fileHeader = {
            0x00, 0x04, 0x00, 0x00, 0x00, 0x04, 0x00, 0x08, 0x00, 0x00, 0x00, 0xA4, 0x00, 0x00, 0x00, 0x00,
            0x30, 0x00, 0x29, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
        private readonly byte[] nuclCommon = {
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x75, 0x43, 0x69, 0x20,
            0x20, 0x20, 0x20, 0x20, 0x75, 0x43, 0x69, 0x20, 0x20, 0x20, 0x20, 0x20, 0x75, 0x43, 0x69, 0x20,
            0x20, 0x20, 0x20, 0x20, 0x75, 0x43, 0x69, 0x20, 0x20, 0x20, 0x20, 0x20, 0x75, 0x43, 0x69, 0x20,
            0x20, 0x20, 0x20, 0x20, 0x75, 0x43, 0x69, 0x20, 0x20, 0x20, 0x20, 0x20, 0x75, 0x43, 0x69, 0x20,
            0x20, 0x20, 0x20, 0x20, 0x75, 0x43, 0x69, 0x20, 0x20, 0x20, 0x20, 0x20, 0x63, 0x6D, 0x33, 0x20,
            0x20, 0x20, 0x20, 0x20, 0x63, 0x6D, 0x33, 0x20, 0x20, 0x20, 0x20, 0x20, 0x63, 0x6D, 0x33, 0x20,
            0x20, 0x20, 0x20, 0x20, 0x63, 0x6D, 0x33, 0x20, 0x20, 0x20, 0x20, 0x20, 0x63, 0x6D, 0x33, 0x20,
            0x20, 0x20, 0x20, 0x20, 0x63, 0x6D, 0x33, 0x20, 0x20, 0x20, 0x20, 0x20, 0x63, 0x6D, 0x33, 0x20,
            0x20, 0x20, 0x20, 0x20, 0x63, 0x6D, 0x33, 0x20, 0x20, 0x20, 0x20, 0x20, 0x80, 0x40, 0x00, 0x00,
            0x80, 0x40, 0x00, 0x00, 0x80, 0x40, 0x00, 0x00, 0x80, 0x40, 0x00, 0x00, 0x80, 0x40, 0x00, 0x00,
            0x80, 0x40, 0x00, 0x00, 0x80, 0x40, 0x00, 0x00, 0x80, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x75, 0x43, 0x69, 0x20, 0x20, 0x20, 0x20, 0x20, 0x63, 0x6D, 0x33, 0x20,
            0x20, 0x20, 0x20, 0x20, 0x80, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x22, 0x22, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x22, 0x22, 0x20, 0x20, 0x20, 0x20,
            0x20, 0x20, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00
 };
        private readonly byte[] nlineCommon = {
            0x6B, 0x65, 0x56, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20,
            0x80, 0x40, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00};
        private readonly byte[] acqpCommon = {
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x30, 0x00, 0x90, 0x1C, 0x0E, 0x00, 0x00, 0xBE, 0x01, 0x00, 0x3C, 0x00, 0x00, 0x00, 0x00, 0x00,
            0x00, 0x00, 0xFF, 0x7F, 0xFF, 0x7F, 0x00, 0x00, 0xFF, 0x7F, 0x00, 0x00, 0x00, 0x00, 0x00, 0x08,
            0x75, 0x43, 0x69, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20};
        private readonly byte[] procCommon = new byte[0x7D0];

        #endregion
        /// <summary>
        /// Initilize CAMIO with file to be read
        /// </summary>
        /// <param name="fileName"></param>
        //public CAMIO(string fileName)
        //{
        //    if (!File.Exists(fileName))
        //        throw new FileNotFoundException();
        //    //read the file
        //    ReadFile(fileName);

        //    if (blockAddresses == null)
        //        throw new FileLoadException("The header format could not be read");

        //}
        public CAMIO()
        {
            //create the proc array because it is mostly zero everywhere, with the exception of this
            Array.Copy(new byte[] { 0x75, 0x43, 0x69, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20, 0x20 }, 0, procCommon, 0, 0x10);
        }
        /// <summary>
        /// Reads the data from the the CAM File
        /// </summary>
        /// <param name="fileName">Full path to the CAM file</param>
        public void ReadFile(string fileName) 
        {
            if (!File.Exists(fileName))
                throw new FileNotFoundException();

            //read the file data from the file
            readData = File.ReadAllBytes(fileName);
            //read the header
            blockAddresses = ReadHeader();

            if (blockAddresses == null)
                throw new FileLoadException("The header format could not be read");
        }
        private Lookup<CAMBlock, UInt32> ReadHeader()
        {
            if (readData == null || readData.Length < 1)
                return null;

            //blockInfo = new Dictionary<CAMBlock, uint>();
            List<KeyValuePair<CAMBlock, UInt32>> blockInfo = new List<KeyValuePair<CAMBlock, uint>>();
            //loop throug the header scexction file
            for (int i = 0; i < 28; i++)
            {
                UInt32 headOff = (UInt32)(0x70 + i * 0x30);

                if (headOff + 0x20 > readData.Length)
                {
                    Debug.WriteLine("File format error");
                    return null;
                }
                //section ID
                UInt32 secId = BitConverter.ToUInt32(readData, (int)headOff);
                //Don't read a blank header (0x00000000)
                if (secId == 0x00000000) { continue; }

                //get the addresses of the info
                UInt32 loc = BitConverter.ToUInt32(readData, (int)(headOff + 0x0a));

                blockInfo.Add(new KeyValuePair<CAMBlock, uint>((CAMBlock)secId, loc));


                string blkName = Enum.GetName(typeof(CAMBlock), secId);
                //Debug.WriteLine("secID: 0x{0:X} Sec: {1} at 0x{2:X} ", secId, blkName, loc);

                //Debug.WriteLine("Header Data");
                //for (int j = 0; j < 17; j++)
                //{
                //    int pos = (int)(loc + 0xa + 0x4 + j * 0x2);
                //    Debug.WriteLine("\t{2,2}: Pos: 0x{0,2:X} value: 0x{1,5:X}  {1}", 0xa + 0x4 + j * 0x2, BitConverter.ToUInt16(data, pos), j);
                //}
                int pos = (int)(loc + 0xa + 0x4);
                //Debug.WriteLine("\t Number of records: {0} {1:X} {2:X}", BitConverter.ToUInt16(readData, (int)(loc + 0x1E)), BitConverter.ToUInt16(readData, (int)(loc + 0x0E)), BitConverter.ToUInt16(readData, (int)(loc + 0x04)));
                //Debug.WriteLine("\t 0x04: {0:X}", BitConverter.ToUInt16(readData, (int)(loc + 0x04)));
                //Debug.WriteLine("\t 0x0E: {0:X}", BitConverter.ToUInt16(readData, (int)(loc + 0x0E)));
                //Debug.WriteLine("\t 0x22: {0:X}", BitConverter.ToUInt16(readData, (int)(loc + 0x22)));
                //Debug.WriteLine("secID: 0x{0,5:X} Sec: {1,6} at 0x{2,5:X} \n\t0x04\t 0x{3,5:X} \n\t0x06\t 0x{4,5:X} \n\t0x08\t 0x{5,5:X} " +
                //    "\n\t0x0E\t 0x{6,5:X} \n\t0x10\t 0x{7,5:X} \n\t0x012\t 0x{8,5:X} \n\t0x14\t 0x{9,5:X} \n\t0x16\t 0x{10,5:X} \n\t0x18\t 0x{11,5:X} \n\t0x1A\t 0x{12,5:X} \n\t0x1C\t 0x{13,5:X} " +
                //    "\n\t0x1E\t 0x{14,5:X} \n\t0x20\t 0x{15,5:X} \n\t0x22\t 0x{16,5:X}  \n\t0x24\t 0x{17,5:X} \n\t0x26\t 0x{18,5:X} \n\t0x28\t 0x{19,5:X} \n\t0x2A\t 0x{20,5:X} \n\t0x2C\t 0x{21,5:X} \n\t0x2E\t 0x{22,5:X}", secId, blkName, loc,
                //    BitConverter.ToUInt16(readData, (int)(loc + 0x04)),
                //    BitConverter.ToUInt16(readData, (int)(loc + 0x06)),
                //    BitConverter.ToUInt16(readData, (int)(loc + 0x08)),
                //    BitConverter.ToUInt16(readData, (int)(loc + 0x0E)),
                //    BitConverter.ToUInt16(readData, (int)(loc + 0x10)),
                //    BitConverter.ToUInt16(readData, (int)(loc + 0x12)),
                //    BitConverter.ToUInt16(readData, (int)(loc + 0x14)),
                //    BitConverter.ToUInt16(readData, (int)(loc + 0x16)),
                //    BitConverter.ToUInt16(readData, (int)(loc + 0x18)),
                //    BitConverter.ToUInt16(readData, (int)(loc + 0x1A)),
                //    BitConverter.ToUInt16(readData, (int)(loc + 0x1C)),
                //    BitConverter.ToUInt16(readData, (int)(loc + 0x1E)),
                //    BitConverter.ToUInt16(readData, (int)(loc + 0x20)),
                //    BitConverter.ToUInt16(readData, (int)(loc + 0x22)),
                //    BitConverter.ToUInt16(readData, (int)(loc + 0x24)),
                //    BitConverter.ToUInt16(readData, (int)(loc + 0x26)),
                //    BitConverter.ToUInt16(readData, (int)(loc + 0x28)),
                //    BitConverter.ToUInt16(readData, (int)(loc + 0x2A)),
                //    BitConverter.ToUInt16(readData, (int)(loc + 0x2C)),
                //    BitConverter.ToUInt16(readData, (int)(loc + 0x2E)));

            }
            return (Lookup<CAMBlock, UInt32>)blockInfo.ToLookup(p => p.Key, p => p.Value);

        }

        protected void ReadBlock(CAMBlock block)
        {
            if (blockAddresses == null)
                throw new FileLoadException("The header format could not be read");
            if (readData == null || readData.Length < 1)
                throw new FileLoadException("The file contains no data");

            var start = blockAddresses[block];
            foreach (UInt32 pos in start)
            {
                string blkName = Enum.GetName(typeof(CAMBlock), block);
                Debug.WriteLine("secID: 0x{0:X} sec: {1} at 0x{2:X}", block, blkName, pos);
                //this should match the header id
                if (BitConverter.ToUInt32(readData, (int)pos) != (UInt32)block)
                    continue;

                UInt16 records = BitConverter.ToUInt16(readData, (int)(pos + 0x1e));
                //section header ends at pos+0x30

                //UInt32 secData = BitConverter.ToUInt32(data, (int)(pos + 0x30));
                //Debug.WriteLine("Section Header Data");
                //for (int i = 0; i < 22; i++)
                //{
                //    int loc = (int)(pos + 0x4 + 0xa + i * 0x2);
                //    Debug.WriteLine("\t{2,2}: Pos: 0x{0,2:X} value: 0x{1:X}", 0x4 + 0xa + i * 0x2, BitConverter.ToUInt16(data, loc), i);
                //}
                //Debug.WriteLine("\tSection Data: 0x{0}",secData);

                //offset to record 0x22 in header if 0x04 is 0x500
                UInt16 recOffset = BitConverter.ToUInt16(readData, (int)(pos + 0x04)) == 0x700 ? (UInt16)0 : BitConverter.ToUInt16(readData, (int)(pos + 0x22));
                //the size of the block 0x06 in header
                UInt16 blkSize = BitConverter.ToUInt16(readData, (int)(pos + 0x06));
                //the size of a record 0x20 in header
                UInt16 recSize = BitConverter.ToUInt16(readData, (int)(pos + 0x20));
                //the size of an entry 0x2A in header
                UInt16 entrySize = BitConverter.ToUInt16(readData, (int)(pos + 0x2A));
                //the number of records 0x1E in header
                UInt16 numRec = BitConverter.ToUInt16(readData, (int)(pos + 0x1E));
                if (block == CAMBlock.GEOM)
                {
                    UInt32 loc = pos + 0x30U + 0x273U;
                    //UInt32 lindegree = BitConverter.ToUInt32(readData, (int)loc) + 1;
                    //Debug.WriteLine("Linear (degree {0}):", lindegree);
                    //for (UInt32 i = 1; i < lindegree+1; i++)
                    //{
                    //    Debug.WriteLine("{0:X}: {2:X} {3:X} {4:X} {5:X}: {1}", loc + i * 0x04, ConvertFloat(readData, loc + i * 0x04), readData[loc + i * 0x04 + 0x00], readData[loc + i * 0x04 + 0x01], readData[loc + i * 0x04 + 0x02], readData[loc + i * 0x04 + 0x03]);
                    //}
                    //Debug.WriteLine("Dual");
                    //loc = pos + 0x30U + 0xeeU; ; //dual location 0xf91e
                    ////cross over point 0x192
                    //for (UInt32 i = 0; i < 20; i++)
                    //{
                    //    Debug.WriteLine("{0:X}: {2:X} {3:X} {4:X} {5:X}: {1}", loc + i * 0x04, ConvertFloat(readData, loc + i * 0x04), readData[loc + i * 0x04 + 0x00], readData[loc + i * 0x04 + 0x01], readData[loc + i * 0x04 + 0x02], readData[loc + i * 0x04 + 0x03]);
                    //}
                    //Debug.WriteLine("Dual pt 2");
                    //loc = pos + 0x30U + 0x1e7; ; //dual location 0xf91e
                    ////cross over point 0x192
                    //for (UInt32 i = 0; i < 20; i++)
                    //{
                    //    Debug.WriteLine("{0:X}: {2:X} {3:X} {4:X} {5:X}: {1}", loc + i * 0x04, ConvertFloat(readData, loc + i * 0x04), readData[loc + i * 0x04 + 0x00], readData[loc + i * 0x04 + 0x01], readData[loc + i * 0x04 + 0x02], readData[loc + i * 0x04 + 0x03]);
                    //}
                    //UInt32 empdegree = BitConverter.ToUInt32(readData, (int)(pos + 0x30U + 0x9AU)) + 1;
                    //Debug.WriteLine("EMP (degree {0}):", empdegree);
                    //loc = pos + 0x30U + 0x3a; ; //emp 0x72                     
                    //for (UInt32 i = 0; i < 37; i++)
                    //{
                    //    Debug.WriteLine("{0:X}: {2:X} {3:X} {4:X} {5:X}: {1}", loc + i * 0x04, ConvertFloat(readData, loc + i * 0x04), readData[loc + i * 0x04 + 0x00], readData[loc + i * 0x04 + 0x01], readData[loc + i * 0x04 + 0x02], readData[loc + i * 0x04 + 0x03]);
                    //}

                    Debug.WriteLine("Points");

                    //offset from the record start to the entry start
                    UInt16 entOffset = BitConverter.ToUInt16(readData, (int)(pos + 0x28));
                    //the size of a record 0x20 in header

                    //get the entry size
                    UInt16 entSize = BitConverter.ToUInt16(readData, (int)(pos + 0x2a));
                    //get the number of records

                    UInt16 headSize = BitConverter.ToUInt16(readData, (int)(pos + 0x10));
                    //loop through the record
                    for (UInt32 i = 0; i < numRec; i++)
                    {
                        loc = pos + headSize + recOffset + entOffset + (UInt32)(i * recSize);

                        //loop through the entries
                        while (readData[loc] == (byte)(i + 0x01))
                        {
                            EfficiencyPoint point = new EfficiencyPoint
                            {
                                Index = (int)i,
                                Energy = ConvertFloat(readData, loc + Convert.ToUInt16(EfficiencyPointParameterLocation.Energy)),
                                Efficiency = ConvertFloat(readData, loc + Convert.ToUInt16(EfficiencyPointParameterLocation.Efficiency)),
                                EfficiencyUncertainty = ConvertFloat(readData, loc + Convert.ToUInt16(EfficiencyPointParameterLocation.EfficiencyUncertainty)),
                            };
                            Debug.WriteLine("{0}: Energy: {1} Efficiency: {2} +/ {3}", point.Index, point.Energy, point.Efficiency, point.EfficiencyUncertainty);

                            loc += entSize;
                        }
                    }
                }
                else if (block == CAMBlock.NLINES)
                {
                    /********************NLINES*****************/
                    for (UInt32 i = 0; i < numRec; i++)
                    {
                        UInt32 ithoffset = pos + 0x30U + recOffset + (UInt32)(i * recSize);
                        UInt16 lineNum = BitConverter.ToUInt16(readData, (int)ithoffset);
                        double energy = ConvertFloat(readData, ithoffset + 0x01);
                        double abun = ConvertFloat(readData, ithoffset + 0x05);
                        double abunUnc = ConvertFloat(readData, ithoffset + 0x39);
                        double enUnc = ConvertFloat(readData, ithoffset + 0x21);
                        bool keyLine = readData[ithoffset + 0x1D] == 0x04;
                        byte nuclNum = readData[ithoffset + 0x1B];
                        Debug.WriteLine("Line :{0,4} nucNum: {6} is Key: {5,5}  En: {1,4:F4} +/- {2:F5} ab: {3,3:F4} +/- {4:F5}", i + 1, energy, enUnc, abun, abunUnc, keyLine, nuclNum);
                    }
                }
                else if (block == CAMBlock.NUCL)
                {
                    /*********************NUCL******************/
                    UInt32 lineListOffset = 0;
                    for (UInt16 i = 0; i < numRec; i++)
                    {
                        UInt32 ithoffset = pos + 0x30U + recOffset + lineListOffset + (UInt32)(i * recSize);
                        double halfLife = ConvertTimeSpan(readData, ithoffset + 0x1bU);
                        double halfLifeUnc = ConvertTimeSpan(readData, ithoffset + 0x89U);
                        string name = Encoding.ASCII.GetString(readData, (int)(ithoffset + 0x3), 0x8);
                        string HFU = Encoding.ASCII.GetString(readData, (int)(ithoffset + 0x61), 0x2);
                        UInt32 numLines = ((UInt32)BitConverter.ToUInt16(readData, (int)(ithoffset)) - 0x23AU) / 3U + 1U;
                        lineListOffset += numLines * 3U;
                        Debug.WriteLine("\nNuclide Name: {1,8} Half-Life: {0:E} +/- {2:E} sec. Unit: {3,2} numLines: {4}", halfLife, name, halfLifeUnc, HFU, numLines);
                        Debug.Write("\tLines: ");
                        for (UInt32 j = 0; j < numLines; j++)
                        {
                            string output = String.Format("{0,2}, ", readData[(int)(ithoffset + 0x237 + 0x01 + (UInt32)(j * 3U))]);
                            Debug.Write(output);
                        }
                    }
                }

                /***********Peak Settings ****************/
                else if (block == CAMBlock.PEAK)
                {
                    for (UInt32 i = 0; i < numRec; i++)
                    {
                        double psEnergy = ConvertFloat(readData, (UInt32)(pos + 0x30 + recOffset + 0x1 + i * recSize));
                        Debug.WriteLine("Peak Energy: {0}", psEnergy);
                        for (UInt32 j = 0; j < 62; j++)
                        {
                            double otherFloat = ConvertFloat(readData, pos + 0x30 + recOffset + 0x1 + i * recSize + j * 0x4);
                            UInt32 otherint = BitConverter.ToUInt32(readData, (int)(pos + 0x30 + recOffset + 0x1 + i * recSize + j * 0x4));
                            Debug.WriteLine("\t{2,2}: Pos: {1,2:X} float: {0} \t\tint:{3}", otherFloat, j * 0x4, j, otherint);
                        }
                    }
                }
                /*********ACQ Settings**********************/
                else if (block == CAMBlock.ACQP)
                {
                    UInt16 timeoffset = BitConverter.ToUInt16(readData, (int)(pos + 0x24));
                    DateTime acqTime = ConvertDateTime(readData, pos + 0x30 + timeoffset + 0x01);

                    UInt16 eCalOffset = BitConverter.ToUInt16(readData, (int)(pos + 0x22));
                    double ecal1 = ConvertFloat(readData, pos + 0x30 + eCalOffset + 0x44);
                    Debug.WriteLine("StartDate: {0} ECal Param: {1}", acqTime, ecal1);

                    UInt16 unkOff = BitConverter.ToUInt16(readData, (int)(pos + 0x23));
                    double unk = ConvertTimeSpan(readData, pos + 0x30 + timeoffset + 0x11);
                    Debug.WriteLine("Live time: {0}", unk);
                }
                /*********Sample Settings**********************/
                else if (block == CAMBlock.SAMP) 
                {
                    DateTime depTime = ConvertDateTime(readData, pos + 0x30 + 0x50);
                    DateTime sTime = ConvertDateTime(readData, pos + 0x30 + 0xb4);
                    DateTime prepTime = ConvertDateTime(readData, pos + 0x30 + 0x140);
                    Debug.WriteLine("{0} {1} {2}",depTime, prepTime, sTime);
                }
            }

        }

        /// <summary>
        /// read the lines from the file
        /// </summary>
        /// <returns>The Lines from the file</returns>
        public IList<Line> GetLines()
        {
            //check if we can read the data
            if (blockAddresses == null)
                throw new FileLoadException("The header format could not be read");
            if (readData == null || readData.Length < 1)
                throw new FileFormatException("The file contains no data");

            //get the address of the block
            var start = blockAddresses[CAMBlock.NLINES];

            if (start.Count() < 1)
                throw new FileFormatException("There is no nuclide line data in the loaded file");

            List<Line> tempLines = new List<Line>();

            //loop through all the blocks
            foreach (UInt32 pos in start)
            {
                //offset to record 0x22 in header if 0x04 is 0x500
                UInt16 commonFlag =  BitConverter.ToUInt16(readData, (int)(pos + 0x04));
                UInt16 recOffset = commonFlag == 0x700  || commonFlag==0x300 ? (UInt16)0 : BitConverter.ToUInt16(readData, (int)(pos + 0x22));
                //the size of a record 0x20 in header
                UInt16 recSize = BitConverter.ToUInt16(readData, (int)(pos + 0x20));
                //get the number of records
                UInt16 numRec = BitConverter.ToUInt16(readData, (int)(pos + 0x1E));
                UInt16 headSize = BitConverter.ToUInt16(readData, (int)(pos + 0x10));

                //loop through the records and create and add a line
                for (int i = 0; i < numRec; i++)
                {
                    UInt32 loc = pos + headSize + recOffset + (UInt32)(i * recSize);
                    Line line = new Line
                    {
                        Energy = ConvertFloat(readData, loc + (UInt32)LineParameterLocation.Energy),
                        EnergyUncertainty = ConvertFloat(readData, loc + (UInt32)LineParameterLocation.EnergyUncertainty),
                        Abundance = ConvertFloat(readData, loc + (UInt32)LineParameterLocation.Abundance),
                        AbundanceUncertainty = ConvertFloat(readData, loc + (UInt32)LineParameterLocation.AbundanceUncertainty),
                        IsKeyLine = readData[loc + (UInt32)LineParameterLocation.IsKeyLine] == 0x04,
                        NuclideIndex = readData[loc + (UInt32)LineParameterLocation.NuclideIndex]
                    };
                    tempLines.Add(line);
                }

            }

            fileLines = tempLines.OrderBy(L => L.Energy).ToList<Line>();
            return fileLines;
        }
        /// <summary>
        /// Read the nuclides and Lines from the file
        /// </summary>
        /// <returns>The nuclides from the file</returns>
        public IList<Nuclide> GetNuclides()
        {
            //check if we can read the data
            if (blockAddresses == null)
                throw new FileLoadException("The header format could not be read");
            if (readData == null || readData.Length < 1)
                throw new FileFormatException("The file contains no data");

            //get the address of the block
            var start = blockAddresses[CAMBlock.NUCL];

            if (start.Count() < 1)
                throw new FileFormatException("There is no nuclide data in the loaded file");

            fileNuclides = new List<Nuclide>();
            //get the lines if they don't exist
            if (fileLines == null || fileLines.Count < 1)
                GetLines();

            if (fileLines == null || fileLines.Count < 1)
                throw new FileFormatException("There are no lines in the file");

            //loop through all the blocks
            foreach (UInt32 pos in start)
            {
                //offset to record 0x22 in header if 0x04 is 0x500
                UInt16 recOffset = BitConverter.ToUInt16(readData, (int)(pos + 0x04)) == 0x700 ? (UInt16)0 : BitConverter.ToUInt16(readData, (int)(pos + 0x22));
                //the size of a record 0x20 in header
                UInt16 recSize = BitConverter.ToUInt16(readData, (int)(pos + 0x20));
                //get the number of records
                UInt16 numRec = BitConverter.ToUInt16(readData, (int)(pos + 0x1E));
                //get the header size
                UInt16 headSize = BitConverter.ToUInt16(readData, (int)(pos + 0x10));
                UInt32 lineListOffset = 0x0U;
                UInt32 lineListLoc = BitConverter.ToUInt16(readData, (int)(pos + 0x20));

                //loop through the records and create and add a nuclide
                for (int i = 0; i < numRec; i++)
                {
                    UInt32 loc = pos + headSize + recOffset + lineListOffset + (UInt32)(i * recSize);
                    //get the first line
                    int lineIndex = BitConverter.ToUInt16(readData, (int)(loc + lineListLoc + 0x01));
                    Nuclide nuc = new Nuclide
                    {
                        Name = Encoding.ASCII.GetString(readData, (int)(loc + (UInt32)NuclideParameterLocation.Name), 0x8),
                        HalfLifeUnit = Encoding.ASCII.GetString(readData, (int)(loc + (UInt32)NuclideParameterLocation.HalfLifeUnit), 0x2),
                        HalfLife = ConvertTimeSpan(readData, loc + (UInt32)NuclideParameterLocation.HalfLife),
                        HalfLifeUncertainty = ConvertTimeSpan(readData, loc + (UInt32)NuclideParameterLocation.HalfLifeUncertainty),
                        Index = fileLines[lineIndex-1].NuclideIndex
                    };
                    fileNuclides.Add(nuc);
                    UInt32 numLines = ((UInt32)BitConverter.ToUInt16(readData, (int)(loc)) - 0x23AU) / 3U + 1U;
                    lineListOffset += numLines * 0x3U;
                }

            }
            return fileNuclides;

        }
        /// <summary>
        /// Read the peaks from the file
        /// </summary>
        /// <returns>The Peaks from the file</returns>
        public IList<Peak> GetPeaks()
        {
            //check if we can read the data
            if (blockAddresses == null)
                throw new FileLoadException("The header format could not be read");
            if (readData == null || readData.Length < 1)
                throw new FileFormatException("The file contains no data");

            //get the address of the block
            var start = blockAddresses[CAMBlock.PEAK];

            if (start.Count() < 1)
                throw new FileFormatException("There is no peak data in the loaded file");

            IList < Peak > filePeaks = new List<Peak>();

            //loop through all the blocks
            foreach (UInt32 pos in start)
            {
                //offset to record 0x22 in header if 0x04 is 0x500
                UInt16 recOffset = BitConverter.ToUInt16(readData, (int)(pos + 0x04)) == 0x700 ? (UInt16)0 : BitConverter.ToUInt16(readData, (int)(pos + 0x22));
                //the size of a record 0x20 in header
                UInt16 recSize = BitConverter.ToUInt16(readData, (int)(pos + 0x20));
                //get the number of records
                UInt16 numRec = BitConverter.ToUInt16(readData, (int)(pos + 0x1E));
                //get the header size
                UInt16 headSize = BitConverter.ToUInt16(readData, (int)(pos + 0x10));
               
                //loop through the records and create and add a peak
                for (int i = 0; i < numRec; i++) 
                {
                    UInt32 loc = pos + headSize + recOffset + 0x01U + (UInt32)(i * recSize);
                    Peak peak = new Peak
                    {
                        Energy = ConvertFloat(readData, loc + Convert.ToUInt16(PeakParameterLocation.Energy)),
                        Centroid = ConvertFloat(readData, loc + Convert.ToUInt16(PeakParameterLocation.Centroid)),
                        CentroidUncertainty = ConvertFloat(readData, loc + Convert.ToUInt16(PeakParameterLocation.CentroidUncertainty)),
                        Continuum = ConvertFloat(readData, loc + Convert.ToUInt16(PeakParameterLocation.Continuum)),
                        CriticalLevel = ConvertFloat(readData, loc + Convert.ToUInt16(PeakParameterLocation.CriticalLevel)),
                        Area = ConvertFloat(readData, loc + Convert.ToUInt16(PeakParameterLocation.Area)),
                        AreaUncertainty = ConvertFloat(readData, loc + Convert.ToUInt16(PeakParameterLocation.AreaUncertainty)),
                        CountRate = ConvertFloat(readData, loc + Convert.ToUInt16(PeakParameterLocation.CountRate)),
                        CountRateUncertainty = ConvertFloat(readData, loc + Convert.ToUInt16(PeakParameterLocation.CountRateUncertainty)),
                        FullWidthAtHalfMaximum = ConvertFloat(readData, loc + Convert.ToUInt16(PeakParameterLocation.FullWidthAtHalfMaximum)),
                        LowTail = ConvertFloat(readData, loc + Convert.ToUInt16(PeakParameterLocation.LowTail))
                    };
                    //add the peak
                    filePeaks.Add(peak);
                }

            }

            return filePeaks;
        }
        /// <summary>
        /// Get the efficiency points from the file
        /// </summary>
        /// <returns>The efficiency points in the file</returns>
        public IList<EfficiencyPoint> GetEfficiencyPoints() 
        {
            //check if we can read the data
            if (blockAddresses == null)
                throw new FileLoadException("The header format could not be read");
            if (readData == null || readData.Length < 1)
                throw new FileFormatException("The file contains no data");

            //get the address of the block
            var start = blockAddresses[CAMBlock.GEOM];

            if (start.Count() < 1)
                throw new FileFormatException("There is no efficiency calibration data in the loaded file");

            IList<EfficiencyPoint> points = new List<EfficiencyPoint>();

            foreach (UInt32 pos in start) 
            {
                //offset to record 0x22 in header if 0x04 is 0x500
                UInt16 recOffset = BitConverter.ToUInt16(readData, (int)(pos + 0x04)) == 0x700 ? (UInt16)0 : BitConverter.ToUInt16(readData, (int)(pos + 0x22));
                //offset from the record start to the entry start
                UInt16 entOffset = BitConverter.ToUInt16(readData, (int)(pos + 0x28));
                //the size of a record 0x20 in header
                UInt16 recSize = BitConverter.ToUInt16(readData, (int)(pos + 0x20));
                //get the entry size
                UInt16 entSize = BitConverter.ToUInt16(readData, (int)(pos + 0x2a));
                //get the number of records
                UInt16 numRec = BitConverter.ToUInt16(readData, (int)(pos + 0x1E));
                //get the header size
                UInt16 headSize = BitConverter.ToUInt16(readData, (int)(pos + 0x10));
                //loop through the record
                for (UInt32 i = 0; i < numRec; i++)
                {
                    UInt32 loc = pos + headSize + recOffset + entOffset + (UInt32)(i * recSize);

                    //loop through the entries
                    while (readData[loc] == (byte)(i + 0x01))
                    {
                        EfficiencyPoint point = new EfficiencyPoint
                        {
                            Index = (int)i,
                            Energy = ConvertFloat(readData, loc + Convert.ToUInt16(EfficiencyPointParameterLocation.Energy)),
                            Efficiency = ConvertFloat(readData, loc + Convert.ToUInt16(EfficiencyPointParameterLocation.Efficiency)),
                            EfficiencyUncertainty = ConvertFloat(readData, loc + Convert.ToUInt16(EfficiencyPointParameterLocation.EfficiencyUncertainty)),
                        };

                        points.Add(point);
                        loc += entSize;
                    }
                }
            }
            return points;
        }
        /// <summary>
        /// Gets the sample time
        /// </summary>
        /// <returns>The sample Time</returns>
        public DateTime GetSampleTime()
        {
            DateTime time = new DateTime();

            //check if we can read the data
            if (blockAddresses == null)
                throw new FileLoadException("The header format could not be read");
            if (readData == null || readData.Length < 1)
                throw new FileFormatException("The file contains no data");

            //get the address of the block
            var start = blockAddresses[CAMBlock.SAMP];

            if (start.Count() < 1)
                throw new FileFormatException("There is no temporal data in the loaded file");

            //this should only go once but lets play is safe
            foreach (UInt32 pos in start)
            {
                //get the header size
                UInt16 headSize = BitConverter.ToUInt16(readData, (int)(pos + 0x10));
                time = ConvertDateTime(readData, pos + headSize + 0xb4);
            }

            return time;
        }
        /// <summary>
        /// Gets the time the spectrum was aquired
        /// </summary>
        /// <returns>The aqusition Time</returns>
        public DateTime GetAqusitionTime() 
        {
            DateTime time = new DateTime();

            //check if we can read the data
            if (blockAddresses == null)
                throw new FileLoadException("The header format could not be read");
            if (readData == null || readData.Length < 1)
                throw new FileFormatException("The file contains no data");

            //get the address of the block
            var start = blockAddresses[CAMBlock.ACQP];

            if (start.Count() < 1)
                throw new FileFormatException("There is no temporal data in the loaded file");

            //this should only go once but lets play is safe
            foreach (UInt32 pos in start)
            {
                //get the header size
                UInt16 headSize = BitConverter.ToUInt16(readData, (int)(pos + 0x10));
                UInt16 timeoffset = BitConverter.ToUInt16(readData, (int)(pos + 0x24));
                time = ConvertDateTime(readData, pos + headSize + timeoffset + 0x01);
            }

            return time;
        }
        /// <summary>
        /// Gets the live time of the spectrum
        /// </summary>
        /// <returns>Live time in seconds</returns>
        public double GetLiveTime()
        {
            double time = 0.0;

            //check if we can read the data
            if (blockAddresses == null)
                throw new FileLoadException("The header format could not be read");
            if (readData == null || readData.Length < 1)
                throw new FileFormatException("The file contains no data");

            //get the address of the block
            var start = blockAddresses[CAMBlock.ACQP];

            if (start.Count() < 1)
                throw new FileFormatException("There is no temporal data in the loaded file");

            //this should only go once but lets play is safe
            foreach (UInt32 pos in start)
            {
                UInt16 timeoffset = BitConverter.ToUInt16(readData, (int)(pos + 0x24));
                time =ConvertTimeSpan (readData, pos + 0x30 + timeoffset + 0x11);
            }

            return time;
        }
        /// <summary>
        /// Gets the real time of the spectrum
        /// </summary>
        /// <returns>Live time in seconds</returns>
        public double GetRealTime()
        {
            double time = 0.0;

            //check if we can read the data
            if (blockAddresses == null)
                throw new FileLoadException("The header format could not be read");
            if (readData == null || readData.Length < 1)
                throw new FileFormatException("The file contains no data");

            //get the address of the block
            var start = blockAddresses[CAMBlock.ACQP];

            if (start.Count() < 1)
                throw new FileFormatException("There is no temporal data in the loaded file");

            //this should only go once but lets play is safe
            foreach (UInt32 pos in start)
            {
                UInt16 timeoffset = BitConverter.ToUInt16(readData, (int)(pos + 0x24));
                time = ConvertTimeSpan(readData, pos + 0x30 + timeoffset + 0x09);
            }

            return time;
        }
        /// <summary>
        /// Get the shape calibration from a loaded file
        /// </summary>
        /// <returns>A collection of energy calibration parameters</returns>
        public double[] GetShapeCalibration() 
        {
            double[] cal = new double[4];

            //check if we can read the data
            if (blockAddresses == null)
                throw new FileLoadException("The header format could not be read");
            if (readData == null || readData.Length < 1)
                throw new FileFormatException("The file contains no data");

            //get the address of the block
            var start = blockAddresses[CAMBlock.ACQP];

            if (start.Count() < 1)
                throw new FileFormatException("There is no calibration data in the loaded file");

            //this should only go once but lets play is safe
            foreach (UInt32 pos in start)
            {
                //fill the array
                UInt16 eCalOffset = (UInt16)(0x30U + BitConverter.ToUInt16(readData, (int)(pos + 0x22)) + 0xDCU);
                for (UInt32 i = 0; i < cal.Length; i++)
                {
                    cal[i] = ConvertFloat(readData, (UInt32)(pos + eCalOffset + i * 0x04U));
                }
            }

            return cal;
        }
        /// <summary>
        /// Get the energy calibration from a loaded file
        /// </summary>
        /// <returns>A collection of energy calibration parameters</returns>
        public double[] GetEnergyCalibration() 
        {
            double[] cal = new double[4];

            //check if we can read the data
            if (blockAddresses == null)
                throw new FileLoadException("The header format could not be read");
            if (readData == null || readData.Length < 1)
                throw new FileFormatException("The file contains no data");

            //get the address of the block
            var start = blockAddresses[CAMBlock.ACQP];

            if (start.Count() < 1)
                throw new FileFormatException("There is no calibration data in the loaded file");

            //this should only go once but lets play is safe
            foreach (UInt32 pos in start)
            {
                //fill the array
                UInt16 eCalOffset = (UInt16)(0x30U+ BitConverter.ToUInt16(readData, (int)(pos + 0x22)) + 0x44U);
                for(UInt32 i = 0; i < cal.Length; i++) 
                { 
                    cal[i] = ConvertFloat(readData, (UInt32)(pos + eCalOffset + i*0x04U));
                }
            }

            return cal;
        }

        /// <summary>
        /// Gets the spectrum from a loaded file
        /// </summary>
        /// <returns>Array of the counts in the channel</returns>
        public UInt32[] GetSpectrum() 
        {
            //check if we can read the data
            if (blockAddresses == null)
                throw new FileLoadException("The header format could not be read");
            if (readData == null || readData.Length < 1)
                throw new FileFormatException("The file contains no data");

            //get the address of the block
            var start = blockAddresses[CAMBlock.SPEC];

            if (start.Count() < 1)
                throw new FileFormatException("There is no spectral data in the loaded file");

            List<UInt32> spec = new List<UInt32>();
            //this should only go once but lets play is safe
            foreach (UInt32 pos in start)
            {
                //get the number of channels
                UInt16 ch = BitConverter.ToUInt16(readData, (int)(pos + 0x2A));
                //get the header offset
                UInt16 hoff = BitConverter.ToUInt16(readData, (int)(pos + 0x10));
                //get the offest the the actual data
                UInt16 dataoff = BitConverter.ToUInt16(readData, (int)(pos + 0x28));

                UInt32[] tSpec = new UInt32[ch];
                for (UInt32 i = 0; i < ch; i++) 
                {
                    tSpec[i] = BitConverter.ToUInt32(readData, (int)(pos + dataoff +hoff + i * 0x04U));
                }
                //add the temporary spectra
                spec.AddRange(tSpec);
            }

            return spec.ToArray();
        }
        /// <summary>
        /// Creates a File with the created data in the library 
        /// </summary>
        /// <param name="filePath">Full path to the file to be created</param>
        public void CreateFile(string filePath)
        {

            if (!Directory.Exists(Path.GetDirectoryName(filePath)))
                throw new IOException("The directory does not exist");

            if (lines == null || nucs == null)
                throw new NullReferenceException("Both Lines and Nuclides must not be null");
            //Get the limiting number of records of Lines, and break up blocks that are too large
            //UInt32 numLineBlocks = 1, numLineRecords = (UInt32)lines.Count;

            //check if the block needs to be broken up
            //if (lines.Count > 125)
            //{
            //    numLineRecords = 126U;
            //    numLineBlocks = lines.Count % numLineRecords > 0U ? 1U + (UInt32)lines.Count / numLineRecords : (UInt32)lines.Count / numLineRecords;
            //}

            ////Get the limiting number of records of nuclies, and break up blocks that are too large
            //UInt32 numNucBlocks = 1, numNucRecords = (UInt32)nucs.Count;

            //check if the block needs to be broken up
            //size of the all the records + Size of all the Entries (lines) + Size of the common
            //if((UInt32)RecordSize.NUCL*nucs.Count + 0x03* (UInt32)lines.Count + 0x03F5 > (UInt32) BlockSize.NUCL)
            ////if (nucs.Count > 29)
            //{
            //    numNucRecords = 30U;
            //    numNucBlocks = nucs.Count % numNucRecords > 0U ? 1U + (UInt32)nucs.Count / numNucRecords : (UInt32)nucs.Count / numNucRecords;
            //}

            //create an array and add the acqp block and proc block
            //byte[][] blockList = new byte[(int)(numLineBlocks + numNucBlocks) + 2][];

            //blockList[0] = GenerateBlock(CAMBlock.ACQP, headerSize);
            //blockList[1] = GenerateBlock(CAMBlock.PROC, headerSize + (UInt16)BlockSize.ACQP);
            List<byte[]> blockList = new List<byte[]>
            {
                GenerateBlock(CAMBlock.ACQP, headerSize),
                GenerateBlock(CAMBlock.PROC, headerSize + (UInt16)BlockSize.ACQP)
            };

            //create and place the lines in the blocks array
            UInt32 loc = headerSize + (UInt16)BlockSize.ACQP + (UInt16)BlockSize.PROC;
            int startRecord = 0, fileIndex = blockList.Count, numRecords = 125;
            UInt16 blockNo = 0;
            while(startRecord < lines.Count)
                //for (int i = 1; i < numLineBlocks + 1; i++)
            {
                //numRecords = numRecords + startRecord  > lines.Count ? lines.Count - startRecord : (int)numLineRecords;
                blockNo = startRecord + numRecords > lines.Count ?   (UInt16)0U:  (UInt16)(blockNo + 1U);
                //blockList[fileIndex] = GenerateBlock(CAMBlock.NLINES, loc, ((List<byte[]>)lines).GetRange(startRecord, numRecords), blockNo, i == 1);
                byte[] block = GenerateBlock(CAMBlock.NLINES, loc, ((List<byte[]>)lines).GetRange(startRecord, lines.Count-startRecord), blockNo, startRecord == 0);
                numRecords = (int)BitConverter.ToUInt16(block, 0x1E);
                startRecord += numRecords;
                blockList.Add(block);
                loc += (UInt32)BlockSize.NLINES;
                //blockNo++;
                fileIndex++;
            }

            //create and place the Nucs in the blocks array
            startRecord = 0; numRecords = 29; blockNo = (UInt16)(fileIndex - 2U);
            //for (int i = 1; i < numNucBlocks + 1; i++)
            while (startRecord < nucs.Count)
            {
                //numRecords = numRecords + startRecord> nucs.Count ? nucs.Count - startRecord : (int)numNucRecords;
                blockNo = startRecord + numRecords > nucs.Count ?  (UInt16)0U : (UInt16)(blockNo + 1);
                //blockList[fileIndex] =  GenerateBlock(CAMBlock.NUCL, loc, ((List<byte[]>)nucs).GetRange(startRecord, numRecords), blockNo, i == 1);
                byte[] block = GenerateBlock(CAMBlock.NUCL, loc, ((List<byte[]>)nucs).GetRange(startRecord, nucs.Count - startRecord), blockNo, startRecord == 0);
                numRecords = (int)BitConverter.ToUInt16(block, 0x1E);
                startRecord += numRecords;
                blockList.Add(block);
                loc += (UInt32)BlockSize.NUCL;
                fileIndex++;               
            }

            //genertates the file by converting a list of blocks to a real file
            byte[] fileBytes = GenerateFile(blockList);

            //put the file size in the file header
            Array.Copy(BitConverter.GetBytes(fileBytes.Length), 0, fileBytes, 0x0A, 0x04);

            //write the file
            try
            {
                File.WriteAllBytes(filePath, fileBytes);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally 
            {
                lines.Clear();
                nucs.Clear();
            }

        }
        /// <summary>
        /// Add a Nuclide to the file
        /// </summary>
        /// <param name="name">Name of the nuclide</param>
        /// <param name="halfLife">Half-Life</param>
        /// <param name="halfLifeUnc">Half-Life Uncertaitny</param>
        /// <param name="halfLifeUnit">HalfLifeUnit</param>
        /// <param name="nucNo">The nuclide record number</param>
        public void AddNuclide(string name, double halfLife, double halfLifeUnc, string halfLifeUnit, int nucNo)
        {
            //create the nuc
            Nuclide nuc =  new Nuclide(name,halfLife, halfLifeUnc, halfLifeUnit, nucNo);
            //add it to the file
            AddNuclide(nuc);
        }

        /// <summary>
        /// Add a nuclide to the file
        /// </summary>
        /// <param name="nuc">Nuclide to add</param>
        /// <param name="nucNo">The nuclide record number</param>
        public void AddNuclide(Nuclide nuc)
        {
            if (nucs == null)
                nucs = new List<byte[]>();

            //convert half life into seconds
            switch (nuc.HalfLifeUnit.ToUpper())
            {
                case "Y":
                    nuc.HalfLife *= 31557600;
                    nuc.HalfLifeUncertainty *= 31557600;
                    break;
                case "D":
                    nuc.HalfLife *= 86400;
                    nuc.HalfLifeUncertainty *= 86400;
                    break;
                case "H":
                    nuc.HalfLife *= 3600;
                    nuc.HalfLifeUncertainty *= 3600;
                    break;
                case "M":
                    nuc.HalfLife *= 60;
                    nuc.HalfLifeUncertainty *= 60;
                    break;
                case "S":
                    break;
                default:
                    throw new ArgumentException("Half Life Unit not recognized");
            }

            int nucNo = nuc.Index;
            //get the lines assoicated with the nuclde
            UInt16[] lineNums = lines != null ?
                lines.Where(line => line[0x1B] == BitConverter.GetBytes(nucNo)[0]).Select(l => (UInt16)(lines.IndexOf(l) + 1U)).ToArray() :
                new UInt16[0];

            nucs.Add(GenerateNuclide(nuc.Name, nuc.HalfLife, nuc.HalfLifeUncertainty, nuc.HalfLifeUnit.ToUpper(), lineNums));
        }

        /// <summary>
        /// Add a Line to the file
        /// </summary>
        /// <param name="energy">The line energy in keV</param>
        /// <param name="enUnc">The line energy uncertainty in keV</param>
        /// <param name="yield">The line yield in percent</param>
        /// <param name="yieldUnc">The line yield uncertainty in percent</param>
        /// <param name="nucNo">The nuclide number associated with line</param>
        /// <param name="key">true if the line is the key line</param>
        public void AddLine(double energy, double enUnc, double yield, double yieldUnc, int nucNo, bool key = false)
        {
            Line line = new Line(energy, enUnc, yield, yieldUnc, nucNo, key);
            AddLine(line);
        }
        /// <summary>
        /// Add a Line to the file
        /// </summary>
        /// <param name="line">The line to be added to the file</param>
        public void AddLine(Line line)
        {
            int nucNo = line.NuclideIndex;

            if (nucNo > 255)
                throw new ArgumentException("Cannot have more than 255 nuclides");

            //if the nuclide alredy exists add lines to it
            if (nucs != null && nucs.Count > nucNo)
                AddLinesToNuclide(nucs.ElementAt(nucNo), new byte[] { BitConverter.GetBytes(lines.Count)[0] });
            //initilize the lines
            if (lines == null || lines.Count < 1)
                lines = new List<byte[]>();

            lines.Add(GenerateLine(line.Energy, line.EnergyUncertainty, line.Abundance, line.AbundanceUncertainty, line.IsKeyLine, BitConverter.GetBytes(nucNo)[0]));
        }
        /// <summary>
        /// Generate the contents of a CAM file file
        /// </summary>
        /// <param name="blocks">Collection of byte arrays that are the blocks to combine into a file</param>
        /// <returns>The Byte array for the file</returns>
        protected byte[] GenerateFile(ICollection<byte[]> blocks)
        {
            //get info from the block headers
            int filelength = 0x800;
            foreach (byte[] block in blocks)
            {
                //get the size of the block
                BlockSize size = (BlockSize)Enum.Parse(typeof(BlockSize), Enum.GetName(typeof(CAMBlock), BitConverter.ToUInt32(block, 0x00)));
                filelength += (int)size;
            }

            //create the containter
            byte[] file = new byte[filelength];
            Array.Copy(fileHeader, 0, file, 0, fileHeader.Length);

            //copy the blocks into the file
            int i = 0;
            foreach (byte[] block in blocks)
            {
                //copy the header
                Array.Copy(block, 0, file, 0x70 + i * 0x30, 0x30);

                //copy the block
                Debug.WriteLine("0x0A: {0:X} 0x06: {1:X}", BitConverter.ToUInt32(block, 0x0a), BitConverter.ToUInt16(block, 0x06));
                Array.Copy(block, 0, file, BitConverter.ToUInt32(block, 0x0a), BitConverter.ToUInt16(block, 0x06));
                i++;
            }
            return file;
        }
        /// <summary>
        /// Generates a block of data
        /// </summary>
        /// <param name="block">The block type to generate</param>
        /// <param name="loc">The location within the file</param>
        /// <param name="records">The records in the block</param>
        /// <param name="blockNo">The block index of a split block</param>
        /// <param name="hasCommon">Does this block have common data</param>
        /// <returns></returns>
        protected byte[] GenerateBlock(CAMBlock block, UInt32 loc, IList<byte[]> records = null, UInt16 blockNo = 0, bool hasCommon = true)
        {
            //return just the default for acqp with the header
            if (block == CAMBlock.ACQP)
            {
                byte[] acqpHead = GenerateBlockHeader(block, loc);
                return acqpHead.Concat(acqpCommon).ToArray();  
            }
            if (block == CAMBlock.PROC)
            {
                byte[] procHead = GenerateBlockHeader(block, loc);
                return procHead.Concat(procCommon).ToArray();
            }

            //check for valid entries
            if (block != CAMBlock.NUCL && block != CAMBlock.NLINES)
                throw new ArgumentException("Only blocks AQCP, PROC, NUCL and NLINES are supported");
            if (records == null || records.Count < 1)
                throw new ArgumentException("records Parameter cannot be null or empty");

            //get the size of the block from the block BlockSize enum
            BlockSize size = (BlockSize)Enum.Parse(typeof(BlockSize), Enum.GetName(typeof(CAMBlock), block));

            // build an empty conatiner for the block
            byte[] blockBytes = new byte[(int)size];

            //copy the common data only for the first block
            int destIndex = blockHeaderSize;
            if (hasCommon)
            {
                //include the common data
                switch (block)
                {
                    case CAMBlock.NUCL:
                        Array.Copy(nuclCommon, 0, blockBytes, blockHeaderSize, nuclCommon.Length);
                        destIndex += 0x3F5;
                        break;
                    case CAMBlock.NLINES:
                        Array.Copy(nlineCommon, 0, blockBytes, blockHeaderSize, nlineCommon.Length);
                        destIndex += 0x18;
                        break;
                    default:
                        throw new ArgumentException("Only blocks AQCP, NUCL and NLINES are supported");
                }
            }
            //copy in the records
            UInt16 totRecLines = 0, totalRec = 0;
            //foreach (byte[] rec in records)
            //loop through the recores
            using (IEnumerator<byte[]> recEnum = records.GetEnumerator()) 
            {
                int recSize = 0;
                while (destIndex + recSize < (int)size && recEnum.MoveNext())
                {
                    byte[] rec = recEnum.Current;
                    recSize = rec.Length;
                    Array.Copy(rec, 0, blockBytes, destIndex, recSize);
                    destIndex += recSize;
                    totalRec++;
                    if (block == CAMBlock.NUCL)
                        totRecLines += GetNumLines(rec);
                }
            }
            Debug.WriteLine("Total Rec Lines{0}",totRecLines);
            //get the header
            byte[] header = GenerateBlockHeader(block, loc, totalRec, totRecLines, blockNo, hasCommon);

            //copy the header to byte array
            Array.Copy(header, 0, blockBytes, 0, blockHeaderSize);

            return blockBytes;
        }

        /// <summary>
        /// Generates a section header for ACQP, NLINES, and NUC blocks
        /// </summary>
        /// <param name="block">The block header to generate (ACQP, NLINES, and NUC only)</param>
        /// <param name="loc">The location to write the block</param>
        /// <param name="numRec">the number of records, if there are any</param>
        /// <param name="numLines">the numRec for the NLINES section</param>
        /// <param name="blockNum">The record of the block in repeated record block</param>
        /// <returns>The Bytes to of a section header</returns>
        protected byte[] GenerateBlockHeader(CAMBlock block, UInt32 loc, UInt16 numRec = 1, UInt16 numLines = 1, UInt16 blockNum = 0, bool hasCommon=false)
        {
            if (block != CAMBlock.ACQP && block != CAMBlock.NUCL && block != CAMBlock.NLINES && block != CAMBlock.PROC)
                throw new ArgumentException("Only blocks AQCP, NUCL and NLINES are supported");

            byte[] header = new byte[0x30];

            UInt16 blockRec = blockNum >= 1 ? (UInt16)(blockNum + 4U) : (UInt16)(0U);

            //defualt to ACQP
            UInt16[] values = new UInt16[20] {
                0x0500,                      //0x04  0
                (UInt16) BlockSize.ACQP,     //0x06  1
                0x0000,                      //0x08  2
                0x0000,                      //0x0E  3
                0x0030,                      //0x10  4
                0x0890,                      //0x12  5
                0x000D,                      //0x14  6
                0x8A00,                      //0x16  7
                0x0000,                      //0x18  8
                0x003C,                      //0x1A  9
                0x0000,                      //0x1C  10
                numRec,                      //0x1E  11
                0x051C,                      //0x20  12
                0x03D8,                      //0x22  13
                0x02F7,                      //0x24  14
                0x0019,                      //0x26  15
                0x04C2,                      //0x28  16
                0x0009,                      //0x2A  17
                0x0000,                      //0x2C  18
                0x0924,                      //0c2E  19
            };
            switch (block)
            {
                case CAMBlock.PROC:
                    values[0] =  0x0100;
                    values[1] =  (UInt16)BlockSize.PROC;
                    values[5] =  0x1C90;
                    values[6] =  0x000E;
                    values[7] =  0xBE00;
                    values[8] =  0x0001;
                    values[11] = 0x0000;
                    values[12] = 0x0000;
                    values[13] = 0x7FFF;
                    values[14] = 0x7FFF;
                    values[15] = 0x0000;
                    values[16] = 0x7FFF;
                    values[17] = 0x0000;
                    values[19] = 0x0800;
                    break;
                case CAMBlock.NUCL:
                    values[0]  = (UInt16)(hasCommon ? 0x0500U : 0x700U); 
                    values[1]  = (UInt16)BlockSize.NUCL;
                    values[3]  = (UInt16)(0x2800U + blockRec);
                    values[5]  = 0x5E90; 
                    values[6]  = 0x0010;
                    values[7]  = 0x4800;
                    values[12] = 0x0237;
                    values[13] = 0x03F5;
                    values[14] = 0x7FFF;
                    values[15] = 0x0000;
                    values[16] = 0x0235;
                    values[17] = 0x0003;
                    values[19] = Convert.ToUInt16(values[4] + values[11] * values[12] + (hasCommon ? values[13] : 0U) + values[17] + (numLines -1)*3);
                    break;
                case CAMBlock.NLINES:
                    values[0]  = (UInt16)(hasCommon ? 0x0500U : 0x700U);
                    values[1]  = (UInt16)BlockSize.NLINES;
                    values[3]  = (UInt16)(0x2800U + blockRec);
                    values[5]  = 0x2290;
                    values[6]  = 0x0015;
                    values[7]  = 0x1200;
                    values[12] = 0x0085;
                    values[13] = 0x0018;
                    values[14] = 0x7FFF;
                    values[15] = 0x0000;
                    values[16] = 0x7FFF;
                    values[17] = 0x0000;
                    values[19] = Convert.ToUInt16(values[4] + values[11] * values[12] + (hasCommon ? values[13] : 0U) + values[17]);
                    break;
            }
            //copy in the block code
            Array.Copy(BitConverter.GetBytes((UInt32)block),0,header,0,0x04);
            //copy in the location
            Array.Copy(BitConverter.GetBytes(loc), 0, header, 0x0A, 0x04);
            int headerindex = 0x04;
            for (int i = 0; i < values.Length; i++)
            {
                //skip the already wirtten address
                headerindex += headerindex == 0x0A ? 0x04 : 0x00;

                Array.Copy(BitConverter.GetBytes(values[i]), 0, header, headerindex, 0x2);
                headerindex += 0x2;
            }

            return header;
        }
        /// <summary>
        /// Gets the number of lines associated with a NUCL record
        /// </summary>
        /// <param name="nuclRecord">The byte array of the record</param>
        /// <returns></returns>
        private UInt16 GetNumLines(byte[] nuclRecord)
        {
            //if (nuclRecord.Length !=(int) RecordSize.NUCL)
            //    throw new ArgumentException("Lines can only be retrieved from a NUCL block")

            if (nuclRecord.Length < (int)RecordSize.NUCL)
                throw new ArgumentOutOfRangeException("There are no Lines assoicated with this record");

            return (UInt16)((((uint)nuclRecord.Length - (uint)RecordSize.NUCL)) / 0x3U);

        }

        /// <summary>
        /// Generates a CAM nuclide
        /// </summary>
        /// <param name="name">Name of the nuclide</param>
        /// <param name="halfLife">Half Life in seconds</param>
        /// <param name="halfLifeUnc">Half Life uncertainty in seconds</param>
        /// <param name="halfLifeUnit">Half Life unit</param>
        /// <param name="lineNums">The record number of the lines associated with the nuclide</param>
        /// <returns>The nuclude that can be written to a CAM file</returns>
        protected byte[] GenerateNuclide(string name, double halfLife, double halfLifeUnc, string halfLifeUnit, UInt16[] lineNums)
        {
            //get the number of lines from the array of line numbers
            UInt32 numLines = (UInt32)lineNums.Length;
            //Create a byte array and fill it
            byte[] nuc = new byte[(UInt32)RecordSize.NUCL+ numLines * 0x3U];

            //set the number of line parameter
            nuc[0] = BitConverter.GetBytes((numLines - 1U) * 3U + 0x23AU)[0];
            //set the spacer
            nuc[1] = 0x02; nuc[2] = 0x01;
            nuc[0x5f] = 0x01;
            //set the time spans
            Array.Copy(TimeSpanToCAM(halfLife), 0, nuc, 0x1b,0x08);
            Array.Copy(TimeSpanToCAM(halfLifeUnc), 0, nuc, 0x89, 0x08);
            //set the strings
            Encoding.ASCII.GetBytes(name.PadRight(0x8,' '), 0, 0x8, nuc, 0x03);
            Encoding.ASCII.GetBytes(halfLifeUnit.PadRight(0x2, ' '), 0, 0x2, nuc, 0x61);
            //do the lines
            for(UInt16 i = 0; i <numLines;i++)
            {
                UInt32 offset = (UInt32)RecordSize.NUCL + i * 0x3U;
                nuc[offset] = 0x01;
                Array.Copy(BitConverter.GetBytes(lineNums[i]), 0, nuc, offset + 0x01, 0x2);
                //nuc[offset + 0x01] = lineNums[i];
                //nuc[offset + 0x02] = i == numLines - 1 ? (byte)0x01 : (byte)0x00;
            }
            return nuc;
        }
        /// <summary>
        /// Add lines to an existing nuclide
        /// </summary>
        /// <param name="nuc">The nuclide</param>
        /// <param name="lineNums">he record number of the lines to be associated with the nuclide</param>
        /// <returns></returns>
        protected byte[] AddLinesToNuclide(byte[] nuc, byte[] lineNums)
        {
            //get the number of lines from the array of line numbers
            UInt32 numLines = (UInt32)lineNums.Length;

            //set the number of line parameter
            nuc[0] = BitConverter.GetBytes((numLines - 1U) * 3U + 0x23AU)[0];
            //Create a byte array and fill it
            byte[] linesList = new byte[numLines * 0x3U];

            for (UInt16 i = 0; i < numLines; i++)
            {
                UInt32 offset = i * 0x3U;
                linesList[offset] = 0x01;
                linesList[offset + 0x01] = lineNums[i];
            }
            //add the lines list to the byte array
            return nuc.Concat(linesList).ToArray();
        }
        /// <summary>
        /// Generates a CAM nuclide line
        /// </summary>
        /// <param name="energy">The line energy in keV</param>
        /// <param name="enUnc">The line energy uncertainty in keV</param>
        /// <param name="yield">The line yield in percent</param>
        /// <param name="yieldUnc">The line yield uncertainty in percent</param>
        /// <param name="key">true if the line is the key line</param>
        /// <param name="nucNo">The nuclide number associated with line</param>
        /// <returns>A line that can be written to a CAM file</returns>
        public byte[] GenerateLine(double energy, double enUnc, double yield, double yieldUnc, bool key, byte nucNo)
        {
            //create a new byte array and fill it
            byte[] line = new byte[(UInt16)RecordSize.NLINES];
            line[0] = 0x01;
            Array.Copy(FloatToCAM(energy), 0,line, 0x01,0x4);
            Array.Copy(FloatToCAM(enUnc), 0, line, 0x21, 0x4);
            Array.Copy(FloatToCAM(yield), 0, line, 0x05, 0x4);
            Array.Copy(FloatToCAM(yieldUnc), 0, line, 0x39, 0x4);
            //set if it is the key line
            line[0x1D] = key ? BitConverter.GetBytes(0x04)[0] : BitConverter.GetBytes(0x00)[0];
            //set the nuclide number
            line[0x1B] = BitConverter.GetBytes(nucNo)[0];

            return line;
        }

        #region conversions
        /// <summary>
        /// Converts the julain UInt64 CAM Date time to a .net Date time
        /// </summary>
        /// <param name="pos">The position of the value to covert</param>
        /// <param name="data">The array of bytes to be converted</param>
        /// <returns>A date time</returns>
        private DateTime ConvertDateTime(byte[] data, UInt32 pos)
        {
            if (data.Length < 0x08)
                throw new ArgumentException("The data input array is not long enough");
            if (data.Length < pos + 0x08)
                throw new ArgumentOutOfRangeException("The provided index exceedes the length of the array");

            UInt64 unixDate = BitConverter.ToUInt64(data, (int)pos) / 10000000UL - 3506716800UL;
            DateTime time = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            //if the date is too small or large just retrun epoch
            return unixDate < 0 || unixDate > 253402325999 ? time:time.AddSeconds(unixDate);

        }
        /// <summary>
        /// Converts a datetime to a CAM Date Time
        /// </summary>
        /// <param name="input">DateTime to convert</param>
        /// <returns>byte repesetnation of hte DateTime in CAM format</returns>
        private byte[] DateTimeToCAM(DateTime input)
        {
            //get the unix epoch
            DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            if (input < epoch)
                throw new ArgumentOutOfRangeException("Date is before UNIX epoch");
            //convert DateTime to epoch in ticks
            UInt64 ticsFromEpoch = Convert.ToUInt64((input.ToUniversalTime() - epoch).Ticks);
            //convert to julain UInt64
            return BitConverter.GetBytes((ticsFromEpoch / TimeSpan.TicksPerSecond + 3506716800UL) * 10000000UL);

        }

        /// <summary>
        /// Convert the the CAM time span (0.1 μs) to seconds 
        /// </summary>
        /// <param name="pos">The position of the value to covert</param>
        /// <param name="data">The array of bytes to be converted</param>
        /// <returns>The number of seconds</returns>
        private double ConvertTimeSpan(byte[] data, UInt32 pos)
        {
            //check the inputs
            if (data.Length < 0x08)
                throw new ArgumentException("The data input array is not long enough");
            if (data.Length < pos + 0x08)
                throw new ArgumentOutOfRangeException("The provided index exceedes the length of the array");
            //get the number
            double span;
            if (data[pos + 0x07] != 0x80)
                span = Math.Abs(BitConverter.ToInt64(data, (int)pos) / (float)10000000);
            else
            {
                span = data[pos + 0x04] == 0x01 ? (BitConverter.ToInt32(data, (int)pos) * 1E6): BitConverter.ToInt32(data, (int)pos);
            }

            return span;
        }

        /// <summary>
        /// Converts a time span into the CAM bytes
        /// </summary>
        /// <param name="input">Time span in seconds to convert</param>
        /// <returns>byte represnentaion of the timespan in CAM format</returns>
        private byte[] TimeSpanToCAM(double input)
        {
            Int64 tspan;
            byte[] span;
            //if the input is too big turn it into years. 
            if (input * 10000000 > Int64.MaxValue)
            {
                double tinput = input / 31557600;
                if (tinput > Int32.MaxValue)
                {
                    tspan = Convert.ToInt32(tinput/1E6);
                    span = BitConverter.GetBytes(tspan);
                    span[7] = 0x80;
                    span[4] = 0x01;
                }
                else
                {
                    tspan = Convert.ToInt32(tinput);
                    span = BitConverter.GetBytes(tspan);
                    span[7] = 0x80;
                }
            }
            else {
                tspan = -1 * Convert.ToInt64(input * 10000000L);
                span = BitConverter.GetBytes(tspan);
            }

            return span;
        }

        /// <summary>
        /// Convert the PDP-11 to a IEEE double number
        /// </summary>
        /// <param name="pos">The start index of the value to covert</param>
        /// <param name="data">The array of bytes to be converted</param> 
        /// <returns>The floating point value </returns>
        private double ConvertFloat(byte[] data, UInt32 pos)
        {
            if (data.Length < 0x04)
                throw new ArgumentException("The data input array is not long enough");
            if (data.Length < pos + 0x04)
                throw new ArgumentOutOfRangeException("The provided index exceedes the length of the array");
            //UInt16[] words = new UInt16[2];
            //swap words
            byte[] word1 = new byte[0x2], word2 = new byte[0x2];
            Array.Copy(data, (int)(pos + 0x2), word1, 0, 0x2);
            Array.Copy(data, (int)(pos), word2, 0, 0x2);

            byte[] bytearr = word1.Concat(word2).ToArray();
            double val = BitConverter.ToSingle(bytearr, 0) / 4;
            return val;
        }


        /// <summary>
        /// Converts and IEEE double point number to PDP-11
        /// </summary>
        /// <param name="input">number to be converted</param>
        /// <returns>byte representation in PDP-11 format</returns>
        private byte[] FloatToCAM(double input)
        {
            byte[] word1 = new byte[0x2], word2 = new byte[0x2];
            byte[] inpByte = BitConverter.GetBytes(Convert.ToSingle(input * 4));

            Array.Copy(inpByte, 0x2, word1, 0, 0x2);
            Array.Copy(inpByte, 0, word2, 0, 0x2);

            return word1.Concat(word2).ToArray();
        }
        #endregion

    }


}
