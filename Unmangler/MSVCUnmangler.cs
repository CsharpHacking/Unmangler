using System;
using System.Collections.Generic;
using System.Text;
using Utils.Types.String;

namespace Unmangler
{
    // Attempts to unmangle MSVC++ mangled functions
    // https://en.wikiversity.org/wiki/Visual_C%2B%2B_name_mangling
    public class MSVCUnmangler
    {
        private static string MangledString;
        private static UnmangleFlags Flags;
        private int CurrentPosition;

        private char ParsedSymbol
        {
            get => MangledString[CurrentPosition];

            init
            {
                if (value < 0) throw new ArgumentOutOfRangeException(nameof(value));
            }
        }

        public MSVCUnmangler(string fncName)
        {
            MangledString = fncName;
            Flags = UnmangleFlags.UNDNAME_COMPLETE;
            ParsedSymbol = char.MinValue;
            CurrentPosition = 0;
        }

        private void ChangePosition(int byInt)
        {
            var positionsRemaining = MangledString.Length - CurrentPosition;

            switch (byInt)
            {
                case > 0 when byInt <= positionsRemaining:
                {
                    var newPos = byInt;
                    //Console.WriteLine($"Changing position in mangled string: \nMangledString: {0} \nCurrentPosition: {1} \nParsedSymbol: {2} \nNewPosition: {3}",
                    //MangledString, CurrentPosition, ParsedSymbol, newPos);
                    CurrentPosition += byInt;
                    break;
                }
                case < 0 when byInt <= CurrentPosition:
                {
                    var newPos = byInt;
                    //Console.WriteLine($"Changing position in mangled string: \nMangledString: {0} \nCurrentPosition: {1} \nParsedSymbol: {2} \nNewPosition: {3}",
                    //MangledString, CurrentPosition, ParsedSymbol, newPos);
                    CurrentPosition -= byInt;
                    break;
                }
            }
        }

        [Flags]
        private enum UnmangleFlags
        {
            UNDNAME_COMPLETE = 0x0000,
            UNDNAME_NO_LEADING_UNDERSCORES = 0x0001, // Don't show __ in calling convention
            UNDNAME_NO_MS_KEYWORDS = 0x0002, // Don't show calling convention at all
            UNDNAME_NO_FUNCTION_RETURNS = 0x0004, // Don't show function/method return value
            UNDNAME_NO_ALLOCATION_MODEL = 0x0008,
            UNDNAME_NO_ALLOCATION_LANGUAGE = 0x0010,
            UNDNAME_NO_MS_THISTYPE = 0x0020,
            UNDNAME_NO_CV_THISTYPE = 0x0040,
            UNDNAME_NO_THISTYPE = 0x0060,
            UNDNAME_NO_ACCESS_SPECIFIERS = 0x0080, // Don't show access specifier = public/protected/private
            UNDNAME_NO_THROW_SIGNATURES = 0x0100,
            UNDNAME_NO_MEMBER_TYPE = 0x0200, // Don't show static/virtual specifier
            UNDNAME_NO_RETURN_UDT_MODEL = 0x0400,
            UNDNAME_32_BIT_DECODE = 0x0800,
            UNDNAME_NAME_ONLY = 0x1000, // Only report the variable/method name
            UNDNAME_NO_ARGUMENTS = 0x2000, // Don't show method arguments
            UNDNAME_NO_SPECIAL_SYMS = 0x4000,
            UNDNAME_NO_COMPLEX_TYPE = 0x8000
        }

        // NOTE: on this mess of local functions
        // https://github.com/dotnet/csharplang/issues/2181 |C# 8.0 switch expressions only work with single return statements. #2181|
        // Q: Why are multi-line statements banned also?
        // A: Multi-line statements aren't expressions. There is a separate proposal which looks into making sequences of statements result in an expression which will enable this scenario.
        // Otherwise, a local function will have to suffice for the time being.
        private static string DefaultGetSpecialName(char curChar)
        {
            Console.WriteLine($"Unknown operator symbol: {curChar}");
            return "";
        }

