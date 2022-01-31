using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace temperature_analysis
{
    class WriteCSV
    {
        static string[] Month = { "January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December" };
        static char Separator = ',';
        /// <summary>
        /// Writes out 1-year data month by month in separate CSV files
        /// </summary>
        /// <param name="filenameTemplate">template for filenames</param>
        /// <param name="arr">data array [month(12), row, col]</param>
        /// <param name="cultureInfo">string for cultureInfo (used for dec point)</param>
        public static void WriteMonths(string filenameTemplate, double[,,] arr, string cultureInfo = "en-US")
        {
            var culture = CultureInfo.CreateSpecificCulture(cultureInfo);
            string filename;
            int rows = arr.GetLength(1);
            int cols = arr.GetLength(2);
            for(int mon = 0; mon < 12; mon++)
            {
                filename = Month[mon] + "_" + filenameTemplate + ".csv";
                StreamWriter writer = new(filename);
                for(int r = 0; r < rows; r++)
                {
                    string line = "";
                    for(int c=0; c < cols; c++)
                    {
                        line += arr[mon, r, c].ToString(culture);
                        if (c < cols - 1)
                            line += Separator;
                    }
                    writer.WriteLine(line);
                }
                writer.Flush();
                writer.Close();
            }
            
        }
        /// <summary>
        /// Write month average for one "pixel" thru year interval
        /// </summary>
        /// <param name="dataArray">array with data [month, row, column]</param>
        /// <param name="month">0-JAN 1-FEB .. 11-DEC</param>
        /// <param name="startFromYear">start from year (1901-2020)</param>
        /// <param name="endToYear">end to year (1901-2020)</param>
        /// <param name="lattitude">coordinate to find rigth "pixel" degrees north</param>
        /// <param name="longitude">coordinate to find rigth "pixel" degrees east</param>
        /// <param name="cultureInfo">string for cultureInfo (used for dec point)</param>
        public static void WriteOnePoint(double[,,] dataArray, int month, double lattitude, double longitude, string pointName ,int startFromYear=1901, int endToYear=2020, string cultureInfo= "en-US") 
        {
            var culture = CultureInfo.CreateSpecificCulture(cultureInfo);
            int yearSt = startFromYear - 1901;
            int yearEnd = endToYear - 1901;
            // string filename = ((int)lattitude).ToString() + "_" + ((int)longitude).ToString() + "_month" + month.ToString() + ".csv";
            string filename = pointName + "_" + Month[month] + ".csv";
            StreamWriter writer = new(filename);
            int row = (int)((lattitude + 90) * 2);
            int col = (int)((longitude + 180) * 2);
            for (int year = yearSt; year <= yearEnd; year++)
            {
                writer.WriteLine(dataArray[month + year * 12, row, col].ToString(culture));
            }
            writer.Flush();
            writer.Close();
        }
    }
}