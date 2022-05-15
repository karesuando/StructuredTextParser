%namespace STLang.Scanner
%visibility public
%scannertype STLangScanner
%using QUT.Gppg;
%using STLang.Parser;
%using STLang.KeyWords;
%using STLang.Extensions;
%using STLang.ErrorManager;
%using STLang.ConstantTokens;
%using STLang.ImplDependentParams;


ALPHA           [a-zåäöA-ZÅÄÖ]
ALPHANUM        {ALPHA}|[0-9]
IDENT           ({ALPHA}|_{ALPHANUM})(_?{ALPHANUM})*
INT             [0-9](_?[0-9])*
REAL            {INT}\.{INT}(([eE]([\+\-]?){INT})?)
BININT          2#[01]((_?[01])*)
OCTINT          8#[0-7]((_?[0-7])*)
HEXINT          16#[0-9a-fA-F]((_?[0-9a-fA-F])*)
DIRECTVAR       %[iqmIQM](\*|[xnbwdlXNBWDL]({INT}((\.{INT})*)))
ESC_CHAR        \$[$'lnprtLNPRT]
ESC_WCHAR       \$[$"lnprtLNPRT]
HEX_CHAR        \$([0-9a-fA-F]+)
ILL_CHAR        \$([^$'lnprtLNPRT0-9a-fA-F])
ILL_WCHAR       \$([^$"lnprtLNPRT0-9a-fA-F])
FIXED_POINT     {INT}((\.{INT})?)
MILLISEC        {FIXED_POINT}[mM][sS]
SECONDS         {FIXED_POINT}[sS]|{INT}[sS](_?){MILLISEC}
MINUTES         {FIXED_POINT}[mM]|{INT}[mM](_?){SECONDS}
HOURS           {FIXED_POINT}[hH]|{INT}[hH](_?){MINUTES}
DAYS            {FIXED_POINT}[dD]|{INT}[dD](_?){HOURS}
INTERVAL        {DAYS}|{HOURS}|{MINUTES}|{SECONDS}|{MILLISEC}
T               ([tT][iI][mM][eE])|[tT]
D               ([dD][aA][tT][eE])|[dD]
TOD             ([tT][iI][mM][eE]_[oO][fF]_[dD][aA][yY])|([tT][oO][dD])
DT              ([dD][aA][tT][eE]_[aA][nN][dD]_[tT][iI][mM][eE])|([dD][tT])
DATE_LIT        {INT}-{INT}-{INT}
DATE            {D}#{DATE_LIT}
TOD_LIT         {INT}:{INT}:{FIXED_POINT}
TIME_OF_DAY     {TOD}#{TOD_LIT}
DATE_TIME       {DT}#{DATE_LIT}-{TOD_LIT}
TIME            {T}#(-?){INTERVAL}
TYPED_INT       {IDENT}#{INT}
TYPED_REAL      {IDENT}#{REAL}
TYPED_BININT    {IDENT}#{BININT}
TYPED_OCTINT    {IDENT}#{OCTINT}
TYPED_HEXINT    {IDENT}#{HEXINT}
TYPED_ENUM      {IDENT}#{IDENT}
WHITESPACE      [ \t\n\r]+

%x ASCIISTR,UNICODESTR,COMMENT,PRAGMA
%%
%{
	// Scanner prolog
	// 
	// This code is always executed just before pattern matching begins.
	//
	if (this.doPushBack)
	{
		TokenData token = this.tokenBuffer[0];
		this.yylval = token.Value;
		this.yylloc = token.Location;
		this.tokenBuffer.RemoveAt(0);
		this.doPushBack = this.tokenBuffer.Count > 0;
		return (int)token.Enum;
	}
%}
{WHITESPACE}   ;
{IDENT}        {
				   Tokens token;

                   // Check if it is a keyword or identifier.

				   if (keyWordTable.IsKeyword(this.yytext, out token))
				   {
					   yylval.Ident = this.yytext;
					   return (int)token;
				   }
				   else {
					   int maxIDLength = STLangParameters.MAX_ID_LENGTH;
					   if (this.yytext.Length <= maxIDLength)
						   yylval.Ident = this.yytext;
					   else {
					       this.report.Warning(7, maxIDLength, this.yytext, this.yylloc);
						   yylval.Ident = this.yytext.Remove(maxIDLength - 1);
					   }
					   return (int)Tokens.IDENT;
				   }
	           }
{INT}          {this.yylval.LInt      = GetInt(this.yytext);            return (int)Tokens.INT_LIT;}
{REAL}         {this.yylval.LReal     = GetReal(this.yytext);          return (int)Tokens.REAL_LIT;}
{HEXINT}       {this.yylval.LInt      = GetHexInt(this.yytext);         return (int)Tokens.INT_LIT;}
{OCTINT}       {this.yylval.LInt      = GetOctInt(this.yytext);         return (int)Tokens.INT_LIT;}
{BININT}       {this.yylval.LInt      = GetBinInt(this.yytext);         return (int)Tokens.INT_LIT;}
{TYPED_INT}    {this.yylval.TypedInt  = GetTypedInt(this.yytext);     return (int)Tokens.TYPED_INT;}
{TYPED_REAL}   {this.yylval.TypedReal = GetTypedReal(this.yytext);   return (int)Tokens.TYPED_REAL;}
{TYPED_BININT} {this.yylval.TypedInt  = GetTypedBinInt(this.yytext);  return (int)Tokens.TYPED_INT;}
{TYPED_OCTINT} {this.yylval.TypedInt  = GetTypedOctInt(this.yytext);  return (int)Tokens.TYPED_INT;}
{TYPED_HEXINT} {this.yylval.TypedInt  = GetTypedHexInt(this.yytext);  return (int)Tokens.TYPED_INT;}
{TYPED_ENUM}   {this.yylval.TypedEnum = GetTypedEnum(this.yytext);   return (int)Tokens.TYPED_ENUM;}
{DATE}         {this.yylval.Date      = GetDate(this.yytext);          return (int)Tokens.DATE_LIT;}
{TIME}         {this.yylval.Time      = GetTimeSpan(this.yytext);      return (int)Tokens.TIME_LIT;}
{DATE_TIME}    {this.yylval.DateTime  = GetDateTime(this.yytext);        return (int)Tokens.DT_LIT;}
{TIME_OF_DAY}  {this.yylval.TimeOfDay = GetTimeOfDay(this.yytext);      return (int)Tokens.TOD_LIT;}
{DIRECTVAR}    {this.yylval.DirectVar = GetDirectVar(this.yytext);   return (int)Tokens.DIRECT_VAR;}
"&"            {return (int)Tokens.IOR;                                                            }
">="           {return (int)Tokens.GEQ;                                                            }
"<="           {return (int)Tokens.LEQ;                                                            }
"<>"           {return (int)Tokens.NEQ;                                                            }
"**"           {return (int)Tokens.POW;                                                            }
":="           {return (int)Tokens.ASSIGN;                                                         }
"=>"           {return (int)Tokens.OUTPUT_ASSIGN;                                                  }
".."           {return (int)Tokens.DOTDOT;                                                         }
\{             {BEGIN(PRAGMA);                                                                     }
"(*"           {BEGIN(COMMENT); this.commentLen = 0; this.flag = false;                            }
'              {BEGIN(ASCIISTR);  this.String = ""; this.flag = false;                             } // AscII string
\"             {BEGIN(UNICODESTR); this.String = "";this.flag = false;                             } // Unicode string
<COMMENT>{
	\*+\)   {
	            this.commentLen += this.yytext.Length - 2; 
	            this.CheckCommentLength();
	            BEGIN(INITIAL);
	        }
    [^*]+   {
			    this.commentLen += this.yytext.Length;
				this.CheckCommentLength();
			}
	\*+     {
			    this.commentLen += this.yytext.Length;
				this.CheckCommentLength();
			}
	<<EOF>> {this.report.SyntaxError(37, this.yylloc); return 0;}
}
<PRAGMA>{
    \}          BEGIN(INITIAL);
    [^}]+       ;
    <<EOF>>    {
			       this.report.SyntaxError(38, this.yylloc);
				   return 0;
			   }
}
<ASCIISTR>{
	[^$'\n]+   {
	               this.String += this.yytext; 
				   this.CheckStringLength(this.String);                   
			   }
    <<EOF>>    {
	                this.report.SyntaxError(38, this.yylloc);
					return (int)Tokens.STRING_LIT;
			   }
    {ILL_CHAR} {this.report.SemanticError(40, yytext, this.yylloc);} 
	{ESC_CHAR} {
				   this.String += this.GetEscChar(this.yytext); 
				   this.CheckStringLength(this.String);  
			   }
    {HEX_CHAR} {
	               this.String += this.GetHexEscChar(this.yytext); 
				   this.CheckStringLength(this.String);}
	'          {
			       this.yylval.String = this.String;
				   BEGIN(INITIAL);
				   return (int)Tokens.STRING_LIT;
               }
    \n         {
	               this.report.SyntaxError(41, this.yylloc);
				   this.yylval.String = this.String;
                   BEGIN(INITIAL);
				   return (int)Tokens.STRING_LIT;
               }
}
<UNICODESTR>{
	[^$\"\n]+   {
	                this.String += yytext; 
					this.CheckWStringLength(this.String);
			    }
    <<EOF>>     {
	                this.report.SyntaxError(38, this.yylloc);
					return (int)Tokens.WSTRING_LIT;
				}
    {ILL_WCHAR} {this.report.SemanticError(40, yytext, this.yylloc);} 
	{ESC_WCHAR} {
	                this.String += this.GetEscChar(yytext); 
					this.CheckWStringLength(this.String);   
				}
    {HEX_CHAR}  {
	                this.String += this.GetHexEscWChar(yytext); 
					this.CheckWStringLength(this.String);
				}
	\"          {
			        this.yylval.String = this.String;
				    BEGIN(INITIAL);
					return (int)Tokens.WSTRING_LIT;
                }
    \n          {
	                this.report.SyntaxError(41, this.yylloc);
				    this.yylval.String = this.String;
                    BEGIN(INITIAL);
					return (int)Tokens.WSTRING_LIT;
                }
}

.  return yytext[0];

// Scanner epilog
// 
// This code is always executed just before the scanner returns.
// Save the location of each token in yyloc. 
 this.yylloc = new LexLocation(this.tokLin, this.tokCol, this.tokELin, this.tokECol);
%%

public STLangScanner(Stream stream, ErrorHandler error)
{
	this.SetSource(stream);
	this.report = error;
	this.doPushBack = false;
	this.tokenBuffer = new List<TokenData>();
}

public STLangScanner(string aString, ErrorHandler error)
{
	this.SetSource(aString, 0);
	this.report = error;
	this.doPushBack = false;
	this.tokenBuffer = new List<TokenData>();
}

static STLangScanner()
{
	keyWordTable = new STLangKeywordDictionary();
	dummyToken = new TokenData();
	dummyToken.Enum = Tokens.WHEN;
	dummyToken.Location = new LexLocation(-1, -1, -1, -1);
}

private bool flag;

private int commentLen;

private string String;

private bool doPushBack;

private readonly ErrorHandler report;

private readonly List<TokenData> tokenBuffer;

private static readonly TokenData dummyToken;

private static readonly STLangKeywordDictionary keyWordTable;

private struct TokenData
{
	public Tokens Enum { get; set; }

	public STLang.Parser.ValueType Value { get; set; }

	public LexLocation Location { get; set; }
}

private ulong ToInt(string text)
{
	ulong intValue = 0;
	ulong digitValue;
	foreach (char ch in text)
	{
		digitValue = (ulong)(ch - '0');
		if (intValue > ((ulong.MaxValue - digitValue)/10))
		{
			// Error: Integer overflow
		    this.report.SemanticError(100, text, this.yylloc);
			break;
		}
		intValue = 10*intValue + digitValue;
	}
	return intValue;
}

private double ToReal(string text)
{
	double value = 0.0d;
    bool result = double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value); 
	if (! result)
		this.report.SemanticError(101, text, this.yylloc);
	return value;
}

private ulong HexToInt(string text)
{
	ulong digitValue;
	ulong intValue = 0;
	string hexDigits = "0123456789abcdef";
	foreach (char ch in text)
	{
		digitValue = (ulong)hexDigits.IndexOf(char.ToLower(ch));
		if (intValue > ((ulong.MaxValue - digitValue) >> 4))
		{
			// Error: Integer overflow
			this.report.SemanticError(100, text, this.yylloc);
			break;
		}
		intValue = (intValue << 4)|digitValue;
	}
	return intValue;
}

private ulong OctalToInt(string text)
{
	ulong digitValue;
	ulong intValue = 0;
	foreach (char ch in text)
	{
		digitValue = (ulong)(ch - '0');
		if (intValue > ((ulong.MaxValue - digitValue) >> 3))
		{
			// Error: Integer overflow
		    this.report.SemanticError(100, text, this.yylloc);
			break;
		}
		intValue = (intValue << 3)|digitValue;
	}
	return intValue;
}

private ulong BinaryToInt(string text)
{
	ulong digitValue;
	ulong intValue = 0;
	foreach (char ch in text)
	{
		digitValue = (ulong)(ch == '1' ? 1 : 0);
		if (intValue > ((ulong.MaxValue - digitValue) >> 1))
		{
			// Error: Integer overflow
		    this.report.SemanticError(100, text, this.yylloc);
			break;
		}
		intValue = (intValue << 1)|digitValue;
	}
	return intValue;
}

private bool ValidDate(int year, int month, int day)
{
	bool result = true;

	if (year < 1 || year > 9999)
	{
		result = false;
	}
	if (month < 1 || month > 12)
	{
		result = false;
	}
	if (day < 1 || day > DateTime.DaysInMonth(year, month))
	{
		result = false;
	}
	return result;
}

private bool ValidTimeOfDay(int hour, int minute, int second, int millisec)
{
	return ! (hour > 23 || minute > 59 || second > 59 || millisec > 999);
}

private Tokens LookupTokenType(string text, out string typeName, out string value)
{
	Tokens token;
	char[] separator = new char[]{'#'};
	string[] component = text.Split(separator);
	
	typeName = component[0];
	value = component[1].RemoveChar('_');
	if (keyWordTable.IsKeyword(component[0], out token))
		return token;
	else {
		if (component[0].Length > STLangParameters.MAX_ID_LENGTH)
			this.report.Warning(7, component[0], this.yylloc);
		return Tokens.IDENT;
	}
}

private TokenInt GetInt(string text)
{
	string strValue = text;
	text = text.RemoveChar('_');
	ulong intValue = this.ToInt(text);
	return new TokenInt(intValue, strValue);
}

private TokenInt GetHexInt(string text)
{
	string strValue = text;
	text = text.RemoveChar('_');
	ulong intValue = this.HexToInt(text);
	return new TokenInt(intValue, strValue);
}

private TokenInt GetOctInt(string text)
{
	string strValue = text;
	text = text.RemoveChar('_');
	ulong intValue = this.OctalToInt(text);
	return new TokenInt(intValue, strValue);
}

private TokenInt GetBinInt(string text)
{
	string strValue = text;
	text = text.RemoveChar('_');
	ulong intValue = this.BinaryToInt(text);
	return new TokenInt(intValue, strValue);
}

private TokenDouble GetReal(string text)
{
	string strValue = text;
	text = text.RemoveChar('_');
//	text = text.Replace('.', ',');
	double realValue = this.ToReal(text);
	return new TokenDouble(realValue, strValue);
}

private TokenTypedInt GetTypedInt(string text)
{
	string strValue,typeName;
	Tokens token = this.LookupTokenType(text, out typeName, out strValue);
	ulong value = this.ToInt(strValue);
	return new TokenTypedInt(token, value, typeName, text);
}

private TokenTypedInt GetTypedBinInt(string text)
{
	string strValue,typeName;
	Tokens token = this.LookupTokenType(text, out typeName, out strValue);
	ulong value = this.BinaryToInt(strValue);
	return new TokenTypedInt(token, value, typeName, text);
}

private TokenTypedInt GetTypedOctInt(string text)
{
	string strValue,typeName;
	Tokens token = this.LookupTokenType(text, out typeName, out strValue);
	ulong value = this.OctalToInt(strValue);
	return new TokenTypedInt(token, value, typeName, text);
}

private TokenTypedInt GetTypedHexInt(string text)
{
	string strValue,typeName;
	Tokens token = this.LookupTokenType(text, out typeName, out strValue);
	ulong value = this.HexToInt(strValue);
	return new TokenTypedInt(token, value, typeName, text);
}

private TokenTypedReal GetTypedReal(string text)
{
    string strValue,typeName;
	Tokens token = this.LookupTokenType(text, out typeName, out strValue);
	double value = this.ToReal(strValue);
	return new TokenTypedReal(token, value, typeName, text);
}

private TokenTypedEnum GetTypedEnum(string text)
{
	Tokens token1, token2;
 	char[] separator = new char[]{'#'};
	string[] component = text.Split(separator);
	
	string enumType = component[0];
	string enumConst = component[1];
	if (keyWordTable.IsKeyword(enumType, out token1))
		this.report.SyntaxError(111, enumType, this.yylloc);
	else {
		if (enumType.Length > STLangParameters.MAX_ID_LENGTH)
			this.report.Warning(7, enumType, this.yylloc);
		token1 = Tokens.IDENT;
	}
	if (keyWordTable.IsKeyword(enumConst, out token2))
		this.report.SyntaxError(111, enumConst, this.yylloc);
	else {
		if (enumConst.Length > STLangParameters.MAX_ID_LENGTH)
			this.report.Warning(7, enumConst, this.yylloc);
		token2 = Tokens.IDENT;
	}
	TokenTypedEnum typedEnum = new TokenTypedEnum(text);
	typedEnum.TypeName = enumType;
	typedEnum.Value = enumConst;
	typedEnum.TypeToken = token1;
	typedEnum.ValueToken = token2;
	return typedEnum;
}

private TokenDate GetDate(string text)
{
	string strValue = text;
	text = text.RemoveChar('_');
	int count = text.IndexOf('#')  + 1;
	text = text.Remove(0, count);
    char[] separator = new char[]{'-'};
    string[] components = text.Split(separator);
	int year = (int)this.ToInt(components[0]);
	int month = (int)this.ToInt(components[1]);
	int day = (int)this.ToInt(components[2]);
	DateTime dt;
	if (ValidDate(year, month, day))
		dt = new DateTime(year, month, day);
	else {
		this.report.SemanticError(103, strValue, this.yylloc);
		dt = DateTime.MinValue;
	}
	return new TokenDate(dt, strValue);
}

private TokenTOD GetTimeOfDay(string text)
{
	string strValue = text;
	text = text.RemoveChar('_');
	int count = text.IndexOf('#')  + 1;
	text = text.Remove(0, count);
    char[] separator = new char[]{':'};
    string[] components = text.Split(separator);
	int hour = (int)this.ToInt(components[0]);
	int minute = (int)this.ToInt(components[1]);
	int second,millisec;
	if (! components[2].Contains("."))
	{
		millisec = 0;
		second = (int)this.ToInt(components[2]);
	}
	else {
		separator[0] = '.';
		string[] fixedPoint = components[2].Split(separator);
		second = (int)this.ToInt(fixedPoint[0]);
		millisec = (int)this.ToInt(fixedPoint[1]);
	}
	if (this.ValidTimeOfDay(hour, minute, second, millisec))
		return new TokenTOD(hour, minute, second, millisec, strValue);
	else {
		this.report.SemanticError(105, strValue, this.yylloc);
		return new TokenTOD(0, 0, 0, 0, strValue);
	}
}

private TokenDateTime GetDateTime(string text)
{
	string strValue = text;
	text = text.RemoveChar('_');
	int count = text.IndexOf('#')  + 1;
	text = text.Remove(0, count);
	char[] separator = new char[]{'-'};
    string[] components = text.Split(separator);
	int year = (int)this.ToInt(components[0]);
	int month = (int)this.ToInt(components[1]);
	int day = (int)this.ToInt(components[2]);
	if (! this.ValidDate(year, month, day))
	{
		this.report.SemanticError(103, strValue, this.yylloc);
		return new TokenDateTime(DateTime.MinValue, text);
	}
	else {
		text = components[3];
		separator[0] = ':';
		components = text.Split(separator);
		int hour = (int)this.ToInt(components[0]);
		int minute = (int)this.ToInt(components[1]);
		int second,millisec;
		if (! components[2].Contains("."))
		{
			millisec = 0;
			second = (int)this.ToInt(components[2]);
		}
		else {
			separator[0] = '.';
			string[] fixedPoint = components[2].Split(separator);
			second = (int)this.ToInt(fixedPoint[0]);
			millisec = (int)this.ToInt(fixedPoint[1]);
		}
		DateTime dt;
		if (this.ValidTimeOfDay(hour, minute, second, millisec))
			dt = new DateTime(year, month, day, hour, minute, second, millisec);
		else {
			this.report.SemanticError(105, strValue, this.yylloc);
			dt = new DateTime(year, month, day, 0, 0, 0);
		}
		return new TokenDateTime(dt, strValue);
	}
}

private TokenDirectVar GetDirectVar(string text)
{
	char size = text[1];
	char location = text[2];
	if (location == '*')
		return new TokenDirectVar(size, location, text);
	else {
		string strValue = text;
		text = text.Remove(0,3);
		text = text.RemoveChar('_');
		char[] separator = new char[]{'.'};
		string[] address = text.Split(separator);
		int levels = address.Length;
		ushort[] addr = new ushort[levels];
		if (levels > STLangParameters.MAX_HIERARCHICAL_LEVELS)
			this.report.SemanticError(106, strValue, this.yylloc);
		for (int i = 0; i < levels; i++)
		{
			ulong tmp = this.ToInt(address[i]);
			if (tmp > 0xffff)
				this.report.SemanticError(107, tmp, this.yylloc);
			addr[i] = (ushort)tmp;
		}
		return new TokenDirectVar(size, location, addr, strValue);
	}
}

private void Normalize(ref int Day, ref int Hr, ref int Min, ref int Sec, ref int mSec)
{
	int Num = mSec;
	if (Num > 999)
	{
	    mSec  = Num % 1000;
		Sec  += Num / 1000;
	}
	Num = Sec;
	if (Num > 59)
	{
	    Sec  = Num % 60;
		Min += Num / 60;
	}
	Num = Min;
	if (Num > 59)
	{
	    Min = Num % 60;
		Hr += Num / 60;
	}
	Num = Hr;
	if (Num > 23)
	{
	    Hr  = Num % 24;
		Day += Num / 24;
	}
}

private TokenTime GetTimeSpan(string text)
{
	int i = 0;
	string unit = "";
	ulong value = 0;
	string strValue = text;
	ulong[] number = new ulong[]{0, 0, 0, 0, 0, 0};
	bool[] error = new bool[]{false, false, false, false, false, false};
	text = text.ToLower();
	text = text.RemoveChar('_');
	text = text.Replace("ms", "x");
	text = text.Remove(0, text.IndexOf('#') + 1);
	bool isNegative = text[0] == '-';
	if (isNegative)
        text = text.Remove(0, 1);
	foreach (char ch in text)
	{
		if (char.IsDigit(ch))
		{
			ulong digit = (ulong)(ch - '0');
			if (value <= (ulong.MaxValue - digit)/10)
				value = 10*value + digit;
			else 
			{
				if (! error[i])
				{
					error[i] = true;
					this.report.SemanticError(100, strValue, this.yylloc);
				}
				value = 0;
			}
		}
		else {
			if (ch != '.')
				unit += ch;
			number[i++] = value;
			value = 0;
		}
	}
	int j = "dhmsx".IndexOf(unit[0]);
	if (j > 0)
	{
		for (int k = i - 1; k >= 0; k--)
		{
		    number[k + j] = number[k];
			number[k]     = 0;
		}
	}
	int days     = (int)number[0];
	int hours    = (int)number[1];
	int minutes  = (int)number[2];
	int seconds  = (int)number[3];
	int millisec = (int)number[4];
	this.Normalize(ref days, ref hours, ref minutes, ref seconds, ref millisec);
	TimeSpan interval;
	try 
	{
		interval = new TimeSpan(days, hours, minutes, seconds, millisec);
		if (isNegative)
			interval = -interval;
	}
	catch (ArgumentOutOfRangeException)
	{
		// Error: Time interval out of range
		this.report.SemanticError(102, strValue, this.yylloc);
		interval = new TimeSpan(0, 0, 0, 0, 0);
	}
	return new TokenTime(interval, strValue);
}

private char GetEscChar(string escChar)
{
    char ch = escChar[1];
	if (ch == 'l' || ch == 'L')
	    return Convert.ToChar(0x0a);
	else if (ch == 'n' || ch == 'N')
	    return '\n';
	else if (ch == 'p' || ch == 'P')
	    return '\f';
	else if (ch == 't' || ch == 'T')
	    return '\t';
	else if (ch == 'r' || ch == 'R')
	    return '\r';
	else
	    return ch;
}

private char GetHexEscChar(string hexChar)
{
	if (hexChar.Length != 3)
		this.report.SemanticError(96, hexChar, this.yylloc);
	hexChar = hexChar.Remove(0,1);
	int value = (int)this.HexToInt(hexChar);
	return Convert.ToChar(value);
}

private char GetHexEscWChar(string hexChar)
{
    if (hexChar.Length != 5)
		this.report.SemanticError(97, hexChar, this.yylloc);
	hexChar = hexChar.Remove(0,1);
	int value = (int)this.HexToInt(hexChar);
	return Convert.ToChar(value);
}

private void CheckStringLength(string str)
{
	if (str.Length > STLangParameters.MAX_STRING_LENGTH && ! flag)
	{
		this.report.SemanticError(-6, this.yylloc);
		this.flag = true;
	}
}

private void CheckWStringLength(string str)
{
	if (str.Length > STLangParameters.MAX_WSTRING_LENGTH && ! flag)
	{
		this.report.SemanticError(-6, this.yylloc);
		this.flag = true;
	}
}

private void CheckCommentLength()
{
	if (this.commentLen > STLangParameters.MAX_COMMENT_LENGTH && ! flag)
	{
		this.report.SemanticError(98, this.yylloc);
		this.flag = true;
	}
}

public STLang.Parser.ValueType GetBufferedValue(int index)
{
	if (index < 0 || index >= this.tokenBuffer.Count)
		throw new STLangCompilerError("STLangScanner.GetBufferedValue index out of range.");
	else 
		return this.tokenBuffer[index].Value;
}

public Tokens GetNextToken()
{
	TokenData token = new TokenData();
	token.Enum = (Tokens)this.yylex();
	token.Value = this.yylval;
	token.Location = this.yylloc;
	this.tokenBuffer.Add(token);
	return token.Enum;
}

public void DoPushBack(bool doPushDummyToken)
{
	if (doPushDummyToken)
		this.tokenBuffer.Insert(0, dummyToken);
	this.doPushBack = true;
}

public override void yyerror(string msg, params object[] args)
{
	this.report.SyntaxError(msg, this.yylloc);
}