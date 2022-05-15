%namespace STLang.Parser
%using System.Linq;
%using System.Collections;
%using STLang.Symbols;
%using STLang.Scanner;
%using STLang.Subranges;
%using STLang.DataTypes;
%using STLang.Properties;
%using STLang.Statements;
%using STLang.Extensions;
%using STLang.Expressions;
%using STLang.SymbolTable;
%using STLang.ErrorManager;
%using STLang.MemoryLayout;
%using STLang.ParserUtility;
%using STLang.VMInstructions;
%using STLang.ConstantTokens;
%using STLang.AttributeStack;
%using STLang.POUDefinitions;
%using STLang.ByteCodegenerator;
%using STLang.ConstantPoolHandler;
%using STLang.ImplDependentParams;
%using STLang.RuntimeWrapper;
%parsertype STLangParser
%output = STLangParser.cs
%partial

%union
{
	public Tokens Token;
	public string Ident;
	public TokenInt LInt;
	public TokenDate Date;
	public TokenTime Time;
	public TokenDouble LReal;
	public TokenTOD TimeOfDay;
	public TokenDateTime DateTime;
	public TokenTypedInt TypedInt;
	public TokenTypedReal TypedReal;
	public TokenTypedEnum TypedEnum;
	public TokenDirectVar DirectVar;
	public SubRange Subrange;
	public string String;
	public object Object;
	public Statement Stat;
	public StatementList StatList;
	public DeclarationStatement Declaration;
	public POUVarDeclarations POUVariableDecls;
	public Expression Expression;
	public List<object> GenericList;
	public VarDeclStatement VarInitDecl;
	public List<VarDeclStatement> POUVarDecl;
	public CaseLabel CaseLabel;
	public List<SubRange> Subranges;
	public List<string> IdentifierList;
	public List<CaseLabel> CaseLabelList;
	public List<CaseElement> CaseElementList;
	public InitializerList InitList;
	public STVarType VarType;
	public STVarQualifier VarQualifier;
	public STDeclQualifier EdgeQualifier;
	public TypeNode DataType; 
	public ForLoopData ForList; 
	public DataTypeSpec TypeSpec;
	public POUParameter POUParameter;
	public StructDeclaration StructDecl;
	public StructMemberDeclaration MemberDecl;
	public ProgramOrganizationUnitCall POU;
	public IndexedVariable IndexedVariable;
}
%token <Ident> WHILE REPEAT UNTIL FOR IF TO DO THEN ELSE CASE END_CASE
			   ELSIF END_FOR END_WHILE END_IF END_REPEAT EXIT BY
			   ASSIGN OUTPUT_ASSIGN PROGRAM END_PROGRAM ARRAY OF STRUCT END_STRUCT 
			   FUNCTION END_FUNCTION FUNCTION_BLOCK END_FUNCTION_BLOCK 
			   TYPE END_TYPE INT SINT DINT LINT USINT UINT UDINT ULINT 
			   REAL LREAL DATE TIME TIME_OF_DAY TOD DATE_AND_TIME DT 
			   STRING WSTRING BOOL BYTE WORD DWORD LWORD DOTDOT ANY ANY_INT
			   ANY_DERIVED ANY_ELEMENTARY ANY_MAGNITUDE ANY_STRING
			   ANY_REAL ANY_BIT ANY_NUM ANY_DATE R_EDGE F_EDGE VAR 
			   VAR_INPUT VAR_OUTPUT VAR_IN_OUT VAR_EXTERNAL AT VAR_GLOBAL 
			   VAR_TEMP VAR_ACCESS VAR_CONFIG END_VAR CONSTANT RETAIN NON_RETAIN 
			   TRUE FALSE RETURN READ_ONLY WRITE_ONLY READ_WRITE DUMMY_TOKEN
			   CONFIGURATION END_CONFIGURATION INITIAL_STEP END_STEP RESOURCE
			   END_RESOURCE WITH TASK TRANSITION END_TRANSITION WHEN

%token <LInt>      INT_LIT
%token <LReal>     REAL_LIT
%token <TypedInt>  TYPED_INT 
%token <TypedReal> TYPED_REAL
%token <TypedEnum> TYPED_ENUM
%token <Date>      DATE_LIT 
%token <DateTime>  DT_LIT
%token <Time>      TIME_LIT
%token <TimeOfDay> TOD_LIT
%token <String>    STRING_LIT WSTRING_LIT
%token <Ident>     IDENT
%token <DirectVar> DIRECT_VAR
%type <Subrange>  subrange
%type <StatList>  statement_list opt_else_stat while_statement_body for_statement_body 
                  default_statement function_body function_block_body program_body case_stat_list
%type <VarInitDecl> var_init_decl
%type <POUVarDecl> var_declaration var_decl_list
%type <GenericList> elsif_stat opt_elsif_stat
%type <CaseElementList> case_element_list case_elem_list
%type <IdentifierList> identifier_list enum_ident_seq
%type <CaseLabelList> case_label_list
%type <InitList> array_init_list array_init_seq struct_init_list struct_init_seq initializer_seq
%type <Stat> statement assignment_stat repeat_statement_body
%type <CaseLabel> case_label
%type <Subranges> subrange_list
%type <StructDecl> struct_member_decls
%type <MemberDecl> struct_member_decl
%type <Object> case_element function_decl
%type <DataType> elementary_type array_type structure_type subrange_type enumerated_type
	  derived_type non_generic_type string_type data_type generic_type
%type <Expression> expression constant condition_DO condition_THEN expression_OF 
	  variable simple_variable symbolic_variable initial_value until_condition 
	  control_variable optional_by_stat initializer
%type <Declaration> var_decl temp_var_decl extern_var_decl global_var_decl
	  access_var_decl config_var_decl input_var_decl output_var_decl inout_var_decl
	  data_type_decl pou_variable_decl
%type <POU> pou_call_sequence prog_org_unit_call
%type <POUParameter> pou_parameter
%type <POUVariableDecls> pou_variable_decls pou_var_decl_list
%type <IndexedVariable> indexed_variable
%type <ForList> for_list_DO
%type <TypeSpec> data_type_spec
%type <VarQualifier> opt_var_qualifier 
%type <EdgeQualifier> opt_decl_qualifier
%type <Ident> enum_identifier reserved_word
%type <Token> assign

%nonassoc DOTDOT ASSIGN OUTPUT_ASSIGN
%left IOR
%left XOR
%left '&' AND
%nonassoc '=' NEQ
%nonassoc '<' '>' GEQ LEQ
%left '+' '-'
%left '*' '/' MOD
%nonassoc UMINUS NOT
%right POW

%%
main            : {this.PushSymbolTable();} pou_declarations {this.PopSymbolTable();}
				;

pou_declarations
                : pou_declaration
				| pou_declarations pou_declaration
				;
				
pou_declaration : function_decl       {this.ReInitializeParser();} 
				| function_block_decl {this.ReInitializeParser();}
				| program_decl        {this.ReInitializeParser();}
				| data_type_decl
				;
				
assign          : ASSIGN        {$$ = Tokens.ASSIGN;} 
				| OUTPUT_ASSIGN {$$ = Tokens.OUTPUT_ASSIGN;} 
				;

subrange        : expression DOTDOT expression {$$ = this.MakeSubrange($1, $3, @1, @3);}
				;

identifier_list : IDENT                                 {$$ = this.MakeIdentifierList($1, @1);}
				| identifier_list ',' IDENT             {$$ = this.AddIdentToList($1, $3, @3);} 
				| identifier_list ',' reserved_word     {$$ = $1; this.report.SyntaxError(192, $3.ToString(), @3);} 
				| identifier_list IDENT 	            {$$ = this.AddIdentToList($1, $2, @2);this.report.SyntaxError(75, @2);}
				;

subrange_list   : subrange                     {$$ = this.MakeSubRangeList($1, @1);}                              
				| subrange_list ',' subrange   {$$ = this.AddSubRange($1, $3, @3); }
				| subrange_list     subrange   {$$ = this.AddSubRange($1, $2, @2);this.report.SyntaxError(75, @2);}
				;

elementary_type : SINT           {$$ = TypeNode.SInt;}
				| INT            {$$ = TypeNode.Int;}
				| DINT           {$$ = TypeNode.DInt;}
				| LINT           {$$ = TypeNode.LInt;}
				| USINT          {$$ = TypeNode.USInt;}
				| UINT           {$$ = TypeNode.UInt;}
				| UDINT          {$$ = TypeNode.UDInt;}
				| ULINT          {$$ = TypeNode.ULInt;}
				| REAL           {$$ = TypeNode.Real;}
				| LREAL          {$$ = TypeNode.LReal;}
				| DATE           {$$ = TypeNode.Date;}
				| TIME_OF_DAY    {$$ = TypeNode.TimeOfDay;}
				| TOD            {$$ = TypeNode.TimeOfDay;} 
				| DATE_AND_TIME  {$$ = TypeNode.DateAndTime;}
				| DT             {$$ = TypeNode.DateAndTime;}
				| TIME           {$$ = TypeNode.Time;}
				| BOOL           {$$ = TypeNode.Bool;}
				| BYTE           {$$ = TypeNode.Byte;}
				| WORD           {$$ = TypeNode.Word;}
				| DWORD          {$$ = TypeNode.DWord;}
				| LWORD          {$$ = TypeNode.LWord;}
				;

generic_type    : ANY            {$$ = TypeNode.Any;}
				| ANY_NUM        {$$ = TypeNode.AnyNum;}
				| ANY_INT        {$$ = TypeNode.AnyInt;}
				| ANY_REAL       {$$ = TypeNode.AnyReal;}
				| ANY_BIT        {$$ = TypeNode.AnyBit;}
				| ANY_DATE       {$$ = TypeNode.AnyDate;}
				| ANY_DERIVED    {$$ = TypeNode.AnyDerived;}
				| ANY_ELEMENTARY {$$ = TypeNode.AnyElementary;}
				| ANY_MAGNITUDE  {$$ = TypeNode.AnyMagnitude;}
				| ANY_STRING     {$$ = TypeNode.AnyString;}
				;

derived_type    : IDENT          {$$ = this.GetDerivedType($1, @1);}
				;

non_generic_type: elementary_type 
				| derived_type
				| string_type
				;

data_type       : elementary_type 
				| array_type      
				| structure_type  
				| enumerated_type   
				| subrange_type   
				| derived_type
				| string_type    
				| generic_type   {$$ = TypeNode.Error; this.report.SemanticError(51, @1);} 
				;

data_type_spec  : data_type opt_decl_qualifier {$$ = this.MakeDataTypeSpec($1, $2, @2);}
				| data_type opt_decl_qualifier ASSIGN {this.Push($1);} initial_value  {$$ = this.MakeDataTypeSpec($1, $2, $5, @2);}
				;

array_type      : ARRAY '[' subrange_list ']' OF non_generic_type  {$$ = this.MakeArrayType($3, $6, @3, @6);}
				| ARRAY '[' error ']' OF non_generic_type {$$ = TypeNode.Error; this.yyerrok();}  
				;

struct_member_decls
                : STRUCT {this.CheckNestingDepth(@1);} struct_member_decl {$$ = this.MakeStructMemberList($3);}
                | struct_member_decls struct_member_decl                  {$$ = this.AddStructMemberDecl($1, $2, @2);}
				;

struct_member_decl   
                : IDENT ':' data_type_spec semicolon  {$$ = this.MakeStructMemberDecl($1, $3, @1);}
				;

structure_type  : struct_member_decls END_STRUCT {$$ = this.MakeStructDataType($1);}
				| STRUCT {this.CheckNestingDepth(@1);} 
				     error 
				  END_STRUCT {$$ = TypeNode.Error; this.yyerrok();}
				| struct_member_decls error END_STRUCT {$$ = this.MakeStructDataType($1); this.yyerrok();}            
				;

enum_identifier : IDENT                              {$$ = $1;}
                | TYPED_ENUM                         {$$ = $1.Value; this.report.SyntaxError(169, $1.ToString(), @1);}
				| reserved_word                      {$$ = ""; this.report.SyntaxError(192, $1.ToString(), @1);}
				;

enum_ident_seq  : '(' enum_identifier                {$$ = this.MakeEnumIdentList($2, @2);}      
                | enum_ident_seq ',' enum_identifier {$$ = this.AddToEnumIdentList($1, $3, @3);}
				| enum_ident_seq enum_identifier     {$$ = this.AddToEnumIdentList($1, $2, @2); this.report.SyntaxError(75, @2);}
				;

enumerated_type : enum_ident_seq ')'                 {$$ = this.MakeEnumeratedType($1);} 
                | enum_ident_seq error ')'           {$$ = this.MakeEnumeratedType($1); this.yyerrok();}
				| '(' error ')'                      {$$ = TypeNode.Error;this.yyerrok();} 
				| '(' ')'                            {$$ = TypeNode.Error; this.report.SyntaxError(166, @1);}          
				;
				
string_type     :  STRING '[' expression ']'         {$$ = this.MakeStringType($3, @1);}        
				| WSTRING '[' expression ']'         {$$ = this.MakeWStringType($3, @1);}     
				| STRING                             {$$ = TypeNode.String;}      
				| WSTRING                            {$$ = TypeNode.WString;}                         
				;

subrange_type   : non_generic_type '(' {this.SubrangeTypeStart($1);} subrange ')'       {$$ = this.MakeSubrangeType($1, $4, @4);}
				;

array_init_seq  : '[' {this.PushArrayElemType(@1);} initializer {$$ = this.MakeArrayInitializer($3, @3);    }
				|  array_init_seq ',' initializer               {$$ = this.AddArrayInitializer($1, $3, @3); }          
				;

struct_init_seq : '(' initializer                               {$$ = this.MakeStructInitializer($2, @1);}
				|  struct_init_seq ',' initializer              {$$ = this.AddStructMemberInitializer($1, $3, @3);} 
				;

array_init_list : array_init_seq ']'                            {$$ = this.WrapUpArrayInitList($1, @2);}
 				| array_init_seq error ']'                      {$$ = this.WrapUpArrayInitList($1, @3); this.yyerrok();} 
				| '[' error ']'                                 {$$ = null; this.yyerrok();}
				| '[' ']'                                       {$$ = null; this.report.Warning(19, @1);}
				;

struct_init_list
                : struct_init_seq ')'                           {$$ = this.WrapUpStructInitList($1, @2);}
				| struct_init_seq error ')'                     {$$ = this.WrapUpStructInitList($1, @3); this.yyerrok();}
	            | '(' ')'                                       {$$ = null; this.report.Warning(19, @1);} 
				;

initializer     : initial_value                                                {$$ = $1;}
				| INT_LIT '(' {this.CheckIfArrayType(@1);} error ')'           {$$ = this.ExpandInitializerSequence($1, null, @1);this.yyerrok();}
				| INT_LIT '(' {this.CheckIfArrayType(@1);} initializer_seq ')' {$$ = this.ExpandInitializerSequence($1, $4, @1);}
				| IDENT {this.PushFieldType($1, @1);} ASSIGN initial_value     {$$ = this.MakeStructMemberInit($1, $4);}
				;

initializer_seq : initializer                           {$$ = this.MakeInitializerSequence($1, @1);                    }
				| initializer_seq ',' initializer       {$$ = this.AddInitializerToSequence($1, $3, @3);               }
				| initializer_seq error initializer     {$$ = this.AddInitializerToSequence($1, $3, @3);this.yyerrok();}
				;

initial_value   : expression                              {$$ = this.CheckInitialValue($1, @1);}
				| array_init_list                         {$$ = $1;}
				| struct_init_list                        {$$ = $1;}
				;

data_type_decl  : TYPE type_decl_list END_TYPE        {this.isTypeDecl = false;}
				| TYPE type_decl_list error END_TYPE  {this.isTypeDecl = false; this.yyerrok();}
				| TYPE error END_TYPE                 {this.isTypeDecl = false; this.yyerrok();}
				;

type_decl_list  : {this.isTypeDecl = true;} type_decl              
				| type_decl_list type_decl
				; 
				
semicolon       : ';'
                | {this.report.SyntaxError(63, this.Scanner.yylloc);}    
				; 
			   
type_decl       : IDENT ':' {this.derivedTypeName = $1;} data_type_spec semicolon  {this.InstallDerivedType($1, $4, @1);}
				;

constant        : TRUE         {$$ = this.MakeConstant(true, $1);}         
				| FALSE        {$$ = this.MakeConstant(false, $1);}  
				| INT_LIT      {$$ = this.MakeConstant($1);}   
				| REAL_LIT     {$$ = this.MakeConstant($1);}    
				| TOD_LIT      {$$ = this.MakeConstant($1);}    
				| TIME_LIT     {$$ = this.MakeConstant($1);}   
				| DATE_LIT     {$$ = this.MakeConstant($1);}   
				| DT_LIT       {$$ = this.MakeConstant($1);}      
				| STRING_LIT   {$$ = this.MakeConstant($1, @1);}       
				| WSTRING_LIT  {$$ = this.MakeWString($1, @1); }   
				| TYPED_INT    {$$ = this.MakeConstant($1, @1);}   
				| TYPED_REAL   {$$ = this.MakeConstant($1, @1);}
				| TYPED_ENUM   {$$ = this.MakeConstant($1, @1);}   
				;
 
variable	    : symbolic_variable               {$$ = this.MakeSymbolicVariable($1);}
				| DIRECT_VAR                      {$$ = this.MakeDirectVariable($1, @1);}       
				;
							   
symbolic_variable    
				: simple_variable                 {$$ = $1;}
				| indexed_variable  ']'           {$$ = this.MakeIndexedVariable($1);}
				| simple_variable '[' error ']'   {$$ = Expression.Error; this.yyerrok();}
				;
							
indexed_variable: simple_variable '[' {this.isIndexExpr = true;} expression  {$$ = this.MakeIndexedVariable($1, $4, @1, @4);}
				| indexed_variable ',' expression {$$ = this.MakeIndexedVariable($1, $3, @1, @3);}
				;
				
simple_variable : IDENT                            {$$ = this.MakeSimpleVariable($1, @1);    }  
				| symbolic_variable '.' IDENT      {$$ = this.MakeSimpleVariable($1, $3, @3);}
				;
				
expression      : NOT expression                   {$$ = this.MakeNotOperator($2, @2);}           
				| '-' expression %prec UMINUS      {$$ = this.MakeUnaryMinusOperator($2, @2);}
				| '+' expression %prec UMINUS      {$$ = this.MakeUnaryPlusOperator($2, @2);}
				| expression '=' expression        {$$ = this.MakeEqlOperator($1, $3, @2);}
				| expression '<' expression        {$$ = this.MakeLesOperator($1, $3, @2);}
				| expression '>' expression        {$$ = this.MakeGtrOperator($1, $3, @2);} 
				| expression NEQ expression        {$$ = this.MakeNeqOperator($1, $3, @2);}
				| expression GEQ expression        {$$ = this.MakeGeqOperator($1, $3, @2);}  
				| expression LEQ expression        {$$ = this.MakeLeqOperator($1, $3, @2);}
				| expression AND expression        {$$ = this.MakeAndOperator($1, $3, @2);}
				| expression IOR expression        {$$ = this.MakeIOrOperator($1, $3, @2);}
				| expression XOR expression        {$$ = this.MakeXOrOperator($1, $3, @2);}
				| expression POW expression        {$$ = this.MakePowOperator($1, $3, @2);}
				| expression MOD expression        {$$ = this.MakeModOperator($1, $3, @2);}
				| expression '&' expression        {$$ = this.MakeAndOperator($1, $3, @2);}
				| expression '+' expression        {$$ = this.MakeAddOperator($1, $3, @2);}
				| expression '-' expression        {$$ = this.MakeSubOperator($1, $3, @2);}
				| expression '*' expression        {$$ = this.MakeMulOperator($1, $3, @2);}
				| expression '/' expression        {$$ = this.MakeDivOperator($1, $3, @2);}
				| '(' expression ')'               {$$ = $2;}
				| '(' expression error             {$$ = $2; this.report.SemanticError(43, @3);}
				| prog_org_unit_call               {$$ = this.MakeFunctionCall($1);       }
				| constant                         {$$ = $1;                              }                
				| variable                         {$$ = $1; this.CheckIfForLoopVar($1, @1);}
				| '(' error ')'                    {$$ = Expression.Error; this.yyerrok();}
				;

pou_parameter   : expression                       {$$ = this.MakeParameter($1, @1);}
				| IDENT assign expression          {$$ = this.MakeParameter($1, $2, $3, @1);}
				| NOT IDENT assign expression      {$$ = this.MakeParameter($2, $3, $4, @2, true);}
				;

pou_call_sequence 
                : IDENT '(' pou_parameter              {$$ = this.MakePOUCall($1, $3, @1);}
                | pou_call_sequence ',' pou_parameter  {$$ = this.AddPOUParameter($1, $3);}
				;

prog_org_unit_call        
                : IDENT '(' ')'                    {$$ = this.MakePOUCall($1, @1);}
                | IDENT '(' error ')'              {$$ = this.MakePOUCall($1, @1); this.yyerrok();}
                | pou_call_sequence ')'            {$$ = this.MakePOUCall($1, @1);}
				;

statement_list  : {this.PushTop();} statement ';'  {$$ = this.MakeStatementList($2);}  
				| statement_list  statement ';'    {$$ = this.AddToStatementList($1, $2, @2);}
				| error ';'                        {$$ = StatementList.Empty; this.yyerrok();}                             
				;
				
statement	    : WHILE condition_DO
					 while_statement_body          {$$ = this.MakeWhileStatement($2, $3, @1);}
				| REPEAT {this.loopNestingDepth++;}
					 repeat_statement_body         {$$ = $3;}
				| IF condition_THEN
					 statement_list  {this.PopTop();}
					 opt_elsif_stat 
					 opt_else_stat
				  END_IF                           {$$ = this.MakeIfStatement($2, $3, $5, $6);}
				| FOR control_variable assign for_list_DO {this.PushForLoopData($2, $4);}
					 for_statement_body            {$$ = this.MakeForLoopStatement($2, $4, $6);}    
				| CASE expression_OF
					 case_element_list         
			      END_CASE                         {$$ = this.MakeCaseStatement($2, $3, @4);}
				| assignment_stat                  {$$ = $1;}
				| EXIT                             {$$ = this.MakeExitStatement(@1);}                  
				| RETURN                           {$$ = this.MakeReturnStatement(@1);}
				| RETURN expression                {$$ = this.MakeReturnStatement2(@2);}
				| prog_org_unit_call               {$$ = this.MakeFunctionBlockCallStatement($1);}
				|                                  {$$ = Statement.Empty;}                     
				;
				
while_statement_body 
				: statement_list END_WHILE       {$$ = $1;}
				| statement_list error END_WHILE {$$ = $1;this.yyerrok();}
				| error END_WHILE                {$$ = StatementList.Empty;this.yyerrok();} 
				;
				
repeat_statement_body
				: statement_list UNTIL until_condition        {$$ = this.MakeRepeatStatement($1, $3, @1);}      
				| statement_list error UNTIL until_condition  {$$ = this.MakeRepeatStatement($1, $4, @1);this.yyerrok();}
				| error END_REPEAT                            {$$ = this.MakeRepeatStatement(); this.yyerrok();} 
				;
  
for_statement_body
				: statement_list END_FOR         {$$ = $1;}
				| statement_list error END_FOR   {$$ = $1; this.yyerrok();}
				| error END_FOR                  {$$ = StatementList.Empty; this.yyerrok();} 
				;
				
condition_DO    : expression DO  {$$ = this.CheckIfBoolCondition("WHILE-DO", $1, @1);}
				| error DO       {$$ = Expression.Error; this.yyerrok();}
				| expression     {$$ = this.CheckIfBoolCondition("WHILE-DO", $1, @1); this.report.SyntaxError(80, @1);}
				;

condition_THEN  : expression THEN  {$$ = this.CheckIfBoolCondition("IF-THEN-ELSE", $1,  @1);}
				| error THEN       {$$ = Expression.Error; this.yyerrok();}
				| expression       {$$ = this.CheckIfBoolCondition("IF-THEN-ELSE", $1, @1);this.report.SyntaxError(81, @1);}
				;

until_condition : expression END_REPEAT {$$ = this.CheckIfBoolCondition("REPEAT-UNTIL", $1, @1);}
				| error END_REPEAT      {$$ = Expression.Error; this.yyerrok();}
				| expression            {$$ = this.CheckIfBoolCondition("REPEAT-UNTIL", $1, @1);this.report.SyntaxError(82, @1);}
				;

expression_OF   : expression OF {$$ = this.CheckCtrlExpression(@1, $1);}
				| error OF      {$$ = this.CheckCtrlExpression(@1);this.yyerrok();}
				| expression    {$$ = this.CheckCtrlExpression(@1, $1);this.report.SyntaxError(83, @1);}
				;

control_variable: variable {$$ = this.SaveControlVariable($1, @1);this.forLoopVarKind = 0x2;}
				;
				
for_list_DO     : expression TO {this.forLoopVarKind = 0x4;} expression optional_by_stat DO {$$ = this.MakeForLoopData($1, $4, $5, @1, @4, @5);}
				| error TO {this.forLoopVarKind = 0x4;} expression optional_by_stat DO      {$$ = this.MakeForLoopData(null, $4, $5, null, @4, @5);this.yyerrok();}
				| expression TO error DO                                                    {$$ = this.MakeForLoopData($1, null, null, @1);this.yyerrok();}
				| error DO                                                                  {$$ = this.MakeForLoopData(null, null, null);this.yyerrok();}
				;

optional_by_stat
				: BY {this.forLoopVarKind = 0x8;} expression {$$ = this.CheckForLoopIncr($3, @3);}
				|                                            {$$ = MakeIntConstant((long)1);}
				;

opt_else_stat   : ELSE statement_list  {$$ = $2; this.PopTop();  }
				| ELSE error           {$$ = null; this.PopTop();}
				|                      {$$ = null;}      
				;

opt_elsif_stat  : elsif_stat   {$$ = $1;}
				|              {$$ = null;}                 
				;
				
default_statement
				: ELSE case_stat_list       {$$ = $2;}
				| ELSE case_stat_list error {$$ = $2; this.yyerrok();}    
				;

case_stat_list  : {this.PushTop();} statement ';' {$$ = this.MakeCaseStatList($2);         }
                | case_stat_list statement ';'    {$$ = this.AddToCaseStatList($1, $2, @2);}
				| error ';'                
				;

// WHEN is a dummy token returned by the scanner to mark the begining of a constant
// list. The token is used to resolve a conflict that arises in the grammar for the
// CASE-statement.
		
case_element    : WHEN case_label_list ':' case_stat_list {$$ = this.MakeCaseElement($2, $4);}
				| WHEN error ':' case_stat_list           {$$ = this.MakeCaseElement(null, $4);this.yyerrok();}
				| WHEN case_label_list case_stat_list     {$$ = this.MakeCaseElement(null, $3); this.report.SyntaxError(91, @3);}
				| default_statement                       {$$ = this.MakeDefaultCaseElement($1);}
				;
			   
case_elem_list  : case_element                    {$$ = this.MakeCaseElementList($1);}
				| case_elem_list case_element     {$$ = this.AddCaseElementToList($1, $2, @2);}
				;

case_element_list
                : case_elem_list   {$$ = $1;}
				|                  {$$ = new List<CaseElement>();}
				;

case_label      : expression       {$$ = this.CheckCaseLabel($1, @1);}
				| subrange         {$$ = this.CheckCaseLabel($1, @1);}
				;

case_label_list : case_label                      {$$ = this.MakeCaseLabelList($1, @1);     }
				| case_label_list ',' case_label  {$$ = this.AddCaseLabelToList($1, $3, @3);}           
				;

elsif_stat      : ELSIF condition_THEN
					 statement_list               {$$ = this.MakeElseIfStatement($2, $3);}
				| elsif_stat ELSIF condition_THEN
					 statement_list               {$$ = this.AddElseIfStatementToList($1, $3, $4);}
				;

assignment_stat : variable ASSIGN expression {$$ = this.MakeAssignmentStatement($1, $3, @1);}
				| variable  '='   expression {$$ = this.MakeAssignmentStatement($1, $3, @1);}
				;

function_body   : statement_list END_FUNCTION         {$$ = this.CheckFunctionValueDefinition($1, @2);}
                | statement_list error END_FUNCTION   {$$ = $1;this.yyerrok();}
//				| instruction_list END_FUNCTION_BLOCK {$$ = $1;}
				;

function_block_body 
				: statement_list END_FUNCTION_BLOCK       {$$ = $1;}
				| statement_list error END_FUNCTION_BLOCK {$$ = $1; this.yyerrok();}
//				| instruction_list END_FUNCTION_BLOCK     {$$ = $1;}
				;

program_body    : statement_list END_PROGRAM              {$$ = $1;}
                | statement_list error END_PROGRAM        {$$ = $1; this.yyerrok();}
//				| instruction_list END_PROGRAM            {$$ = StatementList.Empty;}
				;

function_decl   : FUNCTION IDENT ':' non_generic_type {this.PushSymbolTable(0);}
					 pou_variable_decls {this.InstallFunctionProtoType($2, $4, $6, @1);} 
					 function_body {this.SaveFunctionDefinition($6, $8);} 
				;
				
function_block_decl 
				: FUNCTION_BLOCK IDENT   {this.PushSymbolTable(1);}
					 pou_variable_decls  {this.InstallFunctionBlockProtoType($2, $4, @1);}
					 function_block_body {this.SaveFunctionBlockDeclaration($4, $6);}
				;

program_decl    : PROGRAM IDENT {this.PushSymbolTable(2);}
					 pou_variable_decls
					 program_body
				;

input_var_decl  : VAR_INPUT opt_var_qualifier {this.CheckVarTypeQualUsage(STVarType.VAR_INPUT, $2, @1, @2);}
				     var_declaration {$$ = this.MakeFormalParameterDecl(STVarType.VAR_INPUT, $2, $4);}
				| VAR_INPUT error END_VAR {$$ = null; this.yyerrok();}
				;

var_decl        : VAR opt_var_qualifier {this.CheckVarTypeQualUsage(STVarType.VAR, $2, @1, @2);}
					 var_declaration {$$ = this.MakeLocalVariableDecl(STVarType.VAR, $2, $4);}
			    | VAR error END_VAR  {$$ = null; this.yyerrok();}
				;

output_var_decl : VAR_OUTPUT opt_var_qualifier {this.CheckVarTypeQualUsage(STVarType.VAR_OUTPUT, $2, @1, @2);}
					 var_declaration {$$ = this.MakeFormalParameterDecl(STVarType.VAR_OUTPUT, $2, $4);}
			    | VAR_OUTPUT error END_VAR {$$ = null; this.yyerrok();}
				;
				
global_var_decl : VAR_GLOBAL opt_var_qualifier {this.CheckVarTypeQualUsage(STVarType.VAR_OUTPUT, $2, @1, @2);}
					 var_declaration
				| VAR_GLOBAL error END_VAR {$$ = null; this.yyerrok();}
				;
				
inout_var_decl  : VAR_IN_OUT opt_var_qualifier {this.CheckVarTypeQualUsage(STVarType.VAR_INOUT, $2,  @1, @2);}
					 var_declaration {$$ = this.MakeFormalParameterDecl(STVarType.VAR_INOUT, $2, $4);}
				| VAR_IN_OUT error END_VAR {$$ = null; this.yyerrok();}
				;
				
temp_var_decl   : VAR_TEMP opt_var_qualifier {this.CheckVarTypeQualUsage(STVarType.VAR_TEMP, $2,  @1, @2);}
					 var_declaration {$$ = this.MakeLocalVariableDecl(STVarType.VAR_TEMP, $2, $4);}
				| VAR_TEMP error END_VAR {$$ = null; this.yyerrok();}
				;
				
extern_var_decl : VAR_EXTERNAL opt_var_qualifier {this.CheckVarTypeQualUsage(STVarType.VAR_EXTERNAL, $2,  @1, @2);}
					 var_declaration
				| VAR_EXTERNAL error END_VAR {$$ = null; this.yyerrok();}
				;

access_var_decl : VAR_ACCESS opt_var_qualifier {this.CheckVarTypeQualUsage(STVarType.VAR_ACCESS, $2,  @1, @2);}
					 var_declaration
				| VAR_ACCESS error END_VAR {$$ = null; this.yyerrok();}
				;

config_var_decl : VAR_CONFIG opt_var_qualifier {this.CheckVarTypeQualUsage(STVarType.VAR_CONFIG, $2,  @1, @2);}
					 var_declaration
				| VAR_CONFIG error END_VAR {$$ = null; this.yyerrok();}
				;

pou_variable_decl   
                : var_decl        {$$ = $1;}
				| temp_var_decl   {$$ = $1;}
				| extern_var_decl {$$ = $1;}
				| global_var_decl {$$ = $1;}
				| access_var_decl {$$ = $1;}
				| config_var_decl {$$ = $1;}
                | input_var_decl  {$$ = $1;}
				| output_var_decl {$$ = $1;}
				| inout_var_decl  {$$ = $1;}
				| data_type_decl  {$$ = null;}
				;
				
pou_var_decl_list
                : pou_variable_decl                    {$$ = this.MakePOUVarDeclList($1);}
				| pou_var_decl_list pou_variable_decl  {$$ = this.AddPOUVarDeclToList($1, $2);}
				;
				
pou_variable_decls  
                : pou_var_decl_list {$$ = $1;}
				|                   {$$ = this.MakeEmptyPOUVarDecl();}
				;	 

var_decl_list   : var_init_decl semicolon                {$$ = this.MakeVariableDeclList($1);}
				| var_decl_list var_init_decl semicolon  {$$ = this.AddToVariableDeclList($1, $2);}
				;

var_declaration : var_decl_list END_VAR       {$$ = $1;}
                | var_decl_list error END_VAR {$$ = $1;this.yyerrok();}
				;

var_init_decl   : identifier_list ':' data_type opt_decl_qualifier {$$ = this.InstallLocalVars($1, $3, $4, @4);}
				| identifier_list ':' data_type opt_decl_qualifier {this.Push($3);} ASSIGN initial_value {$$ = this.InstallLocalVars($1, $3, $4, $7, @5);}
				| IDENT AT DIRECT_VAR {} ':' data_type_spec        {$$ = this.InstallSymbolicVariable($1, $6, $3, @1);}
				| AT DIRECT_VAR {} ':' data_type_spec              {$$ = this.InstallDirectVariable($2, $5, @2);}
				;
			
opt_var_qualifier  
				: RETAIN          {$$ = STVarQualifier.RETAIN;}
				| NON_RETAIN      {$$ = STVarQualifier.NON_RETAIN;}
				| CONSTANT        {$$ = STVarQualifier.CONSTANT;}
				| CONSTANT RETAIN {$$ = STVarQualifier.CONSTANT; this.report.SemanticError(173, @1);}
				| RETAIN CONSTANT {$$ = STVarQualifier.CONSTANT; this.report.SemanticError(173, @1);}
				|                 {$$ = STVarQualifier.NONE;}
				;

opt_decl_qualifier  
				: R_EDGE          {$$ = STDeclQualifier.R_EDGE;}
				| F_EDGE          {$$ = STDeclQualifier.F_EDGE;}
				| READ_ONLY       {$$ = STDeclQualifier.READ_ONLY;}
				| WRITE_ONLY      {$$ = STDeclQualifier.WRITE_ONLY;}
				|                 {$$ = STDeclQualifier.NONE;}
				;

reserved_word   : elementary_type     {$$ = $1.Name;}
				| STRING              {$$ = $1;}
				| WSTRING             {$$ = $1;}
				| RETAIN              {$$ = $1;}
				| NON_RETAIN          {$$ = $1;}
				| CONSTANT            {$$ = $1;}
	            | R_EDGE              {$$ = $1;}
				| F_EDGE              {$$ = $1;}
				| READ_ONLY           {$$ = $1;}
				| WRITE_ONLY          {$$ = $1;}
				| VAR_INPUT           {$$ = $1;}
				| VAR_OUTPUT          {$$ = $1;}
				| VAR_IN_OUT          {$$ = $1;}
				| VAR_GLOBAL          {$$ = $1;}
				| VAR_ACCESS          {$$ = $1;}
				| VAR_CONFIG          {$$ = $1;}
				| VAR_TEMP            {$$ = $1;}
				| VAR_EXTERNAL        {$$ = $1;}
				| VAR                 {$$ = $1;}
				| END_VAR             {$$ = $1;}
				| AT                  {$$ = $1;}
				| FUNCTION            {$$ = $1;}
				| END_FUNCTION        {$$ = $1;}
				| FUNCTION_BLOCK      {$$ = $1;}
				| END_FUNCTION_BLOCK  {$$ = $1;}
				| PROGRAM             {$$ = $1;}
				| END_PROGRAM         {$$ = $1;}
				| IF                  {$$ = $1;}
				| THEN                {$$ = $1;}
				| ELSE                {$$ = $1;}
				| ELSIF               {$$ = $1;}
				| END_IF              {$$ = $1;}
				| WHILE               {$$ = $1;}
				| END_WHILE           {$$ = $1;}
				| REPEAT              {$$ = $1;}
				| UNTIL               {$$ = $1;}
				| END_REPEAT          {$$ = $1;}
				| FOR                 {$$ = $1;}
				| TO                  {$$ = $1;} 
				| BY                  {$$ = $1;}
				| DO                  {$$ = $1;}
				| END_FOR             {$$ = $1;}
				| CASE                {$$ = $1;}
				| OF                  {$$ = $1;}
				| END_CASE            {$$ = $1;}
				| EXIT                {$$ = $1;}
				| RETURN              {$$ = $1;}
				| TYPE                {$$ = $1;}
				| END_TYPE            {$$ = $1;}
				| ARRAY               {$$ = $1;}
				| STRUCT              {$$ = $1;}
				| END_STRUCT          {$$ = $1;}
				| AND                 {$$ = "AND";}
				| NOT                 {$$ = "NOT";}
				| IOR                 {$$ = "OR";}
				| XOR                 {$$ = "XOR";}
				| MOD                 {$$ = "MOD";}
				| CONFIGURATION       {$$ = $1;}
				| END_CONFIGURATION   {$$ = $1;}
				| TRANSITION          {$$ = $1;}
				| END_TRANSITION      {$$ = $1;}
				| RESOURCE            {$$ = $1;}
				| END_RESOURCE        {$$ = $1;}
				| WITH                {$$ = $1;}
				| TASK                {$$ = $1;}
				;

%%
	public STLangParser(STLangScanner scanner, ErrorHandler errorHandler) : base(scanner) 
	{ 
		this.report = errorHandler;
		Expression.Report = errorHandler;
		TypeNode.Report = errorHandler;
		STLangSymbol.Report = errorHandler;
		errorHandler.Scanner = scanner;
		this.structNestingDepth = 0;
		this.forLoopVarKind = 0;
		this.loopNestingDepth = 0;
		this.functionValueDef = new List<bool>(){false};
		this.forLoopVarTable = new List<Hashtable>();
		this.forLoopDataList = new List<ForLoopData>();
		this.attributeStack = new SemanticStack(errorHandler);
		constantTable = new Dictionary<string, Expression>();
		this.rwMemoryManager = new RWMemoryLayoutManager();
		this.symbolTable = new STLangSymbolTable(errorHandler);
		this.caseLabelStack = null;
		this.isFunctionDecl = false;
		this.isProgramDecl  = false;
		this.isFunctionBlockDecl = false;
		this.variablePosition = 0;
		this.isTypeDecl = false;
		this.isIndexExpr = false;
		this.isSubrangeDecl = false;
		this.subrangeDataType = TypeNode.Error;
	}

	public int Errors
    {
        get { return this.report.Errors; }
    }

    public int Warnings
    {
        get { return this.report.Warnings; }
    }

    public IEnumerable<string> Messages
    {
        get { return this.report.ErrorMessages; }
    }

	private STVarType variableType;

	private STVarQualifier variableQualifier;

	private int structNestingDepth;

	private int forLoopVarKind;

	private int loopNestingDepth;

	private static Dictionary<string, Expression> constantTable;

	private readonly List<Hashtable> forLoopVarTable;

	private readonly List<bool> functionValueDef;

	private readonly ErrorHandler report;

	private readonly STLangSymbolTable symbolTable;

	private readonly SemanticStack attributeStack;

	private RWMemoryLayoutManager rwMemoryManager;

	private List<List<CaseLabel>> caseLabelStack;

	private List<ForLoopData> forLoopDataList;

	private List<CaseLabel> caseLabelList;

	private int variablePosition;

	private bool isProgramDecl;

	private bool isFunctionDecl;

	private bool isFunctionBlockDecl;

	private bool isTypeDecl;

	private bool isIndexExpr;

	private string derivedTypeName;

	private TypeNode subrangeDataType;

	private bool isSubrangeDecl;

	private void ReInitializeParser()
	{
		this.structNestingDepth = 0;
		this.forLoopVarKind = 0;
		this.loopNestingDepth = 0;
		this.forLoopVarTable.Clear();
		this.attributeStack.Clear();
		this.functionValueDef.Clear();
		this.functionValueDef.Add(false);
		constantTable = new Dictionary<string, Expression>();
		this.symbolTable.ReInitialize();
		this.isFunctionDecl = false;
		this.isProgramDecl  = false;
		this.isFunctionBlockDecl = false;
		this.variablePosition = 0;
		this.isTypeDecl = false;
		this.isIndexExpr = false;
		this.isSubrangeDecl = false;
		this.subrangeDataType = TypeNode.Error;
		this.rwMemoryManager = new RWMemoryLayoutManager();
	}

	private DataTypeSpec MakeDataTypeSpec(TypeNode dataType, STDeclQualifier declQualifier, LexLocation location)
	{
		if (declQualifier != STDeclQualifier.NONE)
		{
			if (dataType != TypeNode.Error && dataType != TypeNode.Bool)
			{
				this.report.SemanticError(120, location);
				declQualifier = STDeclQualifier.NONE;
			}
			if (this.structNestingDepth > 0)
			{
				this.report.SemanticError(121, location);
				declQualifier = STDeclQualifier.NONE;
			}
		}
		return new DataTypeSpec(dataType, declQualifier, dataType.DefaultValue);
	}

	private DataTypeSpec MakeDataTypeSpec(TypeNode dataType, STDeclQualifier declQualifier, Expression initialValue, LexLocation location)
	{
		if (declQualifier != STDeclQualifier.NONE)
		{
			if (dataType != TypeNode.Bool)
			{
				this.report.SemanticError(120, location);
				declQualifier = STDeclQualifier.NONE;
			}
			if (this.structNestingDepth > 0)
			{
				this.report.SemanticError(121, location);
				declQualifier = STDeclQualifier.NONE;
			}
		}
		this.Pop();
		return new DataTypeSpec(dataType, declQualifier, initialValue);
	}

	private List<string> MakeIdentifierList(string ident, LexLocation location)
	{
		List<string> identList = new List<string>();
		if (this.symbolTable.IsValidUserDefinedSymbol(ident, location))
			identList.Add(ident);
		return identList;
	}

	private List<string> AddIdentToList(List<string> identList, string ident, LexLocation location)
	{
		if (identList == null)
			return this.MakeIdentifierList(ident, location);
		else if (! identList.IsUnique(ident))
			this.report.SemanticError(2, ident, location);
		else if (this.symbolTable.IsValidUserDefinedSymbol(ident, location))
			identList.Add(ident);
		return identList;
	}

	// void RegisterForLoopVariable(InstanceSymbol symbol, ForLoopVariableType loopVarType)
	//
	// Keeps track of variables used as control variables, start-, stop-values and increments in for-loops.
	// This information is used to check that these variables aren't changed inside the loop.
	//
	private void RegisterForLoopVariable(InstanceSymbol symbol, ForLoopVariableType loopVarType, LexLocation location)
	{
		if (loopVarType == ForLoopVariableType.CONTROL_VARIABLE)
		{
			// Make sure that the variable isn't already used as a control 
			// variable, start-value or stop-value in any outer for-loop.

			foreach (Hashtable loopVarTable in this.forLoopVarTable)
			{
				if (loopVarTable.Contains(symbol))
				{
					ForLoopVariableType forLoopVarType;
					forLoopVarType = (ForLoopVariableType)loopVarTable[symbol];
					if ((forLoopVarType & ForLoopVariableType.CONTROL_VARIABLE) != 0)
						this.report.SemanticError(125, symbol.Name, location);
					if ((forLoopVarType & ForLoopVariableType.START_VARIABLE) != 0)
						this.report.SemanticError(126, symbol.Name, location);
					if ((forLoopVarType & ForLoopVariableType.STOP_VARIABLE) != 0)
						this.report.SemanticError(127, symbol.Name, location);
					if ((forLoopVarType & ForLoopVariableType.INCR_VARIABLE) != 0)
						this.report.Warning(15, symbol.Name, location);
				}
			}
			Hashtable currentLoopVarTable = new Hashtable();
			currentLoopVarTable[symbol] = ForLoopVariableType.CONTROL_VARIABLE;
			this.forLoopVarTable.Add(currentLoopVarTable);
		}
		else {
			Hashtable loopVarTable = this.forLoopVarTable.Last();
			if (loopVarTable == null)
				throw new STLangCompilerError("For-loop variable table is empty.");
			else if (! loopVarTable.Contains(symbol))
				loopVarTable[symbol] = loopVarType;
			else {
				ForLoopVariableType forLoopVarType;

				forLoopVarType = (ForLoopVariableType)loopVarTable[symbol];
				if ((forLoopVarType & ForLoopVariableType.CONTROL_VARIABLE) == 0)
				{
					forLoopVarType |= loopVarType;
					loopVarTable[symbol] = forLoopVarType;
				}
				else if (loopVarType == ForLoopVariableType.START_VARIABLE)
					this.report.SemanticError(126, symbol.Name, location);
				else if (loopVarType == ForLoopVariableType.STOP_VARIABLE)
					this.report.SemanticError(127, symbol.Name, location);
				else if (loopVarType == ForLoopVariableType.INCR_VARIABLE)
					this.report.Warning(15, symbol.Name, location);
			}
		}
	}


	private void PopForLoopVariables()
	{
		if (this.forLoopVarTable.Count > 0)
		{
			Hashtable currentLoopVars = this.forLoopVarTable.Last();
			this.forLoopVarTable.Remove(currentLoopVars);
			ForLoopData forLoopData = this.forLoopDataList.Last();
			this.forLoopDataList.Remove(forLoopData);
			forLoopData.ControlVariable.Symbol.IsForLoopCtrlVar = false;
		}
	}

	private void Push(Expression expression)
	{
		if (expression == null)
			this.attributeStack.Push(TypeNode.Error);
		else
			this.attributeStack.Push(expression.DataType);
	}

	private void PushTop()
	{
		if (this.functionValueDef.Count == 0)
			throw new STLangCompilerError("Function value stack is empty.");
		else {
			bool topValue = this.functionValueDef.Last();
			this.functionValueDef.Add(topValue);
		}
	}

	private void PushCaseLabelList()
	{
		if (this.caseLabelStack == null)
		{
			this.caseLabelStack = new List<List<CaseLabel>>();
			this.caseLabelList = new List<CaseLabel>();
			this.caseLabelStack.Add(this.caseLabelList);
		}
		else {
			this.caseLabelStack.Add(this.caseLabelList);
			this.caseLabelList = new List<CaseLabel>();
		}
	}

	private void PopCaseLabelList()
	{
		if (this.caseLabelStack.Count == 0)
			throw new STLangCompilerError("PopCaseLabelList(): Case label stack is empty.");
		else {
			int lastIndex = this.caseLabelStack.Count - 1;
			this.caseLabelStack.RemoveAt(lastIndex);
			if (this.caseLabelStack.Count > 0)
                this.caseLabelList = this.caseLabelStack.Last();
            else {
				this.caseLabelStack = null;
				this.caseLabelList = null;
			} 
		}
	}

	private void CopyValue(bool funcValueDefined)
	{
		if (this.functionValueDef.Count == 0)
			throw new STLangCompilerError("CopyValue(): Function value stack is empty.");
		else {
			bool currentValue = this.functionValueDef.Last();
			if (funcValueDefined && ! currentValue)
			{
				int topIndex = this.functionValueDef.Count - 1;
				this.functionValueDef[topIndex] = true;
			}
		}
	}

	private void PopTop()
	{
		if (this.functionValueDef.Count == 0)
			throw new STLangCompilerError("Function value stack is empty.");
		else if (this.functionValueDef.Count > 1)
		{
			int topIndex = this.functionValueDef.Count - 1;
			this.functionValueDef.RemoveAt(topIndex);
		}
	}

	private void CheckFunctionValueIsDefined(LexLocation location)
	{
		if (this.functionValueDef.Count == 0)
			throw new STLangCompilerError("Function value stack is empty.");
		else {
			bool functionValueIsDefined = this.functionValueDef.Last();
			if (! functionValueIsDefined)
				this.report.SemanticError(13, location);
		}
	}

	private StatementList CheckFunctionValueDefinition(StatementList statementList, LexLocation loc)
	{
		this.CheckFunctionValueIsDefined(loc);
		if (statementList == null)
			statementList = StatementList.Empty;
		return statementList;
	}

	private void SaveFunctionDefinition(POUVarDeclarations varDecls, StatementList functionBody)
	{
		this.symbolTable.Pop();
		if (! functionBody.POUReturns)
			functionBody.Add(new ReturnStatement());  // Make sure the function returns
		this.rwMemoryManager.SetSegmentAlignment(sizeof(double));
		varDecls.SetDeclarationSize(this.rwMemoryManager);
		this.symbolTable.SaveFunctionDefinition(functionBody, constantTable, this.rwMemoryManager);
	}

	private void SaveFunctionBlockDeclaration(POUVarDeclarations fbVarDecls, StatementList functionBlockBody)
	{
		this.symbolTable.Pop();
		if (! functionBlockBody.POUReturns)
			functionBlockBody.Add(new ReturnStatement()); // Make sure the function block returns
		this.rwMemoryManager.SetSegmentAlignment(sizeof(double));
		fbVarDecls.SetDeclarationSize(this.rwMemoryManager);
		this.symbolTable.SaveFunctionBlockBody(functionBlockBody, constantTable, this.rwMemoryManager);
	}

	private void CheckIfArrayType(LexLocation location)
	{
		this.attributeStack.CheckIfArrayType(location);
	}

	private void PushArrayElemType(LexLocation location)
	{
		this.attributeStack.PushArrayElementType(location);
	}

	private void Push(TypeNode dataType)
	{
		this.attributeStack.Push(dataType);
	}

	private void Pop()
	{
		this.attributeStack.Pop();
	}

	private TypeNode LargestDataType(Expression expr1, Expression expr2)
	{
		return expr1.DataType.Size > expr2.DataType.Size ? expr1.DataType : expr2.DataType;
	}

	private StructMemberInit MakeStructMemberInit(string name, Expression initValue)
	{
		TypeNode fieldDataType = this.attributeStack.Top;
		this.Pop();
		if (initValue == null)
			return new StructMemberInit(name, fieldDataType.DefaultValue);
		else
			return new StructMemberInit(name, initValue);
	}

	private InitializerList MakeInitializerSequence(Expression initializer, LexLocation loc)
	{
		TypeNode dataType = this.attributeStack.Top;
		InitializerSequence initSequence = new InitializerSequence(dataType);
		initSequence.Add(initializer, loc);
		return initSequence;
	}

	private InitializerList AddInitializerToSequence(InitializerList initSequence, Expression initializer, LexLocation loc)
	{
		if (initSequence != null)
			initSequence.Add(initializer, loc);
		return initSequence;
	}

	private TypeNode MakeInitializerDataType(uint elementCount, TypeNode elementType)
	{
		string typeID;
		TypeNode arrayType;
		
		typeID = string.Format("[0..{0}]{1}", elementCount - 1, elementType.TypeID);
		if (TypeNode.LookUpType(typeID, out arrayType))
			return arrayType;
		else {
			int upper = (int)elementCount - 1;
			long byteCount = elementCount * elementType.Size;
			Expression size =  MakeIntConstant(byteCount);
			string typeName = string.Format("ARRAY [0..{0}] OF {1}", upper, elementType.Name);
			return new ArrayType(typeName, 0, upper, (uint)byteCount, size, elementType, elementType, typeID);
		}
	}

	private InitializerList MakeArrayOfStructInitializer(TypeNode array, FieldSymbol firstField, uint elementCount)
	{
		TypeNode dataType;
		TypeNode fieldDataType;
        InitializerList arrayInit;
		FieldSymbol field = firstField;
		Expression size   = MakeIntConstant(array.Size);
		ArrayOfStructInitializer arrayOfStructInitializer;

		arrayOfStructInitializer = new ArrayOfStructInitializer(array, size);
		while (field != null)
		{
			fieldDataType = field.DataType;
			if (fieldDataType.IsElementaryType || fieldDataType.IsTextType)
			{
				dataType = this.MakeInitializerDataType(elementCount, fieldDataType);
				size = MakeIntConstant((long)dataType.Size);
				arrayInit = new ArrayInitializer(dataType, size);
			}
			else if (fieldDataType.IsStructType)
			{
				StructType struct2 = (StructType)fieldDataType.BaseType;
				FieldSymbol firstField2 = struct2.FirstField;
				dataType = this.MakeInitializerDataType(elementCount, fieldDataType);
				arrayInit = this.MakeArrayOfStructInitializer(dataType, firstField2, elementCount);
			}
			else if (fieldDataType.IsArrayType)
			{
				ArrayType array2 = (ArrayType)fieldDataType.BaseType;
				TypeNode elementType = array2.BasicElementType;
				uint elemCount2 = (array2.Size/elementType.Size)*elementCount;
				dataType = this.MakeInitializerDataType(elemCount2, elementType);
				if (elementType.IsElementaryType || elementType.IsTextType)
				{
					size = MakeIntConstant((long)dataType.Size);
					arrayInit = new ArrayInitializer(dataType, size);
				}
                else if (elementType.IsStructType)
                {
                    StructType struct2 = (StructType)elementType.BaseType;
					FieldSymbol firstField2 = struct2.FirstField;
                    arrayInit = this.MakeArrayOfStructInitializer(dataType, firstField2, elemCount2);
                }
				else if (elementType.IsFunctionBlockType)
                {
					//
					// IEC 61131-3 does not allow arrays of function blocks (yet).
					//
                    size = MakeIntConstant((long)elementType.Size);
					arrayInit = new ArrayInitializer(fieldDataType, size); 
                }
				else if (elementType == TypeNode.Error)
				{
					size = MakeIntConstant((long)elementType.Size);
					arrayInit = new ArrayInitializer(elementType, size);
				}
                else
                {
                    string msg = "MakeArrayOfStructInitializer(): Unknown ";
                    msg += "type of array element " + elementType.Name;
                    throw new STLangCompilerError(msg);
                }
			}
			else if (fieldDataType.IsFunctionBlockType)
			{
				//
				// IEC 61131-3 does not allow arrays of function blocks (yet).
				//
				size = MakeIntConstant((long)fieldDataType.Size);
				arrayInit = new ArrayInitializer(fieldDataType, size); 
			}
			else if (fieldDataType == TypeNode.Error)
			{
				size = MakeIntConstant((long)fieldDataType.Size);
				arrayInit = new ArrayInitializer(fieldDataType, size);
			}
			else {
				string msg;
				msg = "MakeArrayOfStructInitializer(): Unknown type " + fieldDataType.Name;
				throw new STLangCompilerError(msg);
			}
			arrayOfStructInitializer.AddInitializer(field.Name, arrayInit);
			field = field.Next;
		}
		return arrayOfStructInitializer;
	}

	private InitializerList MakeArrayInitializer(Expression initializer, LexLocation location)
	{
		InitializerList arrayInitList;
		TypeNode dataType = this.attributeStack.Top2;
		if (initializer == null || ! dataType.IsArrayType)
		{
			Expression size = MakeIntConstant((long)1000);
			arrayInitList = new ArrayInitializer(TypeNode.Error, size);
			arrayInitList.Add(initializer, location);
			return arrayInitList;
		}
		else {
			ArrayType array = (ArrayType)dataType;
			TypeNode elementType = array.BasicElementType;
		
			if (elementType.IsElementaryType || elementType.IsTextType)
			{
				Expression size = MakeIntConstant((long)dataType.Size);
				arrayInitList = new ArrayInitializer(dataType, size);
				arrayInitList.Add(initializer, location);
				return arrayInitList;
			}
			else if (elementType.IsFunctionBlockType)
			{
				//
				// IEC 61131-3 does not allow arrays of function blocks (yet).
				//
				Expression size = MakeIntConstant((long)dataType.Size);
				arrayInitList = new ArrayInitializer(dataType, size);
				return arrayInitList; 
			}
			else if (elementType.IsStructType)
			{
				uint elemCount = array.Size/elementType.Size;
				StructType structure = (StructType)elementType.BaseType;
				FieldSymbol firstField = structure.FirstField;
				arrayInitList = this.MakeArrayOfStructInitializer(array, firstField, elemCount);
				arrayInitList.Add(initializer, location);
				return arrayInitList;
			}
		}
		throw new STLangCompilerError(Resources.MAKEARRAYINITLIST2);
	}

	private InitializerList AddArrayInitializer(InitializerList arrayInitList, Expression initializer, LexLocation loc)
	{
		if (arrayInitList == null)
		{
			TypeNode dataType = this.attributeStack.Top2;
			initializer = dataType.DefaultValue;
		}
		arrayInitList.Add(initializer, loc);
		return arrayInitList;
	}

	private InitializerList WrapUpArrayInitList(InitializerList arrayInitList, LexLocation location)
	{
		this.attributeStack.Pop();
		if (arrayInitList == null)
			return arrayInitList;
		else if (arrayInitList.DataType == TypeNode.Error)
			return arrayInitList;
		else {
			arrayInitList.CheckInitListSize(location);
			if (! arrayInitList.IsConstant)
				return arrayInitList;
			else {
				string key = arrayInitList.GetKey();
				if (! constantTable.ContainsKey(key))
				{
					constantTable[key] = arrayInitList;
					return arrayInitList;
				}
				else {
					Expression initializer;
					initializer = (Expression)constantTable[key];
					if (initializer is ArrayInitializer)
					{
						ArrayInitializer arrayInitList2;
						arrayInitList2 = (ArrayInitializer)initializer;
						if (arrayInitList2.DataType == arrayInitList.DataType)
							return arrayInitList2;
					}
					else if (initializer is ArrayOfStructInitializer)
					{
						ArrayOfStructInitializer arrayInitList2;
						arrayInitList2 = (ArrayOfStructInitializer)initializer;
						if (arrayInitList2.DataType == arrayInitList.DataType)
							return arrayInitList2;
					}
					throw new STLangCompilerError(Resources.WRAPUPARRAYINILIST);
				} 
			}
		}
	}

	private InitializerList WrapUpStructInitList(InitializerList structInitList, LexLocation location)
	{
		if (structInitList == null)
			return structInitList;
		else if (structInitList.DataType == TypeNode.Error)
			return structInitList;
		else {
			structInitList.CheckInitListSize(location);
			if (! structInitList.IsConstant)
				return structInitList;
			else {
				string key = structInitList.GetKey();
				if (! constantTable.ContainsKey(key))
				{
					constantTable[key] = structInitList;
					return structInitList;
				}
				else {
					Expression initializer;
					initializer = (Expression)constantTable[key];
					if (initializer is StructInitializer)
					{
						StructInitializer structInitList2;
						structInitList2 = (StructInitializer)initializer;
						if (structInitList2.DataType == structInitList.DataType)
							return structInitList2;
					}
					throw new STLangCompilerError(Resources.WRAPUPARRAYINILIST);
				}
			}
		}
	}

	private InitializerList MakeStructInitializer(Expression initializer, LexLocation location)
	{
		TypeNode dataType = this.attributeStack.Top;
		Expression size = MakeIntConstant((long)dataType.Size);
		if (dataType.IsStructType)
		{
			StructInitializer structInitList = new StructInitializer(dataType, size);
			structInitList.Add(initializer, location);
			return structInitList;
		}
		else if (dataType.IsFunctionBlockType)
		{
			FunctionBlockInitializer functionBlockInitList;
			functionBlockInitList = new FunctionBlockInitializer(dataType, size);
			functionBlockInitList.Add(initializer, location);
			return functionBlockInitList;
		}
		else 
		{
			if (dataType != TypeNode.Error)
				this.report.SemanticError(65, dataType.Name, location);
			return new StructInitializer(TypeNode.Error, size);
		}
	}

	private InitializerList AddStructMemberInitializer(InitializerList structInitList, Expression initializer, LexLocation loc)
	{
		if (structInitList != null)
			structInitList.Add(initializer, loc);
		return structInitList;
	}

	private StructDeclaration MakeStructMemberList(StructMemberDeclaration member)
	{	
		StructDeclaration structure = new StructDeclaration();
		if (member != null)
		{
			if (! structure.Add(member))
				this.report.SemanticError(62, member.Name, member.Location);
		}
		return structure;
	}
	
	private StructDeclaration AddStructMemberDecl(StructDeclaration structure, StructMemberDeclaration member, LexLocation loc)
	{
		if (structure != null && member != null)
		{
			if (! structure.Add(member))
				this.report.SemanticError(62, member.Name, member.Location);
		}
		return structure;
	}

	private StructMemberDeclaration MakeStructMemberDecl(string name, DataTypeSpec member, LexLocation loc)
	{
		return new StructMemberDeclaration(name, member.DataType, member.InitialValue, loc);
	}

	private int BitCount(ulong value)
	{
		int count = 0;
		while (value > 0)
		{
			count++;
			value >>= 1;
		}
		return count;
	}

	private ulong IntPower(ulong value, int power)
	{
		if (power == 0)
			return 1;
		else if ((power & 1) == 0)
			return this.IntPower(value*value, power >> 1);
		else if ((value & 1) == 0)
			return this.IntPower(value >> 1, power) << power;
		else
			return value * IntPower(value, power - 1);
	}

	private long IntPower(long value, int power)
	{
		if (power == 0)
			return 1;
		else if ((power & 1) == 0)
			return this.IntPower(value*value, power >> 1);
		else if ((value & 1) == 0)
			return this.IntPower(value >> 1, power) << power;
		else
			return value * IntPower(value, power - 1);
	}

	private bool IsPowerOf2(ulong value, out int power)
	{
		power = 0;
		if (value <= 1)
			return false;
		else {
			while ((value & 1) == 0)
			{
				power++;
				value >>= 1;
			}
			return value == 1;
		}
	}

	private Expression CheckForLoopIncr(Expression expr, LexLocation location)
	{
		if (expr == null)
			return Expression.Error;
		else if (expr is FunctionName)
		{
			this.report.SemanticError(148, expr.ToString(), location);
			return Expression.Error;
		}
		else if (expr.DataType == TypeNode.Error)
			return expr;
		else if (expr.DataType.IsSignedIntType)
			return expr;
		else {
			this.report.SemanticError(135, expr.ToString(), location);
			return Expression.Error;
		}
	}

	private Expression MakeIntConstant(TokenInt token)
	{
		ulong value = token.Value;
		string lexeme = token.ToString();
		if ((value & 0x8000000000000000) != 0)
			return new ULIntConstant(value, lexeme);
		else if (TypeNode.SInt.IsInRange((long)value))
			return new SIntConstant((sbyte)value, lexeme);
		else if (TypeNode.Int.IsInRange((long)value))
			return new IntConstant((short)value, lexeme);
		else if (TypeNode.DInt.IsInRange((long)value))
			return new DIntConstant((int)value, lexeme);
		else
			return new LIntConstant((long)value, lexeme);
	}

	private Expression MakeIntConstant(ulong value)
	{
		string strValue = value.ToString();
		if (constantTable.ContainsKey(strValue))
			return (Expression)constantTable[strValue];
		else {
			Expression intConst;
			if (TypeNode.USInt.IsInRange(value))
				intConst = new USIntConstant((byte)value, strValue);
			else if (TypeNode.UInt.IsInRange(value))
				intConst = new UIntConstant((ushort)value, strValue);
			else if (TypeNode.UDInt.IsInRange(value))
				intConst = new UDIntConstant((uint)value, strValue);
			else 
				intConst = new ULIntConstant((ulong)value, strValue);
			constantTable[strValue] = intConst;
			return intConst;
		}
	}

	public static Expression MakeIntConstant(long value)
	{
		string strValue = value.ToString();
		if (constantTable.ContainsKey(strValue))
			return (Expression)constantTable[strValue];
		else {
			Expression intConst;
			if (TypeNode.SInt.IsInRange(value))
				intConst = new SIntConstant((sbyte)value, strValue);
			else if (TypeNode.Int.IsInRange(value))
				intConst = new IntConstant((short)value, strValue);
			else if (TypeNode.DInt.IsInRange(value))
				intConst = new DIntConstant((int)value, strValue);
			else
				intConst = new LIntConstant((long)value, strValue);
			constantTable[strValue] = intConst;
			return intConst;
		}
	}

	private Expression MakeConstant(ulong value, TypeNode bitStringType)
	{
		if (bitStringType == TypeNode.Bool)
			return this.MakeConstant(value != 0);
		else {
			string typeName = bitStringType.Name;
			string strValue = typeName + "#" + value.ToString();
			if (constantTable.ContainsKey(strValue))
				return (Expression)constantTable[strValue];
			else {
				Expression bitStringConst;
				if (TypeNode.Byte.IsInRange(value))
					bitStringConst = new ByteConstant((byte)value, strValue);
				else if (TypeNode.Word.IsInRange(value))
					bitStringConst = new WordConstant((ushort)value, strValue);
				else if (TypeNode.DInt.IsInRange(value))
					bitStringConst = new DWordConstant((uint)value, strValue);
				else
					bitStringConst = new LWordConstant((ulong)value, strValue);
				constantTable[strValue] = bitStringConst;
				return bitStringConst;
			}
		}
	}

	private Expression MakeConstant(bool value)
	{
		string stringValue = value.ToString();
		string keyValue = stringValue.ToUpper();
		if (constantTable.ContainsKey(keyValue))
			return (Expression)constantTable[keyValue];
		else
		{
			Expression expr = new BoolConstant(value, stringValue);
			constantTable[keyValue] = expr;
			return expr;
		}
	}

	private Expression MakeConstant(bool value, string boolStr)
	{
		string keyValue = boolStr.ToUpper();
		if (constantTable.ContainsKey(keyValue))
			return (Expression)constantTable[keyValue];
		else
		{
			Expression expr = new BoolConstant(value, boolStr);
			constantTable[keyValue] = expr;
			return expr;
		}
	}

	private Expression MakeConstant(string stringValue, LexLocation location)
	{
		if (constantTable.ContainsKey(stringValue))
			return (Expression)constantTable[stringValue];
		else
		{
			long length = stringValue.Length;
			Expression size = MakeIntConstant(length);
			TypeNode stringType = this.MakeStringType(size, location);
			Expression expr = new StringConstant(stringValue, stringType);
			constantTable[stringValue] = expr;
			return expr;
		}
	}

	private Expression MakeWString(string stringValue, LexLocation location)
	{
		if (constantTable.ContainsKey(stringValue))
			return (Expression)constantTable[stringValue];
		else
		{
			long length = stringValue.Length;
			Expression size = MakeIntConstant(length);
			TypeNode stringType = this.MakeWStringType(size, location);
			Expression expr = new StringConstant(stringValue, stringType);
			constantTable[stringValue] = expr;
			return expr;
		}
	}

	private Expression MakeConstant(TokenTypedInt token, LexLocation location)
	{
		string lexeme = token.ToString();
		string keyValue = lexeme.RemoveChar('_').ToUpper();
		ulong value = token.Value;
		if (constantTable.ContainsKey(keyValue))
			return (Expression)constantTable[keyValue];
		else {
			Expression constant = null;
			switch (token.IntType)
			{
			case Tokens.INT:
				constant = new IntConstant((short)value, lexeme);
				break;
			case Tokens.SINT:
				constant = new SIntConstant((sbyte)value, lexeme);
				break;
			case Tokens.DINT:
				constant = new DIntConstant((int)value, lexeme);
				break;
			case Tokens.LINT:
				constant = new LIntConstant((long)value, lexeme);
				break;
			case Tokens.USINT:
				constant = new USIntConstant((byte)value, lexeme);
				break;
			case Tokens.UINT:
				constant = new UIntConstant((ushort)value, lexeme);
				break;
			case Tokens.UDINT:
				constant = new UDIntConstant((uint)value, lexeme);
				break;
			case Tokens.ULINT:
				constant = new ULIntConstant(value, lexeme);
				break;
			case Tokens.BOOL:
				constant = new BoolConstant(value != 0, lexeme);
				break;
			case Tokens.BYTE:
				constant = new ByteConstant((byte)value, lexeme);
				break;
			case Tokens.WORD:
				constant = new WordConstant((ushort)value, lexeme);
				break;
			case Tokens.DWORD:
				constant = new DWordConstant((uint)value, lexeme);
				break;
			case Tokens.LWORD:
				constant = new LWordConstant(value, lexeme);
				break;
			case Tokens.REAL:
				constant = new RealConstant((float)value);
				break;
			case Tokens.LREAL:
				constant = new LRealConstant((double)value);
				break;
			case Tokens.IDENT:
				{
					STLangSymbol symbol;
					string typeName = token.TypeName;
					if (! this.symbolTable.Lookup(typeName, out symbol, location))
					{
						this.report.SemanticError(1, typeName, location);
						return Expression.Error;
					}
					else if (! (symbol is TypeNameSymbol))
					{
						this.report.SemanticError(24, typeName, location);
						return Expression.Error;
					}
					else if (! symbol.DataType.IsElementaryType)
					{
						this.report.SemanticError(114, value, symbol.DataType.Name, location);
						return Expression.Error;
					}
					else {
						TypeNode dataType = symbol.DataType;
						keyValue = dataType.Name + "#" + value;
						keyValue = keyValue.RemoveChar('_').ToUpper();
						if (constantTable.ContainsKey(keyValue))
							return (Expression)constantTable[keyValue];
						else if (dataType == TypeNode.SInt)
							constant = new SIntConstant((sbyte)value, lexeme);
						else if (dataType == TypeNode.Int)
							constant = new IntConstant((short)value, lexeme);
						else if (dataType == TypeNode.DInt)
							constant = new DIntConstant((int)value, lexeme);
						else if (dataType == TypeNode.LInt)
							constant = new LIntConstant((long)value, lexeme);
						else if (dataType == TypeNode.USInt)
							constant = new USIntConstant((byte)value, lexeme);
						else if (dataType == TypeNode.UInt)
							constant = new UIntConstant((ushort)value, lexeme);
						else if (dataType == TypeNode.UDInt)
							constant = new UDIntConstant((uint)value, lexeme);
						else if (dataType == TypeNode.ULInt)
							constant = new ULIntConstant((ulong)value, lexeme);
						else if (dataType == TypeNode.Bool)
							constant = new BoolConstant(value != 0, lexeme);
						else if (dataType == TypeNode.Byte)
							constant = new ByteConstant((byte)value, lexeme);
						else if (dataType == TypeNode.Word)
							constant = new WordConstant((ushort)value, lexeme);
						else if (dataType == TypeNode.DWord)
							constant = new DWordConstant((uint)value, lexeme);
						else if (dataType == TypeNode.LWord)
							constant = new LWordConstant((ulong)value, lexeme);
						else if (dataType == TypeNode.Real)
							constant = new RealConstant((float)value);
						else if (dataType == TypeNode.LReal)
							constant = new LRealConstant((double)value);
						else {
							this.report.SemanticError(114, value, dataType.Name, location);
							return Expression.Error;
						}

					}
				}
				break;
			case Tokens.DATE:
			case Tokens.DATE_AND_TIME:
			case Tokens.DT:
			case Tokens.TIME:
			case Tokens.TIME_OF_DAY:
			case Tokens.TOD:
			case Tokens.STRING:
			case Tokens.WSTRING:
			default:
				this.report.SemanticError(114, value, token.IntType.ToString(), location);
				return Expression.Error;
			}
			if (constant != null)
			{
				TypeNode dataType = constant.DataType;
				constantTable[keyValue] = constant;
				return constant;
			}
		}
		return Expression.Error;
	}

	private Expression MakeConstant(TokenTypedReal token, LexLocation location)
	{
		string lexeme = token.ToString();
		double value = token.Value;
		string keyValue = lexeme.RemoveChar('_').ToUpper();
		if (constantTable.ContainsKey(keyValue))
			return (Expression)constantTable[keyValue];
		else {
			Expression constant = null;
			switch (token.RealType)
			{
			case Tokens.REAL:
				constant = new RealConstant((float)value, lexeme);
				break;
			case Tokens.LREAL:
				constant = new LRealConstant(value);
				break;
			case Tokens.INT:
				constant = new IntConstant((short)value, lexeme);
				break;
			case Tokens.SINT:
				constant = new SIntConstant((sbyte)value, lexeme);
				break;
			case Tokens.DINT:
				constant = new DIntConstant((int)value, lexeme);
				break;
			case Tokens.LINT:
				constant = new LIntConstant((long)value, lexeme);
				break;
			case Tokens.USINT:
				constant = new USIntConstant((byte)value, lexeme);
				break;
			case Tokens.UINT:
				constant = new UIntConstant((ushort)value, lexeme);
				break;
			case Tokens.UDINT:
				constant = new UDIntConstant((uint)value, lexeme);
				break;
			case Tokens.ULINT:
				constant = new ULIntConstant((ulong)value, lexeme);
				break;
			case Tokens.BOOL:
				constant = new BoolConstant(value != 0, lexeme);
				break;
			case Tokens.BYTE:
				constant = new ByteConstant((byte)value, lexeme);
				break;
			case Tokens.WORD:
				constant = new WordConstant((ushort)value, lexeme);
				break;
			case Tokens.DWORD:
				constant = new DWordConstant((uint)value, lexeme);
				break;
			case Tokens.LWORD:
				constant = new LWordConstant((ulong)value, lexeme);
				break;
			case Tokens.IDENT:
			{
				STLangSymbol symbol;
				string typeName = token.TypeName;
				if (! this.symbolTable.Lookup(typeName, out symbol, location))
				{
					this.report.SemanticError(1, typeName, location);
					return MakeConstant(value);
				}
				else if (! (symbol is TypeNameSymbol))
				{
					this.report.SemanticError(24, typeName, location);
					return MakeConstant(value);
				}
				else {
					TypeNode dataType = symbol.DataType;
					lexeme = dataType.Name + "#" + value;
					keyValue = lexeme.RemoveChar('_').ToUpper();
					if (constantTable.ContainsKey(keyValue))
						return (Expression)constantTable[keyValue];
					else if (dataType == TypeNode.SInt)
						constant = new SIntConstant((sbyte)value, lexeme);
					else if (dataType == TypeNode.Int)
						constant = new IntConstant((short)value, lexeme);
					else if (dataType == TypeNode.DInt)
						constant = new DIntConstant((int)value, lexeme);
					else if (dataType == TypeNode.LInt)
						constant = new LIntConstant((long)value, lexeme);
					else if (dataType == TypeNode.USInt)
						constant = new USIntConstant((byte)value, lexeme);
					else if (dataType == TypeNode.UInt)
						constant = new UIntConstant((ushort)value, lexeme);
					else if (dataType == TypeNode.UDInt)
						constant = new UDIntConstant((uint)value, lexeme);
					else if (dataType == TypeNode.ULInt)
						constant = new ULIntConstant((ulong)value, lexeme);
					else if (dataType == TypeNode.Bool)
						constant = new BoolConstant(value != 0.0, lexeme);
					else if (dataType == TypeNode.Byte)
						constant = new ByteConstant((byte)value, lexeme);
					else if (dataType == TypeNode.Word)
						constant = new WordConstant((ushort)value, lexeme);
					else if (dataType == TypeNode.DWord)
						constant = new DWordConstant((uint)value, lexeme);
					else if (dataType == TypeNode.LWord)
						constant = new LWordConstant((ulong)value, lexeme);
					else if (dataType == TypeNode.Real)
						constant = new RealConstant((float)value, lexeme);
					else if (dataType == TypeNode.LReal)
						constant = new LRealConstant(value, lexeme);
					else {
						this.report.SemanticError(114, value, dataType.Name, location);
						return Expression.Error;
					}
				}
			}
			break;
			default:
				this.report.SyntaxError(111, token.TypeName, location);
				return Expression.Error;
			}
			if (constant != null)
			{
				TypeNode dataType = constant.DataType;
				constantTable[keyValue] = constant;
				return constant;
			}
		}
		return Expression.Error;
	}

	private Expression MakeConstant(TokenTypedEnum enumConst, LexLocation location)
	{
		if (enumConst.TypeToken != Tokens.IDENT || enumConst.ValueToken != Tokens.IDENT)
			return Expression.Error;
		else {
			STLangSymbol symbol;
			string qualifiedName = enumConst.ToString();
			string keyValue = qualifiedName.ToUpper();
			if (constantTable.ContainsKey(keyValue))
				return (Expression)constantTable[keyValue];
			else if (this.symbolTable.Lookup(qualifiedName, out symbol, location))
			{
				Expression constant = symbol.MakeSyntaxTreeNode(location);
				constantTable[keyValue] = constant;
				return constant;
			}
			else {
				STLangSymbol enumSymbol;
				string enumTypeName = enumConst.TypeName;
				string enumValue = enumConst.Value;
				this.report.SemanticError(0, qualifiedName, location);
				if (! this.symbolTable.Lookup(enumTypeName, out symbol, location))
					this.report.SemanticError(0, enumTypeName, location);
				else if (! (symbol is TypeNameSymbol))
					this.report.SemanticError(24, enumTypeName, location);
				else if (! symbol.DataType.IsEnumeratedType)
					this.report.SemanticError(-7, enumTypeName, location);
				if (! this.symbolTable.Lookup(enumValue, out enumSymbol, location))
					this.report.SemanticError(0, enumValue, location);
				else if (! (enumSymbol is EnumSymbol))
					this.report.SemanticError(-8, enumValue, location);
				return Expression.Error;
			}
		}
	}

	private SubRange MakeEnumSubRange(Expression lower, Expression upper, LexLocation location)
	{
		int lowerBound = Convert.ToInt32(lower.Evaluate());
		int upperBound = Convert.ToInt32(upper.Evaluate());
		if (lowerBound > upperBound)
		{
			string interval = lower + ".." + upper;
			this.report.SemanticError(16, interval, location);
			int tmp = lowerBound;
			lowerBound = upperBound;
			upperBound = tmp;
		}
		return new EnumSubrange((ushort)lowerBound, (ushort)upperBound, lower.DataType);
	}

	private SubRange MakeIntSubRange(Expression lower, Expression upper, LexLocation location)
	{
		long lowerBound = Convert.ToInt64(lower.Evaluate());
		long upperBound = Convert.ToInt64(upper.Evaluate());
		if (lowerBound > upperBound)
		{
			string interval = lower.ToString() + ".." + upper.ToString();
			this.report.SemanticError(16, interval, location);
			long tmp = lowerBound;
			lowerBound = upperBound;
			upperBound = tmp;
		}
		TypeNode dataType = this.LargestDataType(lower, upper);
		return new IntSubrange(lower, upper, dataType);
	}

	private SubRange MakeUIntSubRange(Expression lower, Expression upper, LexLocation location)
	{
		ulong lowerBound = Convert.ToUInt64(lower.Evaluate());
		ulong upperBound = Convert.ToUInt64(upper.Evaluate());
		if (lowerBound > upperBound)
		{
			string interval = lower.ToString() + ".." + upper.ToString();
			this.report.SemanticError(16, interval, location);
			ulong tmp = lowerBound;
			lowerBound = upperBound;
			upperBound = tmp;
		}
		TypeNode dataType = this.LargestDataType(lower, upper);
		return new UIntSubrange(lower, upper, dataType);
	}

	private SubRange MakeBitStringSubRange(Expression lower, Expression upper, LexLocation location)
	{
		ulong lowerBound = Convert.ToUInt64(lower.Evaluate());
		ulong upperBound = Convert.ToUInt64(upper.Evaluate());
		if (lowerBound > upperBound)
		{
			string interval = lower.ToString() + ".." + upper.ToString();
			this.report.SemanticError(16, interval, location);
			ulong tmp = lowerBound;
			lowerBound = upperBound;
			upperBound = tmp;
		}
		TypeNode dataType = this.LargestDataType(lower, upper);
		return new BitStringSubrange(lower, upper, dataType);
	}

private SubRange MakeSubrange(Expression lower, Expression upper, LexLocation loc1, LexLocation loc2)
{
		if (lower == null || upper == null)
			return SubRange.Error;
		else {
			TypeNode lowerDataType = lower.DataType;
			TypeNode upperDataType = upper.DataType;
		
			if (lowerDataType == TypeNode.Error || upperDataType == TypeNode.Error)
				return SubRange.Error;
			else if (! lower.IsConstant)
			{
				this.report.SemanticError(95, lower.ToString(), loc1);
				if (! upper.IsConstant)
					this.report.SemanticError(95, upper.ToString(), loc2);
				if (! lowerDataType.IsOrdinalType)
					this.report.SemanticError(-1, lower.ToString(), loc1);
				if (! upperDataType.IsOrdinalType)
					this.report.SemanticError(-1, upper.ToString(), loc2);
				return SubRange.Error;
			}
			else if (! upper.IsConstant)
			{
				this.report.SemanticError(95, upper.ToString(), loc2);
				if (! lowerDataType.IsOrdinalType)
					this.report.SemanticError(-1, lower.ToString(), loc1);
				if (! upperDataType.IsOrdinalType)
					this.report.SemanticError(-1, upper.ToString(), loc2);
				return SubRange.Error;
			}
			else if (! lowerDataType.IsOrdinalType)
			{
				this.report.SemanticError(-1, lower.ToString(), loc1);
				if (! upperDataType.IsOrdinalType)
					this.report.SemanticError(-1, upper.ToString(), loc2);
				return SubRange.Error;
			}
			else if (! upperDataType.IsOrdinalType)
			{
				this.report.SemanticError(-1, upper.ToString(), loc2);
				return SubRange.Error;
			}
			else if (lowerDataType.IsSignedIntType)
			{
				if (upperDataType.IsSignedIntType)
					return this.MakeIntSubRange(lower, upper, loc1);
			}
			else if (lowerDataType.IsUnsignedIntType)
			{
				if (upperDataType.IsUnsignedIntType)
					return this.MakeUIntSubRange(lower, upper, loc1);
			}
			else if (lowerDataType.IsBitStringType)
			{
				if (upperDataType.IsBitStringType)
					return this.MakeBitStringSubRange(lower, upper, loc1);
			}
			else if (lowerDataType.BaseType != upperDataType.BaseType)
			{
				string subRange = lower + ".." + upper;
				this.report.SemanticError(94, subRange, loc1);
				return SubRange.Error;
			}
			else if (lowerDataType.IsEnumeratedType)
				return this.MakeEnumSubRange(lower, upper, loc1);
		}
		string interval = lower + ".." + upper;
		this.report.SemanticError(94, interval, loc1);
		return SubRange.Error;
	}

	private void SubrangeTypeStart(TypeNode subrangeType)
	{
		this.isSubrangeDecl = true;
		this.subrangeDataType = subrangeType;
	}

	private TypeNode MakeSubrangeType(TypeNode baseType, SubRange subrange, LexLocation loc)
	{
		TypeNode subRangeType;
		this.isSubrangeDecl = false;            
		this.subrangeDataType = TypeNode.Error;
		string typeID = baseType.TypeID + "(" + subrange + ")";
		if (TypeNode.LookUpType(typeID, out subRangeType))
			return subRangeType;
		else {
			string typeName = baseType.Name + "(" + subrange + ")";
			return baseType.MakeSubrange(typeName, subrange, loc);
		}
	}	

	private List<SubRange> MakeSubRangeList(SubRange subrange, LexLocation location)
	{
		if (subrange.DataType.IsSignedIntType)
			return new List<SubRange>{subrange};
		else {
			this.report.SemanticError(15, subrange.ToString(), location);
			return new List<SubRange>{new IntSubrange(0, 100, TypeNode.Int)};
		}
	}

	private List<SubRange> AddSubRange(List<SubRange> subRangeList, SubRange subrange, LexLocation location)
	{
		if (subRangeList.Count == STLangParameters.MAX_ARRAY_SUBSCRIPTS)
			this.report.SemanticError(50, location);
		if (subrange.DataType.IsIntegerType)
			subRangeList.Add(subrange);
		else {
			if (subrange.DataType != TypeNode.Error)
				this.report.SemanticError(15, subrange.ToString(), location);
			subRangeList.Add(new IntSubrange(0, 100, TypeNode.Int));
		}
		return subRangeList;
	}

	private List<string> MakeEnumIdentList(string enumIdent, LexLocation location)
	{
		List<string> enumIdentList = new List<string>();
		if (enumIdent.Length > 0)
		{
			if (this.isTypeDecl)
			{
				if (this.symbolTable.IsValidTypedEnumSymbol(enumIdent, location))
					enumIdentList.Add(enumIdent);
			}
			else if (this.symbolTable.IsValidUserDefinedSymbol(enumIdent, location))
				enumIdentList.Add(enumIdent);
		}
		return enumIdentList;
	}

	private List<string> AddToEnumIdentList(List<string> enumIdentList, string enumIdent, LexLocation location)
	{
		if (enumIdent.Length == 0)
			return enumIdentList;
		else if (enumIdentList == null)
			return this.MakeEnumIdentList(enumIdent, location);
		else if (!enumIdentList.IsUnique(enumIdent))
			this.report.SemanticError(1, enumIdent, location);
		else if (this.isTypeDecl)
		{
			if (this.symbolTable.IsValidTypedEnumSymbol(enumIdent, location))
				enumIdentList.Add(enumIdent);
		}
		else if (this.symbolTable.IsValidUserDefinedSymbol(enumIdent, location))
			enumIdentList.Add(enumIdent);
		return enumIdentList;
	}

	private EnumeratedType MakeEnumeratedType(List<string> identList)
	{
		EnumeratedType enumType = new EnumeratedType(identList);
		if (this.isTypeDecl)
			this.symbolTable.InstallEnumeratedConstants(enumType, this.derivedTypeName);
		else
			this.symbolTable.InstallEnumeratedConstants(enumType);
		//
		// Install selection functions SEL and MUX for this enumerated type in the symboltable.
		//
		STLangSymbolTable.InstallStandardFunction("SEL", enumType, StandardLibraryFunction.SELECT, TypeNode.Bool, enumType, enumType);
		STLangSymbolTable.InstallExtensibleFunction("MUX", enumType, StandardLibraryFunction.MUX, TypeNode.AnyInt, enumType, enumType);
		return enumType;
	}

	private List<object> MakeArrayInitList(object initElem)
	{
		List<object> initList = new List<object>();
		if (initElem != null)
		{
			if (initElem is List<object>)
				initList.AddRange((List<object>)initElem);
			else
				initList.Add(initElem);
		}
		return initList;
	}

	private void AddArrayInitElem(List<object> initList, object initElem)
	{
		if (initElem != null)
		{
			if (initElem is List<object>)
				initList.AddRange((List<object>)initElem);
			else
				initList.Add(initElem);
		}
	}

	private List<object> MakeStructInitList(object initElem)
	{
		List<object> initList = new List<object>();
		if (initElem != null)
		{
			if (initElem is List<object>)
				initList.AddRange((List<object>)initElem);
			else
				initList.Add(initElem);
		}
		return initList;
	}

	private void AddStructInitElem(List<object> initList, object initElem)
	{
		if (initElem != null)
		{
			if (initElem is List<object>)
				initList.AddRange((List<object>)initElem);
			else
				initList.Add(initElem);
		}
	}

	private string MakeTypeName(List<SubRange> subRanges, TypeNode elemDataType)
	{
		string intervalList = "";

		foreach (SubRange subrange in subRanges)
		{
			if (intervalList.Length > 0)
				intervalList += ",";
			intervalList += subrange;
		}
		return "ARRAY [" + intervalList + "] OF " + elemDataType.Name;
	}

	private List<SubRange> GetSubRanges(ArrayType array)
	{
		int lower,upper;
		TypeNode elementType = array;
		List<SubRange> subRanges = new List<SubRange>();
		
		while (elementType.IsArrayType)
	    {
			array = (ArrayType)elementType;
			lower = array.LowerBound;
			upper = array.UpperBound;
			elementType = array.ElementType;
			subRanges.Add(new IntSubrange(lower, upper, TypeNode.DInt));
		}
		while (elementType.IsArrayType);
		return subRanges;
	}

	private TypeNode MakeArrayType(List<SubRange> subRanges, TypeNode elemDataType, LexLocation loc1, LexLocation loc2) 
	{
		if (elemDataType.IsFunctionBlockType)
		{
			//
			// Error. IEC 61131-3 does not allow arrays of function blocks.
			//
			this.report.SemanticError(164, loc2);
			return TypeNode.Error;
		}
		else {
			string typeID = "";
			TypeNode arrayDataType;
			foreach (SubRange subrange in subRanges)
			{
				typeID +=  "[" + subrange + "]";
			} 
			typeID += elemDataType.TypeID;
			if (TypeNode.LookUpType(typeID, out arrayDataType))
				return arrayDataType;
			else {
				Expression defaultValue = elemDataType.DefaultValue;
				return this.MakeArrayType(subRanges, elemDataType, defaultValue, loc1);
			}
		}
	}

	private TypeNode MakeArrayType(List<SubRange> subranges, TypeNode basicElementType, Expression initializer, LexLocation loc) 
	{
		if (subranges.Count == 0)
			return basicElementType;
		else {
			SubRange subrange = subranges[0];
			if (! (subrange is IntSubrange))
				return TypeNode.Error;
			else {
				TypeNode arrayDataType;
				TypeNode elementType    = this.MakeArrayType(subranges.Succ(), basicElementType, initializer, loc);
				string typeID           = "[" + subrange + "]" + elementType.TypeID;
			    IntSubrange intSubrange = (IntSubrange)subrange;
				long lower              = intSubrange.LowerBound;
				long upper              = intSubrange.UpperBound;
				string typeName         = this.MakeTypeName(subranges, basicElementType);
				long elementCount       = (upper - lower + 1)*(elementType.Size / basicElementType.Size);
				long byteCount          = elementCount * basicElementType.Size;
				if (byteCount > STLangParameters.MAX_ARRAY_SIZE)
					this.report.SemanticError(113, loc);
				if (TypeNode.LookUpType(typeID, out arrayDataType))
					return arrayDataType;
				else if (basicElementType.IsElementaryType || basicElementType.IsTextType)
				{
					Expression size = MakeIntConstant(byteCount);
					return new ArrayType(typeName, (int)lower, (int)upper, (uint)byteCount, size, elementType, basicElementType, initializer, typeID);
				}
				else if (basicElementType.IsArrayType)
				{
					List<SubRange> subranges2 = new List<SubRange>();
					ArrayType array = (ArrayType)basicElementType.BaseType;
					subranges2.AddRange(subranges);
					subranges2.AddRange(this.GetSubRanges(array));
					return this.MakeArrayType(subranges2, array.BasicElementType, initializer, loc);
				}
				else if (basicElementType.IsStructType)
				{
					string key;
					ArrayType array;
					StructType structure = (StructType)basicElementType.BaseType;
					FieldSymbol field = structure.FirstField;
					Dictionary<string, InitializerList> flattenedInitLists;
					flattenedInitLists = new Dictionary<string, InitializerList>();
					while (field != null)
					{
						key = field.Name.ToUpper();
						array = (ArrayType)this.MakeFlattenedArrayType(elementCount, field.DataType, field.InitialValue);
						flattenedInitLists.Add(key, (InitializerList)array.DefaultValue);
						field = field.Next;
					}
					Expression size = MakeIntConstant(byteCount);
					return new ArrayType(typeName, (int)lower, (int)upper, (uint)byteCount, size, elementType, basicElementType, flattenedInitLists, typeID);
				}
				string msg = "Illegal array element type: " + basicElementType.Name;
				throw new STLangCompilerError(msg);
			}
		}
	}

	private TypeNode MakeFlattenedArrayType(long elementCount, TypeNode elementType, Expression initializer)
	{
		if (elementType.IsElementaryType || elementType.IsTextType)
		{
			string typeID;
			TypeNode arrayDataType;
			int upper = (int)elementCount - 1;

			typeID = "[0.." + upper + "]" + elementType.TypeID;
			if (TypeNode.LookUpType(typeID, out arrayDataType))
				return arrayDataType;
			else {
				long byteCount = elementCount * elementType.Size;
				Expression size = MakeIntConstant(byteCount);
				string typeName = "ARRAY [0.." + upper + "] OF " + elementType.Name;
				return new ArrayType(typeName, 0, upper, (uint)byteCount, size, elementType, elementType, initializer, typeID);
			}
		}
		else if (elementType.IsArrayType)
		{
			ArrayType array2 = (ArrayType)elementType.BaseType;
			TypeNode elementType2 = array2.BasicElementType;
			long elementCount2 = elementCount*(array2.Size/elementType2.Size);
			return this.MakeFlattenedArrayType(elementCount2, elementType2, initializer);
		}
		else if (elementType.IsStructType)
		{
			TypeNode arrayDataType;
			int upper = (int)elementCount - 1;
			string typeID = "[0.." + upper + "]" + elementType.TypeID;
			
			if (TypeNode.LookUpType(typeID, out arrayDataType))
				return arrayDataType;
			else {
				string key;
				ArrayType array;
				StructType structure = (StructType)elementType.BaseType;
				FieldSymbol field = structure.FirstField;
				Dictionary<string, InitializerList> flattenedInitLists;
				flattenedInitLists = new Dictionary<string, InitializerList>();
				while (field != null)
				{
					key = field.Name.ToUpper();
					array = (ArrayType)this.MakeFlattenedArrayType(elementCount, field.DataType, field.InitialValue);
					flattenedInitLists.Add(key, (InitializerList)array.DefaultValue);
					field = field.Next;
				}
				long byteCount = elementCount * elementType.Size;
				Expression size = MakeIntConstant(byteCount);
				string typeName = "ARRAY [0.." + upper + "] OF " + elementType.Name;
				return new ArrayType(typeName, 0, upper, (uint)byteCount, size, elementType, elementType, flattenedInitLists, typeID);
			}
		}
		else
		{
			Expression size = MakeIntConstant(0);
			string typeID = "[0..1]" + elementType.TypeID;
			string typeName = "ARRAY [0..1] OF " + elementType.Name;
			return new ArrayType(typeName, 0, 1, 0, size, elementType, elementType, initializer, typeID);
		}
	}

	private string MakeTypeName(StructDeclaration structure)
	{
		string typeName = string.Empty;
		if (structure.MemberCount < 4)
		{
			foreach (StructMemberDeclaration member in structure.Members)
			{
				if (typeName.Length == 0)
					typeName = "STRUCT (" + member.Name + " : " + member.DataType.Name;
				else 
					typeName += "; " + member.Name + " : " + member.DataType.Name;
			}
		}
		else {
			StructMemberDeclaration member = structure.Members.ElementAt(0);
			typeName = "STRUCT (" + member.Name + " : " + member.DataType.Name;
            member = structure.Members.ElementAt(1);
			typeName += "; " + member.Name + " : " + member.DataType.Name;
            member = structure.Members.ElementAt(structure.MemberCount - 1);
			typeName += "; ... ; " + member.Name + " : " + member.DataType.Name;
		}
		typeName += ")";
		return typeName;
	}

	private TypeNode MakeStructDataType(StructDeclaration structure)
	{
		this.structNestingDepth--;
		if (structure == null || structure.MemberCount == 0)
			return TypeNode.Error;
		else {
			TypeNode structType;
			string typeID = string.Empty;
			
			foreach (StructMemberDeclaration member in structure.Members)
			{
				if (typeID.Length > 0)
					typeID += ",";
				typeID += member.Name.ToUpper() + ":" + member.DataType.TypeID;
			}
			typeID = "(" + typeID + ")";
			if (TypeNode.LookUpType(typeID, out structType))
				return structType;
			else {
				string key;
				uint byteCount = 0;
				FieldSymbol field = null;
				FieldSymbol prevField = null;
				FieldSymbol firstField = null;
				bool isContiguouslyStored = true;
				Dictionary<string, FieldSymbol> members;
				
				members = new Dictionary<string, FieldSymbol>();
				foreach (StructMemberDeclaration member in structure.Members)
				{
					prevField = field;
					field = new FieldSymbol(member.Name, member.DataType, member.InitValue);
					key = member.Name.ToUpper();
					members.Add(key, field);
					if (prevField == null)
                        firstField = field;
					else {
                        prevField.Next = field;
						if (prevField.DataType != field.DataType)
							isContiguouslyStored = false;
					}
					byteCount += member.DataType.Size;
				}
				string typeName = this.MakeTypeName(structure);
				Expression size = MakeIntConstant((long)byteCount);
				return new StructType(members, firstField, byteCount, size, isContiguouslyStored, typeName, typeID);
			}
		}
	}

	private InitializerList ExpandInitializerSequence(TokenInt tokenInt, InitializerList initSequence, LexLocation loc)
	{
		int repetitionFactor = (int)tokenInt.Value;
		if (repetitionFactor == 0)
		{
			this.report.SemanticError(89, loc);
			return initSequence; 
		}
		else if (initSequence != null)
			return initSequence.Expand(repetitionFactor, this.report);
		else {
			TypeNode dataType = this.attributeStack.Top;
			Expression defaultValue = dataType.DefaultValue;
			initSequence = new InitializerSequence(dataType);
			for (int i = repetitionFactor; i > 0; i--)
			{
				initSequence.Add(defaultValue, loc);
			}
			return initSequence;
		}
	}

	private TypeNode GetDerivedType(string typeName, LexLocation location)
	{
		STLangSymbol symbol;

		if (! this.symbolTable.Lookup(typeName, out symbol, location))
		{
			// Error: Undefined identifier

			this.report.SemanticError(151, typeName, location);
			return TypeNode.Error;
		}
		else if (! (symbol.IsDerivedType || symbol.IsFunctionBlock))
		{
			// Error: identifier is not a type name or function block.

			this.report.SemanticError(24, typeName, symbol.TypeName, location);
			return TypeNode.Error;
		}
		else if (symbol.IsFunctionBlock)
		{
			// Check that structure members are not function blocks.

			if (this.structNestingDepth > 0)
				this.report.SemanticError(165, location);
			if (this.isFunctionDecl)
			{
				// Check that the function block can be used in functions. 
				// Function blocks such as timers, counters or edge detectors
				// are illegal because they contain data that are declared 
				// retentive or R_EDGE/F_EDGE.

				StandardFunctionBlockSymbol functionBlockSymbol;
				functionBlockSymbol = (StandardFunctionBlockSymbol)symbol;
				if (functionBlockSymbol.HasRetentiveData) 
					this.report.SemanticError(162, typeName, location);
				if (functionBlockSymbol.HasRFEdgeDetection)
					this.report.SemanticError(163, typeName, location);
			}
		}
		return symbol.DataType;
	}

	private TypeNode MakeStringType(Expression size, LexLocation location)
	{
		if (size == null)
			return TypeNode.String;
		else if (! size.IsConstant && location != null)
		{
			this.report.SemanticError(22, location);
			if (! size.DataType.IsSignedIntType)
				this.report.SemanticError(-5, location);
			return TypeNode.String;
		}
		else if (! size.DataType.IsIntegerType)
		{
			this.report.SemanticError(-5, location);
			return TypeNode.String;
		}
		else {
			int length = Convert.ToInt32(size.Evaluate());
			if (length < 1)
			{
				this.report.SemanticError(22, location);
				return TypeNode.String;
			}
			else if (length > STLangParameters.MAX_STRING_LENGTH)
			{
				this.report.SemanticError(-6, length, location);
				return TypeNode.String;
			}
			else {
				TypeNode stringType;
				string baseTypeID = TypeNode.String.TypeID;
				string typeID = baseTypeID + "[" + length + "]";
				if (TypeNode.LookUpType(typeID, out stringType))
					return stringType;
				else {
					string typeName = "STRING[" + length + "]";
					return new StringType(typeName, length + 1, typeID);
				}
			}
		}
	}

	private TypeNode MakeWStringType(Expression size, LexLocation location)
	{
		if (size == null)
			return TypeNode.WString;
		else if (! size.IsConstant && location != null)
		{
			this.report.SemanticError(22, location);
			if (! size.DataType.IsIntegerType)
				this.report.SemanticError(-5, location);
			return TypeNode.WString;
		}
		else if (! size.DataType.IsIntegerType)
		{
			this.report.SemanticError(-5, location);
			return TypeNode.WString;
		}
		else {
			int length = Convert.ToInt32(size.Evaluate());
			if (length < 1)
			{
				this.report.SemanticError(22, location);
				return TypeNode.WString;
			}
			else if (length > STLangParameters.MAX_WSTRING_LENGTH)
			{
				this.report.SemanticError(-6, length, location);
				return TypeNode.WString;
			}
			else {
				TypeNode stringType;
				string baseTypeID = TypeNode.String.TypeID;
				string typeID = baseTypeID + "[" + length + "]";
				if (TypeNode.LookUpType(typeID, out stringType))
					return stringType;
				else {
					string typeName = "WSTRING[" + length + "]";
					return new WStringType(typeName, length + 1, typeID);
				}
			}
		}
	}

	private Expression MakeTODConstant(TimeSpan timeOfDay)
	{
		if (timeOfDay < TimeSpan.Zero)
			timeOfDay += new TimeSpan(0, 23, 59, 59, 999);
		if (timeOfDay.Days != 0)
			timeOfDay -= new TimeSpan(timeOfDay.Days, 0, 0, 0);
		string stringValue = timeOfDay.ToString();
		if (constantTable.ContainsKey(stringValue))
			return (Expression)constantTable[stringValue];
		else
		{
			Expression expr = new TimeOfDayConstant(timeOfDay);
			constantTable[stringValue] = expr;
			return expr;
		}
	}

	private Expression MakeConstant(TokenTOD token)
	{
		string stringValue = token.Value.ToString();
		if (constantTable.ContainsKey(stringValue))
			return (Expression)constantTable[stringValue];
		else
		{
			Expression expr = new TimeOfDayConstant(token);
			constantTable[stringValue] = expr;
			return expr;
		}
	}

	private Expression MakeConstant(TimeSpan time)
	{
		string stringValue = time.ToString();
		if (constantTable.ContainsKey(stringValue))
			return (Expression)constantTable[stringValue];
		else
		{
			Expression expr = new TimeConstant(time);
			constantTable[stringValue] = expr;
			return expr;
		}
	}

	private Expression MakeConstant(TokenTime token)
	{
		string stringValue = token.Value.ToString();
		if (constantTable.ContainsKey(stringValue))
			return (Expression)constantTable[stringValue];
		else
		{
			Expression expr = new TimeConstant(token);
			constantTable[stringValue] = expr;
			return expr;
		}
	}

	public static Expression MakeConstant(double value)
	{
		string stringValue = "LREAL#" + value.GetBinaryString();
		if (constantTable.ContainsKey(stringValue))
			return (Expression)constantTable[stringValue];
		else
		{
			Expression expr = new LRealConstant(value);
			constantTable[stringValue] = expr;
			return expr;
		}
	}

	public static Expression MakeConstant(float value)
	{
		string stringValue = "REAL#" + value.GetBinaryString();
		if (constantTable.ContainsKey(stringValue))
			return (Expression)constantTable[stringValue];
		else
		{
			Expression expr = new RealConstant(value);
			constantTable[stringValue] = expr;
			return expr;
		}
	}

	private Expression MakeConstant(TokenDouble token)
	{
		string stringValue = "LREAL#" + token.Value.GetBinaryString();
		if (constantTable.ContainsKey(stringValue))
			return (Expression)constantTable[stringValue];
		else
		{
			Expression expr = new LRealConstant(token);
			constantTable[stringValue] = expr;
			return expr;
		}
	}

	private Expression MakeConstant(TokenInt token)
	{
		string stringValue = token.Value.ToString();
		if (constantTable.ContainsKey(stringValue))
			return (Expression)constantTable[stringValue];
		else
		{
			Expression expr = this.MakeIntConstant(token);
			constantTable[stringValue] = expr;
			return expr;
		}
	}

	private Expression MakeConstant(TokenDate token)
	{
		string stringValue = token.Value.ToString();
		if (constantTable.ContainsKey(stringValue))
			return (Expression)constantTable[stringValue];
		else
		{
			Expression expr = new DateConstant(token);
			constantTable[stringValue] = expr;
			return expr;
		}
	}

	private Expression MakeConstant(DateTime dateTime)
	{
		string stringValue = dateTime.ToString();
		if (constantTable.ContainsKey(stringValue))
			return (Expression)constantTable[stringValue];
		else
		{
			Expression expr = new DateTimeConstant(dateTime);
			constantTable[stringValue] = expr;
			return expr;
		}
	}

	private Expression MakeConstant(TokenDateTime token)
	{
		string stringValue = token.Value.ToString();
		if (constantTable.ContainsKey(stringValue))
			return (Expression)constantTable[stringValue];
		else
		{
			Expression expr = new DateTimeConstant(token);
			constantTable[stringValue] = expr;
			return expr;
		}
	}

	private void PushFieldType(string field, LexLocation location)
	{
		this.attributeStack.PushFieldType(field, location);
	}

	private void CheckNestingDepth(LexLocation location)
	{
		this.structNestingDepth++;
		if (this.structNestingDepth == STLangParameters.MAX_STRUCT_NESTING_DEPTH + 1)
			this.report.SemanticError(115, location);
	}

	private SubrangeLabel CheckSubRangeTypes(TypeNode selectorDataType, SubRange subRange, LexLocation loc)
	{
		SubRange selectorSubrange = selectorDataType.GetSubrange();
		if (selectorSubrange.Contains(subRange))
			return new SubrangeLabel(subRange);
		else if (selectorSubrange.AreDisjoint(subRange))
		{
			this.report.SemanticError(-3, subRange.ToString(), selectorDataType.Name, loc);
			return new SubrangeLabel(subRange);
		}
		else {
			this.report.Warning(14, subRange.ToString(), loc);
			return new SubrangeLabel(subRange);
		}
	}

	private NumericLabel CheckSubRangeTypes(TypeNode selectorDataType, Expression caseLabel, LexLocation loc)
	{
		SubRange selectorSubrange = selectorDataType.GetSubrange();
		if (selectorSubrange.Contains(caseLabel))
			return new NumericLabel(caseLabel);
		else {
			this.report.SemanticError(108, caseLabel.ToString(), selectorDataType.Name, loc);
			return null;
		}
	}

	private Expression CheckCtrlExpression(LexLocation location, Expression expression = null)
	{
		if (expression == null)
		{
			this.Push(Expression.Error);
			expression = Expression.Error;
		}
		else if (expression is FunctionName)
		{
			this.Push(Expression.Error);
			expression = Expression.Error;
			this.report.SemanticError(148, expression.ToString(), location);
		}
		else {
			TypeNode ctrlExpressionType = expression.DataType;
			if (ctrlExpressionType == TypeNode.Error)
				this.Push(Expression.Error);
			else if (ctrlExpressionType.IsOrdinalType)
				this.Push(expression);
			else {
				this.report.SemanticError(147, location);
				this.Push(Expression.Error);
			}
		}
		this.PushCaseLabelList();
		this.CheckForEndOfCaseStatList();
		return expression;
	}

	private NumericLabel CheckCaseLabel(Expression caseLabel, LexLocation location)
	{
		if (caseLabel == null)
			return null;
		else if (caseLabel is FunctionName)
		{
			this.report.SemanticError(32, caseLabel.ToString(), location);
			return null;
		}
		else if (caseLabel.DataType == TypeNode.Error)
			return null;
		else if (! caseLabel.IsConstant)
		{
			this.report.SemanticError(32, caseLabel.ToString(), location);
			if (! caseLabel.DataType.IsOrdinalType)
				this.report.SemanticError(31, caseLabel.ToString(), location);
			return null;
		}
		else {
			TypeNode labelDataType = caseLabel.DataType;
			TypeNode selectorDataType = this.attributeStack.Top;	

			if (labelDataType == TypeNode.Error)
				return null;
			else if (! labelDataType.IsOrdinalType)
			{
				this.report.SemanticError(31, caseLabel.ToString(), location);
				return null;
			}
			else if (selectorDataType == TypeNode.Error)
				return new NumericLabel(caseLabel);
			else if (selectorDataType == labelDataType)
			{
				if (selectorDataType.IsSubrangeType)
				{
					if (selectorDataType.IsSignedIntType)
					{
						long numericValue = Convert.ToInt64(caseLabel.Evaluate());
						if (! selectorDataType.IsInRange(numericValue))
						{
							string strValue = caseLabel.ToString();
							string typeName = selectorDataType.Name;
							this.report.SemanticError(171, strValue, typeName, location);
						}
					}
					else {
						ulong numericValue = Convert.ToUInt64(caseLabel.Evaluate());
						if (! selectorDataType.IsInRange(numericValue))
						{
							string strValue = caseLabel.ToString();
							string typeName = selectorDataType.Name;
							this.report.SemanticError(171, strValue, typeName, location);
						}
					}
				}
				return new NumericLabel(caseLabel);
			}
			else if (selectorDataType.IsSignedIntType && labelDataType.IsSignedIntType)
			{
				if (selectorDataType.Size < labelDataType.Size)
					this.report.SemanticError(108, caseLabel.ToString(), selectorDataType.Name, location);
				else {
					// Convert case label type to selector type

					long value = Convert.ToInt64(caseLabel.Evaluate());
					if (selectorDataType == TypeNode.Int)
						caseLabel = new IntConstant((short)value);
					else if (selectorDataType == TypeNode.DInt)
						caseLabel = new DIntConstant((int)value);
					else
						caseLabel = new LIntConstant(value);
				}
				return new NumericLabel(caseLabel);
			}
			else if (selectorDataType.IsUnsignedIntType && labelDataType.IsUnsignedIntType)
			{
				if (selectorDataType.Size < labelDataType.Size)
					this.report.SemanticError(108, caseLabel.ToString(), selectorDataType.Name, location);
				else {
					// Convert case label type to selector type

					ulong value = Convert.ToUInt64(caseLabel.Evaluate());
					if (selectorDataType == TypeNode.UInt)
						caseLabel = new UIntConstant((ushort)value);
					else if (selectorDataType == TypeNode.DInt)
						caseLabel = new UDIntConstant((uint)value);
					else
						caseLabel = new ULIntConstant(value);
				}
				return new NumericLabel(caseLabel);
			}
			else if (selectorDataType.IsUnsignedIntType && labelDataType.IsSignedIntType)
			{
				if (selectorDataType.Size < labelDataType.Size)
					this.report.SemanticError(108, caseLabel.ToString(), selectorDataType.Name, location);
				else {
					// Convert case label type to selector type

					long value = Convert.ToInt64(caseLabel.Evaluate());
					if (value < 0)
						this.report.SemanticError(108, caseLabel.ToString(), selectorDataType.Name, location);
					else if (selectorDataType == TypeNode.UInt)
						caseLabel = new UIntConstant((ushort)value);
					else if (selectorDataType == TypeNode.DInt)
						caseLabel = new UDIntConstant((uint)value);
					else
						caseLabel = new ULIntConstant((ulong)value);
				}
				return new NumericLabel(caseLabel);
			}
			else if (selectorDataType.IsBitStringType && labelDataType.IsBitStringType)
			{
				if (selectorDataType.Size < labelDataType.Size)
					this.report.SemanticError(108, caseLabel.ToString(), selectorDataType.Name, location);
				else {
					// Convert case label type to selector type

					ulong value = Convert.ToUInt64(caseLabel.Evaluate());
					if (selectorDataType == TypeNode.Word)
						caseLabel = new WordConstant((ushort)value);
					else if (selectorDataType == TypeNode.DWord)
						caseLabel = new DWordConstant((uint)value);
					else
						caseLabel = new LWordConstant(value);
				}
				return new NumericLabel(caseLabel);
			}
			else if (selectorDataType.IsBitStringType && labelDataType.IsIntegerType)
			{
				if (selectorDataType.Size < labelDataType.Size)
					this.report.SemanticError(108, caseLabel.ToString(), selectorDataType.Name, location);
				else {
					// Convert case label type to selector type

					long value = Convert.ToInt64(caseLabel.Evaluate());
					if (value < 0)
						this.report.SemanticError(108, caseLabel.ToString(), selectorDataType.Name, location);
					else if (selectorDataType == TypeNode.Word)
						caseLabel = new UIntConstant((ushort)value);
					else if (selectorDataType == TypeNode.DWord)
						caseLabel = new DWordConstant((uint)value);
					else
						caseLabel = new LWordConstant((ulong)value);
				}
				return new NumericLabel(caseLabel);
			}
			this.report.SemanticError(86, caseLabel.ToString(), labelDataType.Name, selectorDataType.Name, location);
			return null;
		}
	}

	private SubrangeLabel CheckCaseLabel(SubRange subRange, LexLocation location)
	{
		if (subRange == null)
			return null;
		else {
			TypeNode selectorDataType = this.attributeStack.Top;
			TypeNode labelDataType = subRange.DataType;
			if (labelDataType == TypeNode.Error)
				return null;
			else if (! labelDataType.IsOrdinalType)
			{
				this.report.SemanticError(31, subRange.ToString(), location);
				return null;
			}
			else if (selectorDataType == TypeNode.Error)
				return new SubrangeLabel(subRange);
			else if (! selectorDataType.IsOrdinalType)
				return new SubrangeLabel(subRange);
			else if (selectorDataType == labelDataType)
			{
                SubRange selectorSubRange = selectorDataType.GetSubrange();
                if (selectorSubRange.AreDisjoint(subRange))
                {
					string subRangeStr = subRange.ToString();
                    string selDataTypeName = selectorDataType.Name;
                    this.report.SemanticError(172, subRangeStr, selDataTypeName, location);
                }
				else if (! selectorSubRange.Contains(subRange))
                {
                    string subRangeStr = subRange.ToString();
                    string selDataTypeName = selectorDataType.Name;
                    this.report.Warning(14, subRangeStr, selDataTypeName, location);
                }
                return new SubrangeLabel(subRange);
            }
			else if (selectorDataType.IsSignedIntType && (subRange is IntSubrange))
			{
				if (selectorDataType.Size == labelDataType.Size)
					return CheckSubRangeTypes(selectorDataType, subRange, location);
				else if (selectorDataType.Size < labelDataType.Size)
				{
					this.report.SemanticError(108, subRange.ToString(), selectorDataType.Name, location);
					return null;
				}
				else {
					// Convert subrange type to selector type
					IntSubrange intSubrange = (IntSubrange)subRange;
					long lowerBound = intSubrange.LowerBound;
					long upperBound = intSubrange.UpperBound;
					intSubrange     = new IntSubrange(lowerBound, upperBound, selectorDataType);
					return new SubrangeLabel(intSubrange);
				}
			}
			else if (selectorDataType.IsUnsignedIntType && (subRange is UIntSubrange))
			{
				if (selectorDataType.Size == labelDataType.Size)
					return CheckSubRangeTypes(selectorDataType, subRange, location);
				else if (selectorDataType.Size < labelDataType.Size)
				{
					this.report.SemanticError(108, subRange.ToString(), selectorDataType.Name, location);
					return null;
				}
				else {
				    // Convert subrange type to selector type
					UIntSubrange uintSubrange = (UIntSubrange)subRange;
					ulong lowerBound = uintSubrange.LowerBound;
					ulong upperBound = uintSubrange.UpperBound;
					uintSubrange     = new UIntSubrange(lowerBound, upperBound, selectorDataType);
					return new SubrangeLabel(uintSubrange);
				}
			}
			else if (selectorDataType.IsUnsignedIntType && labelDataType.IsSignedIntType)
			{
				if (selectorDataType.Size < labelDataType.Size)
				{
					this.report.SemanticError(108, subRange.ToString(), selectorDataType.Name, location);
					return null;
				}
				else {
				    IntSubrange intSubrange = (IntSubrange)subRange;
					long lowerBound = intSubrange.LowerBound;
					long upperBound = intSubrange.UpperBound;
					UIntSubrange uintSubrange = new UIntSubrange((ulong)lowerBound, (ulong)upperBound, selectorDataType); 
					if (selectorDataType.Size > labelDataType.Size)
						return new SubrangeLabel(uintSubrange);
					else
						return CheckSubRangeTypes(selectorDataType, uintSubrange, location);
				}
			}
			else if (selectorDataType.IsBitStringType && labelDataType.IsBitStringType)
			{
				if (selectorDataType.Size == labelDataType.Size)
					return CheckSubRangeTypes(selectorDataType, subRange, location);
				else if (selectorDataType.Size < labelDataType.Size)
				{
					this.report.SemanticError(108, subRange.ToString(), selectorDataType.Name, location);
					return null;
				}
				else {
					// Convert subrange type to selector type
					BitStringSubrange bitStringSubrange = (BitStringSubrange)subRange;
					ulong lowerBound  = bitStringSubrange.LowerBound;
					ulong upperBound  = bitStringSubrange.UpperBound;
					bitStringSubrange = new BitStringSubrange(lowerBound, upperBound, selectorDataType);
					return new SubrangeLabel(bitStringSubrange);
				}
			}
			else if (selectorDataType.IsBitStringType && (subRange is UIntSubrange))
			{
				if (selectorDataType.Size < labelDataType.Size)
				{
					this.report.SemanticError(108, subRange.ToString(), selectorDataType.Name, location);
					return null;
				}
				else {
					UIntSubrange uintSubrange = (UIntSubrange)subRange;
					ulong lowerBound = uintSubrange.LowerBound;
					ulong upperBound = uintSubrange.UpperBound;
					uintSubrange     = new UIntSubrange(lowerBound, upperBound, selectorDataType); 
					if (selectorDataType.Size > labelDataType.Size)
						return new SubrangeLabel(uintSubrange);
					else
						return CheckSubRangeTypes(selectorDataType, uintSubrange, location);
				}
			}
			else if (selectorDataType.IsEnumeratedType && labelDataType.IsEnumeratedType)
			{
				SubRange selectorSubrange = selectorDataType.GetSubrange();
				if (selectorSubrange.Contains(subRange))
					return new SubrangeLabel(subRange);
				else if (selectorSubrange.AreDisjoint(subRange))
				{
					this.report.SemanticError(-3, subRange.ToString(), selectorDataType.Name, location);
					return new SubrangeLabel(subRange);
				}
				else {
					this.report.Warning(14, subRange.ToString(), location);
					return new SubrangeLabel(subRange);
				}
			}
			else {
				this.report.SemanticError(86, selectorDataType.Name, subRange.ToString(), location);
				return null;
			}
		}
	}

	private void InstallDerivedType(string typeName, DataTypeSpec typeSpec, LexLocation location)
	{
		if (this.symbolTable.IsValidUserDefinedSymbol(typeName, location))
		{
			TypeNode baseType = typeSpec.DataType;
			Expression initialValue = typeSpec.InitialValue;
			this.symbolTable.InstallDerivedType(typeName, baseType, initialValue);
		}
		this.derivedTypeName = "";
	}

	private Expression MakeRealAddOp(Expression left, Expression right, LexLocation location)
	{
		if (left.IsConstant)
		{
			float leftValue = Convert.ToSingle(left.Evaluate());
			if (leftValue == 0.0f)
				return right;
			else if (right.IsConstant)
			{
				float rightValue = Convert.ToSingle(right.Evaluate());
				float sum = leftValue + rightValue;
					
				if (float.IsPositiveInfinity(sum) || float.IsNegativeInfinity(sum))
				{
					sum = float.NaN;
					string text = left.ToString() + " + " + right.ToString();
					this.report.SemanticError(116, text, location);
				}
				return MakeConstant(sum);
			}
		}
		else if (right.IsConstant)
		{
			float rightValue = Convert.ToSingle(right.Evaluate());
			if (rightValue == 0.0f)
				return left;
			else if (rightValue < 0.0f)
			{
				right = MakeConstant(-rightValue);
				return new RealSubOperator(left, right);
			}
		}
		else if (right is RealUnaryMinusOperator)
		{
			right = ((UnaryOperator)right).Operand;
			Expression expr = new RealSubOperator(left, right);
		}
		return new RealAddOperator(left, right);
	}

	private Expression MakeLRealAddOp(Expression left, Expression right, LexLocation location)
	{
		if (left.IsConstant)
		{
			double leftValue = Convert.ToDouble(left.Evaluate());
			if (Math.Abs(leftValue) == 0.0d)
				return right;
			else if (right.IsConstant)
			{
				double rightValue = Convert.ToDouble(right.Evaluate());
				double sum = leftValue + rightValue;
					
				if (double.IsPositiveInfinity(sum) || double.IsNegativeInfinity(sum))
				{
					sum = double.NaN;
					string text = left.ToString() + " + " + right.ToString();
					this.report.SemanticError(116, text, location);
				}
				return MakeConstant(sum);
			}
		}
		else if (right.IsConstant)
		{
			double rightValue = Convert.ToDouble(right.Evaluate());
			if (Math.Abs(rightValue) == 0.0d)
				return left;
			else if (rightValue < 0.0d)
			{
				right = MakeConstant(-rightValue);
				return new RealSubOperator(left, right);
			}
		}
		else if (right is LRealUnaryMinusOperator)
		{
			UnaryOperator unaryMinus = (UnaryOperator)right;
			return new LRealSubOperator(left, unaryMinus.Operand);
		}
		return new LRealAddOperator(left, right);
	}

	private Expression MakeIntAddOp(Expression left, Expression right)
	{
		if (left.IsConstant)
		{
			long leftValue = Convert.ToInt64(left.Evaluate());
			if (leftValue == 0)
				return right;
			else if (right.IsConstant)
			{
				long rightValue = Convert.ToInt64(right.Evaluate());
				if (rightValue == 0)
					return left;
				else {
					long sum = leftValue + rightValue;
					return MakeIntConstant(sum);
				}
			}
		}
		else if (right.IsConstant)
		{
			long rightValue = Convert.ToInt64(right.Evaluate());
			if (rightValue == 0)
				return left;
		}
		else if (right is IntUnaryMinusOperator)
		{
			UnaryOperator unaryMinus = (UnaryOperator)right;
			return this.MakeIntSubOp(left, unaryMinus.Operand);
		}
		if (left.DataType == right.DataType)
			return new IntAddOperator(left, right, left.DataType);
		else if (left.DataType == TypeNode.LInt)
			return new IntAddOperator(left, this.MakeInt2LInt(right), TypeNode.LInt);
		else if (right.DataType == TypeNode.LInt)
			return new IntAddOperator(this.MakeInt2LInt(left), right, TypeNode.LInt);
		else {
			TypeNode resDataType = this.LargestDataType(left, right);
			return new IntAddOperator(left, right, resDataType);
		}	
	}

	private Expression MakeUIntAddop(Expression left, Expression right)
	{
		if (left.IsConstant)
		{
			ulong leftValue = Convert.ToUInt64(left.Evaluate());
			if (leftValue == 0)
				return right;
			else if (right.IsConstant)
			{
				ulong rightValue = Convert.ToUInt64(right.Evaluate());
				if (rightValue == 0)
					return left;
				else {
					ulong sum = leftValue + rightValue;
					return this.MakeIntConstant(sum);
				}
			}
		}
		else if (right.IsConstant)
		{
			ulong rightValue = Convert.ToUInt64(right.Evaluate());
			if (rightValue == 0)
				return left;
		}
		TypeNode resultDataType = this.LargestDataType(left, right);
		return new UIntAddOperator(left, right, resultDataType);		
	}

	private Expression MakeIntUIntAddop(Expression left, Expression right, LexLocation location)
	{
		TypeNode resultDataType;
		if (left.DataType.Size > right.DataType.Size)
			resultDataType = left.DataType;
		else if (right.DataType == TypeNode.USInt)
			resultDataType = TypeNode.Int;
		else if (right.DataType == TypeNode.UInt)
			resultDataType = TypeNode.DInt;
		else if (right.DataType == TypeNode.UDInt)
			resultDataType = TypeNode.LInt;
		else {
			this.report.SemanticError(14, "+", left.DataType.Name, right.DataType.Name, location);
			return Expression.Error;
		}
		if (left.IsConstant)
		{
			long leftValue = Convert.ToInt64(left.Evaluate());
			if (leftValue == 0)
				return right;
			else if (right.IsConstant)
			{
				uint rightValue = Convert.ToUInt32(right.Evaluate());
				long sum = leftValue + rightValue;
				return MakeIntConstant(sum);
			}
		}
		else if (right.IsConstant)
		{
			uint rightValue = Convert.ToUInt32(right.Evaluate());
			if (rightValue == 0)
				return left;
		}
		return new IntAddOperator(left, right, resultDataType);
	}

	private Expression MakeLeftShiftOp(Expression expression, int power)
	{
		if (expression == null)
			return Expression.Error;
		else if (! expression.IsConstant)
			return new LeftShiftOperator(expression, power);
		else if (expression.DataType.IsSignedIntType)
		{
			long intValue = Convert.ToInt64(expression.Evaluate());
			return MakeIntConstant(intValue << power);
		}
		else {
			ulong intValue = Convert.ToUInt64(expression.Evaluate());
			return this.MakeIntConstant(intValue << power);
		}
	}

	private Expression MakeReal2LReal(Expression expression)
	{
		if (expression == null)
			return Expression.Error;
		else if (expression.DataType != TypeNode.Real)
			throw new STLangCompilerError("MakeReal2LReal() expects Real argument: " + expression);
		else if (! expression.IsConstant)
			return new Real2LRealOperator(expression);
		else
		{
			double value = Convert.ToDouble(expression.Evaluate());
			return MakeConstant(value);
		}
	}

	private Expression MakeLReal2Real(Expression expression)
	{
		if (expression == null)
			return Expression.Error;
		else if (expression.DataType != TypeNode.LReal)
			throw new STLangCompilerError("MakeLReal2Real() expects LReal argument: " + expression);
		else if (! expression.IsConstant)
			return new LReal2RealOperator(expression);
		else
		{
			float value = Convert.ToSingle(expression.Evaluate());
			return MakeConstant(value);
		}
	} 

	private Expression MakeInt2Real(Expression expression)
	{
		if (expression == null)
			return Expression.Error;
		else if (! expression.DataType.IsIntegerType)
			throw new STLangCompilerError("MakeInt2Real() expects integer argument: " + expression);
		else if (expression.IsConstant)
		{
			float value = Convert.ToSingle(expression.Evaluate());
			return MakeConstant(value);
		}
		else if (expression.DataType.Size == TypeNode.LInt.Size)
			return new LInt2RealOperator(expression);
		else
			return new Int2RealOperator(expression);
	}

	private Expression MakeInt2LReal(Expression expression)
	{
		if (expression == null)
			return Expression.Error;
		else if (! expression.DataType.IsIntegerType)
			throw new STLangCompilerError("MakeInt2LReal() expects integer argument: " + expression);
		else if (expression.IsConstant)
		{
			double value = Convert.ToDouble(expression.Evaluate());
			return MakeConstant(value);
		}
		else if (expression.DataType.Size == TypeNode.LInt.Size)
			return new LInt2LRealOperator(expression);
		else
			return new Int2LRealOperator(expression);
	}

	private Expression MakeInt2LInt(Expression expression)
	{
		if (expression == null)
			return Expression.Error;
		else if (! expression.DataType.IsIntegerType)
			throw new STLangCompilerError("MakeInt2LInt() expects integer argument: " + expression);
		else if (expression.IsConstant)
		{
			long value = Convert.ToInt64(expression.Evaluate());
			return new LIntConstant(value);
		}
		else if (expression.DataType.Size <= TypeNode.DInt.Size)
			return new Int2LIntOperator(expression);
		else
			return expression;
	}

	private Expression MakeTTAddOp(Expression left, Expression right, LexLocation location)
	{
		if (left.IsConstant)
		{
			TimeSpan leftValue = (TimeSpan)left.Evaluate();
			if (leftValue == TimeSpan.Zero)
				return right;
			else if (right.IsConstant)
			{
				TimeSpan rightValue = (TimeSpan)right.Evaluate();
				if (rightValue == TimeSpan.Zero)
					return left;
				else {
					TimeSpan sum; 
					try {
						sum = leftValue.Add(rightValue);
					}
					catch (System.OverflowException)
					{
						sum = TimeSpan.Zero;
						string text = left.ToString() + " + " + right.ToString();
						this.report.SemanticError(117, text, location);
					}
					return this.MakeConstant(sum);
				}
			}
		}
		else if (right.IsConstant)
		{
			TimeSpan rightValue = (TimeSpan)right.Evaluate();
			if (rightValue == TimeSpan.Zero)
				return left;
		}
		return new TTAddOperator(left, right);
	}

	private Expression MakeDTTimeAddOp(Expression left, Expression right, LexLocation location)
	{
		if (left.IsConstant)
		{
			DateTime leftValue = Convert.ToDateTime(left.Evaluate());
			if (right.IsConstant)
			{
				TimeSpan rightValue = (TimeSpan)right.Evaluate();
				if (rightValue == TimeSpan.Zero)
					return left;
				else {
					DateTime sum; 
					try {
						sum = leftValue.Add(rightValue);
					}
					catch (System.ArgumentOutOfRangeException )
					{
						sum = DateTime.MinValue;
						string text = left.ToString() + " + " + right.ToString();
						this.report.SemanticError(117, text, location);
					}
					return this.MakeConstant(sum);
				}
			}
		}
		else if (right.IsConstant)
		{
			TimeSpan rightValue = (TimeSpan)right.Evaluate();
			if (rightValue == TimeSpan.Zero)
				return left;
		}
		return new DTTAddOperator(left, right);
	}

	private Expression MakeTTODAddOp(Expression left, Expression right, LexLocation location)
	{
		if (left.IsConstant)
		{
			TimeSpan leftValue = (TimeSpan)left.Evaluate();
			if (leftValue == TimeSpan.Zero)
				return right;
			else if (right.IsConstant)
			{
				TimeSpan timeOfDay; 
				TimeSpan rightValue = (TimeSpan)right.Evaluate();
				if (leftValue.Days != 0)
					leftValue -= new TimeSpan(leftValue.Days, 0, 0, 0);
				try {
					TimeSpan sum = leftValue.Add(rightValue);
					if (sum < TimeSpan.Zero)
						sum = sum.Add(new TimeSpan(0, 23, 59, 59, 999));
					int hours = sum.Hours;
					int minutes = sum.Minutes;
					int seconds = sum.Seconds;
					if (sum.Milliseconds == 0)
						timeOfDay = new TimeSpan(hours, minutes, seconds);
					else
						timeOfDay = new TimeSpan(hours, minutes, seconds, sum.Milliseconds);
				}
				catch (System.OverflowException)
				{
					timeOfDay = TimeSpan.Zero;
					string text = left.ToString() + " + " + right.ToString();
					this.report.SemanticError(117, text, location);
				}
				return this.MakeTODConstant(timeOfDay);
			}
		}
		else if (right.IsConstant)
		{
			TimeSpan rightValue = (TimeSpan)right.Evaluate();
			if (rightValue == TimeSpan.Zero)
				return left;
		}
		return new TTODAddOperator(left, right);
	}

	private Expression MakeIntSubOp(Expression left, Expression right)
	{
		if (left.IsConstant)
		{
			long leftValue = Convert.ToInt64(left.Evaluate());
			
			if (right.IsConstant)
			{
				long rightValue = Convert.ToInt64(right.Evaluate());
				long difference = leftValue - rightValue;
				return MakeIntConstant(difference);
			}
			else if (leftValue == 0)
			{
				if (right is IntUnaryMinusOperator)
					return ((UnaryOperator)right).Operand;
				else 
					return new IntUnaryMinusOperator(right);
			}
		}
		else if (right.IsConstant)
		{
			long rightValue = Convert.ToInt64(right.Evaluate());
			if (rightValue == 0)
				return left;
			else if (rightValue < 0)
			{
				right = MakeIntConstant(-rightValue);
				return this.MakeIntAddOp(left, right);
			}
		}
		else if (right is IntUnaryMinusOperator)
		{
			right = ((UnaryOperator)right).Operand;
			return this.MakeIntAddOp(left, right);
		}
		if (left.DataType == right.DataType)
			return new IntSubOperator(left, right, left.DataType);
		else if (left.DataType == TypeNode.LInt)
			return new IntSubOperator(left, this.MakeInt2LInt(right), TypeNode.LInt);
		else if (right.DataType == TypeNode.LInt)
			return new IntSubOperator(this.MakeInt2LInt(left), right, TypeNode.LInt);
		else {
			TypeNode resDataType = this.LargestDataType(left, right);
			return new IntSubOperator(left, right, resDataType);
		}	
	}

	private Expression MakeIntUIntSubOp(Expression left, Expression right, LexLocation location)
	{
		TypeNode resultDataType;
	
		if (left.IsConstant)
		{
			long leftValue = Convert.ToInt64(left.Evaluate());
			if (right.IsConstant)
			{
				uint rightValue = Convert.ToUInt32(right.Evaluate());
				long difference = leftValue - rightValue;
				return MakeIntConstant(difference);
			}
		}
		if (left.DataType.Size > right.DataType.Size)
			resultDataType = left.DataType;
		else if (right.DataType == TypeNode.USInt)
			resultDataType = TypeNode.Int;
		else if (right.DataType == TypeNode.UInt)
			resultDataType = TypeNode.DInt;
		else if (right.DataType == TypeNode.UDInt)
			resultDataType = TypeNode.LInt;
		else {
			this.report.SemanticError(14, "-", left.DataType.Name, right.DataType.Name, location);
			return Expression.Error;
		}
		 return new UIntAddOperator(left, right, resultDataType);
	}

	private Expression MakeUIntSubOp(Expression left, Expression right)
	{
		if (left.IsConstant)
		{
			ulong leftValue = Convert.ToUInt64(left.Evaluate());
			if (leftValue == 0)
				return right;
			else if (right.IsConstant)
			{
				ulong rightValue = Convert.ToUInt64(right.Evaluate());
				if (rightValue == 0)
					return left;
				else {
					ulong difference = leftValue - rightValue;
					return this.MakeIntConstant(difference);
				}
			}
		}
		else if (right.IsConstant)
		{
			ulong rightValue = Convert.ToUInt64(right.Evaluate());
			if (rightValue == 0)
				return left;
		}
		TypeNode resultDataType = this.LargestDataType(left, right);
		return new UIntSubOperator(left, right, resultDataType);		
	}

	private Expression MakeRealSubOp(Expression left, Expression right, LexLocation location)
	{
		if (left.IsConstant)
		{
			float leftValue = Convert.ToSingle(left.Evaluate());
			if (right.IsConstant)
			{
				float rightValue = Convert.ToSingle(right.Evaluate());
				float diff = leftValue - rightValue;
				if (float.IsPositiveInfinity(diff) || float.IsNegativeInfinity(diff))
				{
					diff = float.NaN;
					string text = left.ToString() + " - " + right.ToString();
					this.report.SemanticError(116, text, location);
				}
				return MakeConstant(diff);
			}
			else if (leftValue == 0.0f)
				return new RealUnaryMinusOperator(right);
		}
		else if (right.IsConstant)
		{
			float rightValue = Convert.ToSingle(right.Evaluate());
			if (rightValue == 0.0f)
				return left;
		}
		else if (left is RealUnaryMinusOperator)
		{
			if (right is RealUnaryMinusOperator)
			{
				right = ((UnaryOperator)right).Operand;
				return new RealSubOperator(right, left);
			}
			else {
				left = ((UnaryOperator)left).Operand;
				Expression expr = new RealAddOperator(left, right);
				return new RealUnaryMinusOperator(expr);
			}
		}
		else if (right is RealUnaryMinusOperator)
		{
			right = ((UnaryOperator)right).Operand;
			return new RealAddOperator(left, right);
		}
		return new RealSubOperator(left, right);
	}

	private Expression MakeLRealSubOp(Expression left, Expression right, LexLocation location)
	{
		if (left.IsConstant)
		{
			double leftValue = Convert.ToDouble(left.Evaluate());
			if (right.IsConstant)
			{
				double rightValue = Convert.ToDouble(right.Evaluate());
				double diff = leftValue - rightValue;
				if (double.IsPositiveInfinity(diff) || double.IsNegativeInfinity(diff))
				{
					diff = double.NaN;
					string text = left.ToString() + " - " + right.ToString();
					this.report.SemanticError(116, text, location);
				}
				return MakeConstant(diff);
			}
			else if (leftValue == 0.0d)
				return new LRealUnaryMinusOperator(right);
		}
		else if (right.IsConstant)
		{
			double rightValue = Convert.ToDouble(right.Evaluate());
			if (rightValue == 0.0d)
				return left;
		}
		else if (left is LRealUnaryMinusOperator)
		{
			if (right is LRealUnaryMinusOperator)
			{
				right = ((UnaryOperator)right).Operand;
				return new LRealSubOperator(right, left);
			}
			else {
				left = ((UnaryOperator)left).Operand;
				Expression expr = new LRealAddOperator(left, right);
				return new LRealUnaryMinusOperator(expr);
			}
		}
		else if (right is LRealUnaryMinusOperator)
		{
			right = ((UnaryOperator)right).Operand;
			return new LRealAddOperator(left, right);
		}
		return new LRealSubOperator(left, right);
	}

	private Expression MakeTimeSubOp(Expression left, Expression right, LexLocation location)
	{
		if (left.IsConstant)
		{
			TimeSpan leftValue = (TimeSpan)left.Evaluate();
			if (right.IsConstant)
			{
				TimeSpan difference;
				try {
					TimeSpan rightValue = (TimeSpan)right.Evaluate();
					difference = leftValue.Subtract(rightValue);
				}
				catch (System.OverflowException)
				{
					difference = TimeSpan.Zero;
					string text = left.ToString() + " - " + right.ToString();
					this.report.SemanticError(117, text, location);
				}
				return this.MakeConstant(difference);
			}
			else if (leftValue == TimeSpan.Zero)
				return new TimeUnaryMinusOperator(right);
		}
		else if (right.IsConstant)
		{
			TimeSpan rightValue = (TimeSpan)right.Evaluate();
			if (rightValue == TimeSpan.Zero)
				return left;
		}
		return new TimeSubOperator(left, right);
	}

	private Expression MakeDateTimeTimeSubOp(Expression left, Expression right, LexLocation location)
	{
		 if (! left.IsConstant || ! right.IsConstant)
			return new DTTimeSubOperator(left, right);
		else {
			DateTime difference;
			DateTime dateTime = Convert.ToDateTime(left.Evaluate());
			try {
				TimeSpan timeSpan =(TimeSpan)right.Evaluate();
				difference = dateTime.Subtract(timeSpan);
			}
			catch (System.ArgumentOutOfRangeException)
			{
				difference = DateTime.MinValue;
				string text = left.ToString() + " - " + right.ToString();
				this.report.SemanticError(118, text, location);
			}
			return this.MakeConstant(difference);
		}
	}

	private Expression MakeDateTimeSubOp(Expression left, Expression right, LexLocation location)
	{
		if (! left.IsConstant || ! right.IsConstant)
			return new DateTimeSubOperator(left, right);
		else {
			TimeSpan difference;
			DateTime leftValue = Convert.ToDateTime(left.Evaluate());
			try {
				DateTime rightValue = Convert.ToDateTime(right.Evaluate());
				difference = leftValue.Subtract(rightValue);
			}
			catch (System.ArgumentOutOfRangeException)
			{
				difference = TimeSpan.Zero;
				string text = left.ToString() + " - " + right.ToString();
				this.report.SemanticError(118, text, location);
			}
			return this.MakeConstant(difference);
		}
	}

	private Expression MakeDateSubOp(Expression left, Expression right, LexLocation location)
	{
		if (! left.IsConstant || ! right.IsConstant)
			return new DateSubOperator(left, right);
		else {
			TimeSpan difference;
			DateTime leftValue = Convert.ToDateTime(left.Evaluate());
			try {
				DateTime rightValue = Convert.ToDateTime(right.Evaluate());
				difference = leftValue.Subtract(rightValue);
			}
			catch (System.ArgumentOutOfRangeException)
			{
				difference = TimeSpan.Zero;
				string text = left.ToString() + " - " + right.ToString();
				this.report.SemanticError(118, text, location);
			}
			return this.MakeConstant(difference);
		}
	}

	private Expression MakeTimeOfDaySubOp(Expression left, Expression right, LexLocation location)
	{
		if (left.IsConstant && right.IsConstant)
		{
			TimeSpan difference;
			TimeSpan leftValue = (TimeSpan)left.Evaluate();
			
			try {
				TimeSpan rightValue = (TimeSpan)right.Evaluate();
				difference = leftValue.Subtract(rightValue);
			}
			catch (System.OverflowException)
			{
				difference = TimeSpan.Zero;
				string text = left.ToString() + " - " + right.ToString();
				this.report.SemanticError(119, text, location);
			}
			return this.MakeConstant(difference);
		}
		return new TimeOfDaySubOperator(left, right);
	}

	private Expression MakeTimeOfDayTimeSubOp(Expression left, Expression right, LexLocation location)
	{
		if (left.IsConstant)
		{
			TimeSpan leftValue = (TimeSpan)left.Evaluate();
			if (leftValue == TimeSpan.Zero)
				return right;
			else if (right.IsConstant)
			{
				TimeSpan difference;
			
				try {
					TimeSpan rightValue = (TimeSpan)right.Evaluate();
					difference = leftValue.Subtract(rightValue);
				}
				catch (System.OverflowException)
				{
					difference = TimeSpan.Zero;
					string text = left.ToString() + " - " + right.ToString();
					this.report.SemanticError(117, text, location);
				}
				return this.MakeConstant(difference);
			}
		}
		return new TODTimeSubOperator(left, right);
	}

	private Expression MakeIntMulOp(Expression left, Expression right)
	{
		if (left.IsConstant)
		{
			long leftValue = Convert.ToInt64(left.Evaluate());
			
			if (leftValue == 0)
				return MakeIntConstant(0);
			else if (leftValue == 1)
				return right;
			else if (right.IsConstant)
			{
				long rightValue = Convert.ToInt64(right.Evaluate());
				long product = leftValue * rightValue;
				return MakeIntConstant(product);
			}
			else if (leftValue == -1)
				return new IntUnaryMinusOperator(right);
		}
		else if (right.IsConstant)
		{
			long rightValue = Convert.ToInt64(right.Evaluate());
			if (rightValue == 0)
				return MakeIntConstant(0);
			else if (rightValue == 1)
				return left;
			else if (rightValue == -1)
				return new IntUnaryMinusOperator(left);
		}
		else if ((left is IntUnaryMinusOperator) && (right is IntUnaryMinusOperator))
		{
			left = ((UnaryOperator)left).Operand;
			right = ((UnaryOperator)right).Operand;
		}
		if (left.DataType == right.DataType)
			return new IntMulOperator(left, right, left.DataType);
		else if (left.DataType == TypeNode.LInt)
			return new IntMulOperator(left, this.MakeInt2LInt(right), TypeNode.LInt);
		else if (right.DataType == TypeNode.LInt)
			return new IntMulOperator(this.MakeInt2LInt(left), right, TypeNode.LInt);
		else {
			TypeNode resultDataType = this.LargestDataType(left, right);
			return new IntMulOperator(left, right, resultDataType);
		}
	}

	private Expression MakeUIntMulop(Expression left, Expression right)
	{
		if (left.IsConstant)
		{
			ulong leftValue = Convert.ToUInt64(left.Evaluate());
			
			if (leftValue == 0)
				return MakeIntConstant((ulong)0);
			else if (leftValue == 1)
				return right;
			else if (right.IsConstant)
			{
				ulong rightValue = Convert.ToUInt64(right.Evaluate());
				ulong product = leftValue * rightValue;
				return this.MakeIntConstant(product);
			}
		}
		else if (right.IsConstant)
		{
			ulong rightValue = Convert.ToUInt64(right.Evaluate());
			if (rightValue == 0)
				return this.MakeIntConstant((ulong)0);
			else if (rightValue == 1)
				return left;
		}
		if (left.DataType == right.DataType)
			return new UIntMulOperator(left, right, left.DataType);
		else if (left.DataType == TypeNode.LInt)
			return new UIntMulOperator(left, this.MakeInt2LInt(right), TypeNode.LInt);
		else if (right.DataType == TypeNode.LInt)
			return new UIntMulOperator(this.MakeInt2LInt(left), right, TypeNode.LInt);
		else {
			TypeNode resultDataType = this.LargestDataType(left, right);
			return new UIntMulOperator(left, right, resultDataType);
		}
	}

	private Expression MakeIntUIntMulop(Expression left, Expression right, LexLocation location)
	{
		TypeNode resultDataType;
		if (left.DataType.Size > right.DataType.Size)
			resultDataType = left.DataType;
		else if (right.DataType == TypeNode.USInt)
			resultDataType = TypeNode.Int;
		else if (right.DataType == TypeNode.UInt)
			resultDataType = TypeNode.DInt;
		else if (right.DataType == TypeNode.UDInt)
			resultDataType = TypeNode.LInt;
		else {
			this.report.SemanticError(14, "*", left.DataType.Name, right.DataType.Name, location);
			return Expression.Error;
		}
		if (left.IsConstant)
		{
			long leftValue = Convert.ToInt64(left.Evaluate());
			if (leftValue == 0)
				return MakeIntConstant(0);
			else if (leftValue == 1)
				return right;
			else if (right.IsConstant)
			{
				uint rightValue = Convert.ToUInt32(right.Evaluate());
				long product = leftValue * rightValue;
				return MakeIntConstant(product);
			}
			else if (leftValue == -1)
				return right;
		}
		else if (right.IsConstant)
		{
			uint rightValue = Convert.ToUInt32(right.Evaluate());
			if (rightValue == 0)
				return MakeIntConstant(0);
			else if (rightValue == 1)
				return left;
		}
		return new IntMulOperator(left, right, resultDataType);
	}

	private Expression MakeRealMulOp(Expression left, Expression right, LexLocation location)
	{
		if (left.IsConstant)
		{
			float leftValue = Convert.ToSingle(left.Evaluate());
			if (leftValue == 0.0f)
				return MakeConstant(0.0f);
			else if (leftValue == 1.0f)
				return right;
			else if (right.IsConstant)
			{
				float product;

				try {
					float rightValue = Convert.ToSingle(right.Evaluate());
					product = leftValue * rightValue;
				}
				catch (System.OverflowException)
				{
					product = float.NaN;
					string text = left.ToString() + " * " + right.ToString();
					this.report.SemanticError(116, text, location);
				}
				return MakeConstant(product);
			}
			else if (leftValue == -1.0f)
				return new RealUnaryMinusOperator(right);
		}
		else if (right.IsConstant)
		{
			float rightValue = Convert.ToSingle(right.Evaluate());
			if (rightValue == 0.0f)
				return MakeConstant(0.0f);
			else if (rightValue == 1.0f)
				return left;
			else if (rightValue == -1.0f)
				return new RealUnaryMinusOperator(left);
		}
		return new RealMulOperator(left, right);
	}

	private Expression MakeLRealMulOp(Expression left, Expression right, LexLocation location)
	{
		if (left.IsConstant)
		{
			double leftValue = Convert.ToDouble(left.Evaluate());
			if (leftValue == 0.0d)
				return MakeConstant(0.0d);
			else if (leftValue == 1.0d)
				return right;
			else if (right.IsConstant)
			{
				double product;

				try {
					double rightValue = Convert.ToDouble(right.Evaluate());
					product = leftValue * rightValue;
				}
				catch (System.OverflowException)
				{
					product = double.NaN;
					string text = left.ToString() + " * " + right.ToString();
					this.report.SemanticError(116, text, location);
				}
				return MakeConstant(product);
			}
			else if (leftValue == -1.0d)
				return new LRealUnaryMinusOperator(right);
		}
		else if (right.IsConstant)
		{
			double rightValue = Convert.ToDouble(right.Evaluate());
			if (rightValue == 0.0d)
				return MakeConstant(0.0d);
			else if (rightValue == 1.0d)
				return left;
			else if (rightValue == -1.0d)
				return new LRealUnaryMinusOperator(left);
		}
		return new LRealMulOperator(left, right);
	}

	private Expression MakeIntTimeMulOp(Expression left, Expression right, LexLocation location)
	{
		if (left.IsConstant)
		{
			TimeSpan timeSpan = (TimeSpan)left.Evaluate();
			
			if (timeSpan == TimeSpan.Zero)
				return this.MakeConstant(TimeSpan.Zero);
			else if (right.IsConstant)
			{
				long factor = Convert.ToInt64(right.Evaluate());
				long ticks = timeSpan.Ticks * factor;
				if (ticks < TimeSpan.MinValue.Ticks || ticks > TimeSpan.MaxValue.Ticks)
				{
					ticks = 0;
					string text = left.ToString() + " * " + right.ToString();
					this.report.SemanticError(117, text, location);
				}
				return this.MakeConstant(new TimeSpan(ticks));
			}
		}
		else if (right.IsConstant)
		{
			long rightValue = Convert.ToInt64(right.Evaluate());
			if (rightValue == 0)
				return this.MakeConstant(TimeSpan.Zero);
			else if (rightValue == 1)
				return left;
			else if (rightValue == -1)
				return new TimeUnaryMinusOperator(left);
		}
		return new IntTimeMulOperator(left, right);
	}

	private Expression MakeRealTimeMulOp(Expression left, Expression right, LexLocation location)
	{
		if (left.IsConstant)
		{
			TimeSpan timeSpan = (TimeSpan)left.Evaluate();
			
			if (timeSpan == TimeSpan.Zero)
				return this.MakeConstant(TimeSpan.Zero);
			else if (right.IsConstant)
			{
				Single factor = Convert.ToSingle(right.Evaluate());
				long ticks = (long)(timeSpan.Ticks * factor);
				if (ticks < TimeSpan.MinValue.Ticks || ticks > TimeSpan.MaxValue.Ticks)
				{
					ticks = 0;
					string text = left.ToString() + " * " + right.ToString();
					this.report.SemanticError(117, text, location);
				}
				return this.MakeConstant(new TimeSpan(ticks));
			}
		}
		else if (right.IsConstant)
		{
			Single rightValue = Convert.ToSingle(right.Evaluate());
			if (rightValue == 0.0f)
				return this.MakeConstant(TimeSpan.Zero);
			else if (rightValue == 1.0f)
				return left;
			else if (rightValue == -1.0f)
				return new TimeUnaryMinusOperator(left);
		}
		return new RealTimeMulOperator(left, right);
	}

	private Expression MakeLRealTimeMulOp(Expression left, Expression right, LexLocation location)
	{
		if (left.IsConstant)
		{
			TimeSpan timeSpan = (TimeSpan)left.Evaluate();
			
			if (timeSpan == TimeSpan.Zero)
				return this.MakeConstant(TimeSpan.Zero);
			else if (right.IsConstant)
			{
				double factor = Convert.ToDouble(right.Evaluate());
				long ticks = (long)(timeSpan.Ticks * factor);
				if (ticks < TimeSpan.MinValue.Ticks || ticks > TimeSpan.MaxValue.Ticks)
				{
					ticks = 0;
					string text = left.ToString() + " * " + right.ToString();
					this.report.SemanticError(117, text, location);
				}
				return this.MakeConstant(new TimeSpan(ticks));
			}
		}
		else if (right.IsConstant)
		{
			double factor = Convert.ToDouble(right.Evaluate());
			if (factor == 0.0d)
				return this.MakeConstant(TimeSpan.Zero);
			else if (factor == 1.0d)
				return left;
			else if (factor == -1.0d)
				return new TimeUnaryMinusOperator(left);
		}
		return new LRealTimeMulOperator(left, right);
	}

	private Expression MakeIntDivOp(Expression left, Expression right, LexLocation location)
	{
		if (left.IsConstant)
		{
			long leftValue = Convert.ToInt64(left.Evaluate());
			
			if (right.IsConstant)
			{
				long rightValue = Convert.ToInt64(right.Evaluate());
				if (rightValue == 0)
				{
					this.report.SemanticError(42, location);
					return Expression.Error;
				}
				else {
					long quotient = leftValue / rightValue;
					return MakeIntConstant(quotient);
				}
			}
			else if (leftValue == 0)
				return MakeIntConstant((long)0);
		}
		else if (right.IsConstant)
		{
			long rightValue = Convert.ToInt64(right.Evaluate());
			if (rightValue == 0)
			{
				this.report.SemanticError(42, location);
				return Expression.Error;
			}
			else if (rightValue == 1)
				return left;
			else if (rightValue == -1)
				return new IntUnaryMinusOperator(left);
			else {
				int power;
				if (this.IsPowerOf2((ulong)Math.Abs(rightValue), out power))
				{
					Expression expr = new LeftShiftOperator(left, power);
					if (rightValue < 0)
						expr = new IntUnaryMinusOperator(expr);
					return expr;
				}
			}
		}
		if (left.DataType == right.DataType)
			return new IntDivOperator(left, right, left.DataType);
		else if (left.DataType == TypeNode.LInt)
			return new IntDivOperator(left, this.MakeInt2LInt(right), TypeNode.LInt);
		else if (right.DataType == TypeNode.LInt)
			return new IntDivOperator(this.MakeInt2LInt(left), right, TypeNode.LInt);
		else {
			TypeNode resultDataType = this.LargestDataType(left, right);
			return new IntDivOperator(left, right, resultDataType);
		}
	}

	private Expression MakeTimeIntDivOp(Expression left, Expression right, LexLocation location)
	{
		if (left.IsConstant)
		{
			TimeSpan leftValue = (TimeSpan)left.Evaluate();
			
			if (right.IsConstant)
			{
				long rightValue = Convert.ToInt64(right.Evaluate());
				if (rightValue == 0)
				{
					this.report.SemanticError(42, location);
					return Expression.Error;
				}
				else {
					long ticks = leftValue.Ticks/rightValue;
					return this.MakeConstant(new TimeSpan(ticks));
				}
			}
			else if (leftValue == TimeSpan.Zero)
				return this.MakeConstant(TimeSpan.Zero);
		}
		else if (right.IsConstant)
		{
			long rightValue = Convert.ToInt64(right.Evaluate());
			if (rightValue == 0)
			{
				this.report.SemanticError(42, location);
				return Expression.Error;
			}
			else if (rightValue == 1)
				return left;
			else if (rightValue == -1)
				return new TimeUnaryMinusOperator(left);
		}
		return new TimeIntDivOperator(left, right);		
	}

	private Expression MakeTimeRealDivOp(Expression left, Expression right, LexLocation location)
	{
		if (left.IsConstant)
		{
			TimeSpan leftValue = (TimeSpan)left.Evaluate();
			
			if (right.IsConstant)
			{
				float rightValue = Convert.ToSingle(right.Evaluate());
				if (rightValue == 0.0f)
				{
					this.report.SemanticError(42, location);
					return Expression.Error;
				}
				else {
					double ticks = leftValue.Ticks/(double)rightValue;
					return this.MakeConstant(new TimeSpan((long)ticks));
				}
			}
			else if (leftValue == TimeSpan.Zero)
				return this.MakeConstant(TimeSpan.Zero);
		}
		else if (right.IsConstant)
		{
			float rightValue = Convert.ToSingle(right.Evaluate());
			if (rightValue == 0.0f)
			{
				this.report.SemanticError(42, location);
				return Expression.Error;
			}
			else if (rightValue == 1.0f)
				return left;
			else if (rightValue == -1.0f)
				return new TimeUnaryMinusOperator(left);
		}
		return new TimeLRealDivOperator(left, right);		
	}

	private Expression MakeTimeLRealDivOp(Expression left, Expression right, LexLocation location)
	{
		if (left.IsConstant)
		{
			TimeSpan leftValue = (TimeSpan)left.Evaluate();
			
			if (right.IsConstant)
			{
				double rightValue = Convert.ToDouble(right.Evaluate());
				if (rightValue == 0.0d)
				{
					this.report.SemanticError(42, location);
					return Expression.Error;
				}
				else {
					double ticks = leftValue.Ticks/rightValue;
					return this.MakeConstant(new TimeSpan((long)ticks));
				}
			}
			else if (leftValue == TimeSpan.Zero)
				return this.MakeConstant(TimeSpan.Zero);
		}
		else if (right.IsConstant)
		{
			float rightValue = Convert.ToSingle(right.Evaluate());
			if (rightValue == 0.0f)
			{
				this.report.SemanticError(42, location);
				return Expression.Error;
			}
			else if (rightValue == 1.0f)
				return left;
			else if (rightValue == -1.0f)
				return new TimeUnaryMinusOperator(left);
		}
		return new TimeRealDivOperator(left, right);		
	}

	private Expression MakeRealDivOp(Expression left, Expression right, LexLocation location)
	{
		if (left.IsConstant)
		{
			float leftValue = Convert.ToSingle(left.Evaluate());
			
			if (right.IsConstant)
			{
				float rightValue = Convert.ToSingle(right.Evaluate());
				float quotient = leftValue / rightValue;
				if (! float.IsInfinity(quotient))
					return MakeConstant(quotient);
				else {
					this.report.SemanticError(42, location);
					return Expression.Error;
				}
			}
			if (leftValue == 0.0f)
				return MakeConstant(0.0f);
		}
		else if (right.IsConstant)
		{
			float rightValue = Convert.ToSingle(right.Evaluate());
			if (rightValue == 0.0f)
			{
				this.report.SemanticError(42, location);
				return Expression.Error;
			}
			else if (rightValue == 1.0f)
				return left;
			else if (rightValue == -1.0f)
				return new RealUnaryMinusOperator(left);
		}
		return new RealDivOperator(left, right);		
	}

	private Expression MakeLRealDivOp(Expression left, Expression right, LexLocation location)
	{
		if (left.IsConstant)
		{
			double leftValue = Convert.ToDouble(left.Evaluate());
			
			if (right.IsConstant)
			{
				double rightValue = Convert.ToDouble(right.Evaluate());
				double quotient   = leftValue / rightValue;
				if (! double.IsInfinity(quotient))
					return MakeConstant(quotient);
				else {
					this.report.SemanticError(42, location);
					return Expression.Error;
				}
			}
			else if (leftValue == 0.0d)
				return MakeConstant(0.0d);
		}
		else if (right.IsConstant)
		{
			double rightValue = Convert.ToDouble(right.Evaluate());
			if (rightValue == 0.0d)
			{
				this.report.SemanticError(42, location);
				return Expression.Error;
			}
			else if (rightValue == 1.0d)
				return left;
			else if (rightValue == -1.0d)
				return new LRealUnaryMinusOperator(left);
		}
		return new LRealDivOperator(left, right);		
	}

	private Expression MakeIntUnaryMinusOp(Expression expression)
	{
		if (expression.IsConstant)
		{
			long value = Convert.ToInt64(expression.Evaluate());
			return MakeIntConstant(-value);
		}
		else if (expression is IntUnaryMinusOperator)
			return ((UnaryOperator)expression).Operand;
		else
			return new IntUnaryMinusOperator(expression);
	}

	private Expression MakeIntPowerOp(Expression left, Expression right)
	{
		if (right.IsConstant)
		{
			int exponent = Convert.ToInt32(right.Evaluate());
			if (exponent < 0)
				return MakeConstant(0);
			else if (exponent == 0)
				return MakeConstant(1);
			else if (exponent == 1)
				return left;
			else if (left.IsConstant)
			{
				if (left.DataType.IsSignedIntType)
				{
					long base_ = Convert.ToInt64(left.Evaluate());
					long result = this.IntPower(base_, exponent);
					return MakeIntConstant(result); 
				}
				else {
					ulong base_ = Convert.ToUInt64(left.Evaluate());
					ulong result = this.IntPower(base_, exponent);
					return this.MakeIntConstant(result); 
				}
			}
		}
		return new IntPowerOperator(left, right, left.DataType);
	}

	private Expression MakeRealPowerOp(Expression left, Expression right)
	{
		if (! right.IsConstant)
		{
			if (right.DataType.IsIntegerType)
				return new RealPowerOperator(left, new Int2RealOperator(right));
			else if (right.DataType == TypeNode.Real)
				return new RealPowerOperator(left, right);
			else {
				string msg;
				msg = "MakeRealPowerOp(): Real type exponent expected.";
				throw new STLangCompilerError(msg);
			}
		}
		else if (right.DataType.IsIntegerType)
		{
			int exponent = Convert.ToInt32(right.Evaluate());
			
			if (exponent == 0)
				return MakeConstant(1.0f);
			else if (exponent == 1)
				return left;
			else if (left.IsConstant)
			{
				double base_ = Convert.ToDouble(left.Evaluate());
				float result = (float)Math.Pow(base_, exponent);
				return MakeConstant(result);
			}
			else if (Math.Abs(exponent) > 4)
				right = MakeConstant((float)exponent);
		}
		else if (right.DataType == TypeNode.Real)
		{
			float exponent = Convert.ToSingle(right.Evaluate());
			if (exponent == 0.0f)
				return MakeConstant(1.0f);
			else if (exponent == 1.0f)
				return left;
			else if (left.IsConstant)
			{
				double base_ = Convert.ToDouble(left.Evaluate());
				float result = (float)Math.Pow(base_, exponent);
				return MakeConstant(result);
			}
		}
		return new RealPowerOperator(left, right);
	}

	private Expression MakeLRealPowerOp(Expression left, Expression right)
	{
		if (! right.IsConstant)
		{
			if (right.DataType.IsIntegerType)
				return new LRealPowerOperator(left, new Int2LRealOperator(right));
			else if (right.DataType == TypeNode.LReal)
				return new LRealPowerOperator(left, right);
			else {
				string msg;
				msg = "MakeRealPowerOp(): LReal type exponent expected.";
				throw new STLangCompilerError(msg);
			}
		}
		else if (right.DataType.IsIntegerType)
		{
			int exponent = Convert.ToInt32(right.Evaluate());
			
			if (exponent == 0)
				return MakeConstant(1.0d);
			else if (exponent == 1)
				return left;
			else if (left.IsConstant)
			{
				double base_ = Convert.ToDouble(left.Evaluate());
				double result = Math.Pow(base_, exponent);
				return MakeConstant(result);
			}
			else if (Math.Abs(exponent) > 4)
				right = MakeConstant((double)exponent);
		}
		else if (right.DataType == TypeNode.LReal)
		{
			double exponent = Convert.ToDouble(right.Evaluate());
			if (exponent == 0.0d)
				return MakeConstant(1.0d);
			else if (exponent == 1.0d)
				return left;
			else if (left.IsConstant)
			{
				double base_ = Convert.ToDouble(left.Evaluate());
				double result = Math.Pow(base_, exponent);
				return MakeConstant(result);
			}
		}
		return new LRealPowerOperator(left, right);
	}

	private Expression MakeIntModOp(Expression left, Expression right, LexLocation location)
	{
		if (right.IsConstant)
		{
			ulong rightValue = Convert.ToUInt64(right.Evaluate());
			if (rightValue == 0)
			{
				this.report.SemanticError(42, location);
				return Expression.Error;
			}
			else if (left.IsConstant)
			{
				ulong leftValue = Convert.ToUInt64(left.Evaluate());
				ulong result = leftValue % rightValue;
				if (left.DataType.IsUnsignedIntType || right.DataType.IsUnsignedIntType)
					return this.MakeIntConstant(result);
				else
					return MakeIntConstant((long)result);
			}
			else {
				int power;

				if (this.IsPowerOf2(rightValue, out power))
				{
					long bitMask = 1;
					bitMask <<= power;
					bitMask -= 1;
					right = MakeIntConstant(bitMask);
					return new BitAndOperator(left, right, right.DataType);
				}
			}
		}
		return new ModOperator(left, right);
	}

	private Expression MakeAddOperator(Expression left, Expression right, LexLocation location)
	{
		if (left == null || right == null)
			return Expression.Error;
		else if (left is FunctionName)
		{
			this.report.SemanticError(148, left.ToString(), location);
			return Expression.Error;
		}
		else if (right is FunctionName)
		{
			this.report.SemanticError(148, right.ToString(), location);
			return Expression.Error;
		}
		else {
			TypeNode leftDataType = left.DataType;
			TypeNode rightDataType = right.DataType;

			if (leftDataType == TypeNode.Error || rightDataType == TypeNode.Error)
				return Expression.Error;
			else if (leftDataType.IsSignedIntType)
			{
				if (rightDataType.IsSignedIntType)
					return this.MakeIntAddOp(left, right);
				else if (rightDataType.IsUnsignedIntType)
					return this.MakeIntUIntAddop(left, right, location);
				else if (rightDataType == TypeNode.Real)
					return this.MakeRealAddOp(this.MakeInt2Real(left), right, location);
				else if (rightDataType == TypeNode.LReal)
					return this.MakeLRealAddOp(this.MakeInt2LReal(left), right, location);
			}
			else if (leftDataType.IsUnsignedIntType)
			{
				if (rightDataType.IsSignedIntType)
					return this.MakeIntUIntAddop(right, left, location);
				else if (rightDataType.IsUnsignedIntType)
					return this.MakeUIntAddop(left, right);
				else if (rightDataType == TypeNode.Real)
					return this.MakeRealAddOp(this.MakeInt2Real(left), right, location);
				else if (rightDataType == TypeNode.LReal)
					return this.MakeLRealAddOp(this.MakeInt2LReal(left), right, location);
			}
			else if (leftDataType == TypeNode.Real)
			{
				if (rightDataType == TypeNode.Real)
					return this.MakeRealAddOp(left, right, location);
				else if (rightDataType == TypeNode.LReal)
					return this.MakeLRealAddOp(this.MakeReal2LReal(left), right, location);
				else if (rightDataType.IsIntegerType)
					return this.MakeRealAddOp(left, this.MakeInt2Real(right), location);
			}
			else if (leftDataType == TypeNode.LReal)
			{
				if (rightDataType == TypeNode.LReal)
					return this.MakeLRealAddOp(left, right, location);
				else if (rightDataType == TypeNode.Real)
					return this.MakeLRealAddOp(left, this.MakeReal2LReal(right), location);
				else if (rightDataType.IsIntegerType)
					return this.MakeLRealAddOp(left, this.MakeInt2LReal(right), location);
			}
			else if (leftDataType == TypeNode.Time && rightDataType == TypeNode.Time)
				return this.MakeTTAddOp(left, right, location);
			else if (leftDataType == TypeNode.Time && rightDataType == TypeNode.TimeOfDay)
				return this.MakeTTODAddOp(left, right, location);
			else if (leftDataType == TypeNode.TimeOfDay && rightDataType == TypeNode.Time)
				return this.MakeTTODAddOp(right, left, location);
			else if (leftDataType == TypeNode.DateAndTime && rightDataType == TypeNode.Time)
				return this.MakeDTTimeAddOp(left, right, location);
			else if (leftDataType == TypeNode.Time && rightDataType == TypeNode.DateAndTime)
				return this.MakeDTTimeAddOp(right, left, location);
			//
			// Error. Incompatible operand types of operator +
			//
			this.report.SemanticError(14, "+", leftDataType.Name, rightDataType.Name, location);
			return Expression.Error;
		}
	}

	private Expression MakeSubOperator(Expression left, Expression right, LexLocation location)
	{
		if (left == null || right == null)
			return Expression.Error;
		else if (left is FunctionName)
		{
			this.report.SemanticError(148, left.ToString(), location);
			return Expression.Error;
		}
		else if (right is FunctionName)
		{
			this.report.SemanticError(148, right.ToString(), location);
			return Expression.Error;
		}
		else {
			TypeNode leftDataType = left.DataType;
			TypeNode rightDataType = right.DataType;

			if (leftDataType == TypeNode.Error || rightDataType == TypeNode.Error)
				return Expression.Error;
			else if (leftDataType.IsSignedIntType)
			{
				if (rightDataType.IsSignedIntType)
					return this.MakeIntSubOp(left, right);
				else if (rightDataType.IsUnsignedIntType)
					return this.MakeIntUIntSubOp(left, right, location);
				else if (rightDataType == TypeNode.Real)
					return this.MakeRealSubOp(this.MakeInt2Real(left), right, location);
				else if (rightDataType == TypeNode.LReal)
					return this.MakeLRealSubOp(this.MakeInt2LReal(left), right, location);
			}
			else if (leftDataType.IsUnsignedIntType)
			{
				if (rightDataType.IsSignedIntType)
					return this.MakeIntUIntSubOp(right, left, location);
				else if (rightDataType.IsUnsignedIntType)
					return this.MakeUIntSubOp(left, right);
				else if (rightDataType == TypeNode.Real)
					return this.MakeRealSubOp(this.MakeInt2Real(left), right, location);
				else if (rightDataType == TypeNode.LReal)
					return this.MakeLRealSubOp(this.MakeInt2LReal(left), right, location);
			}
			else if (leftDataType == TypeNode.Real)
			{
				if (rightDataType == TypeNode.Real)
					return this.MakeRealSubOp(left, right, location);
				else if (rightDataType == TypeNode.LReal)
					return this.MakeLRealSubOp(this.MakeReal2LReal(left), right, location);
				else if (rightDataType.IsIntegerType)
					return this.MakeRealSubOp(left, this.MakeInt2Real(right), location);
			}
			else if (leftDataType == TypeNode.LReal)
			{
				if (rightDataType == TypeNode.LReal)
					return this.MakeLRealSubOp(left, right, location);
				else if (rightDataType == TypeNode.Real)
					return this.MakeLRealSubOp(left, this.MakeReal2LReal(right), location);
				else if (rightDataType.IsIntegerType)
					return this.MakeLRealSubOp(left, this.MakeInt2LReal(right), location);
			}
			else if (leftDataType == TypeNode.Time && rightDataType == TypeNode.Time)
				return this.MakeTimeSubOp(left, right, location);
			else if (leftDataType == TypeNode.TimeOfDay && rightDataType == TypeNode.TimeOfDay)
				return this.MakeTimeOfDaySubOp(left, right, location);
			else if (leftDataType == TypeNode.TimeOfDay && rightDataType == TypeNode.Time)
				return this.MakeTimeOfDayTimeSubOp(right, left, location);
			else if (leftDataType == TypeNode.DateAndTime && rightDataType == TypeNode.Time)
				return this.MakeDateTimeTimeSubOp(left, right, location);
			else if (leftDataType == TypeNode.DateAndTime && rightDataType == TypeNode.DateAndTime)
				return this.MakeDateTimeSubOp(right, left, location);
			else if (leftDataType == TypeNode.Date && rightDataType == TypeNode.Date)
				return this.MakeDateSubOp(right, left, location);
			//
			// Error. Incompatible operand types of operator -
			//
			this.report.SemanticError(14, "-", leftDataType.Name, rightDataType.Name, location);
			return Expression.Error;
		}
	}

	private Expression MakeMulOperator(Expression left, Expression right, LexLocation location)
	{
		if (left == null || right == null)
			return Expression.Error;
		else if (left is FunctionName)
		{
			this.report.SemanticError(148, left.ToString(), location);
			return Expression.Error;
		}
		else if (right is FunctionName)
		{
			this.report.SemanticError(148, right.ToString(), location);
			return Expression.Error;
		}
		else {
			TypeNode leftDataType = left.DataType;
			TypeNode rightDataType = right.DataType;

			if (leftDataType == TypeNode.Error || rightDataType == TypeNode.Error)
				return Expression.Error;
			else if (leftDataType.IsSignedIntType)
			{
				if (rightDataType.IsSignedIntType)
					return this.MakeIntMulOp(left, right);
				else if (rightDataType.IsUnsignedIntType)
					return this.MakeIntUIntMulop(left, right, location);
				else if (rightDataType == TypeNode.Real)
					return this.MakeRealMulOp(this.MakeInt2Real(left), right, location);
				else if (rightDataType == TypeNode.LReal)
					return this.MakeLRealMulOp(this.MakeInt2LReal(left), right, location);
				else if (rightDataType == TypeNode.Time)
					return this.MakeIntTimeMulOp(right, left, location);
			}
			else if (leftDataType.IsUnsignedIntType)
			{
				if (rightDataType.IsSignedIntType)
					return this.MakeIntUIntMulop(right, left, location);
				else if (rightDataType.IsUnsignedIntType)
					return this.MakeUIntMulop(left, right);
				else if (rightDataType == TypeNode.Real)
					return this.MakeRealMulOp(this.MakeInt2Real(left), right, location);
				else if (rightDataType == TypeNode.LReal)
					return this.MakeLRealMulOp(this.MakeInt2LReal(left), right, location);
				else if (rightDataType == TypeNode.Time)
					return this.MakeIntTimeMulOp(right, left, location);
			}
			else if (leftDataType == TypeNode.Real)
			{
				if (rightDataType == TypeNode.Real)
					return this.MakeRealMulOp(left, right, location);
				else if (rightDataType == TypeNode.LReal)
					return this.MakeLRealMulOp(this.MakeReal2LReal(left), right, location);
				else if (rightDataType.IsIntegerType)
					return this.MakeRealMulOp(left, this.MakeInt2Real(right), location);
				else if (rightDataType == TypeNode.Time)
					return this.MakeRealTimeMulOp(right, left, location);
			}
			else if (leftDataType == TypeNode.LReal)
			{
				if (rightDataType == TypeNode.LReal)
					return this.MakeLRealMulOp(left, right, location);
				else if (rightDataType == TypeNode.Real)
					return this.MakeLRealMulOp(left, this.MakeReal2LReal(right), location);
				else if (rightDataType.IsIntegerType)
					return this.MakeLRealMulOp(left, this.MakeInt2LReal(right), location);
				else if (rightDataType == TypeNode.Time)
					return this.MakeLRealTimeMulOp(right, left, location);
			}
			else if (leftDataType == TypeNode.Time)
			{
				if (rightDataType.IsIntegerType)
					return this.MakeIntTimeMulOp(left, right, location);
				else if (rightDataType == TypeNode.Real)
					return this.MakeRealTimeMulOp(left, right, location);
				else if (rightDataType == TypeNode.LReal)
					return this.MakeLRealTimeMulOp(right, left, location);
			}
			//
			// Error. Incompatible operand types of operator *
			//
			this.report.SemanticError(14, "*", leftDataType.Name, rightDataType.Name, location);
			return Expression.Error;
		}
	}

	private Expression MakeDivOperator(Expression left, Expression right, LexLocation location)
	{
		if (left == null || right == null)
			return Expression.Error;
		else if (left is FunctionName)
		{
			this.report.SemanticError(148, left.ToString(), location);
			return Expression.Error;
		}
		else if (right is FunctionName)
		{
			this.report.SemanticError(148, right.ToString(), location);
			return Expression.Error;
		}
		else {
			TypeNode leftDataType = left.DataType;
			TypeNode rightDataType = right.DataType;

			if (leftDataType == TypeNode.Error || rightDataType == TypeNode.Error)
				return Expression.Error;
			else if (leftDataType.IsIntegerType)
			{
				if (rightDataType.IsIntegerType)
					return this.MakeIntDivOp(left, right, location);
				else if (rightDataType == TypeNode.Real)
					return this.MakeRealDivOp(this.MakeInt2Real(left), right, location);
				else if (rightDataType == TypeNode.LReal)
					return this.MakeLRealDivOp(this.MakeInt2LReal(left), right, location);
			}         
			else if (leftDataType == TypeNode.Real)
			{
				if (rightDataType == TypeNode.Real)
					return this.MakeRealDivOp(left, right, location);
				else if (rightDataType == TypeNode.LReal)
					return this.MakeLRealDivOp(this.MakeReal2LReal(left), right, location);
				else if (rightDataType.IsIntegerType)
					return this.MakeRealDivOp(left, this.MakeInt2Real(right), location);
			}
			else if (leftDataType == TypeNode.LReal)
			{
				if (rightDataType == TypeNode.LReal)
					return this.MakeLRealDivOp(left, right, location);
				else if (rightDataType == TypeNode.Real)
					return this.MakeLRealDivOp(left, this.MakeReal2LReal(right), location);
				else if (rightDataType.IsIntegerType)
					return this.MakeLRealDivOp(left, this.MakeInt2LReal(right), location);
			}
			else if (leftDataType == TypeNode.Time)
			{
				if (rightDataType.IsIntegerType)
					return this.MakeTimeIntDivOp(left, right, location);
				else if (rightDataType == TypeNode.Real)
					return this.MakeTimeRealDivOp(left, right, location);
				else if (rightDataType == TypeNode.LReal)
					return this.MakeTimeLRealDivOp(right, left, location);
			}
			//
			// Error. Incompatible operand types of operator /
			//
			this.report.SemanticError(14, "/", leftDataType.Name, rightDataType.Name, location);
			return Expression.Error;
		}
	}

	private Expression MakeRealUnaryMinusOp(Expression expression)
	{
		if (! expression.IsConstant)
			return new RealUnaryMinusOperator(expression);
		else {
			float realValue = -(float)expression.Evaluate();
			return MakeConstant(realValue);
		}
	}

	private Expression MakeLRealUnaryMinusOp(Expression expression)
	{
		if (! expression.IsConstant)
			return new LRealUnaryMinusOperator(expression);
		else {
			double doubleValue = -(double)expression.Evaluate();
			return MakeConstant(doubleValue);
		}
	}

	private Expression MakeTimeUnaryMinusOp(Expression expression)
	{
		if (! expression.IsConstant)
			return new LRealUnaryMinusOperator(expression);
		else {
			double doubleValue = -(double)expression.Evaluate();
			return MakeConstant(doubleValue);
		}
	}

	private Expression MakeUnaryMinusOperator(Expression expression, LexLocation location)
	{
		if (expression == null)
			return Expression.Error;
		else {
			TypeNode dataType = expression.DataType;
			if (dataType == TypeNode.Error)
				return expression;
			else if (dataType.IsSignedIntType)
				return this.MakeIntUnaryMinusOp(expression);
			else if (dataType.IsUnsignedIntType)
			{
				// unary minus applied to unsigned int type has no effect
				this.report.Warning(8, dataType.Name, location);
				return expression;
			}
			else if (dataType == TypeNode.Real)
				return this.MakeRealUnaryMinusOp(expression);
			else if (dataType == TypeNode.LReal)
				return this.MakeLRealUnaryMinusOp(expression);
			else if (dataType == TypeNode.Time)
				return this.MakeTimeUnaryMinusOp(expression);
			else {
				this.report.SemanticError(-14, "-", dataType.Name, location);
				return Expression.Error;
			}
		}
	}

	private Expression MakeUnaryPlusOperator(Expression expression, LexLocation location)
	{
		if (expression == null)
			return Expression.Error;
		else {
			TypeNode dataType = expression.DataType;
			if (dataType == TypeNode.Error)
				return expression;
			else if (dataType.IsNumericalType)
				return expression;
			else if (dataType == TypeNode.Time)
				return expression;
			else {
				this.report.SemanticError(-14, "+", dataType.Name, location);
				return Expression.Error;
			}
		}
	}

	private bool IsBitWiseType(Expression expression)
	{
		TypeNode dataType = expression.DataType;
		return dataType.IsBitStringType || (dataType.IsIntegerType && expression.IsConstant);
	}

	private Expression MakeAndOperator(Expression left, Expression right, LexLocation location)
	{
		if (left == null || right == null)
			return Expression.Error;
		else if (left is FunctionName)
		{
			this.report.SemanticError(148, left.ToString(), location);
			return Expression.Error;
		}
		else if (right is FunctionName)
		{
			this.report.SemanticError(148, right.ToString(), location);
			return Expression.Error;
		}
		else if (left.DataType == TypeNode.Error || right.DataType == TypeNode.Error)
			return Expression.Error;
		else if (! this.IsBitWiseType(left) || ! this.IsBitWiseType(right))
		{
			string dataTypeName1 = left.DataType.Name;
			string dataTypeName2 = right.DataType.Name;
			this.report.SemanticError(14, "AND", dataTypeName1, dataTypeName2, location);
			return Expression.Error;
		}
		else {
			TypeNode resultDataType = this.LargestDataType(left, right);
			if (left.IsConstant)
			{
				ulong leftValue = Convert.ToUInt64(left.Evaluate());
				if (leftValue == 0)
					return this.MakeConstant(leftValue, resultDataType);
				else if (this.BitCount(leftValue) == resultDataType.BitCount)
					return right;
				else if (right.IsConstant)
				{
					ulong rightValue = Convert.ToUInt64(right.Evaluate());
					ulong result = leftValue & rightValue;
					return this.MakeConstant(result, resultDataType);
				}
			}
			else if (right.IsConstant)
			{
				ulong rightValue = Convert.ToUInt64(right.Evaluate());
				if (rightValue == 0)
					return this.MakeConstant(rightValue, resultDataType);
				else if (this.BitCount(rightValue) == resultDataType.BitCount)
					return left;
			}
			if (resultDataType != TypeNode.Bool)
				return new BitAndOperator(left, right, resultDataType);
			else if (left is AndOperator)
			{
				((AndOperator)left).Add(right);
				return left;
			}
			else if (right is AndOperator)
			{
				((AndOperator)right).AddLeft(left);
				return right;
			}
			return new AndOperator(left, right);
		}
	}

	private Expression MakeIOrOperator(Expression left, Expression right, LexLocation location)
	{
		if (left == null || right == null)
			return Expression.Error;
		else if (left is FunctionName)
		{
			this.report.SemanticError(148, left.ToString(), location);
			return Expression.Error;
		}
		else if (right is FunctionName)
		{
			this.report.SemanticError(148, right.ToString(), location);
			return Expression.Error;
		}
		else if (left.DataType == TypeNode.Error || right.DataType == TypeNode.Error)
			return Expression.Error;
		else if (! this.IsBitWiseType(left) || ! this.IsBitWiseType(right))
		{
			string dataTypeName1 = left.DataType.Name;
			string dataTypeName2 = right.DataType.Name;
			this.report.SemanticError(14, "OR", dataTypeName1, dataTypeName2, location);
			return Expression.Error;
		}
		else {
			TypeNode resultDataType = this.LargestDataType(left, right);
			if (left.IsConstant)
			{
				ulong leftValue = Convert.ToUInt64(left.Evaluate());
				if (leftValue == 0)
					return right;
				else if (this.BitCount(leftValue) == resultDataType.BitCount)
					return left;
				else if (right.IsConstant)
				{
					ulong rightValue = Convert.ToUInt64(right.Evaluate());
					ulong result = leftValue | rightValue;
					return this.MakeConstant(result, resultDataType);
				}
			}
			else if (right.IsConstant)
			{
				ulong rightValue = Convert.ToUInt64(right.Evaluate());
				if (rightValue == 0)
					return left;
				else if (this.BitCount(rightValue) == resultDataType.BitCount)
					return right;
			}
			if (resultDataType != TypeNode.Bool)
				return new BitIOrOperator(left, right, resultDataType);
			else if (left is IOrOperator)
			{
				((IOrOperator)left).Add(right);
				return left;
			}
			else if (right is IOrOperator)
			{
				((IOrOperator)right).AddLeft(left);
				return right;
			}
			return new IOrOperator(left, right);
		}
	}

	private Expression MakeXOrOperator(Expression left, Expression right, LexLocation location)
	{
		if (left == null || right == null)
			return Expression.Error;
		else if (left is FunctionName)
		{
			this.report.SemanticError(148, left.ToString(), location);
			return Expression.Error;
		}
		else if (right is FunctionName)
		{
			this.report.SemanticError(148, right.ToString(), location);
			return Expression.Error;
		}
		else if (left.DataType == TypeNode.Error || right.DataType == TypeNode.Error)
			return Expression.Error;
		else if (! this.IsBitWiseType(left) || ! this.IsBitWiseType(right))
		{
			string DataTypeName1 = left.DataType.Name;
			string DataTypeName2 = right.DataType.Name;
			this.report.SemanticError(14, "XOR", DataTypeName1, DataTypeName2, location);
			return Expression.Error;
		}
		else {
			TypeNode resultDataType = this.LargestDataType(left, right);
			if (left.IsConstant)
			{
				ulong leftValue = Convert.ToUInt64(left.Evaluate());
				if (leftValue == 0)
					return right;
				else if (right.IsConstant)
				{
					ulong rightValue = Convert.ToUInt64(right.Evaluate());
					ulong result = leftValue ^ rightValue;
					return this.MakeConstant(result, resultDataType);
				}
			}
			else if (right.IsConstant)
			{
				ulong rightValue = Convert.ToUInt64(right.Evaluate());
				if (rightValue == 0)
					return left;
			}
			if (resultDataType != TypeNode.Bool)
				return new BitXOrOperator(left, right, resultDataType);
			else {
				Expression leftTree = left.DeMorgan();
				Expression rightTree = right.DeMorgan();
				if (leftTree is AndOperator)
					((AndOperator)leftTree).Add(right);
				else
					leftTree = new AndOperator(leftTree, right);
				if (rightTree is AndOperator)
					((AndOperator)rightTree).Add(left);
				else
					rightTree = new AndOperator(left, rightTree);
				return new IOrOperator(leftTree, rightTree);
			}	
		}
	}

	private Expression MakeNotOperator(Expression expression, LexLocation location)
	{
		if (expression == null)
			return Expression.Error;
		else if (expression is FunctionName)
		{
			this.report.SemanticError(148, expression.ToString(), location);
			return Expression.Error;
		}
		else if (expression.DataType == TypeNode.Error)
			return Expression.Error;
		else if (! this.IsBitWiseType(expression))
		{
			string dataTypeName = expression.DataType.Name;
			this.report.SemanticError(-14, "NOT", dataTypeName, location);
			return Expression.Error;
		}
		else if (expression.IsConstant)
		{
			ulong value = Convert.ToUInt64(expression.Evaluate());
			return this.MakeConstant(~value, expression.DataType);
		}
		if (expression.DataType == TypeNode.Bool)
			return expression.DeMorgan();
		else
			return new BitNotOperator(expression);
	}

	private Expression MakePowOperator(Expression base_, Expression exponent, LexLocation location)
	{
		if (base_ == null || exponent == null)
			return Expression.Error;
        else if (base_ is FunctionName)
		{
			this.report.SemanticError(148, base_.ToString(), location);
			return Expression.Error;
		}
		else if (exponent is FunctionName)
		{
			this.report.SemanticError(148, exponent.ToString(), location);
			return Expression.Error;
		}
		else if (base_.DataType == TypeNode.Error || exponent.DataType == TypeNode.Error)
			return Expression.Error;
		else if (base_.DataType == TypeNode.LReal)
		{
			if (exponent.DataType == TypeNode.LReal)
				return this.MakeLRealPowerOp(base_, exponent);
			else if (exponent.DataType == TypeNode.Real)
				return this.MakeLRealPowerOp(base_, this.MakeReal2LReal(exponent));
			else if (exponent.DataType.IsIntegerType)
				return this.MakeLRealPowerOp(base_, exponent);
		}
		else if (base_.DataType == TypeNode.Real)
		{
			if (exponent.DataType == TypeNode.Real)
				return this.MakeRealPowerOp(base_, exponent);
			else if (exponent.DataType == TypeNode.LReal)
				return this.MakeRealPowerOp(this.MakeReal2LReal(base_), exponent);
			else if (exponent.DataType.IsIntegerType)
				return this.MakeRealPowerOp(base_, exponent);
		}
		else if (base_.DataType.IsIntegerType)
		{
			if (exponent.DataType.IsIntegerType)
				return this.MakeIntPowerOp(base_, exponent);
			else if (exponent.DataType == TypeNode.LReal)
				return this.MakeLRealPowerOp(this.MakeInt2LReal(base_), exponent);
			else if (exponent.DataType == TypeNode.Real)
				return this.MakeRealPowerOp(this.MakeInt2Real(base_), exponent);
		}
		//
		// Error. Incompatible operand types of operator **
		//
		string dataTypeName1 = base_.DataType.Name;
		string dataTypeName2 = exponent.DataType.Name;
		this.report.SemanticError(14, " ** ", dataTypeName1, dataTypeName2, location);
		return Expression.Error;
	}

	private Expression MakeModOperator(Expression left, Expression right, LexLocation location)
	{
		if (left == null || right == null)
			return Expression.Error;
		else if (left is FunctionName)
		{
			this.report.SemanticError(148, left.ToString(), location);
			return Expression.Error;
		}
		else if (right is FunctionName)
		{
			this.report.SemanticError(148, right.ToString(), location);
			return Expression.Error;
		}
		else if (left.DataType == TypeNode.Error || right.DataType == TypeNode.Error)
			return Expression.Error;
		else if (left.DataType.IsIntegerType && right.DataType.IsIntegerType)
			return this.MakeIntModOp(left, right, location);
		else {
			//
			// Error. Incompatible operand types of operator MOD
			//
			string dataTypeName1 = left.DataType.Name;
			string dataTypeName2 = right.DataType.Name;
			this.report.SemanticError(14, " MOD ", dataTypeName1, dataTypeName2, location);
			return Expression.Error;
		}
	}

	private Expression MakeIntGtrOp(Expression left, Expression right)
	{
		if (left.IsConstant)
		{
			long leftValue = Convert.ToInt64(left.Evaluate());
			if (right.IsConstant)
			{
				long rightValue = Convert.ToInt64(right.Evaluate());
				return this.MakeConstant(leftValue > rightValue);
			}
			else if (leftValue == 0)
				return new IntLesOperator(right, left, true);
		}
		else if (right.IsConstant)
		{
			long rightValue = Convert.ToInt64(right.Evaluate());
			if (rightValue == 0)
				return new IntGtrOperator(left, right, true);
		}
		return new IntGtrOperator(left, right);
	}

	private Expression MakeUIntGtrOp(Expression left, Expression right)
	{
		if (left.IsConstant)
		{
			ulong leftValue = Convert.ToUInt64(left.Evaluate());
			if (right.IsConstant)
			{
				ulong rightValue = Convert.ToUInt64(right.Evaluate());
				return this.MakeConstant(leftValue > rightValue);
			}
			else if (leftValue == 0)
				return new IntLesOperator(right, left, true);
		}
		else if (right.IsConstant)
		{
			ulong rightValue = Convert.ToUInt64(right.Evaluate());
			if (rightValue == 0)
				return new IntGtrOperator(left, right, true);
		}
		return new IntGtrOperator(left, right);
	}

	private Expression MakeRealGtrOp(Expression left, Expression right)
	{
		if (left.IsConstant)
		{
			Single leftValue = Convert.ToSingle(left.Evaluate());
			if (right.IsConstant)
			{
				Single rightValue = Convert.ToSingle(right.Evaluate());
				return this.MakeConstant(leftValue > rightValue);
			}
			else if (leftValue == 0.0f)
				return new FloatLesOperator(right, left, true);
		}
		else if (right.IsConstant)
		{
			Single rightValue = Convert.ToSingle(right.Evaluate());
			if (rightValue == 0.0f)
				return new FloatGtrOperator(left, right, true);
		}
		return new FloatGtrOperator(left, right);
	}

	private Expression MakeLRealGtrOp(Expression left, Expression right)
	{
		if (left.IsConstant)
		{
			double leftValue = Convert.ToDouble(left.Evaluate());
			if (right.IsConstant)
			{
				double rightValue = Convert.ToDouble(right.Evaluate());
				return this.MakeConstant(leftValue > rightValue);
			}
			else if (leftValue == 0.0d)
				return new LRealLesOperator(right, left, true);
		}
		else if (right.IsConstant)
		{
			double rightValue = Convert.ToDouble(right.Evaluate());
			if (rightValue == 0.0d)
				return new LRealGtrOperator(left, right, true);
		}
		return new LRealGtrOperator(left, right);
	}

	private Expression MakeTimeGtrOp(Expression left, Expression right)
	{
		if (! left.IsConstant || ! right.IsConstant) 
			return new TimeGtrOperator(left, right);
		else {
			TimeSpan leftValue = (TimeSpan)left.Evaluate();
			TimeSpan rightValue = (TimeSpan)right.Evaluate();
			return this.MakeConstant(leftValue > rightValue);
		}
	}

	private Expression MakeDateTimeGtrOp(Expression left, Expression right)
	{
		if (! left.IsConstant || ! right.IsConstant) 
			return new DateTimeGtrOperator(left, right);
		else {
			DateTime leftValue = (DateTime)left.Evaluate();
			DateTime rightValue = (DateTime)right.Evaluate();
			return this.MakeConstant(leftValue > rightValue);
		}
	}

	private Expression MakeStringGtrOp(Expression left, Expression right)
	{
		if (! left.IsConstant || ! right.IsConstant) 
			return new StringGtrOperator(left, right);
		else {
			string leftValue = (string)left.Evaluate();
			string rightValue = (string)right.Evaluate();
			return this.MakeConstant(leftValue.CompareTo(rightValue) > 0);
		}
	}

	private Expression MakeWStringGtrOp(Expression left, Expression right)
	{
		if (! left.IsConstant || ! right.IsConstant) 
			return new WStringGtrOperator(left, right);
		else {
			string leftValue = (string)left.Evaluate();
			string rightValue = (string)right.Evaluate();
			return this.MakeConstant(leftValue.CompareTo(rightValue) > 0);
		}
	}

	private Expression MakeTimeOfDayGtrOp(Expression left, Expression right)
	{
		if (! left.IsConstant || ! right.IsConstant)
			return new TimeOfDayGtrOperator(left, right);
		else {
			TimeSpan leftValue = (TimeSpan)left.Evaluate();
			TimeSpan rightValue = (TimeSpan)right.Evaluate();
			return this.MakeConstant(leftValue > rightValue);
		}
	}

	private Expression MakeIntLesOp(Expression left, Expression right)
	{
		if (left.IsConstant)
		{
			long leftValue = Convert.ToInt64(left.Evaluate());
			if (right.IsConstant)
			{
				long rightValue = Convert.ToInt64(right.Evaluate());
				return this.MakeConstant(leftValue < rightValue);
			}
			else if (leftValue == 0)
				return new IntGtrOperator(right, left, true);
		}
		else if (right.IsConstant)
		{
			long rightValue = Convert.ToInt64(right.Evaluate());
			if (rightValue == 0)
				return new IntLesOperator(left, right, true);
		}
		return new IntLesOperator(left, right);
	}

	private Expression MakeUIntLesOp(Expression left, Expression right)
	{
		if (left.IsConstant)
		{
			ulong leftValue = Convert.ToUInt64(left.Evaluate());
			if (right.IsConstant)
			{
				ulong rightValue = Convert.ToUInt64(right.Evaluate());
				return this.MakeConstant(leftValue < rightValue);
			}
			else if (leftValue == 0)
				return new IntGtrOperator(right, left, true);
		}
		else if (right.IsConstant)
		{
			ulong rightValue = Convert.ToUInt64(right.Evaluate());
			if (rightValue == 0)
				return new IntLesOperator(left, right, true);
		}
		return new IntLesOperator(left, right);
	}

	private Expression MakeRealLesOp(Expression left, Expression right)
	{
		if (left.IsConstant)
		{
			Single leftValue = Convert.ToSingle(left.Evaluate());
			if (right.IsConstant)
			{
				Single rightValue = Convert.ToSingle(right.Evaluate());
				return this.MakeConstant(leftValue < rightValue);
			}
			else if (leftValue == 0.0f)
				return new FloatGtrOperator(right, left, true);
		}
		else if (right.IsConstant)
		{
			Single rightValue = Convert.ToSingle(right.Evaluate());
			if (rightValue == 0.0f)
				return new FloatLesOperator(left, right, true);
		}
		return new FloatLesOperator(left, right);
	}

	private Expression MakeLRealLesOp(Expression left, Expression right)
	{
		if (left.IsConstant)
		{
			double leftValue = Convert.ToDouble(left.Evaluate());
			if (right.IsConstant)
			{
				double rightValue = Convert.ToDouble(right.Evaluate());
				return this.MakeConstant(leftValue < rightValue);
			}
			else if (leftValue == 0.0d)
				return new LRealGtrOperator(right, left, true);
		}
		else if (right.IsConstant)
		{
			double rightValue = Convert.ToDouble(right.Evaluate());
			if (rightValue == 0.0d)
				return new LRealLesOperator(left, right, true);
		}
		return new LRealLesOperator(left, right);
	}

	private Expression MakeTimeLesOp(Expression left, Expression right)
	{
		if (! left.IsConstant || ! right.IsConstant) 
			return new TimeLesOperator(left, right);
		else {
			TimeSpan leftValue = (TimeSpan)left.Evaluate();
			TimeSpan rightValue = (TimeSpan)right.Evaluate();
			return this.MakeConstant(leftValue > rightValue);
		}
	}

	private Expression MakeDateTimeLesOp(Expression left, Expression right)
	{
		if (! left.IsConstant || ! right.IsConstant) 
			return new DateTimeLesOperator(left, right);
		else {
			DateTime leftValue = (DateTime)left.Evaluate();
			DateTime rightValue = (DateTime)right.Evaluate();
			return this.MakeConstant(leftValue > rightValue);
		}
	}

	private Expression MakeStringLesOp(Expression left, Expression right)
	{
		if (! left.IsConstant || ! right.IsConstant) 
			return new StringLesOperator(left, right);
		else {
			string leftValue = (string)left.Evaluate();
			string rightValue = (string)right.Evaluate();
			return this.MakeConstant(leftValue.CompareTo(rightValue) > 0);
		}
	}

	private Expression MakeWStringLesOp(Expression left, Expression right)
	{
		if (! left.IsConstant || ! right.IsConstant) 
			return new WStringLesOperator(left, right);
		else {
			string leftValue = (string)left.Evaluate();
			string rightValue = (string)right.Evaluate();
			return this.MakeConstant(leftValue.CompareTo(rightValue) > 0);
		}
	}

	private Expression MakeTimeOfDayLesOp(Expression left, Expression right)
	{
		if (! left.IsConstant || ! right.IsConstant)
			return new TimeOfDayLesOperator(left, right);
		else {
			TimeSpan leftValue = (TimeSpan)left.Evaluate();
			TimeSpan rightValue = (TimeSpan)right.Evaluate();
			return this.MakeConstant(leftValue > rightValue);
		}
	}

	private Expression MakeUIntEqlOp(Expression left, Expression right)
	{
		if (! left.IsConstant || ! right.IsConstant) 
			return new IntEqlOperator(left, right);
		else {
			ulong leftValue = Convert.ToUInt64(left.Evaluate());
			ulong rightValue = Convert.ToUInt64(right.Evaluate());
			return this.MakeConstant(leftValue == rightValue);
		}
	}

	private Expression MakeIntEqlOp(Expression left, Expression right)
	{
		if (left.IsConstant)
		{
			long leftValue = Convert.ToInt64(left.Evaluate());
			if (right.IsConstant)
			{
				long rightValue = Convert.ToInt64(right.Evaluate());
				return this.MakeConstant(leftValue == rightValue);
			}
			else if (leftValue == 0)
				return new IntEqlOperator(right, left, true);
		}
		else if (right.IsConstant)
		{
			long rightValue = Convert.ToInt64(right.Evaluate());
			if (rightValue == 0)
				return new IntEqlOperator(left, right, true);
		}
		return new IntEqlOperator(left, right);
	}

	private Expression MakeRealEqlOp(Expression left, Expression right)
	{
		if (left.IsConstant)
		{
			Single leftValue = Convert.ToSingle(left.Evaluate());
			if (right.IsConstant)
			{
				float rightValue = Convert.ToSingle(right.Evaluate());
				return this.MakeConstant(leftValue == rightValue);
			}
			else if (leftValue == 0.0f)
				return new FloatEqlOperator(right, left, true);
		}
		else if (right.IsConstant)
		{
			Single rightValue = Convert.ToSingle(right.Evaluate());
			if (rightValue == 0.0f)
				return new FloatEqlOperator(left, right, true);
		}
		return new FloatEqlOperator(left, right);
	}

	private Expression MakeLRealEqlOp(Expression left, Expression right)
	{
		if (left.IsConstant)
		{
			double leftValue = Convert.ToDouble(left.Evaluate());
			if (right.IsConstant)
			{
				double rightValue = Convert.ToDouble(right.Evaluate());
				return this.MakeConstant(leftValue == rightValue);
			}
			else if (leftValue == 0.0d)
				return new LRealEqlOperator(right, left, true);
		}
		else if (right.IsConstant)
		{
			double rightValue = Convert.ToDouble(right.Evaluate());
			if (rightValue == 0.0d)
				return new LRealEqlOperator(left, right, true);
		}
		return new LRealEqlOperator(left, right);
	}

	private Expression MakeTimeEqlOp(Expression left, Expression right)
	{
		if (! left.IsConstant || ! right.IsConstant) 
			return new TimeEqlOperator(left, right);
		else {
			TimeSpan leftValue = (TimeSpan)left.Evaluate();
			TimeSpan rightValue = (TimeSpan)right.Evaluate();
			return this.MakeConstant(leftValue == rightValue);
		}
	}

	private Expression MakeDateTimeEqlOp(Expression left, Expression right)
	{
		if (! left.IsConstant || ! right.IsConstant) 
			return new DateTimeEqlOperator(left, right);
		else {
			DateTime leftValue = (DateTime)left.Evaluate();
			DateTime rightValue = (DateTime)right.Evaluate();
			return this.MakeConstant(leftValue == rightValue);
		}
	}

	private Expression MakeStringEqlOp(Expression left, Expression right)
	{
		if (! left.IsConstant || ! right.IsConstant) 
			return new StringEqlOperator(left, right);
		else {
			string leftValue = (string)left.Evaluate();
			string rightValue = (string)right.Evaluate();
			return this.MakeConstant(leftValue == rightValue);
		}
	}

	private Expression MakeWStringEqlOp(Expression left, Expression right)
	{
		if (! left.IsConstant || ! right.IsConstant) 
			return new WStringEqlOperator(left, right);
		else {
			string leftValue = (string)left.Evaluate();
			string rightValue = (string)right.Evaluate();
			return this.MakeConstant(leftValue == rightValue);
		}
	}

	private Expression MakeTimeOfDayEqlOp(Expression left, Expression right)
	{
		if (! left.IsConstant || ! right.IsConstant) 
			return new TimeOfDayEqlOperator(left, right);
		else {
			TimeSpan leftValue = (TimeSpan)left.Evaluate();
			TimeSpan rightValue = (TimeSpan)right.Evaluate();
			return this.MakeConstant(leftValue == rightValue);
		}
	}

	private Expression MakeIntNeqOp(Expression left, Expression right)
	{
		if (left.IsConstant)
		{
			long leftValue = Convert.ToInt64(left.Evaluate());
			if (right.IsConstant)
			{
				long rightValue = Convert.ToInt64(right.Evaluate());
				return this.MakeConstant(leftValue != rightValue);
			}
			else if (leftValue == 0)
				return new IntNeqOperator(right, left, true);
		}
		else if (right.IsConstant)
		{
			long rightValue = Convert.ToInt64(right.Evaluate());
			if (rightValue == 0)
				return new IntNeqOperator(left, right, true);
		}
		return new IntNeqOperator(left, right);
	}

	private Expression MakeUIntNeqOp(Expression left, Expression right)
	{
		if (left.IsConstant)
		{
			ulong leftValue = Convert.ToUInt64(left.Evaluate());
			if (right.IsConstant)
			{
				ulong rightValue = Convert.ToUInt64(right.Evaluate());
				return this.MakeConstant(leftValue != rightValue);
			}
			else if (leftValue == 0)
				return new IntNeqOperator(right, left, true);
		}
		else if (right.IsConstant)
		{
			ulong rightValue = Convert.ToUInt64(right.Evaluate());
			if (rightValue == 0)
				return new IntNeqOperator(left, right, true);
		}
		return new IntNeqOperator(left, right);
	}

	private Expression MakeRealNeqOp(Expression left, Expression right)
	{
		if (left.IsConstant)
		{
			Single leftValue = Convert.ToSingle(left.Evaluate());
			if (right.IsConstant)
			{
				float rightValue = Convert.ToSingle(right.Evaluate());
				return this.MakeConstant(leftValue != rightValue);
			}
			else if (leftValue == 0.0f)
				return new FloatNeqOperator(right, left, true);
		}
		else if (right.IsConstant)
		{
			Single rightValue = Convert.ToSingle(right.Evaluate());
			if (rightValue == 0.0f)
				return new FloatNeqOperator(left, right, true);
		}
		return new FloatNeqOperator(left, right);
	}

	private Expression MakeLRealNeqOp(Expression left, Expression right)
	{
		if (left.IsConstant)
		{
			double leftValue = Convert.ToDouble(left.Evaluate());
			if (right.IsConstant)
			{
				double rightValue = Convert.ToDouble(right.Evaluate());
				return this.MakeConstant(leftValue != rightValue);
			}
			else if (leftValue == 0.0d)
				return new LRealNeqOperator(right, left, true);
		}
		else if (right.IsConstant)
		{
			double rightValue = Convert.ToDouble(right.Evaluate());
			if (rightValue == 0.0d)
				return new LRealNeqOperator(left, right, true);
		}
		return new LRealNeqOperator(left, right);
	}

	private Expression MakeTimeNeqOp(Expression left, Expression right)
	{
		if (! left.IsConstant || ! right.IsConstant) 
			return new TimeNeqOperator(left, right);
		else {
			TimeSpan leftValue = (TimeSpan)left.Evaluate();
			TimeSpan rightValue = (TimeSpan)right.Evaluate();
			return this.MakeConstant(leftValue != rightValue);
		}
	}

	private Expression MakeDateTimeNeqOp(Expression left, Expression right)
	{
		if (! left.IsConstant || ! right.IsConstant) 
			return new DateTimeNeqOperator(left, right);
		else {
			DateTime leftValue = (DateTime)left.Evaluate();
			DateTime rightValue = (DateTime)right.Evaluate();
			return this.MakeConstant(leftValue != rightValue);
		}
	}

	private Expression MakeStringNeqOp(Expression left, Expression right)
	{
		if (! left.IsConstant || ! right.IsConstant) 
			return new StringNeqOperator(left, right);
		else {
			string leftValue = (string)left.Evaluate();
			string rightValue = (string)right.Evaluate();
			return this.MakeConstant(leftValue.CompareTo(rightValue) != 0);
		}
	}

	private Expression MakeWStringNeqOp(Expression left, Expression right)
	{
		if (! left.IsConstant || ! right.IsConstant) 
			return new WStringNeqOperator(left, right);
		else {
			string leftValue = (string)left.Evaluate();
			string rightValue = (string)right.Evaluate();
			return this.MakeConstant(leftValue.CompareTo(rightValue) != 0);
		}
	}

	private Expression MakeTimeOfDayNeqOp(Expression left, Expression right)
	{
		if (! left.IsConstant || ! right.IsConstant)
			return new TimeOfDayGeqOperator(left, right);
		else {
			TimeSpan leftValue = (TimeSpan)left.Evaluate();
			TimeSpan rightValue = (TimeSpan)right.Evaluate();
			return this.MakeConstant(leftValue != rightValue);
		}
	}

	private Expression MakeIntGeqOp(Expression left, Expression right)
	{
		if (left.IsConstant)
		{
			long leftValue = Convert.ToInt64(left.Evaluate());
			if (right.IsConstant)
			{
				long rightValue = Convert.ToInt64(right.Evaluate());
				return this.MakeConstant(leftValue >= rightValue);
			}
			else if (leftValue == 0)
				return new IntGeqOperator(right, left, true);
		}
		else if (right.IsConstant)
		{
			long rightValue = Convert.ToInt64(right.Evaluate());
			if (rightValue == 0)
				return new IntGeqOperator(left, right, true);
		}
		return new IntGeqOperator(left, right);
	}

	private Expression MakeUIntGeqOp(Expression left, Expression right)
	{
		if (left.IsConstant)
		{
			ulong leftValue = Convert.ToUInt64(left.Evaluate());
			if (right.IsConstant)
			{
				ulong rightValue = Convert.ToUInt64(right.Evaluate());
				return this.MakeConstant(leftValue >= rightValue);
			}
			else if (leftValue == 0)
				return new IntLeqOperator(right, left, true);
		}
		else if (right.IsConstant)
		{
			ulong rightValue = Convert.ToUInt64(right.Evaluate());
			if (rightValue == 0)
				return new IntGeqOperator(left, right, true);
		}
		return new IntGeqOperator(left, right);
	}

	private Expression MakeRealGeqOp(Expression left, Expression right)
	{
		if (left.IsConstant)
		{
			Single leftValue = Convert.ToSingle(left.Evaluate());
			if (right.IsConstant)
			{
				float rightValue = Convert.ToSingle(right.Evaluate());
				return this.MakeConstant(leftValue >= rightValue);
			}
			else if (leftValue == 0.0f)
				return new FloatLeqOperator(right, left, true);
		}
		else if (right.IsConstant)
		{
			Single rightValue = Convert.ToSingle(right.Evaluate());
			if (rightValue == 0.0f)
				return new FloatGeqOperator(left, right, true);
		}
		return new FloatGeqOperator(left, right);
	}

	private Expression MakeLRealGeqOp(Expression left, Expression right)
	{
		if (left.IsConstant)
		{
			double leftValue = Convert.ToDouble(left.Evaluate());
			if (right.IsConstant)
			{
				double rightValue = Convert.ToDouble(right.Evaluate());
				return this.MakeConstant(leftValue >= rightValue);
			}
			else if (leftValue == 0.0d)
				return new LRealLeqOperator(right, left, true);
		}
		else if (right.IsConstant)
		{
			double rightValue = Convert.ToDouble(right.Evaluate());
			if (rightValue == 0.0d)
				return new LRealGeqOperator(left, right, true);
		}
		return new LRealGeqOperator(left, right);
	}

	private Expression MakeTimeGeqOp(Expression left, Expression right)
	{
		if (! left.IsConstant || ! right.IsConstant) 
			return new TimeGeqOperator(left, right);
		else {
			TimeSpan leftValue = (TimeSpan)left.Evaluate();
			TimeSpan rightValue = (TimeSpan)right.Evaluate();
			return this.MakeConstant(leftValue >= rightValue);
		}
	}

	private Expression MakeDateTimeGeqOp(Expression left, Expression right)
	{
		if (! left.IsConstant || ! right.IsConstant) 
			return new DateTimeGeqOperator(left, right);
		else {
			DateTime leftValue = (DateTime)left.Evaluate();
			DateTime rightValue = (DateTime)right.Evaluate();
			return this.MakeConstant(leftValue >= rightValue);
		}
	}

	private Expression MakeStringGeqOp(Expression left, Expression right)
	{
		if (! left.IsConstant || ! right.IsConstant) 
			return new StringGeqOperator(left, right);
		else {
			string leftValue = (string)left.Evaluate();
			string rightValue = (string)right.Evaluate();
			return this.MakeConstant(leftValue.CompareTo(rightValue) >= 0);
		}
	}
	
	private Expression MakeWStringGeqOp(Expression left, Expression right)
	{
		if (! left.IsConstant || ! right.IsConstant) 
			return new WStringGeqOperator(left, right);
		else {
			string leftValue = (string)left.Evaluate();
			string rightValue = (string)right.Evaluate();
			return this.MakeConstant(leftValue.CompareTo(rightValue) >= 0);
		}
	}


	private Expression MakeTimeOfDayGeqOp(Expression left, Expression right)
	{
		if (! left.IsConstant || ! right.IsConstant)
			return new TimeOfDayGeqOperator(left, right);
		else {
			TimeSpan leftValue = (TimeSpan)left.Evaluate();
			TimeSpan rightValue = (TimeSpan)right.Evaluate();
			return this.MakeConstant(leftValue >= rightValue);
		}
	}

	private Expression MakeIntLeqOp(Expression left, Expression right)
	{
		if (left.IsConstant)
		{
			long leftValue = Convert.ToInt64(left.Evaluate());
			if (right.IsConstant)
			{
				long rightValue = Convert.ToInt64(right.Evaluate());
				return this.MakeConstant(leftValue <= rightValue);
			}
			else if (leftValue == 0)
				return new IntGeqOperator(right, left, true);
		}
		else if (right.IsConstant)
		{
			long rightValue = Convert.ToInt64(right.Evaluate());
			if (rightValue == 0)
				return new IntLeqOperator(left, right, true);
		}
		return new IntLeqOperator(left, right);
	}

	private Expression MakeUIntLeqOp(Expression left, Expression right)
	{
		if (left.IsConstant)
		{
			ulong leftValue = Convert.ToUInt64(left.Evaluate());
			if (right.IsConstant)
			{
				ulong rightValue = Convert.ToUInt64(right.Evaluate());
				return this.MakeConstant(leftValue <= rightValue);
			}
			else if (leftValue == 0)
				return new IntGeqOperator(right, left, true);
		}
		else if (right.IsConstant)
		{
			ulong rightValue = Convert.ToUInt64(right.Evaluate());
			if (rightValue == 0)
				return new IntLeqOperator(left, right, true);
		}
		return new IntLeqOperator(left, right);
	}

	private Expression MakeRealLeqOp(Expression left, Expression right)
	{
		if (left.IsConstant)
		{
			Single leftValue = Convert.ToSingle(left.Evaluate());
			if (right.IsConstant)
			{
				float rightValue = Convert.ToSingle(right.Evaluate());
				return this.MakeConstant(leftValue <= rightValue);
			}
			else if (leftValue == 0.0f)
				return new FloatGeqOperator(right, left, true);
		}
		else if (right.IsConstant)
		{
			Single rightValue = Convert.ToSingle(right.Evaluate());
			if (rightValue == 0.0f)
				return new FloatLeqOperator(left, right, true);
		}
		return new FloatLeqOperator(left, right);
	}

	private Expression MakeLRealLeqOp(Expression left, Expression right)
	{
		if (left.IsConstant)
		{
			double leftValue = Convert.ToDouble(left.Evaluate());
			if (right.IsConstant)
			{
				double rightValue = Convert.ToDouble(right.Evaluate());
				return this.MakeConstant(leftValue <= rightValue);
			}
			else if (leftValue == 0.0d)
				return new LRealGeqOperator(right, left, true);
		}
		else if (right.IsConstant)
		{
			double rightValue = Convert.ToDouble(right.Evaluate());
			if (rightValue == 0.0d)
				return new LRealLeqOperator(left, right, true);
		}
		return new LRealLeqOperator(left, right);
	}

	private Expression MakeTimeLeqOp(Expression left, Expression right)
	{
		if (! left.IsConstant || ! right.IsConstant) 
			return new TimeLeqOperator(left, right);
		else {
			TimeSpan leftValue = (TimeSpan)left.Evaluate();
			TimeSpan rightValue = (TimeSpan)right.Evaluate();
			return this.MakeConstant(leftValue <= rightValue);
		}
	}

	private Expression MakeDateTimeLeqOp(Expression left, Expression right)
	{
		if (! left.IsConstant || ! right.IsConstant) 
			return new DateTimeLeqOperator(left, right);
		else {
			DateTime leftValue = (DateTime)left.Evaluate();
			DateTime rightValue = (DateTime)right.Evaluate();
			return this.MakeConstant(leftValue <= rightValue);
		}
	}

	private Expression MakeStringLeqOp(Expression left, Expression right)
	{
		if (! left.IsConstant || ! right.IsConstant) 
			return new StringLeqOperator(left, right);
		else {
			string leftValue = (string)left.Evaluate();
			string rightValue = (string)right.Evaluate();
			return this.MakeConstant(leftValue.CompareTo(rightValue) <= 0);
		}
	}

	private Expression MakeWStringLeqOp(Expression left, Expression right)
	{
		if (! left.IsConstant || ! right.IsConstant) 
			return new WStringLeqOperator(left, right);
		else {
			string leftValue = (string)left.Evaluate();
			string rightValue = (string)right.Evaluate();
			return this.MakeConstant(leftValue.CompareTo(rightValue) <= 0);
		}
	}

	private Expression MakeTimeOfDayLeqOp(Expression left, Expression right)
	{
		if (! left.IsConstant || ! right.IsConstant)
			return new TimeOfDayLeqOperator(left, right);
		else {
			TimeSpan leftValue = (TimeSpan)left.Evaluate();
			TimeSpan rightValue = (TimeSpan)right.Evaluate();
			return this.MakeConstant(leftValue <= rightValue);
		}
	}

	private Expression MakeGtrOperator(Expression left, Expression right, LexLocation location)
	{
		if (left == null || right == null)
			return Expression.Error;
		else if (left is FunctionName)
		{
			this.report.SemanticError(148, left.ToString(), location);
			return Expression.Error;
		}
		else if (right is FunctionName)
		{
			this.report.SemanticError(148, right.ToString(), location);
			return Expression.Error;
		}
		else if (left.DataType == TypeNode.Error || right.DataType == TypeNode.Error)
			return Expression.Error;
		else {
			TypeNode leftDataType = left.DataType;
			TypeNode rightDataType = right.DataType;
			if (leftDataType.IsSignedIntType)
			{
				if (rightDataType.IsUnsignedIntType)
				{
					this.report.Warning(9, ">", location);
					return this.MakeIntGtrOp(left, right);
				}
				else if (rightDataType.IsSignedIntType)
					return this.MakeIntGtrOp(left, right);
				else if (rightDataType == TypeNode.Real)
					return this.MakeRealGtrOp(this.MakeInt2Real(left), right);
				else if (rightDataType == TypeNode.LReal)
					return this.MakeLRealGtrOp(this.MakeInt2LReal(left), right);

			}
			else if (leftDataType.IsUnsignedIntType)
			{
				if (rightDataType.IsSignedIntType)
				{
					this.report.Warning(9, ">", location);
					return this.MakeUIntGtrOp(left, right);
				}
				else if (rightDataType.IsUnsignedIntType)
					return this.MakeUIntGtrOp(left, right);
				else if (rightDataType == TypeNode.Real)
					return this.MakeRealGtrOp(this.MakeInt2Real(left), right);
				else if (rightDataType == TypeNode.LReal)
					return this.MakeLRealGtrOp(this.MakeInt2LReal(left), right);
			}
			else if (leftDataType == TypeNode.Real)
			{
				if (rightDataType == TypeNode.Real)
					return this.MakeRealGtrOp(left, right);
				else if (rightDataType == TypeNode.LReal)
					return this.MakeLRealGtrOp(this.MakeReal2LReal(left), right);
				else if (rightDataType.IsIntegerType)
					return this.MakeLRealGtrOp(left, this.MakeInt2Real(right));
			}
			else if (leftDataType == TypeNode.LReal)
			{
				if (rightDataType == TypeNode.LReal)
					return this.MakeLRealGtrOp(left, right);
				else if (rightDataType.IsIntegerType)
					return this.MakeLRealGtrOp(left, this.MakeInt2LReal(right));
				else if (rightDataType == TypeNode.Real)
					return this.MakeLRealGtrOp(left, this.MakeReal2LReal(right));
			}
			else if (leftDataType.IsBitStringType)
			{
				if (rightDataType.IsBitStringType)
					return this.MakeUIntGtrOp(left, right);
				else if (rightDataType.IsIntegerType && right.IsConstant)
					return this.MakeUIntGtrOp(left, right);
			}
			else if (leftDataType == TypeNode.Time && rightDataType == TypeNode.Time)
				return this.MakeTimeGtrOp(left, right);
			else if (leftDataType == TypeNode.TimeOfDay && rightDataType == TypeNode.TimeOfDay)
				return this.MakeTimeOfDayGtrOp(left, right);
			else if (leftDataType == TypeNode.Date && rightDataType == TypeNode.Date)
				return this.MakeDateTimeGtrOp(left, right);
			else if (leftDataType == TypeNode.DateAndTime && rightDataType == TypeNode.DateAndTime)
				return this.MakeDateTimeGtrOp(left, right);
			else if (leftDataType.IsStringType && rightDataType.IsStringType)
				return this.MakeStringGtrOp(left, right);
			else if (leftDataType.IsWStringType && rightDataType.IsWStringType)
				return this.MakeWStringGtrOp(left, right);
			//
			// Error. Incompatible operand types of operator >
			//
			string dataTypeName1 = leftDataType.Name;
			string dataTypeName2 = rightDataType.Name;
			this.report.SemanticError(14, " > ", dataTypeName1, dataTypeName2, location);
			return Expression.Error;
		}
	}

	private Expression MakeLesOperator(Expression left, Expression right, LexLocation location)
	{
		if (left == null || right == null)
			return Expression.Error;
		else if (left is FunctionName)
		{
			this.report.SemanticError(148, left.ToString(), location);
			return Expression.Error;
		}
		else if (right is FunctionName)
		{
			this.report.SemanticError(148, right.ToString(), location);
			return Expression.Error;
		}
		else if (left.DataType == TypeNode.Error || right.DataType == TypeNode.Error)
			return Expression.Error;
		else {
			TypeNode leftDataType = left.DataType;
			TypeNode rightDataType = right.DataType;
			if (leftDataType.IsSignedIntType)
			{
				if (rightDataType.IsUnsignedIntType)
				{
					this.report.Warning(9, "<", location);
					return this.MakeIntLesOp(left, right);
				}
				else if (rightDataType.IsSignedIntType)
					return this.MakeIntLesOp(left, right);
				else if (rightDataType == TypeNode.Real)
					return this.MakeRealLesOp(this.MakeInt2Real(left), right);
				else if (rightDataType == TypeNode.LReal)
					return this.MakeLRealLesOp(this.MakeInt2LReal(left), right);

			}
			else if (leftDataType.IsUnsignedIntType)
			{
				if (rightDataType.IsSignedIntType)
				{
					this.report.Warning(9, "<", location);
					return this.MakeUIntLesOp(left, right);
				}
				else if (rightDataType.IsUnsignedIntType)
					return this.MakeUIntLesOp(left, right);
				else if (rightDataType == TypeNode.Real)
					return this.MakeRealLesOp(this.MakeInt2Real(left), right);
				else if (rightDataType == TypeNode.LReal)
					return this.MakeLRealLesOp(this.MakeInt2LReal(left), right);
			}
			else if (leftDataType == TypeNode.Real)
			{
				if (rightDataType == TypeNode.Real)
					return this.MakeRealLesOp(left, right);
				else if (rightDataType == TypeNode.LReal)
					return this.MakeLRealLesOp(this.MakeReal2LReal(left), right);
				else if (rightDataType.IsIntegerType)
					return this.MakeLRealLesOp(left, this.MakeInt2Real(right));
			}
			else if (leftDataType == TypeNode.LReal)
			{
				if (rightDataType == TypeNode.LReal)
					return this.MakeLRealLesOp(left, right);
				else if (rightDataType.IsIntegerType)
					return this.MakeLRealLesOp(left, this.MakeInt2LReal(right));
				else if (rightDataType == TypeNode.Real)
					return this.MakeLRealLesOp(left, this.MakeReal2LReal(right));
			}
			else if (leftDataType.IsBitStringType)
			{
				if (rightDataType.IsBitStringType)
					return this.MakeUIntLesOp(left, right);
				else if (rightDataType.IsIntegerType && right.IsConstant)
					return this.MakeUIntLesOp(left, right);
			}
			else if (leftDataType == TypeNode.Time && rightDataType == TypeNode.Time)
				return this.MakeTimeLesOp(left, right);
			else if (leftDataType == TypeNode.TimeOfDay && rightDataType == TypeNode.TimeOfDay)
				return this.MakeTimeOfDayLesOp(left, right);
			else if (leftDataType == TypeNode.Date && rightDataType == TypeNode.Date)
				return this.MakeDateTimeLesOp(left, right);
			else if (leftDataType == TypeNode.DateAndTime && rightDataType == TypeNode.DateAndTime)
				return this.MakeDateTimeLesOp(left, right);
			else if (leftDataType.IsStringType && rightDataType.IsStringType)
				return this.MakeStringLesOp(left, right);
			else if (leftDataType.IsWStringType && rightDataType.IsWStringType)
				return this.MakeWStringLesOp(left, right);
			//
			// Error. Incompatible operand types of operator <
			//
			string dataTypeName1 = leftDataType.Name;
			string dataTypeName2 = rightDataType.Name;
			this.report.SemanticError(14, " < ", dataTypeName1, dataTypeName2, location);
			return Expression.Error;
		}
	}

	private Expression MakeEqlOperator(Expression left, Expression right, LexLocation location)
	{
		if (left == null || right == null)
			return Expression.Error;
		else if (left is FunctionName)
		{
			this.report.SemanticError(148, left.ToString(), location);
			return Expression.Error;
		}
		else if (right is FunctionName)
		{
			this.report.SemanticError(148, right.ToString(), location);
			return Expression.Error;
		}
		else if (left.DataType == TypeNode.Error || right.DataType == TypeNode.Error)
			return Expression.Error;
		else {
			TypeNode leftDataType = left.DataType;
			TypeNode rightDataType = right.DataType;
			if (leftDataType.IsSignedIntType)
			{
				if (rightDataType.IsSignedIntType)
					return this.MakeIntEqlOp(left, right);
				else if (rightDataType == TypeNode.Real)
					return this.MakeRealEqlOp(this.MakeInt2Real(left), right);
				else if (rightDataType == TypeNode.LReal)
					return this.MakeLRealEqlOp(this.MakeInt2LReal(left), right);
				else if (rightDataType.IsUnsignedIntType)
				{
					this.report.Warning(9, "=", location);
					return this.MakeIntEqlOp(left, right);
				}
			}
			else if (leftDataType.IsUnsignedIntType)
			{
				if (rightDataType.IsUnsignedIntType)
					return this.MakeIntEqlOp(left, right);
				else if (rightDataType == TypeNode.Real)
					return this.MakeRealEqlOp(this.MakeInt2Real(left), right);
				else if (rightDataType == TypeNode.LReal)
					return this.MakeLRealEqlOp(this.MakeInt2LReal(left), right);
				else if (rightDataType.IsSignedIntType)
				{
					this.report.Warning(9, "=", location);
					return this.MakeIntEqlOp(left, right);
				}
			}
			else if (leftDataType == TypeNode.Real)
			{
				if (rightDataType == TypeNode.Real)
					return this.MakeRealEqlOp(left, right);
				else if (rightDataType == TypeNode.LReal)
					return this.MakeLRealEqlOp(this.MakeReal2LReal(left), right);
				else if (rightDataType.IsIntegerType)
					return this.MakeLRealEqlOp(left, this.MakeInt2Real(right));
			}
			else if (leftDataType == TypeNode.LReal)
			{
				if (rightDataType == TypeNode.LReal)
					return this.MakeLRealEqlOp(left, right);
				else if (rightDataType.IsIntegerType)
					return this.MakeLRealEqlOp(left, this.MakeInt2LReal(right));
				else if (rightDataType == TypeNode.Real)
					return this.MakeLRealEqlOp(left, this.MakeReal2LReal(right));
			}
			else if (leftDataType.IsBitStringType)
			{
				if (rightDataType.IsBitStringType)
					return this.MakeUIntEqlOp(left, right);
				else if (rightDataType.IsIntegerType && right.IsConstant)
					return this.MakeUIntEqlOp(left, right);
			}
			else if (leftDataType == TypeNode.Time && rightDataType == TypeNode.Time)
				return this.MakeTimeEqlOp(left, right);
			else if (leftDataType == TypeNode.TimeOfDay && rightDataType == TypeNode.TimeOfDay)
				return this.MakeTimeOfDayEqlOp(left, right);
			else if (leftDataType == TypeNode.Date && rightDataType == TypeNode.Date)
				return this.MakeDateTimeEqlOp(left, right);
			else if (leftDataType == TypeNode.DateAndTime && rightDataType == TypeNode.DateAndTime)
				return this.MakeDateTimeEqlOp(left, right);
			else if (leftDataType.IsStringType && rightDataType.IsStringType)
				return this.MakeStringEqlOp(left, right);
			else if (leftDataType.IsWStringType && rightDataType.IsWStringType)
				return this.MakeWStringEqlOp(left, right);
			//
			// Error. Incompatible operand types of operator =
			//
			string dataTypeName1 = leftDataType.Name;
			string dataTypeName2 = rightDataType.Name;
			this.report.SemanticError(14, " = ", dataTypeName1, dataTypeName2, location);
			return Expression.Error;
		}
	}

	private Expression MakeNeqOperator(Expression left, Expression right, LexLocation location)
	{
		if (left == null || right == null)
			return Expression.Error;
		else if (left is FunctionName)
		{
			this.report.SemanticError(148, left.ToString(), location);
			return Expression.Error;
		}
		else if (right is FunctionName)
		{
			this.report.SemanticError(148, right.ToString(), location);
			return Expression.Error;
		}
		else if (left.DataType == TypeNode.Error || right.DataType == TypeNode.Error)
			return Expression.Error;
		else {
			TypeNode leftDataType = left.DataType;
			TypeNode rightDataType = right.DataType;
			if (leftDataType.IsSignedIntType)
			{
				if (rightDataType.IsUnsignedIntType)
				{
					this.report.Warning(9, "<>", location);
					return this.MakeIntNeqOp(left, right);
				}
				else if (rightDataType.IsSignedIntType)
					return this.MakeIntNeqOp(left, right);
				else if (rightDataType == TypeNode.Real)
					return this.MakeRealNeqOp(this.MakeInt2Real(left), right);
				else if (rightDataType == TypeNode.LReal)
					return this.MakeLRealNeqOp(this.MakeInt2LReal(left), right);

			}
			else if (leftDataType.IsUnsignedIntType)
			{
				if (rightDataType.IsSignedIntType)
				{
					this.report.Warning(9, "<>", location);
					return this.MakeUIntNeqOp(left, right);
				}
				else if (rightDataType.IsUnsignedIntType)
					return this.MakeUIntNeqOp(left, right);
				else if (rightDataType == TypeNode.Real)
					return this.MakeRealNeqOp(this.MakeInt2Real(left), right);
				else if (rightDataType == TypeNode.LReal)
					return this.MakeLRealNeqOp(this.MakeInt2LReal(left), right);
			}
			else if (leftDataType == TypeNode.Real)
			{
				if (rightDataType == TypeNode.Real)
					return this.MakeRealNeqOp(left, right);
				else if (rightDataType == TypeNode.LReal)
					return this.MakeLRealNeqOp(this.MakeReal2LReal(left), right);
				else if (rightDataType.IsIntegerType)
					return this.MakeLRealNeqOp(left, this.MakeInt2Real(right));
			}
			else if (leftDataType == TypeNode.LReal)
			{
				if (rightDataType == TypeNode.LReal)
					return this.MakeLRealNeqOp(left, right);
				else if (rightDataType.IsIntegerType)
					return this.MakeLRealNeqOp(left, this.MakeInt2LReal(right));
				else if (rightDataType == TypeNode.Real)
					return this.MakeLRealNeqOp(left, this.MakeReal2LReal(right));
			}
			else if (leftDataType.IsBitStringType)
			{
				if (rightDataType.IsBitStringType)
					return this.MakeUIntNeqOp(left, right);
				else if (rightDataType.IsIntegerType && right.IsConstant)
					return this.MakeUIntNeqOp(left, right);
			}
			else if (leftDataType == TypeNode.Time && rightDataType == TypeNode.Time)
				return this.MakeTimeNeqOp(left, right);
			else if (leftDataType == TypeNode.TimeOfDay && rightDataType == TypeNode.TimeOfDay)
				return this.MakeTimeOfDayNeqOp(left, right);
			else if (leftDataType == TypeNode.Date && rightDataType == TypeNode.Date)
				return this.MakeDateTimeNeqOp(left, right);
			else if (leftDataType == TypeNode.DateAndTime && rightDataType == TypeNode.DateAndTime)
				return this.MakeDateTimeNeqOp(left, right);
			else if (leftDataType.IsStringType && rightDataType.IsStringType)
				return this.MakeStringNeqOp(left, right);
		    else if (leftDataType.IsWStringType && rightDataType.IsWStringType)
				return this.MakeWStringNeqOp(left, right);
			//
			// Error. Incompatible operand types of operator <>
			//
			string dataTypeName1 = leftDataType.Name;
			string dataTypeName2 = rightDataType.Name;
			this.report.SemanticError(14, " <> ", dataTypeName1, dataTypeName2, location);
			return Expression.Error;
		}
	}

	private Expression MakeGeqOperator(Expression left, Expression right, LexLocation location)
	{
		if (left == null || right == null)
			return Expression.Error;
		else if (left is FunctionName)
		{
			this.report.SemanticError(148, left.ToString(), location);
			return Expression.Error;
		}
		else if (right is FunctionName)
		{
			this.report.SemanticError(148, right.ToString(), location);
			return Expression.Error;
		}
		else if (left.DataType == TypeNode.Error || right.DataType == TypeNode.Error)
			return Expression.Error;
		else {
			TypeNode leftDataType = left.DataType;
			TypeNode rightDataType = right.DataType;
			if (leftDataType.IsSignedIntType)
			{
				if (rightDataType.IsUnsignedIntType)
				{
					this.report.Warning(9, ">=", location);
					return this.MakeIntGeqOp(left, right);
				}
				else if (rightDataType.IsSignedIntType)
					return this.MakeIntGeqOp(left, right);
				else if (rightDataType == TypeNode.Real)
					return this.MakeRealGeqOp(this.MakeInt2Real(left), right);
				else if (rightDataType == TypeNode.LReal)
					return this.MakeLRealGeqOp(this.MakeInt2LReal(left), right);

			}
			else if (leftDataType.IsUnsignedIntType)
			{
				if (rightDataType.IsSignedIntType)
				{
					this.report.Warning(9, "<>", location);
					return this.MakeUIntGeqOp(left, right);
				}
				else if (rightDataType.IsUnsignedIntType)
					return this.MakeUIntGeqOp(left, right);
				else if (rightDataType == TypeNode.Real)
					return this.MakeRealGeqOp(this.MakeInt2Real(left), right);
				else if (rightDataType == TypeNode.LReal)
					return this.MakeLRealGeqOp(this.MakeInt2LReal(left), right);
			}
			else if (leftDataType == TypeNode.Real)
			{
				if (rightDataType == TypeNode.Real)
					return this.MakeRealGeqOp(left, right);
				else if (rightDataType == TypeNode.LReal)
					return this.MakeLRealGeqOp(this.MakeReal2LReal(left), right);
				else if (rightDataType.IsIntegerType)
					return this.MakeLRealGeqOp(left, this.MakeInt2Real(right));
			}
			else if (leftDataType == TypeNode.LReal)
			{
				if (rightDataType == TypeNode.LReal)
					return this.MakeLRealGeqOp(left, right);
				else if (rightDataType.IsIntegerType)
					return this.MakeLRealGeqOp(left, this.MakeInt2LReal(right));
				else if (rightDataType == TypeNode.Real)
					return this.MakeLRealGeqOp(left, this.MakeReal2LReal(right));
			}
			else if (leftDataType.IsBitStringType)
			{
				if (rightDataType.IsBitStringType)
					return this.MakeUIntGeqOp(left, right);
				else if (rightDataType.IsIntegerType && right.IsConstant)
					return this.MakeUIntGeqOp(left, right);
			}
			else if (leftDataType == TypeNode.Time && rightDataType == TypeNode.Time)
				return this.MakeTimeGeqOp(left, right);
			else if (leftDataType == TypeNode.TimeOfDay && rightDataType == TypeNode.TimeOfDay)
				return this.MakeTimeOfDayGeqOp(left, right);
			else if (leftDataType == TypeNode.Date && rightDataType == TypeNode.Date)
				return this.MakeDateTimeGeqOp(left, right);
			else if (leftDataType == TypeNode.DateAndTime && rightDataType == TypeNode.DateAndTime)
				return this.MakeDateTimeGeqOp(left, right);
			else if (leftDataType.IsStringType && rightDataType.IsStringType)
				return this.MakeStringGeqOp(left, right);
			else if (leftDataType.IsWStringType && rightDataType.IsWStringType)
				return this.MakeWStringGeqOp(left, right);
			//
			// Error. Incompatible operand types of operator >=
			//
			string dataTypeName1 = leftDataType.Name;
			string dataTypeName2 = rightDataType.Name;
			this.report.SemanticError(14, " >= ", dataTypeName1, dataTypeName2, location);
			return Expression.Error;
		}
	}

	private Expression MakeLeqOperator(Expression left, Expression right, LexLocation location)
	{
		if (left == null || right == null)
			return Expression.Error;
		else if (left is FunctionName)
		{
			this.report.SemanticError(148, left.ToString(), location);
			return Expression.Error;
		}
		else if (right is FunctionName)
		{
			this.report.SemanticError(148, right.ToString(), location);
			return Expression.Error;
		}
		else if (left.DataType == TypeNode.Error || right.DataType == TypeNode.Error)
			return Expression.Error;
		else {
			TypeNode leftDataType = left.DataType;
			TypeNode rightDataType = right.DataType;
			if (leftDataType.IsSignedIntType)
			{
				if (rightDataType.IsUnsignedIntType)
				{
					this.report.Warning(9, ">=", location);
					return this.MakeIntLeqOp(left, right);
				}
				else if (rightDataType.IsSignedIntType)
					return this.MakeIntLeqOp(left, right);
				else if (rightDataType == TypeNode.Real)
					return this.MakeRealLeqOp(this.MakeInt2Real(left), right);
				else if (rightDataType == TypeNode.LReal)
					return this.MakeLRealLeqOp(this.MakeInt2LReal(left), right);

			}
			else if (leftDataType.IsUnsignedIntType)
			{
				if (rightDataType.IsSignedIntType)
				{
					this.report.Warning(9, "<>", location);
					return this.MakeUIntLeqOp(left, right);
				}
				else if (rightDataType.IsUnsignedIntType)
					return this.MakeUIntLeqOp(left, right);
				else if (rightDataType == TypeNode.Real)
					return this.MakeRealLeqOp(this.MakeInt2Real(left), right);
				else if (rightDataType == TypeNode.LReal)
					return this.MakeLRealLeqOp(this.MakeInt2LReal(left), right);
			}
			else if (leftDataType == TypeNode.Real)
			{
				if (rightDataType == TypeNode.Real)
					return this.MakeRealLeqOp(left, right);
				else if (rightDataType == TypeNode.LReal)
					return this.MakeLRealLeqOp(this.MakeReal2LReal(left), right);
				else if (rightDataType.IsIntegerType)
					return this.MakeLRealLeqOp(left, this.MakeInt2Real(right));
			}
			else if (leftDataType == TypeNode.LReal)
			{
				if (rightDataType == TypeNode.LReal)
					return this.MakeLRealLeqOp(left, right);
				else if (rightDataType.IsIntegerType)
					return this.MakeLRealLeqOp(left, this.MakeInt2LReal(right));
				else if (rightDataType == TypeNode.Real)
					return this.MakeLRealLeqOp(left, this.MakeReal2LReal(right));
			}
			else if (leftDataType.IsBitStringType)
			{
				if (rightDataType.IsBitStringType)
					return this.MakeUIntLeqOp(left, right);
				else if (rightDataType.IsIntegerType && right.IsConstant)
					return this.MakeUIntLeqOp(left, right);
			}
			else if (leftDataType == TypeNode.Time && rightDataType == TypeNode.Time)
				return this.MakeTimeLeqOp(left, right);
			else if (leftDataType == TypeNode.TimeOfDay && rightDataType == TypeNode.TimeOfDay)
				return this.MakeTimeOfDayLeqOp(left, right);
			else if (leftDataType == TypeNode.Date && rightDataType == TypeNode.Date)
				return this.MakeDateTimeLeqOp(left, right);
			else if (leftDataType == TypeNode.DateAndTime && rightDataType == TypeNode.DateAndTime)
				return this.MakeDateTimeLeqOp(left, right);
			else if (leftDataType.IsStringType && rightDataType.IsStringType)
				return this.MakeStringLeqOp(left, right);
			else if (leftDataType.IsWStringType && rightDataType.IsWStringType)
				return this.MakeWStringLeqOp(left, right);
			//
			// Error. Incompatible operand types of operator <=
			//
			string dataTypeName1 = leftDataType.Name;
			string dataTypeName2 = rightDataType.Name;
			this.report.SemanticError(14, " <= ", dataTypeName1, dataTypeName2, location);
			return Expression.Error;
		}
	}

	private ProgramOrganizationUnitCall MakePOUCall(string name, LexLocation location)
	{
		STLangSymbol symbol;
		if (! this.symbolTable.Lookup(name, out symbol, location))
		{
			STLangSymbol undefinedPOU;
			this.report.SemanticError(0, name, location);
			undefinedPOU = this.symbolTable.InstallUndefinedFunction(name, 10);
			return new STLangFunctionCall(undefinedPOU, location);
		}
		else if (symbol.IsFunction)
			return new STLangFunctionCall(symbol, location);
		else if (symbol.IsFunctionBlockInstance)
			return new STLangFunctionBlockCall(symbol, location);
		else
		{
			this.report.SemanticError(5, symbol.TypeName, name, location);
			return null;
		}
	}

	private ProgramOrganizationUnitCall MakePOUCall(string name, POUParameter argument, LexLocation location)
	{
		STLangSymbol symbol;
		if (! this.symbolTable.Lookup(name, out symbol, location))
		{
			STLangSymbol undefinedPOU;
			this.report.SemanticError(0, name, location);
			undefinedPOU = this.symbolTable.InstallUndefinedFunction(name, 10);
			return new STLangFunctionCall(undefinedPOU, argument, location);
		}
		else if (symbol.IsFunction)
		{
			if (argument.DataType == TypeNode.Bool && ! (symbol is SELFunctionSymbol))
			{
				if (argument.IsInputParameter && argument.RValue.IsCompoundExpression)
				{
					Expression input = new LoadBoolValueOperator(argument.RValue);
					LexLocation loc = argument.LexicalLocation;
					string inputName = argument.FormalName;
					argument = new InputParameter(inputName, input, loc);
				}
			}
			return new STLangFunctionCall(symbol, argument, location);
		}
		else if (symbol.IsFunctionBlockInstance)
		{
			FunctionBlockType dataType = symbol.DataType as FunctionBlockType;
			this.CheckFunctionBlockParameter(name, dataType, argument, location);
			if (argument.DataType == TypeNode.Bool && argument.RValue.IsCompoundExpression)
			{
				if (argument.IsInputParameter)
				{
					Expression input = new LoadBoolValueOperator(argument.RValue);
					LexLocation loc = argument.LexicalLocation;
					string inputName = argument.FormalName;
					argument = new InputParameter(inputName, input, loc);
				}
			}
			return new STLangFunctionBlockCall(symbol, argument, location);
		}
		else
		{
			this.report.SemanticError(5, symbol.TypeName, name, location);
			return null;
		}
	}

	private ProgramOrganizationUnitCall AddPOUParameter(ProgramOrganizationUnitCall pou, POUParameter argument)
	{
		if (pou == null)
			return null;
		else {
			if (pou.IsInputArgAssignment && ! argument.IsInputArgAssignment)
				this.report.SemanticError(185, argument.ToString(), argument.LexicalLocation);
			else if (! pou.IsInputArgAssignment && argument.IsInputArgAssignment)
				this.report.SemanticError(185,  argument.ToString(), argument.LexicalLocation);
			if (argument.DataType == TypeNode.Bool && argument.RValue.IsCompoundExpression)
			{
				if (argument.IsInputParameter)
				{
					Expression input = new LoadBoolValueOperator(argument.RValue);
					LexLocation loc = argument.LexicalLocation;
					string inputName = argument.FormalName;
					argument = new InputParameter(inputName, input, loc);
				}
			}
			if (pou.IsFunction)
			{
				pou.Add(argument);
				return pou;
			}
			else if (pou.IsFunctionBlock)
			{
				this.CheckFunctionBlockParameter(pou, argument);
				pou.Add(argument);
				return pou;
			}
		}
		throw new STLangCompilerError("AddPOUParameter(): Unknown type of POU.");
	}

	private ProgramOrganizationUnitCall MakePOUCall(ProgramOrganizationUnitCall pou, LexLocation location)
	{
		return pou;
	}

	private Expression MakeFunctionCall(ProgramOrganizationUnitCall pou)
	{
		if (pou == null)
			return Expression.Error;
		else if (pou.IsFunctionBlock)
		{
			// Error: Function block used as a function
			//
			this.report.SemanticError(135, pou.Name, pou.Location);
			return Expression.Error;
		}
		else {
			Expression functionCall = pou.MakeSyntaxTreeNode();
			if (! this.symbolTable.IsRecursiveCall(functionCall))
				return functionCall;
			else {
				// Error: Illegal recursive call
				//
				this.report.SemanticError(90, pou.Name, pou.Location);
				return Expression.Error;
			}
		}
	}

	private Statement MakeFunctionBlockCallStatement(ProgramOrganizationUnitCall pou)
	{
		if (pou == null)
			return Statement.Empty;
		else if (pou.IsFunction)
		{
			// Warning: Function call found where function block call was expected.
			//
			this.report.Warning(16, pou.Name, pou.Location);
			return Statement.Empty;
		}
		else
		{
			Expression expression = pou.MakeSyntaxTreeNode();
			return new FunctionBlockCallStatement(expression);
		}
	}

	private POUParameter MakeParameter(Expression argument, LexLocation location)
	{
		if (argument == null)
			return new InputParameter(Expression.Error, location);
		else if (argument is FunctionName)
		{
			this.report.SemanticError(148, argument.ToString(), location);
			return new InputParameter(Expression.Error, location);
		}
		return new InputParameter(argument, location);
	}

	private POUParameter MakeParameter(string formalParam, Tokens assignToken, Expression argument, LexLocation location, bool inverted = false)
	{
		if (argument == null)
			argument = Expression.Error;
		else if (argument is FunctionName)
			this.report.SemanticError(148, argument.ToString(), location);
		if (assignToken == Tokens.OUTPUT_ASSIGN)
			return new OutputParameter(formalParam, argument, inverted, location);
		else if (assignToken == Tokens.ASSIGN)
		{
			if (inverted)
				this.report.SyntaxError(184, formalParam, location);
			return new InputParameter(formalParam, argument, location);
		}
		throw new STLangCompilerError("MakeParameter(): Unkown assignment token.");
	}

	private void CheckFunctionBlockParameter(ProgramOrganizationUnitCall pou, POUParameter argument)
	{
		FunctionBlockType functionBlock;
		functionBlock = pou.Symbol.DataType as FunctionBlockType;
		this.CheckFunctionBlockParameter(pou.Name, functionBlock, argument, pou.Location);
	}

	private void CheckFunctionBlockParameter(string pouName, FunctionBlockType functionBlock, POUParameter argument, LexLocation location)
	{
		InstanceSymbol formal;
		string formalName = argument.FormalName;

		if (formalName.Length == 0)
			this.report.SemanticError(174, pouName, argument.LexicalLocation);
		else if (! functionBlock.LookUp(formalName, out formal))
			this.report.SemanticError(141, formalName, pouName, argument.LexicalLocation);
		else if (argument.RValue != null)
		{
			argument.Position = formal.Position;
			if (argument.IsOutputParameter)
			{
				Expression lValue;

				lValue = formal.MakeSyntaxTreeNode(argument.LexicalLocation);
				argument.LValue = lValue;
			}
			Expression actual = argument.RValue;
			TypeNode actualDataType = actual.DataType;
			LexLocation formalLoc = argument.LexicalLocation;
			switch (formal.VariableType)
			{
				case STVarType.VAR_INPUT:
				break;

				case STVarType.VAR_INOUT:
					//
					// Check that the actual argument is an l-value
					//
					if (! actual.IsLValue && actualDataType != TypeNode.Error)
						this.report.SemanticError(78, pouName, formalName, formalLoc);
					if (! argument.IsInputParameter)
						this.report.SemanticError(175, formalName, formalLoc);
					break;

				case STVarType.VAR_OUTPUT:
					if (! actual.IsLValue && actualDataType != TypeNode.Error)
						this.report.SemanticError(189, pouName, formalName, formalLoc);
					if (! argument.IsOutputParameter)
					{
						if (! actual.IsLValue)
							this.report.SemanticError(176, formalName, formalLoc);
						else
							this.report.SemanticError(173, actual.ToString(), formalName, formalLoc);
					}
					break;

				default:
					this.report.SemanticError(180, formalName, formalLoc);
					break;
			}
			TypeNode formalDataType = formal.DataType;
			float conversionCost = formalDataType.ConversionCost(actual);
			if (conversionCost > 0.0 && actualDataType != TypeNode.Error)
			{
				if (actual.IsConstant)
				{
					if (formalDataType == TypeNode.LReal)
					{
						if (actualDataType == TypeNode.Real || actualDataType.IsIntegerType)
						{
							double doubleValue;
							doubleValue = Convert.ToDouble(actual.Evaluate());
							actual = MakeConstant(doubleValue);
						}
						else {
							string typeName = formalDataType.Name;
							string constValue = actual.ToString();
							this.report.SemanticError(177, formalName, constValue, typeName, location);
							actual = Expression.Error;
						}
					}
					else if (formalDataType == TypeNode.Real)
					{
						if (actualDataType.IsIntegerType)
						{
							float floatValue;
							floatValue = Convert.ToSingle(actual.Evaluate());
							actual = MakeConstant(floatValue);
						}
						else {
							string typeName = formalDataType.Name;
							string constValue = actual.ToString();
							this.report.SemanticError(177, formalName, constValue, typeName, location);
							actual = Expression.Error;
						}
					}
					else if (formalDataType.IsIntegerType)
					{
						if (actualDataType.IsIntegerType)
						{
							if (actualDataType.Size > formalDataType.Size)
							{
								string typeName = formalDataType.Name;
								string constValue = actual.ToString();
								this.report.SemanticError(178, constValue, formalName, typeName, location);
							}
						}
						else {
							string typeName = formalDataType.Name;
							string constValue = actual.ToString();
							this.report.SemanticError(177, formalName, constValue, typeName, location);
							actual = Expression.Error;
						}
					}
					else if (formalDataType.IsBitStringType)
					{
						if (actualDataType.IsBitStringType)
						{
							if (actualDataType.Size > formalDataType.Size)
							{
								string typeName = formalDataType.Name;
								string constValue = actual.ToString();
								this.report.SemanticError(178, constValue, formalName, typeName, location);
							}
						}
						else if (actualDataType.IsIntegerType)
						{
							if (actualDataType.Size > formalDataType.Size)
							{
								string typeName = formalDataType.Name;
								string constValue = actual.ToString();
								this.report.SemanticError(178, constValue, formalName, typeName, location);
							}
							else {
								ulong value = Convert.ToUInt64(actual.Evaluate());
								actual = this.MakeConstant(value, formalDataType);
							}
						}
						else {
							string typeName = formalDataType.Name;
							string constValue = actual.ToString();
							this.report.SemanticError(177, formalName, constValue, typeName, location);
							actual = Expression.Error;
						}
					}
				}
				else if (formalDataType == TypeNode.LReal)
				{
					if (actualDataType.IsIntegerType)
						actual = this.MakeInt2LReal(actual);
					else if (actualDataType == TypeNode.Real)
						actual = this.MakeReal2LReal(actual);
					else if (this.ConversionOperatorExists(TypeNode.LReal, actualDataType))
					{
						actual = Expression.Error;
						this.report.SemanticError(64, actualDataType.Name, formalDataType.Name, location);
					}
					else {
						actual = Expression.Error;
						this.report.SemanticError(150, actualDataType.Name, formalDataType.Name, location);
					}
				}
				else if (formalDataType == TypeNode.Real)
				{
					if (actualDataType.IsIntegerType)
						actual = this.MakeInt2Real(actual);
					else if (this.ConversionOperatorExists(TypeNode.Real, actualDataType))
					{
						actual = Expression.Error;
						this.report.SemanticError(64, actualDataType.Name, formalDataType.Name, location);
					}
					else {
						actual = Expression.Error;
						this.report.SemanticError(150, actualDataType.Name, formalDataType.Name, location);
					}
				}
			}
		}
	}

	private void CheckVarTypeQualUsage(STVarType variableType, STVarQualifier qualifier, LexLocation loc1, LexLocation loc2)
	{
		this.variableType = variableType;
		this.variableQualifier = qualifier;

		// Kolla att kombinationen POU-typ + variabeltyp + attribut �r giltig.

		switch (variableType)
		{
		case STVarType.VAR:
			break;

		case STVarType.VAR_INPUT:
			if (qualifier == STVarQualifier.CONSTANT)
				this.report.SemanticError(158, "VAR_INPUT", "CONSTANT", loc1);
			break;

		case STVarType.VAR_OUTPUT:
			if (qualifier == STVarQualifier.CONSTANT)
				this.report.SemanticError(158, "VAR_OUTPUT", "CONSTANT", loc1);
			break;

		case STVarType.VAR_INOUT:
			if (qualifier != STVarQualifier.NONE)
				this.report.SemanticError(158, "VAR_INOUT", variableType.ToString(), loc1);
			break;

		case STVarType.VAR_GLOBAL:
			this.report.SemanticError(156, "VAR_GLOBAL", loc1);
			break;

		case STVarType.VAR_CONFIG:
			if (qualifier != STVarQualifier.NONE)
				this.report.SemanticError(158, "VAR_ACCESS", variableType.ToString(), loc1);
			break;

		case STVarType.VAR_EXTERNAL:
			if (this.isFunctionDecl)
				this.report.SemanticError(156, "VAR_EXTERNAL", loc1);
			if (qualifier == STVarQualifier.RETAIN) 
				this.report.SemanticError(158, "VAR_EXTERNAL", "RETAIN", loc1);
			else if (qualifier == STVarQualifier.NON_RETAIN)
				this.report.SemanticError(158, "VAR_EXTERNAL", "NON_RETAIN", loc1);
			break;

		case STVarType.VAR_TEMP:
			if (this.isFunctionDecl)
				this.report.SemanticError(46, "VAR_TEMP", loc1);
			if (qualifier == STVarQualifier.RETAIN) 
				this.report.SemanticError(158, "VAR_TEMP", "RETAIN", loc1);
			else if (qualifier == STVarQualifier.NON_RETAIN)
				this.report.SemanticError(158, "VAR_TEMP", "NON_RETAIN", loc1);
			break;

		case STVarType.VAR_ACCESS:
			this.report.SemanticError(156, "VAR_ACCESS", loc1);
			if (qualifier != STVarQualifier.NONE)
				this.report.SemanticError(158, "VAR_ACCESS", variableType.ToString(), loc1);
			break;

		default:
			break;
		}
	}

	private void CheckDeclQualifierUsage(TypeNode dataType, STVarQualifier varQualifier, STDeclQualifier declQualifier, LexLocation location)
	{
		switch (declQualifier)
		{
		case STDeclQualifier.NONE:
			break;

		case STDeclQualifier.READ_ONLY:
		case STDeclQualifier.WRITE_ONLY:
			if (this.variableType != STVarType.VAR_ACCESS)
			{
				string varType = this.variableType.ToString();
				string edgeQual = declQualifier.ToString();
				this.report.SemanticError(158, varType, edgeQual, location);
			}
			break;

		case STDeclQualifier.F_EDGE:
		case STDeclQualifier.R_EDGE:
			if (this.isFunctionDecl)
				this.report.SemanticError(159, declQualifier.ToString(), location);
			if (dataType != TypeNode.Bool)
				this.report.SemanticError(160, declQualifier.ToString(), location);
			break;
		}
		if (dataType.IsFunctionBlockType && varQualifier == STVarQualifier.CONSTANT)
			this.report.SemanticError(179, dataType.Name, location);
	}

	private ProgramOrganizationUnitType ProgOrgType
	{
		get 
		{
			if (this.isFunctionDecl)
				return ProgramOrganizationUnitType.FUNCTION;
			else if (this.isFunctionBlockDecl)
				return ProgramOrganizationUnitType.FUNCTION_BLOCK;
			else
				return ProgramOrganizationUnitType.PROGRAM;
		}
	}

	private DeclarationStatement MakeFormalParameterDecl(STVarType varType, STVarQualifier varQual, List<VarDeclStatement> formalParDeclList)
	{
		return new IOParameterDeclaration(varType, varQual, formalParDeclList, this.ProgOrgType);
	}

	private DeclarationStatement MakeLocalVariableDecl(STVarType varType, STVarQualifier varQual, List<VarDeclStatement> localVarDeclList)
	{
		return new LocalVarDeclaration(localVarDeclList, varType, varQual, this.ProgOrgType);
	}

	private POUVarDeclarations MakeEmptyPOUVarDecl()
	{
		return new POUVarDeclarations();
	}

	private List<VarDeclStatement> MakeEmptyVarDecl()
	{
		return new List<VarDeclStatement>();
	}

	private List<VarDeclStatement> MakeVariableDeclList(VarDeclStatement varInitDecl)
	{
		List<VarDeclStatement> varInitDeclList = new List<VarDeclStatement>();
		if (varInitDecl != null)
			varInitDeclList.Add(varInitDecl);
		return varInitDeclList;
	}

	private List<VarDeclStatement> AddToVariableDeclList(List<VarDeclStatement> varInitDeclList, VarDeclStatement varInitDecl)
	{
		if (varInitDeclList == null)
			varInitDeclList = new List<VarDeclStatement>();
		if (varInitDecl != null)
			varInitDeclList.Add(varInitDecl);
		return varInitDeclList;
	}

	private POUVarDeclarations MakePOUVarDeclList(DeclarationStatement declaration)
	{
		POUVarDeclarations pouVarDeclarations = new POUVarDeclarations();
		if (declaration != null)
			pouVarDeclarations.Add(declaration);
		return pouVarDeclarations;
	}

	private POUVarDeclarations AddPOUVarDeclToList(POUVarDeclarations pouDeclList, DeclarationStatement declaration)
	{
		if (pouDeclList == null)
			pouDeclList = new POUVarDeclarations();
		if (declaration != null)
			pouDeclList.Add(declaration);
		return pouDeclList;
	}

	private void StoreArrayInitializer(ArrayInitializer arrayInit)
	{
		TypeNode elementType = arrayInit.BasicElementType;
		int alignment = (int)elementType.Alignment;
		byte[] bytes = arrayInit.GetBytes();
		int offset = ByteCodeGenerator.StoreByteArray(bytes, alignment);
		arrayInit.AbsoluteAddress = MakeIntConstant(offset);
	}

	private VarDeclStatement MakeVarDeclStatement(List<MemoryObject> variables, TypeNode dataType, STVarType varType, 
	                                    STDeclQualifier declQual, Expression initialValue, int elementCount = 1)
	{
		Expression declSize;
		if (varType == STVarType.VAR_INOUT)
		{
		    // Store offset of variable as a DINT
			//
			int byteCount = variables.Count*(int)TypeNode.DInt.Size;
			declSize = MakeIntConstant(byteCount);
			return new ElementaryVarDeclStatement(variables, dataType, declQual, initialValue, declSize);
		}
		else if (dataType.IsElementaryType)
		{
			int byteCount = variables.Count*(int)dataType.Size;
			declSize = MakeIntConstant(byteCount);
			return new ElementaryVarDeclStatement(variables, dataType, declQual, initialValue, declSize);
		}
		else if (dataType.IsTextType)
		{
            //
            // Store information about string buffer offset and size.
            //
			int stringType = dataType.IsStringType ? 0 : 1;
            foreach (MemoryObject variable in variables)
            {
                if (variable.Location is StringLocation)
                {
                    StringLocation stringLocation;

                    stringLocation = (StringLocation)variable.Location;
                    int bufferSize = (int)variable.DataType.Size;
                    int bufferOffset = stringLocation.BufferOffset;
					int index = stringLocation.Index;
					ByteCodeGenerator.StoreStringVariableData(bufferOffset, index, bufferSize, stringType);
                }
            }
			int byteCount = variables.Count*(int)dataType.Size;
			declSize = MakeIntConstant(byteCount);
			return new ElementaryVarDeclStatement(variables, dataType, declQual, initialValue, declSize);
		}
		else if (dataType.IsStructType)
		{
			VarDeclStatement memberInitStat;
			List<VarDeclStatement> memberInitCollection;
			int byteCount = variables.Count*(int)dataType.Size;
			FieldSymbol field = ((StructType)dataType.BaseType).FirstField;

			declSize = MakeIntConstant(byteCount);
			memberInitCollection = new List<VarDeclStatement>();
			if (initialValue is StructInitializer)
			{
				Expression initializer;
				List<MemoryObject> members;
				StructInitializer structInit = (StructInitializer)initialValue;
				while (field != null)
				{
					if (structInit.Contains(field.Name, out initializer))
					{
						members = this.CreateMemberList(variables, field.Name);
						memberInitStat = this.MakeVarDeclStatement(members, field.DataType, varType, declQual, initializer, elementCount);
						memberInitCollection.Add(memberInitStat);
					}
					field = field.Next;
				}
			}
			return new StructVarDeclStatement(variables, dataType, declQual, initialValue, declSize, memberInitCollection);
		}
		else if (dataType.IsFunctionBlockType)
		{
			if (initialValue is FunctionBlockInitializer)
			{
				int byteCount;
				InstanceSymbol member;
				Expression initializer;
				List<MemoryObject> members;
				VarDeclStatement memberDeclStat;
				List<VarDeclStatement> memberDeclStats;
				FunctionBlockInitializer functionBlockInit;

				byteCount = variables.Count*(int)dataType.Size;
				declSize = MakeIntConstant(byteCount);
				memberDeclStats = new List<VarDeclStatement>();
				functionBlockInit = (FunctionBlockInitializer)initialValue;
				member = ((FunctionBlockType)dataType.BaseType).FirstMember;
				while (member != null)
				{
					if (functionBlockInit.Contains(member.Name, out initializer))
					{
						members = this.CreateMemberList(variables, member.Name);
						memberDeclStat = this.MakeVarDeclStatement(members, member.DataType, varType, declQual, initializer, elementCount);
						memberDeclStats.Add(memberDeclStat);
					}
					member = member.Next;
				}
				return new FunctionBlockVarDeclStat(variables, dataType, declQual, initialValue, declSize, memberDeclStats);
			}
		}
		else if (dataType.IsArrayType)
		{
		    ArrayType array = (ArrayType)dataType;
            TypeNode elementType = array.BasicElementType;

			if (elementType.IsElementaryType || elementType.IsTextType)
            {
				if (initialValue is DefaultArrayInitializer)
				{
					int initValCase;
					if (initialValue.IsZero)
					{
						initValCase = 0;
						int byteCount = variables.Count*(int)dataType.Size;
						declSize = MakeIntConstant(byteCount);
					}
					else {
						initValCase = 1;
						uint size = elementType.Size;
						long elemCount = (dataType.Size / size)*variables.Count;
						declSize = MakeIntConstant(elemCount);
					}
					if (elementType.IsTextType)
					{
						int stringType = elementType.IsStringType ? 0 : 1;
						int bufferSize = (int)elementType.Size;
						int elemCount = (int)(dataType.Size / bufferSize);
						foreach (MemoryObject variable in variables)
						{
							if (variable.Location is StringLocation)
							{
								StringLocation stringLocation;

								stringLocation = (StringLocation)variable.Location;
								int bufferOffset = stringLocation.BufferOffset;
								int index = stringLocation.Index;
								ByteCodeGenerator.StoreStringVariableData(bufferOffset, index, bufferSize, stringType, elemCount);
							}
						}
					}
					return new ArrayVarDeclStatement(variables, dataType, declQual, initialValue, declSize, initValCase);
				}
				else if (initialValue is ArrayInitializer)
				{
					STVarQualifier varQual = this.variableQualifier;
					ArrayInitializer arrayInit = (ArrayInitializer)initialValue;
					if (elementType.IsTextType)
					{
						int stringType = elementType.IsStringType ? 0 : 1;
						int bufferSize = (int)elementType.Size;
						int elemCount = (int)(dataType.Size / bufferSize);
						foreach (MemoryObject variable in variables)
						{
							if (variable.Location is StringLocation)
							{
								StringLocation stringLocation;

								stringLocation = (StringLocation)variable.Location;
								int bufferOffset = stringLocation.BufferOffset;
								int index = stringLocation.Index;
								ByteCodeGenerator.StoreStringVariableData(bufferOffset, index, bufferSize, stringType, elemCount);
							}
						}
					}
					if (arrayInit.IsConstant)
					{
						this.StoreArrayInitializer(arrayInit);
						declSize = MakeIntConstant((long)dataType.Size);
						if (this.IsConstantVarDecl(varQual))
							return new ArrayVarDeclStatement(variables, dataType, declQual, arrayInit, declSize, 3);
						else
							return new ArrayVarDeclStatement(variables, dataType, declQual, arrayInit, declSize, 2);
					}
					else {
						int constCount = 0;
						IEnumerable<Expression> initializerList;

						initializerList = arrayInit.FlattenedInitializerList;
						foreach (Expression initValue in initializerList)
						{
							if (initValue.IsConstant)
								constCount++;
						}
						double constRatio = (double)constCount/initializerList.Count();
						if (constRatio < 0.7)
						{
							declSize = MakeIntConstant(initializerList.Count());
							if (this.IsConstantVarDecl(varQual))
								return new ArrayVarDeclStatement(variables, dataType, declQual, initialValue, declSize, 3, constRatio);
							else
								return new ArrayVarDeclStatement(variables, dataType, declQual, initialValue, declSize, 2, constRatio);
						}
						else {
							int i,offset;
							int baseIndex; 
							Expression defaultValue;
							ArrayInitializer newArrayInit;
							List<AssignmentStat> assignStatList;
							List<Expression> constInitList,nonConstInitList;

							constInitList = new List<Expression>();
							nonConstInitList = new List<Expression>();
							defaultValue = elementType.DefaultValue;
							declSize = MakeIntConstant((long)dataType.Size);
							foreach (Expression initValue in initializerList)
							{
								if (initValue.IsConstant)
									constInitList.Add(initValue);
								else {
									constInitList.Add(defaultValue);
									nonConstInitList.Add(initValue);
								}
							}
							assignStatList = new List<AssignmentStat>();
							newArrayInit = arrayInit.CreateInitList(constInitList);
							this.StoreArrayInitializer(newArrayInit);
							foreach (MemoryObject variable in variables)
							{
								i = 0;
								offset = 0;
								baseIndex = variable.Location.Index;
								foreach (Expression initValue in initializerList)
								{
									if (! initValue.IsConstant)
									{
										Expression lValue;
										MemoryLocation location;
										AssignmentStat assignStat;
                                        string name = variable + "[" + offset + "]";
										Expression rValue = nonConstInitList[i++];
										location = new ElementaryLocation(baseIndex + offset, elementType);
										lValue = new MemoryObject(name, location, elementType, variable.Symbol, 4);
										assignStat = new SimpleAssignmentStat(lValue, rValue);
										assignStatList.Add(assignStat);
									}
									offset++;
								}
							}
							if (this.IsConstantVarDecl(varQual))
								return new ArrayVarDeclStatement(variables, dataType, declQual, newArrayInit, declSize, 3, constRatio, assignStatList);
							else
								return new ArrayVarDeclStatement(variables, dataType, declQual, newArrayInit, declSize, 2, constRatio, assignStatList);
						}
					}
				}
			}
			else if (elementType.IsStructType)
			{
				VarDeclStatement memberInitStat;
				InitializerList flattenedInitList;
				StructType structure = (StructType)elementType.BaseType;
				FieldSymbol field = structure.FirstField;
				List<VarDeclStatement> memberInitCollection;
				Expression size = MakeIntConstant((long)dataType.Size);

				memberInitCollection = new List<VarDeclStatement>();
			    if (initialValue is ArrayOfStructInitializer)
				{
					List<MemoryObject> members;
					ArrayOfStructInitializer arrayOfStructInit;
					arrayOfStructInit = (ArrayOfStructInitializer)initialValue;
					elementCount = (int)(array.Size/elementType.Size)*elementCount;

					while (field != null)
					{
						members = this.CreateMemberList(variables, field); 
						flattenedInitList = arrayOfStructInit.GetFlattenedInitializerList(field.Name);
						memberInitStat = this.MakeVarDeclStatement(members, flattenedInitList.DataType, varType, declQual, flattenedInitList, elementCount);
						memberInitCollection.Add(memberInitStat);
						field = field.Next;
					}
				}
				return new ArrayOfStructVarDeclStatement(variables, dataType, declQual, initialValue, size, memberInitCollection);
			}
			else if (elementType.IsFunctionBlockType)
			{
			}
		}
		Expression zeroSize = MakeIntConstant((long)0);
		return new ElementaryVarDeclStatement(variables, dataType, declQual, initialValue, zeroSize);
	}

	private List<MemoryObject> CreateMemberList(List<MemoryObject> variables, FieldSymbol field)
	{
		InstanceSymbol symbol;
		List<MemoryObject> members = new List<MemoryObject>();

		foreach (MemoryObject variable in variables)
		{
			symbol = variable.Symbol;
			if (symbol.IsArrayInstance)
			{
                InstanceSymbol elementSymbol;
                ArrayInstanceSymbol arraySymbol;

				arraySymbol = (ArrayInstanceSymbol)symbol;
                elementSymbol = arraySymbol.ElementSymbol;
                if (elementSymbol.IsStructInstance)
                {
                    InstanceSymbol memberSymbol;
                    StructInstanceSymbol structure;

                    structure = (StructInstanceSymbol)elementSymbol;
                    if (structure.LookUp(field.Name, out memberSymbol))
					{
						MemoryObject member;

						member = (MemoryObject)memberSymbol.MakeSyntaxTreeNode();
                        members.Add(member);
					}
                }
			}
		}
		return members;
	}

	private List<MemoryObject> CreateMemberList(List<MemoryObject> variables, string member)
	{
		InstanceSymbol symbol;
		List<MemoryObject> members = new List<MemoryObject>();

		foreach (MemoryObject variable in variables)
		{
			symbol = variable.Symbol;
            if (symbol.IsCompoundInstanceSymbol)
            {
                InstanceSymbol memberSymbol;
                CompoundInstanceSymbol compoundSymbol;

                compoundSymbol = (CompoundInstanceSymbol)symbol;
                if (compoundSymbol.LookUp(member, out memberSymbol))
				{
					MemoryObject memberObject;

					memberObject = (MemoryObject)memberSymbol.MakeSyntaxTreeNode();
                    members.Add(memberObject);
				}
            }
		}
		return members;
	}

	private VarDeclStatement InstallLocalVars(List<string> identifiers, TypeNode dataType, STDeclQualifier declQual, LexLocation location)
	{
		if (identifiers == null)
			return null;
		else if (identifiers.Count == 0)
			return null;
		else {
			STVarType varType = this.variableType;
			Expression initialValue = dataType.DefaultValue;
			STVarQualifier varQual = this.variableQualifier;
			List<InstanceSymbol> symbols = new List<InstanceSymbol>();

			this.CheckDeclQualifierUsage(dataType, varQual, declQual, location);
			foreach (string name in identifiers)
			{
				InstanceSymbol symbol;
				symbol = this.rwMemoryManager.CreateSymbol(name, dataType, varType, varQual, declQual, initialValue, this.variablePosition);
				symbols.Add(symbol);
				this.variablePosition++;
			}
			this.symbolTable.InstallLocalVariables(symbols);
			if (this.IsConstantVarDecl(varQual))
            {
                // Error. Constant variables must be assigned a value in the declaration.
                this.report.SemanticError(9, location);
				// CONSTANT variables (with constant initializers) are initialized at compile time
                if (dataType.IsElementaryType)
                    return null;
            }
            MemoryObject variable;
            List<MemoryObject> variables = new List<MemoryObject>();

            foreach (InstanceSymbol symbol in symbols)
            {
                variable = (MemoryObject)symbol.MakeSyntaxTreeNode();
                variables.Add(variable);
            }
            return this.MakeVarDeclStatement(variables, dataType, varType, declQual, initialValue);
		}
	}

	private bool IsConstantVarDecl(STVarQualifier varQual)
	{
		return varQual == STVarQualifier.CONSTANT 
		    || varQual == STVarQualifier.CONSTANT_RETAIN;
	}

	private VarDeclStatement InstallLocalVars(List<string> identifiers, TypeNode dataType, STDeclQualifier declQual, Expression initialValue, LexLocation location)
	{
		this.Pop(); // Remove datatype saved on stack.
		if (identifiers == null)
			return null;
		else if (identifiers.Count == 0)
			return null;
		else {
			STVarType varType = this.variableType;
			STVarQualifier varQual = this.variableQualifier;
			List<InstanceSymbol> symbols = new List<InstanceSymbol>();

			this.CheckDeclQualifierUsage(dataType, varQual, declQual, location);
			foreach (string name in identifiers)
			{
				InstanceSymbol symbol;
				int pos = this.variablePosition++;
				symbol = this.rwMemoryManager.CreateSymbol(name, dataType, varType, varQual, declQual, initialValue, pos);
				symbols.Add(symbol);
			}
			this.symbolTable.InstallLocalVariables(symbols);
			if (this.IsConstantVarDecl(varQual) && initialValue.IsConstant && dataType.IsElementaryType)
				return null;  // Don't reserve memory for CONSTANT variables (with constant initializers) of elementary type.
			else {
				MemoryObject variable;
				List<MemoryObject> variables = new List<MemoryObject>();

				foreach (InstanceSymbol symbol in symbols)
				{
					variable = (MemoryObject)symbol.MakeSyntaxTreeNode();
					variables.Add(variable);
				}
				if (initialValue.IsLValue && !dataType.IsElementaryType)
				{
					MemoryObject memoryObject = (MemoryObject)initialValue;

					initialValue = memoryObject.InitialValue;
				}
				return this.MakeVarDeclStatement(variables, dataType, varType, declQual, initialValue);
			}
		}
	}

	private VarDeclStatement InstallSymbolicVariable(string name, DataTypeSpec dataTypeSpec, TokenDirectVar directVar, LexLocation loc)
	{
		if (! this.symbolTable.IsValidUserDefinedSymbol(name, loc))
			return null;
		else {
			InstanceSymbol symbol;
			TypeNode dataType = dataTypeSpec.DataType;
			STDeclQualifier declQual = dataTypeSpec.DeclQualifier;
			Expression initialValue = dataTypeSpec.InitialValue;
			this.symbolTable.InstallDirectVariable(name, dataType, this.variableType, this.variableQualifier,
			                                       dataTypeSpec.DeclQualifier, directVar.Size, directVar.Location, 
												   directVar.Address, out symbol);
			Expression size = MakeIntConstant((long)dataType.Size);
			List<MemoryObject> variables = new List<MemoryObject>();
			variables.Add((MemoryObject)symbol.MakeSyntaxTreeNode());
			return new ElementaryVarDeclStatement(variables, dataType, declQual, size, initialValue);
		}
	}

	private VarDeclStatement InstallDirectVariable(TokenDirectVar directVar, DataTypeSpec dataTypeSpec, LexLocation loc)
	{
		string name = directVar.ToString();
		if (! this.symbolTable.IsValidUserDefinedSymbol(name, loc))
			return null;
		else if (! this.isProgramDecl)
		{
			this.report.SemanticError(186, name, loc);
			return null;
		}
		else {
			InstanceSymbol symbol;
			TypeNode dataType = dataTypeSpec.DataType;
			STDeclQualifier declQual = dataTypeSpec.DeclQualifier;
			Expression initialValue = dataTypeSpec.InitialValue;
			this.symbolTable.InstallDirectVariable(name, dataType, this.variableType, this.variableQualifier,
			                                       dataTypeSpec.DeclQualifier, directVar.Size, directVar.Location, 
												   directVar.Address, out symbol);
			Expression size = MakeIntConstant((long)dataType.Size);
			List<MemoryObject> variables = new List<MemoryObject>();
			variables.Add((MemoryObject)symbol.MakeSyntaxTreeNode());
			return new ElementaryVarDeclStatement(variables, dataType, declQual, size, initialValue);
		}
	}

	private Expression CheckInitialValue(Expression initialValue, LexLocation location)
	{	
		TypeNode declDataType = this.attributeStack.Top;

		if (initialValue == null)
			return declDataType.DefaultValue;
		else if (declDataType == TypeNode.Error)
			return Expression.Error;
		else if (initialValue.DataType == TypeNode.Error)
			return declDataType.DefaultValue;
		else if (initialValue is FunctionName)
		{
			this.report.SemanticError(148, initialValue.ToString(), location);
			return declDataType.DefaultValue;
		}
		else {
			TypeNode initValDataType = initialValue.DataType;

			if (initValDataType == TypeNode.Bool && initialValue.IsCompoundExpression)
				initialValue = new LoadBoolValueOperator(initialValue);
			if (declDataType == initValDataType)
			{
				if (declDataType.IsOrdinalType && declDataType.IsSubrangeType)
				{
					if (initialValue.IsConstant)
					{
						if (declDataType.IsSignedIntType)
						{
							long numericValue = Convert.ToInt64(initialValue.Evaluate());
							if (! declDataType.IsInRange(numericValue))
							{
								string strValue = initialValue.ToString();
								string typeName = declDataType.Name;
								this.report.SemanticError(171, strValue, typeName, location);
							}
						}
						else {
							ulong numericValue = Convert.ToUInt64(initialValue.Evaluate());
							if (! declDataType.IsInRange(numericValue))
							{
								string strValue = initialValue.ToString();
								string typeName = declDataType.Name;
								this.report.SemanticError(171, strValue, typeName, location);
							}
						}
					}
					else if (initValDataType.IsSubrangeType)
					{
						SubRange initSubrange = initValDataType.GetSubrange();
						SubRange declSubrange = declDataType.GetSubrange();
						if (declSubrange.AreDisjoint(initSubrange))
							this.report.SemanticError(11, declDataType.Name, initValDataType.Name, location);
					}
				}
				return initialValue;
			}
			else if (declDataType == TypeNode.LReal && initValDataType.IsIntegerType)
			{
				if (! initialValue.IsConstant)
					return new Int2LRealOperator(initialValue);
				else {
					double doubleValue = Convert.ToDouble(initialValue.Evaluate());
					return MakeConstant(doubleValue);
				}
			}
			else if (declDataType == TypeNode.LReal && initValDataType == TypeNode.Real)
			{
				if (! initialValue.IsConstant)
					return new Real2LRealOperator(initialValue);
				else {
					double doubleValue = Convert.ToDouble(initialValue.Evaluate());
					return MakeConstant(doubleValue);
				}
			}
			else if (declDataType == TypeNode.Real && initValDataType == TypeNode.LReal)
			{
				if (! initialValue.IsConstant)
					this.report.SemanticError(27, declDataType.Name, initValDataType.Name, location);
				else {
					float floatValue = Convert.ToSingle(initialValue.Evaluate());
					return MakeConstant(floatValue);
				}
			}
			else if (declDataType == TypeNode.Real && initValDataType.IsIntegerType)
			{
				if (! initialValue.IsConstant)
					return new Int2RealOperator(initialValue);
				else {
					float singleValue = Convert.ToSingle(initialValue.Evaluate());
					return MakeConstant(singleValue);
				}
			}
			else if (declDataType.IsIntegerType && initValDataType.IsIntegerType)
			{
				if (declDataType.IsUnsignedIntType && initValDataType.IsSignedIntType
				 || declDataType.IsSignedIntType && initValDataType.IsUnsignedIntType
				 || declDataType.Size < initValDataType.Size)
				{
					this.report.SemanticError(64, initValDataType.Name, declDataType.Name, location);
					initialValue = declDataType.DefaultValue;
				}
			}
			else if (declDataType.IsBitStringType && initValDataType.IsIntegerType)
			{
				if (! initialValue.IsConstant)
					this.report.SemanticError(64, initValDataType.Name, declDataType.Name, location);
				else {
					ulong value = Convert.ToUInt64(initialValue.Evaluate());
					if (declDataType.Size < initValDataType.Size)
						this.report.SemanticError(64, initValDataType.Name, declDataType.Name, location);
					return this.MakeConstant(value, declDataType);
				}
			}
			else if (declDataType.IsBitStringType && initValDataType.IsBitStringType)
			{
				if (declDataType.Size < initValDataType.Size)
					this.report.SemanticError(64, initValDataType.Name, declDataType.Name, location);
			}
			else if (declDataType.IsStringType && initValDataType.IsStringType)
			{
				if (declDataType.Size < initValDataType.Size)
					this.report.SemanticError(35, initialValue.ToString(), location);
			}
			else
				this.report.SemanticError(27, declDataType.Name, initValDataType.Name, location);
			return initialValue;
		}
	}

	private List<object> MakeExpressionList(Expression expression, LexLocation location)
	{
		if (expression == null)
			return new List<object>{Expression.Error};
		else if (expression is FunctionName)
		{
			this.report.SemanticError(148, expression.ToString(), location);
			return new List<object>{Expression.Error};
		}
		else
			return new List<object>{expression};
	}

	private List<object> MakeExpressionList(List<object> exprList, Expression expression, LexLocation location)
	{
		if (expression == null)
			exprList.Add(Expression.Error);
		else if (expression is FunctionName)
		{
			this.report.SemanticError(148, expression.ToString(), location);
			exprList.Add(Expression.Error);
		}
		else
			exprList.Add(expression);
		return exprList;
	}

	private Expression MakeSymbolicVariable(Expression expression)
	{
		if (expression == null || expression == Expression.Error)
			return Expression.Error;
		else if (! expression.IsLValue)
			return expression;
		else if (expression.DataType.IsArrayType)
		{
			ArrayType array = (ArrayType)expression.DataType;
			TypeNode elementType = array.BasicElementType;

			if (elementType.IsElementaryType || elementType.IsTextType)
			{
                Expression absoluteAddress;
				Expression size = MakeIntConstant(array.Size);
                MemoryObject lValue = (MemoryObject)expression;
				absoluteAddress = this.rwMemoryManager.GetAbsoluteAddress(lValue.Symbol);
				if (lValue.Offset != null)
				{
					Expression factor = MakeIntConstant(elementType.Size);
					Expression offset = this.MakeIntMulOp(lValue.Offset, factor);
                    absoluteAddress = this.MakeIntAddOp(offset, absoluteAddress);
				}
				lValue.Location.AbsoluteAddress = absoluteAddress;
				lValue.Location.Size = size;
			}
		}
		return expression;
	}

	private Expression MakeSimpleVariable(string identifier, LexLocation location)
	{
		STLangSymbol symbol;
		if (this.isSubrangeDecl)
		{
			if (! this.symbolTable.Lookup(identifier, this.subrangeDataType, out symbol, location))
			{
				this.report.SemanticError(0, identifier, location);
				this.symbolTable.InstallUndeclaredVariable(identifier, out symbol);
			}
		}
		else if (! this.symbolTable.Lookup(identifier, out symbol, location))
		{
			this.report.SemanticError(0, identifier, location);
			this.symbolTable.InstallUndeclaredVariable(identifier, out symbol);
		}
		return symbol.MakeSyntaxTreeNode(location);
	}

	private Expression MakeSimpleVariable(Expression expression, string memberName, LexLocation location)
	{
		if (expression == null)
			return Expression.Error;
		else if (expression == Expression.Error)
			return Expression.Error;
		else if (! expression.IsLValue)
		{
			this.report.SemanticError(3, expression.ToString(), location);
			return Expression.Error;
		}
		else {
			TypeNode dataType = expression.DataType;
			if (dataType.IsStructType)
			{
				MemoryObject memoryObject = (MemoryObject)expression;
				InstanceSymbol symbol = memoryObject.Symbol;
				if (symbol.IsArrayInstance)
                {
					ArrayInstanceSymbol arrayInstance;

					arrayInstance = (ArrayInstanceSymbol)symbol;
					symbol = arrayInstance.ElementSymbol;
					if (! symbol.IsStructInstance)
					{
						string msg = "MakeSimpleVariable() failed.";
						msg += " StructInstanceSymbol expected: " + symbol.Name;
						throw new STLangCompilerError(msg);
					}
                }
				InstanceSymbol member;
				StructInstanceSymbol structure = (StructInstanceSymbol)symbol;
				if (! structure.LookUp(memberName, out member))
				{
					this.report.SemanticError(4, memberName, dataType.Name, location);
					return Expression.Error;
				}
				else
				{
					int length = memoryObject.Length + 2;
					Expression offset = memoryObject.Offset;
					string stringValue = expression + "." + memberName;
					return new MemoryObject(member, offset, stringValue, length);
				}
			}
			else if (dataType == TypeNode.Error)
				return Expression.Error;
			else if (dataType.IsFunctionBlockType)
			{
				MemoryObject memoryObject = (MemoryObject)expression;
				InstanceSymbol symbol = memoryObject.Symbol;
				if (! symbol.IsFunctionBlockInstance)
				{
					string msg = "MakeSimpleVariable() failed.";
					msg += " FunctionBlockInstanceSymbol expected: " + symbol.Name;
					throw new STLangCompilerError(msg);
				}
				else
				{
					InstanceSymbol member;
					FunctionBlockInstanceSymbol functionBlock;

					functionBlock = (FunctionBlockInstanceSymbol)symbol;
					if (! functionBlock.LookUp(memberName, out member))
					{
						this.report.SemanticError(136, memberName, dataType.Name, location);
						return Expression.Error;
					}
					else if (member.IsLocalVariable)
					{
						string varType = member.VariableType.ToString();
						this.report.SemanticError(137, varType, memberName, dataType.Name, location);
						return Expression.Error;
					}
					else {
						int length = memoryObject.Length + 2;
						Expression offset = memoryObject.Offset;
						string stringValue = expression + "." + memberName;
						return new MemoryObject(member, offset, stringValue, length);
					}
				}
			}
		}
		this.report.SemanticError(3, expression.ToString(), location);
        return Expression.Error;
	}

	private void CheckArrayIndex(Expression index, ArrayType array, string varName, LexLocation location)
	{
		long indexValue = Convert.ToInt64(index.Evaluate());
		if (indexValue < array.LowerBound)
			this.report.SemanticError(123, indexValue, varName, array.LowerBound, location);
		else if (indexValue > array.UpperBound)
			this.report.SemanticError(124, indexValue, varName, array.UpperBound, location);
	}

	private Expression MakeIndexedVariable(IndexedVariable variable)
	{
		if (variable == null)
			return Expression.Error;
		else {
			string name = variable.Name + "]";
			int length = variable.Length + 1; 
			MemoryLocation location = variable.Offset;
			InstanceSymbol symbol = variable.Symbol;
			int constantPart = variable.ConstantPart;
			Expression variablePart = variable.VariablePart;
			TypeNode dataType = variable.DataType;
			this.isIndexExpr = false;
			if (variablePart == null)
				throw new STLangCompilerError("MakeIndexedVariable() failed: Offset is null.");
			else if (dataType.IsElementaryType || dataType.IsTextType)
			{
				if (variablePart.IsConstant)
				{
					int varPart = Convert.ToInt32(variablePart.Evaluate());
					int offset = varPart - constantPart;
					if (! symbol.IsConstant)
					{
						int index = location.Index + offset;
						location = new ElementaryLocation(index, dataType);
						return new MemoryObject(name, location, dataType, symbol, length);
					}
					else if (symbol.InitialValue is ArrayInitializer)
					{
						// Constant array with constant array index. Get array element
						// at position 'offset' in the array initializer.
						//
						ArrayInitializer arrayInit;
						arrayInit = (ArrayInitializer)symbol.InitialValue;
						Expression rValue = arrayInit.GetElementAt(offset);
						return new MemoryObject(rValue, name, symbol, length);
					}
					else if (symbol.InitialValue is ArrayOfStructInitializer)
					{
						ArrayOfStructInitializer arrayInit;
						arrayInit = (ArrayOfStructInitializer)symbol.InitialValue;
						Expression rValue = arrayInit.GetElementAt(offset);
						return new MemoryObject(rValue, name, symbol, length);
					}
					throw new STLangCompilerError("ArrayInitializer type expected.");
				}
				else if (constantPart == 0)
					return new MemoryObject(name, location, variablePart, dataType, symbol, length);
				else 
				{
				    if (variablePart is IntAddOperator)
					{
						Expression rightOperand;
						rightOperand = ((IntAddOperator)variablePart).RightOperand;
						if (rightOperand.IsConstant)
							constantPart = Convert.ToInt32(rightOperand.Evaluate()) - constantPart;
					}
					Expression constant = MakeIntConstant(constantPart);
					Expression offset = new IntSubOperator(variablePart, constant, TypeNode.DInt);
					return new MemoryObject(name, location, offset, dataType, symbol, length);
				}
			}
			else if (dataType.IsArrayType)
			{
				ArrayType array = (ArrayType)dataType;
				long elementWidth = (long)array.Size/array.BasicElementType.Size;
				Expression elemWidth = MakeIntConstant(elementWidth);
				Expression offset = this.MakeIntMulOp(variablePart, elemWidth);
				if (constantPart != 0)
				{
					long value = constantPart*elementWidth;
					Expression constant = MakeIntConstant(value);
					offset = this.MakeIntSubOp(offset, constant);
				}
				Expression size = MakeIntConstant((long)array.Size);
				if (!offset.IsConstant)
					return new MemoryObject(name, location, offset, dataType, symbol, length);
				else
				{
					int value = Convert.ToInt32(offset.Evaluate());
					location = location.AddOffset(value);
					return new MemoryObject(name, location, dataType, symbol, length);
				}
			}
			else if (dataType.IsStructType)
			{
			}
			return Expression.Error;
		}
	}

	private IndexedVariable MakeIndexedVariable(IndexedVariable variable, Expression index, LexLocation loc1, LexLocation loc2)
	{
		if (variable == null)
			return null;
		else if (index == null)
			return variable;
		else if (! index.DataType.IsIntegerType && index.DataType != TypeNode.Error)
			this.report.SemanticError(30, index.ToString(), loc2);
		if (! variable.DataType.IsArrayType)
		{
			string parameter = variable.Name + "," + index + "]";
			this.report.SemanticError(167, parameter, loc1);
			return variable;
		}
		else {
			Expression variablePart;
			InstanceSymbol symbol = variable.Symbol;
			MemoryLocation offset = variable.Offset;
			int constantPart = variable.ConstantPart;
			string name = variable.Name + "," + index;
			int length = variable.Length + index.Length;
			ArrayType array = (ArrayType)variable.DataType;
			if (index.IsConstant)
				this.CheckArrayIndex(index, array, variable.Symbol.Name, loc2);
			else if (! index.IsLinear()  || !index.ConstantForLoopBounds(this.forLoopDataList))
				index = this.MakeRangeCheckOperator(index, array);
			else {
				int lowerBound = array.LowerBound;
				int upperBound = array.UpperBound;
				int powerSetSize = 1 << this.forLoopDataList.Count;
				for (int word = 0; word < powerSetSize; word++)
				{
					int indexValue = index.Evaluate(word, this.forLoopDataList);
					if (indexValue < lowerBound || indexValue > upperBound)
					{
						index = this.MakeRangeCheckOperator(index, array);
						break;
					}
				}
			}
			Expression factor = MakeIntConstant(array.Range);
			variablePart = this.MakeIntMulOp(variable.VariablePart, factor);
			variablePart = this.MakeIntAddOp(variablePart, index);
			constantPart = array.Range*constantPart + array.LowerBound;
			TypeNode elemType = array.ElementType;
			return new IndexedVariable(offset, elemType, variablePart, constantPart, symbol, name, length);
		}
	}

	private Expression MakeRangeCheckOperator(Expression index, ArrayType array)
	{
		Expression lowerBound = MakeIntConstant(array.LowerBound);
		Expression upperBound = MakeIntConstant(array.UpperBound);
		return new RangeCheckOperator(index, lowerBound, upperBound);
	}

	private IndexedVariable MakeIndexedVariable(Expression expression, Expression index, LexLocation loc1, LexLocation loc2)
	{
		if (expression == null || index == null)
			return null;
		else if (! index.DataType.IsIntegerType && index.DataType != TypeNode.Error)
			this.report.SemanticError(30, index.ToString(), loc2);
		if (expression.DataType == TypeNode.Error)
			return null;
		if (! expression.IsLValue)
		{
			this.report.SemanticError(2, expression.ToString(), loc1);
			return null;
		}
		else if (! expression.DataType.IsArrayType)
		{
			this.report.SemanticError(2, expression.ToString(), loc1);
			return null;
		}
		else {
			MemoryObject lValue = (MemoryObject)expression;
			MemoryLocation location = lValue.Location;
			InstanceSymbol symbol = lValue.Symbol;
			Expression offset = lValue.Offset;
			ArrayType array = (ArrayType)expression.DataType;
			TypeNode elemType = array.ElementType;
			int constPart = array.LowerBound;
			int length = index.Length + 2;
			string name = lValue + "[" + index;
			if (index.IsConstant)
				this.CheckArrayIndex(index, array, symbol.Name, loc2);
			else if (! index.IsLinear() || !index.ConstantForLoopBounds(this.forLoopDataList))
				index = this.MakeRangeCheckOperator(index, array);
			else {
				int lowerBound = array.LowerBound;
				int upperBound = array.UpperBound;
				int powerSetSize = 1 << this.forLoopDataList.Count;
				for (int word = 0; word < powerSetSize; word++)
				{
					int indexValue = index.Evaluate(word, this.forLoopDataList);
					if (indexValue < lowerBound || indexValue > upperBound)
					{
						index = this.MakeRangeCheckOperator(index, array);
						break;
					}
				}
			}
			if (offset == null)
				return new IndexedVariable(location, elemType, index, constPart, symbol, name, length);
			else if (offset is IntSubOperator)
			{
				BinaryOperator binaryOp = (BinaryOperator)offset;
				Expression variablePart = binaryOp.LeftOperand;
				Expression factor = MakeIntConstant(array.Range);
				object constantValue = binaryOp.RightOperand.Evaluate();
				int constantPart = Convert.ToInt32(constantValue);
				variablePart = this.MakeIntMulOp(variablePart, factor);
				variablePart = this.MakeIntAddOp(variablePart, index);
				constantPart = array.Range*constantPart + array.LowerBound;
				return new IndexedVariable(location, elemType, variablePart, constantPart, symbol, name, length);
			}
			return null;
		}
	}

	private Expression MakeDirectVariable(TokenDirectVar directVar, LexLocation location)
	{
		STLangSymbol symbol;
		string name = directVar.ToString();
		if (! this.symbolTable.Lookup(name, out symbol, location))
		{
			this.report.SemanticError(129, name, location);
			this.symbolTable.InstallUndeclaredDirectVariable(directVar, out symbol);
		}
		return symbol.MakeSyntaxTreeNode(location);
	}

	private List<CaseLabel> MakeCaseLabelList(CaseLabel caseLabel, LexLocation location)
	{
		List<CaseLabel> caseLabelList = new List<CaseLabel>();
		return this.AddCaseLabelToList(caseLabelList, caseLabel, location);
	}

	private CaseElement MakeCaseElement(List<CaseLabel> constList, StatementList statList)
	{
		this.PopTop();
		CaseElement caseElem = new CaseElement();
		if (constList == null)
			constList = new List<CaseLabel>();
		caseElem.LabelList = constList;
		caseElem.StatementList = statList;
		return caseElem;
	}

	private CaseElement MakeDefaultCaseElement(StatementList statList)
	{
		this.PopTop();
		CaseElement defaultCaseElem = new CaseElement();
		defaultCaseElem.StatementList = statList;
		defaultCaseElem.LabelList = null; // Empty list
		return defaultCaseElem;
	}

	private List<CaseElement> MakeCaseElementList(object caseElement)
	{
		List<CaseElement> caseElemList = new List<CaseElement>();
		if (caseElement != null)
			caseElemList.Add((CaseElement)caseElement);
		return caseElemList;
	}

	private List<CaseElement> AddCaseElementToList(List<CaseElement> caseElemList, object caseElem, LexLocation location)
	{
		if (caseElemList == null || caseElem == null)
			return caseElemList;
		else if (! (caseElem is CaseElement))
			throw new STLangCompilerError("Type of parameter caseElem must be CaseElement");
		else {
			CaseElement caseElement = (CaseElement)caseElem;
			if (caseElement.LabelList != null)
				caseElemList.Add(caseElement);
			else {
				// caseElement is a default statment. Make sure that there isn't another 
				// default statement in the list.

				CaseElement defaultStat;
				defaultStat = caseElemList.Find(caseEl => caseEl.LabelList == null);
				if (defaultStat == null)
					caseElemList.Add(caseElement);
				else
					this.report.SemanticError(146, location);
			}
			return caseElemList;
		}
	}

	private List<CaseLabel> AddCaseLabelToList(List<CaseLabel> caseLabelList, CaseLabel caseLabel, LexLocation loc)
	{
		if (caseLabel == null)
			return caseLabelList;
		else if (caseLabel is NumericLabel)
		{
			NumericLabel numericLabel = (NumericLabel)caseLabel;
			foreach (CaseLabel thisCaseLabel in this.caseLabelList)
			{
				if (! thisCaseLabel.AreDisjoint(numericLabel))
				{
					int errorCode = thisCaseLabel is NumericLabel ? 33 : 53;
					this.report.SemanticError(errorCode, numericLabel.ToString(), thisCaseLabel.ToString(), loc);
					return caseLabelList;
				}
			}
			caseLabelList.Add(caseLabel);
			this.caseLabelList.Add(caseLabel);
			return caseLabelList;
		}
		else 
		{
			SubrangeLabel subRangeLabel = (SubrangeLabel)caseLabel;
			foreach (CaseLabel thisCaseLabel in this.caseLabelList)
			{
				if (! thisCaseLabel.AreDisjoint(subRangeLabel))
				{
					int errorCode = thisCaseLabel is NumericLabel ? 53 : 52;
					this.report.SemanticError(errorCode, thisCaseLabel.ToString(), subRangeLabel.ToString(), loc);
					return caseLabelList;
				}
			}
			caseLabelList.Add(caseLabel);
			this.caseLabelList.Add(caseLabel);
			return caseLabelList;
		}
	}

	private Expression CheckIfBoolCondition(string statement, Expression expression, LexLocation location)
	{
		if (expression == null)
			return Expression.Error;
		else if (expression.DataType == TypeNode.Error)
			return Expression.Error;
		else if (expression is FunctionName)
		{
			this.report.SemanticError(148, expression.ToString(), location);
			return Expression.Error;
		}
		else if (expression.DataType == TypeNode.Bool)
			return expression;
		else {
			this.report.SemanticError(34, statement, location);
			return Expression.Error;
		}
	}

	private Statement MakeWhileStatement(Expression expression, StatementList statementList, LexLocation location)
	{
		this.PopTop();
		this.loopNestingDepth--;
		if (expression == null || statementList == null)
			return Statement.Empty;
		else {
			if (expression.IsConstant)
			{
				bool conditonIsTrue = Convert.ToBoolean(expression.Evaluate());
				if (conditonIsTrue && !statementList.ContainsExit)
					this.report.SemanticError(92, location);
			}
			return new WhileStatement(expression.InvertRelation(), statementList);
		}
	}

	private Statement MakeRepeatStatement(StatementList statementList = null, Expression expression = null, LexLocation location = null)
	{
		this.PopTop();
		this.loopNestingDepth--;
		if (statementList == null || expression == null)
			return Statement.Empty;
		else {
			if (expression.IsConstant)
			{
				bool conditonIsFalse = !Convert.ToBoolean(expression.Evaluate());
				if (conditonIsFalse && !statementList.ContainsExit)
					this.report.SemanticError(93, location);
			}
			return new RepeatStatement(statementList, expression.InvertRelation());
		}
	}

	private List<object> MakeElseIfStatement(Expression expression, StatementList statementList)
	{
		this.PopTop();
		ElseIfStatement elseIfStat;
		elseIfStat = new ElseIfStatement(expression.InvertRelation(), statementList);
		return new List<object>{ elseIfStat };
	}

	private Statement MakeIfStatement(Expression condition, StatementList thenStat, List<object> objList, StatementList elseStat)
	{
		if (objList == null)
			return new IfStatement(condition.InvertRelation(), thenStat, elseStat);
		else {
			List<ElseIfStatement> elseIfStatList;
			elseIfStatList = objList.Cast<ElseIfStatement>().ToList();
			return new IfStatement(condition.InvertRelation(), thenStat, elseStat, elseIfStatList);
		}
	}

	private List<object> AddElseIfStatementToList(List<object> elseIfList, Expression expr, StatementList statList)
	{
		if (statList == null)
			return elseIfList;
		else {
			ElseIfStatement elseIfStat;
			elseIfStat = new ElseIfStatement(expr.InvertRelation(), statList);
			elseIfList.Add(elseIfStat);
			return elseIfList;
		}
	}

	private void InstallFunctionProtoType(string name, TypeNode resultDataType, POUVarDeclarations varDecls, LexLocation loc)
	{
		this.symbolTable.InstallFunctionProtoType(name, resultDataType, varDecls, loc);
	}

	private void InstallFunctionBlockProtoType(string name, POUVarDeclarations pouVarDecls, LexLocation loc)
	{
		uint byteCount;
		FunctionBlockType dataType;
		List<InstanceSymbol> members;
		List<InstanceSymbol> instanceSymbols;

		string typeID = "{" + name + "}";
        instanceSymbols = pouVarDecls.InputParameters.ToList();
        members = instanceSymbols.Cast<InstanceSymbol>().ToList();
		byteCount = (uint)this.rwMemoryManager.RWDataSegmentSize;
        Expression size = MakeIntConstant((long)byteCount);
		pouVarDecls.Size = size;
		dataType = new FunctionBlockType(name, members, byteCount, size, typeID);
		this.symbolTable.InstallFunctionBlockProtoType(name, dataType, pouVarDecls, loc);
	}

	private ExitStatement MakeExitStatement(LexLocation location)
	{
		if (this.loopNestingDepth == 0)
			this.report.SemanticError(21, location);
		return new ExitStatement();
	}

	private ReturnStatement MakeReturnStatement(LexLocation location)
	{
		if (this.isFunctionDecl)
			this.CheckFunctionValueIsDefined(location);
		return new ReturnStatement();
	}

	private ReturnStatement MakeReturnStatement2(LexLocation location)
	{
		this.report.SyntaxError(47, location);
		return new ReturnStatement();
	}

	private void PushForLoopData(Expression expression, ForLoopData forLoopData)
	{
		if (expression != null && forLoopData != null)
		{
			Predicate<ForLoopData> searchCond;
			MemoryObject memoryObj = (MemoryObject)expression;
			searchCond = forLoop => forLoop.ControlVariable.Symbol == memoryObj.Symbol;
			if (this.forLoopDataList.Find(searchCond) == null)
			{
				memoryObj.Symbol.IsForLoopCtrlVar = true;
				forLoopData.ControlVariable = memoryObj;
				this.forLoopDataList.Add(forLoopData);
			}
		}
	}

	private MemoryObject GenerateTemporary(Expression expr)
    {
        return this.rwMemoryManager.GenerateTemporary(expr);
    }

    private MemoryObject GenerateTemporary(TypeNode dataType)
    {
        return this.rwMemoryManager.GenerateTemporary(dataType);
    }

	private Statement MakeForLoopStatement(Expression controlVar, ForLoopData forLoop, StatementList statList)
	{
		this.PopTop();
		this.loopNestingDepth--;
		this.PopForLoopVariables();
		if (controlVar == null || forLoop == null)
			return Statement.Empty;
		else {
			Expression initValue = forLoop.InitialValue;
			Expression stopValue = forLoop.StopValue;
			Expression increment = forLoop.Increment;

			if (initValue == null || stopValue == null || increment == null)
				return Statement.Empty;
			else {
				Expression condition;
				bool executeLoopOnce = false;

				if (increment.IsConstant)
				{
					if (stopValue.IsCompoundExpression)
					{
						MemoryObject temporary = this.GenerateTemporary(controlVar);
						int incr = Convert.ToInt32(increment.Evaluate());
						if (incr > 0)
							condition = new IntLeqOperator(controlVar, temporary);
						else 
							condition = new IntGeqOperator(controlVar, temporary);
						return new ForStatement((MemoryObject)controlVar, initValue, stopValue, temporary, condition, increment, statList);
					}
					else {
						int incrValue = Convert.ToInt32(increment.Evaluate());
						if (incrValue > 0)
							condition = new IntLeqOperator(controlVar, stopValue);
						else 
							condition = new IntGeqOperator(controlVar, stopValue);
						if (initValue.IsConstant && stopValue.IsConstant)
						{
							int initVal = Convert.ToInt32(initValue.Evaluate());
							int stopVal = Convert.ToInt32(stopValue.Evaluate());
							if (incrValue > 0)
								executeLoopOnce = initVal <= stopVal;
							else 
								executeLoopOnce = initVal >= stopVal;
						}
					}
				}
				else if (initValue.IsConstant && stopValue.IsConstant)
				{
					int initVal = Convert.ToInt32(initValue.Evaluate());
					int stopVal = Convert.ToInt32(stopValue.Evaluate());
					if (initVal <= stopVal)
					{
						condition = new IntLeqOperator(controlVar, stopValue);
						executeLoopOnce = initVal <= stopVal;
					}
					else {
						condition = new IntGeqOperator(controlVar, stopValue);
						executeLoopOnce = initVal >= stopVal;
					}
				}
				else if (! stopValue.IsCompoundExpression)
					condition = new IntLeqOperator(controlVar, stopValue);
				else {
					MemoryObject temporary;
					temporary = this.GenerateTemporary(controlVar);
					condition = new IntLeqOperator(controlVar, temporary);
					return new ForStatement((MemoryObject)controlVar, initValue, stopValue, temporary, condition, increment, statList);
				} 
				return new ForStatement((MemoryObject)controlVar, initValue, condition, increment, statList, executeLoopOnce);
			}
		}
	}

	private Statement MakeCaseStatement(Expression controlExpr, List<CaseElement> caseElemList, LexLocation location)
	{
		this.PopCaseLabelList();
		if (caseElemList.Count == 0)
		{
			this.report.Warning(26, location);
			return Statement.Empty;
		}
		else {
			CaseElement defaultStatement;
			
			defaultStatement = caseElemList.Find(caseElem => caseElem.LabelList == null);
			if (defaultStatement == null)
				return new CaseStatement(controlExpr, caseElemList);
			else {
				caseElemList.Remove(defaultStatement);
				if (caseElemList.Count == 0)
					this.report.Warning(27, location);
				return new CaseStatement(controlExpr, caseElemList, defaultStatement);
			}
		}
	}

	private ForLoopData MakeForLoopData(Expression initialValue, Expression stopValue, Expression increment, LexLocation loc1 = null, LexLocation loc2 = null, LexLocation loc3 = null)
	{
		ForLoopData forLoopData = new ForLoopData();
		if (initialValue == null)
			initialValue = MakeIntConstant((long)0);
		else if (initialValue is FunctionName)
		{
			this.report.SemanticError(148, initialValue.ToString(), loc1);
			initialValue = MakeIntConstant((long)0);
		}
		if (stopValue == null)
			stopValue = Expression.Error;
		else if (stopValue is FunctionName)
		{
			this.report.SemanticError(148, stopValue.ToString(), loc2);
			stopValue = Expression.Error;
		}
		if (increment == null)
			increment = MakeIntConstant((long)1);
		else if (increment is FunctionName)
		{
			this.report.SemanticError(148, increment.ToString(), loc3);
			increment = MakeIntConstant((long)1);
		}
		forLoopData.InitialValue = initialValue;
		forLoopData.StopValue = stopValue;
		forLoopData.Increment = increment;
		this.forLoopVarKind = 0x0;
		this.loopNestingDepth++;
		return forLoopData;
	}

	private Expression SaveControlVariable(Expression expr, LexLocation location)
	{
		if (expr == null)
			return Expression.Error;
		else if (expr is FunctionName)
		{
			this.report.SemanticError(148, expr.ToString(), location);
			return Expression.Error;
		}
		else if (! expr.IsLValue)
		{
			// Error: expr is not an L-Value
			this.report.SemanticError(54, expr.ToString(), location);
			return Expression.Error;
		}
		else {
			TypeNode dataType = expr.DataType;
			MemoryObject variable = (MemoryObject)expr;
			InstanceSymbol varSymbol = variable.Symbol;
			if (variable.IsConstant)
				this.report.SemanticError(8, expr.ToString(), location);
			if ((dataType.IsSignedIntType || dataType == TypeNode.Error) && variable.IsSimpleVariable)
				this.RegisterForLoopVariable(varSymbol, ForLoopVariableType.CONTROL_VARIABLE, location);
			else {
				if (! variable.IsSimpleVariable)
					this.report.SemanticError(49, varSymbol.Name, location); // Control variable can't be an element of an array or struct
				if (! dataType.IsSignedIntType && dataType != TypeNode.Error)
					this.report.SemanticError(132, varSymbol.Name, location); // Control variable must be of a signed integer type
			}
			return expr;
		}
	}

	private void CheckIfForLoopVar(Expression expr, LexLocation location)
	{
		if (expr == null)
			return;
		else if (expr is MemoryObject && this.forLoopVarKind != 0)
		{
			ForLoopVariableType loopVarKind;
			MemoryObject memObject = (MemoryObject)expr;

			if (this.forLoopVarKind == 0x1)
				loopVarKind = ForLoopVariableType.CONTROL_VARIABLE;
			else if (this.forLoopVarKind == 0x2)
				loopVarKind = ForLoopVariableType.START_VARIABLE;
			else if (this.forLoopVarKind == 0x4)
				loopVarKind = ForLoopVariableType.STOP_VARIABLE;
			else if (this.forLoopVarKind == 0x8)
				loopVarKind = ForLoopVariableType.INCR_VARIABLE;
			else
				loopVarKind = ForLoopVariableType.NONE;
			this.RegisterForLoopVariable(memObject.Symbol, loopVarKind, location);
		}
	}

	private bool ConversionOperatorExists(TypeNode fromType, TypeNode toType)
	{
		if (fromType == TypeNode.Error || toType == TypeNode.Error)
			return false;
		else if (!fromType.IsElementaryType || !toType.IsElementaryType)
			return false;
		else if (fromType.IsSubrangeType || toType.IsSubrangeType)
			return true;
		else {
			STLangSymbol symbol;
			string conversionOp;

			conversionOp = string.Format("{0}_TO_{1}", fromType.Name, toType.Name);
			return this.symbolTable.Lookup(conversionOp, out symbol, null);
		}
	}

	private Statement MakeAssignmentStatement(STLangSymbol lSymbol, STLangSymbol rSymbol, Expression lIndex = null, Expression rIndex = null)
	{
		TypeNode dataType = lSymbol.DataType;
		if (dataType.IsStructType)
		{
			if (! lSymbol.IsStructInstance || ! rSymbol.IsStructInstance)
				throw new STLangCompilerError(Resources.MAKEASSGNSTAT1);
			else {
				Statement assignmentStat;
				CompoundAssignmentStat compoundAssignStat;		
				StructInstanceSymbol lStruct = (StructInstanceSymbol)lSymbol;
				StructInstanceSymbol rStruct = (StructInstanceSymbol)rSymbol;
				InstanceSymbol lMember = lStruct.FirstMember;
				InstanceSymbol rMember = rStruct.FirstMember;

				compoundAssignStat = new CompoundAssignmentStat();
				while (lMember != null && rMember != null)
				{
					assignmentStat = this.MakeAssignmentStatement(lMember, rMember, lIndex, rIndex);
					compoundAssignStat.Add(assignmentStat);
					lMember = lMember.Next;
					rMember = rMember.Next;
				}
				return compoundAssignStat;
			}
		}
		else if (dataType.IsArrayType)
		{
			if (! lSymbol.IsArrayInstance || ! rSymbol.IsArrayInstance)
				throw new STLangCompilerError(Resources.MAKEASSGNSTAT2);
			else {
				ArrayType array = (ArrayType)dataType;
				TypeNode elementType = array.BasicElementType;

				if (elementType.IsStructType || elementType.IsArrayType)
				{
					ArrayInstanceSymbol lArray = (ArrayInstanceSymbol)lSymbol;
					ArrayInstanceSymbol rArray = (ArrayInstanceSymbol)rSymbol;
					STLangSymbol lElement = lArray.ElementSymbol;
					STLangSymbol rElement = rArray.ElementSymbol;
					return this.MakeAssignmentStatement(lElement, rElement, lIndex, rIndex);
				}
				else {
					long sizeOfDataType = dataType.Size;
					Expression size = MakeIntConstant(sizeOfDataType);
					this.rwMemoryManager.SetAbsoluteAddress(lSymbol);
					this.rwMemoryManager.SetAbsoluteAddress(rSymbol);
					Expression lValue = lSymbol.MakeSyntaxTreeNode();
					Expression rValue = rSymbol.MakeSyntaxTreeNode();
					((MemoryObject)lValue).Location.Size = size;
					((MemoryObject)rValue).Location.Size = size;
					return new SimpleAssignmentStat(lValue, rValue);
				}
			}
		}
		else if (dataType.IsElementaryType || dataType.IsTextType)
		{
	        // Obs! Om man allokerar mer minne för att spara undan värdet av ett 
		    // deluttryck efter ett anrop av SetAbsoluteAddress() kan absolutad- 
            // ressen bli fel. Man borde egentligen spara undan lSymbol och rSymbol
            // i en lista för att senare fylla i absolutadressen.

			int elementCount = lSymbol.Location.ElementCount;
			Expression lValue = lSymbol.MakeSyntaxTreeNode();
			Expression rValue = rSymbol.MakeSyntaxTreeNode();
			if (elementCount > 1)
			{ 
				// Array assignment

				long sizeOfDataType = lSymbol.DataType.Size * elementCount;
				Expression size = MakeIntConstant(sizeOfDataType);
				this.rwMemoryManager.SetAbsoluteAddress(lSymbol);
				this.rwMemoryManager.SetAbsoluteAddress(rSymbol);
				((MemoryObject)lValue).Location.Size = size;
				((MemoryObject)rValue).Location.Size = size;
			}
			return new SimpleAssignmentStat(lValue, rValue);
		}
		return Statement.Empty;
	}

	private Statement MakeAssignmentStatement(Expression lExpression, Expression rValue, LexLocation location)
	{
		if (lExpression == null || rValue == null)
			return Statement.Empty;
		else if (lExpression == Expression.Error)
			return Statement.Empty;
		else if (rValue is FunctionName)
		{
			this.report.SemanticError(148, rValue.ToString(), location);
			return Statement.Empty;
		}
		else if (lExpression is FunctionName)
		{
			TypeNode rValueDataType = rValue.DataType;
			TypeNode resultType = lExpression.DataType;
			if (rValue.DataType == TypeNode.Bool && rValue.IsCompoundExpression)
				rValue = new LoadBoolValueOperator(rValue);
			if (! this.symbolTable.IsCurrentFunction(lExpression))
			{
				this.report.SemanticError(149, lExpression.ToString(), location);
				return Statement.Empty;
			}
			else if (resultType == TypeNode.Error || rValueDataType == TypeNode.Error)
				return new FunctionResultStatement(Expression.Error);
			else if (resultType == rValueDataType)
				return new FunctionResultStatement(rValue);
			else if (resultType.IsIntegerType && rValueDataType.IsIntegerType)
			{
				if (rValueDataType.Size <= resultType.Size)
					return new FunctionResultStatement(rValue);
			}
			else if (resultType.IsBitStringType)
			{
				if (rValueDataType.IsBitStringType)
				{
					if (rValueDataType.Size <= resultType.Size)
						return new FunctionResultStatement(rValue);
				}
				else if (rValueDataType.IsIntegerType)
				{
					if (rValue.IsConstant && rValueDataType.Size <= resultType.Size)
						return new FunctionResultStatement(rValue);
				}
			}
			else if (resultType == TypeNode.Real)
			{
				if (rValueDataType.IsIntegerType)
				{
					if (! rValue.IsConstant)
						rValue = this.MakeInt2Real(rValue); // Insert conversion operator
					else {
						// Convert constant value from integer to float
						float floatValue = Convert.ToSingle(rValue.Evaluate());
						rValue = MakeConstant(floatValue);
					}
					return new FunctionResultStatement(rValue);
				}
			}
			else if (resultType == TypeNode.LReal)
			{
				if (rValueDataType.IsIntegerType)
				{
					if (! rValue.IsConstant)
						rValue = new Int2LRealOperator(rValue); // Insert conversion operator
					else {
						// Convert constant value from integer to double
						double doubleValue = Convert.ToDouble(rValue.Evaluate());
						rValue = MakeConstant(doubleValue);
					}
					return new FunctionResultStatement(rValue);
				}
				else if (rValueDataType == TypeNode.Real)
				{
					if (! rValue.IsConstant)
						rValue = new Real2LRealOperator(rValue);
					else {
						double doubleValue = Convert.ToDouble(rValue.Evaluate());
						rValue = MakeConstant(doubleValue);
					}
					return new FunctionResultStatement(rValue);
				}
			}
			if (this.ConversionOperatorExists(rValueDataType, resultType))
				this.report.SemanticError(64, rValueDataType.Name, resultType.Name, location);
			else
				this.report.SemanticError(150, rValueDataType.Name, resultType.Name, location);
			return Statement.Empty;
		}
		else if (! lExpression.IsLValue)
		{
			this.report.SemanticError(54, lExpression.ToString(), location);
			return Statement.Empty;
		}
		else {
			TypeNode rValueDataType = rValue.DataType;
			TypeNode lValueDataType = lExpression.DataType;
			MemoryObject lmemObject = (MemoryObject)lExpression;
			InstanceSymbol symbol = lmemObject.Symbol;
			foreach (Hashtable forLoopVars in this.forLoopVarTable)
			{
				if (forLoopVars.Contains(symbol))
				{
					ForLoopVariableType forLoopVarType;
					forLoopVarType = (ForLoopVariableType)forLoopVars[symbol];
					if ((forLoopVarType & ForLoopVariableType.CONTROL_VARIABLE) != 0)
						this.report.SemanticError(125, symbol.Name, location);
					if ((forLoopVarType & ForLoopVariableType.START_VARIABLE) != 0)
						this.report.SemanticError(126, symbol.Name, location);
					if ((forLoopVarType & ForLoopVariableType.STOP_VARIABLE) != 0)
						this.report.SemanticError(127, symbol.Name, location);
					if ((forLoopVarType & ForLoopVariableType.INCR_VARIABLE) != 0)
						this.report.Warning(15, symbol.Name, location);
				}
			}
			if (rValue.DataType == TypeNode.Bool && rValue.IsCompoundExpression)
				rValue = new LoadBoolValueOperator(rValue);
			if (lExpression.IsConstant)
			{
				if (lmemObject.IsSimpleVariable)
					this.report.SemanticError(169, lExpression.ToString(), location);
				else
					this.report.SemanticError(170, lExpression.ToString(), location);
			}
			else if (lExpression.IsConstantLValue)
			{
				if (lmemObject.IsSimpleVariable)
					this.report.SemanticError(168, lExpression.ToString(), location);
				else
					this.report.SemanticError(170, lExpression.ToString(), location);
			}
			if (lValueDataType == rValueDataType)
			{
				if (rValueDataType.IsStructType)
				{
					MemoryObject rmemObject = (MemoryObject)rValue;
					STLangSymbol lSymbol = lmemObject.Symbol;
					STLangSymbol rSymbol = rmemObject.Symbol;
					return this.MakeAssignmentStatement(lSymbol, rSymbol);
				}
     			else if (rValueDataType.IsArrayType)
				{
					MemoryObject rmemObject = (MemoryObject)rValue;
					STLangSymbol lSymbol = lmemObject.Symbol;
					STLangSymbol rSymbol = rmemObject.Symbol;
					Expression lIndex = lmemObject.Offset;
					Expression rIndex = rmemObject.Offset;
					return this.MakeAssignmentStatement(lSymbol, rSymbol, lIndex, rIndex);
				}
				else if (lValueDataType.IsOrdinalType && lValueDataType.IsSubrangeType)
				{
					if (rValue.IsConstant)
					{
						if (lValueDataType.IsSignedIntType)
						{
							long numericValue = Convert.ToInt64(rValue.Evaluate());
							if (! lValueDataType.IsInRange(numericValue))
							{
								string strValue = rValue.ToString();
								string typeName = lValueDataType.Name;
								this.report.SemanticError(171, strValue, typeName, location);
							}
						}
						else {
							ulong numericValue = Convert.ToUInt64(rValue.Evaluate());
							if (! lValueDataType.IsInRange(numericValue))
							{
								string strValue = rValue.ToString();
								string typeName = lValueDataType.Name;
								this.report.SemanticError(171, strValue, typeName, location);
							}
						}
					}
					else if (rValueDataType.IsSubrangeType)
					{
						SubRange rValueSubrange = rValueDataType.GetSubrange();
						SubRange lValueSubrange = lValueDataType.GetSubrange();
						if (lValueSubrange.AreDisjoint(rValueSubrange))
						{
							string lValName = lValueDataType.Name;
							string rValName = rValueDataType.Name;
							this.report.SemanticError(11, lValName, rValName, location);
						}
					}
				}
				return new SimpleAssignmentStat(lExpression, rValue);
			}
			else if (lValueDataType == TypeNode.Error || rValueDataType == TypeNode.Error)
				return new SimpleAssignmentStat(lExpression, rValue);
			else if (lValueDataType.IsIntegerType && rValueDataType.IsIntegerType)
			{
				if (rValueDataType.Size <= lValueDataType.Size)
					return new SimpleAssignmentStat(lExpression, rValue);
			}
			else if (lValueDataType.IsBitStringType)
			{
				if (rValueDataType.IsBitStringType)
				{
					if (rValueDataType.Size <= lValueDataType.Size)
						return new SimpleAssignmentStat(lExpression, rValue);
				}
				else if (rValueDataType.IsIntegerType)
				{
					if (rValue.IsConstant && rValueDataType.Size <= lValueDataType.Size)
						return new SimpleAssignmentStat(lExpression, rValue);
				}
			}
			else if (lValueDataType == TypeNode.Real)
			{
				if (rValueDataType.IsIntegerType)
				{
					if (! rValue.IsConstant)
						rValue = this.MakeInt2Real(rValue); // Insert conversion operator
					else {
						// Convert constant value from integer to float
						float floatValue = Convert.ToSingle(rValue.Evaluate());
						rValue = MakeConstant(floatValue);
					}
					return new SimpleAssignmentStat(lExpression, rValue);
				}
			}
			else if (lValueDataType == TypeNode.LReal)
			{
				if (rValueDataType.IsIntegerType)
				{
					if (! rValue.IsConstant)
						rValue = new Int2LRealOperator(rValue); // Insert conversion operator
					else {
						// Convert constant value from integer to double
						double doubleValue = Convert.ToDouble(rValue.Evaluate());
						rValue = MakeConstant(doubleValue);
					}
					return new SimpleAssignmentStat(lExpression, rValue);
				}
				else if (rValueDataType == TypeNode.Real)
				{
					if (! rValue.IsConstant)
						rValue = new Real2LRealOperator(rValue);
					else {
						double doubleValue = Convert.ToDouble(rValue.Evaluate());
						rValue = MakeConstant(doubleValue);
					}
					return new SimpleAssignmentStat(lExpression, rValue);
				}
			}
			else if (lValueDataType.IsStringType && rValueDataType.IsStringType)
			{
				if (lValueDataType.Size >= rValueDataType.Size)
					return new SimpleAssignmentStat(lExpression, rValue);
				else {
					this.report.SemanticError(188, rValue.ToString(), lExpression.ToString(), location);
					return Statement.Empty;
				}
			}
			else if (lValueDataType.IsWStringType && rValueDataType.IsWStringType)
			{
				if (lValueDataType.Size >= rValueDataType.Size)
					return new SimpleAssignmentStat(lExpression, rValue);
				else {
					this.report.SemanticError(188, rValue.ToString(), lExpression.ToString(), location);
					return Statement.Empty;
				}
			}
			if (this.ConversionOperatorExists(rValueDataType, lValueDataType))
				this.report.SemanticError(64, rValueDataType.Name, lValueDataType.Name, location);
			else
				this.report.SemanticError(150, rValueDataType.Name, lValueDataType.Name, location);
			return Statement.Empty;
		}
	}

	private StatementList MakeStatementList(Statement statement)
	{
		StatementList statementList = new StatementList();
		if (statement != null && statement != Statement.Empty)
		{
			statementList.Add(statement);
			this.CopyValue(statement.IsFunctionValueDefined);
		}
		return statementList;
	}

	private StatementList AddToStatementList(StatementList statementList, Statement statement, LexLocation location)
	{
		if (statementList != null && statement != null)
		{
			if (statement == Statement.Empty)
				return statementList;
			else if (statementList.Count > 0 && statementList.Last.ControlFlowTerminates)
				this.report.Warning(2, location);
			else {
				statementList.Add(statement);
				this.CopyValue(statement.IsFunctionValueDefined);
			}
		}
		return statementList;
	}

	private StatementList MakeCaseStatList(Statement statement)
	{
		StatementList statementList = new StatementList();
		if (statement != null && statement != Statement.Empty)
		{
			statementList.Add(statement);
			this.CopyValue(statement.IsFunctionValueDefined);
		}
		this.CheckForEndOfCaseStatList();
		return statementList;
	}

	private StatementList AddToCaseStatList(StatementList statementList, Statement statement, LexLocation location)
	{
		if (statementList != null && statement != null)
		{
			if (statement == Statement.Empty)
				return statementList;
			else if (statementList.Count > 0 && statementList.ControlFlowTerminates)
				this.report.Warning(2, location);
			else {
				statementList.Add(statement);
				this.CopyValue(statement.IsFunctionValueDefined);
			}
		}
		this.CheckForEndOfCaseStatList();
		return statementList;
	}

	private void CheckForEndOfCaseStatList()
	{
		STLangScanner scanner = (STLangScanner)this.Scanner;
		Tokens token = scanner.GetNextToken();
		switch (token)
		{
		case Tokens.IF:
		case Tokens.ELSE:
		case Tokens.WHILE:
		case Tokens.FOR:
		case Tokens.REPEAT:
		case Tokens.CASE:
		case Tokens.EXIT:
		case Tokens.END_CASE:
			// Control flow statement. 
			scanner.DoPushBack(false); 
		    break;

        case Tokens.TRUE:           
		case Tokens.FALSE:        
		case Tokens.INT_LIT:      
		case Tokens.REAL_LIT:     
		case Tokens.TOD_LIT:      
		case Tokens.TIME_LIT:     
		case Tokens.DATE_LIT:     
		case Tokens.DT_LIT:        
		case Tokens.STRING_LIT:        
		case Tokens.WSTRING_LIT:     
		case Tokens.TYPED_INT:       
		case Tokens.TYPED_REAL:   
		case Tokens.TYPED_ENUM:
		    // Constant ==> End of statement.
			scanner.DoPushBack(true);
			break;

		case Tokens.IDENT:
			token = scanner.GetNextToken();
			if (token == Tokens.ASSIGN)
				scanner.DoPushBack(false);
			else if (token == Tokens.DOTDOT || (char)token == ',' || (char)token == ':')
				scanner.DoPushBack(true);
			else if ((char)token != '(')
				scanner.DoPushBack(false);
			else {
				// Determine whether it is a function call or function block call
				STLangSymbol symbol;
				string ident = scanner.GetBufferedValue(0).Ident;

				if (! this.symbolTable.Lookup(ident, out symbol, null))
					scanner.DoPushBack(false);
				else if (symbol.IsFunctionBlock)
					scanner.DoPushBack(false); // Statement
				else
					scanner.DoPushBack(true);  // Expression
			}
			break;

		default:
			scanner.DoPushBack(true);
			break;
        }
	}

	private void PushSymbolTable(int declarationType = -1)
	{
		if (declarationType == 0)
			this.isFunctionDecl = true;
		else if (declarationType == 1)
			this.isFunctionBlockDecl = true;
		else if (declarationType == 2)
			this.isProgramDecl = true;
		this.symbolTable.Push();
	}

	private void PopSymbolTable()
	{
		this.symbolTable.Pop();
	}

	private void StoreInputOutputData(POUVarDeclarations pouDeclarations)
	{
		IEnumerable<InstanceSymbol> inputs = pouDeclarations.InputParameters;
		IEnumerable<InstanceSymbol> outputs = pouDeclarations.OutputParameters;
		IEnumerable<InstanceSymbol> retainedVars = pouDeclarations.RetainedVariables;

		foreach (InstanceSymbol input in inputs)
			ByteCodeGenerator.StoreInputParameter(input.Name, input.Position);
		foreach (InstanceSymbol output in outputs)
			ByteCodeGenerator.StoreOutputParameter(output.Name, output.Location.Index);
		foreach (InstanceSymbol retainedVar in retainedVars)
			ByteCodeGenerator.StoreRetainedVariable(retainedVar.Name, retainedVar.Location.Index);
		ByteCodeGenerator.StoreIOParameterCount();
	}

	// void GenerateInitInputs(POUVarDeclarations pouDeclarations)
	//
	// Generates code that initializes input parameters (VAR_INPUT).
	//
	private void GenerateInitInputs(POUVarDeclarations pouDeclarations)
	{
		Expression inputParam;
		IEnumerable<InstanceSymbol> inputs;

		inputs = pouDeclarations.InputParameters.Reverse();
		foreach (InstanceSymbol input in inputs)
		{
			inputParam = input.MakeSyntaxTreeNode(null);
			inputParam.GenerateStore();
		}
	}

	// void GenerateInitLocalVars(POUVarDeclarations pouDeclarations)
	//
	// Generates code that initializes local variables (VAR, VAR_TEMP (and VAR_OUTPUT in functions).
	//
	private void GenerateInitLocalVars(POUVarDeclarations pouDeclaration, bool isFunction)
    {
		IEnumerable<DeclarationStatement> localVariables;
		if (isFunction)
		{
			localVariables = pouDeclaration.LocalVariables;
			foreach (DeclarationStatement localVariable in localVariables)
			{
				localVariable.GenerateCode();
			}
		}
		else {
		}
	}

	private void GenerateStatementList(StatementList statementList)
	{
		if (statementList == null)
			throw new STLangCompilerError(Resources.GENFCNBODY);
		else
			statementList.GenerateCode();
	}

    string GetBaseName(string sourceFileName)
    {
        char[] separators = new char[] { '.' };
        string[] result = sourceFileName.Split(separators);
        if (result.Length > 0)
            return result[0];
        else
            return "";
    }

    private void GenerateObjectCode(string path, string sourceFile)
    {
		string executableFile;
		POUVarDeclarations varDeclarations;
		RWMemoryLayoutManager rwMemoryManager;
		ROMemoryLayoutManager roMemoryManager;
		IEnumerable<ProgramOrganizationUnitSymbol> pouSymbols;

		roMemoryManager = new ROMemoryLayoutManager();
		GenericConstant.ConstMemoryManager = roMemoryManager;
		pouSymbols = this.symbolTable.ProgramOrganizationUnits;
		executableFile = this.GetBaseName(sourceFile) + ".xstl";
		foreach (ProgramOrganizationUnitSymbol pouSymbol in pouSymbols)
		{
			foreach (ProgramOrganizationUnit pou in pouSymbol.Definitions)
			{
				varDeclarations = pou.VarDeclarations;
				rwMemoryManager = pou.RWMemoryLayout;
				constantTable = pou.ConstantTable;
				this.StoreInputOutputData(varDeclarations);
				this.GenerateInitInputs(varDeclarations);
                this.GenerateInitLocalVars(varDeclarations, pouSymbol.IsFunction);
                this.GenerateStatementList(pou.Body);
				ByteCodeGenerator.StorePOUName(pou.Name);
				ByteCodeGenerator.StoreRWDataSegmentInfo(rwMemoryManager);
				ByteCodeGenerator.StoreConstantTable(constantTable);
				ByteCodeGenerator.StoreRODataSegmentInfo(roMemoryManager);
				ByteCodeGenerator.OptimizeByteCode();
				ByteCodeGenerator.CreateExecutable(path, executableFile);
				roMemoryManager.Reset();
			}
		}
    }

	internal STLangPOUObject GenerateObjectCode(bool overWrite)
    {
		STLangPOUObject pouObject;
		POUVarDeclarations varDeclarations;
		RWMemoryLayoutManager rwMemoryManager;
		ROMemoryLayoutManager roMemoryManager;
		IEnumerable<ProgramOrganizationUnitSymbol> pouSymbolList;
		List<STLangPOUObject> pouObjectList = new List<STLangPOUObject>();

		roMemoryManager = new ROMemoryLayoutManager();
		GenericConstant.ConstMemoryManager = roMemoryManager;
		pouSymbolList = this.symbolTable.ProgramOrganizationUnits;
		foreach (ProgramOrganizationUnitSymbol pouSymbol in pouSymbolList)
		{
			foreach (ProgramOrganizationUnit pou in pouSymbol.Definitions)
			{
				rwMemoryManager = pou.RWMemoryLayout;
				varDeclarations = pou.VarDeclarations;
				constantTable = pou.ConstantTable;
				this.StoreInputOutputData(varDeclarations);
				this.GenerateInitInputs(varDeclarations);
                this.GenerateInitLocalVars(varDeclarations, pouSymbol.IsFunction);
                this.GenerateStatementList(pou.Body);
				ByteCodeGenerator.StorePOUName(pou.Name);
				ByteCodeGenerator.StoreRWDataSegmentInfo(rwMemoryManager);
				ByteCodeGenerator.StoreConstantTable(constantTable);
				ByteCodeGenerator.StoreRODataSegmentInfo(roMemoryManager);
				ByteCodeGenerator.OptimizeByteCode();
				pouObject = ByteCodeGenerator.CreateExecutable(pou.Name, overWrite);
				pouObjectList.Add(pouObject);
				roMemoryManager.Reset();
			}
		}
		return pouObjectList[0];
    }

	public bool Parse(string path, string sourceFile)
	{
		this.ReInitializeParser();
		bool result = this.Parse();
		if (!result || this.Errors > 0)
			return false;
		else {
			this.GenerateObjectCode(path, sourceFile);
			return true;
		}
	}