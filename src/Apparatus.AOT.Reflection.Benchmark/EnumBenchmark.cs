using System;
using System.ComponentModel;
using BenchmarkDotNet.Attributes;

namespace Apparatus.AOT.Reflection.Benchmark;


public enum AccountKind
{
    [Description("User account")]
    User,
    [Description("Admin account")]
    Admin,
    [Description("Customer account")]
    Customer,
    [Description("Manager account")]
    Manager
}
    
public static class EnumHelperReflection
{
    public static string GetDescription<T>(this T enumValue) 
        where T : Enum
    {
        var description = enumValue.ToString();
        var fieldInfo = enumValue.GetType().GetField(description);

        if (fieldInfo != null)
        {
            var attrs = fieldInfo.GetCustomAttributes(typeof(DescriptionAttribute), true);
            if (attrs is { Length: > 0 })
            {
                description = ((DescriptionAttribute)attrs[0]).Description;
            }
        }

        return description;
    }
}


[MemoryDiagnoser]
public class EnumBenchmark
{
    private readonly AccountKind _account;

    public EnumBenchmark()
    {
        _account = AccountKind.Customer;
    }
        
    [Benchmark]
    public string GetValuesAOT()
    {
        var attributes = _account.GetEnumValueInfo().Attributes;
        for (int i = 0; i < attributes.Length; i++)
        {
            var attribute = attributes[i];
            if (attribute is DescriptionAttribute descriptionAttribute)
            {
                return descriptionAttribute.Description;
            }
        }
            
        return "";
    }
        
    [Benchmark]
    public string GetValuesReflection()
    {
        return _account.GetDescription();
    }
}