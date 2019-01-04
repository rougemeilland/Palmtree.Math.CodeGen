﻿using System;
using System.IO;
using System.Linq;

namespace Palmtree.Math.CodeGen
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var str = Console.OpenStandardOutput())
            using (var writer = new StreamWriter(str))
            {
                writer.WriteLine(string.Join("\n", new string[] {
                    "/*",
                    " * The MIT License",
                    " *",
                    " * Copyright 2019 Palmtree Software.",
                    " *",
                    " * Permission is hereby granted, free of charge, to any person obtaining a copy",
                    " * of this software and associated documentation files (the \"Software\"), to deal",
                    " * in the Software without restriction, including without limitation the rights",
                    " * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell",
                    " * copies of the Software, and to permit persons to whom the Software is",
                    " * furnished to do so, subject to the following conditions:",
                    " *",
                    " * The above copyright notice and this permission notice shall be included in",
                    " * all copies or substantial portions of the Software.",
                    " *",
                    " * THE SOFTWARE IS PROVIDED \"AS IS\", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR",
                    " * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,",
                    " * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE",
                    " * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER",
                    " * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,",
                    " * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN",
                    " * THE SOFTWARE.",
                    " */",
                    "",
                    "/* ",
                    " * File:   autogenerated.h",
                    " * Author: Lunor Kisasage",
                    " *",
                    " * Created on 2019/01/05, 0:18",
                    " */",
                    "",
                    "#ifndef AUTOGENERATED_H",
                    "#define AUTOGENERATED_H",
                    "",
                    "#ifdef __cplusplus",
                    "extern \"C\" {",
                    "#endif",
                    "",
                    "" }));
                writer.WriteLine("#include \"pmc_internal.h\"");
                writer.WriteLine("");
                writer.WriteLine("");
                Generate(writer);

                writer.WriteLine(string.Join("\n", new string[] {
                    "",
                    "#ifdef __cplusplus",
                    "}",
                    "#endif",
                    "",
                    "#endif /* AUTOGENERATED_H */",
                    "",
                    "/*",
                    " * END OF FILE",
                    " */"}));

            }
        }

        static void Generate(TextWriter writer)
        {
            //writer.WriteLine("");
            //GenerateADD_SET(writer, 32);
            //writer.WriteLine("");
            //GenerateADD_SET(writer, 16);
            //writer.WriteLine("");
            GenerateADD_SET(writer, 8);
            writer.WriteLine("");
        }

        private static void GenerateADD_SET(TextWriter writer, int max_count)
        {
            GenerateADD(writer, max_count, "adc");
            writer.WriteLine("");
            GenerateADD(writer, max_count, "adcx");
            writer.WriteLine("");
            GenerateADD(writer, max_count, "adox");
        }

        private static void GenerateADD(TextWriter writer, int max_count, string op)
        {
            writer.WriteLine(string.Format("__inline static char _ADD_{0}WORDS_{1}(char c, __UNIT_TYPE* xp, __UNIT_TYPE* yp, __UNIT_TYPE* zp)", max_count, op.ToUpper()));
            writer.WriteLine("{");
            writer.WriteLine("#ifdef _MSC_VER");
            for (int count = 0; count < max_count; ++count)
                writer.WriteLine(string.Format("    {1}(c, xp[{0}], yp[{0}], &zp[{0}]);", count, op == "adc" ? "_ADD_UNIT" : "_ADDX_UNIT"));
            writer.WriteLine("#elif defined(__GNUC__)");
            writer.WriteLine("#ifdef _M_IX86");
            GenerateASM_ADD(writer, max_count, op + "l", "movl", "ecx");
            writer.WriteLine("#elif defined(_M_IX64)");
            GenerateASM_ADD(writer, max_count, op + "q", "movq", "rcx");
            writer.WriteLine("#else");
            writer.WriteLine("#error unknown platform");
            writer.WriteLine("#endif");
            writer.WriteLine("#else");
            writer.WriteLine("#error unknown compiler");
            writer.WriteLine("#endif");
            writer.WriteLine("    return (c);");
            writer.WriteLine("}");
        }

        private static void GenerateASM_ADD(TextWriter writer, int max_count, string op_add, string op_mov, string temp_reg)
        {
            writer.WriteLine("    __asm__ volatile (");
            writer.WriteLine("        \"addb\\t$-1, %0\\n\\t\"");
            for (int count = 0; count < max_count; ++count)
            {
                writer.WriteLine(string.Format("        \"{0}\\t%{1}, %%{2}\\n\\t\"", op_mov, max_count + 1 + count * 2, temp_reg));
                writer.WriteLine(string.Format("        \"{0}\\t%{1}, %%{2}\\n\\t\"", op_add, max_count + 2 + count * 2, temp_reg));
                writer.WriteLine(string.Format("        \"{0}\\t%%{1}, %{2}\\n\\t\"", op_mov, temp_reg, 1 + count));
            }
            writer.WriteLine("        \"setc\\t%0\"");
            writer.WriteLine(string.Format("        : \"+r\"(c), {0}", string.Join(", ", Enumerable.Range(0, max_count).Select(n => string.Format("\"=g\"(zp[{0}])", n)))));
            writer.WriteLine(string.Format("        : {0}", string.Join(", ", Enumerable.Range(0, max_count).Select(n => string.Format("\"g\"(xp[{0}]), \"rm\"(yp[{0}])", n)))));
            writer.WriteLine(string.Format("        : \"cc\", \"%{0}\"", temp_reg));
            writer.WriteLine(");");
        }
    }


}
