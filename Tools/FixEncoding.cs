using System;
using System.IO;
using System.Text;

class Program {
    static void Main() {
        string[] paths = { 
            @"c:\Users\thean\Desktop\Đồ án cơ sở\PandoraWeb\Views\Shared\_Layout.cshtml",
            @"c:\Users\thean\Desktop\Đồ án cơ sở\PandoraWeb\assets\js\main.js"
        };
        foreach(var path in paths) {
            // Read the corrupted UTF-8 string
            string corrupted = File.ReadAllText(path, Encoding.UTF8);
            
            // Convert the corrupted string back to the bytes it was interpreted from (Windows-1252)
            // Windows-1252 is code page 1252
            Encoding win1252 = Encoding.GetEncoding(1252);
            byte[] originalBytes = win1252.GetBytes(corrupted);
            
            // Decode the original bytes as UTF-8 to get the correct string
            string fixedText = Encoding.UTF8.GetString(originalBytes);
            
            // Save the fixed string back to the file as UTF-8
            File.WriteAllText(path, fixedText, Encoding.UTF8);
            Console.WriteLine("Fixed " + path);
        }
    }
}
