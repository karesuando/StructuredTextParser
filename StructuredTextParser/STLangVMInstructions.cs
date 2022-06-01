using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace STLang.VMInstructions
{
    //
    // Structured Text virtual machine instruction set
    //
    public enum VirtualMachineInstruction : byte
    {
       NOOP,     // NO OPeration
        IADD,     // Integer ADD
        LADD,     // Long integer ADD
        FADD,     // Float ADD
        DADD,     // Double ADD
        ISUB,     // Int SUBtract
        LSUB,     // Lint SUBtract
        FSUB,     // Float SUBtract
        DSUB,     // Double SUBtract
        IMUL,     // Int MULtiply
        LMUL,     // Lint MULtiply
        FMUL,     // Float MULtiply
        DMUL,     // Double MULtiply
        IDIV,     // Int DIVide
        LDIV,     // Lint DIVide
        FDIV,     // Float DIVide
        DDIV,     // Double DIVide
        CALL,     // CALL standard library function
        IMOD,     // Int MODulus
        LMOD,     // LInt MODulus
        INEG,     // Int unary minus
        LNEG,     // Long unary minus
        FNEG,     // Float unary minus
        DNEG,     // Double unary minus
        ISQR,
        FSQR,
        DSQR,
        LSQR,
        IJGT,     // Int Jump if Greater Than
        LJGT,     // Long Jump Greater Than
        FJGT,     // Float Jump if Greater Than
        DJGT,     // Double Jump if Greater Than
        IJLT,     // Int Jump if Less Than
        LJLT,     // Long Jump Less Than
        FJLT,     // Float Jump if Less Than
        DJLT,     // Double Jump if Less Than
        IJLE,     // Int Jump if Less Equal
        LJLE,     // Long Jump Less Equal Than
        FJLE,     // Float Jump if Less Equal
        DJLE,     // Double Jump if Less Equal
        IJGE,     // Int Jump if Greater Equal
        LJGE,     // Long Jump Greater Equal Than
        FJGE,     // Float Jump if Greater Equal
        DJGE,     // Double Jump if Greater Equal
        IJNE,     // Int Jump if Not Equal
        LJNE,     // Long Jump Not Equal 
        FJNE,     // Float Jump if Not Equal
        DJNE,     // Double Jump if Not Equal
        IJEQ,     // Int Jump if EQual
        LJEQ,     // Long Jump EQual
        FJEQ,     // Float Jump if EQual
        DJEQ,     // Double Jump if EQual
        JUMP,     // Unconditional JUMP
        L_SWITCH, // Case statement implemented as a jump table
        T_SWITCH, // Case statement implemented as a binary search table
        M_SWITCH, // MUX function call jump table
        IJEQZ,    // Int Jump if EQual to Zero
        FJEQZ,    // Float Jump if EQual to Zero
        DJEQZ,    // Double Jump if EQual to Zero
        IJNEZ,    // Int Jump if Not Equal to Zero
        FJNEZ,    // Float Jump if Not Equal to Zero
        DJNEZ,    // Double Jump if Not Equal to Zero
        IJGTZ,    // Int Jump if Greater Than Zero
        FJGTZ,    // Float Jump if Greater Than Zero
        DJGTZ,    // Double Jump if Greater Than Zero
        IJLTZ,    // Int Jump if Less Than than Zero
        FJLTZ,    // Float Jump if Less Than Zero
        DJLTZ,    // Double Jump if Less Than Zero
        IJGEZ,    // Int Jump if Greater Equal than Zero
        FJGEZ,    // Float Jump if Greater Equal than Zero
        DJGEZ,    // Double Jump if Greater Equal than Zero
        IJLEZ,    // Int Jump if Less Equal Zero
        FJLEZ,    // Float Jump if Less Equal Zero
        DJLEZ,    // Double Jump if Less Equal Zero
        IBAND,    // Integer Bitwise AND
        LBAND,    // Long integer Bitwise AND
        IBIOR,    // Integer Bitwise Inclusive OR
        LBIOR,    // Long integer Bitwise Inclusive OR
        IBXOR,    // Integer Bitwise eXclusive OR
        LBXOR,    // Long integer Bitwise eXclusive OR
        IBNOT,    // Integer Bitwise NOT
        LBNOT,    // Long integer Bitwise NOT
        IINCR,    // Increment result by 1 
        IINCR0,   // Int INCRement register variable 0
        IINCR1,   // Int INCRement register variable 1
        IINCR2,   // Int INCRement register variable 2
        IINCR3,   // Int INCRement register variable 3
        WINCR0,   // DInt INCRement register variable 0
        WINCR1,   // DInt INCRement register variable 1
        WINCR2,   // DInt INCRement register variable 2
        WINCR3,   // DInt INCRement register variable 3
        IDECR,    // Int DECRement result by 1 
        IDECR0,   // Int DECRement register variable 0
        IDECR1,   // Int DECRement register variable 1
        IDECR2,   // Int Decrement register variable 2
        IDECR3,   // Int Decrement register variable 3
        WDECR0,   // DInt DECRement register variable 0
        WDECR1,   // DInt DECRement register variable 1
        WDECR2,   // DInt DECRement register variable 2
        WDECR3,   // DInt DECRement register variable 3
        IBSHL,    // Integer Bitwise SHift Left
        LBSHL,    // Long Bitwise SHift Left
        IBSHR,    // Integer Bitwise SHift Right
        LBSHR,    // Integer Bitwise SHift Right
        IBSHL_1,  // Bitwise left shift 1 bit
        IBSHR_1,  // Bitwise right shift 1 bit
        LBSHL_1,  // Bitwise left shift 1 bit
        LBSHR_1,  // Bitwise right shift 1 bit
        BROR,     // Byte ROtate Right
        WROR,     // Word ROtate Right
        DROR,     // DWord ROtate Right
        LROR,     // LWord ROtate Right
        BROL,     // Byte ROtate Left
        WROL,     // Word ROtate Left
        DROL,     // DWord ROtate Left
        LROL,     // LWord ROtate Left
        BLOD0,    // Byte LOaD from register variable 0
        ILOD0,    // Int LOaD from register variable 0
        WLOD0,    // Dint LOaD from register variable 0
        LLOD0,    // Lint LOaD from register variable 0
        FLOD0,    // Float LOaD from register variable 0
        DLOD0,    // Double LOaD from register variable 0
        BLOD1,    // Byte LOaD from register variable 1
        ILOD1,    // Int LOaD from register variable 1
        WLOD1,    // Dint LOaD from register variable 1
        LLOD1,    // Lint LOaD from register variable 1
        FLOD1,    // Float LOaD from register variable 1
        DLOD1,    // Double LOaD from register variable 1
        BLOD2,    // Byte LOaD from register variable 2
        ILOD2,    // Int LOaD from register variable 2
        WLOD2,    // Dint LOaD from register variable 2
        LLOD2,    // Lint LOaD from register variable 2
        FLOD2,    // Float LOaD from register variable 2
        DLOD2,    // Double LOaD from register variable 2
        BLOD3,    // Byte LOaD from register variable 3
        ILOD3,    // Int LOaD from register variable 3
        WLOD3,    // Dint LOaD from register variable 3
        LLOD3,    // Lint LOaD from register variable 3
        FLOD3,    // Float LOaD from register variable 3
        DLOD3,    // Double LOaD from register variable 3
        BSTO0,    // Byte STOre to register variable 0
        ISTO0,    // Int STOre to register variable 0
        WSTO0,    // Int STOre reto register variable 0
        LSTO0,    // Long STOre to register variable 0
        FSTO0,    // Float STOre to register variable 0
        DSTO0,    // Double STOre to register variable 0
        BSTO1,    // Byte STOre to register variable 1
        ISTO1,    // Short STOre to register variable 1
        WSTO1,    // Int STOre to register variable 1
        LSTO1,    // Long STOre to register variable 1
        FSTO1,    // Float STOre to register variable 1
        DSTO1,    // Double STOre to register variable 1
        BSTO2,    // Byte STOre to register variable 2
        ISTO2,    // Short STOre to register variable 2
        WSTO2,    // Int STOre to register variable 2
        LSTO2,    // Long STOre to register variable 2
        FSTO2,    // Float STOre to register variable 2
        DSTO2,    // Double STOre to register variable 2
        BSTO3,    // Byte STOre to register variable 3
        ISTO3,    // Short STOre to register variable 3
        WSTO3,    // Int STOre to register variable 3
        LSTO3,    // Long STOre to register variable 3
        FSTO3,    // Float STOre to register variable 3
        DSTO3,    // Double STOre to register variable 3
        BLOD,     // Byte LOaD
        ILOD,
        WLOD,
        LLOD,
        FLOD,
        DLOD,
        SLOD, // (ascii) String LOaD
        WSLOD, // Unicode string LOaD
        BSTO,  
        ISTO,
        WSTO,
        LSTO,   // Long Store
        FSTO,   // Float store
        DSTO,   // Double store
        SSTO,   // String STOre
        WSSTO,  // Unicode string STOre
        BLODX,  // Sbyte LOaD indeXed
        ILODX,  // Int LOaD indeXed
        WLODX,  // Dint LOaD indeXed
        LLODX,  // Lint  LOaD indeXed
        FLODX,  // Float LOaD indeXed
        DLODX,  // Double LOaD indeXed
        SLODX,  // String LOaD indeXed
        WSLODX, // Unicode LOaD indeXed
        BSTOX,  // Byte STOre indeXed
        ISTOX,  // Int STOre indeXed
        WSTOX,  // Dint Store indeXed
        LSTOX,  // Lint STOre indeXed
        FSTOX,  // Float STOre indeXed
        DSTOX,  // Double STOre indeXed
        SSTOX,  // String STOre indeXed
        WSSTOX,  // Wide String (Unicode) STOre indeXed
        RETN,    // RETurN from subroutine
        IDUPL,        // Int DUPLicate (Push a copy of top value)
        LDUPL,        // Long DUPLicate 
        FDUPL,        // Float DUPLicate 
        DDUPL,        // Double DUPLicate 
        FINV,         // Float INVert
        DINV,         // Double INVert
        WCONST,       // DINT CONSTant
        LCONST,       // Long CONSTant
        FCONST,       // Float CONSTant
        DCONST,       // Double CONSTant
        SCONST,       // String CONSTant
        WSCONST,      // Wide String constant
        ICONST,       // SINT,(U)INT,BYTE constant
        ICONST_N1,    // SINT,INT,DINT # -1
        ICONST_0,     // SINT,INT,DINT #  0
        ICONST_1,     // SINT,INT,DINT #  1
        LCONST_N1,    // LINT  # -1
        LCONST_0,     // LINT  # 0
        LCONST_1,     // LINT  # 1
        FCONST_N1,    // REAL  # -1.0
        FCONST_0,     // REAL  # 0.0
        FCONST_1,     // REAL  # 1.0
        DCONST_N1,    // LREAL # -1.0
        DCONST_0,     // LREAL # 0.0
        DCONST_1,     // LREAL # 1.0
        DCONST0,      // Double register constant 0
        DCONST1,      // Double register constant 1
        DCONST2,      // Double register constant 2
        DCONST3,      // Double register constant 3
        JSBR,         // Jump SuBRoutine
        RANGE_CHECK,  // Array index check
        MEM_COPY,     // MEMory to memory COPY
        ROM_COPY,     // Read Only Memory to memory COPY
        MEM_SETB,     // MEMory SET to Bytes (+ parameter)
        MEM_SETZ,     // MEMory SET to Zero
        F2I,          // Float to Int
        F2L,          // Float to Long
        F2D,          // Float to Double
        I2F,          // Int to Float
        I2D,          // Int to Double
        I2L,          // Int to Long
        D2I,          // Double to Int
        D2F,          // Double to Float
        D2L,          // Double to Long
        L2I,          // Long to Int
        L2D,          // Long to double
        L2F,          // Long to float
        I2B,          // Integer to unsigned byte
        I2C,          // Integer to signed byte
        I2S,          // Integer to short
        I2US,         // Integer to unsigned short
        I2UI,         // (signed) Integer to unsigned integer
        CONV,
        EXE_ONCE      // EXEcute block of code ONCE
    }
}


