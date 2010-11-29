using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: AssemblyProduct("プログラミング言語「クロガネ」")]
[assembly: AssemblyTitle("クロガネの基幹機能群")]
[assembly: AssemblyDescription(
	"クロガネを実行するためのアセンブリです。" +
	"構文解析器やエンジン、標準ライブラリなどを一式が含まれています。")]
[assembly: AssemblyCulture("")]
[assembly: AssemblyCopyright("ground")]

// 使わない
//[assembly: AssemblyConfiguration("")]
//[assembly: AssemblyCompany("Microsoft")]

//[assembly: AssemblyTrademark("")]


// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("5f46a843-f32b-480e-aa8d-b10ab0d78a33")]

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
[assembly: AssemblyVersion("1.0.0.*")]
//[assembly: AssemblyFileVersion("1.0.0.0")]


[assembly: InternalsVisibleTo("Kurogane.Test")]
