using Microsoft.CodeAnalysis.Scripting;
using System;

namespace KdSoft.Services
{
    public interface IScriptManager
    {
        Script GetScript (string fileName, Type scriptGlobalsType, Func<ScriptOptions, ScriptOptions> updateOptions = null);
    }
}