public enum StandardLibraryFunction : byte
{
    // Numerical
    NONE,
    DSIN,
    DCOS,
    DTAN,
    DLOGN,
    DLOG10,
    DEXP,
    DEXPT,
    DASIN,
    DACOS,
    DATAN,
    IABS,
    LABS,
    FABS,
    DABS,
    DSQRT,
    FTRUNC,
    DTRUNC,
    FSIN,
    FCOS,
    FTAN,
    FLOGN,
    FLOG10,
    FEXP,
    FEXPT,
    FASIN,
    FACOS,
    FATAN,
    FSQRT,
    IEXP,
    IEXPT,
    LEXPT,
    IMAX,
    LMAX,
    DMAX,
    FMAX,
    SMAX,
    WSMAX,
    TMAX,
    DTMAX,
    DTTMAX,
    TODMAX,
    IMIN,
    LMIN,
    DMIN,
    FMIN,
    SMIN,
    WSMIN,
    TMIN,
    DTMIN,
    DTTMIN,
    TODMIN,

    // Selection
    ILIMIT,
    LLIMIT,
    DLIMIT,
    FLIMIT,
    TLIMIT,
    DTLIMIT,
    DTTLIMIT,
    TODLIMIT,
    SLIMIT,
    WSLIMIT,
    SELECT,
    MUX,

