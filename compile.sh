filenames=$(find . -name "*.cs" | tr '\n' ' ')

echo $filenames

mcs -out:application.exe -r:System.Windows.Forms,System.Drawing $filenames
