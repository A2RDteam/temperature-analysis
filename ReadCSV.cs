using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace temperature_analysis
{
    class ReadCSV
    {
        char Separator;
        string FileName;
        bool Headers;
        string[] Lines;
        double NoValue;
        public ReadCSV(string filename, char sep = ',', bool headers = false)
        {
            Separator = sep;
            FileName = filename;
            Headers = headers;
            NoValue = -200;
        }
        public (int, int) GetDimension()
        {
            int rows;
            int cols;
            Lines = System.IO.File.ReadAllLines(FileName);
            rows = Lines.Length;
            cols = Lines[0].Split(Separator).Length;
            return (rows, cols);
        }
        public int[,] GetDataArray(bool h_flip=false, bool v_flip = false)
        {
            (int rows, int cols) = GetDimension();
            int[,] dataArray = new int[rows, cols];
            int start_row, end_row, inc_row;
            int start_col, end_col, inc_col;
            int data_r = 0, data_c = 0;
            if (h_flip)
            {
                start_col = cols - 1;
                end_col = 0;
                inc_col = -1;
                
            }
            else
            {
                start_col = 0;
                end_col = cols - 1;
                inc_col = 1;
            }
            if (v_flip)
            {
                start_row = rows - 1;
                end_row = 0;
                inc_row = -1;
            }
            else
            {
                start_row = 0;
                end_row = rows - 1;
                inc_row = 1;
            }

            for(int r_c = start_row; r_c * inc_row <= end_row; r_c += inc_row)
            {
                string[] oneline = Lines[r_c].Split(Separator);
                for(int c_c = start_col; c_c * inc_col <= end_col; c_c += inc_col)
                {
                    dataArray[data_r, data_c] = Convert.ToInt32(StrToDouble(oneline[c_c]));
                }
            }
            return dataArray;
        }
        public double [,,] GetDataBlock(int blockCount, int blockSize, int startBlock=0, bool emptyStr = true)
        {
            const Int32 BufferSize = 4096;
            using var fileStream = File.OpenRead(FileName);
            using var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize);
            String line;
            int lineCounter = 0, blockCounter = 0, rowCounter=0, columnCounter = 0;
            
            int interblockLines=0;
            if (emptyStr)
                interblockLines = startBlock;
            int interblockEndLines = 0;
            if (emptyStr)
                interblockEndLines = blockCount - 1;

            int startLine = startBlock * blockSize + interblockLines;
            int lastLine = startLine + blockCount * blockSize + interblockEndLines;
            double[,,] dataArray = new double[1,1,1];
            int lineLen = 0;
            while ((line = streamReader.ReadLine()) != null)
            {
                string[] valuesStr = line.Split(Separator);
                if(lineLen == 0)
                {
                    lineLen = valuesStr.Length;
                    dataArray = new double[blockCount, blockSize, lineLen];
                }
                if (lineCounter >= lastLine)
                    break;
                if (line.Length < 2)
                {
                    blockCounter++;
                    rowCounter = 0;
                    lineCounter++;
                    continue;
                }
                columnCounter = 0;
                foreach(string value in valuesStr)
                {
                    if(value == "NaN")
                    {
                        dataArray[blockCounter, rowCounter, columnCounter] = NoValue;
                    }
                    else
                    {
                        dataArray[blockCounter, rowCounter, columnCounter] = StrToDouble(value);
                    }
                    columnCounter++;
                }
                rowCounter++;
                lineCounter++;
            }
            return dataArray;
        }
        public static int StrToInt(string str, int defValue = -1)
        {
            if (int.TryParse(str, out int val))
            {
                return val;
            }
            else
            {
                Console.WriteLine($"could not parse {str} to int ");
                return defValue;
            }
        }
        public static double StrToDouble(string str, double defValue = -1.0)
        {
            if (double.TryParse(str, NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out double val))
            {
                return val;
            }
            else
            {
                Console.WriteLine($"could not parse {str} to double ");
                return defValue;
            }
        }
    }
}
