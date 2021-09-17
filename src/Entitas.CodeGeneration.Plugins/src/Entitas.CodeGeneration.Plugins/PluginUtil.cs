using System;
using System.Collections.Generic;
using System.Text;
using DesperateDevs.Utils;

namespace Entitas.CodeGeneration.Plugins {

    public static class PluginUtil {

        public const string ASSEMBLY_RESOLVER_KEY = "Entitas.CodeGeneration.Plugins.AssemblyResolver";

        public static AssemblyResolver GetCachedAssemblyResolver(Dictionary<string, object> objectCache, string[] assemblies, string[] basePaths) {
            object cachedAssemblyResolver;
            if (!objectCache.TryGetValue(ASSEMBLY_RESOLVER_KEY, out cachedAssemblyResolver)) {
                cachedAssemblyResolver = new AssemblyResolver(false, basePaths);
                var resolver = (AssemblyResolver)cachedAssemblyResolver;
                foreach (var path in assemblies) {
                    resolver.Load(path);
                }
                objectCache.Add(ASSEMBLY_RESOLVER_KEY, cachedAssemblyResolver);
            }

            return (AssemblyResolver)cachedAssemblyResolver;
        }

        public static (string ns, string typeName) ExtractNamespace(this string fullTypeName)
        {
            var lastDot = fullTypeName.LastIndexOf(".", StringComparison.Ordinal);
            return lastDot != -1
                ? (fullTypeName.Substring(0, lastDot), fullTypeName.Substring(lastDot + 1))
                : (null, fullTypeName);
        }

        public static string WrapInNamespace(this string content, string ns)
        {
            if (!string.IsNullOrEmpty(ns))
            {
                var lines = content.Split('\n');
                var sb = new StringBuilder();
                foreach (var line in lines)
                {
                    if (line.Length > 0)
                        sb.Append(new string(' ', 4)).Append(line).Append('\n');
                    else
                        sb.Append(line).Append('\n');
                }

                return $"namespace {ns} {{\n{sb.ToString().TrimEnd()}\n}}\n";
            }
            else
            {
                return content;
            }
        }
    }
}