        private static string DefaultGetSpecialModifiers(char curChar)
        {
            Console.WriteLine($"Unknown special modifier symbol: {0}", curChar);
            return "";
        }

        private static string DefaultGetSpecialNameUnderscore(char curChar)
        {
            Console.WriteLine($"Unknown operator symbol after underscore: {0}", curChar);
            return "";
        }

        private static string DefaultGetSpecialNameSecondUnderscore(char curChar)
        {
            Console.WriteLine($"Unknown operator symbol after second underscore: {0} \nMangledString: {1}", curChar, MangledString);
            return "";
        }

        private static string DefaultGetTypeModifier(char curChar)
        {
            Console.WriteLine($"Unknown type modifier symbol: {0}", curChar);
            return "";
        }

        private static string DefaultGetEnumTypeModifier(char curChar)
        {
            Console.WriteLine($"Unknown enum type symbol: {0}", curChar);
            return "";
        }

        private static string DefaultDemangleDataType(char curChar)
        {
            Console.WriteLine($"Unknown data type symbol: {0}", curChar);
            return "";
        }

        private static string DefaultGetComplexDataType(char curChar)
        {
            Console.WriteLine($"Unknown complex data type symbol: {0}", curChar);
            return "";
        }

        private static string DefaultGetComplexType(char curChar)
        {
            Console.WriteLine($"Unknown complex type symbol: {0}", curChar);
            return "";
        }

        private static string DefaultRTII(char curChar)
        {
            Console.WriteLine($"Unknown RTTI operator symbol: {0}", curChar);
            return "";
        }

        private static string DefaultNumberResolution(char curChar)
        {
            Console.WriteLine($"Unknown encoded number symbol: {0}", curChar);
            return "";
        }

        private static int DefaultHexNumberResolution(char curChar)
        {
            Console.WriteLine($"Unknown hexadecimal number symbol: {0}", curChar);
            return 0;
        }

        private static string WrongSymbolGetSpecialNameUnderscore(char curChar)
        {
            Console.WriteLine($"Wrong operator symbol after underscore: {0}", curChar);
            return "";
        }

        private static string WrongSymbolDemangleDataType(char curChar)
        {
            Console.WriteLine($"Wrong data type symbol: {0}", curChar);
            return "";
        }

        private static string OperatorReturnType(char curChar)
        {
            Flags &= ~UnmangleFlags.UNDNAME_NO_FUNCTION_RETURNS;

            Console.WriteLine($"OperatorReturnType resolved: {0}", curChar);
            return "operator returntype";
        }

        private string GetSpecialName()
        {
            return MangledString[CurrentPosition] switch
            {
                '0' => "null",
                '1' => "null",
                '2' => "operator new",
                '3' => "operator delete",
                '4' => "operator=",
                '5' => "operator>>",
                '6' => "operator<<",
                '7' => "operator!",
                '8' => "operator==",
                '9' => "operator!=",
                'A' => "operator[]",
                'B' => OperatorReturnType(MangledString[CurrentPosition]),
                'C' => "operator->", // "operator." possible??
                'D' => "operator*",
                'E' => "operator++",
                'F' => "operator--",
                'G' => "operator-",
                'H' => "operator+",
                'I' => "operator&",
                'J' => "operator->*", // "operator.*" possible??
                'K' => "operator/",
                'L' => "operator%",
                'M' => "operator<",
                'N' => "operator<=",
                'O' => "operator>",
                'P' => "operator>=",
                'Q' => "operator,",
                'R' => "operator()",
                'S' => "operator~",
                'T' => "operator^",
                'U' => "operator|",
                'V' => "operator&&",
                'W' => "operator||",
                'X' => "operator*=",
                'Y' => "operator+=",
                'Z' => "operator-=",
                '_' => GetSpecialNameUnderscore(),
                _ => DefaultGetSpecialName(MangledString[CurrentPosition]),
            };
        }

