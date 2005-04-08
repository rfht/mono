#include <mono/jit/jit.h>
#include <mono/metadata/environment.h>
#include <stdlib.h>

/*
 * Very simple mono embedding example.
 * Compile with: 
 * 	gcc -o teste teste.c `pkg-config --cflags --libs mono` -lm
 * 	mcs test.cs
 * Run with:
 * 	./teste test.exe
 */

static MonoString*
gimme () {
	return mono_string_new (mono_domain_get (), "All your monos are belong to us!");
}

static void main_function (MonoDomain *domain, const char *file, int argc, char** argv)
{
	MonoAssembly *assembly;

	assembly = mono_domain_assembly_open (domain, file);
	if (!assembly)
		exit (2);
	/*
	 * mono_jit_exec() will run the Main() method in the assembly.
	 * The return value needs to be looked up from
	 * System.Environment.ExitCode.
	 */
	mono_jit_exec (domain, assembly, argc, argv);
}


int 
main(int argc, char* argv[]) {
	MonoDomain *domain;
	const char *file;
	int retval;
	
	if (argc < 2){
		fprintf (stderr, "Please provide an assembly to load");
		return 1;
	}
	file = argv [1];
	/*
	 * mono_jit_init() creates a domain: each assembly is
	 * loaded and run in a MonoDomain.
	 */
	domain = mono_jit_init (file);
	/*
	 * We add our special internal call, so that C# code
	 * can call us back.
	 */
	mono_add_internal_call ("MonoEmbed::gimme", gimme);

	main_function (domain, file, argc - 1, argv + 1);
	
	retval = mono_environment_exitcode_get ();
	
	mono_jit_cleanup (domain);
	return retval;
}

