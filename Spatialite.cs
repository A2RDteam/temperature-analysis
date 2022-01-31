using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace temperature_analysis
{
    class Spatialite
    {
        string DBname ;
        static string[] Month = { "Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec" };
        CultureInfo culture;
        public Spatialite(string DBpath, string cultureInfo = "en-US")
        {
            DBname = DBpath;
            culture = CultureInfo.CreateSpecificCulture(cultureInfo);
        }
        /// <summary>
        /// Saving data array to SpatialiteDB (POLYGONS)
        /// table tableName will dropped first!
        /// </summary>
        /// <param name="tableName">name for table to save(it will be dropped before!)</param>
        /// <param name="arr">data (month, row, column)</param>
        public void SaveToDB(string tableName, double[,,] arr)
        {
            string constr = "Data Source=" + DBname + ";";
            SqliteConnection connection = new(constr);
            connection.Open();
            connection.EnableExtensions(true);
            string sql = @"SELECT load_extension('mod_spatialite.dll');";
            SqliteCommand sqliteCommand = new(sql, connection);
            sqliteCommand.ExecuteNonQuery();
            string table;
            int rows = arr.GetLength(1);
            int cols = arr.GetLength(2);
            for (int mon = 0; mon < 12; mon++)
            {
                table =  tableName + "_" + Month[mon];
                sql = "DROP TABLE IF EXISTS " + table + ";";
                sqliteCommand = new(sql, connection);
                sqliteCommand.ExecuteNonQuery();

                sql = "CREATE TABLE " + table + "(ID INTEGER NOT NULL PRIMARY KEY, Row INTEGER, Col INTEGER, Value REAL);";
                sqliteCommand = new(sql, connection);
                sqliteCommand.ExecuteNonQuery();

                sql = "SELECT AddGeometryColumn('" + table + "', 'Pixel', 4326, 'POLYGON', 'XY'); ";
                sqliteCommand = new(sql, connection);
                sqliteCommand.ExecuteNonQuery();

                for (int r = 0; r < rows; r++)
                {
                    using var transaction = connection.BeginTransaction();
                    using var command = connection.CreateCommand();
                    command.Transaction = transaction;
                    for (int c = 0; c < cols; c++)
                    {
                        // x - longitude y-lattitude A-B-D-C
                        double xA, yA, xB, yB, xC, yC, xD, yD;
                        xC = xA = c / 2.0 - 180;
                        yB = yA = r / 2.0 - 90;
                        xD = xB = xA + 0.5;
                        yD = yC = yA + 0.5;
                        string value;
                        if (double.IsNaN(arr[mon, r, c]))
                            continue;
                        else
                            value = "'" + arr[mon, r, c].ToString(culture) + "'";

                        sql = "INSERT INTO "
                            + table
                            + "(Row, Col, Pixel, Value) VALUES('"
                            + r
                            + "','"
                            + c
                            + "',GeomFromText('POLYGON((" + xA.ToString(culture) + " " + yA.ToString(culture) + ", " + xB.ToString(culture) + " " + yB.ToString(culture) + ", " + xD.ToString(culture) + " " + yD.ToString(culture) + ", " + xC.ToString(culture) + " " + yC.ToString(culture) + "))', 4326),"
                            + value
                            + ");";
                        command.CommandText = sql;
                        command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                }

            }
        }
    }
}
