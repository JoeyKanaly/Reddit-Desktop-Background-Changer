﻿using System;
using System.Runtime.InteropServices;
using System.ComponentModel;
using Microsoft.Win32;

namespace RedditBackgroundChanger
{
    public static class Wallpaper
    {
        /// <summary>
        /// Determine if .jpg files are supported as wallpaper in the current 
        /// operating system. The .jpg wallpapers are not supported before 
        /// Windows Vista.
        /// </summary>
        public static bool SupportJpgAsWallpaper
        {
            get
            {
                return (Environment.OSVersion.Version >= new Version(6, 0));
            }
        }


        /// <summary>
        /// Determine if the fit and fill wallpaper styles are supported in 
        /// the current operating system. The styles are not supported before 
        /// Windows 7.
        /// </summary>
        public static bool SupportFitFillWallpaperStyles
        {
            get
            {
                return (Environment.OSVersion.Version >= new Version(6, 1));
            }
        }


        /// <summary>
        /// Set the desktop wallpaper.
        /// </summary>
        /// <param name="path">Path of the wallpaper</param>
        /// <param name="style">Wallpaper style</param>
        public static void SetDesktopWallpaper(string path, WallpaperStyle style)
        {
            // Set the wallpaper style and tile. 
            // Two registry values are set in the Control Panel\Desktop key.
            // TileWallpaper
            //  0: The wallpaper picture should not be tiled 
            //  1: The wallpaper picture should be tiled 
            // WallpaperStyle
            //  0:  The image is centered if TileWallpaper=0 or tiled if TileWallpaper=1
            //  2:  The image is stretched to fill the screen
            //  6:  The image is resized to fit the screen while maintaining the aspect 
            //      ratio. (Windows 7 and later)
            //  10: The image is resized and cropped to fill the screen while 
            //      maintaining the aspect ratio. (Windows 7 and later)
            RegistryKey key = Registry.CurrentUser.OpenSubKey(@"Control Panel\Desktop", true);

            switch (style)
            {
                case WallpaperStyle.Tile:
                    key.SetValue(@"WallpaperStyle", "0");
                    key.SetValue(@"TileWallpaper", "1");
                    break;
                case WallpaperStyle.Center:
                    key.SetValue(@"WallpaperStyle", "0");
                    key.SetValue(@"TileWallpaper", "0");
                    break;
                case WallpaperStyle.Stretch:
                    key.SetValue(@"WallpaperStyle", "2");
                    key.SetValue(@"TileWallpaper", "0");
                    break;
                case WallpaperStyle.Fit: // (Windows 7 and later)
                    key.SetValue(@"WallpaperStyle", "6");
                    key.SetValue(@"TileWallpaper", "0");
                    break;
                case WallpaperStyle.Fill: // (Windows 7 and later)
                    key.SetValue(@"WallpaperStyle", "10");
                    key.SetValue(@"TileWallpaper", "0");
                    break;
            }

            key.Close();

            // Set the desktop wallpapaer by calling the Win32 API SystemParametersInfo 
            // with the SPI_SETDESKWALLPAPER desktop parameter. The changes should 
            // persist, and also be immediately visible.
            if (!SystemParametersInfo(SPI_SETDESKWALLPAPER, 0, path,
                SPIF_UPDATEINIFILE | SPIF_SENDWININICHANGE))
            {
                throw new Win32Exception();
            }
        }


        private const uint SPI_SETDESKWALLPAPER = 20;
        private const uint SPIF_UPDATEINIFILE = 0x01;
        private const uint SPIF_SENDWININICHANGE = 0x02;

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool SystemParametersInfo(uint uiAction, uint uiParam,
            string pvParam, uint fWinIni);
    }

    public enum WallpaperStyle
    {
        Tile,
        Center,
        Stretch,
        Fit,
        Fill
    }
}
