using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;
using System.Text.RegularExpressions;

// Offline, metadata-only discovery tool. It never executes or loads game code and
// requires no game DLLs to be checked into this repository.
if (args.Length != 1 || !File.Exists(args[0]))
{
    Console.Error.WriteLine("Usage: dotnet run --project tools/AffinityInspector -- <full-path-to-Assembly-CSharp.dll>");
    return 2;
}

var input = Path.GetFullPath(args[0]);
// Intentionally omit generic 'unit'/'friend': those would dump nearly the entire game API.
var terms = new Regex("intim|favor|affin|relation|spouse|partner|marriage|lover|couple|goodwill|好感|关系|道侣|夫妻|结缘", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);
using var stream = File.OpenRead(input);
using var pe = new PEReader(stream);
if (!pe.HasMetadata)
{
    Console.Error.WriteLine("This file contains no .NET metadata. Locate the generated/managed Assembly-CSharp.dll.");
    return 3;
}
var metadata = pe.GetMetadataReader();
Console.WriteLine("# Affinity inspector: metadata inventory (NOT proof of runtime call paths)");
Console.WriteLine($"# Input: {Path.GetFileName(input)} ({stream.Length} bytes)");
Console.WriteLine($"# SHA-256: {Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(input)))}");
Console.WriteLine("# Method signatures and field signatures are printed as ECMA-335 blobs (hex); do not infer argument types from names alone.");
Console.WriteLine("# No game binaries or executable code are included in this report.\n");

int typeCount = 0, methodCount = 0;
foreach (var typeHandle in metadata.TypeDefinitions)
{
    var type = metadata.GetTypeDefinition(typeHandle);
    var typeName = metadata.GetString(type.Name);
    var ns = metadata.GetString(type.Namespace);
    var fullName = string.IsNullOrEmpty(ns) ? typeName : $"{ns}.{typeName}";
    var matchingMethods = type.GetMethods().Where(handle => terms.IsMatch(metadata.GetString(metadata.GetMethodDefinition(handle).Name))).ToArray();
    var matchingFields = type.GetFields().Where(handle => terms.IsMatch(metadata.GetString(metadata.GetFieldDefinition(handle).Name))).ToArray();
    if (!terms.IsMatch(fullName) && matchingMethods.Length == 0 && matchingFields.Length == 0)
        continue;

    typeCount++;
    Console.WriteLine($"TYPE {fullName} token=0x{MetadataTokens.GetToken(typeHandle):X8} flags={type.Attributes}");
    // Print ALL methods/fields on relevant types, including short generic names like Add, Get and Set.
    foreach (var fieldHandle in type.GetFields())
    {
        var field = metadata.GetFieldDefinition(fieldHandle);
        var name = metadata.GetString(field.Name);
        var sig = Convert.ToHexString(metadata.GetBlobBytes(field.Signature));
        var constant = field.GetDefaultValue();
        var defaultValue = constant.IsNil ? "" : $" constantHex={Convert.ToHexString(metadata.GetBlobBytes(metadata.GetConstant(constant).Value))}";
        Console.WriteLine($"  FIELD {name} token=0x{MetadataTokens.GetToken(fieldHandle):X8} flags={field.Attributes} signatureHex={sig}{defaultValue}");
    }
    foreach (var methodHandle in type.GetMethods())
    {
        var method = metadata.GetMethodDefinition(methodHandle);
        var name = metadata.GetString(method.Name);
        var sig = Convert.ToHexString(metadata.GetBlobBytes(method.Signature));
        var parameterNames = method.GetParameters()
            .Select(h => metadata.GetParameter(h))
            .Where(p => p.SequenceNumber > 0)
            .OrderBy(p => p.SequenceNumber)
            .Select(p => $"{p.SequenceNumber}:{metadata.GetString(p.Name)}");
        Console.WriteLine($"  METHOD {name} token=0x{MetadataTokens.GetToken(methodHandle):X8} flags={method.Attributes} signatureHex={sig} params=[{string.Join(", ", parameterNames)}]");
        methodCount++;
    }
    Console.WriteLine();
}
Console.WriteLine($"# Matching types: {typeCount}; listed methods: {methodCount}");
Console.WriteLine("# Next: inspect exact owner types, overload signatures, relation enum values and CALLERS in ILSpy/dnSpy; metadata alone cannot verify logic.");
return 0;
