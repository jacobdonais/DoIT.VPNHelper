main.exe: main.cs
	csc -out:VPNConnect.exe main.cs -nologo
	
clean:
	rm -rf *.exe

run:
	./VPNConnect.exe
