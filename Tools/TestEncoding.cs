using System;
using System.IO;
using System.Text;

class Program {
    static void Main() {
        string path = @"c:\Users\thean\Desktop\Đồ án cơ sở\PandoraWeb\Views\Home\Contact.cshtml";
        string mangled = File.ReadAllText(path, Encoding.UTF8);
        byte[] bytes = Encoding.GetEncoding(1252).GetBytes(mangled);
        string restored = Encoding.UTF8.GetString(bytes);
        Console.WriteLine(restored.Substring(0, Math.Min(1000, restored.Length)));
    }
}
