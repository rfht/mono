include ../mcs/build/config.make

#
# NETCORETESTS_VERSION and NETCOREAPP_VERSION must be updated in sync, we are using coreclr repo for that but that's fine for now
#

# Extracted MicrosoftPrivateCoreFxNETCoreAppVersion from https://github.com/dotnet/coreclr/blob/master/eng/Versions.props#L21
NETCORETESTS_VERSION := 4.6.0-preview7.19305.1

# Extracted MicrosoftNETCoreAppVersion from https://github.com/dotnet/coreclr/blob/master/eng/Versions.props#L25
NETCOREAPP_VERSION := 3.0.0-preview7-27804-03

# Extracted from https://dotnetcli.blob.core.windows.net/dotnet/aspnetcore/Runtime/master/latest.version
ASPNETCOREAPP_VERSION := 3.0.0-preview-18614-0151

ROSLYN_VERSION := 3.2.0-beta3-19279-03

ifeq ($(HOST_PLATFORM),win32)
NETCORESDK_EXT = zip
UNZIPCMD = python -c "import zipfile,sys; zipfile.ZipFile(sys.argv[1], 'r').extractall()"
XUNIT_FLAGS =
TESTS_PLATFORM = Windows_NT.x64
DOTNET := $(shell powershell -ExecutionPolicy Bypass -Command "./init-tools.ps1")/dotnet.exe
DOTNET := $(subst \,/,$(DOTNET))
endif

ifeq ($(HOST_PLATFORM),macos)
NETCORESDK_EXT = tar.gz
UNZIPCMD = tar -xvf
XUNIT_FLAGS = -notrait category=nonosxtests
TESTS_PLATFORM = OSX.x64
DOTNET := $(shell ./init-tools.sh | tail -1)
endif

ifeq ($(HOST_PLATFORM),linux)
NETCORESDK_EXT = tar.gz
UNZIPCMD = tar -xvf
XUNIT_FLAGS = -notrait category=nonlinuxtests
TESTS_PLATFORM = Linux.x64
DOTNET := $(shell ./init-tools.sh | tail -1)
endif

NETCORESDK_FILE := dotnet-runtime-$(NETCOREAPP_VERSION)-$(RID).$(NETCORESDK_EXT)
ASPNETCORESDK_FILE := aspnetcore-runtime-$(ASPNETCOREAPP_VERSION)-$(RID).$(NETCORESDK_EXT)
NETCORE_URL := https://dotnetcli.blob.core.windows.net/dotnet/Runtime/$(NETCOREAPP_VERSION)/$(NETCORESDK_FILE)
ASPNETCORE_URL := https://dotnetcli.blob.core.windows.net/dotnet/aspnetcore/Runtime/$(ASPNETCOREAPP_VERSION)/$(ASPNETCORESDK_FILE)
FEED_BASE_URL := https://dotnetfeed.blob.core.windows.net/dotnet-core
TEST_ASSETS_URL = $(FEED_BASE_URL)/corefx-tests/$(NETCORETESTS_VERSION)/$(TESTS_PLATFORM)/netcoreapp/corefx-test-assets.xml

# used to calculate exact version number for generating nupkg
BLAME = $(shell git blame ../configure.ac | grep AC_INIT | cut -f1 -d' ')
VERSTUB = .$(shell git rev-list --count $(BLAME)..HEAD)

all: runtime bcl

$(NETCORESDK_FILE):
	curl $(NETCORE_URL) --output $(NETCORESDK_FILE)
	rm -rf shared/Microsoft.NETCore.App
	$(UNZIPCMD) $(NETCORESDK_FILE)

# AspNetCoreApp contains its own .NET Core Runtime but we don't need it so let's remove it 
# and update version in Microsoft.AspNetCore.App.runtimeconfig.json to NETCOREAPP_VERSION
$(ASPNETCORESDK_FILE):
	curl $(ASPNETCORE_URL) --output $(ASPNETCORESDK_FILE)
	$(UNZIPCMD) $(ASPNETCORESDK_FILE)
	sed -e 's/.*version.*/\"version\": \"$(NETCOREAPP_VERSION)\"/' < shared/Microsoft.AspNetCore.App/$(ASPNETCOREAPP_VERSION)/Microsoft.AspNetCore.App.runtimeconfig.json > 2 && mv 2 shared/Microsoft.AspNetCore.App/$(ASPNETCOREAPP_VERSION)/Microsoft.AspNetCore.App.runtimeconfig.json

update-corefx: corefx/.stamp-dl-corefx-$(NETCORETESTS_VERSION)

corefx/.stamp-dl-corefx-$(NETCORETESTS_VERSION): corefx-restore.csproj
	$(DOTNET) build corefx-restore.csproj --runtime $(RID) --packages corefx/packages -p:MicrosoftPrivateCoreFxNETCoreAppVersion=$(NETCORETESTS_VERSION) -p:OutputPath=corefx/restore/
	touch $@

