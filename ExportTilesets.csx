// ExportAllTilesets.csx - but with no user input
// and some other info gets exported as well

// 2026-05-01: cleanup

using System.Text;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using UndertaleModLib.Util;

EnsureDataLoaded();

string tilePath = $"{AppDomain.CurrentDomain.BaseDirectory}ExportedTilesets\\";
if (tilePath is null)
    return;

if (!Directory.Exists(tilePath))
	Directory.CreateDirectory(tilePath);

JsonWriterOptions writerOptions = new JsonWriterOptions {
	Indented = true,
	IndentCharacter = '\t',
	IndentSize = 1
};

SetProgressBar(null, "Tilesets", 0, Data.Backgrounds.Count);
StartProgressBarUpdater();

TextureWorker worker = null;
using (worker = new())
{ await DumpTilesets(); }

await StopProgressBarUpdater();
HideProgressBar();

async Task DumpTilesets()
{ await Task.Run(() => Parallel.ForEach(Data.Backgrounds, DumpTileset)); }

void DumpTileset(UndertaleBackground tileset)
{
    if (tileset?.Texture is not null)
    {
        worker.ExportAsPNG(tileset.Texture, Path.Combine(tilePath, $"{tileset.Name.Content}.png"));
		
		// write JSON
		using MemoryStream stream = new MemoryStream();
		using Utf8JsonWriter writer = new Utf8JsonWriter(stream, writerOptions);
		writer.WriteStartObject();
		
		writer.WriteNumber("Width", tileset.GMS2TileWidth);
		writer.WriteNumber("Height", tileset.GMS2TileHeight);
		
		writer.WriteNumber("BorderX", tileset.GMS2OutputBorderX);
		writer.WriteNumber("BorderY", tileset.GMS2OutputBorderY);
		
		writer.WriteEndObject();
		writer.Flush();
		
		File.WriteAllBytes(Path.Combine(tilePath, $"{tileset.Name.Content}.json"), stream.ToArray());
    }

    IncrementProgressParallel();
}