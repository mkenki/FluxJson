// 2. FluxJsonGeneratedAttribute.cs - Source Generator için marker attribute
using System;

namespace FluxJson.Core
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    internal class FluxJsonGeneratedAttribute : Attribute
    {
    }
}
