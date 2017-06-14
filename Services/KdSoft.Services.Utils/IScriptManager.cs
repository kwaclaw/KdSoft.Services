using Microsoft.CodeAnalysis.Scripting;
using System;

namespace KdSoft.Services
{
    /// <summary>
    /// Script manager interface.
    /// </summary>
    public interface IScriptManager
    {
        /// <summary>
        /// Returns existing script based on file name or creates new script instance based on specified arguments.
        /// </summary>
        /// <param name="fileName">Path to script file. Interpretation of relative paths depends on implementation.</param>
        /// <param name="scriptGlobalsType">Type of global script object.</param>
        /// <param name="updateOptions">Callback that modifies the <see cref="ScriptOptions"/>.</param>
        /// <returns>Script instance.</returns>
        Script GetScript (string fileName, Type scriptGlobalsType, Func<ScriptOptions, ScriptOptions> updateOptions = null);
    }
}
