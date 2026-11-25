

using System.Reflection;
using System.Runtime.InteropServices;

class Program
{
    static int Main(string[] args)
    {
        // args[0] is a path to a .net assembly
        if (args.Length != 1)
        {
            return 1;
        }

        // Load assembly
        // TODO:  Does this run any code?  Static initializers?
        Console.WriteLine($"Validating imports for assembly {args[0]}");
        System.Reflection.Assembly a = Assembly.LoadFrom(args[0]);
        // Iterate over all types
        System.Type[] types = a.GetTypes();
        Dictionary<string,List<string>> importedMethods = new Dictionary<string,List<string>>();
        foreach(Type currType in types)
        {
            // Iterate over all imports
            MethodInfo[] importMethods = currType.GetMethods(BindingFlags.Public|BindingFlags.NonPublic|BindingFlags.Static);
            foreach (MethodInfo currMethod in importMethods)
            {
                // Print anything that has a LibraryImportAttribute
                LibraryImportAttribute? methAttrib = currMethod.GetCustomAttribute<LibraryImportAttribute>();
                if (methAttrib == null)
                {
                    continue;
                }
                string? ep = methAttrib.EntryPoint;
                //Console.WriteLine($"Imported method:  {currMethod.Name} => {ep}\n\t{methAttrib.LibraryName}");
                if (!importedMethods.ContainsKey(methAttrib.LibraryName))
                {
                    importedMethods.Add(methAttrib.LibraryName, new List<string>());
                }
                importedMethods[methAttrib.LibraryName].Add(ep!=null ? ep : currMethod.Name);

            }
        }
        Console.WriteLine("-------------------------------------------------------------------------------");

        foreach(var kvp in importedMethods)
        {
            Console.WriteLine($"{kvp.Key}:\n\t{string.Join("\n\t", kvp.Value)}");
        }
        Console.WriteLine("-------------------------------------------------------------------------------");

        //
        // Inspect native libraries for the imported methods
        //
        foreach (string lib in importedMethods.Keys)
        {
            nint h = NativeLibrary.Load(lib); // Handle is valid; throws on failure
            foreach (string method in importedMethods[lib])
            {
                nint addr = 0;
                bool importExists = NativeLibrary.TryGetExport(h, method, out addr);
                if (!importExists)
                {
                    NativeLibrary.Free(h);
                    Console.WriteLine($"Couldn't find {method} in {lib}");
                    return 1;
                }
            }
            NativeLibrary.Free(h);
        }

        return 0;
    }
}





