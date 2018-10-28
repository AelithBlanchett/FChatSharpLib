using System;
using System.Reflection;

namespace FChatSharpLib
{
    class TypeProxy : MarshalByRefObject
    {
        public Assembly GetAssembly(string assemblyPath)
        {
            try
            {
                var assembly = Assembly.LoadFile(assemblyPath);
                return assembly;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}