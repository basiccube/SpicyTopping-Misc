/*
    Exports sprites as a GIF.
    Script made by CST1229, with parts based off of ExportAllSprites.csx.
    
    Was originally ExportSpritesAsGIFDLL.csx and used an external library,
    but UTMT now uses ImageMagick and that has gif support so I'm using it.
 */

// revision 2: handle breaking Magick.NET changes

// edited by basiccube to work with CLI with no user input
// and to ignore certain PT sprites since this already takes 6000 years
// 2025-06-18: added sprite origin exporting
// 2026-05-01: did some cleanup

using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using UndertaleModLib.Models;
using UndertaleModLib.Util;
using UndertaleModLib.Scripting;
using ImageMagick;

EnsureDataLoaded();

string folder = $"{AppDomain.CurrentDomain.BaseDirectory}ExportedSprites\\";
if (!Directory.Exists(folder))
	Directory.CreateDirectory(folder);

JsonWriterOptions writerOptions = new JsonWriterOptions {
	Indented = true,
	IndentCharacter = '\t',
	IndentSize = 1
};

string[] ignoredSprites = {
	"spr_player",
	"spr_snick_",
	"spr_bombpep",
	"spr_cheesepep",
	"spr_knightpep",
	"spr_manual",
	"spr_climbstairs",
	"spr_pepinoHUD",
	"spr_shotgun_",
	"spr_boxxedpep",
	"spr_hungrypillar",
	"spr_noise",
	"spr_rank",
	"spr_sausageman",
	"spr_pepperman",
	"spr_pizzagoblin",
	"spr_rail",
	"spr_slime",
	"spr_toppin",
	"spr_tv_",
	"spr_xmas",
	"tile_"
};

await ExtractSprites(folder);

async Task ExtractSprites(string folder)
{
    using TextureWorker worker = new TextureWorker();
    IList<UndertaleSprite> sprites = new List<UndertaleSprite> { };
	
    foreach (UndertaleSprite sprite in Data.Sprites)
	{
		if (sprite.Textures.Count != 0 && !ignoredSprites.Any(sprite.Name.Content.StartsWith))
			sprites.Add(sprite);
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
                    image.AnimationDelay = 10;
                else if (sprite.GMS2PlaybackSpeedType is AnimSpeedType.FramesPerGameFrame)
                    image.AnimationDelay = (uint)Math.Max((int)(Math.Round(100f / (sprite.GMS2PlaybackSpeed * Data.GeneralInfo.GMS2FPS))), 1);
                else
                    image.AnimationDelay = (uint)Math.Max((int)(Math.Round(100 / sprite.GMS2PlaybackSpeed)), 1);
            }
            else
                image.AnimationDelay = 3; // 30fps
			
            gif.Add(image);
        }
    }
	
	try { gif.Optimize(); }
	catch (Exception e) { }
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