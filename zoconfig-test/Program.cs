/**
 * Thanks for checking out zoconfig!
 * 
 * This is an example of how to pull in data from a .zc file.
 */

using zoconfig;

string templatePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, @"data\template.zc");

Zocparser parser = new();
parser.GetData(templatePath);

var orc = parser["orc"];
var elf = parser["elf"];
var gnome = parser["gnome"];

Console.Write(
    $"{orc?["name"]} (orc) {Environment.NewLine}" +
    $"strength {orc?["strength"].As<int>()} {Environment.NewLine}" +
    $"speed {orc?["speed"].As<float>()} {Environment.NewLine}" +
    $"intelligence {orc?["intelligence"].As<short>()} {Environment.NewLine}");

Console.WriteLine();

Console.Write(
    $"{elf?["name"]} (elf) {Environment.NewLine}" +
    $"strength {elf?["strength"].As<int>()} {Environment.NewLine}" +
    $"speed {elf?["speed"].As<float>()} {Environment.NewLine}" +
    $"intelligence {elf?["intelligence"].As<short>()} {Environment.NewLine}");

Console.WriteLine();

Console.Write(
    $"{gnome?["name"]} (gnome) {Environment.NewLine}" +
    $"strength {gnome?["strength"].As<int>()} {Environment.NewLine}" +
    $"speed {gnome?["speed"].As<float>()} {Environment.NewLine}" +
    $"intelligence {gnome?["intelligence"].As<short>()} {Environment.NewLine}");