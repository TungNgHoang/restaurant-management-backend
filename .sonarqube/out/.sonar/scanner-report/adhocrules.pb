A
roslynASP0015*Suggest using IHeaderDictionary properties(0æ
roslynCA1822Mark members as static"èMembers that do not access instance data or call instance methods can be marked as static. After you mark the methods as static, the compiler will emit nonvirtual call sites to these members. This can give you a measurable performance gain for performance-sensitive code.(0ª
roslynCA1829;Use Length/Count property instead of Count() when available"hEnumerable.Count() potentially enumerates the sequence while a Length/Count property is a direct access.(0œ
roslynCA1835DPrefer the 'Memory'-based overloads for 'ReadAsync' and 'WriteAsync'"Ú'Stream' has a 'ReadAsync' overload that takes a 'Memory<Byte>' as the first argument, and a 'WriteAsync' overload that takes a 'ReadOnlyMemory<Byte>' as the first argument. Prefer calling the memory based overloads, which are more efficient.(0õ
roslynCA1860/Avoid using 'Enumerable.Any()' extension method"”Prefer using 'IsEmpty', 'Count' or 'Length' properties whichever available, rather than calling 'Enumerable.Any()'. The intent is clearer and it is more performant than using 'Enumerable.Any()' extension method.(0ƒ
roslynCA2208)Instantiate argument exceptions correctly"ÇA call is made to the default (parameterless) constructor of an exception type that is or derives from ArgumentException, or an incorrect string argument is passed to a parameterized constructor of an exception type that is or derives from ArgumentException.(0y
roslynCA2254&Template should be a static expression";The logging message template should not vary between calls.(0Z
roslynCS8600DConverting null literal or possible null value to non-nullable type.(09
roslynCS8601#Possible null reference assignment.(0?
roslynCS8602)Dereference of a possibly null reference.(07
roslynCS8604!Possible null reference argument.(06
roslynCS8629 Nullable value type may be null.(0≠
roslynCA1050Declare types in namespaces"zTypes are declared in namespaces to prevent name collisions and as a way to organize related types in an object hierarchy.(0K
roslynCS01055Using directive appeared previously in this namespace(0)
roslynCS0169Field is never used(0Z
roslynCS8600DConverting null literal or possible null value to non-nullable type.(0£
roslynCS8618åNon-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.(0u
roslynCS8620_Argument cannot be used for parameter due to differences in the nullability of reference types.(0Q
roslynCS8625;Cannot convert null literal to non-nullable reference type.(0≠
roslynCA1050Declare types in namespaces"zTypes are declared in namespaces to prevent name collisions and as a way to organize related types in an object hierarchy.(0K
roslynCS01055Using directive appeared previously in this namespace(0)
roslynCS0169Field is never used(0Z
roslynCS8600DConverting null literal or possible null value to non-nullable type.(0£
roslynCS8618åNon-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.(0u
roslynCS8620_Argument cannot be used for parameter due to differences in the nullability of reference types.(0Q
roslynCS8625;Cannot convert null literal to non-nullable reference type.(0¢
roslynCA1510&Use ArgumentNullException throw helper"dThrow helpers are simpler and more efficient than an if block constructing a new exception instance.(0æ
roslynCA1822Mark members as static"èMembers that do not access instance data or call instance methods can be marked as static. After you mark the methods as static, the compiler will emit nonvirtual call sites to these members. This can give you a measurable performance gain for performance-sensitive code.(0õ
roslynCA1860/Avoid using 'Enumerable.Any()' extension method"”Prefer using 'IsEmpty', 'Count' or 'Length' properties whichever available, rather than calling 'Enumerable.Any()'. The intent is clearer and it is more performant than using 'Enumerable.Any()' extension method.(0æ
roslynCA1861"Avoid constant arrays as arguments"ÉConstant arrays passed as arguments are not reused when called repeatedly, which implies a new array is created each time. Consider extracting them to 'static readonly' fields to improve performance if the passed array is not mutated within the called method.(0Í
roslynCA1862ZUse the 'StringComparison' method overloads to perform case-insensitive string comparisons"˜Avoid calling 'ToLower', 'ToUpper', 'ToLowerInvariant' and 'ToUpperInvariant' to perform case-insensitive string comparisons because they lead to an allocation. Instead, prefer calling the method overloads of 'Contains', 'IndexOf' and 'StartsWith' that take a 'StringComparison' enum value to perform case-insensitive comparisons. Switching to using an overload that takes a 'StringComparison' might cause subtle changes in behavior, so it's important to conduct thorough testing after applying the suggestion. Additionally, if a culturally sensitive comparison is not required, consider using 'StringComparison.OrdinalIgnoreCase'.(0œ
roslynCA1862ZUse the 'StringComparison' method overloads to perform case-insensitive string comparisons"‹Avoid calling 'ToLower', 'ToUpper', 'ToLowerInvariant' and 'ToUpperInvariant' to perform case-insensitive string comparisons, as in 'string.ToLower() == string.ToLower()', because they lead to an allocation. Instead, use 'string.Equals(string, StringComparison)' to perform case-insensitive comparisons. Switching to using an overload that takes a 'StringComparison' might cause subtle changes in behavior, so it's important to conduct thorough testing after applying the suggestion. Additionally, if a culturally sensitive comparison is not required, consider using 'StringComparison.OrdinalIgnoreCase'.(0ƒ
roslynCA2208)Instantiate argument exceptions correctly"ÇA call is made to the default (parameterless) constructor of an exception type that is or derives from ArgumentException, or an incorrect string argument is passed to a parameterized constructor of an exception type that is or derives from ArgumentException.(0±
roslynCS01082Member hides inherited member; missing new keyword"ÊA variable was declared with the same name as a variable in a base type. However, the new keyword was not used. This warning informs you that you should use new; the variable is declared as if new had been used in the declaration.(09
roslynCS0168#Variable is declared but never used(0U
roslynCS1998?Async method lacks 'await' operators and will run synchronously(0Z
roslynCS8600DConverting null literal or possible null value to non-nullable type.(09
roslynCS8601#Possible null reference assignment.(0?
roslynCS8602)Dereference of a possibly null reference.(05
roslynCS8603Possible null reference return.(07
roslynCS8604!Possible null reference argument.(0£
roslynCS8618åNon-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.(06
roslynCS8629 Nullable value type may be null.(0Ñ
roslynCS8765nNullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).(0?
roslyn
SYSLIB1045%Convert to 'GeneratedRegexAttribute'.(0¢
roslynCA1510&Use ArgumentNullException throw helper"dThrow helpers are simpler and more efficient than an if block constructing a new exception instance.(0æ
roslynCA1822Mark members as static"èMembers that do not access instance data or call instance methods can be marked as static. After you mark the methods as static, the compiler will emit nonvirtual call sites to these members. This can give you a measurable performance gain for performance-sensitive code.(0õ
roslynCA1860/Avoid using 'Enumerable.Any()' extension method"”Prefer using 'IsEmpty', 'Count' or 'Length' properties whichever available, rather than calling 'Enumerable.Any()'. The intent is clearer and it is more performant than using 'Enumerable.Any()' extension method.(0æ
roslynCA1861"Avoid constant arrays as arguments"ÉConstant arrays passed as arguments are not reused when called repeatedly, which implies a new array is created each time. Consider extracting them to 'static readonly' fields to improve performance if the passed array is not mutated within the called method.(0Í
roslynCA1862ZUse the 'StringComparison' method overloads to perform case-insensitive string comparisons"˜Avoid calling 'ToLower', 'ToUpper', 'ToLowerInvariant' and 'ToUpperInvariant' to perform case-insensitive string comparisons because they lead to an allocation. Instead, prefer calling the method overloads of 'Contains', 'IndexOf' and 'StartsWith' that take a 'StringComparison' enum value to perform case-insensitive comparisons. Switching to using an overload that takes a 'StringComparison' might cause subtle changes in behavior, so it's important to conduct thorough testing after applying the suggestion. Additionally, if a culturally sensitive comparison is not required, consider using 'StringComparison.OrdinalIgnoreCase'.(0œ
roslynCA1862ZUse the 'StringComparison' method overloads to perform case-insensitive string comparisons"‹Avoid calling 'ToLower', 'ToUpper', 'ToLowerInvariant' and 'ToUpperInvariant' to perform case-insensitive string comparisons, as in 'string.ToLower() == string.ToLower()', because they lead to an allocation. Instead, use 'string.Equals(string, StringComparison)' to perform case-insensitive comparisons. Switching to using an overload that takes a 'StringComparison' might cause subtle changes in behavior, so it's important to conduct thorough testing after applying the suggestion. Additionally, if a culturally sensitive comparison is not required, consider using 'StringComparison.OrdinalIgnoreCase'.(0ƒ
roslynCA2208)Instantiate argument exceptions correctly"ÇA call is made to the default (parameterless) constructor of an exception type that is or derives from ArgumentException, or an incorrect string argument is passed to a parameterized constructor of an exception type that is or derives from ArgumentException.(0±
roslynCS01082Member hides inherited member; missing new keyword"ÊA variable was declared with the same name as a variable in a base type. However, the new keyword was not used. This warning informs you that you should use new; the variable is declared as if new had been used in the declaration.(09
roslynCS0168#Variable is declared but never used(0U
roslynCS1998?Async method lacks 'await' operators and will run synchronously(0Z
roslynCS8600DConverting null literal or possible null value to non-nullable type.(09
roslynCS8601#Possible null reference assignment.(0?
roslynCS8602)Dereference of a possibly null reference.(05
roslynCS8603Possible null reference return.(07
roslynCS8604!Possible null reference argument.(0£
roslynCS8618åNon-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.(06
roslynCS8629 Nullable value type may be null.(0Ñ
roslynCS8765nNullability of type of parameter doesn't match overridden member (possibly because of nullability attributes).(0?
roslyn
SYSLIB1045%Convert to 'GeneratedRegexAttribute'.(0ª
roslynCA1829;Use Length/Count property instead of Count() when available"hEnumerable.Count() potentially enumerates the sequence while a Length/Count property is a direct access.(0M
roslynCS01147Member hides inherited member; missing override keyword(0?
roslynCS8602)Dereference of a possibly null reference.(07
roslynCS8604!Possible null reference argument.(0£
roslynCS8618åNon-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.(0ª
roslynCA1829;Use Length/Count property instead of Count() when available"hEnumerable.Count() potentially enumerates the sequence while a Length/Count property is a direct access.(0M
roslynCS01147Member hides inherited member; missing override keyword(0?
roslynCS8602)Dereference of a possibly null reference.(07
roslynCS8604!Possible null reference argument.(0£
roslynCS8618åNon-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.(0æ
roslynCA1822Mark members as static"èMembers that do not access instance data or call instance methods can be marked as static. After you mark the methods as static, the compiler will emit nonvirtual call sites to these members. This can give you a measurable performance gain for performance-sensitive code.(0õ
roslynCA1860/Avoid using 'Enumerable.Any()' extension method"”Prefer using 'IsEmpty', 'Count' or 'Length' properties whichever available, rather than calling 'Enumerable.Any()'. The intent is clearer and it is more performant than using 'Enumerable.Any()' extension method.(0(
roslynCS1030#warning directive(05
roslynCS8603Possible null reference return.(0£
roslynCS8618åNon-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.(0Q
roslynCS8625;Cannot convert null literal to non-nullable reference type.(06
roslynCS8629 Nullable value type may be null.(0æ
roslynCA1822Mark members as static"èMembers that do not access instance data or call instance methods can be marked as static. After you mark the methods as static, the compiler will emit nonvirtual call sites to these members. This can give you a measurable performance gain for performance-sensitive code.(0õ
roslynCA1860/Avoid using 'Enumerable.Any()' extension method"”Prefer using 'IsEmpty', 'Count' or 'Length' properties whichever available, rather than calling 'Enumerable.Any()'. The intent is clearer and it is more performant than using 'Enumerable.Any()' extension method.(0(
roslynCS1030#warning directive(05
roslynCS8603Possible null reference return.(0£
roslynCS8618åNon-nullable field must contain a non-null value when exiting constructor. Consider adding the 'required' modifier or declaring as nullable.(0Q
roslynCS8625;Cannot convert null literal to non-nullable reference type.(06
roslynCS8629 Nullable value type may be null.(0