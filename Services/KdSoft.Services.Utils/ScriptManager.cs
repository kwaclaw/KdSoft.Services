using KdSoft.Utils;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Concurrent;
using System.Collections.Immutable;
using System.IO;
using System.Reflection;

namespace KdSoft.Services
{
    /// <summary>
    /// Manages a cache of compiled scripts that can be looked up by file name or relative file path.
    /// </summary>
    public class ScriptManager: IScriptManager
    {
        readonly ConcurrentDictionary<string, Script> scriptsMap;
        readonly ScriptOptions scriptOptions;

        /// <summary>
        /// Root directory where scripts are located.
        /// </summary>
        /// <remarks>Scripts located in sub-directories need  to be identified by a file path relative to the root directory.</remarks>
        public readonly string ScriptsDirectory;

        /// <param name="scriptsDirectory">Absolute path to scripts directory.</param>
        /// <param name="resolver">Source reference resolver.</param>
        public ScriptManager(string scriptsDirectory, SourceReferenceResolver resolver = null) {
            this.ScriptsDirectory = scriptsDirectory;

            // explicit using statements for these "imported" namespaces aren't needed
            var options = ScriptOptions.Default.AddImports(
                "System",
                "System.Collections.Generic",
                "System.Linq",
                "System.Text",
                "System.Threading.Tasks",
                "System.Diagnostics",
                "System.Dynamic"
            ).AddReferences(
                typeof(TimeSpanExtensions).GetTypeInfo().Assembly,
                typeof(ValueWrapper).GetTypeInfo().Assembly,
                // for running scripts from a script
                typeof(MetadataReference).GetTypeInfo().Assembly,
                typeof(Script).GetTypeInfo().Assembly,
                typeof(CSharpScript).GetTypeInfo().Assembly,
                typeof(IScriptManager).GetTypeInfo().Assembly
            ).WithSourceResolver(
                resolver ?? new SourceFileResolver(ImmutableArray<string>.Empty, scriptsDirectory)
            );
            this.scriptOptions = options;

            scriptsMap = new ConcurrentDictionary<string, Script>();
        }

        Script CreateScript(string fileName, Type scriptGlobalsType, Func<ScriptOptions, ScriptOptions> updateOptions) {
            var scriptFile = Path.Combine(ScriptsDirectory, fileName);
            string scriptCode = File.ReadAllText(scriptFile);
            var newOptions = updateOptions?.Invoke(scriptOptions) ?? scriptOptions;
            return CSharpScript.Create(scriptCode, options: newOptions, globalsType: scriptGlobalsType);
        }

        /// <summary>
        /// Gets the cached script instance by file name. Recreates script if file was modified.
        /// Modification detection is based on Archive flag being set.
        /// </summary>
        /// <param name="fileName">Relative path of file in Scripts directory.</param>
        /// <param name="scriptGlobalsType">Type of global script parameter object.
        /// Used only when script is first created.</param>
        /// <param name="updateOptions">Function delegate that returns a modified <see cref="ScriptOptions"/> instance.
        /// Used only when script is first created.</param>
        /// <returns>Script instance.</returns>
        /// <remarks>Using the <c>updateOptions</c> argument applies script options *before* the script is created.
        /// When options are changed on an existing script instance, a new script is compiled/created and this
        /// can lead to out-of-memory issues, as script instances are currently not garbage-collectible.</remarks>
        public Script GetScript(string fileName, Type scriptGlobalsType, Func<ScriptOptions, ScriptOptions> updateOptions = null) {
            Script result;
            var scriptFile = Path.Combine(ScriptsDirectory, fileName);

            Func<string, Script> createScript = (string fn) => CreateScript(fn, scriptGlobalsType, updateOptions);
            Func<string, Script, Script> updateScript = (string fn, Script oldScript) => CreateScript(fn, scriptGlobalsType, updateOptions);

            // if file has been modified (means if Archive bit is set) we add or replace the script
            if ((File.GetAttributes(scriptFile) & FileAttributes.Archive) == FileAttributes.Archive) {
                result = scriptsMap.AddOrUpdate(fileName, createScript, updateScript);
                // now that the script is loaded we must clear the Archive flag
                File.SetAttributes(scriptFile, File.GetAttributes(scriptFile) & ~FileAttributes.Archive);
            }
            // if file is not modified, we retrieve the script, or create it if not yet created
            else {
                result = scriptsMap.GetOrAdd(fileName, createScript);
            }

            return result;
        }
    }
}
