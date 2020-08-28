main.exe: main.cs
	csc -out:VPNConnect.exe main.cs -nologo
	
clean:
	rm -rf *.exe
	rm -rf *.zip

run:
	./VPNConnect.exe

package:
	zip VPNConnect.zip VPNConnect.exe Resources/*

all:
	csc -out:VPNConnect.exe main.cs -nologo
	zip VPNConnect.zip VPNConnect.exe Resources/*