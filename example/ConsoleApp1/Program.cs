// See https://aka.ms/new-console-template for more information
using EnumEx;
using System.ComponentModel;

Console.WriteLine("Hello, World!");

TestFunc('A');
void TestFunc(MyCode code)
{
    Console.WriteLine(code==MyCode.Fail);
    Console.WriteLine(code=='B');
    Console.WriteLine(code.GetDescription());
}
internal sealed partial class MyCode : EnumClass<char>
{
    [Description("错误")]
    public static readonly MyCode Error = 'A';

    [Description("操作失败")]
    public static readonly MyCode Fail = 'B';

    [Description("操作成功")]
    public static readonly MyCode Success = 'C';
}