    // DateTime arithmetic operations
    TADD,         // Time Time ADD
    TODTADD,      // TimeOfDay Time ADD
    TODTSUB,      // TimeOfDay Time SUBtract
    DTTADD,       // DateTime Time ADD
    TSUB,         // Time Time SUBtraction
    TODSUB,       // TimeOfDay TimeOfDay SUBtraction
    DTTSUB,       // DateTime Time SUBtraction
    DTSUB,        // Date Date SUB , DateTime DateTime SUB
    TIMUL,        // Time Int MULtiply
    TFMUL,        // Time Float MULtiply
    TDMUL,        // Time Double MULtiply
    TIDIV,        // Time Int DIVide
    TFDIV,        // Time Float DIVide
    TDDIV,        // Time Double DIVide

    // Character string
    SLEN,
    SLEFT,
    SRIGHT,
    SMID,
    SCONCAT,
    SCONCAT2,
    SINSERT,
    SDELETE,
    SREPLACE,
    SFIND,
    WSLEN,
    WSLEFT,
    WSRIGHT,
    WSMID,
    WSCONCAT,
    WSCONCAT2,
    WSINSERT,
    WSDELETE,
    WSREPLACE,
    WSFIND,
    STRCMP,       // STRing CoMPare
    WSTRCMP,      // WSTRing CoMPare

    BCDTO,
    TOBCD
}