        private string ResolveHexadecimalNumber(bool sign)
        {
            var number = 0;
            while (MangledString[CurrentPosition] != '@')
            {
                number = MangledString[CurrentPosition] switch
                {
                    'A' => 0, //0x0
                    'B' => 1, //0x1
                    'C' => 2, //0x2 
                    'D' => 3, //0x3
                    'E' => 4, //0x4
                    'F' => 5, //0x5
                    'G' => 6, //0x6
                    'H' => 7, //0x7
                    'I' => 8, //0x8
                    'J' => 9, //0x9
                    'K' => 10, //0xA
                    'L' => 11, //0xB
                    'M' => 12, //0xC
                    'N' => 13, //0xD
                    'O' => 14, //0xE
                    'P' => 15, //0xF
                    _ => DefaultHexNumberResolution(MangledString[CurrentPosition]),
                };
                ChangePosition(1);
            }

            return $"{(sign ? "-" : "")}{number}";
        }

        private string DecodeNumber()
        {
            var sign = false;

            //prefix
            if (MangledString[CurrentPosition] == '?')
            {
                sign = true;
                ChangePosition(1);
            }

            return MangledString[CurrentPosition] switch
            {
                <= '9' and >= '0' => $"{(sign ? "-" : "")}{MangledString[CurrentPosition]}",
                <= 'P' and >= 'A' => ResolveHexadecimalNumber(sign),
                '@' => $"{(sign ? "-" : "")}0", // TODO: negative zero? :P
                _ => DefaultNumberResolution(MangledString[CurrentPosition]),
            };
        }

        private string GetBaseClassDescriptor()
        {
            var a = DecodeNumber();
            var b = DecodeNumber();
            var c = DecodeNumber();
            var d = DecodeNumber();
            return $"`RTTI Base Class Descriptor at ({a}, {b}, {c}, {d})";
        }

        private string UnmangleDataType()
        {
            ChangePosition(1);
            return MangledString[CurrentPosition] switch
            {
                '?' => GetTypeModifier(), // in case of curPos+1 == 'x' annon type template paramater
                '$' => GetTemplateParameters(),
                <= '9' and >= '0' => "",  // TODO: Back Reference
                'A' => GetTypeModifier(), // reference
                'B' => GetTypeModifier(), // volatile reference
                'C' => "signed char",
                'D' => "char",
                'E' => "unsigned char",
                'F' => "short",
                'G' => "unsigned short",
                'H' => "int",
                'I' => "unsigned int",
                'J' => "long",
                'K' => "unsigned long",
                'L' => WrongSymbolDemangleDataType(MangledString[CurrentPosition]), // should not occur
                'M' => "float",
                'N' => "double",
                'O' => "long double",
                'P' => GetTypeModifier(), // pointer
                'Q' => GetTypeModifier(), // const pointer
                'R' => GetTypeModifier(), // volatile pointer
                'S' => GetTypeModifier(), // const volatile pointer
                'T' or 'U' or 'V' or 'Y' => GetComplexType(),
                'W' => GetEnumTypeModifier(),
                'X' => "void",
                'Z' => "...", // (ellipsis)
                '_' => GetComplexDataType(),
                _ => DefaultDemangleDataType(MangledString[CurrentPosition]),
            };
        }

        private string GetTypeModifier()
        {
            if (MangledString[CurrentPosition + 1] == 'x')
            {
                return "template-parameter-x"; // anonymous type template parameter x ('template-parameter-x')
            }

            var ptrModificator = string.Empty;

            if (MangledString[CurrentPosition] == 'E')
            {
                ptrModificator = " __ptr64";
                ChangePosition(1);
            }

            return MangledString[CurrentPosition] switch
            {
                '?' => "", // None
                'A' => $" &{ptrModificator}",                // Reference modifier none
                'B' => $" &{ptrModificator} volatile",       // Reference modifier volatile
                'P' => $" *{ptrModificator}",                // Pointer modifier none
                'Q' => $" *{ptrModificator} const",          // Pointer modifier const
                'R' => $" *{ptrModificator} volatile",       // Pointer modifier volatile
                'S' => $" *{ptrModificator} const volatile", // Pointer modifier const volatile
                _ => DefaultGetTypeModifier(MangledString[CurrentPosition]),
            };

            // TODO: Optional: Array property (not for functions), multidimensional arrays
        }

