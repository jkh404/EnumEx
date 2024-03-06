using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp;

namespace EnumEx
{
    [Generator]
    internal sealed class MyEnumCodeGenerator : ISourceGenerator
    {
        static string bastTypeText = "EnumClass";
        private List<MyEnumCodeInfo> GetMyEnumCodeInfos(ClassDeclarationSyntax _member, string namespaceText = null)
        {
            List<MyEnumCodeInfo> codeInfos = new List<MyEnumCodeInfo>();
            if (_member.BaseList!=null)
            {
                foreach (var baseType in _member.BaseList.Types)
                {

                    var type = baseType.Type as GenericNameSyntax;
                    if (type != null && type.Identifier.Text==bastTypeText)
                    {
                        //_member.Identifier.Text
                        var argType = type.TypeArgumentList.Arguments.FirstOrDefault(x => x is PredefinedTypeSyntax) as PredefinedTypeSyntax;
                        var accessText = "internal";
                        if (_member.Modifiers.Count>0) accessText=_member.Modifiers[0].Text;
                        codeInfos.Add(new MyEnumCodeInfo
                        {
                            HasNamespace = !string.IsNullOrWhiteSpace(namespaceText),
                            NamespaceText=namespaceText,
                            AccessText=accessText,
                            TypeName = _member.Identifier.Text,
                            InnerValType=argType.Keyword.Text,
                            FileName=$"{_member.Identifier.Text}.g.cs"
                        });

                    }
                }
            }

            return codeInfos;
        }
        public void Execute(GeneratorExecutionContext context)
        {

            var namespaceText = context.Compilation.AssemblyName;
            //var namespaceText = "ClassLibrary1";
            var tress = context.Compilation.SyntaxTrees.ToArray();
            List<MyEnumCodeInfo> codeInfos = new List<MyEnumCodeInfo>();
            foreach (var item in tress)
            {
                var root = item.GetCompilationUnitRoot();

                foreach (var member in root.Members)
                {

                    if (member is ClassDeclarationSyntax)
                    {
                        var _member = member as ClassDeclarationSyntax;
                        codeInfos.AddRange(GetMyEnumCodeInfos(_member));
                    }
                    else if (member is FileScopedNamespaceDeclarationSyntax)
                    {
                        var _Namespace = member as FileScopedNamespaceDeclarationSyntax;
                        foreach (var innermember in _Namespace.Members)
                        {

                            if (innermember is ClassDeclarationSyntax)
                            {
                                var _member = innermember as ClassDeclarationSyntax;

                                codeInfos.AddRange(GetMyEnumCodeInfos(_member, namespaceText));
                            }
                        }
                    }
                    else if (member is NamespaceDeclarationSyntax)
                    {

                        var _Namespace = member as NamespaceDeclarationSyntax;
                        foreach (var innermember in _Namespace.Members)
                        {

                            if (innermember is ClassDeclarationSyntax)
                            {
                                var _member = innermember as ClassDeclarationSyntax;

                                codeInfos.AddRange(GetMyEnumCodeInfos(_member, namespaceText));
                            }
                        }

                    }

                }

            }
            foreach (var item in codeInfos)
            {
                var sb = new StringBuilder();
                string classCode = $@"
{item.AccessText} sealed partial class {item.TypeName}:{bastTypeText}<{item.InnerValType}>, IEquatable<{item.TypeName}>
    {{
        private {item.TypeName}({item.InnerValType} _val) : base(_val) {{ }}
        public static implicit operator {item.TypeName}({item.InnerValType} n) => new {item.TypeName}(n);
        public static bool operator ==({item.TypeName}? obj1, {item.TypeName}? obj2) => obj1?.val == obj2?.val;
        public static bool operator !=({item.TypeName}? obj1, {item.TypeName}? obj2) => obj1?.val != obj2?.val;
        public override bool Equals(object? obj)=> Equals(obj as {item.TypeName});
        public override int GetHashCode()=> val.GetHashCode();
        public bool Equals({item.TypeName}? other)=> val==other?.val;
        public string? GetDescription()
        {{
            var fields = typeof({item.TypeName}).GetFields(BindingFlags.Static|BindingFlags.Public);
            return fields?.FirstOrDefault(x => ({item.TypeName}?)x?.GetValue(null)==val)
                ?.GetCustomAttribute<DescriptionAttribute>()
                ?.Description;
        }}
    }}";
                sb.Append($@"
using System;
using System.ComponentModel;
using System.Reflection;
using EnumEx;
{(item.HasNamespace ? $@"namespace {item.NamespaceText}
{{
    {classCode}
}}" : classCode)}
");
                var code = sb.ToString();
                context.AddSource(item.FileName, code);
            }

        }
        public void Initialize(GeneratorInitializationContext context)
        {
            //System.Diagnostics.Debugger.Launch();
        }
    }
}
