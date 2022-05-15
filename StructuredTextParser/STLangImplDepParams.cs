using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STLang.ImplDependentParams
{
    // Implementation-dependent parameters
    public class STLangParameters
    {
        public const int MAX_ID_LENGTH = 64;

        public const int MAX_COMMENT_LENGTH = 16384;

        public const int MAX_ENUMERATED_VALUES = ushort.MaxValue;

        public const int MAX_ARRAY_SUBSCRIPTS = 10;

        public const int MAX_ARRAY_SUBSCRIPT_RANGE = 100000;

        public const int MAX_EXPRESSION_LENGTH = 2000;

        public const int MAX_ARRAY_SIZE = 1048575; // 2^20 - 1

        public const int MAX_NO_STRUCT_ELEMENTS = 128;

        public const int MAX_STRUCTURE_SIZE = 100000;

        public const int MAX_STRUCT_NESTING_DEPTH = 32;

        public const int MAX_HIERARCHICAL_LEVELS = 5;

        public const int MAX_DEFAULT_STRING_LENGTH = 256;

        public const int MAX_DEFAULT_WSTRING_LENGTH = 256;

        public const int MAX_STRING_LENGTH = 4096;

        public const int MAX_WSTRING_LENGTH = 4096;

        public const int MAX_CASE_SELECTIONS = ushort.MaxValue;

        public const int MAX_VARS_PER_DECLARATION = 512;

        public const int MAX_EXTENSIBLE_FUNCTION_INPUTS = 512;

        public const int MAX_EXECUTABLE_CODE_SIZE = 2000000;
    }
}
