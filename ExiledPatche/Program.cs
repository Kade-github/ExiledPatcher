using System;
using System.IO;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using dnlib.DotNet.Writer;

namespace ExiledPatche
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                if (args.Length != 1)
                {
                    Console.WriteLine("Missing file location argument!");
                    return;
                }

                var Module = ModuleDefMD.Load(args[0]);

                if (Module == null)
                {
                    Console.WriteLine("File not found!");
                    return;
                }

                Module.IsILOnly = true;
                Module.VTableFixups = null;
                Module.IsStrongNameSigned = false;
                Module.Assembly.PublicKey = null;
                Module.Assembly.HasPublicKey = false;

                var opts = new ModuleWriterOptions(Module);

                Console.WriteLine("[EXILED] Loaded " + Module.Name);

                Console.WriteLine("[EXILED-ASSEMBLY] Resolving References...");

                ModuleContext modCtx = ModuleDef.CreateModuleContext();
                // It creates the default assembly resolver
                AssemblyResolver asmResolver = (AssemblyResolver) modCtx.AssemblyResolver;



                Module.Context = modCtx;

                ((AssemblyResolver) Module.Context.AssemblyResolver).AddToCache(Module);

                Console.WriteLine("[INJECTION] Injecting the ModLoader Class.");

                var ModLoader = ModuleDefMD.Load("ModLoader.dll");

                Console.WriteLine("[INJECTION] Loaded " + ModLoader.Name);

                var ModClass = ModLoader.Types[0];

                foreach (var type in ModLoader.Types)
                {
                    if (type.Name == "ModLoader")
                    {
                        ModClass = type;
                        Console.WriteLine("[INJECTION] Hooked to: " + type.Namespace + "." + type.Name);
                    }
                }

                var modRefType = ModClass;


                ModLoader.Types.Remove(ModClass);

                modRefType.DeclaringType = null;

                Module.Types.Add(modRefType);

                MethodDef call = findMethod(modRefType, "LoadBoi");

                if (call == null)
                {
                    Console.WriteLine("Failed to get the 'LoadBoi' method! Maybe we don't have permission?");
                    return;
                }

                Console.WriteLine("[INJECTION] Injected!");

                Console.WriteLine("[EXILED] Completed injection!");

                Console.WriteLine("[EXILED] Patching code...");

                TypeDef def = findType(Module.Assembly, "ServerConsoleSender");

                MethodDef bctor = new MethodDefUser(".ctor", MethodSig.CreateInstance(Module.CorLibTypes.Void),
                    MethodImplAttributes.IL | MethodImplAttributes.Managed,
                    MethodAttributes.Public |
                    MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

                if (findMethod(def, ".ctor") != null)
                {
                    bctor = findMethod(def, ".ctor");
                    Console.WriteLine("[EXILED] Re-using constructor.");
                }
                else
                    def.Methods.Add(bctor);

                CilBody body;
                bctor.Body = body = new CilBody();
                
                body.Instructions.Add(OpCodes.Call.ToInstruction(call));
                body.Instructions.Add(OpCodes.Ret.ToInstruction());

                Module.Write("Assembly-CSharp-EXILED.dll");
                Console.WriteLine("[EXILED] COMPLETE!");
            }
            catch(Exception e)
            {
                Console.WriteLine(e.ToString());
            }

            Console.Read();
        }

        private static MethodDef findMethod(TypeDef type, string methodName)
        {
            if (type != null)
            {
                foreach (var method in type.Methods)
                {
                    if (method.Name == methodName)
                        return method;
                }
            }
            return null;
        }

        private static MethodDef findMethod(AssemblyDef asm, string classPath, string methodName)
        {
            return findMethod(findType(asm, classPath), methodName);
        }

        private static TypeDef findType(AssemblyDef asm, string classPath)
        {
            foreach (var module in asm.Modules)
            {
                foreach (var type in module.Types)
                {
                    if (type.FullName == classPath)
                        return type;
                }
            }
            return null;
        }
    }
}