update-roslyn: roslyn-restore.csproj
	$(DOTNET) restore roslyn-restore.csproj -p:RoslynVersion=$(ROSLYN_VERSION) --packages roslyn/packages -p:OutputPath=roslyn/restore/

update-corefx-tests: corefx/.stamp-dl-corefx-tests-$(NETCORETESTS_VERSION)

corefx/.stamp-dl-corefx-tests-$(NETCORETESTS_VERSION):
	curl $(TEST_ASSETS_URL) --output corefx-test-assets.xml
	python dl-test-assets.py corefx-test-assets.xml $(FEED_BASE_URL) corefx/tests
	touch $@

run-sample: prepare
	$(DOTNET) build sample/HelloWorld
	MONO_ENV_OPTIONS="--debug" COMPlus_DebugWriteToStdErr=1 ./dotnet --fx-version "$(NETCOREAPP_VERSION)" sample/HelloWorld/bin/Debug/netcoreapp3.0/HelloWorld.dll

run-aspnet-sample: prepare
	rm -rf sample/AspNetCore/{bin,obj}
	$(DOTNET) publish sample/AspNetCore -c Debug -r $(RID)
	cp ../mono/mini/.libs/libmonosgen-2.0$(PLATFORM_AOT_SUFFIX) sample/AspNetCore/bin/Debug/netcoreapp3.0/$(RID)/publish/$(PLATFORM_AOT_PREFIX)coreclr$(PLATFORM_AOT_SUFFIX)
	cp ../mcs/class/System.Private.CoreLib/bin/$(COREARCH)/System.Private.CoreLib.dll sample/AspNetCore/bin/Debug/netcoreapp3.0/$(RID)/publish/
	COMPlus_DebugWriteToStdErr=1 ./dotnet --fx-version "$(ASPNETCOREAPP_VERSION)" sample/AspNetCore/bin/Debug/netcoreapp3.0/$(RID)/publish/AspNetCore.dll

# COREHOST_TRACE=1 
SHAREDRUNTIME := shared/Microsoft.NETCore.App/$(NETCOREAPP_VERSION)

bcl: update-roslyn
	$(MAKE) -C ../mcs/build/ common/Consts.cs
	$(DOTNET) build $(CORETARGETS) $(CORLIB_BUILD_FLAGS) -p:BuildArch=$(COREARCH) \
	-p:OutputPath=bin/$(COREARCH) \
	-p:RoslynPropsFile="../../../netcore/roslyn/packages/microsoft.net.compilers.toolset/$(ROSLYN_VERSION)/build/Microsoft.Net.Compilers.Toolset.props" \
	../mcs/class/System.Private.CoreLib/System.Private.CoreLib.csproj

debug-bcl:
	$(MAKE) bcl CORLIB_BUILD_FLAGS="-c Debug"

runtime:
	$(MAKE) -C ../mono

link-mono:
	cp ../mono/mini/.libs/libmonosgen-2.0$(PLATFORM_AOT_SUFFIX) $(SHAREDRUNTIME)/$(PLATFORM_AOT_PREFIX)coreclr$(PLATFORM_AOT_SUFFIX)
	cp ../mcs/class/System.Private.CoreLib/bin/$(COREARCH)/System.Private.CoreLib.{dll,pdb} $(SHAREDRUNTIME)

prepare: $(ASPNETCORESDK_FILE) $(NETCORESDK_FILE) update-corefx update-roslyn link-mono

nupkg:
	$(DOTNET) pack roslyn-restore.csproj -p:NuspecFile=runtime.nuspec -p:NuspecProperties=\"RID=$(RID)\;VERSION=$(VERSION)$(VERSTUB)\;PLATFORM_AOT_SUFFIX=$(PLATFORM_AOT_SUFFIX)\;COREARCH=$(COREARCH)\;PLATFORM_AOT_PREFIX=$(PLATFORM_AOT_PREFIX)\" --output ../artifacts/ --no-build
	$(DOTNET) pack roslyn-restore.csproj -p:NuspecFile=metapackage.nuspec -p:NuspecProperties=\"RID=$(RID)\;VERSION=$(VERSION)$(VERSTUB)\;PLATFORM_AOT_SUFFIX=$(PLATFORM_AOT_SUFFIX)\;COREARCH=$(COREARCH)\;PLATFORM_AOT_PREFIX=$(PLATFORM_AOT_PREFIX)\" --output ../artifacts/ --no-build

clean:
	rm -rf .configured ../.dotnet sdk shared host dotnet tests obj corefx roslyn LICENSE.txt ThirdPartyNotices.txt $(NETCORESDK_FILE) $(ASPNETCORESDK_FILE)

#
# Running tests:
#
# We have two modes to run then with xtest-Foo.Test to be the prefered one because it runs all tests and simple runner
# with rules like tes-Foo which runs limited set of test
#