        private string GetTemplateParameters()
        {
            ChangePosition(1);
            return MangledString[CurrentPosition] switch
            {
                '$' => GetOtherModifier(),
                '0' => DecodeNumber(), // integer value a 
                '1' => "", // constant pointer to mangled symbol s
                '2' => "", // real value a × 10^(b-k+1), where k is number of decimal digits of a
                'A' => "__gc",  // Optional: Managed C++ property 
                'B' => "__pin", // Optional: Managed C++ property 
                'D' => $"template-parameter{DecodeNumber()}", // anonymous type template parameter a ('template-parametera')
                'F' => GetTwoTuple(), // 2-tuple {a,b} (unknown)
                'G' => GetThreeTuple(), // 3-tuple {a,b,c} (unknown)
                'H' => "", // constant pointer to method s (base offset? a, numeric)
                'I' => "", // constant pointer to method s (offsets? a and b, numeric)
                'J' => "", // (unknown)
                'Q' => $"non-type-template-parameter{DecodeNumber()}", // anonymous non-type template parameter a ('non-type-template-parametera')
                'S' => "", // empty non-type parameter pack
                _ => DefaultGetSpecialModifiers(MangledString[CurrentPosition]),
            };
        }

        private string GetOtherModifier()
        {
            ChangePosition(1);
            return MangledString[CurrentPosition] switch
            {
                'A' => "function", //
                'B' => "", // TODO: Array type in template
                'C' => "",  // None Type modifier?
                'F' => "managed function", // Function modifier (managed function [Managed C++ or C++/CLI])
                'Q' => "rvalue reference", //Rvalue reference none
                'R' => "volatile rvalue reference", //Rvalue reference volatile
                'T' => "std::nullptr_t", //
                'V' => "", // Empty type parameter pack
                'Z' => "", // End template parameter pack
                _ => DefaultGetSpecialModifiers(MangledString[CurrentPosition]),
            };
        }

        private string GetTwoTuple()
        {
            ChangePosition(1);
            var a = DecodeNumber();
            ChangePosition(2);
            var b = DecodeNumber();

            return $"{{ {a}, {b} }}";
        }

        private string GetThreeTuple()
        {
            ChangePosition(1);
            var a = DecodeNumber();
            ChangePosition(2);
            var b = DecodeNumber();
            ChangePosition(2);
            var c = DecodeNumber();

            return $"{{ {a}, {b}, {c} }}";
        }

        private string ResolveRunTimeTypeIdentification() // RTTI
        {
            Flags |= UnmangleFlags.UNDNAME_NO_FUNCTION_RETURNS;
            ChangePosition(1);
            return MangledString[CurrentPosition] switch
            {
                '0' => $"{UnmangleDataType()} `RTTI Type Descriptor'",
                '1' => GetBaseClassDescriptor(),
                '2' => "`RTTI Base Class Array'",
                '3' => "`RTTI Class Hierarchy Descriptor'",
                '4' => "`RTTI Complete Object Locator'",
                _ => DefaultRTII(MangledString[CurrentPosition]),
            };
        }

        private string GetSpecialNameUnderscore()
        {
            ChangePosition(1);
            return MangledString[CurrentPosition] switch
            {
                '0' => "operator/=",
                '1' => "operator%=",
                '2' => "operator>>=",
                '3' => "operator<<=",
                '4' => "operator&=",
                '5' => "operator|=",
                '6' => "operator^=",
                '7' => "`vftable'",
                '8' => "`vbtable'",
                '9' => "`vcall'",
                'A' => "`typeof'",
                'B' => "`local static guard'",
                'C' => "`string'", // TODO: implement String constants
                'D' => "`vbase destructor'",
                'E' => "`vector deleting destructor'",
                'F' => "`default constructor closure'",
                'G' => "`scalar deleting destructor'",
                'H' => "`vector constructor iterator'",
                'I' => "`vector destructor iterator'",
                'J' => "`vector vbase constructor iterator'",
                'K' => "`virtual displacement map'",
                'L' => "`eh vector constructor iterator'",
                'M' => "`eh vector destructor iterator'",
                'N' => "`eh vector vbase constructor iterator'",
                'O' => "`copy constructor closure'",
                'P' => "`udt returning'", // TODO: implement as prefix not postfix
                'Q' => "Unknown",
                'R' => ResolveRunTimeTypeIdentification(),
                'S' => "`local vftable'",
                'T' => "`local vftable constructor closure'",
                'U' => "operator new[]",
                'V' => "operator delete[]",
                'W' => "`omni callsig'|",
                'X' => "`placement delete closure'",
                'Y' => "`placement delete[] closure'",
                'Z' => WrongSymbolGetSpecialNameUnderscore(MangledString[CurrentPosition]), // should not happen
                '_' => GetSpecialNameSecondUnderscore(),
                _ => DefaultGetSpecialNameUnderscore(MangledString[CurrentPosition]),
            };
        }

