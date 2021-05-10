using System.Text.RegularExpressions;
using UnityEngine;

namespace PathfinderAutoBuff.Utility.Extensions
/*
* Based on Hsinyu Chan KingmakerModMaker
* https://github.com/hsinyuhcan/KingmakerModMaker
* And Cabarius ToyBox
* https://github.com/cabarius/ToyBox/
* https://docs.unity3d.com/Manual/StyledText.html
*/

{
    public static class RichTextExtensions
    {

        public enum RGBA : uint
        {
            aqua = 0x00ffffff,
            black = 0x000000ff,
            blue = 0x0000ffff,
            brown = 0xa52a2aff,
            cyan = 0x00ffffff,
            darkblue = 0x0000a0ff,
            darkgrey = 0x808080ff,
            fuchsia = 0xff00ffff,
            green = 0x008000ff,
            grey = 0x808080ff,
            lightblue = 0xadd8e6ff,
            lightgrey = 0xE8E8E8ff,
            lime = 0x00ff00ff,
            magenta = 0xff00ffff,
            midnightblue = 0x151B54ff,
            maroon = 0x800000ff,
            navy = 0x000080ff,
            olive = 0x808000ff,
            orange = 0xffa500ff,
            purple = 0x800080ff,
            red = 0xff0000ff,
            silver = 0xc0c0c0ff,
            teal = 0x008080ff,
            white = 0xffffffff,
            yellow = 0xffff00ff,
            test = 0x010101ff
        }

        public static string Bold(this string str)
        {
            return $"<b>{str}</b>";
        }
        public static string Italic(this string str)
        {
            return $"<i>{str}</i>";
        }

        public static string Color(this string str, Color color)
        {
            return $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{str}</color>";
        }

        public static string Color(this string str, RGBA color)
        {
            return $"<color=#{color:x}>{str}</color>";
        }

        public static string Color(this string str, string rrggbbaa)
        {
            return $"<color=#{rrggbbaa}>{str}</color>";
        }

        public static string White(this string s) { return s = s.Color(RGBA.white); }
        public static string Grey(this string s) { return s = s.Color(RGBA.grey); }
        public static string Red(this string s) { return s = s.Color(RGBA.red); }
        public static string Purple(this string s) { return s = s.Color(RGBA.purple); }
        public static string Green(this string s) { return s = s.Color(RGBA.green); }
        public static string Blue(this string s) { return s = s.Color(RGBA.blue); }
        public static string Cyan(this string s) { return s = s.Color(RGBA.cyan); }
        public static string Magenta(this string s) { return s = s.Color(RGBA.magenta); }
        public static string Yellow(this string s) { return s = s.Color(RGBA.yellow); }
        public static string Orange(this string s) { return s = s.Color(RGBA.orange); }

        public static string ToSentence(this string str)
        {
            return Regex.Replace(str, @"((?<=\p{Ll})\p{Lu})|\p{Lu}(?=\p{Ll})", " $0").TrimStart();
            //return string.Concat(str.Select(c => char.IsUpper(c) ? " " + c : c.ToString())).TrimStart(' ');
        }

        public static string Size(this string str, int size)
        {
            return $"<size={size}>{str}</size>";
        }

        public static string SizePercent(this string str, int percent)
        {
            return $"<size={percent}%>{str}</size>";
        }

        public static Color UIntToColor(uint color)
        {
            float a = ((byte)(color >> 24)) / 256f;
            float r = ((byte)(color >> 16)) / 256f;
            float g = ((byte)(color >> 8)) / 256f;
            float b = ((byte)(color >> 0)) / 256f;
            return new Color(r,g,b,a);
        }

        public static Color RGBAToColor(uint color)
        {
            float r = ((byte)(color >> 24))/ 256f;
            float g = ((byte)(color >> 16))/ 256f;
            float b = ((byte)(color >> 8))/ 256f;
            float a = ((byte)(color >> 0))/ 256f;
            return new Color(r, g, b, a);
        }
    }
}
