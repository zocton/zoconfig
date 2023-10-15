# zoconfig
A C# JSON alternative with simple syntax and built-in type casting. Supporting both synchronous and asynchronous operations for optimal implementation depending on file size and developer needs.

## Installation
All you need to do is inlucde zocparser.cs in your project!

Then the typical using statement:
```using zoconfig;```

## Usage
### Instantiation and data retrieval
Two simple lines of code will get you all the data you need.
```
Zocparser parser = new();

// Snychronous
parser.GetData(PATH_TO_YOUR_FILE);

// -- or --

// Asynchronous
await parser.GetDataAsync(PATH_TO_YOUR_FILE);
```
>[!NOTE]
>Each subsequent call to GetData or GetDataAsync will overwrite all data from the previous call.

### Accessing data during runtime
We use nullable datatypes to be more inline with modern C# libararies. Similarly to JSON and other key value pair configuration languages, we overloaded the subscript operators for our datatypes.
```
var orc = parser?["orc"];
var elf = parser?["elf];
var gnome = parser?["gnome"];
```

Objects are mapped as a [KeyValuePair<string, object>](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.keyvaluepair-2?view=net-7.0) so when you use the subscript operator to access data its return type is [object](https://learn.microsoft.com/en-us/dotnet/api/system.object?view=net-7.0).
```
var orcStrength = orc?["strength"]; // returns strength as an object so orcStrength is an object
```

There is generic typcasting for all [Built-in types](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/built-in-types) with the exception of [dynamic](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/reference-types#the-dynamic-type) for obvious reasons.
```
var orcStrength = orc?["strength"].As<int>(); // returns strength as an int so orcStrength is now an int rather than an object
```
>[!NOTE]
>It is recommended to use orc?["strength"].ToString() rather than orc?["strength"].As<string>() if you need or want to convert the type from object to string. This will be faster since it will not need to check if it is convertible to all the other built-in types C# offers. Realistically, this will barely touch perfomance but it's worth noting.

### Exceptions
We throw a _ZoconfigException_ for parsing failures. Since our datatypes are nullable we did not feel the need to implement the bloat and overhead of exception handling on subscript operator usage.
```
Zocparser parser = new();

try
{
  parser.GetData(file.zc);
}
catch(ZoconfigException ze)
{
  Console.Write(ze.Message);
}
``` 

## File Syntax
### Comments 
Defined using the pound sign.
```
# This is a zoconfig comment
```

### Objects and data
Square brackets are used to define objects and contain data.
```
[MyObjectTitle]
[MyObjectData]
[true]
[10]
[Hello World!]
```

### Scope
Curly brackets are used to define object scope.
```
[MyObject]
{
  # Some data goes here and will be encapsulated in MyObject.
}
```

### Binding
Colon is used to bind data to a member.
```
[MyObjectData] : [MyObjectData now contains this string!]
```

### Example File
[template.zc](/zoconfig/data/template.zc)
```
# orcs are strong but slow and dumb
[orc]
{
  [name] : [Thrall]
  [strength] : [10]
  [speed] : [5]
  [intelligence] : [3]
}

# elves are well-rounded and smart
[elf]
{
  [name] : [Legolas]
  [strength] : [7]
  [speed] : [7]
  [intelligence] : [10]
}

# gnomes are small and weak but rather quick
# they also seem to know their way around a book
[gnome]
{
  [name] : [Peter D.]
  [strength] : [5]
  [speed] : [10]
  [intelligence] : [8]
}
```
>[!WARNING]
>Inline comments work only on data binding lines. If you inline a comment on an object title or on scope declaration/closure the file will fail to parse.
>```
>[MyObject] # This is NOT a valid inline comment
>{ # This is also invalid
>  [MyData] : [65535] # This IS a valid inline comment
>} # This is still invalid so don't even think about it...
>```

