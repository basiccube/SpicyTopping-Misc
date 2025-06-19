// Exports some info about the data.win file

using System.Text;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UndertaleModLib.Util;

EnsureDataLoaded();

string timestampPath = $"{AppDomain.CurrentDomain.BaseDirectory}ExportedRooms\\timestamp.txt";
string namePath = $"{AppDomain.CurrentDomain.BaseDirectory}ExportedRooms\\name.txt";

using (StreamWriter writer = new StreamWriter(timestampPath))
{
	writer.WriteLine(Data.GeneralInfo.Timestamp.ToString());
}

using (StreamWriter writer = new StreamWriter(namePath))
{
	writer.WriteLine(Data.GeneralInfo.Name.Content);
}
