// Exports some info about the data.win file

using System.Text;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using UndertaleModLib.Util;

EnsureDataLoaded();

string roomPath = $"{AppDomain.CurrentDomain.BaseDirectory}ExportedRooms\\";
if (!Directory.Exists(roomPath))
	Directory.CreateDirectory(roomPath);

string timestampPath = Path.Combine(roomPath, "timestamp.txt");
string namePath = Path.Combine(roomPath, "name.txt");

using (StreamWriter writer = new StreamWriter(timestampPath))
{ writer.WriteLine(Data.GeneralInfo.Timestamp.ToString()); }

using (StreamWriter writer = new StreamWriter(namePath))
{ writer.WriteLine(Data.GeneralInfo.Name.Content); }