# e.g. `make xtest-System.Collections.Tests`
xtest-%: prepare update-corefx-tests
	echo -n "***************** $* *********************"
	cp corefx/restore/corefx-restore.deps.json corefx/tests/extracted/$*/xunit.console.deps.json
	cp corefx/restore/corefx-restore.deps.json corefx/tests/extracted/$*/Microsoft.DotNet.RemoteExecutorHost.deps.json
	cp corefx/restore/corefx-restore.runtimeconfig.dev.json corefx/tests/extracted/$*/xunit.console.runtimeconfig.dev.json
	cp corefx/restore/corefx-restore.runtimeconfig.dev.json corefx/tests/extracted/$*/Microsoft.DotNet.RemoteExecutorHost.runtimeconfig.dev.json
	cp corefx/restore/corefx-restore.dll corefx/tests/extracted/$*/corefx-restore.dll
	sed -i -e 's/9.9.9/$(NETCOREAPP_VERSION)/g' corefx/tests/extracted/$*/*.runtimeconfig.json
	cd corefx/tests/extracted/$* && \
	COMPlus_DebugWriteToStdErr=1 $(CURDIR)/./dotnet --fx-version "$(NETCOREAPP_VERSION)" xunit.console.dll $*.dll \
		-html ../../TestResult-$*.html -nunit ../../TestResult-$*-netcore-xunit.xml \
		$(XUNIT_FLAGS) @../../../../CoreFX.issues.rsp \
		$(FIXTURE) || true

debug-xtest-%:
	MONO_ENV_OPTIONS="--debug" $(MAKE) xtest-$*

# these tests won't be included in `xtestall`
EXCLUDED_COREFX_TESTS =

xtestall-noload: $(foreach workingtest, $(foreach test, $(wildcard corefx/tests/extracted/*), \
		$(filter-out $(EXCLUDED_COREFX_TESTS), $(notdir $(test)))), $(addprefix xtest-, $(workingtest)))

xtestall: update-corefx-tests
	$(MAKE) xtestall-noload

# build a test assembly in COREFX_ROOT and copy it to corefx/tests/extracted
# e.g. `make build-local-corefx-test-System.Runtime COREFX_ROOT=/prj/corefx-master`
build-local-corefx-test-%: check-env
	cd $(COREFX_ROOT)/src/$*/tests && $(DOTNET) build -o "$(CURDIR)/corefx/tests/extracted/tmp"
	cp $(CURDIR)/corefx/tests/extracted/tmp/$*.Tests.{dll,pdb} $(CURDIR)/corefx/tests/extracted/$*.Tests/
	rm -rf $(CURDIR)/corefx/tests/extracted/tmp

#
# console test runner (obsolete)
#
TEST_SUITES = \
	System.Runtime.Tests \
	System.Runtime.CompilerServices.Unsafe.Tests \
	System.Collections.Tests

# The binary directory under $(COREFIX_BINDIR)
System.Runtime.CompilerServices.Unsafe.Tests_PROFILE = netstandard-Debug
System.Runtime.Tests_PROFILE = netcoreapp-Unix-Debug
System.Collections.Tests_PROFILE = netcoreapp-Debug
System.Reflection.Emit.Tests_PROFILE = netcoreapp-Debug
System.Reflection.Emit.ILGeneration.Tests_PROFILE = netcoreapp-Debug
System.Reflection.Emit.Lightweight.Tests_PROFILE = netcoreapp-Debug
System.Threading.Tests_PROFILE = netcoreapp-Debug
System.Threading.Thread.Tests_PROFILE = netcoreapp-Debug

COREFX_BINDIR=$(COREFX_ROOT)/artifacts/bin

check-env:
	@if [ "x$(COREFX_ROOT)" == "x" ]; then echo "Set COREFX_ROOT to the root of the fully built corefx repo."; exit 1; fi

build-%: check-env
	cd gen-xunit-runner && $(DOTNET) run ../tests $(COREFX_ROOT) $(COREFX_BINDIR)/$*/$($*_PROFILE)/$*.dll $(XUNIT_FLAGS) @../CoreFX.issues.rsp $(XUNIT_ARGS)
	cp $(COREFX_BINDIR)/RemoteExecutorConsoleApp/netstandard-Debug/RemoteExecutorConsoleApp.* tests/$*
	cp $(COREFX_BINDIR)/System.Runtime.Tests/netcoreapp-Unix-Debug/TestLoadAssembly.dll tests/$*
	sed -e 's/9.9.9/$(VERSION)/g' < tests/$*/RemoteExecutorConsoleApp.runtimeconfig.json > 2 && mv 2 tests/$*/RemoteExecutorConsoleApp.runtimeconfig.json
	cd tests/$* && $(DOTNET) build

run-%: check-env
	cd tests/$* && MONO_PATH=bin/Debug/netcoreapp3.0 MONO_ENV_OPTIONS="--debug --explicit-null-checks" COMPlus_DebugWriteToStdErr=1 ../../dotnet bin/Debug/netcoreapp3.0/$*-runner.dll

build-base: $(addprefix build-, $(TEST_SUITES))

check: $(addprefix run-, $(TEST_SUITES))

distdir:
