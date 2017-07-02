using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("NullDestination")]
[assembly: AssemblyDescription("SSIS Component that just throws all the records away")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("None")]
[assembly: AssemblyProduct("NullDestination")]
[assembly: AssemblyCopyright("Copyright © Keith Martin 2017")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("8b4249cf-ed23-48c8-87d3-4457e1b335db")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
#if SQL2012
[assembly: AssemblyVersion("11.0.0.0")]
[assembly: AssemblyFileVersion("11.0.0.0")]
#endif
#if SQL2014
[assembly: AssemblyVersion("12.0.0.0")]
[assembly: AssemblyFileVersion("12.0.0.0")]
#endif
#if SQL2015
[assembly: AssemblyVersion("13.0.0.0")]
[assembly: AssemblyFileVersion("13.0.0.0")]
#endif