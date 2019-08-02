filenames=$(find . -name "*.cs" | tr '\n' ' ')

mcs -out:application.exe -r:System.Windows.Forms,System.Drawing,System.Net.Http $filenames
