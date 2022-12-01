using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using STLang.Scanner;
using STLang.ImplDependentParams;
using System.Collections;
using QUT.Gppg;

namespace STLang.ErrorManager
{
    public class ErrorHandler
    {
        public ErrorHandler()
        {
            this.errors = 0;
            this.warnings = 0;
            this.errorMessages = new List<string>();
            this.warningMessages = new List<string>();
        }

        static ErrorHandler()
        {
            errorMessage = new Hashtable();
            InitializeMessageMap();
        }

        private string GetLineAndColumn(LexLocation location)
        {
            if (location == null)
                return "";
            else
                return "<" + location.StartLine + "," + location.StartColumn + ">";
        }

        public void SemanticError(string msg, LexLocation location)
        {
            string pos = this.GetLineAndColumn(location);
            this.errorMessages.Add(pos + msg);
            this.errors++;
        }

        public void SemanticError(int code, LexLocation location)
        {
            if (errorMessage.Contains(code))
            {
                string pos = this.GetLineAndColumn(location);
                string msg = pos + (string)errorMessage[code];
                this.errorMessages.Add(msg);
                this.errors++;
            }
        }

        public void SemanticError(int code, object arg1, LexLocation location)
        {
            if (errorMessage.Contains(code))
            {
                string pos = this.GetLineAndColumn(location);
                string msg = (string)errorMessage[code];
                string tmp = pos + string.Format(msg, arg1);
                this.errorMessages.Add(tmp);
                this.errors++;
            }
        }

        public void SemanticError(int code, object arg1, object arg2, LexLocation location)
        {
            if (errorMessage.Contains(code))
            {
                string pos = this.GetLineAndColumn(location);
                string msg = (string)errorMessage[code];
                string tmp = pos + string.Format(msg, arg1, arg2);
                this.errorMessages.Add(tmp);
                this.errors++;
            }
        }

        public void SemanticError(int code, object arg1, object arg2, object arg3, LexLocation location)
        {
            if (errorMessage.Contains(code))
            {
                string pos = this.GetLineAndColumn(location);
                string msg = (string)errorMessage[code];
                string tmp = pos + string.Format(msg, arg1, arg2, arg3);
                this.errorMessages.Add(tmp);
                this.errors++;
            }
        }

        public void Warning(int code, LexLocation location)
        {
            if (code >= 0 && code < warningMessage.Count() && location != null)
            {
                string pos = this.GetLineAndColumn(location);
                string msg = (string)warningMessage[code];
                string warning = pos + msg;
                this.warningMessages.Add(warning);
                this.warnings++;
            }
        }

        public void Warning(int code, object arg, LexLocation location)
        {
            if (code >= 0 && code < warningMessage.Count() && location != null)
            {
                string pos = this.GetLineAndColumn(location);
                string msg = warningMessage[code];
                string warning = pos + string.Format(msg, arg);
                this.warningMessages.Add(warning);
                this.warnings++;
            }
        }

        public void Warning(int code, object arg1, object arg2, LexLocation location)
        {
            if (code >= 0 && code < warningMessage.Count() && location != null)
            {
                string pos = this.GetLineAndColumn(location);
                string msg = warningMessage[code];
                string warning = pos + string.Format(msg, arg1, arg2);
                this.warningMessages.Add(warning);
                this.warnings++;
            }
        }

        public void SyntaxError(int code, LexLocation location)
        {
            if (errorMessage.Contains(code))
            {
                this.errors++;
                string pos = this.GetLineAndColumn(location);
                string msg = (string)errorMessage[code];
                string syntaxError = pos + msg;
                this.errorMessages.Add(syntaxError);
            }
        }

        public void SyntaxError(string msg, LexLocation location)
        {
            this.errors++;
            string pos = this.GetLineAndColumn(location);
            string syntaxError = pos + msg;
            this.errorMessages.Add(syntaxError);
        }

        public void SyntaxError(int code, object arg, LexLocation location)
        {
            if (errorMessage.Contains(code))
            {
                this.errors++;
                string pos = this.GetLineAndColumn(location);
                string msg = (string)errorMessage[code];
                string syntaxError = pos + string.Format(msg, arg);
                this.errorMessages.Add(syntaxError);
            }
        }

