
# AOT.Reflection is faster reflection powered via Source Generators

The goal of this library is to create a subset of reflection that will be faster than the default one and will not break at the platforms with the AOT compilation support. The source generators will help us with that.


# How to use

To make it work you will need to install a NuGet package ``` Apparatus.AOT.Reflection ```:

```
dotnet add package Apparatus.AOT.Reflection
```

Then you can use it like that:

``` cs
public class User
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
}

// ...

public static void Main()
{
    var user = new User();
    var properties = user.GetProperties().Values;
    foreach (var property in properties)
    {
        Console.WriteLine(property.Name);
    }
}
```

This sample will print the names of properties.
```
FirstName
LastName
```

Also it works for enums too:
``` cs 

public enum UserKind 
{
    User,
    Admin
}

// ...

public static void Main()
{
    var values = EnumHelper.GetEnumInfo<UserKind>();
    foreach (var value in values)
    {
        Console.WriteLine(value.Name);
    }
}

```

You will see:
```
User
Admin
```


# Performance

Let's imagine that we need to find a property with ``` Required ``` attribute and with the name  ``` FirstName ```.
If it exists then print the value of the property, otherwise return the empty string.
Here the source code with default reflection:

``` cs
var type = _user.GetType();
var property = type.GetProperty(nameof(User.FirstName));

var required = false;
foreach (var o in property.GetCustomAttributes())
{
    if (o.GetType() == typeof(RequiredAttribute))
    {
        required = true;
        break;
    }
}

if (required)
{
    return (string)property.GetMethod?.Invoke(_user, null);
}

return string.Empty;

```

Here the source code with aot reflection:
``` cs 
var entries = _user.GetProperties();
var firstName = entries[nameof(User.FirstName)];

var required = false;
foreach (var o in firstName.Attributes)
{
    if (o.Type == typeof(RequiredAttribute))
    {
        required = true;
        break;
    }
}

if (required)
{
    if (firstName.TryGetValue(_user, out var value))
    {
        return (string)value;
    }

    return string.Empty;
}

return string.Empty;
```

Here are the benchmark results:
``` 
BenchmarkDotNet=v0.13.1, OS=macOS Big Sur 11.5.2 (20G95) [Darwin 20.6.0]
Apple M1, 1 CPU, 8 logical and 8 physical cores
.NET SDK=6.0.100-preview.6.21355.2
  [Host]     : .NET 6.0.0 (6.0.21.35212), Arm64 RyuJIT
  DefaultJob : .NET 6.0.0 (6.0.21.35212), Arm64 RyuJIT


|        Method |        Mean |     Error |    StdDev |  Gen 0 | Allocated |
|-------------- |------------:|----------:|----------:|-------:|----------:|
|    Reflection | 2,246.93 ns | 12.526 ns | 10.460 ns | 0.4959 |   1,040 B |
| AOTReflection |    18.61 ns |  0.074 ns |  0.069 ns |      - |         - |

```

As you can see the AOT.Reflection is significantly faster comparing to default reflection.
The full source code of benchmarks you can find [here](https://github.com/byme8/Apparatus.AOT.Reflection/blob/master/src/Apparatus.AOT.Reflection.Benchmark/Program.cs). 

# Support

Right now, only public properties are supported.
I have plans to add as support for enums and public methods in the future.
Regarding the private members, I have doubts about them, because they would definitely ruin the performance, but we will see.

To be continued...
