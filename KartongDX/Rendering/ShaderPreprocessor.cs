using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;

namespace KartongDX.Rendering
{
    class ShaderPreprocessor
    {
        private const string VS_INPUT_REGEX = @"VS_Input[\n\r ]+\{((?:.|\n)*?)\};";

        public static D3D11.InputElement[] Preprocess(string shaderText)
        {
            string str = ExtractVSInput(shaderText);

            D3D11.InputElement[] elements = CreateInputElements(str); // Extract VS Input
			return elements;
        }

        public static string ExtractVSInput(string shaderText)
        {
            RegexOptions options = RegexOptions.Multiline;

            Regex regex = new Regex(VS_INPUT_REGEX, options);
            Match match = regex.Match(shaderText);


            return match.Groups[1].Value.Trim('{', '}');
        }

        public static D3D11.InputElement[] CreateInputElements(string vsInputStr)
        {

            vsInputStr = vsInputStr.Trim(' ', '\n', '\r');
            string[] lines = vsInputStr.Split('\n');


            List<D3D11.InputElement> inputElements = new List<D3D11.InputElement>();

            int offset = 0;
            foreach (string input in lines)
            {
                D3D11.InputElement element;

                try // TODO This is really bad fix this
                {
                    element = MatchFloat(input, ref offset);
                }
                catch
                {
                    element = MatchUint(input, ref offset);
                }


                if (element != null)
                    inputElements.Add(element);
            }
            return inputElements.ToArray();
        }

        private static D3D11.InputElement MatchFloat(string str, ref int offset)
        {
            Regex regex = new Regex("float([0-4 ]).+ : (.+);");
            Match match = regex.Match(str);

            if (match == null || !match.Success)
            {
                throw new Exception();
            }

            string floatAppendix = match.Groups[1].Value;
            string name = match.Groups[2].Value;

            int thisoffset = offset;

            if (floatAppendix == " ")
                offset += 4;
            else
                offset += int.Parse(floatAppendix) * 4;

            Format format = GetFloatFormat(floatAppendix);

            return new D3D11.InputElement(name, 0, format, thisoffset, 0, D3D11.InputClassification.PerVertexData, 0);
        }

        private static D3D11.InputElement MatchUint(string str, ref int offset)
        {
            Regex regex = new Regex("uint([0-4 ]).+ : (.+);");
            Match match = regex.Match(str);

            if (match == null)
                throw new Exception();

            string floatAppendix = match.Groups[1].Value;
            string name = match.Groups[2].Value;

            int thisoffset = offset;

            if (floatAppendix == " ")
                offset += 4;
            else
                offset += int.Parse(floatAppendix) * 4;

            Format format = GetUintFormat(floatAppendix);

            return new D3D11.InputElement(name, 0, format, thisoffset, 0, D3D11.InputClassification.PerVertexData, 0);
        }

        private static Format GetFloatFormat(string appendix)
        {
            Format? format = null;

            if (appendix == " ")
            {
                format = Format.R32_Float;
            }
            else
            {
                if (appendix == "2")
                    format = Format.R32G32_Float;
                else if (appendix == "3")
                    format = Format.R32G32B32_Float;
                else if (appendix == "4")
                    format = Format.R32G32B32A32_Float;
            }
            if (format == null)
                throw new Exception(string.Format("No format could be determined from float appendix: {0}", appendix));

            return format.Value;
        }

        private static Format GetUintFormat(string appendix)
        {
            Format? format = null;

            if (appendix == " ")
            {
                format = Format.R32_UInt;
            }
            else
            {
                if (appendix == "2")
                    format = Format.R32G32_UInt;
                else if (appendix == "3")
                    format = Format.R32G32B32_UInt;
                else if (appendix == "4")
                    format = Format.R32G32B32A32_UInt;
            }
            if (format == null)
                throw new Exception(string.Format("No format could be determined from uint appendix: {0}", appendix));

            return format.Value;
        }

        public static bool IsDefined(string file, string define, bool fallback)
        {
            Regex regex = new Regex(@"#define\s" + define);
            Match match = regex.Match(file);
            if (match.Success)
                return true;
            return fallback;
        }


    }
}