        public STLangScanner Scanner { get; set; }

        public int Errors
        {
            get { return this.errors; }
        }

        public int Warnings
        {
            get { return this.warnings; }
        }

        public IEnumerable<string> ErrorMessages
        {
            get 
            {
                foreach (string error in this.errorMessages)
                {
                    yield return error;
                }
            }
        }

        public IEnumerable<string> WarningMessages
        {
            get
            {
                foreach (string warning in this.warningMessages)
                {
                    yield return warning;
                }
            }
        }

        private int errors;

        private int warnings;

        private readonly List<string> errorMessages;

        private readonly List<string> warningMessages;

        static private readonly Hashtable errorMessage;

        private static void InitializeMessageMap()
        {
            errorMessage.Add(0, "Identifierare '{0}' ej deklarerad.");
            errorMessage.Add(1, "Identifierare '{0}' redan deklarerad.");
            errorMessage.Add(-9, "Identifierare '{0}' används redan som uppräknad konstant.");
            errorMessage.Add(-4, "Typen {0} redan definierad.");
            errorMessage.Add(2, "{0} är ej ett fält.");
            errorMessage.Add(3, "{0} är ej en struktur.");
            errorMessage.Add(4, "{0} är ej medlem i strukturen {1}.");
            errorMessage.Add(5, "Otillåten användning av {0} {1}.");
            errorMessage.Add(6, "För många parametrar till funktionen '{0}'.");
            errorMessage.Add(7, "Det finns ingen överlagrad funktion som matchar anropet {0}.");
            errorMessage.Add(8, "Otillåten tilldelning: {0} är deklarerad som konstant.");
            errorMessage.Add(9, "Konstanta variabler måste initieras i deklarationen.");
            errorMessage.Add(10, "Fältnamn måste tilldelas ett värde i initieringslistan för strukturer.");
            errorMessage.Add(11, "Definitionsområdena för delintervalltyperna {0} och {1} är disjunkta.");
            errorMessage.Add(12, "Delintervallets övre gräns, {0}, överskrider datatypen {1}'s max-värde.");
            errorMessage.Add(13, "Funktionsvärdet odefinierad. Funktionsnamnet måste tilldelas ett värde innan RETURN/END_FUNCTION.");
            errorMessage.Add(14, "Binära operatorn {0} kan ej appliceras på operander av typerna {1} och {2}.");
            errorMessage.Add(-14, "Unära operatorn {0} kan ej appliceras på operand av typen {1}.");
            errorMessage.Add(15, "Delintervall måste vara av heltalstyp: {0}");
            errorMessage.Add(-1, "Undre och övre gräns i delintervall måste vara av diskret typ: {0}");
            errorMessage.Add(-2, "Typen på intervallet {0} stämmer inte överens med grundtypen {1}.");
            errorMessage.Add(-3, "Delintervallet {0} ligger inte inom definitionsområdet för bastypen {1}. ");
            errorMessage.Add(16, "Undre gräns större än övre gräns i delintervallet {0}.");
            errorMessage.Add(17, "Okänd filtyp. Filnamnet måste sluta på '.stl'.");
            errorMessage.Add(18, "Anropet {0} är tvetydigt mellan följande funktioner: {1}.");
            errorMessage.Add(19, "Filen '{0}' kunde inte skapas.");
            errorMessage.Add(20, "För många index till fältet '{0}'.");
            errorMessage.Add(21, "EXIT kan bara användas inuti en FOR-, WHILE- eller REPEAT-sats.");
            errorMessage.Add(22, "Strängstorleken måste vara ett konstant uttryck.");
            errorMessage.Add(-5, "Strängstorleken måste vara ett heltalsuttryck.");
            errorMessage.Add(23, "Delintervall av typen '{0}' ej tillåtet.");
            errorMessage.Add(24, "{0} är en/ett {1} men används som ett typnamn.");
            errorMessage.Add(-7, "Uppräknad datatyp förväntades: {0}");
            errorMessage.Add(-8, "{0} är ej en uppräknad konstant.");
            errorMessage.Add(25, "Steglängden i FOR-satser får ej vara 0.");
            errorMessage.Add(26, "För många fält i initierings listan för strukturen.");
            errorMessage.Add(27, "Datatyperna i höger- och vänsterled i tilldelningen är oförenliga ({0}, {1}).");
            errorMessage.Add(28, "Fältindex ligger utanför indexgränserna.");
            errorMessage.Add(29, "Strängstorleken måste vara större än 0.");
            errorMessage.Add(-6, "Maxstorleken för strängar överskriden: {0}");
            errorMessage.Add(30, "{0}: Fältindex måste vara av heltalstyp.");
            errorMessage.Add(31, "{0}: Endast värden av diskret typ får förekomma i case-satsers värdelista.");
            errorMessage.Add(32, "Värdet {0} i case-satsens värdelista måste vara konstant.");
            errorMessage.Add(33, "Dubblet av konstanten {0} funnen i CASE-satsen.");
            errorMessage.Add(34, "Villkorsuttrycket i {0}-satsen måste vara av boolsk typ.");
            errorMessage.Add(35, "Strängen {0} är för lång i tilldelningen.");
            errorMessage.Add(36, "Högerledet {0} i tilldelningen ligger utanför vänsterledets definitionsområde.");
            errorMessage.Add(37, "Oavslutad kommentar (Oväntat slut på filen).");
            errorMessage.Add(38, "Oavslutad sträng (Oväntat slut på filen).");
            errorMessage.Add(39, "Icke skrivbart tecken i textsträngen: 0x{0}.");
            errorMessage.Add(40, "Ogiltigt specialtecken '${0}' i textsträngen.");
            errorMessage.Add(41, "Nyrad i strängar ej tillåtet.");
            errorMessage.Add(42, "Division med noll !");
            errorMessage.Add(43, "Komma, ) eller binär operator förväntas.");
            errorMessage.Add(44, "Gränserna i delintervallet har olika typ.");
            errorMessage.Add(45, "Delintervall av typen '{0}' ej tillåtet.");
            errorMessage.Add(46, "Funktioner kan ej deklarera variabler av typen {0}.");
            errorMessage.Add(47, "RETURN kan ej användas för att returnera funktionsvärdet.");
            errorMessage.Add(48, "Funktionen '{0}' används som en procedur.");
            errorMessage.Add(49, "Styrvariablen {0} i FOR-slingan måste vara av enkel datatyp.");
            errorMessage.Add(50, "Maximala antalet tillåtna fältindex överskriden.");
            errorMessage.Add(51, "Generiska typer kan ej användas i program.");
            errorMessage.Add(52, "Delintervallen {0} och {1} överlappar i CASE-satsens konstantlista.");
            errorMessage.Add(53, "Konstanten {0} finns redan i delintervallet {1} i CASE-satsens konstantlista.");
            errorMessage.Add(54, "Otillåten användning av symbolen {0}: Endast variabler eller funktionsnamn får förekomma i vänsterledet.");
            errorMessage.Add(55, "Kan ej applicera {0} på operandparet av typen ({1}, {2}).");
            errorMessage.Add(56, "Kan ej applicera {0} på vänster operand av typen {1}.");
            errorMessage.Add(57, "Kan ej applicera {0} på höger operand av typen {1}.");
            errorMessage.Add(58, "Parametrar kan ej ha attributet '{0}'.");
            errorMessage.Add(59, "Endast inparametrar får ha attributen R_EDGE/F_EDGE.");
            errorMessage.Add(60, "Endast inparametrar av boolsk typ kan ha attributen R_EDGE/F_EDGE.");
            errorMessage.Add(61, "Start-, stop- och inkrementvärde i FOR-satser måste vara av heltalstyp.");
            errorMessage.Add(62, "Fältet '{0}' redan deklarerad.");
            errorMessage.Add(63, "; förväntades.");
            errorMessage.Add(64, "Implicit konvertering från typen {0} till {1} inte möjlig i tilldelningen. (Det finns en konverteringsoperator)");
            errorMessage.Add(65, "Fann initierings lista för en struktur/funktionsblock istället för förväntat värde av typen {0}.");
            errorMessage.Add(66, "Tilldelning av funktionsnamnet '{0}' utanför funktionskroppen.");
            errorMessage.Add(67, "Otillåten användning av typnamnet {0}.");
            errorMessage.Add(68, "Funktionen '{0}' saknar parameterlista.");
            errorMessage.Add(69, "FOR-slingors styrvariabel och startvärde måste vara av samma heltalstyp.");
            errorMessage.Add(70, "FOR-slingors styrvariabel och stopvärde måste vara av samma heltalstyp.");
            errorMessage.Add(71, "FOR-slingors styrvariabel och inkrement måste vara av samma heltalstyp.");
            errorMessage.Add(72, "Komma saknas i variabelistan.");
            errorMessage.Add(73, "Komma saknas i listan av delintervall");
            errorMessage.Add(74, "Komma saknas i konstantlistan.");
            errorMessage.Add(75, ", saknas i listan.");
            errorMessage.Add(76, ") saknas i listan med uppräknade konstanter.");
            errorMessage.Add(77, "Funktionen {0} ej implementerad.");
            errorMessage.Add(78, "{0}: Argumentet motsvarande den VAR_IN_OUT deklarerade parametern {1} måste var ett L_värde.");
            errorMessage.Add(79, "Funktionsnamnet {0} får ej tilldelas ett värde inuti en annan funktion.");
            errorMessage.Add(80, "DO saknas efter villkoret i WHILE-satsen");
            errorMessage.Add(81, "THEN saknas efter villkoret i IF-satsen");
            errorMessage.Add(82, "END_REPEAT saknas");
            errorMessage.Add(83, "OF saknas i CASE-satsen");
            errorMessage.Add(84, "Strukturmedlemen {0} redan initierad.");
            errorMessage.Add(85, "Fann initierings lista för ett fält istället för förväntat värde av typen {0}.");
            errorMessage.Add(86, "Datatypen på konstanten/intervallet {0} i värdelistan och CASE-uttryckets datatyp är oförenliga ({1}, {2}).");
            errorMessage.Add(87, "'TO' saknas i FOR-satsen.");
            errorMessage.Add(88, "Endast en else-sats får finnas i en case-sats.");
            errorMessage.Add(89, "Upprepningsfaktorn i initieringslistan får ej vara noll.");
            errorMessage.Add(90, "{0}: Rekursion ej tillåten.");
            errorMessage.Add(91, ": saknas i konstantlistan i CASE-satsen.");
            errorMessage.Add(92, "Oändlig WHILE-loop.");
            errorMessage.Add(93, "Oändlig REPEAT-loop.");
            errorMessage.Add(94, "Övre och undre gräns i delintervallet av olika datatyp: {0}");
            errorMessage.Add(95, "Övre och undre gräns i delintervallet måste vara konstant: {0}");
            errorMessage.Add(96, "Otillåtet escape-tecken funnet i ascii-sträng: {0}");
            errorMessage.Add(97, "Otillåtet escape-tecken funnet i unicode-sträng: {0}");
            errorMessage.Add(98, "Kommentar för lång.");
            errorMessage.Add(99, "Idenfierare förväntas vid deklaration av direkt representerad variabel med ospecificerad address.");
            errorMessage.Add(100, "Heltal för stort: {0}.");
            errorMessage.Add(101, "Flyttal ligger utanför tillåtna gränser: {0}");
            errorMessage.Add(102, "Tidsinterval ligger utanför tillåtna gränser: {0}");
            errorMessage.Add(103, "Datum ligger utanför tillåtna gränser: {0}");
            errorMessage.Add(104, "Datumtid ligger utanför tillåtna gränser: {0}");
            errorMessage.Add(105, "Tid ligger utanför tillåtna gränser: {0}");
            errorMessage.Add(106, "Antal hierarkiska nivåer överskriden för direkta variabler: {0}");
            errorMessage.Add(107, "Logisk address för direkta variabler överskriden: {0}.");
            errorMessage.Add(108, "Konstanten {0} i CASE-satsen ligger utanför definitionsområdet för case-uttryckets datatyp ({1}).");
            errorMessage.Add(109, "Icke skrivbart tecken {0} i textsträngen.");
            errorMessage.Add(110, "Funktionsvärdet kan vara odefinierat vid återhopp.");
            errorMessage.Add(111, "Syntaxfel vid symbolen {0}.");
            errorMessage.Add(112, "Tvetydig användning av uppräknad konstant {0}. Använd 'typnamn'#{0} för att göra den entydig.");
            errorMessage.Add(113, "Maximal storlek på vektorer överskriden.");
            errorMessage.Add(114, "Otillåten typkonvertering: Kan ej konvertera värdet {0} till typen {1}.");
            errorMessage.Add(115, "Antalet tillåtna nestlade strukturer överskriden.");
            errorMessage.Add(116, "Flyttalsspill i konstant uttryck: {0}");
            errorMessage.Add(117, "Spill i konstant tidsintervall-uttryck: {0}");
            errorMessage.Add(118, "Spill i konstant datum-tid-uttryck: {0}");
            errorMessage.Add(119, "Spill i konstant tidsuttryck: {0}");
            errorMessage.Add(120, "Endast boolska variabler kan deklareras som R_EDGE/F_EDGE.");
            errorMessage.Add(121, "Strukturfält kan ej deklareras som R_EDGE/F_EDGE.");
            errorMessage.Add(122, "Repetionsfaktorer kan bara användas vid initiering av fält.");
            errorMessage.Add(123, "Indexet {0} ligger utanför fältet {1}'s undre gräns ({2}).");
            errorMessage.Add(124, "Indexet {0} ligger utanför fältet {1}'s övre gräns ({2}).");
            errorMessage.Add(125, "FOR-slingans styrvariabel '{0}' får ej ändras inuti slingan.");
            errorMessage.Add(126, "FOR-slingans startvärde '{0}' får ej ändras inuti slingan.");
            errorMessage.Add(127, "FOR-slingans stopvärde '{0}' får ej ändras inuti slingan.");
            errorMessage.Add(128, "Variablen {0} måste vara konstant om den används som start-/stopvärde in en FOR-slinga. ");
            errorMessage.Add(129, "Direkt representerade variabeln {0} ej deklarerad.");
            errorMessage.Add(130, "Typen på konstanten {0} i CASE-satsen överenstämmer inte med typen på selektorn ({1}).");
            errorMessage.Add(131, "Styrvariablen {0} i FOR-slingan måste vara av heltalstyp.");
            errorMessage.Add(132, "Startvärdet för styrvariabeln {0} i FOR-satsen måste var av heltalstyp.");
            errorMessage.Add(133, "Stop-uttrycket {0} i FOR-satsen måste var av heltalstyp.");
            errorMessage.Add(134, "Steg-uttrycket {0} i FOR-satsen måste var av heltalstyp.");
            errorMessage.Add(135, "Otillåten användning av {0}: Funktionsblock kan ej användas som funktioner.");
            errorMessage.Add(136, "{0} är ej medlem i funktionsblocket {1}.");
            errorMessage.Add(137, "{0} deklarerade variabeln {1} är ej åtkomlig utanför funktionsblocket {2}.");
            errorMessage.Add(138, "{0} kan ej tilldelas: IN-parametrar är endast läsbara.");
            errorMessage.Add(139, "{0}: UT-variabler kan endast tilldelas inuti funktionsblock.");
            errorMessage.Add(140, "{0}: Typkonverteringsfunktioner tar alltid en parameter.");
            errorMessage.Add(141, "{0} är ej en inparameter till funktionsblocket {1}.");
            errorMessage.Add(142, "{0}: Typerna på formella parametern {1} och aktuella parametern är oförenliga.");
            errorMessage.Add(143, "Medlemsvariabler måste tilldelas ett värde i initieringslistan för funktionsblock.");
            errorMessage.Add(144, "Endast åtkommliga medlemmar i funktionsblock kan initieras.");
            errorMessage.Add(145, "Identifierare {0} är deklarerad som en {1} men används som en {2}.");
            errorMessage.Add(146, "Endast en ELSE-del får finnas i en CASE-sats.");
            errorMessage.Add(147, "CASE-uttryckets typ måste vara av heltalstyp eller uppräknad datatyp.");
            errorMessage.Add(148, "Otillåten användning av {0}: Funktionsnamn får endast förekomma i vänsterledet i en tilldelning.");
            errorMessage.Add(149, "Otillåten tilldelning av {0}: Funktionsnamn får endast tilldelas ett värde inuti den egna funktionskroppen.");
            errorMessage.Add(150, "Implicit konvertering från typen {0} till {1} inte möjlig i tilldelningen.");
            errorMessage.Add(151, "Typen {0} är odefinierad.");
            errorMessage.Add(152, "{0}: Anrop av funktionsblock inuti funktioner är ej tillåtet.");
            errorMessage.Add(153, "Identifierare {0} används redan som standardfunktion.");
            errorMessage.Add(154, "Identifierare {0} används redan som standardfunktionsblock.");
            errorMessage.Add(155, "Omdefiniering av datatypen {0}.");
            errorMessage.Add(156, "Funktioner/funktionsblock kan ej deklarera variabler av typen {0}.");
            errorMessage.Add(157, "Funktionsblock kan ej deklarera variabler av typen {0}.");
            errorMessage.Add(158, "Variabeltypen {0} kan ej ha attributet {1}.");
            errorMessage.Add(159, "{0}: Kantdetektion är ej tillåtet i funktioner.");
            errorMessage.Add(160, "Endast variabler av boolsk typ kan ha attributet {0}.");
            errorMessage.Add(161, "{0}: Direkt representerade variabler kan ej användas i funktioner/funktionsblock.");
            errorMessage.Add(162, "{0}: Funktionsblock med minne är ej tillåtet i funktioner.");
            errorMessage.Add(163, "{0}: Funktionsblock med kantdetektion är ej tillåtet i funktioner.");
            errorMessage.Add(164, "Fält av funktionsblock är ej tillåtet.");
            errorMessage.Add(165, "Funktionsblock får ej användas som strukturmedlemmar.");
            errorMessage.Add(166, "Lista med uppräknade konstanter får ej vara tom.");
            errorMessage.Add(167, "{0}: För många fältindex i listan.");
            errorMessage.Add(168, "Otillåten tilldelning. {0} är en inparameter.");
            errorMessage.Add(169, "{0}: Uppräknade konstanter som är typade får ej förekomma i listan.");
            errorMessage.Add(170, "Otillåten tilldelning. Vänsterledet {0} är ett konstant uttryck.");
            errorMessage.Add(171, "Värdet {0} ligger utanför definitionsområdet för datatypen {1}.");
            errorMessage.Add(172, "Delintervallet {0} i CASE-satsen ligger utanför definitionsområdet för selektorns datatyp {1}.");
            errorMessage.Add(173, "Felaktig användning av :=. Använd => för att tilldela {0} värdet av utparametern {1}.");
            errorMessage.Add(174, "{0}: Felaktig syntax vid anrop av funktionsblock. Inparametrarna"
                                + "måste tilldelas ett värde i parameterlistan.");
            errorMessage.Add(175, "Felaktig användning av =>. Formella parametern {0} är ej en utparameter.");
            errorMessage.Add(176, "Felaktig användning av :=. Använd => för att komma åt värdet av utparametern {0}.");
            errorMessage.Add(177, "{0}: Högerledet {1} kan ej konverteras till en {2}.");
            errorMessage.Add(178, "Konstanten {0} ligger utanför definitionsområdet för {1} av typen {2}.");
            errorMessage.Add(179, "{0}: Instanser av funktionsblock får ej deklareras som konstant.");
            errorMessage.Add(180, "Otillåten tilldelning av {0}. Endast in(-ut)parameterar får tilldelas ett värde vid proceduranrop.");
            errorMessage.Add(181, "VAR_IN_OUT deklarerade variabler kan ej ha begynnelsevärden.");
            errorMessage.Add(182, "{0}: Otillåtet anrop av konstant ({1}) instans av funktionsblock.");
            errorMessage.Add(183, "{0}: Otillåtet anrop av VAR_IN_OUT deklarerad funktionsblock.");
            errorMessage.Add(184, "{0}: NOT används bara för negering av boolska utparametrar.");
            errorMessage.Add(185, "{0}: Anrop med blandade förekomster av argument med/utan tilldeling av inparametrar ej tillåtet.");
            errorMessage.Add(186, "{0}: Direkt representerade variabler får bara deklareras i huvudprogram.");
            errorMessage.Add(187, "{0}: Parameter nr. {1} förväntas vara av typen {2}.");
            errorMessage.Add(188, "Strängen i högerledet {0} längre än strängen i vänsterledet {1}.");
            errorMessage.Add(189, "{0}: Högerledet i {1} => måste vara ett L-värde.");
            errorMessage.Add(190, "Uppräknad konstant {0} förväntades i deltintervall av uppräknad typ {1}.");
            errorMessage.Add(191, "Uppräknad konstant {0} i delintervallet av annan typ än uppräknad grundtyp {1}.");
            errorMessage.Add(192, "Nyckelord {0} funnet där identifierare förväntades.");
            errorMessage.Add(193, "Uttrycket för namngivna värdet {0} måste vara konstant.");
        }

