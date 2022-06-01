using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using STLang.VMInstructions;

namespace STLang.DisAssemble
{
    public class DisAssembler
    {
        private static readonly string styleElement =
           "    <style type=\"text/css\">\r\n" +
           "      table.disassm\r\n" +
           "      {\r\n" +
           "        width: 320px;\r\n" +
           "        text-align: left;\r\n" +
           "        font-family: System;\r\n" +
           "        border-style: solid;\r\n" +
           "        border-width: thin;\r\n" +
           "        border-collapse: collapse;\r\n" +
           "        background-color:#FFFF99;\r\n" +
           "      }\r\n" +
           "      tr.header\r\n" +
           "      {\r\n" +
           "        font-size: 0.9em;\r\n" +
           "        border-bottom: double;\r\n" +
           "        background-color:#FFEC80;\r\n" +
           "      }\r\n" +
           "      td\r\n" +
           "      {\r\n" +
           "        width: 35px;\r\n" +
           "        padding-left: 8px;\r\n" +
           "      }\r\n" +
           "      td.instr\r\n" +
           "      {\r\n" +
           "        padding: 4px;\r\n" +
           "        font-weight: bold;\r\n" +
           "      }\r\n" +
           "      td.hexadec\r\n" +
           "      {\r\n" +
           "        padding: 4px;\r\n" +
           "        border-left: solid;\r\n" +
           "        border-width: thin;\r\n" +
           "        text-align: center;\r\n" +
           "      }\r\n" +
           "    </style>";

        private static readonly string tableHeader =
           "      <tr class=\"header\">\r\n" +
           "        <th>PC</th>\r\n" +
           "        <th>INSTR</th>\r\n" +
           "        <th>OPD</th>\r\n" +
           "        <th>HEX</th>\r\n" +
           "      </tr>";

        private static void GenerateTableRow(StreamWriter writer, int lineNo, string instr, object operand, uint word)
        {
            string hexValue = word.ToString("X8");
            string tableRow = "      <tr>\r\n"
                            + "        <td>" + lineNo + "</td>\r\n"
                            + "        <td class=\"instr\">" + instr + "</td>\r\n"
                            + "        <td>" + operand + "</td>\r\n"
                            + "        <td class=\"hexadec\">" + hexValue + "</td>\r\n"
                            + "      </tr>";
            writer.WriteLine(tableRow);
        }

        private static bool IsLoadStoreInstruction(VirtualMachineInstruction instruction)
        {
            switch (instruction)
            {
                case VirtualMachineInstruction.ILOD:
                case VirtualMachineInstruction.WLOD:
                case VirtualMachineInstruction.LLOD:
                case VirtualMachineInstruction.FLOD:
                case VirtualMachineInstruction.DLOD:
                case VirtualMachineInstruction.SLOD:
                case VirtualMachineInstruction.WSLOD:
                case VirtualMachineInstruction.BSTO:
                case VirtualMachineInstruction.ISTO:
                case VirtualMachineInstruction.WSTO:
                case VirtualMachineInstruction.LSTO:
                case VirtualMachineInstruction.FSTO:
                case VirtualMachineInstruction.DSTO:
                case VirtualMachineInstruction.SSTO:
                case VirtualMachineInstruction.WSSTO:
                case VirtualMachineInstruction.ILODX:
                case VirtualMachineInstruction.WLODX:
                case VirtualMachineInstruction.LLODX:
                case VirtualMachineInstruction.FLODX:
                case VirtualMachineInstruction.DLODX:
                case VirtualMachineInstruction.SLODX:
                case VirtualMachineInstruction.BSTOX:
                case VirtualMachineInstruction.ISTOX:
                case VirtualMachineInstruction.WSTOX:
                case VirtualMachineInstruction.LSTOX:
                case VirtualMachineInstruction.FSTOX:
                case VirtualMachineInstruction.DSTOX:
                case VirtualMachineInstruction.SSTOX:
                case VirtualMachineInstruction.WSSTOX:
                    return true;
                default:
                    return false;
            }
        }

        public static void DisassembleByteCode(List<uint> byteCode, string functionName)
        {
            string fileName = functionName + "_Disassm.html";
            using (StreamWriter writer = new StreamWriter(fileName))
            {
                uint word;
                int lineNo = 0;
                VirtualMachineInstruction instruction;

                writer.Write("<!DOCTYPE HTML PUBLIC \"-//W3C//DTD HTML 4.0 Transitional//EN\" ");
                writer.WriteLine("\"http://www.w3.org/TR/REC-html40/loose.dtd\">");
                writer.WriteLine("<html>");
                writer.WriteLine("  <head>");
                writer.WriteLine(styleElement);
                writer.WriteLine("    <title>" + fileName + "</title>");
                writer.WriteLine("  </head>");
                writer.WriteLine("  <body>");
                writer.WriteLine("    <table class=\"disassm\">");
                writer.WriteLine(tableHeader);
                while (lineNo < byteCode.Count)
                {
                    word = byteCode[lineNo++];
                    instruction = (VirtualMachineInstruction)(word >> 24);
                    string instr = instruction.ToString();
                    switch (instruction)
                    {
                        case VirtualMachineInstruction.ICONST: // Short int constant
                            {
                                short value = (short)(word & 0x0000ffff);
                                GenerateTableRow(writer, lineNo - 1, instr, "#" + value, word);
                            }
                            break;

                        case VirtualMachineInstruction.DCONST:    // Double constant
                        case VirtualMachineInstruction.FCONST:    // Float constant
                        case VirtualMachineInstruction.LCONST:    // Long int constant
                        case VirtualMachineInstruction.WCONST:    // Int constant
                        case VirtualMachineInstruction.SCONST:    // String constant
                        case VirtualMachineInstruction.WSCONST:   // WString constant
                        case VirtualMachineInstruction.L_SWITCH:  // CASE statement jump table
                        case VirtualMachineInstruction.T_SWITCH:  // CASE statement binary search table
                        case VirtualMachineInstruction.M_SWITCH:  // MUX function call jump table
                            {
                                uint address = word & 0x00ffffff;
                                GenerateTableRow(writer, lineNo - 1, instr, address, word);
                            }
                            break;

                        case VirtualMachineInstruction.CALL:
                            {
                                StandardLibraryFunction stdFunction;

                                stdFunction = (StandardLibraryFunction)(word & 0xff);
                                GenerateTableRow(writer, lineNo - 1, instr, stdFunction, word);
                            }
                            break;

                        default:
                            {
                                int operand = (int)(word & 0x00ffffff);
                                if (operand == 0 && !IsLoadStoreInstruction(instruction))
                                    GenerateTableRow(writer, lineNo - 1, instr, "&nbsp", word);
                                else
                                    GenerateTableRow(writer, lineNo - 1, instr, operand, word);
                            }
                            break;
                    }
                }
                writer.WriteLine("    </table>");
                writer.WriteLine("  </body>");
                writer.WriteLine("</html>");
            }
        }
    }
}