        private string GetSpecialNameSecondUnderscore()
        {
            ChangePosition(1);
            return MangledString[CurrentPosition] switch
            {
                'A' => "`managed vector constructor iterator'",
                'B' => "`managed vector destructor iterator'",
                'C' => "`eh vector copy constructor iterator'",
                'D' => "`eh vector vbase copy constructor iterator'",
                'E' => "`dynamic initializer'", // Used by CRT entry point to construct non-trivial? global objects
                'F' => "`dynamic atexit destructor'", // Used by CRT to destroy non-trivial? global objects on program exit
                'G' => "`vector copy constructor iterator'",
                'H' => "`vector vbase copy constructor iterator'",
                'I' => "`managed vector copy constructor iterator'",
                'J' => "`local static thread guard'",
                'K' => "`user-defined literal operator'",
                _ => DefaultGetSpecialNameSecondUnderscore(MangledString[CurrentPosition]),
            };
        }

        private string GetIntEnum()
        {
            ChangePosition(1);
            var className = ""; // TODO: DemangleClassName()
            return Flags != UnmangleFlags.UNDNAME_NO_COMPLEX_TYPE ? className : $"enum {className}";
        }

        // Note that in modern versions of Visual Studio, it will usually (if not always) generate enum symbols with a type symbol of W4, regardless of the real underlying type.
        // Note that this doesn't affect the underlying type in any way, but appears to be for the sake of compiler simplicity.
        private string GetEnumTypeModifier()
        {
            return MangledString[CurrentPosition] switch
            {
                '0' => "", // Corresponding Real Type: char
                '1' => "", // Corresponding Real Type: unsigned char
                '2' => "", // Corresponding Real Type: short
                '3' => "", // Corresponding Real Type: unsigned short
                '4' => GetIntEnum(), // Corresponding Real Type: int (generally normal "enum")
                '5' => "", // Corresponding Real Type: unsigned int
                '6' => "", // Corresponding Real Type: long
                '7' => "", // Corresponding Real Type: unsigned long
                _ => DefaultGetEnumTypeModifier(MangledString[CurrentPosition]),
            };
        }

        private string GetComplexDataType()
        {
            return MangledString[CurrentPosition] switch
            {
                '?' => "__w64", // TODO: as prefix not postfix
                'D' => "__int8",
                'E' => "unsigned __int8",
                'F' => "__int16",
                'G' => "unsigned __int16",
                'H' => "__int32",
                'I' => "unsigned __int32",
                'J' => "__int64",
                'K' => "unsigned __int64",
                'L' => "__int128",
                'M' => "unsigned __int128",
                'N' => "bool",
                'O' => "Array",
                'S' => "char16_t",
                'U' => "char32_t",
                'W' => "wchar_t",
                'X' => GetComplexType(), // coclass
                'Y' => GetComplexType(), // cointerface
                _ => DefaultGetComplexDataType(MangledString[CurrentPosition]),
            };
        }

        private string GetComplexType()
        {
            var typeName = "";
            var className = ""; // TODO: DemangleClassName()

            if (Flags != UnmangleFlags.UNDNAME_NO_COMPLEX_TYPE)
            {
                typeName = MangledString[CurrentPosition] switch
                {
                    'T' => "union ",
                    'U' => "struct ",
                    'V' => "class ",
                    'X' => "coclass",
                    'Y' => "cointerface ",
                    _ => DefaultGetComplexType(MangledString[CurrentPosition]),
                };
            }

            return $"{typeName} {className}";
        }

