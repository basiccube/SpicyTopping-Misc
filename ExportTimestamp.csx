// Based on ExportAllStrings.csx
// Edited by basiccube

using System.Text;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UndertaleModLib.Util;

EnsureDataLoaded();

string outputPath = $"{AppDomain.CurrentDomain.BaseDirectory}ExportedRooms\\timestamp.txt";
if (outputPath == null) throw new ScriptException("The output path was not set.");

using (StreamWriter writer = new StreamWriter(outputPath))
{
	writer.WriteLine(Data.GeneralInfo.Timestamp.ToString());
}
