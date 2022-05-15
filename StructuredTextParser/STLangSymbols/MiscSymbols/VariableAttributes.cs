namespace STLang.Symbols
{
    public enum STVarType
    {
        VAR          = 0x001,
        VAR_INPUT    = 0x002,
        VAR_OUTPUT   = 0x004,
        VAR_INOUT    = 0x008,
        VAR_GLOBAL   = 0x010,
        VAR_ACCESS   = 0x020,
        VAR_TEMP     = 0x040,
        VAR_CONFIG   = 0x080,
        VAR_EXTERNAL = 0x100
    };

    public enum STVarQualifier
    {
        NONE,
        CONSTANT,
        RETAIN,
        NON_RETAIN,
        CONSTANT_RETAIN
    };

    public enum STDeclQualifier
    {
        NONE,
        R_EDGE,
        F_EDGE,
        READ_ONLY,
        WRITE_ONLY
    }
}