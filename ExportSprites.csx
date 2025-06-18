/*
    Exports sprites as a GIF.
    Script made by CST1229, with parts based off of ExportAllSprites.csx.
    
    Was originally ExportSpritesAsGIFDLL.csx and used an external library,
    but UTMT now uses ImageMagick and that has gif support so I'm using it.
 */

// revision 2: handle breaking Magick.NET changes

// edited by basiccube to work with CLI with no user input
// and to ignore certain PT sprites since this already takes 6000 years
// 06-18: added sprite origin exporting

using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using UndertaleModLib.Models;
using UndertaleModLib.Util;
using UndertaleModLib.Scripting;
using ImageMagick;

EnsureDataLoaded();

string folder = $"{AppDomain.CurrentDomain.BaseDirectory}ExportedSprites\\";
if (folder is null)
{
    return;
}
if (!Directory.Exists(folder))
{
	Directory.CreateDirectory(folder);
}

JsonWriterOptions writerOptions = new JsonWriterOptions { Indented = true };

await ExtractSprites(folder);

async Task ExtractSprites(string folder)
{
    using TextureWorker worker = new TextureWorker();
    IList<UndertaleSprite> sprites = new List<UndertaleSprite> { };
    foreach (UndertaleSprite sprite in Data.Sprites)
	{
		// horrible but don't care enough to do something about it
		if (!sprite.Name.Content.StartsWith("spr_player") &&
			!sprite.Name.Content.StartsWith("spr_snick_") &&
			!sprite.Name.Content.StartsWith("spr_bombpep") &&
			!sprite.Name.Content.StartsWith("spr_cheesepep") &&
			!sprite.Name.Content.StartsWith("spr_knightpep") &&
			!sprite.Name.Content.StartsWith("spr_manual") &&
			!sprite.Name.Content.StartsWith("spr_climbstairs") &&
			!sprite.Name.Content.StartsWith("spr_pepinoHUD") &&
			!sprite.Name.Content.StartsWith("tile_") &&
			!sprite.Name.Content.StartsWith("spr_shotgun_") &&
			!sprite.Name.Content.StartsWith("spr_boxxedpep") &&
			!sprite.Name.Content.StartsWith("spr_hungrypillar") &&
			!sprite.Name.Content.StartsWith("spr_noise") &&
			!sprite.Name.Content.StartsWith("spr_rank") &&
			!sprite.Name.Content.StartsWith("spr_sausageman") &&
			!sprite.Name.Content.StartsWith("spr_pepperman") &&
			!sprite.Name.Content.StartsWith("spr_pizzagoblin") &&
			!sprite.Name.Content.StartsWith("spr_rail") &&
			!sprite.Name.Content.StartsWith("spr_slime") &&
			!sprite.Name.Content.StartsWith("spr_toppin") &&
			!sprite.Name.Content.StartsWith("spr_tv_") &&
			!sprite.Name.Content.StartsWith("spr_xmas") &&
			sprite.Textures.Count != 0)
		{
			sprites.Add(sprite);
		}
	}

    SetProgressBar(null, "Exporting sprites to GIF...", 0, sprites.Count);
    StartProgressBarUpdater();

    bool isParallel = true;
    await Task.Run(() => 
    {
        if (isParallel) 
        {
            Parallel.ForEach(sprites, (sprite) => 
            {
                IncrementProgressParallel();
				ExtractSprite(sprite, folder, worker);
            });
        } 
        else 
        {
            foreach (UndertaleSprite sprite in sprites) 
            {
				ExtractSprite(sprite, folder, worker);
                IncrementProgressParallel();
            }
        }
    });
    await StopProgressBarUpdater();
    HideProgressBar();
}

void ExtractSprite(UndertaleSprite sprite, string folder, TextureWorker worker)
{
    using MagickImageCollection gif = new();
    for (int picCount = 0; picCount < sprite.Textures.Count; picCount++)
    {
        if (sprite.Textures[picCount]?.Texture != null)
        {
            IMagickImage<byte> image = worker.GetTextureFor(sprite.Textures[picCount].Texture, sprite.Name.Content + " (frame " + picCount + ")", true);
            image.GifDisposeMethod = GifDisposeMethod.Previous;
            // the animation delay unit seems to be 100 per second, not milliseconds (1000 per second)
            if (sprite.IsSpecialType && Data.IsGameMaker2()) 
            {
                if (sprite.GMS2PlaybackSpeed == 0f) 
                {
                    image.AnimationDelay = 10;
                } 
                else if (sprite.GMS2PlaybackSpeedType is AnimSpeedType.FramesPerGameFrame) 
                {
                    image.AnimationDelay = (uint)Math.Max((int)(Math.Round(100f / (sprite.GMS2PlaybackSpeed * Data.GeneralInfo.GMS2FPS))), 1);
                } 
                else 
                {
                    image.AnimationDelay = (uint)Math.Max((int)(Math.Round(100 / sprite.GMS2PlaybackSpeed)), 1);
                }
            } 
            else 
            {
                image.AnimationDelay = 3; // 30fps
            }
            gif.Add(image);
        }
    }
    gif.Optimize();
    gif.Write(Path.Join(folder, sprite.Name.Content + ".gif"));
	
	// write JSON for sprite origins
	using MemoryStream stream = new MemoryStream();
    using Utf8JsonWriter writer = new Utf8JsonWriter(stream, writerOptions);
    writer.WriteStartObject();
	
	writer.WriteNumber("OriginX", sprite.OriginX);
    writer.WriteNumber("OriginY", sprite.OriginY);
	
	writer.WriteEndObject();
    writer.Flush();
	
    File.WriteAllBytes(Path.Join(folder, sprite.Name.Content + ".json"), stream.ToArray());
}