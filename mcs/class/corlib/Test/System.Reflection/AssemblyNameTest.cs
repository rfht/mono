//
// AssemblyNameTest.cs - NUnit Test Cases for AssemblyName
//
// Author:
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002 Motus Technologies Inc. (http://www.motus.com)
//

using NUnit.Framework;
using System;
using System.Configuration.Assemblies;
using System.IO;
using System.Reflection;

namespace MonoTests.System.Reflection {

public class AssemblyNameTest : TestCase {

	public AssemblyNameTest () : base ("System.Reflection.AssemblyName testsuite") {}
	public AssemblyNameTest (string name) : base (name) {}

	private AssemblyName an;

	// created with "sn -o test.snk test.txt"
	static byte[] test = { 0x07, 0x02, 0x00, 0x00, 0x00, 0x24, 0x00, 0x00, 0x52, 0x53, 0x41, 0x32, 0x00, 0x04, 0x00, 0x00, 0x01, 0x00, 0x01, 0x00, 0x3D, 0xBD, 0x72, 0x08, 0xC6, 0x2B, 0x0E, 0xA8, 0xC1, 0xC0, 0x58, 0x07, 0x2B, 0x63, 0x5F, 0x7C, 0x9A, 0xBD, 0xCB, 0x22, 0xDB, 0x20, 0xB2, 0xA9, 0xDA, 0xDA, 0xEF, 0xE8, 0x00, 0x64, 0x2F, 0x5D, 0x8D, 0xEB, 0x78, 0x02, 0xF7, 0xA5, 0x36, 0x77, 0x28, 0xD7, 0x55, 0x8D, 0x14, 0x68, 0xDB, 0xEB, 0x24, 0x09, 0xD0, 0x2B, 0x13, 0x1B, 0x92, 0x6E, 0x2E, 0x59, 0x54, 0x4A, 0xAC, 0x18, 0xCF, 0xC9, 0x09, 0x02, 0x3F, 0x4F, 0xA8, 0x3E, 0x94, 0x00, 0x1F, 0xC2, 0xF1, 0x1A, 0x27, 0x47, 0x7D, 0x10, 0x84, 0xF5, 0x14, 0xB8, 0x61, 0x62, 0x1A, 0x0C, 0x66, 0xAB, 0xD2, 0x4C, 0x4B, 0x9F, 0xC9, 0x0F, 0x3C, 0xD8, 0x92, 0x0F, 0xF5, 0xFF, 0xCE, 0xD7, 0x6E, 0x5C, 0x6F, 0xB1, 0xF5, 0x7D, 0xD3, 0x56, 0xF9, 0x67, 0x27, 0xA4, 0xA5, 0x48, 0x5B, 0x07, 0x93, 0x44, 0x00, 0x4A, 0xF8, 0xFF, 0xA4, 0xCB, 0x73, 0xC0, 0x6A, 0x62, 0xB4, 0xB7, 0xC8, 0x92, 0x58, 0x87, 0xCD, 0x07,
		0x0C, 0x7D, 0x6C, 0xC1, 0x4A, 0xFC, 0x82, 0x57, 0x0E, 0x43, 0x85, 0x09, 0x75, 0x98, 0x51, 0xBB, 0x35, 0xF5, 0x64, 0x83, 0xC7, 0x79, 0x89, 0x5C, 0x55, 0x36, 0x66, 0xAB, 0x27, 0xA4, 0xD9, 0xD4, 0x7E, 0x6B, 0x67, 0x64, 0xC1, 0x54, 0x4E, 0x37, 0xF1, 0x4E, 0xCA, 0xB3, 0xE5, 0x63, 0x91, 0x57, 0x12, 0x14, 0xA6, 0xEA, 0x8F, 0x8F, 0x2B, 0xFE, 0xF3, 0xE9, 0x16, 0x08, 0x2B, 0x86, 0xBC, 0x26, 0x0D, 0xD0, 0xC6, 0xC4, 0x1A, 0x72, 0x43, 0x76, 0xDC, 0xFF, 0x28, 0x52, 0xA1, 0xDE, 0x8D, 0xFA, 0xD5, 0x1F, 0x0B, 0xB5, 0x4F, 0xAF, 0x06, 0x79, 0x11, 0xEE, 0xA8, 0xEC, 0xD3, 0x74, 0x55, 0xA2, 0x80, 0xFC, 0xF8, 0xD9, 0x50, 0x69, 0x48, 0x01, 0xC2, 0x5A, 0x04, 0x56, 0xB4, 0x3E, 0x24, 0x32, 0x20, 0xB5, 0x2C, 0xDE, 0xBB, 0xBD, 0x13, 0xFD, 0x13, 0xF7, 0x03, 0x3E, 0xE3, 0x37, 0x84, 0x74, 0xE7, 0xD0, 0x5E, 0x9E, 0xB6, 0x26, 0xAE, 0x6E, 0xB0, 0x55, 0x6A, 0x52, 0x63, 0x6F, 0x5A, 0x9D, 0xF2, 0x67, 0xD6, 0x61, 0x4F, 0x7A, 0x45, 0xEE, 0x5C, 0x3D, 0x2B, 0x7C, 0xB2, 0x40, 0x79, 0x54, 0x84, 0xD1, 
		0xBE, 0x61, 0x3E, 0x5E, 0xD6, 0x18, 0x8E, 0x14, 0x98, 0xFC, 0x35, 0xBF, 0x5F, 0x1A, 0x20, 0x2E, 0x1A, 0xD8, 0xFF, 0xC4, 0x6B, 0xC0, 0xC9, 0x7D, 0x06, 0xEF, 0x09, 0xF9, 0xF3, 0x69, 0xFC, 0xBC, 0xA2, 0xE6, 0x80, 0x22, 0xB9, 0x79, 0x7E, 0xEF, 0x57, 0x9F, 0x49, 0xE1, 0xBC, 0x0D, 0xB6, 0xA1, 0xFE, 0x8D, 0xBC, 0xBB, 0xA3, 0x05, 0x02, 0x6B, 0x04, 0x45, 0xF7, 0x5D, 0xEE, 0x43, 0x06, 0xD6, 0x9C, 0x94, 0x48, 0x1A, 0x0B, 0x9C, 0xBC, 0xB4, 0x4E, 0x93, 0x60, 0x87, 0xCD, 0x58, 0xD6, 0x9A, 0x39, 0xA6, 0xC0, 0x7F, 0x8E, 0xFF, 0x25, 0xC1, 0xD7, 0x2C, 0xF6, 0xF4, 0x6F, 0x24, 0x52, 0x0B, 0x39, 0x42, 0x1B, 0x0D, 0x04, 0xC1, 0x93, 0x2A, 0x19, 0x1C, 0xF0, 0xB1, 0x9B, 0xC1, 0x24, 0x6D, 0x1B, 0x0B, 0xDA, 0x1C, 0x8B, 0x72, 0x48, 0xF0, 0x3E, 0x52, 0xBF, 0x0A, 0x84, 0x3A, 0x9B, 0xC8, 0x6D, 0x13, 0x1E, 0x72, 0xF4, 0x46, 0x93, 0x88, 0x1A, 0x5F, 0x4C, 0x3C, 0xE5, 0x9D, 0x6E, 0xBB, 0x4E, 0xDD, 0x5D, 0x1F, 0x11, 0x40, 0xF4, 0xD7, 0xAF, 0xB3, 0xAB, 0x9A, 0x99, 0x15, 0xF0, 0xDC, 0xAA, 0xFF,
		0x9F, 0x2D, 0x9E, 0x56, 0x4F, 0x35, 0x5B, 0xBA, 0x06, 0x99, 0xEA, 0xC6, 0xB4, 0x48, 0x51, 0x17, 0x1E, 0xD1, 0x95, 0x84, 0x81, 0x18, 0xC0, 0xF1, 0x71, 0xDE, 0x44, 0x42, 0x02, 0x06, 0xAC, 0x0E, 0xA8, 0xE2, 0xF3, 0x1F, 0x96, 0x1F, 0xBE, 0xB6, 0x1F, 0xB5, 0x3E, 0xF6, 0x81, 0x05, 0x20, 0xFA, 0x2E, 0x40, 0x2E, 0x4D, 0xA0, 0x0E, 0xDA, 0x42, 0x9C, 0x05, 0xAA, 0x9E, 0xAF, 0x5C, 0xF7, 0x3A, 0x3F, 0xBB, 0x91, 0x73, 0x45, 0x27, 0xA8, 0xA2, 0x07, 0x4A, 0xEF, 0x59, 0x1E, 0x97, 0x9D, 0xE0, 0x30, 0x5A, 0x83, 0xCE, 0x1E, 0x57, 0x32, 0x89, 0x43, 0x41, 0x28, 0x7D, 0x14, 0x8D, 0x8B, 0x41, 0x1A, 0x56, 0x76, 0x43, 0xDB, 0x64, 0x86, 0x41, 0x64, 0x8D, 0x4C, 0x91, 0x83, 0x4E, 0xF5, 0x6C };

	static byte[] token = { 0xFF, 0xEF, 0x94, 0x53, 0x67, 0x69, 0xDA, 0x06 };

	protected override void SetUp () {}

	protected override void TearDown () {}

	public static ITest Suite {
		get { 
			return new TestSuite (typeof (AssemblyNameTest)); 
		}
	}

	public void TestEmptyAssembly () 
	{
		an = new AssemblyName ();
		AssertNull ("CodeBase", an.CodeBase);
		AssertNull ("CultureInfo", an.CultureInfo);
		AssertNull ("EscapedCodeBase", an.EscapedCodeBase);
		AssertEquals ("Flags", AssemblyNameFlags.None, an.Flags);
		AssertNull ("FullName", an.FullName);
		AssertEquals ("HashAlgorithm", AssemblyHashAlgorithm.None, an.HashAlgorithm);
		AssertNull ("KeyPair", an.KeyPair);
		AssertNull ("Name", an.Name);
		AssertNull ("Version", an.Version);
		AssertEquals ("VersionCompatibility", AssemblyVersionCompatibility.SameMachine, an.VersionCompatibility);
	}

	public void TestPublicKey () 
	{
		an = new AssemblyName ();
		an.SetPublicKey (test);

		AssertEquals ("Flags", AssemblyNameFlags.PublicKey, an.Flags);
		// strangly it doesn't affect the KeyPair ?
		AssertNull ("KeyPair", an.KeyPair);
		AllTests.AssertEquals ("PublicKey", test, an.GetPublicKey ());
		AllTests.AssertEquals ("PublicKeyToken", token, an.GetPublicKeyToken ());
	}

	public void TestPublicKeyToken () 
	{
		an = new AssemblyName ();
		an.SetPublicKeyToken (token);

		AssertEquals ("Flags", AssemblyNameFlags.None, an.Flags);
		AssertNull ("KeyPair", an.KeyPair);
		AssertNull ("PublicKey", an.GetPublicKey ());
		AllTests.AssertEquals ("PublicKeyToken", token, an.GetPublicKeyToken ());
	}

	public void TestKeyPair () 
	{
		an = new AssemblyName ();
		an.KeyPair = new StrongNameKeyPair (test);

		AssertEquals ("Flags", AssemblyNameFlags.None, an.Flags);
		AssertNotNull ("KeyPair", an.KeyPair);
		AssertNull ("PublicKey", an.GetPublicKey ());
		AssertNull ("PublicKeyToken", an.GetPublicKeyToken ());
	}

	private AssemblyName GetAssemblyName (string assemblyFile) 
	{
		Assembly a = Assembly.LoadFrom (assemblyFile);
		return a.GetName ();
	}

	public void TestStatic () 
	{
		/*an = GetAssemblyName ("System.Reflection.dll");*/
	}
}

}