        private static readonly string[] warningMessage = 
        {
/* 0 */	    "':=' : Konvertering från {0} till {1}, möjlig förlust av data.",
/* 1 */	    "Variabeln '{0}' deklarerad men används ej.",
/* 2 */	    "Ej nåbar kod: Satser efter RETURN/EXIT exekveras aldrig.",
/* 3 */	    "Datatypen {0} ej implementerad.", 
/* 4 */	    "Direkt representerade variabler används ej.", 
/* 5 */	    "Attributet {0} ej implementerad.",
/* 6 */	    "Endast de första {0} tecknen i identifierare är signifikanta: {1}",
/* 7 */  	"Unärt minus applicerat på ett värde av typen {0} har ingen verkan.",
/* 8 */	    "Funktionen '{0}' ej implementerad.",
/* 9 */	    "{0}: Jämförelse mellan heltal med tecken och heltal utan tecken.",
/* 10 */	"Antalet element i initieringslistan färre än storleken på fältet.",
/* 11 */    "Antalet element i initieringslistan färre än antalet fält i strukturen.",
/* 12 */	"Variablen '{0}' används innan den är initierad.", //////// << Används ej
/* 13 */	"'{0}': Konvertering av parameter {1} från {2} till {3}, möjlig förlust av data.",
/* 14 */    "Delintervallet {0} i konstantlistan ligger delvis utanför definitionsområdet för CASE-uttryckets datatyp {1}.",
/* 15 */    "Modifiering av inkrement variabeln {0} kan ha oförutsedda effekter.",
/* 16 */    "{0}: Anrop av funktion där sats förväntades.",
/* 17 */    "Delintervallet {0} i konstantlistan ligger utanför definitionsområdet för CASE-uttryckets datatyp.",
/* 18 */    "Tilldelning av stegvariabeln {0} kan få oförutsedda effekter.",
/* 19 */    "Tom initierings lista.",
/* 20 */    "Antalet element i initierings listan färre än storleken på fältet.",
/* 21 */    "Antalet element i initierings listan fler än storleken på fältet.",
/* 22 */    "Fältet {0} saknas i initieringslistan ({0} tilldelas defaultvärdet {1}).",
/* 23 */    "Fältet {0} finns redan i initieringslistan för strukturen.",
/* 24 */    "Medlemmen {0} finns redan i initieringslistan för funktionsblocket.",
/* 25 */    "Konstanten {0} i listan ligger utanför definitionsområdet för CASE-uttryckets datatyp.",
/* 26 */    "CASE-satsen är tom.",
/* 27 */    "CASE-satsen innehåller ett ELSE-block, men inga konstant-block.",
/* 28 */    "Medlemmen {0} saknas i initieringslistan. ({0} tilldelas defaultvärdet).",
/* 29 */    "Möjlig förlust av data vid konvertering från {0} till {1}.",
/* 30 */    "Parameterlista saknas. Formella parametrarna tilldelas default värdena."
        };
    }
}
