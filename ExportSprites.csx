/*
    Exports sprites as a GIF.
    Script made by CST1229, with parts based off of ExportAllSprites.csx.
    
    Was originally ExportSpritesAsGIFDLL.csx and used an external library,
    but UTMT now uses ImageMagick and that has gif support so I'm using it.
 */

// revision 2: handle breaking Magick.NET changes

// edited by basiccube to work with CLI with no user input
// and to ignore certain PT sprites since this already takes 6000 years

using System;
using System.Collections.Generic;
using System.Reflection;
using System.IO;
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

await ExtractSprites(folder);

async Task ExtractSprites(string folder)
{
    using TextureWorker worker = new TextureWorker();
    IList<UndertaleSprite> sprites = new List<UndertaleSprite> { };
    foreach (UndertaleSprite sprite in Data.Sprites)
	{
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
}