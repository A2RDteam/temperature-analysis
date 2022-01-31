using System;

namespace temperature_analysis
{
    class Program
    {
        static void Main(string[] args)
        {
            // if need to save to DB (it takes a lot of time)
            bool isSaveDB = false;
            // if need to save to CSV files
            bool isSaveCSV = false;
            // if need to create single point files (for creating plots)
            bool isSaveOnePoints = true;
            // arrays for single points savings (coordinates: lat, long and names)
            double[] lattiutudes = { 77.99, 73.485, 70.61, 55.956, -2.088, 63.5 };
            double[] longitudes = { 15.12, 80.828, -159.8859, 107.6825, -65.8868, 120.5 };
            string[] pointNames = { "Svalbard", "Taimyr", "Alaska", "Northern Baikal", "Upper Amazon", "Yakutia" };

            ReadCSV tempData = new("D:\\Работа\\weather_index\\tmp_1901-2020.csv");
            // reading all data to memory (using a lot memory (3GB), but faster operations)
            var dataArray = tempData.GetDataBlock(12 * 120, 360);

            // creating average data for period
            var ave120y = Average(1901, 2020, dataArray);
            var ave1901_1930 = Average(1901, 1930, dataArray);
            var ave1931_1960 = Average(1931, 1960, dataArray);
            var ave1961_1990 = Average(1961, 1990, dataArray);
            var ave1991_2020 = Average(1991, 2020, dataArray);

            var ave1981_2010 = Average(1981, 2010, dataArray);
            var ave2011_2020 = Average(2011, 2020, dataArray);

            // substracting one data from other, to see visual differences in GIS (QGIS or any)
            var dif30_4_ave120 = Difference(ave1991_2020, ave120y);
            var dif30_4_30_3 = Difference(ave1991_2020, ave1961_1990);
            var dif10_30 = Difference(ave2011_2020, ave1981_2010);

            // save data to SpatiaLite db file. You need prepared db file - you can create it with QGIS
            if (isSaveDB)
            {
                Spatialite spatialite = new(DBpath: "spatialite3.sqlite", cultureInfo: "en - US");
                spatialite.SaveToDB("Ave120y", ave120y);

                spatialite.SaveToDB("Ave30_1901", ave1901_1930);
                spatialite.SaveToDB("Ave30_1931", ave1931_1960);
                spatialite.SaveToDB("Ave30_1961", ave1961_1990);
                spatialite.SaveToDB("Ave30_1991", ave1991_2020);

                spatialite.SaveToDB("Diff_last_30_to_Ave120", dif30_4_ave120);
                spatialite.SaveToDB("Diff_last_30_to_prev30", dif30_4_30_3);
                spatialite.SaveToDB("Diff_last_10_to_prev30", dif10_30);
            }

            // saving data to CSV files for for further computer processing (if you need to)
            if (isSaveCSV)
            {
                WriteCSV.WriteMonths("Ave30_1901", ave1901_1930);
                WriteCSV.WriteMonths("Ave30_1931", ave1931_1960);
                WriteCSV.WriteMonths("Ave30_1961", ave1961_1990);
                WriteCSV.WriteMonths("Ave30_1991", ave1991_2020);

                WriteCSV.WriteMonths("Diff_last_30_to_Ave120", dif30_4_ave120);
                WriteCSV.WriteMonths("Diff_last_30_to_prev30", dif30_4_30_3);
                WriteCSV.WriteMonths("Diff_last_10_to_prev30", dif10_30);
            }

            // saving single points for further plot creation (in R)
            if (isSaveOnePoints)
            {
                for (int mon = 0; mon < 12; mon++)
                {
                    for (int p = 0; p < lattiutudes.Length; p++)
                    {
                        WriteCSV.WriteOnePoint(dataArray, month: mon, lattiutudes[p], longitudes[p], pointNames[p]);
                    }
                }

            }

        }
        static double[,,] Average(int startYear, int endYear, double[,,] data)
        {
            int yearSt = startYear - 1901;
            int yearEnd = endYear - 1901;
            int columnsNum = data.GetLength(2);
            int rowsNum = 360;
            double[,,] aveMonth = new double[12, rowsNum, columnsNum];
            // Sum
            for (int month = 0; month < 12; month++)
            {
                for (int row = 0; row < rowsNum; row++)
                {
                    for (int col = 0; col < columnsNum; col++)
                    {
                        aveMonth[month, row, col] = 0;
                        for (int year = yearSt; year <= yearEnd; year++)
                        {
                            if (data[month + year * 12, row, col] != -200)
                                aveMonth[month, row, col] += data[month + year * 12, row, col];
                            else
                                aveMonth[month, row, col] += double.NaN;
                        }
                    }
                }
            }
            // Average
            for (int month = 0; month < 12; month++)
            {
                for (int row = 0; row < rowsNum; row++)
                {
                    for (int col = 0; col < columnsNum; col++)
                    {
                        aveMonth[month, row, col] /= (endYear - startYear + 1);
                    }
                }
            }
            return aveMonth;
        }

        static double[,,] Difference(double[,,] data1, double[,,] data2)
        {
            int columnsNum = data1.GetLength(2);
            int rowsNum = 360;
            double[,,] difMonth = new double[12, rowsNum, columnsNum];
            for (int month = 0; month < 12; month++)
            {
                for (int row = 0; row < rowsNum; row++)
                {
                    for (int col = 0; col < columnsNum; col++)
                    {
                        difMonth[month, row, col] = data1[month, row, col] - data2[month, row, col];
                    }
                }
            }
            return difMonth;
        }
    }
}