        public string Unmangle()
        {
            var result = string.Empty;
            var nonStandardNames = new List<string>();
            var failures = new List<string>();

            if (MangledString[0] != '?') // all MSVC++ mangled functions should have this first char!
            {
                Console.WriteLine($"Wrong first symbol: ParsedSymbol: {MangledString[0]} \nMangledString: {MangledString}");

                nonStandardNames.Add(MangledString);
                return "NON_STANDARD_NAME";
            }

            if (MangledString[0] != '?' && MangledString[MangledString.Length] != 'Z') //Functions are always terminated by 'Z'.
            {
                Console.WriteLine($"Wrong first and last symbols: ParsedSymbol: {MangledString[0]}" +
                                  $" \nMangledString: {MangledString} \nLastSymbol: {MangledString[MangledString.Length]}");

                nonStandardNames.Add(MangledString);
                return "NON_STANDARD_NAME_Z_END";
            }

            ChangePosition(1); // to second symbol

            if (ParsedSymbol == char.Parse("?") && MangledString[CurrentPosition + 1] != char.Parse("$") ||
                MangledString[CurrentPosition + 2] == char.Parse("?"))
            {
                var methodName = new StringBuilder();
                var className = new StringBuilder();

                if (MangledString[CurrentPosition + 1] == char.Parse("$"))
                {
                    // TODO: get function arguments,
                    // CurrentPosition += 2;
                }

                ChangePosition(1); // to third symbol
                var specialName = GetSpecialName();
                Console.WriteLine($"specialName: {specialName}");

                // TODO: need complete methodName parsing, mere hack so far
                // traverse mangled string from curPos till char == @, symbolizes end of method name
                while (CurrentPosition < MangledString.Length)
                {
                    ChangePosition(1);

                    if (MangledString[CurrentPosition] == char.Parse("@"))
                    {
                        Console.WriteLine($"End of method name: {CurrentPosition}");
                        break;
                    }

                    methodName.Append(MangledString[CurrentPosition]);
                }

                Console.WriteLine($"methodName: {methodName}");

                // TODO: need complete className parsing, mere hack so far
                // traverse till end of className symbolized by @@
                while (CurrentPosition < MangledString.Length)
                {
                    ChangePosition(1);

                    if (MangledString[CurrentPosition] == char.Parse("@") &&
                        MangledString[CurrentPosition + 1] == char.Parse("@"))
                    {
                        Console.WriteLine($"End of class name: {CurrentPosition}");
                        break;
                    }

                    if (MangledString[CurrentPosition] == char.Parse("@"))
                    {
                        Console.WriteLine($"Skip @ from class name read at position: {CurrentPosition}");
                        continue;
                    }

                    className.Append(MangledString[CurrentPosition]);
                }

                Console.WriteLine($"className: {className}");

                var methodNameSTR = methodName.ToString();
                var classNameSTR = className.ToString();
                var classNameLower = className.ToString().ToLower();
                var classNameFirstUpper = classNameLower.FirstCharToUpper();

                //TODO: different output formats
                result = specialName.Equals("null")
                    ? string.Format("{0}.{1}::{2}::{2}()", classNameFirstUpper, classNameSTR, methodNameSTR)
                    : $"{classNameFirstUpper}.{classNameSTR}::{methodNameSTR}::{specialName}";

                Console.WriteLine($"Unmangled result: {result}");
            }
            else
            {
                failures.Add(MangledString);
            }

            //TODO: return data type and access level
            return result != string.Empty ? result : MangledString;

            //check "?h@@YAXH@Z", "void __cdecl h(int)"
            //check "?AFXSetTopLevelFrame@@YAXPAVCFrameWnd@@@Z", "void __cdecl AFXSetTopLevelFrame(class CFrameWnd *)"
            //check "??0_Lockit@std@@QAE@XZ", "public: __thiscall std::_Lockit::_Lockit(void)"
        }
    }
}
