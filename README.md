# temperature-analysis

some functions
ReadCSV.GetDataBlock - read all data bloc to memory.
arguments:
 blockCount - number of blocks to read. 12*120 - 12 month * 120 years
 blockSize - size of one block. In these dataset its 360 lines: (-90 - 90 degrees) * 2. every line is for 0.5 degree.
 emptyStr - is blocks separated with empty string? In current dataset blocks is separated.
returns:
 3-dimension array [block, row, column] where block represents one month of one year (0 - 1440), row and columns represents square with 0.5 degree side

Average - calculates average temperature, for every separated month, year by year. 
 startYear, endYear - period set by start and end years for averaging. years sets in natural way: "1901" "1961" for example.
 data - array with all dataset (red by GetDataBlock)
returns:
 3-dimension array [month, row, column] where month (0-11) represents one month data averaged for period of years. For example, Januar (0) is average from all Januars  from 1901 to 1930.

Difference - substracts one averaged data from another. For example substract average for period 1991-2020 from average for period 1961-1990.
 data1, data2 - data types, arrays that returned by Average  function.
returns:
 same array as Average returns.
